# 45 — Execução da Fase 2: auditoria automática

**Data:** 16/09/2026
**Autorização:** "Sim siga para a fase 2", dada nesta sessão depois do commit da fase 1 (`cb05b7c`).
**Plano de origem:** [41 — Plano executivo da reestruturação](41-PLANO-EXECUTIVO-DA-REESTRUTURACAO.md), fase 2.
**Fase anterior:** [44 — Execução da fase 1](44-EXECUCAO-FASE-1-SIMPLIFICACAO-ESTRUTURAL.md).
**Migration:** `20260916112755_AuditoriaComOrigemDaOperacao`.

O que esta fase fez, em uma frase: **toda gravação de campo auditado passa a deixar trilha em
`auditoria.AlteracaoDeCampo` automaticamente, na mesma transação do dado, dizendo de onde veio
(pessoa, integração, importação, sistema ou rotina), de qual sistema, em que operação e em que
requisição** — e as quatro cargas deixaram de escrever a trilha à mão.

> **Resultado: 4 dos 5 critérios de aceite da ficha atendidos. O critério de desempenho não foi
> atendido** (§6). A fase está implementada, testada e aplicada ao banco local. **Commit autorizado e
> feito em 16/09/2026**, separado da publicação: a decisão sobre desempenho continua pendente, e
> nada foi aplicado ao servidor.

**Ponto de restauração:** `dados-locais/bancos/antes-fase-2-reestruturacao-20260916.bak`
(52,2 MB, `COPY_ONLY`, `CHECKSUM`). Os pontos anteriores não foram tocados.

---

## 1. Antes

| O que a ficha dizia | Conferido |
|---|---|
| `AlteracaoDeCampo` só recebe linha das cargas, que a chamam à mão | ✔ 7 chamadas a `AlteracaoDeCampo.Registrar` em 4 cargas (território, consolidação de grafias, ART, Vórtice) |
| a API não audita nada; não há interceptador | ✔ nenhum `SaveChanges` da API gravava trilha |
| `CorrelacaoId` nunca é preenchido | ✔ coluna e índice existiam; nenhuma escrita |
| a política de campos saiu na fase 1 | ✔ `CampoAuditado` removida em `cb05b7c` |

Banco local antes da migration: 50 tabelas, 665 colunas, 232 índices, 111 CHECK, 122 FKs,
36 defaults, 1 tabela particionada, 13 migrações, 6.148 linhas, **0 linhas na trilha**.

---

## 2. O que mudou

### 2.1 Banco — `auditoria.AlteracaoDeCampo` (a contagem de tabelas não muda: 50)

| Coluna nova | Tipo | Regra |
|---|---|---|
| `Origem` | `varchar(12) NOT NULL` | `CK_AlteracaoDeCampo_Origem`: `Usuario`, `Integracao`, `Importacao`, `Sistema`, `Job` |
| `Operacao` | `varchar(10) NOT NULL` | `CK_AlteracaoDeCampo_Operacao`: `Inclusao`, `Alteracao`, `Exclusao` |
| `SistemaId` | `int NULL` | `FK_AlteracaoDeCampo_Sistema_SistemaId` → `integracao.Sistema`; índice `IX_AlteracaoDeCampo_SistemaId` |

A migration foi **reescrita à mão em dois pontos** em relação ao que o EF gerou:

1. **Colunas obrigatórias nascem anuláveis, recebem valor e só então ficam `NOT NULL`.** O gerador
   emitiria `NOT NULL DEFAULT ''`, e o texto vazio viola a CHECK nova no primeiro banco que já tenha
   trilha — o do servidor tem as linhas da carga do ART. As linhas antigas recebem `Integracao` /
   `Alteracao`, que é o que elas foram (só as cargas gravavam trilha, e só alteração do que já
   existia). O `SistemaId` delas fica **nulo**: adivinhá-lo pelo nome da entidade seria inventar dado
   na tabela que existe para não ter dado inventado.
