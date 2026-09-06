/**
 * Cálculos da Performance de CEN — porte de `calcularPerfCEN`, `calcularPerfGrupo`,
 * `perfCENsAtivos`, `classAtingimento` e `renderPerfInsights`
 * (`prototipo/referencia/assets/app.js`, linhas 6025-6570; mesma lógica em
 * `prototipo/referencia/perf-block.js`).
 *
 * Diferença proposital em relação a `perf-block.js` (que foi extraído isolado
 * só para gerar os JSONs de seed): aqui `calcularPerfCEN` também soma as
 * oportunidades criadas na sessão atual (`window.OPORTUNIDADES_NOVAS` no
 * original, app.js linhas 6041-6044) ao pipeline aberto do CEN dono — é o que
 * faz "Nova Oportunidade" refletir imediatamente nesta tela.
 *
 * Em `renderPerfInsights` (app.js linha 6479), o insight "quem está no top"
 * ordena o array `detalhes` in-place (`Array.prototype.sort`) e reaproveita
 * essa MESMA referência nos insights seguintes — um efeito colateral que muda
 * a ordem dos nomes listados em "cobertura baixa", "crescimento" e "queda".
 * Aqui usamos uma cópia (`[...detalhes].sort(...)`) para não mutar a lista
 * compartilhada; não há captura de referência que dependa da ordem afetada
 * (a tela abre no modo "Minha performance", com 1 único CEN).
 */
import { obterOportunidadesNovas } from './persistencia';
import type { PerfCen, PerfDetalheCen, PerfGrupo, PerfSeries, PersonaPerf } from '../tipos/performanceCen';

function fmtBRLcompact(v: number): string {
  if (v >= 1_000_000) return `R$ ${(v / 1_000_000).toFixed(v >= 10_000_000 ? 1 : 2)}M`;
  if (v >= 1_000) return `R$ ${(v / 1_000).toFixed(0)}k`;
  return `R$ ${v}`;
}

/** Retorno de `calcularPerfCEN` (app.js linha 6025 / perf-block.js linha 108). */
export function calcularPerfCEN(cenId: string, cens: PerfCen[], series: PerfSeries): PerfDetalheCen {
  const serie = series[cenId];
  const cen = cens.find((c) => c.id === cenId)!;
  const mesAtual = serie[serie.length - 1];
  const fytd_vendas = serie.reduce((s, m) => s + m.vendas, 0);
  const fytd_pipe = serie.reduce((s, m) => s + m.pipeline_criado, 0);
  const fytd_ganhas = serie.reduce((s, m) => s + m.oport_ganhas, 0);
  const fytd_totais = serie.reduce((s, m) => s + m.oport_totais, 0);
  const fytd_visitas = serie.reduce((s, m) => s + m.visitas, 0);
  const cob_atual = mesAtual.cobertura;
  const conv_fytd = fytd_totais > 0 ? fytd_ganhas / fytd_totais : 0;
  const ticket_fytd = fytd_ganhas > 0 ? fytd_vendas / fytd_ganhas : 0;

  // Pipeline aberto = pipeline criado - vendas ganhas (aproximação), mais o
  // que foi criado nesta sessão via "Nova Oportunidade" para este CEN.
  let pipe_aberto = fytd_pipe - fytd_vendas;
  const novasDoCEN = obterOportunidadesNovas().filter((op) => op.cen === cen.id);
  pipe_aberto += novasDoCEN.reduce((s, op) => s + op.valor, 0);

  return {
    cen,
    serie,
    mes_atual: mesAtual,
    mes_vendas: mesAtual.vendas,
    mes_meta: cen.meta_mes,
    mes_atingimento: mesAtual.vendas / cen.meta_mes,
    fytd_vendas,
    fytd_meta: cen.meta_fytd,
    fytd_atingimento: fytd_vendas / cen.meta_fytd,
    pipeline_aberto: pipe_aberto,
    conversao_fytd: conv_fytd,
    cobertura: cob_atual,
    ticket_medio_fytd: ticket_fytd,
    fytd_visitas,
    fytd_ganhas,
    fytd_totais,
  };
}

