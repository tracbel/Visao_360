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
 *   B. realizado do ano civil ao lado da meta — que ainda não existe (issue 138) — e nunca previsão;
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
 * - A META SAIU DO TEXTO COMO SE EXISTISSE: a tabela antiga foi removida na
 *   simplificação do banco (fase 1), e a API devolve o alvo sempre nulo. O
 *   cartão mostra "—" com o motivo, e a issue que destrava é a 138.
 * - O ANO É CIVIL, E A TELA PAROU DE DIZER QUE O FISCAL NÃO FOI CONFIRMADO: foi,
 *   em 24/09 (novembro a outubro). O servidor ainda apura jan–dez, e a visão por
 *   FY é o próximo passo — está na dica.
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
import { useRecurso } from '../../dados/api/useRecurso';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { ValorAusente } from '../comum/ValorAusente';
import { GraficoBarrasHorizontais } from '../GraficoBarrasHorizontais';
import { InfoTooltip } from '../InfoTooltip';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { GraficoDonutCentro } from '../GraficoDonutCentro';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import '../../estilos/painel-executivo.css';

/**
 * POR QUE A META NÃO APARECE — a frase é uma só, no cartão e na tabela.
 *
 * A tabela de metas do legado saiu na simplificação do banco (fase 1), e a API
 * devolve o alvo sempre nulo. Dizer "nenhuma cadastrada" sugeria que bastava
 * alguém cadastrar; não há onde. As metas administráveis são a issue 138.
 */
const MOTIVO_SEM_META =
  'O CRM ainda não tem onde cadastrar meta: as metas administráveis — por filial, com vigência — são a issue 138. ' +
  'Até lá o cartão mostra só o realizado.';

/**
 * O ANO DOS CARTÕES, na dica ao lado de "ano civil".
 *
 * O ano fiscal da Tracbel foi confirmado em 24/09/2026 — novembro a outubro,
 * com o nome do ano em que termina. O servidor ainda apura estes cartões de
 * janeiro a dezembro, então a tela diz "ano civil", que é o que o número é, e
 * anuncia a visão por FY como o próximo passo. Não diz mais que o calendário
 * fiscal "não foi confirmado": isso deixou de ser verdade.
 */
const DICA_DO_ANO_CIVIL =
  'Os cartões somam o ano civil, de janeiro a dezembro, que é como o servidor apura o realizado hoje. ' +
  'O ano fiscal da Tracbel vai de novembro a outubro e leva o nome do ano em que termina (o FY2026 vai de nov/2025 a out/2026); ' +
  'a visão por FY nesta tela vem na próxima etapa.';

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
  const anoCorrente = new Date().getFullYear();
  const [ano, setAno] = useState(anoCorrente);

  const consolidado = useRecurso((sinal) => obterConsolidado(contexto, sinal), [contexto.usuario]);
  const executivo = useRecurso((sinal) => obterExecutivoConsolidado(contexto, ano, sinal), [contexto.usuario, ano]);

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

      {/* O PERÍODO DOS CARTÕES, ESCRITO. O ano segue a escolha — não fica preso a 2026 —, e é civil
          porque é assim que o servidor apura hoje. O calendário fiscal (nov→out) FOI confirmado em
          24/09/2026; a visão por FY é o passo seguinte, e a dica diz isso (documento 32, P-4). */}
      <div className="v360-periodo" role="group" aria-label="Período dos indicadores" data-bloco="periodo">
        <label>
          Ano de referência
          <select value={ano} onChange={(e) => setAno(Number(e.target.value))}>
            {anos.map((a) => (
              <option key={a} value={a}>
                {a}
              </option>
            ))}
          </select>
        </label>
        <span className="v360-periodo-regra">
          ano civil (jan–dez)
          <InfoTooltip texto={DICA_DO_ANO_CIVIL} rotulo="Por que o ano é civil, e quando vem o ano fiscal" />
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
          ano={ano}
          // A CONTAGEM DE CONCORRENTES VEM DE OUTRA LEITURA (o consolidado). Ela só aparece quando as duas
          // leituras estão completas — senão o cartão juntaria um total de dez filiais com um de treze.
          concorrentes={
            ex && dados && ex.respondidas === ex.filiais.length && dados.falhas === 0
              ? vendasPerdidas.porConcorrente.length
              : null
          }
        />
      )}

      {ex && ex.respondidas > 0 && <ComposicaoDosIndicadores ex={ex} />}

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

