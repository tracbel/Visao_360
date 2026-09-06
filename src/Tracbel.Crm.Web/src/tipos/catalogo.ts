/**
 * Forma dos catálogos de referência — a mesma de `dados-referencia/*.json`
 * (ver `dados-referencia/README.md`, seção "Forma de cada arquivo"). Regra do
 * documento 05-MELHORIAS-COMBINADAS §2: **nenhum campo com catálogo aceita
 * digitação livre**. Por isso a tela nunca declara a lista de opções: ela lê
 * o catálogo pela camada de dados, exatamente como vai ler da tabela quando a
 * API existir.
 */

/** Item comum a todo catálogo: código estável, descrição de tela, ordem e ativo. */
export type ItemCatalogo = {
  /** Estável, maiúsculo, sem acento nem espaço. É o que vai para o histórico. */
  codigo: string;
  /** O rótulo em português que aparece na tela. */
  descricao: string;
  ordem: number;
  ativo: boolean;
  observacao?: string;
};

export type Catalogo<T extends ItemCatalogo = ItemCatalogo> = {
  catalogo: string;
  descricao: string;
  origem: string;
  itens: T[];
};

/** O que o resultado faz com o processo — semântica de `IV_ProcResultado` (pesquisa 16 §5.5). */
export type EfeitoResultado = 'neutro' | 'move_fase' | 'encerra_processo';

/** Item de `catalogo-resultado.json`. */
export type ItemResultado = ItemCatalogo & {
  efeito: EfeitoResultado;
  /** Código de `fase.json` para onde o processo vai. `null` quando o resultado é neutro. */
  fase_seguinte: string | null;
  /** Código de `catalogo-passo-seguinte.json` sugerido ao escolher este resultado. */
  passo_seguinte_padrao: string;
};

/** Item de `catalogo-passo-seguinte.json`. */
export type ItemPassoSeguinte = ItemCatalogo & {
  /** Intervalo em dias corridos até a próxima tarefa. `null` quando não gera tarefa. */
  dias: number | null;
};

/** Item de `catalogo-categoria-interacao.json` (cópia de `dados-referencia`). */
export type ItemCategoriaInteracao = ItemCatalogo;
