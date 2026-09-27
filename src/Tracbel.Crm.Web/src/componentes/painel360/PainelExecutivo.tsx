/**
 * Painel executivo da Visão 360 — a mesma tela do protótipo, com dado real.
 *
 * A marcação e as classes são as de `renderVisao360()`
 * (`prototipo/referencia/assets/app.js`, linha 7409): `v360-container`,
 * `v360-kpi-row`, `v360-grid-row2/3/4`, `v360-card`. Nada de estrutura nova —
 * o que muda em relação ao protótipo é **de onde vem o número**, nunca a forma.
 *
 * OS CINCO CARTÕES (documento 36) vêm de `/relatorios/indicadores-executivos`,
 * lido filial a filial e somado por partição — cada número tem fonte, regra,
 * período e alcance escritos no próprio cartão, e a composição filial a filial
 * abre logo abaixo deles:
 *   A. faturamento da competência mais recente, com e sem cliente no CRM;
 *   B. a meta de VENDA da API Gestão de Negócios × as máquinas vendidas (ART), no ano fiscal até o último
 *      mês fechado — de `/relatorios/metas`, lida filial a filial (#138) — e nunca previsão;
 *   C. clientes únicos (filial de cadastro) e vínculos (filial da carteira);
 *   D. cobertura pela cadência declarada da linha — a regra do mapa;
 *   E. vendas perdidas registradas — sem percentual de mercado.
 *
 * Quando falta dado, o cartão **fica na tela**, no mesmo lugar e no mesmo
 * tamanho, dizendo o que falta — some o número, não o cartão.
 *
 * ---------------------------------------------------------------------------
 * 24/09/2026 — responsivo e com os textos em dia.
 *
 * - AS GRADES QUEBRAM PELA LARGURA DO CONTEÚDO (`painel-executivo.css`), e não
 *   da janela: a Visão 360 mora na `PaginaDoPainel`, a mesma dos Indicadores.
 * - OS QUATRO `title=` VIRARAM DICA (issue 167): a regra de cada cartão, o nome
 *   inteiro e a classe do cliente, e os nomes completos das linhas do mix. O
 *   `title` não abre pelo teclado nem no toque.
 * - A META VOLTOU, E É OUTRA (27/09/2026, #138): a cota de venda da API Gestão de
 *   Negócios, em MÁQUINAS, contra as máquinas do ART que o CRM tem. A meta de
 *   faturamento em reais, que nunca teve fonte, saiu do cartão e da composição.
 * - O ANO É O FISCAL (27/09/2026): novembro a outubro, com o nome do ano em que
 *   termina, e o ano fiscal do último mês fechado como padrão. O servidor apura
 *   pelo mesmo calendário (`anoFiscal` na rota) e para no ÚLTIMO MÊS FECHADO,
 *   como os Indicadores Geográficos: o mês em curso fica no cartão do mês,
 *   marcado como parcial. Em novembro, o padrão é o ano que acabou de fechar.
 * - "PARTICIPAÇÃO DE MERCADO" VIROU "CAPTURA TRACBEL" (issue 162), e o cartão
 *   de mercado diz o que conta: derrotas registradas, e não o tamanho do mercado.
 */
import { useMemo, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import {
  censConsolidados,
  mixDeLinhas,
  obterConsolidado,
  obterExecutivoConsolidado,
  perdasConsolidadas,
  somarConsolidado,
  faturamentoConsolidado,
  vendasPerdidasConsolidadas,
  type ExecutivoConsolidado,
} from '../../dados/api/consolidado';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterMetasConsolidadas, type MetasConsolidadas } from '../../dados/api/metas';
import { useRecurso } from '../../dados/api/useRecurso';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { GraficoBarrasHorizontais } from '../GraficoBarrasHorizontais';
import { InfoTooltip } from '../InfoTooltip';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { GraficoDonutCentro } from '../GraficoDonutCentro';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import '../../estilos/painel-executivo.css';

/**
 * COMO A META DE VENDA SE CONTA — a frase é uma só, no cartão e na composição (#138, decisões de 27/09/2026).
 *
 * A meta é a cota da API Gestão de Negócios, em máquinas; o realizado são as máquinas do ART que o CRM tem. As vendas
 * do ART que o CRM ainda não tem vêm à parte, em número, e o consórcio é em cotas, sem realizado.
 */
const REGRA_DA_META =
  'Meta: a cota da API Gestão de Negócios, em máquinas, por consultor, linha, mês e filial. ' +
  'Realizado: as máquinas vendidas que o CRM tem, lidas do ART, pela data da venda — por consultor, conta o vendedor da venda. ' +
  'As vendas que aguardam na integração do ART (cadastro, chassi ou outro motivo) não entram no realizado e aparecem à parte. ' +
  'Consórcio: meta em cotas; o realizado de consórcio não é medido pelo CRM. ' +
  'Período: o ano fiscal (novembro a outubro) até o último mês fechado, comparado com o mesmo trecho do ano fiscal anterior; o mês em curso vem à parte. ' +
  'Previsão: não calculada — nenhum modelo aprovado.';

/** A permissão que falta, dita como a tela de perfis a chama. */
const SEM_PERMISSAO_DA_META =
  'Você não tem a permissão de ler a meta de venda (Meta.Ler). Quem administra o CRM concede pelo perfil.';

/**
 * O ANO DOS CARTÕES, na dica ao lado de "ano fiscal".
 *
 * O ano fiscal da Tracbel foi confirmado em 24/09/2026 — novembro a outubro,
 * com o nome do ano em que termina — e virou o período padrão das telas em
 * 27/09/2026. A dica dizia "ano civil… a visão por FY vem na próxima etapa": a
 * etapa chegou, e o servidor apura pelo mesmo calendário.
 */
const DICA_DO_ANO_FISCAL =
  'Os cartões somam o ANO FISCAL da Tracbel, de novembro a outubro, com o nome do ano em que termina: o FY2026 vai de ' +
  'nov/2025 a out/2026. O ano em curso é somado até o ÚLTIMO MÊS FECHADO, como nos Indicadores Geográficos; o mês em ' +
  'curso, pela metade, fica à parte, no cartão de faturamento em curso. É o período padrão desde 27/09/2026.';

/**
 * O ano fiscal do ÚLTIMO MÊS FECHADO — o padrão (27/09/2026). Em novembro é o ano
 * que acabou de fechar: o novo ainda não tem mês fechado para somar.
 */
function anoFiscalDoUltimoMesFechado(hoje: Date): number {
  const ultimoFechado = new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1);
  return ultimoFechado.getMonth() + 1 >= 11 ? ultimoFechado.getFullYear() + 1 : ultimoFechado.getFullYear();
}

/** "FY2026 (nov/2025 a out/2026)" — o nome do ano fiscal nunca aparece sem o intervalo. */
function nomeDoAno(ano: number): string {
  return `FY${ano} (nov/${ano - 1} a out/${ano})`;
}

/**
 * O valor em milhões, como a diretoria fala dele.
 *
 * "R$ 15,9 M" cabe num cartão e num eixo de gráfico; "R$ 15.928.084,70" não cabe em nenhum dos
 * dois e ninguém lê os centavos de um total de filial.
 */
