# Briefing — CRM próprio da Tracbel

> **Documento 00 de 07** · Versão 1.0 · 30/08/2026
> Base: investigação do banco de produção do Vórtice (767 tabelas, 85,5M linhas) e pesquisa
> na documentação oficial de Salesforce e Microsoft Dynamics 365 / Dataverse.
> Todo número deste documento foi medido ou confirmado em fonte primária. Nada é estimativa.

---

## 1. Em uma página

A Tracbel opera hoje sobre um CRM (Vórtice/Vórtico 4.04.01) cujo **modelo conceitual é bom e cuja
implementação está em falência técnica medida**: a integração financeira está parada há 15 meses,
52% do banco é log, a senha dos usuários não tem salt, o motor de relatórios executa `UPDATE`, e
não existe permissão por registro.

A alternativa de mercado custa caro: Salesforce Sales Cloud Enterprise está a **US$ 175 por usuário/mês
em 2026** — para ~140 usuários, **~US$ 294 mil por ano**, ou cerca de **R$ 1,6 milhão anuais** só de
licença, sem contar implantação e customização.

> **Número medido em 31/08/2026:** o Vórtice tem **1.386 usuários cadastrados**, dos quais
> **≈140 logaram nos últimos 90 dias**. É esse o parque real. Ver [documento 11](11-MODULOS-TELAS-PERMISSOES.md).

Este projeto propõe uma terceira via: **construir o CRM da Tracbel**, copiando deliberadamente o que
Salesforce e Dynamics acertaram em 25 anos, corrigindo o que o Vórtice errou, e mantendo o escopo no
tamanho de um time de três pessoas. A substituição é **gradual** (padrão *strangler fig*): o Vórtice
continua vivo enquanto o sistema novo assume um domínio por vez, começando por Leads e Prospecção.

---

## 2. Por que agora — o estado real do Vórtice

Não é opinião. Cada linha abaixo foi confirmada por consulta ao banco de produção.

### 2.1 A integração com o ERP está morta, e ninguém foi avisado

| O que | Medida |
|---|---|
| Títulos presos em `IMP_Titulo`, nunca promovidos a `EXT_Titulo` | **783.242 títulos · R$ 5,18 bilhões**, parados desde **22/05/2025** |
| Ordens de serviço que nunca chegaram a `EXT_OS` | **280.214 de 287.868** — e as linhas estão marcadas como "processadas" |
| Faturamento | morreu em **três datas distintas**: serviços 29/08/2024, máquinas 07-08/02/2025, peças 11/04/2025 |
| Job `RUNGEPIMPORT` | falha **a cada 20 minutos há meses** com `Versão incompatível [4.04.01r05] x [4.04.01r01]` — registrado apenas como **Warning** |
| Integração John Deere / JD Edwards | **7 tabelas `JDE_*`, todas com zero linhas.** O job existe no catálogo e nunca foi agendado |
| ETL Pentaho (quarta camada de integração) | **parado desde julho/2021** |

A consequência é direta: **a visão financeira do CRM está cega há 15 meses.** Qualquer análise de
faturamento, inadimplência ou pós-venda feita no Vórtice hoje está errada.

E o defeito estrutural por trás disso: as procedures da camada de integração escrita pela Tracbel
**não têm transação nem `TRY/CATCH`** — o `BEGIN TRANSACTION` está literalmente comentado no código.

### 2.2 O CRM e o ERP são duas ilhas

Não existe join entre documento e oportunidade. `EXT_*` (ERP) e `IV_*` (CRM/BPM) se ligam **apenas por
`SeqPessoa`**: não há `IdNFS`, `IDOS` nem `idTitulo` em `IV_Historico` ou `IV_ProcDado`, e **99,1% das
notas fiscais não têm `IdVeic`**. A ponte real entre a venda e a nota é um **formulário digitado à mão**
(`IV_Q_ACOMP_VENDA_FINANC`: chassi, número da NF, data de faturamento).

### 2.3 A segurança não sustenta uma auditoria

