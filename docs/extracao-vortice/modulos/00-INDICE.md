# Índice dos 548 objetos programáveis do banco CRM (Vórtice CRM / Tracbel)

Extraído em 2026-09-02 23:27 do banco de **produção** `CRM` (SQL Server 2019, nível de compatibilidade 100), somente leitura.

## Sumário

| Tipo | Qtd | Escrevem em tabela | Pasta |
|---|---:|---:|---|
| Stored procedures | 85 | 70 | `modulos/procedures/` |
| Functions escalares | 51 | 0 | `modulos/functions/` |
| Triggers | 1 | 1 | `modulos/triggers/` |
| Views | 411 | 0 | `modulos/views/` |
| **Total** | **548** | **71** | |

Arestas de dependência registradas em `sys.sql_expression_dependencies`: **1733**.

### Como ler a coluna "Escreve"

A marcação foi calculada **sobre o texto já baixado**, procurando os verbos de escrita
(INSERT / UPDATE / DELETE / MERGE / TRUNCATE) fora do servidor — nenhuma consulta de escrita
foi enviada ao banco. A coluna "Alvos" lista as tabelas que aparecem imediatamente depois desses
verbos e que existem de fato no banco. É o mapa de **onde a lógica de negócio mora dentro do banco**,
em vez de morar na aplicação.

Resultado central: **nenhuma das 51 functions e nenhuma das 411 views escreve**. Toda a escrita em tabela vinda de código T-SQL está concentrada em **70 procedures e 1 trigger**.

## O único trigger do banco: `VTC_T_AUDITORIA`

- **Tabela**: `IV_ClientePropr`  |  **Evento**: AFTER INSERT, UPDATE, DELETE  |  **261 linhas**
- **Criado em** 2019-08-15, **alterado em** 2019-08-15 — a autoria está declarada em comentário no próprio código (André Rizzatti, 15/08/2019)

**O que ele faz, em prosa.** É um gatilho de *auditoria de propriedades do cliente*. Quando um
registro de `IV_ClientePropr` (as "propriedades" de um cliente: origem da receita, tratores,
implementos, colhedoras, NJUR) é inserido ou alterado, o trigger:

1. Lê a linha afetada em `inserted` para variáveis escalares;
2. Descobre o **assistente** do vendedor que fez a alteração consultando `IV_GlobalPar` com
   `SeqGlbPar = 12` ("Auditoria"), casando `Campo2` com o usuário da alteração e trazendo a
   empresa por `GE_EMPRESA.NomeReduzido`;
3. Verifica se **já existe agenda de auditoria em aberto** para aquela pessoa — `IV_Agenda` com
   `Acao = 358` ("Validação da Propriedade") e `Realizada = 'N'`, ligada por
   `IV_ProcLink.LinkDocto = 'AUDIT'`;
4. Se houver assistente, não houver agenda aberta e a propriedade for uma das cinco codificadas
   **em números literais no corpo do trigger** (`9400` Origem da Receita, `9402` Trator,
   `9406` Implemento, `9411` Colhedora, `9414` NJUR), monta uma linha delimitada por `|` usando
   `dbo.f_s_dado` / `dbo.f_s_dado_N` e **grava em `gep_import`** pedindo a inclusão de um registro
   em `IV_HISTORICO`, com `dtageracao = GETDATE() - 15 minutos` para que o Servidor de Processos
   pegue o registro já no ciclo seguinte.

**Três observações que importam para a substituição:**

- Ele **não trata DELETE**: o ramo de exclusão só executa `PRINT 'DELETANDO'`, embora esteja
  declarado `AFTER INSERT, UPDATE, DELETE`.
- Ele **assume uma única linha por operação** (`SELECT @var = I.coluna FROM inserted`). Num UPDATE
  que atinja várias linhas, apenas a última linha lida é auditada — silenciosamente.
- Ele **não escreve direto no histórico**: enfileira em `gep_import`, a mesma fila genérica usada
  pelas integrações. Ou seja, até a auditoria interna do CRM trafega pelo barramento de importação.

## Famílias funcionais

| Família | Objetos | O que caracteriza |
|---|---:|---|
| — | 396 | sem classificação automática (em sua maioria views de consulta) |
| Integração | 75 | prefixos IMP_ / X_ / PR_VTC_INT / PR_INT, ou grava em GEP_Import, EXT_* e X_TOTVS_* |
| BPM | 57 | toca IV_Agenda / IV_Historico / IV_Processo / IV_AcaoAuto / IV_ProcDado — o motor de workflow |
| Relatório | 56 | prefixos BI_ / VW_REL / TBA / X_CRM_BI — alimentam QlikView, relatórios QRP e planilhas |
| Segmentação | 34 | toca as tabelas IVS_ (departamento, carteira, cobertura de clientes) |
| SSMS (nativo) | 8 | objetos de diagrama do próprio SQL Server Management Studio; não fazem parte do produto |
| Teste/backup | 8 | nome contendo TESTE ou BKP — resíduo que deveria ter sido removido |

## Stored procedures (85), da mais recente para a mais antiga

