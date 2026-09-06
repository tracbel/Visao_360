# Volumetria do banco CRM

Coletado em 2026-09-02 23:27. Fonte: `sys.partitions` e `sys.allocation_units`.

> `sys.dm_db_partition_stats` está **negado** para o login de leitura (falta `VIEW DATABASE STATE`).
> As views de catálogo usadas aqui devolvem os mesmos números de linhas e de páginas.

| Métrica | Valor |
|---|---:|
| Tabelas | 767 |
| Linhas somadas | 86.322.482 |
| Espaço reservado | 32,4 GB |
| Arquivo ROWS `CRM` | 36270 MB (crescimento 10 %, máximo ilimitado) |
| Arquivo LOG `CRM_log` | 257 MB (crescimento 1024 MB, máximo 15360 MB) |

## As 25 maiores tabelas

| # | Tabela | Linhas | Reservado (MB) |
|---:|---|---:|---:|
| 1 | `GE_LOG_PROCESSO` | 12.714.111 | 5.363,2 |
| 2 | `GE_LgTb` | 11.861.777 | 4.232,6 |
| 3 | `IV_AgendaLog` | 11.143.876 | 2.767,5 |
| 4 | `GE_Log2` | 4.591.737 | 1.320,5 |
| 5 | `EXT_VeicModelo` | 4.431.168 | 273,0 |
| 6 | `GEP_SyncUsrSat` | 3.970.706 | 3.903,1 |
| 7 | `EXT_VeicPlanoMan` | 3.571.257 | 366,4 |
| 8 | `EXT_VeicModPlano` | 3.571.235 | 287,5 |
| 9 | `IV_Historico` | 2.485.762 | 1.512,8 |
| 10 | `IV_ProcDado` | 1.532.670 | 649,2 |
| 11 | `EXT_NFSItem` | 1.300.126 | 410,3 |
| 12 | `IV_Processo` | 1.190.768 | 589,7 |
| 13 | `EXT_TituloMov` | 1.134.039 | 201,9 |
| 14 | `GEP_JobAgdExecLog` | 1.092.904 | 347,7 |
| 15 | `GE_LOG_HISTORICO` | 1.048.114 | 398,7 |
| 16 | `IV_Agenda` | 966.941 | 833,4 |
| 17 | `GE_LOG_PESSOA` | 961.628 | 395,9 |
| 18 | `IMP_Titulo` | 892.131 | 416,3 |
| 19 | `IV_ProcStatMonit` | 865.629 | 124,1 |
| 20 | `X_V_IMP_CRM_IMP_NF` | 815.854 | 917,4 |
| 21 | `X_TOTVS_CRM_FATURAMENTO` | 809.821 | 2.519,8 |
| 22 | `GE_LOG_TRANS` | 803.647 | 335,7 |
| 23 | `IV_Interacao` | 720.525 | 58,3 |
| 24 | `IV_ProcFaseMonit` | 666.855 | 109,0 |
| 25 | `IV_HistLink` | 645.850 | 78,3 |

## Crescimento por ano

