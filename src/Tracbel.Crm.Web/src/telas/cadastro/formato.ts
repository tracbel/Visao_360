/**
 * Formatação de valores nas telas de cadastro.
 *
 * UMA COISA SÓ PARA ACERTAR, E ELA É SUTIL: a API grava e devolve UTC, e o
 * serializador do .NET manda `2026-09-04T23:17:53.96` — SEM o `Z` no fim quando
 * o `DateTime` é `Unspecified`. O navegador lê data sem fuso como HORÁRIO LOCAL,
 * e o resultado é um cadastro feito às 20h aparecendo como 23h. Acrescentar o
 * `Z` antes de interpretar é o que mantém a hora na tela igual à hora do relógio
 * de quem cadastrou.
 */

/** Interpreta o instante da API como UTC, mesmo quando ele vem sem o `Z`. */
function comoUtc(iso: string): Date {
  const temFuso = /(?:Z|[+-]\d{2}:?\d{2})$/.test(iso);
  return new Date(temFuso ? iso : `${iso}Z`);
}

/** Data e hora no fuso de quem está olhando. Ex.: `04/09/26 20:17`. */
export function formatarDataHora(iso: string | null): string {
  if (!iso) return '—';
  const data = comoUtc(iso);
  if (Number.isNaN(data.getTime())) return iso;
  return data.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
}

/** Só a data. */
export function formatarData(iso: string | null): string {
  if (!iso) return '—';
  const data = comoUtc(iso);
  if (Number.isNaN(data.getTime())) return iso;
  return data.toLocaleDateString('pt-BR');
}

/** Número com separador de milhar, ou travessão quando não há valor. */
export function formatarNumero(valor: number | null | undefined, casas = 0): string {
  if (valor === null || valor === undefined) return '—';
  return valor.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });
}

/**
 * Dinheiro em reais.
 *
 * NULO NÃO VIRA ZERO. A API devolve `null` — e não `0` — quando nenhum processo
 * da fase declara valor, porque zero diria "o funil vale nada" e o que se sabe é
 * outra coisa: que ninguém preencheu. A tela mantém a distinção.
 */
export function formatarDinheiro(valor: number | null | undefined): string {
  if (valor === null || valor === undefined) return '—';
  return valor.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
    maximumFractionDigits: 0,
  });
}
