/**
 * Forecast da Gerência (28/09/2026) — a previsão de cada gestor, ao lado do PG e do realizado do time dele, num mês.
 *
 * É a tela "Forecast Gerência" da API Gestão de Negócios com o realizado que o CRM tem:
 *
 * - **o Forecast e o Best Guess vêm da GN** — o gestor os informa lá (o Forecast uma vez por mês; o Best Guess, toda
 *   segunda), e a rotina das metas os traz. O CRM não edita;
 * - **o PG é a meta do time do gestor** — o PO da Gestão de Negócios —, e o **realizado**, as máquinas vendidas pelo time
 *   (o ART) — o time é o de-para de consultores da GN;
 * - **previsão não informada fica vazia**, com o motivo, e não vira zero.
 *
 * É da gerência: quem só vê a própria meta recebe a recusa da API, com o motivo.
 *
 * 29/09/2026 — NO PADRÃO DOS INDICADORES GEOGRÁFICOS (#293, bloco 3): página na coluna inteira, o mês na barra de filtros,
 * os cinco números em cartões de decisão e a tabela num painel da seção. Nenhum número nem texto mudou: o "Como ler" que
 * ficava no pé do cartão foi para a dica do painel.
 *
 * 01/10/2026 — A MAQUETE DO RICARDO (`docs/prototipo/capturas-referencia/forecast-gerencia-maquete-2026-10-01.png`).
 * Decisões dele:
 *
 * - A META SE CHAMA PG, como a maquete; o ⓘ diz que é o PO da Gestão de Negócios.
 * - "REGIONAL / GERÊNCIA" ESCOLHE UM GESTOR: o CRM não tem regional, e a gerência da GN é o gestor do time. Escolhido um,
 *   os cartões, o alerta, os gráficos e a tabela passam a ser do time dele.
 * - CADA CARTÃO COMPARA COM O MÊS ANTERIOR INTEIRO, como a maquete: o forecast de cada mês está guardado. No mês aberto, o
 *   realizado ainda vai até hoje, e o ⓘ diz isso.
 *
 * O resto do desenho, sem inventar número: a "Visão" troca o agrupamento (por gestor ou por linha de produto); o alerta
 * "Atenção da gerência" é a conta do forecast contra o realizado e do best guess contra o mês anterior; o ranking por
 * atingimento, o PG × realizado e a distribuição do realizado saem da mesma tabela; a tabela agrupa os cinco primeiros e
 * "Demais gestores", com a barra do atingimento. O que o forecast não afirma foi para o ⓘ da tabela — e aparece aberto
 * quando o mês não tem nada. Sem o botão "Aplicar filtros": os filtros aplicam na hora, como nas outras telas.
 */

import {
  ArrowDown,
  ArrowRight,
  ArrowUp,
  CalendarDays,
  ChartColumn,
  ChartColumnIncreasing,
  ChartPie,
  ChevronDown,
  ChevronRight,
  ChevronsUpDown,
  CircleGauge,
  Download,
  MapPinned,
  RefreshCw,
  Target,
  TrendingUp,
  TriangleAlert,
  Trophy,
  UsersRound,
} from 'lucide-react';
import { useMemo, useState, type CSSProperties, type ReactNode } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { GraficoDonutCentro } from '../componentes/GraficoDonutCentro';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterForecastDaGerencia } from '../dados/api/forecast';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { LinhaDoForecast, RelatorioDoForecast } from '../tipos/forecast';
import { nomeCurto, nomeProprio } from './cadastro/formato';
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
/** "Set/2026", como o seletor da maquete. */
const mesDoSeletor = (aaaammdd: string) => {
  const texto = mesPorExtenso(aaaammdd);
  return texto.charAt(0).toUpperCase() + texto.slice(1);
};

/** O mês anterior, em `AAAA-MM`. */
function mesAnterior(aaaammdd: string): string {
  const ano = Number(aaaammdd.slice(0, 4));
  const mes = Number(aaaammdd.slice(5, 7));
  return mes === 1 ? `${ano - 1}-12` : `${ano}-${String(mes - 1).padStart(2, '0')}`;
}

/** O mês corrente em São Paulo, em `AAAA-MM` — é ele que ainda está aberto. */
const mesCorrente = () => new Date().toLocaleDateString('en-CA', { timeZone: 'America/Sao_Paulo' }).slice(0, 7);

const COMO_LER =
  'O Forecast é o compromisso do gestor para o mês; o Best Guess, a aposta dele na semana. Os dois vêm da Gestão de ' +
  'Negócios, em máquinas, e o CRM não os edita. O PG é a meta dos consultores do time — o PO da Gestão de Negócios —, e o ' +
  'realizado, as máquinas que eles venderam e o ART deu como entregues no mês — a régua da Gestão de Negócios.';

const MES_ABERTO =
  'A comparação é com o mês anterior inteiro. No mês corrente, que ainda está aberto, o realizado vai só até hoje — a ' +
  'diferença diminui até o fim do mês.';

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

/** Quanto do PG: nulo sem PG — não há base para a razão. */
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

const CABECALHO_DO_CSV = ['Gestor', 'Linha', 'PG (meta)', 'Forecast', 'Best Guess', 'Realizado'];

type Soma = Omit<LinhaDoForecast, 'codigo' | 'nome'>;

/** Uma linha da tabela e dos gráficos: um gestor (com as linhas de produto dentro) ou uma linha (com os gestores). */
type Grupo = {
  chave: string;
  nome: string;
  curto: string;
  /** Os consultores do time — só na visão por gestor. */
  consultores: number | null;
  soma: Soma;
  filhos: { chave: string; nome: string; soma: Soma }[];
};

