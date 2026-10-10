// Tipos iguais aos DTOs do backend (src/Application/Ideias/IdeiaDtos.cs e Utilizadores).
// Os enums chegam como texto (JsonStringEnumConverter).

export type EstadoIdeia = 'ValidacaoAutores' | 'ValidacaoEquipa' | 'EmDiscussao' | 'EmAvaliacao' | 'Aprovada' | 'NaoAprovada';

export type TipoIdeia = 'Melhoria' | 'NovoProdutoServicoSimples' | 'NovoProdutoServicoDetalhado';

export type Responsabilidade = 'Dst' | 'Dstelecom';

export type ClassificacaoIdeia = 'EmAvaliacaoPeloManager' | 'Aprovada' | 'NaoAprovada';

export type ClasseIdeia = 'MelhoriaContinua' | 'Projeto' | 'Desafio' | 'IdeiaDuplicada' | 'Arquivada' | 'NaoAplicavel';

export interface UtilizadorAtual {
  id: number;
  nome: string;
  email: string;
  empresa: string | null;
  perfis: string[];
  /** Códigos das permissões (ver Permissoes em api/permissoes.ts). */
  permissoes: string[];
}

// Backoffice

export type EstadoConta = 'ConvitePendente' | 'Ativa' | 'Desativada';

export interface UtilizadorAdmin {
  id: number;
  nome: string;
  email: string;
  empresa: string | null;
  perfis: string[];
  estado: EstadoConta;
}

export interface Perfil {
  nome: string;
  doSistema: boolean;
  todasAsPermissoes: boolean;
  permissoes: string[];
  utilizadores: number;
}

export interface PermissaoInfo {
  codigo: string;
  nome: string;
  grupo: string;
}

export interface Perfis {
  perfis: Perfil[];
  permissoes: PermissaoInfo[];
}

export const nomeEstadoConta: Record<EstadoConta, string> = {
  ConvitePendente: 'convite enviado',
  Ativa: 'ativa',
  Desativada: 'desativada',
};

/** Nome do perfil no ecrã (os perfis novos aparecem com o nome que lhes foi dado). */
export function nomePerfil(nome: string): string {
  return { Administrador: 'Administrador', EquipaInovacao: 'Equipa de Inovação', Trabalhador: 'Trabalhador' }[nome] ?? nome;
}

export interface UtilizadorResumo {
  id: number;
  nome: string;
  empresa: string | null;
}

export interface IdeiaResumo {
  id: number;
  codigo: string;
  titulo: string;
  tipo: TipoIdeia;
  autores: string;
  data: string;
  estado: EstadoIdeia;
  privada: boolean;
  likes: number;
  comentarios: number;
  responsabilidade: Responsabilidade;
  classificacao: ClassificacaoIdeia | null;
  classe: ClasseIdeia | null;
  nota: number | null;
  /** N.º da Caixa de Inovação (ideias importadas do Mod.246). */
  idCaixaInovacao: number | null;
  /** O utilizador atual é coautor e ainda não confirmou. */
  porConfirmar: boolean;
}

export interface Coautor {
  utilizadorId: number;
  nome: string;
  confirmado: boolean;
}

export interface Anexo {
  id: number;
  nomeFicheiro: string;
  tamanhoBytes: number;
}

export interface Comentario {
  id: number;
  autor: string;
  empresa: string | null;
  texto: string;
  data: string;
  gostos: number;
  gostei: boolean;
}

export interface Avaliacao {
  custo: number | null;
  enquadramento: number | null;
  beneficio: number | null;
  adequacaoTecnica: number | null;
  incerteza: number | null;
  nota: number | null;
  aprovada: boolean | null;
  observacoes: string | null;
}

export interface IdeiaDetalhe {
  id: number;
  codigo: string;
  titulo: string;
  tipo: TipoIdeia;
  descricao: string | null;
  vantagens: string | null;
  requisitos: string | null;
  comoEFeitoAtualmente: string | null;
  modeloNegocio: string | null;
  competidores: string | null;
  custos: string | null;
  privada: boolean;
  anonima: boolean;
  autor: string | null;
  autores: string;
  coautores: Coautor[];
  data: string;
  estado: EstadoIdeia;
  fimDiscussao: string | null;
  responsabilidade: Responsabilidade;
  classificacao: ClassificacaoIdeia | null;
  classe: ClasseIdeia | null;
  status: string | null;
  likes: number;
  gostei: boolean;
  anexos: Anexo[];
  comentarios: Comentario[];
  avaliacao: Avaliacao | null;
  podeConfirmarAutoria: boolean;
  podeAnexar: boolean;
  iniciativaId: number | null;
  iniciativaCodigo: string | null;
  idCaixaInovacao: number | null;
  origem: string | null;
}

// Notificações (sino)

export interface Notificacao {
  id: number;
  titulo: string;
  mensagem: string;
  link: string | null;
  data: string;
  lida: boolean;
}

export interface Notificacoes {
  naoLidas: number;
  itens: Notificacao[];
}

// Iniciativas (portefólio, Mod.246)

export type TipoIniciativa = 'Projeto' | 'Desafio' | 'MelhoriaContinua' | 'Oportunidade' | 'Vigilancia' | 'Mestrado' | 'AcaoInovacao';

