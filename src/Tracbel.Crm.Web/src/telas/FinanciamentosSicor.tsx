/**
 * Gestão de Financiamentos — o mercado de crédito de mecanização do SICOR por município e por loja, no período, produto e
 * programa escolhidos (issue 261, a aba "Gestão de Financiamentos" do protótipo da pasta 360).
 *
 * O DESENHO É O DAS TELAS DE MERCADO: filtros na linha, os quatro números do topo, e duas seções na ordem do protótipo —
 *   1. o momento do crédito: os municípios por situação, R3/R6/R12, a série e o peso do recorte em São Paulo;
 *   2. onde está o crédito: a dispersão, cada loja e a tabela analítica, paginada e exportável.
 *
 * A CONTA É A DO CRM: o índice de crédito (70% linhas, 30% valor) e as faixas decididas — e não os cortes de ±5%/±20% do
 * protótipo. LINHA NÃO É CONTRATO: o Banco Central não publica quantidade de contrato. O share Tracbel × concorrência é a
 * issue 262, no painel do crédito dos Indicadores; o que o protótipo tem e esta tela não traz está em "Limitações dos dados".
 */

import { BarChart3, Coins, Flame, Gauge, Landmark, RefreshCw, Snowflake, Sprout, TrendingUp, Thermometer } from 'lucide-react';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { DispersaoDoCredito } from '../componentes/financiamentos/DispersaoDoCredito';
import {
  cabecalhoDaAnalitica,
  comoFiltro,
  linhasDaAnalitica,
  n,
  NOME_DA_FAIXA,
  razao,
  textoDoPeriodo,
  variacaoDaRazao,
} from '../componentes/financiamentos/financiamentos';
import { FiltrosDosFinanciamentosDoSicor } from '../componentes/financiamentos/FiltrosDosFinanciamentos';
import { SerieDoCredito } from '../componentes/financiamentos/SerieDoCredito';
import { TabelaAnaliticaDoCredito } from '../componentes/financiamentos/TabelaAnaliticaDoCredito';
import { TabelaPorLoja } from '../componentes/financiamentos/TabelaPorLoja';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { reaisCurtos } from '../componentes/mercado/momento/formatos';
import { CartaoDoMomento, FileiraDeCartoes, LinhaDePaineis, PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterFinanciamentosDoSicor } from '../dados/api/mercado';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { FinanciamentosDoSicor as Financiamentos, FiltrosDosFinanciamentos, MomentoDoCredito } from '../tipos/mercado';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/diagnostico.css';
import '../estilos/demanda.css';
import '../estilos/financiamentos.css';

