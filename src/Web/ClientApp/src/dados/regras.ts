/** Notas de 0 a 4 de uma ideia da dstelecom; null = ainda não escolhida. */
export interface Notas {
  custo: number | null;
  enquadramento: number | null;
  beneficio: number | null;
  adequacaoTecnica: number | null;
  incerteza: number | null;
}

/** Pesos do Mod.246 — iguais a PesosAvaliacao.Mod246 no backend (que é quem calcula a nota oficial). */
export const pesosMod246: Record<keyof Notas, number> = {
  custo: 0.15,
  enquadramento: 0.25,
  beneficio: 0.3,
  adequacaoTecnica: 0.2,
  incerteza: 0.1,
};

/** Igual a AvaliacaoIdeia.LimiarAprovacao no backend. */
export const limiarAprovacao = 2;

export const variaveis: { chave: keyof Notas; nome: string; curto: string }[] = [
  { chave: 'custo', nome: 'Custo', curto: 'Custo' },
  { chave: 'enquadramento', nome: 'Enquadramento', curto: 'Enquadr.' },
  { chave: 'beneficio', nome: 'Benefício', curto: 'Benefício' },
  { chave: 'adequacaoTecnica', nome: 'Adeq. Técnica', curto: 'Adeq. Téc.' },
  { chave: 'incerteza', nome: 'Incerteza', curto: 'Incerteza' },
];

/** Pré-visualização da nota final; null se faltar alguma variável. */
export function calcularNota(notas: Notas): number | null {
  let total = 0;
  for (const { chave } of variaveis) {
    const valor = notas[chave];
    if (valor === null) {
      return null;
    }
    total += pesosMod246[chave] * valor;
  }
  return Math.round(total * 100) / 100;
}

export function formatarData(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString('pt-PT', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export function haQuantoTempo(iso: string): string {
  const minutos = Math.max(0, Math.round((Date.now() - new Date(iso).getTime()) / 60000));
  if (minutos < 1) return 'agora';
  if (minutos < 60) return `há ${minutos} min`;
  const horas = Math.round(minutos / 60);
  if (horas < 24) return `há ${horas} ${horas === 1 ? 'hora' : 'horas'}`;
  const dias = Math.round(horas / 24);
  return `há ${dias} ${dias === 1 ? 'dia' : 'dias'}`;
}

export function formatarTamanho(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

/** Primeiro nome, para o "olá, …" do menu. */
export function primeiroNome(nome: string): string {
  return nome.trim().split(/\s+/)[0] ?? nome;
}
