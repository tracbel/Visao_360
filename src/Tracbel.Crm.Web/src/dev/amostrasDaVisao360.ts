/**
 * AS AMOSTRAS DO HARNESS DA VISÃO 360 — só em desenvolvimento.
 *
 * NADA AQUI É DADO DA TRACBEL. Filial, cliente, CEN, concorrente e valor são
 * inventados, e os nomes dizem isso ("Filial Fictícia", "Cliente Fictício"):
 * uma captura que for parar numa apresentação se denuncia pelo próprio texto.
 *
 * AS RESPOSTAS TÊM A FORMA DO CONTRATO, e não a da tela: cada função devolve o
 * que a rota da API devolve (`tipos/`), e a tela faz as contas dela por cima —
 * a soma por partição, o corte dos doze meses, o ranking. Assim o harness
 * exercita o mesmo caminho da produção, e não um atalho que só existe aqui.
 *
 * TRÊS ESTADOS:
 *
 * - `completo`: cinco filiais, todos os painéis com número, nomes longos de
 *   verdade (razão social de cooperativa, CEN com quatro sobrenomes) e números
 *   de cinco dígitos na legenda — é o que estoura coluna.
 * - `vazio`: o que a produção mostra hoje, com o banco sanitizado: treze
 *   filiais respondendo, todas com zero e sem faturamento.
 * - `semCarteira`: o passo seguinte da produção — a carga do cadastro trouxe os
 *   clientes da SA1 (todos suspect) e a das carteiras ainda não rodou. O
 *   faturamento casa com os clientes, e todo número que depende de VÍNCULO fica
 *   em zero: clientes na carteira, cobertura, CENs, mix. Os ~27 mil cadastrados
 *   sem carteira NÃO aparecem em lugar nenhum, porque a rota dos cartões conta
 *   situação só entre quem tem vínculo — é por isso que o cartão de clientes
 *   precisa dizer o que o zero dele quer dizer, e este estado o deixa à vista.
 *
 * A CARTEIRA ADMINISTRATIVA E A DE TESTE EXISTEM DE PROPÓSITO no `completo`:
 * elas têm mais vínculos que qualquer carteira comercial, e o Mix por linha que
 * as somasse mostraria "Máquinas e Implemento" com a maior fatia. É o defeito
 * que o filtro do mix corrige, e o harness o deixa à vista.
 */

import type { CatalogoDeSelecao, PaginaDe } from '../tipos/api';
import type { ConferenciaComAGestao } from '../tipos/conferencia';
import type { EstoqueECobertura, MaquinaNaLista } from '../tipos/estoque';
import type { LinhaDoForecast, RelatorioDoForecast } from '../tipos/forecast';
import type { MetaERealizadoDaFilial } from '../tipos/metas';
import type { MaquinasEntreguesNoArt, PainelExecutivoDaFilial } from '../tipos/painelExecutivo';
import type {
  Agregado,
  ContagemPorRotulo,
  FaseDoFunil,
  Faturamento,
  CoberturaDeFilial,
  FunilPorEstagio,
  PainelDaAgenda,
  PainelDoCen,
  TerritorioDeCarteira,
  ProcessoResumo,
  ResumoDeCobertura,
  VendasPerdidas,
} from '../tipos/relacionamento';

export type EstadoDaVisao360 = 'completo' | 'vazio' | 'semCarteira';

export const ESTADOS_DA_VISAO360: { id: EstadoDaVisao360; titulo: string }[] = [
  { id: 'completo', titulo: 'Completo — cinco filiais com dado' },
  { id: 'vazio', titulo: 'Vazio — treze filiais, tudo zero (a produção de hoje)' },
  { id: 'semCarteira', titulo: 'Sem carteira — cadastro carregado, nenhum vínculo' },
];

/** Os estados em que nenhum cliente tem vínculo de carteira: tudo que depende de carteira é zero. */
const semVinculo = (estado: EstadoDaVisao360) => estado !== 'completo';

type FilialDeAmostra = { codigo: string; nome: string; peso: number };

const FILIAIS_COMPLETAS: FilialDeAmostra[] = [
  { codigo: '990001', nome: 'Filial Fictícia Alfa', peso: 1.6 },
  { codigo: '990002', nome: 'Filial Fictícia Beta', peso: 1.25 },
  { codigo: '990003', nome: 'Filial Fictícia Gama do Interior Paulista', peso: 1 },
  { codigo: '990004', nome: 'Filial Fictícia Delta', peso: 0.8 },
  { codigo: '990005', nome: 'Filial Fictícia Épsilon', peso: 0.55 },
];

const FILIAIS_VAZIAS: FilialDeAmostra[] = Array.from({ length: 13 }, (_, i) => ({
  codigo: `9901${String(i + 1).padStart(2, '0')}`,
  nome: `Filial Fictícia ${String(i + 1).padStart(2, '0')}`,
  peso: 0,
}));

/** As mesmas treze, com faturamento: o peso só pesa no que não depende de carteira. */
const FILIAIS_SEM_CARTEIRA: FilialDeAmostra[] = FILIAIS_VAZIAS.map((f, i) => ({ ...f, peso: 0.3 + (i % 5) * 0.2 }));

function filiaisDo(estado: EstadoDaVisao360): FilialDeAmostra[] {
  if (estado === 'vazio') return FILIAIS_VAZIAS;
  if (estado === 'semCarteira') return FILIAIS_SEM_CARTEIRA;
  return FILIAIS_COMPLETAS;
}

function filial(estado: EstadoDaVisao360, codigo: string): FilialDeAmostra {
  const lista = filiaisDo(estado);
  return lista.find((f) => f.codigo === codigo) ?? lista[0];
}

/** Arredonda para inteiro: contagem não tem casa decimal. */
const n = (v: number) => Math.round(v);

/**
 * O peso da OPERAÇÃO — agenda, funil, perdas, processos. Só o `completo` tem:
 * no `semCarteira` o banco tem cadastro e faturamento, e nenhuma tarefa nem
 * processo (a sanitização de 15/09 os tirou).
 */
function pesoDaOperacao(estado: EstadoDaVisao360, codigo: string): number {
  return estado === 'completo' ? filial(estado, codigo).peso : 0;
}

/* ------------------------------------------------------------------------ */
/* Catálogo de filiais                                                       */
/* ------------------------------------------------------------------------ */

export function catalogoDeEmpresas(estado: EstadoDaVisao360): CatalogoDeSelecao[] {
  return [
    {
      codigo: 'EMPRESA',
      nome: 'Filiais (AMOSTRA FICTÍCIA)',
      descricao: null,
      permiteItemNovo: false,
      itens: filiaisDo(estado).map((f, i) => ({
        codigo: f.codigo,
        descricao: f.nome,
        ordem: i + 1,
        exigeObservacao: false,
      })),
    },
  ];
}

/* ------------------------------------------------------------------------ */
/* Cobertura por carteira — é daqui que saem o mix e o ranking de CENs       */
/* ------------------------------------------------------------------------ */

const LINHAS = [
  'Venda de Máquinas e Implemento',
  'Venda de Peças',
  'Venda de Serviços de Oficina',
  'Venda de AMS — Agricultura de Precisão',
  'Prospecção de Novos Clientes',
  'Venda de Tratores de Grande Porte',
  'Venda de Colheitadeiras e Plataformas',
  'Venda de Pulverizadores Autopropelidos',
] as const;

const CENS = [
  'CEN FICTÍCIO ALFA DE SOUZA PEREIRA DOS SANTOS',
  'CEN FICTÍCIA BETA OLIVEIRA',
  'CEN FICTÍCIO GAMA',
  'CEN FICTÍCIA DELTA RIBEIRO',
  'CEN FICTÍCIO ÉPSILON COSTA',
  'CEN FICTÍCIO ZETA',
] as const;

