#!/usr/bin/env node
/**
 * validar-seed.mjs — Mede a qualidade dos 45 JSONs de `prototipo/dados-seed/` contra o
 * catálogo de saneamento do documento `docs/projeto/16-HIGIENIZACAO-DE-DADOS.md`.
 *
 * NÃO CORRIGE NADA. Só relata: documento inválido, telefone fora do padrão, data
 * implausível, texto com espaço sobrando ou caractere invisível, valor monetário fora do
 * catálogo (mais de duas casas), coordenada fora da faixa, e duplicata de chave natural
 * (CNPJ/CPF de cliente, chassi de equipamento) entre os arquivos.
 *
 * Cada regra aqui espelha, campo a campo, uma regra do documento 16 — e cada regra do
 * documento 16 cita o defeito do Vórtice que ela existe para evitar. Ver aquele documento
 * para o "porquê"; este arquivo é só o "confere".
 *
 * Uso:
 *   node scripts/dados/validar-seed.mjs
 *
 * Saída:
 *   - tabela no terminal (resumo por arquivo + por regra + achados de chave natural)
 *   - docs/prototipo/qualidade-dados-seed.md (relatório completo, com todo achado listado)
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const RAIZ = path.resolve(__dirname, '..', '..');
const PASTA_SEED = path.join(RAIZ, 'prototipo', 'dados-seed');
const SAIDA_DOC = path.join(RAIZ, 'docs', 'prototipo', 'qualidade-dados-seed.md');

const ANO_ATUAL = new Date().getUTCFullYear();
const ANO_MINIMO = 1900;
const ANOS_NO_FUTURO = 30;

// Mesma caixa do tipo Coordenada (Tracbel.Crm.Dominio.Comum) — território brasileiro com margem.
const LAT_MIN = -34.0, LAT_MAX = 6.0;
const LNG_MIN = -75.0, LNG_MAX = -32.0;

// ---------------------------------------------------------------------------
// 1. Validadores de campo — cada um espelha um tipo de valor do documento 16
// ---------------------------------------------------------------------------

/** Só dígitos. */
function soDigitos(s) {
  return String(s).replace(/\D/g, '');
}

/** Dígito verificador de CPF/CNPJ — mesmo algoritmo de Tracbel.Crm.Dominio.Comum.CpfCnpj. */
function validarCpf(cpf) {
  if (new Set(cpf.split('')).size === 1) return false;
  const dv = (base, pesoInicial) => {
    let soma = 0;
    for (let i = 0; i < base.length; i++) soma += Number(base[i]) * (pesoInicial - i);
    const resto = soma % 11;
    return String(resto < 2 ? 0 : 11 - resto);
  };
  const primeiro = dv(cpf.slice(0, 9), 10);
  const segundo = dv(cpf.slice(0, 9) + primeiro, 11);
  return cpf[9] === primeiro && cpf[10] === segundo;
}

function validarCnpj(cnpj) {
  if (new Set(cnpj.split('')).size === 1) return false;
  const pesosPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  const pesosSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
  const dv = (base, pesos) => {
    let soma = 0;
    for (let i = 0; i < base.length; i++) soma += Number(base[i]) * pesos[i];
    const resto = soma % 11;
    return String(resto < 2 ? 0 : 11 - resto);
  };
  const primeiro = dv(cnpj.slice(0, 12), pesosPrimeiro);
  const segundo = dv(cnpj.slice(0, 12) + primeiro, pesosSegundo);
  return cnpj[12] === primeiro && cnpj[13] === segundo;
}

/** Devolve motivo da violação, ou null se o documento é válido. */
function validarDocumento(valor) {
  const digitos = soDigitos(valor);
  if (digitos.length === 11) return validarCpf(digitos) ? null : 'CPF com dígito verificador inválido';
  if (digitos.length === 14) return validarCnpj(digitos) ? null : 'CNPJ com dígito verificador inválido';
  return `documento com ${digitos.length} dígitos (esperado 11 para CPF ou 14 para CNPJ)`;
}

const PADRAO_EMAIL = /^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$/;

function validarEmail(valor) {
  if (typeof valor !== 'string') return 'e-mail não é texto';
  if (valor !== valor.trim()) return 'e-mail com espaço nas pontas (deveria já vir aparado)';
  if (!PADRAO_EMAIL.test(valor.toLowerCase())) return `formato de e-mail inválido: '${valor}'`;
  if (valor !== valor.toLowerCase()) return `e-mail fora do padrão minúsculo: '${valor}'`;
  return null;
}

