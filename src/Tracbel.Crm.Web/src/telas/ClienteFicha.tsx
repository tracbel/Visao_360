/**
 * Ficha do Cliente — porte de `renderClienteFicha()` (linha 1741),
 * `renderFichaBloco()` (linha 2181) e `mountClienteFicha()` (linha 2197) do
 * protótipo de referência (`prototipo/referencia/assets/app.js`). Dados de
 * `CLIENTE_DATA` (linha 1634) servidos via `public/dados/cliente-84391.json`.
 *
 * A aba Pós-vendas reaproveita o componente já pronto `AbaPosVendas` (porte
 * de `renderPosVendasTab()`/`mountPosVendasCharts()`, linhas 8333/8657). As
 * demais abas seguem o mesmo padrão de `EquipamentoFicha.tsx`/
 * `OportunidadeFicha.tsx`: um componente local por aba, compondo
 * `BlocoFicha`/`AbasFicha`.
 */
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { AbaPosVendas } from '../componentes/AbaPosVendas';
import { AbasFicha, type AbaFicha } from '../componentes/AbasFicha';
import { BlocoFicha } from '../componentes/BlocoFicha';
import { obterOportunidadesNovas } from '../dados/persistencia';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { useDados } from '../dados/useDados';
import type { ClienteFichaData, PosVendasCliente } from '../tipos/clientes';
import type { FasePipeline, OportunidadeNova } from '../tipos/oportunidade';

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const DIAS_SEMANA = ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'];

function fmtBRL(v: number): string {
  return v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}