export function coberturaPorCarteira(estado: EstadoDaVisao360, codigo: string): Agregado<ResumoDeCobertura> {
  if (semVinculo(estado)) return { itens: [], metricasSemDado: [] };

  const { peso } = filial(estado, codigo);
  const itens: ResumoDeCobertura[] = [];

  LINHAS.forEach((linha, i) => {
    const clientes = n((1400 - i * 150) * peso);
    const responsavel = CENS[(i + Number(codigo.slice(-1))) % CENS.length];
    itens.push({
      carteiraChave: `${codigo}-c${i}`,
      carteiraCodigo: `C${codigo.slice(-2)}${i}`,
      carteiraNome: `Carteira fictícia ${i + 1}`,
      linhaDeNegocioNome: linha,
      responsavelNome: responsavel,
      clientes,
      comContatoEm30Dias: n(clientes * 0.22),
      comContatoEm90Dias: n(clientes * 0.41),
      nuncaContatados: n(clientes * 0.3),
      ultimoContatoEm: '2026-09-20T13:00:00Z',
      naturezaDaCarteira: 'Comercial',
      naturezaDoResponsavel: 'Pessoa',
    });
  });

  // A área entra no ranking, como a Inteligência de Mercado real.
  itens.push({
    carteiraChave: `${codigo}-im`,
    carteiraCodigo: `IM${codigo.slice(-2)}`,
    carteiraNome: 'Inteligência de Mercado (amostra)',
    linhaDeNegocioNome: 'Prospecção de Novos Clientes',
    responsavelNome: 'INTELIGÊNCIA DE MERCADO (AMOSTRA)',
    clientes: n(900 * peso),
    comContatoEm30Dias: n(120 * peso),
    comContatoEm90Dias: n(260 * peso),
    nuncaContatados: n(410 * peso),
    ultimoContatoEm: '2026-09-18T13:00:00Z',
    naturezaDaCarteira: 'Comercial',
    naturezaDoResponsavel: 'Departamento',
  });

  // AS DUAS QUE NÃO SÃO COMERCIAIS, maiores que qualquer comercial — ver o
  // cabeçalho do arquivo: sem o filtro, elas dominariam o mix.
  itens.push(
    {
      carteiraChave: `${codigo}-adm`,
      carteiraCodigo: `ADM${codigo.slice(-2)}`,
      carteiraNome: 'Depósito de cadastro (amostra)',
      linhaDeNegocioNome: 'Venda de Máquinas e Implemento',
      responsavelNome: 'SISTEMA (AMOSTRA)',
      clientes: n(9000 * peso),
      comContatoEm30Dias: 0,
      comContatoEm90Dias: 0,
      nuncaContatados: n(9000 * peso),
      ultimoContatoEm: null,
      naturezaDaCarteira: 'Administrativa',
      naturezaDoResponsavel: 'Sistema',
    },
    {
      carteiraChave: `${codigo}-teste`,
      carteiraCodigo: `TST${codigo.slice(-2)}`,
      carteiraNome: 'Carteira de teste (amostra)',
      linhaDeNegocioNome: 'Linha de Teste (amostra)',
      responsavelNome: 'TESTE (AMOSTRA)',
      clientes: n(2500 * peso),
      comContatoEm30Dias: 0,
      comContatoEm90Dias: 0,
      nuncaContatados: n(2500 * peso),
      ultimoContatoEm: null,
      naturezaDaCarteira: 'Teste',
      naturezaDoResponsavel: 'Teste',
    },
  );

  return { itens, metricasSemDado: [] };
}

/* ------------------------------------------------------------------------ */
/* Agenda, funil e perdas                                                     */
/* ------------------------------------------------------------------------ */

export function painelDaAgenda(estado: EstadoDaVisao360, codigo: string): Agregado<PainelDaAgenda> {
  const peso = pesoDaOperacao(estado, codigo);
  return {
    itens: [
      {
        pendentes: n(1850 * peso),
        atrasadas: n(640 * peso),
        paraHoje: n(35 * peso),
        proximosSeteDias: n(210 * peso),
        concluidasNosUltimosTrintaDias: n(480 * peso),
        semPrazoLimite: n(1400 * peso),
        maisAntigaPendenteEm: peso === 0 ? null : '2024-03-11T12:00:00Z',
      },
    ],
    metricasSemDado: [],
  };
}

export function fasesDoFunil(estado: EstadoDaVisao360, codigo: string): Agregado<FaseDoFunil> {
  const peso = pesoDaOperacao(estado, codigo);
  if (peso === 0) return { itens: [], metricasSemDado: [] };
  const fases = ['Qualificação', 'Proposta', 'Negociação', 'Fechamento'];
  return {
    itens: fases.map((nome, i) => ({
      tipoProcessoCodigo: 'VENDA',
      tipoProcessoNome: 'Venda de máquina (amostra)',
      faseCodigo: `F${i + 1}`,
      faseNome: nome,
      faseOrdem: i + 1,
      processos: n((420 - i * 80) * peso),
      processosComValor: n((12 - i * 2) * peso),
      valorTotal: null,
    })),
    metricasSemDado: [],
  };
}

export function perdasPorMotivo(estado: EstadoDaVisao360, codigo: string): Agregado<ContagemPorRotulo> {
  const peso = pesoDaOperacao(estado, codigo);
  if (peso === 0) return { itens: [], metricasSemDado: [] };
  return { itens: [{ codigo: 'NI', nome: 'Não informado', quantidade: n(310 * peso) }], metricasSemDado: [] };
}

const MOTIVOS = [
  'Preço acima do concorrente',
  'Condição de financiamento',
  'Prazo de entrega',
  'Cliente adiou a compra',
  'Avaliação do usado abaixo do esperado',
  'Outro motivo (amostra)',
] as const;

const CONCORRENTES = [
  'Concorrente Fictício Alfa Máquinas Agrícolas',
  'Concorrente Fictício Beta',
  'Concorrente Fictício Gama',
  'Concorrente Fictício Delta',
  'Concorrente Fictício Épsilon',
] as const;

export function vendasPerdidas(estado: EstadoDaVisao360, codigo: string): VendasPerdidas {
  const peso = pesoDaOperacao(estado, codigo);
  if (peso === 0) {
    return { registradas: 0, processosPerdidos: 0, porMotivo: [], porConcorrente: [], metricasSemDado: [], periodo: PERIODO_PADRAO, formulario: null };
  }

  const fatia = (codigoDaFatia: string, nome: string, base: number) => ({
    codigo: codigoDaFatia,
    nome,
    quantidade: n(base * peso),
    maquinas: n(base * 1.3 * peso),
    comOsDoisPrecos: n(base * 0.4 * peso),
    diferencaMediaDePreco: 18_500 + base * 900,
  });

  return {
    registradas: n(96 * peso),
    processosPerdidos: n(310 * peso),
    porMotivo: MOTIVOS.map((m, i) => fatia(`M${i}`, m, 30 - i * 5)),
    porConcorrente: CONCORRENTES.map((c, i) => fatia(`C${i}`, c, 26 - i * 5)),
    metricasSemDado: [],
    periodo: PERIODO_PADRAO,
    formulario: null,
  };
}

/* ------------------------------------------------------------------------ */
/* Faturamento                                                                */
/* ------------------------------------------------------------------------ */

/** Os doze meses de out/2025 a set/2026, o último aberto. */
const COMPETENCIAS = Array.from({ length: 12 }, (_, i) => {
  const data = new Date(Date.UTC(2025, 9 + i, 1));
  return `${data.getUTCFullYear()}-${String(data.getUTCMonth() + 1).padStart(2, '0')}-01`;
});

const CLIENTES = [
  { chave: 'cli-1', nome: 'COOPERATIVA FICTÍCIA DOS PRODUTORES RURAIS DE AMOSTRA DO INTERIOR PAULISTA LTDA', classe: 'A' },
  { chave: 'cli-2', nome: 'AGROPECUÁRIA FICTÍCIA SANTA AMOSTRA S/A', classe: 'A' },
  { chave: 'cli-3', nome: 'USINA FICTÍCIA DE AÇÚCAR E ETANOL', classe: 'B' },
  { chave: 'cli-4', nome: 'FAZENDA FICTÍCIA BOA VISTA', classe: 'C' },
  { chave: 'cli-5', nome: 'PRODUTOR FICTÍCIO JOÃO DA SILVA', classe: null },
  { chave: 'cli-6', nome: 'GRUPO FICTÍCIO CANAVIEIRO', classe: 'B' },
] as const;

