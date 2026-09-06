/**
 * Barra de abas + conteúdo das fichas — porte do padrão `.tabs` / `.tab-content`
 * do protótipo (ex.: `mountEquipamentoFicha()` e `mountOportunidadeFicha()` em
 * `prototipo/referencia/assets/app.js`). No original, o clique troca a classe
 * `active` via `querySelectorAll`; aqui vira `useState` no componente da tela,
 * mas a marcação (`.tabs`, `.tab`, `.tab-content`) é a mesma.
 */
import type { ReactNode } from 'react';

export type AbaFicha = {
  chave: string;
  rotulo: ReactNode;
  conteudo: ReactNode;
};

type Props = {
  abas: AbaFicha[];
  ativa: string;
  aoSelecionar: (chave: string) => void;
};

export function AbasFicha({ abas, ativa, aoSelecionar }: Props) {
  return (
    <>
      <div className="tabs">
        {abas.map((aba) => (
          <button
            key={aba.chave}
            type="button"
            className={aba.chave === ativa ? 'tab active' : 'tab'}
            data-tab={aba.chave}
            onClick={() => aoSelecionar(aba.chave)}
          >
            {aba.rotulo}
          </button>
        ))}
      </div>
      {abas.map((aba) => (
        <div
          key={aba.chave}
          className={aba.chave === ativa ? 'tab-content active' : 'tab-content'}
          data-tab-content={aba.chave}
        >
          {aba.conteudo}
        </div>
      ))}
    </>
  );
}
