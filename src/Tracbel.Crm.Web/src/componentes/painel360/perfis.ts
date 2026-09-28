/** Os três perfis da Visão 360. O CEN trabalha o cliente; os outros dois abrem o consolidado das filiais. */
export const PERFIS = [
  { id: 'diretoria', rotulo: 'Diretoria comercial' },
  { id: 'gerente', rotulo: 'Gerente regional' },
  { id: 'cen', rotulo: 'CEN — carteira e cliente' },
] as const;

export type PerfilId = (typeof PERFIS)[number]['id'];
