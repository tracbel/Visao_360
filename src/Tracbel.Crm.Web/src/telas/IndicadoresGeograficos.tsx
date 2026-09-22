/**
 * Indicadores Geográficos da ADR — os três mapas do pedido do gerente comercial
 * (documento 32, seção 8; documento 36), com dado do banco do CRM e do IBGE.
 *
 * OS TRÊS MAPAS LADO A LADO, NO MESMO QUADRO: o enquadramento é a ADR inteira e
 * é o mesmo nos três, então o mesmo município está no mesmo lugar em todos — e
 * o cursor sobre ele mostra o detalhe dele nos três ao mesmo tempo.
 *
 * O QUE ESTA TELA NÃO FAZ, e diz na própria tela:
 * - não filtra por SAM/KAM/Varejo, tipo de produto nem modelo — não há dado;
 * - não mostra potencial de cliente nem de não cliente — não há área por
 *   propriedade; o potencial é o do município inteiro, por uma regra ainda a
 *   confirmar;
 * - não mostra valor em reais no potencial — não há preço de máquina confirmado;
 *   o preço das CULTURAS está no painel do fim (issue 66);
 * - não oferece FYTD — o calendário fiscal não foi confirmado.
 *
 * OS NÚMEROS DA MAQUETE NÃO ESTÃO AQUI. Ela era ilustrativa; nenhum valor desta
 * tela é simulado.
 */

import { useEffect, useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DetalheDoMunicipio } from '../componentes/territorio/DetalheDoMunicipio';
import { PainelDeCredito } from '../componentes/territorio/PainelDeCredito';
import { PainelDeCustos } from '../componentes/territorio/PainelDeCustos';
import { PainelDePrecos } from '../componentes/territorio/PainelDePrecos';
import {
  FAIXAS_AREA_PLANTADA,
  FAIXAS_COBERTURA_PERCENTUAL,
  FAIXAS_DENSIDADE_DE_TRATORES,
  FAIXAS_ESTABELECIMENTOS,
  FAIXAS_PENDENCIA_PERCENTUAL,
  FAIXAS_PENDENCIA_QUANTIDADE,
  FAIXAS_POTENCIAL,
  FAIXAS_REBANHO,
  FAIXAS_TRATORES,
  FAIXAS_USINAS,
  FAIXAS_VALOR_DA_PRODUCAO,
  FAIXAS_VENDAS,
  faixaDe,
  reaisCompactos,
  reaisDaProducao,
} from '../componentes/territorio/escalas';
import {
  LegendaDoMapa,
  MapaDeMunicipios,
  type EstadoNoMapa,
  type PoligonoProjetado,
} from '../componentes/territorio/MapaDeMunicipios';
import { caminhoSvg, enquadrar, type ColecaoMunicipal } from '../componentes/territorio/projecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { carregarMalhaDeSaoPaulo, obterIndicadoresTerritoriais } from '../dados/api/territorio';
import { useRecurso } from '../dados/api/useRecurso';
import type { ClassificacaoDeIndicador, FiltrosTerritoriais, IndicadoresDoMunicipio } from '../tipos/territorio';
import '../estilos/territorio.css';

/** A largura de referência do desenho; o SVG escala para o cartão. */
const LARGURA_DO_DESENHO = 520;

type ModoDeCobertura = 'cobertura' | 'pendencia' | 'quantidade';
type RecorteDeVendas = 'valorLiquido' | 'maquina' | 'posVenda';

/** O que o mapa D pinta. Cada recorte vem de uma pesquisa diferente, com ano próprio. */
type RecorteDaEstrutura = 'tratores' | 'densidade' | 'estabelecimentos' | 'rebanho' | 'usinas';

/** O que o mapa C pinta: a regra, ou a lavoura que a sustenta. */
type RecorteDoPotencial = 'maquinas' | 'areaPlantada' | 'valorDaProducao';

const ROTULO_DE_COBERTURA: Record<ModoDeCobertura, string> = {
  cobertura: '% no prazo',
  pendencia: '% pendente',
  quantidade: 'pendentes (qtd.)',
};

const UNIDADE_DE_COBERTURA: Record<ModoDeCobertura, string> = {
  cobertura: '% dos vínculos elegíveis com contato no prazo da cadência — verde é mais coberto',
  pendencia: '% dos vínculos elegíveis fora do prazo ou nunca contatados — vermelho é mais pendente',
  quantidade: 'vínculos pendentes (fora do prazo + nunca contatados)',
};

const FAIXAS_DE_COBERTURA = {
  cobertura: FAIXAS_COBERTURA_PERCENTUAL,
  pendencia: FAIXAS_PENDENCIA_PERCENTUAL,
  quantidade: FAIXAS_PENDENCIA_QUANTIDADE,
} as const;

const ROTULO_DE_VENDAS: Record<RecorteDeVendas, string> = {
  valorLiquido: 'Total líquido',
  maquina: 'Máquina',
  posVenda: 'Pós-venda',
};

const ROTULO_DA_ESTRUTURA: Record<RecorteDaEstrutura, string> = {
  tratores: 'Tratores',
  densidade: 'Tratores / mil km²',
  estabelecimentos: 'Propriedades',
  rebanho: 'Rebanho bovino',
  usinas: 'Usinas de etanol',
};

const FAIXAS_DA_ESTRUTURA = {
  tratores: FAIXAS_TRATORES,
  densidade: FAIXAS_DENSIDADE_DE_TRATORES,
  estabelecimentos: FAIXAS_ESTABELECIMENTOS,
  rebanho: FAIXAS_REBANHO,
  usinas: FAIXAS_USINAS,
} as const;

const UNIDADE_DA_ESTRUTURA: Record<RecorteDaEstrutura, string> = {
  tratores: 'tratores existentes (Censo Agropecuário) — hachurado é sigilo do IBGE, não zero',
  densidade: 'tratores por mil km² — a densidade, que compara município grande com pequeno',
  estabelecimentos: 'estabelecimentos agropecuários (Censo Agropecuário)',
  rebanho: 'cabeças de bovino (Pesquisa da Pecuária Municipal)',
  usinas: 'capacidade autorizada de etanol, em m³/dia (ANP) — hachurado é município sem usina de etanol',
};

const ROTULO_DO_POTENCIAL: Record<RecorteDoPotencial, string> = {
  maquinas: 'Máquinas teóricas',
  areaPlantada: 'Área plantada',
  valorDaProducao: 'Valor da produção',
};

const FAIXAS_DO_POTENCIAL = {
  maquinas: FAIXAS_POTENCIAL,
  areaPlantada: FAIXAS_AREA_PLANTADA,
  valorDaProducao: FAIXAS_VALOR_DA_PRODUCAO,
} as const;

const UNIDADE_DO_POTENCIAL: Record<RecorteDoPotencial, string> = {
  maquinas: 'máquinas teóricas na região (necessidade de frota, não venda nem valor)',
  areaPlantada: 'hectares plantados de TODAS as culturas do município (IBGE/PAM)',
  valorDaProducao: 'valor da produção agrícola do município — o que ele COLHE, não o que a Tracbel vende',
};

