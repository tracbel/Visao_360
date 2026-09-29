/**
 * Funil de Vendas — os seis estágios do Vórtice, de Lead a Faturamento.
 *
 * ---------------------------------------------------------------------------
 * 27/09/2026 — O FUNIL PASSOU A SER O ESTÁGIO, E NÃO A FASE (documento 52).
 *
 * A tela desenhava a FASE do fluxo (`processo.Processo`, o BPM), e a fase não é
 * o funil: ela diz em que caixa do fluxo o processo está, e não até onde ele
 * chegou. O funil agora vem de `/api/v1/relatorios/funil-por-estagio`, que conta
 * `processo.EstagioDoProcesso` — um processo 31/41/50 do Vórtice por estágio
 * alcançado, pelo resultado do histórico, como o BI. A fase continua no
 * Pipeline, que é onde ela responde alguma coisa.
 *
 * AS DECISÕES DO RICARDO (27/09/2026), e onde cada uma aparece:
 *
 * - **Lead e Qualificado são cumulativos**: quem chegou à Cobertura também
 *   conta como Lead e Qualificado. Por isso o funil só estreita.
 * - **A tela abre na COORTE** — os processos abertos no período e até onde
 *   chegaram —, com a chave para o **FLUXO** — as etapas alcançadas no
 *   período. Os dois números são diferentes de propósito, e a tela diz qual
 *   está mostrando.
 * - **O período padrão é o ano fiscal até o último mês fechado**, calculado no
 *   servidor.
 *
 * A FORMA É A DO PROTÓTIPO: o funil em SVG com a legenda de conversão ao lado
 * (`GraficoFunil`, porte de `mountFunil()`). Sem dado, o funil não vira zero:
 * cada estágio mostra "—" com o motivo verdadeiro na dica — a rotina que traz o
 * funil ainda não rodou, falhou, ou não trouxe esta filial.
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NO DESENHO DOS INDICADORES GEOGRÁFICOS (pedido do Ricardo: "vamos deixar todas as telas no padrão de
 * indicadores geográficos"). As mesmas peças, na mesma ordem de leitura: o cabeçalho com a hora da leitura e o botão de
 * reler; a barra de filtros (o período e o formulário da venda perdida); os quatro números de decisão (`CartaoDeDecisao`);
 * e duas seções com título — o funil do período e as perdas do período —, em `PainelDoMomento`. A chave coorte × fluxo
 * mora à direita do título do funil, no alternador dos Indicadores; a nota que explicava a leitura foi para a dica.
 * NENHUM NÚMERO, REGRA OU TEXTO DE REGRA MUDOU.
 *
 * ---------------------------------------------------------------------------
 * 29/09/2026 — NA MAQUETE DO RICARDO (`docs/prototipo/capturas-referencia/funil-de-vendas-maquete-2026-09-29.png`: "quero idêntica a essa;
 * não vai inventar nada; não tirará nenhuma informação da tela"). O mesmo conteúdo, no desenho dela:
 *
 * - os quatro cartões mais tingidos, com a onda decorativa no canto;
 * - o funil em perspectiva, com o nome de cada estágio numa linha-guia (`GraficoFunil`);
 * - a conversão entre estágios num painel próprio, ao lado, com a barra da conversão, e a chave coorte × fluxo em cima
 *   dele;
 * - a tabela dos seis estágios num cartão próprio, embaixo, com o ponto da cor de cada estágio;
 * - as perdas do período numa faixa laranja, com "o que a distribuição de perdas não diz" à direita;
 * - as abas das vendas perdidas na linha do título, e o preço com o denominador na mesma linha.
 *
 * O que a maquete escreve diferente do CRM ficou como o CRM escreve — "Coorte", e não "Conta"; "Formulário da venda
 * perdida". O que não está na maquete e estava na tela continua: as dicas, o aviso de procedência, as métricas sem dado
 * e o bloco "O que este relatório não pode afirmar".
 */

