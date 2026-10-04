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
 * 2. **A contagem por fase é confiável — e, desde 30/09/2026, o filtro por fase
 *    também.** A listagem `/api/v1/processos` aplicava `faseCodigo` e
 *    `tipoProcessoCodigo` depois de o banco paginar (a dívida P-7: a página
 *    vinha pela metade e o total era o da lista inteira), e a tela não oferecia
 *    esse filtro. Os dois códigos passaram a ir ao banco, e a seta de cada fase
 *    do quadro abre a lista daquela fase, com o total dela.
 *
 * ---------------------------------------------------------------------------
 * 30/09/2026 — A MAQUETE DO RICARDO (`docs/prototipo/capturas-referencia/pipeline-maquete-2026-09-30.png`):
 *
 * - o título "Pipeline" com o ícone, e o que o subtítulo antigo dizia foi para o ⓘ ao lado dele;
 * - os filtros sem o ladrilho de ícone, e a lupa dentro da busca;
 * - o aviso "O que estes números não dizem" é o mesmo motivo medido pela API, na caixa laranja;
 * - cada fase num cartão, com a cor da maquete PELO NOME DA FASE (decisão do Ricardo: a cor só identifica a fase, e a
 *   fase que não está na imagem fica cinza). O fundo pinta só a fase que tem valor declarado, e o número da maior fase
 *   vem em verde;
 * - a lista com o Exportar, a pílula dos dias parados e o menu de cada linha;
 * - os mini-gráficos de tendência dos cartões ficaram de fora: o CRM guarda a fase e o valor de hoje de cada processo, e
 *   não uma série dia a dia. A lacuna está escrita no fim da tela.
 */

