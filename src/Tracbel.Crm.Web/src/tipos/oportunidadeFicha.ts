/**
 * Tipos da Ficha de Oportunidade — espelham `OPORTUNIDADE_DATA`
 * (`prototipo/referencia/assets/app.js`, linha 2712) e o JSON extraído em
 * `public/dados/oportunidade-1517613.json`.
 *
 * Nome do arquivo não é só `oportunidade.ts` de propósito: esse nome já é
 * usado pelos tipos da tela Nova Oportunidade (formulário/localStorage), que
 * têm um formato bem diferente do registro completo da ficha.
 */

export type OportunidadeCliente = {
  id: number;
  razao: string;
  cnpj: string;
  classe: string;
  cidade: string;
};

export type OportunidadeCen = {
  nome: string;
  regional: string;
  unidade: string;
};

export type OportunidadeAprovacaoDesconto = 'aprovado' | 'dentro_alcada' | 'pendente' | 'rejeitado';

export type OportunidadeItem = {
  id: number;
  codigo: string;
  descricao: string;
  qtd: number;
  valor_unitario_lista: number;
  valor_unitario: number;
  desconto_pct: number;
  aprovacao_desconto: OportunidadeAprovacaoDesconto;
  aprovador?: string;
  aprovado_em?: string;
  alcada_limite_pct: number;
  total: number;
};

export type OportunidadeFaseStatus = 'concluida' | 'atual' | 'futura';

export type OportunidadeFase = {
  key: string;
  label: string;
  inicio: string | null;
  fim: string | null;
  dias?: number;
  status: OportunidadeFaseStatus;
  bloqueio?: string;
  aprovacao?: string;
};

export type OportunidadeAprovacaoStatus = 'pendente' | 'aprovada' | 'rejeitada' | 'nao_iniciada';

export type OportunidadeAprovacao = {
  id: string;
  tipo: string;
  escopo: string;
  solicitante?: string;
  solicitado_em?: string;
  alcada_regra: string;
  aprovador_atual?: string;
  status: OportunidadeAprovacaoStatus;
  decidido_em?: string;
  comentario?: string | null;
  sla_horas?: number;
  sla_decorridas?: number;
  previsto_em_fase?: string;
};

export type OportunidadeDocumentoStatus = 'aguardando_cliente' | 'nao_aplicavel' | 'anexado' | 'nao_iniciado';

export type OportunidadeDocumento = {
  nome: string;
  obrigatorio: boolean;
  status: OportunidadeDocumentoStatus;
  quem?: string;
  envio_em?: string;
  prazo?: string;
  anexo?: string;
  anexado_em?: string;
  anexado_por?: string;
};

export type OportunidadeDocumentacao = {
  proposta: OportunidadeDocumento[];
  faturamento: OportunidadeDocumento[];
};

export type OportunidadeHistoricoTipo = 'aprovacao' | 'fase' | 'criacao' | 'doc';

export type OportunidadeHistoricoItem = {
  data: string;
  autor: string;
  tipo: OportunidadeHistoricoTipo;
  acao: string;
  descricao: string;
};

export type OportunidadeAcesso = {
  papel: string;
  pessoa: string;
  permissao: string;
};

export type OportunidadeMacroStatus = 'aberto' | 'fechado_ganho' | 'fechado_perdido' | 'cancelado';

export type OportunidadeFicha = {
  id: number;
  numero: string;
  titulo: string;
  cliente: OportunidadeCliente;
  cen: OportunidadeCen;
  linha: string;
  origem: string;
  criada_em: string;
  atualizada_em: string;
  previsao_fechamento: string;
  probabilidade: number;
  fase_atual: string;
  macro_status: OportunidadeMacroStatus;
  valor_total: number;
  desconto_total: number;
  valor_lista: number;
  itens: OportunidadeItem[];
  timeline_fases: OportunidadeFase[];
  aprovacoes: OportunidadeAprovacao[];
  documentacao: OportunidadeDocumentacao;
  historico: OportunidadeHistoricoItem[];
  acessos: OportunidadeAcesso[];
};
