import { enviar, obter, pedido } from './cliente';
import type {
  Anexo,
  ClasseIdeia,
  EstadoIdeia,
  IdeiaCriada,
  IdeiaDetalhe,
  IdeiaResumo,
  DadosIniciativa,
  IniciativaDetalhe,
  IniciativaResumo,
  Notificacoes,
  Responsabilidade,
  ResultadoImportacao,
  TipoIniciativa,
  EstadoIniciativa,
  NotasPedido,
  NovaIdeiaPedido,
  Perfis,
  ResultadoAvaliacao,
  UtilizadorAdmin,
  UtilizadorAtual,
  UtilizadorResumo,
  CategoriaLicao,
  CharterGeral,
  ConhecimentoCodificado,
  LicaoRepositorio,
  OrigemConhecimento,
  ProjectCharter,
  RegistoAuditoria,
  SeccaoCharter,
  Acordo,
  AtividadePlano,
  Ativo,
  ConhecimentoTacito as ConhecimentoTacitoItem,
  Ferramenta,
  Parceiro,
  ValorExterno,
  Indicadores,
  PilarEstrategico,
} from './tipos';

// Contas (ASP.NET Core Identity com cookie). As contas são criadas pelo Administrador (convite por email).
export const contas = {
  eu: () => obter<UtilizadorAtual>('/api/Utilizadores/eu'),
  entrar: (email: string, password: string) => enviar('/api/Conta/entrar', { email, password }),
  sair: () => enviar('/api/Conta/sair'),
  aceitarConvite: (utilizadorId: string, token: string, password: string) =>
    enviar('/api/Conta/aceitar-convite', { utilizadorId, token, password }),
  recuperarPassword: (email: string) => enviar('/api/Conta/recuperar-password', { email }),
  redefinirPassword: (utilizadorId: string, token: string, password: string) =>
    enviar('/api/Conta/redefinir-password', { utilizadorId, token, password }),
  pesquisar: (texto: string) => obter<UtilizadorResumo[]>(`/api/Utilizadores?texto=${encodeURIComponent(texto)}`),
};

// Backoffice (permissões utilizadores.gerir e perfis.gerir).
const admin = '/api/Administracao';

export interface DadosUtilizador {
  nome: string;
  empresa: string | null;
  perfis: string[];
}

export const administracao = {
  utilizadores: () => obter<UtilizadorAdmin[]>(`${admin}/utilizadores`),
  adicionar: (dados: DadosUtilizador & { email: string }) => enviar<number>(`${admin}/utilizadores`, dados),
  editar: (id: number, dados: DadosUtilizador) => pedido<void>('PUT', `${admin}/utilizadores/${id}`, dados),
  reenviarConvite: (id: number) => enviar(`${admin}/utilizadores/${id}/reenviar-convite`),
  desativar: (id: number) => enviar(`${admin}/utilizadores/${id}/desativar`),
  ativar: (id: number) => enviar(`${admin}/utilizadores/${id}/ativar`),
  perfis: () => obter<Perfis>(`${admin}/perfis`),
  criarPerfil: (nome: string) => enviar(`${admin}/perfis`, { nome }),
  definirPermissoes: (perfil: string, permissoes: string[]) =>
    pedido<void>('PUT', `${admin}/perfis/${encodeURIComponent(perfil)}/permissoes`, { permissoes }),
  apagarPerfil: (perfil: string) => pedido<void>('DELETE', `${admin}/perfis/${encodeURIComponent(perfil)}`),
};

const base = '/api/Ideias';

export interface FiltroIdeias {
  estado?: EstadoIdeia;
  soMinhas?: boolean;
  soDecididas?: boolean;
  texto?: string;
  classe?: ClasseIdeia;
  responsabilidade?: Responsabilidade;
  ano?: number;
}

