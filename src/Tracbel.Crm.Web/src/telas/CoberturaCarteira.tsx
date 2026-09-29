/**
 * Cobertura de Carteira — ligada ao dado real de `comercial.ClienteCarteira`.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a tela deixou de ler o JSON do protótipo.
 *
 * Antes ela montava a carteira de `public/dados/carteira-cen.json` e
 * `cobertura.json`, ordenava **por classe de cliente** e desenhava um mapa com
 * as coordenadas de `cidades-latlng.json`. Agora lê `/api/v1/cobertura` —
 * **15.104 vínculos** só na filial de Ribeirão Preto — e o resumo por carteira
 * de `/api/v1/relatorios/cobertura`, agrupado no banco.
 *
 * ---------------------------------------------------------------------------
 * 27/09/2026 — o último contato passou a vir do histórico INTEIRO do Vórtice.
 *
 * Até aqui, `UltimaInteracaoEm` era apurada só das interações que a carga já
 * tinha trazido para o CRM — um recorte. Agora ela vem da regra da BI de
 * carteiras do Vórtice (`BI_CARTEIRA_VN`): 53 resultados que contam como
 * contato, em qualquer canal e em qualquer departamento, calculados todo dia
 * pela rotina `CARTEIRAS_VORTICE` sobre o histórico inteiro — e a data só anda
 * para a frente. Hoje, 5.412 dos 8.537 vínculos ativos (63%) têm essa data;
 * nas carteiras de campo (`MAQ_`), 96,7%.
 *
 * ---------------------------------------------------------------------------
 * AS TRÊS COISAS QUE MUDARAM, e as três são a mesma decisão:
 *
 * 1. **A ordenação por classe A/B saiu — mas a SEGMENTAÇÃO por classe voltou.**
 *    A classe do VÍNCULO (`ClienteCarteira.Classe`) continua sem se sustentar:
 *    ela nasce de `IVS_Pes.Potencial`, um `varchar(3)` sem catálogo, e segue
 *    entrando como `C` por assunção na maioria dos vínculos (documento 25,
 *    §5.1) — ordenar por ela seguiria sendo ordenar por nada. O que mudou é
 *    que `Cliente.Classe` — a curva ABC apurada do faturamento (A 734 · B
 *    1.045 · C 5.726 · D 19.831) — está pronta, e o filtro `classe` da rota
 *    passou a usar essa classe, e não a do vínculo.
 *
 *    **A ordem continua sendo quem está há mais tempo sem contato**, que é
 *    dado real e calculado do fato.
 *
 * 2. **O mapa continua no lugar dele, vazio e com o motivo.** Ele plotava as
 *    coordenadas do protótipo, que são de Mato Grosso, Goiás e Bahia —
 *    geografia que não existe nesta operação: as treze filiais estão todas no
 *    interior de São Paulo. **A moldura fica**: tirar o bloco faria a tela
 *    perder um elemento sem explicar nada, e uma tela de gerência que perde um
 *    gráfico é pior do que uma que mostra o gráfico vazio com o motivo escrito.
 *    Falta uma fonte de coordenada por cliente — `organizacao.Municipio` não
 *    guarda latitude e longitude, e as interações têm coordenada em 24% dos
 *    registros sem rota que as agregue. O território que existe está na
 *    Cobertura por Filial e Carteira (documento 26).
 *
 * 3. **"Registrar contato" saiu.** Gravava em `localStorage` e mexia nos
 *    contadores da própria tela. Interação é fato imutável e somente
 *    acrescentar, e nenhuma rota de relacionamento escreve (dívida D-9).
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS (pedido do Ricardo: "vamos deixar todas as telas no padrão de
 * indicadores geográficos"). O cabeçalho com a hora da leitura, a barra de filtros, os cinco números como
 * `CartaoDeDecisao` e duas seções — qual carteira está descoberta e quem contatar primeiro — em `PainelDoMomento`. A
 * frase longa de onde vem o contato foi para a dica do subtítulo; os `title=` viraram dica que abre pelo teclado.
 * NENHUM NÚMERO, REGRA OU TEXTO DE REGRA MUDOU.
 */

