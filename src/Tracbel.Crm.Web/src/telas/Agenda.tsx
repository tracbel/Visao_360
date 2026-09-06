/**
 * Agenda do CEN — ligada ao dado real de `processo.Tarefa`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a tela deixou de ler o JSON do protótipo.
 *
 * Antes, esta tela montava a agenda de `public/dados/agenda.json` (nove tarefas
 * de exemplo), filtrava e contava no navegador, e gravava conclusões em
 * `localStorage`. Agora ela lê **103.339 tarefas migradas do Vórtice** por
 * `/api/v1/tarefas`, e os contadores do topo vêm de `/api/v1/relatorios/agenda`
 * — um `GROUP BY` no banco, dentro do filtro global da filial do cabeçalho.
 *
 * ---------------------------------------------------------------------------
 * AS TRÊS COISAS QUE MUDARAM DE SIGNIFICADO, e por que a tela as escreve:
 *
 * 1. **O ATRASO É MEDIDO CONTRA A DATA AGENDADA, não contra o prazo.** 81.571
 *    das 103.339 tarefas migradas não têm prazo limite, porque nenhuma das 178
 *    ações em uso no sistema de origem declara prazo (documento 25, §3.1).
 *    Medir por prazo faria 79% da agenda parecer em dia por falta de dado, que
 *    é o pior tipo de indicador verde. Quem calcula o atraso é a API, e a
 *    coluna de prazo diz "não declarado na origem" quando ele não existe.
 *
 * 2. **Não há "Nova tarefa" nem "Concluir".** Nenhuma rota de relacionamento
 *    escreve, e é decisão declarada — processo e tarefa carregam o motor de
 *    regras (concluir tarefa gera a próxima), que entra junto com a tela que o
 *    exercita (dívida D-9 do documento 23). Um botão que gravasse em
 *    `localStorage` daria a impressão de que a agenda mudou; ela não mudaria.
 *
 * 3. **"Só as minhas" usa o usuário do contexto de acesso**, e não um seletor
 *    de CEN. Hoje ela devolve zero: as 273 pessoas migradas do Vórtice não são
 *    o usuário provisório do cabeçalho. A tela mostra o motivo que a própria
 *    API devolve, em vez de uma lista vazia sem explicação.
 */

import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { GraficoDonutCentro } from '../componentes/GraficoDonutCentro';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { listarTarefas, obterPainelDaAgenda, TAREFAS_INICIAL } from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import {
  SITUACOES_DE_TAREFA,
  type ConsultaDeTarefas,
  type OrdemDeTarefa,
  type TarefaResumo,
} from '../tipos/relacionamento';
import { formatarData, formatarDataHora } from './cadastro/formato';

/** As colunas, e quais delas a API sabe ordenar. O resto é exibição. */
const COLUNAS: { rotulo: string; ordem?: OrdemDeTarefa }[] = [
  { rotulo: 'Tarefa', ordem: 'Assunto' },
  { rotulo: 'Cliente' },
  { rotulo: 'Responsável' },
  { rotulo: 'Agendada para', ordem: 'AgendadaPara' },
  { rotulo: 'Prazo limite', ordem: 'PrazoLimite' },
  { rotulo: 'Prioridade', ordem: 'Prioridade' },
  { rotulo: 'Situação' },
];

/** O rótulo que cada situação do domínio fechado recebe na tela. */
const NOME_DA_SITUACAO: Record<string, string> = {
  Pendente: 'Pendente',
  EmAndamento: 'Em andamento',
  Concluida: 'Concluída',
  Cancelada: 'Cancelada',
  Reatribuida: 'Reatribuída',
};

/** De 1 (alta) a 5 (baixa) — a escala é da entidade, não da tela. */
const NOME_DA_PRIORIDADE: Record<number, string> = {
  1: 'Alta',
  2: 'Acima do normal',
  3: 'Normal',
  4: 'Abaixo do normal',
  5: 'Baixa',
};

