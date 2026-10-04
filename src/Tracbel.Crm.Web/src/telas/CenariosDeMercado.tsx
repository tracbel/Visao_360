/**
 * Cenários de Mercado — o planejamento da meta de cada município, do gestor com o CEN (issue 263, a aba "Cenários de
 * Mercado" do protótipo da pasta 360).
 *
 * O DESENHO É O DAS TELAS DE MERCADO: filtros na linha (produto, ano fiscal, regional e loja; o intervalo do realizado em
 * "Mais filtros"), os quatro números do topo e a tabela de planejamento, paginada e exportável.
 *
 * NENHUMA CONTA NOVA: o potencial, o mercado ajustado e a meta estrutural são os da Demanda e Previsão — o mesmo motor, o
 * mesmo fator de ciclo e o mesmo share-alvo. O que esta tela acrescenta é a DECISÃO: conservador, moderado, otimista ou um
 * número, gravado por município e ano fiscal, com autor e trilha (decisões do Ricardo de 02/10/2026 — grava a Gerência e a
 * Diretoria; o CEN vê). No protótipo a escolha ficava no navegador de quem clicou.
 */

import * as Popover from '@radix-ui/react-popover';
import { CalendarDays, Flag, Funnel, Gauge, MapPin, Package, RefreshCw, Store, Target, Tractor } from 'lucide-react';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { cabecalhoDoCsv, linhasDoCsv, mesCurto, n, nomeDoAnoFiscal, aEntregarDe } from '../componentes/cenarios/cenarios';
import { TabelaDosCenarios, type AoEscolher } from '../componentes/cenarios/TabelaDosCenarios';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { escolherMetaDoCenario, obterCenariosDeMercado } from '../dados/api/mercado';
import { useRecurso } from '../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../dados/exportarCsv';
import type { CenariosDeMercado as Cenarios, EscolhaDoCenario, FiltrosDosCenarios } from '../tipos/mercado';
import { CampoDoFiltro } from '../componentes/comum/CampoDoFiltro';
import '../estilos/territorio.css';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/diagnostico.css';
import '../estilos/demanda.css';
import '../estilos/cenarios.css';

const PRODUTOS = [
  { codigo: 'TRATOR', nome: 'Tratores' },
  { codigo: 'COLHEDORA_DE_CANA', nome: 'Colhedora de cana' },
];

