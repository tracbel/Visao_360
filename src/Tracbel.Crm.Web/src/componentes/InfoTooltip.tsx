/**
 * A DICA QUE EXPLICA UM NÚMERO — o que o `title=` do navegador não consegue ser.
 *
 * O CRM tem 27 `title=` espalhados, e eles falham justamente onde a explicação
 * importa:
 *
 * 1. **Ninguém alcança pelo teclado.** O `title` só aparece com o ponteiro
 *    parado sobre o elemento; quem navega por Tab nunca vê o texto.
 * 2. **Some no toque.** No celular e no tablet não há "parar o ponteiro sobre",
 *    e a dica simplesmente não existe.
 * 3. **Demora e desaparece.** O navegador escolhe quando mostrar e por quanto
 *    tempo, e o texto longo — que é o nosso caso, porque explicamos fonte, ano e
 *    regra de cálculo — é cortado.
 *
 * Esta dica abre com o ponteiro, com o foco do teclado e com um toque, fecha com
 * `Esc`, e o balão é ligado ao gatilho por `aria-describedby`: o leitor de tela
 * anuncia a explicação junto com o botão, em vez de deixá-la fora do caminho.
 *
 * **O balão só existe enquanto está aberto.** Manter um texto escondido no
 * documento faria o leitor de tela lê-lo fora de hora; `aria-describedby` só
 * aponta para algo que está na tela.
 *
 * ============================================================================
 * O POSICIONAMENTO É DO RADIX; O COMPORTAMENTO CONTINUA NOSSO (fase T4.6).
 *
 * POR QUE MUDOU. A conferência visual da T4.5 MEDIU o defeito: o balão era
 * posicionado só por CSS (`left: 50%` num pai `position: relative`), sem
 * detecção de colisão e sem portal. Junto da borda direita ele vazava a janela —
 * 2016px numa tela de 1920, 472px numa de 390 — e dentro de um cartão com
 * `overflow:hidden` ele chegava a ser RECORTADO. O texto que explica o número
 * desaparecia justamente para quem tinha ido procurá-lo.
 *
 * O QUE O RADIX FAZ AQUI, e só isto: portal, detecção de colisão, `sideOffset`,
 * folga da viewport e o cálculo da posição. É o problema que ele resolve há anos
 * e que não vale reimplementar.
 *
 * O QUE ELE NÃO FAZ, DE PROPÓSITO: abrir e fechar. O Radix Tooltip é
 * **ponteiro e foco por design** — a documentação dele manda usar Popover para
 * clique e toque. O item 2 lá em cima é requisito declarado deste componente, e
 * entregar a abertura ao Radix seria trocar um defeito de posição por uma
 * regressão no celular. Por isso `open` é CONTROLADO por nós e `onOpenChange`
 * não é passado: o Radix nunca muda o estado, só desenha onde cabe.
 *
 * A API PÚBLICA NÃO MUDOU. As dezenas de chamadas existentes continuam iguais, e
 * `lado` segue significando o que significava.
 * ============================================================================
 */

import * as Dica from '@radix-ui/react-tooltip';
import { useEffect, useRef, useState, type ReactNode } from 'react';

type Props = {
  /** A explicação. Frase inteira, com fonte e ano quando for número. */
  texto: string;
  /**
   * O que o leitor de tela anuncia ao chegar no gatilho. O padrão serve para a
   * dica de um rótulo; quando houver mais de uma na mesma tela, diga qual é
   * ("Como a cobertura é calculada").
   */
  rotulo?: string;
  /** De que lado o balão nasce. Padrão: abaixo do gatilho. */
  lado?: 'acima' | 'abaixo';
  /** Outro gatilho no lugar do "i" redondo — um rótulo, por exemplo. */
  children?: ReactNode;
};

export function InfoTooltip({ texto, rotulo = 'Mais informação', lado = 'abaixo', children }: Props) {
  const [aberta, setAberta] = useState(false);
  const raiz = useRef<HTMLSpanElement>(null);

  // CLICAR FORA FECHA. Sem isto, a dica aberta por toque ficaria na tela até a
  // pessoa achar de novo o mesmo botão — e num celular ela some da vista assim
  // que o dedo rola a página.
  useEffect(() => {
    if (!aberta) return;

    function aoApontarFora(evento: PointerEvent | MouseEvent) {
      if (!raiz.current?.contains(evento.target as Node)) setAberta(false);
    }

    document.addEventListener('mousedown', aoApontarFora);
    document.addEventListener('touchstart', aoApontarFora as EventListener);
    return () => {
      document.removeEventListener('mousedown', aoApontarFora);
      document.removeEventListener('touchstart', aoApontarFora as EventListener);
    };
  }, [aberta]);

  return (
    // `delayDuration={0}` e `disableHoverableContent`: o atraso e a ponte até o
    // balão são comportamento, e comportamento aqui é nosso — o Radix só desenha.
    <Dica.Provider delayDuration={0} disableHoverableContent>
      <Dica.Root open={aberta}>
        <span className="dica" ref={raiz} onMouseEnter={() => setAberta(true)} onMouseLeave={() => setAberta(false)}>
          <Dica.Trigger asChild>
            <button
              type="button"
              className="dica-gatilho"
              aria-label={rotulo}
              // Sem `aria-expanded`: dica não é seção que abre e fecha, é descrição.
              // O que o leitor de tela precisa é do `aria-describedby`, que o Radix
              // põe no gatilho enquanto o balão existe.
              onClick={() => setAberta((estava) => !estava)}
              onFocus={() => setAberta(true)}
              onBlur={() => setAberta(false)}
              // O RADIX FECHARIA NO `pointerdown`, e isso apagaria o toque: no
              // celular o dedo dispara `pointerdown` antes do `click`, e a dica
              // fecharia no mesmo gesto que a abriu.
              onPointerDown={(evento) => evento.preventDefault()}
              onKeyDown={(evento) => {
                if (evento.key !== 'Escape' || !aberta) return;

                // A tecla para de subir: `Esc` numa dica dentro de um diálogo fecharia
                // a dica e o diálogo junto.
                evento.stopPropagation();
                setAberta(false);
              }}
            >
              {children ?? (
                <svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="currentColor" strokeWidth={2.4} aria-hidden="true">
                  <circle cx="12" cy="12" r="10" />
                  <path d="M12 16v-5" />
                  <path d="M12 8h.01" />
                </svg>
              )}
            </button>
          </Dica.Trigger>

          {/* O PORTAL É O QUE TIRA O BALÃO DE DENTRO DO CARTÃO: sem ele, um
              ancestral com `overflow:hidden` recorta a explicação. */}
          <Dica.Portal>
            <Dica.Content
              className="dica-balao"
              side={lado === 'acima' ? 'top' : 'bottom'}
              align="center"
              sideOffset={6}
              // A FOLGA DA JANELA É O CONSERTO MEDIDO: com ela o balão vira para o
              // outro lado, ou desliza, antes de encostar na borda.
              collisionPadding={8}
            >
              {texto}
            </Dica.Content>
          </Dica.Portal>
        </span>
      </Dica.Root>
    </Dica.Provider>
  );
}
