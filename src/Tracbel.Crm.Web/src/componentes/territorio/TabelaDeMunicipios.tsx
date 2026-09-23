/**
 * A tabela de municípios da ADR e o que ficou fora do mapa (issue 170, parte A).
 *
 * A SOMA DAS LINHAS É O TOTAL DA CONSULTA — é a conferência do documento 32: se
 * não fechar, algo sumiu ou foi contado duas vezes.
 */

import type { IndicadoresDoMunicipio, IndicadoresForaDoMapa } from '../../tipos/territorio';
import { ValorAusente } from '../comum/ValorAusente';
import { reaisCompactos } from './escalas';
import { MOTIVO_SEM_PARQUE, nº } from './indicadoresDaAdr';
import type { TotaisDaAdr } from './totaisDaAdr';

export function TabelaDeMunicipios({
  municipios,
  daAdr,
  foraDoMapa,
  totais,
  selecionado,
  aoSelecionar,
  territorioNaoCarregado,
  semFiltro,
}: {
  /** Todos os municípios da resposta — os da ADR e os de fora dela. */
  municipios: IndicadoresDoMunicipio[];
  /** Só os da ADR, já ordenados por venda. */
  daAdr: IndicadoresDoMunicipio[];
  foraDoMapa: IndicadoresForaDoMapa[];
  totais: TotaisDaAdr;
  selecionado: number | null;
  aoSelecionar: (codigo: number) => void;
  territorioNaoCarregado: boolean;
  semFiltro: boolean;
}) {
  return (
    <div className="card cad-cartao" data-bloco="tabela-municipios">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          <div className="card-title">Municípios da ADR e o que ficou fora do mapa</div>
          <div className="card-subtitle">
            A soma das linhas é o total da consulta. Clique numa linha para abrir o detalhe.
          </div>
        </div>
      </div>
      <div className="cad-tabela-wrap terr-tabela-municipios">
        <table className="cad-tabela">
          <caption className="cad-so-leitor">Indicadores por município</caption>
          <thead>
            <tr>
              <th scope="col">Município</th>
              <th scope="col" className="terr-coluna-hierarquia">Região · loja</th>
              {/* AS COLUNAS OPCIONAIS SAEM NO CELULAR (T4.6, corrigido na T4.7).

                  Oito colunas comprimidas em 390px não são uma tabela, são um
                  borrão — e rolagem lateral da página está proibida.

                  A T4.6 tirou três e deixou cinco, e a REVISÃO DAS CAPTURAS
                  mostrou que cinco também não cabem: em 390px a tabela virava
                  uma lista de uma coluna, com as outras quatro escondidas atrás
                  de rolagem horizontal dentro do cartão. O meu próprio teste
                  passou porque contava `:visible`, que em Playwright quer dizer
                  "não está `display:none`" — e não "cabe na tela".

                  Abaixo de 560px ficam DUAS: o município e as vendas. É o que
                  responde "onde vender" num aparelho de mão; o resto está
                  inteiro na ficha do município, a um toque. */}
              <th scope="col" className="terr-coluna-numero">Elegíveis</th>
              <th scope="col" className="terr-coluna-numero">No prazo</th>
              <th scope="col" className="terr-coluna-numero">Pendentes</th>
              <th scope="col" className="terr-coluna-numero">Vendas</th>
              <th scope="col" className="terr-coluna-numero">
                Pós-venda <span className="cad-sub">(provisório)</span>
              </th>
              <th scope="col" className="terr-coluna-numero">Máquinas teóricas</th>
            </tr>
          </thead>
          <tbody>
            {daAdr.map((m) => (
              <tr key={m.codigoIbge} aria-current={m.codigoIbge === selecionado ? 'true' : undefined}>
                <td>
                  <button type="button" className="cad-th-ordenar" onClick={() => aoSelecionar(m.codigoIbge)}>
                    {m.nome}
                  </button>
                </td>
                <td className="terr-coluna-hierarquia">
                  {m.regiao}
                  <div className="cad-sub">{m.lojaNome?.replace(/^.*—\s*/, '') ?? '—'}</div>
                </td>
                <td className="cad-mono">{nº(m.cobertura.vinculosComCadencia)}</td>
                <td className="cad-mono">{nº(m.cobertura.cobertos)}</td>
                <td className="cad-mono">
                  {nº(m.cobertura.pendentes)}
                  {m.cobertura.percentualPendente !== null && (
                    <div className="cad-sub">{m.cobertura.percentualPendente.toLocaleString('pt-BR')}%</div>
                  )}
                </td>
                <td className="cad-mono">{reaisCompactos(m.vendas.valorLiquido)}</td>
                <td className="cad-mono">{reaisCompactos(m.vendas.posVenda)}</td>
                <td className="cad-mono">
                  {m.potencialEstrutural?.parqueDeMaquinas == null ? (
                    <ValorAusente
                      motivo={
                        m.potencialEstrutural
                          ? MOTIVO_SEM_PARQUE[m.potencialEstrutural.motivoSemParque]
                          : 'nenhuma regra de potencial vigente alcança este município'
                      }
                      oQue={`o parque de ${m.nome}`}
                    />
                  ) : (
                    nº(m.potencialEstrutural.parqueDeMaquinas)
                  )}
                </td>
              </tr>
            ))}
            {territorioNaoCarregado ? (
              <tr className="terr-linha-total">
                <td>Total da ADR</td>
                <td colSpan={7}>território não carregado neste banco — as linhas abaixo são o que a consulta encontrou</td>
              </tr>
            ) : (
              <tr className="terr-linha-total">
                <td>Total da ADR {semFiltro ? '' : '(filtro)'}</td>
                <td>{daAdr.length} municípios</td>
                <td className="cad-mono">{nº(totais.elegiveis)}</td>
                <td className="cad-mono">{nº(totais.cobertos)}</td>
                <td className="cad-mono">{nº(totais.pendentes)}</td>
                <td className="cad-mono">{reaisCompactos(totais.vendas)}</td>
                <td className="cad-mono">{reaisCompactos(totais.posVenda)}</td>
                <td className="cad-mono">{nº(Math.round(totais.maquinasTeoricas))}</td>
              </tr>
            )}
            {semFiltro && !territorioNaoCarregado && (
              <LinhaDeGrupo
                rotulo="São Paulo fora da ADR"
                descricao="municípios com cliente fora da ADR"
                itens={municipios.filter((m) => !m.pertenceAAdr)}
              />
            )}
            {foraDoMapa.map((g) => (
              <tr key={g.grupo}>
                <td>
                  {g.grupo}
                  <div className="cad-sub">{g.descricao}</div>
                </td>
                <td>—</td>
                <td className="cad-mono">{nº(g.cobertura.vinculosComCadencia)}</td>
                <td className="cad-mono">{nº(g.cobertura.cobertos)}</td>
                <td className="cad-mono">{nº(g.cobertura.pendentes)}</td>
                <td className="cad-mono">{reaisCompactos(g.vendas.valorLiquido)}</td>
                <td className="cad-mono">{reaisCompactos(g.vendas.posVenda)}</td>
                <td>—</td>
              </tr>
            ))}
            {semFiltro && <LinhaDeTotal municipios={municipios} foraDoMapa={foraDoMapa} />}
          </tbody>
        </table>
      </div>
    </div>
  );
}

