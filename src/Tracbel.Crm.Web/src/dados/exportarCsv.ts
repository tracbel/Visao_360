/**
 * Exportação para CSV — o conserto de meia dúzia de botões "Exportar" que não
 * exportavam nada.
 *
 * POR QUE ISTO EXISTE EM VEZ DE OS BOTÕES SAÍREM: exportar uma tabela que já
 * está inteira na memória do navegador é a única das ações decorativas do
 * protótipo que dá para cumprir sem inventar dado nenhum — o arquivo sai com
 * exatamente as linhas que a pessoa está vendo, com os filtros dela aplicados.
 * Os outros botões sem destino saíram (documento 06).
 *
 * DUAS DECISÕES DE FORMATO, as duas por causa do Excel em português:
 *
 * - **Separador `;`.** O Excel configurado em pt-BR lê vírgula como separador
 *   decimal, e um CSV com `,` cai tudo numa coluna só.
 * - **BOM de UTF-8 na frente.** Sem ele o Excel abre o arquivo em ANSI e todo
 *   acento vira caractere estranho — "Negociação" sai "NegociaÃ§Ã£o".
 */

/** Escapa um valor para uma célula de CSV. */
function celula(valor: unknown): string {
  if (valor == null) return '';
  const texto = String(valor);
  // Aspas, ponto-e-vírgula e quebra de linha obrigam a envolver em aspas, e a
  // aspa de dentro vira duas — é a regra do RFC 4180.
  if (/["\n\r;]/.test(texto)) return `"${texto.replace(/"/g, '""')}"`;
  return texto;
}

/**
 * Monta o CSV e entrega ao navegador como download.
 *
 * @param nomeArquivo sem extensão — o `.csv` entra aqui.
 * @param cabecalho os rótulos das colunas, na ordem.
 * @param linhas uma lista de valores por linha, na mesma ordem do cabeçalho.
 */
export function baixarCsv(nomeArquivo: string, cabecalho: string[], linhas: unknown[][]): void {
  const texto = [cabecalho, ...linhas].map((linha) => linha.map(celula).join(';')).join('\r\n');
  const conteudo = new Blob([`﻿${texto}`], { type: 'text/csv;charset=utf-8' });

  const endereco = URL.createObjectURL(conteudo);
  const ancora = document.createElement('a');
  ancora.href = endereco;
  ancora.download = `${nomeArquivo}.csv`;
  document.body.appendChild(ancora);
  ancora.click();
  ancora.remove();
  // Sem isto o Blob fica preso na memória da aba até ela fechar.
  URL.revokeObjectURL(endereco);
}

/** Data de hoje no nome do arquivo, para dois downloads não se sobrescreverem. */
export function carimboDeData(data: Date = new Date()): string {
  return data.toISOString().slice(0, 10);
}
