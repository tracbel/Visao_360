import { useCallback, useEffect, useState } from 'react';
import { carregar, esquecer } from './carregar';

type Estado<T> = {
  dados: T | null;
  erro: Error | null;
  carregando: boolean;
  /** Descarta o cache e busca de novo. É o que o botão "Tentar de novo" chama. */
  recarregar: () => void;
};

/**
 * Hook de leitura de um arquivo de dados, com estados de carga e erro.
 *
 * `recarregar` existe porque o padrão de tela (documento 07, seção 5) exige o
 * botão **Tentar de novo** em todo erro de leitura, e sem ele o botão não teria
 * o que chamar: `carregar()` guarda a resposta em cache, então repetir a
 * chamada devolveria o mesmo erro. Por isso ele esquece a entrada antes.
 */
export function useDados<T>(nome: string): Estado<T> {
  const [tentativa, setTentativa] = useState(0);
  const [estado, setEstado] = useState<Omit<Estado<T>, 'recarregar'>>({
    dados: null,
    erro: null,
    carregando: true,
  });

  const recarregar = useCallback(() => {
    esquecer(nome);
    setTentativa((n) => n + 1);
  }, [nome]);

  useEffect(() => {
    let ativo = true;
    setEstado({ dados: null, erro: null, carregando: true });
    carregar<T>(nome)
      .then((dados) => ativo && setEstado({ dados, erro: null, carregando: false }))
      .catch((erro: Error) => ativo && setEstado({ dados: null, erro, carregando: false }));
    return () => {
      ativo = false;
    };
  }, [nome, tentativa]);

  return { ...estado, recarregar };
}
