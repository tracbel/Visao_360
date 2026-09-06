# Lacunas do catalogo - o que so o acesso ao vivo resolve

> Snapshot de 03/06/2026. Gerado por [`gerar-dicionario.py`](gerar-dicionario.py) em 02/09/2026 23:50.

> **Atualizacao de 02/09/2026:** a extracao ao vivo em `../extracao-vortice/` ja preenche os itens 1 a 8 e 10 a 15 desta lista (codigo dos 548 objetos, indices, identity, extended properties, dominios reais, catalogos BPM, tamanho fisico, ultima atividade, permissoes). O unico item ainda aberto e o **9 (jobs do SQL Server Agent)** - depende do DBA. Os numeros de linhas deste dicionario continuam sendo os do snapshot de junho; os atuais estao em `../extracao-vortice/volumetria/`.

Este dicionario foi montado **inteiramente** a partir de `schema/*.csv`. Esses CSVs cobrem tabelas, colunas, tipos, nulabilidade, PKs e FKs - e mais nada. Tudo o que esta listado abaixo esta **ausente do catalogo** e precisa de uma nova extracao com o banco acessivel (VPN + conta de leitura).

[Voltar ao indice](00-INDICE.md)

## 1. Metadados ausentes no catalogo CSV

| # | O que falta | Por que importa | Como extrair (SQL Server) |
|---:|---|---|---|
| 1 | **Definicao (SQL) das views** | As colunas das 411 views **ja estao** no catalogo (ver [VIEWS.md](VIEWS.md)) - o que falta e o `SELECT`. As views sao a camada de relatorio real do produto (`VW_REL_*`, `BI_*`, `IV$P_*`); sem o SQL nao da para saber que tabelas cada relatorio le, que filtros aplica nem de onde vem cada coluna. | `SELECT v.name, m.definition FROM sys.views v JOIN sys.sql_modules m ON m.object_id = v.object_id` - `definition` volta **NULL** sem a permissao `VIEW DEFINITION` (REGRAS-DE-NEGOCIO 2.8). |
| 2 | **Stored procedures (6) e functions (1)** | Regras de negocio que rodam dentro do banco, invisiveis no catalogo de tabelas. | `SELECT o.type_desc, o.name, m.definition FROM sys.objects o JOIN sys.sql_modules m ON m.object_id = o.object_id WHERE o.type IN ('P','FN','IF','TF')` |
| 3 | **Triggers** | Efeitos colaterais em INSERT/UPDATE (geracao automatica de agenda, log, validacao). Explicam comportamento que nao esta em nenhuma tabela. | `SELECT t.name, OBJECT_NAME(t.parent_id) AS tabela, m.definition FROM sys.triggers t JOIN sys.sql_modules m ON m.object_id = t.object_id` |
| 4 | **Indices (clustered, nonclustered, unique, filtrados)** | Sem os indices nao da para escrever consulta performatica em `IV_Historico` (2,4M) ou `GE_LOG_PROCESSO` (12,7M), nem saber que colunas tem unicidade de fato. | `SELECT i.name, i.type_desc, i.is_unique, OBJECT_NAME(i.object_id), c.name FROM sys.indexes i JOIN sys.index_columns ic ON ... JOIN sys.columns c ON ...` (+ `sys.dm_db_index_usage_stats` para uso real) |
| 5 | **CHECK e DEFAULT constraints (com nome e expressao)** | A coluna `valor_default` do `colunas.csv` esta **100% vazia** nas 19.859 linhas: nenhum default foi capturado na extracao. Sem os CHECKs tambem nao se conhece o dominio validado das colunas `char(1)` (`Status`, `Natureza`, `Classe`, `Realizada`). | `SELECT * FROM sys.default_constraints` e `SELECT * FROM sys.check_constraints` |
| 6 | **Extended properties (MS_Description)** | Se a Vortice tiver documentado alguma tabela/coluna no proprio banco, esta aqui. O `SCHEMA_MAP.md` afirma que nao ha nenhuma - confirmar. | `SELECT * FROM sys.extended_properties WHERE name = 'MS_Description'` |
| 7 | **Colunas IDENTITY, COMPUTED e PERSISTED** | Define quais PKs sao auto-incremento (e portanto nao devem ser preenchidas por integracao) e quais colunas sao derivadas de outras. | `SELECT OBJECT_NAME(object_id), name, is_identity, is_computed FROM sys.columns` + `sys.computed_columns` |
| 8 | **Collation por coluna e do banco** | Determina se a comparacao de texto e case/accent sensitive - critico para JOIN por `CODVENDEDOR`, `CodUsuario` e demais chaves varchar. | `SELECT OBJECT_NAME(object_id), name, collation_name FROM sys.columns WHERE collation_name IS NOT NULL` + `SELECT DATABASEPROPERTYEX(DB_NAME(),'Collation')` |
| 9 | **Jobs do SQL Server Agent** | As cargas `IMP_*` / `X_TOTVS_*` e a sincronizacao com o ERP rodam por job. Sem isso nao se sabe a periodicidade nem por que o faturamento parou em abr/2025. | `SELECT j.name, s.step_name, s.command FROM msdb.dbo.sysjobs j JOIN msdb.dbo.sysjobsteps s ON s.job_id = j.job_id` + `msdb.dbo.sysjobhistory` |
| 10 | **Dominio real das colunas `char(1)`, `Status` e flags** | O `SCHEMA_MAP.md` avisa: `GE_Pessoa.Status` nao e so A/I - o real e P/A/S/F/O/I. Cada tabela tem seu proprio dominio, nao declarado em lugar nenhum. | `SELECT <coluna>, COUNT(*) FROM <tabela> WITH (NOLOCK) GROUP BY <coluna>` |
| 11 | **Conteudo dos catalogos de parametrizacao** | O significado de `IV_Acao`, `IV_Resultado`, `IV_ProcFase`, `IV_ProcResultado` e `IV_AcaoAuto` esta nas **linhas**, nao no schema. E o que traduz codigo em comportamento (fluxo 41 x 50, ordem das fases, quem recebe a proxima agenda). | `SELECT * FROM IV_Acao WITH (NOLOCK) WHERE EMUSO = 1`; idem para os demais |
| 12 | **Mapeamento `IV_Q_<X>` -> rotulos de `IV_Questao`** | As 175 tabelas de formulario materializado somam 2.601 colunas cujos nomes vem de `IV_Questao.Nomecoluna`. O rotulo legivel de cada coluna so existe nas linhas de `IV_Questao.Descricao`. | `SELECT f.Descricao AS formulario, q.Questao, q.Nomecoluna, q.Descricao, q.TipoDado FROM IV_Questao q JOIN IV_Formulario f ON f.SeqFormulario = q.SeqFormulario ORDER BY 1,2` |
| 13 | **Tamanho fisico real (paginas, MB, particionamento, compressao)** | O CSV traz contagem de linhas, nao espaco ocupado. Necessario para dimensionar migracao e backup. | `EXEC sp_spaceused '<tabela>'` ou `sys.dm_db_partition_stats` |
| 14 | **Freshness por tabela (data do dado mais recente)** | O `SCHEMA_MAP.md` documenta que o faturamento para em abr/2025. Isso precisa ser medido tabela a tabela, nao assumido. | `SELECT MAX(DtaInclusao), MAX(DtaAlteracao) FROM <tabela> WITH (NOLOCK)` |
| 15 | **Permissoes efetivas e roles do banco** | Explica por que `sys.sql_modules.definition` volta NULL para a conta de leitura - ausencia de resultado la e permissao, nao ausencia de codigo. | `SELECT * FROM sys.database_permissions` / `sys.database_role_members` |

