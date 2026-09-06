/**
 * Ficha da oportunidade — `processo.Processo`, pela chave da rota.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a tela deixou de ser a ficha de UMA oportunidade inventada.
 *
 * Antes ela lia `public/dados/oportunidade-1517613.json` e vivia na rota fixa
 * `/oportunidades/1517613` — **existia uma oportunidade só no sistema inteiro**,
 * e ela não era real. Agora a rota é `/oportunidades/:chave` e qualquer um dos
 * 45.397 processos carregados abre, com a linha do tempo e a agenda dele ao
 * lado.
 *
 * ---------------------------------------------------------------------------
 * O QUE ESTA TELA DECLARA EM VEZ DE MOSTRAR:
 *
 * - **Valor estimado e valor final.** 0,8% dos processos declaram valor. Quando
 *   não há, o campo diz *"não declarado na origem"* — nunca `R$ 0,00`, que
 *   afirmaria uma negociação de valor zero.
 * - **Motivo da perda.** A restrição do banco exige motivo para encerrar como
 *   perdido, e o legado encerra sem exigir nenhum: os processos perdidos
 *   entraram apontando para `NAO_INFORMADO_NA_ORIGEM`. A ficha mostra esse
 *   código pelo que ele é — a ausência do motivo, e não um motivo.
 * - **Probabilidade e previsão.** `Fase.ProbabilidadePercentual` é nula nas 50
 *   fases; a previsão de conclusão existe em 7,6% dos processos.
 *
 * **Não há botão de mudar de fase, fechar ou perder.** Nenhuma rota de
 * relacionamento escreve: mudar de fase dispara automação e concluir tarefa
 * gera a próxima, e o motor de regras entra junto com a tela que o exercita
 * (dívida D-9 do documento 23). Dezoito botões sem efeito já saíram desta tela
 * numa rodada anterior; nenhum voltou.
 */

import { Link, useParams } from 'react-router-dom';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida } from '../componentes/cadastro/SemDado';
import { BlocoPainel, type EstadoBloco } from '../componentes/painel360/BlocoPainel';
import { useContextoDeAcesso } from '../dados/api/contexto';
import {
  INTERACOES_INICIAL,
  listarInteracoes,
  listarTarefas,
  obterProcesso,
  TAREFAS_INICIAL,
} from '../dados/api/relacionamento';
import { useRecurso, type Leitura } from '../dados/api/useRecurso';
import { formatarData, formatarDataHora, formatarDinheiro } from './cadastro/formato';

/** Quantas linhas cada fila mostra. */
const LINHAS = 10;

