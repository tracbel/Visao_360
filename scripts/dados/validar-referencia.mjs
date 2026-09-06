#!/usr/bin/env node
/**
 * validar-referencia.mjs — Valida a FORMA dos catálogos de `dados-referencia/` (as tabelas
 * que alimentam os campos de seleção dos formulários do CRM).
 *
 * NÃO CORRIGE NADA. Só relata:
 *   - forma do JSON (campos obrigatórios do arquivo e de cada item, tipos)
 *   - código duplicado dentro do mesmo catálogo
 *   - código com acento ou espaço (código tem que ser estável e vai para o histórico —
 *     documento 16 §3.1)
 *   - ordem repetida dentro do mesmo catálogo
 *   - descrição vazia
 *   - item sem `ativo` (ausente ou não booleano)
 *
 * Uso:
 *   node scripts/dados/validar-referencia.mjs
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const RAIZ = path.resolve(__dirname, '..', '..');
const PASTA_REFERENCIA = path.join(RAIZ, 'dados-referencia');

const CAMPOS_ARQUIVO = ['catalogo', 'descricao', 'origem', 'itens'];
const CAMPOS_ITEM = ['codigo', 'descricao', 'ordem', 'ativo', 'observacao'];

// Código estável: maiúsculo, dígitos, underscore — nunca acento, espaço ou minúscula.
const PADRAO_CODIGO_VALIDO = /^[A-Z0-9_]+$/;

/**
 * Verifica se a string tem letra acentuada ou cedilha. Decompõe em NFD — "á" (1 char) vira
 * "a" + marca diacrítica combinante (2 chars, ponto de código 0x0300 a 0x036F) — e procura
 * por essa marca. Usa código numérico (não classe de regex com caractere literal) de propósito:
 * um acento digitado direto no código-fonte deste validador seria irônico e frágil a encoding.
 */
function TEM_ACENTO(valor) {
  const decomposto = String(valor).normalize('NFD');
  for (const caractere of decomposto) {
    const codigoUnicode = caractere.codePointAt(0);
    if (codigoUnicode >= 0x0300 && codigoUnicode <= 0x036f) return true;
  }
  return false;
}

const TEM_ESPACO = (s) => /\s/.test(s);

/**
 * Valida um arquivo de catálogo. Devolve a lista de achados — nunca lança para arquivo
 * malformado; um JSON inválido também vira achado (para aparecer no relatório, não travar).
 */
function validarArquivo(nomeArquivo, textoOriginal) {
  const achados = [];
  const registrar = (regra, motivo) => achados.push({ arquivo: nomeArquivo, regra, motivo });

  let dado;
  try {
    dado = JSON.parse(textoOriginal);
  } catch (erro) {
    registrar('json-invalido', `arquivo não é um JSON válido: ${erro.message}`);
    return achados;
  }

  if (dado === null || typeof dado !== 'object' || Array.isArray(dado)) {
    registrar('forma-arquivo', 'raiz do JSON precisa ser um objeto ({ catalogo, descricao, origem, itens })');
    return achados;
  }

  for (const campo of CAMPOS_ARQUIVO) {
    if (!(campo in dado)) registrar('forma-arquivo', `campo obrigatório '${campo}' ausente na raiz do arquivo`);
  }
  if ('catalogo' in dado && typeof dado.catalogo !== 'string') {
    registrar('forma-arquivo', "campo 'catalogo' precisa ser texto");
  }
  const nomeSemExtensao = nomeArquivo.replace(/\.json$/, '');
  if (typeof dado.catalogo === 'string' && dado.catalogo !== nomeSemExtensao) {
    registrar('forma-arquivo', `campo 'catalogo' ('${dado.catalogo}') não bate com o nome do arquivo ('${nomeSemExtensao}')`);
  }
  if ('descricao' in dado && (typeof dado.descricao !== 'string' || dado.descricao.trim() === '')) {
    registrar('forma-arquivo', "campo 'descricao' (do catálogo) vazio ou não é texto");
  }
  if ('origem' in dado && (typeof dado.origem !== 'string' || dado.origem.trim() === '')) {
    registrar('forma-arquivo', "campo 'origem' vazio ou não é texto");
  }
  if (!('itens' in dado) || !Array.isArray(dado.itens)) {
    registrar('forma-arquivo', "campo 'itens' ausente ou não é uma lista — parando a validação deste arquivo");
    return achados;
  }

  const codigosVistos = new Map(); // codigo -> [índices]
  const ordensVistas = new Map(); // ordem -> [índices]

  dado.itens.forEach((item, i) => {
    const rotulo = `itens[${i}]`;

    if (item === null || typeof item !== 'object' || Array.isArray(item)) {
      registrar('forma-item', `${rotulo}: item precisa ser um objeto`);
      return;
    }

    for (const campo of CAMPOS_ITEM) {
      if (!(campo in item)) registrar('forma-item', `${rotulo}: campo obrigatório '${campo}' ausente`);
    }

    // codigo
    if ('codigo' in item) {
      if (typeof item.codigo !== 'string' || item.codigo === '') {
        registrar('forma-item', `${rotulo}: 'codigo' vazio ou não é texto`);
      } else {
        const cod = item.codigo;
        if (!codigosVistos.has(cod)) codigosVistos.set(cod, []);
        codigosVistos.get(cod).push(i);
        if (TEM_ACENTO(cod)) registrar('codigo-acento', `${rotulo}: codigo '${cod}' tem acento ou cedilha`);
        if (TEM_ESPACO(cod)) registrar('codigo-espaco', `${rotulo}: codigo '${cod}' tem espaço`);
        if (!PADRAO_CODIGO_VALIDO.test(cod)) {
          registrar('codigo-formato', `${rotulo}: codigo '${cod}' fora do padrão (só A-Z, 0-9 e _ )`);
        }
      }
    }

    // descricao (do item)
    if ('descricao' in item) {
      if (typeof item.descricao !== 'string' || item.descricao.trim() === '') {
        registrar('descricao-vazia', `${rotulo}: descrição vazia ou não é texto`);
      }
    }

    // ordem
    if ('ordem' in item) {
      if (typeof item.ordem !== 'number' || !Number.isFinite(item.ordem)) {
        registrar('forma-item', `${rotulo}: 'ordem' não é número`);
      } else {
        if (!ordensVistas.has(item.ordem)) ordensVistas.set(item.ordem, []);
        ordensVistas.get(item.ordem).push(i);
      }
    }

    // ativo
    if (!('ativo' in item)) {
      registrar('item-sem-ativo', `${rotulo}: campo 'ativo' ausente`);
    } else if (typeof item.ativo !== 'boolean') {
      registrar('item-sem-ativo', `${rotulo}: 'ativo' presente mas não é booleano (valor: ${JSON.stringify(item.ativo)})`);
    }

    // observacao — obrigatório existir (pode ser string vazia), mas precisa ser texto
    if ('observacao' in item && typeof item.observacao !== 'string') {
      registrar('forma-item', `${rotulo}: 'observacao' presente mas não é texto`);
    }
  });

  for (const [cod, indices] of codigosVistos) {
    if (indices.length > 1) {
      registrar('codigo-duplicado', `codigo '${cod}' repetido em itens[${indices.join(', ')}]`);
    }
  }
  for (const [ordem, indices] of ordensVistas) {
    if (indices.length > 1) {
      registrar('ordem-repetida', `ordem ${ordem} repetida em itens[${indices.join(', ')}]`);
    }
  }

  return achados;
}

