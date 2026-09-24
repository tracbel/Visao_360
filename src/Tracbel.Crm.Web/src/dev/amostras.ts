/**
 * AS AMOSTRAS FICTÍCIAS DO HARNESS VISUAL — só em desenvolvimento (fase T4.5).
 *
 * NADA AQUI É DADO DA TRACBEL. Os números são inventados e redondos de propósito,
 * e o harness carimba "AMOSTRA FICTÍCIA" na tela inteira: se uma captura destas
 * vazar para uma apresentação, ela se denuncia sozinha. É a mesma regra do
 * `src/testes/territorio.ts` — e a razão é a de sempre: mock nenhum pode passar
 * por dado real.
 *
 * OS CÓDIGOS E NOMES DE MUNICÍPIO SÃO OS OFICIAIS DO IBGE, lidos da malha de São
 * Paulo que a aplicação já publica. Isso não é dado da empresa — é geografia
 * pública — e é o que faz os quatro mapas desenharem de verdade, que é
 * justamente o que se quer olhar numa conferência visual. Inventar códigos aqui
 * deixaria os mapas vazios e a conferência sem objeto.
 *
 * OS CÓDIGOS DE PRODUTO, ao contrário, são inventados e sequenciais: o catálogo
 * de culturas vem da API, o harness não o serve, e afirmar um código oficial de
 * cabeça seria escrever um número errado num lugar onde ninguém iria conferir.
 */

import type { ColecaoMunicipal } from '../componentes/territorio/projecao';
import { municipioDeTeste } from '../testes/territorio';
import type {
  IndicadoresDoMunicipio,
  PainelTerritorial,
  ParcelaDoParque,
  PotencialTerritorial,
} from '../tipos/territorio';

/** Os estados que o harness sabe montar. A URL escolhe com `?estado=`. */
export type NomeDoEstado =
  | 'completo'
  | 'parcialmenteVazio'
  | 'municipioSelecionado'
  | 'sigiloIbge'
  | 'erro'
  | 'carregando'
  | 'fichaAberta'
  | 'muitasCulturas'
  | 'textosLongos';

/** O que cada estado existe para mostrar — o menu do harness lê daqui. */
export const ESTADOS: readonly { id: NomeDoEstado; titulo: string; oQueProva: string }[] = [
  { id: 'completo', titulo: 'Mercado completo', oQueProva: 'a tela cheia: todo bloco com número e comparação.' },
  {
    id: 'parcialmenteVazio',
    titulo: 'Mercado parcialmente vazio',
    oQueProva: 'metade dos municípios sem potencial — o vazio com motivo não pode parecer carregando.',
  },
  {
    id: 'municipioSelecionado',
    titulo: 'Município selecionado',
    // A FICHA MORA SÓ EM TERRITÓRIO desde 23/09/2026, e a tela abre nessa aba
    // quando a URL já traz o município. O destaque nos quatro mapas continua a
    // um clique, na aba Mercado.
    oQueProva: 'o município no campo do filtro e a ficha aberta ao lado da tabela, em Território.',
  },
  {
    id: 'fichaAberta',
    titulo: 'Ficha com as evidências abertas',
    // A FICHA É EM ABAS desde a fase 4 (maquete): os cinco detalhes abrem
    // expandidos, dentro das abas Lavoura e Estrutura.
    oQueProva: 'a ficha com os cinco detalhes expandidos — eles estão nas abas Lavoura e Estrutura da ficha.',
  },
  { id: 'sigiloIbge', titulo: 'Sigilo do IBGE', oQueProva: 'nulo por sigilo em toda a estrutura: traço e ⓘ, nunca zero.' },
  { id: 'muitasCulturas', titulo: 'Muitas culturas', oQueProva: '18 culturas nas tabelas e na composição do fator.' },
  {
    id: 'textosLongos',
    titulo: 'Textos longos',
    oQueProva: 'nome de município, loja e razão social longos — onde a quebra de linha falha.',
  },
  { id: 'carregando', titulo: 'Carregando', oQueProva: 'a primeira carga, antes de qualquer número.' },
  { id: 'erro', titulo: 'Erro', oQueProva: 'a API recusou: o bloco de erro com o botão de tentar de novo.' },
];

/** Um nome absurdamente longo, para achar a quebra de linha antes que o usuário ache. */
const NOME_LONGO = 'Santa Rita do Passa Quatro dos Campos Gerais de Cima';

