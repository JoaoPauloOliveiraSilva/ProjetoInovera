import { expect, test } from '@playwright/test';
import { admin, entrar, gaveta } from './ajudantes';

test('um utilizador novo tem um só perfil', async ({ page }) => {
  await entrar(page, admin, '/inovacao/admin/utilizadores');
  const email = `e2e.${Date.now().toString(36)}@exemplo.pt`;
  await page.getByRole('button', { name: /adicionar utilizador/ }).click();
  const g = gaveta(page);
  await g.getByLabel('nome').fill('Pessoa de Teste E2E');
  await g.getByLabel('email').fill(email);
  await expect(g.locator('input[type="radio"]:checked')).toHaveCount(1);
  await g.locator('label.caixa', { hasText: 'Equipa de Inovação' }).click();
  await expect(g.getByLabel('Equipa de Inovação')).toBeChecked();
  await expect(g.locator('input[type="radio"]:checked')).toHaveCount(1);
  await g.getByRole('button', { name: /adicionar e enviar convite/ }).click();
  await expect(page.getByText('Pessoa de Teste E2E foi adicionado')).toBeVisible();

  const linha = page.locator('.tabela-linha', { hasText: email });
  await expect(linha).toContainText('Equipa de Inovação');
  await expect(linha).toContainText('convite enviado');
});
