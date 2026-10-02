/**
 * O MAPA DO IOC — a ADR pintada pela classe de prioridade de cada município (28/09/2026; desenho da maquete em
 * 02/10/2026: o zoom no canto e a legenda com a faixa de cada classe).
 *
 * É O MESMO MAPA DOS INDICADORES GEOGRÁFICOS (`MapaDeMunicipios`, a mesma malha do IBGE e o mesmo enquadramento na
 * ADR), com a mesma regra de três aparências: fora da ADR é fundo, sem IOC é hachurado, e o resto tem a cor da classe.
 * Clicar num município abre a ficha dele ao lado — a mesma escolha que o campo Município e a tabela fazem.
 *
 * O ZOOM É O ENQUADRAMENTO, e não uma lupa sobre a imagem: o "+" aproxima o centro da vista, o "−" afasta, e com o mapa
 * aproximado arrastar move a vista. O traço continua fino porque não escala com o desenho.
 */

import { Map as IconeMapa, Minus, Plus } from 'lucide-react';
import { useRef, useState, type PointerEvent as EventoDePonteiro } from 'react';
import { MapaDeMunicipios, type EstadoNoMapa, type PoligonoProjetado } from '../territorio/MapaDeMunicipios';
import type { Enquadramento } from '../territorio/projecao';
import type { MunicipioNoDiagnostico } from '../../tipos/mercado';
import { CLASSES, COR_DA_CLASSE, n, ROTULO_DA_CLASSE } from './diagnostico';

const ESCALAS = [1, 1.5, 2, 3, 4] as const;

