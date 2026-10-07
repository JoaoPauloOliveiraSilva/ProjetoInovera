import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { ErroApi } from '../api/cliente';
import { contas } from '../api/servicos';
import type { UtilizadorAtual } from '../api/tipos';

interface Sessao {
  /** undefined = ainda a verificar; null = sem sessão. */
  utilizador: UtilizadorAtual | null | undefined;
  entrar: (email: string, password: string) => Promise<void>;
  sair: () => Promise<void>;
  registar: (dados: { nome: string; email: string; password: string; empresa: string | null }) => Promise<void>;
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
        try {
          await contas.entrar(email.trim(), password);
        } catch (e) {
          if (e instanceof ErroApi && e.estado === 401) {
            throw new ErroApi(401, 'Email ou palavra-passe errados.');
          }
          throw e;
        }
        await carregar();
      },
      sair: async () => {
        try {
          await contas.sair();
        } finally {
          setUtilizador(null);
        }
      },
      registar: async (dados) => {
        await contas.registar(dados);
        await contas.entrar(dados.email.trim(), dados.password);
        await carregar();
      },
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

/** Só mostra o conteúdo com sessão iniciada; senão manda para /entrar e volta depois. */
export function Protegido({ children, soAdministrador = false }: { children: ReactNode; soAdministrador?: boolean }) {
  const { utilizador } = useSessao();
  const { pathname } = useLocation();

  if (utilizador === undefined) {
    return <div className="a-carregar">a carregar…</div>;
  }
  if (utilizador === null) {
    return <Navigate to={`/entrar?voltar=${encodeURIComponent(pathname)}`} replace />;
  }
  if (soAdministrador && !utilizador.eAdministrador) {
    return <Navigate to="/inovacao" replace />;
  }
  return <>{children}</>;
}
