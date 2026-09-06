/**
 * Leitura dos catálogos de referência pela camada de dados.
 *
 * Regra do documento 05-MELHORIAS-COMBINADAS §2 ("formulário que puxa de
 * tabela"): campo com catálogo não aceita digitação livre, e a lista de opções
 * não mora no componente. Hoje os arquivos vêm de `public/dados/catalogo-*.json`
 * (cópia de carga de `dados-referencia/`); quando a API existir, só a função
 * `carregar()` muda — a tela continua chamando `useCatalogo`.
 *
 * `itensAtivos()` aplica as duas regras do `README.md` de `dados-referencia`:
 * item com `ativo: false` some das telas de criação (mas continua legível no
 * histórico), e a ordem de exibição é a coluna `ordem`, não a alfabética.
 */
import { useDados } from './useDados';
import type { Catalogo, ItemCatalogo } from '../tipos/catalogo';

/** Nome do arquivo em `public/dados/` de cada catálogo que a Agenda usa. */
export const ARQUIVO_CATALOGO = {
  resultado: 'catalogo-resultado',
  passo_seguinte: 'catalogo-passo-seguinte',
  categoria_de_interacao: 'catalogo-categoria-interacao',
} as const;

/** Hook de leitura de um catálogo, com os mesmos estados de carga e erro das telas. */
export function useCatalogo<T extends ItemCatalogo>(arquivo: string) {
  return useDados<Catalogo<T>>(arquivo);
}

/** Itens oferecíveis numa tela de criação: só os ativos, na ordem declarada. */
export function itensAtivos<T extends ItemCatalogo>(catalogo: Catalogo<T> | null): T[] {
  if (!catalogo) return [];
  return catalogo.itens.filter((i) => i.ativo).sort((a, b) => a.ordem - b.ordem);
}

/**
 * Descrição de um código — inclusive de item aposentado, que continua sendo
 * exibido em registro antigo. Devolve o próprio código quando não encontra,
 * para nunca esconder do usuário o que está gravado.
 */
export function descricaoDe<T extends ItemCatalogo>(catalogo: Catalogo<T> | null, codigo: string): string {
  return catalogo?.itens.find((i) => i.codigo === codigo)?.descricao ?? codigo;
}
