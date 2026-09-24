/**
 * RENTABILIDADE POR CULTURA — a aba da maquete `momento-rentabilidade.png`
 * (T4.7; na forma da maquete desde a fidelidade às maquetes, fase 3).
 *
 * TRÊS CAMADAS, na ordem em que se lê — a mesma da T4.7, agora com o desenho da
 * maquete:
 *
 *   1. **CARTÕES** — a melhor margem, a cultura de maior área, a margem média
 *      ponderada da Região Tracbel e a tendência (que ainda não existe).
 *   2. **RANKING E GRÁFICO** — a margem por cultura, ordenável pelas quatro
 *      colunas, e receita × custo × margem lado a lado.
 *   3. **DETALHAMENTO** — a tabela inteira, VISÍVEL (a maquete a mostra aberta).
 *
 * NENHUM NÚMERO NOVO É CALCULADO AQUI, com duas exceções ditas: a margem % é a
 * razão entre margem e receita, que vêm prontas; e a média da Região Tracbel é
 * a margem de cada cultura ponderada pela ÁREA COLHIDA DA REGIÃO — as duas com
 * teste em `momento/contas.teste.ts`.
 *
 * A MARGEM É REFERÊNCIA ESTADUAL (issue 168): preço e custo são publicados para
 * São Paulo e para a localidade de referência da CONAB. Escolher um município
 * muda QUAIS culturas vêm primeiro, e não o número — e a tela diz isso na dica
 * de cada título, onde a maquete não tem espaço para a frase.
 *
 * OS PAINÉIS ANTIGOS DE PREÇO E DE CUSTO NÃO ESTÃO NA MAQUETE, e continuam na
 * tela, inteiros: o botão "Ver séries de preço e custo" do detalhamento os abre
 * embaixo da tabela. A competência de cada linha está no ⋮ dela.
 *
 * MARGEM NEGATIVA APARECE COMO NEGATIVA, e a barra vira hachura. Uma cultura
 * que não paga o custo é exatamente o que este painel existe para mostrar.
 */

import { BarChart3, ChevronRight, Sprout, TrendingUp, Trophy } from 'lucide-react';
import { useId, useMemo, useState } from 'react';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterCatalogoDoMercado } from '../../dados/api/potencial';
import { obterRentabilidadeDasCulturas } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type { RentabilidadeDaCultura } from '../../tipos/mercado';
import type { CulturaNoCatalogo } from '../../tipos/potencial';
import type { IndicadoresDoMunicipio } from '../../tipos/territorio';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { ValorAusente } from '../comum/ValorAusente';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import { PainelDeCustos } from '../territorio/PainelDeCustos';
import { PainelDePrecos } from '../territorio/PainelDePrecos';
import { areaColhidaNoRecorte, comAsCulturasDoMunicipioPrimeiro } from './culturasDoMunicipio';
import {
  CRITERIOS_DA_RENTABILIDADE,
  daMaiorArea,
  margemMediaPonderada,
  margemPercentual,
  ordenarRentabilidade,
  valorDoCriterio,
  type CriterioDaRentabilidade,
} from './momento/contas';
import { numero, reais } from './momento/formatos';
import { CORES_DA_RENTABILIDADE } from './momento/cores';
import { GraficoReceitaCustoMargem } from './momento/GraficoReceitaCustoMargem';
import { NomeDaCultura } from './momento/IconeDaCultura';
import {
  CartaoDoMomento,
  FileiraDeCartoes,
  Legenda,
  LinhaDePaineis,
  MenuDaLinha,
  PainelDoMomento,
  Seletor,
} from './momento/pecas';

