/**
 * Persistência das oportunidades criadas na tela Nova Oportunidade — porte do
 * bloco de `localStorage` do protótipo de referência
 * (`prototipo/referencia/assets/app.js`, linhas 63-146). Mesmas chaves e o
 * mesmo tratamento defensivo em try/catch: o acesso pode ser bloqueado em
 * ambiente sandbox (preview isolado), então falha silenciosamente e cai de
 * volta para o estado em memória.
 *
 * Mantém as oportunidades da sessão em memória (equivalente a
 * `window.OPORTUNIDADES_NOVAS`) e espelha no `localStorage` para resistir a
 * F5. A tela de Pipeline (outra frente) lê estas mesmas chaves para injetar
 * as oportunidades novas no quadro kanban.
 */
import type { NovaOportunidadeEstado, OportunidadeNova } from '../tipos/oportunidade';

const CHAVE_NOVAS = 'crm-tracbel:oportunidades-novas:v1';
const CHAVE_CONTADOR = 'crm-tracbel:contador-op:v1';
const CONTADOR_INICIAL = 8601;

let oportunidades: OportunidadeNova[] = [];
let contadorOportId = CONTADOR_INICIAL;
let carregadoDoStorage = false;

/** Wrapper defensivo: só retorna o Storage se leitura/escrita realmente funcionar. */
function obterStorage(): Storage | null {
  try {
    const s = window.localStorage;
    const chaveTeste = '__crm_test__';
    s.setItem(chaveTeste, '1');
    s.removeItem(chaveTeste);
    return s;
  } catch {
    return null;
  }
}

function persistir(): void {
  const s = obterStorage();
  if (!s) return;
  try {
    s.setItem(CHAVE_NOVAS, JSON.stringify(oportunidades));
    s.setItem(CHAVE_CONTADOR, String(contadorOportId));
  } catch (erro) {
    console.warn('[Nova Oport] falha ao salvar:', erro);
  }
}

function garantirCarregado(): void {
  if (carregadoDoStorage) return;
  carregadoDoStorage = true;
  const s = obterStorage();
  if (!s) return;
  try {
    const bruto = s.getItem(CHAVE_NOVAS);
    if (bruto) {
      const arr = JSON.parse(bruto) as unknown;
      if (Array.isArray(arr)) {
        // Zera o flag "criada_agora" no reload (evita destaque piscando eternamente)
        arr.forEach((op) => {
          (op as OportunidadeNova).criada_agora = false;
        });
        oportunidades = arr as OportunidadeNova[];
      }
    }
    const contadorSalvo = s.getItem(CHAVE_CONTADOR);
    if (contadorSalvo) contadorOportId = parseInt(contadorSalvo, 10);
  } catch (erro) {
    console.warn('[Nova Oport] falha ao carregar:', erro);
  }
}

/** true se o navegador permite gravar no localStorage (falso em sandbox). */
export function storageDisponivel(): boolean {
  return obterStorage() !== null;
}

/** Oportunidades criadas nesta sessão (recuperadas do localStorage se houver). */
export function obterOportunidadesNovas(): OportunidadeNova[] {
  garantirCarregado();
  return oportunidades;
}

/**
 * Cria e persiste uma oportunidade a partir do estado do formulário. Espelha
 * `salvarNovaOportunidade()`, inclusive o formato exato do objeto salvo e o
 * contador de id (começa em 8601, formato `OP-2026-0<n>`).
 */
export function criarOportunidadeNova(estado: NovaOportunidadeEstado): OportunidadeNova {
  garantirCarregado();
  const numOp = contadorOportId++;
  const modeloExibicao = estado.quantidade > 1 ? `${estado.modelo} × ${estado.quantidade}` : estado.modelo;
  const valorTotal = estado.quantidade * estado.valor_unitario;

  const nova: OportunidadeNova = {
    id: `OP-2026-0${numOp}`,
    titulo: estado.titulo,
    cliente: estado.cliente_nome,
    cliente_id: estado.cliente_id,
    classe: estado.cliente_classe,
    cidade: estado.cliente_cidade,
    cen: 'joao', // sempre atribuído ao CEN logado
    linha: estado.linha,
    modelo: modeloExibicao,
    valor: valorTotal,
    probabilidade: estado.probabilidade,
    previsao: estado.previsao_fechamento,
    dias_fase: 0,
    fase: estado.fase_inicial,
    observacao: estado.observacao,
    criada_agora: true, // flag visual para destacar no Pipeline
    criada_em: new Date().toISOString(),
    prospect_novo: estado.cliente_novo,
  };

  oportunidades = [...oportunidades, nova];
  persistir();
  return nova;
}

/** Remove todas as oportunidades da sessão (memória + localStorage). */
export function limparOportunidadesPersistidas(): void {
  garantirCarregado();
  oportunidades = [];
  contadorOportId = CONTADOR_INICIAL;
  const s = obterStorage();
  if (!s) return;
  try {
    s.removeItem(CHAVE_NOVAS);
    s.removeItem(CHAVE_CONTADOR);
  } catch (erro) {
    console.warn('[Nova Oport] falha ao limpar:', erro);
  }
}