import { CalendarDays, CircleAlert, CircleCheck, FileText, Hourglass, RefreshCw, Scale, Users } from 'lucide-react';
import { useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { AvisoDeProcedencia, DadosAtualizadosEm } from '../componentes/cadastro/SeloProcedencia';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { ConversaoDoFunil, corDaFaixa, GraficoFunil, type FaixaDoFunil } from '../componentes/GraficoFunil';
import { InfoTooltip } from '../componentes/InfoTooltip';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { TituloDaSecao } from '../componentes/territorio/TituloDaSecao';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterFunilPorEstagio, obterVendasPerdidas } from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type { BaseDoFunil, EstagioNoFunil, FatiaDeVendaPerdida } from '../tipos/relacionamento';
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/territorio.css';
import '../estilos/funil.css';

/** Os seis estágios, para a tela manter a forma quando o funil não tem dado. */
const ESTAGIOS_DO_FUNIL = [
  { estagio: 'Lead', nome: 'Lead' },
  { estagio: 'Qualificado', nome: 'Qualificado' },
  { estagio: 'Cobertura', nome: 'Cobertura' },
  { estagio: 'Negociacao', nome: 'Negociação' },
  { estagio: 'Pedido', nome: 'Pedido' },
  { estagio: 'Faturamento', nome: 'Faturamento' },
] as const;

/**
 * Os formulários de venda perdida do Vórtice que entram (documento 52 §4), para o filtro. O `_MANITO` (Colorado) fica
 * de fora, como na rotina.
 */
const FORMULARIOS = [
  { codigo: 'IV_Q_VENDA_PERDIDA_FY25', nome: 'FY25 (desde 07/2025)' },
  { codigo: 'IV_Q_VP_SEM_PARTICIPACAO', nome: 'Sem participação (desde 08/2025)' },
  { codigo: 'IV_Q_VENDA_PERDIDA_MAQIMP', nome: 'Máquinas e implementos (2024–2025)' },
  { codigo: 'IV_Q_VENDA_PERDIDA_PROD', nome: 'Produto (2022–2024)' },
  { codigo: 'IV_Q_VENDA_PERDIDA', nome: 'Antigo (2012–2023)' },
  { codigo: 'IV_Q_VENDA_PERDIDA_JDE', nome: 'JDE (2012–2016)' },
  { codigo: 'IV_Q_VENDA_PERDIDA_IMPLEM', nome: 'Implementos' },
  { codigo: 'IV_Q_VENDA_PERDIDA_IMPL', nome: 'Implementos, versão curta' },
] as const;

/** As duas abas do painel das perdas. */
type AbaDasPerdas = 'motivo' | 'concorrente';

const nº = (v: number) => v.toLocaleString('pt-BR');
const pct =(v: number | null) => (v === null ? null : `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`);

