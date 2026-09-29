/**
 * Forecast da Gerência (28/09/2026) — a previsão de cada gestor, ao lado do PO e do realizado do time dele, num mês.
 *
 * É a tela "Forecast Gerência" da API Gestão de Negócios com o realizado que o CRM tem:
 *
 * - **o Forecast e o Best Guess vêm da GN** — o gestor os informa lá (o Forecast uma vez por mês; o Best Guess, toda
 *   segunda), e a rotina das metas os traz. O CRM não edita;
 * - **o PO é a meta do time do gestor**, e o **realizado**, as máquinas vendidas pelo time (o ART) — o time é o de-para de
 *   consultores da GN;
 * - **previsão não informada fica vazia**, com o motivo, e não vira zero.
 *
 * É da gerência: quem só vê a própria meta recebe a recusa da API, com o motivo.
 *
 * 29/09/2026 — NO PADRÃO DOS INDICADORES GEOGRÁFICOS (#293, bloco 3): página na coluna inteira, o mês na barra de filtros,
 * os cinco números em cartões de decisão e a tabela num painel da seção. Nenhum número nem texto mudou: o "Como ler" que
 * ficava no pé do cartão foi para a dica do painel.
 */

import { CalendarDays, Crosshair, RefreshCw, Target, TrendingUp, Truck, Users } from 'lucide-react';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterForecastDaGerencia } from '../dados/api/forecast';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { ForecastDoGestor, LinhaDoForecast, RelatorioDoForecast } from '../tipos/forecast';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';
import '../estilos/forecast.css';

const NAO_INFORMADO = 'O gestor não informou este número na Gestão de Negócios: fica vazio, e não zero.';

const n = (v: number) => v.toLocaleString('pt-BR');
const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const mesPorExtenso = (aaaammdd: string) => `${MESES[Number(aaaammdd.slice(5, 7)) - 1]}/${aaaammdd.slice(0, 4)}`;

const COMO_LER =
  'O Forecast é o compromisso do gestor para o mês; o Best Guess, a aposta dele na semana. Os dois vêm da Gestão de ' +
  'Negócios, em máquinas, e o CRM não os edita. O PO é a meta dos consultores do time, e o realizado, as máquinas que eles ' +
  'venderam e o ART deu como entregues no mês — a régua da Gestão de Negócios.';

/** A soma de várias linhas: o forecast e o best guess só somam o que foi informado — nenhum informado fica nulo. */
export function somar(linhas: LinhaDoForecast[]): Omit<LinhaDoForecast, 'codigo' | 'nome'> {
  const somarNulos = (valores: (number | null)[]) =>
    valores.some((v) => v !== null) ? valores.reduce<number>((s, v) => s + (v ?? 0), 0) : null;
  return {
    meta: linhas.reduce((s, l) => s + l.meta, 0),
    forecast: somarNulos(linhas.map((l) => l.forecast)),
    bestGuess: somarNulos(linhas.map((l) => l.bestGuess)),
    realizado: linhas.reduce((s, l) => s + l.realizado, 0),
  };
}

/** Quanto do PO: nulo sem PO — não há base para a razão. */
export function doPo(valor: number | null, po: number): string | null {
  if (valor === null || po <= 0) return null;
  return `${Math.round((100 * valor) / po).toLocaleString('pt-BR')}%`;
}

/** As linhas da tela como o CSV as leva: uma por gestor e linha, e o total no fim. */
export function linhasDoCsv(relatorio: RelatorioDoForecast): unknown[][] {
  const vazio = (v: number | null) => (v === null ? '' : v);
  const linha = (gestor: string, l: LinhaDoForecast) => [gestor, l.nome, l.meta, vazio(l.forecast), vazio(l.bestGuess), l.realizado];
  return [
    ...relatorio.gestores.flatMap((g) => g.linhas.map((l) => linha(g.gestor, l))),
    ...relatorio.total.map((l) => linha('TOTAL', l)),
  ];
}

const CABECALHO_DO_CSV = ['Gestor', 'Linha', 'PO (meta)', 'Forecast', 'Best Guess', 'Realizado'];

