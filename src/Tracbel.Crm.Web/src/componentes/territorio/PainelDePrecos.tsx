/**
 * Os preços das culturas em São Paulo, mês a mês (issue 66).
 *
 * É a base que o texto-base do potencial pede — "histórico de preço da
 * commodity em real e dólar por tipo de cultivo; não varia, só vamos
 * acrescentando" — e de onde sairão, na issue 73, o momento de preço, a
 * rentabilidade e o termo de troca. AQUI O PREÇO É MOSTRADO, NÃO INTERPRETADO:
 * nenhuma faixa de "mercado aquecido" aparece antes das decisões da issue 63.
 *
 * O VALOR APARECE NA UNIDADE DO MERCADO. A CONAB publica tudo por quilo; o
 * produtor pensa em saca, caixa e arroba. A tabela mostra a unidade comercial e
 * o balão do gráfico diz de onde veio a conversão.
 *
 * O HISTÓRICO É DO CRM. A CONAB publica só os últimos 12 meses; a série cresce
 * um mês por vez, a cada rodada mensal do servidor. A tela diz quantos meses
 * cada série já tem, para ninguém ler "12 meses" como "tudo que existe".
 */

import { InfoTooltip } from '../InfoTooltip';
import { Procedencia } from '../comum/Procedencia';
import { ValorAusente } from '../comum/ValorAusente';
import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { comAsCulturasDoMunicipioPrimeiro } from '../mercado/culturasDoMunicipio';
import { obterCatalogoDoMercado } from '../../dados/api/potencial';
import { obterPrecosDeMercado } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type { SerieDePreco } from '../../tipos/mercado';
import type { CulturaNoCatalogo } from '../../tipos/potencial';

/**
 * AS CULTURAS DO CATÁLOGO, e não uma lista escrita aqui (issue 165).
 *
 * Até aqui esta linha era um vetor com 'CAFE', 'SOJA', 'MILHO'… no código da
 * tela: cultura nova exigia publicação, e a mesma lista estava escrita de outro
 * jeito no painel de custos. Agora a tela pede o catálogo e ordena por ele — o
 * que vier primeiro no catálogo aparece primeiro, e o resto da CONAB fica atrás
 * do "mostrar todos".
 *
 * O boi e o leite continuam aqui porque NÃO são cultura: são produtos de preço
 * que o texto-base cita e que não entram no catálogo de lavoura.
 */
const SEM_LAVOURA = ['BOI', 'LEITE'];

/** A série do kg de ATR acumulado da safra: útil no gráfico, redundante na tabela principal. */
const NIVEL_ACUMULADO = 'ACUMULADO DA SAFRA';

type Moeda = 'reais' | 'dolares';

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/** `2026-08-01` → `ago/26`. */
function rotuloDoMes(iso: string): string {
  const [ano, mes] = iso.split('-');
  return `${MESES[Number(mes) - 1]}/${ano.slice(2)}`;
}

function chave(s: SerieDePreco): string {
  return `${s.fonte}|${s.codigoNaFonte}|${s.nivel}`;
}

/**
 * A ordem da série na tabela, pelo CATÁLOGO (issue 165).
 *
 * A cultura casa pelo produto que ela declara na fonte de preço (`produtoDoPreco`), que é o
 * `id_produto` da CONAB — e não pelo nome, que cada fonte escreve de um jeito. Série que não é de
 * cultura nenhuma, mas é do texto-base (boi, leite), vem logo atrás; o resto fica no "mostrar todos".
 */
function ordemDePrincipal(s: SerieDePreco, culturas: CulturaNoCatalogo[]): number {
  const i = culturas.findIndex((c) => c.produtoDoPreco !== null && c.produtoDoPreco === s.codigoNaFonte);
  if (i >= 0) return i;

  const j = SEM_LAVOURA.findIndex((p) => s.produto.toUpperCase().startsWith(p));
  return j < 0 ? Number.MAX_SAFE_INTEGER : culturas.length + j;
}

/** O valor na unidade comercial, na moeda pedida. */
function naUnidadeComercial(s: SerieDePreco, valorNaFonte: number | null): number | null {
  return valorNaFonte === null ? null : valorNaFonte * s.fatorComercial;
}

/**
 * As casas decimais de uma SÉRIE, decididas pelo maior valor dela e não valor a valor.
 *
 * O kg de ATR vale menos de R$ 1: duas casas o arredondariam a "R$ 0,87" e esconderiam a variação que
 * a Socicana publica na quarta casa. Mas decidir por valor fazia o zero do eixo sair "R$ 0,0000"
 * num gráfico de café a R$ 2.000.
 */
function casasDe(maior: number): number {
  return Math.abs(maior) < 10 ? 4 : 2;
}

function formatarMoeda(valor: number, moeda: Moeda, casas = casasDe(valor)): string {
  return valor.toLocaleString('pt-BR', {
    style: 'currency',
    currency: moeda === 'reais' ? 'BRL' : 'USD',
    minimumFractionDigits: casas,
    maximumFractionDigits: casas,
  });
}

