import type { ReactNode } from 'react';

/**
 * O título de uma seção da tela (fase T1 do documento 50).
 *
 * Usa as mesmas classes da seção "O mercado da região", que já existia: o T1
 * organiza a página em seções, e elas precisam ter todas o mesmo peso visual —
 * hierarquia se faz com tamanho e espaço, e duas famílias de título na mesma
 * página desfazem exatamente isso.
 */
export function TituloDaSecao({
  titulo,
  subtitulo,
  acao,
}: {
  titulo: string;
  subtitulo?: ReactNode;
  /** Uma ação secundária à direita do título — hoje só "Simular cenário". */
  acao?: ReactNode;
}) {
  return (
    <div className="terr-secao-mercado">
      <div className="terr-secao-cabecalho">
        <h2 className="terr-secao-titulo">{titulo}</h2>
        {acao}
      </div>
      {subtitulo && <p className="terr-secao-subtitulo">{subtitulo}</p>}
    </div>
  );
}