export function faturamento(estado: EstadoDaVisao360, codigo: string): Faturamento {
  if (estado === 'vazio') {
    return {
      competenciaMaisRecente: null,
      valorDoUltimoMes: 0,
      ultimoMesEstaAberto: false,
      serie: [],
      topClientes: [],
      metricasSemDado: [],
    };
  }

  const { peso } = filial(estado, codigo);
  const serie = COMPETENCIAS.map((competencia, i) => ({
    competencia,
    valorLiquido: n((2_400_000 + Math.sin(i / 1.7) * 900_000 + i * 60_000) * peso),
    clientes: n((140 + i * 3) * peso),
    notas: n((620 + i * 11) * peso),
  }));

  // Cada filial traz quatro do ranking, e dois deles aparecem em mais de uma:
  // é a soma por cliente que a tela precisa provar.
  const indice = Math.max(0, filiaisDo(estado).findIndex((f) => f.codigo === codigo));
  const escolhidos = [0, 1, 2 + (indice % 4), 3].map((i) => CLIENTES[Math.min(i, CLIENTES.length - 1)]);

  return {
    competenciaMaisRecente: COMPETENCIAS.at(-1)!,
    valorDoUltimoMes: serie.at(-1)!.valorLiquido,
    ultimoMesEstaAberto: true,
    serie,
    topClientes: escolhidos.map((c, i) => ({
      clienteChave: c.chave,
      nome: c.nome,
      classe: c.classe,
      valorLiquido: n((3_100_000 - i * 520_000) * peso),
      ultimaCompraEm: COMPETENCIAS[11 - i],
    })),
    metricasSemDado: [],
  };
}

/**
 * Os processos por situação. A Visão 360 pede UMA linha e só lê o `total`; o Pipeline (29/09/2026, harness com `rota=`)
 * pede uma página, e ganha linhas fictícias — com e sem valor declarado, parados há pouco e há muito — para o desenho
 * da tabela ser conferido com dado.
 */
export function contagemDeProcessos(
  estado: EstadoDaVisao360,
  codigo: string,
  situacao: string,
  tamanho = 1,
): PaginaDe<ProcessoResumo> {
  const peso = pesoDaOperacao(estado, codigo);
  const total = situacao === 'Ganho' ? n(260 * peso) : situacao === 'Perdido' ? n(310 * peso) : n(900 * peso);
  const linhas = tamanho > 1 ? Math.min(tamanho, total, 12) : 0;
  const fases = [
    ['APRESENTACAO', 'Apresentação', 1],
    ['NEGOCIACAO', 'Negociação', 2],
    ['PEDIDO', 'Pedido de venda', 3],
  ] as const;
  const itens: ProcessoResumo[] = Array.from({ length: linhas }, (_, i) => {
    const [faseCodigo, faseNome, faseOrdem] = fases[i % fases.length];
    const dias = [4, 18, 37, 95, 140, 12][i % 6];
    return {
      chave: `00000000-0000-4000-8000-${String(900 + i).padStart(12, '0')}`,
      numero: 48_210 + i,
      titulo: `Trator fictício ${6110 + i * 5}J — negociação de amostra`,
      clienteChave: `00000000-0000-4000-8000-${String(700 + i).padStart(12, '0')}`,
      clienteNome: `Produtor Fictício ${['Alfa', 'Beta', 'Gama', 'Delta', 'Épsilon', 'Zeta'][i % 6]}`,
      tipoProcessoCodigo: 'VENDA_MAQUINA',
      tipoProcessoNome: 'Venda de máquina (amostra)',
      faseCodigo,
      faseNome,
      faseOrdem,
      faseDesde: `2026-0${(i % 8) + 1}-15`,
      diasNaFase: dias,
      situacao: situacao || 'Aberto',
      valorEstimado: i % 4 === 0 ? 485_000 + i * 12_500 : null,
      previsaoConclusao: i % 3 === 0 ? '2026-11-30' : null,
      proprietarioNome: `CEN Fictício ${['Norte', 'Sul', 'Leste'][i % 3]}`,
      criadoEm: '2026-03-02T10:00:00Z',
    };
  });
  return {
    itens,
    pagina: 1,
    tamanho: Math.max(1, tamanho),
    total,
    totalDePaginas: Math.max(1, Math.ceil(total / Math.max(1, tamanho))),
    temProxima: total > Math.max(1, tamanho),
  };
}

/* ------------------------------------------------------------------------ */
/* O território das carteiras (Cobertura por Filial, 29/09/2026)              */
/* ------------------------------------------------------------------------ */

/** Municípios fictícios de SP — nomes inventados, id e UF de mentira, como o resto do harness. */
const MUNICIPIOS_FICTICIOS = Array.from({ length: 14 }, (_, i) => ({
  id: 900_000 + i,
  nome: `Município Fictício ${String.fromCharCode(65 + i)}`,
  uf: 'SP',
  codigoIbge: null,
}));

/**
 * As carteiras da filial com as cidades que atendem. No `completo`, cinco carteiras de campo declaram cidade e duas não
 * (o depósito de cadastro e a de teste) — a lacuna do cadastro de origem, que a tela mostra com zero em vez de esconder.
 */
export function territorioDasCarteiras(estado: EstadoDaVisao360, codigo: string): Agregado<TerritorioDeCarteira> {
  if (semVinculo(estado)) {
    return { itens: [], metricasSemDado: [{ metrica: 'carteiras', motivo: 'Nenhuma carteira desta filial foi carregada ainda.' }] };
  }
  const f = filial(estado, codigo);
  const carteira = (i: number, nome: string, linha: string, responsavel: string, cidades: number) => ({
    carteiraChave: `00000000-0000-4000-8000-0000000c${String(100 + i).padStart(4, '0')}`,
    carteiraCodigo: `CART_FICT_${i}`,
    carteiraNome: nome,
    linhaDeNegocioNome: linha,
    responsavelNome: responsavel,
    empresaCodigo: f.codigo,
    empresaNome: f.nome,
    municipios: MUNICIPIOS_FICTICIOS.slice(i % 4, (i % 4) + cidades),
  });
  const itens = [
    carteira(1, 'Carteira fictícia 1', 'Máquinas e Implementos', 'CEN OLIVEIRA', 7),
    carteira(2, 'Carteira fictícia 2', 'Máquinas e Implementos', 'CEN GAMA', 5),
    carteira(3, 'Carteira fictícia 3', 'Peças e AMS', 'CEN RIBEIRO', 9),
    carteira(4, 'Carteira fictícia 4', 'Prospecção de Novos Clientes', 'CEN COSTA', 3),
    carteira(5, 'Inteligência de Mercado (amostra)', 'Máquinas e Implementos', 'INTELIGÊNCIA (AMOSTRA)', 10),
    carteira(6, 'Depósito de cadastro (amostra)', 'Administrativa', 'SISTEMA (AMOSTRA)', 0),
    carteira(7, 'Carteira de teste (amostra)', 'Administrativa', 'TESTE (AMOSTRA)', 0),
  ];
  return {
    itens,
    metricasSemDado: [
      { metrica: 'carteirasSemMunicipio', motivo: '2 de 7 carteiras não declaram nenhuma cidade no sistema de origem e aparecem com zero.' },
    ],
  };
}

