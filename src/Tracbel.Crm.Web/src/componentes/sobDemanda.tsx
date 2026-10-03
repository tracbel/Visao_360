/**
 * A TELA SÓ É BAIXADA QUANDO ABERTA (documento 54 §3.4) — o pacote inicial levava as 27 telas juntas (1,5 MB).
 *
 * DEPOIS DE UMA PUBLICAÇÃO, a aba aberta com o `index` antigo pede um pedaço que não existe mais: o nome do arquivo
 * mudou. Em vez de tela em branco, a página recarrega UMA vez (marcada na sessão, para não entrar em laço se o problema
 * for outro) e pega o pacote novo.
 */

import { lazy, Suspense, type ComponentType } from 'react';
import { BlocoCarregando } from './cadastro/EstadosDeTela';

const MARCA_DA_RECARGA = 'tracbel-crm-recarregou-por-pedaco';

export function sobDemanda<M>(carregar: () => Promise<M>, escolher: (modulo: M) => ComponentType): ComponentType {
  const Tela = lazy(() =>
    carregar().then(
      (modulo) => {
        sessionStorage.removeItem(MARCA_DA_RECARGA);
        return { default: escolher(modulo) };
      },
      (erro: unknown) => {
        if (!sessionStorage.getItem(MARCA_DA_RECARGA)) {
          sessionStorage.setItem(MARCA_DA_RECARGA, '1');
          window.location.reload();
          return new Promise<never>(() => {});
        }
        throw erro;
      },
    ),
  );

  function TelaSobDemanda() {
    return (
      <Suspense fallback={<BlocoCarregando oQue="a tela" />}>
        <Tela />
      </Suspense>
    );
  }

  return TelaSobDemanda;
}