2. **O índice novo nasce sobre o esquema de partição** `PS_Mensal_AlteracaoDeCampo`. O gerador o
   criaria no filegroup padrão, desalinhado — o que impede `SWITCH` e `TRUNCATE` por partição, que
   são o motivo de a tabela ser particionada.

Nenhum default constraint foi criado (36 antes, 36 depois).

### 2.2 Domínio

| Arquivo | O que é |
|---|---|
| `Auditoria/OrigemDaOperacao.cs` (novo) | `OrigemDaOperacao` e `OperacaoAuditada` |
| `Auditoria/PoliticaDeAuditoria.cs` (novo) | **a política do que se audita, em código** — ver §2.4 |
| `Auditoria/Auditoria.cs` | `AlteracaoDeCampo` ganha `Origem`, `SistemaId`, `Operacao` |
| `Seguranca/ContextoAcesso.cs` | ganha `Origem` (padrão: `Sistema` para serviço de sistema, `Usuario` para pessoa), `SistemaId` e `CorrelacaoId` (um por contexto — logo, um por requisição) |

### 2.3 Infraestrutura

| Arquivo | O que é |
|---|---|
| `Persistencia/TrilhaDeAuditoria.cs` (novo) | captura antes de salvar, grava depois — ver §5.1 |
| `Persistencia/CrmDbContext.cs` | `SaveChanges(bool)` e `SaveChangesAsync(bool, …)` passam a gravar a trilha; `DeclararOrigemDasGravacoes(origem, sistemaId)` para a carga declarar de onde vem |
| `Configuracoes/DocumentoEAuditoriaConfiguracao.cs` | mapeamento das três colunas, FK, índice e as duas CHECK |

### 2.4 A política — o que entra na trilha

| Entidade | Campos | O que fica de fora, e por quê |
|---|---|---|
| `Cliente` | `NomeRazao`, `NomeFantasia`, `TipoDePessoa`, `Documento`, `InscricaoEstadual`, `AtividadeEconomica`, `Situacao`, `MotivoInativacaoId`, `ProprietarioId`, `ClienteMatrizId`, `OrigemId`, `ExcluidoEm` | classe ABC e faturamento apurado: recalculados pela curva, mudariam milhares de linhas por rodada |
| `Equipamento` | `ClienteId`, `ModeloId`, `LinhaDeProdutoId`, `Chassi`, `NumeroSerie`, `Placa`, `AnoFabricacao`, `AnoModelo`, `EnderecoId`, `EquipamentoPaiId`, `EquipamentoSubstitutoId`, `Situacao`, `VendidoEm`, `GarantiaAte`, `ExcluidoEm` | horímetro: é leitura, não decisão |
| `Endereco` | `MunicipioId` | o que as correções de grafia mudam — era o que a carga já registrava |
| `Municipio` | `CodigoIbge`, `Nome` | idem, reconhecimento no IBGE |
| `VendaDeMaquina` | os 16 campos que a origem reescreve (`VendaDeMaquina.AtualizarDaOrigem`) | — |

Em nenhuma entidade: `CriadoEm`, `CriadoPorId`, `AlteradoEm`, `AlteradoPorId`, `Versao`,
`ChavePublica` — carimbo muda em toda gravação e não é decisão de ninguém. Um teste de arquitetura
proíbe que entrem.

**Três regras da trilha**, todas cobertas por teste:

1. **Inclusão só entra quando é pessoa que cria.** O que nasce de integração ou importação já tem o
   rastro de origem gravado ao lado (`RegistroDeOrigem`, `ChaveExterna`, com o valor como veio). É a
   mesma regra que as cargas seguiam à mão.
2. **Exclusão lógica é `Exclusao`**, não `Alteracao`: quando `ExcluidoEm` passa a ter valor, todas as
   linhas daquela gravação são marcadas assim.
