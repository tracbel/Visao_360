/**
 * Os contratos de RELACIONAMENTO e de TERRITÓRIO, do jeito que eles chegam no
 * navegador.
 *
 * Espelham, campo a campo, os `record` de `Tracbel.Crm.Aplicacao.Relacionamento`
 * e `.Territorio`, documentados em `docs/projeto/23-API.md`, seções 2.5 e 2.6.
 * A serialização do ASP.NET Core é camelCase, então o nome aqui é o mesmo nome
 * de lá com a primeira letra minúscula — e nada é renomeado no caminho.
 *
 * NADA AQUI É INVENTADO. Campo que a API não devolve não existe neste arquivo.
 * Onde o comentário diz "nulo é o normal", o número que sustenta a afirmação
 * está no documento 25, seção 9 — a lista das métricas do protótipo que o dado
 * real não sustenta.
 */

/* ---------------------------------------------------------------------- */
/* O envelope dos agregados                                                */
/* ---------------------------------------------------------------------- */

/**
 * UMA MÉTRICA QUE A TELA PEDE E O DADO NÃO SUSTENTA — devolvida vazia, com o
 * motivo medido na mesma consulta que produziu o agregado.
 *
 * É a peça mais importante deste arquivo, e ela existe por causa de um defeito
 * medido no sistema que estamos substituindo: lá, 17 meses de faturamento
 * parado passaram por atual porque a tela mostrava um número sem dizer de onde
 * ele vinha. Uma tela que diz "sem dado" está certa; uma tela que mostra um
 * número construído sobre 0,8% de preenchimento está errada — e não tem como o
 * usuário perceber (documento 23, seção 2.5).
 */
export type MetricaSemDado = {
  /** O nome da métrica, como a tela a chama. Ex.: `valorDoFunilConfiavel`. */
  metrica: string;
  /** Por que ela vem vazia, com o número que sustenta a afirmação. */
  motivo: string;
};

/** Um agregado do banco, junto do que a tela pediu e não pôde ser respondido. */
export type Agregado<T> = { itens: T[]; metricasSemDado: MetricaSemDado[] };

/* ---------------------------------------------------------------------- */
/* Processo                                                                */
/* ---------------------------------------------------------------------- */

/** O processo como a listagem e o cartão do funil o mostram. */
export type ProcessoResumo = {
  chave: string;
  /** O número legível — é o que o usuário fala ao telefone. */
  numero: number;
  titulo: string;
  clienteChave: string;
  clienteNome: string;
  tipoProcessoCodigo: string;
  tipoProcessoNome: string;
  faseCodigo: string;
  faseNome: string;
  faseOrdem: number;
  faseDesde: string;
  diasNaFase: number;
  situacao: string;
  /** Nulo quando a origem não declara — só 0,8% declaram (documento 25, §9). */
  valorEstimado: number | null;
  previsaoConclusao: string | null;
  proprietarioNome: string | null;
  criadoEm: string;
};

/** O processo como a ficha da oportunidade o mostra. */
export type ProcessoDetalhe = {
  resumo: ProcessoResumo;
  descricao: string | null;
  quantidade: number | null;
  valorFinal: number | null;
  /** A primeira previsão registrada — é o que mede derrapagem. */
  previsaoConclusaoOriginal: string | null;
  situacaoDesde: string;
  concluidoEm: string | null;
  motivoDePerdaCodigo: string | null;
  observacaoDaPerda: string | null;
  versao: string | null;
};

/** Uma coluna do funil, agrupada NO BANCO. */
export type FaseDoFunil = {
  tipoProcessoCodigo: string;
  tipoProcessoNome: string;
  faseCodigo: string;
  faseNome: string;
  faseOrdem: number;
  processos: number;
  /** Quantos declaram valor. É a medida da confiança do total. */
  processosComValor: number;
  /** Nulo — e não zero — quando nenhum processo da fase declara valor. */
  valorTotal: number | null;
};

/** Uma contagem por rótulo de catálogo. É o formato do relatório de perdas. */
export type ContagemPorRotulo = { codigo: string; nome: string; quantidade: number };

/**
 * Uma fatia do relatório de vendas perdidas.
 *
 * `comOsDoisPrecos` viaja junto de `diferencaMediaDePreco` de propósito: a média só se calcula
 * sobre as linhas que declararam o preço do concorrente E o nosso, e a tela precisa poder dizer
 * sobre quantas ela foi feita. Média sem denominador é o defeito que este CRM existe para não
 * repetir.
 */
