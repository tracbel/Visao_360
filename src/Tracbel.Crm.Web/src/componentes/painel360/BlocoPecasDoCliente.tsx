/**
 * AS PEÇAS DO CLIENTE no 360 (02/10/2026) — o faturamento de peças por mês e os orçamentos em aberto, pela rota
 * `/api/v1/clientes/{chave}/pecas`, da tabela que o modo de peças da rotina 15 `POS_VENDA_PROTHEUS` apura a partir das
 * views do BI no banco do Protheus.
 *
 * O bloco de faturamento diz QUANTO da nota é peça; este diz COMO: balcão × oficina, o grupo comercial (peças, pneus,
 * lubrificantes…, na régua do painel "Faturamento Peças" do BI), o vendedor principal e o que está orçado esperando o
 * cliente. Sem custo nem margem.
 */

import { obterPecasDoCliente } from '../../dados/api/relacionamento';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarData, formatarDinheiro, formatarNumero } from '../../telas/cadastro/formato';
import type { FatiaDasPecas, PecasDoCliente } from '../../tipos/pecas';
import { MetricasSemDado } from '../cadastro/SemDado';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { ValorAusente } from '../comum/ValorAusente';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { BlocoPainel, type EstadoBloco } from './BlocoPainel';
import { Dado } from './DadoDoPainel';
import { nomeDoGrupo, nomeDoSetor } from './textosDasPecas';

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/** `2026-09-01` vira `set/26`. */
function mesCurto(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${MESES[Number(mes) - 1]}/${ano.slice(2)}`;
}

/** `2026-09-01` vira `09/2026`. */
function mesAno(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${mes}/${ano}`;
}

/** `2026-09-24T16:27:48` (UTC) vira `24/09 13:27`, no fuso de quem lê. */
function diaEHora(instante: string): string {
  const utc = /Z|[+-]\d\d:\d\d$/.test(instante) ? instante : `${instante}Z`;
  return new Date(utc).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
}

/** O eixo do gráfico, curto: `R$ 50 mil`, `R$ 1,2 mi`. */
function reaisCurtos(valor: number): string {
  if (Math.abs(valor) >= 1_000_000) return `R$ ${(valor / 1_000_000).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} mi`;
  if (Math.abs(valor) >= 1_000) return `R$ ${(valor / 1_000).toLocaleString('pt-BR', { maximumFractionDigits: 0 })} mil`;
  return formatarDinheiro(valor);
}

function estadoDe(carregando: boolean, erro: Error | null, dados: PecasDoCliente | null): EstadoBloco {
  if (carregando) return 'carregando';
  if (erro) return 'erro';
  return dados ? 'ok' : 'vazio';
}

/** "Balcão R$ 12 mil (63%) · Oficina R$ 7 mil (37%)". */
function fatiasEmTexto(fatias: FatiaDasPecas[], total: number, nome: (n: string) => string, quantas = 4): string {
  const mostradas = fatias.slice(0, quantas).map((f) => {
    const parte = total > 0 ? ` (${formatarNumero((f.valor / total) * 100)}%)` : '';
    return `${nome(f.nome)} ${formatarDinheiro(f.valor)}${parte}`;
  });
  const resto = fatias.length - quantas;
  return mostradas.join(' · ') + (resto > 0 ? ` · mais ${resto}` : '');
}

export function BlocoPecasDoCliente({ chave }: { chave: string }) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => obterPecasDoCliente(contexto, chave, sinal), [contexto.empresa, chave]);
  const dados = leitura.dados;

  const subtitulo = dados?.carregadoEm
    ? `faturamento de peças do Protheus, na régua do painel do BI · até a carga de ${diaEHora(dados.carregadoEm)}`
    : 'faturamento de peças do Protheus, na régua do painel do BI';

  return (
    <BlocoPainel
      id="pecas"
      titulo="Peças"
      subtitulo={subtitulo}
      fonte={<SeloProcedencia procedencia={leitura.procedencia} />}
      estado={estadoDe(leitura.carregando, leitura.erro, dados)}
      mensagemErro={leitura.erro?.message}
    >
      {dados && <Conteudo dados={dados} />}
    </BlocoPainel>
  );
}