/** Retorno de `calcularPerfGrupo` (app.js linha 6066 / perf-block.js linha 144). */
export function calcularPerfGrupo(cenIds: string[], cens: PerfCen[], series: PerfSeries): PerfGrupo {
  const detalhes = cenIds.map((id) => calcularPerfCEN(id, cens, series));
  const soma = (fn: (d: PerfDetalheCen) => number) => detalhes.reduce((s, d) => s + fn(d), 0);
  const mes_vendas = soma((d) => d.mes_vendas);
  const mes_meta = soma((d) => d.mes_meta);
  const fytd_vendas = soma((d) => d.fytd_vendas);
  const fytd_meta = soma((d) => d.fytd_meta);
  const pipeline = soma((d) => d.pipeline_aberto);
  const fytd_ganhas = soma((d) => d.fytd_ganhas);
  const fytd_totais = soma((d) => d.fytd_totais);
  const conversao = fytd_totais > 0 ? fytd_ganhas / fytd_totais : 0;
  const ticket = fytd_ganhas > 0 ? fytd_vendas / fytd_ganhas : 0;
  const cob = detalhes.reduce((s, d) => s + d.cobertura, 0) / detalhes.length;
  return {
    detalhes,
    mes_vendas,
    mes_meta,
    mes_atingimento: mes_vendas / mes_meta,
    fytd_vendas,
    fytd_meta,
    fytd_atingimento: fytd_vendas / fytd_meta,
    pipeline_aberto: pipeline,
    conversao_fytd: conversao,
    cobertura: cob,
    ticket_medio_fytd: ticket,
    n_cens: detalhes.length,
  };
}

/** `perfCENsAtivos` (app.js linha 6230 / perf-block.js linha 308). */
export function perfCENsAtivos(persona: PersonaPerf, cens: PerfCen[]): string[] {
  if (persona === 'cen') return ['joao'];
  if (persona === 'regional') return cens.filter((c) => c.regional === 'MT Norte').map((c) => c.id);
  return cens.map((c) => c.id);
}

/** `classAtingimento` (app.js linha 6455 / perf-block.js linha 533). */
export function classAtingimento(pct: number): string {
  if (pct >= 1) return 'val-ok';
  if (pct >= 0.85) return 'val-quase';
  if (pct >= 0.7) return 'val-warn';
  return 'val-danger';
}

export type PerfInsight = { tipo: 'ok' | 'warn' | 'danger' | 'info'; icon: string; texto: string };

/**
 * `renderPerfInsights` (app.js linha 6462 / perf-block.js linha 540), sem a
 * parte de DOM: retorna a lista de insights para `PerfInsights` renderizar.
 * `texto` mantém o HTML inline (`<strong>`) do original — conteúdo sempre
 * derivado dos mocks locais, nunca de entrada do usuário.
 */
