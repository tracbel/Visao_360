/**
 * Demanda e Previsão — quantas máquinas a região pede por ano, quanto a Tracbel tem de entregar pelo share-alvo, e em
 * que mês e em que loja (issue 258, a aba "Análise de Demanda" do protótipo da pasta 360).
 *
 * O DESENHO É O DOS INDICADORES GEOGRÁFICOS e do Diagnóstico: filtros na linha, os quatro números de decisão, e os
 * blocos na ordem em que se planeja —
 *   1. a previsão mensal: o ano distribuído pela sazonalidade, cada barra escolhe o mês;
 *   2. a entrega por loja × mês: quem entrega quanto e quando;
 *   3. os parâmetros e a demanda por cultura: de onde vem o número;
 *   4. a matriz município × potencial, paginada e exportável.
 *
 * O AJUSTE É O FATOR DE CICLO DO CRM (preço, crédito e percepção), e não as elasticidades do protótipo — é o mesmo
 * número da ficha do município nos Indicadores. O protótipo fazia o preço e o crédito opcionais; aqui a ajustada vem
 * sempre ao lado da estrutural, e a tela diz quando ela não saiu.
 */

import { BarChart3, CalendarDays, Flag, RefreshCw, Store, Tractor, User, Warehouse } from 'lucide-react';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { CartaoDeIndicador, GradeDeIndicadores, PaginaDoPainel, Painel } from '../componentes/dashboard/Dashboard';
import { EntregaPorLoja } from '../componentes/demanda/EntregaPorLoja';
import { MatrizDaDemanda } from '../componentes/demanda/MatrizDaDemanda';
import { PrevisaoMensal } from '../componentes/demanda/PrevisaoMensal';
import { cabecalhoDoCsv, linhasDoCsv, n, NOME_DO_MES_POR_EXTENSO, variacaoPercentual } from '../componentes/demanda/demanda';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterDemandaEPrevisao } from '../dados/api/mercado';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { DemandaEPrevisaoDaRegiao, FiltrosDaDemanda } from '../tipos/mercado';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/diagnostico.css';
import '../estilos/demanda.css';

