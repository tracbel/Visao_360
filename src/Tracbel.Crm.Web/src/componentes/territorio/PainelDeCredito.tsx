/**
 * O crédito rural de investimento, pelo SICOR do Banco Central (issue 68) — a
 * aba Crédito do Momento do mercado, na forma da maquete `momento-credito.png`
 * (fidelidade às maquetes, fase 3).
 *
 * O texto-base: "a nível município vai trazer todos os financiamentos
 * contratados, o nome do produto"; "pegar os últimos 12 meses e dividir pelos
 * meses anteriores"; "valor, ticket médio". Aqui estão as duas janelas e a
 * variação entre elas, na Região Tracbel e por município.
 *
 * LINHA NÃO É CONTRATO. O SICOR publica a soma dos contratos de cada
 * combinação de produto, programa e fonte, por município e mês — sem número
 * de contrato. A maquete diz "Linhas contratadas" e "Valor médio por operação";
 * a tela diz "Linhas do SICOR" e "Valor médio por linha" (decisão 2 do usuário).
 *
 * O QUE A MAQUETE NÃO MOSTRA CONTINUA A UM PASSO (decisão 3): a comparação
 * Município · Região Tracbel · São Paulo está na dica do valor financiado; a
 * janela exata e a carência, na dica da variação e do detalhamento; os produtos
 * financiados, na dica das linhas; e a lista inteira de municípios, com os de
 * fora da Região, em "Ver todos".
 *
 * OS MESES RECENTES AINDA MUDAM: o Banco Central acrescenta contrato registrado
 * com atraso. A dica da janela diz isso.
 */

import { ClipboardList, HandCoins, MapPin, TrendingUp, Wallet } from 'lucide-react';
import { useState } from 'react';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { obterCreditoRural } from '../../dados/api/territorio';
import { useRecurso } from '../../dados/api/useRecurso';
import type {
  CreditoDeMaquinasNoMunicipio,
  FaixaDeMercado,
  JanelaDoCredito,
  JanelasDeCredito,
  PainelDeCreditoRural,
} from '../../tipos/mercado';
import { BlocoCarregando, BlocoErro, BlocoVazio } from '../cadastro/EstadosDeTela';
import { GraficoLinhaMensal } from '../GraficoLinhaMensal';
import { InfoTooltip } from '../InfoTooltip';
import { MolduraDeGrafico } from '../MolduraDeGrafico';
import {
  CRITERIOS_DO_CREDITO,
  ordenarMunicipios,
  participacao,
  topDaRegiao,
  valorMedioAnterior,
  valorMedioPorLinha,
  variacao,
  type CriterioDoCredito,
} from '../mercado/momento/contas';
import { mesCurto, numero, percentualComSinal, reais, reaisCurtos, sentido, tomDoSentido } from '../mercado/momento/formatos';
import {
  CartaoDoMomento,
  FileiraDeCartoes,
  GraficoSemSerie,
  Legenda,
  LinhaDePaineis,
  MenuDaLinha,
  PainelDoMomento,
  Seletor,
  Variacao,
} from '../mercado/momento/pecas';

const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

const BASE = 'vs. 12 meses anteriores';
const SEM_BASE =
  'A janela anterior não tem linha do SICOR, e sem base não há variação a calcular — zero lá não quer dizer que o crédito "nasceu" agora.';

const O_QUE_E_LINHA =
  'Uma LINHA do SICOR não é um contrato: ela já é a soma dos contratos daquela combinação de município, produto, ' +
  'programa e fonte, e não traz quantidade. O SICOR não identifica cliente nem revenda.';

const NOME_DA_FAIXA: Record<FaixaDeMercado, string> = {
  Retraido: 'retraído',
  Intermediaria: 'intermediária',
  Aquecido: 'aquecido',
  Superaquecido: 'superaquecido',
};

