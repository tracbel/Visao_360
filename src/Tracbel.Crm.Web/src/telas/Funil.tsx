/**
 * Funil de Vendas — o funil do protótipo, agora com os processos reais.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — trocou a origem do dado, e só ela.
 *
 * **A forma é a mesma:** o funil em SVG com a legenda de conversão ao lado é o
 * porte de `mountFunil()` (`prototipo/referencia/assets/app.js`, linha 568), com
 * a mesma geometria, a mesma paleta e a mesma regra de rótulo. O que era uma
 * constante de sete fases no JavaScript agora vem de `/api/v1/relatorios/funil`,
 * que agrupa `processo.Processo` no banco dentro da filial do contexto.
 *
 * ---------------------------------------------------------------------------
 * AS DUAS MÉTRICAS DO ORIGINAL, E O QUE ACONTECE COM CADA UMA:
 *
 * | Aba | Situação |
 * |---|---|
 * | **Quantidade** | ✅ tem dado — 16.032 processos abertos, contados no banco |
 * | **Valor R$** | 🔴 **o funil aparece vazio, com o motivo** — 9 de 16.032 processos declaram valor (0,1%) |
 *
 * A aba de valor **não some**: ela continua ali, e ao ser escolhida o gráfico
 * mostra por que não pode ser desenhado. Uma tela de gerência que perde um
 * gráfico sem explicação é pior do que uma que mostra o gráfico vazio com o
 * motivo escrito — e some-lo faria parecer que a métrica nunca existiu.
 *
 * ---------------------------------------------------------------------------
 * O FUNIL NÃO FORÇA ESTREITAMENTO, e isso é do original: a largura de cada
 * faixa é proporcional ao valor absoluto. Uma fase com mais processos que a
 * anterior aparece mais larga, e a legenda marca `↑` na conversão. Forçar o
 * desenho a estreitar sempre desenharia uma conversão que não aconteceu.
 */

import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import {
  corDaFaixa,
  GraficoFunil,
  LegendaDoFunil,
  type FaixaDoFunil,
} from '../componentes/GraficoFunil';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { contarProcessosPorSituacao, obterFunil, obterVendasPerdidas } from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type { FaseDoFunil, FatiaDeVendaPerdida } from '../tipos/relacionamento';
import { formatarDinheiro } from './cadastro/formato';

/** Os desfechos que a tela conta, cada um com um `COUNT` no banco. */
const DESFECHOS = ['Aberto', 'Ganho', 'Perdido', 'Cancelado', 'Suspenso'] as const;

/** As duas métricas do funil do protótipo que fazem sentido no dado de hoje. */
type Metrica = 'quantidade' | 'valor';

