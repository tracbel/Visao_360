/**
 * O 360 de um cliente, montado **inteiramente do nosso banco pela API**.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — substitui o `Cliente360` que lia JSON do protótipo.
 *
 * O antigo montava a ficha de `cliente-84391.json`, o pós-vendas de
 * `pos-vendas-cliente-84391.json` e a oportunidade de `oportunidade-1517613.json`
 * — **um cliente só tinha ficha completa**, e os outros 22 da carteira
 * mostravam estado vazio. Agora qualquer um dos 23.945 clientes carregados abre
 * com o que existe dele, em cinco leituras paralelas.
 *
 * ---------------------------------------------------------------------------
 * 27/09/2026 — O DADO CRUZADO: CRM × Protheus × ART.
 *
 * Três blocos deixaram de ser lacuna, porque o dado estava no banco e nenhuma
 * tela o lia: a FROTA pelo dono atual da sincronia do parque (22.287 máquinas de
 * 6.165 clientes, contra 2.440 de 1.181 pelo dono confirmado), com a relação e a
 * evidência de cada máquina; o FATURAMENTO do cliente, lido da SD2 do Protheus,
 * com a data da carga; e as CARTEIRAS com o CEN de cada uma. O bloco de
 * faturamento substitui uma frase falsa ("parou em 11/04/2025"), que era da cópia
 * do Vórtice, e não do faturamento.
 *
 * OS BLOCOS QUE NÃO EXISTEM CONTINUAM NA TELA: títulos em aberto e ordens de
 * serviço, que o CRM ainda não carrega do Protheus. Some-los faria a tela
 * parecer completa; o que fica é o bloco, vazio, com o motivo de hoje.
 */


import { Link } from 'react-router-dom';
import { obterCliente } from '../../dados/api/clientes';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { CONSULTA_INICIAL as EQUIPAMENTOS_INICIAL, listarEquipamentos } from '../../dados/api/equipamentos';
import {
  INTERACOES_INICIAL,
  listarInteracoes,
  listarProcessos,
  listarTarefas,
  PROCESSOS_INICIAL,
  TAREFAS_INICIAL,
} from '../../dados/api/relacionamento';
import { useRecurso, type Leitura } from '../../dados/api/useRecurso';
import { formatarData, formatarDataHora, formatarDinheiro } from '../../telas/cadastro/formato';
import { RelacaoComOCliente } from '../cadastro/RelacaoDaMaquina';
import { LACUNAS_DA_FICHA_DO_CLIENTE, LacunaConhecida } from '../cadastro/SemDado';
import { SeloProcedencia } from '../cadastro/SeloProcedencia';
import { BlocoCarteirasDoCliente } from './BlocoCarteirasDoCliente';
import { BlocoFaturamentoDoCliente } from './BlocoFaturamentoDoCliente';
import { BlocoPainel, type EstadoBloco } from './BlocoPainel';
import { Dado, SemValor } from './DadoDoPainel';

/** Quantas linhas cada bloco mostra antes de mandar para a tela cheia. */
const LINHAS = 8;

