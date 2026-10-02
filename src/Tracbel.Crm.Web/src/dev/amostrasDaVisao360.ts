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
  // "TODAS AS FILIAIS" (30/09/2026, #313): o servidor soma todas numa leitura só; aqui, uma filial com o peso de todas.
  if (codigo === 'TODAS') return { codigo, nome: 'Todas as filiais', peso: lista.reduce((s, f) => s + f.peso, 0) };
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

/**
 * DEZ CENs FICTÍCIOS, como a maquete da Performance de CEN (01/10/2026): primeiro nome de letra grega, para ninguém
 * confundir com gente de verdade, e o resto no formato dos nomes do Vórtice — a tela abrevia em "Alfa M. Cruz".
 */
export const CENS = [
  'ALFA MATIAS DE ARAUJO CRUZ',
  'BETA ITALIA JARDIM SILVA',
  'GAMA LUZIA FERRAZ MELO',
  'DELTA CAIO RAMOS ALMEIDA',
  'ÉPSILON RAFA SANTANA LIMA',
  'ZETA JOAO PRADO OLIVEIRA',
  'ETA FERNANDA TAVARES SANTOS',
  'TETA RICO ANDRADE SANTOS',
  'IOTA LUCAS MOREIRA RIBEIRO',
  'CAPA JULIANA CARVALHO COSTA',
] as const;

/**
 * As faixas de cada carteira comercial, em fração dos vínculos: até 30 dias, de 31 a 90, mais de 90 e nunca — as dez
 * primeiras são as linhas da maquete; as duas últimas são a segunda carteira de Alfa e de Beta, mais descobertas.
 */
const FAIXAS_DAS_CARTEIRAS: readonly [em30: number, de31a90: number, acima90: number, nunca: number][] = [
  [0.68, 0.18, 0.1, 0.04],
  [0.62, 0.22, 0.12, 0.04],
  [0.58, 0.26, 0.12, 0.04],
  [0.52, 0.28, 0.16, 0.04],
  [0.48, 0.3, 0.18, 0.04],
  [0.45, 0.32, 0.2, 0.03],
  [0.42, 0.36, 0.18, 0.04],
  [0.38, 0.34, 0.24, 0.04],
  [0.34, 0.4, 0.22, 0.04],
  [0.28, 0.38, 0.3, 0.04],
  [0.24, 0.3, 0.26, 0.2],
  [0.14, 0.28, 0.3, 0.28],
];

/**
 * A CLASSE DA CURVA ABC NA AMOSTRA (o filtro da Performance de CEN, 01/10/2026): a fração dos vínculos de cada classe e
 * o quanto ela é mais (ou menos) contatada que a média — a A mais coberta, a D menos e mais nunca contatada.
 */
const CLASSES_DA_AMOSTRA: Record<string, { parte: number; contato: number; nunca: number }> = {
  A: { parte: 0.09, contato: 1.3, nunca: 0.5 },
  B: { parte: 0.15, contato: 1.15, nunca: 0.7 },
  C: { parte: 0.31, contato: 1, nunca: 1 },
  D: { parte: 0.45, contato: 0.9, nunca: 1.15 },
};

/** Os números de uma carteira com as quatro faixas, na classe pedida — ou em todas. */
function vinculosDaCarteira(
  clientesDaCarteira: number,
  [em30, de31a90, , nunca]: readonly [number, number, number, number],
  classe: string | null,
) {
  const recorte = classe ? CLASSES_DA_AMOSTRA[classe] : undefined;
  const clientes = n(clientesDaCarteira * (recorte?.parte ?? 1));
  const fracaoNunca = Math.min(0.9, nunca * (recorte?.nunca ?? 1));
  const fracao30 = Math.min(1 - fracaoNunca, em30 * (recorte?.contato ?? 1));
  const fracao90 = Math.min(1 - fracaoNunca, fracao30 + de31a90 * (recorte?.contato ?? 1));
  return {
    clientes,
    comContatoEm30Dias: n(clientes * fracao30),
    comContatoEm90Dias: n(clientes * fracao90),
    nuncaContatados: n(clientes * fracaoNunca),
  };
}

