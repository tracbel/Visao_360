/**
 * O TEMPO DE RESPOSTA DA API, MEDIDO PELO SERVIDOR (issue 51) — p50, p95 e máximo de cada rota, nas chamadas de verdade.
 *
 * É a medida que o documento 45 §6.1 deixou para "medir no servidor": a trilha de auditoria grava na mesma transação de
 * cada gravação, e o custo dela só se lê direito no SQL Server de produção. Nenhum roteiro, nenhuma sessão na estação —
 * a API mede sozinha e esta tabela mostra. A medição recomeça a cada publicação, e a tela diz desde quando.
 */

import { useState } from 'react';
import { useContextoDeAcesso } from '../../../dados/api/contexto';
import { obterDesempenhoDaApi } from '../../../dados/api/integracoes';
import { useRecurso } from '../../../dados/api/useRecurso';
import { formatarDataHora } from '../../../telas/cadastro/formato';
import { BlocoCarregando, BlocoErro } from '../../cadastro/EstadosDeTela';
import { CardConfig } from '../ConfigPartes';

const ms = (v: number) => `${v.toLocaleString('pt-BR', { maximumFractionDigits: 1 })} ms`;

export function CartaoDeDesempenho() {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => obterDesempenhoDaApi(contexto, sinal), [contexto.empresa, contexto.usuario]);
  const [soGravacoes, setSoGravacoes] = useState(false);
  const dados = leitura.dados;
  const rotas = dados ? dados.rotas.filter((r) => !soGravacoes || r.grava) : [];

  return (
    <CardConfig titulo="Tempo de resposta da API">
      <p className="config-hint int-intro">
        Medido pelo próprio servidor, com as chamadas de verdade
        {dados ? `, desde ${formatarDataHora(dados.desdeUtc)} (a última publicação)` : ''}. Cada rota guarda as últimas{' '}
        {dados ? dados.amostrasPorRota.toLocaleString('pt-BR') : 'mil'} chamadas. As gravações passam pela trilha de auditoria
        na mesma transação — é o custo que se queria ver no servidor.
      </p>

      {leitura.carregando && !dados && <BlocoCarregando oQue="o tempo de resposta" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}

      {dados && (
        <>
          <div className="int-rodape int-desempenho-barra">
            <label className="cad-filtro-caixa">
              <input type="checkbox" checked={soGravacoes} onChange={(e) => setSoGravacoes(e.target.checked)} />
              Só as gravações
            </label>
            <button type="button" className="btn btn-secondary btn-sm" onClick={leitura.recarregar}>
              Atualizar
            </button>
          </div>

          {rotas.length === 0 ? (
            <p className="config-hint">
              {soGravacoes
                ? 'Nenhuma gravação desde a última publicação.'
                : 'Nenhuma chamada medida desde a última publicação.'}
            </p>
          ) : (
            <div className="cad-tabela-wrap">
              <table className="cad-tabela">
                <caption className="cad-so-leitor">Tempo de resposta de cada rota da API</caption>
                <thead>
                  <tr>
                    <th scope="col">Rota</th>
                    <th scope="col">Chamadas</th>
                    <th scope="col">p50</th>
                    <th scope="col">p95</th>
                    <th scope="col">Máximo</th>
                    <th scope="col">Erros</th>
                  </tr>
                </thead>
                <tbody>
                  {rotas.map((r) => (
                    <tr key={`${r.metodo} ${r.rota}`}>
                      <td>
                        <span className="cad-mono">{r.metodo}</span> {r.rota}
                        {r.grava && <div className="cad-sub">grava — passa pela trilha</div>}
                      </td>
                      <td className="cad-mono">
                        {r.chamadas.toLocaleString('pt-BR')}
                        {r.amostras < r.chamadas && <div className="cad-sub">as últimas {r.amostras.toLocaleString('pt-BR')}</div>}
                      </td>
                      <td className="cad-mono">{ms(r.p50)}</td>
                      <td className="cad-mono">{ms(r.p95)}</td>
                      <td className="cad-mono">{ms(r.maximo)}</td>
                      <td className="cad-mono">{r.erros === 0 ? '0' : r.erros.toLocaleString('pt-BR')}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </CardConfig>
  );
}
