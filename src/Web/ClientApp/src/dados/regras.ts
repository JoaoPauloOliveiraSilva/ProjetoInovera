import type { Notas } from './tipos';

/** Pesos do Mod.246 (configuráveis no backend; a especificação fala em média simples). */
export const pesosMod246: Record<keyof Notas, number> = {
  custo: 0.15,
  enquadramento: 0.25,
  beneficio: 0.3,
  adequacao: 0.2,
  incerteza: 0.1,
};

export const limiarAprovacao = 2;

export const variaveis: { chave: keyof Notas; nome: string; curto: string }[] = [
  { chave: 'custo', nome: 'Custo', curto: 'Custo' },
  { chave: 'enquadramento', nome: 'Enquadramento', curto: 'Enquadr.' },
  { chave: 'beneficio', nome: 'Benefício', curto: 'Benefício' },
  { chave: 'adequacao', nome: 'Adeq. Técnica', curto: 'Adeq. Téc.' },
  { chave: 'incerteza', nome: 'Incerteza', curto: 'Incerteza' },
];

/** Nota final ponderada; null se faltar alguma variável (N.A.). */
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
