import { useLocation, useNavigate } from 'react-router-dom';

/**
 * Espelha a URL corrente no DOM e oferece o "voltar" DO ROTEADOR.
 *
 * `history.back()` do jsdom não fala com o `MemoryRouter`: quem anda no histórico
 * é o roteador, e é o comportamento dele que interessa provar (issue 163).
 *
 * Mora em `testes/` para não deixar um componente num arquivo de teste — o que
 * quebra o recarregamento rápido do Vite e acende aviso de lint.
 */
export function EspelhoDaUrl() {
  const { search } = useLocation();
  const navegar = useNavigate();
  return (
    <>
      <output data-url>{search}</output>
      <button type="button" data-voltar onClick={() => navegar(-1)}>
        voltar
      </button>
    </>
  );
}
