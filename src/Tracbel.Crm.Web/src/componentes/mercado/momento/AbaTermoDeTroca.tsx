/**
 * TERMO DE TROCA — a aba da maquete `momento-termo-de-troca.png` (fidelidade às
 * maquetes, fase 3).
 *
 * A PERGUNTA É "QUANTAS SACAS O PRODUTOR PRECISA PARA COMPRAR UM TRATOR", e ela
 * tem duas metades. A do PREÇO DA SAFRA é a série da CONAB e da Socicana,
 * carregada todo mês (issue 66). A do PREÇO DO TRATOR é a mediana dos tratores
 * vendidos no mês, pela nota do Protheus casada com a venda do ART (issue 70,
 * D-P06 e D-P12, decididas pelo Ricardo em 27/09/2026) — o mesmo trator base
 * para as seis culturas. A conta mora em `termoDeTroca.ts`; aqui só se desenha.
 *
 * O LAYOUT DA MAQUETE FICA INTEIRO (decisão 1 do usuário): os quatro cartões, o
 * gráfico com a moldura, o comparativo e a tabela. Enquanto a rotina do preço
 * da máquina não roda, cada número que depende do trator sai com o traço e o
 * motivo — uma divisão por um preço de trator inventado daria um número
 * plausível e errado.
 */

import { BarChart3, Sprout, Tractor, TrendingUp } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { obterCatalogoDoMercado } from '../../../dados/api/potencial';
import { obterPrecosDeMercado } from '../../../dados/api/territorio';
import { useRecurso } from '../../../dados/api/useRecurso';
import type { SerieDePreco, SerieDePrecoDeMaquina } from '../../../tipos/mercado';
import type { CulturaNoCatalogo } from '../../../tipos/potencial';
import { BlocoErro } from '../../cadastro/EstadosDeTela';
import { InfoTooltip } from '../../InfoTooltip';
import { MolduraDeGrafico } from '../../MolduraDeGrafico';
import { frasesDaProcedencia } from '../../comum/comparacoes';
import { ValorAusente } from '../../comum/ValorAusente';
import { comAsCulturasDoMunicipioPrimeiro } from '../culturasDoMunicipio';
import { mesCurto, numero, percentualComSinal, reais } from './formatos';
import { CORES_DO_TERMO, GraficoDoTermoDeTroca } from './GraficoDoTermoDeTroca';
import { NomeDaCultura } from './IconeDaCultura';
import {
  CartaoDoMomento,
  FileiraDeCartoes,
  GraficoSemSerie,
  Legenda,
  LinhaDePaineis,
  MenuDaLinha,
  PainelDoMomento,
  Seletor,
} from './pecas';
import {
  mediaDoTermo,
  mesesAntes,
  poderDeCompra,
  serieDoTermo,
  tratorBase,
  variacaoDoTermo,
  type TermoNoMes,
} from './termoDeTroca';

const SEM_TRATOR =
  'O preço do trator é a mediana dos tratores vendidos no mês, pela nota do Protheus casada com a venda do ART (issue 70). ' +
  'A rotina "Preço da máquina (nota do Protheus)", em Configurações › Integrações, ainda não trouxe nenhum mês de trator — ' +
  'e a tela não divide o preço da saca por um preço de trator inventado.';

const SEM_MES_EM_COMUM =
  'O preço do trator e o desta cultura ainda não têm um mês em comum: o termo é a divisão dos dois no MESMO mês, e mês ' +
  'sem venda de trator não tem mediana.';

const O_QUE_E_O_TRATOR_BASE =
  'O trator base é a MEDIANA dos tratores vendidos pela Tracbel no mês, pelo valor da nota do Protheus (sem IPI e sem ' +
  'ICMS-ST), casada com a venda do ART — o mesmo trator para todas as culturas (D-P06, decidida em 27/09/2026).';

/** A série do kg de ATR acumulado da safra: útil no gráfico de preço, redundante aqui. */
const NIVEL_ACUMULADO = 'ACUMULADO DA SAFRA';

/** Produtos de preço que o texto-base cita e que não são lavoura — não se trocam por trator em sacas. */
const SEM_LAVOURA = ['BOI', 'LEITE'];

type CulturaDaTroca = { chave: string; nome: string; serie: SerieDePreco | null; termo: TermoNoMes[] };