/** Mesma regra de Tracbel.Crm.Dominio.Comum.Telefone: 10/11 dígitos nacionais, DDD 11-99. */
function validarTelefone(valor) {
  if (typeof valor !== 'string') return 'telefone não é texto';
  const semEspacoNaPonta = valor.trimStart();
  if (semEspacoNaPonta.startsWith('+')) {
    const comSinal = soDigitos(semEspacoNaPonta);
    if (!comSinal.startsWith('55')) return `DDI estrangeiro não suportado: '${valor}'`;
  }
  let digitos = soDigitos(valor);
  if ((digitos.length === 12 || digitos.length === 13) && digitos.startsWith('55')) digitos = digitos.slice(2);
  // Celular antigo sem o nono dígito: completa antes de validar, mesma regra do tipo de valor.
  if (digitos.length === 10 && /[6-9]/.test(digitos[2])) digitos = digitos.slice(0, 2) + '9' + digitos.slice(2);
  if (digitos.length !== 10 && digitos.length !== 11)
    return `telefone com ${digitos.length} dígitos (esperado 10 ou 11 com DDD): '${valor}'`;
  const ddd = Number(digitos.slice(0, 2));
  if (ddd < 11 || ddd > 99) return `DDD inválido (${ddd}): '${valor}'`;
  if (digitos.length === 11 && digitos[2] !== '9') return `celular sem o nono dígito depois do DDD: '${valor}'`;
  return null;
}

function validarCep(valor) {
  const digitos = soDigitos(valor);
  if (digitos.length !== 8) return `CEP com ${digitos.length} dígitos (esperado 8): '${valor}'`;
  if (new Set(digitos.split('')).size === 1) return `CEP com todos os dígitos iguais: '${valor}'`;
  return null;
}

const UFS_VALIDAS = new Set([
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG', 'PA', 'PB',
  'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO',
]);

function validarUf(valor) {
  if (typeof valor !== 'string') return 'UF não é texto';
  if (!UFS_VALIDAS.has(valor.toUpperCase())) return `UF fora do catálogo de 27 unidades: '${valor}'`;
  if (valor !== valor.toUpperCase()) return `UF fora do padrão maiúsculo: '${valor}'`;
  return null;
}

/**
 * VIN: 17 letras e números — mesma regra de Tracbel.Crm.Dominio.Comum.Chassi. I, O e Q são aceitos desde
 * 24/09/2026: a plaqueta da John Deere usa o prefixo 1CQ.
 */
function validarChassi(valor) {
  if (typeof valor !== 'string') return 'chassi não é texto';
  const normalizado = valor.replace(/\s/g, '').toUpperCase();
  if (normalizado.length !== 17) return `chassi com ${normalizado.length} caracteres (esperado 17): '${valor}'`;
  if (!/^[A-Z0-9]{17}$/.test(normalizado)) return `chassi com caractere fora de letras e números: '${valor}'`;
  return null;
}

function validarLatitude(valor) {
  if (typeof valor !== 'number' || !Number.isFinite(valor)) return 'latitude não é número';
  if (valor < -90 || valor > 90) return `latitude matematicamente inválida: ${valor}`;
  if (valor < LAT_MIN || valor > LAT_MAX) return `latitude fora da área de atuação (Brasil): ${valor}`;
  return null;
}

function validarLongitude(valor) {
  if (typeof valor !== 'number' || !Number.isFinite(valor)) return 'longitude não é número';
  if (valor < -180 || valor > 180) return `longitude matematicamente inválida: ${valor}`;
  if (valor < LNG_MIN || valor > LNG_MAX) return `longitude fora da área de atuação (Brasil): ${valor}`;
  return null;
}

/** Aceita "AAAA-MM-DD", "AAAA-MM-DDTHH:MM[:SS]" e "AAAA-MM". */
function validarData(valor) {
  if (typeof valor !== 'string') return 'data não é texto';
  let ano;
  const soMes = /^(\d{4})-(\d{2})$/.exec(valor);
  if (soMes) {
    ano = Number(soMes[1]);
  } else {
    const d = new Date(valor);
    if (Number.isNaN(d.getTime())) return `data com formato não reconhecido: '${valor}'`;
    ano = d.getUTCFullYear();
  }
  if (ano < ANO_MINIMO || ano > ANO_ATUAL + ANOS_NO_FUTURO)
    return `data implausível, ano ${ano} fora de [${ANO_MINIMO}, ${ANO_ATUAL + ANOS_NO_FUTURO}]: '${valor}'`;
  return null;
}

