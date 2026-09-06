# Plano por colaborador — CRM Tracbel

> **Documento 07 de 07** · Versão 1.0 · 30/08/2026
> Um plano por pessoa: o que você faz, o que precisa saber antes, e como saber que terminou.
> Cobre as fases 0 e 1 em detalhe semanal. As fases seguintes seguem o mesmo padrão.

---

## Antes de qualquer código — leitura obrigatória para todos

| Ordem | Documento | Por quê |
|---|---|---|
| 1 | [00-BRIEFING](00-BRIEFING.md) | por que este projeto existe e o que ele não é |
| 2 | [`docs/REGRAS-DE-NEGOCIO.md`](../REGRAS-DE-NEGOCIO.md), Parte 1 | o modelo mental do BPM. Sem ele, metade das decisões parece arbitrária |
| 3 | [03-ARQUITETURA](03-ARQUITETURA.md) | as camadas, a regra de dependência, as convenções |
| 4 | [04-MODELO-DADOS](04-MODELO-DADOS.md), seção 1 | as convenções de tabela e as correções de tipo |
| 5 | [05-SEGURANCA](05-SEGURANCA.md), seções 2 e 5 | as cinco camadas e a ordem de avaliação |

**Uma tarde de leitura.** É o investimento que evita semanas de retrabalho.

---

# Pessoa A — Arquiteto / Tech Lead (Ricardo)

## Seu papel

Você é o dono do **domínio**, do **motor de workflow**, da **segurança** e da **integração**. É também
quem revisa todo PR. Não é o papel de escrever mais código — é o de garantir que o código dos outros
dois não crie o próximo Vórtice.

**Sua responsabilidade contínua, em toda sprint:**
1. Revisar 100% dos PRs. Nenhum merge sem sua aprovação nas fases 0 e 1.
2. Manter `docs/projeto-crm/` atualizado quando uma decisão mudar.
3. Ser o dono dos portões de qualidade: você declara a fase encerrada, ninguém mais.
4. Interlocução com os donos de processo do negócio.

## Fase 0 — semana a semana

| Semana | Entrega | Pronto quando |
|---|---|---|
| 1 | Solução, 6 camadas, CI com build e teste. Teste de arquitetura funcionando | PR de exemplo é barrado ao violar a dependência |
| 1 | `EntidadeBase`, `Resultado<T>`, `IEventoDominio` | testados, cobertura 100% |
| 2 | Tipos de valor: `Email`, `Telefone`, `CpfCnpj` | `Telefone` aceita `+55 (17) 99999-0000` e rejeita lixo — com testes dos dois |
| 2 | Autenticação Entra ID de ponta a ponta | login real em `homolog` |
| 3 | Motor de permissão: camadas 2, 3 e 4 | matriz de teste completa passando |
| 3 | `HasQueryFilter` global + teste de escape | consulta fora de escopo devolve vazio, não erro |
| 4 | `MotorWorkflow` + `wf.RegraExecucao` | regra que **não** dispara grava motivo em português |
| 4 | `IEfeitoRegra` + `IEstrategiaDestinatario` com 2 implementações cada | novo efeito entra sem tocar em arquivo existente |
| 5 | `MonitorSaudeRegras` + alarme | desligar uma regra abre incidente em 24h |
| 5 | **Portão da fase 0** | os 8 critérios do documento 06, seção 3 |

## Fase 1 — seus blocos

| Semanas | Entrega |
|---|---|
| 6-7 | Domínio de Lead, Conta, Contato — entidades com comportamento, não sacos de propriedade |
| 8-9 | Motor de deduplicação: normalização, score, fila de revisão **com dono** |
| 10-11 | Webhook do RD Station com payload íntegro + reprocessamento |
| 12-13 | Migração dos ~77 mil prospects, com `intg.ChaveExterna` e reconciliação |
| 14-15 | Curadoria do catálogo: só ações e desfechos usados em 2026 |
| 16-17 | Regras da fase 1 e seus testes de disparo **e de não-disparo** |
| 18 | Portão da fase 1 |

## O que você precisa dominar

- **EF Core:** global query filters, owned types, `rowversion`, TPH, interceptors
- **Domain-Driven Design:** agregado, tipo de valor, evento de domínio. O suficiente, não o dogma
- **Entra ID:** OIDC, claims, app registrations, client credentials
- O **documento 05 inteiro** — você é o dono da segurança