export function DemandaEPrevisao() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDaDemanda>({});
  const [mes, setMes] = useState<number | null>(null);
  const [busca, setBusca] = useState('');
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
    [contexto.empresa, contexto.usuario, filtros.regiao, filtros.lojaCodigo, filtros.categoria],
  );
  const dados = leitura.dados;
  const totais = dados?.totais;
  const comAjustada = totais?.demandaAjustada != null;
  const doMes = mes === null ? null : (dados?.previsaoMensal.find((m) => m.mes === mes) ?? null);
  const mudar = (parcial: Partial<FiltrosDaDemanda>) => setFiltros((f) => ({ ...f, ...parcial }));

  return (
    <PaginaDoPainel>
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Demanda e Previsão</h1>
          <p className="page-subtitle">
            Quantas máquinas a área de atuação pede por ano, quanto a <strong>Tracbel Agro</strong> tem de entregar pelo
            share-alvo, e em que mês e em que loja.
          </p>
        </div>
        <p className="dash-atualizado">
          {leitura.procedencia ? <DadosAtualizadosEm procedencia={leitura.procedencia} /> : 'Lendo a demanda…'}
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
          <label className="dash-filtro">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Tractor size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Tipo de máquina</span>
              <select value={filtros.categoria ?? dados?.categoria ?? 'TRATOR'} onChange={(e) => mudar({ categoria: e.target.value })}>
                {(dados?.categorias.length ? dados.categorias : [{ codigo: 'TRATOR', nome: 'Trator', ordem: 1 }]).map((c) => (
                  <option key={c.codigo} value={c.codigo}>
                    {c.nome}
                  </option>
                ))}
                <option value="TODAS">Todas as categorias</option>
              </select>
            </span>
          </label>
          <label className="dash-filtro">
            <span className="dash-filtro-icone" aria-hidden="true">
              <User size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Sub-região</span>
              <select value={filtros.regiao ?? ''} onChange={(e) => mudar({ regiao: e.target.value || undefined })}>
                <option value="">Região Tracbel inteira</option>
                <option value="Norte">Norte</option>
                <option value="Noroeste">Noroeste</option>
              </select>
            </span>
          </label>
          <label className="dash-filtro">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Store size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Loja</span>
              <select value={filtros.lojaCodigo ?? ''} onChange={(e) => mudar({ lojaCodigo: e.target.value || undefined })}>
                <option value="">Todas</option>
                {[...lojasConhecidas.entries()]
                  .sort((a, b) => a[1].localeCompare(b[1], 'pt-BR'))
                  .map(([codigo, nome]) => (
                    <option key={codigo} value={codigo}>
                      {nome}
                    </option>
                  ))}
              </select>
            </span>
          </label>
          <label className="dash-filtro">
            <span className="dash-filtro-icone" aria-hidden="true">
              <CalendarDays size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Mês da previsão</span>
              <select value={mes ?? ''} onChange={(e) => setMes(e.target.value === '' ? null : Number(e.target.value))}>
                <option value="">O ano todo</option>
                {(dados?.previsaoMensal ?? []).map((m) => (
                  <option key={m.mes} value={m.mes}>
                    {NOME_DO_MES_POR_EXTENSO[m.mes]}
                  </option>
                ))}
              </select>
            </span>
          </label>
        </div>
      </div>

      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

      <GradeDeIndicadores data-bloco="kpis">
        <CartaoDeIndicador
          rotulo="Parque necessário"
          icone={Warehouse}
          tom="neutro"
          valor={totais?.parque != null ? n(totais.parque, 0) : null}
          unidade="máquinas"
          contexto={dados ? `${dados.categoriaNome} · o que a área plantada comporta` : undefined}
          motivoSemDado="Nenhum município do recorte tem regra de potencial para esta categoria."
        />
        <CartaoDeIndicador
          rotulo="Demanda anual"
          icone={BarChart3}
          tom="demanda"
          valor={totais?.demandaAjustada != null ? n(totais.demandaAjustada) : totais?.demandaEstrutural != null ? n(totais.demandaEstrutural) : null}
          unidade="máquinas/ano"
          contexto={
            totais?.demandaEstrutural != null
              ? comAjustada
                ? `100% do mercado, ajustada pelo momento · estrutural ${n(totais.demandaEstrutural)}`
                : '100% do mercado · a renovação do parque'
              : undefined
          }
          motivoSemDado="Sem ciclo de renovação nas regras da categoria: o parque sai, a demanda anual não."
          destaque
        />
        <CartaoDeIndicador
          rotulo={doMes ? `A entregar em ${NOME_DO_MES_POR_EXTENSO[doMes.mes]}` : 'A entregar no ano'}
          icone={Flag}
          tom="captura"
          valor={
            doMes
              ? (comAjustada ? doMes.aEntregarAjustada : doMes.aEntregar) != null
                ? n((comAjustada ? doMes.aEntregarAjustada : doMes.aEntregar)!)
                : null
              : (comAjustada ? totais?.aEntregarAjustada : totais?.aEntregar) != null
                ? n((comAjustada ? totais!.aEntregarAjustada : totais!.aEntregar)!)
                : null
          }
          unidade="máquinas"
          contexto={
            dados?.shareAlvo != null
              ? `share-alvo de ${n(dados.shareAlvo, 0)}%${doMes ? ` · ${n(doMes.fracao * 100)}% do ano` : ' · a meta anual da Tracbel'}`
              : undefined
          }
          motivoSemDado="Sem share-alvo vigente para a categoria: registre-o em Configurações › Potencial de mercado."
        />
        <CartaoDeIndicador
          rotulo="Culturas com regra"
          icone={Tractor}
          tom="mercado"
          valor={totais ? n(totais.culturasComRegra.length, 0) : null}
          unidade={totais && totais.culturasComRegra.length > 0 ? 'culturas' : undefined}
          contexto={totais?.culturasComRegra.length ? totais.culturasComRegra.join(', ') : 'nenhuma regra nesta categoria'}
        />
      </GradeDeIndicadores>

      {leitura.carregando && !dados && <BlocoCarregando oQue="a demanda" />}

      {dados && (
        <>
          <Painel
            titulo="Previsão mensal"
            metodologia={
              'A demanda do ano distribuída pela sazonalidade vigente, de novembro a outubro — o ano fiscal da Tracbel. ' +
              'A barra é o que a Tracbel tem de entregar no mês (demanda × share-alvo); o percentual embaixo é o peso do ' +
              'mês no ano. Clique num mês para ver a entrega dele no cartão e no quadro das lojas.'
            }
            acao={<Limitacoes dados={dados} />}
            data-bloco="previsao-mensal"
          >
            <p className="diag-subtitulo-da-tabela">
              {dados.categoriaNome} · {comAjustada ? 'pela demanda ajustada' : 'pela demanda estrutural'}
              {dados.sazonalidadeDoPrototipo && <span className="diag-a-confirmar">sazonalidade do protótipo, a confirmar</span>}
            </p>
            <PrevisaoMensal meses={dados.previsaoMensal} ajustada={comAjustada} mesEscolhido={mes} aoEscolherMes={setMes} />
          </Painel>

          <Painel
            titulo="Entrega por loja × mês"
            metodologia="O que cada loja tem de entregar em cada mês: a demanda dos municípios dela × o share-alvo, distribuída pela sazonalidade. A intensidade da célula reforça o número, que vem sempre escrito."
            data-bloco="entrega-por-loja"
          >
            <EntregaPorLoja lojas={dados.porLoja} meses={dados.previsaoMensal.map((m) => m.mes)} mesEscolhido={mes} />
          </Painel>

          <Painel
            titulo="Parâmetros e demanda por cultura"
            metodologia={
              'O parque é a área plantada (PAM do IBGE' +
              (dados.anoDaAreaPlantada ? `, ${dados.anoDaAreaPlantada}` : '') +
              ') ÷ os hectares por máquina da regra; a demanda é o parque ÷ os anos de renovação. Os dois parâmetros vêm das ' +
              'regras do potencial (Configurações › Potencial de mercado). A ajustada é a mesma demanda pelo fator de ciclo: ' +
              'preço da cultura, crédito do município e percepção.'
            }
            data-bloco="por-cultura"
          >
            <div className="cad-tabela-wrap">
              <table className="cad-tabela diag-tabela dem-culturas">
                <caption className="cad-so-leitor">Os parâmetros e a demanda de cada cultura no recorte</caption>
                <thead>
                  <tr>
                    <th scope="col">Cultura</th>
                    <th scope="col">Área (ha)</th>
                    <th scope="col">ha/máquina</th>
                    <th scope="col">Renovação (anos)</th>
                    <th scope="col">Parque</th>
                    <th scope="col">Demanda/ano</th>
                    {comAjustada && (
                      <>
                        <th scope="col">Ajustada/ano</th>
                        <th scope="col">Efeito do momento</th>
                      </>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {dados.porCultura.map((c) => (
                    <tr key={c.culturaCodigo}>
                      <th scope="row">{c.cultura}</th>
                      <td className="cad-mono">{c.areaUtilHectares === null ? '—' : n(c.areaUtilHectares, 0)}</td>
                      <td className="cad-mono">{c.hectaresPorMaquina === null ? '—' : n(c.hectaresPorMaquina, 0)}</td>
                      <td className="cad-mono">{c.anosDeRenovacao === null ? '—' : n(c.anosDeRenovacao, 0)}</td>
                      <td className="cad-mono">{c.parque === null ? '—' : n(c.parque, 0)}</td>
                      <td className="cad-mono dem-destaque">{c.demandaEstrutural === null ? '—' : n(c.demandaEstrutural)}</td>
                      {comAjustada && (
                        <>
                          <td className="cad-mono dem-destaque">{c.demandaAjustada === null ? '—' : n(c.demandaAjustada)}</td>
                          <td className="cad-mono">
                            {c.variacaoPercentual === null ? (
                              '—'
                            ) : (
                              <span className={`diag-seta ${c.variacaoPercentual > 0 ? 'sobe' : c.variacaoPercentual < 0 ? 'desce' : ''}`}>
                                {variacaoPercentual(c.variacaoPercentual)}
                              </span>
                            )}
                          </td>
                        </>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Painel>

          <Painel
            titulo="Matriz município × potencial"
            acao={
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
                  onClick={() => baixarCsv(`demanda-${dados.categoria.toLowerCase()}-${carimboDeData()}`, cabecalhoDoCsv(dados), linhasDoCsv(dados))}
                  disabled={dados.municipios.length === 0}
                >
                  Exportar CSV
                </button>
              </div>
            }
            data-bloco="matriz"
          >
            <MatrizDaDemanda dados={dados} busca={busca} />
          </Painel>
        </>
      )}
    </PaginaDoPainel>
  );
}

function Limitacoes({ dados }: { dados: DemandaEPrevisaoDaRegiao }) {
  if (dados.lacunas.length === 0) return null;
  return (
    <span className="diag-limitacoes">
      Limitações dos dados
      <InfoTooltip
        rotulo="Limitações dos dados da demanda"
        texto={<MetricasSemDado metricas={dados.lacunas} titulo="O que esta leitura não afirma" naDica />}
      />
    </span>
  );
}