## 2. As 411 views - colunas conhecidas, SQL desconhecido

Correcao importante em relacao ao que se supunha: `colunas.csv` cobre **1.178 objetos** - as 767 tabelas **e** as 411 views. Portanto a **assinatura** de cada view (colunas e tipos) esta documentada em [VIEWS.md](VIEWS.md). O que falta e o **`SELECT`** que define cada uma - e sem ele nao se sabe de qual tabela vem cada coluna, nem que filtro a view aplica. Como a camada de relatorio do Vortice vive nas views, este continua sendo o maior buraco do dicionario.

| Familia | Views | Provavel papel | Assinatura | SQL |
|---|---:|---|:---:|:---:|
| `(outras)*` | 218 | nao identificado pelo nome | ok | **falta** |
| `IV$*` | 140 | gerada por propriedade customizada (`IV_Propriedade`) | ok | **falta** |
| `BI_*` | 38 | consumo por BI / QlikView | ok | **falta** |
| `V_*` | 7 | view utilitaria | ok | **falta** |
| `X_*` | 4 | integracao TOTVS | ok | **falta** |
| `X_V_*` | 3 | materializacao/extracao da integracao TOTVS | ok | **falta** |
| `VW_REL*` | 1 | relatorio da aplicacao (`VW_REL_TBA*` - telas de relatorio) | ok | **falta** |
| **TOTAL** | **411** | | | |

<details><summary>Lista completa das 411 views</summary>

