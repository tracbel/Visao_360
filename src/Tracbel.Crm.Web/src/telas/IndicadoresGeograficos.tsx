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

import { useCallback, useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { AbaDeMercado } from '../componentes/mercado/AbaDeMercado';
import { AbasDaTela, type Aba } from '../componentes/territorio/AbasDaTela';
import { AbaDeTerritorio } from '../componentes/territorio/AbaDeTerritorio';
import { AvisoDeTerritorioSemCarga } from '../componentes/territorio/AvisoDeTerritorioSemCarga';
import { CartaoDeAlcance } from '../componentes/territorio/CartaoDeAlcance';
import { ChipDoMunicipio } from '../componentes/territorio/ChipDoMunicipio';
import { ComoLerEstesNumeros } from '../componentes/territorio/ComoLerEstesNumeros';
import { DetalheDoMunicipio } from '../componentes/territorio/DetalheDoMunicipio';
import { FiltrosDosIndicadores } from '../componentes/territorio/FiltrosDosIndicadores';
import { LARGURA_DO_DESENHO } from '../componentes/territorio/indicadoresDaAdr';
import { kpisDaCarteira, kpisDoMercado, type ContextoDosKpis } from '../componentes/territorio/kpisDosIndicadores';
import type { LigacaoDoMapa } from '../componentes/territorio/mapas/CartaoDeMapa';
import { calcularFatiaNoEstado, calcularTotais } from '../componentes/territorio/totaisDaAdr';
import type { PoligonoProjetado } from '../componentes/territorio/MapaDeMunicipios';
import { caminhoSvg, enquadrar, type ColecaoMunicipal } from '../componentes/territorio/projecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { carregarMalhaDeSaoPaulo, obterIndicadoresTerritoriais } from '../dados/api/territorio';
import { useRecurso } from '../dados/api/useRecurso';
import type { ClassificacaoDeIndicador, FiltrosTerritoriais } from '../tipos/territorio';
import '../estilos/territorio.css';

type IdDaAba = 'mercado' | 'territorio';

/** Mercado é a primeira porque o público desta tela é a diretoria (documento 50, §3). */
const ABAS: readonly Aba<IdDaAba>[] = [
  { id: 'mercado', rotulo: 'Mercado' },
  { id: 'territorio', rotulo: 'Território' },
];

export function IndicadoresGeograficos() {
  const { contexto } = useContextoDeAcesso();

  const [aba, setAba] = useState<IdDaAba>('mercado');
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
  const semFiltro = filtros.regiao === '' && filtros.lojaCodigo === '';

  const ligacao: LigacaoDoMapa | null = desenho && {
    enquadramento: desenho.enquadramento,
    poligonos: desenho.poligonos,
    adr,
    porCodigo,
    nomeDoPoligono,
    selecionado,
    aoSelecionar: escolherMunicipio,
    emFoco,
    aoPassar: setEmFoco,
  };

  // A FICHA É MONTADA UMA VEZ e entregue à aba ativa: ela é o detalhe do
  // município escolhido, que é do recorte e não da aba. Abrir pelo mapa
  // (Mercado) ou pela tabela (Território) tem de dar na mesma ficha.
  const ficha =
    escolhido && indicadores ? (
      <DetalheDoMunicipio
        municipio={escolhido}
        regras={indicadores.regras}
        culturasNoEstado={indicadores.culturasNoEstado}
        aoFechar={() => escolherMunicipio(null)}
      />
    ) : null;

  return (
    <>
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Indicadores Geográficos da ADR</h1>
          <p className="page-subtitle">
            Área de atuação da Tracbel Agro em São Paulo, município a município: cobertura de carteira, vendas e potencial
            por área. Dado do banco do CRM e do IBGE — nenhum número desta tela é ilustrativo.
          </p>
        </div>
        <SeloProcedencia procedencia={painel.procedencia} />
      </div>

      <CartaoDeAlcance
        visao={filtros.visao}
        empresa={contexto.empresa}
        podeVerEmpresaInteira={painel.dados?.podeVerEmpresaInteira}
      />

      <FiltrosDosIndicadores
        filtros={filtros}
        aoMudarFiltros={setFiltros}
        lojasConhecidas={lojasConhecidas}
        regra={regra}
        indicadores={indicadores}
        respondeu={painel.dados !== null}
        podeVerEmpresaInteira={painel.dados?.podeVerEmpresaInteira ?? false}
      />

      {painel.dados && <ComoLerEstesNumeros classificacoes={painel.dados.classificacoes} />}

      <ChipDoMunicipio nome={escolhido?.nome ?? null} aoLimpar={() => escolherMunicipio(null)} />

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}
      {erroDaMalha && <BlocoErro erro={erroDaMalha} />}

      {territorioNaoCarregado && indicadores && (
        <AvisoDeTerritorioSemCarga
          vendasForaDoMapa={vendasForaDoMapa}
          vendasSemCodigoIbge={semCodigoIbge?.vendas.valorLiquido ?? null}
        />
      )}

      <AbasDaTela abas={ABAS} ativa={aba} aoTrocar={setAba} rotulo="Leituras do recorte">
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
            mostrarOsMapas={indicadores !== null && desenho !== null && !territorioNaoCarregado}
            ficha={ficha}
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
            ficha={ficha}
          />
        )}
      </AbasDaTela>
    </>
  );
}
