/**
 * A CARTEIRA DE CLIENTES DO RECORTE (issue 259) — os dois quadros do protótipo: quantos clientes, onde (na região ou fora),
 * de que classe e o potencial médio; e quantos foram cobertos em 30, 60, 90 e 120 dias, com o A/B em risco.
 *
 * AS FAIXAS SÃO ACUMULADAS: quem foi contatado há 20 dias está nas quatro. Cada número é "que fatia da carteira foi coberta
 * nesse prazo", e o percentual embaixo é sobre o total da carteira.
 *
 * O SEM CLASSE É CONTADO À PARTE: é o cliente sem faturamento apurado, e chamá-lo de D diria que ele compra pouco quando
 * ele nunca comprou.
 */

import type { DimensionamentoDaAdr } from '../../tipos/mercado';
import { dataCurta, n, pct } from './dimensionamento';

function Numero({ rotulo, valor, apoio, tom }: { rotulo: string; valor: string; apoio?: string; tom?: 'forte' | 'bom' | 'alerta' }) {
  return (
    <div className="dim-numero" data-tom={tom}>
      <span className="dim-numero-valor">{valor}</span>
      <span className="dim-numero-rotulo">{rotulo}</span>
      {apoio && <span className="dim-numero-apoio">{apoio}</span>}
    </div>
  );
}

export function CarteiraDoRecorte({ dados }: { dados: DimensionamentoDaAdr }) {
  const { clientes, naRegiao, fora, potencialMedio } = dados.carteira;
  const total = clientes.clientes;
  const daCarteira = (x: number) => (total > 0 ? `${pct((100 * x) / total)} da carteira` : '—');

  return (
    <div className="dim-carteira" data-bloco="carteira-do-recorte">
      <div className="dim-numeros" role="group" aria-label="Clientes da carteira por região e classe">
        <Numero rotulo="Total da carteira" valor={n(total)} tom="forte" apoio={fora === null ? 'clientes do recorte' : 'na região + fora'} />
        <Numero rotulo="Na região Tracbel" valor={n(naRegiao)} tom="bom" />
        <Numero rotulo="Fora da região" valor={fora === null ? '—' : n(fora)} apoio={fora === null ? 'só sem filtro de território' : undefined} />
        <Numero rotulo="A" valor={n(clientes.a)} />
        <Numero rotulo="B" valor={n(clientes.b)} />
        <Numero rotulo="C" valor={n(clientes.c)} />
        <Numero rotulo="D" valor={n(clientes.d)} />
        <Numero rotulo="Sem classe" valor={n(clientes.semClasse)} apoio="sem faturamento" />
        <Numero rotulo="Potencial médio" valor={potencialMedio === null ? '—' : n(potencialMedio, 2)} apoio="A = 4 … D = 1" />
      </div>
      <div className="dim-numeros" role="group" aria-label="Clientes cobertos por faixa de dias desde o último contato">
        <Numero rotulo="Cobertos em 30 dias" valor={n(clientes.faixas.ate30)} apoio={daCarteira(clientes.faixas.ate30)} tom="bom" />
        <Numero rotulo="Cobertos em 60 dias" valor={n(clientes.faixas.ate60)} apoio={daCarteira(clientes.faixas.ate60)} />
        <Numero rotulo="Cobertos em 90 dias" valor={n(clientes.faixas.ate90)} apoio={daCarteira(clientes.faixas.ate90)} />
        <Numero rotulo="Cobertos em 120 dias" valor={n(clientes.faixas.ate120)} apoio={daCarteira(clientes.faixas.ate120)} />
        <Numero rotulo="Sem contato em 120 dias" valor={n(clientes.faixas.sem120)} apoio={daCarteira(clientes.faixas.sem120)} />
        <Numero rotulo="A/B em risco" valor={n(clientes.abSemContato)} apoio="A e B sem contato em 120 dias" tom="alerta" />
        <Numero rotulo="Último contato" valor={dataCurta(clientes.ultimoContatoEm)} apoio="o mais recente da carteira" />
      </div>
    </div>
  );
}
