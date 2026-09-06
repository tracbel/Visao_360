/**
 * Tipos da tela Nova Oportunidade — espelham as estruturas de dados do
 * protótipo de referência (`prototipo/referencia/assets/app.js`):
 * `NOVA_OP_STATE` (linha 6583), `CATALOGO_MODELOS` (linha 6601),
 * `CARTEIRA_CEN` (linha 3389) e o literal `nova` montado em
 * `salvarNovaOportunidade()` (linha 7014).
 */

/** Um modelo do catálogo, com valor de referência (`catalogo-modelos.json`). */
export type ModeloCatalogo = {
  modelo: string;
  valor_ref: number;
};

/** Catálogo de modelos agrupado por linha de produto. */
export type CatalogoModelos = Record<string, ModeloCatalogo[]>;

/** Última interação registrada com o cliente (agenda/cobertura). */
export type UltimaInteracao = {
  data: string;
  cat: string;
  autor: string;
} | null;

/** Cliente da carteira do CEN (`carteira-cen.json`). */
export type ClienteCarteira = {
  id: number;
  razao: string;
  apelido: string;
  classe: string;
  cidade: string;
  uf: string;
  lat: number;
  lng: number;
  exige_visita: boolean;
  ult_int: UltimaInteracao;
  fat_12m: number;
  oportunidades: number;
  oportunidades_valor?: number;
  obs: string;
};

/** Fase do pipeline comercial (`pipeline-fases.json`). */
export type FasePipeline = {
  id: string;
  label: string;
  hint: string;
};

/** Estado do formulário da tela — espelha `NOVA_OP_STATE`. */
export type NovaOportunidadeEstado = {
  cliente_id: number | null;
  cliente_nome: string;
  cliente_classe: string;
  cliente_cidade: string;
  cliente_novo: boolean;
  linha: string;
  modelo: string;
  quantidade: number;
  valor_unitario: number;
  fase_inicial: string;
  previsao_fechamento: string;
  probabilidade: number;
  titulo: string;
  observacao: string;
};

/**
 * Oportunidade persistida no localStorage ao salvar — mesmo formato que a
 * tela de Pipeline (outra frente) lê de volta. Espelha o literal `nova` em
 * `salvarNovaOportunidade()`.
 */
export type OportunidadeNova = {
  id: string;
  titulo: string;
  cliente: string;
  cliente_id: number | null;
  classe: string;
  cidade: string;
  cen: string;
  linha: string;
  modelo: string;
  valor: number;
  probabilidade: number;
  previsao: string;
  dias_fase: number;
  fase: string;
  observacao: string;
  criada_agora: boolean;
  criada_em: string;
  prospect_novo: boolean;
};
