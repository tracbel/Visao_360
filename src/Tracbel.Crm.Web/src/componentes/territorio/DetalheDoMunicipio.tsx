/**
 * O detalhe de um município: território, quem o atende, os indicadores com a
 * composição de cada um, a lavoura inteira e o que já existe para mecanizar.
 *
 * QUEM ATENDE É O DA CARTEIRA DO CRM, e só ele (issue 107): planilha é requisito,
 * não fonte — a tela não mostra o que uma planilha afirma nem compara planilhas.
 *
 * CADA MEDIDA DIZ A FONTE E O ANO, porque os anos diferem muito: o Censo
 * Agropecuário é de 2017, o rebanho é anual e a PAM tem três anos carregados.
 * E "sigilo do IBGE" nunca vira zero — é ausência de divulgação, não medida.
 */

import { Fragment } from 'react';
import type { CulturaNoEstado, IndicadoresDoMunicipio, RegraDePotencialAplicada } from '../../tipos/territorio';
import { reaisCompactos, reaisDaProducao } from './escalas';

const nº = (v: number) => v.toLocaleString('pt-BR');

/** A produtividade na unidade que o servidor mandou — sem unidade, não se mostra número. */
function produtividade(valor: number | null, unidade: string | null): string {
  if (valor === null || unidade === null) return 'não disponível';
  return `${valor.toLocaleString('pt-BR', { maximumFractionDigits: 2 })} ${unidade}`;
}

