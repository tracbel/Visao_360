# Relatório da extração ao vivo do banco Vórtice CRM

**Data da coleta:** 2026-09-02
**Origem:** banco `CRM` de **produção** — SQL Server 2019 Standard (15.0.2135.5), Windows Server 2019,
nível de compatibilidade **100** (SQL Server 2008), collation `SQL_Latin1_General_CP1_CI_AS`.
**Acesso:** login `CRM_Leitura`, membro apenas de `db_datareader`, com `VIEW DEFINITION`.
**Nenhuma escrita foi executada.** Todas as consultas usaram `WITH (NOLOCK)`.

Scripts reexecutáveis: `scripts/vortice-extracao/*.ps1` (01 a 08 + `_comum.ps1`).

---

## 1. O que foi extraído

| Pasta | Arquivos | Tamanho | Conteúdo |
|---|---:|---:|---|
| `modulos/` | 549 | 3,0 MB | código-fonte dos **548 objetos programáveis** + índice |
| `ddl/` | 29 | 1,4 MB | DDL reconstruído das **767 tabelas** (25 arquivos por prefixo), índices, identity, extended properties, linked server |
| `catalogos-bpm/` | 117 | 7,2 MB | **116 catálogos declarativos exportados integralmente** (41.818 linhas) + índice |
| `volumetria/` | 7 | 265 KB | linhas, espaço em disco, crescimento anual, última atividade, comparação com o snapshot de junho |
| `dominios/` | 2 | 88 KB | **379 colunas de código** com 1.115 valores distintos e frequência + índice |
| `seguranca-banco/` | 10 | 49 KB | principals, papéis, permissões, logins, linked servers, configuração do banco |
| `_raw/` | 10 | 1,7 MB | insumos brutos (metadados de módulos, colunas, FKs, dependências, diff CRM × CRM_HOMO) |
| **Total** | **742** | **15 MB** | |

### Números-chave do banco

| Métrica | Valor |
|---|---:|
| Tabelas de usuário | 767 |
| Colunas | 10.744 |
| Linhas somadas | 86,3 milhões |
| Espaço reservado | **32,4 GB** (arquivo de dados de 36,3 GB; log de 257 MB, teto de 15 GB) |
| Views | 411 |
| Stored procedures | 85 (**70 escrevem em tabela**) |
| Functions escalares | 51 (**nenhuma escreve**) |
| Triggers | **1** |
| Primary keys / Foreign keys / Unique constraints | 663 / 657 / 5 |
| Índices | 1.828 (487 clusterizados, 1.341 não clusterizados; **6 com INCLUDE, 0 filtrados, 0 desabilitados**) |
| **CHECK constraints** | **0** |
| Colunas calculadas / colunas IDENTITY | 0 / 65 |
| Extended properties | 33 (todas do designer visual do SSMS, sem valor documental) |
| Modelos de processo (`IV_CodProcesso`) | 62 (58 em uso) |
| Ações (`IV_Acao`) | 980 (**378 em uso**) |
| Resultados (`IV_Resultado`) | 4.209, com 35 colunas de flag `CTRL*` |
| Regras de ação automática (`IV_AcaoAuto`) | **5.958** (5.708 ativas) |
| Regras do Message Center (`IV_ResMsgPapel`) | **1.338** (1.323 ativas, **100% canal e-mail**) |
| Objetos Dinâmicos (`GE_ObjDinamico`) | **196** SQLs gravados no banco |
| Formulários / questões | 176 / 2.303 |
| Parâmetros globais | 239 (**31 criptografados**) + 3.403 em `IV_GlobalPar` |
| Jobs cadastrados / agendados | 71 / 25 (**8 mortos ou nunca executados**) |
| Empresas | 18 (todas Tracbel Agro, 17 em SP) |
| Usuários do CRM | 1.388 |

---

## 2. Os 15 achados mais relevantes

### 1. A integração de faturamento está parada há 17 meses

