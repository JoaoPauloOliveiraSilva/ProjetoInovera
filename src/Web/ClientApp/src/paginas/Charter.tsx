import { useState } from 'react';
import type { FormEvent, ReactNode } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import { charter as api } from '../api/servicos';
import {
  classeEstadoIniciativa,
  classeEstadoTrabalho,
  nomeCategoriaLicao,
  nomeEstadoIniciativa,
  nomeEstadoRequisito,
  nomeEstadoTrabalho,
  nomeImpacto,
  nomePilar,
  nomeProbabilidade,
  nomeResposta,
  nomeTipoEntrega,
  nomeTipoIniciativa,
  nomeTipoRequisito,
  nomeTipoTarefa,
} from '../api/tipos';
import type {
  CharterGeral,
  Entrega,
  Licao,
  LinhaOrcamento,
  ProjectCharter as Charter,
  Requisito,
  Risco,
  SeccaoCharter,
  Tarefa,
} from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { SetaContinuar } from '../componentes/Icones';
import { LinkExterno, TabelaEditavel, data, euros, hoje, opcoes, texto } from '../componentes/TabelaEditavel';
import type { Campo, DefTabela } from '../componentes/TabelaEditavel';
import { formatarData } from '../dados/regras';
import { EquipaTrabalho, Gantt } from './CharterEquipa';

const separadores = [
  ['geral', 'informação geral'],
  ['planeamento', 'planeamento'],
  ['riscos', 'riscos'],
  ['orcamento', 'orçamento'],
  ['conformidade', 'conformidade'],
  ['licoes', 'lições aprendidas'],
] as const;

type Separador = (typeof separadores)[number][0];

/** Project Charter digital (modelo Excel da dst): 6 separadores, cada linha grava-se sozinha. */
export function CharterPagina() {
  const { id } = useParams();
  const iniciativaId = Number(id);
  const [parametros, setParametros] = useSearchParams();
  const atual: Separador = separadores.find(([s]) => s === parametros.get('separador'))?.[0] ?? 'geral';
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.obter(iniciativaId), [iniciativaId]);

  return (
    <>
      <Cabecalho titulo="project charter" corLampada="#7a9a3a" />
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && (
        <>
          <div className="charter-topo">
            <div>
              <span className="num">{dados.codigo}</span> · <span className="forte">{dados.titulo}</span>
            </div>
            <div className="charter-dados">
              <span>{nomeTipoIniciativa[dados.tipo]}</span>
              <span className={`etiqueta ${classeEstadoIniciativa[dados.estado]}`}>{nomeEstadoIniciativa[dados.estado]}</span>
              <span>responsável: {dados.responsavel ?? '—'}</span>
              {dados.pilar && <span>{nomePilar[dados.pilar]}</span>}
              <span>
                {dados.dataInicio ? formatarData(dados.dataInicio) : '—'} → {dados.dataFim ? formatarData(dados.dataFim) : '—'}
              </span>
              <Link className="ligacao" to={`/inovacao/iniciativas/${dados.iniciativaId}`}>
                ficha da iniciativa
              </Link>
            </div>
          </div>
          <nav className="separadores" aria-label="Separadores do Project Charter">
            {separadores
              .filter(([s]) => s !== 'orcamento' || dados.podeVerOrcamento)
              .map(([s, nome]) => (
              <button
                key={s}
                className={s === atual ? 'ativo' : undefined}
                onClick={() => setParametros(s === 'geral' ? {} : { separador: s }, { replace: true })}
              >
                {nome}
                {s === 'riscos' && dados.riscos.some((r) => r.faltaPlanoMitigacao) && (
                  <span className="ponto-alerta" title="Há riscos sem plano de ação" />
                )}
              </button>
              ))}
          </nav>
          {atual === 'geral' && <Geral key={dados.iniciativaId} charter={dados} aoGuardar={recarregar} />}
          {atual === 'planeamento' && (
            <>
              <Gantt charter={dados} />
              <SeccaoTabela def={defTarefas} charter={dados} itens={dados.tarefas} aoMudar={recarregar} />
              <SeccaoTabela def={defEntregas} charter={dados} itens={dados.entregas} aoMudar={recarregar} />
              <EquipaTrabalho charter={dados} aoMudar={recarregar} />
            </>
          )}
          {atual === 'riscos' && (
            <>
              <AvisoRiscos riscos={dados.riscos} />
              <SeccaoTabela def={defRiscos} charter={dados} itens={dados.riscos} aoMudar={recarregar} />
            </>
          )}
          {atual === 'orcamento' && dados.podeVerOrcamento && (
            <>
              <SeccaoTabela def={defOrcamento} charter={dados} itens={dados.orcamento} aoMudar={recarregar} />
              <TotaisOrcamento charter={dados} />
            </>
          )}
          {atual === 'conformidade' && (
            <SeccaoTabela def={defConformidade} charter={dados} itens={dados.conformidade} aoMudar={recarregar} />
          )}
          {atual === 'licoes' && (
            <>
              <p className="cinzento-escuro charter-nota">
                Cada lição registada entra no{' '}
                <Link className="ligacao" to="/inovacao/licoes">
                  repositório de lições aprendidas (Mod.252)
                </Link>{' '}
                e a equipa de inovação recebe um aviso.
              </p>
              <SeccaoTabela def={defLicoes} charter={dados} itens={dados.licoes} aoMudar={recarregar} />
            </>
          )}
        </>
      )}
    </>
  );
}

