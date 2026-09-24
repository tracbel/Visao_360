/**
 * Os totais da ADR e a fatia dela em São Paulo (issue 170, parte A).
 *
 * Contas puras, tiradas do corpo da tela para poderem ser lidas — e testadas —
 * sem montar a página inteira. Nenhuma regra mudou ao sair de lá.
 */

import type {
  IndicadoresDoMunicipio,
  IndicadoresForaDoMapa,
  MedidaDoEstado,
  TotaisDoEstado,
} from '../../tipos/territorio';
import { nº } from './indicadoresDaAdr';

export type TotaisDaAdr = {
  elegiveis: number;
  cobertos: number;
  foraDaCadencia: number;
  nuncaContatados: number;
  pendentes: number;
  clientes: number;
  vendas: number;
  maquina: number;
  posVenda: number;
  clientesQueCompraram: number;
  maquinasTeoricas: number;
  hectares: number;
  municipiosComArea: number;
  potencialEstimado: boolean;
  tratores: number;
  municipiosComTratores: number;
  estabelecimentos: number;
  bovinos: number;
  areaKm2: number;
  usinas: number;
  municipiosComUsina: number;
  capacidadeDeEtanol: number;
  lavouraHectares: number;
  lavouraValor: number;
};

export function calcularTotais(daAdr: IndicadoresDoMunicipio[]): TotaisDaAdr {
  const soma = (f: (m: IndicadoresDoMunicipio) => number) => daAdr.reduce((s, m) => s + f(m), 0);
  // O NÚMERO VEM DO MOTOR (issue 72), e não mais de uma divisão feita aqui sobre a primeira regra:
  // `potencialEstrutural` já soma todas as culturas com regra, desconta a terra compartilhada entre
  // elas e soma as categorias de máquina sem somar a terra delas duas vezes.
  const comArea = daAdr.filter((m) => m.potencialEstrutural?.parqueDeMaquinas != null);
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
    maquinasTeoricas: comArea.reduce((s, m) => s + (m.potencialEstrutural?.parqueDeMaquinas ?? 0), 0),
    hectares: comArea.reduce((s, m) => s + (m.potencialEstrutural?.areaUtilHectares ?? 0), 0),
    municipiosComArea: comArea.length,
    // O SELO DE ESTIMATIVA É DO DADO, e não uma frase fixa: ele acende quando alguma regra que
    // dimensionou máquina aqui ainda não foi confirmada pelo comercial (D-P01).
    potencialEstimado: comArea.some((m) => m.potencialEstrutural?.estimativa === true),

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
}

/** Uma linha da conferência — o que a tabela de municípios somava embaixo das linhas. */
export type LinhaDaConferencia = {
  rotulo: string;
  descricao: string | null;
  elegiveis: number;
  cobertos: number;
  pendentes: number;
  vendas: number;
  posVenda: number;
  /** Só a ADR tem parque teórico somado; os grupos de fora ficam sem, e não com zero. */
  maquinas: number | null;
};

export type ConferenciaDaConsulta = {
  /** Nulo quando o território não foi carregado — a explicação, e não uma linha de zeros. */
  adr: LinhaDaConferencia | null;
  /** O que ficou fora da ADR e fora do mapa, grupo a grupo. */
  parcelas: LinhaDaConferencia[];
  /** A soma de tudo — só sem filtro, porque com filtro ela seria de outro recorte. */
  consulta: LinhaDaConferencia | null;
};

/**
 * A CONFERÊNCIA DO DOCUMENTO 32, SEM LINHAS NA TABELA (fidelidade às maquetes,
 * 23/09/2026 — fase 4).
 *
 * "Total da ADR", "São Paulo fora da ADR", os grupos fora do mapa e "Total da
 * consulta" eram linhas no fim da tabela. A tabela ganhou paginação, e uma linha
 * de total embaixo da página 3 afirmaria que dez municípios somam a ADR; a
 * maquete também não as tem. As somas saíram do corpo e foram para a dica do
 * título — e a conta mora aqui, pura, para continuar sendo provada por teste: a
 * ADR mais o que ficou fora dela mais o que ficou fora do mapa é o total da
 * consulta. Se não fechar, algo sumiu ou foi contado duas vezes.
 *
 * As regras de quando cada linha aparece são as da tabela antiga, sem mudança.
 */
