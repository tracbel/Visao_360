/**
 * O QUE A MÁQUINA É PARA O CLIENTE — dono atual, dono confirmado, comprador no ART — e com que evidência.
 *
 * 27/09/2026. A frota do cliente passou a vir do vínculo de dono atual da sincronia do parque (decisão de 24/09,
 * PR #238): 22.287 máquinas de 6.165 clientes, contra 2.440 de 1.181 pelo dono confirmado. As três relações podem
 * valer juntas e cada uma é uma afirmação diferente, então cada uma tem o nome dela na tela:
 *
 * - **dono atual** — o vínculo vigente, com a evidência que o sustenta (a nota de venda do Protheus, a ordem de
 *   serviço, só o cadastro antigo, ou a venda no ART) e a data dela;
 * - **dono confirmado** — o cadastro da máquina aponta o cliente, porque o Protheus e o ART concordam ou porque uma
 *   pessoa confirmou;
 * - **comprou no ART** — o comprador de uma venda, que fica como HISTÓRICO mesmo quando a máquina já é de outro.
 *
 * O mesmo desenho serve ao 360 do cliente, à lista de equipamentos filtrada por cliente e à ficha do cliente.
 */

import type { EquipamentoResumo } from '../../tipos/api';
import { InfoTooltip } from '../InfoTooltip';
import '../../estilos/frota-comercial.css';

/** A evidência do dono atual, em palavra — nunca o código do enum. */
const ROTULO_DA_EVIDENCIA: Record<string, string> = {
  NotaDeVenda: 'nota de venda no Protheus',
  OrdemDeServico: 'ordem de serviço no Protheus',
  CadastroAntigo: 'só o cadastro antigo do Protheus',
  VendaNoArt: 'venda no ART',
};

/** O que cada evidência quer dizer — o texto da dica, o mesmo da decisão de 24/09/2026. */
const EXPLICACAO_DA_EVIDENCIA: Record<string, string> = {
  NotaDeVenda: 'A nota de venda válida mais recente da máquina no Protheus é para este cliente.',
  OrdemDeServico: 'Não há nota de venda para ele, mas a ordem de serviço mais recente da oficina é dele.',
  CadastroAntigo:
    'Nem nota de venda nem ordem de serviço apontam para ele: o dono veio de uma carga antiga do cadastro de veículos ' +
    'do Protheus e ninguém o confirmou depois. É a evidência mais fraca.',
  VendaNoArt:
    'A venda no ART: o comprador do ART prevaleceu sobre o Protheus (sem evidência depois da venda, ou com o Protheus ' +
    'apontando a própria Tracbel), ou o Protheus não tem a máquina.',
};

/** `2025-03-10` vira `10/03/2025`. A data sem hora da API não passa por fuso. */
function dataCurta(valor: string | null): string {
  if (!valor) return '—';
  const [ano, mes, dia] = valor.slice(0, 10).split('-');
  return `${dia}/${mes}/${ano}`;
}

/** A evidência do dono atual, com a data e a dica do que ela quer dizer. */
export function EvidenciaDoDono({ evidencia, em }: { evidencia: string | null; em: string | null }) {
  if (!evidencia) return null;
  return (
    <span className="frota-evidencia">
      por {ROTULO_DA_EVIDENCIA[evidencia] ?? evidencia}
      {em ? ` de ${dataCurta(em)}` : ''}
      {EXPLICACAO_DA_EVIDENCIA[evidencia] && (
        <InfoTooltip texto={EXPLICACAO_DA_EVIDENCIA[evidencia]} rotulo="O que esta evidência quer dizer" />
      )}
    </span>
  );
}

/**
 * A relação da máquina com o cliente do filtro — os selos e, quando ele não é o dono atual, quem é.
 *
 * Só existe na listagem filtrada por cliente (`relacaoComOCliente`); sem ela, não desenha nada.
 */
export function RelacaoComOCliente({ maquina }: { maquina: EquipamentoResumo }) {
  const relacao = maquina.relacaoComOCliente;
  if (!relacao) return null;

  return (
    <span className="frota-relacao">
      {relacao.ehDonoAtual && (
        <span className="frota-relacao-parte">
          <span className="cad-selo cad-selo-dono-atual">dono atual</span>{' '}
          <EvidenciaDoDono evidencia={maquina.evidenciaDoDonoAtual} em={maquina.evidenciaDoDonoAtualEm} />
        </span>
      )}
      {relacao.ehDonoConfirmado && (
        <span className="frota-relacao-parte">
          <span className="cad-selo cad-selo-dono-confirmado">dono confirmado</span>
          <InfoTooltip
            texto="O cadastro da máquina aponta este cliente: o dono no Protheus e o comprador no ART concordam, ou uma pessoa confirmou."
            rotulo="O que é dono confirmado"
          />
        </span>
      )}
      {relacao.compradaEm && (
        <span className="frota-relacao-parte">
          <span className="cad-selo cad-selo-comprador">comprou no ART</span> em {dataCurta(relacao.compradaEm)}
          {relacao.compradaPeloDonoNoProtheus && (
            <InfoTooltip
              texto="O comprador que o ART nomeia não é cliente do CRM: a venda entrou com o dono atual do Protheus no lugar dele (decisão de 24/09/2026)."
              rotulo="Por que este cliente aparece como comprador"
            />
          )}
        </span>
      )}
      {!relacao.ehDonoAtual && (
        <span className="frota-relacao-parte frota-relacao-outro">
          {maquina.donoAtualNome ? (
            <>
              hoje é de {maquina.donoAtualNome}{' '}
              <EvidenciaDoDono evidencia={maquina.evidenciaDoDonoAtual} em={maquina.evidenciaDoDonoAtualEm} />
            </>
          ) : (
            'sem dono atual ao seu alcance'
          )}
        </span>
      )}
    </span>
  );
}
