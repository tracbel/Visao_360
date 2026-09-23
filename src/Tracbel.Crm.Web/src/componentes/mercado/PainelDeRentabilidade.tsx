/**
 * RENTABILIDADE POR CULTURA — resposta primeiro, planilha depois (fase T4.7).
 *
 * O QUE ESTAVA ERRADO. A aba Rentabilidade abria com duas tabelas empilhadas —
 * preço e custo —, cada uma com dez colunas. A pergunta que a diretoria traz é
 * *"onde sobra dinheiro por hectare?"*, e a resposta estava distribuída em duas
 * planilhas que precisavam ser cruzadas com a cabeça. Painel que começa em
 * tabela não é painel: é relatório.
 *
 * TRÊS CAMADAS, na ordem em que se lê:
 *
 *   1. **RESUMO** — quatro números: a cultura que mais paga, a que menos paga, a
 *      média e quantas culturas entraram. É a resposta.
 *   2. **RANKING** — barras horizontais de margem por hectare, uma por cultura,
 *      com receita e custo visíveis dentro da mesma barra. É o porquê.
 *   3. **TABELA** — recolhida, com produtividade, preço, receita, custo, margem
 *      por hectare, margem por unidade e área. É a evidência.
 *
 * NENHUM NÚMERO NOVO É CALCULADO AQUI. Receita, custo e margem vêm prontos da
 * rota `/v1/territorio/rentabilidade` (issue 159), cada um com a competência
 * dele — o ano da PAM, os meses de preço na média e a safra do custo, que
 * costumam ser diferentes. O painel ordena e desenha; ele não conta.
 *
 * MARGEM NEGATIVA APARECE COMO NEGATIVA, e a barra vai para o outro lado. Uma
 * cultura que não paga o custo é exatamente o que este painel existe para
 * mostrar; escondê-la faria a tela só saber dar boas notícias.
 */

import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterRentabilidadeDasCulturas } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type { RentabilidadeDaCultura } from '../../tipos/mercado';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { CartaoDeIndicador, GradeDeIndicadores } from '../dashboard/Dashboard';
import { InfoTooltip } from '../InfoTooltip';
import { Procedencia } from '../comum/Procedencia';

const reais = (v: number) =>
  v.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 });

const numero = (v: number, casas = 0) =>
  v.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });

/** Só as culturas em que a margem saiu — as outras não têm o que ordenar. */
const comMargem = (linhas: RentabilidadeDaCultura[]) =>
  linhas.filter((l): l is RentabilidadeDaCultura & { margemPorHectare: number } => l.margemPorHectare !== null);

