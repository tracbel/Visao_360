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
 */

import { useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterForecastDaGerencia } from '../dados/api/forecast';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { ForecastDoGestor, LinhaDoForecast, RelatorioDoForecast } from '../tipos/forecast';
import '../estilos/forecast.css';

const NAO_INFORMADO = 'O gestor não informou este número na Gestão de Negócios: fica vazio, e não zero.';

const n = (v: number) => v.toLocaleString('pt-BR');
const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const mesPorExtenso = (aaaammdd: string) => `${MESES[Number(aaaammdd.slice(5, 7)) - 1]}/${aaaammdd.slice(0, 4)}`;

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

  const indicadores: Indicador[] = [
    { rotulo: 'PO (meta)', valor: total ? total.meta : null, deOnde: 'a meta de máquinas do mês — API Gestão de Negócios' },
    {
      rotulo: 'Forecast',
      valor: total?.forecast ?? null,
      deOnde: total ? (doPo(total.forecast, total.meta) ?? 'sem PO no mês') + ' do PO · revisto uma vez por mês' : '',
      semDado: 'nenhum gestor informou',
    },
    {
      rotulo: 'Best Guess',
      valor: total?.bestGuess ?? null,
      deOnde: total ? (doPo(total.bestGuess, total.meta) ?? 'sem PO no mês') + ' do PO · reavaliado toda segunda' : '',
      semDado: 'nenhum gestor informou',
    },
    {
      rotulo: 'Realizado',
      valor: total ? total.realizado : null,
      deOnde: total ? (doPo(total.realizado, total.meta) ?? 'sem PO no mês') + ' do PO · máquinas vendidas (ART)' : '',
      tom: total && total.meta > 0 && total.realizado >= total.meta ? 'bom' : 'neutro',
    },
    { rotulo: 'Gestores', valor: dados ? dados.gestores.length : null, deOnde: 'pelo de-para de consultores da GN' },
  ];

  const meses = dados ? [...new Set([...dados.mesesDisponiveis, dados.competencia])].sort().reverse() : [];

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Forecast da Gerência</h1>
          <p className="page-subtitle">
            A previsão de cada gestor — o Forecast do mês e o Best Guess da semana — ao lado do PO e do realizado do time
            dele.
          </p>
        </div>
        {dados && (
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => baixarCsv(`forecast-da-gerencia-${dados.competencia.slice(0, 7)}-${carimboDeData()}`, CABECALHO_DO_CSV, linhasDoCsv(dados))}
            disabled={dados.total.length === 0}
          >
            Exportar CSV
          </button>
        )}
      </div>

      <AvisoDeProcedencia procedencia={forecast.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={forecast.carregando} />

      <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que este forecast não afirma" />

      <div className="card cad-cartao" data-bloco="forecast-por-gestor">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Previsão por gestor{dados ? ` · ${dados.texto}` : ''}</div>
            <div className="card-subtitle">
              {dados
                ? dados.alcance === 'Organizacao'
                  ? 'PO e realizado de todas as filiais'
                  : 'PO e realizado da filial escolhida; o forecast é o do gestor inteiro'
                : 'o forecast, o best guess, o PO e o realizado de cada gestor'}
              {dados?.lidoEm && <> · lido da Gestão de Negócios em {dataEHora(dados.lidoEm)}</>}
            </div>
          </div>
          <SeloProcedencia procedencia={forecast.procedencia} />
        </div>

        {meses.length > 0 && (
          <div className="cad-barra">
            <label className="cad-filtro">
              Mês
              <select value={dados!.competencia} onChange={(e) => setCompetencia(e.target.value.slice(0, 7))}>
                {meses.map((m) => (
                  <option key={m} value={m}>
                    {mesPorExtenso(m)}
                    {dados!.mesesDisponiveis.includes(m) ? '' : ' (sem forecast)'}
                  </option>
                ))}
              </select>
            </label>
          </div>
        )}

        {forecast.carregando && <BlocoCarregando oQue="o forecast" />}
        {forecast.erro && <BlocoErro erro={forecast.erro} aoTentarDeNovo={forecast.recarregar} />}

        {dados && dados.total.length === 0 && (
          <BlocoVazio
            titulo={`Nada no mês de ${dados.texto}`}
            texto="Nenhum gestor informou forecast, e o time não tem PO nem venda neste mês."
          />
        )}

        {dados && dados.total.length > 0 && (
          <div className="cad-tabela-wrap">
            <table className="cad-tabela fc-tabela">
              <caption className="cad-so-leitor">Forecast, best guess, PO e realizado de cada gestor, por linha</caption>
              <thead>
                <tr>
                  <th scope="col">Gestor · linha</th>
                  <th scope="col">PO</th>
                  <th scope="col">Forecast</th>
                  <th scope="col">Best Guess</th>
                  <th scope="col">Realizado</th>
                  <th scope="col" title="O realizado sobre o PO">Realizado / PO</th>
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

        <div className="funil-scope-note">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 8h.01" />
            <path d="M11 12h1v4h1" />
          </svg>
          <div>
            <strong>Como ler:</strong> o Forecast é o compromisso do gestor para o mês; o Best Guess, a aposta dele na semana.
            Os dois vêm da Gestão de Negócios, em máquinas, e o CRM não os edita. O PO é a meta dos consultores do time, e o
            realizado, o que eles venderam pelo ART no mês.
          </div>
        </div>
      </div>
    </>
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
      <td className="cad-mono">{n(l.meta)}</td>
      <td className="cad-mono">{l.forecast === null ? <ValorAusente motivo={NAO_INFORMADO} oQue="o forecast" /> : n(l.forecast)}</td>
      <td className="cad-mono">{l.bestGuess === null ? <ValorAusente motivo={NAO_INFORMADO} oQue="o best guess" /> : n(l.bestGuess)}</td>
      <td className="cad-mono">{n(l.realizado)}</td>
      <td className="cad-mono">
        {razao ?? <ValorAusente motivo="Sem PO nesta linha: não há base para a razão." oQue="o realizado sobre o PO" />}
      </td>
    </>
  );
}

function dataEHora(iso: string) {
  return new Date(iso).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}