export function Funil() {
  const { contexto } = useContextoDeAcesso();
  const [base, setBase] = useState<BaseDoFunil>('abertura');
  /** O período escolhido; nulo é o padrão do servidor — o ano fiscal até o último mês fechado. */
  const [periodo, setPeriodo] = useState<{ de: string; ate: string } | null>(null);
  const [formulario, setFormulario] = useState('');
  const [abaDasPerdas, setAbaDasPerdas] = useState<AbaDasPerdas>('concorrente');

  const funil = useRecurso(
    (sinal) => obterFunilPorEstagio(contexto, { base, de: periodo?.de, ate: periodo?.ate }, sinal),
    [contexto.empresa, contexto.usuario, base, periodo?.de, periodo?.ate],
  );
  const vendas = useRecurso(
    (sinal) => obterVendasPerdidas(contexto, sinal, { de: periodo?.de, ate: periodo?.ate, formulario }),
    [contexto.empresa, contexto.usuario, periodo?.de, periodo?.ate, formulario],
  );

  const dados = funil.dados;
  const estagios = dados?.estagios ?? [];
  const temFunil = estagios.length > 0;
  const motivo =
    dados?.metricasSemDado.find((m) => m.metrica === 'funil' || m.metrica === 'funilNoPeriodo')?.motivo ??
    'O funil ainda não respondeu.';
  const lead = estagios[0];
  const faturamento = estagios[5];
  const periodoMostrado = periodo ?? (dados ? { de: dados.periodo.de, ate: dados.periodo.ate } : null);
  const leitura = base === 'abertura' ? 'Coorte' : 'Fluxo';

  const faixas: FaixaDoFunil[] = estagios.map((e, i) => ({
    chave: e.estagio,
    rotulo: e.nome,
    valor: e.processos,
    cor: corDaFaixa(i, estagios.length),
    texto: nº(e.processos),
  }));

  // SEM FUNIL, O NÚMERO É O TRAÇO COM O MOTIVO DO SERVIDOR; LENDO, O CARTÃO PULSA E NÃO AFIRMA MOTIVO NENHUM.
  const motivoDoCartao = funil.carregando ? undefined : motivo;
  const noPeriodo = dados?.periodo.texto ?? 'período';
  const diasParado = dados?.diasParaParado ?? 60;

  return (
    // A LARGURA É A DA COLUNA INTEIRA, como a Visão 360: o teto só volta acima de 2.100px de janela.
    <PaginaDoPainel className="dash-pagina-larga funil-maquete">
      <div className="page-header" data-bloco="cabecalho">
        <div>
          <h1 className="page-title">Funil de Vendas</h1>
          <p className="page-subtitle">
            Os processos do Vórtice por estágio — de Lead a Faturamento —, contados no banco dentro da filial do
            cabeçalho.
          </p>
        </div>
        <p className="dash-atualizado">
          {funil.procedencia ? <DadosAtualizadosEm procedencia={funil.procedencia} /> : 'Lendo o funil…'}
          <button
            type="button"
            className="dash-recarregar"
            onClick={() => {
              funil.recarregar();
              vendas.recarregar();
            }}
            disabled={funil.carregando || vendas.carregando}
            data-carregando={funil.carregando || vendas.carregando ? 'true' : 'false'}
            aria-label="Reler o funil"
          >
            <RefreshCw size={15} strokeWidth={2} aria-hidden="true" />
          </button>
        </p>
      </div>

      {/* O PERÍODO E O FORMULÁRIO, NA BARRA DOS INDICADORES. O período vale para o funil e para as perdas; o formulário, só
          para as perdas. Sem período escolhido, vale o padrão do servidor — o ano fiscal até o último mês fechado. */}
      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <label className="dash-filtro" data-bloco="de">
            <span className="dash-filtro-icone" aria-hidden="true">
              <CalendarDays size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">
                De
                <InfoTooltip
                  texto="O período padrão é o ano fiscal (novembro a outubro) até o último mês fechado, calculado no servidor. Trocar uma das datas troca o período do funil e das perdas."
                  rotulo="Como o período é contado"
                />
              </span>
              <input
                type="date"
                value={periodoMostrado?.de ?? ''}
                onChange={(e) => e.target.value && setPeriodo({ de: e.target.value, ate: periodoMostrado?.ate ?? e.target.value })}
              />
            </span>
          </label>

          <label className="dash-filtro" data-bloco="ate">
            <span className="dash-filtro-icone" aria-hidden="true">
              <CalendarDays size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Até</span>
              <input
                type="date"
                value={periodoMostrado?.ate ?? ''}
                onChange={(e) => e.target.value && setPeriodo({ de: periodoMostrado?.de ?? e.target.value, ate: e.target.value })}
              />
            </span>
          </label>

          <label className="dash-filtro" data-bloco="formulario">
            <span className="dash-filtro-icone" aria-hidden="true">
              <FileText size={17} strokeWidth={2} />
            </span>
            <span className="dash-filtro-corpo">
              <span className="dash-filtro-rotulo">Formulário da venda perdida</span>
              <select value={formulario} onChange={(e) => setFormulario(e.target.value)}>
                <option value="">Todos os formulários</option>
                {FORMULARIOS.map((f) => (
                  <option key={f.codigo} value={f.codigo}>
                    {f.nome}
                  </option>
                ))}
              </select>
            </span>
          </label>

          {periodo && (
            <div className="dash-filtros-acao">
              <button type="button" className="dash-mais-filtros" onClick={() => setPeriodo(null)}>
                Ano fiscal até o último mês fechado
              </button>
            </div>
          )}
        </div>
      </div>

      <AvisoDeProcedencia procedencia={funil.procedencia} />

      <div className="dash-kpis mv-kpis" data-bloco="kpis">
        <CartaoDeDecisao
          rotulo="Leads"
          icone={Users}
          tom="demanda"
          valor={lead ? nº(lead.processos) : null}
          carregando={funil.carregando}
          unidade="processos"
          motivoSemDado={motivoDoCartao}
          variacao={temFunil ? (base === 'abertura' ? 'abertos no período' : 'alcançados no período') : null}
          sobre={
            base === 'abertura'
              ? `Processos abertos em ${noPeriodo} que viraram lead — a coorte do período. Lead e Qualificado são cumulativos: quem chegou à Cobertura também conta aqui.`
              : `Leads alcançados em ${noPeriodo}, qualquer que seja a abertura — o fluxo do período.`
          }
        />
        <CartaoDeDecisao
          rotulo="Faturados"
          icone={CircleCheck}
          tom="captura"
          valor={faturamento ? nº(faturamento.processos) : null}
          carregando={funil.carregando}
          unidade="processos"
          motivoSemDado={motivoDoCartao}
          variacao={
            faturamento && faturamento.percentualSobreOLead !== null ? `${pct(faturamento.percentualSobreOLead)} dos leads` : temFunil ? 'sobre os leads' : null
          }
          sobre="Os processos que chegaram ao estágio Faturamento — o resultado registrado no histórico do Vórtice, como o BI, e não a nota do Protheus. O percentual é sobre os leads da mesma leitura."
        />
        <CartaoDeDecisao
          rotulo="Desfecho dos leads"
          icone={Scale}
          tom="mercado"
          valor={lead ? nº(lead.ganhos) : null}
          carregando={funil.carregando}
          unidade="ganhos"
          motivoSemDado={motivoDoCartao}
          variacao={lead ? `${nº(lead.perdidos)} perdidos · ${nº(lead.abertos)} abertos` : null}
          sobre="O desfecho, no Vórtice, dos processos contados no Lead: ganhos, perdidos e ainda abertos."
        />
        <CartaoDeDecisao
          rotulo={`Parados há mais de ${diasParado} dias`}
          icone={Hourglass}
          tom="oportunidade"
          valor={temFunil && dados?.paradosEmNegociacaoOuPedido != null ? nº(dados.paradosEmNegociacaoOuPedido) : null}
          carregando={funil.carregando}
          unidade="processos"
          motivoSemDado={motivoDoCartao}
          variacao={temFunil ? 'em Negociação ou Pedido, hoje' : null}
          sobre={`Em Negociação ou Pedido há mais de ${diasParado} dias, sem avançar nem encerrar — hoje, qualquer que seja o período escolhido.`}
        />
      </div>

      <MetricasSemDado metricas={dados?.metricasSemDado} />

      {/* ---------------------------------------------------------------- */}
      {/* O funil por estágio, com a forma do protótipo                    */}
      {/* ---------------------------------------------------------------- */}
      <section className="dash-secao" data-bloco="secao-funil">
        <TituloDaSecao
          titulo="O funil do período"
          subtitulo="Até onde os processos do Vórtice chegaram, estágio a estágio."
          metodologia="O estágio é o resultado do histórico do Vórtice, como o BI: seis estágios, uma linha por processo por estágio alcançado. A fase do fluxo (o BPM) não é o funil, e fica no Pipeline."
        />

        {/* O FUNIL E A CONVERSÃO LADO A LADO, como na maquete; a tabela dos seis estágios vem embaixo, num cartão só dela. */}
        <div className="funil-grade">
          <PainelDoMomento
            titulo="Funil por estágio"
            data-bloco="funil-por-estagio"
            area="funil-grafico"
            subtitulo={
              dados
                ? `${leitura} · ${dados.periodo.texto}${dados.periodo.ehOPadrao ? ' · ano fiscal até o último mês fechado' : ''}`
                : leitura
            }
            dica={
              <>
                <p>
                  <strong>Coorte:</strong> os processos abertos no período, e até onde cada um chegou — de cada 100 leads
                  que entraram, quantos viraram pedido.
                </p>
                <p>
                  <strong>Fluxo:</strong> as etapas alcançadas no período, qualquer que seja a abertura — quantos pedidos e
                  faturamentos saíram no período.
                </p>
                <p>
                  Lead e Qualificado são cumulativos: quem chegou à Cobertura também conta nos dois. O estágio é o resultado
                  do histórico do Vórtice, como o BI — a fase do fluxo não é o funil, e fica no Pipeline.
                </p>
              </>
            }
          >
            {funil.carregando && <BlocoCarregando oQue="o funil" />}
            {funil.erro && <BlocoErro erro={funil.erro} aoTentarDeNovo={funil.recarregar} />}

            {temFunil && (
              <div className="funil-grafico">
                <GraficoFunil faixas={faixas} />
              </div>
            )}

            {/* SEM FUNIL, O PAINEL DIZ POR QUÊ — com o motivo do servidor, o mesmo da nota de cima —, em vez de ficar em
                branco. O funil não vira zero: a conversão e a tabela ficam com o traço em cada estágio. */}
            {dados && !temFunil && !funil.carregando && !funil.erro && (
              <p className="funil-sem-grafico" role="note">
                {motivo}
              </p>
            )}
          </PainelDoMomento>

          <div className="funil-lado">
            {/* A CHAVE COORTE × FLUXO (decisão de 27/09/2026), em cima do painel da conversão, como na maquete. A coorte é o
                padrão; o fluxo é o que a Performance de CEN e os alertas usam. */}
            <div className="terr-alternador funil-chave" role="group" aria-label="Leitura do funil">
              <button type="button" aria-pressed={base === 'abertura'} onClick={() => setBase('abertura')}>
                Coorte
              </button>
              <button type="button" aria-pressed={base === 'etapa'} onClick={() => setBase('etapa')}>
                Fluxo
              </button>
            </div>

            <PainelDoMomento
              titulo="Conversão entre estágios"
              data-bloco="funil-conversao"
              area="funil-conversao"
              dica={
                <>
                  <p>
                    A conversão é entre estágios vizinhos: dos processos do estágio de cima, quantos chegaram a este. A
                    barra é essa conversão, na cor do estágio.
                  </p>
                  <p>
                    O primeiro estágio não tem de onde converter. Com menos de 10 processos no estágio anterior, a taxa não
                    aparece — a base é pequena demais para significar alguma coisa. Acima de 100%, aparece ↑: o estágio tem
                    mais processos que o anterior.
                  </p>
                  <p>A cor: 70% ou mais, verde; de 40% a 70%, laranja; abaixo de 40%, vermelho.</p>
                </>
              }
            >
              {dados && (
                <ConversaoDoFunil estagios={ESTAGIOS_DO_FUNIL} faixas={faixas} motivo={temFunil ? null : motivo} />
              )}
            </PainelDoMomento>
          </div>
        </div>

        {dados && (
          <div className="mom-painel funil-painel-tabela" data-bloco="funil-tabela">
            <TabelaDoFunil estagios={estagios} motivo={temFunil ? null : motivo} />
          </div>
        )}
      </section>

      {/* ---------------------------------------------------------------- */}
      {/* As perdas do período — o formulário de venda perdida              */}
      {/* ---------------------------------------------------------------- */}
      {/* SÓ A PRINCIPAL CONTA (27/09/2026): a mesma perda registrada no FY25 e no SEM_PARTICIPACAO, ou detalhada nos
          VP_*, conta uma vez. As duas populações continuam lado a lado — processos perdidos no funil e formulários
          preenchidos —, porque a diferença é quantas derrotas ninguém registrou. */}
      <section className="dash-secao" data-bloco="secao-perdas">
        {/* A FAIXA LARANJA DA MAQUETE: o título da seção à esquerda e, à direita, o que a distribuição de perdas não diz —
            a mesma frase da metodologia, com os números do período, e o que a API mediu que falta. */}
        <div className="funil-perdas-faixa" data-bloco="perdas-resumo">
          <div className="funil-perdas-cabeca">
            <span className="funil-perdas-icone" aria-hidden="true">
              <CircleAlert size={30} strokeWidth={2.2} />
            </span>
            <div className="terr-secao-mercado">
              <h2 className="terr-secao-titulo">
                As perdas do período
                <InfoTooltip
                  texto="O motivo, o concorrente e o preço só existem quando o CEN preenche o formulário de venda perdida no Vórtice, e chegam aqui pela rotina que traz o funil. Só a venda perdida principal conta: a mesma perda registrada em dois formulários conta uma vez. A diferença entre os processos perdidos e os formulários é quantas derrotas ninguém registrou."
                  rotulo="Fonte e método de as perdas do período"
                />
              </h2>
              <p className="terr-secao-subtitulo">
                {vendas.dados
                  ? `${nº(vendas.dados.registradas)} formulários de ${nº(vendas.dados.processosPerdidos)} processos perdidos · ${vendas.dados.periodo.texto}`
                  : 'Do formulário de venda perdida, contadas no banco.'}
              </p>
            </div>
          </div>

          {vendas.dados && (vendas.dados.registradas > 0 || vendas.dados.metricasSemDado.length > 0) && (
            <div className="funil-perdas-aviso" role="note">
              <span className="funil-perdas-aviso-icone" aria-hidden="true" />
              <div>
                <p className="funil-perdas-aviso-titulo">O que a distribuição de perdas não diz</p>
                {vendas.dados.registradas > 0 && (
                  <p>
                    Só aparecem aqui os processos perdidos com o formulário de venda perdida preenchido. A distribuição
                    abaixo é das {nº(vendas.dados.registradas)} derrotas que foram registradas, e não de todos os{' '}
                    {nº(vendas.dados.processosPerdidos)} processos perdidos no período.
                  </p>
                )}
                {vendas.dados.metricasSemDado.map((m) => (
                  <p key={m.metrica}>{m.motivo}</p>
                ))}
              </div>
            </div>
          )}
        </div>

        {vendas.carregando && <BlocoCarregando oQue="as vendas perdidas" />}
        {vendas.erro && <BlocoErro erro={vendas.erro} aoTentarDeNovo={vendas.recarregar} />}

        {vendas.dados && vendas.dados.registradas === 0 && !vendas.erro && (
          <BlocoVazio
            titulo={`Nenhuma venda perdida registrada em ${vendas.dados.periodo.texto}`}
            texto="O motivo, o concorrente e o preço só existem quando o CEN preenche o formulário de venda perdida no Vórtice — e chegam aqui pela rotina que traz o funil."
          />
        )}

        {/* UM PAINEL, DUAS ABAS — o motivo e o concorrente. Na maquete o alternador mora na linha do título, e a frase do
            que a tabela mostra vem ao lado do nome. */}
        {vendas.dados && vendas.dados.registradas > 0 && (
          <PainelDoMomento
            titulo="Vendas perdidas"
            data-bloco="perdas-por-motivo"
            area="funil-perdas"
            dica="O motivo e o concorrente são os que o CEN declarou no formulário. O preço é a diferença média entre o nosso e o do concorrente, só nos formulários que trazem os dois — o denominador vem ao lado."
            direita={
              <>
                <p className="funil-perdas-frase">
                  Quantas, quantas máquinas e o preço John Deere contra o do concorrente, por motivo e por fabricante.
                </p>
                <div className="terr-alternador funil-abas" role="group" aria-label="Vendas perdidas por">
                  <button type="button" aria-pressed={abaDasPerdas === 'motivo'} onClick={() => setAbaDasPerdas('motivo')}>
                    Por motivo
                  </button>
                  <button
                    type="button"
                    aria-pressed={abaDasPerdas === 'concorrente'}
                    onClick={() => setAbaDasPerdas('concorrente')}
                  >
                    Para quem perdemos
                  </button>
                </div>
              </>
            }
          >
            {abaDasPerdas === 'motivo' ? (
              vendas.dados.porMotivo.length > 0 ? (
                <TabelaDePerdas titulo="Motivo" legenda="Vendas perdidas por motivo" fatias={vendas.dados.porMotivo} />
              ) : (
                <p className="v360-nota">Nenhum formulário do período declara o motivo.</p>
              )
            ) : // PARA QUEM PERDEMOS: o fabricante, e o preço John Deere contra o dele quando o formulário traz os dois.
            vendas.dados.porConcorrente.length > 0 ? (
              <TabelaDePerdas titulo="Concorrente" legenda="Vendas perdidas por concorrente" fatias={vendas.dados.porConcorrente} forte />
            ) : (
              <p className="v360-nota">Nenhum formulário do período declara o concorrente.</p>
            )}
          </PainelDoMomento>
        )}
      </section>

      <BlocoRecolhivel titulo="O que este relatório não pode afirmar" resumo="valor do funil, meta e faturamento">
        <div className="cad-fichas">
          <LacunaConhecida
            metrica="Valor do funil e ticket médio"
            motivo={
              'O funil por estágio conta processos, e não valor: a rotina que o traz lê do Vórtice o estágio e o ' +
              'desfecho de cada processo, e não o valor do negócio. Somar um valor aqui seria somar o que não foi lido.'
            }
          />
          <LacunaConhecida
            metrica="Atingimento de meta"
            motivo={
              'A meta de venda tem fonte desde 27/09/2026 — a cota da API Gestão de Negócios, em máquinas ' +
              'por consultor, linha e mês — e o realizado são as máquinas vendidas lidas do ART. As duas ' +
              'estão no cartão "Meta e realizado" da Visão 360 e na Performance de CEN, e não no funil: ' +
              'o funil conta processos do Vórtice, e nada liga um processo faturado à venda do ART.'
            }
          />
          {/* A FRASE ANTIGA DIZIA QUE O FATURAMENTO "PAROU EM 11/04/2025" — era a cópia que o Vórtice
              recebia (EXT_NFS). O faturamento do Protheus está no CRM (27/09/2026); o que falta é a ponte
              entre a nota e o processo. */}
          <LacunaConhecida
            metrica="Faturamento realizado"
            motivo={
              'O faturamento do Protheus está no CRM, lido direto da nota de saída, mas por cliente, ' +
              'filial e mês: a nota não diz de qual processo nasceu, e o estágio Faturamento é o resultado ' +
              'registrado no histórico do Vórtice, e não a nota. O valor faturado aparece na Visão 360 e no ' +
              '360 de cada cliente, e não no funil.'
            }
          />
        </div>
      </BlocoRecolhivel>
    </PaginaDoPainel>
  );
}

