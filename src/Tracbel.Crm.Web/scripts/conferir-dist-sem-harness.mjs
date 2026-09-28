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

import { existsSync, readdirSync, readFileSync, statSync } from 'node:fs';
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
  // AS TELAS DO PROTÓTIPO QUE LEEM JSON FICTÍCIO (issue 191) — só no `npm run dev`. São os nomes dos arquivos que
  // elas pedem e o título do mapa do protótipo: se um deles aparecer no pacote, a tela voltou junto. (`carteira-cen`
  // não serve de marca: é também nome de classe no CSS global.)
  'cliente-84391',
  'equipamento-1RW7250PVMR123456',
  'catalogo-modelos',
  'Mapa do protótipo',
];

function arquivos(pasta) {
  return readdirSync(pasta).flatMap((nome) => {
    const caminho = join(pasta, nome);
    return statSync(caminho).isDirectory() ? arquivos(caminho) : [caminho];
  });
}

let achados = 0;

/**
 * A PASTA `dados` NÃO PODE ESTAR NO PACOTE (issue 191). O plugin do `vite.config.ts` a apaga depois do `build`; se
 * ele sair ou deixar de rodar, o JSON fictício volta a ser publicado sem ninguém ver.
 */
if (existsSync(join(DIST, 'dados'))) {
  console.error(`REPROVADO: ${join(DIST, 'dados')} existe — o JSON fictício do protótipo foi para o pacote.`);
  achados++;
}

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
    `\n${achados} marca(s) de desenvolvimento no pacote de produção.\n` +
      'Os gatilhos em App.tsx e rotas.tsx precisam manter as telas INALCANÇÁVEIS fora do\n' +
      'modo de desenvolvimento — veja o comentário do HarnessVisual lá e o topo de rotas.tsx.',
  );
  process.exit(1);
}

console.log('O pacote não tem nada do harness visual nem das telas do protótipo.');
