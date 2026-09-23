/**
 * A FICHA DO MUNICÍPIO EM TRÊS CAMADAS (documento 50, §6; fase T4).
 *
 * Esta é a tela que motivou a reorganização inteira: quarenta medidas com o
 * mesmo peso visual, em cinco a seis colunas de 240 px, com rótulos quebrados em
 * quatro linhas e prosa dentro do valor.
 *
 *   1. EXECUTIVA, sempre aberta — os quatro números de decisão.
 *   2. INTERMEDIÁRIA, aberta — lavoura e estrutura, em DUAS colunas, cada medida
 *      com a comparação (% da Região Tracbel · % de SP) ao lado.
 *   3. EVIDÊNCIA, recolhida — o detalhe por cultura, as faixas, as usinas, o
 *      operacional de carteira e as notas de fonte e competência.
 *
 * NADA SAI. As quarenta medidas continuam, redistribuídas entre as camadas — e
 * há teste contando cada uma delas.
 *
 * QUEM ATENDE É O DA CARTEIRA DO CRM, e só ele (issue 107): planilha é requisito,
 * não fonte. E "sigilo do IBGE" nunca vira zero — é ausência de divulgação.
 */

import { Fragment } from 'react';
import type {
  CulturaNoEstado,
  IndicadoresDoMunicipio,
  ProcedenciasDoTerritorio,
  RegraDePotencialAplicada,
  TotaisDaRegiaoTracbel,
  TotaisDoEstado,
} from '../../tipos/territorio';
import { ComparacaoSomavel } from '../comum/Comparacao';
import { montarSomavel } from '../comum/comparacoes';
import { MetricaAusente } from '../comum/MetricaAusente';
import { ValorAusente } from '../comum/ValorAusente';
import { reaisCompactos, reaisDaProducao } from './escalas';

const nº = (v: number) => v.toLocaleString('pt-BR');

/** A produtividade na unidade que o servidor mandou — sem unidade, não se mostra número. */
function produtividade(valor: number | null, unidade: string | null): string {
  if (valor === null || unidade === null) return 'não disponível';
  return `${valor.toLocaleString('pt-BR', { maximumFractionDigits: 2 })} ${unidade}`;
}

/** O sigilo do IBGE dito como sigilo, e não como traço mudo nem como zero. */
function SobSigilo({ valor, oQue, formatar = nº }: { valor: number | null; oQue: string; formatar?: (v: number) => string }) {
  if (valor !== null) return <>{formatar(valor)}</>;
  return (
    <ValorAusente
      motivo="O IBGE suprimiu este número por sigilo: ele oculta o valor quando poucos estabelecimentos o compõem. Sigilo NÃO é zero."
      oQue={oQue}
    />
  );
}

