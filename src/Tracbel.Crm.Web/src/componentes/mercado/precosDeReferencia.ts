/**
 * OS PREÇOS COM QUE O MERCADO ANUAL E O POTENCIAL INCREMENTAL FORAM FEITOS (issue 70, 27/09/2026) — uma frase só,
 * para o cartão da página e a ficha do município dizerem a mesma coisa.
 *
 * O número em reais sem o preço ao lado é um número que ninguém confere: a frase diz o preço de cada categoria, de
 * quantos meses de notas ele saiu e até quando.
 */

import type { MercadoAnual, PrecoDeReferenciaDaCategoria } from '../../tipos/territorio';
import { reaisCompactos } from '../territorio/escalas';

/** `aaaa-mm-dd` → `mm/aaaa`. */
const mesAno = (aaaammdd: string) => {
  const [ano, mes] = aaaammdd.split('-');
  return mes && ano ? `${mes}/${ano}` : aaaammdd;
};

function doPreco(p: PrecoDeReferenciaDaCategoria): string {
  const meses = p.meses === 1 ? '1 mês' : `${p.meses} meses`;
  return `${p.categoria}: ${reaisCompactos(p.preco)} (mediana de ${meses} de notas, até ${mesAno(p.ultimoMes)})`;
}

/**
 * A frase dos preços usados; nula quando a conta não trouxe preço nenhum.
 *
 * @param numero O mercado anual ou o potencial incremental.
 */
export function fraseDosPrecos(numero: MercadoAnual | null | undefined): string | null {
  const precos = numero?.precos ?? [];
  if (precos.length === 0) return null;

  return (
    `Preço de referência — ${precos.map(doPreco).join('; ')}. É a mediana das medianas mensais das notas de ` +
    'máquina do Protheus casadas com as vendas do ART nos últimos 12 meses.'
  );
}

/** A marca de parcial, para ir AO LADO do número: sem ela, a soma leria como o todo. */
export function marcaDeParcial(numero: MercadoAnual | null | undefined): string | null {
  return numero?.parcial ? `parcial — sem preço de ${numero.categoriasSemPreco.join(', ')}` : null;
}
