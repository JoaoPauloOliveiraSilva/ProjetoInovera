import { createContext, useCallback, useContext, useEffect, useState } from 'react';

/**
 * Sinal partilhado: quando uma ação altera ideias (gosto, comentário, avaliação...), incrementa-se a versão
 * e todas as listas abertas voltam a pedir os dados.
 */
export const VersaoDados = createContext<{ versao: number; atualizar: () => void }>({ versao: 0, atualizar: () => {} });

export function useAtualizar(): () => void {
  return useContext(VersaoDados).atualizar;
}

interface Estado<T> {
  dados: T | undefined;
  erro: string | null;
  aCarregar: boolean;
}

/** Faz o pedido ao montar, quando as dependências mudam e quando os dados são alterados noutro sítio. */
export function usePedido<T>(funcao: () => Promise<T>, dependencias: unknown[]): Estado<T> & { recarregar: () => void } {
  const { versao } = useContext(VersaoDados);
  const [local, setLocal] = useState(0);
  const [estado, setEstado] = useState<Estado<T>>({ dados: undefined, erro: null, aCarregar: true });

  useEffect(() => {
    let ativo = true;
    setEstado((e) => ({ ...e, aCarregar: true }));
    funcao()
      .then((dados) => ativo && setEstado({ dados, erro: null, aCarregar: false }))
      .catch((e: unknown) => ativo && setEstado({ dados: undefined, erro: e instanceof Error ? e.message : String(e), aCarregar: false }));
    return () => {
      ativo = false;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...dependencias, versao, local]);

  const recarregar = useCallback(() => setLocal((n) => n + 1), []);
  return { ...estado, recarregar };
}