/** O preço do último mês, na unidade em que o mercado negocia. */
function ultimoPreco(s: SerieDePreco) {
  const ultimo = s.meses.at(-1);
  return ultimo ? { mes: ultimo.mes, valor: ultimo.valorEmReais * s.fatorComercial } : null;
}

/** O mesmo mês do ano anterior, quando a série já tem — a variação do PREÇO da saca, e não do termo. */
function variacaoDoPrecoEmUmAno(s: SerieDePreco): number | null {
  const ultimo = s.meses.at(-1);
  if (!ultimo) return null;
  const [ano, mes] = ultimo.mes.split('-');
  const anterior = s.meses.find((m) => m.mes === `${Number(ano) - 1}-${mes}-01`);
  return anterior && anterior.valorEmReais > 0 ? ultimo.valorEmReais / anterior.valorEmReais - 1 : null;
}

/**
 * AS CULTURAS DA TROCA SÃO AS DO CATÁLOGO com fonte de preço (issue 165), com as
 * do município escolhido na frente (issue 168). Sem o catálogo, as séries de
 * lavoura da CONAB entram pelo nome da fonte — em vez de a aba ficar vazia por
 * uma lista ausente.
 */
function culturasDaTroca(
  series: readonly SerieDePreco[],
  catalogo: readonly CulturaNoCatalogo[] | null,
  produtosDoMunicipio: readonly number[],
  trator: SerieDePrecoDeMaquina | null,
): CulturaDaTroca[] {
  const utilizaveis = series.filter((s) => s.nivel !== NIVEL_ACUMULADO && s.meses.length > 0);
  const termoDe = (serie: SerieDePreco | null) => (serie && trator ? serieDoTermo(serie, trator) : []);

  if (catalogo && catalogo.length > 0)
    return comAsCulturasDoMunicipioPrimeiro(
      catalogo.filter((c) => c.estaAtiva && c.produtoDoPreco !== null),
      produtosDoMunicipio,
    ).map((c) => {
      const serie = utilizaveis.find((s) => s.codigoNaFonte === c.produtoDoPreco) ?? null;
      return { chave: c.codigo, nome: c.nome, serie, termo: termoDe(serie) };
    });

  return utilizaveis
    .filter((s) => !SEM_LAVOURA.some((p) => s.produto.toUpperCase().startsWith(p)))
    .sort((a, b) => a.produto.localeCompare(b.produto, 'pt-BR'))
    .map((s) => ({ chave: `${s.fonte}|${s.codigoNaFonte}`, nome: s.produto, serie: s, termo: termoDe(s) }));
}

function Traco({ oQue }: { oQue: string }) {
  return (
    <>
      <span aria-hidden="true">—</span>
      <span className="cad-so-leitor">{oQue} sem dado</span>
    </>
  );
}

/** As unidades comerciais que compram o trator, com uma casa só abaixo de dez (a cana, em toneladas, pede mais). */
function unidades(v: number): string {
  return numero(v, v < 10 ? 1 : 0);
}

