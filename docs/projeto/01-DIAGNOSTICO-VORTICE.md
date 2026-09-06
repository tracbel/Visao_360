# Diagnóstico do Vórtice — o levantamento com evidência

> **Documento 01 de 07** · Versão 1.0 · 30/08/2026
> Levantamento feito no banco de produção, em modo read-only. **Cada achado tem a query que prova.**
> Detalhe completo por frente: [`docs/pesquisa/`](../pesquisa/) — 15 documentos, 387 achados.

---

## 1. Como ler este documento

Este é o dossiê técnico que justifica o projeto. Ele existe para que ninguém precise aceitar por
confiança que "o Vórtice está ruim" — e para que, quando alguém perguntar "mas é tão grave assim?",
haja uma query para rodar.

Severidade: 🔴 crítico · 🟡 relevante · 🟢 registrado

---

## 2. Escala do sistema

| Métrica | Valor |
|---|---|
| Tabelas | **767** (131 vazias — 35% das `IV_`) |
| Colunas | 19.859 |
| Views | **411** (290 auto-geradas por metadados) |
| Foreign keys | 672 — **nenhuma no caminho quente do BPM** |
| Linhas totais | **85,5 milhões** |
| Linhas de log | **44,8 milhões — 52,4% do banco** |
| Procedures / functions | 6 / 1 |
| Documentação do fornecedor | **nenhuma** |

Volumes de negócio: `GE_Pessoa` 118.463 · `IV_Processo` 1.174.932 · `IV_Historico` 2.436.127 ·
`IV_Agenda` 931.989 · `EXT_NFS` 391k · `EXT_Titulo` 578k.

---

## 3. 🔴 Integração — parada há 15 meses

| # | Achado | Medida |
|---|---|---|
| 3.1 | Títulos presos em `IMP_Titulo`, nunca promovidos | **783.242 títulos · R$ 5,18 bi**, desde **22/05/2025** |
| 3.2 | OS que nunca chegaram a `EXT_OS` | **280.214 de 287.868** — e marcadas como processadas |
| 3.3 | Faturamento morreu em 3 datas | serviços 29/08/2024 · máquinas 07-08/02/2025 · peças 11/04/2025 |
| 3.4 | `RUNGEPIMPORT` falha a cada 20 min | `Versão incompatível [4.04.01r05] x [4.04.01r01]` — nível **Warning** |
| 3.5 | JD Edwards | **7 tabelas, todas com 0 linhas.** Job nunca agendado |
| 3.6 | ETL Pentaho | **parado desde jul/2021** |
| 3.7 | Procedures da ACL | **sem transação e sem TRY/CATCH** — `BEGIN TRANSACTION` comentado |
| 3.8 | Datas do Protheus | sentinelas `1900-01-01` tratadas coluna a coluna; o que escapa vira vencimento em **5024** e **2223** |
| 3.9 | Fila de e-mail | **22.512 falhas silenciosas, sem retry** |

```sql
-- 3.1 -- prova do represamento financeiro
SELECT COUNT(*) AS titulos, SUM(VlrOriginal) AS valor, MAX(DtaInclusao) AS ultima_entrada
FROM IMP_Titulo WITH (NOLOCK);
SELECT MAX(DtaInclusao) FROM EXT_Titulo WITH (NOLOCK);  -- a promoção parou aqui
```

**Efeito:** qualquer análise de faturamento, inadimplência ou pós-venda feita no Vórtice hoje **está
errada**.

---

## 4. 🔴 CRM e ERP são duas ilhas

`EXT_*` (ERP) e `IV_*` (CRM/BPM) se ligam **apenas por `SeqPessoa`**. Não há `IdNFS`, `IDOS` nem
`idTitulo` em `IV_Historico` ou `IV_ProcDado`. **99,1% das notas fiscais não têm `IdVeic`.**

A ponte real entre a venda e a nota é um **formulário digitado à mão**: `IV_Q_ACOMP_VENDA_FINANC`
(chassi, número da NF, data de faturamento).

**Consequência:** é impossível responder "esta oportunidade virou qual nota fiscal?" por consulta.

---

## 5. 🔴 Segurança

