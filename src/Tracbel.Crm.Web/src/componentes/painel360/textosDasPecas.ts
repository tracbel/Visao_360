/**
 * Como a ficha escreve o setor e o grupo comercial das peças (02/10/2026). O Protheus e o BI gravam em maiúsculas e sem
 * acento ("PECAS", "LUBRIFICANTE"); a tela mostra o nome que o comercial fala. O que não está na lista sai como veio.
 */

const GRUPOS: Record<string, string> = {
  PECAS: 'Peças',
  PNEU: 'Pneus',
  BATERIA: 'Baterias',
  ADITIVOS: 'Aditivos',
  COOLGARD: 'Coolgard',
  FORQUIMICA: 'Forquímica',
  GRAXAS: 'Graxas',
  LUBRIFICANTE: 'Lubrificantes',
  METISA: 'Metisa',
  TEEJET: 'TeeJet',
  'UNIMIL BY JD': 'Unimil by JD',
  'PRECISION UPGRADE': 'Precision Upgrade',
  'S/CLASSIFICAÇÃO': 'Sem classificação',
};

const SETORES: Record<string, string> = {
  BALCAO: 'Balcão',
  BALCÃO: 'Balcão',
  OFICINA: 'Oficina',
  'S/CLASSIFICAÇÃO': 'Sem setor',
};

/** O grupo comercial como o comercial fala. */
export function nomeDoGrupo(grupo: string): string {
  return GRUPOS[grupo.trim().toUpperCase()] ?? grupo;
}

/** O setor como o comercial fala. */
export function nomeDoSetor(setor: string): string {
  const limpo = setor.trim().toUpperCase();
  return SETORES[limpo] ?? setor.charAt(0).toUpperCase() + setor.slice(1).toLowerCase();
}
