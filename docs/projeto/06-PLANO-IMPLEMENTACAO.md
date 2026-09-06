# Plano de implementação — CRM Tracbel

> **Documento 06 de 07** · Versão 1.0 · 30/08/2026
> Regra do projeto: **uma fase só termina quando o portão de qualidade fecha.** Não há
> "termina agora e testa depois". O portão é condição de avanço.

---

## 1. Como este plano funciona

Cada fase tem quatro blocos — **infraestrutura, banco, backend, frontend** — e um **portão de qualidade**
com critérios verificáveis por comando. Enquanto um critério estiver aberto, a fase não termina e a
seguinte não começa.

**Times:** 3 pessoas. Se forem 2, as durações abaixo aumentam ~50% e o papel de frontend passa a ser
compartilhado.

| Papel | Pessoa | Responsabilidade |
|---|---|---|
| **A — Arquiteto / Tech Lead** | Ricardo | domínio, motor de workflow, segurança, integração, revisão de todo PR |
| **B — Backend** | dev 2 | casos de uso, API, persistência, testes de backend |
| **C — Frontend** | dev 3 | React, design system, testes de tela e E2E |

---

## 2. Visão geral das fases

| Fase | Entrega | Duração (3 pessoas) | Vórtice |
|---|---|---|---|
| **0** | Fundação técnica | 5 semanas | intacto |
| **1** | **Leads e Prospecção em produção** | 12 semanas | intacto |
| **2** | Integração TOTVS + oportunidade | 10 semanas | leitura |
| **3** | Processo de venda completo | 16 semanas | convivência |
| **4** | Equipamentos e pós-venda | 12 semanas | convivência |
| **5** | Relatórios e autoatendimento | 8 semanas | convivência |
| **6** | Migração final e desligamento | 12 semanas | **desligado** |

**Total: ~75 semanas ≈ 17 meses** com 3 pessoas em dedicação integral. Com 2 pessoas ou dedicação
parcial, projete 24 a 30 meses.

> **Honestidade sobre o prazo:** esta estimativa assume que os donos de processo estarão disponíveis
> para validar regras. Historicamente, é aí que projetos assim derrapam — não no código.

---

## 3. Fase 0 — Fundação (5 semanas)

Sem tela para o usuário. É o alicerce, e é a única fase em que isso é aceitável.

### Infraestrutura
- Repositório Git, branch protection, PR obrigatório com uma aprovação
- Três ambientes: `dev` (máquina do dev), `homolog` (VM), `prod` (VM, IIS)
- Database `TRACBEL_CRM` nas três instâncias; **backup diário com teste de restauração**
- Pipeline CI: build → testes → análise estática → migration → deploy em `homolog`
- Serilog → Seq; OpenTelemetry → Prometheus/Grafana; Hangfire com dashboard

### Banco
- Migrations iniciais: schemas `org`, `seg`, `crm`, `wf`, `aud`, `meta`
- Seed: empresas reais da Tracbel, linhas de negócio, permissões, conjuntos
- Role de aplicação **sem `DELETE`** em `wf.Atividade` e `aud.*`

### Backend
- Solução com as 6 camadas e o **teste de arquitetura que impede violação de dependência**
- `EntidadeBase`, tipos de valor (`Email`, `Telefone`, `CpfCnpj`), `Resultado<T>`
- Autenticação Entra ID de ponta a ponta
- Motor de permissão: camadas 2, 3 e 4 com a matriz de teste completa
- `MotorWorkflow` com `wf.RegraExecucao` e o job `MonitorSaudeRegras`

### Governança (nova — vem do documento 11)
- **Conferir o contrato de licenças do Vórtice.** Cinco módulos estão registrados como pagos
  (`TipoAcesso = P`) e **sem versão instalada**: Config RD Station, Web CRM, OUT CRM, BPM 2025
  e Painel de Controle 2025. Se algum está sendo cobrado sem uso, é economia imediata — e
  ajuda a financiar o projeto. Responsável: A, com o setor de contratos.

