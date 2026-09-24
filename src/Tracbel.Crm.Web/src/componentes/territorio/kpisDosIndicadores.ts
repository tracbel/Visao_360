/**
 * As duas linhas de indicadores do topo (issue 170, parte A).
 *
 * A primeira é a carteira da ADR; a segunda é a seção "O mercado da região" —
 * o que o território tem, e que fatia de São Paulo isso representa. Sem o
 * denominador do estado, "62 mil tratores" é um número solto; com ele, é a
 * posição da Tracbel Agro no mercado paulista.
 */

import type { Indicador } from '../cadastro/Indicadores';
import { fatiasEmTexto, montarSomavel } from '../comum/comparacoes';
import type {
  IndicadoresTerritoriais,
  MedidaSomavel,
  PotencialDoRecorteNoMapa,
  ProcedenciasDoTerritorio,
  TotaisDaRegiaoTracbel,
} from '../../tipos/territorio';
import { reaisCompactos, reaisDaProducao } from './escalas';
import { nº, porcento } from './indicadoresDaAdr';
import { diferencaParaASoma, type FatiaNoEstado, type TotaisDaAdr } from './totaisDaAdr';
import { periodoDaLeitura, type PeriodoDaLeitura } from './carteira/periodo';

/** O que as duas linhas precisam saber sobre o estado da leitura. */
export type ContextoDosKpis = {
  indicadores: IndicadoresTerritoriais | null;
  /** Respondeu e o território está carregado. */
  comTerritorio: boolean;
  territorioNaoCarregado: boolean;
  municipiosDaAdr: number;
  totais: TotaisDaAdr;
  fatiaNoEstado: FatiaNoEstado | null;
  coberturaDaAdr: number | null;
  recorte: PotencialDoRecorteNoMapa | null;
  vendasForaDoMapa: number;
  anoDoCenso: number | null;
  anoDoRebanho: number | null;
  /** Os totais da ADR inteira — o denominador da fatia da Região Tracbel (issue 163). */
  regiaoTracbel: TotaisDaRegiaoTracbel | null;
  /** De onde veio cada número — a tela não escreve fonte à mão (issue 167). */
  procedencias: ProcedenciasDoTerritorio | null;
};

const SEM_TERRITORIO = 'território não carregado neste banco';

/**
 * UM CARTÃO DA CARTEIRA (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * Era um `Indicador` do `PainelDeIndicadores` do cadastro, que outras seis
 * telas usam; o cartão da maquete é outro objeto (ladrilho de ícone, pílula de
 * variação, mini-gráfico, barra de progresso), e por isso ganhou tipo próprio
 * em vez de mudar o componente dos outros.
 *
 * O `deOnde` NÃO SAIU: a linha longa que ficava embaixo do número ("330 de 540
 * vínculos elegíveis no prazo · 210 pendentes · regra provisória") foi para a
 * dica do cartão — a maquete põe embaixo do número uma frase curta, que é o
 * `contexto`.
 */
export type CartaoDaCarteira = {
  id: 'municipios' | 'cobertura' | 'vendas' | 'parque';
  rotulo: string;
  /** O valor já escrito. `null` quando não há dado — vira travessão, nunca zero. */
  valor: string | null;
  /** A frase curta embaixo do número, como a maquete a escreve. */
  contexto: string;
  /** O que ficava embaixo do número antes — agora na dica ao lado do rótulo. */
  deOnde: string;
  /** Quando o valor é nulo, o que dizer no lugar do contexto. */
  semDado: string;
  /** A barra da cobertura, de 0 a 100 — só o cartão que a maquete desenha com ela. */
  progresso?: number | null;
};

export type CarteiraNaArea = {
  cartoes: CartaoDaCarteira[];
  /** A janela que o servidor aplicou — o "12 meses" do cabeçalho, da tabela e da ficha. */
  periodo: PeriodoDaLeitura | null;
};

