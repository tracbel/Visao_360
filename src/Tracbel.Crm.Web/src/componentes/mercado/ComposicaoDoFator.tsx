/**
 * A COMPOSIÇÃO DO FATOR (T3.1; redesenhada na T4.6; na forma da maquete
 * `momento-composicao-do-fator.png` desde a fidelidade às maquetes, fase 3).
 *
 * O número do topo da tela é UM, mas ele não nasce de um índice de mercado: ele
 * é a razão entre dois números que o motor já calcula para cada cultura —
 *
 *     fator agregado = Σ demanda ajustada ÷ Σ demanda estrutural
 *
 * — e é aqui que essa conta fica visível. Quem desconfiar do 0,88 do topo soma
 * as colunas do detalhamento e confere (a conta inteira está na dica dele).
 *
 * O QUE MUDOU NA FASE 3 — a forma, e nada da conta. A maquete abre a aba com
 * quatro cartões (Commodity, Custo, Crédito, Percepção comercial), dois painéis
 * (as parcelas por cultura e a demanda estrutural contra a ajustada) e a tabela.
 *
 * AS PARCELAS CONTINUAM TRÊS (D-P05). O custo entra DENTRO da primeira —
 * rentabilidade é preço menos custo —, e a maquete o desenha como um quarto
 * cartão e uma quarta coluna. O lugar dele ficou (decisão 2 do usuário: o
 * layout é o da maquete), mas com o traço e o motivo: um número de custo ali
 * afirmaria uma sensibilidade independente que a decisão não criou.
 *
 * SEM ADJETIVO QUE NÃO TENHA REGRA. "Favorável", "Estável", "Restritivo" pedem
 * limites que ninguém registrou para cada parcela. O que existe é o sentido —
 * ↑, → ou ↓ — e o índice, e é isso que os cartões mostram.
 *
 * PREÇO É DE CADA CULTURA; CRÉDITO E PERCEPÇÃO SÃO DO RECORTE. Por isso as duas
 * últimas parcelas se repetem iguais em todas as linhas, e o cabeçalho diz isso
 * em vez de deixar o leitor achar que é falha.
 */

import { Coins, CreditCard, Leaf, Users } from 'lucide-react';
import { BlocoCarregando } from '../cadastro/EstadosDeTela';
import { InfoTooltip } from '../InfoTooltip';
import { MetricaAusente } from '../comum/MetricaAusente';
import { ValorAusente } from '../comum/ValorAusente';
import type { MomentoDaCultura, MomentoDoRecorte } from '../../tipos/territorio';
import { ColunasAgrupadas } from './momento/ColunasAgrupadas';
import { numero, percentualComSinal, pontosPercentuais, sentido, tomDoSentido } from './momento/formatos';
import { NomeDaCultura } from './momento/IconeDaCultura';
import {
  CartaoDoMomento,
  FileiraDeCartoes,
  GraficoSemSerie,
  Legenda,
  LinhaDePaineis,
  MenuDaLinha,
  PainelDoMomento,
} from './momento/pecas';

/**
 * AS CASAS SÃO FIXAS, e isso não é enfeite: um fator neutro tem de aparecer como
 * `1,00`. Escrito `1`, ele deixa de parecer um fator e passa a parecer contagem.
 */
const pt = (v: number, casas = 2) => numero(v, casas);

/** Quantas culturas os dois gráficos desenham — a tabela mostra todas. */
const MAXIMO_NOS_GRAFICOS = 8;

const COR_COMMODITY = '#5BB26B';
const COR_CREDITO = '#9575E0';
const COR_PERCEPCAO = '#F4B35E';
const COR_ESTRUTURAL = '#0B5D2A';
const COR_AJUSTADA = '#8FD19E';

const SEM_DEMANDA =
  'Nenhuma cultura deste recorte tem regra de potencial com ciclo de renovação, então não há demanda a ajustar e ' +
  'não há composição a decompor. É a decisão D-P01 (issue 63) que fixa cultura, categoria, hectares por máquina e ' +
  'anos de renovação — as quatro juntas.';

