import { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import manual from '../ajuda/manual.md?raw';
import { Cabecalho } from '../componentes/Layout';
import { Markdown, seccoes } from '../componentes/Markdown';

const indice = seccoes(manual);

/** Manual do utilizador dentro da plataforma. */
export function Ajuda() {
  const [parametros] = useSearchParams();
  const seccao = parametros.get('seccao');

  useEffect(() => {
    if (seccao) document.getElementById(seccao)?.scrollIntoView({ block: 'start' });
  }, [seccao]);

  return (
    <>
      <Cabecalho titulo="ajuda" corLampada="#8a8a8a" />
      <div className="ajuda">
        <nav className="ajuda-indice" aria-label="Índice do manual">
          <div className="rotulo">manual do utilizador</div>
          <ol>
            {indice.map((s) => (
              <li key={s.id}>
                <a
                  href={`#${s.id}`}
                  className={s.id === seccao ? 'ativa' : undefined}
                  onClick={(e) => {
                    e.preventDefault();
                    document.getElementById(s.id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
                  }}
                >
                  {s.titulo.replace(/^\d+\.\s*/, '')}
                </a>
              </li>
            ))}
          </ol>
        </nav>
        <article className="painel ajuda-conteudo">
          <Markdown texto={manual} />
        </article>
      </div>
    </>
  );
}
