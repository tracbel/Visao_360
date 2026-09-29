/**
 * O funil de vendas em SVG e a conversão entre estágios — no desenho da maquete do Ricardo de 29/09/2026 (a imagem
 * `docs/prototipo/capturas-referencia/funil-de-vendas-maquete-2026-09-29.png`).
 *
 * O FUNIL É UMA SILHUETA FIXA, EM PERSPECTIVA: a boca elíptica em cima, as seis faixas com o fundo curvo e o brilho da
 * esquerda para a direita, e cada estágio ligado ao nome dele por uma linha-guia. A maquete desenha a forma, e não a
 * proporção: a largura de cada faixa é a da silhueta, e o número de cada estágio está escrito dentro dela — e, com a
 * conversão, no painel ao lado e na tabela embaixo.
 *
 * A CONVERSÃO É A DA LEGENDA ANTIGA, regra por regra: entre estágios vizinhos; travessão no primeiro; travessão abaixo
 * de {@link BASE_MINIMA_DE_CONVERSAO} processos no estágio anterior; `↑` acima de 100%; e a cor por faixa (70% ou
 * mais, verde; de 40% a 70%, laranja; abaixo, vermelho). O que mudou foi o desenho: a barra da conversão, como na
 * maquete, e o motivo do travessão na dica, e não num `title`.
 */

import { ValorAusente } from './comum/ValorAusente';

/** Uma faixa do funil. `valor` é o que a faixa mede na métrica escolhida. */
export type FaixaDoFunil = {
  chave: string;
  rotulo: string;
  valor: number;
  cor: string;
  /** O que aparece dentro da faixa. Deixa a formatação com quem chama. */
  texto: string;
};

/**
 * Abaixo de quantos processos no estágio anterior a conversão deixa de ser exibida.
 *
 * Não é um número mágico: é o ponto em que uma razão passa a dizer mais sobre o acaso do que sobre o funil. Com base
 * 1, qualquer estágio seguinte vira um percentual de quatro ou cinco dígitos.
 */
const BASE_MINIMA_DE_CONVERSAO = 10;

/**
 * As cores dos seis estágios, amostradas da maquete — azul, azul-claro, turquesa, amarelo, laranja e vermelho. A cor diz
 * a POSIÇÃO no funil; o tom claro e o escuro de cada uma fazem o volume.
 */
export const CORES_DO_FUNIL = ['#2672F7', '#07A8F1', '#01BFBE', '#FFC81F', '#FF850C', '#D8141E'] as const;

const CLARAS: Record<string, string> = {
  '#2672F7': '#4A8DFF',
  '#07A8F1': '#2CC8FD',
  '#01BFBE': '#1FD6CF',
  '#FFC81F': '#FFD84A',
  '#FF850C': '#FFA13A',
  '#D8141E': '#EE3A3F',
};

const ESCURAS: Record<string, string> = {
  '#2672F7': '#0D56DD',
  '#07A8F1': '#0590D6',
  '#01BFBE': '#02A3A5',
  '#FFC81F': '#F4B40C',
  '#FF850C': '#F26D05',
  '#D8141E': '#B80C15',
};

/** Sobre estas duas o número branco não se lê; vai o azul-marinho do título (maquete). */
const CORES_CLARAS = new Set<string>(['#FFC81F', '#FF850C']);

/**
 * A cor da posição `i`, espalhando a paleta sobre o número de estágios — sem nunca voltar ao começo: a cor diz a
 * posição no funil, e um funil que volta ao azul no meio deixa de dizer.
 */
export function corDaFaixa(i: number, total: number): string {
  const passo = total <= 1 ? 0 : (CORES_DO_FUNIL.length - 1) / (total - 1);
  return CORES_DO_FUNIL[Math.round(i * passo)];
}

/* ---------------------------------------------------------------------------------------------------------------- */
/* A silhueta                                                                                                         */
/* ---------------------------------------------------------------------------------------------------------------- */

const LARGURA = 520;
const ALTURA = 250;
const CENTRO = 186;
const TOPO = 16;
const FUNDO = 244;
const MEIA_BOCA = 172;
const MEIA_PONTA = 40;
/** A altura relativa de cada faixa, medida na maquete: a de cima leva a boca. */
const PESOS_DAS_FAIXAS = [55, 41, 41, 35, 31, 32];
const X_DA_LEGENDA = 394;