/** A cobertura territorial por filial: uma linha, a filial do cabeçalho, somada das carteiras acima. */
export function coberturaPorFilial(estado: EstadoDaVisao360, codigo: string): Agregado<CoberturaDeFilial> {
  const territorio = territorioDasCarteiras(estado, codigo);
  if (territorio.itens.length === 0) return { itens: [], metricasSemDado: territorio.metricasSemDado };
  const f = filial(estado, codigo);
  const municipios = new Set(territorio.itens.flatMap((c) => c.municipios.map((m) => m.id)));
  return {
    itens: [
      {
        empresaChave: `00000000-0000-4000-8000-0000000e${f.codigo.slice(-4)}`,
        empresaCodigo: f.codigo,
        empresaNome: f.nome,
        carteiras: territorio.itens.length,
        carteirasComMunicipio: territorio.itens.filter((c) => c.municipios.length > 0).length,
        municipios: municipios.size,
        ufs: ['SP'],
      },
    ],
    metricasSemDado: [],
  };
}

/* ------------------------------------------------------------------------ */
/* O painel do CEN (Performance de CEN, 29/09/2026)                           */
/* ------------------------------------------------------------------------ */

/** Os responsáveis do seletor da Performance de CEN — os mesmos nomes fictícios da cobertura por carteira. */
const RESPONSAVEIS_DO_PAINEL = [
  { chave: '00000000-0000-4000-8000-000000000c01', nome: 'CEN OLIVEIRA', natureza: 'Pessoa', carteiras: 2, peso: 1 },
  { chave: '00000000-0000-4000-8000-000000000c02', nome: 'CEN GAMA', natureza: 'Pessoa', carteiras: 2, peso: 0.85 },
  { chave: '00000000-0000-4000-8000-000000000c03', nome: 'CEN RIBEIRO', natureza: 'Pessoa', carteiras: 1, peso: 0.55 },
  { chave: '00000000-0000-4000-8000-000000000c04', nome: 'INTELIGÊNCIA (AMOSTRA)', natureza: 'Departamento', carteiras: 1, peso: 0.45 },
] as const;

/**
 * O painel de um responsável — ou o consolidado, sem responsável —, com a cobertura por classe da curva ABC contra a
 * cadência declarada. No `completo` as quatro classes têm vínculo; nos outros dois, nenhum responsável tem carteira.
 */
export function painelDoCen(estado: EstadoDaVisao360, codigo: string, responsavel: string | null): PainelDoCen {
  const semCarteira = semVinculo(estado);
  const peso = semCarteira ? 0 : filial(estado, codigo).peso;
  const escolhido = RESPONSAVEIS_DO_PAINEL.find((r) => r.chave === responsavel) ?? null;
  const fator = peso * (escolhido ? escolhido.peso / 2.85 : 1);
  const classe = (c: string, clientes: number, cobertos: number, fora: number, nunca: number, sem: number, dias: number | null) => ({
    classe: c,
    clientes: n(clientes * fator),
    cobertos: n(cobertos * fator),
    foraDaCadencia: n(fora * fator),
    nuncaContatados: n(nunca * fator),
    semCadenciaDeclarada: n(sem * fator),
    diasDeCadencia: dias,
  });
  const porClasse = semCarteira
    ? []
    : [
        classe('A', 320, 190, 95, 35, 0, 180),
        classe('B', 540, 250, 190, 100, 0, 180),
        classe('C', 1_900, 610, 720, 470, 100, null),
        classe('D', 5_200, 1_050, 1_600, 2_150, 400, 360),
      ];
  const clientes = porClasse.reduce((s, c) => s + c.clientes, 0);

  return {
    painel: {
      responsavelChave: escolhido?.chave ?? '',
      responsavelNome: escolhido?.nome ?? 'Todos os responsáveis',
      naturezaDoResponsavel: escolhido?.natureza ?? 'Pessoa',
      carteiras: escolhido ? escolhido.carteiras : semCarteira ? 0 : 6,
      clientes,
      porClasse,
      processosGanhos: 0,
      processosPerdidos: 0,
      processosAbertos: 0,
      vendasPerdidasRegistradas: 0,
      faturamentoDaCarteira: n(18_400_000 * fator),
    },
    responsaveis: semCarteira ? [] : RESPONSAVEIS_DO_PAINEL.map(({ chave, nome, natureza, carteiras }) => ({ chave, nome, natureza, carteiras })),
    metricasSemDado: semCarteira
      ? [{ metrica: 'carteiras', motivo: 'Nenhuma carteira desta filial tem vínculo ativo: a carga das carteiras ainda não rodou.' }]
      : [],
  };
}

/* ------------------------------------------------------------------------ */
/* Os cinco cartões                                                           */
/* ------------------------------------------------------------------------ */

export function indicadoresExecutivos(estado: EstadoDaVisao360, codigo: string, ano: number): PainelExecutivoDaFilial {
  const { peso } = filial(estado, codigo);
  const vazio = estado === 'vazio';
  const doAnoCorrente = ano === 2026;
  // Carteira e cobertura dependem de VÍNCULO; o mercado, de processo. No `semCarteira` os dois são zero.
  const pesoDaCarteira = semVinculo(estado) ? 0 : peso;
  const pesoDoMercado = pesoDaOperacao(estado, codigo);

  const comCliente = n(2_750_000 * peso);
  const contraparte = n(310_000 * peso);
  const fabrica = n(180_000 * peso);
  const grupo = n(95_000 * peso);
  const revenda = n(40_000 * peso);
  const semCliente = contraparte + fabrica + grupo + revenda;

  const vinculos = n(9_300 * pesoDaCarteira);
  const elegiveis = n(6_200 * pesoDaCarteira);
  const cobertos = n(2_050 * pesoDaCarteira);
  const foraDaCadencia = n(1_700 * pesoDaCarteira);
  const nunca = elegiveis - cobertos - foraDaCadencia;

  return {
    indicadores: {
      referenciaUtc: '2026-09-24T12:00:00Z',
      faturamentoDoMes: vazio
        ? null
        : {
            competencia: '2026-09-01',
            comCliente,
            contraparteSemCadastro: contraparte,
            repasseDeFabrica: fabrica,
            empresaDoGrupo: grupo,
            outraRevenda: revenda,
            maquina: n(comCliente * 0.62),
            peca: n(comCliente * 0.24),
            servico: n(comCliente * 0.1),
            outros: n(comCliente * 0.04) + semCliente,
            notas: n(640 * peso),
            carregadoEm: '2026-09-24T09:10:00Z',
            semCliente,
            total: comCliente + semCliente,
          },
      // O ANO É O FISCAL (27/09/2026): o FY2026 vai de nov/2025 a out/2026, e em
      // setembro dele há onze meses carregados.
      ano: {
        ano,
        calendario: 'Fiscal',
        inicio: `${ano - 1}-11-01`,
        fim: `${ano}-10-01`,
        primeiraCompetencia: vazio ? null : `${ano - 1}-11-01`,
        ultimaCompetencia: vazio ? null : doAnoCorrente ? '2026-09-01' : `${ano}-10-01`,
        mesesComFaturamento: vazio ? 0 : doAnoCorrente ? 11 : 12,
        comCliente: vazio ? 0 : n(24_000_000 * peso),
        semCliente: vazio ? 0 : n(5_600_000 * peso),
        total: vazio ? 0 : n(29_600_000 * peso),
      },
      // A SITUAÇÃO É CONTADA SÓ ENTRE QUEM TEM VÍNCULO (a rota real faz assim): no `semCarteira` os
      // suspects da carga do cadastro existem no banco e não chegam aqui — a linha inteira é zero.
      carteira: {
        clientesCadastradosComVinculo: n(5_400 * pesoDaCarteira),
        clientes: n(3_900 * pesoDaCarteira),
        prospects: n(1_100 * pesoDaCarteira),
        suspects: n(400 * pesoDaCarteira),
        outrasSituacoes: 0,
        semDocumento: n(230 * pesoDaCarteira),
        clientesNasCarteirasDaFilial: n(5_900 * pesoDaCarteira),
        vinculos: n(21_000 * pesoDaCarteira),
        vinculosComerciais: vinculos,
        carteiras: n(14 * pesoDaCarteira),
        carteirasComerciais: n(11 * pesoDaCarteira),
      },
      cobertura: {
        vinculosComerciais: vinculos,
        elegiveis,
        cobertos,
        foraDaCadencia,
        nuncaContatados: nunca,
        semCadencia: vinculos - elegiveis,
        contatoMaisRecente: pesoDaCarteira === 0 ? null : '2026-09-23T18:00:00Z',
        tiposDeAtividade: vazio ? 0 : 23,
        tiposMarcadosComoVisita: 0,
        pendentes: foraDaCadencia + nunca,
      },
      mercado: {
        vendasPerdidasRegistradas: n(96 * pesoDoMercado),
        comConcorrente: n(81 * pesoDoMercado),
        comModeloDoConcorrente: n(52 * pesoDoMercado),
        comOsDoisPrecos: n(38 * pesoDoMercado),
        unidades: n(124 * pesoDoMercado),
        primeiraEm: pesoDoMercado === 0 ? null : '2025-11-03',
        ultimaEm: pesoDoMercado === 0 ? null : '2026-09-19',
      },
      // O FATURAMENTO PELO ART (29/09/2026): as máquinas entregues do FY até o último mês fechado — agosto, no
      // FY2026 —, o mesmo trecho do FY anterior e setembro à parte. O ART é o realizado da meta (as vendas que o CRM
      // tem) MAIS as que aguardam na integração, e por isso passa dele; ticket médio perto de R$ 520 mil.
      entreguesNoAno: entregues(`${ano - 1}-11-01`, doAnoCorrente ? '2026-08-01' : `${ano}-10-01`, vazio ? 0 : 250 * peso, 520_000),
      entreguesNoMesmoTrechoDoAnoAnterior: entregues(
        `${ano - 2}-11-01`,
        doAnoCorrente ? '2025-08-01' : `${ano - 1}-10-01`,
        vazio ? 0 : 220 * peso,
        495_000,
      ),
      entreguesNoMesEmCurso: entregues('2026-09-01', '2026-09-01', vazio ? 0 : 13 * peso, 510_000),
    },
    metricasSemDado: [],
  };
}