### Frontend
- Projeto React + Vite + TypeScript, roteamento, login Entra ID
- Design system base: tipografia, cores, componentes de formulário, tabela, layout
- Cliente de API tipado gerado do OpenAPI

### 🚦 Portão da fase 0

| # | Critério | Como verificar |
|---|---|---|
| 1 | Build limpo, zero warning | `dotnet build -warnaserror` |
| 2 | Cobertura do domínio ≥ 90% | `dotnet test /p:CollectCoverage=true` |
| 3 | Teste de arquitetura passa | `dotnet test --filter Categoria=Arquitetura` |
| 4 | Matriz de permissão completa | `dotnet test --filter Categoria=Autorizacao` |
| 5 | Login real funciona em `homolog` | roteiro manual assinado |
| 6 | Deploy automatizado ida e volta | pipeline verde + rollback testado |
| 7 | **Restauração de backup testada** | evidência com data e responsável |
| 8 | Alarme de regra dispara | teste de fumaça: desliga regra, alarme abre |

---

## 4. Fase 1 — Leads e Prospecção (12 semanas)

**A primeira fase com usuário real em produção.** Escolhida porque é onde o Vórtice é pior e porque não
depende de TOTVS, JDE nem faturamento.

### Escopo funcional

1. Receber lead: RD Station (webhook), site, entrada manual, importação CSV
2. Deduplicar contra base existente (CPF/CNPJ, e-mail, telefone normalizado)
3. Distribuir por carteira (cidade × linha de negócio) com fila para quando não houver carteira
4. Qualificar: criar Conta + Contato + Processo, mantendo o Lead vivo e ligado
5. Descartar com motivo — **e reabrir**
6. Agenda do usuário: minhas tarefas, com priorização e "próxima ação"
7. Registrar andamento com desfecho, disparando o motor de regras
8. Linha do tempo do lead e da conta
9. Painel do gestor: funil, conversão por origem, tempo em cada estágio

### Banco
As 25 tabelas da fase 1 (documento 04, seção 13) + seed dos catálogos reais: tipos de processo,
estágios, tipos de tarefa, desfechos, origens de lead, motivos de descarte.

> **Curadoria obrigatória:** `[V]` o Vórtice tem **980 ações (169 usadas)** e **4.208 resultados (501
> usados)**. Migramos **só o que foi usado em 2026**, e a coluna `UltimoUsoEm` existe para que o catálogo
> nunca mais apodreça sem que se saiba.

### Backend
- Domínio: `Lead`, `Conta`, `Contato`, `Processo`, `Tarefa`, `Atividade`
- Casos de uso: `ReceberLeadExterno`, `QualificarLead`, `DescartarLead`, `DarAndamento`, `MoverEstagio`
- Efeitos de regra: `CriarTarefa`, `MoverEstagio`, `Notificar`, `AtribuirCarteira`
- Estratégias de destinatário: proprietário, carteira, `gestorDe()`, equipe, quem executou
- Deduplicação: normalização + score de similaridade + fila de revisão
  > `[V]` o motor de dedupe do Vórtice é bom e a **fila de 124 mil pares está abandonada desde 2018**.
  > Aqui a fila tem dono e SLA, ou não existe.
- Webhook do RD Station com **payload íntegro gravado sempre**
  > `[V]` defeito 2.9: campo não mapeado numa tela é descartado. Aqui, guardamos tudo e reprocessamos.
- API completa da fase, com OpenAPI

### Frontend
- Lista de leads com filtros salvos e **Kanban por estágio** (`[SF]` Path e Kanban compartilham a mesma
  configuração — um metadado, duas telas)
- Tela do lead: resumo no topo com 7 campos (`[SF]` Compact Layout, a melhor razão esforço/benefício de
  UX de todo o Salesforce), timeline, ações
