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
 * paradas de Tab. O FOCO (cursor sobre o polígono) é compartilhado pelos três
 * mapas: o mesmo município ganha contorno tracejado nos três, para comparar.
 */

import { COR_BORDA_ADR, COR_BORDA_FORA_DA_ADR, COR_FORA_DA_ADR, ID_HACHURA_SEM_DADO, type Faixa } from './escalas';
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
  /** Os municípios da ADR, desenhados por cima dos vizinhos, com a divisa em tom da própria cor. */
  adr: ReadonlySet<number>;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  /** O município sob o cursor, em qualquer um dos mapas. */
  emFoco?: number | null;
  aoPassar?: (codigo: number | null) => void;
};

export function MapaDeMunicipios({
  id,
  titulo,
  enquadramento,
  poligonos,
  estadoDe,
  adr,
  selecionado,
  aoSelecionar,
  emFoco = null,
  aoPassar,
}: Props) {
  const hachura = `${ID_HACHURA_SEM_DADO}-${id}`;
  const escolhido = selecionado === null ? null : poligonos.find((p) => p.codigo === selecionado);
  const focado = emFoco === null || emFoco === selecionado ? null : poligonos.find((p) => p.codigo === emFoco);

  return (
    <svg
      className="terr-svg"
      viewBox={`0 0 ${enquadramento.largura.toFixed(0)} ${enquadramento.altura.toFixed(0)}`}
      role="img"
      aria-label={titulo}
      onMouseLeave={() => aoPassar?.(null)}
    >
      <defs>
        <pattern id={hachura} width="5" height="5" patternUnits="userSpaceOnUse" patternTransform="rotate(45)">
          <rect width="5" height="5" fill="#F3F4F6" />
          <line x1="0" y1="0" x2="0" y2="5" stroke="#9CA3AF" strokeWidth="1.6" />
        </pattern>
      </defs>

      {/* PRIMEIRO O QUE ESTÁ FORA, DEPOIS A ADR: a divisa da ADR precisa ficar
          por cima da borda clara do vizinho, senão some na fronteira.

          O TRAÇO NÃO ESCALA COM O ENQUADRAMENTO (`non-scaling-stroke`): a ADR
          enquadrada enche o quadro, e um traço em unidades do desenho engrossaria
          ou sumiria conforme o tamanho da área. Em pixel de tela ele fica fino
          como o da maquete em qualquer cartão. */}
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
                stroke={daAdr ? COR_BORDA_ADR : COR_BORDA_FORA_DA_ADR}
                strokeWidth={daAdr ? 0.6 : 0.5}
                vectorEffect="non-scaling-stroke"
                className="terr-poligono"
                onClick={() => aoSelecionar(p.codigo)}
                onMouseEnter={() => aoPassar?.(p.codigo)}
              >
                <title>{`${p.nome} — ${estado.detalhe}`}</title>
              </path>
            );
          }),
      )}

      {focado && (
        <path
          d={focado.caminho}
          fill="none"
          stroke="#111827"
          strokeWidth={1.4}
          strokeDasharray="3 2"
          vectorEffect="non-scaling-stroke"
          pointerEvents="none"
        />
      )}
      {escolhido && (
        <path d={escolhido.caminho} fill="none" stroke="#111827" strokeWidth={2} vectorEffect="non-scaling-stroke" pointerEvents="none" />
      )}
    </svg>
  );
}

/**
 * A LEGENDA DA MAQUETE: as faixas em coluna, da MAIOR para a menor, e o "sem
 * dado" por último (fidelidade às maquetes, 23/09/2026).
 *
 * O QUE SAIU DA LEGENDA VISÍVEL foi para a dica do título do mapa, inteiro: a
 * linha longa da unidade, "fora da ADR" e "contorno: município da ADR". A unidade
 * continua no NOME ACESSÍVEL da legenda — o leitor de tela anuncia "Legenda — %
 * dos vínculos elegíveis…" ao chegar nela.
 *
 * O "SEM DADO" CONTINUA HACHURADO, e não cinza liso como na maquete. A regra
 * desta tela é que zero, sem dado e fora da ADR têm aparência própria (ver
 * `escalas.ts`): no mapa o sem dado é hachurado — sigilo do IBGE não é zero —, e
 * o cinza liso é o fundo de fora da ADR. Uma amostra lisa aqui diria que o fundo
 * é "sem dado".
 */
export function LegendaDoMapa({
  faixas,
  unidade,
  semDado = 'Sem dado',
}: {
  faixas: Faixa[];
  unidade: string;
  /** O nome do estado sem valor, na palavra do mapa — "Sem cliente", "Sem valor". */
  semDado?: string;
}) {
  return (
    <div className="terr-legenda" role="group" aria-label={`Legenda — ${unidade}`}>
      {[...faixas].reverse().map((f) => (
        <span key={f.rotulo} className="terr-legenda-item">
          <span className="terr-amostra" style={{ background: f.cor }} />
          {f.rotulo}
        </span>
      ))}
      <span className="terr-legenda-item">
        <span className="terr-amostra terr-amostra-hachura" />
        {semDado}
      </span>
    </div>
  );
}
