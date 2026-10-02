/**
 * OS PARÂMETROS DE DEMANDA POR CULTURA — de onde vem o número (issue 258; desenho da maquete de 02/10/2026).
 *
 * AS COLUNAS DA MAQUETE, com os números da conta: a área plantada (PAM), os hectares por máquina da regra, a renovação
 * anual (1 ÷ os anos de renovação), a frota estimada (o parque), a demanda e a ajustada, e o efeito do momento. O ano
 * anterior da PAM mora na dica de cada célula.
 *
 * HECTARES POR MÁQUINA, E NÃO "MÁQUINAS/HA" como escreve a maquete: é o parâmetro como ele é registrado em Configurações ›
 * Potencial de mercado — o inverso seria um número que ninguém digitou.
 */

import { Citrus, Coffee, Leaf, Nut, Sprout, Wheat, type LucideIcon } from 'lucide-react';
import type { DemandaDaCultura } from '../../tipos/mercado';
import { n, variacaoPercentual } from './demanda';

const ICONE_DA_CULTURA: Record<string, { Icone: LucideIcon; cor: string }> = {
  SOJA: { Icone: Leaf, cor: '#3f8f3a' },
  MILHO: { Icone: Wheat, cor: '#d29b16' },
  CANA: { Icone: Sprout, cor: '#2f9e5b' },
  CAFE: { Icone: Coffee, cor: '#8a5a2b' },
  LARANJA: { Icone: Citrus, cor: '#e8811a' },
  AMENDOIM: { Icone: Nut, cor: '#b07a3a' },
};

/** A variação de hoje sobre o ano anterior, para a dica: "2023: 720 (+25%)". */
function antes(rotulo: string, atual: number | null, anterior: number | null, casas = 0) {
  if (anterior === null) return `${rotulo}: sem o ano anterior na PAM`;
  const v = atual !== null && anterior > 0 ? ` (${variacaoPercentual((atual / anterior - 1) * 100)})` : '';
  return `${rotulo}: ${n(anterior, casas)}${v}`;
}

export function ParametrosPorCultura({
  culturas,
  comAjustada,
  anoAnterior,
}: {
  culturas: DemandaDaCultura[];
  comAjustada: boolean;
  /** O ano da PAM da comparação, para a dica. */
  anoAnterior: number | null;
}) {
  const ano = anoAnterior === null ? 'Ano anterior' : String(anoAnterior);

  if (culturas.length === 0) return <p className="diag-nota">Nenhuma cultura com demanda neste recorte.</p>;

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela dem-culturas">
        <caption className="cad-so-leitor">Os parâmetros e a demanda de cada cultura no recorte</caption>
        <thead>
          <tr>
            <th scope="col">Cultura</th>
            <th scope="col" className="mom-num">Área (ha)</th>
            <th scope="col" className="mom-num">ha/máquina</th>
            <th scope="col" className="mom-num">Renovação anual</th>
            <th scope="col" className="mom-num">Frota estimada</th>
            <th scope="col" className="mom-num">Demanda (un.)</th>
            <th scope="col" className="mom-num">Ajustada (un.)</th>
            <th scope="col" className="mom-num">Efeito do momento</th>
          </tr>
        </thead>
        <tbody>
          {culturas.map((c) => {
            const icone = ICONE_DA_CULTURA[c.culturaCodigo];
            return (
              <tr key={c.culturaCodigo}>
                <th scope="row">
                  <span className="dem-cultura">
                    {icone ? <icone.Icone size={15} strokeWidth={2.2} style={{ color: icone.cor }} aria-hidden="true" /> : <span className="dem-cultura-sem-icone" aria-hidden="true" />}
                    {c.cultura}
                  </span>
                </th>
                <td className="mom-num" title={antes(`Área em ${ano}`, c.areaUtilHectares, c.areaAnoAnterior)}>
                  {c.areaUtilHectares === null ? '—' : n(c.areaUtilHectares, 0)}
                </td>
                <td className="mom-num">{c.hectaresPorMaquina === null ? '—' : n(c.hectaresPorMaquina, 0)}</td>
                <td className="mom-num" title={c.anosDeRenovacao === null ? undefined : `uma renovação a cada ${n(c.anosDeRenovacao, 1)} anos`}>
                  {c.anosDeRenovacao === null || c.anosDeRenovacao === 0 ? '—' : `${n(100 / c.anosDeRenovacao, 0)}%`}
                </td>
                <td className="mom-num" title={antes(`Frota em ${ano}`, c.parque, c.parqueAnoAnterior)}>
                  {c.parque === null ? '—' : n(c.parque, 0)}
                </td>
                <td className="mom-num" title={antes(`Demanda em ${ano}`, c.demandaEstrutural, c.demandaAnoAnterior, 1)}>
                  {c.demandaEstrutural === null ? '—' : n(c.demandaEstrutural)}
                </td>
                <td className="mom-num dem-destaque">{!comAjustada || c.demandaAjustada === null ? '—' : n(c.demandaAjustada)}</td>
                <td className="mom-num">
                  {c.variacaoPercentual === null ? (
                    '—'
                  ) : (
                    <span className={`diag-seta ${c.variacaoPercentual > 0 ? 'sobe' : c.variacaoPercentual < 0 ? 'desce' : ''}`}>
                      {variacaoPercentual(c.variacaoPercentual)}
                    </span>
                  )}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