export type FatiaDeVendaPerdida = {
  codigo: string;
  nome: string;
  quantidade: number;
  /** A soma das quantidades: uma perda pode ser de três tratores. */
  maquinas: number;
  comOsDoisPrecos: number;
  /** Quanto o nosso preço ficou ACIMA do concorrente, em média. Nulo quando ninguém declarou. */
  diferencaMediaDePreco: number | null;
};

/** O relatório de vendas perdidas inteiro, como a API o devolve. */
export type VendasPerdidas = {
  /** Quantos formulários de venda perdida foram preenchidos. */
  registradas: number;
  /** Quantos processos estão marcados como perdidos. Sempre maior ou igual a `registradas`. */
  processosPerdidos: number;
  porMotivo: FatiaDeVendaPerdida[];
  porConcorrente: FatiaDeVendaPerdida[];
  metricasSemDado: MetricaSemDado[];
};

/* ---------------------------------------------------------------------- */
/* Tarefa                                                                  */
/* ---------------------------------------------------------------------- */

/** A tarefa como a agenda a mostra, com o atraso já calculado pela API. */
export type TarefaResumo = {
  chave: string;
  assunto: string;
  detalhe: string | null;
  tipoTarefaCodigo: string;
  tipoTarefaNome: string;
  clienteChave: string | null;
  clienteNome: string | null;
  processoChave: string | null;
  processoTitulo: string | null;
  responsavelNome: string;
  agendadaPara: string;
  /** Nulo em 79% das tarefas: a origem não declara prazo (documento 25, §3.1). */
  prazoLimite: string | null;
  /** De 1 (alta) a 5 (baixa). */
  prioridade: number;
  situacao: string;
  /** Medido contra a data AGENDADA, não contra o prazo — que quase não existe. */
  estaAtrasada: boolean;
  diasDeAtraso: number;
  concluidaEm: string | null;
  resultadoNome: string | null;
};

/** O painel do CEN, contado no banco. */
export type PainelDaAgenda = {
  pendentes: number;
  atrasadas: number;
  paraHoje: number;
  proximosSeteDias: number;
  concluidasNosUltimosTrintaDias: number;
  /** Quantas pendentes não têm prazo. É o que limita o indicador de cumprimento. */
  semPrazoLimite: number;
  maisAntigaPendenteEm: string | null;
};

/* ---------------------------------------------------------------------- */
/* Interação                                                               */
/* ---------------------------------------------------------------------- */

/** A interação como a linha do tempo a mostra. */
export type InteracaoResumo = {
  chave: string;
  assunto: string;
  detalhe: string | null;
  tipoTarefaNome: string;
  resultadoNome: string | null;
  resultadoComplemento: string | null;
  /** `Ativa`, `Receptiva` ou `Sistema` — quem procurou quem. */
  natureza: string;
  ocorridaEm: string;
  /** Sempre nulo hoje: a coluna da origem é zero em 122.812 de 122.812 linhas. */
  duracaoMinutos: number | null;
  latitude: number | null;
  longitude: number | null;
  autorNome: string;
  clienteChave: string | null;
  clienteNome: string | null;
  processoChave: string | null;
};

/* ---------------------------------------------------------------------- */
/* Cobertura de carteira                                                   */
/* ---------------------------------------------------------------------- */

/** Uma linha da Cobertura: um cliente dentro de uma carteira. */
export type CoberturaResumo = {
  clienteChave: string;
  clienteNome: string;
  carteiraChave: string;
  carteiraNome: string;
  linhaDeNegocioNome: string;
  /**
   * A classe do cliente NESTA carteira.
   *
   * SÓ 59 DE 49.109 VÍNCULOS TÊM CLASSE LIDA da origem; os outros 49.050
   * entraram como `C` por assunção (documento 25, §5.1). A tela não ordena nem
   * segmenta por este campo, e diz por quê.
   */
  classe: string;
  ultimaInteracaoEm: string | null;
  diasSemContato: number | null;
  diasCicloContato: number | null;
  /** Nulo — e não falso — quando não há cadência declarada. */
  estaForaDoCiclo: boolean | null;
  responsavelNome: string;
};