import { BriefcaseBusiness, CalendarCheck, CalendarClock, Layers, RefreshCw, Users, UserX } from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { GraficoBarrasEmpilhadas } from '../componentes/GraficoBarrasEmpilhadas';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { MolduraDeGrafico } from '../componentes/MolduraDeGrafico';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { COBERTURA_INICIAL, listarCobertura, obterResumoDeCobertura } from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type {
  ConsultaDeCobertura,
  CoberturaResumo,
  OrdemDeCobertura,
  ResumoDeCobertura,
} from '../tipos/relacionamento';
import { formatarData } from './cadastro/formato';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';
import '../estilos/territorio.css';

/** As colunas, quais delas a API sabe ordenar, e quais são número (alinhadas à direita). */
const COLUNAS: { rotulo: string; ordem?: OrdemDeCobertura; numerica?: boolean }[] = [
  { rotulo: 'Cliente', ordem: 'Nome' },
  { rotulo: 'Carteira' },
  { rotulo: 'Responsável' },
  { rotulo: 'Último contato', ordem: 'UltimaInteracaoEm', numerica: true },
  { rotulo: 'Dias sem contato', numerica: true },
  { rotulo: 'Ciclo declarado', numerica: true },
];

const nº = (v: number) => v.toLocaleString('pt-BR');

/** Faixas de atraso oferecidas no filtro. Todas resolvidas no banco. */
const FAIXAS = [
  { valor: '', rotulo: 'Qualquer tempo' },
  { valor: '30', rotulo: 'Mais de 30 dias' },
  { valor: '60', rotulo: 'Mais de 60 dias' },
  { valor: '90', rotulo: 'Mais de 90 dias' },
  { valor: '180', rotulo: 'Mais de 180 dias' },
];

/**
 * As classes da curva ABC oferecidas no filtro.
 *
 * É `Cliente.Classe` — a classe apurada do faturamento —, e não a classe do
 * vínculo (`ClienteCarteira.Classe`), que segue entrando como C por assunção
 * na maioria dos casos e por isso não sustenta segmentação nenhuma.
 */
const CLASSES = [
  { valor: '', rotulo: 'Qualquer classe' },
  { valor: 'A', rotulo: 'Classe A' },
  { valor: 'B', rotulo: 'Classe B' },
  { valor: 'C', rotulo: 'Classe C' },
  { valor: 'D', rotulo: 'Classe D' },
];

/**
 * As quatro faixas de tempo sem contato, na ordem em que a barra as empilha.
 *
 * As cores são as mesmas do donut "Status da cobertura" da Visão 360 — verde,
 * âmbar, vermelho, cinza — para que a mesma faixa tenha a mesma cor nas duas
 * telas. O cinza do "nunca contatado" é de propósito o único sem temperatura:
 * ele não é um atraso pior, é a ausência de qualquer registro.
 *
 * `medir` deriva cada faixa dos acumulados que a API devolve, que são
 * cumulativos (`comContatoEm90Dias` inclui os de 30). Subtrair aqui é o que
 * torna as fatias exclusivas e a soma igual ao total de clientes.
 */
const FAIXAS_DE_CONTATO: { nome: string; cor: string; medir: (c: ResumoDeCobertura) => number }[] = [
  { nome: 'Em dia (até 30 dias)', cor: '#367C2B', medir: (c) => c.comContatoEm30Dias },
  { nome: 'Aviso (31 a 90 dias)', cor: '#C1660A', medir: (c) => c.comContatoEm90Dias - c.comContatoEm30Dias },
  {
    nome: 'Atraso (mais de 90 dias)',
    cor: '#DC2626',
    medir: (c) => c.clientes - c.comContatoEm90Dias - c.nuncaContatados,
  },
  { nome: 'Nunca contatado', cor: '#9CA3AF', medir: (c) => c.nuncaContatados },
];

/** Quantas carteiras entram no gráfico antes de a barra virar um traço. */
const CARTEIRAS_NO_GRAFICO = 12;