function fmtNum(v: number): string {
  return v.toLocaleString('pt-BR');
}
function fmtDataHora(iso: string): string {
  const d = new Date(iso);
  return `${DIAS_SEMANA[d.getDay()]} · ${d.getDate().toString().padStart(2, '0')}/${MESES[d.getMonth()]} · ${d
    .getHours()
    .toString()
    .padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
}
function iniciaisContato(nome: string): string {
  return nome
    .split(' ')
    .slice(0, 2)
    .map((p) => p[0])
    .join('');
}

/**
 * As duas exceções do protótipo: o único chassi e a única oportunidade com
 * ficha rica construída. A regra que elas servem é a do documento 05 §8 — só
 * aparece caminho onde existe tela —, e é o que substituiu os três `alert()`
 * que pediam desculpa depois do clique.
 */
const CHASSI_COM_FICHA = '1RW7250PVMR123456';
const OPORTUNIDADE_COM_FICHA = 1517613;

const META_INTERACAO: Record<string, { icon: string; color: string; label: string }> = {
  visita: { icon: '📍', color: '#16A34A', label: 'Visita' },
  ligacao: { icon: '📞', color: '#7C3AED', label: 'Ligação' },
  email: { icon: '✉', color: '#0284C7', label: 'E-mail' },
  whatsapp: { icon: '💬', color: '#059669', label: 'WhatsApp' },
};

export function ClienteFicha() {
  const leituraCliente = useDados<ClienteFichaData>('cliente-84391');
  const leituraPosVendas = useDados<PosVendasCliente>('pos-vendas-cliente-84391');
  const { dados: cliente, erro: erroCliente, carregando: carregandoCliente } = leituraCliente;
  const { dados: posVendas, erro: erroPosVendas, carregando: carregandoPosVendas } = leituraPosVendas;
  const { dados: fasesPipeline } = useDados<FasePipeline[]>('pipeline-fases');
  const [abaAtiva, setAbaAtiva] = useState('visao');
  const navegar = useNavigate();

  // O ERRO VEM ANTES DO CARREGANDO: com a ordem invertida, `!cliente` era
  // verdadeiro nos dois casos e toda falha de leitura ficava eternamente com
  // cara de espera.
  const erroLeitura = erroCliente ?? erroPosVendas;
  if (erroLeitura) {
    return (
      <BlocoErro
        erro={erroLeitura}
        aoTentarDeNovo={() => {
          leituraCliente.recarregar();
          leituraPosVendas.recarregar();
        }}
      />
    );
  }
  if (carregandoCliente || carregandoPosVendas || !cliente || !posVendas) {
    return <BlocoCarregando oQue="a ficha do cliente" />;
  }

  const c = cliente;
  // O decisor é a quem o botão Ligar e o E-mail se dirigem; sem decisor, o
  // primeiro contato da lista. Sem contato nenhum, os dois botões não aparecem.
  const contatoPrincipal = c.contatos.find((x) => x.decisor) ?? c.contatos[0] ?? null;
  // Oportunidades criadas na sessão (tela "Nova Oportunidade", outra frente) para este cliente —
  // mesma leitura de `window.OPORTUNIDADES_NOVAS` do original.
  const novasClienteSA = obterOportunidadesNovas().filter((op) => op.cliente_id === c.id);
  const totalPipeline = c.oportunidades.reduce((s, o) => s + o.valor, 0) + novasClienteSA.reduce((s, o) => s + o.valor, 0);
  const totalOportunidades = c.oportunidades.length + novasClienteSA.length;
  const criticosPv = posVendas.alertas_criticos.filter((a) => a.cor === 'red').length;
  const alteracao = c.alteracoes_pendentes[0];

  const abas: AbaFicha[] = [
    { chave: 'visao', rotulo: 'Visão geral', conteudo: <AbaVisaoGeral c={c} /> },
    { chave: 'frota', rotulo: `Frota (${c.frota.length})`, conteudo: <AbaFrota c={c} /> },
    {
      chave: 'pos-vendas',
      rotulo: (
        <>
          Pós-vendas <span className="tab-badge tab-badge-red">{criticosPv}</span>
        </>
      ),
      conteudo: <AbaPosVendas dados={posVendas} />,
    },
    {
      chave: 'oportunidades',
      rotulo: `Oportunidades (${totalOportunidades})`,
      conteudo: <AbaOportunidades c={c} novas={novasClienteSA} fases={fasesPipeline} navegar={navegar} />,
    },
    { chave: 'historico', rotulo: `Histórico (${c.interacoes.length})`, conteudo: <AbaHistorico c={c} /> },
    { chave: 'financeiro', rotulo: 'Financeiro', conteudo: <AbaFinanceiro c={c} /> },
  ];

  return (
    <>
      <div className="cliente-header">
        <div className="cliente-header-left">
          <div className="cliente-avatar">{c.razao_social.substring(0, 2).toUpperCase()}</div>
          <div className="cliente-header-info">
            <div className="cliente-eyebrow">
              <span className={`badge badge-classe classe-${c.classe.toLowerCase()}`}>Classe {c.classe}</span>
              <span className="badge badge-success">{c.status}</span>
              <span className="cliente-cnpj">CNPJ {c.cnpj}</span>
              <span className="cliente-id-code">#{c.id}</span>
            </div>
            <h1 className="cliente-nome">{c.razao_social}</h1>
            <div className="cliente-sub">
              {c.nome_fantasia} · {c.endereco_principal.cidade}/{c.endereco_principal.uf} · Regional {c.regional}
            </div>
          </div>
        </div>
        {/* As quatro ações ganharam destino. Ligar e E-mail abrem o discador e
            o cliente de e-mail com o contato decisor — é o que o CEN faz da
            tela, e é a única forma de "ligar" que um navegador tem. Nova visita
            leva à Agenda e Nova oportunidade ao cadastro. Nenhuma delas fazia
            nada antes (documento 06). */}
        <div className="cliente-header-actions">
          {contatoPrincipal?.celular ? (
            <a className="btn btn-ghost" href={`tel:${contatoPrincipal.celular.replace(/\D/g, '')}`}>
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
                <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z" />
              </svg>
              Ligar
            </a>
          ) : null}
          {contatoPrincipal?.email ? (
            <a className="btn btn-ghost" href={`mailto:${contatoPrincipal.email}`}>
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
                <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
                <polyline points="22,6 12,13 2,6" />
              </svg>
              E-mail
            </a>
          ) : null}
          <Link to="/agenda" className="btn btn-secondary">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M3 11l19-9-9 19-2-8-8-2z" />
            </svg>
            Nova visita
          </Link>
          <Link to="/oportunidades/nova" className="btn btn-primary">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            Nova oportunidade
          </Link>
        </div>
      </div>

      {/* KPIs resumo */}
      <div className="kpi-grid" style={{ gridTemplateColumns: 'repeat(5,1fr)', marginBottom: 24 }}>
        <div className="kpi">
          <span className="kpi-label">Faturamento YTD 2026</span>
          <span className="kpi-value">{fmtBRL(c.financeiro.faturamento_ytd_2026)}</span>
          <span className="kpi-hint">2025: {fmtBRL(c.financeiro.faturamento_2025)}</span>
        </div>
        <div className="kpi kpi-highlight">
          <span className="kpi-label">Pipeline aberto</span>
          <span className="kpi-value">{fmtBRL(totalPipeline)}</span>
          <span className="kpi-hint">
            {totalOportunidades} oportunidades
            {novasClienteSA.length > 0 && (
              <span style={{ color: 'var(--jd-green)', fontWeight: 600 }}>
                {' '}
                (+{novasClienteSA.length} nova{novasClienteSA.length > 1 ? 's' : ''})
              </span>
            )}
          </span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Frota John Deere</span>
          <span className="kpi-value">{c.frota.length}</span>
          <span className="kpi-hint">
            {c.frota.filter((f) => f.garantia.startsWith('Ativa')).length} em garantia ·{' '}
            {c.frota.filter((f) => f.garantia.startsWith('Expirada')).length} fora
          </span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Área total</span>
          <span className="kpi-value">{fmtNum(c.segmentacao.hectares_totais)} ha</span>
          <span className="kpi-hint">{c.enderecos_adicionais.filter((e) => e.hectares > 0).length} fazendas</span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Crédito disponível</span>
          <span className="kpi-value">{fmtBRL(c.financeiro.limite_disponivel)}</span>
          <span className="kpi-hint">de {fmtBRL(c.financeiro.limite_credito)} · 0 inadimplência</span>
        </div>
      </div>

      {alteracao && (
        <div className="alteracao-pendente">
          <div className="alteracao-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <circle cx="12" cy="12" r="10" />
              <polyline points="12 6 12 12 16 14" />
            </svg>
          </div>
          <div className="alteracao-body">
            <div className="alteracao-titulo">Alteração cadastral pendente · #{alteracao.id}</div>
            <div className="alteracao-detalhes">
              <strong>{alteracao.campo}:</strong>
              <span className="valor-de">{alteracao.valor_atual}</span>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                <path d="M5 12h14M13 5l7 7-7 7" />
              </svg>
              <span className="valor-para">{alteracao.valor_solicitado}</span>
            </div>
            <div className="alteracao-motivo">"{alteracao.motivo}"</div>
            <div className="alteracao-meta">
              Solicitado por {alteracao.solicitante} em {new Date(alteracao.data).toLocaleDateString('pt-BR')} ·{' '}
              <strong>{alteracao.status}</strong>
            </div>
          </div>
          {/* "Ver histórico" e "Cancelar solicitação" saíram: não abriam nada, e
              cancelar uma solicitação de alteração cadastral é escrita — precisa
              da API e de confirmação, que não existem nesta ficha. */}
        </div>
      )}

      <AbasFicha abas={abas} ativa={abaAtiva} aoSelecionar={setAbaAtiva} />
    </>
  );
}

function AbaVisaoGeral({ c }: { c: ClienteFichaData }) {
  return (
    <div className="ficha-grid">
      <div className="ficha-col-main">
        <BlocoFicha id="identificacao" titulo="Identificação">
          <div className="ficha-fields">
            <div className="ff">
              <label>Razão social</label>
              <span>{c.razao_social}</span>
            </div>
            <div className="ff">
              <label>Nome fantasia</label>
              <span>{c.nome_fantasia}</span>
            </div>
            <div className="ff">
              <label>CNPJ</label>
              <span className="mono">{c.cnpj}</span>
            </div>
            <div className="ff">
              <label>Inscrição estadual</label>
              <span className="mono">
                {c.ie} <span className="muted">({c.ie_uf})</span>
              </span>
            </div>
            <div className="ff">
              <label>Data de fundação</label>
              <span>{new Date(c.data_fundacao).toLocaleDateString('pt-BR')}</span>
            </div>
            <div className="ff">
              <label>Tipo</label>
              <span>Pessoa Jurídica</span>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="endereco" titulo="Endereço principal">
          <div className="ficha-endereco">
            <div className="ff-full">
              <label>Logradouro</label>
              <span>
                {c.endereco_principal.logradouro}, {c.endereco_principal.numero} · {c.endereco_principal.complemento}
              </span>
            </div>
            <div className="ficha-fields">
              <div className="ff">
                <label>Bairro</label>
                <span>{c.endereco_principal.bairro}</span>
              </div>
              <div className="ff">
                <label>CEP</label>
                <span className="mono">{c.endereco_principal.cep}</span>
              </div>
              <div className="ff">
                <label>Cidade / UF</label>
                <span>
                  {c.endereco_principal.cidade} / {c.endereco_principal.uf}
                </span>
              </div>
              <div className="ff">
                <label>País</label>
                <span>{c.endereco_principal.pais}</span>
              </div>
              <div className="ff">
                <label>Coordenadas</label>
                <span className="mono">
                  {c.endereco_principal.latitude}, {c.endereco_principal.longitude}
                </span>
              </div>
              <div className="ff">
                <label>Rota comercial</label>
                <span>{c.segmentacao.rota}</span>
              </div>
            </div>
            <div className="ficha-map-placeholder">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.5}>
                <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
                <circle cx="12" cy="10" r="3" />
              </svg>
              <span>
                Abrir no mapa · {c.endereco_principal.cidade}/{c.endereco_principal.uf}
              </span>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="enderecos-adicionais" titulo="Endereços adicionais · Fazendas" espacoso>
          <div className="fazendas-list">
            {c.enderecos_adicionais.map((e, i) => (
              <div className="fazenda-row" key={i}>
                <div className="fazenda-icone">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                    <path d="M3 21h18M5 21V7l7-4 7 4v14M9 9h6M9 13h6M9 17h6" />
                  </svg>
                </div>
                <div className="fazenda-body">
                  <div className="fazenda-nome">{e.tipo}</div>
                  <div className="fazenda-sub">
                    {e.cidade}
                    {e.hectares > 0 ? ` · ${fmtNum(e.hectares)} ha` : ''} · {e.cultura}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </BlocoFicha>

        <BlocoFicha id="contatos" titulo="Contatos" espacoso>
          <div className="contatos-list">
            {c.contatos.map((ct, i) => (
              <div className="contato-card" key={i}>
                <div className="contato-avatar">{iniciaisContato(ct.nome)}</div>
                <div className="contato-body">
                  <div className="contato-linha1">
                    <strong>{ct.nome}</strong>
                    {ct.decisor && <span className="badge-decisor">★ Decisor</span>}
                  </div>
                  <div className="contato-cargo">{ct.cargo}</div>
                  <div className="contato-canais">
                    <span className="contato-canal">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                        <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z" />
                      </svg>
                      {ct.celular}
                    </span>
                    <span className="contato-canal">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
                        <polyline points="22,6 12,13 2,6" />
                      </svg>
                      {ct.email}
                    </span>
                    {ct.aniversario && (
                      <span className="contato-aniv">
                        🎂 {new Date(ct.aniversario).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </BlocoFicha>
      </div>

      <div className="ficha-col-side">
        <BlocoFicha id="carteira" titulo="Vínculo à carteira">
          <div className="carteira-info">
            <div className="carteira-cen">
              <div className="cen-avatar">{c.cen_dono.avatar}</div>
              <div>
                <div className="cen-nome">{c.cen_dono.nome}</div>
                <div className="cen-role">CEN Comercial · {c.regional}</div>
              </div>
            </div>
            <div className="carteira-cen">
              <div className="cen-avatar cen-avatar-secondary">{c.cen_pos_venda.avatar}</div>
              <div>
                <div className="cen-nome">{c.cen_pos_venda.nome}</div>
                <div className="cen-role">Pós-venda / Assistência</div>
              </div>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="segmentacao" titulo="Segmentação comercial">
          <div className="ficha-fields ficha-fields-col">
            <div className="ff">
              <label>Classe ABC</label>
              <span>
                <strong style={{ color: 'var(--jd-green)' }}>{c.classe} · Alto valor</strong>
              </span>
            </div>
            <div className="ff">
              <label>Grupo</label>
              <span>{c.segmentacao.grupo}</span>
            </div>
            <div className="ff">
              <label>Atividade</label>
              <span>{c.segmentacao.atividade}</span>
            </div>
            <div className="ff">
              <label>Porte</label>
              <span>{c.segmentacao.porte}</span>
            </div>
            <div className="ff">
              <label>Faturamento declarado</label>
              <span>{c.segmentacao.faturamento_declarado}</span>
            </div>
            <div className="ff">
              <label>Área plantada</label>
              <span>{fmtNum(c.segmentacao.hectares_totais)} ha</span>
            </div>
            <div className="ff">
              <label>Culturas</label>
              <div className="cultura-tags">
                {c.segmentacao.culturas.map((cu) => (
                  <span className="cultura-tag" key={cu}>
                    {cu}
                  </span>
                ))}
              </div>
            </div>
          </div>
        </BlocoFicha>

        <BlocoFicha id="preferencias" titulo="Preferências de contato · LGPD">
          <div className="prefs-list">
            {Object.entries(c.preferencias_contato).map(([canal, ativo]) => (
              <label className={ativo ? 'pref-item ativo' : 'pref-item inativo'} key={canal}>
                <span className="pref-check">{ativo ? '✓' : '✗'}</span>
                <span className="pref-label">{canal.charAt(0).toUpperCase() + canal.slice(1)}</span>
              </label>
            ))}
          </div>
          <div className="lgpd-nota">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
            </svg>
            Consentimento LGPD ativo desde {new Date(c.consentimento_lgpd.data).toLocaleDateString('pt-BR')} ·{' '}
            {c.consentimento_lgpd.versao}
          </div>
        </BlocoFicha>

        <BlocoFicha id="auditoria" titulo="Auditoria">
          <div className="audit-list">
            <div className="audit-row">
              <span className="audit-label">Origem inclusão</span>
              <span className="audit-value mono">{c.origem_inclusao}</span>
            </div>
            <div className="audit-row">
              <span className="audit-label">Última alteração</span>
              <span className="audit-value mono">{c.origem_alteracao}</span>
            </div>
            <div className="audit-row">
              <span className="audit-label">ID Vortice</span>
              <span className="audit-value mono">#{c.id}</span>
            </div>
          </div>
          {/* "Solicitar alteração cadastral" saiu: não existe formulário de
              solicitação. O cadastro que o CRM já sabe alterar é o da tela
              Clientes, ligada à API — o caminho está no documento 08. */}
          <div className="ficha-nota-leitura">
            Alterar cadastro é pela tela <strong>Clientes</strong>, que grava na API. Esta ficha é leitura.
          </div>
        </BlocoFicha>
      </div>
    </div>
  );
}

function AbaFrota({ c }: { c: ClienteFichaData }) {
  const linhas = ['Tratores', 'Colheitadeiras', 'Plantadeiras', 'Pulverizadores'];
  return (
    <>
      <div className="frota-summary">
        {linhas.map((linha) => {
          const itens = c.frota.filter((f) => f.linha === linha);
          if (itens.length === 0) return null;
          return (
            <div className="frota-summary-card" key={linha}>
              <div className="frota-linha-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
                  <rect x="3" y="8" width="18" height="10" rx="2" />
                  <circle cx="8" cy="18" r="3" />
                  <circle cx="17" cy="18" r="3" />
                </svg>
              </div>
              <div>
                <div className="frota-linha-count">{itens.length}</div>
                <div className="frota-linha-label">{linha}</div>
              </div>
            </div>
          );
        })}
      </div>
      <div className="card" style={{ marginTop: 16 }}>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Chassi</th>
                <th>Modelo</th>
                <th>Linha</th>
                <th className="num">Ano</th>
                <th className="num">Horas</th>
                <th>Garantia</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {c.frota.map((f) => (
                <tr key={f.chassi}>
                  <td className="mono">{f.chassi}</td>
                  <td>
                    <strong>{f.modelo}</strong>
                  </td>
                  <td>{f.linha}</td>
                  <td className="num">{f.ano}</td>
                  <td className="num">{fmtNum(f.horas)}</td>
                  <td>
                    {f.garantia.startsWith('Ativa') ? (
                      <span className="badge badge-success">{f.garantia}</span>
                    ) : (
                      <span className="badge badge-muted">{f.garantia}</span>
                    )}
                  </td>
                  <td>
                    {f.status === 'Manutenção agendada' ? (
                      <span className="badge badge-warning">{f.status}</span>
                    ) : (
                      <span className="badge badge-info">{f.status}</span>
                    )}
                  </td>
                  <td>
                    {/* A seta só aparece no chassi que tem ficha. Antes ela
                        aparecia em todos e, nos outros, cancelava a navegação
                        para pedir desculpa por `alert` — clique gasto para
                        descobrir que não havia para onde ir. */}
                    {f.chassi === CHASSI_COM_FICHA && (
                      <Link
                        to={`/equipamentos/${f.chassi}`}
                        className="btn-icon btn-icon-inline"
                        title="Ver ficha do equipamento"
                        style={{ textDecoration: 'none', display: 'inline-flex', alignItems: 'center', justifyContent: 'center' }}
                      >
                        →
                      </Link>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}

function AbaOportunidades({
  c,
  novas,
  fases,
  navegar,
}: {
  c: ClienteFichaData;
  novas: OportunidadeNova[];
  fases: FasePipeline[] | null;
  navegar: (caminho: string) => void;
}) {
  return (
    <div className="oport-list">
      {novas.map((op) => {
        const fase = fases?.find((f) => f.id === op.fase);
        return (
          <div key={op.id} className="oport-card oport-nova">
            <div className="oport-body">
              <div className="oport-linha1">
                <strong>{op.titulo}</strong>
                <span className="badge badge-info">{op.linha}</span>
                <span className="oport-id mono">{op.id}</span>
                <span className="badge-nova-inline">NOVA</span>
              </div>
              <div className="oport-meta">
                <span>CEN: João Ribeiro</span>
                <span>Previsão: {new Date(op.previsao).toLocaleDateString('pt-BR')}</span>
                <span>Modelo: {op.modelo}</span>
              </div>
            </div>
            <div className="oport-fase">
              <div className="oport-fase-label">{fase?.label ?? op.fase}</div>
              <div className="oport-prob">{op.probabilidade}% probabilidade</div>
              <div className="oport-prob-bar">
                <div
                  className="oport-prob-fill"
                  style={{
                    width: `${op.probabilidade}%`,
                    background: op.probabilidade >= 60 ? 'var(--jd-green)' : op.probabilidade >= 30 ? '#F59E0B' : '#94A3B8',
                  }}
                />
              </div>
            </div>
            <div className="oport-valor">
              <div className="oport-valor-label">Valor</div>
              <div className="oport-valor-num">{fmtBRL(op.valor)}</div>
              <div className="oport-valor-ponderado">Ponderado: {fmtBRL((op.valor * op.probabilidade) / 100)}</div>
            </div>
          </div>
        );
      })}
      {c.oportunidades.map((o) => (
        <div
          key={o.id}
          className="oport-card"
          style={o.id === OPORTUNIDADE_COM_FICHA ? { cursor: 'pointer' } : undefined}
          onClick={o.id === OPORTUNIDADE_COM_FICHA ? () => navegar(`/oportunidades/${OPORTUNIDADE_COM_FICHA}`) : undefined}
        >
          <div className="oport-body">
            <div className="oport-linha1">
              <strong>{o.titulo}</strong>
              <span className="badge badge-info">{o.linha}</span>
              <span className="oport-id">#{o.id}</span>
            </div>
            <div className="oport-meta">
              <span>CEN: {o.cen}</span>
              <span>Previsão: {new Date(o.previsao).toLocaleDateString('pt-BR')}</span>
            </div>
          </div>
          <div className="oport-fase">
            <div className="oport-fase-label">{o.fase}</div>
            <div className="oport-prob">{o.prob}% probabilidade</div>
            <div className="oport-prob-bar">
              <div
                className="oport-prob-fill"
                style={{ width: `${o.prob}%`, background: o.prob >= 60 ? 'var(--jd-green)' : o.prob >= 30 ? '#F59E0B' : '#94A3B8' }}
              />
            </div>
          </div>
          <div className="oport-valor">
            <div className="oport-valor-label">Valor</div>
            <div className="oport-valor-num">{fmtBRL(o.valor)}</div>
            <div className="oport-valor-ponderado">Ponderado: {fmtBRL((o.valor * o.prob) / 100)}</div>
          </div>
        </div>
      ))}
    </div>
  );
}

function AbaHistorico({ c }: { c: ClienteFichaData }) {
  return (
    <div className="timeline">
      {c.interacoes.map((i, idx) => {
        const meta = META_INTERACAO[i.tipo] ?? { icon: '●', color: '#64748B', label: i.tipo };
        return (
          <div className="timeline-item" key={idx}>
            <div className="timeline-marker" style={{ background: meta.color }}>
              {meta.icon}
            </div>
            <div className="timeline-body">
              <div className="timeline-linha1">
                <strong>{i.assunto}</strong>
                <span className="timeline-tipo" style={{ color: meta.color }}>
                  {meta.label}
                </span>
              </div>
              <div className="timeline-meta">
                {fmtDataHora(i.data)} · {i.autor} · <strong>{i.resultado}</strong>
              </div>
              {i.obs && <div className="timeline-obs">{i.obs}</div>}
            </div>
          </div>
        );
      })}
    </div>
  );
}

function AbaFinanceiro({ c }: { c: ClienteFichaData }) {
  const f = c.financeiro;
  const pctUsado = (f.limite_usado / f.limite_credito) * 100;
  return (
    <>
      <div className="kpi-grid" style={{ gridTemplateColumns: 'repeat(4,1fr)', marginBottom: 20 }}>
        <div className="kpi">
          <span className="kpi-label">Faturamento 2024</span>
          <span className="kpi-value">{fmtBRL(f.faturamento_2024)}</span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Faturamento 2025</span>
          <span className="kpi-value">{fmtBRL(f.faturamento_2025)}</span>
        </div>
        <div className="kpi kpi-highlight">
          <span className="kpi-label">Faturamento YTD 2026</span>
          <span className="kpi-value">{fmtBRL(f.faturamento_ytd_2026)}</span>
          <span className="kpi-hint">Projeção anual: {fmtBRL((f.faturamento_ytd_2026 * 12) / 8)}</span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Dias em atraso · máx.</span>
          <span className="kpi-value" style={{ color: 'var(--jd-green)' }}>
            {f.dias_atraso_max}
          </span>
          <span className="kpi-hint">Zero inadimplência histórica</span>
        </div>
      </div>
      <div className="card">
        <div className="card-header">
          <div>
            <div className="card-title">Limite de crédito</div>
            <div className="card-subtitle">Utilização do limite aprovado</div>
          </div>
        </div>
        <div style={{ padding: 24 }}>
          <div className="credit-bar-wrap">
            <div className="credit-bar-track">
              <div className="credit-bar-used" style={{ width: `${pctUsado.toFixed(1)}%` }} />
            </div>
            <div className="credit-bar-legend">
              <span>
                <strong>{fmtBRL(f.limite_usado)}</strong> utilizados
              </span>
              <span>
                <strong>{fmtBRL(f.limite_disponivel)}</strong> disponíveis
              </span>
              <span>
                Limite total: <strong>{fmtBRL(f.limite_credito)}</strong>
              </span>
            </div>
          </div>
          <div className="funil-scope-note" style={{ marginTop: 20 }}>
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2}>
              <path d="M12 9v2m0 4h.01" />
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
            </svg>
            <span>
              Dados financeiros vêm do Protheus (SA1 + SE1) por sync noturno. Última atualização: 25/08/2026 03:14. Alterações de
              limite requerem workflow de aprovação.
            </span>
          </div>
        </div>
      </div>
    </>
  );
}
