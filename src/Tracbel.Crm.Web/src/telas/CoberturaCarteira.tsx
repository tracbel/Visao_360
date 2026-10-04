/**
 * Cobertura de Carteira — ligada ao dado real de `comercial.ClienteCarteira`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a tela deixou de ler o JSON do protótipo.
 *
 * Antes ela montava a carteira de `public/dados/carteira-cen.json` e
 * `cobertura.json`, ordenava **por classe de cliente** e desenhava um mapa com
 * as coordenadas de `cidades-latlng.json`. Agora lê `/api/v1/cobertura` —
 * **15.104 vínculos** só na filial de Ribeirão Preto — e o resumo por carteira
 * de `/api/v1/relatorios/cobertura`, agrupado no banco.
 *
 * ---------------------------------------------------------------------------
 * 27/09/2026 — o último contato passou a vir do histórico INTEIRO do Vórtice.
 *
 * Até aqui, `UltimaInteracaoEm` era apurada só das interações que a carga já
 * tinha trazido para o CRM — um recorte. Agora ela vem da regra da BI de
 * carteiras do Vórtice (`BI_CARTEIRA_VN`): 53 resultados que contam como
 * contato, em qualquer canal e em qualquer departamento, calculados todo dia
 * pela rotina `CARTEIRAS_VORTICE` sobre o histórico inteiro — e a data só anda
 * para a frente. Hoje, 5.412 dos 8.537 vínculos ativos (63%) têm essa data;
 * nas carteiras de campo (`MAQ_`), 96,7%.
 *
 * ---------------------------------------------------------------------------
 * AS TRÊS COISAS QUE MUDARAM, e as três são a mesma decisão:
 *
 * 1. **A ordenação por classe A/B saiu — mas a SEGMENTAÇÃO por classe voltou.**
 *    A classe do VÍNCULO (`ClienteCarteira.Classe`) continua sem se sustentar:
 *    ela nasce de `IVS_Pes.Potencial`, um `varchar(3)` sem catálogo, e segue
 *    entrando como `C` por assunção na maioria dos vínculos (documento 25,
 *    §5.1) — ordenar por ela seguiria sendo ordenar por nada. O que mudou é
 *    que `Cliente.Classe` — a curva ABC apurada do faturamento (A 734 · B
 *    1.045 · C 5.726 · D 19.831) — está pronta, e o filtro `classe` da rota
 *    passou a usar essa classe, e não a do vínculo.
 *
 *    **A ordem continua sendo quem está há mais tempo sem contato**, que é
 *    dado real e calculado do fato.
 *
 * 2. **O mapa saiu, e o motivo está no fim da tela.** Ele plotava as
 *    coordenadas do protótipo, que são de Mato Grosso, Goiás e Bahia —
 *    geografia que não existe nesta operação. O território que existe está na
 *    Cobertura por Filial e Carteira (documento 26).
 *
 * 3. **"Registrar contato" saiu.** Gravava em `localStorage` e mexia nos
 *    contadores da própria tela. Interação é fato imutável e somente
 *    acrescentar, e nenhuma rota de relacionamento escreve (dívida D-9).
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS: o cabeçalho com a hora da leitura, a barra de filtros, os cinco
 * números como `CartaoDeDecisao` e duas seções em `PainelDoMomento`.
 *
 * ---------------------------------------------------------------------------
 * 30/09/2026 — IDÊNTICA À MAQUETE DO RICARDO (`docs/prototipo/capturas-referencia/cobertura-carteira-maquete-2026-09-30.png`),
 * com as regras do Funil e da Visão 360: não inventar e não tirar nada da tela. O desenho mora em `estilos/cobertura.css`,
 * embaixo de `.dash-pagina.cob-maquete`.
 *
 * - **"Qual carteira está descoberta?"** é o cartão da seção: barras de 100% com o percentual de cada faixa escrito
 *   dentro, o total ao lado e o **maior risco** no canto — a carteira comercial com mais vínculos há mais de 90 dias ou
 *   nunca contatados.
 * - **"Distribuição da cobertura"**, ao lado: a rosca das quatro faixas (todas as carteiras, ou a escolhida) e as
 *   **carteiras por maior exposição**.
 * - **"Quem contatar primeiro"**: a busca por cliente, carteira ou responsável (no servidor), o **Exportar** (a lista
 *   inteira com os filtros, em CSV), a **prioridade** e o menu de cada linha (a ficha e as máquinas do cliente).
 *
 * DUAS DECISÕES DO RICARDO (30/09/2026): a PRIORIDADE é a curva ABC do cliente (A alta, B média, C, D e sem classe baixa),
 * e o cartão "Contato em 90 dias" é SÓ A FAIXA de 31 a 90 dias — os cartões e a rosca somam o total de vínculos.
 *
 * O QUE A MAQUETE TEM E ESTA TELA NÃO: o mini-gráfico de tendência no canto de cada cartão. Ele desenha a evolução de cada
 * número, e o CRM não guarda a cobertura de cada dia — desenhar a curva seria inventá-la.
 */

import {
  ArrowDown,
  ArrowUp,
  BriefcaseBusiness,
  CalendarCheck,
  CalendarClock,
  CircleAlert,
  Download,
  Layers,
  RefreshCw,
  Search,
  Users,
  UsersRound,
} from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { GraficoDonutCentro } from '../componentes/GraficoDonutCentro';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { MenuDaLinha } from '../componentes/comum/MenuDaLinha';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento, Seletor } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import {
  COBERTURA_INICIAL,
  LINHAS_NO_EXPORTAR,
  listarCobertura,
  listarCoberturaInteira,
  obterResumoDeCobertura,
} from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type {
  ConsultaDeCobertura,
  CoberturaResumo,
  OrdemDeCobertura,
  PrioridadeDeContato,
  ResumoDeCobertura,
} from '../tipos/relacionamento';
import { formatarData } from './cadastro/formato';
import { formatarPercentualDaParte, formatarNumero as nº } from '../dados/formatadores';
import { CampoDoFiltro } from '../componentes/comum/CampoDoFiltro';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';
import '../estilos/cobertura.css';