export function DetalheDoMunicipio({
  municipio,
  regras,
  culturasNoEstado,
  aoFechar,
}: {
  municipio: IndicadoresDoMunicipio;
  regras: RegraDePotencialAplicada[];
  /** A mesma cultura no total de SP e no mesmo ano — a comparação da produtividade. */
  culturasNoEstado: CulturaNoEstado[];
  aoFechar: () => void;
}) {
  const { cobertura, vendas, estrutura } = municipio;

  return (
    <div className="card cad-cartao terr-detalhe" aria-live="polite">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          <div className="card-title">{municipio.nome}</div>
          <div className="card-subtitle">
            IBGE <span className="cad-mono">{municipio.codigoIbge}</span> ·{' '}
            {municipio.pertenceAAdr
              ? `ADR · região ${municipio.regiao}`
              : municipio.listadoNaAreaDeAtuacao
                ? 'na área de atuação, fora da ADR'
                : 'fora da área de atuação'}
            {municipio.lojaNome && ` · loja ${municipio.lojaNome.replace(/^.*—\s*/, '')}`}
            {municipio.lojaAtivaNoCrm === false && (
              <strong className="cad-atencao"> · filial inativa no CRM</strong>
            )}
          </div>
        </div>
        <button type="button" className="btn btn-secondary btn-sm" onClick={aoFechar}>
          Fechar
        </button>
      </div>

      <div className="terr-detalhe-grade">
        <section className="terr-detalhe-responsaveis">
          <h3 className="terr-detalhe-titulo">Quem atende este município</h3>
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
          <p className="cad-sub">
            Da carteira no CRM: quem tem hoje os clientes com endereço aqui.
          </p>
        </section>

        <section>
          <h3 className="terr-detalhe-titulo">Cobertura de visita <span className="terr-selo terr-selo-RegraComercialProvisoria">Regra provisória</span></h3>
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
            cadência, fora da conta.
          </p>
        </section>

        <section>
          <h3 className="terr-detalhe-titulo">Vendas no período</h3>
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
            <dt>Pós-venda (peça + serviço, composição provisória)</dt>
            <dd>
              <strong>{reaisCompactos(vendas.posVenda)}</strong>
            </dd>
          </dl>
          <p className="cad-sub">{nº(vendas.clientesQueCompraram)} clientes compraram no período.</p>
        </section>

        <section>
          <h3 className="terr-detalhe-titulo">Potencial teórico por área</h3>
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
                    <span className="cad-sub"> · SP {produtividade(noEstado.produtividade, noEstado.unidadeDaProdutividade)}</span>
                  )}
                </dd>
                <dt>Valor da produção dela</dt>
                <dd>{p.valorDaProducaoMilReais === null ? 'não disponível' : reaisDaProducao(p.valorDaProducaoMilReais)}</dd>
                <dt>{regra ? `${regra.modeloDeReferencia} teóricos (1 a cada ${regra.hectaresPorMaquina} ha)` : 'Máquinas teóricas'}</dt>
                <dd>
                  <strong>{p.maquinasTeoricas === null ? '—' : nº(p.maquinasTeoricas)}</strong>
                </dd>
              </dl>
            );
          })}
          <p className="cad-sub">
            Estimativa: área do município inteiro (IBGE), clientes e não clientes juntos, por uma regra a confirmar.
            Produtividade é a quantidade sobre a área colhida, no mesmo ano — como o IBGE calcula.
          </p>
        </section>

        {municipio.producao && (
          <section>
            <h3 className="terr-detalhe-titulo">A lavoura inteira ({municipio.producao.ano})</h3>
            <dl className="terr-numeros">
              <dt>Área plantada, todas as culturas</dt>
              <dd>
                {municipio.producao.areaPlantadaHectares === null
                  ? 'não disponível'
                  : `${nº(Math.round(municipio.producao.areaPlantadaHectares))} ha`}
              </dd>
              <dt>Área colhida</dt>
              <dd>
                {municipio.producao.areaColhidaHectares === null
                  ? 'não disponível'
                  : `${nº(Math.round(municipio.producao.areaColhidaHectares))} ha`}
              </dd>
              <dt>Valor da produção</dt>
              <dd>
                <strong>
                  {municipio.producao.valorDaProducaoMilReais === null
                    ? 'não disponível'
                    : reaisDaProducao(municipio.producao.valorDaProducaoMilReais)}
                </strong>
              </dd>
              <dt>Culturas com área divulgada</dt>
              <dd>{nº(municipio.producao.culturasComArea)}</dd>
            </dl>
            <p className="cad-sub">
              Produção Agrícola Municipal (IBGE). O valor é o que o município <strong>colhe</strong>, não o que a Tracbel
              vende. A quantidade produzida não é somada entre culturas: o IBGE usa tonelada, e mil frutos no abacaxi e no coco
              conforme o produto.
            </p>
          </section>
        )}

        <section>
          <h3 className="terr-detalhe-titulo">O que já existe para mecanizar</h3>
          <dl className="terr-numeros">
            <dt>Tratores{estrutura.anoDoCenso ? ` (Censo ${estrutura.anoDoCenso})` : ''}</dt>
            <dd>
              <strong>{estrutura.tratores === null ? 'sigilo do IBGE' : nº(estrutura.tratores)}</strong>
            </dd>
            <dt>Menos de 100 cv</dt>
            <dd>{estrutura.tratoresAbaixoDe100Cv === null ? 'sigilo do IBGE' : nº(estrutura.tratoresAbaixoDe100Cv)}</dd>
            <dt>De 100 cv e mais</dt>
            <dd>{estrutura.tratoresDe100CvEMais === null ? 'sigilo do IBGE' : nº(estrutura.tratoresDe100CvEMais)}</dd>
            <dt>Propriedades com trator</dt>
            <dd>
              {estrutura.estabelecimentosComTrator === null ? 'sigilo do IBGE' : nº(estrutura.estabelecimentosComTrator)}
              {estrutura.estabelecimentos !== null && estrutura.estabelecimentosComTrator !== null
                ? ` de ${nº(estrutura.estabelecimentos)}`
                : ''}
            </dd>
            <dt>Área do município</dt>
            <dd>
              {estrutura.areaKm2 === null
                ? 'não disponível'
                : `${estrutura.areaKm2.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} km²`}
              {estrutura.tratoresPorMilKm2 !== null
                ? ` · ${estrutura.tratoresPorMilKm2.toLocaleString('pt-BR', { maximumFractionDigits: 0 })} tratores/mil km²`
                : ''}
            </dd>
            <dt>Rebanho bovino{estrutura.anoDoRebanho ? ` (${estrutura.anoDoRebanho})` : ''}</dt>
            <dd>{estrutura.bovinos === null ? 'não disponível' : `${nº(estrutura.bovinos)} cabeças`}</dd>
          </dl>

          {estrutura.faixasDeArea.length > 0 && (
            <>
              <h4 className="terr-detalhe-subtitulo">Propriedades por tamanho</h4>
              <dl className="terr-numeros">
                {estrutura.faixasDeArea.map((f) => (
                  <Fragment key={f.ordem}>
                    <dt>{f.rotulo}</dt>
                    <dd>{f.estabelecimentos === null ? 'sigilo do IBGE' : nº(f.estabelecimentos)}</dd>
                  </Fragment>
                ))}
              </dl>
            </>
          )}

          <h4 className="terr-detalhe-subtitulo">Usinas de etanol</h4>
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

          <p className="cad-sub">
            O Censo Agropecuário é de {estrutura.anoDoCenso ?? '2017'} e o próximo sai em 2028 — o parque tem essa idade.
            "Sigilo do IBGE" não é zero: ele oculta o número quando poucos estabelecimentos o compõem. As duas faixas de
            potência não somam o total, porque o "Total" do IBGE é uma categoria ao lado delas.
          </p>
        </section>
      </div>
    </div>
  );
}
