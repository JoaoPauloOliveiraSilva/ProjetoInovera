import { registos as api } from '../api/servicos';
import type { TipoRegistoApi } from '../api/servicos';
import { Permissoes } from '../api/permissoes';
import {
  nomeConfidencialidade,
  nomeEstadoAcordo,
  nomeTipoAtivo,
  nomeTipoParceiro,
  nomeTipoProtocolo,
} from '../api/tipos';
import type { Acordo, Ativo, ConhecimentoTacito, Ferramenta, Parceiro } from '../api/tipos';
import { usePedido } from '../api/usePedido';
import { EstadoPedido } from '../componentes/Estado';
import { Cabecalho } from '../componentes/Layout';
import { LinkExterno, TabelaEditavel, data, euros, hoje, opcoes, texto } from '../componentes/TabelaEditavel';
import type { DefTabela } from '../componentes/TabelaEditavel';
import { usePermissao } from '../sessao/Sessao';

/** Liga uma tabela editável a um tipo de registo da API. */
function Registo<T extends { id: number }>({
  tipo,
  def,
  itens,
  aoMudar,
  semTitulo,
}: {
  tipo: TipoRegistoApi;
  def: DefTabela<T>;
  itens: T[];
  aoMudar: () => void;
  semTitulo?: boolean;
}) {
  const tem = usePermissao();
  return (
    <TabelaEditavel
      def={def}
      itens={itens}
      podeEditar={tem(Permissoes.gerirRegistos)}
      guardar={(id, corpo) => api.guardar(tipo, id, corpo)}
      apagar={(id) => api.apagar(tipo, id)}
      aoMudar={aoMudar}
      sobretitulo="INOVAÇÃO · REGISTOS"
      semTitulo={semTitulo}
    />
  );
}

// ---------- Parcerias (Mod.235) ----------

const defParceiros: DefTabela<Parceiro> = {
  titulo: 'parceiros',
  singular: 'parceiro',
  colunas: 'minmax(200px, 1fr) 160px 120px 140px',
  cabecalhos: ['Parceiro', 'Tipo', 'NIF', 'Acordos (em curso)'],
  celulas: (p) => [<span className="forte">{p.nome}</span>, nomeTipoParceiro[p.tipo], p.nif ?? '—', `${p.acordos} (${p.acordosEmCurso})`],
  campos: [
    { nome: 'nome', rotulo: 'nome', obrigatorio: true, max: 200 },
    { nome: 'tipo', rotulo: 'tipo', tipo: 'opcoes', opcoes: opcoes(nomeTipoParceiro), obrigatorio: true, ajuda: 'Conta para os KPIs 26 e 27.' },
    { nome: 'nif', rotulo: 'NIF', max: 20 },
    { nome: 'descricao', rotulo: 'descrição', tipo: 'area' },
  ],
  valores: (p) => ({ nome: texto(p?.nome), tipo: p?.tipo ?? 'Empresarial', nif: texto(p?.nif), descricao: texto(p?.descricao) }),
  vazio: 'Ainda não há parceiros.',
};

