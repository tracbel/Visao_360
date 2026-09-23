/**
 * Indicadores Geográficos da ADR — os quatro mapas do pedido do gerente comercial
 * (documento 32, seção 8; documento 36), com dado do banco do CRM e do IBGE.
 *
 * ESTA É A CASCA. Ela busca, calcula os totais e compõe os blocos; cada bloco
 * mora no próprio componente, em `componentes/territorio/` (issue 170, parte A —
 * fase T0 do documento 50). A ordem dos blocos aqui é a ordem da tela, e é o que
 * `IndicadoresGeograficos.teste.tsx` prende.
 *
 * O QUE ESTA TELA NÃO FAZ, e diz na própria tela:
 * - não filtra por SAM/KAM/Varejo, tipo de produto nem modelo — não há dado;
 * - não mostra potencial de cliente nem de não cliente — não há área por
 *   propriedade; o potencial é o do município inteiro, por uma regra ainda a
 *   confirmar;
 * - não mostra valor em reais no potencial — não há preço de máquina confirmado;
 *   o preço das CULTURAS está no painel do fim (issue 66);
 * - não oferece FYTD — o calendário fiscal não foi confirmado.
 *
 * OS NÚMEROS DA MAQUETE NÃO ESTÃO AQUI. Ela era ilustrativa; nenhum valor desta
 * tela é simulado.
 */

import { useEffect, useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores } from '../componentes/cadastro/Indicadores';
import { SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { AvisoDeTerritorioSemCarga } from '../componentes/territorio/AvisoDeTerritorioSemCarga';
import { CartaoDeAlcance } from '../componentes/territorio/CartaoDeAlcance';
import { ComoLerEstesNumeros } from '../componentes/territorio/ComoLerEstesNumeros';
import { DetalheDoMunicipio } from '../componentes/territorio/DetalheDoMunicipio';
import { FiltrosDosIndicadores } from '../componentes/territorio/FiltrosDosIndicadores';
import { GradeDeMapas } from '../componentes/territorio/GradeDeMapas';
import { LARGURA_DO_DESENHO } from '../componentes/territorio/indicadoresDaAdr';
import { kpisDaCarteira, kpisDoMercado, type ContextoDosKpis } from '../componentes/territorio/kpisDosIndicadores';
import type { LigacaoDoMapa } from '../componentes/territorio/mapas/CartaoDeMapa';
import { PainelDeCredito } from '../componentes/territorio/PainelDeCredito';
import { PainelDeCustos } from '../componentes/territorio/PainelDeCustos';
import { PainelDePrecos } from '../componentes/territorio/PainelDePrecos';
import { SecaoDoMercadoDaRegiao } from '../componentes/territorio/SecaoDoMercadoDaRegiao';
import { TabelaDeMunicipios } from '../componentes/territorio/TabelaDeMunicipios';
import { calcularFatiaNoEstado, calcularTotais } from '../componentes/territorio/totaisDaAdr';
import type { PoligonoProjetado } from '../componentes/territorio/MapaDeMunicipios';
import { caminhoSvg, enquadrar, type ColecaoMunicipal } from '../componentes/territorio/projecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { carregarMalhaDeSaoPaulo, obterIndicadoresTerritoriais } from '../dados/api/territorio';
import { useRecurso } from '../dados/api/useRecurso';
import type { ClassificacaoDeIndicador, FiltrosTerritoriais } from '../tipos/territorio';
import '../estilos/territorio.css';

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
  const [selecionado, setSelecionado] = useState<number | null>(null);
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
   * ou a loja; sem filtro, é o mapa inteiro, ADR e fora dela. Os cartões do topo continuam sendo da
   * ADR, e por isso o painel do recorte diz quantos municípios entraram nele.
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
  // município veio marcado como ADR: a carga do território (catálogo IBGE, área de atuação, responsáveis,
  // área plantada) não rodou neste banco.
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
    aoSelecionar: setSelecionado,
    emFoco,
    aoPassar: setEmFoco,
  };

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

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}
      {erroDaMalha && <BlocoErro erro={erroDaMalha} />}

      {territorioNaoCarregado && indicadores && (
        <AvisoDeTerritorioSemCarga
          vendasForaDoMapa={vendasForaDoMapa}
          vendasSemCodigoIbge={semCodigoIbge?.vendas.valorLiquido ?? null}
        />
      )}

      <PainelDeIndicadores indicadores={kpisDaCarteira(contextoDosKpis)} carregando={painel.carregando} />

      <SecaoDoMercadoDaRegiao />
      <PainelDeIndicadores indicadores={kpisDoMercado(contextoDosKpis)} carregando={painel.carregando} />

      {(painel.carregando || !desenho) && !painel.erro && !erroDaMalha && <BlocoCarregando oQue="os mapas da ADR" />}

      {indicadores && ligacao && !territorioNaoCarregado && (
        <GradeDeMapas
          ligacao={ligacao}
          indicadores={indicadores}
          totais={totais}
          coberturaDaAdr={coberturaDaAdr}
          regra={regra}
          recorte={recorte}
          semFiltro={semFiltro}
          contexto={contexto}
          anoDoCenso={estruturaDaAdr.anoDoCenso}
          anoDoRebanho={estruturaDaAdr.anoDoRebanho}
          classificacaoDe={classificacao}
        />
      )}

      {escolhido && indicadores && (
        <DetalheDoMunicipio
          municipio={escolhido}
          regras={indicadores.regras}
          culturasNoEstado={indicadores.culturasNoEstado}
          aoFechar={() => setSelecionado(null)}
        />
      )}

      <MetricasSemDado metricas={painel.dados?.metricasSemDado} titulo="O que estes mapas não dizem" />

      {indicadores && (
        <TabelaDeMunicipios
          municipios={municipios}
          daAdr={ordenados}
          foraDoMapa={indicadores.foraDoMapa}
          totais={totais}
          selecionado={selecionado}
          aoSelecionar={setSelecionado}
          territorioNaoCarregado={territorioNaoCarregado}
          semFiltro={semFiltro}
        />
      )}

      <PainelDePrecos />
      <PainelDeCustos />
      <PainelDeCredito />
    </>
  );
}
