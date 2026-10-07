import { useRef, useState } from 'react';
import { ideias as apiIdeias } from '../api/servicos';
import { nomeClasse, nomeEstado, nomeTipo } from '../api/tipos';
import type { EstadoIdeia, IdeiaDetalhe as Ideia } from '../api/tipos';
import { useAtualizar, usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Gaveta } from '../componentes/Gaveta';
import { Balao, Camara, Coracao, SetaContinuar } from '../componentes/Icones';
import { formatarData, formatarTamanho, haQuantoTempo, variaveis } from '../dados/regras';
import { useUtilizador } from '../sessao/Sessao';

const tamanhoMaximo = 25 * 1024 * 1024;

const passos: { estado: EstadoIdeia; nome: string }[] = [
  { estado: 'ValidacaoAutores', nome: '01. validação dos autores' },
  { estado: 'ValidacaoEquipa', nome: '02. validação equipa inovação' },
  { estado: 'EmDiscussao', nome: '03. discussão pública' },
  { estado: 'EmAvaliacao', nome: '04. avaliação' },
];

function passoAtual(estado: EstadoIdeia): number {
  const i = passos.findIndex((p) => p.estado === estado);
  return i === -1 ? passos.length : i;
}

export function IdeiaGaveta({ id, aoFechar }: { id: number | null; aoFechar: () => void }) {
  return (
    <Gaveta aberta={id !== null} aoFechar={aoFechar}>
      {id !== null && <IdeiaCarregada id={id} />}
    </Gaveta>
  );
}

function IdeiaCarregada({ id }: { id: number }) {
  const { dados, erro, aCarregar, recarregar } = usePedido(() => apiIdeias.obter(id), [id]);
  if (!dados) return <EstadoPedido aCarregar={aCarregar} erro={erro} />;
  return <IdeiaDetalhe ideia={dados} recarregar={recarregar} />;
}