/** Os cinco cartões, cada um com fonte, período e alcance no rodapé — e o motivo quando falta dado. */
function CincoIndicadores({ ex, ano, concorrentes }: { ex: ExecutivoConsolidado | null; ano: number; concorrentes: number | null }) {
  if (!ex) {
    const motivo = 'a leitura dos indicadores das filiais não respondeu';
    return (
      <div className="v360-kpi-row" data-bloco="kpis">
        <CartaoSemDado titulo="Faturamento do mês" cor="#367C2B" motivo={motivo} icone="trending-up" />
        <CartaoSemDado titulo={`Meta e realizado · ${ano}`} cor="#1B5E20" motivo={motivo} icone="target" />
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

  const doAno = ex.realizadoDoAno;
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

      {/* B. META E REALIZADO — o realizado é medido; a meta ainda não existe (issue 138); previsão não existe.
          O ramo com alvo fica: a API continua devolvendo o campo, e o dia em que a 138 trouxer a meta o
          cartão já sabe mostrá-la. */}
      <CartaoIndicador
        titulo={`Meta e realizado · ${ano}`}
        valor={doAno.ultimaCompetencia ? emMilhoes(doAno.total) : '—'}
        subtexto={
          doAno.ultimaCompetencia && doAno.primeiraCompetencia
            ? `realizado ${mesCurto(doAno.primeiraCompetencia)} a ${mesPorExtenso(doAno.ultimaCompetencia)}${
                mesEmCurso && mes && doAno.ultimaCompetencia === mes.competencia ? ' (em curso)' : ''
              } · ano civil`
            : `sem faturamento carregado em ${ano}`
        }
        detalhe={
          doAno.alvo === null ? (
            <>
              meta <ValorAusente motivo={MOTIVO_SEM_META} oQue="a meta" /> · previsão: sem modelo aprovado
            </>
          ) : (
            `meta ${emMilhoes(doAno.alvo)} em ${doAno.filiaisComMeta} filial(is) · ${porcento((100 * doAno.realizadoDasFiliaisComMeta) / doAno.alvo)} realizado nelas · previsão: sem modelo`
          )
        }
        dica="Realizado: soma das notas de saída do ano civil (jan–dez), com e sem cliente no CRM. Meta: o CRM ainda não tem onde cadastrá-la — é a issue 138. Previsão: não calculada — extrapolar a média dos meses não é previsão."
        cor="#1B5E20"
        icone="target"
      />

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
        dica={`${nº(mercado.comModeloDoConcorrente)} com modelo do concorrente, ${nº(mercado.comOsDoisPrecos)} com os dois preços, ${nº(mercado.unidades)} máquinas nessas perdas. Este cartão conta derrotas registradas pelo CEN, e não o tamanho do mercado. As máquinas que a Tracbel vendeu estão no banco do CRM, lidas do ART — em unidades, e não em faturamento —, e a Captura Tracbel, que as divide pela demanda anual estimada, é medida nos Indicadores Geográficos.`}
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
function ComposicaoDosIndicadores({ ex }: { ex: ExecutivoConsolidado }) {
  const mes = ex.faturamentoDoMes;

  return (
    <details className="v360-composicao" data-bloco="composicao">
      <summary>Composição dos cinco indicadores — filial a filial, com fonte e regra</summary>
      <ul className="v360-composicao-regras">
        <li>
          <strong>Faturamento</strong>: Protheus SD2 (nota de saída de venda) → <code>comercial.FaturamentoDoCliente</code> e{' '}
          <code>comercial.FaturamentoSemCliente</code> → valor líquido da competência mais recente, com cliente e sem cliente
          por natureza → soma das filiais. Devolução e cancelamento não são abatidos. O ART não é somado.
        </li>
        {/* A META NÃO TEM TABELA (fase 1): a linha dizia de onde ela vinha, e ela não vem de lugar nenhum. */}
        <li>
          <strong>Meta e realizado</strong>: as mesmas tabelas no ano civil {ex.ano}, de janeiro a dezembro. Meta: o CRM ainda
          não tem onde cadastrá-la — as metas administráveis são a issue 138. Sem previsão.
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
              <th scope="col">Realizado {ex.ano}</th>
              <th scope="col">
                Meta {ex.ano} <InfoTooltip texto={MOTIVO_SEM_META} rotulo="Por que a meta não aparece" />
              </th>
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
                    <td colSpan={10}>não respondeu — {erro?.message ?? 'motivo não informado'}</td>
                  </tr>
                );
              }
              const i = painel.indicadores;
              const doMes = i.faturamentoDoMes && mes && i.faturamentoDoMes.competencia === mes.competencia ? i.faturamentoDoMes : null;
              return (
                <tr key={filial.codigo}>
                  <td>{filial.nome}</td>
                  <td className="num">{doMes ? emMilhoes(doMes.comCliente) : i.faturamentoDoMes ? `em ${mesPorExtenso(i.faturamentoDoMes.competencia)}` : '—'}</td>
                  <td className="num">{doMes ? emMilhoes(doMes.semCliente) : '—'}</td>
                  <td className="num">{emMilhoes(i.ano.total)}</td>
                  <td className="num">{i.ano.alvoDaFilial === null ? '—' : emMilhoes(i.ano.alvoDaFilial)}</td>
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
              <td className="num">{ex.realizadoDoAno.alvo === null ? '—' : emMilhoes(ex.realizadoDoAno.alvo)}</td>
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
  subtexto: string;
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