export function Agenda() {
  const { contexto } = useContextoDeAcesso();
  const [consulta, setConsulta] = useState<ConsultaDeTarefas>(TAREFAS_INICIAL);

  // O painel do topo acompanha só o "minhas": os outros filtros valem para a
  // lista, e um contador que mudasse junto com o filtro de data deixaria de ser
  // "o tamanho da minha fila" para virar "o tamanho do que estou olhando".
  const painel = useRecurso(
    (sinal) => obterPainelDaAgenda(contexto, consulta.minhas, sinal),
    [contexto.empresa, contexto.usuario, consulta.minhas],
  );

  const lista = useRecurso(
    (sinal) => listarTarefas(contexto, consulta, sinal),
    [contexto.empresa, contexto.usuario, JSON.stringify(consulta)],
  );

  const numeros = painel.dados?.itens[0] ?? null;

  /**
   * As quatro janelas de prazo das pendentes, exclusivas entre si.
   *
   * `atrasadas`, `paraHoje` e `proximosSeteDias` vêm contadas no banco e não se
   * sobrepõem — vencida, hoje, e a semana que vem. O que sobra das pendentes é
   * o que está agendado para depois disso, e é subtração, não estimativa.
   * `null` quando não há pendente: um donut de zero não desenha nada.
   */
  const faixasDaAgenda = useMemo(() => {
    if (!numeros || numeros.pendentes <= 0) return null;
    const adiante = Math.max(
      0,
      numeros.pendentes - numeros.atrasadas - numeros.paraHoje - numeros.proximosSeteDias,
    );
    return [
      { nome: 'Vencidas', cor: '#DC2626', valor: numeros.atrasadas },
      { nome: 'Para hoje', cor: '#C1660A', valor: numeros.paraHoje },
      { nome: 'Próximos 7 dias', cor: '#367C2B', valor: numeros.proximosSeteDias },
      { nome: 'Mais adiante', cor: '#9CA3AF', valor: adiante },
    ].filter((f) => f.valor > 0);
  }, [numeros]);
  const pagina = lista.dados;

  const temFiltro = useMemo(
    () =>
      consulta.minhas ||
      consulta.somenteAtrasadas ||
      consulta.situacao !== TAREFAS_INICIAL.situacao ||
      consulta.de !== '' ||
      consulta.ate !== '',
    [consulta],
  );

  const indicadores: Indicador[] = [
    {
      rotulo: 'Pendentes',
      valor: numeros?.pendentes ?? null,
      deOnde: 'tarefas em Pendente ou Em andamento',
      semDado: 'sem tarefa ao alcance deste contexto',
    },
    {
      rotulo: 'Atrasadas',
      valor: numeros?.atrasadas ?? null,
      tom: numeros && numeros.atrasadas > 0 ? 'atencao' : 'neutro',
      deOnde: 'a data agendada já passou e ninguém concluiu',
      semDado: 'sem tarefa ao alcance deste contexto',
    },
    {
      rotulo: 'Para hoje',
      valor: numeros?.paraHoje ?? null,
      deOnde: 'agendadas para a data de hoje',
      semDado: '—',
    },
    {
      rotulo: 'Próximos 7 dias',
      valor: numeros?.proximosSeteDias ?? null,
      deOnde: 'agendadas para a semana que vem',
      semDado: '—',
    },
    {
      rotulo: 'Concluídas em 30 dias',
      valor: numeros?.concluidasNosUltimosTrintaDias ?? null,
      tom: 'bom',
      deOnde: 'com data, autor e desfecho registrados',
      semDado: '—',
    },
    {
      rotulo: 'Sem prazo declarado',
      valor: numeros?.semPrazoLimite ?? null,
      deOnde: 'das pendentes — a origem não declara prazo em nenhuma ação',
      semDado: '—',
    },
  ];

  function trocarOrdem(campo: OrdemDeTarefa) {
    setConsulta((c) =>
      c.ordenarPor === campo
        ? { ...c, descendente: !c.descendente, pagina: 1 }
        : { ...c, ordenarPor: campo, descendente: false, pagina: 1 },
    );
  }

  function limparFiltros() {
    setConsulta({ ...TAREFAS_INICIAL, tamanho: consulta.tamanho });
  }

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Agenda do CEN</h1>
          <p className="page-subtitle">
            <code>processo.Tarefa</code> — 103.339 tarefas migradas do Vórtice, recorte de 2026. A
            filial do cabeçalho define o que aparece aqui.
          </p>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={lista.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={painel.carregando} />

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}

      <MetricasSemDado metricas={painel.dados?.metricasSemDado} />

      {/* A AGENDA NÃO TINHA NENHUM DESENHO, e o painel já tinha o que desenhar.

          As quatro janelas — vencida, hoje, os próximos sete dias e o que vem
          depois — são exclusivas entre si e somam as pendentes. Elas já vinham
          contadas no banco, uma por indicador; o que faltava era mostrar o
          tamanho relativo, que é o que responde se a agenda está em dia ou se
          virou um passivo. Nada aqui é somado no navegador. */}
      {faixasDaAgenda && (
        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Como as pendências estão distribuídas</div>
              <div className="card-subtitle">
                as {numeros!.pendentes.toLocaleString('pt-BR')} tarefas pendentes por janela de
                prazo · contadas no banco
              </div>
            </div>
            <SeloProcedencia procedencia={painel.procedencia} />
          </div>
          <div className="v360-donut-wrap cad-donut-estreito">
            <GraficoDonutCentro
              segmentos={faixasDaAgenda.map((f) => ({ valor: f.valor, cor: f.cor }))}
              largura={160}
              altura={160}
              cutout="70%"
              bordaBranca
              centro={{
                linha1: `${Math.round((numeros!.atrasadas / Math.max(1, numeros!.pendentes)) * 100)}%`,
                linha2: 'vencidas',
                corLinha1: '#DC2626',
              }}
            />
            <div className="v360-donut-legenda">
              {faixasDaAgenda.map((f) => (
                <div className="v360-legenda-item" key={f.nome}>
                  <span className="dot" style={{ background: f.cor }} />
                  {f.nome} <strong>{f.valor.toLocaleString('pt-BR')}</strong>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      <div className="cad-barra">
        <label className="cad-filtro">
          Situação
          <select
            value={consulta.situacao}
            onChange={(e) => setConsulta((c) => ({ ...c, situacao: e.target.value, pagina: 1 }))}
          >
            <option value="">Todas</option>
            {SITUACOES_DE_TAREFA.map((s) => (
              <option key={s} value={s}>
                {NOME_DA_SITUACAO[s]}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro">
          Agendada a partir de
          <input
            type="date"
            value={consulta.de}
            onChange={(e) => setConsulta((c) => ({ ...c, de: e.target.value, pagina: 1 }))}
          />
        </label>

        <label className="cad-filtro">
          Até
          <input
            type="date"
            value={consulta.ate}
            onChange={(e) => setConsulta((c) => ({ ...c, ate: e.target.value, pagina: 1 }))}
          />
        </label>

        <label className="cad-filtro cad-filtro-caixa">
          <input
            type="checkbox"
            checked={consulta.somenteAtrasadas}
            onChange={(e) => setConsulta((c) => ({ ...c, somenteAtrasadas: e.target.checked, pagina: 1 }))}
          />
          Só as atrasadas
        </label>

        <label className="cad-filtro cad-filtro-caixa">
          <input
            type="checkbox"
            checked={consulta.minhas}
            onChange={(e) => setConsulta((c) => ({ ...c, minhas: e.target.checked, pagina: 1 }))}
          />
          Só as minhas
        </label>

        {temFiltro && (
          <button type="button" className="btn btn-secondary btn-sm" onClick={limparFiltros}>
            Limpar filtros
          </button>
        )}
      </div>

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Tarefas desta filial</div>
            <div className="card-subtitle">
              {lista.recarregando ? 'Atualizando…' : `${(pagina?.total ?? 0).toLocaleString('pt-BR')} no total`}
            </div>
          </div>
          <SeloProcedencia procedencia={lista.procedencia} />
        </div>

        {lista.carregando && <BlocoCarregando oQue="a agenda" />}
        {lista.erro && <BlocoErro erro={lista.erro} aoTentarDeNovo={lista.recarregar} />}

        {pagina && !lista.erro && pagina.itens.length === 0 && (
          <BlocoVazio
            titulo={
              consulta.minhas
                ? 'Nenhuma tarefa atribuída a você'
                : temFiltro
                  ? 'Nenhuma tarefa com esses filtros'
                  : 'Esta filial não tem tarefa carregada'
            }
            texto={
              consulta.minhas
                ? 'A agenda migrada pertence às 273 pessoas que vieram do Vórtice; o usuário do cabeçalho é o provisório de desenvolvimento e não é uma delas. Desmarque "Só as minhas" para ver a agenda da filial.'
                : temFiltro
                  ? 'Nenhuma tarefa da filial cai nesta combinação de situação, período e atraso.'
                  : 'A carga de 2026 trouxe tarefa para as treze filiais em operação. Se esta filial não mostra nenhuma, confira a filial escolhida no cabeçalho.'
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
            <div className="cad-tabela-wrap cad-so-largo">
              <table className="cad-tabela">
                <caption className="cad-so-leitor">
                  Tarefas da filial {contexto.empresa}, ordenadas por {consulta.ordenarPor}
                </caption>
                <thead>
                  <tr>
                    {COLUNAS.map((coluna) => (
                      <th key={coluna.rotulo} scope="col" aria-sort={ariaOrdem(coluna.ordem, consulta)}>
                        {coluna.ordem ? (
                          <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem!)}>
                            {coluna.rotulo}
                            <span aria-hidden="true">{seta(coluna.ordem, consulta)}</span>
                          </button>
                        ) : (
                          coluna.rotulo
                        )}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {pagina.itens.map((tarefa) => (
                    <tr key={tarefa.chave}>
                      <td>
                        {/* O TIPO SÓ APARECE QUANDO ELE ACRESCENTA ALGUMA COISA.

                            Na carga do Vórtice o assunto da tarefa é, na maior
                            parte das linhas, o próprio nome do tipo — "Agendar
                            Entrega Técnica Máq/imp" escrito duas vezes, uma em
                            cima da outra, dobrando a altura de cada linha da
                            tabela sem dizer nada de novo. */}
                        <div className="cad-link-forte">{tarefa.assunto}</div>
                        {!repete(tarefa.assunto, tarefa.tipoTarefaNome) && (
                          <div className="cad-sub">{tarefa.tipoTarefaNome}</div>
                        )}
                        {tarefa.processoTitulo && <div className="cad-sub">{tarefa.processoTitulo}</div>}
                      </td>
                      <td>
                        {tarefa.clienteChave && tarefa.clienteNome ? (
                          <Link to={`/clientes/${tarefa.clienteChave}`} className="cad-link-forte">
                            {tarefa.clienteNome}
                          </Link>
                        ) : (
                          <span className="cad-nada">sem cliente na origem</span>
                        )}
                      </td>
                      <td>{tarefa.responsavelNome}</td>
                      <td className="cad-mono">
                        {formatarDataHora(tarefa.agendadaPara)}
                        {tarefa.estaAtrasada && (
                          <div className="cad-alerta">
                            {tarefa.diasDeAtraso} {tarefa.diasDeAtraso === 1 ? 'dia' : 'dias'} de atraso
                          </div>
                        )}
                      </td>
                      <td className="cad-mono">
                        {tarefa.prazoLimite ? (
                          formatarDataHora(tarefa.prazoLimite)
                        ) : (
                          <span className="cad-nada" title="A origem não declara prazo em nenhuma das 178 ações em uso.">
                            não declarado
                          </span>
                        )}
                      </td>
                      <td>{NOME_DA_PRIORIDADE[tarefa.prioridade] ?? tarefa.prioridade}</td>
                      <td>
                        <span className={`cad-selo cad-selo-${tarefa.situacao.toLowerCase()}`}>
                          {NOME_DA_SITUACAO[tarefa.situacao] ?? tarefa.situacao}
                        </span>
                        {tarefa.resultadoNome && <div className="cad-sub">{tarefa.resultadoNome}</div>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Em 390px a tabela de sete colunas não tem conserto por rolagem:
                rolar para o lado esconde a coluna que identifica a linha. */}
            <div className="cad-fichas cad-so-estreito">
              {pagina.itens.map((tarefa) => (
                <FichaDeTarefa key={tarefa.chave} tarefa={tarefa} />
              ))}
            </div>

            <BarraDePaginacao
              pagina={pagina}
              oQue="tarefas"
              aoTrocarPagina={(p) => setConsulta((c) => ({ ...c, pagina: p }))}
              aoTrocarTamanho={(t) => setConsulta((c) => ({ ...c, tamanho: t, pagina: 1 }))}
            />
          </>
        )}
      </div>

      <BlocoRecolhivel
        titulo="O que esta tela ainda não faz"
        resumo="concluir, reagendar e criar tarefa"
      >
        <div className="cad-fichas">
          <p className="cad-estado-texto">
            <strong>Concluir, reagendar e criar tarefa não existem aqui</strong>, e é decisão
            declarada: nenhuma rota de relacionamento escreve. Processo e tarefa carregam o motor de
            regras — concluir uma tarefa gera a próxima e move a fase do processo —, e ele entra
            junto com a tela que o exercita (dívida D-9 do documento 23). Um botão que gravasse só no
            navegador daria a impressão de que a agenda mudou, e ela não mudaria.
          </p>
        </div>
      </BlocoRecolhivel>
    </>
  );
}

/** A mesma tarefa, em ficha, para quando a tabela não cabe. */
function FichaDeTarefa({ tarefa }: { tarefa: TarefaResumo }) {
  return (
    <div className="cad-ficha">
      <div className="cad-ficha-titulo">{tarefa.assunto}</div>
      {!repete(tarefa.assunto, tarefa.tipoTarefaNome) && (
        <div className="cad-sub">{tarefa.tipoTarefaNome}</div>
      )}

      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Cliente</span>
        <span className="cad-ficha-valor">
          {tarefa.clienteChave && tarefa.clienteNome ? (
            <Link to={`/clientes/${tarefa.clienteChave}`}>{tarefa.clienteNome}</Link>
          ) : (
            <span className="cad-nada">sem cliente na origem</span>
          )}
        </span>
      </div>

      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Agendada para</span>
        <span className="cad-ficha-valor">{formatarDataHora(tarefa.agendadaPara)}</span>
      </div>

      {tarefa.estaAtrasada && (
        <div className="cad-ficha-linha">
          <span className="cad-ficha-rotulo">Atraso</span>
          <span className="cad-ficha-valor cad-alerta">
            {tarefa.diasDeAtraso} {tarefa.diasDeAtraso === 1 ? 'dia' : 'dias'}
          </span>
        </div>
      )}

      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Prazo limite</span>
        <span className="cad-ficha-valor">
          {tarefa.prazoLimite ? formatarData(tarefa.prazoLimite) : <span className="cad-nada">não declarado</span>}
        </span>
      </div>

      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Responsável</span>
        <span className="cad-ficha-valor">{tarefa.responsavelNome}</span>
      </div>

      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Situação</span>
        <span className="cad-ficha-valor">
          <span className={`cad-selo cad-selo-${tarefa.situacao.toLowerCase()}`}>
            {NOME_DA_SITUACAO[tarefa.situacao] ?? tarefa.situacao}
          </span>
        </span>
      </div>
    </div>
  );
}

function ariaOrdem(campo: OrdemDeTarefa | undefined, consulta: ConsultaDeTarefas) {
  if (!campo || consulta.ordenarPor !== campo) return undefined;
  return consulta.descendente ? ('descending' as const) : ('ascending' as const);
}

function seta(campo: OrdemDeTarefa | undefined, consulta: ConsultaDeTarefas) {
  if (!campo || consulta.ordenarPor !== campo) return '';
  return consulta.descendente ? '▾' : '▴';
}

/**
 * Se o segundo texto não acrescenta nada ao primeiro.
 *
 * Compara sem acento, sem caixa e sem espaço repetido, porque a origem grava o
 * mesmo nome de duas maneiras — "Agendar Entrega Técnica Máq/imp" e "AGENDAR
 * ENTREGA TECNICA MAQ/IMP" — e as duas são a mesma coisa para quem lê.
 */
function repete(principal: string, secundario: string): boolean {
  const limpar = (t: string) =>
    t
      .normalize('NFD')
      .replace(/\p{Diacritic}/gu, '')
      .replace(/\s+/g, ' ')
      .trim()
      .toLowerCase();
  return limpar(principal) === limpar(secundario);
}
