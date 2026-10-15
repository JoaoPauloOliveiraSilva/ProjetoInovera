/** Secção do manual que explica a página onde a pessoa está (o botão "ajuda" abre logo nessa secção). */
export function seccaoDaPagina(caminho: string): string {
  const p = caminho.replace(/^\/inovacao/, '');
  const regras: [RegExp, number][] = [
    [/^\/admin\/importar/, 11],
    [/^\/admin/, 12],
    [/^\/(validacao|avaliacao)/, 5],
    [/^\/iniciativas\/\d+\/charter/, 7],
    [/^\/vigilancias/, 8],
    [/^\/(iniciativas|projetos|desafios|oportunidades)/, 6],
    [/^\/indicadores/, 10],
    [/^\/(estrategia|plano-anual|parcerias|ativos|conhecimento|licoes|conhecimento-tacito|ferramentas)/, 9],
    [/^\/(ideias|minha-caixa)/, 4],
  ];
  const regra = regras.find(([r]) => r.test(p));
  return regra ? `seccao-${regra[1]}` : 'seccao-3';
}
