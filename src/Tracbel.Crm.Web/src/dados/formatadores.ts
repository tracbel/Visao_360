/**
 * Formatadores de número/moeda usados pelas telas Nova Oportunidade e Funil —
 * porte de `fmtBRL`, `fmtBRLfull`, `fmtNum` e `fmtBRLcompact`
 * (`prototipo/referencia/assets/app.js`, linhas 296-309).
 */

export function formatarBRL(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}

export function formatarBRLCompleto(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

export function formatarNumero(valor: number): string {
  return valor.toLocaleString('pt-BR');
}

export function formatarBRLCompacto(valor: number): string {
  if (valor >= 1000000) return `R$ ${(valor / 1000000).toFixed(valor >= 10000000 ? 1 : 2)}M`;
  if (valor >= 1000) return `R$ ${(valor / 1000).toFixed(0)}k`;
  return `R$ ${valor}`;
}
