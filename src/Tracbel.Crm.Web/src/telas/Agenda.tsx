/**
 * Agenda do CEN — ligada ao dado real de `processo.Tarefa`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a tela deixou de ler o JSON do protótipo.
 *
 * Antes, esta tela montava a agenda de `public/dados/agenda.json` (nove tarefas
 * de exemplo), filtrava e contava no navegador, e gravava conclusões em
 * `localStorage`. Agora ela lê **as tarefas dos processos do Vórtice dos
 * clientes do CRM** (a onda 2 da rotina PROCESSOS_VORTICE, documento 52 §12)
 * por `/api/v1/tarefas`, e os contadores do topo vêm de
 * `/api/v1/relatorios/agenda` — um `GROUP BY` no banco, dentro do filtro global
 * da filial do cabeçalho.
 *
 * ---------------------------------------------------------------------------
 * AS TRÊS COISAS QUE MUDARAM DE SIGNIFICADO, e por que a tela as escreve:
 *
 * 1. **O ATRASO É MEDIDO CONTRA A DATA AGENDADA, não contra o prazo.** Nenhuma
 *    das 178 ações em uso no sistema de origem declara prazo (documento 25,
 *    §3.1), e a maior parte das tarefas chega sem ele. Medir por prazo faria a
 *    agenda parecer em dia por falta de dado, que é o pior tipo de indicador
 *    verde. Quem calcula o atraso é a API, e a coluna de prazo diz "não
 *    declarado na origem" quando ele não existe.
 *
 * 2. **Não há "Nova tarefa" nem "Concluir".** Nenhuma rota de relacionamento
 *    escreve, e é decisão declarada — processo e tarefa carregam o motor de
 *    regras (concluir tarefa gera a próxima), que entra junto com a tela que o
 *    exercita (dívida D-9 do documento 23). Um botão que gravasse em
 *    `localStorage` daria a impressão de que a agenda mudou; ela não mudaria.
 *
 * 3. **"Só as minhas" usa o usuário do contexto de acesso**, e não um seletor
 *    de CEN. A tarefa trazida do Vórtice fica com quem a tem lá quando o login
 *    dele casa com uma conta do CRM; se não casa, fica com o dono do processo
 *    (decisão P2 de 27/09/2026). Quem não tem tarefa no próprio nome vê zero, e
 *    a tela diz por quê, em vez de uma lista vazia sem explicação.
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS (#293, bloco 2). O cabeçalho com a hora da leitura, a barra de
 * filtros, os seis números como `CartaoDeDecisao` em duas linhas de três e duas seções — a fila de pendências e as
 * tarefas — em `PainelDoMomento`. O `title=` do prazo virou dica que abre pelo teclado. NENHUM NÚMERO, REGRA OU TEXTO
 * DE REGRA MUDOU.
 */

import {
  AlarmClock,
  CalendarCheck,
  CalendarDays,
  CalendarRange,
  CalendarX,
  CheckCircle2,
  ListFilter,
  ListTodo,
  RefreshCw,
} from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { GraficoDonutCentro } from '../componentes/GraficoDonutCentro';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
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
import { CampoDoFiltro } from '../componentes/comum/CampoDoFiltro';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';

/** As colunas, quais delas a API sabe ordenar, e quais são data (alinhadas à direita). O resto é exibição. */
const COLUNAS: { rotulo: string; ordem?: OrdemDeTarefa; numerica?: boolean }[] = [
  { rotulo: 'Tarefa', ordem: 'Assunto' },
  { rotulo: 'Cliente' },
  { rotulo: 'Responsável' },
  { rotulo: 'Agendada para', ordem: 'AgendadaPara', numerica: true },
  { rotulo: 'Prazo limite', ordem: 'PrazoLimite', numerica: true },
  { rotulo: 'Prioridade', ordem: 'Prioridade' },
  { rotulo: 'Situação' },
];

