# Integrações e dados de ERP no Vórtice CRM (EXT_, IMP_, X_, JDE_, GEP_) — superfície completa, padrão arquitetural, falhas medidas e desenho da anti-corruption layer para o CRM próprio

> Pesquisa automatizada - workflow `crm-tracbel-pesquisa-profunda`, 30/08/2026.

## Resumo

A integração do Vórtice com os ERPs tem TRÊS camadas independentes, nenhuma delas documentada: (1) uma ACL escrita pela própria Tracbel em T-SQL (procedures X_P_*, views X_V_CRM_IMP_*, linked server ODBC "TOTVS"/totvs6) que puxa Protheus → tabelas X_ → staging IMP_; (2) o motor do fornecedor (VCRM_RUNIMPORTIMP.EXE / RUNGEPIMPORT.EXE, agendados em GEP_JOBAGD) que promove IMP_ → EXT_ e resolve identidade via GE_PessoaLink; (3) ETL Pentaho (Kitchen/Pan em D:\VORTICE\PDI\*.LNK) que não roda desde jul/2021. O padrão é staging → merge por job, pull, delta por chave natural concatenada, com ledger de idempotência (X_INTEGRADO). Está gravemente quebrado e medido: 783.242 títulos (R$ 5,18 bi) presos em IMP_Titulo desde 22/05/2025 e nunca promovidos a EXT_Titulo; 280.214 de 287.868 OS em IMP_OS nunca chegaram a EXT_OS; o faturamento morreu em três datas distintas (FAT_SERVICOS 29/08/2024, FAT/DEV_MAQUINAS 07-08/02/2025, FAT/DEV_PECAS 11/04/2025); RUNGEPIMPORT falha a cada 20 min há meses com "Versão incompatível [4.04.01r05] x [4.04.01r01]" registrando apenas Warning; JDE_* está 100% vazio (o job JD_QUOTE existe no catálogo e nunca foi agendado). O achado estrutural mais grave: EXT_* (ERP) e IV_* (CRM/BPM) são duas ilhas ligadas SÓ por SeqPessoa — não há IdNFS/IDOS/idTitulo em IV_Historico nem em IV_ProcDado, e 99,1% das NFs não têm IdVeic; a ponte real é um formulário digitado à mão (IV_Q_ACOMP_VENDA_FINANC: chassi, nº NF, data fat.). Para o CRM novo isso define a prioridade: modelar o vínculo documento↔oportunidade como cidadão de primeira classe e construir a ACL com transação, watermark, DLQ e observabilidade — tudo o que falta aqui.

## Entidades / objetos mapeados

### `GE_PessoaLink`

**Funcao:** Crosswalk de identidade: mapeia chave externa de cada sistema de origem para o SeqPessoa interno. 33.320 linhas, 32.374 pessoas, 5 origens. Inclui a origem PES_JUNCAO (117 linhas) que preserva links após fusão de cadastros.

**Campos-chave:** SEQPESSOALINK (PK), Pessoalink varchar(250), Origem varchar(20) NOT NULL, SeqPessoa numeric(9) NOT NULL, DtaGeracao

**Relacoes:** SeqPessoa → GE_Pessoa (sem FK declarada); é o 2º nível da cascata de resolução de identidade

### `EXT_Pessoa`

**Funcao:** Espelho da pessoa no ERP (77 colunas, 58.171 linhas). Serve de 3º nível na resolução de identidade quando GE_PessoaLink não resolve. Único EXT_ atualizado em tempo real (DtaImport = hoje).

**Campos-chave:** IdPessoa (PK), PessoaLinkOrigem+Pessoalink (chave externa), SeqPessoa, NroEmpresa, StatusIMP, DtaGeracao, DtaImport, VLRLIMITECREDITO, SITUACAOCREDITO, DEPTO, CARTEIRA

**Relacoes:** SeqPessoa → GE_Pessoa (FK, declarada 2x); EXT_Titulo.IdPessoa e EXT_Pedido.IdPessoa → EXT_Pessoa.IdPessoa. 47 linhas sem SeqPessoa; até 8 espelhos para o mesmo SeqPessoa

### `IMP_Titulo / EXT_Titulo / EXT_TituloMov / EXT_TituloCmpl`

**Funcao:** Contas a receber. IMP_Titulo (42 col., 892.131 linhas) é o staging; EXT_Titulo (37 col., 577.925) o canônico; EXT_TituloMov (9 col., 1.134.039) os movimentos; EXT_TituloCmpl o texto livre. 783.242 linhas de staging nunca promovidas.

**Campos-chave:** idTitulo (PK), Origem (Protheus|SISDIA), NroEmpresa, PessoaLinkOrigem+Pessoalink, SeqPessoa, Especie, NroTitulo, NroDocto, IndAtivo/IndQuitado/IndCobrJuridica, Status, DtaEmissao/DtaVenctoOrig/DtaVencto/DtaQuitacao, VlrOriginal/VlrPago/VlrAberto, CODBARRAS, CHAVESTR1, LinkNro/LinkStr, STATUSIMP, dtaimport

**Relacoes:** EXT_Titulo.SeqPessoa → GE_Pessoa; EXT_Titulo.IdPessoa → EXT_Pessoa; EXT_TituloMov.idTitulo → EXT_Titulo; IMP_Titulo NÃO tem FK alguma

### `IMP_NFS/IMP_NFSItem → EXT_NFS/EXT_NFSItem/EXT_NFSCmpl/EXT_NFSOper/EXT_NFSOperEmp`

**Funcao:** Faturamento (nota fiscal de saída). EXT_NFS 39 col./390.755 linhas; EXT_NFSItem 27 col./1.300.126; EXT_NFSOper 9 col./241 (catálogo de operação fiscal); EXT_NFSOperEmp 330 (operação por empresa); EXT_NFSCmpl 0.

**Campos-chave:** IdNFS (PK), Origem, NroEmpresa, NroNF+SerieNF, SeqPessoa, IdVeic (99,1% NULL), IdNFSOper, IdVendedor, SeqDepto, CFOP, TipoVenda/CanalVenda/Segmento/Setor, FormaPgto/CondicaoPgto, DtaPedido/NroPedido, DtaEmissaoNF, Situacao (N/C/D), IndEstorno (sempre 0), IDNFSEXTERNO, PAIIDNFSEXTERNO; item: IdNFS+IdItem, IdProduto, Qtde, VlrLiqItem, VLRICMSSUBS/VLRPIS/VLRCOFINS

**Relacoes:** EXT_NFS→GE_Pessoa, EXT_Veic, EXT_Vendedor, EXT_NFSOper; EXT_NFSItem→EXT_NFS, EXT_Produto, EXT_Vendedor; EXT_FormaPgto.IdNFS→EXT_NFS

### `IMP_OS/IMP_OSITEM/IMP_OSSOLIC → EXT_OS/EXT_OSITEM/EXT_OSSolic`

**Funcao:** Ordem de serviço (pós-venda/oficina). IMP_OS 46 col./287.868 linhas vs EXT_OS 40 col./8.099 — 280.214 nunca promovidas. Congelado em 25/05/2024.

**Campos-chave:** IDOS (PK), Nroos+SerieOS, SeqPessoa, IdVeic, Nrochassi, Placa, Consultor, TipoOS/CODTIPOOS/TIPOSERVICO, Dtaabertura/Dtaencerramento/Dtafechamento, VlrLiqPecas/VlrLiqServicos, Kilometragem, IDOSEXTERNO, SITUACAO, DTAALTERACAOERP, StatusIMP

**Relacoes:** EXT_OS→GE_Pessoa, EXT_Veic; EXT_OSSolic.IDOS→EXT_OS; EXT_VeicKM.IDOS→EXT_OS; EXT_OSITEM sem FK para EXT_OS

### `EXT_Veic + hierarquia (VeicMarca→VeicFam→VeicModelo→Veic) + manutenção (VeicTipoMan/VeicPlanoMan/VeicModPlano/VeicPlanoMnFX) + VeicProp/VeicKM`

**Funcao:** Equipamento/máquina e frota do cliente. EXT_Veic 8.020 linhas; EXT_VeicModelo 4.431.168 (!); EXT_VeicPlanoMan 3.571.257; EXT_VeicModPlano 3.571.235; EXT_VeicProp 12.621 (histórico de propriedade); EXT_VeicKM 17.378 (leituras de horímetro/km).

**Campos-chave:** IdVeic (PK), IdVeicMarca, IdVeicModelo, SeqPessoa, SeqPlanoMAN, Chassi/ChassiRed/Placa/NroMOTOR, KMAtual/DtaKMAtual/KMMedia, AnoModelo/AnoFabric, DTAPRIMVENDA; VeicProp: IdVeicProp, IdVeic, SeqPessoa, IndPropAtual, Dtavenda, NroNF+SerieNF, Financiador, CanalVenda

**Relacoes:** EXT_Veic→EXT_VeicMarca/EXT_VeicModelo/GE_Pessoa/EXT_VeicPlanoMan; EXT_VeicModelo→EXT_VeicFam→EXT_VeicMarca; EXT_VeicFam.SeqPropriedade→IV_Propriedade (ponte com o CRM); EXT_VeicKM→EXT_VeicProp/EXT_OS/EXT_VeicPlanoMnFX. FKs duplicadas até 9x no catálogo

### `EXT_Pedido / EXT_PedidoItem / IMP_Pedido / IMP_PedidoItem`

**Funcao:** Pedido de venda — a ÚNICA cadeia modelada para saída CRM→ERP e a única EXT_ com coluna PROCESSO. Todas com 0 linhas: nunca usada na Tracbel.

**Campos-chave:** IdPedido (PK), Origem, PessoaLinkOrigem+Pessoalink, NroPedido, NroPedidoERP, IndEnvioERP, DtaEnvioERP, PedidoLinkNro/PedidoLinkStr/PEDIDOLINKORIGEM, PAILINKORIGEM/PAILINKNRO/PAILINKSTR, PROCESSO, Status/STATUSCMPL, DTAALTERACAOERP

**Relacoes:** EXT_Pedido→GE_Pessoa, EXT_Pessoa; EXT_PedidoItem→EXT_Pedido, EXT_Produto; PROCESSO aponta para IV_Processo (sem FK)

### `EXT_Produto / IMP_PRODUTO / EXT_Vendedor`

**Funcao:** Catálogos vindos do ERP. EXT_Produto 27 col./78.749 linhas (Protheus 35.027, SISDIA 27.859, 15.860 sem origem); EXT_Vendedor 6 col./563 (SISDIA 347, sem origem 188, Protheus 28); IMP_PRODUTO 0 linhas.

