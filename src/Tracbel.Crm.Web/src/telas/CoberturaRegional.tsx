/**
 * Cobertura por Filial e Carteira — o agrupamento territorial que existe no dado.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — esta tela mostrava sete regionais que não existem.
 *
 * O protótipo agrupava a cobertura em **MT Norte, GO, BA Oeste** e mais quatro.
 * A premissa estava errada, e quem a corrigiu foi o gerente comercial:
 *
 * > *"Se você olhar no CRM da Vórtice temos as carteiras dos CENs, e nas
 * > carteiras temos a filial que ele vai atuar e quais cidades ele terá na
 * > carteira."*
 *
 * Verificado ao vivo, e o dado é inequívoco: **`IVS_Regional` existe no esquema
 * do Vórtice e tem ZERO linhas.** Não é que esteja pouco usada — nunca recebeu
 * uma linha. Não há tabela, coluna nem valor de texto que sustente aquelas sete
 * regionais, e **as treze filiais em operação estão todas no interior de São
 * Paulo** (documento 26, §1).
 *
 * O que existe preenchido é o par **filial → carteira → municípios**:
 * `IVS_Carteira.NroEmpresa` está preenchida em 655 de 655 carteiras, e
 * `IVS_CartCid` liga carteira e cidade. A tela passa a mostrar isso, em dois
 * níveis, com as rotas `/api/v1/cobertura/filiais` e `/carteiras`.
 *
 * ---------------------------------------------------------------------------
 * A LACUNA É DECLARADA, NÃO ESCONDIDA. Nem toda carteira carregada declara
 * cidade. **A carteira sem cidade aparece com a lista vazia em vez de
 * sumir**: escondê-la faria a tela mostrar uma operação menor do que ela é.
 * E o número vem da API, em `metricasSemDado`.
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS (#293, bloco 2). O cabeçalho com a hora da leitura, a barra de
 * filtros, os cinco números como `CartaoDeDecisao` e duas seções — a cobertura de contato (o medidor e as barras) e o
 * território (os dois níveis) — em `PainelDoMomento`. A nota de que as faixas do medidor não são meta foi para a dica
 * do painel. NENHUM NÚMERO, REGRA OU TEXTO DE REGRA MUDOU.
 */

import { Building2, BriefcaseBusiness, Map as IconeMapa, MapPin, MapPinned, RefreshCw } from 'lucide-react';
import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { ListaDeBarras } from '../componentes/ListaDeBarras';
import { GraficoGauge } from '../componentes/GraficoGauge';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import {
  listarTerritorioPorCarteira,
  obterCoberturaPorFilial,
  obterResumoDeCobertura,
} from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type { TerritorioDeCarteira } from '../tipos/relacionamento';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';

const nº = (v: number) => v.toLocaleString('pt-BR');

/**
 * As faixas do medidor, iguais às do protótipo (`app.js`, `drawGauge`).
 *
 * Elas não são meta: são faixas de leitura, e o rótulo abaixo do medidor diz
 * isso. A meta da empresa não existe em lugar nenhum — `organizacao.Meta` está
 * vazia —, e por isso o medidor não tem ponteiro de alvo nem cor de aprovação.
 */
const FAIXAS_DO_MEDIDOR = [
  { max: 40, cor: '#DC2626' },
  { max: 70, cor: '#F59E0B' },
  { max: 80, cor: '#FFDE00' },
  { max: 100, cor: '#16A34A' },
];

/** Quantas barras cabem sem virar sopa de letrinhas. O original tinha sete. */
const BARRAS_NO_GRAFICO = 8;