const EXPLICA_COMMODITY =
  'Preço e rentabilidade são DE CADA CULTURA — o índice é dela, 12 meses contra os 12 anteriores (issue 73), e não ' +
  'do recorte. O custo entra AQUI: rentabilidade é preço menos custo, e não uma quarta sensibilidade (D-P05).';

const EXPLICA_CUSTO =
  'O custo não é uma parcela separada do fator (D-P05): a sensibilidade é de PREÇO E RENTABILIDADE, e o custo entra ' +
  'dentro dela — rentabilidade é preço menos custo. Não existe índice de custo do recorte para mostrar aqui sem mudar ' +
  'a conta. Os custos de produção da CONAB, safra a safra, estão na aba Rentabilidade (issue 67).';

const EXPLICA_CREDITO =
  'Índice de crédito do SICOR — 70% linhas e 30% valor, a janela recente contra a anterior (issue 73). É do ' +
  'MUNICÍPIO, não da cultura, e por isso entra igual em todas. Ele MULTIPLICA o resto, porque é a condição de ' +
  'financiar: sem crédito, nem a melhor safra vira máquina. Uma linha do SICOR não é um contrato.';

const EXPLICA_PERCEPCAO =
  'A leitura do comercial sobre o município, de −5 a +5 pontos percentuais, com autor e vigência (issue 71). É do ' +
  'RECORTE, não da cultura. Ela soma dentro do parêntese do produtor, então o crédito a amplifica. É uma leitura ' +
  'registrada, e não uma série: não há "variação no período" a calcular para ela.';

/**
 * A pílula da direita do cartão: a variação no período, ou o traço COM O
 * MOTIVO — o "—" mudo deixava a pessoa sem saber se faltava dado ou se a
 * variação era zero (decisão 1 do usuário).
 */
function Pilula({ fracao, texto, motivo, oQue }: { fracao: number | null; texto?: string; motivo: string; oQue: string }) {
  return (
    <>
      <span
        className="mom-pilula"
        data-sentido={fracao === null ? undefined : tomDoSentido(fracao)}
        data-intervalo={texto ? 'true' : undefined}
      >
        {texto ?? (fracao === null ? <ValorAusente motivo={motivo} oQue={oQue} /> : percentualComSinal(fracao))}
      </span>
      <span className="mom-pilula-legenda">variação no período</span>
    </>
  );
}

/** Seta e texto, com a cor só de reforço. */
function Sentido({ fracao, children }: { fracao: number; children: React.ReactNode }) {
  return (
    <span className="mom-sentido" data-sentido={tomDoSentido(fracao)}>
      <span aria-hidden="true">{sentido(fracao)}</span>
      {children}
    </span>
  );
}