**Campos-chave:** IdProduto (PK), CodProduto, CHAVEPRODUTOERP, Marca, CodFamilia/Familia, CATEGORIA1..6, PrecoPublico/Preco1/Preco2, QTDEESTOQUE/CUSTOESTOQUE/DTAESTOQUE, EmUso, ORIGEM; Vendedor: IdVendedor (PK), CodVendedor, NroEmpresa, NROCPF, ORIGEM

**Relacoes:** EXT_NFSItem.IdProduto e EXT_PedidoItem.IdProduto → EXT_Produto; EXT_NFS/EXT_NFSItem/EXT_VeicProp .IdVendedor → EXT_Vendedor

### `X_TOTVS_CRM_FATURAMENTO`

**Funcao:** Tabela de POUSO (landing) do faturamento vindo do Protheus via linked server. 41 colunas, 809.821 linhas, segmentada em 5 streams pela coluna ORIGEM. Última NF 11/04/2025.

**Campos-chave:** ID (PK identity), ORIGEM (FAT_PECAS|FAT_SERVICOS|FAT_MAQUINAS|DEV_PECAS|DEV_MAQUINAS), FILIAL, DATA_EMISSAO_NF, NRO_NF, NRO_ITEM, SERIE, COD_CLIENTE, CPF_CNPJ, COD_MARCA, ID_VENDEDOR, COD_OPERACAO, CFOP, PRODUTO, NCM, NRO_PEDIDO, STATUS_NF, DELETADO, QUANTIDADE, VLR_LIQUIDO_ITEM, VLR_ICMS/PIS/COFINS, VALOR_MARGEM

**Relacoes:** Alimentada por X_TOTVS_ATUA_CRM_FATURAMENTO; consumida pelas views X_V_CRM_IMP_IMP_NFS e X_V_CRM_IMP_NFSItem; sem FK

### `X_V_IMP_CRM_IMP_NF`

**Funcao:** LEDGER DE IDEMPOTÊNCIA do faturamento (apesar do nome começar com X_V_, é TABELA, criada dinamicamente pela procedure). 815.854 linhas, todas X_INTEGRADO='INTEGRADO'. Registra quais chaves já foram empurradas para IMP_NFS/IMP_NFSItem.

**Campos-chave:** ID (PK identity), COD_CHAVE_ITEM varchar(150), COD_CHAVE_NF varchar(100), ORIGEM varchar(12), X_INTEGRADO varchar(20), X_DATA_INTEGRACAO datetime. 4 índices, todos liderados por ID (inúteis)

**Relacoes:** Nenhuma FK. Chave = string concatenada por pipe a partir de X_TOTVS_CRM_FATURAMENTO. Collation misturada na mesma tabela

### `X_T_IMP_CRM_TITULO`

**Funcao:** Tabela de ESTADO (último valor conhecido) dos títulos do Protheus, usada para calcular o delta INCLUIR/ATUALIZAR. 41 col., 462.391 linhas no snapshot / 479.271 medidas, 100% INTEGRADO até 13/07/2026 — o lado Tracbel está vivo.

**Campos-chave:** Obs (usado como chave de negócio!), Origem, PessoaLinkOrigem+Pessoalink, SeqPessoa, NroTitulo, DtaEmissao/DtaVencto, VlrOriginal/VlrPago, DATA_INCLUSAO, DATA_ALTERACAO, X_INTEGRADO, X_DATA_INTEGRACAO

**Relacoes:** Espelha TOTVS.TMPRD.dbo.X_V_IMP_CRM_TITULO; alimenta IMP_Titulo. Sem FK. Existe backup X_T_IMP_CRM_TITULO_bkp_11_04 (121.381) em produção

### `X_V_CRM_IMP_IMP_NFS / X_V_CRM_IMP_NFSItem`

**Funcao:** Views-ADAPTADOR: a anti-corruption layer real, escrita em SQL. Traduzem o dicionário Protheus (F2_DOC, F2_SERIE, D2_TES, A3_NOME, E4_COND, B1_POSIPI) para o contrato IMP_NFS/IMP_NFSItem do Vórtice. Criadas em 12/09/2023.

**Campos-chave:** Produzem COD_CHAVE_NF / COD_CHAVE_ITEM; mapeiam ORIGEM→Departamento (FAT_PECAS→'MAQ-PEÇAS', FAT_SERVICOS→'MAQ-SERV', FAT_MAQUINAS→'MAQ-NOVOS', else 'OUTRO'); literais fixos ('AUTOMOVEIS' as Segmento, 'Normal' as TipoVenda/CanalVenda, 'Protheus' as Origem); contrabandeiam o chassi via CONCAT('#CHASSI:',...) no campo Obs

**Relacoes:** Leem X_TOTVS_CRM_FATURAMENTO; escritas por X_P_IMP_CRM_NF_POS_VENDA em IMP_NFS/IMP_NFSItem

### `JDE_EQUIPAMENTS / JDE_QUOTE / JDE_QUOTE_ITEM / JDE_SALES_PERSON / JDE_CATEGORY / JDE_SUB_CATEGORY / JDE_PURCHASE`

**Funcao:** Integração John Deere / JD Edwards. Modelo relacional completo e coerente, TODAS as 7 tabelas com 0 linhas. Job JD_QUOTE existe no catálogo e nunca foi agendado.

**Campos-chave:** EQUIPAMENT_ID, serialNumber, makeName/modelName, listPrice/costPrice, machineHours; quoteId, deereUserId, quoteStatus/quoteType, netCost/netProceeds, poNumber, SEQPESSOA, signDate; poNumber, warrantyBeginsDate, deliveredDate, SeqQuestionario

**Relacoes:** JDE_EQUIPAMENTS→JDE_CATEGORY/JDE_SUB_CATEGORY; JDE_QUOTE→JDE_SALES_PERSON; JDE_QUOTE_ITEM→JDE_QUOTE/JDE_EQUIPAMENTS; JDE_PURCHASE.SeqQuestionario→IV_Questionario (único elo real com o CRM); JDE_QUOTE.SEQPESSOA SEM FK

### `GEP_JOBCAD / GEP_JOBAGD / GEP_JobAgdExecLog / GEP_JOBFILA / GEP_JOBFILALOG / GEP_JobMonitor / GEP_JOBNOTIFICAR`

**Funcao:** Agendador de jobs do Vórtice. Catálogo (71) × instância agendada (25) × fila sob demanda (23) × log (981.319) × monitor × matriz de notificação. Só 16 dos 71 jobs do catálogo estão agendados; vários agendados estão mortos.

**Campos-chave:** JOBCAD: SeqJOB, Codigo, Tipo, TIPOJOB, Argumento, ESTIMULOFILA, OBJETIVO. JOBAGD: SeqJOBAgd, Tipo (CICLICA/INTERNA), Aplicacao, IntervaloExec, UltimaExecucao, NroTENTATIVA, PlanoExec, ExecucaoExclusiva, SEQJOB. ExecLog: DtaExecucao, Codigo, TipoLog (N/W/E/A), TipoErro, Descricao, LINKSTR/LINKNRO1/LINKNRO2

**Relacoes:** GEP_JOBAGD.SEQJOB→GEP_JOBCAD; GEP_JOBFILA.SeqJobAgd→GEP_JOBAGD; GEP_JOBNOTIFICAR.SeqJOBAgd→GEP_JOBAGD; IV_RESJOB.SEQJOB→GEP_JOBCAD (resultado do CRM dispara job)

### `GEP_ParRecebe / GEP_ParEnvia / GEP_ParRecEnvia`

**Funcao:** Registro e parametrização de sistemas de origem (14) e destino (3) da integração, com flags de comportamento do merge por origem. GEP_ParRecEnvia (matriz origem×destino) está vazia.

**Campos-chave:** Recebe: Origem (PK), IndAlteraPessoa, IndIdentPesCNPJ, IndSobrepoeFone, IndEnviaOut, DestinoOut, QtdDiasRemove, INDALTERAPESSOAEXT, INDMANTEMORDEMFONES, INDMANTEMATIVO. Envia: Destino (PK), SessaoCnx, IndEnvPessoa, IndEnvProspect, INDENVTODAS

**Relacoes:** GEP_ParRecEnvia.Origem→GEP_ParRecebe, .Destino→GEP_ParEnvia; GE_PessoaDestino.Destino→GEP_ParEnvia. Nenhuma coluna Origem das tabelas de dado tem FK para GEP_ParRecebe

### `GEP_Import / GEP_ImportAprovacao / GEP_ImportTry / GEP_ImportErro / GEP_ProcImport`

**Funcao:** Fila genérica de importação linha-a-linha (formato EAV: lista de colunas + lista de dados separadas por um caractere). Usada por RD Station e MOBILELITE. GEP_ImportAprovacao é a fila de quarentena/aprovação (4.861 linhas CRMTRAC nunca processadas).

**Campos-chave:** Seq (PK), Processo, Origem, Acao (I), Banco, Dono, Tabela, Separador, Status (E/X/NULL), Coluna varchar(1000), Dado varchar(2000), ColunaIdentific, DadoIdentificador, DtaGeracao, Prioridade

**Relacoes:** Drenada por VCRM_RUNGEPIMPORT.EXE (que está falhando por versão). Sem FK. Existe GEP_Import_bkpjun com 502.009 linhas

### `GEP_SyncUsrSat / GEP_UsrSat`

**Funcao:** Outbox de replicação para satélites móveis (Vórtico Mobile Lite). 3.901.359 linhas para 65 usuários ativos; ~60 tabelas replicadas com operação I/A/E por registro por usuário.

**Campos-chave:** SEQSYNCUSRSAT (PK), Destino, Usr, Tabela, KN1/KN2 numeric(18), KS1/KS2 varchar(50) (chave genérica), Operacao char(1), IndProcessado (0/1/2/9), IndProcessoWait, NivelSeguranca, DtaGeracao, DtaColeta, FCMTOKEN. UsrSat: Destino+Usr (PK), IDSatelite, DtaUltimoEnvio, VersaoApp, SeqMobileConfig

**Relacoes:** GEP_UsrSat.SeqMobileConfig→GE_MobileConfig; alimentado por SYNCSAT_PRODUTOS_GERA_AUTO. Destino JUNO morto desde 17/03/2025

### `GEP_EMailSend / GEP_EMAILSENT (+ REL)`

**Funcao:** Fila e arquivo de e-mail transacional. 563.426 enviados + 22.512 falhas permanentes sem retry. Contexto WORKFLOW/MESSAGECENTER vivo; COBRANCA/AVISO parado desde 22/03/2023.

