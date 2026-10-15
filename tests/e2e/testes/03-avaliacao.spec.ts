import { expect, test } from '@playwright/test';
import { admin, entrar } from './ajudantes';

// Ideia dos dados de exemplo com a discussão terminada.
const ideia = 'Sensores de ocupação nas salas de reunião';

test('a avaliação com a CE aprova a ideia e cria o projeto com código PRJ', async ({ page }) => {
  await entrar(page, admin, '/inovacao/avaliacao');
  const linha = page.locator('.aval-bloco', { hasText: ideia });
  await expect(linha).toBeVisible();
  for (const variavel of ['Custo', 'Enquadramento', 'Benefício', 'Adeq. Técnica', 'Incerteza']) {
    await linha.getByLabel(variavel).selectOption('3');
  }
  await expect(linha.locator('.total')).toHaveText('3.00');
  await expect(linha).toContainText('Aprovada');
  await linha.locator('select').last().selectOption('Projeto');
  await linha.getByRole('button', { name: 'registar' }).click();
  await expect(page.locator('.aval-bloco', { hasText: ideia })).toHaveCount(0);

  await page.goto('/inovacao/projetos');
  const projeto = page.locator('.tabela-linha', { hasText: ideia });
  await expect(projeto).toBeVisible();
  await expect(projeto).toContainText(/PRJ-\d{3}/);
});
