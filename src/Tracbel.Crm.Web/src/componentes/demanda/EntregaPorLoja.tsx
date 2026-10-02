/**
 * A ENTREGA POR LOJA × MÊS — o mapa de calor do protótipo (issue 258), como tabela; no desenho da maquete de 02/10/2026,
 * com as cinco lojas que mais têm a entregar à vista e "Ver todas as lojas" para o resto.
 *
 * TABELA, E NÃO IMAGEM: cada célula tem o número escrito e a intensidade de fundo só reforça — o leitor de tela lê as
 * linhas e as colunas, e a cor não carrega sozinha nenhuma informação. A última coluna é o total do ano.
 */

import { ArrowRight } from 'lucide-react';
import { useState } from 'react';
import type { EntregaDaLoja } from '../../tipos/mercado';
import { NomeDaLoja } from '../mercado/NomeDaLoja';
import { n, NOME_DO_MES } from './demanda';

/** Quantas lojas a tabela mostra antes de "Ver todas as lojas". */
export const LOJAS_A_VISTA = 5;

/** Uma casa decimal só abaixo de 10: o mês de uma loja pequena é fração de máquina, e arredondar a zero mentiria. */
const numero = (v: number) => n(v, v < 10 ? 1 : 0);

export function EntregaPorLoja({ lojas, meses, mesEscolhido }: { lojas: EntregaDaLoja[]; meses: number[]; mesEscolhido: number | null }) {
  const [todas, setTodas] = useState(false);
  const maiorCelula = Math.max(0, ...lojas.flatMap((l) => l.porMes.map((v) => v ?? 0)));

  if (lojas.length === 0) return <p className="diag-nota">Nenhuma loja com demanda no recorte.</p>;

  const visiveis = todas ? lojas : lojas.slice(0, LOJAS_A_VISTA);

  return (
    <>
      <div className="cad-tabela-wrap dem-calor-rolagem">
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
              <th scope="col" className="dem-calor-total">
                Total
              </th>
            </tr>
          </thead>
          <tbody>
            {visiveis.map((l) => (
              <tr key={l.lojaCodigo ?? l.loja}>
                <th scope="row" title={l.loja}>
                  <NomeDaLoja nome={l.loja} />
                </th>
                {l.porMes.map((v, i) => {
                  const intensidade = maiorCelula > 0 && v !== null ? v / maiorCelula : 0;
                  return (
                    <td
                      key={meses[i]}
                      className="dem-calor-celula"
                      data-escolhido={mesEscolhido === meses[i] ? 'true' : undefined}
                      title={v === null ? undefined : `${n(v, 2)} máquinas`}
                      style={{ background: `rgba(30, 123, 52, ${(0.12 + intensidade * 0.78).toFixed(2)})`, color: intensidade > 0.5 ? '#fff' : undefined }}
                    >
                      {v === null ? '—' : numero(v)}
                    </td>
                  );
                })}
                <td className="dem-calor-total">
                  <strong>{l.aEntregarNoAno === null ? '—' : numero(l.aEntregarNoAno)}</strong>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {/* O BOTÃO ESTÁ SEMPRE, como na maquete; com até cinco lojas, todas já estão na tabela, e a dica diz isso. */}
      <div className="dem-ver-todas">
        <button
          type="button"
          className="dem-botao-link"
          onClick={() => lojas.length > LOJAS_A_VISTA && setTodas((t) => !t)}
          aria-expanded={lojas.length > LOJAS_A_VISTA ? todas : undefined}
          aria-disabled={lojas.length <= LOJAS_A_VISTA || undefined}
          title={lojas.length > LOJAS_A_VISTA ? `${lojas.length} lojas com demanda no recorte` : `As ${lojas.length} lojas do recorte já estão na tabela`}
        >
          {todas ? `Ver só as ${LOJAS_A_VISTA} maiores` : 'Ver todas as lojas'}
          <ArrowRight size={14} strokeWidth={2.2} aria-hidden="true" />
        </button>
      </div>
    </>
  );
}
