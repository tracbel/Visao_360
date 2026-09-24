/**
 * TERMO DE TROCA — a aba da maquete `momento-termo-de-troca.png` (fidelidade às
 * maquetes, fase 3).
 *
 * A PERGUNTA É "QUANTAS SACAS O PRODUTOR PRECISA PARA COMPRAR UM TRATOR", e ela
 * tem duas metades. A do PREÇO DA SAFRA existe: a CONAB e a Socicana, carregadas
 * todo mês (issue 66) — e a coluna do preço da commodity sai preenchida de
 * verdade. A do PREÇO DA MÁQUINA não existe: o preço de referência do trator,
 * por modelo e ao longo do tempo, é a issue 70. Sem ela não há sacas, variação,
 * melhor cultura de troca, tendência nem evolução.
 *
 * O LAYOUT DA MAQUETE FICA INTEIRO (decisão 1 do usuário): os quatro cartões, o
 * gráfico com a moldura, o comparativo com as culturas reais e a tabela — e no
 * lugar de cada número que depende do trator, o traço com o motivo. Uma divisão
 * por um preço de trator inventado daria um número plausível e errado.
 */

import { BarChart3, Sprout, Tractor, TrendingUp } from 'lucide-react';
import { useMemo } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { obterCatalogoDoMercado } from '../../../dados/api/potencial';
import { obterPrecosDeMercado } from '../../../dados/api/territorio';
import { useRecurso } from '../../../dados/api/useRecurso';
import type { SerieDePreco } from '../../../tipos/mercado';
import type { CulturaNoCatalogo } from '../../../tipos/potencial';
import { BlocoErro } from '../../cadastro/EstadosDeTela';
import { InfoTooltip } from '../../InfoTooltip';
import { frasesDaProcedencia } from '../../comum/comparacoes';
import { ValorAusente } from '../../comum/ValorAusente';
import { comAsCulturasDoMunicipioPrimeiro } from '../culturasDoMunicipio';
import { mesCurto, percentualComSinal, reais } from './formatos';
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

const DEPENDE_DO_TRATOR =
  'Depende do preço de referência do trator (issue 70): o preço de máquina por modelo, ao longo do tempo, não ' +
  'existe no CRM. O preço da saca existe (CONAB e Socicana, issue 66) — sozinho ele não responde quantas sacas ' +
  'compram uma máquina, e a tela não divide por um preço de trator inventado.';

/** A série do kg de ATR acumulado da safra: útil no gráfico de preço, redundante aqui. */
const NIVEL_ACUMULADO = 'ACUMULADO DA SAFRA';

/** Produtos de preço que o texto-base cita e que não são lavoura — não se trocam por trator em sacas. */
const SEM_LAVOURA = ['BOI', 'LEITE'];

type CulturaDaTroca = { chave: string; nome: string; serie: SerieDePreco | null };

/** O preço do último mês, na unidade em que o mercado negocia. */
function ultimoPreco(s: SerieDePreco) {
  const ultimo = s.meses.at(-1);
  return ultimo ? { mes: ultimo.mes, valor: ultimo.valorEmReais * s.fatorComercial } : null;
}

/** O mesmo mês do ano anterior, quando a série já tem — a variação do PREÇO, e não do termo. */
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
): CulturaDaTroca[] {
  const utilizaveis = series.filter((s) => s.nivel !== NIVEL_ACUMULADO && s.meses.length > 0);

  if (catalogo && catalogo.length > 0)
    return comAsCulturasDoMunicipioPrimeiro(
      catalogo.filter((c) => c.estaAtiva && c.produtoDoPreco !== null),
      produtosDoMunicipio,
    ).map((c) => ({
      chave: c.codigo,
      nome: c.nome,
      serie: utilizaveis.find((s) => s.codigoNaFonte === c.produtoDoPreco) ?? null,
    }));

  return utilizaveis
    .filter((s) => !SEM_LAVOURA.some((p) => s.produto.toUpperCase().startsWith(p)))
    .sort((a, b) => a.produto.localeCompare(b.produto, 'pt-BR'))
    .map((s) => ({ chave: `${s.fonte}|${s.codigoNaFonte}`, nome: s.produto, serie: s }));
}

function Traco({ oQue }: { oQue: string }) {
  return (
    <>
      <span aria-hidden="true">—</span>
      <span className="cad-so-leitor">{oQue} sem dado</span>
    </>
  );
}

