# Innovera

Plataforma de gestão da inovação da **dstelecom** (Projeto de Informática, Universidade do Minho, 2026/27).

- **Backend:** .NET 10, Clean Architecture (Domain / Application / Infrastructure / Web), MediatR, FluentValidation, EF Core 10
- **Base de dados:** PostgreSQL (Npgsql)
- **Frontend:** React 19 + TypeScript + Vite (`src/Web/ClientApp`), com o aspeto do site de inovação atual da intranet
- **Contas:** ASP.NET Core Identity com cookie (contas próprias, sem ligação aos sistemas da dst)
- **Orquestração em desenvolvimento:** .NET Aspire (`src/AppHost`)
- **Alojamento previsto:** AWS (Lightsail + PostgreSQL gerido), ainda por configurar

## O que é preciso instalar

| Ferramenta | Versão | Para quê |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0.2xx ou mais recente | backend |
| [Node.js](https://nodejs.org) | 22 ou mais recente | frontend |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | — | PostgreSQL (e testes funcionais) |

Depois de clonar, uma vez só:

```bash
dotnet tool restore                 # instala o dotnet-ef (versão em .config/dotnet-tools.json)
cd src/Web/ClientApp && npm install # dependências do frontend
```

## Correr a aplicação

### Opção A — com o Aspire (recomendada)

Com o Docker Desktop aberto:

```bash
dotnet run --project src/AppHost
```

O Aspire cria o PostgreSQL num contentor, arranca a API e o frontend e abre o painel com os endereços
(o frontend é o recurso `webfrontend`). Na primeira vez a base de dados é criada e preenchida com dados de exemplo.

### Opção B — sem o Aspire

```bash
docker compose up -d                      # PostgreSQL em localhost:5432 (postgres/postgres, base innovera)
dotnet run --project src/Web --launch-profile https   # API em https://localhost:7240 (Scalar em /scalar)
cd src/Web/ClientApp && npm run dev       # frontend em http://localhost:5173 (encaminha /api para a API)
```

Se o browser se queixar do certificado HTTPS: `dotnet dev-certs https --trust`.

### Contas de demonstração

Criadas automaticamente em desenvolvimento (`"DadosExemplo": true` em `src/Web/appsettings.Development.json`):

| Conta | Password | Perfil |
|---|---|---|
| `admin@innovera.local` | `Innovera2026!` | Administrador (equipa de Inovação: valida e avalia ideias) |
| `ricardo.lopes@exemplo.pt`, `marta.sousa@exemplo.pt`, … | `Innovera2026!` | Trabalhador |

Também se pode criar uma conta nova no ecrã "entrar" (fica com o perfil Trabalhador).
Em produção a conta de administrador vem da configuração `ContaAdministrador:Email` / `ContaAdministrador:Password`
e não há dados de exemplo.

## Base de dados e migrações

As migrações estão em `src/Infrastructure/Data/Migrations` e são aplicadas automaticamente quando a API arranca.
Depois de mudar entidades ou configurações EF:

```bash
dotnet ef migrations add NomeDaMudanca --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations
```

Para começar com a base de dados vazia: `docker compose down -v` (opção B) ou apagar o volume
`innovera-postgres-dados` no Docker Desktop (opção A).

## Testes

```bash
dotnet test tests/Domain.UnitTests           # regras de negócio (código AA01, fluxo da ideia, nota do Mod.246, riscos…)
dotnet test tests/Application.UnitTests
dotnet test tests/Application.FunctionalTests # casos de uso contra um PostgreSQL real (precisa do Docker)
cd src/Web/ClientApp && npm run build         # verificação de tipos do frontend
```

O GitHub Actions (`.github/workflows/ci.yml`) corre o mesmo em cada push para `main` e `versao2` e em cada pull request.

## Estrutura

```
src/
  Domain/          entidades e regras de negócio (Ideias, Iniciativas, ProjectCharter, Registos, Kpis, Transversais)
  Application/     casos de uso (comandos e consultas MediatR) — Ideias/, Utilizadores/
  Infrastructure/  EF Core + PostgreSQL, Identity, ficheiros (anexos), tarefa que fecha as discussões aos 30 dias
  Web/             API (Endpoints/), arranque, e o frontend em ClientApp/
  AppHost/         Aspire (PostgreSQL + API + frontend)
tests/             testes do domínio, da Application e funcionais
docs/              modelo de domínio
```

### API (resumo)

| Método e caminho | O quê |
|---|---|
| `POST /api/Users/login?useCookies=true` · `POST /api/Users/logout` | entrar / sair |
| `POST /api/Utilizadores/registo` · `GET /api/Utilizadores/eu` · `GET /api/Utilizadores?texto=` | criar conta, utilizador atual, pesquisar colegas |
| `GET /api/Ideias?estado=&soMinhas=` · `GET /api/Ideias/{id}` · `POST /api/Ideias` | listar, ver e registar ideias |
| `POST /api/Ideias/{id}/confirmar-autoria` · `/validar` · `/fechar-discussao` | fluxo da ideia |
| `POST /api/Ideias/{id}/gosto` · `/comentarios` · `POST /api/Ideias/comentarios/{id}/gosto` | discussão pública |
| `POST /api/Ideias/{id}/avaliacao` · `/decisao-dst` · `/classe` | avaliação pela CE |
| `POST /api/Ideias/{id}/anexos` · `GET /api/Ideias/anexos/{id}` | anexos (até 25 MB) |

A documentação completa da API fica em `/scalar` com a API a correr.

## Equipa

Nuno Peixoto (gestão de projeto, backend/KPIs) · Junqing Chen (backend/BD) · Tomás Cunhas (frontend) ·
João Silva (DevOps/QA) · Diogo Canadas (frontend e qualidade)