/** A meia largura da silhueta na altura `y` — boca larga que estreita rápido e afina devagar até a ponta. */
function meiaLargura(y: number): number {
  const t = Math.min(1, Math.max(0, (y - TOPO) / (FUNDO - TOPO)));
  return MEIA_PONTA + (MEIA_BOCA - MEIA_PONTA) * Math.pow(1 - t, 1.25);
}

/** O quanto a borda de baixo de uma faixa se curva, proporcional à largura dela. */
const curva = (meia: number) => Math.max(3, meia * 0.075);

function limitesDasFaixas(n: number): number[] {
  const pesos = n === PESOS_DAS_FAIXAS.length ? PESOS_DAS_FAIXAS : Array.from({ length: n }, () => 1);
  const soma = pesos.reduce((a, b) => a + b, 0);
  const ys = [TOPO];
  for (const p of pesos) ys.push(ys[ys.length - 1] + ((FUNDO - TOPO) * p) / soma);
  return ys;
}

/** A faixa: lados que seguem a silhueta, borda de baixo curva, e a de cima curva também, escondida pela anterior. */
function caminhoDaFaixa(y0: number, y1: number): string {
  const passos = 6;
  const esquerda: string[] = [];
  const direita: string[] = [];
  for (let k = 0; k <= passos; k++) {
    const y = y0 + ((y1 - y0) * k) / passos;
    const m = meiaLargura(y);
    esquerda.push(`${(CENTRO - m).toFixed(2)} ${y.toFixed(2)}`);
    direita.unshift(`${(CENTRO + m).toFixed(2)} ${y.toFixed(2)}`);
  }
  const m0 = meiaLargura(y0);
  const m1 = meiaLargura(y1);
  return (
    `M ${esquerda[0]} L ${esquerda.slice(1).join(' L ')} ` +
    `A ${m1.toFixed(2)} ${curva(m1).toFixed(2)} 0 0 0 ${(CENTRO + m1).toFixed(2)} ${y1.toFixed(2)} ` +
    `L ${direita.slice(1).join(' L ')} ` +
    `A ${m0.toFixed(2)} ${curva(m0).toFixed(2)} 0 0 1 ${(CENTRO - m0).toFixed(2)} ${y0.toFixed(2)} Z`
  );
}

/**
 * O funil em perspectiva, com a legenda de linhas-guia à direita.
 *
 * O `id="funil-svg"` e o `aria-label` com cada estágio e o número dele continuam: o leitor de tela lê o funil inteiro
 * numa frase, e a tabela embaixo tem os mesmos números em linhas.
 */
