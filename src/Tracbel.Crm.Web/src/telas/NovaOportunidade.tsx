/**
 * Nova Oportunidade — porte de `renderNovaOportunidade`/`mountNovaOportunidade`
 * (`prototipo/referencia/assets/app.js`, linhas 6655/6858 até 7195). Fluxo
 * funcional: cadastra oportunidade que aparece em Pipeline, Ficha Cliente,
 * Funil e Performance (via `dados/persistencia.ts`, já compartilhado com essas
 * telas). Estado vive só na sessão (some no F5 se o `localStorage` não
 * persistir) — mesmo aviso do protótipo.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — o passo 3 fechou a maior ponta solta desta tela.
 *
 * **"Cancelar" jogava fora tudo o que tinha sido digitado, sem perguntar.** Um
 * clique em cima do botão de salvar — eles são vizinhos — e o formulário
 * inteiro ia embora, num cadastro que tem três seções e nove campos. É
 * exatamente o defeito que o painel de andamento da Agenda tinha ("um link que
 * fechava a tela levando o que o usuário escreveu") e que já foi corrigido lá.
 * Agora Cancelar só sai direto quando não há nada preenchido; com alteração
 * pendente, pede confirmação, e a aba não fecha em silêncio (`beforeunload`).
 * É a regra do padrão de tela §3.4: nada do que foi digitado se perde.
 *
 * Dois outros consertos:
 *
 * - **O erro de leitura era inalcançável.** O `if` de carregando vinha antes e
 *   testava `!carteira`, então qualquer falha ficava eternamente com cara de
 *   "Carregando…". O erro passou a vir primeiro, com o botão de tentar de novo.
 * - **O quadro "Impacto ao salvar" mentia.** Dizia "Coluna Qualificação" e
 *   "Fase Qualificação" fixos na marcação, mesmo com Diagnóstico ou Proposta
 *   escolhidos como fase inicial. Agora ele diz a fase que a pessoa escolheu.
 */
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { DialogoConfirmacao } from '../componentes/cadastro/DialogoConfirmacao';
import { ModalLimparSessao } from '../componentes/ModalLimparSessao';
import { ToastNovaOportunidade } from '../componentes/ToastNovaOportunidade';
import { formatarBRLCompacto } from '../dados/formatadores';
import {
  criarOportunidadeNova,
  limparOportunidadesPersistidas,
  obterOportunidadesNovas,
  storageDisponivel,
} from '../dados/persistencia';
import { useDados } from '../dados/useDados';
import type { CatalogoModelos, ClienteCarteira, FasePipeline, NovaOportunidadeEstado, OportunidadeNova } from '../tipos/oportunidade';

const PROBS_DEFAULT: Record<string, number> = { qualificacao: 15, diagnostico: 30, proposta: 55 };

/**
 * Previsão de fechamento padrão (60 dias após a data-base fixa do protótipo)
 * — porte de `resetNovaOpState` (app.js linha 6635). Mesma data-base fixa de
 * `HOJE_CARTEIRA`/`AGENDA_HOJE` (25/ago/2026); nunca `new Date()` real, senão
 * o valor muda a cada dia em que a tela é aberta.
 */
function calcularPrevisaoFechamentoPadrao(): string {
  const dt = new Date(2026, 7, 25);
  dt.setDate(dt.getDate() + 60);
  return dt.toISOString().slice(0, 10);
}

function estadoInicial(): NovaOportunidadeEstado {
  return {
    cliente_id: null,
    cliente_nome: '',
    cliente_classe: 'B',
    cliente_cidade: '',
    cliente_novo: false,
    linha: '',
    modelo: '',
    quantidade: 1,
    valor_unitario: 0,
    fase_inicial: 'qualificacao',
    previsao_fechamento: calcularPrevisaoFechamentoPadrao(),
    probabilidade: 15,
    titulo: '',
    observacao: '',
  };
}