export function Cliente360Api({ chave, aoLimpar }: { chave: string; aoLimpar: () => void }) {
  const { contexto } = useContextoDeAcesso();

  const cliente = useRecurso((sinal) => obterCliente(contexto, chave, sinal), [contexto.empresa, chave]);

  const frota = useRecurso(
    (sinal) =>
      listarEquipamentos(contexto, { ...EQUIPAMENTOS_INICIAL, clienteChave: chave, tamanho: LINHAS }, sinal),
    [contexto.empresa, chave],
  );

  const oportunidades = useRecurso(
    (sinal) => listarProcessos(contexto, { ...PROCESSOS_INICIAL, clienteChave: chave, tamanho: LINHAS }, sinal),
    [contexto.empresa, chave],
  );

  const agenda = useRecurso(
    (sinal) =>
      listarTarefas(
        contexto,
        { ...TAREFAS_INICIAL, clienteChave: chave, situacao: '', tamanho: LINHAS, descendente: true },
        sinal,
      ),
    [contexto.empresa, chave],
  );

  const interacoes = useRecurso(
    (sinal) => listarInteracoes(contexto, { ...INTERACOES_INICIAL, clienteChave: chave, tamanho: LINHAS }, sinal),
    [contexto.empresa, chave],
  );

  const ficha = cliente.dados;

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">{ficha?.nomeRazao ?? 'Carregando o cliente…'}</h1>
          <p className="page-subtitle">
            {ficha ? (
              <>
                {ficha.nomeFantasia && <>{ficha.nomeFantasia} · </>}
                {ficha.tipoDePessoa === 'Fisica' ? 'Pessoa física' : 'Pessoa jurídica'}
                {ficha.documento && <> · {ficha.documento}</>}
              </>
            ) : (
              'Buscando a ficha na API do CRM.'
            )}
          </p>
        </div>
        <div className="page-actions">
          <button type="button" className="btn btn-secondary" onClick={aoLimpar}>
            Voltar ao meu dia
          </button>
          <Link to={`/clientes/${chave}`} className="btn btn-primary">
            Abrir o cadastro
          </Link>
        </div>
      </div>

      <div className="p360-grid">
        <BlocoPainel
          id="identificacao"
          titulo="Identificação e situação"
          subtitulo="O cadastro do cliente"
          fonte={<SeloProcedencia procedencia={cliente.procedencia} />}
          estado={estado(cliente, ficha !== null)}
          mensagemErro={cliente.erro?.message}
        >
          {ficha && (
            <dl className="p360-dados">
              <Dado rotulo="Situação" valor={<span className={`cad-selo cad-selo-${ficha.situacao.toLowerCase()}`}>{ficha.situacao}</span>} />
              <Dado rotulo="Situação desde" valor={formatarData(ficha.situacaoDesde)} />
              <Dado rotulo="Documento" valor={ficha.documento ?? <SemValor texto="não informado" />} />
              <Dado rotulo="Inscrição estadual" valor={ficha.inscricaoEstadual ?? <SemValor texto="não informada" />} />
              <Dado
                rotulo="Atividade econômica"
                valor={ficha.atividadeEconomica ?? <SemValor texto="não informada" />}
              />
              <Dado rotulo="Origem" valor={ficha.origemCodigo ?? <SemValor texto="não informada" />} />
              <Dado rotulo="Cadastrado em" valor={formatarDataHora(ficha.criadoEm)} />
              <Dado
                rotulo="Última alteração"
                valor={ficha.alteradoEm ? formatarDataHora(ficha.alteradoEm) : <SemValor texto="nunca alterado" />}
              />
            </dl>
          )}
        </BlocoPainel>

        <BlocoCarteirasDoCliente chave={chave} />

        <BlocoPainel
          id="frota"
          titulo="Frota instalada"
          subtitulo="o dono atual pela sincronia do parque (Protheus e ART), com a evidência, e as compras no ART"
          fonte={<SeloProcedencia procedencia={frota.procedencia} />}
          acao={
            <Link to={`/equipamentos?cliente=${chave}`} className="btn btn-secondary btn-sm">
              Ver todas
            </Link>
          }
          estado={estado(frota, (frota.dados?.itens.length ?? 0) > 0)}
          mensagemVazia={
            'Nenhuma máquina deste cliente ao seu alcance: ele não é o dono atual de nenhuma pela sincronia do ' +
            'parque, não comprou nenhuma no ART e não é o dono confirmado de nenhuma, nas filiais que você alcança.'
          }
          mensagemErro={frota.erro?.message}
        >
          <ul className="p360-lista">
            {frota.dados?.itens.map((e) => (
              <li className={e.relacaoComOCliente?.ehDonoAtual ? 'p360-item' : 'p360-item p360-item-info'} key={e.chave}>
                <div className="p360-item-topo">
                  <Link to={`/equipamentos/${e.chave}`} className="p360-item-titulo">
                    {e.modeloNome ?? e.produtoNaOrigem ?? 'Modelo não informado'}
                  </Link>
                  <span className="p360-item-data cad-mono">{e.chassi}</span>
                </div>
                <div className="p360-item-meta">
                  {e.marca ?? 'marca não informada'}
                  {e.anoModelo ? ` · ${e.anoModelo}` : ''}
                  {e.classificacaoNome ? ` · ${e.classificacaoNome}` : ''} · origem {e.origem}
                  {e.marcaRepresentada === false && ' · máquina de concorrente'}
                </div>
                <div className="p360-item-obs">
                  <RelacaoComOCliente maquina={e} />
                </div>
              </li>
            ))}
          </ul>
          {frota.dados && frota.dados.total > LINHAS && (
            <p className="p360-item-obs">
              {frota.dados.total.toLocaleString('pt-BR')} máquinas no total; as {LINHAS} primeiras
              estão acima.
            </p>
          )}
        </BlocoPainel>

        <BlocoFaturamentoDoCliente chave={chave} />

        <BlocoPainel
          id="oportunidades"
          titulo="Oportunidades abertas"
          subtitulo="Os processos abertos, com a fase e há quanto tempo estão nela"
          fonte={<SeloProcedencia procedencia={oportunidades.procedencia} />}
          acao={
            <Link to="/pipeline" className="btn btn-secondary btn-sm">
              Ver o pipeline
            </Link>
          }
          estado={estado(oportunidades, (oportunidades.dados?.itens.length ?? 0) > 0)}
          mensagemVazia="Este cliente não tem processo aberto."
          mensagemErro={oportunidades.erro?.message}
        >
          <ul className="p360-lista">
            {oportunidades.dados?.itens.map((p) => (
              <li className={`p360-item ${p.diasNaFase > 90 ? 'p360-item-aviso' : ''}`} key={p.chave}>
                <div className="p360-item-topo">
                  <span className="p360-item-titulo">{p.titulo}</span>
                  <span className="p360-item-data cad-mono">nº {p.numero}</span>
                </div>
                <div className="p360-item-meta">
                  {p.tipoProcessoNome} · fase {p.faseNome} há {p.diasNaFase.toLocaleString('pt-BR')}{' '}
                  {p.diasNaFase === 1 ? 'dia' : 'dias'}
                  {p.proprietarioNome ? ` · ${p.proprietarioNome}` : ''}
                </div>
                <div className="p360-item-obs">
                  {p.valorEstimado === null ? (
                    <SemValor texto="valor não declarado na origem" />
                  ) : (
                    formatarDinheiro(p.valorEstimado)
                  )}
                </div>
              </li>
            ))}
          </ul>
          {oportunidades.dados && oportunidades.dados.total > LINHAS && (
            <p className="p360-item-obs">
              {oportunidades.dados.total.toLocaleString('pt-BR')} processos abertos no total.
            </p>
          )}
        </BlocoPainel>

        <BlocoPainel
          id="agenda"
          titulo="Agenda deste cliente"
          subtitulo="O que está marcado e o que passou"
          fonte={<SeloProcedencia procedencia={agenda.procedencia} />}
          acao={
            <Link to="/agenda" className="btn btn-secondary btn-sm">
              Ver a agenda
            </Link>
          }
          estado={estado(agenda, (agenda.dados?.itens.length ?? 0) > 0)}
          mensagemVazia="Nenhuma tarefa registrada para este cliente."
          mensagemErro={agenda.erro?.message}
        >
          <ul className="p360-lista">
            {agenda.dados?.itens.map((t) => (
              <li className={`p360-item ${t.estaAtrasada ? 'p360-item-critico' : ''}`} key={t.chave}>
                <div className="p360-item-topo">
                  <span className="p360-item-titulo">{t.assunto}</span>
                  <span className="p360-item-data cad-mono">{formatarData(t.agendadaPara)}</span>
                </div>
                <div className="p360-item-meta">
                  {t.tipoTarefaNome} · {t.responsavelNome} · {t.situacao}
                  {t.estaAtrasada && ` · ${t.diasDeAtraso} dias de atraso`}
                </div>
                {t.resultadoNome && <div className="p360-item-obs">Desfecho: {t.resultadoNome}</div>}
              </li>
            ))}
          </ul>
        </BlocoPainel>

        <BlocoPainel
          id="interacoes"
          titulo="Linha do tempo"
          subtitulo="O que aconteceu com o cliente, a mais recente primeiro"
          fonte={<SeloProcedencia procedencia={interacoes.procedencia} />}
          estado={estado(interacoes, (interacoes.dados?.itens.length ?? 0) > 0)}
          mensagemVazia="Nenhuma interação registrada com este cliente."
          mensagemErro={interacoes.erro?.message}
          largo
        >
          <ul className="p360-lista">
            {interacoes.dados?.itens.map((i) => (
              <li className="p360-item" key={i.chave}>
                <div className="p360-item-topo">
                  <span className="p360-item-titulo">{i.assunto}</span>
                  <span className="p360-item-data cad-mono">{formatarDataHora(i.ocorridaEm)}</span>
                </div>
                <div className="p360-item-meta">
                  {/* A NATUREZA SEPARA TRÊS COISAS que a linha do tempo do
                      legado mostra misturadas: nós procuramos o cliente, o
                      cliente nos procurou, e o servidor carimbou um evento.
                      30.747 das 121.983 interações são da terceira. */}
                  <span className={`cad-selo cad-selo-${i.natureza.toLowerCase()}`}>{rotuloDaNatureza(i.natureza)}</span>{' '}
                  {i.tipoTarefaNome} · {i.autorNome}
                  {i.resultadoNome ? ` · ${i.resultadoNome}` : ''}
                </div>
                {i.detalhe && <div className="p360-item-obs">{i.detalhe}</div>}
              </li>
            ))}
          </ul>
          {interacoes.dados && interacoes.dados.total > LINHAS && (
            <p className="p360-item-obs">
              {interacoes.dados.total.toLocaleString('pt-BR')} interações no total com este cliente.
            </p>
          )}
        </BlocoPainel>
      </div>

      <div className="card cad-cartao">
        <div className="card-header">
          <div className="card-title">O que a ficha deste cliente não pode mostrar</div>
          <div className="card-subtitle">
            Os blocos continuam aqui, vazios e com o motivo de hoje. Some-los faria a tela parecer
            completa.
          </div>
        </div>
        <div className="cad-fichas">
          {LACUNAS_DA_FICHA_DO_CLIENTE.map((i) => (
            <LacunaConhecida key={i.metrica} metrica={i.metrica} motivo={i.motivo} />
          ))}
        </div>
      </div>
    </>
  );
}

/** Traduz os quatro estados da leitura no estado que o bloco entende. */
function estado(leitura: Leitura<unknown>, temConteudo: boolean): EstadoBloco {
  if (leitura.carregando) return 'carregando';
  if (leitura.erro) return 'erro';
  return temConteudo ? 'ok' : 'vazio';
}

/** Quem procurou quem, em palavra e não em código. */
function rotuloDaNatureza(natureza: string): string {
  if (natureza === 'Ativa') return 'nós procuramos';
  if (natureza === 'Receptiva') return 'o cliente procurou';
  return 'registro do sistema';
}
