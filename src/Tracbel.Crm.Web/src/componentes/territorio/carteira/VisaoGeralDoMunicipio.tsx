/**
 * A VISÃO GERAL DA FICHA — a aba que abre primeiro (fidelidade às maquetes,
 * 23/09/2026 — fase 4; `territorio-ficha.png`, coluna da direita).
 *
 * Quatro blocos, na ordem da maquete: o resumo executivo (quatro cartõezinhos),
 * a lavoura e a estrutura lado a lado, e o cartão verde das oportunidades.
 *
 * O QUE A MAQUETE MOSTRA E O SISTEMA NÃO TEM fica no lugar dela com "—" e a
 * dica do motivo (decisão 1): a área total e o tamanho médio das propriedades
 * e a vocação agrícola. As máquinas potenciais e o potencial incremental em
 * reais passaram a vir do servidor (27/09/2026, issues 69 e 70). Nenhum número
 * daqui é da maquete — cada um vem do município escolhido.
 *
 * A VARIAÇÃO "vs. ano anterior" DOS CARTÕEZINHOS TEM NÚMERO ONDE HÁ NÚMERO
 * (27/09/2026): vendas e pós-venda contra o mesmo trecho do ano fiscal
 * anterior. As máquinas teóricas são estruturais, e os clientes com endereço
 * são o cadastro de hoje — cada um fica com o traço e o motivo DELE.
 *
 * A LAVOURA TRAZ TODAS AS CULTURAS (issue 168) quando o histórico do município
 * chega; antes dele, as culturas com regra e "Outros" até o total.
 *
 * TROCA DE TERMO (decisão 2): "Clientes ativos" vira "Clientes com endereço" —
 * o CRM não classifica cliente como ativo; o que ele sabe é quantos têm o
 * endereço principal neste município.
 */

import {
  ArrowRight,
  ChartColumn,
  ChartPie,
  Fence,
  House,
  Leaf,
  Ruler,
  ShoppingCart,
  Target,
  Tractor,
  Users,
  Wrench,
} from 'lucide-react';
import type { ComponentType, ReactNode } from 'react';
import type {
  CulturaNoEstado,
  HistoricoDoMunicipio,
  IndicadoresDoMunicipio,
  NumerosDeDecisao,
  ProcedenciasDoTerritorio,
  RegraDePotencialAplicada,
} from '../../../tipos/territorio';
import { InfoTooltip } from '../../InfoTooltip';
import { ValorAusente } from '../../comum/ValorAusente';
import { VariacaoAusente } from '../../mercado/VariacaoAusente';
import { VariacaoContraOAnoAnterior } from '../comparacao';
import { useComparacao } from '../contextoDaComparacao';
import { reaisCompactos } from '../escalas';
import { MOTIVO_SEM_PARQUE, nº, porcento } from '../indicadoresDaAdr';
import { lavouraDoMunicipio } from './lavouraDoMunicipio';
import type { PeriodoDaLeitura } from './periodo';

type Icone = ComponentType<{ size?: number; strokeWidth?: number }>;

function MiniCartao({
  id,
  icone: Icone,
  rotulo,
  valor,
  motivoSemValor,
  variacao,
}: {
  id: string;
  icone: Icone;
  rotulo: string;
  valor: string | null;
  motivoSemValor?: string;
  /** A linha "vs. ano anterior" — o número, ou o traço com o motivo deste cartão. */
  variacao: ReactNode;
}) {
  return (
    <div className="terr-ficha-mini" data-mini={id}>
      <div className="terr-ficha-mini-topo">
        <span className="terr-ficha-mini-icone" aria-hidden="true">
          <Icone size={17} strokeWidth={2.1} />
        </span>
        <span className="terr-ficha-mini-rotulo">{rotulo}</span>
      </div>
      <p className="terr-ficha-mini-valor">
        {valor ?? <ValorAusente motivo={motivoSemValor ?? 'Sem dado.'} oQue={rotulo.toLowerCase()} />}
      </p>
      {/* A MESMA LINHA DE VARIAÇÃO DOS CARTÕES DE MERCADO — o número contra o
          mesmo trecho do ano anterior, ou o traço com o motivo deste cartão. */}
      <p className="terr-ficha-mini-variacao">{variacao}</p>
    </div>
  );
}

