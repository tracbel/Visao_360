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
 */

import { CalendarCheck, RefreshCw, UserRound, Users, UsersRound, UserX } from 'lucide-react';
import { useMemo, useState } from 'react';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { GraficoBarrasEmpilhadas } from '../componentes/GraficoBarrasEmpilhadas';
import { GraficoBarrasHorizontais } from '../componentes/GraficoBarrasHorizontais';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { MolduraDeGrafico } from '../componentes/MolduraDeGrafico';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { ENTRAM_NO_RANKING } from '../dados/api/consolidado';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { ErroDaApi } from '../dados/api/http';
import { obterMetaDaFilial } from '../dados/api/metas';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import {
  obterFunilPorEstagio,
  obterPainelDoCen,
  obterResumoDeCobertura,
  obterVendasPerdidas,
} from '../dados/api/relacionamento';
import type { CoberturaPorClasse } from '../tipos/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import { formatarData } from './cadastro/formato';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';

const nº = (v: number) => v.toLocaleString('pt-BR');

/**
 * As métricas que o ranking sabe desenhar. Todas saem do MESMO agregado que a
 * tabela abaixo usa, e por isso o gráfico e a tabela nunca divergem.
 */
const METRICAS = [
  { id: 'clientes', rotulo: 'Vínculos na carteira', unidade: 'vínculos' },
  { id: 'em30', rotulo: 'Contatados em 30 dias', unidade: 'vínculos' },
  { id: 'cobertura30', rotulo: 'Cobertura em 30 dias (%)', unidade: '%' },
  { id: 'nunca', rotulo: 'Nunca contatados', unidade: 'vínculos' },
] as const;

type MetricaId = (typeof METRICAS)[number]['id'];

/** Quantas barras cabem no ranking sem virar uma parede de nomes. */
const BARRAS = 10;

/**
 * As quatro faixas de tempo sem contato, nas mesmas cores da Cobertura de
 * Carteira e do donut da Visão 360 — a mesma faixa tem a mesma cor em toda a
 * aplicação. Os acumulados da API são cumulativos (`em90` inclui `em30`), e
 * subtrair aqui é o que torna as fatias exclusivas.
 */
const FAIXAS_DE_CONTATO: { nome: string; cor: string; medir: (c: LinhaDeCen) => number }[] = [
  { nome: 'Em dia (até 30 dias)', cor: '#367C2B', medir: (c) => c.em30 },
  { nome: 'Aviso (31 a 90 dias)', cor: '#C1660A', medir: (c) => c.em90 - c.em30 },
  { nome: 'Atraso (mais de 90 dias)', cor: '#DC2626', medir: (c) => c.clientes - c.em90 - c.nunca },
  { nome: 'Nunca contatado', cor: '#9CA3AF', medir: (c) => c.nunca },
];

/**
 * Os quatro estados de um vínculo contra a cadência declarada.
 *
 * O cinza do "sem cadência declarada" é de propósito o único sem temperatura: não é bom nem
 * ruim, é a ausência de prazo contra o que medir. Empurrá-lo para verde ou vermelho inventaria
 * uma meta que ninguém definiu.
 */