| # | Procedure | Modificada | Linhas | Escreve | Alvos de escrita | Família |
|---:|---|---|---:|:-:|---|---|
| 1 | `sp_atualiza_fones_ge_pessoa` | 2026-04-20 | 210 | SIM | `GE_PESSOAFONE`, `GE_PESSOA` | — |
| 2 | `vrtc_p_logtableset` | 2026-04-20 | 184 | SIM | `ge_log_historico`, `iv_agendalog` | BPM |
| 3 | `VTC_Gera_REL_TBA101` | 2026-03-20 | 314 | SIM | `IMP_REL_TBA101` | Integração, Relatório |
| 4 | `VTC_P_GERACONDPAGTO` | 2026-03-12 | 264 | SIM | `GEP_IMPORT`, `GEP_IMPORTAPROVACAO`, `EXT_CONDPAGTO` | BPM, Integração |
| 5 | `VTC_P_GERAAGUARDENTREGA` | 2026-03-11 | 258 | SIM | `GEP_IMPORT`, `GEP_IMPORTAPROVACAO`, `EXT_AGUARDENTREGA` | BPM, Integração |
| 6 | `VTC_P_GERAAPROVACAO` | 2026-03-11 | 252 | SIM | `GEP_IMPORT`, `GEP_IMPORTAPROVACAO`, `EXT_APROVACAO` | BPM, Integração |
| 7 | `PR_VTC_INTFAMILIA` | 2025-10-31 | 108 | SIM | `GE_SEQUENCIA`, `IV_GLOBALPAR`, `EXT_VEICFAMREF`, `ext_veicfam`, `EXT_VEICFAM` | Integração |
| 8 | `PR_VTC_INTVEICULO` | 2025-10-31 | 320 | SIM | `imp_veiculo_integrado`, `imp_veiculo`, `IMP_VEICULO` | Integração |
| 9 | `VTC_P_Historico` | 2025-10-31 | 119 | SIM | `gep_import` | Integração |
| 10 | `PR_VTC_INTPROPRIEDADE` | 2025-10-31 | 106 | SIM | `IV_CLIENTEPROPR` | Integração |
| 11 | `PR_VTC_INT_VEIC` | 2025-10-31 | 104 | SIM | `EXT_VEIC`, `ext_veic`, `EXT_VEICPROP`, `ext_veicprop`, `imp_veiculo` | Integração |
| 12 | `PR_VTC_INTMODELO` | 2025-10-31 | 31 | SIM | `EXT_VEICMODELO` | Integração |
| 13 | `PR_VTC_INTMARCA` | 2025-10-31 | 29 | SIM | `EXT_VEICMARCA` | Integração |
| 14 | `vrtc_p_atualizar_data_ultimo_contatoExterno` | 2025-08-07 | 83 | SIM | `GE_PARAMETROGLOBAL` | BPM, Segmentação |
| 15 | `vrtc_p_atualizar_data_ultimo_contato` | 2025-08-07 | 89 | SIM | `GE_PARAMETROGLOBAL` | BPM, Segmentação |
| 16 | `PRC_GET_SEQUENCIA_TABELA` | 2025-06-27 | 47 | SIM | `GE_SEQUENCIA` | — |
| 17 | `fva_SequenciaAuto` | 2025-02-17 | 1 | SIM | `GE_SEQUENCIA` | — |
| 18 | `fva_SequenciaGet` | 2025-02-17 | 1 | SIM | `GE_SEQUENCIA` | — |
| 19 | `PR_COL_ATUCONGLO` | 2024-12-30 | 139 | SIM | `ge_pessoa` | — |
| 20 | `PR_COL_MIGPESSOA_PROSP` | 2024-12-30 | 412 | — | — | — |
| 21 | `PR_COL_MIGPESSOA_ATV` | 2024-12-30 | 414 | — | — | — |
| 22 | `PR_COL_MIGRACADASTRO` | 2024-12-30 | 84 | SIM | `mig_ge_pessoa` | — |
| 23 | `PR_COL_ACERTAFONEFINAL` | 2024-12-30 | 89 | SIM | `ge_sequencia`, `GE_PessoaFone` | — |
| 24 | `PR_COL_ATU_LINKS` | 2024-12-29 | 35 | SIM | `ge_pessoalink` | — |
| 25 | `PR_COL_MIGPROPR` | 2024-12-28 | 302 | — | — | — |
| 26 | `PR_COL_MIGPROPR_EXIST` | 2024-12-28 | 303 | — | — | — |
| 27 | `PR_COL_MOVPROPR` | 2024-12-28 | 429 | SIM | `ge_sequencia`, `GEP_IMPORT` | Integração |
| 28 | `PR_COL_ACERTAFONE` | 2024-12-28 | 79 | SIM | `ge_pessoafone`, `ge_pessoa` | — |
| 29 | `PR_COL_ATUPALAVRACHAVE` | 2024-12-28 | 71 | SIM | `ge_pessoa` | — |
| 30 | `PR_COL_ATULATLONG` | 2024-12-28 | 56 | SIM | `ge_pessoa` | — |
| 31 | `PR_COL_MIGCONTCLASSE` | 2024-12-28 | 144 | SIM | `ge_contatopapel` | — |
| 32 | `PR_COL_ATUDECISAO` | 2024-12-28 | 57 | SIM | `GE_CONTATO` | — |
| 33 | `PR_COL_MIGCONT` | 2024-12-28 | 559 | SIM | `GE_Contato` | — |
| 34 | `PR_COL_MIGPESEND` | 2024-12-28 | 174 | — | — | — |
| 35 | `PR_COL_MIGPESEND_EXIST` | 2024-12-28 | 178 | — | — | — |
| 36 | `PR_COL_MOVEND` | 2024-12-28 | 273 | SIM | `ge_cidade`, `GEP_IMPORT` | Integração |
| 37 | `PR_COL_MIGPESSOA_EXIST` | 2024-12-27 | 514 | — | — | — |
| 38 | `PR_COL_MOVPESSOA` | 2024-12-27 | 578 | SIM | `ge_cidade`, `GEP_IMPORT` | Integração |
| 39 | `PR_COL_ATUPROPRIEDADE` | 2024-12-27 | 114 | SIM | `IV_Propriedade`, `GE_UsuarioPerm` | — |
| 40 | `X_TOTVS_ATUA_CRM_FATURAMENTO` | 2024-05-27 | 757 | SIM | `X_TOTVS_CRM_FATURAMENTO` | Integração |
| 41 | `X_TOTVS_ATUALIZA_BI_FATURAMENTO_MAQUINAS` | 2023-11-29 | 120 | SIM | `X_TOTVS_BI_FATURAMENTO_MAQUINAS` | Integração, Relatório |
| 42 | `X_P_REL_001` | 2023-11-22 | 392 | — | — | Integração, Relatório |
| 43 | `X_P_EXE_INTEGRACOES_TOTVS_CRM` | 2023-10-13 | 16 | — | — | Integração |
| 44 | `X_P_IMP_CRM_NF_POS_VENDA` | 2023-10-11 | 302 | SIM | `IMP_NFSItem`, `IMP_NFS` | Integração |
| 45 | `X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS_N` | 2023-10-10 | 187 | — | — | Integração, Relatório |
| 46 | `X_TOTVS_ATUA_BI_FATURAMENTO_POS_VENDAS` | 2023-09-29 | 286 | SIM | `X_TOTVS_BI_FATURAMENTO_POS_VENDAS` | Integração, Relatório |
| 47 | `X_P_IMP_CRM_TITULO` | 2023-03-21 | 135 | SIM | `X_T_IMP_CRM_TITULO` | Integração |
| 48 | `PR_ATU_PROCESSO` | 2023-03-15 | 77 | SIM | — | BPM |
| 49 | `X_BI_PROSPECCAO_MAQUINAS_BASE_FATURAMENTO` | 2023-03-10 | 132 | — | — | BPM, Integração, Segmentação, Relatório |
| 50 | `X_BI_PROSPECCAO_MAQUINAS_FATURAMENTO` | 2023-03-10 | 150 | — | — | BPM, Integração, Segmentação, Relatório |
| 51 | `X_BI_PROSPECCAO_MAQUINAS_PEDIDO` | 2023-03-10 | 146 | — | — | BPM, Integração, Segmentação, Relatório |
| 52 | `PR_ATU_STATUS_DEPTO_PAR` | 2023-02-08 | 205 | SIM | `IV_STATUS_DEPTO`, `gep_import` | Integração, Segmentação |
| 53 | `PR_ATU_STATUS_DEPTO` | 2023-01-24 | 252 | SIM | `IV_STATUS_DEPTO`, `gep_import` | BPM, Integração |
| 54 | `PR_ATU_FORMPRODTOTVS` | 2022-08-12 | 63 | SIM | `IV_GLOBALPAR`, `iv_globalpar` | Integração |
| 55 | `PR_INT_PROPRNFS` | 2022-03-23 | 148 | SIM | `GE_Sequencia`, `gep_import` | Integração |
| 56 | `PR_ATULINK_PESSOA` | 2021-08-02 | 37 | SIM | `IMP_LINKPESSOA`, `ge_pessoalink` | Integração |
| 57 | `BI_RELATORIO_PROSPECCAO` | 2021-01-18 | 304 | SIM | — | BPM, Integração, Segmentação, Relatório |
| 58 | `PR_ATUALIZA_TITULO_INATIVO` | 2019-10-30 | 13 | SIM | `EXT_Titulo` | Integração |
| 59 | `PR_IV_COBERTURA` | 2019-04-03 | 69 | SIM | `IVS_PES` | BPM, Segmentação |
| 60 | `BI_PROCESSO_333` | 2019-01-18 | 209 | SIM | — | BPM, Relatório |
| 61 | `PR_INT_FORM_ACOMPVENDAVD` | 2018-04-16 | 242 | SIM | `gep_import` | BPM, Integração |
| 62 | `PR_PROPRIEDADE` | 2018-03-20 | 414 | SIM | `gep_import` | Integração |
| 63 | `VTC_P_INSERE_PROPR_CART` | 2018-02-06 | 118 | SIM | `IV_ClientePropr`, `GE_Sequencia` | — |
| 64 | `VTC_P_INSERE_FONE_CART` | 2018-02-06 | 316 | SIM | `GE_PessoaFone`, `GE_Sequencia` | — |
| 65 | `VTC_P_INSERE_EMAIL_CART` | 2018-02-05 | 155 | SIM | `GE_Email`, `GE_Sequencia` | — |
| 66 | `PR_ACERTA_CARTEIRA` | 2017-12-08 | 83 | SIM | `IVS_Pes` | Segmentação |
| 67 | `PR_MIG_ACERTA_IVSCARTCID` | 2017-01-22 | 46 | SIM | `IVS_CARTCID` | Segmentação |
| 68 | `PR_MIG_ACERTA_CIDADE` | 2017-01-22 | 50 | SIM | `GE_PESSOA` | — |
| 69 | `PR_ACERTA_CIDADE` | 2016-12-14 | 52 | SIM | `GE_PESSOA` | — |
| 70 | `PR_INT_NFS` | 2016-12-08 | 205 | SIM | `ext_nfs`, `ext_nfsitem` | Integração |
| 71 | `PR_INTSPR_NFS` | 2016-08-23 | 194 | SIM | `ext_nfs`, `ext_nfsitem` | Integração |
| 72 | `sp_renamediagram` | 2016-03-22 | 62 | SIM | `sysdiagrams` | SSMS (nativo) |
| 73 | `PR_INT_OSAB` | 2016-03-22 | 325 | SIM | `ext_os`, `ext_OSitem` | Integração |
| 74 | `sp_alterdiagram` | 2016-03-22 | 65 | SIM | `sysdiagrams` | SSMS (nativo) |
| 75 | `sp_creatediagram` | 2016-03-22 | 58 | SIM | `sysdiagrams` | SSMS (nativo) |
| 76 | `sp_dropdiagram` | 2016-03-22 | 41 | SIM | `sysdiagrams` | SSMS (nativo) |
| 77 | `PR_ATU_STATUS` | 2016-03-22 | 68 | SIM | `gep_import` | BPM, Integração |
| 78 | `PR_INTSPR_OS` | 2016-03-22 | 263 | SIM | `ext_os`, `ext_OSitem` | Integração |
| 79 | `PR_INC_OUT_PESSOA` | 2016-03-22 | 156 | SIM | `OUT_PESSOA` | — |
| 80 | `PR_INT_FORM_ACOMPVENDA` | 2016-03-22 | 206 | SIM | `gep_import` | BPM, Integração |
| 81 | `sp_helpdiagramdefinition` | 2016-03-22 | 40 | — | — | SSMS (nativo) |
| 82 | `sp_upgraddiagrams` | 2016-03-22 | 60 | SIM | `sysdiagrams` | SSMS (nativo) |
| 83 | `PR_INT_OS` | 2016-03-22 | 263 | SIM | `ext_os`, `ext_OSitem` | Integração |
| 84 | `sp_helpdiagrams` | 2016-03-22 | 31 | — | — | SSMS (nativo) |
| 85 | `TESTE_proc` | 2016-03-22 | 10 | SIM | `TESTE_acesso` | Teste/backup |

