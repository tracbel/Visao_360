/**
 * A confirmação do que não se desfaz sozinho.
 *
 * REGRA DO DOCUMENTO 05 §3: nada destrutivo sem confirmação. Aqui "destrutivo"
 * é inativar — que no nosso banco é exclusão LÓGICA, nada é apagado —, e a
 * confirmação existe porque o registro sai da lista e some do trabalho de todo
 * mundo até alguém reativar.
 *
 * TRÊS COISAS QUE ESTE DIÁLOGO FAZ, e que uma confirmação apressada não faz:
 *
 * 1. **Diz o nome do que vai ser inativado.** "Confirma?" sozinho não deixa a
 *    pessoa perceber que clicou na linha errada.
 * 2. **Prende o foco e devolve ao fechar**, e fecha com `Esc`. Quem usa teclado
 *    não fica preso atrás de uma janela que não tem como sair.
 * 3. **Aceita um formulário dentro.** O motivo da inativação de cliente é de
 *    catálogo e obrigatório: pedi-lo depois de confirmar seria uma segunda
 *    janela; pedi-lo antes, uma tela inteira para um campo.
 */

import { useEffect, useRef, type ReactNode } from 'react';

type Props = {
  titulo: string;
  /** A linha abaixo do título: o nome do registro. */
  subtitulo?: string;
  /** A explicação do que a ação faz e do que ela não faz. */
  children?: ReactNode;
  rotuloConfirmar: string;
  /** Desliga o botão de confirmar enquanto falta preencher algo. */
  podeConfirmar?: boolean;
  /** Verdadeiro enquanto a requisição está em andamento. */
  gravando?: boolean;
  aoCancelar: () => void;
  aoConfirmar: () => void;
};

export function DialogoConfirmacao({
  titulo,
  subtitulo,
  children,
  rotuloConfirmar,
  podeConfirmar = true,
  gravando = false,
  aoCancelar,
  aoConfirmar,
}: Props) {
  const painel = useRef<HTMLDivElement>(null);
  const focoAnterior = useRef<Element | null>(null);

  useEffect(() => {
    focoAnterior.current = document.activeElement;

    // O FOCO VAI PARA O QUE A PESSOA PRECISA PREENCHER, não para o "×" de
    // fechar, que é o primeiro na ordem do documento. Abrir um diálogo com o
    // foco no botão de fechar convida ao gesto contrário do que ela pediu.
    const corpo = painel.current?.querySelector('.mfi-body');
    const primeiroCampo = corpo?.querySelector<HTMLElement>('select, input, textarea');
    const primeiroBotao = painel.current?.querySelector<HTMLElement>('.mfi-actions button');
    (primeiroCampo ?? primeiroBotao)?.focus();

    function aoTeclar(evento: KeyboardEvent) {
      if (evento.key === 'Escape') {
        evento.stopPropagation();
        aoCancelar();
        return;
      }
      if (evento.key !== 'Tab' || !painel.current) return;

      // Prende a tabulação dentro do diálogo: sair dele com o teclado deixaria o
      // foco numa tela coberta, e a pessoa clicaria no escuro.
      const focaveis = [
        ...painel.current.querySelectorAll<HTMLElement>(
          'button:not([disabled]), select, input, textarea, [href], [tabindex]:not([tabindex="-1"])',
        ),
      ];
      if (focaveis.length === 0) return;
      const primeiro = focaveis[0];
      const ultimo = focaveis[focaveis.length - 1];

      if (evento.shiftKey && document.activeElement === primeiro) {
        evento.preventDefault();
        ultimo.focus();
      } else if (!evento.shiftKey && document.activeElement === ultimo) {
        evento.preventDefault();
        primeiro.focus();
      }
    }

    document.addEventListener('keydown', aoTeclar, true);
    return () => {
      document.removeEventListener('keydown', aoTeclar, true);
      (focoAnterior.current as HTMLElement | null)?.focus?.();
    };
  }, [aoCancelar]);

  return (
    <div
      className="modal-ficha-indispo-overlay"
      onClick={(e) => {
        if (e.target === e.currentTarget) aoCancelar();
      }}
    >
      <div
        className="modal-ficha-indispo"
        role="dialog"
        aria-modal="true"
        aria-label={titulo}
        ref={painel}
      >
        <div className="mfi-header">
          <div className="mfi-icon" style={{ background: '#FEF3C7', color: '#B45309' }}>
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2.2" aria-hidden="true">
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
              <path d="M12 9v4" />
              <path d="M12 17h.01" />
            </svg>
          </div>
          <div>
            <div className="mfi-title">{titulo}</div>
            {subtitulo && <div className="mfi-sub">{subtitulo}</div>}
          </div>
          <button className="mfi-close" onClick={aoCancelar} title="Fechar" type="button">
            ×
          </button>
        </div>
        <div className="mfi-body">{children}</div>
        <div className="mfi-actions">
          <button className="btn-cancelar" onClick={aoCancelar} type="button" disabled={gravando}>
            Cancelar
          </button>
          <button
            className="btn-limpar-confirmar"
            onClick={aoConfirmar}
            type="button"
            disabled={!podeConfirmar || gravando}
          >
            {gravando ? 'Gravando…' : rotuloConfirmar}
          </button>
        </div>
      </div>
    </div>
  );
}
