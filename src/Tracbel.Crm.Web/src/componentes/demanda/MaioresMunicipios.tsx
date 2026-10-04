/**
 * OS MAIORES MUNICÍPIOS POR POTENCIAL — a tabela da maquete de 02/10/2026, no lugar da matriz município × potencial.
 *
 * OS DEZ MAIORES À VISTA, pela demanda da base escolhida, com as colunas da maquete: a cultura principal, o parque
 * potencial, a demanda anual, a ajustada e a variação sobre o ano anterior da PAM. "Ver todos os municípios" abre a lista
 * inteira, com a busca e a paginação da matriz antiga.
 *
 * O QUE A MATRIZ MOSTRAVA EM COLUNA NÃO SUMIU: a demanda de cada cultura e o "a entregar" estão na dica da demanda, os
 * dois fatores do momento na dica da ajustada, e o CSV continua levando tudo.
 */

import { ArrowDown, ArrowRight, ArrowUp, ChevronsUpDown } from 'lucide-react';
import { useMemo, useState } from 'react';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import type { DemandaDoMunicipioNaPrevisao, DemandaEPrevisaoDaRegiao } from '../../tipos/mercado';
import { ordenar, proximaOrdem, type Ordem } from '../comum/ordenacao';
import { n, variacaoDoFator, variacaoPercentual } from './demanda';

/** Quantos municípios a tabela mostra antes de "Ver todos". */
export const MUNICIPIOS_A_VISTA = 10;

type Coluna = 'nome' | 'culturaPredominante' | 'parque' | 'demanda' | 'demandaAjustada' | 'variacaoAnoAnterior';

const COLUNAS: { chave: Coluna; rotulo: string; texto?: boolean }[] = [
  { chave: 'nome', rotulo: 'Município', texto: true },
  { chave: 'culturaPredominante', rotulo: 'Cultura principal', texto: true },
  { chave: 'parque', rotulo: 'Parque potencial' },
  { chave: 'demanda', rotulo: 'Demanda anual' },
  { chave: 'demandaAjustada', rotulo: 'Ajustada' },
  { chave: 'variacaoAnoAnterior', rotulo: 'Δ vs. ano ant.' },
];

type Base = 'ajustada' | 'estrutural';

const demandaDe = (m: DemandaDoMunicipioNaPrevisao, base: Base) =>
  base === 'ajustada' ? (m.demandaAjustada ?? m.demandaEstrutural) : m.demandaEstrutural;

const valorDe = (m: DemandaDoMunicipioNaPrevisao, coluna: Coluna, base: Base): number | string | null =>
  coluna === 'demanda' ? demandaDe(m, base) : (m[coluna] ?? null);

