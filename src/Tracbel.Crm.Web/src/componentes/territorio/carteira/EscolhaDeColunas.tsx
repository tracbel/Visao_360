/**
 * A ENGRENAGEM DA MAQUETE: escolher quais colunas da tabela de municípios
 * aparecem (fidelidade às maquetes, 23/09/2026 — fase 4).
 *
 * A fase T4.9 deixou a engrenagem de fora dizendo que ela "é um painel de
 * preferências por usuário, e não acabamento visual". Continua sendo — e é por
 * isso que a escolha mora no navegador de quem escolheu (ver `colunas.ts`), e
 * não na URL nem na API.
 *
 * RADIX POPOVER pelo mesmo motivo do "Mais filtros": foco preso enquanto aberto,
 * devolvido ao fechar, `Esc`, clique fora e posição que não vaza a janela.
 */

import * as Popover from '@radix-ui/react-popover';
import { Settings } from 'lucide-react';
import { COLUNAS_OPCIONAIS, type ColunaOpcional } from './colunas';

export function EscolhaDeColunas({
  ocultas,
  aoAlternar,
  aoMostrarTodas,
}: {
  ocultas: readonly ColunaOpcional[];
  aoAlternar: (coluna: ColunaOpcional) => void;
  aoMostrarTodas: () => void;
}) {
  return (
    <Popover.Root>
      <Popover.Trigger asChild>
        <button type="button" className="terr-engrenagem" aria-label="Escolher as colunas da tabela" data-bloco="escolher-colunas">
          <Settings size={16} strokeWidth={2} aria-hidden="true" />
          {/* QUANTAS ESTÃO ESCONDIDAS, no próprio botão: uma coluna fora da vista
              sem aviso faria a tabela parecer completa. */}
          {ocultas.length > 0 && <span className="terr-engrenagem-selo">{ocultas.length}</span>}
        </button>
      </Popover.Trigger>
      <Popover.Portal>
        <Popover.Content className="terr-cart-popover" sideOffset={6} collisionPadding={16} align="end">
          <fieldset className="terr-colunas">
            <legend className="terr-cart-popover-titulo">Colunas visíveis</legend>
            {COLUNAS_OPCIONAIS.map((c) => (
              <label key={c.id} className="terr-coluna-opcao">
                <input type="checkbox" checked={!ocultas.includes(c.id)} onChange={() => aoAlternar(c.id)} />
                {c.rotulo}
              </label>
            ))}
          </fieldset>
          {/* A PROMESSA É A QUE A TELA CUMPRE: Município e Ação não saem nunca —
              nem pela engrenagem, nem pela largura. As outras, em tabela
              estreita (ficha aberta ao lado, tablet, celular), saem sozinhas, e
              o que sai continua inteiro na ficha do município. */}
          <p className="terr-cart-popover-nota">
            Município e Ação ficam sempre: um é a chave da linha, o outro abre a ficha. Em tabela estreita, algumas das
            outras colunas saem sozinhas — o número delas continua na ficha do município. A escolha fica guardada neste
            navegador.
          </p>
          <div className="terr-cart-popover-acoes">
            <button type="button" className="btn btn-secondary btn-sm" onClick={aoMostrarTodas} disabled={ocultas.length === 0}>
              Mostrar todas
            </button>
            <Popover.Close className="btn btn-secondary btn-sm">Fechar</Popover.Close>
          </div>
        </Popover.Content>
      </Popover.Portal>
    </Popover.Root>
  );
}