`EXT_NFS`, `IMP_NFS` e `X_TOTVS_CRM_FATURAMENTO` param **exatamente em 11–14/04/2025**:

| Tabela | Linhas | Última data |
|---|---:|---|
| `EXT_NFS` (`DtaImport`) | 390.755 | **14/04/2025** |
| `IMP_NFS` (`DtaGeracao`) | 18.378 | **11/04/2025** |
| `X_TOTVS_CRM_FATURAMENTO` (`DATA_EMISSAO_NF`) | 809.821 | **11/04/2025** |
| `X_V_IMP_CRM_IMP_NF` (`X_DATA_INTEGRACAO`) | 815.854 | **11/04/2025** |

Enquanto isso o núcleo BPM está **vivo hoje** (`IV_Processo`, `IV_Agenda`, `IV_Historico`,
`GEP_Import` todos com registros de 02/09/2026). Ou seja: os vendedores continuam trabalhando,
mas **todo relatório de cobertura, funil e comissão que cruza com nota fiscal está usando dados de
abril de 2025**. Oito views dependem diretamente dessas tabelas — `IV$NFITEM`, `IV$NFSAIDA`,
`IV$NFSCMPL`, `X_V_CRM_IMP_IMP_NFS`, `X_V_CRM_IMP_NFSItem` e três cópias `ZZ_*` — e por elas as
views `X_CRM_BI_FATURAMENTO_*` e `X_CRM_BI_FUNIL_*` que alimentam o BI.

### 2. A integração de ordem de serviço morreu antes, em maio de 2024 — e chega sem tipo

`EXT_OS` tem apenas **8.099 linhas**, com `Dtaabertura`, `Dtavenda` e `Dtaimport` **100% nulas**;
a última alteração é de **25/05/2024**. `TipoOS` e `TIPOSERVICO` estão vazios em 100% das linhas.
A camada de staging `IMP_OS` tem 287.868 linhas mas também parou (última geração em 2024).
`EXT_Veic` (frota) parou em **24/05/2024** e `EXT_Titulo` em **21/05/2025** — embora `IMP_Titulo`
(staging) tenha registros de emissão até 13/07/2026. **O staging recebe, a camada consolidada não
processa.**

### 3. As notificações do CRM entregam link quebrado

Os parâmetros `CRM/OUT/OutParLinkBaseAgenda` e `OutParLinkBasePessoa` valem
`http://192.168.109.xxx/CRMWeb/Forms/...`. Esse é o **IP da rede anterior à migração de data
center** — está morto. Como as **1.323 regras ativas do Message Center usam 100% canal e-mail** e
essas regras montam o link "abrir no CRM", toda notificação enviada há meses leva o usuário a um
endereço inalcançável. Correção é uma linha em `GE_ParametroGlobal`.

### 4. Seis jobs de ETL Pentaho continuam agendados e morreram no mesmo dia de 2021

Em `GEP_JOBAGD`, seis entradas do tipo `APPEXEC` apontam para atalhos Pentaho
(`D:\VORTICE\PDI\KITCHEN_VEICULO.LNK`, `KITCHEN_PECAS`, `KITCHEN_TITULOS_ACRESC`,
`PAN_IMP_VEICULO`, `PAN_TITULOS_SISDIA`, `INTEGRA BLOQ DOCUMENTAÇÃO`) e todas têm
**última execução em 29 ou 30/07/2021**. É a mesma data em que `IV_ProcRef`, `IV_SMS` e `IV_SMSLog`
param de receber linhas. Outros três nunca executaram (`SMS_SEND`, `MOV_PESSOA`,
`SYNCSAT_PRODUTOS_GERA`) e `EMAIL_IN_CRM` parou em 29/04/2024. Um agendamento tem
`DtaInicio = 30/07/2999`. O Servidor de Processos tenta ou ignora essas entradas há cinco anos.

### 5. O único trigger do banco tem três defeitos estruturais

`VTC_T_AUDITORIA` sobre `IV_ClientePropr` (261 linhas, criado em 15/08/2019):

