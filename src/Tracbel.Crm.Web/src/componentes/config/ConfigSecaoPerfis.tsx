/**
 * Configurações › Administração › Perfis (issue 113, parte 2b) — `Perfil.Administrar`, só do Administrador.
 *
 * Os perfis do sistema aparecem fixos, com "Duplicar"; os próprios se criam, editam, desativam e reativam. Um
 * perfil é um pacote de permissões: quem o recebe, na tela de Usuários, passa a ter o que ele dá.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { listarCatalogoDePermissoes, listarPerfisDaAdministracao, type PerfilNaAdministracao } from '../../dados/api/perfis';
import { useRecurso } from '../../dados/api/useRecurso';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { CardConfig } from './ConfigPartes';
import { EditorDePerfil, type ModoDoEditor } from './perfis/EditorDePerfil';

export function ConfigSecaoPerfis() {
  const { contexto } = useContextoDeAcesso();
  const perfis = useRecurso((sinal) => listarPerfisDaAdministracao(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const catalogo = useRecurso((sinal) => listarCatalogoDePermissoes(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const [modo, setModo] = useState<ModoDoEditor | null>(null);

  function abrir(perfil: PerfilNaAdministracao) {
    setModo(perfil.ehDoSistema ? { tipo: 'ver', perfil } : { tipo: 'editar', perfil });
  }

  const chaveDoEditor = modo ? `${modo.tipo}-${modo.tipo === 'novo' ? (modo.base?.codigo ?? 'zero') : modo.perfil.codigo}` : '';

  return (
    <>
      <CardConfig titulo="Perfis">
        <div className="adm-barra">
          <p className="config-hint perfil-explicacao">
            Um perfil é um pacote de permissões. Os do sistema ficam fixos; para ajustar, duplique e edite a cópia.
          </p>
          <button type="button" className="btn btn-primary" onClick={() => setModo({ tipo: 'novo', base: null })}>
            Novo perfil
          </button>
        </div>

        {perfis.carregando && <BlocoCarregando oQue="os perfis" />}
        {perfis.erro && <BlocoErro erro={perfis.erro} aoTentarDeNovo={perfis.recarregar} />}
        {perfis.dados && (
          <div className="conta-perfis">
            {perfis.dados.map((p) => (
              <button
                key={p.codigo}
                type="button"
                className={`conta-perfil perfil-cartao ${modo && modo.tipo !== 'novo' && modo.perfil.codigo === p.codigo ? 'perfil-cartao-ativo' : ''}`}
                onClick={() => abrir(p)}
              >
                <span className="conta-perfil-topo">
                  <span className="clr-title">{p.nome}</span>
                  <span>
                    {!p.estaAtivo && <span className="badge badge-warning">desativado</span>}{' '}
                    {p.ehDoSistema ? <span className="badge badge-neutral">sistema</span> : <span className="badge badge-green">próprio</span>}
                  </span>
                </span>
                <span className="clr-desc">
                  {p.permissoes.length} {p.permissoes.length === 1 ? 'permissão' : 'permissões'} ·{' '}
                  {p.ehPadrao ? 'todo usuário recebe' : `${p.pessoasComOPerfil} ${p.pessoasComOPerfil === 1 ? 'pessoa' : 'pessoas'}`}
                </span>
              </button>
            ))}
          </div>
        )}
      </CardConfig>

      {modo && catalogo.dados && (
        <EditorDePerfil
          key={chaveDoEditor}
          modo={modo}
          catalogo={catalogo.dados}
          aoGravar={(gravado) => {
            perfis.recarregar();
            setModo({ tipo: 'editar', perfil: gravado });
          }}
          aoDuplicar={(base) => setModo({ tipo: 'novo', base })}
          aoFechar={() => setModo(null)}
        />
      )}
      {modo && catalogo.erro && <BlocoErro erro={catalogo.erro} aoTentarDeNovo={catalogo.recarregar} />}
    </>
  );
}