export function CoberturaRegional() {
  const { contexto } = useContextoDeAcesso();

  const filiais = useRecurso(
    (sinal) => obterCoberturaPorFilial(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const carteiras = useRecurso(
    // Sem `empresaCodigo`: a rota já devolve só o que o contexto de acesso
    // alcança, porque a consulta parte de `organizacao.Carteira`, que carrega o
    // filtro global. Um parâmetro de empresa aqui seria uma segunda fronteira,
    // que pode divergir da primeira.
    (sinal) => listarTerritorioPorCarteira(contexto, '', sinal),
    [contexto.empresa, contexto.usuario],
  );

  const cobertura = useRecurso(
    (sinal) => obterResumoDeCobertura(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const [somenteComCidade, setSomenteComCidade] = useState(false);
  const [abertas, setAbertas] = useState<Set<string>>(() => new Set());

  const listaDeFiliais = useMemo(() => filiais.dados?.itens ?? [], [filiais.dados]);
  const listaDeCarteiras = useMemo(() => carteiras.dados?.itens ?? [], [carteiras.dados]);

  const totais = useMemo(() => {
    const comCidade = listaDeCarteiras.filter((c) => c.municipios.length > 0);
    const municipios = new Set<number>();
    for (const c of listaDeCarteiras) for (const m of c.municipios) municipios.add(m.id);
    const ufs = new Set<string>();
    for (const c of listaDeCarteiras) for (const m of c.municipios) ufs.add(m.uf);
    return {
      carteiras: listaDeCarteiras.length,
      comCidade: comCidade.length,
      municipios: municipios.size,
      ufs: [...ufs].sort(),
    };
  }, [listaDeCarteiras]);

  const visiveis = useMemo(
    () =>
      [...listaDeCarteiras]
        .filter((c) => !somenteComCidade || c.municipios.length > 0)
        .sort((a, b) => b.municipios.length - a.municipios.length || a.carteiraNome.localeCompare(b.carteiraNome)),
    [listaDeCarteiras, somenteComCidade],
  );

  /** Quantas carteiras não declaram nenhuma cidade — dito uma vez, e não por linha. */
  const semCidade = useMemo(
    () => listaDeCarteiras.filter((c) => c.municipios.length === 0).length,
    [listaDeCarteiras],
  );


  /**
   * A cobertura de contato, que é o que os dois gráficos do protótipo mediam.
   *
   * O PERCENTUAL É DE CONTATO EM 90 DIAS sobre os vínculos da carteira, e o
   * denominador está escrito na tela. O protótipo media "clientes A ou B
   * tocados" — a classe do VÍNCULO não se sustenta (`ClienteCarteira.Classe`
   * segue entrando como C por assunção na maioria dos casos), então o recorte
   * por classe saiu aqui e o universo passou a ser a carteira inteira. A
   * classe do CLIENTE (curva ABC apurada do faturamento) já sustenta
   * segmentação — ver o filtro `classe` da Cobertura de Carteira.
   */
  const carteirasComCobertura = useMemo(() => cobertura.dados?.itens ?? [], [cobertura.dados]);

  const coberturaGeral = useMemo(() => {
    const clientes = carteirasComCobertura.reduce((s, c) => s + c.clientes, 0);
    const em90 = carteirasComCobertura.reduce((s, c) => s + c.comContatoEm90Dias, 0);
    return { clientes, em90, pct: clientes > 0 ? (em90 / clientes) * 100 : null };
  }, [carteirasComCobertura]);

  /** As maiores carteiras, que são as que movem o número da filial. */
  const barras = useMemo(
    () =>
      [...carteirasComCobertura]
        .filter((c) => c.clientes > 0)
        .sort((a, b) => b.clientes - a.clientes)
        .slice(0, BARRAS_NO_GRAFICO)
        .map((c) => ({
          // O NOME VAI INTEIRO. Ele era cortado no caractere 11 porque virava
          // rótulo de eixo em canvas e não cabia; em HTML quem corta é o CSS,
          // com reticências, e o inteiro fica no `title`.
          chave: c.carteiraChave,
          titulo: c.carteiraNome,
          // Compacto de propósito: escrito por extenso — "4.016 vínculos · 2.911
          // com contato" — o detalhe quebrava em duas linhas e dobrava a altura
          // de cada barra. O cabeçalho do cartão já diz o que a fração mede.
          detalhe: `${c.comContatoEm90Dias.toLocaleString('pt-BR')} de ${c.clientes.toLocaleString('pt-BR')}`,
          valor: (c.comContatoEm90Dias / c.clientes) * 100,
        })),
    [carteirasComCobertura],
  );

  function alternar(chave: string) {
    setAbertas((atual) => {
      const proximo = new Set(atual);
      if (proximo.has(chave)) proximo.delete(chave);
      else proximo.add(chave);
      return proximo;
    });
  }

  // SEM CARTEIRA AO ALCANCE, O NÚMERO É O TRAÇO COM O MOTIVO; LENDO, O CARTÃO PULSA E NÃO AFIRMA MOTIVO NENHUM.
  const lendo = filiais.carregando || carteiras.carregando;
  const semCarteira = lendo ? undefined : 'Sem filial com carteira ao alcance deste contexto.';

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Cobertura por Filial e Carteira</h1>
          <p className="page-subtitle">
            O agrupamento territorial que existe no dado: a filial da carteira e as cidades que ela atende.
          </p>
        </div>
        <p className="dash-atualizado">
          {filiais.procedencia ? <DadosAtualizadosEm procedencia={filiais.procedencia} /> : 'Lendo a cobertura…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              filiais.recarregar();
              carteiras.recarregar();
              cobertura.recarregar();
            }}
            disabled={lendo || cobertura.carregando}
            data-carregando={lendo || cobertura.carregando ? 'true' : 'false'}
            aria-label="Reler a cobertura por filial"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES: o único filtro da tela vale para a lista de carteiras do nível 2. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <div className="dash-filtros-acao">
            <label className="dash-caixa">
              <input type="checkbox" checked={somenteComCidade} onChange={(e) => setSomenteComCidade(e.target.checked)} />
              Só as carteiras com cidade cadastrada
            </label>
          </div>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={filiais.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="Filiais ao seu alcance"
          icone={Building2}
          tom="neutro"
          valor={filiais.dados ? nº(listaDeFiliais.length) : null}
          carregando={filiais.carregando}
          unidade="filiais"
          motivoSemDado={filiais.carregando ? undefined : 'Sem filial com carteira ao alcance deste contexto.'}
          variacao={filiais.dados ? 'a filial do cabeçalho' : null}
          sobre="O contexto de acesso enxerga uma filial por vez: a do cabeçalho. A fronteira de multiempresa vale aqui como em toda consulta."
        />
        <CartaoDeDecisao
          rotulo="Carteiras"
          icone={BriefcaseBusiness}
          tom="mercado"
          valor={carteiras.dados ? nº(totais.carteiras) : null}
          carregando={carteiras.carregando}
          unidade="carteiras"
          motivoSemDado={semCarteira}
          variacao={carteiras.dados ? 'com e sem cidade' : null}
          sobre="As carteiras destas filiais, com e sem cidade declarada."
        />
        <CartaoDeDecisao
          rotulo="Com cidade"
          icone={MapPinned}
          tom="oportunidade"
          valor={carteiras.dados ? nº(totais.comCidade) : null}
          carregando={carteiras.carregando}
          unidade="carteiras"
          motivoSemDado={semCarteira}
          variacao={carteiras.dados ? `de ${nº(totais.carteiras)} carteiras` : null}
          sobre={`De ${totais.carteiras} carteiras, as que declaram ao menos uma cidade — a diferença é a lacuna do cadastro de origem.`}
        />
        <CartaoDeDecisao
          rotulo="Municípios atendidos"
          icone={MapPin}
          tom="demanda"
          valor={carteiras.dados ? nº(totais.municipios) : null}
          carregando={carteiras.carregando}
          unidade="municípios"
          motivoSemDado={semCarteira}
          variacao={carteiras.dados ? 'distintos, pelas carteiras' : null}
          sobre="Municípios distintos declarados nas carteiras (organizacao.CarteiraMunicipio)."
        />
        <CartaoDeDecisao
          rotulo="Estados"
          icone={IconeMapa}
          tom="captura"
          valor={carteiras.dados ? totais.ufs.join(', ') || null : null}
          carregando={carteiras.carregando}
          motivoSemDado={carteiras.carregando ? undefined : 'Nenhuma carteira declara cidade.'}
          variacao={carteiras.dados && totais.ufs.length > 0 ? 'pelas cidades cadastradas' : null}
          sobre="As UFs alcançadas pelas cidades cadastradas nas carteiras."
        />
      </div>

      {/* CADA AVISO FICA ONDE ELE QUALIFICA ALGUMA COISA.

          As duas rotas devolvem cada uma a sua lista de `metricasSemDado`, e as
          duas falam das mesmas 30 carteiras sem município — empilhadas no topo,
          eram duas caixas laranja quase iguais antes de qualquer número. A da
          filial fica aqui, porque é ela que qualifica os indicadores acima; a
          da lista de carteiras desceu para junto da lista, que é o que ela
          explica. */}
      <MetricasSemDado metricas={filiais.dados?.metricasSemDado} />

      {/* ------------------------------------------------------------------ */}
      {/* Os dois gráficos do protótipo, com o dado real                      */}
      {/* ------------------------------------------------------------------ */}
      <section className="dash-secao" data-bloco="secao-contato">
        <TituloDaSecao
          titulo="A cobertura de contato"
          subtitulo="Vínculos com interação nos últimos 90 dias, sobre o total da carteira."
          metodologia="O percentual é de contato em 90 dias sobre os vínculos da carteira, e o denominador está escrito na tela. O recorte por classe do vínculo saiu: a classe do vínculo entra como C por assunção na maioria dos casos. A classe do cliente — a curva ABC apurada do faturamento — segmenta a Cobertura de Carteira."
        />

        <div className="dash-duas-colunas dash-analitico">
          <PainelDoMomento
            titulo="Cobertura de contato"
            data-bloco="medidor"
            subtitulo="Vínculos com interação nos últimos 90 dias, sobre o total da carteira."
            dica={
              <>
                As faixas de cor são de leitura, e <strong>não são meta</strong>: nenhuma meta de cobertura foi cadastrada,
                nem no sistema antigo nem no protótipo, onde os 80% eram um número fixo no código. O medidor mostra onde a
                operação está, e não se ela passou.
              </>
            }
          >
            {cobertura.carregando && <BlocoCarregando oQue="a cobertura de contato" />}
            {cobertura.erro && <BlocoErro erro={cobertura.erro} aoTentarDeNovo={cobertura.recarregar} />}

            {cobertura.dados && coberturaGeral.pct === null && !cobertura.erro && (
              <BlocoVazio
                titulo="Sem vínculo de carteira para medir"
                texto="O medidor mede contato sobre carteira. Sem cliente carteirizado ao alcance deste contexto, não há denominador."
              />
            )}

            {coberturaGeral.pct !== null && (
              <div className="cad-medidor">
                <GraficoGauge valor={coberturaGeral.pct} faixas={FAIXAS_DO_MEDIDOR} />
                <p className="cad-medidor-legenda">
                  <strong>
                    {coberturaGeral.em90.toLocaleString('pt-BR')} de {coberturaGeral.clientes.toLocaleString('pt-BR')} vínculos
                  </strong>{' '}
                  tiveram contato nos últimos 90 dias.
                </p>
              </div>
            )}
          </PainelDoMomento>

          <PainelDoMomento
            titulo="Cobertura por carteira"
            data-bloco="por-carteira"
            subtitulo={`As ${Math.min(BARRAS_NO_GRAFICO, barras.length)} maiores carteiras desta filial · 90 dias.`}
            dica="A barra mede, e não julga: não há cor por atingimento, porque não há meta de cobertura. O denominador é 100%, para cada barra dizer quanto da carteira tem contato, e não só quem é maior."
          >
            {cobertura.carregando && <BlocoCarregando oQue="a cobertura por carteira" />}

            {cobertura.dados && barras.length === 0 && !cobertura.erro && (
              <BlocoVazio
                titulo="Nenhuma carteira com cliente vinculado"
                texto="As barras medem contato sobre carteira; sem vínculo não há barra para desenhar."
              />
            )}

            {barras.length > 0 && (
              /* A BARRA MEDE, E NÃO JULGA. Não há cor por atingimento aqui —
                 `organizacao.Meta` está vazia, e pintar de verde e vermelho
                 contra uma meta inventada afirmaria aprovação e reprovação que
                 ninguém definiu. O denominador é 100%, para cada barra dizer
                 quanto da carteira tem contato, e não só quem é maior. */
              <ListaDeBarras itens={barras} maximo={100} formatar={percentual} />
            )}
          </PainelDoMomento>
        </div>
      </section>

      <section className="dash-secao" data-bloco="secao-territorio">
        <TituloDaSecao
          titulo="O território"
          subtitulo="Filial → carteira → cidades, os dois níveis que existem no dado."
          metodologia="IVS_Carteira.NroEmpresa está preenchida em todas as carteiras, e IVS_CartCid liga carteira e cidade. A carteira sem cidade aparece com a lista vazia em vez de sumir: escondê-la faria a tela mostrar uma operação menor do que ela é."
        />

        <PainelDoMomento
          titulo="Nível 1 — por filial"
          data-bloco="por-filial"
          subtitulo="Só as filiais que este contexto de acesso alcança. A fronteira de multiempresa vale aqui como em toda consulta."
        >
          {filiais.carregando && <BlocoCarregando oQue="a cobertura por filial" />}
          {filiais.erro && <BlocoErro erro={filiais.erro} aoTentarDeNovo={filiais.recarregar} />}

          {filiais.dados && listaDeFiliais.length === 0 && !filiais.erro && (
            <BlocoVazio
              titulo="Nenhuma filial com carteira ao seu alcance"
              texto="A cobertura territorial parte da carteira, e a carteira carrega a fronteira de filial. Troque a filial no cabeçalho para conferir."
            />
          )}

          {listaDeFiliais.length > 0 && (
            <div className="mom-tabela-rolagem">
              <table className="mom-tabela">
                <caption className="cad-so-leitor">Cobertura territorial por filial</caption>
                <thead>
                  <tr>
                    <th scope="col">Filial</th>
                    <th scope="col" className="mom-num">Carteiras</th>
                    <th scope="col" className="mom-num">Com cidade</th>
                    <th scope="col" className="mom-num">Municípios</th>
                    <th scope="col">Estados</th>
                  </tr>
                </thead>
                <tbody>
                  {listaDeFiliais.map((f) => (
                    <tr key={f.empresaChave}>
                      <th scope="row">
                        <div className="cad-link-forte">{f.empresaNome}</div>
                        <div className="cad-sub">{f.empresaCodigo}</div>
                      </th>
                      <td className="mom-num">{f.carteiras}</td>
                      <td className="mom-num">
                        <span className={f.carteirasComMunicipio < f.carteiras ? 'cad-atencao' : undefined}>
                          {f.carteirasComMunicipio}
                        </span>
                        <div className="cad-sub">{f.carteiras - f.carteirasComMunicipio} sem nenhuma cidade</div>
                      </td>
                      <td className="mom-num">{f.municipios}</td>
                      <td>{f.ufs.length > 0 ? f.ufs.join(', ') : <span className="cad-nada">—</span>}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Nível 2 — por carteira"
          data-bloco="por-carteira-territorio"
          subtitulo={
            carteiras.recarregando
              ? 'Atualizando…'
              : `${visiveis.length} de ${totais.carteiras} carteiras · ${semCidade} não declaram nenhuma cidade no sistema de origem e aparecem com zero, em vez de sumir da lista`
          }
          dica="Clique numa carteira para ver as cidades que ela atende. A carteira sem cidade fica na lista, com zero, e não abre."
        >
          <MetricasSemDado metricas={carteiras.dados?.metricasSemDado} titulo="O que esta lista não diz" />

          {carteiras.carregando && <BlocoCarregando oQue="o território das carteiras" />}
          {carteiras.erro && <BlocoErro erro={carteiras.erro} aoTentarDeNovo={carteiras.recarregar} />}

          {carteiras.dados && visiveis.length === 0 && !carteiras.erro && (
            <BlocoVazio
              titulo="Nenhuma carteira desta filial declara cidade"
              texto={`O vínculo carteira × município vem da carga do sistema de origem. Das ${totais.carteiras} carteiras carregadas, ${semCidade} não têm nenhuma cidade cadastrada — a lacuna é do cadastro, não da operação.`}
              acao={
                somenteComCidade ? (
                  <button type="button" className="btn btn-secondary" onClick={() => setSomenteComCidade(false)}>
                    Mostrar todas as carteiras
                  </button>
                ) : undefined
              }
            />
          )}

          {visiveis.map((carteira) => (
            <GrupoDeCarteira
              key={carteira.carteiraChave}
              carteira={carteira}
              aberta={abertas.has(carteira.carteiraChave)}
              aoAlternar={() => alternar(carteira.carteiraChave)}
            />
          ))}
        </PainelDoMomento>
      </section>

      {/* A CORREÇÃO DE PREMISSA CONTINUA NA TELA, e não só no documento: quem
          abriu esta rota antes viu sete regionais e pode voltar procurando por
          elas. Ela saiu do topo, onde era a primeira coisa que a diretoria lia,
          e passou a ficar depois do território que de fato existe. */}
      <BlocoRecolhivel titulo="Por que esta tela não tem regional" resumo="as sete regionais do protótipo não existem no dado">
        <p className="cad-coluna-meta">
          MT Norte, GO, BA Oeste e mais quatro vinham do protótipo. No Vórtice, o cadastro de
          regionais <strong>existe e está vazio</strong> — nenhum dado sustenta essas regionais. E as
          treze filiais em operação estão todas no interior de São Paulo. O agrupamento preenchido é{' '}
          <strong>filial → carteira → cidades</strong>, e é o que esta tela mostra.
        </p>
      </BlocoRecolhivel>
    </PaginaDoPainel>
  );
}


/** Uma carteira, com as cidades que ela atende — ou a declaração de que não tem nenhuma. */
function GrupoDeCarteira({
  carteira,
  aberta,
  aoAlternar,
}: {
  carteira: TerritorioDeCarteira;
  aberta: boolean;
  aoAlternar: () => void;
}) {
  const semCidade = carteira.municipios.length === 0;
  const ufs = [...new Set(carteira.municipios.map((m) => m.uf))].sort();

  return (
    <div className="cad-grupo">
      <button
        type="button"
        className="cad-grupo-cabecalho"
        onClick={aoAlternar}
        aria-expanded={aberta}
        disabled={semCidade}
      >
        <div>
          <div className="cad-grupo-nome">
            {carteira.carteiraNome}
            {!semCidade && <span aria-hidden="true"> {aberta ? '▾' : '▸'}</span>}
          </div>
          <div className="cad-grupo-meta">
            {carteira.linhaDeNegocioNome} · {carteira.responsavelNome} · {carteira.empresaNome}
          </div>
        </div>
        <div className="cad-grupo-numeros">
          <div className="cad-grupo-numero">
            <strong>{carteira.municipios.length}</strong>
            {carteira.municipios.length === 1 ? 'cidade' : 'cidades'}
          </div>
          {ufs.length > 0 && (
            <div className="cad-grupo-numero">
              <strong>{ufs.join(', ')}</strong>
              {ufs.length === 1 ? 'estado' : 'estados'}
            </div>
          )}
        </div>
      </button>

      {aberta && !semCidade && (
        <div className="cad-grupo-corpo">
          <div className="cad-cidades">
            {carteira.municipios.map((m) => (
              <span key={m.id} className="cad-cidade">
                {m.nome}/{m.uf}
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

/** Uma casa decimal, com vírgula, como o resto da aplicação escreve. */
function percentual(valor: number): string {
  return `${valor.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`;
}