/** Uma janela de máquinas entregues no ART: uma em seis ainda aguarda cadastro no CRM, uma em cinquenta sem valor. */
function entregues(inicio: string, fim: string, maquinas: number, ticket: number): MaquinasEntreguesNoArt {
  const total = n(maquinas);
  const semValor = n(total / 50);
  return { inicio, fim, maquinas: total, valor: (total - semValor) * ticket, semValor, aguardandoNoCrm: n(total / 6) };
}

/* ------------------------------------------------------------------------ */
/* A meta de venda × o realizado (#138)                                       */
/* ------------------------------------------------------------------------ */

/**
 * A meta de venda de uma filial, em máquinas. No `completo` o cadastro da API Gestão de Negócios foi lido, e o ano fiscal
 * até agosto tem meta, realizado, pendentes do ART e consórcio; nos outros dois a rotina das metas ainda não rodou — é o
 * que a produção mostra até alguém gravar a chave e ligar a rotina.
 */
export function metasDeVenda(estado: EstadoDaVisao360, codigo: string): MetaERealizadoDaFilial {
  const { peso } = filial(estado, codigo);
  const lida = estado === 'completo';
  const p = lida ? peso : 0;
  const meses = ['2025-11-01', '2025-12-01', '2026-01-01', '2026-02-01', '2026-03-01', '2026-04-01', '2026-05-01', '2026-06-01', '2026-07-01', '2026-08-01'];
  const porMes = meses.map((competencia, i) => ({
    competencia,
    metaMaquinas: n((24 + (i % 4) * 3) * p),
    realizadoMaquinas: n((19 + (i % 3) * 2) * p),
    metaConsorcio: n(4 * p),
    realizadoConsorcio: n(3 * p),
  }));
  const meta = porMes.reduce((s, m) => s + m.metaMaquinas, 0);
  const realizado = porMes.reduce((s, m) => s + m.realizadoMaquinas, 0);

  return {
    periodo: {
      inicial: '2025-11-01', final: '2026-08-01', meses: 10, anoFiscal: 2026, texto: 'nov/2025 a ago/2026', ehOPadrao: true,
      inicialDoAnterior: '2024-11-01', finalDoAnterior: '2025-08-01',
    },
    alcance: 'Filial',
    totais: { metaMaquinas: meta, realizadoMaquinas: realizado, pendentesNoArt: n(21 * p), metaConsorcio: n(40 * p), vendasSemVendedor: n(2 * p), realizadoConsorcio: n(33 * p), aguardandoEntrega: n(6 * p) },
    porMes,
    porLinha: lida
      ? [
          { codigo: 'TRATOR_MEDIO', nome: 'TRATOR MÉDIO', meta: n(meta * 0.5), realizado: n(realizado * 0.55) },
          { codigo: 'COLHEDORA_CANA_CH_570', nome: 'COLHEDORA CANA CH 570', meta: n(meta * 0.3), realizado: n(realizado * 0.25) },
          { codigo: 'PULVERIZADOR', nome: 'PULVERIZADOR', meta: meta - n(meta * 0.5) - n(meta * 0.3), realizado: realizado - n(realizado * 0.55) - n(realizado * 0.25) },
        ]
      : [],
    porConsultor: lida
      ? [
          { consultor: 'CONSULTOR.FICTICIO.UM', temConta: true, meta: n(meta * 0.6), realizado: n(realizado * 0.62) },
          { consultor: 'CONSULTORA.FICTICIA.DOIS', temConta: false, meta: meta - n(meta * 0.6), realizado: realizado - n(realizado * 0.62) },
        ]
      : [],
    mesEmCurso: { competencia: '2026-09-01', metaMaquinas: n(27 * p), realizadoMaquinas: n(11 * p), metaConsorcio: n(4 * p), realizadoConsorcio: n(1 * p) },
    mesmoTrechoDoFyAnterior: { realizadoMaquinas: n(realizado * 0.9) },
    origem: lida
      ? { sistema: 'API Gestão de Negócios', rota: '/api/v1/cadastros/metas', lidaEm: '2026-09-27T09:00:00Z', geradaNaOrigemEm: '2026-09-27T08:59:40Z' }
      : null,
    metricasSemDado: lida
      ? [
          { metrica: 'consorcio', motivo: `Meta de consórcio: ${n(40 * p)} cotas no período. O realizado de consórcio não é medido pelo CRM.` },
          { metrica: 'pendentesSemFilial', motivo: '3 vendas pendentes do ART no período têm unidade sem filial no CRM e não são contadas em filial nenhuma.' },
        ]
      : [{ metrica: 'cadastroDeMetasNaoLido', motivo: 'O cadastro de metas da API Gestão de Negócios ainda não foi lido.' }],
  };
}

/* ------------------------------------------------------------------------ */
/* O funil por estágio do Vórtice (documento 52)                             */
/* ------------------------------------------------------------------------ */

/** O período padrão das leituras do funil em 27/09/2026: o FY2026 até agosto. */
const PERIODO_PADRAO = { de: '2025-11-01', ate: '2026-08-31', texto: 'nov/2025 a ago/2026', ehOPadrao: true };

/**
 * O funil de uma filial. No `completo` a rotina PROCESSOS_VORTICE rodou; nos outros dois ela ainda não rodou — é o
 * que a produção mostra até alguém ligar a rotina —, e o funil volta vazio com o motivo.
 */
