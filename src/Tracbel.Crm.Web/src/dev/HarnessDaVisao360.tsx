/**
 * O HARNESS DA VISÃO 360 — `#/dev/visao360-visual`, só em desenvolvimento.
 *
 * POR QUE ELE EXISTE. A Visão 360 é a tela de entrada, e conferir o layout dela
 * exigia banco, VPN e sessão — e, com o banco de hoje sanitizado, nem assim se
 * via um painel cheio. Aqui ela monta inteira, sobre amostras fictícias, em
 * qualquer máquina.
 *
 * DENTRO DO `Layout` DE VERDADE, com o menu lateral: as quebras desta tela são
 * pela largura do CONTEÚDO, e o conteúdo é a janela menos o menu. Sem o menu, a
 * captura de 1536 mostraria 232px de painel a mais do que o Ricardo vê.
 *
 * COMO ELE ALIMENTA A TELA: interceptando `fetch`, pelo mesmo caminho dos
 * harness de Mercado e do shell. A tela não tem uma linha a mais por causa
 * dele. O interceptador é instalado no carregamento do módulo — no `useEffect`
 * o StrictMode o desinstalaria no instante seguinte (ver `HarnessVisual.tsx`).
 * A filial de cada leitura sai do cabeçalho `X-Tracbel-Empresa`, que é
 * exatamente o que o consolidado troca a cada filial.
 *
 * O CARIMBO NÃO SAI DA TELA: "AMOSTRA FICTÍCIA" fixo no canto, e a pessoa da
 * sessão se chama "Amostra Fictícia". Nenhuma captura daqui passa por real.
 *
 * ELE NUNCA ENTRA NO PACOTE: `App.tsx` só o alcança sob `import.meta.env.DEV`,
 * pelo mesmo ternário dos outros dois, e `npm run visual:conferir-pacote`
 * confere.
 *
 * Parâmetros do hash: `estado` (`completo`, `vazio` ou `semCarteira` — ver
 * `amostrasDaVisao360.ts`).
 */

import { useEffect, useState } from 'react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { Layout } from '../componentes/Layout';
import type { EscopoDoUsuario } from '../dados/api/acesso';
import { ProvedorDeContextoDeAcesso } from '../dados/api/contexto';
import { ProvedorDeSessao } from '../dados/api/sessao';
import { ROTAS } from '../rotas';
import { ESTADOS_DA_VISAO360, respostaDaVisao360, type EstadoDaVisao360 } from './amostrasDaVisao360';
import '../estilos/design-system.css';

function parametro(nome: string): string | null {
  const consulta = window.location.hash.split('?')[1] ?? '';
  return new URLSearchParams(consulta).get(nome);
}

function estadoDaUrl(): EstadoDaVisao360 {
  const pedido = parametro('estado');
  return ESTADOS_DA_VISAO360.some((e) => e.id === pedido) ? (pedido as EstadoDaVisao360) : 'completo';
}

/**
 * O escopo do seletor de filial da barra do topo. O código é o padrão do
 * contexto de acesso (`010101`), para o seletor abrir já com a filial escolhida;
 * o nome é o de uma filial fictícia.
 */
const ESCOPO_FICTICIO: EscopoDoUsuario = {
  usuario: 'amostra.ficticia@exemplo.invalid',
  filialAtual: { codigo: '010101', nome: 'Filial Fictícia Alfa', ehCasa: true },
  filialPedidaRecusada: null,
  filiaisPermitidas: [{ codigo: '010101', nome: 'Filial Fictícia Alfa', ehCasa: true }],
  permissoes: [],
  podeVerTodasAsFiliais: false,
  perfis: [],
};

/** Uma resposta pronta, no envelope que o `ler()` espera. */
function envelope(dados: unknown): Response {
  return new Response(
    JSON.stringify({
      dados,
      procedencia: {
        sistema: 'HARNESS DA VISÃO 360 — amostra fictícia',
        objeto: 'nenhum: nada aqui veio de banco',
        lidoEmUtc: '2026-09-24T12:00:00Z',
        dadoMaisRecenteEm: null,
      },
    }),
    { status: 200, headers: { 'Content-Type': 'application/json' } },
  );
}

/** A filial da leitura: o cabeçalho que o consolidado troca a cada volta. */
function filialDoPedido(init: RequestInit | undefined): string {
  const cabecalhos = new Headers(init?.headers);
  return cabecalhos.get('X-Tracbel-Empresa') ?? '';
}

/**
 * EMBRULHA o `fetch` que estiver instalado, em vez de substituí-lo: o que não
 * for da Visão 360 segue para ele — os assets do Vite, e os outros harness se
 * tiverem sido carregados na mesma aba.
 */
function instalarInterceptador(): void {
  const anterior = window.fetch.bind(window);
  window.fetch = async (entrada, init) => {
    const url = typeof entrada === 'string' ? entrada : entrada instanceof URL ? entrada.href : entrada.url;

    if (url.includes('/auth/eu')) {
      return new Response(
        JSON.stringify({
          modo: 'entra',
          nome: 'Amostra Fictícia',
          nomePrincipal: 'amostra.ficticia@exemplo.invalid',
          email: 'amostra.ficticia@exemplo.invalid',
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      );
    }

    if (!url.includes('/api/v1/')) return anterior(entrada, init);

    const [caminho, consulta = ''] = url.replace(/^.*\/api/, '').split('?');
    const dados =
      caminho === '/v1/acesso/escopo'
        ? ESCOPO_FICTICIO
        : respostaDaVisao360(caminho, new URLSearchParams(consulta), filialDoPedido(init), estadoDaUrl());
    if (dados !== undefined) return envelope(dados);

    // UMA ROTA QUE NINGUÉM SIMULOU RESPONDE 404, e não vai à rede: o harness não
    // pode sair pedindo dado de verdade a um servidor que ele não devia tocar.
    return new Response(JSON.stringify({ title: `O harness da Visão 360 não simula ${caminho}.` }), {
      status: 404,
      headers: { 'Content-Type': 'application/problem+json' },
    });
  };
}

instalarInterceptador();

/** O mesmo desenho de rotas do `App.tsx`, num roteador em memória aberto na Visão 360. */
function criarRoteador() {
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
    { initialEntries: ['/'] },
  );
}

function ShellFicticio() {
  const [roteador] = useState(criarRoteador);
  return <RouterProvider router={roteador} />;
}

export function HarnessDaVisao360() {
  const [estado, setEstado] = useState<EstadoDaVisao360>(estadoDaUrl);

  // A URL MANDA, como nos outros harness: é ela que o Playwright abre.
  useEffect(() => {
    const aoTrocarHash = () => setEstado(estadoDaUrl());
    window.addEventListener('hashchange', aoTrocarHash);
    return () => window.removeEventListener('hashchange', aoTrocarHash);
  }, []);

  return (
    // A CHAVE REMONTA TUDO ao trocar de estado: sem ela, o `useRecurso` manteria
    // o dado do estado anterior na tela enquanto o novo carrega.
    <div data-harness="visao360-visual" data-estado={estado} key={estado}>
      <ProvedorDeSessao>
        <ProvedorDeContextoDeAcesso>
          <ShellFicticio />
        </ProvedorDeContextoDeAcesso>
      </ProvedorDeSessao>

      {/* FIXO NO CANTO, e não uma faixa no topo: a faixa empurraria o menu e a
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
        AMOSTRA FICTÍCIA — harness da Visão 360 · {estado}
      </div>
    </div>
  );
}
