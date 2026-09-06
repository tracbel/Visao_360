/**
 * Persistência do andamento das tarefas da Agenda do CEN.
 *
 * Mesmo padrão de `persistencia.ts` (as oportunidades da tela Nova
 * Oportunidade): estado em memória espelhado no `localStorage`, com o mesmo
 * tratamento defensivo em try/catch — o acesso pode ser bloqueado em ambiente
 * sandbox, e nesse caso a gravação falha em silêncio e a tela continua
 * funcionando só com o estado da sessão.
 *
 * Duas chaves, porque são dois fatos diferentes na trilha de auditoria:
 *
 * - `crm-tracbel:tarefas-concluidas:v1` — o andamento gravado ao concluir.
 *   No Vórtice isso é a linha de `IV_Historico` mais o fechamento da agenda
 *   (`Realizada = 'S'`, `DtaRealizacao`), na mesma transação (pesquisa 16 §5.1).
 * - `crm-tracbel:tarefas-reagendadas:v1` — a mudança de data, que **não**
 *   conclui nada: a tarefa continua pendente, em outro dia.
 *
 * Quando a API existir, só este arquivo muda. A tela continua chamando as
 * mesmas funções.
 */
import type { AndamentoTarefa, ReagendamentoTarefa, Tarefa } from '../tipos/agenda';

const CHAVE_CONCLUIDAS = 'crm-tracbel:tarefas-concluidas:v1';
const CHAVE_REAGENDADAS = 'crm-tracbel:tarefas-reagendadas:v1';
/**
 * As tarefas criadas na própria tela, pelo botão "Nova tarefa" — que até
 * 05/09/2026 era a ação primária da Agenda e não fazia nada.
 *
 * Ficam numa chave à parte das concluídas e das reagendadas porque são um fato
 * diferente: no Vórtice isto é uma linha nova em `IV_Agenda`, não uma alteração
 * de uma existente. E ficam separadas do `agenda.json` porque o arquivo é o
 * retrato do protótipo — misturar o que a pessoa criou com o que veio de
 * fábrica tiraria a possibilidade de voltar ao estado inicial.
 */
const CHAVE_CRIADAS = 'crm-tracbel:tarefas-criadas:v1';
/**
 * Onde começam os identificadores das tarefas criadas aqui. Fica bem acima dos
 * do `agenda.json` para nunca colidir com eles — colisão de identificador faria
 * concluir uma tarefa fechar outra.
 */
const PRIMEIRO_ID_CRIADO = 900001;

let andamentos: AndamentoTarefa[] = [];
let reagendamentos: ReagendamentoTarefa[] = [];
let criadas: Tarefa[] = [];
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
    s.setItem(CHAVE_CONCLUIDAS, JSON.stringify(andamentos));
    s.setItem(CHAVE_REAGENDADAS, JSON.stringify(reagendamentos));
    s.setItem(CHAVE_CRIADAS, JSON.stringify(criadas));
  } catch (erro) {
    console.warn('[Agenda] falha ao salvar o andamento:', erro);
  }
}

function lerLista<T>(s: Storage, chave: string): T[] {
  const bruto = s.getItem(chave);
  if (!bruto) return [];
  const arr = JSON.parse(bruto) as unknown;
  return Array.isArray(arr) ? (arr as T[]) : [];
}

function garantirCarregado(): void {
  if (carregadoDoStorage) return;
  carregadoDoStorage = true;
  const s = obterStorage();
  if (!s) return;
  try {
    andamentos = lerLista<AndamentoTarefa>(s, CHAVE_CONCLUIDAS);
    reagendamentos = lerLista<ReagendamentoTarefa>(s, CHAVE_REAGENDADAS);
    criadas = lerLista<Tarefa>(s, CHAVE_CRIADAS);
  } catch (erro) {
    console.warn('[Agenda] falha ao carregar o andamento:', erro);
  }
}

/** true se o navegador permite gravar no localStorage (falso em sandbox). */
export function storageAgendaDisponivel(): boolean {
  return obterStorage() !== null;
}

/** Andamentos gravados (recuperados do localStorage se houver). */
export function obterAndamentos(): AndamentoTarefa[] {
  garantirCarregado();
  return andamentos;
}

/** Reagendamentos gravados. */
export function obterReagendamentos(): ReagendamentoTarefa[] {
  garantirCarregado();
  return reagendamentos;
}

/**
 * Grava o andamento e fecha a tarefa. Regrava por cima se a mesma tarefa já
 * tiver andamento — não existe concluir duas vezes.
 */
export function concluirTarefa(andamento: AndamentoTarefa): AndamentoTarefa {
  garantirCarregado();
  andamentos = [...andamentos.filter((a) => a.tarefa_id !== andamento.tarefa_id), andamento];
  persistir();
  return andamento;
}

/** Desfaz a conclusão: a tarefa volta a ser pendente, com a data que tinha. */
export function desfazerConclusao(tarefaId: number): void {
  garantirCarregado();
  andamentos = andamentos.filter((a) => a.tarefa_id !== tarefaId);
  persistir();
}

/**
 * Grava a nova data de uma tarefa que continua pendente. O último
 * reagendamento vence, e `data_anterior` guarda de onde ela veio.
 */
export function reagendarTarefa(reagendamento: ReagendamentoTarefa): ReagendamentoTarefa {
  garantirCarregado();
  reagendamentos = [...reagendamentos.filter((r) => r.tarefa_id !== reagendamento.tarefa_id), reagendamento];
  persistir();
  return reagendamento;
}

/** As tarefas criadas na tela, que entram na lista junto com as do arquivo. */
export function obterTarefasCriadas(): Tarefa[] {
  garantirCarregado();
  return criadas;
}

/**
 * Grava uma tarefa nova e devolve a tarefa já com o identificador dela.
 *
 * O identificador é o maior já usado mais um, e nunca abaixo de
 * `PRIMEIRO_ID_CRIADO`: reaproveitar um número de tarefa apagada faria a
 * conclusão gravada da antiga fechar a nova assim que ela aparecesse.
 */
export function criarTarefa(tarefa: Omit<Tarefa, 'id'>): Tarefa {
  garantirCarregado();
  const proximoId = Math.max(PRIMEIRO_ID_CRIADO - 1, ...criadas.map((t) => t.id)) + 1;
  const nova: Tarefa = { ...tarefa, id: proximoId };
  criadas = [...criadas, nova];
  persistir();
  return nova;
}

/** Apaga uma tarefa criada aqui — a janela de desfazer do aviso de sucesso. */
export function apagarTarefaCriada(id: number): void {
  garantirCarregado();
  criadas = criadas.filter((t) => t.id !== id);
  persistir();
}

/** Remove todo o andamento da sessão (memória + localStorage). */
export function limparAndamentoPersistido(): void {
  garantirCarregado();
  andamentos = [];
  reagendamentos = [];
  criadas = [];
  const s = obterStorage();
  if (!s) return;
  try {
    s.removeItem(CHAVE_CONCLUIDAS);
    s.removeItem(CHAVE_REAGENDADAS);
    s.removeItem(CHAVE_CRIADAS);
  } catch (erro) {
    console.warn('[Agenda] falha ao limpar o andamento:', erro);
  }
}