export function NovaOportunidade() {
  const navegar = useNavigate();
  const leituraCarteira = useDados<ClienteCarteira[]>('carteira-cen');
  const leituraCatalogo = useDados<CatalogoModelos>('catalogo-modelos');
  const leituraIcones = useDados<Record<string, string>>('linha-icon');
  const leituraFases = useDados<FasePipeline[]>('pipeline-fases');
  const { dados: carteira, carregando: carregandoCarteira, erro: erroCarteira } = leituraCarteira;
  const { dados: catalogo, carregando: carregandoCatalogo, erro: erroCatalogo } = leituraCatalogo;
  const { dados: linhaIcones, carregando: carregandoIcones, erro: erroIcones } = leituraIcones;
  const { dados: fases, carregando: carregandoFases, erro: erroFases } = leituraFases;

  function recarregarTudo() {
    leituraCarteira.recarregar();
    leituraCatalogo.recarregar();
    leituraIcones.recarregar();
    leituraFases.recarregar();
  }

  const [estado, setEstado] = useState<NovaOportunidadeEstado>(estadoInicial);
  const [valorTexto, setValorTexto] = useState('');
  const [sessao, setSessao] = useState<OportunidadeNova[]>(() => obterOportunidadesNovas());
  const [modalLimparAberto, setModalLimparAberto] = useState(false);
  const [toastLimpoAtivo, setToastLimpoAtivo] = useState(false);
  const [toastLimpoVisivel, setToastLimpoVisivel] = useState(false);
  const [oportunidadeSalva, setOportunidadeSalva] = useState<OportunidadeNova | null>(null);
  const [confirmandoSaida, setConfirmandoSaida] = useState(false);

  /**
   * Há alteração pendente?
   *
   * Compara com o estado inicial em vez de guardar um sinalizador de "mexeu":
   * quem digita e apaga não fica preso num aviso de descarte por causa de um
   * campo que voltou a estar vazio.
   */
  const inicial = estadoInicial();
  const temAlteracaoPendente =
    estado.cliente_nome.trim() !== '' ||
    estado.cliente_cidade.trim() !== '' ||
    estado.titulo.trim() !== '' ||
    estado.observacao.trim() !== '' ||
    estado.linha !== '' ||
    estado.modelo !== '' ||
    estado.valor_unitario > 0 ||
    estado.quantidade !== inicial.quantidade ||
    estado.fase_inicial !== inicial.fase_inicial ||
    estado.previsao_fechamento !== inicial.previsao_fechamento;

  /**
   * Fechar a aba com alteração pendente pede confirmação do navegador.
   *
   * Só enquanto há o que perder — um `beforeunload` permanente atrapalha quem
   * abriu a tela e desistiu antes de digitar. Some assim que a oportunidade é
   * salva, senão o redirecionamento para o Pipeline dispararia o aviso.
   */
  useEffect(() => {
    if (!temAlteracaoPendente || oportunidadeSalva) return;
    function aoSair(e: BeforeUnloadEvent) {
      e.preventDefault();
      e.returnValue = '';
    }
    window.addEventListener('beforeunload', aoSair);
    return () => window.removeEventListener('beforeunload', aoSair);
  }, [temAlteracaoPendente, oportunidadeSalva]);

  // Redireciona para o Pipeline 1.5s após salvar — mesmo tempo do protótipo.
  useEffect(() => {
    if (!oportunidadeSalva) return;
    const id = setTimeout(() => navegar('/pipeline'), 1500);
    return () => clearTimeout(id);
  }, [oportunidadeSalva, navegar]);

  // Toast "Sessão limpa" (1.2s) e depois reseta o formulário — equivalente ao
  // `router()` que o protótipo chama para re-renderizar a tela do zero.
  useEffect(() => {
    if (!toastLimpoAtivo) return;
    const t1 = setTimeout(() => setToastLimpoVisivel(true), 50);
    const t2 = setTimeout(() => {
      setToastLimpoAtivo(false);
      setToastLimpoVisivel(false);
      setEstado(estadoInicial());
      setValorTexto('');
    }, 1200);
    return () => {
      clearTimeout(t1);
      clearTimeout(t2);
    };
  }, [toastLimpoAtivo]);

  // O ERRO VEM ANTES DO CARREGANDO: com a ordem invertida, `!carteira` era
  // verdadeiro nos dois casos e a falha nunca chegava a aparecer.
  const erro = erroCarteira ?? erroCatalogo ?? erroIcones ?? erroFases;
  if (erro) {
    return <BlocoErro erro={erro} aoTentarDeNovo={recarregarTudo} />;
  }
  if (carregandoCarteira || carregandoCatalogo || carregandoIcones || carregandoFases || !carteira || !catalogo || !linhaIcones || !fases) {
    return <BlocoCarregando oQue="a carteira e o catálogo de modelos" />;
  }

  const clienteSelecionado = estado.cliente_id !== null ? carteira.find((c) => c.id === estado.cliente_id) : undefined;
  const modelosDaLinha = estado.linha ? catalogo[estado.linha] ?? [] : [];
  const total = estado.quantidade * estado.valor_unitario;
  const valido =
    Boolean(estado.cliente_nome.trim()) &&
    Boolean(estado.linha) &&
    Boolean(estado.modelo) &&
    estado.valor_unitario > 0 &&
    Boolean(estado.titulo.trim());

  function aoMudarModo(novoModo: boolean) {
    setEstado((s) => ({ ...s, cliente_novo: novoModo, cliente_id: null, cliente_nome: '' }));
  }

  function aoMudarClienteCarteira(valor: string) {
    if (!valor) {
      setEstado((s) => ({ ...s, cliente_id: null, cliente_nome: '' }));
      return;
    }
    const c = carteira!.find((x) => x.id === parseInt(valor, 10));
    if (!c) return;
    setEstado((s) => ({ ...s, cliente_id: c.id, cliente_nome: c.razao, cliente_classe: c.classe, cliente_cidade: c.cidade }));
  }

  function aoMudarRazao(valor: string) {
    setEstado((s) => {
      const cliente_id = valor.trim() && !s.cliente_id ? 99000 + Math.floor(Math.random() * 999) : s.cliente_id;
      return { ...s, cliente_nome: valor, cliente_id };
    });
  }

  function aoMudarLinha(valor: string) {
    setEstado((s) => ({ ...s, linha: valor, modelo: '' }));
  }

  function aoMudarModelo(valor: string) {
    const encontrado = modelosDaLinha.find((m) => m.modelo === valor);
    setEstado((s) => ({ ...s, modelo: valor, valor_unitario: encontrado ? encontrado.valor_ref : s.valor_unitario }));
    if (encontrado) setValorTexto(String(encontrado.valor_ref));
  }

  function aoMudarValorTexto(valor: string) {
    setValorTexto(valor);
    setEstado((s) => ({ ...s, valor_unitario: parseInt(valor, 10) || 0 }));
  }

  function aoMudarFase(valor: string) {
    setEstado((s) => ({ ...s, fase_inicial: valor, probabilidade: PROBS_DEFAULT[valor] ?? s.probabilidade }));
  }

  function aoSalvar() {
    if (!valido) return;
    const nova = criarOportunidadeNova({
      ...estado,
      cliente_nome: estado.cliente_nome.trim(),
      cliente_cidade: estado.cliente_cidade.trim(),
      titulo: estado.titulo.trim(),
    });
    setSessao(obterOportunidadesNovas());
    setOportunidadeSalva(nova);
  }

  function aoConfirmarLimpar() {
    limparOportunidadesPersistidas();
    setSessao([]);
    setModalLimparAberto(false);
    setToastLimpoAtivo(true);
  }

  const faseLabel = oportunidadeSalva ? fases.find((f) => f.id === oportunidadeSalva.fase)?.label ?? oportunidadeSalva.fase : '';

  return (
    <>
      <div className="novaop-header">
        <div>
          <div className="novaop-title">Nova Oportunidade</div>
          <div className="novaop-subtitle">Cadastro do zero · aparece em Pipeline, Cliente, Funil e Performance imediatamente</div>
          {sessao.length > 0 && (
            <div className={`novaop-persist-hint${storageDisponivel() ? '' : ' sem-persist'}`}>
              <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" strokeWidth={2.2}>
                <path d="M20 6L9 17l-5-5" />
              </svg>
              <span>{`${sessao.length} oportunidade${sessao.length > 1 ? 's' : ''} nesta sessão · ${storageDisponivel() ? 'salvas no navegador (resistem a F5)' : 'apenas em memória (F5 apaga)'}`}</span>
              <button className="btn-limpar-sessao" onClick={() => setModalLimparAberto(true)}>Limpar</button>
            </div>
          )}
        </div>
        <div className="novaop-cen-badge">
          <div className="novaop-cen-avatar" style={{ background: '#367C2B' }}>JR</div>
          <div>
            <div className="novaop-cen-nome">João Ribeiro</div>
            <div className="novaop-cen-role">CEN atribuído · MT Norte</div>
          </div>
        </div>
      </div>

      <div className="novaop-grid">
        {/* SEÇÃO 1: CLIENTE */}
        <div className="novaop-section">
          <div className="novaop-section-num">1</div>
          <div className="novaop-section-body">
            <div className="novaop-section-title">Cliente</div>
            <div className="novaop-section-hint">Escolha da carteira ou informe um prospect novo</div>

            <div className="novaop-modo-cliente">
              <label className={`novaop-modo-item${!estado.cliente_novo ? ' active' : ''}`}>
                <input type="radio" name="modoCliente" value="carteira" checked={!estado.cliente_novo} onChange={() => aoMudarModo(false)} />
                <span>Da carteira</span>
              </label>
              <label className={`novaop-modo-item${estado.cliente_novo ? ' active' : ''}`}>
                <input type="radio" name="modoCliente" value="novo" checked={estado.cliente_novo} onChange={() => aoMudarModo(true)} />
                <span>Prospect novo</span>
              </label>
            </div>

            <div id="novaop-cliente-carteira" className={`novaop-cliente-box${estado.cliente_novo ? ' oculto' : ''}`}>
              <label className="novaop-field">
                <span className="novaop-label">Cliente da carteira</span>
                <select id="novaopCliente" className="novaop-input" value={estado.cliente_id ?? ''} onChange={(e) => aoMudarClienteCarteira(e.target.value)}>
                  <option value="">Selecione...</option>
                  {carteira.map((c) => (
                    <option key={c.id} value={c.id}>{`${c.razao} · Classe ${c.classe} · ${c.cidade}`}</option>
                  ))}
                </select>
              </label>
              <div id="novaopClienteInfo" className={`novaop-cliente-info${clienteSelecionado ? '' : ' oculto'}`}>
                {clienteSelecionado && (
                  <>
                    <div className="nci-item">
                      <span className="nci-label">Classe</span>
                      <span className={`nci-value classe-${clienteSelecionado.classe.toLowerCase()}`}>{clienteSelecionado.classe}</span>
                    </div>
                    <div className="nci-item">
                      <span className="nci-label">Faturamento 12m</span>
                      <span className="nci-value">{formatarBRLCompacto(clienteSelecionado.fat_12m)}</span>
                    </div>
                    <div className="nci-item">
                      <span className="nci-label">Oportunidades ativas</span>
                      <span className="nci-value">
                        {clienteSelecionado.oportunidades} ·{' '}
                        {clienteSelecionado.oportunidades > 0 && clienteSelecionado.oportunidades_valor != null
                          ? formatarBRLCompacto(clienteSelecionado.oportunidades_valor)
                          : '—'}
                      </span>
                    </div>
                    <div className="nci-item">
                      <span className="nci-label">Última interação</span>
                      <span className="nci-value">
                        {clienteSelecionado.ult_int ? `${clienteSelecionado.ult_int.data} · ${clienteSelecionado.ult_int.cat}` : 'nunca'}
                      </span>
                    </div>
                  </>
                )}
              </div>
            </div>

            <div id="novaop-cliente-novo" className={`novaop-cliente-box${estado.cliente_novo ? '' : ' oculto'}`}>
              <label className="novaop-field">
                <span className="novaop-label">Razão social</span>
                <input
                  id="novaopNovoRazao"
                  className="novaop-input"
                  placeholder="Ex: Fazenda São João Agropecuária Ltda"
                  value={estado.cliente_nome}
                  onChange={(e) => aoMudarRazao(e.target.value)}
                />
              </label>
              <div className="novaop-row-2">
                <label className="novaop-field">
                  <span className="novaop-label">Classe</span>
                  <select
                    id="novaopNovoClasse"
                    className="novaop-input"
                    value={estado.cliente_classe}
                    onChange={(e) => setEstado((s) => ({ ...s, cliente_classe: e.target.value }))}
                  >
                    <option value="A">{'A · Faturamento > R$ 3M/ano'}</option>
                    <option value="B">B · R$ 500k - 3M/ano</option>
                    <option value="C">C · R$ 100k - 500k/ano</option>
                    <option value="D">{'D · Prospect ou < R$ 100k'}</option>
                  </select>
                </label>
                <label className="novaop-field">
                  <span className="novaop-label">Cidade / UF</span>
                  <input
                    id="novaopNovaCidade"
                    className="novaop-input"
                    placeholder="Ex: Sinop / MT"
                    value={estado.cliente_cidade}
                    onChange={(e) => setEstado((s) => ({ ...s, cliente_cidade: e.target.value }))}
                  />
                </label>
              </div>
              <div className="novaop-alerta">
                <strong>Atenção:</strong> prospect novo será criado sem passar por Protheus. Em produção, cadastro real exige aprovação do Admin Comercial.
              </div>
            </div>
          </div>
        </div>

        {/* SEÇÃO 2: EQUIPAMENTO */}
        <div className="novaop-section">
          <div className="novaop-section-num">2</div>
          <div className="novaop-section-body">
            <div className="novaop-section-title">Equipamento e valor</div>
            <div className="novaop-section-hint">Selecione a linha e o modelo. Valor referência do catálogo, ajustável.</div>

            <div className="novaop-row-2">
              <label className="novaop-field">
                <span className="novaop-label">Linha</span>
                <select id="novaopLinha" className="novaop-input" value={estado.linha} onChange={(e) => aoMudarLinha(e.target.value)}>
                  <option value="">Selecione a linha...</option>
                  {Object.keys(catalogo).map((l) => (
                    <option key={l} value={l}>{`${linhaIcones[l] ?? ''} ${l}`}</option>
                  ))}
                </select>
              </label>
              <label className="novaop-field">
                <span className="novaop-label">Modelo</span>
                <select
                  id="novaopModelo"
                  className="novaop-input"
                  disabled={!estado.linha}
                  value={estado.modelo}
                  onChange={(e) => aoMudarModelo(e.target.value)}
                >
                  {estado.linha ? (
                    <>
                      <option value="">Selecione o modelo...</option>
                      {modelosDaLinha.map((m) => (
                        <option key={m.modelo} value={m.modelo}>{`${m.modelo} · ref. R$ ${(m.valor_ref / 1000).toFixed(0)}k`}</option>
                      ))}
                    </>
                  ) : (
                    <option value="">Selecione a linha primeiro</option>
                  )}
                </select>
              </label>
            </div>

            <div className="novaop-row-3">
              <label className="novaop-field">
                <span className="novaop-label">Quantidade</span>
                <input
                  type="number"
                  id="novaopQtde"
                  className="novaop-input"
                  min={1}
                  value={estado.quantidade}
                  onChange={(e) => setEstado((s) => ({ ...s, quantidade: Math.max(1, parseInt(e.target.value, 10) || 1) }))}
                />
              </label>
              <label className="novaop-field">
                <span className="novaop-label">Valor unitário (R$)</span>
                <input
                  type="number"
                  id="novaopValor"
                  className="novaop-input"
                  placeholder="0"
                  step={1000}
                  value={valorTexto}
                  onChange={(e) => aoMudarValorTexto(e.target.value)}
                />
              </label>
              <label className="novaop-field">
                <span className="novaop-label">Valor total</span>
                <div className="novaop-valor-total" id="novaopValorTotal">{formatarBRLCompacto(total)}</div>
              </label>
            </div>
          </div>
        </div>

        {/* SEÇÃO 3: DETALHES E CONFIRMAÇÃO */}
        <div className="novaop-section">
          <div className="novaop-section-num">3</div>
          <div className="novaop-section-body">
            <div className="novaop-section-title">Detalhes da oportunidade</div>
            <div className="novaop-section-hint">Fase inicial, previsão e título. Nasce em Qualificação por padrão.</div>

            <label className="novaop-field">
              <span className="novaop-label">Título</span>
              <input
                id="novaopTitulo"
                className="novaop-input"
                placeholder="Ex: Renovação frota tratores compactos"
                value={estado.titulo}
                onChange={(e) => setEstado((s) => ({ ...s, titulo: e.target.value }))}
              />
            </label>

            <div className="novaop-row-3">
              <label className="novaop-field">
                <span className="novaop-label">Fase inicial</span>
                <select id="novaopFase" className="novaop-input" value={estado.fase_inicial} onChange={(e) => aoMudarFase(e.target.value)}>
                  <option value="qualificacao">Qualificação (15%)</option>
                  <option value="diagnostico">Diagnóstico (30%)</option>
                  <option value="proposta">Proposta (55%)</option>
                </select>
              </label>
              <label className="novaop-field">
                <span className="novaop-label">Previsão fechamento</span>
                <input
                  type="date"
                  id="novaopPrevisao"
                  className="novaop-input"
                  value={estado.previsao_fechamento}
                  onChange={(e) => setEstado((s) => ({ ...s, previsao_fechamento: e.target.value }))}
                />
              </label>
              <label className="novaop-field">
                <span className="novaop-label">Probabilidade (%)</span>
                <input
                  type="number"
                  id="novaopProb"
                  className="novaop-input"
                  min={0}
                  max={100}
                  value={estado.probabilidade}
                  onChange={(e) =>
                    setEstado((s) => ({ ...s, probabilidade: Math.max(0, Math.min(100, parseInt(e.target.value, 10) || 0)) }))
                  }
                />
              </label>
            </div>

            <label className="novaop-field">
              <span className="novaop-label">Observação (opcional)</span>
              <textarea
                id="novaopObs"
                className="novaop-input"
                rows={2}
                placeholder="Notas relevantes sobre o cliente, contexto, próximas ações..."
                value={estado.observacao}
                onChange={(e) => setEstado((s) => ({ ...s, observacao: e.target.value }))}
              />
            </label>
          </div>
        </div>

        {/* PREVIEW DE IMPACTO */}
        <div className="novaop-preview" id="novaopPreview">
          <div className="novaop-preview-title">Impacto ao salvar esta oportunidade:</div>
          <div className="novaop-preview-grid" id="novaopPreviewGrid">
            <div className="novaop-preview-item">
              <div className="npi-label">Aparecerá em Pipeline</div>
              <div className="npi-value">
                Coluna {fases.find((f) => f.id === estado.fase_inicial)?.label ?? estado.fase_inicial} · MT Norte ·
                João Ribeiro
              </div>
            </div>
            <div className="novaop-preview-item">
              <div className="npi-label">Somará ao pipeline aberto do CEN</div>
              <div className="npi-value" id="npiPipeAberto">{`+ ${formatarBRLCompacto(total)}`}</div>
            </div>
            <div className="novaop-preview-item">
              <div className="npi-label">Contará no Funil de Vendas</div>
              <div className="npi-value">
                Fase {fases.find((f) => f.id === estado.fase_inicial)?.label ?? estado.fase_inicial}
              </div>
            </div>
            <div className="novaop-preview-item">
              <div className="npi-label">Aparecerá na Ficha do Cliente</div>
              <div className="npi-value" id="npiClienteFicha">{estado.cliente_nome || '— selecione um cliente —'}</div>
            </div>
          </div>
        </div>

        {/* BOTÕES */}
        <div className="novaop-actions">
          {/* Sem nada preenchido, sai direto. Com alteração pendente, pergunta:
              o botão fica encostado no de salvar, e um clique errado apagava
              nove campos sem aviso (padrão de tela §3.4). */}
          <button
            id="novaopCancelar"
            type="button"
            className="btn-cancelar"
            onClick={() => (temAlteracaoPendente ? setConfirmandoSaida(true) : navegar('/pipeline'))}
          >
            Cancelar
          </button>
          <button id="novaopSalvar" className="btn-salvar" disabled={!valido} onClick={aoSalvar}>
            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth={2.5}>
              <polyline points="20 6 9 17 4 12" />
            </svg>
            Salvar oportunidade
          </button>
        </div>
      </div>

      {oportunidadeSalva && <ToastNovaOportunidade oportunidade={oportunidadeSalva} faseLabel={faseLabel} />}

      {modalLimparAberto && (
        <ModalLimparSessao quantidade={sessao.length} onCancelar={() => setModalLimparAberto(false)} onConfirmar={aoConfirmarLimpar} />
      )}

      {confirmandoSaida && (
        <DialogoConfirmacao
          titulo="Descartar esta oportunidade?"
          subtitulo={estado.titulo.trim() || estado.cliente_nome.trim() || 'Oportunidade sem título'}
          rotuloConfirmar="Descartar e sair"
          aoCancelar={() => setConfirmandoSaida(false)}
          aoConfirmar={() => {
            setConfirmandoSaida(false);
            navegar('/pipeline');
          }}
        >
          <p>
            O que já foi preenchido <strong>não será salvo</strong> e a tela volta ao Pipeline. Nada do que está
            gravado é afetado — as oportunidades da sessão continuam onde estão.
          </p>
          <p>Para guardar esta oportunidade, feche este aviso e use <strong>Salvar oportunidade</strong>.</p>
        </DialogoConfirmacao>
      )}

      {toastLimpoAtivo && (
        <div className={`novaop-toast${toastLimpoVisivel ? ' show' : ''}`} style={{ borderLeftColor: '#B45309' }}>
          <div className="nt-icon" style={{ background: '#B45309' }}>
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth={2.5}>
              <polyline points="3 6 5 6 21 6" />
              <path d="M19 6l-2 14a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2L5 6" />
            </svg>
          </div>
          <div className="nt-body">
            <div className="nt-title">Sessão limpa</div>
            <div className="nt-sub">Oportunidades removidas</div>
            <div className="nt-hint">Recarregando a tela...</div>
          </div>
        </div>
      )}
    </>
  );
}