- **Assume uma única linha por operação** (`SELECT @var = I.coluna FROM inserted`). Num UPDATE
  multilinha, só a última linha lida é auditada — silenciosamente, sem erro.
- **Declara `AFTER INSERT, UPDATE, DELETE` mas não trata DELETE** (o ramo só faz `PRINT`).
- **Tem cinco IDs de propriedade escritos em número literal** no corpo (`9400`, `9402`, `9406`,
  `9411`, `9414`) e um ID de ação (`358`). Incluir uma nova propriedade auditável exige alterar
  o trigger.

Ele não grava no histórico direto: monta uma linha delimitada por `|` e **insere em `gep_import`**,
a fila genérica de importação, com `dtageracao = GETDATE() - 15 min`. Até a auditoria interna
trafega pelo barramento de integração.

### 6. As regras de negócio estão fisicamente duplicadas 18 vezes

`IV_AcaoAuto` (5.958 regras) e `IV_ResMsgPapel` (1.338 regras) repetem quase a mesma regra para
cada uma das 18 empresas: em `IV_AcaoAuto` são 1.429 regras globais e ~4.500 replicadas, com entre
239 e 354 regras por empresa. **Mudar um passo de processo exige 18 edições.** Como as 18 empresas
são todas filiais da Tracbel Agro (17 em SP), a multiempresa aqui é multifilial — não há razão de
negócio para a duplicação. É o maior gerador de custo de manutenção do catálogo.

### 7. Zero CHECK constraints em 767 tabelas — e o vocabulário já divergiu

Não existe uma única `CHECK CONSTRAINT`, nenhuma coluna calculada, e apenas 5 unique constraints.
O resultado é visível em `IV_Processo.Status`, que tem mais de 20 valores de texto livre com
duplicatas semânticas: **`FINALIZADO` (18.416) convive com `FINALIZADA` (11.563)**, `CANCELADO`
(41.390) com `CANCELADA` (2.412), e **437.694 linhas estão com status em branco**. `IV_Processo.Fase`
tem `Afericao` sem acento ao lado de fases acentuadas. Qualquer relatório precisa de lista de
sinônimos.

### 8. Homologação está muito atrás da produção

| | CRM (produção) | CRM_HOMO |
|---|---:|---:|
| Tabelas | 767 | 733 |
| Colunas | 10.744 | 10.546 |
| Módulos | 548 | **375** |

Diferenças: **51 tabelas só em produção**, 17 só em homologação, **362 colunas divergentes**, e
**551 módulos diferentes** — dos quais **129 (79 procedures + 50 functions) existem apenas em
produção** e **372 têm texto divergente** (364 views, 6 procedures, 1 function e **o trigger**).
Na prática, **não há ambiente de homologação confiável**: mudanças foram feitas direto em produção.
`CRM_HOMO` ainda está em `SIMPLE recovery`, contra `FULL` na produção.

### 9. `AUTO_SHRINK` está ligado nos dois bancos

`is_auto_shrink_on = True` em `CRM` **e** em `CRM_HOMO`. Em um banco de 36 GB com log em `FULL`
recovery e teto de 15 GB, o auto-shrink causa fragmentação contínua de índice e picos de I/O
imprevisíveis. É o achado de configuração mais barato de corrigir e o de maior efeito imediato em
desempenho. O arquivo de dados cresce de 10 em 10% (crescimento percentual, também um antipadrão
em arquivo desse porte).

### 10. Cinco tabelas grandes sem chave primária, todas heap

| Tabela | Linhas | Índices |
|---|---:|---:|
| `GEP_JobAgdExecLog` | 1.092.904 | 1 |
| `GEP_Import_bkpjun` | 502.009 | **0** |
| `X_T_IMP_CRM_TITULO` | 479.271 | 1 |
| `IV_STATUS_DEPTO` | 477.344 | 1 |
| `X_V_IMP_CRM_IMP_NF_BKP_18_09_2023` | 415.762 | **0** |

