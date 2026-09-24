import type { ReactNode } from 'react';
import type { ClassificacaoDeIndicador } from '../../tipos/territorio';
import { InfoTooltip } from '../InfoTooltip';
import { ComoLerEstesNumeros } from './ComoLerEstesNumeros';

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
 * ELE NASCE DESLIGADO. É a comparação com o período anterior, e ela não existe:
 * a leitura desta tela devolve UMA janela de competência, não duas. Um
 * interruptor que liga uma comparação inexistente é o "botão que não faz nada"
 * do documento 05, §3.
 *
 * ENTÃO POR QUE DESENHÁ-LO. Porque é o mesmo padrão que os filtros sem dado
 * desta tela já usam: o controle pedido aparece, desligado, com o motivo e a
 * issue na dica. Some-lo faria a tela parecer completa — e o dia em que a issue
 * 69 trouxer o período anterior, o que muda aqui é uma linha.
 *
 * A SEGUNDA DICA, NO FIM DA LINHA, É "COMO LER OS NÚMEROS DE MERCADO": o que
 * era o subtítulo da seção, a dica das fatias ao lado do título e o antigo
 * "Como interpretar os indicadores" — que ficava num `<details>` entre os
 * filtros e as abas, e a maquete não tem. Sem o título visível, é aqui, no alto
 * da aba e ao lado do controle que fala dos mesmos números, que ela mora.
 */
export function ComparacaoComPeriodoAnterior({ classificacoes }: { classificacoes: ClassificacaoDeIndicador[] }) {
  return (
    <div className="dash-secao-controle" data-bloco="comparar-periodo">
      <span className="dash-comparar">
        Comparar com período anterior
        {/* `role="switch"` e `aria-checked` de verdade: o leitor de tela
            anuncia "interruptor, desligado, indisponível", que é o estado.
            `disabled` num `<button>` já tira do Tab — e a dica ao lado
            continua alcançável, que é onde está a explicação. */}
        <button
          type="button"
          className="dash-interruptor"
          role="switch"
          aria-checked={false}
          disabled
          aria-label="Comparar com o período anterior"
        />
        <InfoTooltip
          rotulo="Por que a comparação com o período anterior está desligada"
          texto={
            'A leitura desta tela devolve UMA janela de competência — a do filtro de período —, e não duas: não ' +
            'há período anterior para comparar. É por isso que nenhum cartão traz "+8% vs. ano anterior", e o ' +
            'único bloco com variação é o crédito, que tem duas safras na própria fonte. Ligar isto depende de a ' +
            'leitura passar a devolver a janela anterior junto (issue 69).'
          }
        />
      </span>

      {/* A unidade da variação — Δ% ou Δ absoluto. Desligada pelo mesmo
          motivo: ela escolhe COMO mostrar uma comparação que não existe. */}
      <select disabled aria-label="Unidade da comparação">
        <option>Δ %</option>
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
