/**
 * As abas internas de um bloco da aba Mercado (fase T1 do documento 50).
 *
 * É O MESMO ALTERNADOR DOS MAPAS, de propósito: a diretoria já troca o recorte
 * dos quatro mapas assim, e um segundo jeito de alternar na mesma página seria
 * uma coisa nova para aprender sem ganho nenhum. Por isso `aria-pressed` e não
 * `role="tab"` — é o padrão que o projeto já usa e que o CSS já veste.
 */

import type { ReactNode } from 'react';

export type AbaInterna<T extends string> = {
  id: T;
  rotulo: string;
  conteudo: ReactNode;
};

export function AbasInternas<T extends string>({
  abas,
  ativa,
  aoTrocar,
  rotulo,
  acao,
}: {
  abas: readonly AbaInterna<T>[];
  ativa: T;
  aoTrocar: (id: T) => void;
  rotulo: string;
  /**
   * Uma ação à direita do alternador, na MESMA LINHA (fase T4.9 — maquete).
   *
   * É onde o "Simular cenário" do Potencial estrutural passa a morar. Em cima do
   * alternador, como estava, ele ganhava uma linha inteira só para si — e num
   * painel de um terço de largura essa linha custa caro.
   */
  acao?: ReactNode;
}) {
  const escolhida = abas.find((a) => a.id === ativa) ?? abas[0];

  return (
    <>
      <div className="dash-painel-controles">
        <div className="terr-alternador" role="group" aria-label={rotulo}>
          {abas.map((aba) => (
            <button key={aba.id} type="button" aria-pressed={aba.id === escolhida.id} onClick={() => aoTrocar(aba.id)}>
              {aba.rotulo}
            </button>
          ))}
        </div>
        {acao}
      </div>
      <div className="terr-subaba-conteudo">{escolhida.conteudo}</div>
    </>
  );
}
