/**
 * A MATRIZ MUNICIPAL (issue 259) — as 21 colunas do protótipo, município a município: loja, vendedor, cultura principal,
 * área, quantidade e valor no ano-base e no anterior com a variação, a participação no recorte e em São Paulo, e os
 * clientes por classe com o último contato.
 *
 * O ANO ANTERIOR MORA NA MESMA CÉLULA, embaixo do ano-base, com a variação: três colunas por medida não cabiam na tela de
 * 1.536 px sem rolagem lateral, e esconder o ano anterior seria tirar informação. Pelo mesmo motivo, a loja, a cultura principal
 * e as usinas vão na linha de baixo do município. O CSV traz todas as colunas separadas.
 */

import { useMemo, useState } from 'react';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { DimensionamentoDaAdr, LinhaDaMatrizMunicipal, MedidaComAnterior } from '../../tipos/mercado';
import { CabecalhoOrdenavel } from './CabecalhoOrdenavel';
import { dataCurta, n, ordenar, pct, proximaOrdem, sentidoDe, variacao, type Ordem } from './dimensionamento';

type Coluna =
  | 'nome' | 'vendedor' | 'area' | 'quantidade' | 'valor' | 'fatiaNaRegiao' | 'fatiaNoEstado'
  | 'clientes' | 'a' | 'b' | 'c' | 'd' | 'ultimo';

function valorDe(m: LinhaDaMatrizMunicipal, coluna: Coluna): number | string | null {
  switch (coluna) {
    case 'nome': return m.nome;
    case 'vendedor': return m.vendedor;
    case 'area': return m.area.atual;
    case 'quantidade': return m.quantidade.atual;
    case 'valor': return m.valor.atual;
    case 'fatiaNaRegiao': return m.fatiaNaRegiao;
    case 'fatiaNoEstado': return m.fatiaNoEstado;
    case 'clientes': return m.clientes.clientes;
    case 'a':
    case 'b':
    case 'c':
    case 'd':
      return m.clientes[coluna];
    case 'ultimo': return m.clientes.ultimoContatoEm;
  }
}

function Medida({ medida, anoAnterior }: { medida: MedidaComAnterior; anoAnterior: number | null }) {
  return (
    <td className="mom-num dim-medida">
      <span className="dim-medida-atual">{medida.atual === null ? '—' : n(medida.atual)}</span>
      <span className="dim-medida-anterior">
        {anoAnterior ?? 'antes'}: {medida.anterior === null ? '—' : n(medida.anterior)}
        {medida.variacaoPercentual !== null && (
          <>
            {' · '}
            <span className={`diag-seta ${sentidoDe(medida.variacaoPercentual)}`}>{variacao(medida.variacaoPercentual)}</span>
          </>
        )}
      </span>
    </td>
  );
}

