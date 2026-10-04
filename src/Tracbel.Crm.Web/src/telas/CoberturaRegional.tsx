/**
 * Cobertura por Filial e Carteira — o agrupamento territorial que existe no dado.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — esta tela mostrava sete regionais que não existem.
 *
 * O protótipo agrupava a cobertura em **MT Norte, GO, BA Oeste** e mais quatro.
 * A premissa estava errada, e quem a corrigiu foi o gerente comercial:
 *
 * > *"Se você olhar no CRM da Vórtice temos as carteiras dos CENs, e nas
 * > carteiras temos a filial que ele vai atuar e quais cidades ele terá na
 * > carteira."*
 *
 * Verificado ao vivo, e o dado é inequívoco: **`IVS_Regional` existe no esquema
 * do Vórtice e tem ZERO linhas.** Não é que esteja pouco usada — nunca recebeu
 * uma linha. Não há tabela, coluna nem valor de texto que sustente aquelas sete
 * regionais, e **as treze filiais em operação estão todas no interior de São
 * Paulo** (documento 26, §1).
 *
 * O que existe preenchido é o par **filial → carteira → municípios**:
 * `IVS_Carteira.NroEmpresa` está preenchida em 655 de 655 carteiras, e
 * `IVS_CartCid` liga carteira e cidade. A tela passa a mostrar isso, em dois
 * níveis, com as rotas `/api/v1/cobertura/filiais` e `/carteiras`.
 *
 * ---------------------------------------------------------------------------
 * A LACUNA É DECLARADA, NÃO ESCONDIDA. Nem toda carteira carregada declara
 * cidade. **A carteira sem cidade aparece com a lista vazia em vez de
 * sumir**: escondê-la faria a tela mostrar uma operação menor do que ela é.
 * E o número vem da API, em `metricasSemDado`.
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS (#293, bloco 2).
 *
 * 02/10/2026 — NO DESENHO DA MAQUETE DO RICARDO (`docs/prototipo/capturas-referencia/cobertura-filial-maquete-2026-10-02.png`),
 * com a regra dele: "não tire nada que temos, só melhore o visual". O que entrou:
 *   1. OS SEIS FILTROS DA MAQUETE, todos com dado de verdade: o período do contato (30 ou 90 dias — a rota traz os
 *      dois), a filial, a carteira, o município e o estado (recortes das carteiras ao alcance, cruzadas com a cobertura
 *      de contato pela chave da carteira) e o tipo de cliente (a classe da curva ABC, que a rota da cobertura já aceita);
 *   2. O MEDIDOR com o arco e o ponteiro da maquete, e embaixo dele os vínculos com contato no período, sem contato no
 *      período e nunca contatados — as três partes do mesmo total;
 *   3. AS BARRAS com "Ordenar por", e no nível 2 a cobertura de cada carteira ao lado das cidades;
 *   4. OS DOIS NÍVEIS recolhíveis, com "Expandir todos"; o clique na filial do nível 1 recorta o nível 2.
 * O que a tela já tinha continua: os cinco números, os avisos de lacuna (agora no desenho do aviso da maquete), o
 * filtro "só as carteiras com cidade" (no nível 2, que é o que ele recorta) e a correção das regionais, no fim.
 * A maquete marca uma "Meta 80%" no medidor; ela não entra, porque nenhuma meta de cobertura foi cadastrada.
 */

import {
  BarChart3,
  Building2,
  BriefcaseBusiness,
  CalendarDays,
  ChevronDown,
  ChevronRight,
  ChevronUp,
  CircleAlert,
  CircleCheck,
  Clock3,
  Expand,
  MapPin,
  MapPinned,
  RefreshCw,
  Shrink,
  UserRound,
  Users,
  X,
  type LucideIcon,
} from 'lucide-react';
import { useMemo, useState, type ReactNode } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { ListaDeBarras } from '../componentes/ListaDeBarras';
import { GraficoGauge } from '../componentes/GraficoGauge';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import {
  listarTerritorioPorCarteira,
  obterCoberturaPorFilial,
  obterResumoDeCobertura,
} from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type { CoberturaDeFilial, ResumoDeCobertura, TerritorioDeCarteira } from '../tipos/relacionamento';
import { formatarNumero as nº } from '../dados/formatadores';
import { CampoDoFiltro } from '../componentes/comum/CampoDoFiltro';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';
import '../estilos/cobertura-filial.css';

/**
 * As faixas do medidor, no desenho da maquete de 02/10/2026: do vermelho ao verde, em seis cores.
 *
 * Elas não são meta: são faixas de leitura, e a dica do painel diz isso. A meta da empresa não existe em lugar
 * nenhum — `organizacao.Meta` está vazia —, e por isso o medidor não tem ponteiro de alvo nem cor de aprovação. (Até
 * 02/10 eram as quatro faixas do protótipo, 40/70/80/100; a maquete repartiu o arco em mais cores.)
 */
const FAIXAS_DO_MEDIDOR = [
  { max: 25, cor: '#E5483B' },
  { max: 40, cor: '#F0702E' },
  { max: 50, cor: '#F59E0B' },
  { max: 62, cor: '#FACC15' },
  { max: 75, cor: '#8BC34A' },
  { max: 100, cor: '#1E9E4A' },
];

/** Quantas barras cabem sem virar sopa de letrinhas. O original tinha sete. */
const BARRAS_NO_GRAFICO = 8;

type Periodo = 30 | 90;
type Ordem = 'maior-cobertura' | 'menor-cobertura' | 'maior-carteira' | 'nome';

