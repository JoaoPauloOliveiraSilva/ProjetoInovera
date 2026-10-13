import { useEffect, useState } from 'react';
import type { FormEvent, ReactNode } from 'react';
import { contas as apiContas } from '../api/servicos';
import type { UtilizadorResumo } from '../api/tipos';
import { useAtualizar } from '../api/usePedido';
import { formatarData } from '../dados/regras';
import { Gaveta } from './Gaveta';
import { Fechar, Mais, SetaContinuar } from './Icones';

/** Valores do formulário, sempre em texto (os inputs trabalham com texto); `paraPedido` converte no fim. */
export type Valores = Record<string, string>;

export interface Campo {
  nome: string;
  rotulo: string;
  tipo?: 'texto' | 'area' | 'data' | 'numero' | 'opcoes' | 'link' | 'booleano' | 'cor' | 'pessoas' | 'lista';
  /** Colunas de cada linha de um campo 'lista' (ex.: Atividades · Quem · Quando). */
  subcampos?: SubCampo[];
  opcoes?: [string, string][];
  /** Converte o valor escolhido numa opção para número (ex.: id do parceiro). */
  numerico?: boolean;
  obrigatorio?: boolean;
  max?: number;
  ajuda?: string;
}

/** Coluna de um campo 'lista'. */
export interface SubCampo {
  nome: string;
  rotulo: string;
  tipo?: 'texto' | 'data' | 'numero' | 'booleano';
  /** grid-template-columns desta coluna (por omissão 1fr). */
  largura?: string;
  obrigatorio?: boolean;
}

/** Definição de uma tabela editável: colunas, células e o formulário da gaveta. */
export interface DefTabela<T extends { id: number }> {
  titulo: string;
  singular: string;
  /** Género do nome, para "novo"/"nova". */
  feminino?: boolean;
  /** grid-template-columns da tabela. */
  colunas: string;
  cabecalhos: string[];
  celulas: (item: T) => ReactNode[];
  campos: Campo[];
  valores: (item: T | null) => Valores;
  vazio: string;
  /** Linhas que não se podem apagar (por omissão, todas se podem). */
  podeApagar?: (item: T) => boolean;
}

// Ajudas para as definições
export const opcoes = (nomes: Record<string, string>): [string, string][] => Object.entries(nomes);
export const texto = (v: string | number | boolean | null | undefined) =>
  v === null || v === undefined ? '' : typeof v === 'boolean' ? (v ? 'true' : '') : String(v);