export function GraficoFunil({ faixas }: { faixas: FaixaDoFunil[] }) {
  if (faixas.length === 0) return null;

  const ys = limitesDasFaixas(faixas.length);

  return (
    <svg
      id="funil-svg"
      className="funil-3d"
      viewBox={`0 0 ${LARGURA} ${ALTURA}`}
      role="img"
      aria-label={`Funil de vendas por estágio: ${faixas.map((f) => `${f.rotulo}, ${f.texto}`).join('; ')}`}
    >
      <defs>
        {faixas.map((f) => (
          <linearGradient key={f.chave} id={`funil-grad-${f.chave}`} x1="0" y1="0" x2="1" y2="0">
            <stop offset="0" stopColor={CLARAS[f.cor] ?? f.cor} />
            <stop offset="0.45" stopColor={f.cor} />
            <stop offset="1" stopColor={ESCURAS[f.cor] ?? f.cor} />
          </linearGradient>
        ))}
        <linearGradient id="funil-grad-boca" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor="#7DB5FF" />
          <stop offset="1" stopColor="#2F74F4" />
        </linearGradient>
        <radialGradient id="funil-sombra" cx="0.5" cy="0.5" r="0.5">
          <stop offset="0" stopColor="#0B3B8C" stopOpacity="0.16" />
          <stop offset="1" stopColor="#0B3B8C" stopOpacity="0" />
        </radialGradient>
      </defs>

      {/* A SOMBRA NO CHÃO E O HALO, bem leves — o que dá ao funil o volume da maquete. */}
      <ellipse cx={CENTRO} cy={FUNDO + 2} rx={MEIA_PONTA * 1.9} ry={7} fill="url(#funil-sombra)" />

      {faixas.map((faixa, i) => {
        const y0 = ys[i];
        const y1 = ys[i + 1];
        const meio = (y0 + y1) / 2 + curva(meiaLargura(y1)) * 0.55;
        const direitaDoMeio = CENTRO + meiaLargura((y0 + y1) / 2);
        const clara = CORES_CLARAS.has(faixa.cor);
        const tamanho = i < 3 ? 16 : 14.5;

        return (
          <g key={faixa.chave} className="funil-3d-faixa" data-stage={faixa.chave}>
            <path d={caminhoDaFaixa(y0, y1)} fill={`url(#funil-grad-${faixa.chave})`} />
            {/* O FIO CLARO na junta de uma faixa com a de baixo, como na maquete. */}
            <path
              d={`M ${CENTRO - meiaLargura(y1)} ${y1} A ${meiaLargura(y1)} ${curva(meiaLargura(y1))} 0 0 0 ${CENTRO + meiaLargura(y1)} ${y1}`}
              fill="none"
              stroke="rgba(255,255,255,0.55)"
              strokeWidth={1}
            />
            <text
              x={CENTRO}
              y={meio + tamanho / 3}
              textAnchor="middle"
              fill={clara ? '#0B1638' : '#fff'}
              fontSize={tamanho}
              fontWeight={700}
              fontFamily="Inter, sans-serif"
              style={{ pointerEvents: 'none' }}
            >
              {faixa.texto}
            </text>

            {/* A LINHA-GUIA vai da borda da faixa até o nome do estágio, na cor dele. */}
            <line
              x1={direitaDoMeio + 3}
              y1={(y0 + y1) / 2}
              x2={X_DA_LEGENDA - 10}
              y2={(y0 + y1) / 2}
              stroke={faixa.cor}
              strokeOpacity={0.55}
              strokeWidth={1}
            />
            <circle cx={X_DA_LEGENDA} cy={(y0 + y1) / 2} r={4.5} fill={faixa.cor} />
            <text
              x={X_DA_LEGENDA + 14}
              y={(y0 + y1) / 2 + 4.5}
              fill="currentColor"
              fontSize={13}
              fontWeight={500}
              fontFamily="Inter, sans-serif"
            >
              {faixa.rotulo}
            </text>
          </g>
        );
      })}

      {/* A BOCA: a elipse de cima, com o lado de dentro mais claro. Vem por último para cobrir o topo da primeira faixa. */}
      <ellipse cx={CENTRO} cy={TOPO} rx={MEIA_BOCA} ry={curva(MEIA_BOCA) + 2} fill="url(#funil-grad-boca)" />
      <ellipse
        cx={CENTRO}
        cy={TOPO}
        rx={MEIA_BOCA}
        ry={curva(MEIA_BOCA) + 2}
        fill="none"
        stroke="rgba(255,255,255,0.7)"
        strokeWidth={1}
      />
    </svg>
  );
}

/* ---------------------------------------------------------------------------------------------------------------- */
/* A conversão entre estágios                                                                                         */
/* ---------------------------------------------------------------------------------------------------------------- */

/** A conversão de um estágio: o percentual, ou por que ele não aparece. */
export type ConversaoDoEstagio =
  | { tipo: 'primeiro' }
  | { tipo: 'baseInsuficiente'; anterior: number }
  | { tipo: 'cresceu'; percentual: number }
  | { tipo: 'taxa'; percentual: number };

/** A conversão de cada faixa sobre a vizinha de cima — a mesma conta da legenda antiga. */
export function conversoesDoFunil(faixas: FaixaDoFunil[]): ConversaoDoEstagio[] {
  return faixas.map((faixa, i) => {
    if (i === 0) return { tipo: 'primeiro' };
    const anterior = faixas[i - 1].valor;

    // UMA TAXA SOBRE UMA BASE MINÚSCULA NÃO É UMA TAXA: com 1 processo no estágio anterior e 14.307 no seguinte, a conta
    // dá "↑ 1.430.700%", aritmeticamente certo e sem informação nenhuma.
    if (anterior < BASE_MINIMA_DE_CONVERSAO) return { tipo: 'baseInsuficiente', anterior };

    const percentual = (faixa.valor / anterior) * 100;
    return percentual > 100 ? { tipo: 'cresceu', percentual } : { tipo: 'taxa', percentual };
  });
}