export function CenariosDeMercado() {
  const { contexto } = useContextoDeAcesso();
  const [filtros, setFiltros] = useState<FiltrosDosCenarios>({});
  const [busca, setBusca] = useState('');
  const [lojasConhecidas, setLojasConhecidas] = useState<Map<string, string>>(() => new Map());
  const mudar = (parcial: Partial<FiltrosDosCenarios>) => setFiltros((f) => ({ ...f, ...parcial }));

  const leitura = useRecurso(
    (sinal) =>
      obterCenariosDeMercado(contexto, filtros, sinal).then((resposta) => {
        setLojasConhecidas((atual) => {
          const proximo = new Map(atual);
          for (const m of resposta.dados.municipios) if (m.lojaCodigo && m.loja) proximo.set(m.lojaCodigo, m.loja);
          return proximo.size === atual.size ? atual : proximo;
        });
        return resposta;
      }),
    [contexto.empresa, contexto.usuario, filtros.categoria, filtros.anoFiscal, filtros.de, filtros.ate, filtros.regiao, filtros.lojaCodigo],
  );
  const dados = leitura.dados;

  // AS ESCOLHAS GRAVADAS NESTA VISITA ficam por cima da leitura — sem reler a tela inteira a cada clique — e somem quando a
  // leitura muda (outro filtro, ou reler): aí a resposta já traz o que foi gravado.
  const [gravadas, setGravadas] = useState<{ leitura: Cenarios | null; escolhas: ReadonlyMap<number, EscolhaDoCenario | null> }>({
    leitura: null,
    escolhas: new Map(),
  });
  const escolhas = gravadas.leitura === dados ? gravadas.escolhas : new Map<number, EscolhaDoCenario | null>();

  const aoEscolher: AoEscolher = async (codigoIbge, cenario, valorManual) => {
    if (!dados) return;
    const escolha = await escolherMetaDoCenario(contexto, codigoIbge, {
      categoria: dados.categoria,
      anoFiscal: String(dados.anoFiscal),
      cenario,
      valorManual,
    });
    setGravadas((g) => ({ leitura: dados, escolhas: new Map(g.leitura === dados ? g.escolhas : []).set(codigoIbge, escolha) }));
  };

  const totais = dados?.totais;
  const aEntregar = dados
    ? dados.municipios.reduce((soma, m) => soma + (aEntregarDe(m, escolhas.has(m.codigoIbge) ? (escolhas.get(m.codigoIbge) ?? null) : m.escolha) ?? 0), 0)
    : null;
  const comMeta = dados
    ? dados.municipios.filter((m) => (escolhas.has(m.codigoIbge) ? escolhas.get(m.codigoIbge) : m.escolha) != null).length
    : 0;
  const share = dados?.shareAlvo == null ? 'share-alvo' : `${n(dados.shareAlvo, 0)}%`;
  const media = dados && dados.anosDaMedia.length > 0 ? `FY${dados.anosDaMedia[0]}–FY${dados.anosDaMedia[dados.anosDaMedia.length - 1]}` : '';
  const intervalo = dados ? `${mesCurto(dados.intervaloDe)} a ${mesCurto(dados.intervaloAte)}` : '';
  const semRealizado = dados?.lacunas.find((l) => l.metrica === 'realizado')?.motivo ?? 'O ART não trouxe entrega ao alcance desta consulta.';
  const secundariosAtivos = filtros.de || filtros.ate ? 1 : 0;

  return (
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Cenários de Mercado</h1>
          <p className="page-subtitle">
            Planeje a meta de cada município com o CEN: o realizado, o potencial e três cenários do mercado — ou um número seu.
          </p>
        </div>
        <p className="dash-atualizado">
          {leitura.procedencia ? <DadosAtualizadosEm procedencia={leitura.procedencia} /> : 'Lendo os cenários…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={leitura.recarregar}
            disabled={leitura.carregando}
            data-carregando={leitura.carregando ? 'true' : 'false'}
            aria-label="Reler os cenários"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <CampoDoFiltro icone={Tractor} rotulo="Produto" bloco="produto">
            <select value={filtros.categoria ?? dados?.categoria ?? 'TRATOR'} onChange={(e) => mudar({ categoria: e.target.value })}>
              {PRODUTOS.map((p) => (
                <option key={p.codigo} value={p.codigo}>
                  {p.nome}
                </option>
              ))}
            </select>
          </CampoDoFiltro>
          <CampoDoFiltro icone={CalendarDays} rotulo="Ano fiscal da meta" bloco="ano">
            <select
              value={filtros.anoFiscal ?? dados?.anoFiscal ?? ''}
              onChange={(e) => mudar({ anoFiscal: e.target.value ? Number(e.target.value) : undefined, de: undefined, ate: undefined })}
            >
              {(dados?.anosFiscais ?? []).map((ano, i) => (
                <option key={ano} value={ano} title={nomeDoAnoFiscal(ano)}>
                  FY{ano}
                  {i === 0 ? ' (próximo)' : i === 1 ? ' (corrente)' : ''}
                </option>
              ))}
              {!dados && <option value="">Ano fiscal corrente</option>}
            </select>
          </CampoDoFiltro>
          <CampoDoFiltro icone={MapPin} rotulo="Regional" bloco="regional">
            <select value={filtros.regiao ?? ''} onChange={(e) => mudar({ regiao: e.target.value || undefined })}>
              <option value="">Todas as regionais</option>
              <option value="Norte">Região Norte</option>
              <option value="Noroeste">Região Noroeste</option>
            </select>
          </CampoDoFiltro>
          <CampoDoFiltro icone={Store} rotulo="Loja / Filial" bloco="loja">
            <select value={filtros.lojaCodigo ?? ''} onChange={(e) => mudar({ lojaCodigo: e.target.value || undefined })}>
              <option value="">Todas as lojas</option>
              {[...lojasConhecidas.entries()]
                .sort((a, b) => a[1].localeCompare(b[1], 'pt-BR'))
                .map(([codigo, nome]) => (
                  <option key={codigo} value={codigo}>
                    {nome}
                  </option>
                ))}
            </select>
          </CampoDoFiltro>

          <div className="dash-filtros-acao">
            <Popover.Root>
              <Popover.Trigger asChild>
                <button type="button" className="dash-mais-filtros" data-bloco="mais-filtros">
                  <Funnel size={15} strokeWidth={2} aria-hidden="true" />
                  Mais filtros
                  {secundariosAtivos > 0 && <span className="dash-mais-filtros-selo">{secundariosAtivos}</span>}
                </button>
              </Popover.Trigger>
              <Popover.Portal>
                <Popover.Content className="dash-popover" sideOffset={6} collisionPadding={16} align="end">
                  <div className="dash-popover-titulo">Intervalo do realizado</div>
                  <label className="dash-filtro">
                    <span className="dash-filtro-rotulo">De</span>
                    <input
                      type="month"
                      value={filtros.de ?? dados?.intervaloDe ?? ''}
                      onChange={(e) => mudar({ de: e.target.value || undefined, ate: filtros.ate ?? dados?.intervaloAte })}
                    />
                  </label>
                  <label className="dash-filtro">
                    <span className="dash-filtro-rotulo">Até</span>
                    <input
                      type="month"
                      value={filtros.ate ?? dados?.intervaloAte ?? ''}
                      onChange={(e) => mudar({ ate: e.target.value || undefined, de: filtros.de ?? dados?.intervaloDe })}
                    />
                  </label>
                  {(filtros.de || filtros.ate) && (
                    <button type="button" className="btn btn-secondary btn-sm" onClick={() => mudar({ de: undefined, ate: undefined })}>
                      Voltar ao ano fiscal até hoje
                    </button>
                  )}
                </Popover.Content>
              </Popover.Portal>
            </Popover.Root>
          </div>
        </div>
      </div>

      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo={dados ? `Realizado FY${dados.anoFiscal}` : 'Realizado no ano fiscal'}
          icone={Package}
          tom="captura"
          valor={totais?.realizadoNoAno != null ? n(totais.realizadoNoAno, 0) : null}
          carregando={leitura.carregando && !dados}
          unidade="unidades"
          variacao={
            totais?.realizadoNoAno != null ? (
              <>
                <span className="cen-kpi-linha">{`média de ${n(totais.mediaDosAnosAnteriores ?? 0)} por ano em ${media}`}</span>
                <span className="cen-kpi-linha">{`${n(totais.realizadoNoIntervalo ?? 0, 0)} no intervalo (${intervalo})`}</span>
              </>
            ) : null
          }
          motivoSemDado={semRealizado}
          sobre={`As máquinas ${dados?.categoriaNome.toLocaleLowerCase('pt-BR') ?? ''} entregues pelo ART no ano fiscal${dados?.situacaoDoAno === 'Corrente' ? ' até hoje' : ''}, pela data da ENTREGA, dos compradores dos municípios do recorte. A média é a dos quatro anos fiscais fechados antes do escolhido; o intervalo se escolhe em "Mais filtros".`}
        />
        <CartaoDeDecisao
          rotulo={`Meta estrutural (${share})`}
          icone={Target}
          tom="demanda"
          valor={totais?.metaEstrutural != null ? n(totais.metaEstrutural) : null}
          carregando={leitura.carregando && !dados}
          unidade="unidades"
          variacao={totais?.potencial != null ? `potencial de ${n(totais.potencial)} máquinas × share-alvo` : null}
          motivoSemDado={dados?.shareAlvo == null ? 'A categoria não tem share-alvo vigente (Configurações › Potencial de mercado).' : 'Nenhum município do recorte tem regra de potencial nesta categoria.'}
          sobre={`O potencial estrutural (a renovação anual do parque que a área comporta) × o share-alvo de ${share}: o que entregar para atingir o share no potencial.${dados?.shareDoPrototipo ? ' O share-alvo ainda é o do protótipo, a confirmar.' : ''}`}
        />
        <CartaoDeDecisao
          rotulo="Realização vs potencial"
          icone={Gauge}
          tom="mercado"
          valor={totais?.realizacaoSobrePotencial != null ? `${n(totais.realizacaoSobrePotencial, 0)}%` : null}
          carregando={leitura.carregando && !dados}
          variacao={dados ? `realizado ÷ potencial${dados.situacaoDoAno === 'Corrente' ? ' · o ano ainda não fechou' : ''}` : null}
          motivoSemDado={totais?.realizadoNoAno == null ? semRealizado : 'Sem potencial calculado no recorte.'}
          sobre="O realizado do ano fiscal ÷ o potencial estrutural. Não é share de mercado: o potencial é a renovação teórica do parque, e um município pode passar de 100%."
        />
        <CartaoDeDecisao
          rotulo="A entregar (cenários)"
          icone={Flag}
          tom="oportunidade"
          valor={aEntregar !== null && dados && dados.municipios.length > 0 ? n(aEntregar) : null}
          carregando={leitura.carregando && !dados}
          unidade="unidades"
          variacao={dados ? `${n(comMeta, 0)} de ${n(dados.municipios.length, 0)} municípios com meta gravada` : null}
          motivoSemDado="Nenhum município do recorte tem mercado nesta categoria."
          sobre="A soma das metas: a combinada onde alguém escolheu, e o cenário moderado (o mercado ajustado × share) onde ninguém escolheu ainda."
        />
      </div>

      {leitura.carregando && !dados && <BlocoCarregando oQue="os cenários" />}

      {dados && (
        <section className="dash-secao" data-bloco="secao-planejamento">
          <TituloDaSecao
            titulo={`Planejamento por município — ${dados.categoriaNome}`}
            subtitulo={`${nomeDoAnoFiscal(dados.anoFiscal)} · cenários sobre o mercado ajustado pelo momento (C −5% / M / O +5%) ou um número manual`}
            metodologia={<Metodologia dados={dados} share={share} />}
          />
          {!dados.podeGravar && dados.porQueNaoGrava && (
            <p className="cen-aviso" data-bloco="so-leitura">
              {dados.porQueNaoGrava}
            </p>
          )}
          <PainelDoMomento
            titulo="Metas por município"
            dica="Clique em C, M ou O para gravar o cenário do município — o número é calculado com o mercado de hoje e fica combinado — ou digite a meta e tecle Enter. Sem escolha, vale o moderado (o M tracejado). Clique nos cabeçalhos para ordenar."
            direita={
              <div className="dem-acoes-da-matriz">
                <input
                  type="search"
                  className="dem-busca"
                  value={busca}
                  placeholder="Buscar município ou loja"
                  aria-label="Buscar município ou loja no planejamento"
                  onChange={(e) => setBusca(e.target.value)}
                />
                <button
                  type="button"
                  className="btn btn-secondary btn-sm"
                  onClick={() =>
                    baixarCsv(`cenarios-${dados.categoria.toLowerCase()}-fy${dados.anoFiscal}-${carimboDeData()}`, cabecalhoDoCsv(dados), linhasDoCsv(dados, escolhas))
                  }
                  disabled={dados.municipios.length === 0}
                >
                  Exportar CSV
                </button>
              </div>
            }
            data-bloco="planejamento"
          >
            <TabelaDosCenarios
              municipios={dados.municipios}
              escolhas={escolhas}
              busca={busca}
              anoFiscal={dados.anoFiscal}
              shareAlvo={dados.shareAlvo}
              podeGravar={dados.podeGravar}
              porQueNaoGrava={dados.porQueNaoGrava}
              aoEscolher={aoEscolher}
            />
          </PainelDoMomento>
        </section>
      )}
    </PaginaDoPainel>
  );
}

function Metodologia({ dados, share }: { dados: Cenarios; share: string }) {
  return (
    <>
      <p>
        O potencial é a renovação anual do parque que a área plantada comporta (PAM do IBGE × as regras do potencial); o mercado
        ajustado é o mesmo potencial pelo fator de ciclo do CRM — o preço das culturas, o crédito do município e a percepção. A
        meta estrutural é o potencial × o share-alvo de {share}. Os três cenários são o mercado ajustado × o share com −5%, 0% e +5%;
        sem fator de ciclo, partem da meta estrutural.
      </p>
      <p>
        O realizado é a entrega do ART pela data da entrega, no ano fiscal (novembro a outubro), pelo município do comprador. A
        recomendação compara o moderado com o ano fiscal anterior inteiro: acima de 1,5 vez sugere uma rampa de 1,3 vez o
        realizado; sem histórico e acima de 2 máquinas, pede validação. O share estrutural é realizado ÷ potencial — não é share
        de mercado, e pode passar de 100%.
      </p>
      <p>
        A meta gravada é uma por município, categoria e ano fiscal; mudar de ideia altera a mesma, e a trilha guarda o antes, quem
        e quando. {dados.alcanceDaGravacao ? `Você grava em: ${dados.alcanceDaGravacao.toLocaleLowerCase('pt-BR')}.` : ''}
      </p>
      {dados.lacunas.length > 0 && <MetricasSemDado metricas={dados.lacunas} titulo="O que esta leitura não afirma" naDica />}
    </>
  );
}