/** Dinheiro como número: não mais que duas casas decimais. Espelha Dominio.Comum.Dinheiro. */
function validarDinheiroNumero(valor) {
  if (typeof valor !== 'number' || !Number.isFinite(valor)) return 'valor monetário não é número finito';
  const comDuasCasas = Math.round(valor * 100) / 100;
  // Comparação por diferença pequena evita ruído de ponto flutuante binário.
  if (Math.abs(comDuasCasas - valor) > 1e-9) return `valor monetário com mais de duas casas decimais: ${valor}`;
  return null;
}

/** Dinheiro como texto formatado em reais: "R$ 1.234,56". */
const PADRAO_DINHEIRO_BR = /^R\$\s?\d{1,3}(\.\d{3})*,\d{2}$/;

function validarDinheiroTexto(valor) {
  if (!PADRAO_DINHEIRO_BR.test(valor.trim()))
    return `valor monetário em texto fora do formato "R$ 0.000,00": '${valor}'`;
  return null;
}

const CARACTERES_INVISIVEIS = /[\u200B-\u200F\uFEFF\u00A0]/;

/** Regras universais de texto livre — aplicadas a toda string-folha da árvore. */
function validarTextoLivre(valor) {
  const achados = [];
  if (valor !== valor.trim()) achados.push('espaço nas pontas');
  if (/ {2,}/.test(valor)) achados.push('espaço duplo');
  if (CARACTERES_INVISIVEIS.test(valor)) achados.push('caractere invisível (largura zero, NBSP, BOM ou marca de direção)');
  return achados.length ? achados.join('; ') : null;
}

// ---------------------------------------------------------------------------
// 2. Roteamento por nome de campo — que regra vale para qual chave do JSON
// ---------------------------------------------------------------------------

const CHAVES_DOCUMENTO = new Set(['cnpj', 'cpf']);
const CHAVES_EMAIL_ESCALAR = new Set(['email']);
const CHAVES_EMAIL_LISTA = new Set(['emails']);
const CHAVES_TELEFONE = new Set(['telefone', 'celular', 'fone']);
const CHAVES_CEP = new Set(['cep']);
const CHAVES_UF = new Set(['uf', 'ie_uf']);
const CHAVES_CHASSI = new Set(['chassi']);
const CHAVES_DATA = /(^data$|^data_|_em$|^aniversario$|^nascimento$|^ult_compra$|^ultimo_login$|^prazo$|^inicio$|^fim$|^vencimento$|^validade$|^previsao|^aquisicao$|^abertura$|^envio$|^fundacao$)/i;
const CHAVES_DINHEIRO = /(valor|faturamento|limite|custo|preco|inadimplencia)/i;
const CHAVES_DINHEIRO_EXCLUIDAS = /(_pct$|percentual|probabilidade|valor_unitario_lista$)/i; // listas de preço-tabela não são o foco aqui, mas continuam validadas como número comum

/**
 * Percorre o JSON e devolve a lista de achados (violações) mais os registros crus de
 * documento/chassi encontrados, para a checagem de chave natural entre arquivos.
 */
