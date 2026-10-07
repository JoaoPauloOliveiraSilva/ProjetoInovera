# Innovera — frontend (React + TypeScript + Vite)

Ligado à API do backend (`/api/...`) com o cookie de sessão do ASP.NET Core Identity.

- `npm install` — instala as dependências (só da primeira vez)
- `npm run dev` — abre em http://localhost:5173 e reencaminha `/api` para a API (por omissão https://localhost:7240;
  outro endereço com a variável `API_URL`). Com o Aspire o endereço é passado automaticamente.
- `npm run build` — verifica os tipos e gera a versão de produção em `dist/`

## Organização

| Pasta | O quê |
|---|---|
| `src/api` | pedidos à API (`servicos.ts`), tipos iguais aos DTOs do backend (`tipos.ts`), hook `usePedido` |
| `src/sessao` | sessão (entrar, sair, criar conta) e rotas protegidas |
| `src/componentes` | layout igual ao site atual da intranet (barra, menu, gaveta, ícones) |
| `src/paginas` | landing, entrar, homepage, listas de ideias, detalhe, nova ideia, validação e avaliação |
| `src/dados/regras.ts` | pré-visualização da nota (pesos do Mod.246) e formatação de datas |

Ecrãs: landing "Plataforma de Gestão da Certificação" (só Inovação ativa) → entrar → site de Inovação
(homepage, ideias em discussão, todas as ideias, as minhas ideias, detalhe com likes, comentários e anexos,
nova ideia em 4 passos). A equipa de Inovação (perfil Administrador) vê também "validação de ideias" e "avaliação de ideias".
