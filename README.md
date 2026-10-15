# Innovera

Plataforma de gestão da inovação da **dstelecom** (Projeto de Informática, Universidade do Minho, 2026/27).

- **Backend:** .NET 10, Clean Architecture (Domain / Application / Infrastructure / Web), mediador próprio (sem MediatR), FluentValidation, EF Core 10
- **Base de dados:** PostgreSQL (Npgsql)
- **Frontend:** React 19 + TypeScript + Vite (`src/Web/ClientApp`), com o aspeto do site de inovação atual da intranet
- **Contas:** ASP.NET Core Identity com cookie (contas próprias, sem ligação aos sistemas da dst); o Administrador
  adiciona as pessoas e elas recebem um email para definir a password
- **Perfis e permissões:** Administrador, Equipa de Inovação e Trabalhador, configuráveis no backoffice
- **Email:** SMTP (Mailpit em desenvolvimento, Gmail do projeto no servidor)
- **Orquestração em desenvolvimento:** .NET Aspire (`src/AppHost`)
- **Alojamento:** servidor Lightsail da dst com Docker Compose + Caddy (HTTPS) e PostgreSQL no próprio servidor — https://63.181.134.124.nip.io

A documentação técnica completa (arquitetura, segurança, configuração, testes, publicação e monitorização) está em
[docs/documentacao-tecnica.md](docs/documentacao-tecnica.md).

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

O Aspire cria o PostgreSQL e o Mailpit (emails de teste) em contentores, arranca a API e o frontend e abre o painel
com os endereços (o frontend é o recurso `webfrontend`; os emails veem-se no recurso `mailpit`). Na primeira vez a base de dados é criada e preenchida com dados de exemplo.

### Opção B — sem o Aspire

```bash
docker compose up -d                      # PostgreSQL em localhost:5432 e Mailpit (emails) em http://localhost:8025
dotnet run --project src/Web --launch-profile https   # API em https://localhost:7240 (Scalar em /scalar)
cd src/Web/ClientApp && npm run dev       # frontend em http://localhost:5173 (encaminha /api para a API)
```

Se o browser se queixar do certificado HTTPS: `dotnet dev-certs https --trust`.

### Contas de demonstração

Criadas automaticamente em desenvolvimento (`"DadosExemplo": true` em `src/Web/appsettings.Development.json`):

| Conta | Password | Perfil |
|---|---|---|
| `admin@innovera.local` | `Innovera2026!` | Administrador (tudo o que a Equipa de Inovação faz + gerir a plataforma) |
| `sofia.martins@exemplo.pt` | `Innovera2026!` | Equipa de Inovação (valida e avalia ideias) |
| `ricardo.lopes@exemplo.pt`, `marta.sousa@exemplo.pt`, … | `Innovera2026!` | Trabalhador |

