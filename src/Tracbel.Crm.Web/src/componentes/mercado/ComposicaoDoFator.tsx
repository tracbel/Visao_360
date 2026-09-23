/**
 * A COMPOSIÇÃO DO FATOR, CULTURA POR CULTURA (T3.1; redesenhada na T4.6).
 *
 * O número do topo da tela é UM, mas ele não nasce de um índice de mercado: ele é
 * a razão entre dois números que o motor já calcula para cada cultura —
 *
 *     fator agregado = Σ demanda ajustada ÷ Σ demanda estrutural
 *
 * — e é aqui que essa conta fica visível. Quem desconfiar do 0,88 do topo soma a
 * coluna e confere.
 *
 * O QUE MUDOU NA T4.6 — a forma, e nada da conta.
 *
 * Isto era uma lista de parágrafos: cada cultura numa linha de texto corrido,
 * com "índice de preço 0,80  fator 0,84  demanda 142 → 119 máq/ano" e as três
 * setas embaixo, também em texto. Para comparar o crédito do café com o da cana
 * o olho tinha de andar na diagonal. Virou **tabela**, que é a forma de
 * comparar a mesma grandeza entre linhas — e foi para isso que a tabela existe.
 *
 * AS COLUNAS SÃO AS DA LEITURA: cultura, momento dela, e as três parcelas em
 * colunas próprias, cada uma no seu eixo vertical. Quem quiser o detalhe — os
 * índices, as demandas, o que cada parcela contribuiu — abre a linha.
 *
 * PREÇO É DE CADA CULTURA; CRÉDITO E PERCEPÇÃO SÃO DO RECORTE. Por isso as duas
 * últimas colunas se repetem iguais em todas as linhas, e a tela diz isso no
 * cabeçalho em vez de deixar o leitor achar que é falha.
 *
 * AS PARCELAS SÃO TRÊS, E NÃO QUATRO (D-P05). O custo entra DENTRO da primeira —
 * rentabilidade é preço menos custo —, e uma quarta coluna afirmaria uma
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

/** Uma célula de parcela: a seta, e a conta na dica. */
function Parcela({
  nome,
  parcela,
  explicacao,
  cultura,
}: {
  nome: string;
  parcela: number | null;
  explicacao: string;
  cultura: string;
}) {
  return (
    <td className="dash-tabela-seta">
      <span className="terr-parcela-seta" aria-hidden="true">
        {seta(parcela)}
      </span>
      <span className="cad-so-leitor">{nome}</span>
      <span className="terr-parcela-nome cad-so-leitor">{nome}</span>
      <InfoTooltip
        rotulo={`Como ${nome.toLowerCase()} entra no fator de ${cultura}`}
        texto={
          parcela === null
            ? `${explicacao} Sem índice carregado, esta parcela vale desvio ZERO — e não "indeterminado".`
            : `${explicacao} Contribuição nesta cultura: ${parcela > 0 ? '+' : ''}${pt(parcela * 100, 1)}% sobre a demanda estrutural dela.`
        }
      />
    </td>
  );
}

/** A faixa do fator de uma cultura, em palavra — é o que se lê de longe. */
function faixaDaCultura(fator: FatorDoCiclo): string {
  if (fator.fator === null) return '—';
  if (fator.fator < 1) return 'Retraído';
  if (fator.fator > 1) return 'Aquecido';
  return 'Normal';
}

