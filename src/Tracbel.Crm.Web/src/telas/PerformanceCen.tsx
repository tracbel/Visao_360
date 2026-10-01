/**
 * Performance de CEN — o que existe de agregado real por pessoa.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a tela deixou de ler o JSON do protótipo, e encolheu.
 *
 * O protótipo mostrava faturamento por CEN, atingimento de meta, tendência de
 * doze meses, taxa de conversão e um ranking com cinco pessoas escritas no
 * JavaScript. **Nada disso tem lastro**, e a lista de por quês está no rodapé
 * desta tela, com o número de cada uma.
 *
 * O que existe de verdade, hoje, é **um** agregado por pessoa: a cobertura da
 * carteira. `/api/v1/relatorios/cobertura` devolve uma linha por carteira com o
 * CEN responsável, quantos clientes ela tem, quantos foram contatados em 30 e
 * em 90 dias e quantos nunca foram. Agrupar essas linhas por responsável dá a
 * pergunta que o gerente faz de verdade: **quem está cobrindo a carteira e quem
 * não está.**
 *
 * ---------------------------------------------------------------------------
 * A SOMA É FEITA AQUI, E ISSO PRECISA DE JUSTIFICATIVA, porque a regra do
 * projeto é agregar no banco. A exceção vale por dois motivos e só por eles:
 *
 * 1. **O agregado do banco já veio pronto** — a rota devolve as carteiras já
 *    contadas por `GROUP BY`. O que a tela faz é somar dezenas de linhas, não
 *    quarenta e cinco mil.
 * 2. **Não existe rota que agrupe por pessoa.** `ProprietarioId` existe na
 *    consulta de processos do domínio, mas o endpoint não o expõe, e não há
 *    parâmetro de responsável em `/tarefas` além de `minhas`. Registrado no
 *    rodapé como o que falta.
 *
 * Somar carteira por responsável é exato: cada carteira tem um responsável só,
 * e nenhuma linha é contada duas vezes. O que **não** é somável é o cliente —
 * metade dos clientes está em duas ou mais carteiras (documento 25, §6), então
 * "clientes do CEN" é contagem de VÍNCULOS, e a tela escreve isso.
 *
 * ---------------------------------------------------------------------------
 * 27/09/2026 — A META DE VENDA VOLTOU, COM FONTE (#138).
 *
 * A meta é a cota da API Gestão de Negócios, em máquinas por consultor, linha,
 * mês e filial; o realizado de cada consultor são as vendas do ART em que ele é
 * o vendedor (decisão D-M2). A rota `/api/v1/relatorios/metas` já devolve a
 * soma por consultor da filial do cabeçalho — a tela só desenha. O consultor é
 * o login da GN, e não o responsável da carteira: por isso a tabela é à parte
 * da cobertura, e as duas não se cruzam por nome.
 *
 * ---------------------------------------------------------------------------
 * 27/09/2026 — O FUNIL DO CEN (documento 52).
 *
 * O "0 ganhos · 0 perdidos · 0 abertos" contava `processo.Processo`, que só a
 * onda 2 carrega. No lugar entra `/relatorios/funil-por-estagio` pelo FLUXO
 * (a leitura da Performance, decisão do Ricardo) e com o responsável escolhido,
 * e as perdas do período de `/relatorios/vendas-perdidas` — só a principal.
 * Sem funil, cada estágio mostra "—" com o motivo verdadeiro.
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS (#293, bloco 2). O cabeçalho com a hora da leitura; o CEN saiu de
 * dentro do primeiro cartão e foi para a barra de filtros, porque ele vale para o painel do CEN, o funil e as perdas; os
 * quatro números como `CartaoDeDecisao`; duas seções — o painel do CEN (cobertura por classe e funil lado a lado) e a
 * carteira de cada CEN (a divisão e o ranking) — em `PainelDoMomento`; o seletor de métrica foi para o título do
 * ranking. Os três blocos recolhíveis do fim ficaram. NENHUM NÚMERO, REGRA OU TEXTO DE REGRA MUDOU.
 *
 * ---------------------------------------------------------------------------
 * 01/10/2026 — A MAQUETE DO RICARDO (`docs/prototipo/capturas-referencia/performance-cen-maquete-2026-10-01.png`).
 * Decisões dele:
 *
 * - QUATRO FILTROS QUE MUDAM A TELA INTEIRA, NA HORA (sem o botão "Aplicar"). O CEN, a classe do cliente e a carteira
 *   recortam os cartões, a cobertura por classe, a carteira dos CENs, o ranking, a tabela e o funil — a classe e a
 *   carteira chegaram à API para isso. O PERÍODO muda o funil, as perdas e a meta; a cobertura é sempre o retrato de hoje.
 *   Os gráficos de CENs e o ranking mostram todos os do recorte, com o escolhido em destaque: um ranking de uma barra
 *   não compara ninguém.
 * - A COMPARAÇÃO COM O PERÍODO ANTERIOR DOS CARTÕES FICA DE FORA: o CRM não guarda a cobertura de dias passados. A lacuna
 *   está escrita no cartão "O que esta métrica não faz".
 * - O FUNIL DO CEN E AS PERDAS, que a maquete não tem, viraram o quarto cartão recolhível do fim.
 *
 * O desenho da maquete, sem inventar número: a cobertura por classe em barras com o percentual escrito e a tabela ao
 * lado, o "principal alerta" (a classe com mais vínculos fora da cadência e nunca contatados), a carteira dos CENs pelas
 * faixas de tempo sem contato e o ranking em barras. As faixas da carteira dos CENs são as da Cobertura de Carteira (até
 * 30 dias, de 31 a 90, mais de 90 e nunca) — a carteira não tem cadência por pessoa —, com as cores de lá.
 */

import {
  CalendarDays,
  ChartColumnIncreasing,
  ChartNoAxesColumnIncreasing,
  ChevronDown,
  FileText,
  Funnel,
  Info,
  RefreshCw,
  Sheet,
  Target,
  TriangleAlert,
  Trophy,
  UserRound,
  UsersRound,
} from 'lucide-react';
import { useMemo, useState, type CSSProperties, type ReactNode } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { ENTRAM_NO_RANKING } from '../dados/api/consolidado';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { ErroDaApi } from '../dados/api/http';
import { obterMetaDaFilial } from '../dados/api/metas';
import {
  obterFunilPorEstagio,
  obterPainelDoCen,
  obterResumoDeCobertura,
  obterVendasPerdidas,
} from '../dados/api/relacionamento';
import { useRecurso, type Leitura } from '../dados/api/useRecurso';
import type { MetaERealizadoDaFilial } from '../tipos/metas';
import type {
  Agregado,
  CoberturaPorClasse,
  FunilPorEstagio,
  ResumoDeCobertura,
  VendasPerdidas,
} from '../tipos/relacionamento';
import { formatarData } from './cadastro/formato';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';
import '../estilos/performance-cen.css';

const nº = (v: number) => v.toLocaleString('pt-BR');

/**
 * As métricas que o ranking sabe desenhar. Todas saem do MESMO agregado que a
 * tabela abaixo usa, e por isso o gráfico e a tabela nunca divergem. A primeira
 * é a da maquete: cobertura em 30 dias.
 */
const METRICAS = [
  { id: 'cobertura30', rotulo: 'Cobertura em 30 dias', titulo: 'cobertura em 30 dias', unidade: '%' },
  { id: 'clientes', rotulo: 'Vínculos na carteira', titulo: 'vínculos na carteira', unidade: 'vínculos' },
  { id: 'em30', rotulo: 'Contatados em 30 dias', titulo: 'contatados em 30 dias', unidade: 'vínculos' },
  { id: 'nunca', rotulo: 'Nunca contatados', titulo: 'nunca contatados', unidade: 'vínculos' },
] as const;

type MetricaId = (typeof METRICAS)[number]['id'];

/** Quantas barras o ranking mostra — o "Top 10" da maquete. */
const BARRAS = 10;

/** Quantos CENs a carteira dos CENs mostra: o seletor do painel. Zero é todos. */
const QUANTOS_CENS = [
  { valor: 10, rotulo: 'Top 10 CENs' },
  { valor: 20, rotulo: 'Top 20 CENs' },
  { valor: 0, rotulo: 'Todos os CENs' },
] as const;

/**
 * As quatro faixas de tempo sem contato, nas mesmas cores da Cobertura de
 * Carteira — a mesma faixa tem a mesma cor em toda a aplicação. Os acumulados da
 * API são cumulativos (`em90` inclui `em30`), e subtrair aqui é o que torna as
 * fatias exclusivas.
 */
const FAIXAS_DE_CONTATO: { nome: string; cor: string; medir: (c: LinhaDeCen) => number }[] = [
  { nome: 'Em até 30 dias', cor: '#1B873F', medir: (c) => c.em30 },
  { nome: 'Entre 31 e 90 dias', cor: '#F39A1E', medir: (c) => c.em90 - c.em30 },
  { nome: 'Acima de 90 dias', cor: '#E53935', medir: (c) => c.clientes - c.em90 - c.nunca },
  { nome: 'Nunca contatados', cor: '#8A909A', medir: (c) => c.nunca },
];