| # | Achado | Medida |
|---|---|---|
| 5.1 | Permissão por registro | **não existe.** `sys.security_policies` = 0; **1 trigger no banco inteiro** |
| 5.2 | Onde a segurança é aplicada | **100% no cliente Gupta.** Acesso ao banco ignora tudo, sem rastro |
| 5.3 | Senha sem salt | 663 usuários → **467 valores distintos**; **80 compartilham o mesmo** |
| 5.4 | Política de senha | **1 de 11 políticas**, força "Fraco", sem expiração |
| 5.5 | Auditoria de login | **114 eventos em 9 anos** |
| 5.6 | Papéis/perfis | **não existem.** 939 usuários → **370 combinações** de 51 flags. `IV_SegPerfil` vazia |
| 5.7 | Bloco `IVC_` (equipes) | **totalmente vazio** |
| 5.8 | Escopo de visibilidade | 4 rotas paralelas em código; `SeqUrSuperv` **NULL em 655/655** carteiras |
| 5.9 | Rotas de escopo | só valem para o **mobile** — no desktop não há equivalente |
| 5.10 | Hierarquia | ponteiro de **1 nível**, sem closure, **sem reatribuição** |
| 5.11 | Multiempresa | 18 filiais, mas **340 de 409 usuários acessam 17** |
| 5.12 | Desativar usuário | **destrói as permissões** — não há flag ativo/inativo |
| 5.13 | Contas | **70% nunca logaram**; 87% dormentes >90 dias |
| 5.14 | Token da API | **texto claro** em tabela de parâmetros |
| 5.15 | Permissão por campo | existe, amarrada ao nome do widget Gupta — **17 linhas** |
| 5.16 | Janela de horário | existe — **3 linhas** |
| 5.17 | Usuários sem política nenhuma | **710 de 1.386 — 51%** |
| 5.18 | Permissão por tela | cobre **10 de 184 telas** registradas |
| 5.19 | `NROEMPRESA` na permissão | **um único valor distinto** — multiempresa nunca usada |
| 5.20 | Coluna de inativação em `GE_Usuario` | **não existe** — confirmado no schema |
| 5.21 | Colunas de senha no cadastro | **três**: `Senha`, `Senha3`, `ChkSum` |
| 5.22 | Usuários ativos em 90 dias | **≈140 de 1.386 cadastrados (10%)** |

```sql
-- 5.3 -- prova de que a senha não tem salt
SELECT COUNT(*) AS com_senha, COUNT(DISTINCT Senha) AS valores_distintos
FROM GE_Usuario WITH (NOLOCK) WHERE Senha IS NOT NULL AND Senha <> '';
-- 663 / 467  -> senhas iguais produzem hashes iguais
```

---

## 6. 🔴 LGPD

| # | Achado |
|---|---|
| 6.1 | As views `GE$PESSOA_LGPD`, `GE$CONTATO_LGPD`, `GE$EMAIL_LGPD`, `GE$PESSOAFONE_LGPD` mascaram a coluna **e republicam o valor cru na mesma view**, em `z_NOMERAZAO`, `z_NROCGCCPF`, `z_EMAIL`, `z_FONENRO1`. **A anonimização é contornável com um `SELECT`.** |
| 6.2 | Opt-in **sem data, sem origem e sem prova de consentimento** — apenas um flag. Não sustenta solicitação de titular |
| 6.3 | 108 itens de política de segurança configuráveis, com mascaramento por view — anulado por 6.1 |

---

## 7. 🔴 Motor de workflow

| # | Achado | Medida |
|---|---|---|
| 7.1 | **Regra não tem condição** | `IV_AcaoAutoCtrl` **vazia**; `UsaObjDyn = 0` em 100% |
| 7.2 | Explosão cartesiana de regras | **5.958 regras** para **1.262 pares (Resultado, Ação)** distintos |
| 7.3 | Destinatário fixo na linha | regra 9473 mandou **150 tarefas** para o mesmo usuário desde 10/04/2026 |
| 7.4 | `SeqUsuario = 0` devolve para quem lançou | validado em **20 de 20** casos recentes |
| 7.5 | Aprovação não se reatribui | grava o líder **no momento da geração**; trocar o líder não corrige as existentes |
| 7.6 | Ação 900 deixou de ser gerada | **68% (mar) → 3% (ago/2026)**, sem alarme |
| 7.7 | Processos travados | **155 de 239** em `Fase = Entrega` sem nenhuma agenda pendente |
| 7.8 | "Atividade Cancelada" cancela o processo | resultado 3438 grava `Fase=Finalizado, Status=CANCELADO`. **Sem desfazer** |
| 7.9 | Ação 897 tem limite **vitalício** | `QtdeMaxPessoa = 1`; 189 de 193 casos bloqueados, só 7 tinham tarefa aberta |
| 7.10 | Semântica de `IV_ProcResultado` não documentada | fase aplicada é `COALESCE(FaseSeguinte, Fase)`; **67% das 2.160 linhas** não declaram transição |
| 7.11 | Formulário gera DDL | **175 tabelas físicas `IV_Q_*`** criadas em runtime, nunca coletadas |
| 7.12 | Catálogo apodrecido | **980 ações (169 usadas em 2026)** · **4.208 resultados (501 usados)** |
| 7.13 | Colunas mortas | `IV_AcaoAuto` tem 31 colunas, **9 delas 100% nulas** |
| 7.14 | Órfãos | **345.535 linhas** de `IV_ProcDado` sem processo |
| 7.15 | Três motores de automação coexistem | e só um é conhecido pela operação |

