/**
 * Aba TI e Integrações › Permissões e roles — porte de `renderSecaoPermissoes`
 * (prototipo/referencia/assets/app.js linha 5768). `CONFIG_ROLES` vem de
 * `config-roles.json` via `useDados`; a matriz-resumo é literal no original
 * (não é um `CONFIG_*` à parte), então fica local aqui também.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — "Ver permissões", em cada linha de role, saiu: não abria nada, e
 * a matriz logo abaixo já é o detalhamento que ele prometia.
 */
import type { ConfigRole } from '../../tipos/configuracoes';
import { CardConfig } from './ConfigPartes';

function roleClasse(role: string): string {
  return role.toLowerCase().replace(/[^a-z]/g, '');
}

const MATRIZ: [string, string, string, string, string][] = [
  ['Ver própria carteira', '✓', '✓', '✓', '✓'],
  ['Ver carteira da regional', '—', '✓', '✓', '✓'],
  ['Ver carteira nacional', '—', '—', '✓', '✓'],
  ['Criar oportunidade', '✓', '✓', '✓', '—'],
  ['Aprovar desconto até 5%', '✓', '✓', '✓', '—'],
  ['Aprovar desconto até 10%', '—', '✓', '✓', '—'],
  ['Aprovar desconto até 20%', '—', '—', '✓', '—'],
  ['Reatribuir carteira', '—', '✓', '✓', '—'],
  ['Configurar integrações', '—', '—', '—', '✓'],
  ['Editar taxonomias', '—', '—', '✓', '✓'],
];

export function ConfigSecaoPermissoes({ roles }: { roles: ConfigRole[] }) {
  return (
    <>
      <CardConfig titulo="Roles e alçadas">
        <table className="config-tabela">
          <thead>
            <tr>
              <th>Role</th>
              <th style={{ textAlign: 'center' }}>Usuários</th>
              <th>Descrição</th>
            </tr>
          </thead>
          <tbody>
            {roles.map((r) => (
              <tr key={r.role}>
                <td>
                  <span className={`role-pill role-${roleClasse(r.role)}`}>{r.role}</span>
                </td>
                <td style={{ textAlign: 'center' }} className="mono">
                  {r.usuarios}
                </td>
                <td>{r.descricao}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </CardConfig>

      <CardConfig titulo="Matriz de permissões (resumo)">
        <table className="config-tabela">
          <thead>
            <tr>
              <th>Ação</th>
              <th style={{ textAlign: 'center' }}>CEN</th>
              <th style={{ textAlign: 'center' }}>Gerente</th>
              <th style={{ textAlign: 'center' }}>Diretor</th>
              <th style={{ textAlign: 'center' }}>Admin TI</th>
            </tr>
          </thead>
          <tbody>
            {MATRIZ.map(([acao, ...cells]) => (
              <tr key={acao}>
                <td>{acao}</td>
                {cells.map((c, i) => (
                  <td
                    key={i}
                    style={{ textAlign: 'center', ...(c === '✓' ? { color: '#22C55E', fontWeight: 700 } : { color: '#94A3B8' }) }}
                  >
                    {c}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
        <div className="config-hint" style={{ marginTop: 12 }}>
          Permissões efetivas são calculadas na hora do login e cacheadas na sessão. Alterações aplicam no
          próximo login.
        </div>
      </CardConfig>
    </>
  );
}