const FAIXAS_DE_COBERTURA: {
  nome: string;
  cor: string;
  medir: (c: CoberturaPorClasse) => number;
}[] = [
  { nome: 'Coberto', cor: '#367C2B', medir: (c) => c.cobertos },
  { nome: 'Fora da cadencia', cor: '#DC2626', medir: (c) => c.foraDaCadencia },
  { nome: 'Nunca contatado', cor: '#9CA3AF', medir: (c) => c.nuncaContatados },
  { nome: 'Sem cadencia declarada', cor: '#D4D8D5', medir: (c) => c.semCadenciaDeclarada },
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

/** Quantas pessoas entram no gráfico antes de a barra virar um traço. */
const CENS_NO_GRAFICO = 12;

/**
 * O nome como cabe no eixo: primeiro e último.
 *
 * "LUCAS HENRIQUE SUMIDA BELINI" ocupava metade da largura do gráfico e
 * empurrava as barras para fora. O nome inteiro continua na tabela.
 */
function primeiroENome(nome: string): string {
  const partes = nome.trim().split(/\s+/);
  if (partes.length <= 2) return nome;
  return `${partes[0]} ${partes[partes.length - 1]}`;
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

export function PerformanceCen() {
  const { contexto } = useContextoDeAcesso();
  const [ordem, setOrdem] = useState<Ordem>('cobertura30');
  const [descendente, setDescendente] = useState(false);
  const [metricaDoRanking, setMetricaDoRanking] = useState<MetricaId>('clientes');

  /** O CEN escolhido no seletor. Nulo e o consolidado de todos. */
  const [cenEscolhido, setCenEscolhido] = useState<string | null>(null);

  const painel = useRecurso(
    (sinal) => obterPainelDoCen(contexto, cenEscolhido, sinal),
    [contexto.empresa, contexto.usuario, cenEscolhido],
  );

  const resumo = useRecurso(
    (sinal) => obterResumoDeCobertura(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  /** O funil do CEN escolhido, pelo FLUXO — a leitura da Performance (27/09/2026). */
  const funil = useRecurso(
    (sinal) => obterFunilPorEstagio(contexto, { base: 'etapa', responsavel: cenEscolhido ?? undefined }, sinal),
    [contexto.empresa, contexto.usuario, cenEscolhido],
  );
  const motivoDoFunil =
    funil.dados?.metricasSemDado.find((m) => m.metrica === 'funil' || m.metrica === 'funilNoPeriodo')?.motivo ??
    'O funil ainda não respondeu.';

  /** As perdas do CEN no período: os processos perdidos no funil e os formulários de venda perdida (só a principal). */
  const perdas = useRecurso(
    (sinal) => obterVendasPerdidas(contexto, sinal, { responsavel: cenEscolhido ?? undefined }),
    [contexto.empresa, contexto.usuario, cenEscolhido],
  );

  const carteiras = useMemo(() => resumo.dados?.itens ?? [], [resumo.dados]);

  /** A meta de venda × o realizado da filial do cabeçalho, por consultor (#138). */
  const meta = useRecurso(
    (sinal) => obterMetaDaFilial(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  /** O que o bloco da meta diz fechado: o total da filial com o período, ou por que não há número. */
  const resumoDaMeta = !meta.dados
    ? 'a cota da API Gestão de Negócios contra as vendas do ART, em máquinas'
    : meta.dados.origem === null
      ? 'o cadastro de metas da API Gestão de Negócios ainda não foi lido'
      : `${meta.dados.totais.realizadoMaquinas.toLocaleString('pt-BR')} de ` +
        `${meta.dados.totais.metaMaquinas.toLocaleString('pt-BR')} máquinas · ${meta.dados.periodo.texto} · ` +
        `${meta.dados.porConsultor.length} ${meta.dados.porConsultor.length === 1 ? 'consultor' : 'consultores'}`;

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
  const carteirasDePessoa = useMemo(
    () =>
      carteiras.filter(
        (c) => ENTRAM_NO_RANKING.has(c.naturezaDoResponsavel) && c.naturezaDaCarteira === 'Comercial',
      ),
    [carteiras],
  );

  /** Quantas carteiras ficaram de fora do ranking, para a tela poder dizer. */
  const carteirasForaDoRanking = carteiras.length - carteirasDePessoa.length;

  const cens = useMemo(() => {
    const mapa = new Map<string, LinhaDeCen>();
    for (const c of carteirasDePessoa) {
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
  }, [carteirasDePessoa]);

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

  const totais = useMemo(
    () => ({
      cens: cens.length,
      clientes: cens.reduce((s, c) => s + c.clientes, 0),
      em30: cens.reduce((s, c) => s + c.em30, 0),
      nunca: cens.reduce((s, c) => s + c.nunca, 0),
    }),
    [cens],
  );

  /** As maiores carteiras primeiro: são elas que movem o consolidado. */
  const censNoGrafico = useMemo(
    () => [...cens].sort((a, b) => b.clientes - a.clientes).slice(0, CENS_NO_GRAFICO),
    [cens],
  );

  /** As barras do ranking, na métrica escolhida. Tudo do mesmo agregado. */
  const barrasDoRanking = useMemo(() => {
    const medir = (c: LinhaDeCen) => {
      switch (metricaDoRanking) {
        case 'clientes':
          return c.clientes;
        case 'em30':
          return c.em30;
        case 'cobertura30':
          return c.clientes > 0 ? Math.round((c.em30 / c.clientes) * 100) : 0;
        case 'nunca':
          return c.nunca;
      }
    };
    const unidade = METRICAS.find((m) => m.id === metricaDoRanking)?.unidade ?? '';

    return [...cens]
      .map((c) => ({ cen: c, v: medir(c) }))
      .sort((x, y) => y.v - x.v)
      .slice(0, BARRAS)
      .map(({ cen, v }, i) => ({
        // O MESMO RECORTE DO GRÁFICO AO LADO. Cortar no caractere 21 produzia
        // "VANESSA KELLY B ALVES…" e "HAMILTON DE SOUZA LOP…" — reticências
        // onde caberia o sobrenome. Primeiro e último nome identificam a
        // pessoa e cabem; o nome inteiro continua no balão.
        rotulo: primeiroENome(cen.responsavelNome),
        valor: v,
        // A COR DIZ O SENTIDO, E NÃO O MÉRITO: em "nunca contatados", barra
        // grande é problema, então ela é vermelha. Nas demais, maior é melhor.
        cor: metricaDoRanking === 'nunca' ? '#DC2626' : i === 0 ? '#367C2B' : '#6FBF5E',
        tooltipLinhas: [
          cen.responsavelNome,
          v.toLocaleString('pt-BR') + ' ' + unidade,
          cen.carteiras +
            (cen.carteiras === 1 ? ' carteira' : ' carteiras') +
            ' \u00b7 ' +
            cen.clientes.toLocaleString('pt-BR') +
            ' vínculos',
        ],
      }));
  }, [cens, metricaDoRanking]);

  function trocarOrdem(campo: Ordem) {
    if (ordem === campo) setDescendente((d) => !d);
    else {
      setOrdem(campo);
      setDescendente(false);
    }
  }

  // SEM CARTEIRA AO ALCANCE, O NÚMERO É O TRAÇO COM O MOTIVO; LENDO, O CARTÃO PULSA E NÃO AFIRMA MOTIVO NENHUM.
  const semCarteira = resumo.carregando ? undefined : 'Sem carteira ao alcance deste contexto.';
  const nomeDoCen = painel.dados?.painel.responsavelNome ?? 'Todos os responsáveis';

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Performance de CEN</h1>
          <p className="page-subtitle">
            <strong>Quem está cobrindo a carteira e quem não está</strong> — e a meta de venda de cada consultor contra as
            máquinas que ele vendeu.
          </p>
        </div>
        <p className="dash-atualizado">
          {resumo.procedencia ? <DadosAtualizadosEm procedencia={resumo.procedencia} /> : 'Lendo a performance…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              resumo.recarregar();
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

      {/* A BARRA DOS INDICADORES. O CEN escolhido vale para o painel do CEN e para o funil e as perdas dele; os números do
          topo, o gráfico e o ranking são sempre de todos os responsáveis da filial. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="cen">
            <span className="dash-filtro-icone" aria-hidden="true">
              <UserRound size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                CEN
                <InfoTooltip
                  texto="O responsável escolhido vale para o painel do CEN — a cobertura por classe — e para o funil e as perdas dele. Os números do topo, a divisão da carteira e o ranking são sempre de todos os responsáveis da filial."
                  rotulo="O que o CEN escolhido muda"
                />
              </span>
              <select value={cenEscolhido ?? ''} onChange={(e) => setCenEscolhido(e.target.value === '' ? null : e.target.value)}>
                <option value="">Todos os responsáveis</option>
                {(painel.dados?.responsaveis ?? []).map((r) => (
                  <option key={r.chave} value={r.chave}>
                    {r.nome}
                    {r.natureza === 'Departamento' ? ' (área)' : ''} · {r.carteiras} carteira
                    {r.carteiras === 1 ? '' : 's'}
                  </option>
                ))}
              </select>
            </span>
          </label>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={resumo.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="CENs com carteira"
          icone={UsersRound}
          tom="neutro"
          valor={resumo.dados ? nº(totais.cens) : null}
          carregando={resumo.carregando}
          unidade="responsáveis"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? 'distintos nas carteiras desta filial' : null}
          sobre="Responsáveis distintos nas carteiras comerciais desta filial. Pessoa e área entram; contas do sistema, do fornecedor e de teste não."
        />
        <CartaoDeDecisao
          rotulo="Vínculos atendidos"
          icone={Users}
          tom="mercado"
          valor={resumo.dados ? nº(totais.clientes) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? 'cliente × carteira' : null}
          sobre="Vínculos cliente × carteira; metade dos clientes está em duas ou mais carteiras — por isso é contagem de vínculos, e não de clientes."
        />
        <CartaoDeDecisao
          rotulo="Cobertura em 30 dias"
          icone={CalendarCheck}
          tom="demanda"
          valor={resumo.dados && totais.clientes > 0 ? `${Math.round((totais.em30 / totais.clientes) * 100)}%` : null}
          carregando={resumo.carregando}
          motivoSemDado={resumo.carregando ? undefined : resumo.dados ? 'Sem vínculo na carteira: não há de quanto tirar o percentual.' : semCarteira}
          variacao={resumo.dados && totais.clientes > 0 ? `${nº(totais.em30)} de ${nº(totais.clientes)} vínculos` : null}
          sobre="Vínculos com contato nos últimos 30 dias (regra da BI de carteiras), sobre o total."
        />
        <CartaoDeDecisao
          rotulo="Nunca contatados"
          icone={UserX}
          tom="oportunidade"
          valor={resumo.dados ? nº(totais.nunca) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? 'sem contato no histórico' : null}
          sobre="Sem contato no histórico do Vórtice, pela regra da BI de carteiras."
        />
      </div>

      <MetricasSemDado metricas={resumo.dados?.metricasSemDado} />

      {/* ------------------------------------------------------------------ */}
      {/* O PAINEL DE UM CEN — o filtro que a gerência pediu                  */}
      {/*                                                                     */}
      {/* A pergunta desta seção é a da reunião de resultado: deste CEN,       */}
      {/* quantos clientes A estão cobertos, quantos venceram o prazo e        */}
      {/* quantos ele nunca procurou.                                          */}
      {/*                                                                     */}
      {/* "Coberto" NÃO É UM NÚMERO REDONDO ESCOLHIDO POR ALGUÉM: é a cadência */}
      {/* que o negócio declarou para aquela classe naquela linha de negócio — */}
      {/* 180 dias em Venda de Máquinas, 360 em Peças e AMS, 120 em            */}
      {/* Prospecção. Onde a linha não declara cadência, o vínculo sai numa    */}
      {/* quarta faixa em vez de ser empurrado para um dos dois lados.         */}
      {/* ------------------------------------------------------------------ */}
      <section className="dash-secao" data-bloco="secao-cen">
        <TituloDaSecao
          titulo="O painel do CEN"
          subtitulo={
            painel.dados
              ? `${nomeDoCen} · ${painel.dados.painel.carteiras} carteira(s) · ${painel.dados.painel.clientes.toLocaleString('pt-BR')} vínculos`
              : 'O responsável escolhido na barra, ou todos.'
          }
          metodologia="Coberto é o vínculo com contato dentro da cadência que o negócio declarou para a classe naquela linha de negócio — 180 dias em Venda de Máquinas, 360 em Peças e AMS, 120 em Prospecção. Onde a linha não declara cadência, o vínculo sai numa quarta faixa. O funil é o FLUXO: as etapas que o CEN fez andar no período, pelo responsável do processo no Vórtice."
        />

        <div className="dash-empilhados">
          <PainelDoMomento
            titulo="Cobertura por classe do cliente"
            data-bloco="cobertura-por-classe"
            subtitulo="Classe apurada da curva ABC do faturamento, contra a cadência declarada."
            dica="Três estados, e não dois: quem o CEN conhece e deixou vencer não é o mesmo problema que quem ele nunca procurou. O cinza-claro é a linha sem cadência declarada — não é bom nem ruim."
          >
            {painel.carregando && <BlocoCarregando oQue="o painel do CEN" />}
            {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}

            <MetricasSemDado metricas={painel.dados?.metricasSemDado} />

            {painel.dados && painel.dados.painel.porClasse.length > 0 && (
              <>
                <MolduraDeGrafico altura={Math.max(170, painel.dados.painel.porClasse.length * 34 + 46)}>
                  {(l, a) => (
                    <GraficoBarrasEmpilhadas
                      itens={painel.dados!.painel.porClasse.map((c) => `Classe ${c.classe}${c.diasDeCadencia ? ` · ${c.diasDeCadencia}d` : ''}`)}
                      faixas={FAIXAS_DE_COBERTURA.map((f) => ({
                        nome: f.nome,
                        cor: f.cor,
                        valores: painel.dados!.painel.porClasse.map(f.medir),
                      }))}
                      largura={l}
                      altura={a}
                    />
                  )}
                </MolduraDeGrafico>

                <div className="cad-legenda-faixas">
                  {FAIXAS_DE_COBERTURA.map((f) => (
                    <span key={f.nome}>
                      <i style={{ background: f.cor }} />
                      {f.nome}{' '}
                      <strong>
                        {painel.dados!.painel.porClasse.reduce((t, c) => t + f.medir(c), 0).toLocaleString('pt-BR')}
                      </strong>
                    </span>
                  ))}
                </div>

                <div className="mom-tabela-rolagem">
                  <table className="mom-tabela">
                    <caption className="cad-so-leitor">Cobertura por classe do cliente</caption>
                    <thead>
                      <tr>
                        <th scope="col">Classe</th>
                        <th scope="col" className="mom-num">Cadência</th>
                        <th scope="col" className="mom-num">Vínculos</th>
                        <th scope="col" className="mom-num">Cobertos</th>
                        <th scope="col" className="mom-num">Fora da cadência</th>
                        <th scope="col" className="mom-num">Nunca contatados</th>
                      </tr>
                    </thead>
                    <tbody>
                      {painel.dados.painel.porClasse.map((c) => (
                        <tr key={c.classe}>
                          <th scope="row">{c.classe}</th>
                          <td className="mom-num">
                            {/* Cadência nula com vínculo medido quer dizer que a classe aparece em
                                linhas de negócio com prazos diferentes — 180 em Máquinas, 360 em
                                Peças. Escrever um dos dois seria afirmar um prazo que não vale para
                                todo mundo da linha. */}
                            {c.diasDeCadencia ? (
                              `${c.diasDeCadencia} dias`
                            ) : c.semCadenciaDeclarada === c.clientes ? (
                              <span className="cad-nada">não declarada</span>
                            ) : (
                              <span className="cad-nada">varia por linha</span>
                            )}
                          </td>
                          <td className="mom-num">{c.clientes.toLocaleString('pt-BR')}</td>
                          <td className="mom-num">
                            <Fatia parte={c.cobertos} todo={c.clientes} />
                          </td>
                          <td className="mom-num">
                            <span className={c.foraDaCadencia > 0 ? 'cad-alerta' : undefined}>
                              {c.foraDaCadencia.toLocaleString('pt-BR')}
                            </span>
                          </td>
                          <td className="mom-num">
                            <span className={c.nuncaContatados > 0 ? 'cad-atencao' : undefined}>
                              {c.nuncaContatados.toLocaleString('pt-BR')}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                {/* OS PROCESSOS SAÍRAM DAQUI (27/09/2026): "0 ganhos · 0 perdidos · 0 abertos" contava processo.Processo, que
                    só a onda 2 carrega. O funil do CEN, pelo responsável do processo no Vórtice, está no painel ao lado. */}
                <p className="v360-nota">
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

          {/* ------------------------------------------------------------------ */}
          {/* O FUNIL DO CEN — o fluxo por estágio e as perdas (27/09/2026)        */}
          {/*                                                                     */}
          {/* A Performance usa o FLUXO (decisão do Ricardo): as etapas que o CEN  */}
          {/* fez andar no período, qualquer que seja a abertura do processo. O    */}
          {/* dono é o responsável do processo no Vórtice (UsuResponsavel), e o    */}
          {/* seletor é o mesmo da barra.                                          */}
          {/* ------------------------------------------------------------------ */}
          <PainelDoMomento
            titulo="Funil do CEN — fluxo por estágio"
            data-bloco="funil-do-cen"
            subtitulo={
              funil.dados
                ? `${nomeDoCen} · etapas alcançadas em ${funil.dados.periodo.texto}`
                : 'as etapas alcançadas no período, pelo responsável do processo no Vórtice'
            }
            dica="O fluxo conta as etapas que o CEN fez andar no período, qualquer que seja a abertura do processo. As perdas são os processos perdidos no funil e os formulários de venda perdida — só a principal."
          >
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
                            <td className="mom-num">{e ? e.processos.toLocaleString('pt-BR') : ausente}</td>
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
                    `${perdas.dados.processosPerdidos.toLocaleString('pt-BR')} processos perdidos · ` +
                    `${perdas.dados.registradas.toLocaleString('pt-BR')} com o formulário de venda perdida` +
                    (perdas.dados.porMotivo[0] ? ` · mais comum: ${perdas.dados.porMotivo[0].nome}` : '')
                  ) : (
                    <ValorAusente oQue="as perdas" motivo={perdas.erro?.message ?? 'Lendo as vendas perdidas.'} />
                  )}
                </p>
              </>
            )}
          </PainelDoMomento>
        </div>
      </section>

      {/* O QUE FICOU DE FORA, DITO NA TELA. Excluir sem avisar é a mesma família de defeito que
          mostrar número sem dizer de onde ele vem — quem conhece a base vai procurar o
          INT.MERCADO no ranking e precisa saber por que ele não está lá. */}
      {carteirasForaDoRanking > 0 && (
        <MetricasSemDado
          titulo="O que este ranking deixa de fora"
          metricas={[
            {
              metrica: 'carteirasForaDoRanking',
              motivo:
                `${carteirasForaDoRanking} de ${carteiras.length} carteiras não entram neste ` +
                'ranking: ou a carteira é depósito de cadastro e não carteira de venda, ou o ' +
                'responsável não é operação — conta do próprio sistema, do fornecedor ou de ' +
                'teste. Área entra: a Inteligência de Mercado aparece identificada como tal. ' +
                'Nada some — tudo continua inteiro na Cobertura de Carteira.',
            },
          ]}
        />
      )}

      {/* ------------------------------------------------------------------ */}
      {/* Os dois gráficos, lado a lado, no lugar dos dois do protótipo        */}
      {/* ------------------------------------------------------------------ */}
      <section className="dash-secao" data-bloco="secao-carteiras">
        <TituloDaSecao
          titulo="A carteira de cada CEN"
          subtitulo="Quem está cobrindo e quem não está, responsável a responsável."
          metodologia="Somar carteira por responsável é exato: cada carteira tem um responsável só, e nenhuma linha é contada duas vezes. O gráfico e o ranking saem do mesmo agregado da tabela em número, no fim da tela."
        />

        <div className="dash-duas-colunas dash-analitico">
          {/* A LINHA VAZIA DE DOZE MESES SAIU, E O LUGAR DELA TEM DADO.

              Ficava aqui um gráfico de linha desenhado com doze zeros e uma tampa
              por cima explicando que não há série mensal POR CEN — o motivo
              continua escrito, no rodapé: a carga do faturamento não lê o vendedor
              da nota (27/09/2026; a frase antiga dizia que o faturamento tinha
              parado, e era a cópia do Vórtice). O que mudou é que a moldura
              deixou de gastar meia tela para não dizer nada.

              No lugar entra a única leitura por pessoa que tem lastro: como a
              carteira de cada CEN se divide entre as faixas de tempo sem contato.
              Mesmo agregado da tabela — gráfico e número nunca divergem. */}
          <PainelDoMomento
            titulo="Como a carteira de cada CEN está dividida"
            data-bloco="carteira-dividida"
            subtitulo={
              censNoGrafico.length > 0
                ? `${censNoGrafico.length} maiores de ${cens.length} responsáveis · fatia de cada faixa de tempo sem contato`
                : 'fatia de cada faixa de tempo sem contato'
            }
            dica="As mesmas faixas e cores da Cobertura de Carteira: em dia (até 30 dias), aviso (31 a 90), atraso (mais de 90) e nunca contatado."
          >
            {resumo.carregando && <BlocoCarregando oQue="a divisão da carteira" />}

            {!resumo.carregando && censNoGrafico.length > 0 && (
              <>
                <MolduraDeGrafico altura={Math.max(200, censNoGrafico.length * 24 + 40)}>
                  {(l, a) => (
                    <GraficoBarrasEmpilhadas
                      itens={censNoGrafico.map((c) => primeiroENome(c.responsavelNome))}
                      faixas={FAIXAS_DE_CONTATO.map((f) => ({
                        nome: f.nome,
                        cor: f.cor,
                        valores: censNoGrafico.map(f.medir),
                      }))}
                      largura={l}
                      altura={a}
                      proporcional
                    />
                  )}
                </MolduraDeGrafico>
                <div className="cad-legenda-faixas">
                  {FAIXAS_DE_CONTATO.map((f) => (
                    <span key={f.nome}>
                      <i style={{ background: f.cor }} />
                      {f.nome} <strong>{cens.reduce((t, c) => t + f.medir(c), 0).toLocaleString('pt-BR')}</strong>
                    </span>
                  ))}
                </div>
              </>
            )}
          </PainelDoMomento>

          <PainelDoMomento
            titulo="Ranking por métrica"
            data-bloco="ranking"
            subtitulo={`as ${Math.min(BARRAS, cens.length)} maiores · o mesmo agregado do gráfico ao lado`}
            dica="A cor diz o sentido, e não o mérito: em nunca contatados, barra grande é problema, e ela é vermelha. Nas demais métricas, maior é melhor."
            direita={
              <label className="dash-seletor-do-painel">
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
            {resumo.carregando && <BlocoCarregando oQue="o ranking" />}

            {resumo.dados && barrasDoRanking.length === 0 && !resumo.erro && (
              <BlocoVazio
                titulo="Nenhum CEN com carteira nesta filial"
                texto="O ranking sai da cobertura por carteira; sem carteira com responsavel nao ha barra."
              />
            )}

            {barrasDoRanking.length > 0 && (
              <MolduraDeGrafico altura={260}>
                {(l, a) => <GraficoBarrasHorizontais itens={barrasDoRanking} largura={l} altura={a} />}
              </MolduraDeGrafico>
            )}
          </PainelDoMomento>
        </div>
      </section>

      {/* A TABELA INTEIRA, FECHADA POR PADRÃO.

          Os dois gráficos acima respondem quem está cobrindo e quem não está.
          Quem precisa do número exato de cada um dos responsáveis abre aqui —
          aberta, esta tabela sozinha respondia por cerca de 4.000px da altura
          da tela, e era o motivo de a página ter 5.075px. */}
      <BlocoRecolhivel
        titulo="Cobertura por CEN, em número"
        resumo={`os ${totais.cens} responsáveis, somados das ${carteiras.length} carteiras contadas no banco`}
      >
        {resumo.carregando && <BlocoCarregando oQue="a cobertura por CEN" />}
        {resumo.erro && <BlocoErro erro={resumo.erro} aoTentarDeNovo={resumo.recarregar} />}

        {resumo.dados && cens.length === 0 && !resumo.erro && (
          <BlocoVazio
            titulo="Nenhuma carteira com responsável nesta filial"
            texto="A carteira carrega o CEN responsável desde a carga de 2026. Se não aparece ninguém, confira a filial escolhida no cabeçalho."
          />
        )}

        {cens.length > 0 && (
          <>
            <div className="mom-tabela-rolagem cad-so-largo">
              <table className="mom-tabela">
                <caption className="cad-so-leitor">Cobertura de carteira por CEN da filial {contexto.empresa}</caption>
                <thead>
                  <tr>
                    {COLUNAS.map((coluna) => (
                      <th
                        key={coluna.rotulo}
                        scope="col"
                        className={coluna.ordem === 'nome' ? undefined : 'mom-num'}
                        aria-sort={ordem === coluna.ordem ? (descendente ? 'descending' : 'ascending') : undefined}
                      >
                        <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem)}>
                          {coluna.rotulo}
                          <span aria-hidden="true">{ordem === coluna.ordem ? (descendente ? '▾' : '▴') : ''}</span>
                        </button>
                      </th>
                    ))}
                    <th scope="col" className="mom-num">Último contato na carteira</th>
                  </tr>
                </thead>
                <tbody>
                  {ordenados.map((c) => (
                    <tr key={c.responsavelNome}>
                      <th scope="row">
                        <div className="cad-link-forte">{c.responsavelNome}</div>
                        <div className="cad-sub">
                          {c.carteiras} {c.carteiras === 1 ? 'carteira' : 'carteiras'} · {c.linhas.join(', ')}
                        </div>
                      </th>
                      <td className="mom-num">{c.clientes.toLocaleString('pt-BR')}</td>
                      <td className="mom-num">
                        <Fatia parte={c.em30} todo={c.clientes} />
                      </td>
                      <td className="mom-num">
                        <Fatia parte={c.em90} todo={c.clientes} />
                      </td>
                      <td className="mom-num">
                        <span className={c.nunca > 0 ? 'cad-atencao' : undefined}>{c.nunca.toLocaleString('pt-BR')}</span>
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
              {ordenados.map((c) => (
                <div className="cad-ficha" key={c.responsavelNome}>
                  <div className="cad-ficha-titulo">{c.responsavelNome}</div>
                  <div className="cad-sub">
                    {c.carteiras} {c.carteiras === 1 ? 'carteira' : 'carteiras'} · {c.linhas.join(', ')}
                  </div>
                  <div className="cad-ficha-linha">
                    <span className="cad-ficha-rotulo">Vínculos</span>
                    <span className="cad-ficha-valor">{c.clientes.toLocaleString('pt-BR')}</span>
                  </div>
                  <div className="cad-ficha-linha">
                    <span className="cad-ficha-rotulo">Contato em 30 dias</span>
                    <span className="cad-ficha-valor">
                      <Fatia parte={c.em30} todo={c.clientes} />
                    </span>
                  </div>
                  <div className="cad-ficha-linha">
                    <span className="cad-ficha-rotulo">Nunca contatados</span>
                    <span className="cad-ficha-valor">{c.nunca.toLocaleString('pt-BR')}</span>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </BlocoRecolhivel>

      {/* A META DE VENDA × O REALIZADO, POR CONSULTOR (#138, 27/09/2026) — fechado, como a tabela acima.

          Sem a rotina das metas ter rodado, o bloco diz que o cadastro não foi lido — e não "meta zero".
          O 403 é falta de Meta.Ler, e tentar de novo não muda nada: o bloco não oferece o botão. */}
      <BlocoRecolhivel
        titulo={meta.dados?.alcance === 'Proprios' ? 'Sua meta de venda × o realizado' : 'Meta de venda × realizado, por consultor'}
        resumo={resumoDaMeta}
      >
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
                Meta de venda × realizado por consultor da filial {contexto.empresa}, {meta.dados.periodo.texto}
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
                    <td className="mom-num">{c.meta.toLocaleString('pt-BR')}</td>
                    <td className="mom-num">{c.realizado.toLocaleString('pt-BR')}</td>
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
      </BlocoRecolhivel>

      {/* O AVISO DE QUE ESTA TELA ENCOLHEU MORAVA NO TOPO, EM LARANJA.

          Era a primeira coisa que a diretoria lia ao abrir a Performance, e o
          que ela dizia era uma justificativa. O motivo continua inteiro — cada
          métrica que saiu, com a data e o número que a derrubaram —, só que
          depois do que a tela de fato tem para mostrar. */}
      <BlocoRecolhivel
        titulo="O que esta tela mostrava e não tem como sustentar"
        resumo="faturamento por CEN, tarefas por pessoa e tempo de atendimento"
      >
        <div className="cad-fichas">
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
          {/* O "ATINGIMENTO DE META" SAIU DESTA LISTA EM 27/09/2026: a meta de venda tem fonte — a API
              Gestão de Negócios — e está no bloco "Meta de venda × realizado", acima (#138). */}
          {/* "PROCESSOS E TAREFAS POR CEN" DIZIA QUE FALTAVA ROTA (27/09/2026): os processos por CEN estão no funil do
              CEN, acima, pelo responsável do processo no Vórtice. O que continua faltando é a agenda. */}
          <LacunaConhecida
            metrica="Tarefas por CEN"
            motivo={
              'Os processos de cada CEN estão no bloco "Funil do CEN", pelo responsável do processo no Vórtice. As ' +
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
          {/* A "TAXA DE CONVERSÃO POR CEN" SAIU DESTA LISTA EM 27/09/2026: a conversão de estágio a estágio de cada CEN
              está no funil do CEN, acima. */}
        </div>
      </BlocoRecolhivel>
    </PaginaDoPainel>
  );
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
        {parte.toLocaleString('pt-BR')} de {todo.toLocaleString('pt-BR')}
      </div>
    </>
  );
}
