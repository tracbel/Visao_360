/**
 * Modal de confirmação para limpar as oportunidades da sessão — porte de
 * `confirmarLimparSessao()` (`prototipo/referencia/assets/app.js`, linha 7138).
 * Reaproveita as classes `.modal-ficha-indispo*` / `.mfi-*` já usadas pelo
 * protótipo para o modal de "ficha indisponível".
 */
type Props = {
  quantidade: number;
  onCancelar: () => void;
  onConfirmar: () => void;
};

export function ModalLimparSessao({ quantidade, onCancelar, onConfirmar }: Props) {
  const plural = quantidade > 1;

  return (
    <div
      className="modal-ficha-indispo-overlay"
      onClick={(e) => {
        if (e.target === e.currentTarget) onCancelar();
      }}
    >
      <div className="modal-ficha-indispo">
        <div className="mfi-header">
          <div className="mfi-icon" style={{ background: '#FEF3C7', color: '#B45309' }}>
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2.2">
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
              <path d="M12 9v4" />
              <path d="M12 17h.01" />
            </svg>
          </div>
          <div>
            <div className="mfi-title">Limpar oportunidades da sessão?</div>
            <div className="mfi-sub">
              {quantidade} oportunidade{plural ? 's' : ''} criada{plural ? 's' : ''} será{plural ? 'ão' : ''} removida
              {plural ? 's' : ''}
            </div>
          </div>
          <button className="mfi-close" onClick={onCancelar} title="Fechar">
            ×
          </button>
        </div>
        <div className="mfi-body">
          <p>
            Esta ação remove <strong>apenas as oportunidades criadas nesta sessão</strong> via tela "Nova
            Oportunidade". As oportunidades originais do protótipo permanecem.
          </p>
          <p>Após confirmar, Pipeline, Ficha do Cliente, Funil e Performance voltarão ao estado inicial.</p>
        </div>
        <div className="mfi-actions">
          <button className="btn-cancelar" onClick={onCancelar}>
            Cancelar
          </button>
          <button className="btn-limpar-confirmar" onClick={onConfirmar}>
            Sim, limpar {quantidade} oportunidade{plural ? 's' : ''}
          </button>
        </div>
      </div>
    </div>
  );
}
