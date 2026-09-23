/**
 * COMO INTERPRETAR OS INDICADORES — documentação, não painel (fase T2.1).
 *
 * Isto era um CARTÃO DE LARGURA INTEIRA, permanente, acima dos números: um
 * título e uma linha por selo, com o motivo de cada um. É informação boa e
 * necessária — e é nível 4 da hierarquia da issue 33 (documentação técnica),
 * competindo por espaço com nível 1 (dado operacional).
 *
 * Virou um detalhe recolhível de uma linha. Nada se perdeu: abrir mostra os
 * mesmos selos com os mesmos motivos. `<details>` é do próprio HTML, então abre
 * pelo teclado e pelo leitor de tela sem nenhum código nosso.
 */

import type { ClassificacaoDeIndicador } from '../../tipos/territorio';
import { SeloDeClassificacao } from './SeloDeClassificacao';

export function ComoLerEstesNumeros({ classificacoes }: { classificacoes: ClassificacaoDeIndicador[] }) {
  if (classificacoes.length === 0) return null;

  return (
    <details className="cad-recolhivel" data-bloco="como-ler">
      <summary>Como interpretar os indicadores</summary>
      <div className="terr-classificacoes">
        {classificacoes.map((c) => (
          <span key={c.indicador} className="terr-classificacao">
            <SeloDeClassificacao classificacao={c} /> {c.motivo}
          </span>
        ))}
      </div>
    </details>
  );
}
