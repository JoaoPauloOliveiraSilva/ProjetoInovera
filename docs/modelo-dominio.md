# Modelo de domínio — Innovera

Alinhado com a proposta de arquitetura **v1.3** (secção 3.4, Figura 2 e Anexo C).
Todas as entidades estão em `src/Domain` (namespace `Innovera.Domain.Entities`), organizadas por pasta.

| Pasta | Entidades | Modelo Excel |
|---|---|---|
| `Ideias` | `Ideia`, `AvaliacaoIdeia`, `ComentarioIdeia`, `LikeIdeia` | Mod.246 (Ideias) |
| `Iniciativas` | `Iniciativa` (base) → `Projeto`, `Desafio` (com Charter), `MelhoriaContinua`, `Oportunidade`, `Vigilancia` (+ `ExecucaoVigilancia`), `Mestrado`, `AcaoInovacao`; `MembroEquipa`, `AlocacaoMensal`, `NotaPontoSituacao` | Mod.246 |
| `ProjectCharter` | `ProjectCharter`, `TarefaWbs`, `Entrega` (entregável/milestone), `Risco`, `LinhaOrcamento`, `RequisitoConformidade`, `LicaoAprendida` | Project Charter, Mod.252 |
| `Registos` | `Parceiro`, `AcordoParceria`, `AtivoIntangivel`, `ConhecimentoCodificado`, `ConhecimentoTacito`, `FerramentaMetodo`, `LinhaEstrategia`, `AtividadePlanoAnual` | Mod.235, 236, 237, 238, 241, 52, 69 |
| `Kpis` | `KpiDefinicao`, `ValorKpi`, `AcaoKpi`, `ValorExterno` | Mod.239 |
| `Transversais` | `Utilizador`, `Alerta`, `OutputGerado`, `RegistoAuditoria`, `LinkDocumento`, `AtividadeGestao` | — |

## Regras de negócio já no domínio (com testes em `tests/Domain.UnitTests`)

- Código das ideias AA01 … ZZ99 (`Services/GeradorCodigoIdeia`).
- Ciclo da ideia: Submetida → Em discussão (30 dias) → Em avaliação → Aprovada / Não aprovada; destino Projeto, Desafio, Melhoria Contínua ou Arquivada; ideias duplicadas ou "dst" ficam sem avaliação.
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

## Próximos passos (depois da reunião com a dstelecom)

1. Fechar a base de dados (SQL Server/Azure SQL, como na proposta) e o login (Entra ID ou contas locais).
2. Acrescentar os `DbSet` ao `IApplicationDbContext`, as configurações EF (`Infrastructure/Data/Configurations`: TPH para `Iniciativa`, `OwnsMany` para `AtividadeGestao` e `AnaliseAnual`, índice único em `LikeIdeia` (ideia + utilizador)) e a primeira migração.
3. Casos de uso do Sprint 1 (ideias) na camada Application.
