/**
 * O PREÇO RECEBIDO PELO PRODUTOR, DA PAM (issue 198) — anual e por município.
 *
 * ELE NÃO SUBSTITUI O PAINEL DE PREÇOS DA CONAB, e não se emenda a ele. São conceitos da mesma família e
 * não intercambiáveis: comparando 2025 em São Paulo, a distância vai de +1,1% na soja a −28,4% no
 * amendoim. Por isso são dois painéis, um ao lado do outro, cada um dizendo o que é.
 *
 * O QUE ELE TEM E A CONAB NÃO TEM, e é por isso que ele existe:
 *
 *   - **série longa** — desde 2010, contra a janela de 12 meses da CONAB, que está carregada desde 09/2025;
 *   - **granularidade municipal** — a CONAB publica por UF.
 *
 * AS MÉDIAS DE 3 E 5 ANOS SÓ SAEM COMPLETAS. Faltando um ano, a média não aparece e a tela diz quais anos
 * faltaram: uma média de dois anos rotulada como de três parece completa, e é pior que ausência.
 */

import { useMemo } from 'react';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterPrecoImplicitoDaPam } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type { MediaPlurianual, SerieDoPrecoImplicito } from '../../tipos/mercado';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { ValorAusente } from '../comum/ValorAusente';
import { InfoTooltip } from '../InfoTooltip';

/** Quantos anos da série a tabela mostra sem pedir para abrir. A série inteira fica no recolhível. */
const ANOS_NA_PRIMEIRA_CAMADA = 5;

const reais = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 2 });

const nº = (v: number) => v.toLocaleString('pt-BR', { maximumFractionDigits: 0 });

/** A média, ou o motivo de ela não fechar — com os anos que faltaram, nomeados. */
function Media({ media, unidade }: { media: MediaPlurianual; unidade: string }) {
  if (media.preco !== null) return <strong>{reais(media.preco)}</strong>;

  return (
    <ValorAusente
      motivo={
        `A média de ${media.anos} anos só sai com os ${media.anos} anos completos, e ` +
        (media.anosFaltando.length === 0
          ? 'não há série para formá-la.'
          : `${media.anosFaltando.length === 1 ? 'falta' : 'faltam'} ${media.anosFaltando.join(', ')}.`) +
        ' Uma média de menos anos apresentada como de ' +
        `${media.anos} parece completa, e isso engana mais do que a ausência. O preço é por ${unidade}.`
      }
      oQue={`a média de ${media.anos} anos`}
    />
  );
}

