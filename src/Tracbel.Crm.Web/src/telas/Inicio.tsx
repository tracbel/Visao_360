/**
 * Mapa do protótipo — porte de `renderHome()`
 * (`prototipo/referencia/assets/app.js`, linha 146). Rota `/inicio-antigo`.
 *
 * A lista de telas usa a tabela de rotas de `../rotas` (`ROTAS`/`acharRota`)
 * para caminho e título — apenas a descrição e o status de cada card, que não
 * existem em `ROTAS`, ficam aqui.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — ESTA TELA FICA, E FICA FORA DO MENU. A decisão está escrita em
 * `docs/prototipo/08-NAVEGACAO.md`; o resumo é este:
 *
 * Ela **não é tela de produto** — é o índice de desenvolvimento, feito para
 * abrir o protótipo num workshop e mostrar o que existe. Nenhum CEN, gerente ou
 * diretor tem trabalho a fazer aqui. Por isso continua sem entrada no menu
 * lateral, alcançável só por `#/inicio-antigo`, e ganhou no topo a marca de que
 * é ferramenta de desenvolvimento — para quem cair nela por um link antigo
 * saber, na primeira linha, que não é por ali que se trabalha.
 *
 * Apagá-la seria perder o único inventário navegável das quinze telas, que é
 * usado justamente nas apresentações. Promovê-la a tela inicial seria criar uma
 * quinta tela mostrando as mesmas coisas — o oposto do que este passo veio
 * resolver. A porta de entrada do produto é a Visão 360, e é ela que está no
 * menu.
 *
 * **O que ela dizia de errado, e foi corrigido** (documento 06): os selos de
 * status estavam parados no que era verdade em agosto. `/equipamentos` aparecia
 * como "Planejada" quando é hoje uma das duas telas mais completas, ligada à
 * API; `/clientes` e as fichas apareciam como "Pronta" sem distinguir o que lê
 * o banco do que ainda lê JSON. Um índice que mente sobre o próprio estado é
 * pior que índice nenhum — é para isso que se olha para ele. O contador
 * "Telas planejadas 9 / 6-10" e a linha "Mobile fora do escopo" também saíram:
 * são quinze rotas, e responder fora do desktop virou requisito no documento
 * 05 §3.
 */
import { Link } from 'react-router-dom';
import { ROTAS, acharRota } from '../rotas';

/**
 * O estado de cada tela, na única distinção que importa hoje: de onde ela lê.
 *
 * "Pronta" não separava mais nada — depois do passo 3 todas as quinze telas
 * estão portadas. O que muda entre elas, e o que alguém precisa saber ao abrir
 * este índice, é se a tela grava no banco pela API ou se ainda lê o JSON do
 * protótipo.
 */
type StatusTela = 'api' | 'prototipo' | 'mapa';

const SELO_STATUS: Record<StatusTela, { classe: string; rotulo: string }> = {
  api: { classe: 'badge-success', rotulo: 'Lê e grava na API' },
  prototipo: { classe: 'badge-warning', rotulo: 'Lê JSON do protótipo' },
  mapa: { classe: 'badge-neutral', rotulo: 'Ferramenta de desenvolvimento' },
};

