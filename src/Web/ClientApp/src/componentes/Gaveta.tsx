import { useEffect } from 'react';
import type { ReactNode } from 'react';
import { Fechar } from './Icones';

/** Painel lateral à direita, como no site atual (formulários e detalhes). */
export function Gaveta({ aberta, aoFechar, children }: { aberta: boolean; aoFechar: () => void; children: ReactNode }) {
  useEffect(() => {
    if (!aberta) return;
    const tecla = (e: KeyboardEvent) => {
      if (e.key === 'Escape') aoFechar();
    };
    window.addEventListener('keydown', tecla);
    return () => window.removeEventListener('keydown', tecla);
  }, [aberta, aoFechar]);

  if (!aberta) return null;

  return (
    <div className="gaveta-fundo" onClick={aoFechar}>
      <aside className="gaveta" role="dialog" aria-modal="true" onClick={(e) => e.stopPropagation()}>
        <button className="gaveta-fechar" onClick={aoFechar} aria-label="Fechar">
          <Fechar />
        </button>
        <div className="gaveta-conteudo">{children}</div>
      </aside>
    </div>
  );
}
