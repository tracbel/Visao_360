/**
 * A FICHA DO MUNICÍPIO, EM ABAS (fidelidade às maquetes, 23/09/2026 — fase 4;
 * antes, três camadas do documento 50, §6, fase T4).
 *
 * A MAQUETE `territorio-ficha.png` põe a ficha em cinco abas sublinhadas —
 * Visão geral · Lavoura · Estrutura · Oportunidades · Histórico — e abre na
 * Visão geral: resumo executivo em quatro cartõezinhos, lavoura e estrutura lado
 * a lado, e o cartão verde das oportunidades.
 *
 * NADA SAI (decisão 3 do usuário). As quarenta medidas das três camadas de antes
 * mudaram de casa, e há teste comparando a lista de antes com a de depois:
 *   - a camada INTERMEDIÁRIA e a de EVIDÊNCIA foram para as abas Lavoura e
 *     Estrutura, cada medida com a comparação (% da Região Tracbel · % de SP) e
 *     a procedência, e as evidências continuam em `<details>` recolhidos;
 *   - a camada EXECUTIVA — Demanda anual, Mercado anual, Captura e Oportunidade
 *     — foi para o topo da aba Oportunidades, que é a pergunta que ela responde;
 *   - o código IBGE e a marca "ADR" do subtítulo foram para a dica ao lado dele.
 *
 * QUEM ATENDE É O DA CARTEIRA DO CRM, e só ele (issue 107): planilha é requisito,
 * não fonte. E "sigilo do IBGE" nunca vira zero — é ausência de divulgação.
 */