const CARDS_NAVEGACAO: { caminho: string; desc: string; status: StatusTela }[] = [
  {
    caminho: '/agenda',
    desc: 'Fila priorizada por urgência real, com prazo em toda tarefa, criação de tarefa e registro de andamento.',
    status: 'prototipo',
  },
  {
    caminho: '/clientes/84391',
    desc: 'Ficha rica de leitura: dados, frota, oportunidades, histórico e faturamento. Alterar cadastro é em Clientes.',
    status: 'prototipo',
  },
  {
    caminho: '/equipamentos/1RW7250PVMR123456',
    desc: 'Ficha rica de leitura de um chassi: venda, garantia, revisões, peças, telemetria e horas.',
    status: 'prototipo',
  },
  {
    // A rota fixa saiu: a ficha da oportunidade passou a ser `/oportunidades/:chave`
    // e abre qualquer um dos 45.397 processos carregados. Como este mapa lista
    // caminhos que se pode clicar, e uma chave de exemplo aqui envelheceria na
    // primeira recarga do banco, o caminho para uma oportunidade é o Pipeline.
    caminho: '/pipeline',
    desc: 'Kanban por fase com o funil do banco. A ficha de uma oportunidade abre a partir daqui, pela chave dela.',
    status: 'api',
  },
  {
    caminho: '/cobertura',
    desc: 'Carteira do CEN com KPIs, mapa MT Norte, tabela filtrável e o registro de contato que atualiza a cobertura.',
    status: 'prototipo',
  },
  {
    caminho: '/pipeline',
    desc: 'Kanban de 6 fases · arrastar para avançar · fechar com motivo de catálogo · aprovação travada.',
    status: 'prototipo',
  },
  {
    caminho: '/clientes',
    desc: 'O cadastro de clientes da filial, pela API: busca, filtro, ordenação e paginação do servidor, ficha e alteração.',
    status: 'api',
  },
  {
    caminho: '/equipamentos',
    desc: 'O cadastro de frota da filial, pela API: busca por chassi, série e placa, filtro por marca, família e modelo, ficha e baixa.',
    status: 'api',
  },
  {
    caminho: '/relatorios/funil',
    desc: 'Relatório de conversão por estágio, com filtros e busca que funcionam e exportação em CSV.',
    status: 'prototipo',
  },
  {
    caminho: '/relatorios/cobertura',
    desc: 'Painel de 6 widgets: cobertura A/B por regional e vendas perdidas, com exportação do detalhamento.',
    status: 'prototipo',
  },
  {
    caminho: '/config',
    desc: 'Perfil · metas · aprovações · taxonomias · integrações · usuários · auditoria. O que é editável grava de verdade.',
    status: 'prototipo',
  },
  {
    caminho: '/relatorios/performance',
    desc: 'Painel executivo: 5 métricas, ranking que ordena, tendência FYTD, breakdown e insights. Switcher CEN/Regional/Nacional.',
    status: 'prototipo',
  },
];

