import { expect, test } from '@playwright/test';
import { admin, entrar } from './ajudantes';

test('os KPIs abrem por semestre e cada KPI tem a evolução por ano e o objetivo', async ({ page }, info) => {
  await entrar(page, admin, '/inovacao/indicadores');
  await expect(page.locator('.kpi-nome').first()).toBeVisible();
  await page.locator('select').filter({ hasText: '1.º semestre' }).selectOption({ label: '1.º semestre' });
  await expect(page.locator('.kpi-nome', { hasText: 'N.º de ideias submetidas' })).toBeVisible();

  await page.locator('a', { hasText: 'N.º de ideias submetidas' }).first().click();
  await expect(page).toHaveURL(/\/inovacao\/indicadores\/1/);
  const evolucao = page.locator('.evolucao-kpi');
  await expect(evolucao.getByRole('heading', { name: 'evolução por ano' })).toBeVisible();
  await expect(evolucao.locator('svg[role="img"]')).toBeVisible();
  await expect(evolucao.locator('.grafico-tabela tbody tr').first()).toBeVisible();

  // Um valor diferente em cada tentativa (numa repetição o anterior já ficou guardado e o botão não aparece).
  const novo = `≥ ${5 + info.retry}`;
  await page.getByLabel('Objetivo anual').fill(novo);
  const guardar = page.getByRole('button', { name: 'guardar' });
  await guardar.click();
  // O botão desaparece quando o objetivo fica guardado e a página o volta a ler.
  await expect(guardar).toBeHidden();
  await page.reload();
  await expect(page.getByLabel('Objetivo anual')).toHaveValue(novo);
});