| Achado | Medida |
|---|---|
| Senha sem salt | **663 usuários com senha produzem apenas 467 valores distintos.** **80 usuários compartilham exatamente o mesmo valor armazenado** |
| Política de senha | existe em **1 das 11 políticas**, com força declarada "Fraco" e **sem expiração** |
| Auditoria de login | **114 eventos em 9 anos.** Login praticamente não é auditado |
| Permissão por registro | **não existe.** Sem RLS no SQL Server (`sys.security_policies` = 0), **um único trigger no banco inteiro** |
| Onde a segurança é aplicada | 100% no cliente Gupta. **Acesso direto ao banco ignora tudo e não deixa rastro** |
| Token da API | gravado **em texto claro** numa tabela de parâmetros |
| Papéis/perfis | **não existem.** 939 usuários produzem **370 combinações distintas** de 51 flags em `IV_Operador` — cada usuário é único |
| Contas | **70% nunca logaram**; 87% estão dormentes há mais de 90 dias |
| Desativar usuário | **destrói as permissões** — não há flag de ativo/inativo |

### 2.4 A LGPD é decorativa

As views de anonimização (`GE$PESSOA_LGPD`, `GE$CONTATO_LGPD`, `GE$EMAIL_LGPD`, `GE$PESSOAFONE_LGPD`)
mascaram a coluna **e republicam o valor cru na mesma view**, em colunas `z_NOMERAZAO`, `z_NROCGCCPF`,
`z_EMAIL`, `z_FONENRO1`. A anonimização é contornável com um `SELECT`.

Além disso, o opt-in de comunicação **não tem data, não tem origem e não tem prova de consentimento** —
apenas um flag. Isso não sustenta uma solicitação de titular sob a LGPD.

### 2.5 O motor de relatórios é um executor de SQL arbitrário

O módulo QVW guarda **134 relatórios como SQL cru** na coluna `GE_QVCons.InstrSql`, sobrescrita a cada
edição. Consequências medidas:

- **Sem versionamento.** A tabela de histórico tem 69 linhas para 134 relatórios; a última é de abr/2025.
  Clonagem substituiu versionamento — há **9 gerações** do mesmo "Análise da Carteira de Pedidos".
- **Sem segurança de linha.** **125 dos 134 relatórios não têm nenhum predicado de usuário.** O picklist
  de "Carteiras" devolve a carteira de todo mundo.
- **O motor aceita `UPDATE`, `INSERT`, `EXEC` e `COMMIT`.** Existem "relatórios" que são DML disfarçado.
- **Staging nunca limpa.** Em nenhum dos 134 corpos existe um `DELETE FROM`. `IMP_REL_TBA101_PG2` saiu
  de 208 para 1.558 linhas entre junho e agosto de 2026.
- **Só 10 dos 134 relatórios rodaram nos últimos 3 meses.** 26 nunca rodaram.

### 2.6 O motor de workflow não tem linguagem de condição

Esta é a causa raiz da maior parte dos chamados. A tabela que guardaria condições (`IV_AcaoAutoCtrl`)
está **vazia**, e `UsaObjDyn = 0` em 100% das linhas. O único discriminante real de uma regra é o par
`(Resultado, NroEmpresa)`.

Sem condição, a única saída é **replicar a regra**: **5.958 regras cobrem apenas 1.262 pares
(Resultado, Ação) distintos** — uma cópia por filial, com o destinatário fixo na linha. É por isso que
manter o fluxo é caro e que uma mudança simples exige tocar dezenas de registros.

### 2.7 O banco está apodrecendo

| Achado | Medida |
|---|---|
| Log de auditoria | **44,8M de 85,5M linhas — 52,4% do banco inteiro** |
| `GE_LOG_PROCESSO` | **96,8% são re-carimbos** `(Atualizado em ...)` de um job de cobrança: 617 linhas de log por processo, em 19.936 processos |
| `GE_LgTb` | **11,9M linhas congeladas desde jun/2023**, nunca expurgadas |
| Tabelas `IV_` vazias | **131 de 377 (35%)** |
| Catálogo em uso | **980 ações, só 169 usadas em 2026.** **4.208 resultados, 501 usados em 2026** |
| Formulários geram DDL | cada formulário vira **uma tabela física**: **175 tabelas `IV_Q_*`** e 175 views, nunca coletadas quando o formulário morre |
| Órfãos | **345.535 linhas** de `IV_ProcDado` sem processo correspondente |
| Módulos entregues e nunca ativados | `IVF_` (financiamento), `IVM_`, `IVP_`, `GEL_` — **34 tabelas com zero linhas** |
| Motor de RFV/segmentação | **100% morto**: as 10 colunas de score em `IVS_Pes` nunca foram calculadas |
| Fila de deduplicação | **124 mil pares abandonados desde 2018** |

### 2.8 A entrega é frágil por construção

