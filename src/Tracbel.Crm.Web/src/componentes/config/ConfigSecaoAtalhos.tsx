/**
 * Aba Minhas preferências › Atalhos e produtividade — porte de
 * `renderSecaoAtalhos` (prototipo/referencia/assets/app.js linha 5467).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a assinatura passou a ser gravada, e a lista de atalhos deixou
 * de prometer o que não existe.
 *
 * Os seis atalhos estavam escritos como se funcionassem — inclusive o `Ctrl+K`,
 * que a barra de busca do cabeçalho também anuncia. Nenhum deles tem
 * `keydown` em lugar nenhum da aplicação. Como é uma lista de referência e não
 * um controle, ela ficou, mas com a marca de que ainda não vale: quem lê a tela
 * fica sabendo, em vez de descobrir apertando.
 */
import type { PreferenciasConfig } from '../../dados/persistenciaConfig';
import { CardConfig, ConfigFooterAcoes } from './ConfigPartes';

const ATALHOS: [string, string][] = [
  ['Abrir busca global', 'Ctrl+K'],
  ['Nova oportunidade', 'Ctrl+Shift+O'],
  ['Registrar visita', 'Ctrl+Shift+V'],
  ['Ir para Agenda', 'G A'],
  ['Ir para Pipeline', 'G P'],
  ['Ir para Clientes', 'G C'],
];

export function ConfigSecaoAtalhos({
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
      <CardConfig titulo="Atalhos de teclado">
        <div className="config-hint" style={{ marginBottom: 12 }}>
          <strong>Ainda não estão ligados.</strong> Esta é a combinação planejada para cada ação; nenhuma delas
          responde ao teclado hoje. A lista fica aqui para valer como decisão — quando os atalhos existirem, é este
          o mapa que eles seguem.
        </div>
        <div className="atalhos-grid">
          {ATALHOS.map(([acao, teclas]) => (
            <div className="atalho-row" key={acao}>
              <span className="atalho-acao">{acao}</span>
              <span className="atalho-teclas">
                {teclas.split(' ').map((t, i) => (
                  <kbd key={i}>{t}</kbd>
                ))}
              </span>
            </div>
          ))}
        </div>
      </CardConfig>

      <CardConfig titulo="Assinatura de e-mail (registro de contato)">
        <div className="form-field">
          <label htmlFor="atalhosAssinatura">Rodapé automático em contatos enviados pelo CRM</label>
          <textarea
            id="atalhosAssinatura"
            rows={4}
            value={rascunho.assinaturaEmail}
            onChange={(e) => aoMudar({ assinaturaEmail: e.target.value })}
          />
        </div>
      </CardConfig>

      <ConfigFooterAcoes temAlteracao={temAlteracao} onSalvar={onSalvar} onCancelar={onCancelar} />
    </>
  );
}
