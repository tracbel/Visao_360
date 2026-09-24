/**
 * AS COLUNAS DA TABELA DE MUNICÍPIOS E A ESCOLHA DE QUAIS APARECEM (fidelidade
 * às maquetes, 23/09/2026 — fase 4; a engrenagem da maquete).
 *
 * MUNICÍPIO E AÇÃO NÃO SE ESCONDEM: um é a chave da linha, o outro é a porta da
 * ficha. As outras sete são escolha de quem lê.
 *
 * A ESCOLHA FICA NO NAVEGADOR (`localStorage`), e só ela: é preferência de
 * leitura de uma pessoa numa máquina, não dado da empresa — não vai à API nem à
 * URL, que é do recorte (issue 163). E o `localStorage` pode não existir ou
 * recusar (janela privada, cota, política da empresa): cada acesso está num
 * `try`, e sem ele a tabela abre com todas as colunas, como se fosse a primeira
 * vez.
 */

import { useCallback, useState } from 'react';

export const COLUNAS_OPCIONAIS = [
  { id: 'hierarquia', rotulo: 'Sub-região · Loja' },
  { id: 'elegiveis', rotulo: 'Elegíveis' },
  { id: 'noPrazo', rotulo: 'No prazo' },
  { id: 'pendentes', rotulo: 'Pendentes' },
  { id: 'vendas', rotulo: 'Vendas' },
  { id: 'posVenda', rotulo: 'Pós-venda (provisório)' },
  { id: 'maquinas', rotulo: 'Máquinas (teórico)' },
] as const;

export type ColunaOpcional = (typeof COLUNAS_OPCIONAIS)[number]['id'];

/** A chave no navegador. Versionada: se as colunas mudarem, a escolha velha não vale. */
export const CHAVE_DAS_COLUNAS = 'tracbel.indicadores.territorio.colunasOcultas.v1';

const ehColuna = (v: unknown): v is ColunaOpcional => COLUNAS_OPCIONAIS.some((c) => c.id === v);

function lerOcultas(): ColunaOpcional[] {
  try {
    const bruto = localStorage.getItem(CHAVE_DAS_COLUNAS);
    if (!bruto) return [];
    const lido: unknown = JSON.parse(bruto);
    // O QUE VEIO DO NAVEGADOR É CONFERIDO: uma chave editada à mão, ou de uma
    // versão antiga, não pode esconder uma coluna que não existe nem quebrar a tela.
    return Array.isArray(lido) ? lido.filter(ehColuna) : [];
  } catch {
    return [];
  }
}

function gravarOcultas(ocultas: ColunaOpcional[]) {
  try {
    localStorage.setItem(CHAVE_DAS_COLUNAS, JSON.stringify(ocultas));
  } catch {
    // Sem onde guardar, a escolha vale até a página fechar — e a tabela segue.
  }
}

/** As colunas escondidas por escolha da pessoa, lidas uma vez e gravadas a cada troca. */
export function useColunasOcultas() {
  const [ocultas, setOcultas] = useState<ColunaOpcional[]>(lerOcultas);

  const alternar = useCallback((coluna: ColunaOpcional) => {
    setOcultas((atuais) => {
      const proximas = atuais.includes(coluna) ? atuais.filter((c) => c !== coluna) : [...atuais, coluna];
      gravarOcultas(proximas);
      return proximas;
    });
  }, []);

  const mostrarTodas = useCallback(() => {
    gravarOcultas([]);
    setOcultas([]);
  }, []);

  return { ocultas, alternar, mostrarTodas };
}