const ORDENS: { valor: Ordem; rotulo: string }[] = [
  { valor: 'maior-cobertura', rotulo: 'Maior cobertura' },
  { valor: 'menor-cobertura', rotulo: 'Menor cobertura' },
  { valor: 'maior-carteira', rotulo: 'Maior carteira' },
  { valor: 'nome', rotulo: 'Nome' },
];

/** Os vínculos com contato no período escolhido — a rota traz os dois. */
const comContato = (c: ResumoDeCobertura, periodo: Periodo) => (periodo === 30 ? c.comContatoEm30Dias : c.comContatoEm90Dias);

export function CoberturaRegional() {
  const { contexto } = useContextoDeAcesso();

  const [periodo, setPeriodo] = useState<Periodo>(90);
  const [filial, setFilial] = useState('');
  const [carteira, setCarteira] = useState('');
  const [classe, setClasse] = useState('');
  const [municipio, setMunicipio] = useState('');
  const [uf, setUf] = useState('');
  const [ordem, setOrdem] = useState<Ordem>('maior-cobertura');
  const [nivel1Aberto, setNivel1Aberto] = useState(true);
  const [nivel2Aberto, setNivel2Aberto] = useState(true);
  const [avisoFechado, setAvisoFechado] = useState(false);

  const filiais = useRecurso(
    (sinal) => obterCoberturaPorFilial(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const carteiras = useRecurso(
    // Sem `empresaCodigo`: a rota já devolve só o que o contexto de acesso
    // alcança, porque a consulta parte de `organizacao.Carteira`, que carrega o
    // filtro global. Um parâmetro de empresa aqui seria uma segunda fronteira,
    // que pode divergir da primeira. O filtro "Filial" da tela recorta DENTRO desse alcance.
    (sinal) => listarTerritorioPorCarteira(contexto, '', sinal),
    [contexto.empresa, contexto.usuario],
  );

  const cobertura = useRecurso(
    // O TIPO DE CLIENTE é a classe da curva ABC: a rota conta só os clientes daquela classe (sem classe apurada é D).
    (sinal) => obterResumoDeCobertura(contexto, sinal, classe || undefined),
    [contexto.empresa, contexto.usuario, classe],
  );

  const [somenteComCidade, setSomenteComCidade] = useState(false);
  const [abertas, setAbertas] = useState<Set<string>>(() => new Set());

  const listaDeFiliais = useMemo(() => filiais.dados?.itens ?? [], [filiais.dados]);
  const listaDeCarteiras = useMemo(() => carteiras.dados?.itens ?? [], [carteiras.dados]);
  const carteirasComCobertura = useMemo(() => cobertura.dados?.itens ?? [], [cobertura.dados]);

  /* -------------------------------------------------------------------------------------------------------------- */
  /* O RECORTE DOS FILTROS. Filial, carteira, município e estado recortam as carteiras do território; a cobertura de  */
  /* contato segue o mesmo recorte pela chave da carteira, que é a mesma nas duas rotas (`organizacao.Carteira`).     */
  /* -------------------------------------------------------------------------------------------------------------- */
  const recorteTerritorial = filial !== '' || carteira !== '' || municipio !== '' || uf !== '';

  const carteirasDoRecorte = useMemo(
    () =>
      listaDeCarteiras.filter(
        (c) =>
          (!filial || c.empresaCodigo === filial) &&
          (!carteira || c.carteiraChave === carteira) &&
          (!municipio || c.municipios.some((m) => String(m.id) === municipio)) &&
          (!uf || c.municipios.some((m) => m.uf === uf)),
      ),
    [listaDeCarteiras, filial, carteira, municipio, uf],
  );

  const coberturaDoRecorte = useMemo(() => {
    if (!recorteTerritorial) return carteirasComCobertura;
    const chaves = new Set(carteirasDoRecorte.map((c) => c.carteiraChave));
    return carteirasComCobertura.filter((c) => chaves.has(c.carteiraChave));
  }, [recorteTerritorial, carteirasComCobertura, carteirasDoRecorte]);

  const coberturaPorChave = useMemo(() => new Map(carteirasComCobertura.map((c) => [c.carteiraChave, c])), [carteirasComCobertura]);

  // AS OPÇÕES DOS FILTROS saem do que as rotas trouxeram: a carteira, o município e o estado da filial escolhida.
  const opcoes = useMemo(() => {
    const daFilial = listaDeCarteiras.filter((c) => !filial || c.empresaCodigo === filial);
    const municipios = new Map<string, string>();
    const ufs = new Set<string>();
    for (const c of daFilial) {
      for (const m of c.municipios) {
        municipios.set(String(m.id), `${m.nome}/${m.uf}`);
        ufs.add(m.uf);
      }
    }
    return {
      carteiras: [...daFilial].sort((a, b) => a.carteiraNome.localeCompare(b.carteiraNome, 'pt-BR')),
      municipios: [...municipios.entries()].sort((a, b) => a[1].localeCompare(b[1], 'pt-BR')),
      ufs: [...ufs].sort(),
    };
  }, [listaDeCarteiras, filial]);

  const totais = useMemo(() => {
    const comCidade = carteirasDoRecorte.filter((c) => c.municipios.length > 0);
    const municipios = new Set<number>();
    for (const c of carteirasDoRecorte) for (const m of c.municipios) municipios.add(m.id);
    const ufs = new Set<string>();
    for (const c of carteirasDoRecorte) for (const m of c.municipios) ufs.add(m.uf);
    return {
      carteiras: carteirasDoRecorte.length,
      comCidade: comCidade.length,
      municipios: municipios.size,
      ufs: [...ufs].sort(),
      filiais: recorteTerritorial ? new Set(carteirasDoRecorte.map((c) => c.empresaCodigo)).size : listaDeFiliais.length,
    };
  }, [carteirasDoRecorte, recorteTerritorial, listaDeFiliais.length]);

  // O NÍVEL 1: as linhas da rota quando nada recorta; com recorte, a mesma conta feita sobre as carteiras do recorte.
  const linhasDoNivel1 = useMemo<CoberturaDeFilial[]>(() => {
    if (!recorteTerritorial) return listaDeFiliais;
    return listaDeFiliais
      .map((f) => {
        const daFilial = carteirasDoRecorte.filter((c) => c.empresaCodigo === f.empresaCodigo);
        return {
          ...f,
          carteiras: daFilial.length,
          carteirasComMunicipio: daFilial.filter((c) => c.municipios.length > 0).length,
          municipios: new Set(daFilial.flatMap((c) => c.municipios.map((m) => m.id))).size,
          ufs: [...new Set(daFilial.flatMap((c) => c.municipios.map((m) => m.uf)))].sort(),
        };
      })
      .filter((f) => f.carteiras > 0);
  }, [recorteTerritorial, listaDeFiliais, carteirasDoRecorte]);

  const visiveis = useMemo(
    () =>
      [...carteirasDoRecorte]
        .filter((c) => !somenteComCidade || c.municipios.length > 0)
        .sort((a, b) => b.municipios.length - a.municipios.length || a.carteiraNome.localeCompare(b.carteiraNome)),
    [carteirasDoRecorte, somenteComCidade],
  );

  /** Quantas carteiras não declaram nenhuma cidade — dito uma vez, e não por linha. */
  const semCidade = useMemo(() => carteirasDoRecorte.filter((c) => c.municipios.length === 0).length, [carteirasDoRecorte]);

  /**
   * A cobertura de contato, que é o que os dois gráficos do protótipo mediam.
   *
   * O PERCENTUAL É DE CONTATO NO PERÍODO sobre os vínculos da carteira, e o
   * denominador está escrito na tela. O protótipo media "clientes A ou B
   * tocados" — a classe do VÍNCULO não se sustenta (`ClienteCarteira.Classe`
   * segue entrando como C por assunção na maioria dos casos), então o recorte
   * por classe do vínculo saiu e o universo passou a ser a carteira inteira. A
   * classe do CLIENTE (curva ABC apurada do faturamento) sustenta o filtro
   * "Tipo de cliente".
   *
   * AS TRÊS PARTES SOMAM O TOTAL: com contato no período, com contato antes dele
   * (ou sem data) e nunca contatados.
   */
  const coberturaGeral = useMemo(() => {
    const clientes = coberturaDoRecorte.reduce((s, c) => s + c.clientes, 0);
    const noPeriodo = coberturaDoRecorte.reduce((s, c) => s + comContato(c, periodo), 0);
    const nunca = coberturaDoRecorte.reduce((s, c) => s + c.nuncaContatados, 0);
    const fora = Math.max(0, clientes - noPeriodo - nunca);
    return { clientes, noPeriodo, nunca, fora, pct: clientes > 0 ? (noPeriodo / clientes) * 100 : null };
  }, [coberturaDoRecorte, periodo]);

  /** As carteiras do gráfico, na ordem escolhida. */
  const comVinculo = useMemo(() => coberturaDoRecorte.filter((c) => c.clientes > 0), [coberturaDoRecorte]);
  const barras = useMemo(() => {
    const pct = (c: ResumoDeCobertura) => comContato(c, periodo) / c.clientes;
    const ordenadas = [...comVinculo].sort((a, b) =>
      ordem === 'maior-cobertura'
        ? pct(b) - pct(a) || b.clientes - a.clientes
        : ordem === 'menor-cobertura'
          ? pct(a) - pct(b) || b.clientes - a.clientes
          : ordem === 'maior-carteira'
            ? b.clientes - a.clientes
            : a.carteiraNome.localeCompare(b.carteiraNome, 'pt-BR'),
    );
    return ordenadas.slice(0, BARRAS_NO_GRAFICO).map((c) => ({
      // O NOME VAI INTEIRO. Ele era cortado no caractere 11 porque virava
      // rótulo de eixo em canvas e não cabia; em HTML quem corta é o CSS,
      // com reticências, e o inteiro fica no `title`.
      chave: c.carteiraChave,
      titulo: c.carteiraNome,
      // Compacto de propósito: escrito por extenso — "4.016 vínculos · 2.911
      // com contato" — o detalhe quebrava em duas linhas e dobrava a altura
      // de cada barra. O cabeçalho do cartão já diz o que a fração mede.
      detalhe: `${nº(comContato(c, periodo))} de ${nº(c.clientes)}`,
      valor: pct(c) * 100,
    }));
  }, [comVinculo, ordem, periodo]);

  function alternar(chave: string) {
    setAbertas((atual) => {
      const proximo = new Set(atual);
      if (proximo.has(chave)) proximo.delete(chave);
      else proximo.add(chave);
      return proximo;
    });
  }

  const comCidadeAbertas = visiveis.filter((c) => c.municipios.length > 0);
  const tudoAberto = nivel1Aberto && nivel2Aberto && comCidadeAbertas.length > 0 && comCidadeAbertas.every((c) => abertas.has(c.carteiraChave));

  function expandirTudo() {
    if (tudoAberto) {
      setAbertas(new Set());
      return;
    }
    setNivel1Aberto(true);
    setNivel2Aberto(true);
    setAbertas(new Set(comCidadeAbertas.map((c) => c.carteiraChave)));
  }

  // O CLIQUE NA FILIAL DO NÍVEL 1 DESDOBRA NO NÍVEL 2: escolhe a filial no filtro e abre a lista dela.
  function abrirFilial(empresaCodigo: string) {
    setFilial(empresaCodigo);
    setCarteira('');
    setNivel2Aberto(true);
    document.getElementById('nivel-2-por-carteira')?.scrollIntoView?.({ block: 'start', behavior: 'smooth' });
  }

  // SEM CARTEIRA AO ALCANCE, O NÚMERO É O TRAÇO COM O MOTIVO; LENDO, O CARTÃO PULSA E NÃO AFIRMA MOTIVO NENHUM.
  const lendo = filiais.carregando || carteiras.carregando;
  const semCarteira = lendo ? undefined : recorteTerritorial ? 'Nenhuma carteira neste recorte dos filtros.' : 'Sem filial com carteira ao alcance deste contexto.';
  const pctDe = (parte: number, todo: number) => (todo > 0 ? percentual((parte / todo) * 100) : '—');

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga cobf-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Cobertura por Filial e Carteira</h1>
          <p className="page-subtitle">
            O agrupamento territorial que existe no dado: a filial da carteira e as cidades que ela atende.
          </p>
        </div>
        <p className="dash-atualizado">
          {filiais.procedencia ? <DadosAtualizadosEm procedencia={filiais.procedencia} dicaNoTexto /> : 'Lendo a cobertura…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              filiais.recarregar();
              carteiras.recarregar();
              cobertura.recarregar();
            }}
            disabled={lendo || cobertura.carregando}
            data-carregando={lendo || cobertura.carregando ? 'true' : 'false'}
            aria-label="Reler a cobertura por filial"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* OS SEIS FILTROS DA MAQUETE, todos aplicados na hora. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <Filtro icone={CalendarDays} cor="verde" rotulo="Período" bloco="periodo" dica="O período do contato: o medidor, as barras e a cobertura de cada carteira contam os vínculos com interação nesses dias.">
            <select value={periodo} onChange={(e) => setPeriodo(Number(e.target.value) as Periodo)}>
              <option value={90}>Últimos 90 dias</option>
              <option value={30}>Últimos 30 dias</option>
            </select>
          </Filtro>
          <Filtro icone={Building2} cor="verde" rotulo="Filial" bloco="filial" dica="Recorta dentro das filiais que o contexto de acesso alcança — a do cabeçalho, ou todas.">
            <select
              value={filial}
              onChange={(e) => {
                setFilial(e.target.value);
                setCarteira('');
                setMunicipio('');
                setUf('');
              }}
            >
              <option value="">Todas as filiais</option>
              {listaDeFiliais.map((f) => (
                <option key={f.empresaChave} value={f.empresaCodigo}>
                  {f.empresaNome}
                </option>
              ))}
            </select>
          </Filtro>
          <Filtro icone={BriefcaseBusiness} cor="azul" rotulo="Carteira" bloco="carteira">
            <select value={carteira} onChange={(e) => setCarteira(e.target.value)}>
              <option value="">Todas as carteiras</option>
              {opcoes.carteiras.map((c) => (
                <option key={c.carteiraChave} value={c.carteiraChave}>
                  {c.carteiraNome}
                </option>
              ))}
            </select>
          </Filtro>
          <Filtro
            icone={Users}
            cor="roxo"
            rotulo="Tipo de cliente"
            bloco="tipo"
            dica="A classe do cliente na curva ABC do faturamento. Recorta a cobertura de contato (o medidor e as barras); sem classe apurada, o cliente conta como D."
          >
            <select value={classe} onChange={(e) => setClasse(e.target.value)}>
              <option value="">Todos</option>
              <option value="A">Classe A</option>
              <option value="B">Classe B</option>
              <option value="C">Classe C</option>
              <option value="D">Classe D</option>
            </select>
          </Filtro>
          <Filtro icone={MapPin} cor="verde" rotulo="Município" bloco="municipio" dica="Deixa as carteiras que atendem o município — e a cobertura de contato delas.">
            <select value={municipio} onChange={(e) => setMunicipio(e.target.value)} disabled={opcoes.municipios.length === 0}>
              <option value="">Todos os municípios</option>
              {opcoes.municipios.map(([id, nome]) => (
                <option key={id} value={id}>
                  {nome}
                </option>
              ))}
            </select>
          </Filtro>
          <Filtro icone={BarChart3} cor="laranja" rotulo="Estado" bloco="estado" dica="Deixa as carteiras com alguma cidade no estado.">
            <select value={uf} onChange={(e) => setUf(e.target.value)} disabled={opcoes.ufs.length === 0}>
              <option value="">Todos os estados</option>
              {opcoes.ufs.map((u) => (
                <option key={u} value={u}>
                  {u}
                </option>
              ))}
            </select>
          </Filtro>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={filiais.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="Filiais ao alcance"
          icone={Building2}
          tom="demanda"
          valor={filiais.dados ? nº(totais.filiais) : null}
          carregando={filiais.carregando}
          motivoSemDado={filiais.carregando ? undefined : 'Sem filial com carteira ao alcance deste contexto.'}
          variacao={filiais.dados ? (totais.filiais === 1 ? 'filial selecionada' : 'filiais selecionadas') : null}
          sobre="O contexto de acesso enxerga a filial do cabeçalho (ou todas, quando o perfil alcança todas). A fronteira de multiempresa vale aqui como em toda consulta; o filtro Filial recorta dentro dela."
        />
        <CartaoDeDecisao
          rotulo="Carteiras"
          icone={BriefcaseBusiness}
          tom="mercado"
          valor={carteiras.dados ? nº(totais.carteiras) : null}
          carregando={carteiras.carregando}
          unidade="carteiras"
          motivoSemDado={semCarteira}
          variacao={carteiras.dados ? 'ativas, com e sem cidade' : null}
          sobre="As carteiras destas filiais que não foram excluídas, com e sem cidade declarada."
        />
        <CartaoDeDecisao
          rotulo="Com cidade"
          icone={MapPinned}
          tom="oportunidade"
          valor={carteiras.dados ? nº(totais.comCidade) : null}
          carregando={carteiras.carregando}
          unidade="carteiras"
          motivoSemDado={semCarteira}
          variacao={carteiras.dados ? (totais.carteiras > 0 ? `${pctDe(totais.comCidade, totais.carteiras)} das carteiras` : 'sem carteira no recorte') : null}
          sobre={`De ${totais.carteiras} carteiras, as que declaram ao menos uma cidade — a diferença é a lacuna do cadastro de origem.`}
        />
        <CartaoDeDecisao
          rotulo="Municípios atendidos"
          icone={MapPin}
          tom="demanda"
          valor={carteiras.dados ? nº(totais.municipios) : null}
          carregando={carteiras.carregando}
          unidade="municípios"
          motivoSemDado={semCarteira}
          variacao={carteiras.dados ? 'distintos, pelas carteiras' : null}
          sobre="Municípios distintos declarados nas carteiras (organizacao.CarteiraMunicipio)."
        />
        <CartaoDeDecisao
          rotulo="Estados"
          icone={BarChart3}
          tom="captura"
          valor={carteiras.dados ? totais.ufs.join(', ') || null : null}
          carregando={carteiras.carregando}
          motivoSemDado={carteiras.carregando ? undefined : 'Nenhuma carteira declara cidade.'}
          variacao={carteiras.dados ? (totais.ufs.length > 0 ? 'pelas cidades cadastradas' : 'sem dados') : null}
          sobre="As UFs alcançadas pelas cidades cadastradas nas carteiras."
        />
      </div>

      {/* O AVISO DA MAQUETE: o que estes números não dizem, com as lacunas que a rota das filiais mediu. Fecha no X, e
          volta na próxima visita — é aviso, e não preferência. */}
      {!avisoFechado && (
        <AvisoDaTela titulo="O que estes números não dizem" aoFechar={() => setAvisoFechado(true)} bloco="aviso">
          <p>
            Os indicadores mostram a cobertura de contato das carteiras no período escolhido e o território que elas
            declaram. Eles não substituem a estratégia da equipe de campo nem medem a qualidade do relacionamento com os
            clientes.
          </p>
          <MetricasSemDado metricas={filiais.dados?.metricasSemDado} naDica titulo="O que a leitura das filiais não afirma" />
        </AvisoDaTela>
      )}

      {/* ------------------------------------------------------------------ */}
      {/* Os dois gráficos do protótipo, com o dado real                      */}
      {/* ------------------------------------------------------------------ */}
      <section className="dash-secao" data-bloco="secao-contato">
        <TituloDaSecao
          titulo="A cobertura do contato"
          icone={<BarChart3 size={20} strokeWidth={2.4} className="cobf-icone" aria-hidden="true" />}
          subtitulo={`Visualize, nos últimos ${periodo} dias, o alcance da sua carteira.`}
        />

        <div className="cobf-dupla">
          <PainelDoMomento
            titulo="Cobertura no período"
            icone={<BarChart3 size={15} strokeWidth={2.4} className="cobf-icone" aria-hidden="true" />}
            data-bloco="medidor"
            subtitulo={`Vínculos com interação nos últimos ${periodo} dias, sobre o total da carteira.`}
            dica={
              <>
                <p>
                  As faixas de cor são de leitura, e <strong>não são meta</strong>: nenhuma meta de cobertura foi cadastrada,
                  nem no sistema antigo nem no protótipo, onde os 80% eram um número fixo no código. O medidor mostra onde a
                  operação está, e não se ela passou.
                </p>
                <p>
                  O percentual é de contato no período sobre os vínculos da carteira, e o denominador está escrito na tela. O
                  recorte por classe do vínculo saiu: a classe do vínculo entra como C por assunção na maioria dos casos. A
                  classe do cliente — a curva ABC apurada do faturamento — é o filtro &quot;Tipo de cliente&quot;.
                </p>
              </>
            }
          >
            {cobertura.carregando && <BlocoCarregando oQue="a cobertura de contato" />}
            {cobertura.erro && <BlocoErro erro={cobertura.erro} aoTentarDeNovo={cobertura.recarregar} />}

            {cobertura.dados && coberturaGeral.pct === null && !cobertura.erro && (
              <BlocoVazio
                titulo="Sem vínculo de carteira para medir"
                texto="O medidor mede contato sobre carteira. Sem cliente carteirizado neste recorte, não há denominador."
              />
            )}

            {coberturaGeral.pct !== null && (
              <div className="cad-medidor cobf-medidor">
                <div className="cobf-medidor-arco">
                  <GraficoGauge
                    valor={coberturaGeral.pct}
                    faixas={FAIXAS_DO_MEDIDOR}
                    legenda="Cobertura no período"
                    espessura={22}
                    separadores
                    corPonteiro="#1f2a24"
                    corPivo="#22a35a"
                  />
                </div>
                {/* AS TRÊS PARTES DO TOTAL, cada uma com a fração dela. */}
                <ul className="cobf-partes" aria-label="Os vínculos da carteira pelo contato">
                  <Parte icone={CircleCheck} tom="verde" valor={coberturaGeral.noPeriodo} rotulo="dentro do período" pct={pctDe(coberturaGeral.noPeriodo, coberturaGeral.clientes)} dica={`Com interação nos últimos ${periodo} dias.`} />
                  <Parte icone={Clock3} tom="laranja" valor={coberturaGeral.fora} rotulo="fora do período" pct={pctDe(coberturaGeral.fora, coberturaGeral.clientes)} dica={`Já tiveram contato, mas não nos últimos ${periodo} dias.`} />
                  <Parte icone={UserRound} tom="cinza" valor={coberturaGeral.nunca} rotulo="nunca contatados" pct={pctDe(coberturaGeral.nunca, coberturaGeral.clientes)} dica="Nenhuma interação registrada." />
                </ul>
                <p className="cad-medidor-legenda">
                  <strong>
                    {nº(coberturaGeral.noPeriodo)} de {nº(coberturaGeral.clientes)} vínculos
                  </strong>{' '}
                  tiveram contato nos últimos {periodo} dias.
                </p>
              </div>
            )}
          </PainelDoMomento>

          <PainelDoMomento
            titulo="Cobertura por carteira"
            data-bloco="por-carteira"
            subtitulo={`% dos vínculos com contato nos últimos ${periodo} dias · ${Math.min(BARRAS_NO_GRAFICO, barras.length)} de ${comVinculo.length} carteiras.`}
            dica="A barra mede, e não julga: não há cor por atingimento, porque não há meta de cobertura. O denominador é 100%, para cada barra dizer quanto da carteira tem contato, e não só quem é maior."
            direita={
              <label className="cobf-ordenar">
                <span>Ordenar por</span>
                <select value={ordem} onChange={(e) => setOrdem(e.target.value as Ordem)}>
                  {ORDENS.map((o) => (
                    <option key={o.valor} value={o.valor}>
                      {o.rotulo}
                    </option>
                  ))}
                </select>
              </label>
            }
          >
            {cobertura.carregando && <BlocoCarregando oQue="a cobertura por carteira" />}

            {cobertura.dados && barras.length === 0 && !cobertura.erro && (
              <BlocoVazio
                titulo="Nenhuma carteira com cliente vinculado"
                texto="As barras medem contato sobre carteira; sem vínculo não há barra para desenhar."
              />
            )}

            {barras.length > 0 && (
              /* A BARRA MEDE, E NÃO JULGA. Não há cor por atingimento aqui —
                 `organizacao.Meta` está vazia, e pintar de verde e vermelho
                 contra uma meta inventada afirmaria aprovação e reprovação que
                 ninguém definiu. O denominador é 100%, para cada barra dizer
                 quanto da carteira tem contato, e não só quem é maior. */
              <ListaDeBarras itens={barras} maximo={100} formatar={percentual} />
            )}
          </PainelDoMomento>
        </div>
      </section>

      <section className="dash-secao" data-bloco="secao-territorio">
        <TituloDaSecao
          titulo="O território"
          icone={<MapPin size={20} strokeWidth={2.4} className="cobf-icone-escuro" aria-hidden="true" />}
          subtitulo={
            <span title="IVS_Carteira.NroEmpresa está preenchida em todas as carteiras, e IVS_CartCid liga carteira e cidade. A carteira sem cidade aparece com a lista vazia em vez de sumir: escondê-la faria a tela mostrar uma operação menor do que ela é.">
              Filial → carteira → cidades, os dois níveis que existem no dado.
            </span>
          }
          acao={
            <button type="button" className="cobf-botao" onClick={expandirTudo} disabled={comCidadeAbertas.length === 0}>
              {tudoAberto ? <Shrink size={14} strokeWidth={2.2} aria-hidden="true" /> : <Expand size={14} strokeWidth={2.2} aria-hidden="true" />}
              {tudoAberto ? 'Recolher todos' : 'Expandir todos'}
            </button>
          }
        />

        <PainelDoMomento
          titulo="Nível 1 — por filial"
          icone={<BarChart3 size={15} strokeWidth={2.4} className="cobf-icone-escuro" aria-hidden="true" />}
          data-bloco="por-filial"
          data-aberto={nivel1Aberto ? 'true' : 'false'}
          subtitulo="Só as filiais que este contexto de acesso alcança. A fronteira de multiempresa vale aqui como em toda consulta."
          direita={<BotaoDeRecolher aberto={nivel1Aberto} aoAlternar={() => setNivel1Aberto((a) => !a)} oQue="o nível 1" />}
        >
          {nivel1Aberto && (
            <>
              {filiais.carregando && <BlocoCarregando oQue="a cobertura por filial" />}
              {filiais.erro && <BlocoErro erro={filiais.erro} aoTentarDeNovo={filiais.recarregar} />}

              {filiais.dados && linhasDoNivel1.length === 0 && !filiais.erro && (
                <BlocoVazio
                  titulo={recorteTerritorial ? 'Nenhuma filial neste recorte' : 'Nenhuma filial com carteira ao seu alcance'}
                  texto="A cobertura territorial parte da carteira, e a carteira carrega a fronteira de filial. Troque a filial no cabeçalho para conferir."
                />
              )}

              {linhasDoNivel1.length > 0 && (
                <div className="mom-tabela-rolagem">
                  <table className="mom-tabela cobf-tabela">
                    <caption className="cad-so-leitor">Cobertura territorial por filial</caption>
                    <thead>
                      <tr>
                        <th scope="col">Filial</th>
                        <th scope="col" className="mom-num">Carteiras</th>
                        <th scope="col" className="mom-num">Com cidade</th>
                        <th scope="col" className="mom-num">Municípios</th>
                        <th scope="col">Estados</th>
                        <th scope="col" className="cobf-coluna-abrir">
                          <span className="cad-so-leitor">Abrir as carteiras</span>
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {linhasDoNivel1.map((f) => (
                        <tr key={f.empresaChave}>
                          <th scope="row">
                            <div className="cad-link-forte">{f.empresaNome}</div>
                            <div className="cad-sub">{f.empresaCodigo}</div>
                          </th>
                          <td className="mom-num">{f.carteiras}</td>
                          <td className="mom-num">
                            <span className={f.carteirasComMunicipio < f.carteiras ? 'cad-atencao' : undefined}>
                              {f.carteirasComMunicipio}
                            </span>
                            <div className="cad-sub">{f.carteiras - f.carteirasComMunicipio} sem nenhuma cidade</div>
                          </td>
                          <td className="mom-num">{f.municipios}</td>
                          <td>{f.ufs.length > 0 ? f.ufs.join(', ') : <span className="cad-nada">—</span>}</td>
                          <td className="cobf-coluna-abrir">
                            <button
                              type="button"
                              className="cobf-abrir"
                              onClick={() => abrirFilial(f.empresaCodigo)}
                              aria-label={`Ver as carteiras de ${f.empresaNome}`}
                              title="Ver as carteiras desta filial no nível 2"
                            >
                              <ChevronRight size={16} strokeWidth={2.2} aria-hidden="true" />
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Nível 2 — por carteira"
          icone={<Users size={15} strokeWidth={2.4} className="cobf-icone-escuro" aria-hidden="true" />}
          id="nivel-2-por-carteira"
          data-bloco="por-carteira-territorio"
          data-aberto={nivel2Aberto ? 'true' : 'false'}
          subtitulo={
            carteiras.recarregando
              ? 'Atualizando…'
              : `${visiveis.length} de ${totais.carteiras} carteiras · ${semCidade} não declaram nenhuma cidade no sistema de origem e aparecem com zero, em vez de sumir da lista`
          }
          dica="Clique numa carteira para ver as cidades que ela atende. A carteira sem cidade fica na lista, com zero, e não abre. A cobertura de cada uma é a do período escolhido."
          direita={
            <span className="cobf-acoes-do-nivel">
              <label className="dash-caixa">
                <input type="checkbox" checked={somenteComCidade} onChange={(e) => setSomenteComCidade(e.target.checked)} />
                Só as carteiras com cidade cadastrada
              </label>
              <BotaoDeRecolher aberto={nivel2Aberto} aoAlternar={() => setNivel2Aberto((a) => !a)} oQue="o nível 2" />
            </span>
          }
        >
          {nivel2Aberto && (
            <>
              <AvisoDaTela titulo="O que você está vendo" bloco="aviso-nivel-2">
                <p>
                  As carteiras deste recorte, com as cidades que cada uma atende e a cobertura de contato dos últimos {periodo}{' '}
                  dias. A carteira sem cidade fica na lista, com zero.
                </p>
                <MetricasSemDado metricas={carteiras.dados?.metricasSemDado} naDica titulo="O que esta lista não diz" />
              </AvisoDaTela>

              {carteiras.carregando && <BlocoCarregando oQue="o território das carteiras" />}
              {carteiras.erro && <BlocoErro erro={carteiras.erro} aoTentarDeNovo={carteiras.recarregar} />}

              {carteiras.dados && visiveis.length === 0 && !carteiras.erro && (
                <BlocoVazio
                  titulo={recorteTerritorial ? 'Nenhuma carteira neste recorte' : 'Nenhuma carteira desta filial declara cidade'}
                  texto={`O vínculo carteira × município vem da carga do sistema de origem. Das ${totais.carteiras} carteiras do recorte, ${semCidade} não têm nenhuma cidade cadastrada — a lacuna é do cadastro, não da operação.`}
                  acao={
                    somenteComCidade ? (
                      <button type="button" className="btn btn-secondary" onClick={() => setSomenteComCidade(false)}>
                        Mostrar todas as carteiras
                      </button>
                    ) : undefined
                  }
                />
              )}

              {visiveis.length > 0 && (
                <div className="cobf-carteiras">
                  {visiveis.map((c) => (
                    <GrupoDeCarteira
                      key={c.carteiraChave}
                      carteira={c}
                      cobertura={coberturaPorChave.get(c.carteiraChave) ?? null}
                      periodo={periodo}
                      aberta={abertas.has(c.carteiraChave)}
                      aoAlternar={() => alternar(c.carteiraChave)}
                    />
                  ))}
                </div>
              )}
            </>
          )}
        </PainelDoMomento>
      </section>

      {/* A CORREÇÃO DE PREMISSA CONTINUA NA TELA, e não só no documento: quem
          abriu esta rota antes viu sete regionais e pode voltar procurando por
          elas. Ela fica depois do território que de fato existe. */}
      <BlocoRecolhivel titulo="Por que esta tela não tem regional" resumo="as sete regionais do protótipo não existem no dado">
        <p className="cad-coluna-meta">
          MT Norte, GO, BA Oeste e mais quatro vinham do protótipo. No Vórtice, o cadastro de
          regionais <strong>existe e está vazio</strong> — nenhum dado sustenta essas regionais. E as
          treze filiais em operação estão todas no interior de São Paulo. O agrupamento preenchido é{' '}
          <strong>filial → carteira → cidades</strong>, e é o que esta tela mostra.
        </p>
      </BlocoRecolhivel>
    </PaginaDoPainel>
  );
}

/** Um filtro da maquete: o ícone colorido e um pouco maior, o nome pequeno em cima e o valor escolhido, numa caixa só. */
function Filtro(props: {
  icone: LucideIcon;
  cor: 'verde' | 'azul' | 'roxo' | 'laranja';
  rotulo: string;
  bloco: string;
  dica?: string;
  children: ReactNode;
}) {
  return (
    <CampoDoFiltro classe="cobf-filtro" tamanhoDoIcone={18} espessura={2.1} {...props} />
  );
}

/** Uma das três partes do total embaixo do medidor. */
function Parte({
  icone: Icone,
  tom,
  valor,
  rotulo,
  pct,
  dica,
}: {
  icone: LucideIcon;
  tom: 'verde' | 'laranja' | 'cinza';
  valor: number;
  rotulo: string;
  pct: string;
  dica: string;
}) {
  return (
    <li className="cobf-parte" data-tom={tom} title={dica}>
      <span className="cobf-parte-icone" aria-hidden="true">
        <Icone size={18} strokeWidth={2.2} />
      </span>
      <span className="cobf-parte-texto">
        <strong>{nº(valor)}</strong>
        <span>{rotulo}</span>
      </span>
      <span className="cobf-parte-pct">{pct}</span>
    </li>
  );
}

/** O aviso laranja da maquete: o ponto de exclamação, o título em negrito, o texto e, quando fecha, o X. */
function AvisoDaTela({ titulo, children, aoFechar, bloco }: { titulo: string; children: ReactNode; aoFechar?: () => void; bloco: string }) {
  return (
    <div className="cobf-aviso" role="note" data-bloco={bloco}>
      <CircleAlert size={18} strokeWidth={2.2} className="cobf-aviso-icone" aria-hidden="true" />
      <div className="cobf-aviso-corpo">
        <strong className="cobf-aviso-titulo">{titulo}</strong>
        {children}
      </div>
      {aoFechar && (
        <button type="button" className="cobf-aviso-fechar" onClick={aoFechar} aria-label={`Fechar o aviso "${titulo}"`}>
          <X size={16} strokeWidth={2.2} aria-hidden="true" />
        </button>
      )}
    </div>
  );
}

/** O chevron que recolhe e abre um nível. */
function BotaoDeRecolher({ aberto, aoAlternar, oQue }: { aberto: boolean; aoAlternar: () => void; oQue: string }) {
  return (
    <button type="button" className="cobf-recolher" onClick={aoAlternar} aria-expanded={aberto} aria-label={aberto ? `Recolher ${oQue}` : `Abrir ${oQue}`}>
      {aberto ? <ChevronUp size={18} strokeWidth={2.2} aria-hidden="true" /> : <ChevronDown size={18} strokeWidth={2.2} aria-hidden="true" />}
    </button>
  );
}

/** Uma carteira, com as cidades que ela atende — ou a declaração de que não tem nenhuma — e a cobertura de contato. */
function GrupoDeCarteira({
  carteira,
  cobertura,
  periodo,
  aberta,
  aoAlternar,
}: {
  carteira: TerritorioDeCarteira;
  cobertura: ResumoDeCobertura | null;
  periodo: Periodo;
  aberta: boolean;
  aoAlternar: () => void;
}) {
  const semCidade = carteira.municipios.length === 0;
  const ufs = [...new Set(carteira.municipios.map((m) => m.uf))].sort();
  const pct = cobertura && cobertura.clientes > 0 ? (comContato(cobertura, periodo) / cobertura.clientes) * 100 : null;

  return (
    <div className="cad-grupo cobf-grupo">
      <button
        type="button"
        className="cad-grupo-cabecalho"
        onClick={aoAlternar}
        aria-expanded={aberta}
        disabled={semCidade}
      >
        <div className="cobf-grupo-nome">
          <div className="cad-grupo-nome">
            {!semCidade && (aberta ? <ChevronDown size={14} strokeWidth={2.4} aria-hidden="true" /> : <ChevronRight size={14} strokeWidth={2.4} aria-hidden="true" />)}
            {carteira.carteiraNome}
          </div>
          <div className="cad-grupo-meta">
            {carteira.linhaDeNegocioNome} · {carteira.responsavelNome} · {carteira.empresaNome}
          </div>
        </div>
        {/* A COBERTURA DA CARTEIRA NO PERÍODO, ao lado do território — o cruzamento das duas leituras pela chave. */}
        <div className="cobf-grupo-cobertura" title={cobertura ? `${nº(comContato(cobertura, periodo))} de ${nº(cobertura.clientes)} vínculos com contato nos últimos ${periodo} dias` : 'Sem vínculo de cliente nesta carteira'}>
          {pct === null ? (
            <span className="cad-nada">sem vínculo</span>
          ) : (
            <>
              <span className="cobf-grupo-trilho" aria-hidden="true">
                <span style={{ width: `${Math.min(100, pct)}%` }} />
              </span>
              <span className="cobf-grupo-pct">{percentual(pct)}</span>
            </>
          )}
        </div>
        <div className="cad-grupo-numeros">
          {cobertura && (
            <div className="cad-grupo-numero">
              <strong>{nº(cobertura.clientes)}</strong>
              vínculos
            </div>
          )}
          <div className="cad-grupo-numero">
            <strong>{carteira.municipios.length}</strong>
            {carteira.municipios.length === 1 ? 'cidade' : 'cidades'}
          </div>
          {ufs.length > 0 && (
            <div className="cad-grupo-numero">
              <strong>{ufs.join(', ')}</strong>
              {ufs.length === 1 ? 'estado' : 'estados'}
            </div>
          )}
        </div>
      </button>

      {aberta && !semCidade && (
        <div className="cad-grupo-corpo">
          <div className="cad-cidades">
            {carteira.municipios.map((m) => (
              <span key={m.id} className="cad-cidade">
                {m.nome}/{m.uf}
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

/** Uma casa decimal, com vírgula, como o resto da aplicação escreve. */
function percentual(valor: number): string {
  return `${valor.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;
}
