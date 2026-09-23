import type { ReactNode } from 'react';
import { InfoTooltip } from '../InfoTooltip';

/**
 * O título de uma seção da tela (fase T1; densidade na T2.1).
 *
 * Usa as mesmas classes da seção "O mercado da região": hierarquia se faz com
 * tamanho e espaço, e duas famílias de título na mesma página desfazem isso.
 *
 * O SUBTÍTULO É UMA FRASE CURTA, E SÓ (issues 31 e 33). Ele estava virando o
 * lugar onde a metodologia se acumulava — "o denominador é o total publicado
 * pelo IBGE, que não é a soma dos municípios…" —, e três seções assim empilham
 * mais texto do que número. O que é método vai em `metodologia`, na dica.
 */
export function TituloDaSecao({
  titulo,
  subtitulo,
  metodologia,
  acao,
}: {
  titulo: string;
  /** Uma frase curta. Se precisar de ponto e vírgula, é metodologia. */
  subtitulo?: ReactNode;
  /** Fonte, método e ressalvas — vai para a dica ao lado do título. */
  metodologia?: string;
  /** Uma ação secundária à direita do título — hoje só "Simular cenário". */
  acao?: ReactNode;
}) {
  return (
    <div className="terr-secao-mercado">
      <div className="terr-secao-cabecalho">
        <h2 className="terr-secao-titulo">
          {titulo}
          {metodologia && <InfoTooltip texto={metodologia} rotulo={`Fonte e método de ${titulo.toLowerCase()}`} />}
        </h2>
        {acao}
      </div>
      {subtitulo && <p className="terr-secao-subtitulo">{subtitulo}</p>}
    </div>
  );
}
