/**
 * O acesso HTTP à nossa API — o único lugar do front que conhece `fetch`.
 *
 * TRÊS COISAS MORAM AQUI, e nenhuma delas em tela:
 *
 * 1. **O endereço.** Em desenvolvimento, `/api` é redirecionado pelo Vite para
 *    `http://localhost:5145` (ver `vite.config.ts`); a API não publica CORS, e o
 *    redirecionamento é o que evita ligar CORS só por causa do ambiente de
 *    desenvolvimento. Em outro ambiente, `VITE_API_URL` manda.
 * 2. **O contexto de acesso.** Os dois cabeçalhos `X-Tracbel-Usuario` e
 *    `X-Tracbel-Empresa` (documento 23, seção 4.1). São PROVISÓRIOS e não
 *    autenticam ninguém; quando o Entra ID entrar, é este arquivo que troca o
 *    cabeçalho pelo token — e nenhuma tela fica sabendo.
 * 3. **A tradução da recusa.** Toda falha vira um {@link ErroDaApi}, com a lista
 *    `erros` campo a campo preservada. É o que permite ao formulário acender o
 *    campo certo em vez de mostrar um aviso genérico no topo.
 */

import type { ComProcedencia, ErroDeCampo, ProblemaDaApi } from '../../tipos/api';

/** A raiz da API. Em desenvolvimento é relativa, e o Vite redireciona. */
const RAIZ = import.meta.env.VITE_API_URL ?? '/api';

/** O contexto de acesso que acompanha toda requisição. */
export type ContextoDeAcesso = {
  /** O e-mail do usuário, resolvido contra `seguranca.Usuario`. */
  usuario: string;
  /** O código da filial (`0101NN`), resolvido contra `organizacao.Empresa`. */
  empresa: string;
};

/**
 * Uma recusa da API, já desmontada no que a tela precisa.
 *
 * O CAMPO MAIS IMPORTANTE É {@link erros}: a API recusa TRÊS campos de uma vez,
 * e não o primeiro — recusar campo a campo faria o usuário corrigir, reenviar e
 * descobrir o próximo, o mesmo trabalho repartido em três viagens (documento 23,
 * seção 3).
 */
export class ErroDaApi extends Error {
  /** O código HTTP. 422 validação, 404 fora do alcance, 409 conflito, 503 dependência. */
  readonly status: number;
  /** O `type` do problema, sem o prefixo. Ex.: `validacao`, `concorrencia`. */
  readonly tipo: string;
  /** O `detail`: a explicação da categoria da falha. */
  readonly detalhe: string | null;
  /** A lista campo a campo, quando a falha é de entrada. Vazia quando não é. */
  readonly erros: ErroDeCampo[];

  constructor(status: number, problema: ProblemaDaApi, mensagemPadrao: string) {
    super(problema.title?.trim() || mensagemPadrao);
    this.name = 'ErroDaApi';
    this.status = status;
    this.tipo = (problema.type ?? '').split('/').pop() ?? 'indefinido';
    this.detalhe = problema.detail ?? null;
    this.erros = problema.erros ?? [];
  }

  /** Alguém alterou o registro antes: a tela oferece recarregar sem perder o digitado. */
  get ehConcorrencia(): boolean {
    return this.tipo === 'concorrencia';
  }

  /** A entrada não serve: as mensagens vão para os campos. */
  get ehValidacao(): boolean {
    return this.tipo === 'validacao';
  }

  /** Não existe, ou não está ao alcance da filial escolhida. A API não distingue, de propósito. */
  get ehNaoEncontrado(): boolean {
    return this.tipo === 'nao-encontrado';
  }

  /**
   * As mensagens indexadas pelo nome do campo — é assim que o formulário acende
   * o campo certo. Quando o mesmo campo tem duas mensagens, a primeira vence:
   * duas frases empilhadas no mesmo rótulo confundem mais do que ajudam.
   */
  porCampo(): Record<string, string> {
    const mapa: Record<string, string> = {};
    for (const erro of this.erros) if (!(erro.campo in mapa)) mapa[erro.campo] = erro.mensagem;
    return mapa;
  }

