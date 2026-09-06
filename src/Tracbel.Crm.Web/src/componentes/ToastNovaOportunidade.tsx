/**
 * Toast de confirmação ao salvar uma oportunidade — porte de
 * `mostrarToastNovaOp()` (`prototipo/referencia/assets/app.js`, linha 7057).
 * No original o elemento é anexado a `document.body` e sobrevive à troca de
 * hash; aqui ele é local à tela Nova Oportunidade e some quando o componente
 * desmonta (redirecionamento para `/pipeline`).
 */
import { useEffect, useState } from 'react';
import { formatarBRLCompacto } from '../dados/formatadores';
import type { OportunidadeNova } from '../tipos/oportunidade';

type Props = {
  oportunidade: OportunidadeNova;
  faseLabel: string;
};

export function ToastNovaOportunidade({ oportunidade, faseLabel }: Props) {
  const [visivel, setVisivel] = useState(false);

  useEffect(() => {
    const id = setTimeout(() => setVisivel(true), 50);
    return () => clearTimeout(id);
  }, []);

  return (
    <div className={visivel ? 'novaop-toast show' : 'novaop-toast'}>
      <div className="nt-icon">
        <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="2.5">
          <polyline points="20 6 9 17 4 12" />
        </svg>
      </div>
      <div className="nt-body">
        <div className="nt-title">Oportunidade {oportunidade.id} criada</div>
        <div className="nt-sub">
          {oportunidade.cliente} · {formatarBRLCompacto(oportunidade.valor)} · fase {faseLabel}
        </div>
        <div className="nt-hint">Já visível no Pipeline, Ficha do Cliente, Funil e Performance. Redirecionando...</div>
      </div>
    </div>
  );
}
