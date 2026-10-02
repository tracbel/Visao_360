/** A ordenação das tabelas das telas de mercado: a coluna, o sentido e a regra de clicar de novo. */

export type Ordem<C extends string> = { coluna: C; sentido: 1 | -1 };

/** Clicar na coluna já escolhida inverte; numa nova, texto começa de A a Z e número do maior para o menor. */
export function proximaOrdem<C extends string>(atual: Ordem<C>, coluna: C, texto: boolean): Ordem<C> {
  return atual.coluna === coluna ? { coluna, sentido: atual.sentido === 1 ? -1 : 1 } : { coluna, sentido: texto ? 1 : -1 };
}

/** Ordena sem mexer na lista recebida; o vazio vai sempre para o fim, nos dois sentidos. */
export function ordenar<T, C extends string>(linhas: readonly T[], ordem: Ordem<C>, valor: (linha: T, coluna: C) => number | string | null): T[] {
  return [...linhas].sort((a, b) => {
    const va = valor(a, ordem.coluna);
    const vb = valor(b, ordem.coluna);
    if (va === null && vb === null) return 0;
    if (va === null) return 1;
    if (vb === null) return -1;
    if (typeof va === 'string' && typeof vb === 'string') return ordem.sentido * va.localeCompare(vb, 'pt-BR');
    return ordem.sentido * ((va as number) - (vb as number));
  });
}
