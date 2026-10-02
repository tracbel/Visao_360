/**
 * AS AMOSTRAS DOS QUATRO PAINÉIS DE MERCADO — preço, custo, rentabilidade e
 * crédito (fase T4.7, só em desenvolvimento).
 *
 * POR QUE ELAS PRECISARAM EXISTIR. A T4.5 fez o harness responder VAZIO para
 * estas quatro rotas, e escreveu que enchê-las era passo seguinte. O passo
 * chegou pelo pior caminho: pediram a revisão visual de Rentabilidade e Crédito,
 * e não havia como olhar nenhum dos dois — os painéis abriam dizendo "a carga
 * não rodou". Um harness que não mostra o painel não serve para revisar o
 * painel.
 *
 * NADA AQUI É DADO DA TRACBEL, pela mesma regra do `amostras.ts`: os números são
 * inventados, redondos e plausíveis só o bastante para o desenho ser julgável. A
 * barra do topo carimba AMOSTRA FICTÍCIA em toda captura.
 *
 * OS NÚMEROS SÃO COERENTES ENTRE SI de propósito — receita menos custo dá a
 * margem, as janelas de crédito somam o total, a série de preço sobe e desce sem
 * saltos. Amostra incoerente faz o revisor discutir o dado em vez do desenho.
 */

import type {
  ClasseDePrioridade,
  DemandaEPrevisaoDaRegiao,
  DiagnosticoComercialDaRegiao,
  MediaPlurianual,
  PainelDeCreditoRural,
  PrecoImplicitoDoRecorte,
  PrecoImplicitoNoAno,
  PrecosDeMercado,
  RentabilidadeDaCultura,
  SerieDeCusto,
} from '../tipos/mercado';
import type { CatalogoDoMercado, ParametrosDoPotencialVigentes } from '../tipos/potencial';
import { CENS } from './amostrasDaVisao360';

/** As culturas que aparecem nos quatro painéis, na mesma ordem. */
const CULTURAS = [
  { codigo: 'CANA', nome: 'Cana-de-açúcar', unidade: 'tonelada' },
  { codigo: 'CAFE', nome: 'Café (Total)', unidade: 'saca de 60 kg' },
  { codigo: 'SOJA', nome: 'Soja', unidade: 'saca de 60 kg' },
  { codigo: 'MILHO', nome: 'Milho', unidade: 'saca de 60 kg' },
  { codigo: 'LARANJA', nome: 'Laranja', unidade: 'caixa de 40,8 kg' },
] as const;

const PROCEDENCIA = {
  fonte: 'HARNESS VISUAL',
  pesquisa: 'amostra fictícia — nada aqui veio de fonte nenhuma',
  tabela: null,
  variavel: null,
  competencia: 'amostra',
  ultimaCargaUtc: '2026-09-23T12:00:00Z',
  ressalva: 'AMOSTRA FICTÍCIA. Nenhum número deste painel é dado da Tracbel nem de fonte pública.',
};

/** Uma onda suave e determinística — série que sobe e desce sem saltar. */
const onda = (i: number, base: number, amplitude: number) =>
  Number((base + amplitude * Math.sin(i / 5) + amplitude * 0.3 * Math.sin(i / 2.3)).toFixed(2));

