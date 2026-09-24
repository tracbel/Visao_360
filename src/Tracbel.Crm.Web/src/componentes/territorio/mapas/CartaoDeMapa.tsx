/**
 * A casca comum dos quatro cartões de mapa (issue 170 parte A; densidade na #31;
 * desenho da maquete desde 23/09/2026).
 *
 * OS QUATRO TÊM O MESMO ESQUELETO — título, resumo, mapa com a legenda ao lado e
 * alternador —, e ele está escrito uma vez só; cada mapa traz o que é dele.
 *
 * O CARTÃO TEM O TAMANHO DA MAQUETE, ~350 × 245 px numa coluna de 1490 px. Ele
 * chegava a 640 px: título com piso de duas linhas, resumo com três metadados,
 * legenda de oito linhas com a unidade por extenso, "fora da ADR", "contorno", e
 * uma linha reservada para o cursor. Nada disso foi apagado:
 *
 * - os metadados que a maquete não mostra (elegíveis, no prazo e pendentes;
 *   máquina, pós-venda e clientes; bovinos e usinas) abrem a dica do título;
 * - a unidade por extenso, o "fora da ADR" e o hachurado do sem dado também
 *   estão nela — e a unidade continua no nome acessível da legenda;
 * - a linha do cursor passou a flutuar sobre o pé do mapa, e só aparece quando
 *   há cursor: reservar a altura dela custava duas linhas em cada cartão.
 *
 * O ENQUADRAMENTO É COMPARTILHADO: os quatro recebem a mesma `ligacao`, e é por
 * isso que o mesmo município fica no mesmo lugar nos quatro e o cursor sobre ele
 * acende os quatro ao mesmo tempo.
 */

import type { LucideIcon } from 'lucide-react';
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
  /** Os municípios da ADR, desenhados por cima dos vizinhos. */
  adr: ReadonlySet<number>;
  porCodigo: ReadonlyMap<number, IndicadoresDoMunicipio>;
  nomeDoPoligono: ReadonlyMap<number, string>;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  emFoco: number | null;
  aoPassar: (codigo: number | null) => void;
};

/**
 * O RESUMO DO CARTÃO — o número que responde o cartão, na forma da maquete.
 *
 * As quatro maquetes não têm o mesmo desenho: a cobertura e as vendas põem o nome
 * ao lado do número; o potencial, embaixo, com dois metadados à direita; a
 * estrutura, ao lado, com uma segunda linha menor. As formas são poucas e
 * declaradas aqui — cada mapa escolhe a sua, e nenhum inventa uma quinta.
 */
export type ResumoDoMapa = {
  /** O número que responde o cartão. `null` mostra a frase de `semValor`. */
  valor: ReactNode | null;
  /** O que ele mede — "total no período", "tratores". */
  rotulo: ReactNode;
  /** O nome embaixo do número, e não ao lado (potencial). */
  rotuloEmbaixo?: boolean;
  /** Uma segunda linha, menor — "54.886 propriedades" (estrutura). */
  complemento?: { valor: ReactNode; rotulo: ReactNode } | null;
  /** Metadados à direita, em colunas divididas por filete (potencial). */
  meta?: readonly { valor: ReactNode; rotulo: ReactNode }[];
  /** Um selo na linha do número — a variação das vendas contra o ano anterior. */
  selo?: ReactNode;
  /** A frase de quando não há número — "sem área plantada ou regra para calcular". */
  semValor?: string;
};