- `BI_AGENDA`
- `BI_AGENDA_CEN`
- `BI_AGENDA_PROSPECCAO`
- `BI_CARTEIRA_EQUIP`
- `BI_CARTEIRA_MANITOU`
- `BI_CARTEIRA_PUK`
- `BI_CARTEIRA_USADO`
- `BI_CARTEIRA_VN`
- `BI_COBERTURA_PROSPECCAO`
- `BI_COBERTURA_PROSPECCAO_MAQUINAS`
- `BI_DEPTO`
- `BI_DTAPROVCREDITO`
- `BI_EMPRESA`
- `BI_FROTA`
- `BI_GRUPO_USR`
- `BI_OPORTUNIDADE_ENC`
- `BI_OPORT_ABERTAS`
- `BI_OPORT_ABERTASRESUMO`
- `BI_OPORT_FUNIL`
- `BI_OPORT_FUNIL2`
- `BI_OPORT_FUNIL2_BKP`
- `BI_ORIGEM_RECEITA`
- `BI_PESSOA`
- `BI_PESSOAOP`
- `BI_QUALIDADE`
- `BI_USUARIO`
- `BI_VENDA_MAQ`
- `BI_VENDA_MAQ_ENC`
- `BI_VENDA_MAQ_NEW`
- `BI_VENDA_PERDIDA`
- `BI_VENDA_PERDIDA_PROD`
- `BI_VISITA`
- `GE$CONTATO_FULL`
- `GE$CONTATO_LGPD`
- `GE$EMAIL_FULL`
- `GE$EMAIL_LGPD`
- `GE$MODULOPERM`
- `GE$OBJDINAMICO`
- `GE$PESSOA`
- `GE$PESSOAENDCORR`
- `GE$PESSOAEND_FULL`
- `GE$PESSOAEND_LGPD`
- `GE$PESSOAFONE_FULL`
- `GE$PESSOAFONE_LGPD`
- `GE$PESSOA_FULL`
- `GE$PESSOA_LGPD`
- `GE$POLSEGPERM`
- `GE$USUARIOPERM`
- `GEL$CIDADE`
- `GEL$LOGRADOURO`
- `GEL$TIPOLOGRADOURO`
- `GEL$UF`
- `IMPV_VEICULO`
- `IV$ATIVREQUISITO`
- `IV$A_BENEFICIO`
- `IV$A_CAMPANHA_AGRISHOW_2012`
- `IV$A_COR_PREFERIDO`
- `IV$A_PERFIL_CLIENTE_SERVICOS`
- `IV$A_PERSONA_JOHN_DEERE`
- `IV$A_POTENCIAL_CARTEIRA_PECAS`
- `IV$A_PROJETO_CULTIVAR`
- `IV$A_TIME_PREFERIDO`
- `IV$DADOS_PESSOA_COMPLETA`
- `IV$DOITRES`
- `IV$FICHANEG`
- `IV$HISTORICO`
- `IV$NFITEM`
- `IV$NFSAIDA`
- `IV$NFSCMPL`
- `IV$OPERADOR`
- `IV$OPERGRUPO`
- `IV$OSItem`
- `IV$OSSolicitacao`
- `IV$OrdemServico`
- `IV$PESSOA_GEOSIGA`
- `IV$PG_AGRICULTURA_PRECISAO`
- `IV$PG_ALCADA`
- `IV$PG_AMS_TOTVS`
- `IV$PG_ANO`
- `IV$PG_AUDITORIA`
- `IV$PG_CAMPANHA_DE_INCENTIV`
- `IV$PG_CLASSE_RESULTADO`
- `IV$PG_COLHEDORA_CANA_TOTVS`
- `IV$PG_COLHEDORA_DE_CANA`
- `IV$PG_COLHEITADEIRA_ANO`
- `IV$PG_COLHEITADEIRA_GRAOS`
- `IV$PG_COLHEITADEIRA_TOTVS`
- `IV$PG_CONSORCIO`
- `IV$PG_CULTURA`
- `IV$PG_EQUIPAMENTOS`
- `IV$PG_FAMILIA_DEPTOS`
- `IV$PG_FENO_E_FORRAGEM`
- `IV$PG_GREEN_SYSTEM`
- `IV$PG_GRUPO_REGIONAL`
- `IV$PG_IMPLEMENTOS`
- `IV$PG_IMPLEMENTOS_TOTVS`
- `IV$PG_IMP_GREEN_SYSTEM`
- `IV$PG_INST_FINANCEIRA`
- `IV$PG_INTEGRACAO_FAMILIA`
- `IV$PG_MARCAS`
- `IV$PG_MARCAS_E_FAMILIA`
- `IV$PG_MARCA_TOTVS`
- `IV$PG_META_DEPTO`
- `IV$PG_ORIGEM_DA_RECEITA`
- `IV$PG_PLANTADEIRA`
- `IV$PG_PLANTADEIRA_TOTVS`
- `IV$PG_PLATAFORMA_ADICIONAL`
- `IV$PG_POLITICA_DE_COMISSAO`
- `IV$PG_PRODUTOSISDIA`
- `IV$PG_PULVERIZADOR`
- `IV$PG_PULVERIZADOR_TOTVS`
- `IV$PG_RESPONSAVEL_TECNICO`
- `IV$PG_RETAIL`
- `IV$PG_REVENDAS`
- `IV$PG_SEGURO`
- `IV$PG_SEM_INCENTIVO`
- `IV$PG_STATUS_PESSOA_DEPTO`
- `IV$PG_TIPO`
- `IV$PG_TRATORES`
- `IV$PG_TRATOR_TESTE`
- `IV$PG_TRATOR_TOTVS`
- `IV$PG_TURF`
- `IV$PG_VENDEDOR`
- `IV$PROCESSO`
- `IV$PRODUTO`
- `IV$P_AGRICULTURA_PRECISAO`
- `IV$P_CNAE`
- `IV$P_COLHEDORA_DE_CANA`
- `IV$P_COLHEITADEIRA_GRAOS`
- `IV$P_CONTRATO_GFC`
- `IV$P_DISTRIBUIDORA`
- `IV$P_EQUIPAMENTOS`
- `IV$P_EQUIPAMENTOS111`
- `IV$P_EQUIPAMENTOS_MANITOU`
- `IV$P_EQUIPAMENTOS_NOVO`
- `IV$P_EQUIPAMENTO_SISDIA`
- `IV$P_FENO_E_FORRAGEM`
- `IV$P_FROTA`
- `IV$P_IMPLEMENTO`
- `IV$P_LOCACAO`
- `IV$P_NJUR`
- `IV$P_NO_AGRICULTURA_PREC`
- `IV$P_NO_AREA_PLANTADA`
- `IV$P_NO_AREA_TOTAL`
- `IV$P_NO_CHACARA`
- `IV$P_NO_COLHEDORA_DE_CAN`
- `IV$P_NO_COLHEITADEIRA_GR`
- `IV$P_NO_CONSORCIO`
- `IV$P_NO_ESTANCIA`
- `IV$P_NO_FAZENDA`
- `IV$P_NO_FENO_E_FORRAGEM`
- `IV$P_NO_FROTA`
- `IV$P_NO_IMOVEL`
- `IV$P_NO_IMPLEMENTO`
- `IV$P_NO_IMPLEMENTOS`
- `IV$P_NO_JOHN_DEERE`
- `IV$P_NO_MAQUINA_IMPLEMENT`
- `IV$P_NO_ORIGEM_DA_RECEIT`
- `IV$P_NO_PLANTADEIRA`
- `IV$P_NO_PLATAFORMA_ADICI`
- `IV$P_NO_PROPRIEDADE_RURAL`
- `IV$P_NO_PULVERIZADOR`
- `IV$P_NO_SEGURO_DE_VIDA`
- `IV$P_NO_SITIO`
- `IV$P_NO_TIPO_CULTURA`
- `IV$P_NO_TRATOR`
- `IV$P_NO_TRATOR_MANUAL_FRO`
- `IV$P_NO_TRATOR_SISDIA`
- `IV$P_NO_TURF`
- `IV$P_NO_VEICULO`
- `IV$P_OPERATIONS_CENTER`
- `IV$P_ORIGEM_DA_RECEITA`
- `IV$P_PLANTADEIRA`
- `IV$P_PLATAFORMA_ADICIONAL`
- `IV$P_PULVERIZADOR`
- `IV$P_SEGURO`
- `IV$P_TRATOR`
- `IV$P_TURF`
- `IV$RESULTADO`
- `IV$RESULTADOFULL`
- `IV$S_AGENDA`
- `IV$S_CARTEIRA`
- `IV$S_CONTATO`
- `IV$S_ENDERECOADIC`
- `IV$S_HISTORICO`
- `IV$S_PESSOA`
- `IV$S_PESSOA_CLASSE`
- `IV$S_RFV`
- `IV$TITULO`
- `IV$TITULOEMPR`
- `IV$TITULOORIGEM`
- `IV$TITULOVENC`
- `IV$_PONTUACAO`
- `IV_EQUIPE`
- `IV_EQUIPEEMPR`
- `IV_Q$ABERTURA_OS_REVISAO`
- `IV_Q$ACOMPANHAMENTO_VENDA`
- `IV_Q$ACOMPANHAM_VENDA_IMP`
- `IV_Q$ACOMPANHA_COMPRA_IMP`
- `IV_Q$ACOMPANH_VENDA_JDE`
- `IV_Q$ACOMPAN_COMPRA_JDE`
- `IV_Q$ACOMPAN_VEND_CONCESS`
- `IV_Q$ACOMP_VENDA_DIRETA`
- `IV_Q$ACOMP_VENDA_DIRETAJD`
- `IV_Q$ACOMP_VENDA_FINANC`
- `IV_Q$ACOMP_VENDA_LOCACAO`
- `IV_Q$ACOMP_VENDA_MANITOU`
- `IV_Q$ACOMP_VENDA_USADO`
- `IV_Q$ACOMP_VEND_MAQUINAS`
- `IV_Q$ADM_FINANCEIRO`
- `IV_Q$ADM_FINANCEIRO_N`
- `IV_Q$AFERICAO_CSC`
- `IV_Q$AFERICAO_DE_PECAS`
- `IV_Q$AFERICAO_IMPLEMENTO`
- `IV_Q$AFERICAO_MAQ_1_CONT`
- `IV_Q$AFERICAO_MAQ_1_WEB`
- `IV_Q$AFERICAO_MAQ_2_CONT`
- `IV_Q$AFERICAO_MAQ_2_WEB`
- `IV_Q$AFERICAO_MAQ_3_CONT`
- `IV_Q$AFERICAO_MAQ_3_WEB`
- `IV_Q$AFERICAO_PECAS`
- `IV_Q$AFERICAO_POS_SOLUCAO`
- `IV_Q$AFERICAO_SERVICOS`
- `IV_Q$AFERICAO_SERV_WEB`
- `IV_Q$AFERICAO_SUPORTE_INT`
- `IV_Q$AFERICAO_VENDA_MAQ`
- `IV_Q$AFERICAO_VENDA_MQ_IM`
- `IV_Q$AFE_VENDA_MQ_IM_USAD`
- `IV_Q$AGUARDAR_PECAS`
- `IV_Q$ALTERADO_PAGAMENTO`
- `IV_Q$ANALISE_DE_CREDITO`
- `IV_Q$APRESENTACAO`
- `IV_Q$APRESENTACAO_JDE`
- `IV_Q$APRESENT_EQUIPAMENTO`
- `IV_Q$APRESENT_IMPLEMENTO`
- `IV_Q$APROVACAO_TCSM`
- `IV_Q$ATUALIZACAO_PUK`
- `IV_Q$AVALIACAO_AMS_USADO`
- `IV_Q$AVALIA_COLHEIT_USADA`
- `IV_Q$AVALIA_IMPLEM_USADO`
- `IV_Q$AVALIA_USADO_ENTRADA`
- `IV_Q$AVAL_COLH_CANA_USADA`
- `IV_Q$AVAL_TRATORES_USADOS`
- `IV_Q$AVAL_USADO_ENTRADA`
- `IV_Q$CADASTROS_LISTAS`
- `IV_Q$CANCELAMENTO_SEGURO`
- `IV_Q$CANCEL_RENOVACAO_SEG`
- `IV_Q$CANHOTO_DIGITAL`
- `IV_Q$CHASSI_ENTREGA_FISIC`
- `IV_Q$CHASSI_EQUIPAMENTO`
- `IV_Q$CHASSI_PMP`
- `IV_Q$CHEGADA_IMPLEMENTO`
- `IV_Q$COMISSAO`
- `IV_Q$COMISSAO_AMS`
- `IV_Q$COMISSAO_CONTACHAVE`
- `IV_Q$COMISSAO_LOCACAO`
- `IV_Q$COMISSAO_SERV_AMS`
- `IV_Q$COMISSAO_USADO`
- `IV_Q$COMISSAO_VD_LOCACAO`
- `IV_Q$COMPETIDORES_NA_NEG`
- `IV_Q$COMPETIDORES_NA_NEGO`
- `IV_Q$COMPETID_NEGOC_IMPL`
- `IV_Q$COM_INTERESSE_FUTURO`
- `IV_Q$CONDICOES_DE_VENDAS`
- `IV_Q$CONT_COMISSAO_22`
- `IV_Q$COTA_CONSORCIO`
- `IV_Q$DEMONSTRACAO_JD`
- `IV_Q$DEMONSTRACAO_LOG`
- `IV_Q$DEMONSTRACAO_NF`
- `IV_Q$DEMONSTRACAO_TRATOR`
- `IV_Q$DEMONSTR_COLHEITAD`
- `IV_Q$DEMO_EQUIP_JD`
- `IV_Q$DEMO_IMPLEMENTO`
- `IV_Q$DEMO_MAQUINAS`
- `IV_Q$DEMO_TRATOR`
- `IV_Q$DEVOLUCAO_PECA`
- `IV_Q$DEVOLUCAO_PUK`
- `IV_Q$DOC_ANALISE_CREDITO`
- `IV_Q$EVENTOS_AFERICAO`
- `IV_Q$EXP_FLUXO_MODELER`
- `IV_Q$FORA_SERVICO_PMP`
- `IV_Q$FORM_TREINO`
- `IV_Q$GAR_DATA_SERVICO`
- `IV_Q$GAR_FAB_SOL_PECA`
- `IV_Q$GESTAO_CREDITO`
- `IV_Q$GESTAO_CREDITO_AMS`
- `IV_Q$GESTAO_CREDITO_IMP`
- `IV_Q$GESTAO_PRODUTO_IMPL`
- `IV_Q$GESTAO_PRODUTO___AMS`
- `IV_Q$HORIMETRO_AGREGA`
- `IV_Q$INCENTIVO`
- `IV_Q$INTERESSE_FUTURO_PRO`
- `IV_Q$INTERESSE_PROJETO_IR`
- `IV_Q$LIBERAR_DEMONSTRACAO`
- `IV_Q$LICENCAS_PUK`
- `IV_Q$LOCACAO_COMISSAO`
- `IV_Q$OFERECE_RENOV_SEGURO`
- `IV_Q$ORIGEM_DA_RENDA`
- `IV_Q$OS_ABERTA`
- `IV_Q$OS_CORTESIA`
- `IV_Q$OS_GARANTIA`
- `IV_Q$OS_REVISAO_ENTREGA`
- `IV_Q$PECAS_AFERICAO`
- `IV_Q$PEDIDO_GC`
- `IV_Q$PEDIDO_KAM`
- `IV_Q$PEDIDO_SAM`
- `IV_Q$PERCEPCAO_JD`
- `IV_Q$PESQUISA_NPS`
- `IV_Q$PESQUISA_TI`
- `IV_Q$PESQ_SATISFACAO_PECA`
- `IV_Q$PREMIO_DEMO`
- `IV_Q$PREVISAO_RECEBIMENTO`
- `IV_Q$PRODUTO_RD`
- `IV_Q$PROPOSTA_COMERCIAL`
- `IV_Q$PROSPECCAO_SERV__JD`
- `IV_Q$QUALIDADE_PECAS`
- `IV_Q$QUALIDADE_SERVICOS`
- `IV_Q$QUALIDADE_VENDAMAQ`
- `IV_Q$RECEBIMENTO_A_PRAZO`
- `IV_Q$RECEBIMENTO_COMISSAO`
- `IV_Q$RECEBIMENTO_FINANC`
- `IV_Q$RECEB_FINAN_IMPLEM`
- `IV_Q$RESPONSAVEL_TECNICO`
- `IV_Q$RESULTADO_DEMO`
- `IV_Q$RETORNADO_JD`
- `IV_Q$REVISAO_100H`
- `IV_Q$REVISAO_1100_1150H`
- `IV_Q$REVISAO_1500H`
- `IV_Q$REVISAO_450_600H`
- `IV_Q$REVISAO_800H`
- `IV_Q$REVISAO_FIM_GARANTIA`
- `IV_Q$REV_DATA_SERVICO`
- `IV_Q$ROMANEIO_DEV_PECA`
- `IV_Q$SEPARACAO_PEDIDO`
- `IV_Q$SERVICOS_AFERICAO`
- `IV_Q$SERVICO_EXTERNOS_JD`
- `IV_Q$SOLICITACAO_TCAT`
- `IV_Q$TESTE1`
- `IV_Q$TESTE2`
- `IV_Q$TESTE_PRIMEIRO_JD`
- `IV_Q$TICKET_DSI`
- `IV_Q$VENDA`
- `IV_Q$VENDAPERDIDA_SEGURO`
- `IV_Q$VENDA_AMS`
- `IV_Q$VENDA_CONSORCIO`
- `IV_Q$VENDA_DIRETA`
- `IV_Q$VENDA_DSI`
- `IV_Q$VENDA_EQUIPAMENTO`
- `IV_Q$VENDA_LOCACAO`
- `IV_Q$VENDA_MAQUINA_FY25`
- `IV_Q$VENDA_N`
- `IV_Q$VENDA_PERDIDA`
- `IV_Q$VENDA_PERDIDA_FY25`
- `IV_Q$VENDA_PERDIDA_IMPL`
- `IV_Q$VENDA_PERDIDA_IMPLEM`
- `IV_Q$VENDA_PERDIDA_JDE`
- `IV_Q$VENDA_PERDIDA_MANITO`
- `IV_Q$VENDA_PERDIDA_MAQIMP`
- `IV_Q$VENDA_PERDIDA_PROD`
- `IV_Q$VENDA_PERDIDA_SEGURO`
- `IV_Q$VENDA_PERDIDA_TESTE`
- `IV_Q$VENDA_PNEUS`
- `IV_Q$VENDA_PRECISION_UP`
- `IV_Q$VENDA_SEMINOVO`
- `IV_Q$VENDA_SERVICOS_AMS`
- `IV_Q$VENDA_VP_PNEUS`
- `IV_Q$VENDER_RENOVACAO_SEG`
- `IV_Q$VISITA_DSI`
- `IV_Q$VISITA_EXP_CLIENTE`
- `IV_Q$VP_COLHEDORA`
- `IV_Q$VP_COLHEITADEIRA`
- `IV_Q$VP_PLANTADEIRA`
- `IV_Q$VP_PULVERIZADOR`
- `IV_Q$VP_RENOVACAO_SEGURO`
- `IV_Q$VP_SEM_PARTICIPACAO`
- `IV_Q$VP_TRATOR`
- `SYSCOLUMN`
- `SYSTABLE`
- `VBI$CONTA`
- `VBI$CONTAFAM`
- `VW_REL_TBA101`
- `V_C5SYSCOLUMN`
- `V_C5SYSCONSTRAINT`
- `V_C5SYSINDEX`
- `V_C5SYSPK`
- `V_C5SYSREFERENCE`
- `V_C5SYSTABLE`
- `V_UTIL_TELEFONES_CONTATOS`
- `X_CRM_BI_CONGLOMERADO`
- `X_CRM_BI_FATURAMENTO_PECA_CARTEIRA`
- `X_CRM_BI_FUNIL_PECA_CARTEIRA`
- `X_CRM_BI_FUNIL_PECA_CLASSE`
- `X_V_BI_DESPESAS_VENDA_MAQUINAS`
- `X_V_CRM_IMP_IMP_NFS`
- `X_V_CRM_IMP_NFSItem`
- `ZZ_IV$NFITEM`
- `ZZ_IV$NFSAIDA`
- `ZZ_IV$NFSCMPL`
- `ZZ_IV$OSItem`
- `ZZ_IV$OSSolicitacao`
- `ZZ_IV$OrdemServico`
- `bi_Oportunidades`
- `bi_cobertura`
- `bi_faturamentos`
- `bi_oportunidade`
- `bi_oportunidade_produto`
- `bi_pedidos`
- `iv_w3_conglomerado`
- `tba_clientes`
- `tba_usuarios`
- `teste_felipe`

