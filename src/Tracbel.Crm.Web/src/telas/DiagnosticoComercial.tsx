/**
 * Diagnóstico Comercial — o Índice de Oportunidade Comercial (IOC) de cada município da ADR (issue 257).
 *
 * É a aba 5 do protótipo da pasta 360 (`renderDiag`), com a mesma pergunta — onde a Tracbel tem mais a ganhar
 * agindo agora — e as mesmas contas. Os números vêm do banco do CRM, pela mesma apuração dos Indicadores Geográficos;
 * os pesos são parâmetro com vigência; e componente sem dado sai da conta e é dito.
 *
 * O DESENHO É O DA MAQUETE DO RICARDO DE 02/10/2026 (`docs/prototipo/capturas-referencia/diagnostico-comercial-maquete-
 * 2026-10-02.png`), "idêntico":
 *   1. os filtros — período, regional / loja, cultura principal e CEN / gestor, e o resto em "Mais filtros";
 *   2. os quatro números de decisão — municípios prioritários, demanda, meta (com a rosca do share-alvo) e vendas;
 *   3. "Onde agir?": a prioridade dos municípios (que também filtra a tela), o mapa com zoom e "Onde agir primeiro" —
 *      que vira a ficha do município escolhido;
 *   4. "Os municípios": a tabela inteira, paginada e exportável.
 *
 * O QUE A MAQUETE NÃO DESENHA E A TELA NÃO PERDE: os pesos do IOC, o share-alvo e as limitações estão no "Entenda os
 * indicadores"; o que era sub-linha da tabela está na dica da célula e na ficha. A variação dos municípios prioritários
 * e os minigráficos dos cartões não entram: o CRM não guarda o IOC de antes para comparar.
 *
 * ESTA É A CASCA: busca, guarda a escolha e monta os blocos, que moram em `componentes/diagnostico/`.
 */

import { BarChart3, Coins, Download, Flag, MapPin, RefreshCw, Sheet, Target } from 'lucide-react';
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
  resumir,
  ROTULO_DA_CLASSE,
  SEM_ART,
  type ColunaDoDiagnostico,
} from '../componentes/diagnostico/diagnostico';
import { DistribuicaoPorClasse } from '../componentes/diagnostico/DistribuicaoPorClasse';
import { EntendaOsIndicadores } from '../componentes/diagnostico/EntendaOsIndicadores';
import { FichaDoMunicipioNoIoc } from '../componentes/diagnostico/FichaDoMunicipioNoIoc';
import { FiltrosDoDiagnosticoComercial } from '../componentes/diagnostico/FiltrosDoDiagnostico';
import { MapaDoIoc } from '../componentes/diagnostico/MapaDoIoc';
import { OndeAgirPrimeiro } from '../componentes/diagnostico/OndeAgirPrimeiro';
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
import { nomeProprio } from './cadastro/formato';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/diagnostico.css';