import {
  ArrowDown,
  ArrowUp,
  ChartColumnIncreasing,
  ChevronRight,
  CircleDollarSign,
  ClipboardList,
  Columns3,
  Download,
  FileText,
  FolderOpen,
  RefreshCw,
  Search,
  Workflow,
  X,
} from 'lucide-react';
import { useEffect, useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { MenuDaLinha } from '../componentes/comum/MenuDaLinha';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import {
  LINHAS_NO_EXPORTAR,
  listarProcessos,
  listarProcessosInteira,
  obterFunil,
  PROCESSOS_INICIAL,
} from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import {
  SITUACOES_DE_PROCESSO,
  type ConsultaDeProcessos,
  type FaseDoFunil,
  type OrdemDeProcesso,
  type ProcessoResumo,
} from '../tipos/relacionamento';
import { formatarData, formatarDinheiro } from './cadastro/formato';
import { formatarNumero as nº } from '../dados/formatadores';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/territorio.css';
import '../estilos/pipeline.css';

/** As colunas da lista e quais delas a API sabe ordenar. */
const COLUNAS: { rotulo: string; ordem?: OrdemDeProcesso }[] = [
  { rotulo: 'Processo', ordem: 'Titulo' },
  { rotulo: 'Cliente' },
  { rotulo: 'Fluxo e fase' },
  { rotulo: 'Parado há', ordem: 'FaseDesde' },
  { rotulo: 'Valor', ordem: 'ValorEstimado' },
  { rotulo: 'Previsão', ordem: 'PrevisaoConclusao' },
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

/** Espera antes de mandar a busca à API, para não consultar a cada tecla. */
const ESPERA_DA_BUSCA_MS = 350;

/** A partir de quantos dias na mesma fase o tempo parado vira alerta. */
const DIAS_PARADO_EM_ALERTA = 90;

type CorDaFase = 'verde' | 'azul' | 'laranja' | 'roxo' | 'cinza';

/**
 * A COR DE CADA FASE É A DA MAQUETE, PELO NOME (decisão do Ricardo em 30/09/2026). O banco não guarda cor de fase, e a
 * cor aqui só ajuda o olho a achar a fase — não mede nada. A fase que não está na imagem fica cinza.
 */
const COR_DA_FASE: Record<string, CorDaFase> = {
  'nao iniciado': 'cinza',
  apresentacao: 'verde',
  monitoramento: 'azul',
  finalizado: 'cinza',
  negociacao: 'laranja',
  'pedido de venda': 'verde',
  montagem: 'azul',
  analise: 'roxo',
  aprovacao: 'verde',
  formalizacao: 'roxo',
  autorizacao: 'azul',
  faturamento: 'verde',
  recebimento: 'laranja',
  preparacao: 'azul',
  entrega: 'verde',
};

/** O nome sem acento, sem caixa e sem espaço sobrando — "Pedido de Venda" e "PEDIDO DE VENDA" são a mesma fase. */
const chaveDoNome = (nome: string) =>
  nome
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .trim()
    .toLowerCase()
    .replace(/\s+/g, ' ');

const corDaFase = (nome: string): CorDaFase => COR_DA_FASE[chaveDoNome(nome)] ?? 'cinza';

export function Pipeline() {
  const { contexto } = useContextoDeAcesso();

  const funil = useRecurso(
    (sinal) => obterFunil(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const [consulta, setConsulta] = useState<ConsultaDeProcessos>(PROCESSOS_INICIAL);
  const [termoDigitado, setTermoDigitado] = useState('');
  const [fluxoEscolhido, setFluxoEscolhido] = useState<string | null>(null);
  const secaoDaLista = useRef<HTMLElement>(null);

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
  const nomeDoFluxoAtivo = fluxos.find((f) => f.codigo === fluxoAtivo)?.nome ?? '';

  /** As colunas do fluxo ativo, na ordem que a fase declara. */
  const colunas = useMemo(
    () =>
      fases
        .filter((f) => f.tipoProcessoCodigo === fluxoAtivo)
        .sort((a, b) => a.faseOrdem - b.faseOrdem || a.faseNome.localeCompare(b.faseNome)),
    [fases, fluxoAtivo],
  );

  const maiorColuna = Math.max(0, ...colunas.map((c) => c.processos));

  const abertos = fases.reduce((s, f) => s + f.processos, 0);
  const comValor = fases.reduce((s, f) => s + f.processosComValor, 0);

  /** A fase que filtra a lista, quando a seta de uma fase foi clicada. */
  const faseDaLista = useMemo(
    () =>
      consulta.faseCodigo === ''
        ? null
        : fases.find((f) => f.faseCodigo === consulta.faseCodigo && f.tipoProcessoCodigo === consulta.tipoProcessoCodigo) ??
          null,
    [fases, consulta.faseCodigo, consulta.tipoProcessoCodigo],
  );

  const temFiltro = useMemo(
    () => consulta.termo !== '' || consulta.situacao !== '' || consulta.incluirEncerrados || consulta.faseCodigo !== '',
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

  /** A SETA DE UMA FASE abre a lista daquela fase — e a mesma seta de novo tira o filtro. */
  function escolherFase(coluna: FaseDoFunil) {
    const mesma = consulta.faseCodigo === coluna.faseCodigo && consulta.tipoProcessoCodigo === coluna.tipoProcessoCodigo;
    setConsulta((c) => ({
      ...c,
      faseCodigo: mesma ? '' : coluna.faseCodigo,
      tipoProcessoCodigo: mesma ? '' : coluna.tipoProcessoCodigo,
      pagina: 1,
    }));
    if (!mesma) secaoDaLista.current?.scrollIntoView?.({ behavior: 'smooth', block: 'start' });
  }

  function tirarFase() {
    setConsulta((c) => ({ ...c, faseCodigo: '', tipoProcessoCodigo: '', pagina: 1 }));
  }

  const pagina = lista.dados;
  const semProcesso = funil.carregando ? undefined : 'Sem processo ao alcance deste contexto.';

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga pip-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div className="pip-cabecalho">
          <span className="pip-titulo-icone" aria-hidden="true">
            <ChartColumnIncreasing size={22} strokeWidth={2.4} />
          </span>
          <div>
            <h1 className="page-title">
              Pipeline
              <InfoTooltip
                rotulo="De onde vêm os processos do Pipeline"
                texto="Os processos do Vórtice dos clientes cadastrados no CRM, por fluxo e fase. As colunas do quadro vêm do dado, e não de uma lista escrita na tela."
              />
            </h1>
            <p className="page-subtitle">Acompanhe os processos comerciais agrupados por fluxo e fase.</p>
          </div>
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

      {/* A BARRA DOS INDICADORES: o fluxo abre o quadro; a busca, a situação e os encerrados filtram a lista. A fase
          filtra a lista pela seta de cada fase do quadro. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="fluxo">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Fluxo do quadro
                <InfoTooltip
                  texto={`Um quadro com os ${fases.length} pares fluxo × fase de uma vez não se lê. O seletor abre um fluxo por vez, e a lista completa vem do dado.`}
                  rotulo="Por que o quadro abre um fluxo por vez"
                />
              </span>
              <select
                value={fluxoAtivo}
                onChange={(e) => {
                  setFluxoEscolhido(e.target.value);
                  // A FASE ESCOLHIDA É DE UM FLUXO: trocar o quadro de fluxo tira o filtro dela da lista.
                  tirarFase();
                }}
                disabled={fluxos.length === 0}
              >
                {fluxos.length === 0 && <option value="">sem fluxo com processo</option>}
                {fluxos.map((f) => (
                  <option key={f.codigo} value={f.codigo}>
                    {f.nome} ({nº(f.processos)})
                  </option>
                ))}
              </select>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="busca">
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Buscar processo</span>
              <span className="pip-busca">
                <Search size={15} strokeWidth={2} aria-hidden="true" />
                <input
                  type="search"
                  aria-label="Buscar processo por título ou número"
                  placeholder="Título ou número…"
                  value={termoDigitado}
                  onChange={(e) => setTermoDigitado(e.target.value)}
                />
              </span>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="situacao">
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
          motivoSemDado={semProcesso}
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
          motivoSemDado={semProcesso}
          variacao={funil.dados ? 'com pelo menos um aberto' : null}
          sobre="Os tipos de processo do Vórtice que têm pelo menos um processo aberto nesta filial. Nenhum é escrito na tela."
        />
        <CartaoDeDecisao
          rotulo="Pares fluxo × fase"
          icone={Columns3}
          tom="captura"
          valor={funil.dados ? nº(fases.length) : null}
          carregando={funil.carregando}
          unidade="colunas"
          motivoSemDado={semProcesso}
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
          motivoSemDado={semProcesso}
          variacao={funil.dados ? `de ${nº(abertos)} abertos` : null}
          sobre="Os processos abertos que informam valor na origem. O resto não tem valor no Vórtice: somar o valor e chamá-lo de valor do funil seria somar o que quase ninguém declarou."
        />
      </div>

      {/* O QUE ESTES NÚMEROS NÃO DIZEM — o motivo medido pela API na mesma leitura, na caixa laranja da maquete. */}
      <MetricasSemDado metricas={funil.dados?.metricasSemDado} />

      <section className="dash-secao pip-cartao" data-bloco="secao-quadro">
        <TituloDaSecao
          icone={<FileText className="pip-secao-icone" size={22} strokeWidth={2.2} aria-hidden="true" />}
          titulo="O quadro por fase"
          subtitulo="Os processos abertos do fluxo escolhido, fase a fase."
          metodologia="Agrupado no banco, dentro da filial do cabeçalho. A contagem por fase é confiável — ela sai de um agrupamento no banco. O valor de cada fase vem sempre com quantos processos o sustentam: a fase que aparece com “sem valor declarado” não tem nenhum processo informando valor na origem. A cor só identifica a fase, como na maquete; o fundo pinta a fase que tem valor declarado, e o número verde é o da maior fase. A seta abre a lista daquela fase."
          acao={
            fluxos.length > 0 ? (
              <p className="pip-secao-acao pip-quadro-resumo">
                {nomeDoFluxoAtivo} • {nº(fluxos.length)} {fluxos.length === 1 ? 'fluxo' : 'fluxos'} • {nº(colunas.length)}{' '}
                {colunas.length === 1 ? 'fase' : 'fases'} • filial {contexto.empresa}
              </p>
            ) : undefined
          }
        />

        {/* O PAINEL NÃO TEM TÍTULO À VISTA NA MAQUETE: o título dele fica só para o leitor de tela. */}
        <PainelDoMomento titulo="Funil por fase" data-bloco="funil-por-fase">
          {funil.carregando && <BlocoCarregando oQue="o funil" />}
          {funil.erro && <BlocoErro erro={funil.erro} aoTentarDeNovo={funil.recarregar} />}

          {funil.dados && fluxos.length === 0 && !funil.erro && (
            <BlocoVazio
              titulo="Nenhum processo aberto nesta filial"
              texto="O funil conta só o que está aberto. Se esta filial não mostra nenhuma coluna, confira a filial escolhida no cabeçalho."
            />
          )}

          {fluxos.length > 0 && (
            <ul className="pip-fases">
              {colunas.map((coluna) => (
                <CartaoDaFase
                  key={coluna.faseCodigo}
                  coluna={coluna}
                  ehAMaior={coluna.processos === maiorColuna}
                  escolhida={
                    consulta.faseCodigo === coluna.faseCodigo && consulta.tipoProcessoCodigo === coluna.tipoProcessoCodigo
                  }
                  aoEscolher={() => escolherFase(coluna)}
                />
              ))}
            </ul>
          )}
        </PainelDoMomento>
      </section>

      <section className="dash-secao pip-cartao" data-bloco="secao-processos" ref={secaoDaLista}>
        <TituloDaSecao
          icone={<ClipboardList className="pip-secao-icone" size={22} strokeWidth={2.2} aria-hidden="true" />}
          titulo="Os processos"
          subtitulo={
            lista.recarregando
              ? 'Atualizando…'
              : `${nº(pagina?.total ?? 0)} no total, com os filtros da barra${faseDaLista ? ` e a fase ${faseDaLista.faseNome}` : ''}.`
          }
          metodologia="A lista filtra no banco pela busca, pela situação, pelos encerrados e pela fase escolhida no quadro. A busca compara o título por trecho e o número por valor inteiro. Clique no título de uma coluna com seta para ordenar por ela. O processo abre a ficha da oportunidade; o cliente, o 360 dele."
          acao={
            <div className="pip-secao-acao pip-lista-acoes">
              {faseDaLista && (
                <span className="pip-fase-filtro">
                  Fase <strong>{faseDaLista.faseNome}</strong>
                  <button type="button" onClick={tirarFase} aria-label={`Tirar o filtro da fase ${faseDaLista.faseNome}`}>
                    <X size={13} strokeWidth={2.6} aria-hidden="true" />
                  </button>
                </span>
              )}
              <BotaoExportar consulta={consulta} total={pagina?.total ?? 0} />
            </div>
          }
        />

        {/* O PAINEL NÃO TEM TÍTULO À VISTA NA MAQUETE: o título e a ordem ficam para o leitor de tela. */}
        <PainelDoMomento
          titulo="Processos desta filial"
          data-bloco="processos"
          subtitulo={`Ordenados por ${ROTULO_DA_ORDEM[consulta.ordenarPor] ?? consulta.ordenarPor}${consulta.descendente ? ', do maior para o menor' : ''}.`}
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
                <table className="mom-tabela pip-tabela">
                  <caption className="cad-so-leitor">
                    Processos da filial {contexto.empresa}, ordenados por {ROTULO_DA_ORDEM[consulta.ordenarPor] ?? consulta.ordenarPor}
                  </caption>
                  <thead>
                    <tr>
                      {COLUNAS.map((coluna) => (
                        <th key={coluna.rotulo} scope="col" aria-sort={ariaOrdem(coluna.ordem, consulta)}>
                          {coluna.ordem ? (
                            <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem!)}>
                              {coluna.rotulo}
                              <SetaDaOrdem campo={coluna.ordem} consulta={consulta} />
                            </button>
                          ) : (
                            coluna.rotulo
                          )}
                        </th>
                      ))}
                      <th scope="col" className="pip-coluna-menu">
                        <span className="cad-so-leitor">Ações</span>
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {pagina.itens.map((p) => (
                      <tr key={p.chave}>
                        <th scope="row">
                          <Link to={`/oportunidades/${p.chave}`} className="pip-processo">
                            {p.titulo}
                          </Link>
                          <div className="cad-sub">
                            # {p.numero}
                            {p.proprietarioNome ? ` • ${p.proprietarioNome}` : ''}
                          </div>
                        </th>
                        <td>
                          <Link to={`/clientes/${p.clienteChave}`} className="pip-cliente">
                            {p.clienteNome}
                          </Link>
                        </td>
                        <td>
                          <div className="pip-fase-da-linha">{p.faseNome}</div>
                          <div className="cad-sub">{p.tipoProcessoNome}</div>
                        </td>
                        <td>
                          <DiasParado dias={p.diasNaFase} />
                          <div className="cad-sub">desde {formatarData(p.faseDesde)}</div>
                        </td>
                        <td>
                          {p.valorEstimado === null ? (
                            <span className="cad-nada">não declarado</span>
                          ) : (
                            <span className="pip-valor">{formatarDinheiro(p.valorEstimado)}</span>
                          )}
                        </td>
                        <td>
                          {p.previsaoConclusao ? formatarData(p.previsaoConclusao) : <span className="cad-nada">sem previsão</span>}
                        </td>
                        <td>
                          <span className={`pip-situacao cad-selo cad-selo-${p.situacao.toLowerCase()}`}>{p.situacao}</span>
                        </td>
                        <td className="pip-coluna-menu">
                          <MenuDaLinha
                            horizontal
                            rotulo={`Ações do processo ${p.numero}`}
                            itens={[
                              { rotulo: 'Abrir o processo', para: `/oportunidades/${p.chave}` },
                              { rotulo: 'Abrir a ficha do cliente', para: `/clientes/${p.clienteChave}` },
                            ]}
                          />
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

      {/* O FILTRO POR FASE FOI PARA A DICA DA LISTA (29/09/2026, #31) e virou a seta de cada fase (30/09/2026), com a
          dívida P-7 paga na API. */}
      <BlocoRecolhivel titulo="O que esta tela ainda não faz, e por quê" resumo="mudança de fase pelo quadro · tendência dos cartões">
        <div className="cad-fichas">
          <p className="cad-estado-texto">
            {/* A citação "(dívida D-9 do documento 23)" saiu da tela pelo mesmo motivo das outras. */}
            <strong>Mudar de fase, fechar e marcar como perdida ainda não estão aqui.</strong> Mudar
            de fase dispara automações — concluir uma tarefa gera a próxima —, e elas entram junto
            com a tela que as controla.
          </p>
          <LacunaConhecida
            metrica="A tendência de cada cartão"
            motivo="A maquete desenha um mini-gráfico em cada cartão. O CRM guarda a fase e o valor de hoje de cada processo, e não uma série dia a dia: a linha seria desenho, e não dado. Os cartões mostram o retrato de hoje."
          />
        </div>
      </BlocoRecolhivel>
    </PaginaDoPainel>
  );
}

/**
 * UMA FASE DO QUADRO, no cartão da maquete: a bolinha da cor, o nome, a seta que abre a lista da fase, o número e o
 * valor — sempre com quantos processos o sustentam.
 *
 * A SETA É O BOTÃO, e a área dele cobre o cartão inteiro pelo CSS: o travessão do "sem valor" tem a dica dele, e um
 * botão não pode morar dentro de outro.
 */
function CartaoDaFase({
  coluna,
  ehAMaior,
  escolhida,
  aoEscolher,
}: {
  coluna: FaseDoFunil;
  ehAMaior: boolean;
  escolhida: boolean;
  aoEscolher: () => void;
}) {
  return (
    <li
      className="pip-fase"
      data-cor={corDaFase(coluna.faseNome)}
      data-com-valor={coluna.valorTotal !== null ? 'true' : 'false'}
      data-escolhida={escolhida ? 'true' : 'false'}
    >
      <div className="pip-fase-cabeca">
        <i className="pip-fase-ponto" aria-hidden="true" />
        <span className="pip-fase-nome">{coluna.faseNome}</span>
        <button
          type="button"
          className="pip-fase-seta"
          aria-pressed={escolhida}
          aria-label={
            escolhida
              ? `Tirar o filtro da fase ${coluna.faseNome} da lista`
              : `Ver na lista os ${nº(coluna.processos)} processos da fase ${coluna.faseNome}`
          }
          onClick={aoEscolher}
        >
          <ChevronRight size={16} strokeWidth={2.2} aria-hidden="true" />
        </button>
      </div>
      <strong className="pip-fase-numero" data-maior={ehAMaior ? 'true' : 'false'}>
        {nº(coluna.processos)}
      </strong>
      {/* O VALOR VEM SEMPRE COM QUANTOS PROCESSOS O SUSTENTAM. Sozinho, ele pareceria o valor da fase inteira — e
          representa uma fração dela. */}
      {coluna.valorTotal === null ? (
        /* O TRAVESSÃO, A FRASE E O ⓘ NESSA ORDEM, como a maquete escreve "— sem valor declarado": o ⓘ no fim não
           empurra a frase para a segunda linha. */
        <div className="pip-fase-meta cad-nada">
          <span aria-hidden="true">—</span> sem valor declarado
          <InfoTooltip
            texto={`Nenhum dos ${nº(coluna.processos)} processos desta fase informa valor na origem.`}
            rotulo={`Por que o valor da fase ${coluna.faseNome} não aparece`}
          />
        </div>
      ) : (
        <div className="pip-fase-meta">
          <strong className="pip-fase-valor">{formatarDinheiro(coluna.valorTotal)}</strong>
          somados de {nº(coluna.processosComValor)} de {nº(coluna.processos)} processos — o resto não declara valor.
        </div>
      )}
    </li>
  );
}

/** OS DIAS NA MESMA FASE — a pílula vermelha da maquete a partir de {@link DIAS_PARADO_EM_ALERTA}. */
function DiasParado({ dias }: { dias: number }) {
  const texto = `${nº(dias)} ${dias === 1 ? 'dia' : 'dias'}`;
  return dias > DIAS_PARADO_EM_ALERTA ? (
    <span className="pip-parado" data-alerta="true">
      {texto}
      <span className="cad-so-leitor"> — parado há mais de {DIAS_PARADO_EM_ALERTA} dias</span>
    </span>
  ) : (
    <span className="pip-parado">{texto}</span>
  );
}

/**
 * O EXPORTAR DA MAQUETE — a lista inteira, com a busca, a situação, os encerrados e a fase da tela, em CSV. Vai ao
 * servidor de 200 em 200, até o total ou até {@link LINHAS_NO_EXPORTAR}; passando disso, o ⓘ diz que leva os primeiros.
 */
function BotaoExportar({ consulta, total }: { consulta: ConsultaDeProcessos; total: number }) {
  const { contexto } = useContextoDeAcesso();
  const [exportando, setExportando] = useState(false);
  const [falha, setFalha] = useState<string | null>(null);

  async function exportar() {
    setExportando(true);
    setFalha(null);
    try {
      const { itens } = await listarProcessosInteira(contexto, consulta);
      baixarCsv(`pipeline-${contexto.empresa}-${carimboDeData()}`, CABECALHO_DO_CSV, itens.map(linhaDoCsv));
    } catch (causa) {
      setFalha(causa instanceof Error ? causa.message : 'A exportação não terminou.');
    } finally {
      setExportando(false);
    }
  }

  return (
    <span className="pip-exportar-caixa">
      <button type="button" className="pip-exportar" onClick={exportar} disabled={exportando || total === 0}>
        <Download size={16} strokeWidth={2.2} aria-hidden="true" />
        {exportando ? 'Exportando…' : 'Exportar'}
      </button>
      {total > LINHAS_NO_EXPORTAR && (
        <InfoTooltip
          rotulo="Quantas linhas o Exportar leva"
          texto={`A lista tem ${nº(total)} processos; o Exportar leva os primeiros ${nº(LINHAS_NO_EXPORTAR)}, na ordem da tela. Um filtro, a busca ou a fase diminuem a lista.`}
        />
      )}
      {falha && (
        <span className="cad-alerta" role="alert">
          {falha}
        </span>
      )}
    </span>
  );
}

const CABECALHO_DO_CSV = [
  'Número',
  'Processo',
  'Cliente',
  'Fluxo',
  'Fase',
  'Na fase desde',
  'Dias parado',
  'Valor declarado',
  'Previsão',
  'Situação',
  'Responsável',
];

function linhaDoCsv(p: ProcessoResumo): unknown[] {
  return [
    p.numero,
    p.titulo,
    p.clienteNome,
    p.tipoProcessoNome,
    p.faseNome,
    formatarData(p.faseDesde),
    p.diasNaFase,
    p.valorEstimado ?? 'não declarado',
    p.previsaoConclusao ? formatarData(p.previsaoConclusao) : 'sem previsão',
    p.situacao,
    p.proprietarioNome ?? '',
  ];
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
          <DiasParado dias={processo.diasNaFase} />
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
          <span className={`pip-situacao cad-selo cad-selo-${processo.situacao.toLowerCase()}`}>{processo.situacao}</span>
        </span>
      </div>
    </div>
  );
}

function ariaOrdem(campo: OrdemDeProcesso | undefined, consulta: ConsultaDeProcessos) {
  if (!campo || consulta.ordenarPor !== campo) return undefined;
  return consulta.descendente ? ('descending' as const) : ('ascending' as const);
}

/** A seta da coluna ordenada, como na maquete — para cima na ordem crescente. */
function SetaDaOrdem({ campo, consulta }: { campo: OrdemDeProcesso; consulta: ConsultaDeProcessos }) {
  if (consulta.ordenarPor !== campo) return null;
  const Seta = consulta.descendente ? ArrowDown : ArrowUp;
  return <Seta className="pip-seta-da-ordem" size={13} strokeWidth={2.4} aria-hidden="true" />;
}
