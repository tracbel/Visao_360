/**
 * Aba "Pós-vendas" da Ficha do Cliente — porte de `renderPosVendasTab()`
 * (linha 8333) + `mountPosVendasCharts()` (linha 8657) do protótipo de
 * referência (`prototipo/referencia/assets/app.js`), usando os dados de
 * `POSVENDAS_CLIENTE_84391` (linha 8095, hoje em `pos-vendas-cliente-84391.json`).
 *
 * Os dois `<canvas>` do protótipo (montados sob demanda com Chart.js puro)
 * viram aqui `<Bar>`/`<Doughnut>` do react-chartjs-2, com os mesmos `data`/`options`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — três botões sem efeito tratados (documento 06): "Ver todas as
 * OS" e "Histórico de PMPs concluídas" saíram, por não existirem as telas que
 * prometiam; "Criar proposta JDCP" virou o caminho que existe de verdade, a
 * tela de Nova Oportunidade.
 */
import {
  ArcElement,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Legend,
  LinearScale,
  Tooltip,
  type ChartData,
  type ChartOptions,
} from 'chart.js';
import { Link } from 'react-router-dom';
import { Bar, Doughnut } from 'react-chartjs-2';
import type { PosVendasCliente } from '../tipos/clientes';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Tooltip, Legend);

function fmtBRL(v: number): string {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}

function fmtBRLcompact(v: number): string {
  if (v >= 1_000_000) return `R$ ${(v / 1_000_000).toFixed(v >= 10_000_000 ? 1 : 2)}M`;
  if (v >= 1_000) return `R$ ${(v / 1_000).toFixed(0)}k`;
  return `R$ ${v}`;
}

function fmtData(iso: string): string {
  return new Date(iso).toLocaleDateString('pt-BR');
}

const ICONE_ALERTA: Record<string, React.ReactNode> = {
  red: (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.5}>
      <circle cx="12" cy="12" r="10" />
      <line x1="12" y1="8" x2="12" y2="12" />
      <line x1="12" y1="16" x2="12.01" y2="16" />
    </svg>
  ),
  amber: (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
      <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
      <line x1="12" y1="9" x2="12" y2="13" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </svg>
  ),
};

const ICONE_ALERTA_PADRAO = (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
    <circle cx="12" cy="12" r="10" />
    <line x1="12" y1="16" x2="12" y2="12" />
    <line x1="12" y1="8" x2="12.01" y2="8" />
  </svg>
);

type Props = { dados: PosVendasCliente };

