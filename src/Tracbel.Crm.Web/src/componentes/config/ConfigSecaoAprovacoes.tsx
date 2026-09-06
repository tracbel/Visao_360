/**
 * Aba Comercial › Políticas de aprovação — porte de `renderSecaoAprovacoes`
 * (prototipo/referencia/assets/app.js linha 5560).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — as três alçadas e os quatro interruptores passaram a ser
 * gravados (`dados/persistenciaConfig.ts`), e o desenho das faixas passou a
 * acompanhar o que está sendo digitado — antes ele lia o JSON e ficava parado
 * enquanto os números embaixo mudavam, mostrando duas verdades ao mesmo tempo.
 */
import type { PreferenciasConfig } from '../../dados/persistenciaConfig';
import type { ConfigMetas } from '../../tipos/configuracoes';
import { CardConfig, ConfigFooterAcoes, ConfigToggleRow } from './ConfigPartes';

function numero(valor: string): number {
  const n = parseInt(valor, 10);
  return Number.isFinite(n) ? Math.max(0, n) : 0;
}

/** Largura da faixa em relação à maior alçada. Nunca divide por zero. */
function largura(valor: number, teto: number): string {
  if (teto <= 0) return '0%';
  return `${Math.min(100, (valor / teto) * 100)}%`;
}

export function ConfigSecaoAprovacoes({
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
  const teto = metas.desconto_ate_presidente;

  function mudarMeta(campo: keyof ConfigMetas, valor: number) {
    aoMudar({ metas: { ...metas, [campo]: valor } });
  }

  return (
    <>
      <CardConfig titulo="Alçadas de desconto">
        <div className="alcadas-visual">
          <div className="alcada-linha">
            <div className="alcada-role">CEN</div>
            <div className="alcada-range" style={{ width: largura(metas.desconto_ate_gerente, teto) }}>
              <span>Até {metas.desconto_ate_gerente}%</span>
            </div>
          </div>
          <div className="alcada-linha">
            <div className="alcada-role">Gerente Regional</div>
            <div className="alcada-range warn" style={{ width: largura(metas.desconto_ate_diretor, teto) }}>
              <span>Até {metas.desconto_ate_diretor}%</span>
            </div>
          </div>
          <div className="alcada-linha">
            <div className="alcada-role">Diretor Comercial</div>
            <div className="alcada-range danger" style={{ width: '100%' }}>
              <span>Até {teto}%</span>
            </div>
          </div>
          <div className="alcada-linha">
            <div className="alcada-role">Presidência</div>
            <div className="alcada-range dark">
              <span>Acima de {teto}% · caso a caso</span>
            </div>
          </div>
        </div>

        <div className="form-grid cols-3" style={{ marginTop: 20 }}>
          <div className="form-field">
            <label htmlFor="alcadaGerente">Desconto até (Gerente aprova)</label>
            <div className="input-suffix">
              <input
                id="alcadaGerente"
                type="number"
                min={0}
                max={100}
                value={metas.desconto_ate_gerente}
                onChange={(e) => mudarMeta('desconto_ate_gerente', numero(e.target.value))}
              />
              <span>%</span>
            </div>
          </div>
          <div className="form-field">
            <label htmlFor="alcadaDiretor">Desconto até (Diretor aprova)</label>
            <div className="input-suffix">
              <input
                id="alcadaDiretor"
                type="number"
                min={0}
                max={100}
                value={metas.desconto_ate_diretor}
                onChange={(e) => mudarMeta('desconto_ate_diretor', numero(e.target.value))}
              />
              <span>%</span>
            </div>
          </div>
          <div className="form-field">
            <label htmlFor="alcadaPresidencia">Desconto até (Presidência aprova)</label>
            <div className="input-suffix">
              <input
                id="alcadaPresidencia"
                type="number"
                min={0}
                max={100}
                value={metas.desconto_ate_presidente}
                onChange={(e) => mudarMeta('desconto_ate_presidente', numero(e.target.value))}
              />
              <span>%</span>
            </div>
          </div>
        </div>
      </CardConfig>

      <CardConfig titulo="Fluxo automático">
        <div className="config-list">
          <ConfigToggleRow
            titulo="Escalar automaticamente após vencer SLA"
            desc={`Após ${metas.sla_aprovacao_gerente}h no Gerente sem resposta, envia ao Diretor`}
            ativo={rascunho.escalarAposSla}
            aoMudar={(v) => aoMudar({ escalarAposSla: v })}
          />
          <ConfigToggleRow
            titulo="Bloquear geração de proposta acima da alçada"
            desc="Impede CEN de gerar PDF sem aprovação prévia"
            ativo={rascunho.bloquearPropostaAcimaAlcada}
            aoMudar={(v) => aoMudar({ bloquearPropostaAcimaAlcada: v })}
          />
          <ConfigToggleRow
            titulo={`Exigir justificativa obrigatória em descontos > ${metas.desconto_ate_gerente}%`}
            desc="Campo de texto livre com mínimo 30 caracteres"
            ativo={rascunho.exigirJustificativaDesconto}
            aoMudar={(v) => aoMudar({ exigirJustificativaDesconto: v })}
          />
          <ConfigToggleRow
            titulo="Notificar Financeiro em oportunidades > R$ 5M"
            desc="Antecipa análise de limite de crédito"
            ativo={rascunho.notificarFinanceiroAcimaCinco}
            aoMudar={(v) => aoMudar({ notificarFinanceiroAcimaCinco: v })}
          />
        </div>
      </CardConfig>

      <ConfigFooterAcoes temAlteracao={temAlteracao} onSalvar={onSalvar} onCancelar={onCancelar} />
    </>
  );
}
