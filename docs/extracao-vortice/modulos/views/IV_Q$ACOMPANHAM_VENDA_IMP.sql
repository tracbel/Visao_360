/* ==============================================================
   Objeto ..........: dbo.IV_Q$ACOMPANHAM_VENDA_IMP
   Tipo ............: VIEW
   Criado em .......: 2021-06-09 16:51:00
   Modificado em ...: 2022-08-26 10:17:50
   Linhas ..........: 16
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_ACOMPANHAM_VENDA_IMP, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

-- dbo.IV_Q$ACOMPANHAM_VENDA_IMP source

CREATE VIEW dbo.IV_Q$ACOMPANHAM_VENDA_IMP
AS
SELECT TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, QST.Obs AS Observacao, 
                  QST.SeqHistorico, QST.Resultado, QST.Processo, QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.MODELO_IMPLEMENTO, TABF.VALOR_TOTAL_IMPLEM, TABF.TIPO_PAGTO_IMPL, TABF.AGENCIA_BANC, 
                  TABF.NOME_GER_BANCO, TABF.TEL_AGENCIA_BANCO, TABF.NUM_PED_FABRICA, TABF.TIP_IMPLEMENTO, TABF.PROBAB_APR_CREDITO, TABF.DATA_PREV_FATURAM, TABF.ORIG_FATUR_IMPL, TABF.MARCA_IMPLEMENTO, 
                  TABF.VALOR_SINAL_IMPL, TABF.VALOR_FINAN_IMPL, TABF.NUM_PED_VENDA, TABF.NUM_CHASSI_IMPL, TABF.INST_FINANC, TABF.VALOR_REC_PROP, TABF.NRO_CONTRATO, TABF.NRO_DO_PAC, TABF.EMAIL_CONT, 
                  TABF.DESCR_DET_DIMENS, TABF.NUMERO_NOTAFISCAL, TABF.NUMERO_AGENCIA, TABF.COD_FINAME_MDA_CONSO, TABF.DATA_DO_SINAL, TABF.INFORMACOES_PARA_O_F, TABF.INFO_AGREGA_DESAGREG, 
                  TABF.DATA_COMBINADA_CLIEN, TABF.DTA_PEDIDO, TABF.BONIFICACAO_DESCONTO, TABF.VALOR_BONIFICACAO_DE, TABF.USADO_NA_NEGOCIACAO, TABF.COR, TABF.ANO_IMPLEMENTO, TABF.VENDA_IMPLEMENTO, 
                  TABF.SINAL_EMBUTIDO, TABF.LOCAL_DE_FATURAMENTO
FROM     
			dbo.IV_Q_ACOMPANHAM_VENDA_IMP (nolock) 	AS TABF 
INNER JOIN	dbo.IV_Questionario (nolock) 			AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO 
INNER JOIN 	dbo.IV_Formulario (nolock)				AS FRM ON FRM.SeqFormulario = QST.SeqFormulario 
INNER JOIN	dbo.GE_Pessoa  (nolock) 				AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa;