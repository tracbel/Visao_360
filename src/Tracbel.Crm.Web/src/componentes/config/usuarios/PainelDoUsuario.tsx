/**
 * A conta escolhida na lista de usuários: quem é, os perfis (com o histórico) e as ações (issue 113).
 *
 * AS AÇÕES SÓ APARECEM PARA QUEM ADMINISTRA (`Usuario.Administrar`), e o formulário de concessão só oferece os
 * perfis que quem concede pode dar — a API recusa o resto de qualquer jeito. Toda ação pede a justificativa
 * ou o motivo, que fica na concessão e na trilha de auditoria.
 */

import { useState } from 'react';
import {
  concederPerfil,
  desativarUsuario,
  liberarUsuario,
  listarPerfisParaConceder,
  obterUsuario,
  reativarUsuario,
  revogarConcessao,
  type ConcessaoNaAdministracao,
  type UsuarioDetalhado,
} from '../../../dados/api/administracao';
import type { FilialDoEscopo } from '../../../dados/api/acesso';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { useRecurso } from '../../../dados/api/useRecurso';
import { formatarData, formatarDataHora } from '../../../telas/cadastro/formato';
import { BlocoCarregando, BlocoErro } from '../../cadastro/EstadosDeTela';
import { CardConfig } from '../ConfigPartes';
import { useEnvio } from '../potencial/useEnvio';

type Props = {
  chave: string;
  podeAdministrar: boolean;
  filiais: FilialDoEscopo[];
  /** Chamado depois de cada ação, para a lista refletir a mudança. */
  aoMudar: () => void;
};

function Aviso({ aviso }: { aviso: { titulo: string; texto: string } | null }) {
  if (!aviso) return null;
  return (
    <div className="config-hint adm-aviso" role="alert">
      <strong>{aviso.titulo}</strong>
      {aviso.texto && <div>{aviso.texto}</div>}
    </div>
  );
}

function Concessao({ concessao, aoRevogar }: { concessao: ConcessaoNaAdministracao; aoRevogar?: (motivo: string) => Promise<boolean> }) {
  const [revogando, setRevogando] = useState(false);
  const [motivo, setMotivo] = useState('');

  const onde = concessao.filialCodigo ? `só em ${concessao.filialNome ?? concessao.filialCodigo}` : 'em todas as filiais';
  const validade = concessao.expiraEm ? `até ${formatarData(concessao.expiraEm)}` : 'sem data para expirar';

  return (
    <div className={`config-list-row ${concessao.vigente ? '' : 'adm-concessao-encerrada'}`}>
      <div>
        <div className="clr-title">
          {concessao.perfilNome}{' '}
          {concessao.vigente ? <span className="badge badge-green">vale</span> : <span className="badge badge-neutral">não vale</span>}
        </div>
        <div className="clr-desc">
          {onde}, {validade}. Concedido em {formatarDataHora(concessao.concedidaEm)} por {concessao.concedidaPor}: “{concessao.justificativa}”
        </div>
        {concessao.revogadaEm && (
          <div className="clr-desc">
            Revogado em {formatarDataHora(concessao.revogadaEm)} por {concessao.revogadaPor ?? '—'}: “{concessao.motivoDaRevogacao}”
          </div>
        )}
        {revogando && aoRevogar && (
          <div className="adm-form-inline">
            <input
              aria-label={`Motivo da revogação de ${concessao.perfilNome}`}
              placeholder="Motivo — por que, e por autorização de quem"
              value={motivo}
              onChange={(e) => setMotivo(e.target.value)}
            />
            <button
              type="button"
              className="btn btn-primary btn-sm"
              disabled={!motivo.trim()}
              onClick={async () => {
                if (await aoRevogar(motivo)) {
                  setRevogando(false);
                  setMotivo('');
                }
              }}
            >
              Revogar
            </button>
            <button type="button" className="btn btn-secondary btn-sm" onClick={() => setRevogando(false)}>
              Cancelar
            </button>
          </div>
        )}
      </div>
      {concessao.vigente && aoRevogar && !revogando && (
        <button type="button" className="btn btn-secondary btn-sm" onClick={() => setRevogando(true)}>
          Revogar
        </button>
      )}
    </div>
  );
}

