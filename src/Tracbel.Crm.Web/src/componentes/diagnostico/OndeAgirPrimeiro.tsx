/**
 * "ONDE AGIR PRIMEIRO" — os cinco municípios de maior IOC no recorte, no desenho da maquete de 02/10/2026: a posição, o
 * nome, a região e o CEN, a barra do índice, o número e a classe.
 *
 * É A FICHA VAZIA: escolher um deles (aqui, no mapa ou na tabela) abre a ficha do município no lugar desta lista, e
 * fechar a ficha volta para cá. "Ver todos os municípios" leva à tabela.
 */

import { ArrowRight, Target } from 'lucide-react';
import { nomeCurto } from '../../telas/cadastro/formato';
import type { MunicipioNoDiagnostico } from '../../tipos/mercado';
import { CURTO_DA_CLASSE, n, ROTULO_DA_CLASSE } from './diagnostico';

export function OndeAgirPrimeiro({
  primeiros,
  aoEscolher,
  aoVerTodos,
}: {
  primeiros: MunicipioNoDiagnostico[];
  aoEscolher: (codigo: number) => void;
  aoVerTodos: () => void;
}) {
  return (
    <div className="card cad-cartao diag-ficha diag-primeiros-cartao" data-bloco="ficha">
      <div className="diag-ficha-cabecalho">
        <div>
          <div className="card-title">
            <Target size={18} strokeWidth={2.2} className="diag-icone-verde" aria-hidden="true" />
            Onde agir primeiro
          </div>
          <div className="card-subtitle">Municípios com maior potencial de retorno. Clique em um deles para ver mais detalhes.</div>
        </div>
        <button type="button" className="diag-ver-todos" onClick={aoVerTodos}>
          Ver todos os municípios
          <ArrowRight size={14} strokeWidth={2.2} aria-hidden="true" />
        </button>
      </div>

      {primeiros.length === 0 ? (
        <p className="diag-nota">Nenhum município do recorte tem IOC.</p>
      ) : (
        <ol className="diag-primeiros">
          {primeiros.map((p, i) => (
            <li key={p.codigoIbge}>
              <button type="button" className="diag-primeiro" onClick={() => aoEscolher(p.codigoIbge)}>
                <span className="diag-primeiro-posicao" aria-hidden="true">
                  {i + 1}
                </span>
                <span className="diag-primeiro-nome">
                  <strong>{p.nome}</strong>
                  <span className="diag-primeiro-onde">
                    Região {p.regiao}
                    {p.responsavel && <> • {nomeCurto(p.responsavel)}</>}
                  </span>
                </span>
                {p.ioc !== null && p.classe !== null && (
                  <>
                    <span className="diag-primeiro-trilho" aria-hidden="true">
                      <span style={{ width: `${Math.min(100, Math.max(0, p.ioc))}%` }} />
                    </span>
                    <span className="diag-primeiro-ioc">{n(p.ioc)}</span>
                    <span className={`diag-primeiro-classe diag-classe-${p.classe}`} title={ROTULO_DA_CLASSE[p.classe]}>
                      {CURTO_DA_CLASSE[p.classe]}
                    </span>
                  </>
                )}
              </button>
            </li>
          ))}
        </ol>
      )}
    </div>
  );
}
