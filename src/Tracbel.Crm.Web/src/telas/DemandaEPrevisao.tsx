/**
 * Demanda e Previsão — quantas máquinas a região pede por ano, quanto a Tracbel entregou, e em que mês e em que loja
 * (issue 258, a aba "Análise de Demanda" do protótipo da pasta 360).
 *
 * O DESENHO É O DA MAQUETE DO RICARDO DE 02/10/2026 (`docs/prototipo/capturas-referencia/demanda-previsao-maquete-
 * 2026-10-02.png`), "idêntico", com as decisões dele:
 *   1. os filtros — Período (o ano fiscal, nov→out), Regional / Loja, Loja / Filial, Cultura e Meta / Previsão (a base:
 *      ajustada pelo momento ou estrutural); o tipo de máquina e o mês ficam em "Mais filtros";
 *   2. os quatro números — parque potencial e demanda anual contra o ano anterior da PAM, a entrega realizada no período
 *      (ART, pela data da entrega) contra o mesmo trecho do ano anterior, e as culturas com aumento de área;
 *   3. "A previsão": a demanda prevista mês a mês ao lado da entrega realizada e do atendimento, e a entrega por loja;
 *   4. "De onde vem o número": os parâmetros por cultura e os maiores municípios por potencial.
 *
 * O AJUSTE É O FATOR DE CICLO DO CRM (preço, crédito e percepção), e não as elasticidades do protótipo — é o mesmo número
 * da ficha do município nos Indicadores. O que a maquete não desenha está nas dicas: o "a entregar" pelo share-alvo, a
 * demanda de cada cultura por município, os fatores do momento e as limitações.
 */

import * as Popover from '@radix-ui/react-popover';
import { BarChart3, CalendarDays, Download, Funnel, Leaf, ListFilter, MapPin, Package, RefreshCw, Sheet, Store, Tractor, UserRound } from 'lucide-react';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { MenuDaLinha } from '../componentes/comum/MenuDaLinha';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { MiniGrafico } from '../componentes/mercado/MiniGrafico';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { EntregaPorLoja } from '../componentes/demanda/EntregaPorLoja';
import { MaioresMunicipios } from '../componentes/demanda/MaioresMunicipios';
import { ParametrosPorCultura } from '../componentes/demanda/ParametrosPorCultura';
import { PrevisaoMensal } from '../componentes/demanda/PrevisaoMensal';
import { cabecalhoDoCsv, linhasDoCsv, n, NOME_DO_MES, NOME_DO_MES_POR_EXTENSO, variacaoPercentual } from '../componentes/demanda/demanda';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterDemandaEPrevisao } from '../dados/api/mercado';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { DemandaEPrevisaoDaRegiao, FiltrosDaDemanda } from '../tipos/mercado';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/diagnostico.css';
import '../estilos/demanda.css';

type Base = 'ajustada' | 'estrutural';

/**
 * O FILTRO "META / PREVISÃO" DA MAQUETE: "Todos os cenários" (o padrão, como na imagem) mostra a estrutural no número e a
 * ajustada ao lado; as outras duas escolhem a base de tudo.
 */
type Cenario = 'todos' | Base;

/** "soja, milho e café" — a lista de culturas em texto corrido. */
function emTexto(itens: string[]): string {
  const minusculas = itens.map((i) => i.toLocaleLowerCase('pt-BR'));
  return minusculas.length <= 1 ? (minusculas[0] ?? '') : `${minusculas.slice(0, -1).join(', ')} e ${minusculas[minusculas.length - 1]}`;
}

/** "FY2026 (nov/2025 a out/2026)". */
const nomeDoAno = (ano: number) => `FY${ano} (nov/${ano - 1} a out/${ano})`;

/** A variação de um número sobre outro, em %, com o sinal — nula quando não há base. */
function variacaoSobre(atual: number | null | undefined, anterior: number | null | undefined): number | null {
  return atual != null && anterior != null && anterior > 0 ? (atual / anterior - 1) * 100 : null;
}

