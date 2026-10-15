import { expect, test } from '@playwright/test';
import { admin, entrar, gaveta, unico } from './ajudantes';

test('no Project Charter acrescenta-se uma tarefa ao planeamento', async ({ page }) => {
  await entrar(page, admin, '/inovacao/projetos');
  await page.locator('.tabela-linha', { hasText: 'Manutenção preditiva das OLT com IA' }).click();
  await gaveta(page).getByRole('link', { name: /abrir project charter/ }).click();
  await expect(page).toHaveURL(/\/charter/);

  await page.getByRole('button', { name: 'planeamento', exact: true }).click();
  const nome = unico('Testar o modelo no terreno');
  await page.getByRole('button', { name: /nova tarefa/ }).click();
  const g = gaveta(page);
  await g.getByLabel('código WBS (ex.: 1.2)').fill('9.9');
  await g.locator('label.campo', { hasText: /^tarefa/ }).locator('input, textarea').fill(nome);
  await g.getByRole('button', { name: /^guardar/ }).click();
  await expect(page.getByText(nome)).toBeVisible();
});
