/**
 * A COMPOSIÇÃO DO FATOR, CULTURA POR CULTURA (fase T3.1).
 *
 * O número do topo da tela é UM, mas ele não nasce de um índice de mercado: ele é
 * a razão entre dois números que o motor já calcula para cada cultura —
 *
 *     fator agregado = Σ demanda ajustada ÷ Σ demanda estrutural
 *
 * — e é aqui que essa conta fica visível. Cada linha mostra a cultura, o índice
 * de preço DELA, o fator DELA e as três parcelas que o explicam; a última linha
 * mostra as duas somas e o quociente. Quem desconfiar do 0,88 do topo soma a
 * coluna e confere.
 *
 * AS SETAS MORAM AQUI, E NÃO NO RESUMO EXECUTIVO. As três parcelas — preço e
 * rentabilidade, crédito e percepção — são calculadas por cultura; desenhá-las
 * agregadas lá em cima exigiria decompor o quociente em três pedaços que o
 * domínio não produz. Inventar essa decomposição só para manter três setas no
 * topo seria desenho mandando na matemática.
 *
 * PREÇO É DE CADA CULTURA; CRÉDITO E PERCEPÇÃO SÃO DO RECORTE. Por isso as duas
 * últimas setas se repetem iguais em todas as linhas, e a tela diz isso em vez de
 * deixar o leitor achar que é bug.
 *
 * AS PARCELAS SÃO TRÊS, E NÃO QUATRO (D-P05). O custo entra DENTRO da primeira —
 * rentabilidade é preço menos custo —, e uma quarta seta afirmaria uma
 * sensibilidade independente que a decisão não criou. O termo de troca ficou de
 * fora porque precisa do preço de máquina (issue 70).
 */

import { InfoTooltip } from '../InfoTooltip';
import { ValorAusente } from '../comum/ValorAusente';
import type { FatorDoCiclo, MomentoDaCultura, MomentoDoRecorte } from '../../tipos/territorio';

/**
 * AS CASAS SÃO FIXAS, e isso não é enfeite: um fator neutro tem de aparecer como
 * `1,00`. Escrito `1`, ele deixa de parecer um fator e passa a parecer contagem.
 */
const pt = (v: number, casas = 2) =>
  v.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });

const inteiro = (v: number) => v.toLocaleString('pt-BR', { maximumFractionDigits: 0 });

/**
 * A DIREÇÃO CARREGA O SIGNIFICADO, e não a cor.
 *
 * Quem não distingue verde de vermelho lê a seta igual. Perto de zero é "→": uma
 * parcela de 0,001 não empurra nada, e desenhar ↑ ali sugeriria um movimento que
 * não existe.
 */
function seta(parcela: number | null): string {
  if (parcela === null) return '·';
  if (parcela > 0.005) return '↑';
  if (parcela < -0.005) return '↓';
  return '→';
}

/** Uma parcela do fator de UMA cultura, com a conta e a origem na dica. */
function Parcela({
  nome,
  parcela,
  explicacao,
  cultura,
}: {
  nome: string;
  parcela: number | null;
  explicacao: string;
  /** Para a dica não ficar ambígua quando a tela tem várias culturas abertas. */
  cultura: string;
}) {
  return (
    <span className="terr-parcela">
      <span className="terr-parcela-seta" aria-hidden="true">
        {seta(parcela)}
      </span>
      <span className="terr-parcela-nome">{nome}</span>
      <InfoTooltip
        rotulo={`Como ${nome.toLowerCase()} entra no fator de ${cultura}`}
        texto={
          parcela === null
            ? `${explicacao} Sem índice carregado, esta parcela vale desvio ZERO — e não "indeterminado".`
            : `${explicacao} Contribuição nesta cultura: ${parcela > 0 ? '+' : ''}${pt(parcela * 100, 1)}% sobre a demanda estrutural dela.`
        }
      />
    </span>
  );
}

/** As três parcelas de uma cultura, mais o aviso de corte quando o limite agiu. */
function ParcelasDaCultura({ cultura, fator }: { cultura: string; fator: FatorDoCiclo }) {
  return (
    <div className="terr-parcelas" aria-label={`Por que ${cultura} está neste momento`}>
      <Parcela
        nome="Commodity"
        cultura={cultura}
        parcela={fator.parcelaDePreco}
        explicacao={`Preço e rentabilidade de ${cultura} — o índice é DESTA cultura, e não do recorte. O custo entra AQUI: rentabilidade é preço menos custo, e não uma quarta sensibilidade.`}
      />
      <Parcela
        nome="Crédito"
        cultura={cultura}
        parcela={fator.parcelaDeCredito}
        explicacao="Índice de crédito do SICOR — 70% linhas e 30% valor, na janela vigente. É do MUNICÍPIO, não da cultura, e por isso entra igual em todas. Ele MULTIPLICA o resto, porque é a condição de financiar: sem crédito, nem a melhor safra vira máquina."
      />
      <Parcela
        nome="Percepção"
        cultura={cultura}
        parcela={fator.parcelaDaPercepcao}
        explicacao="A leitura do comercial sobre o município, de −5 a +5 pontos percentuais, com autor e vigência (issue 71). É do RECORTE, não da cultura. Ela soma dentro do parêntese do produtor, então o crédito a amplifica."
      />
      {fator.cortadoPeloLimite && (
        <span className="terr-parcela">
          <span className="terr-parcela-nome">no limite</span>
          <InfoTooltip
            rotulo={`Por que o fator de ${cultura} foi cortado`}
            texto={`O fator calculado foi ${pt(fator.fatorSemLimite ?? 0)} e o limite registrado o trouxe para ${pt(fator.fator ?? 0)}. O limite é decisão com vigência, não arredondamento.`}
          />
        </span>
      )}
    </div>
  );
}

