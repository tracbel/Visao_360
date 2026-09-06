/**
 * Aba Comercial › Metas e SLA — porte de `renderSecaoMetas`
 * (prototipo/referencia/assets/app.js linha 5503). Os valores de fábrica vêm de
 * `config-metas.json`; o que a pessoa altera é gravado por
 * `dados/persistenciaConfig.ts`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — os onze campos numéricos passaram a ser gravados. Antes eram
 * `defaultValue`, e alterar a meta da classe A, salvar e voltar à seção
 * devolvia o número antigo, **sem nenhum aviso de que a alteração se perdeu**.
 */
import type { PreferenciasConfig } from '../../dados/persistenciaConfig';
import type { ConfigMetas } from '../../tipos/configuracoes';
import { CardConfig, ConfigFooterAcoes } from './ConfigPartes';

const CLASSES = ['A', 'B', 'C', 'D'] as const;

/** Campo numérico só aceita número, e nunca fica `NaN`: vazio vale zero. */
function numero(valor: string): number {
  const n = parseInt(valor, 10);
  return Number.isFinite(n) ? Math.max(0, n) : 0;
}

export function ConfigSecaoMetas({
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
  const metas = rascunho.metas;

  function mudarMeta(campo: keyof ConfigMetas, valor: number) {
    aoMudar({ metas: { ...metas, [campo]: valor } });
  }

  function mudarFrequencia(qual: 'frequencia_visita' | 'frequencia_ligacao', classe: 'A' | 'B' | 'C' | 'D', valor: number) {
    aoMudar({ metas: { ...metas, [qual]: { ...metas[qual], [classe]: valor } } });
  }

  return (
    <>
      <CardConfig titulo="Frequência de visita presencial (dias · meta máxima)">
        <div className="form-grid cols-4">
          {CLASSES.map((c) => (
            <div className="form-field" key={c}>
              <label htmlFor={`metaVisita${c}`}>
                Classe <strong>{c}</strong>
              </label>
              <div className="input-suffix">
                <input
                  id={`metaVisita${c}`}
                  type="number"
                  min={1}
                  value={metas.frequencia_visita[c]}
                  onChange={(e) => mudarFrequencia('frequencia_visita', c, numero(e.target.value))}
                />
                <span>dias</span>
              </div>
            </div>
          ))}
        </div>
        <div className="config-hint">
          Cliente Classe {CLASSES[0]} sem visita há mais que {metas.frequencia_visita.A} dias vira "atraso" no
          dashboard de cobertura.
        </div>
      </CardConfig>

      <CardConfig titulo="Frequência de contato remoto (ligação/WhatsApp/e-mail)">
        <div className="form-grid cols-4">
          {CLASSES.map((c) => (
            <div className="form-field" key={c}>
              <label htmlFor={`metaRemoto${c}`}>
                Classe <strong>{c}</strong>
              </label>
              <div className="input-suffix">
                <input
                  id={`metaRemoto${c}`}
                  type="number"
                  min={1}
                  value={metas.frequencia_ligacao[c]}
                  onChange={(e) => mudarFrequencia('frequencia_ligacao', c, numero(e.target.value))}
                />
                <span>dias</span>
              </div>
            </div>
          ))}
        </div>
      </CardConfig>

      <CardConfig titulo="SLA de aprovações (horas)">
        <div className="form-grid cols-3">
          <div className="form-field">
            <label htmlFor="slaGerente">Gerente Regional</label>
            <div className="input-suffix">
              <input
                id="slaGerente"
                type="number"
                min={1}
                value={metas.sla_aprovacao_gerente}
                onChange={(e) => mudarMeta('sla_aprovacao_gerente', numero(e.target.value))}
              />
              <span>h</span>
            </div>
            <span className="field-hint">Depois disso escala para Diretor</span>
          </div>
          <div className="form-field">
            <label htmlFor="slaDiretor">Diretor Comercial</label>
            <div className="input-suffix">
              <input
                id="slaDiretor"
                type="number"
                min={1}
                value={metas.sla_aprovacao_diretor}
                onChange={(e) => mudarMeta('sla_aprovacao_diretor', numero(e.target.value))}
              />
              <span>h</span>
            </div>
            <span className="field-hint">Depois vira alerta vermelho</span>
          </div>
          <div className="form-field">
            <label htmlFor="slaFinanceiro">Financeiro (limite de crédito)</label>
            <div className="input-suffix">
              <input
                id="slaFinanceiro"
                type="number"
                min={1}
                value={metas.sla_aprovacao_financeiro}
                onChange={(e) => mudarMeta('sla_aprovacao_financeiro', numero(e.target.value))}
              />
              <span>h</span>
            </div>
          </div>
        </div>
      </CardConfig>

      <ConfigFooterAcoes temAlteracao={temAlteracao} onSalvar={onSalvar} onCancelar={onCancelar} />
    </>
  );
}
