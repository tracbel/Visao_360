/**
 * "Comparar com período anterior" + interruptor + "12 meses" — o controle à
 * direita do título da carteira (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * POR QUE NÃO É O `ComparacaoComPeriodoAnterior` DE MERCADO. Aquele traz o Δ%
 * e a dica "Como ler os números de Mercado"; o da maquete de Território traz o
 * seletor de período e nada de Δ%. O interruptor é o mesmo desenho (as mesmas
 * classes), desligado pelo mesmo motivo — a leitura devolve uma janela só.
 *
 * O "12 MESES" NÃO MUDA O PERÍODO AQUI, e é de propósito: o período é filtro do
 * RECORTE, e mora no alto da página, valendo para as duas abas. Um segundo
 * seletor que mudasse só esta aba faria Mercado e Território falarem de janelas
 * diferentes com a mesma cara. Ele MOSTRA a janela em vigor, desligado, e a dica
 * diz onde ela muda.
 */

import { InfoTooltip } from '../../InfoTooltip';
import type { PeriodoDaLeitura } from './periodo';

export function ComparacaoDaCarteira({ periodo }: { periodo: PeriodoDaLeitura | null }) {
  return (
    <div className="dash-secao-controle terr-cart-comparar">
      <span className="dash-comparar">
        Comparar com período anterior
        {/* `role="switch"` e `aria-checked` de verdade: o leitor de tela anuncia
            "interruptor, desligado, indisponível", que é o estado. */}
        <button
          type="button"
          className="dash-interruptor"
          role="switch"
          aria-checked={false}
          disabled
          aria-label="Comparar com o período anterior"
        />
      </span>
      <InfoTooltip
        rotulo="Por que a comparação e o período não mudam aqui"
        texto={
          <>
            <p>
              A leitura desta tela devolve UMA janela de competência, e não duas: não há período anterior para
              comparar. Ligar a comparação depende de a leitura passar a devolver a janela anterior junto (issue 69).
            </p>
            <p>
              {periodo ? (
                <>
                  O período em vigor é <strong>{periodo.rotulo}</strong> ({periodo.intervalo}).{' '}
                </>
              ) : null}
              Ele muda no filtro Período, no alto da página, e vale para as duas abas.
            </p>
          </>
        }
      />
      {/* `defaultValue`, e não `value`: o campo é desligado e não tem `onChange`. */}
      <select disabled aria-label="Período da carteira" defaultValue="periodo">
        <option value="periodo">{periodo?.rotulo ?? 'Período'}</option>
      </select>
    </div>
  );
}