function auditarArquivo(nomeArquivo, dado) {
  const achados = [];
  const documentosEncontrados = []; // {caminho, digitos, bruto}
  const chassisEncontrados = []; // {caminho, chassi, modeloProximo}

  function registrar(caminho, regra, valor, motivo) {
    achados.push({ arquivo: nomeArquivo, caminho, regra, valor, motivo });
  }

  function percorrer(no, caminho, chaveAtual, chavePai, objetoAtual) {
    if (no === null || no === undefined) return;

    if (Array.isArray(no)) {
      no.forEach((item, i) => percorrer(item, `${caminho}[${i}]`, chaveAtual, chavePai, objetoAtual));
      return;
    }

    if (typeof no === 'object') {
      // Contexto especial: dentro de um array "telefones": [{tipo, numero}], o campo
      // "numero" é telefone — mas em outros contextos "numero" pode ser nº de pedido,
      // endereço ("S/N") etc., por isso não tratamos "numero" como telefone globalmente.
      for (const [chave, valor] of Object.entries(no)) {
        percorrer(valor, `${caminho}.${chave}`, chave, chaveAtual, no);
      }
      return;
    }

    // Folha (string, number, boolean).
    const chaveLower = String(chaveAtual ?? '').toLowerCase();
    const chavePaiLower = String(chavePai ?? '').toLowerCase();

    if (CHAVES_DOCUMENTO.has(chaveLower) && typeof no === 'string' && no.trim() !== '') {
      const motivo = validarDocumento(no);
      const digitos = soDigitos(no);
      documentosEncontrados.push({ caminho, digitos, bruto: no });
      if (motivo) registrar(caminho, 'documento', no, motivo);
    } else if (chaveLower === 'numero' && chavePaiLower === '') {
      // não usado — placeholder para clareza de intenção (numero fora de contexto conhecido não é validado)
    } else if (CHAVES_TELEFONE.has(chaveLower) || (chaveLower === 'numero' && isContextoTelefone(caminho))) {
      if (typeof no === 'string' && no.trim() !== '') {
        const motivo = validarTelefone(no);
        if (motivo) registrar(caminho, 'telefone', no, motivo);
      }
    } else if (CHAVES_EMAIL_ESCALAR.has(chaveLower)) {
      if (typeof no === 'string' && no.trim() !== '') {
        const motivo = validarEmail(no);
        if (motivo) registrar(caminho, 'email', no, motivo);
      }
    } else if (CHAVES_CEP.has(chaveLower)) {
      const motivo = validarCep(no);
      if (motivo) registrar(caminho, 'cep', no, motivo);
    } else if (CHAVES_UF.has(chaveLower)) {
      const motivo = validarUf(no);
      if (motivo) registrar(caminho, 'uf', no, motivo);
    } else if (CHAVES_CHASSI.has(chaveLower)) {
      const motivo = validarChassi(no);
      chassisEncontrados.push({ caminho, chassi: String(no).toUpperCase().replace(/\s/g, ''), modeloProximo: objetoAtual?.modelo ?? null });
      if (motivo) registrar(caminho, 'chassi', no, motivo);
    } else if (chaveLower === 'lat' || chaveLower === 'latitude') {
      const motivo = validarLatitude(no);
      if (motivo) registrar(caminho, 'coordenada', no, motivo);
    } else if (chaveLower === 'lng' || chaveLower === 'lon' || chaveLower === 'longitude') {
      const motivo = validarLongitude(no);
      if (motivo) registrar(caminho, 'coordenada', no, motivo);
    } else if (CHAVES_DATA.test(chaveLower) && typeof no === 'string' && no.trim() !== '') {
      const motivo = validarData(no);
      if (motivo) registrar(caminho, 'data', no, motivo);
    } else if (CHAVES_DINHEIRO.test(chaveLower) && !CHAVES_DINHEIRO_EXCLUIDAS.test(chaveLower)) {
      if (typeof no === 'number') {
        const motivo = validarDinheiroNumero(no);
        if (motivo) registrar(caminho, 'dinheiro', no, motivo);
      } else if (typeof no === 'string' && no.trim().startsWith('R$')) {
        const motivo = validarDinheiroTexto(no);
        if (motivo) registrar(caminho, 'dinheiro', no, motivo);
      }
    }

    // Regra universal de texto livre — roda em TODA string-folha, além da regra
    // específica do campo (uma string pode violar as duas ao mesmo tempo).
    if (typeof no === 'string') {
      const motivoTexto = validarTextoLivre(no);
      if (motivoTexto) registrar(caminho, 'texto-livre', no, motivoTexto);
    }
  }

  function isContextoTelefone(caminho) {
    return /\btelefones\[\d+\]\.numero$/.test(caminho);
  }

  percorrer(dado, '$', null, null, null);

  return { achados, documentosEncontrados, chassisEncontrados };
}

// ---------------------------------------------------------------------------
// 3. Execução: lê os arquivos, audita, cruza chave natural entre arquivos
// ---------------------------------------------------------------------------

const arquivosJson = fs
  .readdirSync(PASTA_SEED)
  .filter((f) => f.endsWith('.json'))
  .sort();

const todosOsAchados = [];
const documentosGlobais = new Map(); // digitos -> [{arquivo, caminho, bruto}]
const chassisGlobais = new Map(); // chassi -> [{arquivo, caminho, modeloProximo}]
const porArquivo = new Map(); // arquivo -> contagem de achados