</details>

## 3. Tabelas cuja funcao ficou `(nao documentado)`

Sao **48 tabelas** (de 767) para as quais nem os documentos-fonte nem o nome/colunas permitiram uma descricao honesta. Esta e a **fila de investigacao** com acesso ao vivo: para cada uma, rodar `SELECT TOP 20 * FROM <tabela> WITH (NOLOCK)` e localizar a tela do CRM que a alimenta.

| Modulo | Tabelas sem funcao |
|---|---:|
| `GE` | 16 |
| `OUTROS` | 7 |
| `IVF` | 6 |
| `IV-2` | 4 |
| `IVS` | 4 |
| `GEP` | 2 |
| `IV-4` | 2 |
| `TESTE` | 2 |
| `DMN` | 1 |
| `ESPACO` | 1 |
| `EXT` | 1 |
| `GEL` | 1 |
| `IV-3` | 1 |
| **TOTAL** | **48** |

Prioridade: **alta** = tem volume relevante (> 10 mil linhas) e portanto guarda dado de producao; **media** = tem linhas; **baixa** = vazia ou classificada como `lixo/backup`.

### Modulo `GE` - 16 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `GE_Membro_BKP20250520` | lixo/backup | 3 | 1.915 | baixa (lixo/backup) |
| `GE_Alias` | isolada | 9 | 1.413 | media |
| `GE_QVRegra` | isolada | 7 | 234 | media |
| `GE_ObjDinAplic` | isolada | 4 | 122 | media |
| `GE_Sequencia` | isolada | 2 | 77 | media |
| `GE_Tab` | catalogo | 2 | 12 | media |
| `GE_TempLong` | isolada | 3 | 6 | media |
| `GE_CFinConj` | vazia | 22 | 0 | baixa (vazia) |
| `GE_CFinFis` | vazia | 51 | 0 | baixa (vazia) |
| `GE_ColPosition` | vazia | 5 | 0 | baixa (vazia) |
| `GE_FCadConj` | vazia | 43 | 0 | baixa (vazia) |
| `GE_FCadF` | vazia | 97 | 0 | baixa (vazia) |
| `GE_FCadInstCred` | vazia | 13 | 0 | baixa (vazia) |
| `GE_FcadRBanc` | vazia | 18 | 0 | baixa (vazia) |
| `GE_FcadRCom` | vazia | 20 | 0 | baixa (vazia) |
| `GE_TabRegra` | vazia | 5 | 0 | baixa (vazia) |

