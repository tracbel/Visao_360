/**
 * A TABELA DO DIAGNÓSTICO — todos os municípios do recorte (28/09/2026; desenho da maquete em 02/10/2026).
 *
 * AS COLUNAS SÃO AS DA MAQUETE: a posição, o município, a regional / loja, a cultura principal, o IOC, a demanda, as
 * vendas, os clientes, a cobertura em barra, a penetração, o crédito, a próxima ação e o menu da linha. Toda coluna
 * ordena, com a seta à vista.
 *
 * UMA LINHA DE ALTURA. O que antes vinha numa sub-linha — a demanda ajustada, os clientes em carteira, os vínculos da
 * cobertura, o momento de preço da cultura, a base pequena do crédito e o resto do plano de ação — está na dica da
 * própria célula e na ficha do município, que abre ao clicar no nome.
 *
 * PAGINADA, 25 por página: com 203 municípios a tabela era a página inteira. A ordem e os filtros valem para todas as
 * páginas, e o CSV leva todas as linhas filtradas, e não só a página.
 */

import { ArrowDown, ArrowUp, ChevronsUpDown } from 'lucide-react';
import { useMemo, useState, type ReactNode } from 'react';
import { BarraDePaginacao } from '../cadastro/BarraDePaginacao';
import { MenuDaLinha } from '../comum/MenuDaLinha';
import { ValorAusente } from '../comum/ValorAusente';
import { PainelDoMomento } from '../mercado/momento/pecas';
import type { MunicipioNoDiagnostico } from '../../tipos/mercado';
import { acoesDoPlano, n, pct, ROTULO_DA_CLASSE, SEM_ART, variacao, type ColunaDoDiagnostico } from './diagnostico';

const COLUNAS: { chave: ColunaDoDiagnostico; rotulo: string; texto?: boolean; numerica?: boolean; classe?: string }[] = [
  { chave: 'nome', rotulo: 'Município', texto: true },
  { chave: 'loja', rotulo: 'Regional / Loja', texto: true },
  { chave: 'culturaPrincipal', rotulo: 'Cultura principal', texto: true },
  { chave: 'ioc', rotulo: 'IOC' },
  { chave: 'demandaEstrutural', rotulo: 'Demanda (un)', numerica: true },
  { chave: 'vendidasNoPeriodo', rotulo: 'Vendas (un)', numerica: true },
  { chave: 'clientes', rotulo: 'Clientes', numerica: true },
  { chave: 'cobertura', rotulo: 'Cobertura', classe: 'diag-coluna-cobertura' },
  { chave: 'penetracao', rotulo: 'Penetração', numerica: true },
  { chave: 'indiceDeCredito', rotulo: 'Crédito', numerica: true },
  { chave: 'planoDeAcao', rotulo: 'Próxima ação', texto: true, classe: 'diag-coluna-acao' },
];

