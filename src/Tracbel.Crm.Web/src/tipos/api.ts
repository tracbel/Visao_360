/**
 * Os contratos da nossa API, do jeito que eles chegam no navegador.
 *
 * Espelham, campo a campo, os `record` de `Tracbel.Crm.Aplicacao` documentados em
 * `docs/projeto/23-API.md`. A serialização do ASP.NET Core é camelCase, então o
 * nome aqui é o mesmo nome de lá com a primeira letra minúscula — e nada é
 * renomeado no caminho, para quem lê o documento 23 achar o campo na tela.
 *
 * NADA AQUI É INVENTADO. Campo que a API não devolve não existe neste arquivo;
 * se a tela precisa de algo que não está aqui, o caminho é o documento 23, não
 * um campo opcional a mais.
 */

/* ---------------------------------------------------------------------- */
/* Envelope comum                                                          */
/* ---------------------------------------------------------------------- */

/**
 * De onde veio o dado e quando — o carimbo que acompanha TODA leitura, do nosso
 * banco e da ponte do legado (documento 23, seção 1).
 */
export type Procedencia = {
  /** Quem respondeu. Ex.: `CRM Tracbel`, `Vórtice`. */
  sistema: string;
  /** O que foi lido, no vocabulário do sistema de origem. Ex.: `comercial.Cliente`. */
  objeto: string;
  /** Quando NÓS lemos (UTC). É a idade da resposta, não a do dado. */
  lidoEmUtc: string;
  /** A alteração mais recente encontrada na origem (UTC), quando a origem data. */
  dadoMaisRecenteEm: string | null;
  /** Verdadeiro quando o dado é velho o bastante para a tela avisar. */
  estaDesatualizado: boolean;
  /** O que dizer ao usuário, em português, quando há o que dizer. */
  aviso: string | null;
};

/** O envelope de toda leitura: o dado e o carimbo de origem, juntos. */
export type ComProcedencia<T> = { dados: T; procedencia: Procedencia };

/** Uma fatia de listagem, com o suficiente para montar a barra de paginação. */
export type PaginaDe<T> = {
  itens: T[];
  pagina: number;
  tamanho: number;
  total: number;
  totalDePaginas: number;
  temProxima: boolean;
};

/* ---------------------------------------------------------------------- */
/* Erro                                                                    */
/* ---------------------------------------------------------------------- */

/**
 * Um erro de entrada, já endereçado ao campo que o causou.
 *
 * É O QUE FAZ A MENSAGEM CAIR NO CAMPO CERTO: `campo` é o nome do campo no
 * contrato (`nomeRazao`, `documento`, `origemCodigo`), e o formulário usa esse
 * nome como chave para acender o campo — nunca um aviso genérico no topo.
 */
export type ErroDeCampo = {
  campo: string;
  mensagem: string;
  valorRecebido: string | null;
};

/** O corpo `application/problem+json` que toda recusa da API devolve. */
export type ProblemaDaApi = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  erros?: ErroDeCampo[];
};

/* ---------------------------------------------------------------------- */
/* Catálogos                                                               */
/* ---------------------------------------------------------------------- */

/** Um item pronto para virar opção de um campo de seleção. */
export type ItemDeSelecao = {
  /** O código estável — é ele que vai para o banco e para o histórico. */
  codigo: string;
  /** O rótulo que o usuário lê. */
  descricao: string;
  ordem: number;
  /** Escolher este item obriga a escrever uma observação (o item "Outro"). */
  exigeObservacao: boolean;
};

/** Um catálogo inteiro, com os itens ativos na ordem em que a tela deve oferecê-los. */
export type CatalogoDeSelecao = {
  codigo: string;
  nome: string;
  descricao: string | null;
  /** Falso nos domínios fechados de código: a tela não oferece "novo item". */
  permiteItemNovo: boolean;
  itens: ItemDeSelecao[];
};

/** Os códigos de catálogo que estas telas consomem. */
export const CATALOGO = {
  situacaoCliente: 'SITUACAO_CLIENTE',
  tipoDePessoa: 'TIPO_DE_PESSOA',
  origemLead: 'ORIGEM_LEAD',
  motivoInativacao: 'MOTIVO_INATIVACAO',
  situacaoEquipamento: 'SITUACAO_EQUIPAMENTO',
  origemEquipamento: 'ORIGEM_EQUIPAMENTO',
  modeloEquipamento: 'MODELO_EQUIPAMENTO',
  linhaDeProduto: 'LINHA_DE_PRODUTO',
  porteDeMaquina: 'PORTE_DE_MAQUINA',
  empresa: 'EMPRESA',
} as const;

/* ---------------------------------------------------------------------- */
/* Cliente                                                                 */
/* ---------------------------------------------------------------------- */