/** Os quatro cartões — Commodity, Custo, Crédito e Percepção comercial. */
function Cartoes({ momento }: { momento: MomentoDoRecorte }) {
  // O PREÇO É POR CULTURA: o cartão diz quantas sobem e quantas descem, e o
  // intervalo entre elas — sem inventar um índice de commodity do recorte, que
  // o domínio não tem (o agregado não é média de commodity, T3.1).
  const desvios = momento.porCultura
    .map((c) => (c.indiceDePreco === null ? null : c.indiceDePreco - 1))
    .filter((v): v is number => v !== null);
  const conta = (s: '↑' | '→' | '↓') => desvios.filter((v) => sentido(v) === s).length;
  const menor = desvios.length > 0 ? Math.min(...desvios) : null;
  const maior = desvios.length > 0 ? Math.max(...desvios) : null;

  const credito = momento.indiceDeCredito;
  const percepcao = momento.percepcaoPercentual;

  return (
    <FileiraDeCartoes>
      <CartaoDoMomento
        icone={Leaf}
        tom="verde"
        rotulo="Commodity"
        oQue="o momento de preço das culturas"
        dica={
          EXPLICA_COMMODITY +
          ' Não há um índice de commodity do recorte — o fator agregado não é média de commodity —, e por isso o ' +
          'cartão mostra quantas culturas sobem e quantas descem, e o intervalo entre elas. O índice de cada uma está ' +
          'no detalhamento, abaixo.'
        }
        valor={
          desvios.length === 0 ? null : (
            <span className="mom-sentido">
              {(['↑', '→', '↓'] as const)
                .filter((s) => conta(s) > 0)
                .map((s) => (
                  <span key={s} data-sentido={s === '↑' ? 'alta' : s === '↓' ? 'baixa' : 'neutro'}>
                    <span aria-hidden="true">{s}</span> {conta(s)}
                    <span className="cad-so-leitor">
                      {' '}
                      {conta(s) === 1 ? 'cultura' : 'culturas'} {s === '↑' ? 'em alta' : s === '↓' ? 'em baixa' : 'estáveis'}
                    </span>
                  </span>
                ))}
            </span>
          )
        }
        motivoSemDado="Nenhuma cultura do recorte tem índice de preço carregado (CONAB e Socicana, issue 66). Índice ausente vale desvio ZERO no fator — e não uma queda."
        apoio="Índice de preço de cada cultura"
        lateral={
          <Pilula
            fracao={menor !== null && menor === maior ? menor : null}
            texto={
              menor !== null && maior !== null && menor !== maior
                ? `${percentualComSinal(menor)} a ${percentualComSinal(maior)}`
                : undefined
            }
            motivo="Nenhuma cultura do recorte tem índice de preço carregado (CONAB e Socicana, issue 66): não há variação de preço a mostrar."
            oQue="a variação de preço das culturas"
          />
        }
      />

      <CartaoDoMomento
        icone={Coins}
        tom="azul"
        rotulo="Custo"
        oQue="o custo no fator"
        valor={null}
        motivoSemDado={EXPLICA_CUSTO}
        apoio="Entra na parcela de commodity (D-P05)"
        lateral={
          <Pilula
            fracao={null}
            motivo="O custo não tem variação própria no fator: ele entra dentro da parcela de commodity (D-P05), e a série de custo da CONAB está na aba Rentabilidade (issue 67)."
            oQue="a variação do custo"
          />
        }
      />

      <CartaoDoMomento
        icone={CreditCard}
        tom="roxo"
        rotulo="Crédito"
        oQue="o índice de crédito"
        dica={EXPLICA_CREDITO}
        valor={credito === null ? null : <Sentido fracao={credito - 1}>índice {pt(credito)}</Sentido>}
        motivoSemDado="Sem índice de crédito para este recorte: ele precisa de linha do SICOR nas duas janelas e do parâmetro vigente com os pesos (issue 73). Índice ausente vale desvio ZERO no fator."
        apoio="Linhas e valor do SICOR"
        lateral={
          <Pilula
            fracao={credito === null ? null : credito - 1}
            motivo="Sem índice de crédito para este recorte não há variação: ele precisa de linha do SICOR nas duas janelas e do parâmetro vigente com os pesos (issue 73)."
            oQue="a variação do crédito"
          />
        }
      />

      <CartaoDoMomento
        icone={Users}
        tom="laranja"
        rotulo="Percepção comercial"
        oQue="a percepção comercial"
        dica={EXPLICA_PERCEPCAO}
        valor={percepcao === null ? null : <Sentido fracao={percepcao / 100}>{pontosPercentuais(percepcao)}</Sentido>}
        motivoSemDado="Nenhuma percepção do gestor está registrada para este recorte (issue 71). Ausente vale ZERO no fator — e zero aqui não é neutralidade declarada, é falta de registro."
        apoio="Leitura do gestor, de −5 a +5 p.p."
        lateral={
          <Pilula
            fracao={null}
            motivo="A percepção é uma leitura registrada, com vigência, e não uma série: não há variação no período a calcular para ela (issue 71)."
            oQue="a variação da percepção"
          />
        }
      />
    </FileiraDeCartoes>
  );
}

