/**
 * A TABELA DO DIAGNÓSTICO — todos os municípios do recorte, com as colunas do protótipo (28/09/2026).
 *
 * UMA LINHA POR MUNICÍPIO, E UMA LINHA DE ALTURA. A situação e o plano de ação inteiros foram para a ficha: na
 * coluna, as frases quebravam em cinco ou seis linhas e a página passava de cinco mil pixels. Aqui fica a PRIMEIRA ação
 * do plano, que é a que decide o que fazer, e o "+2" diz que há mais na ficha.
 *
 * A META DE PLANEJAMENTO NÃO É COLUNA — como no protótipo. Ela está no cartão do topo e na ficha, e com ela a coluna
 * da próxima ação saía da largura do painel a 1.536 px.
 *
 * PAGINADA, 25 por página: com 203 municípios a tabela era a página inteira. A ordem e o filtro de classe valem para
 * todas as páginas, e o CSV leva todas as linhas filtradas, e não só a página.
 */

import { useMemo, useState } from 'react';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import { ValorAusente } from '../comum/ValorAusente';
import { PainelDoMomento } from '../mercado/momento/pecas';
import type { MunicipioNoDiagnostico } from '../../tipos/mercado';
import { acoesDoPlano, n, pct, SEM_ART, variacao, type ColunaDoDiagnostico } from './diagnostico';
import { SeloDoIoc } from './FichaDoMunicipioNoIoc';

const COLUNAS: { chave: ColunaDoDiagnostico; rotulo: string; texto?: boolean; dica?: string; numerica?: boolean }[] = [
  { chave: 'nome', rotulo: 'Município', texto: true },
  { chave: 'culturaPrincipal', rotulo: 'Cultura principal', texto: true, dica: 'com o momento de preço dela' },
  { chave: 'ioc', rotulo: 'IOC' },
  { chave: 'demandaEstrutural', rotulo: 'Demanda/ano', dica: 'estrutural, com a ajustada embaixo', numerica: true },
  { chave: 'vendidasNoPeriodo', rotulo: 'Vendidas', dica: 'no período, pelo ART', numerica: true },
  { chave: 'clientes', rotulo: 'Clientes', dica: 'com os em carteira embaixo', numerica: true },
  { chave: 'cobertura', rotulo: 'Cobertura', numerica: true },
  { chave: 'penetracao', rotulo: 'Penetração', numerica: true },
  { chave: 'indiceDeCredito', rotulo: 'Crédito', numerica: true },
];

export function TabelaDoIoc({
  linhas,
  total,
  semArt,
  ordem,
  aoOrdenar,
  selecionado,
  aoSelecionar,
  subtitulo,
  acao,
}: {
  /** As linhas já filtradas e ordenadas. */
  linhas: MunicipioNoDiagnostico[];
  /** Quantos municípios o recorte tem, antes do filtro de classe. */
  total: number;
  semArt: boolean;
  ordem: { coluna: ColunaDoDiagnostico; sentido: 1 | -1 };
  aoOrdenar: (coluna: ColunaDoDiagnostico, texto?: boolean) => void;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  subtitulo: string;
  acao?: React.ReactNode;
}) {
  const [tamanho, setTamanho] = useState(25);

  // UM FILTRO OU UMA ORDEM NOVA VOLTA PARA A PRIMEIRA PÁGINA — senão a página 5 de uma lista de 3 fica vazia. Derivado na
  // renderização, e não num efeito: a página guardada vale só para a lista em que foi escolhida.
  const chave = `${linhas.length}|${ordem.coluna}|${ordem.sentido}`;
  const [escolha, setEscolha] = useState({ chave, pagina: 1 });
  const pagina = escolha.chave === chave ? escolha.pagina : 1;
  const setPagina = (p: number) => setEscolha({ chave, pagina: p });

  const totalDePaginas = Math.max(1, Math.ceil(linhas.length / tamanho));
  const atual = Math.min(pagina, totalDePaginas);
  const daPagina = useMemo(() => linhas.slice((atual - 1) * tamanho, atual * tamanho), [linhas, atual, tamanho]);

  return (
    <PainelDoMomento titulo="Municípios por prioridade" subtitulo={subtitulo} direita={acao} data-bloco="municipios-por-prioridade">

      {linhas.length === 0 ? (
        <p className="diag-nota">{total === 0 ? 'Nenhum município da ADR neste recorte.' : 'Nenhum município com esses filtros.'}</p>
      ) : (
        <>
          <div className="mom-tabela-rolagem">
            <table className="mom-tabela diag-tabela">
              <caption className="cad-so-leitor">Municípios da ADR pelo Índice de Oportunidade Comercial</caption>
              <thead>
                <tr>
                  {COLUNAS.map((c) => (
                    <th
                      key={c.chave}
                      scope="col"
                      className={c.numerica ? 'mom-num' : undefined}
                      aria-sort={ordem.coluna === c.chave ? (ordem.sentido === 1 ? 'ascending' : 'descending') : 'none'}
                      title={c.dica}
                    >
                      <button type="button" className="diag-ordenar" onClick={() => aoOrdenar(c.chave, c.texto)}>
                        {c.rotulo}
                        {ordem.coluna === c.chave && <span aria-hidden="true">{ordem.sentido === 1 ? ' ▲' : ' ▼'}</span>}
                      </button>
                    </th>
                  ))}
                  <th scope="col">Próxima ação</th>
                </tr>
              </thead>
              <tbody>
                {daPagina.map((m) => (
                  <Linha
                    key={m.codigoIbge}
                    municipio={m}
                    semArt={semArt}
                    escolhida={selecionado === m.codigoIbge}
                    aoEscolher={() => aoSelecionar(m.codigoIbge)}
                  />
                ))}
              </tbody>
            </table>
          </div>
          <BarraDePaginacao
            pagina={{
              itens: daPagina,
              pagina: atual,
              tamanho,
              total: linhas.length,
              totalDePaginas,
              temProxima: atual < totalDePaginas,
            }}
            oQue="municípios"
            aoTrocarPagina={setPagina}
            aoTrocarTamanho={(t) => {
              setTamanho(t);
              setPagina(1);
            }}
          />
        </>
      )}
    </PainelDoMomento>
  );
}