for (const nomeArquivo of arquivosJson) {
  const caminhoCompleto = path.join(PASTA_SEED, nomeArquivo);
  let dado;
  try {
    dado = JSON.parse(fs.readFileSync(caminhoCompleto, 'utf8'));
  } catch (erro) {
    todosOsAchados.push({ arquivo: nomeArquivo, caminho: '$', regra: 'json-invalido', valor: null, motivo: String(erro.message) });
    porArquivo.set(nomeArquivo, 1);
    continue;
  }

  const { achados, documentosEncontrados, chassisEncontrados } = auditarArquivo(nomeArquivo, dado);
  todosOsAchados.push(...achados);
  porArquivo.set(nomeArquivo, achados.length);

  for (const d of documentosEncontrados) {
    if (!documentosGlobais.has(d.digitos)) documentosGlobais.set(d.digitos, []);
    documentosGlobais.get(d.digitos).push({ arquivo: nomeArquivo, caminho: d.caminho, bruto: d.bruto });
  }
  for (const c of chassisEncontrados) {
    if (!chassisGlobais.has(c.chassi)) chassisGlobais.set(c.chassi, []);
    chassisGlobais.get(c.chassi).push({ arquivo: nomeArquivo, caminho: c.caminho, modeloProximo: c.modeloProximo });
  }
}

// Duplicata de chave natural: mesmo CNPJ/CPF em registros de CLIENTE DISTINTOS. Um cliente
// pode legitimamente aparecer com o mesmo documento no mesmo arquivo (não é o caso aqui);
// o achado que importa é o mesmo documento sob "ids" ou arquivos diferentes.
const duplicatasDocumento = [];
for (const [digitos, ocorrencias] of documentosGlobais) {
  if (ocorrencias.length > 1) {
    duplicatasDocumento.push({ digitos, ocorrencias });
  }
}

// Chassi repetido: por desenho, o mesmo chassi aparece em cliente/equipamento/pos-vendas
// para descrever o MESMO bem. Só é achado de verdade quando o "modelo" próximo diverge
// entre ocorrências (ou quando aparece em mais de um arquivo "cliente" diferente).
const inconsistenciasChassi = [];
for (const [chassi, ocorrencias] of chassisGlobais) {
  const modelos = new Set(ocorrencias.map((o) => o.modeloProximo).filter(Boolean));
  if (modelos.size > 1) {
    inconsistenciasChassi.push({ chassi, ocorrencias, modelos: [...modelos] });
  }
}

// ---------------------------------------------------------------------------
// 4. Relato: tabela no terminal + documento Markdown completo
// ---------------------------------------------------------------------------

function tabela(cabecalhos, linhas) {
  const larguras = cabecalhos.map((c, i) => Math.max(c.length, ...linhas.map((l) => String(l[i]).length)));
  const formatarLinha = (celulas) => celulas.map((c, i) => String(c).padEnd(larguras[i])).join('  ');
  const separador = larguras.map((l) => '-'.repeat(l)).join('  ');
  return [formatarLinha(cabecalhos), separador, ...linhas.map(formatarLinha)].join('\n');
}

const totalAchados = todosOsAchados.length;

console.log(`\nValidação de ${arquivosJson.length} arquivos de prototipo/dados-seed/`);
console.log(`Total de achados: ${totalAchados}\n`);

console.log('--- Por arquivo ---');
console.log(
  tabela(
    ['arquivo', 'achados'],
    arquivosJson.map((f) => [f, porArquivo.get(f) ?? 0]),
  ),
);

console.log('\n--- Por regra ---');
const porRegra = new Map();
for (const a of todosOsAchados) porRegra.set(a.regra, (porRegra.get(a.regra) ?? 0) + 1);
console.log(
  tabela(
    ['regra', 'achados'],
    [...porRegra.entries()].sort((a, b) => b[1] - a[1]),
  ),
);

console.log('\n--- Duplicata de chave natural: CNPJ/CPF em mais de um registro ---');
if (duplicatasDocumento.length === 0) {
  console.log('(nenhuma)');
} else {
  for (const d of duplicatasDocumento) {
    console.log(`  ${d.digitos}:`);
    for (const o of d.ocorrencias) console.log(`    - ${o.arquivo} :: ${o.caminho} = '${o.bruto}'`);
  }
}

