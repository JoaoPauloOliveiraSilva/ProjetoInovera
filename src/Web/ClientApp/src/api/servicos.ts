import { enviar, obter, pedido } from './cliente';
import type {
  Anexo,
  ClasseIdeia,
  EstadoIdeia,
  IdeiaCriada,
  IdeiaDetalhe,
  IdeiaResumo,
  NotasPedido,
  NovaIdeiaPedido,
  Perfis,
  ResultadoAvaliacao,
  UtilizadorAdmin,
  UtilizadorAtual,
  UtilizadorResumo,
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