import { Check, MapPin, X } from 'lucide-react';
import { Fragment, useState } from 'react';
import type {
  CulturaNoEstado,
  IndicadoresDoMunicipio,
  NumerosDeDecisao,
  ProcedenciasDoTerritorio,
  RegraDePotencialAplicada,
  TotaisDaRegiaoTracbel,
  TotaisDoEstado,
} from '../../tipos/territorio';
import { InfoTooltip } from '../InfoTooltip';
import { ComparacaoSomavel } from '../comum/Comparacao';
import { montarSomavel } from '../comum/comparacoes';
import { ValorAusente } from '../comum/ValorAusente';
import { AbasDaFicha, PainelDaFicha, type AbaDaFicha } from './carteira/AbasDaFicha';
import { HistoricoDoMunicipio } from './carteira/HistoricoDoMunicipio';
import { OportunidadesDoMunicipio } from './carteira/OportunidadesDoMunicipio';
import { usePeriodoDaLeitura } from './carteira/periodo';
import { VisaoGeralDoMunicipio } from './carteira/VisaoGeralDoMunicipio';
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
  numerosDeDecisao,
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
  /**
   * Os números de decisão do RECORTE, com o motivo de cada ausência — da API
   * (issue 69, parte A). A ficha usa só a FRASE: o que falta para o recorte falta
   * para o município, e o servidor diz uma vez só, com a mesma redação do topo.
   */
  numerosDeDecisao: NumerosDeDecisao | null;
  aoFechar: () => void;
}) {
  const { cobertura, vendas, estrutura, producao } = municipio;
  const motor = municipio.potencialEstrutural;
  const periodo = usePeriodoDaLeitura();

  // A ABA NÃO VOLTA PARA A VISÃO GERAL QUANDO O MUNICÍPIO MUDA: quem está na
  // aba Lavoura e clica noutra linha da tabela está comparando lavouras — e é a
  // lavoura do outro município que quer ver.
  const [aba, setAba] = useState<AbaDaFicha>('visao-geral');
  // AS ABAS JÁ ABERTAS — o conteúdo de cada uma monta na primeira visita e fica.
  const [abertas, setAbertas] = useState<ReadonlySet<AbaDaFicha>>(() => new Set(['visao-geral']));
  const trocarDeAba = (proxima: AbaDaFicha) => {
    setAba(proxima);
    setAbertas((atuais) => (atuais.has(proxima) ? atuais : new Set([...atuais, proxima])));
  };
  const painel = (id: AbaDaFicha) => ({ id, ativa: aba, montado: abertas.has(id) });

  const loja = municipio.lojaNome?.replace(/^.*—\s*/, '') ?? null;
  const onde = municipio.pertenceAAdr
    ? municipio.regiao
    : municipio.listadoNaAreaDeAtuacao
      ? 'na área de atuação, fora da ADR'
      : 'fora da área de atuação';

  const irParaOportunidades = () => {
    trocarDeAba('oportunidades');
    document.getElementById('ficha-aba-oportunidades')?.focus();
  };

  return (
    <div className="card terr-detalhe" data-bloco="ficha-do-municipio">
      {/* O CABEÇALHO DA MAQUETE: pin verde, nome grande, "✓ Selecionado" e o ✕.
          A REGIÃO VIVA É SÓ ELE — trocar de município anuncia o nome novo, e
          trocar de aba não relê a ficha inteira para quem usa leitor de tela. */}
      <div className="terr-ficha-cabecalho">
        <div className="terr-ficha-identidade" aria-live="polite">
          <div className="terr-ficha-titulo">
            <span className="terr-ficha-pin" aria-hidden="true">
              <MapPin size={22} strokeWidth={2} />
            </span>
            <h2 className="terr-ficha-nome">{municipio.nome}</h2>
            <span className="terr-selo-escolhido">
              <Check size={12} strokeWidth={3} aria-hidden="true" />
              Selecionado
            </span>
          </div>
          <p className="terr-ficha-local">
            <MapPin size={12} strokeWidth={2} aria-hidden="true" />
            <span>{onde}</span>
            {loja && (
              <>
                <span className="terr-ficha-ponto" aria-hidden="true">
                  •
                </span>
                <span>Loja {loja}</span>
              </>
            )}
            {/* O CÓDIGO IBGE E A MARCA "ADR" MORAM NA DICA (maquete): são a
                identidade do município para quem confere, não para quem lê. */}
            <InfoTooltip
              rotulo={`Código IBGE e área de atuação de ${municipio.nome}`}
              texto={`IBGE ${municipio.codigoIbge} · ${
                municipio.pertenceAAdr
                  ? `ADR · sub-região ${municipio.regiao}`
                  : municipio.listadoNaAreaDeAtuacao
                    ? 'na área de atuação, fora da ADR'
                    : 'fora da área de atuação'
              }${loja ? ` · loja ${loja}${municipio.lojaCodigo ? ` (${municipio.lojaCodigo})` : ''}` : ''}. Sub-região e loja são as da área de atuação cadastrada no CRM.`}
            />
            {municipio.lojaAtivaNoCrm === false && <strong className="cad-atencao">filial inativa no CRM</strong>}
          </p>
        </div>
        {/* O ✕ DA MAQUETE. O nome acessível continua dizendo o que o clique faz
            — "Fechar a ficha de Batatais" —, e não o desenho do botão. */}
        <button
          type="button"
          className="terr-fechar-ficha"
          onClick={aoFechar}
          aria-label={`Fechar a ficha de ${municipio.nome}`}
        >
          <X size={16} strokeWidth={2.4} aria-hidden="true" />
        </button>
      </div>

      <AbasDaFicha ativa={aba} aoTrocar={trocarDeAba} nomeDoMunicipio={municipio.nome} />

      {/* ------------------------------------------------------------------
          VISÃO GERAL — a aba da maquete.
          ------------------------------------------------------------------ */}
      <PainelDaFicha {...painel('visao-geral')}>
        <VisaoGeralDoMunicipio
          municipio={municipio}
          regras={regras}
          culturasNoEstado={culturasNoEstado}
          procedencias={procedencias}
          periodo={periodo}
          numerosDeDecisao={numerosDeDecisao}
          aoVerOportunidades={irParaOportunidades}
        />
      </PainelDaFicha>

      {/* ------------------------------------------------------------------
          LAVOURA — a antiga camada intermediária da lavoura, o potencial por
          cultura e as notas de fonte, recolhidos como antes.
          ------------------------------------------------------------------ */}
      <PainelDaFicha {...painel('lavoura')}>
        <div className="terr-detalhe-grade">
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

          <div className="terr-detalhe-evidencia" data-camada="evidencia-da-lavoura">
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
      </PainelDaFicha>

      {/* ------------------------------------------------------------------
          ESTRUTURA — o que já existe para mecanizar, e as evidências dela:
          faixas de tamanho, usinas e a carteira (quem atende, cobertura e
          vendas), recolhidas como antes.
          ------------------------------------------------------------------ */}
      <PainelDaFicha {...painel('estrutura')}>
        <div className="terr-detalhe-grade">
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
              {/* A MEDIDA EM UNIDADES, do ART (issue 69, D-P08). Ela fica na MESMA lista de propósito —
                  é a mesma pergunta, "quanto esta praça comprou" —, e com a unidade escrita porque é o
                  único número daqui que não é em reais. Nulo é o ART não ter trazido venda; zero é medida. */}
              <dt>Máquinas vendidas</dt>
              <dd>
                {municipio.maquinasVendidas === null ? (
                  <span className="cad-sub">— o ART não trouxe venda de máquina nesta consulta</span>
                ) : (
                  <>
                    <strong>{nº(municipio.maquinasVendidas)}</strong>
                    <span className="cad-sub"> unidades, pelo ART — não se somam aos reais acima</span>
                  </>
                )}
              </dd>
            </dl>
          </section>

          <div className="terr-detalhe-evidencia" data-camada="evidencia-da-estrutura">
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
                  Nenhuma usina de etanol autorizada pela ANP aqui. Isso <strong>não</strong> prova que não há usina: a
                  ANP não enxerga quem produz só açúcar.
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
              {/* A citação "(documento 32, P-2)" saiu do fim da frase: o número do documento não diz nada a
                  quem lê a ficha, e a referência continua aqui, no código. */}
              <p className="cad-sub">
                {nº(cobertura.clientes)} clientes com endereço aqui · {nº(cobertura.semCadencia)} vínculos em linha sem
                cadência, fora da conta. Contato é qualquer interação registrada: nenhum tipo de atividade está marcado
                como visita.
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
          </div>
        </div>
      </PainelDaFicha>

      {/* ------------------------------------------------------------------
          OPORTUNIDADES — os quatro números de decisão (a antiga camada
          executiva) e a lista com confiança e origem (issue 162).
          ------------------------------------------------------------------ */}
      <PainelDaFicha {...painel('oportunidades')}>
        <OportunidadesDoMunicipio municipio={municipio} numerosDeDecisao={numerosDeDecisao} />
      </PainelDaFicha>

      <PainelDaFicha {...painel('historico')}>
        <HistoricoDoMunicipio nome={municipio.nome} />
      </PainelDaFicha>
    </div>
  );
}