3. **"Mudou?" é perguntado como o banco pergunta.** `CK_AlteracaoDeCampo_Mudou` compara sem
   maiúscula nem acento; "FAZENDA SAO JOAO" → "Fazenda São João" não é mudança para ele, e a linha
   seria recusada **derrubando a gravação do cliente junto**. A trilha pula o que o banco não
   reconhece como mudança.

### 2.5 Cargas

| Carga | Antes | Depois |
|---|---|---|
| Território — IBGE (catálogo de municípios) | `Registrar` à mão, com checagem de colação própria | `DeclararOrigemDasGravacoes(Integracao, IBGE)` |
| Território — IBGE (área plantada) | sem trilha (entidade fora da política) | `DeclararOrigemDasGravacoes(Integracao, IBGE)` |
| Território — planilhas | sem trilha | `DeclararOrigemDasGravacoes(Importacao, PLANILHA)` |
| Consolidação de grafias cortadas | `Registrar` à mão | `DeclararOrigemDasGravacoes(Integracao, IBGE)` |
| ART | 3 chamadas a `Registrar`, só para registro existente | `DeclararOrigemDasGravacoes(Integracao, ART)` |
| Vórtice (**congelada**, D-12) | 1 chamada a `Registrar` | as duas classes abrem contexto por `AbrirContextoDaCarga()`, que declara `Integracao` com o sistema do legado. Mudança mínima, só para compilar sem duplicar a trilha; a carga continua bloqueada |

`CargaDeTerritorio` perdeu o parâmetro `empresaDeCasaId`, que só servia à trilha manual — a filial
de casa agora vem do contexto de acesso, que já era construído com o mesmo valor.

**O que muda no conteúdo da trilha das cargas:** os valores passam a ser o que o banco guarda
(`"4521"` → `"4530"`), e não o texto explicativo que a carga montava (`"4521 (SAO JOSE DO RIO PRET)"`).
O sistema e a origem, que antes não existiam, cobrem o contexto.

---

## 3. Depois — contagem real

| Medida | Antes | Depois | Diferença |
|---|---:|---:|---:|
| Tabelas | 50 | 50 | 0 |
| Colunas | 665 | **668** | +3 |
| Índices | 232 | **233** | +1 |
| CHECK | 111 | **113** | +2 |
| Chaves estrangeiras | 122 | **123** | +1 |
| Defaults | 36 | 36 | 0 |
| Tabelas particionadas | 1 | 1 | 0 |
| Índices da trilha fora da partição | 0 | **0** | 0 |
| FKs / CHECK não confiáveis | 0 / 0 | 0 / 0 | 0 |
| Migrações | 13 | 14 | +1 |
| Linhas | 6.148 | 6.149 | +1 (o registro da migration) |

Medido no banco local (`TracbelCrm`) e, idêntico, no banco de ensaio.

---

## 4. Testes executados

### 4.1 A migration no banco de ensaio

Restauração do ponto de restauração em `TracbelCrmEnsaioFase2`, no contêiner `tracbel-crm-ensaio`:

| Passo | Resultado |
|---|---|
| `up` | ✅ números da §3 |
| `down` | ✅ **as 13 medidas voltaram idênticas ao antes** |
| linha de trilha no formato antigo inserida à mão, e `up` de novo | ✅ a linha recebeu `Integracao` / `Alteracao` / `SistemaId` nulo — o caminho do servidor, que tem trilha |
| aplicação no banco local | ✅ |
| `DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS` depois de 440 gravações pela API | ✅ 0 violação |

### 4.2 A trilha no SQL Server de verdade

Depois de 440 clientes criados pela API contra o ensaio: **1.760 linhas** (4 campos preenchidos por
inclusão), todas `Usuario` / `Inclusao`, **440 correlações distintas** (uma por requisição),
**0 cliente sem trilha**, **0 linha de trilha apontando para cliente inexistente**, gravadas na
partição do mês.

### 4.3 Build e testes

