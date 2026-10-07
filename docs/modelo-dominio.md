# Modelo de domínio — Innovera

Alinhado com a proposta de arquitetura **v1.3** (secção 3.4, Figura 2 e Anexo C).
Todas as entidades estão em `src/Domain` (namespace `Innovera.Domain.Entities`), organizadas por pasta.

| Pasta | Entidades | Modelo Excel |
|---|---|---|
| `Ideias` | `Ideia` (formulário de 4 passos), `AutorIdeia`, `AnexoIdeia`, `AvaliacaoIdeia`, `ComentarioIdeia`, `GostoComentario`, `LikeIdeia` | Mod.246 (Ideias) |
| `Iniciativas` | `Iniciativa` (base) → `Projeto`, `Desafio` (com Charter), `MelhoriaContinua`, `Oportunidade`, `Vigilancia` (+ `ExecucaoVigilancia`), `Mestrado`, `AcaoInovacao`; `MembroEquipa`, `AlocacaoMensal`, `NotaPontoSituacao` | Mod.246 |
| `ProjectCharter` | `ProjectCharter`, `TarefaWbs`, `Entrega` (entregável/milestone), `Risco`, `LinhaOrcamento`, `RequisitoConformidade`, `LicaoAprendida` | Project Charter, Mod.252 |
| `Registos` | `Parceiro`, `AcordoParceria`, `AtivoIntangivel`, `ConhecimentoCodificado`, `ConhecimentoTacito`, `FerramentaMetodo`, `LinhaEstrategia`, `AtividadePlanoAnual` | Mod.235, 236, 237, 238, 241, 52, 69 |
| `Kpis` | `KpiDefinicao`, `ValorKpi`, `AcaoKpi`, `ValorExterno` | Mod.239 |
| `Transversais` | `Utilizador`, `Alerta`, `OutputGerado`, `RegistoAuditoria`, `LinkDocumento`, `AtividadeGestao` | — |

## Regras de negócio já no domínio (com testes em `tests/Domain.UnitTests`)

- Código das ideias AA01 … ZZ99 (`Services/GeradorCodigoIdeia`).
- Ciclo da ideia igual ao site atual: validação dos autores → validação da equipa → discussão pública (30 dias) → em avaliação pelo manager → aprovada / não aprovada. Classe: aprovada → Melhoria Contínua, Projeto, Desafio ou Arquivada; não aprovada → Ideia Duplicada ou N.A. Ideias "dst" só registam a decisão (sem notas).
- Nota da CE com pesos configuráveis (`ValueObjects/PesosAvaliacao`: Mod.246 15/25/30/20/10 ou média simples) e aprovação com nota ≥ 2.
- Risco: I × P, mitigação obrigatória quando I × P > 4, reavaliação.
- Eventos de domínio: `IdeiaAvaliadaEvent`, `LicaoRegistadaEvent` (→ Mod.252), `RelatorioDeMilestoneRegistadoEvent` e `ResultadoVigilanciaRegistadoEvent` (→ Mod.237).

## Correspondência com a primeira versão

| Antes | Agora |
|---|---|
| `Ideia : Iniciativa` | `Ideia` separada; `Iniciativa.IdeiaOrigemId` liga a iniciativa à ideia |
| `Evento`, `Ações de Inovação.cs` | `AcaoInovacao` (tipo criada/externa, n.º de participantes) |
| `Atividade` | `AtividadePlanoAnual` (Mod.69) e, no Charter, `TarefaWbs` / `Entrega` |
| `Orçamento` | `LinhaOrcamento` (previsto vs. real por ano) |
| `Licao_Aprendida` + `Categoria` | `LicaoAprendida` + `Enums/CategoriaLicao` |
| `Projeto.MatrizRiscos`, `NotasConformidade` (texto) | `Risco`, `RequisitoConformidade` |
| `Indicador` | `KpiDefinicao` + `ValorKpi` (por período) + `AcaoKpi` + `ValorExterno` |
| `Conhecimento` | `ConhecimentoCodificado` e `ConhecimentoTacito` |
| `Documento` | `LinkDocumento` (iniciativas) e `AcordoParceria` (acordos) |
| `EstrategiaAnual` | `LinhaEstrategia` (uma linha do Mod.52, com ano) |
| `Ativo_intangivel` | `AtivoIntangivel` (tipo + entidade de registo + validade) |
| `FerramentaModulo.cs` | `FerramentaMetodo` (+ análise anual) |
| `AvaliacaoIdeia` (vazio), `EstadoIdeia` (classe vazia) | `AvaliacaoIdeia`, `Enums/EstadoIdeia` |

Nomes sem acentos nem underscores, um ficheiro por classe com o mesmo nome.

## Base de dados (PostgreSQL)

- EF Core 10 + Npgsql. Configurações em `src/Infrastructure/Data/Configurations` (uma por pasta do domínio).
- `Iniciativa` em TPH (uma tabela, coluna `Discriminador`); `AtividadeGestao` e `AnaliseAnual` com `OwnsMany`.
- Número das ideias numa sequência do PostgreSQL (`ideia_numero_seq`) → código AA01 … ZZ99.
- Like único por ideia + utilizador; gosto único por comentário + utilizador.
- Apagar: filhos em cascata (anexos, comentários, likes…); referências a utilizadores ficam a `NULL` ou bloqueiam.
- Migração inicial em `src/Infrastructure/Data/Migrations` (gerada com `dotnet ef`, ver README).

## Casos de uso já feitos (Sprint 1 — ideias)

`src/Application/Ideias` e `src/Application/Utilizadores`, expostos em `/api/Ideias` e `/api/Utilizadores`:
registar ideia (4 passos, coautores, anexos até 25 MB), confirmar autoria, validar (equipa de Inovação),
likes e comentários durante a discussão, fecho automático aos 30 dias (`Infrastructure/Jobs/FecharDiscussoesJob`),
avaliação pela CE (notas ou decisão dst), classe e criação do Projeto/Desafio/Melhoria Contínua ligado à ideia.

## Próximos passos

1. Projetos e Project Charter (Sprint 2), desafios e vigilâncias.
2. KPIs e relatórios (Mod.239).
3. Na AWS: trocar `ArmazenamentoLocal` pelo bucket e enviar emails pelo SES.