type Visao = 'gestor' | 'linha';

/**
 * O RECORTE DO FILTRO: o relatório com só o gestor escolhido — os gestores e o total —, para a tela e o CSV dizerem a
 * mesma coisa. Sem gestor, o relatório inteiro, com as vendas sem gestor no total.
 */
function recortar(relatorio: RelatorioDoForecast, gestor: string | null): RelatorioDoForecast {
  if (!gestor) return relatorio;
  const dele = relatorio.gestores.filter((g) => g.gestor === gestor);
  return { ...relatorio, gestores: dele, total: dele.flatMap((g) => g.linhas), vendasSemGestor: 0 };
}

/** Os grupos da visão: por gestor, cada gestor com as linhas dele; por linha, cada linha com os gestores que a têm. */
function gruposDe(relatorio: RelatorioDoForecast, visao: Visao): Grupo[] {
  if (visao === 'gestor') {
    return relatorio.gestores.map((g) => ({
      chave: g.gestor,
      nome: nomeProprio(g.gestor),
      curto: nomeCurto(g.gestor),
      consultores: g.consultores,
      soma: somar(g.linhas),
      filhos: g.linhas.map((l) => ({ chave: l.codigo, nome: nomeProprio(l.nome), soma: l })),
    }));
  }
  const codigos = [...new Map(relatorio.total.map((l) => [l.codigo, l])).values()];
  return codigos.map((total) => {
    const daLinha = relatorio.total.filter((l) => l.codigo === total.codigo);
    const soma = somar(daLinha);
    const filhos = relatorio.gestores.flatMap((g) =>
      g.linhas.filter((l) => l.codigo === total.codigo).map((l) => ({ chave: g.gestor, nome: nomeProprio(g.gestor), soma: l as Soma })),
    );
    // AS VENDAS SEM GESTOR DA LINHA, para os gestores somarem a linha: estão no total e em gestor nenhum.
    const semGestor = soma.realizado - filhos.reduce((s, f) => s + f.soma.realizado, 0);
    if (semGestor > 0) {
      filhos.push({ chave: '__sem_gestor', nome: 'Vendas sem gestor', soma: { meta: 0, forecast: null, bestGuess: null, realizado: semGestor } });
    }
    return { chave: total.codigo, nome: nomeProprio(total.nome), curto: nomeProprio(total.nome), consultores: null, soma, filhos };
  });
}

type Campo = 'nome' | 'meta' | 'forecast' | 'bestGuess' | 'realizado' | 'atingimento';

const atingimento = (s: Soma) => (s.meta > 0 ? s.realizado / s.meta : null);

/** Quantos gestores aparecem um a um na tabela antes de "Demais gestores" — os cinco da maquete. */
const LINHAS_A_VISTA = 5;

/** As cores da distribuição do realizado, na ordem da maquete; a última é a de "Demais". */
const CORES_DA_DISTRIBUICAO = ['#1B5E20', '#3CB54A', '#C5DD6E', '#FB8C00', '#A855F7'];
const COR_DE_DEMAIS = '#CDD3DB';
const COR_SEM_GESTOR = '#7DB8F2';