function defAcordos(parceiros: Parceiro[]): DefTabela<Acordo> {
  return {
    titulo: 'acordos de parceria',
    singular: 'acordo',
    colunas: 'minmax(160px, 0.8fr) minmax(200px, 1fr) 150px 100px 100px 100px 70px',
    cabecalhos: ['Parceiro', 'Acordo', 'Protocolo', 'Início', 'Fim', 'Estado', 'Doc.'],
    celulas: (a) => [
      <span className="forte">{a.parceiro}</span>,
      a.descricao,
      a.tipoProtocolo ? nomeTipoProtocolo[a.tipoProtocolo] : a.protocoloEstabelecido ? 'sim' : '—',
      data(a.dataInicio),
      a.dataFim ? data(a.dataFim) : 'sem fim',
      <span className={`etiqueta ${a.estado === 'EmCurso' ? 'estado-aprovada' : 'estado-nao-aprovada'}`}>{nomeEstadoAcordo[a.estado]}</span>,
      <LinkExterno url={a.linkDocumento} />,
    ],
    campos: [
      {
        nome: 'parceiroId',
        rotulo: 'parceiro',
        tipo: 'opcoes',
        numerico: true,
        obrigatorio: true,
        opcoes: parceiros.map((p): [string, string] => [String(p.id), p.nome]),
        ajuda: parceiros.length === 0 ? 'Registe primeiro o parceiro na tabela de cima.' : undefined,
      },
      { nome: 'descricao', rotulo: 'descrição da parceria', tipo: 'area', obrigatorio: true, max: 2000 },
      { nome: 'protocoloEstabelecido', rotulo: 'há protocolo assinado', tipo: 'booleano' },
      { nome: 'tipoProtocolo', rotulo: 'tipo de protocolo', tipo: 'opcoes', opcoes: opcoes(nomeTipoProtocolo) },
      { nome: 'dataInicio', rotulo: 'início', tipo: 'data' },
      { nome: 'dataFim', rotulo: 'fim (vazio = sem fim)', tipo: 'data' },
      { nome: 'incluiGestaoPropriedadeIntelectual', rotulo: 'inclui gestão de propriedade intelectual', tipo: 'booleano' },
      { nome: 'estado', rotulo: 'estado', tipo: 'opcoes', opcoes: opcoes(nomeEstadoAcordo), obrigatorio: true },
      { nome: 'linkDocumento', rotulo: 'link do protocolo (OneDrive)', tipo: 'link', max: 2000 },
      { nome: 'observacoes', rotulo: 'observações', tipo: 'area' },
    ],
    valores: (a) => ({
      parceiroId: texto(a?.parceiroId ?? (parceiros.length === 1 ? parceiros[0].id : null)),
      descricao: texto(a?.descricao),
      protocoloEstabelecido: texto(a?.protocoloEstabelecido ?? false),
      tipoProtocolo: texto(a?.tipoProtocolo),
      dataInicio: texto(a?.dataInicio),
      dataFim: texto(a?.dataFim),
      incluiGestaoPropriedadeIntelectual: texto(a?.incluiGestaoPropriedadeIntelectual ?? false),
      estado: a?.estado ?? 'EmCurso',
      linkDocumento: texto(a?.linkDocumento),
      observacoes: texto(a?.observacoes),
    }),
    vazio: 'Ainda não há acordos de parceria.',
  };
}

export function Parcerias() {
  const parceiros = usePedido(() => api.parceiros(), []);
  const acordos = usePedido(() => api.acordos(), []);
  const recarregar = () => {
    parceiros.recarregar();
    acordos.recarregar();
  };

  return (
    <>
      <Cabecalho titulo="parcerias" corLampada="#3a6f9a" />
      <p className="cinzento-escuro charter-nota pagina-nota">Acordos de parceria (Mod.235). Os parceiros académicos e empresariais contam para os KPIs.</p>
      <EstadoPedido aCarregar={parceiros.aCarregar} erro={parceiros.erro ?? acordos.erro} vazio={parceiros.dados === undefined} />
      {parceiros.dados && acordos.dados && (
        <>
          <Registo tipo="acordos" def={defAcordos(parceiros.dados)} itens={acordos.dados} aoMudar={recarregar} />
          <Registo tipo="parceiros" def={defParceiros} itens={parceiros.dados} aoMudar={recarregar} />
        </>
      )}
    </>
  );
}

// ---------- Ativos intangíveis (Mod.236) ----------

function Validade({ a }: { a: Ativo }) {
  if (!a.dataValidade || a.diasParaValidade === null) return <span className="cinzento-escuro">—</span>;
  if (a.diasParaValidade < 0) return <span className="etiqueta estado-em-discussao">expirou {data(a.dataValidade)}</span>;
  if (a.diasParaValidade <= 90) return <span className="etiqueta estado-em-avaliacao">{`${data(a.dataValidade)} · ${a.diasParaValidade} dias`}</span>;
  return <span>{data(a.dataValidade)}</span>;
}