/**
 * O mesmo mês do ano anterior, quando a série já tem. Não é o índice de
 * momento da issue 73 (12 meses ÷ 12 anteriores) — é a comparação simples que o
 * histórico acumulado já permite.
 */
function variacaoEmUmAno(s: SerieDePreco): number | null {
  const ultimo = s.meses.at(-1);
  if (!ultimo) return null;
  const [ano, mes] = ultimo.mes.split('-');
  const alvo = `${Number(ano) - 1}-${mes}-01`;
  const anterior = s.meses.find((m) => m.mes === alvo);
  return anterior ? ultimo.valorEmReais / anterior.valorEmReais - 1 : null;
}

export function PainelDePrecos({ produtosDoMunicipio = [] }: { produtosDoMunicipio?: readonly number[] } = {}) {
  const { contexto } = useContextoDeAcesso();
  const precos = useRecurso((sinal) => obterPrecosDeMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);

  // O CATÁLOGO DECIDE QUAIS SÃO AS CULTURAS (issue 165). Se ele não vier — permissão ou banco novo —,
  // a tabela mostra tudo em ordem alfabética, em vez de esconder série por causa de uma lista ausente.
  const catalogo = useRecurso((sinal) => obterCatalogoDoMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);
  // AS CULTURAS DO MUNICÍPIO ESCOLHIDO VÊM PRIMEIRO (issue 168). O preço continua
  // sendo o de São Paulo — muda a ordem, não o número.
  const culturas = useMemo(
    () =>
      comAsCulturasDoMunicipioPrimeiro(
        (catalogo.dados?.culturas ?? []).filter((c: CulturaNoCatalogo) => c.estaAtiva),
        produtosDoMunicipio,
      ),
    [catalogo.dados, produtosDoMunicipio],
  );

  const [todos, setTodos] = useState(false);
  const [moeda, setMoeda] = useState<Moeda>('reais');
  const [escolhida, setEscolhida] = useState<string | null>(null);

  const series = useMemo(
    () =>
      [...(precos.dados?.series ?? [])].sort(
        (a, b) => ordemDePrincipal(a, culturas) - ordemDePrincipal(b, culturas) || a.produto.localeCompare(b.produto),
      ),
    [precos.dados, culturas],
  );

  const naTabela = series.filter((s) => s.nivel !== NIVEL_ACUMULADO);
  const principais = naTabela.filter((s) => ordemDePrincipal(s, culturas) !== Number.MAX_SAFE_INTEGER);
  const visiveis = todos ? naTabela : principais;
  const selecionada = series.find((s) => chave(s) === escolhida) ?? principais[0] ?? naTabela[0] ?? null;

  // SEM useMemo: são no máximo algumas centenas de pontos, e \selecionada\ é derivada a cada render —
  // memoizar sobre ela não guardaria nada.
  const pontos = (selecionada?.meses ?? [])
    .map((m) => ({
      rotulo: rotuloDoMes(m.mes),
      valor: selecionada ? naUnidadeComercial(selecionada, moeda === 'reais' ? m.valorEmReais : m.valorEmDolares) : null,
    }))
    .filter((p): p is { rotulo: string; valor: number } => p.valor !== null);
  const grafico = selecionada ? { rotulos: pontos.map((p) => p.rotulo), valores: pontos.map((p) => p.valor) } : null;

  return (
    <>
      <div className="terr-secao-mercado">
        <h2 className="terr-secao-titulo">Preços das culturas — São Paulo</h2>
        <p className="terr-secao-subtitulo">
          Preço <strong>recebido pelo produtor</strong> (CONAB), preço do kg de ATR da cana (Socicana) e o dólar PTAX do mês
          (Banco Central) — todos de fonte aberta, carregados pelo servidor todo mês. A base <strong>só cresce</strong>: a
          CONAB publica os últimos 12 meses, e o que sai da janela dela continua aqui.
        </p>
      </div>

      {precos.carregando && <BlocoCarregando oQue="os preços das culturas" />}
      {precos.erro && <BlocoErro erro={precos.erro} aoTentarDeNovo={precos.recarregar} />}

      {precos.dados && series.length === 0 && (
        <div className="card cad-cartao terr-cartao">
          <BlocoVazio
            titulo="Os preços ainda não foram carregados neste banco"
            texto={
              <>
                A rota respondeu, mas não há nenhuma cotação gravada: a carga mensal de preços (
                <code>--somente-precos</code>) ainda não rodou aqui. Não é zero nem falta de permissão.
              </>
            }
          />
        </div>
      )}

      {precos.dados && series.length > 0 && (
        <div className="terr-grade-precos">
          <div className="card cad-cartao terr-cartao">
            <div className="card-title terr-precos-cabecalho">
              <span>Último mês de cada série</span>
              <span className="terr-precos-acoes">
                <label className="terr-filtro-inline">
                  <input type="radio" name="moeda" checked={moeda === 'reais'} onChange={() => setMoeda('reais')} /> R$
                </label>
                <label className="terr-filtro-inline">
                  <input type="radio" name="moeda" checked={moeda === 'dolares'} onChange={() => setMoeda('dolares')} /> US$
                </label>
              </span>
            </div>
            <div className="cad-tabela-wrap">
            <table className="cad-tabela terr-tabela-precos">
              <thead>
                <tr>
                  <th>Produto</th>
                  <th>Mês</th>
                  <th className="terr-num">Preço</th>
                  <th className="terr-num">
                    Em 1 ano{' '}
                    <InfoTooltip
                      rotulo="O que é a variação em 1 ano"
                      texto="O último mês contra o mesmo mês do ano anterior, em reais, quando a série já tem os dois pontos."
                    />
                  </th>
                  <th className="terr-num">
                    Meses <InfoTooltip rotulo="O que é a coluna Meses" texto="Quantos meses esta série já tem no CRM." />
                  </th>
                </tr>
              </thead>
              <tbody>
                {visiveis.map((s) => {
                  const ultimo = s.meses.at(-1)!;
                  const valor = naUnidadeComercial(s, moeda === 'reais' ? ultimo.valorEmReais : ultimo.valorEmDolares);
                  const variacao = variacaoEmUmAno(s);
                  const ativa = selecionada && chave(s) === chave(selecionada);
                  return (
                    <tr
                      key={chave(s)}
                      className={ativa ? 'terr-linha-ativa' : undefined}
                      onClick={() => setEscolhida(chave(s))}
                      style={{ cursor: 'pointer' }}
                    >
                      <td>
                        {s.produto}
                        {/* CADA SÉRIE DIZ A PRÓPRIA ORIGEM (issue 167): fonte, código na fonte e o
                            intervalo que ESTA série tem vêm do contrato, e não de um selo do painel. */}
                        <Procedencia procedencia={s.procedencia} oQue={`o preço de ${s.produto.toLowerCase()}`} />
                        <div className="cad-sub">
                          {s.classificacao} · {s.fonte}
                        </div>
                      </td>
                      <td className="cad-mono">{rotuloDoMes(ultimo.mes)}</td>
                      <td className="terr-num">
                        <span className="cad-mono">
                          {valor === null ? (
                            <ValorAusente motivo="O mês ainda não tem dólar PTAX carregado." oQue="o preço em dólar" />
                          ) : (
                            formatarMoeda(valor, moeda)
                          )}
                        </span>
                        <div className="cad-sub">por {s.unidadeComercial}</div>
                      </td>
                      <td className="cad-mono terr-num">
                        {variacao === null ? (
                          <ValorAusente
                            motivo="A série ainda não tem o mesmo mês do ano anterior — sem os dois pontos não há variação a calcular."
                            oQue="a variação em 1 ano"
                          />
                        ) : (
                          `${variacao >= 0 ? '+' : ''}${(variacao * 100).toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`
                        )}
                      </td>
                      <td className="cad-mono terr-num">{s.meses.length}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
            </div>
            {naTabela.length > principais.length && (
              <button type="button" className="btn btn-ghost btn-sm" onClick={() => setTodos((t) => !t)}>
                {todos ? 'Mostrar só as culturas do potencial' : `Mostrar todos os ${naTabela.length} produtos da CONAB e da Socicana`}
              </button>
            )}
          </div>

          {selecionada && grafico && (
            <div className="card cad-cartao terr-cartao">
              <div className="card-title">
                {selecionada.produto} — {moeda === 'reais' ? 'R$' : 'US$'} por {selecionada.unidadeComercial}
              </div>
              <div className="cad-sub">
                {selecionada.classificacao} · {selecionada.fonte} · {selecionada.nivel.toLowerCase()}
                {selecionada.fatorComercial !== 1 &&
                  ` · a fonte publica por ${selecionada.unidade}; convertido × ${selecionada.fatorComercial.toLocaleString('pt-BR')}`}
              </div>
              {grafico.valores.length > 0 ? (
                // A LARGURA VEM DO CARTÃO, e não de uma constante: os 560 px fixos passavam da borda abaixo
                // de 1.280 px e sobravam no meio do cartão numa tela larga (medido em 21/09/2026).
                <MolduraDeGrafico altura={240}>
                  {(l, a) => (
                    <GraficoLinhaMensal
                      rotulos={grafico.rotulos}
                      valores={grafico.valores}
                      largura={l}
                      altura={a}
                      formatar={(v) => formatarMoeda(v, moeda, casasDe(Math.max(...grafico.valores)))}
                    />
                  )}
                </MolduraDeGrafico>
              ) : (
                <p className="cad-sub">Nenhum mês desta série tem dólar PTAX ainda.</p>
              )}
              <SeloProcedencia procedencia={precos.procedencia} />
            </div>
          )}
        </div>
      )}
    </>
  );
}
