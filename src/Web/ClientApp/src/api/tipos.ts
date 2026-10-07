// Tipos iguais aos DTOs do backend (src/Application/Ideias/IdeiaDtos.cs e Utilizadores).
// Os enums chegam como texto (JsonStringEnumConverter).

export type EstadoIdeia = 'ValidacaoAutores' | 'ValidacaoEquipa' | 'EmDiscussao' | 'EmAvaliacao' | 'Aprovada' | 'NaoAprovada';

export type TipoIdeia = 'Melhoria' | 'NovoProdutoServicoSimples' | 'NovoProdutoServicoDetalhado';

export type Responsabilidade = 'Dst' | 'Dstelecom';

export type ClassificacaoIdeia = 'EmAvaliacaoPeloManager' | 'Aprovada' | 'NaoAprovada';

export type ClasseIdeia = 'MelhoriaContinua' | 'Projeto' | 'Desafio' | 'IdeiaDuplicada' | 'Arquivada' | 'NaoAplicavel';

export type PerfilUtilizador = 'Trabalhador' | 'GestorProjeto' | 'Administrador';

export interface UtilizadorAtual {
  id: number;
  nome: string;
  email: string;
  empresa: string | null;
  perfil: PerfilUtilizador;
  eAdministrador: boolean;
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
}

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