export function DetalheDoMunicipio({
  municipio,
  regras,
  culturasNoEstado,
  regiaoTracbel,
  estado,
  procedencias,
  aoFechar,
}: {
  municipio: IndicadoresDoMunicipio;
  regras: RegraDePotencialAplicada[];
  /** A mesma cultura no total de SP e no mesmo ano — a comparação da produtividade. */
  culturasNoEstado: CulturaNoEstado[];
  /** Os denominadores da Região Tracbel — a ADR inteira (issue 163). */
  regiaoTracbel: TotaisDaRegiaoTracbel | null;
  /** Os totais publicados de São Paulo — o outro denominador. */
  estado: TotaisDoEstado | null;
  /** De onde veio cada medida (issue 167). */
  procedencias: ProcedenciasDoTerritorio | null;
  aoFechar: () => void;
}) {
  const { cobertura, vendas, estrutura, producao } = municipio;
  const motor = municipio.potencialEstrutural;

  return (
    <div className="card cad-cartao terr-detalhe" aria-live="polite" data-bloco="ficha-do-municipio">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          <div className="card-title">{municipio.nome}</div>
          <div className="card-subtitle">
            IBGE <span className="cad-mono">{municipio.codigoIbge}</span> ·{' '}
            {municipio.pertenceAAdr
              ? `ADR · sub-região ${municipio.regiao}`
              : municipio.listadoNaAreaDeAtuacao
                ? 'na área de atuação, fora da ADR'
                : 'fora da área de atuação'}
            {municipio.lojaNome && ` · loja ${municipio.lojaNome.replace(/^.*—\s*/, '')}`}
            {municipio.lojaAtivaNoCrm === false && <strong className="cad-atencao"> · filial inativa no CRM</strong>}
          </div>
        </div>
        <button type="button" className="btn btn-secondary btn-sm" onClick={aoFechar}>
          Fechar
        </button>
      </div>

      <div className="terr-detalhe-grade">
        {/* ------------------------------------------------------------------
            CAMADA 1 — EXECUTIVA. Os quatro números de decisão, sempre abertos.
            Três deles não têm dado hoje e dizem qual issue os destrava.
            ------------------------------------------------------------------ */}
        <section className="terr-detalhe-executiva" data-camada="executiva">
          <h3 className="terr-detalhe-titulo">O que este município decide</h3>
          <dl className="terr-numeros">
            <dt>Demanda anual</dt>
            <dd>
              {motor?.demandaAnualDeMaquinas == null ? (
                <ValorAusente
                  motivo={
                    motor?.motivoSemDemanda === 'SemCicloDeRenovacao'
                      ? 'A regra não informou de quantos em quantos anos a máquina é trocada. A decisão D-P01 (issue 63) fixa cultura, categoria, hectares por máquina e anos de renovação — as quatro juntas.'
                      : 'Sem parque, não há o que renovar.'
                  }
                  oQue="a demanda anual"
                />
              ) : (
                <strong>{nº(motor.demandaAnualDeMaquinas)} máq/ano</strong>
              )}
            </dd>
          </dl>
          <MetricaAusente
            metrica="Mercado anual"
            motivo="Demanda anual × preço de referência, agregada por categoria de máquina. Precisa do preço de máquina por modelo (issue 70), que não existe no CRM."
          />
          <MetricaAusente
            metrica="Captura Tracbel"
            motivo="Vendas em unidades ÷ demanda anual estimada. Precisa da issue 69: o faturamento em reais não serve de numerador para uma demanda medida em máquinas. Não é market share."
          />
          <MetricaAusente
            metrica="Oportunidade"
            motivo="A demanda ajustada menos as vendas, nunca abaixo de zero (issue 162). Depende da issue 69 e, para sair em reais, da issue 70."
          />
        </section>

        {/* ------------------------------------------------------------------
            CAMADA 2 — INTERMEDIÁRIA, aberta, em DUAS colunas: a lavoura e a
            estrutura, cada medida com a comparação ao lado.
            ------------------------------------------------------------------ */}
        <section data-camada="lavoura">
          <h3 className="terr-detalhe-titulo">A lavoura{producao ? ` (${producao.ano})` : ''}</h3>
          {producao === null ? (
            <ValorAusente motivo="A Produção Agrícola Municipal não foi carregada para este município." oQue="a lavoura" />
          ) : (
            <dl className="terr-numeros">
              <dt>Área plantada</dt>
              <dd>
                <ComparacaoSomavel
                  medida={montarSomavel(
                    producao.areaPlantadaHectares,
                    regiaoTracbel?.areaPlantadaHectares ?? null,
                    estado?.areaPlantadaHectares ?? null,
                    { procedencia: procedencias?.areaPlantada ?? null },
                  )}
                  oQue="a área plantada"
                  formatar={(v) => `${nº(Math.round(v))} ha`}
                />
              </dd>
              <dt>Área colhida</dt>
              <dd>
                <ComparacaoSomavel
                  medida={montarSomavel(
                    producao.areaColhidaHectares,
                    regiaoTracbel?.areaColhidaHectares ?? null,
                    estado?.areaColhidaHectares ?? null,
                    { procedencia: procedencias?.areaPlantada ?? null },
                  )}
                  oQue="a área colhida"
                  formatar={(v) => `${nº(Math.round(v))} ha`}
                />
              </dd>
              <dt>Valor da produção</dt>
              <dd>
                <ComparacaoSomavel
                  medida={montarSomavel(
                    producao.valorDaProducaoMilReais,
                    regiaoTracbel?.valorDaProducaoMilReais ?? null,
                    estado?.valorDaProducaoMilReais ?? null,
                    { procedencia: procedencias?.valorDaProducao ?? null },
                  )}
                  oQue="o valor da produção"
                  formatar={reaisDaProducao}
                />
              </dd>
              <dt>Culturas com área divulgada</dt>
              <dd>{nº(producao.culturasComArea)}</dd>
            </dl>
          )}
        </section>

        <section data-camada="estrutura">
          <h3 className="terr-detalhe-titulo">O que já existe para mecanizar</h3>
          <dl className="terr-numeros">
            <dt>Tratores{estrutura.anoDoCenso ? ` (${estrutura.anoDoCenso})` : ''}</dt>
            <dd>
              <ComparacaoSomavel
                medida={montarSomavel(estrutura.tratores, regiaoTracbel?.tratores ?? null, estado?.tratores.publicado ?? null, {
                  motivoDaAusencia:
                    'O IBGE suprimiu o número de tratores deste município por sigilo. Sigilo NÃO é zero.',
                  procedencia: procedencias?.tratores ?? null,
                })}
                oQue="o parque de tratores"
                formatar={nº}
              />
            </dd>
            <dt>Menos de 100 cv</dt>
            <dd>
              <SobSigilo valor={estrutura.tratoresAbaixoDe100Cv} oQue="os tratores abaixo de 100 cv" />
            </dd>
            <dt>De 100 cv e mais</dt>
            <dd>
              <SobSigilo valor={estrutura.tratoresDe100CvEMais} oQue="os tratores de 100 cv e mais" />
            </dd>
            <dt>Propriedades</dt>
            <dd>
              <ComparacaoSomavel
                medida={montarSomavel(
                  estrutura.estabelecimentos,
                  regiaoTracbel?.estabelecimentos ?? null,
                  estado?.estabelecimentos.publicado ?? null,
                  { procedencia: procedencias?.estabelecimentos ?? null },
                )}
                oQue="as propriedades"
                formatar={nº}
              />
            </dd>
            <dt>Propriedades com trator</dt>
            <dd>
              <SobSigilo valor={estrutura.estabelecimentosComTrator} oQue="as propriedades com trator" />
              {estrutura.estabelecimentos !== null && estrutura.estabelecimentosComTrator !== null && (
                <span className="cad-sub"> de {nº(estrutura.estabelecimentos)}</span>
              )}
            </dd>
            <dt>Rebanho bovino{estrutura.anoDoRebanho ? ` (${estrutura.anoDoRebanho})` : ''}</dt>
            <dd>
              <ComparacaoSomavel
                medida={montarSomavel(estrutura.bovinos, regiaoTracbel?.bovinos ?? null, estado?.rebanho.publicado ?? null, {
                  procedencia: procedencias?.rebanho ?? null,
                })}
                oQue="o rebanho bovino"
                formatar={(v) => `${nº(v)} cabeças`}
              />
            </dd>
            <dt>Área do município</dt>
            <dd>
              {estrutura.areaKm2 === null ? (
                <ValorAusente motivo="A área territorial deste município não foi carregada." oQue="a área do município" />
              ) : (
                <>
                  {estrutura.areaKm2.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} km²
                  {estrutura.tratoresPorMilKm2 !== null && (
                    <span className="cad-sub">
                      {' '}
                      · {estrutura.tratoresPorMilKm2.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} tratores/mil km²
                    </span>
                  )}
                </>
              )}
            </dd>
          </dl>
        </section>

        {/* ------------------------------------------------------------------
            CAMADA 3 — EVIDÊNCIA, recolhida. Nada some: o detalhe por cultura,
            as faixas, as usinas, o operacional de carteira e as notas de fonte.
            ------------------------------------------------------------------ */}
        <div className="terr-detalhe-evidencia" data-camada="evidencia">
          <details className="cad-recolhivel" data-evidencia="potencial">
            <summary>Potencial por cultura</summary>
            {municipio.potencial.map((p) => {
              const regra = regras.find((r) => r.produtoCodigoIbge === p.produtoCodigoIbge);
              const noEstado = culturasNoEstado.find((c) => c.produtoCodigoIbge === p.produtoCodigoIbge) ?? null;
              return (
                <dl className="terr-numeros" key={p.produtoCodigoIbge}>
                  <dt>
                    Área plantada de {regra?.produtoNome ?? p.produtoCodigoIbge}
                    {p.ano !== null && ` (${p.ano})`}
                  </dt>
                  <dd>{p.areaPlantadaHectares === null ? 'não disponível' : `${nº(p.areaPlantadaHectares)} ha`}</dd>
                  <dt>Área colhida da mesma cultura</dt>
                  <dd>{p.areaColhidaHectares === null ? 'não disponível' : `${nº(p.areaColhidaHectares)} ha`}</dd>
                  <dt>Quantidade produzida</dt>
                  <dd>
                    {p.quantidadeProduzida === null || p.unidadeDaQuantidade === null
                      ? 'não disponível'
                      : `${nº(p.quantidadeProduzida)} ${p.unidadeDaQuantidade}`}
                  </dd>
                  <dt>Produtividade</dt>
                  <dd>
                    {produtividade(p.produtividade, p.unidadeDaProdutividade)}
                    {noEstado?.produtividade != null && (
                      <span className="cad-sub">
                        {' '}
                        · SP {produtividade(noEstado.produtividade, noEstado.unidadeDaProdutividade)}
                      </span>
                    )}
                  </dd>
                  <dt>Valor da produção dela</dt>
                  <dd>
                    {p.valorDaProducaoMilReais === null ? 'não disponível' : reaisDaProducao(p.valorDaProducaoMilReais)}
                  </dd>
                  <dt>
                    {regra ? `${regra.modeloDeReferencia} teóricos (1 a cada ${regra.hectaresPorMaquina} ha)` : 'Máquinas teóricas'}
                  </dt>
                  <dd>
                    <strong>{p.maquinasTeoricas === null ? '—' : nº(p.maquinasTeoricas)}</strong>
                  </dd>
                </dl>
              );
            })}
            {motor && (
              <dl className="terr-numeros">
                <dt>Parque teórico do município</dt>
                <dd>
                  <strong>{motor.parqueDeMaquinas === null ? '—' : nº(motor.parqueDeMaquinas)}</strong>
                  {motor.areaUtilHectares !== null && (
                    <span className="cad-sub">
                      {' '}
                      · {nº(Math.round(motor.areaUtilHectares))} ha úteis, já sem a terra contada duas vezes
                    </span>
                  )}
                </dd>
              </dl>
            )}
            <p className="cad-sub">
              {motor?.estimativa ? 'Estimativa: a regra de hectares por máquina ainda não foi confirmada pelo comercial. ' : ''}
              É a área do município inteiro (IBGE), clientes e não clientes juntos. Produtividade é a quantidade sobre a
              área colhida, no mesmo ano — como o IBGE calcula.
            </p>
          </details>

          {estrutura.faixasDeArea.length > 0 && (
            <details className="cad-recolhivel" data-evidencia="faixas">
              <summary>Propriedades por tamanho</summary>
              <dl className="terr-numeros">
                {estrutura.faixasDeArea.map((f) => (
                  <Fragment key={f.ordem}>
                    <dt>{f.rotulo}</dt>
                    <dd>
                      <SobSigilo valor={f.estabelecimentos} oQue={`as propriedades de ${f.rotulo}`} />
                    </dd>
                  </Fragment>
                ))}
              </dl>
              <p className="cad-sub">
                As faixas de potência e de área não somam o total: o "Total" do IBGE é uma categoria ao lado delas.
              </p>
            </details>
          )}

          <details className="cad-recolhivel" data-evidencia="usinas">
            <summary>Usinas de etanol</summary>
            {estrutura.usinas.length === 0 ? (
              <p className="cad-sub">
                Nenhuma usina de etanol autorizada pela ANP aqui. Isso <strong>não</strong> prova que não há usina: a ANP
                não enxerga quem produz só açúcar.
              </p>
            ) : (
              <dl className="terr-numeros">
                {estrutura.usinas.map((u) => (
                  <Fragment key={u.razaoSocial}>
                    <dt>{u.razaoSocial}</dt>
                    <dd>{u.capacidadeM3Dia === null ? 'capacidade não informada' : `${nº(u.capacidadeM3Dia)} m³/dia`}</dd>
                  </Fragment>
                ))}
              </dl>
            )}
          </details>

          <details className="cad-recolhivel" data-evidencia="carteira">
            <summary>Quem atende, cobertura e vendas</summary>

            <h4 className="terr-detalhe-subtitulo">Quem atende este município</h4>
            {municipio.responsaveisPelasCarteiras.length === 0 ? (
              <p className="cad-nada">Nenhum vínculo em carteira comercial com cliente deste município, ao seu alcance.</p>
            ) : (
              <table className="cad-tabela terr-tabela-compacta">
                <caption className="cad-so-leitor">Responsáveis das carteiras com vínculo neste município</caption>
                <thead>
                  <tr>
                    <th scope="col">Responsável no CRM</th>
                    <th scope="col">Natureza</th>
                    <th scope="col">Vínculos aqui</th>
                    <th scope="col">Carteiras</th>
                  </tr>
                </thead>
                <tbody>
                  {municipio.responsaveisPelasCarteiras.map((r) => (
                    <tr key={`${r.nome}-${r.natureza}`}>
                      <td>{r.nome}</td>
                      <td>{r.natureza === 'Departamento' ? 'área' : r.natureza === 'Pessoa' ? 'pessoa' : r.natureza}</td>
                      <td className="cad-mono">{nº(r.vinculos)}</td>
                      <td className="cad-mono">{nº(r.carteiras)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
            <p className="cad-sub">Da carteira no CRM: quem tem hoje os clientes com endereço aqui.</p>

            <h4 className="terr-detalhe-subtitulo">Cobertura de visita</h4>
            {cobertura.vinculosComCadencia === 0 ? (
              <p className="cad-nada">Sem vínculo em carteira comercial com cadência declarada — sem base para medir.</p>
            ) : (
              <dl className="terr-numeros">
                <dt>Elegíveis</dt>
                <dd>{nº(cobertura.vinculosComCadencia)}</dd>
                <dt>Cobertos</dt>
                <dd>{nº(cobertura.cobertos)}</dd>
                <dt>Fora da cadência</dt>
                <dd>{nº(cobertura.foraDaCadencia)}</dd>
                <dt>Nunca contatados</dt>
                <dd>{nº(cobertura.nuncaContatados)}</dd>
                <dt>Pendentes</dt>
                <dd>
                  <strong>
                    {nº(cobertura.pendentes)} ({cobertura.percentualPendente?.toLocaleString('pt-BR')}%)
                  </strong>
                </dd>
              </dl>
            )}
            <p className="cad-sub">
              {nº(cobertura.clientes)} clientes com endereço aqui · {nº(cobertura.semCadencia)} vínculos em linha sem
              cadência, fora da conta. Contato é qualquer interação registrada: nenhum tipo de atividade está marcado
              como visita (documento 32, P-2).
            </p>

            <h4 className="terr-detalhe-subtitulo">Vendas no período</h4>
            <dl className="terr-numeros">
              <dt>Máquina</dt>
              <dd>{reaisCompactos(vendas.maquina)}</dd>
              <dt>Peça</dt>
              <dd>{reaisCompactos(vendas.peca)}</dd>
              <dt>Serviço</dt>
              <dd>{reaisCompactos(vendas.servico)}</dd>
              <dt>Outros</dt>
              <dd>{reaisCompactos(vendas.outros)}</dd>
              <dt>Total líquido</dt>
              <dd>
                <strong>{reaisCompactos(vendas.valorLiquido)}</strong>
              </dd>
              <dt>Pós-venda</dt>
              <dd>
                <strong>{reaisCompactos(vendas.posVenda)}</strong>
                <span className="cad-sub"> peça + serviço, provisório</span>
              </dd>
            </dl>
            <p className="cad-sub">{nº(vendas.clientesQueCompraram)} clientes compraram no período.</p>
          </details>

          <details className="cad-recolhivel" data-evidencia="fontes">
            <summary>Fontes e competências</summary>
            <p className="cad-sub">
              O Censo Agropecuário é de {estrutura.anoDoCenso ?? '2017'} e o próximo sai em 2028 — o parque tem essa
              idade. A Pesquisa da Pecuária Municipal é anual e anda sozinha
              {estrutura.anoDoRebanho ? `, hoje em ${estrutura.anoDoRebanho}` : ''}. "Sigilo do IBGE" não é zero: ele
              oculta o número quando poucos estabelecimentos o compõem.
            </p>
            <p className="cad-sub">
              O valor da produção é o que o município <strong>colhe</strong>, publicado pelo IBGE em mil reais — não é
              venda da Tracbel nem preço de máquina. A quantidade produzida não é somada entre culturas: o IBGE usa
              tonelada, e mil frutos no abacaxi e no coco. O café entra uma vez só, pelo "Total".
            </p>
            <p className="cad-sub">
              As fatias comparam com a <strong>Região Tracbel</strong> — a área de atuação inteira, que não muda com o
              filtro de sub-região — e com o total <strong>publicado</strong> de São Paulo, que não é a soma dos
              municípios: o valor municipal sigiloso entra nele sem aparecer embaixo.
            </p>
          </details>
        </div>
      </div>
    </div>
  );
}