const defAtivos: DefTabela<Ativo> = {
  titulo: 'ativos intangíveis',
  singular: 'ativo intangível',
  colunas: '90px minmax(200px, 1fr) 100px 130px 160px 120px 110px',
  cabecalhos: ['Tipo', 'Designação', 'Entidade', 'N.º pedido', 'Validade', 'Situação', 'Custo'],
  celulas: (a) => [
    nomeTipoAtivo[a.tipo],
    <span className="forte">{a.designacao}</span>,
    a.entidadeRegisto,
    a.numeroPedido ?? '—',
    <Validade a={a} />,
    a.situacao ?? '—',
    euros(a.custo),
  ],
  campos: [
    { nome: 'tipo', rotulo: 'tipo', tipo: 'opcoes', opcoes: opcoes(nomeTipoAtivo), obrigatorio: true, ajuda: 'Patentes, designs e marcas contam para os KPIs 29 a 31.' },
    { nome: 'designacao', rotulo: 'designação', obrigatorio: true, max: 300 },
    { nome: 'entidadeRegisto', rotulo: 'entidade de registo (ex.: INPI, EUIPO, EPO)', obrigatorio: true, max: 100 },
    { nome: 'descricao', rotulo: 'descrição', tipo: 'area' },
    { nome: 'numeroPedido', rotulo: 'n.º do pedido' },
    { nome: 'numeroProcesso', rotulo: 'n.º do processo' },
    { nome: 'autores', rotulo: 'autores / inventores' },
    { nome: 'data', rotulo: 'data do registo nesta tabela', tipo: 'data' },
    { nome: 'dataRegisto', rotulo: 'data de registo', tipo: 'data' },
    {
      nome: 'dataValidade',
      rotulo: 'validade',
      tipo: 'data',
      ajuda: 'A equipa de inovação recebe um aviso 90 dias antes de terminar.',
    },
    { nome: 'ambito', rotulo: 'âmbito (ex.: nacional, europeu)' },
    { nome: 'situacao', rotulo: 'situação (ex.: pedido, concedida)' },
    { nome: 'custo', rotulo: 'custo (€)', tipo: 'numero' },
    { nome: 'perspetivaValor', rotulo: 'perspetiva de valor', tipo: 'area' },
    { nome: 'observacoes', rotulo: 'observações', tipo: 'area' },
  ],
  valores: (a) => ({
    tipo: a?.tipo ?? 'Marca',
    designacao: texto(a?.designacao),
    entidadeRegisto: texto(a?.entidadeRegisto),
    descricao: texto(a?.descricao),
    numeroPedido: texto(a?.numeroPedido),
    numeroProcesso: texto(a?.numeroProcesso),
    autores: texto(a?.autores),
    data: a ? a.data : hoje(),
    dataRegisto: texto(a?.dataRegisto),
    dataValidade: texto(a?.dataValidade),
    ambito: texto(a?.ambito),
    situacao: texto(a?.situacao),
    custo: texto(a?.custo),
    perspetivaValor: texto(a?.perspetivaValor),
    observacoes: texto(a?.observacoes),
  }),
  vazio: 'Ainda não há ativos intangíveis.',
};

export function AtivosIntangiveis() {
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.ativos(), []);
  return (
    <>
      <Cabecalho titulo="ativos intangíveis" corLampada="#3a6f9a" />
      <p className="cinzento-escuro charter-nota pagina-nota">Patentes, designs e marcas (Mod.236). As validades nos próximos 90 dias ficam a amarelo.</p>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && <Registo tipo="ativos" def={defAtivos} itens={dados} aoMudar={recarregar} semTitulo />}
    </>
  );
}

// ---------- Conhecimento tácito (Mod.238) ----------

