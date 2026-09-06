/**
 * Tipos das telas Clientes e Ficha do Cliente — espelham as estruturas
 * extraídas do protótipo de referência (ver `prototipo/dados-seed/manifesto.json`
 * e `public/dados/*.json`). Mantidos junto ao porte para os dois telas
 * (`telas/Clientes.tsx` e `telas/ClienteFicha.tsx`) e seus componentes.
 */

export type Classe = 'A' | 'B' | 'C' | 'D';

export type CategoriaInteracao = 'visita' | 'ligacao' | 'whatsapp' | 'email' | 'remota';

export type UltimaInteracao = {
  data: string;
  cat: CategoriaInteracao;
  autor: string;
};

/** Item bruto de `carteira-cen.json` (CARTEIRA_CEN no protótipo) — a carteira base do CEN dono. */
export type ClienteCarteira = {
  id: number;
  razao: string;
  apelido: string;
  classe: Classe;
  cidade: string;
  uf: string;
  lat: number;
  lng: number;
  exige_visita: boolean;
  ult_int: UltimaInteracao | null;
  fat_12m: number;
  oportunidades: number;
  oportunidades_valor?: number;
  obs: string;
};

/** Atributos comerciais complementares por id de cliente (`clientes-extra.json`). */
export type ClienteExtra = {
  cnpj: string;
  segmento: string;
  porte: string;
  n_equipamentos: number;
  cen: string;
  ult_compra: string | null;
};

export type MapaClientesExtra = Record<string, ClienteExtra>;

/** Cliente de outra regional, já no formato "achatado" (`clientes-outras.json`). */
export type ClienteOutro = {
  id: number;
  razao: string;
  apelido: string;
  classe: Classe;
  cidade: string;
  uf: string;
  cnpj: string;
  segmento: string;
  porte: string;
  n_equipamentos: number;
  cen: string;
  ult_compra: string | null;
  ult_int: UltimaInteracao | null;
  fat_12m: number;
  oportunidades: number;
  oportunidades_valor?: number;
  exige_visita: boolean;
  obs: string;
};

/** Linha unificada da tabela — mesmo formato para clientes da carteira e de outras regionais. */
export type ClienteLinha = {
  id: number;
  razao: string;
  apelido: string;
  cnpj: string;
  classe: Classe;
  cidade: string;
  uf: string;
  segmento: string;
  porte: string;
  n_equipamentos: number;
  cen: string;
  fat_12m: number;
  oportunidades: number;
  oportunidades_valor: number;
  ult_compra: string | null;
  ult_int: UltimaInteracao | null;
  exige_visita: boolean;
  obs: string;
};

export type CenInfo = { nome: string; sigla: string; regional: string; cor: string };
export type MapaCens = Record<string, CenInfo>;

export type StatusCobertura = 'em_dia' | 'aviso' | 'atraso' | 'critico' | 'nunca';

export type Persona = 'cen' | 'regional' | 'nacional';

export type FiltrosClientes = {
  classe: 'todas' | Classe;
  cidade: string;
  status: 'todos' | StatusCobertura;
  segmento: string;
  porte: string;
};

export type CampoOrdenacao =
  | 'razao'
  | 'cidade'
  | 'classe'
  | 'segmento'
  | 'porte'
  | 'n_equipamentos'
  | 'fat_12m'
  | 'oportunidades_valor'
  | 'ult_compra';

export type OrdemClientes = { campo: CampoOrdenacao; dir: 'asc' | 'desc' };

/* ---------------------------------------------------------------------- */
/* Ficha do cliente                                                       */
/* ---------------------------------------------------------------------- */

export type ContatoCliente = {
  nome: string;
  cargo: string;
  celular: string;
  email: string;
  decisor: boolean;
  aniversario: string | null;
};

export type EnderecoAdicional = {
  tipo: string;
  cidade: string;
  hectares: number;
  cultura: string;
};

export type ItemFrota = {
  chassi: string;
  modelo: string;
  linha: string;
  ano: number;
  aquisicao: string;
  horas: number;
  garantia: string;
  status: string;
};

export type OportunidadeCliente = {
  id: number;
  titulo: string;
  linha: string;
  valor: number;
  fase: string;
  prob: number;
  previsao: string;
  cen: string;
};

export type InteracaoCliente = {
  data: string;
  tipo: string;
  autor: string;
  assunto: string;
  resultado: string;
  obs?: string;
};

