/**
 * Configurações › Administração › Usuários (issue 113) — no lugar da lista de exemplo do protótipo.
 *
 * QUEM VÊ: `Usuario.Ler` (a gerência vê os da própria filial; a diretoria e o administrador, todos).
 * QUEM AGE: `Usuario.Administrar` — liberar a conta que nasceu no primeiro login, conceder e revogar perfil,
 * desativar e reativar. A fila "Aguardando liberação" só aparece para quem age: a filial de quem espera é
 * provisória, e mostrá-la a uma gerência sugeriria que a pessoa é daquela filial.
 */

import { useEffect, useState } from 'react';
import { listarUsuarios, type SituacaoDaConta } from '../../dados/api/administracao';
import type { FilialDoEscopo } from '../../dados/api/acesso';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarDataHora } from '../../telas/cadastro/formato';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { CardConfig } from './ConfigPartes';
import { PainelDoUsuario } from './usuarios/PainelDoUsuario';

const ESPERA_DA_BUSCA_MS = 350;

const ROTULO: Record<SituacaoDaConta, string> = {
  AguardandoLiberacao: 'Aguardando liberação',
  Ativa: 'Ativas',
  Desativada: 'Desativadas',
};

export function ConfigSecaoUsuarios({ podeAdministrar, filiais }: { podeAdministrar: boolean; filiais: FilialDoEscopo[] }) {
  const { contexto } = useContextoDeAcesso();
  const situacoes: SituacaoDaConta[] = podeAdministrar ? ['AguardandoLiberacao', 'Ativa', 'Desativada'] : ['Ativa', 'Desativada'];

  const [situacao, setSituacao] = useState<SituacaoDaConta>(podeAdministrar ? 'AguardandoLiberacao' : 'Ativa');
  const [termoDigitado, setTermoDigitado] = useState('');
  const [termo, setTermo] = useState('');
  const [pagina, setPagina] = useState(1);
  const [tamanho, setTamanho] = useState(25);
  const [selecionada, setSelecionada] = useState<string | null>(null);

  useEffect(() => {
    const relogio = setTimeout(() => {
      setTermo(termoDigitado.trim());
      setPagina(1);
    }, ESPERA_DA_BUSCA_MS);
    return () => clearTimeout(relogio);
  }, [termoDigitado]);

  const leitura = useRecurso(
    (sinal) => listarUsuarios(contexto, { situacao, termo, pagina, tamanho }, sinal),
    [contexto.empresa, contexto.usuario, situacao, termo, pagina, tamanho],
  );
  const dados = leitura.dados;

  return (
    <>
      <CardConfig titulo="Usuários">
        <div className="adm-barra">
          <div className="adm-situacoes" role="tablist" aria-label="Situação da conta">
            {situacoes.map((s) => (
              <button
                key={s}
                type="button"
                role="tab"
                aria-selected={s === situacao}
                className={`btn btn-sm ${s === situacao ? 'btn-primary' : 'btn-secondary'}`}
                onClick={() => {
                  setSituacao(s);
                  setPagina(1);
                  setSelecionada(null);
                }}
              >
                {ROTULO[s]}
              </button>
            ))}
          </div>
          <input
            type="search"
            className="adm-busca"
            aria-label="Buscar usuário por nome ou e-mail"
            placeholder="Buscar por nome ou e-mail…"
            value={termoDigitado}
            onChange={(e) => setTermoDigitado(e.target.value)}
          />
        </div>

        {leitura.carregando && <BlocoCarregando oQue="os usuários" />}
        {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
        {dados && dados.itens.length === 0 && (
          <div className="config-tabela-vazia">
            {situacao === 'AguardandoLiberacao'
              ? 'Ninguém esperando. Quem está no grupo do Entra e entra pela primeira vez aparece aqui.'
              : 'Nenhuma conta com este filtro.'}
          </div>
        )}
        {dados && dados.itens.length > 0 && (
          <div className="cad-tabela-wrap">
            <table className="config-tabela">
              <thead>
                <tr>
                  <th scope="col">Pessoa</th>
                  <th scope="col">{situacao === 'AguardandoLiberacao' ? 'Esperando desde' : 'Filial de casa'}</th>
                  <th scope="col">Perfis</th>
                  <th scope="col">Último acesso</th>
                </tr>
              </thead>
              <tbody>
                {dados.itens.map((u) => (
                  <tr
                    key={u.chave}
                    className={`adm-linha ${u.chave === selecionada ? 'adm-linha-ativa' : ''}`}
                    onClick={() => setSelecionada(u.chave)}
                  >
                    <td>
                      <button type="button" className="adm-link" onClick={() => setSelecionada(u.chave)}>
                        {u.nome}
                      </button>
                      <div className="muted">{u.nomePrincipal}</div>
                    </td>
                    <td>{situacao === 'AguardandoLiberacao' ? formatarDataHora(u.aguardandoLiberacaoDesde) : u.filialNome}</td>
                    <td>
                      {u.perfis.length === 0 ? (
                        <span className="muted">padrão</span>
                      ) : (
                        u.perfis.map((p) => (
                          <span key={p} className="badge badge-green adm-perfil">
                            {p}
                          </span>
                        ))
                      )}
                    </td>
                    <td className="mono">{u.ultimoLoginEm ? formatarDataHora(u.ultimoLoginEm) : 'nunca'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        {dados && dados.total > 0 && (
          <BarraDePaginacao
            pagina={dados}
            oQue="contas"
            aoTrocarPagina={setPagina}
            aoTrocarTamanho={(novo) => {
              setTamanho(novo);
              setPagina(1);
            }}
          />
        )}
      </CardConfig>

      {selecionada && (
        <PainelDoUsuario chave={selecionada} podeAdministrar={podeAdministrar} filiais={filiais} aoMudar={leitura.recarregar} />
      )}
    </>
  );
}
