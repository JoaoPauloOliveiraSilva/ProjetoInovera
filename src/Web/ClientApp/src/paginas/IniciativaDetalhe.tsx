import { Fragment, useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { iniciativas as api } from '../api/servicos';
import { classeEstadoIniciativa, nomeEstadoIniciativa, nomeTipoIniciativa } from '../api/tipos';
import type { DadosIniciativa, EstadoIniciativa, Horizonte, IniciativaDetalhe as Detalhe, TipoIniciativa } from '../api/tipos';
import { useAtualizar, usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Gaveta } from '../componentes/Gaveta';
import { SetaContinuar } from '../componentes/Icones';
import { formatarData, haQuantoTempo } from '../dados/regras';

const estados = Object.keys(nomeEstadoIniciativa) as EstadoIniciativa[];

/** Ficha da iniciativa (gaveta à direita). <c>id = 'nova'</c> abre o formulário de criação. */
export function IniciativaGaveta({
  id,
  tipoNova,
  aoFechar,
}: {
  id: number | 'nova' | null;
  tipoNova?: TipoIniciativa;
  aoFechar: () => void;
}) {
  return (
    <Gaveta aberta={id !== null} aoFechar={aoFechar}>
      {id === 'nova' && <FormIniciativa tipoInicial={tipoNova ?? 'Projeto'} aoGuardar={aoFechar} />}
      {typeof id === 'number' && <IniciativaCarregada key={id} id={id} />}
    </Gaveta>
  );
}

function IniciativaCarregada({ id }: { id: number }) {
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.obter(id), [id]);
  const [aEditar, setAEditar] = useState(false);
  if (!dados) return <EstadoPedido aCarregar={aCarregar} erro={erro} />;
  if (aEditar) {
    return (
      <FormIniciativa
        existente={dados}
        aoGuardar={() => {
          setAEditar(false);
          recarregar();
        }}
      />
    );
  }
  return <Ficha detalhe={dados} aoEditar={() => setAEditar(true)} recarregar={recarregar} />;
}