**Campos-chave:** SEQEMAILSEND/SEQEMAILSENT (PK), De/Para/Cc/Bcc, Assunto, Mensagem text, Solicitante, NroEmpresa, Status varchar(1), MSGERRO, DtaGeracao/DtaEnviar/DTAENVIO, SEQCONTAEMAIL, CONTEXTO, SUBCONTEXTO, ORIGEM char(3), CHAVE

**Relacoes:** SEQCONTAEMAIL→GE_ParamLista; GEP_EMAILSENDREL.SEQEMAILSEND→GEP_EMailSend e .SEQCONSSQL→GE_CONSSQL; GEP_EMAILSENTREL.SEQEMAILSENT→GEP_EMAILSENT

### `IMP_REL_TBA101 / IMP_REL_TBA101_PG2`

**Funcao:** Staging de relatório (Análise da Carteira de Pedidos FY25), discriminado por codusuario. PG2 acumula sem DELETE: 1.558 linhas para muito menos processos (V:AMAURY 208 linhas / 13 processos).

**Campos-chave:** codusuario, EMPRESA, CEN, JD_QUOTE, TIPO_VENDA, TIPO_DE_EQUIPAMENTO, MARCA, MODELO, SEQPESSOA, CNPJCPF, LINHA_CREDITO, INST_FINANCEIRA, FINANC_CHASSI, ULT_RESULTADO, DTA_FATURAMENTO_D, NF_N, VALOR_N, PROCESSO_N, PROCESSODNA, CARTEIRA, CAMPANHA, COMAR (decimal, precisa virar varchar)

**Relacoes:** Gerada por GE_QVCons SeqCons 170/171 e pela procedure VTC_Gera_REL_TBA101; lê IV_ProcDado, IVS_Pes, IVS_Carteira, IV_Q_ACOMP_VENDA_FINANC

### `IV_Q_ACOMP_VENDA_FINANC`

**Funcao:** A PONTE MANUAL entre o CRM e o ERP: formulário onde o vendedor DIGITA chassi, número da NF, data de faturamento, data de entrega, filial, proposta JD e COMAR. É a única fonte de 'esta oportunidade virou esta nota'.

**Campos-chave:** SEQQUESTIONARIO (PK), VENDA_FINANC_CHASSI varchar(50), VENDA_FINANC_NRO_NF varchar(30), VENDA_FINANC_DATAFAT, VENDA_FINANC_DATAENT, VENDA_FINANC_FILIAL, NUMERO_PROPOSTA, DATA_PROPOSTA, COMAR decimal(5), DATA_PREVISAO_FAT, COMAR_ varchar(30)

**Relacoes:** SEQQUESTIONARIO→IV_Questionario→IV_Processo. COMAR e COMAR_ coexistem sem migração: 132 registros só no antigo, 125 só no novo (invisíveis no relatório), 27 em ambos

### `EXT_APROVACAO / EXT_AGUARDENTREGA / EXT_CONDPAGTO`

**Funcao:** Apesar do prefixo EXT_, NÃO vêm de ERP: são tabelas de controle escritas pelas procedures locais VTC_P_GERAAPROVACAO / VTC_P_GERAAGUARDENTREGA / VTC_P_GERACONDPAGTO, que rodam a cada 5 minutos e estão entre os poucos jobs vivos (30/08/2026).

**Campos-chave:** PROCESSO numeric(18) (PK), DTAGERACAO, processodna. Volumes: 2.655 / 52 / 22

**Relacoes:** PROCESSO→IV_Processo (sem FK); geram agendas de aprovação, aguardando entrega e condição de pagamento no BPM

### `Procedures X_P_* e PR_INT_* / PR_VTC_INT*`

**Funcao:** A camada de código da integração. X_P_EXE_INTEGRACOES_TOTVS_CRM é o orquestrador (chama X_P_IMP_CRM_TITULO e X_P_IMP_CRM_NF_POS_VENDA). PR_INT_NFS/PR_INT_OS/PR_INT_OSAB/PR_INT_PROPRNFS/PR_INTSPR_* são do fornecedor (2012-2022). PR_VTC_INTMARCA/INTFAMILIA/INTMODELO/INT_VEIC/INTVEICULO/INTPROPRIEDADE são de 31/10/2025 (tentativa recente de reativar veículos).

**Campos-chave:** Nenhuma usa transação explícita, TRY/CATCH, watermark persistido ou log estruturado. X_TOTVS_ATUA_CRM_FATURAMENTO (mod. 27/05/2024) atualiza a landing via linked server.

**Relacoes:** Linked server TOTVS (product=totvs6, provider=MSDASQL/ODBC) → TOTVS.TMPRD.dbo.X_V_IMP_CRM_TITULO e X_V_BI_FATURAMENTO_PECAS

## Achados

### 1. O padrão real é pull + staging + merge por job, em 3 saltos, com ledger de idempotência (e não API)