export function DiagnosticoComercial() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDoDiagnostico>({});
  const [classe, setClasse] = useState<ClasseDePrioridade | null>(null);
  const [cultura, setCultura] = useState<string | null>(null);
  const [ordem, setOrdem] = useState<{ coluna: ColunaDoDiagnostico; sentido: 1 | -1 }>({ coluna: 'ioc', sentido: -1 });
  const [lojasConhecidas, setLojasConhecidas] = useState<Map<string, string>>(() => new Map());
  const [culturasConhecidas, setCulturasConhecidas] = useState<string[]>([]);

  // O MUNICÍPIO ESCOLHIDO MORA NA URL, como nos Indicadores Geográficos (issue 163): sobrevive ao F5, ao voltar do
  // navegador e ao link colado no chat — e o "Ver a ficha" do menu da linha é só esse link.
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
        // AS LOJAS E AS CULTURAS DOS FILTROS SÃO AS QUE AS RESPOSTAS JÁ MOSTRARAM, acumuladas: tirá-las só da resposta
        // corrente faria a lista encolher ao escolher uma sub-região.
        setLojasConhecidas((atual) => {
          const proximo = new Map(atual);
          for (const m of resposta.dados.municipios) if (m.lojaCodigo && m.loja) proximo.set(m.lojaCodigo, m.loja);
          return proximo.size === atual.size ? atual : proximo;
        });
        setCulturasConhecidas((atual) => {
          const todas = new Set(atual);
          for (const m of resposta.dados.municipios) if (m.culturaPrincipal) todas.add(m.culturaPrincipal);
          return todas.size === atual.length ? atual : [...todas].sort((a, b) => a.localeCompare(b, 'pt-BR'));
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
      filtros.responsavel,
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

  // O RECORTE DA TELA (02/10/2026): a cultura principal, e o CEN — com ele escolhido, ficam os municípios em que a
  // carteira dele tem vínculo (a API já tirou os clientes das outras). Os cartões e a prioridade passam a ser do recorte.
  const doRecorte = useMemo(
    () => municipios.filter((m) => (cultura === null || m.culturaPrincipal === cultura) && (!filtros.responsavel || m.responsavel !== null)),
    [municipios, cultura, filtros.responsavel],
  );
  const recortado = cultura !== null || !!filtros.responsavel;
  const comArt = dados ? dados.resumo.vendidasNoPeriodo !== null : false;
  const resumo = useMemo(() => (dados ? (recortado ? resumir(doRecorte, comArt) : dados.resumo) : undefined), [dados, recortado, doRecorte, comArt]);

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

  const daClasse = useMemo(() => doRecorte.filter((m) => classe === null || m.classe === classe), [doRecorte, classe]);
  const visiveis = useMemo(() => ordenar(daClasse, ordem.coluna, ordem.sentido), [daClasse, ordem]);
  const acesos = useMemo(() => (recortado || classe !== null ? new Set(daClasse.map((m) => m.codigoIbge)) : null), [recortado, classe, daClasse]);
  const primeiros = useMemo(() => ordenar(daClasse, 'ioc', -1).filter((m) => m.ioc !== null).slice(0, 5), [daClasse]);

  const escolhido = selecionado === null ? null : porCodigo.get(selecionado) ?? null;
  const semArt = dados?.lacunas.some((l) => l.metrica === 'vendasEmUnidades') ?? false;
  const nomeDoCen = dados?.responsaveis.find((r) => String(r.id) === filtros.responsavel)?.nome ?? null;

  function ordenarPor(coluna: ColunaDoDiagnostico, texto?: boolean) {
    setOrdem((o) => (o.coluna === coluna ? { coluna, sentido: o.sentido === 1 ? -1 : 1 } : { coluna, sentido: texto ? 1 : -1 }));
  }

  // ESCOLHER NA TABELA LEVA À FICHA: ela fica acima, ao lado do mapa, e sem rolar a escolha não se veria.
  function escolherNaTabela(codigo: number) {
    escolher(codigo);
    document.getElementById('ficha-do-ioc')?.scrollIntoView?.({ block: 'start', behavior: 'smooth' });
  }

  const share = dados?.shares.find((s) => s.categoriaCodigo === dados.categoria);
  const prioritarios = resumo ? resumo.maxima + resumo.alta : null;

  const aviso = [
    classe !== null && `só a classe ${ROTULO_DA_CLASSE[classe].toLowerCase()}`,
    cultura !== null && `só a cultura principal ${cultura}`,
    nomeDoCen !== null && `só os municípios com carteira de ${nomeProprio(nomeDoCen)}`,
  ].filter(Boolean);

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como as outras telas do padrão: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga diag-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Diagnóstico Comercial</h1>
          <p className="page-subtitle">
            Identifique onde o time comercial deve agir para aumentar a cobertura, ativar oportunidades e impulsionar as vendas.
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
        culturas={culturasConhecidas}
        cultura={cultura}
        aoMudarCultura={setCultura}
        municipios={municipios}
        escolhido={escolhido}
        aoEscolher={escolher}
      />

      {diagnostico.erro && <BlocoErro erro={diagnostico.erro} aoTentarDeNovo={diagnostico.recarregar} />}

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="Municípios prioritários"
          icone={MapPin}
          tom="oportunidade"
          valor={prioritarios === null || !resumo || resumo.total === 0 ? null : n(prioritarios, 0)}
          carregando={diagnostico.carregando && !dados}
          unidade={`de ${resumo ? n(resumo.total, 0) : '—'}`}
          variacao={
            <>
              <span>prioridade máxima e alta</span>
              {resumo?.iocMedio != null && <span className="diag-kpi-nota">(IOC médio: {n(resumo.iocMedio)})</span>}
            </>
          }
          motivoSemDado="A área de atuação entra pela carga do território; sem ela, o diagnóstico não tem município para ordenar."
          sobre="Os municípios do recorte com IOC de prioridade máxima (80 ou mais) e alta (60 a 80), sobre todos os do recorte. O IOC médio é o dos municípios com índice. A variação contra o período anterior não entra: o CRM não guarda o IOC de antes."
        />
        <CartaoDeDecisao
          rotulo="Demanda anual"
          icone={BarChart3}
          tom="demanda"
          valor={resumo?.demandaAjustada != null ? n(resumo.demandaAjustada, 0) : resumo?.demandaEstrutural != null ? n(resumo.demandaEstrutural, 0) : null}
          carregando={diagnostico.carregando && !dados}
          unidade="unidades"
          variacao={
            resumo?.demandaEstrutural != null ? (
              <>
                <span>
                  {dados?.categoriaNome} · {resumo.demandaAjustada != null ? 'ajustada pelo momento' : 'estrutural'}
                </span>
                <span className="diag-kpi-nota">
                  {resumo.demandaAjustada != null ? `estrutural ${n(resumo.demandaEstrutural, 0)}` : 'a renovação do parque'}
                </span>
              </>
            ) : null
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
          unidade="máquinas"
          variacao={<span>{share ? `demanda × share-alvo de ${n(share.percentual)}%` : 'demanda × share-alvo'}</span>}
          grafico={share && resumo?.metaDePlanejamento != null ? <RoscaDoShare percentual={share.percentual} /> : undefined}
          motivoSemDado="Sem share-alvo vigente para esta categoria: registre-o em Configurações › Potencial de mercado."
          sobre="A demanda anual × o share-alvo vigente da categoria — o que a Tracbel planeja vender por ano. A rosca é o share-alvo. O share-alvo é parâmetro com vigência, em Configurações › Potencial de mercado."
        />
        <CartaoDeDecisao
          rotulo="Vendas no período"
          icone={Coins}
          tom="captura"
          valor={resumo?.vendidasNoPeriodo != null ? n(resumo.vendidasNoPeriodo, 0) : null}
          carregando={diagnostico.carregando && !dados}
          unidade="máquinas"
          variacao={
            resumo?.penetracao != null ? (
              <>
                <span>penetração de {pct(resumo.penetracao)} da demanda</span>
                <span className="diag-kpi-nota">({n(resumo.clientesQueCompraram, 0)} clientes compraram)</span>
              </>
            ) : dados ? (
              `${mes(dados.competenciaInicial)} a ${mes(dados.competenciaFinal)}`
            ) : null
          }
          motivoSemDado={SEM_ART}
          sobre="As máquinas vendidas no período pelo ART, nos municípios do recorte. A penetração é a venda levada a um ano sobre a demanda."
        />
      </div>

      {diagnostico.carregando && !dados && <BlocoCarregando oQue="o diagnóstico" />}

      {dados && resumo && (
        <>
          <section className="dash-secao diag-cartao" data-bloco="secao-onde-agir">
            <TituloDaSecao
              titulo="Onde agir?"
              icone={<Target size={22} strokeWidth={2.2} className="diag-icone-verde" aria-hidden="true" />}
              subtitulo="A partir dos dados de demanda, cobertura e performance, veja onde estão as maiores oportunidades."
              acao={<EntendaOsIndicadores dados={dados} />}
            />

            <DistribuicaoPorClasse resumo={resumo} classe={classe} aoEscolherClasse={setClasse} />

            <div className="diag-mapa-e-ficha" id="ficha-do-ioc">
              <MapaDoIoc
                desenho={desenho}
                porCodigo={porCodigo}
                adr={adr}
                acesos={acesos}
                selecionado={selecionado}
                aoSelecionar={escolher}
              />
              {escolhido ? (
                <FichaDoMunicipioNoIoc municipio={escolhido} semArt={semArt} aoFechar={() => escolher(null)} />
              ) : (
                <OndeAgirPrimeiro
                  primeiros={primeiros}
                  aoEscolher={escolher}
                  aoVerTodos={() => document.getElementById('os-municipios')?.scrollIntoView?.({ block: 'start', behavior: 'smooth' })}
                />
              )}
            </div>
          </section>

          <section className="dash-secao diag-cartao" data-bloco="secao-municipios" id="os-municipios">
            <TituloDaSecao
              titulo="Os municípios"
              icone={<Sheet size={20} strokeWidth={2.2} className="diag-icone-verde" aria-hidden="true" />}
              subtitulo="Todos os municípios, ordenados por prioridade, região ou oportunidade."
              metodologia={
                `${dados.categoriaNome} · vendas de ${mes(dados.competenciaInicial)} a ${mes(dados.competenciaFinal)}` +
                (dados.fracaoDoAnoNoPeriodo !== 1 ? ` (levadas a um ano pela sazonalidade: ${pct(dados.fracaoDoAnoNoPeriodo)} do ano)` : '') +
                '. A tabela leva a mesma ordem e os mesmos filtros para todas as páginas, e o CSV leva todas as linhas filtradas, e não só a página. A dica de cada célula traz o resto: a demanda ajustada, os clientes em carteira, os vínculos da cobertura, o momento de preço da cultura e o plano de ação inteiro.'
              }
              acao={
                <button
                  type="button"
                  className="diag-exportar"
                  onClick={() => baixarCsv(`diagnostico-comercial-${dados.categoria.toLowerCase()}-${carimboDeData()}`, CABECALHO_DO_CSV, linhasDoCsv(visiveis))}
                  disabled={visiveis.length === 0}
                >
                  <Download size={15} strokeWidth={2.2} aria-hidden="true" />
                  Exportar CSV
                </button>
              }
            />

            <TabelaDoIoc
              linhas={visiveis}
              total={municipios.length}
              semArt={semArt}
              ordem={ordem}
              aoOrdenar={ordenarPor}
              selecionado={selecionado}
              aoSelecionar={escolherNaTabela}
              aviso={
                aviso.length > 0 && (
                  <>
                    Mostrando {aviso.join(', ')}.{' '}
                    {classe !== null && (
                      <button type="button" className="diag-limpar" onClick={() => setClasse(null)}>
                        Mostrar todas as classes
                      </button>
                    )}
                  </>
                )
              }
            />
          </section>
        </>
      )}
    </PaginaDoPainel>
  );
}

/** A ROSCA DO SHARE-ALVO no cartão da meta (maquete de 02/10/2026) — o percentual escrito no meio. */
function RoscaDoShare({ percentual }: { percentual: number }) {
  const raio = 24;
  const volta = 2 * Math.PI * raio;
  const parte = Math.min(Math.max(percentual, 0), 100) / 100;
  return (
    <svg className="diag-rosca" width="64" height="64" viewBox="0 0 64 64" role="img" aria-label={`Share-alvo de ${n(percentual)}%`}>
      <circle cx="32" cy="32" r={raio} fill="none" stroke="#dbe7fb" strokeWidth="9" />
      <circle
        cx="32"
        cy="32"
        r={raio}
        fill="none"
        stroke="#2f6fe0"
        strokeWidth="9"
        strokeDasharray={`${(volta * parte).toFixed(2)} ${volta.toFixed(2)}`}
        transform="rotate(-90 32 32)"
      />
      <text x="32" y="36" textAnchor="middle" fontSize="12" fontWeight="700" fill="#0b1638">
        {n(percentual, 0)}%
      </text>
    </svg>
  );
}
