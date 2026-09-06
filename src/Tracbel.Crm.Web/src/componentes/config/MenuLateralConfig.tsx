/**
 * Menu lateral das Configurações — porte de `renderConfigMenu`
 * (prototipo/referencia/assets/app.js linha 5305). Os itens variam por aba.
 */
import type { AbaConfig, SecaoConfig } from '../../tipos/configuracoes';

type ItemMenu = { id: SecaoConfig; icon: string; label: string };

const MENUS: Record<AbaConfig, ItemMenu[]> = {
  preferencias: [
    { id: 'perfil', icon: '👤', label: 'Perfil e conta' },
    { id: 'notificacoes', icon: '🔔', label: 'Notificações' },
    { id: 'atalhos', icon: '⌨️', label: 'Atalhos e produtividade' },
  ],
  comercial: [
    { id: 'metas', icon: '🎯', label: 'Metas e SLA' },
    { id: 'aprovacoes', icon: '✅', label: 'Políticas de aprovação' },
    { id: 'taxonomias', icon: '🏷️', label: 'Taxonomias' },
  ],
  ti: [
    { id: 'integracoes', icon: '🔌', label: 'Integrações' },
    { id: 'usuarios', icon: '👥', label: 'Usuários' },
    { id: 'permissoes', icon: '🛡️', label: 'Permissões e roles' },
    { id: 'auditoria', icon: '📋', label: 'Auditoria e logs' },
  ],
};

export function MenuLateralConfig({
  aba,
  secaoAtiva,
  onSelecionar,
}: {
  aba: AbaConfig;
  secaoAtiva: SecaoConfig;
  onSelecionar: (secao: SecaoConfig) => void;
}) {
  return (
    <>
      {MENUS[aba].map((item) => (
        <button
          key={item.id}
          type="button"
          className={`config-menu-item ${secaoAtiva === item.id ? 'active' : ''}`}
          onClick={() => onSelecionar(item.id)}
        >
          <span className="cmi-icon">{item.icon}</span>
          <span>{item.label}</span>
        </button>
      ))}
    </>
  );
}
