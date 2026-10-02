/**
 * Dimensionamento ADR — o tamanho do território da Tracbel dentro de São Paulo, o perfil de cada loja, a carteira que
 * cobre os municípios e a matriz município a município (issue 259, a aba "Dimensionamento ADR" do protótipo da pasta 360).
 *
 * O DESENHO É O DAS TELAS DE MERCADO (Diagnóstico e Demanda): filtros na linha, os quatro números do topo, e três seções na
 * ordem do protótipo —
 *   1. o território: a fatia da Tracbel no estado, o peso da loja, o mapa de São Paulo, as culturas e as lojas;
 *   2. a carteira: os clientes por classe e por faixa de dias, os municípios sem cobertura e cada vendedor;
 *   3. município a município: a matriz, paginada e exportável.
 *
 * TUDO É DA PAM DO IBGE, dos dois lados: o recorte é a soma dos municípios, e São Paulo é o total que o IBGE publica para o
 * estado. O que o protótipo tem e esta tela não traz (o peso 0–100 da filial e a área responsável) está em "Limitações
 * dos dados", com o motivo.
 */

import { Coins, Gauge, LineChart, MapPinned, RefreshCw, Ruler, Sprout, TrendingUp, Wheat, type LucideIcon } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CarteiraDoRecorte } from '../componentes/dimensionamento/CarteiraDoRecorte';
import { CarteiraPorVendedor } from '../componentes/dimensionamento/CarteiraPorVendedor';
import {
  cabecalhoDaMatriz,
  contraOAnterior,
  hectares,
  linhasDaMatriz,
  n,
  pct,
  reais,
  reaisDeMil,
  toneladas,
  variacao,
} from '../componentes/dimensionamento/dimensionamento';
import { FiltrosDoDimensionamentoDaAdr } from '../componentes/dimensionamento/FiltrosDoDimensionamento';
import { MapaEstrategico } from '../componentes/dimensionamento/MapaEstrategico';
import { MatrizMunicipal } from '../componentes/dimensionamento/MatrizMunicipal';
import { MunicipiosPrioritarios } from '../componentes/dimensionamento/MunicipiosPrioritarios';
import { ParticipacaoPorCultura } from '../componentes/dimensionamento/ParticipacaoPorCultura';
import { PerfilPorLoja } from '../componentes/dimensionamento/PerfilPorLoja';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { CartaoDoMomento, FileiraDeCartoes, PainelDoMomento } from '../componentes/mercado/momento/pecas';
import type { PoligonoProjetado } from '../componentes/territorio/MapaDeMunicipios';
import { caminhoSvg, enquadrar, type ColecaoMunicipal } from '../componentes/territorio/projecao';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterDimensionamentoDaAdr } from '../dados/api/mercado';
import { carregarMalhaDeSaoPaulo } from '../dados/api/territorio';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { DimensionamentoDaAdr as Dimensionamento, Fatia, FiltrosDoDimensionamento } from '../tipos/mercado';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/diagnostico.css';
import '../estilos/demanda.css';
import '../estilos/dimensionamento.css';

/** A largura do desenho do mapa do estado, em unidades do SVG — o SVG escala para a largura do painel. */
const LARGURA_DO_MAPA = 900;