/** O cliente como a LISTAGEM o mostra. A chave é o GUID, nunca o id interno. */
export type ClienteResumo = {
  chave: string;
  nomeRazao: string;
  nomeFantasia: string | null;
  tipoDePessoa: string;
  documento: string | null;
  situacao: string;
  criadoEm: string;
  alteradoEm: string | null;
  estaInativo: boolean;
};

/** O cliente como a FICHA o mostra, com o carimbo de concorrência. */
export type ClienteDetalhe = ClienteResumo & {
  documentoSemMascara: string | null;
  inscricaoEstadual: string | null;
  atividadeEconomica: string | null;
  situacaoDesde: string;
  empresaId: number;
  proprietarioId: number;
  origemCodigo: string | null;
  motivoInativacaoCodigo: string | null;
  /** O carimbo de concorrência, em base64. Volta no PUT e no DELETE. */
  versao: string | null;
};

/** O corpo do POST de cliente. Tudo texto: a recusa é da API, campo a campo. */
export type NovoCliente = {
  nomeRazao: string;
  nomeFantasia: string;
  tipoDePessoa: string;
  documento: string;
  inscricaoEstadual: string;
  atividadeEconomica: string;
  situacao: string;
  origemCodigo: string;
};

/** O corpo do PUT de cliente: o mesmo do POST mais a versão lida no GET. */
export type AlteracaoDeCliente = NovoCliente & { versao: string | null };

/** O corpo do DELETE de cliente: o motivo é de catálogo e é obrigatório. */
export type InativacaoDeCliente = { motivoCodigo: string; versao: string | null };

/** Parâmetros da listagem de clientes — os mesmos do documento 23, seção 2.1. */
export type ConsultaDeClientes = {
  pagina: number;
  tamanho: number;
  termo: string;
  situacao: string;
  tipoDePessoa: string;
  ordenarPor: OrdemDeCliente;
  descendente: boolean;
  incluirInativos: boolean;
};

/** Domínio FECHADO de ordenação: o que não está aqui a API recusa. */
export const ORDENS_DE_CLIENTE = ['Nome', 'CriadoEm', 'Situacao', 'AlteradoEm'] as const;
export type OrdemDeCliente = (typeof ORDENS_DE_CLIENTE)[number];

/* ---------------------------------------------------------------------- */
/* Equipamento                                                             */
/* ---------------------------------------------------------------------- */

/** A máquina como a listagem a mostra. */
export type EquipamentoResumo = {
  chave: string;
  chassi: string;
  modeloCodigo: string | null;
  modeloNome: string | null;
  marca: string | null;
  /** Falso quando é máquina de concorrente — é o que a Cobertura usa. */
  marcaRepresentada: boolean | null;
  situacao: string;
  origem: string;
  anoModelo: number | null;
  clienteChave: string | null;
  clienteNome: string | null;
  criadoEm: string;
  alteradoEm: string | null;
  estaInativo: boolean;
  /** A classificação de produto (documento 35, seção 10). Ex.: `TRATOR_MEDIO`. */
  classificacaoCodigo: string | null;
  classificacaoNome: string | null;
  /** `Pequeno`, `Medio`, `Grande` ou `NaoSeAplica`. */
  porte: string | null;
  /** Quantas vendas a máquina tem registradas (ART). */
  vendas: number;
  ultimaVendaEm: string | null;
  /** O comprador NA venda mais recente — nunca apresentado como dono atual. */
  compradorNaUltimaVendaChave: string | null;
  compradorNaUltimaVendaNome: string | null;
  /** `CompradorNaVenda` quando há venda. */
  naturezaDoVinculo: string | null;
  produtoNaOrigem: string | null;
  sistemaDaVenda: string | null;
};

/** A máquina como a ficha a mostra, com o carimbo de concorrência. */
export type EquipamentoDetalhe = EquipamentoResumo & {
  numeroSerie: string | null;
  placa: string | null;
  familia: string | null;
  anoFabricacao: number | null;
  horimetroAtual: number | null;
  localizacaoDescrita: string | null;
  empresaId: number;
  versao: string | null;
  /** As divergências abertas entre ART, CRM e Protheus sobre esta máquina. */
  divergenciasAbertas: DivergenciaDaMaquina[];
};

/** Uma divergência aberta sobre a máquina — documento 35, seção 10. */
export type DivergenciaDaMaquina = {
  tipo: string;
  descricao: string;
  detectadaEm: string;
};

/** Uma máquina que o cliente comprou numa venda registrada (vínculo "comprador na venda"). */
export type MaquinaCompradaPeloCliente = {
  equipamentoChave: string;
  chassi: string;
  modeloNome: string | null;
  classificacaoNome: string | null;
  produtoNaOrigem: string | null;
  vendidaEm: string | null;
  natureza: string;
  filialCodigo: string;
  sistemaCodigo: string;
  /** Se o cliente também é o dono atual registrado. Comprar não faz dono. */
  ehDonoAtual: boolean;
};

