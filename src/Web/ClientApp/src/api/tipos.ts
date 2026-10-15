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

/** Ideia com título parecido com o de uma ideia nova (aviso de ideia repetida). */
export interface IdeiaParecida {
  id: number;
  codigo: string;
  titulo: string;
  estado: EstadoIdeia;
  classe: ClasseIdeia | null;
  /** 0 a 100. */
  semelhanca: number;
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

/** Pilares estratégicos da dstelecom (KPIs 35–37). */
export type PilarEstrategico = 'Pilar1' | 'Pilar2' | 'Pilar3';

export const nomePilar: Record<PilarEstrategico, string> = {
  Pilar1: 'Pilar 1 · Modelo de Negócio Core',
  Pilar2: 'Pilar 2 · Exportação do Modelo de Negócio',
  Pilar3: 'Pilar 3 · Ambiente e Prosperidade do Planeta',
};

/** Linhas de cada pilar (como no documento de estratégia da dstelecom). */
export const areasPilar: Record<PilarEstrategico, string[]> = {
  Pilar1: ['Inovação Tecnológica', 'Digitalização de Processos'],
  Pilar2: ['Neutral Hosting para Rede Móvel', 'Infraestrutura Aeroespacial', 'Tecnologia para Cabos Submarinos Sensorizados', 'Data Centers Avançados'],
  Pilar3: ['Eficiência Energética', 'Sustentabilidade e Economia Circular'],
};

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
  pilar: PilarEstrategico | null;
}

export interface IniciativaDetalhe {
  resumo: IniciativaResumo;
  descricao: string | null;
  horizonte: Horizonte | null;
  detalhes: { nome: string; valor: string }[];
  historico: { data: string; texto: string; autor: string | null }[];
  links: LinkDocumento[];
  temCharter: boolean;
  campos: CamposTipo;
  podeEditar: boolean;
  parceiros: ParceiroLigado[];
}

export interface ParceiroLigado {
  id: number;
  nome: string;
  tipo: TipoParceiro;
}

/** Execução de uma vigilância: data prevista e realizada (KPI 24); o resultado entra no conhecimento codificado (Mod.237). */
export interface ExecucaoVigilancia {
  id: number;
  dataPrevista: string;
  dataRealizada: string | null;
  resultado: string | null;
  linkResultado: string | null;
  noPrazo: boolean;
  atrasada: boolean;
  conhecimentoId: number | null;
  /** Relatório carregado na plataforma (descarrega-se pelo linkResultado). */
  nomeFicheiro: string | null;
  tamanhoBytes: number | null;
}

/** Uma vigilância na página das vigilâncias, com os relatórios. */
export interface VigilanciaPainel {
  id: number;
  codigo: string;
  titulo: string;
  estado: EstadoIniciativa;
  responsavel: string | null;
  descritivo: string | null;
  periodicidade: Periodicidade;
  dataInicio: string | null;
  proximaExecucao: string | null;
  ativa: boolean;
  execucoes: ExecucaoVigilancia[];
}

/** Modelo de gestão/partilha (Atividades · Quem · Quando) dos Mod.236, 237 e 238. */
export interface AtividadeGestao {
  descricao: string;
  responsavel: string | null;
  data: string | null;
  concluida: boolean;
}

/** Coluna "Análise <ano>" do Mod.241. */
export interface AnaliseAnual {
  ano: number;
  analise: string;
}

export type TipoProjeto = 'Investigacao' | 'DesenvolvimentoInovacao' | 'Inovacao' | 'InvestigacaoDesenvolvimento' | 'InvestigacaoDesenvolvimentoInovacao';
export type AmbitoProjeto = 'Interno' | 'Externo';
export type Periodicidade = 'Nenhuma' | 'Semanal' | 'Mensal' | 'Trimestral' | 'Semestral' | 'Anual';
export type TipoAcaoInovacao = 'Criada' | 'Externa';

