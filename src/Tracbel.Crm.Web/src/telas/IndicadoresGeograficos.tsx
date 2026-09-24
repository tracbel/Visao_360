/**
 * Indicadores Geográficos da ADR — a Visão Diretoria (documento 50).
 *
 * ESTA É A CASCA. Ela busca, calcula os totais e monta duas abas sobre o MESMO
 * recorte: **Mercado** (padrão) e **Território**. Cada bloco mora no próprio
 * componente, em `componentes/mercado/` e `componentes/territorio/`.
 *
 * OS FILTROS E O MUNICÍPIO ESCOLHIDO FICAM ACIMA DAS ABAS, e é isso que faz as
 * duas serem leituras do mesmo recorte: escolher um município no mapa e ir para
 * Território continua falando do mesmo município. Se os filtros vivessem dentro
 * de uma aba, a outra seria outra página (fase T1; a issue 163 leva a escolha
 * para a URL e faz os painéis reagirem a ela).
 *
 * O QUE ESTA TELA NÃO FAZ, e diz na própria tela:
 * - não filtra por SAM/KAM/Varejo, tipo de produto nem modelo — não há dado;
 * - não mostra potencial de cliente nem de não cliente — não há área por
 *   propriedade; o potencial é o do município inteiro, por uma regra ainda a
 *   confirmar;
 * - não mostra valor em reais no potencial — não há preço de máquina confirmado;
 * - não oferece FYTD — o calendário fiscal não foi confirmado.
 *
 * OS NÚMEROS DA MAQUETE NÃO ESTÃO AQUI. Ela era ilustrativa; nenhum valor desta
 * tela é simulado.
 */

// `Map` do lucide entra COM OUTRO NOME: esta tela usa `new Map(...)` e
// `Map<string, string>` em quatro pontos, e o import sombrearia o construtor
// nativo — um erro que o TypeScript aceitaria calado até o primeiro `new`.
import { ChartSpline, Map as IconeMapa, RefreshCw } from 'lucide-react';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { AbaDeMercado } from '../componentes/mercado/AbaDeMercado';
import { produtosDoMunicipio } from '../componentes/mercado/culturasDoMunicipio';
import { AbasDaTela, type Aba } from '../componentes/territorio/AbasDaTela';
import { AbaDeTerritorio } from '../componentes/territorio/AbaDeTerritorio';
import { AvisoDeTerritorioSemCarga } from '../componentes/territorio/AvisoDeTerritorioSemCarga';
import { DetalheDoMunicipio } from '../componentes/territorio/DetalheDoMunicipio';
import { FiltrosDosIndicadores } from '../componentes/territorio/FiltrosDosIndicadores';
import { LARGURA_DO_DESENHO, recorteFiltrado } from '../componentes/territorio/indicadoresDaAdr';
import { kpisDaCarteira, kpisDoMercado, type ContextoDosKpis } from '../componentes/territorio/kpisDosIndicadores';
import type { LigacaoDoMapa } from '../componentes/territorio/mapas/CartaoDeMapa';
import { calcularFatiaNoEstado, calcularTotais } from '../componentes/territorio/totaisDaAdr';
import type { PoligonoProjetado } from '../componentes/territorio/MapaDeMunicipios';
import { caminhoSvg, enquadrar, type ColecaoMunicipal } from '../componentes/territorio/projecao';
import { ComparacaoComPeriodoAnterior } from '../componentes/territorio/SecaoDoMercadoDaRegiao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { carregarMalhaDeSaoPaulo, obterIndicadoresTerritoriais } from '../dados/api/territorio';
import { useRecurso } from '../dados/api/useRecurso';
import type { ClassificacaoDeIndicador, FiltrosTerritoriais } from '../tipos/territorio';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';

type IdDaAba = 'mercado' | 'territorio';

/** Mercado é a primeira porque o público desta tela é a diretoria (documento 50, §3). */
const ABAS: readonly Aba<IdDaAba>[] = [
  { id: 'mercado', rotulo: 'Mercado', icone: ChartSpline },
  { id: 'territorio', rotulo: 'Território', icone: IconeMapa },
];

