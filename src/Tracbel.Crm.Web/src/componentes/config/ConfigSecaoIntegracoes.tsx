/**
 * Aba TI e Integrações › Integrações — porte de `renderSecaoIntegracoes`
 * (prototipo/referencia/assets/app.js linha 5671). Dados vêm de
 * `config-integracoes.json` via `useDados` (ver telas/Configuracoes.tsx).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — os botões sem destino saíram: "Testar" e "Logs" em cada
 * integração e "+ Conectar novo sistema". Testar uma integração exige chamar a
 * integração; nenhuma delas está ligada nesta rodada. O rodapé "Salvar
 * alterações" também saiu: esta seção é só leitura, e não havia nada nela para
 * salvar.
 */
import type { ConfigIntegracao, StatusIntegracao } from '../../tipos/configuracoes';
import { CardConfig } from './ConfigPartes';

const STATUS_PILL: Record<StatusIntegracao, { l: string; cor: string }> = {
  online: { l: 'Online', cor: '#22C55E' },
  pendente: { l: 'Pendente', cor: '#F59E0B' },
  degradado: { l: 'Degradado', cor: '#EF4444' },
  somente_leitura: { l: 'Somente-leitura', cor: '#94A3B8' },
  offline: { l: 'Offline', cor: '#991B1B' },
};

export function ConfigSecaoIntegracoes({ integracoes }: { integracoes: ConfigIntegracao[] }) {
  return (
    <>
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
          Esta lista é leitura do estado das integrações. Conectar, testar ou ler o log de uma delas depende do
          serviço de integração, que ainda não existe nesta rodada.
        </div>
      </CardConfig>
    </>
  );
}
