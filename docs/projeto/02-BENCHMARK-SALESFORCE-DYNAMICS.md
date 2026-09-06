# Benchmark — Salesforce × Dynamics 365 × Vórtice

> **Documento 02 de 07** · Versão 1.0 · 30/08/2026
> Pesquisa na documentação oficial (developer.salesforce.com, learn.microsoft.com).
> Detalhe completo: [`docs/pesquisa/`](../pesquisa/), documentos 05, 08, 10, 11, 12, 13, 14, 15.
> **Decisão de cada linha registrada na coluna "CRM Tracbel".**

---

## 1. Linguagens e arquitetura de execução

| | Salesforce | Dynamics 365 CE | CRM Tracbel |
|---|---|---|---|
| Servidor | **Apex** (OO, tipada, sintaxe Java) | **C# / .NET** (plugins) | **C# / .NET 8** |
| Cliente | LWC (JS + HTML + CSS) | JavaScript (form scripts) | **React + TypeScript** |
| Low-code | Flow | Power Fx, Business Rules | — (fase futura) |
| Consulta | SOQL / SOSL | FetchXML / OData | **LINQ + EF Core** |

**O padrão que os dois seguem:** linguagem de tipagem estática e classes no servidor, JavaScript
confinado à tela. **Nenhum dos dois roda JS no servidor.** Foi essa constatação que decidiu a stack
deste projeto.

---

## 2. Modelo de dados

| Tema | Salesforce | Dynamics | Vórtice | **CRM Tracbel** |
|---|---|---|---|---|
| Pré-relacionamento | `Lead` flat e autossuficiente | `Lead` | `GE_Pessoa` com `Status='P'` misturado | **`crm.Lead` flat** `[SF]` |
| Qualificação | conversão **irreversível**, trava o Lead | `QualifyLead`, **o Lead sobrevive**, link por `originatingleadid` | não existe conceito | **Lead sobrevive** `[DYN]` |
| Conta / contato | `Account` + `Contact` + `AccountContactRelation` (N:N) | `Account` + `Contact` | contato é **entidade fraca**, 625 órfãos | **N:N com papel** `[SF]` |
| Atividade | `Task`/`Event` com `WhoId`/`WhatId` polimórficos | **`ActivityPointer` é herança de tabela real** + `ActivityParty` (13 tipos) | `IV_Historico` | **TPH + `AtividadeParticipante`** `[DYN]` |
| Equipamento do cliente | `Asset` + `AssetRelationship`, hierarquia até 10.000 | `msdyn_customerasset` com **duas hierarquias** + Work Order | `EXT_Veic` | **duas hierarquias** `[DYN]` |
| Carteira multi-linha | não tem nativo | não tem nativo | **`IVS_Pes`, até 7 por pessoa** | **copiado do Vórtice** ✅ |
| Campo customizado | linha em `MT_Fields` + slot EAV | metadados | **tabela física por formulário** (175) | **coluna real + catálogo** |
| Chave de integração | `External ID` (25/objeto), upsert idempotente | `Alternate Key` (10, 900 bytes) | string concatenada por pipes | **`intg.ChaveExterna`** |

### O preço do modelo por metadados

O Salesforce cria campo **sem DDL**: insere linha em `MT_Fields` e aluga um slot `Value{N}` numa tabela
EAV única (`MT_Data`), com pivôs tipados para permitir índice nativo. É elegante — e cobra caro:

| Edição | Teto de campos por objeto |
|---|---|
| Professional | 100 |
| Enterprise | **500** |
| Unlimited / Performance | 800 |

E **o Suporte não aumenta.** Confirmado na fonte primária.

> **Decisão Tracbel:** copiar o **catálogo** (`meta.CampoEntidade` — a UI monta a tela a partir do
> metadado), **descartar o motor EAV**. Com SQL Server e EF Core, coluna real + migration entrega
> ~90% do benefício, com ~5% da complexidade e **sem teto**.

---

## 3. Segurança — a comparação que mais importa

| Camada | Salesforce | Dynamics | Vórtice | **Tracbel** |
|---|---|---|---|---|
| Autenticação | SSO + MFA obrigatório por contrato | Entra ID + Conditional Access | **senha sem salt**, 80 usuários com o mesmo hash | **Entra ID** |
| Objeto | Profile + Permission Sets (aditivo) | Security Role: 8 verbos | 8 camadas, nenhuma por registro | **`ConjuntoPermissao`** `[SF]` |
| Campo | Field-Level Security | Column Security Profile | **17 linhas**, presa ao widget Gupta | **campo sensível** `[DYN]` |
| Registro | OWD + Role Hierarchy + **300 sharing rules/objeto**, materializadas em tabelas de share | **matriz privilégio × profundidade** (5 níveis) resolvida por colunas da própria linha, **sem join** | **não existe** | **profundidade** `[DYN]` |
| Hierarquia | Role Hierarchy | Manager / Position (máx. **50 usuários** efetivos) | ponteiro de 1 nível, sem reatribuição | **closure table** |
| Compartilhamento | share table com `RowCause` tipado | `PrincipalObjectAccess` (a Microsoft nem deixa limpar por `DELETE`) | não existe | **share com motivo** `[SF]` |
| Negação | Restriction Rules (**deny real**) | **não existe DENY** | — | **sem deny**, de propósito |
| Auditoria | Field History (20 campos, 18 meses) + Setup Audit Trail (180 dias) | Dataverse Audit + Purview | 44,8M linhas, **sem retenção**; **114 logins em 9 anos** | **escopada + retenção** `[SF]` |