  /**
   * As mensagens que não pertencem a nenhum campo da tela — as que sobrariam
   * invisíveis se a tela só olhasse os campos que conhece. Elas vão para o aviso
   * do topo, para nenhuma recusa da API desaparecer.
   */
  errosForaDoFormulario(camposDaTela: readonly string[]): ErroDeCampo[] {
    return this.erros.filter((e) => !camposDaTela.includes(e.campo));
  }
}

/** A falha de rede — a API não respondeu, e isso é diferente de ela ter recusado. */
export class ErroDeRede extends Error {
  constructor(causa: unknown) {
    super(
      'Não foi possível falar com a API do CRM. Confira se ela está no ar ' +
        '(`dotnet run --project src/Tracbel.Crm.Api`) e tente de novo.',
    );
    this.name = 'ErroDeRede';
    this.cause = causa;
  }
}

type Opcoes = {
  metodo?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  corpo?: unknown;
  parametros?: Record<string, string | number | boolean | undefined>;
  sinal?: AbortSignal;
};

/**
 * O aviso de que a sessão acabou.
 *
 * Um evento do navegador, e não uma dependência: quem chama a API não precisa
 * conhecer a tela de login, e a tela de login não precisa conhecer cada chamada.
 * `sessao.tsx` escuta; este arquivo só grita.
 */
export const EVENTO_SESSAO_EXPIRADA = 'tracbel:sessao-expirada';

/**
 * Faz a requisição e devolve o corpo já tipado, ou lança {@link ErroDaApi}.
 *
 * @param caminho Caminho a partir da raiz. Ex.: `/v1/clientes`.
 * @param contexto Usuário e filial que vão nos cabeçalhos.
 * @param opcoes Verbo, corpo, parâmetros de consulta e cancelamento.
 */
export async function pedir<T>(caminho: string, contexto: ContextoDeAcesso, opcoes: Opcoes = {}): Promise<T> {
  const { metodo = 'GET', corpo, parametros, sinal } = opcoes;

  const consulta = new URLSearchParams();
  for (const [chave, valor] of Object.entries(parametros ?? {})) {
    if (valor === undefined || valor === '') continue;
    consulta.set(chave, String(valor));
  }
  const sufixo = consulta.size > 0 ? `?${consulta}` : '';

  let resposta: Response;
  try {
    resposta = await fetch(`${RAIZ}${caminho}${sufixo}`, {
      method: metodo,
      signal: sinal,
      // O COOKIE DE SESSÃO VIAJA SOZINHO: mesma origem, e `same-origin` é o
      // padrão do `fetch`. Declarado para ninguém trocar por `omit` sem ver.
      credentials: 'same-origin',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        // COM O LOGIN PELO ENTRA ID LIGADO, a API IGNORA este cabeçalho: quem a
        // pessoa é sai do token. Ele continua saindo porque, desligado o login,
        // é a ponte provisória — e mandar sempre é mais simples que decidir aqui.
        'X-Tracbel-Usuario': contexto.usuario,
        'X-Tracbel-Empresa': contexto.empresa,
      },
      // O DELETE LEVA CORPO, e é de propósito: o motivo da inativação é de
      // catálogo e obrigatório. Na consulta ele iria parar no log do servidor
      // web e na barra do navegador (documento 23, seção 2.1).
      body: corpo === undefined ? undefined : JSON.stringify(corpo),
    });
  } catch (causa) {
    if (causa instanceof DOMException && causa.name === 'AbortError') throw causa;
    throw new ErroDeRede(causa);
  }

  if (resposta.status === 401) window.dispatchEvent(new Event(EVENTO_SESSAO_EXPIRADA));

  if (resposta.status === 204) return undefined as T;

  const texto = await resposta.text();
  const conteudo = texto.length > 0 ? (JSON.parse(texto) as unknown) : null;

  if (!resposta.ok) {
    throw new ErroDaApi(
      resposta.status,
      (conteudo ?? {}) as ProblemaDaApi,
      `A API recusou a requisição (HTTP ${resposta.status}).`,
    );
  }

  return conteudo as T;
}

/** Atalho para as leituras, que sempre vêm envelopadas com a procedência. */
export function ler<T>(caminho: string, contexto: ContextoDeAcesso, opcoes: Opcoes = {}) {
  return pedir<ComProcedencia<T>>(caminho, contexto, opcoes);
}
