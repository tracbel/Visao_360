/**
 * Raiz da aplicação. Router por hash para manter as mesmas URLs do protótipo
 * (`#/cobertura`, `#/clientes/84391`, ...) e permitir comparação direta.
 *
 * O PROVEDOR DE CONTEXTO DE ACESSO ENVOLVE O ROTEADOR, e não o contrário: o
 * cabeçalho (`Layout`) mostra o seletor de filial, e ele é parte do roteador.
 * Toda chamada à API lê a filial daí.
 */

import { createHashRouter, RouterProvider } from 'react-router-dom';
import { Layout } from './componentes/Layout';
import { ProvedorDeContextoDeAcesso } from './dados/api/contexto';
import { ROTAS } from './rotas';

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

export function App() {
  return (
    <ProvedorDeContextoDeAcesso>
      <RouterProvider router={roteador} />
    </ProvedorDeContextoDeAcesso>
  );
}
