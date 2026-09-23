/**
 * O POTENCIAL DO RECORTE CONSULTADO, pelo motor (issue 72; redesenhado na T4.6).
 *
 * Ele responde três perguntas que o número sozinho não responde: **de que máquina**
 * estamos falando (as categorias), **de qual cultura** vem o parque (as parcelas, com
 * quem divide a terra), e **que fatia de São Paulo** está aqui.
 *
 * O QUE MUDOU NA T4.6 — a ordem da leitura, e nada do conteúdo.
 *
 * Isto abria com a frase "No recorte consultado — 30 municípios com parque" e
 * descia numa lista de oito a quinze linhas de mesmo peso: o parque total, cada
 * categoria, cada cultura e a fatia de SP, tudo em corpo de texto. A resposta —
 * *quantas máquinas existem aqui* — estava na terceira linha, do mesmo tamanho da
 * décima. Era um relatório técnico ocupando o lugar de um painel.
 *
 * Agora a primeira camada é **dois números**: o parque e em quantos municípios
 * ele está. A decomposição inteira continua na tela, num detalhe recolhido — é a
 * hierarquia da issue 33, e nenhuma linha foi apagada.
 *
 * O AVISO DE ESTIMATIVA VIROU SELO. Ele era uma faixa amarela de largura inteira
 * no fim do painel, e dominava o bloco: a ressalva é importante, mas ela
 * qualifica o número, não substitui a leitura dele. Agora é um selo ao lado do
 * número, com o texto inteiro na dica.
 *
 * A demanda anual sai VAZIA COM O MOTIVO quando falta o ciclo de renovação de alguma
 * cultura: somar só as que têm daria um total menor que o real, com cara de completo.
 */

import { Leaf, MapPin, RotateCw, Tractor } from 'lucide-react';
import type { PotencialDoRecorteNoMapa } from '../../tipos/territorio';
import { ValorAusente } from '../comum/ValorAusente';
import { GradeDeMiniIndicadores, MiniIndicador } from '../dashboard/Dashboard';
import { InfoTooltip } from '../InfoTooltip';
import { MOTIVO_SEM_PARQUE, nº, porcento } from './indicadoresDaAdr';