function IdeiaDetalhe({ ideia, recarregar }: { ideia: Ideia; recarregar: () => void }) {
  const eu = useUtilizador();
  const atualizarListas = useAtualizar();
  const [texto, setTexto] = useState('');
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);
  const ficheiros = useRef<HTMLInputElement>(null);

  const atual = passoAtual(ideia.estado);
  const podeParticipar = ideia.estado === 'EmDiscussao';
  const pendentes = ideia.coautores.filter((c) => !c.confirmado);

  /** Corre uma ação na API e volta a carregar a ideia e as listas. */
  const acao = async (f: () => Promise<unknown>) => {
    setErro('');
    setAEnviar(true);
    try {
      await f();
      recarregar();
      atualizarListas();
    } catch (e) {
      setErro(e instanceof Error ? e.message : String(e));
    } finally {
      setAEnviar(false);
    }
  };

  const anexar = (lista: FileList | null) => {
    if (!lista || lista.length === 0) return;
    const escolhidos = Array.from(lista);
    const grande = escolhidos.find((f) => f.size > tamanhoMaximo);
    if (grande) {
      setErro(`${grande.name} ultrapassa o limite de 25MB.`);
      return;
    }
    void acao(async () => {
      for (const f of escolhidos) {
        await apiIdeias.anexar(ideia.id, f);
      }
    });
    if (ficheiros.current) ficheiros.current.value = '';
  };

  const detalhes: [string, string | null][] = [
    ['vantagens', ideia.vantagens],
    ['requisitos', ideia.requisitos],
    ['como é feito actualmente', ideia.comoEFeitoAtualmente],
    ['modelo de negócio', ideia.modeloNegocio],
    ['competidores', ideia.competidores],
    ['custos', ideia.custos],
  ];

  const nomes = ideia.autores.split(', ');

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

      {ideia.podeConfirmarAutoria && (
        <div className="aviso">
          Foi indicado como autor desta ideia. Confirme a autoria para a ideia seguir para a equipa de Inovação.
          <button className="botao-pequeno" disabled={aEnviar} onClick={() => void acao(() => apiIdeias.confirmarAutoria(ideia.id))}>
            confirmar autoria
          </button>
        </div>
      )}

      {eu.eAdministrador && ideia.estado === 'ValidacaoEquipa' && (
        <div className="aviso">
          A ideia está à espera da validação da equipa de Inovação. Ao validar, abre a discussão pública durante 30 dias.
          <button className="botao-pequeno" disabled={aEnviar} onClick={() => void acao(() => apiIdeias.validar(ideia.id))}>
            validar e publicar
          </button>
        </div>
      )}

      {eu.eAdministrador && ideia.estado === 'EmDiscussao' && (
        <div className="aviso">
          Discussão aberta{ideia.fimDiscussao ? ` até ${formatarData(ideia.fimDiscussao)}` : ''}. Fecha sozinha ao fim de 30 dias; pode
          fechá-la já para a ideia seguir para avaliação.
          <button className="botao-pequeno" disabled={aEnviar} onClick={() => void acao(() => apiIdeias.fecharDiscussao(ideia.id))}>
            fechar discussão
          </button>
        </div>
      )}

      {erro && <div className="erro">{erro}</div>}

      <div className="ficha">
        <dl>
          <div className="ficha-autor">
            <span className="avatar">{ideia.anonima ? '?' : (nomes[0] ?? '?')[0]}</span>
          </div>
          <dt>{nomes.length > 1 ? 'autores' : 'autor'}</dt>
          <dd>
            {ideia.autores}
            {pendentes.length > 0 && <div className="cinzento-escuro">por confirmar: {pendentes.map((c) => c.nome).join(', ')}</div>}
          </dd>
          <dt>código</dt>
          <dd>{ideia.codigo}</dd>
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
        <a key={a.id} className="anexo" href={apiIdeias.urlAnexo(a.id)} download={a.nomeFicheiro}>
          {a.nomeFicheiro} <span className="cinzento-escuro">({formatarTamanho(a.tamanhoBytes)})</span>
        </a>
      ))}
      {ideia.podeAnexar && (
        <label className="anexar-pequeno">
          <input ref={ficheiros} type="file" multiple onChange={(e) => anexar(e.target.files)} disabled={aEnviar} />
          <Camara /> anexar ficheiros <span className="cinzento-escuro">(até 25MB cada)</span>
        </label>
      )}

      {(ideia.avaliacao || ideia.classe) && (
        <div className="resultado">
          <h3 className="secao">avaliação</h3>
          <div className="notas">
            {ideia.avaliacao && ideia.responsabilidade === 'Dstelecom' && (
              <>
                {variaveis.map((v) => (
                  <span key={v.chave}>
                    {v.nome}: <strong>{ideia.avaliacao?.[v.chave] ?? 'N.A.'}</strong>
                  </span>
                ))}
                <span>
                  Total: <strong>{ideia.avaliacao.nota === null ? 'N.A.' : ideia.avaliacao.nota.toFixed(2)}</strong>
                </span>
              </>
            )}
            {ideia.responsabilidade === 'Dst' && (
              <span>
                Responsabilidade: <strong>dst</strong>
              </span>
            )}
            {ideia.classe && (
              <span>
                Classe: <strong>{nomeClasse[ideia.classe]}</strong>
              </span>
            )}
            {ideia.status && (
              <span>
                Status: <strong>{ideia.status}</strong>
              </span>
            )}
          </div>
        </div>
      )}

      <button
        className={`botao-gosto${ideia.gostei ? ' ativo' : ''}`}
        onClick={() => void acao(() => apiIdeias.alternarGosto(ideia.id))}
        disabled={!podeParticipar || aEnviar}
      >
        <Coracao /> gosto
      </button>

      <h3 className="secao grande">comentários</h3>
      {podeParticipar && (
        <form
          className="comentar"
          onSubmit={(e) => {
            e.preventDefault();
            const t = texto.trim();
            if (t) {
              void acao(async () => {
                await apiIdeias.comentar(ideia.id, t);
                setTexto('');
              });
            }
          }}
        >
          <textarea value={texto} onChange={(e) => setTexto(e.target.value)} aria-label="Comentário" rows={3} maxLength={2000} />
          <button type="submit" className="botao-largo" disabled={aEnviar || !texto.trim()}>
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
                    onClick={() => void acao(() => apiIdeias.alternarGostoComentario(c.id))}
                    disabled={!podeParticipar || aEnviar}
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
