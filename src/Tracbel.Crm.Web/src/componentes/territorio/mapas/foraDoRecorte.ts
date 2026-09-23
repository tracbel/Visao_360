import type { IndicadoresDoMunicipio } from '../../../tipos/territorio';
import type { EstadoNoMapa } from '../MapaDeMunicipios';

/**
 * O estado "fora", que os quatro mapas escrevem igual.
 *
 * Município que não é da ADR não é "sem dado": ele está fora do recorte, e o
 * mapa diz isso com outra palavra e outra cor. Devolve `null` quando o município
 * é da ADR — aí cada mapa segue com a medida dele.
 */
export function foraDoRecorte(m: IndicadoresDoMunicipio | undefined): EstadoNoMapa | null {
  if (m?.pertenceAAdr) return null;
  return { tipo: 'fora', detalhe: m ? 'fora da ADR' : 'fora da área de atuação ou do filtro' };
}