function emMilhoes(valor: number): string {
  if (Math.abs(valor) >= 1_000_000) {
    return `R$ ${(valor / 1_000_000).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} M`;
  }

  return `R$ ${(valor / 1_000).toLocaleString('pt-BR', { maximumFractionDigits: 0 })} mil`;
}

/** `2026-09-01` vira `set/2026`. */
function mesPorExtenso(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${MESES[Number(mes) - 1]}/${ano}`;
}

/** `2026-09-01` vira `set/26` — o rótulo curto do eixo. */
function mesCurto(competencia: string): string {
  const [ano, mes] = competencia.slice(0, 7).split('-');
  return `${MESES[Number(mes) - 1]}/${ano.slice(2)}`;
}

/** `2026-09-08T18:46:07Z` vira `08/09 15:46`, no fuso de quem lê. */
function diaEHora(instante: string | null): string {
  if (!instante) return 'data não informada';
  const utc = /Z|[+-]\d\d:\d\d$/.test(instante) ? instante : `${instante}Z`;
  return new Date(utc).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
}

/** `2026-03-10` vira `10/03/2026`. */
function data(dia: string | null): string {
  if (!dia) return '—';
  const [a, m, d] = dia.slice(0, 10).split('-');
  return `${d}/${m}/${a}`;
}

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/** O primeiro ano do seletor: antes disso não há faturamento carregado. É o mesmo limite da API. */
const PRIMEIRO_ANO = 2020;

/**
 * A cor da classe na curva ABC.
 *
 * Verde escuro em A e cinza em D não é juízo sobre o cliente: é a mesma escala de intensidade
 * que o resto do painel usa para "pesa mais" e "pesa menos". D quer dizer "não comprou na
 * janela", e é justamente quem a cobertura existe para atacar.
 */
function corDaClasse(classe: string | null): string {
  switch (classe) {
    case 'A':
      return '#1B5E20';
    case 'B':
      return '#367C2B';
    case 'C':
      return '#6FBF5E';
    default:
      return '#9CA3AF';
  }
}

/** As cores dos estados da cobertura pela cadência — as mesmas do mapa: verde coberto, vermelho pendente. */
const COR_COBERTO = '#367C2B';
const COR_FORA_DA_CADENCIA = '#DC2626';
const COR_NUNCA = '#7F1D1D';
const COR_SEM_CADENCIA = '#D1D5DB';

/** As cores do mix por linha, na paleta John Deere do protótipo. */
const CORES_MIX = [
  '#367C2B', '#4A9B3D', '#6FBF5E', '#FFDE00', '#F59E0B', '#0EA5E9', '#8B5CF6', '#DC2626',
] as const;


/**
 * Os cinco ícones dos indicadores, porte literal de `iconeKPI360()`
 * (`prototipo/referencia/assets/app.js`, linha 7660).
 */
const ICONES = {
  'trending-up': (
    <>
      <polyline points="23 6 13.5 15.5 8.5 10.5 1 18" />
      <polyline points="17 6 23 6 23 12" />
    </>
  ),
  target: (
    <>
      <circle cx="12" cy="12" r="10" />
      <circle cx="12" cy="12" r="6" />
      <circle cx="12" cy="12" r="2" />
    </>
  ),
  users: (
    <>
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="4" />
      <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </>
  ),
  shield: <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />,
  eye: (
    <>
      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
      <circle cx="12" cy="12" r="3" />
    </>
  ),
} as const;

type NomeDeIcone = keyof typeof ICONES;

function IconeDoIndicador({ nome }: { nome: NomeDeIcone }) {
  return (
    <svg viewBox="0 0 24 24" width={16} height={16} fill="none" stroke="currentColor" strokeWidth={2.2}>
      {ICONES[nome]}
    </svg>
  );
}

/** As cores dos avatares do ranking, na ordem do protótipo. */
const CORES_AVATAR = ['#367C2B', '#1B5E20', '#0EA5E9', '#7C3AED', '#DB2777'] as const;

/**
 * Tira o "Venda de " que abre quase todas as linhas de negócio do legado.
 * O cartão tem uma coluna estreita e o prefixo se repete em oito de nove
 * rótulos — os nomes inteiros ficam na dica do título do cartão, que abre pelo
 * ponteiro, pelo teclado e pelo toque (era um `title` em cada rótulo).
 */
function semPrefixoDeVenda(nome: string): string {
  return nome.replace(/^Venda(s)? de\s+/i, '').replace(/^Venda(s)?\s+/i, '');
}

/** As duas primeiras iniciais do nome, para o avatar redondo. */
function iniciais(nome: string): string {
  const partes = nome.trim().split(/\s+/).filter(Boolean);
  if (partes.length === 0) return '—';
  if (partes.length === 1) return partes[0].slice(0, 2).toUpperCase();
  return (partes[0][0] + partes[partes.length - 1][0]).toUpperCase();
}

/** O protótipo mostra cinco no ranking. */
const TOP = 5;

const nº = (v: number) => v.toLocaleString('pt-BR');
const porcento = (v: number) => `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;

export function PainelExecutivo() {
  const { contexto } = useContextoDeAcesso();
  const anoCorrente = anoFiscalDoUltimoMesFechado(new Date());
  const [ano, setAno] = useState(anoCorrente);

  const consolidado = useRecurso((sinal) => obterConsolidado(contexto, sinal), [contexto.usuario]);
  const executivo = useRecurso((sinal) => obterExecutivoConsolidado(contexto, ano, sinal), [contexto.usuario, ano]);
  // A META DE VENDA TEM LEITURA PRÓPRIA (#138): o período dela é o ano fiscal, e não o ano do seletor, e a falha dela não
  // derruba os outros cartões.
  const metas = useRecurso((sinal) => obterMetasConsolidadas(contexto, sinal), [contexto.usuario]);

  const dados = consolidado.dados;
  const total = useMemo(() => somarConsolidado(dados), [dados]);
  const mix = useMemo(() => mixDeLinhas(dados), [dados]);
  const cens = useMemo(() => censConsolidados(dados), [dados]);
  const perdas = useMemo(() => perdasConsolidadas(dados), [dados]);
  const vendasPerdidas = useMemo(() => vendasPerdidasConsolidadas(dados), [dados]);
  const faturamento = useMemo(() => faturamentoConsolidado(dados), [dados]);

  const ex = executivo.dados;
  const cobertura = ex?.cobertura ?? null;
  const coberturaPct = cobertura && cobertura.elegiveis > 0 ? (100 * cobertura.cobertos) / cobertura.elegiveis : null;

  /* A rosca de cobertura mostra os estados da CADÊNCIA — os mesmos do mapa —, e não faixas de dias. */
  const fatiasDaCobertura = useMemo(
    () =>
      cobertura
        ? [
            { valor: cobertura.cobertos, cor: COR_COBERTO },
            { valor: cobertura.foraDaCadencia, cor: COR_FORA_DA_CADENCIA },
            { valor: cobertura.nuncaContatados, cor: COR_NUNCA },
            { valor: cobertura.semCadencia, cor: COR_SEM_CADENCIA },
          ]
        : [],
    [cobertura],
  );

  const topCens = useMemo(() => cens.slice(0, TOP), [cens]);
  const maiorCen = topCens[0]?.clientes ?? 1;

  const totalDoMix = mix.slice(0, TOP + 3).reduce((s, l) => s + l.clientes, 0);
  const segmentosDoMix = useMemo(
    () => mix.slice(0, TOP + 3).map((l, i) => ({ valor: l.clientes, cor: CORES_MIX[i % CORES_MIX.length] })),
    [mix],
  );

  /*
   * AS BARRAS SAEM DO FORMULÁRIO, E NÃO DO PROCESSO.
   *
   * Elas liam `perdasConsolidadas`, que agrupa `processo.Processo` por motivo — e ali o motivo
   * é "não informado" em 100% das linhas, porque o Vórtice encerra o processo sem coluna de
   * motivo. Era essa consulta, e não o legado, que fazia a tela dizer "o legado não declara o
   * motivo de nenhuma delas".
   *
   * O motivo mora no formulário de venda perdida (`IV_Q_VENDA_PERDIDA_FY25`, 165 respostas em
   * 2026), que agora está em `processo.VendaPerdida`. O total do subtítulo continua sendo o de
   * processos perdidos: é ele que dá a dimensão, e a diferença entre os dois números — quantas
   * derrotas ninguém registrou — é o que o rodapé do cartão diz.
   */
  const barrasDePerda = useMemo(
    () =>
      vendasPerdidas.porMotivo.slice(0, 6).map((m, i) => ({
        rotulo: m.nome,
        valor: m.quantidade,
        cor: CORES_MIX[i % CORES_MIX.length],
        tooltipLinhas: [
          m.nome,
          `${nº(m.quantidade)} venda(s) perdida(s)`,
          m.diferencaMediaDePreco !== null
            ? `Nosso preço ficou R$ ${nº(Math.round(m.diferencaMediaDePreco))} acima, em média de ${m.comOsDoisPrecos}`
            : 'Sem os dois preços declarados',
        ],
      })),
    [vendasPerdidas],
  );
  const totalPerdido = perdas.reduce((s, m) => s + m.quantidade, 0);

  if (consolidado.carregando) return <BlocoCarregando oQue="o consolidado das filiais" />;

  const anos = Array.from({ length: anoCorrente - PRIMEIRO_ANO + 1 }, (_, i) => anoCorrente - i);

  return (
    <div className="v360-container">
      {consolidado.erro && (
        <BlocoErro erro={consolidado.erro} aoTentarDeNovo={consolidado.recarregar} />
      )}

      {/* O PERÍODO DOS CARTÕES, ESCRITO. O ano segue a escolha — não fica preso a 2026 —, e é o
          FISCAL (nov→out), o período padrão desde 27/09/2026. O nome do ano aparece sempre com o
          intervalo ao lado: "FY2026" sozinho se lê como ano civil. */}
      <div className="v360-periodo" role="group" aria-label="Período dos indicadores" data-bloco="periodo">
        <label>
          Ano fiscal de referência
          <select value={ano} onChange={(e) => setAno(Number(e.target.value))}>
            {anos.map((a) => (
              <option key={a} value={a}>
                {nomeDoAno(a)}
              </option>
            ))}
          </select>
        </label>
        <span className="v360-periodo-regra">
          ano fiscal (nov–out)
          <InfoTooltip texto={DICA_DO_ANO_FISCAL} rotulo="Como o ano fiscal é contado" />
        </span>
        <span>o faturamento do mês é a competência mais recente carregada</span>
        {ex && (
          <span className={ex.respondidas < ex.filiais.length ? 'v360-periodo-alerta' : undefined}>
            {ex.respondidas} de {ex.filiais.length} filiais em operação responderam
          </span>
        )}
      </div>

      {executivo.erro && <BlocoErro erro={executivo.erro} aoTentarDeNovo={executivo.recarregar} />}
      {ex && ex.respondidas === 0 && ex.filiais.length > 0 && (
        <BlocoErro
          erro={ex.filiais.find((f) => f.erro)?.erro ?? new Error('Nenhuma filial respondeu.')}
          aoTentarDeNovo={executivo.recarregar}
        />
      )}

      {/* ROW 1: os cinco indicadores ------------------------------------- */}
      {executivo.carregando ? (
        <BlocoCarregando oQue="os cinco indicadores das filiais" />
      ) : (
        <CincoIndicadores
          ex={ex && ex.respondidas > 0 ? ex : null}
          metas={metas.dados}
          carregandoMetas={metas.carregando}
          // A CONTAGEM DE CONCORRENTES VEM DE OUTRA LEITURA (o consolidado). Ela só aparece quando as duas
          // leituras estão completas — senão o cartão juntaria um total de dez filiais com um de treze.
          concorrentes={
            ex && dados && ex.respondidas === ex.filiais.length && dados.falhas === 0
              ? vendasPerdidas.porConcorrente.length
              : null
          }
        />
      )}

      {ex && ex.respondidas > 0 && <ComposicaoDosIndicadores ex={ex} metas={metas.dados} />}

      {/* ROW 2: conhecimento · status da cobertura · faturamento --------- */}
      <div className="v360-grid-row2" data-bloco="linha-2">
        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Conhecimento de mercado</div>
              <div className="v360-card-sub">Para quem perdemos, pelo formulário de venda perdida</div>
            </div>
          </div>
          {vendasPerdidas.porConcorrente.length > 0 ? (
            <>
              <ul className="v360-concorrentes">
                {vendasPerdidas.porConcorrente.slice(0, TOP).map((c) => (
                  <li key={c.codigo}>
                    <span>{c.nome}</span>
                    <span>
                      {nº(c.quantidade)} perda(s) · {nº(c.maquinas)} máq.
                    </span>
                  </li>
                ))}
              </ul>
              {/* CAPTURA TRACBEL, E NÃO "PARTICIPAÇÃO DE MERCADO" (issue 162). Este cartão conta derrotas;
                  a parte da demanda que a Tracbel leva é medida nos Indicadores, com as unidades do ART. */}
              <p className="v360-nota">
                {nº(vendasPerdidas.registradas)} formulários de {nº(vendasPerdidas.processosPerdidos)} processos perdidos.
                A <strong>Captura Tracbel</strong> — máquinas vendidas sobre a demanda estimada — é medida nos{' '}
                <Link to="/relatorios/territorio" className="v360-link">
                  Indicadores geográficos
                </Link>
                .
              </p>
            </>
          ) : (
            <SemDado
              oQue="o mercado"
              porque="Nenhuma venda perdida com concorrente registrada no formulário do CEN, nas filiais que responderam."
            />
          )}
        </div>

        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Status da cobertura</div>
              <div className="v360-card-sub">
                {cobertura
                  ? `${nº(cobertura.vinculosComerciais)} vínculos em carteira comercial, pela cadência da linha`
                  : 'pela cadência declarada da linha de negócio'}
              </div>
            </div>
            <Link to="/relatorios/territorio" className="v360-link">
              Ver no mapa →
            </Link>
          </div>
          {/* SEM VÍNCULO, SEM ROSCA: com tudo em zero ela desenhava um anel vazio, um "—" no meio e
              quatro zeros na legenda — quatro afirmações para dizer uma coisa só. */}
          {cobertura && cobertura.vinculosComerciais === 0 ? (
            <SemDado oQue="a cobertura" porque="Nenhum vínculo em carteira comercial, nas filiais que responderam." />
          ) : cobertura ? (
            <div className="v360-donut-wrap">
              <GraficoDonutCentro
                segmentos={fatiasDaCobertura}
                /* 160 e não os 200 do protótipo: a legenda real traz números de
                   cinco dígitos ("15.560"), e não "8". O donut cede a diferença. */
                largura={160}
                altura={160}
                cutout="70%"
                bordaBranca
                centro={{ linha1: coberturaPct === null ? '—' : porcento(coberturaPct), linha2: 'no prazo' }}
              />
              <div className="v360-donut-legenda">
                <ItemDeLegenda cor={COR_COBERTO} rotulo="No prazo da cadência" valor={cobertura.cobertos} />
                <ItemDeLegenda cor={COR_FORA_DA_CADENCIA} rotulo="Fora da cadência" valor={cobertura.foraDaCadencia} />
                <ItemDeLegenda cor={COR_NUNCA} rotulo="Nunca contatados" valor={cobertura.nuncaContatados} />
                <ItemDeLegenda cor={COR_SEM_CADENCIA} rotulo="Linha sem cadência (fora do %)" valor={cobertura.semCadencia} />
              </div>
            </div>
          ) : (
            <SemDado oQue="a cobertura" porque="A leitura dos indicadores das filiais não respondeu." />
          )}
        </div>

        <div className="v360-card v360-card-lg">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Faturamento — 12 meses</div>
              <div className="v360-card-sub">
                {faturamento.serie.length > 0
                  ? `${mesPorExtenso(faturamento.serie[0].competencia)} a ${mesPorExtenso(faturamento.competenciaMaisRecente!)} · notas com cliente no CRM`
                  : 'nota fiscal de saída, lida do Protheus'}
              </div>
            </div>
          </div>

          {/* O SUBTÍTULO ESCREVE O PERÍODO, SEMPRE. Se a carga do ERP parar de novo, a série
              para de avançar e o período denuncia — em vez de mostrar um total plausível e
              velho, que foi o defeito que passou dezessete meses sem ninguém notar. */}
          {faturamento.serie.length > 0 ? (
            <MolduraDeGrafico altura={200}>
              {(l, a) => (
                <GraficoLinhaMensal
                  rotulos={faturamento.serie.map((m) => mesCurto(m.competencia))}
                  valores={faturamento.serie.map((m) => m.valorLiquido)}
                  largura={l}
                  altura={a}
                  formatar={emMilhoes}
                  ultimoParcial={faturamento.ultimoMesEstaAberto}
                />
              )}
            </MolduraDeGrafico>
          ) : (
            <SemDado
              oQue="o faturamento"
              porque="Nenhuma nota carregada. A carga lê a SD2 do Protheus; se ela não rodou com a ponte configurada, não há série para desenhar."
            />
          )}
        </div>
      </div>

      {/* ROW 3: top clientes · top CENs · mix por linha ------------------- */}
      <div className="v360-grid-row3" data-bloco="linha-3">
        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Top 5 clientes</div>
              <div className="v360-card-sub">
                Maior faturamento acumulado · classe da curva ABC
              </div>
            </div>
          </div>

          {faturamento.topClientes.length > 0 ? (
            <div className="v360-topbar-list">
              {faturamento.topClientes.map((cliente, i) => (
                <div className="v360-topbar-item" key={cliente.clienteChave}>
                  <div className="v360-topbar-pos">#{i + 1}</div>
                  {/* A LETRA NO LUGAR DA INICIAL. Nos outros rankings o quadrado traz as
                      iniciais da pessoa; aqui ele traz a classe da curva ABC, que é a
                      informação que qualifica o cliente — e ela é apurada do mesmo
                      faturamento que ordena a lista. O quadrado é decoração: a classe
                      por extenso está na dica do nome, e o leitor de tela a lê lá. */}
                  <div
                    className="v360-topbar-avatar"
                    style={{ background: corDaClasse(cliente.classe) }}
                    aria-hidden="true"
                  >
                    {cliente.classe ?? '—'}
                  </div>
                  <div className="v360-topbar-info">
                    {/* O NOME CORTADO COM RETICÊNCIAS ABRE O NOME INTEIRO (issue 167). Era
                        um `title`, que não abre pelo teclado nem no toque — e razão social
                        de cooperativa é justamente o nome que não cabe. */}
                    <InfoTooltip
                      texto={`${cliente.nome} — ${cliente.classe ? `classe ${cliente.classe}` : 'classe não apurada'} na curva ABC.`}
                      rotulo={`${cliente.nome}, ${cliente.classe ? `classe ${cliente.classe}` : 'classe não apurada'}`}
                    >
                      <span className="v360-topbar-nome">{cliente.nome}</span>
                    </InfoTooltip>
                    <div className="v360-topbar-meta">
                      {cliente.ultimaCompraEm
                        ? `última compra em ${mesPorExtenso(cliente.ultimaCompraEm)}`
                        : 'sem compra na janela'}
                    </div>
                  </div>
                  <div className="v360-topbar-valor">{emMilhoes(cliente.valorLiquido)}</div>
                </div>
              ))}
            </div>
          ) : (
            <SemDado
              oQue="o ranking de clientes"
              porque="Nenhum faturamento carregado para ordenar os clientes."
            />
          )}
        </div>

        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Top CENs</div>
              <div className="v360-card-sub">Ranking por vínculos em carteira comercial</div>
            </div>
          </div>
          {/* SEM CARTEIRA, O CARTÃO DIZ POR QUÊ — como os vizinhos. Vazio, ele ficava só com o título,
              que é o jeito de um painel parecer que não carregou. */}
          {topCens.length === 0 && (
            <SemDado
              oQue="o ranking de CENs"
              porque="Nenhum vínculo em carteira comercial de pessoa ou área, nas filiais que responderam."
            />
          )}
          <div className="v360-topbar-list">
            {topCens.map((c, i) => (
              <div className="v360-topbar-item" key={c.nome}>
                <div className="v360-topbar-rank">#{i + 1}</div>
                <div
                  className="v360-topbar-avatar"
                  style={{ background: CORES_AVATAR[i % CORES_AVATAR.length] }}
                >
                  {iniciais(c.nome)}
                </div>
                <div className="v360-topbar-info">
                  <div className="v360-topbar-nome">{c.nome}</div>
                  <div className="v360-topbar-meta">
                    {nº(c.carteiras)} {c.carteiras === 1 ? 'carteira' : 'carteiras'} ·{' '}
                    {nº(c.em30)} com contato em 30 dias
                  </div>
                </div>
                <div className="v360-topbar-valor-wrap">
                  <div className="v360-topbar-valor">{nº(c.clientes)}</div>
                  <div className="v360-topbar-progress">
                    <div
                      className="v360-topbar-fill"
                      style={{
                        width: `${Math.round((c.clientes / maiorCen) * 100)}%`,
                        background: i === 0 ? '#367C2B' : i === 1 ? '#4A9040' : '#7CB342',
                      }}
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">
                Mix por linha
                <InfoTooltip
                  rotulo="Como o mix por linha é contado"
                  texto={
                    <>
                      <p>
                        A parte de cada linha de negócio nos vínculos das carteiras comerciais. Carteira administrativa e
                        de teste ficam de fora: são depósito de cadastro, e não carteira de ninguém.
                      </p>
                      {mix.length > 0 && (
                        <p>Nomes completos, na ordem da legenda: {mix.slice(0, TOP + 3).map((l) => l.nome).join('; ')}.</p>
                      )}
                    </>
                  }
                />
              </div>
              <div className="v360-card-sub">Participação de cada linha nos vínculos das carteiras comerciais</div>
            </div>
          </div>
          {mix.length > 0 ? (
            <div className="v360-mix-wrap">
              <GraficoDonutCentro segmentos={segmentosDoMix} largura={140} altura={140} cutout="72%" bordaBranca tooltipUnidade="%" />
              <div className="v360-mix-legenda">
                {mix.slice(0, TOP + 3).map((l, i) => (
                  <div className="v360-legenda-item" key={l.nome}>
                    <span className="dot" style={{ background: CORES_MIX[i % CORES_MIX.length] }} />
                    <span className="v360-mix-linha">{semPrefixoDeVenda(l.nome)}</span>
                    <strong>
                      {totalDoMix > 0 ? Math.round((l.clientes / totalDoMix) * 100) : 0}%
                    </strong>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <SemDado oQue="o mix por linha" porque="Nenhum vínculo em carteira comercial, nas filiais que responderam." />
          )}
        </div>
      </div>

      {/* ROW 4: vendas perdidas · alertas gerenciais ---------------------- */}
      <div className="v360-grid-row4" data-bloco="linha-4">
        <div className="v360-card v360-card-lg">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Vendas perdidas por motivo</div>
              <div className="v360-card-sub">
                {nº(totalPerdido)} processos perdidos no período
              </div>
            </div>
          </div>
          <div className="v360-perdidas-wrap">
            {barrasDePerda.length > 0 ? (
              <>
                {/* A moldura mede o cartão. Com largura fixa de 600px o gráfico
                    passava por cima da borda direita — o cartão tem menos que isso
                    a 1.280px de viewport. */}
                <MolduraDeGrafico altura={Math.max(160, barrasDePerda.length * 30 + 50)}>
                  {(l, a) => <GraficoBarrasHorizontais itens={barrasDePerda} largura={l} altura={a} />}
                </MolduraDeGrafico>
                <p className="v360-perdidas-nota">
                  De <strong>{nº(vendasPerdidas.registradas)}</strong> derrotas com formulário
                  preenchido. As outras{' '}
                  <strong>{nº(Math.max(0, totalPerdido - vendasPerdidas.registradas))}</strong>{' '}
                  foram encerradas sem ninguém registrar o motivo.
                </p>
              </>
            ) : (
              <SemDado
                oQue="o motivo da perda"
                // "OS 0 PROCESSOS PERDIDOS EXISTEM" não é frase: sem processo perdido, o que falta
                // não é o formulário, é a perda.
                porque={
                  totalPerdido > 0
                    ? `Os ${nº(totalPerdido)} processos perdidos existem, e nenhum deles tem o formulário de venda perdida preenchido.`
                    : 'Nenhum processo perdido nas filiais que responderam.'
                }
              />
            )}
          </div>
        </div>

        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Alertas gerenciais</div>
              <div className="v360-card-sub">Para atenção neste perfil</div>
            </div>
          </div>
          <div className="v360-alertas-list">
            {cobertura && (
              <Alerta
                tipo="critico"
                titulo={`${nº(cobertura.nuncaContatados)} vínculos elegíveis nunca contatados`}
                detalhe={`De ${nº(cobertura.elegiveis)} vínculos em linha com cadência declarada`}
                acao="Ver cobertura"
                para="/cobertura"
              />
            )}
            <Alerta
              tipo="aviso"
              titulo={`${nº(total.atrasadas)} tarefas atrasadas`}
              detalhe={`De ${nº(total.pendentes)} pendentes no total`}
              acao="Ver agenda"
              para="/agenda"
            />
            <Alerta
              tipo="info"
              titulo={`${nº(total.abertos)} processos abertos`}
              detalhe={`${nº(total.ganhos)} ganhos e ${nº(total.perdidos)} perdidos no período`}
              acao="Ver funil"
              para="/relatorios/funil"
            />
          </div>
        </div>
      </div>
    </div>
  );
}

/* ------------------------------------------------------------------------ */

/**
 * B. A META DE VENDA × O REALIZADO (#138) — em MÁQUINAS, no ano fiscal até o último mês fechado.
 *
 * Tem leitura própria (`/relatorios/metas`), e por isso aparece mesmo quando os indicadores das filiais não responderam.
 * No alcance Próprios — o vendedor, no perfil Padrão — é a meta dele; sem `Meta.Ler`, o cartão diz que falta a permissão,
 * e não "meta zero".
 */
function CartaoDaMeta({ metas, carregando }: { metas: MetasConsolidadas | null; carregando: boolean }) {
  const titulo = metas?.periodo ? `Meta e realizado · FY${metas.periodo.anoFiscal}` : 'Meta e realizado';

  if (!metas || metas.respondidas === 0) {
    const motivo = carregando
      ? 'lendo a meta das filiais…'
      : metas?.semPermissao
        ? SEM_PERMISSAO_DA_META
        : 'a leitura da meta das filiais não respondeu';
    return <CartaoSemDado titulo={titulo} cor="#1B5E20" motivo={motivo} icone="target" />;
  }

  const { periodo, metaMaquinas: meta, realizadoMaquinas: realizado, origem } = metas;
  const proprio = metas.alcance === 'Proprios';
  const pct = meta > 0 ? (100 * realizado) / meta : null;

  // O CADASTRO NÃO LIDO NÃO É "META ZERO": a rotina das metas ainda não rodou, e o cartão diz o que falta.
  if (origem === null) {
    return (
      <CartaoIndicador
        titulo={titulo}
        valor="—"
        subtexto="o cadastro de metas da Gestão de Negócios ainda não foi lido"
        detalhe={`realizado: ${nº(realizado)} máquina(s) em ${periodo?.texto ?? 'período'} · a meta entra quando a rotina das metas rodar`}
        dica={REGRA_DA_META}
        cor="#1B5E20"
        icone="target"
      />
    );
  }

  const partes = [
    metas.pendentesNoArt ? `${nº(metas.pendentesNoArt)} vendas aguardam na integração do ART (cadastro, chassi ou outro motivo)` : null,
    metas.metaConsorcio > 0 ? `consórcio: ${nº(metas.metaConsorcio)} cotas, realizado não medido` : null,
    `mesmo trecho do FY anterior: ${nº(metas.realizadoNoAnterior)}`,
    metas.mesEmCurso
      ? `${mesPorExtenso(metas.mesEmCurso.competencia)} em curso: ${nº(metas.mesEmCurso.realizadoMaquinas)} de ${nº(metas.mesEmCurso.metaMaquinas)}`
      : null,
  ].filter((p): p is string => p !== null);

  // A FILIAL QUE FALHOU SAI DA SOMA, E NÃO EM SILÊNCIO (revisão do PR #248): o subtexto diz quantas responderam e
  // quais ficaram fora — a que disse 403 está fora do alcance, e não é falha.
  const esperadas = metas.respondidas + metas.falhas.length;

  return (
    <CartaoIndicador
      titulo={titulo}
      valor={meta > 0 ? `${nº(realizado)} de ${nº(meta)}` : nº(realizado)}
      subtexto={
        <>
          {`${proprio ? 'Sua meta · máquinas vendidas pela sua filial' : 'máquinas vendidas'}${pct === null ? ' · sem meta no período' : ` · ${porcento(pct)} da meta`} · ${periodo?.texto ?? ''}`}
          {metas.falhas.length > 0 && (
            <span className="v360-periodo-alerta">
              {` · ${metas.respondidas} de ${esperadas} filiais — fora: ${metas.falhas.map((f) => f.nome).join(', ')}`}
            </span>
          )}
        </>
      }
      detalhe={partes.join(' · ')}
      dica={[
        REGRA_DA_META,
        `Cadastro lido em ${diaEHora(origem.lidaEm)}${proprio ? '' : `, ${metas.respondidas} filial(is)`}.`,
        ...metas.observacoes,
      ].join(' ')}
      cor="#1B5E20"
      icone="target"
    />
  );
}

/** Os cinco cartões, cada um com fonte, período e alcance no rodapé — e o motivo quando falta dado. */
function CincoIndicadores({
  ex,
  metas,
  carregandoMetas,
  concorrentes,
}: {
  ex: ExecutivoConsolidado | null;
  metas: MetasConsolidadas | null;
  carregandoMetas: boolean;
  concorrentes: number | null;
}) {
  if (!ex) {
    const motivo = 'a leitura dos indicadores das filiais não respondeu';
    return (
      <div className="v360-kpi-row" data-bloco="kpis">
        <CartaoSemDado titulo="Faturamento do mês" cor="#367C2B" motivo={motivo} icone="trending-up" />
        <CartaoDaMeta metas={metas} carregando={carregandoMetas} />
        <CartaoSemDado titulo="Clientes na carteira" cor="#0EA5E9" motivo={motivo} icone="users" />
        <CartaoSemDado titulo="Cobertura pela cadência" cor="#7C3AED" motivo={motivo} icone="shield" />
        <CartaoSemDado titulo="Conhecimento de mercado" cor="#B45309" motivo={motivo} icone="eye" destaque />
      </div>
    );
  }

  const mes = ex.faturamentoDoMes;
  const hoje = new Date();
  const mesCorrente = `${hoje.getFullYear()}-${String(hoje.getMonth() + 1).padStart(2, '0')}`;
  const mesEmCurso = mes !== null && mes.competencia.slice(0, 7) === mesCorrente;
  const alcance = `${ex.respondidas} filiais`;

  const cobertura = ex.cobertura;
  const coberturaPct = cobertura.elegiveis > 0 ? (100 * cobertura.cobertos) / cobertura.elegiveis : null;
  const mercado = ex.mercado;
  const carteira = ex.carteira;

  return (
    <div className="v360-kpi-row" data-bloco="kpis">
      {/* A. FATURAMENTO — nota de saída do Protheus, com e sem cliente no CRM. O ART não entra: ele
          registra venda de máquina, não nota, e somar os dois contaria a mesma máquina duas vezes. */}
      {mes ? (
        <CartaoIndicador
          titulo={mesEmCurso ? 'Faturamento em curso' : 'Faturamento do mês'}
          valor={emMilhoes(mes.total)}
          subtexto={
            mesEmCurso
              ? `${mesPorExtenso(mes.competencia)} até a carga de ${diaEHora(mes.carregadoEm)} · ${alcance}`
              : `${mesPorExtenso(mes.competencia)} · última competência carregada · ${alcance}`
          }
          detalhe={`NF de saída (Protheus): com cliente ${emMilhoes(mes.comCliente)} + sem cliente no CRM ${emMilhoes(mes.semCliente)} · devolução não abatida${
            mes.filiaisEmOutroMes.length > 0 ? ` · fora da soma, em outro mês: ${mes.filiaisEmOutroMes.join(', ')}` : ''
          }`}
          dica={`Sem cliente no CRM: contraparte sem cadastro ${emMilhoes(mes.contraparteSemCadastro)}, fábrica ${emMilhoes(mes.repasseDeFabrica)}, empresa do grupo ${emMilhoes(mes.empresaDoGrupo)}, outra revenda ${emMilhoes(mes.outraRevenda)}. Por grupo de item: máquina ${emMilhoes(mes.maquina)}, peça ${emMilhoes(mes.peca)}, serviço ${emMilhoes(mes.servico)}, outros ${emMilhoes(mes.outros)}. ${nº(mes.notas)} notas.`}
          cor="#367C2B"
          icone="trending-up"
        />
      ) : (
        <CartaoSemDado titulo="Faturamento do mês" cor="#367C2B" motivo="nenhuma nota carregada nas filiais que responderam" icone="trending-up" />
      )}

      {/* B. META E REALIZADO — a meta de VENDA da API Gestão de Negócios × as máquinas do ART (#138). O faturamento
          do ano em reais continua na composição, com o nome dele. */}
      <CartaoDaMeta metas={metas} carregando={carregandoMetas} />

      {/* C. CLIENTES — únicos pela filial de cadastro; vínculos pela filial da carteira.

          O ZERO É "SEM VÍNCULO", E NÃO "SEM CLIENTE" (24/09/2026). A conta — e também a linha de
          situações — só enxerga cliente com vínculo ativo em carteira. A carga do cadastro traz os
          clientes da SA1 sem carteira, todos como suspect; até a carga das carteiras rodar, o banco tem
          milhares de clientes e este cartão tem zero. Sem dizer isso, o zero se lia "cadastro vazio".
          O número dos cadastrados sem carteira a API não devolve, e a tela não o inventa. */}
      <CartaoIndicador
        titulo="Clientes na carteira"
        valor={nº(carteira.clientesCadastradosComVinculo)}
        subtexto={
          carteira.clientesCadastradosComVinculo > 0
            ? `clientes únicos com vínculo ativo · ${nº(carteira.vinculosComerciais)} vínculos em ${nº(carteira.carteirasComerciais)} carteiras comerciais`
            : 'nenhum cliente com vínculo ativo em carteira — cliente cadastrado sem carteira não entra nesta conta'
        }
        detalhe={
          carteira.clientesCadastradosComVinculo > 0
            ? `com vínculo: ${nº(carteira.clientes)} clientes · ${nº(carteira.prospects)} prospects · ${nº(carteira.suspects)} suspects · ${nº(carteira.semDocumento)} sem CPF/CNPJ`
            : `${nº(carteira.vinculosComerciais)} vínculos em ${nº(carteira.carteirasComerciais)} carteiras comerciais`
        }
        dica="Cada cliente é contado uma vez, na filial em que está cadastrado, se tiver vínculo ativo em qualquer carteira. Um cliente em três carteiras é um cliente e três vínculos. Cliente cadastrado sem carteira não entra — nem no número, nem na divisão por situação: zero aqui quer dizer que nenhum cliente tem vínculo de carteira ainda, e não que o cadastro está vazio."
        cor="#0EA5E9"
        icone="users"
      />

      {/* D. COBERTURA — cadência declarada da linha, a mesma regra do mapa. Contato não é visita. */}
      <CartaoIndicador
        titulo="Cobertura pela cadência"
        valor={coberturaPct === null ? '—' : porcento(coberturaPct)}
        subtexto={`${nº(cobertura.cobertos)} de ${nº(cobertura.elegiveis)} vínculos elegíveis com contato no prazo da linha`}
        detalhe={`${nº(cobertura.pendentes)} pendentes (${nº(cobertura.foraDaCadencia)} fora do prazo + ${nº(cobertura.nuncaContatados)} nunca) · ${nº(cobertura.semCadencia)} em linha sem cadência, fora do % · contato registrado, não visita`}
        dica={`Elegível: vínculo em carteira comercial de linha com cadência declarada (máquinas 180/180/180/360 dias por classe A/B/C/D; prospecção 120/120/120/180; peças e AMS 360). Contato: a última interação registrada de qualquer tipo — nenhum dos ${nº(cobertura.tiposDeAtividade)} tipos de atividade está marcado como visita.`}
        cor="#7C3AED"
        icone="shield"
      />

      {/* E. MERCADO — o que o CRM registra de derrota. A Captura Tracbel (issue 162) não é deste cartão:
          ela divide as unidades do ART (`frota.VendaDeMaquina`) pela demanda estimada, e mora nos
          Indicadores Geográficos. As unidades do ART são máquinas, e não faturamento. */}
      <CartaoIndicador
        titulo="Conhecimento de mercado"
        valor={nº(mercado.vendasPerdidasRegistradas)}
        subtexto={`vendas perdidas registradas · ${nº(mercado.comConcorrente)} com concorrente${concorrentes === null ? '' : ` · ${nº(concorrentes)} concorrentes`}`}
        detalhe={`${mercado.primeiraEm ? `${data(mercado.primeiraEm)} a ${data(mercado.ultimaEm)}` : 'sem registro'} · Captura Tracbel: nos Indicadores Geográficos`}
        dica={`${nº(mercado.comModeloDoConcorrente)} com modelo do concorrente, ${nº(mercado.comOsDoisPrecos)} com os dois preços, ${nº(mercado.unidades)} máquinas nessas perdas. Este cartão conta derrotas registradas pelo CEN, e não o tamanho do mercado. As máquinas que a Tracbel vendeu estão no banco do CRM, lidas do ART — em unidades, e não em faturamento —, e a Captura Tracbel, que as divide pela demanda estimada do período (a anual proporcional aos meses), é medida nos Indicadores Geográficos.`}
        cor="#B45309"
        icone="eye"
        destaque
      />
    </div>
  );
}

/**
 * A COMPOSIÇÃO DOS CINCO NÚMEROS, filial a filial — o que o pedido chama de "consultar os registros
 * que compõem". Cada coluna é somável; a linha de total é a do cartão.
 */
function ComposicaoDosIndicadores({ ex, metas }: { ex: ExecutivoConsolidado; metas: MetasConsolidadas | null }) {
  const mes = ex.faturamentoDoMes;
  const fy = metas?.periodo ? `FY${metas.periodo.anoFiscal}` : 'FY';
  const metaDa = (codigo: string) => metas?.filiais.find((f) => f.filial.codigo === codigo)?.meta ?? null;
  // O TOTAL DA META COM FILIAL QUE FALHOU diz de quantas é: "1.250 (15 de 16)" — e não um número inteiro que não é.
  const deQuantas =
    metas && metas.falhas.length > 0 ? ` (${metas.respondidas} de ${metas.respondidas + metas.falhas.length})` : '';

  return (
    <details className="v360-composicao" data-bloco="composicao">
      <summary>Composição dos cinco indicadores — filial a filial, com fonte e regra</summary>
      <ul className="v360-composicao-regras">
        <li>
          <strong>Faturamento</strong>: Protheus SD2 (nota de saída de venda) → <code>comercial.FaturamentoDoCliente</code> e{' '}
          <code>comercial.FaturamentoSemCliente</code> → valor líquido da competência mais recente, com cliente e sem cliente
          por natureza → soma das filiais. Devolução e cancelamento não são abatidos. O ART não é somado.
        </li>
        <li>
          <strong>Faturamento do ano</strong>: as mesmas tabelas no ano fiscal {nomeDoAno(ex.ano)} (novembro a outubro), até o
          último mês fechado, em reais.
        </li>
        <li>
          <strong>Meta e realizado</strong>: API Gestão de Negócios (cadastro de metas) → <code>organizacao.MetaDeVenda</code>, em
          máquinas, contra <code>frota.VendaDeMaquina</code> (o ART), pela data da venda, no ano fiscal até o último mês fechado{' '}
          {metas?.periodo ? `(${metas.periodo.texto})` : ''}. As vendas que aguardam na integração do ART (cadastro, chassi ou outro motivo) ficam à parte; o
          consórcio é em cotas, sem realizado. Sem previsão.
        </li>
        <li>
          <strong>Clientes</strong>: <code>comercial.Cliente</code> × <code>comercial.ClienteCarteira</code> → cliente com
          vínculo ativo, na filial de cadastro. Vínculos e carteiras, pela filial da carteira.
        </li>
        <li>
          <strong>Cobertura</strong>: <code>ClienteCarteira.UltimaInteracaoEm</code> contra a cadência de{' '}
          <code>organizacao.LinhaDeNegocio</code> pela classe ABC do cliente (sem classe = D) → cobertos ÷ elegíveis. Mesma regra
          do mapa de cobertura.
        </li>
        <li>
          <strong>Mercado</strong>: <code>processo.VendaPerdida</code> (formulário do CEN) — derrotas registradas, e não o
          tamanho do mercado. A Captura Tracbel é medida nos Indicadores Geográficos.
        </li>
      </ul>
      {/* A ROLAGEM MORA AQUI, e só aqui: onze colunas não cabem num celular, e a tabela rola dentro da
          própria caixa em vez de empurrar a página para o lado. */}
      <div className="cad-tabela-wrap v360-composicao-tabela">
        <table className="cad-tabela">
          <caption className="cad-so-leitor">Composição dos indicadores por filial</caption>
          <thead>
            <tr>
              <th scope="col">Filial</th>
              <th scope="col">Mês · com cliente</th>
              <th scope="col">Mês · sem cliente</th>
              <th scope="col">Faturamento FY{ex.ano}</th>
              <th scope="col">
                Meta {fy} (máq.) <InfoTooltip texto={REGRA_DA_META} rotulo="Como a meta de venda se conta" />
              </th>
              <th scope="col">Realizado {fy} (máq.)</th>
              <th scope="col">Clientes únicos</th>
              <th scope="col">Vínculos comerciais</th>
              <th scope="col">Elegíveis</th>
              <th scope="col">No prazo</th>
              <th scope="col">Pendentes</th>
              <th scope="col">Vendas perdidas</th>
            </tr>
          </thead>
          <tbody>
            {ex.filiais.map(({ filial, painel, erro }) => {
              if (!painel) {
                return (
                  <tr key={filial.codigo}>
                    <td>{filial.nome}</td>
                    <td colSpan={11}>não respondeu — {erro?.message ?? 'motivo não informado'}</td>
                  </tr>
                );
              }
              const i = painel.indicadores;
              const doMes = i.faturamentoDoMes && mes && i.faturamentoDoMes.competencia === mes.competencia ? i.faturamentoDoMes : null;
              const metaDaFilial = metaDa(filial.codigo);
              return (
                <tr key={filial.codigo}>
                  <td>{filial.nome}</td>
                  <td className="num">{doMes ? emMilhoes(doMes.comCliente) : i.faturamentoDoMes ? `em ${mesPorExtenso(i.faturamentoDoMes.competencia)}` : '—'}</td>
                  <td className="num">{doMes ? emMilhoes(doMes.semCliente) : '—'}</td>
                  <td className="num">{emMilhoes(i.ano.total)}</td>
                  <td className="num">{metaDaFilial ? nº(metaDaFilial.totais.metaMaquinas) : '—'}</td>
                  <td className="num">{metaDaFilial ? nº(metaDaFilial.totais.realizadoMaquinas) : '—'}</td>
                  <td className="num">{nº(i.carteira.clientesCadastradosComVinculo)}</td>
                  <td className="num">{nº(i.carteira.vinculosComerciais)}</td>
                  <td className="num">{nº(i.cobertura.elegiveis)}</td>
                  <td className="num">{nº(i.cobertura.cobertos)}</td>
                  <td className="num">{nº(i.cobertura.pendentes)}</td>
                  <td className="num">{nº(i.mercado.vendasPerdidasRegistradas)}</td>
                </tr>
              );
            })}
            <tr className="total">
              <td>Total ({ex.respondidas} filiais)</td>
              <td className="num">{mes ? emMilhoes(mes.comCliente) : '—'}</td>
              <td className="num">{mes ? emMilhoes(mes.semCliente) : '—'}</td>
              <td className="num">{emMilhoes(ex.realizadoDoAno.total)}</td>
              <td className="num">{metas && metas.respondidas > 0 ? `${nº(metas.metaMaquinas)}${deQuantas}` : '—'}</td>
              <td className="num">{metas && metas.respondidas > 0 ? `${nº(metas.realizadoMaquinas)}${deQuantas}` : '—'}</td>
              <td className="num">{nº(ex.carteira.clientesCadastradosComVinculo)}</td>
              <td className="num">{nº(ex.carteira.vinculosComerciais)}</td>
              <td className="num">{nº(ex.cobertura.elegiveis)}</td>
              <td className="num">{nº(ex.cobertura.cobertos)}</td>
              <td className="num">{nº(ex.cobertura.pendentes)}</td>
              <td className="num">{nº(ex.mercado.vendasPerdidasRegistradas)}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </details>
  );
}

function CartaoIndicador({
  titulo,
  valor,
  subtexto,
  detalhe,
  dica,
  cor,
  icone,
  destaque = false,
}: {
  titulo: string;
  valor: string;
  /** A primeira linha do rodapé. Aceita marcação para o alerta das filiais que não responderam. */
  subtexto: ReactNode;
  /** A segunda linha: a composição do número. Aceita marcação para o "—ⓘ" de um número ausente. */
  detalhe?: ReactNode;
  /**
   * O texto completo da regra, na dica ao lado do título (issue 167). Era o `title` do cartão
   * inteiro: só aparecia com o ponteiro parado, e nunca pelo teclado nem no toque.
   */
  dica?: string;
  cor: string;
  icone: NomeDeIcone;
  destaque?: boolean;
}) {
  return (
    <div className={destaque ? 'v360-kpi-card v360-kpi-highlight' : 'v360-kpi-card'} data-kpi={titulo}>
      <div className="v360-kpi-header">
        <div className="v360-kpi-icone" style={{ background: `${cor}20`, color: cor }}>
          <IconeDoIndicador nome={icone} />
        </div>
        {/* A ÚLTIMA PALAVRA VAI COLADA AO ⓘ: com cinco cartões numa linha o título quebra em duas, e
            sem isto o ⓘ caía sozinho na segunda — um ícone órfão que parece de outro bloco. */}
        <div className="v360-kpi-titulo">
          {dica ? (
            <>
              {titulo.slice(0, titulo.lastIndexOf(' ') + 1)}
              <span className="v360-kpi-titulo-fim">
                {titulo.slice(titulo.lastIndexOf(' ') + 1)}
                <InfoTooltip texto={dica} rotulo={`Como se conta: ${titulo.toLowerCase()}`} />
              </span>
            </>
          ) : (
            titulo
          )}
        </div>
      </div>
      {/* O TRAÇO TEM O PESO DO TRAÇO DO CARTÃO SEM DADO, e não o de um número: lado a lado, os dois
          "—" da linha (faturamento e cobertura sem dado) liam como duas coisas diferentes. */}
      <div className={valor === '—' ? 'v360-kpi-valor v360-kpi-vazio' : 'v360-kpi-valor'}>{valor}</div>
      <div className="v360-kpi-rodape">
        <span className="v360-kpi-sub">{subtexto}</span>
        {detalhe && <span className="v360-kpi-detalhe">{detalhe}</span>}
      </div>
    </div>
  );
}

/** Mesmo cartão, mesmo tamanho, sem o número — e dizendo por quê. */
function CartaoSemDado({
  titulo,
  cor,
  motivo,
  icone,
  destaque = false,
}: {
  titulo: string;
  cor: string;
  motivo: string;
  icone: NomeDeIcone;
  destaque?: boolean;
}) {
  return (
    <div className={destaque ? 'v360-kpi-card v360-kpi-highlight' : 'v360-kpi-card'} data-kpi={titulo}>
      <div className="v360-kpi-header">
        <div className="v360-kpi-icone" style={{ background: `${cor}20`, color: cor }}>
          <IconeDoIndicador nome={icone} />
        </div>
        <div className="v360-kpi-titulo">{titulo}</div>
      </div>
      <div className="v360-kpi-valor v360-kpi-vazio">—</div>
      <div className="v360-kpi-rodape">
        <span className="v360-kpi-sub">{motivo}</span>
      </div>
    </div>
  );
}

function SemDado({ oQue, porque }: { oQue: string; porque: string }) {
  return (
    <div className="v360-sem-dado">
      <strong>Sem dado para {oQue}</strong>
      <span>{porque}</span>
    </div>
  );
}

function ItemDeLegenda({ cor, rotulo, valor }: { cor: string; rotulo: string; valor: number }) {
  return (
    <div className="v360-legenda-item">
      <span className="dot" style={{ background: cor }} />
      {rotulo} <strong>{nº(valor)}</strong>
    </div>
  );
}

function Alerta({
  tipo,
  titulo,
  detalhe,
  acao,
  para,
}: {
  tipo: 'critico' | 'aviso' | 'info';
  titulo: string;
  detalhe: string;
  acao: string;
  para: string;
}) {
  return (
    <div className={`v360-alerta v360-alerta-${tipo}`}>
      <div className="v360-alerta-icon">
        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth={2.4}>
          <circle cx="12" cy="12" r="10" />
          <path d="M12 16v-4" />
          <path d="M12 8h.01" />
        </svg>
      </div>
      <div className="v360-alerta-body">
        <div className="v360-alerta-titulo">{titulo}</div>
        <div className="v360-alerta-detalhe">{detalhe}</div>
      </div>
      <Link to={para} className="v360-alerta-acao">
        {acao}
      </Link>
    </div>
  );
}
