import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { iniciativas as api, registos as apiRegistos } from '../api/servicos';
import { nomeTipoParceiro } from '../api/tipos';
import type { ExecucaoVigilancia, IniciativaDetalhe as Detalhe } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Fechar } from '../componentes/Icones';
import { hoje } from '../componentes/TabelaEditavel';
import { formatarData } from '../dados/regras';

const mensagem = (falha: unknown) => (falha instanceof Error ? falha.message : String(falha));

/** Parceiros (Mod.235) que trabalham nesta iniciativa. */
export function ParceirosIniciativa({ detalhe, recarregar }: { detalhe: Detalhe; recarregar: () => void }) {
  const podeEditar = detalhe.podeEditar;
  const todos = usePedido(() => (podeEditar ? apiRegistos.parceiros() : Promise.resolve([])), [podeEditar]);
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);
  const atuais = detalhe.parceiros;

  if (!podeEditar && atuais.length === 0) return null;

  const definir = async (ids: number[]) => {
    setErro('');
    setAEnviar(true);
    try {
      await api.definirParceiros(detalhe.resumo.id, ids);
      recarregar();
    } catch (falha) {
      setErro(mensagem(falha));
    } finally {
      setAEnviar(false);
    }
  };

  const disponiveis = (todos.dados ?? []).filter((p) => !atuais.some((a) => a.id === p.id));

  return (
    <>
      <h3 className="secao">parceiros</h3>
      {atuais.length > 0 ? (
        <div className="fichas fichas-parceiros">
          {atuais.map((p) => (
            <span key={p.id} className="ficha-autor-escolhido" title={nomeTipoParceiro[p.tipo]}>
              {p.nome}
              {podeEditar && (
                <button
                  type="button"
                  aria-label={`Tirar ${p.nome}`}
                  disabled={aEnviar}
                  onClick={() => void definir(atuais.filter((x) => x.id !== p.id).map((x) => x.id))}
                >
                  <Fechar />
                </button>
              )}
            </span>
          ))}
        </div>
      ) : (
        <p className="cinzento-escuro">Ainda sem parceiros.</p>
      )}
      {podeEditar && (
        <div className="link-novo parceiro-novo">
          <select
            aria-label="Acrescentar parceiro"
            value=""
            disabled={aEnviar || disponiveis.length === 0}
            onChange={(e) => e.target.value && void definir([...atuais.map((x) => x.id), Number(e.target.value)])}
          >
            <option value="">
              {todos.dados && todos.dados.length === 0 ? 'registe os parceiros em parcerias' : 'acrescentar parceiro…'}
            </option>
            {disponiveis.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nome} ({nomeTipoParceiro[p.tipo]})
              </option>
            ))}
          </select>
          <Link className="ligacao" to="/inovacao/parcerias">
            parcerias
          </Link>
        </div>
      )}
      {erro && <div className="erro">{erro}</div>}
    </>
  );
}

/** Execuções de uma vigilância: previstas, feitas e o resultado (que entra no conhecimento codificado). */
export function ExecucoesVigilancia({ detalhe }: { detalhe: Detalhe }) {
  const id = detalhe.resumo.id;
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.execucoes(id), [id]);
  const [aberta, setAberta] = useState<number | 'nova' | null>(null);

  return (
    <>
      <h3 className="secao">execuções</h3>
      <p className="cinzento-escuro charter-nota">
        Data prevista e realizada (KPI 24). Ao registar o resultado, entra no{' '}
        <Link className="ligacao" to="/inovacao/conhecimento">
          conhecimento codificado (Mod.237)
        </Link>{' '}
        e fica planeada a execução seguinte.
      </p>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && (
        <ul className="execucoes">
          {dados.length === 0 && <li className="cinzento-escuro">Ainda não há execuções planeadas.</li>}
          {dados.map((e) =>
            aberta === e.id ? (
              <li key={e.id}>
                <FormExecucao vigilanciaId={id} execucao={e} aoTerminar={() => {
                    setAberta(null);
                    recarregar();
                  }} />
              </li>
            ) : (
              <li key={e.id}>
                <div className="execucao-topo">
                  <span>
                    <span className="num">{formatarData(e.dataPrevista)}</span>
                    {e.dataRealizada ? (
                      <span className={`etiqueta ${e.noPrazo ? 'estado-aprovada' : 'estado-em-avaliacao'}`}>
                        feita a {formatarData(e.dataRealizada)}
                        {e.noPrazo ? '' : ' (fora do prazo)'}
                      </span>
                    ) : e.atrasada ? (
                      <span className="etiqueta estado-em-discussao">atrasada</span>
                    ) : (
                      <span className="etiqueta estado-validacao-equipa">prevista</span>
                    )}
                  </span>
                  {detalhe.podeEditar && (
                    <button className="ligacao-simples" onClick={() => setAberta(e.id)}>
                      {e.dataRealizada ? 'editar' : 'registar resultado'}
                    </button>
                  )}
                </div>
                {e.resultado && <p className="texto">{e.resultado}</p>}
                {(e.linkResultado || e.conhecimentoId) && (
                  <div className="execucao-links">
                    {e.linkResultado && (
                      <a className="ligacao" href={e.linkResultado} target="_blank" rel="noreferrer">
                        relatório ↗
                      </a>
                    )}
                    {e.conhecimentoId && <span className="cinzento-escuro">no conhecimento codificado ✓</span>}
                  </div>
                )}
              </li>
            ),
          )}
          {aberta === 'nova' && (
            <li>
              <FormExecucao vigilanciaId={id} execucao={null} aoTerminar={() => {
                    setAberta(null);
                    recarregar();
                  }} />
            </li>
          )}
        </ul>
      )}
      {detalhe.podeEditar && aberta === null && (
        <button className="botao-pequeno claro" onClick={() => setAberta('nova')}>
          planear execução
        </button>
      )}
    </>
  );
}