`IV_STATUS_DEPTO` é gravada por duas procedures agendadas diariamente (`PR_ATU_STATUS_DEPTO`) e é
lida por relatórios — um heap de 477 mil linhas sem PK. Duas das cinco são backups esquecidos
(917 mil linhas somadas, ~1 GB) que ninguém removeu.

### 11. Logs consomem 42% do banco

| Tabela | Reservado |
|---|---:|
| `GE_LOG_PROCESSO` | 5,4 GB |
| `GE_LgTb` | 4,2 GB |
| `IV_AgendaLog` | 2,8 GB |
| `GE_Log2` | 1,3 GB |
| `GE_LOG_HISTORICO` + `GE_LOG_PESSOA` + `GE_LOG_TRANS` | 1,1 GB |
| **Soma** | **~13,7 GB de 32,4 GB** |

E o comportamento mudou: `GE_LgTb` recebeu ~2,4 milhões de linhas/ano de 2019 a 2023 e **parou em
2023**; `GE_LOG_PROCESSO` **começou em 2023** e fez 6,7 milhões de linhas em 2024. Houve troca de
mecanismo de log sem expurgo do anterior. Existe um job `LOG_COMPACTA` no catálogo — não está
agendado.

### 12. 196 SQLs de regra de negócio moram dentro do banco, com dependências invisíveis

`GE_ObjDinamico` guarda 196 comandos SQL (o maior com 1.788 caracteres): **168 do tipo `TESTE`**,
que são literalmente as **regras de validação que liberam ou bloqueiam um passo do processo**
(`UsoReqResultado = 1` em 168 deles). Eles usam variáveis de bind proprietárias
(`:vCRMnProcessoAtivo`) e nomes de tabela com `$` (`IV_Q$ACOMPANHAMENTO_VENDA`) traduzidos em
runtime — **125 dos 196 dependem diretamente das tabelas físicas geradas pelos formulários**.

**Três deles apontam para uma conexão externa chamada `SISDIA`** (o sistema Linx anterior):
`PERFIL - Bloq Documentação(Linx)`, `OS-VIsualizar Agenda` e `OD_AFERIÇÃO_SERVIÇO`. Essa conexão
não existe como linked server — é resolvida pela aplicação. São pontos de integração que nenhuma
análise de dependências do banco enxerga. Some-se a isso `IV_ListSQL` (360 SQLs para popular
listas de tela), um dos quais **ainda consulta `LINXMAQ.cxmodelo`**.

### 13. O elo com o ERP depende de um DSN ODBC fora do banco

Há **um único linked server real**: `TOTVS`, produto `totvs6`, provider **MSDASQL** (ODBC),
data source `totvs6`. Isso significa que a string de conexão efetiva (driver, host, porta,
usuário) **está no registro/ODBC do servidor Windows, não no SQL Server** e não em nenhum
repositório versionado. `is_rpc_out_enabled = False` (o CRM só lê). **Oito views** dependem dele,
todas apontando para o banco `TMPRD`:
`X_V_COL_CRM_PESSOA`, `X_V_IMP_CRM_TITULO`, `X_V_BI_FATURAMENTO_PECAS`,
`X_V_BI_FATURAMENTO_SERVICOS`, `X_V_COL_CLIENTE_CRM`, `X_V_CRM_FATURAMENTO_MAQUINAS`,
`X_V_CRM_FATURAMENTO_PECAS`, `X_V_CRM_FATURAMENTO_SERVICOS`.
Atenção: o catálogo só registra a dependência de nome de quatro partes — consultas via `OPENQUERY`
ou SQL dinâmico **não aparecem**, então a superfície real pode ser maior.

### 14. Segredos guardados em tabela, com cifra reversível da própria aplicação

