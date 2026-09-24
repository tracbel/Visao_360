/**
 * COMO OS NÚMEROS DO MOMENTO SE ESCREVEM — um lugar só para as cinco abas.
 *
 * As abas do bloco foram escritas em épocas diferentes, e cada uma tinha o seu
 * `reais()`: uma arredondava para inteiro, outra mostrava centavos, a terceira
 * encurtava para "mi". Na maquete elas convivem na mesma tela — "R$ 14.085,13"
 * no ranking, "R$ 186,4 mi" no cartão do crédito —, e cada forma tem o seu
 * lugar. O que não pode é a mesma grandeza sair de dois jeitos em duas abas.
 */

/** Reais com centavos — a margem por hectare da maquete é "R$ 14.085,13". */
export function reais(valor: number, casas = 2): string {
  return valor.toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
    minimumFractionDigits: casas,
    maximumFractionDigits: casas,
  });
}

/**
 * Reais encurtados para o cartão — "R$ 186,4 mi", "R$ 582 mil".
 *
 * É a forma do cartão e do ranking, onde a ordem de grandeza é a leitura. A
 * tabela do detalhamento mostra o número inteiro, com centavos, para quem
 * confere.
 */
export function reaisCurtos(valor: number): string {
  const abs = Math.abs(valor);
  if (abs >= 1e9) return `R$ ${(valor / 1e9).toLocaleString('pt-BR', { maximumFractionDigits: 2 })} bi`;
  if (abs >= 1e6) return `R$ ${(valor / 1e6).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} mi`;
  if (abs >= 1e3) return `R$ ${(valor / 1e3).toLocaleString('pt-BR', { maximumFractionDigits: 0 })} mil`;
  return reais(valor, 0);
}

/** Inteiro ou com casas fixas, no padrão brasileiro. */
export function numero(valor: number, casas = 0): string {
  return valor.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });
}

/**
 * Uma FRAÇÃO como percentual com sinal — 0,12 vira "+12%".
 *
 * O SINAL É ESCRITO SEMPRE, e não só no negativo: "12%" ao lado de "-8%" deixa
 * o leitor sem saber se o primeiro é alta ou nível. É o sinal, e não a cor, que
 * carrega o sentido — a tela continua legível em escala de cinza.
 */
export function percentualComSinal(fracao: number, casas = 0): string {
  const arredondado = Number((fracao * 100).toFixed(casas));
  // "-0%" é ruído de arredondamento, não queda: zero sai sem sinal.
  if (arredondado === 0) return `${numero(0, casas)}%`;
  return `${arredondado > 0 ? '+' : ''}${numero(arredondado, casas)}%`;
}

/** Pontos percentuais com sinal — a percepção do gestor é "+2 p.p.", e não "+2%". */
export function pontosPercentuais(valor: number, casas = 1): string {
  const texto = valor.toLocaleString('pt-BR', { minimumFractionDigits: 0, maximumFractionDigits: casas });
  return `${valor > 0 ? '+' : ''}${texto} p.p.`;
}

/**
 * O SENTIDO DE UMA VARIAÇÃO — ↑, → ou ↓.
 *
 * É a mesma régua da composição do fator desde a T3.1: perto de zero é "→",
 * porque uma parcela de 0,001 não empurra nada, e desenhar ↑ ali sugeriria um
 * movimento que não existe. NÃO HÁ ADJETIVO AQUI: "favorável", "estável" e
 * "restritivo" pediriam limites que ninguém registrou. O que existe é a seta.
 */
export function sentido(valor: number | null, limiar = 0.005): '↑' | '→' | '↓' | null {
  if (valor === null) return null;
  if (valor > limiar) return '↑';
  if (valor < -limiar) return '↓';
  return '→';
}

/** A classe de cor de um sentido — reforço, nunca o único sinal. */
export function tomDoSentido(valor: number | null, limiar = 0.005): 'alta' | 'baixa' | 'neutro' {
  const s = sentido(valor, limiar);
  return s === '↑' ? 'alta' : s === '↓' ? 'baixa' : 'neutro';
}

/** `aaaa-mm-01` → "jan/24". */
export function mesCurto(iso: string): string {
  const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
  const [ano, mes] = iso.split('-').map(Number);
  return `${MESES[mes - 1]}/${String(ano).slice(2)}`;
}

/** `aaaa-mm-dd` → "21/09/2026". */
export function dataCurta(iso: string): string {
  const [ano, mes, dia] = iso.slice(0, 10).split('-');
  return `${dia}/${mes}/${ano}`;
}