## Suas armadilhas

1. **Construir plataforma em vez de produto.** A tentação de generalizar o motor de metadados é grande.
   Regra: só generalize no terceiro caso concreto.
2. **Ser o gargalo de revisão.** Se o PR de alguém espera mais de um dia, o problema é seu.
3. **Deixar o vocabulário do Vórtice vazar.** Você é o último filtro. O teste automatizado ajuda, mas
   não pega tudo.

---

# Pessoa B — Backend

## Seu papel

Casos de uso, API, persistência e os testes de tudo isso. Você constrói **em cima** do domínio que A
define — e é você quem descobre, na prática, se o domínio está bom.

## Fase 0 — semana a semana

| Semana | Entrega | Pronto quando |
|---|---|---|
| 1 | Ambiente de dev, banco local, primeiro build verde | você roda os testes sozinho |
| 1-2 | Migrations dos schemas `org` e `seg` | `dotnet ef database update` do zero funciona |
| 2 | Seed: empresas, linhas de negócio, permissões, conjuntos | dados reais da Tracbel, não fictícios |
| 3 | Configurações EF de todas as entidades da fase 1 | mapeamento bate com o documento 04, coluna por coluna |
| 3-4 | Repositórios + `Testcontainers` com SQL Server | teste de integração roda no CI sem banco local |
| 4 | Pipeline de comando (MediatR ou equivalente) + validação | um caso de uso de exemplo ponta a ponta |
| 5 | OpenAPI gerado + cliente TypeScript para C | C consegue chamar a API |

## Fase 1 — seus blocos

| Semanas | Entrega |
|---|---|
| 6-8 | Migrations e configuração das 25 tabelas da fase 1 |
| 9-11 | Casos de uso: `ReceberLeadExterno`, `QualificarLead`, `DescartarLead`, `ReabrirLead` |
| 12-14 | Casos de uso: `DarAndamento`, `MoverEstagio`, `ReatribuirTarefa` |
| 15-16 | API completa da fase, com idempotência e paginação por cursor |
| 17 | Consultas do painel do gestor (funil, conversão, tempo por estágio) |
| 18 | Teste de carga: 500 leads/hora |

## Seu checklist de "pronto" — por caso de uso

- [ ] Teste de unidade do caminho feliz
- [ ] Teste de **cada** regra de negócio que pode recusar
- [ ] Teste de integração com banco real
- [ ] Teste de autorização: quem **não** pode, não consegue
- [ ] Endpoint com `[RequerPermissao]` — sem isso o build falha
- [ ] Documentado no OpenAPI, com exemplo de requisição e de erro
- [ ] Erro de negócio devolve 409 ou 422 com mensagem em português. **Nunca 500**

## O que você precisa dominar

- **EF Core avançado:** migrations, configuração fluente, `AsNoTracking`, projeção
- **Testcontainers:** teste de integração com SQL Server real
- **ASP.NET Core:** minimal APIs ou controllers, filtros, model binding, `ProblemDetails`
- Leitura do documento 04 **inteiro** — é a sua especificação

## Suas armadilhas

1. **Colocar regra de negócio no caso de uso.** Se tem `if` sobre estado da entidade, a regra pertence
   à entidade. Pergunte a A.
2. **`Include` em cascata.** Vira consulta de 12 joins. Projete para DTO.
3. **Esquecer o `CancellationToken`.** Propague sempre, até o fim.
4. **Teste que só cobre o caminho feliz.** O valor está nos testes que provam que o sistema **recusa**.

---

# Pessoa C — Frontend

## Seu papel

A interface. E ela **não é acessório**: o requisito declarado é que o usuário "mexa tranquilamente". Um
sistema que exige treinamento longo para tarefa básica falhou, mesmo com todos os testes verdes.

O portão da fase 1 tem um critério que é **seu**: cinco usuários reais completando três tarefas **sem
ajuda**.

## Fase 0 — semana a semana

| Semana | Entrega | Pronto quando |
|---|---|---|
| 1 | React + Vite + TypeScript, rotas, layout base | roda local e em `homolog` |
| 2 | Login Entra ID (MSAL), guarda de rota, contexto de usuário | o login real funciona |
| 2-3 | Design system: tipografia, cores, espaçamento, botão, input, select, modal, tabela | Storybook publicado |
| 3-4 | Componentes de dado: tabela com filtro/ordenação/paginação, formulário com validação | usados numa tela de exemplo |
| 4 | Cliente de API tipado + tratamento padrão de erro | erro 409 vira mensagem legível, não *toast* genérico |
| 5 | Vitest + Testing Library configurados; axe-core no CI | cobertura reportada |

