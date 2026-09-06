/**
 * Configurações — porte de `renderConfig`/`mountConfig`
 * (prototipo/referencia/assets/app.js linhas 5244-5918, idêntico a
 * prototipo/referencia/config-block.js). Layout com abas (preferências /
 * comercial / TI) e menu lateral de seções, replicados com `useState` —
 * mesma navegação do original, cuja aba/seção inicial já é "preferências ›
 * perfil" (o mesmo valor fixo de `CONFIG_STATE`/`estado-config.json`).
 *
 * Dados de cada seção vêm de `public/dados/config-*.json` via `useDados`;
 * `eventos` (Auditoria), `atalhos` (Atalhos) e a matriz de permissões
 * (Permissões) continuam locais aos seus componentes porque já eram listas
 * locais às respectivas `render*` no original, não `CONFIG_*` extraídos.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — "Salvar alterações" passou a salvar.
 *
 * ERA A PONTA SOLTA MAIS GRAVE DE TODAS, e a única de gravidade "engana e perde
 * dado" fora do painel de registrar contato da Cobertura: seis seções tinham o
 * rodapé Cancelar / Salvar alterações, `salvar()` só mostrava o aviso
 * *"Alterações salvas · em produção grava no Protheus/AD"*, e **todos** os
 * campos eram `defaultValue`/`defaultChecked` — ninguém lia o valor. Alterar a
 * meta de visita da classe A, salvar, trocar de seção e voltar devolvia o
 * número antigo. A pessoa via o aviso de sucesso e perdia o trabalho.
 *
 * O QUE MUDOU, na ordem em que se percebe:
 *
 * 1. **Os campos ficaram controlados** e vivem num rascunho aqui, para o valor
 *    sobreviver à troca de seção antes mesmo de ser salvo.
 * 2. **`Salvar alterações` grava** em `dados/persistenciaConfig.ts`, e o aviso
 *    diz onde ficou gravado: o navegador desta máquina. Não o Protheus, não o
 *    AD — dizer o que não acontece é como se perde a confiança no aviso.
 * 3. **`Cancelar` volta ao último gravado**, em vez de só piscar um aviso.
 * 4. **Os dois botões só habilitam quando há alteração pendente**, e uma linha
 *    avisa que ela existe. Antes os dois respondiam sempre, mesmo sem nada
 *    alterado.
 * 5. **Trocar de seção com alteração pendente pede confirmação.** As seções são
 *    telas diferentes com um rodapé só; sair de uma sem salvar apagava tudo em
 *    silêncio.
 * 6. **A tela ganhou estado de carregando e de erro.** Antes, cada seção era
 *    `metas && <Secao …>`: enquanto o arquivo não chegava, a área de conteúdo
 *    ficava simplesmente em branco, e se a leitura falhasse ficava em branco
 *    para sempre.
 */
import { useEffect, useMemo, useState } from 'react';
import { ConfigToast, type ConfigToastEstado } from '../componentes/config/ConfigPartes';
import { ConfigSecaoAprovacoes } from '../componentes/config/ConfigSecaoAprovacoes';
import { ConfigSecaoAtalhos } from '../componentes/config/ConfigSecaoAtalhos';
import { ConfigSecaoAuditoria } from '../componentes/config/ConfigSecaoAuditoria';
import { ConfigSecaoIntegracoes } from '../componentes/config/ConfigSecaoIntegracoes';
import { ConfigSecaoMetas } from '../componentes/config/ConfigSecaoMetas';
import { ConfigSecaoNotificacoes } from '../componentes/config/ConfigSecaoNotificacoes';
import { ConfigSecaoPerfil } from '../componentes/config/ConfigSecaoPerfil';
import { ConfigSecaoPermissoes } from '../componentes/config/ConfigSecaoPermissoes';
import { ConfigSecaoTaxonomias } from '../componentes/config/ConfigSecaoTaxonomias';
import { ConfigSecaoUsuarios } from '../componentes/config/ConfigSecaoUsuarios';
import { MenuLateralConfig } from '../componentes/config/MenuLateralConfig';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { DialogoConfirmacao } from '../componentes/cadastro/DialogoConfirmacao';
import {
  gravarPreferencias,
  lerPreferencias,
  storageConfigDisponivel,
  type PreferenciasConfig,
} from '../dados/persistenciaConfig';
import { useDados } from '../dados/useDados';
import type {
  AbaConfig,
  ConfigCategoriaInteracao,
  ConfigIntegracao,
  ConfigMetas,
  ConfigMotivoPerda,
  ConfigRole,
  ConfigUsuario,
  SecaoConfig,
} from '../tipos/configuracoes';

