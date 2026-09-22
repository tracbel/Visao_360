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
