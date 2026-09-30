/**
 * Painel executivo da Visão 360 — o consolidado das filiais em operação, com dado real.
 *
 * OS CINCO CARTÕES (documento 36) vêm de `/relatorios/indicadores-executivos`, lido filial a filial e somado por
 * partição — cada número tem fonte, regra, período e alcance na dica ao lado do rótulo, e a composição filial a filial
 * abre logo abaixo deles:
 *   A. o faturamento do ano fiscal pelo ART — o valor de venda das máquinas ENTREGUES, com e sem comprador no CRM
 *      (decisão de 29/09/2026); a nota do Protheus fica como conferência, na dica e na composição;
 *   B. a meta de VENDA da API Gestão de Negócios × as máquinas vendidas (ART), no ano fiscal até o último mês fechado —
 *      de `/relatorios/metas`, lida filial a filial (#138) — e nunca previsão;
 *   C. clientes únicos (filial de cadastro) e vínculos (filial da carteira);
 *   D. cobertura pela cadência declarada da linha — a regra do mapa;
 *   E. vendas perdidas registradas — sem percentual de mercado.
 *
 * Quando falta dado, o cartão **fica na tela**, no mesmo lugar e no mesmo tamanho, com o traço e o motivo na dica —
 * some o número, não o cartão.
 *
 * ---------------------------------------------------------------------------
 * 28/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS (pedido do Ricardo: "a tela da Visão 360 precisa seguir o mesmo
 * padrão visual da de Indicadores Geográficos").
 *
 * A tela tinha a marcação do protótipo (`v360-kpi-row`, `v360-card`): cartões brancos com dois parágrafos de rodapé,
 * o período solto em cima, painéis de altura desencontrada e rankings em fonte monoespaçada. Agora ela usa as mesmas
 * peças dos Indicadores, na mesma ordem de leitura:
 *   1. o cabeçalho com o estado das filiais e o botão de reler;
 *   2. a barra de filtros — o período (ano fiscal) e o perfil;
 *   3. os cinco números de decisão (`CartaoDeIndicador`), curtos: o valor, uma linha de contexto e a regra na dica;
 *   4. a composição filial a filial, num painel que abre e fecha;
 *   5. três seções com título — Resultado e atenção, Carteira e cobertura, Clientes e mercado —, com os painéis do
 *      Momento do mercado (`PainelDoMomento`), os rankings com barra e as tabelas de faixa dos Indicadores.
 * NENHUM NÚMERO, REGRA OU TEXTO DE REGRA MUDOU: o que estava no corpo dos cartões foi para a dica deles, e as vendas
 * perdidas por motivo e por concorrente passaram a ser um painel com duas abas.
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DA MAQUETE DO RICARDO (`docs/prototipo/capturas-referencia/visao-360-maquete-2026-09-29.png`):
 * "deixe idêntico", sem inventar e sem tirar nada da tela. O desenho mora em `estilos/visao360.css`, embaixo de
 * `.v360-maquete`. O que a maquete mostra e o CRM não tem de onde tirar ficou de fora, e não inventado: a variação dos
 * clientes e do conhecimento de mercado (não há período anterior para essas contas), o "Ver todos os alertas" (não há
 * tela de alertas) e o menu "⋮" do faturamento. Os seletores de unidade e de ordem do ranking entram DESLIGADOS, com o
 * motivo na dica — o mesmo padrão dos Indicadores.
 */
import {
  ArrowDown,
  ArrowRight,
  ArrowUp,
  CalendarDays,
  ChartNoAxesCombined,
  ChartPie,
  CircleAlert,
  Clock3,
  Map as IconeDoMapa,
  RefreshCw,
  ShieldCheck,
  Target,
  TriangleAlert,
  Users,
  type LucideIcon,
} from 'lucide-react';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { TODAS_AS_FILIAIS } from '../../dados/api/acesso';
import {
  censConsolidados,
  mixDeLinhas,
  noRecorte,
  obterConsolidado,
  obterExecutivoConsolidado,
  obterExecutivoPorFilial,
  obterPerdasEFunilConsolidados,
  somarConsolidado,
  obterFaturamentoDoAno,
  somarFaturamentos,
  type FaturamentoSomado,
  type ExecutivoConsolidado,
  type ExecutivoDaFilial,
} from '../../dados/api/consolidado';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import {
  obterMetasConsolidadas,
  obterMetasPorFilial,
  type MetaDaFilial,
  type MetasConsolidadas,
} from '../../dados/api/metas';
import { useRecurso } from '../../dados/api/useRecurso';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { ValorAusente } from '../comum/ValorAusente';
import { SecaoDoPainel } from '../dashboard/Dashboard';
import { CartaoDeDecisao } from '../mercado/CartaoDeDecisao';
import { GraficoBarrasHorizontais } from '../GraficoBarrasHorizontais';
import { GraficoDonutCentro } from '../GraficoDonutCentro';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { InfoTooltip } from '../InfoTooltip';
import { PainelDoMomento, Seletor } from '../mercado/momento/pecas';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { TituloDaSecao } from '../territorio/TituloDaSecao';
import { FiltroDoPerfil } from './FiltroDoPerfil';
import type { PerfilId } from './perfis';
import '../../estilos/mercado-visao.css';
import '../../estilos/momento.css';
import '../../estilos/territorio.css';
import '../../estilos/painel-executivo.css';
import '../../estilos/visao360.css';

/**
 * COMO A META DE VENDA SE CONTA — a frase é uma só, no cartão e na composição (#138, decisões de 27/09/2026).
 *
 * A meta é a cota da API Gestão de Negócios, em máquinas; o realizado são as máquinas do ART que o CRM tem. As vendas
 * do ART que o CRM ainda não tem vêm à parte, em número, e o consórcio é em cotas — o realizado dele, desde 28/09/2026, são
 * as cotas vendidas da performance de consórcio da mesma API.
 */
const REGRA_DA_META =
  'Meta: a cota da API Gestão de Negócios, em máquinas, por consultor, linha, mês e filial. ' +
  'Realizado: as máquinas ENTREGUES que o CRM tem, lidas do ART, no mês da entrega — a mesma régua da Gestão de Negócios; a vendida e ainda não entregue vem à parte. Por consultor, conta o vendedor da venda. ' +
  'As vendas que aguardam na integração do ART (cadastro, chassi ou outro motivo) não entram no realizado e aparecem à parte. ' +
  'Consórcio: meta em cotas; o realizado são as cotas vendidas da performance de consórcio da API Gestão de Negócios, pelo mês que ela atribui — as da loja Digital não são de filial nenhuma e ficam fora. ' +
  'Período: o ano fiscal (novembro a outubro) até o último mês fechado, comparado com o mesmo trecho do ano fiscal anterior; o mês em curso vem à parte. ' +
  'Previsão: o CRM não calcula a sua; o forecast e o best guess dos gestores estão no relatório "Forecast da gerência".';

/** A permissão que falta, dita como a tela de perfis a chama. */
const SEM_PERMISSAO_DA_META =
  'Você não tem a permissão de ler a meta de venda (Meta.Ler). Quem administra o CRM concede pelo perfil.';

/**
 * O ANO DOS CARTÕES, na dica do filtro de período.
 *
 * O ano fiscal da Tracbel foi confirmado em 24/09/2026 — novembro a outubro, com o nome do ano em que termina — e virou
 * o período padrão das telas em 27/09/2026; o servidor apura pelo mesmo calendário.
 */
const DICA_DO_ANO_FISCAL =
  'Os cartões somam o ANO FISCAL da Tracbel, de novembro a outubro, com o nome do ano em que termina: o FY2026 vai de ' +
  'nov/2025 a out/2026. O ano em curso é somado até o ÚLTIMO MÊS FECHADO, como nos Indicadores Geográficos; o mês em ' +
  'curso, pela metade, fica à parte, na dica do cartão de faturamento. É o período padrão desde 27/09/2026.';

/**
 * O ano fiscal do ÚLTIMO MÊS FECHADO — o padrão (27/09/2026). Em novembro é o ano que acabou de fechar: o novo ainda não
 * tem mês fechado para somar.
 */
function anoFiscalDoUltimoMesFechado(hoje: Date): number {
  const ultimoFechado = new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1);
  return ultimoFechado.getMonth() + 1 >= 11 ? ultimoFechado.getFullYear() + 1 : ultimoFechado.getFullYear();
}

/** "FY2026 (nov/2025 a out/2026)" — o nome do ano fiscal nunca aparece sem o intervalo. */
function nomeDoAno(ano: number): string {
  return `FY${ano} (nov/${ano - 1} a out/${ano})`;
}

/**
 * O valor em milhões, como a diretoria fala dele.
 *
 * "R$ 15,9 M" cabe num cartão e num eixo de gráfico; "R$ 15.928.084,70" não cabe em nenhum dos dois e ninguém lê os
 * centavos de um total de filial.
 */
function emMilhoes(valor: number): string {
  if (Math.abs(valor) >= 1_000_000) {
    return `R$ ${(valor / 1_000_000).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} M`;
  }

  return `R$ ${(valor / 1_000).toLocaleString('pt-BR', { maximumFractionDigits: 0 })} mil`;
}

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/** `2026-09-01` vira `set/2026`. */
function mesPorExtenso(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${MESES[Number(mes) - 1]}/${ano}`;
}

/** `2026-09-01` vira `set/26` — o rótulo curto do eixo. */
function mesCurto(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${MESES[Number(mes) - 1]}/${ano.slice(2)}`;
}

/** `2026-09-08T18:46:07Z` vira `08/09 15:46`, no fuso de quem lê. */
function diaEHora(instante: string | null): string {
  if (!instante) return 'data não informada';
  const utc = /Z|[+-]\d\d:\d\d$/.test(instante) ? instante : `${instante}Z`;
  return new Date(utc).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
}

/** O instante vira `29/09/2026 15:58`, no fuso de quem lê — a "Última atualização" do cabeçalho. */
function diaEHoraCompletos(instante: Date): string {
  const dia = instante.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  const hora = instante.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
  return `${dia} ${hora}`;
}

/** `2026-03-10` vira `10/03/2026`. */
function data(dia: string | null): string {
  if (!dia) return '—';
  const [a, m, d] = dia.slice(0, 10).split('-');
  return `${d}/${m}/${a}`;
}

/** O primeiro ano do seletor: antes disso não há faturamento carregado. É o mesmo limite da API. */
const PRIMEIRO_ANO = 2020;