export function IndicadoresGeograficos() {
  const { contexto } = useContextoDeAcesso();

  const [filtros, setFiltros] = useState<FiltrosTerritoriais>({
    competenciaInicial: '',
    competenciaFinal: '',
    regiao: '',
    lojaCodigo: '',
    visao: 'Filial',
    filialDaVenda: '',
    filialDoCliente: '',
  });
  // O MUNICÍPIO ESCOLHIDO MORA NA URL (issue 163), e não em `useState`.
  //
  // A URL é o que sobrevive ao F5, ao voltar do navegador e ao link colado no
  // chat — e é a única fonte da verdade aqui: guardar a escolha também em estado
  // local criaria duas versões dela, que discordam assim que alguém usa o botão
  // voltar. O roteador é o que a aplicação já usa (`createHashRouter`).
  const [parametros, definirParametros] = useSearchParams();
  const pedido = Number(parametros.get('municipio'));

  // CÓDIGO INVÁLIDO NÃO É ERRO NEM DADO INVENTADO: `?municipio=banana` e
  // `?municipio=999` simplesmente não escolhem ninguém, e a tela abre inteira.
  const selecionado = Number.isSafeInteger(pedido) && pedido > 0 ? pedido : null;

  const escolherMunicipio = useCallback(
    (codigo: number | null) =>
      definirParametros(
        (atuais) => {
          const proximos = new URLSearchParams(atuais);
          if (codigo === null) proximos.delete('municipio');
          else proximos.set('municipio', String(codigo));
          return proximos;
        },
        // Cada escolha é uma entrada no histórico: é o que faz voltar e avançar
        // andarem entre municípios, que é como se compara dois deles.
        { replace: false },
      ),
    [definirParametros],
  );

  // A ABA NÃO MORA NA URL — ela não é do recorte, e trocar de aba não muda o
  // endereço (issue 163). Mas a ABA INICIAL segue a URL (fidelidade às
  // maquetes, 23/09/2026): a ficha do município passou a morar só em
  // Território, e um endereço com `?municipio=` — um F5, o botão voltar, um
  // link colado no chat — é o de quem estava olhando uma ficha. Abrir em
  // Mercado esconderia justamente o que o link aponta.
  const [aba, setAba] = useState<IdDaAba>(() => (selecionado !== null ? 'territorio' : 'mercado'));

  // CLICAR NO MAPA ESCOLHE E LEVA À FICHA (decisão do usuário, 23/09/2026). A
  // escolha é a mesma de sempre — o parâmetro na URL, que vale para as duas
  // abas —, e a aba troca para Território, onde a ficha abre ao lado da linha
  // do município. O mapa fica mais embaixo na página do que a ficha; sem rolar,
  // a troca de aba deixaria a pessoa olhando o fim de uma página mais curta.
  const levarAFicha = useRef(false);
  const escolherNoMapa = useCallback(
    (codigo: number) => {
      escolherMunicipio(codigo);
      levarAFicha.current = true;
      setAba('territorio');
    },
    [escolherMunicipio],
  );

  useEffect(() => {
    if (!levarAFicha.current || aba !== 'territorio') return;
    levarAFicha.current = false;
    const abaDeTerritorio = document.getElementById('aba-territorio');
    // `?.()` porque o jsdom dos testes não implementa `scrollIntoView`.
    abaDeTerritorio?.scrollIntoView?.({ block: 'start' });
    // O FOCO VAI PARA A ABA QUE ABRIU: o leitor de tela anuncia "Território,
    // aba, selecionada", e o Tab seguinte continua dali — e não do topo.
    abaDeTerritorio?.focus({ preventScroll: true });
  }, [aba]);

  const [emFoco, setEmFoco] = useState<number | null>(null);
  const [lojasConhecidas, setLojasConhecidas] = useState<Map<string, string>>(() => new Map());
  const [adrConhecida, setAdrConhecida] = useState<ReadonlySet<number>>(() => new Set());

  // AS LOJAS DO FILTRO E O QUADRO DO MAPA SÃO OS QUE A ADR JÁ MOSTROU, acumulados a cada resposta.
  // Tirá-los só da resposta corrente faria a lista encolher — e o mapa mudar de enquadramento — ao
  // escolher uma região, e duas capturas da mesma cidade deixariam de ser comparáveis.
  const painel = useRecurso(
    (sinal) =>
      obterIndicadoresTerritoriais(contexto, filtros, sinal).then((resposta) => {
        const municipiosDaResposta = resposta.dados.indicadores.municipios;
        setLojasConhecidas((atual) => {
          const proximo = new Map(atual);
          for (const m of municipiosDaResposta)
            if (m.lojaCodigo && m.lojaNome) proximo.set(m.lojaCodigo, m.lojaNome.replace(/^.*—\s*/, ''));
          return proximo.size === atual.size ? atual : proximo;
        });
        setAdrConhecida((atual) => {
          const novos = municipiosDaResposta.filter((m) => m.pertenceAAdr && !atual.has(m.codigoIbge));
          return novos.length === 0 ? atual : new Set([...atual, ...novos.map((m) => m.codigoIbge)]);
        });
        return resposta;
      }),
    [
      contexto.empresa,
      contexto.usuario,
      filtros.competenciaInicial,
      filtros.competenciaFinal,
      filtros.regiao,
      filtros.lojaCodigo,
      filtros.visao,
      filtros.filialDaVenda,
      filtros.filialDoCliente,
    ],
  );

  const [malha, setMalha] = useState<ColecaoMunicipal | null>(null);
  const [erroDaMalha, setErroDaMalha] = useState<Error | null>(null);

  useEffect(() => {
    const controlador = new AbortController();
    carregarMalhaDeSaoPaulo(controlador.signal)
      .then(setMalha)
      .catch((causa: unknown) => {
        if (!controlador.signal.aborted) setErroDaMalha(causa instanceof Error ? causa : new Error(String(causa)));
      });
    return () => controlador.abort();
  }, []);

  const indicadores = painel.dados?.indicadores ?? null;
  const municipios = useMemo(() => indicadores?.municipios ?? [], [indicadores]);
  const porCodigo = useMemo(() => new Map(municipios.map((m) => [m.codigoIbge, m])), [municipios]);
  const adr = useMemo(() => new Set(municipios.filter((m) => m.pertenceAAdr).map((m) => m.codigoIbge)), [municipios]);
  const regra = indicadores?.regras[0] ?? null;

  /**
   * O potencial do RECORTE CONSULTADO, pelo motor (issue 72).
   *
   * Ele cobre os municípios que a consulta deixou passar — com filtro de região ou de loja, é a região
   * ou a loja; sem filtro, é o mapa inteiro, ADR e fora dela.
   */
  const recorte = indicadores?.potencialDoRecorte ?? null;

  const desenho = useMemo(() => {
    if (!malha) return null;
    // O QUADRO É A ADR INTEIRA, o mesmo nos quatro mapas. Os vizinhos continuam desenhados e o SVG corta o
    // que passa da borda: o mapa fica do tamanho da área de atuação, e não do estado.
    const enquadramento = enquadrar(
      malha,
      LARGURA_DO_DESENHO,
      10,
      adrConhecida.size > 0 ? (codigo) => adrConhecida.has(codigo) : undefined,
    );
    const poligonos: PoligonoProjetado[] = malha.features.map((f) => ({
      codigo: Number(f.properties.codarea),
      nome: f.properties.nome,
      caminho: caminhoSvg(enquadramento, f.geometry),
    }));
    return { enquadramento, poligonos };
  }, [malha, adrConhecida]);

  const nomeDoPoligono = useMemo(() => new Map((desenho?.poligonos ?? []).map((p) => [p.codigo, p.nome])), [desenho]);

  const daAdr = useMemo(() => municipios.filter((m) => m.pertenceAAdr), [municipios]);
  const totais = useMemo(() => calcularTotais(daAdr), [daAdr]);
  const fatiaNoEstado = useMemo(() => calcularFatiaNoEstado(indicadores?.estado, totais), [indicadores, totais]);

  const coberturaDaAdr = totais.elegiveis > 0 ? (100 * totais.cobertos) / totais.elegiveis : null;

  /** Os anos das fontes da estrutura — eles diferem, e a tela precisa dizer qual é qual. */
  const estruturaDaAdr = useMemo(
    () => ({
      anoDoCenso: daAdr.find((m) => m.estrutura.anoDoCenso !== null)?.estrutura.anoDoCenso ?? null,
      anoDoRebanho: daAdr.find((m) => m.estrutura.anoDoRebanho !== null)?.estrutura.anoDoRebanho ?? null,
    }),
    [daAdr],
  );

  const classificacao = (indicador: ClassificacaoDeIndicador['indicador']) =>
    painel.dados?.classificacoes.find((c) => c.indicador === indicador) ?? null;

  // TERRITÓRIO NÃO CARREGADO NÃO É ZERO. A consulta respondeu, sem filtro de região nem de loja, e nenhum
  // município veio marcado como ADR: a carga do território não rodou neste banco.
  const territorioNaoCarregado = indicadores !== null && filtros.regiao === '' && filtros.lojaCodigo === '' && daAdr.length === 0;
  const vendasForaDoMapa = indicadores?.foraDoMapa.reduce((s, g) => s + g.vendas.valorLiquido, 0) ?? 0;
  const semCodigoIbge = indicadores?.foraDoMapa.find((g) => g.grupo === 'MunicipioSemCodigoIbge') ?? null;
  const comTerritorio = indicadores !== null && !territorioNaoCarregado;

  const contextoDosKpis: ContextoDosKpis = {
    indicadores,
    comTerritorio,
    territorioNaoCarregado,
    municipiosDaAdr: daAdr.length,
    totais,
    fatiaNoEstado,
    coberturaDaAdr,
    recorte,
    vendasForaDoMapa,
    anoDoCenso: estruturaDaAdr.anoDoCenso,
    anoDoRebanho: estruturaDaAdr.anoDoRebanho,
    regiaoTracbel: indicadores?.regiaoTracbel ?? null,
    procedencias: indicadores?.procedencias ?? null,
  };

  const escolhido = selecionado === null ? null : porCodigo.get(selecionado) ?? null;
  const ordenados = useMemo(() => [...daAdr].sort((a, b) => b.vendas.valorLiquido - a.vendas.valorLiquido), [daAdr]);

  // AS CULTURAS DO MUNICÍPIO ESCOLHIDO, por área plantada (issue 168). Elas não
  // mudam o preço nem o custo — que são de São Paulo e da localidade da CONAB —,
  // mudam quais culturas aparecem primeiro nos dois painéis.
  const produtosPriorizados = useMemo(() => produtosDoMunicipio(escolhido), [escolhido]);
  const semFiltro = filtros.regiao === '' && filtros.lojaCodigo === '';
  // O NOME DO RECORTE FILTRADO (revisão de 24/09/2026): o Momento conta e pondera
  // sobre os municípios desta leitura, que já vêm filtrados — e "Região Tracbel",
  // na tela, é a área de atuação inteira. A loja sai pelo nome que o filtro mostra.
  const recorteDosFiltros = recorteFiltrado(
    filtros.regiao,
    filtros.lojaCodigo === '' ? null : (lojasConhecidas.get(filtros.lojaCodigo) ?? filtros.lojaCodigo),
  );

  const ligacao: LigacaoDoMapa | null = desenho && {
    enquadramento: desenho.enquadramento,
    poligonos: desenho.poligonos,
    adr,
    porCodigo,
    nomeDoPoligono,
    selecionado,
    aoSelecionar: escolherNoMapa,
    emFoco,
    aoPassar: setEmFoco,
  };

  // A FICHA É MONTADA UMA VEZ, e mora só em Território (fidelidade às maquetes,
  // 23/09/2026). Ela aparecia também embaixo dos mapas de Mercado — duas casas
  // para o mesmo detalhe. Abrir pelo mapa, pela tabela ou pelo campo de
  // município dá na mesma ficha, porque a escolha é uma só: a da URL.
  const ficha =
    escolhido && indicadores ? (
      <DetalheDoMunicipio
        municipio={escolhido}
        regras={indicadores.regras}
        culturasNoEstado={indicadores.culturasNoEstado}
        regiaoTracbel={indicadores.regiaoTracbel}
        estado={indicadores.estado}
        procedencias={indicadores.procedencias}
        numerosDeDecisao={painel.dados?.numerosDeDecisao ?? null}
        aoFechar={() => escolherMunicipio(null)}
      />
    ) : null;

  return (
    // O CONTAINER DA PÁGINA (fase T4.6). Sem ele, num monitor de 1920 os cartões
    // esticavam por quase 1800px: a linha de leitura ficava longa demais e a
    // grade de quatro KPIs virava quatro faixas separadas por vazio. A largura
    // está no `dashboard.css` e foi escolhida no harness, não no chute.
    //
    // ELE TAMBÉM É A RÉGUA DOS ARRANJOS (fidelidade às maquetes): as quebras
    // desta tela medem a largura DELE, e não a da janela — ver `dashboard.css`.
    <PaginaDoPainel>
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Indicadores Geográficos da ADR</h1>
          {/* O SUBTÍTULO É A LINHA DA MAQUETE (fase T4.9) — uma frase, com o
              nome da empresa em negrito como ela mostra. Ele tinha três linhas,
              e a terceira ("nenhum número desta tela é ilustrativo") não se
              perdeu: a procedência, na dica de "Dados atualizados em…", é o
              controle que responde de onde vem o dado. */}
          <p className="page-subtitle">
            Panorama de mercado, potencial e performance por município da área de atuação da{' '}
            <strong>Tracbel Agro.</strong>
          </p>
        </div>

        {/* QUANDO O DADO FOI LIDO, E O BOTÃO DE RELER (maquete) — e só isso.

            O SELO DE PROCEDÊNCIA INTEIRO (sistema, objeto e instante) ocupava
            uma segunda linha aqui, e a maquete reserva ao canto uma frase curta.
            Ele foi para a dica ao lado da hora, com a mesma frase de sempre; o
            aviso de dado desatualizado, que é alerta e não metadado, continua
            escrito na linha quando aparece.

            O ícone de recarga da maquete é decorativo; aqui ele refaz a consulta
            — um botão de atualizar que não atualiza é a promessa mais fácil de
            quebrar numa tela de dado. */}
        <p className="dash-atualizado">
          {painel.procedencia ? <DadosAtualizadosEm procedencia={painel.procedencia} /> : 'Lendo os indicadores…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={painel.recarregar}
            disabled={painel.carregando}
            data-carregando={painel.carregando ? 'true' : 'false'}
            aria-label="Reler os indicadores desta tela"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <FiltrosDosIndicadores
        filtros={filtros}
        aoMudarFiltros={setFiltros}
        lojasConhecidas={lojasConhecidas}
        regra={regra}
        indicadores={indicadores}
        respondeu={painel.dados !== null}
        podeVerEmpresaInteira={painel.dados?.podeVerEmpresaInteira ?? false}
        // O ALCANCE DA CONSULTA ("Visão: filial … e as abaixo dela") era uma
        // linha acima dos filtros; foi para a dica da Sub-região.
        empresa={contexto.empresa}
        // O MUNICÍPIO ESCOLHIDO É FILTRO DE RECORTE (T4.6): ele mora na linha
        // com os outros, e não flutuando entre blocos. Ele vale para as duas
        // abas, exatamente como sub-região e loja — por isso escolher no campo
        // NÃO troca de aba; quem leva à ficha é o clique no mapa.
        municipios={daAdr}
        municipioEscolhido={escolhido}
        aoEscolherMunicipio={escolherMunicipio}
      />

      {/* "COMO INTERPRETAR OS INDICADORES" morava aqui, num `<details>` entre os
          filtros e as abas; a maquete não o tem, e o texto foi para a dica ao
          lado do "Comparar com período anterior" (Mercado) e do título "A
          carteira na área de atuação" (Território). */}

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}
      {erroDaMalha && <BlocoErro erro={erroDaMalha} />}

      {territorioNaoCarregado && indicadores && (
        <AvisoDeTerritorioSemCarga
          vendasForaDoMapa={vendasForaDoMapa}
          vendasSemCodigoIbge={semCodigoIbge?.vendas.valorLiquido ?? null}
        />
      )}

      <AbasDaTela
        abas={ABAS}
        ativa={aba}
        aoTrocar={setAba}
        rotulo="Leituras do recorte"
        // A COMPARAÇÃO COM O PERÍODO ANTERIOR FICA NA LINHA DAS ABAS, e só em
        // Mercado — é onde a maquete de rentabilidade e crédito a põe, com os
        // quatro números logo abaixo. Em Território a maquete não a mostra aí.
        acessorio={
          aba === 'mercado' ? <ComparacaoComPeriodoAnterior classificacoes={painel.dados?.classificacoes ?? []} /> : null
        }
      >
        {aba === 'mercado' ? (
          <AbaDeMercado
            kpisDoMercado={kpisDoMercado(contextoDosKpis)}
            carregando={painel.carregando}
            ligacao={ligacao}
            indicadores={indicadores}
            totais={totais}
            comTerritorio={comTerritorio}
            coberturaDaAdr={coberturaDaAdr}
            regra={regra}
            recorte={recorte}
            semFiltro={semFiltro}
            contexto={contexto}
            anoDoCenso={estruturaDaAdr.anoDoCenso}
            anoDoRebanho={estruturaDaAdr.anoDoRebanho}
            classificacaoDe={classificacao}
            metricasSemDado={painel.dados?.metricasSemDado}
            municipioCodigoIbge={selecionado}
            nomeDoMunicipio={escolhido?.nome ?? null}
            produtosDoMunicipio={produtosPriorizados}
            mostrarOsMapas={indicadores !== null && desenho !== null && !territorioNaoCarregado}
            recorteDosFiltros={recorteDosFiltros}
            numerosDeDecisao={painel.dados?.numerosDeDecisao ?? null}
          />
        ) : (
          <AbaDeTerritorio
            kpisDaCarteira={kpisDaCarteira(contextoDosKpis)}
            carregando={painel.carregando}
            indicadores={indicadores !== null}
            municipios={municipios}
            ordenados={ordenados}
            foraDoMapa={indicadores?.foraDoMapa ?? []}
            totais={totais}
            selecionado={selecionado}
            aoSelecionar={escolherMunicipio}
            territorioNaoCarregado={territorioNaoCarregado}
            semFiltro={semFiltro}
            classificacoes={painel.dados?.classificacoes ?? []}
            ficha={ficha}
          />
        )}
      </AbasDaTela>
    </PaginaDoPainel>
  );
}
