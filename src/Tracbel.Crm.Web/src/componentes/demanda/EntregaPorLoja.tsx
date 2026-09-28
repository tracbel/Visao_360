/**
 * A ENTREGA POR LOJA × MÊS — o mapa de calor do protótipo (issue 258), como tabela.
 *
 * TABELA, E NÃO IMAGEM: cada célula tem o número escrito e a intensidade de fundo só reforça — o leitor de tela lê as
 * linhas e as colunas, e a cor não carrega sozinha nenhuma informação. A última coluna é o total do ano, com a barra
 * que o protótipo punha ao lado.
 */

import type { EntregaDaLoja } from '../../tipos/mercado';
import { n, NOME_DO_MES } from './demanda';

export function EntregaPorLoja({ lojas, meses, mesEscolhido }: { lojas: EntregaDaLoja[]; meses: number[]; mesEscolhido: number | null }) {
  const maiorCelula = Math.max(0, ...lojas.flatMap((l) => l.porMes.map((v) => v ?? 0)));
  const maiorTotal = Math.max(0, ...lojas.map((l) => l.aEntregarNoAno ?? 0));

  if (lojas.length === 0) return <p className="diag-nota">Nenhuma loja com demanda no recorte.</p>;

  return (
    <div className="cad-tabela-wrap">
      <table className="cad-tabela dem-calor">
        <caption className="cad-so-leitor">Máquinas a entregar por loja em cada mês do ano fiscal</caption>
        <thead>
          <tr>
            <th scope="col">Loja</th>
            {meses.map((m) => (
              <th key={m} scope="col" data-escolhido={mesEscolhido === m ? 'true' : undefined}>
                {NOME_DO_MES[m]}
              </th>
            ))}
            <th scope="col">No ano</th>
          </tr>
        </thead>
        <tbody>
          {lojas.map((l) => (
            <tr key={l.lojaCodigo ?? l.loja}>
              <th scope="row">{l.loja}</th>
              {l.porMes.map((v, i) => {
                const intensidade = maiorCelula > 0 && v !== null ? v / maiorCelula : 0;
                return (
                  <td
                    key={meses[i]}
                    className="dem-calor-celula"
                    data-escolhido={mesEscolhido === meses[i] ? 'true' : undefined}
                    style={{ background: `rgba(47, 107, 51, ${(0.08 + intensidade * 0.62).toFixed(2)})`, color: intensidade > 0.55 ? '#fff' : undefined }}
                  >
                    {v === null ? '—' : n(v)}
                  </td>
                );
              })}
              <td className="dem-calor-total">
                <span className="dem-calor-barra" aria-hidden="true">
                  <span style={{ width: `${maiorTotal > 0 && l.aEntregarNoAno !== null ? (100 * l.aEntregarNoAno) / maiorTotal : 0}%` }} />
                </span>
                <strong>{l.aEntregarNoAno === null ? '—' : n(l.aEntregarNoAno)}</strong>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
