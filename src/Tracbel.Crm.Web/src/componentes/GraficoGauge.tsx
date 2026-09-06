/**
 * Medidor semicircular — percentual sobre faixas de leitura.
 *
 * ---------------------------------------------------------------------------
 * 06/09/2026 — A GEOMETRIA FOI REFEITA. O que havia aqui era o porte fiel de
 * `drawGauge()` do protótipo (`prototipo/referencia/assets/app.js`, linha 964),
 * e ele tinha três defeitos que só apareceram com número real na tela:
 *
 * 1. **Os rótulos ficavam DENTRO do arco**, no mesmo lugar onde o valor é
 *    escrito. Com "64.1%" em 28px, o "0%" e o "20%" sumiam atrás do número.
 * 2. **O arco era o semicírculo da esquerda** — nascia embaixo, subia pela
 *    esquerda e terminava em cima. Medidor não se lê assim: o percurso
 *    esperado é da esquerda para a direita, passando por cima.
 * 3. **O centro caía fora da moldura** (cy 140 + r 100 = 240, num viewBox de
 *    180), então a parte de baixo do círculo era cortada pela borda. O
 *    recorte não aparecia porque o arco daquele lado não era desenhado, mas
 *    qualquer mudança de raio revelava o problema.
 *
 * Agora: semicírculo de 180°, da esquerda para a direita passando por cima;
 * rótulos POR FORA do arco; e o valor embaixo, no espaço que sobra do
 * semicírculo, onde nada mais é desenhado.
 *
 * Continua em `<canvas>` desenhado à mão, e não em Chart.js, porque a
 * geometria é fixa e proporcional: tudo é escalado por `largura / VB_LARGURA`,
 * então o mesmo desenho serve 260px e 520px sem uma segunda versão.
 */
import { useEffect, useRef } from 'react';

/** Uma faixa de leitura: até `max` por cento, desta cor. */
export type FaixaDoMedidor = { max: number; cor: string };

type Props = {
  /** De 0 a 100. */
  valor: number;
  faixas: FaixaDoMedidor[];
  corPonteiro?: string;
  corPivo?: string;
};

const COR_PONTEIRO_PADRAO = '#1A2420'; // var(--text-primary)
const COR_PIVO_PADRAO = '#FFDE00'; // var(--jd-yellow)
const COR_TICK = '#7A857D'; // var(--text-tertiary)

/*
 * Geometria, em unidades do viewBox. O centro fica na base do semicírculo, e a
 * altura é o raio mais o espaço dos rótulos de cima e do número de baixo — não
 * sobra borda para cortar nada.
 */
const VB_LARGURA = 280;
const VB_ALTURA = 190;
const CX = 140;
const CY = 128;
const R = 92;
/** Espessura do arco. */
const TRACO = 16;
/** A que distância do arco ficam os rótulos de porcentagem. */
const FOLGA_TICK = 20;

export function GraficoGauge({
  valor,
  faixas,
  corPonteiro = COR_PONTEIRO_PADRAO,
  corPivo = COR_PIVO_PADRAO,
}: Props) {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    function desenhar() {
      const canvasEl = canvasRef.current;
      if (!canvasEl) return;
      const largura = canvasEl.clientWidth;
      const altura = canvasEl.clientHeight;
      if (largura === 0 || altura === 0) return;

      const dpr = window.devicePixelRatio || 1;
      canvasEl.width = Math.round(largura * dpr);
      canvasEl.height = Math.round(altura * dpr);
      const ctx = canvasEl.getContext('2d');
      if (!ctx) return;
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
      ctx.clearRect(0, 0, largura, altura);

      const escala = largura / VB_LARGURA;
      const cx = CX * escala;
      const cy = CY * escala;
      const r = R * escala;

      /*
       * No canvas, 0 rad aponta para a direita e o ângulo cresce no sentido
       * horário (o eixo y cresce para baixo). O semicírculo de cima é, então,
       * de π (esquerda) a 2π (direita), passando por 3π/2 (topo) — e a fração
       * do valor caminha nesse mesmo sentido.
       */
      const anguloDe = (pct: number) => Math.PI + (Math.min(100, Math.max(0, pct)) / 100) * Math.PI;

      // Faixas de leitura, uma atrás da outra.
      ctx.lineCap = 'butt';
      ctx.lineWidth = TRACO * escala;
      let anterior = 0;
      faixas.forEach((faixa) => {
        ctx.beginPath();
        ctx.arc(cx, cy, r, anguloDe(anterior), anguloDe(faixa.max));
        ctx.strokeStyle = faixa.cor;
        ctx.stroke();
        anterior = faixa.max;
      });

      // Rótulos POR FORA do arco: dentro eles disputariam espaço com o valor.
      ctx.font = `${10 * escala}px Inter, sans-serif`;
      ctx.fillStyle = COR_TICK;
      ctx.textBaseline = 'middle';
      const raioTick = r + (TRACO / 2 + FOLGA_TICK) * escala;
      for (let v = 0; v <= 100; v += 25) {
        const ang = anguloDe(v);
        ctx.textAlign = 'center';
        if (v === 0 || v === 100) {
          // AS PONTAS FICAM EMBAIXO DO ARCO, e não na linha dele.
          // Alinhadas por fora, na horizontal, elas ocupam o mesmo lugar onde o
          // arco termina — o "100%" encostava na ponta verde. Embaixo da ponta
          // é onde um medidor põe o mínimo e o máximo, e ali não há nada.
          const xPonta = cx + (v === 0 ? -r : r);
          ctx.fillText(`${v}%`, xPonta, cy + 16 * escala);
        } else {
          ctx.fillText(`${v}%`, cx + raioTick * Math.cos(ang), cy + raioTick * Math.sin(ang));
        }
      }

      // Ponteiro.
      const angValor = anguloDe(valor);
      const raioPonteiro = r - (TRACO / 2 + 6) * escala;
      ctx.beginPath();
      ctx.moveTo(cx, cy);
      ctx.lineTo(cx + raioPonteiro * Math.cos(angValor), cy + raioPonteiro * Math.sin(angValor));
      ctx.strokeStyle = corPonteiro;
      ctx.lineWidth = 3 * escala;
      ctx.lineCap = 'round';
      ctx.stroke();

      ctx.beginPath();
      ctx.arc(cx, cy, 8 * escala, 0, Math.PI * 2);
      ctx.fillStyle = corPonteiro;
      ctx.fill();

      ctx.beginPath();
      ctx.arc(cx, cy, 3.5 * escala, 0, Math.PI * 2);
      ctx.fillStyle = corPivo;
      ctx.fill();

      // O valor, embaixo do eixo — a metade de baixo do círculo não é desenhada.
      ctx.font = `700 ${26 * escala}px Inter, sans-serif`;
      ctx.fillStyle = COR_PONTEIRO_PADRAO;
      ctx.textAlign = 'center';
      ctx.textBaseline = 'alphabetic';
      ctx.fillText(formatar(valor), cx, cy + 42 * escala);
    }

    desenhar();
    const ro = new ResizeObserver(desenhar);
    ro.observe(canvas);
    return () => ro.disconnect();
  }, [valor, faixas, corPonteiro, corPivo]);

  return (
    <canvas
      ref={canvasRef}
      style={{ display: 'block', width: '100%', aspectRatio: `${VB_LARGURA} / ${VB_ALTURA}` }}
      role="img"
      aria-label={`Medidor em ${formatar(valor)}`}
    />
  );
}

/** Uma casa decimal, com vírgula — e sem a casa quando ela é zero. */
function formatar(valor: number): string {
  return `${valor.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;
}