/** A frase da regra da issue 168 — ela mora na dica dos títulos, e não no corpo. */
function referenciaEstadual(nomeDoMunicipio: string | null): string {
  return (
    'Referência: São Paulo (preço da CONAB e da Socicana) e a localidade de referência da CONAB (custo). Não existe ' +
    'versão municipal destas séries, e a tela não reparte um número estadual por município — isso inventaria uma ' +
    'precisão que a fonte não tem.' +
    (nomeDoMunicipio
      ? ` Escolher ${nomeDoMunicipio} destaca as culturas dele e as põe primeiro; não muda o número.`
      : '')
  );
}

/** Preço por quilo: três casas abaixo de R$ 1 — o kg da cana vale centavos. */
const precoPorKg = (v: number) => `${reais(v, v < 1 ? 3 : 2)} / kg`;

function formatarCriterio(v: number, criterio: CriterioDaRentabilidade): string {
  return criterio === 'margemPercentual' ? `${numero(v * 100, 0)}%` : reais(v);
}

/** Leva a pessoa até a linha da cultura no detalhamento, e a faz piscar de leve. */
function irParaALinha(idDaTabela: string, cultura: string) {
  const linha = document.getElementById(idDaTabela)?.querySelector<HTMLElement>(`tr[data-cultura="${cultura}"]`);
  if (!linha) return;
  linha.scrollIntoView?.({ block: 'center', behavior: 'smooth' });
  linha.dataset.achada = 'false';
  // O reflow entre as duas atribuições reinicia a animação num segundo clique.
  void linha.offsetWidth;
  linha.dataset.achada = 'true';
}

function BotaoIr({ rotulo, aoClicar }: { rotulo: string; aoClicar?: () => void }) {
  return (
    <button type="button" className="mom-cartao-ir" aria-label={rotulo} disabled={!aoClicar} onClick={aoClicar}>
      <ChevronRight size={18} strokeWidth={2} aria-hidden="true" />
    </button>
  );
}

