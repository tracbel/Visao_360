/**
 * A porta de entrada — e o que a aplicação desenha antes de saber quem é você.
 *
 * NÃO HÁ CAMPO DE SENHA AQUI, e isso é o desenho, não uma simplificação. O CRM
 * não guarda senha de ninguém: quem autentica é o Microsoft Entra ID, que a
 * Tracbel já opera com MFA. [V] no Vórtice a senha tem 30 bytes sem sal e 80
 * usuários compartilham exatamente o mesmo valor armazenado. Um formulário de
 * usuário e senha nesta tela seria o convite para repetir isso.
 *
 * TRÊS SITUAÇÕES DIFERENTES CHEGAM AQUI, e a frase muda com cada uma: ninguém
 * entrou; a pessoa entrou mas o CRM não a conhece; ou a pessoa acabou de sair.
 * Tratar as três com o mesmo "faça login" mandaria quem não tem cadastro num
 * laço — entra, é recusado, volta, entra.
 */

import { useState } from 'react';
import { LogoTracbel } from '../componentes/Icones';
import type { Sessao } from '../dados/api/sessao';
import '../estilos/login.css';

type Aviso = { tipo: 'erro' | 'alerta' | 'sucesso'; texto: string } | null;

/** Lê `?erro=` e `?saiu=` do hash — é para onde o servidor devolve depois da Microsoft. */
function avisoDaUrl(): Aviso {
  const [, consulta = ''] = window.location.hash.split('?');
  const parametros = new URLSearchParams(consulta);

  const erro = parametros.get('erro');
  if (erro) return { tipo: 'erro', texto: erro };

  if (parametros.get('saiu')) return { tipo: 'sucesso', texto: 'Você saiu da sua conta com segurança.' };

  return null;
}

function avisoDaSessao(sessao: Sessao): Aviso {
  if (sessao.estado === 'sem-acesso') return { tipo: 'alerta', texto: sessao.mensagem };
  if (sessao.estado === 'erro') return { tipo: 'erro', texto: sessao.mensagem };
  return null;
}

/** O logotipo oficial da Microsoft, nas quatro cores — como pede o guia de marca de login. */
function LogoMicrosoft() {
  return (
    <svg width="20" height="20" viewBox="0 0 21 21" aria-hidden="true">
      <rect x="1" y="1" width="9" height="9" fill="#F25022" />
      <rect x="11" y="1" width="9" height="9" fill="#7FBA00" />
      <rect x="1" y="11" width="9" height="9" fill="#00A4EF" />
      <rect x="11" y="11" width="9" height="9" fill="#FFB900" />
    </svg>
  );
}

type Props = { sessao: Sessao; aoEntrar: () => void };

export function Login({ sessao, aoEntrar }: Props) {
  const [redirecionando, setRedirecionando] = useState(false);
  const aviso = avisoDaSessao(sessao) ?? avisoDaUrl();
  const semCadastro = sessao.estado === 'sem-acesso';

  function entrar() {
    setRedirecionando(true);
    aoEntrar();
  }

  return (
    <div className="login">
      <section className="login-marca" aria-hidden="true">
        <div className="login-marca-grade" />
        <div className="login-marca-conteudo">
          <div className="login-logo">
            <LogoTracbel />
            <span>Tracbel Agro</span>
          </div>

          <h2 className="login-slogan">
            Visão <strong>360</strong>
          </h2>
          <p className="login-tagline">
            O cliente inteiro num lugar só — do faturamento à última visita.
          </p>

          {/* O QUE A TELA ENTREGA, dito sem número. Um total aqui envelheceria na
              primeira carga, e número desatualizado na porta de entrada ensina a
              desconfiar de todos os que vêm depois. */}
          <ul className="login-pontos">
            <li>
              <span className="login-ponto-marcador" />
              Faturamento das dezesseis filiais, direto do Protheus
            </li>
            <li>
              <span className="login-ponto-marcador" />
              Carteira, cobertura e pós-venda lado a lado
            </li>
            <li>
              <span className="login-ponto-marcador" />
              Entrada com a sua conta corporativa, protegida por MFA
            </li>
          </ul>
        </div>
      </section>

      <main className="login-painel">
        <div className="login-cartao">
          <div className="login-logo login-logo-compacto">
            <LogoTracbel />
            <span>Tracbel Agro</span>
          </div>

          <h1 className="login-titulo">Entrar</h1>
          <p className="login-subtitulo">
            Use a sua conta corporativa Tracbel — a mesma do e-mail e do Teams.
          </p>

          <div className="login-aviso-area" aria-live="polite">
            {aviso && <div className={`login-aviso login-aviso-${aviso.tipo}`}>{aviso.texto}</div>}
          </div>

          <button
            type="button"
            className="login-botao-microsoft"
            onClick={entrar}
            disabled={redirecionando}
          >
            <LogoMicrosoft />
            {redirecionando
              ? 'Abrindo a Microsoft…'
              : semCadastro
                ? 'Entrar com outra conta Microsoft'
                : 'Entrar com a conta Microsoft'}
          </button>

          {semCadastro && (
            <p className="login-ajuda">
              O login funcionou, mas esta conta ainda não foi liberada no CRM. Entrar de novo com a
              mesma conta não resolve — fale com a TI para liberar o seu usuário.
            </p>
          )}

          <p className="login-rodape">
            Acesso restrito a colaboradores autorizados.
            <br />
            Suas credenciais são verificadas pela Microsoft; o CRM não vê nem guarda a sua senha.
          </p>
        </div>
      </main>
    </div>
  );
}
