/**
 * Bloco de card usado nas fichas (cliente/equipamento/oportunidade) — porte de
 * `renderFichaBloco()` (`prototipo/referencia/assets/app.js`, linha 2181).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — o lápis de "Editar bloco" saiu.
 *
 * Ele aparecia no cabeçalho de **todos** os blocos das três fichas — dezenas de
 * vezes — e respondia com `alert('Modo edição de bloco (protótipo…)')`. Era o
 * tipo de ponta solta que a primeira varredura não pegou, porque tinha
 * `onClick`: o que faltava não era o manipulador, era o efeito. Um lápis é a
 * promessa mais direta que existe numa ficha, e essa promessa custava um clique
 * e uma janela de desculpa.
 *
 * Editar cadastro de cliente é na tela **Clientes** e editar equipamento é em
 * **Equipamentos** — as duas ligadas à API, as duas com o caminho escrito no
 * documento 08. As três fichas ricas são leitura, e agora parecem leitura.
 */
import type { ReactNode } from 'react';

type Props = {
  id: string;
  titulo: string;
  espacoso?: boolean;
  children: ReactNode;
};

export function BlocoFicha({ id, titulo, espacoso = false, children }: Props) {
  return (
    <div className={`ficha-bloco ${espacoso ? 'ficha-bloco-full' : ''}`} data-bloco={id}>
      <div className="ficha-bloco-header">
        <div className="ficha-bloco-titulo">{titulo}</div>
      </div>
      <div className="ficha-bloco-body">{children}</div>
    </div>
  );
}
