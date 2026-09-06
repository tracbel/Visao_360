/**
 * O carimbo de origem da leitura — de onde veio, quando foi lido, e se está velho.
 *
 * POR QUE MARCAR ATÉ O NOSSO PRÓPRIO BANCO: a mesma tela mistura o cadastro
 * nosso e a leitura da ponte do Vórtice. Se só o legado viesse carimbado, o
 * usuário teria de deduzir a origem do resto pela AUSÊNCIA de carimbo — e
 * dedução é exatamente o que este selo existe para eliminar (documento 23,
 * seção 1).
 *
 * `[V]` É a resposta direta aos 17 meses de faturamento parado passando por
 * atual no legado: dado velho aparece marcado como velho, e o aviso que a API
 * escreveu aparece inteiro, sem a tela reescrever.
 */

import type { Procedencia } from '../../tipos/api';

/** Formata o instante da leitura no fuso de quem está olhando. */
function formatarInstante(iso: string): string {
  const data = new Date(iso.endsWith('Z') ? iso : `${iso}Z`);
  if (Number.isNaN(data.getTime())) return iso;
  return data.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
}

export function SeloProcedencia({ procedencia }: { procedencia: Procedencia | null }) {
  if (!procedencia) return null;

  const lidoEm = formatarInstante(procedencia.lidoEmUtc);
  const maisRecente = procedencia.dadoMaisRecenteEm ? formatarInstante(procedencia.dadoMaisRecenteEm) : null;
  const velho = procedencia.estaDesatualizado;

  const titulo =
    `${procedencia.sistema} · ${procedencia.objeto}\n` +
    `Lido em ${lidoEm}` +
    (maisRecente ? `\nAlteração mais recente na origem: ${maisRecente}` : '') +
    (procedencia.aviso ? `\n${procedencia.aviso}` : '');

  return (
    <span className={`cad-procedencia${velho ? ' cad-procedencia-velha' : ''}`} title={titulo}>
      <svg viewBox="0 0 24 24" width="11" height="11" fill="none" stroke="currentColor" strokeWidth={2.2} aria-hidden="true">
        {velho ? (
          <>
            <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
            <path d="M12 9v4" />
            <path d="M12 17h.01" />
          </>
        ) : (
          <>
            <circle cx="12" cy="12" r="9" />
            <path d="M12 7v5l3 2" />
          </>
        )}
      </svg>
      <span className="cad-procedencia-sistema">{procedencia.sistema}</span>
      <span className="cad-procedencia-objeto">{procedencia.objeto}</span>
      <span className="cad-procedencia-quando">lido em {lidoEm}</span>
      {velho && <span className="cad-procedencia-alerta">dado desatualizado</span>}
    </span>
  );
}

/**
 * O aviso que a API escreveu, quando ela escreveu algum.
 *
 * Fica separado do selo de propósito: o selo é discreto e vive no cabeçalho da
 * lista; o aviso é uma frase que o usuário precisa LER, e frase escondida em
 * `title` não é lida por ninguém.
 */
export function AvisoDeProcedencia({ procedencia }: { procedencia: Procedencia | null }) {
  if (!procedencia?.aviso) return null;
  return (
    <div className="cad-aviso-procedencia" role="status">
      <strong>{procedencia.sistema}</strong> {procedencia.aviso}
    </div>
  );
}