function FormExecucao({
  vigilanciaId,
  execucao,
  aoTerminar,
}: {
  vigilanciaId: number;
  execucao: ExecucaoVigilancia | null;
  aoTerminar: () => void;
}) {
  const [prevista, setPrevista] = useState(execucao?.dataPrevista ?? hoje());
  const [comResultado, setComResultado] = useState(execucao !== null);
  const [realizada, setRealizada] = useState(execucao?.dataRealizada ?? hoje());
  const [resultado, setResultado] = useState(execucao?.resultado ?? '');
  const [link, setLink] = useState(execucao?.linkResultado ?? '');
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);

  const executar = async (acao: () => Promise<unknown>) => {
    setErro('');
    setAEnviar(true);
    try {
      await acao();
      aoTerminar();
    } catch (falha) {
      setErro(mensagem(falha));
      setAEnviar(false);
    }
  };

  const submeter = (e: FormEvent) => {
    e.preventDefault();
    const dados = {
      dataPrevista: prevista,
      dataRealizada: comResultado && resultado.trim() ? realizada : null,
      resultado: comResultado && resultado.trim() ? resultado.trim() : null,
      linkResultado: link.trim() || null,
    };
    void executar(() => api.guardarExecucao(vigilanciaId, execucao?.id ?? null, dados));
  };

  return (
    <form className="form-execucao" onSubmit={submeter}>
      <div className="campos-lado">
        <label className="campo">
          <span>data prevista</span>
          <input type="date" value={prevista} required onChange={(e) => setPrevista(e.target.value)} />
        </label>
        {comResultado && (
          <label className="campo">
            <span>feita a</span>
            <input type="date" value={realizada} required onChange={(e) => setRealizada(e.target.value)} />
          </label>
        )}
      </div>
      {comResultado ? (
        <>
          <label className="campo">
            <span>resultado *</span>
            <textarea rows={3} maxLength={4000} value={resultado} required onChange={(e) => setResultado(e.target.value)} />
          </label>
          <label className="campo">
            <span>link do relatório (OneDrive)</span>
            <input type="url" value={link} maxLength={2000} placeholder="https://…" onChange={(e) => setLink(e.target.value)} />
          </label>
        </>
      ) : (
        <button type="button" className="ligacao-simples" onClick={() => setComResultado(true)}>
          já foi feita: registar o resultado
        </button>
      )}
      {erro && <div className="erro">{erro}</div>}
      <div className="botoes-linha">
        {execucao && !execucao.dataRealizada && (
          <button
            type="button"
            className="ligacao-simples cinzento-escuro"
            disabled={aEnviar}
            onClick={() => void executar(() => api.apagarExecucao(vigilanciaId, execucao.id))}
          >
            apagar
          </button>
        )}
        <button type="button" className="ligacao-simples" onClick={aoTerminar}>
          cancelar
        </button>
        <button type="submit" className="botao-pequeno" disabled={aEnviar}>
          {aEnviar ? 'a guardar…' : 'guardar'}
        </button>
      </div>
    </form>
  );
}
