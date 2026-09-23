/**
 * O crédito rural de investimento em São Paulo, pelo SICOR do Banco Central (issue 68).
 *
 * O texto-base: "a nível município vai trazer todos os financiamentos
 * contratados, o nome do produto"; "pegar os últimos 12 meses e dividir pelos
 * meses anteriores"; "valor, ticket médio". Aqui estão as duas janelas lado a
 * lado e a variação entre elas. O ÍNDICE PONDERADO (70% linhas, 30% valor) E AS
 * FAIXAS ("quente, morno, frio") SÃO DA ISSUE 73 e não aparecem antes das
 * decisões da issue 63.
 *
 * LINHA NÃO É CONTRATO. O SICOR publica a soma dos contratos de cada
 * combinação de produto, programa e fonte, por município e mês — sem número
 * de contrato. A tela chama pelo nome certo.
 *
 * OS MESES RECENTES AINDA MUDAM: o Banco Central acrescenta contrato
 * registrado com atraso. A tela diz isso ao lado do último mês.
 */

import { InfoTooltip } from '../InfoTooltip';
import { Procedencia } from '../comum/Procedencia';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import type { Indicador } from '../cadastro/Indicadores';
import { CartaoDeIndicador, GradeDeIndicadores } from '../dashboard/Dashboard';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterCreditoRural } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type { JanelasDeCredito } from '../../tipos/mercado';

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

function reaisCurtos(valor: number): string {
  if (Math.abs(valor) >= 1e9) return `R$ ${(valor / 1e9).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} bi`;
  if (Math.abs(valor) >= 1e6) return `R$ ${(valor / 1e6).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} mi`;
  return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });
}

/** A variação da última janela sobre a anterior; nula quando a anterior é zero. */
function variacao(atual: number, anterior: number): number | null {
  return anterior > 0 ? atual / anterior - 1 : null;
}

function textoDaVariacao(v: number | null): string {
  if (v === null) return '—';
  return `${v >= 0 ? '+' : ''}${(v * 100).toLocaleString('pt-BR', { maximumFractionDigits: 0 })}%`;
}

function Variacao({ v }: { v: number | null }) {
  const classe = v === null ? '' : v >= 0 ? 'terr-variacao-alta' : 'terr-variacao-baixa';
  return <span className={`terr-variacao ${classe}`}>{textoDaVariacao(v)}</span>;
}

function ticket(j: JanelasDeCredito): number | null {
  return j.linhas > 0 ? j.valor / j.linhas : null;
}

/** "jan/24" a partir de um `aaaa-mm-01` do servidor. */
function mes(data: string): string {
  const [ano, m] = data.split('-').map(Number);
  return `${MESES[m - 1]}/${String(ano).slice(2)}`;
}

/** A fatia da Região dentro de São Paulo; nula quando o estado não tem valor na janela. */
function fatia(parte: number, total: number): string {
  return total > 0 ? `${((100 * parte) / total).toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%` : '—';
}

function somar(janelas: JanelasDeCredito[]): JanelasDeCredito {
  return janelas.reduce(
    (s, j) => ({
      linhas: s.linhas + j.linhas,
      valor: s.valor + j.valor,
      linhasAnteriores: s.linhasAnteriores + j.linhasAnteriores,
      valorAnterior: s.valorAnterior + j.valorAnterior,
    }),
    { linhas: 0, valor: 0, linhasAnteriores: 0, valorAnterior: 0 },
  );
}

/** Uma linha de tabela com as duas janelas. */
function LinhaDeJanelas({ nome, sub, j }: { nome: string; sub?: string; j: JanelasDeCredito }) {
  const t = ticket(j);
  return (
    <tr>
      <td>
        {nome}
        {sub && <div className="cad-sub">{sub}</div>}
      </td>
      <td className="terr-num">
        <span className="cad-mono">{j.linhas.toLocaleString('pt-BR')}</span>
        <div className="cad-sub">
          <Variacao v={variacao(j.linhas, j.linhasAnteriores)} />
        </div>
      </td>
      <td className="terr-num">
        <span className="cad-mono">{reaisCurtos(j.valor)}</span>
        <div className="cad-sub">
          <Variacao v={variacao(j.valor, j.valorAnterior)} />
        </div>
      </td>
      <td className="terr-num cad-mono">{t === null ? '—' : reaisCurtos(t)}</td>
    </tr>
  );
}

/**
 * O CRÉDITO É O ÚNICO DOS TRÊS PAINÉIS COM DADO MUNICIPAL DE VERDADE (issue 168).
 *
 * O SICOR publica por município, então aqui o recorte escolhido muda o que se lê:
 * o município entra ao lado da Região Tracbel e de São Paulo, e sobe para o topo
 * da lista mesmo quando está fora dos quinze primeiros. Preço e custo não fazem
 * isto porque a fonte deles é estadual — e a tela não inventa granularidade que a
 * fonte não tem.
 */
