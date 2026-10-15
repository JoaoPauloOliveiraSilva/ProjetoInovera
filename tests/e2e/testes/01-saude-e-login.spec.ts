import { expect, test } from '@playwright/test';
import { admin, entrar } from './ajudantes';

test('o endereço de saúde responde (usado pela monitorização)', async ({ request }) => {
  const resposta = await request.get('/saude');
  expect(resposta.status()).toBe(200);
  expect(await resposta.text()).toBe('Healthy');
});

test('password errada não entra', async ({ page }) => {
  await page.goto('/entrar');
  await page.getByLabel('email').fill(admin.email);
  await page.getByLabel('password').fill('password-errada');
  await page.getByRole('button', { name: /^entrar/ }).click();
  await expect(page.locator('.erro')).toBeVisible();
  await expect(page).toHaveURL(/\/entrar/);
});

test('sem sessão, uma página interna pede para entrar', async ({ page }) => {
  await page.goto('/inovacao/indicadores');
  await expect(page).toHaveURL(/\/entrar\?voltar=/);
});

test('o Administrador entra e vê a homepage e o menu de administração', async ({ page }) => {
  await entrar(page, admin);
  await expect(page.locator('.menu-ola')).toContainText('olá');
  await expect(page.getByRole('button', { name: 'administração' })).toBeVisible();
});