export function FinanciamentosSicor() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDosFinanciamentos>({});
  const [busca, setBusca] = useState('');
  const [fimPadrao, setFimPadrao] = useState<string | null>(null);
  const mudar = (parcial: Partial<FiltrosDosFinanciamentos>) => setFiltros((f) => ({ ...f, ...parcial }));

  const leitura = useRecurso(
    (sinal) =>
      obterFinanciamentosDoSicor(contexto, filtros, sinal).then((resposta) => {
        // O FIM DO PERÍODO PADRÃO é lembrado da resposta sem período escolhido: é dele que os atalhos de 3 e 6 meses contam.
        if (!filtros.de && !filtros.ate) setFimPadrao(comoFiltro(resposta.dados.ate) ?? null);
        return resposta;
      }),
    [
      contexto.empresa,
      contexto.usuario,
      filtros.de,
      filtros.ate,
      filtros.produto,
      filtros.programa,
      filtros.recorte,
      filtros.lojaCodigo,
      filtros.usina,
    ],
  );
  const dados = leitura.dados;
  const totais = dados?.totais;
  const r12 = dados?.momento.find((m) => m.meses === 12) ?? null;
  const periodo = dados ? textoDoPeriodo(dados.de, dados.ate) : '';
  const anterior = dados ? textoDoPeriodo(dados.anteriorDe, dados.anteriorAte) : '';

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como as outras telas do padrão.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Gestão de Financiamentos</h1>
          <p className="page-subtitle">
            O mercado de crédito de mecanização do SICOR por município e por loja — onde o financiamento de máquina esquenta e
            onde esfria.
          </p>
        </div>
        <p className="dash-atualizado">
          {leitura.procedencia ? <DadosAtualizadosEm procedencia={leitura.procedencia} /> : 'Lendo o crédito…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={leitura.recarregar}
            disabled={leitura.carregando}
            data-carregando={leitura.carregando ? 'true' : 'false'}
            aria-label="Reler o crédito"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <FiltrosDosFinanciamentosDoSicor filtros={filtros} aoMudar={mudar} dados={dados ?? null} fimPadrao={fimPadrao} />

      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="Linhas do SICOR"
          icone={BarChart3}
          tom="neutro"
          valor={dados?.de ? n(totais!.linhas) : null}
          carregando={leitura.carregando && !dados}
          variacao={dados?.de ? `${dados.recorteNome} · ${periodo}` : null}
          motivoSemDado="O SICOR ainda não foi carregado."
          sobre="As linhas do SICOR de máquina no período. LINHA NÃO É CONTRATO: o Banco Central não publica quantidade de contrato, e cada linha já é a soma dos contratos de uma combinação de município, produto e programa."
        />
        <CartaoDeDecisao
          rotulo="Valor financiado"
          icone={Coins}
          tom="mercado"
          valor={dados?.de ? reaisCurtos(totais!.valor) : null}
          carregando={leitura.carregando && !dados}
          variacao={dados?.de ? `vs ${anterior} ${variacaoDaRazao(razao(totais!.valor, totais!.valorAnterior))}` : null}
          motivoSemDado="O SICOR ainda não foi carregado."
          sobre="O valor contratado em crédito rural de investimento para os produtos de máquina, no período, comparado com o mesmo período do ano anterior."
        />
        <CartaoDeDecisao
          rotulo="R12 linhas"
          icone={TrendingUp}
          tom="captura"
          valor={r12?.indice?.indiceDeLinhas != null ? variacaoDaRazao(r12.indice.indiceDeLinhas) : null}
          carregando={leitura.carregando && !dados}
          variacao="12 meses recentes ÷ os mesmos 12 do ano anterior"
          motivoSemDado="Não há os 12 meses do ano anterior no SICOR para comparar."
          sobre="As linhas dos 12 meses que terminam no fim do período contra as dos mesmos 12 meses um ano antes."
        />
        <CartaoDeDecisao
          rotulo="R12 valor"
          icone={Gauge}
          tom="demanda"
          valor={r12?.indice?.indiceDeValor != null ? variacaoDaRazao(r12.indice.indiceDeValor) : null}
          carregando={leitura.carregando && !dados}
          variacao={r12?.indice?.faixa ? `índice ${variacaoDaRazao(r12.indice.indice)} · ${NOME_DA_FAIXA[r12.indice.faixa].toLowerCase()}` : 'o R12 é o que entra no fator de ciclo'}
          motivoSemDado="Não há os 12 meses do ano anterior no SICOR para comparar."
          sobre="O valor dos 12 meses que terminam no fim do período contra o dos mesmos 12 meses um ano antes. O índice junta as duas parcelas: 70% linhas e 30% valor."
        />
      </div>

      {leitura.carregando && !dados && <BlocoCarregando oQue="o crédito" />}

      {dados?.de && (
        <>
          <section className="dash-secao" data-bloco="secao-momento">
            <TituloDaSecao
              titulo="O momento do crédito"
              subtitulo={`${dados.recorteNome} · ${periodo} contra ${anterior}${dados.produto ? '' : ' · os três produtos de máquina'}`}
              metodologia="SICOR de investimento, produtos de máquina. Toda comparação é com o mesmo período do ano anterior. O índice é o do CRM: 70% da variação das linhas e 30% da do valor, nas faixas decididas (retraído, intermediária, aquecido, superaquecido)."
            />
            <PainelDoMomento
              titulo={`Municípios por situação do crédito — ${periodo}`}
              dica="A faixa do índice de cada município do recorte. Sem crédito no mesmo período do ano anterior não há o que comparar: o município fica em Sem base, e não vira aquecido por ter saído do zero."
              direita={<Limitacoes dados={dados} />}
              data-bloco="situacoes"
            >
              <FileiraDeCartoes>
                <CartaoDoMomento icone={Snowflake} tom="neutro" rotulo="Retraídos" valor={n(dados.situacoes.retraidos)} apoio="índice abaixo de 1" />
                <CartaoDoMomento icone={Thermometer} tom="laranja" rotulo="Intermediários" valor={n(dados.situacoes.intermediarios)} apoio="de 1 a 1,2" />
                <CartaoDoMomento icone={Flame} tom="verde" rotulo="Aquecidos" valor={n(dados.situacoes.aquecidos + dados.situacoes.superaquecidos)} apoio={`${n(dados.situacoes.superaquecidos)} superaquecidos (acima de 1,4)`} />
                <CartaoDoMomento icone={Sprout} tom="azul" rotulo="Sem base" valor={n(dados.situacoes.semBase)} apoio="sem crédito no ano anterior" />
              </FileiraDeCartoes>
            </PainelDoMomento>

            <PainelDoMomento
              titulo="Momento do crédito — R3, R6 e R12"
              dica="Os últimos 3, 6 e 12 meses do período contra os mesmos meses do ano anterior — respeita a sazonalidade. O R12 é o indicador que entra no fator de ciclo da demanda."
              data-bloco="momento"
            >
              <FileiraDeCartoes>
                {dados.momento.map((m) => (
                  <CartaoDoHorizonte key={m.meses} momento={m} />
                ))}
                <CartaoDoMomento
                  icone={Landmark}
                  tom="neutro"
                  rotulo={dados.noEstado ? 'Peso em São Paulo' : 'Recorte'}
                  valor={dados.noEstado ? (dados.noEstado.fatiaDoValor === null ? null : `${n(dados.noEstado.fatiaDoValor, 1)}%`) : 'SP'}
                  apoio={
                    dados.noEstado
                      ? `do valor · ${n(dados.noEstado.fatiaDasLinhas ?? 0, 1)}% das linhas (${n(dados.noEstado.linhas)} de ${n(dados.noEstado.linhasDoEstado)})`
                      : 'o recorte é o estado inteiro'
                  }
                  motivoSemDado="São Paulo não teve crédito de máquina no período."
                  dica="Quanto do crédito de máquina de São Paulo, no período, foi para o recorte."
                />
              </FileiraDeCartoes>
            </PainelDoMomento>

            <PainelDoMomento
              titulo="Série temporal"
              subtitulo={`${dados.recorteNome} · do primeiro ao último mês do SICOR (${textoDoPeriodo(dados.primeiroMesDoSicor, dados.ultimoMesDoSicor)})`}
              dica="A série inteira, e não só o período: é o caminho até ali. O último grupo que ainda não fechou os meses dele sai tracejado, e os meses mais recentes o Banco Central ainda completa com registro atrasado."
              data-bloco="painel-serie"
            >
              <SerieDoCredito serie={dados.serie} />
            </PainelDoMomento>
          </section>

          <section className="dash-secao" data-bloco="secao-onde">
            <TituloDaSecao
              titulo="Onde está o crédito"
              subtitulo="Cada município e cada loja no período, contra o mesmo período do ano anterior."
              metodologia="A fatia de SP é o valor do município sobre o valor do estado no período. O valor médio por linha não é ticket médio: a linha do SICOR é uma soma de contratos."
            />
            <LinhaDePaineis variante="financiamentos">
              <PainelDoMomento
                titulo="Dispersão: valor × linhas por município"
                dica="Cada bolha é um município: linhas no eixo horizontal, valor no vertical, tamanho pelo valor e cor pela faixa do índice. Acima da diagonal, operações maiores; abaixo, crédito pulverizado."
                area="dispersao"
              >
                <DispersaoDoCredito municipios={dados.municipios} />
              </PainelDoMomento>
              <PainelDoMomento
                titulo="Por loja"
                dica="As linhas e o valor dos municípios de cada loja no período, com o ano anterior embaixo, o índice e a situação."
                area="por-loja"
                data-bloco="por-loja"
              >
                <TabelaPorLoja lojas={dados.porLoja} />
              </PainelDoMomento>
            </LinhaDePaineis>

            <PainelDoMomento
              titulo="Tabela analítica — período contra o mesmo período do ano anterior"
              direita={
                <div className="dem-acoes-da-matriz">
                  <input
                    type="search"
                    className="dem-busca"
                    value={busca}
                    placeholder="Buscar município"
                    aria-label="Buscar município na tabela analítica"
                    onChange={(e) => setBusca(e.target.value)}
                  />
                  <button
                    type="button"
                    className="btn btn-secondary btn-sm"
                    onClick={() => baixarCsv(`financiamentos-sicor-${comoFiltro(dados.ate) ?? ''}-${carimboDeData()}`, cabecalhoDaAnalitica(dados), linhasDaAnalitica(dados))}
                    disabled={dados.municipios.length === 0}
                  >
                    Exportar CSV
                  </button>
                </div>
              }
              data-bloco="analitica"
            >
              <TabelaAnaliticaDoCredito municipios={dados.municipios} busca={busca} />
            </PainelDoMomento>
          </section>
        </>
      )}
    </PaginaDoPainel>
  );
}