A cadeia canônica do faturamento é: Protheus (linked server SQL Server 'TOTVS', product=totvs6, provider=MSDASQL/ODBC) → view remota TOTVS.TMPRD.dbo.X_V_IMP_CRM_TITULO / X_V_BI_FATURAMENTO_PECAS → tabela de pouso X_TOTVS_CRM_FATURAMENTO (41 col., 809.821 linhas) → views-adaptador X_V_CRM_IMP_IMP_NFS e X_V_CRM_IMP_NFSItem (traduzem o dicionário Protheus F2_DOC/D2_TES/A3_NOME/E4_COND para o contrato IMP_ do Vórtice) → staging IMP_NFS/IMP_NFSItem → tabelas canônicas EXT_NFS/EXT_NFSItem → CRM. Os saltos 1-4 são código da Tracbel (procedures X_P_EXE_INTEGRACOES_TOTVS_CRM → X_P_IMP_CRM_TITULO + X_P_IMP_CRM_NF_POS_VENDA, autor Felipe Augusto Violin, deploy 20/03/2023 e 20/09/2023). O salto IMP_→EXT_ é executável do fornecedor (C:\VORTICE\TDEV\APPSRV\VCRM_RUNIMPORTIMP.EXE, agendado em GEP_JOBAGD a cada 5 min). A idempotência vive em tabelas-ledger separadas: X_V_IMP_CRM_IMP_NF (COD_CHAVE_ITEM, COD_CHAVE_NF, ORIGEM, X_INTEGRADO, X_DATA_INTEGRACAO) e X_T_IMP_CRM_TITULO (41 col., espelho do último estado conhecido + X_INTEGRADO). A API REST wsVorticeCrmApi tem endpoints /api/crm/imp/* (pessoa, nfs, nfsitem, os, ositem, titulo, veiculo, pedido, produto, evento) que escrevem NAS MESMAS tabelas IMP_, mas NÃO são o caminho usado pelo ERP — IMP_PESSOA, IMP_VEICULO, IMP_PRODUTO, IMP_Evento, IMP_Pedido e IMP_PedidoItem estão com 0 linhas.

**Evidencia:** sys.servers (is_linked=1) → TOTVS/totvs6/MSDASQL; sys.sql_modules de X_P_EXE_INTEGRACOES_TOTVS_CRM, X_P_IMP_CRM_TITULO, X_P_IMP_CRM_NF_POS_VENDA, X_V_CRM_IMP_IMP_NFS, X_V_CRM_IMP_NFSItem; GEP_JOBAGD SeqJOBAgd=4 (VCRM_RUNIMPORTIMP.EXE, IntervaloExec=5); contagens SELECT COUNT(*) das IMP_*

**Licao para a Tracbel:** COPIAR o esqueleto de 3 estágios (landing bruto → adaptador → canônico) e o ledger de idempotência com chave natural + flag/data de integração; é o desenho certo. ADAPTAR: no CRM novo o adaptador não deve ser uma VIEW SQL com literais hardcoded, e sim um mapper versionado em código (com testes), e o transporte deve ser fila/evento (outbox no ERP ou CDC) em vez de linked server ODBC. EVITAR: expor endpoints de importação que gravam direto no staging sem que ninguém os use — vira superfície de ataque e confunde o diagnóstico.

### 2. A chave de correlação com o ERP é o par (PessoaLinkOrigem, Pessoalink) e a resolução de identidade tem cascata de 4 níveis documentada no próprio log de erro

Toda tabela de entrada carrega PessoaLinkOrigem varchar(20) + Pessoalink varchar(250) — presente em EXT_Pessoa, IMP_PESSOA, IMP_Titulo, IMP_OS, IMP_OSITEM, IMP_NFS, IMP_Pedido, IMP_VEICULO, IMP_Evento, IMP_EMAIL, X_T_IMP_CRM_TITULO. O Pessoalink é POLIMÓRFICO por origem: para Origem='RD' é o e-mail do lead (ex.: 'philippe_xbm@yahoo.com.br'); para 'Protheus' é o código de cliente do ERP; para 'INTEGRAÇÃONOROESTE' é o código do sistema legado. A resolução para SeqPessoa segue a ordem exata que o próprio job imprime: (1) SeqPessoa informado; (2) PessoaLink+Origem em GE_PESSOALINK; (3) PessoaLink+Origem em EXT_PESSOA; (4) CPF/CNPJ (só se GEP_ParRecebe.IndIdentPesCNPJ=1 para aquela origem). GE_PessoaLink é a tabela de crosswalk: (Pessoalink varchar(250), Origem varchar(20) NOT NULL, SeqPessoa numeric(9) NOT NULL, DtaGeracao, SEQPESSOALINK PK) — 33.320 linhas, 32.374 pessoas, 5 origens (INTEGRAÇÃONOROESTE 28.037, Protheus 4.063, RD 883, EEMOVEL 220, PES_JUNCAO 117). A origem 'PES_JUNCAO' é o registro de FUSÃO de cadastros: quando duas pessoas são unificadas, o link antigo é preservado apontando para o SeqPessoa sobrevivente (117 links, 90 pessoas).

**Evidencia:** GEP_JobAgdExecLog, Descricao completa: 'Advertencia: Pessoa não localizada: / PessoaLink + Origem na GE_PESSOALINK / PessoaLink + Origem na EXT_PESSOA , parametros de busca SeqPessoa: 0, PessoaLink: philippe_xbm@yahoo.com.br, PessoaLinkOrigem: RD, CPF/CNPJ: 0' (30/08/2026 08:05:50); sys.columns de GE_PessoaLink; GROUP BY Origem em GE_PessoaLink

**Licao para a Tracbel:** COPIAR integralmente: tabela de crosswalk external_id (system, external_key, internal_id) separada da entidade, cascata de matching explícita e configurável, e — crucialmente — a preservação do link após merge de cadastros (PES_JUNCAO). É o melhor pedaço de design do sistema inteiro. ADAPTAR: tipar a chave externa por sistema (não deixar um varchar(250) polimórfico), guardar o score/regra que produziu o match e a data, e nunca deixar o CPF/CNPJ como fallback silencioso — deve gerar candidato para revisão humana, não vínculo automático.

### 3. 783.242 títulos (R$ 5,18 bilhões) estão presos em IMP_Titulo desde 22/05/2025 e nunca chegaram a EXT_Titulo — a visão financeira do CRM está cega há 15 meses

IMP_Titulo tem 892.131 linhas. StatusIMP='E' (processado) em 108.889 linhas, com dtaimport de 17/10/2023 a 21/05/2025. StatusIMP vazio/NULL em 783.242 linhas, com dtaimport de 22/05/2025 a 13/07/2026 — TODAS Origem='Protheus'. Verificação direta: as 783.242 linhas pendentes NÃO existem em EXT_Titulo (LEFT JOIN por idTitulo, zero casamentos). Valor represado por ano de emissão: 2025 → 474.167 títulos / R$ 3.300.555.098,76; 2026 → 295.127 títulos / R$ 1.789.550.202,67; 2024 → 12.337 / R$ 82,7 mi. Enquanto isso EXT_Titulo (577.925 linhas) tem MAX(DtaEmissao)=21/05/2025 e o CRM enxerga apenas 12.915 títulos em aberto somando R$ 103.425.024,01. O lado Tracbel da esteira está VIVO (X_T_IMP_CRM_TITULO: 479.271 linhas, 100% X_INTEGRADO='INTEGRADO', até 13/07/2026); quem parou foi o consumidor do fornecedor. O job COBRANCA_CRIT (GEP_JOBCAD SeqJOB=38, 'Realiza o processamento dos critérios de cobrança') tem UltimaExecucao=21/05/2025 16:30:01 — exatamente a data de congelamento. O e-mail de cobrança (GEP_EMAILSENT CONTEXTO='COBRANCA', SUBCONTEXTO='AVISO', ORIGEM='TIT') parou em 22/03/2023 com 65.165 envios.

**Evidencia:** SELECT STATUSIMP, COUNT(*), MIN/MAX(dtaimport) FROM IMP_Titulo; NOT EXISTS contra EXT_Titulo = 783.242; SUM(VlrOriginal) por YEAR(DtaEmissao); SELECT COUNT(*),SUM(VlrAberto) FROM EXT_Titulo WHERE IndQuitado=0 AND IndAtivo=1; GEP_JOBAGD.UltimaExecucao do COBRANCA_CRIT

**Licao para a Tracbel:** EVITAR o padrão em que o único sinal de saúde é uma coluna de status dentro da própria linha, sem contador, sem alerta e sem SLA. No CRM novo: toda fila de integração precisa de (a) métrica de lag (idade do registro pendente mais antigo) exportada, (b) alerta quando lag > N minutos, (c) dashboard de backlog por stream. Um represamento de R$ 5,18 bi por 15 meses sem ninguém perceber é falha de observabilidade, não de código.

### 4. IMP_OS: 280.214 de 287.868 ordens de serviço nunca chegaram a EXT_OS, e as 100% marcadas como processadas mentem

IMP_OS tem 287.868 linhas, TODAS com StatusIMP='E' e DtaImport NULL, DtaGeracao de 02/08/2021 a 25/05/2024, todas Origem='Protheus'. EXT_OS tem apenas 8.099 linhas, com Dtaabertura 100% NULL e MAX(DtaAlteracao)=25/05/2024. O teste direto (NOT EXISTS por IDOS) mostra 280.214 OS de staging sem contrapartida em EXT_OS — ou seja, o status 'E' foi gravado sem que o registro fosse promovido. IMP_OSITEM (399.040 linhas) está com StatusIMP 100% NULL (nunca processado), enquanto EXT_OSITEM tem 182.238 linhas com DTAIMPORT NULL — cabeçalho e item ficaram dessincronizados. Toda a área de pós-venda/oficina do CRM (EXT_OS, EXT_OSITEM, EXT_OSSolic, EXT_VeicKM que referencia IDOS) está congelada em maio/2024. O job PR_INT_PROPROS ('INTEGRAÇÃO PROPRIEDADE DA OS / INTEGRAR VEICULOS DA OS', GEP_JOBCAD SeqJOB=62) tem UltimaExecucao=18/09/2025 e intervalo de 480 min — morto há quase um ano.

**Evidencia:** SELECT StatusIMP,COUNT(*),MAX(DtaGeracao),MAX(DtaImport) FROM IMP_OS; SELECT COUNT(*) FROM IMP_OS i WHERE NOT EXISTS(SELECT 1 FROM EXT_OS e WHERE e.IDOS=i.IDOS) = 280.214; contagens e datas de EXT_OS/EXT_OSITEM; GEP_JOBAGD SeqJOBAgd=37

**Licao para a Tracbel:** EVITAR status de processamento gravado fora da transação que faz o trabalho — 'E' aqui significa 'alguém marcou', não 'chegou ao destino'. No CRM novo, a marcação de processado deve estar na MESMA transação do INSERT no destino, ou (melhor) não existir status mutável: usar offset/watermark do consumidor + reconciliação por contagem origem vs destino executada diariamente.

### 5. O faturamento parou em três datas distintas por stream — não foi uma falha, foram três

X_TOTVS_CRM_FATURAMENTO segmenta o feed em 5 streams pela coluna ORIGEM, e cada um morreu em momento diferente: FAT_SERVICOS última NF em 29/08/2024 (44.220 registros no ledger, 9.842 em 2024); FAT_MAQUINAS em 07/02/2025 (149 em 2025) e DEV_MAQUINAS em 06/02/2025; FAT_PECAS e DEV_PECAS em 11/04/2025 (107.092 e 1.040 em 2025). O ledger X_V_IMP_CRM_IMP_NF (815.854 linhas) confirma X_DATA_INTEGRACAO máximo por stream: FAT_PECAS 11/04/2025 14:02:31, DEV_PECAS 11/04/2025 12:03:47, FAT_MAQUINAS 08/02/2025 10:13:52, DEV_MAQUINAS 07/02/2025 12:13:09, FAT_SERVICOS 29/08/2024 14:09:26. Consequência a jusante: EXT_NFS congelou em DtaEmissaoNF=11/04/2025 (DtaImport 14/04/2025 16:57:06) e EXT_NFSItem tem MAX(DtaImport)=03/10/2023 (o item ficou 18 meses atrás do cabeçalho). O SCHEMA_MAP do repositório afirma 'para em 11/04/2025' — está incompleto: serviços já estava morto 7 meses antes.

**Evidencia:** SELECT ORIGEM,X_INTEGRADO,MIN/MAX(X_DATA_INTEGRACAO) FROM X_V_IMP_CRM_IMP_NF GROUP BY; SELECT YEAR(DATA_EMISSAO_NF),ORIGEM,COUNT(*),MAX(DATA_EMISSAO_NF) FROM X_TOTVS_CRM_FATURAMENTO; MAX(DtaImport) de EXT_NFS e EXT_NFSItem

**Licao para a Tracbel:** ADAPTAR: monitorar por STREAM, nunca pela tabela agregada. No CRM novo cada fluxo (peças, serviços, máquinas, devoluções) é um tópico com seu próprio lag e seu próprio alerta — a falha de um não pode ficar mascarada pelo volume do outro. E cabeçalho e item precisam ser publicados/consumidos como uma unidade transacional; 18 meses de defasagem entre EXT_NFS e EXT_NFSItem é dado silenciosamente inconsistente.

### 6. RUNGEPIMPORT falha a cada 20 minutos há meses com erro de versão, e o sistema registra isso apenas como Warning

O job APPVTC/SeqJOBAgd=40 executa C:\VORTICE\TDEV\APPSRV\VCRM_RUNGEPIMPORT.EXE (#MONITORA=1) a cada 20 minutos. Toda execução grava em GEP_JobAgdExecLog: 'Advertencia: CodMsg:C5K003E, msg: Versão incompatível Sistema: GLOBAL, módulo: RUNGEPIMPORT, versão no aplicativo [4.04.01r05], versão no banco [4.04.01r01]'. TipoLog='W' (warning) e TipoErro='Execução normal' — ou seja, o sistema classifica uma falha estrutural como execução normal. Em 30 dias: 2.338 W, 8.568 N e 1 E. No histórico total: 3.948 'App não encontrado', 3.393+82+30 'App travado'. Esse é o processo que drena GEP_Import — a fila genérica de integração — e ela está entupida: RD/GE_CONTATO 393 linhas Status='E', RD/IV_QUESTIONARIO 155 'E', MOBILELITE/IV_HISTORICO 128 'E' + 38 'X', RD/IV_HISTORICO 7 'E'. Em paralelo, GEP_ImportAprovacao tem 4.861 linhas da origem CRMTRAC com Status NULL (IV_AGENDA 3.257, IV_PROCDADO 818, IV_PROCESSO 786) até 28/08/2026 — fila de quarentena/aprovação nunca drenada.

**Evidencia:** GEP_JobAgdExecLog: SELECT TOP 1 CAST(Descricao AS varchar(1200)) WHERE Descricao LIKE '%RUNGEPIMPORT%'; SELECT Codigo,TipoLog,COUNT(*) últimos 30 dias; SELECT TipoLog,TipoErro,COUNT(*) histórico; SELECT Origem,Tabela,Status,COUNT(*) FROM GEP_Import e GEP_ImportAprovacao

**Licao para a Tracbel:** EVITAR severidade errada: uma incompatibilidade de versão entre binário e schema é ERRO FATAL, nunca warning. No CRM novo: (a) o worker valida a versão do schema no startup e RECUSA subir se divergir (fail fast, com alerta), (b) níveis de log têm semântica contratual e são testados, (c) toda fila tem DLQ explícita com contador exposto — GEP_Import com Status='E' é uma DLQ sem nome, sem retry e sem dono.

### 7. As procedures da ACL não têm transação, não têm TRY/CATCH e o BEGIN TRANSACTION/COMMIT está literalmente comentado

Em X_P_IMP_CRM_TITULO o passo 2 é '--BEGIN TRANSACTION;' e o passo 9 é '--COMMIT' — ambos comentados. A sequência é: (3) monta #Base_Importacao classificando ATUALIZAR/INCLUIR; (4) DELETE FROM X_T_IMP_CRM_TITULO das chaves ATUALIZAR; (5) INSERT do novo estado vindo do linked server; (7) INSERT INTO IMP_Titulo de tudo com X_INTEGRADO='NAO'; (8) UPDATE X_T_IMP_CRM_TITULO SET X_INTEGRADO='INTEGRADO'. Sem transação, uma falha entre (4) e (5) PERDE os registros (deletados do estado, nunca reinseridos); uma falha entre (7) e (8) DUPLICA (a próxima execução reinsere as mesmas linhas em IMP_Titulo, que nunca é limpa). X_P_IMP_CRM_NF_POS_VENDA tem o mesmo problema entre os passos 4 (grava ledger com 'NAO'), 6/7 (insere IMP_NFS/IMP_NFSItem) e 8 (marca 'INTEGRADO'). Nenhuma das duas tem TRY/CATCH, log de execução, contador de linhas afetadas ou watermark persistido. X_P_IMP_CRM_NF_POS_VENDA ainda contém código de debug comentado inline ('and Nronf in (192878, 23104, 23284)') e um piso de data hardcoded: WHERE A.DATA_EMISSAO_NF >= '2023-01-01'.

**Evidencia:** sys.sql_modules de X_P_IMP_CRM_TITULO (passos 2 e 9 comentados) e de X_P_IMP_CRM_NF_POS_VENDA (passos 4-8, debug comentado, filtro de data literal)

**Licao para a Tracbel:** EVITAR integralmente. No CRM novo: cada lote é uma unidade transacional (ou usa outbox/inbox com exactly-once por chave); TRY/CATCH com registro em tabela de execução (job, início, fim, lidos, inseridos, rejeitados, erro); watermark persistido em tabela de controle, nunca literal no código; nada de código de debug comentado em produção. Se a linguagem for SQL, no mínimo SET XACT_ABORT ON + BEGIN/COMMIT/ROLLBACK reais.

### 8. A chave de correlação com o TOTVS é uma string concatenada por pipes, e mudança de status gera chave nova em vez de update

COD_CHAVE_NF = CONCAT(STATUS_NF,'|',ORIGEM,'|',FILIAL,'|',NRO_NF,'|',SERIE,'|',COD_CLIENTE,'|',DELETADO) e COD_CHAVE_ITEM acrescenta '|',COD_OPERACAO,'|',PRODUTO,'|',NRO_ITEM,'|',DELETADO. Exemplo real: 'N|FAT_PECAS|7|000431176|1  |053009825|0001|' e 'N|FAT_PECAS|7|000431176|1  |053009825|0001|506|R282831|01|' (note os espaços à direita em SERIE, preservados no concat). Como STATUS_NF e DELETADO fazem PARTE da chave, o cancelamento de uma NF no Protheus produz uma chave DIFERENTE — e a procedure só tem branch 'INCLUIR' (WHERE ... NOT IN (SELECT COD_CHAVE_ITEM FROM X_V_IMP_CRM_IMP_NF)). Resultado: a NF cancelada entra como registro NOVO em IMP_NFS em vez de atualizar/estornar a original. Já X_P_IMP_CRM_TITULO usa como chave a coluna chamada 'Obs' (o comentário na view irmã é explícito: '--Null|varchar(250) (vou usar como id)') e faz o join com COLLATE SQL_Latin1_General_CP1_CI_AS explícito em cada comparação, porque linked server e CRM têm collations diferentes; a própria DDL de X_V_IMP_CRM_IMP_NF mistura Latin1_General_CI_AS (COD_CHAVE_ITEM, COD_CHAVE_NF, ORIGEM) com SQL_Latin1_General_CP1_CI_AS (X_INTEGRADO) na MESMA tabela. Os 4 índices criados pela procedure lideram todos pela coluna ID (identity), inúteis para as buscas por COD_CHAVE_ITEM que eles deveriam servir.

**Evidencia:** Corpo de X_P_IMP_CRM_NF_POS_VENDA (DDL da tabela + 4 CREATE NONCLUSTERED INDEX liderados por ID); SELECT TOP 5 COD_CHAVE_NF, COD_CHAVE_ITEM FROM X_V_IMP_CRM_IMP_NF; corpo de X_P_IMP_CRM_TITULO (joins com COLLATE); comentário 'vou usar como id' em X_V_CRM_IMP_IMP_NFS

**Licao para a Tracbel:** ADAPTAR: chave natural composta é correta, mas deve ser COLUNAS separadas com índice único, não string concatenada — concat esconde espaços, quebra com pipe no dado e impede índice seletivo. E a chave NUNCA pode conter atributos mutáveis (status, deletado): a chave identifica o documento, o status é payload. EVITAR: usar campo Obs como identificador; misturar collation na mesma base; criar índices liderados pela PK identity quando a busca é por outra coluna.

### 9. As datas do Protheus chegam com sentinelas 1900-01-01 tratadas coluna a coluna, e o que escapa vira lixo (vencimento em 5024 e 2223)

X_P_IMP_CRM_TITULO higieniza manualmente três colunas, com um CASE repetido em DOIS INSERTs diferentes: CASE WHEN DtaUltPgto='1900-01-01 00:00:00.000' THEN NULL ELSE DtaUltPgto END, idem para DtaQuitacao, DtaUltAlteracao e DATA_ALTERACAO. Mas DtaEmissao, DtaVencto e DtaVenctoOrig NÃO são higienizadas nem validadas. Resultado medido em EXT_Titulo: MAX(DtaVencto)=08/05/5024 e, entre os títulos em aberto (IndQuitado=0 AND IndAtivo=1), MAX(DtaVencto)=10/08/2223; 6 títulos com DtaVencto > 2100-01-01. Não há CHECK constraint nem validação de faixa em nenhum ponto da esteira.

**Evidencia:** Corpo de X_P_IMP_CRM_TITULO (CASE 1900-01-01 em 3 colunas, ausente nas demais); SELECT MAX(DtaVencto) FROM EXT_Titulo = 08/05/5024; SELECT MAX(DtaVencto) WHERE IndQuitado=0 AND IndAtivo=1 = 10/08/2223; COUNT(*) WHERE DtaVencto>'2100-01-01' = 6

**Licao para a Tracbel:** COPIAR a ideia (normalizar sentinelas de data do ERP na fronteira) mas EVITAR a execução: a normalização deve ser uma função/policy aplicada a TODAS as colunas de data do contrato, uma vez, no adaptador — não um CASE copiado e colado que esquece metade. Adicionar validação de faixa (ex.: 1990 ≤ ano ≤ ano_atual+30) que REJEITA a linha para a DLQ em vez de gravar lixo, e CHECK constraints no destino.

### 10. EXT_* (ERP) e IV_* (CRM/BPM) são duas ilhas ligadas só por SeqPessoa — não existe join documento↔oportunidade

IV_Historico (29 colunas, ~2,44M linhas) NÃO tem nenhuma coluna de vínculo com ERP: as colunas são SeqHistorico, SeqPessoa, Contato, NroEmpresa, SeqUsuario, CodUsuario, AcaoGeradora, Departamento, Resultado, ResultadoCmpl, AgendaOrigem, DtaRealizacao, Detalhe, Natureza, Vendedor, UltAlteracao, UsuAlteracao, FormaPrimCont, Processo, CodProcesso, Valor, Qtde, Duracao, TemCiencia, Latitude, Longitude, SEQPESSOACTTO, USUINCLUSAO, DTAINCLUSAO — nenhum IdNFS, IDOS, idTitulo, NroNF ou Chassi. IV_ProcDado idem (24 colunas, nenhuma chave de ERP). IV_Questionario TEM LinkDocto/LinkNro/LinkSerie, mas o GROUP BY mostra ZERO linhas preenchidas. Do lado ERP, EXT_NFS tem IdVeic NULL em 387.390 de 390.755 linhas (99,1%) e NroPedido vazio em 7.805. Portanto não há caminho relacional de 'esta oportunidade virou esta nota fiscal'. A ponte real é MANUAL: a tabela IV_Q_ACOMP_VENDA_FINANC (formulário 'ACOMP DE VENDA FINANCEIRO (FY25)') tem VENDA_FINANC_CHASSI varchar(50), VENDA_FINANC_NRO_NF varchar(30), VENDA_FINANC_DATAFAT, VENDA_FINANC_DATAENT, VENDA_FINANC_FILIAL, NUMERO_PROPOSTA, COMAR decimal + COMAR_ varchar(30) — tudo digitado pelo vendedor. É de lá que o relatório TBA_101XX3 tira faturamento, e é por isso que 125 questionários sumiram quando trocaram COMAR por COMAR_.

**Evidencia:** sys.columns de IV_Historico e IV_ProcDado; SELECT LinkDocto,COUNT(*) FROM IV_Questionario WHERE LinkDocto<>'' → 0 linhas; SELECT COUNT(*), SUM(CASE WHEN IdVeic IS NULL...) FROM EXT_NFS → 387.390/390.755; sys.columns de IV_Q_ACOMP_VENDA_FINANC; docs/chamado-vortice-relatorio-tba101.md

**Licao para a Tracbel:** ESTE É O ACHADO QUE MAIS IMPORTA PARA O CRM NOVO. Modelar de saída a relação Oportunidade ↔ Documento (pedido, NF, OS, título, equipamento/chassi) como tabela de vínculo de primeira classe (opportunity_document: opportunity_id, doc_type, doc_external_key, source_system, matched_by, matched_at), populada pela integração e não por digitação. Enquanto isso não existir, todo indicador de conversão/funil dependerá de formulário preenchido à mão — que é exatamente a origem dos dois defeitos do TBA_101XX3.

### 11. A integração JD Edwards / John Deere é schema morto: 7 tabelas JDE_*, todas com 0 linhas, e o job nunca foi agendado

JDE_EQUIPAMENTS (18 col.: EQUIPAMENT_ID, categoryId, subCategoryId, costPrice, listPrice, machineHours, makeName, modelName, serialNumber, equip_status, equip_year, storeLocation, totalEquipmentMargin/SellingPrice etc.), JDE_QUOTE (19 col.: quoteId, deereUserId, balanceDue, creationDate, expirationDate, netCost, netProceeds, poNumber, quoteName, quoteStatus, quoteType, totalNetTradeValue, tradeDifference, downPayment, SEQPESSOA, agreementDate, signDate), JDE_QUOTE_ITEM, JDE_SALES_PERSON (deereUserId, firstName, lastName, middleName, emailAddress), JDE_CATEGORY, JDE_SUB_CATEGORY e JDE_PURCHASE (18 col., poNumber, warrantyBeginsDate, deliveredDate, SeqQuestionario) — TODAS com COUNT(*)=0. O job existe no catálogo (GEP_JOBCAD SeqJOB=34, Codigo='JD_QUOTE', 'Realiza a integração de dados com a JD Quote') mas NÃO consta em GEP_JOBAGD, isto é, nunca foi agendado. O modelo interno é coerente (FKs JDE_EQUIPAMENTS→JDE_CATEGORY/JDE_SUB_CATEGORY, JDE_QUOTE→JDE_SALES_PERSON, JDE_QUOTE_ITEM→QUOTE/EQUIPAMENTS) mas o único ponto de contato com o CRM seria JDE_QUOTE.SEQPESSOA (SEM FK declarada para GE_Pessoa) e JDE_PURCHASE.SeqQuestionario→IV_Questionario. Na prática o número da cotação Deere entra no CRM à mão: IMP_REL_TBA101.JD_QUOTE nvarchar(120) alimentado pelo formulário, e existem views IV_Q$ACOMPAN_COMPRA_JDE, IV_Q$ACOMPANH_VENDA_JDE, IV_Q$APRESENTACAO_JDE, IV_Q$VENDA_PERDIDA_JDE, IV_Q$SERVICO_EXTERNOS_JD — todas questionários.

**Evidencia:** SELECT COUNT(*) das 7 tabelas JDE_* → 0; GEP_JOBCAD SeqJOB=34 sem correspondente em GEP_JOBAGD (LEFT JOIN); schema/fks.csv (JDE_QUOTE.SEQPESSOA sem FK); schema/views.csv (IV_Q$*_JDE); IMP_REL_TBA101 coluna JD_QUOTE

**Licao para a Tracbel:** EVITAR carregar schema morto: 7 tabelas e 6 FKs que nunca receberam um registro poluem o modelo e enganam quem lê o banco (o SCHEMA_MAP deste repositório documenta JDE_* como integração ativa). No CRM novo, integração que não está ligada não tem tabela. ADAPTAR: se JD Quote voltar ao escopo, tratar como origem externa normal via a mesma crosswalk (system='JDE', external_key=quoteId) em vez de um conjunto de tabelas paralelas com nomenclatura própria (camelCase inglês no meio de um banco PT-BR).

### 12. O ETL Pentaho (Kitchen/Pan) é a quarta camada de integração e está morto desde julho/2021

GEP_JOBAGD tem 6 entradas Tipo='CICLICA' Codigo='APPEXEC' apontando para atalhos Pentaho Data Integration: D:\VORTICE\PDI\KITCHEN_VEICULO.LNK (UltimaExecucao 30/07/2021 17:48:37), KITCHEN_PECAS.LNK (29/07/2021 16:00:00), PAN_IMP_VEICULO.LNK (29/07/2021 14:00:00), 'INTEGRA BLOQ DOCUMENTAÇÃO.LNK' (29/07/2021 12:00:01), PAN_TITULOS_SISDIA.LNK (29/07/2021 16:00:00, sem IntervaloExec) e KITCHEN_TITULOS_ACRESC.LNK (29/07/2021 16:30:01). Kitchen executa jobs e Pan executa transformações do Pentaho/Kettle. O log GEP_JobAgdExecLog só tem 15 registros de APPEXEC, todos de 16/01/2017. O nome PAN_TITULOS_SISDIA confirma que o ETL Pentaho era a esteira do ERP legado SISDIA — que ainda responde por 313.734 títulos em EXT_Titulo, 197.315 NFs em EXT_NFS, 27.859 produtos e 347 vendedores. A migração SISDIA→Protheus aconteceu em 2021 (EXT_NFS: SISDIA vai até 2021, Protheus começa em 2021) e os dois espelhos coexistem na MESMA tabela, distinguidos apenas pela coluna Origem varchar(20), sem versionamento nem marcação de sistema descontinuado.

**Evidencia:** GEP_JOBAGD SeqJOBAgd 5,6,7,10,13,14 (Aplicacao e UltimaExecucao); GEP_JobAgdExecLog WHERE Codigo='APPEXEC' → 15 linhas, todas 16/01/2017; SELECT Origem,YEAR(DtaEmissaoNF),COUNT(*) FROM EXT_NFS; SELECT Origem,COUNT(*) FROM EXT_Titulo

**Licao para a Tracbel:** ADAPTAR: coexistência de sistemas de origem numa mesma tabela canônica é aceitável e até desejável (o dado histórico do SISDIA tem valor), MAS precisa de um catálogo de sistemas de origem com ciclo de vida (ativo/descontinuado/data de corte) e não de um varchar livre. EVITAR: manter agendamentos apontando para ferramentas que ninguém opera há 5 anos — cada um deles é uma falsa sensação de que a integração existe.

### 13. O vocabulário de 'Origem' é texto livre, com duplicatas e valores de teste em produção

GEP_ParRecebe (14 linhas) é o registro de origens de entrada, com flags de comportamento por origem: IndAlteraPessoa, IndIdentPesCNPJ (identifica por CNPJ), IndSobrepoeFone, IndEnviaOut, DestinoOut, QtdDiasRemove (retenção do staging), INDALTERAPESSOAEXT, INDMANTEMORDEMFONES, INDMANTEMATIVO. As origens cadastradas: AUDIT, COLORADO, CRM-Manual, CRMTRAC, EEMOVEL, IMPORT, IMPORTACAO, INTEGRAÇÃONOROESTE, JUNO, MOBILELITE, Protheus, RD, WEB_CRM, WEBCRM. Note IMPORT vs IMPORTACAO e WEB_CRM vs WEBCRM — duplicatas semânticas. GEP_ParEnvia (3 linhas: MOBILELITE, RD, WEBHOOK) é o registro de saída, e GEP_ParRecEnvia (que ligaria origem↔destino) está VAZIA. Pior: os valores realmente presentes nos dados NÃO batem com o cadastro — EXT_Pessoa.PessoaLinkOrigem tem 'VTC_CRT' (6.481 linhas) e 'SISDIA' (2), ausentes de GEP_ParRecebe, além de 'erro' (264 linhas) e 'teste' (1). GE_Pessoa.Origem tem 13 valores distintos incluindo 'ERP' (4.723), 'EMAIL' (1), COLORADO (51.199), vazio (6). Não há FK nem CHECK ligando essas colunas ao cadastro de origens.

**Evidencia:** SELECT * FROM GEP_ParRecebe / GEP_ParEnvia / GEP_ParRecEnvia; SELECT PessoaLinkOrigem,COUNT(*) FROM EXT_Pessoa GROUP BY; SELECT Origem,COUNT(*) FROM GE_Pessoa GROUP BY; ausência de FK em schema/fks.csv

**Licao para a Tracbel:** COPIAR o conceito de GEP_ParRecebe — parametrizar POR ORIGEM o comportamento do merge (pode alterar cadastro? identifica por CNPJ? sobrepõe telefone? quantos dias retém o staging?) é excelente e evita if/else no código. EVITAR: deixar a origem como varchar livre sem FK. No CRM novo, source_system é tabela com PK e TODA coluna de origem é FK para ela; 'erro' e 'teste' em produção seriam impossíveis.

### 14. Nenhuma tabela de staging (IMP_*) nem de integração (X_*) tem FK, PK confiável ou política de retenção consistente

Das 672 FKs do banco, ZERO parte de uma tabela IMP_* ou X_*. As IMP_* têm PK declarada (ex.: IMP_Titulo.idTitulo, IMP_OS.IDOS, IMP_NFSItem.IdNFSItem) mas nenhuma restrição de integridade referencial — inclusive IMP_OSITEM tem PK Iditem e uma coluna IDOS nullable sem FK. A retenção é incoerente entre tabelas: IMP_PESSOA/IMP_VEICULO/IMP_PRODUTO/IMP_Evento/IMP_Pedido são drenadas (0 linhas), enquanto IMP_Titulo acumula 892.131, IMP_OSITEM 399.040, IMP_OS 287.868 e IMP_NFSItem 67.404 — mesmo mecanismo, políticas opostas, e o parâmetro que deveria controlar isso (GEP_ParRecebe.QtdDiasRemove) está preenchido com 0 ou vazio em todas as 14 origens. Somam-se a isso as tabelas de backup manual deixadas em produção: EXT_Titulo_bkp_11_04 (108.061), X_T_IMP_CRM_TITULO_bkp_11_04 (121.381), X_V_IMP_CRM_IMP_NF_BKP_18_09_2023 (415.762), EXT_Pessoa_bkpago22 (27.280), IMP_Titulo_bkp_11_04, EXT_Titulo_BKP_22_03_2023 e _BAIXADOS, GEP_Import_bkpjun (502.009).

**Evidencia:** schema/fks.csv (nenhuma origem IMP_*/X_*); schema/pks.csv; schema/tabelas.csv (contagens das *_bkp*); SELECT QtdDiasRemove FROM GEP_ParRecebe (0/vazio nas 14 linhas)