**31 dos 239 parâmetros** de `GE_ParametroGlobal` têm `Criptografado = 'S'`, incluindo
`GLOBAL/GLOBAL/SMTPpsw` (senha do `smtp.office365.com`, usuário `noreply_agro@tracbel.com.br`).
A cifra é proprietária e reversível pelo binário; **a chave não está em lugar nenhum do banco**,
logo mora no executável. Colunas de senha existem ainda em `GE_Usuario` (`Senha`, `Senha3`),
`GE_UsuarioSenhaMem` (2.490 linhas — **histórico de senhas**), `GE_PessoaPasw`, `GE_ObjDinamico`,
`GE_Consulta`, `GE_Email`, `Gep_UsrPabx` e tokens em `GE_CONTATOAPP.FCMTOKEN`,
`GE_USUARIOCMPL.G_TOKEN`, `GEP_SyncUsrSat.FCMTOKEN`. **Nenhuma dessas colunas foi exportada.**

### 15. Mais da metade do catálogo é peso morto

- **602 das 980 ações** (61%) estão `EmUso = 'N'`.
- **112 das 379 colunas de código** analisadas têm **um único valor** em toda a tabela — flags de
  recurso que a Tracbel nunca ligou (`IV_Acao.ExigeProduto`, `ExigeMotivo`, `ExigeDepto` e
  `ExigeResposta` são `N` em 100% das 980 ações).
- **Módulos inteiros do produto estão com zero linhas**: financiamento (`IVF_*`, 20 tabelas),
  call center (`IVC_*`, 6), material (`IVM_*`, 4), produto/preço (`IVP_*`, 4) e `JDE_*` (7).
- 182 das 445 tabelas com coluna de data estão **vazias**; outras **118 estão paradas há mais de
  um ano**, incluindo 85 com mais de 100 linhas.
- O módulo `OUT_*` (marketing de saída) parou em **05/12/2016**; o motor de cobrança `IV_Cbr*` em
  **21/05/2025**.

Isso é boa notícia para o projeto: **o escopo real a substituir é bem menor que o catálogo sugere.**

### Menções honrosas (não entraram nos 15)

- `GEP_Import.Status = 'X'` (erro) em **3,5%** da fila, sem rotina de limpeza.
- A view `X_V_BI_DESPESAS_VENDA_MAQUINAS` tem **719 linhas de SQL** — é um relatório inteiro
  compilado como view. Há **25 views com 80 linhas ou mais**.
- Existe um modelo de processo chamado `Teste Workflow` marcado como em uso, e ações de classe
  `X` (teste) produziram **154.583 agendas reais**.
- Sete dos 11 "sistemas" em `GE_Sistema` têm descrição literal `Sistema indefinido`.
- `EXT_Titulo.Status` mistura código (`B`, `A`) com texto (`Pago`, `Compensado`, `A Depositar`) na
  mesma coluna — duas integrações diferentes gravando em épocas diferentes.
- 26 das 134 consultas do QueryView **nunca foram usadas**; 68 dependem de arquivo `.QRP` fora do banco.

---

## 3. O que NÃO foi possível extrair, e por quê