O cliente principal é um executável **Gupta/Team Developer de 32 bits** publicado via RemoteApp em um
terminal server que **vive com o disco lotado** (chegou a 0,42 GB livres em 129 GB, com 66 GB só em
perfis de usuário) e **sem backup** — a tarefa de System State está desabilitada.

---

## 3. O que o Vórtice ACERTOU — e que vamos copiar

Seria desonesto listar só os defeitos. O modelo conceitual do Vórtice é bom, e é a razão de ele atender
seis fluxos muito diferentes com um núcleo só.

**As cinco entidades.** Processo (o caso, com estado), Agenda (a tarefa atribuível), Histórico (o fato
imutável), Ação (o catálogo de tipos de tarefa) e Resultado (o catálogo de desfechos). É um autômato
correto, e é mais claro do que o equivalente no Salesforce.

**O duplo ponteiro rastreável.** `Agenda.HistoricoOrigem → Historico` e `Historico.AgendaOrigem → Agenda`.
Medido: 684.110 de 931.989 agendas nasceram de um histórico (73%); 1.423.965 de 2.436.127 históricos
vieram de uma agenda (58%). É esse par que permite responder *"quem gerou essa tarefa e por quê"* em uma
única query — e foi ele que resolveu os dois casos mais difíceis do suporte.

**A separação transição/geração.** O resultado dispara dois mecanismos independentes: um muda a fase do
processo, outro cria a próxima tarefa. A separação é correta; o erro foi não observar quando um dispara
e o outro não.

**A carteirização multi-linha-de-negócio.** `IVS_Pes` permite o mesmo cliente em até 7 carteiras, uma por
departamento (MAQ, PEÇ, DSI, PNEUS, PUK), com chave real. Nem Salesforce nem Dynamics fazem isso
nativamente — é um ativo conceitual da Tracbel e vai para o modelo novo.

**Previsão versus baseline.** `DtaPrevConclusao` e `DtaPrevConcOrig` guardam a previsão atual *e* a
original. Poucos sistemas fazem isso, e é o que permite medir derrapagem de prazo.

**Natureza e geolocalização do atendimento.** Ativo/receptivo e latitude/longitude no histórico — dados
que a operação de campo agrícola usa de verdade.

**A camada semântica `IV$S_*`.** Views que traduzem código em rótulo legível. A ideia é ótima (é o
embrião de um *report type*); a execução tem bugs — `POSSUI_EMAIL` está com a lógica invertida.

---

## 4. O que aprendemos de Salesforce e Dynamics

Pesquisamos os dois na documentação oficial. O resumo do que importa:

### 4.1 As três ideias que os dois compartilham

**1 — A plataforma é dirigida por metadados.** No Salesforce, criar um campo **não executa DDL**: insere
uma linha em `MT_Fields` e aluga um slot `Value{N}` numa tabela EAV única (`MT_Data`), com pivôs tipados
para permitir índice. É isso que permite "criar funcionalidade sem release" — e é também a origem dos
tetos duros (100 campos por objeto no Professional, 500 no Enterprise, 800 no Unlimited; **o Suporte não
aumenta**). O Dynamics faz o mesmo por outro caminho: "os componentes que os makers usam para criar um
app **tornam-se os metadados**".

> **Nossa decisão:** copiar o **catálogo de metadados**, não o motor EAV. Com SQL Server e EF Core,
> colunas reais mais migrations entregam ~90% do benefício com ~5% da complexidade — e sem teto de campos.

**2 — Permissão é aditiva; restrição é a exceção.** Nos dois sistemas, permissão de objeto e campo é
união (vence a mais permissiva) e funciona como **teto**; acesso a registro parte do piso mais restritivo
e só é **aberto** pelas camadas seguintes. O Dataverse é explícito: **não existe DENY em lugar nenhum**.

**3 — O ponto de extensão é uma linha de tabela, não uma linha de código.** No Dynamics, um plugin se
registra como *step* em `SdkMessageProcessingStep` — mensagem + tabela + estágio + modo + ordem — e o
contrato de código é minúsculo: `IPlugin.Execute(IServiceProvider)`. No Salesforce, o padrão oficial
recomendado é o *Metadata-Driven Trigger Framework*, com as classes registradas em `TriggerAction__mdt`
definindo ordem, ativo/inativo e bypass **sem deploy**.

### 4.2 O que cada um faz melhor

