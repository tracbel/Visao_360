/**
 * OS TOTAIS DA TABELA, NA DICA DO TÍTULO (decisão 3 do usuário, 23/09/2026 —
 * fase 4).
 *
 * Eram quatro tipos de linha no fim da tabela — "Total da ADR", "São Paulo fora
 * da ADR", os grupos fora do mapa e "Total da consulta". A maquete não os tem, e
 * com a paginação um total embaixo da página 3 leria como a soma de dez
 * municípios. Eles vieram para cá inteiros, na mesma ordem e com os mesmos
 * números: a conferência do documento 32 continua a um Tab do título.
 *
 * A CONTA NÃO É FEITA AQUI: é `conferenciaDaConsulta`, pura e testada. Este
 * componente só a escreve.
 */

import { reaisCompactos } from '../escalas';
import { nº } from '../indicadoresDaAdr';
import type { ConferenciaDaConsulta, LinhaDaConferencia } from '../totaisDaAdr';

/**
 * Uma linha da conferência. `daAdr` é a linha que TEM parque a somar: sem
 * nenhum município com parque, ela diz o traço e o porquê — e não "0 máquinas".
 * Os grupos de fora não têm parque nenhum, e ficam sem a medida.
 */
function Linha({ linha, daAdr = false }: { linha: LinhaDaConferencia; daAdr?: boolean }) {
  return (
    <li data-conferencia={linha.rotulo}>
      <strong>{linha.rotulo}</strong>
      {linha.descricao && <> ({linha.descricao})</>}: {nº(linha.elegiveis)} elegíveis · {nº(linha.cobertos)} no prazo ·{' '}
      {nº(linha.pendentes)} pendentes · vendas {reaisCompactos(linha.vendas)} · pós-venda {reaisCompactos(linha.posVenda)}
      {linha.maquinas !== null ? (
        <> · {nº(linha.maquinas)} máquinas teóricas</>
      ) : (
        daAdr && <> · — máquinas teóricas (nenhum município com parque: sem regra de potencial vigente ou sem área divulgada)</>
      )}
    </li>
  );
}

export function ConferenciaDosTotais({ conferencia }: { conferencia: ConferenciaDaConsulta }) {
  return (
    <>
      {/* A CONFERÊNCIA É A DO DOCUMENTO 32 — a referência mora aqui, e não na
          dica: o número do documento não diz nada a quem lê. */}
      <p>
        <strong>A soma das linhas é o total da consulta</strong> — é a conferência da tabela: se não fechar, algo
        sumiu ou foi contado duas vezes. As linhas de total saíram do fim da tabela e estão aqui.
      </p>
      <ul>
        {conferencia.adr ? (
          <Linha linha={conferencia.adr} daAdr />
        ) : (
          <li>
            <strong>Total da ADR</strong>: território não carregado neste banco — as linhas abaixo são o que a consulta
            encontrou.
          </li>
        )}
        {conferencia.parcelas.map((p) => (
          <Linha key={p.rotulo} linha={p} />
        ))}
        {conferencia.consulta && <Linha linha={conferencia.consulta} />}
      </ul>
      {!conferencia.consulta && (
        <p>
          Com filtro de sub-região ou de loja, São Paulo fora da ADR e o total da consulta não entram: seriam de outro
          recorte.
        </p>
      )}
    </>
  );
}