| Tabela | Coluna de data | 2019 | 2020 | 2021 | 2022 | 2023 | 2024 | 2025 | 2026 | 2028 | 2030 |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| `IV_Processo` | `DtaInclusao` | 96.438 | 104.984 | 108.373 | 72.142 | 143.034 | 114.846 | 101.335 | 47.771 | — | — |
| `IV_Historico` | `DTAINCLUSAO` | — | — | — | — | 124.937 | 160.115 | 273.734 | 126.731 | — | — |
| `IV_Agenda` | `DtaRealizacao` | 70.266 | 110.307 | 113.567 | 84.496 | 102.582 | 71.196 | 178.170 | 82.756 | — | — |
| `IV_Questionario` | `DtaRealizacao` | 4.346 | 6.691 | 5.763 | 5.837 | 7.209 | 3.374 | 25.633 | 7.089 | — | — |
| `GE_Pessoa` | `DtaInclusao` | 1.769 | 1.479 | 1.205 | 1.174 | 1.018 | 27.258 | 2.842 | 2.510 | — | — |
| `EXT_NFS` | `DtaEmissaoNF` | 46.995 | 43.509 | 41.753 | 22.133 | 76.109 | 61.732 | 14.441 | — | — | — |
| `EXT_OS` | `DtaAlteracao` | — | — | 2.105 | 1.568 | 2.291 | 2.135 | — | — | — | — |
| `EXT_Titulo` | `DtaEmissao` | 70.990 | 71.507 | 66.447 | 66.632 | 69.179 | 73.702 | 29.951 | — | — | — |
| `EXT_Veic` | `DtaAlteracao` | 1.602 | 1.829 | 1.470 | 239 | 444 | 302 | — | — | — | — |
| `GE_LOG_PROCESSO` | `DTALOG` | — | — | — | — | 2.601.787 | 6.688.755 | 3.346.795 | 76.774 | — | — |
| `GE_LgTb` | `DtaLog` | 2.402.291 | 2.300.483 | 1.785.117 | 2.894.935 | 2.404.061 | — | — | — | — | — |
| `DMN_Doc` | `DtaBase` | 22 | 96 | 13 | 5 | 7 | 203 | 713 | 454 | — | — |
| `IV_AgendaLog` | `DtaLog` | 712.663 | 679.629 | 548.310 | 381.650 | 2.233.561 | 3.449.472 | 1.964.119 | 220.875 | — | — |
| `IV_ProcDado` | `DtaUltResultado` | 133.128 | 135.433 | 173.938 | 84.095 | 145.381 | 111.957 | 98.281 | 47.324 | — | — |
| `IV_Interacao` | `DtaAgenda` | 91.044 | 137.739 | 146.813 | 104.410 | 44.934 | 3 | 4 | — | 1 | 1 |
| `GEP_EMAILSENT` | `DTAENVIO` | 68.158 | 114.179 | 93.314 | 71.211 | 89.700 | 37.930 | 53.731 | 35.660 | — | — |
| `IMP_Titulo` | `DtaEmissao` | — | — | 23 | 1.365 | 3.869 | 34.401 | 557.346 | 295.127 | — | — |
| `IMP_OS` | `DtaGeracao` | — | — | 64.779 | 112.098 | 99.593 | 11.398 | — | — | — | — |

## Última atividade por tabela

| Situação | Tabelas |
|---|---:|
| VAZIA | 182 |
| PARADA (>1 ano) | 118 |
| viva | 90 |
| ok | 30 |
| fria (>90 dias) | 22 |
| PULADA POR VOLUME | 3 |

Critério: `MAX()` da coluna de data de inclusão mais provável de cada tabela. **viva** = recebeu
linha nos últimos 90 dias; **fria** = entre 90 e 365 dias; **PARADA** = mais de um ano sem linha nova.

### Tabelas PARADAS há mais de um ano com mais de 10.000 linhas

