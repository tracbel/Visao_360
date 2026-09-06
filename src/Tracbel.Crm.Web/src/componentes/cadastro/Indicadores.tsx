/**
 * Os cartões de número no topo de uma tela.
 *
 * TRÊS REGRAS, e as três vêm da mesma decisão de não afirmar o que não se sabe:
 *
 * 1. **Todo número tem uma origem escrita embaixo dele.** `de onde` diz de qual
 *    consulta o número saiu — não é enfeite: é o que permite conferir. Um
 *    cartão com "4.067" e nada mais é indistinguível de um cartão com um número
 *    inventado.
 * 2. **`null` não vira zero.** Quando o valor não existe, o cartão mostra um
 *    travessão e o motivo, porque zero afirma uma medição que não houve.
 * 3. **A grade se reorganiza sozinha.** `auto-fit` com largura mínima, e não um
 *    número fixo de colunas: as telas foram portadas de um protótipo fixo em
 *    1.280px e nunca souberam ser estreitas.
 */

import type { ReactNode } from 'react';

/** Um número, o que ele é, e de onde ele saiu. */
export type Indicador = {
  rotulo: string;
  /** O valor. `null` quando não há dado — vira travessão, nunca zero. */
  valor: number | string | null;
  /** De qual consulta o número saiu. Aparece embaixo, em letra menor. */
  deOnde: string;
  /** Chama atenção: `atencao` para o que está vencido, `bom` para o que está em dia. */
  tom?: 'neutro' | 'atencao' | 'bom';
  /** Quando o valor é nulo, o que dizer no lugar. */
  semDado?: string;
};

/** A faixa de cartões. Some inteira quando não há nenhum indicador. */
export function PainelDeIndicadores({
  indicadores,
  carregando = false,
}: {
  indicadores: Indicador[];
  /** Enquanto busca, os cartões viram esqueletos em vez de sumirem e voltarem. */
  carregando?: boolean;
}) {
  if (indicadores.length === 0) return null;

  return (
    <div className="cad-kpis">
      {indicadores.map((i) => (
        <div key={i.rotulo} className={`cad-kpi cad-kpi-${i.tom ?? 'neutro'}`}>
          <div className="cad-kpi-rotulo">{i.rotulo}</div>
          <div className="cad-kpi-valor">
            {carregando ? (
              <span className="cad-kpi-esqueleto" aria-hidden="true" />
            ) : i.valor === null ? (
              <span className="cad-vazio">—</span>
            ) : (
              formatar(i.valor)
            )}
          </div>
          <div className="cad-kpi-fonte">{i.valor === null && i.semDado ? i.semDado : i.deOnde}</div>
        </div>
      ))}
    </div>
  );
}

/** Número com separador de milhar; texto passa direto. */
function formatar(valor: number | string): ReactNode {
  return typeof valor === 'number' ? valor.toLocaleString('pt-BR') : valor;
}