function Linha({
  municipio: m,
  semArt,
  escolhida,
  aoEscolher,
}: {
  municipio: MunicipioNoDiagnostico;
  semArt: boolean;
  escolhida: boolean;
  aoEscolher: () => void;
}) {
  const acoes = acoesDoPlano(m.planoDeAcao);
  const semDemanda = 'O município não tem demanda estimada nesta categoria: falta regra de potencial ou área plantada.';

  return (
    <tr className={escolhida ? 'diag-linha escolhida' : 'diag-linha'} aria-selected={escolhida}>
      <td>
        <button type="button" className="diag-municipio" aria-pressed={escolhida} onClick={aoEscolher}>
          {m.nome}
        </button>
        <div className="cad-sub">{m.loja ?? m.regiao}</div>
      </td>
      <td>
        {m.culturaPrincipal ?? '—'}
        {m.indiceDePreco !== null && (
          <span className={`diag-seta ${m.indiceDePreco >= 1 ? 'sobe' : 'desce'}`} title="Momento de preço da cultura">
            {' '}
            {variacao(m.indiceDePreco)}
          </span>
        )}
      </td>
      <td>
        {m.ioc === null || m.classe === null ? (
          <ValorAusente motivo="Nenhum componente com peso tem dado neste município." oQue={`o IOC de ${m.nome}`} />
        ) : (
          <SeloDoIoc ioc={m.ioc} classe={m.classe} />
        )}
      </td>
      <td className="mom-num">
        {m.demandaEstrutural === null ? <ValorAusente motivo={semDemanda} oQue="a demanda" /> : n(m.demandaEstrutural)}
        {m.demandaAjustada !== null && m.demandaAjustada !== m.demandaEstrutural && (
          <div className="cad-sub" title="A demanda ajustada pelo momento: preço, crédito e percepção">
            ajustada {n(m.demandaAjustada)}
          </div>
        )}
      </td>
      <td className="mom-num">
        {m.vendidasNoPeriodo === null ? (
          <ValorAusente motivo={semArt ? SEM_ART : 'Sem venda do ART para este município.'} oQue="as vendas" />
        ) : (
          n(m.vendidasNoPeriodo, 0)
        )}
      </td>
      <td className="mom-num">
        {n(m.clientes, 0)}
        {m.clientesEmCarteira !== null && <div className="cad-sub">{n(m.clientesEmCarteira, 0)} em carteira</div>}
      </td>
      <td className="mom-num">
        {m.cobertura === null ? (
          <ValorAusente motivo="Nenhum vínculo de carteira com cadência declarada neste município." oQue="a cobertura" />
        ) : (
          <>
            {pct(m.cobertura)}
            <div className="cad-sub">
              {n(m.cobertos, 0)} de {n(m.vinculosComCadencia, 0)}
            </div>
          </>
        )}
      </td>
      <td className="mom-num">
        {m.penetracao === null ? (
          <ValorAusente motivo={semArt ? SEM_ART : 'Sem demanda estimada para comparar.'} oQue="a penetração" />
        ) : (
          pct(m.penetracao)
        )}
      </td>
      <td className="mom-num">
        {m.indiceDeCredito === null ? (
          <ValorAusente motivo="O SICOR não formou o índice de crédito deste município." oQue="o crédito" />
        ) : (
          <>
            <span className={`diag-seta ${m.indiceDeCredito >= 1 ? 'sobe' : 'desce'}`}>{variacao(m.indiceDeCredito)}</span>
            {m.creditoBasePequena && <div className="cad-sub">base pequena</div>}
          </>
        )}
      </td>
      <td className="diag-proxima">
        {acoes.length > 0 ? (
          <span className="diag-acao" title={m.situacao ? `${m.situacao}. Plano: ${acoes.join('; ')}` : undefined}>
            <span>{acoes[0]}</span>
            {acoes.length > 1 && <span className="diag-acao-mais">+{acoes.length - 1}</span>}
          </span>
        ) : (
          '—'
        )}
      </td>
    </tr>
  );
}