export function conferenciaDaConsulta({
  municipios,
  daAdr,
  foraDoMapa,
  totais,
  semFiltro,
  territorioNaoCarregado,
}: {
  municipios: IndicadoresDoMunicipio[];
  daAdr: IndicadoresDoMunicipio[];
  foraDoMapa: IndicadoresForaDoMapa[];
  totais: TotaisDaAdr;
  semFiltro: boolean;
  territorioNaoCarregado: boolean;
}): ConferenciaDaConsulta {
  const somarMunicipios = (rotulo: string, descricao: string | null, itens: IndicadoresDoMunicipio[]): LinhaDaConferencia => {
    const soma = (f: (m: IndicadoresDoMunicipio) => number) => itens.reduce((s, m) => s + f(m), 0);
    return {
      rotulo,
      descricao,
      elegiveis: soma((m) => m.cobertura.vinculosComCadencia),
      cobertos: soma((m) => m.cobertura.cobertos),
      pendentes: soma((m) => m.cobertura.pendentes),
      vendas: soma((m) => m.vendas.valorLiquido),
      posVenda: soma((m) => m.vendas.posVenda),
      maquinas: null,
    };
  };

  const adr: LinhaDaConferencia | null = territorioNaoCarregado
    ? null
    : {
        rotulo: semFiltro ? 'Total da ADR' : 'Total da ADR (filtro)',
        descricao: `${nº(daAdr.length)} municípios`,
        elegiveis: totais.elegiveis,
        cobertos: totais.cobertos,
        pendentes: totais.pendentes,
        vendas: totais.vendas,
        posVenda: totais.posVenda,
        maquinas: Math.round(totais.maquinasTeoricas),
      };

  const foraDaAdr = municipios.filter((m) => !m.pertenceAAdr);
  const parcelas: LinhaDaConferencia[] = [
    ...(semFiltro && !territorioNaoCarregado
      ? [somarMunicipios('São Paulo fora da ADR', `municípios com cliente fora da ADR (${nº(foraDaAdr.length)})`, foraDaAdr)]
      : []),
    ...foraDoMapa.map((g) => ({
      rotulo: g.grupo,
      descricao: g.descricao,
      elegiveis: g.cobertura.vinculosComCadencia,
      cobertos: g.cobertura.cobertos,
      pendentes: g.cobertura.pendentes,
      vendas: g.vendas.valorLiquido,
      posVenda: g.vendas.posVenda,
      maquinas: null,
    })),
  ];

  let consulta: LinhaDaConferencia | null = null;
  if (semFiltro) {
    const mapa = somarMunicipios('Total da consulta', 'municípios do mapa + tudo o que ficou fora dele', municipios);
    const fora = (f: (g: IndicadoresForaDoMapa) => number) => foraDoMapa.reduce((s, g) => s + f(g), 0);
    consulta = {
      ...mapa,
      elegiveis: mapa.elegiveis + fora((g) => g.cobertura.vinculosComCadencia),
      cobertos: mapa.cobertos + fora((g) => g.cobertura.cobertos),
      pendentes: mapa.pendentes + fora((g) => g.cobertura.pendentes),
      vendas: mapa.vendas + fora((g) => g.vendas.valorLiquido),
      posVenda: mapa.posVenda + fora((g) => g.vendas.posVenda),
    };
  }

  return { adr, parcelas, consulta };
}

export type FatiaNoEstado = {
  ano: number;
  area: number | null;
  valor: number | null;
  tratores: number | null;
  estabelecimentos: number | null;
  rebanho: number | null;
};

/**
 * A fatia da região dentro de São Paulo.
 *
 * O DENOMINADOR É O TOTAL PUBLICADO pelo IBGE, e não a soma dos 645 municípios: o valor municipal
 * sigiloso entra no total do estado sem aparecer embaixo, e é o número publicado que a diretoria
 * encontra em qualquer outra fonte.
 */
export function calcularFatiaNoEstado(estado: TotaisDoEstado | null | undefined, totais: TotaisDaAdr): FatiaNoEstado | null {
  if (!estado) return null;
  const parte = (regiao: number, total: number | null) => (total && total > 0 ? (100 * regiao) / total : null);
  return {
    ano: estado.ano,
    area: parte(totais.lavouraHectares, estado.areaPlantadaHectares),
    valor: parte(totais.lavouraValor, estado.valorDaProducaoMilReais),
    tratores: parte(totais.tratores, estado.tratores.publicado),
    estabelecimentos: parte(totais.estabelecimentos, estado.estabelecimentos.publicado),
    rebanho: parte(totais.bovinos, estado.rebanho.publicado),
  };
}

/**
 * QUANTO O SIGILO ESCONDE, em cada medida do Censo e da PPM (issue 155).
 *
 * O denominador é a linha PUBLICADA para São Paulo. A soma dos 645 municípios fica abaixo dela
 * onde o IBGE ocultou a parcela municipal, e dizer isso no cartão evita que quem confira na mão
 * encontre uma diferença sem explicação.
 */
export function diferencaParaASoma(medida: MedidaDoEstado | undefined): string {
  if (!medida?.publicado || medida.somaDosMunicipios === null) return '';
  const abaixo = medida.publicado - medida.somaDosMunicipios;
  return abaixo > 0 ? ` · a soma dos municípios fica ${nº(abaixo)} abaixo do publicado` : '';
}
