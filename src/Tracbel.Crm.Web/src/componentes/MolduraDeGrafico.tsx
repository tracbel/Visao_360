/**
 * A moldura que dá largura a um gráfico de tamanho fixo.
 *
 * POR QUE ELA EXISTE: os componentes de gráfico portados do protótipo recebem
 * `largura` e `altura` em pixels e desenham com `responsive: false` — é assim
 * que eles reproduzem a referência pixel a pixel, e mexer nisso mudaria o
 * desenho. O efeito colateral é que eles não sabem encolher, e as telas foram
 * portadas de uma maquete fixa em 1.280px.
 *
 * A moldura mede a própria largura e repassa. O gráfico continua de tamanho
 * fixo — só que o número agora vem do espaço que ele tem, e não de uma
 * constante. É o que permite o mesmo gráfico caber em 1.280px e em 390px sem
 * uma segunda versão dele.
 *
 * Enquanto a medida não chegou (primeira renderização, antes do layout), nada é
 * desenhado: um gráfico com largura zero estoura no Chart.js.
 */

import { useEffect, useRef, useState, type ReactNode } from 'react';

export function MolduraDeGrafico({
  altura,
  larguraMaxima,
  children,
}: {
  /** A altura do desenho, em pixels. */
  altura: number;
  /** Teto de largura, para o gráfico não esticar além do que faz sentido. */
  larguraMaxima?: number;
  /** Recebe a largura medida. */
  children: (largura: number, altura: number) => ReactNode;
}) {
  const caixa = useRef<HTMLDivElement>(null);
  const [largura, setLargura] = useState(0);

  useEffect(() => {
    const elemento = caixa.current;
    if (!elemento) return;

    function medir() {
      const l = caixa.current?.clientWidth ?? 0;
      setLargura(larguraMaxima ? Math.min(l, larguraMaxima) : l);
    }

    medir();
    const observador = new ResizeObserver(medir);
    observador.observe(elemento);
    return () => observador.disconnect();
  }, [larguraMaxima]);

  return (
    <div className="cad-moldura-grafico" ref={caixa} style={{ minHeight: altura }}>
      {largura > 0 && children(largura, altura)}
    </div>
  );
}
