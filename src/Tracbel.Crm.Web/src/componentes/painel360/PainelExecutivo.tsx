/**
 * Painel executivo da Visão 360 — a mesma tela do protótipo, com dado real.
 *
 * A marcação e as classes são as de `renderVisao360()`
 * (`prototipo/referencia/assets/app.js`, linha 7409): `v360-container`,
 * `v360-kpi-row`, `v360-grid-row2/3/4`, `v360-card`. Nada de estrutura nova —
 * o que muda em relação ao protótipo é **de onde vem o número**, nunca a forma.
 *
 * O que tem dado real hoje (banco `TracbelCrm`, carga de 2026):
 *   clientes na carteira · cobertura por tempo sem contato · ranking de CEN ·
 *   mix por linha de negócio · vendas perdidas por motivo · alertas.
 *
 * O que NÃO tem, e por quê:
 *   faturamento, previsão e conhecimento de mercado dependem do Protheus, cuja
 *   integração está parada desde 11/04/2025 (documento 18). O cartão **fica na
 *   tela**, no mesmo lugar e no mesmo tamanho, dizendo que não há dado — some
 *   o número, não o cartão. Mostrar valor velho com cara de atual é o defeito
 *   do legado que este projeto existe para corrigir.
 */
import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import {
  censConsolidados,
  mixDeLinhas,
  obterConsolidado,
  perdasConsolidadas,
  somarConsolidado,
  vendasPerdidasConsolidadas,
} from '../../dados/api/consolidado';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { GraficoBarrasHorizontais } from '../GraficoBarrasHorizontais';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { GraficoDonutCentro } from '../GraficoDonutCentro';

/** As cores das faixas de cobertura, iguais às da legenda do protótipo. */
const COR_EM_DIA = '#367C2B';
const COR_AVISO = '#B45309';
const COR_ATRASO = '#DC2626';
const COR_NUNCA = '#9CA3AF';

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
 * rótulos — o nome inteiro fica no `title`, para quem passar o cursor.
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

