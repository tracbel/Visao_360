/**
 * As nove leituras de relacionamento e as três de território (documento 23,
 * seções 2.5 e 2.6).
 *
 * TODAS SÃO DE LEITURA, e isso é decisão e não etapa faltando: a escrita de
 * processo e de tarefa carrega o motor de regras — mudar de fase dispara
 * automação, concluir tarefa gera a próxima — e ele entra junto com a tela que
 * o exercita (dívida D-9 do documento 23).
 *
 * NENHUMA REGRA DE NEGÓCIO MORA AQUI. Estas funções montam a requisição e
 * devolvem o que a API respondeu, inclusive `metricasSemDado` — que é o campo
 * que a tela usa para dizer "sem dado" com o motivo do lado, em vez de mostrar
 * um número plausível.
 *
 * **O agregado é calculado no banco.** A tela recebe dez linhas de funil, não
 * 45 mil processos para somar no navegador — e o `GROUP BY` roda dentro do
 * filtro global de empresa.
 */

import type { ComProcedencia, PaginaDe } from '../../tipos/api';
import type { PecasDoCliente } from '../../tipos/pecas';
import type {
  Agregado,
  CarteirasDoCliente,
  CoberturaDeFilial,
  CoberturaResumo,
  ConsultaDeCobertura,
  ConsultaDeInteracoes,
  ConsultaDeProcessos,
  ConsultaDeTarefas,
  ContagemPorRotulo,
  Faturamento,
  FaturamentoDoCliente,
  PainelDoCen,
  VendasPerdidas,
  FunilPorEstagio,
  RecorteDoFunil,
  FaseDoFunil,
  InteracaoResumo,
  MunicipioParaSelecao,
  PainelDaAgenda,
  ProcessoDetalhe,
  ProcessoResumo,
  ResumoDeCobertura,
  TarefaResumo,
  TerritorioDeCarteira,
} from '../../tipos/relacionamento';
import { ler, type ContextoDeAcesso } from './http';

/* ---------------------------------------------------------------------- */
/* Processo                                                                */
/* ---------------------------------------------------------------------- */

/** Os valores iniciais de uma consulta de processos. */
export const PROCESSOS_INICIAL: ConsultaDeProcessos = {
  pagina: 1,
  tamanho: 25,
  termo: '',
  situacao: '',
  clienteChave: '',
  faseCodigo: '',
  tipoProcessoCodigo: '',
  ordenarPor: 'FaseDesde',
  descendente: false,
  incluirEncerrados: false,
};

