# Catálogo de fluxos e ordem de migração

> **Documento 10 de 10** · Versão 1.0 · 31/08/2026
> Levantado a partir da tela **"Códigos de processos cadastrados"** do Vórtice e **cruzado com
> o uso real no banco de produção** (consulta read-only, 31/08/2026).
> Este documento dimensiona a migração e define uma decisão de modelagem que não estava no
> documento 04.

---

## 1. O retrato

| Métrica | Valor |
|---|---|
| Modelos de processo cadastrados | **62** |
| Geraram alguma agenda em 2026 | **17** |
| **Não geraram nada em 2026** | **45 — 73% do catálogo** |
| Nunca geraram agenda, jamais | **12** |
| Agendas em 2026 | 105.415 |
| Pendentes em 2026 | **25.503 — 24%** |

**Os 5 maiores fluxos concentram 90% do trabalho.** Os outros 57 dividem os 10% restantes.

---

## 2. Os fluxos vivos, por volume

Agendas criadas em 2026, com o tamanho real do catálogo de cada um.

| Cód | Fluxo | Agendas | Pendentes | Processos | Ações | Resultados |
|---:|---|---:|---:|---:|---:|---:|
| **50** | Venda Equipamento Tracbel Agro | **62.277** | 19.895 | 29.929 | **47** | **178** |
| **32** | Prospecção Peças/Serviços | **10.272** | 1.838 | 10.265 | **4** | **19** |
| **51** | SEGUROS (FY25) | 8.897 | 1.259 | 3.134 | 14 | 50 |
| **34** | Venda Peças/Pneus Tracbel Agro | 6.769 | 81 | 3.570 | 5 | 24 |
| **9408** | Aferição Qualidade JDE | 6.596 | 268 | 6.518 | 11 | 36 |
| 46 | Pre-Entrega | 3.934 | 154 | 1.453 | 29 | 63 |
| 47 | Entrega Física / Técnica | 3.757 | 822 | 1.310 | 11 | 21 |
| 39 | Demonstração Tracbel Agro | 1.292 | 163 | 163 | 23 | 57 |
| 49 | Venda de Seguro (NV) | 465 | 13 | 260 | 14 | 38 |
| 41 | Venda Maquina/Implemento/AMS | 66 | 3 | 23 | 19 | 35 |
| 42 | VENDA DE CONSÓRCIO | 35 | 8 | 35 | 1 | 6 |
| 45 | Garantia | 26 | 23 | 25 | 9 | 15 |
| 48 | Soluções Integradas | 14 | 10 | 14 | 2 | 3 |
| 23 | Venda Serviço AMS | 14 | 4 | 14 | 1 | 2 |
| 35 | Demonstracao | 10 | 7 | 10 | 5 | 5 |
| **44** | **Teste Workflow** | **2** | 2 | 2 | 2 | 1 |
| 24 | VENDA DE SERVIÇOS | 1 | 0 | 1 | 1 | 3 |

> ⚠️ **"Teste Workflow" gerou processos em produção em agosto de 2026.** Duas agendas, as duas
> ainda pendentes. Confirma o padrão de dado de teste em produção que a pesquisa já tinha achado
> em outras tabelas.

---

## 3. 🔴 CORREÇÃO — "JDE" no nome não significa integração morta

O documento 01 e a pesquisa registram que as **tabelas** `JDE_*` estão 100% vazias e que o job
`JD_QUOTE` nunca foi agendado. **Isso continua correto.**

Mas há um segundo conjunto de coisas com "JDE" no nome — os **modelos de processo** 9400 a 9414 — e
esses são outra coisa. Medido:

| Cód | Fluxo | Total histórico | Agendas 2026 |
|---:|---|---:|---:|
| **9408** | Aferição Qualidade JDE | 52.623 processos | **6.596 — o 5º maior fluxo vivo** |
| 9400 | Prospecção Máquina/Implem JDE | 43.965 processos | 0 (parou em abr/2024) |
| 9409 | Prospecção Peças JDE | 11.718 processos | 0 (parou em mai/2021) |
| 9406 | Venda Peças JDE | 7.997 processos | 0 (parou em mai/2021) |
| 9402–9405, 9411–9414 | oito modelos | **0, sempre** | 0 |

**Conclusão:** os modelos com "JDE" no nome rodam **dentro do Vórtice**, não no JD Edwards. O nome
é herança de uma nomenclatura de projeto, não indicação de integração.

**Descartar um fluxo pelo nome teria eliminado o 9408, que está vivo e com 6.596 agendas.**
A regra que fica: **decidir por uso medido, nunca por nome.**

---

## 4. 🔴 A decisão de modelagem que este catálogo revela

Olhe os nomes lado a lado:

```
 7  VENDA EQUIPAMENTO JD                    ← marca
13  VENDA DIRETA EQUIPAMENTO                ← canal
15  VENDA EQUIPAMENTO                       ← genérico
16  Venda Equipamento Manitou               ← marca
41  Venda Maquina/Implemento/AMS            ← sistema
50  Venda Equipamento Tracbel Agro          ← divisão
9401 Venda Máquina/Implemento JDE           ← projeto
```

**Sete modelos de processo para a mesma forma de fluxo**, clonados por marca, canal, sistema,
divisão e projeto. Só o 50 está vivo.

O mesmo padrão se repete:

| Forma de fluxo | Modelos cadastrados | Vivos em 2026 |
|---|---:|---:|
| Prospecção | **11** (12, 17, 26, 27, 31, 32, 33, 38, 9400, 9409, 9410) | **1** (o 32) |
| Venda de equipamento/implemento | **10** (6, 7, 13, 15, 16, 36, 41, 50, 9401, 9403) | **2** (50, 41) |
| Seguros | **4** (38, 49, 51, 9411) | **3** |
| Garantia | **2** (25, 45) | 1 |
| Demonstração | **2** (35, 39) | **2 — o antigo ainda recebe** |
| Usados | **4** (19, 27, 30, 9413) | 0 |

E há clonagem por **ano fiscal**: `SEGUROS (FY25) - 51`. E por **geração**:
`Venda Produto Colorado - NOVO - 37` — o mesmo antipadrão dos relatórios
(`TBA_101X (Novo)`, `TBA_101XX3 FY25`, `Tba_101xx4`).

### Por que isso acontece

No Vórtice, o modelo de processo carrega **tudo junto**: a forma do fluxo, a linha de negócio, a
marca, o sistema de origem e a geração. Não há dimensão separada para variar. Então **variar
significa clonar** — e clonar o modelo obriga a clonar as fases, as ações, os resultados e as regras.

É a causa raiz de todos os números de podridão que já medimos:

| Consequência | Medida |
|---|---|
| Modelos de processo | 62, com 45 mortos |
| Ações | 980, com 169 usadas em 2026 |
| Resultados | 4.208, com 501 usados |
| Regras de automação | 5.958, para 1.262 pares distintos |

### A correção no CRM Tracbel

**Separar a forma do fluxo das dimensões que variam.** A forma vira `TipoProcesso`; o resto vira
**dado**, não um fluxo novo.

```
TipoProcesso (a FORMA — cerca de 7)
   Prospecção · Venda · Demonstração · Entrega · Pós-venda · Garantia · Cobrança
        ×
LinhaNegocio (DADO)      MAQ · PEC · DSI · PNEUS · PUK · SEGUROS · USADOS · CONSORCIO
        ×
Marca (DADO)             John Deere · Manitou · …
        ×
Versão (VERSIONAMENTO)   não clonagem
```

Uma marca nova, uma linha nova ou um ano fiscal novo passam a ser **uma linha de tabela**, não um
fluxo inteiro para configurar. É essa separação que impede o catálogo novo de virar o catálogo velho.