export function PainelDeCredito({ municipioSelecionado = null }: { municipioSelecionado?: number | null } = {}) {
  const { contexto } = useContextoDeAcesso();
  const credito = useRecurso((sinal) => obterCreditoRural(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const [soAdr, setSoAdr] = useState(true);

  const dados = credito.dados;
  const janela = dados?.janela ?? null;

  // O ÚLTIMO MÊS DO GRÁFICO POR ANO continua sendo o que a fonte tem; a janela de comparação é outra
  // coisa e vem pronta do servidor (issue 157), com a carência já descontada.
  const mesFim = dados?.ultimoMes ? Number(dados.ultimoMes.split('-')[1]) : 0;
  const janelaTexto = janela ? `${mes(janela.inicio)} a ${mes(janela.fim)}` : '';
  const janelaAnteriorTexto = janela ? `${mes(janela.inicioAnterior)} a ${mes(janela.fimAnterior)}` : '';

  const maquinas = somar((dados?.porProduto ?? []).filter((p) => p.ehMaquina).map((p) => p.janelas));
  const indicadores: Indicador[] = janela
    ? [
        {
          rotulo: `Linhas de máquinas — ${janelaTexto}`,
          valor: `${maquinas.linhas.toLocaleString('pt-BR')} (${textoDaVariacao(variacao(maquinas.linhas, maquinas.linhasAnteriores))})`,
          deOnde: `SICOR · trator, máquinas e implementos, colheitadeiras · contra ${janelaAnteriorTexto}`,
        },
        {
          rotulo: 'Valor financiado em máquinas',
          valor: `${reaisCurtos(maquinas.valor)} (${textoDaVariacao(variacao(maquinas.valor, maquinas.valorAnterior))})`,
          deOnde: 'SICOR · mesmos produtos e janela',
        },
        {
          rotulo: 'Valor médio por linha',
          valor: ticket(maquinas) === null ? null : reaisCurtos(ticket(maquinas)!),
          deOnde: 'valor ÷ linhas · uma linha soma os contratos de uma combinação',
        },
      ]
    : [];

  const anos = dados?.porAno ?? [];
  const escolhido = municipioSelecionado === null ? null : (dados?.porMunicipio.find((m) => m.codigoIbge === municipioSelecionado) ?? null);

  // O MUNICÍPIO ESCOLHIDO SOBE PARA O TOPO, mesmo fora dos quinze primeiros e
  // mesmo com o filtro "só a ADR" ligado: quem escolheu um município quer vê-lo,
  // e uma lista que o esconde faz o clique no mapa parecer que não fez nada.
  const primeiros = (dados?.porMunicipio ?? [])
    .filter((m) => (!soAdr || m.pertenceAAdr) && m.codigoIbge !== escolhido?.codigoIbge)
    .slice(0, escolhido ? 14 : 15);
  const municipios = escolhido ? [escolhido, ...primeiros] : primeiros;
  const produtos = (dados?.porProduto ?? []).slice(0, 12);

  return (
    <>
      <div className="terr-secao-mercado">
        <h2 className="terr-secao-titulo">Crédito rural de investimento — SICOR (Banco Central)</h2>
        <Procedencia procedencia={dados?.procedencia} oQue="o crédito rural" />
        <p className="terr-secao-subtitulo">
          Tudo o que foi financiado em São Paulo desde 2013, por município, produto e mês. <strong>Uma linha não é um
          contrato:</strong> o SICOR publica a soma dos contratos de cada combinação de produto, programa e fonte. Ele não
          identifica cliente nem revenda. Os meses mais recentes ainda recebem registros atrasados.
        </p>
      </div>

      {credito.carregando && <BlocoCarregando oQue="o crédito rural" />}
      {credito.erro && <BlocoErro erro={credito.erro} aoTentarDeNovo={credito.recarregar} />}

      {dados && !dados.ultimoMes && (
        <div className="card cad-cartao terr-cartao">
          <BlocoVazio
            titulo="O crédito rural ainda não foi carregado neste banco"
            texto={
              <>
                A rota respondeu, mas não há nenhuma linha do SICOR gravada: a carga (<code>--somente-credito</code>) ainda
                não rodou aqui. Não é zero nem falta de permissão.
              </>
            }
          />
        </div>
      )}

      {dados?.ultimoMes && janela && (
        <>
          {/* ---------- CAMADA 1: a resposta (fase T4.7) ----------
              Os três números na mesma grade do topo da página. Eles vinham no
              `PainelDeIndicadores` do cadastro, com outro desenho — e o painel de
              crédito abria parecendo uma tela de outra família. */}
          <GradeDeIndicadores data-colunas="3">
            {indicadores.map((i, n) => (
              <CartaoDeIndicador
                key={i.rotulo}
                rotulo={i.rotulo}
                destaque={n === 0}
                valor={i.valor}
                contexto={i.deOnde}
                motivoSemDado={i.semDado}
              />
            ))}
          </GradeDeIndicadores>

          {/* A JANELA, DITA POR EXTENSO — e RECOLHIDA (fase T4.7). Sem ela, "os últimos 12 meses" é
              uma frase e quem confere na mão não sabe que meses entraram; com ela aberta, o painel
              começava por uma explicação de metodologia em vez de por um número. */}
          <details className="cad-recolhivel" data-bloco="credito-janela">
            <summary>A janela desta comparação — {janelaTexto}</summary>
            <div className="cad-sub">
              <strong>{janelaTexto}</strong> contra <strong>{janelaAnteriorTexto}</strong> · {janela.mesesPorJanela} meses de cada
              lado · o SICOR tem dado até {mes(janela.ultimoMesComDado)}
              {janela.carenciaDecidida
                ? janela.mesesDeCarencia > 0
                  ? ` · os ${janela.mesesDeCarencia} ${janela.mesesDeCarencia === 1 ? 'mês mais recente ficou' : 'meses mais recentes ficaram'} de fora, porque o Banco Central ainda acrescenta contrato registrado com atraso`
                  : ' · a carência está decidida como zero: nenhum mês fica de fora'
                : ' · carência não decidida: nenhum mês foi descartado, e por isso o mês mais recente pode aparecer abaixo do que será'}
            </div>
          </details>

          {/* REGIÃO × SÃO PAULO. Sem o estado ao lado, o número da Região é solto: ele pode ser um
              terço de São Paulo ou um vigésimo, e a diferença é o tamanho do mercado que falta.
              Recolhido na T4.7: é comparação de recorte, e vem depois da leitura. */}
          {dados.regiao && dados.saoPaulo && (
            <details className="cad-recolhivel" data-bloco="credito-regiao-sp">
              <summary>Máquinas: a Região dentro de São Paulo — {janelaTexto}</summary>
              <div className="cad-tabela-wrap">
                <table className="cad-tabela terr-tabela-precos">
                  <thead>
                    <tr>
                      <th>Recorte</th>
                      <th className="terr-num">Linhas</th>
                      <th className="terr-num">Valor</th>
                      <th className="terr-num">
                        Valor médio{' '}
                        <InfoTooltip
                          rotulo="O que é o valor médio por linha"
                          texto="Valor contratado dividido pelo número de LINHAS do SICOR. Não é ticket médio: a linha do SICOR não é um contrato — ela já é a soma dos contratos daquela combinação de município e produto, e não traz quantidade."
                        />
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {/* MUNICÍPIO · REGIÃO TRACBEL · SÃO PAULO, lado a lado (issue 168).
                        A hierarquia da issue 163 lida de cima para baixo: o crédito é a
                        única das três fontes deste bloco que desce ao município. */}
                    {escolhido && (
                      <LinhaDeJanelas nome={escolhido.nome} sub="município escolhido" j={escolhido.janelas} />
                    )}
                    <LinhaDeJanelas
                      nome="Região Tracbel"
                      sub={`${dados.regiao.municipios.toLocaleString('pt-BR')} municípios com linha na janela`}
                      j={dados.regiao.janelas}
                    />
                    <LinhaDeJanelas
                      nome={dados.saoPaulo.recorte}
                      sub={`${dados.saoPaulo.municipios.toLocaleString('pt-BR')} municípios com linha na janela`}
                      j={dados.saoPaulo.janelas}
                    />
                    <tr>
                      <td>
                        <strong>A Região é esta fatia de São Paulo</strong>
                      </td>
                      <td className="terr-num cad-mono">{fatia(dados.regiao.janelas.linhas, dados.saoPaulo.janelas.linhas)}</td>
                      <td className="terr-num cad-mono">{fatia(dados.regiao.janelas.valor, dados.saoPaulo.janelas.valor)}</td>
                      <td className="terr-num">—</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </details>
          )}

          {/* ---------- CAMADA 2: o visual (fase T4.7) ----------
              À esquerda o RANKING dos municípios, que responde "onde está o
              crédito"; à direita a EVOLUÇÃO por ano, que responde "está subindo
              ou caindo". Eram uma tabela de quinze linhas e um gráfico colados
              lado a lado sem composição — duas leituras disputando o mesmo
              espaço. A tabela inteira continua abaixo, recolhida. */}
          <div className="dash-duas-colunas">
            <div className="dash-ranking" aria-label="Valor financiado em máquinas, por município">
              <div className="dash-painel-cabecalho">
                <h3 className="dash-painel-titulo">Onde está o crédito</h3>
                <label className="terr-filtro-inline">
                  <input type="checkbox" checked={soAdr} onChange={(e) => setSoAdr(e.target.checked)} /> só a ADR
                </label>
              </div>

              <ul className="dash-ranking-lista">
                {municipios.slice(0, 10).map((m) => {
                  const maior = Math.max(1, ...municipios.map((x) => x.janelas.valor));
                  return (
                    <li key={m.codigoIbge} className="dash-ranking-item" data-municipio={m.codigoIbge}>
                      <span className="dash-ranking-nome">{m.nome}</span>
                      <span className="dash-ranking-barra">
                        <span
                          className="dash-ranking-preenchimento"
                          style={{ width: `${Math.max(2, (m.janelas.valor / maior) * 100)}%` }}
                          aria-hidden="true"
                        />
                      </span>
                      <span className="dash-ranking-valor">
                        {reaisCurtos(m.janelas.valor)}
                        <InfoTooltip
                          rotulo={`O crédito de ${m.nome}`}
                          texto={`${m.janelas.linhas.toLocaleString('pt-BR')} linhas do SICOR na janela, contra ${m.janelas.linhasAnteriores.toLocaleString('pt-BR')} nos 12 meses anteriores. Uma linha NÃO é um contrato: ela já é a soma dos contratos daquela combinação de município e produto.`}
                        />
                      </span>
                    </li>
                  );
                })}
              </ul>
            </div>

            <div className="card cad-cartao terr-cartao">
              <div className="card-title">Valor financiado em máquinas, por ano</div>
              <div className="cad-sub">
                trator, máquinas e implementos e colheitadeiras · {anos.at(-1)?.ano} vai até {MESES[mesFim - 1]}, e por isso
                aparece tracejado
              </div>
              {anos.length > 1 && (
                <MolduraDeGrafico altura={300}>
                  {(l, a) => (
                    <GraficoLinhaMensal
                      rotulos={anos.map((ano) => String(ano.ano))}
                      valores={anos.map((ano) => ano.valorDeMaquinas)}
                      largura={l}
                      altura={a}
                      formatar={reaisCurtos}
                      ultimoParcial={(anos.at(-1)?.ultimoMes ?? 12) < 12}
                    />
                  )}
                </MolduraDeGrafico>
              )}
              <SeloProcedencia procedencia={credito.procedencia} />
            </div>
          </div>

          {/* ---------- CAMADA 3: a evidência (fase T4.7) ----------
              A tabela inteira dos municípios, com as três colunas e a variação.
              Ela era a PRIMEIRA coisa da segunda metade do painel, com quinze
              linhas ocupando meia tela antes de qualquer leitura. Nenhuma linha
              foi removida: ela é consulta e auditoria, e é onde a consulta e a
              auditoria acontecem. */}
          <details className="cad-recolhivel" data-bloco="credito-municipios">
            <summary>Máquinas por município — a tabela inteira, com a variação sobre os 12 meses anteriores</summary>
            <div className="cad-tabela-wrap">
              <table className="cad-tabela terr-tabela-precos">
                <thead>
                  <tr>
                    <th>Município</th>
                    <th className="terr-num">
                      Linhas{' '}
                      <InfoTooltip
                        rotulo="O que a coluna Linhas conta"
                        texto="Linhas do SICOR nos últimos 12 meses, e a variação sobre os 12 anteriores. Cada linha já é a soma dos contratos daquela combinação — não é um contrato, e não há quantidade."
                      />
                    </th>
                    <th className="terr-num">Valor</th>
                    <th className="terr-num">
                      Valor médio{' '}
                      <InfoTooltip
                        rotulo="O que é o valor médio por linha"
                        texto="Valor contratado dividido pelo número de LINHAS do SICOR. Não é ticket médio: a linha do SICOR não é um contrato — ela já é a soma dos contratos daquela combinação de município e produto, e não traz quantidade."
                      />
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {municipios.map((m) => (
                    <LinhaDeJanelas key={m.codigoIbge} nome={m.nome} sub={m.pertenceAAdr ? 'ADR' : undefined} j={m.janelas} />
                  ))}
                </tbody>
              </table>
            </div>
          </details>

          <div className="card cad-cartao terr-cartao">
            <div className="card-title">Produtos financiados — {janelaTexto}, contra os 12 meses anteriores</div>
            <div className="cad-tabela-wrap">
              <table className="cad-tabela terr-tabela-precos">
                <thead>
                  <tr>
                    <th>Produto</th>
                    <th className="terr-num">Linhas</th>
                    <th className="terr-num">Valor</th>
                    <th className="terr-num">Valor médio</th>
                  </tr>
                </thead>
                <tbody>
                  {produtos.map((p) => (
                    <LinhaDeJanelas key={p.codigo} nome={p.nome} sub={p.ehMaquina ? 'máquina' : undefined} j={p.janelas} />
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </>
  );
}
