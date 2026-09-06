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
 */

import { useMemo, useState } from 'react';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { GraficoBarrasEmpilhadas } from '../componentes/GraficoBarrasEmpilhadas';
import { GraficoBarrasHorizontais } from '../componentes/GraficoBarrasHorizontais';
import { MolduraDeGrafico } from '../componentes/MolduraDeGrafico';
import { ENTRAM_NO_RANKING } from '../dados/api/consolidado';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterPainelDoCen, obterResumoDeCobertura } from '../dados/api/relacionamento';
import type { CoberturaPorClasse } from '../tipos/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import { formatarData } from './cadastro/formato';

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

  const carteiras = useMemo(() => resumo.dados?.itens ?? [], [resumo.dados]);

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

  const indicadores: Indicador[] = [
    {
      rotulo: 'CENs com carteira',
      valor: resumo.dados ? totais.cens : null,
      deOnde: 'responsáveis distintos nas carteiras desta filial',
      semDado: 'sem carteira ao alcance deste contexto',
    },
    {
      rotulo: 'Vínculos atendidos',
      valor: resumo.dados ? totais.clientes : null,
      deOnde: 'vínculos cliente × carteira; metade dos clientes está em duas ou mais',
      semDado: '—',
    },
    {
      rotulo: 'Cobertura em 30 dias',
      valor: resumo.dados && totais.clientes > 0 ? `${Math.round((totais.em30 / totais.clientes) * 100)}%` : null,
      tom: 'bom',
      deOnde: 'vínculos com interação nos últimos 30 dias, sobre o total',
      semDado: '—',
    },
    {
      rotulo: 'Nunca contatados',
      valor: resumo.dados ? totais.nunca : null,
      tom: 'atencao',
      deOnde: 'sem nenhuma interação no recorte carregado',
      semDado: '—',
    },
  ];

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Performance de CEN</h1>
          <p className="page-subtitle">
            <code>comercial.ClienteCarteira</code> — o único agregado por pessoa que tem dado real
            hoje: <strong>quem está cobrindo a carteira e quem não está</strong>.
          </p>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={resumo.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={resumo.carregando} />

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
      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Cobertura por classe do cliente</div>
            <div className="card-subtitle">
              {painel.dados
                ? `${painel.dados.painel.responsavelNome} · ${painel.dados.painel.carteiras} carteira(s) · ${painel.dados.painel.clientes.toLocaleString('pt-BR')} vínculos`
                : 'classe apurada da curva ABC do faturamento, contra a cadência declarada'}
            </div>
          </div>
          <SeloProcedencia procedencia={painel.procedencia} />
        </div>

        <div className="cad-barra">
          <label className="cad-filtro">
            CEN
            <select
              value={cenEscolhido ?? ''}
              onChange={(e) => setCenEscolhido(e.target.value === '' ? null : e.target.value)}
            >
              <option value="">Todos os responsáveis</option>
              {(painel.dados?.responsaveis ?? []).map((r) => (
                <option key={r.chave} value={r.chave}>
                  {r.nome}
                  {r.natureza === 'Departamento' ? ' (área)' : ''} · {r.carteiras} carteira
                  {r.carteiras === 1 ? '' : 's'}
                </option>
              ))}
            </select>
          </label>
        </div>

        {painel.carregando && <BlocoCarregando oQue="o painel do CEN" />}
        {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}

        <MetricasSemDado metricas={painel.dados?.metricasSemDado} />

        {painel.dados && painel.dados.painel.porClasse.length > 0 && (
          <>
            <MolduraDeGrafico altura={Math.max(170, painel.dados.painel.porClasse.length * 34 + 46)}>
              {(l, a) => (
                <GraficoBarrasEmpilhadas
                  itens={painel.dados!.painel.porClasse.map(
                      (c) => `Classe ${c.classe}${c.diasDeCadencia ? ` · ${c.diasDeCadencia}d` : ''}`,
                  )}
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
                    {painel
                      .dados!.painel.porClasse.reduce((t, c) => t + f.medir(c), 0)
                      .toLocaleString('pt-BR')}
                  </strong>
                </span>
              ))}
            </div>

            <div className="cad-tabela-wrap">
              <table className="cad-tabela">
                <caption className="cad-so-leitor">Cobertura por classe do cliente</caption>
                <thead>
                  <tr>
                    <th scope="col">Classe</th>
                    <th scope="col">Cadência</th>
                    <th scope="col">Vínculos</th>
                    <th scope="col">Cobertos</th>
                    <th scope="col">Fora da cadência</th>
                    <th scope="col">Nunca contatados</th>
                  </tr>
                </thead>
                <tbody>
                  {painel.dados.painel.porClasse.map((c) => (
                    <tr key={c.classe}>
                      <td className="cad-link-forte">{c.classe}</td>
                      <td className="cad-mono">
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
                      <td className="cad-mono">{c.clientes.toLocaleString('pt-BR')}</td>
                      <td className="cad-mono">
                        <Fatia parte={c.cobertos} todo={c.clientes} />
                      </td>
                      <td className="cad-mono">
                        <span className={c.foraDaCadencia > 0 ? 'cad-alerta' : undefined}>
                          {c.foraDaCadencia.toLocaleString('pt-BR')}
                        </span>
                      </td>
                      <td className="cad-mono">
                        <span className={c.nuncaContatados > 0 ? 'cad-atencao' : undefined}>
                          {c.nuncaContatados.toLocaleString('pt-BR')}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="cad-fichas">
              <div className="cad-ficha-linha">
                <span className="cad-ficha-rotulo">Processos dos clientes desta carteira</span>
                <span className="cad-ficha-valor">
                  {painel.dados.painel.processosGanhos.toLocaleString('pt-BR')} ganhos ·{' '}
                  {painel.dados.painel.processosPerdidos.toLocaleString('pt-BR')} perdidos ·{' '}
                  {painel.dados.painel.processosAbertos.toLocaleString('pt-BR')} abertos
                </span>
              </div>
              <div className="cad-ficha-linha">
                <span className="cad-ficha-rotulo">Derrotas com formulário preenchido</span>
                <span className="cad-ficha-valor">
                  {painel.dados.painel.vendasPerdidasRegistradas.toLocaleString('pt-BR')} de{' '}
                  {painel.dados.painel.processosPerdidos.toLocaleString('pt-BR')}
                </span>
              </div>
              <div className="cad-ficha-linha">
                <span className="cad-ficha-rotulo">Faturamento dos clientes da carteira</span>
                <span className="cad-ficha-valor">
                  {painel.dados.painel.faturamentoDaCarteira.toLocaleString('pt-BR', {
                    style: 'currency',
                    currency: 'BRL',
                    maximumFractionDigits: 0,
                  })}
                </span>
              </div>
            </div>
          </>
        )}
      </div>

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
      <div className="p360-grid">
        {/* A LINHA VAZIA DE DOZE MESES SAIU, E O LUGAR DELA TEM DADO.

            Ficava aqui um gráfico de linha desenhado com doze zeros e uma tampa
            por cima explicando que não há série mensal — o motivo continua
            verdadeiro (o faturamento parou na origem em 11/04/2025) e continua
            escrito, no rodapé. O que mudou é que a moldura deixou de gastar
            meia tela para não dizer nada.

            No lugar entra a única leitura por pessoa que tem lastro: como a
            carteira de cada CEN se divide entre as faixas de tempo sem contato.
            Mesmo agregado da tabela — gráfico e número nunca divergem. */}
        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Como a carteira de cada CEN está dividida</div>
              <div className="card-subtitle">
                {censNoGrafico.length > 0
                  ? `${censNoGrafico.length} maiores de ${cens.length} responsáveis · fatia de cada faixa de tempo sem contato`
                  : 'fatia de cada faixa de tempo sem contato'}
              </div>
            </div>
            <SeloProcedencia procedencia={resumo.procedencia} />
          </div>

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
                    {f.nome}{' '}
                    <strong>{cens.reduce((t, c) => t + f.medir(c), 0).toLocaleString('pt-BR')}</strong>
                  </span>
                ))}
              </div>
            </>
          )}
        </div>

        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Ranking por métrica</div>
              <div className="card-subtitle">
                as {Math.min(BARRAS, cens.length)} maiores · o mesmo agregado do gráfico ao lado
              </div>
            </div>
            <label className="cad-filtro">
              Métrica
              <select
                value={metricaDoRanking}
                onChange={(e) => setMetricaDoRanking(e.target.value as MetricaId)}
              >
                {METRICAS.map((m) => (
                  <option key={m.id} value={m.id}>
                    {m.rotulo}
                  </option>
                ))}
              </select>
            </label>
          </div>

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
        </div>
      </div>

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
            <div className="cad-tabela-wrap cad-so-largo">
              <table className="cad-tabela">
                <caption className="cad-so-leitor">
                  Cobertura de carteira por CEN da filial {contexto.empresa}
                </caption>
                <thead>
                  <tr>
                    {COLUNAS.map((coluna) => (
                      <th
                        key={coluna.rotulo}
                        scope="col"
                        aria-sort={
                          ordem === coluna.ordem ? (descendente ? 'descending' : 'ascending') : undefined
                        }
                      >
                        <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem)}>
                          {coluna.rotulo}
                          <span aria-hidden="true">
                            {ordem === coluna.ordem ? (descendente ? '▾' : '▴') : ''}
                          </span>
                        </button>
                      </th>
                    ))}
                    <th scope="col">Último contato na carteira</th>
                  </tr>
                </thead>
                <tbody>
                  {ordenados.map((c) => (
                    <tr key={c.responsavelNome}>
                      <td>
                        <div className="cad-link-forte">{c.responsavelNome}</div>
                        <div className="cad-sub">
                          {c.carteiras} {c.carteiras === 1 ? 'carteira' : 'carteiras'} ·{' '}
                          {c.linhas.join(', ')}
                        </div>
                      </td>
                      <td className="cad-mono">{c.clientes.toLocaleString('pt-BR')}</td>
                      <td>
                        <Fatia parte={c.em30} todo={c.clientes} />
                      </td>
                      <td>
                        <Fatia parte={c.em90} todo={c.clientes} />
                      </td>
                      <td className="cad-mono">
                        <span className={c.nunca > 0 ? 'cad-atencao' : undefined}>
                          {c.nunca.toLocaleString('pt-BR')}
                        </span>
                      </td>
                      <td className="cad-mono">
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

      {/* O AVISO DE QUE ESTA TELA ENCOLHEU MORAVA NO TOPO, EM LARANJA.

          Era a primeira coisa que a diretoria lia ao abrir a Performance, e o
          que ela dizia era uma justificativa. O motivo continua inteiro — cada
          métrica que saiu, com a data e o número que a derrubaram —, só que
          depois do que a tela de fato tem para mostrar. */}
      <BlocoRecolhivel
        titulo="O que esta tela mostrava e não tem como sustentar"
        resumo="faturamento por CEN, meta, processos por pessoa, tempo de atendimento e conversão"
      >
        <div className="cad-fichas">
          <LacunaConhecida
            metrica="Faturamento por CEN e ranking de vendas"
            desde="11/04/2025"
            motivo={
              'A tabela de notas fiscais do ERP (EXT_NFS) parou de receber carga em 11/04/2025. ' +
              'Ela continua respondendo à consulta e continua cheia — é exatamente por isso que o ' +
              'número que ela devolveria pareceria atual, e é o defeito de 17 meses que este ' +
              'projeto existe para corrigir. A ponte do CRM não lê esta tabela, de propósito.'
            }
          />
          <LacunaConhecida
            metrica="Atingimento de meta"
            motivo={
              'A tabela organizacao.Meta está vazia e não há fonte: nem o legado nem o protótipo ' +
              'têm meta carregada — no protótipo elas eram números escritos no JavaScript. Sem ' +
              'meta não existe percentual de atingimento, e um percentual sobre meta inventada é ' +
              'pior do que percentual nenhum.'
            }
          />
          <LacunaConhecida
            metrica="Processos e tarefas por CEN"
            motivo={
              'Falta rota. ProprietarioId existe na consulta de processos do domínio, mas o ' +
              'endpoint /api/v1/processos não o expõe; e /api/v1/tarefas só aceita "minhas", que ' +
              'usa o usuário do contexto de acesso. Sem um parâmetro de responsável ou um ' +
              'agregado por pessoa, não há como contar o pipeline nem a agenda de cada CEN sem ' +
              'baixar 45 mil processos e 103 mil tarefas para somar no navegador.'
            }
          />
          <LacunaConhecida
            metrica="Tempo médio de atendimento"
            motivo={
              'A coluna Duracao existe na origem e é ZERO em 122.812 de 122.812 interações do ' +
              'período. Teve 122.812 oportunidades de ser preenchida no ano e não foi preenchida ' +
              'nenhuma vez. Qualquer métrica de produtividade por tempo é impossível hoje ' +
              '(documento 25, §5).'
            }
          />
          <LacunaConhecida
            metrica="Taxa de conversão por CEN"
            motivo={
              'Depende das duas anteriores ao mesmo tempo: não há como contar ganhos e perdidos ' +
              'por responsável sem a rota, e o desfecho comercial da origem tem o problema do ' +
              'cancelamento — 3.675 cancelados contra 190 perdidos nesta filial, sem motivo ' +
              'declarado em nenhum deles.'
            }
          />
        </div>
      </BlocoRecolhivel>
    </>
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