/** A cobertura de uma carteira inteira, contada no banco. */
export type ResumoDeCobertura = {
  carteiraChave: string;
  carteiraCodigo: string;
  carteiraNome: string;
  linhaDeNegocioNome: string;
  responsavelNome: string;
  clientes: number;
  comContatoEm30Dias: number;
  comContatoEm90Dias: number;
  nuncaContatados: number;
  ultimoContatoEm: string | null;
  /** `Comercial`, `Administrativa` ou `Teste` — o que a carteira é. */
  naturezaDaCarteira: string;
  /**
   * O que o dono da carteira é: `Pessoa`, `Departamento`, `Sistema`, `Fornecedor` ou `Teste`.
   *
   * A área ENTRA nos números — a carteira da Inteligência de Mercado tem 5.000 clientes reais.
   * O que este campo permite é a tela dizer que aquilo é uma área, em vez de deixar alguém
   * comparar o volume de um time com o de uma pessoa sem saber.
   */
  naturezaDoResponsavel: string;
};

/* ---------------------------------------------------------------------- */
/* Território — filial, carteira e município                               */
/* ---------------------------------------------------------------------- */

/**
 * A cobertura territorial de uma FILIAL — o primeiro nível do agrupamento que
 * existe de verdade.
 *
 * NÃO EXISTE REGIONAL, e não é omissão deste arquivo: `IVS_Regional` existe no
 * Vórtice e tem ZERO linhas. As sete regionais que o protótipo desenhava — MT
 * Norte, GO, BA Oeste e mais quatro — não têm tabela, coluna nem valor de texto
 * que as sustente, e as treze filiais em operação estão todas no interior de
 * São Paulo (documento 26, §1).
 */
export type CoberturaDeFilial = {
  empresaChave: string;
  empresaCodigo: string;
  empresaNome: string;
  carteiras: number;
  /** Quantas declaram cidade. A diferença para `carteiras` é a lacuna do cadastro. */
  carteirasComMunicipio: number;
  municipios: number;
  ufs: string[];
};

/** Um município do catálogo nacional. */
export type MunicipioParaSelecao = {
  id: number;
  nome: string;
  uf: string;
  /** Nulo é o normal hoje: `GE_Cidade` não tem essa coluna (documento 26, §4.5). */
  codigoIbge: number | null;
};

/** O território de uma carteira: a filial dona e as cidades atendidas. */
export type TerritorioDeCarteira = {
  carteiraChave: string;
  carteiraCodigo: string;
  carteiraNome: string;
  linhaDeNegocioNome: string;
  responsavelNome: string;
  empresaCodigo: string;
  empresaNome: string;
  /** Vazia quando a carteira não declara cidade — 69 das 142 estão assim. */
  municipios: MunicipioParaSelecao[];
};

/* ---------------------------------------------------------------------- */
/* Os parâmetros das consultas                                             */
/* ---------------------------------------------------------------------- */

/** Domínio FECHADO: o que não está aqui a API recusa com as opções na mensagem. */
export const SITUACOES_DE_PROCESSO = [
  'Aberto', 'Suspenso', 'Ganho', 'Perdido', 'Cancelado',
] as const;

/** Domínio FECHADO de situação de tarefa. */
export const SITUACOES_DE_TAREFA = [
  'Pendente', 'EmAndamento', 'Concluida', 'Cancelada', 'Reatribuida',
] as const;

/** Domínio FECHADO de natureza da interação: quem procurou quem. */
export const NATUREZAS_DE_INTERACAO = ['Ativa', 'Receptiva', 'Sistema'] as const;

/** Domínio FECHADO de ordenação de processo. */
export const ORDENS_DE_PROCESSO = [
  'Numero', 'Titulo', 'ValorEstimado', 'PrevisaoConclusao', 'FaseDesde', 'CriadoEm',
] as const;
export type OrdemDeProcesso = (typeof ORDENS_DE_PROCESSO)[number];

/** Domínio FECHADO de ordenação de tarefa. */
export const ORDENS_DE_TAREFA = ['AgendadaPara', 'Prioridade', 'PrazoLimite', 'Assunto'] as const;
export type OrdemDeTarefa = (typeof ORDENS_DE_TAREFA)[number];

/** Domínio FECHADO de ordenação da cobertura. */
export const ORDENS_DE_COBERTURA = ['UltimaInteracaoEm', 'Classe', 'Nome'] as const;
export type OrdemDeCobertura = (typeof ORDENS_DE_COBERTURA)[number];

/** Parâmetros de `/api/v1/processos` — documento 23, seção 2.5. */
export type ConsultaDeProcessos = {
  pagina: number;
  tamanho: number;
  termo: string;
  situacao: string;
  clienteChave: string;
  faseCodigo: string;
  tipoProcessoCodigo: string;
  ordenarPor: OrdemDeProcesso;
  descendente: boolean;
  incluirEncerrados: boolean;
};