### O que cada um faz melhor

**Dynamics ganha:** a matriz privilégio × profundidade é mais simples e mais barata que OWD + role
hierarchy + sharing rules, porque resolve com `OwnerId` e `OwningBusinessUnit` **na própria linha**. A
própria Microsoft publica a hierarquia de custo: *Organization é o mais rápido; Sharing é o mais caro.*

**Salesforce ganha:** sharing rules declarativas por critério e restriction rules com negação real. O
Dataverse não tem equivalente declarativo.

**Onde os dois concordam:** permissão é **aditiva**; acesso a registro parte do piso mais restritivo e
só é **aberto**. `[DYN]` é explícito: *"se você deu Organization Read em contato, não dá pra esconder um
registro depois."*

> **Decisão Tracbel:** matriz de profundidade do Dynamics + share table com motivo tipado do Salesforce.
> **Sem regra de negação** — ela torna a ordem de avaliação imprevisível e é a origem de metade dos bugs
> de autorização.

---

## 4. Extensibilidade e automação

| Tema | Salesforce | Dynamics | Vórtice | **Tracbel** |
|---|---|---|---|---|
| Ponto de extensão | trigger + handler registrado em `TriggerAction__mdt` | **`SdkMessageProcessingStep`** — linha de tabela | `IV_AcaoAuto`, sem condição | **`meta.ManipuladorEvento`** `[DYN]` |
| Estágios | Order of Execution de **20 etapas** | PreValidation · PreOperation · MainOperation · PostOperation | não há conceito | **3 estágios** `[DYN]` |
| Contrato de código | classe Apex | **`IPlugin.Execute(IServiceProvider)`** | — | **`IManipuladorEvento<T>`** |
| Otimização | — | **filtering attributes** ("decide a performance de todo o sistema") | — | **`CamposFiltro`** `[DYN]` |
| Condição em regra | Flow / Validation Rules | Business Rules (máx. 150/tabela) | **não existe** — `IV_AcaoAutoCtrl` vazia | **expressão em `Regra.Condicao`** |
| Anti-loop | governor limits | **`depth` máx. 8** | não existe | **`depth` máx. 8** `[DYN]` |
| Timeout | 10.000 ms CPU | **2 min duro, 2 s recomendado** | não existe | **2 s alvo, 30 s duro** |
| Assíncrono | Queueable, Batchable, Platform Events | só em `PostOperation` | jobs `GEP_` sem alarme | **outbox + Hangfire** |
| Configuração sem release | Custom Metadata Types (`__mdt`) — leitura **não consome limite de query** | tabela de steps | Objetos Dinâmicos = **SQL solto no banco** (196) | **tabela de registro + cache** |

### Governor limits — copiar a disciplina, não os números

Salesforce impõe 100 SOQL, 150 DML, 10.000 ms de CPU, 6 MB de heap por transação. Esses números existem
por **multitenancy** e não fazem sentido num sistema single-tenant.

Mas a **disciplina que eles forçaram** é o que salvou a plataforma: bulkificação obrigatória, proibição
de query em loop, handler que sempre recebe lista. É exatamente o que falta no Vórtice.

> **Decisão Tracbel:** adotar a disciplina como regra de code review, e um orçamento de performance
> observável (alerta quando um handler passa de 2 s), sem limite artificial que aborte transação.

### A matriz de densidade da Salesforce

Critério objetivo publicado pela própria Salesforce para escolher configuração vs código:

| Automações por objeto | Recomendação |
|---|---|
| < 15 | Flow |
| 15 – 30 | Flow + Apex invocável |
| > 30 | framework de trigger em Apex |

E a orientação atual: **Workflow Rules e Process Builder sem suporte desde 31/12/2025.**

> **Decisão Tracbel:** **não construir um Flow Builder visual.** Com 3 devs, isso é um produto dentro
> do produto. Construir o pipeline + tabela de registro de handlers + outbox, e deixar "configuração"
> significar ordenar, ligar, desligar e parametrizar — não desenhar fluxograma.

---

## 5. Experiência do usuário