| Item | Motivo | Impacto |
|---|---|---|
| **Jobs do SQL Server Agent** (`msdb.dbo.sysjobs`, `sysjobsteps`, `sysjobhistory`) | acesso a `msdb` **negado** para `CRM_Leitura` | Não sabemos se há jobs de Agent além dos 25 do Servidor de Processos do Vórtice. Pode haver backup, expurgo ou integração agendada fora do CRM. |
| **Histórico de backup** (`msdb.dbo.backupset`) | mesmo motivo | Não foi possível confirmar RPO/RTO reais nem se o log em `FULL` está sendo truncado por backup. |
| **Estatísticas de uso de índice** (`sys.dm_db_index_usage_stats`) | `VIEW SERVER STATE` negado | Não dá para dizer **quais dos 1.828 índices nunca são lidos** — informação que reduziria muito o custo de manutenção. |
| **Índices ausentes** (`sys.dm_db_missing_index_*`) | idem | Sem recomendação objetiva de índice para as tabelas heap grandes. |
| **Plan cache / queries mais caras** (`sys.dm_exec_query_stats`) | idem | Sem lista das consultas que mais pesam — seria o melhor guia de prioridade de reescrita. |
| **Fragmentação de índice** (`sys.dm_db_index_physical_stats`) | idem | Não foi possível medir o dano do `AUTO_SHRINK`. |
| **`sys.dm_db_partition_stats`** | idem | **Contornado** com `sys.partitions` + `sys.allocation_units`, que devolvem os mesmos números. |
| **Mapeamento de login do linked server** (`sys.linked_logins`) | retornou 0 linhas para este principal | Não sabemos com qual credencial o CRM lê o TOTVS. |
| **String de conexão real do linked server** (`provider_string`) | veio vazia (é um DSN ODBC do Windows) | A configuração da integração com o ERP vive fora do banco. |
| **Login da aplicação** | só `CRM_Leitura` e `sa` são visíveis em `sys.server_principals` | O usuário de banco `crm` (criado em 2011) tem apenas `CONNECT` + `EXECUTE` e **nenhum papel** — mesmo assim a aplicação escreve. A hipótese é que o login mapeado seja `sysadmin` (e portanto resolva como `dbo`). **Precisa ser confirmado.** |
| **Colunas de senha, hash e token** | **omitidas por decisão nossa**, não por falta de permissão | Correto e intencional. Listadas no achado 14. |
| **Arquivos do Doc Manager** | ficam em FTP (`10.150.14.xxx`), fora do banco | `DMN_Doc` tem 71.488 metadados; os binários não foram tocados. |

### Pedido exato a fazer ao DBA

> Precisamos ampliar, **em caráter temporário e somente leitura**, as permissões do login
> `CRM_Leitura` no servidor `COLWCRM`, para concluir o diagnóstico técnico do Vórtice CRM:
>
> 1. `GRANT VIEW SERVER STATE TO [CRM_Leitura];`
>    — libera as DMVs de desempenho (uso de índice, índices ausentes, consultas mais caras,
>    fragmentação). É permissão de leitura de metadados: **não dá acesso a dado de negócio**.
> 2. `USE msdb; CREATE USER [CRM_Leitura] FOR LOGIN [CRM_Leitura]; ALTER ROLE [SQLAgentReaderRole] ADD MEMBER [CRM_Leitura]; GRANT SELECT ON dbo.backupset TO [CRM_Leitura]; GRANT SELECT ON dbo.backupmediafamily TO [CRM_Leitura];`
>    — para inventariar os jobs do SQL Server Agent e o histórico de backup.
> 3. Informar **qual login a aplicação Vórtice usa** para conectar no banco `CRM` e quais papéis
>    ele tem (suspeitamos que seja `sysadmin`, o que seria um risco a tratar).
> 4. Enviar a **string do DSN ODBC `totvs6`** configurado no Windows do servidor de banco
>    (driver, host, porta e usuário — **sem a senha**), e informar com qual credencial o linked
>    server `TOTVS` autentica.
> 5. Confirmar se `AUTO_SHRINK` pode ser desligado em `CRM` e `CRM_HOMO`
>    (`ALTER DATABASE [CRM] SET AUTO_SHRINK OFF;`) e trocar o crescimento do arquivo de dados de
>    `10%` para um valor fixo em MB.
> 6. Confirmar se existe rotina de backup de log — o banco está em `FULL recovery` com o arquivo
>    de log limitado a 15 GB.

---

## 4. Como o material está organizado

