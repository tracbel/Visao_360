import type { ClassificacaoDeIndicador } from '../../tipos/territorio';
import { SeloDeClassificacao } from './SeloDeClassificacao';

/** A legenda dos selos: um por indicador, com o motivo ao lado. */
export function ComoLerEstesNumeros({ classificacoes }: { classificacoes: ClassificacaoDeIndicador[] }) {
  if (classificacoes.length === 0) return null;

  return (
    <div className="card cad-cartao terr-cartao" data-bloco="como-ler">
      <div className="terr-classificacoes" aria-label="Como ler estes números">
        <strong>Como ler estes números</strong>
        {classificacoes.map((c) => (
          <span key={c.indicador} className="terr-classificacao">
            <SeloDeClassificacao classificacao={c} /> {c.motivo}
          </span>
        ))}
      </div>
    </div>
  );
}
