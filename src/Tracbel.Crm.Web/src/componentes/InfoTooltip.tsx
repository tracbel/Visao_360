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
 * Cores, raio e sombra saem dos tokens do `design-system.css` — nada de cor
 * escrita à mão aqui.
 */

import { useEffect, useId, useRef, useState, type ReactNode } from 'react';

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
  const id = useId();
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
    <span className="dica" ref={raiz} onMouseEnter={() => setAberta(true)} onMouseLeave={() => setAberta(false)}>
      <button
        type="button"
        className="dica-gatilho"
        aria-label={rotulo}
        // Sem `aria-expanded`: dica não é seção que abre e fecha, é descrição.
        // O que o leitor de tela precisa é do `aria-describedby` abaixo.
        aria-describedby={aberta ? id : undefined}
        onClick={() => setAberta((estava) => !estava)}
        onFocus={() => setAberta(true)}
        onBlur={() => setAberta(false)}
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

      {aberta && (
        <span role="tooltip" id={id} className={`dica-balao dica-balao-${lado}`}>
          {texto}
        </span>
      )}
    </span>
  );
}
