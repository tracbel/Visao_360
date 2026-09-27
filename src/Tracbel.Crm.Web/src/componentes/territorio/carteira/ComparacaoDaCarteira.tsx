/**
 * "Comparar com período anterior" + interruptor + o período — o controle à
 * direita do título da carteira (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * POR QUE NÃO É O `ComparacaoComPeriodoAnterior` DE MERCADO. Aquele traz o Δ%
 * e a dica "Como ler os números de Mercado"; o da maquete de Território traz o
 * seletor de período e nada de Δ%. O interruptor é o MESMO — o estado mora no
 * contexto da comparação, e ligar aqui liga lá (27/09/2026: ele passou a ligar
 * de verdade, quando a leitura começou a trazer o mesmo trecho do ano anterior).
 *
 * O PERÍODO NÃO MUDA AQUI, e é de propósito: o período é filtro do RECORTE, e
 * mora no alto da página, valendo para as duas abas. Um segundo seletor que
 * mudasse só esta aba faria Mercado e Território falarem de janelas diferentes
 * com a mesma cara. Ele MOSTRA a janela em vigor, desligado, e a dica diz onde
 * ela muda.
 */

import { InfoTooltip } from '../../InfoTooltip';
import { janelaAnteriorPorExtenso, useComparacao } from '../contextoDaComparacao';
import type { PeriodoDaLeitura } from './periodo';

export function ComparacaoDaCarteira({ periodo }: { periodo: PeriodoDaLeitura | null }) {
  const { ligada, aoLigar, periodoAnterior } = useComparacao();
  const janelaAnterior = janelaAnteriorPorExtenso(periodoAnterior);

  return (
    <div className="dash-secao-controle terr-cart-comparar">
      <span className="dash-comparar">
        Comparar com período anterior
        {/* `role="switch"` e `aria-checked` de verdade: o leitor de tela anuncia
            "interruptor, ligado", que é o estado. */}
        <button
          type="button"
          className="dash-interruptor"
          role="switch"
          aria-checked={ligada}
          onClick={() => aoLigar(!ligada)}
          aria-label="Comparar com o período anterior"
        />
      </span>
      <InfoTooltip
        rotulo="Com o que a comparação compara, e onde o período muda"
        texto={
          <>
            <p>
              A comparação é com o <strong>mesmo trecho do ano anterior</strong>
              {janelaAnterior ? ` (${janelaAnterior})` : ''} — cada mês contra ele mesmo, doze meses para trás. Ela vale
              para as vendas; municípios, cobertura e parque não são medidas do período, e cada cartão diz por quê.
            </p>
            <p>
              {periodo ? (
                <>
                  O período em vigor é <strong>{periodo.descricao}</strong> ({periodo.intervalo}).{' '}
                </>
              ) : null}
              Ele muda no filtro Período, no alto da página, e vale para as duas abas.
            </p>
          </>
        }
      />
      {/* `defaultValue`, e não `value`: o campo é desligado e não tem `onChange`. */}
      <select disabled aria-label="Período da carteira" defaultValue="periodo" key={periodo?.rotulo ?? 'periodo'}>
        <option value="periodo">{periodo?.rotulo ?? 'Período'}</option>
      </select>
    </div>
  );
}
