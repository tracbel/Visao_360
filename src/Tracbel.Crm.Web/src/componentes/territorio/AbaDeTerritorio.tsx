/**
 * A aba Território — a leitura operacional (documento 50, §5).
 *
 * ELA NÃO DUPLICA OS CARTÕES EXECUTIVOS DE MERCADO. Onde um dado já aparece
 * resumido lá, aqui ele aparece na forma de trabalho: a linha, o nome, a data.
 *
 * O QUE MUDOU DE ABA: a linha de indicadores da carteira — municípios da ADR,
 * cobertura pela cadência, vendas no período e parque teórico — saiu do topo de
 * Mercado e veio para cá, porque é a leitura de quem trabalha o território, e
 * não a de quem decide sobre o mercado. Ela não perdeu nem ganhou um número.
 *
 * O QUE AINDA NÃO ESTÁ AQUI: responsáveis, cadência e carteira por município
 * existem hoje **dentro da ficha** do município, e não como painéis próprios.
 * Trazê-los para cá é trabalho da issue 78 e das fases seguintes — inventar
 * painéis vazios agora só encheria a aba.
 */

import type { IndicadoresDoMunicipio, IndicadoresForaDoMapa } from '../../tipos/territorio';
import { PainelDeIndicadores, type Indicador } from '../cadastro/Indicadores';
import { TabelaDeMunicipios } from './TabelaDeMunicipios';
import { TituloDaSecao } from './TituloDaSecao';
import type { TotaisDaAdr } from './totaisDaAdr';
import type { ReactNode } from 'react';

export function AbaDeTerritorio({
  kpisDaCarteira,
  carregando,
  indicadores,
  municipios,
  ordenados,
  foraDoMapa,
  totais,
  selecionado,
  aoSelecionar,
  territorioNaoCarregado,
  semFiltro,
  ficha,
}: {
  kpisDaCarteira: Indicador[];
  carregando: boolean;
  /** Nulo enquanto a leitura não respondeu — a tabela não aparece antes disso. */
  indicadores: boolean;
  municipios: IndicadoresDoMunicipio[];
  ordenados: IndicadoresDoMunicipio[];
  foraDoMapa: IndicadoresForaDoMapa[];
  totais: TotaisDaAdr;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  territorioNaoCarregado: boolean;
  semFiltro: boolean;
  ficha: ReactNode;
}) {
  return (
    <>
      <TituloDaSecao
        titulo="A carteira na área de atuação"
        subtitulo="Quem está coberto, quanto foi vendido e quanto a área comporta — a leitura de quem trabalha o território."
      />
      <PainelDeIndicadores indicadores={kpisDaCarteira} carregando={carregando} />

      {indicadores && (
        <TabelaDeMunicipios
          municipios={municipios}
          daAdr={ordenados}
          foraDoMapa={foraDoMapa}
          totais={totais}
          selecionado={selecionado}
          aoSelecionar={aoSelecionar}
          territorioNaoCarregado={territorioNaoCarregado}
          semFiltro={semFiltro}
        />
      )}

      {ficha}
    </>
  );
}
