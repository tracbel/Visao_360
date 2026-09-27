import type { ReactNode } from 'react';
import type { ClassificacaoDeIndicador } from '../../tipos/territorio';
import { InfoTooltip } from '../InfoTooltip';
import { ComoLerEstesNumeros } from './ComoLerEstesNumeros';
import { janelaAnteriorPorExtenso, useComparacao } from './contextoDaComparacao';

/**
 * A seção "O mercado da região" — os quatro números de decisão.
 *
 * A §10.3 do documento 49 e o documento 50 mandam preservá-la: é a seção que a
 * diretoria já reconhece. ELA CONTINUA EXISTINDO, com o nome dela, e continua
 * sendo a primeira coisa da aba Mercado.
 *
 * O QUE MUDOU É QUE O TÍTULO NÃO APARECE (fidelidade às maquetes, 23/09/2026).
 * As duas maquetes discordam: a de visão geral põe "O mercado da região" com
 * subtítulo acima dos cartões; a de rentabilidade e crédito — que é a que o
 * usuário escolheu como alvo da ordem da aba — põe os quatro cartões logo
 * abaixo das abas, sem título, com o "Comparar com período anterior" subindo
 * para a linha das abas. Seguimos a segunda, que é a decisão.
 *
 * POR QUE O TÍTULO FICA NO DOCUMENTO, SÓ PARA O LEITOR DE TELA: sem ele, quem
 * navega por títulos pula de "Indicadores Geográficos da ADR" direto para
 * "Momento do mercado", e os quatro números de decisão ficam sem nome. A seção é
 * rotulada pelo `h2`, e o olho vê exatamente o que a maquete mostra.
 *
 * O SUBTÍTULO E A DICA DAS FATIAS NÃO SE PERDERAM: foram para a dica ao lado do
 * "Comparar com período anterior" — ver {@link ComparacaoComPeriodoAnterior}.
 */
export function SecaoDoMercadoDaRegiao({ children }: { children: ReactNode }) {
  return (
    <section data-bloco="mercado-da-regiao" aria-labelledby="titulo-mercado-da-regiao">
      <h2 id="titulo-mercado-da-regiao" className="cad-so-leitor">
        O mercado da região
      </h2>
      {children}
    </section>
  );
}

/**
 * "Comparar com período anterior" + Δ% — O CONTROLE DA MAQUETE, na linha das
 * abas (fidelidade às maquetes, 23/09/2026; antes, à direita do título "O
 * mercado da região").
 *
 * ELE LIGA DE VERDADE DESDE 27/09/2026. Até ali nascia desligado, porque a
 * leitura devolvia uma janela só; agora ela traz o mesmo trecho do ano anterior
 * (`periodoAnterior`), e o interruptor mostra ou esconde as variações da página.
 * O Δ escolhe entre a variação percentual e a diferença na unidade do número.
 * Os dois moram no contexto da comparação — ver `comparacao.tsx`.
 *
 * A SEGUNDA DICA, NO FIM DA LINHA, É "COMO LER OS NÚMEROS DE MERCADO": o que
 * era o subtítulo da seção, a dica das fatias ao lado do título e o antigo
 * "Como interpretar os indicadores" — que ficava num `<details>` entre os
 * filtros e as abas, e a maquete não tem. Sem o título visível, é aqui, no alto
 * da aba e ao lado do controle que fala dos mesmos números, que ela mora.
 */
export function ComparacaoComPeriodoAnterior({ classificacoes }: { classificacoes: ClassificacaoDeIndicador[] }) {
  const { ligada, aoLigar, unidade, aoEscolherUnidade, periodoAnterior } = useComparacao();
  const janela = janelaAnteriorPorExtenso(periodoAnterior);

  return (
    <div className="dash-secao-controle" data-bloco="comparar-periodo">
      <span className="dash-comparar">
        Comparar com período anterior
        {/* `role="switch"` e `aria-checked` de verdade: o leitor de tela
            anuncia "interruptor, ligado", que é o estado. */}
        <button
          type="button"
          className="dash-interruptor"
          role="switch"
          aria-checked={ligada}
          onClick={() => aoLigar(!ligada)}
          aria-label="Comparar com o período anterior"
        />
        <InfoTooltip
          rotulo="Com o que a comparação compara"
          texto={
            <>
              <p>
                O <strong>mesmo trecho do ano anterior</strong>
                {janela ? ` — ${janela}` : ''}: a janela do filtro de período, doze meses para trás. No padrão, o ano
                fiscal até o último mês fechado contra o ano fiscal anterior até o mesmo mês (decisão de 27/09/2026).
              </p>
              <p>
                Reais (Protheus) e máquinas (ART) comparam cada um consigo mesmo, e nunca se somam. Quando a carga de
                uma fonte começa depois da janela anterior, a variação dela fica com o traço e o motivo — comparar com
                meses que não foram carregados mostraria uma queda que não aconteceu. A demanda e o mercado anual são
                estruturais e não têm ano anterior.
              </p>
            </>
          }
        />
      </span>

      {/* A UNIDADE DA VARIAÇÃO — Δ % é a relativa; Δ absoluto é a diferença na
          unidade do número (R$, máquinas). A captura é sempre em pontos
          percentuais, porque ela já é um percentual. */}
      <select
        aria-label="Unidade da comparação"
        value={unidade}
        disabled={!ligada}
        onChange={(e) => aoEscolherUnidade(e.target.value as 'percentual' | 'absoluta')}
      >
        <option value="percentual">Δ %</option>
        <option value="absoluta">Δ absoluto</option>
      </select>

      <InfoTooltip
        rotulo="Como ler os números de Mercado"
        texto={
          <>
            <p>
              <strong>O mercado da região:</strong> o que existe no território, por fonte pública.
            </p>
            {/* A DICA DAS FATIAS, que morava ao lado do título (fase T2.1): é
                método, e método é nível 2 da hierarquia da issue 33. */}
            <p>
              O denominador de São Paulo é o total PUBLICADO pelo IBGE, e não a soma dos municípios: o valor
              municipal sigiloso entra no total do estado sem aparecer embaixo. O denominador da Região Tracbel é a
              área de atuação inteira, e não muda quando o filtro de sub-região muda.
            </p>
            <ComoLerEstesNumeros classificacoes={classificacoes} />
          </>
        }
      />
    </div>
  );
}
