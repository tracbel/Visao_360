/**
 * OS FORMATOS DE NÚMERO, MOEDA E DATA DAS TELAS (documento 54 §3.5) — cada um num lugar só; a regra de arquitetura
 * (`arquitetura.teste.ts`) reprova a cópia nova. Os quatro primeiros são o porte de `fmtBRL`, `fmtBRLfull`, `fmtNum` e
 * `fmtBRLcompact` (`prototipo/referencia/assets/app.js`, linhas 296-309); os outros eram cópias idênticas em duas ou mais
 * telas até 03/10/2026.
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

/** Número com no máximo `casas` decimais: `1.234,6`. */
export function formatarNumeroComCasas(valor: number, casas: number): string {
  return valor.toLocaleString('pt-BR', { maximumFractionDigits: casas });
}

/** O percentual inteiro de uma parte: `33%`. Sem denominador, o travessão. */
export function formatarPercentualDaParte(parte: number, todo: number): string {
  if (todo <= 0) return '—';
  return `${Math.round((parte / todo) * 100)}%`;
}

/** O eixo do gráfico, curto: `R$ 50 mil`, `R$ 1,2 mi`; abaixo de mil, a moeda inteira. */
export function formatarReaisCurtos(valor: number): string {
  if (Math.abs(valor) >= 1_000_000) return `R$ ${(valor / 1_000_000).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} mi`;
  if (Math.abs(valor) >= 1_000) return `R$ ${(valor / 1_000).toLocaleString('pt-BR', { maximumFractionDigits: 0 })} mil`;
  return formatarBRL(valor);
}

export const MESES_CURTOS: readonly string[] = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
export const DIAS_DA_SEMANA_CURTOS: readonly string[] = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];

/** O instante no horário de quem lê, com o dia da semana: `sáb · 03/out · 21:07` — a régua das fichas. */
export function formatarDiaDaSemanaEHora(iso: string): string {
  const d = new Date(iso);
  return `${DIAS_DA_SEMANA_CURTOS[d.getDay()]} · ${d.getDate().toString().padStart(2, '0')}/${MESES_CURTOS[d.getMonth()]} · ${d
    .getHours()
    .toString()
    .padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
}

/** O instante da API (UTC, às vezes sem o `Z`) no fuso de quem lê: `2026-09-24T16:27:48` vira `24/09 13:27`. */
export function formatarDiaEHora(instante: string): string {
  const utc = /Z|[+-]\d\d:\d\d$/.test(instante) ? instante : `${instante}Z`;
  return new Date(utc).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
}

/** A competência `aaaa-mm` (ou `aaaa-mm-dd`) como mês curto: `set/26`. */
export function formatarMesCurto(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${MESES_CURTOS[Number(mes) - 1]}/${ano.slice(2)}`;
}

/** A competência com o ano inteiro: `set/2026`. */
export function formatarMesComAno(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${MESES_CURTOS[Number(mes) - 1]}/${ano}`;
}

/** A competência como `09/2026`; o que não for competência volta como veio. */
export function formatarMesNumerico(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return mes && ano ? `${mes}/${ano}` : competencia;
}