export function CoberturaCarteira() {
  const { contexto } = useContextoDeAcesso();
  const [consulta, setConsulta] = useState<ConsultaDeCobertura>(COBERTURA_INICIAL);

  const resumo = useRecurso(
    (sinal) => obterResumoDeCobertura(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const lista = useRecurso(
    (sinal) => listarCobertura(contexto, consulta, sinal),
    [contexto.empresa, contexto.usuario, JSON.stringify(consulta)],
  );

  const carteiras = useMemo(() => resumo.dados?.itens ?? [], [resumo.dados]);

  /** As maiores primeiro: são elas que movem o número consolidado. */
  const carteirasOrdenadas = useMemo(
    () => [...carteiras].sort((a, b) => b.clientes - a.clientes),
    [carteiras],
  );
  const carteirasNoGrafico = carteirasOrdenadas.slice(0, CARTEIRAS_NO_GRAFICO);

  const totais = useMemo(() => {
    const clientes = carteiras.reduce((s, c) => s + c.clientes, 0);
    return {
      carteiras: carteiras.length,
      clientes,
      em30: carteiras.reduce((s, c) => s + c.comContatoEm30Dias, 0),
      em90: carteiras.reduce((s, c) => s + c.comContatoEm90Dias, 0),
      nunca: carteiras.reduce((s, c) => s + c.nuncaContatados, 0),
    };
  }, [carteiras]);

  const temFiltro = useMemo(
    () =>
      consulta.somenteSemContato ||
      consulta.diasSemContato !== '' ||
      consulta.classe !== '' ||
      consulta.ordenarPor !== COBERTURA_INICIAL.ordenarPor ||
      consulta.descendente,
    [consulta],
  );

  function trocarOrdem(campo: OrdemDeCobertura) {
    setConsulta((c) =>
      c.ordenarPor === campo
        ? { ...c, descendente: !c.descendente, pagina: 1 }
        : { ...c, ordenarPor: campo, descendente: false, pagina: 1 },
    );
  }

  function limparFiltros() {
    setConsulta({ ...COBERTURA_INICIAL, tamanho: consulta.tamanho });
  }

  const pagina = lista.dados;

  const semCarteira = resumo.carregando ? undefined : 'Sem carteira ao alcance deste contexto.';
  const pct = (parte: number) => (totais.clientes > 0 ? `${Math.round((100 * parte) / totais.clientes)}% dos vínculos` : null);

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Cobertura de Carteira</h1>
          <p className="page-subtitle">
            Quem está há tempo demais sem contato, carteira a carteira, na filial do cabeçalho.
            <InfoTooltip
              rotulo="De onde vem o último contato"
              texto="A data do último contato vem do histórico inteiro do Vórtice, pela regra da BI de carteiras (53 resultados que contam como contato, em qualquer canal), apurada todo dia pela rotina das carteiras — e só anda para a frente."
            />
          </p>
        </div>
        <p className="dash-atualizado">
          {lista.procedencia ? <DadosAtualizadosEm procedencia={lista.procedencia} /> : 'Lendo a cobertura…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              resumo.recarregar();
              lista.recarregar();
            }}
            disabled={resumo.carregando || lista.carregando}
            data-carregando={resumo.carregando || lista.carregando ? 'true' : 'false'}
            aria-label="Reler a cobertura"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* A BARRA DOS INDICADORES: os três filtros da lista, todos resolvidos no banco. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="sem-contato-ha">
            <span className="dash-filtro-icone" aria-hidden="true">
              <CalendarClock size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Sem contato há</span>
              <select
                value={consulta.diasSemContato}
                onChange={(e) => setConsulta((c) => ({ ...c, diasSemContato: e.target.value, pagina: 1 }))}
              >
                {FAIXAS.map((f) => (
                  <option key={f.valor} value={f.valor}>
                    {f.rotulo}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <label className="dash-filtro" data-bloco="classe">
            <span className="dash-filtro-icone" aria-hidden="true">
              <Layers size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                Classe
                <InfoTooltip
                  texto="Curva ABC apurada do faturamento (Cliente.Classe) — não a classe do vínculo importada do Vórtice."
                  rotulo="Qual classe é esta"
                />
              </span>
              <select value={consulta.classe} onChange={(e) => setConsulta((c) => ({ ...c, classe: e.target.value, pagina: 1 }))}>
                {CLASSES.map((cl) => (
                  <option key={cl.valor} value={cl.valor}>
                    {cl.rotulo}
                  </option>
                ))}
              </select>
            </span>
          </label>

          <div className="dash-filtros-acao">
            <label className="dash-caixa">
              <input
                type="checkbox"
                checked={consulta.somenteSemContato}
                onChange={(e) => setConsulta((c) => ({ ...c, somenteSemContato: e.target.checked, pagina: 1 }))}
              />
              Só quem nunca foi contatado
            </label>
          </div>

          {temFiltro && (
            <div className="dash-filtros-acao">
              <button type="button" className="dash-mais-filtros" onClick={limparFiltros}>
                Limpar filtros
              </button>
            </div>
          )}
        </div>
      </div>

      <AvisoDeProcedencia procedencia={lista.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="Carteiras"
          icone={BriefcaseBusiness}
          tom="neutro"
          valor={resumo.dados ? nº(totais.carteiras) : null}
          carregando={resumo.carregando}
          unidade="carteiras"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? 'com cliente vinculado' : null}
          sobre="As carteiras desta filial com pelo menos um cliente vinculado."
        />
        <CartaoDeDecisao
          rotulo="Clientes carteirizados"
          icone={Users}
          tom="mercado"
          valor={resumo.dados ? nº(totais.clientes) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? 'cliente × carteira' : null}
          sobre="Vínculos cliente × carteira: o mesmo cliente conta em cada carteira em que está."
        />
        <CartaoDeDecisao
          rotulo="Contato em 30 dias"
          icone={CalendarCheck}
          tom="demanda"
          valor={resumo.dados ? nº(totais.em30) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? pct(totais.em30) : null}
          sobre="Último contato (regra da BI de carteiras do Vórtice) nos últimos 30 dias."
        />
        <CartaoDeDecisao
          rotulo="Contato em 90 dias"
          icone={CalendarClock}
          tom="captura"
          valor={resumo.dados ? nº(totais.em90) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? pct(totais.em90) : null}
          sobre="Último contato (regra da BI de carteiras do Vórtice) nos últimos 90 dias — inclui os de 30."
        />
        <CartaoDeDecisao
          rotulo="Nunca contatados"
          icone={UserX}
          tom="oportunidade"
          valor={resumo.dados ? nº(totais.nunca) : null}
          carregando={resumo.carregando}
          unidade="vínculos"
          motivoSemDado={semCarteira}
          variacao={resumo.dados ? pct(totais.nunca) : null}
          sobre="Sem contato no histórico do Vórtice, pela regra da BI de carteiras."
        />
      </div>

      {resumo.erro && <BlocoErro erro={resumo.erro} aoTentarDeNovo={resumo.recarregar} />}

      <MetricasSemDado metricas={resumo.dados?.metricasSemDado} />

      {/*
        O MAPA SAIU DAQUI, E NO LUGAR DELE ENTROU O DADO QUE EXISTE.

        Ele ficou meses como moldura vazia com o motivo escrito — os pinos eram
        de Mato Grosso, e as treze filiais estão no interior de São Paulo. A
        moldura vazia se defendia enquanto era o único jeito de não perder um
        elemento da tela; ela deixou de se defender quando ocupava uma tela
        inteira de altura sem responder nada.

        O que ocupa o espaço agora responde a pergunta que a gerência faz aqui:
        QUAL CARTEIRA ESTÁ DESCOBERTA. Mesmo dado do resumo por carteira, lido
        como barra em vez de sete colunas de número. O motivo do mapa não sumiu:
        virou a nota do fim da tela.
      */}
      <section className="dash-secao" data-bloco="secao-carteiras">
        <TituloDaSecao
          titulo="Qual carteira está descoberta"
          subtitulo="A fatia de cada faixa de tempo sem contato, carteira a carteira."
          metodologia="As faixas são exclusivas: em dia (até 30 dias), aviso (31 a 90), atraso (mais de 90) e nunca contatado — a soma é o total de vínculos. As cores são as mesmas da cobertura da Visão 360."
        />

        <PainelDoMomento
          titulo="Cobertura por carteira"
          data-bloco="cobertura-por-carteira"
          subtitulo={
            carteiras.length > 0
              ? `${carteirasNoGrafico.length} maiores de ${carteiras.length} carteiras · fatia de cada faixa de tempo sem contato`
              : 'distribuição de cada carteira por faixa de tempo sem contato'
          }
          dica="As maiores carteiras primeiro: são elas que movem o número consolidado. O número exato de todas as carteiras está em “Cobertura por carteira, em número”, no fim da tela."
        >
          {resumo.carregando && <BlocoCarregando oQue="a cobertura por carteira" />}

          {!resumo.carregando && carteirasNoGrafico.length > 0 && (
            <>
              <MolduraDeGrafico altura={Math.max(220, carteirasNoGrafico.length * 26 + 40)}>
                {(largura, altura) => (
                  <GraficoBarrasEmpilhadas
                    itens={carteirasNoGrafico.map((c) => c.carteiraNome)}
                    faixas={FAIXAS_DE_CONTATO.map((f) => ({
                      nome: f.nome,
                      cor: f.cor,
                      valores: carteirasNoGrafico.map(f.medir),
                    }))}
                    largura={largura}
                    altura={altura}
                    proporcional
                  />
                )}
              </MolduraDeGrafico>
              <div className="cad-legenda-faixas">
                {FAIXAS_DE_CONTATO.map((f) => (
                  <span key={f.nome}>
                    <i style={{ background: f.cor }} />
                    {f.nome} <strong>{somar(carteiras, f.medir).toLocaleString('pt-BR')}</strong>
                  </span>
                ))}
              </div>
            </>
          )}
        </PainelDoMomento>
      </section>

      <section className="dash-secao" data-bloco="secao-clientes">
        <TituloDaSecao
          titulo="Quem contatar primeiro"
          subtitulo={
            lista.recarregando
              ? 'Atualizando…'
              : `${(pagina?.total ?? 0).toLocaleString('pt-BR')} vínculos · quem está há mais tempo sem contato primeiro`
          }
          metodologia="A ordem é quem está há mais tempo sem contato — dado real, calculado do fato —, e o nunca contatado vem antes de todos. A classe do vínculo importada do Vórtice não ordena nada: ela entra como C por assunção na maioria dos casos."
        />

        <PainelDoMomento
          titulo="Clientes por tempo sem contato"
          data-bloco="clientes-sem-contato"
          subtitulo={`Ordenados por ${consulta.ordenarPor === 'Nome' ? 'nome' : 'último contato'}${consulta.descendente ? ', do maior para o menor' : ''}.`}
          dica="Fora do ciclo é a carteira que declara de quantos em quantos dias o cliente deve ser visitado, e o último contato passou desse prazo. Sem cadência declarada, não se afirma que está em dia nem fora."
        >
          {lista.carregando && <BlocoCarregando oQue="a cobertura da carteira" />}
          {lista.erro && <BlocoErro erro={lista.erro} aoTentarDeNovo={lista.recarregar} />}

          {pagina && !lista.erro && pagina.itens.length === 0 && (
            <BlocoVazio
              titulo={temFiltro ? 'Nenhum cliente com esses filtros' : 'Esta filial não tem carteira carregada'}
              texto={
                temFiltro
                  ? 'Nenhum vínculo desta filial cai nesta faixa de tempo sem contato.'
                  : 'A carteirização ativa tem 8.537 vínculos nas treze filiais. Confira a filial escolhida no cabeçalho.'
              }
              acao={
                temFiltro ? (
                  <button type="button" className="btn btn-secondary" onClick={limparFiltros}>
                    Limpar filtros
                  </button>
                ) : undefined
              }
            />
          )}

          {pagina && pagina.itens.length > 0 && (
            <>
              <div className="mom-tabela-rolagem cad-so-largo">
                <table className="mom-tabela">
                  <caption className="cad-so-leitor">
                    Cobertura da filial {contexto.empresa}, ordenada por {consulta.ordenarPor}
                  </caption>
                  <thead>
                    <tr>
                      {COLUNAS.map((coluna) => (
                        <th
                          key={coluna.rotulo}
                          scope="col"
                          className={coluna.numerica ? 'mom-num' : undefined}
                          aria-sort={ariaOrdem(coluna.ordem, consulta)}
                        >
                          {coluna.ordem ? (
                            <button type="button" className="cad-th-ordenar" onClick={() => trocarOrdem(coluna.ordem!)}>
                              {coluna.rotulo}
                              <span aria-hidden="true">{seta(coluna.ordem, consulta)}</span>
                            </button>
                          ) : (
                            coluna.rotulo
                          )}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {pagina.itens.map((linha) => (
                      <tr key={`${linha.clienteChave}-${linha.carteiraChave}`}>
                        <th scope="row">
                          <Link to={`/clientes/${linha.clienteChave}`} className="cad-link-forte">
                            {linha.clienteNome}
                          </Link>
                        </th>
                        <td>
                          <div>{linha.carteiraNome}</div>
                          <div className="cad-sub">{linha.linhaDeNegocioNome}</div>
                        </td>
                        <td>{linha.responsavelNome}</td>
                        {/* A AUSÊNCIA É DITA UMA VEZ, e não três.

                            Estas três colunas repetiam "nunca", "sem contato
                            registrado" e "sem cadência declarada" em vermelho na
                            mesma linha, e a página inteira — a ordem põe os nunca
                            contatados primeiro — virava um bloco de texto igual.
                            O aviso fica na primeira coluna, onde a informação
                            nasce; as outras duas mostram o travessão de dado
                            ausente, com o motivo na dica (issue 167: nenhum `title=`). */}
                        <td className="mom-num">
                          {linha.ultimaInteracaoEm ? (
                            formatarData(linha.ultimaInteracaoEm)
                          ) : (
                            <span className="cad-alerta">nunca contatado</span>
                          )}
                        </td>
                        <td className="mom-num">
                          {linha.diasSemContato === null ? (
                            <ValorAusente motivo="Sem contato registrado: não há de quando contar." oQue="os dias sem contato" />
                          ) : (
                            <span className={linha.diasSemContato > 90 ? 'cad-alerta' : undefined}>
                              {linha.diasSemContato.toLocaleString('pt-BR')}
                            </span>
                          )}
                        </td>
                        <td className="mom-num">
                          {/* FORA DO CICLO É NULO, E NÃO FALSO, quando não há
                              cadência declarada: falso diria "está em dia", e não
                              é isso que se sabe. */}
                          {linha.diasCicloContato === null ? (
                            <ValorAusente
                              motivo="A carteira não declara de quantos em quantos dias este cliente deve ser visitado."
                              oQue="o ciclo declarado"
                            />
                          ) : (
                            <>
                              {linha.diasCicloContato} dias
                              {linha.estaForaDoCiclo && <div className="cad-alerta">fora do ciclo</div>}
                            </>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="cad-fichas cad-so-estreito">
                {pagina.itens.map((linha) => (
                  <FichaDeCobertura key={`${linha.clienteChave}-${linha.carteiraChave}`} linha={linha} />
                ))}
              </div>

              <BarraDePaginacao
                pagina={pagina}
                oQue="vínculos"
                aoTrocarPagina={(p) => setConsulta((c) => ({ ...c, pagina: p }))}
                aoTrocarTamanho={(t) => setConsulta((c) => ({ ...c, tamanho: t, pagina: 1 }))}
              />
            </>
          )}
        </PainelDoMomento>
      </section>

      {/* A MESMA COBERTURA POR CARTEIRA, EM NÚMERO.

          O gráfico acima responde "quem está descoberto" e para por aí. Quem
          precisa do número exato — as 42 carteiras, e não as 12 maiores — abre
          aqui. Fechado por padrão: aberto, esta tabela sozinha respondia por
          cerca de 2.000px da altura da tela. */}
      <BlocoRecolhivel
        titulo="Cobertura por carteira, em número"
        resumo={`as ${carteiras.length} carteiras desta filial, contadas no banco`}
      >
        {resumo.carregando && <BlocoCarregando oQue="o resumo por carteira" />}

        {carteiras.length > 0 && (
          <div className="mom-tabela-rolagem">
            <table className="mom-tabela">
              <caption className="cad-so-leitor">Cobertura por carteira da filial {contexto.empresa}</caption>
              <thead>
                <tr>
                  <th scope="col">Carteira</th>
                  <th scope="col">Responsável</th>
                  <th scope="col" className="mom-num">Clientes</th>
                  <th scope="col" className="mom-num">Em 30 dias</th>
                  <th scope="col" className="mom-num">Em 90 dias</th>
                  <th scope="col" className="mom-num">Nunca contatados</th>
                  <th scope="col" className="mom-num">Último contato</th>
                </tr>
              </thead>
              <tbody>
                {carteirasOrdenadas.map((c) => (
                  <tr key={c.carteiraChave}>
                    <th scope="row">
                      <div className="cad-link-forte">{c.carteiraNome}</div>
                      <div className="cad-sub">{c.linhaDeNegocioNome}</div>
                    </th>
                    <td>{c.responsavelNome}</td>
                    <td className="mom-num">{c.clientes.toLocaleString('pt-BR')}</td>
                    <td className="mom-num">
                      {c.comContatoEm30Dias.toLocaleString('pt-BR')}
                      <div className="cad-sub">{percentual(c.comContatoEm30Dias, c.clientes)}</div>
                    </td>
                    <td className="mom-num">
                      {c.comContatoEm90Dias.toLocaleString('pt-BR')}
                      <div className="cad-sub">{percentual(c.comContatoEm90Dias, c.clientes)}</div>
                    </td>
                    <td className="mom-num">
                      <span className={c.nuncaContatados > 0 ? 'cad-atencao' : undefined}>
                        {c.nuncaContatados.toLocaleString('pt-BR')}
                      </span>
                    </td>
                    <td className="mom-num">
                      {c.ultimoContatoEm ? formatarData(c.ultimoContatoEm) : <span className="cad-nada">nunca</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </BlocoRecolhivel>

      <BlocoRecolhivel titulo="O que esta tela deixou de afirmar" resumo="canal de contato e o mapa">
        <div className="cad-fichas">
          <LacunaConhecida
            metrica="Interações por canal (visita, ligação, WhatsApp)"
            motivo={
              'O contato usado nesta tela vem da regra da BI de carteiras do Vórtice (53 resultados ' +
              'que contam como contato), calculada todo dia pela rotina das carteiras — e ela conta ' +
              'qualquer canal. O Vórtice REGISTRA o canal: nos últimos 12 meses, os contatos se ' +
              'dividem em Visita (45%), Externo (18%), Telefone (14%) e WhatsApp (11%). O CRM, porém, ' +
              'ainda não carrega essa coluna — só o resultado, não o canal que o produziu —, e por ' +
              'isso não há como abrir esta lista por canal.'
            }
          />
          <LacunaConhecida
            metrica="Mapa da carteira"
            motivo={
              'O mapa ficava aqui e plotava pinos em Mato Grosso, Goiás e Bahia — geografia do ' +
              'protótipo, não desta operação: as treze filiais estão todas no interior de São ' +
              'Paulo. Falta uma fonte de coordenada por cliente: organizacao.Municipio guarda o ' +
              'município mas não latitude e longitude, e todo cliente segue com latitude 0. O ' +
              'território que existe está em Cobertura por Filial e Carteira.'
            }
          />
        </div>
      </BlocoRecolhivel>
    </PaginaDoPainel>
  );
}


/** A mesma linha, em ficha, para quando a tabela não cabe. */
function FichaDeCobertura({ linha }: { linha: CoberturaResumo }) {
  return (
    <div className="cad-ficha">
      <div className="cad-ficha-titulo">
        <Link to={`/clientes/${linha.clienteChave}`}>{linha.clienteNome}</Link>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Carteira</span>
        <span className="cad-ficha-valor">
          {linha.carteiraNome}
          <div className="cad-sub">{linha.linhaDeNegocioNome}</div>
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Responsável</span>
        <span className="cad-ficha-valor">{linha.responsavelNome}</span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Último contato</span>
        <span className="cad-ficha-valor">
          {linha.ultimaInteracaoEm ? formatarData(linha.ultimaInteracaoEm) : <span className="cad-alerta">nunca</span>}
        </span>
      </div>
      <div className="cad-ficha-linha">
        <span className="cad-ficha-rotulo">Dias sem contato</span>
        <span className="cad-ficha-valor">
          {linha.diasSemContato === null ? (
            <span className="cad-alerta">sem contato registrado</span>
          ) : (
            linha.diasSemContato.toLocaleString('pt-BR')
          )}
        </span>
      </div>
    </div>
  );
}

/** A fração, quando ela tem denominador. Zero cliente não vira "0%". */
function percentual(parte: number, todo: number): string {
  if (todo <= 0) return '—';
  return `${Math.round((parte / todo) * 100)}%`;
}

function ariaOrdem(campo: OrdemDeCobertura | undefined, consulta: ConsultaDeCobertura) {
  if (!campo || consulta.ordenarPor !== campo) return undefined;
  return consulta.descendente ? ('descending' as const) : ('ascending' as const);
}

function seta(campo: OrdemDeCobertura | undefined, consulta: ConsultaDeCobertura) {
  if (!campo || consulta.ordenarPor !== campo) return '';
  return consulta.descendente ? '▾' : '▴';
}

/** A soma de uma faixa em todas as carteiras. */
function somar(carteiras: ResumoDeCobertura[], medir: (c: ResumoDeCobertura) => number): number {
  return carteiras.reduce((total, c) => total + medir(c), 0);
}