const nº = (v: number) => v.toLocaleString('pt-BR');
const porcento = (v: number) => `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;

/** `2025-09-01` vira `set/2025`. */
function mes(competencia: string): string {
  const [ano, m] = competencia.split('-');
  return `${['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'][Number(m) - 1]}/${ano}`;
}

/**
 * O ano civil até o último mês fechado — o recorte "acumulado do ano" que existe sem calendário fiscal.
 * Em janeiro, o último mês fechado é dezembro: o recorte vira o ano anterior inteiro.
 */
function anoCivilFechado(hoje = new Date()): Pick<FiltrosTerritoriais, 'competenciaInicial' | 'competenciaFinal'> {
  const ultimoFechado = new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1);
  const ano = ultimoFechado.getFullYear();
  return {
    competenciaInicial: `${ano}-01`,
    competenciaFinal: `${ano}-${String(ultimoFechado.getMonth() + 1).padStart(2, '0')}`,
  };
}

export function IndicadoresGeograficos() {
  const { contexto } = useContextoDeAcesso();

  const [filtros, setFiltros] = useState<FiltrosTerritoriais>({
    competenciaInicial: '',
    competenciaFinal: '',
    regiao: '',
    lojaCodigo: '',
    visao: 'Filial',
    filialDaVenda: '',
    filialDoCliente: '',
  });
  const [modoDeCobertura, setModoDeCobertura] = useState<ModoDeCobertura>('cobertura');
  const [recorteDeVendas, setRecorteDeVendas] = useState<RecorteDeVendas>('valorLiquido');
  const [recorteDaEstrutura, setRecorteDaEstrutura] = useState<RecorteDaEstrutura>('tratores');
  const [recorteDoPotencial, setRecorteDoPotencial] = useState<RecorteDoPotencial>('maquinas');
  const [selecionado, setSelecionado] = useState<number | null>(null);
  const [emFoco, setEmFoco] = useState<number | null>(null);
  const [lojasConhecidas, setLojasConhecidas] = useState<Map<string, string>>(() => new Map());
  const [adrConhecida, setAdrConhecida] = useState<ReadonlySet<number>>(() => new Set());

  // AS LOJAS DO FILTRO E O QUADRO DO MAPA SÃO OS QUE A ADR JÁ MOSTROU, acumulados a cada resposta.
  // Tirá-los só da resposta corrente faria a lista encolher — e o mapa mudar de enquadramento — ao
  // escolher uma região, e duas capturas da mesma cidade deixariam de ser comparáveis.
  const painel = useRecurso(
    (sinal) =>
      obterIndicadoresTerritoriais(contexto, filtros, sinal).then((resposta) => {
        const municipiosDaResposta = resposta.dados.indicadores.municipios;
        setLojasConhecidas((atual) => {
          const proximo = new Map(atual);
          for (const m of municipiosDaResposta)
            if (m.lojaCodigo && m.lojaNome) proximo.set(m.lojaCodigo, m.lojaNome.replace(/^.*—\s*/, ''));
          return proximo.size === atual.size ? atual : proximo;
        });
        setAdrConhecida((atual) => {
          const novos = municipiosDaResposta.filter((m) => m.pertenceAAdr && !atual.has(m.codigoIbge));
          return novos.length === 0 ? atual : new Set([...atual, ...novos.map((m) => m.codigoIbge)]);
        });
        return resposta;
      }),
    [
      contexto.empresa,
      contexto.usuario,
      filtros.competenciaInicial,
      filtros.competenciaFinal,
      filtros.regiao,
      filtros.lojaCodigo,
      filtros.visao,
      filtros.filialDaVenda,
      filtros.filialDoCliente,
    ],
  );

  const [malha, setMalha] = useState<ColecaoMunicipal | null>(null);
  const [erroDaMalha, setErroDaMalha] = useState<Error | null>(null);

  useEffect(() => {
    const controlador = new AbortController();
    carregarMalhaDeSaoPaulo(controlador.signal)
      .then(setMalha)
      .catch((causa: unknown) => {
        if (!controlador.signal.aborted) setErroDaMalha(causa instanceof Error ? causa : new Error(String(causa)));
      });
    return () => controlador.abort();
  }, []);

  const indicadores = painel.dados?.indicadores ?? null;
  const municipios = useMemo(() => indicadores?.municipios ?? [], [indicadores]);
  const porCodigo = useMemo(() => new Map(municipios.map((m) => [m.codigoIbge, m])), [municipios]);
  const adr = useMemo(() => new Set(municipios.filter((m) => m.pertenceAAdr).map((m) => m.codigoIbge)), [municipios]);
  const regra = indicadores?.regras[0] ?? null;

  const desenho = useMemo(() => {
    if (!malha) return null;
    // O QUADRO É A ADR INTEIRA, o mesmo nos três mapas. Os vizinhos continuam desenhados e o SVG corta o
    // que passa da borda: o mapa fica do tamanho da área de atuação, e não do estado.
    const enquadramento = enquadrar(
      malha,
      LARGURA_DO_DESENHO,
      10,
      adrConhecida.size > 0 ? (codigo) => adrConhecida.has(codigo) : undefined,
    );
    const poligonos: PoligonoProjetado[] = malha.features.map((f) => ({
      codigo: Number(f.properties.codarea),
      nome: f.properties.nome,
      caminho: caminhoSvg(enquadramento, f.geometry),
    }));
    return { enquadramento, poligonos };
  }, [malha, adrConhecida]);

  const nomeDoPoligono = useMemo(() => new Map((desenho?.poligonos ?? []).map((p) => [p.codigo, p.nome])), [desenho]);

  const daAdr = useMemo(() => municipios.filter((m) => m.pertenceAAdr), [municipios]);

  const totais = useMemo(() => {
    const soma = (f: (m: IndicadoresDoMunicipio) => number) => daAdr.reduce((s, m) => s + f(m), 0);
    // PRIMEIRO EXISTE, DEPOIS TEM VALOR: sem regra ativa a lista vem vazia, e sem área o valor vem nulo.
    const comArea = daAdr.filter((m) => m.potencial.length > 0 && m.potencial[0].maquinasTeoricas !== null);
    return {
      elegiveis: soma((m) => m.cobertura.vinculosComCadencia),
      cobertos: soma((m) => m.cobertura.cobertos),
      foraDaCadencia: soma((m) => m.cobertura.foraDaCadencia),
      nuncaContatados: soma((m) => m.cobertura.nuncaContatados),
      pendentes: soma((m) => m.cobertura.pendentes),
      clientes: soma((m) => m.cobertura.clientes),
      vendas: soma((m) => m.vendas.valorLiquido),
      maquina: soma((m) => m.vendas.maquina),
      posVenda: soma((m) => m.vendas.posVenda),
      clientesQueCompraram: soma((m) => m.vendas.clientesQueCompraram),
      maquinasTeoricas: comArea.reduce((s, m) => s + (m.potencial[0].maquinasTeoricas ?? 0), 0),
      hectares: comArea.reduce((s, m) => s + (m.potencial[0].areaPlantadaHectares ?? 0), 0),
      municipiosComArea: comArea.length,

      // A SOMA IGNORA O SIGILO em vez de contá-lo como zero: o total é "o que o IBGE divulgou",
      // e o número de municípios que entraram fica ao lado para que isso seja visível.
      tratores: soma((m) => m.estrutura.tratores ?? 0),
      municipiosComTratores: daAdr.filter((m) => m.estrutura.tratores !== null).length,
      estabelecimentos: soma((m) => m.estrutura.estabelecimentos ?? 0),
      bovinos: soma((m) => m.estrutura.bovinos ?? 0),
      areaKm2: soma((m) => m.estrutura.areaKm2 ?? 0),
      usinas: soma((m) => m.estrutura.usinas.length),
      municipiosComUsina: daAdr.filter((m) => m.estrutura.usinas.length > 0).length,
      capacidadeDeEtanol: soma((m) => m.estrutura.capacidadeDeEtanolM3Dia ?? 0),
      lavouraHectares: soma((m) => m.producao?.areaPlantadaHectares ?? 0),
      lavouraValor: soma((m) => m.producao?.valorDaProducaoMilReais ?? 0),
    };
  }, [daAdr]);

  /**
   * A fatia da região dentro de São Paulo.
   *
   * O DENOMINADOR É O TOTAL PUBLICADO pelo IBGE, e não a soma dos 645 municípios: o valor municipal
   * sigiloso entra no total do estado sem aparecer embaixo, e é o número publicado que a diretoria
   * encontra em qualquer outra fonte.
   */
  const fatiaNoEstado = useMemo(() => {
    const estado = indicadores?.estado;
    if (!estado) return null;
    const parte = (regiao: number, total: number | null) =>
      total && total > 0 ? (100 * regiao) / total : null;
    return {
      ano: estado.ano,
      area: parte(totais.lavouraHectares, estado.areaPlantadaHectares),
      valor: parte(totais.lavouraValor, estado.valorDaProducaoMilReais),
      tratores: parte(totais.tratores, estado.tratores),
      estabelecimentos: parte(totais.estabelecimentos, estado.estabelecimentos),
    };
  }, [indicadores, totais]);

  const coberturaDaAdr = totais.elegiveis > 0 ? (100 * totais.cobertos) / totais.elegiveis : null;

  /** Os anos das fontes da estrutura — eles diferem, e a tela precisa dizer qual é qual. */
  const estruturaDaAdr = useMemo(
    () => ({
      anoDoCenso: daAdr.find((m) => m.estrutura.anoDoCenso !== null)?.estrutura.anoDoCenso ?? null,
      anoDoRebanho: daAdr.find((m) => m.estrutura.anoDoRebanho !== null)?.estrutura.anoDoRebanho ?? null,
    }),
    [daAdr],
  );

  function estadoDaCobertura(codigo: number): EstadoNoMapa {
    const m = porCodigo.get(codigo);
    if (!m?.pertenceAAdr) return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };
    const c = m.cobertura;
    if (c.vinculosComCadencia === 0) return { tipo: 'semDado', detalhe: 'sem vínculo elegível para medir' };
    const noPrazo = (100 * c.cobertos) / c.vinculosComCadencia;
    const valor =
      modoDeCobertura === 'cobertura' ? noPrazo : modoDeCobertura === 'pendencia' ? (c.percentualPendente ?? 0) : c.pendentes;
    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_DE_COBERTURA[modoDeCobertura], valor).cor,
      detalhe: `${nº(c.vinculosComCadencia)} elegíveis · ${nº(c.cobertos)} no prazo (${porcento(noPrazo)}) · ${nº(c.pendentes)} pendentes (${nº(c.foraDaCadencia)} fora do prazo + ${nº(c.nuncaContatados)} nunca)`,
    };
  }

  function estadoDasVendas(codigo: number): EstadoNoMapa {
    const m = porCodigo.get(codigo);
    if (!m?.pertenceAAdr) return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };
    const valor = m.vendas[recorteDeVendas];
    if (m.cobertura.clientes === 0 && valor === 0) return { tipo: 'semDado', detalhe: 'nenhum cliente com endereço aqui' };
    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_VENDAS, valor).cor,
      detalhe: `${ROTULO_DE_VENDAS[recorteDeVendas]}: ${reaisCompactos(valor)} · máquina ${reaisCompactos(m.vendas.maquina)} · pós-venda ${reaisCompactos(m.vendas.posVenda)} · ${nº(m.vendas.clientesQueCompraram)} clientes compraram`,
    };
  }

  function estadoDoPotencial(codigo: number): EstadoNoMapa {
    const m = porCodigo.get(codigo);
    if (!m?.pertenceAAdr) return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };

    // A CULTURA DA REGRA E A LAVOURA INTEIRA SÃO COISAS DIFERENTES: o recorte "máquinas teóricas"
    // olha só o produto da regra (café), e os outros dois olham TODAS as culturas do município.
    // Misturá-los faria a área do café aparecer ao lado do valor da lavoura inteira como se fossem
    // a mesma base.
    const potencial = m.potencial[0];
    const producao = m.producao;

    const valor =
      recorteDoPotencial === 'maquinas'
        ? (potencial?.maquinasTeoricas ?? null)
        : recorteDoPotencial === 'areaPlantada'
          ? (producao?.areaPlantadaHectares ?? null)
          : (producao?.valorDaProducaoMilReais ?? null);

    if (valor === null)
      return {
        tipo: 'semDado',
        detalhe:
          recorteDoPotencial === 'maquinas'
            ? 'área plantada da cultura da regra não disponível'
            : 'produção agrícola não carregada para este município',
      };

    // O ANO DA CULTURA SÓ APARECE QUANDO DIFERE DO DA LAVOURA ao lado (issue 152): os dois números estão na mesma
    // linha, e o leitor precisa saber que não são do mesmo ano.
    const anoDaCultura =
      potencial?.ano != null && potencial.ano !== producao?.ano ? ` (${potencial.ano})` : '';
    const daRegra =
      potencial?.maquinasTeoricas !== null && potencial !== undefined
        ? `${nº(potencial.areaPlantadaHectares ?? 0)} ha de ${regra?.produtoNome ?? 'cultura da regra'}${anoDaCultura} · ${nº(potencial.maquinasTeoricas ?? 0)} ${regra?.modeloDeReferencia ?? 'máquinas'} teóricos`
        : 'sem área da cultura da regra';

    const daLavoura = producao
      ? `lavoura ${nº(Math.round(producao.areaPlantadaHectares ?? 0))} ha em ${nº(producao.culturasComArea)} culturas · ${reaisDaProducao(producao.valorDaProducaoMilReais ?? 0)} (${producao.ano})`
      : 'lavoura não carregada';

    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_DO_POTENCIAL[recorteDoPotencial], valor).cor,
      detalhe: `${daRegra} · ${daLavoura}`,
    };
  }

  /**
   * O mapa da estrutura agropecuária.
   *
   * SEM USINA NÃO É CAPACIDADE ZERO. Município fora da lista da ANP fica hachurado, porque a
   * ausência ali só prova que não há usina de ETANOL — usina que só faz açúcar não é autorizada
   * pela ANP e não aparece. Já a usina parada, com capacidade zerada, é um valor e entra na escala.
   */
  function estadoDaEstrutura(codigo: number): EstadoNoMapa {
    const m = porCodigo.get(codigo);
    if (!m?.pertenceAAdr) return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };

    const e = m.estrutura;
    const valor =
      recorteDaEstrutura === 'tratores'
        ? e.tratores
        : recorteDaEstrutura === 'densidade'
          ? e.tratoresPorMilKm2
          : recorteDaEstrutura === 'estabelecimentos'
            ? e.estabelecimentos
            : recorteDaEstrutura === 'rebanho'
              ? e.bovinos
              : e.usinas.length === 0
                ? null
                : (e.capacidadeDeEtanolM3Dia ?? 0);

    if (valor === null)
      return {
        tipo: 'semDado',
        detalhe:
          recorteDaEstrutura === 'usinas'
            ? 'sem usina de etanol autorizada aqui (a ANP não enxerga usina só de açúcar)'
            : 'sigilo do IBGE ou fonte não carregada — não é zero',
      };

    const parque =
      e.tratores === null
        ? 'tratores sob sigilo'
        : `${nº(e.tratores)} tratores${e.tratoresAbaixoDe100Cv !== null ? ` (${nº(e.tratoresAbaixoDe100Cv)} < 100 cv)` : ''}`;

    const usinas =
      e.usinas.length === 0
        ? 'sem usina'
        : `${nº(e.usinas.length)} usina${e.usinas.length > 1 ? 's' : ''}${e.capacidadeDeEtanolM3Dia !== null ? ` · ${nº(e.capacidadeDeEtanolM3Dia)} m³/d` : ''}`;

    return {
      tipo: 'valor',
      cor: faixaDe(FAIXAS_DA_ESTRUTURA[recorteDaEstrutura], valor).cor,
      detalhe: `${parque}${e.anoDoCenso ? ` (${e.anoDoCenso})` : ''} · ${e.estabelecimentos !== null ? `${nº(e.estabelecimentos)} propriedades` : 'propriedades sob sigilo'} · ${e.bovinos !== null ? `${nº(e.bovinos)} bovinos${e.anoDoRebanho ? ` (${e.anoDoRebanho})` : ''}` : 'sem rebanho'} · ${usinas}`,
    };
  }

  /** O detalhe do município sob o cursor, na medida de cada mapa. */
  function textoDoFoco(estadoDe: (codigo: number) => EstadoNoMapa): string {
    if (emFoco === null) return 'Passe o cursor sobre um município para ver o número dele nos três mapas; clique para abrir a ficha.';
    return `${nomeDoPoligono.get(emFoco) ?? emFoco} — ${estadoDe(emFoco).detalhe}`;
  }

  const classificacao = (indicador: ClassificacaoDeIndicador['indicador']) =>
    painel.dados?.classificacoes.find((c) => c.indicador === indicador) ?? null;

  // TERRITÓRIO NÃO CARREGADO NÃO É ZERO. A consulta respondeu, sem filtro de região nem de loja, e nenhum
  // município veio marcado como ADR: a carga do território (catálogo IBGE, área de atuação, responsáveis,
  // área plantada) não rodou neste banco. Mostrar "0 municípios" e "R$ 0" nesse caso faz a falta de carga
  // parecer resultado — e as vendas continuam no banco, somadas nos grupos fora do mapa.
  const territorioNaoCarregado = indicadores !== null && filtros.regiao === '' && filtros.lojaCodigo === '' && daAdr.length === 0;
  const vendasForaDoMapa = indicadores?.foraDoMapa.reduce((s, g) => s + g.vendas.valorLiquido, 0) ?? 0;
  const semCodigoIbge = indicadores?.foraDoMapa.find((g) => g.grupo === 'MunicipioSemCodigoIbge') ?? null;
  const comTerritorio = indicadores !== null && !territorioNaoCarregado;
  const semTerritorio = 'território não carregado neste banco';

  const anoCivil = anoCivilFechado();
  const presetDoPeriodo =
    filtros.competenciaInicial === '' && filtros.competenciaFinal === ''
      ? '12meses'
      : filtros.competenciaInicial === anoCivil.competenciaInicial && filtros.competenciaFinal === anoCivil.competenciaFinal
        ? 'anoCivil'
        : 'personalizado';

  const kpis: Indicador[] = [
    {
      rotulo: 'Municípios da ADR',
      valor: comTerritorio ? daAdr.length : null,
      deOnde: `${nº(totais.clientes)} clientes com endereço neles`,
      semDado: indicadores ? semTerritorio : 'área de atuação não carregada',
    },
    {
      rotulo: 'Cobertura pela cadência',
      valor: comTerritorio && coberturaDaAdr !== null ? porcento(coberturaDaAdr) : null,
      tom: 'atencao',
      deOnde: `${nº(totais.cobertos)} de ${nº(totais.elegiveis)} vínculos elegíveis no prazo · ${nº(totais.pendentes)} pendentes · regra provisória`,
      semDado: territorioNaoCarregado ? semTerritorio : 'sem vínculo elegível',
    },
    {
      rotulo: 'Vendas no período',
      valor: comTerritorio ? reaisCompactos(totais.vendas) : null,
      deOnde: `máquina ${reaisCompactos(totais.maquina)} · pós-venda ${reaisCompactos(totais.posVenda)} (composição provisória)`,
      semDado: territorioNaoCarregado ? `${reaisCompactos(vendasForaDoMapa)} no período, todos fora do mapa` : '—',
    },
    {
      rotulo: regra ? `${regra.modeloDeReferencia} teóricos` : 'Potencial teórico',
      valor: comTerritorio && regra ? nº(Math.round(totais.maquinasTeoricas)) : null,
      deOnde: regra ? `1 a cada ${regra.hectaresPorMaquina} ha de ${regra.produtoNome} · estimativa regional, regra a confirmar` : '',
      semDado: territorioNaoCarregado ? semTerritorio : 'sem regra de potencial',
    },
  ];

  // O QUE A REGIÃO TEM, E QUE FATIA DE SÃO PAULO ELA É. Sem o denominador do estado, "62 mil
  // tratores" é um número solto; com ele, é a posição da Tracbel Agro no mercado paulista.
  const kpisDoMercado: Indicador[] = [
    {
      rotulo: 'Parque de tratores',
      valor: comTerritorio ? nº(totais.tratores) : null,
      deOnde: `Censo Agropecuário${estruturaDaAdr.anoDoCenso ? ` ${estruturaDaAdr.anoDoCenso}` : ''} · ${nº(totais.municipiosComTratores)} municípios divulgados${fatiaNoEstado?.tratores !== null && fatiaNoEstado !== null ? ` · ${porcento(fatiaNoEstado.tratores)} de São Paulo` : ''}`,
      semDado: territorioNaoCarregado ? semTerritorio : 'Censo não carregado',
    },
    {
      rotulo: 'Propriedades',
      valor: comTerritorio ? nº(totais.estabelecimentos) : null,
      deOnde: `estabelecimentos agropecuários${fatiaNoEstado?.estabelecimentos != null ? ` · ${porcento(fatiaNoEstado.estabelecimentos)} de São Paulo` : ''}`,
      semDado: territorioNaoCarregado ? semTerritorio : 'Censo não carregado',
    },
    {
      rotulo: 'Valor da lavoura',
      valor: comTerritorio && totais.lavouraValor > 0 ? reaisDaProducao(totais.lavouraValor) : null,
      deOnde: `o que a região COLHE, não o que a Tracbel vende · PAM${fatiaNoEstado ? ` ${fatiaNoEstado.ano}` : ''}${fatiaNoEstado?.valor != null ? ` · ${porcento(fatiaNoEstado.valor)} de São Paulo` : ''}`,
      semDado: territorioNaoCarregado ? semTerritorio : 'produção agrícola não carregada',
    },
    {
      rotulo: 'Usinas de etanol',
      valor: comTerritorio ? nº(totais.usinas) : null,
      deOnde: `em ${nº(totais.municipiosComUsina)} municípios · ${nº(totais.capacidadeDeEtanol)} m³/dia autorizados (ANP) · não inclui usina só de açúcar`,
      semDado: territorioNaoCarregado ? semTerritorio : 'ANP não carregada',
    },
    {
      rotulo: 'Rebanho bovino',
      valor: comTerritorio && totais.bovinos > 0 ? nº(totais.bovinos) : null,
      deOnde: `cabeças · Pesquisa da Pecuária Municipal${estruturaDaAdr.anoDoRebanho ? ` ${estruturaDaAdr.anoDoRebanho}` : ''}, anual`,
      semDado: territorioNaoCarregado ? semTerritorio : 'rebanho não carregado',
    },
  ];

  const escolhido = selecionado === null ? null : porCodigo.get(selecionado) ?? null;
  const ordenados = useMemo(() => [...daAdr].sort((a, b) => b.vendas.valorLiquido - a.vendas.valorLiquido), [daAdr]);
  const semFiltro = filtros.regiao === '' && filtros.lojaCodigo === '';

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Indicadores Geográficos da ADR</h1>
          <p className="page-subtitle">
            Área de atuação da Tracbel Agro em São Paulo, município a município: cobertura de carteira, vendas e potencial
            por área. Dado do banco do CRM e do IBGE — nenhum número desta tela é ilustrativo.
          </p>
        </div>
        <SeloProcedencia procedencia={painel.procedencia} />
      </div>

      <div className="card cad-cartao terr-cartao">
        <div className="terr-alcance">
          <span>
            <strong>Visão:</strong>{' '}
            {filtros.visao === 'Empresa'
              ? 'empresa inteira — cada venda no município do cliente, qualquer que seja a filial que faturou.'
              : `filial ${contexto.empresa || 'do login'} e as abaixo dela — os clientes cadastrados nela e as notas que ela emitiu.`}
          </span>
          <span>
            <strong>Empresa inteira:</strong>{' '}
            {painel.dados?.podeVerEmpresaInteira
              ? 'disponível para o seu perfil.'
              : 'exige a permissão de alcance entre filiais; o seu perfil não a tem, e a distribuição oficial dos acessos está pendente (documento 32, P-10).'}
          </span>
          <span>A ADR e a área plantada são da empresa inteira e aparecem para todos.</span>
        </div>
      </div>

      <div className="card cad-cartao terr-cartao">
        <div className="terr-filtros">
          <div className="terr-periodo" role="group" aria-label="Período das vendas">
            Período das vendas
            <div className="terr-alternador">
              <button
                type="button"
                aria-pressed={presetDoPeriodo === '12meses'}
                onClick={() => setFiltros((f) => ({ ...f, competenciaInicial: '', competenciaFinal: '' }))}
              >
                12 meses fechados
              </button>
              <button
                type="button"
                aria-pressed={presetDoPeriodo === 'anoCivil'}
                onClick={() => setFiltros((f) => ({ ...f, ...anoCivil }))}
              >
                Ano civil até o último mês fechado
              </button>
              <button type="button" aria-pressed={presetDoPeriodo === 'personalizado'} disabled={presetDoPeriodo !== 'personalizado'}>
                Personalizado
              </button>
            </div>
            <span className="terr-filtro-motivo">
              FYTD não é oferecido: o calendário fiscal não foi confirmado (documento 32, P-4). O mês em curso fica fora do padrão.
            </span>
          </div>
          <label className="terr-filtro">
            Vendas de
            <input
              type="month"
              value={filtros.competenciaInicial}
              onChange={(e) => setFiltros((f) => ({ ...f, competenciaInicial: e.target.value }))}
            />
          </label>
          <label className="terr-filtro">
            até
            <input
              type="month"
              value={filtros.competenciaFinal}
              onChange={(e) => setFiltros((f) => ({ ...f, competenciaFinal: e.target.value }))}
            />
          </label>
          <label className="terr-filtro">
            Região da ADR
            <select
              value={filtros.regiao}
              onChange={(e) => setFiltros((f) => ({ ...f, regiao: e.target.value as FiltrosTerritoriais['regiao'] }))}
            >
              <option value="">Norte e Noroeste</option>
              <option value="Norte">Norte</option>
              <option value="Noroeste">Noroeste</option>
            </select>
          </label>
          <label className="terr-filtro">
            Loja responsável
            <select value={filtros.lojaCodigo} onChange={(e) => setFiltros((f) => ({ ...f, lojaCodigo: e.target.value }))}>
              <option value="">Todas</option>
              {[...lojasConhecidas.entries()]
                .sort((a, b) => a[1].localeCompare(b[1]))
                .map(([codigo, nome]) => (
                  <option key={codigo} value={codigo}>
                    {nome}
                  </option>
                ))}
            </select>
          </label>
          <label className="terr-filtro">
            Visão
            <select
              value={filtros.visao}
              onChange={(e) => setFiltros((f) => ({ ...f, visao: e.target.value as FiltrosTerritoriais['visao'] }))}
            >
              <option value="Filial">Filial do cabeçalho</option>
              <option value="Empresa" disabled={painel.dados ? !painel.dados.podeVerEmpresaInteira : false}>
                Empresa inteira
              </option>
            </select>
            {painel.dados && !painel.dados.podeVerEmpresaInteira && (
              <span className="terr-filtro-motivo">empresa inteira: seu perfil não tem a permissão</span>
            )}
          </label>
          <FiltroDeFilial
            rotulo="Filial que vendeu"
            aplicaA="só às vendas"
            valor={filtros.filialDaVenda}
            lojas={lojasConhecidas}
            aoMudar={(valor) => setFiltros((f) => ({ ...f, filialDaVenda: valor }))}
          />
          <FiltroDeFilial
            rotulo="Filial de cadastro do cliente"
            aplicaA="à cobertura e às vendas"
            valor={filtros.filialDoCliente}
            lojas={lojasConhecidas}
            aoMudar={(valor) => setFiltros((f) => ({ ...f, filialDoCliente: valor }))}
          />
          <label className="terr-filtro">
            Cultura da regra
            <select value={regra?.produtoCodigoIbge ?? ''} disabled={!regra}>
              {regra && <option value={regra.produtoCodigoIbge}>{regra.produtoNome}</option>}
            </select>
            <span className="terr-filtro-motivo">só há uma regra informada</span>
          </label>
          <FiltroSemDado rotulo="Tipo de cliente" opcoes="SAM · KAM · Varejo" motivo="não há classificação por cliente" />
          <FiltroSemDado rotulo="Tipo de produto" opcoes="colhedora · trator grande · médio" motivo="vendas sem item da nota" />
          <FiltroSemDado rotulo="Modelo" opcoes="modelo da máquina" motivo="vendas sem item da nota" />
          <FiltroSemDado
            rotulo="CEN / gestor"
            opcoes="—"
            motivo="a carteira do CRM ainda não diz quem atende cada município (issue 107); o que ela já tem está na ficha do município"
          />
        </div>
        {indicadores && (
          <p className="cad-sub">
            Vendas de <strong>{mes(indicadores.competenciaInicial)}</strong> a{' '}
            <strong>{mes(indicadores.competenciaFinal)}</strong>
            {presetDoPeriodo === '12meses' && ' (12 meses fechados)'}
            {presetDoPeriodo === 'anoCivil' && ' (ano civil até o último mês fechado)'} · cobertura medida em{' '}
            {new Date(indicadores.referenciaDaCobertura).toLocaleDateString('pt-BR')}, interação mais recente carregada em{' '}
            {indicadores.interacaoMaisRecente ? new Date(indicadores.interacaoMaisRecente).toLocaleDateString('pt-BR') : '—'}
            {indicadores.anoDaAreaPlantada && ` · área plantada PAM/IBGE ${indicadores.anoDaAreaPlantada}`}.
          </p>
        )}
      </div>

      {painel.dados && painel.dados.classificacoes.length > 0 && (
        <div className="card cad-cartao terr-cartao">
          <div className="terr-classificacoes" aria-label="Como ler estes números">
            <strong>Como ler estes números</strong>
            {painel.dados.classificacoes.map((c) => (
              <span key={c.indicador} className="terr-classificacao">
                <SeloDeClassificacao classificacao={c} /> {c.motivo}
              </span>
            ))}
          </div>
        </div>
      )}

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}
      {erroDaMalha && <BlocoErro erro={erroDaMalha} />}

      {territorioNaoCarregado && indicadores && (
        <div className="card cad-cartao terr-cartao terr-territorio-sem-carga" role="status">
          <BlocoVazio
            titulo="O território ainda não foi carregado neste banco"
            texto={
              <>
                A consulta respondeu, mas nenhum município tem código IBGE nem está marcado como ADR: a carga do território
                (catálogo IBGE, área de atuação, responsáveis e área plantada) não rodou neste banco.{' '}
                <strong>Não é falta de permissão nem falha de carregamento</strong>, e não é zero: as vendas do período continuam
                no banco — {reaisCompactos(vendasForaDoMapa)}, somadas nos grupos fora do mapa
                {semCodigoIbge && `, dos quais ${reaisCompactos(semCodigoIbge.vendas.valorLiquido)} de clientes cujo município ainda não tem código IBGE`}
                . Os mapas e os totais da ADR aparecem assim que a carga do território rodar neste banco (documento 32, §4.6.1).
              </>
            }
          />
        </div>
      )}

      <PainelDeIndicadores indicadores={kpis} carregando={painel.carregando} />

      <div className="terr-secao-mercado">
        <h2 className="terr-secao-titulo">O mercado da região</h2>
        <p className="terr-secao-subtitulo">
          O que existe no território, por fonte pública — e que fatia de São Paulo isso representa. O denominador é o
          total <strong>publicado</strong> pelo IBGE, que não é a soma dos municípios: o valor municipal sigiloso entra
          nele sem aparecer embaixo.
        </p>
      </div>
      <PainelDeIndicadores indicadores={kpisDoMercado} carregando={painel.carregando} />

      {(painel.carregando || !desenho) && !painel.erro && !erroDaMalha && <BlocoCarregando oQue="os mapas da ADR" />}

      {indicadores && desenho && !territorioNaoCarregado && (
        <div className="terr-grade-mapas">
          <div className="card cad-cartao terr-mapa">
            <div className="card-title">
              Cobertura de carteira <SeloDeClassificacao classificacao={classificacao('coberturaDeVisita')} />
            </div>
            <div className="card-subtitle">
              Último contato registrado de cada vínculo em carteira comercial, contra a cadência declarada da linha.
            </div>
            <p className="terr-mapa-resumo">
              {totais.elegiveis > 0
                ? `${nº(totais.elegiveis)} elegíveis · ${nº(totais.cobertos)} no prazo (${porcento(coberturaDaAdr ?? 0)}) · ${nº(totais.pendentes)} pendentes`
                : 'nenhum vínculo elegível no recorte'}
            </p>
            <div className="terr-alternador" role="group" aria-label="Cobertura em">
              {(Object.keys(ROTULO_DE_COBERTURA) as ModoDeCobertura[]).map((modo) => (
                <button key={modo} type="button" aria-pressed={modoDeCobertura === modo} onClick={() => setModoDeCobertura(modo)}>
                  {ROTULO_DE_COBERTURA[modo]}
                </button>
              ))}
            </div>
            <MapaDeMunicipios
              id="cobertura"
              titulo="Mapa de cobertura de carteira por município"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDaCobertura}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={setSelecionado}
              emFoco={emFoco}
              aoPassar={setEmFoco}
            />
            <p className="terr-mapa-foco" aria-live="polite">{textoDoFoco(estadoDaCobertura)}</p>
            <LegendaDoMapa faixas={FAIXAS_DE_COBERTURA[modoDeCobertura]} unidade={UNIDADE_DE_COBERTURA[modoDeCobertura]} />
            <p className="terr-aviso">
              {modoDeCobertura === 'quantidade'
                ? 'Quantidade favorece cidades grandes; alterne para % para comparar.'
                : 'Percentual compara municípios de tamanhos diferentes; uma cidade com 3 vínculos muda de faixa com 1 contato.'}{' '}
              Contato é qualquer interação registrada, inclusive registro gerado pelo sistema: nenhum tipo de atividade está
              marcado como visita (documento 32, P-2).
            </p>
          </div>

          <div className="card cad-cartao terr-mapa">
            <div className="card-title">
              Vendas realizadas <SeloDeClassificacao classificacao={classificacao('vendas')} />
            </div>
            <div className="card-subtitle">
              Faturamento líquido pelo endereço principal do cliente, de {mes(indicadores.competenciaInicial)} a{' '}
              {mes(indicadores.competenciaFinal)}.
            </div>
            <p className="terr-mapa-resumo">
              {reaisCompactos(totais.vendas)} · máquina {reaisCompactos(totais.maquina)} · pós-venda {reaisCompactos(totais.posVenda)} ·{' '}
              {nº(totais.clientesQueCompraram)} clientes compraram
            </p>
            <div className="terr-alternador" role="group" aria-label="Recorte das vendas">
              {(Object.keys(ROTULO_DE_VENDAS) as RecorteDeVendas[]).map((recorte) => (
                <button key={recorte} type="button" aria-pressed={recorteDeVendas === recorte} onClick={() => setRecorteDeVendas(recorte)}>
                  {ROTULO_DE_VENDAS[recorte]}
                </button>
              ))}
            </div>
            <MapaDeMunicipios
              id="vendas"
              titulo="Mapa de vendas por município"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDasVendas}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={setSelecionado}
              emFoco={emFoco}
              aoPassar={setEmFoco}
            />
            <p className="terr-mapa-foco" aria-live="polite">{textoDoFoco(estadoDasVendas)}</p>
            <LegendaDoMapa faixas={FAIXAS_VENDAS} unidade={`R$ no período — ${ROTULO_DE_VENDAS[recorteDeVendas].toLowerCase()}`} />
            <p className="terr-aviso">
              Total = máquina + peça + serviço + outros; pós-venda = peça + serviço, <strong>composição provisória</strong>.
              Valor absoluto favorece cidades grandes. Devolução e cancelamento não são abatidos. Nota sem cliente no CRM não
              tem município e fica na tabela, fora do mapa.
            </p>
          </div>

          <div className="card cad-cartao terr-mapa">
            <div className="card-title">
              Potencial teórico — cultura × área <SeloDeClassificacao classificacao={classificacao('potencial')} />
            </div>
            <div className="card-subtitle">
              {regra
                ? `1 ${regra.modeloDeReferencia} a cada ${regra.hectaresPorMaquina} ha de ${regra.produtoNome} · área plantada do município (IBGE)`
                : 'Sem regra de potencial ativa'}
            </div>
            <p className="terr-mapa-resumo">
              {regra && totais.municipiosComArea > 0
                ? `${nº(Math.round(totais.maquinasTeoricas))} ${regra.modeloDeReferencia} teóricos · ${nº(Math.round(totais.hectares))} ha em ${nº(totais.municipiosComArea)} municípios com área divulgada`
                : 'sem área plantada ou regra para calcular'}
            </p>
            <div className="terr-alternador" role="group" aria-label="O que o mapa mostra">
              {(Object.keys(ROTULO_DO_POTENCIAL) as RecorteDoPotencial[]).map((r) => (
                <button
                  key={r}
                  type="button"
                  aria-pressed={recorteDoPotencial === r}
                  onClick={() => setRecorteDoPotencial(r)}
                >
                  {ROTULO_DO_POTENCIAL[r]}
                </button>
              ))}
            </div>
            <MapaDeMunicipios
              id="potencial"
              titulo="Mapa de potencial teórico por município"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDoPotencial}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={setSelecionado}
              emFoco={emFoco}
              aoPassar={setEmFoco}
            />
            <p className="terr-mapa-foco" aria-live="polite">{textoDoFoco(estadoDoPotencial)}</p>
            <LegendaDoMapa faixas={FAIXAS_DO_POTENCIAL[recorteDoPotencial]} unidade={UNIDADE_DO_POTENCIAL[recorteDoPotencial]} />
            <p className="terr-aviso terr-aviso-alerta">
              {recorteDoPotencial === 'maquinas' ? (
                <>
                  Regra a confirmar. É a área do município inteiro — clientes e não clientes juntos, sem separar: somar
                  "clientes" e "não clientes" a ela contaria a mesma área duas vezes. Clientes:{' '}
                  {nº(indicadores.enderecosComArea)} de {nº(indicadores.enderecos)} endereços com área e cultura. Não
                  considera ciclo de troca, parque instalado, concorrência nem outras culturas.
                </>
              ) : (
                <>
                  Agora o mapa mostra a <strong>lavoura inteira</strong> do município, e não só a cultura da regra — são
                  bases diferentes. O valor da produção é o que o município <strong>colhe</strong>, publicado pelo IBGE em
                  mil reais; não é venda da Tracbel nem preço de máquina. O café entra uma vez só (o "Total" do IBGE, sem
                  somar Arábica e Canephora de novo).
                </>
              )}
            </p>
          </div>

          <div className="card cad-cartao terr-mapa">
            <div className="card-title">Estrutura agropecuária — o que já existe para mecanizar</div>
            <div className="card-subtitle">
              Censo Agropecuário{estruturaDaAdr.anoDoCenso ? ` ${estruturaDaAdr.anoDoCenso}` : ''} (tratores e
              propriedades) · Pesquisa da Pecuária Municipal
              {estruturaDaAdr.anoDoRebanho ? ` ${estruturaDaAdr.anoDoRebanho}` : ''} (rebanho) · ANP (usinas)
            </div>
            <p className="terr-mapa-resumo">
              {nº(totais.tratores)} tratores em {nº(totais.municipiosComTratores)} municípios ·{' '}
              {nº(totais.estabelecimentos)} propriedades · {nº(totais.bovinos)} bovinos ·{' '}
              {nº(totais.usinas)} usinas em {nº(totais.municipiosComUsina)} municípios
            </p>
            <div className="terr-alternador" role="group" aria-label="Recorte da estrutura">
              {(Object.keys(ROTULO_DA_ESTRUTURA) as RecorteDaEstrutura[]).map((r) => (
                <button
                  key={r}
                  type="button"
                  aria-pressed={recorteDaEstrutura === r}
                  onClick={() => setRecorteDaEstrutura(r)}
                >
                  {ROTULO_DA_ESTRUTURA[r]}
                </button>
              ))}
            </div>
            <MapaDeMunicipios
              id="estrutura"
              titulo="Mapa da estrutura agropecuária por município"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDaEstrutura}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={setSelecionado}
              emFoco={emFoco}
              aoPassar={setEmFoco}
            />
            <p className="terr-mapa-foco" aria-live="polite">{textoDoFoco(estadoDaEstrutura)}</p>
            <LegendaDoMapa faixas={FAIXAS_DA_ESTRUTURA[recorteDaEstrutura]} unidade={UNIDADE_DA_ESTRUTURA[recorteDaEstrutura]} />
            <p className="terr-aviso">
              <strong>O Censo Agropecuário é de {estruturaDaAdr.anoDoCenso ?? '2017'}</strong> e o próximo sai em 2028: o
              parque tem essa idade. O rebanho é anual e está em {estruturaDaAdr.anoDoRebanho ?? '—'}. Hachurado é{' '}
              <strong>sigilo do IBGE</strong>, que não é zero — ele oculta o número quando poucos estabelecimentos o
              compõem. As faixas de potência não se somam ao total: o "Total" do IBGE é uma categoria ao lado delas. A ANP
              só enxerga usina de <strong>etanol</strong>: ausência aqui não prova ausência de usina.
            </p>
          </div>
        </div>
      )}

      {escolhido && indicadores && (
        <DetalheDoMunicipio
          municipio={escolhido}
          regras={indicadores.regras}
          culturasNoEstado={indicadores.culturasNoEstado}
          aoFechar={() => setSelecionado(null)}
        />
      )}

      <MetricasSemDado metricas={painel.dados?.metricasSemDado} titulo="O que estes mapas não dizem" />

      {indicadores && (
        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Municípios da ADR e o que ficou fora do mapa</div>
              <div className="card-subtitle">
                A soma das linhas é o total da consulta. Clique numa linha para abrir o detalhe.
              </div>
            </div>
          </div>
          <div className="cad-tabela-wrap terr-tabela-municipios">
            <table className="cad-tabela">
              <caption className="cad-so-leitor">Indicadores por município</caption>
              <thead>
                <tr>
                  <th scope="col">Município</th>
                  <th scope="col">Região · loja</th>
                  <th scope="col">Elegíveis</th>
                  <th scope="col">No prazo</th>
                  <th scope="col">Pendentes</th>
                  <th scope="col">Vendas</th>
                  <th scope="col">Pós-venda <span className="cad-sub">(provisório)</span></th>
                  <th scope="col">{regra ? `${regra.modeloDeReferencia} teóricos` : 'Potencial'}</th>
                </tr>
              </thead>
              <tbody>
                {ordenados.map((m) => (
                  <tr key={m.codigoIbge} aria-current={m.codigoIbge === selecionado ? 'true' : undefined}>
                    <td>
                      <button type="button" className="cad-th-ordenar" onClick={() => setSelecionado(m.codigoIbge)}>
                        {m.nome}
                      </button>
                    </td>
                    <td>
                      {m.regiao}
                      <div className="cad-sub">{m.lojaNome?.replace(/^.*—\s*/, '') ?? '—'}</div>
                    </td>
                    <td className="cad-mono">{nº(m.cobertura.vinculosComCadencia)}</td>
                    <td className="cad-mono">{nº(m.cobertura.cobertos)}</td>
                    <td className="cad-mono">
                      {nº(m.cobertura.pendentes)}
                      {m.cobertura.percentualPendente !== null && (
                        <div className="cad-sub">{m.cobertura.percentualPendente.toLocaleString('pt-BR')}%</div>
                      )}
                    </td>
                    <td className="cad-mono">{reaisCompactos(m.vendas.valorLiquido)}</td>
                    <td className="cad-mono">{reaisCompactos(m.vendas.posVenda)}</td>
                    <td className="cad-mono">{m.potencial[0]?.maquinasTeoricas === null || !m.potencial[0] ? '—' : nº(m.potencial[0].maquinasTeoricas)}</td>
                  </tr>
                ))}
                {territorioNaoCarregado ? (
                  <tr className="terr-linha-total">
                    <td>Total da ADR</td>
                    <td colSpan={7}>território não carregado neste banco — as linhas abaixo são o que a consulta encontrou</td>
                  </tr>
                ) : (
                  <tr className="terr-linha-total">
                    <td>Total da ADR {semFiltro ? '' : '(filtro)'}</td>
                    <td>{daAdr.length} municípios</td>
                    <td className="cad-mono">{nº(totais.elegiveis)}</td>
                    <td className="cad-mono">{nº(totais.cobertos)}</td>
                    <td className="cad-mono">{nº(totais.pendentes)}</td>
                    <td className="cad-mono">{reaisCompactos(totais.vendas)}</td>
                    <td className="cad-mono">{reaisCompactos(totais.posVenda)}</td>
                    <td className="cad-mono">{nº(Math.round(totais.maquinasTeoricas))}</td>
                  </tr>
                )}
                {semFiltro && !territorioNaoCarregado && (
                  <LinhaDeGrupo
                    rotulo="São Paulo fora da ADR"
                    descricao="municípios com cliente fora da ADR"
                    itens={municipios.filter((m) => !m.pertenceAAdr)}
                  />
                )}
                {indicadores.foraDoMapa.map((g) => (
                  <tr key={g.grupo}>
                    <td>
                      {g.grupo}
                      <div className="cad-sub">{g.descricao}</div>
                    </td>
                    <td>—</td>
                    <td className="cad-mono">{nº(g.cobertura.vinculosComCadencia)}</td>
                    <td className="cad-mono">{nº(g.cobertura.cobertos)}</td>
                    <td className="cad-mono">{nº(g.cobertura.pendentes)}</td>
                    <td className="cad-mono">{reaisCompactos(g.vendas.valorLiquido)}</td>
                    <td className="cad-mono">{reaisCompactos(g.vendas.posVenda)}</td>
                    <td>—</td>
                  </tr>
                ))}
                {semFiltro && (
                  <LinhaDeTotal municipios={municipios} foraDoMapa={indicadores.foraDoMapa} />
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <PainelDePrecos />
      <PainelDeCustos />
      <PainelDeCredito />
    </>
  );
}

/** O selo de como ler um indicador — medido, regra provisória, estimativa —, com o motivo no título. */
function SeloDeClassificacao({ classificacao }: { classificacao: ClassificacaoDeIndicador | null }) {
  if (!classificacao) return null;
  return (
    <span className={`terr-selo terr-selo-${classificacao.situacao}`} title={classificacao.motivo}>
      {classificacao.selo}
    </span>
  );
}

/** Um filtro pedido que o dado não sustenta: aparece, desligado, dizendo por quê. */
function FiltroSemDado({ rotulo, opcoes, motivo }: { rotulo: string; opcoes: string; motivo: string }) {
  return (
    <label className="terr-filtro">
      {rotulo}
      <select disabled title={motivo}>
        <option>{opcoes}</option>
      </select>
      <span className="terr-filtro-motivo">sem dado: {motivo}</span>
    </label>
  );
}

/** Uma linha que soma um grupo de municípios. */
function LinhaDeGrupo({ rotulo, descricao, itens }: { rotulo: string; descricao: string; itens: IndicadoresDoMunicipio[] }) {
  const soma = (f: (m: IndicadoresDoMunicipio) => number) => itens.reduce((s, m) => s + f(m), 0);
  return (
    <tr>
      <td>
        {rotulo}
        <div className="cad-sub">
          {descricao} ({itens.length})
        </div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.cobertos))}</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.posVenda))}</td>
      <td>—</td>
    </tr>
  );
}

/** Uma filial para filtrar — pelas lojas que a ADR já mostrou, com o que o filtro alcança. */
function FiltroDeFilial({
  rotulo,
  aplicaA,
  valor,
  lojas,
  aoMudar,
}: {
  rotulo: string;
  aplicaA: string;
  valor: string;
  lojas: Map<string, string>;
  aoMudar: (valor: string) => void;
}) {
  return (
    <label className="terr-filtro">
      {rotulo}
      <select value={valor} onChange={(e) => aoMudar(e.target.value)}>
        <option value="">Todas ao alcance</option>
        {[...lojas.entries()]
          .sort((a, b) => a[1].localeCompare(b[1]))
          .map(([codigo, nome]) => (
            <option key={codigo} value={codigo}>
              {nome}
            </option>
          ))}
      </select>
      <span className="terr-filtro-motivo">aplica-se {aplicaA}</span>
    </label>
  );
}

/**
 * O total da consulta: municípios do mapa mais o que ficou fora dele. É o número
 * que a conferência SQL do documento 32 compara — se não fechar, algo sumiu ou
 * foi contado duas vezes.
 */
function LinhaDeTotal({
  municipios,
  foraDoMapa,
}: {
  municipios: IndicadoresDoMunicipio[];
  foraDoMapa: { cobertura: { vinculosComCadencia: number; cobertos: number; pendentes: number }; vendas: { valorLiquido: number; posVenda: number } }[];
}) {
  const somaMapa = (f: (m: IndicadoresDoMunicipio) => number) => municipios.reduce((s, m) => s + f(m), 0);
  const somaFora = (f: (g: (typeof foraDoMapa)[number]) => number) => foraDoMapa.reduce((s, g) => s + f(g), 0);
  return (
    <tr className="terr-linha-total">
      <td>
        Total da consulta
        <div className="cad-sub">municípios do mapa + tudo o que ficou fora dele</div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.vinculosComCadencia) + somaFora((g) => g.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.cobertos) + somaFora((g) => g.cobertura.cobertos))}</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.pendentes) + somaFora((g) => g.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.valorLiquido) + somaFora((g) => g.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.posVenda) + somaFora((g) => g.vendas.posVenda))}</td>
      <td>—</td>
    </tr>
  );
}
