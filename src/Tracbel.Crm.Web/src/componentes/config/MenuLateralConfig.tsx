/**
 * Menu lateral das Configurações — porte de `renderConfigMenu`
 * (prototipo/referencia/assets/app.js linha 5305).
 *
 * 22/09/2026 (issue 134): os itens vêm de `secoes.ts`, já filtrados pela
 * permissão de quem está usando. O menu não decide nada sozinho.
 */
import type { DefinicaoDeSecao, SecaoConfig } from './secoes';

export function MenuLateralConfig({
  secoes,
  secaoAtiva,
  onSelecionar,
}: {
  secoes: DefinicaoDeSecao[];
  secaoAtiva: SecaoConfig;
  onSelecionar: (secao: SecaoConfig) => void;
}) {
  return (
    <>
      {secoes.map((item) => (
        <button
          key={item.id}
          type="button"
          className={`config-menu-item ${secaoAtiva === item.id ? 'active' : ''}`}
          onClick={() => onSelecionar(item.id)}
        >
          <span className="cmi-icon">{item.icone}</span>
          <span>{item.rotulo}</span>
        </button>
      ))}
    </>
  );
}