export function PainelDeRentabilidade({ produtosDoMunicipio = [] }: { produtosDoMunicipio?: readonly number[] } = {}) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso(
    (sinal) => obterRentabilidadeDasCulturas(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  if (leitura.carregando) return <BlocoCarregando oQue="a rentabilidade das culturas" />;
  if (leitura.erro) return <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />;

  const linhas = leitura.dados ?? [];
  if (linhas.length === 0)
    return (
      <BlocoVazio
        titulo="A rentabilidade por cultura ainda não foi calculada neste banco"
        texto="Ela cruza o preço (issue 66), o custo da CONAB (issue 67) e a produtividade da PAM. Falta pelo menos uma das três — não é zero nem falta de permissão."
      />
    );

  const ordenadas = [...comMargem(linhas)].sort((a, b) => b.margemPorHectare - a.margemPorHectare);
  const melhor = ordenadas[0] ?? null;
  const pior = ordenadas[ordenadas.length - 1] ?? null;
  const media = ordenadas.length > 0 ? ordenadas.reduce((s, l) => s + l.margemPorHectare, 0) / ordenadas.length : null;

  // A ESCALA DA BARRA SAI DO MAIOR VALOR ABSOLUTO, para que a barra da margem
  // negativa tenha o mesmo comprimento por real que a da positiva — senão a
  // comparação entre elas engana.
  const escala = Math.max(1, ...ordenadas.map((l) => Math.abs(l.margemPorHectare)));

  return (
    <div data-bloco="rentabilidade">
      {/* ---------- CAMADA 1: a resposta ---------- */}
      <GradeDeIndicadores>
        <CartaoDeIndicador
          rotulo="Maior margem"
          destaque
          valor={melhor ? `${reais(melhor.margemPorHectare)}/ha` : null}
          contexto={melhor?.culturaNome}
          oQue="a maior margem"
          motivoSemDado="Nenhuma cultura teve margem apurada: falta preço, custo ou produtividade em todas elas."
          procedencia={null}
        />
        <CartaoDeIndicador
          rotulo="Menor margem"
          valor={pior ? `${reais(pior.margemPorHectare)}/ha` : null}
          contexto={pior?.culturaNome}
          oQue="a menor margem"
          motivoSemDado="Nenhuma cultura teve margem apurada."
        />
        <CartaoDeIndicador
          rotulo="Margem média"
          valor={media === null ? null : `${reais(media)}/ha`}
          contexto={`entre as ${ordenadas.length} culturas com margem`}
          oQue="a margem média"
          motivoSemDado="Sem margem em nenhuma cultura não há média a tirar."
        />
        <CartaoDeIndicador
          rotulo="Culturas apuradas"
          valor={`${ordenadas.length} de ${linhas.length}`}
          contexto={
            linhas.length > ordenadas.length
              ? `${linhas.length - ordenadas.length} sem margem — falta preço, custo ou produtividade`
              : 'todas com preço, custo e produtividade'
          }
          oQue="quantas culturas foram apuradas"
        />
      </GradeDeIndicadores>

      {/* ---------- CAMADA 2: o porquê ---------- */}
      <div className="dash-ranking" aria-label="Margem por hectare, por cultura">
        <div className="dash-painel-cabecalho">
          <h3 className="dash-painel-titulo">
            Margem por hectare
            <InfoTooltip
              rotulo="Como a margem por hectare é calculada"
              texto={
                'Receita por hectare menos custo por hectare. A receita é produtividade × preço médio; o custo é o da ' +
                'CONAB na localidade de referência. As três competências costumam ser DIFERENTES — o ano da PAM, os ' +
                'meses de preço na média e a safra do custo —, e cada linha da tabela abaixo diz as suas.'
              }
            />
          </h3>
          <Procedencia procedencia={null} oQue="a rentabilidade" />
        </div>

        <ul className="dash-ranking-lista">
          {ordenadas.map((l) => {
            const proporcao = Math.abs(l.margemPorHectare) / escala;
            const negativa = l.margemPorHectare < 0;

            return (
              <li key={l.culturaCodigo} className="dash-ranking-item" data-cultura={l.culturaCodigo}>
                <span className="dash-ranking-nome">
                  {l.culturaNome}
                  {/* AS CULTURAS DO MUNICÍPIO ESCOLHIDO GANHAM MARCA (issue 168):
                      a fonte é estadual, mas quem escolheu um município quer
                      saber qual destas linhas é dele. */}
                  {produtosDoMunicipio.length > 0 && <span className="cad-so-leitor"> — cultura da região</span>}
                </span>

                <span className="dash-ranking-barra">
                  <span
                    className={negativa ? 'dash-ranking-preenchimento dash-ranking-negativa' : 'dash-ranking-preenchimento'}
                    style={{ width: `${Math.max(2, proporcao * 100)}%` }}
                    aria-hidden="true"
                  />
                </span>

                <span className="dash-ranking-valor">
                  {reais(l.margemPorHectare)}
                  <InfoTooltip
                    rotulo={`Como a margem de ${l.culturaNome} se compõe`}
                    texto={
                      `Receita ${l.receitaPorHectare === null ? '—' : reais(l.receitaPorHectare)}/ha menos custo ` +
                      `${l.custoPorHectare === null ? '—' : reais(l.custoPorHectare)}/ha. ` +
                      `Produtividade de ${l.anoDaProdutividade ?? '—'}: ${l.produtividadeKgPorHa === null ? '—' : numero(l.produtividadeKgPorHa)} kg/ha. ` +
                      `Preço: média de ${l.mesesDePrecoNaMedia} meses. ` +
                      `Custo: ${l.camadaDoCusto ?? 'camada não decidida (D-P07)'}, safra ${l.safraDoCusto ?? '—'}, em ${l.localDoCusto ?? '—'}.`
                    }
                  />
                </span>
              </li>
            );
          })}
        </ul>
      </div>

      {/* ---------- CAMADA 3: a evidência ---------- */}
      <details className="cad-recolhivel" data-bloco="rentabilidade-detalhe">
        <summary>A conta de cada cultura — produtividade, preço, receita, custo e área</summary>

        <div className="cad-tabela-wrap dash-tabela-rolagem">
          <table className="cad-tabela dash-tabela">
            <thead>
              <tr>
                <th scope="col">Cultura</th>
                <th scope="col">Produtividade</th>
                <th scope="col">Preço médio</th>
                <th scope="col">Receita/ha</th>
                <th scope="col">Custo/ha</th>
                <th scope="col">Margem/ha</th>
                <th scope="col">Margem/{'unidade'}</th>
                <th scope="col">Área colhida</th>
              </tr>
            </thead>
            <tbody>
              {linhas.map((l) => (
                <tr key={l.culturaCodigo} data-cultura={l.culturaCodigo}>
                  <th scope="row" className="dash-tabela-nome">
                    {l.culturaNome}
                  </th>
                  <td className="dash-tabela-numero">
                    {l.produtividadeKgPorHa === null ? '—' : `${numero(l.produtividadeKgPorHa)} kg/ha`}
                  </td>
                  <td className="dash-tabela-numero">
                    {l.precoMedioPorKg === null ? '—' : `${numero(l.precoMedioPorKg, 3)} R$/kg`}
                  </td>
                  <td className="dash-tabela-numero">{l.receitaPorHectare === null ? '—' : reais(l.receitaPorHectare)}</td>
                  <td className="dash-tabela-numero">{l.custoPorHectare === null ? '—' : reais(l.custoPorHectare)}</td>
                  <td className="dash-tabela-numero">
                    {l.margemPorHectare === null ? (
                      <span title={l.fraseDoMotivo}>—</span>
                    ) : (
                      reais(l.margemPorHectare)
                    )}
                  </td>
                  <td className="dash-tabela-numero">
                    {l.margemPorUnidade === null ? '—' : `${reais(l.margemPorUnidade)} / ${l.unidadeComercial}`}
                  </td>
                  <td className="dash-tabela-numero">
                    {l.areaColhidaHectares === null ? '—' : `${numero(l.areaColhidaHectares)} ha`}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </details>
    </div>
  );
}
