/**
 * OS QUATRO CARTÕES DA CARTEIRA — "A carteira na área de atuação" (fidelidade
 * às maquetes, 23/09/2026 — fase 4; maquete `territorio-ficha.png`).
 *
 * ERA O `PainelDeIndicadores` DO CADASTRO, que seis outras telas usam. O cartão
 * da maquete é outro objeto — ladrilho de ícone colorido à esquerda, pílula de
 * variação no canto de cima, mini-gráfico no de baixo, barra de progresso na
 * cobertura —, e mudar o componente dos outros mudaria seis telas que ninguém
 * pediu para mudar. Por isso este é novo, e só desta aba.
 *
 * O QUE A MAQUETE MOSTRA E O SISTEMA NÃO TEM (decisão 1 do usuário): a variação
 * "vs. ano anterior" e o mini-gráfico. O LUGAR deles continua no cartão, do
 * mesmo tamanho: a pílula traz "—" e a dica com o motivo e a issue que destrava;
 * o mini-gráfico não desenha barras — não há série mensal, e barra desenhada sem
 * série é número inventado —, e fica só um traço de base discreto, para o
 * cartão não mudar de altura nem de alinhamento quando a série chegar.
 */

import { ChartColumn, CircleDollarSign, Target, Tractor } from 'lucide-react';
import type { ComponentType } from 'react';
import { InfoTooltip } from '../../InfoTooltip';
import { VariacaoAusente } from '../../mercado/VariacaoAusente';
import type { CartaoDaCarteira } from '../kpisDosIndicadores';

/** O ícone de cada cartão, na cor da maquete: verde, laranja, azul e lilás. */
const ICONE: Record<CartaoDaCarteira['id'], ComponentType<{ size?: number; strokeWidth?: number }>> = {
  municipios: ChartColumn,
  cobertura: Target,
  vendas: CircleDollarSign,
  parque: Tractor,
};

/** Por que o canto do mini-gráfico está vazio — vai na dica do cartão. */
const SEM_SERIE_MENSAL =
  'O mini-gráfico do canto fica vazio: a leitura desta tela devolve o total da janela de competência, e não o mês ' +
  'a mês que ele desenharia (issue 69). Barra desenhada sem série seria número inventado.';

export function CartoesDaCarteira({ cartoes, carregando }: { cartoes: CartaoDaCarteira[]; carregando: boolean }) {
  return (
    <div className="terr-cart-kpis" data-bloco="kpis-da-carteira">
      {cartoes.map((c) => {
        const Icone = ICONE[c.id];
        return (
          // O CARTÃO É O CONTÊINER E A GRADE MORA DENTRO: o corpo do valor muda
          // pela largura do PRÓPRIO cartão (um elemento não consulta a si
          // mesmo). A geometria é a da maquete — rótulo e pílula na primeira
          // linha, e o valor na linha inteira embaixo dela, que é o que deixa
          // "R$ 186,4 mi" caber num quarto de 1.300px.
          <article key={c.id} className="terr-cart-kpi" data-kpi-carteira={c.id}>
            <div className="terr-cart-kpi-grade">
            <span className="terr-cart-kpi-icone" aria-hidden="true">
              <Icone size={22} strokeWidth={2.2} />
            </span>

            <h3 className="terr-cart-kpi-rotulo">
              {c.rotulo}
              {/* A LINHA LONGA DE ANTES MORA AQUI: "330 de 540 vínculos
                  elegíveis no prazo · 210 pendentes · regra provisória". */}
              <InfoTooltip
                rotulo={`De onde vem ${c.rotulo.toLowerCase()}`}
                texto={
                  c.id === 'cobertura' ? (
                    c.deOnde
                  ) : (
                    <>
                      <p>{c.deOnde}</p>
                      <p>{SEM_SERIE_MENSAL}</p>
                    </>
                  )
                }
              />
            </h3>

            {/* A PÍLULA DA MAQUETE com a linha de variação de Mercado dentro
                (`VariacaoAusente`, o mesmo motivo e a mesma issue): o CSS a
                dobra em duas linhas — "— ⓘ" em cima, "vs. ano anterior"
                embaixo —, que é a forma da pílula. */}
            <span className="terr-cart-variacao">
              <VariacaoAusente deQue={c.rotulo.toLowerCase()} compacta />
            </span>

            <p className="terr-cart-kpi-valor">
              {carregando ? (
                <span className="cad-kpi-esqueleto" aria-hidden="true" />
              ) : c.valor === null ? (
                <>
                  <span className="cad-vazio" aria-hidden="true">
                    —
                  </span>
                  <span className="cad-so-leitor">sem dado</span>
                </>
              ) : (
                c.valor
              )}
            </p>
            <p className="terr-cart-kpi-contexto">{c.valor === null ? c.semDado : c.contexto}</p>

            {/* A cobertura tem a barra no lugar do mini-gráfico, como na maquete. */}
            {c.id !== 'cobertura' && <span className="terr-cart-mini" aria-hidden="true" />}

            {/* A BARRA DA COBERTURA É O VALOR REAL, e só aparece com ele. */}
            {c.id === 'cobertura' && c.progresso != null && !carregando && (
              <div
                className="terr-cart-progresso"
                role="progressbar"
                aria-label="Cobertura pela cadência"
                aria-valuemin={0}
                aria-valuemax={100}
                aria-valuenow={Math.round(c.progresso)}
              >
                <span style={{ width: `${Math.min(100, Math.max(0, c.progresso))}%` }} />
              </div>
            )}
            </div>
          </article>
        );
      })}
    </div>
  );
}
