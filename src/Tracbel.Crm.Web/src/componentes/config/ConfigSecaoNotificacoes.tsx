/**
 * Aba Minhas preferências › Notificações — porte de `renderSecaoNotificacoes`
 * (prototipo/referencia/assets/app.js linha 5435).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — os quatro interruptores e as três regras passaram a ser gravados
 * (`dados/persistenciaConfig.ts`). Antes eram `defaultChecked`/`defaultValue`,
 * e a preferência sumia ao trocar de seção — depois do aviso de "salvas".
 */
import type { PreferenciasConfig } from '../../dados/persistenciaConfig';
import { CardConfig, ConfigFooterAcoes, ConfigToggleRow } from './ConfigPartes';

const PARADA = ['7 dias', '14 dias', '30 dias'];
const SEM_CONTATO = ['Padrão da classe', '7 dias além da meta', '15 dias além da meta'];
const SILENCIO = ['Fim de semana e feriados', 'Nunca', 'Após 19h'];

export function ConfigSecaoNotificacoes({
  rascunho,
  aoMudar,
  temAlteracao,
  onSalvar,
  onCancelar,
}: {
  rascunho: PreferenciasConfig;
  aoMudar: (mudanca: Partial<PreferenciasConfig>) => void;
  temAlteracao: boolean;
  onSalvar: () => void;
  onCancelar: () => void;
}) {
  return (
    <>
      <CardConfig titulo="Canais preferidos">
        <div className="config-list">
          <ConfigToggleRow
            titulo="Notificações no navegador"
            desc="Alertas de aprovação, tarefa vencendo e menções"
            ativo={rascunho.notificaNavegador}
            aoMudar={(v) => aoMudar({ notificaNavegador: v })}
          />
          <ConfigToggleRow
            titulo="E-mail para itens críticos"
            desc="Aprovações travadas > 24h, oportunidades acima de R$ 3M"
            ativo={rascunho.notificaEmailCritico}
            aoMudar={(v) => aoMudar({ notificaEmailCritico: v })}
          />
          <ConfigToggleRow
            titulo="Resumo diário por e-mail"
            desc="Enviado às 07:00 com pipeline, tarefas e alertas"
            ativo={rascunho.notificaResumoDiario}
            aoMudar={(v) => aoMudar({ notificaResumoDiario: v })}
          />
          <ConfigToggleRow
            titulo="WhatsApp para aprovações"
            desc="Requer número validado com Meta Business"
            ativo={rascunho.notificaWhatsapp}
            aoMudar={(v) => aoMudar({ notificaWhatsapp: v })}
          />
        </div>
      </CardConfig>

      <CardConfig titulo="Regras específicas">
        <div className="form-grid">
          <div className="form-field">
            <label htmlFor="notifParada">Alertar quando oportunidade fica parada por</label>
            <select
              id="notifParada"
              value={rascunho.alertaOportunidadeParada}
              onChange={(e) => aoMudar({ alertaOportunidadeParada: e.target.value })}
            >
              {PARADA.map((o) => (
                <option key={o}>{o}</option>
              ))}
            </select>
          </div>
          <div className="form-field">
            <label htmlFor="notifSemContato">Cliente sem contato há mais de</label>
            <select
              id="notifSemContato"
              value={rascunho.alertaClienteSemContato}
              onChange={(e) => aoMudar({ alertaClienteSemContato: e.target.value })}
            >
              {SEM_CONTATO.map((o) => (
                <option key={o}>{o}</option>
              ))}
            </select>
          </div>
          <div className="form-field">
            <label htmlFor="notifSilencio">Silenciar notificações</label>
            <select
              id="notifSilencio"
              value={rascunho.silenciarNotificacoes}
              onChange={(e) => aoMudar({ silenciarNotificacoes: e.target.value })}
            >
              {SILENCIO.map((o) => (
                <option key={o}>{o}</option>
              ))}
            </select>
          </div>
        </div>
      </CardConfig>

      <ConfigFooterAcoes temAlteracao={temAlteracao} onSalvar={onSalvar} onCancelar={onCancelar} />
    </>
  );
}