export function PainelDeRentabilidade({
  produtosDoMunicipio = [],
  nomeDoMunicipio = null,
  municipios = [],
}: {
  produtosDoMunicipio?: readonly number[];
  nomeDoMunicipio?: string | null;
  /** Os municípios da leitura — a área colhida da Região Tracbel, peso da média. */
  municipios?: readonly IndicadoresDoMunicipio[];
} = {}) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso(
    (sinal) => obterRentabilidadeDasCulturas(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );
  // O CATÁLOGO LIGA A CULTURA À PAM DOS MUNICÍPIOS (issue 165): é por ele que a
  // área colhida da Região Tracbel chega a cada linha. Sem ele (permissão ou
  // banco novo), a área e a média saem com o motivo — e nada mais some.
  const catalogo = useRecurso((sinal) => obterCatalogoDoMercado(contexto, sinal), [contexto.empresa, contexto.usuario]);

  const [criterio, setCriterio] = useState<CriterioDaRentabilidade>('margem');
  const [series, setSeries] = useState(false);
  const idDaTabela = useId();
  const idDasSeries = useId();

  const culturasDoCatalogo = useMemo(
    () =>
      comAsCulturasDoMunicipioPrimeiro(
        (catalogo.dados?.culturas ?? []).filter((c: CulturaNoCatalogo) => c.estaAtiva),
        produtosDoMunicipio,
      ),
    [catalogo.dados, produtosDoMunicipio],
  );

  /** A área colhida de cada cultura na Região Tracbel do recorte — ou nula, com o motivo. */
  const areaDe = useMemo(() => {
    const mapa = new Map<string, number | null>();
    for (const c of culturasDoCatalogo) mapa.set(c.codigo, areaColhidaNoRecorte(c, municipios));
    return (l: RentabilidadeDaCultura) => mapa.get(l.culturaCodigo) ?? null;
  }, [culturasDoCatalogo, municipios]);

  const doMunicipio = useMemo(() => {
    const codigos = new Set<string>();
    for (const c of culturasDoCatalogo)
      if (c.produtos.some((p) => produtosDoMunicipio.includes(p.codigoIbge))) codigos.add(c.codigo);
    return codigos;
  }, [culturasDoCatalogo, produtosDoMunicipio]);

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

  const semArea = catalogo.erro
    ? 'O catálogo de culturas não respondeu (ele exige a permissão de leitura dos parâmetros do potencial), e é ele que liga cada cultura à área da PAM dos municípios. Sem ele a tela não sabe a área — e não a estima.'
    : 'Nenhum município da Região Tracbel neste recorte tem área colhida divulgada (PAM) para as culturas com margem.';

  // ---------- os quatro cartões ----------
  const porMargem = ordenarRentabilidade(linhas, 'margem');
  const melhor = porMargem.find((l) => l.margemPorHectare !== null) ?? null;
  const destaque = daMaiorArea(linhas, areaDe);
  const media = margemMediaPonderada(linhas.map((l) => ({ margem: l.margemPorHectare, area: areaDe(l) })));

  // ---------- o ranking ----------
  const ranking = ordenarRentabilidade(linhas, criterio);
  const escala = Math.max(
    Number.EPSILON,
    ...ranking.map((l) => Math.abs(valorDoCriterio(l, criterio) ?? 0)),
  );
  const rotuloDoCriterio = CRITERIOS_DA_RENTABILIDADE.find((c) => c.id === criterio)!.rotulo;

  // ---------- a tabela: as culturas do município escolhido primeiro (issue 168) ----------
  const ordemDoCatalogo = new Map(culturasDoCatalogo.map((c, i) => [c.codigo, i]));
  const naTabela = [...linhas].sort(
    (a, b) =>
      (ordemDoCatalogo.get(a.culturaCodigo) ?? Number.MAX_SAFE_INTEGER) -
      (ordemDoCatalogo.get(b.culturaCodigo) ?? Number.MAX_SAFE_INTEGER),
  );

  return (
    <div className="mom-painel-da-aba" data-bloco="rentabilidade">
      <FileiraDeCartoes>
        <CartaoDoMomento
          icone={Trophy}
          tom="verde"
          rotulo="Melhor margem/ha"
          oQue="a melhor margem por hectare"
          dica={
            'A cultura de maior margem por hectare: receita (produtividade da PAM × preço médio da CONAB) menos o ' +
            'custo da CONAB. ' +
            referenciaEstadual(nomeDoMunicipio) +
            ' Por isso o cartão diz a cultura, e não um município.'
          }
          valor={melhor ? reais(melhor.margemPorHectare!) : null}
          motivoSemDado="Nenhuma cultura teve margem apurada: falta preço, custo ou produtividade em todas elas."
          apoio={melhor?.culturaNome}
          acao={
            <BotaoIr
              rotulo={melhor ? `Ver ${melhor.culturaNome} no detalhamento` : 'Sem cultura para mostrar'}
              aoClicar={melhor ? () => irParaALinha(idDaTabela, melhor.culturaCodigo) : undefined}
            />
          }
        />

        <CartaoDoMomento
          icone={Sprout}
          tom="azul"
          rotulo="Cultura destaque"
          oQue="a cultura destaque"
          dica={
            'Critério: a cultura de MAIOR ÁREA COLHIDA nos municípios da Região Tracbel do recorte (PAM do IBGE, ' +
            'somada pelo catálogo de culturas). "Destaque" aqui é a que mais ocupa a terra, e não a que mais paga — ' +
            'essa é a Melhor margem/ha, ao lado. A margem embaixo é a dela, referência de São Paulo.'
          }
          valor={destaque?.culturaNome ?? null}
          motivoSemDado={semArea}
          apoio={
            destaque ? (
              destaque.margemPorHectare === null ? (
                <>margem sem dado</>
              ) : (
                <>{reais(destaque.margemPorHectare)} / ha</>
              )
            ) : null
          }
          acao={
            <BotaoIr
              rotulo={destaque ? `Ver ${destaque.culturaNome} no detalhamento` : 'Sem cultura para mostrar'}
              aoClicar={destaque ? () => irParaALinha(idDaTabela, destaque.culturaCodigo) : undefined}
            />
          }
        />

        <CartaoDoMomento
          icone={BarChart3}
          tom="roxo"
          rotulo="Média da Região Tracbel"
          oQue="a margem média da Região Tracbel"
          dica={
            'Σ (margem/ha × área colhida) ÷ Σ área colhida' +
            (media.culturas > 0
              ? `, com as ${media.culturas} culturas que têm margem e área na Região Tracbel (${numero(media.areaTotal)} ha colhidos).`
              : '.') +
            ' A margem de cada cultura é a referência estadual; o PESO é a área colhida (PAM) dos municípios da ' +
            'Região Tracbel no recorte consultado — sub-região e loja escolhidas. Ponderada, e não a média simples: a ' +
            'cultura de 12 mil hectares não pesa o mesmo que a de 280 mil.'
          }
          valor={media.media === null ? null : reais(media.media)}
          unidade="/ ha"
          motivoSemDado={semArea}
          apoio="margem média ponderada"
          acao={
            <BotaoIr
              rotulo="Ver o detalhamento por cultura"
              aoClicar={() => document.getElementById(idDaTabela)?.scrollIntoView?.({ block: 'center', behavior: 'smooth' })}
            />
          }
        />

        <CartaoDoMomento
          icone={TrendingUp}
          tom="laranja"
          rotulo="Tendência"
          oQue="a tendência da margem"
          valor={null}
          motivoSemDado="Falta a série do ano anterior: a rentabilidade é calculada só para o ano da PAM mais recente (issue 159), e sem o ano anterior não há tendência a medir. Não é zero nem estabilidade."
          apoio="na margem média, vs. ano anterior"
          acao={<BotaoIr rotulo="Tendência sem série" />}
        />
      </FileiraDeCartoes>

      <LinhaDePaineis variante="rentabilidade">
        <PainelDoMomento
          titulo={`Ranking de culturas por ${rotuloDoCriterio.toLowerCase()}`}
          dica={
            'Receita por hectare menos custo por hectare. A receita é produtividade × preço médio; o custo é o da ' +
            'CONAB na localidade de referência. As três competências costumam ser DIFERENTES — o ano da PAM, os ' +
            'meses de preço na média e a safra do custo —, e o ⋮ de cada linha do detalhamento diz as suas. ' +
            referenciaEstadual(nomeDoMunicipio)
          }
          subtitulo="Por hectare, com a produtividade média da PAM e os preços da CONAB em São Paulo."
          direita={
            <Seletor rotulo="Ordenar por" valor={criterio} opcoes={CRITERIOS_DA_RENTABILIDADE} aoMudar={setCriterio} />
          }
          data-ranking={criterio}
        >
          <ol className="mom-ranking" aria-label={`Culturas por ${rotuloDoCriterio}`}>
            {ranking.map((l, i) => {
              const v = valorDoCriterio(l, criterio);
              const negativa = v !== null && v < 0;
              return (
                <li key={l.culturaCodigo} className="mom-ranking-linha" data-cultura={l.culturaCodigo}>
                  <span className="mom-ranking-nome">
                    <NomeDaCultura nome={l.culturaNome} />
                    {doMunicipio.has(l.culturaCodigo) && (
                      <span className="cad-so-leitor"> — cultura do município escolhido</span>
                    )}
                  </span>
                  <span className="mom-barra" aria-hidden="true">
                    {v !== null && (
                      <span
                        data-negativa={negativa ? 'true' : undefined}
                        style={{
                          width: `${Math.max(2, (Math.abs(v) / escala) * 100)}%`,
                          background: negativa ? undefined : i < 3 ? '#3E9B57' : '#A5D6A7',
                        }}
                      />
                    )}
                  </span>
                  <span className="mom-ranking-valor">
                    {v === null ? (
                      <ValorAusente
                        motivo={l.fraseDoMotivo || `${rotuloDoCriterio} de ${l.culturaNome} não foi apurado.`}
                        oQue={`${rotuloDoCriterio.toLowerCase()} de ${l.culturaNome}`}
                      />
                    ) : (
                      formatarCriterio(v, criterio)
                    )}
                  </span>
                </li>
              );
            })}
          </ol>
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Receita, custo e margem por cultura"
          dica={
            'Barras: receita e custo por hectare. Linhas: a margem por hectare (mesmo eixo, à esquerda) e a margem ' +
            'como fração da receita (eixo da direita). ' +
            referenciaEstadual(nomeDoMunicipio)
          }
          subtitulo="Valores por hectare (R$) e margem (%), referência de São Paulo."
          direita={
            <>
              <Seletor
                rotulo="Visualizar"
                valor="valor"
                opcoes={[{ id: 'valor', rotulo: 'Por valor (R$/ha)' }]}
                motivoDesligado="Só há um modo hoje: a rota devolve receita, custo e margem POR HECTARE. Por unidade (saca, caixa, tonelada) existe só a margem — receita e custo por unidade não vêm prontos, e a tela não os deriva."
              />
              <Legenda
                itens={[
                  { nome: 'Receita/ha', cor: CORES_DA_RENTABILIDADE.receita },
                  { nome: 'Custo/ha', cor: CORES_DA_RENTABILIDADE.custo },
                  { nome: 'Margem/ha', cor: CORES_DA_RENTABILIDADE.margem, forma: 'linha' },
                  { nome: 'Margem %', cor: CORES_DA_RENTABILIDADE.margemPercentual, forma: 'anel' },
                ]}
              />
            </>
          }
        >
          <div role="img" aria-label="Receita, custo e margem por hectare de cada cultura; os números estão no detalhamento por cultura.">
            <MolduraDeGrafico altura={210}>
              {(l, a) => (
                <GraficoReceitaCustoMargem
                  culturas={linhas.map((x) => x.culturaNome)}
                  receita={linhas.map((x) => x.receitaPorHectare)}
                  custo={linhas.map((x) => x.custoPorHectare)}
                  margem={linhas.map((x) => x.margemPorHectare)}
                  margemPercentual={linhas.map(margemPercentual)}
                  largura={l}
                  altura={a}
                />
              )}
            </MolduraDeGrafico>
          </div>
        </PainelDoMomento>
      </LinhaDePaineis>

      <PainelDoMomento
        titulo="Detalhamento por cultura"
        dica={
          'Receita, custo e margem por hectare vêm prontos da rota de rentabilidade (issue 159); a margem % é margem ' +
          '÷ receita. A área colhida é a da Região Tracbel no recorte (PAM); a de São Paulo, que a rota usa na margem ' +
          'total, está no ⋮ de cada linha. ' +
          referenciaEstadual(nomeDoMunicipio)
        }
        direita={
          <button
            type="button"
            className="mom-botao"
            aria-expanded={series}
            aria-controls={idDasSeries}
            onClick={() => setSeries((s) => !s)}
          >
            {series ? 'Fechar as séries' : 'Ver séries de preço e custo'}
          </button>
        }
      >
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela" id={idDaTabela}>
            <thead>
              <tr>
                <th scope="col">Cultura</th>
                <th scope="col" className="mom-num">Receita / ha</th>
                <th scope="col" className="mom-num">Custo / ha</th>
                <th scope="col" className="mom-num">Margem / ha</th>
                <th scope="col" className="mom-num">Margem %</th>
                <th scope="col" className="mom-num">Produtividade</th>
                <th scope="col" className="mom-num">Preço médio</th>
                <th scope="col" className="mom-num">Área colhida (ha)</th>
                <th scope="col" className="mom-acoes">
                  <span className="cad-so-leitor">Detalhes</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {naTabela.map((l) => {
                const pct = margemPercentual(l);
                const area = areaDe(l);
                return (
                  <tr
                    key={l.culturaCodigo}
                    data-cultura={l.culturaCodigo}
                    data-destaque={doMunicipio.has(l.culturaCodigo) ? 'true' : undefined}
                  >
                    <th scope="row">
                      <NomeDaCultura nome={l.culturaNome} />
                    </th>
                    <td className="mom-num">{l.receitaPorHectare === null ? '—' : reais(l.receitaPorHectare)}</td>
                    <td className="mom-num">{l.custoPorHectare === null ? '—' : reais(l.custoPorHectare)}</td>
                    <td className={`mom-num ${l.margemPorHectare !== null && l.margemPorHectare < 0 ? 'mom-baixa' : ''}`}>
                      {l.margemPorHectare === null ? (
                        <ValorAusente motivo={l.fraseDoMotivo} oQue={`a margem de ${l.culturaNome}`} />
                      ) : (
                        reais(l.margemPorHectare)
                      )}
                    </td>
                    <td className="mom-num mom-roxo">{pct === null ? '—' : `${numero(pct * 100, 0)}%`}</td>
                    <td className="mom-num">
                      {l.produtividadeKgPorHa === null ? '—' : `${numero(l.produtividadeKgPorHa)} kg/ha`}
                    </td>
                    <td className="mom-num">{l.precoMedioPorKg === null ? '—' : precoPorKg(l.precoMedioPorKg)}</td>
                    <td className="mom-num">
                      {area === null ? (
                        <ValorAusente motivo={semArea} oQue={`a área colhida de ${l.culturaNome}`} />
                      ) : (
                        numero(area)
                      )}
                    </td>
                    <td className="mom-acoes">
                      <MenuDaLinha rotulo={`Como a margem de ${l.culturaNome} se compõe`}>
                        <dl>
                          <dt>Produtividade</dt>
                          <dd>PAM {l.anoDaProdutividade ?? '—'}</dd>
                          <dt>Preço</dt>
                          <dd>
                            média de {l.mesesDePrecoNaMedia} {l.mesesDePrecoNaMedia === 1 ? 'mês' : 'meses'} (CONAB)
                          </dd>
                          <dt>Custo</dt>
                          <dd>
                            CONAB, {l.localDoCusto ?? '—'}, safra {l.safraDoCusto ?? '—'}, camada{' '}
                            {l.camadaDoCusto?.toLowerCase() ?? 'não decidida (D-P07)'}
                          </dd>
                          <dt>Margem por unidade</dt>
                          <dd>
                            {l.margemPorUnidade === null ? '—' : `${reais(l.margemPorUnidade)} / ${l.unidadeComercial}`}
                          </dd>
                          <dt>Área colhida em SP</dt>
                          <dd>{l.areaColhidaHectares === null ? '—' : `${numero(l.areaColhidaHectares)} ha`}</dd>
                          <dt>Margem total em SP</dt>
                          <dd>{l.margemTotal === null ? '—' : reais(l.margemTotal, 0)}</dd>
                        </dl>
                        {l.margemPorHectare === null && l.fraseDoMotivo && <p>{l.fraseDoMotivo}</p>}
                        <p>O custo é da localidade de referência da CONAB, e não do município escolhido.</p>
                      </MenuDaLinha>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {/* AS DUAS SÉRIES QUE COMPÕEM A MARGEM — preço e custo, com tabela,
            gráfico e a origem de cada série. Não estão na maquete; continuam a
            um clique, inteiras (decisão 3 do usuário). */}
        {series && (
          <div className="mom-series" id={idDasSeries} data-bloco="rentabilidade-fontes">
            <PainelDePrecos produtosDoMunicipio={produtosDoMunicipio} />
            <PainelDeCustos produtosDoMunicipio={produtosDoMunicipio} />
          </div>
        )}
      </PainelDoMomento>
    </div>
  );
}
