/**
 * Pipeline de Vendas — ligado ao dado real de `processo.Processo`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a tela deixou de ler o JSON do protótipo.
 *
 * Antes, o kanban tinha **seis colunas escritas no código** — Qualificação,
 * Diagnóstico, Proposta, Negociação, Fechamento, Ganho/Perdido — e os cartões
 * vinham de `public/dados/pipeline.json`. A carga de 2026 mostrou que **quatro
 * dessas seis fases não existem no dado**: o funil real do fluxo de vendas é
 * Apresentação → Negociação → Pedido de Venda → Montagem → Análise → …, e a
 * coluna com 14.302 dos 15.463 processos abertos chama-se **Apresentação**,
 * nome que o protótipo não tinha (documento 25, §8.1).
 *
 * **As colunas vêm do dado, não de uma lista escrita aqui.** Uma coluna de
 * kanban vazia é pior do que uma coluna com nome estranho — as seis do
 * protótipo produziriam cinco colunas vazias e uma com tudo dentro.
 *
 * ---------------------------------------------------------------------------
 * AS DUAS COISAS QUE ESTA TELA NÃO PODE AFIRMAR, e como ela diz isso:
 *
 * 1. **Valor do funil e ticket médio não têm dado.** 358 de 45.397 processos
 *    (0,8%) declaram valor; na filial de Ribeirão Preto são 9 de 16.032 (0,1%).
 *    A coluna mostra **quantos processos sustentam o valor** ao lado do valor,
 *    sempre — e a API devolve `valorTotal` nulo, e não zero, quando nenhum
 *    declara. Somar essa coluna e chamar o resultado de "valor do funil" seria
 *    exatamente o defeito que este projeto existe para corrigir.
 *
 * 2. **A contagem por fase é confiável; o FILTRO por fase, não.** O agregado
 *    `/relatorios/funil` agrupa no banco e está certo. Já a listagem
 *    `/api/v1/processos` aplica `faseCodigo` e `tipoProcessoCodigo` **depois**
 *    de o banco paginar — medido: `?faseCodigo=APRESENTACAO&tamanho=25`
 *    devolve 12 itens e continua declarando `total: 16.039`. Oferecer esse
 *    filtro na lista mostraria uma contagem que não corresponde às linhas.
 *    Por isso a lista abaixo filtra só pelo que o banco resolve — busca,
 *    situação e encerrados — e a dívida está registrada como **P-7**.
 */

import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { listarProcessos, obterFunil, PROCESSOS_INICIAL } from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import {
  SITUACOES_DE_PROCESSO,
  type ConsultaDeProcessos,
  type FaseDoFunil,
  type OrdemDeProcesso,
  type ProcessoResumo,
} from '../tipos/relacionamento';
import { formatarData, formatarDinheiro } from './cadastro/formato';

/** As colunas da lista, e quais delas a API sabe ordenar. */
const COLUNAS: { rotulo: string; ordem?: OrdemDeProcesso }[] = [
  { rotulo: 'Processo', ordem: 'Titulo' },
  { rotulo: 'Cliente' },
  { rotulo: 'Fluxo e fase' },
  { rotulo: 'Parado há', ordem: 'FaseDesde' },
  { rotulo: 'Valor', ordem: 'ValorEstimado' },
  { rotulo: 'Previsão', ordem: 'PrevisaoConclusao' },
  { rotulo: 'Situação' },
];

/** Espera antes de mandar a busca à API, para não consultar a cada tecla. */
const ESPERA_DA_BUSCA_MS = 350;