/** A janela desta comparação, dita por extenso (issue 157). */
function textoDaJanela(janela: JanelaDoCredito): string {
  const recente = `${mesCurto(janela.inicio)} a ${mesCurto(janela.fim)}`;
  const anterior = `${mesCurto(janela.inicioAnterior)} a ${mesCurto(janela.fimAnterior)}`;
  const carencia = janela.carenciaDecidida
    ? janela.mesesDeCarencia > 0
      ? `os ${janela.mesesDeCarencia} ${janela.mesesDeCarencia === 1 ? 'mês mais recente ficou' : 'meses mais recentes ficaram'} de fora, porque o Banco Central ainda acrescenta contrato registrado com atraso`
      : 'a carência está decidida como zero: nenhum mês fica de fora'
    : 'carência não decidida: nenhum mês foi descartado, e por isso o mês mais recente pode aparecer abaixo do que será';
  return (
    `${recente} contra ${anterior} · ${janela.mesesPorJanela} meses de cada lado · o SICOR tem dado até ` +
    `${mesCurto(janela.ultimoMesComDado)} · ${carencia}.`
  );
}

/** Uma linha da comparação por recorte, dentro da dica. */
function LinhaDoRecorte({ nome, j }: { nome: string; j: JanelasDeCredito }) {
  const v = variacao(j.valor, j.valorAnterior);
  return (
    <tr>
      <td>{nome}</td>
      <td>{numero(j.linhas)}</td>
      <td>{reaisCurtos(j.valor)}</td>
      <td>{v === null ? '—' : percentualComSinal(v)}</td>
    </tr>
  );
}

/**
 * MUNICÍPIO · REGIÃO TRACBEL · SÃO PAULO, lado a lado (issue 168) — na dica do
 * valor financiado. Sem o estado ao lado, o número da Região é solto: ele pode
 * ser um terço de São Paulo ou um vigésimo.
 */
function ComparacaoDosRecortes({
  dados,
  escolhido,
}: {
  dados: PainelDeCreditoRural;
  escolhido: CreditoDeMaquinasNoMunicipio | null;
}) {
  return (
    <div className="mom-detalhe">
      <p>
        Valor financiado em MÁQUINAS (trator, máquinas e implementos, colheitadeiras) nos 12 meses da janela, na Região
        Tracbel inteira — a ADR, independente de sub-região e loja.
      </p>
      {dados.regiao && dados.saoPaulo && (
        <table>
          <thead>
            <tr>
              <th>Recorte</th>
              <th>Linhas</th>
              <th>Valor</th>
              <th>Variação</th>
            </tr>
          </thead>
          <tbody>
            {escolhido && <LinhaDoRecorte nome={`${escolhido.nome} (município escolhido)`} j={escolhido.janelas} />}
            <LinhaDoRecorte nome="Região Tracbel" j={dados.regiao.janelas} />
            <LinhaDoRecorte nome={dados.saoPaulo.recorte} j={dados.saoPaulo.janelas} />
          </tbody>
        </table>
      )}
      {dados.regiao && dados.saoPaulo && dados.saoPaulo.janelas.valor > 0 && (
        <p>
          A Região Tracbel é{' '}
          {percentualComSinal(dados.regiao.janelas.valor / dados.saoPaulo.janelas.valor, 1).replace('+', '')} do valor
          de São Paulo ({numero(dados.regiao.municipios)} municípios com linha na janela, de{' '}
          {numero(dados.saoPaulo.municipios)}).
        </p>
      )}
      {dados.janela && <p>Janela: {textoDaJanela(dados.janela)}</p>}
    </div>
  );
}

/** Os produtos financiados na janela, em São Paulo — na dica das linhas do SICOR. */
function ProdutosFinanciados({ dados }: { dados: PainelDeCreditoRural }) {
  const produtos = dados.porProduto.slice(0, 12);
  return (
    <div className="mom-detalhe">
      <p>{O_QUE_E_LINHA}</p>
      <p>
        <strong>Produtos financiados em São Paulo</strong> — a janela recente, contra os 12 meses anteriores:
      </p>
      <table>
        <thead>
          <tr>
            <th>Produto</th>
            <th>Linhas</th>
            <th>Valor</th>
            <th>Variação</th>
          </tr>
        </thead>
        <tbody>
          {produtos.map((p) => (
            <LinhaDoRecorte key={p.codigo} nome={p.ehMaquina ? `${p.nome} (máquina)` : p.nome} j={p.janelas} />
          ))}
        </tbody>
      </table>
    </div>
  );
}

