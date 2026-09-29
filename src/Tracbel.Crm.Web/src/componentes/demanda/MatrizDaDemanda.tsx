/**
 * A MATRIZ MUNICÍPIO × POTENCIAL (issue 258) — a tabela do protótipo: área, parque, a demanda de cada cultura, o total,
 * o que a Tracbel tem de entregar e, com o momento, os dois fatores e o efeito líquido.
 *
 * AS CULTURAS SÃO COLUNAS, na ordem da demanda do recorte: a primeira coluna é a cultura que mais pede máquina. Os dois
 * fatores podem ir para lados opostos — preço em baixa e crédito em alta —, e é por isso que eles vêm separados.
 */

import { useMemo, useState } from 'react';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import type { DemandaDoMunicipioNaPrevisao, DemandaEPrevisaoDaRegiao } from '../../tipos/mercado';
import { n, variacaoDoFator, variacaoPercentual } from './demanda';

type Coluna = 'nome' | 'areaUtilHectares' | 'parque' | 'demandaEstrutural' | 'demandaAjustada' | 'aEntregar' | string;

function valorDe(m: DemandaDoMunicipioNaPrevisao, coluna: Coluna): number | string | null {
  if (coluna === 'nome') return m.nome;
  if (coluna.startsWith('cultura:')) return m.porCultura.find((c) => c.culturaCodigo === coluna.slice(8))?.demanda ?? null;
  const v = (m as unknown as Record<string, unknown>)[coluna];
  return typeof v === 'number' || typeof v === 'string' ? v : null;
}

type Ordem = { coluna: Coluna; sentido: 1 | -1 };

function Cabecalho({
  coluna,
  rotulo,
  texto,
  ordem,
  aoOrdenar,
}: {
  coluna: Coluna;
  rotulo: string;
  texto?: boolean;
  ordem: Ordem;
  aoOrdenar: (coluna: Coluna, texto?: boolean) => void;
}) {
  return (
    <th scope="col" className={texto ? undefined : 'mom-num'} aria-sort={ordem.coluna === coluna ? (ordem.sentido === 1 ? 'ascending' : 'descending') : 'none'}>
      <button type="button" className="diag-ordenar" onClick={() => aoOrdenar(coluna, texto)}>
        {rotulo}
        {ordem.coluna === coluna && <span aria-hidden="true">{ordem.sentido === 1 ? ' ▲' : ' ▼'}</span>}
      </button>
    </th>
  );
}