export function DimensionamentoAdr() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDoDimensionamento>({});
  const [busca, setBusca] = useState('');
  const mudar = (parcial: Partial<FiltrosDoDimensionamento>) => setFiltros((f) => ({ ...f, ...parcial }));

  const leitura = useRecurso(
    (sinal) => obterDimensionamentoDaAdr(contexto, filtros, sinal),
    [
      contexto.empresa,
      contexto.usuario,
      filtros.anoBase,
      filtros.cultura,
      filtros.regiao,
      filtros.lojaCodigo,
      filtros.visao,
      filtros.responsavel,
      filtros.classe,
      filtros.usina,
    ],
  );
  const dados = leitura.dados;
  const totais = dados?.totais;

  const [malha, setMalha] = useState<ColecaoMunicipal | null>(null);
  useEffect(() => {
    const controlador = new AbortController();
    carregarMalhaDeSaoPaulo(controlador.signal)
      .then(setMalha)
      // SEM MALHA, O MAPA DIZ QUE ESTÁ CARREGANDO e o resto da tela funciona.
      .catch(() => undefined);
    return () => controlador.abort();
  }, []);

  // O ESTADO INTEIRO ENQUADRADO, e não a ADR: a pergunta do mapa é que pedaço de São Paulo a Tracbel atende.
  const desenho = useMemo(() => {
    if (!malha) return null;
    const enquadramento = enquadrar(malha, LARGURA_DO_MAPA, 10, () => true);
    const poligonos: PoligonoProjetado[] = malha.features.map((f) => ({
      codigo: Number(f.properties.codarea),
      nome: f.properties.nome,
      caminho: caminhoSvg(enquadramento, f.geometry),
    }));
    return { enquadramento, poligonos };
  }, [malha]);

  const ano = dados?.anoBase ?? null;
  const anterior = dados?.anoAnterior ?? null;
  const daCultura = dados?.cultura ? dados.culturaNome : 'todas as culturas';

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como as outras telas do padrão.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Dimensionamento ADR</h1>
          <p className="page-subtitle">
            O tamanho do território da <strong>Tracbel Agro</strong> dentro de São Paulo, o perfil de cada loja e a carteira que
            cobre os municípios.
          </p>
        </div>
        <p className="dash-atualizado">
          {leitura.procedencia ? <DadosAtualizadosEm procedencia={leitura.procedencia} /> : 'Lendo o dimensionamento…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={leitura.recarregar}
            disabled={leitura.carregando}
            data-carregando={leitura.carregando ? 'true' : 'false'}
            aria-label="Reler o dimensionamento"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <FiltrosDoDimensionamentoDaAdr filtros={filtros} aoMudar={mudar} dados={dados ?? null} />

      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="Área plantada"
          icone={Ruler}
          tom="neutro"
          valor={totais?.area.atual != null ? hectares(totais.area.atual) : null}
          carregando={leitura.carregando && !dados}
          variacao={totais ? contraOAnterior(totais.area, anterior) : null}
          motivoSemDado="A PAM do ano-base não tem área divulgada no recorte."
          sobre={`A soma da área plantada dos municípios do recorte, ${daCultura}, pela PAM do IBGE (tabela 5457).`}
        />
        <CartaoDeDecisao
          rotulo="Quantidade produzida"
          icone={Wheat}
          tom="demanda"
          valor={totais?.quantidade.atual != null ? toneladas(totais.quantidade.atual) : null}
          carregando={leitura.carregando && !dados}
          variacao={totais ? contraOAnterior(totais.quantidade, anterior) : null}
          motivoSemDado="A PAM do ano-base não tem quantidade divulgada em toneladas no recorte."
          sobre="As toneladas produzidas no recorte. Abacaxi e coco-da-baía são publicados em mil frutos e ficam fora da soma."
        />
        <CartaoDeDecisao
          rotulo="Valor da produção"
          icone={Coins}
          tom="mercado"
          valor={totais?.valor.atual != null ? reaisDeMil(totais.valor.atual) : null}
          carregando={leitura.carregando && !dados}
          variacao={totais ? contraOAnterior(totais.valor, anterior) : null}
          motivoSemDado="A PAM do ano-base não tem valor divulgado no recorte."
          sobre="O valor da produção agrícola dos municípios do recorte, como o IBGE publica (em mil reais)."
        />
        <CartaoDeDecisao
          rotulo="Densidade econômica"
          icone={Gauge}
          tom="captura"
          valor={totais?.densidade.atual != null ? `${reais(Math.round(totais.densidade.atual))}/ha` : null}
          carregando={leitura.carregando && !dados}
          variacao={totais ? contraOAnterior(totais.densidade, anterior) : null}
          motivoSemDado="Sem área e valor no recorte não há densidade."
          sobre="O valor da produção ÷ a área plantada: quanto cada hectare do recorte produz em reais."
        />
      </div>

      {leitura.carregando && !dados && <BlocoCarregando oQue="o dimensionamento" />}

      {dados && (
        <>
          <section className="dash-secao" data-bloco="secao-territorio">
            <TituloDaSecao
              titulo="O território"
              subtitulo={`${dados.recorte} · ${ano ?? 'sem PAM'}${anterior ? ` (vs ${anterior})` : ''} · ${daCultura}`}
              metodologia="A PAM do IBGE (tabela 5457) dos dois lados: o recorte é a soma dos municípios, e São Paulo é o total que o IBGE publica para o estado. A cultura de cada produto vem do catálogo do CRM; o resto da lavoura entra em Outras."
            />
            <PainelDoMomento
              titulo={`Tracbel × Estado de São Paulo${ano ? ` — ${ano}` : ''}`}
              dica="Que pedaço da produção paulista está no recorte: a parte é a soma dos municípios, o todo é o total publicado para o estado. A cor da fatia é só endereço — nada aqui quer dizer bom ou ruim."
              direita={<Limitacoes dados={dados} />}
              data-bloco="tracbel-no-estado"
            >
              <FileiraDeCartoes>
                <CartaoDaFatia rotulo="Área plantada" icone={Ruler} tom="verde" fatia={dados.tracbelNoEstado.area} formatar={hectares} />
                <CartaoDaFatia rotulo="Quantidade produzida" icone={Wheat} tom="azul" fatia={dados.tracbelNoEstado.quantidade} formatar={toneladas} />
                <CartaoDaFatia rotulo="Valor da produção" icone={Coins} tom="laranja" fatia={dados.tracbelNoEstado.valor} formatar={reaisDeMil} />
                <CartaoDoMomento
                  icone={MapPinned}
                  tom="neutro"
                  rotulo="Municípios no recorte"
                  valor={n(dados.municipiosNoRecorte)}
                  unidade={`de ${n(dados.mapa.length)}`}
                  apoio={`${n(totais!.municipiosComProducao)} com produção em ${ano ?? 'o ano-base'}`}
                  dica="Os municípios da ADR que entraram no recorte, contra os de São Paulo no mapa."
                />
              </FileiraDeCartoes>
              {dados.cultura && (
                <FileiraDeCartoes>
                  <CartaoDoMomento
                    icone={Sprout}
                    tom="verde"
                    rotulo="Produtividade média"
                    valor={totais!.produtividade === null ? null : n(totais!.produtividade, 2)}
                    unidade="t/ha"
                    apoio={`${dados.culturaNome} · quantidade ÷ área colhida`}
                    motivoSemDado="Produtividade é de uma cultura do catálogo: em Outras, os produtos não se comparam."
                    dica="A quantidade produzida ÷ a área COLHIDA, como o rendimento médio do IBGE — e não a plantada, que inclui lavoura perene que ainda não produz."
                  />
                  <CartaoDoMomento
                    icone={TrendingUp}
                    tom="azul"
                    rotulo="Rentabilidade R12"
                    valor={dados.momento?.indice != null ? variacao((dados.momento.indice - 1) * 100) : null}
                    apoio={dados.momento?.faixa ? `momento ${dados.momento.faixa.toLowerCase()}` : 'o preço de 12 meses contra os 12 anteriores'}
                    motivoSemDado="A cultura não tem série de preço com 24 meses no CRM."
                    dica="O preço médio dos últimos 12 meses contra o dos 12 anteriores — o mesmo momento de preço dos Indicadores (CONAB, e o ATR da Socicana na cana)."
                  />
                </FileiraDeCartoes>
              )}
            </PainelDoMomento>

            {dados.representatividade && (
              <PainelDoMomento
                titulo={`Representatividade da loja ${dados.representatividade.loja} na Tracbel`}
                dica="O peso da loja na ADR inteira, em todas as culturas: a parte são os municípios dela, o todo são os 203 municípios da ADR."
                data-bloco="representatividade"
              >
                <FileiraDeCartoes>
                  <CartaoDaFatia rotulo="Área plantada" icone={Ruler} tom="verde" fatia={dados.representatividade.area} formatar={hectares} naAdr />
                  <CartaoDaFatia rotulo="Quantidade produzida" icone={Wheat} tom="azul" fatia={dados.representatividade.quantidade} formatar={toneladas} naAdr />
                  <CartaoDaFatia rotulo="Valor da produção" icone={Coins} tom="laranja" fatia={dados.representatividade.valor} formatar={reaisDeMil} naAdr />
                </FileiraDeCartoes>
              </PainelDoMomento>
            )}

            <PainelDoMomento
              titulo="Mapa estratégico — Tracbel no Estado de São Paulo"
              dica="Verde é o recorte; cinza, os demais municípios de São Paulo. A intensidade segue a variável escolhida, em faixas pelos quantis de cada grupo. Hachurado é sem dado divulgado — sigilo do IBGE não é zero."
              data-bloco="mapa"
            >
              <MapaEstrategico desenho={desenho} municipios={dados.mapa} />
            </PainelDoMomento>

            <PainelDoMomento
              titulo={`Participação por cultura — Tracbel × Estado${ano ? ` (${ano})` : ''}`}
              dica="Área em hectares, produção em toneladas e valor em mil reais, recorte contra estado. Em Outras, a quantidade fica sem número quando só há produtos publicados em mil frutos."
              data-bloco="por-cultura"
            >
              <ParticipacaoPorCultura dados={dados} />
            </PainelDoMomento>

            <PainelDoMomento
              titulo="Perfil comercial por loja"
              subtitulo={`tamanho do território · densidade econômica · tecnificação · cultura dominante — ${daCultura}`}
              dica="A tecnificação é a produtividade da loja em % da média do recorte, numa cultura só: escolha uma cultura para vê-la. A cultura dominante é a de maior valor no ano-base."
              data-bloco="por-loja"
            >
              <PerfilPorLoja dados={dados} />
            </PainelDoMomento>
          </section>

          <section className="dash-secao" data-bloco="secao-carteira">
            <TituloDaSecao
              titulo="A carteira"
              subtitulo="Quem cobre o território: clientes por classe, por faixa de dias desde o último contato, e cada vendedor."
              metodologia="Clientes com vínculo em carteira comercial, no município do endereço principal, pela classe da curva ABC do faturamento. As faixas de dias são acumuladas e contam do último contato registrado."
            />
            <PainelDoMomento
              titulo="Carteira de clientes"
              dica="Clientes distintos ao alcance da filial do cabeçalho. Com região, loja ou usina escolhida, só os clientes dos municípios do recorte entram — e não existe fora da região."
              data-bloco="carteira"
            >
              <CarteiraDoRecorte dados={dados} />
            </PainelDoMomento>

            <PainelDoMomento
              titulo="Municípios — alto potencial e baixa cobertura"
              subtitulo="ordenado pela prioridade = valor da produção × lacuna de cobertura · municípios com três clientes ou mais"
              dica="A lacuna é a fatia dos clientes sem contato nos últimos 120 dias; a cobertura da barra é a de 90 dias, como o protótipo. A prioridade é o valor de produção que está sem cobertura."
              data-bloco="prioritarios"
            >
              <MunicipiosPrioritarios municipios={dados.prioritarios} />
            </PainelDoMomento>

            <PainelDoMomento
              titulo="Carteira por vendedor"
              dica="Cada vendedor com os clientes das carteiras dele, contados pelos vínculos DELE: o mesmo cliente em duas carteiras aparece nas duas linhas, e a soma da coluna pode passar do total da carteira."
              data-bloco="por-vendedor"
            >
              <CarteiraPorVendedor vendedores={dados.porVendedor} />
            </PainelDoMomento>
          </section>

          <section className="dash-secao" data-bloco="secao-matriz">
            <TituloDaSecao
              titulo="Município a município"
              subtitulo={`${n(dados.matriz.length)} municípios · ${daCultura} · ${ano ?? ''}${anterior ? ` vs ${anterior}` : ''}`}
              metodologia="Cada município do recorte com a produção do ano-base e do anterior, a participação no recorte e em São Paulo, e os clientes da carteira. A matriz é paginada e exportável."
            />
            <PainelDoMomento
              titulo="Matriz municipal"
              icone={<LineChart size={18} strokeWidth={2} aria-hidden="true" />}
              direita={
                <div className="dem-acoes-da-matriz">
                  <input
                    type="search"
                    className="dem-busca"
                    value={busca}
                    placeholder="Buscar município"
                    aria-label="Buscar município na matriz"
                    onChange={(e) => setBusca(e.target.value)}
                  />
                  <button
                    type="button"
                    className="btn btn-secondary btn-sm"
                    onClick={() => baixarCsv(`dimensionamento-adr-${ano ?? 'pam'}-${carimboDeData()}`, cabecalhoDaMatriz(dados), linhasDaMatriz(dados))}
                    disabled={dados.matriz.length === 0}
                  >
                    Exportar CSV
                  </button>
                </div>
              }
              data-bloco="matriz"
            >
              <MatrizMunicipal dados={dados} busca={busca} />
            </PainelDoMomento>
          </section>
        </>
      )}
    </PaginaDoPainel>
  );
}

