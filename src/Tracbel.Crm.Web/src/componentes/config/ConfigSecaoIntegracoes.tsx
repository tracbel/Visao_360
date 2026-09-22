/**
 * Configurações › Administração › Integrações — porte de `renderSecaoIntegracoes`
 * (prototipo/referencia/assets/app.js linha 5671).
 *
 * ---------------------------------------------------------------------------
 * 14/09/2026 — O CARTÃO "SINCRONIZAÇÕES" LÊ A API (documento 35, seção 11): cada
 * ciclo do serviço do Windows `TracbelCrmSincronizacaoArt`.
 *
 * 22/09/2026 (issue 136) — "SISTEMAS CONECTADOS" VOLTA, COM DADO REAL E MEXÍVEL:
 * cada conexão que o CRM usa (Protheus, ART, Vórtice e as fontes públicas), de
 * onde vem a credencial, a última verificação e o botão "Testar", que roda no
 * servidor. O Administrador configura o endereço e a senha (guardada protegida,
 * nunca mostrada), cadastra API nova para monitorar, e muda a agenda das rotinas
 * do servidor — que o orquestrador segue. A Gerência e a Diretoria (Integracao.Ler)
 * veem tudo e não mexem.
 */import { useState } from 'react';
import { BlocoCarregando, BlocoErro } from '../cadastro/EstadosDeTela';
import { obterPainelDeIntegracoes } from '../../dados/api/integracoes';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { listarSincronizacoes } from '../../dados/api/sincronizacoes';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarDataHora } from '../../telas/cadastro/formato';
import type { SituacaoDaSincronizacao } from '../../tipos/api';
import { CardConfig } from './ConfigPartes';
import { CartaoDeConexao } from './integracoes/CartaoDeConexao';
import { CartaoDeRotina } from './integracoes/CartaoDeRotina';
import { NovaApiMonitorada } from './integracoes/NovaApiMonitorada';

const RESULTADO: Record<string, { l: string; cor: string }> = {
  Sucesso: { l: 'Sucesso', cor: '#22C55E' },
  Falha: { l: 'Falha', cor: '#EF4444' },
  Ignorada: { l: 'Não rodou', cor: '#F59E0B' },
  EmAndamento: { l: 'Em andamento', cor: '#94A3B8' },
};

function Pill({ resultado }: { resultado: string | null }) {
  const s = (resultado && RESULTADO[resultado]) || { l: resultado ?? 'Nunca executado', cor: '#94A3B8' };
  return (
    <span className="int-status" style={{ background: `${s.cor}20`, color: s.cor, borderColor: `${s.cor}40` }}>
      <span className="int-dot" style={{ background: s.cor }} />
      {s.l}
    </span>
  );
}