// ---------- 1. Informação geral ----------

const camposGeral: { nome: keyof CharterGeral; rotulo: string; linhas?: number; max?: number }[] = [
  { nome: 'supervisorNome', rotulo: 'supervisor', max: 200 },
  { nome: 'fase', rotulo: 'fase atual', max: 150 },
  { nome: 'classe', rotulo: 'classe do charter', max: 150 },
  { nome: 'decisaoAprovacao', rotulo: 'decisão de aprovação', linhas: 2 },
  { nome: 'contextoJustificacao', rotulo: 'contexto e justificação', linhas: 4 },
  { nome: 'businessCase', rotulo: 'business case', linhas: 4 },
  { nome: 'objetivos', rotulo: 'objetivos (um por linha)', linhas: 4 },
  { nome: 'resultadosEsperados', rotulo: 'resultados esperados (um por linha)', linhas: 4 },
];

function Geral({ charter, aoGuardar }: { charter: Charter; aoGuardar: () => void }) {
  const [dados, setDados] = useState<CharterGeral>(charter.geral);
  const [erro, setErro] = useState('');
  const [aEnviar, setAEnviar] = useState(false);
  const [guardado, setGuardado] = useState(false);

  if (!charter.podeEditar) {
    return (
      <div className="ficha ficha-simples charter-painel">
        <dl>
          {camposGeral.map((c) => (
            <Linha key={c.nome} rotulo={c.rotulo}>
              {charter.geral[c.nome] ?? '—'}
            </Linha>
          ))}
        </dl>
      </div>
    );
  }

  const mudar = (nome: keyof CharterGeral, valor: string) => {
    setGuardado(false);
    setDados((d) => ({ ...d, [nome]: valor || null }));
  };

  const submeter = async (e: FormEvent) => {
    e.preventDefault();
    setErro('');
    setGuardado(false);
    setAEnviar(true);
    try {
      await api.guardarGeral(charter.iniciativaId, dados);
      setGuardado(true);
      aoGuardar();
    } catch (falha) {
      setErro(falha instanceof Error ? falha.message : String(falha));
    } finally {
      setAEnviar(false);
    }
  };

  return (
    <form className="form-gaveta charter-painel" onSubmit={(e) => void submeter(e)}>
      <div className="campos-tres">
        {camposGeral
          .filter((c) => !c.linhas)
          .map((c) => (
            <label key={c.nome} className="campo">
              <span>{c.rotulo}</span>
              <input value={dados[c.nome] ?? ''} maxLength={c.max} onChange={(e) => mudar(c.nome, e.target.value)} />
            </label>
          ))}
      </div>
      {camposGeral
        .filter((c) => c.linhas)
        .map((c) => (
          <label key={c.nome} className="campo">
            <span>{c.rotulo}</span>
            <textarea rows={c.linhas} value={dados[c.nome] ?? ''} onChange={(e) => mudar(c.nome, e.target.value)} />
          </label>
        ))}
      {erro && <div className="erro">{erro}</div>}
      {guardado && <div className="aviso-ok">Informação geral guardada.</div>}
      <div className="botoes">
        <button type="submit" className="botao-largo" disabled={aEnviar}>
          {aEnviar ? 'a guardar…' : 'guardar informação geral'} <SetaContinuar className="seta-vermelha" />
        </button>
      </div>
    </form>
  );
}