export type AlteracaoPendente = {
  id: number;
  campo: string;
  valor_atual: string;
  valor_solicitado: string;
  solicitante: string;
  data: string;
  motivo: string;
  status: string;
};

export type ClienteFichaData = {
  id: number;
  razao_social: string;
  nome_fantasia: string;
  cnpj: string;
  ie: string;
  ie_uf: string;
  data_fundacao: string;
  tipo_pessoa: string;
  classe: Classe;
  status: string;
  bloqueio: string | null;
  regional: string;
  cen_dono: { user: string; nome: string; avatar: string };
  cen_pos_venda: { user: string; nome: string; avatar: string };
  origem_inclusao: string;
  origem_alteracao: string;
  endereco_principal: {
    logradouro: string;
    numero: string;
    complemento: string;
    bairro: string;
    cidade: string;
    uf: string;
    cep: string;
    pais: string;
    latitude: number;
    longitude: number;
  };
  enderecos_adicionais: EnderecoAdicional[];
  preferencias_contato: Record<string, boolean>;
  consentimento_lgpd: { ativo: boolean; data: string; versao: string };
  contatos: ContatoCliente[];
  telefones: { tipo: string; numero: string }[];
  emails: string[];
  home_page: string;
  segmentacao: {
    grupo: string;
    atividade: string;
    rota: string;
    faturamento_declarado: string;
    porte: string;
    hectares_totais: number;
    culturas: string[];
  };
  frota: ItemFrota[];
  oportunidades: OportunidadeCliente[];
  interacoes: InteracaoCliente[];
  financeiro: {
    faturamento_ytd_2026: number;
    faturamento_2025: number;
    faturamento_2024: number;
    limite_credito: number;
    limite_usado: number;
    limite_disponivel: number;
    inadimplencia: number;
    dias_atraso_max: number;
    ultima_fatura: string;
  };
  alteracoes_pendentes: AlteracaoPendente[];
};

/* ---------------------------------------------------------------------- */
/* Pós-vendas (aba da ficha)                                              */
/* ---------------------------------------------------------------------- */

export type OsAberta = {
  id: string;
  chassi: string;
  modelo: string;
  tipo: string;
  abertura: string;
  previsao: string;
  status: string;
  status_cor: string;
  valor: number;
  tecnico: string;
  diagnostico: string;
  dias_aberta: number;
  sla_dias: number;
};

export type PmpPendente = {
  id: string;
  codigo_jd: string;
  titulo: string;
  criticidade: string;
  criticidade_cor: string;
  chassi_afetado: string[];
  modelo: string;
  qtd_equipamentos: number;
  publicacao: string;
  prazo: string;
  dias_restantes: number;
  cobertura: string;
  duracao_estimada: string;
  descricao: string;
  status: string;
  status_cor: string;
  agenda?: string;
};

export type AlertaCritico = {
  id: string;
  tipo: string;
  cor: string;
  titulo: string;
  detalhe: string;
  origem: string;
  criado_em: string;
  acao_cta: string;
  acao_link: string;
};

export type ContratoJdcp = {
  chassi: string;
  modelo: string;
  plano: string;
  inicio: string;
  fim: string;
  horas_cobertas: string;
  valor_anual: number;
};

export type FrotaSemJdcp = { chassi: string; modelo: string; ano: number; potencial: number };

export type PosVendasCliente = {
  fat_pv: {
    fytd_pecas: number;
    fytd_servicos: number;
    fytd_total: number;
    ytd_2026_pecas: number;
    ytd_2026_servicos: number;
    ytd_2026_total: number;
    ano_2025_total: number;
    ano_2024_total: number;
    ticket_medio_pecas: number;
    mix_canal: { balcao: number; oficina_interna: number; campo_tecnico: number };
  };
  fat_12m: { mes: string; pecas: number; servicos: number }[];
  os_abertas: OsAberta[];
  pmp_pendentes: PmpPendente[];
  alertas_criticos: AlertaCritico[];
  contratos_jdcp: ContratoJdcp[];
  frota_sem_jdcp: FrotaSemJdcp[];
  nps: {
    score: number;
    respostas: number;
    detratores: number;
    neutros: number;
    promotores: number;
    comentario_recente: { data: string; autor: string; nota: number; texto: string };
  };
};