export function PainelDoUsuario({ chave, podeAdministrar, filiais, aoMudar }: Props) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => obterUsuario(contexto, chave, sinal), [contexto.empresa, contexto.usuario, chave]);
  const perfis = useRecurso(
    (sinal) => (podeAdministrar ? listarPerfisParaConceder(contexto, sinal) : Promise.resolve({ dados: [], procedencia: null })),
    [contexto.empresa, contexto.usuario, podeAdministrar],
  );
  const envio = useEnvio(['filialCodigo', 'perfilCodigo', 'validaAte', 'justificativa', 'motivo']);

  const [atual, setAtual] = useState<UsuarioDetalhado | null>(null);
  const [filialDaLiberacao, setFilialDaLiberacao] = useState('');
  const [nova, setNova] = useState({ perfilCodigo: '', filialCodigo: '', validaAte: '', justificativa: '' });
  const [confirmandoDesativacao, setConfirmandoDesativacao] = useState(false);

  // O QUE A AÇÃO DEVOLVEU vale mais que a leitura: é a conta como ficou, sem esperar outra ida à API.
  const detalhe = atual?.usuario.chave === chave ? atual : leitura.dados;

  async function agir(acao: () => Promise<UsuarioDetalhado>): Promise<boolean> {
    const resultado = await envio.enviar(acao);
    if (!resultado) return false;
    setAtual(resultado);
    aoMudar();
    return true;
  }

  if (leitura.erro) return <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />;
  if (!detalhe) return <BlocoCarregando oQue="a conta" />;

  const { usuario, concessoes } = detalhe;
  const aguarda = usuario.aguardandoLiberacaoDesde !== null;
  const concediveis = (perfis.dados ?? []).filter((p) => p.podeConceder);

  return (
    <CardConfig titulo={usuario.nome}>
      <div className="config-list">
        <div className="config-list-row">
          <div className="clr-desc">E-mail</div>
          <div className="clr-title">{usuario.nomePrincipal}</div>
        </div>
        <div className="config-list-row">
          <div className="clr-desc">{aguarda ? 'Filial provisória' : 'Filial de casa'}</div>
          <div className="clr-title">{usuario.filialNome}</div>
        </div>
        <div className="config-list-row">
          <div className="clr-desc">Situação</div>
          <div className="clr-title">
            {aguarda
              ? `aguardando liberação desde ${formatarDataHora(usuario.aguardandoLiberacaoDesde)}`
              : usuario.estaAtivo
                ? 'ativa'
                : 'desativada'}
          </div>
        </div>
        <div className="config-list-row">
          <div className="clr-desc">Último acesso</div>
          <div className="clr-title">{usuario.ultimoLoginEm ? formatarDataHora(usuario.ultimoLoginEm) : 'nunca entrou'}</div>
        </div>
      </div>

      <Aviso aviso={envio.aviso} />

      {podeAdministrar && aguarda && (
        <div className="adm-bloco">
          <h4>Liberar a conta</h4>
          <p className="config-hint">Escolha a filial de casa da pessoa. Ela passa a entrar com o perfil padrão; perfis a mais se concedem depois.</p>
          <div className="adm-form-inline">
            <select aria-label="Filial de casa" value={filialDaLiberacao} onChange={(e) => setFilialDaLiberacao(e.target.value)}>
              <option value="">Escolha a filial…</option>
              {filiais.map((f) => (
                <option key={f.codigo} value={f.codigo}>
                  {f.nome}
                </option>
              ))}
            </select>
            <button
              type="button"
              className="btn btn-primary"
              disabled={!filialDaLiberacao || envio.enviando}
              onClick={() => agir(() => liberarUsuario(contexto, chave, filialDaLiberacao))}
            >
              Liberar
            </button>
          </div>
          {envio.erros.filialCodigo && <div className="form-erro">{envio.erros.filialCodigo}</div>}
        </div>
      )}

      <div className="adm-bloco">
        <h4>Perfis</h4>
        {concessoes.length === 0 ? (
          <p className="config-hint">Só o perfil padrão, que todo usuário recebe.</p>
        ) : (
          <div className="config-list">
            {concessoes.map((c) => (
              <Concessao
                key={c.id}
                concessao={c}
                aoRevogar={podeAdministrar ? (motivo) => agir(() => revogarConcessao(contexto, chave, c.id, motivo)) : undefined}
              />
            ))}
          </div>
        )}
      </div>

      {podeAdministrar && !aguarda && usuario.estaAtivo && (
        <div className="adm-bloco">
          <h4>Conceder perfil</h4>
          <div className="form-grid">
            <div className="form-field">
              <label htmlFor="adm-perfil">Perfil</label>
              <select id="adm-perfil" value={nova.perfilCodigo} onChange={(e) => setNova({ ...nova, perfilCodigo: e.target.value })}>
                <option value="">Escolha…</option>
                {concediveis.map((p) => (
                  <option key={p.codigo} value={p.codigo}>
                    {p.nome}
                  </option>
                ))}
              </select>
              {envio.erros.perfilCodigo && <div className="form-erro">{envio.erros.perfilCodigo}</div>}
            </div>
            <div className="form-field">
              <label htmlFor="adm-filial">Vale em</label>
              <select id="adm-filial" value={nova.filialCodigo} onChange={(e) => setNova({ ...nova, filialCodigo: e.target.value })}>
                <option value="">todas as filiais que a pessoa pode escolher</option>
                {filiais.map((f) => (
                  <option key={f.codigo} value={f.codigo}>
                    só em {f.nome}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-field">
              <label htmlFor="adm-validade">Válido até (opcional)</label>
              <input id="adm-validade" type="date" value={nova.validaAte} onChange={(e) => setNova({ ...nova, validaAte: e.target.value })} />
              {envio.erros.validaAte && <div className="form-erro">{envio.erros.validaAte}</div>}
            </div>
            <div className="form-field form-field-full">
              <label htmlFor="adm-justificativa">Justificativa</label>
              <textarea
                id="adm-justificativa"
                rows={2}
                placeholder="Por que, e por autorização de quem"
                value={nova.justificativa}
                onChange={(e) => setNova({ ...nova, justificativa: e.target.value })}
              />
              {envio.erros.justificativa && <div className="form-erro">{envio.erros.justificativa}</div>}
            </div>
          </div>
          <div className="adm-acoes">
            <button
              type="button"
              className="btn btn-primary"
              disabled={!nova.perfilCodigo || !nova.justificativa.trim() || envio.enviando}
              onClick={async () => {
                if (await agir(() => concederPerfil(contexto, chave, nova))) setNova({ perfilCodigo: '', filialCodigo: '', validaAte: '', justificativa: '' });
              }}
            >
              Conceder
            </button>
          </div>
        </div>
      )}

      {podeAdministrar && !aguarda && (
        <div className="adm-acoes adm-bloco">
          {usuario.estaAtivo ? (
            confirmandoDesativacao ? (
              <>
                <span className="config-hint">A pessoa deixa de entrar no CRM. Nada é apagado, e dá para reativar depois.</span>
                <button
                  type="button"
                  className="btn btn-primary"
                  disabled={envio.enviando}
                  onClick={async () => {
                    await agir(() => desativarUsuario(contexto, chave));
                    setConfirmandoDesativacao(false);
                  }}
                >
                  Confirmar desativação
                </button>
                <button type="button" className="btn btn-secondary" onClick={() => setConfirmandoDesativacao(false)}>
                  Cancelar
                </button>
              </>
            ) : (
              <button type="button" className="btn btn-secondary" onClick={() => setConfirmandoDesativacao(true)}>
                Desativar conta
              </button>
            )
          ) : (
            <button type="button" className="btn btn-primary" disabled={envio.enviando} onClick={() => agir(() => reativarUsuario(contexto, chave))}>
              Reativar conta
            </button>
          )}
        </div>
      )}
    </CardConfig>
  );
}