function Linha({ rotulo, children }: { rotulo: string; children: ReactNode }) {
  return (
    <>
      <dt>{rotulo}</dt>
      <dd className="texto">{children}</dd>
    </>
  );
}

// ---------- Tabelas editáveis (separadores 2 a 6) ----------

interface DefSeccao<T extends { id: number }> extends Omit<DefTabela<T>, 'campos'> {
  seccao: SeccaoCharter;
  /** Quem pode editar este separador (por omissão, quem pode editar o Charter). */
  podeEditar?: (charter: Charter) => boolean;
  campos: (charter: Charter) => Campo[];
}

const Relatorio = ({ url }: { url: string | null }) => <LinkExterno url={url} />;

const defTarefas: DefSeccao<Tarefa> = {
  seccao: 'tarefas',
  titulo: 'tarefas (WBS)',
  singular: 'tarefa',
  feminino: true,
  colunas: '70px minmax(200px, 1fr) 110px 150px 100px 100px 120px',
  cabecalhos: ['WBS', 'Tarefa', 'Tipo', 'Responsável', 'Início', 'Fim', 'Estado'],
  celulas: (t) => [
    <span className="num">{t.codigoWbs || '—'}</span>,
    <span className="forte">{t.nome}</span>,
    nomeTipoTarefa[t.tipo],
    t.responsavel ?? '—',
    data(t.inicioPlaneado),
    data(t.fimPlaneado),
    t.atrasada ? (
      <span className="etiqueta estado-em-discussao">atrasada</span>
    ) : (
      <span className={`etiqueta ${classeEstadoTrabalho[t.estado]}`}>{nomeEstadoTrabalho[t.estado]}</span>
    ),
  ],
  campos: (c) => [
    { nome: 'codigoWbs', rotulo: 'código WBS (ex.: 1.2)', max: 20 },
    { nome: 'nome', rotulo: 'tarefa', obrigatorio: true, max: 300 },
    { nome: 'tipo', rotulo: 'tipo', tipo: 'opcoes', opcoes: opcoes(nomeTipoTarefa), obrigatorio: true },
    { nome: 'estado', rotulo: 'estado', tipo: 'opcoes', opcoes: opcoes(nomeEstadoTrabalho), obrigatorio: true },
    { nome: 'responsavel', rotulo: 'responsável', max: 200 },
    {
      nome: 'milestoneId',
      rotulo: 'milestone',
      tipo: 'opcoes',
      numerico: true,
      opcoes: c.entregas.filter((e) => e.tipo === 'Milestone').map((e): [string, string] => [String(e.id), `${e.codigo} · ${e.descricao}`]),
    },
    { nome: 'inicioPlaneado', rotulo: 'início planeado', tipo: 'data' },
    { nome: 'fimPlaneado', rotulo: 'fim planeado', tipo: 'data' },
    { nome: 'inicioReal', rotulo: 'início real', tipo: 'data' },
    { nome: 'fimReal', rotulo: 'fim real', tipo: 'data' },
  ],
  valores: (t) => ({
    codigoWbs: texto(t?.codigoWbs),
    nome: texto(t?.nome),
    tipo: t?.tipo ?? 'Gestao',
    estado: t?.estado ?? 'Planeado',
    responsavel: texto(t?.responsavel),
    milestoneId: texto(t?.milestoneId),
    inicioPlaneado: texto(t?.inicioPlaneado),
    fimPlaneado: texto(t?.fimPlaneado),
    inicioReal: texto(t?.inicioReal),
    fimReal: texto(t?.fimReal),
  }),
  vazio: 'Ainda não há tarefas. Comece pela estrutura WBS (1, 1.1, 1.2…).',
};

