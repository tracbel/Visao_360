/**
 * Tipos da tela Configurações — espelham as estruturas `CONFIG_*` de
 * `prototipo/referencia/assets/app.js` (linhas 5094-5918) e o formato dos
 * JSONs em `public/dados/config-*.json`.
 */

export type AbaConfig = 'preferencias' | 'comercial' | 'ti';

export type SecaoConfig =
  | 'perfil'
  | 'notificacoes'
  | 'atalhos'
  | 'metas'
  | 'aprovacoes'
  | 'taxonomias'
  | 'integracoes'
  | 'usuarios'
  | 'permissoes'
  | 'auditoria';

/** `CONFIG_METAS` — metas comerciais e SLAs de aprovação (config-metas.json). */
export type ConfigMetas = {
  frequencia_visita: Record<'A' | 'B' | 'C' | 'D', number>;
  frequencia_ligacao: Record<'A' | 'B' | 'C' | 'D', number>;
  sla_aprovacao_diretor: number;
  sla_aprovacao_gerente: number;
  sla_aprovacao_financeiro: number;
  desconto_ate_gerente: number;
  desconto_ate_diretor: number;
  desconto_ate_presidente: number;
};

/** `CONFIG_MOTIVOS_PERDA` (config-motivos-perda.json). */
export type ConfigMotivoPerda = {
  id: number;
  motivo: string;
  ativo: boolean;
  usos_90d: number;
};

/** `CONFIG_CATEGORIAS_INTERACAO` (config-categorias-interacao.json). */
export type ConfigCategoriaInteracao = {
  id: number;
  nome: string;
  vale_cobertura: boolean;
  ativo: boolean;
};

export type StatusIntegracao = 'online' | 'pendente' | 'degradado' | 'somente_leitura' | 'offline';

/** `CONFIG_INTEGRACOES` (config-integracoes.json). */
export type ConfigIntegracao = {
  nome: string;
  tipo: string;
  icone: string;
  status: StatusIntegracao;
  ambiente: string;
  endpoint: string;
  last_sync: string | null;
  frequencia: string;
  donos: string;
  notas: string;
};

/** `CONFIG_USUARIOS` (config-usuarios.json). */
export type ConfigUsuario = {
  id: number;
  nome: string;
  email: string;
  role: string;
  regional: string;
  ultimo_login: string;
};

/** `CONFIG_ROLES` (config-roles.json). */
export type ConfigRole = {
  role: string;
  usuarios: number;
  descricao: string;
};

/** Evento de auditoria — mock local (não vira JSON: já era local a `renderSecaoAuditoria` no original). */
export type EventoAuditoria = {
  data: string;
  usuario: string;
  acao: string;
  ip: string;
};
