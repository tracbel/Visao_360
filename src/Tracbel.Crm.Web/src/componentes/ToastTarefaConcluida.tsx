/**
 * Aviso de que a tarefa foi concluída, com desfazer.
 *
 * Mesmo padrão visual do `ToastNovaOportunidade` (porte de `mostrarToastNovaOp()`,
 * `prototipo/referencia/assets/app.js` linha 7057) e as mesmas classes de
 * `design-system.css` — `.novaop-toast`, `.nt-icon`, `.nt-title`, `.nt-sub`,
 * `.nt-hint`. O que este acrescenta é o que o documento 05 §3 pede junto com a
 * confirmação: **o que der para desfazer, desfaz**. A janela de desfazer é
 * visível e contada em segundos, para o usuário saber que ela existe e quanto
 * tempo tem.
 */
import { useEffect, useState } from 'react';

const SEGUNDOS_PARA_DESFAZER = 8;

type Props = {
  titulo: string;
  detalhe: string;
  /** A linha que diz o que aconteceu com o registro. Padrão: o caso da Agenda. */
  dica?: string;
  aoDesfazer: () => void;
  aoFechar: () => void;
};

export function ToastTarefaConcluida({
  titulo,
  detalhe,
  dica = 'Saiu da lista de pendentes e ficou salva no navegador.',
  aoDesfazer,
  aoFechar,
}: Props) {
  const [visivel, setVisivel] = useState(false);
  const [restam, setRestam] = useState(SEGUNDOS_PARA_DESFAZER);

  useEffect(() => {
    const id = setTimeout(() => setVisivel(true), 50);
    return () => clearTimeout(id);
  }, []);

  useEffect(() => {
    const id = setInterval(() => setRestam((s) => s - 1), 1000);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    if (restam <= 0) aoFechar();
  }, [restam, aoFechar]);

  return (
    <div className={visivel ? 'novaop-toast show' : 'novaop-toast'} role="status" aria-live="polite">
      <div className="nt-icon">
        <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2.5">
          <polyline points="20 6 9 17 4 12" />
        </svg>
      </div>
      <div className="nt-body">
        <div className="nt-title">{titulo}</div>
        <div className="nt-sub">{detalhe}</div>
        <div className="nt-hint">{dica}</div>
        <button type="button" className="nt-desfazer" onClick={aoDesfazer}>
          Desfazer ({restam}s)
        </button>
      </div>
    </div>
  );
}
