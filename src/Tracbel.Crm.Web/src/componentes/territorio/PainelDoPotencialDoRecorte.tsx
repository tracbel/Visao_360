/**
 * O POTENCIAL DO RECORTE CONSULTADO, pelo motor (issue 72; redesenhado na T4.6 e
 * na fidelidade às maquetes de 23/09/2026).
 *
 * Ele responde três perguntas que o número sozinho não responde: **de que máquina**
 * estamos falando (as categorias), **de qual cultura** vem o parque (as parcelas, com
 * quem divide a terra), e **que fatia de São Paulo** está aqui.
 *
 * A PRIMEIRA CAMADA É UM CARTÃO COM QUATRO NÚMEROS (maquete): o parque, em quantos
 * municípios ele está, a fatia da área plantada de SP e quantas máquinas se
 * renovam por ano — nessa ordem, com filete entre eles. A maquete escreve a
 * renovação em porcentagem; a grandeza verdadeira é máquinas por ano, e é ela
 * que aparece.
 *
 * A DECOMPOSIÇÃO INTEIRA CONTINUA NA TELA, atrás de "Ver composição" — o link
 * discreto embaixo do cartão, que abre categoria, cultura, relevância e a fatia do
 * valor de São Paulo. Era um `<details>` de largura inteira com o título "De onde
 * vem este parque…", e a maquete não tem nada ali. Nenhuma linha foi apagada.
 *
 * O AVISO DE ESTIMATIVA É O SELO DA MAQUETE, embaixo do nome do parque — e o
 * próprio selo abre a dica com o motivo.
 *
 * A demanda anual sai VAZIA COM O MOTIVO quando falta o ciclo de renovação de alguma
 * cultura: somar só as que têm daria um total menor que o real, com cara de completo.
 */

import { Leaf, MapPin, RefreshCw, Tractor, type LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';
import type { PotencialDoRecorteNoMapa } from '../../tipos/territorio';
import { ValorAusente } from '../comum/ValorAusente';
import { InfoTooltip } from '../InfoTooltip';
import { MOTIVO_SEM_PARQUE, nº, porcento } from './indicadoresDaAdr';

/** Uma célula do cartão: ícone, número, nome e — no parque — o selo embaixo. */
function Celula({
  icone: Icone,
  valor,
  rotulo,
  oQue,
  motivoSemDado,
  selo,
}: {
  icone: LucideIcon;
  valor: string | null;
  rotulo: string;
  /** Como o leitor de tela chama este número na dica de ausência. */
  oQue: string;
  motivoSemDado?: string;
  selo?: ReactNode;
}) {
  return (
    <div className="mv-potencial-celula">
      <span className="mv-icone-ladrilho" aria-hidden="true">
        <Icone size={17} strokeWidth={2} />
      </span>
      <div className="mv-potencial-corpo">
        <strong className="mv-potencial-valor">
          {valor === null ? <ValorAusente motivo={motivoSemDado ?? 'sem dado'} oQue={oQue} /> : valor}
        </strong>
        <span className="mv-potencial-rotulo">{rotulo}</span>
        {selo}
      </div>
    </div>
  );
}

export function PainelDoPotencialDoRecorte({
  recorte,
  comFiltro,
}: {
  recorte: PotencialDoRecorteNoMapa;
  comFiltro: boolean;
}) {
  const relevancia = recorte.relevanciaNoEstado;
  const fatiaDaArea = relevancia?.fatiaDaAreaPlantada ?? null;

  return (
    <div className="mv-potencial" data-bloco="potencial-do-recorte">
      <div className="mv-potencial-cartao">
        <Celula
          icone={Tractor}
          valor={recorte.parqueDeMaquinas == null ? null : nº(Math.round(recorte.parqueDeMaquinas))}
          rotulo="máquinas de parque"
          oQue="máquinas de parque"
          motivoSemDado={MOTIVO_SEM_PARQUE[recorte.motivoSemDemanda]}
          selo={
            recorte.frase ? (
              <InfoTooltip rotulo="Por que o parque é estimativa" texto={`${recorte.frase}.`}>
                <span className="dash-selo mv-selo-estimativa">estimativa</span>
              </InfoTooltip>
            ) : undefined
          }
        />

        <Celula
          icone={MapPin}
          valor={nº(recorte.municipiosComParque)}
          rotulo={`municípios com parque${comFiltro ? ', no filtro aplicado' : ''}`}
          oQue="os municípios com parque"
        />

        {/* A FATIA SÓ EXISTE COM DENOMINADOR: sem o total de SP a célula some,
            em vez de dizer "0% da área plantada". */}
        {relevancia && (
          <Celula
            icone={Leaf}
            valor={fatiaDaArea == null ? null : porcento(fatiaDaArea)}
            rotulo="da área plantada de SP"
            oQue="a fatia da área plantada de São Paulo"
            motivoSemDado="Sem área plantada apurada para o recorte no ano de referência."
          />
        )}

        <Celula
          icone={RefreshCw}
          valor={recorte.demandaAnualDeMaquinas == null ? null : nº(Math.round(recorte.demandaAnualDeMaquinas))}
          rotulo="renovadas por ano"
          oQue="a demanda anual"
          motivoSemDado={`${MOTIVO_SEM_PARQUE[recorte.motivoSemDemanda]}. A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro juntas.`}
        />
      </div>

      {/* "VER COMPOSIÇÃO" — o detalhe inteiro, recolhido. `<details>` nativo: o
          navegador já resolve teclado, leitor de tela e Ctrl+F dentro do bloco
          fechado — o que uma implementação em React perderia. */}
      <details className="mv-composicao" data-bloco="potencial-detalhe">
        <summary>Ver composição</summary>

        <div className="mv-composicao-corpo">
          {relevancia && (
            <p className="cad-sub">
              {`${fatiaDaArea == null ? '—' : porcento(fatiaDaArea)} da área plantada e ${
                relevancia.fatiaDoValor == null ? '—' : porcento(relevancia.fatiaDoValor)
              } do valor da produção do estado. O denominador é o total PUBLICADO pelo IBGE, que não é a soma dos municípios: o valor municipal sob sigilo entra no total do estado sem aparecer embaixo.`}
            </p>
          )}

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
            <div className="dash-tabela-rolagem">
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
            </div>
          )}
        </div>
      </details>
    </div>
  );
}
