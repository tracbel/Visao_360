/**
 * Configurações — porte de `renderConfig`/`mountConfig`
 * (prototipo/referencia/assets/app.js linhas 5244-5918): abas e menu lateral
 * de seções.
 *
 * ---------------------------------------------------------------------------
 * 22/09/2026 — A PÁGINA PASSOU A MOSTRAR SÓ O QUE É DE VERDADE, E SÓ PARA QUEM
 * PODE (issue 134).
 *
 * Até aqui, todo mundo via as três abas com cadeados que não conferiam nada, o
 * cabeçalho dizia "Hugo Rocha · Admin TI" para qualquer pessoa, e seis seções
 * liam arquivos de exemplo do protótipo e gravavam no navegador. Agora:
 *
 * 1. **As seções dependem da permissão**, lida da rota de escopo — a mesma que a
 *    API usa para responder 403 (`componentes/config/secoes.ts`). Aba sem seção
 *    visível não aparece.
 * 2. **O cabeçalho mostra quem está usando e os perfis dessa pessoa.**
 * 3. **O que não tem fonte saiu**: Notificações e Atalhos (#137), Metas e SLA
 *    (#138), Políticas de aprovação (#139), Taxonomias (#45), Usuários e
 *    Permissões de exemplo (#113), Auditoria de exemplo (#135), "Sistemas
 *    conectados" (#136) e os dados de perfil inventados (#140).
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO PADRÃO DOS INDICADORES GEOGRÁFICOS (#293, bloco 5): a faixa verde do protótipo virou o `page-header`
 * das outras telas, com quem está usando à direita; as abas são as mesmas dos Indicadores (`AbasDaTela`), com o teclado
 * andando pelas setas; e a página ocupa a coluna inteira. As seções e o que cada uma mostra não mudaram.
 */
import { useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { ConfigSecaoAuditoria } from '../componentes/config/ConfigSecaoAuditoria';
import { ConfigSecaoConta } from '../componentes/config/ConfigSecaoConta';
import { ConfigSecaoFontes } from '../componentes/config/ConfigSecaoFontes';
import { ConfigSecaoIntegracoes } from '../componentes/config/ConfigSecaoIntegracoes';
import { ConfigSecaoPerfis } from '../componentes/config/ConfigSecaoPerfis';
import { ConfigSecaoPotencial } from '../componentes/config/ConfigSecaoPotencial';
import { ConfigSecaoUsuarios } from '../componentes/config/ConfigSecaoUsuarios';
import { MenuLateralConfig } from '../componentes/config/MenuLateralConfig';
import { abasVisiveis, secoesVisiveis, type AbaConfig, type SecaoConfig } from '../componentes/config/secoes';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { AbasDaTela } from '../componentes/territorio/AbasDaTela';
import { obterEscopo, type EscopoDoUsuario } from '../dados/api/acesso';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { PERMISSAO } from '../dados/api/permissoes';
import { useSessao } from '../dados/api/sessao';
import { useRecurso } from '../dados/api/useRecurso';
import '../estilos/dashboard.css';
import '../estilos/territorio.css';
import '../estilos/configuracoes.css';

function iniciais(nome: string): string {
  const partes = nome.replace(/@.*/, '').split(/[\s.]+/).filter(Boolean);
  return ((partes[0]?.[0] ?? '?') + (partes.length > 1 ? partes[partes.length - 1][0] : '')).toUpperCase();
}

/** Os perfis concedidos, pelo nome; quem só tem o padrão aparece como "Padrão". */
function rotuloDosPerfis(escopo: EscopoDoUsuario): string {
  const concedidos = [...new Set(escopo.perfis.filter((p) => !p.ehPadrao).map((p) => p.nome))];
  return concedidos.length > 0 ? concedidos.join(' · ') : (escopo.perfis[0]?.nome ?? 'Padrão');
}

export function Configuracoes() {
  const { contexto } = useContextoDeAcesso();
  const { sessao } = useSessao();
  const leitura = useRecurso((sinal) => obterEscopo(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const [escolha, setEscolha] = useState<{ aba: AbaConfig; secao: SecaoConfig }>({ aba: 'conta', secao: 'conta' });

  const escopo = leitura.dados;
  const codigos = new Set(escopo?.permissoes.map((p) => p.codigo) ?? []);
  const tem = (codigo: string) => codigos.has(codigo);

  const abas = abasVisiveis(tem);
  const secoes = secoesVisiveis(tem);

  // A ESCOLHA PODE DEIXAR DE VALER quando a filial muda e, com ela, as permissões: aí a página volta para
  // a primeira seção que a pessoa vê, em vez de mostrar uma seção que a API vai recusar.
  const secaoValida = secoes.find((s) => s.id === escolha.secao) ?? secoes[0];
  const abaAtual = secaoValida?.aba ?? 'conta';
  const secoesDaAba = secoes.filter((s) => s.aba === abaAtual);

  function selecionarAba(aba: AbaConfig) {
    const primeira = secoes.find((s) => s.aba === aba);
    if (primeira) setEscolha({ aba, secao: primeira.id });
  }

  function renderSecao() {
    if (leitura.erro) return <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />;
    if (!escopo) return <BlocoCarregando oQue="a sua conta" />;

    switch (secaoValida?.id) {
      case 'potencial':
        return <ConfigSecaoPotencial />;
      case 'fontes':
        return <ConfigSecaoFontes />;
      case 'integracoes':
        return <ConfigSecaoIntegracoes />;
      case 'usuarios':
        return <ConfigSecaoUsuarios podeAdministrar={tem(PERMISSAO.usuarioAdministrar)} filiais={escopo.filiaisPermitidas} />;
      case 'perfis':
        return <ConfigSecaoPerfis />;
      case 'auditoria':
        return <ConfigSecaoAuditoria filialAtual={escopo.filialAtual} podeVerTodasAsFiliais={escopo.podeVerTodasAsFiliais} />;
      default:
        return <ConfigSecaoConta escopo={escopo} sessao={sessao} />;
    }
  }

  const nome = sessao.estado === 'autenticado' ? sessao.nome : (escopo?.usuario ?? contexto.usuario);

  // O MENU LATERAL SÓ APARECE QUANDO HÁ ESCOLHA: com uma seção na aba, ele repetiria o nome da aba.
  const corpo = (
    <div className={`config-body ${secoesDaAba.length > 1 ? '' : 'config-body-sem-menu'}`}>
      {secoesDaAba.length > 1 && (
        <aside className="config-sidebar">
          <MenuLateralConfig
            secoes={secoesDaAba}
            secaoAtiva={secaoValida?.id ?? 'conta'}
            onSelecionar={(secao) => setEscolha({ aba: abaAtual, secao })}
          />
        </aside>
      )}
      <section className="config-content">{renderSecao()}</section>
    </div>
  );

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Configurações</h1>
          <p className="page-subtitle">A sua conta e, conforme o seu perfil, a administração do CRM</p>
        </div>
        <div className="config-chip-no-cabecalho">
          <div className="cu-avatar" style={{ background: '#367C2B' }}>
            {iniciais(nome)}
          </div>
          <div>
            <div className="cu-nome">{nome}</div>
            <div className="cu-role">{escopo ? rotuloDosPerfis(escopo) : 'carregando o perfil…'}</div>
          </div>
        </div>
      </div>

      {abas.length > 1 ? (
        <AbasDaTela abas={abas} ativa={abaAtual} aoTrocar={selecionarAba} rotulo="Áreas de Configurações">
          {corpo}
        </AbasDaTela>
      ) : (
        corpo
      )}
    </PaginaDoPainel>
  );
}
