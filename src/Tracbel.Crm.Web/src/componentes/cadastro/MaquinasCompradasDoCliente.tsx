/**
 * As máquinas que o cliente comprou — o cartão da ficha do cliente que lê o
 * vínculo "comprador na venda" das vendas de máquina (documento 35, seções 10
 * e 11).
 *
 * COMPRAR NÃO FAZ DONO. Uma máquina revendida teve dois compradores, e o ART
 * não diz quem a tem hoje: a coluna "Dono atual" diz "sim" quando a sincronia do
 * parque aponta este cliente como dono atual (vínculo com evidência, desde
 * 24/09/2026) ou quando o cadastro da máquina o tem como dono confirmado.
 *
 * 29/09/2026 — o cartão virou o painel das outras telas (`PainelDoMomento`) e a tabela, `mom-tabela` (#293, bloco 5).
 * A nota de que comprar não faz dono foi para a dica do painel, com o mesmo texto.
 */

import { Link } from 'react-router-dom';
import { listarMaquinasCompradasPeloCliente } from '../../dados/api/equipamentos';
import type { ContextoDeAcesso } from '../../dados/api/http';
import { useRecurso } from '../../dados/api/useRecurso';
import type { MaquinaCompradaPeloCliente } from '../../tipos/api';
import { PainelDoMomento } from '../mercado/momento/pecas';
import { BlocoCarregando, BlocoErro } from './EstadosDeTela';
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
    <PainelDoMomento
      titulo="Máquinas compradas"
      data-bloco="maquinas-compradas"
      subtitulo="As vendas de máquina em que este cliente foi o comprador, da mais recente para a mais antiga"
      dica="O comprador de uma venda não vira dono automaticamente: o dono atual vem da sincronia do parque, com a evidência — é o cartão “Frota do cliente”, acima."
    >
      {leitura.carregando && <BlocoCarregando oQue="as máquinas compradas" />}
      {leitura.erro && <BlocoErro erro={leitura.erro} aoTentarDeNovo={leitura.recarregar} />}
      {leitura.dados && leitura.dados.length === 0 && (
        <p className="cad-nota">Nenhuma venda de máquina registrada com este cliente como comprador.</p>
      )}
      {leitura.dados && leitura.dados.length > 0 && (
        <div className="mom-tabela-rolagem">
          <table className="mom-tabela">
            <caption className="cad-so-leitor">As vendas de máquina em que o cliente foi o comprador</caption>
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
                  <th scope="row" className="cad-mono">
                    <Link to={`/equipamentos/${maquina.equipamentoChave}`}>{maquina.chassi}</Link>
                  </th>
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
    </PainelDoMomento>
  );
}