export function MatrizDaDemanda({ dados, busca }: { dados: DemandaEPrevisaoDaRegiao; busca: string }) {
  const [ordem, setOrdem] = useState<Ordem>({ coluna: 'demandaEstrutural', sentido: -1 });
  const [tamanho, setTamanho] = useState(25);
  const comAjustada = dados.totais.demandaAjustada !== null;
  const comShare = dados.totais.aEntregar !== null;

  const linhas = useMemo(() => {
    const termo = busca.trim().toLocaleLowerCase('pt-BR');
    return dados.municipios
      .filter((m) => !termo || m.nome.toLocaleLowerCase('pt-BR').includes(termo))
      .sort((a, b) => {
        const va = valorDe(a, ordem.coluna);
        const vb = valorDe(b, ordem.coluna);
        if (va === null && vb === null) return 0;
        if (va === null) return 1;
        if (vb === null) return -1;
        if (typeof va === 'string' && typeof vb === 'string') return ordem.sentido * va.localeCompare(vb, 'pt-BR');
        return ordem.sentido * ((va as number) - (vb as number));
      });
  }, [dados.municipios, busca, ordem]);

  // A PÁGINA VOLTA PARA A PRIMEIRA quando a lista ou a ordem muda — derivado na renderização, e não num efeito: a página
  // guardada vale só para a lista em que foi escolhida.
  const chave = `${linhas.length}|${String(ordem.coluna)}|${ordem.sentido}`;
  const [escolha, setEscolha] = useState({ chave, pagina: 1 });
  const pagina = escolha.chave === chave ? escolha.pagina : 1;
  const setPagina = (p: number) => setEscolha({ chave, pagina: p });

  const totalDePaginas = Math.max(1, Math.ceil(linhas.length / tamanho));
  const atual = Math.min(pagina, totalDePaginas);
  const daPagina = linhas.slice((atual - 1) * tamanho, atual * tamanho);

  const ordenarPor = (coluna: Coluna, texto = false) =>
    setOrdem((o) => (o.coluna === coluna ? { coluna, sentido: o.sentido === 1 ? -1 : 1 } : { coluna, sentido: texto ? 1 : -1 }));
  const cab = { ordem, aoOrdenar: ordenarPor };

  const num = (v: number | null, casas = 1) => (v === null ? '—' : n(v, casas));

  return (
    <>
      <div className="mom-tabela-rolagem">
        <table className="mom-tabela diag-tabela dem-matriz">
          <caption className="cad-so-leitor">A demanda de máquinas de cada município da ADR, por cultura</caption>
          <thead>
            <tr>
              <Cabecalho {...cab} coluna="nome" rotulo="Município" texto />
              <Cabecalho {...cab} coluna="areaUtilHectares" rotulo="Área (ha)" />
              <Cabecalho {...cab} coluna="parque" rotulo="Parque" />
              {dados.culturas.map((c) => (
                <Cabecalho {...cab} key={c.codigo} coluna={`cultura:${c.codigo}`} rotulo={c.nome} />
              ))}
              <Cabecalho {...cab} coluna="demandaEstrutural" rotulo="Demanda/ano" />
              {comAjustada && <Cabecalho {...cab} coluna="demandaAjustada" rotulo="Ajustada" />}
              {comShare && <Cabecalho {...cab} coluna="aEntregar" rotulo={`A entregar ${dados.shareAlvo !== null ? `${n(dados.shareAlvo, 0)}%` : ''}`} />}
              {comAjustada && (
                <>
                  <th scope="col" className="mom-num">Fator preço</th>
                  <th scope="col" className="mom-num">Fator crédito</th>
                  <th scope="col" className="mom-num">Efeito líquido</th>
                </>
              )}
            </tr>
          </thead>
          <tbody>
            {daPagina.map((m) => (
              <tr key={m.codigoIbge}>
                <td>
                  <span className="dem-municipio">{m.nome}</span>
                  <div className="cad-sub">
                    {m.loja ?? m.regiao}
                    {m.culturaPredominante && <> · {m.culturaPredominante}</>}
                  </div>
                </td>
                <td className="mom-num">{num(m.areaUtilHectares, 0)}</td>
                <td className="mom-num">{num(m.parque, 0)}</td>
                {dados.culturas.map((c) => {
                  const d = m.porCultura.find((p) => p.culturaCodigo === c.codigo)?.demanda ?? null;
                  return (
                    <td key={c.codigo} className="mom-num dem-cultura">
                      {num(d)}
                    </td>
                  );
                })}
                <td className="mom-num dem-destaque">{num(m.demandaEstrutural)}</td>
                {comAjustada && <td className="mom-num dem-destaque">{num(m.demandaAjustada)}</td>}
                {comShare && (
                  <td className="mom-num dem-entregar">{num(comAjustada ? (m.aEntregarAjustada ?? m.aEntregar) : m.aEntregar)}</td>
                )}
                {comAjustada && (
                  <>
                    <td className="mom-num">{m.fatorDePreco === null ? '—' : <Seta valor={m.fatorDePreco - 1}>{variacaoDoFator(m.fatorDePreco)}</Seta>}</td>
                    <td className="mom-num">{m.fatorDeCredito === null ? '—' : <Seta valor={m.fatorDeCredito - 1}>{variacaoDoFator(m.fatorDeCredito)}</Seta>}</td>
                    <td className="mom-num">
                      {m.variacaoPercentual === null ? '—' : <Seta valor={m.variacaoPercentual}>{variacaoPercentual(m.variacaoPercentual)}</Seta>}
                    </td>
                  </>
                )}
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

function Seta({ valor, children }: { valor: number; children: React.ReactNode }) {
  return <span className={`diag-seta ${valor > 0.0005 ? 'sobe' : valor < -0.0005 ? 'desce' : ''}`}>{children}</span>;
}
