/**
 * COLUNAS AGRUPADAS EM HTML — "Demanda estrutural x Demanda ajustada".
 *
 * POR QUE NÃO O CHART.JS: são duas colunas por cultura e um rótulo em cima de
 * cada uma. Em HTML a altura é declarada, o texto é texto (o leitor de tela e a
 * busca da página o acham), não há canvas a medir nem fonte a esperar — e um
 * desenho que não se mede não tem como crescer sozinho.
 *
 * A ESCALA COMEÇA NO ZERO, sempre: coluna que não começa no zero exagera a
 * diferença entre as duas demandas, que é exatamente o que o painel compara.
 */

import { numero } from './formatos';

/** Um passo "redondo" para o eixo: 1, 2, 2,5 ou 5 vezes uma potência de 10. */
function passoRedondo(bruto: number): number {
  if (bruto <= 0) return 1;
  const potencia = 10 ** Math.floor(Math.log10(bruto));
  const fracao = bruto / potencia;
  const degrau = fracao <= 1 ? 1 : fracao <= 2 ? 2 : fracao <= 2.5 ? 2.5 : fracao <= 5 ? 5 : 10;
  return degrau * potencia;
}

function marcasDoEixo(maior: number, quantas = 5): number[] {
  const passo = passoRedondo(maior / (quantas - 1));
  const topo = Math.max(passo, Math.ceil(maior / passo) * passo);
  const marcas: number[] = [];
  for (let v = 0; v <= topo + passo / 2; v += passo) marcas.push(v);
  return marcas;
}

export function ColunasAgrupadas({
  grupos,
  series,
  altura,
  formatar = (v) => numero(v),
  descricao,
}: {
  grupos: readonly { rotulo: string; valores: readonly (number | null)[] }[];
  series: readonly { nome: string; cor: string }[];
  /** A altura da área das colunas, em pixels — fixa. */
  altura: number;
  formatar?: (valor: number) => string;
  /** O que o leitor de tela ouve no lugar do desenho. */
  descricao: string;
}) {
  const maior = Math.max(0, ...grupos.flatMap((g) => g.valores.filter((v): v is number => v !== null)));
  const marcas = marcasDoEixo(maior || 1);
  const topo = marcas[marcas.length - 1];

  return (
    <div className="mom-colunas" role="img" aria-label={descricao} style={{ gridTemplateRows: `${altura}px auto` }}>
      <div className="mom-colunas-eixo" aria-hidden="true">
        {marcas.map((m) => (
          <span key={m} style={{ bottom: `${(m / topo) * 100}%` }}>
            {formatar(m)}
          </span>
        ))}
      </div>
      <div className="mom-colunas-area" aria-hidden="true">
        <div className="mom-colunas-grade">
          {marcas.slice(1).map((m) => (
            <span key={m} style={{ bottom: `${(m / topo) * 100}%` }} />
          ))}
        </div>
        {grupos.map((g) => (
          <div key={g.rotulo} className="mom-colunas-grupo">
            {g.valores.map((v, i) => (
              <div
                key={series[i]?.nome ?? i}
                className="mom-colunas-barra"
                style={{ height: `${v === null ? 0 : (v / topo) * 100}%`, background: series[i]?.cor }}
              >
                <span>{v === null ? '—' : formatar(v)}</span>
              </div>
            ))}
          </div>
        ))}
      </div>
      <div className="mom-colunas-rotulos" aria-hidden="true">
        {grupos.map((g) => (
          <span key={g.rotulo}>{g.rotulo}</span>
        ))}
      </div>
    </div>
  );
}
