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

import { useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../cadastro/Indicadores';
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

export function PainelDeCredito() {
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
          rotulo: 'Ticket médio por linha',
          valor: ticket(maquinas) === null ? null : reaisCurtos(ticket(maquinas)!),
          deOnde: 'valor ÷ linhas · uma linha soma os contratos de uma combinação',
        },
      ]
    : [];

  const anos = dados?.porAno ?? [];
  const municipios = (dados?.porMunicipio ?? []).filter((m) => !soAdr || m.pertenceAAdr).slice(0, 15);
  const produtos = (dados?.porProduto ?? []).slice(0, 12);

  return (
    <>
      <div className="terr-secao-mercado">
        <h2 className="terr-secao-titulo">Crédito rural de investimento — SICOR (Banco Central)</h2>
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
          <PainelDeIndicadores indicadores={indicadores} />

          {/* A JANELA, DITA POR EXTENSO. Sem isso, "os últimos 12 meses" é uma frase — e quem confere
              na mão não sabe que meses entraram nem por que o mês mais recente ficou de fora. */}
          <div className="card cad-cartao terr-cartao">
            <div className="card-title">A janela desta comparação</div>
            <div className="cad-sub">
              <strong>{janelaTexto}</strong> contra <strong>{janelaAnteriorTexto}</strong> · {janela.mesesPorJanela} meses de cada
              lado · o SICOR tem dado até {mes(janela.ultimoMesComDado)}
              {janela.carenciaDecidida
                ? janela.mesesDeCarencia > 0
                  ? ` · os ${janela.mesesDeCarencia} ${janela.mesesDeCarencia === 1 ? 'mês mais recente ficou' : 'meses mais recentes ficaram'} de fora, porque o Banco Central ainda acrescenta contrato registrado com atraso`
                  : ' · a carência está decidida como zero: nenhum mês fica de fora'
                : ' · carência não decidida: nenhum mês foi descartado, e por isso o mês mais recente pode aparecer abaixo do que será'}
            </div>
          </div>

          {/* REGIÃO × SÃO PAULO. Sem o estado ao lado, o número da Região é solto: ele pode ser um
              terço de São Paulo ou um vigésimo, e a diferença é o tamanho do mercado que falta. */}
          {dados.regiao && dados.saoPaulo && (
            <div className="card cad-cartao terr-cartao">
              <div className="card-title">Máquinas: a Região dentro de São Paulo — {janelaTexto}</div>
              <div className="cad-tabela-wrap">
                <table className="cad-tabela terr-tabela-precos">
                  <thead>
                    <tr>
                      <th>Recorte</th>
                      <th className="terr-num">Linhas</th>
                      <th className="terr-num">Valor</th>
                      <th className="terr-num" title="Valor ÷ linhas">
                        Ticket
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    <LinhaDeJanelas
                      nome={dados.regiao.recorte}
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
            </div>
          )}

          <div className="terr-grade-precos terr-grade-custos">
            <div className="card cad-cartao terr-cartao">
              <div className="card-title terr-precos-cabecalho">
                <span>Máquinas por município — {janelaTexto}</span>
                <label className="terr-filtro-inline">
                  <input type="checkbox" checked={soAdr} onChange={(e) => setSoAdr(e.target.checked)} /> só a ADR
                </label>
              </div>
              <div className="cad-tabela-wrap">
                <table className="cad-tabela terr-tabela-precos">
                  <thead>
                    <tr>
                      <th>Município</th>
                      <th className="terr-num" title="Linhas do SICOR nos últimos 12 meses, e a variação sobre os 12 anteriores">
                        Linhas
                      </th>
                      <th className="terr-num">Valor</th>
                      <th className="terr-num" title="Valor ÷ linhas">
                        Ticket
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
            </div>

            <div className="card cad-cartao terr-cartao">
              <div className="card-title">Valor financiado em máquinas, por ano</div>
              <div className="cad-sub">
                trator, máquinas e implementos e colheitadeiras · {anos.at(-1)?.ano} vai até {MESES[mesFim - 1]}, e por isso
                aparece tracejado
              </div>
              {anos.length > 1 && (
                <MolduraDeGrafico altura={240}>
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

          <div className="card cad-cartao terr-cartao">
            <div className="card-title">Produtos financiados — {janelaTexto}, contra os 12 meses anteriores</div>
            <div className="cad-tabela-wrap">
              <table className="cad-tabela terr-tabela-precos">
                <thead>
                  <tr>
                    <th>Produto</th>
                    <th className="terr-num">Linhas</th>
                    <th className="terr-num">Valor</th>
                    <th className="terr-num">Ticket</th>
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
