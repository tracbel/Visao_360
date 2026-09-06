/**
 * Um cartão que começa fechado.
 *
 * POR QUE ELE EXISTE: as telas carregam blocos de texto que explicam o que o
 * dado NÃO diz — as lacunas conhecidas, o motivo de uma coluna ter saído. Esse
 * texto precisa continuar acessível: é ele que impede que alguém leia um número
 * pela metade e decida errado. Mas ele ocupava quatro parágrafos no fim de toda
 * tela, e a Cobertura de Carteira chegou a 5.905px de altura por causa disso.
 *
 * Recolhido, o texto continua na página, indexável e a um clique — e a tela
 * cabe. `<details>` nativo, e não um estado em React, porque o navegador já
 * resolve teclado, leitor de tela e busca por Ctrl+F dentro do bloco fechado.
 */

import type { ReactNode } from 'react';

export function BlocoRecolhivel({
  titulo,
  resumo,
  children,
}: {
  titulo: string;
  /** Uma linha do lado do título, dizendo o que há dentro. */
  resumo?: string;
  children: ReactNode;
}) {
  return (
    <details className="card cad-cartao cad-recolhivel">
      <summary className="cad-recolhivel-cabecalho">
        <span className="card-title">{titulo}</span>
        {resumo && <span className="card-subtitle">{resumo}</span>}
      </summary>
      <div className="cad-recolhivel-corpo">{children}</div>
    </details>
  );
}
