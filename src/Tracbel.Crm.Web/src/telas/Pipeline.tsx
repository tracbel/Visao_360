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
 * 1. **Valor do funil e ticket médio quase não têm dado.** No Vórtice, menos
 *    de 1% dos processos declara valor (documento 25, medido na carga antiga).
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

import { CircleDollarSign, Columns3, FolderOpen, ListFilter, RefreshCw, Search, Workflow } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
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
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/territorio.css';

/** As colunas da lista, quais delas a API sabe ordenar, e quais são número (alinhadas à direita). */
const COLUNAS: { rotulo: string; ordem?: OrdemDeProcesso; numerica?: boolean }[] = [
  { rotulo: 'Processo', ordem: 'Titulo' },
  { rotulo: 'Cliente' },
  { rotulo: 'Fluxo e fase' },
  { rotulo: 'Parado há', ordem: 'FaseDesde', numerica: true },
  { rotulo: 'Valor', ordem: 'ValorEstimado', numerica: true },
  { rotulo: 'Previsão', ordem: 'PrevisaoConclusao', numerica: true },
  { rotulo: 'Situação' },
];

/** A ordem da lista, dita como a tela a chama. */
const ROTULO_DA_ORDEM: Record<OrdemDeProcesso, string> = {
  Numero: 'número',
  Titulo: 'título',
  ValorEstimado: 'valor',
  PrevisaoConclusao: 'previsão de conclusão',
  FaseDesde: 'tempo parado na fase',
  CriadoEm: 'data de criação',
};

