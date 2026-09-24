/**
 * PERCEPÇÃO COMERCIAL — a aba da maquete `momento-percepcao-comercial.png`
 * (fidelidade às maquetes, fase 3).
 *
 * O QUE EXISTE (issue 71): a leitura do gestor sobre cada município, em pontos
 * percentuais de −5 a +5 (D-P04), com vigência e autor — e a do recorte, que
 * entra no fator. É isso que a aba mostra: a leitura do recorte, quantos
 * municípios estão de cada lado do zero, os cinco de maior leitura e a tabela.
 *
 * O QUE NÃO EXISTE, e fica com o traço e o motivo no lugar da maquete: o índice
 * de 0 a 100 (a escala registrada é outra, e a tela não converte uma na outra),
 * a comparação com o ano anterior, a tendência declarada para os próximos
 * meses, a série mensal e o impacto em reais. Tudo isso "aguarda a coleta da
 * percepção comercial por município" (issue 71) — ou o preço de máquina (issue
 * 70), no caso do impacto.
 *
 * SEM ADJETIVO QUE NÃO TENHA REGRA: "aquecido", "em alerta" e "muito positiva"
 * pedem limites que ninguém registrou. A leitura é positiva, neutra ou negativa
 * pelo SINAL — que é aritmética, e não decisão.
 *
 * A COLUNA "GESTOR" NUNCA TEM NOME INVENTADO (issue 191). O nome que aparece é
 * o do responsável pela carteira do CRM com vínculo no município, que já vem na
 * leitura da tela; sem ele, o traço.
 */

import { Download, Smile, ThumbsUp, TriangleAlert, Users } from 'lucide-react';
import { useMemo } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { obterParametrosDoPotencial } from '../../../dados/api/potencial';
import { useRecurso } from '../../../dados/api/useRecurso';
import { baixarCsv, carimboDeData } from '../../../dados/exportarCsv';
import type { PercepcaoDoGestorDetalhe } from '../../../tipos/potencial';
import type { IndicadoresDoMunicipio, MomentoDoRecorte } from '../../../tipos/territorio';
import { BlocoErro } from '../../cadastro/EstadosDeTela';
import { InfoTooltip } from '../../InfoTooltip';
import type { RecorteFiltrado } from '../../territorio/indicadoresDaAdr';
import { distribuicaoDaPercepcao, responsavelPrincipal, sentidoDaLeitura } from './contas';
import { dataCurta, numero, pontosPercentuais, sentido, tomDoSentido } from './formatos';
import {
  CartaoDoMomento,
  FileiraDeCartoes,
  GraficoSemSerie,
  LinhaDePaineis,
  MenuDaLinha,
  PainelDoMomento,
  Seletor,
} from './pecas';
import { Rosca } from './Rosca';
// A MESMA LINHA "— vs. ano anterior ⓘ" DOS CARTÕES DE CIMA DA TELA (fase 2), com
// o motivo desta aba: aqui o que falta é a série da percepção (issue 71).
import { VariacaoAusente } from '../VariacaoAusente';

const AGUARDA_A_COLETA =
  'Aguarda a coleta da percepção comercial por município (issue 71): hoje o que se registra é a leitura atual do ' +
  'gestor, com vigência — não há série guardada mês a mês, e a tela não reconstrói uma a partir das vigências.';

const SEM_ANO_ANTERIOR = `Sem a leitura do ano anterior não há variação a calcular. ${AGUARDA_A_COLETA}`;

// A DICA FALA COM QUEM USA A TELA, e não com quem a desenhou: "a maquete
// mostra" não significa nada para o gerente. O que ele precisa saber é que o
// índice de 0 a 100 não existe.
const ESCALA =
  'A leitura do gestor é registrada em pontos percentuais, de −5 a +5 (D-P04, issue 71). Não existe um índice de ' +
  '0 a 100 para ela, e a tela não converte uma escala na outra — isso seria inventar a régua.';

const CORES = { positiva: '#2E7D32', neutra: '#E7B416', negativa: '#E53935', semRegistro: '#C4CBC6' } as const;

