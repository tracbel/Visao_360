/**
 * PORTE E MOMENTO — dois números, nunca um (documento 50, §4.2; corrigido na T3.1).
 *
 * O PORTE é o tamanho do mercado e muda devagar: safra, censo. O MOMENTO é o
 * fator de ciclo e muda todo mês. Misturá-los esconderia a leitura que a
 * diretoria precisa: *"estruturalmente grande, mas agora retraído"* é uma decisão
 * diferente de *"pequeno e aquecido"*.
 *
 * O PORTE NASCE SEM NOME, e isso não é omissão: nomear exige um corte, e um corte
 * sem dono é parâmetro inventado (R-27 do documento 46). As bandas são a issue
 * 166. **Nulo não é "pequeno".**
 *
 * AQUI NÃO HÁ SETAS, E ISSO É A CORREÇÃO DA T3.1. As três parcelas — preço e
 * rentabilidade, crédito e percepção — são calculadas POR CULTURA. Desenhá-las
 * agregadas no topo exigiria uma decomposição que o domínio não produz, e
 * inventá-la só para manter três setas seria mentir matematicamente. O resumo
 * mostra o número e o ⓘ; a composição inteira, cultura por cultura, está no bloco
 * "Momento do mercado".
 *
 * A CULTURA PREDOMINANTE APARECE COMO CONTEXTO, com o critério dito. Ela responde
 * "o que se planta aqui?" — e não decide o fator de ninguém.
 */

import { InfoTooltip } from '../InfoTooltip';
import { Procedencia } from '../comum/Procedencia';
import { ValorAusente } from '../comum/ValorAusente';
import type { MomentoDoRecorte } from '../../tipos/territorio';

/** Casas fixas: um fator neutro tem de sair `1,00`, e não `1` — que vira contagem. */
const pt = (v: number, casas = 2) =>
  v.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });

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

export function PorteEMomento({ momento }: { momento: MomentoDoRecorte | null }) {
  if (!momento) return null;

  return (
    <div className="terr-porte-momento" data-bloco="porte-e-momento">
      <div className="terr-porte-momento-linha">
        {/* PORTE — o número e a comparação; o nome só quando houver banda. */}
        <span className="terr-leitura">
          <strong>Porte:</strong>{' '}
          {momento.porte ?? (
            <ValorAusente
              motivo={
                'O nome do porte — pequeno, médio ou grande — depende de bandas registradas, e elas ainda não ' +
                'foram decididas (issue 166). Um corte sem dono é parâmetro inventado, então a tela mostra o ' +
                'número da demanda anual acima e não dá nome. Nulo aqui NÃO quer dizer "pequeno".'
              }
              oQue="o nome do porte"
            />
          )}
        </span>

        {/* MOMENTO — o fator AGREGADO, que já tem faixas decididas (issues 73 e 74). */}
        <span className="terr-leitura">
          <strong>Momento:</strong>{' '}
          {momento.fatorAgregado === null ? (
            <ValorAusente motivo={motivoEmPortugues(momento.motivoSemFator)} oQue="o momento do mercado" />
          ) : (
            <>
              <span className="cad-mono">{pt(momento.fatorAgregado)}</span>
              {momento.faixaDoMomento && <> · {momento.faixaDoMomento}</>}
              <Procedencia procedencia={momento.procedencia} oQue="o momento do mercado" />
              {/* O ⓘ QUE LEVA À CONTA. As três parcelas não cabem aqui em cima
                  porque são de cada cultura; quem quiser vê-las abre o bloco. */}
              <InfoTooltip
                rotulo="Como o momento do mercado é composto"
                texto={
                  'Este número é a razão entre a demanda ajustada somada e a demanda estrutural somada — cada ' +
                  'cultura pesa pela demanda que representa. As três parcelas (commodity, crédito e percepção) ' +
                  'são calculadas POR CULTURA e estão abertas em "Momento do mercado", na aba "Composição do ' +
                  'fator", uma linha por cultura. Não há três setas aqui em cima porque não existe uma ' +
                  'decomposição agregada: inventá-la só para desenhar as setas seria um número sem conta.'
                }
              />
            </>
          )}
        </span>
      </div>

      {momento.leitura && <p className="terr-leitura-frase">{momento.leitura}</p>}

      {/* A PREDOMINANTE É CONTEXTO, e o critério vai junto: a tela não deixa o
          leitor deduzir se é por área ou por demanda. */}
      {momento.predominante && (
        <p className="cad-sub" data-contexto="predominante">
          Principal cultura: <strong>{momento.predominante.cultura}</strong> ·{' '}
          {pt(momento.predominante.fatia, 0)}% da área relevante
          <InfoTooltip
            rotulo="Qual é o critério da principal cultura"
            texto={
              `Critério: ${momento.predominante.criterio}. ` +
              'Isto é CONTEXTO — responde "o que se planta aqui?" — e não entra no cálculo do momento: o fator ' +
              'de cada cultura pesa pela demanda que ela representa, não pela área. Trocar qual cultura tem a ' +
              'maior área muda esta linha e não muda o número acima.'
            }
          />
        </p>
      )}
    </div>
  );
}