export function AbaTermoDeTroca({ produtosDoMunicipio = [] }: { produtosDoMunicipio?: readonly number[] }) {
  const { contexto } = useContextoDeAcesso();
  const precos = useRecurso((sinal) => obterPrecosDeMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const catalogo = useRecurso((sinal) => obterCatalogoDoMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);

  const trator = useMemo(() => tratorBase(precos.dados?.maquinas), [precos.dados]);

  const culturas = useMemo(
    () => culturasDaTroca(precos.dados?.series ?? [], catalogo.dados?.culturas ?? null, produtosDoMunicipio, trator),
    [precos.dados, catalogo.dados, produtosDoMunicipio, trator],
  );

  const comTermo = culturas.filter((c) => c.termo.length > 0);
  const [escolhida, setEscolhida] = useState<string | null>(null);
  const noGrafico = comTermo.find((c) => c.chave === escolhida) ?? comTermo[0] ?? null;

  // O CARTÃO DE CIMA É DA PRIMEIRA CULTURA COM TERMO — a do município escolhido, quando há (issue 168).
  const principal = comTermo[0] ?? null;
  const ultimoDaPrincipal = principal?.termo.at(-1) ?? null;
  const cincoAnos = principal ? variacaoDoTermo(principal.termo, 60) : null;
  const tendencia = principal ? poderDeCompra(variacaoDoTermo(principal.termo, 3)) : null;

  const melhor = comTermo.reduce<CulturaDaTroca | null>(
    (m, c) => (m === null || c.termo.at(-1)!.unidades < m.termo.at(-1)!.unidades ? c : m),
    null,
  );

  const maiorDoComparativo = Math.max(0, ...comTermo.map((c) => c.termo.at(-1)!.unidades));

  const semTermo = trator === null ? SEM_TRATOR : SEM_MES_EM_COMUM;

  const semPreco = precos.carregando
    ? 'Lendo os preços da CONAB e da Socicana…'
    : precos.erro
      ? 'A leitura dos preços não respondeu.'
      : 'Esta cultura não tem série de preço carregada (CONAB e Socicana, issue 66).';

  return (
    <div className="mom-painel-da-aba" data-bloco="termo-de-troca">
      <FileiraDeCartoes>
        <CartaoDoMomento
          icone={Tractor}
          tom="verde"
          rotulo="Sacas para comprar 1 trator"
          oQue="as sacas por trator"
          dica={O_QUE_E_O_TRATOR_BASE}
          procedencia={trator?.procedencia ?? null}
          valor={ultimoDaPrincipal ? unidades(ultimoDaPrincipal.unidades) : null}
          unidade={principal?.serie?.unidadeComercial}
          motivoSemDado={semTermo}
          apoio={principal && ultimoDaPrincipal ? `de ${principal.nome} · trator de ${mesCurto(ultimoDaPrincipal.mes)}` : 'no trator base'}
        />
        <CartaoDoMomento
          icone={BarChart3}
          tom="azul"
          rotulo="Variação (5 anos)"
          oQue="a variação do termo em 5 anos"
          valor={cincoAnos === null ? null : percentualComSinal(cincoAnos)}
          motivoSemDado={
            trator === null || !ultimoDaPrincipal
              ? semTermo
              : `A variação em cinco anos compara ${mesCurto(ultimoDaPrincipal.mes)} com ${mesCurto(mesesAntes(ultimoDaPrincipal.mes, 60))}, e as duas séries ainda não chegam lá: a do trator começa em ${mesCurto(trator.meses[0].mes)}, e a da CONAB é acumulada pelo CRM mês a mês.`
          }
          apoio="sacas hoje contra cinco anos atrás"
        />
        <CartaoDoMomento
          icone={Sprout}
          tom="neutro"
          rotulo="Melhor cultura de troca"
          oQue="a melhor cultura de troca"
          dica="A cultura que entrega MENOS unidades pelo mesmo trator, no último mês de cada uma. As unidades são as do mercado de cada cultura — saca, caixa, tonelada —, então a comparação é de quantidade entregue, e não de peso."
          valor={melhor ? melhor.nome : null}
          motivoSemDado={semTermo}
          apoio={
            melhor
              ? `${unidades(melhor.termo.at(-1)!.unidades)} ${melhor.serie?.unidadeComercial ?? ''} por trator`
              : 'a que compra o trator com menos sacas'
          }
        />
        <CartaoDoMomento
          icone={TrendingUp}
          tom="laranja"
          rotulo="Tendência atual"
          oQue="a tendência do termo de troca"
          dica="O poder de compra da safra contra o mesmo trator, no último mês contra três meses antes: precisar de 10% mais sacas é comprar cerca de 9% menos trator."
          valor={tendencia === null ? null : percentualComSinal(tendencia, 1)}
          motivoSemDado={
            trator === null || !principal
              ? semTermo
              : `A tendência compara o último mês com três meses antes, e as duas séries de ${principal.nome} ainda não têm os dois meses — mês sem venda de trator não tem mediana.`
          }
          apoio={principal ? `poder de compra de ${principal.nome} vs. trimestre anterior` : 'poder de compra vs. trimestre anterior'}
        />
      </FileiraDeCartoes>

      <LinhaDePaineis variante="troca">
        <PainelDoMomento
          titulo="Evolução do termo de troca"
          dica={`Unidades de cada cultura necessárias para comprar 1 trator, mês a mês, e a média da série. Só os meses que as duas séries têm. ${O_QUE_E_O_TRATOR_BASE}`}
          subtitulo="Número de sacas de cada cultura necessárias para comprar 1 trator."
          direita={
            <>
              <Seletor
                rotulo="Cultura"
                valor={noGrafico?.chave ?? ''}
                opcoes={comTermo.length > 0 ? comTermo.map((c) => ({ id: c.chave, rotulo: c.nome })) : [{ id: '', rotulo: '—' }]}
                aoMudar={comTermo.length > 1 ? setEscolhida : undefined}
                motivoDesligado={comTermo.length === 0 ? semTermo : undefined}
              />
              <Legenda
                itens={[
                  { nome: 'Sacas necessárias', cor: CORES_DO_TERMO.unidades },
                  { nome: 'Média da série', cor: CORES_DO_TERMO.media },
                ]}
              />
            </>
          }
        >
          {noGrafico ? (
            <MolduraDeGrafico altura={190}>
              {(l, a) => (
                <GraficoDoTermoDeTroca
                  termo={noGrafico.termo}
                  media={mediaDoTermo(noGrafico.termo)}
                  unidade={noGrafico.serie?.unidadeComercial ?? ''}
                  largura={l}
                  altura={a}
                />
              )}
            </MolduraDeGrafico>
          ) : (
            <GraficoSemSerie altura={190} frase="Sem série do termo de troca" motivo={semTermo} oQue="a evolução do termo de troca" />
          )}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Comparativo por cultura"
          dica={`Unidades necessárias para comprar 1 trator no último mês de cada cultura, e a variação contra o mesmo mês do ano anterior. ${O_QUE_E_O_TRATOR_BASE}`}
          subtitulo="Número de sacas necessárias para comprar 1 trator e variação no ano."
        >
          {precos.erro ? (
            <BlocoErro erro={precos.erro} aoTentarDeNovo={precos.recarregar} />
          ) : (
            <ul className="mom-comparativo" aria-label="Sacas por trator, por cultura">
              <li className="mom-comparativo-linha mom-ranking-cabecalho" aria-hidden="true">
                <span>Cultura</span>
                <span>Sacas necessárias</span>
                <span />
                <span className="mom-num">Variação anual</span>
                <span />
              </li>
              {culturas.length === 0 && !precos.carregando && (
                <li className="mom-comparativo-linha">
                  <span>Nenhuma cultura com série de preço carregada.</span>
                </li>
              )}
              {culturas.map((c) => {
                const ultimo = c.termo.at(-1) ?? null;
                const noAno = variacaoDoTermo(c.termo, 12);
                return (
                  <li key={c.chave} className="mom-comparativo-linha" data-cultura={c.chave}>
                    <NomeDaCultura nome={c.nome} />
                    <span className="mom-barra" aria-hidden="true">
                      {ultimo && maiorDoComparativo > 0 && (
                        <span style={{ width: `${(ultimo.unidades / maiorDoComparativo) * 100}%` }} />
                      )}
                    </span>
                    <span className="mom-num">{ultimo ? unidades(ultimo.unidades) : <Traco oQue="sacas necessárias" />}</span>
                    <span className="mom-num">{noAno === null ? <Traco oQue="variação anual" /> : percentualComSinal(noAno)}</span>
                    <MenuDaLinha rotulo={`O termo de troca de ${c.nome}`}>
                      {ultimo && c.serie ? (
                        <dl>
                          <dt>Mês</dt>
                          <dd>{mesCurto(ultimo.mes)}</dd>
                          <dt>Trator base</dt>
                          <dd>
                            {reais(ultimo.precoDoTrator, 0)} — mediana de {ultimo.notasDoTrator}{' '}
                            {ultimo.notasDoTrator === 1 ? 'nota' : 'notas'}
                          </dd>
                          <dt>Preço da unidade</dt>
                          <dd>
                            {reais(ultimo.precoDaUnidade)} / {c.serie.unidadeComercial}
                          </dd>
                          <dt>Variação no ano</dt>
                          <dd>{noAno === null ? 'as duas séries ainda não têm o mesmo mês do ano anterior' : percentualComSinal(noAno, 1)}</dd>
                        </dl>
                      ) : (
                        <p>{c.serie ? semTermo : semPreco}</p>
                      )}
                    </MenuDaLinha>
                  </li>
                );
              })}
            </ul>
          )}
        </PainelDoMomento>
      </LinhaDePaineis>

      <PainelDoMomento
        titulo="Detalhamento do termo de troca por cultura"
        dica={
          'O preço da commodity é o último mês da série da CONAB (ou da Socicana, na cana), na unidade em que o ' +
          'mercado negocia — é preço de São Paulo, recebido pelo produtor. O trator base e as sacas são do último mês ' +
          `que as duas séries têm. ${O_QUE_E_O_TRATOR_BASE}`
        }
        subtitulo="Preços das commodities, preço do trator base e sacas necessárias."
      >
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela">
            <thead>
              <tr>
                <th scope="col">Cultura</th>
                <th scope="col" className="mom-num">
                  Preço da commodity
                </th>
                <th scope="col" className="mom-num">
                  Preço do trator base (R$){' '}
                  <InfoTooltip rotulo="O que é o trator base" texto={trator ? O_QUE_E_O_TRATOR_BASE : SEM_TRATOR} />
                </th>
                <th scope="col" className="mom-num">
                  Sacas necessárias
                </th>
                <th scope="col" className="mom-num">
                  Variação anual
                </th>
                <th scope="col">Tendência</th>
                <th scope="col" className="mom-acoes">
                  <span className="cad-so-leitor">Detalhes</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {culturas.length === 0 && (
                <tr className="mom-tabela-vazia">
                  <td colSpan={7}>{precos.carregando ? 'Lendo os preços…' : 'Nenhuma cultura com série de preço carregada.'}</td>
                </tr>
              )}
              {culturas.map((c) => {
                const preco = c.serie ? ultimoPreco(c.serie) : null;
                const emUmAno = c.serie ? variacaoDoPrecoEmUmAno(c.serie) : null;
                const ultimo = c.termo.at(-1) ?? null;
                const noAno = variacaoDoTermo(c.termo, 12);
                const noTrimestre = poderDeCompra(variacaoDoTermo(c.termo, 3));
                return (
                  <tr key={c.chave} data-cultura={c.chave}>
                    <th scope="row">
                      <NomeDaCultura nome={c.nome} />
                    </th>
                    <td className="mom-num">
                      {preco && c.serie ? (
                        <>
                          {reais(preco.valor)} <span className="mom-painel-subtitulo">/ {c.serie.unidadeComercial}</span>
                        </>
                      ) : (
                        <ValorAusente motivo={semPreco} oQue={`o preço de ${c.nome}`} />
                      )}
                    </td>
                    <td className="mom-num">{ultimo ? reais(ultimo.precoDoTrator, 0) : <Traco oQue="preço do trator" />}</td>
                    <td className="mom-num">{ultimo ? unidades(ultimo.unidades) : <Traco oQue="sacas necessárias" />}</td>
                    <td className="mom-num">{noAno === null ? <Traco oQue="variação anual" /> : percentualComSinal(noAno)}</td>
                    <td>
                      {noTrimestre === null ? (
                        <Traco oQue="tendência" />
                      ) : (
                        `${noTrimestre >= 0 ? 'poder de compra ↑' : 'poder de compra ↓'} ${percentualComSinal(noTrimestre)}`
                      )}
                    </td>
                    <td className="mom-acoes">
                      <MenuDaLinha rotulo={`O preço de ${c.nome}`}>
                        {c.serie && preco ? (
                          <>
                            <dl>
                              <dt>Mês</dt>
                              <dd>{mesCurto(preco.mes)}</dd>
                              <dt>Fonte</dt>
                              <dd>
                                {c.serie.fonte} · {c.serie.classificacao}
                              </dd>
                              <dt>Unidade</dt>
                              <dd>
                                {c.serie.unidadeComercial}
                                {c.serie.fatorComercial !== 1 &&
                                  ` (a fonte publica por ${c.serie.unidade}; × ${c.serie.fatorComercial.toLocaleString('pt-BR')})`}
                              </dd>
                              <dt>Preço em 1 ano</dt>
                              <dd>{emUmAno === null ? 'a série ainda não tem o mesmo mês do ano anterior' : percentualComSinal(emUmAno, 1)}</dd>
                              <dt>Meses na série</dt>
                              <dd>{c.serie.meses.length}</dd>
                            </dl>
                            <p>
                              A variação acima é do PREÇO da saca.{' '}
                              {ultimo ? `O termo de troca usa o trator de ${mesCurto(ultimo.mes)}.` : semTermo}
                            </p>
                            {/* CADA SÉRIE DIZ A PRÓPRIA ORIGEM (issue 167), e não um selo do painel. */}
                            {c.serie.procedencia && <p>{frasesDaProcedencia(c.serie.procedencia)}</p>}
                            {trator?.procedencia && <p>{frasesDaProcedencia(trator.procedencia)}</p>}
                          </>
                        ) : (
                          <p>{semPreco}</p>
                        )}
                      </MenuDaLinha>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </PainelDoMomento>
    </div>
  );
}
