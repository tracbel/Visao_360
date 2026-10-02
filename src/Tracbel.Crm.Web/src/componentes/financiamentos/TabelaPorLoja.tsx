/**
 * O CRÉDITO POR LOJA (issue 261): linhas e valor de cada loja no período contra o mesmo período do ano anterior, o índice
 * (70% linhas, 30% valor) e a situação pela faixa do CRM.
 */

import { useMemo, useState } from 'react';
import { CabecalhoOrdenavel } from '../comum/CabecalhoOrdenavel';
import { ordenar, proximaOrdem, type Ordem } from '../comum/ordenacao';
import { reaisCurtos } from '../mercado/momento/formatos';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { CreditoDaLoja } from '../../tipos/mercado';
import { CelulaComAnterior, CelulaDaSituacao } from './CelulasDoCredito';
import { n, variacaoDaRazao } from './financiamentos';

type Coluna = 'loja' | 'linhas' | 'valor' | 'indice';

function valorDe(l: CreditoDaLoja, coluna: Coluna): number | string | null {
  switch (coluna) {
    case 'loja': return l.loja;
    case 'linhas': return l.janelas.linhas;
    case 'valor': return l.janelas.valor;
    case 'indice': return l.indice?.indice ?? null;
  }
}

export function TabelaPorLoja({ lojas }: { lojas: readonly CreditoDaLoja[] }) {
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'valor', sentido: -1 });
  const linhas = useMemo(() => ordenar(lojas, ordem, valorDe), [lojas, ordem]);
  const cab = { ordem, aoOrdenar: (coluna: Coluna, texto: boolean) => setOrdem((o) => proximaOrdem(o, coluna, texto)) };

  if (lojas.length === 0) return <p className="fin-nota">Nenhuma loja da ADR no recorte escolhido.</p>;

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela diag-tabela fin-tabela">
        <caption className="cad-so-leitor">O crédito de máquinas de cada loja, no período e no mesmo período do ano anterior</caption>
        <thead>
          <tr>
            <CabecalhoOrdenavel {...cab} coluna="loja" rotulo="Loja" texto />
            <CabecalhoOrdenavel {...cab} coluna="linhas" rotulo="Linhas" />
            <CabecalhoOrdenavel {...cab} coluna="valor" rotulo="Valor" />
            <CabecalhoOrdenavel {...cab} coluna="indice" rotulo="Índice" titulo="70% da variação das linhas e 30% da do valor" />
            <th scope="col">Situação</th>
          </tr>
        </thead>
        <tbody>
          {linhas.map((l) => (
            <tr key={l.lojaCodigo}>
              <th scope="row">
                {nomeProprio(l.loja)}
                <div className="cad-sub">{l.municipios === 1 ? '1 município' : `${n(l.municipios)} municípios`}</div>
              </th>
              <CelulaComAnterior atual={l.janelas.linhas} anterior={l.janelas.linhasAnteriores} formatar={(v) => n(v)} />
              <CelulaComAnterior atual={l.janelas.valor} anterior={l.janelas.valorAnterior} formatar={reaisCurtos} />
              <td className="mom-num">{variacaoDaRazao(l.indice?.indice)}</td>
              <CelulaDaSituacao indice={l.indice} janelas={l.janelas} />
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
