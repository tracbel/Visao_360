/**
 * Um mapa de São Paulo por município, pintado por um indicador.
 *
 * O componente não sabe QUAL indicador desenha: recebe, para cada código IBGE,
 * um de três estados — fora da ADR, sem dado, ou um valor já com a cor da
 * faixa. É o que garante que os três mapas distinguem zero, ausência e fora
 * da área exatamente do mesmo jeito (ver `escalas.ts`).
 *
 * A seleção é por clique no polígono; pelo teclado, a mesma seleção se faz na
 * tabela de municípios abaixo dos mapas — 645 polígonos focáveis seriam 645
 * paradas de Tab.
 */

import { COR_BORDA_ADR, COR_FORA_DA_ADR, ID_HACHURA_SEM_DADO, type Faixa } from './escalas';
import type { Enquadramento } from './projecao';

export type EstadoNoMapa =
  | { tipo: 'fora'; detalhe: string }
  | { tipo: 'semDado'; detalhe: string }
  | { tipo: 'valor'; cor: string; detalhe: string };

export type PoligonoProjetado = { codigo: number; nome: string; caminho: string };

type Props = {
  /** Identificador do mapa — separa os padrões de hachura dos três SVGs. */
  id: string;
  titulo: string;
  enquadramento: Enquadramento;
  poligonos: PoligonoProjetado[];
  estadoDe: (codigo: number) => EstadoNoMapa;
  /** Os municípios da ADR, que ganham o contorno verde. */
  adr: ReadonlySet<number>;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
};

export function MapaDeMunicipios({ id, titulo, enquadramento, poligonos, estadoDe, adr, selecionado, aoSelecionar }: Props) {
  const hachura = `${ID_HACHURA_SEM_DADO}-${id}`;
  const escolhido = selecionado === null ? null : poligonos.find((p) => p.codigo === selecionado);

  return (
    <svg
      className="terr-svg"
      viewBox={`0 0 ${enquadramento.largura.toFixed(0)} ${enquadramento.altura.toFixed(0)}`}
      role="img"
      aria-label={titulo}
    >
      <defs>
        <pattern id={hachura} width="5" height="5" patternUnits="userSpaceOnUse" patternTransform="rotate(45)">
          <rect width="5" height="5" fill="#F3F4F6" />
          <line x1="0" y1="0" x2="0" y2="5" stroke="#9CA3AF" strokeWidth="1.6" />
        </pattern>
      </defs>

      {/* PRIMEIRO O QUE ESTÁ FORA, DEPOIS A ADR: o contorno verde da ADR precisa
          ficar por cima da borda cinza do vizinho, senão some na divisa. */}
      {[false, true].map((daAdr) =>
        poligonos
          .filter((p) => adr.has(p.codigo) === daAdr)
          .map((p) => {
            const estado = estadoDe(p.codigo);
            const preenchimento =
              estado.tipo === 'fora' ? COR_FORA_DA_ADR : estado.tipo === 'semDado' ? `url(#${hachura})` : estado.cor;
            return (
              <path
                key={p.codigo}
                d={p.caminho}
                fill={preenchimento}
                stroke={daAdr ? COR_BORDA_ADR : '#D1D5DB'}
                strokeWidth={daAdr ? 0.7 : 0.35}
                className="terr-poligono"
                onClick={() => aoSelecionar(p.codigo)}
              >
                <title>{`${p.nome} — ${estado.detalhe}`}</title>
              </path>
            );
          }),
      )}

      {escolhido && (
        <path d={escolhido.caminho} fill="none" stroke="#111827" strokeWidth={2} pointerEvents="none" />
      )}
    </svg>
  );
}

/** A legenda: as faixas da escala, e os dois estados que não são valor. */
export function LegendaDoMapa({ faixas, unidade }: { faixas: Faixa[]; unidade: string }) {
  return (
    <div className="terr-legenda" aria-label={`Legenda — ${unidade}`}>
      <span className="terr-legenda-unidade">{unidade}</span>
      {faixas.map((f) => (
        <span key={f.rotulo} className="terr-legenda-item">
          <span className="terr-amostra" style={{ background: f.cor }} />
          {f.rotulo}
        </span>
      ))}
      <span className="terr-legenda-item">
        <span className="terr-amostra terr-amostra-hachura" />
        sem dado
      </span>
      <span className="terr-legenda-item">
        <span className="terr-amostra" style={{ background: COR_FORA_DA_ADR, borderColor: '#D1D5DB' }} />
        fora da ADR
      </span>
    </div>
  );
}
