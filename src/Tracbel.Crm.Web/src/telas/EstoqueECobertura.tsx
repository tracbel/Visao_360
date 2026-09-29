/**
 * Estoque e Cobertura (28/09/2026) — a disponibilidade de máquina para a venda e quantos meses o estoque dura.
 *
 * É o painel "Estoque & Pedidos" do TOTVS, lido pela API Gestão de Negócios:
 *
 * - **o pátio e a fábrica** — o que está em estoque (com a reserva e os dias parado) e o que vem da fábrica (com a chegada
 *   prevista), da filial escolhida ou de todas;
 * - **a cobertura em meses** — o estoque dividido pelo ritmo de venda, em quantidade, da empresa inteira, por mês e por
 *   grupo, como a GN calcula;
 * - **sem custo e sem cliente** — o CRM não lê nenhum dos dois.
 *
 * 29/09/2026 — NO PADRÃO DOS INDICADORES GEOGRÁFICOS (#293, bloco 3): página na coluna inteira, os filtros da lista na
 * barra, os seis números em cartões de decisão e duas seções — os grupos com a cobertura, e as máquinas. Nenhum número
 * nem texto mudou.
 */

import { CalendarClock, Factory, Gauge, Layers, ListFilter, Lock, PackageCheck, RefreshCw, Search, Warehouse } from 'lucide-react';
import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterEstoqueECobertura } from '../dados/api/estoque';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { ItemDaCobertura, MaquinaNaLista } from '../tipos/estoque';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/territorio.css';
import '../estilos/estoque.css';

/** O recorte da lista. */
export type Recorte = 'patio' | 'disponiveis' | 'pedidos' | 'todas';

const RECORTES: { chave: Recorte; rotulo: string }[] = [
  { chave: 'patio', rotulo: 'No pátio' },
  { chave: 'disponiveis', rotulo: 'Disponíveis (sem reserva)' },
  { chave: 'pedidos', rotulo: 'Pedidos à fábrica' },
  { chave: 'todas', rotulo: 'Todas' },
];

const n = (v: number) => v.toLocaleString('pt-BR');
const meses = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
const data = (aaaammdd: string) => `${aaaammdd.slice(8, 10)}/${aaaammdd.slice(5, 7)}/${aaaammdd.slice(0, 4)}`;
const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
const mesCurto = (item: ItemDaCobertura) =>
  item.competencia ? `${MESES[Number(item.competencia.slice(5, 7)) - 1]}/${item.competencia.slice(2, 4)}` : item.chave;

const NAO_LIDO = 'o estoque ainda não foi lido';

/** As máquinas do recorte, com a busca pelo modelo e o filtro de grupo. */
export function filtrar(maquinas: MaquinaNaLista[], recorte: Recorte, grupo: string, busca: string): MaquinaNaLista[] {
  const termo = busca.trim().toLocaleLowerCase('pt-BR');
  return maquinas.filter(
    (m) =>
      (recorte === 'todas' ||
        (recorte === 'pedidos' ? m.ehPedidoAFabrica : !m.ehPedidoAFabrica && (recorte === 'patio' || !m.reservado))) &&
      (!grupo || m.grupo === grupo) &&
      (!termo || `${m.descricao} ${m.configuracao ?? ''}`.toLocaleLowerCase('pt-BR').includes(termo)),
  );
}

/** As linhas da tela como o CSV as leva. */
export function linhasDoCsv(maquinas: MaquinaNaLista[]): unknown[][] {
  return maquinas.map((m) => [
    m.filial,
    m.grupo,
    m.descricao,
    m.configuracao ?? '',
    m.ehPedidoAFabrica ? 'Pedido à fábrica' : m.situacao,
    m.reservado ? 'Sim' : 'Não',
    m.pago ? 'Sim' : 'Não',
    m.ehUsado ? 'Usado' : 'Novo',
    m.anoModelo ?? '',
    m.chassi ?? '',
    m.entradaEm ? data(m.entradaEm) : '',
    m.diasNoPatio ?? '',
    m.chegadaPrevistaEm ? data(m.chegadaPrevistaEm) : '',
    m.faturamentoPrevistoEm ? data(m.faturamentoPrevistoEm) : '',
  ]);
}

const CABECALHO_DO_CSV = [
  'Filial', 'Grupo', 'Modelo', 'Configuração', 'Situação', 'Reservada', 'Paga', 'Novo/Usado', 'Ano', 'Chassi', 'Entrada',
  'Dias no pátio', 'Chegada prevista', 'Faturamento previsto',
];

