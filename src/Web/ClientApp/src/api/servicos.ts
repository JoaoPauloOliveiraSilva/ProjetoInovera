import { enviar, obter } from './cliente';
import type {
  Anexo,
  ClasseIdeia,
  EstadoIdeia,
  IdeiaCriada,
  IdeiaDetalhe,
  IdeiaResumo,
  NotasPedido,
  NovaIdeiaPedido,
  ResultadoAvaliacao,
  UtilizadorAtual,
  UtilizadorResumo,
} from './tipos';

// Contas (ASP.NET Core Identity com cookie).
export const contas = {
  eu: () => obter<UtilizadorAtual>('/api/Utilizadores/eu'),
  entrar: (email: string, password: string) =>
    enviar('/api/Users/login?useCookies=true', { email, password }),
  sair: () => enviar('/api/Users/logout', {}),
  registar: (dados: { nome: string; email: string; password: string; empresa: string | null }) =>
    enviar<number>('/api/Utilizadores/registo', dados),
  pesquisar: (texto: string) => obter<UtilizadorResumo[]>(`/api/Utilizadores?texto=${encodeURIComponent(texto)}`),
};

const base = '/api/Ideias';

export const ideias = {
  listar: (filtro: { estado?: EstadoIdeia; soMinhas?: boolean } = {}) => {
    const p = new URLSearchParams();
    if (filtro.estado) p.set('estado', filtro.estado);
    if (filtro.soMinhas) p.set('soMinhas', 'true');
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