export const data = (v: string | null) => (v ? formatarData(v) : '—');
export const euros = (v: number | null) => (v === null ? '—' : v.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' }));
export const hoje = () => new Date().toISOString().slice(0, 10);
export const pessoas = (lista: UtilizadorResumo[]) => JSON.stringify(lista.map((u) => ({ id: u.id, nome: u.nome })));
/** Valor inicial de um campo 'lista': cada objeto passa a texto (como os inputs). */
export const lista = (itens: object[] | null | undefined) =>
  JSON.stringify(
    (itens ?? []).map((o) => Object.fromEntries(Object.entries(o).map(([k, v]) => [k, v === null || v === undefined ? '' : String(v)]))),
  );

/** Campos de "Atividades · Quem · Quando" (Mod.236, 237, 238). */
export const subcamposAtividades: SubCampo[] = [
  { nome: 'descricao', rotulo: 'atividade', obrigatorio: true, largura: 'minmax(140px, 2fr)' },
  { nome: 'responsavel', rotulo: 'quem', largura: 'minmax(90px, 1fr)' },
  { nome: 'data', rotulo: 'quando', tipo: 'data', largura: '130px' },
  { nome: 'concluida', rotulo: 'feita', tipo: 'booleano', largura: '44px' },
];

/** Próxima atividade por fazer (para mostrar na tabela). */
export function proximaAtividade(atividades: { descricao: string; data: string | null; concluida: boolean }[] | null | undefined) {
  const porFazer = (atividades ?? []).filter((a) => !a.concluida);
  if (porFazer.length === 0) return (atividades ?? []).length > 0 ? <span className="cinzento-escuro">todas feitas</span> : '—';
  const a = [...porFazer].sort((x, y) => (x.data ?? '9999').localeCompare(y.data ?? '9999'))[0];
  return (
    <span title={porFazer.map((x) => `${x.descricao}${x.data ? ` (${formatarData(x.data)})` : ''}`).join('\n')}>
      {a.data ? `${formatarData(a.data)} · ` : ''}
      {a.descricao}
      {porFazer.length > 1 && <span className="cinzento-escuro"> +{porFazer.length - 1}</span>}
    </span>
  );
}

export function LinkExterno({ url, texto: rotulo = 'abrir ↗' }: { url: string | null; texto?: string }) {
  if (!url) return <span className="cinzento-escuro">—</span>;
  return (
    <a className="ligacao" href={url} target="_blank" rel="noreferrer" onClick={(e) => e.stopPropagation()}>
      {rotulo}
    </a>
  );
}

/** Converte os valores do formulário (texto) no corpo do pedido: vazio → null, números → number, sim/não → boolean. */
export function paraPedido(campos: Campo[], valores: Valores): Record<string, unknown> {
  const corpo: Record<string, unknown> = {};
  for (const c of campos) {
    const v = (valores[c.nome] ?? '').trim();
    if (c.tipo === 'booleano') corpo[c.nome] = v === 'true';
    else if (c.tipo === 'pessoas') corpo[c.nome] = lerPessoas(v).map((p) => p.id);
    else if (c.tipo === 'lista') corpo[c.nome] = lerLista(v).filter((l) => Object.values(l).some((x) => x.trim() && x !== 'false')).map((l) => linhaParaPedido(c.subcampos ?? [], l));
    else if (!v) corpo[c.nome] = null;
    else if (c.tipo === 'numero' || c.numerico) corpo[c.nome] = Number(v.replace(',', '.'));
    else corpo[c.nome] = v;
  }
  return corpo;
}

function lerLista(v: string): Record<string, string>[] {
  if (!v) return [];
  try {
    return JSON.parse(v) as Record<string, string>[];
  } catch {
    return [];
  }
}

function linhaParaPedido(subcampos: SubCampo[], linha: Record<string, string>) {
  const corpo: Record<string, unknown> = {};
  for (const s of subcampos) {
    const v = (linha[s.nome] ?? '').trim();
    if (s.tipo === 'booleano') corpo[s.nome] = v === 'true';
    else if (!v) corpo[s.nome] = null;
    else if (s.tipo === 'numero') corpo[s.nome] = Number(v.replace(',', '.'));
    else corpo[s.nome] = v;
  }
  return corpo;
}

function lerPessoas(v: string): { id: number; nome: string }[] {
  if (!v) return [];
  try {
    return JSON.parse(v) as { id: number; nome: string }[];
  } catch {
    return [];
  }
}

/** Tabela com botão "novo" e gaveta para criar, editar e apagar linhas. */
export function TabelaEditavel<T extends { id: number }>({
  def,
  itens,
  podeEditar,
  guardar,
  apagar,
  aoMudar,
  sobretitulo,
  semTitulo,
}: {
  def: DefTabela<T>;
  itens: T[];
  podeEditar: boolean;
  guardar: (id: number | null, corpo: Record<string, unknown>) => Promise<unknown>;
  apagar: (id: number) => Promise<unknown>;
  aoMudar: () => void;
  sobretitulo: string;
  /** Sem o título da secção (quando a página já o mostra). */
  semTitulo?: boolean;
}) {
  const [aberto, setAberto] = useState<T | 'novo' | null>(null);
  const estilo = { gridTemplateColumns: def.colunas };
  const botaoNovo = podeEditar && (
    <button className="botao-pequeno com-icone" onClick={() => setAberto('novo')}>
      <Mais /> {def.feminino ? 'nova' : 'novo'} {def.singular}
    </button>
  );

  return (
    <section className="charter-seccao">
      <div className="charter-seccao-topo">
        {semTitulo ? <span /> : <h3 className="secao">{def.titulo}</h3>}
        {botaoNovo}
      </div>
      <div className="tabela charter-tabela">
        <div className="tabela-linha tabela-cabecalho" style={estilo}>
          {def.cabecalhos.map((c) => (
            <span key={c}>{c}</span>
          ))}
        </div>
        {itens.map((item) => (
          <div
            key={item.id}
            className={`tabela-linha${podeEditar ? '' : ' so-leitura'}`}
            style={estilo}
            onClick={podeEditar ? () => setAberto(item) : undefined}
          >
            {def.celulas(item).map((celula, i) => (
              <span key={i}>{celula}</span>
            ))}
          </div>
        ))}
        {itens.length === 0 && <div className="vazio">{def.vazio}</div>}
      </div>
      <Gaveta aberta={aberto !== null} aoFechar={() => setAberto(null)}>
        {aberto !== null && (
          <FormItem
            key={aberto === 'novo' ? 'novo' : aberto.id}
            def={def}
            item={aberto === 'novo' ? null : aberto}
            sobretitulo={sobretitulo}
            guardar={guardar}
            apagar={apagar}
            aoTerminar={() => {
              setAberto(null);
              aoMudar();
            }}
          />
        )}
      </Gaveta>
    </section>
  );
}

function FormItem<T extends { id: number }>({
  def,
  item,
  sobretitulo,
  guardar,
  apagar,
  aoTerminar,
}: {
  def: DefTabela<T>;
  item: T | null;
  sobretitulo: string;
  guardar: (id: number | null, corpo: Record<string, unknown>) => Promise<unknown>;
  apagar: (id: number) => Promise<unknown>;
  aoTerminar: () => void;
}) {
  const atualizarListas = useAtualizar();
  const [valores, setValores] = useState<Valores>(() => def.valores(item));
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);
  const [confirmarApagar, setConfirmarApagar] = useState(false);

  const executar = async (acao: () => Promise<unknown>) => {
    setErro('');
    setAEnviar(true);
    try {
      await acao();
      atualizarListas();
      aoTerminar();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
      setAEnviar(false);
    }
  };

  const submeter = (e: FormEvent) => {
    e.preventDefault();
    void executar(() => guardar(item?.id ?? null, paraPedido(def.campos, valores)));
  };

  const mudar = (nome: string, valor: string) => setValores((v) => ({ ...v, [nome]: valor }));

  return (
    <form className="form-gaveta" onSubmit={submeter}>
      <div className="sobretitulo">{sobretitulo}</div>
      <h2 className="detalhe-titulo">{item ? `editar ${def.singular}` : `${def.feminino ? 'nova' : 'novo'} ${def.singular}`}</h2>
      {def.campos.map((c) =>
        c.tipo === 'lista' ? (
          <div key={c.nome} className="campo">
            <span>{c.rotulo}</span>
            <EditarLista subcampos={c.subcampos ?? []} valor={valores[c.nome] ?? ''} aoMudar={(v) => mudar(c.nome, v)} />
            {c.ajuda && <small className="ajuda">{c.ajuda}</small>}
          </div>
        ) : c.tipo === 'booleano' ? (
          <label key={c.nome} className="caixa-simples">
            <input type="checkbox" checked={valores[c.nome] === 'true'} onChange={(e) => mudar(c.nome, e.target.checked ? 'true' : '')} />
            <span>{c.rotulo}</span>
          </label>
        ) : (
          <label key={c.nome} className="campo">
            <span>
              {c.rotulo}
              {c.obrigatorio && ' *'}
            </span>
            <Entrada campo={c} valor={valores[c.nome] ?? ''} aoMudar={(v) => mudar(c.nome, v)} />
            {c.ajuda && <small className="ajuda">{c.ajuda}</small>}
          </label>
        ),
      )}
      {erro && <div className="erro">{erro}</div>}
      <div className="botoes">
        {item &&
          (def.podeApagar?.(item) ?? true) &&
          (confirmarApagar ? (
            <button type="button" className="botao-largo curto perigo" disabled={aEnviar} onClick={() => void executar(() => apagar(item.id))}>
              confirmar apagar
            </button>
          ) : (
            <button type="button" className="botao-largo curto" onClick={() => setConfirmarApagar(true)}>
              apagar
            </button>
          ))}
        <button type="submit" className="botao-largo" disabled={aEnviar}>
          {aEnviar ? 'a guardar…' : 'guardar'} <SetaContinuar className="seta-vermelha" />
        </button>
      </div>
    </form>
  );
}

