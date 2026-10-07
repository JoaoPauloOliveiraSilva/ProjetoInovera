/** Mensagens de "a carregar" e de erro iguais em todas as páginas. */
export function EstadoPedido({ aCarregar, erro, vazio }: { aCarregar: boolean; erro: string | null; vazio?: boolean }) {
  if (erro) return <div className="vazio erro">{erro}</div>;
  if (aCarregar && vazio !== false) return <div className="vazio">a carregar…</div>;
  return null;
}
