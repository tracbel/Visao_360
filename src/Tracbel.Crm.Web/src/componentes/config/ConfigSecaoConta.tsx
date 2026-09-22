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
 * O DESENHO É O DO PROTÓTIPO (22/09/2026): rótulo em cima, valor numa caixa, em
 * grade — e não linhas com o rótulo numa ponta e o valor na outra, que em tela
 * larga deixavam o olho atravessando a página. As permissões saem agrupadas por
 * área, cada área dizendo onde vale.
 *
 * É SÓ LEITURA. Quem muda perfil e filial é o administrador, pela tela de
 * usuários (#113); cargo e telefone voltam quando vierem do Entra (#140).
 */

import type { EscopoDoUsuario, PerfilDoEscopo } from '../../dados/api/acesso';
import type { Sessao } from '../../dados/api/sessao';
import { formatarData } from '../../telas/cadastro/formato';
import { ALCANCE, agruparPorArea } from './areasDePermissao';
import { CampoDeLeitura, CardConfig } from './ConfigPartes';

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

export function ConfigSecaoConta({ escopo, sessao }: { escopo: EscopoDoUsuario; sessao: Sessao }) {
  const autenticado = sessao.estado === 'autenticado' ? sessao : null;
  const casa = escopo.filiaisPermitidas.find((f) => f.ehCasa) ?? (escopo.filialAtual.ehCasa ? escopo.filialAtual : null);
  const areas = agruparPorArea(escopo.permissoes);

  // QUANDO TUDO VALE NO MESMO ALCANCE (o administrador: toda a organização), diz uma vez, em cima, em vez de
  // repetir a mesma frase em cada cartão.
  const alcancesDaPessoa = new Set(escopo.permissoes.map((p) => p.profundidade));
  const alcanceDeTudo = alcancesDaPessoa.size === 1 ? escopo.permissoes[0].profundidade : null;

  return (
    <>
      <CardConfig titulo="Quem você é">
        <div className="conta-campos">
          <CampoDeLeitura id="conta-nome" rotulo="Nome" valor={autenticado?.nome || escopo.usuario} dica="Vem da sua conta Microsoft" />
          <CampoDeLeitura
            id="conta-email"
            rotulo="E-mail"
            valor={autenticado?.email || 'acesso provisório, sem login da Microsoft'}
            dica="O login da Microsoft"
          />
          <CampoDeLeitura id="conta-casa" rotulo="Filial de casa" valor={casa?.nome ?? '—'} dica="Definida por quem administra o CRM" />
          <CampoDeLeitura id="conta-agora" rotulo="Olhando agora" valor={escopo.filialAtual.nome} dica="Troque no seletor de filial, no topo" />
        </div>
      </CardConfig>

      <CardConfig titulo="Seus perfis">
        <div className="conta-perfis">
          {escopo.perfis.map((perfil) => (
            <div className="conta-perfil" key={`${perfil.codigo}-${perfil.filialCodigo ?? 'todas'}`}>
              <div className="conta-perfil-topo">
                <span className="clr-title">{perfil.nome}</span>
                {perfil.ehPadrao ? <span className="badge badge-neutral">padrão</span> : <span className="badge badge-green">concedido</span>}
              </div>
              <div className="clr-desc">
                <OndeVale perfil={perfil} />
              </div>
            </div>
          ))}
        </div>
      </CardConfig>

      <CardConfig titulo="O que você pode fazer aqui">
        {areas.length === 0 ? (
          <div className="config-tabela-vazia">Nenhuma permissão nesta filial.</div>
        ) : (
          <>
            {alcanceDeTudo && (
              <p className="conta-alcance-geral">
                Tudo abaixo vale em <strong>{ALCANCE[alcanceDeTudo] ?? alcanceDeTudo}</strong>.
              </p>
            )}
            <div className="conta-areas">
              {areas.map((area) => {
                const alcances = new Set(area.itens.map((p) => p.profundidade));
                const alcanceUnico = alcances.size === 1 ? area.itens[0].profundidade : null;
                return (
                  <div className="conta-area" key={area.nome}>
                    <div className="conta-area-topo">
                      <span className="clr-title">{area.nome}</span>
                      {alcanceUnico && !alcanceDeTudo && <span className="conta-alcance">{ALCANCE[alcanceUnico] ?? alcanceUnico}</span>}
                    </div>
                    <ul className="conta-permissoes">
                      {area.itens.map((p) => (
                        <li key={p.codigo}>
                          {p.descricao}
                          {!alcanceUnico && <span className="conta-alcance"> · {ALCANCE[p.profundidade] ?? p.profundidade}</span>}
                        </li>
                      ))}
                    </ul>
                  </div>
                );
              })}
            </div>
          </>
        )}
        <div className="config-hint" style={{ marginTop: 12 }}>
          É a soma do perfil padrão com os perfis concedidos a você, na filial que está no seletor. Precisa de mais?
          Peça a quem administra o CRM.
        </div>
      </CardConfig>
    </>
  );
}