function Entrada({ campo, valor, aoMudar }: { campo: Campo; valor: string; aoMudar: (v: string) => void }) {
  switch (campo.tipo) {
    case 'area':
      return (
        <textarea rows={3} value={valor} maxLength={campo.max} required={campo.obrigatorio} onChange={(e) => aoMudar(e.target.value)} />
      );
    case 'data':
      return <input type="date" value={valor} required={campo.obrigatorio} onChange={(e) => aoMudar(e.target.value)} />;
    case 'numero':
      return <input type="number" step="any" value={valor} required={campo.obrigatorio} onChange={(e) => aoMudar(e.target.value)} />;
    case 'cor':
      return <input type="color" className="campo-cor" value={valor || '#c8102e'} onChange={(e) => aoMudar(e.target.value)} />;
    case 'link':
      return (
        <input
          type="url"
          value={valor}
          maxLength={campo.max}
          placeholder="https://…"
          required={campo.obrigatorio}
          onChange={(e) => aoMudar(e.target.value)}
        />
      );
    case 'opcoes':
      return (
        <select value={valor} required={campo.obrigatorio} onChange={(e) => aoMudar(e.target.value)}>
          {!campo.obrigatorio && <option value="">—</option>}
          {campo.obrigatorio && !valor && <option value="">escolha…</option>}
          {(campo.opcoes ?? []).map(([v, nome]) => (
            <option key={v} value={v}>
              {nome}
            </option>
          ))}
        </select>
      );
    case 'pessoas':
      return <EscolherPessoas valor={valor} aoMudar={aoMudar} />;
    default:
      return <input value={valor} maxLength={campo.max} required={campo.obrigatorio} onChange={(e) => aoMudar(e.target.value)} />;
  }
}

