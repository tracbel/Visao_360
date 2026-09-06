/**
 * Barra de contexto da persona ativa — porte de `renderPerfContext`
 * (prototipo/referencia/assets/app.js linha 6201/perf-block.js linha 279).
 * Os 3 blocos são texto fixo no original (não vêm de `PERF_CENS`), então
 * seguem fixos aqui também.
 */
import type { PersonaPerf } from '../../tipos/performanceCen';

export function PerfContexto({ persona }: { persona: PersonaPerf }) {
  return (
    <div className={`perf-context-bar${persona === 'nacional' ? ' warn' : ''}`}>
      {persona === 'cen' && (
        <>
          <span className="perf-context-icon" style={{ background: '#367C2B' }}>
            JR
          </span>
          <div>
            <div>
              <strong>João Ribeiro</strong> · CEN · Regional MT Norte
            </div>
            <div className="perf-context-sub">
              Admissão 03/2019 · Meta agosto R$ 4.200.000 · Meta FYTD R$ 42.000.000
            </div>
          </div>
        </>
      )}
      {persona === 'regional' && (
        <>
          <span className="perf-context-icon" style={{ background: '#F59E0B' }}>
            CB
          </span>
          <div>
            <div>
              <strong>Cláudia Batista</strong> · Gerente Regional MT Norte · 5 CENs
            </div>
            <div className="perf-context-sub">João Ribeiro · José Rufino · Ana Paula · Ricardo · Fernanda</div>
          </div>
        </>
      )}
      {persona === 'nacional' && (
        <>
          <span className="perf-context-icon" style={{ background: '#EF4444' }}>
            RM
          </span>
          <div>
            <div>
              <strong>Rafael Menezes</strong> · Diretor Comercial · 4 regionais · 8 CENs
            </div>
            <div className="perf-context-sub">
              MT Norte (5) · MT Sul (1) · GO (1) · BA (1) — cobertura Brasil Central
            </div>
          </div>
        </>
      )}
    </div>
  );
}
