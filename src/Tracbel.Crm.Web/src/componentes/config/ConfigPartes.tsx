/**
 * Peças compartilhadas entre as seções de Configurações — porte de `cardConfig`
 * (prototipo/referencia/assets/app.js linha 5873).
 *
 * 22/09/2026 (issue 134): o interruptor, o rodapé Cancelar/Salvar e o aviso
 * flutuante saíram junto com as seções fictícias que os usavam. As seções que
 * ficaram leem a API e gravam cada uma pelo seu formulário.
 */
import type { ReactNode } from 'react';

export function CardConfig({ titulo, children }: { titulo: string; children: ReactNode }) {
  return (
    <div className="config-card">
      <div className="cc-header">
        <h3>{titulo}</h3>
      </div>
      <div className="cc-body">{children}</div>
    </div>
  );
}

/**
 * Um dado só de leitura, no desenho do protótipo: rótulo em cima, valor numa caixa e uma dica embaixo. Fica numa
 * grade `.conta-campos`, que se ajusta à largura — em vez de linhas com o rótulo numa ponta e o valor na outra.
 */
export function CampoDeLeitura({ id, rotulo, valor, dica }: { id: string; rotulo: string; valor: string; dica?: string }) {
  return (
    <div className="form-field conta-campo">
      <label htmlFor={id}>{rotulo}</label>
      <input id={id} value={valor} title={valor} readOnly />
      {dica && <span className="conta-dica">{dica}</span>}
    </div>
  );
}