export const ideias = {
  listar: (filtro: FiltroIdeias = {}) => {
    const p = new URLSearchParams();
    if (filtro.estado) p.set('estado', filtro.estado);
    if (filtro.soMinhas) p.set('soMinhas', 'true');
    if (filtro.soDecididas) p.set('soDecididas', 'true');
    if (filtro.texto?.trim()) p.set('texto', filtro.texto.trim());
    if (filtro.classe) p.set('classe', filtro.classe);
    if (filtro.responsabilidade) p.set('responsabilidade', filtro.responsabilidade);
    if (filtro.ano) p.set('ano', String(filtro.ano));
    const q = p.toString();
    return obter<IdeiaResumo[]>(q ? `${base}?${q}` : base);
  },
  obter: (id: number) => obter<IdeiaDetalhe>(`${base}/${id}`),
  criar: (dados: NovaIdeiaPedido) => enviar<IdeiaCriada>(base, dados),
  confirmarAutoria: (id: number) => enviar(`${base}/${id}/confirmar-autoria`),
  validar: (id: number) => enviar(`${base}/${id}/validar`),
  fecharDiscussao: (id: number) => enviar(`${base}/${id}/fechar-discussao`),
  alternarGosto: (id: number) => enviar<boolean>(`${base}/${id}/gosto`),
  comentar: (id: number, texto: string) => enviar(`${base}/${id}/comentarios`, { texto }),
  alternarGostoComentario: (comentarioId: number) => enviar<boolean>(`${base}/comentarios/${comentarioId}/gosto`),
  registarAvaliacao: (id: number, notas: NotasPedido) => enviar<ResultadoAvaliacao>(`${base}/${id}/avaliacao`, notas),
  registarDecisaoDst: (id: number, aprovada: boolean) => enviar(`${base}/${id}/decisao-dst`, { aprovada }),
  definirClasse: (id: number, classe: ClasseIdeia, status: string | null = null) =>
    enviar<number | null>(`${base}/${id}/classe`, { classe, status }),
  anexar: (id: number, ficheiro: File) => {
    const dados = new FormData();
    dados.append('ficheiro', ficheiro);
    return enviar<Anexo>(`${base}/${id}/anexos`, dados);
  },
  urlAnexo: (anexoId: number) => `${base}/anexos/${anexoId}`,
};

export const notificacoes = {
  listar: () => obter<Notificacoes>('/api/Notificacoes'),
  marcarLida: (id: number) => enviar(`/api/Notificacoes/${id}/lida`),
  marcarTodas: () => enviar('/api/Notificacoes/lidas'),
};

const baseIniciativas = '/api/Iniciativas';

export const iniciativas = {
  listar: (filtro: { tipo?: TipoIniciativa; estado?: EstadoIniciativa; texto?: string; pilar?: PilarEstrategico; ano?: number } = {}) => {
    const p = new URLSearchParams();
    if (filtro.tipo) p.set('tipo', filtro.tipo);
    if (filtro.estado) p.set('estado', filtro.estado);
    if (filtro.pilar) p.set('pilar', filtro.pilar);
    if (filtro.ano) p.set('ano', String(filtro.ano));
    if (filtro.texto?.trim()) p.set('texto', filtro.texto.trim());
    const q = p.toString();
    return obter<IniciativaResumo[]>(q ? `${baseIniciativas}?${q}` : baseIniciativas);
  },
  obter: (id: number) => obter<IniciativaDetalhe>(`${baseIniciativas}/${id}`),
  criar: (tipo: TipoIniciativa, dados: DadosIniciativa) => enviar<number>(baseIniciativas, { tipo, dados }),
  atualizar: (id: number, dados: DadosIniciativa) => pedido<void>('PUT', `${baseIniciativas}/${id}`, dados),
  pontoSituacao: (id: number, texto: string) => enviar(`${baseIniciativas}/${id}/ponto-situacao`, { texto }),
  importarMod246: (ficheiro: File) => {
    const dados = new FormData();
    dados.append('ficheiro', ficheiro);
    return enviar<ResultadoImportacao>(`${baseIniciativas}/importar-mod246`, dados);
  },
  adicionarLink: (id: number, titulo: string, url: string) => enviar<number>(`${baseIniciativas}/${id}/links`, { titulo, url }),
  removerLink: (id: number, linkId: number) => pedido<void>('DELETE', `${baseIniciativas}/${id}/links/${linkId}`),
};

