/**
 * As células que as tabelas dos Financiamentos (issue 261) repetem: a medida com o ano anterior embaixo e a variação, e a
 * situação com o nome da faixa — a cor só reforça o que está escrito.
 */

import type { IndiceDeCredito, JanelasDeCredito } from '../../tipos/mercado';
import { razao, sentidoDaRazao, situacaoDe, TOM_DA_FAIXA, variacaoDaRazao } from './financiamentos';

export function CelulaComAnterior({
  atual,
  anterior,
  formatar,
}: {
  atual: number | null;
  anterior: number | null;
  formatar: (v: number) => string;
}) {
  const r = atual !== null ? razao(atual, anterior) : null;
  return (
    <td className="mom-num fin-medida">
      <span className="fin-medida-atual">{atual === null ? '—' : formatar(atual)}</span>
      <span className="fin-medida-anterior">
        ano ant.: {anterior === null ? '—' : formatar(anterior)}
        {r !== null && (
          <>
            {' · '}
            <span className={`diag-seta ${sentidoDaRazao(r)}`}>{variacaoDaRazao(r)}</span>
          </>
        )}
      </span>
    </td>
  );
}

export function CelulaDaSituacao({ indice, janelas }: { indice: IndiceDeCredito | null; janelas: JanelasDeCredito }) {
  const texto = situacaoDe(indice, janelas);
  return (
    <td className="fin-situacao" data-tom={indice?.faixa ? TOM_DA_FAIXA[indice.faixa] : 'sem'}>
      {texto}
      {indice?.basePequena && <span className="fin-selo" title="Abaixo do mínimo de linhas: leia a variação com cuidado.">base pequena</span>}
    </td>
  );
}