export function funilPorEstagio(estado: EstadoDaVisao360, codigo: string): FunilPorEstagio {
  const peso = pesoDaOperacao(estado, codigo);
  if (peso === 0) {
    return {
      periodo: PERIODO_PADRAO,
      base: 'abertura',
      estagios: [],
      paradosEmNegociacaoOuPedido: null,
      diasParaParado: 60,
      rotina: { nome: 'Funil e vendas perdidas do Vórtice', estaLigada: false, iniciadaEm: null, terminadaEm: null, resultado: null },
      metricasSemDado: [
        {
          metrica: 'funil',
          motivo:
            'A rotina "Funil e vendas perdidas do Vórtice" (PROCESSOS_VORTICE) ainda não rodou: ela nasce desligada, e quem administra a liga em Configurações › Integrações.',
        },
      ],
    };
  }

  const bases = [2_900, 2_440, 2_350, 780, 540, 330].map((v) => n(v * peso));
  const nomes = ['Lead', 'Qualificado', 'Cobertura', 'Negociação', 'Pedido', 'Faturamento'];
  const codigos = ['Lead', 'Qualificado', 'Cobertura', 'Negociacao', 'Pedido', 'Faturamento'] as const;
  return {
    periodo: PERIODO_PADRAO,
    base: 'abertura',
    estagios: codigos.map((estagio, i) => ({
      estagio,
      nome: nomes[i],
      processos: bases[i],
      percentualSobreOAnterior: i === 0 || bases[i - 1] === 0 ? null : Math.round((1000 * bases[i]) / bases[i - 1]) / 10,
      percentualSobreOLead: bases[0] === 0 ? null : Math.round((1000 * bases[i]) / bases[0]) / 10,
      pelaEntradaDigital: n(bases[i] * (i < 2 ? 0.24 : 0.03)),
      ganhos: n(bases[5] * 0.9),
      perdidos: n(bases[i] * 0.2),
      abertos: n(bases[i] * 0.5),
      outros: n(bases[i] * 0.05),
    })),
    paradosEmNegociacaoOuPedido: n(37 * peso),
    diasParaParado: 60,
    rotina: {
      nome: 'Funil e vendas perdidas do Vórtice',
      estaLigada: true,
      iniciadaEm: '2026-09-27T09:30:00Z',
      terminadaEm: '2026-09-27T09:30:12Z',
      resultado: 'Sucesso',
    },
    metricasSemDado: [],
  };
}

/* ------------------------------------------------------------------------ */
/* O trabalho do dia do CEN (28/09/2026)                                      */
/* ------------------------------------------------------------------------ */

/** Uma página na forma que a API devolve. */
function pagina<T>(itens: T[], total: number) {
  return { itens, pagina: 1, tamanho: itens.length, total, totalDePaginas: 1, temProxima: false };
}

/**
 * AS TAREFAS ATRASADAS DO PERFIL DO CEN — só no `completo`, com nomes que se declaram fictícios. Até 28/09/2026 o
 * harness não simulava `/v1/tarefas`, e o perfil do CEN só mostrava o erro "não simula".
 */
function tarefasAtrasadas(estado: EstadoDaVisao360) {
  if (estado !== 'completo') return pagina([], 0);
  const assuntos = ['Retorno da proposta do trator', 'Visita de pós-venda', 'Negociar o usado', 'Revisão das 500 horas'];
  return pagina(
    assuntos.map((assunto, i) => ({
      chave: `tarefa-ficticia-${i + 1}`,
      assunto,
      detalhe: null,
      tipoTarefaCodigo: 'VISITA',
      tipoTarefaNome: 'Visita',
      clienteChave: `cliente-ficticio-${i + 1}`,
      clienteNome: `Cliente Fictício ${['Alfa', 'Beta', 'Gama', 'Delta'][i]}`,
      processoChave: null,
      processoTitulo: null,
      responsavelNome: 'CEN Fictício Gama',
      agendadaPara: `2026-09-${String(10 + i).padStart(2, '0')}`,
      prazoLimite: null,
      prioridade: 2,
      situacao: 'Pendente',
      estaAtrasada: true,
      diasDeAtraso: 18 - i * 4,
      concluidaEm: null,
      resultadoNome: null,
    })),
    1_024,
  );
}

/** OS CLIENTES HÁ MAIS TEMPO SEM CONTATO — o nunca contatado primeiro, como a API ordena. */
function clientesSemContato(estado: EstadoDaVisao360) {
  if (estado !== 'completo') return pagina([], 0);
  const dias = [null, 412, 365, 290];
  return pagina(
    dias.map((diasSemContato, i) => ({
      clienteChave: `cliente-ficticio-${i + 11}`,
      clienteNome: `Produtor Fictício ${['Epsilon', 'Zeta', 'Eta', 'Teta'][i]}`,
      carteiraChave: 'carteira-ficticia-1',
      carteiraNome: 'Carteira Fictícia Norte',
      linhaDeNegocioNome: 'Máquinas e Implemento',
      classe: 'C',
      ultimaInteracaoEm: null,
      diasSemContato,
      diasCicloContato: 180,
      estaForaDoCiclo: true,
      responsavelNome: 'CEN Fictício Gama',
    })),
    48_360,
  );
}

/* ------------------------------------------------------------------------ */
/* Forecast, estoque e conferência com a GN (bloco 3 do #293, 29/09/2026)     */
/* ------------------------------------------------------------------------ */

const MESES_CURTOS = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/**
 * O FORECAST DA GERÊNCIA: três gestores, um com nome de regional inteiro, um forecast e um best guess não informados e
 * uma linha sem PO — o que a tela mostra com o traço. O total soma as linhas, e as vendas sem gestor entram só nele.
 */
function forecastDaGerencia(estado: EstadoDaVisao360, competencia: string | null): RelatorioDoForecast {
  const mes = competencia && /^\d{4}-\d{2}$/.test(competencia) ? `${competencia}-01` : '2026-09-01';
  const vazio = estado !== 'completo';
  const linha = (codigo: string, nome: string, meta: number, forecast: number | null, bestGuess: number | null, realizado: number) =>
    ({ codigo, nome, meta, forecast, bestGuess, realizado }) satisfies LinhaDoForecast;
  const gestores = vazio
    ? []
    : [
        {
          gestor: 'GESTOR FICTÍCIO DA REGIONAL NORTE PAULISTA',
          consultores: 6,
          linhas: [
            linha('TRATOR_MEDIO', 'TRATOR MÉDIO', 14, 12, 13, 9),
            linha('TRATOR_GRANDE', 'TRATOR GRANDE', 6, 5, 6, 4),
            linha('COLHEITADEIRA', 'COLHEITADEIRA', 3, null, 2, 1),
            linha('PULVERIZADOR', 'PULVERIZADOR', 2, 2, 2, 3),
          ],
        },
        {
          gestor: 'GESTOR FICTÍCIO BETA',
          consultores: 4,
          linhas: [linha('TRATOR_MEDIO', 'TRATOR MÉDIO', 10, 9, 8, 7), linha('PLANTADEIRA', 'PLANTADEIRA', 4, 3, null, 2)],
        },
        { gestor: 'GESTOR FICTÍCIO GAMA', consultores: 2, linhas: [linha('TRATOR_MEDIO', 'TRATOR MÉDIO', 0, null, null, 1)] },
      ];
  const vendasSemGestor = vazio ? 0 : 2;

  const soma = (a: number | null, b: number | null) => (a === null && b === null ? null : (a ?? 0) + (b ?? 0));
  const porCodigo = new Map<string, LinhaDoForecast>();
  for (const l of gestores.flatMap((g) => g.linhas)) {
    const atual = porCodigo.get(l.codigo);
    porCodigo.set(
      l.codigo,
      atual
        ? { ...atual, meta: atual.meta + l.meta, forecast: soma(atual.forecast, l.forecast), bestGuess: soma(atual.bestGuess, l.bestGuess), realizado: atual.realizado + l.realizado }
        : { ...l },
    );
  }
  const total = [...porCodigo.values()];
  if (total[0]) total[0] = { ...total[0], realizado: total[0].realizado + vendasSemGestor };

  return {
    competencia: mes,
    texto: `${MESES_CURTOS[Number(mes.slice(5, 7)) - 1]}/${mes.slice(0, 4)}`,
    mesesDisponiveis: vazio ? [] : ['2026-07-01', '2026-08-01', '2026-09-01'],
    alcance: 'Filiais',
    gestores,
    total,
    vendasSemGestor,
    lidoEm: vazio ? null : '2026-09-28T09:00:00Z',
    geradoNaOrigemEm: vazio ? null : '2026-09-28T04:30:00Z',
    metricasSemDado: [{ metrica: 'alcanceDasFiliais', motivo: 'O PO e o realizado são só da filial escolhida; o forecast é o do gestor inteiro.' }],
  };
}