/** Uma execução do serviço de sincronização — documento 35, seção 11. */
export type ExecucaoDeSincronizacao = {
  maquina: string;
  iniciadaEm: string;
  terminadaEm: string | null;
  resultado: 'EmAndamento' | 'Sucesso' | 'Falha' | 'Ignorada' | string;
  tentativas: number;
  registrosLidos: number;
  incluidos: number;
  atualizados: number;
  pendentes: number;
  mensagem: string | null;
};

/** Um fluxo de sincronização com a última execução, o último sucesso e as recentes. */
export type SituacaoDaSincronizacao = {
  sistemaCodigo: string;
  fluxo: string;
  ultimaExecucaoEm: string | null;
  ultimoResultado: string | null;
  ultimoSucessoEm: string | null;
  execucoes: ExecucaoDeSincronizacao[];
};

/** O corpo do POST de equipamento. */
export type NovoEquipamento = {
  chassi: string;
  modeloCodigo: string;
  clienteChave: string;
  situacao: string;
  origem: string;
  anoFabricacao: string;
  anoModelo: string;
  numeroSerie: string;
  placa: string;
  localizacaoDescrita: string;
  /** Do catálogo `LINHA_DE_PRODUTO`. No PUT, texto vazio retira a classificação. */
  linhaDeProdutoCodigo: string;
};

/**
 * O corpo do PUT de equipamento.
 *
 * CHASSI E ORIGEM NÃO ESTÃO AQUI de propósito, e a razão é do domínio: o chassi
 * é a identidade da máquina e a chave de deduplicação; a origem diz quem afirma
 * que a máquina existe. Nenhum dos dois muda por edição de tela — chassi errado
 * se corrige inativando e cadastrando o certo (documento 23, seção 2.2).
 */
export type AlteracaoDeEquipamento = Omit<NovoEquipamento, 'chassi' | 'origem'> & {
  versao: string | null;
};

/** O corpo do DELETE de equipamento: só a versão. */
export type BaixaDeEquipamento = { versao: string | null };

/** Parâmetros da listagem de equipamentos — documento 23, seção 2.2. */
export type ConsultaDeEquipamentos = {
  pagina: number;
  tamanho: number;
  termo: string;
  situacao: string;
  origem: string;
  clienteChave: string;
  ordenarPor: OrdemDeEquipamento;
  descendente: boolean;
  incluirInativos: boolean;
  /** Código da classificação, ou `SEM_CLASSIFICACAO`. Filtro de banco. */
  linhaDeProduto: string;
  /** `Pequeno`, `Medio`, `Grande` ou `NaoSeAplica`. Filtro de banco. */
  porte: string;
  /** Só as máquinas com venda registrada. */
  somenteComVenda: boolean;
};

/** Domínio FECHADO de ordenação de equipamento. */
export const ORDENS_DE_EQUIPAMENTO = ['Chassi', 'CriadoEm', 'Situacao', 'AnoModelo'] as const;
export type OrdemDeEquipamento = (typeof ORDENS_DE_EQUIPAMENTO)[number];

/** O valor do filtro de classificação que traz as máquinas sem classificação. */
export const SEM_CLASSIFICACAO = 'SEM_CLASSIFICACAO';

/**
 * Uma venda da máquina — o histórico comercial (documento 35, seção 10).
 *
 * O COMPRADOR É O DA VENDA, naquela data. Ele não é o dono atual: o dono está em
 * `EquipamentoDetalhe.clienteChave`, e só uma pessoa o confirma.
 */
export type VendaDaMaquina = {
  chave: string;
  sistemaCodigo: string;
  chaveOrigem: string;
  vendidaEm: string | null;
  faturadaEm: string | null;
  entregueEm: string | null;
  registradaNaOrigemEm: string | null;
  filialCodigo: string;
  filialNome: string;
  filialDoFaturamentoCodigo: string | null;
  compradorChave: string | null;
  compradorNome: string | null;
  natureza: string | null;
  vinculoReferenciaEm: string | null;
  vinculoEncerradoEm: string | null;
  motivoDoEncerramento: string | null;
  linhaNaOrigem: string;
  produtoNaOrigem: string;
  /** Varejo ou Grandes Contas — atributo DA VENDA, não do cliente. */
  gestaoNaOrigem: string | null;
  situacaoNaOrigem: string | null;
  numeroDoPedido: string | null;
  numeroDaNotaFiscal: string | null;
  vendaDireta: boolean;
  repasseDireto: boolean;
  unidadeNaOrigem: string | null;
  unidadeDoFaturamentoNaOrigem: string | null;
  transformacoes: string | null;
  importadaEm: string;
  atualizadaPelaOrigemEm: string | null;
};