/** `aaaa-mm-01`, contando para trás a partir de setembro de 2026. */
function mesDe(indiceDoFim: number): string {
  const d = new Date(Date.UTC(2026, 8, 1));
  d.setUTCMonth(d.getUTCMonth() - indiceDoFim);
  return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, '0')}-01`;
}

/** Preço por kg, 60 meses, uma série por cultura. */
export function precosFicticios(): PrecosDeMercado {
  const basePorCultura = [0.14, 24.5, 2.4, 1.15, 1.9];

  return {
    primeiroMesDoDolar: mesDe(59),
    ultimoMesDoDolar: mesDe(0),
    series: CULTURAS.map((c, i) => ({
      fonte: 'HARNESS',
      codigoNaFonte: `AMOSTRA-${c.codigo}`,
      nivel: 'Produtor',
      produto: c.nome,
      classificacao: 'amostra fictícia',
      unidade: 'R$/kg',
      unidadeComercial: c.unidade,
      fatorComercial: c.unidade.includes('60') ? 60 : c.unidade.includes('40,8') ? 40.8 : 1000,
      meses: Array.from({ length: 60 }, (_, m) => {
        const valor = onda(m, basePorCultura[i], basePorCultura[i] * 0.18);
        return { mes: mesDe(59 - m), valorEmReais: valor, valorEmDolares: Number((valor / 5.4).toFixed(4)) };
      }),
      procedencia: PROCEDENCIA,
    })),
    // O TRATOR BASE (issue 70): a mediana fictícia dos tratores vendidos, só nos últimos 20 meses — a série do trator
    // começa depois da da safra, como em produção (a primeira venda do ART casada com a nota), e há mês sem venda.
    maquinas: [
      {
        categoriaCodigo: 'TRATOR',
        categoriaNome: 'Trator',
        meses: Array.from({ length: 20 }, (_, m) => m)
          .filter((m) => m % 7 !== 3)
          .map((m) => {
            const mediana = Math.round(onda(m, 520_000, 60_000) / 1_000) * 1_000;
            return { mes: mesDe(19 - m), mediana, menor: mediana - 80_000, maior: mediana + 150_000, notas: 3 + (m % 5) };
          }),
        procedencia: PROCEDENCIA,
      },
    ],
  };
}

/**
 * O PREÇO RECEBIDO PELO PRODUTOR, DA PAM (issue 198) — a segunda série de preço,
 * anual, que abre ao lado da CONAB em "Ver séries de preço e custo".
 *
 * PREÇO = VALOR DA PRODUÇÃO × 1000 ÷ QUANTIDADE em toda linha, como na API. E com
 * um buraco de propósito: a laranja não tem 2022, então as duas médias dela não
 * fecham e o painel diz qual ano faltou. Um harness só com médias completas
 * esconderia o estado que mais importa revisar nesse painel.
 */
export function precoImplicitoFicticio(): PrecoImplicitoDoRecorte {
  const porTonelada = [140, 24_500, 2_400, 1_150, 1_900];
  const anos = Array.from({ length: 12 }, (_, k) => 2024 - k);

  return {
    ressalva:
      'AMOSTRA FICTÍCIA. Nenhum número deste painel é dado da Tracbel nem do IBGE — a série de verdade é o valor ' +
      'da produção dividido pela quantidade produzida, da Produção Agrícola Municipal.',
    series: CULTURAS.map((c, i) => {
      const quantidade = 10_000 * (i + 2);
      const noAno: PrecoImplicitoNoAno[] = anos.map((ano, k) => {
        const buraco = c.codigo === 'LARANJA' && ano === 2022;
        const preco = onda(k, porTonelada[i], porTonelada[i] * 0.12);
        return {
          ano,
          precoPorUnidade: buraco ? null : preco,
          unidade: 'toneladas',
          valorDaProducaoMilReais: buraco ? null : Number(((preco * quantidade) / 1000).toFixed(3)),
          quantidadeProduzida: quantidade,
          motivo: buraco ? 'SemValorDaProducao' : 'Nenhum',
        };
      });

      // A MÉDIA SÓ SAI COM A JANELA INTEIRA, a mesma regra da API.
      const media = (n: number): MediaPlurianual => {
        const janela = noAno.slice(0, n);
        const faltando = janela.filter((a) => a.precoPorUnidade === null).map((a) => a.ano);
        const soma = janela.reduce((s, a) => s + (a.precoPorUnidade ?? 0), 0);
        return { anos: n, preco: faltando.length > 0 ? null : Number((soma / n).toFixed(2)), anosFaltando: faltando };
      };

      return {
        produtoCodigoIbge: 900_001 + i,
        produto: c.nome,
        unidade: 'toneladas',
        anos: noAno,
        mediaDeTresAnos: media(3),
        mediaDeCincoAnos: media(5),
        municipiosComDadoNoUltimoAno: 30 - i * 4,
      };
    }),
  };
}

/**
 * A rentabilidade por cultura.
 *
 * MARGEM = RECEITA − CUSTO, e os três números fecham em toda linha: é o que
 * permite revisar o desenho sem desconfiar da conta. A laranja sai NEGATIVA de
 * propósito — um painel que só mostra margem positiva esconde como ele se
 * comporta quando o número é ruim, que é justamente quando alguém olha.
 */
export function rentabilidadeFicticia(): RentabilidadeDaCultura[] {
  // O PREÇO É POR QUILO, o mesmo das séries fictícias acima (fidelidade às
  // maquetes, fase 3). Estava dividido de novo pela saca — R$ 0,40 o quilo de
  // café —, e quatro das cinco culturas saíam com margem de −2.000%: o gráfico
  // de receita, custo e margem não era revisável. A laranja continua negativa
  // de propósito, agora pelo custo.
  const linhas: { produtividade: number; preco: number; custo: number }[] = [
    { produtividade: 82_000, preco: 0.14, custo: 8_900 },
    { produtividade: 2_100, preco: 24.5, custo: 21_400 },
    { produtividade: 3_600, preco: 2.4, custo: 4_050 },
    { produtividade: 6_400, preco: 1.15, custo: 4_800 },
    { produtividade: 28_000, preco: 1.9, custo: 55_000 },
  ];

  return CULTURAS.map((c, i) => {
    const { produtividade, preco, custo } = linhas[i];
    const receita = Number((produtividade * preco).toFixed(2));
    const margem = Number((receita - custo).toFixed(2));
    const area = [283_000, 141_500, 94_333, 70_750, 56_600][i];

    return {
      culturaCodigo: c.codigo,
      culturaNome: c.nome,
      unidadeComercial: c.unidade,
      anoDaProdutividade: 2024,
      produtividadeKgPorHa: produtividade,
      precoMedioPorKg: Number(preco.toFixed(5)),
      mesesDePrecoNaMedia: 12,
      receitaPorHectare: receita,
      localDoCusto: 'Localidade de referência (amostra)',
      camadaDoCusto: 'Operacional',
      safraDoCusto: 2025,
      custoPorHectare: custo,
      margemPorHectare: margem,
      margemPorUnidade: Number(
        (margem / (produtividade / (c.unidade.includes('60') ? 60 : c.unidade.includes('40,8') ? 40.8 : 1000))).toFixed(2),
      ),
      areaColhidaHectares: area,
      margemTotal: Number((margem * area).toFixed(0)),
      motivo: 'Nenhum',
      fraseDoMotivo: '',
      // A TENDÊNCIA E A SAFRA ANTERIOR (28/09/2026): o ano anterior pela PAM, com margens um pouco diferentes, e o
      // custo da safra anterior 4% a 9% abaixo — números da amostra, sem nada da Tracbel.
      tendencia: {
        ano: 2024,
        anoAnterior: 2023,
        margemPorHectare: margem,
        margemPorHectareAnterior: Number((margem * [0.88, 0.93, 1.1, 0.97, 1.2][i]).toFixed(2)),
        safraDoCusto: 2024,
        custoPorHectare: custo,
        safraDoCustoAnterior: 2023,
        custoPorHectareAnterior: Number((custo * 0.95).toFixed(2)),
        variacaoDaMargem: null,
      },
      safraAnteriorDoCusto: 2024,
      custoPorHectareDaSafraAnterior: Number((custo / [1.04, 1.09, 1.06, 0.98, 1.07][i]).toFixed(2)),
      variacaoDoCusto: [0.04, 0.09, 0.06, -0.02, 0.07][i],
    };
  });
}

/** O custo por safra, quatro safras por cultura. */
export function custosFicticios(): SerieDeCusto[] {
  return CULTURAS.slice(0, 4).map((c, i) => {
    const base = [8_900, 21_400, 4_050, 4_800][i];

    return {
      cultura: c.nome,
      local: 'Localidade de referência (amostra)',
      variante: null,
      codigoIbge: null,
      unidadeComercial: c.unidade,
      safras: Array.from({ length: 4 }, (_, s) => {
        const safra = 2022 + s;
        const total = Number((base * (0.88 + s * 0.045)).toFixed(2));
        const variavel = Number((total * 0.62).toFixed(2));
        const fixo = Number((total * 0.18).toFixed(2));
        const operacional = Number((variavel + fixo).toFixed(2));

        return {
          aba: `${c.nome} — amostra`,
          safra,
          mesDoRelatorio: 6,
          produtividade: [82, 2.1, 3.6, 6.4][i],
          unidadeDaProdutividade: c.unidade.includes('60') ? 'sc/ha' : 't/ha',
          custoVariavelHa: variavel,
          custoFixoHa: fixo,
          custoOperacionalHa: operacional,
          rendaDeFatoresHa: Number((total * 0.2).toFixed(2)),
          custoTotalHa: total,
          custoOperacionalUnidade: Number((operacional / [82, 2.1, 3.6, 6.4][i]).toFixed(2)),
          custoTotalUnidade: Number((total / [82, 2.1, 3.6, 6.4][i]).toFixed(2)),
        };
      }),
      procedencia: PROCEDENCIA,
    };
  });
}

/** Duas janelas de crédito que fecham entre si. */
const janelas = (linhas: number, valor: number, crescimento: number) => ({
  linhas,
  valor,
  linhasAnteriores: Math.round(linhas / crescimento),
  valorAnterior: Math.round(valor / crescimento),
});

const indice = (i: number) => ({
  indice: Number((0.92 + i * 0.04).toFixed(2)),
  faixa: (i > 1 ? 'Aquecido' : 'Retraido') as 'Aquecido' | 'Retraido',
  indiceDeLinhas: Number((0.9 + i * 0.05).toFixed(2)),
  indiceDeValor: Number((0.95 + i * 0.03).toFixed(2)),
  linhas: 120 + i * 30,
  linhasAnteriores: 130 + i * 25,
  valorMedioPorLinha: 268_000 + i * 12_000,
  valorMedioAnterior: 259_000 + i * 11_000,
  indiceDoValorMedio: 1.03,
  basePequena: false,
  motivo: 'Nenhum' as const,
});

/**
 * O crédito rural.
 *
 * A JANELA TEM CARÊNCIA DECIDIDA na amostra: sem isso o painel abre com a
 * ressalva de que o mês recente ainda enche, e o revisor olha a ressalva em vez
 * do desenho. A ressalva tem o próprio teste; aqui o que se revisa é o layout.
 */
export function creditoFicticio(municipios: { codigo: number; nome: string }[]): PainelDeCreditoRural {
  const produtos = [
    { codigo: 101, nome: 'Tratores', ehMaquina: true, j: janelas(1_240, 332_000_000, 1.08) },
    { codigo: 102, nome: 'Colheitadeiras', ehMaquina: true, j: janelas(410, 246_000_000, 1.14) },
    { codigo: 103, nome: 'Máquinas e implementos', ehMaquina: true, j: janelas(980, 121_000_000, 0.96) },
    { codigo: 201, nome: 'Armazenagem', ehMaquina: false, j: janelas(320, 88_000_000, 1.02) },
    { codigo: 202, nome: 'Irrigação', ehMaquina: false, j: janelas(210, 54_000_000, 1.21) },
  ];

  return {
    ultimoMes: mesDe(2),
    janela: {
      ultimoMesComDado: mesDe(0),
      mesesDeCarencia: 2,
      carenciaDecidida: true,
      inicio: mesDe(13),
      fim: mesDe(2),
      inicioAnterior: mesDe(25),
      fimAnterior: mesDe(14),
      mesesPorJanela: 12,
    },
    porAno: Array.from({ length: 12 }, (_, i) => {
      const ano = 2014 + i;
      const linhasDeMaquinas = 1_400 + i * 130 + (i % 3) * 90;
      return {
        ano,
        linhasDeMaquinas,
        valorDeMaquinas: linhasDeMaquinas * 268_000,
        linhasTotais: Math.round(linhasDeMaquinas * 1.45),
        valorTotal: Math.round(linhasDeMaquinas * 268_000 * 1.32),
        ultimoMes: ano === 2025 ? 9 : 12,
      };
    }),
    porProduto: produtos.map((p) => ({ codigo: p.codigo, nome: p.nome, ehMaquina: p.ehMaquina, janelas: p.j })),
    porMunicipio: municipios.slice(0, 18).map((m, i) => ({
      codigoIbge: m.codigo,
      nome: m.nome,
      pertenceAAdr: i % 5 !== 4,
      janelas: janelas(210 - i * 9, (58 - i * 2.4) * 1_000_000, 1 + ((i % 5) - 2) * 0.06),
      indice: indice(i % 4),
    })),
    regiao: {
      recorte: 'Região Tracbel',
      municipios: municipios.length,
      janelas: janelas(2_630, 699_000_000, 1.07),
      indice: indice(1),
    },
    saoPaulo: {
      recorte: 'São Paulo',
      municipios: 645,
      janelas: janelas(18_900, 5_120_000_000, 1.04),
      indice: indice(2),
    },
    procedencia: PROCEDENCIA,
    // O MÊS A MÊS DAS DUAS JANELAS (issue 68, 27/09/2026): 24 meses, da anterior à recente. Norte e
    // Noroeste somam a Região mês a mês, como no servidor; a recente um pouco acima da anterior.
    porMes: Array.from({ length: 24 }, (_, i) => {
      const recente = i >= 12;
      const valor = Math.round(onda(i, 52, 14) * 1_000_000 * (recente ? 1.07 : 1));
      const linhas = Math.round(onda(i, 210, 40) * (recente ? 1.05 : 1));
      const norte = { linhas: Math.round(linhas * 0.42), valor: Math.round(valor * 0.42) };
      return {
        mes: mesDe(25 - i),
        janelaRecente: recente,
        regiaoTracbel: { linhas, valor },
        norte,
        noroeste: { linhas: linhas - norte.linhas, valor: valor - norte.valor },
        saoPaulo: { linhas: linhas * 7, valor: valor * 7 },
      };
    }),
  };
}

/**
 * O CATÁLOGO DE CULTURAS (fidelidade às maquetes, fase 3 — Momento do mercado).
 *
 * POR QUE ELE PRECISOU EXISTIR NO HARNESS: é o catálogo que liga a cultura da
 * rentabilidade à área colhida da PAM dos municípios (a "Média da Região
 * Tracbel") e ao preço da CONAB (a coluna de preço do Termo de troca). Sem ele
 * as duas abas abriam com o traço em tudo, e o layout não era revisável.
 *
 * OS CÓDIGOS DE PRODUTO SÃO OS INVENTADOS DO `amostras.ts` (900001 em diante,
 * na mesma ordem de culturas), para a PAM fictícia dos municípios casar. O
 * produto de preço é o `AMOSTRA-…` das séries fictícias acima.
 */
export function catalogoFicticio(): CatalogoDoMercado {
  const quilos = [1000, 60, 60, 60, 40.8];
  return {
    categorias: [],
    culturas: CULTURAS.map((c, i) => ({
      codigo: c.codigo,
      nome: c.nome,
      segmento: 'Lavoura (amostra)',
      unidadeComercial: c.unidade,
      quilosPorUnidade: quilos[i],
      fonteDoPreco: 'HARNESS',
      produtoDoPreco: `AMOSTRA-${c.codigo}`,
      serieDeCusto: i < 4 ? c.nome : null,
      estaAtiva: true,
      produtos: [{ codigoIbge: 900_001 + i, nome: c.nome, entraNaSomaDaLavoura: true }],
    })),
  };
}

/**
 * AS PERCEPÇÕES DO GESTOR (issue 71), para a aba Percepção comercial.
 *
 * NENHUM NOME DE PESSOA: `informadoPor` fica nulo (o mesmo da semente da
 * migração) e a justificativa se declara amostra. Os municípios são os da malha
 * pública, os mesmos que o `amostras.ts` põe na Região Tracbel; as leituras são
 * inventadas e cobrem os três sinais — positivo, zero e negativo.
 */
export function parametrosFicticios(municipios: { codigo: number; nome: string }[]): ParametrosDoPotencialVigentes {
  const leituras = [4, 3, 2.5, 1, 0, -1.5, -3];
  const tendencias = ['Alta', 'Alta', 'Estavel', null, 'Estavel', 'Queda', 'Queda'] as const;
  const comLeitura = municipios.slice(0, leituras.length);
  // A SÉRIE MENSAL (28/09/2026): a leitura de cada município nos 12 meses, subindo devagar até a de hoje.
  const meses = Array.from({ length: 12 }, (_, i) => {
    const d = new Date(Date.UTC(2025, 9 + i, 1));
    return `${d.getUTCFullYear()}-${String(d.getUTCMonth() + 1).padStart(2, '0')}-01`;
  });
  return {
    em: '2026-09-23',
    geral: null,
    culturas: [],
    pendencias: [],
    serieDasPercepcoes: meses.flatMap((mes, j) =>
      comLeitura.map((m, i) => ({
        mes,
        municipioCodigoIbge: m.codigo,
        percentual: Number((leituras[i] - (11 - j) * 0.15 + (j % 3 === 0 ? 0.3 : 0)).toFixed(2)),
      })),
    ),
    percepcoes: comLeitura.map((m, i) => ({
      municipioCodigoIbge: m.codigo,
      municipioNome: m.nome,
      uf: 'SP',
      percentual: leituras[i],
      tendenciaParaTresMeses: tendencias[i],
      vigencia: {
        vigenteDesde: '2026-08-01',
        justificativa: 'amostra do harness — não é leitura de gestor nenhum',
        informadoPor: null,
        informadoEm: '2026-08-01T12:00:00Z',
        revogadoEm: null,
        revogadoPor: null,
        motivoDaRevogacao: null,
      },
    })),
  };
}

/**
 * O DIAGNÓSTICO COMERCIAL FICTÍCIO (issue 257) — números inventados, mas coerentes entre si: o IOC é a média
 * ponderada dos componentes pelos pesos do protótipo, e a classe sai do IOC. Um município em cada seis não tem carteira
 * e um em cada nove não tem regra de potencial, para a tela mostrar os dois vazios com o motivo.
 *
 * O CEN DE CADA MUNICÍPIO (02/10/2026) é um dos dez CENs fictícios da Visão 360 — o município sem carteira fica sem CEN.
 * Com um CEN escolhido, como na API, só o município da carteira dele mantém o CEN.
 */
export function diagnosticoFicticio(
  municipios: { codigo: number; nome: string }[],
  vazio: boolean,
  responsavel: string | null = null,
): DiagnosticoComercialDaRegiao {
  const responsaveis = CENS.map((nome, i) => ({ id: 9001 + i, nome, natureza: 'Pessoa', carteiras: 1 + (i % 3), gestor: null }));
  const escolhido = responsaveis.find((r) => String(r.id) === responsavel) ?? null;
  const pesos = { potencial: 25, cobertura: 20, credito: 15, rentabilidade: 15, clientes: 10, realizacao: 5, penetracao: 10 };
  const culturas = ['Cana-de-açúcar', 'Soja', 'Café', 'Laranja', 'Milho', 'Amendoim'];
  // O NOME DA LOJA COMO ELE VEM DE PRODUÇÃO ("Tracbel Agro — Cidade", o padrão das filiais), e como a maquete escreve.
  const lojas = ['Araraquara', 'Ribeirão Preto', 'Barretos', 'Franca', 'Bebedouro'].map((cidade) => `Tracbel Agro — ${cidade}`);
  const classe = (ioc: number): ClasseDePrioridade =>
    ioc >= 80 ? 'Maxima' : ioc >= 60 ? 'Alta' : ioc >= 40 ? 'Moderada' : ioc >= 20 ? 'Baixa' : 'Manutencao';

  const linhas = vazio
    ? []
    : municipios.map((m, i) => {
        const r = (k: number) => ((i * 37 + k * 17) % 100) / 100;
        const semCarteira = i % 6 === 5;
        const semRegra = i % 9 === 8;
        const demanda = semRegra ? null : Math.round((2 + r(1) * 40) * 10) / 10;
        const ajustada = demanda === null ? null : Math.round(demanda * (0.85 + r(2) * 0.3) * 10) / 10;
        const vendidas = demanda === null ? null : Math.round(demanda * r(3) * 0.6);
        const vinculos = semCarteira ? 0 : 20 + Math.round(r(4) * 180);
        const cobertos = Math.round(vinculos * r(5));
        const credito = 0.7 + r(6) * 0.7;
        const preco = 0.75 + r(7) * 0.55;
        const c = {
          potencial: ajustada === null ? null : Math.min(1, ajustada / 36),
          cobertura: vinculos === 0 ? null : 1 - cobertos / vinculos,
          credito: Math.max(0, Math.min(1, (credito - 0.6) / 0.8)),
          rentabilidade: Math.max(0, Math.min(1, (preco - 0.6) / 0.8)),
          clientes: ajustada === null ? null : Math.max(0, 1 - (vinculos / 4) / Math.max(1, ajustada * 3)),
          realizacao: ajustada === null || vendidas === null ? null : Math.max(0, 1 - vendidas / Math.max(0.1, ajustada * 0.31)),
          penetracao: demanda === null || vendidas === null ? null : Math.max(0, 1 - vendidas / demanda),
        };
        const comDado = (Object.keys(pesos) as (keyof typeof pesos)[]).filter((k) => c[k] !== null);
        const soma = comDado.reduce((s, k) => s + pesos[k], 0);
        const ioc = soma ? Math.round((1000 * comDado.reduce((s, k) => s + pesos[k] * (c[k] as number), 0)) / soma) / 10 : null;
        const ausentes = [
          ...(vinculos === 0 ? ['cobertura: nenhum vínculo de carteira com cadência declarada'] : []),
          ...(demanda === null
            ? ['potencial: o município não tem demanda estimada', 'clientes: o município não tem demanda estimada']
            : []),
        ];
        return {
          codigoIbge: m.codigo,
          nome: m.nome,
          regiao: i % 2 ? 'Noroeste' : 'Norte',
          lojaCodigo: `0101${String(10 + (i % lojas.length)).padStart(2, '0')}`,
          loja: lojas[i % lojas.length],
          culturaPrincipal: culturas[i % culturas.length],
          indiceDePreco: Math.round(preco * 1000) / 1000,
          indiceDeCredito: Math.round(credito * 1000) / 1000,
          creditoBasePequena: i % 7 === 3,
          demandaEstrutural: demanda,
          demandaAjustada: ajustada,
          metaDePlanejamento: ajustada === null ? null : Math.round(ajustada * 0.31 * 100) / 100,
          vendidasNoPeriodo: vendidas,
          vendidasNoAno: vendidas,
          clientes: Math.round(vinculos / 4),
          clientesPorClasse: (() => {
            const n = Math.round(vinculos / 4);
            const a = Math.round(n * 0.1);
            const b = Math.round(n * 0.2);
            const cc = Math.round(n * 0.3);
            const semClasse = Math.round(n * 0.15);
            return { a, b, c: cc, d: Math.max(0, n - a - b - cc - semClasse), semClasse };
          })(),
          clientesEmCarteira: Math.round((vinculos / 4) * 0.8),
          clientesQueCompraram: Math.round((vinculos / 4) * r(8) * 0.5),
          vinculosComCadencia: vinculos,
          cobertos,
          cobertura: vinculos === 0 ? null : cobertos / vinculos,
          penetracao: demanda === null || vendidas === null ? null : vendidas / demanda,
          componentes: c,
          ioc,
          classe: ioc === null ? null : classe(ioc),
          situacao: ioc !== null && ioc >= 60 ? 'elevado potencial, baixa cobertura comercial, crédito de mecanização em alta' : 'situação equilibrada',
          planoDeAcao: ioc !== null && ioc >= 60 ? 'Expandir cobertura e visitas presenciais · Explorar financiamento (Moderfrota, Finame)' : 'Monitorar',
          componentesAusentes: ausentes,
          estimativa: true,
          responsavel: semCarteira ? null : ((cen) => (!escolhido || escolhido.nome === cen ? cen : null))(CENS[i % CENS.length]),
        };
      });

  const conta = (k: ClasseDePrioridade) => linhas.filter((l) => l.classe === k).length;
  const comIoc = linhas.filter((l) => l.ioc !== null);
  return {
    competenciaInicial: '2025-09-01',
    competenciaFinal: '2026-08-01',
    fracaoDoAnoNoPeriodo: 1,
    categoria: 'TRATOR',
    categoriaNome: 'Trator',
    categorias: [
      { codigo: 'TRATOR', nome: 'Trator', ordem: 1 },
      { codigo: 'COLHEITADEIRA', nome: 'Colheitadeira', ordem: 3 },
      { codigo: 'COLHEDORA_DE_CANA', nome: 'Colhedora de cana', ordem: 7 },
    ],
    pesos,
    pesosVigentesDesde: '2026-09-27',
    pesosDoPrototipo: true,
    shares: [{ categoriaCodigo: 'TRATOR', categoriaNome: 'Trator', percentual: 31, doPrototipo: true }],
    percentil90: 36,
    resumo: {
      maxima: conta('Maxima'),
      alta: conta('Alta'),
      moderada: conta('Moderada'),
      baixa: conta('Baixa'),
      manutencao: conta('Manutencao'),
      semIndice: linhas.length - comIoc.length,
      total: linhas.length,
      iocMedio: comIoc.length ? Math.round((10 * comIoc.reduce((s, l) => s + (l.ioc as number), 0)) / comIoc.length) / 10 : null,
      ...(() => {
        const soma = (f: (l: (typeof linhas)[number]) => number | null) => {
          const com = linhas.map(f).filter((v): v is number => v !== null);
          return com.length ? Math.round(com.reduce((s, v) => s + v, 0) * 10) / 10 : null;
        };
        const estrutural = soma((l) => l.demandaEstrutural);
        const noAno = soma((l) => l.vendidasNoAno);
        return {
          demandaEstrutural: estrutural,
          demandaAjustada: soma((l) => l.demandaAjustada),
          municipiosComDemanda: linhas.filter((l) => l.demandaEstrutural !== null).length,
          metaDePlanejamento: soma((l) => l.metaDePlanejamento),
          vendidasNoPeriodo: linhas.reduce((s, l) => s + (l.vendidasNoPeriodo ?? 0), 0),
          vendidasNoAno: noAno,
          penetracao: estrutural && noAno !== null ? Math.round((noAno / estrutural) * 10000) / 10000 : null,
          clientes: linhas.reduce((s, l) => s + l.clientes, 0),
          clientesQueCompraram: linhas.reduce((s, l) => s + l.clientesQueCompraram, 0),
        };
      })(),
    },
    municipios: linhas,
    lacunas: [
      {
        metrica: 'pesosDoIoc',
        motivo: 'AMOSTRA FICTÍCIA — os pesos do IOC e a sazonalidade ainda são os do protótipo da pasta 360, a confirmar.',
      },
      {
        metrica: 'cobertura',
        motivo: 'AMOSTRA FICTÍCIA — a cobertura é a da cadência de cada classe de cliente no CRM, e não o corte fixo de 90 dias.',
      },
    ],
    responsaveis,
  };
}
/**
 * A DEMANDA E PREVISÃO FICTÍCIA (issue 258) — números inventados e coerentes: parque = área ÷ ha/máquina, demanda =
 * parque ÷ anos, a entregar = demanda × 31%, e os meses pela sazonalidade do protótipo. Um município em cada nove não
 * tem regra, para a tela mostrar o traço.
 *
 * DESDE A MAQUETE DE 02/10/2026: a área do ano anterior é a de hoje × um fator por cultura (a cana e a soja cresceram, o
 * café encolheu); a entrega do ano fiscal é ~30% da demanda do mês, até out/2026 no ano corrente e o ano inteiro nos
 * anteriores; a cultura escolhida recorta a demanda e tira a entrega, como na API.
 */
export function demandaFicticia(
  municipios: { codigo: number; nome: string }[],
  vazio: boolean,
  opcoes: { anoFiscal?: number | null; cultura?: string | null } = {},
): DemandaEPrevisaoDaRegiao {
  const todasAsCulturas = [
    { codigo: 'CANA', nome: 'Cana-de-açúcar', ha: 170, anos: 8, antes: 0.93 },
    { codigo: 'SOJA', nome: 'Soja', ha: 200, anos: 10, antes: 0.88 },
    { codigo: 'CAFE', nome: 'Café', ha: 20, anos: 10, antes: 1.04 },
    { codigo: 'LARANJA', nome: 'Laranja', ha: 20, anos: 10, antes: 0.97 },
  ];
  const cultura = opcoes.cultura ? opcoes.cultura.toUpperCase() : null;
  const culturas = todasAsCulturas.filter((c) => cultura === null || c.codigo === cultura);
  const anoFiscal = opcoes.anoFiscal ?? 2026;
  // O NOME DA LOJA COMO ELE VEM DE PRODUÇÃO ("Tracbel Agro — Cidade"), e como a maquete escreve.
  const lojas = [
    ['010110', 'Tracbel Agro — Araraquara'],
    ['010111', 'Tracbel Agro — Ribeirão Preto'],
    ['010112', 'Tracbel Agro — Barretos'],
    ['010113', 'Tracbel Agro — Franca'],
    ['010114', 'Tracbel Agro — Bebedouro'],
  ];
  const share = 0.31;
  const sazonalidade = [7, 6, 6, 7, 8, 9, 10, 10, 10, 10, 9, 8]; // nov..out, soma 100
  const meses = [11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  const soma = (xs: (number | null)[]) => {
    const com = xs.filter((x): x is number => x !== null);
    return com.length ? Math.round(com.reduce((s, x) => s + x, 0) * 100) / 100 : null;
  };

  const linhas = vazio
    ? []
    : municipios.map((m, i) => {
        const r = (k: number) => ((i * 31 + k * 13) % 100) / 100;
        const semRegra = i % 9 === 8;
        const porCultura = semRegra
          ? []
          : todasAsCulturas
              .filter((_, j) => (i + j) % 3 !== 0)
              .filter((c) => cultura === null || c.codigo === cultura)
              .map((c) => {
                const area = Math.round((500 + r(c.ha) * 12000) / 10) * 10;
                const parque = area / c.ha;
                return { c, area, parque, demanda: Math.round((parque / c.anos) * 100) / 100 };
              });
        const antes = soma(porCultura.map((p) => Math.round(p.demanda * p.c.antes * 100) / 100));
        const fatorPreco = 0.9 + r(2) * 0.25;
        const fatorCredito = 0.85 + r(3) * 0.3;
        const estrutural = soma(porCultura.map((p) => p.demanda));
        const ajustada = estrutural === null ? null : Math.round(estrutural * (fatorPreco + fatorCredito - 1) * 100) / 100;
        const [lojaCodigo, loja] = lojas[i % lojas.length];
        return {
          codigoIbge: m.codigo,
          nome: m.nome,
          regiao: i % 2 ? 'Noroeste' : 'Norte',
          lojaCodigo,
          loja,
          areaUtilHectares: soma(porCultura.map((p) => p.area)),
          parque: soma(porCultura.map((p) => Math.round(p.parque * 100) / 100)),
          porCultura: porCultura.map((p) => ({ culturaCodigo: p.c.codigo, demanda: p.demanda })),
          demandaEstrutural: estrutural,
          demandaAjustada: ajustada,
          aEntregar: estrutural === null ? null : Math.round(estrutural * share * 100) / 100,
          aEntregarAjustada: ajustada === null ? null : Math.round(ajustada * share * 100) / 100,
          fatorDePreco: semRegra ? null : Math.round(fatorPreco * 1000) / 1000,
          fatorDeCredito: semRegra ? null : Math.round(fatorCredito * 1000) / 1000,
          culturaPredominante: porCultura.length ? [...porCultura].sort((a, b) => b.parque - a.parque)[0].c.nome : null,
          variacaoPercentual: estrutural && ajustada !== null ? Math.round((ajustada / estrutural - 1) * 1000) / 10 : null,
          demandaEstruturalAnoAnterior: antes,
          variacaoAnoAnterior: estrutural !== null && antes ? Math.round((estrutural / antes - 1) * 1000) / 10 : null,
        };
      });

  const estrutural = soma(linhas.map((l) => l.demandaEstrutural));
  const ajustada = soma(linhas.map((l) => l.demandaAjustada));
  const porCulturaDoRecorte = culturas.map((c) => {
    const demanda = soma(linhas.map((l) => l.porCultura.find((p) => p.culturaCodigo === c.codigo)?.demanda ?? null));
    return {
      culturaCodigo: c.codigo,
      cultura: c.nome,
      areaUtilHectares: demanda === null ? null : Math.round(demanda * c.anos * c.ha),
      hectaresPorMaquina: c.ha,
      anosDeRenovacao: c.anos,
      parque: demanda === null ? null : Math.round(demanda * c.anos),
      demandaEstrutural: demanda,
      demandaAjustada: demanda === null ? null : Math.round(demanda * 1.04 * 100) / 100,
      variacaoPercentual: demanda === null ? null : 4,
      areaAnoAnterior: demanda === null ? null : Math.round(demanda * c.anos * c.ha * c.antes),
      parqueAnoAnterior: demanda === null ? null : Math.round(demanda * c.anos * c.antes),
      demandaAnoAnterior: demanda === null ? null : Math.round(demanda * c.antes * 100) / 100,
    };
  });

  // A ENTREGA DO ANO FISCAL: ~30% da demanda de cada mês, oscilando; o ano corrente (2026) vai até outubro, o mês da
  // amostra; os anteriores, inteiros. Com cultura escolhida, nula — a máquina não diz a cultura.
  const entregues = sazonalidade.map((s, i) =>
    cultura !== null || ajustada === null ? null : Math.round(((ajustada * s) / 100) * (0.27 + 0.06 * Math.sin(i + anoFiscal))),
  );
  const somaEntregue = entregues.reduce<number>((t, v) => t + (v ?? 0), 0);

  const porLoja = lojas
    .map(([codigo, nome]) => {
      const anual = soma(linhas.filter((l) => l.lojaCodigo === codigo).map((l) => l.aEntregarAjustada));
      return {
        lojaCodigo: codigo,
        loja: nome,
        aEntregarNoAno: anual,
        porMes: sazonalidade.map((s) => (anual === null ? null : Math.round(anual * s) / 100)),
      };
    })
    .sort((a, b) => (b.aEntregarNoAno ?? 0) - (a.aEntregarNoAno ?? 0));

  return {
    categoria: 'TRATOR',
    categoriaNome: 'Trator',
    categorias: [
      { codigo: 'TRATOR', nome: 'Trator', ordem: 1 },
      { codigo: 'COLHEITADEIRA', nome: 'Colheitadeira', ordem: 3 },
    ],
    shareAlvo: 31,
    shareDoPrototipo: true,
    sazonalidadeVigenteDesde: '2026-09-27',
    sazonalidadeDoPrototipo: true,
    anoDaAreaPlantada: 2024,
    totais: {
      parque: soma(linhas.map((l) => l.parque)),
      demandaEstrutural: estrutural,
      demandaAjustada: ajustada,
      aEntregar: soma(linhas.map((l) => l.aEntregar)),
      aEntregarAjustada: soma(linhas.map((l) => l.aEntregarAjustada)),
      culturasComRegra: culturas.map((c) => c.nome),
      municipios: linhas.length,
      municipiosComDemanda: linhas.filter((l) => l.demandaEstrutural !== null).length,
      estimativa: true,
      parqueAnoAnterior: soma(porCulturaDoRecorte.map((c) => c.parqueAnoAnterior)),
      demandaEstruturalAnoAnterior: soma(linhas.map((l) => l.demandaEstruturalAnoAnterior)),
      culturasComAumentoDeArea: culturas.filter((c) => c.antes < 1).map((c) => c.nome),
      entreguesNoPeriodo: cultura !== null || linhas.length === 0 ? null : somaEntregue,
      entreguesNoPeriodoAnterior: cultura !== null || linhas.length === 0 ? null : Math.round(somaEntregue * 0.89),
      entregasAte: cultura !== null ? null : anoFiscal === 2026 ? '2026-10-02' : `${anoFiscal}-10-31`,
    },
    porCultura: porCulturaDoRecorte.filter((c) => c.demandaEstrutural !== null),
    previsaoMensal: meses.map((mes, i) => ({
      mes,
      fracao: sazonalidade[i] / 100,
      demandaEstrutural: estrutural === null ? null : Math.round(estrutural * sazonalidade[i]) / 100,
      demandaAjustada: ajustada === null ? null : Math.round(ajustada * sazonalidade[i]) / 100,
      aEntregar: estrutural === null ? null : Math.round(estrutural * share * sazonalidade[i]) / 100,
      aEntregarAjustada: ajustada === null ? null : Math.round(ajustada * share * sazonalidade[i]) / 100,
      entregues: entregues[i],
    })),
    porLoja,
    municipios: linhas,
    culturas: culturas.map((c) => ({ codigo: c.codigo, nome: c.nome, demanda: soma(linhas.map((l) => l.porCultura.find((p) => p.culturaCodigo === c.codigo)?.demanda ?? null)) })),
    lacunas: [
      { metrica: 'sazonalidade', motivo: 'AMOSTRA FICTÍCIA — a sazonalidade ainda é a do protótipo da pasta 360, a confirmar.' },
      { metrica: 'porCliente', motivo: 'AMOSTRA FICTÍCIA — a demanda é por município, e não por cliente.' },
      cultura !== null
        ? { metrica: 'entregaPorCultura', motivo: 'AMOSTRA FICTÍCIA — a máquina entregue não diz para que cultura foi: com uma cultura escolhida, a entrega fica de fora.' }
        : { metrica: 'entregaRealizada', motivo: 'AMOSTRA FICTÍCIA — a entrega realizada é a do ART pela data da entrega, dos compradores dos municípios do recorte.' },
      { metrica: 'anoAnterior', motivo: 'AMOSTRA FICTÍCIA — o ano anterior é a mesma conta com a área da PAM de um ano antes.' },
    ],
    anoFiscal,
    anosFiscais: [2026, 2025, 2024, 2023],
    cultura,
    culturasDoFiltro: todasAsCulturas.map((c) => ({ codigo: c.codigo, nome: c.nome, demanda: null })),
    anoDaAreaAnterior: 2023,
  };
}
