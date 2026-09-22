/**
 * UMA ROTINA DO SERVIDOR (issue 136): a agenda, ligada ou não, a próxima execução, a última e o que falta.
 *
 * QUEM RODA É O ORQUESTRADOR, a cada cinco minutos: "Rodar agora" põe o pedido na fila, e ele começa em até cinco
 * minutos. A tela muda QUANDO a rotina roda — O QUE ela roda vem do código.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { listarExecucoes, pedirExecucao, reagendarRotina, type CadenciaDaRotina, type RotinaNaTela } from '../../../dados/api/integracoes';
import { useRecurso } from '../../../dados/api/useRecurso';
import { formatarDataHora } from '../../../telas/cadastro/formato';
import { useEnvio } from '../potencial/useEnvio';

const MESES = ['janeiro', 'fevereiro', 'março', 'abril', 'maio', 'junho', 'julho', 'agosto', 'setembro', 'outubro', 'novembro', 'dezembro'];

const RESULTADO: Record<string, { l: string; cor: string }> = {
  Sucesso: { l: 'Sucesso', cor: '#22C55E' },
  Falha: { l: 'Falha', cor: '#EF4444' },
  Ignorada: { l: 'Não rodou', cor: '#F59E0B' },
  EmAndamento: { l: 'Rodando', cor: '#3B82F6' },
};

const MOTIVO = { Agenda: 'pela agenda', Pedido: 'rodar agora', PrimeiraCarga: 'primeira carga' } as const;

function Resultado({ resultado }: { resultado: string | null }) {
  const s = (resultado && RESULTADO[resultado]) || { l: 'Nunca rodou', cor: '#94A3B8' };
  return (
    <span className="int-status" style={{ background: `${s.cor}20`, color: s.cor, borderColor: `${s.cor}40` }}>
      <span className="int-dot" style={{ background: s.cor }} />
      {s.l}
    </span>
  );
}

function Historico({ codigo }: { codigo: string }) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => listarExecucoes(contexto, codigo, sinal), [contexto.empresa, codigo]);
  if (leitura.carregando) return <div className="muted">Carregando o histórico…</div>;
  if (leitura.erro) return <div className="muted">Não foi possível ler o histórico.</div>;
  if (!leitura.dados?.length) return <div className="muted">Ainda não rodou pelo orquestrador.</div>;
  return (
    <ul className="int-historico">
      {leitura.dados.map((e) => (
        <li key={e.iniciadaEm}>
          <Resultado resultado={e.resultado} />
          <span className="mono">{formatarDataHora(e.iniciadaEm)}</span>
          <span className="muted">
            {MOTIVO[e.motivo]}
            {e.pedidaPor ? ` · ${e.pedidaPor}` : ''} · {e.maquina}
          </span>
          <span>{e.mensagem ?? '—'}</span>
        </li>
      ))}
    </ul>
  );
}

function EditorDeAgenda({ rotina: r, aoSalvar, aoCancelar }: { rotina: RotinaNaTela; aoSalvar: () => void; aoCancelar: () => void }) {
  const { contexto } = useContextoDeAcesso();
  const envio = useEnvio(['cadencia', 'mes', 'dia', 'hora', 'intervaloMinutos']);
  const [cadencia, setCadencia] = useState<CadenciaDaRotina>(r.cadencia);
  const [mes, setMes] = useState(r.mes ?? 1);
  const [dia, setDia] = useState(r.dia?.toString() ?? '1');
  const [hora, setHora] = useState(r.hora ?? '03:00');
  const [minutos, setMinutos] = useState(r.intervaloMinutos?.toString() ?? '60');
  const [ligada, setLigada] = useState(r.estaLigada);
  const id = (campo: string) => `rotina-${r.codigo}-${campo}`;

  async function salvar() {
    const salva = await envio.enviar(() =>
      reagendarRotina(contexto, r.codigo, {
        cadencia,
        mes: cadencia === 'Anual' ? mes : null,
        dia: cadencia === 'Anual' || cadencia === 'Mensal' ? Number(dia) : null,
        hora: cadencia === 'Intervalo' ? null : hora,
        intervaloMinutos: cadencia === 'Intervalo' ? Number(minutos) : null,
        ligada,
      }),
    );
    if (salva) aoSalvar();
  }

  return (
    <div className="int-editor">
      {envio.aviso && (
        <div className="config-hint adm-aviso" role="alert">
          <strong>{envio.aviso.titulo}</strong>
          {envio.aviso.texto && <div>{envio.aviso.texto}</div>}
        </div>
      )}
      <div className="form-grid">
        <div className="form-field">
          <label htmlFor={id('cadencia')}>Frequência</label>
          <select id={id('cadencia')} value={cadencia} onChange={(e) => setCadencia(e.target.value as CadenciaDaRotina)}>
            <option value="Anual">Uma vez por ano</option>
            <option value="Mensal">Todo mês</option>
            <option value="Diaria">Todo dia</option>
            <option value="Intervalo">A cada tantos minutos</option>
          </select>
        </div>
        {cadencia === 'Anual' && (
          <div className="form-field">
            <label htmlFor={id('mes')}>Mês</label>
            <select id={id('mes')} value={mes} onChange={(e) => setMes(Number(e.target.value))}>
              {MESES.map((m, i) => (
                <option key={m} value={i + 1}>
                  {m}
                </option>
              ))}
            </select>
          </div>
        )}
        {(cadencia === 'Anual' || cadencia === 'Mensal') && (
          <div className="form-field">
            <label htmlFor={id('dia')}>Dia (1 a 28)</label>
            <input id={id('dia')} inputMode="numeric" value={dia} onChange={(e) => setDia(e.target.value.replace(/\D/g, ''))} />
            {envio.erros.dia && <div className="form-erro">{envio.erros.dia}</div>}
          </div>
        )}
        {cadencia !== 'Intervalo' && (
          <div className="form-field">
            <label htmlFor={id('hora')}>Hora (São Paulo)</label>
            <input id={id('hora')} type="time" value={hora} onChange={(e) => setHora(e.target.value)} />
            {envio.erros.hora && <div className="form-erro">{envio.erros.hora}</div>}
          </div>
        )}
        {cadencia === 'Intervalo' && (
          <div className="form-field">
            <label htmlFor={id('minutos')}>A cada (15 a 1440 minutos)</label>
            <input id={id('minutos')} inputMode="numeric" value={minutos} onChange={(e) => setMinutos(e.target.value.replace(/\D/g, ''))} />
            {envio.erros.intervaloMinutos && <div className="form-erro">{envio.erros.intervaloMinutos}</div>}
          </div>
        )}
        <div className="form-field">
          <label htmlFor={id('ligada')}>Situação</label>
          <label className="int-ligada">
            <input id={id('ligada')} type="checkbox" checked={ligada} onChange={(e) => setLigada(e.target.checked)} /> Ligada — roda pela agenda
          </label>
        </div>
      </div>
      <p className="config-hint">A nova agenda vale daqui para a frente: a execução que ficou para trás da agenda antiga não é cobrada.</p>
      <div className="adm-acoes">
        <button type="button" className="btn btn-secondary" onClick={aoCancelar}>
          Cancelar
        </button>
        <button type="button" className="btn btn-primary" disabled={envio.enviando} onClick={salvar}>
          {envio.enviando ? 'Salvando…' : 'Salvar agenda'}
        </button>
      </div>
    </div>
  );
}

type Props = { rotina: RotinaNaTela; podeAdministrar: boolean; aoMudar: () => void };

export function CartaoDeRotina({ rotina: r, podeAdministrar, aoMudar }: Props) {
  const { contexto } = useContextoDeAcesso();
  const envio = useEnvio([]);
  const [editando, setEditando] = useState(false);
  const [historico, setHistorico] = useState(false);

  async function rodarAgora() {
    const pedida = await envio.enviar(() => pedirExecucao(contexto, r.codigo));
    if (pedida) aoMudar();
  }

  return (
    <div className={`integracao-item ${r.estaLigada ? '' : 'int-desativada'}`}>
      <div className="int-icon" aria-hidden>
        ⏱️
      </div>
      <div className="int-info">
        <div className="int-header">
          <strong>{r.nome}</strong>
          <span className="int-tipo">{r.estaLigada ? 'ligada' : 'desligada'}</span>
          <Resultado resultado={r.ultimoResultado} />
          {r.naFila && <span className="int-tipo int-fila">na fila</span>}
        </div>
        <div className="int-meta">
          <div>
            <span>Agenda:</span> {r.agenda}
          </div>
          <div>
            <span>Próxima:</span>{' '}
            {r.naFila ? 'em até 5 minutos (pedido na fila)' : r.proximaExecucaoEm ? <span className="mono">{formatarDataHora(r.proximaExecucaoEm)}</span> : 'desligada'}
          </div>
          <div>
            <span>Última:</span>{' '}
            {r.ultimaExecucaoIniciadaEm ? <span className="mono">{formatarDataHora(r.ultimaExecucaoIniciadaEm)}</span> : <span className="muted">nunca pelo orquestrador</span>}
          </div>
          <div>
            <span>Cargas:</span> <span className="mono">{r.cargas.join(' ')}</span>
          </div>
        </div>
        <div className="int-notas">{r.descricao}</div>
        {r.pendencia && (
          <div className="int-resultado int-erro" role="note">
            {r.pendencia}
          </div>
        )}
        {r.ultimaMensagem && <div className="int-resumo muted">{r.ultimaMensagem}</div>}
        {envio.aviso && (
          <div className="config-hint adm-aviso" role="alert">
            <strong>{envio.aviso.titulo}</strong>
            {envio.aviso.texto && <div>{envio.aviso.texto}</div>}
          </div>
        )}
        {historico && <Historico codigo={r.codigo} />}
        {editando && (
          <EditorDeAgenda
            rotina={r}
            aoSalvar={() => {
              setEditando(false);
              aoMudar();
            }}
            aoCancelar={() => setEditando(false)}
          />
        )}
      </div>
      <div className="int-acoes">
        {podeAdministrar && (
          <button
            type="button"
            className="btn-config-inline"
            disabled={envio.enviando || r.naFila || r.pendencia !== null}
            title={r.pendencia ?? undefined}
            onClick={rodarAgora}
          >
            {r.naFila ? 'Na fila' : 'Rodar agora'}
          </button>
        )}
        {podeAdministrar && (
          <button type="button" className="btn-config-inline" onClick={() => setEditando((v) => !v)} aria-expanded={editando}>
            Agenda
          </button>
        )}
        <button type="button" className="btn-config-inline" onClick={() => setHistorico((v) => !v)} aria-expanded={historico}>
          Histórico
        </button>
      </div>
    </div>
  );
}
