import { defineConfig, devices } from '@playwright/test';

// Os testes partilham a mesma base de dados (dados de exemplo), por isso correm um de cada vez e pela ordem dos ficheiros.
export default defineConfig({
  testDir: './testes',
  fullyParallel: false,
  workers: 1,
  retries: 1,
  timeout: 45_000,
  expect: { timeout: 10_000 },
  reporter: [['list'], ['html', { open: 'never', outputFolder: 'relatorio' }]],
  outputDir: 'resultados',
  use: {
    baseURL: process.env.E2E_URL ?? 'http://127.0.0.1:18080',
    locale: 'pt-PT',
    timezoneId: 'Europe/Lisbon',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    acceptDownloads: true,
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'], viewport: { width: 1440, height: 900 } } }],
});