export function DemandaEPrevisao() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDaDemanda>({});
  const [mes, setMes] = useState<number | null>(null);
  const [cenario, setCenario] = useState<Cenario>('todos');
  const [lojasConhecidas, setLojasConhecidas] = useState<Map<string, string>>(() => new Map());

  const leitura = useRecurso(
    (sinal) =>
      obterDemandaEPrevisao(contexto, filtros, sinal).then((resposta) => {
        setLojasConhecidas((atual) => {
          const proximo = new Map(atual);
          for (const m of resposta.dados.municipios) if (m.lojaCodigo && m.loja) proximo.set(m.lojaCodigo, m.loja);
          return proximo.size === atual.size ? atual : proximo;
        });
        return resposta;
      }),
    [contexto.empresa, contexto.usuario, filtros.regiao, filtros.lojaCodigo, filtros.categoria, filtros.anoFiscal, filtros.cultura],
  );
  const dados = leitura.dados;
  const totais = dados?.totais;
  const comAjustada = totais?.demandaAjustada != null;
  // A BASE DOS NÚMEROS: a ajustada só quando foi escolhida e o momento está medido; em "Todos os cenários", a estrutural no
  // número e a ajustada ao lado, como a maquete ("3.097,3 unidades · 1.618,9 ajustada pelo momento atual").
  const base: Base = cenario === 'ajustada' && comAjustada ? 'ajustada' : 'estrutural';
  const prevista = (m: { demandaEstrutural: number | null; demandaAjustada: number | null }) =>
    base === 'ajustada' ? (m.demandaAjustada ?? m.demandaEstrutural) : m.demandaEstrutural;
  const todasAsCategorias = dados?.categoria === 'TODAS';
  const mudar = (parcial: Partial<FiltrosDaDemanda>) => setFiltros((f) => ({ ...f, ...parcial }));

  const demandaDoAno = totais ? (base === 'ajustada' ? totais.demandaAjustada : totais.demandaEstrutural) : null;
  const aEntregarDoAno = totais ? (base === 'ajustada' ? (totais.aEntregarAjustada ?? totais.aEntregar) : totais.aEntregar) : null;
  const doMes = mes === null ? null : (dados?.previsaoMensal.find((m) => m.mes === mes) ?? null);
  const previstaDoMes = doMes ? prevista(doMes) : null;
  const entregue = doMes ? doMes.entregues : (totais?.entreguesNoPeriodo ?? null);
  const atendimento = doMes
    ? (entregue !== null && previstaDoMes ? entregue / previstaDoMes : null)
    : entregue !== null && demandaDoAno
      ? entregue / demandaDoAno
      : null;
  const motivoSemEntrega =
    dados?.lacunas.find((l) => l.metrica === 'entregaPorCultura' || l.metrica === 'entregaRealizada')?.motivo ??
    'O ART não trouxe entrega ao alcance desta consulta.';

  const secundariosAtivos = (filtros.categoria && filtros.categoria !== 'TRATOR' ? 1 : 0) + (mes !== null ? 1 : 0);
  const crescidas = totais?.culturasComAumentoDeArea ?? [];

  return (
    <PaginaDoPainel className="dash-pagina-larga dem-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Demanda e Previsão</h1>
          <p className="page-subtitle">
            Estime a demanda de máquinas por região, cultura e período, e priorize municípios e filiais com maior potencial.
          </p>
        </div>
        <p className="dash-atualizado">
          {leitura.procedencia ? <DadosAtualizadosEm procedencia={leitura.procedencia} dicaNoTexto /> : 'Lendo a demanda…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={leitura.recarregar}
            disabled={leitura.carregando}
            data-carregando={leitura.carregando ? 'true' : 'false'}
            aria-label="Reler a demanda"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <Campo icone={CalendarDays} rotulo="Período" bloco="periodo">
            <select
              value={filtros.anoFiscal ?? dados?.anoFiscal ?? ''}
              onChange={(e) => mudar({ anoFiscal: e.target.value ? Number(e.target.value) : undefined })}
            >
              {(dados?.anosFiscais ?? []).map((ano) => (
                // O INTERVALO, COMO A MAQUETE ("Jan/2024 a Dez/2024"): o nome do ano fiscal está na dica da previsão.
                <option key={ano} value={ano} title={nomeDoAno(ano)}>
                  Nov/{ano - 1} a Out/{ano}
                </option>
              ))}
              {!dados && <option value="">Ano fiscal corrente</option>}
            </select>
          </Campo>
          <Campo icone={MapPin} rotulo="Regional / Loja" bloco="regional">
            <select value={filtros.regiao ?? ''} onChange={(e) => mudar({ regiao: e.target.value || undefined })}>
              <option value="">Todas as regionais</option>
              <option value="Norte">Região Norte</option>
              <option value="Noroeste">Região Noroeste</option>
            </select>
          </Campo>
          <Campo icone={Store} rotulo="Loja / Filial" bloco="loja">
            <select value={filtros.lojaCodigo ?? ''} onChange={(e) => mudar({ lojaCodigo: e.target.value || undefined })}>
              <option value="">Todas as lojas</option>
              {[...lojasConhecidas.entries()]
                .sort((a, b) => a[1].localeCompare(b[1], 'pt-BR'))
                .map(([codigo, nome]) => (
                  <option key={codigo} value={codigo}>
                    {nome}
                  </option>
                ))}
            </select>
          </Campo>
          <Campo icone={ListFilter} rotulo="Cultura" bloco="cultura">
            <select value={filtros.cultura ?? ''} onChange={(e) => mudar({ cultura: e.target.value || undefined })}>
              <option value="">Todas as culturas</option>
              {(dados?.culturasDoFiltro ?? []).map((c) => (
                <option key={c.codigo} value={c.codigo}>
                  {c.nome}
                </option>
              ))}
            </select>
          </Campo>
          <Campo icone={UserRound} rotulo="Meta / Previsão" bloco="base">
            <select value={cenario === 'ajustada' && !comAjustada ? 'todos' : cenario} onChange={(e) => setCenario(e.target.value as Cenario)}>
              <option value="todos">Todos os cenários</option>
              <option value="ajustada" disabled={!comAjustada}>
                {comAjustada ? 'Ajustada pelo momento' : 'Ajustada pelo momento (sem fator de ciclo)'}
              </option>
              <option value="estrutural">Estrutural (renovação do parque)</option>
            </select>
          </Campo>

          <div className="dash-filtros-acao">
            <Popover.Root>
              <Popover.Trigger asChild>
                <button type="button" className="dash-mais-filtros" data-bloco="mais-filtros">
                  <Funnel size={15} strokeWidth={2} aria-hidden="true" />
                  Mais filtros
                  {secundariosAtivos > 0 && <span className="dash-mais-filtros-selo">{secundariosAtivos}</span>}
                </button>
              </Popover.Trigger>
              <Popover.Portal>
                <Popover.Content className="dash-popover" sideOffset={6} collisionPadding={16} align="end">
                  <div className="dash-popover-titulo">Mais filtros</div>
                  <label className="dash-filtro">
                    <span className="dash-filtro-rotulo">
                      <Tractor size={14} strokeWidth={2} aria-hidden="true" /> Tipo de máquina
                    </span>
                    <select value={filtros.categoria ?? dados?.categoria ?? 'TRATOR'} onChange={(e) => mudar({ categoria: e.target.value })}>
                      {(dados?.categorias.length ? dados.categorias : [{ codigo: 'TRATOR', nome: 'Trator', ordem: 1 }]).map((c) => (
                        <option key={c.codigo} value={c.codigo}>
                          {c.nome}
                        </option>
                      ))}
                      <option value="TODAS">Todas as categorias</option>
                    </select>
                  </label>
                  <label className="dash-filtro">
                    <span className="dash-filtro-rotulo">
                      <CalendarDays size={14} strokeWidth={2} aria-hidden="true" /> Mês da previsão
                    </span>
                    <select value={mes ?? ''} onChange={(e) => setMes(e.target.value === '' ? null : Number(e.target.value))}>
                      <option value="">O ano todo</option>
                      {(dados?.previsaoMensal ?? []).map((m) => (
                        <option key={m.mes} value={m.mes}>
                          {NOME_DO_MES_POR_EXTENSO[m.mes]}
                        </option>
                      ))}
                    </select>
                  </label>
                </Popover.Content>
              </Popover.Portal>
            </Popover.Root>
          </div>
        </div>
      </div>

      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="Parque potencial"
          icone={Tractor}
          tom="demanda"
          valor={totais?.parque != null ? n(totais.parque, 0) : null}
          carregando={leitura.carregando && !dados}
          selo={<Variacao valor={variacaoSobre(totais?.parque, totais?.parqueAnoAnterior)} />}
          variacao={dados ? <span>{todasAsCategorias ? 'Trator + equipamentos' : dados.categoriaNome} na área plantada comparável</span> : null}
          grafico={
            dados && (
              <MiniGrafico
                tipo="barras"
                cor="#16a34a"
                valores={dados.porCultura.map((c) => c.parque ?? 0).sort((a, b) => a - b)}
                rotulo={`Parque potencial por cultura: ${dados.porCultura.map((c) => `${c.cultura} ${n(c.parque ?? 0, 0)}`).join(', ')}`}
              />
            )
          }
          motivoSemDado="Nenhum município do recorte tem regra de potencial para esta categoria."
          sobre={
            <>
              A área plantada do recorte (PAM do IBGE{dados?.anoDaAreaPlantada ? `, ${dados.anoDaAreaPlantada}` : ''}) ÷ os hectares por máquina da
              regra de cada cultura: o parque que a área comporta. A variação é contra a mesma conta com a área do ano anterior da PAM
              {totais?.parqueAnoAnterior != null && ` (${n(totais.parqueAnoAnterior, 0)} em ${dados?.anoDaAreaAnterior ?? 'o ano anterior'})`} — a área
              comparável. O gráfico é o parque de cada cultura.
            </>
          }
        />
        <CartaoDeDecisao
          rotulo="Demanda anual"
          icone={BarChart3}
          tom="demanda"
          valor={demandaDoAno != null ? n(demandaDoAno) : null}
          carregando={leitura.carregando && !dados}
          unidade="unidades"
          variacao={
            totais?.demandaEstrutural != null ? (
              <>
                <span>
                  {(() => {
                    const v = variacaoSobre(totais.demandaEstrutural, totais.demandaEstruturalAnoAnterior);
                    return v === null ? (
                      'sem o ano anterior na PAM'
                    ) : (
                      <>
                        <Variacao valor={v} naLinha /> em relação ao ano anterior
                      </>
                    );
                  })()}
                </span>
                <span>
                  {base === 'ajustada'
                    ? `${n(totais.demandaEstrutural)} estrutural, sem o momento`
                    : comAjustada
                      ? `${n(totais.demandaAjustada!)} ajustada pelo momento atual`
                      : 'sem o fator de ciclo medido'}
                </span>
              </>
            ) : null
          }
          grafico={
            dados && (
              <MiniGrafico
                tipo="linha"
                cor="#16a34a"
                valores={dados.previsaoMensal.map((m) => prevista(m) ?? 0)}
                rotulo={`A demanda prevista mês a mês, de ${NOME_DO_MES_POR_EXTENSO[dados.previsaoMensal[0]?.mes ?? 11]} a ${NOME_DO_MES_POR_EXTENSO[dados.previsaoMensal[dados.previsaoMensal.length - 1]?.mes ?? 10]}`}
              />
            )
          }
          motivoSemDado="Sem ciclo de renovação nas regras da categoria: o parque sai, a demanda anual não."
          sobre="O parque ÷ os anos de renovação: as máquinas que a região pede por ano — 100% do mercado. A ajustada é a mesma demanda pelo fator de ciclo (preço da cultura, crédito do município e percepção). A variação é da estrutural contra a do ano anterior da PAM: a área mudou, e só ela. O gráfico é a previsão mês a mês."
        />
        <CartaoDeDecisao
          rotulo={doMes ? `Entrega em ${NOME_DO_MES_POR_EXTENSO[doMes.mes]}` : 'Entrega no período'}
          icone={Package}
          tom="captura"
          valor={entregue !== null ? n(entregue, 0) : null}
          carregando={leitura.carregando && !dados}
          unidade="unidades"
          variacao={
            entregue !== null ? (
              <>
                <span>{atendimento !== null ? `${n(atendimento * 100, 0)}% da previsão ${doMes ? 'do mês' : 'anual'}` : 'sem previsão para comparar'}</span>
                {!doMes && (
                  <span>
                    {(() => {
                      const v = variacaoSobre(totais?.entreguesNoPeriodo, totais?.entreguesNoPeriodoAnterior);
                      return v === null ? (
                        'sem entrega no mesmo período do ano anterior'
                      ) : (
                        <>
                          <Variacao valor={v} naLinha /> vs. mesmo período ano anterior
                        </>
                      );
                    })()}
                  </span>
                )}
              </>
            ) : null
          }
          grafico={
            dados && (
              <MiniGrafico
                tipo="barras"
                cor="#f07a17"
                valores={dados.previsaoMensal.filter((m) => m.entregues !== null).map((m) => m.entregues!)}
                rotulo={`As máquinas entregues mês a mês: ${dados.previsaoMensal
                  .filter((m) => m.entregues !== null)
                  .map((m) => `${NOME_DO_MES[m.mes]} ${n(m.entregues!, 0)}`)
                  .join(', ')}`}
              />
            )
          }
          motivoSemDado={motivoSemEntrega}
          sobre={
            <>
              As máquinas entregues {doMes ? 'no mês' : 'no ano fiscal até hoje'} pelo ART, pela data da ENTREGA, dos compradores dos
              municípios do recorte e das categorias da demanda. O atendimento é a entrega ÷ a demanda prevista.
              {aEntregarDoAno !== null && dados?.shareAlvo !== null && dados?.shareAlvo !== undefined && (
                <> A meta do ano pelo share-alvo de {n(dados.shareAlvo, 0)}% é {n(aEntregarDoAno)} máquinas.</>
              )}
            </>
          }
        />
        <CartaoDeDecisao
          rotulo="Culturas com espaço"
          icone={Leaf}
          tom="mercado"
          valor={totais ? n(crescidas.length, 0) : null}
          carregando={leitura.carregando && !dados}
          variacao={
            totais
              ? crescidas.length > 0
                ? `Aumento em área, puxado por ${emTexto(crescidas)}`
                : 'Nenhuma cultura com aumento em área sobre o ano anterior'
              : null
          }
          grafico={
            dados && (
              <MiniGrafico
                tipo="barras"
                cor="#2f6fe0"
                valores={dados.porCultura.map((c) => c.areaUtilHectares ?? 0).sort((a, b) => a - b)}
                rotulo={`Área plantada por cultura (ha): ${dados.porCultura.map((c) => `${c.cultura} ${n(c.areaUtilHectares ?? 0, 0)}`).join(', ')}`}
              />
            )
          }
          motivoSemDado={leitura.carregando ? undefined : 'A leitura da demanda não respondeu.'}
          sobre={`As culturas do recorte cuja área útil cresceu sobre o ano anterior da PAM — onde há espaço para mais máquina —, entre as ${totais ? `${n(totais.culturasComRegra.length, 0)} ` : ''}que têm regra de potencial nesta categoria${totais ? ` (${totais.culturasComRegra.join(', ')})` : ''}. O gráfico é a área plantada de cada cultura.`}
        />
      </div>

      {leitura.carregando && !dados && <BlocoCarregando oQue="a demanda" />}

      {dados && (
        <>
          <section className="dash-secao dem-cartao" data-bloco="secao-previsao">
            <TituloDaSecao
              titulo="A previsão"
              subtitulo="Demanda estimada de máquinas e equipamentos para o período selecionado, considerando o parque potencial, renovação e o cenário de mercado."
              metodologia={<Limitacoes dados={dados} />}
            />
            <div className="dem-dupla dem-dupla-previsao">
              <PainelDoMomento
                titulo="Previsão mensal"
                dica={
                  'A demanda do ano distribuída pela sazonalidade vigente, de novembro a outubro — o ano fiscal da Tracbel. A barra ' +
                  'escura é a demanda prevista do mês pela base escolhida; a clara, as máquinas entregues no mês pelo ART; a linha, o ' +
                  'atendimento (entregue ÷ previsto). Clique num mês para ver a entrega dele no cartão e no quadro das lojas.'
                }
                direita={<SeletorDeUnidade />}
                data-bloco="previsao-mensal"
              >
                <ul className="dem-legenda" aria-label="Legenda da previsão mensal">
                  <li>
                    <i className="dem-legenda-prevista" aria-hidden="true" /> Demanda prevista (un.)
                  </li>
                  <li>
                    <i className="dem-legenda-realizada" aria-hidden="true" /> Entrega realizada (un.)
                  </li>
                  <li>
                    <svg className="dem-legenda-linha" width="26" height="10" viewBox="0 0 26 10" aria-hidden="true">
                      <line x1="1" x2="25" y1="5" y2="5" />
                      <circle cx="13" cy="5" r="3" />
                    </svg>
                    % de atendimento
                  </li>
                </ul>
                <PrevisaoMensal meses={dados.previsaoMensal} base={base} mesEscolhido={mes} aoEscolherMes={setMes} />
              </PainelDoMomento>

              <PainelDoMomento
                titulo="Entrega por loja x mês"
                dica="O que cada loja tem de entregar em cada mês: a demanda dos municípios dela × o share-alvo, distribuída pela sazonalidade (pela ajustada quando o momento está medido). A intensidade da célula reforça o número, que vem sempre escrito."
                direita={
                  <span className="dem-acoes">
                    <SeletorDeUnidade />
                    <MenuDaLinha
                      rotulo="Mais ações da entrega por loja"
                      itens={[
                        { rotulo: 'Ver o Diagnóstico Comercial', para: '/mercado/diagnostico' },
                        { rotulo: 'Ver os Indicadores Geográficos', para: '/relatorios/territorio' },
                      ]}
                    />
                  </span>
                }
                data-bloco="entrega-por-loja"
              >
                <EntregaPorLoja lojas={dados.porLoja} meses={dados.previsaoMensal.map((m) => m.mes)} mesEscolhido={mes} />
              </PainelDoMomento>
            </div>
          </section>

          <section className="dash-secao dem-cartao" data-bloco="secao-origem">
            <TituloDaSecao
              titulo="De onde vem o número"
              subtitulo="Detalhamento dos parâmetros utilizados para estimar a demanda e os municípios com maior potencial."
              metodologia="Os hectares por máquina e os anos de renovação vêm das regras do potencial (Configurações › Potencial de mercado); a área plantada, da PAM do IBGE. A renovação anual é 1 ÷ os anos de renovação. O CSV leva todos os municípios, com a demanda de cada cultura e os fatores do momento."
            />
            <div className="dem-dupla dem-dupla-origem">
              <PainelDoMomento
                titulo="Parâmetros de demanda por cultura"
                dica={`O parque é a área plantada (PAM do IBGE${dados.anoDaAreaPlantada ? `, ${dados.anoDaAreaPlantada}` : ''}) ÷ os hectares por máquina da regra; a demanda é o parque ÷ os anos de renovação. A ajustada é a mesma demanda pelo fator de ciclo. A dica de cada célula traz o ano anterior.`}
                data-bloco="por-cultura"
              >
                <ParametrosPorCultura culturas={dados.porCultura} comAjustada={comAjustada} anoAnterior={dados.anoDaAreaAnterior} />
              </PainelDoMomento>

              <PainelDoMomento
                titulo="Maiores municípios por potencial"
                icone={<Sheet size={17} strokeWidth={2.2} className="dem-icone-verde" aria-hidden="true" />}
                direita={
                  <button
                    type="button"
                    className="dem-exportar"
                    onClick={() => baixarCsv(`demanda-${dados.categoria.toLowerCase()}-${carimboDeData()}`, cabecalhoDoCsv(dados), linhasDoCsv(dados))}
                    disabled={dados.municipios.length === 0}
                  >
                    <Download size={14} strokeWidth={2.2} aria-hidden="true" />
                    Exportar CSV
                  </button>
                }
                data-bloco="matriz"
              >
                <MaioresMunicipios dados={dados} base={base} />
              </PainelDoMomento>
            </div>
          </section>
        </>
      )}
    </PaginaDoPainel>
  );
}