**Licao para a Tracbel:** ADAPTAR: staging sem FK é aceitável e correto (o staging deve aceitar dado sujo), MAS precisa de (a) política de retenção declarada e AUTOMÁTICA por stream, (b) particionamento por data para o purge ser barato, (c) contrato de qualidade explícito na promoção staging→canônico com destino para DLQ. EVITAR: backup por CREATE TABLE ... SELECT INTO na base de produção — no CRM novo isso é snapshot/PITR de infraestrutura, nunca tabela irmã com sufixo de data.

### 15. O flag de estorno da NF nunca é populado — quem carrega a informação é Situacao, e a consulta 'padrão' do repositório está errada

EXT_NFS.IndEstorno numeric(1) está em 0 nas 390.755 linhas, sem exceção. A informação real de cancelamento está em Situacao char(1): 'N' 379.603 (normal), 'C' 11.080 (cancelada), 'D' 72 (devolução). O catálogo EXT_NFSOper (241 linhas: 231 Protheus + 10 SISDIA) também tem IndEstorno=0 em 100% e EntradaSaida='S' em 100%, mesmo tendo operações cujo Descricao é 'DEV_PECAS' e 'DEV_MAQUINAS' (CodOperacao 200, 201, 203, 207-250, 401) — ou seja, devolução é identificável apenas pelo texto da descrição ou pela ORIGEM do stream. Consequência prática: a query de exemplo do próprio SCHEMA_MAP deste repositório ('WHERE IndEstorno = 0 ... faturamento por mês') inclui silenciosamente as 11.080 notas canceladas. Também há duplicidade de mapeamento no catálogo: CodOperacao '501' aparece 3 vezes (IdNFSOper 14 com Descricao 'VENDA', 31 e 69 com Descricao vazia, SeqDepto 3 e 2).