### Modulo `OUTROS` - 7 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `teste333` | lixo/backup | 11 | 24.024 | baixa (lixo/backup) |
| `SYSCONVERT2` | lixo/backup | 2 | 24 | baixa (lixo/backup) |
| `SYSCONVERT3` | lixo/backup | 2 | 14 | baixa (lixo/backup) |
| `testepiv` | lixo/backup | 4 | 12 | baixa (lixo/backup) |
| `SYSCONVERT1` | lixo/backup | 2 | 3 | baixa (lixo/backup) |
| `sysdiagrams` | lixo/backup | 5 | 0 | baixa (lixo/backup) |
| `SYSDUMMY` | lixo/backup | 2 | 0 | baixa (lixo/backup) |

### Modulo `IVF` - 6 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `IVF_Agregado` | vazia | 8 | 0 | baixa (vazia) |
| `IVF_AgregCC` | vazia | 8 | 0 | baixa (vazia) |
| `IVF_Molicar` | vazia | 25 | 0 | baixa (vazia) |
| `IVF_Tabela` | vazia | 24 | 0 | baixa (vazia) |
| `IVF_TabelaFiltro` | vazia | 4 | 0 | baixa (vazia) |
| `IVF_TabIndice` | vazia | 6 | 0 | baixa (vazia) |

