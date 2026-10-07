import { createContext, useContext, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import type { ClasseIdeia, Ideia, Notas, Responsabilidade } from './tipos';
import { ideiasExemplo, utilizadorAtual } from './exemplo';
import { calcularNota, limiarAprovacao } from './regras';

export type NovaIdeia = Omit<Ideia, 'num' | 'data' | 'estado' | 'likes' | 'gostei' | 'comentarios'>;

interface IdeiasApi {
  ideias: Ideia[];
  registar: (nova: NovaIdeia) => number;
  alternarGosto: (num: number) => void;
  comentar: (num: number, texto: string) => void;
  alternarGostoComentario: (num: number, comentarioId: number) => void;
  avaliar: (num: number, responsabilidade: Responsabilidade, notas: Notas, decisaoDst: boolean | null, classe: ClasseIdeia | null) => void;
}

const Contexto = createContext<IdeiasApi | null>(null);

export function IdeiasProvider({ children }: { children: ReactNode }) {
  const [ideias, setIdeias] = useState<Ideia[]>(ideiasExemplo);

  const api = useMemo<IdeiasApi>(() => {
    const atualizar = (num: number, f: (i: Ideia) => Ideia) =>
      setIdeias((lista) => lista.map((i) => (i.num === num ? f(i) : i)));

    return {
      ideias,
      registar: (nova) => {
        const num = Math.max(...ideias.map((i) => i.num)) + 1;
        const ideia: Ideia = {
          ...nova,
          num,
          data: new Date().toISOString(),
          estado: nova.autores.length > 1 ? 'validacao-autores' : 'validacao-equipa',
          likes: 0,
          gostei: false,
          comentarios: [],
        };
        setIdeias((lista) => [ideia, ...lista]);
        return num;
      },
      alternarGosto: (num) =>
        atualizar(num, (i) => ({ ...i, gostei: !i.gostei, likes: i.likes + (i.gostei ? -1 : 1) })),
      comentar: (num, texto) =>
        atualizar(num, (i) => ({
          ...i,
          comentarios: [
            ...i.comentarios,
            {
              id: Date.now(),
              autor: utilizadorAtual.nomeCompleto,
              empresa: utilizadorAtual.empresa,
              texto,
              data: new Date().toISOString(),
              gostos: 0,
              gostei: false,
            },
          ],
        })),
      alternarGostoComentario: (num, comentarioId) =>
        atualizar(num, (i) => ({
          ...i,
          comentarios: i.comentarios.map((c) =>
            c.id === comentarioId ? { ...c, gostei: !c.gostei, gostos: c.gostos + (c.gostei ? -1 : 1) } : c,
          ),
        })),
      avaliar: (num, responsabilidade, notas, decisaoDst, classe) =>
        atualizar(num, (i) => {
          const nota = calcularNota(notas);
          const aprovada = responsabilidade === 'dst' ? decisaoDst === true : nota !== null && nota >= limiarAprovacao;
          return {
            ...i,
            responsabilidade,
            notas,
            estado: aprovada ? 'aprovada' : 'nao-aprovada',
            classe: classe ?? undefined,
          };
        }),
    };
  }, [ideias]);

  return <Contexto.Provider value={api}>{children}</Contexto.Provider>;
}

export function useIdeias(): IdeiasApi {
  const api = useContext(Contexto);
  if (!api) {
    throw new Error('useIdeias tem de ser usado dentro de IdeiasProvider');
  }
  return api;
}