> **Ajuste no documento 04:** `wf.TipoProcesso` já tem `LinhaNegocioId` e `Versao`. Falta acrescentar
> **`MarcaId`** (nulo = vale para todas) e tornar explícito, na tela de administração, que criar um
> `TipoProcesso` novo é decisão rara — o caminho normal é acrescentar uma dimensão.

---

## 5. A ordem de migração, agora com tamanho

Cada fluxo tem seu catálogo medido. Isso dimensiona o trabalho de cada fase.

| Fase | Fluxo a assumir | Ações | Resultados | Agendas/ano | Dificuldade |
|---|---|---:|---:|---:|---|
| **1** | **32 Prospecção Peças/Serviços** | **4** | **19** | 10.272 | **baixa** — catálogo minúsculo, volume alto, sem dependência de ERP |
| 2 | 34 Venda Peças/Pneus | 5 | 24 | 6.769 | baixa — precisa de faturamento |
| 2 | 9408 Aferição Qualidade | 11 | 36 | 6.596 | baixa — pós-venda isolado |
| 3 | **50 Venda Equipamento** | **47** | **178** | **62.277** | **alta** — é o coração da operação |
| 3 | 46 Pre-Entrega + 47 Entrega | 40 | 84 | 7.691 | média — acopladas ao 50 |
| 4 | 51 + 49 Seguros | 28 | 88 | 9.362 | média |
| 4 | 39 Demonstração | 23 | 57 | 1.292 | média |
| 5 | 42, 45, 48, 23, 41 (cauda) | 51 | 61 | 155 | baixa — avaliar se migra ou aposenta |

### O que isso valida e o que corrige

**Valida a escolha da fase 1.** O fluxo 32 (Prospecção) tem **4 ações e 19 resultados** — o menor
catálogo de todos — com **10.272 agendas por ano**, o segundo maior volume. É a melhor relação
valor/risco do catálogo inteiro, e é exatamente o domínio que já escolhemos.

**Corrige a estimativa da fase 3.** O fluxo 50 tem **47 ações e 178 resultados** em uso. O plano
(documento 06) reserva 16 semanas para ele. Com esse tamanho de catálogo, mais as aprovações
roteadas e os formulários, **16 semanas é apertado.** Recomendo rever para 18–20 na próxima revisão
do plano, ou dividir o fluxo 50 em duas entregas (até Faturamento; depois Entrega).

**Reduz o escopo total.** Só **17 fluxos** precisam de migração, não 62. E **8 deles somam 155
agendas por ano** — vale conversar com o negócio sobre aposentar em vez de migrar.

---

## 6. Os 45 fluxos mortos — o que fazer

| Grupo | Quantos | Recomendação |
|---|---:|---|
| **Nunca usados** (4, 5, 15, 36, 9402-9405, 9411-9414) | 12 | não migrar. Excluir do catálogo antes da migração |
| **Parados há mais de 2 anos** (12, 17, 19, 20, 21, 22, 26, 27, 28, 30, 40, 9401, 9407, 9409, 9410, 3) | 16 | não migrar. Arquivar o histórico |
| **Parados entre 1 e 2 anos** (1, 6, 13, 14, 29, 38, 9400) | 7 | confirmar com o dono do processo antes de descartar |
| **Pararam em 2025** (11, 18 Cobrança, 25, 31, 33) | 5 | **investigar** — ver seção 7 |
| **Teste** (44, 9414) | 2 | excluir, e impedir que teste rode em produção |
| Outros com uso residual | 3 | avaliar caso a caso |

---

## 7. 🟡 Um achado novo: Cobrança parou junto com a integração financeira

`18 Cobrança` é o modelo com **maior volume histórico do sistema — 65.208 processos**. E parou:
**última agenda em 21/05/2025**, zero em 2026.

Coincide com o outro represamento que já tínhamos medido:

| Evento | Data |
|---|---|
| Última agenda de Cobrança | **21/05/2025** |
| Títulos param em `IMP_Titulo`, nunca promovidos a `EXT_Titulo` | **22/05/2025** |

Um dia de diferença. A régua de cobrança depende do título; o título parou de ser carregado; a
cobrança parou junto. **São o mesmo incidente**, e ninguém ligou os dois — porque a integração
falhou registrando apenas `Warning`.

> **Ação:** isto é um incidente do sistema atual, não do projeto. Vale abrir com o fornecedor
> independentemente do CRM novo — são 15 meses de régua de cobrança parada. E entra no documento 01
> como confirmação da cadeia causal.

---

## 8. 🟢 Qualidade de dado: datas de agenda sem validação

Medido em `IV_Agenda` (966.041 linhas):

| Achado | Quantidade |
|---|---|
| Agendas com data posterior a 2030 | **19** |
| Dessas, ainda pendentes | **3** |
| Maior data encontrada | **20/08/5173** |
| Outras: 2314, 2116, 2112, 2110, 2061, 2039, 2035 | — |

**É pequeno — 0,002% das agendas.** Não é crise, e seria desonesto apresentar como tal. Mas prova
que **não há validação de faixa na data da agenda**, e as 3 pendentes são tarefas que nunca vão
aparecer como atrasadas: estão agendadas para daqui a séculos.

> **No CRM novo:** `CHECK` de faixa em `Tarefa.AgendadaPara` (entre hoje − 5 anos e hoje + 5 anos),
> e validação na entrada. Custo: uma linha de DDL.

---

## 8.1 🔴 949 tarefas fora de qualquer fluxo — e 100% pendentes

Ao somar as agendas dos 17 fluxos vivos, sobravam 988 contra o total de 2026. A causa:

> **987 agendas de 2026 têm `CodProcesso` NULO.**

| O que | Agendas | Pendentes | Usuários |
|---|---:|---:|---:|
| **Sem ação e sem fluxo** | **949** | **949 — 100%** | 21 |
| VALIDAR CARTEIRA PEÇAS (431) | 34 | 3 | 3 |
| TESTE ENVIO E-MAIL (904) | 2 | 0 | 1 |
| DAR BOAS-VINDAS 1º JD (491) | 1 | 0 | 1 |
| PROGRAMAR TIPO DE SERVIÇO JD (413) | 1 | 0 | 1 |

As 949 **estão vinculadas a um processo**, mas fora de qualquer fluxo. Foram criadas ao longo de
todo 2026 — a mais antiga em 07/01, a mais recente em **31/08/2026 às 11:04**, no dia deste
levantamento. Estão espalhadas por **21 usuários**.

**Hipótese** (não confirmada): são tarefas livres criadas pelo botão "+Nova agenda" da tela de
Agenda, que nascem sem tipo de ação e sem tipo de fluxo. Confirmar exigiria checar
`IV_Agenda.TarefaCompromisso`.

**O que está confirmado é a consequência.** O motor casa regra por `Resultado` e por `CodProcesso`.
Sem os dois, **nenhuma regra pode disparar sobre elas**: nada as move, nada as fecha, nada avisa.
Elas entram na agenda e ficam. É a mesma doença dos 155 processos travados em Entrega, num lugar
diferente — e ajuda a explicar por que a agenda de um CEN chega a 194 tarefas atrasadas.

> **No CRM novo é impossível por construção:** `wf.Tarefa.TipoTarefaId` é `NOT NULL` com FK
> (documento 04, seção 5.4). Tarefa avulsa — sem processo — continua tendo tipo, logo continua
> tendo desfechos válidos e regras que a encerram. Não existe tarefa que o sistema aceite criar
> e depois não saiba governar.