export function MaioresMunicipios({ dados, base }: { dados: DemandaEPrevisaoDaRegiao; base: Base }) {
  const [todos, setTodos] = useState(false);
  const [busca, setBusca] = useState('');
  const [ordem, setOrdem] = useState<Ordem<Coluna>>({ coluna: 'demanda', sentido: -1 });
  const [tamanho, setTamanho] = useState(25);

  const linhas = useMemo(() => {
    const termo = todos ? busca.trim().toLocaleLowerCase('pt-BR') : '';
    const filtrados = dados.municipios.filter((m) => !termo || m.nome.toLocaleLowerCase('pt-BR').includes(termo));
    return ordenar(filtrados, ordem, (m, coluna) => valorDe(m, coluna, base));
  }, [dados.municipios, busca, ordem, todos, base]);

  // A PÁGINA VOLTA PARA A PRIMEIRA quando a lista ou a ordem muda — derivado na renderização, e não num efeito.
  const chave = `${linhas.length}|${ordem.coluna}|${ordem.sentido}`;
  const [escolha, setEscolha] = useState({ chave, pagina: 1 });
  const pagina = escolha.chave === chave ? escolha.pagina : 1;
  const totalDePaginas = Math.max(1, Math.ceil(linhas.length / tamanho));
  const atual = Math.min(pagina, totalDePaginas);
  const visiveis = todos ? linhas.slice((atual - 1) * tamanho, atual * tamanho) : linhas.slice(0, MUNICIPIOS_A_VISTA);
  const posicaoInicial = todos ? (atual - 1) * tamanho : 0;

  const nomeDaCultura = new Map(dados.culturas.map((c) => [c.codigo, c.nome]));
  const num = (v: number | null, casas = 1) => (v === null ? '—' : n(v, casas));

  function ordenarPor(coluna: Coluna, texto?: boolean) {
    setOrdem((o) => proximaOrdem(o, coluna, texto ?? false));
  }

  return (
    <>
      {todos && (
        <input
          type="search"
          className="dem-busca"
          value={busca}
          placeholder="Buscar município"
          aria-label="Buscar município"
          onChange={(e) => setBusca(e.target.value)}
        />
      )}
      <div className="mom-tabela-rolagem">
        <table className="mom-tabela dem-maiores">
          <caption className="cad-so-leitor">Os municípios da ADR pela demanda de máquinas</caption>
          <thead>
            <tr>
              <th scope="col" className="dem-posicao">
                #
              </th>
              {COLUNAS.map((c) => {
                // A ORDEM PADRÃO (a maior demanda primeiro) NÃO TEM SETA, como a maquete; a seta aparece quando o usuário
                // reordena, e a das outras colunas só ao passar o mouse.
                const padrao = ordem.coluna === 'demanda' && ordem.sentido === -1;
                const ativa = ordem.coluna === c.chave && !padrao;
                const Seta = !ativa ? ChevronsUpDown : ordem.sentido === 1 ? ArrowUp : ArrowDown;
                return (
                  <th
                    key={c.chave}
                    scope="col"
                    className={c.texto ? undefined : 'mom-num'}
                    aria-sort={ordem.coluna === c.chave ? (ordem.sentido === 1 ? 'ascending' : 'descending') : 'none'}
                  >
                    <button type="button" className="diag-ordenar" onClick={() => ordenarPor(c.chave, c.texto)}>
                      {c.rotulo}
                      <Seta size={11} strokeWidth={2.2} className={ativa ? 'dem-seta-ativa' : 'dem-seta-inativa'} aria-hidden="true" />
                    </button>
                  </th>
                );
              })}
            </tr>
          </thead>
          <tbody>
            {visiveis.map((m, i) => {
              const porCultura = m.porCultura
                .map((p) => `${nomeDaCultura.get(p.culturaCodigo) ?? p.culturaCodigo} ${n(p.demanda)}`)
                .join(' · ');
              const aEntregar = base === 'ajustada' ? (m.aEntregarAjustada ?? m.aEntregar) : m.aEntregar;
              return (
                <tr key={m.codigoIbge}>
                  <td className="dem-posicao">{posicaoInicial + i + 1}</td>
                  <td>
                    <span className="dem-municipio" title={`Região ${m.regiao}${m.loja ? ` · ${m.loja}` : ''}`}>
                      {m.nome}
                    </span>
                  </td>
                  <td>{m.culturaPredominante ?? '—'}</td>
                  <td className="mom-num">{num(m.parque, 0)}</td>
                  <td
                    className="mom-num"
                    title={[porCultura, aEntregar !== null && dados.shareAlvo !== null ? `a entregar (${n(dados.shareAlvo, 0)}%): ${n(aEntregar)}` : '']
                      .filter(Boolean)
                      .join(' — ')}
                  >
                    {num(demandaDe(m, base))}
                  </td>
                  <td
                    className="mom-num"
                    title={[
                      m.fatorDePreco !== null ? `preço ${variacaoDoFator(m.fatorDePreco)}` : '',
                      m.fatorDeCredito !== null ? `crédito ${variacaoDoFator(m.fatorDeCredito)}` : '',
                      m.variacaoPercentual !== null ? `efeito líquido ${variacaoPercentual(m.variacaoPercentual)}` : '',
                    ]
                      .filter(Boolean)
                      .join(' · ')}
                  >
                    {num(m.demandaAjustada)}
                  </td>
                  <td className="mom-num" title={m.demandaEstruturalAnoAnterior !== null ? `estrutural no ano anterior: ${n(m.demandaEstruturalAnoAnterior)}` : undefined}>
                    {m.variacaoAnoAnterior === null ? (
                      '—'
                    ) : (
                      <span className={`diag-seta ${m.variacaoAnoAnterior > 0 ? 'sobe' : m.variacaoAnoAnterior < 0 ? 'desce' : ''}`}>
                        {variacaoPercentual(m.variacaoAnoAnterior)}
                      </span>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
      {todos ? (
        <BarraDePaginacao
          pagina={{ itens: visiveis, pagina: atual, tamanho, total: linhas.length, totalDePaginas, temProxima: atual < totalDePaginas }}
          oQue="municípios"
          aoTrocarPagina={(p) => setEscolha({ chave, pagina: p })}
          aoTrocarTamanho={(t) => {
            setTamanho(t);
            setEscolha({ chave, pagina: 1 });
          }}
        />
      ) : null}
      {dados.municipios.length > MUNICIPIOS_A_VISTA && (
        <div className="dem-ver-todas">
          <button
            type="button"
            className="dem-botao-link"
            onClick={() => setTodos((t) => !t)}
            aria-expanded={todos}
            title={`${dados.municipios.length} municípios da ADR no recorte`}
          >
            {todos ? `Ver só os ${MUNICIPIOS_A_VISTA} maiores` : 'Ver todos os municípios'}
            <ArrowRight size={14} strokeWidth={2.2} aria-hidden="true" />
          </button>
        </div>
      )}
    </>
  );
}