function CelulaDaVariacao({ v }: { v: number | null }) {
  if (v === null) return <td className="mom-num">—</td>;
  const tom = tomDoSentido(v);
  return (
    <td className={`mom-num ${tom === 'alta' ? 'mom-alta' : tom === 'baixa' ? 'mom-baixa' : ''}`}>
      <span aria-hidden="true">{sentido(v)}</span> {percentualComSinal(v)}
    </td>
  );
}

function LinhaDoMunicipio({ m, escolhido }: { m: CreditoDeMaquinasNoMunicipio; escolhido: boolean }) {
  const medio = valorMedioPorLinha(m.janelas);
  return (
    <tr data-municipio={m.codigoIbge} data-escolhido={escolhido ? 'true' : undefined}>
      <th scope="row">
        <span className="mom-cultura">
          <MapPin size={15} strokeWidth={2.2} color="#2E7D32" aria-hidden="true" />
          <span>{m.nome}</span>
          {escolhido && <span className="cad-so-leitor"> — município escolhido</span>}
        </span>
      </th>
      <td className="mom-num">{numero(m.janelas.linhas)}</td>
      <td className="mom-num">{reais(m.janelas.valor)}</td>
      <td className="mom-num">{medio === null ? '—' : reais(medio)}</td>
      <CelulaDaVariacao v={variacao(m.janelas.valor, m.janelas.valorAnterior)} />
      <td className="mom-acoes">
        <MenuDaLinha rotulo={`O crédito de ${m.nome}`}>
          <dl>
            <dt>Região Tracbel</dt>
            <dd>{m.pertenceAAdr ? 'sim' : 'não — fora da área de atuação'}</dd>
            <dt>Linhas do SICOR</dt>
            <dd>
              {numero(m.janelas.linhas)} na janela, contra {numero(m.janelas.linhasAnteriores)} nos 12 meses anteriores
            </dd>
            <dt>Valor anterior</dt>
            <dd>{reais(m.janelas.valorAnterior)}</dd>
            <dt>Valor médio anterior</dt>
            <dd>{valorMedioAnterior(m.janelas) === null ? '—' : reais(valorMedioAnterior(m.janelas)!)}</dd>
            {m.indice && (
              <>
                <dt>Índice de crédito</dt>
                <dd>
                  {m.indice.indice === null ? '—' : numero(m.indice.indice, 2)}
                  {m.indice.faixa ? ` · ${NOME_DA_FAIXA[m.indice.faixa]}` : ''}
                  {m.indice.basePequena ? ' · base pequena, leia com cuidado' : ''}
                </dd>
              </>
            )}
          </dl>
          <p>{O_QUE_E_LINHA}</p>
        </MenuDaLinha>
      </td>
    </tr>
  );
}

/**
 * O CRÉDITO REAGE AO MUNICÍPIO (issue 168). O SICOR publica por município, então
 * aqui o recorte escolhido muda o que se lê: ele entra na comparação da dica e
 * sobe para o topo do detalhamento, mesmo fora da Região Tracbel. Preço e custo
 * não fazem isto porque a fonte deles é estadual.
 */