export function CartaoDeMapa({
  mapa,
  id,
  titulo,
  icone: Icone,
  selo,
  metodologia,
  resumo,
  alternador,
  tituloDoMapa,
  estadoDe,
  faixas,
  unidade,
  semDado,
  ligacao,
}: {
  /** O nome do mapa para o teste de estrutura e para a cor do ícone. */
  mapa: string;
  /** Separa os padrões de hachura dos SVGs. */
  id: string;
  titulo: ReactNode;
  /** O ícone colorido à esquerda do título (maquete). */
  icone: LucideIcon;
  /**
   * O selo de como ler o indicador — "Regra provisória".
   *
   * Ele fica na PONTA DA LINHA do título (maquete), e não colado no texto: assim
   * os cartões que têm selo o mostram no mesmo lugar.
   */
  selo?: ReactNode;
  /**
   * O que o cartão não mostra e não pode perder: os números do recorte que
   * saíram do resumo, fonte, competência, método e ressalvas.
   *
   * Vai para a dica ao lado do título: continua a um toque, a um Tab e a um
   * ponteiro de distância, e não ocupa a tela de quem só quer ver o mapa.
   */
  metodologia: ReactNode;
  resumo: ResumoDoMapa;
  alternador: ReactNode;
  tituloDoMapa: string;
  estadoDe: (codigo: number) => EstadoNoMapa;
  faixas: ComponentProps<typeof LegendaDoMapa>['faixas'];
  /** A unidade por extenso — no nome acessível da legenda e na dica do título. */
  unidade: string;
  /**
   * O estado sem valor NA PALAVRA DO MAPA ("Sem cliente", "Sem valor") e o que
   * ele quer dizer ali — hachurado não é zero em nenhum dos quatro.
   */
  semDado: { rotulo: string; explicacao: string };
  ligacao: LigacaoDoMapa;
}) {
  const semValor = resumo.valor === null || resumo.valor === undefined;

  return (
    <div className="card cad-cartao terr-mapa" data-mapa={mapa}>
      <div className="terr-mapa-cabecalho">
        <span className="terr-mapa-icone" aria-hidden="true">
          <Icone size={15} strokeWidth={2} />
        </span>
        <div className="card-title">
          {titulo}
          <InfoTooltip
            rotulo="Fonte e método deste mapa"
            texto={
              <>
                {metodologia}
                {/* O QUE SAIU DA LEGENDA VISÍVEL: a unidade por extenso, o
                    hachurado e o fundo de fora da ADR. */}
                <p>
                  {`Legenda — ${unidade}. ${semDado.rotulo} (hachurado): ${semDado.explicacao}; não é zero. ` +
                    'Cinza-claro, sem valor pintado: fora da ADR — o vizinho que entra no quadro do mapa.'}
                </p>
              </>
            }
          />
        </div>
        {selo && <span className="terr-mapa-selo">{selo}</span>}
      </div>

      <div className="terr-mapa-resumo">
        {semValor ? (
          <span className="terr-mapa-unidade">{resumo.semValor ?? 'sem dado no recorte'}</span>
        ) : (
          <>
            <span className="terr-mapa-principal">
              <span className="terr-mapa-linha">
                <strong className="terr-mapa-numero">{resumo.valor}</strong>
                {!resumo.rotuloEmbaixo && <span className="terr-mapa-unidade"> {resumo.rotulo}</span>}
                {resumo.selo}
              </span>
              {resumo.rotuloEmbaixo && <span className="terr-mapa-unidade">{resumo.rotulo}</span>}
              {resumo.complemento && (
                <span className="terr-mapa-complemento">
                  <strong>{resumo.complemento.valor}</strong> {resumo.complemento.rotulo}
                </span>
              )}
            </span>

            {resumo.meta && resumo.meta.length > 0 && (
              <span className="terr-mapa-meta">
                {resumo.meta.map((m, i) => (
                  <span className="terr-mapa-meta-item" key={i}>
                    <strong>{m.valor}</strong> <span className="terr-mapa-meta-rotulo">{m.rotulo}</span>
                  </span>
                ))}
              </span>
            )}
          </>
        )}
      </div>

      {/* O MAPA À ESQUERDA E A LEGENDA À DIREITA, em coluna — como na maquete,
          em qualquer largura de cartão. */}
      <div className="terr-mapa-corpo">
        <div className="terr-mapa-desenho">
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

          {/* A LINHA DO CURSOR SÓ FALA QUANDO HÁ CURSOR (fase T2.1), e agora
              flutua sobre o pé do mapa em vez de reservar duas linhas do cartão.
              A região `aria-live` continua no documento, vazia, para o leitor de
              tela anunciar o município assim que o cursor chegar. */}
          <p className="terr-mapa-foco" aria-live="polite">
            {textoDoFoco(ligacao, estadoDe)}
          </p>
        </div>
        <LegendaDoMapa faixas={faixas} unidade={unidade} semDado={semDado.rotulo} />
      </div>

      {/* O ALTERNADOR FECHA O CARTÃO: ele é o controle, e controle vem depois do
          que ele controla. */}
      {alternador}
    </div>
  );
}

/** O detalhe do município sob o cursor, na medida de cada mapa. */
function textoDoFoco(ligacao: LigacaoDoMapa, estadoDe: (codigo: number) => EstadoNoMapa): string {
  if (ligacao.emFoco === null) return '';
  return `${ligacao.nomeDoPoligono.get(ligacao.emFoco) ?? ligacao.emFoco} — ${estadoDe(ligacao.emFoco).detalhe}`;
}