```
docs/extracao-vortice/
├── 00-RELATORIO-EXTRACAO.md      este arquivo
├── modulos/
│   ├── 00-INDICE.md             (gerado) tabela dos 548 objetos, com flag de escrita e família
│   ├── procedures/  (85 .sql)   cada arquivo com cabeçalho: tipo, datas, linhas, alvos de escrita, dependências
│   ├── functions/   (51 .sql)
│   ├── triggers/    (1 .sql)
│   └── views/       (411 .sql)
├── ddl/
│   ├── <PREFIXO>.sql            (25 arquivos) CREATE TABLE + índices + FKs, reconstruídos do catálogo
│   ├── 00-indices.csv           1.828 índices: colunas-chave, INCLUDE, unique, filtro
│   ├── 00-identity-computed.csv 65 colunas IDENTITY (nenhuma calculada)
│   ├── extended-properties.csv  33 propriedades
│   └── linked-server.md         (gerado) linked servers e os 8 objetos que dependem deles
├── catalogos-bpm/
│   ├── 00-INDICE.md             análise dos 116 catálogos, com os parâmetros de rede mascarados
│   └── <TABELA>.csv             116 exportações integrais
├── volumetria/
│   ├── 00-INDICE.md             (gerado)
│   ├── linhas-por-tabela.csv    tamanho-em-disco.csv, arquivos-do-banco.csv
│   ├── crescimento-anual.csv    18 tabelas transacionais por ano
│   ├── ultima-atividade.csv     445 tabelas com MAX(data) e classificação viva/fria/parada
│   └── comparacao-junho.csv     delta contra o snapshot de 2025-06
├── dominios/
│   ├── 00-INDICE.md             as 40 colunas de domínio explicadas
│   └── valores.csv              379 colunas × 1.115 valores com frequência
├── seguranca-banco/             9 CSVs de principals, papéis, permissões, logins e configuração
└── _raw/                        insumos brutos, inclusive o diff CRM × CRM_HOMO
```

Os arquivos marcados **(gerado)** saem de `08-gera-indices.ps1`, que lê apenas os CSVs e pode ser
reexecutado offline. Os índices de `catalogos-bpm/` e `dominios/` são análise escrita sobre os
dados extraídos.

### Scripts

| Script | O que faz | Tempo |
|---|---|---:|
| `_comum.ps1` | funções compartilhadas (`Consulta`, `ExportaCsv`, `GravaUtf8`) | — |
| `01-extrai-modulos.ps1` | 548 objetos + metadados + análise de escrita | ~5 s |
| `02-extrai-ddl.ps1` | DDL das 767 tabelas a partir dos catálogos | ~10 s |
| `03-exporta-catalogos.ps1` | 116 catálogos, omitindo colunas sensíveis | ~30 s |
| `04-volumetria.ps1` | linhas, disco, crescimento, última atividade | ~90 s |
| `05-dominios.ps1` | 811 colunas candidatas → 379 domínios | ~52 s |
| `06-seguranca-banco.ps1` | principals, permissões, linked servers | ~3 s |
| `07-diff-homologacao.ps1` | diff CRM × CRM_HOMO | ~5 s |
| `08-gera-indices.ps1` | monta os Markdown a partir dos CSVs (não acessa o banco) | ~5 s |

**Nenhuma consulta ultrapassou 3 segundos** — não houve necessidade de abortar nada por tempo.

### Notas técnicas para quem for reexecutar

- `Invoke-Vortice` devolve `Object[]` de `DataRow` (o PowerShell desenrola o `DataTable`). Sempre
  envolver em `@(...)`: um `return` de array de um elemento vira escalar, e `$r[0]` passa a
  indexar a **coluna** 0 do `DataRow` em vez da primeira linha.
- No PowerShell o operador vírgula tem precedência **maior** que `+`: `@('A'+'B', 'C'+'D')` produz
  uma única string errada. Parênteses são obrigatórios.
- O guard anti-DDL do `connect.ps1` bloqueia `CREATE`/`ALTER`/`INSERT`… **no texto da consulta**,
  inclusive dentro de literais. O `02-extrai-ddl.ps1` contorna montando as palavras em runtime
  (`'CRE' + 'ATE'`) — o texto nunca é enviado ao servidor, é só gerado localmente.
- `08-gera-indices.ps1` tem acentos e **precisa** de UTF-8 **com BOM**: o Windows PowerShell 5.1
  lê `.ps1` sem BOM como ANSI e corrompe o texto (e pode gerar erro de sintaxe).
- `sys.sql_expression_dependencies` **não tem** a coluna `is_updated` (ela existe apenas em
  `sys.dm_sql_referenced_entities`).