- **Barra de estágios** com caminho percorrido (`[DYN]` Business Process Flow)
- **Minha agenda** com widget "próxima ação" (`[DYN]` Sales Accelerator: work list + Up next)
- Formulário de qualificação em um passo, com deduplicação em tempo real
- Painel do gestor

### Migração de dados
- Leads e prospects do Vórtice (`GE_Pessoa` com `Status` `P` e `S` ≈ 77 mil) para `crm.Conta`
- Correlação preservada em `intg.ChaveExterna` (`SistemaOrigem = 'VORTICE'`)
- **Reconciliação automática:** contagem origem × destino, com relatório de divergência

### 🚦 Portão da fase 1

| # | Critério | Como verificar |
|---|---|---|
| 1 | Todos os critérios da fase 0 continuam verdes | pipeline |
| 2 | Cobertura de domínio ≥ 90%, aplicação ≥ 80% | relatório de cobertura |
| 3 | **Cobertura de regra = 100%** — cada regra testada disparando **e não disparando** | `--filter Categoria=Workflow` |
| 4 | Matriz de autorização completa, incluindo testes de escape | `--filter Categoria=Autorizacao` |
| 5 | E2E dos 5 fluxos principais | Playwright verde |
| 6 | Reconciliação da migração com **zero divergência** | relatório assinado |
| 7 | Carga: 500 leads/hora sem degradação | teste de carga com evidência |
| 8 | **UAT com 5 usuários reais**, cada um completando 3 tarefas sem ajuda | roteiro assinado pelo dono do processo |
| 9 | Acessibilidade: navegação por teclado e contraste AA nas telas principais | axe-core no CI |
| 10 | Runbook de operação escrito e **testado por quem não construiu** | evidência |
| 11 | Sem vulnerabilidade alta ou crítica | `dotnet list package --vulnerable` |
| 12 | Alarme de saúde de regra ativo em produção | incidente de teste aberto e fechado |

> **Critério 8 é o que separa este projeto do Vórtice.** Um sistema que exige treinamento longo para
> tarefas básicas falhou no requisito de usabilidade, mesmo com todos os testes verdes.

---

## 5. Fase 2 — Integração e oportunidade (10 semanas)

### Escopo
- Anti-corruption layer com TOTVS Protheus: contas, produtos, notas fiscais, títulos
- Leitura read-only do Vórtice para dados ainda não migrados
- Escrita de volta ao Vórtice pela `wsVorticeCrmApi` (lead qualificado vira processo lá)
- Oportunidade com valor, itens e previsão

### O que corrigimos do legado

`[V]` A integração do Vórtice está morta e ninguém foi avisado: **783.242 títulos (R$ 5,18 bi) presos em
`IMP_Titulo` desde 22/05/2025**; **280.214 de 287.868 OS** nunca promovidas; `RUNGEPIMPORT` falhando a
cada 20 minutos há meses, registrado como **Warning**; procedures **sem transação** (`BEGIN TRANSACTION`
comentado).

Nossa ACL tem, obrigatoriamente:

| Item | Implementação |
|---|---|
| Transação | toda promoção é atômica; falha reverte |
| Idempotência | `intg.ChaveExterna` com unique por sistema — reprocessar não duplica |
| Watermark | `intg.Watermark` com contagem de lidos, gravados e erro |
| Dead-letter | `intg.MensagemDescartada` **com dono e SLA de tratativa** |
| Alarme | fluxo sem sucesso em 2 ciclos → incidente. **Nunca "Warning"** |
| Painel | tela de saúde das integrações, visível para o TI |

### 🚦 Portão da fase 2
Critérios da fase 1 + reconciliação financeira batendo com o Protheus + **teste de caos**: derruba a
conexão no meio da carga e verifica que nada duplicou e nada se perdeu + alarme testado em produção.

---

## 6. Fase 3 — Processo de venda completo (16 semanas)