/** Lista processos da filial do contexto, com paginação, filtro e ordenação. */
export function listarProcessos(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeProcessos,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<ProcessoResumo>>> {
  return ler<PaginaDe<ProcessoResumo>>('/v1/processos', contexto, {
    sinal,
    parametros: {
      pagina: consulta.pagina,
      tamanho: consulta.tamanho,
      termo: consulta.termo.trim(),
      situacao: consulta.situacao,
      clienteChave: consulta.clienteChave,
      faseCodigo: consulta.faseCodigo,
      tipoProcessoCodigo: consulta.tipoProcessoCodigo,
      ordenarPor: consulta.ordenarPor,
      descendente: consulta.descendente,
      incluirEncerrados: consulta.incluirEncerrados,
    },
  });
}

/**
 * A LISTA INTEIRA DOS PROCESSOS, com os filtros da tela, para o Exportar do Pipeline (30/09/2026): de 200 em 200 — o
 * tamanho máximo da API —, até o total ou até {@link LINHAS_NO_EXPORTAR}, como a da Cobertura.
 */
export async function listarProcessosInteira(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeProcessos,
  sinal?: AbortSignal,
): Promise<{ itens: ProcessoResumo[]; total: number }> {
  const tamanho = 200;
  const primeira = await listarProcessos(contexto, { ...consulta, pagina: 1, tamanho }, sinal);
  const total = primeira.dados.total;
  const itens = [...primeira.dados.itens];
  const paginas = Math.min(Math.ceil(total / tamanho), Math.ceil(LINHAS_NO_EXPORTAR / tamanho));
  for (let pagina = 2; pagina <= paginas; pagina++) {
    const resposta = await listarProcessos(contexto, { ...consulta, pagina, tamanho }, sinal);
    itens.push(...resposta.dados.itens);
  }
  return { itens, total };
}

/**
 * Quantos processos existem numa situação — só a contagem, sem trazer as linhas.
 *
 * A CONTAGEM É DO BANCO, e não da tela: pede uma linha e lê o `total`, que o
 * repositório apura com um `COUNT` dentro do filtro global de empresa. Somar
 * situações no navegador exigiria baixar 45 mil processos para produzir cinco
 * números.
 *
 * `incluirEncerrados` acompanha a situação porque ganho, perdido e cancelado
 * SÃO os encerrados: pedir um desfecho sem incluí-los devolveria zero sempre.
 */
export async function contarProcessosPorSituacao(
  contexto: ContextoDeAcesso,
  situacao: string,
  sinal?: AbortSignal,
): Promise<number> {
  const resposta = await ler<PaginaDe<ProcessoResumo>>('/v1/processos', contexto, {
    sinal,
    parametros: { pagina: 1, tamanho: 1, situacao, incluirEncerrados: situacao !== 'Aberto' },
  });
  return resposta.dados.total;
}

/** A ficha de um processo. 404 significa "não existe OU não é desta filial". */
export function obterProcesso(
  contexto: ContextoDeAcesso,
  chave: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<ProcessoDetalhe>> {
  return ler<ProcessoDetalhe>(`/v1/processos/${chave}`, contexto, { sinal });
}

/**
 * O funil por fase, agrupado no banco.
 *
 * Devolve `processosComValor` SEMPRE, e `valorTotal` só quando ele existe. É a
 * diferença entre "o funil vale zero" e "ninguém preencheu o valor" — e a
 * segunda é a verdade em 99,2% dos processos.
 */
export function obterFunil(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<Agregado<FaseDoFunil>>> {
  return ler<Agregado<FaseDoFunil>>('/v1/relatorios/funil', contexto, { sinal });
}

/**
 * As perdas por motivo, contadas em `processo.Processo`.
 *
 * O motivo aqui é `NAO_INFORMADO_NA_ORIGEM` em 100% das linhas, e isso é verdade sobre o
 * PROCESSO: o Vórtice encerra o processo sem coluna de motivo. Quem responde "por que
 * perdemos" é {@link obterVendasPerdidas}, que lê o formulário.
 */
export function obterPerdas(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<Agregado<ContagemPorRotulo>>> {
  return ler<Agregado<ContagemPorRotulo>>('/v1/relatorios/perdas', contexto, { sinal });
}

/**
 * As vendas perdidas registradas no formulário — motivo, concorrente e diferença de preço.
 *
 * É outra população, e não outro recorte da mesma: conta FORMULÁRIOS preenchidos, e não
 * processos marcados como perdidos. A diferença entre os dois números vem na resposta, em
 * `metricasSemDado`, porque ela é a informação mais acionável da tela: quantas derrotas
 * ninguém registrou.
 */
export function obterVendasPerdidas(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
  recorte: RecorteDoFunil = {},
): Promise<ComProcedencia<VendasPerdidas>> {
  return ler<VendasPerdidas>('/v1/relatorios/vendas-perdidas', contexto, {
    sinal,
    parametros: { de: recorte.de, ate: recorte.ate, formulario: recorte.formulario, responsavel: recorte.responsavel },
  });
}

/**
 * O funil por estágio do Vórtice — Lead a Faturamento —, contado no banco (documento 52).
 *
 * Sem `de`/`ate`, o servidor usa o ano fiscal até o último mês fechado; sem `base`, a coorte. O vazio vem com o motivo
 * verdadeiro em `metricasSemDado` — a rotina que traz o funil ainda não rodou, falhou, ou não trouxe esta filial.
 */
export function obterFunilPorEstagio(
  contexto: ContextoDeAcesso,
  recorte: RecorteDoFunil,
  sinal?: AbortSignal,
): Promise<ComProcedencia<FunilPorEstagio>> {
  return ler<FunilPorEstagio>('/v1/relatorios/funil-por-estagio', contexto, {
    sinal,
    parametros: {
      de: recorte.de,
      ate: recorte.ate,
      base: recorte.base,
      carteira: recorte.carteira,
      responsavel: recorte.responsavel,
    },
  });
}

/* ---------------------------------------------------------------------- */
/* Tarefa                                                                  */
/* ---------------------------------------------------------------------- */

/** Os valores iniciais da agenda: a fila inteira, do mais antigo para o mais novo. */
export const TAREFAS_INICIAL: ConsultaDeTarefas = {
  pagina: 1,
  tamanho: 25,
  minhas: false,
  situacao: 'Pendente',
  clienteChave: '',
  de: '',
  ate: '',
  somenteAtrasadas: false,
  ordenarPor: 'AgendadaPara',
  descendente: false,
};

/** Lista tarefas da agenda, com prazo e atraso já calculados pela API. */
export function listarTarefas(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeTarefas,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<TarefaResumo>>> {
  return ler<PaginaDe<TarefaResumo>>('/v1/tarefas', contexto, {
    sinal,
    parametros: {
      pagina: consulta.pagina,
      tamanho: consulta.tamanho,
      minhas: consulta.minhas,
      situacao: consulta.situacao,
      clienteChave: consulta.clienteChave,
      de: consulta.de,
      ate: consulta.ate,
      somenteAtrasadas: consulta.somenteAtrasadas,
      ordenarPor: consulta.ordenarPor,
      descendente: consulta.descendente,
    },
  });
}

/** O painel do CEN: pendentes, atrasadas, hoje e próximos sete dias. */
export function obterPainelDaAgenda(
  contexto: ContextoDeAcesso,
  minhas: boolean,
  sinal?: AbortSignal,
): Promise<ComProcedencia<Agregado<PainelDaAgenda>>> {
  return ler<Agregado<PainelDaAgenda>>('/v1/relatorios/agenda', contexto, {
    sinal,
    parametros: { minhas },
  });
}

/* ---------------------------------------------------------------------- */
/* Interação                                                               */
/* ---------------------------------------------------------------------- */

/** Os valores iniciais da linha do tempo. */
export const INTERACOES_INICIAL: ConsultaDeInteracoes = {
  pagina: 1,
  tamanho: 25,
  clienteChave: '',
  natureza: '',
  de: '',
  ate: '',
};

/** A linha do tempo de contatos. A ordem é sempre a mais recente primeiro. */
export function listarInteracoes(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeInteracoes,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<InteracaoResumo>>> {
  return ler<PaginaDe<InteracaoResumo>>('/v1/interacoes', contexto, {
    sinal,
    parametros: {
      pagina: consulta.pagina,
      tamanho: consulta.tamanho,
      clienteChave: consulta.clienteChave,
      natureza: consulta.natureza,
      de: consulta.de,
      ate: consulta.ate,
    },
  });
}

/* ---------------------------------------------------------------------- */
/* Cobertura                                                               */
/* ---------------------------------------------------------------------- */

/**
 * Os valores iniciais da Cobertura.
 *
 * A ORDEM PADRÃO É QUEM ESTÁ HÁ MAIS TEMPO SEM CONTATO, e o nunca-contatado vem
 * antes de todos. Não é ordenação por classe, que é o que o protótipo fazia:
 * a classe do VÍNCULO (`ClienteCarteira.Classe`) segue entrando como `C` por
 * assunção na maioria dos casos, e ordenar por ela ordenaria por nada
 * (documento 25, §5.1). O filtro `classe` abaixo é outra coisa: ele usa a
 * classe do CLIENTE — a curva ABC apurada do faturamento —, que sustenta
 * segmentação.
 */
export const COBERTURA_INICIAL: ConsultaDeCobertura = {
  pagina: 1,
  tamanho: 25,
  classe: '',
  diasSemContato: '',
  somenteSemContato: false,
  ordenarPor: 'UltimaInteracaoEm',
  descendente: false,
  termo: '',
};

/** A carteira cliente a cliente, com a data do último contato. */
export function listarCobertura(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeCobertura,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<CoberturaResumo>>> {
  return ler<PaginaDe<CoberturaResumo>>('/v1/cobertura', contexto, {
    sinal,
    parametros: {
      pagina: consulta.pagina,
      tamanho: consulta.tamanho,
      classe: consulta.classe,
      diasSemContato: consulta.diasSemContato,
      somenteSemContato: consulta.somenteSemContato,
      ordenarPor: consulta.ordenarPor,
      descendente: consulta.descendente,
      termo: consulta.termo.trim(),
    },
  });
}

/** Quantas linhas o Exportar leva no máximo — 50 páginas de 200. Vale para a Cobertura e para o Pipeline. */
export const LINHAS_NO_EXPORTAR = 10_000;

/**
 * A LISTA INTEIRA DA COBERTURA, com os mesmos filtros e a mesma busca, para o Exportar (30/09/2026): página a página, de
 * 200 em 200 — o tamanho máximo da API —, até o total ou até {@link LINHAS_NO_EXPORTAR}.
 */
export async function listarCoberturaInteira(
  contexto: ContextoDeAcesso,
  consulta: ConsultaDeCobertura,
  sinal?: AbortSignal,
): Promise<{ itens: CoberturaResumo[]; total: number }> {
  const tamanho = 200;
  const primeira = await listarCobertura(contexto, { ...consulta, pagina: 1, tamanho }, sinal);
  const total = primeira.dados.total;
  const itens = [...primeira.dados.itens];
  const paginas = Math.min(Math.ceil(total / tamanho), Math.ceil(LINHAS_NO_EXPORTAR / tamanho));
  for (let pagina = 2; pagina <= paginas; pagina++) {
    const resposta = await listarCobertura(contexto, { ...consulta, pagina, tamanho }, sinal);
    itens.push(...resposta.dados.itens);
  }
  return { itens, total };
}

/**
 * A cobertura por carteira: clientes, contatados em 30 e 90 dias, nunca contatados. `classe` (A a D) conta só os
 * clientes daquela classe — sem classe apurada conta como D, a regra do painel do CEN (01/10/2026).
 */
export function obterResumoDeCobertura(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
  classe?: string,
): Promise<ComProcedencia<Agregado<ResumoDeCobertura>>> {
  return ler<Agregado<ResumoDeCobertura>>('/v1/relatorios/cobertura', contexto, { sinal, parametros: { classe } });
}

/* ---------------------------------------------------------------------- */
/* Território                                                              */
/* ---------------------------------------------------------------------- */

/**
 * A cobertura agrupada por FILIAL — o agrupamento que existe no dado.
 *
 * Não há rota de regional para chamar, e é o achado do documento 26: a tabela
 * de regional do Vórtice existe e tem zero linhas.
 */
export function obterCoberturaPorFilial(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<Agregado<CoberturaDeFilial>>> {
  return ler<Agregado<CoberturaDeFilial>>('/v1/cobertura/filiais', contexto, { sinal });
}

/** O território de cada carteira, com as cidades. Carteira sem cidade vem com a lista vazia. */
export function listarTerritorioPorCarteira(
  contexto: ContextoDeAcesso,
  empresaCodigo: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<Agregado<TerritorioDeCarteira>>> {
  return ler<Agregado<TerritorioDeCarteira>>('/v1/cobertura/carteiras', contexto, {
    sinal,
    parametros: { empresaCodigo },
  });
}

/**
 * Busca município por COMEÇO do nome.
 *
 * `?termo=ribeir` encontra Ribeirão Preto; `?termo=eirão` NÃO encontra nada. É
 * decisão de desempenho declarada no contrato: busca por trecho não usa índice
 * e varreria as 9.750 linhas a cada tecla. Acento e caixa não importam — o
 * front não normaliza nada antes de mandar (documento 26, §8.2).
 */
export function listarMunicipios(
  contexto: ContextoDeAcesso,
  termo: string,
  uf: string,
  tamanho = 25,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PaginaDe<MunicipioParaSelecao>>> {
  return ler<PaginaDe<MunicipioParaSelecao>>('/v1/municipios', contexto, {
    sinal,
    parametros: { termo: termo.trim(), uf, pagina: 1, tamanho },
  });
}

/**
 * O painel de um CEN — cobertura por classe, processos e faturamento da carteira.
 *
 * Sem `responsavelChave` devolve o consolidado de todos os responsáveis, que é o que a tela
 * mostra antes de alguém escolher um nome. `carteiraChave` recorta numa carteira só — o filtro
 * de carteira da Performance (01/10/2026).
 */
export function obterPainelDoCen(
  contexto: ContextoDeAcesso,
  responsavelChave: string | null,
  sinal?: AbortSignal,
  carteiraChave?: string | null,
): Promise<ComProcedencia<PainelDoCen>> {
  return ler<PainelDoCen>('/v1/relatorios/cen', contexto, {
    sinal,
    parametros: { responsavel: responsavelChave ?? undefined, carteira: carteiraChave ?? undefined },
  });
}

/**
 * O faturamento — série de doze meses e os maiores clientes, lidos da SD2 do Protheus.
 *
 * Até 06/09/2026 as telas diziam "depende do Protheus, que está parado". Estava errado: o que
 * tinha parado era a cópia que o Vórtice recebe. A origem tem nota da mesma semana.
 */
export function obterFaturamento(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<Faturamento>> {
  return ler<Faturamento>('/v1/relatorios/faturamento', contexto, { sinal });
}

/**
 * O faturamento de UM cliente — doze meses, quebra, filial que emitiu a nota e a data da carga. 404 quer dizer "não
 * existe OU não está ao seu alcance".
 */
export function obterFaturamentoDoCliente(
  contexto: ContextoDeAcesso,
  chaveDoCliente: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<FaturamentoDoCliente>> {
  return ler<FaturamentoDoCliente>(`/v1/clientes/${chaveDoCliente}/faturamento`, contexto, { sinal });
}

/**
 * As peças de UM cliente (02/10/2026) — os doze meses com balcão × oficina e o grupo comercial, o vendedor principal e os
 * orçamentos em aberto, do Protheus. 404 quer dizer "não existe OU não está ao seu alcance".
 */
export function obterPecasDoCliente(
  contexto: ContextoDeAcesso,
  chaveDoCliente: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PecasDoCliente>> {
  return ler<PecasDoCliente>(`/v1/clientes/${chaveDoCliente}/pecas`, contexto, { sinal });
}

/** As carteiras em que o cliente está, com o CEN de cada uma — nas carteiras ao alcance de quem consulta. */
export function listarCarteirasDoCliente(
  contexto: ContextoDeAcesso,
  chaveDoCliente: string,
  sinal?: AbortSignal,
): Promise<ComProcedencia<CarteirasDoCliente>> {
  return ler<CarteirasDoCliente>(`/v1/clientes/${chaveDoCliente}/carteiras`, contexto, { sinal });
}
