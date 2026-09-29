/**
 * Visão 360 — a tela inicial do CRM, em duas camadas e **sem dado de arquivo**.
 *
 * ---------------------------------------------------------------------------
 * AS CAMADAS, e o que sustenta cada uma:
 *
 * | Perfil                 | O que aparece | De onde vem |
 * |---|---|---|
 * | Diretoria e Gerente    | o painel executivo do consolidado das filiais | `PainelExecutivo`, filial a filial |
 * | CEN, sem cliente       | o trabalho do dia | `/relatorios/agenda`, `/tarefas`, `/cobertura` |
 * | CEN, com cliente       | o 360 do cliente | cinco leituras em paralelo, todas por `clienteChave` |
 *
 * A busca do cliente é a mesma `SeletorDeCliente` do cadastro — busca com sugestão contra `/api/v1/clientes`, dentro
 * da filial do cabeçalho. O que a tela guarda é a CHAVE; o nome digitado nunca vira dado.
 *
 * ---------------------------------------------------------------------------
 * 28/09/2026 — AS TRÊS CAMADAS NO DESENHO DOS INDICADORES GEOGRÁFICOS (pedido do Ricardo). A página é a
 * `PaginaDoPainel` — o mesmo contêiner das quebras —, o cabeçalho é o dos Indicadores, e o perfil deixou de ser uma
 * faixa de três botões com avatar para ser um filtro da barra, ao lado do período. O trabalho do dia do CEN usa os
 * mesmos cartões e painéis do painel executivo.
 */

import { AlarmClock, CalendarCheck, CalendarRange, CheckCircle2, ListTodo } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { BlocoCarregando, BlocoErro } from '../componentes/cadastro/EstadosDeTela';
import { SeletorDeCliente } from '../componentes/cadastro/SeletorDeCliente';
import { MetricasSemDado } from '../componentes/cadastro/SemDado';
import { PaginaDoPainel } from '../componentes/dashboard/Dashboard';
import { CartaoDeDecisao } from '../componentes/mercado/CartaoDeDecisao';
import { PainelDoMomento } from '../componentes/mercado/momento/pecas';
import { Cliente360Api } from '../componentes/painel360/Cliente360Api';
import { FiltroDoPerfil } from '../componentes/painel360/FiltroDoPerfil';
import type { PerfilId } from '../componentes/painel360/perfis';
import { PainelExecutivo } from '../componentes/painel360/PainelExecutivo';
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
import '../estilos/dashboard.css';
import '../estilos/mercado-visao.css';
import '../estilos/momento.css';
import '../estilos/painel-executivo.css';

/** Quantas linhas cada fila mostra antes de mandar para a tela cheia. */
const LINHAS = 8;

export function Visao360() {
  // A PORTA DE ENTRADA É O PAINEL EXECUTIVO, e não o trabalho do CEN: quem abre o CRM precisa ver o negócio inteiro.
  const [perfil, setPerfil] = useState<PerfilId>('diretoria');
  const [clienteChave, setClienteChave] = useState('');
  const [clienteNome, setClienteNome] = useState('');

  if (perfil !== 'cen') {
    return (
      // A LARGURA É A DA COLUNA INTEIRA (`v360-pagina`): o teto de 1.940 só volta acima de 2.100px de janela. O desenho da
      // maquete de 29/09/2026 (`v360-maquete`) é o do painel executivo; o trabalho do dia do CEN segue como está.
      <PaginaDoPainel className="v360-pagina v360-maquete">
        <PainelExecutivo perfil={perfil} aoTrocarPerfil={setPerfil} />
      </PaginaDoPainel>
    );
  }

  const escolher = (chave: string, nome: string) => {
    setClienteChave(chave);
    setClienteNome(nome);
  };

  return (
    <PaginaDoPainel className="v360-pagina">
      {/* COM CLIENTE ESCOLHIDO, O TÍTULO É O DELE: a ficha do 360 traz o próprio cabeçalho, com o nome e o botão de
          trocar de cliente — dois títulos empilhados diriam a mesma coisa duas vezes. */}
      {!clienteChave && (
        <div className="page-header" data-bloco="cabecalho">
          <div>
            <h1 className="page-title">Visão 360</h1>
            <p className="page-subtitle">
              O trabalho do dia e o 360 de qualquer cliente da filial. Todo número desta tela vem do banco do CRM.
            </p>
          </div>
        </div>
      )}

      <div className="dash-filtros" data-bloco="filtros">
        <div className="dash-filtros-linha">
          <FiltroDoPerfil perfil={perfil} aoTrocar={setPerfil} />
          {clienteChave && (
            <div className="dash-filtros-acao">
              <button
                type="button"
                className="dash-mais-filtros"
                onClick={() => {
                  setClienteChave('');
                  setClienteNome('');
                }}
              >
                ← Voltar ao trabalho do dia
              </button>
            </div>
          )}
        </div>
      </div>

      {clienteChave ? (
        <div className="v360-pagina-cen">
          <Cliente360Api
            chave={clienteChave}
            aoLimpar={() => {
              setClienteChave('');
              setClienteNome('');
            }}
          />
        </div>
      ) : (
        <MeuDia clienteChave={clienteChave} clienteNome={clienteNome} aoEscolher={escolher} />
      )}
    </PaginaDoPainel>
  );
}

