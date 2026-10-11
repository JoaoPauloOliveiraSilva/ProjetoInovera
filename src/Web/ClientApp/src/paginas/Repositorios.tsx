import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { conhecimento as api } from '../api/servicos';
import { nomeCategoriaLicao, nomeOrigemConhecimento } from '../api/tipos';
import type { CategoriaLicao, OrigemConhecimento } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { formatarData } from '../dados/regras';

/** Texto de procura com espera (só pede à API depois de parar de escrever). */
function useProcura(): [string, (v: string) => void, string] {
  const [texto, setTexto] = useState('');
  const [procura, setProcura] = useState('');
  useEffect(() => {
    const t = setTimeout(() => setProcura(texto), 300);
    return () => clearTimeout(t);
  }, [texto]);
  return [texto, setTexto, procura];
}

/** Repositório de lições aprendidas (Mod.252): junta as lições de todos os Project Charters. */
export function RepositorioLicoes() {
  const [categoria, setCategoria] = useState<CategoriaLicao | ''>('');
  const [texto, setTexto, procura] = useProcura();
  const { dados, erro, aCarregar } = usePedido(() => api.licoes({ categoria: categoria || undefined, texto: procura }), [categoria, procura]);
  const lista = dados ?? [];

  return (
    <>
      <Cabecalho titulo="lições aprendidas" corLampada="#3a6f9a" />
      <div className="filtros filtros-varios">
        <label>
          categoria
          <select value={categoria} onChange={(e) => setCategoria(e.target.value as CategoriaLicao | '')}>
            <option value="">todas</option>
            {(Object.keys(nomeCategoriaLicao) as CategoriaLicao[]).map((c) => (
              <option key={c} value={c}>
                {nomeCategoriaLicao[c]}
              </option>
            ))}
          </select>
        </label>
        <label>
          procurar
          <input className="filtro-texto" value={texto} onChange={(e) => setTexto(e.target.value)} placeholder="lição, projeto ou código" />
        </label>
        {dados && <span className="cinzento-escuro">{lista.length} lições · Mod.252</span>}
      </div>
      <section className="tabela">
        <div className="tabela-linha tabela-cabecalho repositorio-linha">
          <span>Data</span>
          <span>Iniciativa</span>
          <span>Categoria</span>
          <span>Lição</span>
          <span>Impacto</span>
        </div>
        {lista.map((l) => (
          <div key={l.id} className="tabela-linha repositorio-linha so-leitura">
            <span>{formatarData(l.data)}</span>
            <span>
              <Link className="ligacao" to={`/inovacao/iniciativas/${l.iniciativaId}/charter?separador=licoes`}>
                {l.iniciativaCodigo}
              </Link>
              <span className="caixa-id">{l.iniciativaTitulo}</span>
            </span>
            <span>{nomeCategoriaLicao[l.categoria]}</span>
            <span>
              <span className="forte">{l.licao}</span>
              {l.descricao && <span className="caixa-id">{l.descricao}</span>}
            </span>
            <span>{l.impacto ?? '—'}</span>
          </div>
        ))}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && lista.length === 0 && (
          <div className="vazio">Ainda não há lições. São registadas no separador "lições aprendidas" do Project Charter de cada projeto.</div>
        )}
      </section>
    </>
  );
}

/** Repositório de conhecimento codificado (Mod.237): relatórios de entregáveis/milestones e resultados de vigilâncias. */
export function RepositorioConhecimento() {
  const [origem, setOrigem] = useState<OrigemConhecimento | ''>('');
  const [texto, setTexto, procura] = useProcura();
  const { dados, erro, aCarregar } = usePedido(() => api.codificado({ origem: origem || undefined, texto: procura }), [origem, procura]);
  const lista = dados ?? [];

  return (
    <>
      <Cabecalho titulo="conhecimento codificado" corLampada="#3a6f9a" />
      <div className="filtros filtros-varios">
        <label>
          origem
          <select value={origem} onChange={(e) => setOrigem(e.target.value as OrigemConhecimento | '')}>
            <option value="">todas</option>
            {(Object.keys(nomeOrigemConhecimento) as OrigemConhecimento[]).map((o) => (
              <option key={o} value={o}>
                {nomeOrigemConhecimento[o]}
              </option>
            ))}
          </select>
        </label>
        <label>
          procurar
          <input className="filtro-texto" value={texto} onChange={(e) => setTexto(e.target.value)} placeholder="documento ou código" />
        </label>
        {dados && <span className="cinzento-escuro">{lista.length} documentos · Mod.237</span>}
      </div>
      <section className="tabela">
        <div className="tabela-linha tabela-cabecalho conhecimento-linha">
          <span>Data</span>
          <span>Documento</span>
          <span>Iniciativa</span>
          <span>Origem</span>
          <span>Consulta</span>
        </div>
        {lista.map((c) => (
          <div key={c.id} className="tabela-linha conhecimento-linha so-leitura">
            <span>{formatarData(c.data)}</span>
            <span>
              <span className="forte">{c.documento}</span>
              {c.descricao && <span className="caixa-id">{c.descricao}</span>}
            </span>
            <span>
              {c.iniciativaId ? (
                <Link className="ligacao" to={`/inovacao/iniciativas/${c.iniciativaId}`}>
                  {c.iniciativaCodigo}
                </Link>
              ) : (
                '—'
              )}
              {c.entregaCodigo && <span className="caixa-id">{c.entregaCodigo}</span>}
            </span>
            <span>{nomeOrigemConhecimento[c.origem]}</span>
            <span>
              {c.url ? (
                <a className="ligacao" href={c.url} target="_blank" rel="noreferrer">
                  abrir ↗
                </a>
              ) : (
                (c.modoConsulta ?? '—')
              )}
            </span>
          </div>
        ))}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && lista.length === 0 && (
          <div className="vazio">
            Ainda não há documentos. Entram aqui sozinhos quando se cola o link do relatório de um entregável ou milestone no Project Charter.
          </div>
        )}
      </section>
    </>
  );
}
