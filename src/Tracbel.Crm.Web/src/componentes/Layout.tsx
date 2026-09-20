/**
 * Shell da aplicação — sidebar, topbar e área de conteúdo.
 *
 * A marcação reproduz `prototipo/referencia/index.html` classe por classe, para
 * que `estilos/design-system.css` (cópia fiel do `app.css` do protótipo) se
 * aplique sem ajuste. Ao mexer aqui, revalide com as capturas de referência.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a barra de cima parou de prometer o que não fazia.
 *
 * **A busca global busca.** Era um campo decorativo — em TODAS as telas, o que
 * a tornava a ponta solta mais visível da aplicação — e o `Ctrl K` ao lado dela
 * também não fazia nada. Agora `Ctrl+K` põe o foco no campo (e `Esc` tira), e
 * `Enter` leva o termo à tela **Clientes**, que é a única busca de verdade que
 * existe: ela vai à API, dentro da filial do cabeçalho. O caminho está escrito
 * em `docs/prototipo/08-NAVEGACAO.md` — "quero achar um cliente → Clientes".
 *
 * O placeholder mudou junto: prometia chassi e oportunidade, e a busca de
 * cliente não acha nem um nem outro. Chassi se procura em Equipamentos, que
 * tem a própria busca; oportunidade, no Pipeline.
 *
 * **Os botões de notificações e de sincronização saíram.** Não existe sistema
 * de notificação nem sincronização para disparar, e o de notificações ainda
 * trazia um ponto vermelho de "não lido" que não contava nada — um alerta falso
 * é pior que alerta nenhum. Ficam registrados no documento 06.
 */

import { useEffect, useRef, useState } from 'react';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { useSessao } from '../dados/api/sessao';
import { acharRota } from '../rotas';
import { SeletorDeFilial } from './cadastro/SeletorDeFilial';
import { IconeBusca, LogoTracbel } from './Icones';
// O menu mora em `navegacao.ts`, que é dado: o teste o lê sem montar a
// aplicação inteira. A Agenda saiu dele em 19/09/2026 (issue 029) e continua
// sendo tela — a explicação está lá.
import { SECOES } from './navegacao';

