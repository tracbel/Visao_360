/**
 * Lista de insights automáticos — porte de `renderPerfInsights`
 * (prototipo/referencia/assets/app.js linha 6462 / perf-block.js linha 540).
 * `texto` é HTML confiável (só interpola nomes de CEN e números calculados
 * localmente, nunca entrada de usuário), por isso `dangerouslySetInnerHTML`
 * em vez de recompor o `<strong>` em JSX.
 */
import type { PerfInsight } from '../../dados/performanceCen';

export function PerfInsights({ insights }: { insights: PerfInsight[] }) {
  return (
    <div id="perfInsights" className="perf-insights-list">
      {insights.length === 0 ? (
        <div className="perf-insight-vazio">Nenhum insight relevante para o recorte atual.</div>
      ) : (
        insights.map((insight, i) => (
          <div className={`perf-insight ${insight.tipo}`} key={i}>
            <span className="pi-icon">{insight.icon}</span>
            <div className="pi-texto" dangerouslySetInnerHTML={{ __html: insight.texto }} />
          </div>
        ))
      )}
    </div>
  );
}