export function AbaPosVendas({ dados: p }: Props) {
  const criticos = p.alertas_criticos.filter((a) => a.cor === 'red').length;
  const atrasadas = p.os_abertas.filter((o) => o.status_cor === 'red').length;

  const dataBarra: ChartData<'bar'> = {
    labels: p.fat_12m.map((x) => x.mes),
    datasets: [
      { label: 'Peças', data: p.fat_12m.map((x) => x.pecas), backgroundColor: '#367C2B', borderRadius: 4 },
      { label: 'Serviços', data: p.fat_12m.map((x) => x.servicos), backgroundColor: '#FFDE00', borderRadius: 4 },
    ],
  };
  const opcoesBarra: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: { callbacks: { label: (ctx) => `${ctx.dataset.label}: R$ ${ctx.parsed.y}k` } },
    },
    scales: {
      x: { stacked: true, grid: { display: false }, ticks: { font: { size: 11 } } },
      y: {
        stacked: true,
        beginAtZero: true,
        grid: { color: 'rgba(0,0,0,0.05)' },
        ticks: { font: { size: 11 }, callback: (v) => `R$ ${v}k` },
      },
    },
  };

  const coresMixCanal = ['#367C2B', '#7CB342', '#C5E1A5'];
  const dataDonut: ChartData<'doughnut'> = {
    labels: ['Balcão', 'Oficina interna', 'Campo (técnico)'],
    datasets: [
      {
        data: [p.fat_pv.mix_canal.balcao, p.fat_pv.mix_canal.oficina_interna, p.fat_pv.mix_canal.campo_tecnico],
        backgroundColor: coresMixCanal,
        borderWidth: 0,
      },
    ],
  };
  const opcoesDonut: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,
    cutout: '62%',
    plugins: {
      legend: {
        position: 'right',
        labels: {
          font: { size: 12 },
          padding: 12,
          generateLabels: (chart) => {
            const { data } = chart;
            const valores = (data.datasets[0]?.data ?? []) as number[];
            return data.labels!.map((label, i) => ({
              text: `${label}  ${valores[i]}%`,
              fillStyle: coresMixCanal[i],
              strokeStyle: coresMixCanal[i],
              hidden: false,
              index: i,
            }));
          },
        },
      },
      tooltip: { callbacks: { label: (ctx) => `${ctx.label}: ${ctx.parsed}%` } },
    },
  };

  return (
    <>
      {/* ROW 1: KPIs pós-vendas */}
      <div className="pv-kpi-row">
        <div className="pv-kpi pv-kpi-primary">
          <div className="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
            </svg>
            <span>Faturamento pós-vendas FYTD</span>
          </div>
          <div className="pv-kpi-valor">{fmtBRL(p.fat_pv.fytd_total)}</div>
          <div className="pv-kpi-hint">
            <span className="pv-kpi-split">
              <b>{fmtBRLcompact(p.fat_pv.fytd_pecas)}</b> peças
            </span>
            <span className="pv-kpi-sep">·</span>
            <span className="pv-kpi-split">
              <b>{fmtBRLcompact(p.fat_pv.fytd_servicos)}</b> serviços
            </span>
          </div>
          <div className="pv-kpi-sub">
            2025: {fmtBRL(p.fat_pv.ano_2025_total)} · 2024: {fmtBRL(p.fat_pv.ano_2024_total)}
          </div>
        </div>

        <div className="pv-kpi">
          <div className="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
              <polyline points="14 2 14 8 20 8" />
            </svg>
            <span>Ordens de serviço abertas</span>
          </div>
          <div className="pv-kpi-valor">{p.os_abertas.length}</div>
          <div className="pv-kpi-hint">
            {atrasadas > 0 ? (
              <span className="pv-badge pv-badge-red">
                {atrasadas} atrasada{atrasadas > 1 ? 's' : ''}
              </span>
            ) : (
              <span className="pv-badge pv-badge-green">todas no prazo</span>
            )}
            <span className="pv-kpi-sep">·</span>
            <span>{fmtBRLcompact(p.os_abertas.reduce((s, o) => s + o.valor, 0))} em execução</span>
          </div>
        </div>

        <div className="pv-kpi">
          <div className="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
            </svg>
            <span>PMPs pendentes</span>
          </div>
          <div className="pv-kpi-valor">{p.pmp_pendentes.length}</div>
          <div className="pv-kpi-hint">
            {p.pmp_pendentes.filter((x) => x.criticidade === 'Crítica').length > 0 && (
              <span className="pv-badge pv-badge-red">
                {p.pmp_pendentes.filter((x) => x.criticidade === 'Crítica').length} crítica
              </span>
            )}
            <span>{p.pmp_pendentes.reduce((s, x) => s + x.qtd_equipamentos, 0)} equipamentos afetados</span>
          </div>
        </div>

        <div className={`pv-kpi ${criticos > 0 ? 'pv-kpi-alert' : ''}`}>
          <div className="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
              <line x1="12" y1="9" x2="12" y2="13" />
              <line x1="12" y1="17" x2="12.01" y2="17" />
            </svg>
            <span>Alertas críticos</span>
          </div>
          <div className="pv-kpi-valor">{p.alertas_criticos.length}</div>
          <div className="pv-kpi-hint">
            <span className="pv-badge pv-badge-red">{criticos} críticos</span>
            <span className="pv-kpi-sep">·</span>
            <span>{p.alertas_criticos.length - criticos} atenção</span>
          </div>
        </div>

        <div className="pv-kpi">
          <div className="pv-kpi-header">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
              <circle cx="9" cy="7" r="4" />
              <path d="M23 11h-6M20 8v6" />
            </svg>
            <span>NPS pós-serviço · 12m</span>
          </div>
          <div
            className="pv-kpi-valor"
            style={{ color: p.nps.score >= 70 ? 'var(--jd-green)' : p.nps.score >= 50 ? '#F59E0B' : '#DC2626' }}
          >
            {p.nps.score}
          </div>
          <div className="pv-kpi-hint">
            {p.nps.respostas} respostas · {p.nps.promotores} promotores
          </div>
        </div>
      </div>

      {/* ROW 2: Gráfico 12m + Mix canal */}
      <div className="pv-row-2">
        <div className="card pv-card-grafico">
          <div className="card-header">
            <div>
              <div className="card-title">Faturamento pós-vendas — 12 meses</div>
              <div className="card-subtitle">Peças vs Serviços · R$ mil</div>
            </div>
            <div className="pv-legend-inline">
              <span className="pv-legend-item">
                <span className="pv-dot" style={{ background: '#367C2B' }} />
                Peças
              </span>
              <span className="pv-legend-item">
                <span className="pv-dot" style={{ background: '#FFDE00' }} />
                Serviços
              </span>
            </div>
          </div>
          <div style={{ padding: '16px 20px 20px' }}>
            <div style={{ height: 220, maxHeight: 220 }}>
              <Bar data={dataBarra} options={opcoesBarra} />
            </div>
          </div>
        </div>
        <div className="card pv-card-mix">
          <div className="card-header">
            <div>
              <div className="card-title">Canal de faturamento</div>
              <div className="card-subtitle">Peças FYTD por origem</div>
            </div>
          </div>
          <div style={{ padding: '16px 20px 20px' }}>
            <div style={{ height: 220, maxHeight: 220 }}>
              <Doughnut data={dataDonut} options={opcoesDonut} />
            </div>
            <div className="pv-ticket-medio">
              <span className="pv-ticket-label">Ticket médio de peças</span>
              <span className="pv-ticket-valor">{fmtBRL(p.fat_pv.ticket_medio_pecas)}</span>
            </div>
          </div>
        </div>
      </div>

      {/* ROW 3: Alertas críticos */}
      <div className="card">
        <div className="card-header">
          <div>
            <div className="card-title">Alertas de pós-vendas</div>
            <div className="card-subtitle">Telemetria JDLink · garantia · ciclo de relacionamento · boletins JD</div>
          </div>
          <span className="pv-badge pv-badge-neutral">{p.alertas_criticos.length} ativos</span>
        </div>
        <div className="pv-alertas-lista">
          {p.alertas_criticos.map((a) => (
            <div key={a.id} className={`pv-alerta pv-alerta-${a.cor}`}>
              <div className="pv-alerta-icon">{ICONE_ALERTA[a.cor] ?? ICONE_ALERTA_PADRAO}</div>
              <div className="pv-alerta-corpo">
                <div className="pv-alerta-titulo">{a.titulo}</div>
                <div className="pv-alerta-detalhe">{a.detalhe}</div>
                <div className="pv-alerta-meta">
                  <span className="pv-alerta-origem">{a.origem}</span>
                  <span className="pv-kpi-sep">·</span>
                  <span>{fmtData(a.criado_em)}</span>
                </div>
              </div>
              {/* O botão do alerta saiu: respondia com `alert('Ação do
                  protótipo: …')`. Cada alerta traz o próprio texto de ação, que
                  continua legível — o que sumiu foi a promessa de executá-la. */}
            </div>
          ))}
        </div>
      </div>

      {/* ROW 4: OS abertas */}
      <div className="card" style={{ marginTop: 20 }}>
        <div className="card-header">
          <div>
            <div className="card-title">Ordens de serviço em aberto</div>
            <div className="card-subtitle">Módulo Oficina · Protheus · atualizado 08:45</div>
          </div>
          {/* "Ver todas as OS" saiu: não existe tela de ordens de serviço. A
              tabela abaixo é o que o protótipo tem, e o cabeçalho já diz de
              onde ela vem (documento 06). */}
        </div>
        <div style={{ overflowX: 'auto' }}>
          <table className="pv-table">
            <thead>
              <tr>
                <th>OS</th>
                <th>Equipamento</th>
                <th>Tipo</th>
                <th>Aberta</th>
                <th>Prazo</th>
                <th>Status</th>
                <th>Técnico</th>
                <th className="num">Valor</th>
              </tr>
            </thead>
            <tbody>
              {p.os_abertas.map((o) => (
                <tr key={o.id}>
                  <td>
                    <span className="pv-mono">{o.id}</span>
                  </td>
                  <td>
                    <div className="pv-eqp-info">
                      <span className="pv-eqp-modelo">{o.modelo}</span>
                      <span className="pv-eqp-chassi">{o.chassi.slice(-6)}</span>
                    </div>
                  </td>
                  <td>{o.tipo === 'Garantia' ? <span className="pv-badge pv-badge-blue">Garantia</span> : o.tipo}</td>
                  <td>
                    {fmtData(o.abertura)} <span className="pv-tenue">({o.dias_aberta}d)</span>
                  </td>
                  <td>{fmtData(o.previsao)}</td>
                  <td>
                    <span className={`pv-status pv-status-${o.status_cor}`}>{o.status}</span>
                  </td>
                  <td>{o.tecnico}</td>
                  <td className="num pv-mono">{o.valor > 0 ? fmtBRLcompact(o.valor) : <span className="pv-tenue">—</span>}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* ROW 5: PMP */}
      <div className="card" style={{ marginTop: 20 }}>
        <div className="card-header">
          <div>
            <div className="card-title">PMPs · Campanhas de campo John Deere</div>
            <div className="card-subtitle">
              Programa de Manutenção Preventiva + Boletins técnicos (DFF/PIP) · fonte: portal JD Dealer
            </div>
          </div>
          {/* "Histórico de PMPs concluídas" saiu: o arquivo traz só as
              pendentes; não há histórico para mostrar. */}
        </div>
        <div className="pv-pmp-lista">
          {p.pmp_pendentes.map((pmp) => (
            <div key={pmp.id} className="pv-pmp">
              <div className="pv-pmp-lado">
                <span className="pv-pmp-cod">{pmp.codigo_jd}</span>
                <span className={`pv-pmp-badge pv-pmp-crit-${pmp.criticidade_cor}`}>{pmp.criticidade}</span>
              </div>
              <div className="pv-pmp-corpo">
                <div className="pv-pmp-titulo">{pmp.titulo}</div>
                <div className="pv-pmp-desc">{pmp.descricao}</div>
                <div className="pv-pmp-meta">
                  <span>
                    <b>{pmp.qtd_equipamentos}</b> {pmp.qtd_equipamentos === 1 ? 'equipamento' : 'equipamentos'} · {pmp.modelo}
                  </span>
                  <span className="pv-kpi-sep">·</span>
                  <span>{pmp.cobertura}</span>
                  <span className="pv-kpi-sep">·</span>
                  <span>Duração: {pmp.duracao_estimada}</span>
                  <span className="pv-kpi-sep">·</span>
                  <span
                    className={`pv-pmp-prazo pv-pmp-prazo-${
                      pmp.dias_restantes < 15 ? 'red' : pmp.dias_restantes < 45 ? 'amber' : 'green'
                    }`}
                  >
                    Prazo {fmtData(pmp.prazo)} · <b>{pmp.dias_restantes}d restantes</b>
                  </span>
                </div>
              </div>
              <div className="pv-pmp-acao">
                <span className={`pv-status pv-status-${pmp.status_cor}`}>
                  {pmp.status}
                  {pmp.agenda ? ` · ${fmtData(pmp.agenda)}` : ''}
                </span>
                {/* "Agendar" leva à Agenda, que é onde se agenda de verdade
                    desde que ela ganhou o painel de nova tarefa. "Ver detalhes"
                    saiu: a PMP já agendada mostra a data ao lado, e não existe
                    tela de detalhe de PMP. */}
                {pmp.status !== 'Agendada' && (
                  <Link to="/agenda" className="btn btn-secondary btn-sm">
                    Agendar
                  </Link>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* ROW 6: Contratos JDCP + oportunidade */}
      <div className="pv-row-6">
        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Contratos JDCP ativos</div>
              <div className="card-subtitle">John Deere Complete Protection · cobertura estendida</div>
            </div>
            <span className="pv-badge pv-badge-green">
              {p.contratos_jdcp.length} de {p.contratos_jdcp.length + p.frota_sem_jdcp.length} equipamentos
            </span>
          </div>
          <table className="pv-table">
            <thead>
              <tr>
                <th>Equipamento</th>
                <th>Plano</th>
                <th>Vigência</th>
                <th className="num">Valor/ano</th>
              </tr>
            </thead>
            <tbody>
              {p.contratos_jdcp.map((c) => (
                <tr key={c.chassi}>
                  <td>
                    <div className="pv-eqp-info">
                      <span className="pv-eqp-modelo">{c.modelo}</span>
                      <span className="pv-eqp-chassi">{c.chassi.slice(-6)}</span>
                    </div>
                  </td>
                  <td>
                    <span className="pv-badge pv-badge-blue">{c.plano}</span>
                  </td>
                  <td>
                    {fmtData(c.inicio)} → {fmtData(c.fim)}
                  </td>
                  <td className="num pv-mono">{fmtBRLcompact(c.valor_anual)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <div className="card pv-card-oportunidade">
          <div className="card-header">
            <div>
              <div className="card-title">Oportunidade pós-venda</div>
              <div className="card-subtitle">Frota sem contrato JDCP</div>
            </div>
          </div>
          <div className="pv-op-corpo">
            <div className="pv-op-valor">
              <span className="pv-op-label">Potencial anual estimado</span>
              <span className="pv-op-num">{fmtBRLcompact(p.frota_sem_jdcp.reduce((s, x) => s + x.potencial, 0))}</span>
            </div>
            <ul className="pv-op-lista">
              {p.frota_sem_jdcp.map((e) => (
                <li key={e.chassi}>
                  <span>
                    {e.modelo} · {e.ano} · chassi ...{e.chassi.slice(-6)}
                  </span>
                  <span className="pv-mono">{fmtBRLcompact(e.potencial)}/ano</span>
                </li>
              ))}
            </ul>
            {/* "Criar proposta JDCP" virou o caminho que existe de verdade:
                a tela de Nova Oportunidade. Antes não fazia nada. */}
            <Link to="/oportunidades/nova" className="btn btn-primary btn-sm" style={{ width: '100%' }}>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
                <line x1="12" y1="5" x2="12" y2="19" />
                <line x1="5" y1="12" x2="19" y2="12" />
              </svg>
              Abrir oportunidade JDCP
            </Link>
          </div>
        </div>
      </div>

      {/* ROW 7: NPS comentário */}
      <div className="card pv-card-nps" style={{ marginTop: 20 }}>
        <div className="card-header">
          <div>
            <div className="card-title">Último feedback do cliente</div>
            <div className="card-subtitle">NPS pós-serviço · {fmtData(p.nps.comentario_recente.data)}</div>
          </div>
          <div className="pv-nps-nota">
            <span className="pv-nps-num">{p.nps.comentario_recente.nota}</span>
            <span className="pv-nps-max">/10</span>
          </div>
        </div>
        <div className="pv-nps-corpo">
          <div className="pv-nps-autor">{p.nps.comentario_recente.autor}</div>
          <div className="pv-nps-texto">"{p.nps.comentario_recente.texto}"</div>
        </div>
      </div>

      <div className="pv-footer-note">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="16" x2="12" y2="12" />
          <line x1="12" y1="8" x2="12.01" y2="8" />
        </svg>
        <span>
          Fontes: <b>Protheus</b> (OS, faturamento, contratos) · <b>JDLink</b> (telemetria, DTCs) · <b>Portal JD Dealer</b>{' '}
          (PMPs, boletins DFF/PIP) · <b>Fabric</b> (agregações). Sincronização a cada 15 minutos no MVP.
        </span>
      </div>
    </>
  );
}