function Conteudo({ dados }: { dados: PecasDoCliente }) {
  // A VALIDADE SE COMPARA COM O DIA DE HOJE de quem lê — o orçamento vencido vai para a faixa vermelha.
  const hoje = new Date().toISOString().slice(0, 10);
  const semCarga = dados.metricasSemDado.find((m) => m.metrica === 'pecas')?.motivo;
  const semCompra =
    dados.metricasSemDado.find((m) => m.metrica === 'pecasDoCliente')?.motivo ?? 'Nenhuma compra de peça nas filiais ao seu alcance.';

  return (
    <div className="ficha-pecas">
      <dl className="p360-dados">
        <Dado
          rotulo="Peças em 12 meses"
          valor={semCarga ? <ValorAusente motivo={semCarga} oQue="o faturamento de peças" /> : formatarDinheiro(dados.dozeMeses)}
          detalhe={`${mesAno(dados.de)} a ${mesAno(dados.ate)} · mês corrente parcial`}
        />
        <Dado
          rotulo="Balcão e oficina"
          valor={
            dados.porSetor.length === 0 ? (
              <ValorAusente motivo={semCarga ?? semCompra} oQue="a divisão por setor" />
            ) : (
              nomeDoSetor(dados.porSetor[0].nome)
            )
          }
          detalhe={dados.porSetor.length === 0 ? undefined : fatiasEmTexto(dados.porSetor, dados.dozeMeses, nomeDoSetor, 3)}
        />
        <Dado
          rotulo="Última compra"
          valor={dados.ultimaCompraEm ? mesAno(dados.ultimaCompraEm) : <ValorAusente motivo={semCarga ?? semCompra} oQue="a última compra" />}
          detalhe={dados.vendedorPrincipal ? `vendedor principal: ${dados.vendedorPrincipal}` : undefined}
        />
        <Dado
          rotulo="Orçamentos em aberto"
          valor={formatarNumero(dados.orcamentosEmAberto)}
          detalhe={
            dados.orcamentosEmAberto === 0
              ? 'nada orçado esperando o cliente'
              : `${formatarDinheiro(dados.valorEmOrcamentosAbertos)}${dados.orcamentosVencidos > 0 ? ` · ${formatarNumero(dados.orcamentosVencidos)} vencido(s)` : ''}`
          }
        />
      </dl>

      {dados.serie.some((m) => m.valor !== 0) && (
        <MolduraDeGrafico altura={120}>
          {(largura, altura) => (
            <GraficoLinhaMensal
              rotulos={dados.serie.map((m) => mesCurto(m.competencia))}
              valores={dados.serie.map((m) => m.valor)}
              largura={largura}
              altura={altura}
              formatar={reaisCurtos}
              ultimoParcial
            />
          )}
        </MolduraDeGrafico>
      )}

      {dados.porGrupo.length > 0 && (
        <p className="p360-item-obs">
          <strong>Por grupo:</strong> {fatiasEmTexto(dados.porGrupo, dados.dozeMeses, nomeDoGrupo)}
          {dados.devolucoesNosDozeMeses !== 0 && ` · devoluções ${formatarDinheiro(dados.devolucoesNosDozeMeses)} já descontadas`}
        </p>
      )}

      {dados.orcamentos.length > 0 && (
        <>
          <h4 className="ficha-subtitulo">Orçamentos esperando o cliente</h4>
          <ul className="p360-lista">
            {dados.orcamentos.map((o) => {
              const vencido = o.validoAte !== null && o.validoAte < hoje;
              return (
                <li key={o.chave} className={vencido ? 'p360-item p360-item-critico' : 'p360-item'}>
                  <div className="p360-item-topo">
                    <span className="p360-item-titulo">
                      Orçamento <span className="cad-mono">{o.numero}</span> · {formatarDinheiro(o.valorTotal)}
                    </span>
                    <span className="p360-item-data cad-mono">{formatarData(o.orcadoEm)}</span>
                  </div>
                  <div className="p360-item-meta">
                    {o.situacao.toLowerCase()}
                    {o.validoAte && ` · ${vencido ? 'venceu em' : 'válido até'} ${formatarData(o.validoAte)}`} · {o.filialNome}
                    {o.vendedorNome ? ` · ${o.vendedorNome}` : ''}
                  </div>
                  <div className="p360-item-obs">
                    {formatarNumero(o.itens)} {o.itens === 1 ? 'item' : 'itens'}
                    {o.reserva ? ` · ${o.reserva.toLowerCase()}` : ''}
                  </div>
                </li>
              );
            })}
          </ul>
        </>
      )}

      <p className="p360-item-obs">
        Valor líquido das notas de peça do Protheus, com as devoluções descontadas — sem custo nem margem. A nota entra aqui
        quando o CPF/CNPJ dela é o deste cliente; só aparecem as filiais ao seu alcance.
      </p>

      <MetricasSemDado metricas={dados.metricasSemDado} titulo="O que este bloco não diz" />
    </div>
  );
}