export function OportunidadeFicha() {
  const { chave = '' } = useParams();
  const { contexto } = useContextoDeAcesso();

  const leitura = useRecurso(
    (sinal) => obterProcesso(contexto, chave, sinal),
    [contexto.empresa, contexto.usuario, chave],
  );

  const ficha = leitura.dados;
  const clienteChave = ficha?.resumo.clienteChave ?? '';

  // A agenda e a linha do tempo são do CLIENTE, e não do processo: nenhuma das
  // duas rotas aceita `processoChave`. A tela diz isso em vez de deixar parecer
  // que a lista é a do processo.
  const agenda = useRecurso(
    (sinal) =>
      clienteChave
        ? listarTarefas(contexto, { ...TAREFAS_INICIAL, clienteChave, situacao: '', tamanho: LINHAS }, sinal)
        : Promise.resolve({ dados: null as never, procedencia: null }),
    [contexto.empresa, clienteChave],
  );

  const interacoes = useRecurso(
    (sinal) =>
      clienteChave
        ? listarInteracoes(contexto, { ...INTERACOES_INICIAL, clienteChave, tamanho: LINHAS }, sinal)
        : Promise.resolve({ dados: null as never, procedencia: null }),
    [contexto.empresa, clienteChave],
  );

  if (leitura.carregando) return <BlocoCarregando oQue="a oportunidade" />;
  if (leitura.erro) return <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />;
  if (!ficha) return <BlocoErro erro={new Error('A oportunidade não voltou da API.')} aoTentarDeNovo={leitura.recarregar} />;

  const r = ficha.resumo;

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">{r.titulo}</h1>
          <p className="page-subtitle">
            <code>processo.Processo</code> nº {r.numero} ·{' '}
            <Link to={`/clientes/${r.clienteChave}`}>{r.clienteNome}</Link> · leitura apenas
          </p>
        </div>
        <div className="page-actions">
          <Link to="/pipeline" className="btn btn-secondary">
            Voltar ao pipeline
          </Link>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={leitura.procedencia} />

      <div className="p360-grid">
        <BlocoPainel
          id="situacao"
          titulo="Situação e fase"
          subtitulo="a fase e há quanto tempo o processo está nela"
          fonte={<SeloProcedencia procedencia={leitura.procedencia} />}
          estado="ok"
        >
          <dl className="p360-dados">
            <Dado
              rotulo="Situação"
              valor={<span className={`cad-selo cad-selo-${r.situacao.toLowerCase()}`}>{r.situacao}</span>}
            />
            <Dado rotulo="Situação desde" valor={formatarData(ficha.situacaoDesde)} />
            <Dado rotulo="Fluxo" valor={r.tipoProcessoNome} />
            <Dado rotulo="Fase atual" valor={r.faseNome} />
            <Dado
              rotulo="Parado nesta fase há"
              valor={
                <span className={r.diasNaFase > 90 ? 'cad-alerta' : undefined}>
                  {r.diasNaFase.toLocaleString('pt-BR')} {r.diasNaFase === 1 ? 'dia' : 'dias'}
                </span>
              }
            />
            <Dado rotulo="Aberto em" valor={formatarDataHora(r.criadoEm)} />
            <Dado
              rotulo="Responsável"
              valor={r.proprietarioNome ?? <SemValor texto="login não encontrado no cadastro" />}
            />
            <Dado
              rotulo="Encerrado em"
              valor={ficha.concluidoEm ? formatarDataHora(ficha.concluidoEm) : <SemValor texto="ainda aberto" />}
            />
          </dl>
        </BlocoPainel>

        <BlocoPainel
          id="negocio"
          titulo="O negócio"
          subtitulo="valor, quantidade e previsão — quando a origem os declara"
          estado="ok"
        >
          <dl className="p360-dados">
            <Dado
              rotulo="Valor estimado"
              valor={
                r.valorEstimado === null ? (
                  <SemValor texto="não declarado na origem" />
                ) : (
                  formatarDinheiro(r.valorEstimado)
                )
              }
            />
            <Dado
              rotulo="Valor final"
              valor={
                ficha.valorFinal === null ? (
                  <SemValor texto="não declarado na origem" />
                ) : (
                  formatarDinheiro(ficha.valorFinal)
                )
              }
            />
            <Dado
              rotulo="Quantidade"
              valor={
                ficha.quantidade === null ? (
                  <SemValor texto="não declarada" />
                ) : (
                  ficha.quantidade.toLocaleString('pt-BR')
                )
              }
            />
            <Dado
              rotulo="Previsão de conclusão"
              valor={r.previsaoConclusao ? formatarData(r.previsaoConclusao) : <SemValor texto="sem previsão" />}
            />
            <Dado
              rotulo="Previsão original"
              valor={
                ficha.previsaoConclusaoOriginal ? (
                  formatarData(ficha.previsaoConclusaoOriginal)
                ) : (
                  <SemValor texto="sem previsão inicial registrada" />
                )
              }
            />
            <Dado
              rotulo="Motivo da perda"
              valor={
                ficha.motivoDePerdaCodigo === null ? (
                  <SemValor texto="não se aplica" />
                ) : ficha.motivoDePerdaCodigo === 'NAO_INFORMADO_NA_ORIGEM' ? (
                  <span className="cad-atencao">
                    não informado na origem — o legado encerra sem exigir motivo
                  </span>
                ) : (
                  ficha.motivoDePerdaCodigo
                )
              }
            />
          </dl>

          {ficha.descricao && (
            <p className="p360-item-obs" style={{ marginTop: 10 }}>
              <strong>Descrição:</strong> {ficha.descricao}
            </p>
          )}
          {ficha.observacaoDaPerda && (
            <p className="p360-item-obs">
              <strong>Observação da perda:</strong> {ficha.observacaoDaPerda}
            </p>
          )}
        </BlocoPainel>

        <BlocoPainel
          id="agenda"
          titulo="Agenda do cliente"
          subtitulo="processo.Tarefa — do CLIENTE, não deste processo: a rota não filtra por processo"
          fonte={<SeloProcedencia procedencia={agenda.procedencia} />}
          estado={estado(agenda, (agenda.dados?.itens.length ?? 0) > 0)}
          mensagemVazia="Nenhuma tarefa registrada para este cliente no recorte carregado."
          mensagemErro={agenda.erro?.message}
        >
          <ul className="p360-lista">
            {agenda.dados?.itens.map((t) => (
              <li className={`p360-item ${t.estaAtrasada ? 'p360-item-critico' : ''}`} key={t.chave}>
                <div className="p360-item-topo">
                  <span className="p360-item-titulo">{t.assunto}</span>
                  <span className="p360-item-data cad-mono">{formatarData(t.agendadaPara)}</span>
                </div>
                <div className="p360-item-meta">
                  {t.responsavelNome} · {t.situacao}
                  {t.estaAtrasada && ` · ${t.diasDeAtraso} dias de atraso`}
                </div>
              </li>
            ))}
          </ul>
        </BlocoPainel>

        <BlocoPainel
          id="interacoes"
          titulo="Linha do tempo do cliente"
          subtitulo="processo.Interacao — a mais recente primeiro"
          fonte={<SeloProcedencia procedencia={interacoes.procedencia} />}
          estado={estado(interacoes, (interacoes.dados?.itens.length ?? 0) > 0)}
          mensagemVazia="Nenhuma interação registrada com este cliente no recorte de 2026."
          mensagemErro={interacoes.erro?.message}
        >
          <ul className="p360-lista">
            {interacoes.dados?.itens.map((i) => (
              <li className="p360-item" key={i.chave}>
                <div className="p360-item-topo">
                  <span className="p360-item-titulo">{i.assunto}</span>
                  <span className="p360-item-data cad-mono">{formatarData(i.ocorridaEm)}</span>
                </div>
                <div className="p360-item-meta">
                  {i.tipoTarefaNome} · {i.autorNome}
                  {i.resultadoNome ? ` · ${i.resultadoNome}` : ''}
                </div>
              </li>
            ))}
          </ul>
        </BlocoPainel>
      </div>

      <div className="card cad-cartao">
        <div className="card-header">
          <div className="card-title">O que esta ficha não faz e não afirma</div>
        </div>
        <div className="cad-fichas">
          <p className="cad-estado-texto">
            <strong>Mudar de fase, fechar como ganha e marcar como perdida não existem aqui.</strong>{' '}
            Nenhuma rota de relacionamento escreve — mudar de fase dispara automação e concluir
            tarefa gera a próxima, e o motor de regras entra junto com a tela que o exercita (dívida
            D-9 do documento 23).
          </p>
          <LacunaConhecida
            metrica="Probabilidade de fechamento"
            motivo={
              'Fase.ProbabilidadePercentual é nula nas 50 fases carregadas. A origem tem uma coluna ' +
              'Perspectiva preenchida em 3.922 processos, mas por processo e não por fase — não dá ' +
              'para derivar dela a probabilidade desta fase.'
            }
          />
          <LacunaConhecida
            metrica="A agenda e o histórico DESTE processo"
            motivo={
              'As rotas /api/v1/tarefas e /api/v1/interacoes aceitam clienteChave, e não ' +
              'processoChave. As duas listas acima são do cliente inteiro, e os blocos dizem isso ' +
              'no subtítulo em vez de deixar parecer que são deste processo.'
            }
          />
        </div>
      </div>
    </>
  );
}

function estado(leitura: Leitura<unknown>, temConteudo: boolean): EstadoBloco {
  if (leitura.carregando) return 'carregando';
  if (leitura.erro) return 'erro';
  return temConteudo ? 'ok' : 'vazio';
}

function Dado({ rotulo, valor }: { rotulo: string; valor: React.ReactNode }) {
  return (
    <div className="p360-dado">
      <dt className="p360-dado-rotulo">{rotulo}</dt>
      <dd className="p360-dado-valor">{valor}</dd>
    </div>
  );
}

/** Ausência escrita em palavra, e nunca um zero. */
function SemValor({ texto }: { texto: string }) {
  return <span className="cad-nada">{texto}</span>;
}
