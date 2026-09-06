/**
 * Aba Minhas preferências › Perfil e conta — porte de `renderSecaoPerfil`
 * (prototipo/referencia/assets/app.js linha 5370).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — o único campo editável passou a ser gravado, e os dois botões
 * sem destino saíram.
 *
 * **O que veio do AD continua não editável, e agora diz por quê.** Nome, e-mail,
 * cargo e regional são leitura de outro sistema — regra do documento 05 §5:
 * campo que vem de fora não se edita aqui, corrige-se na origem. O telefone é o
 * que o CRM guarda, e é o que esta tela grava.
 *
 * **"Trocar" (foto de perfil) saiu**: não existe upload de foto. **"Encerrar
 * todas" (as sessões) saiu** por dois motivos: não encerrava nada, e é uma ação
 * destrutiva — derrubaria a pessoa dos outros aparelhos — que não pode voltar
 * sem confirmação e sem um serviço de sessão por trás.
 */
import type { PreferenciasConfig } from '../../dados/persistenciaConfig';
import { CardConfig, ConfigFooterAcoes } from './ConfigPartes';

export function ConfigSecaoPerfil({
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
      <CardConfig titulo="Perfil e conta">
        <div className="form-grid">
          <div className="form-field">
            <label htmlFor="perfilNome">Nome completo</label>
            {/* Campo desabilitado NÃO leva texto de exemplo: placeholder em campo
                cinza é lido como valor gravado (padrão de tela §3.3). */}
            <input id="perfilNome" type="text" value="Hugo Rocha" disabled readOnly />
            <span className="field-hint">Sincronizado com AD · corrija na origem, não aqui</span>
          </div>
          <div className="form-field">
            <label htmlFor="perfilEmail">E-mail corporativo</label>
            <input id="perfilEmail" type="text" value="hugo.rocha@tracbel.com.br" disabled readOnly />
            <span className="field-hint">Login SSO Microsoft · vem do AD</span>
          </div>
          <div className="form-field">
            <label htmlFor="perfilCargo">Cargo</label>
            <input id="perfilCargo" type="text" value="Gerente de TI" disabled readOnly />
            <span className="field-hint">Vem do AD</span>
          </div>
          <div className="form-field">
            <label htmlFor="perfilRegional">Regional</label>
            <input id="perfilRegional" type="text" value="Nacional (TI)" disabled readOnly />
            <span className="field-hint">Vem do AD</span>
          </div>
          <div className="form-field">
            <label htmlFor="perfilTelefone">
              Telefone celular <span className="pill-editavel">editável</span>
            </label>
            <input
              id="perfilTelefone"
              type="text"
              value={rascunho.telefone}
              onChange={(e) => aoMudar({ telefone: e.target.value })}
            />
            <span className="field-hint">É o número que o CRM usa para as notificações por WhatsApp</span>
          </div>
          <div className="form-field">
            <label>Foto de perfil</label>
            <div className="foto-perfil-row">
              <div className="foto-atual" style={{ background: '#367C2B' }}>
                HR
              </div>
              {/* "Trocar" saiu: não existe upload de foto. */}
              <span className="field-hint">As iniciais vêm do nome do AD.</span>
            </div>
          </div>
        </div>
      </CardConfig>

      <CardConfig titulo="Sessão e segurança">
        <div className="config-list">
          <div className="config-list-row">
            <div>
              <div className="clr-title">Autenticação em duas etapas</div>
              <div className="clr-desc">Obrigatório pela política de TI · via Microsoft Authenticator</div>
            </div>
            <span className="badge-status ok">Ativa</span>
          </div>
          <div className="config-list-row">
            <div>
              <div className="clr-title">Sessão ativa em outros dispositivos</div>
              {/* "Encerrar todas" saiu: não encerrava nada, e é destrutivo. */}
              <div className="clr-desc">
                2 sessões · Windows Chrome e iOS Edge. Encerrar sessão de outro aparelho depende do serviço de
                sessão, que ainda não existe — por enquanto, faça isso pelo portal da Microsoft.
              </div>
            </div>
          </div>
          <div className="config-list-row">
            <div>
              <div className="clr-title">Último login</div>
              <div className="clr-desc">Hoje, 18:30 · Chrome 128 · IP interno 10.42.18.221</div>
            </div>
          </div>
        </div>
      </CardConfig>

      <ConfigFooterAcoes temAlteracao={temAlteracao} onSalvar={onSalvar} onCancelar={onCancelar} />
    </>
  );
}
