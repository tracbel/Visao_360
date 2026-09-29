/**
 * Diagnóstico Comercial — o Índice de Oportunidade Comercial (IOC) de cada município da ADR (issue 257).
 *
 * É a aba 5 do protótipo da pasta 360 (`renderDiag`), com a mesma pergunta — onde a Tracbel tem mais a ganhar
 * agindo agora — e as mesmas contas. Os números vêm do banco do CRM, pela mesma apuração dos Indicadores Geográficos;
 * os pesos são parâmetro com vigência; e componente sem dado sai da conta e é dito.
 *
 * O DESENHO É O DOS INDICADORES GEOGRÁFICOS (28/09/2026, pedido do Ricardo: "no modelo da indicadores geográficos,
 * mesmo estilo, melhor organizada"). A ordem é a da decisão:
 *   1. os filtros do recorte, na linha dos Indicadores;
 *   2. os quatro números de decisão — quantos municípios são prioridade, a demanda, a meta e o realizado;
 *   3. a distribuição pelas cinco classes, que também filtra a tela;
 *   4. o mapa da oportunidade ao lado da ficha do município escolhido;
 *   5. a tabela inteira, paginada e exportável.
 *
 * ESTA É A CASCA: busca, guarda a escolha e monta os blocos, que moram em `componentes/diagnostico/`.
 *
 * 29/09/2026 — AS ÚLTIMAS PEÇAS DO PADRÃO (#293, bloco 4): os quatro números passaram ao `CartaoDeDecisao`, os painéis
 * ao `PainelDoMomento`, as tabelas à `mom-tabela`, e a tela ganhou as duas seções com título — onde agir e os municípios
 * — e a largura toda. NENHUM NÚMERO, REGRA OU TEXTO DE REGRA MUDOU.
 */

import { BarChart3, Flag, RefreshCw, Target, Tractor } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import {
  CABECALHO_DO_CSV,
  linhasDoCsv,
  mes,
  n,
  ordenar,
  pct,
  SEM_ART,
  type ColunaDoDiagnostico,
} from '../componentes/diagnostico/diagnostico';
import { DistribuicaoPorClasse } from '../componentes/diagnostico/DistribuicaoPorClasse';
import { FichaDoMunicipioNoIoc } from '../componentes/diagnostico/FichaDoMunicipioNoIoc';
import { FiltrosDoDiagnosticoComercial } from '../componentes/diagnostico/FiltrosDoDiagnostico';
import { MapaDoIoc } from '../componentes/diagnostico/MapaDoIoc';
import { TabelaDoIoc } from '../componentes/diagnostico/TabelaDoIoc';
import type { PoligonoProjetado } from '../componentes/territorio/MapaDeMunicipios';
import { LARGURA_DO_DESENHO } from '../componentes/territorio/indicadoresDaAdr';
import { caminhoSvg, enquadrar, type ColecaoMunicipal } from '../componentes/territorio/projecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterDiagnosticoComercial } from '../dados/api/mercado';
import { carregarMalhaDeSaoPaulo } from '../dados/api/territorio';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { ClasseDePrioridade, FiltrosDoDiagnostico } from '../tipos/mercado';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/diagnostico.css';