/** As três parcelas de uma cultura, na ordem das colunas. */
function parcelasDe(c: MomentoDaCultura) {
  return [
    { id: 'commodity', nome: 'Commodity e custo', valor: c.fator.parcelaDePreco },
    { id: 'credito', nome: 'Crédito', valor: c.fator.parcelaDeCredito },
    { id: 'percepcao', nome: 'Percepção comercial', valor: c.fator.parcelaDaPercepcao },
  ] as const;
}

/**
 * AS PARCELAS EMPILHADAS POR CULTURA.
 *
 * O COMPRIMENTO DE CADA PEDAÇO É O TAMANHO DO EMPURRÃO, para cima ou para
 * baixo: a parcela de −8% e a de +8% ocupam o mesmo espaço, e o sinal escrito
 * dentro (mais a hachura na que puxa para baixo) diz o sentido. A escala é a da
 * cultura que mais se move, para as linhas serem comparáveis entre si.
 */
function PilhasDasParcelas({ porCultura }: { porCultura: readonly MomentoDaCultura[] }) {
  const tamanho = (c: MomentoDaCultura) => parcelasDe(c).reduce((s, p) => s + Math.abs(p.valor ?? 0), 0);
  const escala = Math.max(0.0001, ...porCultura.map(tamanho));

  return (
    <ul className="mom-pilhas" aria-label="Parcelas do fator, por cultura">
      <li className="mom-pilhas-cabecalho" aria-hidden="true">
        <span />
        <span />
        <span>Fator final</span>
      </li>
      {porCultura.map((c) => {
        const parcelas = parcelasDe(c);
        return (
          <li key={c.culturaCodigo} className="mom-pilha" data-cultura={c.culturaCodigo}>
            <NomeDaCultura nome={c.cultura} />
            <span className="mom-pilha-trilho">
              <span className="cad-so-leitor">
                {parcelas
                  .map((p) => `${p.nome}: ${p.valor === null ? 'sem índice' : percentualComSinal(p.valor, 1)}`)
                  .join('; ')}
              </span>
              {parcelas.map((p) => {
                const largura = (Math.abs(p.valor ?? 0) / escala) * 100;
                if (largura <= 0) return null;
                return (
                  <span
                    key={p.id}
                    data-parcela={p.id}
                    data-negativa={(p.valor ?? 0) < 0 ? 'true' : undefined}
                    style={{ width: `${largura}%` }}
                    aria-hidden="true"
                  >
                    {largura >= 11 ? percentualComSinal(p.valor ?? 0, 1) : ''}
                  </span>
                );
              })}
            </span>
            <span className="mom-pilha-fator">
              {c.fator.fator === null ? <ValorAusente motivo={c.fator.motivo} oQue={`o fator de ${c.cultura}`} /> : pt(c.fator.fator)}
            </span>
          </li>
        );
      })}
    </ul>
  );
}

/** Uma célula de parcela — sinal e número; sem índice, o traço. */
function CelulaDaParcela({ valor }: { valor: number | null }) {
  if (valor === null)
    return (
      <td className="mom-num">
        <span aria-hidden="true">—</span>
        <span className="cad-so-leitor">sem índice, vale zero</span>
      </td>
    );
  const tom = tomDoSentido(valor);
  return (
    <td className={`mom-num ${tom === 'alta' ? 'mom-alta' : tom === 'baixa' ? 'mom-baixa' : ''}`}>
      {percentualComSinal(valor, 1)}
    </td>
  );
}

