import { expect, test } from '@playwright/test';
import { admin, entrar, gaveta, trabalhador, unico } from './ajudantes';

// O ciclo de uma ideia: o trabalhador submete, a equipa valida, fica em discussão, recebe gostos e comentários.
test.describe.configure({ mode: 'serial' });

const titulo = unico('Ideia de teste E2E sobre drones para inspeção de postes');

test('um trabalhador submete uma ideia nos 4 passos', async ({ page }) => {
  await entrar(page, trabalhador);
  await page.getByRole('button', { name: 'nova ideia' }).first().click();
  const g = gaveta(page);
  await g.getByRole('button', { name: 'melhoria', exact: true }).click();
  await g.getByRole('button', { name: /continuar/ }).click();
  await g.getByLabel('título').fill(titulo);
  await g.getByLabel('descrição').fill('Usar drones para inspecionar os postes e encontrar problemas antes de avariarem.');
  await g.getByRole('button', { name: /continuar/ }).click();
  await g.getByLabel('vantagens').fill('Menos deslocações das equipas.');
  await g.getByRole('button', { name: /continuar/ }).click();
  await g.getByText('li e aceito os termos e condições').click();
  await g.getByRole('button', { name: /registar ideia/ }).click();
  await expect(g.getByRole('heading', { name: /Ideia AA\d+ registada|Ideia [A-Z]{2}\d{2} registada/ })).toBeVisible();
});

test('a ideia aparece na minha caixa à espera da validação da equipa', async ({ page }) => {
  await entrar(page, trabalhador, '/inovacao/minha-caixa');
  const linha = page.locator('.tabela-linha', { hasText: titulo });
  await expect(linha).toBeVisible();
  await expect(linha).toContainText('validação da equipa de inovação');
});

test('a equipa de inovação valida a ideia e ela fica em discussão pública', async ({ page }) => {
  await entrar(page, admin, '/inovacao/validacao');
  const linha = page.locator('.tabela-linha', { hasText: titulo });
  await linha.getByRole('button', { name: 'validar' }).click();
  await expect(linha).toHaveCount(0);
  await page.goto('/inovacao/ideias/discussao');
  await expect(page.locator('.tabela-linha', { hasText: titulo })).toBeVisible();
});

test('na discussão pública dá-se gosto e comenta-se', async ({ page }) => {
  await entrar(page, admin, '/inovacao/ideias/discussao');
  await page.locator('.tabela-linha', { hasText: titulo }).click();
  const g = gaveta(page);
  await g.locator('.botao-gosto').click();
  await expect(g.locator('.botao-gosto')).toHaveClass(/ativo/);
  const comentario = unico('Boa ideia, testar primeiro numa zona');
  await g.getByLabel('Comentário').fill(comentario);
  await g.getByRole('button', { name: /comentar/ }).click();
  await expect(g.locator('.comentarios')).toContainText(comentario);
});

test('ao escrever o título de uma ideia parecida aparece o aviso', async ({ page }) => {
  await entrar(page, trabalhador);
  await page.getByRole('button', { name: 'nova ideia' }).first().click();
  const g = gaveta(page);
  await g.getByRole('button', { name: 'melhoria', exact: true }).click();
  await g.getByRole('button', { name: /continuar/ }).click();
  await g.getByLabel('título').fill('Drones para inspeção de postes');
  const aviso = g.locator('.ideias-parecidas');
  await expect(aviso).toBeVisible();
  await expect(aviso).toContainText(titulo);
});
