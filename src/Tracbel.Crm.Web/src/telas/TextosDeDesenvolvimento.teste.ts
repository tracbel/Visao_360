/**
 * NENHUMA TELA MOSTRA TEXTO DE QUEM PROGRAMA (29/09/2026, issue 31).
 *
 * O levantamento de 28/09 achou, no que o usuário lê, nome de tabela (`processo.Processo`, `comercial.ClienteCarteira`,
 * `organizacao.Meta`…), comando de carga (`--somente-credito`), SQL (`GROUP BY`, `DELETE`) e o GUID da API no campo
 * "Chave pública". Tudo saiu: o nome de tabela virou nome de negócio, a carga virou o nome da rotina como aparece em
 * Configurações › Integrações, e a linhagem até a tabela mora no documento 29.
 *
 * O teste lê o FONTE de cada tela e de cada componente e recusa esses textos onde eles aparecem para o usuário: dentro
 * de `<code>` e nos textos passados por propriedade (título, subtítulo, ajuda, dica, texto, resumo, mensagem).
 * Comentário de código fica de fora de propósito — é lá que a linhagem técnica deve morar.
 */

import { describe, expect, it } from 'vitest';

const FONTES = import.meta.glob(['../telas/**/*.tsx', '../componentes/**/*.tsx', '!../**/*.teste.tsx'], {
  query: '?raw',
  import: 'default',
  eager: true,
}) as Record<string, string>;

const ESQUEMAS = 'comercial|processo|organizacao|frota|seguranca|integracao|auditoria|mercado|territorio';

const PROIBIDOS: [string, RegExp][] = [
  ['nome de tabela dentro de <code>', new RegExp(`<code>[^<]*\\b(${ESQUEMAS})\\.[A-Za-z]`)],
  ['comando de carga dentro de <code>', /<code>\s*--[a-z]/],
  ['SQL ou verbo HTTP dentro de <code>', /<code>\s*(GROUP BY|SELECT|DELETE|PUT|POST|PATCH)\s*<\/code>/],
  [
    'nome de tabela num texto passado por propriedade',
    new RegExp(`\\b(titulo|subtitulo|ajuda|dica|texto|resumo|mensagem\\w*)="[^"]*\\b(${ESQUEMAS})\\.[A-Z]`),
  ],
  ['o GUID da API num campo "Chave pública"', /rotulo="Chave pública"/],
];

describe('os textos de desenvolvimento nas telas (issue 31)', () => {
  it('encontra as telas e os componentes', () => {
    // Sem isto, um glob errado deixaria o teste verde lendo nada.
    expect(Object.keys(FONTES).length).toBeGreaterThan(100);
  });

  it.each(Object.entries(FONTES).map(([arquivo, fonte]) => [arquivo.replace(/^\.\.\//, ''), fonte]))(
    '%s não mostra texto de quem programa',
    (_, fonte) => {
      for (const [oQue, padrao] of PROIBIDOS) {
        const achado = padrao.exec(fonte);
        expect(achado?.[0] ?? null, `${oQue}: ${achado?.[0]}`).toBeNull();
      }
    },
  );
});
