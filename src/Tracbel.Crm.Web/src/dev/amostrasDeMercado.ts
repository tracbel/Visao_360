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
  PainelDeCreditoRural,
  PrecosDeMercado,
  RentabilidadeDaCultura,
  SerieDeCusto,
} from '../tipos/mercado';

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
  const linhas: { produtividade: number; preco: number; custo: number }[] = [
    { produtividade: 82_000, preco: 0.14, custo: 8_900 },
    { produtividade: 2_100, preco: 24.5 / 60, custo: 21_400 },
    { produtividade: 3_600, preco: 2.4 / 60, custo: 4_050 },
    { produtividade: 6_400, preco: 1.15 / 60, custo: 4_800 },
    { produtividade: 28_000, preco: 1.9 / 40.8, custo: 32_500 },
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
  };
}
