/**
 * O custo de produção das culturas em São Paulo, pelas séries históricas da CONAB (issue 67).
 *
 * É a outra metade da rentabilidade que o texto-base pede ("se o cara está
 * rentabilizando X e o custo de produção é tanto"); a primeira é o painel de
 * preços logo acima. A MARGEM, que junta os dois, é da issue 73 e não aparece
 * aqui — o custo é mostrado, não interpretado.
 *
 * O LOCAL É O DA CONAB: é onde ela levantou o custo (Franca para o café,
 * Piracicaba e Penápolis para a cana), não um município da carteira.
 *
 * CUSTO TOTAL AUSENTE NÃO É ZERO. As abas antigas de laranja e duas de cana
 * param no custo operacional; a tela diz isso em vez de mostrar um traço mudo.
 */

import { useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterCustosDeProducao } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type { CustoNaSafra, SerieDeCusto } from '../../tipos/mercado';

/** A ordem do texto-base; o que vier além delas entra no fim. */
const ORDEM = ['CAFÉ ARÁBICA', 'CANA DE AÇÚCAR', 'SOJA', 'MILHO', 'AMENDOIM', 'LARANJA'];

function reais(valor: number): string {
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 2 });
}

/** "CANA DE AÇÚCAR" → "Cana de açúcar": só a primeira letra, como se escreve. */
function comoSeEscreve(texto: string): string {
  const minusculo = texto.toLocaleLowerCase('pt-BR');
  return minusculo.charAt(0).toLocaleUpperCase('pt-BR') + minusculo.slice(1);
}

function chave(s: SerieDeCusto): string {
  return `${s.cultura}|${s.local}|${s.variante ?? ''}`;
}

function nomeDoLocal(s: SerieDeCusto): string {
  return s.variante ? `${s.local} — ${s.variante.toLowerCase()}` : s.local;
}

/** A aba mais recente: a última safra e, nela, o último relatório. */
function ultima(s: SerieDeCusto): CustoNaSafra {
  return s.safras[s.safras.length - 1];
}

/** "2025" ou, quando a CONAB publicou mais de um relatório no ano, "11/2022". */
function rotuloDaSafra(c: CustoNaSafra, s: SerieDeCusto): string {
  const noAno = s.safras.filter((x) => x.safra === c.safra).length;
  return noAno > 1 && c.mesDoRelatorio ? `${String(c.mesDoRelatorio).padStart(2, '0')}/${c.safra}` : String(c.safra);
}

function ordemDaCultura(cultura: string): number {
  const i = ORDEM.findIndex((c) => cultura.startsWith(c));
  return i < 0 ? ORDEM.length : i;
}