/**
 * Os quatro estados de um vínculo contra a cadência declarada, nas cores da maquete.
 *
 * O cinza do "sem cadência declarada" é de propósito o único sem temperatura: não é bom nem
 * ruim, é a ausência de prazo contra o que medir. Empurrá-lo para verde ou vermelho inventaria
 * uma meta que ninguém definiu.
 */
const FAIXAS_DE_COBERTURA: { nome: string; cor: string; medir: (c: CoberturaPorClasse) => number }[] = [
  { nome: 'Coberto na cadência', cor: '#1B873F', medir: (c) => c.cobertos },
  { nome: 'Fora da cadência', cor: '#F39A1E', medir: (c) => c.foraDaCadencia },
  { nome: 'Nunca contatado', cor: '#E53935', medir: (c) => c.nuncaContatados },
  { nome: 'Sem cadência declarada', cor: '#9CA3AF', medir: (c) => c.semCadenciaDeclarada },
];

/** Os seis estágios do funil do Vórtice, na ordem (documento 52). */
const ESTAGIOS = [
  { estagio: 'Lead', nome: 'Lead' },
  { estagio: 'Qualificado', nome: 'Qualificado' },
  { estagio: 'Cobertura', nome: 'Cobertura' },
  { estagio: 'Negociacao', nome: 'Negociação' },
  { estagio: 'Pedido', nome: 'Pedido' },
  { estagio: 'Faturamento', nome: 'Faturamento' },
] as const;

/**
 * O PERÍODO DO FILTRO (decisão do Ricardo, 01/10/2026): muda o funil, as perdas e a meta. A cobertura da carteira é o
 * retrato de hoje e não muda com ele. O padrão é o do servidor — o ano fiscal até o último mês fechado.
 */
const PERIODOS = [
  { id: 'fy', rotulo: 'Ano fiscal até o mês fechado', dias: null },
  { id: '30', rotulo: 'Últimos 30 dias', dias: 30 },
  { id: '90', rotulo: 'Últimos 90 dias', dias: 90 },
  { id: '365', rotulo: 'Últimos 12 meses', dias: 365 },
] as const;

type PeriodoId = (typeof PERIODOS)[number]['id'];

/** As classes do filtro — a curva ABC do cliente; sem classe apurada conta como D, como no painel do CEN. */
const CLASSES = ['A', 'B', 'C', 'D'] as const;

const dataIso = (d: Date) => d.toISOString().slice(0, 10);

/** O intervalo do período, em dias: de hoje para trás. O padrão não manda intervalo — o servidor usa o dele. */
function intervaloDo(periodo: PeriodoId, hoje: Date): { de: string; ate: string } | null {
  const dias = PERIODOS.find((p) => p.id === periodo)?.dias ?? null;
  if (dias === null) return null;
  return { de: dataIso(new Date(hoje.getTime() - dias * 86_400_000)), ate: dataIso(hoje) };
}

/** Conectivos que não viram inicial: "Hamilton de Souza Lopes" é "Hamilton S. Lopes". */
const CONECTIVOS = new Set(['da', 'das', 'de', 'do', 'dos', 'e']);

/** "JOÃO" vira "João"; a sigla com ponto (INT.MERCADO) fica como está. */
const capitular = (palavra: string) =>
  palavra.includes('.') ? palavra : palavra.charAt(0).toLocaleUpperCase('pt-BR') + palavra.slice(1).toLocaleLowerCase('pt-BR');

/**
 * O NOME COMO A MAQUETE ESCREVE: primeiro nome, a inicial do segundo e o último — "Matheus A. Cruz". O nome inteiro
 * continua na tabela em números e no leitor de tela de cada barra.
 */
function nomeCurto(nome: string): string {
  const partes = nome.trim().split(/\s+/);
  if (partes.length === 1) return nome;
  const primeiro = capitular(partes[0]);
  const ultimo = capitular(partes[partes.length - 1]);
  const meio = partes.slice(1, -1).find((p) => !CONECTIVOS.has(p.toLocaleLowerCase('pt-BR')));
  return meio ? `${primeiro} ${meio.charAt(0).toLocaleUpperCase('pt-BR')}. ${ultimo}` : `${primeiro} ${ultimo}`;
}

/** Uma pessoa, com as carteiras dela somadas. */
type LinhaDeCen = {
  responsavelNome: string;
  carteiras: number;
  linhas: string[];
  clientes: number;
  em30: number;
  em90: number;
  nunca: number;
  ultimoContatoEm: string | null;
};

/** As colunas ordenáveis. A ordenação é da tela porque o conjunto inteiro está nela. */
type Ordem = 'clientes' | 'cobertura30' | 'cobertura90' | 'nunca' | 'nome';

const COLUNAS: { rotulo: string; ordem: Ordem }[] = [
  { rotulo: 'CEN responsável', ordem: 'nome' },
  { rotulo: 'Vínculos na carteira', ordem: 'clientes' },
  { rotulo: 'Contato em 30 dias', ordem: 'cobertura30' },
  { rotulo: 'Contato em 90 dias', ordem: 'cobertura90' },
  { rotulo: 'Nunca contatados', ordem: 'nunca' },
];

const cobertura30 = (c: LinhaDeCen) => (c.clientes > 0 ? c.em30 / c.clientes : 0);

/**
 * QUEM ENTRA NO RANKING.
 *
 * Pessoa e área entram: a Inteligência de Mercado atende 5.000 clientes reais e a cobertura
 * deles conta como a de qualquer CEN. Ficam de fora as contas que não são operação — o
 * servidor da origem, o fornecedor e os testes — e as carteiras administrativas, que são
 * depósito de cadastro inativo e não performance de ninguém.
 *
 * Nada é apagado: o que sai daqui continua na Cobertura de Carteira, que é a tela operacional.
 */
const entraNoRanking = (c: ResumoDeCobertura) =>
  ENTRAM_NO_RANKING.has(c.naturezaDoResponsavel) && c.naturezaDaCarteira === 'Comercial';

/** Soma as carteiras por responsável. */
function porResponsavel(carteiras: ResumoDeCobertura[]): LinhaDeCen[] {
  const mapa = new Map<string, LinhaDeCen>();
  for (const c of carteiras) {
    const atual = mapa.get(c.responsavelNome);
    if (atual) {
      atual.carteiras += 1;
      atual.clientes += c.clientes;
      atual.em30 += c.comContatoEm30Dias;
      atual.em90 += c.comContatoEm90Dias;
      atual.nunca += c.nuncaContatados;
      if (!atual.linhas.includes(c.linhaDeNegocioNome)) atual.linhas.push(c.linhaDeNegocioNome);
      if (c.ultimoContatoEm && (!atual.ultimoContatoEm || c.ultimoContatoEm > atual.ultimoContatoEm)) {
        atual.ultimoContatoEm = c.ultimoContatoEm;
      }
    } else {
      mapa.set(c.responsavelNome, {
        responsavelNome: c.responsavelNome,
        carteiras: 1,
        linhas: [c.linhaDeNegocioNome],
        clientes: c.clientes,
        em30: c.comContatoEm30Dias,
        em90: c.comContatoEm90Dias,
        nunca: c.nuncaContatados,
        ultimoContatoEm: c.ultimoContatoEm,
      });
    }
  }
  return [...mapa.values()];
}

/** Sem classe escolhida, o resumo por classe não vai à API: a filial inteira já é ele. */
const semLeitura = () => Promise.resolve({ dados: null as Agregado<ResumoDeCobertura> | null, procedencia: null });