/** O agregado, por extenso — a conta que fecha o número do topo. */
function ContaDoAgregado({ momento }: { momento: MomentoDoRecorte }) {
  if (momento.fatorAgregado === null)
    return (
      <>
        {momento.motivoSemFator === 'SemDemandaEstrutural'
          ? 'Nenhuma cultura do recorte tem demanda estrutural, então não há denominador para a razão. Enquanto o ciclo de renovação (D-P01, issue 63) não for decidido, o agregado fica ausente: "não há base para dizer" é diferente de "o mercado está neutro".'
          : 'Há demanda estrutural, mas nenhuma cultura produziu fator — faltam os pesos das sensibilidades (D-P05). Peso é decisão registrada, não conta.'}
      </>
    );

  return (
    <div className="mom-detalhe">
      <p>
        <strong>
          Agregado: {numero(momento.demandaAjustadaTotal ?? 0)} ÷ {numero(momento.demandaEstruturalTotal ?? 0)} ={' '}
          {pt(momento.fatorAgregado)}
        </strong>
      </p>
      <p>
        Σ demanda ajustada ÷ Σ demanda estrutural, somando só as culturas que têm os dois números. Cada cultura pesa
        exatamente pela demanda que representa — a área dela não entra nesta conta. Nenhuma fórmula nova foi criada: com
        todas as culturas neutras, as duas somas se igualam e o agregado dá 1,00. E como cada fator já vem dentro dos
        limites registrados, a razão não escapa deles.
      </p>
    </div>
  );
}

function LinhaDaCultura({ c }: { c: MomentoDaCultura }) {
  const variacaoDaDemanda =
    c.demandaEstrutural !== null && c.demandaEstrutural > 0 && c.demandaAjustada !== null
      ? c.demandaAjustada / c.demandaEstrutural - 1
      : null;

  return (
    <tr data-cultura={c.culturaCodigo}>
      <th scope="row">
        <NomeDaCultura nome={c.cultura} />
      </th>
      <CelulaDaParcela valor={c.fator.parcelaDePreco} />
      <td className="mom-num">
        <span aria-hidden="true">—</span>
        <span className="cad-so-leitor">dentro de commodity</span>
      </td>
      <CelulaDaParcela valor={c.fator.parcelaDeCredito} />
      <CelulaDaParcela valor={c.fator.parcelaDaPercepcao} />
      <td className="mom-num">
        {c.fator.fator === null ? (
          <ValorAusente motivo={c.fator.motivo} oQue={`o fator de ${c.cultura}`} />
        ) : (
          <strong>{pt(c.fator.fator)}</strong>
        )}
      </td>
      <td className="mom-num">
        {c.demandaEstrutural === null ? (
          <ValorAusente
            motivo={`${c.cultura} não tem ciclo de renovação informado (D-P01, issue 63), então não há demanda estrutural aqui — e uma cultura sem demanda não pesa na agregação, em vez de entrar como zero e puxar o número para baixo.`}
            oQue={`a demanda de ${c.cultura}`}
          />
        ) : (
          numero(c.demandaEstrutural)
        )}
      </td>
      <td className="mom-num">
        {c.demandaAjustada === null ? (
          <ValorAusente
            motivo={
              c.demandaEstrutural === null
                ? `A demanda ajustada é a estrutural vezes o fator, e ${c.cultura} não tem demanda estrutural (ciclo de renovação, D-P01, issue 63).`
                : `A demanda ajustada é a estrutural vezes o fator, e ${c.cultura} ficou sem fator — o motivo está na coluna Fator final.`
            }
            oQue={`a demanda ajustada de ${c.cultura}`}
          />
        ) : (
          numero(c.demandaAjustada)
        )}
      </td>
      <td className={`mom-num ${variacaoDaDemanda === null ? '' : tomDoSentido(variacaoDaDemanda) === 'baixa' ? 'mom-baixa' : tomDoSentido(variacaoDaDemanda) === 'alta' ? 'mom-alta' : ''}`}>
        {variacaoDaDemanda === null ? (
          <ValorAusente
            motivo={`A variação compara a demanda ajustada com a estrutural, e ${c.cultura} não tem as duas — sem base, não há variação a calcular.`}
            oQue={`a variação da demanda de ${c.cultura}`}
          />
        ) : (
          <>
            <span aria-hidden="true">{sentido(variacaoDaDemanda)}</span> {percentualComSinal(variacaoDaDemanda)}
          </>
        )}
      </td>
      <td className="mom-acoes">
        <MenuDaLinha rotulo={`Detalhes do fator de ${c.cultura}`}>
          <dl>
            <dt>Índice de preço</dt>
            <dd>{c.indiceDePreco === null ? 'não carregado — vale desvio zero' : pt(c.indiceDePreco)}</dd>
            <dt>Indicadores com dado</dt>
            <dd>{c.fator.indicadoresUsados} de 3</dd>
            {c.fator.cortadoPeloLimite && (
              <>
                <dt>Corte pelo limite</dt>
                <dd>
                  calculado {pt(c.fator.fatorSemLimite ?? 0)}, trazido para {pt(c.fator.fator ?? 0)} — o limite é decisão com
                  vigência, não arredondamento
                </dd>
              </>
            )}
            {c.fator.estimativa && (
              <>
                <dt>Situação</dt>
                <dd>estimativa: os pesos do fator ainda estão a confirmar</dd>
              </>
            )}
          </dl>
          <p>
            Cada parcela é quanto aquele indicador moveu a demanda estrutural DESTA cultura. {EXPLICA_COMMODITY}
          </p>
        </MenuDaLinha>
      </td>
    </tr>
  );
}