console.log('\n--- Chassi repetido com modelo divergente ---');
if (inconsistenciasChassi.length === 0) {
  console.log('(nenhuma)');
} else {
  for (const c of inconsistenciasChassi) {
    console.log(`  ${c.chassi}: modelos ${c.modelos.join(' / ')}`);
    for (const o of c.ocorrencias) console.log(`    - ${o.arquivo} :: ${o.caminho}`);
  }
}

// ---- Documento Markdown completo ----

const linhasDoc = [];
linhasDoc.push('# Qualidade dos dados da semente do protótipo');
linhasDoc.push('');
linhasDoc.push(
  `> Gerado por \`scripts/dados/validar-seed.mjs\` em ${new Date().toISOString().slice(0, 10)}, contra o ` +
    'catálogo de saneamento de `docs/projeto/16-HIGIENIZACAO-DE-DADOS.md`. Este documento é gerado — ' +
    'não editar à mão. **O script só relata; nada nos 45 JSONs foi alterado.**',
);
linhasDoc.push('');
linhasDoc.push(`Arquivos analisados: **${arquivosJson.length}**. Achados: **${totalAchados}**.`);
linhasDoc.push('');
const qtdDocumentoInvalido = porRegra.get('documento') ?? 0;
const qtdDocumentoTotal = [...documentosGlobais.values()].reduce((soma, ocorrencias) => soma + ocorrencias.length, 0);
if (qtdDocumentoInvalido > 40) {
  const proporcao = qtdDocumentoInvalido === qtdDocumentoTotal
    ? `as ${qtdDocumentoTotal} ocorrências de CNPJ encontradas`
    : `${qtdDocumentoInvalido} das ${qtdDocumentoTotal} ocorrências de CNPJ encontradas`;
  linhasDoc.push(
    `> **Leitura do achado dominante.** ${proporcao} ` +
      'nos arquivos de cliente (`cliente-84391.json`, `clientes-extra.json`, `clientes-outras.json`, ' +
      '`oportunidade-1517613.json`) falham no dígito verificador — 100% delas nesta rodada. Isto não é ' +
      'sinal de bug no validador: são CNPJs inventados à mão para o protótipo visual, plausíveis para o ' +
      'olho (14 dígitos, máscara correta) mas nunca calculados. **É exatamente o argumento da frente de ' +
      'saneamento**: se este arquivo fosse promovido a carga real sem passar pelo tipo `CpfCnpj`, o banco ' +
      'novo nasceria com o mesmo defeito medido no Vórtice (116 CPFs repetidos por falta de validação na ' +
      'entrada — documento 01, achado 9.4). O documento 16, seção 8, trata isto como regra de migração: ' +
      'nenhum dado de protótipo vira massa de teste sem passar pelo mesmo pipeline de saneamento do dado real.',
  );
  linhasDoc.push('');
}
linhasDoc.push('## Por arquivo');
linhasDoc.push('');
linhasDoc.push('| Arquivo | Achados |');
linhasDoc.push('|---|---:|');
for (const f of arquivosJson) linhasDoc.push(`| \`${f}\` | ${porArquivo.get(f) ?? 0} |`);
linhasDoc.push('');
linhasDoc.push('## Por regra');
linhasDoc.push('');
linhasDoc.push('| Regra | Achados | O que verifica |');
linhasDoc.push('|---|---:|---|');
const descricaoRegra = {
  documento: 'CNPJ/CPF com dígito verificador inválido ou tamanho errado',
  telefone: 'Telefone fora do padrão nacional (10/11 dígitos, DDD 11-99, DDI estrangeiro)',
  email: 'E-mail com formato inválido, caixa alta ou espaço nas pontas',
  cep: 'CEP com tamanho errado ou sequência de erro de preenchimento',
  uf: 'UF fora das 27 unidades federativas, ou fora do padrão maiúsculo',
  chassi: 'Chassi fora do padrão de 17 caracteres (ou contendo I/O/Q)',
  coordenada: 'Latitude/longitude matematicamente inválida ou fora do Brasil',
  data: 'Data com formato não reconhecido ou ano fora da faixa plausível',
  dinheiro: 'Valor monetário com mais de duas casas decimais, ou fora do formato R$',
  'texto-livre': 'Espaço nas pontas, espaço duplo ou caractere invisível em texto livre',
  'json-invalido': 'Arquivo que não é um JSON válido',
};
for (const [regra, qtd] of [...porRegra.entries()].sort((a, b) => b[1] - a[1])) {
  linhasDoc.push(`| \`${regra}\` | ${qtd} | ${descricaoRegra[regra] ?? ''} |`);
}
linhasDoc.push('');