/** As colunas, quais delas a API sabe ordenar, e quais são número (alinhadas à direita). */
const COLUNAS: { rotulo: string; ordem?: OrdemDeCobertura; numerica?: boolean }[] = [
  { rotulo: 'Cliente', ordem: 'Nome' },
  { rotulo: 'Carteira' },
  { rotulo: 'Responsável' },
  { rotulo: 'Último contato', ordem: 'UltimaInteracaoEm' },
  { rotulo: 'Dias sem contato', numerica: true },
  { rotulo: 'Ciclo declarado', numerica: true },
  { rotulo: 'Prioridade' },
];

/** Faixas de atraso oferecidas no filtro. Todas resolvidas no banco. */
const FAIXAS = [
  { valor: '', rotulo: 'Qualquer tempo' },
  { valor: '30', rotulo: 'Mais de 30 dias' },
  { valor: '60', rotulo: 'Mais de 60 dias' },
  { valor: '90', rotulo: 'Mais de 90 dias' },
  { valor: '180', rotulo: 'Mais de 180 dias' },
];

/**
 * As classes da curva ABC oferecidas no filtro.
 *
 * É `Cliente.Classe` — a classe apurada do faturamento —, e não a classe do
 * vínculo (`ClienteCarteira.Classe`), que segue entrando como C por assunção
 * na maioria dos casos e por isso não sustenta segmentação nenhuma.
 */
const CLASSES = [
  { valor: '', rotulo: 'Qualquer classe' },
  { valor: 'A', rotulo: 'Classe A' },
  { valor: 'B', rotulo: 'Classe B' },
  { valor: 'C', rotulo: 'Classe C' },
  { valor: 'D', rotulo: 'Classe D' },
];

/** As quatro faixas de um conjunto de vínculos — exclusivas, e a soma é o total. */
type Faixas = { em30: number; entre31e90: number; acima90: number; nunca: number; total: number };

/**
 * As quatro faixas de tempo sem contato, na ordem em que a barra as empilha, com os nomes e as cores da maquete.
 *
 * `medir` deriva cada faixa dos acumulados que a API devolve, que são cumulativos (`comContatoEm90Dias` inclui os de
 * 30). Subtrair aqui é o que torna as fatias exclusivas e a soma igual ao total de vínculos.
 */
const FAIXAS_DE_CONTATO: { nome: string; cor: string; de: (f: Faixas) => number }[] = [
  { nome: 'Em até 30 dias', cor: '#1B873F', de: (f) => f.em30 },
  { nome: 'Entre 31 e 90 dias', cor: '#F39A1E', de: (f) => f.entre31e90 },
  { nome: 'Acima de 90 dias', cor: '#E53935', de: (f) => f.acima90 },
  { nome: 'Nunca contatados', cor: '#8A909A', de: (f) => f.nunca },
];

function faixasDe(c: Pick<ResumoDeCobertura, 'clientes' | 'comContatoEm30Dias' | 'comContatoEm90Dias' | 'nuncaContatados'>): Faixas {
  return {
    em30: c.comContatoEm30Dias,
    entre31e90: c.comContatoEm90Dias - c.comContatoEm30Dias,
    acima90: Math.max(0, c.clientes - c.comContatoEm90Dias - c.nuncaContatados),
    nunca: c.nuncaContatados,
    total: c.clientes,
  };
}

/** A exposição: há mais de 90 dias ou nunca contatados, sobre o total. Nula sem vínculo. */
function exposicao(f: Faixas): number | null {
  return f.total > 0 ? (f.acima90 + f.nunca) / f.total : null;
}

/** O percentual inteiro de uma parte, ou nulo sem denominador. */
const fatia = (parte: number, todo: number) => (todo > 0 ? Math.round((100 * parte) / todo) : null);

/** Quantas carteiras entram nas barras antes de a barra virar um traço. */
const CARTEIRAS_NO_GRAFICO = 12;

/** Quantas carteiras o ranking de exposição mostra. */
const CARTEIRAS_NO_RANKING = 5;

/** O intervalo entre a última tecla e o pedido à API — o mesmo das outras listas com busca. */
const ESPERA_DA_BUSCA_MS = 350;

/**
 * A partir de quanto a exposição de uma carteira pinta de vermelho no ranking — um terço dos vínculos há mais de 90 dias
 * ou nunca contatados. Abaixo, laranja. É desenho, e não regra de negócio: o número está ao lado.
 */
const EXPOSICAO_VERMELHA = 1 / 3;

/**
 * O MAIOR RISCO E O RANKING DE EXPOSIÇÃO CONTAM AS CARTEIRAS COMERCIAIS — a mesma regra do mix e do ranking de CENs da
 * Visão 360: a carteira administrativa e a de teste são depósito de cadastro, com todo mundo "nunca contatado" por
 * construção, e não carteira de ninguém.
 */
const ehComercial = (c: ResumoDeCobertura) => c.naturezaDaCarteira === 'Comercial';