```sql
-- 7.2 -- a explosão de regras
SELECT COUNT(*) AS regras,
       COUNT(DISTINCT CONCAT(Resultado,'|',Acao)) AS pares_distintos
FROM IV_AcaoAuto WITH (NOLOCK);   -- 5958 / 1262

-- 7.1 -- a tabela de condições está vazia
SELECT COUNT(*) FROM IV_AcaoAutoCtrl WITH (NOLOCK);   -- 0
```

---

## 8. 🔴 Relatórios

| # | Achado | Medida |
|---|---|---|
| 8.1 | Relatório é **SQL cru** em coluna `TEXT` | 134 relatórios em `GE_QVCons.InstrSql`, sobrescrita a cada edição |
| 8.2 | Sem versionamento | histórico tem **69 linhas para 134 relatórios**, última em abr/2025 |
| 8.3 | Clonagem substituiu versionamento | **9 gerações** do mesmo "Análise da Carteira de Pedidos" |
| 8.4 | Sem segurança de linha | **125 dos 134** sem nenhum predicado de usuário |
| 8.5 | Motor executa DML | aceita `UPDATE`, `INSERT`, `EXEC`, `COMMIT` |
| 8.6 | Staging nunca limpa | **nenhum `DELETE FROM`** nos 134 corpos; `IMP_REL_TBA101_PG2` foi de 208 → 1.558 linhas |
| 8.7 | Filtro de carteira ignorado | os ramos de staging filtram só por `CODUSUARIO` |
| 8.8 | Coluna `CARTEIRA` não tem carteira | guarda o **login do vendedor** (43 de 44 valores casam com `GE_Usuario.CodUsuario`) |
| 8.9 | Uso real | **10 de 134** rodaram nos últimos 3 meses; 26 nunca rodaram |
| 8.10 | Bug na camada semântica | `IV$S_*.POSSUI_EMAIL` está com a lógica **invertida** |

---

## 9. 🟡 Qualidade do modelo de dados

| # | Achado |
|---|---|
| 9.1 | `GE_Pessoa.FoneNro1` é **`decimal(12)`** — telefone com DDI ou zero à esquerda estoura |
| 9.2 | `IV_Historico.ResultadoCmpl` é `varchar(150)` e a aplicação **trunca em 20** |
| 9.3 | `GE_Contato` é entidade **fraca**, sem FK e sem identidade — **625 órfãos** |
| 9.4 | **116 CPFs repetidos** entre clientes distintos |
| 9.5 | **Três modelos concorrentes de e-mail e três de telefone** no mesmo banco |
| 9.6 | `GE_Pessoa.Status`: 6 valores + **2.126 linhas em branco** que a view devolve como `'???'` |
| 9.7 | Motor RFV **100% morto** — 10 colunas de score nunca calculadas |
| 9.8 | Fila de deduplicação: **124 mil pares abandonados desde 2018** |
| 9.9 | Módulos entregues e nunca ativados: `IVF_`, `IVM_`, `IVP_`, `GEL_` — **34 tabelas vazias** |
| 9.10 | `Origem` é texto livre, com duplicatas e valores de teste em produção |
| 9.11 | Sync do mobile aborta em documento órfão: **5.302 vínculos (6,9%)**, em 2.635 processos |

---

## 10. 🟡 Custo de auditoria

| # | Achado | Medida |
|---|---|---|
| 10.1 | Log é a maior parte do banco | **44,8M de 85,5M linhas (52,4%)** |
| 10.2 | `GE_LOG_PROCESSO` é ruído | **96,8% são re-carimbos** de um job de cobrança — 617 linhas por processo |
| 10.3 | `GE_LgTb` congelada | **11,9M linhas desde jun/2023**, nunca expurgadas |
| 10.4 | Sem política de retenção | em nenhuma tabela de log |