| Tabela | Linhas | Primeira | Última | Coluna |
|---|---:|---|---|---|
| `EXT_NFSItem` | 1.300.126 | 2021-08-02 | 2023-10-03 | `DtaImport` |
| `EXT_TituloMov` | 1.134.039 | 1996-01-01 | 2023-05-18 | `DtaMov` |
| `X_V_IMP_CRM_IMP_NF` | 815.854 | 2023-10-03 | 2025-04-11 | `X_DATA_INTEGRACAO` |
| `X_TOTVS_CRM_FATURAMENTO` | 809.821 | 2021-08-01 | 2025-04-11 | `DATA_EMISSAO_NF` |
| `EXT_Titulo` | 577.925 | 2015-01-09 | 2025-05-21 | `DtaEmissao` |
| `GEP_Import_bkpjun` | 502.009 | 2017-02-08 | 2024-12-10 | `DtaGeracao` |
| `X_V_IMP_CRM_IMP_NF_BKP_18_09_2023` | 415.762 | 2023-09-15 | 2023-09-15 | `X_DATA_INTEGRACAO` |
| `X_TOTVS_BI_FATURAMENTO_POS_VENDAS` | 407.100 | 2021-08-02 | 2023-09-29 | `DATA_EMISSAO_NF` |
| `IMP_OSITEM` | 399.040 | 2021-08-02 | 2024-05-26 | `DtaGeracao` |
| `EXT_NFS` | 390.755 | 2017-01-04 | 2025-04-11 | `DtaEmissaoNF` |
| `IV_CbrCobrancaTitLog` | 304.684 | 2017-09-27 | 2025-05-21 | `DtaLog` |
| `IV_CBRTITULOHST` | 173.530 | 2019-05-31 | 2023-03-22 | `DATA` |
| `IV_ClientePropr_ITA` | 157.900 | 2008-11-07 | 2024-12-19 | `DtaInclusao` |
| `IV_CbrCobrancaTit` | 155.204 | 2017-09-27 | 2025-05-21 | `DtaInclusao` |
| `GE_PessoaMural` | 145.313 | 2012-01-09 | 2019-12-19 | `DtaAlteracao` |
| `IVS_PES_NELSON` | 134.872 | 2012-03-22 | 2025-08-07 | `dtaultctto` |
| `IV_CBRCRITMON` | 128.167 | 2020-11-16 | 2025-05-21 | `DTAANALISE` |
| `X_T_IMP_CRM_TITULO_bkp_11_04` | 121.381 | 2021-08-02 | 2023-04-10 | `DtaEmissao` |
| `EXT_Titulo_bkp_11_04` | 108.061 | 2012-05-01 | 2023-04-10 | `DtaEmissao` |
| `GE_PessoaLink_bkpago22` | 92.068 | 2011-12-20 | 2022-09-09 | `DtaGeracao` |
| `GE_PessoaLink_BKPJUN` | 90.857 | 2011-12-20 | 2024-12-28 | `DtaGeracao` |
| `GE_Pessoa_BKPJUN` | 88.142 | 2011-12-20 | 2024-12-27 | `DtaInclusao` |
| `EXT_Produto` | 78.749 | 2017-02-22 | 2025-04-14 | `Dtaimport` |
| `GE_PessoaFone_BKPJUN` | 69.894 | 2019-12-19 | 2024-12-27 | `DTAINCLUSAO` |
| `IMP_NFSItem` | 67.404 | 2023-09-18 | 2025-04-11 | `DtaGeracao` |
| `nfsaida$` | 62.619 | 2010-05-14 | 2011-12-14 | `DTEMISSAO` |
| `IV_ProcRef` | 61.975 | 2017-01-23 | 2021-07-29 | `DtaGeracao` |
| `GE_PessoaLinkbkp` | 58.741 | 2011-12-20 | 2021-08-02 | `DtaGeracao` |
| `GE_PessoaFone_ITA` | 42.594 | 2019-12-10 | 2024-12-20 | `DTAINCLUSAO` |
| `GE_ContatoPapel_BKPJUN` | 35.413 | 2012-02-15 | 2024-12-27 | `DtaAlteracao` |
| `GE_Pessoa_ITA` | 28.377 | 2008-11-06 | 2024-12-19 | `DtaInclusao` |
| `GE_Contato_BKPJUN` | 28.051 | 2023-06-06 | 2024-12-27 | `DTAINCLUSAO` |
| `IV_SMSLog` | 27.430 | 2019-05-31 | 2021-07-29 | `DtaLog` |
| `EXT_Pessoa_bkpago22` | 27.280 | 2018-02-02 | 2022-08-11 | `DtaInclusao` |
| `IV_SMS` | 26.749 | 2019-06-01 | 2021-07-29 | `DtaEnvio` |
| `GE_PessoaLink_ITA` | 24.864 | 2014-06-26 | 2023-11-29 | `DtaGeracao` |
| `IV_CbrCobrancaLote` | 21.048 | 2017-09-22 | 2025-05-21 | `DtaGeracao` |
| `IMP_NFS` | 18.378 | 2023-01-02 | 2025-04-11 | `Dtaemissaonf` |
| `IVS_Pes_RAO_Pneus_02_03` | 18.261 | 2016-05-12 | 2016-06-13 | `DtaInclusao` |
| `EXT_VeicKM` | 17.378 | 2017-02-04 | 2024-05-23 | `DtaAlteracao` |
| `EXT_Titulo_BKP_22_03_2023_BAIXADOS` | 16.705 | 2021-08-02 | 2023-03-22 | `DtaEmissao` |
| `IVS_TGLCLICLIENT` | 16.654 | 2015-07-01 | 2015-07-01 | `DTIMPORT` |
| `EXT_VeicProp` | 12.621 | 2017-02-04 | 2024-05-24 | `DtaAlteracao` |
| `IVS_TGLCLIFIS` | 11.232 | 2015-07-01 | 2015-07-01 | `DTIMPORT` |
| `GE_Contato_ITA` | 10.866 | 2024-11-14 | 2024-12-27 | `DTAINCLUSAO` |
| `GE_Cidade` | 10.214 | 2017-01-22 | 2024-12-28 | `DtaAlteracao` |