const NOME_DO_SENTIDO = { positiva: 'Positiva', neutra: 'Neutra', negativa: 'Negativa' } as const;

/** A barra divergente da leitura: o zero no meio, a barra para o lado do sinal. */
function BarraDivergente({ percentual, limite }: { percentual: number; limite: number }) {
  const metade = Math.min(50, (Math.abs(percentual) / limite) * 50);
  return (
    <span className="mom-divergente" aria-hidden="true">
      <span
        data-negativa={percentual < 0 ? 'true' : undefined}
        style={percentual < 0 ? { right: '50%', width: `${metade}%` } : { left: '50%', width: `${metade}%` }}
      />
    </span>
  );
}

export function AbaPercepcaoComercial({
  momento,
  municipios = [],
  recorte = null,
}: {
  momento: MomentoDoRecorte | null;
  municipios?: readonly IndicadoresDoMunicipio[];
  /**
   * O recorte dos filtros, quando há. Os municípios já vêm filtrados por ele, e
   * "Municípios da Região Tracbel" sobre uma sub-região seria o nome da área de
   * atuação inteira num pedaço dela (issue 163).
   */
  recorte?: RecorteFiltrado | null;
}) {
  const da = recorte?.da ?? 'da Região Tracbel';
  const { contexto } = useContextoDeAcesso();
  const parametros = useRecurso(
    (sinal) => obterParametrosDoPotencial(contexto, undefined, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const daRegiao = useMemo(() => municipios.filter((m) => m.pertenceAAdr), [municipios]);
  const porCodigo = useMemo(() => new Map(municipios.map((m) => [m.codigoIbge, m])), [municipios]);

  /**
   * AS LEITURAS DA REGIÃO TRACBEL NO RECORTE, da maior para a menor. Sem a
   * lista de municípios (território não carregado), entram todas as vigentes —
   * em vez de a aba sumir por falta do filtro.
   */
  const leituras = useMemo(() => {
    const codigos = new Set(daRegiao.map((m) => m.codigoIbge));
    return [...(parametros.dados?.percepcoes ?? [])]
      .filter((p) => codigos.size === 0 || codigos.has(p.municipioCodigoIbge))
      .sort((a, b) => b.percentual - a.percentual || a.municipioNome.localeCompare(b.municipioNome, 'pt-BR'));
  }, [parametros.dados, daRegiao]);

  const distribuicao = distribuicaoDaPercepcao(leituras, daRegiao.length);
  const limite = Math.max(5, ...leituras.map((l) => Math.abs(l.percentual)));
  const top = leituras.slice(0, 5);
  const semLeitura = parametros.erro
    ? 'A leitura das percepções registradas não respondeu — ela exige a permissão de leitura dos parâmetros do potencial.'
    : parametros.carregando
      ? 'Lendo as percepções registradas…'
      : `Nenhum município ${da} tem percepção registrada (issue 71). ${AGUARDA_A_COLETA}`;
  const lido = !parametros.carregando && !parametros.erro;

  const gestorDe = (p: PercepcaoDoGestorDetalhe) =>
    responsavelPrincipal(porCodigo.get(p.municipioCodigoIbge)?.responsaveisPelasCarteiras ?? []);

  function exportar() {
    baixarCsv(
      `percepcao-comercial-${carimboDeData()}`,
      ['Município', 'Leitura (p.p.)', 'Sentido', 'Responsável pela carteira', 'Vigente desde'],
      leituras.map((p) => [
        p.municipioNome,
        p.percentual.toLocaleString('pt-BR'),
        NOME_DO_SENTIDO[sentidoDaLeitura(p.percentual)],
        gestorDe(p)?.nome ?? '',
        dataCurta(p.vigencia.vigenteDesde),
      ]),
    );
  }

  const percepcao = momento?.percepcaoPercentual ?? null;
  const fatias = [
    { id: 'positiva', nome: 'Positiva (> 0)', valor: distribuicao.positiva, cor: CORES.positiva },
    { id: 'neutra', nome: 'Neutra (= 0)', valor: distribuicao.neutra, cor: CORES.neutra },
    { id: 'negativa', nome: 'Negativa (< 0)', valor: distribuicao.negativa, cor: CORES.negativa },
    { id: 'semRegistro', nome: 'Sem leitura', valor: distribuicao.semRegistro, cor: CORES.semRegistro },
  ] as const;

  return (
    <div className="mom-painel-da-aba" data-bloco="percepcao-comercial">
      <FileiraDeCartoes>
        <CartaoDoMomento
          icone={Smile}
          tom="verde"
          rotulo="Percepção no recorte"
          oQue="a percepção no recorte"
          dica={`A leitura do gestor que entra no fator do recorte. ${ESCALA}`}
          valor={
            percepcao === null ? null : (
              <span className="mom-sentido" data-sentido={tomDoSentido(percepcao / 100)}>
                <span aria-hidden="true">{sentido(percepcao / 100)}</span> {pontosPercentuais(percepcao)}
              </span>
            )
          }
          unidade="de −5 a +5"
          motivoSemDado={`Nenhuma percepção do gestor está registrada para este recorte. ${AGUARDA_A_COLETA}`}
          apoio={<VariacaoAusente deQue="percepção no recorte" motivo={SEM_ANO_ANTERIOR} />}
        />
        <CartaoDoMomento
          icone={ThumbsUp}
          tom="verde"
          rotulo="Percepção positiva"
          oQue="os municípios com percepção positiva"
          dica={`Municípios ${da} com leitura registrada ACIMA de zero. É o sinal da leitura, e não uma faixa: não há limite registrado para chamar um município de aquecido.`}
          valor={lido ? numero(distribuicao.positiva) : null}
          unidade={distribuicao.positiva === 1 ? 'município' : 'municípios'}
          motivoSemDado={semLeitura}
          apoio={<VariacaoAusente deQue="municípios com percepção positiva" motivo={SEM_ANO_ANTERIOR} />}
        />
        <CartaoDoMomento
          icone={TriangleAlert}
          tom="laranja"
          rotulo="Percepção negativa"
          oQue="os municípios com percepção negativa"
          dica={`Municípios ${da} com leitura registrada ABAIXO de zero. É o sinal da leitura, e não uma faixa: não há limite registrado para chamar um município de em alerta.`}
          valor={lido ? numero(distribuicao.negativa) : null}
          unidade={distribuicao.negativa === 1 ? 'município' : 'municípios'}
          motivoSemDado={semLeitura}
          apoio={<VariacaoAusente deQue="municípios com percepção negativa" motivo={SEM_ANO_ANTERIOR} />}
        />
        <CartaoDoMomento
          icone={Users}
          tom="roxo"
          rotulo="Tendência dos gestores"
          oQue="a tendência dos gestores"
          valor={null}
          motivoSemDado={`O registro de hoje é a leitura atual, sem horizonte declarado. ${AGUARDA_A_COLETA}`}
          apoio="para os próximos 3 meses"
        />
      </FileiraDeCartoes>

      {parametros.erro && <BlocoErro erro={parametros.erro} aoTentarDeNovo={parametros.recarregar} />}

      <LinhaDePaineis variante="percepcao">
        <PainelDoMomento
          titulo="Distribuição da percepção comercial"
          dica={`Os municípios ${da}, pelo sinal da leitura registrada. ${ESCALA}`}
          subtitulo={`Municípios ${recorte?.daCurto ?? 'da Região Tracbel'} por sentido da leitura registrada.`}
        >
          <div className="mom-rosca-e-lista">
            <Rosca
              fatias={fatias}
              centro={numero(distribuicao.total)}
              legendaDoCentro={distribuicao.total === 1 ? 'município' : 'municípios'}
            />
            <table className="mom-faixas">
              <thead>
                <tr>
                  <th scope="col">
                    <span className="cad-so-leitor">Sentido</span>
                  </th>
                  <th scope="col" className="mom-num">
                    %
                  </th>
                  <th scope="col" className="mom-num">
                    Municípios
                  </th>
                </tr>
              </thead>
              <tbody>
                {fatias.map((f) => (
                  <tr key={f.id} data-sentido-da-leitura={f.id}>
                    <th scope="row">
                      <span className="mom-ponto" style={{ '--mom-cor': f.cor } as React.CSSProperties} aria-hidden="true" />
                      {f.nome}
                    </th>
                    <td className="mom-num">
                      <strong>{distribuicao.total > 0 ? `${numero((f.valor / distribuicao.total) * 100, 0)}%` : '—'}</strong>
                    </td>
                    <td className="mom-num">{numero(f.valor)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Top 5 municípios por percepção comercial"
          dica={`Os cinco municípios ${da} de maior leitura registrada. ${ESCALA}`}
          subtitulo="Municípios com a maior leitura registrada."
        >
          {top.length === 0 ? (
            <GraficoSemSerie altura={170} frase="Nenhuma leitura registrada" motivo={semLeitura} oQue="o top 5 da percepção" />
          ) : (
            <ol className="mom-ranking" data-numerado="true" data-colunas="4" data-bloco="percepcao-top5">
              <li className="mom-ranking-linha mom-ranking-cabecalho" aria-hidden="true">
                <span className="mom-ranking-posicao">#</span>
                <span>Município</span>
                <span className="mom-num">Leitura (p.p.)</span>
                <span className="mom-barra-vazia" />
              </li>
              {top.map((p, i) => (
                <li key={p.municipioCodigoIbge} className="mom-ranking-linha" data-municipio={p.municipioCodigoIbge}>
                  <span className="mom-ranking-posicao" aria-hidden="true">
                    {i + 1}
                  </span>
                  <span className="mom-ranking-nome">{p.municipioNome}</span>
                  <span className="mom-ranking-valor">{pontosPercentuais(p.percentual)}</span>
                  <span className="mom-barra" aria-hidden="true">
                    <span
                      data-negativa={p.percentual < 0 ? 'true' : undefined}
                      style={{
                        width: `${Math.max(2, (Math.abs(p.percentual) / limite) * 100)}%`,
                        background: p.percentual < 0 ? undefined : i === 0 ? '#1E7B34' : i < 3 ? '#3E9B57' : '#8FD19E',
                      }}
                    />
                  </span>
                </li>
              ))}
            </ol>
          )}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Evolução da percepção comercial"
          dica={`A leitura média dos gestores sobre os municípios ${da}, mês a mês. ${AGUARDA_A_COLETA}`}
          subtitulo={`Leitura média dos gestores ${recorte ? `no recorte (${recorte.nome})` : 'na Região Tracbel'}.`}
          direita={
            <Seletor
              rotulo="Período"
              rotuloVisivel={false}
              valor="12"
              opcoes={[{ id: '12', rotulo: 'Últimos 12 meses' }]}
              motivoDesligado={`Não há série para escolher o período. ${AGUARDA_A_COLETA}`}
            />
          }
        >
          <GraficoSemSerie altura={170} frase="Sem série mensal" motivo={AGUARDA_A_COLETA} oQue="a evolução da percepção" />
        </PainelDoMomento>
      </LinhaDePaineis>

      <PainelDoMomento
        titulo="Percepção comercial por município"
        dica={
          `A leitura registrada de cada município ${da}, da maior para a menor. ${ESCALA} ` +
          'Gestor é o responsável pela carteira do CRM com mais vínculos no município (issue 107) — nunca um nome ' +
          'escrito para preencher a coluna.'
        }
        subtitulo="A leitura registrada de cada município, com o responsável pela carteira no CRM."
        direita={
          <button type="button" className="mom-botao" onClick={exportar} disabled={leituras.length === 0}>
            <Download size={15} strokeWidth={2} aria-hidden="true" />
            Exportar
          </button>
        }
      >
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela" data-bloco="percepcao-municipios">
            <thead>
              <tr>
                <th scope="col">Município</th>
                <th scope="col">Percepção</th>
                <th scope="col">
                  Leitura (−5 a +5 p.p.) <InfoTooltip rotulo="Por que não há índice de 0 a 100" texto={ESCALA} />
                </th>
                <th scope="col">
                  Gestor{' '}
                  <InfoTooltip
                    rotulo="De onde vem o nome do gestor"
                    texto="É o responsável pela carteira do CRM com mais vínculos no município (issue 107). Onde nenhuma carteira tem responsável cadastrado, a célula fica com o traço — a tela nunca escreve um nome para preencher a coluna."
                  />
                </th>
                <th scope="col">
                  Tendência (3 meses){' '}
                  <InfoTooltip rotulo="Por que a tendência não aparece" texto={AGUARDA_A_COLETA} />
                </th>
                <th scope="col">
                  Impacto estimado{' '}
                  <InfoTooltip
                    rotulo="Por que o impacto estimado não aparece"
                    texto="O impacto em reais multiplica a leitura pela demanda e pelo preço de máquina — e o preço de máquina por modelo é a issue 70, que não existe no CRM."
                  />
                </th>
                <th scope="col" className="mom-acoes">
                  <span className="cad-so-leitor">Detalhes</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {leituras.length === 0 && (
                <tr className="mom-tabela-vazia">
                  <td colSpan={7}>
                    {parametros.carregando ? 'Lendo as percepções registradas…' : 'Nenhum município com percepção registrada.'}{' '}
                    {!parametros.carregando && <InfoTooltip rotulo="Por que a tabela está vazia" texto={semLeitura} />}
                  </td>
                </tr>
              )}
              {leituras.map((p) => {
                const s = sentidoDaLeitura(p.percentual);
                const tom = s === 'positiva' ? 'alta' : s === 'negativa' ? 'baixa' : 'neutro';
                const gestor = gestorDe(p);
                const responsaveis = porCodigo.get(p.municipioCodigoIbge)?.responsaveisPelasCarteiras ?? [];
                return (
                  <tr key={p.municipioCodigoIbge} data-municipio={p.municipioCodigoIbge}>
                    <th scope="row">{p.municipioNome}</th>
                    <td>
                      <span className="mom-leitura" data-sentido={tom}>
                        <span className="mom-ponto" style={{ '--mom-cor': CORES[s] } as React.CSSProperties} aria-hidden="true" />
                        <span>{NOME_DO_SENTIDO[s]}</span>
                      </span>
                    </td>
                    <td>
                      <strong className="mom-leitura-valor">{pontosPercentuais(p.percentual)}</strong>
                      <BarraDivergente percentual={p.percentual} limite={limite} />
                    </td>
                    <td data-gestor>
                      {gestor ? (
                        <>
                          {gestor.nome}
                          {gestor.outros > 0 && <span className="mom-painel-subtitulo"> +{gestor.outros}</span>}
                        </>
                      ) : (
                        <>
                          <span aria-hidden="true">—</span>
                          <span className="cad-so-leitor">sem responsável cadastrado</span>
                        </>
                      )}
                    </td>
                    <td>
                      <span aria-hidden="true">—</span>
                      <span className="cad-so-leitor">sem dado</span>
                    </td>
                    <td>
                      <span aria-hidden="true">—</span>
                      <span className="cad-so-leitor">sem dado</span>
                    </td>
                    <td className="mom-acoes">
                      <MenuDaLinha rotulo={`A percepção de ${p.municipioNome}`}>
                        <dl>
                          <dt>Vigente desde</dt>
                          <dd>{dataCurta(p.vigencia.vigenteDesde)}</dd>
                          <dt>Justificativa</dt>
                          <dd>{p.vigencia.justificativa || '—'}</dd>
                          {responsaveis.length > 0 && (
                            <>
                              <dt>Carteiras</dt>
                              <dd>
                                {responsaveis
                                  .map((r) => `${r.nome} (${r.vinculos} ${r.vinculos === 1 ? 'vínculo' : 'vínculos'})`)
                                  .join(', ')}
                              </dd>
                            </>
                          )}
                        </dl>
                      </MenuDaLinha>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </PainelDoMomento>
    </div>
  );
}