---

## 11. 🟡 Infraestrutura

| # | Achado |
|---|---|
| 11.1 | Cliente principal é **Gupta/Team Developer 32 bits** via RemoteApp |
| 11.2 | Terminal server chegou a **0,42 GB livres** em 129 GB; 66 GB em ~130 perfis |
| 11.3 | **Sem backup** — a tarefa de System State está desabilitada |
| 11.4 | Auto-atualização quebrada — aponta para share de rede morto |
| 11.5 | Erro 401 do cliente = DLLs do roteador ODBC ausentes; copiar a pasta errada **apaga** essas DLLs |
| 11.6 | Envio de e-mail depende de **senha de aplicativo** que expira |
| 11.7 | Config órfã da migração de IP ainda quebra links dos e-mails |

---

## 11.8 🔴 Cinco gerações de software convivendo

As versões instaladas vão de **3.7.00** a **4.04.01r18** — cinco gerações no mesmo parque:

| Versão | Módulo |
|---|---|
| 3.7.00 | `TESTESMS` |
| 3.90.00 | `IMPORTADOREXT` (integração com ERP) |
| 4.01.01r43 | `GLOBAL/OPERADOR` |
| 4.03.01r01 | `QUERYVIEW`, `QVW/BUILDER` |
| 4.04.01r01–r18 | o núcleo |

É a causa direta do achado 3.4: o `RUNGEPIMPORT` falha a cada 20 minutos com
`Versão incompatível [4.04.01r05] x [4.04.01r01]`. Não é defeito isolado — é o resultado
previsível de uma instalação que nunca subiu por inteiro. Detalhe em
[documento 11](11-MODULOS-TELAS-PERMISSOES.md).

---

## 12. ✅ O que o Vórtice acertou

Não é um sistema sem mérito. O que vai para o CRM novo:

| Acerto | Evidência | Para onde vai |
|---|---|---|
| **As 5 entidades** (Processo, Agenda, Histórico, Ação, Resultado) | atende 6 fluxos com um núcleo só | modelo canônico do documento 04 |
| **Duplo ponteiro rastreável** | 684.110 de 931.989 agendas nasceram de histórico | `Tarefa.AtividadeConclusaoId` ↔ `Atividade.TarefaId` |
| **Separação transição/geração** | `IV_ProcResultado` × `IV_AcaoAuto` | `wf.Regra` com efeitos distintos |
| **Carteira multi-linha-de-negócio** | até 7 carteiras por pessoa; nem SF nem DYN fazem | `crm.ContaCarteira` |
| **Previsão vs baseline** | `DtaPrevConclusao` × `DtaPrevConcOrig` | `Processo.PrevisaoConclusao` × `...Original` |
| **Natureza ativo/receptivo** | 1.428.832 × 1.054.975 | `Atividade.Natureza` |
| **Geolocalização do atendimento** | lat/long em `IV_Historico` | `AtividadeVisita` |
| **Flag de regra desligada** (`EMUSO`) | 234 regras desligadas sem apagar | `Regra.EstaAtiva` |
| **Camada semântica** (`IV$S_*`) | embrião de report type | `rel.FonteRelatorio` |
| **Segmentação por potencial e cadência** | `IVS_DEPTOPOT`, 109.118 linhas vivas | `ContaCarteira.PotencialAnual` / `DiasCicloContato` |

---

## 13. Índice da pesquisa completa

| Doc | Frente | Achados |
|---|---|---|
| [04](../pesquisa/04-nucleo-crm-bpm-do-vortice-prefixo-iv-377-tabelas-motor-de-wo.md) | Núcleo BPM | 17 |
| [07](../pesquisa/07-usuarios-permissoes-e-multiempresa-no-vortice-crm-prefixos-g.md) | Segurança e usuários | 29 |
| [02](../pesquisa/02-integracoes-e-dados-de-erp-no-vortice-crm-ext-imp-x-jde-gep-.md) | Integrações ERP | 21 |
| [06](../pesquisa/06-vortice-crm-pessoas-segmentacao-financiamento-e-documentos-g.md) | Pessoas e segmentação | 17 |
| [09](../pesquisa/09-relatorios-views-e-bi-do-vortice-crm-411-views-motor-qvw-ge-.md) | Relatórios e BI | 19 |
| [03](../pesquisa/03-catalogo-de-funcionalidades-do-produto-vortice-vortico-crm-o.md) | Catálogo do produto | 42 |