export function AbaTermoDeTroca({ produtosDoMunicipio = [] }: { produtosDoMunicipio?: readonly number[] }) {
  const { contexto } = useContextoDeAcesso();
  const precos = useRecurso((sinal) => obterPrecosDeMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const catalogo = useRecurso((sinal) => obterCatalogoDoMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);

  const culturas = useMemo(
    () => culturasDaTroca(precos.dados?.series ?? [], catalogo.dados?.culturas ?? null, produtosDoMunicipio),
    [precos.dados, catalogo.dados, produtosDoMunicipio],
  );

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
          valor={null}
          motivoSemDado={DEPENDE_DO_TRATOR}
          apoio="no trator base (issue 70)"
        />
        <CartaoDoMomento
          icone={BarChart3}
          tom="azul"
          rotulo="Variação (5 anos)"
          oQue="a variação do termo em 5 anos"
          valor={null}
          motivoSemDado={`${DEPENDE_DO_TRATOR} A variação em cinco anos pede o preço do trator nos cinco anos.`}
          apoio="sacas hoje contra cinco anos atrás"
        />
        <CartaoDoMomento
          icone={Sprout}
          tom="neutro"
          rotulo="Melhor cultura de troca"
          oQue="a melhor cultura de troca"
          valor={null}
          motivoSemDado={`${DEPENDE_DO_TRATOR} Sem as sacas de cada cultura não há a que precisa de menos.`}
          apoio="a que compra o trator com menos sacas"
        />
        <CartaoDoMomento
          icone={TrendingUp}
          tom="laranja"
          rotulo="Tendência atual"
          oQue="a tendência do termo de troca"
          valor={null}
          motivoSemDado={`${DEPENDE_DO_TRATOR} Sem a série do termo não há tendência a medir.`}
          apoio="poder de compra vs. trimestre anterior"
        />
      </FileiraDeCartoes>

      <LinhaDePaineis variante="troca">
        <PainelDoMomento
          titulo="Evolução do termo de troca"
          dica={`Sacas de cada cultura necessárias para comprar 1 trator, mês a mês, e a média de cinco anos. ${DEPENDE_DO_TRATOR}`}
          subtitulo="Número de sacas de cada cultura necessárias para comprar 1 trator."
          direita={
            <>
              <Seletor
                rotulo="Cultura"
                valor={culturas[0]?.chave ?? ''}
                opcoes={culturas.length > 0 ? culturas.map((c) => ({ id: c.chave, rotulo: c.nome })) : [{ id: '', rotulo: '—' }]}
                motivoDesligado="Escolher a cultura não mudaria nada enquanto não houver série do termo de troca (issue 70)."
              />
              <Legenda
                itens={[
                  { nome: 'Sacas necessárias', cor: '#0B5D2A' },
                  { nome: 'Média (5 anos)', cor: '#8FD19E' },
                ]}
              />
            </>
          }
        >
          <GraficoSemSerie altura={190} frase="Sem série do termo de troca" motivo={DEPENDE_DO_TRATOR} oQue="a evolução do termo de troca" />
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Comparativo por cultura"
          dica={`Sacas necessárias para comprar 1 trator e a variação no ano, por cultura. As culturas são as reais, com preço carregado; o número espera o preço do trator. ${DEPENDE_DO_TRATOR}`}
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
              {/* AS CULTURAS SÃO AS REAIS, com o trilho da barra vazio: o lugar do
                  número existe, e o número espera o preço do trator (issue 70). */}
              {culturas.map((c) => (
                <li key={c.chave} className="mom-comparativo-linha" data-cultura={c.chave}>
                  <NomeDaCultura nome={c.nome} />
                  <span className="mom-barra" aria-hidden="true" />
                  <span className="mom-num">
                    <Traco oQue="sacas necessárias" />
                  </span>
                  <span className="mom-num">
                    <Traco oQue="variação anual" />
                  </span>
                  <MenuDaLinha rotulo={`O termo de troca de ${c.nome}`}>
                    <p>{DEPENDE_DO_TRATOR}</p>
                  </MenuDaLinha>
                </li>
              ))}
            </ul>
          )}
        </PainelDoMomento>
      </LinhaDePaineis>

      <PainelDoMomento
        titulo="Detalhamento do termo de troca por cultura"
        dica={
          'O preço da commodity é o último mês da série da CONAB (ou da Socicana, na cana), na unidade em que o ' +
          'mercado negocia — é preço de São Paulo, recebido pelo produtor. O ⋮ de cada linha diz o mês, a fonte e a ' +
          `variação do preço em um ano. ${DEPENDE_DO_TRATOR}`
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
                  <InfoTooltip rotulo="Por que o preço do trator não aparece" texto={DEPENDE_DO_TRATOR} />
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
                    <td className="mom-num">
                      <Traco oQue="preço do trator" />
                    </td>
                    <td className="mom-num">
                      <Traco oQue="sacas necessárias" />
                    </td>
                    <td className="mom-num">
                      <Traco oQue="variação anual" />
                    </td>
                    <td>
                      <Traco oQue="tendência" />
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
                            <p>A variação acima é do PREÇO da saca; a do termo de troca espera o preço do trator (issue 70).</p>
                            {/* CADA SÉRIE DIZ A PRÓPRIA ORIGEM (issue 167), e não um selo do painel. */}
                            {c.serie.procedencia && <p>{frasesDaProcedencia(c.serie.procedencia)}</p>}
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