export function ForecastGerencia() {
  const { contexto } = useContextoDeAcesso();
  const [competencia, setCompetencia] = useState<string | undefined>(undefined);

  const forecast = useRecurso(
    (sinal) => obterForecastDaGerencia(contexto, competencia, sinal),
    [contexto.empresa, contexto.usuario, competencia],
  );
  const dados = forecast.dados;
  const total = dados ? somar(dados.total) : null;

  /** A linha de baixo do cartão: sem o número, o que falta (como a faixa antiga escrevia); com ele, o quanto do PO. */
  const doPoNoMes = (valor: number | null, resto: string, semDado: string) =>
    !total ? null : valor === null ? semDado : `${doPo(valor, total.meta) ?? 'sem PO no mês'} do PO · ${resto}`;

  const meses = dados ? [...new Set([...dados.mesesDisponiveis, dados.competencia])].sort().reverse() : [];

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Forecast da Gerência</h1>
          <p className="page-subtitle">
            A previsão de cada gestor — o Forecast do mês e o Best Guess da semana — ao lado do PO e do realizado do time
            dele.
          </p>
        </div>
        <p className="dash-atualizado">
          {forecast.procedencia ? <DadosAtualizadosEm procedencia={forecast.procedencia} /> : 'Lendo o forecast…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={forecast.recarregar}
            disabled={forecast.carregando}
            data-carregando={forecast.carregando ? 'true' : 'false'}
            aria-label="Reler o forecast"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="mes">
            <span className="dash-filtro-icone" aria-hidden="true">
              <CalendarDays size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Mês</span>
              <select
                value={dados?.competencia ?? ''}
                onChange={(e) => setCompetencia(e.target.value.slice(0, 7))}
                disabled={meses.length === 0}
              >
                {meses.length === 0 && <option value="">—</option>}
                {meses.map((m) => (
                  <option key={m} value={m}>
                    {mesPorExtenso(m)}
                    {dados!.mesesDisponiveis.includes(m) ? '' : ' (sem forecast)'}
                  </option>
                ))}
              </select>
            </span>
          </label>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={forecast.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="PO (meta)"
          icone={Target}
          tom="demanda"
          valor={total ? n(total.meta) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={undefined}
          variacao={total ? 'a meta de máquinas do mês — API Gestão de Negócios' : null}
          sobre="A meta de máquinas do mês dos consultores do time de cada gestor, somada — lida da API Gestão de Negócios."
        />
        <CartaoDeDecisao
          rotulo="Forecast"
          icone={Crosshair}
          tom="mercado"
          valor={total?.forecast != null ? n(total.forecast) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={total ? NAO_INFORMADO : undefined}
          variacao={doPoNoMes(total?.forecast ?? null, 'revisto uma vez por mês', 'nenhum gestor informou')}
          sobre="O compromisso do gestor para o mês, informado na Gestão de Negócios. O CRM não o edita, e o que ninguém informou fica vazio, e não zero."
        />
        <CartaoDeDecisao
          rotulo="Best Guess"
          icone={TrendingUp}
          tom="oportunidade"
          valor={total?.bestGuess != null ? n(total.bestGuess) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={total ? NAO_INFORMADO : undefined}
          variacao={doPoNoMes(total?.bestGuess ?? null, 'reavaliado toda segunda', 'nenhum gestor informou')}
          sobre="A aposta do gestor na semana, informada na Gestão de Negócios toda segunda. O CRM não a edita, e o que ninguém informou fica vazio, e não zero."
        />
        <CartaoDeDecisao
          rotulo="Realizado"
          icone={Truck}
          tom="captura"
          valor={total ? n(total.realizado) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={undefined}
          variacao={doPoNoMes(total?.realizado ?? null, 'máquinas vendidas (ART)', '')}
          sobre="As máquinas que os consultores do time venderam e o ART deu como entregues no mês — a régua da Gestão de Negócios."
        />
        <CartaoDeDecisao
          rotulo="Gestores"
          icone={Users}
          tom="neutro"
          valor={dados ? n(dados.gestores.length) : null}
          carregando={forecast.carregando}
          motivoSemDado={undefined}
          variacao={dados ? 'pelo de-para de consultores da GN' : null}
          sobre="Os gestores com forecast, PO ou venda no mês. O time de cada um é o de-para de consultores da Gestão de Negócios."
        />
      </div>

      <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que este forecast não afirma" />

      <section className="dash-secao" data-bloco="secao-gestores">
        <TituloDaSecao
          titulo="Os gestores"
          subtitulo={
            dados
              ? dados.alcance === 'Organizacao'
                ? 'PO e realizado de todas as filiais'
                : 'PO e realizado da filial escolhida; o forecast é o do gestor inteiro'
              : 'o forecast, o best guess, o PO e o realizado de cada gestor'
          }
        />

        <PainelDoMomento
          titulo={`Previsão por gestor${dados ? ` · ${dados.texto}` : ''}`}
          data-bloco="forecast-por-gestor"
          subtitulo={dados?.lidoEm ? `lido da Gestão de Negócios em ${dataEHora(dados.lidoEm)}` : undefined}
          dica={COMO_LER}
          direita={
            dados && (
              <button
                type="button"
                className="btn btn-secondary btn-sm"
                onClick={() => baixarCsv(`forecast-da-gerencia-${dados.competencia.slice(0, 7)}-${carimboDeData()}`, CABECALHO_DO_CSV, linhasDoCsv(dados))}
                disabled={dados.total.length === 0}
              >
                Exportar CSV
              </button>
            )
          }
        >
          {forecast.carregando && <BlocoCarregando oQue="o forecast" />}
          {forecast.erro && <BlocoErro erro={forecast.erro} aoTentarDeNovo={forecast.recarregar} />}

          {dados && dados.total.length === 0 && (
            <BlocoVazio
              titulo={`Nada no mês de ${dados.texto}`}
              texto="Nenhum gestor informou forecast, e o time não tem PO nem venda neste mês."
            />
          )}

          {dados && dados.total.length > 0 && (
            <div className="mom-tabela-rolagem">
              <table className="mom-tabela fc-tabela">
                <caption className="cad-so-leitor">Forecast, best guess, PO e realizado de cada gestor, por linha</caption>
                <thead>
                  <tr>
                    <th scope="col">Gestor · linha</th>
                    <th scope="col" className="mom-num">PO</th>
                    <th scope="col" className="mom-num">Forecast</th>
                    <th scope="col" className="mom-num">Best Guess</th>
                    <th scope="col" className="mom-num">Realizado</th>
                    <th scope="col" className="mom-num">
                      Realizado / PO
                      <InfoTooltip texto="O realizado sobre o PO." rotulo="O que é realizado / PO" />
                    </th>
                  </tr>
                </thead>
                {dados.gestores.map((g) => (
                  <Gestor key={g.gestor} gestor={g} />
                ))}
                <tbody className="fc-total">
                  <LinhaDeSoma rotulo="Total" sub={dados.vendasSemGestor > 0 ? `com ${n(dados.vendasSemGestor)} venda(s) sem gestor` : null} linhas={dados.total} />
                </tbody>
              </table>
            </div>
          )}
        </PainelDoMomento>
      </section>
    </PaginaDoPainel>
  );
}

/** Um gestor: a soma na primeira linha, e cada linha de produto abaixo. */
function Gestor({ gestor: g }: { gestor: ForecastDoGestor }) {
  return (
    <tbody className="fc-gestor">
      <LinhaDeSoma rotulo={g.gestor} sub={`${n(g.consultores)} consultor(es) no time`} linhas={g.linhas} />
      {g.linhas.map((l) => (
        <tr key={l.codigo} className="fc-linha">
          <td className="fc-nome-da-linha">{l.nome}</td>
          <Numeros linha={l} />
        </tr>
      ))}
    </tbody>
  );
}

function LinhaDeSoma({ rotulo, sub, linhas }: { rotulo: string; sub: string | null; linhas: LinhaDoForecast[] }) {
  const soma = somar(linhas);
  return (
    <tr className="fc-soma">
      <th scope="rowgroup">
        {rotulo}
        {sub && <div className="cad-sub">{sub}</div>}
      </th>
      <Numeros linha={soma} />
    </tr>
  );
}

function Numeros({ linha: l }: { linha: Omit<LinhaDoForecast, 'codigo' | 'nome'> }) {
  const razao = doPo(l.realizado, l.meta);
  return (
    <>
      <td className="mom-num">{n(l.meta)}</td>
      <td className="mom-num">{l.forecast === null ? <ValorAusente motivo={NAO_INFORMADO} oQue="o forecast" /> : n(l.forecast)}</td>
      <td className="mom-num">{l.bestGuess === null ? <ValorAusente motivo={NAO_INFORMADO} oQue="o best guess" /> : n(l.bestGuess)}</td>
      <td className="mom-num">{n(l.realizado)}</td>
      <td className="mom-num">
        {razao ?? <ValorAusente motivo="Sem PO nesta linha: não há base para a razão." oQue="o realizado sobre o PO" />}
      </td>
    </>
  );
}

function dataEHora(iso: string) {
  return new Date(iso).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}