export function PainelDoPotencialDoRecorte({
  recorte,
  comFiltro,
}: {
  recorte: PotencialDoRecorteNoMapa;
  comFiltro: boolean;
}) {
  const relevancia = recorte.relevanciaNoEstado;

  return (
    <div className="terr-recorte" data-bloco="potencial-do-recorte">
      {/* A PRIMEIRA CAMADA SÃO DOIS NÚMEROS. Quantas máquinas, e em quantos
          municípios — é a resposta da seção, e ela chega antes de qualquer
          decomposição. */}
      {/* OS QUATRO NÚMEROS VIRARAM MINI-CARTÕES (fase T4.9 — maquete): selo de
          ícone à esquerda, número, rótulo embaixo. Eram quatro números soltos
          numa linha, sem caixa e sem âncora — e na maquete cada um tem a sua.
          Nenhum número mudou, e nenhuma dica saiu. */}
      <GradeDeMiniIndicadores>
        <MiniIndicador
          rotulo="máquinas de parque"
          icone={Tractor}
          tom="demanda"
          valor={recorte.parqueDeMaquinas == null ? null : nº(Math.round(recorte.parqueDeMaquinas))}
          motivoSemDado={MOTIVO_SEM_PARQUE[recorte.motivoSemDemanda]}
          selo={
            recorte.frase ? (
              <>
                <span className="dash-selo">estimativa</span>
                <InfoTooltip rotulo="Por que o parque é estimativa" texto={`${recorte.frase}.`} />
              </>
            ) : undefined
          }
        />

        <MiniIndicador
          rotulo={`municípios com parque${comFiltro ? ', no filtro aplicado' : ''}`}
          oQue="os municípios com parque"
          icone={MapPin}
          valor={nº(recorte.municipiosComParque)}
        />

        <MiniIndicador
          rotulo="renovadas por ano"
          oQue="a demanda anual"
          icone={RotateCw}
          valor={recorte.demandaAnualDeMaquinas == null ? null : nº(Math.round(recorte.demandaAnualDeMaquinas))}
          motivoSemDado={`${MOTIVO_SEM_PARQUE[recorte.motivoSemDemanda]}. A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro juntas.`}
        />

        {relevancia && (
          <MiniIndicador
            rotulo="da área plantada de SP"
            oQue="a fatia da área plantada de São Paulo"
            icone={Leaf}
            valor={relevancia.fatiaDaAreaPlantada == null ? null : porcento(relevancia.fatiaDaAreaPlantada)}
            selo={
              <InfoTooltip
                rotulo="A fatia de São Paulo"
                texto={`${relevancia.fatiaDaAreaPlantada == null ? '—' : porcento(relevancia.fatiaDaAreaPlantada)} da área plantada e ${relevancia.fatiaDoValor == null ? '—' : porcento(relevancia.fatiaDoValor)} do valor da produção do estado. O denominador é o total PUBLICADO pelo IBGE, que não é a soma dos municípios: o valor municipal sob sigilo entra no total do estado sem aparecer embaixo.`}
              />
            }
          />
        )}
      </GradeDeMiniIndicadores>

      {/* A DECOMPOSIÇÃO INTEIRA CONTINUA NA TELA, recolhida. `<details>` nativo:
          o navegador já resolve teclado, leitor de tela e Ctrl+F dentro do bloco
          fechado — o que uma implementação em React perderia. */}
      <details className="cad-recolhivel" data-bloco="potencial-detalhe">
        <summary>De onde vem este parque — por categoria e por cultura</summary>

        <ul className="terr-recorte-linhas">
          {recorte.porCategoria.map((c) => (
            <li key={c.categoriaCodigo}>
              {c.categoriaNome}: <strong>{c.parqueDeMaquinas == null ? '—' : nº(Math.round(c.parqueDeMaquinas))}</strong>
              {c.demandaAnualDeMaquinas != null && (
                <span className="cad-sub"> · {nº(Math.round(c.demandaAnualDeMaquinas))} por ano</span>
              )}
            </li>
          ))}

          {recorte.porCultura.map((p) => (
            <li key={p.culturaCodigo}>
              {p.cultura}: {p.parque == null ? '—' : nº(Math.round(p.parque))} máquinas em{' '}
              {nº(Math.round(p.areaUtilHectares ?? 0))} ha
              {p.compartilhada.length > 0 && (
                <span className="cad-sub"> · área compartilhada com {p.compartilhada.join(', ')}</span>
              )}
            </li>
          ))}
        </ul>

        {recorte.relevanciaPorCultura.length > 0 && (
          <table className="cad-tabela terr-recorte-tabela">
            <caption className="cad-sub">Relevância por cultura, contra o total publicado de São Paulo</caption>
            <thead>
              <tr>
                <th scope="col">Cultura</th>
                <th scope="col">Ano</th>
                <th scope="col">Área plantada</th>
                <th scope="col">Quantidade</th>
                <th scope="col">Valor</th>
                <th scope="col">Produtividade × SP</th>
              </tr>
            </thead>
            <tbody>
              {recorte.relevanciaPorCultura.map((c) => (
                <tr key={c.produtoCodigoIbge}>
                  <td>{c.produtoNome}</td>
                  <td className="cad-mono">{c.ano}</td>
                  <td className="cad-mono">
                    {c.relevancia.fatiaDaAreaPlantada == null ? '—' : porcento(c.relevancia.fatiaDaAreaPlantada)}
                  </td>
                  <td className="cad-mono">
                    {c.relevancia.fatiaDaQuantidade == null ? '—' : porcento(c.relevancia.fatiaDaQuantidade)}
                  </td>
                  <td className="cad-mono">
                    {c.relevancia.fatiaDoValor == null ? '—' : porcento(c.relevancia.fatiaDoValor)}
                  </td>
                  {/* A RAZÃO NÃO LEVA FATIA: produtividade é razão, e o que se
                      compara é a distância até SP (documento 50, §7). */}
                  <td className="cad-mono">
                    {c.relevancia.razaoDeProdutividade == null ? (
                      <ValorAusente
                        motivo={`Sem produtividade apurada para ${c.produtoNome} — falta quantidade produzida ou área colhida no ano de referência.`}
                        oQue={`a produtividade de ${c.produtoNome}`}
                      />
                    ) : (
                      <>
                        {`${c.relevancia.razaoDeProdutividade.toLocaleString('pt-BR', { maximumFractionDigits: 2 })}×`}
                        {c.relevancia.produtividadeDoRecorte != null && (
                          <InfoTooltip
                            rotulo={`Como a produtividade de ${c.produtoNome} se compara com São Paulo`}
                            texto={`${nº(Math.round(c.relevancia.produtividadeDoRecorte))} aqui contra ${nº(Math.round(c.relevancia.produtividadeNoEstado ?? 0))} em São Paulo (${c.unidadeDaProdutividade}). É uma razão: o que se compara é a distância até a referência, não uma fatia.`}
                          />
                        )}
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </details>
    </div>
  );
}
