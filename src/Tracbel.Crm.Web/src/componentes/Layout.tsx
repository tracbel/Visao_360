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
 *
 * ---------------------------------------------------------------------------
 * 23/09/2026 — o shell ganhou o visual das maquetes, com os NOSSOS itens.
 *
 * A queixa era "zero espaço no nosso menu". A decisão foi "visual da maquete,
 * nossos itens": o fundo verde-escuro, a marca por extenso, a pílula do item
 * ativo e o bloco do usuário vêm da maquete; a lista de telas e as quatro
 * seções continuam as de `navegacao.ts`, porque a maquete lista telas que não
 * existem aqui e nenhum item pode levar a página vazia.
 *
 * - **Sem "Ajuda" no pé**, embora a maquete tenha: não existe tela nem
 *   documento de ajuda para onde ele levar. Volta quando houver.
 * - **"Sair" mora no menu do usuário** (a seta ao lado do nome), e não mais
 *   solto na linha da filial. Continua sendo o mesmo link para `/auth/sair`.
 * - **Abaixo de 900 px o menu vira gaveta**, aberta pelo ☰ da barra do topo. A
 *   faixa horizontal de antes tinha 1.550 px de itens rolando de lado; a gaveta
 *   mostra os mesmos itens em pé, com as seções, e some quando não é usada.
 */

