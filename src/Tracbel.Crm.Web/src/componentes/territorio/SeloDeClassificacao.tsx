import type { ClassificacaoDeIndicador } from '../../tipos/territorio';

/** O selo de como ler um indicador — medido, regra provisória, estimativa —, com o motivo no título. */
export function SeloDeClassificacao({ classificacao }: { classificacao: ClassificacaoDeIndicador | null }) {
  if (!classificacao) return null;
  return (
    <span className={`terr-selo terr-selo-${classificacao.situacao}`} title={classificacao.motivo}>
      {classificacao.selo}
    </span>
  );
}
