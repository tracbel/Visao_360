/**
 * O FATURAMENTO DO CLIENTE no 360 — o bloco que substitui a frase falsa "o faturamento parou em 11/04/2025".
 *
 * 27/09/2026. A frase era da cópia que o Vórtice recebia; o faturamento do Protheus estava no banco do CRM, de
 * 09/2023 a 09/2026 (53.977 meses de 7.505 clientes, em 16 filiais), e nenhuma tela o lia por cliente. Agora lê, pela
 * rota `/api/v1/clientes/{chave}/faturamento`.
 *
 * TRÊS COISAS QUE O BLOCO NÃO PODE ESCONDER:
 *
 * - **a data da carga** — "até a carga de 24/09 13:28", como o cartão de faturamento da Visão 360. A rotina do
 *   faturamento está desligada; sem a data, o número pareceria de hoje;
 * - **a fronteira** — a filial que EMITIU a nota. Quem não alcança a filial não vê a nota dela;
 * - **reais não somam com unidades** (D-P08) — os reais vêm do Protheus; as máquinas em unidades vêm do ART, na frota.
 */

import { obterFaturamentoDoCliente } from '../../dados/api/relacionamento';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarDinheiro } from '../../telas/cadastro/formato';
import type { FaturamentoDoCliente } from '../../tipos/relacionamento';
import { MetricasSemDado } from '../cadastro/SemDado';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { ValorAusente } from '../comum/ValorAusente';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { BlocoPainel, type EstadoBloco } from './BlocoPainel';
import { Dado } from './DadoDoPainel';
import '../../estilos/ficha-do-cliente.css';

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

/** `2026-09-24T16:27:48` (UTC) vira `24/09 13:27`, no fuso de quem lê — a régua do cartão da Visão 360. */
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

function estadoDe(carregando: boolean, erro: Error | null, dados: FaturamentoDoCliente | null): EstadoBloco {
  if (carregando) return 'carregando';
  if (erro) return 'erro';
  return dados ? 'ok' : 'vazio';
}

export function BlocoFaturamentoDoCliente({ chave }: { chave: string }) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => obterFaturamentoDoCliente(contexto, chave, sinal), [contexto.empresa, chave]);
  const dados = leitura.dados;

  const subtitulo = dados?.carregadoEm
    ? `notas de saída do Protheus (SD2), pela filial que emitiu a nota · até a carga de ${diaEHora(dados.carregadoEm)}`
    : 'notas de saída do Protheus (SD2), pela filial que emitiu a nota';

  return (
    <BlocoPainel
      id="faturamento"
      titulo="Faturamento"
      subtitulo={subtitulo}
      fonte={<SeloProcedencia procedencia={leitura.procedencia} />}
      estado={estadoDe(leitura.carregando, leitura.erro, dados)}
      mensagemErro={leitura.erro?.message}
    >
      {dados && <Conteudo dados={dados} />}
    </BlocoPainel>
  );
}

function Conteudo({ dados }: { dados: FaturamentoDoCliente }) {
  const doze = dados.dozeMeses;
  const janela = dados.janelaCarregada;
  const motivoSemCarga =
    dados.metricasSemDado.find((m) => m.metrica === 'faturamento')?.motivo ??
    'Não há faturamento carregado nas filiais ao seu alcance.';

  if (!doze || !dados.competenciaMaisRecente) {
    return (
      <dl className="p360-dados">
        <Dado rotulo="Últimos 12 meses" valor={<ValorAusente motivo={motivoSemCarga} oQue="o faturamento" />} />
      </dl>
    );
  }

  const resto = doze.peca + doze.servico + doze.outros;
  const motivoSemNota =
    dados.metricasSemDado.find((m) => m.metrica === 'faturamentoDoCliente')?.motivo ??
    'Nenhuma nota deste cliente nas filiais ao seu alcance.';

  return (
    <div className="ficha-faturamento">
      <dl className="p360-dados">
        <Dado
          rotulo="Últimos 12 meses"
          valor={formatarDinheiro(doze.valorLiquido)}
          detalhe={`${mesAno(doze.de)} a ${mesAno(doze.ate)}${dados.ultimoMesEstaIncompleto ? ' · último mês parcial' : ''}`}
        />
        <Dado rotulo="Máquina, em reais" valor={formatarDinheiro(doze.maquina)} detalhe="grupo VEIC da nota" />
        <Dado rotulo="Peça, serviço e outros" valor={formatarDinheiro(resto)} detalhe={`${doze.notas.toLocaleString('pt-BR')} notas no período`} />
        <Dado
          rotulo="Nota mais recente"
          valor={dados.ultimaCompraEm ? mesAno(dados.ultimaCompraEm) : <ValorAusente motivo={motivoSemNota} oQue="a nota mais recente" />}
        />
      </dl>

      <MolduraDeGrafico altura={140}>
        {(largura, altura) => (
          <GraficoLinhaMensal
            rotulos={dados.serie.map((m) => mesCurto(m.competencia))}
            valores={dados.serie.map((m) => m.valorLiquido)}
            largura={largura}
            altura={altura}
            formatar={reaisCurtos}
            ultimoParcial={dados.ultimoMesEstaIncompleto}
          />
        )}
      </MolduraDeGrafico>

      {dados.porFilial.length > 0 && (
        <>
          <h4 className="ficha-subtitulo">Por filial que emitiu a nota</h4>
          <ul className="ficha-filiais">
            {dados.porFilial.map((f) => (
              <li key={f.filialCodigo} className="ficha-filial">
                <span className="ficha-filial-nome">
                  <span className="cad-mono">{f.filialCodigo}</span> {f.filialNome}
                </span>
                <span className="ficha-filial-valor">{formatarDinheiro(f.dozeMeses)}</span>
                <span className="ficha-filial-meta">
                  em 12 meses{f.maquinaNosDozeMeses > 0 ? ` (máquina ${formatarDinheiro(f.maquinaNosDozeMeses)})` : ''} ·{' '}
                  {formatarDinheiro(f.naJanela)} na janela · última nota {mesAno(f.ultimaNotaEm)}
                </span>
              </li>
            ))}
          </ul>
        </>
      )}

      {janela && janela.valorLiquido > 0 && (
        <p className="p360-item-obs">
          Na janela que a carga mantém ({mesAno(janela.de)} a {mesAno(janela.ate)}): {formatarDinheiro(janela.valorLiquido)} — máquina{' '}
          {formatarDinheiro(janela.maquina)}, peça {formatarDinheiro(janela.peca)}, serviço {formatarDinheiro(janela.servico)}
          {janela.outros > 0 ? `, outros ${formatarDinheiro(janela.outros)}` : ''}.
        </p>
      )}

      <p className="p360-item-obs">
        Reais das notas de saída do Protheus. As máquinas em unidades vêm do ART, na frota, e não se somam a estes valores
        (D-P08). Só entram as notas das filiais ao seu alcance.
      </p>

      <MetricasSemDado metricas={dados.metricasSemDado} titulo="O que este bloco não diz" />
    </div>
  );
}