import { ChevronDown, ChevronRight, LogOut, Menu } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { TODAS_AS_FILIAIS } from '../dados/api/acesso';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { useSessao } from '../dados/api/sessao';
import { acharRota } from '../rotas';
import { SeletorDeFilial } from './cadastro/SeletorDeFilial';
import { IconeBusca, MarcaTracbelAgro } from './Icones';
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

  // A GAVETA SÓ EXISTE ABAIXO DE 900 px (o CSS decide). Acima disso o estado
  // até pode ficar `true`, e nada muda: o menu está sempre à vista.
  const [menuAberto, setMenuAberto] = useState(false);
  const botaoMenu = useRef<HTMLButtonElement>(null);
  const menuLateral = useRef<HTMLElement>(null);

  /**
   * `Esc` fecha a gaveta e devolve o foco ao ☰, que é de onde a pessoa veio —
   * sem isso o foco ficaria num link escondido.
   */
  useEffect(() => {
    if (!menuAberto) return;
    function aoTeclar(e: KeyboardEvent) {
      if (e.key !== 'Escape') return;
      setMenuAberto(false);
      botaoMenu.current?.focus();
    }
    document.addEventListener('keydown', aoTeclar);
    return () => document.removeEventListener('keydown', aoTeclar);
  }, [menuAberto]);

  /**
   * Abrir leva o foco para dentro da gaveta, no item da tela atual. A gaveta
   * vem ANTES da barra no DOM, então sem isso o próximo Tab depois do ☰ iria
   * para a busca, e quem usa teclado nunca chegaria ao menu que acabou de abrir.
   */
  function alternarMenu() {
    const abrir = !menuAberto;
    setMenuAberto(abrir);
    if (abrir) {
      requestAnimationFrame(() => {
        const alvo =
          menuLateral.current?.querySelector<HTMLElement>('a[aria-current="page"]') ??
          menuLateral.current?.querySelector<HTMLElement>('a');
        alvo?.focus();
      });
    }
  }

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
    <div className={menuAberto ? 'app-shell menu-aberto' : 'app-shell'}>
      <aside className="sidebar" id="menu-lateral" ref={menuLateral}>
        <div className="sidebar-brand">
          <MarcaTracbelAgro />
        </div>

        <nav className="sidebar-nav" aria-label="Menu principal">
          {SECOES.map((secao) => (
            <div className="nav-section" key={secao.titulo}>
              <span className="nav-section-title">{secao.titulo}</span>
              {secao.itens.map(({ caminho, rotulo, Icone }) => (
                <NavLink
                  key={caminho}
                  to={caminho}
                  end={caminho === '/'}
                  className={({ isActive }) => (isActive ? 'nav-item active' : 'nav-item')}
                  // Na gaveta, escolher a tela fecha o menu: ele já cumpriu o papel.
                  onClick={() => setMenuAberto(false)}
                >
                  <Icone tamanho={18} />
                  <span className="nav-item-rotulo">{rotulo}</span>
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

          A linha de baixo é o CÓDIGO da filial, e não o nome da maquete: o nome
          sai da rota de escopo, que o seletor do topo já lê — pedir de novo aqui
          dobraria a chamada em toda tela para repetir o que o topo já mostra.
        */}
        <div className="sidebar-footer">
          {sessao.estado === 'autenticado' ? (
            <MenuDoUsuario
              nome={sessao.nome}
              email={sessao.email}
              sigla={iniciais(sessao.nomePrincipal)}
              filial={contexto.empresa === TODAS_AS_FILIAIS ? 'Todas as filiais' : `Filial ${contexto.empresa}`}
            />
          ) : (
            <div className="user-chip">
              <div className="avatar" aria-hidden="true">
                {iniciais(contexto.usuario)}
              </div>
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

      {/* O FUNDO DA GAVETA fecha ao clicar fora dela. Não é botão de propósito:
          para o teclado quem fecha é o `Esc` e o próprio ☰, e um segundo
          "fechar" na ordem de tabulação seria só ruído. */}
      {menuAberto && <div className="menu-fundo" aria-hidden="true" onClick={() => setMenuAberto(false)} />}

      <main className="main">
        <header className="topbar">
          <div className="topbar-inicio">
            <button
              ref={botaoMenu}
              type="button"
              className="botao-menu"
              aria-controls="menu-lateral"
              aria-expanded={menuAberto}
              aria-label={menuAberto ? 'Fechar menu' : 'Abrir menu'}
              onClick={alternarMenu}
            >
              <Menu size={20} aria-hidden="true" />
            </button>
            <nav className="breadcrumb" aria-label="Você está em">
              {rota.trilha.map((parte, i) =>
                i === rota.trilha.length - 1 ? (
                  <span className="current" key={parte} aria-current="page">
                    {parte}
                  </span>
                ) : (
                  <span className="breadcrumb-nivel" key={parte}>
                    <span>{parte}</span>
                    <ChevronRight className="sep" size={14} aria-hidden="true" />
                  </span>
                ),
              )}
            </nav>
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
              {/* `kbd` dentro de `kbd` é a combinação de teclas, e cada tecla
                  ganha a sua caixinha, como na maquete. */}
              <kbd className="search-atalho">
                <kbd>Ctrl</kbd>
                <kbd>K</kbd>
              </kbd>
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
        {/* `conteudo-largo` tira o teto de largura das telas de análise (`larga` na rota). */}
        <div className={['content', rota.usaApi && 'conteudo-cadastro', rota.larga && 'conteudo-largo'].filter(Boolean).join(' ')}>
          <Outlet />
        </div>
      </main>
    </div>
  );
}

/**
 * O bloco do usuário no pé do menu, e o menu pequeno que a seta abre.
 *
 * É um BOTÃO QUE MOSTRA/ESCONDE (`aria-expanded`), e não um `role="menu"`: com
 * um item só, as setas do teclado de um menu de verdade não teriam para onde ir,
 * e o padrão de mostrar/esconder já é o que o leitor de tela anuncia certo.
 * O painel vem logo depois do botão no DOM, então o Tab seguinte chega no Sair.
 *
 * O Sair continua sendo ÂNCORA COMUM, e não navegação do roteador: sair precisa
 * passar pelo servidor, que derruba o cookie E a sessão na Microsoft. Só limpar
 * o estado da tela deixaria a próxima pessoa da máquina entrar direto nesta
 * conta.
 */
function MenuDoUsuario({ nome, email, sigla, filial }: { nome: string; email: string; sigla: string; filial: string }) {
  const [aberto, setAberto] = useState(false);
  const bloco = useRef<HTMLDivElement>(null);
  const botao = useRef<HTMLButtonElement>(null);

  // Clicar em qualquer outro lugar fecha, como todo menu suspenso.
  useEffect(() => {
    if (!aberto) return;
    function aoClicarFora(e: MouseEvent) {
      if (!bloco.current?.contains(e.target as Node)) setAberto(false);
    }
    document.addEventListener('mousedown', aoClicarFora);
    return () => document.removeEventListener('mousedown', aoClicarFora);
  }, [aberto]);

  return (
    <div
      className="usuario"
      ref={bloco}
      onKeyDown={(e) => {
        if (e.key !== 'Escape' || !aberto) return;
        // Sem isto o mesmo Esc subiria até o `document` e fecharia a gaveta inteira.
        e.stopPropagation();
        setAberto(false);
        botao.current?.focus();
      }}
    >
      <button
        ref={botao}
        type="button"
        className="usuario-botao"
        aria-expanded={aberto}
        aria-controls="menu-do-usuario"
        onClick={() => setAberto((valor) => !valor)}
      >
        <span className="avatar" aria-hidden="true">
          {sigla}
        </span>
        <span className="user-info">
          <span className="user-name" title={email}>
            {nome}
          </span>
          <span className="user-role">{filial}</span>
        </span>
        <ChevronDown className="usuario-seta" size={16} aria-hidden="true" />
      </button>

      {aberto && (
        <div className="usuario-menu" id="menu-do-usuario">
          <a className="usuario-menu-item" href="/auth/sair">
            <LogOut size={16} aria-hidden="true" />
            Sair
          </a>
        </div>
      )}
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