/**
 * O ESTOQUE: onze máquinas em cinco grupos — reservada, parada há mais de 180 dias, usada, sem data de entrada, pedido à
 * fábrica com e sem chegada prevista, configuração longa. Os totais e os grupos saem da lista, como a API os soma.
 */
function estoqueECobertura(estado: EstadoDaVisao360): EstoqueECobertura {
  const vazio = estado !== 'completo';
  const maquina = (p: Partial<MaquinaNaLista> & { descricao: string; grupo: string }): MaquinaNaLista => ({
    filial: 'Filial Fictícia Alfa', configuracao: null, situacao: 'Estoque', tipo: 'MÁQUINA', ehUsado: false, anoModelo: '2026/2026',
    chassi: null, entradaEm: '2026-07-01', diasNoPatio: 89, chegadaPrevistaEm: null, faturamentoPrevistoEm: null, pago: true,
    reservado: false, ehPedidoAFabrica: false, situacaoNaFabrica: null, ...p,
  });
  const pedido = { situacao: 'PEDIDO', ehPedidoAFabrica: true, diasNoPatio: null, entradaEm: null, pago: false };
  const maquinas = vazio
    ? []
    : [
        maquina({ descricao: 'TRATOR FICTÍCIO 6155M', grupo: 'TRATOR 6000', chassi: '1FC6155MXPA000001', diasNoPatio: 12, entradaEm: '2026-09-16', configuracao: 'Cabine, transmissão AutoQuad Plus, pneus 520/85R42 duplados' }),
        maquina({ descricao: 'TRATOR FICTÍCIO 6190M', grupo: 'TRATOR 6000', reservado: true, chassi: '1FC6190MXPA000002', diasNoPatio: 203, entradaEm: '2026-03-09' }),
        maquina({ descricao: 'TRATOR FICTÍCIO 6125J', grupo: 'TRATOR 6000', ehUsado: true, anoModelo: '2021/2021', chassi: '1FC6125JXMA000003', diasNoPatio: 64, entradaEm: '2026-07-26', pago: false }),
        maquina({ descricao: 'TRATOR FICTÍCIO 7230J', grupo: 'TRATOR 7000/8000', chassi: '1FC7230JXPA000004', diasNoPatio: 41, entradaEm: '2026-08-18' }),
        maquina({ descricao: 'TRATOR FICTÍCIO 8R 340', grupo: 'TRATOR 7000/8000', reservado: true, chassi: '1FC8R340XPA000005', diasNoPatio: 18, entradaEm: '2026-09-10', filial: 'Filial Fictícia Gama do Interior Paulista' }),
        maquina({ descricao: 'COLHEITADEIRA FICTÍCIA S760', grupo: 'COLHEITADEIRA', chassi: '1FCS760XPA000006', diasNoPatio: 256, entradaEm: '2026-01-15' }),
        maquina({ descricao: 'COLHEITADEIRA FICTÍCIA S790', grupo: 'COLHEITADEIRA', ...pedido, chegadaPrevistaEm: '2026-11-30', situacaoNaFabrica: 'Em produção' }),
        maquina({ descricao: 'PULVERIZADOR FICTÍCIO M4040', grupo: 'PULVERIZADOR', situacao: 'Remessa', chassi: '1FCM4040XPA000008', diasNoPatio: 97, entradaEm: '2026-06-23' }),
        maquina({ descricao: 'PULVERIZADOR FICTÍCIO R4045', grupo: 'PULVERIZADOR', ...pedido, situacaoNaFabrica: 'Confirmado' }),
        maquina({ descricao: 'PLANTADEIRA FICTÍCIA 2130', grupo: 'PLANTADEIRA', situacao: 'Consignado', reservado: true, chassi: '1FC2130XPA000010', diasNoPatio: 145, entradaEm: '2026-05-06' }),
        maquina({ descricao: 'PLANTADEIRA FICTÍCIA 2150', grupo: 'PLANTADEIRA', diasNoPatio: null, entradaEm: null }),
      ];

  const conta = (lista: MaquinaNaLista[], f: (m: MaquinaNaLista) => boolean) => lista.filter(f).length;
  const noPatio = (lista: MaquinaNaLista[]) => lista.filter((m) => !m.ehPedidoAFabrica);
  const patio = noPatio(maquinas);
  const COBERTURA_DO_GRUPO: Record<string, number | null> = {
    'TRATOR 6000': 2.4, 'TRATOR 7000/8000': 3.1, COLHEITADEIRA: 5.8, PULVERIZADOR: 1.9, PLANTADEIRA: null,
  };
  const porGrupo = [...new Set(maquinas.map((m) => m.grupo))].map((grupo) => {
    const doGrupo = maquinas.filter((m) => m.grupo === grupo);
    const doPatio = noPatio(doGrupo);
    const dias = doPatio.map((m) => m.diasNoPatio).filter((d): d is number => d !== null);
    return {
      grupo,
      noPatio: doPatio.length,
      disponiveis: conta(doPatio, (m) => !m.reservado),
      reservadas: conta(doPatio, (m) => m.reservado),
      pedidosAFabrica: conta(doGrupo, (m) => m.ehPedidoAFabrica),
      idadeMediaEmDias: dias.length > 0 ? Math.round(dias.reduce((s, d) => s + d, 0) / dias.length) : null,
      coberturaEmMeses: COBERTURA_DO_GRUPO[grupo] ?? null,
    };
  });

  const media = (valores: number[]) => (valores.length > 0 ? valores.reduce((s, v) => s + v, 0) / valores.length : null);
  const porMes = vazio
    ? []
    : [3.8, 3.4, 3.1, 2.9, 3.6, 3.2].map((meses, i) => ({
        chave: `2026-0${i + 3}`,
        competencia: `2026-0${i + 3}-01`,
        meses,
        vendas: [131, 148, 162, 171, 139, 152][i]!,
      }));
  const coberturaPorGrupo = vazio
    ? []
    : Object.entries(COBERTURA_DO_GRUPO)
        .filter((par): par is [string, number] => par[1] !== null)
        .map(([chave, meses]) => ({ chave, competencia: null, meses, vendas: Math.round(40 / meses) * 6 }));

  return {
    alcance: 'Filiais',
    hoje: '2026-09-28',
    totais: {
      noPatio: patio.length,
      disponiveis: conta(patio, (m) => !m.reservado),
      reservadas: conta(patio, (m) => m.reservado),
      pagas: conta(patio, (m) => m.pago),
      maisDe180Dias: conta(patio, (m) => (m.diasNoPatio ?? 0) > 180),
      pedidosAFabrica: conta(maquinas, (m) => m.ehPedidoAFabrica),
    },
    porGrupo,
    maquinas,
    cobertura: {
      porMes,
      porGrupo: coberturaPorGrupo,
      mediaPorMes: media(porMes.map((m) => m.meses)),
      mediaPorGrupo: media(coberturaPorGrupo.map((g) => g.meses)),
      lidaEm: vazio ? null : '2026-09-28T12:00:00Z',
      geradaNaOrigemEm: null,
    },
    lidoEm: vazio ? null : '2026-09-28T12:00:00Z',
    geradoNaOrigemEm: null,
    metricasSemDado: [{ metrica: 'valor', motivo: 'O valor do estoque não aparece: está a custo no TOTVS, e o CRM não lê custo.' }],
  };
}

/**
 * A CONFERÊNCIA COM A GN: cinco filiais, duas com diferença, seis meses e uma máquina de cada um dos seis tipos de
 * divergência — com os rótulos que a API escreve.
 */
