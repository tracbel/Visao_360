import type { BandasDoPorte } from '../../tipos/territorio';

/**
 * Por que o porte não tem nome — a mesma frase na régua do mercado e na faixa de porte e momento.
 *
 * DESDE 28/09/2026 AS BANDAS NÃO ESPERAM NINGUÉM: sem banda registrada, a apuração calcula os tercis da ADR (o
 * critério decidido em 27/09). O porte só fica sem nome quando nem isso sai — e a frase diz os dois casos.
 */
export const MOTIVO_SEM_PORTE =
  'O porte não saiu neste recorte. O nome compara a demanda anual média por município do recorte com as bandas — as ' +
  'registradas nos parâmetros gerais ou, sem elas, os tercis da demanda dos municípios da ADR (issue 166). Falta um ' +
  'dos dois: o recorte não tem município da ADR com demanda anual, ou a ADR tem menos de três municípios com demanda ' +
  '(o tercil não corta). Nulo aqui NÃO quer dizer "pequeno".';

const n1 = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });

/** As bandas por extenso, com a origem — o que a dica do porte acrescenta quando ele tem nome. */
export function descricaoDasBandas(bandas: BandasDoPorte | null | undefined): string {
  if (!bandas) return '';
  const origem =
    bandas.origem === 'Registradas'
      ? 'registradas nos parâmetros gerais'
      : `pelos tercis dos ${bandas.municipiosNaBase ?? ''} municípios da ADR com demanda, calculados na apuração — ` +
        'ninguém registrou outras; em Configurações › Parâmetros do potencial dá para fixá-las';
  return (
    ` Bandas: médio a partir de ${n1(bandas.medioAPartirDe)} e grande a partir de ${n1(bandas.grandeAPartirDe)} ` +
    `máquinas por ano, ${origem}. O município típico deste recorte tem ${n1(bandas.demandaMediaDoRecorte)} máquinas por ano.`
  );
}
