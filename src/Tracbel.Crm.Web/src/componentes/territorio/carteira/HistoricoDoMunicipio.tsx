/**
 * A ABA HISTÓRICO DA FICHA — o município ao longo do tempo (27/09/2026; a aba é
 * da maquete de 23/09/2026, que não mostra o conteúdo dela).
 *
 * DUAS SÉRIES QUE EXISTEM, UMA QUE NÃO:
 *   - as VENDAS por ano fiscal (nov → out), em reais pelo Protheus e em
 *     unidades pelo ART, em colunas separadas — as duas nunca se somam (D-P08).
 *     O ano em curso vem até o último mês fechado, e embaixo dele o MESMO
 *     TRECHO do ano anterior, que é a comparação que a diretoria faz;
 *   - a LAVOURA de cada ano da PAM carregado, com todas as culturas (issue 168);
 *   - a COBERTURA DE VISITA não tem série: é medida no instante, e fica com o
 *     traço e o motivo.
 *
 * O ANO QUE A CARGA NÃO COBRE INTEIRO fica com o traço e o motivo do servidor:
 * um ano de seis meses ao lado de um de doze se leria como queda.
 */

import { ChartLine } from 'lucide-react';
import type { AnoFiscalDoMunicipio, HistoricoDoMunicipio as Historico } from '../../../tipos/territorio';
import { InfoTooltip } from '../../InfoTooltip';
import { BlocoErro } from '../../cadastro/EstadosDeTela';
import { ValorAusente } from '../../comum/ValorAusente';
import { reaisCompactos, reaisDaProducao } from '../escalas';
import { mes, nº } from '../indicadoresDaAdr';

/** O que a ficha sabe da leitura do histórico — a ficha lê, a aba mostra. */
export type EstadoDoHistorico = {
  /** Nulo enquanto lê — e quando a resposta é de outro município. */
  historico: Historico | null;
  erro: Error | null;
  recarregar: () => void;
  /** Falso quando a ficha foi aberta sem os filtros da página e não leu o histórico. */
  lido: boolean;
};

/** A variação relativa, com o sinal escrito; nula quando a base é zero. */
function variacao(atual: number, anterior: number): string | null {
  if (anterior === 0) return null;
  const pontos = (100 * (atual - anterior)) / Math.abs(anterior);
  const casas = Math.abs(pontos) >= 10 ? 0 : 1;
  const texto = Math.abs(pontos).toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });
  return `${pontos > 0 ? '+' : pontos < 0 ? '−' : ''}${texto}%`;
}

function Reais({ ano, valor, oQue }: { ano: AnoFiscalDoMunicipio; valor: (v: NonNullable<AnoFiscalDoMunicipio['vendas']>) => number; oQue: string }) {
  if (ano.vendas === null) return <ValorAusente motivo={ano.motivoSemVendas ?? 'Sem faturamento carregado.'} oQue={oQue} />;
  return <>{reaisCompactos(valor(ano.vendas))}</>;
}

function Unidades({ ano }: { ano: AnoFiscalDoMunicipio }) {
  if (ano.maquinasVendidas === null)
    return (
      <ValorAusente motivo={ano.motivoSemMaquinas ?? 'Sem venda do ART.'} oQue={`as máquinas vendidas no ${rotuloDoAno(ano)}`} />
    );
  return <>{nº(ano.maquinasVendidas)}</>;
}

/** "FY26": o ano fiscal leva o nome do ano em que termina. */
function rotuloDoAno(ano: AnoFiscalDoMunicipio): string {
  return `FY${String(ano.anoFiscal % 100).padStart(2, '0')}`;
}

function LinhaDoAno({ ano, mesmoTrecho = false }: { ano: AnoFiscalDoMunicipio; mesmoTrecho?: boolean }) {
  const nome = rotuloDoAno(ano);
  return (
    <tr data-ano-fiscal={ano.anoFiscal} data-mesmo-trecho={mesmoTrecho || undefined}>
      <th scope="row">
        {nome}
        <span className="cad-sub">
          {' '}
          {mes(ano.inicio)} a {mes(ano.fim)}
          {mesmoTrecho ? ' · o mesmo trecho' : ano.emCurso ? ' · em curso' : ''}
        </span>
      </th>
      <td className="terr-num">
        <Reais ano={ano} valor={(v) => v.valorLiquido} oQue={`as vendas do ${nome}`} />
      </td>
      <td className="terr-num">
        <Reais ano={ano} valor={(v) => v.maquina} oQue={`as vendas de máquina do ${nome}`} />
      </td>
      <td className="terr-num">
        <Reais ano={ano} valor={(v) => v.posVenda} oQue={`o pós-venda do ${nome}`} />
      </td>
      <td className="terr-num">
        <Unidades ano={ano} />
      </td>
    </tr>
  );
}