const SECAO_DEFAULT_POR_ABA: Record<AbaConfig, SecaoConfig> = {
  preferencias: 'perfil',
  comercial: 'metas',
  ti: 'integracoes',
};

export function Configuracoes() {
  const [aba, setAba] = useState<AbaConfig>('preferencias');
  const [secao, setSecao] = useState<SecaoConfig>('perfil');
  const [toast, setToast] = useState<ConfigToastEstado>(null);

  const leituraMetas = useDados<ConfigMetas>('config-metas');
  const leituraMotivos = useDados<ConfigMotivoPerda[]>('config-motivos-perda');
  const leituraCategorias = useDados<ConfigCategoriaInteracao[]>('config-categorias-interacao');
  const leituraIntegracoes = useDados<ConfigIntegracao[]>('config-integracoes');
  const leituraUsuarios = useDados<ConfigUsuario[]>('config-usuarios');
  const leituraRoles = useDados<ConfigRole[]>('config-roles');
  const { dados: metas } = leituraMetas;
  const { dados: motivosPerda } = leituraMotivos;
  const { dados: categoriasInteracao } = leituraCategorias;
  const { dados: integracoes } = leituraIntegracoes;
  const { dados: usuarios } = leituraUsuarios;
  const { dados: roles } = leituraRoles;

  /**
   * O que está gravado, e o que está sendo editado.
   *
   * São dois estados e não um porque "há alteração pendente" é exatamente a
   * diferença entre eles — e é isso que habilita o rodapé, que avisa ao trocar
   * de seção, e que o Cancelar desfaz.
   */
  const [gravado, setGravado] = useState<PreferenciasConfig | null>(null);
  const [rascunho, setRascunho] = useState<PreferenciasConfig | null>(null);
  const [secaoPendente, setSecaoPendente] = useState<{ aba: AbaConfig; secao: SecaoConfig } | null>(null);

  // O rascunho só nasce quando `config-metas.json` chega: os valores de fábrica
  // das metas são os do arquivo, não números repetidos no código.
  useEffect(() => {
    if (!metas || gravado) return;
    const lido = lerPreferencias(metas);
    setGravado(lido);
    setRascunho(lido);
  }, [metas, gravado]);

  useEffect(() => {
    if (!toast) return;
    const id = setTimeout(() => setToast(null), 4000);
    return () => clearTimeout(id);
  }, [toast]);

  const temAlteracao = useMemo(
    () => Boolean(gravado && rascunho) && JSON.stringify(gravado) !== JSON.stringify(rascunho),
    [gravado, rascunho],
  );

  /** Fechar a aba com alteração pendente pede confirmação do navegador. */
  useEffect(() => {
    if (!temAlteracao) return;
    function aoSair(e: BeforeUnloadEvent) {
      e.preventDefault();
      e.returnValue = '';
    }
    window.addEventListener('beforeunload', aoSair);
    return () => window.removeEventListener('beforeunload', aoSair);
  }, [temAlteracao]);

  function mudar(mudanca: Partial<PreferenciasConfig>) {
    setRascunho((r) => (r ? { ...r, ...mudanca } : r));
  }

  /** Trocar de seção com alteração pendente pergunta antes de descartar. */
  function irPara(novaAba: AbaConfig, novaSecao: SecaoConfig) {
    if (temAlteracao) {
      setSecaoPendente({ aba: novaAba, secao: novaSecao });
      return;
    }
    setAba(novaAba);
    setSecao(novaSecao);
  }

  function selecionarAba(novaAba: AbaConfig) {
    irPara(novaAba, SECAO_DEFAULT_POR_ABA[novaAba]);
  }

  function salvar() {
    if (!rascunho) return;
    const deu = gravarPreferencias(rascunho);
    if (!deu) {
      setToast({
        msg: 'Não foi possível salvar: este navegador está bloqueando o armazenamento local desta página.',
        tipo: 'warn',
      });
      return;
    }
    setGravado(rascunho);
    setToast({
      msg: 'Alterações salvas neste navegador. Quando a API existir, elas passam a valer para todos os aparelhos.',
      tipo: 'ok',
    });
  }

  function cancelar() {
    if (!gravado) return;
    setRascunho(gravado);
    setToast({ msg: 'Alterações descartadas · os valores voltaram ao último salvo.', tipo: 'warn' });
  }

  const erroLeitura =
    leituraMetas.erro ??
    leituraMotivos.erro ??
    leituraCategorias.erro ??
    leituraIntegracoes.erro ??
    leituraUsuarios.erro ??
    leituraRoles.erro;

  function renderSecao() {
    if (erroLeitura) {
      return (
        <BlocoErro
          erro={erroLeitura}
          aoTentarDeNovo={() => {
            leituraMetas.recarregar();
            leituraMotivos.recarregar();
            leituraCategorias.recarregar();
            leituraIntegracoes.recarregar();
            leituraUsuarios.recarregar();
            leituraRoles.recarregar();
          }}
        />
      );
    }

    switch (secao) {
      case 'perfil':
        return rascunho ? (
          <ConfigSecaoPerfil
            rascunho={rascunho}
            aoMudar={mudar}
            temAlteracao={temAlteracao}
            onSalvar={salvar}
            onCancelar={cancelar}
          />
        ) : (
          <BlocoCarregando oQue="as suas preferências" />
        );
      case 'notificacoes':
        return rascunho ? (
          <ConfigSecaoNotificacoes
            rascunho={rascunho}
            aoMudar={mudar}
            temAlteracao={temAlteracao}
            onSalvar={salvar}
            onCancelar={cancelar}
          />
        ) : (
          <BlocoCarregando oQue="as suas preferências" />
        );
      case 'atalhos':
        return rascunho ? (
          <ConfigSecaoAtalhos
            rascunho={rascunho}
            aoMudar={mudar}
            temAlteracao={temAlteracao}
            onSalvar={salvar}
            onCancelar={cancelar}
          />
        ) : (
          <BlocoCarregando oQue="as suas preferências" />
        );
      case 'metas':
        return rascunho ? (
          <ConfigSecaoMetas
            rascunho={rascunho}
            aoMudar={mudar}
            temAlteracao={temAlteracao}
            onSalvar={salvar}
            onCancelar={cancelar}
          />
        ) : (
          <BlocoCarregando oQue="as metas comerciais" />
        );
      case 'aprovacoes':
        return rascunho ? (
          <ConfigSecaoAprovacoes
            rascunho={rascunho}
            aoMudar={mudar}
            temAlteracao={temAlteracao}
            onSalvar={salvar}
            onCancelar={cancelar}
          />
        ) : (
          <BlocoCarregando oQue="as políticas de aprovação" />
        );
      case 'taxonomias':
        return motivosPerda && categoriasInteracao ? (
          <ConfigSecaoTaxonomias motivos={motivosPerda} categorias={categoriasInteracao} />
        ) : (
          <BlocoCarregando oQue="as taxonomias" />
        );
      case 'integracoes':
        return integracoes ? (
          <ConfigSecaoIntegracoes integracoes={integracoes} />
        ) : (
          <BlocoCarregando oQue="as integrações" />
        );
      case 'usuarios':
        return usuarios ? <ConfigSecaoUsuarios usuarios={usuarios} /> : <BlocoCarregando oQue="os usuários" />;
      case 'permissoes':
        return roles ? <ConfigSecaoPermissoes roles={roles} /> : <BlocoCarregando oQue="os roles" />;
      case 'auditoria':
        return <ConfigSecaoAuditoria />;
      default:
        return <div>Seção em construção</div>;
    }
  }

  return (
    <>
      <div className="config-header">
        <div>
          <div className="config-title">Configurações</div>
          <div className="config-subtitle">Administração do CRM · integrações · usuários e políticas comerciais</div>
        </div>
        <div className="config-user-chip">
          <div className="cu-avatar" style={{ background: '#367C2B' }}>
            HR
          </div>
          <div>
            <div className="cu-nome">Hugo Rocha</div>
            <div className="cu-role">Admin TI · Tracbel Agro</div>
          </div>
        </div>
      </div>

      <div className="config-abas">
        <button
          type="button"
          className={`config-aba ${aba === 'preferencias' ? 'active' : ''}`}
          onClick={() => selecionarAba('preferencias')}
        >
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth={2}>
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
            <circle cx="12" cy="7" r="4" />
          </svg>
          Minhas preferências
        </button>
        <button
          type="button"
          className={`config-aba ${aba === 'comercial' ? 'active' : ''}`}
          onClick={() => selecionarAba('comercial')}
        >
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth={2}>
            <line x1="12" y1="1" x2="12" y2="23" />
            <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
          </svg>
          Comercial
          <span className="aba-lock" title="Gerente Comercial ou superior">
            🔒
          </span>
        </button>
        <button
          type="button"
          className={`config-aba ${aba === 'ti' ? 'active' : ''}`}
          onClick={() => selecionarAba('ti')}
        >
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth={2}>
            <rect x="2" y="3" width="20" height="14" rx="2" />
            <line x1="8" y1="21" x2="16" y2="21" />
            <line x1="12" y1="17" x2="12" y2="21" />
          </svg>
          TI e Integrações
          <span className="aba-lock" title="Admin TI">
            🔒
          </span>
        </button>
      </div>

      {!storageConfigDisponivel() && (
        <div className="config-aviso-sem-storage" role="status">
          Este navegador está bloqueando o armazenamento local desta página. As preferências podem ser alteradas na
          tela, mas <strong>não vão sobreviver a um F5</strong> — e o botão de salvar vai avisar quando falhar.
        </div>
      )}

      <div className="config-body">
        <aside className="config-sidebar">
          <MenuLateralConfig aba={aba} secaoAtiva={secao} onSelecionar={(s) => irPara(aba, s)} />
        </aside>
        <section className="config-content">{renderSecao()}</section>
      </div>

      <ConfigToast toast={toast} />

      {/* Trocar de seção com alteração pendente pergunta antes de descartar:
          as seções são telas diferentes com um rodapé só, e sair de uma sem
          salvar apagava tudo em silêncio (padrão de tela §3.4). */}
      {secaoPendente && (
        <DialogoConfirmacao
          titulo="Sair desta seção sem salvar?"
          subtitulo="Há alterações que ainda não foram salvas."
          rotuloConfirmar="Descartar e trocar de seção"
          aoCancelar={() => setSecaoPendente(null)}
          aoConfirmar={() => {
            if (gravado) setRascunho(gravado);
            setAba(secaoPendente.aba);
            setSecao(secaoPendente.secao);
            setSecaoPendente(null);
          }}
        >
          <p>
            O que você alterou nesta seção <strong>voltará ao último valor salvo</strong>. Nada do que já está salvo
            é afetado.
          </p>
          <p>
            Para guardar antes de trocar, feche este aviso e use <strong>Salvar alterações</strong> no rodapé da
            seção.
          </p>
        </DialogoConfirmacao>
      )}
    </>
  );
}
