/**
 * Selo de origem do dado — usado nos blocos que o CRM lê e não escreve.
 *
 * Regra do doc 05 §5: campo que vem do Protheus não é editável no CRM, mostra
 * de onde veio e quando foi atualizado. Quando a última carga passou de
 * `DIAS_ATE_DADO_VELHO`, o selo aparece marcado como velho — é o que faltou nos
 * 17 meses de faturamento congelado sem ninguém notar.
 */
import { type CarimboFonte, formatarData } from '../../dados/painel360';

function IconeCadeado() {
  return (
    <svg viewBox="0 0 24 24" width="11" height="11" fill="none" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <rect x="4" y="11" width="16" height="10" rx="2" />
      <path d="M8 11V7a4 4 0 0 1 8 0v4" />
    </svg>
  );
}

function IconeAviso() {
  return (
    <svg viewBox="0 0 24 24" width="11" height="11" fill="none" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
      <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
      <path d="M12 9v4" />
      <path d="M12 17h.01" />
    </svg>
  );
}

export function SeloFonte({ carimbo }: { carimbo: CarimboFonte }) {
  if (carimbo.situacao === 'sem_leitura') {
    return (
      <span className="p360-fonte p360-fonte-sem" title={`Sem leitura do ${carimbo.sistema} para este registro`}>
        <IconeAviso />
        {carimbo.sistema} · sem leitura
      </span>
    );
  }

  const quando = carimbo.atualizadoEm ? formatarData(carimbo.atualizadoEm) : '—';
  const dias = carimbo.diasDesde ?? 0;
  const idade = dias === 0 ? 'hoje' : dias === 1 ? 'há 1 dia' : `há ${dias} dias`;

  if (carimbo.situacao === 'velho') {
    return (
      <span
        className="p360-fonte p360-fonte-velho"
        title={`Somente leitura — vem do ${carimbo.sistema}. Última carga em ${quando} (${idade}). Dado velho: corrija na origem.`}
      >
        <IconeAviso />
        {carimbo.sistema} · dado velho, {idade}
      </span>
    );
  }

  return (
    <span
      className="p360-fonte"
      title={`Somente leitura — vem do ${carimbo.sistema}. Última carga em ${quando} (${idade}).`}
    >
      <IconeCadeado />
      {carimbo.sistema} · {quando}
    </span>
  );
}