| Verificação | Resultado |
|---|---|
| `dotnet build` | ✅ 0 erros, 0 avisos |
| `Dominio.Testes` | ✅ 163 |
| `Integracao.Testes` | ✅ 133 |
| `Api.Testes` | ✅ 93 (**+3 novos**) |
| `Aplicacao.Testes` | ✅ 58 (**+3 novos**) |
| `Arquitetura.Testes` | ✅ 64 (**+2 novos**; inclui a cadeia inteira de migrations do zero em contêiner e o alinhamento de índice à partição) |
| **Total** | **✅ 511 casos, 0 falhas** (eram 503) |
| `npm run build` / `npm run lint` | ✅ (frontend sem nenhuma alteração nesta fase) |
| 12 telas contra o banco local migrado | ✅ 0 quebra, 0 erro de console, 0 erro de API |

**Testes novos:**

| Teste | Prova |
|---|---|
| `AuditoriaAutomaticaNaApiTestes.Criar_alterar_e_inativar_pela_API_…` | criar gera `Inclusao`, alterar gera `Alteracao` com antes e depois, inativar gera `Exclusao`; sempre `Usuario`, o autor, a filial e uma correlação por requisição |
| `…Campo_fora_da_politica_nao_entra_na_trilha` | carimbos não entram |
| `…Mudanca_so_de_maiuscula_ou_acento_…` | não gera linha **e a gravação não cai** |
| `TrilhaDeAuditoriaTestes.A_origem_declarada_…` | a origem e o sistema declarados pela carga chegam à linha |
| `…O_registro_que_nasce_de_integracao_…` | nascimento por integração não entra; por pessoa, entra |
| `…Sem_como_gravar_a_trilha_o_dado_tambem_nao_e_gravado` | **sem autor nem filial para a trilha, a exceção sobe e o dado fica como estava** |
| `AuditoriaTestes.A_politica_… so_nomeia_… que_existem_no_modelo` | um campo renomeado e esquecido na política quebra o build, não a produção |
| `AuditoriaTestes.A_politica_… nao_audita_carimbo_…` | carimbo e concorrência fora da trilha |
| `ConsolidacaoDeGrafiasCortadasTestes` (ampliado) | a carga gera `Integracao` com o `SistemaId` do IBGE |

---

## 5. Decisões técnicas tomadas durante a execução

### 5.1 Não é um `ISaveChangesInterceptor` — é o `SaveChanges` do contexto

A ficha previa um `InterceptadorDeAuditoria`. Não funciona para o requisito "na mesma transação":

- o registro que nasce só tem `Id` **depois** do `INSERT` (coluna `IDENTITY`), e a linha da trilha
  precisa desse `Id`;
- na API a conexão tem **retentativa em falha transitória**, e nela transação explícita só é segura
  dentro da estratégia de execução, salvando sem aceitar as mudanças e aceitando depois do commit.

O caminho implementado: captura antes de salvar (é só ali que o valor original existe); se quem
chamou já abriu transação (a carga), salva e grava a trilha dentro dela; se não (a API), abre uma
dentro da estratégia de execução, salva com `acceptAllChangesOnSuccess: false`, grava a trilha,
faz commit e só então aceita. A trilha é gravada por **`INSERT` direto**, não por entidade
rastreada, porque um segundo `SaveChanges` reinseriria o registro novo que ainda está "a incluir".
Gravação sem campo auditado continua exatamente como antes, sem transação a mais.

### 5.2 Falha da trilha é falha da gravação

Nada é engolido. Sem filial ou sem autor para a trilha, a exceção sobe com a mensagem dizendo o
que falta, a transação é desfeita e o dado não é gravado — provado por teste.

### 5.3 Retenção (D-10) não foi decidida

Continua a declarada na migração inicial: 18 meses disponíveis. A decisão é de Ricardo com o
jurídico, e não foi tomada nesta fase.

---

## 6. Critérios de aceite da ficha