A fase mais longa e mais arriscada: é o fluxo 50 inteiro (documento `docs/FLUXOS.md`), da prospecção à
entrega, com aprovações, formulários e documentos.

### Escopo
- Todos os estágios do fluxo de venda de equipamentos
- Aprovações roteadas por hierarquia resolvida em runtime
- Formulários dinâmicos
  > `[V]` **não copiamos** o modelo do Vórtice, em que cada formulário vira **uma tabela física**
  > (175 tabelas `IV_Q_*` criadas em runtime, nunca coletadas quando o formulário morre). Usamos
  > definição em metadados + coluna `json` tipada e validada.
- Documentos anexados com FK real (`ON DELETE CASCADE`)
  > `[V]` corrige os **5.302 vínculos órfãos (6,9%)** que travam o sincronismo do mobile
- Demonstração, Peças/Pneus, DSI/PUK e Grandes Contas como tipos de processo adicionais

### 🚦 Portão da fase 3
Anteriores + **cada fluxo validado pelo respectivo dono de processo** + operação em paralelo com o
Vórtice por 4 semanas, com divergência menor que 1%.

---

## 7. Fase 4 — Equipamentos e pós-venda (12 semanas)

Frota do cliente, planos de manutenção, ordens de serviço, garantia. É o maior ativo de pós-venda:
`[V]` o Vórtice tem **3,57 milhões de linhas** de plano de manutenção.

**Portão:** anteriores + reconciliação da frota com o ERP + rastreabilidade equipamento → NF → OS
funcionando de ponta a ponta.

> `[V]` Hoje isso **não existe**: `EXT_*` e `IV_*` são duas ilhas ligadas só por `SeqPessoa`, e
> **99,1% das notas fiscais não têm `IdVeic`**.

---

## 8. Fase 5 — Relatórios e autoatendimento (8 semanas)

`[SF]` Report Type curado + builder drag-and-drop. O usuário monta relatório **sem ver o schema e sem
escrever SQL**.

**Portão:** anteriores + **10 usuários montando um relatório sozinhos**, sem ajuda, em menos de 10
minutos + toda fonte com filtro de segurança verificado por teste de escape.

---

## 9. Fase 6 — Migração final e desligamento (12 semanas)

- Migração do histórico (1,17M processos, 2,44M históricos) com reconciliação
- Congelamento de escrita no Vórtice
- Período de leitura-somente do legado (90 dias)
- Arquivamento do banco antigo
- **Desligamento**

**Portão:** reconciliação completa + plano de rollback testado + arquivo do legado íntegro e verificado.

---

## 10. Ritmo de trabalho

| Cerimônia | Quando | Duração |
|---|---|---|
| Planejamento da sprint | segunda, quinzenal | 1h |
| Alinhamento diário | todo dia, 9h30 | 15 min |
| Revisão de PR | contínua — **todo PR tem revisor** | — |
| Demo para o negócio | fim de cada sprint | 30 min |
| Retrospectiva | fim de cada sprint | 45 min |
| Revisão do portão | fim de cada fase | 2h, com o dono do processo |

**Regra de PR:** nenhum merge sem revisão de outra pessoa. Com 3 devs isso é sustentável e é a principal
defesa contra o risco de dependência de uma pessoa só.

---

## 11. Riscos operacionais do plano

| Risco | Sinal de alerta | Ação |
|---|---|---|
| Fase 1 atrasa mais de 3 semanas | portão 8 (UAT) não fecha | cortar escopo do painel do gestor, nunca dos testes |
| Dono de processo indisponível | validação pendente há 2 sprints | escalar para a diretoria; não implementar por suposição |
| Escopo crescendo | pedidos fora do documento | a lista de "não copiar" (briefing, seção 4.4) é vinculante |
| Ricardo indisponível | — | todo conhecimento em `docs/`; revisão cruzada obrigatória em todo PR |
| Vórtice piorar durante o projeto | novo defeito crítico | tratar como incidente separado; **não acelerar a migração por pânico** |