/**
 * Os seis estágios em número: processos, os dois percentuais, o subfunil digital e os desfechos.
 *
 * SEM DADO, A TABELA FICA — com "—" e o motivo na dica em cada estágio. Some o número, não o funil: um funil que
 * desaparece parece um funil que nunca existiu, e um de zeros afirma uma medição que não houve.
 */
function TabelaDoFunil({ estagios, motivo }: { estagios: EstagioNoFunil[]; motivo: string | null }) {
  const porCodigo = new Map(estagios.map((e) => [e.estagio, e]));

  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela">
        <caption className="cad-so-leitor">O funil por estágio, em número</caption>
        <thead>
          <tr>
            <th scope="col">Estágio</th>
            <th scope="col" className="mom-num">Processos</th>
            <th scope="col" className="mom-num">Sobre o anterior</th>
            <th scope="col" className="mom-num">Sobre o Lead</th>
            <th scope="col" className="mom-num">Pela entrada digital</th>
            <th scope="col" className="mom-num">Ganhos</th>
            <th scope="col" className="mom-num">Perdidos</th>
            <th scope="col" className="mom-num">Abertos</th>
          </tr>
        </thead>
        <tbody>
          {ESTAGIOS_DO_FUNIL.map(({ estagio, nome }, i) => {
            const e = porCodigo.get(estagio);
            const ausente = <ValorAusente motivo={motivo ?? 'sem dado'} oQue={`o estágio ${nome}`} />;
            return (
              <tr key={estagio}>
                <th scope="row">
                  {/* O PONTO DA COR DO ESTÁGIO, a mesma do funil e da conversão (maquete). */}
                  <span className="funil-ponto" style={{ background: corDaFaixa(i, ESTAGIOS_DO_FUNIL.length) }} aria-hidden="true" />
                  {nome}
                </th>
                <td className="mom-num">{e ? nº(e.processos) : ausente}</td>
                <td className="mom-num">{e ? (pct(e.percentualSobreOAnterior) ?? '—') : ausente}</td>
                <td className="mom-num">{e ? (pct(e.percentualSobreOLead) ?? '—') : ausente}</td>
                <td className="mom-num">{e ? nº(e.pelaEntradaDigital) : ausente}</td>
                <td className="mom-num">{e ? nº(e.ganhos) : ausente}</td>
                <td className="mom-num">{e ? nº(e.perdidos) : ausente}</td>
                <td className="mom-num">{e ? nº(e.abertos) : ausente}</td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

/** A tabela de uma distribuição de perdas: quantas, quantas máquinas, e o preço John Deere contra o concorrente. */
function TabelaDePerdas({
  titulo,
  legenda,
  fatias,
  forte = false,
}: {
  titulo: string;
  legenda: string;
  fatias: FatiaDeVendaPerdida[];
  forte?: boolean;
}) {
  return (
    <div className="mom-tabela-rolagem">
      <table className="mom-tabela">
        <caption className="cad-so-leitor">{legenda}</caption>
        <thead>
          <tr>
            <th scope="col">{titulo}</th>
            <th scope="col" className="mom-num">Vendas perdidas</th>
            <th scope="col" className="mom-num">Máquinas</th>
            <th scope="col" className="mom-num">Preço JD acima, em média</th>
          </tr>
        </thead>
        <tbody>
          {fatias.map((f) => (
            <tr key={f.codigo}>
              <th scope="row" data-forte={forte || undefined}>{f.nome}</th>
              <td className="mom-num">{nº(f.quantidade)}</td>
              <td className="mom-num">{nº(f.maquinas)}</td>
              <td className="mom-num">
                <Diferenca fatia={f} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

/**
 * A diferença média de preço, com o denominador ao lado.
 *
 * O número sozinho diria "perdemos por R$ 52 mil", sem dizer sobre quantas negociações a média
 * foi feita — e a média de seis linhas não é a mesma coisa que a média de noventa e oito. O
 * denominador vem junto, sempre, e quando ele é zero o que aparece é o motivo, não um zero.
 */
function Diferenca({ fatia }: { fatia: FatiaDeVendaPerdida }) {
  if (fatia.diferencaMediaDePreco === null || fatia.comOsDoisPrecos === 0) {
    return <span className="cad-nada">sem os dois preços</span>;
  }

  // O VALOR E O DENOMINADOR NA MESMA LINHA, o denominador menor e cinza à direita (maquete).
  return (
    <span className="funil-preco">
      <span>
        {fatia.diferencaMediaDePreco.toLocaleString('pt-BR', {
          style: 'currency',
          currency: 'BRL',
          maximumFractionDigits: 0,
        })}
      </span>
      <span className="cad-sub funil-preco-base">
        de {fatia.comOsDoisPrecos} de {fatia.quantidade}
      </span>
    </span>
  );
}