export function TabelaDoIoc({
  linhas,
  total,
  semArt,
  ordem,
  aoOrdenar,
  selecionado,
  aoSelecionar,
  aviso,
}: {
  /** As linhas já filtradas e ordenadas. */
  linhas: MunicipioNoDiagnostico[];
  /** Quantos municípios o recorte tem, antes dos filtros da tela. */
  total: number;
  semArt: boolean;
  ordem: { coluna: ColunaDoDiagnostico; sentido: 1 | -1 };
  aoOrdenar: (coluna: ColunaDoDiagnostico, texto?: boolean) => void;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  /** O que recorta a lista além do período e da categoria — a classe, a cultura ou o CEN —, escrito em cima da tabela. */
  aviso?: ReactNode;
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
    <PainelDoMomento titulo="Municípios por prioridade" data-bloco="municipios-por-prioridade">
      {aviso && <p className="diag-aviso-da-tabela">{aviso}</p>}
      {linhas.length === 0 ? (
        <p className="diag-nota">{total === 0 ? 'Nenhum município da ADR neste recorte.' : 'Nenhum município com esses filtros.'}</p>
      ) : (
        <>
          <div className="mom-tabela-rolagem">
            <table className="mom-tabela diag-tabela">
              <caption className="cad-so-leitor">Municípios da ADR pelo Índice de Oportunidade Comercial</caption>
              <thead>
                <tr>
                  <th scope="col" className="diag-coluna-posicao">
                    #
                  </th>
                  {COLUNAS.map((c) => {
                    const ativa = ordem.coluna === c.chave;
                    const Seta = !ativa ? ChevronsUpDown : ordem.sentido === 1 ? ArrowUp : ArrowDown;
                    return (
                      <th
                        key={c.chave}
                        scope="col"
                        className={[c.numerica ? 'mom-num' : '', c.classe ?? ''].join(' ').trim() || undefined}
                        aria-sort={ativa ? (ordem.sentido === 1 ? 'ascending' : 'descending') : 'none'}
                      >
                        <button type="button" className="diag-ordenar" onClick={() => aoOrdenar(c.chave, c.texto)}>
                          {c.rotulo}
                          <Seta size={12} strokeWidth={2.2} className={ativa ? 'diag-seta-ativa' : 'diag-seta-inativa'} aria-hidden="true" />
                        </button>
                      </th>
                    );
                  })}
                  <th scope="col" className="diag-coluna-menu">
                    <span className="cad-so-leitor">Ações</span>
                  </th>
                </tr>
              </thead>
              <tbody>
                {daPagina.map((m, i) => (
                  <Linha
                    key={m.codigoIbge}
                    posicao={(atual - 1) * tamanho + i + 1}
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
  posicao,
  municipio: m,
  semArt,
  escolhida,
  aoEscolher,
}: {
  posicao: number;
  municipio: MunicipioNoDiagnostico;
  semArt: boolean;
  escolhida: boolean;
  aoEscolher: () => void;
}) {
  const acoes = acoesDoPlano(m.planoDeAcao);
  const semDemanda = 'O município não tem demanda estimada nesta categoria: falta regra de potencial ou área plantada.';

  return (
    <tr className={escolhida ? 'diag-linha escolhida' : 'diag-linha'} aria-selected={escolhida}>
      <td className="diag-coluna-posicao">{posicao}</td>
      <td>
        <button type="button" className="diag-municipio" aria-pressed={escolhida} onClick={aoEscolher}>
          {m.nome}
        </button>
      </td>
      <td className="diag-loja" title={`Região ${m.regiao}${m.responsavel ? ` · CEN ${m.responsavel}` : ''}`}>
        {m.loja ?? `Região ${m.regiao}`}
      </td>
      <td>
        {m.culturaPrincipal ? (
          <span
            className="diag-cultura"
            title={m.indiceDePreco !== null ? `Momento de preço da cultura: ${variacao(m.indiceDePreco)}` : 'Sem série de preço para a cultura'}
          >
            {m.culturaPrincipal}
          </span>
        ) : (
          '—'
        )}
      </td>
      <td>
        {m.ioc === null || m.classe === null ? (
          <ValorAusente motivo="Nenhum componente com peso tem dado neste município." oQue={`o IOC de ${m.nome}`} />
        ) : (
          <span className={`diag-ioc-pilula diag-classe-${m.classe}`} title={ROTULO_DA_CLASSE[m.classe]}>
            {n(m.ioc)}
            <span className="cad-so-leitor"> — {ROTULO_DA_CLASSE[m.classe]}</span>
          </span>
        )}
      </td>
      <td className="mom-num" title={m.demandaAjustada !== null ? `Ajustada pelo momento: ${n(m.demandaAjustada)}` : undefined}>
        {m.demandaEstrutural === null ? <ValorAusente motivo={semDemanda} oQue="a demanda" /> : n(m.demandaEstrutural, 0)}
      </td>
      <td className="mom-num">
        {m.vendidasNoPeriodo === null ? (
          <ValorAusente motivo={semArt ? SEM_ART : 'Sem venda do ART para este município.'} oQue="as vendas" />
        ) : (
          n(m.vendidasNoPeriodo, 0)
        )}
      </td>
      <td
        className="mom-num"
        title={`${m.clientesEmCarteira !== null ? `${n(m.clientesEmCarteira, 0)} em carteira · ` : ''}${n(m.clientesQueCompraram, 0)} compraram no período`}
      >
        {n(m.clientes, 0)}
      </td>
      <td className="diag-coluna-cobertura">
        {m.cobertura === null ? (
          <ValorAusente motivo="Nenhum vínculo de carteira com cadência declarada neste município." oQue="a cobertura" />
        ) : (
          <span className="diag-cobertura" title={`${n(m.cobertos, 0)} de ${n(m.vinculosComCadencia, 0)} vínculos cobertos`}>
            <span className="diag-cobertura-trilho" aria-hidden="true">
              <span style={{ width: `${Math.min(100, m.cobertura * 100)}%` }} />
            </span>
            {pct(m.cobertura)}
          </span>
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
          <span
            className={`diag-seta ${m.indiceDeCredito >= 1 ? 'sobe' : 'desce'}`}
            title={m.creditoBasePequena ? 'Base pequena: poucas operações para ser tendência' : 'Crédito de mecanização (SICOR), momento recente'}
          >
            {variacao(m.indiceDeCredito)}
            {m.creditoBasePequena && <span className="cad-so-leitor"> (base pequena)</span>}
          </span>
        )}
      </td>
      <td className="diag-coluna-acao">
        {acoes.length > 0 ? (
          <span className="diag-acao" title={m.situacao ? `${m.situacao}. Plano: ${acoes.join('; ')}` : acoes.join('; ')}>
            {acoes[0]}
          </span>
        ) : (
          '—'
        )}
      </td>
      <td className="diag-coluna-menu">
        <MenuDaLinha
          rotulo={`Ações de ${m.nome}`}
          itens={[
            { rotulo: 'Ver a ficha do município', para: `/mercado/diagnostico?municipio=${m.codigoIbge}` },
            { rotulo: 'Ver nos Indicadores Geográficos', para: `/relatorios/territorio?municipio=${m.codigoIbge}` },
          ]}
        />
      </td>
    </tr>
  );
}