/** A ordem da lista, dita como a tela a chama. */
const ROTULO_DA_ORDEM: Record<OrdemDeTarefa, string> = {
  AgendadaPara: 'data agendada',
  Prioridade: 'prioridade',
  PrazoLimite: 'prazo limite',
  Assunto: 'assunto',
};

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

  // SEM TAREFA AO ALCANCE, O NÚMERO É O TRAÇO COM O MOTIVO; LENDO, O CARTÃO PULSA E NÃO AFIRMA MOTIVO NENHUM.
  const semTarefa = painel.carregando ? undefined : 'Sem tarefa ao alcance deste contexto.';
  const numero = (v: number | undefined) => (v === undefined ? null : v.toLocaleString('pt-BR'));

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Agenda do CEN</h1>
          <p className="page-subtitle">
            As tarefas dos processos do Vórtice dos clientes cadastrados no CRM. A filial do cabeçalho define o que aparece
            aqui.
          </p>
        </div>
        <p className="dash-atualizado">
          {lista.procedencia ? <DadosAtualizadosEm procedencia={lista.procedencia} /> : 'Lendo a agenda…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              painel.recarregar();
              lista.recarregar();
            }}
            disabled={painel.carregando || lista.carregando}
            data-carregando={painel.carregando || lista.carregando ? 'true' : 'false'}
            aria-label="Reler a agenda"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES. Os filtros valem para a lista; o painel do topo acompanha só o "Só as minhas". */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <CampoDoFiltro icone={ListFilter} rotulo="Situação" bloco="situacao">
            <select value={consulta.situacao} onChange={(e) => setConsulta((c) => ({ ...c, situacao: e.target.value, pagina: 1 }))}>
              <option value="">Todas</option>
              {SITUACOES_DE_TAREFA.map((s) => (
                <option key={s} value={s}>
                  {NOME_DA_SITUACAO[s]}
                </option>
              ))}
            </select>
          </CampoDoFiltro>

          <CampoDoFiltro icone={CalendarDays} rotulo="Agendada a partir de" bloco="de">
            <input type="date" value={consulta.de} onChange={(e) => setConsulta((c) => ({ ...c, de: e.target.value, pagina: 1 }))} />
          </CampoDoFiltro>

          <CampoDoFiltro icone={CalendarDays} rotulo="Até" bloco="ate">
            <input type="date" value={consulta.ate} onChange={(e) => setConsulta((c) => ({ ...c, ate: e.target.value, pagina: 1 }))} />
          </CampoDoFiltro>

          <div className="dash-filtros-acao">
            <label className="dash-caixa">
              <input
                type="checkbox"
                checked={consulta.somenteAtrasadas}
                onChange={(e) => setConsulta((c) => ({ ...c, somenteAtrasadas: e.target.checked, pagina: 1 }))}
              />
              Só as atrasadas
            </label>
          </div>

          <div className="dash-filtros-acao">
            <label className="dash-caixa">
              <input
                type="checkbox"
                checked={consulta.minhas}
                onChange={(e) => setConsulta((c) => ({ ...c, minhas: e.target.checked, pagina: 1 }))}
              />
              Só as minhas
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

      {/* SEIS NÚMEROS, EM DUAS LINHAS DE TRÊS: quatro mais dois deixaria dois cartões sozinhos numa linha larga. */}
      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="3">
        <CartaoDeDecisao
          rotulo="Pendentes"
          icone={ListTodo}
          tom="demanda"
          valor={numero(numeros?.pendentes)}
          carregando={painel.carregando}
          unidade="tarefas"
          motivoSemDado={semTarefa}
          variacao={numeros ? 'em Pendente ou Em andamento' : null}
          sobre="Tarefas em Pendente ou Em andamento, nesta filial — com ou sem data."
        />
        <CartaoDeDecisao
          rotulo="Atrasadas"
          icone={AlarmClock}
          tom="captura"
          valor={numero(numeros?.atrasadas)}
          carregando={painel.carregando}
          unidade="tarefas"
          motivoSemDado={semTarefa}
          variacao={numeros ? 'a data agendada já passou' : null}
          sobre="A data agendada já passou e ninguém concluiu. O atraso é medido contra a data agendada, e não contra o prazo: a origem quase nunca declara prazo."
        />
        <CartaoDeDecisao
          rotulo="Para hoje"
          icone={CalendarCheck}
          tom="mercado"
          valor={numero(numeros?.paraHoje)}
          carregando={painel.carregando}
          unidade="tarefas"
          motivoSemDado={semTarefa}
          variacao={numeros ? 'agendadas para hoje' : null}
          sobre="Tarefas agendadas para a data de hoje, ainda não concluídas."
        />
        <CartaoDeDecisao
          rotulo="Próximos 7 dias"
          icone={CalendarRange}
          tom="oportunidade"
          valor={numero(numeros?.proximosSeteDias)}
          carregando={painel.carregando}
          unidade="tarefas"
          motivoSemDado={semTarefa}
          variacao={numeros ? 'agendadas para a semana que vem' : null}
          sobre="Tarefas agendadas para os próximos sete dias."
        />
        <CartaoDeDecisao
          rotulo="Concluídas em 30 dias"
          icone={CheckCircle2}
          tom="neutro"
          valor={numero(numeros?.concluidasNosUltimosTrintaDias)}
          carregando={painel.carregando}
          unidade="tarefas"
          motivoSemDado={semTarefa}
          variacao={numeros ? 'com data, autor e desfecho' : null}
          sobre="Tarefas concluídas nos últimos 30 dias, com a data, quem concluiu e o desfecho registrados."
        />
        <CartaoDeDecisao
          rotulo="Sem prazo declarado"
          icone={CalendarX}
          tom="neutro"
          valor={numero(numeros?.semPrazoLimite)}
          carregando={painel.carregando}
          unidade="tarefas"
          motivoSemDado={semTarefa}
          variacao={numeros ? 'das pendentes' : null}
          sobre="Das pendentes, as que não têm prazo limite — a origem não declara prazo em nenhuma ação."
        />
      </div>

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}

      <MetricasSemDado metricas={painel.dados?.metricasSemDado} />

      {/* A AGENDA NÃO TINHA NENHUM DESENHO, e o painel já tinha o que desenhar.

          As quatro janelas — vencida, hoje, os próximos sete dias e o que vem
          depois — são exclusivas entre si e somam as pendentes. Elas já vinham
          contadas no banco, uma por indicador; o que faltava era mostrar o
          tamanho relativo, que é o que responde se a agenda está em dia ou se
          virou um passivo. Nada aqui é somado no navegador. */}
      {faixasDaAgenda && (
        <section className="dash-secao" data-bloco="secao-fila">
          <TituloDaSecao
            titulo="A fila de pendências"
            subtitulo={`As ${numeros!.pendentes.toLocaleString('pt-BR')} tarefas pendentes por janela de prazo.`}
            metodologia="Vencidas, para hoje e próximos 7 dias vêm contadas no banco e não se sobrepõem. O que sobra das pendentes é o que está agendado para depois disso — subtração, e não estimativa."
          />
          <PainelDoMomento
            titulo="Como as pendências estão distribuídas"
            data-bloco="distribuicao"
            subtitulo="Contadas no banco, dentro da filial do cabeçalho."
            dica="A fatia vermelha é o que já venceu. O centro da rosca diz quanto das pendentes está vencido."
          >
            <div className="v360-donut-wrap cad-donut-agenda">
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
                    <span>{f.nome}</span>
                    {/* O tamanho da janela entre as pendentes — o mesmo que a rosca desenha. */}
                    <span className="cad-donut-agenda-barra" aria-hidden="true">
                      <span
                        style={{
                          width: `${(f.valor / Math.max(1, numeros!.pendentes)) * 100}%`,
                          background: f.cor,
                        }}
                      />
                    </span>
                    <strong>{f.valor.toLocaleString('pt-BR')}</strong>
                  </div>
                ))}
              </div>
            </div>
          </PainelDoMomento>
        </section>
      )}

      <section className="dash-secao" data-bloco="secao-tarefas">
        <TituloDaSecao
          titulo="As tarefas"
          subtitulo={lista.recarregando ? 'Atualizando…' : `${(pagina?.total ?? 0).toLocaleString('pt-BR')} no total, com os filtros da barra.`}
          metodologia="A tarefa trazida do Vórtice fica com quem a tem lá quando o login dessa pessoa casa com uma conta do CRM; se não casa, fica com o dono do processo. O atraso é medido contra a data agendada."
        />

        <PainelDoMomento
          titulo="Tarefas desta filial"
          data-bloco="tarefas"
          subtitulo={`Ordenadas por ${ROTULO_DA_ORDEM[consulta.ordenarPor] ?? consulta.ordenarPor}${consulta.descendente ? ', do maior para o menor' : ''}.`}
          dica="Clique no título de uma coluna com seta para ordenar por ela. O cliente abre o 360 dele."
        >
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
                  ? 'Nenhuma tarefa está no seu nome. A tarefa trazida do Vórtice fica com quem a tem lá quando o login dessa pessoa casa com uma conta do CRM; se não casa, fica com o dono do processo. Desmarque "Só as minhas" para ver a agenda da filial.'
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
              <div className="mom-tabela-rolagem cad-so-largo">
                <table className="mom-tabela">
                  <caption className="cad-so-leitor">
                    Tarefas da filial {contexto.empresa}, ordenadas por {consulta.ordenarPor}
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
                        <th scope="row">
                          {/* O TIPO SÓ APARECE QUANDO ELE ACRESCENTA ALGUMA COISA.

                              Na carga do Vórtice o assunto da tarefa é, na maior
                              parte das linhas, o próprio nome do tipo — "Agendar
                              Entrega Técnica Máq/imp" escrito duas vezes, uma em
                              cima da outra, dobrando a altura de cada linha da
                              tabela sem dizer nada de novo. */}
                          <div className="cad-link-forte">{tarefa.assunto}</div>
                          {!repete(tarefa.assunto, tarefa.tipoTarefaNome) && <div className="cad-sub">{tarefa.tipoTarefaNome}</div>}
                          {tarefa.processoTitulo && <div className="cad-sub">{tarefa.processoTitulo}</div>}
                        </th>
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
                        <td className="mom-num">
                          {formatarDataHora(tarefa.agendadaPara)}
                          {tarefa.estaAtrasada && (
                            <div className="cad-alerta">
                              {tarefa.diasDeAtraso} {tarefa.diasDeAtraso === 1 ? 'dia' : 'dias'} de atraso
                            </div>
                          )}
                        </td>
                        <td className="mom-num">
                          {tarefa.prazoLimite ? (
                            formatarDataHora(tarefa.prazoLimite)
                          ) : (
                            <>
                              <span className="cad-nada">não declarado</span>{' '}
                              <InfoTooltip
                                texto="A origem não declara prazo em nenhuma das 178 ações em uso."
                                rotulo="Por que o prazo não está declarado"
                              />
                            </>
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
        </PainelDoMomento>
      </section>

      <BlocoRecolhivel titulo="O que esta tela ainda não faz" resumo="concluir, reagendar e criar tarefa">
        <div className="cad-fichas">
          <p className="cad-estado-texto">
            <strong>Concluir, reagendar e criar tarefa não existem aqui</strong>, e é decisão
            declarada: nenhuma rota de relacionamento escreve. Processo e tarefa carregam o motor de
            regras — concluir uma tarefa gera a próxima e move a fase do processo —, e ele entra
            {/* A citação "(dívida D-9 do documento 23)" saiu da tela: o número do documento não diz nada
                a quem lê, e a referência continua aqui. */}
            junto com a tela que o exercita. Um botão que gravasse só no navegador daria a impressão de
            que a agenda mudou, e ela não mudaria.
          </p>
        </div>
      </BlocoRecolhivel>
    </PaginaDoPainel>
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