| Tema | Salesforce | Dynamics | Quem vence |
|---|---|---|---|
| Segurança por registro | OWD + hierarquia + 300 sharing rules por objeto, **materializadas** em tabelas de share | matriz **privilégio × profundidade** resolvida por `OwnerId` + `OwningBusinessUnit` na própria linha, **sem join** | **Dynamics** — mais simples e mais barato |
| Regras declarativas de compartilhamento | sharing rules por critério, restriction rules com **deny real** | não tem equivalente declarativo | **Salesforce** |
| Modelo de atividade | `Task`/`Event` com `WhoId`/`WhatId` polimórficos | **`ActivityPointer` é herança de tabela real** + `ActivityParty` com 13 tipos de participação | **Dynamics** |
| Guiar o usuário pelo processo | `Path` (barra de etapas, compartilha configuração com o Kanban) | **Business Process Flow** — barra de estágios com *tabela de instância própria*, que grava o **caminho percorrido** | **Dynamics** |
| Autoatendimento de relatório | **Report Type curado pelo admin** + builder drag-and-drop | dashboards + Power BI | **Salesforce** |
| Extensibilidade sem release | Custom Metadata Types (`__mdt`) — leitura **não consome limite de query** | `SdkMessageProcessingStep` + Custom API | empate |
| Qualificação de lead | conversão **irreversível**, trava o Lead como read-only | `QualifyLead` com 3 booleanos, **o Lead sobrevive**, link por `originatingleadid` | **Dynamics** |

### 4.3 Os números que definiram nosso desenho

- **Business Process Flow:** 30 estágios × 30 passos, até 5 tabelas por processo, 10 BPFs ativos por
  tabela. **Tabela de instância própria** com `activestageid` e `traversedpath`.
- **Plugin do Dataverse:** limite duro de 2 minutos, **recomendação oficial de 2 segundos**; anti-loop
  com `depth` máximo 8.
- **Alternate keys:** 10 por tabela, 900 bytes, 16 colunas — o mecanismo de **upsert idempotente** que a
  integração com TOTVS exige.
- **Rollup columns:** 200 por ambiente, 50 por tabela, job de 1 hora, **sem rollup de rollup**.
- **Mobile offline (Briefcase, Salesforce):** teto de 2.000 registros por objeto, **recomendado 500**.
  Isso explica tecnicamente a dor do Vórtico Mobile Lite.
- **Field History Tracking:** 20 campos por objeto, 18 meses de retenção. **Auditoria é escopada, não
  total** — o oposto do que o Vórtice faz.
- **Hierarchy Security (Dynamics):** recomendação oficial de **no máximo 50 usuários efetivos** sob um gestor.

### 4.4 O que decidimos NÃO copiar

Ambos carregam complexidade que existe porque são **multi-tenant globais**. A Tracbel não é.

Descartados: motor de armazenamento EAV · governor limits numéricos · Territory Management ·
Apex Managed Sharing · Muting Permission Sets · Access Teams auto-criadas · sharing linha-a-linha para
usuário individual · Position Hierarchy · Change Sets · a escada de 4 níveis de sandbox ·
PCF/canvas apps · Power BI embedded · mobile offline · Einstein (exige 200 oportunidades ganhas + 200
perdidas em 24 meses e custa US$ 50/usuário/mês) · Console Apps (padrão de call center, não de venda de campo).

---

## 5. A tese do CRM Tracbel

Dez princípios. Cada um responde a um defeito medido no Vórtice ou copia um acerto dos dois grandes.

| # | Princípio | Responde a |
|---|---|---|
| 1 | **Nada falha em silêncio.** Toda regra avaliada grava se disparou, e por quê. Falha de integração vira alarme, não Warning. | 899/900 · `RUNGEPIMPORT` |
| 2 | **Integridade no banco, não só na aplicação.** FK de verdade no caminho quente. | 345.535 órfãos · sync do mobile |
| 3 | **Toda transição de estado é explícita, reversível e auditada.** | "Atividade Cancelada" sem desfazer |
| 4 | **Permissão por registro desde a fase 1**, com um único ponto de aplicação no acesso a dados. | ausência total no Vórtice |
| 5 | **API-first: se a tela consegue, a API consegue.** | criar usuário sem API |
| 6 | **Extensão por evento registrado**, não por edição do núcleo. | Dynamics `SdkMessageProcessingStep` |
| 7 | **Regra tem condição.** Uma regra com expressão, não mil cópias. | 5.958 regras / 1.262 pares |
| 8 | **Relatório é contrato curado, nunca SQL solto.** | QVW aceita `UPDATE` |
| 9 | **Auditoria é escopada e tem retenção.** | 52,4% do banco é log |
| 10 | **O processo guia o usuário.** Barra de estágios com caminho percorrido, não um campo `status`. | Dynamics BPF |