export type EstadoIniciativa = 'Pendente' | 'EmAnalise' | 'EmCurso' | 'EmImplementacao' | 'Concluida' | 'Rejeitada' | 'Cancelada';

export type Horizonte = 'H1' | 'H2' | 'H3';

export interface IniciativaResumo {
  id: number;
  codigo: string;
  tipo: TipoIniciativa;
  titulo: string;
  estado: EstadoIniciativa;
  responsavel: string | null;
  dataInicio: string | null;
  dataFim: string | null;
  origem: string | null;
  pontoSituacao: string | null;
  ideiaOrigemId: number | null;
  ideiaOrigemCodigo: string | null;
}

export interface IniciativaDetalhe {
  resumo: IniciativaResumo;
  descricao: string | null;
  horizonte: Horizonte | null;
  detalhes: { nome: string; valor: string }[];
  historico: { data: string; texto: string; autor: string | null }[];
  podeEditar: boolean;
}

export interface DadosIniciativa {
  titulo: string;
  descricao: string | null;
  estado: EstadoIniciativa;
  responsavel: string | null;
  dataInicio: string | null;
  dataFim: string | null;
  origem: string | null;
  horizonte: Horizonte | null;
}

export interface ResultadoImportacao {
  folhas: { folha: string; importadas: number; jaExistiam: number; ignoradas: number }[];
  avisos: string[];
}

export const nomeTipoIniciativa: Record<TipoIniciativa, string> = {
  Projeto: 'projeto',
  Desafio: 'desafio',
  MelhoriaContinua: 'melhoria contínua',
  Oportunidade: 'oportunidade',
  Vigilancia: 'vigilância',
  Mestrado: 'mestrado',
  AcaoInovacao: 'ação de inovação',
};

export const nomeEstadoIniciativa: Record<EstadoIniciativa, string> = {
  Pendente: 'pendente',
  EmAnalise: 'em análise',
  EmCurso: 'em curso',
  EmImplementacao: 'em implementação',
  Concluida: 'concluída',
  Rejeitada: 'rejeitada',
  Cancelada: 'cancelada',
};

export const classeEstadoIniciativa: Record<EstadoIniciativa, string> = {
  Pendente: 'estado-validacao-equipa',
  EmAnalise: 'estado-em-avaliacao',
  EmCurso: 'estado-em-discussao',
  EmImplementacao: 'estado-em-discussao',
  Concluida: 'estado-aprovada',
  Rejeitada: 'estado-nao-aprovada',
  Cancelada: 'estado-nao-aprovada',
};

export interface NovaIdeiaPedido {
  tipo: TipoIdeia;
  titulo: string;
  descricao: string;
  privada: boolean;
  anonima: boolean;
  coautoresIds: number[];
  vantagens: string | null;
  requisitos: string | null;
  comoEFeitoAtualmente: string | null;
  modeloNegocio: string | null;
  competidores: string | null;
  custos: string | null;
  termosAceites: boolean;
}

export interface IdeiaCriada {
  id: number;
  codigo: string;
  estado: EstadoIdeia;
}

export interface NotasPedido {
  custo: number;
  enquadramento: number;
  beneficio: number;
  adequacaoTecnica: number;
  incerteza: number;
  dataReuniaoCE?: string | null;
  observacoes?: string | null;
}

export interface ResultadoAvaliacao {
  nota: number | null;
  estado: EstadoIdeia;
  classificacao: ClassificacaoIdeia | null;
}

// Nomes mostrados no ecrã (iguais ao site atual).

export const nomeTipo: Record<TipoIdeia, string> = {
  Melhoria: 'melhoria',
  NovoProdutoServicoSimples: 'novo produto/serviço simples',
  NovoProdutoServicoDetalhado: 'novo produto/serviço detalhado',
};

export const nomeEstado: Record<EstadoIdeia, string> = {
  ValidacaoAutores: 'validação dos autores',
  ValidacaoEquipa: 'validação equipa inovação',
  EmDiscussao: 'em discussão',
  EmAvaliacao: 'em avaliação pelo manager',
  Aprovada: 'aprovada',
  NaoAprovada: 'não aprovada',
};

export const nomeClasse: Record<ClasseIdeia, string> = {
  MelhoriaContinua: 'Melhoria Contínua',
  Projeto: 'Projeto',
  Desafio: 'Desafio',
  IdeiaDuplicada: 'Ideia Duplicada',
  Arquivada: 'Arquivada',
  NaoAplicavel: 'N.A.',
};

export const classesAprovada: ClasseIdeia[] = ['MelhoriaContinua', 'Projeto', 'Desafio', 'Arquivada'];
export const classesNaoAprovada: ClasseIdeia[] = ['IdeiaDuplicada', 'NaoAplicavel'];

/** Classe CSS do estado (as etiquetas coloridas do site atual). */
export const classeEstado: Record<EstadoIdeia, string> = {
  ValidacaoAutores: 'estado-validacao-autores',
  ValidacaoEquipa: 'estado-validacao-equipa',
  EmDiscussao: 'estado-em-discussao',
  EmAvaliacao: 'estado-em-avaliacao',
  Aprovada: 'estado-aprovada',
  NaoAprovada: 'estado-nao-aprovada',
};
