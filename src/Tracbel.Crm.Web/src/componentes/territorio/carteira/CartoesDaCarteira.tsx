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
 * A PÍLULA E O MINI-GRÁFICO TÊM NÚMERO ONDE HÁ NÚMERO (27/09/2026). A leitura
 * passou a trazer o mesmo trecho do ano anterior e o mês a mês das vendas: o
 * cartão de vendas mostra a variação e as barras de cada mês, com o ano anterior
 * atrás. Municípios, cobertura e parque não são medidas do período — a área de
 * atuação é a de hoje, a cobertura é medida no instante, o parque é estrutural —,
 * e a pílula deles fica com o traço e o motivo DE CADA UM. O canto sem série
 * fica com o traço de base discreto, para o cartão não mudar de altura.
 */

import { ChartColumn, CircleDollarSign, Target, Tractor } from 'lucide-react';
import type { ComponentType } from 'react';
import { InfoTooltip } from '../../InfoTooltip';
import { VariacaoAusente } from '../../mercado/VariacaoAusente';
import { VariacaoContraOAnoAnterior } from '../comparacao';
import type { CartaoDaCarteira } from '../kpisDosIndicadores';

/** O ícone de cada cartão, na cor da maquete: verde, laranja, azul e lilás. */
const ICONE: Record<CartaoDaCarteira['id'], ComponentType<{ size?: number; strokeWidth?: number }>> = {
  municipios: ChartColumn,
  cobertura: Target,
  vendas: CircleDollarSign,
  parque: Tractor,
};

/**
 * O MINI-GRÁFICO DO CANTO: uma barra por mês da janela, e o mesmo mês do ano
 * anterior como barra clara atrás dela. É desenho, e não número a ler — o
 * número está no cartão e na dica —, então fica fora do leitor de tela.
 */
function MiniGrafico({ serie }: { serie: NonNullable<CartaoDaCarteira['serie']> }) {
  const largura = 44;
  const altura = 20;
  const maior = Math.max(1, ...serie.atual, ...serie.anterior);
  const passo = largura / Math.max(1, serie.atual.length);
  const barra = Math.max(1, passo * 0.6);

  return (
    <svg
      className="terr-cart-mini terr-cart-mini-serie"
      width={largura}
      height={altura}
      viewBox={`0 0 ${largura} ${altura}`}
      aria-hidden="true"
      data-meses={serie.atual.length}
    >
      {serie.atual.map((valor, i) => {
        const antes = serie.anterior[i];
        const x = i * passo + (passo - barra) / 2;
        const h = Math.max(0, (valor / maior) * altura);
        const hAntes = antes === undefined ? 0 : Math.max(0, (antes / maior) * altura);
        return (
          <g key={serie.meses[i] ?? i}>
            {antes !== undefined && (
              <rect x={x - 1} y={altura - hAntes} width={barra + 2} height={hAntes} className="terr-cart-mini-antes" />
            )}
            <rect x={x} y={altura - h} width={barra} height={h} className="terr-cart-mini-agora" />
          </g>
        );
      })}
    </svg>
  );
}

export function CartoesDaCarteira({ cartoes, carregando }: { cartoes: CartaoDaCarteira[]; carregando: boolean }) {
  return (
    <div className="terr-cart-kpis" data-bloco="kpis-da-carteira">
      {cartoes.map((c) => {
        const Icone = ICONE[c.id];
        const nome = c.rotulo.toLowerCase();
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
                  elegíveis no prazo · 210 pendentes · regra provisória". O
                  porquê do canto sem mini-gráfico vai junto, verdadeiro para
                  ESTE cartão. */}
              <InfoTooltip
                rotulo={`De onde vem ${nome}`}
                texto={
                  c.id === 'cobertura' ? (
                    c.deOnde
                  ) : (
                    <>
                      <p>{c.deOnde}</p>
                      <p>
                        {c.serie
                          ? `O mini-gráfico do canto é o mês a mês da janela (${c.serie.meses[0]} a ${c.serie.meses.at(-1)}), com o mesmo mês do ano anterior como barra clara atrás.`
                          : `O canto do mini-gráfico fica vazio: ${c.motivoSemSerie}`}
                      </p>
                    </>
                  )
                }
              />
            </h3>

            {/* A PÍLULA DA MAQUETE: o CSS dobra a linha de variação em duas —
                o número (ou o traço) e a dica em cima, "vs. ano anterior"
                embaixo —, que é a forma da pílula. */}
            <span className="terr-cart-variacao">
              {c.variacao && !carregando ? (
                <VariacaoContraOAnoAnterior
                  deQue={nome}
                  atual={c.variacao.atual}
                  anterior={c.variacao.anterior}
                  motivoSemAnterior={c.motivoSemVariacao}
                  formatar={c.variacao.formatar}
                  compacta
                />
              ) : (
                <VariacaoAusente deQue={nome} compacta motivo={carregando ? 'A leitura ainda não voltou.' : c.motivoSemVariacao} />
              )}
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
            {c.id !== 'cobertura' &&
              (c.serie && !carregando ? <MiniGrafico serie={c.serie} /> : <span className="terr-cart-mini" aria-hidden="true" />)}

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
