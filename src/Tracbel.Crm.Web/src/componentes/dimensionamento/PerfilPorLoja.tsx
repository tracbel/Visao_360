/**
 * O PERFIL COMERCIAL POR LOJA (issue 259): o tamanho do território de cada loja (municípios, área, valor), a densidade
 * econômica (R$ por hectare), a tecnificação e a cultura dominante.
 *
 * A TECNIFICAÇÃO É A PRODUTIVIDADE DA LOJA contra a média do recorte, numa cultura só — produtividade de cana e de café
 * não se comparam. Acima de 105% a loja colhe mais por hectare que a média; abaixo de 95%, menos.
 */

import { useMemo, useState } from 'react';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { DimensionamentoDaAdr, PerfilDaLoja } from '../../tipos/mercado';
import { CabecalhoOrdenavel } from './CabecalhoOrdenavel';
import { n, ordenar, pct, proximaOrdem, reais, type Ordem } from './dimensionamento';

type Coluna = 'loja' | 'municipios' | 'area' | 'valor' | 'reaisPorHectare' | 'tecnificacao' | 'culturaDominante';

const valorDe = (l: PerfilDaLoja, coluna: Coluna): number | string | null => l[coluna];

export function PerfilPorLoja({ dados }: { dados: DimensionamentoDaAdr }) {
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'valor', sentido: -1 });
  const linhas = useMemo(() => ordenar(dados.porLoja, ordem, valorDe), [dados.porLoja, ordem]);
  const cab = { ordem, aoOrdenar: (coluna: Coluna, texto: boolean) => setOrdem((o) => proximaOrdem(o, coluna, texto)) };

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela diag-tabela dim-tabela">
        <caption className="cad-so-leitor">O perfil comercial de cada loja do recorte</caption>
        <thead>
          <tr>
            <CabecalhoOrdenavel {...cab} coluna="loja" rotulo="Loja" texto />
            <CabecalhoOrdenavel {...cab} coluna="municipios" rotulo="Munic." titulo="Municípios da loja no recorte" />
            <CabecalhoOrdenavel {...cab} coluna="area" rotulo="Área (ha)" />
            <CabecalhoOrdenavel {...cab} coluna="valor" rotulo="Valor (mil R$)" />
            <CabecalhoOrdenavel {...cab} coluna="reaisPorHectare" rotulo="R$/ha" titulo="Densidade econômica: valor por hectare plantado" />
            <CabecalhoOrdenavel {...cab} coluna="tecnificacao" rotulo="Tecnificação" titulo="Produtividade da loja em % da média do recorte" />
            <CabecalhoOrdenavel {...cab} coluna="culturaDominante" rotulo="Cultura dominante" texto />
          </tr>
        </thead>
        <tbody>
          {linhas.map((l) => (
            <tr key={l.lojaCodigo ?? 'sem-loja'}>
              <th scope="row">{l.lojaCodigo ? nomeProprio(l.loja) : l.loja}</th>
              <td className="mom-num">{n(l.municipios)}</td>
              <td className="mom-num">{l.area === null ? '—' : n(l.area)}</td>
              <td className="mom-num">{l.valor === null ? '—' : n(l.valor)}</td>
              <td className="mom-num">{reais(l.reaisPorHectare === null ? null : Math.round(l.reaisPorHectare))}</td>
              <td
                className="mom-num dim-tecnificacao"
                data-tom={l.tecnificacao === null ? undefined : l.tecnificacao >= 105 ? 'acima' : l.tecnificacao < 95 ? 'abaixo' : 'media'}
              >
                {pct(l.tecnificacao)}
              </td>
              <td>{l.culturaDominante ?? '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
