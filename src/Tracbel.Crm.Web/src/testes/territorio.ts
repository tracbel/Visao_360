/**
 * Municípios de mentira para os testes do território.
 *
 * O NOME DIZ QUE É DE TESTE, e os números são redondos e inventados de propósito:
 * nenhum valor daqui parece dado da empresa se cair numa captura de tela.
 */

import type { IndicadoresDoMunicipio } from '../tipos/territorio';

export const CAFELANDIA = 3509502;
export const ARARAQUARA = 3503208;

export function municipioDeTeste(
  parcial: Partial<IndicadoresDoMunicipio> & Pick<IndicadoresDoMunicipio, 'codigoIbge' | 'nome'>,
): IndicadoresDoMunicipio {
  return {
    pertenceAAdr: true,
    listadoNaAreaDeAtuacao: true,
    regiao: 'Noroeste',
    lojaCodigo: '010101',
    lojaNome: '010101 — Catanduva',
    lojaAtivaNoCrm: true,
    cobertura: {
      clientes: 12,
      vinculos: 20,
      vinculosComCadencia: 18,
      cobertos: 11,
      foraDaCadencia: 5,
      nuncaContatados: 2,
      semCadencia: 2,
      pendentes: 7,
      percentualPendente: 38.9,
    },
    vendas: {
      clientesQueCompraram: 4,
      valorLiquido: 1_250_000,
      maquina: 900_000,
      peca: 200_000,
      servico: 150_000,
      outros: 0,
      posVenda: 350_000,
    },
    potencial: [],
    // NULO, E NÃO ZERO: o município de teste nasce sem ART carregado, que é o estado de hoje. Zero
    // aqui faria os testes passarem afirmando que a Tracbel não vendeu máquina nenhuma.
    maquinasVendidas: null,
    responsaveisPelasCarteiras: [],
    producao: {
      ano: 2024,
      areaPlantadaHectares: 37_226,
      areaColhidaHectares: 36_000,
      valorDaProducaoMilReais: 468_600,
      culturasComArea: 9,
    },
    estrutura: {
      anoDoCenso: 2017,
      tratores: 422,
      tratoresAbaixoDe100Cv: 300,
      tratoresDe100CvEMais: 122,
      estabelecimentosComTrator: 150,
      estabelecimentos: 253,
      faixasDeArea: [],
      anoDoRebanho: 2024,
      bovinos: 12_400,
      areaKm2: 900,
      usinas: [],
      tratoresPorMilKm2: 468,
      capacidadeDeEtanolM3Dia: null,
    },
    potencialEstrutural: {
      parqueDeMaquinas: 1_240,
      demandaAnualDeMaquinas: null,
      areaUtilHectares: 12_400,
      estimativa: true,
      motivoSemParque: 'Nenhum',
      motivoSemDemanda: 'SemCicloDeRenovacao',
    },
    ...parcial,
  };
}