export function DiagnosticoComercial() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDoDiagnostico>({});
  const [classe, setClasse] = useState<ClasseDePrioridade | null>(null);
  const [ordem, setOrdem] = useState<{ coluna: ColunaDoDiagnostico; sentido: 1 | -1 }>({ coluna: 'ioc', sentido: -1 });
  const [lojasConhecidas, setLojasConhecidas] = useState<Map<string, string>>(() => new Map());

  // O MUNICÍPIO ESCOLHIDO MORA NA URL, como nos Indicadores Geográficos (issue 163): sobrevive ao F5, ao voltar do
  // navegador e ao link colado no chat.
  const [parametros, definirParametros] = useSearchParams();
  const pedido = Number(parametros.get('municipio'));
  const selecionado = Number.isSafeInteger(pedido) && pedido > 0 ? pedido : null;
  const escolher = useCallback(
    (codigo: number | null) =>
      definirParametros((atuais) => {
        const proximos = new URLSearchParams(atuais);
        if (codigo === null) proximos.delete('municipio');
        else proximos.set('municipio', String(codigo));
        return proximos;
      }),
    [definirParametros],
  );

  const diagnostico = useRecurso(
    (sinal) =>
      obterDiagnosticoComercial(contexto, filtros, sinal).then((resposta) => {
        // AS LOJAS DO FILTRO SÃO AS QUE AS RESPOSTAS JÁ MOSTRARAM, acumuladas: tirá-las só da resposta corrente faria
        // a lista encolher ao escolher uma sub-região.
        setLojasConhecidas((atual) => {
          const proximo = new Map(atual);
          for (const m of resposta.dados.municipios) if (m.lojaCodigo && m.loja) proximo.set(m.lojaCodigo, m.loja);
          return proximo.size === atual.size ? atual : proximo;
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
      filtros.categoria,
      filtros.visao,
    ],
  );
  const dados = diagnostico.dados;

  const [malha, setMalha] = useState<ColecaoMunicipal | null>(null);
  useEffect(() => {
    const controlador = new AbortController();
    carregarMalhaDeSaoPaulo(controlador.signal)
      .then(setMalha)
      // SEM MALHA, O MAPA DIZ QUE ESTÁ CARREGANDO e o resto da tela funciona: a ficha e a tabela não dependem dele.
      .catch(() => undefined);
    return () => controlador.abort();
  }, []);

  const municipios = useMemo(() => dados?.municipios ?? [], [dados]);
  const porCodigo = useMemo(() => new Map(municipios.map((m) => [m.codigoIbge, m])), [municipios]);
  const adr = useMemo(() => new Set(municipios.map((m) => m.codigoIbge)), [municipios]);

  const desenho = useMemo(() => {
    if (!malha || adr.size === 0) return null;
    const enquadramento = enquadrar(malha, LARGURA_DO_DESENHO, 10, (codigo) => adr.has(codigo));
    const poligonos: PoligonoProjetado[] = malha.features.map((f) => ({
      codigo: Number(f.properties.codarea),
      nome: f.properties.nome,
      caminho: caminhoSvg(enquadramento, f.geometry),
    }));
    return { enquadramento, poligonos };
  }, [malha, adr]);

  const visiveis = useMemo(
    () => ordenar(municipios.filter((m) => classe === null || m.classe === classe), ordem.coluna, ordem.sentido),
    [municipios, classe, ordem],
  );
  const primeiros = useMemo(() => ordenar(municipios, 'ioc', -1).filter((m) => m.ioc !== null).slice(0, 5), [municipios]);

  const escolhido = selecionado === null ? null : porCodigo.get(selecionado) ?? null;
  const semArt = dados?.lacunas.some((l) => l.metrica === 'vendasEmUnidades') ?? false;

  function ordenarPor(coluna: ColunaDoDiagnostico, texto?: boolean) {
    setOrdem((o) => (o.coluna === coluna ? { coluna, sentido: o.sentido === 1 ? -1 : 1 } : { coluna, sentido: texto ? 1 : -1 }));
  }

  // ESCOLHER NA TABELA LEVA À FICHA: ela fica acima, ao lado do mapa, e sem rolar a escolha não se veria.
  function escolherNaTabela(codigo: number) {
    escolher(codigo);
    document.getElementById('ficha-do-ioc')?.scrollIntoView?.({ block: 'start', behavior: 'smooth' });
  }

  const resumo = dados?.resumo;
  const share = dados?.shares.find((s) => s.categoriaCodigo === dados.categoria);
  const prioritarios = resumo ? resumo.maxima + resumo.alta : null;

  const exportar = dados && (
    <button
      type="button"
      className="btn btn-secondary btn-sm"
      onClick={() => baixarCsv(`diagnostico-comercial-${dados.categoria.toLowerCase()}-${carimboDeData()}`, CABECALHO_DO_CSV, linhasDoCsv(visiveis))}
      disabled={visiveis.length === 0}
    >
      Exportar CSV
    </button>
  );

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como as outras telas do padrão: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Diagnóstico Comercial</h1>
          <p className="page-subtitle">
            Onde a <strong>Tracbel Agro</strong> tem mais a ganhar agindo agora: o Índice de Oportunidade Comercial (IOC)
            de cada município da área de atuação.
          </p>
        </div>
        <p className="dash-atualizado">
          {diagnostico.procedencia ? <DadosAtualizadosEm procedencia={diagnostico.procedencia} /> : 'Lendo o diagnóstico…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={diagnostico.recarregar}
            disabled={diagnostico.carregando}
            data-carregando={diagnostico.carregando ? 'true' : 'false'}
            aria-label="Reler o diagnóstico"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <FiltrosDoDiagnosticoComercial
        filtros={filtros}
        aoMudar={(parcial) => setFiltros((f) => ({ ...f, ...parcial }))}
        dados={dados}
        lojasConhecidas={lojasConhecidas}
        municipios={municipios}
        escolhido={escolhido}
        aoEscolher={escolher}
      />

      {diagnostico.erro && <BlocoErro erro={diagnostico.erro} aoTentarDeNovo={diagnostico.recarregar} />}

      {/* OS QUATRO NÚMEROS NO CARTÃO DOS INDICADORES (29/09/2026, #293 bloco 4): o `CartaoDeDecisao` tingido, com o
          glifo grande e o que o número é na dica — o mesmo da Visão 360, do Funil e da Performance de CEN. */}
      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="Municípios prioritários"
          icone={Target}
          tom="oportunidade"
          valor={prioritarios === null || !resumo || resumo.total === 0 ? null : n(prioritarios, 0)}
          carregando={diagnostico.carregando && !dados}
          unidade={`de ${resumo ? n(resumo.total, 0) : '—'}`}
          variacao={resumo?.iocMedio != null ? `prioridade máxima e alta · IOC médio ${n(resumo.iocMedio)}` : 'prioridade máxima e alta'}
          motivoSemDado="A área de atuação entra pela carga do território; sem ela, o diagnóstico não tem município para ordenar."
          sobre="Os municípios da área de atuação com IOC de prioridade máxima (80 ou mais) e alta (60 a 80), sobre todos os do recorte. O IOC médio é o dos municípios com índice."
        />
        <CartaoDeDecisao
          rotulo="Demanda anual"
          icone={BarChart3}
          tom="demanda"
          valor={resumo?.demandaAjustada != null ? n(resumo.demandaAjustada, 0) : resumo?.demandaEstrutural != null ? n(resumo.demandaEstrutural, 0) : null}
          carregando={diagnostico.carregando && !dados}
          unidade="máquinas"
          variacao={
            resumo?.demandaEstrutural != null
              ? `${dados?.categoriaNome ?? ''} · ${resumo.demandaAjustada != null ? `ajustada pelo momento; estrutural ${n(resumo.demandaEstrutural, 0)}` : 'estrutural'}`
              : null
          }
          motivoSemDado="Nenhum município do recorte tem demanda estimada nesta categoria: falta regra de potencial ou área plantada."
          sobre="As máquinas que a área de atuação pede por ano nesta categoria: a renovação do parque. Quando o momento está medido, o número é a demanda ajustada pelo fator de ciclo (preço, crédito e percepção), com a estrutural ao lado."
        />
        <CartaoDeDecisao
          rotulo="Meta de planejamento"
          icone={Flag}
          tom="mercado"
          valor={resumo?.metaDePlanejamento != null ? n(resumo.metaDePlanejamento, 0) : null}
          carregando={diagnostico.carregando && !dados}
          unidade="máquinas/ano"
          variacao={share ? `demanda × share-alvo de ${n(share.percentual)}%` : 'demanda × share-alvo'}
          motivoSemDado="Sem share-alvo vigente para esta categoria: registre-o em Configurações › Potencial de mercado."
          sobre="A demanda anual × o share-alvo vigente da categoria — o que a Tracbel planeja vender por ano. O share-alvo é parâmetro com vigência, em Configurações › Potencial de mercado."
        />
        <CartaoDeDecisao
          rotulo="Vendidas no período"
          icone={Tractor}
          tom="captura"
          valor={resumo?.vendidasNoPeriodo != null ? n(resumo.vendidasNoPeriodo, 0) : null}
          carregando={diagnostico.carregando && !dados}
          unidade="máquinas"
          variacao={
            resumo?.penetracao != null
              ? `penetração ${pct(resumo.penetracao)} da demanda · ${n(resumo.clientesQueCompraram, 0)} clientes compraram`
              : dados
                ? `${mes(dados.competenciaInicial)} a ${mes(dados.competenciaFinal)}`
                : null
          }
          motivoSemDado={SEM_ART}
          sobre="As máquinas vendidas no período pelo ART, nos municípios do recorte. A penetração é a venda levada a um ano sobre a demanda."
        />
      </div>

      {diagnostico.carregando && !dados && <BlocoCarregando oQue="o diagnóstico" />}

      {dados && (
        <>
          <section className="dash-secao" data-bloco="secao-onde-agir">
            <TituloDaSecao
              titulo="Onde agir"
              subtitulo="A prioridade dos municípios, o mapa da oportunidade e a ficha do escolhido."
              metodologia="O IOC vai de 0 a 100 e ordena os municípios pelo que a Tracbel tem a ganhar agindo agora — potencial grande e pouco explorado, com crédito e preço a favor. Não é previsão de venda. Os pesos dos componentes são parâmetro com vigência."
            />

            <DistribuicaoPorClasse dados={dados} classe={classe} aoEscolherClasse={setClasse} />

            <div className="diag-mapa-e-ficha" id="ficha-do-ioc">
              <MapaDoIoc
                desenho={desenho}
                porCodigo={porCodigo}
                adr={adr}
                classe={classe}
                selecionado={selecionado}
                aoSelecionar={escolher}
              />
              <FichaDoMunicipioNoIoc
                municipio={escolhido}
                primeiros={primeiros}
                semArt={semArt}
                aoEscolher={escolher}
                aoFechar={() => escolher(null)}
              />
            </div>
          </section>

          <section className="dash-secao" data-bloco="secao-municipios">
          <TituloDaSecao
            titulo="Os municípios"
            subtitulo="Todos os do recorte, pela ordem escolhida, paginados e exportáveis."
            metodologia="A tabela leva a mesma ordem e o mesmo filtro de classe para todas as páginas, e o CSV leva todas as linhas filtradas, e não só a página."
          />

          <TabelaDoIoc
            linhas={visiveis}
            total={municipios.length}
            semArt={semArt}
            ordem={ordem}
            aoOrdenar={ordenarPor}
            selecionado={selecionado}
            aoSelecionar={escolherNaTabela}
            subtitulo={
              `${dados.categoriaNome} · vendas de ${mes(dados.competenciaInicial)} a ${mes(dados.competenciaFinal)}` +
              (dados.fracaoDoAnoNoPeriodo !== 1
                ? ` (levadas a um ano pela sazonalidade: ${pct(dados.fracaoDoAnoNoPeriodo)} do ano)`
                : '') +
              (classe !== null ? ` · só a classe escolhida na distribuição` : '')
            }
            acao={exportar}
          />
          </section>
        </>
      )}
    </PaginaDoPainel>
  );
}