/** Campos próprios de cada tipo de iniciativa. */
export interface CamposTipo {
  tipoProjeto: TipoProjeto | null;
  ambito: AmbitoProjeto | null;
  resultado: string | null;
  descritivo: string | null;
  periodicidade: Periodicidade | null;
  nomeAluno: string | null;
  curso: string | null;
  tipoAcao: TipoAcaoInovacao | null;
  numeroParticipantes: number | null;
}

export const nomeTipoProjeto: Record<TipoProjeto, string> = {
  Investigacao: 'investigação',
  DesenvolvimentoInovacao: 'desenvolvimento e inovação',
  Inovacao: 'inovação',
  InvestigacaoDesenvolvimento: 'investigação e desenvolvimento',
  InvestigacaoDesenvolvimentoInovacao: 'investigação, desenvolvimento e inovação',
};

export const nomePeriodicidade: Record<Periodicidade, string> = {
  Nenhuma: 'não se repete',
  Semanal: 'semanal',
  Mensal: 'mensal',
  Trimestral: 'trimestral',
  Semestral: 'semestral',
  Anual: 'anual',
};

/** Link para um documento no OneDrive (RF15). */
export interface LinkDocumento {
  id: number;
  titulo: string;
  url: string;
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
  pilar: PilarEstrategico | null;
  campos?: CamposTipo | null;
}