const PROCEDENCIA_DA_PAM = {
  fonte: 'IBGE/SIDRA',
  pesquisa: 'PAM — Produção Agrícola Municipal',
  tabela: '5457',
  variavel: 'Área plantada',
  competencia: '2024',
  ultimaCargaUtc: '2026-09-23T03:00:00Z',
  ressalva: null,
};

const PROCEDENCIA_DO_CENSO = {
  ...PROCEDENCIA_DA_PAM,
  pesquisa: 'Censo Agropecuário',
  tabela: '6779',
  variavel: 'Tratores',
  competencia: '2017',
  ressalva: 'O Censo é de 2017 e só sai de novo em 2028.',
};

/** As culturas fictícias. O código é inventado — ver o cabeçalho deste arquivo. */
const CULTURAS = [
  'Cana-de-açúcar', 'Café (Total)', 'Soja', 'Milho', 'Laranja', 'Amendoim',
  'Feijão', 'Algodão', 'Trigo', 'Mandioca', 'Sorgo', 'Girassol',
  'Tomate', 'Batata', 'Banana', 'Uva', 'Limão', 'Manga',
] as const;

const codigoDaCultura = (indice: number) => 900_001 + indice;

/** A fatia de cada cultura na área plantada do município — decrescente, e a soma não passa de 100%. */
const PESO_DA_CULTURA = [0.62, 0.15, 0.12, 0.04, 0.02, 0.01];

function potencialDaCultura(indice: number, area: number): PotencialTerritorial {
  return {
    produtoCodigoIbge: codigoDaCultura(indice),
    areaPlantadaHectares: area,
    maquinasTeoricas: Math.round(area / 250),
    areaColhidaHectares: Math.round(area * 0.96),
    valorDaProducaoMilReais: Math.round(area * 12.5),
    ano: 2024,
    quantidadeProduzida: Math.round(area * 4.2),
    unidadeDaQuantidade: 'Toneladas',
    produtividade: 4.4,
    unidadeDaProdutividade: 't/ha',
  };
}

function parcelaDoParque(indice: number, area: number): ParcelaDoParque {
  return {
    culturaCodigo: CULTURAS[indice].toUpperCase().replace(/[^A-Z]/g, ''),
    cultura: CULTURAS[indice],
    compartilhada: [],
    areaUtilHectares: area,
    parque: Math.round(area / 250),
    demandaAnual: Math.round(area / 250 / 8),
    motivo: 'Nenhum',
  };
}

/**
 * Os municípios da amostra, tirados da malha real de São Paulo.
 *
 * UMA ÁREA CONTÍGUA, COMO A ADR DE VERDADE (fidelidade às maquetes, fase 2): os
 * `quantos` municípios mais próximos do primeiro da malha (Adamantina, o da ficha
 * do harness), pelo centro de cada um. Era um de cada 21 — trinta pontos
 * espalhados pelo estado —, e com a ADR assim o mapa enquadrado na ADR era o
 * estado inteiro com manchas: não havia como conferir o enquadramento da maquete,
 * que mostra a área de atuação enchendo o quadro.
 *
 * CONTINUA DETERMINÍSTICA, que é o que torna a captura comparável: a mesma
 * distância dá sempre o mesmo conjunto, no mesmo lugar do mapa. Os NÚMEROS não
 * mudaram — eles dependem da posição na lista, não do município.
 *
 * Exportada para as percepções fictícias do Momento (`amostrasDeMercado.ts`)
 * caírem nos MESMOS municípios que estão na Região Tracbel da amostra.
 */
export function municipiosDaMalha(malha: ColecaoMunicipal, quantos: number) {
  const centro = (f: ColecaoMunicipal['features'][number]): [number, number] => {
    const anel = f.geometry.type === 'Polygon' ? f.geometry.coordinates[0] : f.geometry.coordinates[0][0];
    const [x, y] = anel.reduce<[number, number]>(([sx, sy], [lon, lat]) => [sx + lon, sy + lat], [0, 0]);
    return [x / anel.length, y / anel.length];
  };
  const [x0, y0] = centro(malha.features[0]);

  return malha.features
    .map((f) => {
      const [x, y] = centro(f);
      return { f, distancia: (x - x0) ** 2 + (y - y0) ** 2 };
    })
    .sort((a, b) => a.distancia - b.distancia)
    .slice(0, quantos)
    .map(({ f }) => ({ codigo: Number(f.properties.codarea), nome: f.properties.nome }));
}