**Evidencia:** SELECT IndEstorno,Situacao,COUNT(*) FROM EXT_NFS GROUP BY; SELECT Origem,EntradaSaida,IndEstorno,COUNT(*) FROM EXT_NFSOper GROUP BY; SELECT TOP 20 ... FROM EXT_NFSOper (CodOperacao 501 triplicado); SCHEMA_MAP.md seção 7

**Licao para a Tracbel:** EVITAR colunas de flag que a integração nunca preenche — são pior que ausentes, porque induzem consultas erradas com aparência de corretas. No CRM novo: se o campo faz parte do contrato, o adaptador é obrigado a preenchê-lo e um teste de contrato falha se ficar constante; se não faz, não existe. E o catálogo de operação fiscal deve ter chave única (source_system, cod_operacao, filial) — não permitir três linhas para '501'.

### 16. O CRM tem cadeia de saída (CRM→ERP) modelada e completamente inutilizada

EXT_Pedido tem 57 colunas incluindo IndEnvioERP numeric(1), DtaEnvioERP datetime, NroPedidoERP varchar(30), PedidoLinkNro/PedidoLinkStr/PEDIDOLINKORIGEM, PAILINKORIGEM/PAILINKNRO/PAILINKSTR (hierarquia pai/filho de pedido), PROCESSO numeric(18) — a única tabela EXT_ que aponta explicitamente para o processo do CRM — e DTAALTERACAOERP. E tem 0 linhas. Mesma coisa para EXT_PedidoItem (0), IMP_Pedido (61 col., 0), IMP_PedidoItem (0). GEP_ParEnvia cadastra 3 destinos (MOBILELITE, RD, WEBHOOK/SessaoCnx=VORTICO_SERVER) e o job OUT_EVENTO ('Envia eventos de integração', GEP_JOBCAD SeqJOB=23) não está agendado; MOV_PESSOA_LOTE_SAAS (SeqJOB=50) também não. Ou seja: o Vórtice na Tracbel é uma integração de MÃO ÚNICA — o CRM consome do ERP e não devolve nada. Os pedidos vivem só como IV_Processo + questionário. Complementando o quadro, EXT_APROVACAO (2.655), EXT_AGUARDENTREGA (52) e EXT_CONDPAGTO (22) têm prefixo EXT_ mas NÃO vêm de ERP nenhum: são tabelas de controle (PROCESSO, DTAGERACAO, processodna) escritas pelas procedures locais VTC_P_GERAAPROVACAO / VTC_P_GERAAGUARDENTREGA / VTC_P_GERACONDPAGTO, que rodam a cada 5 minutos e estão entre os poucos jobs realmente vivos (última execução 30/08/2026 10:35).