Não há registo livre: o Administrador adiciona as pessoas em **administração › utilizadores** e elas recebem um
email com o link para definir a password (válido 3 dias). Em desenvolvimento os emails não saem para a Internet:
ficam no Mailpit (http://localhost:8025). "Esqueci-me da password" funciona da mesma forma.

Em produção a conta de administrador vem da configuração `ContaAdministrador:Email` / `ContaAdministrador:Password`
e não há dados de exemplo.

## Perfis e permissões

Os casos de uso pedem **permissões** (`[Authorize(Permissao = Permissoes.Ideias.Validar)]`), nunca perfis.
Cada perfil tem uma lista de permissões guardada na base de dados e editável em **administração › perfis e permissões**,
onde também se criam perfis novos. O Administrador tem sempre todas. Uma funcionalidade nova só precisa de uma
constante em `src/Domain/Constants/Permissoes.cs` (e, se quiser, da permissão por omissão de cada perfil).

| Permissão | Equipa de Inovação | Trabalhador |
|---|:-:|:-:|
| `ideias.ver-todas` — ver ideias privadas e por validar, anexar ficheiros | ✔ | |
| `ideias.validar` — validar ideias e fechar discussões | ✔ | |
| `ideias.avaliar` — avaliação da CE, decisão dst e classe | ✔ | |
| `iniciativas.gerir` — criar/editar iniciativas e importar o Mod.246 | ✔ | |
| `utilizadores.gerir` — adicionar e gerir utilizadores | | |
| `perfis.gerir` — gerir perfis e permissões | | |

## Portefólio (Mod.246) e avisos

- **Importar o Excel Mod.246** em **administração › importar Mod.246**: entram as ideias da Caixa de Inovação (com o n.º
  da Caixa, notas, decisão e classe) e as folhas Projetos, Melhorias Contínuas, Oportunidades, Vigilâncias, Mestrados e
  Ações de Inovação. Pode repetir-se: o que já existe não se duplica. Os dados reais **não** vão para o git — cada um
  importa o ficheiro na sua base de dados.
- **Iniciativas** (menu projetos, desafios, vigilâncias, iniciativas, oportunidades): lista com filtros, ficha,
  edição e histórico do ponto de situação.
- **Avisos** (sino na barra da esquerda): coautoria por confirmar, ideia para validar, ideia publicada, discussão
  terminada, decisão da CE e comentários. Os mais importantes também seguem por email.

## Email (SMTP)

Configuração (`appsettings` ou variáveis de ambiente, ex.: `Smtp__Host`):

| Chave | Exemplo (AWS) |
|---|---|
| `Email:Remetente` / `Email:NomeRemetente` | `inovacao@innovera.dstelecom.pt` / `Inovação dstelecom` |
| `Email:UrlSite` | endereço do site (usado nos links dos emails) |
| `Smtp:Host` / `Smtp:Porta` / `Smtp:Ssl` | `email-smtp.eu-west-3.amazonaws.com` / `587` / `true` |
| `Smtp:Utilizador` / `Smtp:Password` | credenciais SMTP criadas na consola do SES (nunca no git) |

Sem `Smtp:Host` os emails só são escritos no log da API.

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
# testes no browser (Playwright) contra a imagem de produção: ver docs/documentacao-tecnica.md, secção 10
cd src/Web/ClientApp && npm run build         # verificação de tipos do frontend
```

O GitHub Actions (`.github/workflows/ci.yml`) corre o mesmo em cada push para `main` e `versao2` e em cada pull request.

## Estrutura

```
src/
  Domain/          entidades e regras de negócio (Ideias, Iniciativas, ProjectCharter, Registos, Kpis, Transversais)
  Application/     casos de uso (comandos e consultas com o mediador próprio) — Ideias/, Iniciativas/, Charters/, Registos/, Kpis/, Importacoes/…
  Infrastructure/  EF Core + PostgreSQL, Identity (contas, perfis, permissões), email SMTP, ficheiros, tarefa dos 30 dias
  Web/             API (Endpoints/), arranque, e o frontend em ClientApp/
  AppHost/         Aspire (PostgreSQL + API + frontend)
tests/             testes do domínio, da Application, funcionais e E2E (tests/e2e)
docs/              documentação técnica, modelo de domínio, requisitos, publicação
```

### API (resumo)

| Método e caminho | O quê |
|---|---|
| `POST /api/Conta/entrar` · `/sair` | entrar / sair |
| `POST /api/Conta/aceitar-convite` · `/recuperar-password` · `/redefinir-password` | links enviados por email |
| `GET /api/Utilizadores/eu` · `GET /api/Utilizadores?texto=` | utilizador atual (com permissões), pesquisar colegas |
| `GET/POST /api/Administracao/utilizadores` · `PUT …/{id}` · `POST …/{id}/reenviar-convite` · `/desativar` · `/ativar` | backoffice de utilizadores |
| `GET/POST /api/Administracao/perfis` · `PUT …/{nome}/permissoes` · `DELETE …/{nome}` | perfis e permissões |
| `GET /api/Ideias?estado=&soMinhas=` · `GET /api/Ideias/{id}` · `POST /api/Ideias` | listar, ver e registar ideias |
| `POST /api/Ideias/{id}/confirmar-autoria` · `/validar` · `/fechar-discussao` | fluxo da ideia |
| `POST /api/Ideias/{id}/gosto` · `/comentarios` · `POST /api/Ideias/comentarios/{id}/gosto` | discussão pública |
| `POST /api/Ideias/{id}/avaliacao` · `/decisao-dst` · `/classe` | avaliação pela CE |
| `POST /api/Ideias/{id}/anexos` · `GET /api/Ideias/anexos/{id}` | anexos (até 25 MB) |
| `GET /api/Ideias?texto=&classe=&responsabilidade=&ano=&soDecididas=` | portefólio de ideias com filtros |
| `GET/POST /api/Iniciativas` · `GET/PUT /api/Iniciativas/{id}` · `POST …/{id}/ponto-situacao` | iniciativas |
| `POST /api/Importacoes` | importar qualquer modelo Excel (reconhecido pelo conteúdo) |
| `GET /api/Exportacoes/{modelo}` | exportar um modelo em Excel |
| `GET /api/Kpis/indicadores?ano=&periodo=` · `GET /api/Kpis/{n}/evolucao` | KPIs e evolução anual |
| `GET /api/Ideias/parecidas?titulo=` | ideias com título parecido |
| `GET /saude` | estado da aplicação (monitorização) |
| `GET /api/Notificacoes` · `POST …/{id}/lida` · `POST …/lidas` | avisos |

A documentação completa da API fica em `/scalar` com a API a correr.

## Equipa

Nuno Peixoto (gestão de projeto, backend/KPIs) · Junqing Chen (backend/BD) · Tomás Cunhas (frontend) ·
João Silva (DevOps/QA) · Diogo Canadas (frontend e qualidade)