const defEntregas: DefSeccao<Entrega> = {
  seccao: 'entregas',
  titulo: 'entregáveis e milestones',
  singular: 'entregável / milestone',
  colunas: '60px 100px minmax(200px, 1fr) 100px 120px 100px 80px',
  cabecalhos: ['Código', 'Tipo', 'Descrição', 'Prazo', 'Estado', 'Concluído', 'Relatório'],
  celulas: (e) => [
    <span className="num">{e.codigo || '—'}</span>,
    nomeTipoEntrega[e.tipo],
    <span className="forte">{e.descricao}</span>,
    data(e.prazo),
    <span className={`etiqueta ${classeEstadoTrabalho[e.estado]}`}>{nomeEstadoTrabalho[e.estado]}</span>,
    data(e.dataConclusao),
    <Relatorio url={e.linkRelatorio} />,
  ],
  campos: () => [
    { nome: 'tipo', rotulo: 'tipo', tipo: 'opcoes', opcoes: opcoes(nomeTipoEntrega), obrigatorio: true },
    { nome: 'codigo', rotulo: 'código (ex.: E1, M1)', max: 20 },
    { nome: 'descricao', rotulo: 'descrição', tipo: 'area', obrigatorio: true, max: 1000 },
    { nome: 'prazo', rotulo: 'prazo', tipo: 'data' },
    { nome: 'estado', rotulo: 'estado', tipo: 'opcoes', opcoes: opcoes(nomeEstadoTrabalho), obrigatorio: true },
    { nome: 'meioVerificacao', rotulo: 'meio de verificação', max: 300 },
    { nome: 'controlo', rotulo: 'controlo (horas, custos, desvios)', tipo: 'area' },
    {
      nome: 'linkRelatorio',
      rotulo: 'link do relatório (OneDrive)',
      tipo: 'link',
      max: 2000,
      ajuda: 'Ao colar o link, a entrega fica concluída e o relatório entra no repositório de conhecimento codificado (Mod.237).',
    },
    { nome: 'dataConclusao', rotulo: 'data de conclusão', tipo: 'data' },
  ],
  valores: (e) => ({
    tipo: e?.tipo ?? 'Entregavel',
    codigo: texto(e?.codigo),
    descricao: texto(e?.descricao),
    prazo: texto(e?.prazo),
    estado: e?.estado ?? 'Planeado',
    meioVerificacao: texto(e?.meioVerificacao),
    controlo: texto(e?.controlo),
    linkRelatorio: texto(e?.linkRelatorio),
    dataConclusao: texto(e?.dataConclusao),
  }),
  vazio: 'Ainda não há entregáveis nem milestones.',
};

function Severidade({ r }: { r: Risco }) {
  const classe = r.faltaPlanoMitigacao ? 'estado-em-discussao' : r.exigeMitigacao ? 'estado-em-avaliacao' : 'estado-aprovada';
  const titulo = r.faltaPlanoMitigacao
    ? 'Severidade acima de 4 sem plano de ação'
    : r.exigeMitigacao
      ? 'Severidade acima de 4: exige mitigação'
      : 'Severidade aceitável';
  return (
    <span className={`etiqueta ${classe}`} title={titulo}>
      {r.severidade}
    </span>
  );
}

const defRiscos: DefSeccao<Risco> = {
  seccao: 'riscos',
  titulo: 'registo de riscos',
  singular: 'risco',
  colunas: '40px minmax(200px, 1fr) 110px 140px 60px 100px 150px 100px',
  cabecalhos: ['Nº', 'Risco', 'Impacto', 'Probabilidade', 'Sev.', 'Resposta', 'Responsável', 'Prazo'],
  celulas: (r) => [
    <span className="num">{r.numero}</span>,
    <span className="forte">{r.descricao}</span>,
    nomeImpacto[r.impacto],
    nomeProbabilidade[r.probabilidade],
    <Severidade r={r} />,
    r.resposta ? nomeResposta[r.resposta] : '—',
    r.responsavel ?? '—',
    data(r.prazo),
  ],
  campos: () => [
    { nome: 'descricao', rotulo: 'descrição do risco', tipo: 'area', obrigatorio: true, max: 1000 },
    { nome: 'data', rotulo: 'data de identificação', tipo: 'data' },
    { nome: 'impacto', rotulo: 'impacto', tipo: 'opcoes', opcoes: opcoes(nomeImpacto), obrigatorio: true },
    { nome: 'probabilidade', rotulo: 'probabilidade', tipo: 'opcoes', opcoes: opcoes(nomeProbabilidade), obrigatorio: true },
    { nome: 'resposta', rotulo: 'resposta', tipo: 'opcoes', opcoes: opcoes(nomeResposta) },
    {
      nome: 'planoAcao',
      rotulo: 'plano de ação',
      tipo: 'area',
      ajuda: 'Necessário quando impacto × probabilidade é maior do que 4 (o risco fica assinalado a vermelho até ter plano).',
    },
    { nome: 'responsavel', rotulo: 'responsável', max: 200 },
    { nome: 'prazo', rotulo: 'prazo', tipo: 'data' },
    { nome: 'dataConclusao', rotulo: 'data de conclusão', tipo: 'data' },
    { nome: 'avaliacaoEficacia', rotulo: 'avaliação da eficácia', tipo: 'area' },
  ],
  valores: (r) => ({
    descricao: texto(r?.descricao),
    data: r ? r.data : hoje(),
    impacto: r?.impacto ?? 'Medio',
    probabilidade: r?.probabilidade ?? 'Improvavel',
    resposta: texto(r?.resposta),
    planoAcao: texto(r?.planoAcao),
    responsavel: texto(r?.responsavel),
    prazo: texto(r?.prazo),
    dataConclusao: texto(r?.dataConclusao),
    avaliacaoEficacia: texto(r?.avaliacaoEficacia),
  }),
  vazio: 'Ainda não há riscos registados.',
};