export function PainelExecutivo() {
  const { contexto } = useContextoDeAcesso();
  const consolidado = useRecurso((sinal) => obterConsolidado(contexto, sinal), [contexto.usuario]);

  const dados = consolidado.dados;
  const total = useMemo(() => somarConsolidado(dados), [dados]);
  const mix = useMemo(() => mixDeLinhas(dados), [dados]);
  const cens = useMemo(() => censConsolidados(dados), [dados]);
  const perdas = useMemo(() => perdasConsolidadas(dados), [dados]);
  const vendasPerdidas = useMemo(() => vendasPerdidasConsolidadas(dados), [dados]);

  /* Cobertura por tempo sem contato — as cinco faixas da legenda original. */
  const emDia = total.em30;
  const aviso = Math.max(0, total.em90 - total.em30);
  const atraso = Math.max(0, total.clientes - total.em90 - total.nunca);
  const nunca = total.nunca;
  const coberturaPct = total.clientes > 0 ? Math.round((emDia / total.clientes) * 100) : 0;

  const fatiasDaCobertura = useMemo(
    () => [
      { valor: emDia, cor: COR_EM_DIA },
      { valor: aviso, cor: COR_AVISO },
      { valor: atraso, cor: COR_ATRASO },
      { valor: nunca, cor: COR_NUNCA },
    ],
    [emDia, aviso, atraso, nunca],
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

  if (consolidado.carregando) return <BlocoCarregando oQue="o consolidado das treze filiais" />;

  return (
    <div className="v360-container">
      {consolidado.erro && (
        <BlocoErro erro={consolidado.erro} aoTentarDeNovo={consolidado.recarregar} />
      )}

      {/* ROW 1: os cinco indicadores ------------------------------------- */}
      <div className="v360-kpi-row">
        <CartaoSemDado
          titulo="Faturamento do mês"
          cor="#367C2B"
          motivo="Integração parada em 11/04/2025"
          icone="trending-up"
        />
        <CartaoSemDado
          titulo="Previsão FY 2026"
          cor="#1B5E20"
          motivo="Depende do faturamento"
          icone="target"
        />
        <CartaoIndicador
          titulo="Clientes na carteira"
          valor={nº(total.clientes)}
          subtexto={`${nº(total.carteiras)} carteiras · ${total.filiais} filiais`}
          cor="#0EA5E9"
          icone="users"
        />
        <CartaoIndicador
          titulo="Cobertura ativa"
          valor={`${coberturaPct}%`}
          subtexto={`${nº(emDia)} de ${nº(total.clientes)} com contato em 30 dias`}
          cor="#7C3AED"
          icone="shield"
        />
        <CartaoSemDado
          titulo="Conhecimento de mercado"
          cor="#B45309"
          motivo="Emplacamento não integrado"
          icone="eye"
          destaque
        />
      </div>

      {/* ROW 2: conhecimento · status da cobertura · faturamento --------- */}
      <div className="v360-grid-row2">
        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Conhecimento de mercado</div>
              <div className="v360-card-sub">Máquinas emplacadas com registro nosso</div>
            </div>
          </div>
          <SemDado
            oQue="o emplacamento"
            porque="Depende do John Deere Connect cruzado com o Protheus. Nenhum dos dois está integrado."
          />
        </div>

        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Status da cobertura</div>
              <div className="v360-card-sub">
                Distribuição de {nº(total.clientes)} clientes por status
              </div>
            </div>
            <Link to="/cobertura" className="v360-link">
              Ver detalhes →
            </Link>
          </div>
          <div className="v360-donut-wrap">
            <GraficoDonutCentro
              segmentos={fatiasDaCobertura}
              /* 160 e não os 200 do protótipo: a legenda real traz números de
                 cinco dígitos ("15.560"), e não "8". O donut cede a diferença. */
              largura={160}
              altura={160}
              cutout="70%"
              bordaBranca
              centro={{ linha1: `${coberturaPct}%`, linha2: 'ativa' }}
            />
            <div className="v360-donut-legenda">
              <ItemDeLegenda cor={COR_EM_DIA} rotulo="Em dia" valor={emDia} />
              <ItemDeLegenda cor={COR_AVISO} rotulo="Aviso" valor={aviso} />
              <ItemDeLegenda cor={COR_ATRASO} rotulo="Atraso" valor={atraso} />
              <ItemDeLegenda cor={COR_NUNCA} rotulo="Nunca" valor={nunca} />
            </div>
          </div>
        </div>

        <div className="v360-card v360-card-lg">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Faturamento — 12 meses</div>
              <div className="v360-card-sub">Realizado vs previsto vs meta</div>
            </div>
          </div>
          <SemDado
            oQue="o faturamento"
            porque="A integração com o Protheus parou em 11/04/2025. O CRM não recebe nota fiscal desde então."
          />
        </div>
      </div>

      {/* ROW 3: top clientes · top CENs · mix por linha ------------------- */}
      <div className="v360-grid-row3">
        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Top 5 clientes</div>
              <div className="v360-card-sub">Maior faturamento acumulado</div>
            </div>
          </div>
          <SemDado
            oQue="o ranking de clientes"
            porque="O ranking é por faturamento, que depende do Protheus."
          />
        </div>

        <div className="v360-card v360-card-md">
          <div className="v360-card-header">
            <div>
              <div className="v360-card-title">Top CENs</div>
              <div className="v360-card-sub">Ranking por clientes em carteira</div>
            </div>
          </div>
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
                    {nº(c.em30)} em dia
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
              <div className="v360-card-title">Mix por linha</div>
              <div className="v360-card-sub">Participação de cada linha na carteira</div>
            </div>
          </div>
          <div className="v360-mix-wrap">
            <GraficoDonutCentro segmentos={segmentosDoMix} largura={140} altura={140} cutout="72%" bordaBranca tooltipUnidade="%" />
            <div className="v360-mix-legenda">
              {mix.slice(0, TOP + 3).map((l, i) => (
                <div className="v360-legenda-item" key={l.nome}>
                  <span className="dot" style={{ background: CORES_MIX[i % CORES_MIX.length] }} />
                  <span className="v360-mix-linha" title={l.nome}>
                    {semPrefixoDeVenda(l.nome)}
                  </span>
                  <strong>
                    {totalDoMix > 0 ? Math.round((l.clientes / totalDoMix) * 100) : 0}%
                  </strong>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* ROW 4: vendas perdidas · alertas gerenciais ---------------------- */}
      <div className="v360-grid-row4">
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
                porque={`Os ${nº(totalPerdido)} processos perdidos existem, e nenhum deles tem o formulário de venda perdida preenchido.`}
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
            <Alerta
              tipo="critico"
              titulo={`${nº(nunca)} clientes nunca contatados`}
              detalhe={`De ${nº(total.clientes)} na carteira das ${total.filiais} filiais`}
              acao="Ver cobertura"
              para="/cobertura"
            />
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

function CartaoIndicador({
  titulo,
  valor,
  subtexto,
  cor,
  icone,
}: {
  titulo: string;
  valor: string;
  subtexto: string;
  cor: string;
  icone: NomeDeIcone;
}) {
  return (
    <div className="v360-kpi-card">
      <div className="v360-kpi-header">
        <div className="v360-kpi-icone" style={{ background: `${cor}20`, color: cor }}>
          <IconeDoIndicador nome={icone} />
        </div>
        <div className="v360-kpi-titulo">{titulo}</div>
      </div>
      <div className="v360-kpi-valor">{valor}</div>
      <div className="v360-kpi-rodape">
        <span className="v360-kpi-sub">{subtexto}</span>
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
    <div className={destaque ? 'v360-kpi-card v360-kpi-highlight' : 'v360-kpi-card'}>
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
