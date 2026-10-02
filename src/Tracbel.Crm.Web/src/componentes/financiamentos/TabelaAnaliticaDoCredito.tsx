/**
 * A TABELA ANALÍTICA (issue 261) — as 14 colunas do protótipo, município a município: valor, linhas e valor médio por linha
 * no período e no mesmo período do ano anterior, com as variações; a fatia de São Paulo e quanto ela mudou; o índice e a
 * situação.
 *
 * O ANO ANTERIOR MORA NA MESMA CÉLULA, embaixo do período, com a variação: três colunas por medida não cabiam na tela de
 * 1.536 px sem rolagem lateral. O CSV traz todas as colunas separadas.
 *
 * VALOR MÉDIO POR LINHA, E NÃO TICKET: a linha do SICOR já é a soma dos contratos de uma combinação de município, produto e
 * programa.
 */

import { useMemo, useState } from 'react';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import { CabecalhoOrdenavel } from '../comum/CabecalhoOrdenavel';
import { ordenar, proximaOrdem, type Ordem } from '../comum/ordenacao';
import { pontosPercentuais, reaisCurtos } from '../mercado/momento/formatos';
import { nomeProprio } from '../../telas/cadastro/formato';
import type { CreditoDoMunicipio } from '../../tipos/mercado';
import { CelulaComAnterior, CelulaDaSituacao } from './CelulasDoCredito';
import { medioPorLinha, n, variacaoDaRazao } from './financiamentos';

type Coluna = 'nome' | 'valor' | 'linhas' | 'medio' | 'fatia' | 'variacaoDaFatia' | 'indice';

function valorDe(m: CreditoDoMunicipio, coluna: Coluna): number | string | null {
  switch (coluna) {
    case 'nome': return m.nome;
    case 'valor': return m.janelas.valor;
    case 'linhas': return m.janelas.linhas;
    case 'medio': return medioPorLinha(m.janelas.valor, m.janelas.linhas);
    case 'fatia': return m.fatiaNoEstado;
    case 'variacaoDaFatia': return m.variacaoDaFatia;
    case 'indice': return m.indice?.indice ?? null;
  }
}

export function TabelaAnaliticaDoCredito({ municipios, busca }: { municipios: readonly CreditoDoMunicipio[]; busca: string }) {
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'valor', sentido: -1 });
  const [tamanho, setTamanho] = useState(25);

  const linhas = useMemo(() => {
    const termo = busca.trim().toLocaleLowerCase('pt-BR');
    const filtradas = termo ? municipios.filter((m) => m.nome.toLocaleLowerCase('pt-BR').includes(termo)) : municipios;
    return ordenar(filtradas, ordem, valorDe);
  }, [municipios, busca, ordem]);

  // A PÁGINA VOLTA PARA A PRIMEIRA quando a lista ou a ordem muda — derivado na renderização, como a matriz da Demanda.
  const chave = `${linhas.length}|${ordem.coluna}|${ordem.sentido}`;
  const [escolha, setEscolha] = useState({ chave, pagina: 1 });
  const pagina = escolha.chave === chave ? escolha.pagina : 1;
  const setPagina = (p: number) => setEscolha({ chave, pagina: p });

  const totalDePaginas = Math.max(1, Math.ceil(linhas.length / tamanho));
  const atual = Math.min(pagina, totalDePaginas);
  const daPagina = linhas.slice((atual - 1) * tamanho, atual * tamanho);
  const cab = { ordem, aoOrdenar: (coluna: Coluna, texto: boolean) => setOrdem((o) => proximaOrdem(o, coluna, texto)) };

  if (municipios.length === 0) return <p className="fin-nota">Nenhum município do recorte teve crédito de máquina no período nem no ano anterior.</p>;

  return (
    <>
      <div className="mom-tabela-rolagem">
        <table className="mom-tabela diag-tabela fin-tabela fin-analitica">
          <caption className="cad-so-leitor">O crédito de máquinas de cada município, no período e no mesmo período do ano anterior</caption>
          <thead>
            <tr>
              <CabecalhoOrdenavel {...cab} coluna="nome" rotulo="Município" texto />
              <CabecalhoOrdenavel {...cab} coluna="valor" rotulo="Valor" />
              <CabecalhoOrdenavel {...cab} coluna="linhas" rotulo="Linhas" />
              <CabecalhoOrdenavel {...cab} coluna="medio" rotulo="Valor médio por linha" titulo="Valor ÷ linhas — não é ticket médio por contrato" />
              <CabecalhoOrdenavel {...cab} coluna="fatia" rotulo="Fatia de SP" titulo="Quanto do valor de São Paulo no período é deste município" />
              <CabecalhoOrdenavel {...cab} coluna="variacaoDaFatia" rotulo="Δ fatia" titulo="Quantos pontos a fatia mudou contra o ano anterior" />
              <CabecalhoOrdenavel {...cab} coluna="indice" rotulo="Índice" titulo="70% da variação das linhas e 30% da do valor" />
              <th scope="col">Situação</th>
            </tr>
          </thead>
          <tbody>
            {daPagina.map((m) => {
              const j = m.janelas;
              return (
                <tr key={m.codigoIbge}>
                  <th scope="row">
                    <span className="fin-municipio">{m.nome}</span>
                    {!m.pertenceAAdr && <span className="fin-selo">fora</span>}
                    {m.loja && <div className="cad-sub">{nomeProprio(m.loja)}</div>}
                  </th>
                  <CelulaComAnterior atual={j.valor} anterior={j.valorAnterior} formatar={reaisCurtos} />
                  <CelulaComAnterior atual={j.linhas} anterior={j.linhasAnteriores} formatar={(v) => n(v)} />
                  <CelulaComAnterior
                    atual={medioPorLinha(j.valor, j.linhas)}
                    anterior={medioPorLinha(j.valorAnterior, j.linhasAnteriores)}
                    formatar={reaisCurtos}
                  />
                  <td className="mom-num">{m.fatiaNoEstado === null ? '—' : `${n(m.fatiaNoEstado, 2)}%`}</td>
                  <td className="mom-num">{m.variacaoDaFatia === null ? '—' : pontosPercentuais(m.variacaoDaFatia, 2)}</td>
                  <td className="mom-num">{variacaoDaRazao(m.indice?.indice)}</td>
                  <CelulaDaSituacao indice={m.indice} janelas={j} />
                </tr>
              );
            })}
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