export function Layout() {
  const { pathname } = useLocation();
  const rota = acharRota(pathname);
  const navegar = useNavigate();
  const { contexto } = useContextoDeAcesso();
  const { sessao } = useSessao();

  const [termoBusca, setTermoBusca] = useState('');
  const campoBusca = useRef<HTMLInputElement>(null);

  /**
   * `Ctrl+K` põe o foco na busca — é o que o `<kbd>` ao lado dela anuncia, e
   * até agora não acontecia. `preventDefault` porque o atalho é do navegador em
   * alguns casos, e quem digitou aqui quer este campo.
   */
  useEffect(() => {
    function aoTeclar(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        campoBusca.current?.focus();
        campoBusca.current?.select();
      }
    }
    document.addEventListener('keydown', aoTeclar);
    return () => document.removeEventListener('keydown', aoTeclar);
  }, []);

  /**
   * Leva o termo à tela Clientes, que busca na API dentro da filial escolhida.
   * O termo vai na própria rota para a lista já abrir com ele aplicado — sem
   * isso, a pessoa digitaria duas vezes.
   */
  function buscar(evento: React.FormEvent) {
    evento.preventDefault();
    const termo = termoBusca.trim();
    if (!termo) return;
    navegar(`/clientes?busca=${encodeURIComponent(termo)}`);
    campoBusca.current?.blur();
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <LogoTracbel />
          <div className="brand-text">
            <span className="brand-name">Tracbel Agro</span>
            <span className="brand-sub">CRM</span>
          </div>
        </div>

        <nav className="sidebar-nav">
          {SECOES.map((secao) => (
            <div className="nav-section" key={secao.titulo}>
              <span className="nav-section-title">{secao.titulo}</span>
              {secao.itens.map(({ caminho, rotulo, Icone }) => (
                <NavLink
                  key={caminho}
                  to={caminho}
                  end={caminho === '/'}
                  className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
                >
                  <Icone />
                  {rotulo}
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        {/*
          QUEM ESTÁ USANDO — o contexto de acesso de verdade, e não uma persona.

          Aqui ficava "João Ribeiro · CEN · Regional MT", que era o protagonista
          do protótipo. As duas metades estavam erradas: a pessoa não existe no
          cadastro, e a **regional não existe em lugar nenhum** — `IVS_Regional`
          tem zero linhas no Vórtice, e as treze filiais em operação estão todas
          no interior de São Paulo (documento 26, §1). Um rótulo de geografia
          inventada aparecendo em toda tela é o tipo de detalhe que se lê como
          fato depois da terceira vez.

          O que aparece agora é o que a API vai receber nos cabeçalhos: o
          usuário do contexto e a filial escolhida. É provisório e diz que é —
          cabeçalho HTTP não autentica ninguém (dívida D-1 do documento 23).
        */}
        <div className="sidebar-footer">
          {sessao.estado === 'autenticado' ? (
            <div className="user-chip">
              <div className="avatar">{iniciais(sessao.nomePrincipal)}</div>
              <div className="user-info">
                <span className="user-name" title={sessao.email}>
                  {sessao.nome}
                </span>
                <span className="user-role">
                  filial {contexto.empresa} ·{' '}
                  {/* ÂNCORA COMUM, e não navegação do roteador: sair precisa
                      passar pelo servidor, que derruba o cookie E a sessão na
                      Microsoft. Só limpar o estado da tela deixaria a próxima
                      pessoa da máquina entrar direto nesta conta. */}
                  <a className="user-sair" href="/auth/sair">
                    Sair
                  </a>
                </span>
              </div>
            </div>
          ) : (
            <div className="user-chip">
              <div className="avatar">{iniciais(contexto.usuario)}</div>
              <div className="user-info">
                <span className="user-name">{contexto.usuario.split('@')[0]}</span>
                <span className="user-role" title="Contexto de acesso provisório: os dois cabeçalhos que a API recebe. Não autentica ninguém.">
                  filial {contexto.empresa} · acesso provisório
                </span>
              </div>
            </div>
          )}
        </div>
      </aside>

      <main className="main">
        <header className="topbar">
          <div className="breadcrumb">
            {rota.trilha.map((parte, i) =>
              i === rota.trilha.length - 1 ? (
                <span className="current" key={parte}>
                  {parte}
                </span>
              ) : (
                <span key={parte}>
                  <span>{parte}</span>
                  <span className="sep">/</span>
                </span>
              ),
            )}
          </div>
          <div className="topbar-actions">
            {/* O seletor de filial só existe nas telas ligadas à API. A filial é a
                fronteira de acesso dela; nas telas que ainda leem o JSON do protótipo
                trocar de filial não mudaria nada, e um controle que não faz nada é
                exatamente o "botão que não faz nada" do documento 05 §3. */}
            {rota.usaApi && <SeletorDeFilial />}
            <form className="search" onSubmit={buscar} role="search">
              <IconeBusca />
              {/* O placeholder diz o que a busca acha de verdade. Chassi é em
                  Equipamentos e oportunidade é no Pipeline, cada uma com a
                  própria busca — prometer as três aqui era o engano. */}
              <input
                ref={campoBusca}
                type="search"
                aria-label="Buscar cliente por nome, nome fantasia ou documento"
                placeholder="Buscar cliente por nome ou documento..."
                value={termoBusca}
                onChange={(e) => setTermoBusca(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Escape') e.currentTarget.blur();
                }}
              />
              <kbd>Ctrl K</kbd>
            </form>
            {/* Os botões de notificações e sincronização saíram: não havia
                notificação nem sincronização para disparar, e o ponto vermelho
                de "não lido" não contava nada (documento 06). */}
          </div>
        </header>

        {/* `conteudo-cadastro` existe para o `design-system.css` poder restaurar,
            SÓ nestas telas, a borda do `.btn-secondary` — que uma regra antiga do
            bloco de Pipeline apaga em toda a aplicação ao usar duas variáveis que
            não existem (`--bg-primary`, `--border-primary`). Consertar a regra
            global mudaria o pixel das outras treze telas e quebraria a comparação
            visual; o conserto amplo é do passo 3. */}
        <div className={rota.usaApi ? 'content conteudo-cadastro' : 'content'}>
          <Outlet />
        </div>
      </main>
    </div>
  );
}

/**
 * As duas letras do disco do rodapé, tiradas do e-mail do contexto de acesso.
 *
 * `cen.ribeiraopreto@tracbel.com.br` vira `CR`. Quando não há ponto no nome,
 * usa as duas primeiras letras — é rótulo visual, não identificação.
 */
function iniciais(email: string): string {
  const nome = email.split('@')[0] ?? '';
  const partes = nome.split(/[._-]+/).filter(Boolean);
  const letras =
    partes.length >= 2
      ? `${partes[0][0]}${partes[1][0]}`
      : nome.slice(0, 2);
  return letras.toUpperCase() || '?';
}