export function PerformanceCen() {
  const { contexto } = useContextoDeAcesso();
  const [ordem, setOrdem] = useState<Ordem>('cobertura30');
  const [descendente, setDescendente] = useState(false);
  const [metricaDoRanking, setMetricaDoRanking] = useState<MetricaId>('cobertura30');
  const [quantosCens, setQuantosCens] = useState<number>(10);
  const [emPercentual, setEmPercentual] = useState(true);

  /** Os quatro filtros da barra. Vazio é todos. */
  const [cenEscolhido, setCenEscolhido] = useState<string | null>(null);
  const [periodo, setPeriodo] = useState<PeriodoId>('fy');
  const [classe, setClasse] = useState('');
  const [carteiraEscolhida, setCarteiraEscolhida] = useState<string | null>(null);

  const intervalo = useMemo(() => intervaloDo(periodo, new Date()), [periodo]);

  const painel = useRecurso(
    (sinal) => obterPainelDoCen(contexto, cenEscolhido, sinal, carteiraEscolhida),
    [contexto.empresa, contexto.usuario, cenEscolhido, carteiraEscolhida],
  );

  /** A cobertura da filial inteira: os denominadores ("de 52 CENs da filial") e as carteiras do filtro. */
  const resumo = useRecurso(
    (sinal) => obterResumoDeCobertura(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  /** A mesma cobertura, só da classe escolhida — o recorte dos cartões, dos gráficos e da tabela. */
  const resumoDaClasse = useRecurso(
    (sinal) => (classe ? obterResumoDeCobertura(contexto, sinal, classe) : semLeitura()),
    [contexto.empresa, contexto.usuario, classe],
  );

  /** O funil do CEN escolhido, pelo FLUXO — a leitura da Performance (27/09/2026) —, na carteira e no período do filtro. */
  const funil = useRecurso(
    (sinal) =>
      obterFunilPorEstagio(
        contexto,
        {
          base: 'etapa',
          responsavel: cenEscolhido ?? undefined,
          carteira: carteiraEscolhida ?? undefined,
          de: intervalo?.de,
          ate: intervalo?.ate,
        },
        sinal,
      ),
    [contexto.empresa, contexto.usuario, cenEscolhido, carteiraEscolhida, intervalo?.de, intervalo?.ate],
  );
  const motivoDoFunil =
    funil.dados?.metricasSemDado.find((m) => m.metrica === 'funil' || m.metrica === 'funilNoPeriodo')?.motivo ??
    'O funil ainda não respondeu.';

  /** As perdas do CEN no período: os processos perdidos no funil e os formulários de venda perdida (só a principal). */
  const perdas = useRecurso(
    (sinal) =>
      obterVendasPerdidas(contexto, sinal, { responsavel: cenEscolhido ?? undefined, de: intervalo?.de, ate: intervalo?.ate }),
    [contexto.empresa, contexto.usuario, cenEscolhido, intervalo?.de, intervalo?.ate],
  );

  /** A meta de venda × o realizado da filial do cabeçalho, por consultor (#138), no período do filtro. */
  const meta = useRecurso(
    (sinal) =>
      obterMetaDaFilial(
        contexto,
        sinal,
        intervalo ? { inicial: intervalo.de.slice(0, 7), final: intervalo.ate.slice(0, 7) } : undefined,
      ),
    [contexto.empresa, contexto.usuario, intervalo?.de, intervalo?.ate],
  );

  /** O que o bloco da meta diz fechado: o total da filial com o período, ou por que não há número. */
  const resumoDaMeta = !meta.dados
    ? 'a cota da API Gestão de Negócios contra as vendas do ART, em máquinas'
    : meta.dados.origem === null
      ? 'o cadastro de metas da API Gestão de Negócios ainda não foi lido'
      : `${meta.dados.totais.realizadoMaquinas.toLocaleString('pt-BR')} de ` +
        `${meta.dados.totais.metaMaquinas.toLocaleString('pt-BR')} máquinas · ${meta.dados.periodo.texto} · ` +
        `${meta.dados.porConsultor.length} ${meta.dados.porConsultor.length === 1 ? 'consultor' : 'consultores'}`;

  const carteirasDaFilial = useMemo(() => resumo.dados?.itens ?? [], [resumo.dados]);
  const doRankingNaFilial = useMemo(() => carteirasDaFilial.filter(entraNoRanking), [carteirasDaFilial]);

  /** O nome do CEN escolhido: a cobertura por carteira traz o nome, e o seletor, a chave. */
  const responsaveis = useMemo(() => painel.dados?.responsaveis ?? [], [painel.dados]);
  const nomeDoCenEscolhido = responsaveis.find((r) => r.chave === cenEscolhido)?.nome ?? null;

  /** O RECORTE DA CLASSE E DA CARTEIRA: as carteiras do ranking que sobram. O CEN destaca, e não tira. */
  const carteirasDePessoa = useMemo(() => {
    const daClasse = classe ? (resumoDaClasse.dados?.itens ?? []) : carteirasDaFilial;
    return daClasse.filter((c) => entraNoRanking(c) && (!carteiraEscolhida || c.carteiraChave === carteiraEscolhida));
  }, [classe, resumoDaClasse.dados, carteirasDaFilial, carteiraEscolhida]);

  /** Quantas carteiras ficaram de fora do ranking, para a tela poder dizer. */
  const carteirasForaDoRanking = carteirasDaFilial.length - doRankingNaFilial.length;

  const cens = useMemo(() => porResponsavel(carteirasDePessoa).filter((c) => c.clientes > 0), [carteirasDePessoa]);

  /** Os números dos cartões: o recorte, e só o CEN escolhido quando há um. */
  const doRecorte = useMemo(
    () => (nomeDoCenEscolhido ? cens.filter((c) => c.responsavelNome === nomeDoCenEscolhido) : cens),
    [cens, nomeDoCenEscolhido],
  );

  const totais = useMemo(
    () => ({
      cens: doRecorte.length,
      clientes: doRecorte.reduce((s, c) => s + c.clientes, 0),
      em30: doRecorte.reduce((s, c) => s + c.em30, 0),
      nunca: doRecorte.reduce((s, c) => s + c.nunca, 0),
      censDaFilial: new Set(doRankingNaFilial.map((c) => c.responsavelNome)).size,
      vinculosDaFilial: carteirasDaFilial.reduce((s, c) => s + c.clientes, 0),
    }),
    [doRecorte, doRankingNaFilial, carteirasDaFilial],
  );

  /** As carteiras do seletor: as comerciais do ranking, do CEN escolhido quando há um. */
  const carteirasDoSeletor = useMemo(
    () =>
      doRankingNaFilial
        .filter((c) => !nomeDoCenEscolhido || c.responsavelNome === nomeDoCenEscolhido)
        .sort((a, b) => a.carteiraNome.localeCompare(b.carteiraNome, 'pt-BR')),
    [doRankingNaFilial, nomeDoCenEscolhido],
  );

  const ordenados = useMemo(() => {
    const chave = (c: LinhaDeCen) => {
      switch (ordem) {
        case 'nome':
          return c.responsavelNome;
        case 'clientes':
          return c.clientes;
        case 'cobertura30':
          return c.clientes > 0 ? c.em30 / c.clientes : -1;
        case 'cobertura90':
          return c.clientes > 0 ? c.em90 / c.clientes : -1;
        case 'nunca':
          return c.nunca;
      }
    };
    const lista = [...cens].sort((a, b) => {
      const x = chave(a);
      const y = chave(b);
      if (typeof x === 'string' || typeof y === 'string') return String(x).localeCompare(String(y));
      return (x as number) - (y as number);
    });
    // A ordem que abre é a mais útil: a menor cobertura de 30 dias primeiro —
    // quem precisa de atenção, e não quem já está bem.
    return descendente ? lista.reverse() : lista;
  }, [cens, ordem, descendente]);

  /**
   * A CARTEIRA DOS CENs, COMO NA MAQUETE: a maior cobertura em 30 dias primeiro, as N do seletor — e o CEN escolhido
   * sempre entra, em destaque, mesmo fora do corte.
   */
  const censNoGrafico = useMemo(() => {
    const porCobertura = [...cens].sort((a, b) => cobertura30(b) - cobertura30(a) || b.clientes - a.clientes);
    const cortados = quantosCens > 0 ? porCobertura.slice(0, quantosCens) : porCobertura;
    const escolhido = nomeDoCenEscolhido ? cens.find((c) => c.responsavelNome === nomeDoCenEscolhido) : undefined;
    return escolhido && !cortados.includes(escolhido) ? [...cortados, escolhido] : cortados;
  }, [cens, quantosCens, nomeDoCenEscolhido]);

  /** As barras do ranking, na métrica escolhida. Tudo do mesmo agregado. */
  const barrasDoRanking = useMemo(() => {
    const medir = (c: LinhaDeCen) => {
      switch (metricaDoRanking) {
        case 'clientes':
          return c.clientes;
        case 'em30':
          return c.em30;
        case 'cobertura30':
          return Math.round(cobertura30(c) * 100);
        case 'nunca':
          return c.nunca;
      }
    };
    const ordenadas = [...cens].map((c) => ({ cen: c, valor: medir(c) })).sort((x, y) => y.valor - x.valor);
    const topo = ordenadas.slice(0, BARRAS);
    const escolhido = nomeDoCenEscolhido ? ordenadas.find((o) => o.cen.responsavelNome === nomeDoCenEscolhido) : undefined;
    return escolhido && !topo.includes(escolhido) ? [...topo, escolhido] : topo;
  }, [cens, metricaDoRanking, nomeDoCenEscolhido]);

  const metrica = METRICAS.find((m) => m.id === metricaDoRanking)!;

  /** As classes do painel, só a escolhida quando há uma. */
  const classes = useMemo(
    () => (painel.dados?.painel.porClasse ?? []).filter((c) => !classe || c.classe === classe),
    [painel.dados, classe],
  );

  function trocarOrdem(campo: Ordem) {
    if (ordem === campo) setDescendente((d) => !d);
    else {
      setOrdem(campo);
      setDescendente(false);
    }
  }

  function trocarCen(chave: string | null) {
    setCenEscolhido(chave);
    // A CARTEIRA É DE UM CEN: trocar o CEN tira a carteira que não é dele.
    const nome = responsaveis.find((r) => r.chave === chave)?.nome;
    const daCarteira = doRankingNaFilial.find((c) => c.carteiraChave === carteiraEscolhida)?.responsavelNome;
    if (chave && carteiraEscolhida && daCarteira !== nome) setCarteiraEscolhida(null);
  }

  function limparFiltros() {
    setCenEscolhido(null);
    setPeriodo('fy');
    setClasse('');
    setCarteiraEscolhida(null);
  }

  const temFiltro = cenEscolhido !== null || periodo !== 'fy' || classe !== '' || carteiraEscolhida !== null;

  // SEM CARTEIRA AO ALCANCE, O NÚMERO É O TRAÇO COM O MOTIVO; LENDO, O CARTÃO PULSA E NÃO AFIRMA MOTIVO NENHUM.
  const lendoResumo = resumo.carregando || (classe !== '' && resumoDaClasse.carregando);
  const semCarteira = lendoResumo ? undefined : 'Sem carteira ao alcance deste recorte.';
  const nomeDoCen = painel.dados?.painel.responsavelNome ?? 'Todos os responsáveis';

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga pcen-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">
            Performance de CEN
            <InfoTooltip
              rotulo="O que a Performance de CEN mede"
              texto="Quem está cobrindo a carteira e quem não está — e a meta de venda de cada consultor contra as máquinas que ele vendeu."
            />
          </h1>
          <p className="page-subtitle">
            Acompanhe quem está cobrindo a carteira de clientes, o ritmo dos vínculos e a eficiência comercial dos CENs.
          </p>
        </div>
        <p className="dash-atualizado">
          <CalendarDays className="pcen-calendario" size={15} strokeWidth={2} aria-hidden="true" />
          {resumo.procedencia ? <DadosAtualizadosEm procedencia={resumo.procedencia} /> : 'Lendo a performance…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              resumo.recarregar();
              resumoDaClasse.recarregar();
              painel.recarregar();
              funil.recarregar();
              perdas.recarregar();
              meta.recarregar();
            }}
            disabled={resumo.carregando || painel.carregando}
            data-carregando={resumo.carregando || painel.carregando ? 'true' : 'false'}
            aria-label="Reler a performance"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES (01/10/2026): os quatro filtros da maquete, aplicados na hora — sem o botão "Aplicar"
          (decisão do Ricardo). O CEN, a classe e a carteira recortam a tela inteira; o período, o funil, as perdas e a
          meta. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="cen">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                CEN
                <InfoTooltip
                  texto="O responsável escolhido recorta os cartões, a cobertura por classe e o funil. Na carteira dos CENs, no ranking e na tabela ele aparece em destaque entre os outros — um ranking de uma barra não compara ninguém."
                  rotulo="O que o CEN escolhido muda"
                />
              </span>
              <select value={cenEscolhido ?? ''} onChange={(e) => trocarCen(e.target.value === '' ? null : e.target.value)}>
                <option value="">Todos os CENs</option>
                {responsaveis.map((r) => (
                  <option key={r.chave} value={r.chave}>
                    {r.nome}
                    {r.natureza === 'Departamento' ? ' (área)' : ''} · {r.carteiras} carteira
                    {r.carteiras === 1 ? '' : 's'}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="periodo">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Período
                <InfoTooltip
                  texto="O período muda o funil do CEN, as perdas e a meta × realizado. A cobertura da carteira é o retrato de hoje — contato em até 30 ou 90 dias — e não muda com ele."
                  rotulo="O que o período muda"
                />
              </span>
              <span className="pcen-campo-com-icone">
                <CalendarDays size={15} strokeWidth={2} aria-hidden="true" />
                <select value={periodo} onChange={(e) => setPeriodo(e.target.value as PeriodoId)}>
                  {PERIODOS.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.rotulo}
                    </option>
                  ))}
                </select>
              </span>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="classe">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Classe do cliente
                <InfoTooltip
                  texto="A curva ABC do cliente, apurada do faturamento. Quem não tem classe apurada conta como D — a mesma regra da cobertura por classe."
                  rotulo="Qual classe é esta"
                />
              </span>
              <select value={classe} onChange={(e) => setClasse(e.target.value)}>
                <option value="">Todas as classes</option>
                {CLASSES.map((c) => (
                  <option key={c} value={c}>
                    Classe {c}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="carteira">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Carteira</span>
              <select
                value={carteiraEscolhida ?? ''}
                onChange={(e) => setCarteiraEscolhida(e.target.value === '' ? null : e.target.value)}
                disabled={carteirasDoSeletor.length === 0}
              >
                <option value="">Todas as carteiras</option>
                {carteirasDoSeletor.map((c) => (
                  <option key={c.carteiraChave} value={c.carteiraChave}>
                    {c.carteiraNome} · {nomeCurto(c.responsavelNome)}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <div className="dash-filtros-acao">
            {/* SEMPRE À VISTA, como a maquete: sem filtro escolhido, o clique não muda nada — e o leitor de tela ouve isso. */}
            <button type="button" className="pcen-limpar" onClick={limparFiltros} aria-disabled={!temFiltro}>
              Limpar filtros
            </button>
          </div>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={resumo.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="CENs com carteira"
          icone={UsersRound}
          tom="demanda"
          valor={resumo.dados && !lendoResumo ? nº(totais.cens) : null}
          carregando={lendoResumo}
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? `de ${nº(totais.censDaFilial)} CENs da filial` : null}
          sobre="Responsáveis distintos nas carteiras comerciais do recorte. Pessoa e área entram; contas do sistema, do fornecedor e de teste não. O total é o da filial, sem os filtros."
        />
        <CartaoDeDecisao
          rotulo="Vínculos atendidos"
          icone={FileText}
          tom="mercado"
          valor={resumo.dados && !lendoResumo ? nº(totais.clientes) : null}
          carregando={lendoResumo}
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? `de ${nº(totais.vinculosDaFilial)} vínculos` : null}
          sobre="Vínculos cliente × carteira das carteiras comerciais com CEN, no recorte; metade dos clientes está em duas ou mais carteiras — por isso é contagem de vínculos, e não de clientes. O total é o de todas as carteiras da filial, inclusive o depósito de cadastro e a de teste."
        />
        <CartaoDeDecisao
          rotulo="Cobertura em 30 dias"
          icone={Target}
          tom="demanda"
          valor={resumo.dados && !lendoResumo && totais.clientes > 0 ? `${Math.round((totais.em30 / totais.clientes) * 100)}%` : null}
          carregando={lendoResumo}
          motivoSemDado={lendoResumo ? undefined : resumo.dados ? 'Sem vínculo no recorte: não há de quanto tirar o percentual.' : semCarteira}
          variacao={resumo.dados && totais.clientes > 0 ? `${nº(totais.em30)} de ${nº(totais.clientes)} vínculos` : null}
          sobre="Vínculos com contato nos últimos 30 dias (regra da BI de carteiras), sobre o total do recorte."
        />
        <CartaoDeDecisao
          rotulo="Nunca contatados"
          icone={UserRound}
          tom="oportunidade"
          valor={resumo.dados && !lendoResumo ? nº(totais.nunca) : null}
          carregando={lendoResumo}
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? 'sem contato no histórico' : null}
          sobre="Sem contato no histórico do Vórtice, pela regra da BI de carteiras."
        />
      </div>

      <MetricasSemDado metricas={resumo.dados?.metricasSemDado} />

      {/* ------------------------------------------------------------------ */}
      {/* A COBERTURA POR CLASSE — o painel do CEN                             */}
      {/*                                                                     */}
      {/* "Coberto" NÃO É UM NÚMERO REDONDO ESCOLHIDO POR ALGUÉM: é a cadência */}
      {/* que o negócio declarou para aquela classe naquela linha de negócio — */}
      {/* 180 dias em Venda de Máquinas, 360 em Peças e AMS, 120 em            */}
      {/* Prospecção. Onde a linha não declara cadência, o vínculo sai numa    */}
      {/* quarta faixa em vez de ser empurrado para um dos dois lados.         */}
      {/* ------------------------------------------------------------------ */}
      <section className="dash-secao pcen-cartao" data-bloco="secao-cen">
        <TituloDaSecao
          icone={<ChartNoAxesColumnIncreasing className="pcen-secao-icone" size={22} strokeWidth={2.6} aria-hidden="true" />}
          titulo="Cobertura por classe do cliente"
          subtitulo="Distribuição dos vínculos por classe de cliente e status de cobertura."
          metodologia="Coberto é o vínculo com contato dentro da cadência que o negócio declarou para a classe naquela linha de negócio — 180 dias em Venda de Máquinas, 360 em Peças e AMS, 120 em Prospecção. Onde a linha não declara cadência, o vínculo sai numa quarta faixa, que não é boa nem ruim. Três estados, e não dois: quem o CEN conhece e deixou vencer não é o mesmo problema que quem ele nunca procurou. A classe é a curva ABC do faturamento; sem classe apurada conta como D. Na classe A, fora da cadência e nunca contatados vêm em vermelho: é o cliente que mais pesa."
          acao={
            <label className="pcen-secao-acao pcen-seletor">
              <span className="cad-so-leitor">Como as barras escrevem cada faixa</span>
              <select value={emPercentual ? 'pct' : 'num'} onChange={(e) => setEmPercentual(e.target.value === 'pct')}>
                <option value="pct">Visualizar em %</option>
                <option value="num">Visualizar em número</option>
              </select>
            </label>
          }
        />

        {/* O PAINEL NÃO TEM TÍTULO À VISTA NA MAQUETE: o título e o recorte ficam para o leitor de tela. */}
        <PainelDoMomento
          titulo="O painel do CEN"
          data-bloco="cobertura-por-classe"
          subtitulo={
            painel.dados
              ? `${nomeDoCen} · ${painel.dados.painel.carteiras} carteira(s) · ${painel.dados.painel.clientes.toLocaleString('pt-BR')} vínculos`
              : 'O responsável escolhido na barra, ou todos.'
          }
        >
          {painel.carregando && <BlocoCarregando oQue="a cobertura por classe" />}
          {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}

          <MetricasSemDado metricas={painel.dados?.metricasSemDado} />

          {painel.dados && classes.length === 0 && !painel.carregando && (
            <BlocoVazio
              titulo="Nenhum vínculo neste recorte"
              texto="O CEN, a carteira e a classe escolhidos não têm vínculo de carteira comercial juntos. Limpe um dos filtros."
            />
          )}

          {painel.dados && classes.length > 0 && (
            <>
              <div className="pcen-classes">
                <BarrasPorClasse classes={classes} emPercentual={emPercentual} />
                <TabelaPorClasse classes={classes} />
              </div>

              <PrincipalAlerta classes={classes} />

              {/* OS PROCESSOS SAÍRAM DAQUI (27/09/2026): "0 ganhos · 0 perdidos · 0 abertos" contava processo.Processo,
                  que só a onda 2 carrega. O funil do CEN, pelo responsável do processo no Vórtice, está no cartão do fim. */}
              <p className="v360-nota pcen-nota">
                Faturamento dos clientes da carteira:{' '}
                <strong>
                  {painel.dados.painel.faturamentoDaCarteira.toLocaleString('pt-BR', {
                    style: 'currency',
                    currency: 'BRL',
                    maximumFractionDigits: 0,
                  })}
                </strong>
                <InfoTooltip
                  texto="O faturamento dos CLIENTES da carteira, e não das vendas desta pessoa: a carga do Protheus agrega a nota por cliente, filial e mês e ainda não lê o vendedor dela."
                  rotulo="O que é este faturamento"
                />
              </p>
            </>
          )}
        </PainelDoMomento>
      </section>

      {/* ------------------------------------------------------------------ */}
      {/* A CARTEIRA DE CADA CEN — a divisão e o ranking, lado a lado          */}
      {/* ------------------------------------------------------------------ */}
      <section className="dash-secao pcen-secao-dupla" data-bloco="secao-carteiras">
        {/* A SEÇÃO NÃO TEM TÍTULO À VISTA NA MAQUETE: cada painel tem o seu. O título fica para o leitor de tela. */}
        <TituloDaSecao
          titulo="A carteira de cada CEN"
          subtitulo="Quem está cobrindo e quem não está, responsável a responsável."
        />

        <div className="pcen-paineis">
          <PainelDoMomento
            titulo="Carteira dos CENs"
            area="pcen-carteira"
            data-bloco="carteira-dividida"
            icone={<UsersRound className="pcen-painel-icone" size={22} strokeWidth={2.2} aria-hidden="true" />}
            subtitulo="Distribuição da carteira por faixa de cobertura (em % dos vínculos)."
            dica={`As mesmas faixas e cores da Cobertura de Carteira: até 30 dias, de 31 a 90, mais de 90 e nunca contatado. A ordem é a maior cobertura em 30 dias primeiro${cens.length > censNoGrafico.length ? `, as ${censNoGrafico.length} de ${cens.length}` : ''}; o CEN escolhido na barra entra sempre, em destaque. Somar carteira por responsável é exato: cada carteira tem um responsável só.`}
            direita={
              <label className="pcen-seletor">
                <span className="cad-so-leitor">Quantos CENs</span>
                <select value={quantosCens} onChange={(e) => setQuantosCens(Number(e.target.value))}>
                  {QUANTOS_CENS.map((q) => (
                    <option key={q.valor} value={q.valor}>
                      {q.rotulo}
                    </option>
                  ))}
                </select>
              </label>
            }
          >
            {lendoResumo && <BlocoCarregando oQue="a divisão da carteira" />}
            {!lendoResumo && censNoGrafico.length === 0 && resumo.dados && (
              <BlocoVazio titulo="Nenhum CEN neste recorte" texto="A carteira dos CENs sai das carteiras comerciais com responsável." />
            )}
            {!lendoResumo && censNoGrafico.length > 0 && (
              <CarteiraDosCens cens={censNoGrafico} escolhido={nomeDoCenEscolhido} />
            )}
          </PainelDoMomento>

          <PainelDoMomento
            titulo="Ranking por métrica"
            area="pcen-ranking"
            data-bloco="ranking"
            icone={<Trophy className="pcen-painel-icone" size={22} strokeWidth={2.2} aria-hidden="true" />}
            subtitulo={`Top ${Math.min(BARRAS, cens.length)} CENs por ${metrica.titulo}.`}
            dica="A cor diz o sentido, e não o mérito: em nunca contatados, barra grande é problema, e ela é vermelha. Nas demais métricas, maior é melhor. O gráfico e o ranking saem do mesmo agregado da tabela em números."
            direita={
              <label className="pcen-seletor">
                <span className="cad-so-leitor">Métrica do ranking</span>
                <select value={metricaDoRanking} onChange={(e) => setMetricaDoRanking(e.target.value as MetricaId)}>
                  {METRICAS.map((m) => (
                    <option key={m.id} value={m.id}>
                      {m.rotulo}
                    </option>
                  ))}
                </select>
              </label>
            }
          >
            {lendoResumo && <BlocoCarregando oQue="o ranking" />}
            {resumo.dados && !lendoResumo && barrasDoRanking.length === 0 && !resumo.erro && (
              <BlocoVazio
                titulo="Nenhum CEN com carteira neste recorte"
                texto="O ranking sai da cobertura por carteira; sem carteira com responsável não há barra."
              />
            )}
            {!lendoResumo && barrasDoRanking.length > 0 && (
              <RankingDosCens
                barras={barrasDoRanking.map(({ cen, valor }) => ({ cen, valor }))}
                unidade={metrica.unidade}
                vermelho={metricaDoRanking === 'nunca'}
                escolhido={nomeDoCenEscolhido}
              />
            )}
          </PainelDoMomento>
        </div>
      </section>

      {/* OS QUATRO CARTÕES DO FIM, como a maquete: fechados, e um abre por vez embaixo deles, na largura toda. */}
      <Recolhiveis
        cartoes={[
          {
            id: 'numeros',
            icone: <Sheet size={22} strokeWidth={2} aria-hidden="true" />,
            titulo: 'Cobertura por CEN, em números',
            selo: `${nº(cens.length)} CENs`,
            texto: 'Veja a relação completa de CENs com quantidade de vínculos e status de cobertura.',
            corpo: (
              <TabelaDosCens
                cens={ordenados}
                carteiras={carteirasDePessoa.length}
                escolhido={nomeDoCenEscolhido}
                ordem={ordem}
                descendente={descendente}
                aoOrdenar={trocarOrdem}
                carregando={lendoResumo}
                erro={resumo.erro}
                aoTentarDeNovo={resumo.recarregar}
                filial={contexto.empresa}
              />
            ),
          },
          {
            id: 'meta',
            icone: <ChartColumnIncreasing size={22} strokeWidth={2.4} aria-hidden="true" />,
            titulo: meta.dados?.alcance === 'Proprios' ? 'Sua meta de venda × o realizado' : 'Meta de venda × realizado, por consultor',
            selo: meta.dados && meta.dados.origem !== null ? `${nº(meta.dados.porConsultor.length)} consultores` : null,
            texto: 'Acompanhe o desempenho de cada consultor em relação às metas.',
            corpo: (
              <BlocoDaMeta meta={meta} resumoDaMeta={resumoDaMeta} filial={contexto.empresa} />
            ),
          },
          {
            id: 'funil',
            icone: <Funnel size={22} strokeWidth={2} aria-hidden="true" />,
            titulo: 'Funil do CEN — fluxo por estágio',
            selo: funil.dados ? funil.dados.periodo.texto : null,
            texto: 'As etapas que o CEN fez andar no período, e as perdas.',
            corpo: (
              <BlocoDoFunil
                funil={funil}
                perdas={perdas}
                motivoDoFunil={motivoDoFunil}
                nomeDoCen={nomeDoCen}
              />
            ),
          },
          {
            id: 'lacunas',
            icone: (
              <span className="pcen-icone-info" aria-hidden="true">
                <Info size={16} strokeWidth={2.6} />
              </span>
            ),
            titulo: 'O que esta métrica não faz',
            selo: null,
            texto: 'Entenda o que está (e o que não está) contemplado nesta análise.',
            corpo: (
              <OQueEstaMetricaNaoFaz
                carteirasForaDoRanking={carteirasForaDoRanking}
                carteiras={carteirasDaFilial.length}
              />
            ),
          },
        ]}
      />
    </PaginaDoPainel>
  );
}

/**
 * A COBERTURA POR CLASSE EM BARRAS DE 100%, como a maquete: a faixa com o percentual (ou o número) escrito dentro,
 * o eixo de 0 a 100% e a legenda. O leitor de tela ouve a linha inteira.
 */
function BarrasPorClasse({ classes, emPercentual }: { classes: CoberturaPorClasse[]; emPercentual: boolean }) {
  return (
    <div className="pcen-barras" data-bloco="barras-por-classe">
      <ol className="pcen-barras-lista">
        {classes.map((c) => (
          <li key={c.classe} className="pcen-barras-linha">
            <span className="pcen-barras-nome" aria-hidden="true">
              Classe {c.classe}
            </span>
            <span className="pcen-barras-trilho" aria-hidden="true">
              {FAIXAS_DE_COBERTURA.map((f) => {
                const valor = f.medir(c);
                const p = c.clientes > 0 ? (100 * valor) / c.clientes : 0;
                if (p <= 0) return null;
                return (
                  <span key={f.nome} className="pcen-barras-faixa" style={{ width: `${p}%`, background: f.cor }}>
                    {p >= 6 ? (emPercentual ? `${Math.round(p)}%` : nº(valor)) : ''}
                  </span>
                );
              })}
            </span>
            <span className="cad-so-leitor">
              Classe {c.classe}, {nº(c.clientes)} vínculos —{' '}
              {FAIXAS_DE_COBERTURA.map((f) => `${f.nome.toLowerCase()}: ${percentual(f.medir(c), c.clientes)}`).join(', ')}
            </span>
          </li>
        ))}
      </ol>
      <div className="pcen-eixo" aria-hidden="true">
        {[0, 20, 40, 60, 80, 100].map((v) => (
          <span key={v} style={{ left: `${v}%` }}>
            {v}%
          </span>
        ))}
      </div>
      <ul className="pcen-legenda" aria-label="Legenda da cobertura por classe">
        {FAIXAS_DE_COBERTURA.map((f) => (
          <li key={f.nome}>
            <i style={{ background: f.cor }} aria-hidden="true" />
            {f.nome}
          </li>
        ))}
      </ul>
    </div>
  );
}

/** A TABELA AO LADO DAS BARRAS: o número, a barrinha e o percentual de cada estado, como a maquete. */
function TabelaPorClasse({ classes }: { classes: CoberturaPorClasse[] }) {
  const colunas = [
    { nome: 'Cobertos', cor: '#1B873F', medir: (c: CoberturaPorClasse) => c.cobertos, alerta: false },
    { nome: 'Fora da cadência', cor: '#F39A1E', medir: (c: CoberturaPorClasse) => c.foraDaCadencia, alerta: true },
    { nome: 'Nunca contatados', cor: '#E53935', medir: (c: CoberturaPorClasse) => c.nuncaContatados, alerta: true },
  ];
  return (
    <div className="mom-tabela-rolagem pcen-tabela-classes">
      <table className="mom-tabela">
        <caption className="cad-so-leitor">Cobertura por classe do cliente</caption>
        <thead>
          <tr>
            <th scope="col">Classe</th>
            <th scope="col">Cadência</th>
            <th scope="col" className="mom-num">Vínculos</th>
            {colunas.map((c) => (
              <th key={c.nome} scope="colgroup" colSpan={3} className="pcen-grupo">
                <span>{c.nome}</span>
                <span aria-hidden="true">%</span>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {classes.map((c) => (
            <tr key={c.classe}>
              <th scope="row">{c.classe}</th>
              <td>
                {/* Cadência nula com vínculo medido quer dizer que a classe aparece em linhas de negócio com prazos
                    diferentes — 180 em Máquinas, 360 em Peças. Escrever um dos dois seria afirmar um prazo que não vale
                    para todo mundo da linha. */}
                {c.diasDeCadencia ? (
                  `≤ ${c.diasDeCadencia} dias`
                ) : c.semCadenciaDeclarada === c.clientes ? (
                  <span className="cad-nada">não declarada</span>
                ) : (
                  <span className="cad-nada">varia por linha</span>
                )}
              </td>
              <td className="mom-num">{nº(c.clientes)}</td>
              {colunas.map((col) => {
                const valor = col.medir(c);
                const p = c.clientes > 0 ? Math.round((100 * valor) / c.clientes) : 0;
                return [
                  <td key={`${col.nome}-n`} className="mom-num" data-alerta={col.alerta && c.classe === 'A' && valor > 0 ? 'true' : undefined}>
                    {nº(valor)}
                  </td>,
                  <td key={`${col.nome}-b`} className="pcen-celula-barra" aria-hidden="true">
                    <span className="pcen-mini-trilho">
                      <span style={{ width: `${p}%`, background: col.cor }} />
                    </span>
                  </td>,
                  <td key={`${col.nome}-p`} className="mom-num pcen-pct">
                    {c.clientes > 0 ? `${p}%` : '—'}
                  </td>,
                ];
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

/**
 * O PRINCIPAL ALERTA da maquete: a classe com o maior volume de vínculos fora da cadência e nunca contatados, com as
 * duas fatias dela e o peso dela no total. Com uma classe só na tela (o filtro), não há o que comparar, e ele some.
 */
function PrincipalAlerta({ classes }: { classes: CoberturaPorClasse[] }) {
  if (classes.length < 2) return null;
  const total = classes.reduce((s, c) => s + c.clientes, 0);
  const pior = [...classes].sort(
    (a, b) => b.foraDaCadencia + b.nuncaContatados - (a.foraDaCadencia + a.nuncaContatados),
  )[0];
  if (!pior || pior.foraDaCadencia + pior.nuncaContatados === 0 || total === 0) return null;
  return (
    <div className="pcen-alerta" role="note" data-bloco="principal-alerta">
      <TriangleAlert className="pcen-alerta-icone" size={26} strokeWidth={2.2} aria-hidden="true" />
      <strong className="pcen-alerta-rotulo">Principal alerta</strong>
      <div className="pcen-alerta-texto">
        <strong>
          Classe {pior.classe} concentra o maior volume de vínculos fora da cadência ({percentual(pior.foraDaCadencia, pior.clientes)}) e
          nunca contatados ({percentual(pior.nuncaContatados, pior.clientes)}).
        </strong>
        <span>
          Essa classe representa {percentual(pior.clientes, total)} de todos os vínculos e deve ser priorizada nas ações
          comerciais.
        </span>
      </div>
    </div>
  );
}

/** A CARTEIRA DOS CENs: uma barra de 100% por pessoa, com as quatro faixas de tempo sem contato, como a maquete. */
function CarteiraDosCens({ cens, escolhido }: { cens: LinhaDeCen[]; escolhido: string | null }) {
  return (
    <div className="pcen-cens">
      <ol className="pcen-cens-lista">
        {cens.map((c) => (
          <li key={c.responsavelNome} className="pcen-cens-linha" data-escolhido={c.responsavelNome === escolhido ? 'true' : undefined}>
            <span className="pcen-cens-nome" aria-hidden="true">
              {nomeCurto(c.responsavelNome)}
            </span>
            <span className="pcen-cens-trilho" aria-hidden="true">
              {FAIXAS_DE_CONTATO.map((f) => {
                const p = c.clientes > 0 ? (100 * f.medir(c)) / c.clientes : 0;
                if (p <= 0) return null;
                return (
                  <span key={f.nome} className="pcen-cens-faixa" style={{ width: `${p}%`, background: f.cor }}>
                    {p >= 3 ? `${Math.round(p)}%` : ''}
                  </span>
                );
              })}
            </span>
            <span className="cad-so-leitor">
              {c.responsavelNome}, {nº(c.clientes)} vínculos —{' '}
              {FAIXAS_DE_CONTATO.map((f) => `${f.nome.toLowerCase()}: ${percentual(f.medir(c), c.clientes)}`).join(', ')}
            </span>
          </li>
        ))}
      </ol>
      <ul className="pcen-legenda" aria-label="Legenda da carteira dos CENs">
        {FAIXAS_DE_CONTATO.map((f) => (
          <li key={f.nome}>
            <i style={{ background: f.cor }} aria-hidden="true" />
            {f.nome}
          </li>
        ))}
      </ul>
    </div>
  );
}

/** O RANKING EM BARRAS, como a maquete: o primeiro em verde-escuro, os outros clareando, o valor no fim da barra. */
function RankingDosCens({
  barras,
  unidade,
  vermelho,
  escolhido,
}: {
  barras: { cen: LinhaDeCen; valor: number }[];
  unidade: string;
  vermelho: boolean;
  escolhido: string | null;
}) {
  const maior = Math.max(1, ...barras.map((b) => b.valor));
  const escrever = (v: number) => (unidade === '%' ? `${v}%` : nº(v));
  return (
    <ol className="pcen-ranking" data-vermelho={vermelho ? 'true' : undefined}>
      {barras.map(({ cen, valor }, i) => (
        <li
          key={cen.responsavelNome}
          className="pcen-ranking-linha"
          data-escolhido={cen.responsavelNome === escolhido ? 'true' : undefined}
          style={{ '--pcen-tom': `${Math.round((i / Math.max(1, barras.length - 1)) * 55)}%` } as CSSProperties}
        >
          <span className="pcen-ranking-nome" aria-hidden="true">
            {nomeCurto(cen.responsavelNome)}
          </span>
          <span className="pcen-ranking-trilho" aria-hidden="true">
            <span className="pcen-ranking-barra" data-primeira={i === 0 ? 'true' : undefined} style={{ width: `${(100 * valor) / maior}%` }} />
            <span className="pcen-ranking-valor">{escrever(valor)}</span>
          </span>
          <span className="cad-so-leitor">
            {i + 1}º, {cen.responsavelNome}: {escrever(valor)}
            {unidade === '%' ? '' : ` ${unidade}`}, {nº(cen.carteiras)} {cen.carteiras === 1 ? 'carteira' : 'carteiras'}
          </span>
        </li>
      ))}
    </ol>
  );
}

type CartaoRecolhivel = {
  id: string;
  icone: ReactNode;
  titulo: string;
  selo: string | null;
  texto: string;
  corpo: ReactNode;
};

/**
 * OS CARTÕES DO FIM, como a maquete: fechados lado a lado, e o que abre mostra o conteúdo embaixo deles, na largura
 * toda — a tabela dos CENs não cabe num quarto da tela. Os quatro conteúdos ficam no documento, escondidos.
 */
function Recolhiveis({ cartoes }: { cartoes: CartaoRecolhivel[] }) {
  const [aberto, setAberto] = useState<string | null>(null);
  return (
    <div className="pcen-recolhiveis" data-bloco="recolhiveis">
      <div className="pcen-recolhiveis-linha">
        {cartoes.map((c) => (
          <button
            key={c.id}
            type="button"
            className="pcen-recolhivel"
            data-recolhivel={c.id}
            aria-expanded={aberto === c.id}
            aria-controls={`pcen-recolhivel-${c.id}`}
            onClick={() => setAberto((a) => (a === c.id ? null : c.id))}
          >
            <span className="pcen-recolhivel-icone">{c.icone}</span>
            <span className="pcen-recolhivel-corpo-do-botao">
              <span className="pcen-recolhivel-titulo">
                {c.titulo}
                {c.selo && <span className="pcen-recolhivel-selo">{c.selo}</span>}
              </span>
              <span className="pcen-recolhivel-texto">{c.texto}</span>
            </span>
            <ChevronDown className="pcen-recolhivel-seta" size={18} strokeWidth={2.2} aria-hidden="true" />
          </button>
        ))}
      </div>
      {cartoes.map((c) => (
        <section
          key={c.id}
          id={`pcen-recolhivel-${c.id}`}
          className="pcen-recolhivel-aberto"
          data-recolhivel-corpo={c.id}
          aria-label={c.titulo}
          hidden={aberto !== c.id}
        >
          {c.corpo}
        </section>
      ))}
    </div>
  );
}

/** A tabela inteira de cada CEN — o número exato por trás dos dois gráficos. */
function TabelaDosCens({
  cens,
  carteiras,
  escolhido,
  ordem,
  descendente,
  aoOrdenar,
  carregando,
  erro,
  aoTentarDeNovo,
  filial,
}: {
  cens: LinhaDeCen[];
  carteiras: number;
  escolhido: string | null;
  ordem: Ordem;
  descendente: boolean;
  aoOrdenar: (o: Ordem) => void;
  carregando: boolean;
  erro: Error | null;
  aoTentarDeNovo: () => void;
  filial: string;
}) {
  return (
    <>
      <p className="pcen-recolhivel-resumo">
        os {nº(cens.length)} responsáveis do recorte, somados das {nº(carteiras)} carteiras contadas no banco
      </p>
      {carregando && <BlocoCarregando oQue="a cobertura por CEN" />}
      {erro && <BlocoErro erro={erro} aoTentarDeNovo={aoTentarDeNovo} />}

      {!carregando && cens.length === 0 && !erro && (
        <BlocoVazio
          titulo="Nenhuma carteira com responsável neste recorte"
          texto="A carteira carrega o CEN responsável desde a carga de 2026. Se não aparece ninguém, confira a filial escolhida no cabeçalho e os filtros."
        />
      )}

      {cens.length > 0 && (
        <>
          <div className="mom-tabela-rolagem cad-so-largo">
            <table className="mom-tabela">
              <caption className="cad-so-leitor">Cobertura de carteira por CEN da filial {filial}</caption>
              <thead>
                <tr>
                  {COLUNAS.map((coluna) => (
                    <th
                      key={coluna.rotulo}
                      scope="col"
                      className={coluna.ordem === 'nome' ? undefined : 'mom-num'}
                      aria-sort={ordem === coluna.ordem ? (descendente ? 'descending' : 'ascending') : undefined}
                    >
                      <button type="button" className="cad-th-ordenar" onClick={() => aoOrdenar(coluna.ordem)}>
                        {coluna.rotulo}
                        <span aria-hidden="true">{ordem === coluna.ordem ? (descendente ? '▾' : '▴') : ''}</span>
                      </button>
                    </th>
                  ))}
                  <th scope="col" className="mom-num">Último contato na carteira</th>
                </tr>
              </thead>
              <tbody>
                {cens.map((c) => (
                  <tr key={c.responsavelNome} data-escolhido={c.responsavelNome === escolhido ? 'true' : undefined}>
                    <th scope="row">
                      <div className="cad-link-forte">{c.responsavelNome}</div>
                      <div className="cad-sub">
                        {c.carteiras} {c.carteiras === 1 ? 'carteira' : 'carteiras'} · {c.linhas.join(', ')}
                      </div>
                    </th>
                    <td className="mom-num">{nº(c.clientes)}</td>
                    <td className="mom-num">
                      <Fatia parte={c.em30} todo={c.clientes} />
                    </td>
                    <td className="mom-num">
                      <Fatia parte={c.em90} todo={c.clientes} />
                    </td>
                    <td className="mom-num">
                      <span className={c.nunca > 0 ? 'cad-atencao' : undefined}>{nº(c.nunca)}</span>
                    </td>
                    <td className="mom-num">
                      {c.ultimoContatoEm ? formatarData(c.ultimoContatoEm) : <span className="cad-nada">nunca</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="cad-fichas cad-so-estreito">
            {cens.map((c) => (
              <div className="cad-ficha" key={c.responsavelNome}>
                <div className="cad-ficha-titulo">{c.responsavelNome}</div>
                <div className="cad-sub">
                  {c.carteiras} {c.carteiras === 1 ? 'carteira' : 'carteiras'} · {c.linhas.join(', ')}
                </div>
                <div className="cad-ficha-linha">
                  <span className="cad-ficha-rotulo">Vínculos</span>
                  <span className="cad-ficha-valor">{nº(c.clientes)}</span>
                </div>
                <div className="cad-ficha-linha">
                  <span className="cad-ficha-rotulo">Contato em 30 dias</span>
                  <span className="cad-ficha-valor">
                    <Fatia parte={c.em30} todo={c.clientes} />
                  </span>
                </div>
                <div className="cad-ficha-linha">
                  <span className="cad-ficha-rotulo">Nunca contatados</span>
                  <span className="cad-ficha-valor">{nº(c.nunca)}</span>
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </>
  );
}

/**
 * A META DE VENDA × O REALIZADO, POR CONSULTOR (#138, 27/09/2026).
 *
 * Sem a rotina das metas ter rodado, o bloco diz que o cadastro não foi lido — e não "meta zero". O 403 é falta de
 * Meta.Ler, e tentar de novo não muda nada: o bloco não oferece o botão.
 */
function BlocoDaMeta({
  meta,
  resumoDaMeta,
  filial,
}: {
  meta: Leitura<MetaERealizadoDaFilial>;
  resumoDaMeta: string;
  filial: string;
}) {
  return (
    <>
      <p className="pcen-recolhivel-resumo">{resumoDaMeta}</p>
      {meta.carregando && <BlocoCarregando oQue="a meta de venda" />}
      {meta.erro && (
        <BlocoErro
          erro={meta.erro}
          aoTentarDeNovo={meta.erro instanceof ErroDaApi && meta.erro.status === 403 ? undefined : meta.recarregar}
        />
      )}

      {/* O VAZIO DIZ DE QUEM É (revisão do PR #248): no alcance Próprios, "esta filial não tem meta" seria falso — é a
          pessoa que não tem, ou o login dela que não casa, e a lacuna logo abaixo diz qual dos dois. */}
      {meta.dados && meta.dados.porConsultor.length === 0 && (
        <BlocoVazio
          titulo={
            meta.dados.origem === null
              ? 'O cadastro de metas ainda não foi lido'
              : meta.dados.alcance === 'Proprios'
                ? `Nenhuma meta nem venda de máquina em seu nome em ${meta.dados.periodo.texto}`
                : `Nenhuma meta nem venda de máquina em ${meta.dados.periodo.texto}`
          }
          texto={
            meta.dados.origem === null
              ? 'A rotina das metas da API Gestão de Negócios ainda não rodou. Sem ela não há meta para comparar — e o CRM não mostra zero no lugar.'
              : meta.dados.alcance === 'Proprios'
                ? 'O cadastro da API Gestão de Negócios foi lido, e não há meta sua nem venda do ART em seu nome pela sua filial no período. Se o seu login não casa com a GN ou com o ART, a nota abaixo diz.'
                : 'O cadastro da API Gestão de Negócios foi lido, e esta filial não tem meta de máquina nem venda do ART no período.'
          }
        />
      )}

      {meta.dados && meta.dados.porConsultor.length > 0 && (
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela">
            <caption className="cad-so-leitor">
              Meta de venda × realizado por consultor da filial {filial}, {meta.dados.periodo.texto}
            </caption>
            <thead>
              <tr>
                <th scope="col">Consultor</th>
                <th scope="col" className="mom-num">Meta (máq.)</th>
                <th scope="col" className="mom-num">Realizado (máq.)</th>
                <th scope="col" className="mom-num">Atingimento</th>
              </tr>
            </thead>
            <tbody>
              {meta.dados.porConsultor.map((c) => (
                <tr key={c.consultor}>
                  <th scope="row">
                    <div className="cad-link-forte">{c.consultor}</div>
                    {!c.temConta && <div className="cad-sub">sem conta no CRM</div>}
                  </th>
                  <td className="mom-num">{nº(c.meta)}</td>
                  <td className="mom-num">{nº(c.realizado)}</td>
                  <td className="mom-num">
                    {c.meta > 0 ? `${Math.round((c.realizado / c.meta) * 100)}%` : <span className="cad-nada">sem meta no período</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {meta.dados && (
        <p className="cad-sub">
          Meta: a cota da API Gestão de Negócios, em máquinas. Realizado: as vendas do ART em que o
          consultor é o vendedor, só as entregues e pelo mês da entrega, como a Gestão de Negócios
          {meta.dados.alcance === 'Filial'
            ? ' — a venda sem vendedor conta no total da filial e em ninguém'
            : ', vendas pela sua filial — a que você fez por outra filial conta lá'}
          .
          Período: {meta.dados.periodo.texto}
          {meta.dados.periodo.ehOPadrao ? ', o ano fiscal até o último mês fechado' : ''}.
        </p>
      )}

      <MetricasSemDado metricas={meta.dados?.metricasSemDado} />
    </>
  );
}

/**
 * O FUNIL DO CEN — o fluxo por estágio e as perdas (27/09/2026), no quarto cartão do fim desde a maquete (decisão do
 * Ricardo, 01/10/2026). A Performance usa o FLUXO: as etapas que o CEN fez andar no período, qualquer que seja a
 * abertura do processo. O dono é o responsável do processo no Vórtice (UsuResponsavel).
 */
function BlocoDoFunil({
  funil,
  perdas,
  motivoDoFunil,
  nomeDoCen,
}: {
  funil: Leitura<FunilPorEstagio>;
  perdas: Leitura<VendasPerdidas>;
  motivoDoFunil: string;
  nomeDoCen: string;
}) {
  return (
    <>
      <p className="pcen-recolhivel-resumo">
        {funil.dados
          ? `${nomeDoCen} · etapas alcançadas em ${funil.dados.periodo.texto}`
          : 'as etapas alcançadas no período, pelo responsável do processo no Vórtice'}
        <InfoTooltip
          rotulo="Como ler o funil do CEN"
          texto="O fluxo conta as etapas que o CEN fez andar no período, qualquer que seja a abertura do processo. As perdas são os processos perdidos no funil e os formulários de venda perdida — só a principal. A carteira e a classe do filtro não recortam as perdas: o formulário não diz a carteira."
        />
      </p>
      {funil.carregando && <BlocoCarregando oQue="o funil do CEN" />}
      {funil.erro && <BlocoErro erro={funil.erro} aoTentarDeNovo={funil.recarregar} />}

      {funil.dados && (
        <>
          <div className="mom-tabela-rolagem">
            <table className="mom-tabela">
              <caption className="cad-so-leitor">O funil do CEN, pelo fluxo</caption>
              <thead>
                <tr>
                  <th scope="col">Estágio</th>
                  <th scope="col" className="mom-num">Processos</th>
                  <th scope="col" className="mom-num">Sobre o anterior</th>
                </tr>
              </thead>
              <tbody>
                {ESTAGIOS.map(({ estagio, nome }) => {
                  const e = funil.dados!.estagios.find((x) => x.estagio === estagio);
                  const ausente = <ValorAusente oQue={`o estágio ${nome}`} motivo={motivoDoFunil} />;
                  return (
                    <tr key={estagio}>
                      <th scope="row">{nome}</th>
                      <td className="mom-num">{e ? nº(e.processos) : ausente}</td>
                      <td className="mom-num">
                        {e
                          ? e.percentualSobreOAnterior !== null
                            ? `${e.percentualSobreOAnterior.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`
                            : '—'
                          : ausente}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
          <p className="v360-nota">
            <strong>Perdas no período:</strong>{' '}
            {perdas.dados ? (
              `${nº(perdas.dados.processosPerdidos)} processos perdidos · ` +
              `${nº(perdas.dados.registradas)} com o formulário de venda perdida` +
              (perdas.dados.porMotivo[0] ? ` · mais comum: ${perdas.dados.porMotivo[0].nome}` : '')
            ) : (
              <ValorAusente oQue="as perdas" motivo={perdas.erro?.message ?? 'Lendo as vendas perdidas.'} />
            )}
          </p>
        </>
      )}
    </>
  );
}

/**
 * O QUE ESTA MÉTRICA NÃO FAZ — o que a tela mostrava e não tem como sustentar, a comparação com o período anterior da
 * maquete e as carteiras que ficam fora do ranking. Excluir sem avisar é a mesma família de defeito que mostrar número
 * sem dizer de onde ele vem.
 */
function OQueEstaMetricaNaoFaz({ carteirasForaDoRanking, carteiras }: { carteirasForaDoRanking: number; carteiras: number }) {
  return (
    <div className="cad-fichas">
      <LacunaConhecida
        metrica="A comparação com o período anterior"
        motivo={
          'A maquete compara cada cartão com o período anterior ("+12% vs. período anterior"). O CRM guarda a data do ' +
          'último contato de cada vínculo, e não a cobertura de cada dia: não há o retrato de 30 dias atrás para ' +
          'comparar. Os cartões mostram o retrato de hoje.'
        }
      />
      {/* A FRASE ANTIGA DIZIA QUE O FATURAMENTO "PAROU EM 11/04/2025" — era a cópia que o Vórtice
          recebia (EXT_NFS), e não o faturamento. O do Protheus está no CRM, lido direto da SD2 pela
          rotina de faturamento (#233). O que falta de verdade é o recorte por vendedor (27/09/2026). */}
      <LacunaConhecida
        metrica="Faturamento por CEN e ranking de vendas"
        motivo={
          'O faturamento do Protheus está no CRM, mas por cliente, filial e mês: a carga agrega a ' +
          'nota e ainda não lê o vendedor dela (F2_VEND1), e a carteira não diz quem vendeu cada ' +
          'nota. Sem esse recorte não há faturamento POR CEN nem ranking de vendas — o que o painel ' +
          'mostra é o faturamento dos CLIENTES da carteira de cada um.'
        }
      />
      {/* "PROCESSOS E TAREFAS POR CEN" DIZIA QUE FALTAVA ROTA (27/09/2026): os processos por CEN estão no funil do
          CEN, no cartão do fim, pelo responsável do processo no Vórtice. O que continua faltando é a agenda. */}
      <LacunaConhecida
        metrica="Tarefas por CEN"
        motivo={
          'Os processos de cada CEN estão no cartão "Funil do CEN", pelo responsável do processo no Vórtice. As ' +
          'tarefas não: a agenda do Vórtice ainda não é trazida para o CRM, e sem ela não há pendências nem ' +
          'atrasos por pessoa para contar.'
        }
      />
      <LacunaConhecida
        metrica="Tempo médio de atendimento"
        motivo={
          'A coluna Duracao existe na origem e é ZERO em 122.812 de 122.812 interações do ' +
          'período. Teve 122.812 oportunidades de ser preenchida no ano e não foi preenchida ' +
          // A citação "(documento 25, §5)" saiu do texto pelo mesmo motivo das outras.
          'nenhuma vez. Qualquer métrica de produtividade por tempo é impossível hoje.'
        }
      />
      {/* O QUE FICOU DE FORA DO RANKING, DITO NA TELA: quem conhece a base vai procurar o INT.MERCADO no ranking e
          precisa saber por que ele não está lá. */}
      {carteirasForaDoRanking > 0 && (
        <MetricasSemDado
          titulo="O que este ranking deixa de fora"
          metricas={[
            {
              metrica: 'carteirasForaDoRanking',
              motivo:
                `${carteirasForaDoRanking} de ${carteiras} carteiras não entram neste ` +
                'ranking: ou a carteira é depósito de cadastro e não carteira de venda, ou o ' +
                'responsável não é operação — conta do próprio sistema, do fornecedor ou de ' +
                'teste. Área entra: a Inteligência de Mercado aparece identificada como tal. ' +
                'Nada some — tudo continua inteiro na Cobertura de Carteira.',
            },
          ]}
        />
      )}
    </div>
  );
}

/** A fração, quando ela tem denominador. Zero vínculo não vira "0%". */
function percentual(parte: number, todo: number): string {
  if (todo <= 0) return '—';
  return `${Math.round((parte / todo) * 100)}%`;
}

/** Uma fatia com o denominador escrito. Sem denominador, o percentual não significa nada. */
function Fatia({ parte, todo }: { parte: number; todo: number }) {
  if (todo <= 0) return <span className="cad-nada">sem cliente na carteira</span>;
  const pct = Math.round((parte / todo) * 100);
  return (
    <>
      <div className="cad-mono">
        <span className={pct < 40 ? 'cad-atencao' : undefined}>{pct}%</span>
      </div>
      <span className="cad-barra-trilho" aria-hidden="true">
        <span className="cad-barra-mini" style={{ width: `${pct}%` }} />
      </span>
      <div className="cad-sub">
        {nº(parte)} de {nº(todo)}
      </div>
    </>
  );
}
