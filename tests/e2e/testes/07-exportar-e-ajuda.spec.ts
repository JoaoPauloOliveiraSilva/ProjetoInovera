import { expect, test } from '@playwright/test';
import { admin, entrar } from './ajudantes';

test('exportar o Mod.235 descarrega um Excel', async ({ page }) => {
  await entrar(page, admin, '/inovacao/parcerias');
  const [descarga] = await Promise.all([page.waitForEvent('download'), page.getByRole('link', { name: /exportar Excel/ }).click()]);
  expect(descarga.suggestedFilename()).toMatch(/\.xlsx$/);
});

test('o botão ajuda abre o manual na secção da página onde se está', async ({ page }) => {
  await entrar(page, admin, '/inovacao/indicadores');
  await page.locator('.barra-ajuda').click();
  await expect(page).toHaveURL(/\/inovacao\/ajuda\?seccao=seccao-10/);
  await expect(page.getByRole('heading', { name: /10\. Indicadores de desempenho/ })).toBeInViewport();
  await expect(page.getByRole('navigation', { name: 'Índice do manual' })).toContainText('Project Charter');
});
