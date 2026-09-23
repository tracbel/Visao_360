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

export function kpisDaCarteira(c: ContextoDosKpis): Indicador[] {
  return [
    {
      rotulo: 'Municípios da ADR',
      valor: c.comTerritorio ? c.municipiosDaAdr : null,
      deOnde: `${nº(c.totais.clientes)} clientes com endereço neles`,
      semDado: c.indicadores ? SEM_TERRITORIO : 'área de atuação não carregada',
    },
    {
      rotulo: 'Cobertura pela cadência',
      valor: c.comTerritorio && c.coberturaDaAdr !== null ? porcento(c.coberturaDaAdr) : null,
      tom: 'atencao',
      deOnde: `${nº(c.totais.cobertos)} de ${nº(c.totais.elegiveis)} vínculos elegíveis no prazo · ${nº(c.totais.pendentes)} pendentes · regra provisória`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'sem vínculo elegível',
    },
    {
      rotulo: 'Vendas no período',
      valor: c.comTerritorio ? reaisCompactos(c.totais.vendas) : null,
      deOnde: `máquina ${reaisCompactos(c.totais.maquina)} · pós-venda ${reaisCompactos(c.totais.posVenda)} (composição provisória)`,
      semDado: c.territorioNaoCarregado ? `${reaisCompactos(c.vendasForaDoMapa)} no período, todos fora do mapa` : '—',
    },
    {
      rotulo: 'Parque teórico de máquinas',
      valor: c.comTerritorio && c.totais.municipiosComArea > 0 ? nº(Math.round(c.totais.maquinasTeoricas)) : null,
      deOnde: `${nº(Math.round(c.totais.hectares))} ha úteis em ${nº(c.totais.municipiosComArea)} municípios${
        c.recorte?.demandaAnualDeMaquinas != null ? ` · ${nº(Math.round(c.recorte.demandaAnualDeMaquinas))} por ano` : ''
      }${c.totais.potencialEstimado ? ' · estimativa, regra a confirmar' : ''}`,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'sem regra de potencial',
    },
  ];
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

  const tratores = montarSomavel(c.totais.tratores, regiao?.tratores ?? null, c.indicadores?.estado?.tratores.publicado ?? null);
  const propriedades = montarSomavel(
    c.totais.estabelecimentos,
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
      valor: c.comTerritorio ? nº(c.totais.tratores) : null,
      deOnde: contexto(
        tratores,
        `${nº(c.totais.municipiosComTratores)} municípios divulgados${diferencaParaASoma(c.indicadores?.estado?.tratores)}`,
      ),
      procedencia: p?.tratores,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'Censo não carregado',
    },
    {
      rotulo: 'Propriedades',
      valor: c.comTerritorio ? nº(c.totais.estabelecimentos) : null,
      deOnde: contexto(propriedades, `estabelecimentos agropecuários${diferencaParaASoma(c.indicadores?.estado?.estabelecimentos)}`),
      procedencia: p?.estabelecimentos,
      semDado: c.territorioNaoCarregado ? SEM_TERRITORIO : 'Censo não carregado',
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