| Critério | Resultado |
|---|---|
| criar, alterar e inativar um cliente pela API gera linhas com `Origem = Usuario`, autor e `CorrelacaoId` | ✅ teste + 440 gravações no SQL Server |
| uma execução de carga gera linhas com `Origem = Integracao` e o `SistemaId` certo | ✅ teste da consolidação (IBGE) e da origem declarada. **Nenhuma carga real foi executada**: IBGE e planilhas exigem rede e arquivos do comercial, o ART segue desabilitado e o Vórtice está congelado |
| campo fora da política não gera linha | ✅ |
| **p95 do `POST /api/v1/clientes` não piora mais que 10%** | ❌ **não atendido** — ver abaixo |
| retenção (D-10) registrada na migração e no documento 14 | ⏸ **pendente** — a decisão D-10 não existe ainda |

### 6.1 O desempenho, medido

Código de antes (commit `cb05b7c`, montado numa worktree temporária) e código de depois, **três
rodadas intercaladas**, cada uma num banco restaurado limpo do ponto de restauração, no contêiner
de ensaio, 40 requisições de aquecimento e 400 medidas. Tempo registrado **pelo próprio servidor**
(`Request finished`), que não tem o ruído do cliente HTTP:

| Rodada | p50 antes | p50 depois | p95 antes | p95 depois |
|---|---:|---:|---:|---:|
| 1 | 26,4 ms | 34,4 ms | 42,7 ms | 59,9 ms |
| 2 | 15,3 ms | 21,7 ms | 27,2 ms | 46,3 ms |
| 3 | 15,0 ms | 19,7 ms | 23,0 ms | 29,9 ms |

**A criação de cliente ficou cerca de 5 ms mais lenta em p50, estável nas três rodadas; o p95
piorou de 30% a 70%.** O limite era 10%.

**Por quê.** Os comandos em si continuam custando ~1 ms cada (log do EF). O que mudou é o número de
idas ao banco: antes, um `INSERT` sem transação; depois, abertura de transação, `INSERT` do cliente,
`INSERT` da trilha e commit — três idas a mais. No Docker Desktop cada ida custa entre 1 e 2 ms, e
numa gravação de 15 ms isso é muito em proporção.

**O que não resolve:** qualquer trilha "na mesma transação" de um registro com `IDENTITY` pelo EF Core
precisa de pelo menos duas idas a mais que um `INSERT` solto. Dá para tirar **uma** ida só nas
alterações (não nas criações, que é o que o critério mede), à custa de complexidade no provedor de
teste.

**Caminhos possíveis — a decisão é sua:**

| Opção | Efeito | Risco |
|---|---|---|
| **A. Medir no servidor antes de decidir** (recomendado) | lá o SQL Server é nativo e a ida ao banco custa uma fração da do Docker Desktop; o número que importa é o de produção | nenhum — só adia a decisão |
| B. Aceitar o custo e registrar o critério como revisto | ~5 ms por gravação auditada | nenhum técnico |
| C. Trilha fora da transação nas inclusões | uma ida a menos | **dado sem trilha** se a segunda gravação falhar — contraria o objetivo da fase |
| D. Chave por sequência nas entidades auditadas | trilha no mesmo lote do `INSERT` | mudança de chave em `Cliente` e `Equipamento`, com reconstrução de tabela — escopo de outra fase |

---

## 7. O que fica pendente

1. **Decisão sobre o critério de desempenho** (§6.1).
2. ~~**Commit.**~~ Feito em 16/09/2026 ("sim você fará o commit de cada fase"), com a mensagem
   `feat(db): trilha de auditoria automática com origem da operação`. **Sem push.** O commit não
   decide o desempenho nem publica: essas continuam sendo os itens 1 e 4.
3. **D-10** — retenção da auditoria, com o jurídico.
4. **Servidor.** A migration não foi aplicada ao banco central. Lá já existe trilha da carga do ART:
   o caminho de preenchimento das linhas antigas foi ensaiado (§4.1).
5. **Evento de acesso (LGPD)** continua fora — a tabela saiu na fase 1 e a ficha da fase 2 não o
   incluía.