## Comparação com o snapshot de 2025-06 (`vortice-crm-agent/schema/tabelas.csv`)

As 20 tabelas que mais cresceram entre o snapshot e agora:

| Tabela | Agora | Snapshot | Delta | % |
|---|---:|---:|---:|---:|
| `GE_Log2` | 4.591.737 | 4.394.779 | 196.958 | 4,5 |
| `GEP_JobAgdExecLog` | 1.092.904 | 981.319 | 111.585 | 11,4 |
| `IV_AgendaLog` | 11.143.876 | 11.049.475 | 94.401 | 0,9 |
| `GEP_SyncUsrSat` | 3.970.706 | 3.901.359 | 69.347 | 1,8 |
| `IMP_Titulo` | 892.131 | 823.952 | 68.179 | 8,3 |
| `IV_Historico` | 2.485.762 | 2.436.127 | 49.635 | 2 |
| `GE_LOG_PROCESSO` | 12.714.111 | 12.678.640 | 35.471 | 0,3 |
| `IV_Agenda` | 966.941 | 931.989 | 34.952 | 3,8 |
| `GE_LOG_HISTORICO` | 1.048.114 | 1.015.045 | 33.069 | 3,3 |
| `GE_LOG_TRANS` | 803.647 | 782.089 | 21.558 | 2,8 |
| `GE_LOG_PESSOA` | 961.628 | 943.712 | 17.916 | 1,9 |
| `X_T_IMP_CRM_TITULO` | 479.271 | 462.391 | 16.880 | 3,7 |
| `IV_ProcDado` | 1.532.670 | 1.516.214 | 16.456 | 1,1 |
| `IV_Processo` | 1.190.768 | 1.174.932 | 15.836 | 1,3 |
| `IV_ProcStatMonit` | 865.629 | 850.022 | 15.607 | 1,8 |
| `GEP_EMAILSENT` | 586.458 | 571.866 | 14.592 | 2,6 |
| `IV_ProcFaseMonit` | 666.855 | 655.144 | 11.711 | 1,8 |
| `DMN_DocHst` | 189.651 | 183.667 | 5.984 | 3,3 |
| `IV_STATUS_DEPTO` | 477.344 | 473.844 | 3.500 | 0,7 |
| `IV_ProcPerspMonit` | 129.453 | 126.145 | 3.308 | 2,6 |

