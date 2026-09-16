/**
 * Visão 360 — a tela inicial do CRM, em duas camadas e **sem dado de arquivo**.
 *
 * ---------------------------------------------------------------------------
 * 05/09/2026 — a última tela que ainda lia JSON do protótipo.
 *
 * O que saiu, e por quê:
 *
 * - **O seletor de perfil e o painel executivo.** O painel do Gerente e o da
 *   Diretora mostravam faturamento de doze meses, ranking de CENs, mix de
 *   linhas e cobertura regional, todos de `public/dados/*.json`. Os quatro não
 *   têm lastro: o faturamento está parado na origem desde 11/04/2025, não
 *   existe rota que agrupe por pessoa, não há tabela de mix carregada, e a
 *   **regional não existe** — `IVS_Regional` tem zero linhas. Um painel de
 *   diretoria inteiro construído sobre isso é o pior lugar possível para um
 *   número plausível e errado.
 * - **O cliente 84391.** Só ele tinha ficha completa no protótipo; os outros 22
 *   da carteira abriam vazios. Agora **qualquer um dos 23.945 clientes
 *   carregados** abre com o que existe dele.
 *
 * ---------------------------------------------------------------------------
 * AS DUAS CAMADAS, e o que sustenta cada uma:
 *
 * | Camada | Quando aparece | De onde vem |
 * |---|---|---|
 * | **O meu dia** | nenhum cliente escolhido | `/relatorios/agenda`, `/tarefas`, `/cobertura` |
 * | **O 360 do cliente** | um cliente escolhido na busca | cinco leituras em paralelo, todas por `clienteChave` |
 *
 * A busca é a mesma `SeletorDeCliente` do cadastro — busca com sugestão contra
 * `/api/v1/clientes`, dentro da filial do cabeçalho. O que a tela guarda é a
 * CHAVE; o nome digitado nunca vira dado.
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import { PainelExecutivo } from '../componentes/painel360/PainelExecutivo';
import { BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { PainelDeIndicadores, type Indicador } from '../componentes/cadastro/Indicadores';
import { SeletorDeCliente } from '../componentes/cadastro/SeletorDeCliente';
import { SeloProcedencia } from '../componentes/cadastro/SeloProcedencia';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { BlocoPainel } from '../componentes/painel360/BlocoPainel';
import { Cliente360Api } from '../componentes/painel360/Cliente360Api';
import { useContextoDeAcesso } from '../dados/api/contexto';
import {
  COBERTURA_INICIAL,
  listarCobertura,
  listarTarefas,
  obterPainelDaAgenda,
  TAREFAS_INICIAL,
} from '../dados/api/relacionamento';
import { useRecurso } from '../dados/api/useRecurso';
import { formatarData } from './cadastro/formato';

/** Quantas linhas cada fila mostra antes de mandar para a tela cheia. */
const LINHAS = 8;

/**
 * Os três perfis do protótipo. O CEN trabalha o cliente; os outros dois abrem o
 * painel consolidado das treze filiais.
 */
const PERFIS = [
  { id: 'cen', cargo: 'CEN', nome: 'Carteira e cliente', avatar: 'CE', cor: '#367C2B' },
  { id: 'gerente', cargo: 'Gerente regional', nome: 'Consolidado das filiais', avatar: 'GR', cor: '#1B5E20' },
  { id: 'diretoria', cargo: 'Diretoria comercial', nome: 'Consolidado das filiais', avatar: 'DC', cor: '#7C3AED' },
] as const;

type PerfilId = (typeof PERFIS)[number]['id'];

export function Visao360() {
  // A PORTA DE ENTRADA É O PAINEL EXECUTIVO, e não o painel do CEN.
  //
  // Quem abre o CRM precisa ver a tela do protótipo — o painel de gráficos com
  // o negócio inteiro. O painel do CEN continua existindo, no perfil dele, e é
  // para onde a etapa dos fluxos de negócio volta; ele só deixa de ser a
  // primeira coisa que aparece.
  const [perfil, setPerfil] = useState<PerfilId>('diretoria');
  const [clienteChave, setClienteChave] = useState('');
  const [clienteNome, setClienteNome] = useState('');

  if (perfil !== 'cen') {
    return (
      <>
        <div className="v360-header">
          <div className="v360-header-left">
            <h1 className="v360-title">Visão 360</h1>
            <div className="v360-subtitle">
              Dashboard executivo · <strong>Consolidado das filiais em operação</strong> ·{' '}
              <Link to="/relatorios/territorio" className="v360-link">
                Indicadores geográficos da ADR →
              </Link>
            </div>
          </div>
          <SeletorDePerfil perfil={perfil} aoTrocar={setPerfil} />
        </div>

        <PainelExecutivo />
      </>
    );
  }

  if (clienteChave) {
    return (
      <>
        <SeletorDePerfil perfil={perfil} aoTrocar={setPerfil} />
        <Cliente360Api
          chave={clienteChave}
          aoLimpar={() => {
            setClienteChave('');
            setClienteNome('');
          }}
        />
      </>
    );
  }

  return (
    <MeuDia
      perfil={perfil}
      aoTrocarPerfil={setPerfil}
      clienteChave={clienteChave}
      clienteNome={clienteNome}
      aoEscolher={(chave, nome) => {
        setClienteChave(chave);
        setClienteNome(nome);
      }}
    />
  );
}

