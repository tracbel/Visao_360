/**
 * O MENU DE UMA LINHA DE TABELA — os três pontos das maquetes (Cobertura de Carteira e Pipeline, 30/09/2026).
 *
 * Só leva a telas que já existem: cada item é um link, e nada aqui grava. Fecha com clique fora ou com Esc, e o botão diz
 * ao leitor de tela de qual linha é o menu.
 */

import { Ellipsis, EllipsisVertical } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import '../../estilos/menu-da-linha.css';

export type ItemDoMenu = { rotulo: string; para: string };

export function MenuDaLinha({
  rotulo,
  itens,
  horizontal = false,
}: {
  /** O nome acessível do botão — "Ações de <linha>". */
  rotulo: string;
  itens: ItemDoMenu[];
  /** Os três pontos deitados (Pipeline) ou em pé (Cobertura), como cada maquete desenha. */
  horizontal?: boolean;
}) {
  const [aberto, setAberto] = useState(false);
  const caixa = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!aberto) return;
    const fora = (e: MouseEvent) => {
      if (!caixa.current?.contains(e.target as Node)) setAberto(false);
    };
    const tecla = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setAberto(false);
    };
    document.addEventListener('mousedown', fora);
    document.addEventListener('keydown', tecla);
    return () => {
      document.removeEventListener('mousedown', fora);
      document.removeEventListener('keydown', tecla);
    };
  }, [aberto]);

  const Icone = horizontal ? Ellipsis : EllipsisVertical;

  return (
    <div className="menu-da-linha" ref={caixa}>
      <button
        type="button"
        className="menu-da-linha-botao"
        aria-haspopup="menu"
        aria-expanded={aberto}
        aria-label={rotulo}
        onClick={() => setAberto((a) => !a)}
      >
        <Icone size={16} strokeWidth={2.2} aria-hidden="true" />
      </button>
      {aberto && (
        <div className="menu-da-linha-lista" role="menu">
          {itens.map((item) => (
            <Link key={item.para} role="menuitem" to={item.para}>
              {item.rotulo}
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
