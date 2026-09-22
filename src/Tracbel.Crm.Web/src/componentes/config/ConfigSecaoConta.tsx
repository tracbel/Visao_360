/**
 * Minha conta — o que o CRM sabe de verdade sobre quem está usando (issue 134).
 *
 * NO LUGAR DE "PERFIL E CONTA" DO PROTÓTIPO, que mostrava uma pessoa inventada
 * (nome, cargo, regional, telefone, duas etapas, sessões) e gravava no
 * navegador. Aqui só entra o que tem fonte:
 *
 * - nome e e-mail: a sessão do Entra ID (`/auth/eu`);
 * - filial de casa, filial atual, perfis e permissões: a rota de escopo
 *   (`/api/v1/acesso/escopo`), a mesma que a API usa para decidir o 403.
 *
 * É SÓ LEITURA. Quem muda perfil e filial é o administrador, pela tela de
 * usuários (#113); cargo e telefone voltam quando vierem do Entra (#140).
 */

import type { EscopoDoUsuario, PerfilDoEscopo } from '../../dados/api/acesso';
import type { Sessao } from '../../dados/api/sessao';
import { formatarData } from '../../telas/cadastro/formato';
import { CardConfig } from './ConfigPartes';

/** A profundidade como a pessoa entende — o mesmo texto que o servidor usa nas recusas. */
const ALCANCE: Record<string, string> = {
  Organizacao: 'toda a organização',
  EmpresaEAbaixo: 'a filial escolhida e as que estão abaixo dela',
  Empresa: 'só a filial escolhida',
  Equipe: 'você e a sua equipe',
  Proprios: 'só os seus registros',
};

function OndeVale({ perfil }: { perfil: PerfilDoEscopo }) {
  if (perfil.ehPadrao) return <>todo usuário recebe</>;
  const onde = perfil.filialCodigo ? `só em ${perfil.filialNome ?? perfil.filialCodigo}` : 'em todas as filiais que você pode escolher';
  const ate = perfil.expiraEm ? `, até ${formatarData(perfil.expiraEm)}` : ', sem data para expirar';
  return (
    <>
      {onde}
      {ate}
    </>
  );
}

function Linha({ titulo, valor }: { titulo: string; valor: string }) {
  return (
    <div className="config-list-row">
      <div className="clr-desc">{titulo}</div>
      <div className="clr-title">{valor}</div>
    </div>
  );
}

export function ConfigSecaoConta({ escopo, sessao }: { escopo: EscopoDoUsuario; sessao: Sessao }) {
  const autenticado = sessao.estado === 'autenticado' ? sessao : null;
  const casa = escopo.filiaisPermitidas.find((f) => f.ehCasa) ?? (escopo.filialAtual.ehCasa ? escopo.filialAtual : null);

  return (
    <>
      <CardConfig titulo="Quem você é">
        <div className="config-list">
          <Linha titulo="Nome" valor={autenticado?.nome || escopo.usuario} />
          <Linha titulo="E-mail" valor={autenticado?.email || 'acesso provisório, sem login da Microsoft'} />
          <Linha titulo="Filial de casa" valor={casa?.nome ?? '—'} />
          <Linha titulo="Olhando agora" valor={escopo.filialAtual.nome} />
        </div>
        <div className="config-hint" style={{ marginTop: 12 }}>
          Nome e e-mail vêm da sua conta Microsoft. A filial de casa e os perfis são definidos por quem administra o
          CRM.
        </div>
      </CardConfig>

      <CardConfig titulo="Seus perfis">
        <div className="config-list">
          {escopo.perfis.map((perfil) => (
            <div className="config-list-row" key={`${perfil.codigo}-${perfil.filialCodigo ?? 'todas'}`}>
              <div>
                <div className="clr-title">{perfil.nome}</div>
                <div className="clr-desc">
                  <OndeVale perfil={perfil} />
                </div>
              </div>
              {perfil.ehPadrao ? <span className="badge badge-neutral">padrão</span> : <span className="badge badge-green">concedido</span>}
            </div>
          ))}
        </div>
      </CardConfig>

      <CardConfig titulo="O que você pode fazer aqui">
        {escopo.permissoes.length === 0 ? (
          <div className="config-tabela-vazia">Nenhuma permissão nesta filial.</div>
        ) : (
          <table className="config-tabela">
            <thead>
              <tr>
                <th scope="col">Permissão</th>
                <th scope="col">Onde vale</th>
              </tr>
            </thead>
            <tbody>
              {escopo.permissoes.map((permissao) => (
                <tr key={permissao.codigo}>
                  <td>{permissao.descricao}</td>
                  <td>{ALCANCE[permissao.profundidade] ?? permissao.profundidade}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        <div className="config-hint" style={{ marginTop: 12 }}>
          É a soma do perfil padrão com os perfis concedidos a você, na filial que está no seletor. Precisa de mais?
          Peça a quem administra o CRM.
        </div>
      </CardConfig>
    </>
  );
}
