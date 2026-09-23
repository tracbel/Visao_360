/**
 * A RÉGUA DO MERCADO — momento, porte e o que a região tem (fase T4.8).
 *
 * Sete células divididas por um filete, numa faixa só: o momento e o porte nas
 * duas primeiras, com a faixa em pílula; os cinco indicadores estruturais nas
 * seguintes. É o `market-strip` do protótipo visual.
 *
 * POR QUE ELES FICAM JUNTOS. Antes eram dois blocos empilhados — uma faixa de
 * "porte e momento" e outra de "o que a região tem". São a mesma leitura: *como
 * está o mercado, e o que existe nele*. Separá-los dava dois retângulos brancos
 * seguidos, e fazia a página parecer mais longa do que o conteúdo pede.
 *
 * O DIVISOR VERTICAL É O QUE TRANSFORMA SETE NÚMEROS NUMA RÉGUA, em vez de numa
 * lista: o olho corre por ela sem precisar de espaço grande entre os itens.
 *
 * NENHUM NÚMERO SAIU, e nenhuma explicação também: a comparação de cada
 * indicador continua na dica dele (documento 50, §7), o porte continua sem nome
 * enquanto a issue 166 não tiver bandas, e o momento continua dizendo por que
 * está ausente quando está.
 */

import { BarChart3, Beef, Factory, Landmark, Sprout, Tractor, TrendingUp } from 'lucide-react';
import { FaixaDeEstrutura, ItemDaFaixa } from '../dashboard/Dashboard';
import { InfoTooltip } from '../InfoTooltip';
import { Procedencia } from '../comum/Procedencia';
import { ValorAusente } from '../comum/ValorAusente';
import type { Indicador } from '../cadastro/Indicadores';
import type { MomentoDoRecorte } from '../../tipos/territorio';

/**
 * O ícone de cada indicador estrutural, pelo rótulo.
 *
 * Por rótulo e não por posição: a ordem da faixa vem de `kpisDoMercado`, e
 * amarrar o ícone ao índice faria o trator virar boi no dia em que alguém
 * reordenasse a lista. Rótulo desconhecido simplesmente não ganha ícone — é
 * melhor que ganhar o ícone errado.
 */
const ICONES: Record<string, typeof Tractor> = {
  'Parque de tratores': Tractor,
  Propriedades: Landmark,
  'Valor da lavoura': Sprout,
  'Usinas de etanol': Factory,
  'Rebanho bovino': Beef,
};

/** Casas fixas: um fator neutro tem de sair `1,00`, e não `1`. */
const pt = (v: number) => v.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** Por que o agregado não saiu — dito na língua de quem lê, com a decisão que falta. */
function motivoEmPortugues(motivo: string): string {
  if (motivo === 'SemDemandaEstrutural')
    return (
      'Nenhuma cultura do recorte tem ciclo de renovação informado, então não há demanda estrutural para ajustar. ' +
      'A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro ' +
      'juntas. Sem ela o fator fica ausente: "não há base para dizer" é diferente de "o mercado está neutro".'
    );

  if (motivo === 'SemFatorPorCultura')
    return (
      'Há demanda estrutural, mas nenhuma cultura produziu fator: faltam os pesos das sensibilidades (D-P05). ' +
      'Peso é decisão registrada, não conta — sem ele a tela mostra a demanda estrutural sozinha.'
    );

  return 'O fator agregado não pôde ser apurado para este recorte.';
}

export function FaixaDoMercado({
  indicadores,
  momento,
}: {
  indicadores: Indicador[];
  momento: MomentoDoRecorte | null;
}) {
  if (indicadores.length === 0 && !momento) return null;

  const semFator = momento?.fatorAgregado == null;

  return (
    <FaixaDeEstrutura data-bloco="faixa-do-mercado" aria-label="O momento do mercado e o que a região tem">
      {/* ---- MOMENTO ---- */}
      <div className="dash-faixa-leitura" data-bloco="porte-e-momento">
        <TrendingUp size={19} strokeWidth={2} aria-hidden="true" />
        <div>
          <strong>Momento do mercado</strong>
          {semFator ? (
            <ValorAusente
              motivo={motivoEmPortugues(momento?.motivoSemFator ?? '')}
              oQue="o momento do mercado"
            />
          ) : (
            <span className="dash-pilula" data-faixa={momento!.faixaDoMomento ?? undefined}>
              {momento!.faixaDoMomento ?? 'sem faixa'} · {pt(momento!.fatorAgregado!)}
              <Procedencia procedencia={momento!.procedencia} oQue="o momento do mercado" />
              {/* O ⓘ QUE LEVA À CONTA. As três parcelas não cabem aqui porque são
                  de cada cultura; quem quiser vê-las abre o bloco. */}
              <InfoTooltip
                rotulo="Como o momento do mercado é composto"
                texto={
                  'Este número é a razão entre a demanda ajustada somada e a demanda estrutural somada — cada ' +
                  'cultura pesa pela demanda que representa. As três parcelas (rentabilidade, crédito e percepção) ' +
                  'são calculadas POR CULTURA e estão abertas em "Momento do mercado", na aba "Composição do ' +
                  'fator", uma linha por cultura. Não há três setas aqui porque não existe uma decomposição ' +
                  'agregada: inventá-la só para desenhar as setas seria um número sem conta.'
                }
              />
            </span>
          )}
          {/* A FRASE DO MOMENTO, embaixo da pílula (maquete: "Demanda estável e
              preços firmes"). Ela vem pronta da API (`leitura`) e já existia na
              resposta sem ter lugar na tela desde que a faixa virou régua —
              "Mercado grande, agora retraído" é a leitura que a palavra da
              pílula sozinha não dá. Vazia quando falta um dos dois lados. */}
          {momento?.leitura && <p className="dash-faixa-frase">{momento.leitura}</p>}
        </div>
      </div>

      {/* ---- PORTE ---- */}
      <div className="dash-faixa-leitura">
        <BarChart3 size={19} strokeWidth={2} aria-hidden="true" />
        <div>
          <strong>Porte estrutural</strong>
          {momento?.porte ? (
            <span className="dash-pilula">{momento.porte}</span>
          ) : (
            <ValorAusente
              motivo={
                'O nome do porte — pequeno, médio ou grande — depende de bandas registradas, e elas ainda não ' +
                'foram decididas (issue 166). Um corte sem dono é parâmetro inventado, então a tela mostra o ' +
                'número da demanda anual acima e não dá nome. Nulo aqui NÃO quer dizer "pequeno".'
              }
              oQue="o nome do porte"
            />
          )}
        </div>
      </div>

      {/* ---- OS CINCO INDICADORES ESTRUTURAIS ---- */}
      {indicadores.map((i) => (
        <ItemDaFaixa
          key={i.rotulo}
          rotulo={i.rotulo}
          icone={ICONES[i.rotulo]}
          valor={i.valor}
          // A COMPARAÇÃO CONTINUA SENDO DE CADA NÚMERO (documento 50, §7), só
          // que na dica dele em vez de numa linha permanente embaixo.
          comparacao={i.deOnde === '—' ? undefined : i.deOnde}
          procedencia={i.procedencia}
          motivoSemDado={i.semDado}
        />
      ))}
    </FaixaDeEstrutura>
  );
}