/** Project Charter: um GET com os 6 separadores; cada linha grava-se sozinha. */
export const charter = {
  obter: (id: number) => obter<ProjectCharter>(`${baseIniciativas}/${id}/charter`),
  guardarGeral: (id: number, dados: CharterGeral) => pedido<void>('PUT', `${baseIniciativas}/${id}/charter/geral`, dados),
  guardarItem: (id: number, seccao: SeccaoCharter, itemId: number | null, dados: unknown) =>
    itemId === null
      ? enviar<number>(`${baseIniciativas}/${id}/charter/${seccao}`, dados)
      : pedido<number>('PUT', `${baseIniciativas}/${id}/charter/${seccao}/${itemId}`, dados),
  apagarItem: (id: number, seccao: SeccaoCharter, itemId: number) =>
    pedido<void>('DELETE', `${baseIniciativas}/${id}/charter/${seccao}/${itemId}`),
  guardarMembro: (id: number, membroId: number | null, dados: { utilizadorId: number | null; nome: string | null; funcao: string | null }) =>
    membroId === null
      ? enviar<number>(`${baseIniciativas}/${id}/equipa`, dados)
      : pedido<number>('PUT', `${baseIniciativas}/${id}/equipa/${membroId}`, dados),
  apagarMembro: (id: number, membroId: number) => pedido<void>('DELETE', `${baseIniciativas}/${id}/equipa/${membroId}`),
  guardarAlocacoes: (id: number, membroId: number, ano: number, meses: (number | null)[]) =>
    pedido<void>('PUT', `${baseIniciativas}/${id}/equipa/${membroId}/alocacoes/${ano}`, { meses }),
};

function consulta(base: string, filtros: Record<string, string | undefined>): string {
  const p = new URLSearchParams();
  for (const [chave, valor] of Object.entries(filtros)) {
    if (valor?.trim()) p.set(chave, valor.trim());
  }
  const q = p.toString();
  return q ? `${base}?${q}` : base;
}

/** Repositórios de conhecimento: lições aprendidas (Mod.252) e conhecimento codificado (Mod.237). */
export const conhecimento = {
  licoes: (filtro: { categoria?: CategoriaLicao; texto?: string } = {}) =>
    obter<LicaoRepositorio[]>(consulta('/api/Conhecimento/licoes', filtro)),
  codificado: (filtro: { origem?: OrigemConhecimento; texto?: string } = {}) =>
    obter<ConhecimentoCodificado[]>(consulta('/api/Conhecimento/codificado', filtro)),
};

export const auditoria = {
  listar: (filtro: { entidade?: string; entidadeId?: string; texto?: string } = {}) =>
    obter<RegistoAuditoria[]>(consulta('/api/Administracao/auditoria', filtro)),
};

/** Registos manuais (RF14): parcerias, ativos intangíveis, conhecimento tácito e ferramentas. */
export type TipoRegistoApi = 'parceiros' | 'acordos' | 'ativos' | 'conhecimento-tacito' | 'ferramentas';

const baseRegistos = '/api/Registos';

export const registos = {
  parceiros: () => obter<Parceiro[]>(`${baseRegistos}/parceiros`),
  acordos: (texto?: string) => obter<Acordo[]>(consulta(`${baseRegistos}/acordos`, { texto })),
  ativos: () => obter<Ativo[]>(`${baseRegistos}/ativos`),
  conhecimentoTacito: () => obter<ConhecimentoTacitoItem[]>(`${baseRegistos}/conhecimento-tacito`),
  ferramentas: () => obter<Ferramenta[]>(`${baseRegistos}/ferramentas`),
  guardar: (tipo: TipoRegistoApi, id: number | null, dados: unknown) =>
    id === null ? enviar<number>(`${baseRegistos}/${tipo}`, dados) : pedido<number>('PUT', `${baseRegistos}/${tipo}/${id}`, dados),
  apagar: (tipo: TipoRegistoApi, id: number) => pedido<void>('DELETE', `${baseRegistos}/${tipo}/${id}`),
};

export const planoAnual = {
  listar: (ano: number) => obter<AtividadePlano[]>(`/api/PlanoAnual?ano=${ano}`),
  guardar: (id: number | null, dados: unknown) =>
    id === null ? enviar<number>('/api/PlanoAnual', dados) : pedido<number>('PUT', `/api/PlanoAnual/${id}`, dados),
  apagar: (id: number) => pedido<void>('DELETE', `/api/PlanoAnual/${id}`),
};

export const kpis = {
  indicadores: (ano: number) => obter<Indicadores>(`/api/Kpis/indicadores?ano=${ano}`),
  valoresExternos: () => obter<ValorExterno[]>('/api/Kpis/valores-externos'),
  guardarValorExterno: (id: number | null, dados: unknown) =>
    id === null ? enviar<number>('/api/Kpis/valores-externos', dados) : pedido<number>('PUT', `/api/Kpis/valores-externos/${id}`, dados),
  apagarValorExterno: (id: number) => pedido<void>('DELETE', `/api/Kpis/valores-externos/${id}`),
};
