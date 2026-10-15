import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';

// Contas dos dados de exemplo (DadosExemplo = true): o Administrador vem do .env de teste; os outros são criados pela aplicação.
export const admin = {
  email: process.env.E2E_ADMIN_EMAIL ?? 'admin@innovera.local',
  password: process.env.E2E_ADMIN_PASSWORD ?? 'Innovera2026!',
};
export const trabalhador = { email: 'ricardo.lopes@exemplo.pt', password: 'Innovera2026!' };

/** Entra e fica na página pedida. */
export async function entrar(page: Page, conta: { email: string; password: string }, destino = '/inovacao') {
  await page.goto(`/entrar?voltar=${encodeURIComponent(destino)}`);
  await page.getByLabel('email').fill(conta.email);
  await page.getByLabel('password').fill(conta.password);
  await page.getByRole('button', { name: /^entrar/ }).click();
  await expect(page).toHaveURL(new RegExp(`${destino.replace(/[?]/g, '\\?')}`));
}

/** Gaveta aberta (formulários e fichas abrem numa gaveta à direita). */
export const gaveta = (page: Page) => page.locator('.gaveta');

/** Texto único para não chocar com dados de corridas anteriores. */
export const unico = (prefixo: string) => `${prefixo} ${Date.now().toString(36)}`;