## Functions escalares (51)

Nenhuma escreve. São utilitários de formatação (`fva_*`, `fn_*`), extração de dados do BPM
(`fIV_*`, `fva_Get*`) e serialização para a fila de importação (`f_s_Dado`, `f_s_Dado_N`,
`f_s_Campo`) — estas últimas são a "cola" usada pelo trigger e por várias procedures para montar
as linhas de largura fixa que alimentam `gep_import`.

| Function | Linhas | Modificada | Tabelas referenciadas |
|---|---:|---|---|
| `f_GetColumnData` | 46 | 2022-03-06 | — |
| `f_IntervaloNum` | 1 | 2022-03-06 | — |
| `f_IntervaloNumX` | 15 | 2022-03-06 | — |
| `f_IV_UltimoResHistProcesso` | 1 | 2025-02-17 | IV_HISTORICO; IV_RESULTADO |
| `f_s_Campo` | 19 | 2022-03-06 | — |
| `f_s_Dado` | 20 | 2022-03-06 | — |
| `f_s_Dado_N` | 15 | 2022-03-06 | — |
| `fC5_ColDate` | 14 | 2024-12-26 | — |
| `fC5_ColNumber` | 17 | 2024-12-26 | — |
| `fC5_ColString` | 17 | 2024-12-26 | — |
| `fcnDiasUteis` | 40 | 2022-03-06 | — |
| `fiv_GetGrupoUsr` | 1 | 2025-02-17 | GE_MEMBRO; GE_USUARIO |
| `fIV_GetPessoaFones` | 1 | 2022-03-06 | GE_PESSOA |
| `fiv_GetProdutoEstrutura` | 1 | 2025-02-17 | IV_ESTRPRODUTO |
| `fIV_GetProdutoProcesso` | 31 | 2022-03-06 | IV_PROCPRODUTO |
| `fIV_GetResultadoAcao` | 1 | 2025-02-17 | IV_RESULTADO |
| `fIV_GetStatusNaData` | 1 | 2022-03-06 | IV_PROCSTATMONIT |
| `fIV_GetStatusOrdNaData` | 1 | 2022-03-06 | IV_PROCDADO; IV_PROCST; IV_PROCSTATMONIT |
| `fn_ContaHoras` | 39 | 2022-03-06 | — |
| `fn_diagramobjects` | 47 | 2022-03-06 | — |
| `fn_RightCase` | 20 | 2022-03-06 | — |
| `fn_RightCaseProprio` | 25 | 2022-03-06 | — |
| `fn_StripCharacters` | 16 | 2022-03-06 | — |
| `fva_CGCCPFFormat` | 1 | 2025-02-17 | — |
| `fva_DateTimeFromParts` | 1 | 2025-02-17 | — |
| `fva_DateTrunc` | 1 | 2025-02-17 | — |
| `fva_Dinheiro` | 1 | 2022-03-06 | — |
| `fva_FmtNum` | 69 | 2026-05-07 | — |
| `fva_FormatMoney` | 1 | 2025-02-17 | — |
| `fva_GetData` | 6 | 2023-03-24 | — |
| `fva_GetExisteProcessolink` | 1 | 2025-02-17 | IV_DOCTOTIPO; IV_ProcDocto; IV_PROCLINK; IV_Questionario |
| `fva_GetHistoricoTag` | 1 | 2025-02-17 | IV_HISTORICOTAG |
| `fva_GetHora` | 9 | 2023-03-24 | — |
| `fva_GetPessoaEndereco` | 1 | 2025-02-17 | GE_PESSOA; GE_PESSOAEND |
| `fva_GetPessoaFones` | 1 | 2025-02-17 | GE_PESSOA |
| `fva_GetProcessoTag` | 1 | 2025-02-17 | IV_PROCTAG |
| `fva_GetProdutoProcesso` | 1 | 2025-02-17 | IV_PROCPRODUTO |
| `fva_InitCap` | 1 | 2025-02-17 | — |
| `fva_IntervaloDataB` | 1 | 2022-03-06 | — |
| `fva_IntervaloNum` | 1 | 2025-02-17 | — |
| `fva_IntervaloNumX` | 1 | 2025-02-17 | — |
| `fva_QryDiaUtil` | 1 | 2025-02-17 | — |
| `fva_RemoveCRLF` | 1 | 2025-02-17 | — |
| `fva_SoNro` | 1 | 2025-02-17 | — |
| `fva_StrMaskLGPD` | 1 | 2025-02-17 | — |
| `fva_TruncDate` | 6 | 2022-03-06 | — |
| `fva_UnFormatCGCCPF` | 12 | 2022-03-06 | — |
| `MINUTOS` | 1 | 2022-03-06 | — |
| `toChar` | 1 | 2025-02-17 | — |
| `Trunc_Date` | 6 | 2022-03-06 | — |
| `vrtc_f_aliastablelogget` | 71 | 2026-04-20 | — |

## Views (411)

Nenhuma view escreve, mas várias concentram lógica de negócio pesada. As **25 views com 80 linhas ou mais** são, na prática, relatórios compilados dentro do banco — e o
principal risco de reescrita silenciosa quando o CRM for substituído.

### Views com 80 linhas ou mais (lógica de negócio embutida)

