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

import { useEffect, type ReactNode } from 'react';
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

export function App() {
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