function Ficha({ detalhe, aoEditar, recarregar }: { detalhe: Detalhe; aoEditar: () => void; recarregar: () => void }) {
  const r = detalhe.resumo;
  const atualizarListas = useAtualizar();
  const [texto, setTexto] = useState('');
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  const registar = async (e: FormEvent) => {
    e.preventDefault();
    if (!texto.trim()) return;
    setErro('');
    setAEnviar(true);
    try {
      await api.pontoSituacao(r.id, texto.trim());
      setTexto('');
      recarregar();
      atualizarListas();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <article className="detalhe">
      <div className="sobretitulo">INOVAÇÃO · {nomeTipoIniciativa[r.tipo].toUpperCase()}</div>
      <h2 className="detalhe-titulo">{r.titulo}</h2>
      <div className="detalhe-tipo">
        <span className={`etiqueta ${classeEstadoIniciativa[r.estado]}`}>{nomeEstadoIniciativa[r.estado]}</span>
      </div>

      {detalhe.temCharter && (
        <div className="ficha-acoes">
          <Link className="botao-pequeno" to={`/inovacao/iniciativas/${r.id}/charter`}>
            abrir project charter →
          </Link>
        </div>
      )}

      <div className="ficha ficha-simples">
        <dl>
          <dt>código</dt>
          <dd>{r.codigo}</dd>
          <dt>responsável</dt>
          <dd>{r.responsavel ?? '—'}</dd>
          <dt>início</dt>
          <dd>{r.dataInicio ? formatarData(r.dataInicio) : '—'}</dd>
          <dt>fim</dt>
          <dd>{r.dataFim ? formatarData(r.dataFim) : '—'}</dd>
          <dt>origem</dt>
          <dd>{r.origem ?? '—'}</dd>
          {detalhe.horizonte && (
            <>
              <dt>horizonte</dt>
              <dd>{detalhe.horizonte}</dd>
            </>
          )}
          {detalhe.detalhes.map((d) => (
            <Fragment key={d.nome}>
              <dt>{d.nome.toLowerCase()}</dt>
              <dd>{d.valor}</dd>
            </Fragment>
          ))}
          {r.ideiaOrigemId && (
            <>
              <dt>ideia de origem</dt>
              <dd>
                <Link to={`/inovacao/ideias/${r.ideiaOrigemId}`} className="ligacao">
                  {r.ideiaOrigemCodigo}
                </Link>
              </dd>
            </>
          )}
        </dl>
      </div>

      {detalhe.descricao && (
        <>
          <h3 className="secao">descrição</h3>
          <p className="texto">{detalhe.descricao}</p>
        </>
      )}

      <LinksDocumentos detalhe={detalhe} recarregar={recarregar} />

      <h3 className="secao">ponto de situação</h3>
      <p className="texto">{r.pontoSituacao ?? 'Ainda sem ponto de situação.'}</p>

      {detalhe.podeEditar && (
        <>
          <form className="comentar" onSubmit={(e) => void registar(e)}>
            <textarea
              value={texto}
              onChange={(e) => setTexto(e.target.value)}
              rows={3}
              maxLength={2000}
              aria-label="Novo ponto de situação"
              placeholder="novo ponto de situação"
            />
            <button type="submit" className="botao-largo" disabled={aEnviar || !texto.trim()}>
              registar ponto de situação <SetaContinuar className="seta-vermelha" />
            </button>
          </form>
          {erro && <div className="erro">{erro}</div>}
          <button className="botao-pequeno claro" onClick={aoEditar}>
            editar dados
          </button>
        </>
      )}

      {detalhe.historico.length > 0 && (
        <>
          <h3 className="secao">histórico</h3>
          <ul className="historico">
            {detalhe.historico.map((h, i) => (
              <li key={i}>
                <span className="cinzento-escuro">
                  {formatarData(h.data)} · {h.autor ?? '—'} · {haQuantoTempo(h.data)}
                </span>
                <p>{h.texto}</p>
              </li>
            ))}
          </ul>
        </>
      )}
    </article>
  );
}

function FormIniciativa({
  existente,
  tipoInicial,
  aoGuardar,
}: {
  existente?: Detalhe;
  tipoInicial?: TipoIniciativa;
  aoGuardar: () => void;
}) {
  const atualizarListas = useAtualizar();
  const r = existente?.resumo;
  const [tipo, setTipo] = useState<TipoIniciativa>(r?.tipo ?? tipoInicial ?? 'Projeto');
  const [dados, setDados] = useState<DadosIniciativa>({
    titulo: r?.titulo ?? '',
    descricao: existente?.descricao ?? null,
    estado: r?.estado ?? 'Pendente',
    responsavel: r?.responsavel ?? null,
    dataInicio: r?.dataInicio ?? null,
    dataFim: r?.dataFim ?? null,
    origem: r?.origem ?? null,
    horizonte: existente?.horizonte ?? null,
  });
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  const mudar = (alteracao: Partial<DadosIniciativa>) => setDados((d) => ({ ...d, ...alteracao }));
  const vazioANull = (v: string) => (v.trim() ? v : null);

  const submeter = async (e: FormEvent) => {
    e.preventDefault();
    setErro('');
    setAEnviar(true);
    try {
      if (r) {
        await api.atualizar(r.id, dados);
      } else {
        await api.criar(tipo, dados);
      }
      atualizarListas();
      aoGuardar();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <form className="form-gaveta" onSubmit={(e) => void submeter(e)}>
      <div className="sobretitulo">INOVAÇÃO</div>
      <h2 className="detalhe-titulo">{r ? `editar ${r.codigo}` : 'nova iniciativa'}</h2>
      {!r && (
        <label className="campo">
          <span>tipo</span>
          <select value={tipo} onChange={(e) => setTipo(e.target.value as TipoIniciativa)}>
            {(Object.keys(nomeTipoIniciativa) as TipoIniciativa[]).map((t) => (
              <option key={t} value={t}>
                {nomeTipoIniciativa[t]}
              </option>
            ))}
          </select>
        </label>
      )}
      <label className="campo">
        <span>título</span>
        <input value={dados.titulo} onChange={(e) => mudar({ titulo: e.target.value })} required maxLength={300} />
      </label>
      <label className="campo">
        <span>descrição</span>
        <textarea rows={3} value={dados.descricao ?? ''} onChange={(e) => mudar({ descricao: vazioANull(e.target.value) })} />
      </label>
      <div className="campos-lado">
        <label className="campo">
          <span>estado</span>
          <select value={dados.estado} onChange={(e) => mudar({ estado: e.target.value as EstadoIniciativa })}>
            {estados.map((s) => (
              <option key={s} value={s}>
                {nomeEstadoIniciativa[s]}
              </option>
            ))}
          </select>
        </label>
        <label className="campo">
          <span>horizonte</span>
          <select value={dados.horizonte ?? ''} onChange={(e) => mudar({ horizonte: (e.target.value || null) as Horizonte | null })}>
            <option value="">—</option>
            <option value="H1">H1</option>
            <option value="H2">H2</option>
            <option value="H3">H3</option>
          </select>
        </label>
      </div>
      <label className="campo">
        <span>responsável</span>
        <input value={dados.responsavel ?? ''} onChange={(e) => mudar({ responsavel: vazioANull(e.target.value) })} maxLength={200} />
      </label>
      <div className="campos-lado">
        <label className="campo">
          <span>data de início</span>
          <input type="date" value={dados.dataInicio ?? ''} onChange={(e) => mudar({ dataInicio: e.target.value || null })} />
        </label>
        <label className="campo">
          <span>data de fim</span>
          <input type="date" value={dados.dataFim ?? ''} onChange={(e) => mudar({ dataFim: e.target.value || null })} />
        </label>
      </div>
      <label className="campo">
        <span>origem / financiamento</span>
        <input value={dados.origem ?? ''} onChange={(e) => mudar({ origem: vazioANull(e.target.value) })} maxLength={200} />
      </label>
      {erro && <div className="erro">{erro}</div>}
      <div className="botoes">
        {r && (
          <button type="button" className="botao-largo curto" onClick={aoGuardar}>
            cancelar
          </button>
        )}
        <button type="submit" className="botao-largo" disabled={aEnviar || !dados.titulo.trim()}>
          {aEnviar ? 'a guardar…' : 'guardar'} <SetaContinuar className="seta-vermelha" />
        </button>
      </div>
    </form>
  );
}

/** Documentos no OneDrive (RF15): a plataforma guarda só o link. */
function LinksDocumentos({ detalhe, recarregar }: { detalhe: Detalhe; recarregar: () => void }) {
  const [titulo, setTitulo] = useState('');
  const [url, setUrl] = useState('');
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);
  const id = detalhe.resumo.id;

  if (!detalhe.podeEditar && detalhe.links.length === 0) return null;

  const executar = async (acao: () => Promise<unknown>) => {
    setErro('');
    setAEnviar(true);
    try {
      await acao();
      recarregar();
      return true;
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
      return false;
    } finally {
      setAEnviar(false);
    }
  };

  const adicionar = async (e: FormEvent) => {
    e.preventDefault();
    if (await executar(() => api.adicionarLink(id, titulo.trim(), url.trim()))) {
      setTitulo('');
      setUrl('');
    }
  };

  return (
    <>
      <h3 className="secao">documentos (OneDrive)</h3>
      {detalhe.links.length > 0 ? (
        <ul className="links-documentos">
          {detalhe.links.map((l) => (
            <li key={l.id}>
              <a className="ligacao" href={l.url} target="_blank" rel="noreferrer">
                {l.titulo} ↗
              </a>
              {detalhe.podeEditar && (
                <button className="ligacao-simples cinzento-escuro" disabled={aEnviar} onClick={() => void executar(() => api.removerLink(id, l.id))}>
                  remover
                </button>
              )}
            </li>
          ))}
        </ul>
      ) : (
        <p className="cinzento-escuro">Ainda não há documentos ligados.</p>
      )}
      {detalhe.podeEditar && (
        <form className="link-novo" onSubmit={(e) => void adicionar(e)}>
          <input value={titulo} onChange={(e) => setTitulo(e.target.value)} placeholder="título (ex.: candidatura PRR)" maxLength={300} aria-label="Título do documento" />
          <input type="url" value={url} onChange={(e) => setUrl(e.target.value)} placeholder="https://…sharepoint.com/…" maxLength={2000} aria-label="Link do documento" />
          <button type="submit" className="botao-pequeno" disabled={aEnviar || !titulo.trim() || !url.trim()}>
            adicionar
          </button>
        </form>
      )}
      {erro && <div className="erro">{erro}</div>}
    </>
  );
}