export function PainelDoPrecoImplicito({
  municipioCodigoIbge = null,
  nomeDoMunicipio = null,
  produtosDoMunicipio = [],
}: {
  /** O município escolhido; nulo soma a ADR inteira. */
  municipioCodigoIbge?: number | null;
  nomeDoMunicipio?: string | null;
  /** Os produtos da PAM do município, por área — eles vêm primeiro (issue 168). */
  produtosDoMunicipio?: readonly number[];
}) {
  const { contexto } = useContextoDeAcesso();

  const leitura = useRecurso(
    (sinal) => obterPrecoImplicitoDaPam(contexto, municipioCodigoIbge, sinal),
    [contexto.empresa, contexto.usuario, municipioCodigoIbge ?? 0],
  );

  /**
   * AS CULTURAS DO MUNICÍPIO VÊM PRIMEIRO (issue 168), na ordem de área plantada dele.
   *
   * A ordenação é feita aqui, e não com o `comAsCulturasDoMunicipioPrimeiro`: aquele opera sobre
   * `CulturaNoCatalogo`, que casa produto da PAM por uma lista de produtos, e esta série já é POR produto
   * da PAM. Generalizá-lo para os dois formatos custaria mais do que estas seis linhas.
   */
  const series = useMemo(() => {
    const todas = leitura.dados?.series ?? [];
    if (produtosDoMunicipio.length === 0) return todas;

    const posicao = (s: SerieDoPrecoImplicito) => {
      const i = produtosDoMunicipio.indexOf(s.produtoCodigoIbge);
      return i === -1 ? Number.MAX_SAFE_INTEGER : i;
    };

    // `toSorted` não muda a lista que veio da API — ordenar no lugar faria a ordem depender de quantas
    // vezes o componente renderizou.
    return todas.toSorted((a, b) => posicao(a) - posicao(b) || a.produto.localeCompare(b.produto, 'pt-BR'));
  }, [leitura.dados, produtosDoMunicipio]);

  if (leitura.erro) return <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />;
  if (leitura.carregando && !leitura.dados) return <BlocoCarregando oQue="o preço recebido pelo produtor" />;

  return (
    <div className="card cad-cartao" data-bloco="preco-implicito">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          <div className="card-title">
            Preço recebido pelo produtor
            <InfoTooltip rotulo="De onde vem este preço" texto={leitura.dados?.ressalva ?? ''} />
          </div>
          <div className="card-subtitle">
            Série <strong>anual</strong>, do IBGE — {nomeDoMunicipio ?? 'a Região Tracbel inteira'}. É outra série que a
            mensal da CONAB, e as duas não se somam.
          </div>
        </div>
        <SeloProcedencia procedencia={leitura.procedencia} />
      </div>

      {series.length === 0 ? (
        <BlocoVazio
          titulo="Sem produção agrícola carregada neste recorte"
          texto="O preço sai da Produção Agrícola Municipal, e ela não tem linha para este recorte."
        />
      ) : (
        <div className="cad-tabela-wrap dash-tabela-rolagem">
          <table className="cad-tabela dash-tabela">
            <caption className="cad-so-leitor">
              Preço recebido pelo produtor, por cultura e ano, derivado da Produção Agrícola Municipal
            </caption>
            <thead>
              <tr>
                <th scope="col">Cultura</th>
                <th scope="col" className="dash-tabela-numero">Último ano</th>
                <th scope="col" className="dash-tabela-numero">Média de 3 anos</th>
                <th scope="col" className="dash-tabela-numero">Média de 5 anos</th>
                <th scope="col">Unidade</th>
                <th scope="col" className="dash-tabela-numero">Municípios</th>
              </tr>
            </thead>
            <tbody>
              {series.map((s) => {
                const recente = s.anos[0];
                return (
                  <tr key={s.produtoCodigoIbge}>
                    <th scope="row" className="dash-tabela-nome">{s.produto}</th>
                    <td className="dash-tabela-numero">
                      {recente?.precoPorUnidade == null ? (
                        <ValorAusente
                          motivo={`${recente?.ano ?? 'O ano mais recente'}: o IBGE não divulgou os dois números da conta.`}
                          oQue={`o preço de ${s.produto}`}
                        />
                      ) : (
                        <>
                          {reais(recente.precoPorUnidade)}
                          <div className="cad-sub">{recente.ano}</div>
                        </>
                      )}
                    </td>
                    <td className="dash-tabela-numero"><Media media={s.mediaDeTresAnos} unidade={s.unidade} /></td>
                    <td className="dash-tabela-numero"><Media media={s.mediaDeCincoAnos} unidade={s.unidade} /></td>
                    <td>por {s.unidade}</td>
                    <td className="dash-tabela-numero">
                      {/* QUANTOS SUSTENTAM O NÚMERO. Sem isto, "R$ 18.000 a tonelada na região" pode ser de
                          um município só, e ninguém teria como saber. */}
                      {municipioCodigoIbge === null ? nº(s.municipiosComDadoNoUltimoAno) : '—'}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {/* A SÉRIE INTEIRA CONTINUA NA TELA, recolhida — é a hierarquia da issue 33: a primeira camada
          responde "quanto vale hoje e na média", e quem quiser o ano a ano abre. */}
      {series.length > 0 && (
        <details className="cad-recolhivel" data-bloco="preco-implicito-serie">
          <summary>A série ano a ano, desde 2010</summary>
          {series.map((s) => (
            <dl className="terr-numeros" key={s.produtoCodigoIbge}>
              <dt>{s.produto}</dt>
              <dd>
                {s.anos.slice(0, ANOS_NA_PRIMEIRA_CAMADA * 4).map((a) => (
                  <span key={a.ano} className="cad-sub">
                    {a.ano}:{' '}
                    {a.precoPorUnidade === null ? 'sem preço' : reais(a.precoPorUnidade)}
                    {' · '}
                  </span>
                ))}
              </dd>
            </dl>
          ))}
        </details>
      )}
    </div>
  );
}