export function Funil() {
  const { contexto } = useContextoDeAcesso();
  const [fluxoEscolhido, setFluxoEscolhido] = useState<string>('');
  const [metrica, setMetrica] = useState<Metrica>('quantidade');

  const funil = useRecurso((sinal) => obterFunil(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const vendas = useRecurso(
    (sinal) => obterVendasPerdidas(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const desfechos = useRecurso(
    async (sinal) => {
      const valores = await Promise.all(DESFECHOS.map((s) => contarProcessosPorSituacao(contexto, s, sinal)));
      const mapa = Object.fromEntries(DESFECHOS.map((s, i) => [s, valores[i]])) as Record<
        (typeof DESFECHOS)[number],
        number
      >;
      return { dados: mapa, procedencia: null };
    },
    [contexto.empresa, contexto.usuario],
  );

  const fases = useMemo(() => funil.dados?.itens ?? [], [funil.dados]);
  const contagens = desfechos.dados;

  /** Os fluxos que existem NO DADO, do maior para o menor. Nenhum é escrito aqui. */
  const fluxos = useMemo(() => {
    const mapa = new Map<string, { codigo: string; nome: string; processos: number }>();
    for (const f of fases) {
      const atual = mapa.get(f.tipoProcessoCodigo);
      if (atual) atual.processos += f.processos;
      else
        mapa.set(f.tipoProcessoCodigo, {
          codigo: f.tipoProcessoCodigo,
          nome: f.tipoProcessoNome,
          processos: f.processos,
        });
    }
    return [...mapa.values()].sort((a, b) => b.processos - a.processos);
  }, [fases]);

  /** O fluxo desenhado: o escolhido, ou o maior que existir. */
  const fluxoAtivo = fluxoEscolhido || fluxos[0]?.codigo || '';

  /** As fases do fluxo ativo, NA ORDEM DO FUNIL — é `faseOrdem`, do catálogo. */
  const doFluxo = useMemo(
    () =>
      fases
        .filter((f) => f.tipoProcessoCodigo === fluxoAtivo)
        .sort((a, b) => a.faseOrdem - b.faseOrdem || a.faseNome.localeCompare(b.faseNome)),
    [fases, fluxoAtivo],
  );

  const processosComValor = doFluxo.reduce((s, f) => s + f.processosComValor, 0);
  const processosDoFluxo = doFluxo.reduce((s, f) => s + f.processos, 0);

  /** O funil só é desenhável na métrica de valor se algum processo declarar valor. */
  const valorTemLastro = processosComValor > 0;

  const faixas: FaixaDoFunil[] = useMemo(
    () =>
      doFluxo.map((f, i) => ({
        chave: `${f.tipoProcessoCodigo}-${f.faseCodigo}`,
        rotulo: f.faseNome,
        valor: metrica === 'quantidade' ? f.processos : (f.valorTotal ?? 0),
        cor: corDaFaixa(i, doFluxo.length),
        texto:
          metrica === 'quantidade'
            ? f.processos.toLocaleString('pt-BR')
            : f.valorTotal === null
              ? 'sem valor'
              : formatarDinheiro(f.valorTotal),
      })),
    [doFluxo, metrica],
  );

  const somaDaMetrica =
    metrica === 'quantidade'
      ? processosDoFluxo
      : doFluxo.reduce((s, f) => s + (f.valorTotal ?? 0), 0);

  const abertos = fases.reduce((s, f) => s + f.processos, 0);
  const comValorGeral = fases.reduce((s, f) => s + f.processosComValor, 0);

  const razaoDeGanho =
    contagens && contagens.Ganho + contagens.Perdido > 0
      ? Math.round((contagens.Ganho / (contagens.Ganho + contagens.Perdido)) * 100)
      : null;

  const indicadores: Indicador[] = [
    {
      rotulo: 'Abertos',
      valor: contagens?.Aberto ?? null,
      deOnde: 'inclui fluxos que não são de venda — ver a nota do funil',
      semDado: 'sem processo ao alcance deste contexto',
    },
    { rotulo: 'Ganhos', valor: contagens?.Ganho ?? null, tom: 'bom', deOnde: 'encerrados com venda', semDado: '—' },
    { rotulo: 'Perdidos', valor: contagens?.Perdido ?? null, tom: 'atencao', deOnde: 'encerrados sem venda', semDado: '—' },
    {
      rotulo: 'Cancelados',
      valor: contagens?.Cancelado ?? null,
      tom: 'atencao',
      deOnde: 'um quarto do fluxo de vendas da origem termina assim',
      semDado: '—',
    },
    {
      rotulo: 'Declaram valor',
      valor: funil.dados ? comValorGeral : null,
      tom: 'atencao',
      deOnde: `de ${abertos.toLocaleString('pt-BR')} abertos — o resto não tem valor na origem`,
      semDado: '—',
    },
    {
      rotulo: 'Ganhos ÷ (ganhos + perdidos)',
      valor: razaoDeGanho === null ? null : `${razaoDeGanho}%`,
      deOnde: 'só os dois desfechos comerciais — não cobre os cancelados',
      semDado: 'nenhum encerrado com desfecho comercial',
    },
  ];

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Funil de Vendas</h1>
          <p className="page-subtitle">
            <code>processo.Processo</code> — o funil por fase e os desfechos, agrupados no banco
            dentro da filial do cabeçalho.
          </p>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={funil.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={desfechos.carregando} />

      {desfechos.erro && <BlocoErro erro={desfechos.erro} aoTentarDeNovo={desfechos.recarregar} />}

      <MetricasSemDado metricas={funil.dados?.metricasSemDado} />

      {/* ---------------------------------------------------------------- */}
      {/* O funil, com a mesma forma do protótipo                          */}
      {/* ---------------------------------------------------------------- */}
      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Funil de Vendas</div>
            <div className="card-subtitle">
              {metrica === 'quantidade'
                ? `Soma de processos abertos: ${somaDaMetrica.toLocaleString('pt-BR')}`
                : valorTemLastro
                  ? `Soma de valor declarado: ${formatarDinheiro(somaDaMetrica)}`
                  : 'Soma de valor: sem dado'}
            </div>
          </div>
          <SeloProcedencia procedencia={funil.procedencia} />
        </div>

        <div className="cad-barra">
          <label className="cad-filtro">
            Fluxo
            <select value={fluxoAtivo} onChange={(e) => setFluxoEscolhido(e.target.value)}>
              {fluxos.map((f) => (
                <option key={f.codigo} value={f.codigo}>
                  {f.nome} ({f.processos.toLocaleString('pt-BR')})
                </option>
              ))}
            </select>
          </label>

          {/* As abas de métrica do original. A de valor CONTINUA AQUI mesmo sem
              lastro: escondê-la faria parecer que a métrica nunca existiu. */}
          <div className="cad-filtro" role="group" aria-label="Métrica do funil">
            Métrica
            <div className="funil-abas">
              <button
                type="button"
                className={metrica === 'quantidade' ? 'funil-aba ativa' : 'funil-aba'}
                aria-pressed={metrica === 'quantidade'}
                onClick={() => setMetrica('quantidade')}
              >
                Quantidade
              </button>
              <button
                type="button"
                className={metrica === 'valor' ? 'funil-aba ativa' : 'funil-aba'}
                aria-pressed={metrica === 'valor'}
                onClick={() => setMetrica('valor')}
              >
                Valor R$
              </button>
            </div>
          </div>
        </div>

        {funil.carregando && <BlocoCarregando oQue="o funil" />}
        {funil.erro && <BlocoErro erro={funil.erro} aoTentarDeNovo={funil.recarregar} />}

        {funil.dados && doFluxo.length === 0 && !funil.erro && (
          <BlocoVazio
            titulo="Nenhum processo aberto neste fluxo"
            texto="O funil conta só o que está aberto. Troque o fluxo, ou confira a filial escolhida no cabeçalho."
          />
        )}

        {doFluxo.length > 0 && metrica === 'valor' && !valorTemLastro && (
          /* O GRÁFICO NÃO SOME — ele explica. É a regra em forma de tela. */
          <div className="funil-container">
            <div className="funil-sem-dado" role="note">
              <strong>O funil por valor não pode ser desenhado.</strong>
              <p>
                Nenhum dos {processosDoFluxo.toLocaleString('pt-BR')} processos abertos deste fluxo
                declara valor na origem. Desenhar as faixas com zero mostraria um funil de
                R$ 0,00 com a forma de um funil de verdade — e é exatamente esse tipo de número
                plausível e errado que este projeto existe para corrigir.
              </p>
              <p>
                Na base inteira são <strong>358 de 45.397 processos (0,8%)</strong> com valor
                declarado. Volte para <strong>Quantidade</strong>, que tem dado, ou escolha outro
                fluxo.
              </p>
            </div>
            <LegendaDoFunil faixas={faixas} />
          </div>
        )}

        {/* A RESSALVA FICA COLADA NO GRÁFICO, e não só no aviso do topo. Quem
            olha um funil desenhado lê a forma antes de ler qualquer outra coisa
            da tela; a fração que o sustenta precisa estar no mesmo campo de
            visão que as faixas. */}
        {doFluxo.length > 0 && metrica === 'valor' && valorTemLastro && (
          <div className="funil-cobertura" role="note">
            <strong>
              Este funil está desenhado sobre {processosComValor} de{' '}
              {processosDoFluxo.toLocaleString('pt-BR')} processos
            </strong>{' '}
            ({((processosComValor / Math.max(processosDoFluxo, 1)) * 100).toFixed(1)}%) — os únicos
            que declaram valor na origem. As faixas marcadas <em>sem valor</em> não valem zero: elas
            não têm valor informado. A soma no cabeçalho é a desses {processosComValor}, e não a do
            funil.
          </div>
        )}

        {doFluxo.length > 0 && (metrica === 'quantidade' || valorTemLastro) && (
          <div className="funil-container">
            <GraficoFunil faixas={faixas} />
            <LegendaDoFunil faixas={faixas} />
          </div>
        )}

        {doFluxo.length > 0 && (
          <div className="funil-scope-note">
            <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden="true">
              <circle cx="12" cy="12" r="9" />
              <path d="M12 8h.01" />
              <path d="M11 12h1v4h1" />
            </svg>
            <div>
              <strong>Escopo:</strong> as fases vêm do dado, e não de uma lista escrita na tela — o
              catálogo de fases foi carregado do próprio uso de 2026, em 50 pares fluxo × fase. As
              faixas <strong>não são forçadas a estreitar</strong>: a largura é proporcional ao
              número de processos, e uma fase maior que a anterior aparece maior, com{' '}
              <strong>↑</strong> na coluna de conversão.{' '}
              {metrica === 'quantidade' && processosComValor > 0 && (
                <>
                  Nesta seleção, {processosComValor} de {processosDoFluxo.toLocaleString('pt-BR')}{' '}
                  processos declaram valor.
                </>
              )}
            </div>
          </div>
        )}
      </div>

      {/* ---------------------------------------------------------------- */}
      {/* O detalhamento por fase, que a tabela do original ocupava         */}
      {/* ---------------------------------------------------------------- */}
      {/* OS 32 PARES FLUXO × FASE, FECHADOS POR PADRÃO.

          O funil acima já responde a pergunta da gerência — onde o processo
          para. Esta tabela responde a pergunta seguinte, que é de quem vai
          agir: em qual fluxo, exatamente. Aberta, ela respondia sozinha por
          cerca de 1.700px da altura da tela.

          O aviso sobre os fluxos que não são de venda veio para dentro dela: é
          esta lista que ele explica. */}
      <BlocoRecolhivel
        titulo="Detalhamento por fase"
        resumo={`todos os ${fases.length} pares fluxo × fase desta filial, e não só o fluxo desenhado`}
      >
        <div className="cad-aviso cad-aviso-atencao" role="note">
          <strong>O número de abertos não é o tamanho do funil comercial.</strong>
          <p>
            <strong>16.422 processos</strong> de fluxos que não são de venda — aferição de
            qualidade, pré-entrega, entrega física e prospecção de peças — ficam abertos para
            sempre, porque o domínio de situação não tem o valor <code>Encerrado</code>. Eles
            inflam o funil em <strong>46%</strong>. O seletor de fluxo acima separa o que é venda do
            que não é.
          </p>
        </div>

        {fases.length > 0 && (
          <div className="cad-tabela-wrap">
            <table className="cad-tabela">
              <caption className="cad-so-leitor">Processos abertos por fluxo e fase</caption>
              <thead>
                <tr>
                  <th scope="col">Fluxo</th>
                  <th scope="col">Fase</th>
                  <th scope="col">Processos</th>
                  <th scope="col">Peso</th>
                  <th scope="col">Declaram valor</th>
                  <th scope="col">Valor somado</th>
                </tr>
              </thead>
              <tbody>
                {[...fases]
                  .sort((a, b) => b.processos - a.processos)
                  .map((f) => (
                    <LinhaDoFunil
                      key={`${f.tipoProcessoCodigo}-${f.faseCodigo}`}
                      fase={f}
                      maior={Math.max(1, ...fases.map((x) => x.processos))}
                    />
                  ))}
              </tbody>
            </table>
          </div>
        )}
      </BlocoRecolhivel>

      {/* ---------------------------------------------------------------- */}
      {/* Perdas por motivo                                                 */}
      {/* ---------------------------------------------------------------- */}
      {/* PERDAS POR MOTIVO — E O MOTIVO VEM DO FORMULÁRIO, NÃO DO PROCESSO.

          Este cartão lia `/relatorios/perdas`, que agrupa `processo.Processo` por motivo. Ali o
          motivo é "não informado" em 100% das linhas, e a tabela tinha uma linha só. Não era o
          legado que não registrava: o Vórtice registra a derrota num FORMULÁRIO
          (`IV_Q_VENDA_PERDIDA_FY25`), com motivo, concorrente e os dois preços, e ninguém tinha
          olhado ali.

          As duas populações continuam separadas de propósito — processos perdidos e formulários
          preenchidos são números diferentes, e a diferença é quantas derrotas ninguém
          registrou. */}
      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Vendas perdidas por motivo</div>
            <div className="card-subtitle">
              {vendas.dados
                ? `${vendas.dados.registradas.toLocaleString('pt-BR')} de ${vendas.dados.processosPerdidos.toLocaleString('pt-BR')} processos perdidos têm o formulário preenchido`
                : 'do formulário de venda perdida, contadas no banco'}
            </div>
          </div>
          <SeloProcedencia procedencia={vendas.procedencia} />
        </div>

        {vendas.carregando && <BlocoCarregando oQue="as vendas perdidas" />}
        {vendas.erro && <BlocoErro erro={vendas.erro} aoTentarDeNovo={vendas.recarregar} />}

        <MetricasSemDado
          metricas={vendas.dados?.metricasSemDado}
          titulo="O que a distribuição de perdas não diz"
        />

        {vendas.dados && vendas.dados.registradas === 0 && !vendas.erro && (
          <BlocoVazio
            titulo="Nenhuma venda perdida registrada nesta filial"
            texto="O motivo, o concorrente e a diferença de preço só existem quando o CEN preenche o formulário de venda perdida ao encerrar o processo."
          />
        )}

        {vendas.dados && vendas.dados.porMotivo.length > 0 && (
          <div className="cad-tabela-wrap">
            <table className="cad-tabela">
              <caption className="cad-so-leitor">Vendas perdidas por motivo</caption>
              <thead>
                <tr>
                  <th scope="col">Motivo</th>
                  <th scope="col">Vendas perdidas</th>
                  <th scope="col">Máquinas</th>
                  <th scope="col">Nosso preço acima, em média</th>
                </tr>
              </thead>
              <tbody>
                {vendas.dados.porMotivo.map((m) => (
                  <tr key={m.codigo}>
                    <td>{m.nome}</td>
                    <td className="cad-mono">{m.quantidade.toLocaleString('pt-BR')}</td>
                    <td className="cad-mono">{m.maquinas.toLocaleString('pt-BR')}</td>
                    <td className="cad-mono">
                      <Diferenca fatia={m} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* PARA QUEM PERDEMOS. É a pergunta seguinte à do motivo, e o formulário responde: a
          marca, e a diferença de preço média contra ela. */}
      {vendas.dados && vendas.dados.porConcorrente.length > 0 && (
        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Para quem perdemos</div>
              <div className="card-subtitle">
                o fabricante que levou a venda, e a diferença de preço contra ele
              </div>
            </div>
            <SeloProcedencia procedencia={vendas.procedencia} />
          </div>

          <div className="cad-tabela-wrap">
            <table className="cad-tabela">
              <caption className="cad-so-leitor">Vendas perdidas por concorrente</caption>
              <thead>
                <tr>
                  <th scope="col">Concorrente</th>
                  <th scope="col">Vendas perdidas</th>
                  <th scope="col">Máquinas</th>
                  <th scope="col">Nosso preço acima, em média</th>
                </tr>
              </thead>
              <tbody>
                {vendas.dados.porConcorrente.map((c) => (
                  <tr key={c.codigo}>
                    <td className="cad-link-forte">{c.nome}</td>
                    <td className="cad-mono">{c.quantidade.toLocaleString('pt-BR')}</td>
                    <td className="cad-mono">{c.maquinas.toLocaleString('pt-BR')}</td>
                    <td className="cad-mono">
                      <Diferenca fatia={c} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <BlocoRecolhivel
        titulo="O que este relatório não pode afirmar"
        resumo="valor do funil, probabilidade de fechamento, meta e faturamento"
      >
        <div className="cad-fichas">
          <LacunaConhecida
            metrica="Valor do funil e ticket médio"
            motivo={
              '358 de 45.397 processos (0,8%) declaram valor na origem; nesta filial são 9 de ' +
              '16.032 (0,1%). A aba "Valor R$" do funil continua na tela e mostra esse motivo no ' +
              'lugar das faixas, em vez de desenhar um funil de R$ 0,00.'
            }
          />
          <LacunaConhecida
            metrica="Probabilidade de fechamento por fase"
            motivo={
              'Fase.ProbabilidadePercentual é nula nas 50 fases carregadas. A origem tem uma coluna ' +
              'Perspectiva preenchida em 3.922 processos, mas por processo e não por fase — não dá ' +
              'para derivar dela a probabilidade de uma coluna do funil.'
            }
          />
          <LacunaConhecida
            metrica="Atingimento de meta"
            motivo={
              'A tabela organizacao.Meta está vazia. Não há fonte no legado nem no protótipo: lá as ' +
              'metas eram números escritos no JavaScript.'
            }
          />
          <LacunaConhecida
            metrica="Faturamento realizado"
            desde="11/04/2025"
            motivo={
              'A tabela de notas fiscais do ERP (EXT_NFS) parou de receber carga em 11/04/2025. Ela ' +
              'continua respondendo à consulta e continua cheia, e é por isso que o número que ela ' +
              'devolveria pareceria atual.'
            }
          />
        </div>
      </BlocoRecolhivel>
    </>
  );
}

/** Uma linha do detalhamento: a fase, o peso dela, e a cobertura do valor. */
function LinhaDoFunil({ fase, maior }: { fase: FaseDoFunil; maior: number }) {
  const proporcao = Math.round((fase.processos / maior) * 100);

  return (
    <tr>
      <td>{fase.tipoProcessoNome}</td>
      <td>
        <div className="cad-link-forte">{fase.faseNome}</div>
        <div className="cad-sub cad-mono">{fase.faseCodigo}</div>
      </td>
      <td className="cad-mono">{fase.processos.toLocaleString('pt-BR')}</td>
      <td>
        <span className="cad-barra-trilho" aria-hidden="true">
          <span className="cad-barra-mini" style={{ width: `${proporcao}%` }} />
        </span>
        <span className="cad-so-leitor">{proporcao}% da maior fase</span>
      </td>
      <td className="cad-mono">
        {fase.processosComValor === 0 ? (
          <span className="cad-nada">nenhum</span>
        ) : (
          `${fase.processosComValor} de ${fase.processos.toLocaleString('pt-BR')}`
        )}
      </td>
      <td className="cad-mono">
        {fase.valorTotal === null ? (
          <span className="cad-nada">sem valor declarado</span>
        ) : (
          formatarDinheiro(fase.valorTotal)
        )}
      </td>
    </tr>
  );
}

/**
 * A diferença média de preço, com o denominador ao lado.
 *
 * O número sozinho diria "perdemos por R$ 52 mil", sem dizer sobre quantas negociações a média
 * foi feita — e a média de seis linhas não é a mesma coisa que a média de noventa e oito. O
 * denominador vem junto, sempre, e quando ele é zero o que aparece é o motivo, não um zero.
 */
function Diferenca({ fatia }: { fatia: FatiaDeVendaPerdida }) {
  if (fatia.diferencaMediaDePreco === null || fatia.comOsDoisPrecos === 0) {
    return <span className="cad-nada">sem os dois preços</span>;
  }

  return (
    <>
      <div>
        {fatia.diferencaMediaDePreco.toLocaleString('pt-BR', {
          style: 'currency',
          currency: 'BRL',
          maximumFractionDigits: 0,
        })}
      </div>
      <div className="cad-sub">
        de {fatia.comOsDoisPrecos} de {fatia.quantidade}
      </div>
    </>
  );
}
