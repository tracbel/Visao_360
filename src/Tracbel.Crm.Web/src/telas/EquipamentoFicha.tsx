/**
 * Ficha do Equipamento — porte de `renderEquipamentoFicha()`
 * (`prototipo/referencia/assets/app.js`, linha 2315), dados de
 * `EQUIPAMENTO_DATA` (linha 2212) servidos via `public/dados/equipamento-1RW7250PVMR123456.json`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — o passo 3 aplicou o padrão de tela (documento 07).
 *
 * O que virou real: **Copiar chassi** copia mesmo, **Exportar histórico** e
 * **Exportar CSV** geram o arquivo das revisões e das peças. O que saiu, por
 * não ter destino: **Ver no JD Connect** (não existe endereço do portal por
 * chassi nesta base), **Abrir chamado** e **Ver chamado** (não existe tela de
 * chamado), **Reagendar** (o chamado não é nosso para remarcar) e o **Abrir no
 * Operations Center** da telemetria.
 *
 * Também foi corrigida a ordem dos estados: o `if` de carregando vinha antes do
 * de erro e testava `!equipamento`, então toda falha de leitura ficava
 * eternamente com cara de "Carregando…".
 */
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { AbasFicha, type AbaFicha } from '../componentes/AbasFicha';
import { BlocoFicha } from '../componentes/BlocoFicha';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import { useDados } from '../dados/useDados';
import type { Equipamento, EquipamentoRevisao } from '../tipos/equipamento';

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const DIAS_SEMANA = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];

// "Hoje" fixo do protótipo (mesma referência de AGENDA_HOJE/HOJE_CARTEIRA em
// prototipo/dados-seed/constantes-escalares.json) — o original usa `new Date()`
// aqui, mas isso tornaria a garantia (KPI "2 anos") dependente do dia em que a
// tela é aberta. Fixamos para bater sempre com a captura de referência.
const HOJE_FICHA = new Date('2026-08-25T12:00:00.000Z');

