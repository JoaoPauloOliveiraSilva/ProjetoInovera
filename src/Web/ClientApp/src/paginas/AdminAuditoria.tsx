import { useEffect, useState } from 'react';
import { auditoria as api } from '../api/servicos';
import { nomeAcaoAuditoria } from '../api/tipos';
import type { RegistoAuditoria } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';

// Entidades mais consultadas (o nome é o da classe no domínio).
const entidades: [string, string][] = [
  ['Ideia', 'ideias'],
  ['AvaliacaoIdeia', 'avaliações de ideias'],
  ['Projeto', 'projetos'],
  ['Desafio', 'desafios'],
  ['MelhoriaContinua', 'melhorias contínuas'],
  ['ProjectCharter', 'project charter (geral)'],
  ['TarefaWbs', 'tarefas'],
  ['Entrega', 'entregáveis e milestones'],
  ['Risco', 'riscos'],
  ['LinhaOrcamento', 'orçamento'],
  ['RequisitoConformidade', 'conformidade'],
  ['LicaoAprendida', 'lições aprendidas'],
  ['ConhecimentoCodificado', 'conhecimento codificado'],
  ['LinkDocumento', 'links de documentos'],
  ['Utilizador', 'utilizadores'],
];

const classeAcao = { Criar: 'estado-aprovada', Alterar: 'estado-em-avaliacao', Eliminar: 'estado-em-discussao' } as const;

type Valores = Record<string, unknown>;

function ler(json: string | null): Valores {
  if (!json) return {};
  try {
    return JSON.parse(json) as Valores;
  } catch {
    return {};
  }
}

const mostrar = (v: unknown) => (v === null || v === undefined || v === '' ? '∅' : typeof v === 'object' ? JSON.stringify(v) : String(v));

/** Registo de auditoria (ISO 56001): quem criou, alterou ou eliminou o quê e quando. */
export function AdminAuditoria() {
  const [entidade, setEntidade] = useState('');
  const [entidadeId, setEntidadeId] = useState('');
  const [texto, setTexto] = useState('');
  const [procura, setProcura] = useState({ texto: '', entidadeId: '' });
  const [aberto, setAberto] = useState<number | null>(null);

  useEffect(() => {
    const t = setTimeout(() => setProcura({ texto, entidadeId }), 300);
    return () => clearTimeout(t);
  }, [texto, entidadeId]);

  const { dados, erro, aCarregar } = usePedido(
    () => api.listar({ entidade, entidadeId: procura.entidadeId, texto: procura.texto }),
    [entidade, procura],
  );
  const lista = dados ?? [];

  return (
    <>
      <Cabecalho titulo="registo de auditoria" />
      <div className="filtros filtros-varios">
        <label>
          o quê
          <select value={entidade} onChange={(e) => setEntidade(e.target.value)}>
            <option value="">tudo</option>
            {entidades.map(([v, nome]) => (
              <option key={v} value={v}>
                {nome}
              </option>
            ))}
          </select>
        </label>
        <label>
          id
          <input className="filtro-texto curto" value={entidadeId} onChange={(e) => setEntidadeId(e.target.value)} inputMode="numeric" />
        </label>
        <label>
          procurar
          <input className="filtro-texto" value={texto} onChange={(e) => setTexto(e.target.value)} placeholder="pessoa ou valor" />
        </label>
        {dados && <span className="cinzento-escuro">{lista.length === 200 ? 'últimos 200 registos' : `${lista.length} registos`}</span>}
      </div>
      <section className="tabela">
        <div className="tabela-linha tabela-cabecalho auditoria-linha">
          <span>Quando</span>
          <span>Quem</span>
          <span>Ação</span>
          <span>O quê</span>
          <span>Campos</span>
        </div>
        {lista.map((r) => (
          <div key={r.id} className="auditoria-bloco">
            <div className="tabela-linha auditoria-linha" onClick={() => setAberto(aberto === r.id ? null : r.id)}>
              <span>{new Date(r.data).toLocaleString('pt-PT', { dateStyle: 'short', timeStyle: 'medium' })}</span>
              <span>{r.utilizador ?? <span className="cinzento-escuro">sistema</span>}</span>
              <span className={`etiqueta ${classeAcao[r.acao]}`}>{nomeAcaoAuditoria[r.acao]}</span>
              <span>
                <span className="forte">{r.entidade}</span> #{r.entidadeId}
              </span>
              <span className="cinzento-escuro">{Object.keys(ler(r.acao === 'Eliminar' ? r.valoresAntes : r.valoresDepois)).join(', ') || '—'}</span>
            </div>
            {aberto === r.id && <Diferencas registo={r} />}
          </div>
        ))}
        <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
        {dados && lista.length === 0 && <div className="vazio">Não há registos com estes filtros.</div>}
      </section>
    </>
  );
}

function Diferencas({ registo }: { registo: RegistoAuditoria }) {
  const antes = ler(registo.valoresAntes);
  const depois = ler(registo.valoresDepois);
  const campos = Array.from(new Set([...Object.keys(antes), ...Object.keys(depois)]));
  return (
    <div className="auditoria-detalhe resultado-importacao">
      <table>
        <thead>
          <tr>
            <th>Campo</th>
            {registo.acao !== 'Criar' && <th>Antes</th>}
            {registo.acao !== 'Eliminar' && <th>Depois</th>}
          </tr>
        </thead>
        <tbody>
          {campos.map((c) => (
            <tr key={c}>
              <td>{c}</td>
              {registo.acao !== 'Criar' && <td>{mostrar(antes[c])}</td>}
              {registo.acao !== 'Eliminar' && <td>{mostrar(depois[c])}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