export function Inicio() {
  const totalDeRotas = ROTAS.length;
  const ligadasNaApi = CARDS_NAVEGACAO.filter((c) => c.status === 'api').length;

  return (
    <>
      <div className="home-hero">
        <h1>Mapa do protótipo · CRM Tracbel Agro</h1>
        <p>
          <strong>Esta não é uma tela de produto.</strong> É o índice de desenvolvimento das telas do protótipo,
          feito para abrir numa apresentação e conferir o que existe. Quem vem trabalhar entra pela{' '}
          <strong>Visão 360</strong>, no menu — e é por isso que este mapa não está lá.
        </p>
      </div>

      <div className="home-quickaction">
        <Link to="/oportunidades/nova" className="quickaction-card">
          <div className="quickaction-icon">
            <svg viewBox="0 0 24 24" width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2.5">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
          </div>
          <div className="quickaction-body">
            <div className="quickaction-title">Criar Nova Oportunidade</div>
            <div className="quickaction-sub">
              Fluxo funcional · aparece imediatamente no Pipeline, Ficha do Cliente, Funil e Performance
            </div>
          </div>
          <div className="quickaction-arrow">→</div>
        </Link>
      </div>

      {/* Os números vêm da tabela de rotas, não escritos à mão: o contador
          "9 / 6-10" ficou parado enquanto as telas eram construídas, e um
          índice que erra a própria contagem não serve para conferir nada. */}
      <div className="kpi-grid">
        <div className="kpi">
          <span className="kpi-label">Rotas no protótipo</span>
          <span className="kpi-value">{totalDeRotas}</span>
          <span className="kpi-hint">
            Contadas na tabela de rotas: as {CARDS_NAVEGACAO.length} telas abaixo mais as de cadastro, edição e este
            mapa
          </span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Telas ligadas à API</span>
          <span className="kpi-value">
            {ligadasNaApi} <span style={{ fontSize: 14, color: 'var(--text-tertiary)', fontWeight: 500 }}>/ {CARDS_NAVEGACAO.length}</span>
          </span>
          <span className="kpi-hint">Clientes e Equipamentos · as demais ainda leem o JSON do protótipo</span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Fidelidade visual</span>
          <span className="kpi-value" style={{ fontSize: 16, paddingTop: 8 }}>
            Alta · John Deere
          </span>
          <span className="kpi-hint">Verde primário, amarelo de destaque</span>
        </div>
        <div className="kpi">
          <span className="kpi-label">Plataforma</span>
          <span className="kpi-value" style={{ fontSize: 16, paddingTop: 8 }}>
            Desktop e celular
          </span>
          <span className="kpi-hint">Responder fora do desktop virou requisito (documento 05 §3)</span>
        </div>
      </div>

      <div className="home-grid">
        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Estrutura de navegação</div>
              <div className="card-subtitle">
                As {CARDS_NAVEGACAO.length} telas do protótipo · clique para abrir
              </div>
            </div>
          </div>
          <div className="card-body">
            <div className="screens-index">
              {CARDS_NAVEGACAO.map(({ caminho, desc, status }) => {
                const rota = acharRota(caminho);
                const selo = SELO_STATUS[status];
                // Título do card é cópia literal de `screenTile()` (app.js:199-210).
                // Diverge do título canônico da rota (`rotas.tsx`) só em
                // /relatorios/cobertura: lá o <h1> da própria tela é "· Painel
                // Regional" (app.js:730), mas o card da Home usa "· Regional"
                // (app.js:208). Usar rota.titulo aqui quebra o <h4> em 2 linhas
                // e estica junto o card vizinho na mesma linha da grade.
                const titulo = caminho === '/relatorios/cobertura' ? 'Cobertura de Carteira · Regional' : rota.titulo;
                return (
                  <Link key={caminho} to={caminho} className="screen-tile">
                    <div className="screen-tile-icon">
                      <svg
                        width="18"
                        height="18"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.75"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      >
                        <rect x="3" y="3" width="18" height="18" rx="2" />
                        <line x1="3" y1="9" x2="21" y2="9" />
                        <line x1="9" y1="21" x2="9" y2="9" />
                      </svg>
                    </div>
                    <div className="screen-tile-body">
                      <h4>{titulo}</h4>
                      <p>{desc}</p>
                      <span className="status">
                        <span className={`badge ${selo.classe}`}>{selo.rotulo}</span>
                      </span>
                    </div>
                  </Link>
                );
              })}
            </div>
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Como este protótipo funciona</div>
            </div>
          </div>
          <div className="card-body" style={{ fontSize: 13, color: 'var(--text-secondary)', lineHeight: 1.6 }}>
            <p style={{ marginBottom: 12 }}>
              <strong style={{ color: 'var(--text-primary)' }}>Propósito:</strong> apoiar workshops, apresentações à
              diretoria e servir como referência visual para os devs.
            </p>
            <p style={{ marginBottom: 12 }}>
              <strong style={{ color: 'var(--text-primary)' }}>Método:</strong> você envia uma tela por vez (print +
              descrição). Cada tela é construída, revisada e refinada antes de partir para a próxima.
            </p>
            <p style={{ marginBottom: 12 }}>
              <strong style={{ color: 'var(--text-primary)' }}>Escopo visual:</strong> alta fidelidade, identidade
              John Deere, sem lógica de backend. Dados fictícios coerentes com o negócio Tracbel Agro.
            </p>
            <p style={{ padding: 12, background: 'var(--warning-bg)', color: 'var(--warning)', borderRadius: 6, fontWeight: 500 }}>
              Clientes e Equipamentos gravam no banco do CRM pela API. As demais telas leem o JSON do protótipo e
              guardam o que você faz nelas no próprio navegador — nenhuma conecta ao Protheus ainda.
            </p>
          </div>
        </div>
      </div>
    </>
  );
}
