/** Iguais a Domain/Constants/Permissoes.cs. O backend volta a verificar tudo; aqui só servem para mostrar menus e botões. */
export const Permissoes = {
  ideiasVerTodas: 'ideias.ver-todas',
  ideiasValidar: 'ideias.validar',
  ideiasAvaliar: 'ideias.avaliar',
  iniciativasGerir: 'iniciativas.gerir',
  gerirUtilizadores: 'utilizadores.gerir',
  gerirPerfis: 'perfis.gerir',
  verAuditoria: 'auditoria.ver',
  gerirRegistos: 'registos.gerir',
  gerirPlano: 'plano.gerir',
  valoresExternos: 'kpis.valores-externos',
  objetivosKpi: 'kpis.objetivos',
  verOrcamento: 'orcamento.ver',
  gerirOrcamento: 'orcamento.gerir',
} as const;

export type Permissao = (typeof Permissoes)[keyof typeof Permissoes];
