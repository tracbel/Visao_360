/**
 * Aba Comercial › Taxonomias — porte de `renderSecaoTaxonomias`
 * (prototipo/referencia/assets/app.js linha 5620). Dados vêm de
 * `config-motivos-perda.json` e `config-categorias-interacao.json` via
 * `useDados` (ver telas/Configuracoes.tsx).
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — os três botões sem destino saíram: "Editar" em cada linha das
 * duas tabelas e "+ Adicionar motivo". Não existe formulário de taxonomia, e
 * inventar um agora seria decidir sozinho a lista de motivos de perda de
 * verdade, que é do negócio (dívida P-4 do padrão de tela). As duas tabelas
 * continuam sendo a lista de referência — que é do que a tela de fechar
 * oportunidade se serve.
 */
import type { ConfigCategoriaInteracao, ConfigMotivoPerda } from '../../tipos/configuracoes';
import { CardConfig } from './ConfigPartes';

export function ConfigSecaoTaxonomias({
  motivos,
  categorias,
}: {
  motivos: ConfigMotivoPerda[];
  categorias: ConfigCategoriaInteracao[];
}) {
  return (
    <>
      <CardConfig titulo="Motivos de perda de oportunidade">
        <table className="config-tabela">
          <thead>
            <tr>
              <th>#</th>
              <th>Motivo</th>
              <th style={{ textAlign: 'right' }}>Uso (90d)</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {motivos.map((m) => (
              <tr key={m.id}>
                <td className="mono">{m.id}</td>
                <td>{m.motivo}</td>
                <td style={{ textAlign: 'right' }} className="mono">
                  {m.usos_90d}
                </td>
                <td>
                  {m.ativo ? (
                    <span className="badge-status ok">Ativo</span>
                  ) : (
                    <span className="badge-status muted">Inativo</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="config-hint" style={{ marginTop: 12 }}>
          Estes motivos são a lista obrigatória que aparece ao marcar uma oportunidade como perdida, no Pipeline.
          Editar a lista depende da definição dos motivos reais pelo negócio — por isso ainda não há formulário aqui.
        </div>
      </CardConfig>

      <CardConfig titulo="Categorias de interação (contato com cliente)">
        <table className="config-tabela">
          <thead>
            <tr>
              <th>Categoria</th>
              <th>Zera contador de cobertura</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {categorias.map((c) => (
              <tr key={c.id}>
                <td>{c.nome}</td>
                <td>
                  {c.vale_cobertura ? (
                    <span className="badge-status ok">Sim</span>
                  ) : (
                    <span className="badge-status muted">Não</span>
                  )}
                </td>
                <td>
                  {c.ativo ? (
                    <span className="badge-status ok">Ativo</span>
                  ) : (
                    <span className="badge-status muted">Inativo</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="config-hint" style={{ marginTop: 12 }}>
          Cliente Classe A "exige visita" ignora categorias marcadas como "não zera contador" na análise de
          cobertura.
        </div>
      </CardConfig>

    </>
  );
}
