/**
 * O HARNESS DO SHELL — `#/dev/shell-visual`, só em desenvolvimento (fidelidade
 * às maquetes, fase 5).
 *
 * POR QUE ELE EXISTE. O menu lateral e a barra do topo valem para TODAS as
 * telas, e o harness de Mercado não os desenha: ele monta só a área de conteúdo.
 * Conferir o shell exigia login e VPN — sem eles, ajuste de menu vira aposta.
 * Aqui o `Layout` DE VERDADE monta inteiro, com sessão, filial e rota fictícias.
 *
 * COMO ELE ALIMENTA O SHELL: pelo mesmo caminho do harness de Mercado —
 * interceptando `fetch`, e não injetando dependência no `Layout`. A sessão sai
 * do `ProvedorDeSessao` de produção perguntando `/auth/eu`; o seletor de filial,
 * do `obterEscopo` de produção. Os dois só ouvem respostas daqui.
 *
 * AS TELAS SÃO AS DE VERDADE (`ROTAS`). Indicadores Geográficos desenha com as
 * amostras do harness de Mercado, porque o interceptador dele é instalado ao
 * importar o módulo. As outras telas recebem 404 do interceptador ("o harness
 * não simula") e mostram o estado de erro delas — o que se confere aqui é o
 * shell em volta, e não o conteúdo.
 *
 * O CARIMBO NÃO SAI DA TELA: "AMOSTRA FICTÍCIA" fica fixo no canto, e a pessoa
 * da sessão se chama "Amostra Fictícia". Nenhuma captura daqui passa por real.
 *
 * ELE NUNCA ENTRA NO PACOTE: `App.tsx` só o alcança sob `import.meta.env.DEV`,
 * pelo mesmo ternário do harness de Mercado.
 *
 * Parâmetros do hash: `rota` (padrão `/relatorios/territorio`), `sessao`
 * (`entra` ou `provisorio`), `estado` (os do harness de Mercado) e `filial`
 * (`recusada` faz o escopo dizer que a filial guardada deixou de ser permitida —
 * o aviso da P-20 na barra do topo, que é o texto mais largo dela).
 */

import { useEffect, useState } from 'react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { Layout } from '../componentes/Layout';
import type { EscopoDoUsuario } from '../dados/api/acesso';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { ProvedorDeSessao } from '../dados/api/sessao';
import { ROTAS } from '../rotas';
// SÓ PELO EFEITO: o módulo instala o interceptador de `/api/v1/territorio/*` ao
// carregar. Sem esta linha a tela Indicadores cairia no erro de rede.
import './HarnessVisual';
import '../estilos/design-system.css';

function parametro(nome: string): string | null {
  const consulta = window.location.hash.split('?')[1] ?? '';
  return new URLSearchParams(consulta).get(nome);
}

function rotaDaUrl(): string {
  const pedida = parametro('rota') ?? '/relatorios/territorio';
  return pedida.startsWith('/') ? pedida : `/${pedida}`;
}

const ESCOPO_FICTICIO: EscopoDoUsuario = {
  usuario: 'amostra.ficticia@exemplo.invalid',
  filialAtual: { codigo: '010101', nome: 'Tracbel Agro — Araraquara', ehCasa: true },
  filialPedidaRecusada: null,
  filiaisPermitidas: [
    { codigo: '010101', nome: 'Tracbel Agro — Araraquara', ehCasa: true },
    { codigo: '010102', nome: 'Tracbel Agro — Ribeirão Preto', ehCasa: false },
  ],
  permissoes: [],
  podeVerTodasAsFiliais: false,
  perfis: [],
};

function json(corpo: unknown, status = 200): Response {
  return new Response(JSON.stringify(corpo), { status, headers: { 'Content-Type': 'application/json' } });
}

/**
 * EMBRULHA o interceptador que já está instalado, em vez de substituí-lo: o de
 * Mercado continua respondendo o território, e este só acrescenta as duas
 * perguntas do shell. Instalado no módulo pela mesma razão de lá — o StrictMode
 * desinstalaria no instante seguinte um interceptador posto num efeito.
 */
function instalarInterceptadorDoShell(): void {
  const anterior = window.fetch.bind(window);
  window.fetch = async (entrada, init) => {
    const url = typeof entrada === 'string' ? entrada : entrada instanceof URL ? entrada.href : entrada.url;

    if (url.includes('/auth/eu')) {
      return parametro('sessao') === 'provisorio'
        ? json({ modo: 'provisorio' })
        : json({
            modo: 'entra',
            nome: 'Amostra Fictícia',
            nomePrincipal: 'amostra.ficticia@exemplo.invalid',
            email: 'amostra.ficticia@exemplo.invalid',
          });
    }

    if (url.includes('/api/v1/acesso/escopo')) {
      return json({
        dados:
          parametro('filial') === 'recusada'
            ? { ...ESCOPO_FICTICIO, filialPedidaRecusada: '010199' }
            : ESCOPO_FICTICIO,
        procedencia: {
          sistema: 'HARNESS DO SHELL — amostra fictícia',
          objeto: 'nenhum: nada aqui veio de banco',
          lidoEmUtc: '2026-09-23T12:00:00Z',
          dadoMaisRecenteEm: null,
        },
      });
    }

    return anterior(entrada, init);
  };
}

instalarInterceptadorDoShell();

/** O mesmo desenho de rotas do `App.tsx`, num roteador em memória: o hash é do harness. */
function criarRoteador(rota: string) {
  return createMemoryRouter(
    [
      {
        path: '/',
        element: <Layout />,
        children: ROTAS.map(({ caminho, Componente }) => ({
          index: caminho === '/',
          path: caminho === '/' ? undefined : caminho.slice(1),
          element: <Componente />,
        })),
      },
    ],
    { initialEntries: [rota] },
  );
}

function ShellFicticio({ rota }: { rota: string }) {
  const [roteador] = useState(() => criarRoteador(rota));
  return <RouterProvider router={roteador} />;
}

export function HarnessDoShell() {
  const [chave, setChave] = useState(() => window.location.hash);

  // A URL MANDA, como no harness de Mercado: é ela que o Playwright abre.
  useEffect(() => {
    const aoTrocarHash = () => setChave(window.location.hash);
    window.addEventListener('hashchange', aoTrocarHash);
    return () => window.removeEventListener('hashchange', aoTrocarHash);
  }, []);

  return (
    <div data-harness="shell-visual" key={chave}>
      <ProvedorDeSessao>
        <ProvedorDeContextoDeAcesso>
          <ShellFicticio rota={rotaDaUrl()} />
        </ProvedorDeContextoDeAcesso>
      </ProvedorDeSessao>

      {/* FIXO NO CANTO, e não uma faixa no topo: uma faixa empurraria o menu e a
          barra para baixo, e a captura deixaria de medir o que o usuário vê. */}
      <div
        aria-hidden="true"
        style={{
          position: 'fixed',
          right: 8,
          bottom: 8,
          zIndex: 1000,
          pointerEvents: 'none',
          background: '#7F1D1D',
          color: '#FFF',
          padding: '4px 10px',
          borderRadius: 4,
          font: '600 11px/1.4 system-ui, sans-serif',
        }}
      >
        AMOSTRA FICTÍCIA — harness do shell
      </div>
    </div>
  );
}