export function PainelDeCustos() {
  const { contexto } = useContextoDeAcesso();
  const custos = useRecurso((sinal) => obterCustosDeProducao(contexto, sinal), [contexto.empresa, contexto.usuario]);

  const series = custos.dados ?? [];
  const culturas = [...new Set(series.map((s) => s.cultura))].sort(
    (a, b) => ordemDaCultura(a) - ordemDaCultura(b) || a.localeCompare(b),
  );

  const [cultura, setCultura] = useState<string | null>(null);
  const [escolhida, setEscolhida] = useState<string | null>(null);

  const culturaAtiva = cultura ?? culturas[0] ?? null;
  // AS SÉRIES MAIS RECENTES PRIMEIRO: quem abre a cultura quer o custo de hoje, e as séries paradas em
  // 2014 (o amendoim de Guariba, por exemplo) são história.
  const daCultura = series
    .filter((s) => s.cultura === culturaAtiva)
    .sort((a, b) => ultima(b).safra - ultima(a).safra || a.local.localeCompare(b.local));
  const selecionada = daCultura.find((s) => chave(s) === escolhida) ?? daCultura[0] ?? null;

  // O GRÁFICO USA O CUSTO TOTAL, que é o que a planilha do comercial usa; na aba que parou no
  // operacional, o ponto fica de fora — misturar as duas camadas na mesma linha faria a série "cair".
  const pontos = (selecionada?.safras ?? [])
    .filter((c) => c.custoTotalHa !== null)
    .map((c) => ({ rotulo: rotuloDaSafra(c, selecionada!), valor: c.custoTotalHa! }));

  return (
    <>
      <div className="terr-secao-mercado">
        <h2 className="terr-secao-titulo">Custo de produção — referências da CONAB em São Paulo</h2>
        <p className="terr-secao-subtitulo">
          As séries históricas da CONAB, por cultura e local de referência: custo <strong>operacional</strong> (variável +
          fixo) e <strong>total</strong> (operacional + remuneração do capital e da terra), por hectare e por unidade. A
          planilha do comercial usa o custo total por hectare. A margem — preço menos custo — vem com os indicadores de
          mercado.
        </p>
      </div>

      {custos.carregando && <BlocoCarregando oQue="os custos de produção" />}
      {custos.erro && <BlocoErro erro={custos.erro} aoTentarDeNovo={custos.recarregar} />}

      {custos.dados && series.length === 0 && (
        <div className="card cad-cartao terr-cartao">
          <BlocoVazio
            titulo="Os custos de produção ainda não foram carregados neste banco"
            texto={
              <>
                A rota respondeu, mas não há nenhum custo gravado: a carga (<code>--somente-custos</code>) ainda não rodou
                aqui. Não é zero nem falta de permissão.
              </>
            }
          />
        </div>
      )}

      {custos.dados && series.length > 0 && (
        <>
          <div className="terr-precos-acoes terr-abas-cultura" role="tablist" aria-label="Cultura">
            {culturas.map((c) => (
              <button
                key={c}
                type="button"
                role="tab"
                aria-selected={c === culturaAtiva}
                className={c === culturaAtiva ? 'btn btn-primary btn-sm' : 'btn btn-secondary btn-sm'}
                onClick={() => {
                  setCultura(c);
                  setEscolhida(null);
                }}
              >
                {comoSeEscreve(c)}
              </button>
            ))}
          </div>

          <div className="terr-grade-precos terr-grade-custos">
            <div className="card cad-cartao terr-cartao">
              <div className="card-title">Última safra de cada local</div>
              <div className="cad-tabela-wrap">
                <table className="cad-tabela terr-tabela-precos">
                  <thead>
                    <tr>
                      <th>Local</th>
                      <th>Safra</th>
                      <th className="terr-num">Operacional / ha</th>
                      <th className="terr-num">Total / ha</th>
                      <th className="terr-num" title="Quantas safras esta série tem">
                        Safras
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {daCultura.map((s) => {
                      const u = ultima(s);
                      const ativa = selecionada && chave(s) === chave(selecionada);
                      return (
                        <tr
                          key={chave(s)}
                          className={ativa ? 'terr-linha-ativa' : undefined}
                          onClick={() => setEscolhida(chave(s))}
                          style={{ cursor: 'pointer' }}
                        >
                          <td>
                            {nomeDoLocal(s)}
                            <div className="cad-sub">
                              {u.produtividade !== null
                                ? `${u.produtividade.toLocaleString('pt-BR')} ${u.unidadeDaProdutividade ?? ''}`
                                : 'produtividade não informada'}
                            </div>
                          </td>
                          <td className="cad-mono">{rotuloDaSafra(u, s)}</td>
                          <td className="terr-num">
                            <span className="cad-mono">{reais(u.custoOperacionalHa)}</span>
                            <div className="cad-sub">
                              {reais(u.custoOperacionalUnidade)} / {s.unidadeComercial}
                            </div>
                          </td>
                          <td className="terr-num">
                            {u.custoTotalHa !== null ? (
                              <>
                                <span className="cad-mono">{reais(u.custoTotalHa)}</span>
                                {u.custoTotalUnidade !== null && (
                                  <div className="cad-sub">
                                    {reais(u.custoTotalUnidade)} / {s.unidadeComercial}
                                  </div>
                                )}
                              </>
                            ) : (
                              <span className="cad-sub" title="A CONAB publicou só até o custo operacional nesta aba">
                                a CONAB parou no operacional
                              </span>
                            )}
                          </td>
                          <td className="cad-mono terr-num">{s.safras.length}</td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            {selecionada && (
              <div className="card cad-cartao terr-cartao">
                <div className="card-title">
                  {comoSeEscreve(selecionada.cultura)} em {nomeDoLocal(selecionada)} — custo total por hectare
                </div>
                <div className="cad-sub">
                  {selecionada.safras.length} safras na série da CONAB
                  {pontos.length < selecionada.safras.length &&
                    ` · ${selecionada.safras.length - pontos.length} sem custo total (a CONAB parou no operacional) ficam fora do gráfico`}
                </div>
                {pontos.length > 1 ? (
                  <GraficoLinhaMensal
                    rotulos={pontos.map((p) => p.rotulo)}
                    valores={pontos.map((p) => p.valor)}
                    largura={460}
                    altura={240}
                    formatar={reais}
                  />
                ) : (
                  <p className="cad-sub">A série tem menos de duas safras com custo total — não há linha para desenhar.</p>
                )}
                <SeloProcedencia procedencia={custos.procedencia} />
              </div>
            )}
          </div>
        </>
      )}
    </>
  );
}