/**
 * A cor da classe na curva ABC.
 *
 * Verde escuro em A e cinza em D não é juízo sobre o cliente: é a mesma escala de intensidade que o resto do painel usa
 * para "pesa mais" e "pesa menos". D quer dizer "não comprou na janela", e é justamente quem a cobertura existe para
 * atacar.
 */
function corDaClasse(classe: string | null): string {
  switch (classe) {
    case 'A':
      return '#1B5E20';
    case 'B':
      return '#367C2B';
    case 'C':
      return '#6FBF5E';
    default:
      return '#9CA3AF';
  }
}

/**
 * As cores dos estados da cobertura pela cadência — verde coberto, vermelho pendente, como no mapa —, nos tons amostrados
 * da maquete de 29/09/2026. A linha sem cadência, fora do percentual, é o cinza-azulado dela.
 */
const COR_COBERTO = '#287B2E';
const COR_FORA_DA_CADENCIA = '#E93034';
const COR_NUNCA = '#7C1517';
const COR_SEM_CADENCIA = '#8090B4';

/** As cores do mix por linha, na paleta John Deere do protótipo; a primeira é o verde da rosca da maquete. */
const CORES_MIX = ['#29792D', '#4A9B3D', '#6FBF5E', '#FFDE00', '#F59E0B', '#0EA5E9', '#8B5CF6', '#DC2626'] as const;

/** As barras das vendas perdidas por motivo, na ordem da maquete: três verdes, amarelo, terracota e azul. */
const CORES_DAS_PERDAS = ['#2C7C30', '#5DB65B', '#A5D458', '#FDDB0B', '#CC784E', '#56ACFB'] as const;

/**
 * A LINHA DO FATURAMENTO NA MAQUETE: verde escuro, pontos cheios com borda branca, a área verde clara e a grade dos meses.
 * O mês em curso continua tracejado — na maquete, cinza-azulado e cheio.
 */
const APARENCIA_DO_FATURAMENTO = {
  corDaLinha: '#1E7A32',
  corDaArea: 'rgba(46, 139, 62, 0.17)',
  corDoParcial: '#6779A6',
  parcialCheio: true,
  larguraDaLinha: 2.4,
  raioDoPonto: 4.5,
  pontoComBorda: true,
  gradeVertical: true,
  tamanhoDaFonte: 11,
  corDaFonte: '#4B5563',
} as const;

/** As barras do Top CENs: o primeiro escuro, o segundo e o terceiro médios, os outros claros (maquete). */
function corDoCen(posicao: number): string {
  return posicao === 0 ? '#036B37' : posicao < 3 ? '#50B26B' : '#8BD89A';
}

/**
 * Tira o "Venda de " que abre quase todas as linhas de negócio do legado. O prefixo se repete em oito de nove rótulos —
 * os nomes inteiros ficam na dica do título do painel, que abre pelo ponteiro, pelo teclado e pelo toque.
 */
function semPrefixoDeVenda(nome: string): string {
  return nome.replace(/^Venda(s)? de\s+/i, '').replace(/^Venda(s)?\s+/i, '');
}

/** O protótipo mostra cinco no ranking. */
const TOP = 5;

/** Enquanto a leitura das perdas não volta — ou quando nenhuma filial respondeu. */
const SEM_VENDA_PERDIDA = { registradas: 0, processosPerdidos: 0, porMotivo: [], porConcorrente: [] };