**Evidencia:** sys.columns de EXT_Pedido; SELECT COUNT(*) de EXT_Pedido/EXT_PedidoItem/IMP_Pedido/IMP_PedidoItem = 0; GEP_ParEnvia (3 linhas); GEP_JOBCAD SeqJOB 23 e 50 sem agendamento; GEP_JOBAGD SeqJOBAgd 43,44,45 (UltimaExecucao 30/08/2026); contagens de EXT_APROVACAO/AGUARDENTREGA/CONDPAGTO

**Licao para a Tracbel:** ADAPTAR: o CRM novo PRECISA de saída (pedido gerado no CRM → ERP), e o modelo do EXT_Pedido dá a lista certa de campos de controle (enviado?, quando?, id no destino, link pai/filho, processo de origem). Implementar como OUTBOX transacional: a mesma transação que grava o pedido grava o evento de saída; um worker publica e marca. EVITAR: prefixo mentindo sobre a natureza da tabela (EXT_ para tabela de controle local) — no CRM novo o prefixo/schema deve refletir a camada (raw/staging/core/control), e isso deve ser verificado no CI.

### 17. O relatório TBA_101XX3/XX4 usa staging por usuário sem DELETE, e a tabela nunca é limpa

IMP_REL_TBA101 (29 col.) e IMP_REL_TBA101_PG2 (31 col.) são tabelas de apoio gravadas pelo relatório, com codusuario como discriminador. Estado atual medido: IMP_REL_TBA101 tem 302 linhas em 18 usuários, aparentemente 1 linha por processo (LORENA.TAVARES 149 linhas/149 processos). Já IMP_REL_TBA101_PG2 tem 1.558 linhas em 4 usuários com duplicação grosseira: V:AMAURY 208 linhas para apenas 13 processos (16 cópias de cada), RENATA.ABRA 548/487, RENATA.ABRA2 405/401, MATHEUS.AUGUSTO 397/397. A causa está documentada no chamado: a procedure VTC_Gera_REL_TBA101 faz o DELETE (linha 17) mas o SeqCons 171 replica a lógica inline e faz o INSERT (linha 78) SEM o DELETE. Além disso a coluna COMAR está como decimal na PG2 e precisa virar varchar(30) porque o formulário passou a gravar em COMAR_ varchar(30) — 125 questionários ficaram invisíveis (100% dos preenchimentos desde 01/08/2026 vão para o campo novo).

**Evidencia:** SELECT codusuario,COUNT(*),COUNT(DISTINCT PROCESSO_N) FROM IMP_REL_TBA101 e _PG2; docs/chamado-vortice-relatorio-tba101.md itens 1.9 e 2; sys.columns de IV_Q_ACOMP_VENDA_FINANC (COMAR decimal(5) e COMAR_ varchar(30) coexistindo)

**Licao para a Tracbel:** EVITAR materializar resultado de relatório em tabela compartilhada discriminada por usuário — é race condition, vazamento entre usuários e lixo acumulado por design. No CRM novo: relatório é query com paginação, ou materialização em cache com TTL e chave própria (não o login), ou tabela temporária/CTE. E EVITAR o padrão 'cria coluna nova, mantém a antiga, não migra' — toda mudança de tipo de campo de formulário deve ter migração de dados obrigatória no mesmo deploy.

### 18. GEP_SyncUsrSat é um outbox por dispositivo com 3,9 milhões de linhas e fan-out por usuário

GEP_SyncUsrSat (15 col., 3.901.359 linhas) replica o CRM para os satélites móveis: (Destino, Usr, Tabela, KN1/KN2 numeric(18) e KS1/KS2 varchar(50) como chave genérica do registro, Operacao char(1) = I/A/E, IndProcessado, IndProcessoWait, NivelSeguranca, DtaGeracao, DtaColeta, FCMTOKEN, SEQSYNCUSRSAT PK). É um CDC caseiro: uma linha por (usuário × tabela × registro × operação), replicando ~60 tabelas (IV_QUESTIONARIO 352.978+161.370+..., IV_CLIENTEPROPR 294.338, IV_HISTORICO 239.673, IV_ACAOAUTO 207.582, IV_PROCESSO 193.589, IVS_PES, GE_POLSEGPERM, DMN_DOC, IV_PROCDOCTO...). Para apenas 65 usuários MOBILELITE (GEP_UsrSat, VersaoApp 5.0.7, último envio 29/08/2026) isso são 3,9M de linhas. IndProcessado tem domínio 0/1/2/9 sem catálogo. Há backlog grande em 0 (não processado): IV_QUESTIONARIO 352.978 com Operacao='E', IV_HISTORICO 152.938, IV_CLIENTEPROPR 141.882. O destino JUNO (6 usuários) está morto desde 17/03/2025. Relacionado: IV_ProcDocto tem 5.306 de 76.603 linhas (6,9%) apontando para SeqDocto inexistente em DMN_Doc — os órfãos que quebram a carga inicial do Vórtico Mobile Lite com erro de FOREIGN KEY, e que continuam crescendo (a memória do projeto registrava 5.302).

**Evidencia:** SELECT Destino,Tabela,Operacao,IndProcessado,COUNT(*),MAX(DtaGeracao) FROM GEP_SyncUsrSat; SELECT * FROM GEP_UsrSat; SELECT COUNT(*), SUM(CASE WHEN d.SeqDocto IS NULL...) FROM IV_ProcDocto p LEFT JOIN DMN_Doc d → 76.603/5.306

**Licao para a Tracbel:** ADAPTAR: a ideia de outbox por destino com operação I/A/E e chave genérica está certa, mas o fan-out por usuário é insustentável. No CRM novo, publicar UM evento por mudança e resolver visibilidade no momento da leitura (o cliente pede delta desde seu cursor, filtrado por permissão) — não materializar N cópias. Se precisar de push, usar cursor/watermark por dispositivo, não fila por dispositivo. E blindar o sync contra órfãos: a promoção só publica o vínculo se o alvo existir, ou publica junto o documento — 6,9% de órfãos derrubando a carga inicial é falha de integridade que o banco deveria ter impedido com FK.

### 19. O envio de e-mail é uma fila com 22.512 falhas silenciosas e sem retry

GEP_EMailSend (21 col.) é a fila de saída (Prioridade, De, Para, Cc, Bcc, Assunto, Anexado, Mensagem text, Solicitante, NroEmpresa, EnderecoResposta, Status varchar(1), DtaGeracao, DtaEnviar, SEQCONTAEMAIL→GE_ParamLista, CONTEXTO, SUBCONTEXTO, MSGERRO, ORIGEM char(3), CHAVE) e GEP_EMAILSENT (20 col.) é o arquivo. Estado: 563.426 enviados sem status e 22.512 com STATUS='E' (3,8% de falha). Os MSGERRO mais frequentes: 'E-mail sem destino' 6.381 (até 29/08/2026), 'E-mail remetente inválido' 2.723, 'E-mail com mensagem vazia' 1.115, além de e-mails com espaço no meio ('emanuele. vaz@vencorr. com', 423 ocorrências). Nada disso é retentado nem alertado. Por CONTEXTO/SUBCONTEXTO: WORKFLOW/MESSAGECENTER (ORIGEM='HST') 492.233 ativo até 29/08/2026; COBRANCA/AVISO (ORIGEM='TIT') 65.165 PARADO desde 22/03/2023 — coerente com o congelamento de EXT_Titulo. O job EMAIL_SEND roda a cada 3 min e está vivo; o EMAIL_IN_CRM (integração de e-mails de entrada, intervalo 1 min) tem UltimaExecucao 29/04/2024 — morto há 16 meses.

**Evidencia:** SELECT Status,COUNT(*) FROM GEP_EMailSend/GEP_EMAILSENT; SELECT CONTEXTO,SUBCONTEXTO,ORIGEM,COUNT(*),MAX(DTAENVIO); SELECT MSGERRO,COUNT(*) WHERE MSGERRO<>''; GEP_JOBAGD SeqJOBAgd 15 e 35