function fmtBRL(v: number) {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}
function fmtNum(v: number) {
  return v.toLocaleString('pt-BR');
}
function fmtDataHora(iso: string) {
  const d = new Date(iso);
  return `${DIAS_SEMANA[d.getDay()]} · ${d.getDate().toString().padStart(2, '0')}/${MESES[d.getMonth()]} · ${d
    .getHours()
    .toString()
    .padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
}

/** Ícone de chave de fenda — usado no alerta de chamado aberto e no marcador de cada revisão. */
function IconeChave() {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
      <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z" />
    </svg>
  );
}

export function EquipamentoFicha() {
  const leitura = useDados<Equipamento>('equipamento-1RW7250PVMR123456');
  const { dados: equipamento, erro, carregando } = leitura;
  const [abaAtiva, setAbaAtiva] = useState('visao');
  const [chassiCopiado, setChassiCopiado] = useState(false);

  // O ERRO VEM ANTES DO CARREGANDO: com a ordem invertida, `!equipamento` era
  // verdadeiro nos dois casos e a falha nunca chegava a aparecer.
  if (erro) {
    return <BlocoErro erro={erro} aoTentarDeNovo={leitura.recarregar} />;
  }
  if (carregando || !equipamento) {
    return <BlocoCarregando oQue="a ficha do equipamento" />;
  }

  const e = equipamento;

  /**
   * Copia o chassi. O aviso é o próprio rótulo do botão por dois segundos: um
   * `alert()` para dizer "copiei" custa um clique a mais do que a ação inteira.
   */
  async function copiarChassi() {
    try {
      await navigator.clipboard.writeText(e.chassi);
      setChassiCopiado(true);
      setTimeout(() => setChassiCopiado(false), 2000);
    } catch {
      // Sem permissão de área de transferência (contexto não seguro, por
      // exemplo). Não inventa sucesso: o rótulo simplesmente não muda.
    }
  }

  function exportarHistorico() {
    baixarCsv(
      `equipamento-${e.chassi}-revisoes-${carimboDeData()}`,
      ['Data', 'Tipo', 'Horas', 'Técnico', 'Duração (h)', 'Custo', 'OS', 'Status', 'Observação'],
      e.revisoes.map((r) => [r.data, r.tipo, r.horas, r.tecnico, r.duracao_h, r.custo, r.os, r.status, r.obs]),
    );
  }

  const diasParaExpirar = Math.round((new Date(e.garantia.fim).getTime() - HOJE_FICHA.getTime()) / 86400000);

  const abas: AbaFicha[] = [
    { chave: 'visao', rotulo: 'Visão geral', conteudo: <AbaVisaoGeral e={e} /> },
    { chave: 'revisoes', rotulo: `Revisões (${e.revisoes.length})`, conteudo: <AbaRevisoes e={e} /> },
    { chave: 'pecas', rotulo: `Peças aplicadas (${e.pecas.length})`, conteudo: <AbaPecas e={e} /> },
    { chave: 'telemetria', rotulo: 'Telemetria', conteudo: <AbaTelemetria e={e} /> },
    { chave: 'comercial', rotulo: 'Comercial', conteudo: <AbaComercial e={e} /> },
  ];

  return (
    <>
      <div className="equip-header">
        <div className="equip-header-left">
          <div className="equip-icon-hero">
            <svg viewBox="0 0 100 60" fill="none" stroke="currentColor" strokeWidth={2}>
              <rect x="20" y="20" width="60" height="25" rx="3" fill="currentColor" fillOpacity={0.15} />
              <rect x="35" y="10" width="30" height="15" rx="2" fill="currentColor" fillOpacity={0.25} />
              <circle cx="30" cy="50" r="8" fill="currentColor" fillOpacity={0.3} />
              <circle cx="70" cy="50" r="10" fill="currentColor" fillOpacity={0.3} />
              <line x1="20" y1="30" x2="80" y2="30" />
            </svg>
          </div>
          <div className="equip-header-info">
            <div className="equip-eyebrow">
              <span className="badge badge-jd">John Deere · {e.linha}</span>
              <span className="badge badge-success">{e.status}</span>
              <span className="equip-chassi mono">CHASSI {e.chassi}</span>
            </div>
            <h1 className="cliente-nome">{e.modelo}</h1>
            <div className="cliente-sub">
              {e.potencia} · {e.ano_modelo} · N/S {e.numero_serie} ·{' '}
              <Link to={`/clientes/${e.cliente.id}`} className="cliente-link">
                {e.cliente.razao}
              </Link>
            </div>
          </div>
        </div>
        <div className="cliente-header-actions">
          <button type="button" className="btn btn-ghost" onClick={copiarChassi}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <rect x="9" y="9" width="13" height="13" rx="2" ry="2" />
              <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
            </svg>
            {chassiCopiado ? 'Chassi copiado' : 'Copiar chassi'}
          </button>
          <button type="button" className="btn btn-ghost" onClick={exportarHistorico}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
              <polyline points="17 8 12 3 7 8" />
              <line x1="12" y1="3" x2="12" y2="15" />
            </svg>
            Exportar histórico
          </button>
          {/* "Ver no JD Connect" e "Abrir chamado" saíram: não há endereço do
              portal por chassi nesta base, e não existe tela de chamado. */}
        </div>
      </div>

      <div className="kpi-grid" style={{ gridTemplateColumns: 'repeat(5,1fr)', marginBottom: 24 }}>
        <div className="kpi">
          <span className="kpi-label">Horas de operação</span>
          <span className="kpi-value">
            {fmtNum(e.horas_operacao.atual)}
            <span style={{ fontSize: 14, color: 'var(--text-tertiary)', fontWeight: 500 }}> h</span>
          </span>
          <span className="kpi-hint">Última leitura: {fmtDataHora(e.horas_operacao.ultima_leitura)}</span>
        </div>
        <div className="kpi kpi-highlight">
          <span className="kpi-label">Garantia</span>
          <span className="kpi-value">
            {diasParaExpirar > 365 ? `${Math.round(diasParaExpirar / 365)} anos` : `${diasParaExpirar} dias`}
          </span>
          <span className="kpi-hint">Ativa até {new Date(e.garantia.fim).toLocaleDateString('pt-BR')}</span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Uso da garantia</span>
          <span className="kpi-value">{e.garantia.percentual_uso.toFixed(1)}%</span>
          <span className="kpi-hint">
            {fmtNum(e.garantia.horas_atual)}h de {fmtNum(e.garantia.horas_limite)}h
          </span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Próxima revisão</span>
          <span className="kpi-value" style={{ color: '#F59E0B' }}>
            {e.horas_operacao.horas_para_proxima_revisao}
            <span style={{ fontSize: 14, color: 'var(--text-tertiary)', fontWeight: 500 }}> h</span>
          </span>
          <span className="kpi-hint">
            Previsto {new Date(e.horas_operacao.proxima_revisao_previsao).toLocaleDateString('pt-BR')} ·{' '}
            {e.horas_operacao.proxima_revisao_horas}h
          </span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Eficiência (telemetria)</span>
          <span className="kpi-value" style={{ color: 'var(--jd-green)' }}>
            {e.telemetria.eficiencia}%
          </span>
          <span className="kpi-hint">
            Consumo: {e.telemetria.consumo_medio_lh} L/h · ideal {e.telemetria.consumo_ideal_lh}
          </span>
        </div>
      </div>

      {e.chamados_abertos.length > 0 && (
        <div
          className="alteracao-pendente"
          style={{
            background: 'linear-gradient(90deg,rgba(2,132,199,.06),transparent)',
            borderColor: 'rgba(2,132,199,.3)',
            borderLeftColor: '#0284C7',
          }}
        >
          <div className="alteracao-icon" style={{ color: '#075985' }}>
            <IconeChave />
          </div>
          <div className="alteracao-body">
            <div className="alteracao-titulo">Chamado aberto · Preventiva 3000h · #{e.chamados_abertos[0].id}</div>
            <div className="alteracao-detalhes">
              <span>
                Prioridade <strong>{e.chamados_abertos[0].prioridade}</strong> · Técnico agendado:{' '}
                <strong>{e.chamados_abertos[0].tecnico_agendado}</strong> · Previsão:{' '}
                <strong>{new Date(e.chamados_abertos[0].previsao).toLocaleDateString('pt-BR')}</strong>
              </span>
            </div>
            <div className="alteracao-motivo">{e.chamados_abertos[0].descricao}</div>
          </div>
          {/* "Ver chamado" e "Reagendar" saíram: não existe tela de chamado, e
              remarcar um atendimento é escrita no sistema da oficina. */}
        </div>
      )}

      <AbasFicha abas={abas} ativa={abaAtiva} aoSelecionar={setAbaAtiva} />
    </>
  );
}

function AbaVisaoGeral({ e }: { e: Equipamento }) {
  const max = Math.max(...e.historico_horas.map((h) => h.horas));
  const ganhoMensal = Math.round(
    (e.historico_horas[e.historico_horas.length - 1].horas - e.historico_horas[0].horas) /
      (e.historico_horas.length - 1),
  );

  return (
    <div className="ficha-grid">
      <div className="ficha-col-main">
        <BlocoFicha id="identificacao" titulo="Identificação do equipamento">
          <div className="ficha-fields">
            <div className="ff">
              <label>Chassi (VIN)</label>
              <span className="mono">{e.chassi}</span>
            </div>
            <div className="ff">
              <label>Número de série</label>
              <span className="mono">{e.numero_serie}</span>
            </div>
            <div className="ff">
              <label>Marca / Modelo</label>
              <span>
                <strong>{e.marca}</strong> · {e.modelo}
              </span>
            </div>
            <div className="ff">
              <label>Linha</label>
              <span>{e.linha}</span>
            </div>
            <div className="ff">
              <label>Potência</label>
              <span>
                {e.potencia} <span className="muted">({e.potencia_faixa})</span>
              </span>
            </div>
            <div className="ff">
              <label>Ano modelo / fabricação</label>
              <span>
                {e.ano_modelo} / {e.ano_fabricacao}
              </span>
            </div>
            <div className="ff">
              <label>Transmissão</label>
              <span>{e.transmissao}</span>
            </div>
            <div className="ff">
              <label>Tração</label>
              <span>{e.tracao}</span>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="tecnicas" titulo="Especificações técnicas">
          <div className="ficha-fields">
            <div className="ff">
              <label>Cabine</label>
              <span>{e.cabine}</span>
            </div>
            <div className="ff">
              <label>Peso operacional</label>
              <span>{e.peso_operacional}</span>
            </div>
            <div className="ff">
              <label>Tanque de diesel</label>
              <span>{e.tanque_diesel}</span>
            </div>
            <div className="ff">
              <label>Cor</label>
              <span>{e.cor}</span>
            </div>
            <div className="ff">
              <label>Pneus dianteiros</label>
              <span className="mono">{e.pneus_diant}</span>
            </div>
            <div className="ff">
              <label>Pneus traseiros</label>
              <span className="mono">{e.pneus_tras}</span>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="horas" titulo="Evolução de horas operacionais" espacoso>
          <div className="horas-chart-wrap">
            <div className="horas-chart">
              {e.historico_horas.map((h) => {
                const pct = ((h.horas / max) * 100).toFixed(0);
                const [ano, mes] = h.data.split('-');
                const mesLabel = MESES[parseInt(mes, 10) - 1];
                return (
                  <div className="horas-bar-col" key={h.data}>
                    <div className="horas-bar-val">{fmtNum(h.horas)}</div>
                    <div className="horas-bar" style={{ height: `${pct}%` }}>
                      <div className="horas-bar-fill" />
                    </div>
                    <div className="horas-bar-label">
                      {mesLabel}/{ano.substring(2)}
                    </div>
                  </div>
                );
              })}
            </div>
            <div className="horas-stats">
              <div className="hs-item">
                <span className="hs-label">Média diária (30d)</span>
                <span className="hs-value">{e.horas_operacao.media_diaria_30d} h</span>
              </div>
              <div className="hs-item">
                <span className="hs-label">Média diária (90d)</span>
                <span className="hs-value">{e.horas_operacao.media_diaria_90d} h</span>
              </div>
              <div className="hs-item">
                <span className="hs-label">Ganho mensal médio</span>
                <span className="hs-value">+{ganhoMensal} h</span>
              </div>
              <div className="hs-item">
                <span className="hs-label">Fonte</span>
                <span className="hs-value" style={{ fontSize: 11 }}>
                  {e.horas_operacao.fonte}
                </span>
              </div>
            </div>
          </div>
        </BlocoFicha>
      </div>

      <div className="ficha-col-side">
        <BlocoFicha id="cliente" titulo="Cliente atual">
          <Link to={`/clientes/${e.cliente.id}`} className="cliente-link-card">
            <div className="cliente-avatar" style={{ width: 44, height: 44, fontSize: 16, borderRadius: 10 }}>
              AG
            </div>
            <div>
              <div className="cen-nome">{e.cliente.razao}</div>
              <div className="cen-role">
                Classe {e.cliente.classe} · #{e.cliente.id}
              </div>
            </div>
            <svg
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth={2}
              style={{ marginLeft: 'auto' }}
            >
              <polyline points="9 18 15 12 9 6" />
            </svg>
          </Link>
          <div style={{ borderTop: '1px dashed var(--border-primary)', margin: '12px 0', paddingTop: 12 }}>
            <div className="audit-row">
              <span className="audit-label">Localização atual</span>
              <span className="audit-value">{e.localizacao}</span>
            </div>
            <div className="audit-row">
              <span className="audit-label">Operador principal</span>
              <span className="audit-value">{e.operador_principal}</span>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="garantia" titulo="Garantia PowerGard">
          <div className="garantia-status">
            <div className="garantia-badge garantia-ativa">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.5}>
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
                <polyline points="9 12 11 14 15 10" />
              </svg>
              <span>Ativa</span>
            </div>
            <div className="garantia-linha">
              <span>Duração</span>
              <strong>
                {new Date(e.garantia.inicio).toLocaleDateString('pt-BR')} →{' '}
                {new Date(e.garantia.fim).toLocaleDateString('pt-BR')}
              </strong>
            </div>
            <div className="garantia-linha">
              <span>Uso</span>
              <strong>
                {e.garantia.percentual_uso.toFixed(1)}% ({fmtNum(e.garantia.horas_atual)}/{fmtNum(e.garantia.horas_limite)}h)
              </strong>
            </div>
            <div className="garantia-bar-track">
              <div className="garantia-bar-fill" style={{ width: `${e.garantia.percentual_uso}%` }} />
            </div>
            <div className="garantia-info">
              <div>
                <label>Tipo</label>
                <span>{e.garantia.tipo}</span>
              </div>
              <div>
                <label>Contrato</label>
                <span className="mono">{e.garantia.contrato}</span>
              </div>
              <div>
                <label>Cobre</label>
                <span style={{ fontSize: 12 }}>{e.garantia.cobertura}</span>
              </div>
              <div>
                <label>Não cobre</label>
                <span style={{ fontSize: 12, color: 'var(--text-tertiary)' }}>{e.garantia.exclusoes}</span>
              </div>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="auditoria" titulo="Auditoria">
          <div className="audit-list">
            <div className="audit-row">
              <span className="audit-label">Origem inclusão</span>
              <span className="audit-value mono">Protheus-Sync · 24/05/2023</span>
            </div>
            <div className="audit-row">
              <span className="audit-label">Última atualização</span>
              <span className="audit-value mono">JD Sync · 24/08/2026 18:30</span>
            </div>
            <div className="audit-row">
              <span className="audit-label">Pedido Protheus</span>
              <span className="audit-value mono">{e.aquisicao.pedido_protheus}</span>
            </div>
            <div className="audit-row">
              <span className="audit-label">Nota fiscal</span>
              <span className="audit-value mono">{e.aquisicao.nota_fiscal}</span>
            </div>
          </div>
        </BlocoFicha>
      </div>
    </div>
  );
}

function RevisaoTimelineItem({ r }: { r: EquipamentoRevisao }) {
  return (
    <div className="revisao-item">
      <div className="revisao-marker">
        <IconeChave />
      </div>
      <div className="revisao-body">
        <div className="revisao-header">
          <div>
            <div className="revisao-titulo">{r.tipo}</div>
            <div className="revisao-meta">
              {new Date(r.data).toLocaleDateString('pt-BR')} · {fmtNum(r.horas)}h · {r.tecnico} · {r.duracao_h}h de
              execução
            </div>
          </div>
          <div className="revisao-actions">
            <span className="revisao-os mono">{r.os}</span>
            <span className="revisao-custo">{fmtBRL(r.custo)}</span>
          </div>
        </div>
        <div className="revisao-obs">{r.obs}</div>
      </div>
    </div>
  );
}

function AbaRevisoes({ e }: { e: Equipamento }) {
  return (
    <>
      <div className="revisoes-timeline">
        {e.revisoes.map((r) => (
          <RevisaoTimelineItem r={r} key={r.id} />
        ))}
      </div>
      <div className="funil-scope-note" style={{ marginTop: 20 }}>
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="8" x2="12" y2="12" />
          <line x1="12" y1="16" x2="12.01" y2="16" />
        </svg>
        <span>Revisões vêm do módulo TOTVS Protheus SIGAOFI (Oficina). O CRM só exibe; alterações são feitas no Protheus.</span>
      </div>
    </>
  );
}

function AbaPecas({ e }: { e: Equipamento }) {
  const total = e.pecas.reduce((s, p) => s + p.valor * p.qtd, 0);

  function exportar() {
    baixarCsv(
      `equipamento-${e.chassi}-pecas-${carimboDeData()}`,
      ['Data', 'Código', 'Descrição', 'Qtd', 'Valor unitário', 'Total', 'OS'],
      e.pecas.map((pc) => [pc.data, pc.codigo, pc.descricao, pc.qtd, pc.valor, pc.valor * pc.qtd, pc.os]),
    );
  }

  return (
    <div className="card">
      <div className="card-header">
        <div>
          <div className="card-title">Peças aplicadas · {e.pecas.length} itens</div>
          <div className="card-subtitle">Total gasto em peças: {fmtBRL(total)}</div>
        </div>
        <button type="button" className="btn btn-ghost btn-sm" onClick={exportar}>
          Exportar CSV
        </button>
      </div>
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Data</th>
              <th>Código</th>
              <th>Descrição</th>
              <th className="num">Qtd</th>
              <th className="num">Valor unit.</th>
              <th className="num">Total</th>
              <th>OS</th>
            </tr>
          </thead>
          <tbody>
            {e.pecas.map((p, i) => (
              <tr key={`${p.os}-${p.codigo}-${i}`}>
                <td>{new Date(p.data).toLocaleDateString('pt-BR')}</td>
                <td className="mono">{p.codigo}</td>
                <td>{p.descricao}</td>
                <td className="num">{p.qtd}</td>
                <td className="num">{fmtBRL(p.valor)}</td>
                <td className="num">
                  <strong>{fmtBRL(p.valor * p.qtd)}</strong>
                </td>
                <td className="mono">{p.os}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function AbaTelemetria({ e }: { e: Equipamento }) {
  const excesso = ((e.telemetria.consumo_medio_lh / e.telemetria.consumo_ideal_lh - 1) * 100).toFixed(1);
  return (
    <>
      <div className="kpi-grid" style={{ gridTemplateColumns: 'repeat(4,1fr)', marginBottom: 20 }}>
        <div className="kpi">
          <span className="kpi-label">Consumo médio</span>
          <span className="kpi-value">
            {e.telemetria.consumo_medio_lh} <span style={{ fontSize: 14, color: 'var(--text-tertiary)', fontWeight: 500 }}>L/h</span>
          </span>
          <span className="kpi-hint">
            Ideal para modelo: {e.telemetria.consumo_ideal_lh} L/h · <span style={{ color: '#F59E0B' }}>+{excesso}%</span>
          </span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Horas ociosas</span>
          <span className="kpi-value">{e.telemetria.horas_ociosas_pct}%</span>
          <span className="kpi-hint">Motor ligado sem trabalho</span>
        </div>
        <div className="kpi kpi-highlight">
          <span className="kpi-label">Uso do AutoTrac</span>
          <span className="kpi-value">{e.telemetria.autotrac_uso_pct}%</span>
          <span className="kpi-hint">Piloto automático GPS</span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Códigos de falha (30d)</span>
          <span className="kpi-value" style={{ color: e.telemetria.codigos_falha_30d > 5 ? '#DC2626' : 'var(--jd-green)' }}>
            {e.telemetria.codigos_falha_30d}
          </span>
          <span className="kpi-hint">
            Histórico: {e.telemetria.codigos_falha_historico} · {e.telemetria.alertas_ativos} ativos
          </span>
        </div>
      </div>
      <div className="card">
        <div className="card-header">
          <div>
            <div className="card-title">Telemetria John Deere Operations Center</div>
            <div className="card-subtitle">Última sincronização: {fmtDataHora(e.telemetria.ultimo_sync)}</div>
          </div>
          {/* "Abrir no JD Connect" saiu: não há endereço do portal por chassi
              nesta base, e um link externo sem destino é pior que nenhum. */}
        </div>
        <div style={{ padding: 24, textAlign: 'center', color: 'var(--text-tertiary)', borderTop: '1px solid var(--border-primary)' }}>
          <svg
            width="48"
            height="48"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth={1.5}
            style={{ marginBottom: 12, opacity: 0.5 }}
          >
            <rect x="3" y="3" width="18" height="18" rx="2" />
            <path d="M3 12h18M12 3v18" />
          </svg>
          <div>Mapas de operação, curvas de consumo por talhão e log de códigos de falha ficam no portal JD.</div>
          <div style={{ fontSize: 12, marginTop: 8 }}>CRM mostra só as métricas agregadas.</div>
        </div>
      </div>
    </>
  );
}

function AbaComercial({ e }: { e: Equipamento }) {
  const descontoPct = ((e.aquisicao.desconto / e.aquisicao.valor_lista) * 100).toFixed(1);
  return (
    <div className="ficha-grid">
      <div className="ficha-col-main">
        <BlocoFicha id="aquisicao" titulo="Aquisição">
          <div className="ficha-fields">
            <div className="ff">
              <label>Tipo de operação</label>
              <span>{e.aquisicao.tipo}</span>
            </div>
            <div className="ff">
              <label>Concessionária</label>
              <span>{e.aquisicao.concessionaria}</span>
            </div>
            <div className="ff">
              <label>Data da venda</label>
              <span>{new Date(e.aquisicao.data_venda).toLocaleDateString('pt-BR')}</span>
            </div>
            <div className="ff">
              <label>Data de entrega</label>
              <span>{new Date(e.aquisicao.data_entrega).toLocaleDateString('pt-BR')}</span>
            </div>
            <div className="ff">
              <label>Vendedor (CEN)</label>
              <span>{e.aquisicao.vendedor}</span>
            </div>
            <div className="ff">
              <label>Cliente atual</label>
              <span>{e.cliente.razao}</span>
            </div>
            <div className="ff">
              <label>Pedido Protheus</label>
              <span className="mono">{e.aquisicao.pedido_protheus}</span>
            </div>
            <div className="ff">
              <label>Nota fiscal</label>
              <span className="mono">{e.aquisicao.nota_fiscal}</span>
            </div>
          </div>
        </BlocoFicha>
      </div>
      <div className="ficha-col-side">
        <BlocoFicha id="valores" titulo="Valores da venda">
          <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            <div className="valor-row">
              <span>Valor de lista</span>
              <strong>{fmtBRL(e.aquisicao.valor_lista)}</strong>
            </div>
            <div className="valor-row" style={{ color: '#DC2626' }}>
              <span>Desconto</span>
              <strong>−{fmtBRL(e.aquisicao.desconto)}</strong>
            </div>
            <div className="valor-row" style={{ paddingTop: 12, borderTop: '1px dashed var(--border-primary)', fontSize: 16 }}>
              <span>
                <strong>Valor faturado</strong>
              </span>
              <strong style={{ color: 'var(--jd-green-dark)', fontFamily: "'JetBrains Mono',monospace" }}>
                {fmtBRL(e.aquisicao.valor_faturado)}
              </strong>
            </div>
            <div style={{ fontSize: 11, color: 'var(--text-tertiary)', marginTop: 4 }}>
              Desconto de {descontoPct}%
            </div>
          </div>
        </BlocoFicha>
      </div>
    </div>
  );
}