export function PainelDeCredito({ municipioSelecionado = null }: { municipioSelecionado?: number | null } = {}) {
  const { contexto } = useContextoDeAcesso();
  const credito = useRecurso((sinal) => obterCreditoRural(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const [criterioDoTop, setCriterioDoTop] = useState<'valor' | 'linhas'>('valor');
  const [ordem, setOrdem] = useState<CriterioDoCredito>('valor');
  const [todos, setTodos] = useState(false);
  const [foraDaRegiao, setForaDaRegiao] = useState(false);

  if (credito.carregando) return <BlocoCarregando oQue="o crédito rural" />;
  if (credito.erro) return <BlocoErro erro={credito.erro} aoTentarDeNovo={credito.recarregar} />;

  const dados = credito.dados;
  if (!dados || !dados.ultimoMes || !dados.janela)
    return (
      <BlocoVazio
        titulo="O crédito rural ainda não foi carregado neste banco"
        texto={
          <>
            A rota respondeu, mas não há nenhuma linha do SICOR gravada: a carga (<code>--somente-credito</code>) ainda
            não rodou aqui. Não é zero nem falta de permissão.
          </>
        }
      />
    );

  const janela = dados.janela;
  const regiao = dados.regiao?.janelas ?? null;
  const escolhido =
    municipioSelecionado === null ? null : (dados.porMunicipio.find((m) => m.codigoIbge === municipioSelecionado) ?? null);

  const variacaoDoValor = regiao ? variacao(regiao.valor, regiao.valorAnterior) : null;
  const medio = regiao ? valorMedioPorLinha(regiao) : null;
  const medioAntes = regiao ? valorMedioAnterior(regiao) : null;

  // ---------- o top 5 da Região Tracbel ----------
  const top = topDaRegiao(dados.porMunicipio, criterioDoTop);
  const totalDoTop = criterioDoTop === 'valor' ? (regiao?.valor ?? 0) : (regiao?.linhas ?? 0);
  const maiorDoTop = Math.max(1, ...top.map((m) => (criterioDoTop === 'valor' ? m.janelas.valor : m.janelas.linhas)));

  // ---------- o detalhamento: o escolhido primeiro, depois a ordem pedida ----------
  const candidatos = dados.porMunicipio.filter(
    (m) => (foraDaRegiao || m.pertenceAAdr) && m.codigoIbge !== escolhido?.codigoIbge,
  );
  const ordenados = ordenarMunicipios(candidatos, ordem);
  const visiveis = todos ? ordenados : ordenados.slice(0, escolhido ? 4 : 5);
  const daRegiao = dados.porMunicipio.filter((m) => m.pertenceAAdr).length;

  // ---------- a evolução: a série ANUAL que existe ----------
  const anos = dados.porAno;
  const mesFim = Number(dados.ultimoMes.split('-')[1]);

  return (
    <div className="mom-painel-da-aba" data-bloco="credito">
      <FileiraDeCartoes>
        <CartaoDoMomento
          icone={HandCoins}
          tom="verde"
          rotulo="Valor total financiado"
          oQue="o crédito rural"
          dica={<ComparacaoDosRecortes dados={dados} escolhido={escolhido} />}
          procedencia={dados.procedencia}
          valor={regiao ? reaisCurtos(regiao.valor) : null}
          motivoSemDado="A leitura do SICOR não trouxe a soma da Região Tracbel: a área de atuação não tem município com linha na janela, ou o território não foi carregado."
          apoio={
            <Variacao fracao={variacaoDoValor} base={BASE} motivoSemBase={SEM_BASE} oQue="a variação do valor financiado" />
          }
        />

        <CartaoDoMomento
          icone={ClipboardList}
          tom="azul"
          rotulo="Linhas do SICOR"
          oQue="a linha do SICOR"
          dica={<ProdutosFinanciados dados={dados} />}
          valor={regiao ? numero(regiao.linhas) : null}
          motivoSemDado="Sem a soma da Região Tracbel não há linhas a contar."
          apoio={
            <Variacao
              fracao={regiao ? variacao(regiao.linhas, regiao.linhasAnteriores) : null}
              base={BASE}
              motivoSemBase={SEM_BASE}
              oQue="a variação das linhas"
            />
          }
        />

        <CartaoDoMomento
          icone={Wallet}
          tom="roxo"
          rotulo="Valor médio por linha"
          oQue="o valor médio por linha"
          dica={
            'Valor financiado dividido pelo número de LINHAS do SICOR, na Região Tracbel. Não é ticket médio nem valor ' +
            'por operação: ' +
            O_QUE_E_LINHA
          }
          valor={medio === null ? null : reaisCurtos(medio)}
          motivoSemDado="Sem linha do SICOR na janela não há o que dividir."
          apoio={
            <Variacao
              fracao={medio !== null && medioAntes !== null ? variacao(medio, medioAntes) : null}
              base={BASE}
              motivoSemBase={SEM_BASE}
              oQue="a variação do valor médio"
            />
          }
        />

        <CartaoDoMomento
          icone={TrendingUp}
          tom="neutro"
          rotulo="Variação anual"
          oQue="a variação anual do valor financiado"
          dica={
            <div className="mom-detalhe">
              <p>
                Os 12 meses da janela recente contra os 12 anteriores, no valor financiado em máquinas da Região
                Tracbel.
              </p>
              <p>Janela: {textoDaJanela(janela)}</p>
            </div>
          }
          valor={
            variacaoDoValor === null ? null : (
              <span className="mom-sentido" data-sentido={tomDoSentido(variacaoDoValor)}>
                <span aria-hidden="true">{sentido(variacaoDoValor)}</span> {percentualComSinal(variacaoDoValor)}
              </span>
            )
          }
          motivoSemDado={SEM_BASE}
          apoio="no valor financiado"
        />
      </FileiraDeCartoes>

      <LinhaDePaineis variante="credito">
        <PainelDoMomento
          titulo="Evolução do valor financiado"
          dica={
            'A maquete mostra a série MENSAL da região, dois anos sobrepostos. O SICOR chega a esta tela somado por ANO ' +
            'e para São Paulo inteiro — máquinas: trator, máquinas e implementos e colheitadeiras. Desenhar meses ' +
            `seria fingir um detalhe que a leitura não traz. ${anos.at(-1)?.ano ?? ''} vai até ${MESES[mesFim - 1] ?? '—'}, ` +
            'e por isso o último ponto aparece tracejado.'
          }
          direita={<Legenda itens={[{ nome: 'Máquinas · São Paulo, por ano', cor: '#367C2B' }]} />}
        >
          {anos.length > 1 ? (
            <MolduraDeGrafico altura={210}>
              {(l, a) => (
                <GraficoLinhaMensal
                  rotulos={anos.map((ano) => String(ano.ano))}
                  valores={anos.map((ano) => ano.valorDeMaquinas)}
                  largura={l}
                  altura={a}
                  formatar={reaisCurtos}
                  ultimoParcial={(anos.at(-1)?.ultimoMes ?? 12) < 12}
                />
              )}
            </MolduraDeGrafico>
          ) : (
            <GraficoSemSerie
              altura={210}
              frase="Sem série anual"
              motivo="O SICOR carregado tem menos de dois anos: não há linha para desenhar."
              oQue="a evolução do valor financiado"
            />
          )}
        </PainelDoMomento>

        <PainelDoMomento
          titulo={`Top 5 municípios em ${criterioDoTop === 'valor' ? 'valor financiado' : 'linhas do SICOR'}`}
          dica={
            'Só municípios da Região Tracbel — o SICOR publica São Paulo inteiro, e um vizinho grande entraria numa ' +
            'lista que promete a sua área de atuação. Participação é a fatia do município no total da Região Tracbel ' +
            'na mesma janela: fatia do crédito, e não participação de mercado.'
          }
          direita={
            <Seletor
              rotuloVisivel={false}
              rotulo="Ordenar o top 5"
              valor={criterioDoTop}
              opcoes={[
                { id: 'valor', rotulo: 'Por valor (R$)' },
                { id: 'linhas', rotulo: 'Por linhas do SICOR' },
              ]}
              aoMudar={setCriterioDoTop}
            />
          }
        >
          {top.length === 0 ? (
            <GraficoSemSerie
              altura={180}
              frase="Nenhum município da Região Tracbel"
              motivo="Nenhum município da área de atuação tem linha de máquina do SICOR na janela — ou o território não foi carregado."
              oQue="o top 5"
            />
          ) : (
            <ol className="mom-ranking" data-numerado="true" data-bloco="credito-top5">
              <li className="mom-ranking-linha mom-ranking-cabecalho" aria-hidden="true">
                <span className="mom-ranking-posicao">#</span>
                <span>Município</span>
                <span className="mom-barra-vazia" />
                <span className="mom-num">{criterioDoTop === 'valor' ? 'Valor financiado' : 'Linhas'}</span>
                <span className="mom-num">Participação</span>
              </li>
              {top.map((m, i) => {
                const v = criterioDoTop === 'valor' ? m.janelas.valor : m.janelas.linhas;
                const fatia = participacao(v, totalDoTop);
                return (
                  <li key={m.codigoIbge} className="mom-ranking-linha" data-municipio={m.codigoIbge}>
                    <span className="mom-ranking-posicao" aria-hidden="true">
                      {i + 1}
                    </span>
                    <span className="mom-ranking-nome">{m.nome}</span>
                    <span className="mom-barra" aria-hidden="true">
                      <span
                        style={{
                          width: `${Math.max(2, (v / maiorDoTop) * 100)}%`,
                          background: i === 0 ? '#1E7B34' : i < 3 ? '#3E9B57' : '#8FD19E',
                        }}
                      />
                    </span>
                    <span className="mom-ranking-valor">{criterioDoTop === 'valor' ? reaisCurtos(v) : numero(v)}</span>
                    <span className="mom-ranking-participacao">
                      {fatia === null ? '—' : `${numero(fatia * 100, 0)}%`}
                      <span className="cad-so-leitor"> da Região Tracbel</span>
                    </span>
                  </li>
                );
              })}
            </ol>
          )}
        </PainelDoMomento>
      </LinhaDePaineis>

      <PainelDoMomento
        titulo="Detalhamento por município"
        dica={
          <div className="mom-detalhe">
            <p>
              Crédito de máquinas por município, com a variação sobre os 12 meses anteriores. O município escolhido no
              filtro sobe para o topo, mesmo fora da Região Tracbel.
            </p>
            <p>Janela: {textoDaJanela(janela)}</p>
            <p>{O_QUE_E_LINHA}</p>
          </div>
        }
        direita={
          <>
            {todos && (
              <label className="mom-seletor">
                <input type="checkbox" checked={foraDaRegiao} onChange={(e) => setForaDaRegiao(e.target.checked)} />
                incluir municípios fora da Região Tracbel
              </label>
            )}
            <button type="button" className="mom-link" aria-expanded={todos} onClick={() => setTodos((t) => !t)}>
              {todos ? 'Ver só os 5 primeiros' : `Ver todos (${numero(daRegiao)})`}
            </button>
            <Seletor rotulo="Ordenar por" valor={ordem} opcoes={CRITERIOS_DO_CREDITO} aoMudar={setOrdem} />
          </>
        }
      >
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela" data-bloco="credito-municipios">
            <thead>
              <tr>
                <th scope="col">Município</th>
                <th scope="col" className="mom-num">
                  Linhas do SICOR <InfoTooltip rotulo="O que a coluna Linhas do SICOR conta" texto={O_QUE_E_LINHA} />
                </th>
                <th scope="col" className="mom-num">Valor financiado (R$)</th>
                <th scope="col" className="mom-num">
                  Valor médio (R$){' '}
                  <InfoTooltip
                    rotulo="O que é o valor médio por linha"
                    texto={`Valor financiado dividido pelo número de LINHAS do SICOR. Não é ticket médio: ${O_QUE_E_LINHA}`}
                  />
                </th>
                <th scope="col" className="mom-num">Variação anual</th>
                <th scope="col" className="mom-acoes">
                  <span className="cad-so-leitor">Detalhes</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {escolhido && <LinhaDoMunicipio m={escolhido} escolhido />}
              {visiveis.map((m) => (
                <LinhaDoMunicipio key={m.codigoIbge} m={m} escolhido={false} />
              ))}
              {!escolhido && visiveis.length === 0 && (
                <tr className="mom-tabela-vazia">
                  <td colSpan={6}>Nenhum município da Região Tracbel tem linha de máquina do SICOR na janela.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </PainelDoMomento>
    </div>
  );
}