/** A faixa de cor da conversão: 70% ou mais, verde; de 40% a 70%, laranja; abaixo de 40%, vermelho. */
export function tomDaConversao(percentual: number): 'bom' | 'medio' | 'baixo' {
  return percentual >= 70 ? 'bom' : percentual >= 40 ? 'medio' : 'baixo';
}

/**
 * A CONVERSÃO ENTRE ESTÁGIOS, como na maquete: o estágio com a cor dele, o número, a barra da conversão e o percentual
 * colorido. Sem funil, os seis estágios ficam — com o traço e o motivo — para a tabela não parecer um funil vazio.
 */
export function ConversaoDoFunil({
  estagios,
  faixas,
  motivo,
}: {
  /** Os seis estágios, na ordem, para a forma não sumir quando não há dado. */
  estagios: readonly { estagio: string; nome: string }[];
  faixas: FaixaDoFunil[];
  /** Por que não há funil, quando não há. */
  motivo: string | null;
}) {
  const porChave = new Map(faixas.map((f) => [f.chave, f]));
  const conversoes = new Map(conversoesDoFunil(faixas).map((c, i) => [faixas[i].chave, c]));

  return (
    <table className="funil-conversao">
      <caption className="cad-so-leitor">A conversão entre estágios vizinhos</caption>
      <colgroup>
        <col className="funil-col-nome" />
        <col className="funil-col-num" />
        <col className="funil-col-barra" />
        <col className="funil-col-pct" />
      </colgroup>
      <thead>
        <tr>
          <th scope="col">Estágio</th>
          <th scope="col" className="funil-conversao-num">
            <span className="cad-so-leitor">Processos</span>
          </th>
          {/* O NOME DA CONVERSÃO COBRE A BARRA E O PERCENTUAL, como na maquete: a barra é a conversão desenhada. */}
          <th scope="col" colSpan={2} className="funil-conversao-pct">
            Conv. sobre o anterior
          </th>
        </tr>
      </thead>
      <tbody>
        {estagios.map(({ estagio, nome }, i) => {
          const faixa = porChave.get(estagio);
          const cor = faixa?.cor ?? corDaFaixa(i, estagios.length);
          const conversao = conversoes.get(estagio);

          return (
            <tr key={estagio}>
              <th scope="row">
                <span className="funil-ponto" style={{ background: cor }} aria-hidden="true" />
                {nome}
              </th>
              <td className="funil-conversao-num">
                {faixa ? faixa.texto : <ValorAusente motivo={motivo ?? 'sem dado'} oQue={`o estágio ${nome}`} />}
              </td>
              <td className="funil-conversao-barra-col">
                {conversao && (conversao.tipo === 'taxa' || conversao.tipo === 'cresceu') && (
                  <span className="funil-conversao-barra" aria-hidden="true">
                    <span style={{ width: `${Math.min(100, conversao.percentual)}%`, background: cor }} />
                  </span>
                )}
              </td>
              <td className="funil-conversao-pct">
                <PercentualDaConversao conversao={conversao} temFunil={Boolean(faixa)} />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}

function PercentualDaConversao({ conversao, temFunil }: { conversao: ConversaoDoEstagio | undefined; temFunil: boolean }) {
  if (!temFunil || !conversao) return <span className="funil-conversao-traco">—</span>;

  switch (conversao.tipo) {
    case 'primeiro':
      // O TRAVESSÃO SOZINHO, como na maquete: o primeiro estágio não tem de onde converter, e isso não é falta de dado.
      return (
        <span className="funil-conversao-traco">
          <span aria-hidden="true">—</span>
          <span className="cad-so-leitor">primeiro estágio, sem estágio anterior de onde converter</span>
        </span>
      );
    case 'baseInsuficiente':
      return (
        <ValorAusente
          motivo={`O estágio anterior tem ${conversao.anterior.toLocaleString('pt-BR')} — base pequena demais para uma taxa significar alguma coisa.`}
          oQue="a conversão"
        />
      );
    case 'cresceu':
      return <span data-tom="cresceu">↑ {conversao.percentual.toFixed(0)}%</span>;
    case 'taxa':
      return <span data-tom={tomDaConversao(conversao.percentual)}>{conversao.percentual.toFixed(0)}%</span>;
  }
}