/** A frase do ano em curso contra o mesmo trecho do anterior — só com os dois números de cada fonte. */
function FraseDaComparacao({ atual, anterior }: { atual: AnoFiscalDoMunicipio; anterior: AnoFiscalDoMunicipio }) {
  const emReais =
    atual.vendas && anterior.vendas ? variacao(atual.vendas.valorLiquido, anterior.vendas.valorLiquido) : null;
  const emUnidades =
    atual.maquinasVendidas !== null && anterior.maquinasVendidas !== null
      ? variacao(atual.maquinasVendidas, anterior.maquinasVendidas)
      : null;
  if (emReais === null && emUnidades === null) return null;
  return (
    <p className="cad-sub" data-comparacao-do-historico>
      {rotuloDoAno(atual)} até {mes(atual.fim)} contra {rotuloDoAno(anterior)} no mesmo trecho:{' '}
      {emReais !== null && <strong>{emReais} em reais</strong>}
      {emReais !== null && emUnidades !== null && ' · '}
      {emUnidades !== null && <strong>{emUnidades} em unidades</strong>}. Reais e unidades são medidas separadas e
      não se somam.
    </p>
  );
}

export function HistoricoDoMunicipio({ nome, estado }: { nome: string; estado: EstadoDoHistorico }) {
  const { historico } = estado;
  const anos = historico ? [...historico.anosFiscais].reverse() : [];
  const corrente = historico?.anosFiscais.at(-1) ?? null;
  const lavoura = historico ? [...historico.lavoura].reverse() : [];

  return (
    <section className="terr-ficha-bloco" data-bloco-da-ficha="historico" aria-labelledby="ficha-historico-titulo">
      <h3 id="ficha-historico-titulo" className="terr-ficha-bloco-titulo">
        Histórico de {nome}
      </h3>
      <p className="terr-ficha-bloco-subtitulo">
        Vendas por ano fiscal (nov → out) e lavoura por ano da PAM. A cobertura de visita não tem série.
      </p>

      {!estado.lido ? (
        <div className="terr-ficha-sem-serie">
          <ChartLine size={22} strokeWidth={1.8} aria-hidden="true" />
          <span>
            <span aria-hidden="true">— </span>Histórico não lido
          </span>
          <InfoTooltip
            rotulo="Por que o histórico do município não foi lido"
            texto="O histórico é lido com os filtros de alcance da página — visão, filiais, tipo de produto e CEN —, e esta ficha foi aberta sem eles."
          />
        </div>
      ) : estado.erro !== null && historico === null ? (
        <BlocoErro erro={estado.erro} aoTentarDeNovo={estado.recarregar} />
      ) : historico === null ? (
        <p className="cad-sub" aria-live="polite">
          Lendo o histórico de {nome}…
        </p>
      ) : (
        <>
          <h4 className="terr-detalhe-subtitulo">Vendas por ano fiscal</h4>
          <div className="cad-tabela-wrap">
            <table className="cad-tabela terr-tabela-compacta" data-historico="vendas">
              <caption className="cad-so-leitor">Vendas de {nome} por ano fiscal, em reais e em unidades</caption>
              <thead>
                <tr>
                  <th scope="col">Ano fiscal</th>
                  <th scope="col" className="terr-num">
                    Vendas (R$)
                  </th>
                  <th scope="col" className="terr-num">
                    Máquina (R$)
                  </th>
                  <th scope="col" className="terr-num">
                    Pós-venda (R$)
                  </th>
                  <th scope="col" className="terr-num">
                    Máquinas (un., ART)
                  </th>
                </tr>
              </thead>
              <tbody>
                {anos.map((ano) => (
                  <LinhaDoAno key={ano.anoFiscal} ano={ano} />
                ))}
                {historico.mesmoTrechoDoAnoAnterior && <LinhaDoAno ano={historico.mesmoTrechoDoAnoAnterior} mesmoTrecho />}
              </tbody>
            </table>
          </div>
          {corrente && historico.mesmoTrechoDoAnoAnterior && (
            <FraseDaComparacao atual={corrente} anterior={historico.mesmoTrechoDoAnoAnterior} />
          )}
          <p className="cad-sub">
            Reais do faturamento do Protheus (valor líquido); unidades do ART, pela data do faturamento. Pós-venda é
            peça + serviço, provisório.
            {historico.primeiraCompetenciaDoFaturamento &&
              ` O faturamento carregado começa em ${mes(historico.primeiraCompetenciaDoFaturamento)}`}
            {historico.primeiroMesDoArt && `${historico.primeiraCompetenciaDoFaturamento ? ';' : ' O'} o ART, em ${mes(historico.primeiroMesDoArt)}`}
            {historico.primeiraCompetenciaDoFaturamento || historico.primeiroMesDoArt
              ? ' — ano que a carga não cobre inteiro fica com o traço.'
              : ''}
          </p>

          <h4 className="terr-detalhe-subtitulo">Lavoura por ano (PAM)</h4>
          {lavoura.length === 0 ? (
            <p className="cad-nada">
              <ValorAusente motivo="A Produção Agrícola Municipal não foi carregada para este município." oQue="a lavoura" /> PAM
              não carregada aqui.
            </p>
          ) : (
            <div className="cad-tabela-wrap">
              <table className="cad-tabela terr-tabela-compacta" data-historico="lavoura">
                <caption className="cad-so-leitor">Lavoura de {nome} por ano da PAM</caption>
                <thead>
                  <tr>
                    <th scope="col">Ano</th>
                    <th scope="col" className="terr-num">
                      Área plantada (ha)
                    </th>
                    <th scope="col" className="terr-num">
                      Área colhida (ha)
                    </th>
                    <th scope="col" className="terr-num">
                      Valor da produção
                    </th>
                    <th scope="col">Maiores culturas</th>
                  </tr>
                </thead>
                <tbody>
                  {lavoura.map((l) => (
                    <tr key={l.ano} data-ano-da-pam={l.ano}>
                      <th scope="row">{l.ano}</th>
                      <td className="terr-num">
                        {l.areaPlantadaHectares === null ? '—' : nº(Math.round(l.areaPlantadaHectares))}
                      </td>
                      <td className="terr-num">
                        {l.areaColhidaHectares === null ? '—' : nº(Math.round(l.areaColhidaHectares))}
                      </td>
                      <td className="terr-num">
                        {l.valorDaProducaoMilReais === null ? '—' : reaisDaProducao(l.valorDaProducaoMilReais)}
                      </td>
                      <td>
                        {l.culturas.length === 0
                          ? '—'
                          : [...l.culturas]
                              .sort((a, b) => (b.areaPlantadaHectares ?? 0) - (a.areaPlantadaHectares ?? 0))
                              .slice(0, 3)
                              .map((c) => `${c.produtoNome} ${nº(Math.round(c.areaPlantadaHectares ?? 0))} ha`)
                              .join(' · ')}
                        {l.culturas.length > 3 && <span className="cad-sub"> · mais {nº(l.culturas.length - 3)}</span>}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          <p className="cad-sub">
            Todas as culturas que a PAM divulgou com área; o café entra uma vez só, pelo total. Sigilo do IBGE não é zero:
            a cultura sem área divulgada não entra na soma.
          </p>

          <h4 className="terr-detalhe-subtitulo">Cobertura de visita</h4>
          <div className="terr-ficha-sem-serie">
            <ChartLine size={22} strokeWidth={1.8} aria-hidden="true" />
            <span>
              <span aria-hidden="true">— </span>Sem série histórica
            </span>
            <InfoTooltip
              rotulo="Por que a cobertura de visita não tem histórico"
              texto="A cobertura é medida no instante: o CRM guarda o último contato de cada vínculo da carteira, e não o estado da carteira em cada mês. Uma série montada sobre os vínculos de hoje se leria como história — e não é. A cobertura de hoje está na aba Estrutura."
            />
          </div>
        </>
      )}
    </section>
  );
}