export function kpisDaCarteira(c: ContextoDosKpis): CarteiraNaArea {
  const daAdr = c.indicadores?.municipios.filter((m) => m.pertenceAAdr) ?? [];

  // "COM VISITA" É O MUNICÍPIO COM AO MENOS UM VÍNCULO NO PRAZO DA CADÊNCIA —
  // `cobertos`, e não `vinculosComCadencia`: elegível sem contato no prazo é
  // justamente o que a cobertura NÃO cobre.
  const comVisita = daAdr.filter((m) => m.cobertura.cobertos > 0).length;
  const comVenda = daAdr.filter((m) => m.vendas.valorLiquido > 0).length;

  return {
    periodo: c.indicadores ? periodoDaLeitura(c.indicadores.competenciaInicial, c.indicadores.competenciaFinal) : null,
    cartoes: [
      {
        id: 'municipios',
        rotulo: 'Municípios da ADR',
        valor: c.comTerritorio ? nº(c.municipiosDaAdr) : null,
        contexto: `${nº(c.totais.clientes)} clientes com endereço neles`,
        deOnde:
          `Os municípios que a área de atuação marca como ADR, no recorte dos filtros. ${nº(c.totais.clientes)} ` +
          'clientes com endereço neles: cada cliente conta no município do endereço principal do cadastro.',
        semDado: c.indicadores ? SEM_TERRITORIO : 'área de atuação não carregada',
      },
      {
        id: 'cobertura',
        rotulo: 'Cobertura pela cadência',
        // SEM `tom: 'atencao'`: o número ficava vermelho, e a maquete não o
        // pinta. O que o vermelho queria dizer — que a regra é provisória — está
        // escrito na dica, que é onde uma ressalva se lê.
        valor: c.comTerritorio && c.coberturaDaAdr !== null ? porcento(c.coberturaDaAdr) : null,
        progresso: c.comTerritorio ? c.coberturaDaAdr : null,
        contexto: `${nº(comVisita)} municípios com visita`,
        deOnde:
          `${nº(c.totais.cobertos)} de ${nº(c.totais.elegiveis)} vínculos elegíveis no prazo · ${nº(c.totais.pendentes)} ` +
          'pendentes · regra provisória. "Com visita" é o município com ao menos um vínculo no prazo: contato é ' +
          'qualquer interação registrada, porque nenhum tipo de atividade está marcado como visita (documento 32, P-2).',
        semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'sem vínculo elegível',
      },
      {
        id: 'vendas',
        rotulo: 'Vendas no período',
        valor: c.comTerritorio ? reaisCompactos(c.totais.vendas) : null,
        contexto: `em ${nº(comVenda)} municípios`,
        deOnde: `máquina ${reaisCompactos(c.totais.maquina)} · pós-venda ${reaisCompactos(c.totais.posVenda)} (composição provisória). Vendas líquidas pelo endereço principal do cliente; "em N municípios" conta os da ADR com venda no período.`,
        semDado: c.territorioNaoCarregado ? `${reaisCompactos(c.vendasForaDoMapa)} no período, todos fora do mapa` : '—',
      },
      {
        id: 'parque',
        rotulo: 'Parque teórico de máquinas',
        valor: c.comTerritorio && c.totais.municipiosComArea > 0 ? nº(Math.round(c.totais.maquinasTeoricas)) : null,
        // O SELO DE ESTIMATIVA É DO DADO: a palavra só aparece quando alguma
        // regra que dimensionou máquina aqui ainda não foi confirmada.
        contexto: c.totais.potencialEstimado ? 'estimativa na área de atuação' : 'na área de atuação',
        deOnde: `${nº(Math.round(c.totais.hectares))} ha úteis em ${nº(c.totais.municipiosComArea)} municípios${
          c.recorte?.demandaAnualDeMaquinas != null ? ` · ${nº(Math.round(c.recorte.demandaAnualDeMaquinas))} por ano` : ''
        }${c.totais.potencialEstimado ? ' · estimativa, regra a confirmar' : ''}`,
        semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'sem regra de potencial',
      },
    ],
  };
}

/**
 * A LINHA "O MERCADO DA REGIÃO" — cada número com as DUAS comparações (issue 163).
 *
 * <b>A fonte saiu do texto e foi para a dica</b> (issue 167): o `deOnde` dizia
 * "Censo Agropecuário 2017 · ... do total publicado de São Paulo", escrito à mão
 * aqui. Agora o ano, a tabela e a ressalva vêm do contrato, e o que sobra embaixo
 * do número é o que a diretoria lê: quanto isto é da Região Tracbel e de SP.
 *
 * <b>Região Tracbel é a ADR inteira</b>, e o denominador dela não muda quando o
 * filtro de sub-região muda — senão cada município do Norte viraria uma fatia
 * maior de si mesmo.
 */
