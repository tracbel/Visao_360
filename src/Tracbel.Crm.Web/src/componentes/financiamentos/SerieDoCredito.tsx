/**
 * A SÉRIE TEMPORAL DO CRÉDITO (issue 261) — o recorte do primeiro ao último mês do SICOR, em valor ou em linhas, por mês,
 * trimestre, semestre ou ano. É o `GraficoLinhaMensal` do painel do crédito, com o último grupo incompleto tracejado.
 *
 * A SÉRIE INTEIRA, E NÃO SÓ O PERÍODO: o período escolhido recorta os números do topo e as tabelas; a série é o contexto —
 * de onde o crédito veio até chegar ali.
 */

import { useMemo, useState } from 'react';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { GraficoSemSerie, Seletor } from '../mercado/momento/pecas';
import { reaisCurtos } from '../mercado/momento/formatos';
import type { LinhasDoSicorNoMes } from '../../tipos/mercado';
import { agrupar, GRANULARIDADES, METRICAS, n, type Granularidade, type MetricaDoCredito } from './financiamentos';

export function SerieDoCredito({ serie }: { serie: readonly LinhasDoSicorNoMes[] }) {
  const [granularidade, setGranularidade] = useState<Granularidade>('mes');
  const [metrica, setMetrica] = useState<MetricaDoCredito>('valor');
  const pontos = useMemo(() => agrupar(serie, granularidade), [serie, granularidade]);
  const formatar = metrica === 'valor' ? reaisCurtos : (v: number) => n(v);

  return (
    <div className="fin-serie" data-bloco="serie">
      <div className="fin-serie-controles">
        <Seletor rotulo="Métrica" valor={metrica} opcoes={METRICAS} aoMudar={setMetrica} />
        <Seletor rotulo="Granularidade" valor={granularidade} opcoes={GRANULARIDADES} aoMudar={setGranularidade} />
      </div>
      {pontos.length > 1 ? (
        <MolduraDeGrafico altura={240}>
          {(l, a) => (
            <GraficoLinhaMensal
              rotulos={pontos.map((p) => p.rotulo)}
              valores={pontos.map((p) => p[metrica])}
              largura={l}
              altura={a}
              formatar={formatar}
              ultimoParcial={!pontos[pontos.length - 1].completo}
            />
          )}
        </MolduraDeGrafico>
      ) : (
        <GraficoSemSerie
          altura={240}
          frase="Série sem pontos suficientes"
          motivo="O recorte não tem crédito de máquina em mais de um período do SICOR."
          oQue="a série do crédito"
        />
      )}
    </div>
  );
}