function AvisoRiscos({ riscos }: { riscos: Risco[] }) {
  const semPlano = riscos.filter((r) => r.faltaPlanoMitigacao);
  if (semPlano.length === 0) return null;
  return (
    <div className="charter-aviso">
      {semPlano.length === 1 ? 'Há 1 risco' : `Há ${semPlano.length} riscos`} com severidade acima de 4 sem plano de ação (
      {semPlano.map((r) => `nº ${r.numero}`).join(', ')}).
    </div>
  );
}

const defOrcamento: DefSeccao<LinhaOrcamento> = {
  seccao: 'orcamento',
  titulo: 'orçamento',
  singular: 'linha de orçamento',
  feminino: true,
  podeEditar: (c) => c.podeEditarOrcamento,
  colunas: '160px minmax(180px, 1fr) 70px 120px 120px 120px',
  cabecalhos: ['Categoria', 'Descrição', 'Ano', 'Previsto', 'Real', 'Variação'],
  celulas: (l) => [
    <span className="forte">{l.categoria}</span>,
    l.descricao ?? '—',
    l.ano,
    euros(l.valorPrevisto),
    euros(l.valorReal),
    <span className={l.variacao !== null && l.variacao > 0 ? 'erro' : undefined}>{euros(l.variacao)}</span>,
  ],
  campos: () => [
    { nome: 'categoria', rotulo: 'categoria (ex.: RH, serviços, equipamento)', obrigatorio: true, max: 100 },
    { nome: 'descricao', rotulo: 'descrição' },
    { nome: 'ano', rotulo: 'ano', tipo: 'numero', obrigatorio: true },
    { nome: 'valorPrevisto', rotulo: 'valor previsto (€)', tipo: 'numero', obrigatorio: true },
    { nome: 'valorReal', rotulo: 'valor real (€)', tipo: 'numero' },
    { nome: 'observacoes', rotulo: 'observações', tipo: 'area' },
  ],
  valores: (l) => ({
    categoria: texto(l?.categoria),
    descricao: texto(l?.descricao),
    ano: texto(l?.ano ?? new Date().getFullYear()),
    valorPrevisto: texto(l?.valorPrevisto ?? 0),
    valorReal: texto(l?.valorReal),
    observacoes: texto(l?.observacoes),
  }),
  vazio: 'Ainda não há linhas de orçamento.',
};