export function EstoqueECobertura() {
  const { contexto } = useContextoDeAcesso();
  const [recorte, setRecorte] = useState<Recorte>('patio');
  const [grupo, setGrupo] = useState('');
  const [busca, setBusca] = useState('');

  const estoque = useRecurso((sinal) => obterEstoqueECobertura(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const dados = estoque.dados;

  const visiveis = useMemo(() => (dados ? filtrar(dados.maquinas, recorte, grupo, busca) : []), [dados, recorte, grupo, busca]);
  const grupos = useMemo(() => (dados ? [...new Set(dados.maquinas.map((m) => m.grupo))].sort((a, b) => a.localeCompare(b, 'pt-BR')) : []), [dados]);

  const t = dados?.totais;
  const lido = dados?.lidoEm != null;

  /** Um número do estoque: sem leitura, o traço — e a linha de baixo diz que o estoque não foi lido, como a faixa antiga. */
  const doEstoque = (valor: number | undefined) => (t && lido && valor !== undefined ? n(valor) : null);
  const linhaDeBaixo = (deOnde: string) => (estoque.carregando ? null : t && lido ? deOnde : NAO_LIDO);
  const motivo = dados ? 'O estoque ainda não foi lido pela rotina da Gestão de Negócios.' : undefined;

  const cobertura = dados?.cobertura.mediaPorMes ?? null;

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Estoque e Cobertura</h1>
          <p className="page-subtitle">
            As máquinas no pátio e as que vêm da fábrica, com a reserva e os dias parado — e quantos meses o estoque dura no
            ritmo de venda.
          </p>
        </div>
        <p className="dash-atualizado">
          {estoque.procedencia ? <DadosAtualizadosEm procedencia={estoque.procedencia} /> : 'Lendo o estoque…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={estoque.recarregar}
            disabled={estoque.carregando}
            data-carregando={estoque.carregando ? 'true' : 'false'}
            aria-label="Reler o estoque"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES: o recorte, o grupo e o modelo filtram a lista das máquinas. Os cartões e os grupos são
          sempre do estoque inteiro da filial. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="recorte">
            <span className="dash-filtro-icone" aria-hidden="true">
              <ListFilter size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Mostrar</span>
              <select value={recorte} onChange={(e) => setRecorte(e.target.value as Recorte)}>
                {RECORTES.map((r) => (
                  <option key={r.chave} value={r.chave}>
                    {r.rotulo}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="grupo">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Layers size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Grupo</span>
              <select value={grupo} onChange={(e) => setGrupo(e.target.value)}>
                <option value="">Todos</option>
                {grupos.map((g) => (
                  <option key={g} value={g}>
                    {g}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="busca">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Search size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Modelo</span>
              <input type="search" value={busca} placeholder="Buscar pelo modelo" onChange={(e) => setBusca(e.target.value)} />
            </span>
          </label>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={estoque.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="3">
        <CartaoDeDecisao
          rotulo="No pátio"
          icone={Warehouse}
          tom="demanda"
          valor={doEstoque(t?.noPatio)}
          carregando={estoque.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('estoque, remessa, consignado e em transferência')}
          sobre="As máquinas no pátio da filial: em estoque, em remessa, em consignado e em transferência — lidas do painel Estoque & Pedidos do TOTVS pela Gestão de Negócios."
        />
        <CartaoDeDecisao
          rotulo="Disponíveis"
          icone={PackageCheck}
          tom="captura"
          valor={doEstoque(t?.disponiveis)}
          carregando={estoque.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('no pátio e sem reserva')}
          sobre="As máquinas no pátio sem reserva para uma venda: é o que o vendedor pode oferecer hoje."
        />
        <CartaoDeDecisao
          rotulo="Reservadas"
          icone={Lock}
          tom="oportunidade"
          valor={doEstoque(t?.reservadas)}
          carregando={estoque.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('no pátio, para uma venda')}
          sobre="As máquinas no pátio já reservadas para uma venda."
        />
        <CartaoDeDecisao
          rotulo="Mais de 180 dias"
          icone={CalendarClock}
          tom="neutro"
          valor={doEstoque(t?.maisDe180Dias)}
          carregando={estoque.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('no pátio desde a entrada')}
          sobre="As máquinas no pátio há mais de 180 dias, contados da data de entrada até hoje. Na lista, os dias delas aparecem em destaque."
        />
        <CartaoDeDecisao
          rotulo="Pedidos à fábrica"
          icone={Factory}
          tom="mercado"
          valor={doEstoque(t?.pedidosAFabrica)}
          carregando={estoque.carregando}
          unidade="máquinas"
          motivoSemDado={motivo}
          variacao={linhaDeBaixo('ainda sem entrada, com a chegada prevista')}
          sobre="As máquinas pedidas à fábrica que ainda não deram entrada, com a chegada prevista."
        />
        <CartaoDeDecisao
          rotulo="Cobertura"
          icone={Gauge}
          tom="neutro"
          valor={cobertura !== null ? meses(cobertura) : null}
          carregando={estoque.carregando}
          unidade="meses"
          motivoSemDado={dados ? 'A cobertura ainda não foi lida pela rotina da Gestão de Negócios.' : undefined}
          variacao={estoque.carregando ? null : cobertura !== null ? 'média dos meses fechados, da empresa inteira' : 'a cobertura ainda não foi lida'}
          sobre="Quantos meses o estoque dura no ritmo de venda: o estoque dividido pelas vendas, em quantidade. É a média dos meses fechados, da empresa inteira, como a Gestão de Negócios calcula."
        />
      </div>

      <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que este estoque não afirma" />

      {estoque.carregando && <BlocoCarregando oQue="o estoque" />}
      {estoque.erro && <BlocoErro erro={estoque.erro} aoTentarDeNovo={estoque.recarregar} />}

      {dados && (
        <section className="dash-secao" data-bloco="secao-grupos">
          <TituloDaSecao titulo="Os grupos e a cobertura" subtitulo="O pátio e a fábrica por grupo de máquina, e quantos meses o estoque dura." />

          <div className="est-grade">
            <PainelDoMomento
              titulo="Por grupo de máquina"
              data-bloco="estoque-por-grupo"
              subtitulo={
                <>
                  {dados.alcance === 'Organizacao' ? 'todas as filiais' : 'a filial escolhida'}
                  {dados.lidoEm && <> · lido da Gestão de Negócios em {dataEHora(dados.lidoEm)}</>}
                </>
              }
            >
              {dados.porGrupo.length === 0 ? (
                <BlocoVazio titulo="Nenhuma máquina" texto="O estoque desta filial está vazio, ou ainda não foi lido." />
              ) : (
                <div className="mom-tabela-rolagem">
                  <table className="mom-tabela est-tabela">
                    <caption className="cad-so-leitor">Estoque por grupo de máquina</caption>
                    <thead>
                      <tr>
                        <th scope="col">Grupo</th>
                        <th scope="col" className="mom-num">No pátio</th>
                        <th scope="col" className="mom-num">Disponíveis</th>
                        <th scope="col" className="mom-num">Reservadas</th>
                        <th scope="col" className="mom-num">Pedidos</th>
                        <th scope="col" className="mom-num">
                          Idade média
                          <InfoTooltip texto="A idade média no pátio, em dias." rotulo="O que é a idade média" />
                        </th>
                        <th scope="col" className="mom-num">
                          Cobertura
                          <InfoTooltip texto="Meses de estoque do grupo, da empresa inteira." rotulo="O que é a cobertura do grupo" />
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {dados.porGrupo.map((g) => (
                        <tr key={g.grupo}>
                          <th scope="row">{g.grupo}</th>
                          <td className="mom-num">{n(g.noPatio)}</td>
                          <td className="mom-num">{n(g.disponiveis)}</td>
                          <td className="mom-num">{n(g.reservadas)}</td>
                          <td className="mom-num">{n(g.pedidosAFabrica)}</td>
                          <td className="mom-num">
                            {g.idadeMediaEmDias === null ? (
                              <ValorAusente motivo="Nenhuma máquina do grupo no pátio com data de entrada." oQue={`a idade de ${g.grupo}`} />
                            ) : (
                              `${n(g.idadeMediaEmDias)} dias`
                            )}
                          </td>
                          <td className="mom-num">
                            {g.coberturaEmMeses === null ? (
                              <ValorAusente motivo="A Gestão de Negócios não calcula a cobertura deste grupo." oQue={`a cobertura de ${g.grupo}`} />
                            ) : (
                              `${meses(g.coberturaEmMeses)} meses`
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </PainelDoMomento>

            <PainelDoMomento
              titulo="Cobertura em meses de estoque"
              data-bloco="cobertura"
              subtitulo="o estoque dividido pelas vendas, em quantidade · da empresa inteira"
            >
              {dados.cobertura.porMes.length === 0 ? (
                <BlocoVazio titulo="Cobertura não lida" texto="Ela vem da mesma rotina do estoque." />
              ) : (
                <div className="est-cobertura">
                  <Barras titulo="No ritmo de venda de cada mês" itens={dados.cobertura.porMes} rotulo={mesCurto} media={dados.cobertura.mediaPorMes} />
                  <Barras titulo="Por grupo, no período inteiro" itens={dados.cobertura.porGrupo} rotulo={(i) => i.chave} media={dados.cobertura.mediaPorGrupo} />
                </div>
              )}
            </PainelDoMomento>
          </div>
        </section>
      )}

      {dados && (
        <section className="dash-secao" data-bloco="secao-maquinas">
          <TituloDaSecao titulo="As máquinas" subtitulo="Cada máquina do recorte da barra, com a situação e os dias parado." />

          <PainelDoMomento
            titulo="Máquinas"
            data-bloco="maquinas"
            subtitulo={`${n(visiveis.length)} de ${n(dados.maquinas.length)} · dias no pátio contados até ${data(dados.hoje)}`}
            direita={
              <button
                type="button"
                className="btn btn-secondary btn-sm"
                onClick={() => baixarCsv(`estoque-${recorte}-${carimboDeData()}`, CABECALHO_DO_CSV, linhasDoCsv(visiveis))}
                disabled={visiveis.length === 0}
              >
                Exportar CSV
              </button>
            }
          >
            {visiveis.length === 0 ? (
              <p className="est-nada">Nenhuma máquina com esses filtros.</p>
            ) : (
              <div className="mom-tabela-rolagem">
                <table className="mom-tabela est-tabela est-lista">
                  <caption className="cad-so-leitor">Máquinas do estoque e pedidos à fábrica</caption>
                  <thead>
                    <tr>
                      <th scope="col">Modelo</th>
                      <th scope="col">Grupo</th>
                      <th scope="col">Filial</th>
                      <th scope="col">Situação</th>
                      <th scope="col" className="mom-num">Dias no pátio · chegada</th>
                      <th scope="col">Paga</th>
                      <th scope="col" className="mom-num">Ano</th>
                    </tr>
                  </thead>
                  <tbody>
                    {visiveis.map((m, i) => (
                      <tr key={`${m.filial}-${m.descricao}-${m.chassi ?? i}-${i}`}>
                        <td>
                          {m.descricao}
                          {(m.configuracao || m.ehUsado) && (
                            <div className="cad-sub">{[m.configuracao, m.ehUsado ? 'usado' : null].filter(Boolean).join(' · ')}</div>
                          )}
                        </td>
                        <td>{m.grupo}</td>
                        <td>{m.filial}</td>
                        <td>
                          {m.ehPedidoAFabrica ? 'Pedido à fábrica' : m.situacao}
                          {m.reservado && <span className="est-reservada">reservada</span>}
                          {m.situacaoNaFabrica && <div className="cad-sub">{m.situacaoNaFabrica}</div>}
                        </td>
                        <td className="mom-num">
                          {m.ehPedidoAFabrica ? (
                            m.chegadaPrevistaEm ? (
                              `chega ${data(m.chegadaPrevistaEm)}`
                            ) : (
                              <ValorAusente motivo="O pedido não tem chegada prevista (FDD) no TOTVS." oQue="a chegada" />
                            )
                          ) : m.diasNoPatio === null ? (
                            <ValorAusente motivo="A máquina não tem data de entrada no TOTVS." oQue="os dias no pátio" />
                          ) : (
                            <span className={m.diasNoPatio > 180 ? 'est-velha' : undefined}>{`${n(m.diasNoPatio)} dias`}</span>
                          )}
                        </td>
                        <td>{m.pago ? 'Sim' : 'Não'}</td>
                        <td className="mom-num">{m.anoModelo ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </PainelDoMomento>
        </section>
      )}
    </PaginaDoPainel>
  );
}

/** Barras horizontais da cobertura, com o número escrito ao lado e a média do recorte. */
function Barras({
  titulo,
  itens,
  rotulo,
  media,
}: {
  titulo: string;
  itens: ItemDaCobertura[];
  rotulo: (item: ItemDaCobertura) => string;
  media: number | null;
}) {
  const maior = Math.max(...itens.map((i) => i.meses), 1);
  return (
    <div className="est-barras">
      <div className="est-barras-titulo">
        {titulo}
        {media !== null && <span className="est-media">{`média ${meses(media)} meses`}</span>}
      </div>
      <ul>
        {itens.map((i) => (
          <li key={i.chave} title={`${n(i.vendas)} vendas no período`}>
            <span className="est-barra-rotulo">{rotulo(i)}</span>
            <span className="est-barra" aria-hidden="true">
              <span style={{ width: `${(100 * i.meses) / maior}%` }} />
            </span>
            <span className="est-barra-valor cad-mono">{meses(i.meses)}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

function dataEHora(iso: string) {
  return new Date(iso).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}