function ItemDaEstrutura({ icone: Icone, valor, rotulo }: { icone: Icone; valor: ReactNode; rotulo: string }) {
  return (
    <li className="terr-ficha-estrutura-item">
      <span className="terr-ficha-estrutura-icone" aria-hidden="true">
        <Icone size={17} strokeWidth={1.9} />
      </span>
      <span>
        <strong className="terr-ficha-estrutura-valor">{valor}</strong>
        <span className="terr-ficha-estrutura-rotulo">{rotulo}</span>
      </span>
    </li>
  );
}

function ItemDeOportunidade({ icone: Icone, valor, rotulo }: { icone: Icone; valor: ReactNode; rotulo: string }) {
  return (
    <div className="terr-ficha-oport-item">
      <span className="terr-ficha-oport-icone" aria-hidden="true">
        <Icone size={20} strokeWidth={2.1} />
      </span>
      <span>
        <strong className="terr-ficha-oport-valor">{valor}</strong>
        <span className="terr-ficha-oport-rotulo">{rotulo}</span>
      </span>
    </div>
  );
}

export function VisaoGeralDoMunicipio({
  municipio,
  regras,
  culturasNoEstado,
  procedencias,
  periodo,
  numerosDeDecisao,
  historico,
  aoVerOportunidades,
}: {
  municipio: IndicadoresDoMunicipio;
  regras: RegraDePotencialAplicada[];
  culturasNoEstado: CulturaNoEstado[];
  procedencias: ProcedenciasDoTerritorio | null;
  periodo: PeriodoDaLeitura | null;
  /** Os números de decisão do recorte — daqui só sai a frase da oportunidade. */
  numerosDeDecisao: NumerosDeDecisao | null;
  /** O município ao longo do tempo — daqui sai a lavoura com todas as culturas; nulo antes de chegar. */
  historico: HistoricoDoMunicipio | null;
  aoVerOportunidades: () => void;
}) {
  const { cobertura, vendas, estrutura } = municipio;
  const motor = municipio.potencialEstrutural;
  const { periodoAnterior } = useComparacao();
  const lavoura = lavouraDoMunicipio(municipio, regras, culturasNoEstado, 4, historico?.lavoura ?? null);
  const antes = municipio.vendasNoPeriodoAnterior;
  const cobre = cobertura.vinculosComCadencia > 0 ? (100 * cobertura.cobertos) / cobertura.vinculosComCadencia : null;
  const potencial = municipio.numerosDeDecisao?.potencialIncremental ?? null;

  return (
    <div className="terr-ficha-visao">
      {/* ---------------------------------------------------------------
          RESUMO EXECUTIVO
          --------------------------------------------------------------- */}
      <section className="terr-ficha-bloco" aria-labelledby="ficha-resumo-titulo">
        <div className="terr-ficha-bloco-cabecalho">
          <div>
            <h3 id="ficha-resumo-titulo" className="terr-ficha-bloco-titulo">
              Resumo executivo
            </h3>
            <p className="terr-ficha-bloco-subtitulo">Principais indicadores do município no período selecionado.</p>
          </div>
          {/* O "12 MESES" DA MAQUETE MOSTRA o período da página e não o muda:
              a ficha é do mesmo recorte da tabela ao lado, e um período só
              dela faria a linha e a ficha discordarem. */}
          <span className="terr-ficha-periodo">
            <select disabled aria-label="Período do resumo" defaultValue="periodo">
              <option value="periodo">{periodo?.rotulo ?? 'Período da página'}</option>
            </select>
            <InfoTooltip
              rotulo="Por que o período do resumo não muda aqui"
              texto={`O período da ficha é o do filtro Período, no alto da página${periodo ? ` — ${periodo.rotulo}, ${periodo.intervalo}` : ''}. Ele muda lá, e vale para a tabela e para a ficha juntas.`}
            />
          </span>
        </div>

        <div className="terr-ficha-minis">
          <MiniCartao
            id="vendas"
            icone={ShoppingCart}
            rotulo="Vendas no período"
            valor={reaisCompactos(vendas.valorLiquido)}
            variacao={
              <VariacaoContraOAnoAnterior
                deQue="vendas no período"
                atual={vendas.valorLiquido}
                anterior={antes?.valorLiquido ?? null}
                motivoSemAnterior={periodoAnterior?.motivoSemVendas}
                formatar={reaisCompactos}
                compacta
              />
            }
          />
          <MiniCartao
            id="posVenda"
            icone={Wrench}
            rotulo="Pós-venda (provisório)"
            valor={reaisCompactos(vendas.posVenda)}
            variacao={
              <VariacaoContraOAnoAnterior
                deQue="pós-venda (provisório)"
                atual={vendas.posVenda}
                anterior={antes?.posVenda ?? null}
                motivoSemAnterior={periodoAnterior?.motivoSemVendas}
                formatar={reaisCompactos}
                compacta
              />
            }
          />
          <MiniCartao
            id="maquinas"
            icone={Tractor}
            rotulo="Máquinas teóricas"
            valor={motor?.parqueDeMaquinas == null ? null : nº(motor.parqueDeMaquinas)}
            motivoSemValor={
              motor ? MOTIVO_SEM_PARQUE[motor.motivoSemParque] : 'Nenhuma regra de potencial vigente alcança este município.'
            }
            variacao={
              <VariacaoAusente
                deQue="máquinas teóricas"
                compacta
                motivo="As máquinas teóricas são um número estrutural — a área da PAM dividida pela regra de hectares por máquina — e não uma medida do período: não há ano anterior para comparar."
              />
            }
          />
          <MiniCartao
            id="clientes"
            icone={Users}
            rotulo="Clientes com endereço"
            valor={nº(cobertura.clientes)}
            variacao={
              <VariacaoAusente
                deQue="clientes com endereço"
                compacta
                motivo="É o cadastro de hoje: o CRM guarda o endereço atual de cada cliente, e não o de um ano atrás — não há como contar os clientes com endereço aqui no ano anterior."
              />
            }
          />
        </div>
      </section>

      {/* ---------------------------------------------------------------
          LAVOURA E ESTRUTURA, lado a lado
          --------------------------------------------------------------- */}
      <div className="terr-ficha-duas">
        <section className="terr-ficha-cartao" aria-labelledby="ficha-lavoura-titulo" data-bloco-da-ficha="lavoura">
          <h3 id="ficha-lavoura-titulo" className="terr-ficha-cartao-titulo">
            Lavoura (área plantada)
            <InfoTooltip
              rotulo="De onde vem a lavoura do município"
              texto={
                <>
                  <p>
                    Área plantada da PAM (IBGE)
                    {lavoura?.ano ? `, ${lavoura.ano}` : ''}
                    {procedencias?.areaPlantada ? `, tabela ${procedencias.areaPlantada.tabela ?? '—'}` : ''}. A
                    participação é sobre a área plantada total do município
                    {lavoura?.totalPlantado != null ? ` (${nº(Math.round(lavoura.totalPlantado))} ha)` : ''}.
                  </p>
                  {lavoura?.todasAsCulturas ? (
                    <p>
                      São todas as culturas que a PAM divulgou com área aqui ({nº(lavoura.culturasDetalhadas)}), da
                      maior para a menor, cada uma no último ano em que a área dela foi divulgada
                      {lavoura.anoMaisAntigo !== null && lavoura.anoMaisAntigo !== lavoura.ano
                        ? ` (de ${lavoura.anoMaisAntigo} a ${lavoura.ano})`
                        : ''}
                      . As que não cabem nas linhas entram em "Outros", que é o total menos a soma das listadas.
                      Cultura sem área divulgada (sigilo do IBGE ou não cultivada) não entra na conta — ausência não é
                      zero. O café entra uma vez só, pelo total.
                    </p>
                  ) : (
                    <p>
                      Esta leitura traz o detalhe só das culturas com regra de potencial
                      {lavoura ? ` (${nº(lavoura.culturasDetalhadas)} aqui)` : ''}
                      {lavoura?.culturasComArea != null ? `, de ${nº(lavoura.culturasComArea)} com área divulgada` : ''}.
                      As demais entram em "Outros", que é o total menos a soma das listadas — nenhuma cultura é estimada.
                      {lavoura && lavoura.culturasSemAreaDivulgada > 0
                        ? ` ${nº(lavoura.culturasSemAreaDivulgada)} cultura(s) sem área divulgada no município (sigilo do IBGE ou não cultivada) não entram na conta — ausência não é zero.`
                        : ''}{' '}
                      O detalhe de todas as culturas chega com o histórico do município.
                    </p>
                  )}
                  <p>
                    A lista das culturas com regra, com colheita, produção e produtividade, está na aba Lavoura; a lavoura
                    ano a ano, na aba Histórico.
                  </p>
                </>
              }
            />
          </h3>
          {lavoura === null ? (
            <p className="terr-ficha-vazio">
              <ValorAusente motivo="A Produção Agrícola Municipal não foi carregada para este município." oQue="a lavoura" />
              <span>PAM não carregada aqui.</span>
            </p>
          ) : (
            <table className="terr-ficha-lavoura">
              <thead>
                <tr>
                  <th scope="col">Cultura</th>
                  <th scope="col" className="terr-num">
                    Área <span className="terr-ficha-unidade">(ha)</span>
                  </th>
                  <th scope="col">Participação</th>
                </tr>
              </thead>
              <tbody>
                {[...lavoura.linhas, ...(lavoura.outros ? [lavoura.outros] : [])].map((l, i) => (
                  <tr key={l.nome} data-cultura={l.nome}>
                    <td>{l.nome}</td>
                    <td className="terr-num">{nº(Math.round(l.areaHectares))}</td>
                    <td>
                      <span className="terr-ficha-fatia">
                        <span className="terr-ficha-fatia-numero">
                          {l.participacao > 0 && l.participacao < 1 ? '<1%' : `${Math.round(l.participacao)}%`}
                        </span>
                        <span className="terr-ficha-barra" aria-hidden="true">
                          <span
                            data-primeira={i === 0 ? 'true' : undefined}
                            style={{ width: `${Math.min(100, Math.max(2, l.participacao))}%` }}
                          />
                        </span>
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </section>

        <section className="terr-ficha-cartao" aria-labelledby="ficha-estrutura-titulo" data-bloco-da-ficha="estrutura">
          <h3 id="ficha-estrutura-titulo" className="terr-ficha-cartao-titulo">
            Estrutura agropecuária
            <InfoTooltip
              rotulo="De onde vem a estrutura agropecuária"
              texto={`Propriedades rurais: estabelecimentos agropecuários do Censo Agropecuário${estrutura.anoDoCenso ? ` ${estrutura.anoDoCenso}` : ''} (IBGE). Tratores, rebanho, faixas de tamanho e usinas estão na aba Estrutura.`}
            />
          </h3>
          <ul className="terr-ficha-estrutura">
            {/* A ÁREA DOS ESTABELECIMENTOS DO CENSO NÃO ESTÁ NA CARGA do
                território: o Censo entra com tratores, estabelecimentos e
                faixas de tamanho, e não com a área deles. */}
            <ItemDaEstrutura
              icone={Fence}
              rotulo="área total das propriedades"
              valor={
                <ValorAusente
                  motivo="A área dos estabelecimentos do Censo Agropecuário não está na carga do território — ela entra pelo épico de integração do IBGE (issue 174), ainda sem issue própria."
                  oQue="a área total das propriedades"
                />
              }
            />
            <ItemDaEstrutura
              icone={House}
              rotulo="propriedades rurais"
              valor={
                estrutura.estabelecimentos === null ? (
                  <ValorAusente
                    motivo="O IBGE suprimiu este número por sigilo: ele oculta o valor quando poucos estabelecimentos o compõem. Sigilo NÃO é zero."
                    oQue="as propriedades rurais"
                  />
                ) : (
                  nº(estrutura.estabelecimentos)
                )
              }
            />
            <ItemDaEstrutura
              icone={Ruler}
              rotulo="tamanho médio da propriedade"
              valor={
                <ValorAusente
                  motivo="É a área total das propriedades dividida pelo número delas — e a área total não está carregada (épico de integração do IBGE, issue 174)."
                  oQue="o tamanho médio da propriedade"
                />
              }
            />
            <ItemDaEstrutura
              icone={Leaf}
              rotulo="vocação agrícola"
              valor={
                <ValorAusente
                  motivo="Não há regra decidida que classifique a vocação de um município (Alta, Média…). Como o porte estrutural, um adjetivo só entra com bandas decididas (issue 166)."
                  oQue="a vocação agrícola"
                />
              }
            />
          </ul>
        </section>
      </div>

      {/* ---------------------------------------------------------------
          OPORTUNIDADES NO MUNICÍPIO — o cartão verde
          --------------------------------------------------------------- */}
      <section className="terr-ficha-oport" aria-labelledby="ficha-oport-titulo">
        <div className="terr-ficha-oport-cabecalho">
          <span className="terr-ficha-oport-marca" aria-hidden="true">
            <Target size={20} strokeWidth={2.2} />
          </span>
          <h3 id="ficha-oport-titulo" className="terr-ficha-oport-titulo">
            Oportunidades no município
          </h3>
          <button type="button" className="terr-ficha-ver" onClick={aoVerOportunidades}>
            Ver detalhes
            <ArrowRight size={14} strokeWidth={2.2} aria-hidden="true" />
          </button>
        </div>
        <div className="terr-ficha-oport-itens">
          <ItemDeOportunidade
            icone={ChartColumn}
            rotulo="potencial incremental"
            // O POTENCIAL INCREMENTAL DO MUNICÍPIO (issue 70, 27/09/2026): a oportunidade de cada categoria daqui
            // vezes o preço dela, montado no servidor. Parcial vai ao lado do número.
            valor={
              potencial?.valor != null ? (
                `${reaisCompactos(potencial.valor)}${potencial.parcial ? ' (parcial)' : ''}`
              ) : (
                <ValorAusente
                  motivo={
                    potencial?.frase ||
                    numerosDeDecisao?.potencialIncremental?.frase ||
                    'O potencial incremental em reais é a oportunidade de cada categoria vezes o preço de referência dela (issue 70), e ele não saiu para este município: a conta pede a demanda das categorias que têm regra aqui e as vendas em unidades.'
                  }
                  oQue="o potencial incremental"
                />
              )
            }
          />
          {/* AS MÁQUINAS POTENCIAIS SÃO A OPORTUNIDADE EM MÁQUINAS DESTE MUNICÍPIO
              (27/09/2026), a mesma da aba Oportunidades: o servidor passou a
              montá-la por município. Na ausência, a frase é a do servidor —
              do município, e na falta dela a do recorte. */}
          <ItemDeOportunidade
            icone={Tractor}
            rotulo="máquinas potenciais"
            valor={
              municipio.numerosDeDecisao?.oportunidade.valor != null ? (
                municipio.numerosDeDecisao.oportunidade.valor.toLocaleString('pt-BR', { maximumFractionDigits: 1 })
              ) : (
                <ValorAusente
                  motivo={
                    municipio.numerosDeDecisao?.oportunidade.frase ||
                    numerosDeDecisao?.oportunidade.frase ||
                    'A oportunidade em máquinas ainda não saiu para este município: a conta pede a demanda das categorias que têm regra aqui e as vendas em unidades.'
                  }
                  oQue="as máquinas potenciais"
                />
              )
            }
          />
          <ItemDeOportunidade
            icone={ChartPie}
            rotulo="cobertura atual"
            valor={
              cobre === null ? (
                <ValorAusente
                  motivo="Sem vínculo em carteira comercial com cadência declarada neste município — sem base para medir."
                  oQue="a cobertura atual"
                />
              ) : (
                porcento(cobre)
              )
            }
          />
        </div>
      </section>
    </div>
  );
}