/** Um campo da linha de filtros: o ladrilho do ícone, o rótulo em cima e o seletor. */
function Campo({ icone: Icone, rotulo, bloco, children }: { icone: typeof CalendarDays; rotulo: string; bloco: string; children: React.ReactNode }) {
  return (
    <label className="dash-filtro" data-bloco={bloco}>
      <span className="dash-filtro-icone" aria-hidden="true">
        <Icone size={17} strokeWidth={2} />
      </span>
      <span className="dash-filtro-corpo">
        <span className="dash-filtro-rotulo">{rotulo}</span>
        {children}
      </span>
    </label>
  );
}

/** O seletor "Unidades" da maquete — só há unidades: a demanda não tem preço (issue 70), e o seletor diz isso. */
function SeletorDeUnidade() {
  return (
    <select className="dem-unidade" value="unidades" disabled title="Só em unidades: a demanda em reais espera o preço da máquina (issue 70)." aria-label="Unidade">
      <option value="unidades">Unidades</option>
    </select>
  );
}

/** A variação ao lado do número do cartão, ou no começo de uma linha dele: verde quando sobe, vermelha quando cai, nada sem base. */
function Variacao({ valor, naLinha = false }: { valor: number | null; naLinha?: boolean }) {
  if (valor === null) return null;
  return (
    <span className={`dem-variacao ${naLinha ? 'na-linha' : ''} ${valor > 0 ? 'sobe' : valor < 0 ? 'desce' : ''}`}>{variacaoPercentual(valor)}</span>
  );
}

function Limitacoes({ dados }: { dados: DemandaEPrevisaoDaRegiao }) {
  return (
    <>
      <p>
        A demanda do ano, de novembro a outubro — o ano fiscal da Tracbel —, distribuída pela sazonalidade vigente. A entrega
        realizada é a do ART pela data da entrega no {nomeDoAno(dados.anoFiscal)}; o mês que ainda não começou não tem entrega.
        {dados.sazonalidadeDoPrototipo && ' A sazonalidade ainda é a do protótipo, a confirmar.'} Os meses: {dados.previsaoMensal.map((m) => NOME_DO_MES[m.mes]).join(', ')}.
      </p>
      {dados.lacunas.length > 0 && <MetricasSemDado metricas={dados.lacunas} titulo="O que esta leitura não afirma" naDica />}
    </>
  );
}
