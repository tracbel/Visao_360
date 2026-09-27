/**
 * A COMPARAÇÃO COM O MESMO TRECHO DO ANO ANTERIOR (decisão do Ricardo de 27/09/2026).
 *
 * O período padrão da tela é o ano fiscal até o último mês fechado, e todo
 * "vs. ano anterior" compara com O MESMO TRECHO do ano fiscal anterior —
 * nov/2025 a ago/2026 contra nov/2024 a ago/2025. A janela e a cobertura vêm do
 * servidor (`periodoAnterior`): se a carga começa depois da janela anterior, a
 * variação fica com o traço e o motivo, e não vira uma queda que não aconteceu.
 *
 * O INTERRUPTOR "COMPARAR COM PERÍODO ANTERIOR" E O "Δ %" SÃO DA PÁGINA, e não de
 * um bloco: eles moram neste contexto, que a casca da tela monta uma vez. Até
 * aqui os dois nasciam desligados — a leitura devolvia uma janela só. Agora
 * ligam de verdade: desligar esconde as variações (o lugar fica, para os
 * cartões não mudarem de altura), e o Δ escolhe entre a variação percentual e a
 * diferença na unidade do número.
 *
 * R$ E UNIDADES NUNCA SE SOMAM (D-P08): cada variação compara o número com ele
 * mesmo, e cada fonte tem a sua cobertura.
 */

import { useMemo, useState, type ReactNode } from 'react';
import type { ComparacaoComOAnoAnterior, PeriodoAnterior } from '../../tipos/territorio';
import { InfoTooltip } from '../InfoTooltip';
import { VariacaoAusente } from '../mercado/VariacaoAusente';
import {
  ContextoDaComparacao,
  janelaAnteriorPorExtenso,
  useComparacao,
  type UnidadeDaVariacao,
} from './contextoDaComparacao';

/** A casca da tela declara a comparação uma vez; cartões, mapas e ficha a leem. */
export function ProvedorDaComparacao({
  periodoAnterior,
  numeros,
  children,
}: {
  periodoAnterior: PeriodoAnterior | null;
  numeros: ComparacaoComOAnoAnterior | null;
  children: ReactNode;
}) {
  // LIGADA POR PADRÃO, como na maquete: a comparação é o que a diretoria pediu ver.
  const [ligada, setLigada] = useState(true);
  const [unidade, setUnidade] = useState<UnidadeDaVariacao>('percentual');
  const valor = useMemo(
    () => ({ ligada, unidade, aoLigar: setLigada, aoEscolherUnidade: setUnidade, periodoAnterior, numeros }),
    [ligada, unidade, periodoAnterior, numeros],
  );
  return <ContextoDaComparacao.Provider value={valor}>{children}</ContextoDaComparacao.Provider>;
}

/** O que a linha diz antes da resposta: não se afirma por que falta um número antes de saber se falta. */
const LENDO = 'A leitura ainda não voltou: a variação aparece com ela.';

/** A variação relativa, arredondada: inteira a partir de 10%, com uma casa abaixo — "+0,4%", e nunca "+0%". */
function percentual(fracao: number): string {
  const pontos = fracao * 100;
  const casas = Math.abs(pontos) >= 10 ? 0 : 1;
  const texto = Math.abs(pontos).toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });
  return `${pontos > 0 ? '+' : pontos < 0 ? '−' : ''}${texto}%`;
}

/**
 * UMA VARIAÇÃO "vs. ano anterior", no lugar e no desenho da maquete: seta, o
 * número, o texto e — no cartão largo — o valor de antes entre parênteses.
 *
 * SEM O NÚMERO DE ANTES, É O TRAÇO COM O MOTIVO VERDADEIRO (`VariacaoAusente`):
 * a carga não cobre a janela anterior, ou o número é estrutural e não tem ano
 * anterior. Uma seta sem número afirma uma direção que ninguém mediu.
 *
 * ANTERIOR ZERO NÃO TEM VARIAÇÃO PERCENTUAL: de zero para qualquer coisa não é
 * "+∞%". Aí a linha mostra a diferença na unidade, que é o que existe.
 */
export function VariacaoContraOAnoAnterior({
  deQue,
  atual,
  anterior,
  motivoSemAnterior,
  formatar,
  emPontos = false,
  compacta = false,
}: {
  /** O nome do número, para a dica e o leitor de tela. */
  deQue: string;
  atual: number | null;
  anterior: number | null;
  /** Por que o número de antes falta — a frase do servidor sempre que ela existe. */
  motivoSemAnterior: string | null | undefined;
  /** Como o número e a diferença aparecem na unidade dele. */
  formatar: (valor: number) => string;
  /** Se o número já é um percentual: a diferença é em pontos percentuais. */
  emPontos?: boolean;
  compacta?: boolean;
}) {
  const { ligada, unidade, periodoAnterior } = useComparacao();

  // DESLIGADA, A LINHA FICA INVISÍVEL NO LUGAR DELA: os cartões não mudam de
  // altura quando a comparação liga e desliga, e o leitor de tela não a lê.
  if (!ligada)
    return (
      <span className="mv-variacao" data-compacta={compacta || undefined} data-comparacao="desligada" aria-hidden="true">
        <span className="mv-variacao-traco">—</span>
        <span className="mv-variacao-texto">vs. ano anterior</span>
      </span>
    );

  if (atual === null || anterior === null)
    return <VariacaoAusente deQue={deQue} compacta={compacta} motivo={motivoSemAnterior || LENDO} />;

  const diferenca = atual - anterior;
  const sentido = diferenca > 0 ? 'alta' : diferenca < 0 ? 'baixa' : 'estavel';
  const seta = diferenca > 0 ? '↑' : diferenca < 0 ? '↓' : '→';
  const sinal = diferenca > 0 ? '+' : diferenca < 0 ? '−' : '';

  const absoluta = emPontos
    ? `${sinal}${Math.abs(diferenca).toLocaleString('pt-BR', { maximumFractionDigits: 1 })} p.p.`
    : `${sinal}${formatar(Math.abs(diferenca))}`;
  const numero = emPontos || unidade === 'absoluta' || anterior === 0 ? absoluta : percentual(diferenca / Math.abs(anterior));

  const janela = janelaAnteriorPorExtenso(periodoAnterior);
  const antes = `Antes: ${formatar(anterior)}${janela ? ` — ${janela}, o mesmo trecho do ano anterior` : ''}.`;

  return (
    <span className="mv-variacao" data-compacta={compacta || undefined} data-sentido={sentido}>
      <span className="mv-variacao-numero" aria-hidden="true">
        {seta} {numero}
      </span>
      <span className="cad-so-leitor">{`${numero} contra o mesmo trecho do ano anterior, quando foi ${formatar(anterior)}`}</span>
      <span className="mv-variacao-texto" aria-hidden="true">
        vs. ano anterior{compacta ? '' : ` (${formatar(anterior)})`}
      </span>
      {compacta && <InfoTooltip texto={antes} rotulo={`A variação de ${deQue}`} />}
    </span>
  );
}
