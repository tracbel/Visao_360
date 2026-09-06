/**
 * Carregador dos dados do protótipo.
 *
 * Hoje lê os JSONs extraídos do protótipo original (`public/dados/*.json`,
 * gerados por `prototipo/dados-seed/extrair.mjs`). Quando a API existir, só
 * esta camada muda: as telas continuam consumindo as mesmas funções.
 */

const cache = new Map<string, unknown>();

export async function carregar<T>(nome: string): Promise<T> {
  if (cache.has(nome)) return cache.get(nome) as T;
  const resposta = await fetch(`${import.meta.env.BASE_URL}dados/${nome}.json`);
  if (!resposta.ok) {
    throw new Error(`Falha ao carregar dados "${nome}": ${resposta.status}`);
  }
  const dados = (await resposta.json()) as T;
  cache.set(nome, dados);
  return dados;
}

/** Carrega vários arquivos de uma vez, preservando a ordem pedida. */
export function carregarVarios<T extends unknown[]>(...nomes: string[]): Promise<T> {
  return Promise.all(nomes.map((n) => carregar(n))) as Promise<T>;
}

/**
 * Esquece a resposta guardada de um arquivo, para a próxima leitura ir de novo
 * à rede. É o que permite ao botão "Tentar de novo" tentar de verdade — com o
 * cache intacto, ele devolveria o mesmo erro sem sequer sair da máquina.
 */
export function esquecer(nome: string): void {
  cache.delete(nome);
}
