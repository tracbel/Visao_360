/**
 * A FROTA DO CLIENTE na ficha do cadastro — pelo dono atual, e não só pelas compras no ART (27/09/2026).
 *
 * Até aqui a ficha só tinha "Máquinas compradas": as vendas do ART em que o cliente foi o comprador — 3.564 vínculos
 * de 1.497 clientes. O dono atual da sincronia do parque (decisão de 24/09/2026) existe em 22.287 máquinas de 6.165
 * clientes, e nenhuma tela o lia por cliente. Este cartão lê a mesma listagem que o 360 e a lista de equipamentos usam
 * (`/api/v1/equipamentos?clienteChave=`), com a relação e a evidência de cada máquina. As compras continuam no cartão
 * de baixo, como histórico.
 */

import { Link } from 'react-router-dom';
import { CONSULTA_INICIAL, listarEquipamentos } from '../../dados/api/equipamentos';
import type { ContextoDeAcesso } from '../../dados/api/http';
import { useRecurso } from '../../dados/api/useRecurso';
import { BlocoCarregando, BlocoErro } from './EstadosDeTela';
import { RelacaoComOCliente } from './RelacaoDaMaquina';
import { SeloProcedencia } from './SeloProcedencia';

/** Quantas máquinas o cartão mostra antes de mandar para a lista. */
const LINHAS = 25;

export function FrotaDoCliente({ contexto, chaveDoCliente }: { contexto: ContextoDeAcesso; chaveDoCliente: string }) {
  const leitura = useRecurso(
    (sinal) => listarEquipamentos(contexto, { ...CONSULTA_INICIAL, clienteChave: chaveDoCliente, tamanho: LINHAS }, sinal),
    [contexto.empresa, contexto.usuario, chaveDoCliente],
  );
  const pagina = leitura.dados;

  return (
    <div className="card cad-cartao" data-bloco="frota-do-cliente">
      <div className="card-header cad-cartao-cabecalho">
        <div>
          <div className="card-title">Frota do cliente</div>
          <div className="card-subtitle">
            As máquinas de que ele é o dono atual pela sincronia do parque (Protheus e ART), com a evidência, e as que ele
            comprou no ART
          </div>
        </div>
        <SeloProcedencia procedencia={leitura.procedencia} />
      </div>
      {leitura.carregando && <BlocoCarregando oQue="a frota do cliente" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {pagina && pagina.itens.length === 0 && (
        <div className="card-body">
          <p className="cad-nota">
            Nenhuma máquina deste cliente ao seu alcance: ele não é o dono atual de nenhuma pela sincronia do parque, não
            comprou nenhuma no ART e não é o dono confirmado de nenhuma — nas filiais que você alcança.
          </p>
        </div>
      )}
      {pagina && pagina.itens.length > 0 && (
        <div className="cad-tabela-wrap">
          <table className="cad-tabela">
            <thead>
              <tr>
                <th scope="col">Chassi</th>
                <th scope="col">Modelo e classificação</th>
                <th scope="col">Relação com o cliente</th>
                <th scope="col">Origem</th>
              </tr>
            </thead>
            <tbody>
              {pagina.itens.map((maquina) => (
                <tr key={maquina.chave}>
                  <td className="cad-mono">
                    <Link to={`/equipamentos/${maquina.chave}`}>{maquina.chassi}</Link>
                  </td>
                  <td>
                    {maquina.modeloNome ?? maquina.produtoNaOrigem ?? <span className="cad-vazio">sem modelo</span>}
                    <div className="cad-sub">
                      {maquina.marca ?? 'marca não informada'}
                      {maquina.anoModelo ? ` · ${maquina.anoModelo}` : ''}
                      {maquina.classificacaoNome ? ` · ${maquina.classificacaoNome}` : ''}
                    </div>
                  </td>
                  <td>
                    <RelacaoComOCliente maquina={maquina} />
                  </td>
                  <td>{maquina.origem === 'Art' ? <span className="cad-selo cad-selo-art">ART</span> : maquina.origem}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      {pagina && pagina.total > 0 && (
        <div className="card-body">
          <p className="cad-nota">
            {pagina.total > LINHAS
              ? `${pagina.total.toLocaleString('pt-BR')} máquinas no total; as ${LINHAS} primeiras estão acima. `
              : `${pagina.total.toLocaleString('pt-BR')} ${pagina.total === 1 ? 'máquina' : 'máquinas'}. `}
            <Link to={`/equipamentos?cliente=${chaveDoCliente}`}>Ver na lista de equipamentos</Link>
          </p>
        </div>
      )}
    </div>
  );
}