/** A camada do CEN: o que precisa de ação hoje, tudo contado no banco. */
function MeuDia({
  clienteChave,
  clienteNome,
  aoEscolher,
}: {
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
  // LENDO NÃO É AUSÊNCIA: enquanto a agenda não volta, o cartão pulsa, sem afirmar motivo.
  const motivo = painel.carregando ? undefined : 'Sem tarefa ao alcance deste contexto.';
  const numero = (v: number | undefined) => (v === undefined ? null : v.toLocaleString('pt-BR'));

  return (
    <>
      <div className="dash-kpis mv-kpis" data-bloco="kpis" data-colunas="5">
        <CartaoDeDecisao
          rotulo="Tarefas atrasadas"
          icone={AlarmClock}
          tom="captura"
          valor={numero(numeros?.atrasadas)}
          carregando={painel.carregando}
          motivoSemDado={motivo}
          variacao="a data agendada já passou e ninguém concluiu"
          sobre="Tarefas em Pendente ou Em andamento cuja data agendada já passou, nesta filial. É o que o dia de hoje precisa resolver primeiro."
        />
        <CartaoDeDecisao
          rotulo="Para hoje"
          icone={CalendarCheck}
          tom="demanda"
          valor={numero(numeros?.paraHoje)}
          carregando={painel.carregando}
          motivoSemDado={motivo}
          variacao="agendadas para a data de hoje"
          sobre="Tarefas agendadas para hoje, ainda não concluídas, nesta filial."
        />
        <CartaoDeDecisao
          rotulo="Próximos 7 dias"
          icone={CalendarRange}
          tom="mercado"
          valor={numero(numeros?.proximosSeteDias)}
          carregando={painel.carregando}
          motivoSemDado={motivo}
          variacao="agendadas para a semana que vem"
          sobre="Tarefas agendadas para os próximos sete dias, nesta filial."
        />
        <CartaoDeDecisao
          rotulo="Pendentes ao todo"
          icone={ListTodo}
          tom="oportunidade"
          valor={numero(numeros?.pendentes)}
          carregando={painel.carregando}
          motivoSemDado={motivo}
          variacao="em Pendente ou Em andamento, nesta filial"
          sobre="Todas as tarefas ainda abertas — Pendente ou Em andamento —, com ou sem data, nesta filial."
        />
        <CartaoDeDecisao
          rotulo="Concluídas em 30 dias"
          icone={CheckCircle2}
          tom="neutro"
          valor={numero(numeros?.concluidasNosUltimosTrintaDias)}
          carregando={painel.carregando}
          motivoSemDado={motivo}
          variacao="com data, autor e desfecho registrados"
          sobre="Tarefas concluídas nos últimos 30 dias, com a data, quem concluiu e o desfecho registrados."
        />
      </div>

      {painel.erro && <BlocoErro erro={painel.erro} aoTentarDeNovo={painel.recarregar} />}
      <MetricasSemDado metricas={painel.dados?.metricasSemDado} />

      <PainelDoMomento
        titulo="Abrir o 360 de um cliente"
        dica="Busca com sugestão contra o cadastro de clientes, dentro da filial do cabeçalho: nome e nome fantasia por trecho, documento por valor inteiro."
        subtitulo="Escolha um cliente para ver a frota, as oportunidades, a agenda e a linha do tempo dele."
        data-bloco="abrir-cliente"
      >
        <SeletorDeCliente rotulo="Cliente" valor={clienteChave} nome={clienteNome} aoEscolher={aoEscolher} largo />
      </PainelDoMomento>

      <div className="v360-linha" data-bloco="linha-cen" data-variante="mercado">
        <PainelDoMomento
          titulo="Tarefas atrasadas"
          dica="As mais antigas primeiro: o atraso é medido contra a data agendada."
          subtitulo={
            atrasadas.dados && atrasadas.dados.total > atrasadas.dados.itens.length
              ? `As ${atrasadas.dados.itens.length} mais antigas de ${atrasadas.dados.total.toLocaleString('pt-BR')} atrasadas.`
              : 'As mais antigas primeiro.'
          }
          direita={
            <Link to="/agenda" className="v360-link">
              Ver a agenda →
            </Link>
          }
          data-bloco="atrasadas"
        >
          {atrasadas.carregando ? (
            <BlocoCarregando oQue="as tarefas atrasadas" />
          ) : atrasadas.erro ? (
            <BlocoErro erro={atrasadas.erro} aoTentarDeNovo={atrasadas.recarregar} />
          ) : (atrasadas.dados?.itens.length ?? 0) === 0 ? (
            <p className="v360-nota">Nenhuma tarefa atrasada nesta filial.</p>
          ) : (
            <ul className="p360-lista">
              {atrasadas.dados!.itens.map((t) => (
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
          )}
        </PainelDoMomento>

        <PainelDoMomento
          titulo="Clientes há mais tempo sem contato"
          dica="O cliente nunca contatado vem antes de todos; depois, os de contato mais antigo. Clicar no nome abre o 360 dele."
          subtitulo={
            semContato.dados && semContato.dados.total > LINHAS
              ? `${semContato.dados.total.toLocaleString('pt-BR')} vínculos na carteira desta filial.`
              : 'O nunca contatado vem antes de todos.'
          }
          direita={
            <Link to="/cobertura" className="v360-link">
              Ver a cobertura →
            </Link>
          }
          data-bloco="sem-contato"
        >
          {semContato.carregando ? (
            <BlocoCarregando oQue="a cobertura da carteira" />
          ) : semContato.erro ? (
            <BlocoErro erro={semContato.erro} aoTentarDeNovo={semContato.recarregar} />
          ) : (semContato.dados?.itens.length ?? 0) === 0 ? (
            <p className="v360-nota">Nenhum vínculo de carteira nesta filial.</p>
          ) : (
            <ul className="p360-lista">
              {semContato.dados!.itens.map((c) => (
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
          )}
        </PainelDoMomento>
      </div>
    </>
  );
}
