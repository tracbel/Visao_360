/**
 * O HARNESS VISUAL NÃO PODE ESTAR NO PACOTE (fase T4.5).
 *
 * POR QUE ISTO É UM SCRIPT, e não um comentário pedindo cuidado: em 23/09/2026 a
 * primeira versão do gatilho usava `lazy(() => import('./dev/HarnessVisual'))`
 * solto no escopo do módulo. O ramo `import.meta.env.DEV` morria no build, mas o
 * `import()` continuava alcançável e o Rollup gerava o chunk assim mesmo — 13 kB
 * de amostra fictícia dentro do `dist`, sem nenhum aviso.
 *
 * Quem for mexer no gatilho não tem como saber disso de cabeça. Esta conferência
 * é o que transforma "lembre-se de checar" em algo que falha sozinho.
 *
 * Roda depois do `build` e varre TODO o `dist`, não só o index: o problema
 * original era justamente um arquivo à parte.
 */

import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join } from 'node:path';

const DIST = 'dist';

/**
 * Marcas que só existem no harness e nas amostras. Qualquer uma reprova.
 *
 * AS DO HARNESS DO SHELL ENTRARAM JUNTO (fase 5): ele tem o mesmo gatilho em
 * `App.tsx` e o mesmo risco — um `import()` alcançável levaria o `Layout` de
 * mentira e a sessão fictícia para o pacote.
 *
 * AS DO HARNESS DA VISÃO 360 TAMBÉM: o mesmo gatilho, o mesmo risco — e as
 * amostras dele têm filial, cliente e faturamento inventados.
 */
const MARCAS = [
  'AMOSTRA FICTÍCIA',
  'HarnessVisual',
  'mercado-visual',
  'painelFicticio',
  'shell-visual',
  'HarnessDoShell',
  'visao360-visual',
  'HarnessDaVisao360',
  'amostrasDaVisao360',
];

function arquivos(pasta) {
  return readdirSync(pasta).flatMap((nome) => {
    const caminho = join(pasta, nome);
    return statSync(caminho).isDirectory() ? arquivos(caminho) : [caminho];
  });
}

let achados = 0;

for (const caminho of arquivos(DIST)) {
  // A malha e as imagens não são texto do nosso código; ler tudo como utf8 e
  // procurar marca ali só geraria falso positivo caro de entender.
  if (!/\.(js|css|html|json)$/i.test(caminho)) continue;
  if (caminho.includes('geo')) continue;

  const conteudo = readFileSync(caminho, 'utf8');
  for (const marca of MARCAS) {
    if (conteudo.includes(marca)) {
      console.error(`REPROVADO: "${marca}" está em ${caminho}`);
      achados++;
    }
  }
}

if (achados > 0) {
  console.error(
    `\n${achados} marca(s) do harness visual no pacote de produção.\n` +
      'O gatilho em App.tsx precisa manter o import() INALCANÇÁVEL fora do modo de\n' +
      'desenvolvimento — veja o comentário do HarnessVisual lá.',
  );
  process.exit(1);
}

console.log('O pacote não tem nada do harness visual.');