/** A camada 1: o que precisa de ação hoje, tudo contado no banco. */
function MeuDia({
  perfil,
  aoTrocarPerfil,
  clienteChave,
  clienteNome,
  aoEscolher,
}: {
  perfil: PerfilId;
  aoTrocarPerfil: (p: PerfilId) => void;
  clienteChave: string;
  clienteNome: string;
  aoEscolher: (chave: string, nome: string) => void;
}) {
  const { contexto } = useContextoDeAcesso();

  const painel = useRecurso(
    (sinal) => obterPainelDaAgenda(contexto, false, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const atrasadas = useRecurso(
    (sinal) =>
      listarTarefas(
        contexto,
        { ...TAREFAS_INICIAL, somenteAtrasadas: true, situacao: 'Pendente', tamanho: LINHAS },
        sinal,
      ),
    [contexto.empresa, contexto.usuario],
  );

  const semContato = useRecurso(
    (sinal) => listarCobertura(contexto, { ...COBERTURA_INICIAL, tamanho: LINHAS }, sinal),
    [contexto.empresa, contexto.usuario],
  );

  const numeros = painel.dados?.itens[0] ?? null;

  const indicadores: Indicador[] = [
    {
      rotulo: 'Tarefas atrasadas',
      valor: numeros?.atrasadas ?? null,
      tom: 'atencao',
      deOnde: 'a data agendada já passou e ninguém concluiu',
      semDado: 'sem tarefa ao alcance deste contexto',
    },
    {
      rotulo: 'Para hoje',
      valor: numeros?.paraHoje ?? null,
      deOnde: 'agendadas para a data de hoje',
      semDado: '—',
    },
    {
      rotulo: 'Próximos 7 dias',
      valor: numeros?.proximosSeteDias ?? null,
      deOnde: 'agendadas para a semana que vem',
      semDado: '—',
    },
    {
      rotulo: 'Pendentes ao todo',
      valor: numeros?.pendentes ?? null,
      deOnde: 'em Pendente ou Em andamento, nesta filial',
      semDado: '—',
    },
    {
      rotulo: 'Concluídas em 30 dias',
      valor: numeros?.concluidasNosUltimosTrintaDias ?? null,
      tom: 'bom',
      deOnde: 'com data, autor e desfecho registrados',
      semDado: '—',
    },
  ];

  return (
    <>
      <div className="page-header">
        <div>
          <h1 className="page-title">Visão 360</h1>
          <p className="page-subtitle">
            O trabalho do dia e o 360 de qualquer cliente da filial. Todo número desta tela vem do
            banco do CRM.
          </p>
        </div>
      </div>

      <SeletorDePerfil perfil={perfil} aoTrocar={aoTrocarPerfil} />

      <div className="card cad-cartao">
        <div className="card-header">
          <div className="card-title">Abrir o 360 de um cliente</div>
          <div className="card-subtitle">
            Busca com sugestão contra <code>/api/v1/clientes</code>, dentro da filial do cabeçalho.
            Nome e nome fantasia por trecho; documento por valor inteiro.
          </div>
        </div>
        <div className="cad-fichas">
          <SeletorDeCliente
            rotulo="Cliente"
            valor={clienteChave}
            nome={clienteNome}
            aoEscolher={aoEscolher}
            largo
            ajuda="Escolha um cliente para ver a frota, as oportunidades, a agenda e a linha do tempo dele."
          />
        </div>
      </div>

      <PainelDeIndicadores indicadores={indicadores} carregando={painel.carregando} />

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}

      <MetricasSemDado metricas={painel.dados?.metricasSemDado} />

      <div className="p360-grid">
        <BlocoPainel
          id="atrasadas"
          titulo="Tarefas atrasadas"
          subtitulo="as mais antigas primeiro — o atraso é medido contra a data agendada"
          fonte={<SeloProcedencia procedencia={atrasadas.procedencia} />}
          acao={
            <Link to="/agenda" className="btn btn-secondary btn-sm">
              Ver a agenda
            </Link>
          }
          estado={
            atrasadas.carregando
              ? 'carregando'
              : atrasadas.erro
                ? 'erro'
                : (atrasadas.dados?.itens.length ?? 0) > 0
                  ? 'ok'
                  : 'vazio'
          }
          mensagemVazia="Nenhuma tarefa atrasada nesta filial."
          mensagemErro={atrasadas.erro?.message}
        >
          <ul className="p360-lista">
            {atrasadas.dados?.itens.map((t) => (
              <li className="p360-item p360-item-critico" key={t.chave}>
                <div className="p360-item-topo">
                  <span className="p360-item-titulo">{t.assunto}</span>
                  <span className="p360-item-data cad-mono">{t.diasDeAtraso} dias</span>
                </div>
                <div className="p360-item-meta">
                  {t.clienteNome ?? 'sem cliente na origem'} · {t.responsavelNome} · agendada para{' '}
                  {formatarData(t.agendadaPara)}
                </div>
              </li>
            ))}
          </ul>
          {atrasadas.dados && atrasadas.dados.total > LINHAS && (
            <p className="p360-item-obs">
              {atrasadas.dados.total.toLocaleString('pt-BR')} tarefas atrasadas ao todo.
            </p>
          )}
        </BlocoPainel>

        <BlocoPainel
          id="sem-contato"
          titulo="Clientes há mais tempo sem contato"
          subtitulo="o nunca-contatado vem antes de todos"
          fonte={<SeloProcedencia procedencia={semContato.procedencia} />}
          acao={
            <Link to="/cobertura" className="btn btn-secondary btn-sm">
              Ver a cobertura
            </Link>
          }
          estado={
            semContato.carregando
              ? 'carregando'
              : semContato.erro
                ? 'erro'
                : (semContato.dados?.itens.length ?? 0) > 0
                  ? 'ok'
                  : 'vazio'
          }
          mensagemVazia="Nenhum vínculo de carteira nesta filial."
          mensagemErro={semContato.erro?.message}
        >
          <ul className="p360-lista">
            {semContato.dados?.itens.map((c) => (
              <li className="p360-item p360-item-aviso" key={`${c.clienteChave}-${c.carteiraChave}`}>
                <div className="p360-item-topo">
                  <button
                    type="button"
                    className="p360-item-titulo cad-th-ordenar"
                    onClick={() => aoEscolher(c.clienteChave, c.clienteNome)}
                  >
                    {c.clienteNome}
                  </button>
                  <span className="p360-item-data cad-mono">
                    {c.diasSemContato === null ? 'nunca' : `${c.diasSemContato} dias`}
                  </span>
                </div>
                <div className="p360-item-meta">
                  {c.carteiraNome} · {c.linhaDeNegocioNome} · {c.responsavelNome}
                </div>
              </li>
            ))}
          </ul>
          {semContato.dados && semContato.dados.total > LINHAS && (
            <p className="p360-item-obs">
              {semContato.dados.total.toLocaleString('pt-BR')} vínculos na carteira desta filial.
            </p>
          )}
        </BlocoPainel>
      </div>

    </>
  );
}

/**
 * O seletor de perfil do protótipo, com os três papéis.
 *
 * Ele não é decorativo: **troca de tela**. O CEN abre o trabalho do dia e o 360
 * de um cliente; Gerente e Diretoria abrem o painel consolidado das treze
 * filiais. É a mesma decisão registrada no documento 05 §8, agora com dado real
 * dos dois lados.
 */
function SeletorDePerfil({ perfil, aoTrocar }: { perfil: PerfilId; aoTrocar: (p: PerfilId) => void }) {
  return (
    <div className="v360-perfil-toggle" role="group" aria-label="Perfil de visualização">
      {PERFIS.map((p) => (
        <button
          key={p.id}
          type="button"
          className={p.id === perfil ? 'v360-perfil-btn active' : 'v360-perfil-btn'}
          aria-pressed={p.id === perfil}
          onClick={() => aoTrocar(p.id)}
        >
          <div className="v360-perfil-avatar" style={{ background: p.cor }}>
            {p.avatar}
          </div>
          <div className="v360-perfil-info">
            <div className="v360-perfil-cargo">{p.cargo}</div>
            <div className="v360-perfil-nome">{p.nome}</div>
          </div>
        </button>
      ))}
    </div>
  );
}