function montarMunicipio(
  base: { codigo: number; nome: string },
  posicao: number,
  estado: NomeDoEstado,
): IndicadoresDoMunicipio {
  const semDado = estado === 'parcialmenteVazio' && posicao % 2 === 1;
  const sobSigilo = estado === 'sigiloIbge';
  const quantasCulturas = estado === 'muitasCulturas' ? CULTURAS.length : 3;
  const area = 8_000 + posicao * 1_400;

  return municipioDeTeste({
    codigoIbge: base.codigo,
    nome: estado === 'textosLongos' && posicao === 0 ? NOME_LONGO : base.nome,
    regiao: posicao % 3 === 0 ? 'Norte' : 'Noroeste',
    lojaCodigo: posicao % 3 === 0 ? '010102' : '010101',
    lojaNome:
      estado === 'textosLongos'
        ? '010101 — Catanduva, Ribeirão Preto e Região Noroeste do Estado'
        : posicao % 3 === 0
          ? '010102 — Ribeirão Preto'
          : '010101 — Catanduva',
    vendas: {
      clientesQueCompraram: semDado ? 0 : 3 + (posicao % 7),
      valorLiquido: semDado ? 0 : 400_000 + posicao * 155_000,
      maquina: semDado ? 0 : 300_000 + posicao * 100_000,
      peca: semDado ? 0 : 60_000 + posicao * 30_000,
      servico: semDado ? 0 : 40_000 + posicao * 25_000,
      outros: 0,
      posVenda: semDado ? 0 : 100_000 + posicao * 55_000,
    },
    potencial: semDado
      ? []
      : Array.from({ length: quantasCulturas }, (_, i) =>
          // AS CULTURAS CABEM NA ÁREA PLANTADA DO MUNICÍPIO (fase 4): eram
          // `area / (i + 1)`, e as três primeiras somavam 183% do total — a
          // tabela "Lavoura (área plantada)" da ficha mostrava participações
          // impossíveis. Os pesos somam menos de 100%, e o resto vira "Outros".
          potencialDaCultura(i, Math.round(area * (PESO_DA_CULTURA[i] ?? 0.002))),
        ),
    producao: semDado
      ? null
      : {
          ano: 2024,
          areaPlantadaHectares: area,
          areaColhidaHectares: Math.round(area * 0.96),
          valorDaProducaoMilReais: Math.round(area * 12.5),
          culturasComArea: quantasCulturas,
        },
    estrutura: {
      anoDoCenso: 2017,
      // SIGILO É NULO, NUNCA ZERO: o IBGE omite o valor de um município onde
      // poucos produtores o identificariam. Zero diria que não há trator ali.
      tratores: sobSigilo ? null : 180 + posicao * 37,
      tratoresAbaixoDe100Cv: sobSigilo ? null : 120 + posicao * 20,
      tratoresDe100CvEMais: sobSigilo ? null : 60 + posicao * 17,
      estabelecimentosComTrator: sobSigilo ? null : 90 + posicao * 12,
      estabelecimentos: sobSigilo ? null : 210 + posicao * 18,
      faixasDeArea: [
        { ordem: 1, rotulo: 'Até 50 ha', estabelecimentos: sobSigilo ? null : 120 + posicao },
        { ordem: 2, rotulo: 'De 50 a 200 ha', estabelecimentos: sobSigilo ? null : 60 + posicao },
        { ordem: 3, rotulo: 'Acima de 200 ha', estabelecimentos: sobSigilo ? null : 30 + posicao },
      ],
      anoDoRebanho: 2024,
      bovinos: sobSigilo ? null : 9_000 + posicao * 1_100,
      areaKm2: 700 + posicao * 45,
      usinas:
        posicao % 4 === 0
          ? [
              {
                razaoSocial:
                  estado === 'textosLongos'
                    ? 'Usina Agroindustrial Cooperativa de Produtores de Cana do Noroeste Paulista S.A.'
                    : 'Usina Fictícia do Noroeste S.A.',
                capacidadeM3Dia: 1_200 + posicao * 50,
              },
            ]
          : [],
      tratoresPorMilKm2: sobSigilo ? null : 250 + posicao * 10,
      capacidadeDeEtanolM3Dia: posicao % 4 === 0 ? 1_200 + posicao * 50 : null,
    },
    // AS UNIDADES DO ART (issue 69). NO ESTADO PARCIALMENTE VAZIO O ART NÃO TROUXE NADA: é o caminho
    // do vazio com motivo, que a conferência visual precisa provar tanto quanto o caminho cheio.
    maquinasVendidas: estado === 'parcialmenteVazio' ? null : Math.round((area / 250 / 8) * 0.15),
    potencialEstrutural: semDado
      ? { parqueDeMaquinas: null, demandaAnualDeMaquinas: null, areaUtilHectares: null, estimativa: true, motivoSemParque: 'SemArea', motivoSemDemanda: 'SemArea' }
      : {
          parqueDeMaquinas: Math.round(area / 250),
          demandaAnualDeMaquinas: Math.round(area / 250 / 8),
          areaUtilHectares: area,
          estimativa: true,
          motivoSemParque: 'Nenhum',
          motivoSemDemanda: 'Nenhum',
        },
  });
}

