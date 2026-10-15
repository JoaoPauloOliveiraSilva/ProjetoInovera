import { expect, test } from '@playwright/test';
import { admin, entrar, unico } from './ajudantes';

test('carregar o relatório de uma vigilância regista-o e permite descarregá-lo', async ({ page }) => {
  await entrar(page, admin, '/inovacao/vigilancias');
  const cartao = page.locator('article.vigilancia').first();
  await expect(cartao).toBeVisible();
  await cartao.getByRole('button', { name: 'carregar relatório' }).click();

  const nome = `${unico('relatorio-e2e').replace(/\s/g, '-')}.pdf`;
  await cartao.getByLabel('Relatório', { exact: true }).setInputFiles({
    name: nome,
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4 relatório de teste'),
  });
  await cartao.getByLabel('Resumo do resultado').fill('Sem novidades relevantes este mês.');
  await cartao.getByRole('button', { name: 'carregar', exact: true }).click();

  const link = page.getByRole('link', { name: new RegExp(nome.replace('.', '\\.')) });
  await expect(link).toBeVisible();
  const [descarga] = await Promise.all([page.waitForEvent('download'), link.click()]);
  expect(descarga.suggestedFilename()).toBe(nome);
});