export interface ResultadoImportacao {
  /** Modelo reconhecido (ex.: "Mod.237 · Conhecimento codificado"); não vem na importação antiga do Mod.246. */
  modelo?: string;
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

/** Estados das ideias com os nomes que a dstelecom usa (reunião de 13/10). */
export const nomeEstado: Record<EstadoIdeia, string> = {
  ValidacaoAutores: 'validação dos autores',
  ValidacaoEquipa: 'validação da equipa de inovação',
  EmDiscussao: 'em discussão pública',
  EmAvaliacao: 'em análise pela equipa',
  Aprovada: 'aprovada',
  NaoAprovada: 'rejeitada',
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

/**
 * Estado visível de uma ideia: depois de aprovada, a classe diz se foi "passada a projeto" (ou a desafio) ou "arquivada".
 */
export function estadoIdeia(i: { estado: EstadoIdeia; classe: ClasseIdeia | null }): { nome: string; classe: string } {
  if (i.estado === 'Aprovada' && (i.classe === 'Projeto' || i.classe === 'Desafio')) {
    return { nome: i.classe === 'Projeto' ? 'passada a projeto' : 'passada a desafio', classe: 'estado-passada' };
  }
  if (i.classe === 'Arquivada' || i.classe === 'IdeiaDuplicada') {
    return { nome: 'arquivada', classe: 'estado-arquivada' };
  }
  return { nome: nomeEstado[i.estado], classe: classeEstado[i.estado] };
}

/** Classe CSS do estado (as etiquetas coloridas do site atual). */
export const classeEstado: Record<EstadoIdeia, string> = {
  ValidacaoAutores: 'estado-validacao-autores',
  ValidacaoEquipa: 'estado-validacao-equipa',
  EmDiscussao: 'estado-em-discussao',
  EmAvaliacao: 'estado-em-avaliacao',
  Aprovada: 'estado-aprovada',
  NaoAprovada: 'estado-nao-aprovada',
};

// Project Charter (projetos e desafios)

export type TipoTarefa = 'Analise' | 'Gestao' | 'Comunicacao' | 'Tecnica' | 'Dados' | 'Testes';
export type EstadoTrabalho = 'Planeado' | 'EmCurso' | 'Concluido' | 'Atrasado' | 'Cancelado';
export type TipoEntrega = 'Entregavel' | 'Milestone';
export type NivelImpacto = 'Baixo' | 'Medio' | 'Alto' | 'Extremo';
export type NivelProbabilidade = 'Raro' | 'Improvavel' | 'PoucoProvavel' | 'MuitoProvavel';
export type RespostaRisco = 'Evitar' | 'Mitigar' | 'Transferir' | 'Aceitar';
export type TipoRequisito = 'Legal' | 'Qualidade' | 'Ambiente' | 'Seguranca';
export type EstadoRequisito = 'Pendente' | 'EmCurso' | 'Cumprido' | 'NaoAplicavel';
export type CategoriaLicao = 'Estrategica' | 'Gestao' | 'Operacional' | 'Tecnica';
export type OrigemConhecimento = 'Manual' | 'RelatorioIniciativa' | 'Vigilancia';
export type AcaoAuditoria = 'Criar' | 'Alterar' | 'Eliminar';

export interface CharterGeral {
  supervisorNome: string | null;
  fase: string | null;
  classe: string | null;
  contextoJustificacao: string | null;
  businessCase: string | null;
  decisaoAprovacao: string | null;
  objetivos: string | null;
  resultadosEsperados: string | null;
}

export interface Tarefa {
  id: number;
  codigoWbs: string;
  nome: string;
  tipo: TipoTarefa;
  estado: EstadoTrabalho;
  inicioPlaneado: string | null;
  fimPlaneado: string | null;
  inicioReal: string | null;
  fimReal: string | null;
  responsavel: string | null;
  milestoneId: number | null;
  atrasada: boolean;
}

export interface Entrega {
  id: number;
  tipo: TipoEntrega;
  codigo: string;
  descricao: string;
  prazo: string | null;
  estado: EstadoTrabalho;
  meioVerificacao: string | null;
  controlo: string | null;
  dataConclusao: string | null;
  linkRelatorio: string | null;
}

export interface Risco {
  id: number;
  numero: number;
  data: string;
  descricao: string;
  impacto: NivelImpacto;
  probabilidade: NivelProbabilidade;
  severidade: number;
  exigeMitigacao: boolean;
  faltaPlanoMitigacao: boolean;
  resposta: RespostaRisco | null;
  planoAcao: string | null;
  responsavel: string | null;
  prazo: string | null;
  dataConclusao: string | null;
  avaliacaoEficacia: string | null;
}

export interface LinhaOrcamento {
  id: number;
  categoria: string;
  descricao: string | null;
  ano: number;
  valorPrevisto: number;
  valorReal: number | null;
  variacao: number | null;
  observacoes: string | null;
}

export interface Requisito {
  id: number;
  tipo: TipoRequisito;
  requisito: string;
  descricao: string | null;
  entidade: string | null;
  resposta: string | null;
  responsavel: string | null;
  dataPrevista: string | null;
  dataConfirmacao: string | null;
  estado: EstadoRequisito;
  observacoes: string | null;
}

export interface Licao {
  id: number;
  categoria: CategoriaLicao;
  licao: string;
  descricao: string | null;
  impacto: string | null;
  data: string;
}

export interface ProjectCharter {
  iniciativaId: number;
  codigo: string;
  titulo: string;
  tipo: TipoIniciativa;
  estado: EstadoIniciativa;
  responsavel: string | null;
  dataInicio: string | null;
  dataFim: string | null;
  geral: CharterGeral;
  tarefas: Tarefa[];
  entregas: Entrega[];
  riscos: Risco[];
  orcamento: LinhaOrcamento[];
  totaisOrcamento: { ano: number; previsto: number; real: number }[];
  conformidade: Requisito[];
  licoes: Licao[];
  podeEditar: boolean;
  /** Orçamento: por omissão só o Administrador. */
  podeVerOrcamento: boolean;
  podeEditarOrcamento: boolean;
  equipa: MembroEquipa[];
  pilar: PilarEstrategico | null;
}

/** Alocação de uma pessoa num mês (em %; horas = 8 h × 21 dias × %). */
export interface AlocacaoMensal {
  ano: number;
  mes: number;
  percentagem: number;
  horas: number;
}

export interface MembroEquipa {
  id: number;
  utilizadorId: number | null;
  nome: string;
  funcao: string;
  alocacoes: AlocacaoMensal[];
}

/** Caminho de cada separador na API (/api/Iniciativas/{id}/charter/{seccao}). */
export type SeccaoCharter = 'tarefas' | 'entregas' | 'riscos' | 'orcamento' | 'conformidade' | 'licoes';

export interface LicaoRepositorio {
  id: number;
  data: string;
  iniciativaId: number;
  iniciativaCodigo: string;
  iniciativaTitulo: string;
  categoria: CategoriaLicao;
  licao: string;
  descricao: string | null;
  impacto: string | null;
}

export interface ConhecimentoCodificado {
  id: number;
  data: string;
  documento: string;
  url: string | null;
  modoConsulta: string | null;
  descricao: string | null;
  resultado: string | null;
  origem: OrigemConhecimento;
  iniciativaId: number | null;
  iniciativaCodigo: string | null;
  entregaCodigo: string | null;
  arquivadoEm: string | null;
  atividades: AtividadeGestao[] | null;
}

export interface RegistoAuditoria {
  id: number;
  data: string;
  utilizador: string | null;
  acao: AcaoAuditoria;
  entidade: string;
  entidadeId: string;
  valoresAntes: string | null;
  valoresDepois: string | null;
}

export const nomeTipoTarefa: Record<TipoTarefa, string> = {
  Analise: 'análise',
  Gestao: 'gestão',
  Comunicacao: 'comunicação',
  Tecnica: 'técnica',
  Dados: 'dados',
  Testes: 'testes',
};

export const nomeEstadoTrabalho: Record<EstadoTrabalho, string> = {
  Planeado: 'planeado',
  EmCurso: 'em curso',
  Concluido: 'concluído',
  Atrasado: 'atrasado',
  Cancelado: 'cancelado',
};

export const classeEstadoTrabalho: Record<EstadoTrabalho, string> = {
  Planeado: 'estado-validacao-equipa',
  EmCurso: 'estado-em-avaliacao',
  Concluido: 'estado-aprovada',
  Atrasado: 'estado-em-discussao',
  Cancelado: 'estado-nao-aprovada',
};

export const nomeTipoEntrega: Record<TipoEntrega, string> = { Entregavel: 'entregável', Milestone: 'milestone' };

export const nomeImpacto: Record<NivelImpacto, string> = { Baixo: '1 · baixo', Medio: '2 · médio', Alto: '3 · alto', Extremo: '4 · extremo' };

export const nomeProbabilidade: Record<NivelProbabilidade, string> = {
  Raro: '1 · raro',
  Improvavel: '2 · improvável',
  PoucoProvavel: '3 · pouco provável',
  MuitoProvavel: '4 · muito provável',
};

export const nomeResposta: Record<RespostaRisco, string> = { Evitar: 'evitar', Mitigar: 'mitigar', Transferir: 'transferir', Aceitar: 'aceitar' };

export const nomeTipoRequisito: Record<TipoRequisito, string> = {
  Legal: 'legal',
  Qualidade: 'qualidade',
  Ambiente: 'ambiente',
  Seguranca: 'segurança',
};

export const nomeEstadoRequisito: Record<EstadoRequisito, string> = {
  Pendente: 'pendente',
  EmCurso: 'em curso',
  Cumprido: 'cumprido',
  NaoAplicavel: 'não aplicável',
};

export const nomeCategoriaLicao: Record<CategoriaLicao, string> = {
  Estrategica: 'estratégica',
  Gestao: 'gestão',
  Operacional: 'operacional',
  Tecnica: 'técnica',
};

export const nomeOrigemConhecimento: Record<OrigemConhecimento, string> = {
  Manual: 'manual',
  RelatorioIniciativa: 'relatório de iniciativa',
  Vigilancia: 'vigilância',
};

export const nomeAcaoAuditoria: Record<AcaoAuditoria, string> = { Criar: 'criou', Alterar: 'alterou', Eliminar: 'eliminou' };

// Registos manuais (RF14) e plano anual (Mod.69)

export type TipoParceiro = 'Empresarial' | 'Academico';
export type TipoProtocolo = 'InvestigacaoPartilhada' | 'CoPromocao' | 'Confidencialidade' | 'Inovacao' | 'Investigacao';
export type EstadoAcordo = 'EmCurso' | 'Terminado';
export type TipoAtivo = 'Patente' | 'Design' | 'Marca';
export type Confidencialidade = 'Baixo' | 'Elevado';
export type TipoValorExterno = 'NumeroTrabalhadores' | 'Receitas' | 'ProveitosIdi';
export type PeriodoKpi = 'Anual' | 'PrimeiroSemestre' | 'SegundoSemestre';

export interface Parceiro {
  id: number;
  nome: string;
  nif: string | null;
  tipo: TipoParceiro;
  descricao: string | null;
  acordos: number;
  acordosEmCurso: number;
  iniciativas: { id: number; codigo: string; titulo: string }[];
}

export interface Acordo {
  id: number;
  parceiroId: number;
  parceiro: string;
  tipoParceiro: TipoParceiro;
  descricao: string;
  protocoloEstabelecido: boolean;
  tipoProtocolo: TipoProtocolo | null;
  dataInicio: string | null;
  dataFim: string | null;
  incluiGestaoPropriedadeIntelectual: boolean;
  estado: EstadoAcordo;
  observacoes: string | null;
  linkDocumento: string | null;
}

export interface Ativo {
  id: number;
  data: string;
  tipo: TipoAtivo;
  entidadeRegisto: string;
  designacao: string;
  descricao: string | null;
  numeroPedido: string | null;
  autores: string | null;
  dataRegisto: string | null;
  dataValidade: string | null;
  ambito: string | null;
  situacao: string | null;
  custo: number | null;
  perspetivaValor: string | null;
  numeroProcesso: string | null;
  observacoes: string | null;
  diasParaValidade: number | null;
  atividades: AtividadeGestao[];
}

export interface ConhecimentoTacito {
  id: number;
  data: string;
  empresa: string | null;
  detentor: string;
  descricao: string;
  confidencialidade: Confidencialidade;
  atividades: AtividadeGestao[];
}

export interface Ferramenta {
  id: number;
  data: string;
  nome: string;
  sumarioExecutivo: string | null;
  potencial: string | null;
  custoAquisicao: number | null;
  divulgacao: string | null;
  analises: AnaliseAnual[];
}

export interface AtividadePlano {
  id: number;
  titulo: string;
  descricao: string | null;
  cor: string | null;
  dataInicio: string;
  dataFim: string | null;
  recorrencia: Periodicidade;
  diasAntecedenciaAviso: number;
  responsaveis: UtilizadorResumo[];
  ocorrencias: string[];
  duracaoDias: number;
  proximaOcorrencia: string | null;
}

export interface ValorExterno {
  id: number;
  tipo: TipoValorExterno;
  ano: number;
  periodo: PeriodoKpi;
  valor: number;
  observacoes: string | null;
}

export const nomeTipoParceiro: Record<TipoParceiro, string> = { Empresarial: 'empresarial', Academico: 'académico (ENESI)' };

export const nomeTipoProtocolo: Record<TipoProtocolo, string> = {
  InvestigacaoPartilhada: 'investigação partilhada',
  CoPromocao: 'co-promoção',
  Confidencialidade: 'confidencialidade (NDA)',
  Inovacao: 'inovação',
  Investigacao: 'investigação',
};

export const nomeEstadoAcordo: Record<EstadoAcordo, string> = { EmCurso: 'em curso', Terminado: 'terminado' };

export const nomeTipoAtivo: Record<TipoAtivo, string> = { Patente: 'patente', Design: 'design', Marca: 'marca' };

export const nomeConfidencialidade: Record<Confidencialidade, string> = { Baixo: 'baixa', Elevado: 'elevada' };

export const nomeTipoValorExterno: Record<TipoValorExterno, string> = {
  NumeroTrabalhadores: 'n.º de trabalhadores',
  Receitas: 'receitas (€)',
  ProveitosIdi: 'proveitos de IDI (€)',
};

export const nomePeriodoKpi: Record<PeriodoKpi, string> = { PrimeiroSemestre: '1.º semestre', SegundoSemestre: '2.º semestre', Anual: 'ano completo' };

// Indicadores (Mod.239) e dashboard inicial

export type UnidadeKpi = 'Numero' | 'Percentagem' | 'Euros' | 'Media';

/** Valor de um KPI num ano (oficial = importado do Mod.239; parcial = ano em curso, até hoje). */
export interface PontoEvolucao {
  ano: number;
  valor: number | null;
  objetivo: string | null;
  atingido: boolean | null;
  oficial: boolean;
  parcial: boolean;
}

export interface EvolucaoKpi {
  numero: number;
  nome: string;
  unidade: UnidadeKpi;
  pontos: PontoEvolucao[];
  nota: string | null;
}

export interface Kpi {
  numero: number;
  nome: string;
  categoria: string;
  unidade: UnidadeKpi;
  valor: number | null;
  valorAnoAnterior: number | null;
  disponivel: boolean;
  nota: string | null;
  /** Objetivo anual escrito como no Mod.239 (ex.: "≥ 30"). */
  objetivo: string | null;
  /** Só com o ano completo; null sem objetivo ou sem valor. */
  atingido: boolean | null;
  acoesAbertas: number;
}

export type TipoAcaoKpi = 'PlanoInicial' | 'Acompanhamento';

export const nomeTipoAcaoKpi: Record<TipoAcaoKpi, string> = { PlanoInicial: 'plano de ação inicial', Acompanhamento: 'acompanhamento' };

export interface AcaoKpi {
  id: number;
  tipo: TipoAcaoKpi;
  dataCriacao: string;
  descricao: string;
  analiseResultados: string | null;
  recursos: string | null;
  responsavelId: number | null;
  responsavel: string | null;
  prazo: string | null;
  concluidaEm: string | null;
  eficaz: boolean | null;
  avaliacaoAnual: string | null;
}

export interface PlanoKpi {
  numero: number;
  nome: string;
  categoria: string;
  unidade: UnidadeKpi;
  ano: number;
  objetivo: string | null;
  acoes: AcaoKpi[];
  podeEditar: boolean;
}

export interface Indicadores {
  ano: number;
  periodo: PeriodoKpi;
  kpis: Kpi[];
  iniciativasPorEstado: { tipo: TipoIniciativa; estado: EstadoIniciativa; quantidade: number }[];
  proximasAtividades: { id: number; titulo: string; data: string; cor: string | null }[];
  alertas: { tipo: 'validade' | 'atraso' | 'risco' | string; titulo: string; detalhe: string; data: string | null; link: string }[];
}

// Estratégia de inovação (Mod.52)

export interface LinhaEstrategia {
  id: number;
  ano: number;
  areaAtuacao: string;
  abordagem: string | null;
  planoAcao: string | null;
  kpis: { numero: number; nome: string }[];
  duracaoMeses: number | null;
  numeroRecursosHumanos: number | null;
  horasMesPorRecurso: number | null;
  dedicacaoTotalHoras: number | null;
  recursosExtra: string | null;
  horizonte: Horizonte | null;
  iniciativaId: number | null;
  iniciativaCodigo: string | null;
  iniciativaTitulo: string | null;
}

/** Áreas de atuação do Mod.52 (sugestões; pode escrever-se outra). */
export const areasAtuacao = ['Tecnologia de Fibra Ótica', 'Automatização', 'Novas áreas de Negócio', 'Planeta e Prosperidade', 'Cultura'];