export function CoberturaCarteira() {
  const { contexto } = useContextoDeAcesso();
  const [consulta, setConsulta] = useState<ConsultaDeCobertura>(COBERTURA_INICIAL);
  const [termoDigitado, setTermoDigitado] = useState('');

  // A BUSCA VAI AO SERVIDOR um instante depois da última tecla, e não a cada letra — o mesmo desenho do Pipeline.
  useEffect(() => {
    const relogio = setTimeout(
      () => setConsulta((c) => (c.termo === termoDigitado ? c : { ...c, termo: termoDigitado, pagina: 1 })),
      ESPERA_DA_BUSCA_MS,
    );
    return () => clearTimeout(relogio);
  }, [termoDigitado]);

  const resumo = useRecurso(
    (sinal) => obterResumoDeCobertura(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const lista = useRecurso(
    (sinal) => listarCobertura(contexto, consulta, sinal),
    [contexto.empresa, contexto.usuario, JSON.stringify(consulta)],
  );

  const carteiras = useMemo(() => resumo.dados?.itens ?? [], [resumo.dados]);

  /** As maiores primeiro: são elas que movem o número consolidado. */
  const carteirasOrdenadas = useMemo(
    () => [...carteiras].sort((a, b) => b.clientes - a.clientes),
    [carteiras],
  );
  const carteirasNoGrafico = carteirasOrdenadas.slice(0, CARTEIRAS_NO_GRAFICO);

  const totais = useMemo(() => {
    const clientes = carteiras.reduce((s, c) => s + c.clientes, 0);
    return {
      carteiras: carteiras.length,
      faixas: faixasDe({
        clientes,
        comContatoEm30Dias: carteiras.reduce((s, c) => s + c.comContatoEm30Dias, 0),
        comContatoEm90Dias: carteiras.reduce((s, c) => s + c.comContatoEm90Dias, 0),
        nuncaContatados: carteiras.reduce((s, c) => s + c.nuncaContatados, 0),
      }),
    };
  }, [carteiras]);

  /** As carteiras comerciais pela exposição, da maior para a menor. */
  const porExposicao = useMemo(
    () =>
      carteiras
        .filter((c) => ehComercial(c) && c.clientes > 0)
        .map((c) => ({ carteira: c, exposicao: exposicao(faixasDe(c)) ?? 0 }))
        .sort((a, b) => b.exposicao - a.exposicao || b.carteira.clientes - a.carteira.clientes),
    [carteiras],
  );

  const temFiltro = useMemo(
    () =>
      consulta.somenteSemContato ||
      consulta.diasSemContato !== '' ||
      consulta.classe !== '' ||
      consulta.termo.trim() !== '' ||
      consulta.ordenarPor !== COBERTURA_INICIAL.ordenarPor ||
      consulta.descendente,
    [consulta],
  );

  function trocarOrdem(campo: OrdemDeCobertura) {
    setConsulta((c) =>
      c.ordenarPor === campo
        ? { ...c, descendente: !c.descendente, pagina: 1 }
        : { ...c, ordenarPor: campo, descendente: false, pagina: 1 },
    );
  }

  function limparFiltros() {
    setTermoDigitado('');
    setConsulta({ ...COBERTURA_INICIAL, tamanho: consulta.tamanho });
  }

  const pagina = lista.dados;
  const { faixas } = totais;

  const semCarteira = resumo.carregando ? undefined : 'Sem carteira ao alcance deste contexto.';
  const dosVinculos = (parte: number) => {
    const p = fatia(parte, faixas.total);
    return p === null ? null : `${p}% dos vínculos`;
  };

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga cob-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">
            <BriefcaseBusiness className="cob-titulo-icone" size={24} strokeWidth={2.4} aria-hidden="true" />
            Cobertura de Carteira
            <InfoTooltip
              rotulo="De onde vem o último contato"
              texto="A data do último contato vem do histórico inteiro do Vórtice, pela regra da BI de carteiras (53 resultados que contam como contato, em qualquer canal), apurada todo dia pela rotina das carteiras — e só anda para a frente."
            />
          </h1>
          <p className="page-subtitle">Quem está há tempo demais sem contato, carteira a carteira, na filial do cabeçalho.</p>
        </div>
        <p className="dash-atualizado">
          {lista.procedencia ? <DadosAtualizadosEm procedencia={lista.procedencia} /> : 'Lendo a cobertura…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              resumo.recarregar();
              lista.recarregar();
            }}
            disabled={resumo.carregando || lista.carregando}
            data-carregando={resumo.carregando || lista.carregando ? 'true' : 'false'}
            aria-label="Reler a cobertura"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES: os três filtros da lista, todos resolvidos no banco. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <CampoDoFiltro icone={CalendarClock} rotulo="Sem contato há" bloco="sem-contato-ha">
            <select
              value={consulta.diasSemContato}
              onChange={(e) => setConsulta((c) => ({ ...c, diasSemContato: e.target.value, pagina: 1 }))}
            >
              {FAIXAS.map((f) => (
                <option key={f.valor} value={f.valor}>
                  {f.rotulo}
                </option>
              ))}
            </select>
          </CampoDoFiltro>

          <CampoDoFiltro
            icone={Layers}
            rotulo={
              <>
                Classe
                <InfoTooltip
                  texto="Curva ABC apurada do faturamento (Cliente.Classe) — não a classe do vínculo importada do Vórtice."
                  rotulo="Qual classe é esta"
                />
              </>
            }
            bloco="classe"
          >
            <select value={consulta.classe} onChange={(e) => setConsulta((c) => ({ ...c, classe: e.target.value, pagina: 1 }))}>
              {CLASSES.map((cl) => (
                <option key={cl.valor} value={cl.valor}>
                  {cl.rotulo}
                </option>
              ))}
            </select>
          </CampoDoFiltro>

          <div className="dash-filtros-acao">
            <label className="dash-caixa">
              <input
                type="checkbox"
                checked={consulta.somenteSemContato}
                onChange={(e) => setConsulta((c) => ({ ...c, somenteSemContato: e.target.checked, pagina: 1 }))}
              />
              Só quem nunca foi contatado
            </label>
          </div>

          {temFiltro && (
            <div className="dash-filtros-acao">
              <button type="button" className="dash-mais-filtros" onClick={limparFiltros}>
                Limpar filtros
              </button>
            </div>
          )}
        </div>
      </div>

      <AvisoDeProcedencia procedencia={lista.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="Carteiras"
          icone={BriefcaseBusiness}
          tom="demanda"
          valor={resumo.dados ? nº(totais.carteiras) : null}
          carregando={resumo.carregando}
          unidade="carteiras"
          motivoSemDado={semCarteira}
          variacao={null}
          sobre="As carteiras desta filial com pelo menos um cliente vinculado."
        />
        <CartaoDeDecisao
          rotulo="Clientes carteira"
          icone={Users}
          tom="mercado"
          valor={resumo.dados ? nº(faixas.total) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={null}
          sobre="Vínculos cliente × carteira: o mesmo cliente conta em cada carteira em que está."
        />
        <CartaoDeDecisao
          rotulo="Contato em 30 dias"
          icone={CalendarCheck}
          tom="demanda"
          valor={resumo.dados ? nº(faixas.em30) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? dosVinculos(faixas.em30) : null}
          sobre="Último contato (regra da BI de carteiras do Vórtice) nos últimos 30 dias."
        />
        <CartaoDeDecisao
          rotulo="Contato em 90 dias"
          icone={CalendarClock}
          tom="captura"
          valor={resumo.dados ? nº(faixas.entre31e90) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? dosVinculos(faixas.entre31e90) : null}
          sobre="Último contato (regra da BI de carteiras do Vórtice) entre 31 e 90 dias atrás — só a faixa, sem os de 30 dias: as quatro faixas somam o total de vínculos (decisão de 30/09/2026)."
        />
        <CartaoDeDecisao
          rotulo="Nunca contatados"
          icone={UsersRound}
          tom="oportunidade"
          valor={resumo.dados ? nº(faixas.nunca) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? dosVinculos(faixas.nunca) : null}
          sobre="Sem contato no histórico do Vórtice, pela regra da BI de carteiras."
        />
      </div>

      {resumo.erro && <BlocoErro erro={resumo.erro} aoTentarDeNovo={resumo.recarregar} />}

      <MetricasSemDado metricas={resumo.dados?.metricasSemDado} />

      <div className="cob-linha" data-bloco="linha-cobertura">
        {/* QUAL CARTEIRA ESTÁ DESCOBERTA — o cartão da seção, como na maquete. O mapa que ficava aqui plotava pinos de
            outro estado; o motivo virou a nota do fim da tela. */}
        <section className="dash-secao cob-cartao" data-bloco="secao-carteiras">
          <TituloDaSecao
            titulo="Qual carteira está descoberta?"
            subtitulo="A fatia de cada faixa de tempo sem contato, carteira a carteira."
            metodologia="As faixas são exclusivas: até 30 dias, de 31 a 90, mais de 90 e nunca contatado — a soma é o total de vínculos. O maior risco é a carteira comercial com a maior fatia há mais de 90 dias ou nunca contatada; a administrativa e a de teste ficam fora dele, porque são depósito de cadastro."
            acao={
              porExposicao[0] && porExposicao[0].exposicao > 0 ? (
                <div className="cob-secao-acao">
                  <MaiorRisco carteira={porExposicao[0].carteira} exposicao={porExposicao[0].exposicao} />
                </div>
              ) : undefined
            }
          />

          <div className="cob-barras" data-bloco="cobertura-por-carteira">
            <div className="cob-barras-cabecalho">
              <h3 className="cob-barras-titulo">
                Cobertura por carteira
                <InfoTooltip
                  rotulo="Como ler a cobertura por carteira"
                  texto={`As maiores carteiras primeiro: são elas que movem o número consolidado${
                    carteiras.length > carteirasNoGrafico.length
                      ? ` (as ${carteirasNoGrafico.length} maiores de ${carteiras.length})`
                      : ''
                  }. O número exato de todas as carteiras está em “Cobertura por carteira, em número”, no fim da tela.`}
                />
              </h3>
              <span className="cob-barras-total" aria-hidden="true">
                Total
              </span>
            </div>

            {resumo.carregando && <BlocoCarregando oQue="a cobertura por carteira" />}
            {!resumo.carregando && carteirasNoGrafico.length > 0 && <BarrasDaCobertura carteiras={carteirasNoGrafico} />}
          </div>
        </section>

        <DistribuicaoDaCobertura carteiras={carteiras} total={faixas} porExposicao={porExposicao} carregando={resumo.carregando} />
      </div>

      <section className="dash-secao cob-cartao" data-bloco="secao-clientes">
        <TituloDaSecao
          titulo="Quem contatar primeiro"
          subtitulo={
            lista.recarregando
              ? 'Atualizando…'
              : `${nº(pagina?.total ?? 0)} vínculos · quem está há mais tempo sem contato primeiro.`
          }
          metodologia="A ordem é quem está há mais tempo sem contato — dado real, calculado do fato —, e o nunca contatado vem antes de todos. A prioridade é a curva ABC do cliente, apurada do faturamento: A é alta, B é média, C, D e o cliente sem classe são baixa. A classe do vínculo importada do Vórtice não ordena nada: ela entra como C por assunção na maioria dos casos."
          acao={
            <div className="cob-secao-acao cob-busca-e-exportar">
              <label className="cob-busca">
                <Search size={15} strokeWidth={2} aria-hidden="true" />
                <span className="cad-so-leitor">Buscar cliente, carteira ou responsável</span>
                <input
                  type="search"
                  value={termoDigitado}
                  placeholder="Buscar cliente, carteira ou responsável..."
                  onChange={(e) => setTermoDigitado(e.target.value)}
                />
              </label>
              <BotaoExportar consulta={consulta} total={pagina?.total ?? 0} />
            </div>
          }
        />

        <PainelDoMomento
          titulo="Clientes por tempo sem contato"
          data-bloco="clientes-sem-contato"
          dica={`Ordenados por ${consulta.ordenarPor === 'Nome' ? 'nome' : 'último contato'}${consulta.descendente ? ', do maior para o menor' : ''}. Fora do ciclo é a carteira que declara de quantos em quantos dias o cliente deve ser visitado, e o último contato passou desse prazo. Sem cadência declarada, não se afirma que está em dia nem fora.`}
        >
          {lista.carregando && <BlocoCarregando oQue="a cobertura da carteira" />}
          {lista.erro && <BlocoErro erro={lista.erro} aoTentarDeNovo={lista.recarregar} />}

          {pagina && !lista.erro && pagina.itens.length === 0 && (
            <BlocoVazio
              titulo={temFiltro ? 'Nenhum cliente com esses filtros' : 'Esta filial não tem carteira carregada'}
              texto={
                temFiltro
                  ? 'Nenhum vínculo desta filial cai nesta faixa de tempo sem contato, ou nesta busca.'
                  : 'A carteirização ativa tem 8.537 vínculos nas treze filiais. Confira a filial escolhida no cabeçalho.'
              }
              acao={
                temFiltro ? (
                  <button type="button" className="btn btn-secondary" onClick={limparFiltros}>
                    Limpar filtros
                  </button>
                ) : undefined
              }
            />
          )}

          {pagina && pagina.itens.length > 0 && (
            <>
              <div className="mom-tabela-rolagem cad-so-largo">
                <table className="mom-tabela cob-tabela">
                  <caption className="cad-so-leitor">
                    Cobertura da filial {contexto.empresa}, ordenada por {consulta.ordenarPor}
                  </caption>
                  <thead>
                    <tr>
                      {COLUNAS.map((coluna) => (
                        <th
                          key={coluna.rotulo}
                          scope="col"
                          className={coluna.numerica ? 'mom-num' : undefined}
                          aria-sort={ariaOrdem(coluna.ordem, consulta)}
                        >
                          {coluna.ordem ? (
                            <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem!)}>
                              {coluna.rotulo}
                              <SetaDaOrdem campo={coluna.ordem} consulta={consulta} />
                            </button>
                          ) : (
                            coluna.rotulo
                          )}
                        </th>
                      ))}
                      <th scope="col">
                        <span className="cad-so-leitor">Ações</span>
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {pagina.itens.map((linha) => (
                      <tr key={`${linha.clienteChave}-${linha.carteiraChave}`}>
                        <th scope="row">
                          <Link to={`/clientes/${linha.clienteChave}`} className="cob-cliente">
                            {linha.clienteNome}
                          </Link>
                        </th>
                        <td>
                          <div className="cob-carteira">{linha.carteiraNome}</div>
                          <div className="cad-sub">{linha.linhaDeNegocioNome}</div>
                        </td>
                        <td className="cob-responsavel">{linha.responsavelNome}</td>
                        {/* A AUSÊNCIA É DITA UMA VEZ, e não três: o aviso fica no último contato, onde a informação nasce;
                            as outras duas mostram o travessão de dado ausente, com o motivo na dica (issue 167). */}
                        <td>
                          {linha.ultimaInteracaoEm ? (
                            formatarData(linha.ultimaInteracaoEm)
                          ) : (
                            <span className="cob-nunca">nunca contatado</span>
                          )}
                        </td>
                        <td className="mom-num">
                          {linha.diasSemContato === null ? (
                            <ValorAusente motivo="Sem contato registrado: não há de quando contar." oQue="os dias sem contato" />
                          ) : (
                            <span className={linha.diasSemContato > 90 ? 'cad-alerta' : undefined}>
                              {nº(linha.diasSemContato)}
                            </span>
                          )}
                        </td>
                        <td className="mom-num">
                          {/* FORA DO CICLO É NULO, E NÃO FALSO, quando não há cadência declarada: falso diria "está em
                              dia", e não é isso que se sabe. */}
                          {linha.diasCicloContato === null ? (
                            <ValorAusente
                              motivo="A carteira não declara de quantos em quantos dias este cliente deve ser visitado."
                              oQue="o ciclo declarado"
                            />
                          ) : (
                            <>
                              {linha.diasCicloContato} dias
                              {linha.estaForaDoCiclo && <div className="cad-alerta">fora do ciclo</div>}
                            </>
                          )}
                        </td>
                        <td>
                          <SeloDePrioridade prioridade={linha.prioridade} classe={linha.classeDoCliente} />
                        </td>
                        <td className="cob-coluna-menu">
                          {/* O QUE JÁ EXISTE PARA O CLIENTE: a ficha e as máquinas dele. */}
                          <MenuDaLinha
                            rotulo={`Ações de ${linha.clienteNome}`}
                            itens={[
                              { rotulo: 'Abrir a ficha do cliente', para: `/clientes/${linha.clienteChave}` },
                              { rotulo: 'Ver as máquinas do cliente', para: `/equipamentos?cliente=${linha.clienteChave}` },
                            ]}
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="cad-fichas cad-so-estreito">
                {pagina.itens.map((linha) => (
                  <FichaDeCobertura key={`${linha.clienteChave}-${linha.carteiraChave}`} linha={linha} />
                ))}
              </div>

              <BarraDePaginacao
                pagina={pagina}
                oQue="vínculos"
                aoTrocarPagina={(p) => setConsulta((c) => ({ ...c, pagina: p }))}
                aoTrocarTamanho={(t) => setConsulta((c) => ({ ...c, tamanho: t, pagina: 1 }))}
              />
            </>
          )}
        </PainelDoMomento>
      </section>

      {/* A MESMA COBERTURA POR CARTEIRA, EM NÚMERO. As barras respondem "quem está descoberto" e param por aí; quem
          precisa do número exato — todas as carteiras, e não as doze maiores — abre aqui. Fechado por padrão. */}
      <BlocoRecolhivel
        titulo="Cobertura por carteira, em número"
        resumo={`as ${carteiras.length} carteiras desta filial, contadas no banco`}
      >
        {resumo.carregando && <BlocoCarregando oQue="o resumo por carteira" />}

        {carteiras.length > 0 && (
          <div className="mom-tabela-rolagem">
            <table className="mom-tabela">
              <caption className="cad-so-leitor">Cobertura por carteira da filial {contexto.empresa}</caption>
              <thead>
                <tr>
                  <th scope="col">Carteira</th>
                  <th scope="col">Responsável</th>
                  <th scope="col" className="mom-num">Clientes</th>
                  <th scope="col" className="mom-num">Em 30 dias</th>
                  <th scope="col" className="mom-num">Em 90 dias</th>
                  <th scope="col" className="mom-num">Nunca contatados</th>
                  <th scope="col" className="mom-num">Último contato</th>
                </tr>
              </thead>
              <tbody>
                {carteirasOrdenadas.map((c) => (
                  <tr key={c.carteiraChave}>
                    <th scope="row">
                      <div className="cad-link-forte">{c.carteiraNome}</div>
                      <div className="cad-sub">{c.linhaDeNegocioNome}</div>
                    </th>
                    <td>{c.responsavelNome}</td>
                    <td className="mom-num">{nº(c.clientes)}</td>
                    <td className="mom-num">
                      {nº(c.comContatoEm30Dias)}
                      <div className="cad-sub">{formatarPercentualDaParte(c.comContatoEm30Dias, c.clientes)}</div>
                    </td>
                    <td className="mom-num">
                      {nº(c.comContatoEm90Dias)}
                      <div className="cad-sub">{formatarPercentualDaParte(c.comContatoEm90Dias, c.clientes)}</div>
                    </td>
                    <td className="mom-num">
                      <span className={c.nuncaContatados > 0 ? 'cad-atencao' : undefined}>{nº(c.nuncaContatados)}</span>
                    </td>
                    <td className="mom-num">
                      {c.ultimoContatoEm ? formatarData(c.ultimoContatoEm) : <span className="cad-nada">nunca</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </BlocoRecolhivel>

      <BlocoRecolhivel titulo="O que esta tela deixou de afirmar" resumo="canal de contato, o mapa e a tendência dos cartões">
        <div className="cad-fichas">
          <LacunaConhecida
            metrica="Interações por canal (visita, ligação, WhatsApp)"
            motivo={
              'O contato usado nesta tela vem da regra da BI de carteiras do Vórtice (53 resultados ' +
              'que contam como contato), calculada todo dia pela rotina das carteiras — e ela conta ' +
              'qualquer canal. O Vórtice REGISTRA o canal: nos últimos 12 meses, os contatos se ' +
              'dividem em Visita (45%), Externo (18%), Telefone (14%) e WhatsApp (11%). O CRM, porém, ' +
              'ainda não carrega essa coluna — só o resultado, não o canal que o produziu —, e por ' +
              'isso não há como abrir esta lista por canal.'
            }
          />
          <LacunaConhecida
            metrica="Mapa da carteira"
            motivo={
              'O mapa ficava aqui e plotava pinos em Mato Grosso, Goiás e Bahia — geografia do ' +
              'protótipo, não desta operação: as treze filiais estão todas no interior de São ' +
              'Paulo. Falta uma fonte de coordenada por cliente: organizacao.Municipio guarda o ' +
              'município mas não latitude e longitude, e todo cliente segue com latitude 0. O ' +
              'território que existe está em Cobertura por Filial e Carteira.'
            }
          />
          <LacunaConhecida
            metrica="A tendência de cada cartão"
            motivo={
              'A maquete desenha, no canto de cada um dos cinco cartões, a curva da evolução do número. ' +
              'O CRM guarda o último contato de cada vínculo, e não a cobertura de cada dia: sem essa ' +
              'série, a curva seria inventada. Ela entra quando a cobertura passar a ser guardada dia a dia.'
            }
          />
        </div>
      </BlocoRecolhivel>
    </PaginaDoPainel>
  );
}

/**
 * AS BARRAS DE 100% DA MAQUETE — em HTML, e não em canvas: o percentual de cada faixa escrito dentro dela, o total ao
 * lado, e o nome inteiro da carteira para o leitor de tela. A faixa estreita demais para o número fica com a cor; o
 * número está na frase de cada linha e na tabela do fim da tela.
 */
function BarrasDaCobertura({ carteiras }: { carteiras: ResumoDeCobertura[] }) {
  return (
    <div className="cob-barras-corpo">
      <ul className="cob-barras-lista">
        {carteiras.map((c) => {
          const f = faixasDe(c);
          const frase = FAIXAS_DE_CONTATO.map((faixa) => `${faixa.nome.toLowerCase()}: ${fatia(faixa.de(f), f.total) ?? 0}%`).join(
            ', ',
          );
          return (
            <li key={c.carteiraChave} className="cob-barras-linha">
              <span className="cob-barras-nome">{c.carteiraNome}</span>
              <span className="cob-barras-trilho" aria-hidden="true">
                {FAIXAS_DE_CONTATO.map((faixa) => {
                  const p = f.total > 0 ? (100 * faixa.de(f)) / f.total : 0;
                  return p > 0 ? (
                    <span key={faixa.nome} className="cob-barras-faixa" style={{ width: `${p}%`, background: faixa.cor }}>
                      {p >= 6 ? `${Math.round(p)}%` : ''}
                    </span>
                  ) : null;
                })}
              </span>
              <span className="cob-barras-numero">{nº(c.clientes)}</span>
              <span className="cad-so-leitor">
                {c.carteiraNome}, {nº(c.clientes)} vínculos — {frase}.
              </span>
            </li>
          );
        })}
      </ul>
      <div className="cob-barras-eixo" aria-hidden="true">
        {[0, 20, 40, 60, 80, 100].map((t) => (
          <span key={t} style={{ left: `${t}%` }}>
            {t}%
          </span>
        ))}
      </div>
      <ul className="cob-legenda">
        {FAIXAS_DE_CONTATO.map((faixa) => (
          <li key={faixa.nome}>
            <i style={{ background: faixa.cor }} aria-hidden="true" />
            {faixa.nome}
          </li>
        ))}
      </ul>
    </div>
  );
}

/** O MAIOR RISCO, no canto do cartão: a carteira comercial com a maior fatia há mais de 90 dias ou nunca contatada. */
function MaiorRisco({ carteira, exposicao: valor }: { carteira: ResumoDeCobertura; exposicao: number }) {
  return (
    <div className="cob-risco" role="note">
      <span className="cob-risco-icone" aria-hidden="true">
        <CircleAlert size={22} strokeWidth={2.4} />
      </span>
      <span className="cob-risco-texto">
        <strong>Maior risco: {carteira.carteiraNome}</strong>
        <span>{Math.round(valor * 100)}% dos clientes há mais de 90 dias ou nunca contatados.</span>
      </span>
    </div>
  );
}

/**
 * A DISTRIBUIÇÃO DA COBERTURA — a rosca das quatro faixas, de todas as carteiras ou da escolhida, e as carteiras
 * comerciais pela exposição.
 */
function DistribuicaoDaCobertura({
  carteiras,
  total,
  porExposicao,
  carregando,
}: {
  carteiras: ResumoDeCobertura[];
  total: Faixas;
  porExposicao: { carteira: ResumoDeCobertura; exposicao: number }[];
  carregando: boolean;
}) {
  const [escolhida, setEscolhida] = useState('');
  const carteira = carteiras.find((c) => c.carteiraChave === escolhida) ?? null;
  const f = carteira ? faixasDe(carteira) : total;
  const opcoes = useMemo(
    () => [
      { id: '', rotulo: 'Todas as carteiras' },
      ...[...carteiras].sort((a, b) => a.carteiraNome.localeCompare(b.carteiraNome)).map((c) => ({ id: c.carteiraChave, rotulo: c.carteiraNome })),
    ],
    [carteiras],
  );

  return (
    <PainelDoMomento
      titulo="Distribuição da cobertura"
      data-bloco="distribuicao-da-cobertura"
      area="cob-distribuicao"
      dica="Os vínculos pelas quatro faixas de tempo sem contato — de todas as carteiras, ou da escolhida ao lado. As carteiras por maior exposição são as comerciais, pela fatia há mais de 90 dias ou nunca contatada."
      direita={<Seletor rotulo="Carteira da distribuição" rotuloVisivel={false} valor={escolhida} opcoes={opcoes} aoMudar={setEscolhida} />}
    >
      {carregando && <BlocoCarregando oQue="a distribuição da cobertura" />}

      {!carregando && f.total > 0 && (
        <>
          <div className="cob-rosca-e-legenda">
            <GraficoDonutCentro
              segmentos={FAIXAS_DE_CONTATO.map((faixa) => ({ valor: faixa.de(f), cor: faixa.cor }))}
              largura={140}
              altura={140}
              cutout="70%"
              bordaBranca
              centro={{
                linha1: nº(f.total),
                linha2: 'vínculos',
                corLinha1: '#0B1638',
                // O NÚMERO CABE NO MIOLO: o "310" da maquete é grande; o "31.040" de uma filial grande encolhe.
                tamanhoLinha1: f.total >= 10_000 ? 19 : f.total >= 1_000 ? 22 : 26,
                tamanhoLinha2: 12,
                deslocamentoLinha2: 19,
              }}
            />
            <table className="cob-legenda-tabela">
              <caption className="cad-so-leitor">
                A distribuição {carteira ? `da carteira ${carteira.carteiraNome}` : 'de todas as carteiras'}
              </caption>
              <thead className="cad-so-leitor">
                <tr>
                  <th scope="col">Faixa</th>
                  <th scope="col">Vínculos</th>
                  <th scope="col">Parte</th>
                </tr>
              </thead>
              <tbody>
                {FAIXAS_DE_CONTATO.map((faixa) => (
                  <tr key={faixa.nome}>
                    <th scope="row">
                      <i style={{ background: faixa.cor }} aria-hidden="true" />
                      {faixa.nome}
                    </th>
                    <td>{nº(faixa.de(f))}</td>
                    <td>{fatia(faixa.de(f), f.total)}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {porExposicao.length > 0 && (
            <div className="cob-exposicao">
              <h4 className="cob-exposicao-titulo">
                Carteiras por maior exposição <span>( &gt; 90 dias + nunca contatados )</span>
              </h4>
              <ol className="cob-exposicao-lista">
                {porExposicao.slice(0, CARTEIRAS_NO_RANKING).map(({ carteira: c, exposicao: e }, i) => (
                  <li key={c.carteiraChave}>
                    <span className="cob-exposicao-posicao" data-primeira={i === 0 ? 'true' : undefined} aria-hidden="true">
                      {i + 1}
                    </span>
                    <span className="cob-exposicao-nome">{c.carteiraNome}</span>
                    <strong className="cob-exposicao-pct">{Math.round(e * 100)}%</strong>
                    <span className="cob-exposicao-trilho" aria-hidden="true">
                      <span
                        style={{ width: `${Math.round(e * 100)}%` }}
                        data-tom={e >= EXPOSICAO_VERMELHA ? 'alta' : 'media'}
                      />
                    </span>
                  </li>
                ))}
              </ol>
            </div>
          )}
        </>
      )}
    </PainelDoMomento>
  );
}

/** A PRIORIDADE — pela curva ABC do cliente (decisão de 30/09/2026). A seta e a palavra carregam o sentido. */
function SeloDePrioridade({ prioridade, classe }: { prioridade: PrioridadeDeContato; classe: string | null }) {
  const rotulo = prioridade === 'Media' ? 'Média' : prioridade;
  return (
    <span className="cob-prioridade" data-prioridade={prioridade}>
      {prioridade === 'Baixa' ? <i aria-hidden="true" /> : <ArrowUp size={13} strokeWidth={2.6} aria-hidden="true" />}
      {rotulo}
      <span className="cad-so-leitor">, {classe ? `classe ${classe} da curva ABC` : 'cliente sem classe na curva ABC'}</span>
    </span>
  );
}

/**
 * O EXPORTAR DA MAQUETE — a lista inteira, com os mesmos filtros e a mesma busca, em CSV. Vai ao servidor de 200 em 200,
 * até o total ou até {@link LINHAS_NO_EXPORTAR}; passando disso, o botão diz que leva as primeiras.
 */
function BotaoExportar({ consulta, total }: { consulta: ConsultaDeCobertura; total: number }) {
  const { contexto } = useContextoDeAcesso();
  const [exportando, setExportando] = useState(false);
  const [falha, setFalha] = useState<string | null>(null);

  async function exportar() {
    setExportando(true);
    setFalha(null);
    try {
      const { itens } = await listarCoberturaInteira(contexto, consulta);
      baixarCsv(`cobertura-de-carteira-${contexto.empresa}-${carimboDeData()}`, CABECALHO_DO_CSV, itens.map(linhaDoCsv));
    } catch (causa) {
      setFalha(causa instanceof Error ? causa.message : 'A exportação não terminou.');
    } finally {
      setExportando(false);
    }
  }

  return (
    <span className="cob-exportar-caixa">
      <button type="button" className="cob-exportar" onClick={exportar} disabled={exportando || total === 0}>
        <Download size={15} strokeWidth={2.2} aria-hidden="true" />
        {exportando ? 'Exportando…' : 'Exportar'}
      </button>
      {total > LINHAS_NO_EXPORTAR && (
        <InfoTooltip
          rotulo="Quantas linhas o Exportar leva"
          texto={`A lista tem ${nº(total)} vínculos; o Exportar leva os primeiros ${nº(LINHAS_NO_EXPORTAR)}, na ordem da tela. Um filtro ou a busca diminuem a lista.`}
        />
      )}
      {falha && (
        <span className="cad-alerta" role="alert">
          {falha}
        </span>
      )}
    </span>
  );
}

const CABECALHO_DO_CSV = [
  'Cliente',
  'Classe do cliente (curva ABC)',
  'Prioridade',
  'Carteira',
  'Linha de negócio',
  'Responsável',
  'Último contato',
  'Dias sem contato',
  'Ciclo declarado (dias)',
  'Fora do ciclo',
];

function linhaDoCsv(l: CoberturaResumo): unknown[] {
  return [
    l.clienteNome,
    l.classeDoCliente ?? '',
    l.prioridade === 'Media' ? 'Média' : l.prioridade,
    l.carteiraNome,
    l.linhaDeNegocioNome,
    l.responsavelNome,
    l.ultimaInteracaoEm ? formatarData(l.ultimaInteracaoEm) : 'nunca contatado',
    l.diasSemContato ?? '',
    l.diasCicloContato ?? '',
    l.estaForaDoCiclo === null ? '' : l.estaForaDoCiclo ? 'sim' : 'não',
  ];
}

/** A mesma linha, em ficha, para quando a tabela não cabe. */
function FichaDeCobertura({ linha }: { linha: CoberturaResumo }) {
  return (
    <div className="cad-ficha">
      <div className="cad-ficha-titulo">
        <Link to={`/clientes/${linha.clienteChave}`}>{linha.clienteNome}</Link>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Carteira</span>
        <span className="cad-ficha-valor">
          {linha.carteiraNome}
          <div className="cad-sub">{linha.linhaDeNegocioNome}</div>
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Responsável</span>
        <span className="cad-ficha-valor">{linha.responsavelNome}</span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Último contato</span>
        <span className="cad-ficha-valor">
          {linha.ultimaInteracaoEm ? formatarData(linha.ultimaInteracaoEm) : <span className="cad-alerta">nunca</span>}
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Dias sem contato</span>
        <span className="cad-ficha-valor">
          {linha.diasSemContato === null ? <span className="cad-alerta">sem contato registrado</span> : nº(linha.diasSemContato)}
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Prioridade</span>
        <span className="cad-ficha-valor">
          <SeloDePrioridade prioridade={linha.prioridade} classe={linha.classeDoCliente} />
        </span>
      </div>
    </div>
  );
}

function ariaOrdem(campo: OrdemDeCobertura | undefined, consulta: ConsultaDeCobertura) {
  if (!campo || consulta.ordenarPor !== campo) return undefined;
  return consulta.descendente ? ('descending' as const) : ('ascending' as const);
}

/** A seta da coluna ordenada, como na maquete — para cima na ordem crescente. */
function SetaDaOrdem({ campo, consulta }: { campo: OrdemDeCobertura; consulta: ConsultaDeCobertura }) {
  if (consulta.ordenarPor !== campo) return null;
  const Seta = consulta.descendente ? ArrowDown : ArrowUp;
  return <Seta className="cob-seta-da-ordem" size={13} strokeWidth={2.4} aria-hidden="true" />;
}
