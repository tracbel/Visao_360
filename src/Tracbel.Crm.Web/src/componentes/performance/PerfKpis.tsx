/**
 * Grade de KPIs — porte de `renderPerfKpis`
 * (prototipo/referencia/assets/app.js linha 6236/perf-block.js linha 314).
 */
import type { PerfDetalheCen, PerfResumo } from '../../tipos/performanceCen';
import { fmtBRLcompact } from './formatoPerformance';

function metaFillClasse(progresso: number): string {
  if (progresso >= 1) return 'atingido';
  if (progresso >= 0.85) return 'quase';
  if (progresso >= 0.7) return 'aviso';
  return 'baixo';
}

function Kpi({
  label,
  valor,
  hint,
  metaProgresso,
}: {
  label: string;
  valor: string;
  hint: string;
  metaProgresso?: number;
}) {
  return (
    <div className="perf-kpi">
      <div className="pk-label">{label}</div>
      <div className="pk-value">{valor}</div>
      <div className="pk-hint">{hint}</div>
      {metaProgresso !== undefined && (
        <>
          <div className="pk-meta-bar">
            <div
              className={`pk-meta-fill ${metaFillClasse(metaProgresso)}`}
              style={{ width: `${Math.min(100, metaProgresso * 100)}%` }}
            />
          </div>
          <div className="pk-meta-txt">{(metaProgresso * 100).toFixed(0)}% da meta</div>
        </>
      )}
    </div>
  );
}

export function PerfKpis({
  resumo,
  isCEN,
  detalheCen,
  qtdCens,
}: {
  resumo: PerfResumo;
  isCEN: boolean;
  detalheCen?: PerfDetalheCen;
  qtdCens: number;
}) {
  return (
    <div className="perf-kpi-grid">
      <Kpi
        label="Vendas mês (Ago/26)"
        valor={fmtBRLcompact(resumo.mes_vendas)}
        hint={isCEN ? 'Parcial · 17 dias úteis' : `${qtdCens} CENs · média ${fmtBRLcompact(resumo.mes_vendas / qtdCens)}`}
        metaProgresso={resumo.mes_atingimento}
      />
      <Kpi
        label="Vendas FYTD"
        valor={fmtBRLcompact(resumo.fytd_vendas)}
        hint={isCEN ? '10 meses (nov/25 → ago/26)' : `Meta ${fmtBRLcompact(resumo.fytd_meta)}`}
        metaProgresso={resumo.fytd_atingimento}
      />
      <Kpi label="Pipeline aberto" valor={fmtBRLcompact(resumo.pipeline_aberto)} hint="Oport. em andamento no funil" />
      <Kpi
        label="Conversão FYTD"
        valor={`${(resumo.conversao_fytd * 100).toFixed(1)}%`}
        hint={isCEN ? `${detalheCen?.fytd_ganhas ?? 0} ganhas de ${detalheCen?.fytd_totais ?? 0}` : 'Média ponderada'}
      />
      <Kpi
        label="Cobertura de carteira"
        valor={`${Math.round(resumo.cobertura)}%`}
        hint={isCEN ? 'Frequência conforme classe' : `Média ${qtdCens} CENs`}
      />
    </div>
  );
}