export function MatrizMunicipal({ dados, busca }: { dados: DimensionamentoDaAdr; busca: string }) {
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'valor', sentido: -1 });
  const [tamanho, setTamanho] = useState(25);

  const linhas = useMemo(() => {
    const termo = busca.trim().toLocaleLowerCase('pt-BR');
    const filtradas = termo ? dados.matriz.filter((m) => m.nome.toLocaleLowerCase('pt-BR').includes(termo)) : dados.matriz;
    return ordenar(filtradas, ordem, valorDe);
  }, [dados.matriz, busca, ordem]);

  // A PÁGINA VOLTA PARA A PRIMEIRA quando a lista ou a ordem muda — derivado na renderização, como a matriz da Demanda.
  const chave = `${linhas.length}|${ordem.coluna}|${ordem.sentido}`;
  const [escolha, setEscolha] = useState({ chave, pagina: 1 });
  const pagina = escolha.chave === chave ? escolha.pagina : 1;
  const setPagina = (p: number) => setEscolha({ chave, pagina: p });

  const totalDePaginas = Math.max(1, Math.ceil(linhas.length / tamanho));
  const atual = Math.min(pagina, totalDePaginas);
  const daPagina = linhas.slice((atual - 1) * tamanho, atual * tamanho);
  const cab = { ordem, aoOrdenar: (coluna: Coluna, texto: boolean) => setOrdem((o) => proximaOrdem(o, coluna, texto)) };
  const ano = dados.anoBase ?? '';

  return (
    <>
      <div className="mom-tabela-rolagem">
        <table className="mom-tabela diag-tabela dim-tabela dim-matriz">
          <caption className="cad-so-leitor">
            Os municípios do recorte: produção no ano-base e no anterior, participação e clientes da carteira
          </caption>
          <thead>
            <tr>
              <CabecalhoOrdenavel {...cab} coluna="nome" rotulo="Município" texto />
              <CabecalhoOrdenavel {...cab} coluna="vendedor" rotulo="Vendedor" texto titulo="O responsável com mais vínculos no município" />
              <CabecalhoOrdenavel {...cab} coluna="area" rotulo={`Área ${ano} (ha)`} />
              <CabecalhoOrdenavel {...cab} coluna="quantidade" rotulo={`Qtd ${ano} (t)`} />
              <CabecalhoOrdenavel {...cab} coluna="valor" rotulo={`Valor ${ano} (mil R$)`} />
              <CabecalhoOrdenavel {...cab} coluna="fatiaNaRegiao" rotulo="Part. reg." titulo="Quanto do valor do recorte é deste município" />
              <CabecalhoOrdenavel {...cab} coluna="fatiaNoEstado" rotulo="Part. SP" titulo="Quanto do valor de São Paulo é deste município" />
              <CabecalhoOrdenavel {...cab} coluna="clientes" rotulo="Clientes" />
              <CabecalhoOrdenavel {...cab} coluna="a" rotulo="A" />
              <CabecalhoOrdenavel {...cab} coluna="b" rotulo="B" />
              <CabecalhoOrdenavel {...cab} coluna="c" rotulo="C" />
              <CabecalhoOrdenavel {...cab} coluna="d" rotulo="D" />
              <CabecalhoOrdenavel {...cab} coluna="ultimo" rotulo="Últ. contato" />
            </tr>
          </thead>
          <tbody>
            {daPagina.map((m) => (
              <tr key={m.codigoIbge}>
                <th scope="row">
                  <span className="dim-municipio">{m.nome}</span>
                  <div className="cad-sub">
                    {m.loja ? nomeProprio(m.loja) : 'Sem loja'}
                    {m.culturaPrincipal && <> · {m.culturaPrincipal}</>}
                    {m.usinas > 0 && <> · {m.usinas === 1 ? '1 usina' : `${m.usinas} usinas`}</>}
                  </div>
                </th>
                <td>{m.vendedor ? nomeProprio(m.vendedor) : '—'}</td>
                <Medida medida={m.area} anoAnterior={dados.anoAnterior} />
                <Medida medida={m.quantidade} anoAnterior={dados.anoAnterior} />
                <Medida medida={m.valor} anoAnterior={dados.anoAnterior} />
                <td className="mom-num">{pct(m.fatiaNaRegiao, 2)}</td>
                <td className="mom-num dim-destaque">{pct(m.fatiaNoEstado, 2)}</td>
                <td className="mom-num">{n(m.clientes.clientes)}</td>
                <td className="mom-num dim-classe-a">{n(m.clientes.a)}</td>
                <td className="mom-num">{n(m.clientes.b)}</td>
                <td className="mom-num">{n(m.clientes.c)}</td>
                <td className="mom-num">{n(m.clientes.d)}</td>
                <td className="mom-num">{dataCurta(m.clientes.ultimoContatoEm)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <BarraDePaginacao
        pagina={{ itens: daPagina, pagina: atual, tamanho, total: linhas.length, totalDePaginas, temProxima: atual < totalDePaginas }}
        oQue="municípios"
        aoTrocarPagina={setPagina}
        aoTrocarTamanho={(t) => {
          setTamanho(t);
          setPagina(1);
        }}
      />
    </>
  );
}