**Licao para a Tracbel:** COPIAR o par fila/arquivo com CONTEXTO+SUBCONTEXTO+ORIGEM+CHAVE — essa tripla permite rastrear qual regra de negócio gerou cada e-mail, o que é raro e muito útil. ADAPTAR: adicionar política de retry com backoff, contador de tentativas, DLQ e — obrigatório — validação de destinatário ANTES de enfileirar (6.381 'sem destino' são bugs de origem, não de envio). EVITAR: 3,8% de falha permanente que ninguém vê.

### 20. O agendador GEP_ tem o desenho certo (catálogo × instância × fila × log × notificação) e está subutilizado e sem alerta

O subsistema é bem modelado: GEP_JOBCAD (71 linhas, catálogo do que o produto sabe fazer: Codigo, Tipo, TIPOJOB, Argumento, ESTIMULOFILA, OBJETIVO, RESTRICOES, PRIORIDADEFILA), GEP_JOBAGD (25 linhas, a instância agendada: Tipo CICLICA/INTERNA, Aplicacao, Argumento, IntervaloExec em minutos, UltimaExecucao, NroTENTATIVA, IntervaloTENTATIVA, DiasVIdALOG, PlanoExec, ExecucaoExclusiva, THREADEXCLUSIVA, NroZUMBI, FK SEQJOB→GEP_JOBCAD), GEP_JOBFILA + GEP_JOBFILALOG (execução sob demanda), GEP_JobAgdExecLog (981.319 linhas, com LINKSTR/LINKNRO1/LINKNRO2 para correlacionar com o registro afetado), GEP_JobMonitor e GEP_JOBNOTIFICAR (notificação por job com granularidade fina: IndENVEMAIL, IndENVSMS, IndENVCRM, IndENVREDESOC, IndEXECUCAO, IndERRO, IndADVERTENCIA, IndGERARESULTADO, Resultado). Só 16 dos 71 jobs do catálogo estão agendados. Dos agendados, MORTOS: COBRANCA_CRIT (21/05/2025), EMAIL_IN_CRM (29/04/2024), PR_INT_PROPROS (18/09/2025), PR_VTC_INTVEICULO (UltimaExecucao NULL, nunca rodou, intervalo 10 min), MOV_PESSOA / SMS_SEND / SYNCSAT_PRODUTOS_GERA (nunca executados). VIVOS: VCRM_RUNIMPORTIMP (5 min), VCRM_RUNGEPIMPORT (20 min, mas falhando), CRMOUT_AGD_SEND (15), EMAIL_SEND (3), GT_STATUS_SEND (30, telemetria para a Vórtice), PR_ATU_FORMPRODTOTVS (diário), PR_ATU_STATUS_DEPTO (diário), VTC_P_GERAAPROVACAO/GERAAGUARDENTREGA/GERACONDPAGTO (5 min), SYNCSAT_PRODUTOS_GERA_AUTO (diário). Não há evidência de GEP_JOBNOTIFICAR configurado disparando para os jobs mortos.

**Evidencia:** SELECT * FROM GEP_JOBCAD (71) e GEP_JOBAGD (25); LEFT JOIN GEP_JOBCAD×GEP_JOBAGD por SeqJOB → 16 agendados; UltimaExecucao de cada; sys.columns de GEP_JOBNOTIFICAR

**Licao para a Tracbel:** COPIAR o modelo catálogo→agendamento→fila→log→notificação, especialmente a correlação do log com o registro afetado (LINKSTR/LINKNRO) e a matriz de notificação por tipo de evento (execução, erro, advertência). ADAPTAR: 'job parado' precisa ser um alerta automático derivado de UltimaExecucao + IntervaloExec (heartbeat/deadman switch), não algo que se descobre 15 meses depois consultando o banco. EVITAR: PlanoExec/Argumento como varchar livre apontando para caminhos de arquivo em servidor (D:\VORTICE\PDI\*.LNK, C:\CLIENTVTC\GLOBAL\*.EXE) — acopla o agendamento ao filesystem de uma máquina específica e foi exatamente o que quebrou na migração de data center.

### 21. A API wsVorticeCrmApi cobre a entrada de leads e o portal, mas NÃO cobre o que o ERP realmente precisa

Comparando docs/API-wsVorticeCrmApi.md com o que os dados mostram: a API TEM endpoints /api/crm/imp/{pessoa, pessoa/contato, pessoa/propriedade, pessoa/fone, email, evento, nfs, nfsitem, nfscmpl, formapgto, os, ositem, ossolic, pedido, pedidoitem, produto, titulo, veiculo} — cobertura nominal de quase toda a superfície IMP_. Mas as tabelas correspondentes estão vazias (IMP_PESSOA, IMP_VEICULO, IMP_PRODUTO, IMP_Evento, IMP_Pedido, IMP_PedidoItem = 0) enquanto as que têm volume (IMP_Titulo 892k, IMP_OS 287k, IMP_NFS 18k) são alimentadas por procedure via linked server, NÃO pela API. O que a API cobre de fato e funciona: webhooks de lead (Facebook /api/facebook/webhook/lead, RD /api/rd/v2/lead/{nroempresa}/{formulario}/{propriedade}, Followize, Infobip SMS, Ativmob, WiFire), portal do cliente, fluxo de cartão/Certiface/Conductor/Neurotech, chat e PABX. O que ela NÃO cobre: (a) leitura de dados do ERP pelo CRM (não há GET de NF/título/OS); (b) escrita do CRM no ERP (não há saída de pedido); (c) criação de usuário do CRM (confirmado em docs/pedido-vortice-api-usuario.md — a criação toca 5-7 tabelas e não tem endpoint); (d) qualquer operação sobre EXT_* (só IMP_*); (e) consulta ao estado das filas/jobs. Há POST /api/crm/view/{nomeview} — leitura genérica de view, que é o escape hatch usado na prática.

**Evidencia:** docs/API-wsVorticeCrmApi.md; contagens SELECT COUNT(*) das IMP_*; corpo de X_P_IMP_CRM_TITULO/X_P_IMP_CRM_NF_POS_VENDA (INSERT direto em IMP_, sem passar por HTTP); docs/pedido-vortice-api-usuario.md

**Licao para a Tracbel:** COPIAR: endpoint por entidade de importação, com o mesmo contrato usado pelo batch (a API e o ETL escrevem no MESMO staging) — isso é bom design, evita dois caminhos divergentes. ADAPTAR: no CRM novo, a API de ingestão deve ser o ÚNICO caminho (nada de INSERT direto por linked server), com idempotency-key no header, validação de contrato na borda e resposta 202 + id de rastreio. EVITAR: publicar endpoints que ninguém usa enquanto o caminho real é um linked server ODBC com credencial de banco — é o pior dos dois mundos em segurança e em rastreabilidade.

## Lacunas declaradas

- Não consegui ver o CÓDIGO do lado ERP: a view TOTVS.TMPRD.dbo.X_V_IMP_CRM_TITULO e X_V_BI_FATURAMENTO_PECAS estão no linked server (base TMPRD do Protheus) e a conta CRM_Leitura não tem acesso a ele. O mapeamento de campos Protheus→CRM que acontece lá dentro é inferido apenas pelos comentários das views X_V_CRM_IMP_* (F2_DOC, F2_SERIE, D2_TES, A3_NOME, E4_COND, B1_POSIPI, C5_NUM, C5_VEND1, BM_CODMAR).
- Não consegui confirmar POR QUE o RUNIMPORTIMP parou de promover IMP_Titulo→EXT_Titulo em 21/05/2025 nem IMP_OS→EXT_OS em 25/05/2024. A correlação com COBRANCA_CRIT (última execução 21/05/2025 16:30) é forte mas não é prova; o log do executável fica em arquivo no app server (C:\VORTICE\TDEV\APPSRV), fora do banco. Seria preciso ler os logs de disco do VRTCSERVER em 10.150.6.230.
- Não consegui verificar se os jobs Pentaho (D:\VORTICE\PDI\KITCHEN_*.LNK, PAN_*.LNK) rodam por fora, via Agendador de Tarefas do Windows — só sei que no agendador do Vórtice a última execução registrada é jul/2021 e que o log APPEXEC tem apenas 15 linhas de 2017. Também não vi os arquivos .ktr/.kjb.
- Sem permissão em msdb.dbo.sysjobs (SELECT negado), não consegui listar os jobs do SQL Server Agent — é possível que X_P_EXE_INTEGRACOES_TOTVS_CRM seja disparada por lá (o que explicaria X_T_IMP_CRM_TITULO estar atualizada até 13/07/2026 sem constar em GEP_JOBAGD). Falta confirmar quem chama essa procedure e por que ela também parou em 13/07/2026.
- Não apurei o domínio exato de StatusIMP. Inferi 'E' = já processado e NULL/vazio = pendente a partir do corte temporal limpo em IMP_Titulo (E até 21/05/2025, vazio a partir de 22/05/2025), mas IMP_OS tem 100% 'E' com 280.214 registros ausentes de EXT_OS, o que contradiz a inferência. Não há catálogo dessa coluna no banco.
- Não apurei o domínio de GEP_SyncUsrSat.IndProcessado (valores 0, 1, 2 e 9 observados) nem de GEP_Import.Status (E e X observados) — não existe tabela de catálogo para nenhum dos dois.
- Não medi o impacto financeiro/operacional real do represamento de OS e NF (só o de títulos, R$ 5,18 bi em VlrOriginal), porque IMP_OS não tem coluna de valor consolidado confiável e o X_TOTVS_CRM_FATURAMENTO pós-11/04/2025 simplesmente não existe (a landing também parou).
- Não confirmei se as 47 linhas de EXT_Pessoa sem SeqPessoa e as origens 'erro' (264) e 'teste' (1) causam falha ativa hoje ou são resíduo histórico — faltou olhar as datas dessas linhas especificamente.
- Não investiguei as views X_CRM_BI_* (X_CRM_BI_CONGLOMERADO, X_CRM_BI_FATURAMENTO_PECA_CARTEIRA, X_CRM_BI_FUNIL_PECA_CARTEIRA, X_CRM_BI_FUNIL_PECA_CLASSE, X_V_BI_DESPESAS_VENDA_MAQUINAS, bi_faturamentos) nem as procedures X_BI_PROSPECCAO_MAQUINAS_* — são a camada de consumo/BI sobre a integração e podem revelar mais regras de mapeamento.
- Não apurei o conteúdo de EXT_VEICREF (SEQPROPRIEDADE, CAMPOORIGEM, CAMPODEST — 40 linhas) e EXT_VEICFAMREF (23 linhas), que parecem ser a tabela de-para entre campos do veículo do ERP e propriedades do CRM (IV_Propriedade). É provavelmente o mecanismo de mapeamento dinâmico de atributos e merecia um exame dedicado.