const nº = (v: number) => v.toLocaleString('pt-BR');
const porcento = (v: number) => `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;

type AbaDasPerdas = 'motivo' | 'concorrente';

export function PainelExecutivo({
  perfil = 'diretoria',
  aoTrocarPerfil,
}: {
  perfil?: PerfilId;
  /** Trocar de perfil troca de tela — quem monta a Visão 360 decide. Sem ele, o filtro aparece desligado. */
  aoTrocarPerfil?: (p: PerfilId) => void;
} = {}) {
  const { contexto } = useContextoDeAcesso();
  const anoCorrente = anoFiscalDoUltimoMesFechado(new Date());
  const [ano, setAno] = useState(anoCorrente);
  const [abaDasPerdas, setAbaDasPerdas] = useState<AbaDasPerdas>('motivo');

  // A FILIAL DO SELETOR DO TOPO MANDA EM TODA LEITURA (30/09/2026, #313): trocar a filial — ou pôr "Todas as filiais" —
  // relê o painel inteiro, e cada painel é uma leitura só.
  const consolidado = useRecurso((sinal) => obterConsolidado(contexto, sinal), [contexto.usuario, contexto.empresa]);
  const executivo = useRecurso((sinal) => obterExecutivoConsolidado(contexto, ano, sinal), [contexto.usuario, contexto.empresa, ano]);
  // A META DE VENDA TEM LEITURA PRÓPRIA (#138): o período dela é o ano fiscal, e não o ano do seletor, e a falha dela não
  // derruba os outros cartões.
  const metas = useRecurso((sinal) => obterMetasConsolidadas(contexto, sinal), [contexto.usuario, contexto.empresa]);
  // AS PERDAS E O FUNIL DO ANO FISCAL (27/09/2026, documento 52): leitura própria, porque o período segue o ano escolhido.
  const perdasEFunil = useRecurso(
    (sinal) => obterPerdasEFunilConsolidados(contexto, ano, anoCorrente, sinal),
    [contexto.usuario, contexto.empresa, ano],
  );

  const dados = consolidado.dados;
  const total = useMemo(() => somarConsolidado(dados), [dados]);
  const mix = useMemo(() => mixDeLinhas(dados), [dados]);
  const cens = useMemo(() => censConsolidados(dados), [dados]);
  const pf = perdasEFunil.dados;
  const vendasPerdidas = pf?.vendas ?? SEM_VENDA_PERDIDA;
  const periodoDasPerdas = pf?.periodoTexto ?? nomeDoAno(ano);
  // O FATURAMENTO PELA NOTA SEGUE O ANO (30/09/2026, #313): leitura própria, e trocar o ano não relê o que é "hoje".
  const faturamentoDoAno = useRecurso((sinal) => obterFaturamentoDoAno(contexto, ano, sinal), [contexto.usuario, contexto.empresa, ano]);
  const faturamento = useMemo(() => somarFaturamentos(faturamentoDoAno.dados ? [faturamentoDoAno.dados] : []), [faturamentoDoAno.dados]);

  const ex = executivo.dados;
  const exComResposta = ex && ex.respondidas > 0 ? ex : null;
  // ONDE O NÚMERO FOI MEDIDO, para os textos que dizem "nenhum … em Barretos" ou "nas filiais que você alcança".
  const selecao = dados?.filiais[0]?.filial ?? ex?.filiais[0]?.filial ?? null;
  const recorte = noRecorte(selecao);
  const cobertura = ex?.cobertura ?? null;
  const coberturaPct = cobertura && cobertura.elegiveis > 0 ? (100 * cobertura.cobertos) / cobertura.elegiveis : null;

  /* A rosca de cobertura mostra os estados da CADÊNCIA — os mesmos do mapa —, e não faixas de dias. */
  const fatiasDaCobertura = useMemo(
    () =>
      cobertura
        ? [
            { nome: 'No prazo da cadência', valor: cobertura.cobertos, cor: COR_COBERTO },
            { nome: 'Fora da cadência', valor: cobertura.foraDaCadencia, cor: COR_FORA_DA_CADENCIA },
            { nome: 'Nunca contatados', valor: cobertura.nuncaContatados, cor: COR_NUNCA },
            { nome: 'Linha sem cadência (fora do %)', valor: cobertura.semCadencia, cor: COR_SEM_CADENCIA },
          ]
        : [],
    [cobertura],
  );

  const topCens = useMemo(() => cens.slice(0, TOP), [cens]);
  const maiorCen = topCens[0]?.clientes ?? 1;

  const mixDoGrafico = useMemo(() => mix.slice(0, TOP + 3), [mix]);
  const totalDoMix = mixDoGrafico.reduce((s, l) => s + l.clientes, 0);
  // O CENTRO DA ROSCA É A MAIOR LINHA, com o mesmo arredondamento da legenda — como a cobertura, que põe no centro a
  // primeira fatia. Com uma linha só, é o "100% da carteira" da maquete.
  const participacaoDaMaiorLinha =
    mixDoGrafico.length > 0 && totalDoMix > 0 ? Math.round((mixDoGrafico[0].clientes / totalDoMix) * 100) : null;

  /*
   * AS BARRAS SAEM DO FORMULÁRIO, E NÃO DO PROCESSO.
   *
   * O motivo mora no formulário de venda perdida (`IV_Q_VENDA_PERDIDA_FY25`), que está em `processo.VendaPerdida`. O
   * total do subtítulo continua sendo o de processos perdidos: é ele que dá a dimensão, e a diferença entre os dois
   * números — quantas derrotas ninguém registrou — é o que o rodapé do painel diz.
   */
  const barrasDePerda = useMemo(
    () =>
      vendasPerdidas.porMotivo.slice(0, 6).map((m, i) => ({
        rotulo: m.nome,
        valor: m.quantidade,
        cor: CORES_DAS_PERDAS[i % CORES_DAS_PERDAS.length],
        tooltipLinhas: [
          m.nome,
          `${nº(m.quantidade)} venda(s) perdida(s)`,
          m.diferencaMediaDePreco !== null
            ? `Nosso preço ficou R$ ${nº(Math.round(m.diferencaMediaDePreco))} acima, em média de ${m.comOsDoisPrecos}`
            : 'Sem os dois preços declarados',
        ],
      })),
    [vendasPerdidas],
  );
  const totalPerdido = vendasPerdidas.processosPerdidos;
  const maiorConcorrente = vendasPerdidas.porConcorrente[0]?.quantidade ?? 1;
  const semPerdaNenhuma =
    !perdasEFunil.carregando && barrasDePerda.length === 0 && vendasPerdidas.porConcorrente.length === 0;

  const anos = Array.from({ length: anoCorrente - PRIMEIRO_ANO + 1 }, (_, i) => anoCorrente - i);
  const lendo =
    consolidado.carregando || executivo.carregando || metas.carregando || perdasEFunil.carregando || faturamentoDoAno.carregando;
  const reler = () => {
    consolidado.recarregar();
    executivo.recarregar();
    metas.recarregar();
    perdasEFunil.recarregar();
    faturamentoDoAno.recarregar();
  };

  // A ÚLTIMA ATUALIZAÇÃO É A DESTA LEITURA DO PAINEL — a hora em que as leituras das filiais voltaram, e que o botão ao
  // lado refaz. Sem nenhuma filial respondendo, não houve atualização a datar.
  const [atualizadoEm, setAtualizadoEm] = useState<Date | null>(null);
  const comResposta = exComResposta !== null;
  useEffect(() => {
    if (!lendo && comResposta) setAtualizadoEm(new Date());
  }, [lendo, comResposta]);

  return (
    <>
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Visão 360</h1>
          <p className="page-subtitle">
            Painel executivo da Tracbel Agro — da <strong>filial escolhida no seletor do topo</strong>, ou de todas: resultado,
            carteira, cobertura e mercado.
          </p>
        </div>
        <p className="dash-atualizado">
          {ex ? (
            <span className="v360-filiais">
              <span className={ex.respondidas === 0 ? 'v360-periodo-alerta' : undefined}>
                {selecao?.codigo === TODAS_AS_FILIAIS ? 'Todas as filiais que você alcança' : `Filial: ${selecao?.nome ?? '—'}`}
              </span>
              <InfoTooltip texto={<FiliaisLidas ex={ex} />} rotulo="De onde vêm os números" />
            </span>
          ) : (
            'Lendo a filial…'
          )}
          {atualizadoEm && <span className="v360-atualizado-em">Última atualização: {diaEHoraCompletos(atualizadoEm)}</span>}
          <button
            type="button"
            className="dash-recarregar"
            onClick={reler}
            disabled={lendo}
            data-carregando={lendo ? 'true' : 'false'}
            aria-label="Reler o painel"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* O PERÍODO DOS CARTÕES, ESCRITO. O ano segue a escolha, e é o FISCAL (nov→out), o período padrão desde
          27/09/2026. O nome do ano aparece sempre com o intervalo ao lado: "FY2026" sozinho se lê como ano civil. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="periodo">
            <span className="dash-filtro-icone" aria-hidden="true">
              <CalendarDays size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Período · ano fiscal (nov–out)
                <InfoTooltip texto={DICA_DO_ANO_FISCAL} rotulo="Como o ano fiscal é contado" />
              </span>
              <select value={ano} onChange={(e) => setAno(Number(e.target.value))}>
                {anos.map((a) => (
                  <option key={a} value={a}>
                    {nomeDoAno(a)}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <FiltroDoPerfil perfil={perfil} aoTrocar={aoTrocarPerfil} />

          <div className="dash-filtros-acao">
            <Link to="/relatorios/territorio" className="dash-mais-filtros v360-botao-adr">
              <IconeDoMapa size={19} strokeWidth={2} aria-hidden="true" />
              Indicadores geográficos da ADR
              <ArrowRight size={15} strokeWidth={2.4} aria-hidden="true" />
            </Link>
          </div>
        </div>
      </div>

      {consolidado.erro && <BlocoErro erro={consolidado.erro} aoTentarDeNovo={consolidado.recarregar} />}
      {executivo.erro && <BlocoErro erro={executivo.erro} aoTentarDeNovo={executivo.recarregar} />}
      {ex && ex.respondidas === 0 && ex.filiais.length > 0 && (
        <BlocoErro
          erro={ex.filiais.find((f) => f.erro)?.erro ?? new Error('Nenhuma filial respondeu.')}
          aoTentarDeNovo={executivo.recarregar}
        />
      )}

      <CincoIndicadores ano={ano} ex={exComResposta} carregando={executivo.carregando} metas={metas.dados} carregandoMetas={metas.carregando} />

      {exComResposta && <ComposicaoDosIndicadores ex={exComResposta} metas={metas.dados} />}

      {consolidado.carregando ? (
        <BlocoCarregando oQue="o consolidado das filiais" />
      ) : (
        <>
          {/* RESULTADO E ATENÇÃO ------------------------------------------------ */}
          <SecaoDoPainel bloco="secao-resultado">
            <TituloDaSecao
              titulo="Resultado e atenção"
              subtitulo="O faturamento mês a mês do período escolhido e o que pede ação hoje."
            />
            <div className="v360-linha" data-bloco="linha-2" data-variante="resultado">
              <PainelDoFaturamentoMensal ex={exComResposta} carregando={executivo.carregando} faturamento={faturamento} />

              <PainelDoMomento
                titulo="Alertas gerenciais"
                dica={`O que pede ação hoje, ${recorte}: vínculos elegíveis que nunca tiveram contato, tarefas atrasadas e processos parados no funil do Vórtice.`}
                subtitulo="Hoje, para atenção neste perfil."
              >
                <div className="v360-alertas-list">
                  {cobertura && (
                    <Alerta
                      tipo="critico"
                      icone={TriangleAlert}
                      titulo={`${nº(cobertura.nuncaContatados)} vínculos elegíveis nunca contatados`}
                      detalhe={`De ${nº(cobertura.elegiveis)} vínculos em linha com cadência declarada`}
                      acao="Ver cobertura"
                      para="/cobertura"
                    />
                  )}
                  <Alerta
                    tipo="aviso"
                    icone={Clock3}
                    titulo={`${nº(total.atrasadas)} tarefas atrasadas`}
                    detalhe={`De ${nº(total.pendentes)} pendentes no total`}
                    acao="Ver agenda"
                    para="/agenda"
                  />
                  {/* PARADOS EM NEGOCIAÇÃO OU PEDIDO (27/09/2026, documento 52 §8) — pelo estágio mais avançado do
                      processo, ainda aberto no Vórtice, alcançado há mais de 60 dias. Sem funil, o número vira "—" e o
                      motivo verdadeiro. */}
                  {pf?.parados != null ? (
                    <Alerta
                      tipo="atencao"
                      icone={CircleAlert}
                      titulo={`${nº(pf.parados)} processos parados em Negociação ou Pedido há mais de ${pf.diasParaParado} dias`}
                      detalhe={`O estágio mais avançado, ainda aberto no Vórtice, sem avançar nem encerrar — hoje, ${recorte}`}
                      acao="Ver funil"
                      para="/relatorios/funil"
                    />
                  ) : (
                    <Alerta
                      tipo="info"
                      icone={CircleAlert}
                      titulo={
                        <>
                          Processos parados em Negociação ou Pedido:{' '}
                          <ValorAusente
                            oQue="os processos parados"
                            motivo={
                              perdasEFunil.carregando
                                ? 'Lendo o funil das filiais.'
                                : (pf?.motivoSemFunil ?? 'A leitura do funil das filiais não respondeu.')
                            }
                          />
                        </>
                      }
                      detalhe="Pelo estágio mais avançado de cada processo do Vórtice, ainda aberto há mais de 60 dias"
                      acao="Ver funil"
                      para="/relatorios/funil"
                    />
                  )}
                </div>
              </PainelDoMomento>
            </div>
          </SecaoDoPainel>

          {/* CARTEIRA E COBERTURA ----------------------------------------------- */}
          <SecaoDoPainel bloco="secao-carteira">
            <TituloDaSecao
              titulo="Carteira e cobertura"
              subtitulo="Quem a Tracbel acompanha, com que frequência, e em que linhas."
            />
            <div className="v360-linha" data-bloco="linha-3" data-variante="tres">
              <PainelDoMomento
                titulo="Status da cobertura"
                dica="Os vínculos em carteira comercial pela cadência declarada da linha de negócio e pela classe ABC do cliente — a mesma regra do mapa de cobertura. Linha sem cadência declarada fica fora do percentual."
                subtitulo={
                  cobertura
                    ? `Hoje: ${nº(cobertura.vinculosComerciais)} vínculos em carteira comercial, pela cadência da linha.`
                    : 'Hoje, pela cadência declarada da linha de negócio.'
                }
                direita={
                  <Link to="/relatorios/territorio" className="v360-link v360-botao-contorno">
                    Ver no mapa
                    <ArrowRight size={13} strokeWidth={2.4} aria-hidden="true" />
                  </Link>
                }
              >
                {/* SEM VÍNCULO, SEM ROSCA: com tudo em zero ela desenhava um anel vazio, um "—" no meio e quatro
                    zeros na legenda — quatro afirmações para dizer uma coisa só. */}
                {cobertura && cobertura.vinculosComerciais === 0 ? (
                  <SemDado oQue="a cobertura" porque={`Nenhum vínculo em carteira comercial ${recorte}.`} />
                ) : cobertura ? (
                  <div className="v360-rosca-e-tabela">
                    <GraficoDonutCentro
                      segmentos={fatiasDaCobertura}
                      largura={124}
                      altura={124}
                      cutout="74%"
                      bordaBranca
                      centro={{
                        linha1: coberturaPct === null ? '—' : porcento(coberturaPct),
                        linha2: 'no prazo',
                        corLinha1: '#0B1638',
                        tamanhoLinha1: 21,
                        tamanhoLinha2: 11,
                        deslocamentoLinha2: 18,
                      }}
                    />
                    <TabelaDeFaixas
                      itens={fatiasDaCobertura.map((f) => ({ nome: f.nome, cor: f.cor, valor: nº(f.valor) }))}
                      coluna="Vínculos"
                    />
                  </div>
                ) : (
                  <SemDado oQue="a cobertura" porque="A leitura dos indicadores das filiais não respondeu." />
                )}
              </PainelDoMomento>

              <PainelDoMomento
                titulo="Top CENs"
                dica={`Os responsáveis de carteira comercial com mais vínculos ${recorte}, e quantos desses vínculos tiveram contato nos últimos 30 dias.`}
                subtitulo="Hoje: ranking por vínculos em carteira comercial."
                direita={
                  topCens.length > 0 ? (
                    <Seletor
                      rotulo="Ordem do ranking"
                      rotuloVisivel={false}
                      valor="vinculos"
                      opcoes={[{ id: 'vinculos', rotulo: 'Vínculos' }]}
                      motivoDesligado="O ranking é por vínculos em carteira comercial. As carteiras e os vínculos com contato em 30 dias de cada responsável estão embaixo do nome dele."
                    />
                  ) : undefined
                }
              >
                {/* SEM CARTEIRA, O PAINEL DIZ POR QUÊ — como os vizinhos. */}
                {topCens.length === 0 ? (
                  <SemDado
                    oQue="o ranking de CENs"
                    porque={`Nenhum vínculo em carteira comercial de pessoa ou área ${recorte}.`}
                  />
                ) : (
                  <ol className="mom-ranking v360-ranking" data-numerado="true" data-variante="cens">
                    <li className="mom-ranking-linha mom-ranking-cabecalho" aria-hidden="true">
                      <span className="mom-ranking-posicao">#</span>
                      <span>Responsável</span>
                      <span />
                      <span className="mom-num">Vínculos</span>
                    </li>
                    {topCens.map((c, i) => (
                      <li key={c.nome} className="mom-ranking-linha" data-cen={c.nome}>
                        <span className="mom-ranking-posicao" aria-hidden="true">
                          {i + 1}
                        </span>
                        <span className="v360-ranking-nome">
                          <span className="v360-ranking-titulo">{c.nome}</span>
                          <span className="v360-ranking-meta">
                            {nº(c.carteiras)} {c.carteiras === 1 ? 'carteira' : 'carteiras'} · {nº(c.em30)} com contato em 30 dias
                          </span>
                        </span>
                        <span className="mom-barra" aria-hidden="true">
                          <span
                            style={{
                              width: `${Math.max(2, Math.round((c.clientes / maiorCen) * 100))}%`,
                              background: corDoCen(i),
                            }}
                          />
                        </span>
                        <span className="mom-ranking-valor">{nº(c.clientes)}</span>
                      </li>
                    ))}
                  </ol>
                )}
              </PainelDoMomento>

              <PainelDoMomento
                titulo="Mix por linha"
                dica={
                  <>
                    <p>
                      A parte de cada linha de negócio nos vínculos das carteiras comerciais. Carteira administrativa e de
                      teste ficam de fora: são depósito de cadastro, e não carteira de ninguém.
                    </p>
                    {mix.length > 0 && (
                      <p>Nomes completos, na ordem da legenda: {mixDoGrafico.map((l) => l.nome).join('; ')}.</p>
                    )}
                  </>
                }
                subtitulo="Hoje: participação de cada linha nos vínculos das carteiras comerciais."
              >
                {mix.length > 0 ? (
                  <div className="v360-rosca-e-tabela">
                    <GraficoDonutCentro
                      segmentos={mixDoGrafico.map((l, i) => ({ valor: l.clientes, cor: CORES_MIX[i % CORES_MIX.length] }))}
                      largura={118}
                      altura={118}
                      cutout="78%"
                      bordaBranca
                      tooltipUnidade="%"
                      centro={
                        participacaoDaMaiorLinha === null
                          ? undefined
                          : {
                              linha1: `${participacaoDaMaiorLinha}%`,
                              linha2: 'da carteira',
                              corLinha1: '#0B1638',
                              tamanhoLinha1: 20,
                              tamanhoLinha2: 11,
                              deslocamentoLinha2: 18,
                            }
                      }
                    />
                    <TabelaDeFaixas
                      className="v360-mix-legenda"
                      coluna="Participação"
                      itens={mixDoGrafico.map((l, i) => ({
                        nome: semPrefixoDeVenda(l.nome),
                        cor: CORES_MIX[i % CORES_MIX.length],
                        valor: `${totalDoMix > 0 ? Math.round((l.clientes / totalDoMix) * 100) : 0}%`,
                      }))}
                    />
                  </div>
                ) : (
                  <SemDado oQue="o mix por linha" porque={`Nenhum vínculo em carteira comercial ${recorte}.`} />
                )}
              </PainelDoMomento>
            </div>
          </SecaoDoPainel>

          {/* CLIENTES E MERCADO ------------------------------------------------- */}
          <SecaoDoPainel bloco="secao-mercado">
            <TituloDaSecao
              titulo="Clientes e mercado"
              subtitulo="Os clientes que mais compram e as vendas que a Tracbel perdeu, e para quem."
              metodologia={
                <>
                  <p>
                    Os clientes são ordenados pelo faturamento com cliente no CRM no ano fiscal escolhido; a classe é a da curva ABC
                    apurada do mesmo faturamento.
                  </p>
                  <p>
                    As vendas perdidas vêm do formulário de venda perdida do CEN, pela venda perdida principal — a mesma
                    perda registrada em dois formulários conta uma vez —, no ano fiscal escolhido. Este bloco conta
                    derrotas registradas, e não o tamanho do mercado: a Captura Tracbel é medida nos Indicadores
                    Geográficos.
                  </p>
                </>
              }
            />
            <div className="v360-linha" data-bloco="linha-4" data-variante="mercado">
              <PainelDoMomento
                titulo="Top 5 clientes"
                dica={`Maior faturamento pela nota do Protheus no ${nomeDoAno(ano)} — o ano escolhido no período —, com a classe da curva ABC e o mês da última compra no ano.`}
                subtitulo={`Maior faturamento no ${nomeDoAno(ano)} · classe da curva ABC.`}
              >
                {faturamento.topClientes.length > 0 ? (
                  <ol className="mom-ranking v360-ranking" data-numerado="true" data-variante="clientes">
                    <li className="mom-ranking-linha mom-ranking-cabecalho" aria-hidden="true">
                      <span className="mom-ranking-posicao">#</span>
                      <span>Cliente</span>
                      <span>ABC</span>
                      <span className="mom-num">Faturamento</span>
                    </li>
                    {faturamento.topClientes.map((cliente, i) => (
                      <li className="mom-ranking-linha" key={cliente.clienteChave}>
                        <span className="mom-ranking-posicao" aria-hidden="true">
                          {i + 1}
                        </span>
                        <span className="v360-ranking-nome">
                          {/* O NOME CORTADO COM RETICÊNCIAS ABRE O NOME INTEIRO (issue 167): razão social de cooperativa
                              é justamente o nome que não cabe. */}
                          <InfoTooltip
                            texto={`${cliente.nome} — ${cliente.classe ? `classe ${cliente.classe}` : 'classe não apurada'} na curva ABC.`}
                            rotulo={`${cliente.nome}, ${cliente.classe ? `classe ${cliente.classe}` : 'classe não apurada'}`}
                          >
                            <span className="v360-ranking-titulo">{cliente.nome}</span>
                          </InfoTooltip>
                          <span className="v360-ranking-meta">
                            {cliente.ultimaCompraEm
                              ? `última compra em ${mesPorExtenso(cliente.ultimaCompraEm)}`
                              : 'sem compra na janela'}
                          </span>
                        </span>
                        {/* A CLASSE É DECORAÇÃO COM ENDEREÇO: por extenso, ela está na dica do nome. */}
                        <span className="v360-classe" style={{ background: corDaClasse(cliente.classe) }} aria-hidden="true">
                          {cliente.classe ?? '—'}
                        </span>
                        <span className="mom-ranking-valor">{emMilhoes(cliente.valorLiquido)}</span>
                      </li>
                    ))}
                  </ol>
                ) : (
                  <SemDado
                    oQue="o ranking de clientes"
                    porque={
                      faturamentoDoAno.carregando
                        ? `Lendo o faturamento do ${nomeDoAno(ano)}…`
                        : `Nenhum faturamento com cliente no CRM no ${nomeDoAno(ano)} ${recorte}.`
                    }
                  />
                )}
              </PainelDoMomento>

              <PainelDoMomento
                titulo="Vendas perdidas"
                dica="Pelo formulário de venda perdida do CEN, no ano fiscal escolhido. Por motivo: o que o CEN declarou. Para quem perdemos: pela venda perdida principal — a mesma perda em dois formulários conta uma vez."
                subtitulo={`${nº(totalPerdido)} processos do Vórtice perdidos · ${periodoDasPerdas}`}
                area="v360-perdas"
                direita={
                  // AS ABAS NA LINHA DO TÍTULO (maquete de 29/09/2026). SEM PERDA NENHUMA, SEM ABAS: duas abas que
                  // mostram o mesmo vazio são um controle que não controla nada.
                  semPerdaNenhuma ? undefined : (
                    <div className="terr-alternador v360-abas" role="group" aria-label="Vendas perdidas">
                      <button type="button" aria-pressed={abaDasPerdas === 'motivo'} onClick={() => setAbaDasPerdas('motivo')}>
                        Por motivo
                      </button>
                      <button
                        type="button"
                        aria-pressed={abaDasPerdas === 'concorrente'}
                        onClick={() => setAbaDasPerdas('concorrente')}
                      >
                        Para quem perdemos
                      </button>
                    </div>
                  )
                }
              >
                {semPerdaNenhuma ? (
                  <SemDado
                    oQue="as vendas perdidas"
                    porque={
                      pf && pf.respondidas === 0
                        ? 'A leitura das vendas perdidas das filiais não respondeu.'
                        : totalPerdido > 0
                          ? `Os ${nº(totalPerdido)} processos perdidos em ${periodoDasPerdas} existem, e nenhum deles tem o formulário de venda perdida preenchido no período.`
                          : (pf?.motivoSemFunil ??
                            `Nenhum processo perdido nem venda perdida registrada em ${periodoDasPerdas}, ${recorte}.`)
                    }
                  />
                ) : perdasEFunil.carregando ? (
                  <BlocoCarregando oQue="as vendas perdidas do período" />
                ) : abaDasPerdas === 'motivo' ? (
                  barrasDePerda.length > 0 ? (
                    <>
                      <MolduraDeGrafico altura={Math.max(120, barrasDePerda.length * 19 + 32)} preencher>
                        {(l, a) => (
                          <GraficoBarrasHorizontais
                            itens={barrasDePerda}
                            largura={l}
                            altura={a}
                            valoresNaPonta
                            espessura={11}
                            tamanhoDoRotulo={11.5}
                            corDoRotulo="#1F2937"
                          />
                        )}
                      </MolduraDeGrafico>
                      <p className="v360-nota">
                        De <strong>{nº(vendasPerdidas.registradas)}</strong> derrotas com formulário preenchido. As outras{' '}
                        <strong>{nº(Math.max(0, totalPerdido - vendasPerdidas.registradas))}</strong> foram encerradas sem
                        ninguém registrar o motivo.
                      </p>
                    </>
                  ) : (
                    <SemDado
                      oQue="o motivo da perda"
                      // "OS 0 PROCESSOS PERDIDOS EXISTEM" não é frase: sem processo perdido, o que falta não é o
                      // formulário, é a perda.
                      porque={
                        pf && pf.respondidas === 0
                          ? 'A leitura das vendas perdidas das filiais não respondeu.'
                          : totalPerdido > 0
                            ? `Os ${nº(totalPerdido)} processos perdidos em ${periodoDasPerdas} existem, e nenhum deles tem o formulário de venda perdida preenchido no período.`
                            : (pf?.motivoSemFunil ??
                              `Nenhum processo perdido nem venda perdida registrada em ${periodoDasPerdas}, ${recorte}.`)
                      }
                    />
                  )
                ) : vendasPerdidas.porConcorrente.length > 0 ? (
                  <>
                    <p className="v360-nota">Pela venda perdida principal · {periodoDasPerdas}</p>
                    <ol className="mom-ranking v360-ranking" data-variante="concorrentes">
                      {vendasPerdidas.porConcorrente.slice(0, TOP).map((c) => (
                        <li key={c.codigo} className="mom-ranking-linha">
                          <span className="mom-ranking-nome">{c.nome}</span>
                          <span className="mom-barra" aria-hidden="true">
                            <span style={{ width: `${Math.max(2, (c.quantidade / maiorConcorrente) * 100)}%`, background: '#B45309' }} />
                          </span>
                          <span className="mom-ranking-valor">
                            {nº(c.quantidade)} {c.quantidade === 1 ? 'perda' : 'perdas'} · {nº(c.maquinas)} máq.
                          </span>
                        </li>
                      ))}
                    </ol>
                    {/* CAPTURA TRACBEL, E NÃO "PARTICIPAÇÃO DE MERCADO" (issue 162). Este painel conta derrotas; a parte da
                        demanda que a Tracbel leva é medida nos Indicadores, com as unidades do ART. */}
                    <p className="v360-nota">
                      {nº(vendasPerdidas.registradas)} formulários de {nº(vendasPerdidas.processosPerdidos)} processos perdidos
                      no período. A <strong>Captura Tracbel</strong> — máquinas vendidas sobre a demanda estimada — é medida
                      nos{' '}
                      <Link to="/relatorios/territorio" className="v360-link">
                        Indicadores geográficos
                      </Link>
                      .
                    </p>
                  </>
                ) : (
                  <SemDado
                    oQue="o mercado"
                    porque={
                      pf && pf.respondidas === 0
                        ? 'A leitura das vendas perdidas das filiais não respondeu.'
                        : `Nenhuma venda perdida com concorrente registrada no formulário do CEN em ${periodoDasPerdas}, ${recorte}.`
                    }
                  />
                )}
              </PainelDoMomento>
            </div>
          </SecaoDoPainel>
        </>
      )}
    </>
  );
}

/* ------------------------------------------------------------------------ */

/** A tabela de faixas da Percepção comercial: a bolinha, o nome e o número. */
function TabelaDeFaixas({
  itens,
  coluna,
  className,
}: {
  itens: readonly { nome: string; cor: string; valor: string }[];
  coluna: string;
  className?: string;
}) {
  return (
    <table className={className ? `mom-faixas ${className}` : 'mom-faixas'}>
      <thead>
        <tr>
          <th scope="col">
            <span className="cad-so-leitor">Faixa</span>
          </th>
          <th scope="col" className="mom-num">
            {coluna}
          </th>
        </tr>
      </thead>
      <tbody>
        {itens.map((i) => (
          <tr key={i.nome}>
            <th scope="row">
              <span className="mom-ponto" style={{ '--mom-cor': i.cor } as React.CSSProperties} aria-hidden="true" />
              {i.nome}
            </th>
            <td className="mom-num">
              <strong>{i.valor}</strong>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

type FonteDoFaturamento = 'art' | 'nota';

const FONTES_DO_FATURAMENTO: readonly { id: FonteDoFaturamento; rotulo: string }[] = [
  { id: 'art', rotulo: 'ART · máquinas entregues' },
  { id: 'nota', rotulo: 'Protheus · notas fiscais' },
];

const DICA_DO_FATURAMENTO_ART =
  'O valor de venda do ART das máquinas ENTREGUES, pelo mês da data de entrega, somado filial a filial — a mesma régua ' +
  'do cartão do faturamento do ano, com e sem comprador no CRM. A máquina sem valor de venda conta nas máquinas, e não ' +
  'no valor. O mês em curso sai tracejado: ele não caiu, está pela metade. No balão de cada mês ficam as máquinas ' +
  'entregues e, como conferência, a nota fiscal do Protheus do mesmo mês (máquina, peça e serviço) — as duas medem ' +
  'caminhos diferentes e nunca se somam. O seletor ao lado troca o gráfico para a nota do Protheus.';

const DICA_DO_FATURAMENTO_NOTA =
  'Nota fiscal de saída de venda, lida do Protheus, das notas com cliente no CRM, somada filial a filial — máquina, peça e ' +
  'serviço. O último mês, quando ainda está correndo, sai tracejado: ele não caiu, está pela metade. O faturamento de ' +
  'máquina do CRM é o ART, no outro modo do seletor; a nota é a conferência e o faturamento de peça e serviço.';

/**
 * O FATURAMENTO — 12 MESES, pelo ART (pedido do Ricardo, 29/09/2026: "conseguimos pôr faturamento por mês ali no 360?
 * Pegamos do ART na coluna entregue e valor").
 *
 * ABRE NO ART: o valor de venda das máquinas entregues, mês a mês, na régua do cartão do ano — os doze meses que terminam
 * no mês em curso. A NOTA DO PROTHEUS, que era este gráfico até aqui, não sai da tela: está no balão de cada mês, como
 * conferência, e no seletor do canto, que troca o gráfico para ela — o seletor "R$ (Milhões)", que não tinha o que
 * escolher, virou a escolha da fonte.
 */
function PainelDoFaturamentoMensal({
  ex,
  carregando,
  faturamento,
}: {
  ex: ExecutivoConsolidado | null;
  carregando: boolean;
  faturamento: FaturamentoSomado;
}) {
  const [fonte, setFonte] = useState<FonteDoFaturamento>('art');
  const serie = ex?.entregues.porMes ?? null;
  const notaDoMes = useMemo(
    () => new Map(faturamento.serie.map((m) => [m.competencia.slice(0, 7), m.valorLiquido])),
    [faturamento.serie],
  );

  const seletor = (
    <Seletor
      rotulo="Fonte do faturamento"
      rotuloVisivel={false}
      valor={fonte}
      opcoes={FONTES_DO_FATURAMENTO}
      aoMudar={setFonte}
    />
  );

  if (fonte === 'nota') {
    return (
      <PainelDoMomento
        titulo="Faturamento — 12 meses"
        data-fonte="nota"
        dica={DICA_DO_FATURAMENTO_NOTA}
        subtitulo={
          faturamento.serie.length > 0
            ? `${mesPorExtenso(faturamento.serie[0].competencia)} a ${mesPorExtenso(faturamento.competenciaMaisRecente!)} · notas com cliente no CRM`
            : 'Nota fiscal de saída, lida do Protheus.'
        }
        direita={seletor}
      >
        {/* O SUBTÍTULO ESCREVE O PERÍODO, SEMPRE. Se a carga do ERP parar de novo, a série para de avançar e o período
            denuncia — em vez de mostrar um total plausível e velho. */}
        {faturamento.serie.length > 0 ? (
          <MolduraDeGrafico altura={150} preencher>
            {(l, a) => (
              <GraficoLinhaMensal
                rotulos={faturamento.serie.map((m) => mesCurto(m.competencia))}
                valores={faturamento.serie.map((m) => m.valorLiquido)}
                largura={l}
                altura={a}
                formatar={emMilhoes}
                ultimoParcial={faturamento.ultimoMesEstaAberto}
                aparencia={APARENCIA_DO_FATURAMENTO}
              />
            )}
          </MolduraDeGrafico>
        ) : (
          <SemDado
            oQue="o faturamento"
            porque="Nenhuma nota carregada. A carga lê a SD2 do Protheus; se ela não rodou com a ponte configurada, não há série para desenhar."
          />
        )}
      </PainelDoMomento>
    );
  }

  // O QUE FALTA DIZ POR QUÊ: a leitura correndo, o servidor sem a série, nenhuma entrega na janela, ou as entregas ainda
  // sem valor de venda — nunca um gráfico em zero.
  const maquinas = serie?.reduce((s, m) => s + m.maquinas, 0) ?? 0;
  const comValor = serie?.some((m) => m.valor > 0) ?? false;
  const periodo = serie && serie.length > 0 ? `${mesPorExtenso(serie[0].inicio)} a ${mesPorExtenso(serie.at(-1)!.inicio)}` : null;
  const porque = !ex
    ? 'A leitura dos indicadores das filiais não respondeu.'
    : !serie
      ? 'A leitura não trouxe o faturamento mês a mês pelo ART: o servidor ainda não tem essa versão. A nota do Protheus está no seletor ao lado.'
      : maquinas === 0
        ? `Nenhuma máquina com entrega de ${periodo} no ART ${noRecorte(ex.filiais[0]?.filial)}.`
        : !comValor
          ? `As ${nº(maquinas)} máquinas entregues de ${periodo} ainda estão sem valor de venda: o valor do ART chega na primeira leitura do ART depois da publicação.`
          : null;

  return (
    <PainelDoMomento
      titulo="Faturamento — 12 meses"
      data-fonte="art"
      dica={DICA_DO_FATURAMENTO_ART}
      subtitulo={periodo ? `${periodo} · máquinas entregues, pelo ART` : 'Máquinas entregues, pelo ART.'}
      direita={seletor}
    >
      {carregando ? (
        <BlocoCarregando oQue="o faturamento mês a mês" />
      ) : porque !== null || !serie ? (
        <SemDado oQue="o faturamento mês a mês" porque={porque ?? ''} />
      ) : (
        <MolduraDeGrafico altura={150} preencher>
          {(l, a) => (
            <GraficoLinhaMensal
              rotulos={serie.map((m) => mesCurto(m.inicio))}
              valores={serie.map((m) => m.valor)}
              largura={l}
              altura={a}
              formatar={emMilhoes}
              // O ÚLTIMO É O MÊS EM CURSO, sempre: a série termina nele.
              ultimoParcial
              aparencia={APARENCIA_DO_FATURAMENTO}
              linhasDoBalao={(i) => {
                const mes = serie[i];
                const nota = notaDoMes.get(mes.inicio.slice(0, 7));
                return [
                  `${nº(mes.maquinas)} ${mes.maquinas === 1 ? 'máquina entregue' : 'máquinas entregues'}${mes.semValor > 0 ? ` (${nº(mes.semValor)} sem valor de venda)` : ''}`,
                  nota !== undefined
                    ? `NF do Protheus no mês, conferência: ${emMilhoes(nota)}`
                    : 'NF do Protheus no mês: sem nota carregada',
                ];
              }}
            />
          )}
        </MolduraDeGrafico>
      )}
    </PainelDoMomento>
  );
}

/**
 * A. O FATURAMENTO DO ANO PELO ART (decisão do Ricardo de 29/09/2026): "o faturamento real do ano fiscal vem do ART, das
 * máquinas entregues" — vendida é ENTREGUE, a data de entrega preenchida.
 *
 * O NÚMERO É O VALOR DE VENDA DO ART das máquinas com a entrega nos meses do ano — até o último mês fechado, como o resto
 * do painel —, com e sem comprador no CRM: o ART inteiro, que é o que a Gestão de Negócios conta. A quantidade vai na linha
 * de baixo. A nota de saída do Protheus, que era este cartão até 28/09, fica na dica e na composição como conferência, e
 * não se soma: as duas medem a mesma máquina por caminhos diferentes.
 *
 * A MÁQUINA SEM VALOR NÃO VIRA ZERO: ela conta nas máquinas e fica fora do valor, dita. Quando nenhuma tem valor — o
 * intervalo entre a publicação e a primeira leitura do ART —, o valor é o traço com o motivo.
 */
function CartaoDoFaturamento({ ex }: { ex: ExecutivoConsolidado }) {
  const { ano, anterior, mesEmCurso } = ex.entregues;
  const nota = ex.faturamentoDoMes;

  const nenhumComValor = ano !== null && ano.maquinas > 0 && ano.semValor === ano.maquinas;
  const valor = ano && ano.maquinas > 0 && !nenhumComValor ? emMilhoes(ano.valor) : null;
  const motivo = !ano
    ? 'A leitura não trouxe o faturamento pelo ART: o servidor ainda não tem a versão de 29/09/2026.'
    : ano.maquinas === 0
      ? `Nenhuma máquina com entrega de ${mesPorExtenso(ano.inicio)} a ${mesPorExtenso(ano.fim)} no ART ${noRecorte(ex.filiais[0]?.filial)}.`
      : `As ${nº(ano.maquinas)} máquinas entregues ainda estão sem valor de venda: o valor do ART passou a ser lido em 29/09/2026 e chega na primeira leitura do ART depois da publicação.`;

  // A VARIAÇÃO COMPARA O VALOR COM ELE MESMO, no mesmo trecho do ano anterior; sem valor de antes, não há percentual.
  const variacao = ano && anterior && anterior.valor > 0 && !nenhumComValor ? (100 * (ano.valor - anterior.valor)) / anterior.valor : null;
  const ressalvas = ano
    ? [
        ano.aguardandoNoCrm > 0
          ? `${nº(ano.aguardandoNoCrm)} ainda não viraram venda no CRM (comprador sem cadastro, chassi ou outro motivo da integração) e contam aqui — o realizado da meta conta só as do CRM`
          : null,
        ano.semValor > 0 && !nenhumComValor ? `${nº(ano.semValor)} sem valor de venda no ART contam nas máquinas, e não no valor` : null,
      ].filter((r): r is string => r !== null)
    : [];

  return (
    <CartaoDeDecisao
      rotulo={`Faturamento FY${ex.ano}`}
      oQue="o faturamento do ano"
      icone={ChartNoAxesCombined}
      tom="demanda"
      valor={valor}
      selo={variacao !== null ? <SeloDaVariacao percentual={variacao} /> : undefined}
      motivoSemDado={motivo}
      variacao={ano ? `${nº(ano.maquinas)} máquinas entregues · até ${mesPorExtenso(ano.fim)}` : undefined}
      sobre={
        <>
          <p>
            <strong>O valor de venda do ART das máquinas ENTREGUES</strong> — a data de entrega preenchida
            {ano ? `, de ${mesPorExtenso(ano.inicio)} a ${mesPorExtenso(ano.fim)} (o último mês fechado)` : ''} —, com e sem
            comprador no CRM, pela filial da unidade que vendeu: uma máquina por venda do ART, como a Gestão de Negócios conta.{' '}
            Medido {noRecorte(ex.filiais[0]?.filial)}.
          </p>
          {ressalvas.length > 0 && <p>{ressalvas.join('. ')}.</p>}
          {anterior && (
            <p>
              Mesmo trecho do ano anterior ({mesPorExtenso(anterior.inicio)} a {mesPorExtenso(anterior.fim)}): {emMilhoes(anterior.valor)} em{' '}
              {nº(anterior.maquinas)} máquinas
              {variacao !== null ? ` — ${variacao > 0 ? '+' : variacao < 0 ? '−' : ''}${porcento(Math.abs(variacao))} no valor` : ''}.
            </p>
          )}
          {mesEmCurso && (
            <p>
              {mesPorExtenso(mesEmCurso.inicio)}, em curso e à parte: {emMilhoes(mesEmCurso.valor)} em {nº(mesEmCurso.maquinas)} máquinas
              entregues até agora.
            </p>
          )}
          {nota && (
            <p>
              Conferência, sem somar: a nota de saída do Protheus (máquina, peça e serviço) somou {emMilhoes(ex.realizadoDoAno.total)} no
              ano e {emMilhoes(nota.total)} em {mesPorExtenso(nota.competencia)}, até {diaEHora(nota.carregadoEm)} — devolução não abatida.
            </p>
          )}
        </>
      }
    />
  );
}

/**
 * B. A META DE VENDA × O REALIZADO (#138) — em MÁQUINAS, no ano fiscal até o último mês fechado.
 *
 * Tem leitura própria (`/relatorios/metas`), e por isso aparece mesmo quando os indicadores das filiais não responderam.
 * No alcance Próprios — o vendedor, no perfil Padrão — é a meta dele; sem `Meta.Ler`, o cartão diz que falta a permissão,
 * e não "meta zero".
 */
function CartaoDaMeta({ metas, carregando }: { metas: MetasConsolidadas | null; carregando: boolean }) {
  const titulo = metas?.periodo ? `Meta e realizado · FY${metas.periodo.anoFiscal}` : 'Meta e realizado';
  const comum = { rotulo: titulo, icone: Target, tom: 'captura' as const, oQue: 'a meta e o realizado' };

  if (!metas || metas.respondidas === 0) {
    const motivo = carregando
      ? 'Lendo a meta das filiais…'
      : metas?.semPermissao
        ? SEM_PERMISSAO_DA_META
        : 'A leitura da meta das filiais não respondeu.';
    return <CartaoDeDecisao {...comum} valor={null} carregando={carregando} motivoSemDado={carregando ? undefined : motivo} variacao={null} sobre={REGRA_DA_META} />;
  }

  const { periodo, metaMaquinas: meta, realizadoMaquinas: realizado, origem } = metas;
  const proprio = metas.alcance === 'Proprios';
  const pct = meta > 0 ? (100 * realizado) / meta : null;

  // O CADASTRO NÃO LIDO NÃO É "META ZERO": a rotina das metas ainda não rodou, e o cartão diz o que falta.
  if (origem === null) {
    return (
      <CartaoDeDecisao
        {...comum}
        valor={nº(realizado)}
        unidade="máquinas"
        motivoSemDado={undefined}
        variacao="o cadastro de metas da Gestão de Negócios ainda não foi lido"
        sobre={`Realizado: ${nº(realizado)} máquina(s) em ${periodo?.texto ?? 'período'}; a meta entra quando a rotina das metas rodar. ${REGRA_DA_META}`}
      />
    );
  }

  const partes = [
    metas.aguardandoEntrega > 0 ? `${nº(metas.aguardandoEntrega)} vendidas aguardando entrega` : null,
    metas.pendentesNoArt ? `${nº(metas.pendentesNoArt)} vendas aguardam na integração do ART (cadastro, chassi ou outro motivo)` : null,
    // O CONSÓRCIO NÃO LIDO NÃO É "ZERO COTAS": o cartão diz que falta a leitura.
    metas.realizadoConsorcio !== null
      ? metas.metaConsorcio > 0 || metas.realizadoConsorcio > 0
        ? `consórcio: ${nº(metas.realizadoConsorcio)} de ${nº(metas.metaConsorcio)} cotas`
        : null
      : metas.metaConsorcio > 0
        ? `consórcio: ${nº(metas.metaConsorcio)} cotas de meta, realizado ainda não lido`
        : null,
    `mesmo trecho do FY anterior: ${nº(metas.realizadoNoAnterior)}`,
    metas.mesEmCurso
      ? `${mesPorExtenso(metas.mesEmCurso.competencia)} em curso: ${nº(metas.mesEmCurso.realizadoMaquinas)} de ${nº(metas.mesEmCurso.metaMaquinas)}`
      : null,
  ].filter((p): p is string => p !== null);

  // A FILIAL QUE FALHOU SAI DA SOMA, E NÃO EM SILÊNCIO (revisão do PR #248): o contexto diz quantas responderam e quais
  // ficaram fora — a que disse 403 está fora do alcance, e não é falha.
  const esperadas = metas.respondidas + metas.falhas.length;

  return (
    <CartaoDeDecisao
      {...comum}
      valor={nº(realizado)}
      unidade={meta > 0 ? `de ${nº(meta)}` : 'máquinas'}
      // O PERCENTUAL DA META NO SELO É O MESMO DA LINHA DE BAIXO (maquete): para o leitor de tela, ele já foi dito ali.
      selo={
        pct !== null ? (
          <span className="v360-selo" data-tom="meta" aria-hidden="true">
            {porcento(pct)}
          </span>
        ) : undefined
      }
      variacao={
        <>
          {`${pct === null ? 'sem meta no período' : `${porcento(pct)} da meta`} · ${periodo?.texto ?? ''}`}
          {metas.falhas.length > 0 && (
            <span className="v360-periodo-alerta">
              {` · ${metas.respondidas} de ${esperadas} filiais — fora: ${metas.falhas.map((f) => f.nome).join(', ')}`}
            </span>
          )}
        </>
      }
      motivoSemDado={undefined}
      sobre={
        <>
          <p>
            <strong>{proprio ? 'Sua meta: máquinas vendidas pela sua filial.' : 'Máquinas vendidas contra a meta de venda.'}</strong>{' '}
            {partes.join(' · ')}.
          </p>
          <p>{REGRA_DA_META}</p>
          <p>
            {`Cadastro lido em ${diaEHora(origem.lidaEm)}${proprio ? '' : `, ${metas.respondidas} filial(is)`}.`}{' '}
            {metas.observacoes.join(' ')}
          </p>
        </>
      }
    />
  );
}

/**
 * Os cinco números de decisão — o valor, uma linha de contexto, e a composição e a regra na dica ao lado do rótulo.
 *
 * O CORPO NÃO TEM PARÁGRAFO (28/09/2026): os cartões antigos traziam duas linhas de rodapé com a composição do número, e
 * a linha dos cinco virava leitura. O que o número é e como se compõe está no ⓘ, que abre pelo ponteiro, pelo teclado e
 * pelo toque.
 */
function CincoIndicadores({
  ano,
  ex,
  carregando,
  metas,
  carregandoMetas,
}: {
  ano: number;
  ex: ExecutivoConsolidado | null;
  carregando: boolean;
  metas: MetasConsolidadas | null;
  carregandoMetas: boolean;
}) {
  // LENDO NÃO É AUSÊNCIA: enquanto a leitura não volta, o cartão pulsa, e não afirma motivo nenhum.
  const semResposta = {
    valor: null,
    carregando,
    motivoSemDado: carregando ? undefined : 'A leitura dos indicadores das filiais não respondeu.',
    variacao: null,
    sobre: 'Somado filial a filial, pela leitura dos indicadores executivos de cada filial em operação.',
  };

  if (!ex) {
    return (
      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao rotulo={`Faturamento FY${ano}`} icone={ChartNoAxesCombined} tom="demanda" {...semResposta} />
        <CartaoDaMeta metas={metas} carregando={carregandoMetas} />
        <CartaoDeDecisao rotulo="Clientes na carteira" icone={Users} tom="mercado" {...semResposta} />
        <CartaoDeDecisao rotulo="Cobertura pela cadência" icone={ShieldCheck} tom="oportunidade" {...semResposta} />
        <CartaoDeDecisao rotulo="Conhecimento de mercado" icone={ChartPie} tom="neutro" {...semResposta} />
      </div>
    );
  }

  const cobertura = ex.cobertura;
  const coberturaPct = cobertura.elegiveis > 0 ? (100 * cobertura.cobertos) / cobertura.elegiveis : null;
  const mercado = ex.mercado;
  const carteira = ex.carteira;

  return (
    <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
      {/* A. FATURAMENTO DO ANO — o ART das máquinas ENTREGUES (decisão do Ricardo de 29/09/2026). */}
      <CartaoDoFaturamento ex={ex} />

      {/* B. META E REALIZADO — a meta de VENDA da API Gestão de Negócios × as máquinas do ART (#138). */}
      <CartaoDaMeta metas={metas} carregando={carregandoMetas} />

      {/* C. CLIENTES — únicos pela filial de cadastro; vínculos pela filial da carteira.

          O ZERO É "SEM VÍNCULO", E NÃO "SEM CLIENTE" (24/09/2026). A conta só enxerga cliente com vínculo ativo em
          carteira; a carga do cadastro traz os clientes da SA1 sem carteira, e até a carga das carteiras rodar, o banco
          tem milhares de clientes e este cartão tem zero. */}
      <CartaoDeDecisao
        rotulo="Clientes na carteira"
        icone={Users}
        tom="mercado"
        valor={nº(carteira.clientesCadastradosComVinculo)}
        unidade="clientes"
        variacao={
          carteira.clientesCadastradosComVinculo > 0
            ? `${nº(carteira.vinculosComerciais)} vínculos em ${nº(carteira.carteirasComerciais)} carteiras`
            : 'nenhum cliente com vínculo ativo em carteira'
        }
        motivoSemDado={undefined}
        sobre={
          <>
            <p>
              {carteira.clientesCadastradosComVinculo > 0
                ? `Com vínculo: ${nº(carteira.clientes)} clientes · ${nº(carteira.prospects)} prospects · ${nº(carteira.suspects)} suspects · ${nº(carteira.semDocumento)} sem CPF/CNPJ.`
                : `${nº(carteira.vinculosComerciais)} vínculos em ${nº(carteira.carteirasComerciais)} carteiras comerciais: cliente cadastrado sem carteira não entra nesta conta.`}
            </p>
            <p>
              Cada cliente é contado uma vez, na filial em que está cadastrado, se tiver vínculo ativo em qualquer
              carteira. Um cliente em três carteiras é um cliente e três vínculos. Cliente cadastrado sem carteira não
              entra — nem no número, nem na divisão por situação: zero aqui quer dizer que nenhum cliente tem vínculo de
              carteira ainda, e não que o cadastro está vazio.
            </p>
          </>
        }
      />

      {/* D. COBERTURA — cadência declarada da linha, a mesma regra do mapa. Contato não é visita. */}
      <CartaoDeDecisao
        rotulo="Cobertura pela cadência"
        icone={ShieldCheck}
        tom="oportunidade"
        valor={coberturaPct === null ? null : porcento(coberturaPct)}
        motivoSemDado="Nenhum vínculo em linha com cadência declarada: sem elegível, não há percentual."
        variacao={`${nº(cobertura.cobertos)} de ${nº(cobertura.elegiveis)} vínculos no prazo`}
        sobre={
          <>
            <p>
              {nº(cobertura.pendentes)} pendentes ({nº(cobertura.foraDaCadencia)} fora do prazo + {nº(cobertura.nuncaContatados)}{' '}
              nunca) · {nº(cobertura.semCadencia)} em linha sem cadência, fora do % · contato pela regra da BI, não visita.
            </p>
            <p>
              Elegível: vínculo em carteira comercial de linha com cadência declarada (máquinas 180/180/180/360 dias por
              classe A/B/C/D; prospecção 120/120/120/180; peças e AMS 360). Contato: o apurado pela regra da BI de
              carteiras do Vórtice (53 resultados, em qualquer canal) — não os {nº(cobertura.tiposDeAtividade)} tipos de
              atividade cadastrados no CRM (nenhum marcado como visita); o Vórtice registra o canal, mas o CRM ainda não o
              carrega.
            </p>
          </>
        }
      />

      {/* E. MERCADO — o que o CRM registra de derrota. A Captura Tracbel (issue 162) não é deste cartão: ela divide as
          unidades do ART pela demanda estimada, e mora nos Indicadores Geográficos. */}
      <CartaoDeDecisao
        rotulo="Conhecimento de mercado"
        icone={ChartPie}
        tom="neutro"
        valor={nº(mercado.vendasPerdidasRegistradas)}
        unidade="perdas"
        motivoSemDado={undefined}
        variacao={`${nº(mercado.comConcorrente)} com concorrente registrado`}
        sobre={
          <>
            <p>
              Vendas perdidas registradas pelo CEN
              {mercado.primeiraEm ? `, de ${data(mercado.primeiraEm)} a ${data(mercado.ultimaEm)}` : ''}:{' '}
              {nº(mercado.comModeloDoConcorrente)} com modelo do concorrente, {nº(mercado.comOsDoisPrecos)} com os dois
              preços, {nº(mercado.unidades)} máquinas nessas perdas.
            </p>
            <p>
              Este cartão conta derrotas registradas pelo CEN, e não o tamanho do mercado. As máquinas que a Tracbel
              vendeu estão no banco do CRM, lidas do ART — em unidades, e não em faturamento —, e a Captura Tracbel, que
              as divide pela demanda estimada do período (a anual proporcional aos meses), é medida nos Indicadores
              Geográficos.
            </p>
          </>
        }
      />
    </div>
  );
}

/**
 * A COMPOSIÇÃO DOS CINCO NÚMEROS, filial a filial — o que o pedido chama de "consultar os registros que compõem". Cada
 * coluna é somável; a linha de total é a do cartão. Fechada por padrão, como as séries da Rentabilidade: a tabela de
 * onze colunas é conferência, e não leitura de todo dia.
 *
 * FILIAL A FILIAL SÓ QUANDO ABRE (30/09/2026, #313): os cartões são uma leitura só, na filial do seletor. Em "Todas as
 * filiais", abrir a composição lê filial a filial — a ponte de antes, agora fora do caminho da tela —, e o total é o
 * do cartão. Com uma filial escolhida, a linha é a dela.
 */
function ComposicaoDosIndicadores({ ex, metas }: { ex: ExecutivoConsolidado; metas: MetasConsolidadas | null }) {
  const [aberta, setAberta] = useState(false);
  const lida = ex.filiais[0]?.filial;
  const todas = lida?.codigo === TODAS_AS_FILIAIS;

  return (
    <PainelDoMomento
      titulo="Composição dos cinco indicadores"
      data-bloco="composicao"
      subtitulo={
        todas || !lida
          ? 'Filial a filial, com a fonte e a regra de cada número.'
          : `${lida.nome}, com a fonte e a regra de cada número. Filial a filial em “Todas as filiais”.`
      }
      // A COMPOSIÇÃO FALA A LÍNGUA DO NEGÓCIO (29/09/2026, #31): trazia os nomes das tabelas e das colunas
      // (`integracao.RegistroDeOrigem`, `vr_vda`, `comercial.FaturamentoDoCliente`, `organizacao.MetaDeVenda`,
      // `frota.VendaDeMaquina`, `organizacao.CotaDeConsorcioVendida`, `ClienteCarteira.UltimaInteracaoEm`,
      // `organizacao.LinhaDeNegocio`, `processo.VendaPerdida`). A regra de cada número é a mesma; a linhagem até a tabela
      // mora no documento 29 ("de onde vem cada dado").
      dica={
        <ul className="v360-composicao-regras">
          <li>
            <strong>Faturamento</strong> (decisão de 29/09/2026): as vendas de máquina do ART → o valor de venda das máquinas com
            a data de entrega no ano fiscal {nomeDoAno(ex.ano)}, até o último mês fechado, com e sem comprador no CRM, pela filial
            da unidade que vendeu → soma das filiais. Máquinas entregues: uma por venda do ART; a sem valor conta nas máquinas e
            não no valor.
          </li>
          <li>
            <strong>NF Protheus</strong>, como conferência e sem somar: as notas fiscais de saída de venda do Protheus → valor
            líquido no mesmo ano, com cliente e sem cliente — máquina, peça e serviço. Devolução e cancelamento não são abatidos.
          </li>
          <li>
            <strong>Meta e realizado</strong>: API Gestão de Negócios (cadastro de metas) → a meta de venda, em máquinas, contra as
            vendas do ART, só as entregues e pelo mês da entrega, como a Gestão de Negócios, no ano fiscal até o último mês fechado{' '}
            {metas?.periodo ? `(${metas.periodo.texto})` : ''}. As vendas que aguardam na integração do ART (cadastro, chassi ou outro motivo) ficam à parte; o
            consórcio é em cotas, contra as cotas vendidas da performance de consórcio da mesma API.
            A previsão dos gestores está no relatório Forecast da gerência.
          </li>
          <li>
            <strong>Clientes</strong>: o cadastro de clientes × os vínculos com as carteiras → cliente com vínculo ativo, na filial
            de cadastro. Vínculos e carteiras, pela filial da carteira.
          </li>
          <li>
            <strong>Cobertura</strong>: a última interação de cada vínculo contra a cadência da linha de negócio, pela classe ABC
            do cliente (sem classe = D) → cobertos ÷ elegíveis. Mesma regra do mapa de cobertura.
          </li>
          <li>
            <strong>Mercado</strong>: as vendas perdidas (formulário do CEN) — derrotas registradas, e não o tamanho do mercado. A
            Captura Tracbel é medida nos Indicadores Geográficos.
          </li>
        </ul>
      }
      direita={
        <button type="button" className="mom-botao" aria-expanded={aberta} onClick={() => setAberta((a) => !a)}>
          {aberta ? 'Fechar a composição' : 'Ver filial a filial'}
          {!aberta && <ArrowRight size={14} strokeWidth={2.4} aria-hidden="true" />}
        </button>
      }
    >
      {/* A ROLAGEM MORA AQUI, e só aqui: onze colunas não cabem num celular, e a tabela rola dentro da própria caixa em
          vez de empurrar a página para o lado. */}
      {aberta &&
        (todas ? (
          <ComposicaoFilialAFilial ex={ex} metas={metas} />
        ) : (
          <TabelaDaComposicao ex={ex} linhas={ex.filiais} metasDasFiliais={metas?.filiais ?? []} metas={metas} comTotal={false} />
        ))}
    </PainelDoMomento>
  );
}

/**
 * "TODAS AS FILIAIS", ABERTA: a composição lê filial a filial só agora — a ponte de antes, fora do caminho da tela —, e
 * o total continua o do cartão, que é a leitura de todas numa vez só.
 */
function ComposicaoFilialAFilial({ ex, metas }: { ex: ExecutivoConsolidado; metas: MetasConsolidadas | null }) {
  const { contexto } = useContextoDeAcesso();
  const porFilial = useRecurso(
    (sinal) => obterExecutivoPorFilial(contexto, ex.ano, sinal),
    [contexto.usuario, contexto.empresa, ex.ano],
  );
  const metasPorFilial = useRecurso((sinal) => obterMetasPorFilial(contexto, sinal), [contexto.usuario, contexto.empresa]);

  if (porFilial.carregando) return <BlocoCarregando oQue="a composição filial a filial" />;
  if (porFilial.erro) return <BlocoErro erro={porFilial.erro} aoTentarDeNovo={porFilial.recarregar} />;
  return (
    <TabelaDaComposicao
      ex={ex}
      linhas={porFilial.dados ?? []}
      metasDasFiliais={metasPorFilial.dados ?? []}
      metas={metas}
      comTotal
    />
  );
}

/** A tabela da composição: uma linha por filial lida e, em "Todas as filiais", o total do cartão. */
function TabelaDaComposicao({
  ex,
  linhas,
  metasDasFiliais,
  metas,
  comTotal,
}: {
  ex: ExecutivoConsolidado;
  linhas: ExecutivoDaFilial[];
  metasDasFiliais: MetaDaFilial[];
  metas: MetasConsolidadas | null;
  comTotal: boolean;
}) {
  const fy = metas?.periodo ? `FY${metas.periodo.anoFiscal}` : 'FY';
  // A META QUE FALHOU NA FILIAL NÃO VIRA TRAÇO MUDO: a célula diz que a leitura falhou; o traço é "sem meta".
  const metaDa = (codigo: string) => metasDasFiliais.find((f) => f.filial.codigo === codigo) ?? null;
  const celulaDaMeta = (codigo: string, valor: (m: NonNullable<MetaDaFilial['meta']>) => number) => {
    const lida = metaDa(codigo);
    if (lida?.meta) return nº(valor(lida.meta));
    return lida?.erro && !lida.foraDoAlcance ? 'não respondeu' : '—';
  };

  return (
    <div className="mom-tabela-rolagem v360-composicao-tabela">
      <table className="mom-tabela">
        <caption className="cad-so-leitor">Composição dos indicadores por filial</caption>
        <thead>
          <tr>
            <th scope="col">Filial</th>
            <th scope="col" className="mom-num">Faturamento FY{ex.ano}</th>
            <th scope="col" className="mom-num">Máquinas entregues</th>
            <th scope="col" className="mom-num">NF Protheus FY{ex.ano}</th>
            <th scope="col" className="mom-num">
              Meta {fy} (máq.) <InfoTooltip texto={REGRA_DA_META} rotulo="Como a meta de venda se conta" />
            </th>
            <th scope="col" className="mom-num">Realizado {fy} (máq.)</th>
            <th scope="col" className="mom-num">Clientes únicos</th>
            <th scope="col" className="mom-num">Vínculos comerciais</th>
            <th scope="col" className="mom-num">Elegíveis</th>
            <th scope="col" className="mom-num">No prazo</th>
            <th scope="col" className="mom-num">Pendentes</th>
            <th scope="col" className="mom-num">Vendas perdidas</th>
          </tr>
        </thead>
        <tbody>
          {linhas.map(({ filial, painel, erro }) => {
            if (!painel) {
              return (
                <tr key={filial.codigo}>
                  <th scope="row">{filial.nome}</th>
                  <td colSpan={11}>não respondeu — {erro?.message ?? 'motivo não informado'}</td>
                </tr>
              );
            }
            const i = painel.indicadores;
            const entregues = i.entreguesNoAno ?? null;
            return (
              <tr key={filial.codigo}>
                <th scope="row">{filial.nome}</th>
                <td className="mom-num">{entregues ? emMilhoes(entregues.valor) : '—'}</td>
                <td className="mom-num">{entregues ? nº(entregues.maquinas) : '—'}</td>
                <td className="mom-num">{emMilhoes(i.ano.total)}</td>
                <td className="mom-num">{celulaDaMeta(filial.codigo, (m) => m.totais.metaMaquinas)}</td>
                <td className="mom-num">{celulaDaMeta(filial.codigo, (m) => m.totais.realizadoMaquinas)}</td>
                <td className="mom-num">{nº(i.carteira.clientesCadastradosComVinculo)}</td>
                <td className="mom-num">{nº(i.carteira.vinculosComerciais)}</td>
                <td className="mom-num">{nº(i.cobertura.elegiveis)}</td>
                <td className="mom-num">{nº(i.cobertura.cobertos)}</td>
                <td className="mom-num">{nº(i.cobertura.pendentes)}</td>
                <td className="mom-num">{nº(i.mercado.vendasPerdidasRegistradas)}</td>
              </tr>
            );
          })}
          {comTotal && (
            <tr className="v360-total" data-linha="total">
              <th scope="row">Total — todas as filiais</th>
              <td className="mom-num">{ex.entregues.ano ? emMilhoes(ex.entregues.ano.valor) : '—'}</td>
              <td className="mom-num">{ex.entregues.ano ? nº(ex.entregues.ano.maquinas) : '—'}</td>
              <td className="mom-num">{emMilhoes(ex.realizadoDoAno.total)}</td>
              <td className="mom-num">{metas && metas.respondidas > 0 ? nº(metas.metaMaquinas) : '—'}</td>
              <td className="mom-num">{metas && metas.respondidas > 0 ? nº(metas.realizadoMaquinas) : '—'}</td>
              <td className="mom-num">{nº(ex.carteira.clientesCadastradosComVinculo)}</td>
              <td className="mom-num">{nº(ex.carteira.vinculosComerciais)}</td>
              <td className="mom-num">{nº(ex.cobertura.elegiveis)}</td>
              <td className="mom-num">{nº(ex.cobertura.cobertos)}</td>
              <td className="mom-num">{nº(ex.cobertura.pendentes)}</td>
              <td className="mom-num">{nº(ex.mercado.vendasPerdidasRegistradas)}</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

/** "Sem dado" compacto: o que falta e por quê, sem guardar a altura do gráfico que não está ali. */
function SemDado({ oQue, porque }: { oQue: string; porque: string }) {
  return (
    <div className="v360-sem-dado">
      <strong>Sem dado para {oQue}</strong>
      <span>{porque}</span>
    </div>
  );
}

/**
 * A VARIAÇÃO DO FATURAMENTO NO SELO DO CARTÃO (maquete de 29/09/2026) — a mesma da dica, contra o mesmo trecho do ano
 * anterior. A seta é desenho; o leitor de tela ouve a frase inteira.
 */
function SeloDaVariacao({ percentual }: { percentual: number }) {
  const sentido = percentual > 0 ? 'alta' : percentual < 0 ? 'baixa' : 'neutro';
  const Seta = percentual < 0 ? ArrowDown : ArrowUp;
  const texto = porcento(Math.abs(percentual));
  return (
    <span className="v360-selo" data-tom={sentido}>
      <span className="v360-selo-desenho" aria-hidden="true">
        {percentual !== 0 && <Seta size={12} strokeWidth={2.6} />}
        {texto}
      </span>
      <span className="cad-so-leitor">
        {percentual === 0 ? 'igual' : `${percentual > 0 ? 'mais' : 'menos'} ${texto}`} no valor, contra o mesmo trecho do ano
        anterior
      </span>
    </span>
  );
}

/**
 * DE ONDE VÊM OS NÚMEROS, na dica ao lado da filial do cabeçalho (30/09/2026, #313): a filial escolhida no seletor do
 * topo, ou todas as que a pessoa alcança — numa leitura só por painel.
 */
function FiliaisLidas({ ex }: { ex: ExecutivoConsolidado }) {
  const lida = ex.filiais[0];
  const todas = lida?.filial.codigo === TODAS_AS_FILIAIS;
  return (
    <>
      {todas ? (
        <p>
          Os números somam todas as filiais que você alcança. Para ver uma filial só, escolha-a no seletor do topo; o
          detalhe filial a filial está na composição dos cinco indicadores.
        </p>
      ) : (
        <p>
          Os números são de {lida?.filial.nome ?? 'uma filial'}, a filial escolhida no seletor do topo. Quem tem o alcance
          entre filiais vê a empresa inteira escolhendo “Todas as filiais”.
        </p>
      )}
      {lida && !lida.painel && <p>A leitura não respondeu: {lida.erro?.message ?? 'motivo não informado'}.</p>}
    </>
  );
}

function Alerta({
  tipo,
  icone: Icone,
  titulo,
  detalhe,
  acao,
  para,
}: {
  tipo: 'critico' | 'aviso' | 'atencao' | 'info';
  /** O ícone da maquete: triângulo no crítico, relógio nas tarefas, círculo com exclamação nos processos. */
  icone: LucideIcon;
  titulo: ReactNode;
  detalhe: string;
  acao: string;
  para: string;
}) {
  return (
    <div className={`v360-alerta v360-alerta-${tipo}`}>
      <div className="v360-alerta-icon" aria-hidden="true">
        <Icone size={22} strokeWidth={2.2} />
      </div>
      <div className="v360-alerta-body">
        <div className="v360-alerta-titulo">{titulo}</div>
        <div className="v360-alerta-detalhe">{detalhe}</div>
      </div>
      <Link to={para} className="v360-alerta-acao">
        {acao}
      </Link>
    </div>
  );
}
