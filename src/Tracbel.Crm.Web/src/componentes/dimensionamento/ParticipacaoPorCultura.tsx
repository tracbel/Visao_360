/**
 * A PARTICIPAÇÃO POR CULTURA — Tracbel × Estado (issue 259): área, quantidade e valor de cada cultura no recorte, o total de
 * São Paulo e a fatia. Com uma loja escolhida, a última coluna diz quanto do valor da cultura na ADR é da loja.
 *
 * "OUTRAS" É O RESTO DA PAM, e a quantidade dela não soma o que o IBGE publica em mil frutos (abacaxi e coco): por isso ela
 * pode vir com área e valor, e a quantidade num traço.
 */

import { useMemo, useState } from 'react';
import type { CulturaTracbelNoEstado, DimensionamentoDaAdr } from '../../tipos/mercado';
import { CabecalhoOrdenavel } from './CabecalhoOrdenavel';
import { n, ordenar, pct, proximaOrdem, type Ordem } from './dimensionamento';

type Coluna = 'nome' | 'areaT' | 'areaSp' | 'areaPct' | 'qtdT' | 'qtdSp' | 'qtdPct' | 'valorT' | 'valorSp' | 'valorPct' | 'loja';

function valorDe(c: CulturaTracbelNoEstado, coluna: Coluna): number | string | null {
  switch (coluna) {
    case 'nome': return c.nome;
    case 'areaT': return c.area.parte;
    case 'areaSp': return c.area.todo;
    case 'areaPct': return c.area.percentual;
    case 'qtdT': return c.quantidade.parte;
    case 'qtdSp': return c.quantidade.todo;
    case 'qtdPct': return c.quantidade.percentual;
    case 'valorT': return c.valor.parte;
    case 'valorSp': return c.valor.todo;
    case 'valorPct': return c.valor.percentual;
    case 'loja': return c.fatiaDaLojaNaTracbel;
  }
}

/** A cor da fatia, como o protótipo: forte acima de 50%, média de 30% a 50%, fraca de 15% a 30%, e o resto. */
function tomDaFatia(v: number | null) {
  if (v === null) return undefined;
  return v >= 50 ? 'alta' : v >= 30 ? 'boa' : v >= 15 ? 'media' : 'baixa';
}

export function ParticipacaoPorCultura({ dados }: { dados: DimensionamentoDaAdr }) {
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'valorT', sentido: -1 });
  const linhas = useMemo(() => ordenar(dados.porCultura, ordem, valorDe), [dados.porCultura, ordem]);
  const comLoja = dados.representatividade !== null;
  const cab = { ordem, aoOrdenar: (coluna: Coluna, texto: boolean) => setOrdem((o) => proximaOrdem(o, coluna, texto)) };
  const num = (v: number | null) => (v === null ? '—' : n(v));
  const fatia = (v: number | null) => (
    <td className="mom-num dim-fatia" data-tom={tomDaFatia(v)}>
      {pct(v)}
    </td>
  );

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela diag-tabela dim-tabela">
        <caption className="cad-so-leitor">A participação de cada cultura do recorte no total de São Paulo</caption>
        <thead>
          <tr>
            <CabecalhoOrdenavel {...cab} coluna="nome" rotulo="Cultura" texto />
            <CabecalhoOrdenavel {...cab} coluna="areaT" rotulo="Área Tracbel" titulo="Área plantada no recorte (ha)" />
            <CabecalhoOrdenavel {...cab} coluna="areaSp" rotulo="Área SP" titulo="Área plantada em São Paulo (ha)" />
            <CabecalhoOrdenavel {...cab} coluna="areaPct" rotulo="%" titulo="Fatia da área" />
            <CabecalhoOrdenavel {...cab} coluna="qtdT" rotulo="Prod. Tracbel" titulo="Quantidade produzida no recorte (t)" />
            <CabecalhoOrdenavel {...cab} coluna="qtdSp" rotulo="Prod. SP" titulo="Quantidade produzida em São Paulo (t)" />
            <CabecalhoOrdenavel {...cab} coluna="qtdPct" rotulo="%" titulo="Fatia da quantidade" />
            <CabecalhoOrdenavel {...cab} coluna="valorT" rotulo="Valor Tracbel" titulo="Valor da produção no recorte (mil R$)" />
            <CabecalhoOrdenavel {...cab} coluna="valorSp" rotulo="Valor SP" titulo="Valor da produção em São Paulo (mil R$)" />
            <CabecalhoOrdenavel {...cab} coluna="valorPct" rotulo="%" titulo="Fatia do valor" />
            {comLoja && <CabecalhoOrdenavel {...cab} coluna="loja" rotulo="% da loja na Tracbel" titulo="Quanto do valor da cultura na ADR é da loja" />}
          </tr>
        </thead>
        <tbody>
          {linhas.map((c) => (
            <tr key={c.codigo}>
              <th scope="row">{c.nome}</th>
              <td className="mom-num">{num(c.area.parte)}</td>
              <td className="mom-num">{num(c.area.todo)}</td>
              {fatia(c.area.percentual)}
              <td className="mom-num">{num(c.quantidade.parte)}</td>
              <td className="mom-num">{num(c.quantidade.todo)}</td>
              {fatia(c.quantidade.percentual)}
              <td className="mom-num">{num(c.valor.parte)}</td>
              <td className="mom-num">{num(c.valor.todo)}</td>
              {fatia(c.valor.percentual)}
              {comLoja && <td className="mom-num dim-destaque">{pct(c.fatiaDaLojaNaTracbel)}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
