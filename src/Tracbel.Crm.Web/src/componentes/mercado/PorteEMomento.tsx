/**
 * PORTE E MOMENTO — uma faixa executiva, não duas frases soltas (T4.6).
 *
 * O PORTE é o tamanho do mercado e muda devagar: safra, censo. O MOMENTO é o
 * fator de ciclo e muda todo mês. Misturá-los esconderia a leitura que a
 * diretoria precisa: *"estruturalmente grande, mas agora retraído"* é uma decisão
 * diferente de *"pequeno e aquecido"*.
 *
 * O QUE MUDOU NA T4.6. Isto era uma linha de texto corrido — `Porte: — Momento:
 * —` — solta entre dois blocos, sem composição visual: quando os dois lados
 * estavam vazios, sobrava um par de travessões que não dizia nada a quem passasse
 * o olho. Agora é uma faixa com a faixa do momento em destaque, o número abaixo
 * dela e o porte ao lado **somente quando houver classificação válida**.
 *
 * O PORTE NASCE SEM NOME, e isso não é omissão: nomear exige um corte, e um corte
 * sem dono é parâmetro inventado (R-27 do documento 46). As bandas são a issue
 * 166. **Nulo não é "pequeno".** Sem nome, o lugar dele simplesmente não é
 * desenhado — em vez de um travessão ocupando espaço de destaque.
 *
 * AS TRÊS SETAS NÃO ESTÃO AQUI, e continua sendo de propósito (T3.1). Preço e
 * rentabilidade, crédito e percepção são calculados POR CULTURA; uma seta
 * agregada exigiria decompor `Σ ajustada ÷ Σ estrutural` em três pedaços que o
 * domínio não produz. Elas estão no bloco "Momento do mercado", uma linha por
 * cultura — que é onde elas existem de verdade, e para onde o ⓘ daqui aponta.
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

  const semFator = momento.fatorAgregado === null;

  return (
    <div className="dash-momento" data-bloco="porte-e-momento">
      <div className="dash-momento-principal">
        <span className="dash-momento-rotulo">Momento do mercado</span>

        {semFator ? (
          <span className="dash-momento-faixa dash-momento-faixa-vazia">
            <ValorAusente motivo={motivoEmPortugues(momento.motivoSemFator)} oQue="o momento do mercado" />
          </span>
        ) : (
          <>
            <span className="dash-momento-faixa">{momento.faixaDoMomento ?? 'sem faixa'}</span>
            <span className="dash-momento-numero">
              {pt(momento.fatorAgregado!)}
              <Procedencia procedencia={momento.procedencia} oQue="o momento do mercado" />
              {/* O ⓘ QUE LEVA À CONTA. As três parcelas não cabem aqui porque são
                  de cada cultura; quem quiser vê-las abre o bloco. */}
              <InfoTooltip
                rotulo="Como o momento do mercado é composto"
                texto={
                  'Este número é a razão entre a demanda ajustada somada e a demanda estrutural somada — cada ' +
                  'cultura pesa pela demanda que representa. As três parcelas (rentabilidade, crédito e percepção) ' +
                  'são calculadas POR CULTURA e estão abertas em "Momento do mercado", na aba "Composição do ' +
                  'fator", uma linha por cultura. Não há três setas aqui em cima porque não existe uma ' +
                  'decomposição agregada: inventá-la só para desenhar as setas seria um número sem conta.'
                }
              />
            </span>
          </>
        )}
      </div>

      {/* O PORTE SÓ OCUPA ESPAÇO QUANDO TEM NOME. Sem banda registrada (issue
          166), desenhar "Porte: —" num lugar de destaque daria peso visual a uma
          ausência — e nulo aqui não quer dizer "pequeno". */}
      {momento.porte ? (
        <div className="dash-momento-porte">
          <span className="dash-momento-rotulo">Porte estrutural</span>
          <span className="dash-momento-faixa">{momento.porte}</span>
        </div>
      ) : (
        <div className="dash-momento-porte dash-momento-porte-sem-nome">
          <span className="dash-momento-rotulo">Porte estrutural</span>
          <ValorAusente
            motivo={
              'O nome do porte — pequeno, médio ou grande — depende de bandas registradas, e elas ainda não ' +
              'foram decididas (issue 166). Um corte sem dono é parâmetro inventado, então a tela mostra o ' +
              'número da demanda anual acima e não dá nome. Nulo aqui NÃO quer dizer "pequeno".'
            }
            oQue="o nome do porte"
          />
        </div>
      )}

      <div className="dash-momento-leitura">
        {momento.leitura && <p className="dash-momento-frase">{momento.leitura}</p>}

        {/* A PREDOMINANTE É CONTEXTO, e o critério vai junto: a tela não deixa o
            leitor deduzir se é por área ou por demanda. */}
        {momento.predominante && (
          <p className="dash-momento-contexto" data-contexto="predominante">
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
    </div>
  );
}