/** Escolha de várias pessoas por pesquisa (nome ou email); guarda [{id, nome}] em JSON. */
function EscolherPessoas({ valor, aoMudar }: { valor: string; aoMudar: (v: string) => void }) {
  const escolhidas = lerPessoas(valor);
  const [procura, setProcura] = useState('');
  const [sugestoes, setSugestoes] = useState<UtilizadorResumo[]>([]);

  useEffect(() => {
    if (procura.trim().length < 2) {
      setSugestoes([]);
      return;
    }
    let ativo = true;
    const t = setTimeout(() => {
      apiContas
        .pesquisar(procura.trim())
        .then((lista) => ativo && setSugestoes(lista))
        .catch(() => ativo && setSugestoes([]));
    }, 250);
    return () => {
      ativo = false;
      clearTimeout(t);
    };
  }, [procura]);

  const guardar = (lista: { id: number; nome: string }[]) => aoMudar(JSON.stringify(lista));

  return (
    <div className="escolher-pessoas">
      {escolhidas.length > 0 && (
        <div className="fichas">
          {escolhidas.map((p) => (
            <span key={p.id} className="ficha-autor-escolhido">
              {p.nome}
              <button type="button" aria-label={`Tirar ${p.nome}`} onClick={() => guardar(escolhidas.filter((x) => x.id !== p.id))}>
                <Fechar />
              </button>
            </span>
          ))}
        </div>
      )}
      <input value={procura} onChange={(e) => setProcura(e.target.value)} placeholder="procurar por nome ou email" />
      {sugestoes.length > 0 && (
        <div className="sugestoes">
          {sugestoes
            .filter((s) => !escolhidas.some((p) => p.id === s.id))
            .map((s) => (
              <button
                type="button"
                key={s.id}
                onClick={() => {
                  guardar([...escolhidas, { id: s.id, nome: s.nome }]);
                  setProcura('');
                  setSugestoes([]);
                }}
              >
                {s.nome} <span className="cinzento-escuro">{s.empresa}</span>
              </button>
            ))}
        </div>
      )}
    </div>
  );
}

/** Tabela pequena dentro do formulário (ex.: Atividades · Quem · Quando); guarda as linhas em JSON. */
function EditarLista({ subcampos, valor, aoMudar }: { subcampos: SubCampo[]; valor: string; aoMudar: (v: string) => void }) {
  const linhas = lerLista(valor);
  const colunas = { gridTemplateColumns: `${subcampos.map((s) => s.largura ?? '1fr').join(' ')} 28px` };
  const guardar = (novas: Record<string, string>[]) => aoMudar(JSON.stringify(novas));
  const mudar = (i: number, nome: string, v: string) => guardar(linhas.map((l, j) => (j === i ? { ...l, [nome]: v } : l)));

  return (
    <div className="lista-campo">
      {linhas.length > 0 && (
        <div className="lista-linha lista-cabecalho" style={colunas}>
          {subcampos.map((s) => (
            <span key={s.nome}>{s.rotulo}</span>
          ))}
          <span />
        </div>
      )}
      {linhas.map((l, i) => (
        <div key={i} className="lista-linha" style={colunas}>
          {subcampos.map((s) =>
            s.tipo === 'booleano' ? (
              <input
                key={s.nome}
                type="checkbox"
                aria-label={s.rotulo}
                checked={l[s.nome] === 'true'}
                onChange={(e) => mudar(i, s.nome, e.target.checked ? 'true' : 'false')}
              />
            ) : (
              <input
                key={s.nome}
                aria-label={s.rotulo}
                type={s.tipo === 'data' ? 'date' : s.tipo === 'numero' ? 'number' : 'text'}
                required={s.obrigatorio}
                value={l[s.nome] ?? ''}
                onChange={(e) => mudar(i, s.nome, e.target.value)}
              />
            ),
          )}
          <button type="button" className="lista-tirar" aria-label="Tirar esta linha" onClick={() => guardar(linhas.filter((_, j) => j !== i))}>
            <Fechar />
          </button>
        </div>
      ))}
      <button type="button" className="botao-pequeno com-icone lista-mais" onClick={() => guardar([...linhas, {}])}>
        <Mais /> acrescentar
      </button>
    </div>
  );
}
