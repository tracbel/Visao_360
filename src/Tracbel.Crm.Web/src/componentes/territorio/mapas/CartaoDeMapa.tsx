/**
 * A casca comum dos quatro cartões de mapa (issue 170 parte A; densidade na #31).
 *
 * OS QUATRO TÊM O MESMO ESQUELETO — título, resumo, alternador, mapa, linha do
 * cursor e legenda —, e antes ele estava escrito quatro vezes. Aqui ele está uma
 * vez só, e cada mapa traz o que é dele.
 *
 * O AVISO PERMANENTE SAIU DAQUI (fase T2.1). Cada cartão terminava num parágrafo
 * de quatro a oito linhas explicando Censo, sigilo, ANP e regra provisória — e
 * quatro deles empilhados eram mais texto do que mapa. A hierarquia da issue 33
 * diz o que fazer: nível 1 é dado operacional, nível 2 é dica. Toda essa
 * metodologia virou o `ⓘ` ao lado do título, **sem perder uma palavra**.
 *
 * O ENQUADRAMENTO É COMPARTILHADO: os quatro recebem a mesma `ligacao`, e é por
 * isso que o mesmo município fica no mesmo lugar nos quatro e o cursor sobre ele
 * acende os quatro ao mesmo tempo.
 */

import type { ComponentProps, ReactNode } from 'react';
import type { IndicadoresDoMunicipio } from '../../../tipos/territorio';
import { InfoTooltip } from '../../InfoTooltip';
import {
  LegendaDoMapa,
  MapaDeMunicipios,
  type EstadoNoMapa,
  type PoligonoProjetado,
} from '../MapaDeMunicipios';
import type { Enquadramento } from '../projecao';

/** O que os quatro mapas compartilham: o desenho, o recorte e o cursor. */
export type LigacaoDoMapa = {
  enquadramento: Enquadramento;
  poligonos: PoligonoProjetado[];
  /** Os municípios da ADR, que ganham o contorno verde. */
  adr: ReadonlySet<number>;
  porCodigo: ReadonlyMap<number, IndicadoresDoMunicipio>;
  nomeDoPoligono: ReadonlyMap<number, string>;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  emFoco: number | null;
  aoPassar: (codigo: number | null) => void;
};

export function CartaoDeMapa({
  mapa,
  id,
  titulo,
  metodologia,
  resumo,
  alternador,
  tituloDoMapa,
  estadoDe,
  faixas,
  unidade,
  ligacao,
}: {
  /** O nome do mapa para o teste de estrutura; nenhuma regra de CSS o usa. */
  mapa: string;
  /** Separa os padrões de hachura dos SVGs. */
  id: string;
  titulo: ReactNode;
  /**
   * Fonte, competência, método e ressalvas — tudo o que era parágrafo fixo.
   *
   * Vai para a dica ao lado do título: continua a um toque, a um Tab e a um
   * ponteiro de distância, e não ocupa a tela de quem só quer ver o mapa.
   */
  metodologia: string;
  resumo: ReactNode;
  alternador: ReactNode;
  tituloDoMapa: string;
  estadoDe: (codigo: number) => EstadoNoMapa;
  faixas: ComponentProps<typeof LegendaDoMapa>['faixas'];
  unidade: string;
  ligacao: LigacaoDoMapa;
}) {
  return (
    <div className="card cad-cartao terr-mapa" data-mapa={mapa}>
      <div className="card-title">
        {titulo}
        <InfoTooltip texto={metodologia} rotulo={`Fonte e método deste mapa`} />
      </div>
      <p className="terr-mapa-resumo">{resumo}</p>

      {/* O MAPA E A LEGENDA FICAM LADO A LADO — como na imagem base.

          A legenda embaixo ocupava a largura inteira do cartão e empurrava o
          alternador para fora da dobra; e as faixas ficavam em duas ou três
          linhas, cada uma num lugar diferente nos quatro cartões. À direita, em
          coluna, ela tem a altura do mapa, cabe numa coluna só e começa no mesmo
          ponto nos quatro. */}
      <div className="terr-mapa-corpo">
        <MapaDeMunicipios
          id={id}
          titulo={tituloDoMapa}
          enquadramento={ligacao.enquadramento}
          poligonos={ligacao.poligonos}
          estadoDe={estadoDe}
          adr={ligacao.adr}
          selecionado={ligacao.selecionado}
          aoSelecionar={ligacao.aoSelecionar}
          emFoco={ligacao.emFoco}
          aoPassar={ligacao.aoPassar}
        />
        <LegendaDoMapa faixas={faixas} unidade={unidade} />
      </div>

      {/* A LINHA DO CURSOR SÓ FALA QUANDO HÁ CURSOR (fase T2.1). Ela dizia
          permanentemente "Passe o cursor sobre um município para ver o número
          dele nos três mapas; clique para abrir a ficha" — instrução fixa, em
          quatro cartões, e ainda por cima errada: os mapas são QUATRO. */}
      <p className="terr-mapa-foco" aria-live="polite">{textoDoFoco(ligacao, estadoDe)}</p>

      {/* O ALTERNADOR FECHA O CARTÃO: ele é o controle, e controle vem depois do
          que ele controla. Em cima, ele era a primeira coisa abaixo do título —
          e a leitura começava por um botão em vez de por um mapa. */}
      {alternador}
    </div>
  );
}

/** O detalhe do município sob o cursor, na medida de cada mapa. */
function textoDoFoco(ligacao: LigacaoDoMapa, estadoDe: (codigo: number) => EstadoNoMapa): string {
  if (ligacao.emFoco === null) return '';
  return `${ligacao.nomeDoPoligono.get(ligacao.emFoco) ?? ligacao.emFoco} — ${estadoDe(ligacao.emFoco).detalhe}`;
}
