# Innovera — Requisitos e user stories (RF01–RF16)

Base: especificação funcional da dstelecom (01/10/2026), reuniões de 07/10 e 09/10/2026 e os modelos Excel do processo de inovação.
Perfis: **Administrador** (gere a plataforma, tem sempre todas as permissões), **Equipa de Inovação** e **Trabalhador**.
As permissões de cada perfil mudam-se no backoffice, sem alterar código.

Estado: ✅ feito · 🟡 em parte · ⏳ por fazer (sprint indicado)

| RF | Requisito | Estado |
|---|---|---|
| RF01 | Autenticação e gestão de contas | ✅ |
| RF02 | Criar, consultar e editar iniciativas por tipo e estado | ✅ |
| RF03 | Histórico e ponto de situação | ✅ |
| RF04 | Fluxo das ideias e avaliação em 5 variáveis | ✅ |
| RF05 | Conversão de ideia aprovada em iniciativa (ou arquivo) | ✅ |
| RF06 | Project Charter digital | ✅ |
| RF07 | Portefólios por categoria (Mod.246) | 🟡 consulta e importação feitas; exportação Excel no sprint 3 |
| RF08 | KPIs calculados automaticamente (Mod.239) | ⏳ sprint 3 |
| RF09 | Dados externos dos KPIs, manuais e parametrizáveis | ⏳ sprint 3 |
| RF10 | Conhecimento codificado automático (Mod.237) | 🟡 relatórios de entregáveis/milestones feitos; vigilâncias no sprint 3 |
| RF11 | Lições aprendidas agregadas (Mod.252) | ✅ |
| RF12 | Orçamento agregado (só o Administrador, por omissão) | 🟡 totais por projeto feitos; agregado global no sprint 3 |
| RF13 | Relatório semestral/anual pré-preenchido | ⏳ sprint 4 |
| RF14 | Registos manuais (estratégia, planeamento, tácito, ferramentas, parcerias, ativos) | ⏳ sprint 3 |
| RF15 | Links para documentos no OneDrive | ✅ |
| RF16 | Avisos prévios do planeamento anual (Mod.69) | 🟡 avisos na plataforma e por email feitos; ligação ao Mod.69 no sprint 3 |
| — | Registo de auditoria (ISO 56001) | ✅ |

---

## RF01 — Autenticação e contas

**US01.1** Como **Administrador**, quero adicionar um utilizador (nome, email, empresa, perfis) para que ele receba um convite por email e defina a sua password.
- Não há registo livre; só o Administrador cria contas.
- O link do convite vale 3 dias e pode ser reenviado.
- Só se entra com o email confirmado.

**US01.2** Como **utilizador**, quero entrar com email e password e recuperar a password se a esquecer.
- 5 tentativas erradas bloqueiam a conta 15 minutos.
- "Esqueci-me da password" envia um link por email, sem revelar se o email existe.

**US01.3** Como **Administrador**, quero desativar e reativar contas.
- Uma conta desativada perde a sessão em menos de 5 minutos.
- Não posso desativar a minha própria conta nem ficar sem nenhum Administrador ativo.

**US01.4** Como **Administrador**, quero criar perfis e escolher as permissões de cada um, para adaptar a plataforma à organização sem mudar o código.
- Os perfis do sistema não se apagam; o Administrador tem sempre todas as permissões.
- As alterações aplicam-se no pedido seguinte do utilizador.

## RF02 — Iniciativas por tipo e estado

**US02.1** Como **membro da Equipa de Inovação**, quero criar e editar projetos, desafios, melhorias contínuas, oportunidades, vigilâncias, mestrados e ações de inovação.
- O código é gerado por tipo (PRJ-001, DES-001, MC-001…).
- Os estados são: pendente, em análise, em curso, em implementação, concluída, rejeitada e cancelada.

**US02.2** Como **trabalhador**, quero consultar o portefólio e filtrar por tipo, estado e texto, para saber o que está a ser feito.
- Consulta para todos; criar e editar exige a permissão `iniciativas.gerir`.

## RF03 — Histórico e ponto de situação

**US03.1** Como **Equipa de Inovação**, quero registar o ponto de situação, para que fique o histórico de cada atualização, com autor e data.
- O último ponto de situação aparece na lista; os anteriores ficam no histórico da ficha.

## RF04 — Fluxo das ideias e avaliação

**US04.1** Como **trabalhador**, quero submeter uma ideia (simples ou detalhada, privada ou anónima, com coautores e anexos).
- Os coautores confirmam a autoria.
- A ideia segue para validação da Equipa de Inovação.

**US04.2** Como **Equipa de Inovação**, quero validar a ideia, para que abra o mês de discussão pública.

**US04.3** Como **trabalhador**, quero comentar e gostar das ideias em discussão.

**US04.4** Como **Equipa de Inovação**, quero registar a avaliação da Comissão Executiva nas 5 variáveis (custo, enquadramento, benefício, adequação técnica e incerteza, de 0 a 4).
- Nota ponderada: 0,15·custo + 0,25·enquadramento + 0,30·benefício + 0,20·adequação + 0,10·incerteza.
- Nota ≥ 2 → aprovada; < 2 → não aprovada.

**US04.5** Como **autor**, quero ser avisado (na plataforma e por email) das mudanças de estado da minha ideia.

## RF05 — Conversão de ideia

