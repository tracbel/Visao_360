/**
 * Aba TI e Integrações › Integrações — porte de `renderSecaoIntegracoes`
 * (prototipo/referencia/assets/app.js linha 5671).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — os botões sem destino saíram: "Testar" e "Logs" em cada
 * integração e "+ Conectar novo sistema". O rodapé "Salvar alterações" também
 * saiu: esta seção é só leitura.
 *
 * 14/09/2026 — O CARTÃO "SINCRONIZAÇÕES" LÊ A API (documento 35, seção 11): cada
 * ciclo do serviço do Windows `TracbelCrmSincronizacaoArt`, com resultado,
 * tentativas, contagens e motivo da falha. É aqui que mora a informação técnica
 * da sincronização — e não numa tela exclusiva do ART. "Sistemas conectados"
 * continua lendo `config-integracoes.json`, do protótipo.
 */
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { listarSincronizacoes } from '../../dados/api/sincronizacoes';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarDataHora } from '../../telas/cadastro/formato';
import type { SituacaoDaSincronizacao } from '../../tipos/api';
import type { ConfigIntegracao, StatusIntegracao } from '../../tipos/configuracoes';
import { CardConfig } from './ConfigPartes';

const STATUS_PILL: Record<StatusIntegracao, { l: string; cor: string }> = {
  online: { l: 'Online', cor: '#22C55E' },
  pendente: { l: 'Pendente', cor: '#F59E0B' },
  degradado: { l: 'Degradado', cor: '#EF4444' },
  somente_leitura: { l: 'Somente-leitura', cor: '#94A3B8' },
  offline: { l: 'Offline', cor: '#991B1B' },
};

const RESULTADO: Record<string, { l: string; cor: string }> = {
  Sucesso: { l: 'Sucesso', cor: '#22C55E' },
  Falha: { l: 'Falha', cor: '#EF4444' },
  Ignorada: { l: 'Não rodou', cor: '#F59E0B' },
  EmAndamento: { l: 'Em andamento', cor: '#94A3B8' },
};

function Pill({ resultado }: { resultado: string | null }) {
  const s = (resultado && RESULTADO[resultado]) || { l: resultado ?? 'Nunca executado', cor: '#94A3B8' };
  return (
    <span className="int-status" style={{ background: `${s.cor}20`, color: s.cor, borderColor: `${s.cor}40` }}>
      <span className="int-dot" style={{ background: s.cor }} />
      {s.l}
    </span>
  );
}

function CartaoDeSincronizacoes() {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso<SituacaoDaSincronizacao[]>(
    (sinal) => listarSincronizacoes(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  return (
    <CardConfig titulo="Sincronizações do servidor">
      {leitura.carregando && <BlocoCarregando oQue="as sincronizações" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {leitura.dados && leitura.dados.length === 0 && (
        <div className="config-hint">
          Nenhuma execução registrada. O serviço TracbelCrmSincronizacaoArt grava cada ciclo aqui assim que roda no
          servidor.
        </div>
      )}
      {leitura.dados?.map((fluxo) => (
        <div className="integracao-item" key={`${fluxo.sistemaCodigo}-${fluxo.fluxo}`}>
          <div className="int-info">
            <div className="int-header">
              <strong>{fluxo.sistemaCodigo}</strong>
              <span className="int-tipo">{fluxo.fluxo}</span>
              <Pill resultado={fluxo.ultimoResultado} />
            </div>
            <div className="int-meta">
              <div>
                <span>Última execução:</span>{' '}
                <span className="mono">{fluxo.ultimaExecucaoEm ? formatarDataHora(fluxo.ultimaExecucaoEm) : '—'}</span>
              </div>
              <div>
                <span>Último sucesso:</span>{' '}
                <span className="mono">{fluxo.ultimoSucessoEm ? formatarDataHora(fluxo.ultimoSucessoEm) : 'nunca'}</span>
              </div>
            </div>
            <div className="cad-tabela-wrap" style={{ marginTop: 12 }}>
              <table className="cad-tabela">
                <thead>
                  <tr>
                    <th scope="col">Início</th>
                    <th scope="col">Resultado</th>
                    <th scope="col">Tentativas</th>
                    <th scope="col">Lidos</th>
                    <th scope="col">Incluídas</th>
                    <th scope="col">Atualizadas</th>
                    <th scope="col">Pendentes</th>
                    <th scope="col">Máquina</th>
                    <th scope="col">Resumo ou motivo</th>
                  </tr>
                </thead>
                <tbody>
                  {fluxo.execucoes.map((e) => (
                    <tr key={e.iniciadaEm}>
                      <td className="cad-mono">
                        {formatarDataHora(e.iniciadaEm)}
                        <div className="cad-sub">{e.terminadaEm ? `fim ${formatarDataHora(e.terminadaEm)}` : 'sem fim registrado'}</div>
                      </td>
                      <td>
                        <Pill resultado={e.resultado} />
                      </td>
                      <td className="cad-mono">{e.tentativas}</td>
                      <td className="cad-mono">{e.registrosLidos}</td>
                      <td className="cad-mono">{e.incluidos}</td>
                      <td className="cad-mono">{e.atualizados}</td>
                      <td className="cad-mono">{e.pendentes}</td>
                      <td className="cad-mono">{e.maquina}</td>
                      <td>{e.mensagem ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      ))}
      <div className="config-hint" style={{ marginTop: 16 }}>
        A sincronização roda no servidor, como serviço do Windows, sem depender de navegador nem de sessão aberta. Esta
        lista é só leitura; o detalhe de cada ciclo também fica no Log de Aplicativo do servidor.
      </div>
    </CardConfig>
  );
}

export function ConfigSecaoIntegracoes({ integracoes }: { integracoes: ConfigIntegracao[] }) {
  return (
    <>
      <CartaoDeSincronizacoes />
      <CardConfig titulo="Sistemas conectados">
        <div className="integracoes-lista">
          {integracoes.map((i) => {
            const s = STATUS_PILL[i.status] ?? STATUS_PILL.offline;
            return (
              <div className="integracao-item" key={i.nome}>
                <div className="int-icon">{i.icone}</div>
                <div className="int-info">
                  <div className="int-header">
                    <strong>{i.nome}</strong>
                    <span className="int-tipo">{i.tipo}</span>
                    <span
                      className="int-status"
                      style={{ background: `${s.cor}20`, color: s.cor, borderColor: `${s.cor}40` }}
                    >
                      <span className="int-dot" style={{ background: s.cor }} />
                      {s.l}
                    </span>
                  </div>
                  <div className="int-meta">
                    <div>
                      <span>Ambiente:</span> <span className="mono">{i.ambiente}</span>
                    </div>
                    <div>
                      <span>Endpoint:</span> <span className="mono">{i.endpoint}</span>
                    </div>
                    <div>
                      <span>Sync:</span>{' '}
                      {i.last_sync ? (
                        <>
                          <span className="mono">{i.last_sync}</span> · {i.frequencia}
                        </>
                      ) : (
                        <span className="muted">Nunca sincronizado</span>
                      )}
                    </div>
                    <div>
                      <span>Responsável:</span> {i.donos}
                    </div>
                  </div>
                  <div className="int-notas">{i.notas}</div>
                </div>
              </div>
            );
          })}
        </div>
        <div className="config-hint" style={{ marginTop: 16 }}>
          Esta lista ainda vem do protótipo e descreve o estado planejado de cada sistema. O estado real da sincronização
          do ART está no cartão acima.
        </div>
      </CardConfig>
    </>
  );
}