```sql
-- As agendas de 2026 que não pertencem a fluxo nenhum
SELECT a.Acao, ac.Descricao, COUNT(*) AS Agendas,
       SUM(CASE WHEN a.Realizada = 'S' THEN 0 ELSE 1 END) AS Pendentes,
       COUNT(DISTINCT a.SeqUsuario) AS Usuarios
FROM   IV_Agenda a WITH (NOLOCK)
LEFT JOIN IV_Acao ac WITH (NOLOCK) ON ac.Acao = a.Acao
WHERE  a.DtaAgenda >= '2026-01-01' AND a.DtaAgenda < '2027-01-01'
  AND  a.CodProcesso IS NULL
GROUP BY a.Acao, ac.Descricao ORDER BY Agendas DESC;
```

---

## 9. Resumo — o que muda no plano

| # | Descoberta | Efeito |
|---|---|---|
| 1 | 62 modelos, **17 vivos**, 5 concentram 90% | escopo de migração cai de 62 para 17, e a ordem fica clara |
| 2 | Fluxo 32 tem o **menor catálogo e o 2º maior volume** | confirma Prospecção como fase 1 |
| 3 | Fluxo 50 usa **47 ações e 178 resultados** | **16 semanas para a fase 3 é apertado** — rever para 18–20 |
| 4 | "JDE" no nome ≠ integração morta | **decidir por uso medido, nunca por nome** |
| 5 | O modelo de processo acumula forma + marca + linha + geração | separar em `TipoProcesso` × `LinhaNegocio` × `Marca` × `Versao` |
| 6 | Cobrança parou junto com a carga de títulos | incidente do sistema atual, para abrir com o fornecedor |
| 7 | "Teste Workflow" rodando em produção em 2026 | impedir teste em produção no CRM novo |
| 8 | 19 agendas com data absurda | `CHECK` de faixa — custo trivial |
| 9 | **949 tarefas sem fluxo, 100% pendentes, em 21 usuários** | `TipoTarefaId` NOT NULL com FK — nenhuma tarefa fora de governo |

---

## Como este documento foi levantado

Tela **"Códigos de processos cadastrados"** (capturas de 31/08/2026) para o catálogo,
cruzada com consultas read-only ao banco de produção:

```sql
-- Uso real por modelo de processo
SELECT cp.CodProcesso, cp.Descricao,
       COUNT(DISTINCT a.Processo) AS ProcTotal,
       COUNT(DISTINCT CASE WHEN a.DtaAgenda >= '2026-01-01' THEN a.Processo END) AS Proc2026,
       MAX(a.DtaAgenda) AS UltimaAgenda
FROM IV_CodProcesso cp WITH (NOLOCK)
LEFT JOIN IV_Agenda a WITH (NOLOCK) ON a.CodProcesso = cp.CodProcesso
GROUP BY cp.CodProcesso, cp.Descricao
ORDER BY Proc2026 DESC;

-- Tamanho do catálogo por fluxo vivo (agendas e resultados contados em CTEs
-- separadas: juntar IV_Agenda com IV_Historico na mesma consulta infla a contagem)
WITH Ag AS (
    SELECT CodProcesso, COUNT(*) AS Agendas2026,
           COUNT(DISTINCT Processo) AS Processos2026,
           COUNT(DISTINCT Acao) AS AcoesUsadas,
           SUM(CASE WHEN Realizada = 'S' THEN 0 ELSE 1 END) AS Pendentes
    FROM IV_Agenda WITH (NOLOCK)
    WHERE DtaAgenda >= '2026-01-01' AND DtaAgenda < '2027-01-01'
    GROUP BY CodProcesso
),
Hs AS (
    SELECT CodProcesso, COUNT(DISTINCT Resultado) AS ResultadosUsados
    FROM IV_Historico WITH (NOLOCK)
    WHERE DtaRealizacao >= '2026-01-01'
    GROUP BY CodProcesso
)
SELECT Ag.*, ISNULL(Hs.ResultadosUsados, 0) AS ResultadosUsados
FROM Ag LEFT JOIN Hs ON Hs.CodProcesso = Ag.CodProcesso
ORDER BY Ag.Agendas2026 DESC;
```
