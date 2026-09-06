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
import type {
  Agregado,
  CoberturaDeFilial,
  CoberturaResumo,
  ConsultaDeCobertura,
  ConsultaDeInteracoes,
  ConsultaDeProcessos,
  ConsultaDeTarefas,
  ContagemPorRotulo,
  PainelDoCen,
  VendasPerdidas,
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
): Promise<ComProcedencia<VendasPerdidas>> {
  return ler<VendasPerdidas>('/v1/relatorios/vendas-perdidas', contexto, { sinal });
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
 * 59 de 49.109 vínculos têm classe lida, e ordenar por um campo assumido em
 * 99,9% dos casos ordenaria por nada (documento 25, §5.1).
 */
export const COBERTURA_INICIAL: ConsultaDeCobertura = {
  pagina: 1,
  tamanho: 25,
  classe: '',
  diasSemContato: '',
  somenteSemContato: false,
  ordenarPor: 'UltimaInteracaoEm',
  descendente: false,
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
    },
  });
}

/** A cobertura por carteira: clientes, contatados em 30 e 90 dias, nunca contatados. */
export function obterResumoDeCobertura(
  contexto: ContextoDeAcesso,
  sinal?: AbortSignal,
): Promise<ComProcedencia<Agregado<ResumoDeCobertura>>> {
  return ler<Agregado<ResumoDeCobertura>>('/v1/relatorios/cobertura', contexto, { sinal });
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
 * mostra antes de alguém escolher um nome.
 */
export function obterPainelDoCen(
  contexto: ContextoDeAcesso,
  responsavelChave: string | null,
  sinal?: AbortSignal,
): Promise<ComProcedencia<PainelDoCen>> {
  const caminho = responsavelChave
    ? `/v1/relatorios/cen?responsavel=${encodeURIComponent(responsavelChave)}`
    : '/v1/relatorios/cen';

  return ler<PainelDoCen>(caminho, contexto, { sinal });
}
