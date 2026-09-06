/**
 * Ícones usados na Ficha de Oportunidade — porte de `iconOppSvg()`
 * (`prototipo/referencia/assets/app.js`, linha 2896). Mesmos traçados do
 * protótipo; não redesenhar.
 */

export type NomeIconeOportunidade =
  | 'check'
  | 'clock'
  | 'x'
  | 'file'
  | 'upload'
  | 'shield'
  | 'alert'
  | 'plus'
  | 'edit'
  | 'play';

type Nome = NomeIconeOportunidade;

type Props = { nome: Nome; tamanho?: number };

function Caminho({ nome }: { nome: Nome }) {
  switch (nome) {
    case 'check':
      return <polyline points="20 6 9 17 4 12" />;
    case 'clock':
      return (
        <>
          <circle cx="12" cy="12" r="10" />
          <polyline points="12 6 12 12 16 14" />
        </>
      );
    case 'x':
      return (
        <>
          <line x1="18" y1="6" x2="6" y2="18" />
          <line x1="6" y1="6" x2="18" y2="18" />
        </>
      );
    case 'file':
      return (
        <>
          <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
          <polyline points="14 2 14 8 20 8" />
        </>
      );
    case 'upload':
      return (
        <>
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
          <polyline points="17 8 12 3 7 8" />
          <line x1="12" y1="3" x2="12" y2="15" />
        </>
      );
    case 'shield':
      return <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />;
    case 'alert':
      return (
        <>
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="8" x2="12" y2="12" />
          <line x1="12" y1="16" x2="12.01" y2="16" />
        </>
      );
    case 'plus':
      return (
        <>
          <line x1="12" y1="5" x2="12" y2="19" />
          <line x1="5" y1="12" x2="19" y2="12" />
        </>
      );
    case 'edit':
      return (
        <>
          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
        </>
      );
    case 'play':
      return <polygon points="5 3 19 12 5 21 5 3" />;
  }
}

export function IconeOportunidade({ nome, tamanho = 16 }: Props) {
  return (
    <svg
      width={tamanho}
      height={tamanho}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <Caminho nome={nome} />
    </svg>
  );
}
