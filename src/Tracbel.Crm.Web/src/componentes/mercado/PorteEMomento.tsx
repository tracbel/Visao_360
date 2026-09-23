/**
 * PORTE E MOMENTO — dois números, nunca um (documento 50, §4.2; fase T3).
 *
 * O PORTE é o tamanho do mercado e muda devagar: safra, censo. O MOMENTO é o
 * fator de ciclo e muda todo mês. Misturá-los esconderia a leitura que a
 * diretoria precisa: *"estruturalmente grande, mas agora retraído"* é uma decisão
 * diferente de *"pequeno e aquecido"*.
 *
 * O PORTE NASCE SEM NOME, e isso não é omissão: nomear exige um corte, e um corte
 * sem dono é parâmetro inventado (R-27 do documento 46). As bandas são a issue
 * 166; enquanto forem nulas, o porte se expressa pelo NÚMERO e pela comparação.
 * **Nulo não é "pequeno".**
 *
 * AS SETAS SÃO TRÊS, e não quatro. O fator tem três sensibilidades (D-P05):
 * preço e rentabilidade, crédito e percepção comercial. O CUSTO entra DENTRO da
 * primeira — rentabilidade é preço menos custo —, e desenhar uma quarta seta para
 * ele afirmaria uma parcela independente que a decisão não criou. O termo de
 * troca ficou de fora porque precisa do preço de máquina (issue 70).
 */

import { InfoTooltip } from '../InfoTooltip';
import { Procedencia } from '../comum/Procedencia';
import { ValorAusente } from '../comum/ValorAusente';
import type { MomentoDoRecorte } from '../../tipos/territorio';

const pt = (v: number, casas = 2) => v.toLocaleString('pt-BR', { maximumFractionDigits: casas });

/**
 * A DIREÇÃO CARREGA O SIGNIFICADO, e não a cor.
 *
 * Quem não distingue verde de vermelho lê a seta igual. Perto de zero é "→":
 * uma parcela de 0,001 não empurra nada, e desenhar ↑ ali sugeriria um movimento
 * que não existe.
 */
function seta(parcela: number | null): string {
  if (parcela === null) return '·';
  if (parcela > 0.005) return '↑';
  if (parcela < -0.005) return '↓';
  return '→';
}

/** Uma parcela do fator, com a conta e a fonte na dica. */
function Parcela({ nome, parcela, explicacao }: { nome: string; parcela: number | null; explicacao: string }) {
  return (
    <span className="terr-parcela">
      <span className="terr-parcela-seta" aria-hidden="true">
        {seta(parcela)}
      </span>
      <span className="terr-parcela-nome">{nome}</span>
      <InfoTooltip
        rotulo={`Como ${nome.toLowerCase()} entra no fator`}
        texto={
          parcela === null
            ? `${explicacao} Sem índice carregado, esta parcela vale desvio ZERO — e não "indeterminado".`
            : `${explicacao} Contribuição neste recorte: ${parcela > 0 ? '+' : ''}${pt(parcela * 100, 1)}% sobre a demanda estrutural.`
        }
      />
    </span>
  );
}

export function PorteEMomento({ momento }: { momento: MomentoDoRecorte | null }) {
  if (!momento) return null;

  const fator = momento.potencial.fator;

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

        {/* MOMENTO — este já tem faixas decididas (issues 73 e 74) e pode ser nomeado. */}
        <span className="terr-leitura">
          <strong>Momento:</strong>{' '}
          {fator.fator === null ? (
            <ValorAusente motivo={momento.potencial.frase || fator.motivo} oQue="o momento do mercado" />
          ) : (
            <>
              {momento.faixaDoMomento ?? 'sem faixa'} <span className="cad-mono">{pt(fator.fator)}</span>
              <Procedencia procedencia={momento.procedencia} oQue="o momento do mercado" />
            </>
          )}
        </span>
      </div>

      {momento.leitura && <p className="terr-leitura-frase">{momento.leitura}</p>}

      {fator.fator !== null && (
        <div className="terr-parcelas" aria-label="Por que o mercado está neste momento">
          <Parcela
            nome="Commodity"
            parcela={fator.parcelaDePreco}
            explicacao={
              momento.culturaDoIndiceDePreco
                ? `Preço e rentabilidade, pela cultura de maior área do recorte (${momento.culturaDoIndiceDePreco}). O custo entra AQUI: rentabilidade é preço menos custo, e não uma quarta sensibilidade.`
                : 'Preço e rentabilidade da cultura de maior área do recorte. O custo entra aqui: rentabilidade é preço menos custo.'
            }
          />
          <Parcela
            nome="Crédito"
            parcela={fator.parcelaDeCredito}
            explicacao="Índice de crédito do SICOR — 70% linhas e 30% valor, na janela vigente. Ele MULTIPLICA o resto, porque é a condição de financiar: sem crédito, nem a melhor safra vira máquina."
          />
          <Parcela
            nome="Percepção"
            parcela={fator.parcelaDaPercepcao}
            explicacao="A leitura do comercial sobre o município, de −5 a +5 pontos percentuais, com autor e vigência (issue 71). Ela soma dentro do parêntese do produtor, então o crédito a amplifica."
          />
          {fator.cortadoPeloLimite && (
            <span className="terr-parcela">
              <span className="terr-parcela-nome">no limite</span>
              <InfoTooltip
                rotulo="Por que o fator foi cortado"
                texto={`O fator calculado foi ${pt(fator.fatorSemLimite ?? 0)} e o limite registrado o trouxe para ${pt(fator.fator)}. O limite é decisão com vigência, não arredondamento.`}
              />
            </span>
          )}
        </div>
      )}
    </div>
  );
}