| View | Linhas | Família | Tabelas referenciadas |
|---|---:|---|---|
| `X_V_BI_DESPESAS_VENDA_MAQUINAS` | 719 | Integração, Relatório | GE_Pessoa; IV_Formulario; IV_Q_ACOMP_VEND_MAQUINAS; IV_Q_CONT_COMISSAO_22; IV_Q_GESTAO_CREDITO; IV_Q_GESTAO_CREDITO_AMS; IV_Q_GESTAO_CREDITO_IMP; IV_Q_GESTAO_PRODUTO___AMS; IV_Q_GESTAO_PRODUTO_IMPL; IV_Questionario; X_TOTVS_BI_FATURAMENTO_MAQUINAS |
| `BI_OPORT_ABERTASRESUMO` | 442 | Relatório | — |
| `X_CRM_BI_FUNIL_PECA_CARTEIRA` | 375 | Integração, Segmentação, Relatório | GE_Empresa; GE_Pessoa; GE_Usuario; IVS_CARTEIRA; IVS_Depto; IVS_PES |
| `BI_VENDA_MAQ` | 268 | BPM, Relatório | GE_LgTb; IV_AGENDA; IV_HISTORICO; IV_PROCDADO; IV_PROCESSO; IV_RESULTADO |
| `BI_OPORT_ABERTAS` | 251 | BPM, Relatório | ge_pessoa; iv_acao; iv_agenda; iv_historico; iv_procdado; iv_resultado |
| `BI_VENDA_MAQ_ENC` | 235 | BPM, Relatório | GE_LgTb; IV_PROCDADO; IV_PROCESSO; IV_PROCSTATMONIT |
| `BI_COBERTURA_PROSPECCAO` | 224 | BPM, Segmentação, Relatório | GE_Pessoa; IV_Acao; IV_CLASSERES; IV_HISTORICO; IV_PROCDADO; IV_RESCLASSE; IV_RESULTADO; IVS_Depto; IVS_PES |
| `BI_COBERTURA_PROSPECCAO_MAQUINAS` | 220 | BPM, Segmentação, Relatório | GE_Pessoa; IV_Acao; IV_CLASSERES; IV_HISTORICO; IV_PROCDADO; IV_RESCLASSE; IV_RESULTADO; IVS_Depto; IVS_PES |
| `X_CRM_BI_FUNIL_PECA_CLASSE` | 194 | BPM, Integração, Segmentação, Relatório | IV_Acao; IV_CLASSERES; IV_HISTORICO; IV_PROCDADO; IV_RESCLASSE; IV_RESULTADO; IVS_PES |
| `BI_AGENDA_PROSPECCAO` | 160 | BPM, Relatório | GE_Empresa; GE_Pessoa; Ge_Usuario; IV_Acao; IV_Agenda |
| `VW_REL_TBA101` | 157 | BPM, Integração, Relatório | GE_EMPRESA; GE_PESSOA; GE_USUARIO; IMP_REL_TBA101; IV_ACAO; IV_HISTORICO; IV_PROCDADO; IV_PROCESSO; IV_RESULTADO |
| `V_UTIL_TELEFONES_CONTATOS` | 156 | — | GE_Contato; GE_Pessoa |
| `teste_felipe` | 141 | BPM, Teste/backup | IV_CLASSERES; IV_GLOBALPAR; IV_GLOBALPARCTRL; IV_HISTORICO; IV_PROCDADO; IV_PROCESSO; IV_PROCPRODUTO; IV_RESCLASSE; IV_RESULTADO |
| `X_CRM_BI_FATURAMENTO_PECA_CARTEIRA` | 140 | Integração, Segmentação, Relatório | GE_Empresa; GE_Pessoa; GE_Usuario; IVS_CARTEIRA; IVS_Depto; IVS_PES |
| `BI_PESSOAOP` | 136 | Segmentação, Relatório | GE_PESSOA; IV_CLIENTEPROPR; IVS_CARTEIRA; IVS_DEPTO; IVS_PES |
| `BI_OPORT_FUNIL2` | 132 | BPM, Relatório | IV_HISTORICO; IV_PROCDADO; IV_PROCESSO; IV_PROCPERSP; IV_PROCSTATMONIT |
| `BI_PESSOA` | 132 | Relatório | GE_PESSOA; IV_CLIENTEPROPR |
| `X_V_CRM_IMP_IMP_NFS` | 121 | Integração | X_TOTVS_CRM_FATURAMENTO |
| `BI_OPORTUNIDADE_ENC` | 101 | BPM, Relatório | GE_PESSOA; IV_ACAO; IV_AGENDA; IV_HISTORICO; IV_PROCDADO; IV_PROCESSO; IV_PROCPERSP; IV_ProcStatMonit; IV_RESULTADO |
| `X_V_CRM_IMP_NFSItem` | 99 | Integração | X_TOTVS_CRM_FATURAMENTO |
| `BI_CARTEIRA_PUK` | 88 | BPM, Segmentação, Relatório | GE_EMPRESA; GE_PESSOA; GE_USUARIO; IV_HISTORICO; IVS_CARTEIRA; IVS_DEPTO; IVS_PES |
| `IV$DADOS_PESSOA_COMPLETA` | 87 | Segmentação | GE_PESSOA; GE_PESSOALINK; GE_REGIAO; GE_ROTA; IVS_CARTEIRA; IVS_Depto; IVS_PES |
| `IV_Q$ADM_FINANCEIRO_N` | 86 | — | GE_PESSOA; IV_FORMULARIO; IV_Q_ADM_FINANCEIRO; IV_QUESTIONARIO |
| `BI_OPORT_FUNIL2_BKP` | 82 | BPM, Relatório, Teste/backup | IV_PROCDADO; IV_PROCESSO; IV_PROCPERSP; IV_PROCSTATMONIT |
| `BI_CARTEIRA_VN` | 80 | BPM, Segmentação, Relatório | GE_EMPRESA; GE_PESSOA; GE_USUARIO; IV_HISTORICO; IVS_CARTEIRA; IVS_DEPTO; IVS_PES |

### Todas as views

