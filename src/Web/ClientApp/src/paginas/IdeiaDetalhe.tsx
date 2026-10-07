import { useState } from 'react';
import { Gaveta } from '../componentes/Gaveta';
import { Balao, Coracao, SetaContinuar } from '../componentes/Icones';
import { useIdeias } from '../dados/IdeiasContext';
import { calcularNota, formatarData, haQuantoTempo, variaveis } from '../dados/regras';
import { nomeClasse, nomeEstado, nomeTipo } from '../dados/tipos';
import type { EstadoIdeia, Ideia } from '../dados/tipos';

const passos: { estado: EstadoIdeia; nome: string }[] = [
  { estado: 'validacao-autores', nome: '01. validação dos autores' },
  { estado: 'validacao-equipa', nome: '02. validação equipa inovação' },
  { estado: 'em-discussao', nome: '03. discussão pública' },
  { estado: 'em-avaliacao', nome: '04. avaliação' },
];

function passoAtual(estado: EstadoIdeia): number {
  const i = passos.findIndex((p) => p.estado === estado);
  return i === -1 ? passos.length : i;
}

export function IdeiaGaveta({ num, aoFechar }: { num: number | null; aoFechar: () => void }) {
  const { ideias } = useIdeias();
  const ideia = ideias.find((i) => i.num === num);
  return (
    <Gaveta aberta={ideia !== undefined} aoFechar={aoFechar}>
      {ideia && <IdeiaDetalhe ideia={ideia} />}
    </Gaveta>
  );
}

function IdeiaDetalhe({ ideia }: { ideia: Ideia }) {
  const { alternarGosto, comentar, alternarGostoComentario } = useIdeias();
  const [texto, setTexto] = useState('');
  const atual = passoAtual(ideia.estado);
  const podeParticipar = ideia.estado === 'em-discussao';
  const nota = ideia.notas ? calcularNota(ideia.notas) : null;

  const detalhes: [string, string | undefined][] = [
    ['vantagens', ideia.vantagens],
    ['requisitos', ideia.requisitos],
    ['como é feito actualmente', ideia.comoEFeito],
    ['modelo de negócio', ideia.modeloNegocio],
    ['competidores', ideia.competidores],
    ['custos', ideia.custos],
  ];

  return (
    <article className="detalhe">
      <div className="sobretitulo">INOVAÇÃO</div>
      <h2 className="detalhe-titulo">{ideia.titulo}</h2>
      <div className="detalhe-tipo">{nomeTipo[ideia.tipo]}</div>

      <div className="detalhe-contadores">
        <span>
          <Coracao /> {ideia.likes}
        </span>
        <span>
          <Balao /> {ideia.comentarios.length}
        </span>
      </div>

      <div className="ficha">
        <dl>
          <div className="ficha-autor">
            <span className="avatar">{ideia.anonima ? '?' : (ideia.autores[0] ?? '?')[0]}</span>
          </div>
          <dt>{ideia.autores.length > 1 ? 'autores' : 'autor'}</dt>
          <dd>{ideia.anonima ? 'Autor Anónimo' : ideia.autores.join(', ')}</dd>
          <dt>data</dt>
          <dd>{formatarData(ideia.data)}</dd>
          <dt>estado</dt>
          <dd>{nomeEstado[ideia.estado]}</dd>
          <dt>privada</dt>
          <dd>{ideia.privada ? 'sim' : 'não'}</dd>
        </dl>
        <ol className="fluxo">
          {passos.map((p, i) => (
            <li key={p.estado} className={i === atual ? 'atual' : i < atual ? 'feito' : ''}>
              {p.nome}
            </li>
          ))}
        </ol>
      </div>

      <h3 className="secao">descrição</h3>
      <p className="texto">{ideia.descricao}</p>

      {detalhes
        .filter(([, valor]) => valor)
        .map(([nome, valor]) => (
          <div key={nome}>
            <h3 className="secao">{nome}</h3>
            <p className="texto">{valor}</p>
          </div>
        ))}

      {ideia.anexos.map((a) => (
        <a key={a} className="anexo" href="#" onClick={(e) => e.preventDefault()}>
          {a}
        </a>
      ))}

      {ideia.notas && (
        <div className="resultado">
          <h3 className="secao">avaliação</h3>
          <div className="notas">
            {variaveis.map((v) => (
              <span key={v.chave}>
                {v.nome}: <strong>{ideia.notas?.[v.chave] ?? 'N.A.'}</strong>
              </span>
            ))}
            <span>
              Total: <strong>{nota === null ? 'N.A.' : nota.toFixed(2)}</strong>
            </span>
            {ideia.classe && (
              <span>
                Classe: <strong>{nomeClasse[ideia.classe]}</strong>
              </span>
            )}
          </div>
        </div>
      )}

      <button className={`botao-gosto${ideia.gostei ? ' ativo' : ''}`} onClick={() => alternarGosto(ideia.num)} disabled={!podeParticipar}>
        <Coracao /> gosto
      </button>

      <h3 className="secao grande">comentários</h3>
      {podeParticipar && (
        <form
          className="comentar"
          onSubmit={(e) => {
            e.preventDefault();
            if (texto.trim()) {
              comentar(ideia.num, texto.trim());
              setTexto('');
            }
          }}
        >
          <textarea value={texto} onChange={(e) => setTexto(e.target.value)} aria-label="Comentário" rows={3} />
          <button type="submit" className="botao-largo">
            comentar <SetaContinuar className="seta-vermelha" />
          </button>
        </form>
      )}
      <ul className="comentarios">
        {ideia.comentarios.map((c) => (
          <li key={c.id}>
            <span className="avatar">{c.autor[0]}</span>
            <div>
              <div className="comentario-balao">
                <div className="comentario-topo">
                  <div>
                    <strong>{c.autor}</strong>
                    <div className="empresa">{c.empresa}</div>
                  </div>
                  <button
                    className={`gosto-pequeno${c.gostei ? ' ativo' : ''}`}
                    onClick={() => alternarGostoComentario(ideia.num, c.id)}
                    aria-label="Gosto"
                  >
                    <Coracao />
                    {c.gostos > 0 && <span>{c.gostos}</span>}
                  </button>
                </div>
                <p>{c.texto}</p>
              </div>
              <div className="comentario-rodape">
                <strong>gosto</strong> {haQuantoTempo(c.data)}
              </div>
            </div>
          </li>
        ))}
        {ideia.comentarios.length === 0 && <li className="vazio">Ainda não há comentários.</li>}
      </ul>
    </article>
  );
}
