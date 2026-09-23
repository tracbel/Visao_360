import { BlocoVazio } from '../cadastro/EstadosDeTela';
import { reaisCompactos } from './escalas';

/**
 * TERRITÓRIO NÃO CARREGADO NÃO É ZERO.
 *
 * A consulta respondeu, sem filtro de região nem de loja, e nenhum município veio
 * marcado como ADR: a carga do território não rodou neste banco. Mostrar
 * "0 municípios" e "R$ 0" faria a falta de carga parecer resultado — e as vendas
 * continuam no banco, somadas nos grupos fora do mapa.
 */
export function AvisoDeTerritorioSemCarga({
  vendasForaDoMapa,
  vendasSemCodigoIbge,
}: {
  vendasForaDoMapa: number;
  /** Nulo quando nenhum grupo fora do mapa é de município sem código IBGE. */
  vendasSemCodigoIbge: number | null;
}) {
  return (
    <div className="card cad-cartao terr-cartao terr-territorio-sem-carga" role="status" data-bloco="territorio-sem-carga">
      <BlocoVazio
        titulo="O território ainda não foi carregado neste banco"
        texto={
          <>
            A consulta respondeu, mas nenhum município tem código IBGE nem está marcado como ADR: a carga do território
            (catálogo IBGE, área de atuação, responsáveis e área plantada) não rodou neste banco.{' '}
            <strong>Não é falta de permissão nem falha de carregamento</strong>, e não é zero: as vendas do período continuam
            no banco — {reaisCompactos(vendasForaDoMapa)}, somadas nos grupos fora do mapa
            {vendasSemCodigoIbge !== null &&
              `, dos quais ${reaisCompactos(vendasSemCodigoIbge)} de clientes cujo município ainda não tem código IBGE`}
            . Os mapas e os totais da ADR aparecem assim que a carga do território rodar neste banco (documento 32, §4.6.1).
          </>
        }
      />
    </div>
  );
}
