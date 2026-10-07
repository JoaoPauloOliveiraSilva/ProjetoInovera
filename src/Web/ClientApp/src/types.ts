export type PageId = 'home' | 'projects' | 'ocienegocio' | 'profile' | 'help';

export interface IdeaComment {
  author: string;
  company: string;
  text: string;
  time: string;
}

export interface Idea {
  num: string;
  titulo: string;
  author: string;
  date: string;
  likes: number;
  comments: IdeaComment[];
  description: string;
  kind: string;
  status: string;
  isPrivate: boolean;
}

export interface NewIdea {
  titulo: string;
  descricao: string;
  tipo: string;
  autor: 'current' | 'anon' | 'other';
  visivel: boolean;
}
