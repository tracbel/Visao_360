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
 */

import { useState } from 'react';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { AvisoDeProcedencia, SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { BlocoRecolhivel } from '../componentes/cadastro/BlocoRecolhivel';
import { LacunaConhecida, MetricasSemDado } from '../componentes/cadastro/SemDado';
import { ValorAusente } from '../componentes/comum/ValorAusente';
import { corDaFaixa, GraficoFunil, LegendaDoFunil, type FaixaDoFunil } from '../componentes/GraficoFunil';
import { useContextoDeAcesso } from '../dados/api/contexto';
import { obterFunilPorEstagio, obterVendasPerdidas } from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import type { BaseDoFunil, EstagioNoFunil, FatiaDeVendaPerdida } from '../tipos/relacionamento';

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

const nº = (v: number) => v.toLocaleString('pt-BR');
const pct = (v: number | null) => (v === null ? null : `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })}%`);

export function Funil() {
  const { contexto } = useContextoDeAcesso();
  const [base, setBase] = useState<BaseDoFunil>('abertura');
  /** O período escolhido; nulo é o padrão do servidor — o ano fiscal até o último mês fechado. */
  const [periodo, setPeriodo] = useState<{ de: string; ate: string } | null>(null);
  const [formulario, setFormulario] = useState('');

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

  const semNumero = temFunil ? undefined : 'sem dado — o motivo está logo abaixo';

  const indicadores: Indicador[] = [
    {
      rotulo: 'Leads',
      valor: lead ? lead.processos : null,
      deOnde:
        base === 'abertura'
          ? `processos abertos em ${dados?.periodo.texto ?? 'período'} que viraram lead`
          : `leads alcançados em ${dados?.periodo.texto ?? 'período'}`,
      semDado: semNumero,
    },
    {
      rotulo: 'Faturados',
      valor: faturamento ? faturamento.processos : null,
      tom: 'bom',
      deOnde: faturamento && faturamento.percentualSobreOLead !== null ? `${pct(faturamento.percentualSobreOLead)} dos leads` : 'sobre os leads',
      semDado: semNumero,
    },
    {
      rotulo: 'Ganhos · perdidos · abertos',
      valor: lead ? `${nº(lead.ganhos)} · ${nº(lead.perdidos)} · ${nº(lead.abertos)}` : null,
      deOnde: 'o desfecho, no Vórtice, dos processos contados no Lead',
      semDado: semNumero,
    },
    {
      rotulo: `Parados há mais de ${dados?.diasParaParado ?? 60} dias`,
      valor: dados?.paradosEmNegociacaoOuPedido ?? null,
      tom: 'atencao',
      deOnde: 'em Negociação ou Pedido, sem avançar nem encerrar — hoje, qualquer que seja o período',
      semDado: semNumero,
    },
  ];

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Funil de Vendas</h1>
          <p className="page-subtitle">
            Os processos do Vórtice por estágio — de Lead a Faturamento —, contados no banco dentro da filial do
            cabeçalho.
          </p>
        </div>
      </div>

      <AvisoDeProcedencia procedencia={funil.procedencia} />

      <PainelDeIndicadores indicadores={indicadores} carregando={funil.carregando} />

      <MetricasSemDado metricas={dados?.metricasSemDado} />

      {/* ---------------------------------------------------------------- */}
      {/* O funil por estágio, com a forma do protótipo                    */}
      {/* ---------------------------------------------------------------- */}
      <div className="card cad-cartao" data-bloco="funil-por-estagio">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Funil por estágio</div>
            <div className="card-subtitle">
              {dados ? `${leitura} · ${dados.periodo.texto}${dados.periodo.ehOPadrao ? ' · ano fiscal até o último mês fechado' : ''}` : leitura}
            </div>
          </div>
          <SeloProcedencia procedencia={funil.procedencia} />
        </div>

        <div className="cad-barra">
          {/* A CHAVE COORTE × FLUXO (decisão de 27/09/2026). A coorte é o padrão; o fluxo é o que a Performance de CEN
              e os alertas usam. */}
          <div className="cad-filtro" role="group" aria-label="Leitura do funil">
            Leitura
            <div className="funil-abas">
              <button
                type="button"
                className={base === 'abertura' ? 'funil-aba ativa' : 'funil-aba'}
                aria-pressed={base === 'abertura'}
                onClick={() => setBase('abertura')}
              >
                Coorte
              </button>
              <button
                type="button"
                className={base === 'etapa' ? 'funil-aba ativa' : 'funil-aba'}
                aria-pressed={base === 'etapa'}
                onClick={() => setBase('etapa')}
              >
                Fluxo
              </button>
            </div>
          </div>

          <label className="cad-filtro">
            De
            <input
              type="date"
              value={periodoMostrado?.de ?? ''}
              onChange={(e) => e.target.value && setPeriodo({ de: e.target.value, ate: periodoMostrado?.ate ?? e.target.value })}
            />
          </label>
          <label className="cad-filtro">
            Até
            <input
              type="date"
              value={periodoMostrado?.ate ?? ''}
              onChange={(e) => e.target.value && setPeriodo({ de: periodoMostrado?.de ?? e.target.value, ate: e.target.value })}
            />
          </label>
          {periodo && (
            <div className="cad-filtro">
              &nbsp;
              <button type="button" className="funil-aba" onClick={() => setPeriodo(null)}>
                Ano fiscal até o último mês fechado
              </button>
            </div>
          )}
        </div>

        {funil.carregando && <BlocoCarregando oQue="o funil" />}
        {funil.erro && <BlocoErro erro={funil.erro} aoTentarDeNovo={funil.recarregar} />}

        {temFunil && (
          <div className="funil-container">
            <GraficoFunil faixas={faixas} />
            <LegendaDoFunil faixas={faixas} />
          </div>
        )}

        {dados && <TabelaDoFunil estagios={estagios} motivo={temFunil ? null : motivo} />}

        <div className="funil-scope-note">
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" strokeWidth={2} aria-hidden="true">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 8h.01" />
            <path d="M11 12h1v4h1" />
          </svg>
          <div>
            <strong>{leitura}:</strong>{' '}
            {base === 'abertura'
              ? 'os processos abertos no período, e até onde cada um chegou — de cada 100 leads que entraram, quantos viraram pedido.'
              : 'as etapas alcançadas no período, qualquer que seja a abertura — quantos pedidos e faturamentos saíram no período.'}{' '}
            Lead e Qualificado são cumulativos: quem chegou à Cobertura também conta nos dois. O estágio é o resultado do
            histórico do Vórtice, como o BI — a fase do fluxo não é o funil, e fica no Pipeline.
          </div>
        </div>
      </div>

      {/* ---------------------------------------------------------------- */}
      {/* As perdas do período — o formulário de venda perdida              */}
      {/* ---------------------------------------------------------------- */}
      {/* SÓ A PRINCIPAL CONTA (27/09/2026): a mesma perda registrada no FY25 e no SEM_PARTICIPACAO, ou detalhada nos
          VP_*, conta uma vez. As duas populações continuam lado a lado — processos perdidos no funil e formulários
          preenchidos —, porque a diferença é quantas derrotas ninguém registrou. */}
      <div className="card cad-cartao" data-bloco="perdas-por-motivo">
        <div className="card-header cad-cartao-cabecalho">
          <div>
            <div className="card-title">Vendas perdidas por motivo</div>
            <div className="card-subtitle">
              {vendas.dados
                ? `${nº(vendas.dados.registradas)} formulários de ${nº(vendas.dados.processosPerdidos)} processos perdidos · ${vendas.dados.periodo.texto}`
                : 'do formulário de venda perdida, contadas no banco'}
            </div>
          </div>
          <SeloProcedencia procedencia={vendas.procedencia} />
        </div>

        <div className="cad-barra">
          <label className="cad-filtro">
            Formulário
            <select value={formulario} onChange={(e) => setFormulario(e.target.value)}>
              <option value="">Todos os formulários</option>
              {FORMULARIOS.map((f) => (
                <option key={f.codigo} value={f.codigo}>
                  {f.nome}
                </option>
              ))}
            </select>
          </label>
        </div>

        {vendas.carregando && <BlocoCarregando oQue="as vendas perdidas" />}
        {vendas.erro && <BlocoErro erro={vendas.erro} aoTentarDeNovo={vendas.recarregar} />}

        <MetricasSemDado metricas={vendas.dados?.metricasSemDado} titulo="O que a distribuição de perdas não diz" />

        {vendas.dados && vendas.dados.registradas === 0 && !vendas.erro && (
          <BlocoVazio
            titulo={`Nenhuma venda perdida registrada em ${vendas.dados.periodo.texto}`}
            texto="O motivo, o concorrente e o preço só existem quando o CEN preenche o formulário de venda perdida no Vórtice — e chegam aqui pela rotina que traz o funil."
          />
        )}

        {vendas.dados && vendas.dados.porMotivo.length > 0 && (
          <TabelaDePerdas titulo="Motivo" legenda="Vendas perdidas por motivo" fatias={vendas.dados.porMotivo} />
        )}
      </div>

      {/* PARA QUEM PERDEMOS: o fabricante, e o preço John Deere contra o dele quando o formulário traz os dois. */}
      {vendas.dados && vendas.dados.porConcorrente.length > 0 && (
        <div className="card cad-cartao" data-bloco="para-quem-perdemos">
          <div className="card-header cad-cartao-cabecalho">
            <div>
              <div className="card-title">Para quem perdemos</div>
              <div className="card-subtitle">
                o fabricante que levou a venda, e o preço John Deere contra o dele quando o formulário traz os dois ·{' '}
                {vendas.dados.periodo.texto}
              </div>
            </div>
            <SeloProcedencia procedencia={vendas.procedencia} />
          </div>
          <TabelaDePerdas titulo="Concorrente" legenda="Vendas perdidas por concorrente" fatias={vendas.dados.porConcorrente} forte />
        </div>
      )}

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
    </>
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
    <div className="cad-tabela-wrap">
      <table className="cad-tabela">
        <caption className="cad-so-leitor">O funil por estágio, em número</caption>
        <thead>
          <tr>
            <th scope="col">Estágio</th>
            <th scope="col">Processos</th>
            <th scope="col">Sobre o anterior</th>
            <th scope="col">Sobre o Lead</th>
            <th scope="col">Pela entrada digital</th>
            <th scope="col">Ganhos</th>
            <th scope="col">Perdidos</th>
            <th scope="col">Abertos</th>
          </tr>
        </thead>
        <tbody>
          {ESTAGIOS_DO_FUNIL.map(({ estagio, nome }) => {
            const e = porCodigo.get(estagio);
            const ausente = <ValorAusente motivo={motivo ?? 'sem dado'} oQue={`o estágio ${nome}`} />;
            return (
              <tr key={estagio}>
                <td className="cad-link-forte">{nome}</td>
                <td className="cad-mono">{e ? nº(e.processos) : ausente}</td>
                <td className="cad-mono">{e ? (pct(e.percentualSobreOAnterior) ?? '—') : ausente}</td>
                <td className="cad-mono">{e ? (pct(e.percentualSobreOLead) ?? '—') : ausente}</td>
                <td className="cad-mono">{e ? nº(e.pelaEntradaDigital) : ausente}</td>
                <td className="cad-mono">{e ? nº(e.ganhos) : ausente}</td>
                <td className="cad-mono">{e ? nº(e.perdidos) : ausente}</td>
                <td className="cad-mono">{e ? nº(e.abertos) : ausente}</td>
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
    <div className="cad-tabela-wrap">
      <table className="cad-tabela">
        <caption className="cad-so-leitor">{legenda}</caption>
        <thead>
          <tr>
            <th scope="col">{titulo}</th>
            <th scope="col">Vendas perdidas</th>
            <th scope="col">Máquinas</th>
            <th scope="col">Preço JD acima, em média</th>
          </tr>
        </thead>
        <tbody>
          {fatias.map((f) => (
            <tr key={f.codigo}>
              <td className={forte ? 'cad-link-forte' : undefined}>{f.nome}</td>
              <td className="cad-mono">{nº(f.quantidade)}</td>
              <td className="cad-mono">{nº(f.maquinas)}</td>
              <td className="cad-mono">
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

  return (
    <>
      <div>
        {fatia.diferencaMediaDePreco.toLocaleString('pt-BR', {
          style: 'currency',
          currency: 'BRL',
          maximumFractionDigits: 0,
        })}
      </div>
      <div className="cad-sub">
        de {fatia.comOsDoisPrecos} de {fatia.quantidade}
      </div>
    </>
  );
}