| View | Linhas | Modificada | Família |
|---|---:|---|---|
| `BI_AGENDA` | 50 | 2023-03-15 | BPM, Relatório |
| `BI_AGENDA_CEN` | 27 | 2016-03-22 | BPM, Segmentação, Relatório |
| `BI_AGENDA_PROSPECCAO` | 160 | 2023-09-05 | BPM, Relatório |
| `BI_CARTEIRA_EQUIP` | 51 | 2018-03-21 | Segmentação, Relatório |
| `BI_CARTEIRA_MANITOU` | 48 | 2018-03-23 | Segmentação, Relatório |
| `BI_CARTEIRA_PUK` | 88 | 2026-03-03 | BPM, Segmentação, Relatório |
| `BI_CARTEIRA_USADO` | 48 | 2018-03-23 | Segmentação, Relatório |
| `BI_CARTEIRA_VN` | 80 | 2026-02-06 | BPM, Segmentação, Relatório |
| `bi_cobertura` | 33 | 2019-10-28 | BPM, Segmentação, Relatório |
| `BI_COBERTURA_PROSPECCAO` | 224 | 2023-12-05 | BPM, Segmentação, Relatório |
| `BI_COBERTURA_PROSPECCAO_MAQUINAS` | 220 | 2024-01-04 | BPM, Segmentação, Relatório |
| `BI_DEPTO` | 1 | 2016-03-22 | Segmentação, Relatório |
| `BI_DTAPROVCREDITO` | 38 | 2023-10-16 | BPM, Relatório |
| `BI_EMPRESA` | 1 | 2016-03-22 | Relatório |
| `bi_faturamentos` | 26 | 2019-10-28 | BPM, Segmentação, Relatório |
| `BI_FROTA` | 28 | 2023-10-05 | Relatório |
| `BI_GRUPO_USR` | 1 | 2016-03-22 | Relatório |
| `BI_OPORT_ABERTAS` | 251 | 2023-07-18 | BPM, Relatório |
| `BI_OPORT_ABERTASRESUMO` | 442 | 2023-07-18 | Relatório |
| `BI_OPORT_FUNIL` | 1 | 2016-03-22 | BPM, Relatório |
| `BI_OPORT_FUNIL2` | 132 | 2016-10-10 | BPM, Relatório |
| `BI_OPORT_FUNIL2_BKP` | 82 | 2016-03-22 | BPM, Relatório, Teste/backup |
| `bi_oportunidade` | 51 | 2023-03-17 | BPM, Relatório |
| `BI_OPORTUNIDADE_ENC` | 101 | 2023-11-13 | BPM, Relatório |
| `bi_oportunidade_produto` | 45 | 2023-02-22 | BPM, Relatório |
| `bi_Oportunidades` | 35 | 2022-09-15 | BPM, Segmentação, Relatório |
| `BI_ORIGEM_RECEITA` | 27 | 2025-07-31 | Relatório |
| `bi_pedidos` | 31 | 2019-10-28 | BPM, Segmentação, Relatório |
| `BI_PESSOA` | 132 | 2018-12-05 | Relatório |
| `BI_PESSOAOP` | 136 | 2023-12-21 | Segmentação, Relatório |
| `BI_QUALIDADE` | 1 | 2016-03-22 | BPM, Segmentação, Relatório |
| `BI_USUARIO` | 1 | 2016-03-22 | Relatório |
| `BI_VENDA_MAQ` | 268 | 2023-03-01 | BPM, Relatório |
| `BI_VENDA_MAQ_ENC` | 235 | 2023-11-13 | BPM, Relatório |
| `BI_VENDA_MAQ_NEW` | 36 | 2022-10-07 | BPM, Relatório |
| `BI_VENDA_PERDIDA` | 35 | 2016-12-14 | BPM, Relatório |
| `BI_VENDA_PERDIDA_PROD` | 68 | 2023-04-19 | BPM, Relatório |
| `BI_VISITA` | 72 | 2016-03-22 | BPM, Segmentação, Relatório |
| `GE$CONTATO_FULL` | 1 | 2025-02-17 | — |
| `GE$CONTATO_LGPD` | 1 | 2025-02-17 | — |
| `GE$EMAIL_FULL` | 1 | 2025-02-17 | — |
| `GE$EMAIL_LGPD` | 1 | 2025-02-17 | — |
| `GE$MODULOPERM` | 1 | 2025-02-17 | — |
| `GE$OBJDINAMICO` | 1 | 2025-02-17 | — |
| `GE$PESSOA` | 15 | 2019-10-25 | — |
| `GE$PESSOA_FULL` | 1 | 2025-02-17 | — |
| `GE$PESSOA_LGPD` | 1 | 2025-02-17 | — |
| `GE$PESSOAEND_FULL` | 1 | 2025-02-17 | — |
| `GE$PESSOAEND_LGPD` | 1 | 2025-02-17 | — |
| `GE$PESSOAENDCORR` | 39 | 2016-03-22 | — |
| `GE$PESSOAFONE_FULL` | 1 | 2025-02-17 | — |
| `GE$PESSOAFONE_LGPD` | 1 | 2025-02-17 | — |
| `GE$POLSEGPERM` | 1 | 2025-02-17 | — |
| `GE$USUARIOPERM` | 1 | 2025-02-17 | — |
| `GEL$CIDADE` | 1 | 2025-02-17 | — |
| `GEL$LOGRADOURO` | 1 | 2025-02-17 | — |
| `GEL$TIPOLOGRADOURO` | 1 | 2025-02-17 | — |
| `GEL$UF` | 1 | 2025-02-17 | — |
| `IMPV_VEICULO` | 1 | 2017-01-16 | Integração |
| `IV$_PONTUACAO` | 1 | 2016-03-22 | — |
| `IV$A_BENEFICIO` | 1 | 2020-09-07 | — |
| `IV$A_CAMPANHA_AGRISHOW_2012` | 1 | 2016-03-22 | — |
| `IV$A_COR_PREFERIDO` | 1 | 2016-03-22 | — |
| `IV$A_PERFIL_CLIENTE_SERVICOS` | 1 | 2016-03-22 | — |
| `IV$A_PERSONA_JOHN_DEERE` | 1 | 2022-02-15 | — |
| `IV$A_POTENCIAL_CARTEIRA_PECAS` | 1 | 2020-07-29 | Segmentação |
| `IV$A_PROJETO_CULTIVAR` | 1 | 2020-09-07 | — |
| `IV$A_TIME_PREFERIDO` | 1 | 2018-03-28 | — |
| `IV$ATIVREQUISITO` | 1 | 2017-04-19 | — |
| `IV$DADOS_PESSOA_COMPLETA` | 87 | 2025-09-24 | Segmentação |
| `IV$DOITRES` | 1 | 2023-06-05 | — |
| `IV$FICHANEG` | 1 | 2016-03-22 | BPM |
| `IV$HISTORICO` | 1 | 2025-02-17 | BPM |
| `IV$NFITEM` | 20 | 2021-08-17 | Integração |
| `IV$NFSAIDA` | 26 | 2021-08-17 | Integração |
| `IV$NFSCMPL` | 8 | 2021-08-17 | Integração |
| `IV$OPERADOR` | 1 | 2025-02-17 | — |
| `IV$OPERGRUPO` | 1 | 2025-02-17 | — |
| `IV$OrdemServico` | 52 | 2021-08-19 | Integração |
| `IV$OSItem` | 13 | 2021-08-18 | Integração |
| `IV$OSSolicitacao` | 10 | 2021-08-18 | Integração |
| `IV$P_AGRICULTURA_PRECISAO` | 1 | 2025-12-01 | — |
| `IV$P_CNAE` | 1 | 2024-09-12 | — |
| `IV$P_COLHEDORA_DE_CANA` | 1 | 2026-01-13 | — |
| `IV$P_COLHEITADEIRA_GRAOS` | 1 | 2026-01-13 | — |
| `IV$P_CONTRATO_GFC` | 1 | 2024-09-12 | — |
| `IV$P_DISTRIBUIDORA` | 1 | 2024-08-23 | — |
| `IV$P_EQUIPAMENTO_SISDIA` | 1 | 2018-03-23 | — |
| `IV$P_EQUIPAMENTOS` | 1 | 2018-03-20 | — |
| `IV$P_EQUIPAMENTOS_MANITOU` | 1 | 2025-01-08 | — |
| `IV$P_EQUIPAMENTOS_NOVO` | 1 | 2018-03-21 | — |
| `IV$P_EQUIPAMENTOS111` | 1 | 2018-03-20 | — |
| `IV$P_FENO_E_FORRAGEM` | 1 | 2019-10-04 | — |
| `IV$P_FROTA` | 1 | 2025-08-12 | — |
| `IV$P_IMPLEMENTO` | 1 | 2025-01-08 | — |
| `IV$P_LOCACAO` | 1 | 2017-12-08 | — |
| `IV$P_NJUR` | 1 | 2024-09-12 | — |
| `IV$P_NO_AGRICULTURA_PREC` | 1 | 2025-11-28 | — |
| `IV$P_NO_AREA_PLANTADA` | 1 | 2025-04-14 | — |
| `IV$P_NO_AREA_TOTAL` | 1 | 2025-04-14 | — |
| `IV$P_NO_CHACARA` | 1 | 2025-04-14 | — |
| `IV$P_NO_COLHEDORA_DE_CAN` | 1 | 2025-04-14 | — |
| `IV$P_NO_COLHEITADEIRA_GR` | 1 | 2025-11-28 | — |
| `IV$P_NO_CONSORCIO` | 1 | 2025-11-28 | — |
| `IV$P_NO_ESTANCIA` | 1 | 2025-04-14 | — |
| `IV$P_NO_FAZENDA` | 1 | 2025-04-14 | — |
| `IV$P_NO_FENO_E_FORRAGEM` | 1 | 2025-11-28 | — |
| `IV$P_NO_FROTA` | 1 | 2025-04-14 | — |
| `IV$P_NO_IMOVEL` | 1 | 2025-04-14 | — |
| `IV$P_NO_IMPLEMENTO` | 1 | 2025-11-28 | — |
| `IV$P_NO_IMPLEMENTOS` | 1 | 2025-04-14 | — |
| `IV$P_NO_JOHN_DEERE` | 1 | 2025-04-14 | — |
| `IV$P_NO_MAQUINA_IMPLEMENT` | 1 | 2025-04-14 | — |
| `IV$P_NO_ORIGEM_DA_RECEIT` | 1 | 2025-12-04 | — |
| `IV$P_NO_PLANTADEIRA` | 1 | 2026-05-12 | — |
| `IV$P_NO_PLATAFORMA_ADICI` | 1 | 2025-11-28 | — |
| `IV$P_NO_PROPRIEDADE_RURAL` | 1 | 2025-04-14 | — |
| `IV$P_NO_PULVERIZADOR` | 1 | 2025-11-28 | — |
| `IV$P_NO_SEGURO_DE_VIDA` | 1 | 2025-04-14 | — |
| `IV$P_NO_SITIO` | 1 | 2025-04-14 | — |
| `IV$P_NO_TIPO_CULTURA` | 1 | 2025-04-14 | — |
| `IV$P_NO_TRATOR` | 1 | 2025-11-28 | — |
| `IV$P_NO_TRATOR_MANUAL_FRO` | 1 | 2025-01-07 | — |
| `IV$P_NO_TRATOR_SISDIA` | 1 | 2025-01-27 | — |
| `IV$P_NO_TURF` | 1 | 2025-11-28 | — |
| `IV$P_NO_VEICULO` | 1 | 2025-04-14 | — |
| `IV$P_OPERATIONS_CENTER` | 1 | 2026-06-09 | — |
| `IV$P_ORIGEM_DA_RECEITA` | 1 | 2025-06-09 | — |
| `IV$P_PLANTADEIRA` | 1 | 2026-01-13 | — |
| `IV$P_PLATAFORMA_ADICIONAL` | 1 | 2021-05-03 | — |
| `IV$P_PULVERIZADOR` | 1 | 2026-01-13 | — |
| `IV$P_SEGURO` | 1 | 2024-09-12 | — |
| `IV$P_TRATOR` | 1 | 2026-01-13 | — |
| `IV$P_TURF` | 1 | 2025-01-08 | — |
| `IV$PESSOA_GEOSIGA` | 1 | 2016-03-22 | BPM |
| `IV$PG_AGRICULTURA_PRECISAO` | 1 | 2017-02-03 | — |
| `IV$PG_ALCADA` | 1 | 2019-07-09 | — |
| `IV$PG_AMS_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_ANO` | 1 | 2024-08-29 | — |
| `IV$PG_AUDITORIA` | 1 | 2019-08-14 | — |
| `IV$PG_CAMPANHA_DE_INCENTIV` | 1 | 2019-07-03 | — |
| `IV$PG_CLASSE_RESULTADO` | 1 | 2023-10-09 | — |
| `IV$PG_COLHEDORA_CANA_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_COLHEDORA_DE_CANA` | 1 | 2025-07-21 | — |
| `IV$PG_COLHEITADEIRA_ANO` | 1 | 2025-07-17 | — |
| `IV$PG_COLHEITADEIRA_GRAOS` | 1 | 2025-07-17 | — |
| `IV$PG_COLHEITADEIRA_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_CONSORCIO` | 1 | 2016-04-13 | — |
| `IV$PG_CULTURA` | 1 | 2025-07-17 | — |
| `IV$PG_EQUIPAMENTOS` | 1 | 2018-03-20 | — |
| `IV$PG_FAMILIA_DEPTOS` | 1 | 2023-02-01 | — |
| `IV$PG_FENO_E_FORRAGEM` | 1 | 2017-02-03 | — |
| `IV$PG_GREEN_SYSTEM` | 1 | 2016-03-22 | — |
| `IV$PG_GRUPO_REGIONAL` | 1 | 2023-01-27 | — |
| `IV$PG_IMP_GREEN_SYSTEM` | 1 | 2016-03-22 | — |
| `IV$PG_IMPLEMENTOS` | 1 | 2020-05-17 | — |
| `IV$PG_IMPLEMENTOS_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_INST_FINANCEIRA` | 1 | 2016-03-22 | — |
| `IV$PG_INTEGRACAO_FAMILIA` | 1 | 2025-10-29 | Integração |
| `IV$PG_MARCA_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_MARCAS` | 1 | 2017-05-10 | — |
| `IV$PG_MARCAS_E_FAMILIA` | 1 | 2023-01-24 | — |
| `IV$PG_META_DEPTO` | 1 | 2025-05-12 | — |
| `IV$PG_ORIGEM_DA_RECEITA` | 1 | 2016-11-10 | — |
| `IV$PG_PLANTADEIRA` | 1 | 2025-07-17 | — |
| `IV$PG_PLANTADEIRA_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_PLATAFORMA_ADICIONAL` | 1 | 2017-02-07 | — |
| `IV$PG_POLITICA_DE_COMISSAO` | 1 | 2016-05-31 | — |
| `IV$PG_PRODUTOSISDIA` | 1 | 2016-12-26 | — |
| `IV$PG_PULVERIZADOR` | 1 | 2025-07-21 | — |
| `IV$PG_PULVERIZADOR_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_RESPONSAVEL_TECNICO` | 1 | 2016-03-22 | — |
| `IV$PG_RETAIL` | 1 | 2019-07-09 | — |
| `IV$PG_REVENDAS` | 1 | 2016-03-22 | — |
| `IV$PG_SEGURO` | 1 | 2022-05-04 | — |
| `IV$PG_SEM_INCENTIVO` | 1 | 2019-07-09 | — |
| `IV$PG_STATUS_PESSOA_DEPTO` | 1 | 2018-03-23 | — |
| `IV$PG_TIPO` | 1 | 2025-01-14 | — |
| `IV$PG_TRATOR_TESTE` | 1 | 2022-08-11 | Teste/backup |
| `IV$PG_TRATOR_TOTVS` | 1 | 2022-08-12 | Integração |
| `IV$PG_TRATORES` | 1 | 2024-12-16 | — |
| `IV$PG_TURF` | 1 | 2017-02-03 | — |
| `IV$PG_VENDEDOR` | 1 | 2025-04-29 | — |
| `IV$PROCESSO` | 1 | 2025-02-17 | BPM |
| `IV$PRODUTO` | 29 | 2020-07-15 | — |
| `IV$RESULTADO` | 1 | 2025-02-17 | — |
| `IV$RESULTADOFULL` | 1 | 2025-02-17 | — |
| `IV$S_AGENDA` | 45 | 2025-08-12 | BPM |
| `IV$S_CARTEIRA` | 49 | 2025-08-13 | Segmentação |
| `IV$S_CONTATO` | 1 | 2025-02-17 | — |
| `IV$S_ENDERECOADIC` | 1 | 2025-02-17 | — |
| `IV$S_HISTORICO` | 1 | 2025-02-17 | BPM |
| `IV$S_PESSOA` | 1 | 2025-02-17 | — |
| `IV$S_PESSOA_CLASSE` | 1 | 2025-02-17 | — |
| `IV$S_RFV` | 1 | 2025-02-17 | Segmentação |
| `IV$TITULO` | 55 | 2018-02-27 | Integração |
| `IV$TITULOEMPR` | 1 | 2025-02-17 | Integração |
| `IV$TITULOORIGEM` | 2 | 2017-09-15 | — |
| `IV$TITULOVENC` | 23 | 2017-12-27 | Integração |
| `IV_EQUIPE` | 1 | 2025-02-17 | — |
| `IV_EQUIPEEMPR` | 1 | 2025-02-17 | — |
| `IV_Q$ABERTURA_OS_REVISAO` | 1 | 2025-02-14 | — |
| `IV_Q$ACOMP_VEND_MAQUINAS` | 1 | 2022-11-11 | — |
| `IV_Q$ACOMP_VENDA_DIRETA` | 1 | 2017-06-01 | — |
| `IV_Q$ACOMP_VENDA_DIRETAJD` | 1 | 2023-07-19 | — |
| `IV_Q$ACOMP_VENDA_FINANC` | 1 | 2026-08-18 | — |
| `IV_Q$ACOMP_VENDA_LOCACAO` | 1 | 2017-11-09 | — |
| `IV_Q$ACOMP_VENDA_MANITOU` | 1 | 2018-03-19 | — |
| `IV_Q$ACOMP_VENDA_USADO` | 1 | 2018-03-19 | — |
| `IV_Q$ACOMPAN_COMPRA_JDE` | 43 | 2026-05-22 | — |
| `IV_Q$ACOMPAN_VEND_CONCESS` | 1 | 2021-01-25 | — |
| `IV_Q$ACOMPANH_VENDA_JDE` | 49 | 2022-08-26 | — |
| `IV_Q$ACOMPANHA_COMPRA_IMP` | 45 | 2026-05-22 | — |
| `IV_Q$ACOMPANHAM_VENDA_IMP` | 16 | 2022-08-26 | — |
| `IV_Q$ACOMPANHAMENTO_VENDA` | 1 | 2016-03-22 | — |
| `IV_Q$ADM_FINANCEIRO` | 1 | 2025-06-05 | — |
| `IV_Q$ADM_FINANCEIRO_N` | 86 | 2024-03-08 | — |
| `IV_Q$AFE_VENDA_MQ_IM_USAD` | 1 | 2016-03-22 | — |
| `IV_Q$AFERICAO_CSC` | 1 | 2021-03-02 | — |
| `IV_Q$AFERICAO_DE_PECAS` | 1 | 2020-06-18 | — |
| `IV_Q$AFERICAO_IMPLEMENTO` | 1 | 2020-08-27 | — |
| `IV_Q$AFERICAO_MAQ_1_CONT` | 1 | 2020-07-23 | — |
| `IV_Q$AFERICAO_MAQ_1_WEB` | 1 | 2020-07-23 | — |
| `IV_Q$AFERICAO_MAQ_2_CONT` | 1 | 2020-07-23 | — |
| `IV_Q$AFERICAO_MAQ_2_WEB` | 1 | 2020-07-23 | — |
| `IV_Q$AFERICAO_MAQ_3_CONT` | 1 | 2020-07-23 | — |
| `IV_Q$AFERICAO_MAQ_3_WEB` | 1 | 2020-07-24 | — |
| `IV_Q$AFERICAO_PECAS` | 1 | 2016-03-22 | — |
| `IV_Q$AFERICAO_POS_SOLUCAO` | 1 | 2016-03-22 | — |
| `IV_Q$AFERICAO_SERV_WEB` | 11 | 2020-05-14 | — |
| `IV_Q$AFERICAO_SERVICOS` | 1 | 2016-03-22 | — |
| `IV_Q$AFERICAO_SUPORTE_INT` | 1 | 2018-01-18 | — |
| `IV_Q$AFERICAO_VENDA_MAQ` | 1 | 2018-02-27 | — |
| `IV_Q$AFERICAO_VENDA_MQ_IM` | 1 | 2016-03-22 | — |
| `IV_Q$AGUARDAR_PECAS` | 1 | 2024-09-11 | — |
| `IV_Q$ALTERADO_PAGAMENTO` | 1 | 2025-08-07 | — |
| `IV_Q$ANALISE_DE_CREDITO` | 1 | 2022-03-31 | — |
| `IV_Q$APRESENT_EQUIPAMENTO` | 1 | 2016-03-22 | — |
| `IV_Q$APRESENT_IMPLEMENTO` | 1 | 2016-03-22 | — |
| `IV_Q$APRESENTACAO` | 1 | 2016-03-22 | — |
| `IV_Q$APRESENTACAO_JDE` | 1 | 2016-03-22 | — |
| `IV_Q$APROVACAO_TCSM` | 1 | 2024-09-11 | — |
| `IV_Q$ATUALIZACAO_PUK` | 1 | 2026-02-10 | — |
| `IV_Q$AVAL_COLH_CANA_USADA` | 1 | 2016-03-22 | — |
| `IV_Q$AVAL_TRATORES_USADOS` | 1 | 2016-03-22 | — |
| `IV_Q$AVAL_USADO_ENTRADA` | 1 | 2022-06-10 | — |
| `IV_Q$AVALIA_COLHEIT_USADA` | 1 | 2016-03-22 | — |
| `IV_Q$AVALIA_IMPLEM_USADO` | 13 | 2019-12-01 | — |
| `IV_Q$AVALIA_USADO_ENTRADA` | 1 | 2019-03-29 | — |
| `IV_Q$AVALIACAO_AMS_USADO` | 1 | 2021-02-07 | — |
| `IV_Q$CADASTROS_LISTAS` | 1 | 2016-03-22 | — |
| `IV_Q$CANCEL_RENOVACAO_SEG` | 1 | 2025-05-07 | — |
| `IV_Q$CANCELAMENTO_SEGURO` | 1 | 2024-12-26 | — |
| `IV_Q$CANHOTO_DIGITAL` | 1 | 2025-06-13 | — |
| `IV_Q$CHASSI_ENTREGA_FISIC` | 1 | 2024-11-27 | — |
| `IV_Q$CHASSI_EQUIPAMENTO` | 1 | 2025-03-13 | — |
| `IV_Q$CHASSI_PMP` | 1 | 2024-09-11 | — |
| `IV_Q$CHEGADA_IMPLEMENTO` | 1 | 2016-03-22 | — |
| `IV_Q$COM_INTERESSE_FUTURO` | 1 | 2026-03-31 | — |
| `IV_Q$COMISSAO` | 14 | 2021-10-04 | — |
| `IV_Q$COMISSAO_AMS` | 12 | 2021-11-29 | — |
| `IV_Q$COMISSAO_CONTACHAVE` | 11 | 2020-12-14 | — |
| `IV_Q$COMISSAO_LOCACAO` | 1 | 2017-11-06 | — |
| `IV_Q$COMISSAO_SERV_AMS` | 1 | 2021-07-27 | — |
| `IV_Q$COMISSAO_USADO` | 1 | 2017-09-19 | — |
| `IV_Q$COMISSAO_VD_LOCACAO` | 1 | 2017-11-09 | — |
| `IV_Q$COMPETID_NEGOC_IMPL` | 1 | 2016-03-22 | — |
| `IV_Q$COMPETIDORES_NA_NEG` | 1 | 2016-03-22 | — |
| `IV_Q$COMPETIDORES_NA_NEGO` | 1 | 2024-01-31 | — |
| `IV_Q$CONDICOES_DE_VENDAS` | 1 | 2025-08-07 | — |
| `IV_Q$CONT_COMISSAO_22` | 1 | 2022-04-25 | — |
| `IV_Q$COTA_CONSORCIO` | 1 | 2025-07-10 | — |
| `IV_Q$DEMO_EQUIP_JD` | 1 | 2026-02-26 | — |
| `IV_Q$DEMO_IMPLEMENTO` | 1 | 2016-03-22 | — |
| `IV_Q$DEMO_MAQUINAS` | 11 | 2020-05-17 | — |
| `IV_Q$DEMO_TRATOR` | 1 | 2024-02-05 | — |
| `IV_Q$DEMONSTR_COLHEITAD` | 1 | 2016-03-22 | — |
| `IV_Q$DEMONSTRACAO_JD` | 1 | 2026-03-18 | — |
| `IV_Q$DEMONSTRACAO_LOG` | 1 | 2026-02-04 | — |
| `IV_Q$DEMONSTRACAO_NF` | 1 | 2026-01-19 | — |
| `IV_Q$DEMONSTRACAO_TRATOR` | 1 | 2016-03-22 | — |
| `IV_Q$DEVOLUCAO_PECA` | 1 | 2024-09-11 | — |
| `IV_Q$DEVOLUCAO_PUK` | 1 | 2025-08-27 | — |
| `IV_Q$DOC_ANALISE_CREDITO` | 1 | 2016-08-08 | — |
| `IV_Q$EVENTOS_AFERICAO` | 1 | 2023-04-17 | — |
| `IV_Q$EXP_FLUXO_MODELER` | 1 | 2024-07-25 | — |
| `IV_Q$FORA_SERVICO_PMP` | 1 | 2024-09-11 | — |
| `IV_Q$FORM_TREINO` | 1 | 2018-01-18 | — |
| `IV_Q$GAR_DATA_SERVICO` | 1 | 2024-09-11 | — |
| `IV_Q$GAR_FAB_SOL_PECA` | 1 | 2025-02-26 | — |
| `IV_Q$GESTAO_CREDITO` | 1 | 2023-05-31 | — |
| `IV_Q$GESTAO_CREDITO_AMS` | 15 | 2022-12-01 | — |
| `IV_Q$GESTAO_CREDITO_IMP` | 15 | 2023-07-27 | — |
| `IV_Q$GESTAO_PRODUTO___AMS` | 1 | 2023-02-24 | — |
| `IV_Q$GESTAO_PRODUTO_IMPL` | 1 | 2023-02-24 | — |
| `IV_Q$HORIMETRO_AGREGA` | 1 | 2024-09-19 | — |
| `IV_Q$INCENTIVO` | 1 | 2019-08-02 | — |
| `IV_Q$INTERESSE_FUTURO_PRO` | 1 | 2023-12-04 | — |
| `IV_Q$INTERESSE_PROJETO_IR` | 1 | 2024-01-28 | — |
| `IV_Q$LIBERAR_DEMONSTRACAO` | 1 | 2026-01-19 | — |
| `IV_Q$LICENCAS_PUK` | 1 | 2026-02-12 | — |
| `IV_Q$LOCACAO_COMISSAO` | 1 | 2017-11-09 | — |
| `IV_Q$OFERECE_RENOV_SEGURO` | 1 | 2024-12-27 | — |
| `IV_Q$ORIGEM_DA_RENDA` | 1 | 2016-03-22 | — |
| `IV_Q$OS_ABERTA` | 1 | 2024-09-19 | — |
| `IV_Q$OS_CORTESIA` | 1 | 2024-09-11 | — |
| `IV_Q$OS_GARANTIA` | 1 | 2025-02-14 | — |
| `IV_Q$OS_REVISAO_ENTREGA` | 1 | 2024-09-19 | — |
| `IV_Q$PECAS_AFERICAO` | 1 | 2020-06-18 | — |
| `IV_Q$PEDIDO_GC` | 1 | 2025-10-01 | — |
| `IV_Q$PEDIDO_KAM` | 1 | 2026-04-01 | — |
| `IV_Q$PEDIDO_SAM` | 1 | 2026-04-01 | — |
| `IV_Q$PERCEPCAO_JD` | 1 | 2026-02-10 | — |
| `IV_Q$PESQ_SATISFACAO_PECA` | 1 | 2016-03-22 | — |
| `IV_Q$PESQUISA_NPS` | 1 | 2025-06-13 | — |
| `IV_Q$PESQUISA_TI` | 1 | 2018-03-26 | — |
| `IV_Q$PREMIO_DEMO` | 1 | 2021-04-29 | — |
| `IV_Q$PREVISAO_RECEBIMENTO` | 1 | 2016-03-22 | — |
| `IV_Q$PRODUTO_RD` | 1 | 2026-07-13 | — |
| `IV_Q$PROPOSTA_COMERCIAL` | 1 | 2025-04-09 | — |
| `IV_Q$PROSPECCAO_SERV__JD` | 1 | 2019-08-08 | — |
| `IV_Q$QUALIDADE_PECAS` | 1 | 2016-03-22 | — |
| `IV_Q$QUALIDADE_SERVICOS` | 1 | 2020-05-12 | — |
| `IV_Q$QUALIDADE_VENDAMAQ` | 1 | 2016-03-22 | — |
| `IV_Q$RECEB_FINAN_IMPLEM` | 1 | 2016-03-22 | — |
| `IV_Q$RECEBIMENTO_A_PRAZO` | 1 | 2025-04-11 | — |
| `IV_Q$RECEBIMENTO_COMISSAO` | 1 | 2024-02-05 | — |
| `IV_Q$RECEBIMENTO_FINANC` | 1 | 2016-03-22 | — |
| `IV_Q$RESPONSAVEL_TECNICO` | 1 | 2016-03-22 | — |
| `IV_Q$RESULTADO_DEMO` | 1 | 2021-02-14 | — |
| `IV_Q$RETORNADO_JD` | 1 | 2024-09-11 | — |
| `IV_Q$REV_DATA_SERVICO` | 1 | 2025-03-12 | — |
| `IV_Q$REVISAO_100H` | 1 | 2025-03-21 | — |
| `IV_Q$REVISAO_1100_1150H` | 1 | 2025-03-21 | — |
| `IV_Q$REVISAO_1500H` | 1 | 2025-03-21 | — |
| `IV_Q$REVISAO_450_600H` | 1 | 2025-03-21 | — |
| `IV_Q$REVISAO_800H` | 1 | 2025-03-21 | — |
| `IV_Q$REVISAO_FIM_GARANTIA` | 1 | 2025-03-21 | — |
| `IV_Q$ROMANEIO_DEV_PECA` | 1 | 2024-09-11 | — |
| `IV_Q$SEPARACAO_PEDIDO` | 1 | 2025-09-29 | — |
| `IV_Q$SERVICO_EXTERNOS_JD` | 1 | 2019-08-16 | — |
| `IV_Q$SERVICOS_AFERICAO` | 1 | 2020-05-22 | — |
| `IV_Q$SOLICITACAO_TCAT` | 1 | 2024-11-25 | — |
| `IV_Q$TESTE_PRIMEIRO_JD` | 1 | 2024-08-22 | Teste/backup |
| `IV_Q$TESTE1` | 1 | 2022-08-05 | Teste/backup |
| `IV_Q$TESTE2` | 1 | 2022-08-05 | Teste/backup |
| `IV_Q$TICKET_DSI` | 1 | 2025-01-21 | — |
| `IV_Q$VENDA` | 1 | 2025-05-20 | — |
| `IV_Q$VENDA_AMS` | 16 | 2020-10-27 | — |
| `IV_Q$VENDA_CONSORCIO` | 1 | 2024-02-06 | — |
| `IV_Q$VENDA_DIRETA` | 1 | 2025-05-19 | — |
| `IV_Q$VENDA_DSI` | 1 | 2025-01-21 | — |
| `IV_Q$VENDA_EQUIPAMENTO` | 1 | 2026-07-06 | — |
| `IV_Q$VENDA_LOCACAO` | 1 | 2017-11-09 | — |
| `IV_Q$VENDA_MAQUINA_FY25` | 1 | 2025-02-03 | — |
| `IV_Q$VENDA_N` | 72 | 2024-04-17 | — |
| `IV_Q$VENDA_PERDIDA` | 1 | 2022-06-07 | — |
| `IV_Q$VENDA_PERDIDA_FY25` | 1 | 2026-02-04 | — |
| `IV_Q$VENDA_PERDIDA_IMPL` | 1 | 2016-03-22 | — |
| `IV_Q$VENDA_PERDIDA_IMPLEM` | 1 | 2016-03-22 | — |
| `IV_Q$VENDA_PERDIDA_JDE` | 1 | 2024-01-31 | — |
| `IV_Q$VENDA_PERDIDA_MANITO` | 1 | 2017-06-05 | — |
| `IV_Q$VENDA_PERDIDA_MAQIMP` | 1 | 2025-04-17 | — |
| `IV_Q$VENDA_PERDIDA_PROD` | 1 | 2022-11-25 | — |
| `IV_Q$VENDA_PERDIDA_SEGURO` | 1 | 2023-05-25 | — |
| `IV_Q$VENDA_PERDIDA_TESTE` | 1 | 2024-09-13 | Teste/backup |
| `IV_Q$VENDA_PNEUS` | 1 | 2026-01-12 | — |
| `IV_Q$VENDA_PRECISION_UP` | 1 | 2025-01-21 | — |
| `IV_Q$VENDA_SEMINOVO` | 1 | 2025-08-27 | — |
| `IV_Q$VENDA_SERVICOS_AMS` | 1 | 2017-12-21 | — |
| `IV_Q$VENDA_VP_PNEUS` | 1 | 2025-12-12 | — |
| `IV_Q$VENDAPERDIDA_SEGURO` | 1 | 2024-12-27 | — |
| `IV_Q$VENDER_RENOVACAO_SEG` | 1 | 2024-12-27 | — |
| `IV_Q$VISITA_DSI` | 1 | 2025-02-20 | — |
| `IV_Q$VISITA_EXP_CLIENTE` | 1 | 2018-08-27 | — |
| `IV_Q$VP_COLHEDORA` | 1 | 2025-09-09 | — |
| `IV_Q$VP_COLHEITADEIRA` | 1 | 2025-05-19 | — |
| `IV_Q$VP_PLANTADEIRA` | 1 | 2025-05-19 | — |
| `IV_Q$VP_PULVERIZADOR` | 1 | 2025-07-14 | — |
| `IV_Q$VP_RENOVACAO_SEGURO` | 1 | 2024-12-27 | — |
| `IV_Q$VP_SEM_PARTICIPACAO` | 1 | 2026-02-04 | — |
| `IV_Q$VP_TRATOR` | 1 | 2025-07-14 | — |
| `iv_w3_conglomerado` | 28 | 2025-08-13 | — |
| `SYSCOLUMN` | 1 | 2025-02-17 | — |
| `SYSTABLE` | 1 | 2025-02-17 | — |
| `tba_clientes` | 34 | 2025-01-29 | Segmentação, Relatório |
| `tba_usuarios` | 13 | 2026-01-29 | Relatório |
| `teste_felipe` | 141 | 2019-06-27 | BPM, Teste/backup |
| `V_C5SYSCOLUMN` | 1 | 2025-02-17 | — |
| `V_C5SYSCONSTRAINT` | 1 | 2025-02-17 | — |
| `V_C5SYSINDEX` | 1 | 2025-02-17 | — |
| `V_C5SYSPK` | 1 | 2025-02-17 | — |
| `V_C5SYSREFERENCE` | 1 | 2025-02-17 | — |
| `V_C5SYSTABLE` | 1 | 2025-02-17 | — |
| `V_UTIL_TELEFONES_CONTATOS` | 156 | 2025-09-11 | — |
| `VBI$CONTA` | 1 | 2017-07-12 | — |
| `VBI$CONTAFAM` | 1 | 2017-07-11 | — |
| `VW_REL_TBA101` | 157 | 2026-04-06 | BPM, Integração, Relatório |
| `X_CRM_BI_CONGLOMERADO` | 62 | 2023-07-10 | Integração, Relatório |
| `X_CRM_BI_FATURAMENTO_PECA_CARTEIRA` | 140 | 2023-08-15 | Integração, Segmentação, Relatório |
| `X_CRM_BI_FUNIL_PECA_CARTEIRA` | 375 | 2023-08-15 | Integração, Segmentação, Relatório |
| `X_CRM_BI_FUNIL_PECA_CLASSE` | 194 | 2023-04-24 | BPM, Integração, Segmentação, Relatório |
| `X_V_BI_DESPESAS_VENDA_MAQUINAS` | 719 | 2023-03-31 | Integração, Relatório |
| `X_V_CRM_IMP_IMP_NFS` | 121 | 2023-10-04 | Integração |
| `X_V_CRM_IMP_NFSItem` | 99 | 2023-10-04 | Integração |
| `ZZ_IV$NFITEM` | 23 | 2017-01-25 | Integração |
| `ZZ_IV$NFSAIDA` | 28 | 2017-01-25 | Integração |
| `ZZ_IV$NFSCMPL` | 9 | 2017-01-25 | Integração |
| `ZZ_IV$OrdemServico` | 44 | 2017-01-25 | Integração |
| `ZZ_IV$OSItem` | 11 | 2017-01-25 | Integração |
| `ZZ_IV$OSSolicitacao` | 9 | 2017-01-25 | Integração |

