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

/** Conectivos que não viram inicial e ficam em minúscula: "Hamilton de Souza Lopes". */
const CONECTIVOS = new Set(['da', 'das', 'de', 'do', 'dos', 'e']);

/** As siglas do negócio que ficam em caixa alta no meio de um nome: "Implementos CEN", e não "Implementos Cen". */
const SIGLAS = new Set(['CEN', 'AMS', 'ART', 'GN', 'JD', 'ABC']);

/** "JOÃO" vira "João"; o login ou a sigla com ponto (GESTOR.NORTE, INT.MERCADO) e as siglas do negócio ficam como estão. */
function capitular(palavra: string): string {
  if (palavra.includes('.')) return palavra;
  const maiuscula = palavra.toLocaleUpperCase('pt-BR');
  if (SIGLAS.has(maiuscula)) return maiuscula;
  return palavra.charAt(0).toLocaleUpperCase('pt-BR') + palavra.slice(1).toLocaleLowerCase('pt-BR');
}

/**
 * O NOME COMO AS MAQUETES ESCREVEM NOS GRÁFICOS: primeiro nome, a inicial do segundo e o último — "Matheus A. Cruz"
 * (Performance de CEN e Forecast da Gerência, 01/10/2026). O nome inteiro continua na tabela e no leitor de tela.
 */
export function nomeCurto(nome: string): string {
  const partes = nome.trim().split(/\s+/);
  if (partes.length === 1) return nome;
  const primeiro = capitular(partes[0]);
  const ultimo = capitular(partes[partes.length - 1]);
  const meio = partes.slice(1, -1).find((p) => !CONECTIVOS.has(p.toLocaleLowerCase('pt-BR')));
  return meio ? `${primeiro} ${meio.charAt(0).toLocaleUpperCase('pt-BR')}. ${ultimo}` : `${primeiro} ${ultimo}`;
}

/** O nome inteiro em caixa de nome próprio — "Gestor Fictício da Regional Norte" —, com os conectivos em minúscula. */
export function nomeProprio(nome: string): string {
  return nome
    .trim()
    .split(/\s+/)
    .map((p, i) => (i > 0 && CONECTIVOS.has(p.toLocaleLowerCase('pt-BR')) ? p.toLocaleLowerCase('pt-BR') : capitular(p)))
    .join(' ');
}