### Modulo `IV-2` - 4 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `IV_ClientePropr_ITA` | lixo/backup | 51 | 157.900 | baixa (lixo/backup) |
| `IV_ClientePropr_BKPJUN` | lixo/backup | 51 | 1.000 | baixa (lixo/backup) |
| `IV_PcteAtrFx` | nucleo | 3 | 22 | media |
| `IV_Pcte` | isolada | 3 | 1 | media |

### Modulo `IVS` - 4 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `IVS_TGLCLICLIENT` | isolada | 25 | 16.654 | **alta** (volume relevante) |
| `IVS_TGLCLIFIS` | isolada | 16 | 11.232 | **alta** (volume relevante) |
| `IVS_TGLCLIJUR` | isolada | 10 | 5.438 | media |
| `ivs_callcenter` | isolada | 2 | 3.475 | media |

### Modulo `GEP` - 2 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `gep_excseqcar` | isolada | 1 | 3.143 | media |
| `gep_excseqcarok` | isolada | 1 | 1.668 | media |

### Modulo `IV-4` - 2 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `IV_CFinDocAceito` | vazia | 4 | 0 | baixa (vazia) |
| `IV_ObjVenda` | vazia | 13 | 0 | baixa (vazia) |

### Modulo `TESTE` - 2 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `teste_acesso` | lixo/backup | 1 | 471 | baixa (lixo/backup) |
| `teste_fefa` | lixo/backup | 1 | 0 | baixa (lixo/backup) |