/** O painel inteiro, fictício, no estado pedido. */
export function painelFicticio(malha: ColecaoMunicipal, estado: NomeDoEstado): PainelTerritorial {
  const bases = municipiosDaMalha(malha, 30);
  const municipios = bases.map((b, i) => montarMunicipio(b, i, estado));
  const quantasCulturas = estado === 'muitasCulturas' ? CULTURAS.length : 3;

  const areaTotal = municipios.reduce((s, m) => s + (m.producao?.areaPlantadaHectares ?? 0), 0);
  const demandaTotal = municipios.reduce((s, m) => s + (m.potencialEstrutural?.demandaAnualDeMaquinas ?? 0), 0);

  const porCultura = Array.from({ length: quantasCulturas }, (_, i) =>
    parcelaDoParque(i, Math.round(areaTotal / (i + 1) / quantasCulturas)),
  );

  return {
    indicadores: {
      competenciaInicial: '2025-10',
      competenciaFinal: '2026-09',
      referenciaDaCobertura: '2026-09-23T00:00:00Z',
      interacaoMaisRecente: '2026-09-21T00:00:00Z',
      anoDaAreaPlantada: 2024,
      // UMA REGRA POR CULTURA, e não só a primeira (corrigido na T4.7): a ficha
      // do município resolve o NOME da cultura pela regra, e com uma regra só as
      // demais apareciam na captura como "Área plantada de 900002" — o código
      // inventado do harness, no lugar onde se lê o nome. Amostra que não dá para
      // revisar não serve para revisão visual.
      regras: Array.from({ length: quantasCulturas }, (_, i) => ({
        produtoCodigoIbge: codigoDaCultura(i),
        produtoNome: CULTURAS[i],
        hectaresPorMaquina: 250,
        modeloDeReferencia: 'Trator de referência (amostra)',
        situacao: 'AConfirmar' as const,
        justificativa: 'regra de amostra do harness — não é decisão do comercial',
        vigenteDesde: '2026-01-01',
        anosDeRenovacao: 8,
      })),
      municipios,
      foraDoMapa: [],
      enderecos: 480,
      enderecosComArea: 0,
      visao: 'Filial',
      estado: {
        ano: 2024,
        areaPlantadaHectares: 8_000_000,
        valorDaProducaoMilReais: 120_000_000,
        areaColhidaHectares: 7_800_000,
        tratores: { publicado: 190_000, somaDosMunicipios: 188_400 },
        estabelecimentos: { publicado: 180_000, somaDosMunicipios: 179_000 },
        anoDoCenso: 2017,
        rebanho: { publicado: 10_000_000, somaDosMunicipios: 9_900_000 },
        anoDoRebanho: 2024,
      },
      culturasNoEstado: Array.from({ length: quantasCulturas }, (_, i) => ({
        produtoCodigoIbge: codigoDaCultura(i),
        produtoNome: CULTURAS[i],
        ano: 2024,
        areaPlantadaHectares: 900_000 / (i + 1),
        areaColhidaHectares: 880_000 / (i + 1),
        quantidadeProduzida: 3_800_000 / (i + 1),
        unidadeDaQuantidade: 'Toneladas',
        valorDaProducaoMilReais: 11_000_000 / (i + 1),
        produtividade: 4.3,
        unidadeDaProdutividade: 't/ha',
      })),
      potencialDoRecorte: {
        parqueDeMaquinas: Math.round(areaTotal / 250),
        demandaAnualDeMaquinas: demandaTotal,
        areaUtilHectares: areaTotal,
        estimativa: true,
        motivoSemParque: 'Nenhum',
        motivoSemDemanda: 'Nenhum',
        frase: 'estimativa: a regra de amostra não foi confirmada pelo comercial',
        municipiosComParque: municipios.filter((m) => m.potencialEstrutural?.parqueDeMaquinas).length,
        porCultura,
        porCategoria: [],
        relevanciaNoEstado: {
          fatiaDaAreaPlantada: 3.9,
          fatiaDaAreaColhida: 3.8,
          fatiaDaQuantidade: 4.1,
          fatiaDoValor: 4.4,
          produtividadeDoRecorte: 4.4,
          produtividadeNoEstado: 4.3,
          razaoDeProdutividade: 1.02,
        },
        relevanciaPorCultura: [],
      },
      regiaoTracbel: {
        anoDaLavoura: 2024,
        areaPlantadaHectares: areaTotal,
        areaColhidaHectares: Math.round(areaTotal * 0.96),
        valorDaProducaoMilReais: Math.round(areaTotal * 12.5),
        anoDoCenso: 2017,
        tratores: 7_400,
        estabelecimentos: 6_100,
        anoDoRebanho: 2024,
        bovinos: 310_000,
        municipios: municipios.length,
      },
      procedencias: {
        areaPlantada: PROCEDENCIA_DA_PAM,
        valorDaProducao: { ...PROCEDENCIA_DA_PAM, variavel: 'Valor da produção' },
        tratores: PROCEDENCIA_DO_CENSO,
        estabelecimentos: { ...PROCEDENCIA_DO_CENSO, variavel: 'Estabelecimentos agropecuários' },
        rebanho: { ...PROCEDENCIA_DO_CENSO, pesquisa: 'PPM', tabela: '3939', variavel: 'Efetivo bovino', competencia: '2024' },
        usinas: null,
        // O ART NÃO TROUXE VENDA NA AMOSTRA, que é o estado de hoje: sem carimbo de captura.
        maquinasVendidas: null,
      },
      // O MOMENTO COM A COMPOSIÇÃO FECHADA (T3.1): Σ ajustada ÷ Σ estrutural.
      momento: (() => {
        const culturas = porCultura.slice(0, quantasCulturas).map((p, i) => {
          const indice = [0.8, 1.3, 1.0][i % 3];
          const fator = [0.84, 1.08, 1.0][i % 3];
          const estrutural = p.demandaAnual ?? 0;
          return {
            culturaCodigo: p.culturaCodigo,
            cultura: p.cultura,
            demandaEstrutural: estrutural,
            areaUtilHectares: p.areaUtilHectares,
            indiceDePreco: indice,
            fator: {
              fator,
              parcelaDePreco: Number(((indice - 1) * 0.4).toFixed(4)),
              parcelaDaPercepcao: 0.02,
              parcelaDeCredito: -0.06,
              fatorSemLimite: fator,
              cortadoPeloLimite: false,
              indicadoresUsados: 3,
              estimativa: true,
              motivo: 'Nenhum',
            },
            demandaAjustada: Math.round(estrutural * fator),
          };
        });

        const estrutural = culturas.reduce((s, c) => s + (c.demandaEstrutural ?? 0), 0);
        const ajustada = culturas.reduce((s, c) => s + (c.demandaAjustada ?? 0), 0);
        const agregado = estrutural > 0 ? Number((ajustada / estrutural).toFixed(4)) : null;

        return {
          fatorAgregado: agregado,
          motivoSemFator: agregado === null ? 'SemDemandaEstrutural' : 'Nenhum',
          demandaEstruturalTotal: estrutural > 0 ? estrutural : null,
          demandaAjustadaTotal: estrutural > 0 ? ajustada : null,
          porCultura: culturas,
          predominante: {
            cultura: culturas[0]?.cultura ?? 'Cana-de-açúcar',
            fatia: 58,
            criterio: 'maior área útil entre as culturas com regra de potencial',
          },
          indiceDeCredito: 0.92,
          percepcaoPercentual: 2,
          // O porte segue SEM NOME: as bandas da issue 166 não foram decididas,
          // e o harness não pode inventar a decisão que a tela diz faltar.
          porte: null,
          faixaDoMomento: agregado === null ? null : agregado < 1 ? 'Retraído' : 'Normal',
          leitura: agregado === null ? '' : agregado < 1 ? 'Mercado retraído.' : 'Mercado normal.',
          procedencia: {
            fonte: 'CRM Tracbel',
            pesquisa: 'Fator de ciclo de mercado (issue 74), agregado pela demanda',
            tabela: null,
            variavel: 'Demanda ajustada total ÷ demanda estrutural total',
            competencia: 'amostra do harness',
            ultimaCargaUtc: '2026-09-23T12:00:00Z',
            ressalva: 'AMOSTRA FICTÍCIA — nenhum número desta tela é dado da Tracbel.',
          },
        };
      })(),
      // AS VENDAS EM UNIDADES DO RECORTE (issue 69). A soma dos municípios, para a captura do harness
      // bater com a quebra que ele mesmo desenha; nula no estado parcialmente vazio.
      maquinasVendidas:
        estado === 'parcialmenteVazio'
          ? null
          : {
              criterioDeData: 'Faturamento',
              fraseDoCriterio:
                'Contadas pela DATA DO FATURAMENTO (D-P08.1): a view do ART é de máquina faturada, e máquina ' +
                'faturada é máquina vendida. AMOSTRA FICTÍCIA.',
              unidades: municipios.reduce((s, m) => s + (m.maquinasVendidas ?? 0), 0),
              porCategoria: [
                { categoriaCodigo: 'TRATOR', categoriaNome: 'Trator', unidades: 118 },
                { categoriaCodigo: 'COLHEITADEIRA', categoriaNome: 'Colheitadeira', unidades: 24 },
                { categoriaCodigo: 'PLANTADEIRA', categoriaNome: 'Plantadeira', unidades: 17 },
                { categoriaCodigo: 'PULVERIZADOR', categoriaNome: 'Pulverizador', unidades: 9 },
              ],
              unidadesForaDoMapa: 6,
              unidadesSemClassificacao: 3,
              unidadesEmLinhaSemCategoria: 11,
              vendasSemAData: 2,
              vendaMaisRecente: '2026-08-29',
              carregadoAte: '2026-09-14T03:00:00Z',
            },
    },
    metricasSemDado: [
      {
        metrica: 'participacaoDeMercado',
        motivo:
          estado === 'textosLongos'
            ? 'O emplacamento por município não está integrado, e sem ele não existe participação de mercado: a ' +
              'demanda estimada pelo motor é denominador de CAPTURA, e não de share — share exigiria o total ' +
              'vendido por todos os fabricantes, que nenhuma fonte aberta publica com recorte municipal.'
            : 'emplacamento não integrado',
      },
    ],
    podeVerEmpresaInteira: false,
    // AS CINCO CLASSIFICAÇÕES QUE A API DEVOLVE (`ConsultasDeTerritorio.Classificar`),
    // com o selo que ela escreve: faltavam a cobertura e o pós-venda, e sem
    // elas o selo "Regra provisória" do mapa de cobertura (maquete) não tinha
    // como aparecer no harness.
    classificacoes: [
      { indicador: 'potencial', situacao: 'Estimativa', selo: 'estimativa', motivo: 'regra de amostra a confirmar' },
      { indicador: 'vendas', situacao: 'Medido', selo: 'medido', motivo: 'notas fiscais de saída (amostra)' },
      {
        indicador: 'coberturaDeVisita',
        situacao: 'RegraComercialProvisoria',
        selo: 'Regra provisória',
        motivo: 'o que conta como visita e a periodicidade aguardam decisão do comercial (amostra)',
      },
      {
        indicador: 'posVenda',
        situacao: 'RegraComercialProvisoria',
        selo: 'Composição provisória',
        motivo: 'o que a diretoria considera pós-venda ainda não foi definido (amostra)',
      },
    ],
    // A AMOSTRA REPRODUZ O ESTADO DE HOJE: a demanda sai, e os outros três dizem o que falta. O harness
    // existe para mostrar a tela como ela é, e a tela de hoje tem três travessões no topo.
    numerosDeDecisao: {
      demandaAnual: { valor: demandaTotal, motivo: 'Nenhum', frase: '' },
      mercadoAnual: {
        valor: null,
        motivo: 'SemPrecoDeMaquina',
        frase: 'AMOSTRA FICTÍCIA — não há preço de referência de máquina no CRM (issue 70).',
        parcial: false,
        categoriasSemPreco: [],
      },
      capturaPercentual: {
        valor: null,
        motivo: 'SemVendasEmUnidades',
        frase: 'AMOSTRA FICTÍCIA — as vendas em máquinas não estão carregadas (issue 69, D-P08).',
      },
      oportunidade: {
        valor: null,
        motivo: 'SemVendasEmUnidades',
        frase: 'AMOSTRA FICTÍCIA — as vendas em máquinas não estão carregadas (issue 69, D-P08).',
      },
    },
  };
}
