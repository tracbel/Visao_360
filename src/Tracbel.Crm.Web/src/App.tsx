/**
 * Raiz da aplicação. Router por hash para manter as mesmas URLs do protótipo
 * (`#/cobertura`, `#/clientes/84391`, ...) e permitir comparação direta.
 *
 * TRÊS CAMADAS, de fora para dentro, e a ordem é a regra:
 *
 * 1. **A sessão** — alguém entrou? Enquanto não se sabe, nada abaixo existe.
 * 2. **O contexto de acesso** — a filial. Só faz sentido para quem já entrou.
 * 3. **O roteador** — as telas.
 *
 * O PORTÃO FICA ACIMA DO ROTEADOR, e não é uma rota `/login` dentro dele. Uma
 * rota de login seria só mais uma tela: bastaria digitar `#/cobertura` na barra
 * para passar por cima dela. Fora do roteador, sem sessão nenhuma tela chega a
 * montar — e nenhuma chamada à API chega a sair.
 */

import { lazy, Suspense, useEffect, type ReactNode } from 'react';
import { createHashRouter, RouterProvider } from 'react-router-dom';
import { LogoTracbel } from './componentes/Icones';
import { Layout } from './componentes/Layout';
import { ProvedorDeContextoDeAcesso } from './dados/api/contexto';
import { ProvedorDeSessao, useSessao } from './dados/api/sessao';
import { ROTAS } from './rotas';
import { Login } from './telas/Login';

const roteador = createHashRouter([
  {
    path: '/',
    element: <Layout />,
    children: ROTAS.map(({ caminho, Componente }) => ({
      index: caminho === '/',
      path: caminho === '/' ? undefined : caminho.slice(1),
      element: <Componente />,
    })),
  },
]);

function PortaoDeAutenticacao({ children }: { children: ReactNode }) {
  const { sessao, entrar } = useSessao();
  const dentro = sessao.estado === 'autenticado' || sessao.estado === 'provisorio';

  // QUEM ENTROU E CAIU EM `#/login` vai para o início. Acontece ao abrir o
  // endereço de volta do "Sair" já com a sessão nova: `login` não é rota do
  // roteador, e ele mostraria "página não encontrada".
  useEffect(() => {
    if (dentro && window.location.hash.startsWith('#/login')) window.location.hash = '#/';
  }, [dentro]);

  if (sessao.estado === 'carregando') {
    return (
      <div className="login-carregando" role="status" aria-label="Verificando a sessão">
        <LogoTracbel />
      </div>
    );
  }

  if (!dentro) return <Login sessao={sessao} aoEntrar={entrar} />;

  return <>{children}</>;
}

/**
 * O HARNESS VISUAL DE DESENVOLVIMENTO (fase T4.5) — `#/dev/mercado-visual`.
 *
 * ELE FICA ACIMA DO PORTÃO, e não é uma rota. Duas razões, e as duas importam:
 *
 * 1. **Ele não tem o que autenticar.** Só mostra amostra fictícia; exigir sessão
 *    seria pedir credencial para ver dado inventado — e é exatamente a sessão
 *    que falta quando não há VPN, que é quando ele mais serve.
 * 2. **Ele não pode aparecer na navegação.** Fora do `ROTAS`, nenhum menu o
 *    lista e nenhuma tela chega nele por engano.
 *
 * O TERNÁRIO É O QUE TIRA O HARNESS DO PACOTE, e ele não é estilo: com
 * `lazy(() => import(...))` solto no escopo do módulo, o `import()` continua
 * ALCANÇÁVEL mesmo com o ramo morto, e o Rollup gera o chunk assim mesmo —
 * medido em 23/09/2026, 13 kB de amostra fictícia dentro do `dist`. Sob o
 * ternário, `import.meta.env.DEV` vira `false` no `build`, isto vira `null` e o
 * `import()` deixa de ser alcançado. `npm run visual:conferir-pacote` confere.
 */
const HarnessVisual = import.meta.env.DEV
  ? lazy(() => import('./dev/HarnessVisual').then((m) => ({ default: m.HarnessVisual })))
  : null;

/**
 * O HARNESS DO SHELL — `#/dev/shell-visual` (fidelidade às maquetes, fase 5).
 * Desenha o `Layout` real, com menu e barra do topo, sobre sessão e filial
 * fictícias. Mesmo ternário, mesma razão: fora do `DEV` ele não chega ao `dist`.
 */
const HarnessDoShell = import.meta.env.DEV
  ? lazy(() => import('./dev/HarnessDoShell').then((m) => ({ default: m.HarnessDoShell })))
  : null;

export function App() {
  if (HarnessDoShell && window.location.hash.startsWith('#/dev/shell-visual')) {
    return (
      <Suspense fallback={null}>
        <HarnessDoShell />
      </Suspense>
    );
  }

  if (HarnessVisual && window.location.hash.startsWith('#/dev/')) {
    return (
      <Suspense fallback={null}>
        <HarnessVisual />
      </Suspense>
    );
  }

  return (
    <ProvedorDeSessao>
      <PortaoDeAutenticacao>
        <ProvedorDeContextoDeAcesso>
          <RouterProvider router={roteador} />
        </ProvedorDeContextoDeAcesso>
      </PortaoDeAutenticacao>
    </ProvedorDeSessao>
  );
}
