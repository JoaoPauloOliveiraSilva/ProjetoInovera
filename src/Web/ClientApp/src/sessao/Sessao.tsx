import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { ErroApi } from '../api/cliente';
import type { Permissao } from '../api/permissoes';
import { contas } from '../api/servicos';
import type { UtilizadorAtual } from '../api/tipos';

interface Sessao {
  /** undefined = ainda a verificar; null = sem sessão. */
  utilizador: UtilizadorAtual | null | undefined;
  entrar: (email: string, password: string) => Promise<void>;
  sair: () => Promise<void>;
  /** Volta a ler o utilizador (ex.: depois de definir a password, que já inicia sessão). */
  recarregar: () => Promise<void>;
}

const Contexto = createContext<Sessao | null>(null);

export function SessaoProvider({ children }: { children: ReactNode }) {
  const [utilizador, setUtilizador] = useState<UtilizadorAtual | null | undefined>(undefined);

  const carregar = useCallback(async () => {
    try {
      setUtilizador(await contas.eu());
    } catch (e) {
      if (e instanceof ErroApi && (e.estado === 401 || e.estado === 403)) {
        setUtilizador(null);
      } else {
        // API em baixo: tratamos como sem sessão para mostrar o ecrã de entrada.
        setUtilizador(null);
      }
    }
  }, []);

  useEffect(() => {
    void carregar();
  }, [carregar]);

  const sessao = useMemo<Sessao>(
    () => ({
      utilizador,
      entrar: async (email, password) => {
        await contas.entrar(email.trim(), password);
        await carregar();
      },
      sair: async () => {
        try {
          await contas.sair();
        } finally {
          setUtilizador(null);
        }
      },
      recarregar: carregar,
    }),
    [utilizador, carregar],
  );

  return <Contexto.Provider value={sessao}>{children}</Contexto.Provider>;
}

export function useSessao(): Sessao {
  const sessao = useContext(Contexto);
  if (!sessao) {
    throw new Error('useSessao tem de ser usado dentro de SessaoProvider');
  }
  return sessao;
}

/** O utilizador com sessão iniciada (só usar dentro de páginas protegidas). */
export function useUtilizador(): UtilizadorAtual {
  const { utilizador } = useSessao();
  if (!utilizador) {
    throw new Error('Sem sessão iniciada');
  }
  return utilizador;
}

/** Diz se o utilizador com sessão tem a permissão (para mostrar menus e botões). */
export function usePermissao(): (permissao: Permissao) => boolean {
  const { utilizador } = useSessao();
  return (permissao) => utilizador?.permissoes.includes(permissao) ?? false;
}

/** Só mostra o conteúdo com sessão iniciada (e com a permissão, se indicada); senão manda para /entrar e volta depois. */
export function Protegido({ children, permissao }: { children: ReactNode; permissao?: Permissao }) {
  const { utilizador } = useSessao();
  const { pathname } = useLocation();

  if (utilizador === undefined) {
    return <div className="a-carregar">a carregar…</div>;
  }
  if (utilizador === null) {
    return <Navigate to={`/entrar?voltar=${encodeURIComponent(pathname)}`} replace />;
  }
  if (permissao && !utilizador.permissoes.includes(permissao)) {
    return <Navigate to="/inovacao" replace />;
  }
  return <>{children}</>;
}