/** Parâmetros de `/api/v1/tarefas` — documento 23, seção 2.5. */
export type ConsultaDeTarefas = {
  pagina: number;
  tamanho: number;
  /** Usa o usuário do contexto de acesso, e não um parâmetro de nome. */
  minhas: boolean;
  situacao: string;
  clienteChave: string;
  /** `AAAA-MM-DD`, ou vazio. */
  de: string;
  ate: string;
  somenteAtrasadas: boolean;
  ordenarPor: OrdemDeTarefa;
  descendente: boolean;
};

/** Parâmetros de `/api/v1/interacoes` — a ordem é sempre a mais recente primeiro. */
export type ConsultaDeInteracoes = {
  pagina: number;
  tamanho: number;
  clienteChave: string;
  natureza: string;
  de: string;
  ate: string;
};

/** Parâmetros de `/api/v1/cobertura` — documento 23, seção 2.5. */
export type ConsultaDeCobertura = {
  pagina: number;
  tamanho: number;
  classe: string;
  /** Vazio, ou um número em texto: só quem está há mais dias que isto sem contato. */
  diasSemContato: string;
  somenteSemContato: boolean;
  ordenarPor: OrdemDeCobertura;
  descendente: boolean;
};

/**
 * A cobertura de uma classe da curva ABC dentro da carteira de um responsável.
 *
 * São TRÊS estados, e não dois: coberto, fora da cadência e nunca contatado. Somar os dois
 * últimos esconderia a diferença mais acionável da tela — quem o CEN conhece e deixou vencer não
 * é o mesmo problema que quem ele nunca procurou.
 */
export type CoberturaPorClasse = {
  /** `A`, `B`, `C` ou `D`. */
  classe: string;
  clientes: number;
  /** Com contato dentro da cadência declarada para a classe. */
  cobertos: number;
  /** Com contato, mas há mais tempo do que a cadência permite. */
  foraDaCadencia: number;
  nuncaContatados: number;
  /**
   * Em linha de negócio que não declara cadência nenhuma.
   *
   * Não são cobertos nem atrasados: não há prazo contra o que medi-los, e escolher um número
   * aqui seria inventar a meta.
   */
  semCadenciaDeclarada: number;
  /** De quantos em quantos dias esta classe deve ser visitada. Nulo quando não declarado. */
  diasDeCadencia: number | null;
};

/** Um responsável no seletor do painel. */
export type ResponsavelParaSelecao = {
  chave: string;
  nome: string;
  /** `Pessoa` ou `Departamento`. */
  natureza: string;
  carteiras: number;
};

/** Os números de um responsável. */
export type PainelDoResponsavel = {
  responsavelChave: string;
  responsavelNome: string;
  naturezaDoResponsavel: string;
  carteiras: number;
  clientes: number;
  porClasse: CoberturaPorClasse[];
  processosGanhos: number;
  processosPerdidos: number;
  processosAbertos: number;
  vendasPerdidasRegistradas: number;
  /**
   * O faturamento dos CLIENTES da carteira, e não das vendas desta pessoa — a origem não diz
   * quem vendeu cada nota. Para em 11/04/2025.
   */
  faturamentoDaCarteira: number;
};

/** O painel do CEN inteiro, como a API o devolve. */
export type PainelDoCen = {
  painel: PainelDoResponsavel;
  responsaveis: ResponsavelParaSelecao[];
  metricasSemDado: MetricaSemDado[];
};

/** Um mês da série de faturamento. */
export type MesDeFaturamento = {
  /** O primeiro dia do mês. */
  competencia: string;
  valorLiquido: number;
  /** Clientes DISTINTOS que compraram no mês — não linhas. */
  clientes: number;
  notas: number;
};

/** Um cliente no ranking de faturamento. */
export type ClienteNoRanking = {
  clienteChave: string;
  nome: string;
  /** A letra da curva ABC, quando apurada. */
  classe: string | null;
  valorLiquido: number;
  ultimaCompraEm: string | null;
};

/**
 * O faturamento lido da SD2 do Protheus.
 *
 * `competenciaMaisRecente` viaja junto de propósito: se a carga do ERP parar de novo, ela para
 * de avançar e a tela diz isso — em vez de mostrar um total plausível e velho, que foi
 * exatamente o defeito que passou dezessete meses sem ninguém notar.
 */
export type Faturamento = {
  competenciaMaisRecente: string | null;
  valorDoUltimoMes: number;
  /** Quando verdadeiro, o valor do último mês é parcial: o mês ainda está correndo. */
  ultimoMesEstaAberto: boolean;
  serie: MesDeFaturamento[];
  topClientes: ClienteNoRanking[];
  metricasSemDado: MetricaSemDado[];
};