**US05.1** Como **Equipa de Inovação**, quero classificar uma ideia aprovada como projeto, desafio, melhoria contínua ou arquivada, para que, quando aplicável, seja criada a iniciativa correspondente ligada à ideia de origem.

## RF06 — Project Charter digital

**US06.1** Como **Equipa de Inovação**, quero preencher a informação geral do Charter: supervisor, fase, classe, contexto, business case, decisão de aprovação, objetivos e resultados esperados.

**US06.2** Como **Equipa de Inovação**, quero planear as tarefas (WBS) com datas planeadas e reais, responsável e milestone, e ver as tarefas atrasadas assinaladas.

**US06.3** Como **Equipa de Inovação**, quero registar entregáveis e milestones e colar o link do relatório no OneDrive quando ficam concluídos.

**US06.4** Como **Equipa de Inovação**, quero registar riscos com impacto e probabilidade (1–4).
- A severidade é calculada automaticamente.
- Os riscos com severidade acima de 4 e sem plano de ação ficam assinalados.

**US06.5** Como **Administrador**, quero registar o orçamento de cada projeto por categoria e ano (previsto e real) e ver os totais e a execução.
- Por omissão, só o Administrador vê e altera o orçamento; pode dar essa permissão a outros perfis (`orcamento.ver`, `orcamento.gerir`).
- Quem não tem permissão não vê o separador do orçamento.

**US06.6** Como **Equipa de Inovação**, quero registar os requisitos de conformidade (legal, qualidade, ambiente, segurança) e o seu estado.

**US06.7** Como **Equipa de Inovação**, quero registar lições aprendidas por categoria (estratégica, gestão, operacional, técnica).

- Só projetos e desafios têm Charter.
- Quem edita o Charter é a Equipa de Inovação (permissão `iniciativas.gerir`); não há um gestor por projeto com permissões próprias.
- Os restantes utilizadores consultam.

## RF07 — Portefólios (Mod.246)

**US07.1** Como **Equipa de Inovação**, quero importar o Excel Mod.246 atual, para começar com o histórico real.
- Importar duas vezes não duplica.
- As melhorias contínuas ficam ligadas às ideias de origem.

**US07.2** Como **Equipa de Inovação**, quero exportar o portefólio no formato do Mod.246 *(sprint 3)*.

## RF08 / RF09 — Indicadores (Mod.239) *(sprint 3)*

**US08.1** Como **Equipa de Inovação**, quero ver os 41 indicadores calculados a partir dos registos (ideias, projetos, conhecimento, parcerias…) por período, com o objetivo e o desvio.

**US09.1** Como **Administrador**, quero introduzir manualmente os valores externos (ex.: n.º de trabalhadores, volume de negócios) e mudar fórmulas e objetivos sem alterar o código.

## RF10 — Conhecimento codificado (Mod.237)

**US10.1** Como **Equipa de Inovação**, quero que o relatório de um entregável ou milestone, quando é registado, entre automaticamente no repositório de conhecimento codificado, com data, documento, iniciativa e link.
- Mudar o link atualiza a mesma entrada (não duplica).

**US10.2** Como **trabalhador**, quero consultar e procurar o conhecimento codificado.

**US10.3** Os resultados das vigilâncias entram no mesmo repositório *(sprint 3)*.

## RF11 — Lições aprendidas (Mod.252)

**US11.1** Como **trabalhador**, quero consultar num só sítio as lições aprendidas de todos os projetos, filtrando por categoria e texto.
- Cada lição nova avisa a Equipa de Inovação.

## RF12 — Orçamento agregado

**US12.1** Como **Administrador**, quero ver o orçamento de inovação previsto e real por ano, somando os Charters de todos os projetos *(sprint 3)*.
- Mesmas permissões do orçamento de cada projeto: por omissão só o Administrador.

## RF13 — Relatório de inovação *(sprint 4)*

**US13.1** Como **Equipa de Inovação**, quero gerar o relatório semestral/anual já preenchido com os números do período e campos livres para a análise.

## RF14 — Registos manuais *(sprint 3)*

**US14.1** Como **Equipa de Inovação**, quero registar a estratégia (Mod.52), o planeamento anual (Mod.69), o conhecimento tácito (Mod.238), as ferramentas e métodos (Mod.241), as parcerias (Mod.235) e os ativos intangíveis (Mod.236), em formulários estruturados que alimentam os KPIs.

## RF15 — Links OneDrive

**US15.1** Como **Equipa de Inovação**, quero juntar links para documentos no OneDrive/SharePoint à ficha da iniciativa.
- A plataforma guarda só o título e o link, não o ficheiro.
- Só aceita endereços http(s).

## RF16 — Avisos

**US16.1** Como **utilizador**, quero receber avisos na plataforma (sino) e por email das ações que me dizem respeito.

**US16.2** Como **Equipa de Inovação**, quero ser avisada antes das atividades do planeamento anual (ex.: avaliação mensal com a CE) *(sprint 3, com o Mod.69)*.

## Requisitos não funcionais / ISO 56001

**USA.1** Como **Administrador**, quero consultar o registo de auditoria (quem criou, alterou ou eliminou o quê e quando, com os valores antes e depois), para ter evidência para as auditorias da norma.
- Ver o registo exige a permissão `auditoria.ver`.
- Não regista likes nem avisos.

**USA.2** A plataforma corre em HTTPS, com backups diários da base de dados e dos ficheiros, guardados 14 dias (ver `docs/publicacao-aws.md`).