export function MapaDoIoc({
  desenho,
  porCodigo,
  adr,
  acesos,
  selecionado,
  aoSelecionar,
}: {
  desenho: { enquadramento: Enquadramento; poligonos: PoligonoProjetado[] } | null;
  porCodigo: ReadonlyMap<number, MunicipioNoDiagnostico>;
  adr: ReadonlySet<number>;
  /**
   * Os municípios do recorte da tela — a classe, a cultura e o CEN escolhidos. Os outros ficam apagados, sem sair do
   * mapa. Nulo é nenhum recorte.
   */
  acesos: ReadonlySet<number> | null;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
}) {
  const [nivel, setNivel] = useState(0);
  const [centro, setCentro] = useState<{ x: number; y: number } | null>(null);
  const area = useRef<HTMLDivElement>(null);
  const arraste = useRef<{ x: number; y: number; centro: { x: number; y: number }; moveu: boolean } | null>(null);

  const estadoDe = (codigo: number): EstadoNoMapa => {
    const m = porCodigo.get(codigo);
    if (!m) return { tipo: 'fora', detalhe: 'Fora da área de atuação' };
    if (m.ioc === null || m.classe === null) return { tipo: 'semDado', detalhe: `${m.nome}: sem IOC — nenhum componente com dado` };
    const apagado = acesos !== null && !acesos.has(codigo);
    return {
      tipo: 'valor',
      cor: apagado ? '#E4E8E4' : COR_DA_CLASSE[m.classe],
      detalhe: `${m.nome}: IOC ${n(m.ioc)} · ${ROTULO_DA_CLASSE[m.classe]}`,
    };
  };

  const escala = ESCALAS[nivel];
  const largura = desenho?.enquadramento.largura ?? 0;
  const altura = desenho?.enquadramento.altura ?? 0;

  // A VISTA NUNCA SAI DO DESENHO: o centro é preso para a janela caber inteira dentro dele.
  const recortar = (c: { x: number; y: number }) => {
    const w = largura / escala;
    const h = altura / escala;
    return { x: Math.min(Math.max(c.x - w / 2, 0), largura - w), y: Math.min(Math.max(c.y - h / 2, 0), altura - h), largura: w, altura: h };
  };
  const vista = recortar(centro ?? { x: largura / 2, y: altura / 2 });

  function aproximar(passo: 1 | -1) {
    const proximo = Math.min(Math.max(nivel + passo, 0), ESCALAS.length - 1);
    if (proximo === 0) setCentro(null);
    else setCentro({ x: vista.x + vista.largura / 2, y: vista.y + vista.altura / 2 });
    setNivel(proximo);
  }

  function comecar(e: EventoDePonteiro<HTMLDivElement>) {
    if (nivel === 0) return;
    arraste.current = { x: e.clientX, y: e.clientY, centro: { x: vista.x + vista.largura / 2, y: vista.y + vista.altura / 2 }, moveu: false };
  }

  function mover(e: EventoDePonteiro<HTMLDivElement>) {
    const a = arraste.current;
    const caixa = area.current?.querySelector('svg')?.getBoundingClientRect();
    if (!a || !caixa || caixa.width === 0) return;
    const dx = e.clientX - a.x;
    const dy = e.clientY - a.y;
    if (!a.moveu && Math.hypot(dx, dy) < 4) return;
    a.moveu = true;
    const porPixel = vista.largura / caixa.width;
    const novo = recortar({ x: a.centro.x - dx * porPixel, y: a.centro.y - dy * porPixel });
    setCentro({ x: novo.x + novo.largura / 2, y: novo.y + novo.altura / 2 });
  }

  return (
    <div className="card cad-cartao terr-mapa diag-mapa" data-bloco="mapa-do-ioc">
      <div className="terr-mapa-cabecalho">
        <span className="terr-mapa-icone" aria-hidden="true">
          <IconeMapa size={16} strokeWidth={2} />
        </span>
        <div className="card-title">Onde está a oportunidade</div>
      </div>
      {desenho ? (
        <div className="diag-mapa-corpo">
          <div
            className="diag-mapa-area"
            ref={area}
            data-arrastavel={nivel > 0 ? 'true' : undefined}
            onPointerDown={comecar}
            onPointerMove={mover}
            onPointerUp={() => window.setTimeout(() => (arraste.current = null), 0)}
            onPointerLeave={() => (arraste.current = null)}
            // DEPOIS DE ARRASTAR, O CLIQUE NÃO ESCOLHE: soltar o botão em cima de um município seria escolhê-lo sem querer.
            onClickCapture={(e) => {
              if (arraste.current?.moveu) e.stopPropagation();
            }}
          >
            <div className="diag-zoom" role="group" aria-label="Zoom do mapa">
              <button type="button" onClick={() => aproximar(1)} disabled={nivel === ESCALAS.length - 1} aria-label="Aproximar o mapa">
                <Plus size={15} strokeWidth={2.2} aria-hidden="true" />
              </button>
              <button type="button" onClick={() => aproximar(-1)} disabled={nivel === 0} aria-label="Afastar o mapa">
                <Minus size={15} strokeWidth={2.2} aria-hidden="true" />
              </button>
            </div>
            <MapaDeMunicipios
              id="ioc"
              titulo="Municípios da ADR pela classe de prioridade do IOC"
              enquadramento={desenho.enquadramento}
              poligonos={desenho.poligonos}
              estadoDe={estadoDe}
              adr={adr}
              selecionado={selecionado}
              aoSelecionar={aoSelecionar}
              recorte={nivel === 0 ? undefined : vista}
            />
          </div>
          <ul className="diag-legenda" aria-label="Legenda — classe de prioridade">
            {CLASSES.map((c) => (
              <li key={c.chave}>
                <i style={{ background: c.cor }} aria-hidden="true" />
                <span>
                  {c.rotulo}
                  <br />({c.faixa})
                </span>
              </li>
            ))}
            <li>
              <i className="diag-legenda-sem-ioc" aria-hidden="true" />
              <span>Sem dados</span>
            </li>
          </ul>
        </div>
      ) : (
        <p className="diag-nota">Carregando a malha dos municípios…</p>
      )}
    </div>
  );
}