function TotaisOrcamento({ charter }: { charter: Charter }) {
  if (charter.totaisOrcamento.length === 0) return null;
  return (
    <section className="charter-seccao">
      <h3 className="secao">totais por ano</h3>
      <div className="resultado-importacao">
        <table>
          <thead>
            <tr>
              <th>Ano</th>
              <th>Previsto</th>
              <th>Real</th>
              <th>Execução</th>
            </tr>
          </thead>
          <tbody>
            {charter.totaisOrcamento.map((t) => (
              <tr key={t.ano}>
                <td>{t.ano}</td>
                <td>{euros(t.previsto)}</td>
                <td>{euros(t.real)}</td>
                <td>{t.previsto > 0 ? `${Math.round((t.real / t.previsto) * 100)} %` : '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

const classeEstadoRequisito = (r: Requisito) =>
  r.estado === 'Cumprido' ? 'estado-aprovada' : r.estado === 'NaoAplicavel' ? 'estado-nao-aprovada' : 'estado-em-avaliacao';

const defConformidade: DefSeccao<Requisito> = {
  seccao: 'conformidade',
  titulo: 'requisitos de conformidade',
  singular: 'requisito',
  colunas: '100px minmax(200px, 1fr) 150px 150px 100px 120px',
  cabecalhos: ['Tipo', 'Requisito', 'Entidade', 'Responsável', 'Prevista', 'Estado'],
  celulas: (r) => [
    nomeTipoRequisito[r.tipo],
    <span className="forte">{r.requisito}</span>,
    r.entidade ?? '—',
    r.responsavel ?? '—',
    data(r.dataPrevista),
    <span className={`etiqueta ${classeEstadoRequisito(r)}`}>{nomeEstadoRequisito[r.estado]}</span>,
  ],
  campos: () => [
    { nome: 'tipo', rotulo: 'tipo', tipo: 'opcoes', opcoes: opcoes(nomeTipoRequisito), obrigatorio: true },
    { nome: 'requisito', rotulo: 'requisito', obrigatorio: true, max: 300 },
    { nome: 'descricao', rotulo: 'descrição', tipo: 'area' },
    { nome: 'entidade', rotulo: 'entidade', max: 200 },
    { nome: 'resposta', rotulo: 'resposta / evidência', tipo: 'area' },
    { nome: 'responsavel', rotulo: 'responsável', max: 200 },
    { nome: 'dataPrevista', rotulo: 'data prevista', tipo: 'data' },
    { nome: 'dataConfirmacao', rotulo: 'data de confirmação', tipo: 'data' },
    { nome: 'estado', rotulo: 'estado', tipo: 'opcoes', opcoes: opcoes(nomeEstadoRequisito), obrigatorio: true },
    { nome: 'observacoes', rotulo: 'observações', tipo: 'area' },
  ],
  valores: (r) => ({
    tipo: r?.tipo ?? 'Legal',
    requisito: texto(r?.requisito),
    descricao: texto(r?.descricao),
    entidade: texto(r?.entidade),
    resposta: texto(r?.resposta),
    responsavel: texto(r?.responsavel),
    dataPrevista: texto(r?.dataPrevista),
    dataConfirmacao: texto(r?.dataConfirmacao),
    estado: r?.estado ?? 'Pendente',
    observacoes: texto(r?.observacoes),
  }),
  vazio: 'Ainda não há requisitos legais, de qualidade, ambiente ou segurança.',
};

const defLicoes: DefSeccao<Licao> = {
  seccao: 'licoes',
  titulo: 'lições aprendidas',
  singular: 'lição aprendida',
  feminino: true,
  colunas: '120px minmax(220px, 1fr) minmax(160px, 0.6fr) 100px',
  cabecalhos: ['Categoria', 'Lição', 'Impacto', 'Data'],
  celulas: (l) => [nomeCategoriaLicao[l.categoria], <span className="forte">{l.licao}</span>, l.impacto ?? '—', data(l.data)],
  campos: () => [
    { nome: 'categoria', rotulo: 'categoria', tipo: 'opcoes', opcoes: opcoes(nomeCategoriaLicao), obrigatorio: true },
    { nome: 'licao', rotulo: 'lição', tipo: 'area', obrigatorio: true, max: 1000 },
    { nome: 'descricao', rotulo: 'descrição / contexto', tipo: 'area' },
    { nome: 'impacto', rotulo: 'impacto na iniciativa', tipo: 'area' },
  ],
  valores: (l) => ({
    categoria: l?.categoria ?? 'Gestao',
    licao: texto(l?.licao),
    descricao: texto(l?.descricao),
    impacto: texto(l?.impacto),
  }),
  vazio: 'Ainda não há lições aprendidas.',
};

/** Converte os valores do formulário (texto) no corpo do pedido: vazio → null, números → number. */
function SeccaoTabela<T extends { id: number }>({
  def,
  charter,
  itens,
  aoMudar,
}: {
  def: DefSeccao<T>;
  charter: Charter;
  itens: T[];
  aoMudar: () => void;
}) {
  return (
    <TabelaEditavel
      def={{ ...def, campos: def.campos(charter) }}
      itens={itens}
      podeEditar={def.podeEditar ? def.podeEditar(charter) : charter.podeEditar}
      guardar={(id, corpo) => api.guardarItem(charter.iniciativaId, def.seccao, id, corpo)}
      apagar={(id) => api.apagarItem(charter.iniciativaId, def.seccao, id)}
      aoMudar={aoMudar}
      sobretitulo={`PROJECT CHARTER · ${charter.codigo}`}
    />
  );
}
