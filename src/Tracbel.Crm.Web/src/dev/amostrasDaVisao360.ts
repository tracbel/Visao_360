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
import type { MetaERealizadoDaFilial } from '../tipos/metas';
import type { MaquinasEntreguesNoArt, PainelExecutivoDaFilial } from '../tipos/painelExecutivo';
import type {
  Agregado,
  ContagemPorRotulo,
  FaseDoFunil,
  Faturamento,
  FunilPorEstagio,
  PainelDaAgenda,
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

/** A contagem de processos por situação: a tela só lê o `total`. */
export function contagemDeProcessos(estado: EstadoDaVisao360, codigo: string, situacao: string): PaginaDe<ProcessoResumo> {
  const peso = pesoDaOperacao(estado, codigo);
  const total = situacao === 'Ganho' ? n(260 * peso) : situacao === 'Perdido' ? n(310 * peso) : n(900 * peso);
  return { itens: [], pagina: 1, tamanho: 1, total, totalDePaginas: total, temProxima: total > 1 };
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
    case '/v1/processos':
      return contagemDeProcessos(estado, empresa, consulta.get('situacao') ?? '');
    case '/v1/tarefas':
      return tarefasAtrasadas(estado);
    case '/v1/cobertura':
      return clientesSemContato(estado);
    default:
      return undefined;
  }
}