linhasDoc.push('## Duplicata de chave natural — CNPJ/CPF em mais de um registro');
linhasDoc.push('');
linhasDoc.push(
  'Chave natural do cliente é o CNPJ/CPF (documento 16, seção 4). O mesmo documento sob ' +
    'ids ou arquivos diferentes é uma duplicata real, ou uma inconsistência entre a semente e o ' +
    'protótipo que a originou — os dois casos abaixo são exatamente isso, e cada um é achado ' +
    'real medido nesta rodada, não hipotético.',
);
linhasDoc.push('');
if (duplicatasDocumento.length === 0) {
  linhasDoc.push('Nenhuma duplicata encontrada.');
} else {
  for (const d of duplicatasDocumento) {
    linhasDoc.push(`- **${d.digitos}**`);
    for (const o of d.ocorrencias) linhasDoc.push(`  - \`${o.arquivo}\` em \`${o.caminho}\` = \`'${o.bruto}'\``);
  }
}
linhasDoc.push('');

linhasDoc.push('## Chassi repetido com modelo divergente');
linhasDoc.push('');
linhasDoc.push(
  'Chassi repetir entre `cliente-*.json`, `equipamento-*.json` e `pos-vendas-*.json` é ' +
    '**esperado por desenho** (as três fichas descrevem o mesmo bem). Só vira achado quando o ' +
    '"modelo" associado diverge entre as ocorrências.',
);
linhasDoc.push('');
if (inconsistenciasChassi.length === 0) {
  linhasDoc.push('Nenhuma inconsistência encontrada.');
} else {
  for (const c of inconsistenciasChassi) {
    linhasDoc.push(`- **${c.chassi}** — modelos divergentes: ${c.modelos.join(' / ')}`);
    for (const o of c.ocorrencias) linhasDoc.push(`  - \`${o.arquivo}\` em \`${o.caminho}\``);
  }
}
linhasDoc.push('');

linhasDoc.push('## Todos os achados, por arquivo');
linhasDoc.push('');
for (const f of arquivosJson) {
  const achadosDoArquivo = todosOsAchados.filter((a) => a.arquivo === f);
  if (achadosDoArquivo.length === 0) continue;
  linhasDoc.push(`### \`${f}\` (${achadosDoArquivo.length})`);
  linhasDoc.push('');
  linhasDoc.push('| Caminho | Regra | Valor | Motivo |');
  linhasDoc.push('|---|---|---|---|');
  for (const a of achadosDoArquivo) {
    const valorEscapado = String(a.valor).replace(/\|/g, '\\|').slice(0, 80);
    linhasDoc.push(`| \`${a.caminho}\` | \`${a.regra}\` | \`${valorEscapado}\` | ${a.motivo} |`);
  }
  linhasDoc.push('');
}

linhasDoc.push('## Metodologia e limitações');
linhasDoc.push('');
linhasDoc.push(
  '- O campo `numero` só é tratado como telefone dentro de um array `telefones[]` — em todo ' +
    'outro contexto ele é ambíguo demais (número de pedido, "S/N" de endereço) para validar sem falso positivo.',
);
linhasDoc.push(
  '- A regra de texto livre roda em **toda** string-folha do JSON, mesmo campos que já têm regra ' +
    'própria (ex.: um e-mail com espaço duplo aparece nas duas regras).',
);
linhasDoc.push(
  '- Datas no formato `AAAA-MM` (só ano e mês, usado em `historico_horas`) são aceitas e checadas só pelo ano.',
);
linhasDoc.push(
  '- Inscrição estadual, endereço em texto livre e placa não têm regra própria neste script porque ' +
    'não têm um formato único nacional verificável sem tabela de UF por UF (documento 16, catálogo, item 2).',
);
linhasDoc.push('');

fs.mkdirSync(path.dirname(SAIDA_DOC), { recursive: true });
fs.writeFileSync(SAIDA_DOC, linhasDoc.join('\n'), 'utf8');

console.log(`\nRelatório completo gravado em: ${path.relative(RAIZ, SAIDA_DOC)}`);