/** Uma linha da composição: a cultura, o índice dela, o fator dela e as parcelas. */
function LinhaDaCultura({ c }: { c: MomentoDaCultura }) {
  return (
    <li className="terr-composicao-linha" data-cultura={c.culturaCodigo}>
      <div className="terr-composicao-cabecalho">
        <span className="terr-composicao-cultura">{c.cultura}</span>

        <span className="terr-composicao-numero">
          índice de preço{' '}
          {c.indiceDePreco === null ? (
            <ValorAusente
              motivo={`A série de preço não cobre ${c.cultura}, então o preço não desvia o fator dela. Indicador ausente vale desvio ZERO — não é "cultura em queda".`}
              oQue={`o índice de preço de ${c.cultura}`}
            />
          ) : (
            <span className="cad-mono">{pt(c.indiceDePreco)}</span>
          )}
        </span>

        <span className="terr-composicao-numero">
          fator{' '}
          {c.fator.fator === null ? (
            <ValorAusente motivo={c.fator.motivo} oQue={`o fator de ${c.cultura}`} />
          ) : (
            <span className="cad-mono">{pt(c.fator.fator)}</span>
          )}
        </span>

        <span className="terr-composicao-numero">
          demanda{' '}
          {c.demandaEstrutural === null ? (
            <ValorAusente
              motivo={`${c.cultura} não tem ciclo de renovação informado (D-P01, issue 63), então não há demanda estrutural aqui — e uma cultura sem demanda não pesa na agregação, em vez de entrar como zero e puxar o número para baixo.`}
              oQue={`a demanda de ${c.cultura}`}
            />
          ) : (
            <>
              <span className="cad-mono">{inteiro(c.demandaEstrutural)}</span>
              {c.demandaAjustada !== null && (
                <>
                  {' → '}
                  <span className="cad-mono">{inteiro(c.demandaAjustada)}</span>
                </>
              )}{' '}
              máq/ano
            </>
          )}
        </span>
      </div>

      {c.fator.fator !== null && <ParcelasDaCultura cultura={c.cultura} fator={c.fator} />}
    </li>
  );
}

export function ComposicaoDoFator({ momento }: { momento: MomentoDoRecorte | null }) {
  if (!momento) return null;

  const { porCultura, fatorAgregado, demandaEstruturalTotal, demandaAjustadaTotal } = momento;

  // SEM CULTURA NENHUMA NÃO HÁ COMPOSIÇÃO A MOSTRAR, e a tela diz por quê em vez
  // de desenhar uma tabela vazia que pareça carregando. É esta a resposta com o
  // dado de hoje, e ela explica o vazio do momento lá em cima.
  if (porCultura.length === 0)
    return (
      <p className="cad-sub" data-bloco="composicao-do-fator">
        Nenhuma cultura deste recorte tem regra de potencial com ciclo de renovação, então não há demanda a
        ajustar e não há composição a decompor. É a decisão D-P01 (issue 63) que fixa cultura, categoria,
        hectares por máquina e anos de renovação — as quatro juntas.
      </p>
    );

  return (
    <div data-bloco="composicao-do-fator">
      <p className="cad-sub">
        O fator de cada cultura tem o preço <strong>dela</strong>; crédito e percepção são do recorte e entram
        iguais em todas. O número do topo da página é a razão entre as duas somas abaixo — some a coluna e confira.
      </p>

      <ul className="terr-composicao">
        {porCultura.map((c) => (
          <LinhaDaCultura key={c.culturaCodigo} c={c} />
        ))}
      </ul>

      <p className="terr-composicao-total">
        <strong>Agregado:</strong>{' '}
        {fatorAgregado === null || demandaEstruturalTotal === null || demandaAjustadaTotal === null ? (
          <ValorAusente
            motivo={
              momento.motivoSemFator === 'SemDemandaEstrutural'
                ? 'Nenhuma cultura do recorte tem demanda estrutural, então não há denominador para a razão. Enquanto o ciclo de renovação (D-P01, issue 63) não for decidido, o agregado fica ausente: "não há base para dizer" é diferente de "o mercado está neutro".'
                : 'Há demanda estrutural, mas nenhuma cultura produziu fator — faltam os pesos das sensibilidades (D-P05). Peso é decisão registrada, não conta.'
            }
            oQue="o fator agregado"
          />
        ) : (
          <>
            <span className="cad-mono">{inteiro(demandaAjustadaTotal)}</span> ajustada ÷{' '}
            <span className="cad-mono">{inteiro(demandaEstruturalTotal)}</span> estrutural ={' '}
            <span className="cad-mono">{pt(fatorAgregado)}</span> máq/ano
            <InfoTooltip
              rotulo="Como o fator agregado é calculado"
              texto={
                'Σ demanda ajustada ÷ Σ demanda estrutural, somando só as culturas que têm os dois números. ' +
                'Cada cultura pesa exatamente pela demanda que representa — a área dela não entra nesta conta. ' +
                'Nenhuma fórmula nova foi criada: com todas as culturas neutras, as duas somas se igualam e o ' +
                'agregado dá 1,00. E como cada fator já vem dentro dos limites registrados, a razão não escapa deles.'
              }
            />
          </>
        )}
      </p>
    </div>
  );
}
