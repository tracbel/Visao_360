/**
 * Linha de ranking com barra de progresso — porte do item repetido em
 * `renderTopClientes360()` e `renderTopCensOuFiliais360()`
 * (prototipo/referencia/assets/app.js:7690-7791). Serve tanto o Top 5
 * clientes (sem avatar) quanto o Top CENs (com avatar de iniciais) e o Top
 * filiais. Reutilizável em qualquer card de ranking com barra de progresso.
 */
export type TopBarraAvatar = { texto: string; cor: string };

export type TopBarraItemProps = {
  rank: number;
  nome: string;
  meta: string;
  valorFormatado: string;
  progressoPct: number;
  corBarra: string;
  avatar?: TopBarraAvatar;
  /** Aplica a classe `v360-eu` (linha do próprio usuário no Top CENs). */
  destaque?: boolean;
  aoClicar?: () => void;
};

export function TopBarraItem({
  rank,
  nome,
  meta,
  valorFormatado,
  progressoPct,
  corBarra,
  avatar,
  destaque,
  aoClicar,
}: TopBarraItemProps) {
  return (
    <div
      className={destaque ? 'v360-topbar-item v360-eu' : 'v360-topbar-item'}
      onClick={aoClicar}
      style={aoClicar ? { cursor: 'pointer' } : undefined}
    >
      <div className="v360-topbar-rank">#{rank}</div>
      {avatar && (
        <div className="v360-topbar-avatar" style={{ background: avatar.cor }}>
          {avatar.texto}
        </div>
      )}
      <div className="v360-topbar-info">
        <div className="v360-topbar-nome">{nome}</div>
        <div className="v360-topbar-meta">{meta}</div>
      </div>
      <div className="v360-topbar-valor-wrap">
        <div className="v360-topbar-valor">{valorFormatado}</div>
        <div className="v360-topbar-progress">
          <div className="v360-topbar-fill" style={{ width: `${progressoPct.toFixed(0)}%`, background: corBarra }} />
        </div>
      </div>
    </div>
  );
}