export function gerarInsights(
  detalhes: PerfDetalheCen[],
  ids: string[],
  cens: PerfCen[],
  series: PerfSeries,
): PerfInsight[] {
  const insights: PerfInsight[] = [];

  // Insight 1: quem está fora da meta mês
  const fora = detalhes.filter((d) => d.mes_atingimento < 0.7);
  if (fora.length > 0 && ids.length > 1) {
    insights.push({
      tipo: 'danger',
      icon: '⚠️',
      texto: `<strong>${fora.length} CEN${fora.length > 1 ? 's' : ''} abaixo de 70% da meta em Ago/26</strong>: ${fora.map((d) => d.cen.nome.split(' ')[0]).join(', ')}. Considere revisão de pipeline e reforço de fechamento.`,
    });
  }

  // Insight 2: quem está no top
  if (ids.length > 1) {
    const top = [...detalhes].sort((a, b) => b.fytd_atingimento - a.fytd_atingimento)[0];
    insights.push({
      tipo: 'ok',
      icon: '🏆',
      texto: `<strong>${top.cen.nome}</strong> lidera em atingimento FYTD com ${(top.fytd_atingimento * 100).toFixed(0)}% da meta. Considere replicar padrão de cobertura (${top.cobertura}%) e ritmo de visitas nas reuniões da regional.`,
    });
  }

  // Insight 3: cobertura baixa correlaciona com atingimento?
  const cobBaixa = detalhes.filter((d) => d.cobertura < 70);
  const foraMeta = detalhes.filter((d) => d.fytd_atingimento < 0.85);
  const intersect = cobBaixa.filter((c) => foraMeta.some((f) => f.cen.id === c.cen.id));
  if (intersect.length > 0 && ids.length > 1) {
    insights.push({
      tipo: 'warn',
      icon: '📊',
      texto: `<strong>${intersect.length} CEN${intersect.length > 1 ? 's' : ''} com cobertura de carteira < 70% e atingimento < 85%</strong>: ${intersect.map((d) => d.cen.nome.split(' ')[0]).join(', ')}. Correlação sugere que aumentar contato com base pode destravar vendas.`,
    });
  }

  // Insight 4: crescimento mensal
  const emCrescimento = detalhes.filter((d) => {
    const s = d.serie;
    if (s.length < 3) return false;
    const ultimos3 = s.slice(-3);
    return ultimos3[2].vendas > ultimos3[0].vendas * 1.15;
  });
  if (emCrescimento.length > 0) {
    insights.push({
      tipo: 'ok',
      icon: '📈',
      texto: `<strong>${emCrescimento.length} CEN${emCrescimento.length > 1 ? 's' : ''} em crescimento acima de 15%</strong> nos últimos 3 meses: ${emCrescimento.map((d) => d.cen.nome.split(' ')[0]).join(', ')}. Sinal positivo de aceleração no fim do ano fiscal.`,
    });
  }

  // Insight 5: ticket médio alto/baixo
  if (ids.length > 1) {
    const sorted = [...detalhes].sort((a, b) => b.ticket_medio_fytd - a.ticket_medio_fytd);
    const alto = sorted[0];
    const baixo = sorted[sorted.length - 1];
    if (alto.ticket_medio_fytd / baixo.ticket_medio_fytd > 1.4) {
      insights.push({
        tipo: 'info',
        icon: '💰',
        texto: `<strong>Discrepância de ticket médio</strong>: ${alto.cen.nome.split(' ')[0]} (${fmtBRLcompact(alto.ticket_medio_fytd)}) vende ${((alto.ticket_medio_fytd / baixo.ticket_medio_fytd - 1) * 100).toFixed(0)}% acima de ${baixo.cen.nome.split(' ')[0]} (${fmtBRLcompact(baixo.ticket_medio_fytd)}). Verificar mix de produto e perfil de cliente.`,
      });
    }
  }

  // Insight 6: piorou vs início FYTD
  const piorou = detalhes.filter((d) => {
    const s = d.serie;
    const primeiros3 = s.slice(0, 3).reduce((sum, m) => sum + m.vendas, 0) / 3;
    const ultimos3 = s.slice(-3).reduce((sum, m) => sum + m.vendas, 0) / 3;
    return ultimos3 < primeiros3 * 0.9;
  });
  if (piorou.length > 0) {
    insights.push({
      tipo: 'warn',
      icon: '📉',
      texto: `<strong>${piorou.length} CEN${piorou.length > 1 ? 's' : ''} com queda > 10% vs início do FYTD</strong>: ${piorou.map((d) => d.cen.nome.split(' ')[0]).join(', ')}. Investigar causa: mudança de território, perda de clientes chave, ou desengajamento.`,
    });
  }

  // No modo CEN mostra insights pessoais
  if (ids.length === 1) {
    const d = detalhes[0];
    const mediaRegional = calcularPerfGrupo(
      cens.filter((c) => c.regional === d.cen.regional).map((c) => c.id),
      cens,
      series,
    );
    insights.push({
      tipo: d.fytd_atingimento >= mediaRegional.fytd_atingimento ? 'ok' : 'warn',
      icon: d.fytd_atingimento >= mediaRegional.fytd_atingimento ? '✅' : '⚠️',
      texto: `<strong>Comparado com média MT Norte</strong>: seu atingimento FYTD é ${(d.fytd_atingimento * 100).toFixed(0)}% vs ${(mediaRegional.fytd_atingimento * 100).toFixed(0)}% da regional. Ticket médio: ${fmtBRLcompact(d.ticket_medio_fytd)} vs ${fmtBRLcompact(mediaRegional.ticket_medio_fytd)}.`,
    });
    if (d.mes_atingimento < 0.85) {
      insights.push({
        tipo: 'warn',
        icon: '🎯',
        texto: `<strong>Atingimento de agosto em ${(d.mes_atingimento * 100).toFixed(0)}%</strong> — parcial de 17 dias úteis. Faltam ${fmtBRLcompact(d.mes_meta - d.mes_vendas)} para bater os R$ ${(d.mes_meta / 1e6).toFixed(1)}M do mês. Pipeline aberto: ${fmtBRLcompact(d.pipeline_aberto)}.`,
      });
    }
  }

  return insights;
}
