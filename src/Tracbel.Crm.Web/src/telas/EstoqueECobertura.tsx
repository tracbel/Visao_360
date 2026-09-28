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
 */

import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterEstoqueECobertura } from '../dados/api/estoque';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { ItemDaCobertura, MaquinaNaLista } from '../tipos/estoque';
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
  const indicadores: Indicador[] = [
    { rotulo: 'No pátio', valor: t && lido ? t.noPatio : null, deOnde: 'estoque, remessa, consignado e em transferência', semDado: 'o estoque ainda não foi lido' },
    { rotulo: 'Disponíveis', valor: t && lido ? t.disponiveis : null, deOnde: 'no pátio e sem reserva', tom: 'bom', semDado: 'o estoque ainda não foi lido' },
    { rotulo: 'Reservadas', valor: t && lido ? t.reservadas : null, deOnde: 'no pátio, para uma venda', semDado: 'o estoque ainda não foi lido' },
    {
      rotulo: 'Mais de 180 dias',
      valor: t && lido ? t.maisDe180Dias : null,
      deOnde: 'no pátio desde a entrada',
      tom: t && t.maisDe180Dias > 0 ? 'atencao' : 'neutro',
      semDado: 'o estoque ainda não foi lido',
    },
    { rotulo: 'Pedidos à fábrica', valor: t && lido ? t.pedidosAFabrica : null, deOnde: 'ainda sem entrada, com a chegada prevista', semDado: 'o estoque ainda não foi lido' },
    {
      rotulo: 'Cobertura',
      valor: dados?.cobertura.mediaPorMes != null ? `${meses(dados.cobertura.mediaPorMes)} meses` : null,
      deOnde: 'média dos meses fechados, da empresa inteira',
      semDado: 'a cobertura ainda não foi lida',
    },
  ];

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Estoque e Cobertura</h1>
          <p className="page-subtitle">
            As máquinas no pátio e as que vêm da fábrica, com a reserva e os dias parado — e quantos meses o estoque dura no
            ritmo de venda.
          </p>
        </div>
        {dados && (
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => baixarCsv(`estoque-${recorte}-${carimboDeData()}`, CABECALHO_DO_CSV, linhasDoCsv(visiveis))}
            disabled={visiveis.length === 0}
          >
            Exportar CSV
          </button>
        )}
      </div>

      <AvisoDeProcedencia procedencia={estoque.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={estoque.carregando} />

      <MetricasSemDado metricas={dados?.metricasSemDado} titulo="O que este estoque não afirma" />

      {estoque.carregando && <BlocoCarregando oQue="o estoque" />}
      {estoque.erro && <BlocoErro erro={estoque.erro} aoTentarDeNovo={estoque.recarregar} />}

      {dados && (
        <div className="est-grade">
          <div className="card cad-cartao" data-bloco="estoque-por-grupo">
            <div className="card-header cad-cartao-cabecalho">
              <div>
                <div className="card-title">Por grupo de máquina</div>
                <div className="card-subtitle">
                  {dados.alcance === 'Organizacao' ? 'todas as filiais' : 'a filial escolhida'}
                  {dados.lidoEm && <> · lido da Gestão de Negócios em {dataEHora(dados.lidoEm)}</>}
                </div>
              </div>
              <SeloProcedencia procedencia={estoque.procedencia} />
            </div>
            {dados.porGrupo.length === 0 ? (
              <BlocoVazio titulo="Nenhuma máquina" texto="O estoque desta filial está vazio, ou ainda não foi lido." />
            ) : (
              <div className="cad-tabela-wrap">
                <table className="cad-tabela est-tabela">
                  <caption className="cad-so-leitor">Estoque por grupo de máquina</caption>
                  <thead>
                    <tr>
                      <th scope="col">Grupo</th>
                      <th scope="col">No pátio</th>
                      <th scope="col">Disponíveis</th>
                      <th scope="col">Reservadas</th>
                      <th scope="col">Pedidos</th>
                      <th scope="col" title="A idade média no pátio, em dias">Idade média</th>
                      <th scope="col" title="Meses de estoque do grupo, da empresa inteira">Cobertura</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dados.porGrupo.map((g) => (
                      <tr key={g.grupo}>
                        <th scope="row">{g.grupo}</th>
                        <td className="cad-mono">{n(g.noPatio)}</td>
                        <td className="cad-mono">{n(g.disponiveis)}</td>
                        <td className="cad-mono">{n(g.reservadas)}</td>
                        <td className="cad-mono">{n(g.pedidosAFabrica)}</td>
                        <td className="cad-mono">
                          {g.idadeMediaEmDias === null ? (
                            <ValorAusente motivo="Nenhuma máquina do grupo no pátio com data de entrada." oQue={`a idade de ${g.grupo}`} />
                          ) : (
                            `${n(g.idadeMediaEmDias)} dias`
                          )}
                        </td>
                        <td className="cad-mono">
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
          </div>

          <div className="card cad-cartao" data-bloco="cobertura">
            <div className="card-header cad-cartao-cabecalho">
              <div>
                <div className="card-title">Cobertura em meses de estoque</div>
                <div className="card-subtitle">o estoque dividido pelas vendas, em quantidade · da empresa inteira</div>
              </div>
            </div>
            {dados.cobertura.porMes.length === 0 ? (
              <BlocoVazio titulo="Cobertura não lida" texto="Ela vem da mesma rotina do estoque." />
            ) : (
              <div className="est-cobertura">
                <Barras titulo="No ritmo de venda de cada mês" itens={dados.cobertura.porMes} rotulo={mesCurto} media={dados.cobertura.mediaPorMes} />
                <Barras titulo="Por grupo, no período inteiro" itens={dados.cobertura.porGrupo} rotulo={(i) => i.chave} media={dados.cobertura.mediaPorGrupo} />
              </div>
            )}
          </div>
        </div>
      )}

      {dados && (
        <div className="card cad-cartao" data-bloco="maquinas">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Máquinas</div>
              <div className="card-subtitle">{`${n(visiveis.length)} de ${n(dados.maquinas.length)} · dias no pátio contados até ${data(dados.hoje)}`}</div>
            </div>
          </div>

          <div className="cad-barra">
            <label className="cad-filtro">
              Mostrar
              <select value={recorte} onChange={(e) => setRecorte(e.target.value as Recorte)}>
                {RECORTES.map((r) => (
                  <option key={r.chave} value={r.chave}>
                    {r.rotulo}
                  </option>
                ))}
              </select>
            </label>
            <label className="cad-filtro">
              Grupo
              <select value={grupo} onChange={(e) => setGrupo(e.target.value)}>
                <option value="">Todos</option>
                {grupos.map((g) => (
                  <option key={g} value={g}>
                    {g}
                  </option>
                ))}
              </select>
            </label>
            <label className="cad-filtro">
              Modelo
              <input type="search" value={busca} placeholder="Buscar pelo modelo" onChange={(e) => setBusca(e.target.value)} />
            </label>
          </div>

          {visiveis.length === 0 ? (
            <p className="est-nada">Nenhuma máquina com esses filtros.</p>
          ) : (
            <div className="cad-tabela-wrap">
              <table className="cad-tabela est-tabela est-lista">
                <caption className="cad-so-leitor">Máquinas do estoque e pedidos à fábrica</caption>
                <thead>
                  <tr>
                    <th scope="col">Modelo</th>
                    <th scope="col">Grupo</th>
                    <th scope="col">Filial</th>
                    <th scope="col">Situação</th>
                    <th scope="col">Dias no pátio · chegada</th>
                    <th scope="col">Paga</th>
                    <th scope="col">Ano</th>
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
                      <td className="cad-mono">
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
                      <td className="cad-mono">{m.anoModelo ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </>
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