## Fase 1 — seus blocos

| Semanas | Entrega |
|---|---|
| 6-8 | Lista de leads: filtros salvos, busca, seleção múltipla, **Kanban por estágio** |
| 9-10 | Tela do lead: resumo de 7 campos no topo, timeline, painel de ações |
| 11-12 | **Barra de estágios** com caminho percorrido e campos obrigatórios por estágio |
| 13-14 | **Minha agenda** + widget "próxima ação" com Concluir / Adiar / Pular |
| 15 | Formulário de qualificação com deduplicação em tempo real |
| 16 | Painel do gestor: funil, conversão por origem, tempo por estágio |
| 17-18 | Playwright nos 5 fluxos + rodada de UAT com usuários reais |

## As cinco ideias de UX que você vai implementar (e de onde vieram)

| # | Ideia | Origem | Por que importa |
|---|---|---|---|
| 1 | **Resumo de 7 campos** no topo do registro | `[SF]` Compact Layout | a melhor razão esforço/benefício de UX de todo o Salesforce |
| 2 | **Barra de estágios** com caminho percorrido | `[DYN]` Business Process Flow | o usuário vê onde está e o que falta, sem treinamento |
| 3 | **Kanban e lista compartilham a configuração** | `[SF]` Path + Kanban | um metadado, duas telas — metade do trabalho |
| 4 | **Próxima ação** em destaque | `[DYN]` Sales Accelerator | elimina a pergunta "e agora, o que eu faço?" |
| 5 | **Lista como interface de trabalho**, relatório como exceção | `[SF]` List Views | o usuário trabalha na lista; relatório é caso raro |

## Seu checklist de "pronto" — por tela

- [ ] Funciona em 1366×768 (a resolução real dos terminais da Tracbel)
- [ ] Navegável **inteiramente por teclado**
- [ ] Contraste AA — verificado pelo axe-core no CI
- [ ] Estado de carregamento, estado vazio e estado de erro, todos desenhados
- [ ] Mensagem de erro em português, dizendo **o que fazer**, não só o que houve
- [ ] Teste de componente no Vitest
- [ ] Fluxo principal coberto no Playwright
- [ ] Um usuário real completou a tarefa **sem você explicar**

## O que você precisa dominar

- **React 18:** hooks, Suspense, error boundaries
- **TanStack Query:** cache, invalidação, otimista
- **React Hook Form + Zod:** validação espelhando as regras do backend
- **Playwright** e **axe-core**
- Leitura do documento 03, seção 7 (contrato da API) e do 05, seção 7 (campos omitidos)

## Suas armadilhas

1. **Hardcodar campo.** A API devolve dado **e metadado** (`[SF]` UI API). Monte a tela a partir do
   metadado — é o que permite acrescentar campo sem release.
2. **Ignorar `camposOmitidos`.** O usuário sem permissão de margem não recebe o campo. A tela precisa
   lidar com isso sem quebrar (e sem mostrar "null").
3. **Design bonito e improdutivo.** Meça cliques. A tarefa mais comum tem que ser a mais rápida.
4. **Deixar UAT para o fim.** Faça uma rodada informal na semana 12, não na 18.

---

## Combinados do time

1. **Português no domínio, inglês no técnico.** `Processo`, `DarAndamento`; `Repository`, `Handler`.
2. **Comentário explica o porquê.** O marcador `[V]` registra uma lição do Vórtice — nunca remova um
   sem entender o que ele evita.
3. **Todo PR tem revisor.** Nas fases 0 e 1, o revisor é A.
4. **Teste junto com o código, no mesmo PR.** "Testo depois" é como se acumula dívida.
5. **Portão fechado não se negocia.** Cortar escopo é permitido; cortar teste, não.
6. **Achado não-óbvio vira documento.** É a regra que produziu `docs/REGRAS-DE-NEGOCIO.md` e é o que
   torna este projeto transferível.

---

## Se o time for de 2 pessoas

A + B se mantêm; o frontend passa a ser de B, com A pegando as telas de configuração. As durações
crescem ~50%: fase 0 vira 7 semanas, fase 1 vira 18. **O portão não muda** — é a parte que não se
comprime.