export function kpisDoMercado(c: ContextoDosKpis): Indicador[] {
  const regiao = c.regiaoTracbel;
  const p = c.procedencias;

  /** O contexto embaixo do número: as duas fatias, mais o que a soma esconde. */
  const contexto = (medida: MedidaSomavel, complemento?: string) =>
    [fatiasEmTexto(medida), complemento].filter(Boolean).join(' · ') || '—';

  // SIGILO EM TODOS OS MUNICÍPIOS NÃO É ZERO. A soma ignora o município sob
  // sigilo (ele entra como nada, e não como zero); mas quando NENHUM município
  // do recorte foi divulgado, a soma vazia saía "0 tratores" — que afirma que a
  // região não tem trator. Aí o número é ausente, com o motivo.
  const daAdr = c.indicadores?.municipios.filter((m) => m.pertenceAAdr) ?? [];
  const algumComPropriedades = daAdr.some((m) => m.estrutura.estabelecimentos !== null);
  const sobSigilo = 'sigilo do IBGE em todos os municípios do recorte — o número existe e foi ocultado, e não é zero';

  const tratores = montarSomavel(
    c.totais.municipiosComTratores > 0 ? c.totais.tratores : null,
    regiao?.tratores ?? null,
    c.indicadores?.estado?.tratores.publicado ?? null,
  );
  const propriedades = montarSomavel(
    algumComPropriedades ? c.totais.estabelecimentos : null,
    regiao?.estabelecimentos ?? null,
    c.indicadores?.estado?.estabelecimentos.publicado ?? null,
  );
  const lavoura = montarSomavel(
    c.totais.lavouraValor > 0 ? c.totais.lavouraValor : null,
    regiao?.valorDaProducaoMilReais ?? null,
    c.indicadores?.estado?.valorDaProducaoMilReais ?? null,
  );
  const rebanho = montarSomavel(
    c.totais.bovinos > 0 ? c.totais.bovinos : null,
    regiao?.bovinos ?? null,
    c.indicadores?.estado?.rebanho.publicado ?? null,
  );

  return [
    {
      rotulo: 'Parque de tratores',
      valor: c.comTerritorio && c.totais.municipiosComTratores > 0 ? nº(c.totais.tratores) : null,
      deOnde: contexto(
        tratores,
        `${nº(c.totais.municipiosComTratores)} municípios divulgados${diferencaParaASoma(c.indicadores?.estado?.tratores)}`,
      ),
      procedencia: p?.tratores,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : daAdr.length > 0 ? sobSigilo : 'Censo não carregado',
    },
    {
      rotulo: 'Propriedades',
      valor: c.comTerritorio && algumComPropriedades ? nº(c.totais.estabelecimentos) : null,
      deOnde: contexto(propriedades, `estabelecimentos agropecuários${diferencaParaASoma(c.indicadores?.estado?.estabelecimentos)}`),
      procedencia: p?.estabelecimentos,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : daAdr.length > 0 ? sobSigilo : 'Censo não carregado',
    },
    {
      rotulo: 'Valor da lavoura',
      valor: c.comTerritorio && c.totais.lavouraValor > 0 ? reaisDaProducao(c.totais.lavouraValor) : null,
      deOnde: contexto(lavoura, 'o que a região COLHE, não o que a Tracbel vende'),
      procedencia: p?.valorDaProducao,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'produção agrícola não carregada',
    },
    {
      rotulo: 'Usinas de etanol',
      valor: c.comTerritorio ? nº(c.totais.usinas) : null,
      deOnde: `em ${nº(c.totais.municipiosComUsina)} municípios · ${nº(c.totais.capacidadeDeEtanol)} m³/dia autorizados`,
      procedencia: p?.usinas,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'ANP não carregada',
    },
    {
      rotulo: 'Rebanho bovino',
      valor: c.comTerritorio && c.totais.bovinos > 0 ? nº(c.totais.bovinos) : null,
      deOnde: contexto(rebanho, `cabeças${diferencaParaASoma(c.indicadores?.estado?.rebanho)}`),
      procedencia: p?.rebanho,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'rebanho não carregado',
    },
  ];
}
