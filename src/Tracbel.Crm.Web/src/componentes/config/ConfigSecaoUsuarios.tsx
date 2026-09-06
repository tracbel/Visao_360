/**
 * Aba TI e Integrações › Usuários — porte de `renderSecaoUsuarios`
 * (prototipo/referencia/assets/app.js linha 5721). Dados vêm de
 * `config-usuarios.json` via `useDados` (ver telas/Configuracoes.tsx).
 * Sem `footerAcoes` — o original também não tem nesta seção.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a busca e os dois filtros passaram a funcionar; os dois botões
 * sem destino saíram.
 *
 * Filtrar por nome, role e regional é barato — a lista já está toda na memória
 * — e é o que se faz numa tela de usuários. Já **"+ Convidar usuário"** e
 * **"Editar"** saíram: os usuários vêm do Entra ID pelo provisionamento SCIM,
 * como o próprio rodapé da seção explica, e criar ou alterar usuário aqui
 * significaria escrever no diretório — não é decisão desta rodada nem desta
 * tela (documento 05 §5: o que vem de fora se corrige na origem).
 */
import { useMemo, useState } from 'react';
import type { ConfigUsuario } from '../../tipos/configuracoes';
import { CardConfig } from './ConfigPartes';

/** Mesma regra do original: `role.toLowerCase().replace(/[^a-z]/g, '')`. */
function roleClasse(role: string): string {
  return role.toLowerCase().replace(/[^a-z]/g, '');
}

export function ConfigSecaoUsuarios({ usuarios }: { usuarios: ConfigUsuario[] }) {
  const [busca, setBusca] = useState('');
  const [role, setRole] = useState('');
  const [regional, setRegional] = useState('');

  // As opções saem da própria lista: oferecer um role que ninguém tem devolve
  // tabela vazia e parece defeito (padrão de tela §2.2).
  const roles = useMemo(
    () => [...new Set(usuarios.map((u) => u.role))].sort((a, b) => a.localeCompare(b, 'pt-BR')),
    [usuarios],
  );
  const regionais = useMemo(
    () => [...new Set(usuarios.map((u) => u.regional))].sort((a, b) => a.localeCompare(b, 'pt-BR')),
    [usuarios],
  );

  const filtrados = useMemo(() => {
    const termo = busca.trim().toLowerCase();
    return usuarios.filter((u) => {
      if (role && u.role !== role) return false;
      if (regional && u.regional !== regional) return false;
      if (termo && !`${u.nome} ${u.email}`.toLowerCase().includes(termo)) return false;
      return true;
    });
  }, [usuarios, busca, role, regional]);

  return (
    <CardConfig titulo="Usuários ativos">
      <div className="config-filtros-inline">
        <input
          type="search"
          aria-label="Buscar usuário por nome ou e-mail"
          placeholder="Buscar por nome, e-mail..."
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
        />
        <select aria-label="Filtrar por role" value={role} onChange={(e) => setRole(e.target.value)}>
          <option value="">Todos os roles</option>
          {roles.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </select>
        <select aria-label="Filtrar por regional" value={regional} onChange={(e) => setRegional(e.target.value)}>
          <option value="">Todas as regionais</option>
          {regionais.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </select>
      </div>

      <table className="config-tabela">
        <thead>
          <tr>
            <th>Nome</th>
            <th>E-mail</th>
            <th>Role</th>
            <th>Regional</th>
            <th>Último login</th>
          </tr>
        </thead>
        <tbody>
          {filtrados.length === 0 ? (
            <tr>
              <td colSpan={5} className="config-tabela-vazia">
                Nenhum usuário com esses filtros. A busca compara nome e e-mail por trecho.
              </td>
            </tr>
          ) : (
            filtrados.map((u) => (
              <tr key={u.id}>
                <td>{u.nome}</td>
                <td className="mono">{u.email}</td>
                <td>
                  <span className={`role-pill role-${roleClasse(u.role)}`}>{u.role}</span>
                </td>
                <td>{u.regional}</td>
                <td className="mono muted">{u.ultimo_login}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
      <div className="config-hint" style={{ marginTop: 12 }}>
        Mostrando {filtrados.length} de {usuarios.length} usuários. Provisionamento automático via SCIM diário
        03:00 (Microsoft Entra ID → CRM) — criar e alterar usuário se faz no Entra ID, não aqui.
      </div>
    </CardConfig>
  );
}
