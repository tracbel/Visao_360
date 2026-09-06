import { useEffect, useState } from 'react';

/**
 * Diz quando as fontes da página terminaram de carregar.
 *
 * Por que existe: o Chart.js mede o espaço dos rótulos do eixo no primeiro
 * desenho. Se a Inter ainda não carregou, ele mede com a fonte substituta, que
 * é mais estreita, reserva pouco espaço e depois redesenha com a fonte
 * definitiva — o texto estoura a área e a primeira letra some ("reço acima do
 * concorrente" em vez de "Preço acima do concorrente").
 *
 * Quem usa isto força um novo desenho depois que as fontes chegam.
 */
export function useFontesProntas(): boolean {
  const [prontas, setProntas] = useState(() => document.fonts?.status === 'loaded');

  useEffect(() => {
    if (prontas || !document.fonts) return;
    let ativo = true;
    document.fonts.ready
      .then(() => {
        if (ativo) setProntas(true);
      })
      .catch(() => {
        // navegador sem a API ou carregamento falhou: segue com a fonte que houver
        if (ativo) setProntas(true);
      });
    return () => {
      ativo = false;
    };
  }, [prontas]);

  return prontas;
}