/** Um horizonte do momento: a variação do valor em destaque, a das linhas e a faixa do índice embaixo. */
function CartaoDoHorizonte({ momento }: { momento: MomentoDoCredito }) {
  const indice = momento.indice;
  return (
    <CartaoDoMomento
      icone={TrendingUp}
      tom={indice?.faixa === 'Retraido' ? 'laranja' : indice?.faixa ? 'verde' : 'neutro'}
      rotulo={`R${momento.meses} — valor`}
      valor={indice?.indiceDeValor != null ? variacaoDaRazao(indice.indiceDeValor) : null}
      apoio={
        indice
          ? `linhas ${variacaoDaRazao(indice.indiceDeLinhas)} · ${indice.faixa ? NOME_DA_FAIXA[indice.faixa].toLowerCase() : 'sem faixa'}`
          : 'sem o mesmo período do ano anterior'
      }
      motivoSemDado={`Não há os ${momento.meses} meses do ano anterior no SICOR, ou eles não tiveram crédito.`}
      dica={`Os últimos ${momento.meses} meses do período contra os mesmos ${momento.meses} meses um ano antes. O índice junta 70% da variação das linhas e 30% da do valor.`}
    />
  );
}

function Limitacoes({ dados }: { dados: Financiamentos }) {
  if (dados.lacunas.length === 0) return null;
  return (
    <span className="diag-limitacoes">
      Limitações dos dados
      <InfoTooltip
        rotulo="Limitações dos dados do crédito"
        texto={<MetricasSemDado metricas={dados.lacunas} titulo="O que esta leitura não afirma" naDica />}
      />
    </span>
  );
}
