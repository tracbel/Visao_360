/**
 * Bloco do painel 360 do cliente.
 *
 * Todo bloco que busca dado tem os quatro estados exigidos pelo doc 05 §3:
 * carregando, vazio, erro e conteúdo. O bloco também carrega, quando é o caso,
 * o selo da fonte externa (`SeloFonte`) — porque nesta tela existe dado que o
 * CRM lê e não escreve.
 */
import type { ReactNode } from 'react';

export type EstadoBloco = 'carregando' | 'erro' | 'vazio' | 'ok';

type Props = {
  id: string;
  titulo: string;
  subtitulo?: string;
  /** Selo de origem do dado (ver `SeloFonte`). */
  fonte?: ReactNode;
  /** Link ou botão do canto superior direito. Só aparece se a ação existir de fato. */
  acao?: ReactNode;
  estado: EstadoBloco;
  mensagemVazia?: string;
  mensagemErro?: string;
  /** Ocupa a largura toda da grade. */
  largo?: boolean;
  children?: ReactNode;
};

export function BlocoPainel({
  id,
  titulo,
  subtitulo,
  fonte,
  acao,
  estado,
  mensagemVazia = 'Nada a mostrar aqui.',
  mensagemErro = 'Não foi possível carregar este bloco.',
  largo = false,
  children,
}: Props) {
  const tituloId = `p360-bloco-${id}-titulo`;

  return (
    <section className={largo ? 'p360-bloco p360-bloco-largo' : 'p360-bloco'} aria-labelledby={tituloId} data-bloco={id}>
      <div className="p360-bloco-header">
        <div className="p360-bloco-header-texto">
          <h3 className="p360-bloco-titulo" id={tituloId}>
            {titulo}
          </h3>
          {subtitulo && <p className="p360-bloco-sub">{subtitulo}</p>}
        </div>
        <div className="p360-bloco-header-lado">
          {fonte}
          {acao}
        </div>
      </div>

      <div className="p360-bloco-body">
        {estado === 'carregando' && (
          <div className="p360-estado" role="status" aria-live="polite">
            <span className="p360-estado-spinner" aria-hidden="true" />
            Carregando…
          </div>
        )}
        {estado === 'erro' && (
          <div className="p360-estado p360-estado-erro" role="alert">
            {mensagemErro}
          </div>
        )}
        {estado === 'vazio' && <div className="p360-estado p360-estado-vazio">{mensagemVazia}</div>}
        {estado === 'ok' && children}
      </div>
    </section>
  );
}
