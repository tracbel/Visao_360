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
 *
 * MEDIDA NOVA, GRÁFICO NOVO (29/09/2026): com `responsive: false`, o Chart.js
 * guarda o tamanho com que nasceu. Quando a medida muda depois, o
 * `react-chartjs-2` só troca os atributos `width`/`height` do canvas, e o
 * Chart.js continua desenhando e lendo o ponteiro no tamanho antigo: o desenho
 * sai cortado e o balão não aparece mais. Na Visão 360 isso acontecia quando o
 * funil chegava e o alerta dos processos parados esticava a linha do
 * faturamento. Por isso o gráfico é montado de novo a cada medida (`key`).
 */

import { Fragment, useEffect, useRef, useState, type ReactNode } from 'react';

export function MolduraDeGrafico({
  altura,
  larguraMaxima,
  preencher = false,
  children,
}: {
  /** A altura do desenho, em pixels. Com `preencher`, é a altura MÍNIMA. */
  altura: number;
  /** Teto de largura, para o gráfico não esticar além do que faz sentido. */
  larguraMaxima?: number;
  /**
   * O GRÁFICO ENCHE A ALTURA QUE SOBRA no painel (29/09/2026, a Visão 360 da maquete): num painel esticado até o vizinho
   * da linha, o desenho cresce até o fundo em vez de deixar uma faixa vazia embaixo. O desenho fica numa camada
   * absoluta — assim ele não empurra a altura do painel, e a linha pode voltar a encolher quando o vizinho encolhe.
   */
  preencher?: boolean;
  /** Recebe a largura medida (e, com `preencher`, a altura medida). */
  children: (largura: number, altura: number) => ReactNode;
}) {
  const caixa = useRef<HTMLDivElement>(null);
  const [medida, setMedida] = useState({ largura: 0, altura });

  useEffect(() => {
    const elemento = caixa.current;
    if (!elemento) return;

    function medir() {
      const l = caixa.current?.clientWidth ?? 0;
      const a = preencher ? Math.max(altura, Math.floor(caixa.current?.clientHeight ?? 0)) : altura;
      setMedida({ largura: larguraMaxima ? Math.min(l, larguraMaxima) : l, altura: a });
    }

    medir();
    const observador = new ResizeObserver(medir);
    observador.observe(elemento);
    return () => observador.disconnect();
  }, [larguraMaxima, preencher, altura]);

  const alturaDoDesenho = preencher ? medida.altura : altura;
  const desenho = (
    <Fragment key={`${medida.largura}x${alturaDoDesenho}`}>{children(medida.largura, alturaDoDesenho)}</Fragment>
  );

  return (
    <div
      className="cad-moldura-grafico"
      data-preencher={preencher ? 'true' : undefined}
      ref={caixa}
      style={{ minHeight: altura }}
    >
      {medida.largura > 0 &&
        (preencher ? <div className="cad-moldura-grafico-camada">{desenho}</div> : desenho)}
    </div>
  );
}
