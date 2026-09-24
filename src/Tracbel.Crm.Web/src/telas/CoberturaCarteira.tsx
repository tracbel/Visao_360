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
 * AS TRÊS COISAS QUE MUDARAM, e as três são a mesma decisão:
 *
 * 1. **A ordenação por classe A/B saiu.** É a métrica mais visível do protótipo
 *    e ela não se sustenta: **59 de 49.109 vínculos** têm classe lida da
 *    origem; os outros 49.050 entraram como `C` por assunção, porque
 *    `IVS_Pes.Potencial` é `varchar(3)` sem catálogo e guarda `64`, `43`, `22`
 *    e `85`, que são resquício de outro domínio (documento 25, §5.1). Ordenar
 *    por um campo assumido em 99,9% dos casos é ordenar por nada.
 *
 *    **A ordem passa a ser quem está há mais tempo sem contato**, que é dado
 *    real e calculado do fato: `UltimaInteracaoEm` não foi copiada da origem —
 *    ela é apurada das interações que efetivamente entraram. Copiar a coluna do
 *    legado teria alcançado 14.496 vínculos; calcular alcançou **39.589**
 *    (documento 25, §6.1).
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
 */

import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { GraficoBarrasEmpilhadas } from '../componentes/GraficoBarrasEmpilhadas';
import { MolduraDeGrafico } from '../componentes/MolduraDeGrafico';
import { BarraDePaginacao } from '../componentes/cadastro/BarraDePaginacao';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
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

/** As colunas, e quais delas a API sabe ordenar. */
const COLUNAS: { rotulo: string; ordem?: OrdemDeCobertura }[] = [
  { rotulo: 'Cliente', ordem: 'Nome' },
  { rotulo: 'Carteira' },
  { rotulo: 'Responsável' },
  { rotulo: 'Último contato', ordem: 'UltimaInteracaoEm' },
  { rotulo: 'Dias sem contato' },
  { rotulo: 'Ciclo declarado' },
];