// ---------------------------------------------------------------------------
// Execução
// ---------------------------------------------------------------------------

if (!fs.existsSync(PASTA_REFERENCIA)) {
  console.error(`Pasta não encontrada: ${PASTA_REFERENCIA}`);
  process.exit(1);
}

const arquivosJson = fs
  .readdirSync(PASTA_REFERENCIA)
  .filter((f) => f.endsWith('.json'))
  .sort();

const todosOsAchados = [];
const porArquivo = new Map();
const itensPorArquivo = new Map();

for (const nomeArquivo of arquivosJson) {
  const caminhoCompleto = path.join(PASTA_REFERENCIA, nomeArquivo);
  const texto = fs.readFileSync(caminhoCompleto, 'utf8');
  const achados = validarArquivo(nomeArquivo, texto);
  todosOsAchados.push(...achados);
  porArquivo.set(nomeArquivo, achados.length);
  try {
    const dado = JSON.parse(texto);
    itensPorArquivo.set(nomeArquivo, Array.isArray(dado.itens) ? dado.itens.length : 0);
  } catch {
    itensPorArquivo.set(nomeArquivo, 0);
  }
}

function tabela(cabecalhos, linhas) {
  const larguras = cabecalhos.map((c, i) => Math.max(c.length, ...linhas.map((l) => String(l[i]).length)));
  const formatarLinha = (celulas) => celulas.map((c, i) => String(c).padEnd(larguras[i])).join('  ');
  const separador = larguras.map((l) => '-'.repeat(l)).join('  ');
  return [formatarLinha(cabecalhos), separador, ...linhas.map(formatarLinha)].join('\n');
}

const totalAchados = todosOsAchados.length;
const totalItens = [...itensPorArquivo.values()].reduce((a, b) => a + b, 0);

console.log(`\nValidação de ${arquivosJson.length} catálogos em dados-referencia/ (${totalItens} itens no total)`);
console.log(`Total de achados: ${totalAchados}\n`);

console.log('--- Por arquivo ---');
console.log(
  tabela(
    ['arquivo', 'itens', 'achados'],
    arquivosJson.map((f) => [f, itensPorArquivo.get(f) ?? 0, porArquivo.get(f) ?? 0]),
  ),
);

console.log('\n--- Por regra ---');
const porRegra = new Map();
for (const a of todosOsAchados) porRegra.set(a.regra, (porRegra.get(a.regra) ?? 0) + 1);
if (porRegra.size === 0) {
  console.log('(nenhum achado)');
} else {
  console.log(
    tabela(
      ['regra', 'achados'],
      [...porRegra.entries()].sort((a, b) => b[1] - a[1]),
    ),
  );
}

console.log('\n--- Catálogos vazios (itens: []) ---');
const vazios = arquivosJson.filter((f) => (itensPorArquivo.get(f) ?? 0) === 0);
if (vazios.length === 0) {
  console.log('(nenhum)');
} else {
  for (const f of vazios) console.log(`  - ${f} — ver dados-referencia/PENDENTES.md`);
}

console.log('\n--- Todos os achados ---');
if (totalAchados === 0) {
  console.log('(nenhum achado — todos os catálogos passaram nas seis checagens)');
} else {
  for (const f of arquivosJson) {
    const achadosDoArquivo = todosOsAchados.filter((a) => a.arquivo === f);
    if (achadosDoArquivo.length === 0) continue;
    console.log(`\n${f}:`);
    for (const a of achadosDoArquivo) console.log(`  [${a.regra}] ${a.motivo}`);
  }
}

console.log(`\n${totalAchados === 0 ? 'OK' : 'ATENÇÃO'} — ${totalAchados} achado(s) em ${arquivosJson.length} arquivo(s).\n`);

process.exit(totalAchados === 0 ? 0 : 1);
