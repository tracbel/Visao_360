/**
 * As máquinas que o cliente comprou — o cartão da ficha do cliente que lê o
 * vínculo "comprador na venda" das vendas de máquina (documento 35, seções 10
 * e 11).
 *
 * COMPRAR NÃO FAZ DONO. Uma máquina revendida teve dois compradores, e o ART
 * não diz quem a tem hoje: a coluna "Dono atual" só diz "sim" quando o cadastro
 * da máquina aponta este cliente como proprietário.
 */

import { Link } from 'react-router-dom';
import { listarMaquinasCompradasPeloCliente } from '../../dados/api/equipamentos';
import type { ContextoDeAcesso } from '../../dados/api/http';
import { useRecurso } from '../../dados/api/useRecurso';
import type { MaquinaCompradaPeloCliente } from '../../tipos/api';
import { BlocoCarregando, BlocoErro } from './EstadosDeTela';
import { SeloProcedencia } from './SeloProcedencia';
import '../../estilos/frota-comercial.css';

function dataCurta(valor: string | null): string {
  if (!valor) return '—';
  const [ano, mes, dia] = valor.slice(0, 10).split('-');
  return `${dia}/${mes}/${ano}`;
}

export function MaquinasCompradasDoCliente({
  contexto,
  chaveDoCliente,
}: {
  contexto: ContextoDeAcesso;
  chaveDoCliente: string;
}) {
  const leitura = useRecurso<MaquinaCompradaPeloCliente[]>(
    (sinal) => listarMaquinasCompradasPeloCliente(contexto, chaveDoCliente, sinal),
    [contexto.empresa, contexto.usuario, chaveDoCliente],
  );

  return (
    <div className="card cad-cartao">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          <div className="card-title">Máquinas compradas</div>
          <div className="card-subtitle">
            As vendas de máquina em que este cliente foi o comprador, da mais recente para a mais antiga
          </div>
        </div>
        <SeloProcedencia procedencia={leitura.procedencia} />
      </div>
      {leitura.carregando && <BlocoCarregando oQue="as máquinas compradas" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {leitura.dados && leitura.dados.length === 0 && (
        <div className="card-body">
          <p className="cad-nota">Nenhuma venda de máquina registrada com este cliente como comprador.</p>
        </div>
      )}
      {leitura.dados && leitura.dados.length > 0 && (
        <div className="cad-tabela-wrap">
          <table className="cad-tabela">
            <thead>
              <tr>
                <th scope="col">Venda</th>
                <th scope="col">Chassi</th>
                <th scope="col">Modelo e classificação</th>
                <th scope="col">Filial</th>
                <th scope="col">Dono atual</th>
                <th scope="col">Origem</th>
              </tr>
            </thead>
            <tbody>
              {leitura.dados.map((maquina) => (
                <tr key={`${maquina.equipamentoChave}-${maquina.vendidaEm ?? ''}`}>
                  <td className="cad-mono">{dataCurta(maquina.vendidaEm)}</td>
                  <td className="cad-mono">
                    <Link to={`/equipamentos/${maquina.equipamentoChave}`}>{maquina.chassi}</Link>
                  </td>
                  <td>
                    {maquina.modeloNome ?? maquina.produtoNaOrigem ?? '—'}
                    <div className="cad-sub">{maquina.classificacaoNome ?? 'sem classificação'}</div>
                  </td>
                  <td className="cad-mono">{maquina.filialCodigo}</td>
                  <td>
                    {maquina.ehDonoAtual ? (
                      'Sim'
                    ) : (
                      <span className="cad-selo cad-selo-comprador">comprador na venda</span>
                    )}
                  </td>
                  <td>{maquina.sistemaCodigo}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <div className="card-body">
        <p className="cad-nota">
          O comprador de uma venda não vira dono automaticamente: a posse é confirmada no cadastro da máquina.
        </p>
      </div>
    </div>
  );
}