---

## 6. Escopo, decomposição e rota

Isto **não é um projeto** — são cinco subsistemas. Cada um terá seu próprio ciclo spec → plano → implementação.

1. Núcleo de cadastro (pessoas, contas, contatos)
2. Motor de workflow e processo
3. Integrações (TOTVS Protheus, JD Edwards, RD Station)
4. Autorização, multiempresa e permissões
5. Relatórios e autoatendimento

**Rota:** substituição gradual (*strangler fig*). O Vórtice permanece o sistema de registro enquanto o
CRM novo assume um domínio por vez.

**Fase 1 — Leads e Prospecção.** Escolhida porque é onde o Vórtice é pior, e porque é *upstream*: não
depende de faturamento, de TOTVS nem de JDE. Prova o motor de workflow, o modelo de permissão e a API
com risco baixo.

**Regras invioláveis do projeto:**

- O CRM novo **nunca escreve no banco do Vórtice**. Banco próprio; leitura read-only; escrita de volta
  só pela `wsVorticeCrmApi`.
- Todo vocabulário herdado (`SeqPessoa`, `CodProcesso` que na verdade é tipo de fluxo, `Vendedor` que
  guarda login) fica confinado na camada de integração e **nunca vaza para o domínio**.
- **Uma fase só termina quando estiver 100% testada.** O portão de qualidade é condição de avanço,
  não formalidade.

---

## 7. Riscos — e o que fazemos com cada um

| Risco | Severidade | Mitigação |
|---|---|---|
| Time de 3 pessoas substituindo sistema de 767 tabelas | **Alta** | escopo por domínio; nunca migrar tudo; Vórtice vivo o tempo todo |
| Perda de conhecimento tácito do fluxo comercial | **Alta** | os fluxos já estão documentados em `docs/FLUXOS.md` e `docs/REGRAS-DE-NEGOCIO.md`; validar cada um com o dono do processo antes de implementar |
| Projeto morrer por falta de valor visível | **Alta** | fase 1 entrega tela usável em produção para um time real, não infraestrutura |
| Migração de dados (118k pessoas, 1,17M processos, 2,44M históricos) | **Média** | migrar por domínio, com reconciliação automática e chave de correlação preservada |
| Dependência de uma pessoa só (Ricardo) | **Alta** | é a razão de o código ser comentado e de existir um plano por colaborador |
| Integração TOTVS/JDE herdar o mesmo destino | **Média** | ACL com transação, watermark, dead-letter queue e alarme — tudo o que falta hoje |
| Escopo crescer para virar "outro Vórtice" | **Média** | YAGNI explícito; a lista da seção 4.4 é vinculante |

---

## 8. O custo de não fazer nada

| Cenário | Custo anual | Observação |
|---|---|---|
| Manter o Vórtice como está | licença do fornecedor + suporte | com a integração financeira cega e o risco de auditoria em aberto |
| Migrar para Salesforce Sales Cloud Enterprise | **~US$ 273 mil** (US$ 175/usuário/mês × 130) | ≈ R$ 1,5 milhão/ano só de licença, fora implantação |
| Construir | custo do time | o ativo fica na Tracbel, e o conhecimento também |

---

## Documentos deste projeto

| # | Documento | Conteúdo |
|---|---|---|
| 00 | **Briefing** (este) | por que, diagnóstico, benchmark, tese, escopo, riscos |
| 01 | Diagnóstico do Vórtice | o levantamento completo, com as queries que provam |
| 02 | Benchmark Salesforce × Dynamics | comparativo detalhado por área |
| 03 | Arquitetura | camadas, motor de workflow, pipeline de extensão, código comentado |
| 04 | Modelo de dados | todas as tabelas, colunas, tipos, FKs e índices |
| 05 | Segurança e permissões | as camadas, a ordem de avaliação, LGPD e auditoria |
| 06 | Plano de implementação | fases, portões de qualidade, infra, testes |
| 07 | Plano por colaborador | o que cada pessoa faz, na ordem |

**Pesquisa bruta:** [`docs/pesquisa/`](../pesquisa/) — 15 documentos, 387 achados, 249 entidades mapeadas.