/** Uma linha da composição. */
function LinhaDaCultura({ c }: { c: MomentoDaCultura }) {
  return (
    <tr data-cultura={c.culturaCodigo}>
      <th scope="row" className="dash-tabela-nome">
        {c.cultura}
      </th>

      <td className="dash-tabela-faixa">{faixaDaCultura(c.fator)}</td>

      <td className="cad-mono dash-tabela-numero">
        {c.fator.fator === null ? (
          <ValorAusente motivo={c.fator.motivo} oQue={`o fator de ${c.cultura}`} />
        ) : (
          pt(c.fator.fator)
        )}
      </td>

      <Parcela
        nome="Rentabilidade"
        cultura={c.cultura}
        parcela={c.fator.parcelaDePreco}
        explicacao={`Preço e rentabilidade de ${c.cultura} — o índice é DESTA cultura (${c.indiceDePreco === null ? 'não carregado' : pt(c.indiceDePreco)}), e não do recorte. O custo entra AQUI: rentabilidade é preço menos custo, e não uma quarta sensibilidade.`}
      />
      <Parcela
        nome="Crédito"
        cultura={c.cultura}
        parcela={c.fator.parcelaDeCredito}
        explicacao="Índice de crédito do SICOR — 70% linhas e 30% valor, na janela vigente. É do MUNICÍPIO, não da cultura, e por isso entra igual em todas. Ele MULTIPLICA o resto, porque é a condição de financiar: sem crédito, nem a melhor safra vira máquina."
      />
      <Parcela
        nome="Percepção"
        cultura={c.cultura}
        parcela={c.fator.parcelaDaPercepcao}
        explicacao="A leitura do comercial sobre o município, de −5 a +5 pontos percentuais, com autor e vigência (issue 71). É do RECORTE, não da cultura. Ela soma dentro do parêntese do produtor, então o crédito a amplifica."
      />

      <td className="cad-mono dash-tabela-numero">
        {c.demandaEstrutural === null ? (
          <ValorAusente
            motivo={`${c.cultura} não tem ciclo de renovação informado (D-P01, issue 63), então não há demanda estrutural aqui — e uma cultura sem demanda não pesa na agregação, em vez de entrar como zero e puxar o número para baixo.`}
            oQue={`a demanda de ${c.cultura}`}
          />
        ) : (
          <>
            {inteiro(c.demandaEstrutural)}
            {c.demandaAjustada !== null && <> → {inteiro(c.demandaAjustada)}</>}
          </>
        )}
        {c.fator.cortadoPeloLimite && (
          <InfoTooltip
            rotulo={`Por que o fator de ${c.cultura} foi cortado`}
            texto={`O fator calculado foi ${pt(c.fator.fatorSemLimite ?? 0)} e o limite registrado o trouxe para ${pt(c.fator.fator ?? 0)}. O limite é decisão com vigência, não arredondamento.`}
          />
        )}
      </td>
    </tr>
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
    // A ROLAGEM É DENTRO DO CARTÃO, e nunca da página. Sete colunas com cabeçalho
    // que não quebra dão 860px, e numa tela de 768 isso empurrava a PÁGINA para o
    // lado — medido pela conferência visual assim que a tabela entrou. Rolagem
    // lateral da página esconde coluna sem avisar; dentro do cartão, o leitor vê
    // a barra e sabe que há mais à direita.
    <div className="cad-tabela-wrap dash-tabela-rolagem" data-bloco="composicao-do-fator">
      <table className="cad-tabela dash-tabela">
        <thead>
          <tr>
            <th scope="col">Cultura</th>
            <th scope="col">Momento</th>
            <th scope="col">Fator</th>
            <th scope="col">
              Rentab.
              <InfoTooltip
                rotulo="De quem é a rentabilidade"
                texto="Preço e rentabilidade são DE CADA CULTURA — por isso esta coluna muda de linha para linha. O custo entra aqui dentro."
              />
            </th>
            <th scope="col">
              Crédito
              <InfoTooltip
                rotulo="Por que o crédito se repete"
                texto="O índice de crédito é do MUNICÍPIO, não da cultura: ele entra igual em todas as linhas. Não é falha da tela."
              />
            </th>
            <th scope="col">
              Percep.
              <InfoTooltip
                rotulo="Por que a percepção se repete"
                texto="A percepção do comercial é do RECORTE, não da cultura: ela entra igual em todas as linhas. Não é falha da tela."
              />
            </th>
            <th scope="col">Demanda → ajustada</th>
          </tr>
        </thead>
        <tbody>
          {porCultura.map((c) => (
            <LinhaDaCultura key={c.culturaCodigo} c={c} />
          ))}
        </tbody>
        <tfoot>
          <tr className="dash-tabela-total">
            <th scope="row" colSpan={2}>
              Agregado
            </th>
            <td className="cad-mono dash-tabela-numero">
              {fatorAgregado === null ? (
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
                  {pt(fatorAgregado)}
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
            </td>
            <td colSpan={3} />
            <td className="cad-mono dash-tabela-numero">
              {demandaEstruturalTotal === null || demandaAjustadaTotal === null ? (
                '—'
              ) : (
                <>
                  {inteiro(demandaEstruturalTotal)} → {inteiro(demandaAjustadaTotal)}
                </>
              )}
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  );
}