const defTacito: DefTabela<ConhecimentoTacito> = {
  titulo: 'conhecimento tácito',
  singular: 'registo de conhecimento',
  colunas: '100px 160px minmax(260px, 1fr) 140px 110px',
  cabecalhos: ['Data', 'Detentor', 'Conhecimento', 'Empresa', 'Confidencial.'],
  celulas: (c) => [
    data(c.data),
    <span className="forte">{c.detentor}</span>,
    c.descricao,
    c.empresa ?? '—',
    <span className={`etiqueta ${c.confidencialidade === 'Elevado' ? 'estado-em-discussao' : 'estado-aprovada'}`}>
      {nomeConfidencialidade[c.confidencialidade]}
    </span>,
  ],
  campos: [
    { nome: 'detentor', rotulo: 'quem detém o conhecimento', obrigatorio: true, max: 200 },
    { nome: 'descricao', rotulo: 'conhecimento', tipo: 'area', obrigatorio: true, max: 4000 },
    { nome: 'empresa', rotulo: 'empresa', max: 200 },
    {
      nome: 'confidencialidade',
      rotulo: 'confidencialidade',
      tipo: 'opcoes',
      opcoes: opcoes(nomeConfidencialidade),
      obrigatorio: true,
      ajuda: 'Os registos de confidencialidade elevada só aparecem a quem gere os registos.',
    },
    { nome: 'data', rotulo: 'data', tipo: 'data' },
  ],
  valores: (c) => ({
    detentor: texto(c?.detentor),
    descricao: texto(c?.descricao),
    empresa: texto(c?.empresa),
    confidencialidade: c?.confidencialidade ?? 'Baixo',
    data: c ? c.data : hoje(),
  }),
  vazio: 'Ainda não há conhecimento tácito registado.',
};

export function ConhecimentoTacitoPagina() {
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.conhecimentoTacito(), []);
  return (
    <>
      <Cabecalho titulo="conhecimento tácito" corLampada="#3a6f9a" />
      <p className="cinzento-escuro charter-nota pagina-nota">Quem sabe o quê dentro da empresa (Mod.238), para não se perder quando as pessoas mudam.</p>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && <Registo tipo="conhecimento-tacito" def={defTacito} itens={dados} aoMudar={recarregar} semTitulo />}
    </>
  );
}

// ---------- Ferramentas e métodos (Mod.241) ----------

const defFerramentas: DefTabela<Ferramenta> = {
  titulo: 'ferramentas e métodos',
  singular: 'ferramenta ou método',
  feminino: true,
  colunas: '100px minmax(180px, 0.7fr) minmax(240px, 1fr) 120px',
  cabecalhos: ['Data', 'Ferramenta / método', 'Sumário', 'Custo'],
  celulas: (f) => [data(f.data), <span className="forte">{f.nome}</span>, f.sumarioExecutivo ?? '—', euros(f.custoAquisicao)],
  campos: [
    { nome: 'nome', rotulo: 'nome', obrigatorio: true, max: 200 },
    { nome: 'sumarioExecutivo', rotulo: 'sumário executivo', tipo: 'area' },
    { nome: 'potencial', rotulo: 'potencial para a empresa', tipo: 'area' },
    { nome: 'custoAquisicao', rotulo: 'custo de aquisição (€)', tipo: 'numero' },
    { nome: 'divulgacao', rotulo: 'divulgação (onde e a quem)', tipo: 'area' },
    { nome: 'data', rotulo: 'data', tipo: 'data' },
  ],
  valores: (f) => ({
    nome: texto(f?.nome),
    sumarioExecutivo: texto(f?.sumarioExecutivo),
    potencial: texto(f?.potencial),
    custoAquisicao: texto(f?.custoAquisicao),
    divulgacao: texto(f?.divulgacao),
    data: f ? f.data : hoje(),
  }),
  vazio: 'Ainda não há ferramentas nem métodos registados.',
};

export function Ferramentas() {
  const { dados, erro, aCarregar, recarregar } = usePedido(() => api.ferramentas(), []);
  return (
    <>
      <Cabecalho titulo="ferramentas e métodos" corLampada="#3a6f9a" />
      <p className="cinzento-escuro charter-nota pagina-nota">Portefólio de ferramentas e métodos de inovação (Mod.241).</p>
      <EstadoPedido aCarregar={aCarregar} erro={erro} vazio={dados === undefined} />
      {dados && <Registo tipo="ferramentas" def={defFerramentas} itens={dados} aoMudar={recarregar} semTitulo />}
    </>
  );
}
