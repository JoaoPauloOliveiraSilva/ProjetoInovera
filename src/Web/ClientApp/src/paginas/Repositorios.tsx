import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Permissoes } from '../api/permissoes';
import { conhecimento as api, iniciativas as apiIniciativas, urlExportacao } from '../api/servicos';
import { nomeCategoriaLicao, nomeOrigemConhecimento } from '../api/tipos';
import type { CategoriaLicao, ConhecimentoCodificado, IniciativaResumo, OrigemConhecimento } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { LinkExterno, TabelaEditavel, hoje, lista as valorLista, proximaAtividade, subcamposAtividades, texto } from '../componentes/TabelaEditavel';
import type { DefTabela } from '../componentes/TabelaEditavel';
import { formatarData } from '../dados/regras';
import { usePermissao } from '../sessao/Sessao';

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
      <Cabecalho titulo="lições aprendidas" corLampada="#3a6f9a" exportar={urlExportacao('mod252')} />
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

/** Repositório de conhecimento codificado (Mod.237): relatórios de entregáveis/milestones, resultados de vigilâncias e entradas manuais. */
export function RepositorioConhecimento() {
  const tem = usePermissao();
  const podeEditar = tem(Permissoes.gerirRegistos);
  const [origem, setOrigem] = useState<OrigemConhecimento | ''>('');
  const [texto, setTexto, procura] = useProcura();
  const { dados, erro, aCarregar, recarregar } = usePedido(
    () => api.codificado({ origem: origem || undefined, texto: procura }),
    [origem, procura],
  );
  const iniciativas = usePedido(() => (podeEditar ? apiIniciativas.listar() : Promise.resolve([])), [podeEditar]);
  const lista = dados ?? [];

  return (
    <>
      <Cabecalho titulo="conhecimento codificado" corLampada="#3a6f9a" exportar={urlExportacao('mod237')} />
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
      <p className="cinzento-escuro charter-nota pagina-nota">
        Os relatórios de entregáveis/milestones e os resultados das vigilâncias entram sozinhos. Outros documentos (estudos, relatórios SIFIDE…)
        registam-se à mão. Em todos se planeiam as atividades de partilha.
      </p>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && (
        <TabelaEditavel
          def={defConhecimento(iniciativas.dados ?? [])}
          itens={lista}
          podeEditar={podeEditar}
          guardar={(id, corpo) => api.guardarCodificado(id, corpo)}
          apagar={(id) => api.apagarCodificado(id)}
          aoMudar={recarregar}
          sobretitulo="CONHECIMENTO CODIFICADO · MOD.237"
          semTitulo
        />
      )}
    </>
  );
}

function defConhecimento(iniciativas: IniciativaResumo[]): DefTabela<ConhecimentoCodificado> {
  return {
    titulo: 'conhecimento codificado',
    singular: 'documento',
    colunas: '100px minmax(240px, 1.4fr) 110px 120px minmax(160px, 1fr) 90px',
    cabecalhos: ['Data', 'Documento', 'Iniciativa', 'Origem', 'Partilha', 'Consulta'],
    celulas: (c) => [
      formatarData(c.data),
      <span>
        <span className="forte">{c.documento}</span>
        {c.descricao && <span className="caixa-id">{c.descricao}</span>}
      </span>,
      <span>
        {c.iniciativaId ? (
          <Link className="ligacao" to={`/inovacao/iniciativas/${c.iniciativaId}`} onClick={(e) => e.stopPropagation()}>
            {c.iniciativaCodigo}
          </Link>
        ) : (
          '—'
        )}
        {c.entregaCodigo && <span className="caixa-id">{c.entregaCodigo}</span>}
      </span>,
      nomeOrigemConhecimento[c.origem],
      proximaAtividade(c.atividades),
      c.url ? <LinkExterno url={c.url} /> : (c.modoConsulta ?? '—'),
    ],
    campos: [
      { nome: 'documento', rotulo: 'documento (título)', obrigatorio: true, max: 300 },
      { nome: 'descricao', rotulo: 'descrição do conhecimento', tipo: 'area', max: 4000 },
      {
        nome: 'url',
        rotulo: 'link (OneDrive)',
        tipo: 'link',
        max: 2000,
        ajuda: 'Nas entradas automáticas, o link e a iniciativa vêm do relatório ou da vigilância.',
      },
      { nome: 'modoConsulta', rotulo: 'modo de consulta (onde está arquivado)', max: 300 },
      {
        nome: 'iniciativaId',
        rotulo: 'iniciativa',
        tipo: 'opcoes',
        numerico: true,
        opcoes: iniciativas.map((i): [string, string] => [String(i.id), `${i.codigo} · ${i.titulo}`]),
      },
      { nome: 'data', rotulo: 'data', tipo: 'data' },
      { nome: 'arquivadoEm', rotulo: 'arquivado em', tipo: 'data' },
      { nome: 'resultado', rotulo: 'resultado obtido', max: 1000 },
      {
        nome: 'atividades',
        rotulo: 'partilha: atividades · quem · quando',
        tipo: 'lista',
        subcampos: subcamposAtividades,
        ajuda: 'Ex.: apresentar à CE, partilhar com os trabalhadores, enviar ao CTO. Avisa 7 dias antes.',
      },
    ],
    valores: (c) => ({
      documento: texto(c?.documento),
      descricao: texto(c?.descricao),
      url: texto(c?.url),
      modoConsulta: c ? texto(c.modoConsulta) : 'OneDrive dstelecom inov',
      iniciativaId: texto(c?.iniciativaId),
      data: c ? c.data : hoje(),
      arquivadoEm: texto(c?.arquivadoEm),
      resultado: c ? texto(c.resultado) : 'Síntese e partilha do conhecimento gerado',
      atividades: valorLista(c?.atividades),
    }),
    vazio: 'Ainda não há documentos. Entram sozinhos quando se cola o link do relatório de uma entrega no Project Charter ou se regista o resultado de uma vigilância.',
    podeApagar: (c) => c.origem === 'Manual',
  };
}