const nº = (v: number) => v.toLocaleString('pt-BR');

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

  const fases = useMemo(() => funil.dados?.itens ?? [], [funil.dados]);

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
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Pipeline de Vendas</h1>
          <p className="page-subtitle">
            Os processos do Vórtice dos clientes cadastrados no CRM, por fluxo e fase. As colunas do quadro vêm do dado, e
            não de uma lista escrita na tela.
          </p>
        </div>
        <p className="dash-atualizado">
          {funil.procedencia ? <DadosAtualizadosEm procedencia={funil.procedencia} /> : 'Lendo o pipeline…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              funil.recarregar();
              lista.recarregar();
            }}
            disabled={funil.carregando || lista.carregando}
            data-carregando={funil.carregando || lista.carregando ? 'true' : 'false'}
            aria-label="Reler o pipeline"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES: o fluxo abre o quadro; a busca, a situação e os encerrados filtram a lista. Nenhum
          filtro por fase na lista — ver "O que esta tela ainda não faz". */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="fluxo">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Workflow size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Fluxo do quadro
                <InfoTooltip
                  texto={`Um quadro com os ${fases.length} pares fluxo × fase de uma vez não se lê. O seletor abre um fluxo por vez, e a lista completa vem do dado.`}
                  rotulo="Por que o quadro abre um fluxo por vez"
                />
              </span>
              <select value={fluxoAtivo} onChange={(e) => setFluxoEscolhido(e.target.value)} disabled={fluxos.length === 0}>
                {fluxos.length === 0 && <option value="">sem fluxo com processo</option>}
                {fluxos.map((f) => (
                  <option key={f.codigo} value={f.codigo}>
                    {f.nome} ({f.processos.toLocaleString('pt-BR')})
                  </option>
                ))}
              </select>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="busca">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Search size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Buscar processo</span>
              <input
                type="search"
                aria-label="Buscar processo por título ou número"
                placeholder="Título ou número…"
                value={termoDigitado}
                onChange={(e) => setTermoDigitado(e.target.value)}
              />
            </span>
          </label>

          <label className="dash-filtro" data-bloco="situacao">
            <span className="dash-filtro-icone" aria-hidden="true">
              <ListFilter size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Situação</span>
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
            </span>
          </label>

          <div className="dash-filtros-acao">
            <label className="dash-caixa">
              <input
                type="checkbox"
                checked={consulta.incluirEncerrados}
                onChange={(e) => setConsulta((c) => ({ ...c, incluirEncerrados: e.target.checked, pagina: 1 }))}
              />
              Incluir encerrados
            </label>
          </div>

          {temFiltro && (
            <div className="dash-filtros-acao">
              <button type="button" className="dash-mais-filtros" onClick={limparFiltros}>
                Limpar filtros
              </button>
            </div>
          )}
        </div>
      </div>

      <AvisoDeProcedencia procedencia={funil.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="Processos abertos"
          icone={FolderOpen}
          tom="demanda"
          valor={funil.dados ? nº(abertos) : null}
          carregando={funil.carregando}
          unidade="processos"
          motivoSemDado={funil.carregando ? undefined : 'Sem processo ao alcance deste contexto.'}
          variacao={funil.dados ? 'agrupados por fluxo e fase' : null}
          sobre="Os processos em aberto dos clientes cadastrados no CRM, contados no banco por fluxo e fase, dentro da filial do cabeçalho."
        />
        <CartaoDeDecisao
          rotulo="Fluxos com processo"
          icone={Workflow}
          tom="mercado"
          valor={funil.dados ? nº(fluxos.length) : null}
          carregando={funil.carregando}
          unidade="fluxos"
          motivoSemDado={funil.carregando ? undefined : 'Sem processo ao alcance deste contexto.'}
          variacao={funil.dados ? 'com pelo menos um aberto' : null}
          sobre="Os tipos de processo do Vórtice que têm pelo menos um processo aberto nesta filial. Nenhum é escrito na tela."
        />
        <CartaoDeDecisao
          rotulo="Pares fluxo × fase"
          icone={Columns3}
          tom="neutro"
          valor={funil.dados ? nº(fases.length) : null}
          carregando={funil.carregando}
          unidade="colunas"
          motivoSemDado={funil.carregando ? undefined : 'Sem processo ao alcance deste contexto.'}
          variacao={funil.dados ? 'as colunas que existem no dado' : null}
          sobre="As colunas que existem no dado — cada fase de cada fluxo com processo aberto —, e não uma lista fixa."
        />
        <CartaoDeDecisao
          rotulo="Declaram valor"
          icone={CircleDollarSign}
          tom="oportunidade"
          valor={funil.dados ? nº(comValor) : null}
          carregando={funil.carregando}
          unidade="processos"
          motivoSemDado={funil.carregando ? undefined : 'Sem processo ao alcance deste contexto.'}
          variacao={funil.dados ? `de ${nº(abertos)} abertos` : null}
          sobre="Os processos abertos que informam valor na origem. O resto não tem valor no Vórtice: somar o valor e chamá-lo de valor do funil seria somar o que quase ninguém declarou."
        />
      </div>

      <MetricasSemDado metricas={funil.dados?.metricasSemDado} />

      <section className="dash-secao" data-bloco="secao-quadro">
        <TituloDaSecao
          titulo="O quadro por fase"
          subtitulo="Os processos abertos do fluxo escolhido, fase a fase."
          metodologia="Agrupado no banco, dentro da filial do cabeçalho. A contagem por fase é confiável — ela sai de um agrupamento no banco. O valor de cada fase vem sempre com quantos processos o sustentam."
        />

        <PainelDoMomento
          titulo="Funil por fase"
          data-bloco="funil-por-fase"
          subtitulo={
            fluxos.length > 0
              ? `${fluxos.find((f) => f.codigo === fluxoAtivo)?.nome ?? ''} · ${colunas.length} fases · filial ${contexto.empresa}`
              : `Filial ${contexto.empresa}`
          }
          dica="A fase que aparece com “sem valor declarado” não tem nenhum processo informando valor na origem. Quando alguns declaram, o valor vem com quantos processos o sustentam — sozinho, ele pareceria o valor da fase inteira."
        >
          {funil.carregando && <BlocoCarregando oQue="o funil" />}
          {funil.erro && <BlocoErro erro={funil.erro} aoTentarDeNovo={funil.recarregar} />}

          {funil.dados && fluxos.length === 0 && !funil.erro && (
            <BlocoVazio
              titulo="Nenhum processo aberto nesta filial"
              texto="O funil conta só o que está aberto. Se esta filial não mostra nenhuma coluna, confira a filial escolhida no cabeçalho."
            />
          )}

          {fluxos.length > 0 && (
            <div className="cad-colunas">
              {colunas.map((coluna) => (
                <ColunaDoFunil key={coluna.faseCodigo} coluna={coluna} maior={maiorColuna} />
              ))}
            </div>
          )}
        </PainelDoMomento>
      </section>

      <section className="dash-secao" data-bloco="secao-processos">
        <TituloDaSecao
          titulo="Os processos"
          subtitulo={lista.recarregando ? 'Atualizando…' : `${(pagina?.total ?? 0).toLocaleString('pt-BR')} no total, com os filtros da barra.`}
          metodologia="A lista filtra só pelo que o banco resolve — busca, situação e encerrados. A busca compara o título por trecho e o número por valor inteiro."
        />

        <PainelDoMomento
          titulo="Processos desta filial"
          data-bloco="processos"
          subtitulo={`Ordenados por ${ROTULO_DA_ORDEM[consulta.ordenarPor] ?? consulta.ordenarPor}${consulta.descendente ? ', do maior para o menor' : ''}.`}
          dica="Clique no título de uma coluna com seta para ordenar por ela. O processo abre a ficha da oportunidade; o cliente, o 360 dele."
        >
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
              <div className="mom-tabela-rolagem cad-so-largo">
                <table className="mom-tabela">
                  <caption className="cad-so-leitor">
                    Processos da filial {contexto.empresa}, ordenados por {consulta.ordenarPor}
                  </caption>
                  <thead>
                    <tr>
                      {COLUNAS.map((coluna) => (
                        <th
                          key={coluna.rotulo}
                          scope="col"
                          className={coluna.numerica ? 'mom-num' : undefined}
                          aria-sort={ariaOrdem(coluna.ordem, consulta)}
                        >
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
                        <th scope="row">
                          <Link to={`/oportunidades/${p.chave}`} className="cad-link-forte">
                            {p.titulo}
                          </Link>
                          <div className="cad-sub">
                            nº {p.numero}
                            {p.proprietarioNome ? ` · ${p.proprietarioNome}` : ''}
                          </div>
                        </th>
                        <td>
                          <Link to={`/clientes/${p.clienteChave}`} className="cad-link-forte">
                            {p.clienteNome}
                          </Link>
                        </td>
                        <td>
                          <div>{p.faseNome}</div>
                          <div className="cad-sub">{p.tipoProcessoNome}</div>
                        </td>
                        <td className="mom-num">
                          <span className={p.diasNaFase > 90 ? 'cad-alerta' : undefined}>
                            {p.diasNaFase.toLocaleString('pt-BR')} {p.diasNaFase === 1 ? 'dia' : 'dias'}
                          </span>
                          <div className="cad-sub">desde {formatarData(p.faseDesde)}</div>
                        </td>
                        <td className="mom-num">
                          {p.valorEstimado === null ? (
                            <span className="cad-nada">não declarado</span>
                          ) : (
                            formatarDinheiro(p.valorEstimado)
                          )}
                        </td>
                        <td className="mom-num">
                          {p.previsaoConclusao ? formatarData(p.previsaoConclusao) : <span className="cad-nada">sem previsão</span>}
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
        </PainelDoMomento>
      </section>

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
    </PaginaDoPainel>
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
             O aviso completo é dado uma vez na dica do painel; aqui fica o
             travessão de valor ausente, com o motivo na dica que abre pelo teclado
             (issue 167: nenhum `title=`). */
          <div className="cad-coluna-meta cad-nada">
            <ValorAusente
              motivo={`Nenhum dos ${coluna.processos.toLocaleString('pt-BR')} processos desta fase informa valor na origem.`}
              oQue={`o valor da fase ${coluna.faseNome}`}
            />{' '}
            sem valor declarado
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
