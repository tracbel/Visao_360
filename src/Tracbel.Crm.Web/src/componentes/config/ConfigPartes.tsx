/**
 * Peças compartilhadas entre as seções de Configurações — porte de
 * `cardConfig`, `toggleRow`, `footerAcoes` e `configToast`
 * (prototipo/referencia/assets/app.js linhas 5873-5917).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — `ConfigToggleRow` virou componente controlado.
 *
 * Ele nascia com `defaultChecked` e ninguém lia o valor depois: a pessoa ligava
 * o resumo diário por e-mail, clicava em "Salvar alterações", recebia o aviso
 * de sucesso e nada tinha sido gravado. Agora o estado vem de fora, e quem
 * grava é `dados/persistenciaConfig.ts`.
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

export function ConfigToggleRow({
  titulo,
  desc,
  ativo,
  aoMudar,
}: {
  titulo: string;
  desc: string;
  ativo: boolean;
  aoMudar: (ativo: boolean) => void;
}) {
  return (
    <div className="config-list-row">
      <div>
        <div className="clr-title">{titulo}</div>
        <div className="clr-desc">{desc}</div>
      </div>
      <label className="switch">
        <input
          type="checkbox"
          checked={ativo}
          aria-label={titulo}
          onChange={(e) => aoMudar(e.target.checked)}
        />
        <span className="slider" />
      </label>
    </div>
  );
}

/**
 * O rodapé das seções que gravam.
 *
 * OS DOIS BOTÕES SÓ HABILITAM QUANDO HÁ O QUE SALVAR OU DESCARTAR. Antes, os
 * dois estavam sempre ativos e os dois respondiam com um aviso — "Alterações
 * salvas" e "Alterações descartadas" — mesmo sem nenhuma alteração ter sido
 * feita, o que ensinava a pessoa a não acreditar no aviso.
 */
export function ConfigFooterAcoes({
  temAlteracao,
  onSalvar,
  onCancelar,
}: {
  temAlteracao: boolean;
  onSalvar: () => void;
  onCancelar: () => void;
}) {
  return (
    <div className="config-footer-acoes">
      {temAlteracao && <span className="config-hint config-pendente">Há alterações não salvas nesta tela.</span>}
      <button type="button" className="btn-config-cancelar" onClick={onCancelar} disabled={!temAlteracao}>
        Cancelar
      </button>
      <button type="button" className="btn-config-salvar" onClick={onSalvar} disabled={!temAlteracao}>
        Salvar alterações
      </button>
    </div>
  );
}

export type ConfigToastEstado = { msg: string; tipo: 'ok' | 'warn' } | null;

export function ConfigToast({ toast }: { toast: ConfigToastEstado }) {
  if (!toast) return null;
  return (
    <div className={`config-toast show toast-${toast.tipo}`} role="status" aria-live="polite">
      {toast.msg}
    </div>
  );
}
