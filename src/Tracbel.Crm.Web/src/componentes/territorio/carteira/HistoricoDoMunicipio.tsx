/**
 * A ABA HISTÓRICO DA FICHA (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * A maquete tem a aba e não mostra o conteúdo dela. O que ela pediria — vendas,
 * cobertura e lavoura do município ao longo dos anos — não existe em série: a
 * leitura desta tela devolve UMA janela de competência, e a PAM carrega três
 * anos. A aba fica, vazia e dizendo o motivo; uma linha do tempo desenhada sem
 * série seria número inventado.
 */

import { ChartLine } from 'lucide-react';
import { InfoTooltip } from '../../InfoTooltip';

export function HistoricoDoMunicipio({ nome }: { nome: string }) {
  return (
    <section className="terr-ficha-bloco" data-bloco-da-ficha="historico" aria-labelledby="ficha-historico-titulo">
      <h3 id="ficha-historico-titulo" className="terr-ficha-bloco-titulo">
        Histórico de {nome}
      </h3>
      <p className="terr-ficha-bloco-subtitulo">Vendas, cobertura e lavoura do município ao longo do tempo.</p>
      <div className="terr-ficha-sem-serie">
        <ChartLine size={22} strokeWidth={1.8} aria-hidden="true" />
        <span>
          <span aria-hidden="true">— </span>Sem série histórica
        </span>
        <InfoTooltip
          rotulo="Por que o histórico do município está vazio"
          texto="A série histórica do município ainda não existe: a leitura desta tela devolve uma janela de competência só, e não o ano a ano das vendas e da cobertura (issue 69); a lavoura carrega os três anos mais recentes da PAM, e a série longa é a issue 156."
        />
      </div>
    </section>
  );
}
