import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Quando corre pelo Aspire (AppHost), a porta e o endereço da API chegam por variáveis de ambiente.
const api = process.env['services__webapi__https__0'] ?? process.env['services__webapi__http__0'];

export default defineConfig({
  plugins: [react()],
  server: {
    port: process.env['PORT'] ? Number(process.env['PORT']) : 5173,
    proxy: api
      ? {
          '/api': { target: api, secure: false },
          '/openapi': { target: api, secure: false },
          '/scalar': { target: api, secure: false },
        }
      : undefined,
  },
  build: {
    outDir: 'dist',
  },
});
