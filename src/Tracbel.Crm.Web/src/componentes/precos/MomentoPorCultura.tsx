/**
 * O MOMENTO ECONÔMICO POR CULTURA (issue 260) — cada cultura nos quatro horizontes, com a seta do sentido, ordenada pelo
 * horizonte escolhido (como o protótipo). A coluna escolhida vem em destaque, e a situação é a faixa dela.
 *
 * O R12 QUE VEM DA PAM É DITO NA CÉLULA: enquanto a série mensal da CONAB não fecha 24 meses, o ciclo é o preço anual da PAM
 * (o último ano contra o anterior), e quem lê precisa saber que é outra régua.
 */

import { ValorAusente } from '../comum/ValorAusente';
import type { PrecoDaCultura } from '../../tipos/mercado';
import { HORIZONTES, horizonteDe, motivoDoHorizonte, NOME_DA_FAIXA, sentido, TOM_DA_FAIXA, variacao, type Horizonte } from './precos';

export function MomentoPorCultura({
  culturas,
  horizonte,
  aoEscolher,
}: {
  culturas: readonly PrecoDaCultura[];
  horizonte: Horizonte;
  aoEscolher: (codigo: string) => void;
}) {
  const ordenadas = [...culturas].sort(
    (a, b) => (horizonteDe(b, horizonte)?.indice.indice ?? -9) - (horizonteDe(a, horizonte)?.indice.indice ?? -9),
  );

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela diag-tabela prc-tabela">
        <caption className="cad-so-leitor">A variação do preço de cada cultura em cada horizonte</caption>
        <thead>
          <tr>
            <th scope="col">Cultura</th>
            {HORIZONTES.map((h) => (
              <th key={h.id} scope="col" className="mom-num" data-escolhido={h.id === horizonte ? 'true' : undefined}>
                {h.rotulo}
                {h.id === 12 && ' ★'}
              </th>
            ))}
            <th scope="col">Situação</th>
          </tr>
        </thead>
        <tbody>
          {ordenadas.map((c) => {
            const escolhido = horizonteDe(c, horizonte)?.indice;
            return (
              <tr key={c.codigo}>
                <th scope="row">
                  <button type="button" className="prc-cultura" onClick={() => aoEscolher(c.codigo)}>
                    {c.nome}
                  </button>
                </th>
                {HORIZONTES.map((h) => {
                  const indice = horizonteDe(c, h.id)?.indice;
                  const s = sentido(indice?.indice);
                  return (
                    <td key={h.id} className="mom-num" data-escolhido={h.id === horizonte ? 'true' : undefined}>
                      {indice?.indice === null || indice === undefined ? (
                        <ValorAusente motivo={motivoDoHorizonte(h.id, indice?.motivo ?? 'SemFonte')} oQue={`a variação de ${h.rotulo}`} />
                      ) : (
                        <span className={`diag-seta ${s.classe}`}>
                          {variacao(indice.indice)} <span aria-hidden="true">{s.seta}</span>
                        </span>
                      )}
                      {h.id === 12 && indice?.serie === 'AnualPam' && <span className="prc-selo" title={`Preço anual da PAM: ${indice.anoRecente ?? 'último ano'} contra o anterior`}>PAM</span>}
                    </td>
                  );
                })}
                <td className="prc-situacao" data-tom={escolhido?.faixa ? TOM_DA_FAIXA[escolhido.faixa] : 'sem'}>
                  {escolhido?.faixa ? NOME_DA_FAIXA[escolhido.faixa] : '—'}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
