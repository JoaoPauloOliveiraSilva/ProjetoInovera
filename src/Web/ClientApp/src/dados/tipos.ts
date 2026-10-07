export type EstadoIdeia =
  | 'validacao-autores'
  | 'validacao-equipa'
  | 'em-discussao'
  | 'em-avaliacao'
  | 'aprovada'
  | 'nao-aprovada';

export type TipoIdeia = 'melhoria' | 'simples' | 'detalhado';

export type Responsabilidade = 'dst' | 'dstelecom';

export type ClasseIdeia =
  | 'melhoria-continua'
  | 'projeto'
  | 'desafio'
  | 'duplicada'
  | 'arquivada'
  | 'na';

/** Nota de 0 a 4; null = N.A. */
export type Nota = number | null;

export interface Notas {
  custo: Nota;
  enquadramento: Nota;
  beneficio: Nota;
  adequacao: Nota;
  incerteza: Nota;
}

export interface Comentario {
  id: number;
  autor: string;
  empresa: string;
  texto: string;
  data: string;
  gostos: number;
  gostei: boolean;
}

export interface Ideia {
  num: number;
  titulo: string;
  tipo: TipoIdeia;
  descricao: string;
  autores: string[];
  anonima: boolean;
  privada: boolean;
  data: string;
  estado: EstadoIdeia;
  likes: number;
  gostei: boolean;
  comentarios: Comentario[];
  anexos: string[];
  vantagens?: string;
  requisitos?: string;
  comoEFeito?: string;
  modeloNegocio?: string;
  competidores?: string;
  custos?: string;
  responsabilidade?: Responsabilidade;
  notas?: Notas;
  classe?: ClasseIdeia;
}

export const nomeTipo: Record<TipoIdeia, string> = {
  melhoria: 'melhoria',
  simples: 'novo produto/serviço simples',
  detalhado: 'novo produto/serviço detalhado',
};

export const nomeEstado: Record<EstadoIdeia, string> = {
  'validacao-autores': 'validação dos autores',
  'validacao-equipa': 'validação equipa inovação',
  'em-discussao': 'em discussão',
  'em-avaliacao': 'em avaliação pelo manager',
  aprovada: 'aprovada',
  'nao-aprovada': 'não aprovada',
};

export const nomeClasse: Record<ClasseIdeia, string> = {
  'melhoria-continua': 'Melhoria Contínua',
  projeto: 'Projeto',
  desafio: 'Desafio',
  duplicada: 'Ideia Duplicada',
  arquivada: 'Arquivada',
  na: 'N.A.',
};

export const classesAprovada: ClasseIdeia[] = ['melhoria-continua', 'projeto', 'desafio', 'arquivada'];
export const classesNaoAprovada: ClasseIdeia[] = ['duplicada', 'na'];
