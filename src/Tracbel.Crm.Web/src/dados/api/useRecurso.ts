/**
 * O hook de leitura das telas ligadas à API.
 *
 * OS QUATRO ESTADOS SÃO O CONTRATO: carregando, com dado, vazio e com erro
 * (documento 05-MELHORIAS-COMBINADAS §3). Nenhuma tela pode ficar em branco
 * enquanto busca, nem mostrar "nenhum resultado" quando o que houve foi uma
 * falha — são coisas diferentes e pedem ações diferentes do usuário.
 *
 * DUAS DECISÕES QUE VALEM PARA TODA LISTA:
 *
 * - **O dado anterior fica na tela enquanto o próximo carrega** (`recarregando`).
 *   Trocar de página apagando a tabela faz o conteúdo piscar e a página pular; o
 *   usuário perde o lugar onde estava olhando.
 * - **A procedência sai junto do dado**, do mesmo envelope, porque foi lida no
 *   mesmo instante. Guardá-las separadas deixaria a tela mostrar o carimbo de
 *   uma leitura ao lado das linhas de outra.
 */

import { useCallback, useEffect, useRef, useState } from 'react';
import type { Procedencia } from '../../tipos/api';

/** O que uma tela recebe de uma leitura da API. */
export type Leitura<T> = {
  dados: T | null;
  procedencia: Procedencia | null;
  erro: Error | null;
  /** Primeira carga: ainda não há nada para mostrar. */
  carregando: boolean;
  /** Recarga: já há dado na tela e outro está a caminho. */
  recarregando: boolean;
  /** Refaz a leitura. É o que o botão "tentar de novo" chama. */
  recarregar: () => void;
};

/**
 * Lê um recurso envelopado com procedência.
 *
 * A procedência é aceita como nula para o caso do formulário de CADASTRO, que
 * usa a mesma tela da ficha e não tem leitura nenhuma para carimbar. Inventar um
 * carimbo ali seria a tela afirmando uma origem que não existe.
 *
 * @param buscar A chamada da API. Recebe o sinal de cancelamento.
 * @param dependencias O que, ao mudar, obriga a reler. A filial entra sempre.
 */
export function useRecurso<T>(
  buscar: (sinal: AbortSignal) => Promise<{ dados: T; procedencia: Procedencia | null }>,
  dependencias: readonly unknown[],
): Leitura<T> {
  const [dados, setDados] = useState<T | null>(null);
  const [procedencia, setProcedencia] = useState<Procedencia | null>(null);
  const [erro, setErro] = useState<Error | null>(null);
  const [ocupado, setOcupado] = useState(true);
  const [gatilho, setGatilho] = useState(0);

  // A busca muda de identidade a cada render (é uma seta declarada na tela); a
  // referência mantém a versão corrente sem transformar isso numa dependência
  // que dispararia a leitura em todo render.
  const buscarRef = useRef(buscar);
  buscarRef.current = buscar;

  const temDado = dados !== null;

  useEffect(() => {
    const controlador = new AbortController();
    setOcupado(true);

    buscarRef
      .current(controlador.signal)
      .then((resposta) => {
        if (controlador.signal.aborted) return;
        setDados(resposta.dados);
        setProcedencia(resposta.procedencia);
        setErro(null);
        setOcupado(false);
      })
      .catch((causa: unknown) => {
        if (controlador.signal.aborted) return;
        // O dado anterior é descartado junto: mostrar tabela velha ao lado de uma
        // mensagem de erro faria o usuário agir sobre o que não vale mais.
        setDados(null);
        setProcedencia(null);
        setErro(causa instanceof Error ? causa : new Error(String(causa)));
        setOcupado(false);
      });

    return () => controlador.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencias, gatilho]);

  const recarregar = useCallback(() => setGatilho((n) => n + 1), []);

  return {
    dados,
    procedencia,
    erro,
    carregando: ocupado && !temDado,
    recarregando: ocupado && temDado,
    recarregar,
  };
}