export function ComposicaoDoFator({
  momento,
  carregando = false,
}: {
  momento: MomentoDoRecorte | null;
  carregando?: boolean;
}) {
  if (!momento)
    return carregando ? (
      <BlocoCarregando oQue="o momento do mercado" />
    ) : (
      <MetricaAusente
        metrica="Composição do fator"
        motivo="A leitura dos indicadores não trouxe o momento do recorte. Sem ele não há fator, parcelas nem demanda ajustada a mostrar — e a tela não os estima."
      />
    );

  const { porCultura } = momento;

  // OS GRÁFICOS MOSTRAM AS OITO CULTURAS DE MAIOR DEMANDA, na ordem em que o
  // servidor as mandou; a tabela mostra todas. Com dezoito culturas o painel
  // das parcelas ficava com 600 px e as colunas viravam traços com rótulo
  // cortado — a maquete desenha seis. A linha embaixo do gráfico diz o corte.
  const nosGraficos = (() => {
    if (porCultura.length <= MAXIMO_NOS_GRAFICOS) return porCultura;
    const maiores = new Set(
      [...porCultura]
        .sort((a, b) => (b.demandaEstrutural ?? -1) - (a.demandaEstrutural ?? -1))
        .slice(0, MAXIMO_NOS_GRAFICOS),
    );
    return porCultura.filter((c) => maiores.has(c));
  })();
  const corte =
    porCultura.length > nosGraficos.length
      ? `As ${nosGraficos.length} culturas de maior demanda; as ${porCultura.length} estão no detalhamento.`
      : null;
  const comDemanda = nosGraficos.filter((c) => c.demandaEstrutural !== null || c.demandaAjustada !== null);

  return (
    <div className="mom-painel-da-aba" data-bloco="composicao-do-fator">
      <Cartoes momento={momento} />

      <LinhaDePaineis variante="composicao">
        <PainelDoMomento
          titulo="Composição do fator por cultura"
          dica={
            'Cada barra é o fator de uma cultura aberto nas três parcelas: commodity (preço e rentabilidade, com o ' +
            'custo dentro — D-P05), crédito e percepção comercial. O comprimento é o tamanho do empurrão; o sinal ' +
            'escrito e a hachura dizem se ele puxa a renovação para frente ou para trás. O termo de troca não entra: ' +
            'ele precisa do preço de máquina (issue 70).'
          }
          subtitulo="Contribuição de cada parcela para o fator final da cultura; acima de 1,00 o momento antecipa a renovação."
          direita={
            <Legenda
              itens={[
                { nome: 'Commodity e custo', cor: COR_COMMODITY },
                { nome: 'Crédito', cor: COR_CREDITO },
                { nome: 'Percepção comercial', cor: COR_PERCEPCAO },
              ]}
            />
          }
        >
          {porCultura.length === 0 ? (
            <GraficoSemSerie altura={180} frase="Sem cultura com ciclo de renovação" motivo={SEM_DEMANDA} oQue="a composição por cultura" />
          ) : (
            <PilhasDasParcelas porCultura={nosGraficos} />
          )}
          {corte && <p className="mom-painel-subtitulo">{corte}</p>}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Demanda estrutural x Demanda ajustada"
          dica="A demanda estrutural é o que a área comporta renovar por ano, pelo motor (issue 72); a ajustada é ela vezes o fator da cultura. As duas vêm prontas do servidor."
          subtitulo="Máquinas por ano, por cultura: o que a área comporta e o que o momento ajusta."
          direita={
            <Legenda
              itens={[
                { nome: 'Demanda estrutural', cor: COR_ESTRUTURAL },
                { nome: 'Demanda ajustada', cor: COR_AJUSTADA },
              ]}
            />
          }
        >
          {comDemanda.length === 0 ? (
            <GraficoSemSerie altura={180} frase="Sem demanda estrutural" motivo={SEM_DEMANDA} oQue="a demanda por cultura" />
          ) : (
            <ColunasAgrupadas
              altura={170}
              descricao="Demanda estrutural e demanda ajustada, em máquinas por ano, por cultura. Os números estão no detalhamento por cultura."
              series={[
                { nome: 'Demanda estrutural', cor: COR_ESTRUTURAL },
                { nome: 'Demanda ajustada', cor: COR_AJUSTADA },
              ]}
              grupos={comDemanda.map((c) => ({ rotulo: c.cultura, valores: [c.demandaEstrutural, c.demandaAjustada] }))}
            />
          )}
          {corte && <p className="mom-painel-subtitulo">{corte}</p>}
        </PainelDoMomento>
      </LinhaDePaineis>

      <PainelDoMomento
        titulo="Detalhamento por cultura"
        dica={<ContaDoAgregado momento={momento} />}
        subtitulo="Contribuição de cada parcela e o resultado na demanda ajustada."
      >
        {/* A ROLAGEM É DENTRO DO PAINEL, e nunca da página: dez colunas passam de
            860px, e numa tela de 768 isso empurrava a PÁGINA para o lado. */}
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela">
            <thead>
              <tr>
                <th scope="col">Cultura</th>
                <th scope="col" className="mom-num">
                  Commodity <InfoTooltip rotulo="De quem é a parcela de commodity" texto={EXPLICA_COMMODITY} />
                </th>
                <th scope="col" className="mom-num">
                  Custo <InfoTooltip rotulo="Por que o custo não tem coluna própria" texto={EXPLICA_CUSTO} />
                </th>
                <th scope="col" className="mom-num">
                  Crédito <InfoTooltip rotulo="Por que o crédito se repete" texto={EXPLICA_CREDITO} />
                </th>
                <th scope="col" className="mom-num">
                  Percepção comercial{' '}
                  <InfoTooltip rotulo="Por que a percepção se repete" texto={EXPLICA_PERCEPCAO} />
                </th>
                <th scope="col" className="mom-num">
                  Fator final
                </th>
                <th scope="col" className="mom-num">
                  Demanda estrutural
                </th>
                <th scope="col" className="mom-num">
                  Demanda ajustada
                </th>
                <th scope="col" className="mom-num">
                  Var. %
                </th>
                <th scope="col" className="mom-acoes">
                  <span className="cad-so-leitor">Detalhes</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {porCultura.length === 0 ? (
                <tr className="mom-tabela-vazia">
                  <td colSpan={10}>
                    Nenhuma cultura deste recorte tem demanda a ajustar.{' '}
                    <InfoTooltip rotulo="Por que não há composição" texto={SEM_DEMANDA} />
                  </td>
                </tr>
              ) : (
                porCultura.map((c) => <LinhaDaCultura key={c.culturaCodigo} c={c} />)
              )}
            </tbody>
          </table>
        </div>
      </PainelDoMomento>
    </div>
  );
}