| Ideia | Origem | O que é | Tracbel |
|---|---|---|---|
| **Business Process Flow** | `[DYN]` | barra de estágios com **tabela de instância própria** que grava o **caminho percorrido** (`traversedpath`), não só o ponto atual. 30 estágios × 30 passos, 5 tabelas, 10 BPFs ativos | ✅ **fase 1** |
| **Compact Layout** | `[SF]` | 10 campos configurados, 7 exibidos no topo do registro. "A melhor razão esforço/benefício de UX de todo o Salesforce" | ✅ **fase 1** |
| **Path + Kanban** | `[SF]` | compartilham **a mesma configuração** — um metadado, duas telas | ✅ **fase 1** |
| **List Views** | `[SF]` | a interface de trabalho real. Relatório é o caso raro | ✅ **fase 1** |
| **Work list + "Up next"** | `[DYN]` | fila priorizada + próxima ação com Concluir/Pular | ✅ **fase 1** |
| **Timeline** | `[DYN]` | posts, notas e atividades unificados, paginação 10→50, filtro e pin | ✅ **fase 1** |
| **Dynamic Forms** | `[SF]` | visibilidade condicional substitui N layouts × M record types | ✅ fase 2 |
| **Focused View** | `[DYN]` | lista e registro na mesma tela | ✅ fase 2 |
| **Editable grid** | `[DYN]` | edição inline com 20+ propriedades declarativas | 🔸 fase 3 |
| **Report Type + builder** | `[SF]` | admin cura o conjunto; usuário monta sem ver schema | ✅ fase 5 |
| PCF / canvas apps | `[DYN]` | componentes customizados | ❌ em React basta um registry por nome |
| Power BI embedded | `[DYN]` | — | ❌ biblioteca de charts React |
| Mobile offline | ambos | `[SF]` Briefcase: teto de 2.000 registros, **recomendado 500** | ❌ **18 limitações documentadas**; explica a dor do Vórtico Mobile |
| Einstein | `[SF]` | exige **200 ganhas + 200 perdidas em 24 meses**, US$ 50/usuário/mês | ❌ overkill declarado |
| Console Apps | `[SF]` | padrão de call center | ❌ não é venda de campo |
| Playbooks | `[DYN]` | **descontinuado pela própria Microsoft** | ❌ |

---

## 6. Plataforma e operação

| Tema | Salesforce | Dynamics | **Tracbel** |
|---|---|---|---|
| Ambientes | 4 níveis de sandbox com refresh intervals | ambientes por solution | **3: dev, homolog, prod** |
| Deploy | Change Sets (antipadrão) ou SFDX | Solutions managed/unmanaged, com camadas | **migrations + CI/CD** |
| Gate de teste | **75% de cobertura** obrigatório | não tem | **90% no domínio** |
| Limite de deploy | 39 MB comprimido, ~600 MB descomprimido | 16 MB por assembly | — |
| API de integração | REST, SOAP, Bulk, Streaming, Composite (25 subrequisições) | Web API OData v4, **6.000 req/300s, 52 concorrentes/usuário** | **REST versionada + outbox** |
| Eventos | Platform Events, CDC (**72h de retenção**, entrega contada por assinante) | webhooks (256 KB), Service Bus (192 KB), Change Tracking (janela de **7 dias**) | **outbox + webhook** |
| Qualidade de dado | Duplicate Rules + Matching Rules | — | **fase 1** |
| Arquivamento | Big Objects | — | **partição + arquivo** |

---

## 7. O custo de não construir

| Opção | Custo anual |
|---|---|
| **Salesforce Sales Cloud Enterprise** | US$ 175/usuário/mês × 130 = **~US$ 273 mil/ano** (≈ R$ 1,5 milhão) só de licença |
| Dynamics 365 Sales Enterprise | faixa comparável |
| Construir | custo do time — e o ativo fica na Tracbel |

E há um custo que nenhum dos dois resolve: **a carteira multi-linha-de-negócio**, que é como a Tracbel
Agro realmente opera, não existe nativamente em nenhum deles. Seria customização paga em qualquer
cenário de compra.

---

## 8. Placar das decisões

| Decisão | Escolhido | De onde |
|---|---|---|
| Linguagem do servidor | C# / .NET 8 | `[DYN]` + infra da Tracbel |
| Linguagem da tela | React + TypeScript | `[SF]` LWC · `[DYN]` form scripts |
| Modelo de Lead | flat, autossuficiente | `[SF]` |
| Qualificação | lead sobrevive | `[DYN]` |
| Atividade | TPH + participantes tipados | `[DYN]` |
| Segurança por registro | matriz privilégio × profundidade | `[DYN]` |
| Compartilhamento | share table com motivo | `[SF]` |
| Negação | **não implementar** | `[DYN]` |
| Extensão | handler registrado em tabela, 3 estágios | `[DYN]` |
| Metadados | catálogo sim, EAV não | `[SF]` invertido |
| Guia de processo | barra de estágios com caminho percorrido | `[DYN]` |
| Resumo do registro | 7 campos no topo | `[SF]` |
| Relatório | fonte curada + builder | `[SF]` |
| Carteira multi-linha | manter | **Vórtice** |
| Duplo ponteiro tarefa↔atividade | manter | **Vórtice** |
| Previsão vs baseline | manter | **Vórtice** |
