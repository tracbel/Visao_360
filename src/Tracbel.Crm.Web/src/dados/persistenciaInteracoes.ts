/**
 * Persistência das interações registradas na Cobertura de Carteira.
 *
 * POR QUE ISTO EXISTE: a tela tinha um painel de registrar contato com escolha
 * de categoria, anotação e próxima ação, e o botão "Registrar contato"
 * respondia com um `alert()` dizendo que nada seria gravado — depois de
 * fechar o painel e jogar fora o que a pessoa tinha escrito. É o mesmo defeito
 * que o painel de andamento da Agenda tinha e que foi corrigido em 04/09/2026;
 * este arquivo é a mesma solução, no mesmo formato.
 *
 * O QUE MUDA NA TELA quando a interação é gravada: a linha do cliente passa a
 * mostrar a interação nova como "última interação", o contador de dias zera (ou
 * não zera, quando o cliente exige visita presencial e o contato foi remoto — a
 * regra de `calcularStatusCobertura` continua valendo sobre o dado novo) e os
 * KPIs de cobertura se recalculam. Sem isso, registrar contato não teria efeito
 * visível nenhum, e um registro sem efeito é o mesmo botão decorativo de antes.
 *
 * Quando a API existir, só este arquivo muda. A tela continua chamando as
 * mesmas funções.
 */
import type { CategoriaInteracao } from '../tipos/cobertura';

const CHAVE_INTERACOES = 'crm-tracbel:interacoes-registradas:v1';

export type InteracaoRegistrada = {
  cliente_id: number;
  cliente_razao: string;
  /** Data do contato, em ISO curto (`AAAA-MM-DD`) — igual ao `ult_int.data`. */
  data: string;
  categoria: CategoriaInteracao;
  anotacao: string;
  /** Código da próxima ação escolhida, ou vazio quando não há. */
  proxima_acao: string;
  registrada_em: string;
};

let interacoes: InteracaoRegistrada[] = [];
let carregadoDoStorage = false;

/** Wrapper defensivo: só retorna o Storage se leitura/escrita realmente funcionar. */
function obterStorage(): Storage | null {
  try {
    const s = window.localStorage;
    const chaveTeste = '__crm_test__';
    s.setItem(chaveTeste, '1');
    s.removeItem(chaveTeste);
    return s;
  } catch {
    return null;
  }
}

function persistir(): void {
  const s = obterStorage();
  if (!s) return;
  try {
    s.setItem(CHAVE_INTERACOES, JSON.stringify(interacoes));
  } catch (erro) {
    console.warn('[Cobertura] falha ao salvar a interação:', erro);
  }
}

function garantirCarregado(): void {
  if (carregadoDoStorage) return;
  carregadoDoStorage = true;
  const s = obterStorage();
  if (!s) return;
  try {
    const bruto = s.getItem(CHAVE_INTERACOES);
    const lista = bruto ? (JSON.parse(bruto) as unknown) : [];
    interacoes = Array.isArray(lista) ? (lista as InteracaoRegistrada[]) : [];
  } catch (erro) {
    console.warn('[Cobertura] falha ao carregar as interações:', erro);
  }
}

/** true se o navegador permite gravar (falso em sandbox). */
export function storageInteracoesDisponivel(): boolean {
  return obterStorage() !== null;
}

/** Todas as interações registradas nesta sessão (e nas anteriores, do navegador). */
export function obterInteracoes(): InteracaoRegistrada[] {
  garantirCarregado();
  return interacoes;
}

/**
 * A interação mais recente por cliente. É o que a tela sobrepõe ao `ult_int`
 * do JSON — a última vence, e só a última importa para o contador de cobertura.
 */
export function ultimaInteracaoPorCliente(): Map<number, InteracaoRegistrada> {
  garantirCarregado();
  const mapa = new Map<number, InteracaoRegistrada>();
  for (const i of interacoes) {
    const atual = mapa.get(i.cliente_id);
    if (!atual || i.data >= atual.data) mapa.set(i.cliente_id, i);
  }
  return mapa;
}

/** Grava a interação. */
export function registrarInteracao(interacao: InteracaoRegistrada): InteracaoRegistrada {
  garantirCarregado();
  interacoes = [...interacoes, interacao];
  persistir();
  return interacao;
}

/**
 * Desfaz o último registro de um cliente — a janela de desfazer do aviso de
 * sucesso (documento 05 §3: confirmação e desfazer).
 */
export function desfazerInteracao(registradaEm: string): void {
  garantirCarregado();
  interacoes = interacoes.filter((i) => i.registrada_em !== registradaEm);
  persistir();
}