export function Pipeline() {
  const { contexto } = useContextoDeAcesso();

  const funil = useRecurso(
    (sinal) => obterFunil(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const [consulta, setConsulta] = useState<ConsultaDeProcessos>(PROCESSOS_INICIAL);
  const [termoDigitado, setTermoDigitado] = useState('');
  const [fluxoEscolhido, setFluxoEscolhido] = useState<string | null>(null);

  // O que está no campo e o que foi pedido à API são coisas diferentes: sem essa
  // separação, cada tecla vira uma requisição e a lista pisca a cada letra.
  useEffect(() => {
    const relogio = setTimeout(
      () => setConsulta((c) => (c.termo === termoDigitado ? c : { ...c, termo: termoDigitado, pagina: 1 })),
      ESPERA_DA_BUSCA_MS,
    );
    return () => clearTimeout(relogio);
  }, [termoDigitado]);

  const lista = useRecurso(
    (sinal) => listarProcessos(contexto, consulta, sinal),
    [contexto.empresa, contexto.usuario, JSON.stringify(consulta)],
  );

  const fases = funil.dados?.itens ?? [];

  /** Os fluxos que existem NO DADO, do maior para o menor. Nenhum é escrito aqui. */
  const fluxos = useMemo(() => {
    const mapa = new Map<string, { codigo: string; nome: string; processos: number }>();
    for (const f of fases) {
      const atual = mapa.get(f.tipoProcessoCodigo);
      if (atual) atual.processos += f.processos;
      else mapa.set(f.tipoProcessoCodigo, {
        codigo: f.tipoProcessoCodigo,
        nome: f.tipoProcessoNome,
        processos: f.processos,
      });
    }
    return [...mapa.values()].sort((a, b) => b.processos - a.processos);
  }, [fases]);

  /** O fluxo aberto no kanban: o escolhido, ou o maior que existir. */
  const fluxoAtivo = fluxoEscolhido ?? fluxos[0]?.codigo ?? '';

  /** As colunas do fluxo ativo, na ordem que a fase declara. */
  const colunas = useMemo(
    () =>
      fases
        .filter((f) => f.tipoProcessoCodigo === fluxoAtivo)
        .sort((a, b) => a.faseOrdem - b.faseOrdem || a.faseNome.localeCompare(b.faseNome)),
    [fases, fluxoAtivo],
  );

  const maiorColuna = Math.max(1, ...colunas.map((c) => c.processos));

  const abertos = fases.reduce((s, f) => s + f.processos, 0);
  const comValor = fases.reduce((s, f) => s + f.processosComValor, 0);

  const indicadores: Indicador[] = [
    {
      rotulo: 'Processos abertos',
      valor: funil.dados ? abertos : null,
      deOnde: 'agrupados no banco por fluxo e fase',
      semDado: 'sem processo ao alcance deste contexto',
    },
    {
      rotulo: 'Fluxos com processo',
      valor: funil.dados ? fluxos.length : null,
      deOnde: 'tipos de processo com pelo menos um aberto',
      semDado: '—',
    },
    {
      rotulo: 'Pares fluxo × fase',
      valor: funil.dados ? fases.length : null,
      deOnde: 'as colunas que existem no dado, não uma lista fixa',
      semDado: '—',
    },
    {
      rotulo: 'Declaram valor',
      valor: funil.dados ? comValor : null,
      tom: 'atencao',
      deOnde: `de ${abertos.toLocaleString('pt-BR')} abertos — o resto não tem valor na origem`,
      semDado: '—',
    },
  ];

  const temFiltro = useMemo(
    () => consulta.termo !== '' || consulta.situacao !== '' || consulta.incluirEncerrados,
    [consulta],
  );

  function trocarOrdem(campo: OrdemDeProcesso) {
    setConsulta((c) =>
      c.ordenarPor === campo
        ? { ...c, descendente: !c.descendente, pagina: 1 }
        : { ...c, ordenarPor: campo, descendente: false, pagina: 1 },
    );
  }

  function limparFiltros() {
    setTermoDigitado('');
    setConsulta({ ...PROCESSOS_INICIAL, tamanho: consulta.tamanho });
  }

  const pagina = lista.dados;

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Pipeline de Vendas</h1>
          <p className="page-subtitle">
            <code>processo.Processo</code> — 45.397 processos migrados do Vórtice, recorte de 2026.
            As colunas do funil vêm do dado, não de uma lista escrita na tela.
          </p>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={funil.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={funil.carregando} />

      <MetricasSemDado metricas={funil.dados?.metricasSemDado} />

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Funil por fase</div>
            <div className="card-subtitle">
              Agrupado no banco, dentro da filial {contexto.empresa} · a fase que aparece com{' '}
              <em>sem valor declarado</em> não tem nenhum processo informando valor na origem
            </div>
          </div>
          <SeloProcedencia procedencia={funil.procedencia} />
        </div>

        {funil.carregando && <BlocoCarregando oQue="o funil" />}
        {funil.erro && <BlocoErro erro={funil.erro} aoTentarDeNovo={funil.recarregar} />}

        {funil.dados && fluxos.length === 0 && !funil.erro && (
          <BlocoVazio
            titulo="Nenhum processo aberto nesta filial"
            texto="O funil conta só o que está aberto. Se esta filial não mostra nenhuma coluna, confira a filial escolhida no cabeçalho."
          />
        )}

        {fluxos.length > 0 && (
          <>
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
              <p className="cad-ajuda-campo">
                Um kanban com os {fases.length} pares fluxo × fase de uma vez não se lê. O seletor
                abre um fluxo por vez, e a lista completa vem do dado.
              </p>
            </div>

            <div className="cad-colunas">
              {colunas.map((coluna) => (
                <ColunaDoFunil key={coluna.faseCodigo} coluna={coluna} maior={maiorColuna} />
              ))}
            </div>
          </>
        )}
      </div>

      <div className="cad-barra">
        <div className="cad-busca">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            type="search"
            aria-label="Buscar processo por título ou número"
            placeholder="Buscar por título ou número do processo…"
            value={termoDigitado}
            onChange={(e) => setTermoDigitado(e.target.value)}
          />
        </div>

        <label className="cad-filtro">
          Situação
          <select
            value={consulta.situacao}
            onChange={(e) =>
              setConsulta((c) => ({
                ...c,
                situacao: e.target.value,
                // Filtrar por um desfecho sem incluir os encerrados devolveria
                // lista vazia: o encerrado é justamente o que se está pedindo.
                incluirEncerrados: e.target.value !== '' && e.target.value !== 'Aberto',
                pagina: 1,
              }))
            }
          >
            <option value="">Todas</option>
            {SITUACOES_DE_PROCESSO.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro cad-filtro-caixa">
          <input
            type="checkbox"
            checked={consulta.incluirEncerrados}
            onChange={(e) => setConsulta((c) => ({ ...c, incluirEncerrados: e.target.checked, pagina: 1 }))}
          />
          Incluir encerrados
        </label>

        {temFiltro && (
          <button type="button" className="btn btn-secondary btn-sm" onClick={limparFiltros}>
            Limpar filtros
          </button>
        )}
      </div>

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Processos desta filial</div>
            <div className="card-subtitle">
              {lista.recarregando ? 'Atualizando…' : `${(pagina?.total ?? 0).toLocaleString('pt-BR')} no total`}
            </div>
          </div>
          <SeloProcedencia procedencia={lista.procedencia} />
        </div>

        {lista.carregando && <BlocoCarregando oQue="os processos" />}
        {lista.erro && <BlocoErro erro={lista.erro} aoTentarDeNovo={lista.recarregar} />}

        {pagina && !lista.erro && pagina.itens.length === 0 && (
          <BlocoVazio
            titulo={temFiltro ? 'Nenhum processo com esses filtros' : 'Esta filial não tem processo carregado'}
            texto={
              temFiltro
                ? 'A busca compara o título por trecho e o número por valor inteiro. Situação é domínio fechado — quem procura um desfecho precisa dos encerrados incluídos.'
                : 'A carga de 2026 trouxe processo para as treze filiais em operação. Confira a filial escolhida no cabeçalho.'
            }
            acao={
              temFiltro ? (
                <button type="button" className="btn btn-secondary" onClick={limparFiltros}>
                  Limpar filtros
                </button>
              ) : undefined
            }
          />
        )}

        {pagina && pagina.itens.length > 0 && (
          <>
            <div className="cad-tabela-wrap cad-so-largo">
              <table className="cad-tabela">
                <caption className="cad-so-leitor">
                  Processos da filial {contexto.empresa}, ordenados por {consulta.ordenarPor}
                </caption>
                <thead>
                  <tr>
                    {COLUNAS.map((coluna) => (
                      <th key={coluna.rotulo} scope="col" aria-sort={ariaOrdem(coluna.ordem, consulta)}>
                        {coluna.ordem ? (
                          <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem!)}>
                            {coluna.rotulo}
                            <span aria-hidden="true">{seta(coluna.ordem, consulta)}</span>
                          </button>
                        ) : (
                          coluna.rotulo
                        )}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {pagina.itens.map((p) => (
                    <tr key={p.chave}>
                      <td>
                        <Link to={`/oportunidades/${p.chave}`} className="cad-link-forte">
                          {p.titulo}
                        </Link>
                        <div className="cad-sub cad-mono">nº {p.numero}</div>
                        {p.proprietarioNome && <div className="cad-sub">{p.proprietarioNome}</div>}
                      </td>
                      <td>
                        <Link to={`/clientes/${p.clienteChave}`} className="cad-link-forte">
                          {p.clienteNome}
                        </Link>
                      </td>
                      <td>
                        <div>{p.faseNome}</div>
                        <div className="cad-sub">{p.tipoProcessoNome}</div>
                      </td>
                      <td className="cad-mono">
                        <span className={p.diasNaFase > 90 ? 'cad-alerta' : undefined}>
                          {p.diasNaFase.toLocaleString('pt-BR')} {p.diasNaFase === 1 ? 'dia' : 'dias'}
                        </span>
                        <div className="cad-sub">desde {formatarData(p.faseDesde)}</div>
                      </td>
                      <td className="cad-mono">
                        {p.valorEstimado === null ? (
                          <span className="cad-nada" title="A origem não declara valor em 99,2% dos processos.">
                            não declarado
                          </span>
                        ) : (
                          formatarDinheiro(p.valorEstimado)
                        )}
                      </td>
                      <td className="cad-mono">
                        {p.previsaoConclusao ? (
                          formatarData(p.previsaoConclusao)
                        ) : (
                          <span className="cad-nada">sem previsão</span>
                        )}
                      </td>
                      <td>
                        <span className={`cad-selo cad-selo-${p.situacao.toLowerCase()}`}>{p.situacao}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="cad-fichas cad-so-estreito">
              {pagina.itens.map((p) => (
                <FichaDeProcesso key={p.chave} processo={p} />
              ))}
            </div>

            <BarraDePaginacao
              pagina={pagina}
              oQue="processos"
              aoTrocarPagina={(p) => setConsulta((c) => ({ ...c, pagina: p }))}
              aoTrocarTamanho={(t) => setConsulta((c) => ({ ...c, tamanho: t, pagina: 1 }))}
            />
          </>
        )}
      </div>

      <BlocoRecolhivel
        titulo="O que esta tela ainda não faz, e por quê"
        resumo="filtro por fase na lista e mudança de fase pelo quadro"
      >
        <div className="cad-fichas">
          <p className="cad-estado-texto">
            <strong>Não há filtro por fase na lista.</strong> A contagem por fase do quadro acima é
            confiável — ela sai de um <code>GROUP BY</code> no banco. Já a listagem aplica{' '}
            <code>faseCodigo</code> <em>depois</em> de o banco paginar: medido ao vivo,{' '}
            <code>?faseCodigo=APRESENTACAO&amp;tamanho=25</code> devolve 12 linhas e continua
            declarando <code>total: 16.039</code>. Oferecer esse filtro mostraria uma contagem que
            não corresponde às linhas. Registrado como dívida <strong>P-7</strong>.
          </p>
          <p className="cad-estado-texto">
            <strong>Mudar de fase, fechar e marcar como perdida saíram.</strong> Nenhuma rota de
            {/* A citação "(dívida D-9 do documento 23)" saiu da tela pelo mesmo motivo das outras. */}
            relacionamento escreve: mudar de fase dispara automação e concluir tarefa gera a
            próxima, e o motor de regras entra junto com a tela que o exercita.
          </p>
        </div>
      </BlocoRecolhivel>
    </>
  );
}

/** Uma coluna do funil: quantos processos, e quanto disso tem valor declarado. */
function ColunaDoFunil({ coluna, maior }: { coluna: FaseDoFunil; maior: number }) {
  const proporcao = Math.round((coluna.processos / maior) * 100);

  return (
    <div className="cad-coluna">
      <div className="cad-coluna-cabecalho">
        <div className="cad-coluna-nome">{coluna.faseNome}</div>
        <div className="cad-coluna-contagem">{coluna.processos.toLocaleString('pt-BR')}</div>
        <span className="cad-barra-trilho" aria-hidden="true">
          <span className="cad-barra-mini" style={{ width: `${proporcao}%` }} />
        </span>
      </div>
      <div className="cad-coluna-corpo">
        {/* O VALOR VEM SEMPRE COM QUANTOS PROCESSOS O SUSTENTAM. Sozinho, ele
            pareceria o valor da fase inteira — e representa 0,1% dela. */}
        {coluna.valorTotal === null ? (
          /* A FRASE INTEIRA ERA REPETIDA EM CADA COLUNA — sete vezes na mesma
             tela, com só o número mudando, e o kanban virava um bloco de texto.
             O aviso completo é dado uma vez acima do funil; aqui fica o
             travessão de valor ausente, com o motivo no `title`. */
          <div
            className="cad-coluna-meta cad-nada"
            title={`Nenhum dos ${coluna.processos.toLocaleString('pt-BR')} processos desta fase informa valor na origem.`}
          >
            — sem valor declarado
          </div>
        ) : (
          <div className="cad-coluna-meta">
            <strong>{formatarDinheiro(coluna.valorTotal)}</strong>
            <br />
            somados de {coluna.processosComValor} de {coluna.processos.toLocaleString('pt-BR')}{' '}
            processos — o resto não declara valor.
          </div>
        )}
      </div>
    </div>
  );
}

/** O mesmo processo, em ficha, para quando a tabela não cabe. */
function FichaDeProcesso({ processo }: { processo: ProcessoResumo }) {
  return (
    <div className="cad-ficha">
      <div className="cad-ficha-titulo">
        <Link to={`/oportunidades/${processo.chave}`}>{processo.titulo}</Link>
      </div>
      <div className="cad-sub cad-mono">nº {processo.numero}</div>

      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Cliente</span>
        <span className="cad-ficha-valor">
          <Link to={`/clientes/${processo.clienteChave}`}>{processo.clienteNome}</Link>
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Fluxo</span>
        <span className="cad-ficha-valor">{processo.tipoProcessoNome}</span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Fase</span>
        <span className="cad-ficha-valor">{processo.faseNome}</span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Parado há</span>
        <span className="cad-ficha-valor">
          {processo.diasNaFase.toLocaleString('pt-BR')} {processo.diasNaFase === 1 ? 'dia' : 'dias'}
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Valor</span>
        <span className="cad-ficha-valor">
          {processo.valorEstimado === null ? (
            <span className="cad-nada">não declarado</span>
          ) : (
            formatarDinheiro(processo.valorEstimado)
          )}
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Situação</span>
        <span className="cad-ficha-valor">
          <span className={`cad-selo cad-selo-${processo.situacao.toLowerCase()}`}>{processo.situacao}</span>
        </span>
      </div>
    </div>
  );
}

function ariaOrdem(campo: OrdemDeProcesso | undefined, consulta: ConsultaDeProcessos) {
  if (!campo || consulta.ordenarPor !== campo) return undefined;
  return consulta.descendente ? ('descending' as const) : ('ascending' as const);
}

function seta(campo: OrdemDeProcesso | undefined, consulta: ConsultaDeProcessos) {
  if (!campo || consulta.ordenarPor !== campo) return '';
  return consulta.descendente ? '▾' : '▴';
}