export function ForecastGerencia() {
  const { contexto } = useContextoDeAcesso();
  const [competencia, setCompetencia] = useState<string | undefined>(undefined);
  const [gestorEscolhido, setGestorEscolhido] = useState<string | null>(null);
  const [visao, setVisao] = useState<Visao>('gestor');
  const [quantosNoRanking, setQuantosNoRanking] = useState(5);

  const forecast = useRecurso(
    (sinal) => obterForecastDaGerencia(contexto, competencia, sinal),
    [contexto.empresa, contexto.usuario, competencia],
  );
  const dados = forecast.dados;

  /** O MÊS ANTERIOR, para a comparação dos cartões (decisão do Ricardo, 01/10/2026): o forecast de cada mês está guardado. */
  const anterior = useRecurso(
    (sinal) =>
      dados
        ? obterForecastDaGerencia(contexto, mesAnterior(dados.competencia), sinal)
        : Promise.resolve({ dados: null as RelatorioDoForecast | null, procedencia: null }),
    [contexto.empresa, contexto.usuario, dados?.competencia],
  );

  const recorte = useMemo(() => (dados ? recortar(dados, gestorEscolhido) : null), [dados, gestorEscolhido]);
  const recorteAnterior = useMemo(
    () => (anterior.dados ? recortar(anterior.dados, gestorEscolhido) : null),
    [anterior.dados, gestorEscolhido],
  );

  const total = recorte ? somar(recorte.total) : null;
  const totalAnterior = recorteAnterior ? somar(recorteAnterior.total) : null;
  const grupos = useMemo(() => (recorte ? gruposDe(recorte, visao) : []), [recorte, visao]);

  const meses = dados ? [...new Set([...dados.mesesDisponiveis, dados.competencia])].sort().reverse() : [];
  const gestoresDoSeletor = useMemo(
    () => [...(dados?.gestores ?? [])].sort((a, b) => nomeProprio(a.gestor).localeCompare(nomeProprio(b.gestor), 'pt-BR')),
    [dados],
  );
  const mesAberto = dados ? dados.competencia.slice(0, 7) === mesCorrente() : false;
  const entidade = visao === 'gestor' ? 'gestores' : 'linhas';

  /** A comparação de um cartão com o mês anterior: a seta, o percentual e o "vs. mês anterior". */
  const comparacao = (atual: number | null, doMesAnterior: number | null | undefined) => (
    <Comparacao atual={atual} anterior={anterior.carregando ? undefined : doMesAnterior ?? null} />
  );

  function limparFiltros() {
    setCompetencia(undefined);
    setGestorEscolhido(null);
    setVisao('gestor');
  }

  const temFiltro = competencia !== undefined || gestorEscolhido !== null || visao !== 'gestor';
  const gestoresComTime = recorte ? recorte.gestores.filter((g) => g.consultores > 0).length : 0;

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga fc-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">
            Forecast da Gerência
            <InfoTooltip
              rotulo="O que o Forecast da Gerência mostra"
              texto={`A previsão de cada gestor — o Forecast do mês e o Best Guess da semana — ao lado do PG e do realizado do time dele. ${COMO_LER}`}
            />
          </h1>
          <p className="page-subtitle">
            Acompanhe o planejado (PG), o forecast, o best guess e o realizado da sua gerência, com detalhamento por gestor e
            linha.
          </p>
        </div>
        <p className="dash-atualizado">
          <CalendarDays className="fc-calendario" size={15} strokeWidth={2} aria-hidden="true" />
          {forecast.procedencia ? <DadosAtualizadosEm procedencia={forecast.procedencia} /> : 'Lendo o forecast…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              forecast.recarregar();
              anterior.recarregar();
            }}
            disabled={forecast.carregando}
            data-carregando={forecast.carregando ? 'true' : 'false'}
            aria-label="Reler o forecast"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES (01/10/2026): o mês, a gerência e a visão, aplicados na hora — sem o botão "Aplicar". */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="mes">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Mês de referência</span>
              <span className="fc-campo-com-icone">
                <CalendarDays size={15} strokeWidth={2} aria-hidden="true" />
                <select
                  value={dados?.competencia ?? ''}
                  onChange={(e) => setCompetencia(e.target.value.slice(0, 7))}
                  disabled={meses.length === 0}
                >
                  {meses.length === 0 && <option value="">—</option>}
                  {meses.map((m) => (
                    <option key={m} value={m}>
                      {mesDoSeletor(m)}
                      {dados!.mesesDisponiveis.includes(m) ? '' : ' (sem forecast)'}
                    </option>
                  ))}
                </select>
              </span>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="gerencia">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Regional / Gerência
                <InfoTooltip
                  rotulo="O que é a regional aqui"
                  texto="O CRM não tem regional: a gerência da Gestão de Negócios é o gestor do time — alguns se chamam pela regional. Escolhido um, os cartões, o alerta, os gráficos e a tabela passam a ser do time dele."
                />
              </span>
              <span className="fc-campo-com-icone">
                <MapPinned size={15} strokeWidth={2} aria-hidden="true" />
                <select
                  value={gestorEscolhido ?? ''}
                  onChange={(e) => setGestorEscolhido(e.target.value === '' ? null : e.target.value)}
                  disabled={gestoresDoSeletor.length === 0}
                >
                  <option value="">Todas as regionais</option>
                  {gestoresDoSeletor.map((g) => (
                    <option key={g.gestor} value={g.gestor}>
                      {nomeProprio(g.gestor)}
                    </option>
                  ))}
                </select>
              </span>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="visao">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Visão</span>
              <span className="fc-campo-com-icone">
                <ChartColumn size={15} strokeWidth={2} aria-hidden="true" />
                <select value={visao} onChange={(e) => setVisao(e.target.value as Visao)}>
                  <option value="gestor">Por gestor</option>
                  <option value="linha">Por linha de produto</option>
                </select>
              </span>
            </span>
          </label>

          <div className="dash-filtros-acao">
            {/* SEMPRE À VISTA, como a maquete: sem filtro escolhido, o clique não muda nada — e o leitor de tela ouve isso. */}
            <button type="button" className="fc-limpar" onClick={limparFiltros} aria-disabled={!temFiltro}>
              Limpar filtros
            </button>
          </div>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={forecast.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="PG"
          icone={Target}
          tom="demanda"
          valor={total ? n(total.meta) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={undefined}
          variacao={total ? <LinhasDoCartao comparacao={comparacao(total.meta, totalAnterior?.meta)} texto="Meta comercial da gerência" /> : null}
          sobre={`O PG — o PO da Gestão de Negócios: a meta de máquinas do mês dos consultores do time de cada gestor, somada. ${MES_ABERTO}`}
        />
        <CartaoDeDecisao
          rotulo="Forecast"
          icone={ChartColumn}
          tom="mercado"
          valor={total?.forecast != null ? n(total.forecast) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={total ? NAO_INFORMADO : undefined}
          variacao={
            total ? (
              <LinhasDoCartao
                comparacao={total.forecast === null ? 'nenhum gestor informou' : comparacao(total.forecast, totalAnterior?.forecast)}
                texto="Previsão atual da gerência"
              />
            ) : null
          }
          sobre={`O compromisso do gestor para o mês, informado na Gestão de Negócios e revisto uma vez por mês. O CRM não o edita, e o que ninguém informou fica vazio, e não zero. ${MES_ABERTO}`}
        />
        <CartaoDeDecisao
          rotulo="Best Guess"
          icone={TrendingUp}
          tom="oportunidade"
          valor={total?.bestGuess != null ? n(total.bestGuess) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={total ? NAO_INFORMADO : undefined}
          variacao={
            total ? (
              <LinhasDoCartao
                comparacao={total.bestGuess === null ? 'nenhum gestor informou' : comparacao(total.bestGuess, totalAnterior?.bestGuess)}
                texto="Cenário mais provável de fechamento"
              />
            ) : null
          }
          sobre={`A aposta do gestor na semana, informada na Gestão de Negócios toda segunda. O CRM não a edita, e o que ninguém informou fica vazio, e não zero. ${MES_ABERTO}`}
        />
        <CartaoDeDecisao
          rotulo="Realizado"
          icone={ChartPie}
          tom="captura"
          valor={total ? n(total.realizado) : null}
          carregando={forecast.carregando}
          unidade="máquinas"
          motivoSemDado={undefined}
          variacao={
            total ? (
              <LinhasDoCartao
                comparacao={comparacao(total.realizado, totalAnterior?.realizado)}
                texto={mesAberto ? 'Vendido até o momento (MTD)' : 'Vendido no mês'}
              />
            ) : null
          }
          sobre={`As máquinas que os consultores do time venderam e o ART deu como entregues no mês — a régua da Gestão de Negócios. ${MES_ABERTO}`}
        />
        <CartaoDeDecisao
          rotulo="Gestores"
          icone={UsersRound}
          tom="neutro"
          valor={recorte ? n(recorte.gestores.length) : null}
          carregando={forecast.carregando}
          unidade="gestores"
          motivoSemDado={undefined}
          variacao={
            recorte && dados
              ? gestorEscolhido
                ? `de ${n(dados.gestores.length)} gestores no mês`
                : gestoresComTime === recorte.gestores.length
                  ? 'com pelo menos um consultor no time'
                  : `${n(recorte.gestores.length - gestoresComTime)} sem consultor no de-para`
              : null
          }
          sobre="Os gestores com forecast, PG ou venda no mês. O time de cada um é o de-para de consultores da Gestão de Negócios."
        />
      </div>

      {total && <AtencaoDaGerencia total={total} anterior={totalAnterior} comGestor={gestorEscolhido !== null} />}

      {/* OS TRÊS GRÁFICOS DA MAQUETE — todos da mesma tabela de baixo. */}
      <div className="fc-graficos" data-bloco="graficos">
        <PainelDoMomento
          titulo={`Ranking de ${entidade} por atingimento`}
          area="fc-ranking"
          data-bloco="ranking"
          icone={<Trophy className="fc-painel-icone" size={22} strokeWidth={2.2} aria-hidden="true" />}
          subtitulo={`Top ${Math.min(quantosNoRanking || grupos.length, grupos.filter((g) => g.soma.meta > 0).length)} ${entidade} com maior atingimento no mês.`}
          dica={`O atingimento é o realizado sobre o PG. Sem PG não há base para a razão, e a ${visao === 'gestor' ? 'gestor' : 'linha'} sem PG fica fora do ranking — ela continua na tabela.`}
          direita={
            <label className="fc-seletor">
              <span className="cad-so-leitor">Quantos no ranking</span>
              <select value={quantosNoRanking} onChange={(e) => setQuantosNoRanking(Number(e.target.value))}>
                <option value={5}>Top 5</option>
                <option value={10}>Top 10</option>
                <option value={0}>Todos</option>
              </select>
            </label>
          }
        >
          {forecast.carregando && <BlocoCarregando oQue="o ranking" />}
          {!forecast.carregando && <RankingPorAtingimento grupos={grupos} quantos={quantosNoRanking} />}
        </PainelDoMomento>

        <PainelDoMomento
          titulo={`PG vs Realizado por ${visao === 'gestor' ? 'gestor' : 'linha'}`}
          area="fc-pg"
          data-bloco="pg-e-realizado"
          icone={<ChartColumnIncreasing className="fc-painel-icone" size={22} strokeWidth={2.6} aria-hidden="true" />}
          subtitulo="Comparativo de meta (PG) e realizado no mês."
          dica={`Os cinco maiores PG do mês. ${COMO_LER}`}
          direita={
            <ul className="fc-legenda fc-legenda-pg" aria-label="Legenda do PG e do realizado">
              <li>
                <i style={{ background: '#1B6E35' }} aria-hidden="true" />
                PG
              </li>
              <li>
                <i style={{ background: '#5FD07A' }} aria-hidden="true" />
                Realizado
              </li>
            </ul>
          }
        >
          {forecast.carregando && <BlocoCarregando oQue="o PG e o realizado" />}
          {!forecast.carregando && <PgERealizado grupos={grupos} />}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Distribuição do realizado"
          area="fc-distribuicao"
          data-bloco="distribuicao"
          icone={<CircleGauge className="fc-painel-icone" size={22} strokeWidth={2.2} aria-hidden="true" />}
          subtitulo={total ? `Participação no total de ${n(total.realizado)} máquinas.` : 'Participação no total do realizado.'}
          dica={`Os cinco que mais venderam no mês e o resto somado em "Demais".${visao === 'gestor' && !gestorEscolhido ? ' As vendas cujo vendedor não está no de-para de consultores entram no total e em gestor nenhum — elas têm a fatia delas.' : ''}`}
        >
          {forecast.carregando && <BlocoCarregando oQue="a distribuição do realizado" />}
          {!forecast.carregando && total && (
            <DistribuicaoDoRealizado
              grupos={grupos}
              total={total.realizado}
              semGestor={visao === 'gestor' && recorte ? recorte.vendasSemGestor : 0}
              entidade={entidade}
            />
          )}
        </PainelDoMomento>
      </div>

      <section className="dash-secao fc-cartao" data-bloco="secao-gestores">
        <TituloDaSecao
          icone={<UsersRound className="fc-secao-icone" size={24} strokeWidth={2.2} aria-hidden="true" />}
          titulo={visao === 'gestor' ? 'Previsão por gestor' : 'Previsão por linha de produto'}
          subtitulo={
            visao === 'gestor'
              ? 'Detalhamento da previsão, best guess e realizado por gestor e suas linhas de produto.'
              : 'Detalhamento da previsão, best guess e realizado por linha de produto e seus gestores.'
          }
          metodologia={
            <>
              <p>{COMO_LER}</p>
              {dados?.lidoEm && <p>Lido da Gestão de Negócios em {dataEHora(dados.lidoEm)}.</p>}
              {dados && (
                <p>
                  {dados.alcance === 'Organizacao'
                    ? 'PG e realizado de todas as filiais.'
                    : 'PG e realizado da filial escolhida; o forecast é o do gestor inteiro.'}
                </p>
              )}
              <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que este forecast não afirma" naDica />
            </>
          }
          acao={
            recorte && (
              <button
                type="button"
                className="fc-secao-acao fc-exportar"
                onClick={() =>
                  baixarCsv(
                    `forecast-da-gerencia-${recorte.competencia.slice(0, 7)}-${carimboDeData()}`,
                    CABECALHO_DO_CSV,
                    linhasDoCsv(recorte),
                  )
                }
                disabled={recorte.total.length === 0}
              >
                <Download size={16} strokeWidth={2.2} aria-hidden="true" />
                Exportar CSV
              </button>
            )
          }
        />

        {/* O PAINEL NÃO TEM TÍTULO À VISTA NA MAQUETE: o título e o mês ficam para o leitor de tela. */}
        <PainelDoMomento
          titulo={`${visao === 'gestor' ? 'Previsão por gestor' : 'Previsão por linha de produto'}${dados ? ` · ${dados.texto}` : ''}`}
          data-bloco="forecast-por-gestor"
        >
          {forecast.carregando && <BlocoCarregando oQue="o forecast" />}
          {forecast.erro && <BlocoErro erro={forecast.erro} aoTentarDeNovo={forecast.recarregar} />}

          {recorte && recorte.total.length === 0 && (
            <>
              <BlocoVazio
                titulo={gestorEscolhido ? `Nada deste gestor em ${recorte.texto}` : `Nada no mês de ${recorte.texto}`}
                texto={
                  gestorEscolhido
                    ? 'O gestor escolhido não informou forecast, e o time dele não tem PG nem venda neste mês.'
                    : 'Nenhum gestor informou forecast, e o time não tem PG nem venda neste mês.'
                }
              />
              {/* SEM NÚMERO NENHUM, O QUE O FORECAST NÃO AFIRMA SAI DO ⓘ: é ele que explica o vazio. */}
              <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que este forecast não afirma" />
            </>
          )}

          {recorte && total && recorte.total.length > 0 && (
            <TabelaDoForecast
              grupos={grupos}
              total={total}
              visao={visao}
              rotuloDoTotal={gestorEscolhido ? 'Total do gestor' : 'Total da gerência'}
              vendasSemGestor={recorte.vendasSemGestor}
            />
          )}
        </PainelDoMomento>
      </section>
    </PaginaDoPainel>
  );
}

/** As duas linhas de baixo do cartão da maquete: a comparação com o mês anterior e o que o número é. */
function LinhasDoCartao({ comparacao, texto }: { comparacao: ReactNode; texto: string }) {
  return (
    <>
      <span className="fc-comparacao">{comparacao}</span>
      <span className="fc-descricao">{texto}</span>
    </>
  );
}

/**
 * A COMPARAÇÃO COM O MÊS ANTERIOR (decisão do Ricardo, 01/10/2026): a seta, o percentual e o "vs. mês anterior". Sem o
 * número do mês anterior, a frase diz isso — e não "0%".
 */
function Comparacao({ atual, anterior }: { atual: number | null; anterior: number | null | undefined }) {
  if (atual === null) return null;
  if (anterior === undefined) return <span className="fc-sem-comparacao">lendo o mês anterior…</span>;
  if (anterior === null) return <span className="fc-sem-comparacao">sem o mês anterior para comparar</span>;
  if (anterior === 0) return <span className="fc-sem-comparacao">sem base no mês anterior</span>;
  const pct = Math.round(((atual - anterior) / anterior) * 100);
  const sentido = pct > 0 ? 'sobe' : pct < 0 ? 'desce' : 'igual';
  const Seta = pct > 0 ? ArrowUp : pct < 0 ? ArrowDown : ArrowRight;
  return (
    <>
      <span className="fc-variacao" data-sentido={sentido}>
        <Seta size={13} strokeWidth={2.6} aria-hidden="true" />
        {pct > 0 ? '+' : ''}
        {pct}%
      </span>{' '}
      vs. mês anterior
    </>
  );
}

/**
 * ATENÇÃO DA GERÊNCIA, como a maquete: o forecast contra o realizado do mês, e o best guess contra o forecast e contra o
 * mês anterior. Só aparece com previsão informada — sem ela não há o que comparar.
 */
function AtencaoDaGerencia({ total, anterior, comGestor }: { total: Soma; anterior: Soma | null; comGestor: boolean }) {
  const { forecast, bestGuess, realizado } = total;
  if (forecast === null && bestGuess === null) return null;

  const frases: string[] = [];
  if (forecast !== null) {
    if (forecast > realizado) {
      const diferenca = forecast - realizado;
      frases.push(
        `O forecast (${n(forecast)}) está acima do realizado (${n(realizado)}), com uma diferença de ${n(diferenca)} ${diferenca === 1 ? 'máquina' : 'máquinas'}${realizado > 0 ? ` (+${Math.round((100 * diferenca) / realizado)}%)` : ''}.`,
      );
    } else {
      frases.push(`O realizado (${n(realizado)}) já alcançou o forecast (${n(forecast)}).`);
    }
  }
  if (bestGuess !== null) {
    const cenario =
      forecast === null ? '' : bestGuess < forecast ? ' indica um cenário mais conservador' : bestGuess > forecast ? ' indica um cenário mais otimista' : ' é igual ao forecast';
    const contraOAnterior =
      anterior?.bestGuess != null && anterior.bestGuess > 0
        ? (() => {
            const pct = Math.round(((bestGuess - anterior.bestGuess) / anterior.bestGuess) * 100);
            return pct === 0 ? ', igual ao mês anterior' : `, ${Math.abs(pct)}% ${pct < 0 ? 'abaixo' : 'acima'} do mês anterior`;
          })()
        : '';
    frases.push(`O Best Guess (${n(bestGuess)})${cenario}${contraOAnterior}.`);
  }

  return (
    <div className="fc-alerta" role="note" data-bloco="atencao-da-gerencia">
      <TriangleAlert className="fc-alerta-icone" size={30} strokeWidth={2.2} aria-hidden="true" />
      <div className="fc-alerta-texto">
        <strong className="fc-alerta-titulo">Atenção da Gerência</strong>
        <span>
          <strong>{frases[0]}</strong> {frases.slice(1).join(' ')}
        </span>
        {!comGestor && (
          <span>
            Recomenda-se foco nos gestores com maior gap de atingimento e priorização das ações comerciais nos CENs com
            menor cobertura.
          </span>
        )}
      </div>
    </div>
  );
}

/** O RANKING POR ATINGIMENTO, como a maquete: a trilha cinza inteira, a barra verde que clareia e o percentual no fim. */
function RankingPorAtingimento({ grupos, quantos }: { grupos: Grupo[]; quantos: number }) {
  const comBase = grupos
    .map((g) => ({ grupo: g, valor: atingimento(g.soma) }))
    .filter((x): x is { grupo: Grupo; valor: number } => x.valor !== null)
    .sort((a, b) => b.valor - a.valor);
  const mostrados = quantos > 0 ? comBase.slice(0, quantos) : comBase;
  if (mostrados.length === 0) return <BlocoVazio titulo="Ninguém com PG no mês" texto="O atingimento é o realizado sobre o PG; sem PG não há razão." />;
  const maior = Math.max(1, ...mostrados.map((m) => m.valor));
  return (
    <ol className="fc-ranking">
      {mostrados.map(({ grupo, valor }, i) => (
        <li
          key={grupo.chave}
          className="fc-ranking-linha"
          style={{ '--fc-tom': `${Math.round((i / Math.max(1, mostrados.length - 1)) * 60)}%` } as CSSProperties}
        >
          <span className="fc-ranking-nome" aria-hidden="true">
            {grupo.curto}
          </span>
          <span className="fc-ranking-trilho" aria-hidden="true">
            <span className="fc-ranking-barra" data-primeira={i === 0 ? 'true' : undefined} style={{ width: `${Math.min(100, (100 * valor) / maior) * 0.82}%` }} />
            <span className="fc-ranking-valor">{Math.round(valor * 100)}%</span>
          </span>
          <span className="cad-so-leitor">
            {i + 1}º, {grupo.nome}: {Math.round(valor * 100)}% do PG — {n(grupo.soma.realizado)} de {n(grupo.soma.meta)} máquinas
          </span>
        </li>
      ))}
    </ol>
  );
}

/** O PG E O REALIZADO, em par, dos cinco maiores PG — o número na ponta de cada barra. */
function PgERealizado({ grupos }: { grupos: Grupo[] }) {
  const cinco = [...grupos].filter((g) => g.soma.meta > 0 || g.soma.realizado > 0).sort((a, b) => b.soma.meta - a.soma.meta).slice(0, 5);
  if (cinco.length === 0) return <BlocoVazio titulo="Sem PG nem venda no mês" texto="Nada para comparar." />;
  const maior = Math.max(1, ...cinco.flatMap((g) => [g.soma.meta, g.soma.realizado]));
  return (
    <ol className="fc-pg">
      {cinco.map((g) => (
        <li key={g.chave} className="fc-pg-linha">
          <span className="fc-pg-nome" aria-hidden="true">
            {g.curto}
          </span>
          <span className="fc-pg-barras" aria-hidden="true">
            <span className="fc-pg-par">
              <span className="fc-pg-barra" data-serie="pg" style={{ width: `${(100 * g.soma.meta) / maior * 0.86}%` }} />
              <span className="fc-pg-valor" data-serie="pg">
                {n(g.soma.meta)}
              </span>
            </span>
            <span className="fc-pg-par">
              <span className="fc-pg-barra" data-serie="realizado" style={{ width: `${(100 * g.soma.realizado) / maior * 0.86}%` }} />
              <span className="fc-pg-valor">{n(g.soma.realizado)}</span>
            </span>
          </span>
          <span className="cad-so-leitor">
            {g.nome}: PG {n(g.soma.meta)}, realizado {n(g.soma.realizado)} máquinas
          </span>
        </li>
      ))}
    </ol>
  );
}

/** A DISTRIBUIÇÃO DO REALIZADO: a rosca com o total no meio, os cinco que mais venderam, "Demais" e as vendas sem gestor. */
function DistribuicaoDoRealizado({
  grupos,
  total,
  semGestor,
  entidade,
}: {
  grupos: Grupo[];
  total: number;
  semGestor: number;
  entidade: string;
}) {
  if (total <= 0) return <BlocoVazio titulo="Nenhuma venda no mês" texto="O realizado é zero: não há o que distribuir." />;
  const vendeu = [...grupos].filter((g) => g.soma.realizado > 0).sort((a, b) => b.soma.realizado - a.soma.realizado);
  const fatias = [
    ...vendeu.slice(0, 5).map((g, i) => ({ nome: g.curto, inteiro: g.nome, valor: g.soma.realizado, cor: CORES_DA_DISTRIBUICAO[i] })),
    ...(vendeu.length > 5
      ? [{ nome: `Demais ${entidade}`, inteiro: `Demais ${entidade} (${vendeu.length - 5})`, valor: vendeu.slice(5).reduce((s, g) => s + g.soma.realizado, 0), cor: COR_DE_DEMAIS }]
      : []),
    ...(semGestor > 0 ? [{ nome: 'Sem gestor', inteiro: 'Vendas sem gestor no de-para', valor: semGestor, cor: COR_SEM_GESTOR }] : []),
  ];
  return (
    <div className="fc-distribuicao">
      <GraficoDonutCentro
        segmentos={fatias.map((f) => ({ valor: f.valor, cor: f.cor }))}
        largura={150}
        altura={150}
        cutout="62%"
        bordaBranca
        centro={{ linha1: n(total), linha2: 'máquinas', corLinha1: '#0B1638', tamanhoLinha1: total >= 1_000 ? 20 : 24, tamanhoLinha2: 12 }}
        tooltipUnidade="máquinas"
      />
      <table className="fc-distribuicao-legenda">
        <caption className="cad-so-leitor">Distribuição do realizado de {n(total)} máquinas</caption>
        <tbody>
          {fatias.map((f) => (
            <tr key={f.inteiro}>
              <th scope="row">
                <i style={{ background: f.cor }} aria-hidden="true" />
                <span aria-hidden="true">{f.nome}</span>
                <span className="cad-so-leitor">{f.inteiro}</span>
              </th>
              <td>{Math.round((100 * f.valor) / total)}%</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

const COLUNAS: { campo: Campo; rotulo: string }[] = [
  { campo: 'meta', rotulo: 'PG' },
  { campo: 'forecast', rotulo: 'Forecast' },
  { campo: 'bestGuess', rotulo: 'Best Guess' },
  { campo: 'realizado', rotulo: 'Realizado' },
  { campo: 'atingimento', rotulo: 'Atingimento' },
];

/**
 * A TABELA DA MAQUETE: os cinco primeiros um a um, com as linhas de produto (ou os gestores, na visão por linha) dentro, e
 * o resto em "Demais gestores"; o atingimento em barra; o total no fim. A ordem abre pelo maior PG.
 */
function TabelaDoForecast({
  grupos,
  total,
  visao,
  rotuloDoTotal,
  vendasSemGestor,
}: {
  grupos: Grupo[];
  total: Soma;
  visao: Visao;
  rotuloDoTotal: string;
  vendasSemGestor: number;
}) {
  const [ordem, setOrdem] = useState<{ campo: Campo; desc: boolean }>({ campo: 'meta', desc: true });
  const [abertos, setAbertos] = useState<Set<string> | null>(null);

  const ordenados = useMemo(() => {
    const valor = (g: Grupo): number | string => {
      switch (ordem.campo) {
        case 'nome':
          return g.nome;
        case 'atingimento':
          return atingimento(g.soma) ?? -1;
        case 'forecast':
          return g.soma.forecast ?? -1;
        case 'bestGuess':
          return g.soma.bestGuess ?? -1;
        default:
          return g.soma[ordem.campo];
      }
    };
    const lista = [...grupos].sort((a, b) => {
      const x = valor(a);
      const y = valor(b);
      return typeof x === 'string' || typeof y === 'string' ? String(x).localeCompare(String(y), 'pt-BR') : x - y;
    });
    return ordem.desc ? lista.reverse() : lista;
  }, [grupos, ordem]);

  // OS CINCO DA MAQUETE, um a um; o resto em "Demais" quando ele tem pelo menos dois — um só fica à vista.
  const aVista = ordenados.length > LINHAS_A_VISTA + 1 ? ordenados.slice(0, LINHAS_A_VISTA) : ordenados;
  const demais = ordenados.length > LINHAS_A_VISTA + 1 ? ordenados.slice(LINHAS_A_VISTA) : [];
  const abertosDeFato = abertos ?? new Set(aVista[0] ? [aVista[0].chave] : []);

  function alternar(chave: string) {
    const novo = new Set(abertosDeFato);
    if (novo.has(chave)) novo.delete(chave);
    else novo.add(chave);
    setAbertos(novo);
  }

  function ordenarPor(campo: Campo) {
    setOrdem((o) => (o.campo === campo ? { campo, desc: !o.desc } : { campo, desc: campo !== 'nome' }));
  }

  const rotuloDoNome = visao === 'gestor' ? 'Gestor / linha' : 'Linha / gestor';
  const somaDeDemais: Soma | null = demais.length > 0 ? somarSomas(demais.map((g) => g.soma)) : null;

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela fc-tabela">
        <caption className="cad-so-leitor">Forecast, best guess, PG e realizado de cada {visao === 'gestor' ? 'gestor' : 'linha'}</caption>
        <thead>
          <tr>
            <th scope="col" aria-sort={ordem.campo === 'nome' ? (ordem.desc ? 'descending' : 'ascending') : undefined}>
              <button type="button" className="cad-th-ordenar" onClick={() => ordenarPor('nome')}>
                {rotuloDoNome}
                <SetaDaOrdem ativa={ordem.campo === 'nome'} desc={ordem.desc} />
              </button>
            </th>
            {COLUNAS.map((c) => (
              <th key={c.campo} scope="col" className={c.campo === 'atingimento' ? 'fc-coluna-atingimento' : 'mom-num'} aria-sort={ordem.campo === c.campo ? (ordem.desc ? 'descending' : 'ascending') : undefined}>
                <button type="button" className="cad-th-ordenar" onClick={() => ordenarPor(c.campo)}>
                  {c.rotulo}
                  <SetaDaOrdem ativa={ordem.campo === c.campo} desc={ordem.desc} />
                </button>
              </th>
            ))}
          </tr>
        </thead>
        {aVista.map((g) => (
          <tbody key={g.chave} className="fc-grupo" data-aberto={abertosDeFato.has(g.chave) ? 'true' : 'false'}>
            <tr className="fc-soma">
              <th scope="rowgroup">
                <button
                  type="button"
                  className="fc-abrir"
                  aria-expanded={abertosDeFato.has(g.chave)}
                  aria-label={`${abertosDeFato.has(g.chave) ? 'Fechar' : 'Abrir'} as ${visao === 'gestor' ? 'linhas' : 'gestores'} de ${g.nome}`}
                  onClick={() => alternar(g.chave)}
                >
                  {abertosDeFato.has(g.chave) ? (
                    <ChevronDown size={16} strokeWidth={2.4} aria-hidden="true" />
                  ) : (
                    <ChevronRight size={16} strokeWidth={2.4} aria-hidden="true" />
                  )}
                </button>
                <span className="fc-nome">{g.nome}</span>
                {g.consultores !== null && (
                  <span className="fc-consultores">
                    {n(g.consultores)} {g.consultores === 1 ? 'consultor' : 'consultores'}
                  </span>
                )}
              </th>
              <Numeros soma={g.soma} forte />
            </tr>
            {abertosDeFato.has(g.chave) &&
              g.filhos.map((f) => (
                <tr key={f.chave} className="fc-filho">
                  <td className="fc-nome-do-filho">{f.nome}</td>
                  <Numeros soma={f.soma} />
                </tr>
              ))}
          </tbody>
        ))}
        {somaDeDemais && (
          <tbody className="fc-grupo fc-demais" data-aberto={abertosDeFato.has('__demais') ? 'true' : 'false'}>
            <tr className="fc-soma">
              <th scope="rowgroup">
                <button
                  type="button"
                  className="fc-abrir"
                  aria-expanded={abertosDeFato.has('__demais')}
                  aria-label={`${abertosDeFato.has('__demais') ? 'Fechar' : 'Abrir'} os demais ${visao === 'gestor' ? 'gestores' : 'linhas'}`}
                  onClick={() => alternar('__demais')}
                >
                  {abertosDeFato.has('__demais') ? (
                    <ChevronDown size={16} strokeWidth={2.4} aria-hidden="true" />
                  ) : (
                    <ChevronRight size={16} strokeWidth={2.4} aria-hidden="true" />
                  )}
                </button>
                <span className="fc-nome">
                  Demais {visao === 'gestor' ? 'gestores' : 'linhas'} ({n(demais.length)})
                </span>
              </th>
              <Numeros soma={somaDeDemais} forte />
            </tr>
            {abertosDeFato.has('__demais') &&
              demais.map((g) => (
                <tr key={g.chave} className="fc-filho">
                  <td className="fc-nome-do-filho">
                    {g.nome}
                    {g.consultores !== null && (
                      <span className="fc-consultores">
                        {n(g.consultores)} {g.consultores === 1 ? 'consultor' : 'consultores'}
                      </span>
                    )}
                  </td>
                  <Numeros soma={g.soma} />
                </tr>
              ))}
          </tbody>
        )}
        <tbody className="fc-total">
          <tr className="fc-soma">
            <th scope="rowgroup">
              <span className="fc-nome">{rotuloDoTotal}</span>
              {vendasSemGestor > 0 && <span className="fc-consultores">com {n(vendasSemGestor)} venda(s) sem gestor</span>}
            </th>
            <Numeros soma={total} forte total />
          </tr>
        </tbody>
      </table>
    </div>
  );
}

/** A soma de somas — "Demais gestores". */
function somarSomas(somas: Soma[]): Soma {
  return somar(somas.map((s, i) => ({ ...s, codigo: String(i), nome: '' })));
}

function SetaDaOrdem({ ativa, desc }: { ativa: boolean; desc: boolean }) {
  if (!ativa) return <ChevronsUpDown className="fc-seta-inativa" size={12} strokeWidth={2.2} aria-hidden="true" />;
  const Seta = desc ? ArrowDown : ArrowUp;
  return <Seta className="fc-seta-ativa" size={12} strokeWidth={2.6} aria-hidden="true" />;
}

function Numeros({ soma: l, forte = false, total = false }: { soma: Soma; forte?: boolean; total?: boolean }) {
  const razao = atingimento(l);
  return (
    <>
      <td className="mom-num">{n(l.meta)}</td>
      <td className="mom-num">{l.forecast === null ? <ValorAusente motivo={NAO_INFORMADO} oQue="o forecast" /> : n(l.forecast)}</td>
      <td className="mom-num">{l.bestGuess === null ? <ValorAusente motivo={NAO_INFORMADO} oQue="o best guess" /> : n(l.bestGuess)}</td>
      <td className="mom-num">{n(l.realizado)}</td>
      <td className="fc-coluna-atingimento">
        {razao === null ? (
          <ValorAusente motivo="Sem PG nesta linha: não há base para a razão." oQue="o atingimento" />
        ) : (
          <span className="fc-atingimento" data-forte={forte ? 'true' : undefined} data-total={total ? 'true' : undefined}>
            <span className="fc-atingimento-trilho" aria-hidden="true">
              <span style={{ width: `${Math.min(100, Math.round(razao * 100))}%` }} />
            </span>
            {doPo(l.realizado, l.meta)}
          </span>
        )}
      </td>
    </>
  );
}

function dataEHora(iso: string) {
  return new Date(iso).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}