export function coberturaPorCarteira(
  estado: EstadoDaVisao360,
  codigo: string,
  classe: string | null = null,
): Agregado<ResumoDeCobertura> {
  if (semVinculo(estado)) return { itens: [], metricasSemDado: [] };

  const { peso } = filial(estado, codigo);
  const parte = classe ? (CLASSES_DA_AMOSTRA[classe]?.parte ?? 1) : 1;
  const itens: ResumoDeCobertura[] = [];

  FAIXAS_DAS_CARTEIRAS.forEach((faixas, i) => {
    itens.push({
      carteiraChave: `${codigo}-c${i}`,
      carteiraCodigo: `C${codigo.slice(-2)}${String(i).padStart(2, '0')}`,
      carteiraNome: `Carteira fictícia ${i + 1}`,
      linhaDeNegocioNome: LINHAS[i % LINHAS.length],
      responsavelNome: CENS[i % CENS.length],
      ...vinculosDaCarteira((1400 - i * 90) * peso, faixas, classe),
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
    ...vinculosDaCarteira(900 * peso, [0.13, 0.16, 0.25, 0.46], classe),
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
      clientes: n(9000 * peso * parte),
      comContatoEm30Dias: 0,
      comContatoEm90Dias: 0,
      nuncaContatados: n(9000 * peso * parte),
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
      clientes: n(2500 * peso * parte),
      comContatoEm30Dias: 0,
      comContatoEm90Dias: 0,
      nuncaContatados: n(2500 * peso * parte),
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

/** Uma fase da amostra: código, nome, processos, quantos declaram valor e a soma do que declaram. */
type FaseDaAmostra = readonly [codigo: string, nome: string, processos: number, comValor: number, valor: number];

/**
 * O FUNIL DA MAQUETE DO PIPELINE (30/09/2026): três fluxos e 24 pares fluxo × fase. O de venda tem as 15 fases da imagem,
 * com os nomes dela — é pelo nome que a tela escolhe a cor —, algumas com valor declarado e a maioria sem; os outros dois
 * completam a conta. Somados, 13.838 abertos e 653 com valor, como na maquete; cada filial leva a sua fração.
 */
const FLUXOS_DA_AMOSTRA: { codigo: string; nome: string; fases: FaseDaAmostra[] }[] = [
  {
    codigo: 'VENDA',
    nome: 'Venda de equipamento (amostra)',
    fases: [
      ['NAO_INICIADO', 'Não iniciado', 1, 0, 0],
      ['APRESENTACAO', 'Apresentação', 7_234, 8, 1_475_830],
      ['MONITORAMENTO', 'Monitoramento', 129, 0, 0],
      ['FINALIZADO', 'Finalizado', 2, 0, 0],
      ['NEGOCIACAO', 'Negociação', 186, 19, 539_387],
      ['PEDIDO_DE_VENDA', 'Pedido de Venda', 18, 0, 0],
      ['MONTAGEM', 'Montagem', 120, 23, 39_378_912],
      ['ANALISE', 'Análise', 3, 0, 0],
      ['APROVACAO', 'Aprovação', 87, 8, 989_263],
      ['FORMALIZACAO', 'Formalização', 1, 0, 0],
      ['AUTORIZACAO', 'Autorização', 6, 0, 0],
      ['FATURAMENTO', 'Faturamento', 99, 16, 184_266],
      ['RECEBIMENTO', 'Recebimento', 1, 0, 0],
      ['PREPARACAO', 'Preparação', 8, 0, 0],
      ['ENTREGA', 'Entrega', 8, 2, 4_312_000],
    ],
  },
  {
    codigo: 'PROSPECCAO',
    nome: 'Prospecção de clientes (amostra)',
    fases: [
      ['PROSPECCAO', 'Prospecção', 5_420, 540, 12_800_000],
      ['QUALIFICACAO', 'Qualificação', 214, 21, 3_150_000],
      ['VISITA', 'Visita agendada', 96, 9, 870_000],
      ['PROPOSTA', 'Proposta', 41, 4, 1_240_000],
      ['RETORNO', 'Retorno', 27, 0, 0],
      ['DESCARTE', 'Descarte', 12, 0, 0],
    ],
  },
  {
    codigo: 'POS_VENDA',
    nome: 'Pós-venda (amostra)',
    fases: [
      ['ENTREGA_TECNICA', 'Entrega técnica', 88, 3, 96_000],
      ['ACOMPANHAMENTO', 'Acompanhamento', 31, 0, 0],
      ['ENCERRAMENTO', 'Encerramento', 6, 0, 0],
    ],
  },
];

export function fasesDoFunil(estado: EstadoDaVisao360, codigo: string): Agregado<FaseDoFunil> {
  const peso = pesoDaOperacao(estado, codigo);
  if (peso === 0) return { itens: [], metricasSemDado: [] };

  const itens: FaseDoFunil[] = FLUXOS_DA_AMOSTRA.flatMap((fluxo) =>
    fluxo.fases.map(([faseCodigo, faseNome, processos, comValor, valor], i) => {
      // NENHUMA FASE SOME NA FILIAL PEQUENA: o agrupamento do banco não devolve fase com zero processo.
      const naFilial = Math.max(1, n(processos * peso));
      const declarando = Math.min(naFilial, n(comValor * peso));
      return {
        tipoProcessoCodigo: fluxo.codigo,
        tipoProcessoNome: fluxo.nome,
        faseCodigo,
        faseNome,
        faseOrdem: i + 1,
        processos: naFilial,
        processosComValor: declarando,
        valorTotal: declarando > 0 ? n(valor * peso) : null,
      };
    }),
  );

  // O MOTIVO COM O TEXTO DA API (`ObterFunil`, em português): nenhum valor, ou menos de 10% dos abertos declarando.
  const processos = itens.reduce((s, f) => s + f.processos, 0);
  const comValor = itens.reduce((s, f) => s + f.processosComValor, 0);
  const pct = ((comValor * 100) / processos).toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
  const metricasSemDado =
    comValor === 0
      ? [
          {
            metrica: 'valorDoFunil',
            motivo: `Nenhum dos ${processos.toLocaleString('pt-BR')} processos abertos declara valor. O sistema de origem tem a coluna e praticamente não a preenche; somar zeros e apresentar o total como valor do funil mostraria um número que não representa negócio nenhum.`,
          },
        ]
      : (comValor * 100) / processos < 10
        ? [
            {
              metrica: 'valorDoFunilConfiavel',
              motivo: `Só ${comValor.toLocaleString('pt-BR')} de ${processos.toLocaleString('pt-BR')} processos abertos declaram valor (${pct}%). O total vem preenchido, mas ele representa essa fração — não o funil inteiro.`,
            },
          ]
        : [];

  return { itens, metricasSemDado };
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

/** O dia de referência do harness — o mesmo do "Dados atualizados em 24/09/2026". */
const HOJE_DA_AMOSTRA = Date.UTC(2026, 8, 24);

/** Os dias parados das linhas da amostra, do mais antigo para o mais novo — a ordem padrão da lista. */
const DIAS_PARADOS_DA_AMOSTRA = [1_302, 1_063, 1_063, 1_063, 1_063, 845, 610, 402, 233, 140, 95, 37];

/** As fases das linhas sem filtro — de fluxos diferentes, como a lista da maquete. */
const FASES_DAS_LINHAS: readonly [fluxo: string, fase: string][] = [
  ['VENDA', 'MONITORAMENTO'],
  ['PROSPECCAO', 'PROSPECCAO'],
  ['PROSPECCAO', 'PROSPECCAO'],
  ['VENDA', 'APRESENTACAO'],
  ['PROSPECCAO', 'QUALIFICACAO'],
  ['VENDA', 'NEGOCIACAO'],
];

/**
 * Os processos por situação. A Visão 360 pede UMA linha e só lê o `total`; o Pipeline (29/09/2026, harness com `rota=`)
 * pede uma página, e ganha linhas fictícias — com e sem valor declarado, parados há pouco e há muito — para o desenho
 * da tabela ser conferido com dado.
 *
 * DESDE A MAQUETE DO PIPELINE (30/09/2026) a amostra respeita o fluxo, a fase e a busca, como a API depois da P-7: o total
 * aberto é o do funil acima, e o de uma fase é o da fase.
 */
export function contagemDeProcessos(estado: EstadoDaVisao360, codigo: string, consulta: URLSearchParams): PaginaDe<ProcessoResumo> {
  const peso = pesoDaOperacao(estado, codigo);
  const situacao = consulta.get('situacao') ?? '';
  const tamanho = Number(consulta.get('tamanho') ?? '1');
  const fluxoPedido = consulta.get('tipoProcessoCodigo') ?? '';
  const fasePedida = consulta.get('faseCodigo') ?? '';
  const termo = (consulta.get('termo') ?? '').trim().toLowerCase();

  const fases = fasesDoFunil(estado, codigo).itens;
  const daFase = (fluxo: string, fase: string) => fases.find((f) => f.tipoProcessoCodigo === fluxo && f.faseCodigo === fase)!;
  const escolhidas = fases.filter(
    (f) => (!fluxoPedido || f.tipoProcessoCodigo === fluxoPedido) && (!fasePedida || f.faseCodigo === fasePedida),
  );
  const abertos = escolhidas.reduce((s, f) => s + f.processos, 0);
  const filtrado = fluxoPedido !== '' || fasePedida !== '';

  const totalDaSituacao =
    situacao === 'Ganho'
      ? n(260 * peso)
      : situacao === 'Perdido'
        ? n(310 * peso)
        : situacao === 'Aberto'
          ? abertos
          : // SEM SITUAÇÃO, a lista traz os abertos e os suspensos, como a API.
            abertos + (filtrado ? 0 : n(9 * peso));

  const quantas = tamanho > 1 ? Math.min(tamanho, totalDaSituacao, DIAS_PARADOS_DA_AMOSTRA.length) : 0;
  const todas: ProcessoResumo[] = Array.from({ length: quantas }, (_, i) => {
    const fase = filtrado ? escolhidas[i % escolhidas.length] : daFase(...FASES_DAS_LINHAS[i % FASES_DAS_LINHAS.length]);
    const dias = DIAS_PARADOS_DA_AMOSTRA[i];
    const numero = 48_210 + i;
    const deVenda = fase.tipoProcessoCodigo === 'VENDA';
    return {
      chave: `00000000-0000-4000-8000-${String(900 + i).padStart(12, '0')}`,
      numero,
      titulo: deVenda ? `Trator fictício ${6110 + i * 5}J — nº ${numero}` : `Prospecção de amostra — nº ${numero}`,
      clienteChave: `00000000-0000-4000-8000-${String(700 + i).padStart(12, '0')}`,
      clienteNome: `PRODUTOR FICTÍCIO ${['ALFA', 'BETA', 'GAMA', 'DELTA', 'ÉPSILON', 'ZETA'][i % 6]}`,
      tipoProcessoCodigo: fase.tipoProcessoCodigo,
      tipoProcessoNome: fase.tipoProcessoNome,
      faseCodigo: fase.faseCodigo,
      faseNome: fase.faseNome,
      faseOrdem: fase.faseOrdem,
      faseDesde: new Date(HOJE_DA_AMOSTRA - dias * 86_400_000).toISOString().slice(0, 10),
      diasNaFase: dias,
      situacao: situacao || 'Aberto',
      valorEstimado: i % 4 === 3 ? 485_000 + i * 12_500 : null,
      previsaoConclusao: i % 5 === 4 ? '2026-11-30' : null,
      proprietarioNome: `CEN FICTÍCIO ${['NORTE', 'SUL', 'LESTE'][i % 3]}`,
      criadoEm: '2023-03-02T10:00:00Z',
    };
  });

  const itens = termo
    ? todas.filter((p) => p.titulo.toLowerCase().includes(termo) || String(p.numero) === termo)
    : todas;
  const total = termo ? itens.length : totalDaSituacao;
  const porPagina = Math.max(1, tamanho);
  return {
    itens,
    pagina: 1,
    tamanho: porPagina,
    total,
    totalDePaginas: Math.max(1, Math.ceil(total / porPagina)),
    temProxima: total > porPagina,
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
 * As carteiras da filial com as cidades que atendem.
 *
 * SÃO AS MESMAS CARTEIRAS DA COBERTURA DE CONTATO (`coberturaPorCarteira`), com a mesma chave e o mesmo nome — em
 * produção as duas rotas partem de `organizacao.Carteira`, e a Cobertura por Filial cruza uma com a outra pela chave
 * (os filtros de filial, carteira, município e estado recortam também o medidor e as barras, 02/10/2026). No `completo`,
 * as carteiras de campo declaram cidade, e algumas não — além do depósito de cadastro e da de teste —: a lacuna do
 * cadastro de origem, que a tela mostra com zero em vez de esconder.
 */
export function territorioDasCarteiras(estado: EstadoDaVisao360, codigo: string): Agregado<TerritorioDeCarteira> {
  if (semVinculo(estado)) {
    return { itens: [], metricasSemDado: [{ metrica: 'carteiras', motivo: 'Nenhuma carteira desta filial foi carregada ainda.' }] };
  }
  const f = filial(estado, codigo);
  // AS CIDADES DE CADA CARTEIRA, pela posição: quantas e a partir de qual — as de campo se sobrepõem, como na vida real.
  const CIDADES_POR_POSICAO = [7, 5, 9, 3, 6, 4, 0, 8, 2, 5, 0, 3];
  const itens = coberturaPorCarteira(estado, codigo).itens.map((c, i) => {
    const cidades = c.naturezaDaCarteira !== 'Comercial' ? 0 : c.carteiraChave.endsWith('-im') ? 10 : (CIDADES_POR_POSICAO[i] ?? 0);
    return {
      carteiraChave: c.carteiraChave,
      carteiraCodigo: c.carteiraCodigo,
      carteiraNome: c.carteiraNome,
      linhaDeNegocioNome: c.linhaDeNegocioNome,
      responsavelNome: c.responsavelNome,
      empresaCodigo: f.codigo,
      empresaNome: f.nome,
      municipios: MUNICIPIOS_FICTICIOS.slice(i % 5, (i % 5) + cidades),
    };
  });
  const semCidade = itens.filter((c) => c.municipios.length === 0).length;
  return {
    itens,
    metricasSemDado: [
      {
        metrica: 'carteirasSemMunicipio',
        motivo: `${semCidade} de ${itens.length} carteiras não declaram nenhuma cidade no sistema de origem e aparecem com zero.`,
      },
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

/**
 * Cada classe contra a cadência declarada, em fração dos vínculos dela — coberta, fora da cadência, nunca contatada e
 * sem cadência —, nas proporções da maquete da Performance (01/10/2026). A cadência é a da amostra antiga: 180 dias
 * para A e B, a C em linhas de prazos diferentes, 360 para a D.
 */
const COBERTURA_POR_CLASSE_DA_AMOSTRA: readonly [classe: string, cob: number, fora: number, nunca: number, sem: number, dias: number | null][] = [
  ['A', 0.62, 0.18, 0.12, 0.08, 180],
  ['B', 0.48, 0.22, 0.2, 0.1, 180],
  ['C', 0.28, 0.34, 0.26, 0.12, null],
  ['D', 0.14, 0.32, 0.42, 0.12, 360],
];

/** A chave fictícia de um responsável, pela posição dele na lista da filial. */
const chaveDoResponsavel = (i: number) => `00000000-0000-4000-8000-${(0xc00 + i).toString(16).padStart(12, '0')}`;

/**
 * O painel de um responsável — ou o consolidado, sem responsável —, com a cobertura por classe da curva ABC contra a
 * cadência declarada. No `completo` as quatro classes têm vínculo; nos outros dois, nenhum responsável tem carteira.
 *
 * DESDE A MAQUETE DA PERFORMANCE (01/10/2026) os responsáveis e os vínculos saem das MESMAS carteiras de
 * `coberturaPorCarteira` — nomes, chaves e totais batem com os cartões —, e a carteira escolhida recorta o painel.
 */
export function painelDoCen(
  estado: EstadoDaVisao360,
  codigo: string,
  responsavel: string | null,
  carteira: string | null = null,
): PainelDoCen {
  const comerciais = coberturaPorCarteira(estado, codigo).itens.filter(
    (c) => c.naturezaDaCarteira === 'Comercial' && (c.naturezaDoResponsavel === 'Pessoa' || c.naturezaDoResponsavel === 'Departamento'),
  );
  const nomes = [...new Set(comerciais.map((c) => c.responsavelNome))];
  const responsaveis = nomes.map((nome, i) => {
    const dele = comerciais.filter((c) => c.responsavelNome === nome);
    return { chave: chaveDoResponsavel(i), nome, natureza: dele[0].naturezaDoResponsavel, carteiras: dele.length };
  });

  const escolhido = responsaveis.find((r) => r.chave === responsavel) ?? null;
  const noRecorte = comerciais.filter(
    (c) => (!escolhido || c.responsavelNome === escolhido.nome) && (!carteira || c.carteiraChave === carteira),
  );
  const vinculos = noRecorte.reduce((s, c) => s + c.clientes, 0);
  const total = comerciais.reduce((s, c) => s + c.clientes, 0);

  const porClasse = COBERTURA_POR_CLASSE_DA_AMOSTRA.map(([classe, cob, fora, nunca, , dias]) => {
    const daClasse = n(vinculos * (CLASSES_DA_AMOSTRA[classe]?.parte ?? 0));
    const cobertos = n(daClasse * cob);
    const foraDaCadencia = n(daClasse * fora);
    const nuncaContatados = n(daClasse * nunca);
    // O RESTO, para as quatro somarem a classe: a sobra do arredondamento cai aqui, e nunca abaixo de zero.
    const semCadenciaDeclarada = Math.max(0, daClasse - cobertos - foraDaCadencia - nuncaContatados);
    return {
      classe,
      clientes: cobertos + foraDaCadencia + nuncaContatados + semCadenciaDeclarada,
      cobertos,
      foraDaCadencia,
      nuncaContatados,
      semCadenciaDeclarada,
      diasDeCadencia: dias,
    };
  }).filter((c) => c.clientes > 0);

  return {
    painel: {
      responsavelChave: escolhido?.chave ?? '',
      responsavelNome: escolhido?.nome ?? 'Todos os responsáveis',
      naturezaDoResponsavel: escolhido?.natureza ?? 'Pessoa',
      carteiras: noRecorte.length,
      clientes: porClasse.reduce((s, c) => s + c.clientes, 0),
      porClasse,
      processosGanhos: 0,
      processosPerdidos: 0,
      processosAbertos: 0,
      vendasPerdidasRegistradas: 0,
      faturamentoDaCarteira: total > 0 ? n(18_400_000 * filial(estado, codigo).peso * (vinculos / total)) : 0,
    },
    responsaveis,
    metricasSemDado:
      comerciais.length === 0
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
      // MÊS A MÊS (29/09/2026): os doze meses até setembro, o mês em curso e parcial; de novembro a agosto somam os ~250
      // do ano. Não depende do ano escolhido, como na rota.
      entreguesPorMes: ENTREGAS_DOS_DOZE_MESES.map(([mes, maquinas]) =>
        entregues(mes, mes, vazio ? 0 : maquinas * peso, mes === '2026-09-01' ? 510_000 : 520_000),
      ),
    },
    metricasSemDado: [],
  };
}

/** As entregas de cada um dos doze meses da amostra, com a sazonalidade de uma revenda de máquina. */
const ENTREGAS_DOS_DOZE_MESES: [string, number][] = [
  ['2025-10-01', 22],
  ['2025-11-01', 28],
  ['2025-12-01', 20],
  ['2026-01-01', 18],
  ['2026-02-01', 21],
  ['2026-03-01', 26],
  ['2026-04-01', 27],
  ['2026-05-01', 24],
  ['2026-06-01', 25],
  ['2026-07-01', 28],
  ['2026-08-01', 31],
  ['2026-09-01', 13],
];

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

/**
 * OS CLIENTES HÁ MAIS TEMPO SEM CONTATO — o nunca contatado primeiro, como a API ordena. Desde a maquete da Cobertura
 * (30/09/2026), com a curva ABC do cliente e a prioridade que sai dela, e a busca por cliente, carteira ou responsável.
 */
function clientesSemContato(estado: EstadoDaVisao360, consulta: URLSearchParams) {
  if (estado !== 'completo') return pagina([], 0);
  const linhas: [string, number | null, string | null][] = [
    ['Epsilon', null, 'A'],
    ['Zeta', null, 'A'],
    ['Eta', null, 'B'],
    ['Teta', null, null],
    ['Iota', 412, 'A'],
    ['Capa', 365, 'B'],
    ['Lambda', 290, 'C'],
    ['Mi', 210, 'D'],
    ['Ni', 95, 'B'],
    ['Xi', 40, 'A'],
  ];
  const prioridade = (classe: string | null) => (classe === 'A' ? 'Alta' : classe === 'B' ? 'Media' : 'Baixa');
  const itens = linhas.map(([nome, diasSemContato, classeDoCliente], i) => ({
    clienteChave: `cliente-ficticio-${i + 11}`,
    clienteNome: `PRODUTOR FICTÍCIO ${nome.toUpperCase()}`,
    carteiraChave: `carteira-ficticia-${(i % 2) + 1}`,
    carteiraNome: i % 2 === 0 ? 'Carteira fictícia 1' : 'Carteira fictícia 2',
    linhaDeNegocioNome: 'Venda de Máquinas e Implementos',
    classe: 'C',
    ultimaInteracaoEm:
      diasSemContato === null ? null : new Date(Date.UTC(2026, 8, 24) - diasSemContato * 86_400_000).toISOString(),
    diasSemContato,
    diasCicloContato: 180,
    estaForaDoCiclo: diasSemContato === null || diasSemContato > 180,
    responsavelNome: 'CEN FICTÍCIO GAMA DOS SANTOS',
    classeDoCliente,
    prioridade: prioridade(classeDoCliente),
  }));
  const termo = (consulta.get('termo') ?? '').trim().toLowerCase();
  if (!termo) return pagina(itens, 48_360);
  const achados = itens.filter((l) =>
    [l.clienteNome, l.carteiraNome, l.responsavelNome].some((texto) => texto.toLowerCase().includes(termo)),
  );
  return pagina(achados, achados.length);
}

/* ------------------------------------------------------------------------ */
/* Forecast, estoque e conferência com a GN (bloco 3 do #293, 29/09/2026)     */
/* ------------------------------------------------------------------------ */

const MESES_CURTOS = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/** Uma linha de produto do forecast da amostra: código, nome, PG, forecast, best guess e realizado. */
type LinhaDoForecastDaAmostra = readonly [codigo: string, nome: string, meta: number, forecast: number | null, bestGuess: number | null, realizado: number];

/**
 * OS NOVE GESTORES DA MAQUETE DO FORECAST (01/10/2026): cinco com nome de pessoa (letra grega, como os CENs fictícios),
 * um com nome de regional inteiro, um login da GN, um com uma linha só e um sem PG — o que a tela mostra com o traço.
 */
const GESTORES_DO_FORECAST: { gestor: string; consultores: number; linhas: LinhaDoForecastDaAmostra[] }[] = [
  {
    gestor: 'ALFA MATIAS DE ARAUJO CRUZ',
    consultores: 5,
    linhas: [
      ['TRATOR_PEQUENO', 'TRATOR PEQUENO', 4, 6, 2, 4],
      ['IMPLEMENTOS', 'IMPLEMENTOS CEN', 6, 8, 3, 6],
      ['TRATOR_MEDIO', 'TRATOR MÉDIO', 4, 5, 2, 4],
      ['TRATOR_GRANDE', 'TRATOR GRANDE', 4, 3, 1, 4],
    ],
  },
  {
    gestor: 'BETA ITALO JARDIM SILVA',
    consultores: 4,
    linhas: [
      ['TRATOR_PEQUENO', 'TRATOR PEQUENO', 4, 3, 4, 3],
      ['TRATOR_MEDIO', 'TRATOR MÉDIO', 6, 4, 6, 5],
      ['TRATOR_GRANDE', 'TRATOR GRANDE', 3, 3, 3, 2],
    ],
  },
  {
    gestor: 'GAMA LUZIA FERRAZ MELO',
    consultores: 3,
    linhas: [
      ['TRATOR_MEDIO', 'TRATOR MÉDIO', 8, 5, 5, 5],
      ['COLHEITADEIRA', 'COLHEITADEIRA', 5, 3, 3, 3],
    ],
  },
  {
    gestor: 'DELTA CAIO RAMOS ALMEIDA',
    consultores: 3,
    linhas: [
      ['TRATOR_MEDIO', 'TRATOR MÉDIO', 6, 3, 2, 3],
      ['PULVERIZADOR', 'PULVERIZADOR', 4, 2, 2, 2],
    ],
  },
  {
    gestor: 'ÉPSILON RAFA SANTANA LIMA',
    consultores: 2,
    linhas: [
      ['TRATOR_MEDIO', 'TRATOR MÉDIO', 7, 3, 2, 3],
      ['PLANTADEIRA', 'PLANTADEIRA', 3, 1, 1, 1],
    ],
  },
  {
    gestor: 'GESTOR FICTÍCIO DA REGIONAL NORTE PAULISTA',
    consultores: 6,
    linhas: [
      ['TRATOR_MEDIO', 'TRATOR MÉDIO', 14, 12, 13, 9],
      ['TRATOR_GRANDE', 'TRATOR GRANDE', 6, 5, 6, 4],
      ['COLHEITADEIRA', 'COLHEITADEIRA', 3, null, 2, 1],
      ['PULVERIZADOR', 'PULVERIZADOR', 2, 2, 2, 3],
    ],
  },
  {
    gestor: 'GESTOR.SUL',
    consultores: 3,
    linhas: [
      ['TRATOR_MEDIO', 'TRATOR MÉDIO', 10, 9, 8, 7],
      ['PLANTADEIRA', 'PLANTADEIRA', 4, 3, null, 2],
    ],
  },
  { gestor: 'ZETA JOAO PRADO OLIVEIRA', consultores: 2, linhas: [['TRATOR_MEDIO', 'TRATOR MÉDIO', 6, 4, 3, 4]] },
  { gestor: 'ETA FERNANDA TAVARES SANTOS', consultores: 2, linhas: [['TRATOR_MEDIO', 'TRATOR MÉDIO', 0, null, null, 1]] },
];

/** O mês da amostra muda os números um pouco, para a comparação com o mês anterior ter o que comparar. */
const FATOR_DO_MES_DO_FORECAST: Record<string, number> = { '2026-06': 0.97, '2026-07': 1.05, '2026-08': 0.92, '2026-09': 1 };

/**
 * O FORECAST DA GERÊNCIA: os nove gestores acima, um forecast e um best guess não informados e uma linha sem PG — o que a
 * tela mostra com o traço. O total soma as linhas, e as vendas sem gestor entram só nele.
 */
function forecastDaGerencia(estado: EstadoDaVisao360, competencia: string | null): RelatorioDoForecast {
  const mes = competencia && /^\d{4}-\d{2}$/.test(competencia) ? `${competencia}-01` : '2026-09-01';
  const vazio = estado !== 'completo';
  const fator = FATOR_DO_MES_DO_FORECAST[mes.slice(0, 7)] ?? 1;
  const noMes = (v: number | null) => (v === null ? null : n(v * fator));
  const gestores = vazio
    ? []
    : GESTORES_DO_FORECAST.map((g) => ({
        gestor: g.gestor,
        consultores: g.consultores,
        linhas: g.linhas.map(
          ([codigo, nome, meta, forecast, bestGuess, realizado]) =>
            ({
              codigo,
              nome,
              meta: n(meta * fator),
              forecast: noMes(forecast),
              bestGuess: noMes(bestGuess),
              realizado: n(realizado * fator),
            }) satisfies LinhaDoForecast,
        ),
      }));
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
    mesesDisponiveis: vazio ? [] : ['2026-06-01', '2026-07-01', '2026-08-01', '2026-09-01'],
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
      return coberturaPorCarteira(estado, empresa, consulta.get('classe'));
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
      return painelDoCen(estado, empresa, consulta.get('responsavel'), consulta.get('carteira'));
    case '/v1/processos':
      return contagemDeProcessos(estado, empresa, consulta);
    case '/v1/tarefas':
      return tarefasAtrasadas(estado);
    case '/v1/cobertura':
      return clientesSemContato(estado, consulta);
    default:
      return undefined;
  }
}