function CartaoDeSincronizacoes() {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso<SituacaoDaSincronizacao[]>(
    (sinal) => listarSincronizacoes(contexto, sinal),
    [contexto.empresa, contexto.usuario],
  );

  return (
    <CardConfig titulo="Sincronizações do servidor">
      {leitura.carregando && <BlocoCarregando oQue="as sincronizações" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {leitura.dados && leitura.dados.length === 0 && (
        <div className="config-hint">
          Nenhuma execução registrada. O serviço TracbelCrmSincronizacaoArt grava cada ciclo aqui assim que roda no
          servidor.
        </div>
      )}
      {leitura.dados?.map((fluxo) => (
        <div className="integracao-item" key={`${fluxo.sistemaCodigo}-${fluxo.fluxo}`}>
          <div className="int-info">
            <div className="int-header">
              <strong>{fluxo.sistemaCodigo}</strong>
              <span className="int-tipo">{fluxo.fluxo}</span>
              <Pill resultado={fluxo.ultimoResultado} />
            </div>
            <div className="int-meta">
              <div>
                <span>Última execução:</span>{' '}
                <span className="mono">{fluxo.ultimaExecucaoEm ? formatarDataHora(fluxo.ultimaExecucaoEm) : '—'}</span>
              </div>
              <div>
                <span>Último sucesso:</span>{' '}
                <span className="mono">{fluxo.ultimoSucessoEm ? formatarDataHora(fluxo.ultimoSucessoEm) : 'nunca'}</span>
              </div>
            </div>
            <div className="cad-tabela-wrap" style={{ marginTop: 12 }}>
              <table className="cad-tabela">
                <thead>
                  <tr>
                    <th scope="col">Início</th>
                    <th scope="col">Resultado</th>
                    <th scope="col">Tentativas</th>
                    <th scope="col">Lidos</th>
                    <th scope="col">Incluídas</th>
                    <th scope="col">Atualizadas</th>
                    <th scope="col">Pendentes</th>
                    <th scope="col">Máquina</th>
                    <th scope="col">Resumo ou motivo</th>
                  </tr>
                </thead>
                <tbody>
                  {fluxo.execucoes.map((e) => (
                    <tr key={e.iniciadaEm}>
                      <td className="cad-mono">
                        {formatarDataHora(e.iniciadaEm)}
                        <div className="cad-sub">{e.terminadaEm ? `fim ${formatarDataHora(e.terminadaEm)}` : 'sem fim registrado'}</div>
                      </td>
                      <td>
                        <Pill resultado={e.resultado} />
                      </td>
                      <td className="cad-mono">{e.tentativas}</td>
                      <td className="cad-mono">{e.registrosLidos}</td>
                      <td className="cad-mono">{e.incluidos}</td>
                      <td className="cad-mono">{e.atualizados}</td>
                      <td className="cad-mono">{e.pendentes}</td>
                      <td className="cad-mono">{e.maquina}</td>
                      <td>{e.mensagem ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      ))}
      <div className="config-hint" style={{ marginTop: 16 }}>
        A sincronização roda no servidor, como serviço do Windows, sem depender de navegador nem de sessão aberta. Esta
        lista é só leitura; o detalhe de cada ciclo também fica no Log de Aplicativo do servidor.
      </div>
    </CardConfig>
  );
}

function SistemasEConexoes() {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => obterPainelDeIntegracoes(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const [novaApi, setNovaApi] = useState(false);
  const painel = leitura.dados;

  if (leitura.carregando && !painel) return <BlocoCarregando oQue="as integrações" />;
  if (leitura.erro) return <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />;
  if (!painel) return null;

  return (
    <>
      <CardConfig titulo="Sistemas conectados">
        <p className="config-hint int-intro">
          Cada sistema que o CRM lê, de onde vem a credencial e se respondeu no último teste. {painel.podeAdministrar
            ? '"Testar" roda no próprio servidor, só leitura, e fica no histórico.'
            : 'Configurar e testar é do Administrador.'}
        </p>
        <div className="integracoes-lista">
          {painel.conexoes.map((c) => (
            <CartaoDeConexao key={c.codigo} conexao={c} podeAdministrar={painel.podeAdministrar} aoMudar={leitura.recarregar} />
          ))}
        </div>
        {painel.podeAdministrar && !novaApi && (
          <div className="int-rodape">
            <button type="button" className="btn-config-primary" onClick={() => setNovaApi(true)}>
              + Conectar nova API
            </button>
          </div>
        )}
        {novaApi && (
          <NovaApiMonitorada
            aoCriar={() => {
              setNovaApi(false);
              leitura.recarregar();
            }}
            aoCancelar={() => setNovaApi(false)}
          />
        )}
      </CardConfig>

      <CardConfig titulo="Rotinas do servidor">
        <p className="config-hint int-intro">
          O que o servidor roda sozinho. Quem roda é o orquestrador, a cada cinco minutos, seguindo a agenda daqui — sem tarefa do Windows para
          editar. "Rodar agora" entra na fila e começa em até cinco minutos.
        </p>
        <div className="integracoes-lista">
          {painel.rotinas.map((r) => (
            <CartaoDeRotina key={r.codigo} rotina={r} podeAdministrar={painel.podeAdministrar} aoMudar={leitura.recarregar} />
          ))}
        </div>
      </CardConfig>
    </>
  );
}

export function ConfigSecaoIntegracoes() {
  return (
    <>
      <SistemasEConexoes />
      <CartaoDeSincronizacoes />
    </>
  );
}