/** Uma linha que soma um grupo de municípios. */
function LinhaDeGrupo({ rotulo, descricao, itens }: { rotulo: string; descricao: string; itens: IndicadoresDoMunicipio[] }) {
  const soma = (f: (m: IndicadoresDoMunicipio) => number) => itens.reduce((s, m) => s + f(m), 0);
  return (
    <tr>
      <td>
        {rotulo}
        <div className="cad-sub">
          {descricao} ({itens.length})
        </div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.cobertos))}</td>
      <td className="cad-mono">{nº(soma((m) => m.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(soma((m) => m.vendas.posVenda))}</td>
      <td>—</td>
    </tr>
  );
}

/**
 * O total da consulta: municípios do mapa mais o que ficou fora dele. É o número
 * que a conferência SQL do documento 32 compara — se não fechar, algo sumiu ou
 * foi contado duas vezes.
 */
function LinhaDeTotal({
  municipios,
  foraDoMapa,
}: {
  municipios: IndicadoresDoMunicipio[];
  foraDoMapa: { cobertura: { vinculosComCadencia: number; cobertos: number; pendentes: number }; vendas: { valorLiquido: number; posVenda: number } }[];
}) {
  const somaMapa = (f: (m: IndicadoresDoMunicipio) => number) => municipios.reduce((s, m) => s + f(m), 0);
  const somaFora = (f: (g: (typeof foraDoMapa)[number]) => number) => foraDoMapa.reduce((s, g) => s + f(g), 0);
  return (
    <tr className="terr-linha-total">
      <td>
        Total da consulta
        <div className="cad-sub">municípios do mapa + tudo o que ficou fora dele</div>
      </td>
      <td>—</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.vinculosComCadencia) + somaFora((g) => g.cobertura.vinculosComCadencia))}</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.cobertos) + somaFora((g) => g.cobertura.cobertos))}</td>
      <td className="cad-mono">{nº(somaMapa((m) => m.cobertura.pendentes) + somaFora((g) => g.cobertura.pendentes))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.valorLiquido) + somaFora((g) => g.vendas.valorLiquido))}</td>
      <td className="cad-mono">{reaisCompactos(somaMapa((m) => m.vendas.posVenda) + somaFora((g) => g.vendas.posVenda))}</td>
      <td>—</td>
    </tr>
  );
}
