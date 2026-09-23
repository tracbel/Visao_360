/**
 * UMA MÉTRICA QUE AINDA NÃO EXISTE, na forma compacta (fase T2.1).
 *
 * `LacunaConhecida` desenha um bloco com selo, nome e um parágrafo — e ela
 * continua certa onde o espaço é de explicação. Na Visão Diretoria, porém, o
 * espaço é de MÉTRICA: o lugar onde um número apareceria não pode virar um
 * cartão de texto, senão a leitura executiva vira leitura de documentação
 * (issues 31 e 33).
 *
 * Aqui o formato é o mesmo de qualquer número ausente da tela — rótulo, traço e
 * `ⓘ` — e o motivo inteiro, com a issue que o destrava, vai na dica.
 *
 * `LacunaConhecida` NÃO foi alterada: outras telas dependem do desenho dela.
 */

import { ValorAusente } from './ValorAusente';

export function MetricaAusente({
  metrica,
  motivo,
}: {
  /** O nome do número que apareceria aqui. */
  metrica: string;
  /** Por que ele não existe, com a issue que o destrava. */
  motivo: string;
}) {
  return (
    <p className="cad-metrica-ausente">
      <span className="cad-metrica-ausente-rotulo">{metrica}</span>
      <ValorAusente motivo={motivo} oQue={metrica.toLowerCase()} />
    </p>
  );
}
