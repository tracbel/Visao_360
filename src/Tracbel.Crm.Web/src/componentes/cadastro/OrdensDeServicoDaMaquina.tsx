/**
 * AS ORDENS DE SERVIÇO DA MÁQUINA, na ficha do equipamento (02/10/2026) — `/api/v1/equipamentos/{chave}/ordens-de-servico`,
 * da tabela que a rotina 15 `POS_VENDA_PROTHEUS` mantém a partir das views do BI no banco do Protheus.
 *
 * A OS entra aqui pelo CHASSI da OS. O horímetro de cada OS é o que a oficina anotou na abertura — a série dele é o
 * histórico de uso da máquina que o CRM não tinha (o horímetro atual vem da telemetria, quando há).
 */

import { Link } from 'react-router-dom';
import { useContextoDeAcesso } from '../../dados/api/contexto';
import { listarOrdensDeServicoDoEquipamento } from '../../dados/api/equipamentos';
import { useRecurso } from '../../dados/api/useRecurso';
import { formatarData, formatarDinheiro, formatarNumero } from '../../telas/cadastro/formato';
import { PainelDoMomento } from '../mercado/momento/pecas';
import { DIAS_DA_FAIXA_VERMELHA, rotuloDaSituacao } from '../painel360/regrasDasOrdensDeServico';
import { BlocoCarregando, BlocoErro } from './EstadosDeTela';
import { MetricasSemDado } from './SemDado';

export function OrdensDeServicoDaMaquina({ chave }: { chave: string }) {
  const { contexto } = useContextoDeAcesso();
  const leitura = useRecurso((sinal) => listarOrdensDeServicoDoEquipamento(contexto, chave, sinal), [contexto.empresa, chave]);
  const dados = leitura.dados;

  const resumo =
    dados && dados.totalDeOrdens > 0
      ? `${formatarNumero(dados.totalDeOrdens)} OS · ${formatarNumero(dados.emAberto)} na oficina agora · peças e serviços sem desconto, na régua do painel de pós-venda do BI`
      : 'A oficina do Protheus, pelo chassi da OS — peças e serviços sem desconto, na régua do painel de pós-venda do BI';

  return (
    <PainelDoMomento
      titulo="Ordens de serviço"
      data-bloco="ordens-de-servico"
      subtitulo={resumo}
      dica="A situação é a da capa da OS: aberta e liberada estão na oficina (o empenhado do BI); a faixa vermelha é a OS aberta há mais de 45 dias. O horímetro é o anotado na abertura."
    >
      {leitura.carregando && <BlocoCarregando oQue="as ordens de serviço" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {dados && dados.totalDeOrdens === 0 && (
        <p className="cad-nota">
          {dados.metricasSemDado[0]?.motivo ?? 'Nenhuma ordem de serviço para este chassi nas filiais ao seu alcance.'}
        </p>
      )}
      {dados && dados.totalDeOrdens > 0 && (
        <>
          <div className="mom-tabela-rolagem">
            <table className="mom-tabela">
              <caption className="cad-so-leitor">As ordens de serviço desta máquina, as abertas primeiro</caption>
              <thead>
                <tr>
                  <th scope="col">OS</th>
                  <th scope="col">Situação</th>
                  <th scope="col">Abertura · fechamento</th>
                  <th scope="col">Filial</th>
                  <th scope="col">Cliente na OS</th>
                  <th scope="col" className="num">Horímetro</th>
                  <th scope="col" className="num">Peças</th>
                  <th scope="col" className="num">Serviços</th>
                </tr>
              </thead>
              <tbody>
                {dados.ordens.map(({ ordem, diasEmAberto }) => (
                  <tr key={ordem.chave}>
                    <td className="cad-mono">{ordem.numero}</td>
                    <td>
                      <span
                        className={`cad-selo ${diasEmAberto !== null && diasEmAberto > DIAS_DA_FAIXA_VERMELHA ? 'cad-selo-pendente' : ''}`}
                      >
                        {rotuloDaSituacao(ordem.situacao)}
                      </span>
                      {diasEmAberto !== null && <div className="cad-sub">há {formatarNumero(diasEmAberto)} dias</div>}
                    </td>
                    <td className="cad-mono">
                      {formatarData(ordem.abertaEm)}
                      {ordem.fechadaEm && <div className="cad-sub">{formatarData(ordem.fechadaEm)}</div>}
                    </td>
                    <td>{ordem.filialNome}</td>
                    <td className="ficha-texto">
                      {ordem.clienteChave ? (
                        <Link to={`/clientes/${ordem.clienteChave}`}>{ordem.clienteNome}</Link>
                      ) : (
                        <span className="cad-vazio">não casou com cliente do CRM</span>
                      )}
                      {ordem.tipoDeAtendimento && <div className="cad-sub">{ordem.tipoDeAtendimento.toLowerCase()}</div>}
                    </td>
                    <td className="num cad-mono">{ordem.horimetro === null ? '—' : `${formatarNumero(ordem.horimetro, 1)} h`}</td>
                    <td className="num">{formatarDinheiro(ordem.valorDePecas)}</td>
                    <td className="num">{formatarDinheiro(ordem.valorDeServicos)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {dados.totalDeOrdens > dados.ordens.length && (
            <p className="cad-nota">
              {formatarNumero(dados.totalDeOrdens)} ordens de serviço no total; a tabela traz as {dados.ordens.length} primeiras.
            </p>
          )}
          <MetricasSemDado metricas={dados.metricasSemDado} titulo="O que este quadro não diz" />
        </>
      )}
    </PainelDoMomento>
  );
}
