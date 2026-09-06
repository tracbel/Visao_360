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
 * A LACUNA É DECLARADA, NÃO ESCONDIDA. Das 142 carteiras carregadas, 73 têm
 * cidade e 69 não têm — quase metade. **A carteira sem cidade aparece com a
 * lista vazia em vez de sumir**: escondê-la faria a tela mostrar uma operação
 * menor do que ela é. E o número vem da API, em `metricasSemDado`.
 */

import { useMemo, useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ListaDeBarras } from '../componentes/ListaDeBarras';
import { GraficoGauge } from '../componentes/GraficoGauge';
import { useContextoDeAcesso } from '../dados/api/contexto';
import {
  listarTerritorioPorCarteira,
  obterCoberturaPorFilial,
  obterResumoDeCobertura,
} from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type { TerritorioDeCarteira } from '../tipos/relacionamento';

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

  const listaDeFiliais = filiais.dados?.itens ?? [];
  const listaDeCarteiras = carteiras.dados?.itens ?? [];

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

  const indicadores: Indicador[] = [
    {
      rotulo: 'Filiais ao seu alcance',
      valor: filiais.dados ? listaDeFiliais.length : null,
      deOnde: 'o contexto de acesso enxerga uma filial por vez',
      semDado: 'sem filial com carteira ao alcance',
    },
    {
      rotulo: 'Carteiras',
      valor: carteiras.dados ? totais.carteiras : null,
      deOnde: 'carteiras destas filiais, com e sem cidade',
      semDado: '—',
    },
    {
      rotulo: 'Com cidade cadastrada',
      valor: carteiras.dados ? totais.comCidade : null,
      tom: 'atencao',
      deOnde: `de ${totais.carteiras} — a diferença é a lacuna do cadastro de origem`,
      semDado: '—',
    },
    {
      rotulo: 'Municípios atendidos',
      valor: carteiras.dados ? totais.municipios : null,
      deOnde: 'municípios distintos, de organizacao.CarteiraMunicipio',
      semDado: '—',
    },
    {
      rotulo: 'Estados',
      valor: carteiras.dados ? totais.ufs.join(', ') || null : null,
      deOnde: 'as UFs alcançadas pelas cidades cadastradas',
      semDado: 'nenhuma carteira declara cidade',
    },
  ];

  /**
   * A cobertura de contato, que é o que os dois gráficos do protótipo mediam.
   *
   * O PERCENTUAL É DE CONTATO EM 90 DIAS sobre os vínculos da carteira, e o
   * denominador está escrito na tela. O protótipo media "clientes A ou B
   * tocados" — a classe A/B não se sustenta (59 de 49.109 vínculos têm classe
   * lida), então o recorte por classe saiu e o universo passou a ser a carteira
   * inteira, que é o que o dado sustenta.
   */
  const carteirasComCobertura = cobertura.dados?.itens ?? [];

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

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Cobertura por Filial e Carteira</h1>
          <p className="page-subtitle">
            <code>organizacao.CarteiraMunicipio</code> — o agrupamento territorial que existe no
            dado: a filial da carteira e as cidades que ela atende.
          </p>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={filiais.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={filiais.carregando || carteiras.carregando} />

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
      <div className="p360-grid">
        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Cobertura de contato</div>
              <div className="card-subtitle">
                vínculos com interação nos últimos 90 dias, sobre o total da carteira
              </div>
            </div>
            <SeloProcedencia procedencia={cobertura.procedencia} />
          </div>

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
                  {coberturaGeral.em90.toLocaleString('pt-BR')} de{' '}
                  {coberturaGeral.clientes.toLocaleString('pt-BR')} vínculos
                </strong>{' '}
                tiveram contato nos últimos 90 dias.
              </p>
              <p className="cad-medidor-nota">
                As faixas de cor são de leitura, e <strong>não são meta</strong>:
                <code>organizacao.Meta</code> está vazia e não há fonte no legado nem no protótipo,
                onde a meta de 80% era um número escrito no JavaScript. O medidor mostra onde a
                operação está, e não se ela passou.
              </p>
            </div>
          )}
        </div>

        <div className="card cad-cartao">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Cobertura por carteira</div>
              <div className="card-subtitle">
                as {Math.min(BARRAS_NO_GRAFICO, barras.length)} maiores carteiras desta filial · 90 dias
              </div>
            </div>
          </div>

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
        </div>
      </div>

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Nível 1 — por filial</div>
            <div className="card-subtitle">
              Só as filiais que este contexto de acesso alcança. A fronteira de multiempresa vale
              aqui como em toda consulta.
            </div>
          </div>
          <SeloProcedencia procedencia={filiais.procedencia} />
        </div>

        {filiais.carregando && <BlocoCarregando oQue="a cobertura por filial" />}
        {filiais.erro && <BlocoErro erro={filiais.erro} aoTentarDeNovo={filiais.recarregar} />}

        {filiais.dados && listaDeFiliais.length === 0 && !filiais.erro && (
          <BlocoVazio
            titulo="Nenhuma filial com carteira ao seu alcance"
            texto="A cobertura territorial parte da carteira, e a carteira carrega a fronteira de filial. Troque a filial no cabeçalho para conferir."
          />
        )}

        {listaDeFiliais.length > 0 && (
          <div className="cad-tabela-wrap">
            <table className="cad-tabela">
              <caption className="cad-so-leitor">Cobertura territorial por filial</caption>
              <thead>
                <tr>
                  <th scope="col">Filial</th>
                  <th scope="col">Carteiras</th>
                  <th scope="col">Com cidade</th>
                  <th scope="col">Municípios</th>
                  <th scope="col">Estados</th>
                </tr>
              </thead>
              <tbody>
                {listaDeFiliais.map((f) => (
                  <tr key={f.empresaChave}>
                    <td>
                      <div className="cad-link-forte">{f.empresaNome}</div>
                      <div className="cad-sub cad-mono">{f.empresaCodigo}</div>
                    </td>
                    <td className="cad-mono">{f.carteiras}</td>
                    <td className="cad-mono">
                      <span className={f.carteirasComMunicipio < f.carteiras ? 'cad-atencao' : undefined}>
                        {f.carteirasComMunicipio}
                      </span>
                      <div className="cad-sub">
                        {f.carteiras - f.carteirasComMunicipio} sem nenhuma cidade
                      </div>
                    </td>
                    <td className="cad-mono">{f.municipios}</td>
                    <td>{f.ufs.length > 0 ? f.ufs.join(', ') : <span className="cad-nada">—</span>}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <div className="cad-barra">
        <label className="cad-filtro cad-filtro-caixa">
          <input
            type="checkbox"
            checked={somenteComCidade}
            onChange={(e) => setSomenteComCidade(e.target.checked)}
          />
          Só as carteiras com cidade cadastrada
        </label>
        {somenteComCidade && (
          <button type="button" className="btn btn-secondary btn-sm" onClick={() => setSomenteComCidade(false)}>
            Mostrar todas
          </button>
        )}
      </div>

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Nível 2 — por carteira</div>
            <div className="card-subtitle">
              {carteiras.recarregando
                ? 'Atualizando…'
                : `${visiveis.length} de ${totais.carteiras} carteiras · ${semCidade} não declaram nenhuma cidade no sistema de origem e aparecem com zero, em vez de sumir da lista`}
            </div>
          </div>
          <SeloProcedencia procedencia={carteiras.procedencia} />
        </div>

        <MetricasSemDado
          metricas={carteiras.dados?.metricasSemDado}
          titulo="O que esta lista não diz"
        />

        {carteiras.carregando && <BlocoCarregando oQue="o território das carteiras" />}
        {carteiras.erro && <BlocoErro erro={carteiras.erro} aoTentarDeNovo={carteiras.recarregar} />}

        {carteiras.dados && visiveis.length === 0 && !carteiras.erro && (
          <BlocoVazio
            titulo="Nenhuma carteira desta filial declara cidade"
            texto="O vínculo carteira × município vem da carga do sistema de origem. Das 142 carteiras carregadas, 69 não têm nenhuma cidade cadastrada — a lacuna é do cadastro, não da operação."
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
      </div>

      {/* A CORREÇÃO DE PREMISSA CONTINUA NA TELA, e não só no documento: quem
          abriu esta rota antes viu sete regionais e pode voltar procurando por
          elas. Ela saiu do topo, onde era a primeira coisa que a diretoria lia,
          e passou a ficar depois do território que de fato existe. */}
      <BlocoRecolhivel
        titulo="Por que esta tela não tem regional"
        resumo="as sete regionais do protótipo não existem no dado"
      >
        <p className="cad-coluna-meta">
          MT Norte, GO, BA Oeste e mais quatro vinham do protótipo. No Vórtice, a tabela de regional
          (<code>IVS_Regional</code>) <strong>existe e tem zero linhas</strong> — não há tabela,
          coluna nem valor de texto que as sustente. E as treze filiais em operação estão todas no
          interior de São Paulo. O agrupamento preenchido é{' '}
          <strong>filial → carteira → cidades</strong>, e é o que esta tela mostra.
        </p>
      </BlocoRecolhivel>
    </>
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