### Modulo `DMN` - 1 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `DMN_DocProp` | vazia | 4 | 0 | baixa (vazia) |

### Modulo `ESPACO` - 1 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `Espaco_Tabelas` | lixo/backup | 6 | 735 | baixa (lixo/backup) |

### Modulo `EXT` - 1 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `EXT_Pot_Pecas` | isolada | 13 | 1.287 | media |

### Modulo `GEL` - 1 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `GEL_ConvDe` | vazia | 4 | 0 | baixa (vazia) |

### Modulo `IV-3` - 1 tabelas

| Tabela | Classe | Colunas | Linhas | Prioridade |
|---|---|---:|---:|---|
| `IV_RecUso` | vazia | 7 | 0 | baixa (vazia) |

## 4. Roteiro para a proxima extracao

1. **Rodar a extracao ampliada** cobrindo os 15 itens da secao 1 e salvar os novos CSVs em `vortice-crm-agent/schema/` (mesmos nomes + novos arquivos: `views_def.csv`, `rotinas.csv`, `triggers.csv`, `indices.csv`, `constraints.csv`, `identity.csv`, `jobs.csv`, `dominios.csv`, `freshness.csv`).
2. **Extrair as linhas dos catalogos de parametrizacao** (`IV_Acao`, `IV_Resultado`, `IV_ProcFase`, `IV_ProcResultado`, `IV_AcaoAuto`, `IV_CodProcesso`, `IV_Formulario`, `IV_Questao`) - e ai que mora a regra de negocio do BPM.
3. **Amostrar as tabelas da secao 3** (`SELECT TOP 20`) para fechar as descricoes que hoje estao `(nao documentado)`.
4. **Medir freshness** por tabela e anexar ao dicionario, para nao repetir a surpresa do faturamento parado em abr/2025.
5. **Reexecutar** `python docs/dicionario/gerar-dicionario.py` - o gerador absorve os CSVs atualizados e reescreve todos os markdowns.

> Enquanto isso nao acontece: toda afirmacao deste dicionario que nao venha diretamente de `schema/*.csv` esta marcada como `(inferido)` ou citando a fonte entre parenteses. Numeros de linha sao do snapshot de 03/06/2026.