/** Uma fatia como cartão: o percentual em destaque, e "parte de todo" embaixo. */
function CartaoDaFatia({
  rotulo,
  icone,
  tom,
  fatia,
  formatar,
  naAdr = false,
}: {
  rotulo: string;
  icone: LucideIcon;
  tom: 'verde' | 'azul' | 'laranja';
  fatia: Fatia;
  formatar: (v: number | null) => string;
  naAdr?: boolean;
}) {
  return (
    <CartaoDoMomento
      icone={icone}
      tom={tom}
      rotulo={`${rotulo} — ${naAdr ? 'na ADR' : 'no Estado'}`}
      valor={fatia.percentual === null ? null : pct(fatia.percentual)}
      apoio={`${formatar(fatia.parte)} de ${formatar(fatia.todo)}`}
      motivoSemDado={
        fatia.todo === null
          ? naAdr
            ? 'A ADR não tem este número divulgado no ano-base.'
            : 'O total de São Paulo deste ano não foi carregado — a fatia não vira a soma dos municípios com outro nome.'
          : 'O recorte não tem este número divulgado no ano-base.'
      }
    />
  );
}

function Limitacoes({ dados }: { dados: Dimensionamento }) {
  if (dados.lacunas.length === 0) return null;
  return (
    <span className="diag-limitacoes">
      Limitações dos dados
      <InfoTooltip
        rotulo="Limitações dos dados do dimensionamento"
        texto={<MetricasSemDado metricas={dados.lacunas} titulo="O que esta leitura não afirma" naDica />}
      />
    </span>
  );
}