function conferenciaComAGestao(estado: EstadoDaVisao360): ConferenciaComAGestao {
  const TIPOS: [string, string][] = [
    ['RealizadoSoNaGestao', 'Só na Gestão de Negócios'],
    ['RealizadoPendenteNoArt', 'Pendente na integração do ART'],
    ['RealizadoNaoEntregueNoCrm', 'Sem a entrega no CRM'],
    ['RealizadoEmOutraFilial', 'Em outra filial'],
    ['RealizadoEmOutroMes', 'Em outro mês'],
    ['RealizadoSoNoCrm', 'Só no CRM'],
  ];
  const regua = {
    metrica: 'regua',
    motivo: 'Os dois lados contam pela mesma régua: a meta sem consórcio, e o realizado só com a máquina entregue, no mês da entrega.',
  };
  const numeros = (metaNaGestao: number, metaNoCrm: number, realizadoNaGestao: number, realizadoNoCrm: number) => ({
    metaNaGestao, metaNoCrm, realizadoNaGestao, realizadoNoCrm,
  });

  if (estado !== 'completo')
    return {
      alcance: 'Filiais',
      totais: numeros(0, 0, 0, 0),
      porFilial: [],
      porMes: [],
      porTipo: TIPOS.map(([tipo, rotulo]) => ({ tipo, rotulo, quantidade: 0 })),
      divergencias: [],
      apuradaEm: null,
      geradaNaOrigemEm: null,
      metricasSemDado: [
        {
          metrica: 'naoApurada',
          motivo:
            'A conferência ainda não foi apurada: ela vem da rotina "Conferência com a Gestão de Negócios". Sem ela, a tela fica vazia — e não quer dizer que os números batem.',
        },
        regua,
      ],
    };

  const porFilial = [
    { filial: 'Filial Fictícia Alfa', numeros: numeros(48, 48, 41, 39) },
    { filial: 'Filial Fictícia Beta', numeros: numeros(36, 36, 30, 30) },
    { filial: 'Filial Fictícia Delta', numeros: numeros(20, 20, 15, 16) },
    { filial: 'Filial Fictícia Épsilon', numeros: numeros(12, 12, 9, 9) },
    { filial: 'Filial Fictícia Gama do Interior Paulista', numeros: numeros(28, 27, 22, 21) },
  ];
  const porMes = [
    [24, 24, 18, 18],
    [24, 24, 21, 20],
    [24, 24, 19, 19],
    [24, 23, 20, 19],
    [24, 24, 17, 17],
    [24, 24, 22, 22],
  ].map(([a, b, c, d], i) => ({ competencia: `2026-0${i + 3}-01`, numeros: numeros(a!, b!, c!, d!) }));
  const somar = (campo: keyof ReturnType<typeof numeros>) => porFilial.reduce((s, f) => s + f.numeros[campo], 0);

  const divergencia = (tipo: string, chassi: string, filial: string, descricao: string, noCrm: string | null, naGestao: string | null) => ({
    tipo, rotulo: TIPOS.find(([t]) => t === tipo)![1], chassi, filial, descricao, noCrm, naGestao, detectadaEm: '2026-09-28T10:00:00Z',
  });
  const divergencias = [
    divergencia('RealizadoSoNaGestao', '1FC6155MXPA100001', 'Filial Fictícia Alfa', 'A GN conta a máquina como entregue, e o CRM não tem a venda dela.', null, 'Filial Fictícia Alfa · 2026-08'),
    divergencia('RealizadoPendenteNoArt', '1FC7230JXPA100002', 'Filial Fictícia Alfa', 'A venda está no ART, mas a integração a deixou pendente: o comprador não está no CRM.', 'pendente: COMPRADOR_AUSENTE_NO_CRM', 'Filial Fictícia Alfa · 2026-07'),
    divergencia('RealizadoNaoEntregueNoCrm', '1FC8R340XPA100005', 'Filial Fictícia Delta', 'O CRM tem a venda, mas sem a data de entrega.', 'venda sem entrega', 'Filial Fictícia Delta · 2026-08'),
    divergencia('RealizadoEmOutraFilial', '1FCS760XPA100003', 'Filial Fictícia Gama do Interior Paulista', 'O CRM conta a máquina em outra filial.', 'Filial Fictícia Delta · 2026-06', 'Filial Fictícia Gama do Interior Paulista · 2026-06'),
    divergencia('RealizadoEmOutroMes', '1FCM4040XPA100004', 'Filial Fictícia Beta', 'O CRM conta a entrega em outro mês.', 'Filial Fictícia Beta · 2026-05', 'Filial Fictícia Beta · 2026-04'),
    divergencia('RealizadoSoNoCrm', '1FC2130XPA100006', 'Filial Fictícia Delta', 'Só o CRM conta a máquina como entregue.', 'Filial Fictícia Delta · 2026-06', null),
  ];

  return {
    alcance: 'Filiais',
    totais: numeros(somar('metaNaGestao'), somar('metaNoCrm'), somar('realizadoNaGestao'), somar('realizadoNoCrm')),
    porFilial,
    porMes,
    porTipo: TIPOS.map(([tipo, rotulo]) => ({ tipo, rotulo, quantidade: divergencias.filter((d) => d.tipo === tipo).length })),
    divergencias,
    apuradaEm: '2026-09-28T10:15:00Z',
    geradaNaOrigemEm: null,
    metricasSemDado: [regua],
  };
}

/* ------------------------------------------------------------------------ */
/* A rota → a amostra                                                         */
/* ------------------------------------------------------------------------ */
/**
 * A AMOSTRA DE CADA ROTA QUE A VISÃO 360 LÊ, pelo caminho da API — ou
 * `undefined` para a rota que ninguém simulou.
 *
 * Mora aqui, e não no harness, para os testes de componente usarem as MESMAS
 * respostas sem importar o harness: ele instala um interceptador de `fetch` ao
 * carregar e traz a aplicação inteira junto.
 */
export function respostaDaVisao360(
  caminho: string,
  consulta: URLSearchParams,
  empresa: string,
  estado: EstadoDaVisao360,
): unknown {
  switch (caminho) {
    case '/v1/catalogos/EMPRESA':
      return catalogoDeEmpresas(estado);
    case '/v1/relatorios/forecast':
      return forecastDaGerencia(estado, consulta.get('competencia'));
    case '/v1/relatorios/estoque':
      return estoqueECobertura(estado);
    case '/v1/integracoes/conferencia-gn':
      return conferenciaComAGestao(estado);
    case '/v1/relatorios/cobertura':
      return coberturaPorCarteira(estado, empresa);
    case '/v1/relatorios/agenda':
      return painelDaAgenda(estado, empresa);
    case '/v1/relatorios/funil':
      return fasesDoFunil(estado, empresa);
    case '/v1/relatorios/perdas':
      return perdasPorMotivo(estado, empresa);
    case '/v1/relatorios/vendas-perdidas':
      return vendasPerdidas(estado, empresa);
    case '/v1/relatorios/funil-por-estagio':
      return funilPorEstagio(estado, empresa);
    case '/v1/relatorios/faturamento':
      return faturamento(estado, empresa);
    case '/v1/relatorios/indicadores-executivos':
      return indicadoresExecutivos(estado, empresa, Number(consulta.get('anoFiscal') ?? 2026));
    case '/v1/relatorios/metas':
      return metasDeVenda(estado, empresa);
    case '/v1/cobertura/filiais':
      return coberturaPorFilial(estado, empresa);
    case '/v1/cobertura/carteiras':
      return territorioDasCarteiras(estado, empresa);
    case '/v1/relatorios/cen':
      return painelDoCen(estado, empresa, consulta.get('responsavel'));
    case '/v1/processos':
      return contagemDeProcessos(estado, empresa, consulta.get('situacao') ?? '', Number(consulta.get('tamanho') ?? '1'));
    case '/v1/tarefas':
      return tarefasAtrasadas(estado);
    case '/v1/cobertura':
      return clientesSemContato(estado);
    default:
      return undefined;
  }
}