/** Faixas de atraso oferecidas no filtro. Todas resolvidas no banco. */
const FAIXAS = [
  { valor: '', rotulo: 'Qualquer tempo' },
  { valor: '30', rotulo: 'Mais de 30 dias' },
  { valor: '60', rotulo: 'Mais de 60 dias' },
  { valor: '90', rotulo: 'Mais de 90 dias' },
  { valor: '180', rotulo: 'Mais de 180 dias' },
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

  const carteiras = resumo.dados?.itens ?? [];

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

  const indicadores: Indicador[] = [
    {
      rotulo: 'Carteiras',
      valor: resumo.dados ? totais.carteiras : null,
      deOnde: 'carteiras desta filial com cliente vinculado',
      semDado: 'sem carteira ao alcance deste contexto',
    },
    {
      rotulo: 'Clientes carteirizados',
      valor: resumo.dados ? totais.clientes : null,
      deOnde: 'vínculos cliente × carteira; o mesmo cliente conta em cada carteira',
      semDado: '—',
    },
    {
      rotulo: 'Contato em 30 dias',
      valor: resumo.dados ? totais.em30 : null,
      tom: 'bom',
      deOnde: 'última interação registrada nos últimos 30 dias',
      semDado: '—',
    },
    {
      rotulo: 'Contato em 90 dias',
      valor: resumo.dados ? totais.em90 : null,
      deOnde: 'última interação registrada nos últimos 90 dias',
      semDado: '—',
    },
    {
      rotulo: 'Nunca contatados',
      valor: resumo.dados ? totais.nunca : null,
      tom: 'atencao',
      deOnde: 'sem nenhuma interação no recorte carregado',
      semDado: '—',
    },
  ];

  const temFiltro = useMemo(
    () =>
      consulta.somenteSemContato ||
      consulta.diasSemContato !== '' ||
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

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Cobertura de Carteira</h1>
          <p className="page-subtitle">
            <code>comercial.ClienteCarteira</code> — quem está há tempo demais sem contato. A data do
            último contato é <strong>calculada das interações que entraram</strong>, não copiada da
            origem.
          </p>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={lista.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={resumo.carregando} />

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
      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Cobertura por carteira</div>
            <div className="card-subtitle">
              {carteiras.length > 0
                ? `${carteirasNoGrafico.length} maiores de ${carteiras.length} carteiras · fatia de cada faixa de tempo sem contato`
                : 'distribuição de cada carteira por faixa de tempo sem contato'}
            </div>
          </div>
          <SeloProcedencia procedencia={resumo.procedencia} />
        </div>

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
      </div>

      <div className="cad-barra">
        <label className="cad-filtro">
          Sem contato há
          <select
            value={consulta.diasSemContato}
            onChange={(e) =>
              setConsulta((c) => ({ ...c, diasSemContato: e.target.value, pagina: 1 }))
            }
          >
            {FAIXAS.map((f) => (
              <option key={f.valor} value={f.valor}>
                {f.rotulo}
              </option>
            ))}
          </select>
        </label>

        <label className="cad-filtro cad-filtro-caixa">
          <input
            type="checkbox"
            checked={consulta.somenteSemContato}
            onChange={(e) =>
              setConsulta((c) => ({ ...c, somenteSemContato: e.target.checked, pagina: 1 }))
            }
          />
          Só quem nunca foi contatado
        </label>

        {temFiltro && (
          <button type="button" className="btn btn-secondary btn-sm" onClick={limparFiltros}>
            Limpar filtros
          </button>
        )}
      </div>

      <div className="card cad-cartao">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Clientes por tempo sem contato</div>
            <div className="card-subtitle">
              {lista.recarregando
                ? 'Atualizando…'
                : `${(pagina?.total ?? 0).toLocaleString('pt-BR')} vínculos · quem está há mais tempo sem contato primeiro`}
            </div>
          </div>
          <SeloProcedencia procedencia={lista.procedencia} />
        </div>

        {lista.carregando && <BlocoCarregando oQue="a cobertura da carteira" />}
        {lista.erro && <BlocoErro erro={lista.erro} aoTentarDeNovo={lista.recarregar} />}

        {pagina && !lista.erro && pagina.itens.length === 0 && (
          <BlocoVazio
            titulo={temFiltro ? 'Nenhum cliente com esses filtros' : 'Esta filial não tem carteira carregada'}
            texto={
              temFiltro
                ? 'Nenhum vínculo desta filial cai nesta faixa de tempo sem contato.'
                : 'A carteirização de 2026 trouxe 49.109 vínculos para as treze filiais. Confira a filial escolhida no cabeçalho.'
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
            <div className="cad-tabela-wrap cad-so-largo">
              <table className="cad-tabela">
                <caption className="cad-so-leitor">
                  Cobertura da filial {contexto.empresa}, ordenada por {consulta.ordenarPor}
                </caption>
                <thead>
                  <tr>
                    {COLUNAS.map((coluna) => (
                      <th key={coluna.rotulo} scope="col" aria-sort={ariaOrdem(coluna.ordem, consulta)}>
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
                      <td>
                        <Link to={`/clientes/${linha.clienteChave}`} className="cad-link-forte">
                          {linha.clienteNome}
                        </Link>
                      </td>
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
                          ausente, com o motivo no `title`. */}
                      <td className="cad-mono">
                        {linha.ultimaInteracaoEm ? (
                          formatarData(linha.ultimaInteracaoEm)
                        ) : (
                          <span className="cad-alerta">nunca contatado</span>
                        )}
                      </td>
                      <td className="cad-mono">
                        {linha.diasSemContato === null ? (
                          <span className="cad-nada" title="Sem contato registrado: não há de quando contar.">
                            —
                          </span>
                        ) : (
                          <span className={linha.diasSemContato > 90 ? 'cad-alerta' : undefined}>
                            {linha.diasSemContato.toLocaleString('pt-BR')}
                          </span>
                        )}
                      </td>
                      <td className="cad-mono">
                        {/* FORA DO CICLO É NULO, E NÃO FALSO, quando não há
                            cadência declarada: falso diria "está em dia", e não
                            é isso que se sabe. */}
                        {linha.diasCicloContato === null ? (
                          <span className="cad-nada" title="A carteira não declara de quantos em quantos dias este cliente deve ser visitado.">
                            —
                          </span>
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
      </div>

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
          <div className="cad-tabela-wrap">
            <table className="cad-tabela">
              <caption className="cad-so-leitor">Cobertura por carteira da filial {contexto.empresa}</caption>
              <thead>
                <tr>
                  <th scope="col">Carteira</th>
                  <th scope="col">Responsável</th>
                  <th scope="col">Clientes</th>
                  <th scope="col">Em 30 dias</th>
                  <th scope="col">Em 90 dias</th>
                  <th scope="col">Nunca contatados</th>
                  <th scope="col">Último contato</th>
                </tr>
              </thead>
              <tbody>
                {carteirasOrdenadas.map((c) => (
                    <tr key={c.carteiraChave}>
                      <td>
                        <div className="cad-link-forte">{c.carteiraNome}</div>
                        <div className="cad-sub">{c.linhaDeNegocioNome}</div>
                      </td>
                      <td>{c.responsavelNome}</td>
                      <td className="cad-mono">{c.clientes.toLocaleString('pt-BR')}</td>
                      <td className="cad-mono">
                        {c.comContatoEm30Dias.toLocaleString('pt-BR')}
                        <div className="cad-sub">{percentual(c.comContatoEm30Dias, c.clientes)}</div>
                      </td>
                      <td className="cad-mono">
                        {c.comContatoEm90Dias.toLocaleString('pt-BR')}
                        <div className="cad-sub">{percentual(c.comContatoEm90Dias, c.clientes)}</div>
                      </td>
                      <td className="cad-mono">
                        <span className={c.nuncaContatados > 0 ? 'cad-atencao' : undefined}>
                          {c.nuncaContatados.toLocaleString('pt-BR')}
                        </span>
                      </td>
                      <td className="cad-mono">
                        {c.ultimoContatoEm ? formatarData(c.ultimoContatoEm) : <span className="cad-nada">nunca</span>}
                      </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </BlocoRecolhivel>

      <BlocoRecolhivel
        titulo="O que esta tela deixou de afirmar"
        resumo="classe de cliente, canal de contato e o mapa"
      >
        <div className="cad-fichas">
          <LacunaConhecida
            metrica="Segmentação por classe de cliente (A, B, C, D)"
            motivo={
              'A Cobertura do protótipo ordenava por classe A e B. Na carga de 2026, 59 de 49.109 ' +
              'vínculos têm classe LIDA da origem; os outros 49.050 entraram como C por assunção — ' +
              'IVS_Pes.Potencial é varchar(3) sem catálogo e guarda 64, 43, 22 e 85, que são ' +
              'resquício de outro domínio. Ordenar por um campo assumido em 99,9% dos casos é ' +
              'ordenar por nada, então a coluna de classe saiu e a ordem passou a ser o tempo sem ' +
              'contato, que é dado real e verificável.'
            }
          />
          <LacunaConhecida
            metrica="Interações por canal (visita, ligação, WhatsApp)"
            motivo={
              'As 178 ações migradas entraram todas como categoria Interna. IV_Acao.Classe é ' +
              'char(1) com sete valores e sem tabela de domínio no banco — o significado mora ' +
              'dentro do cliente Gupta. Classificar por palavra do nome foi considerado e ' +
              'recusado: "Monitorar Cliente", com 25.230 tarefas, pode ser visita, ligação ou ' +
              // A citação "(documento 25, §3.4)" saiu do texto: o símbolo e o número do documento não
              // dizem nada a quem lê a tela, e a referência fica aqui.
              'WhatsApp, e o dado não diz qual. Destrava com uma triagem de 178 linhas pelo negócio.'
            }
          />
          <LacunaConhecida
            metrica="Mapa da carteira"
            motivo={
              'O mapa ficava aqui e plotava pinos em Mato Grosso, Goiás e Bahia — geografia do ' +
              'protótipo, não desta operação: as treze filiais estão todas no interior de São ' +
              'Paulo. Falta uma fonte de coordenada por cliente: organizacao.Municipio guarda o ' +
              'município mas não latitude e longitude, e as interações têm coordenada em 29.491 ' +
              'de 121.983 registros (24%) sem rota que as agregue por cliente. O território que ' +
              'existe — 532 vínculos carteira × município — está em Cobertura por Filial e ' +
              'Carteira.'
            }
          />
        </div>
      </BlocoRecolhivel>
    </>
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
