/* ==============================================================
   Objeto ..........: dbo.IV_Q$GESTAO_CREDITO_IMP
   Tipo ............: VIEW
   Criado em .......: 2022-04-20 15:00:01
   Modificado em ...: 2023-07-27 16:22:33
   Linhas ..........: 15
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_GESTAO_CREDITO_IMP, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$GESTAO_CREDITO_IMP
AS
SELECT TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, QST.Obs AS Observacao, 
                  QST.SeqHistorico, QST.Resultado, QST.Processo, QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.TAXTAXA_FLAT_FLAT, TABF.STATUS_ANALISE_BANCO, TABF.NF_DESCONTO_INCONDIC, 
                  TABF.FORMULARIO_NUMERO, TABF.VALOR_TOTAL, TABF.VALOR_DO_SINAL, TABF.DATA_SINAL, TABF.RECEBIMENTO_USADO, TABF.VALOR_FINANCIADO, TABF.VALOR_USADO, TABF.VALOR_AVALIACAO_USAD, 
                  TABF.BONIFICACAO_DESCONTO, TABF.TIPO_BONIFICACAO_DES, TABF.VALOR_DESCONTO_INCON, TABF.DETALHES_DESCONTO_IN, TABF.SINAL_EMSINAL_EMBUTI, TABF.FORMA_PAGAMENTO, TABF.CODIGO_FINAME, 
                  TABF.INF_FINANCEIRO, TABF.CODIGO_MDA, TABF.NUMERO_AMS, TABF.NUMERO_IMP, TABF.NUMERO, TABF.INSTITUICAO_FIINSTIT, TABF.PROCESSO_STATUS, TABF.NF_REFATURAMENTO, TABF.DADATA_RESPOSTATA_RE, 
                  TABF.DATA_RESPOSTA__1_, TABF.DATA_RESPOSTA__2_, TABF.TAXA_FLAT, TABF.ANALISE_BC_3, TABF.DATA_ENVIO__2_, TABF.DATA_ENVIO__3_, TABF.LOCAL_FATURAMENTO, TABF.FATURAMENTO_REALIZAD, 
                  TABF.NOTA_FISCAL, TABF.INSTITUICAO_FINANCEI, TABF.ANALISE_3, TABF.STATUS_BC_2, TABF.GESTOR___TELEFONE___, TABF.STATUS_ANALISE_BC, TABF.DATA_ENVIO_DATA_ENVI, TABF.ANALISE_2, 
                  TABF.STATUS_PROCESSO, TABF.PROBABILIDADE, TABF.COM_CONTRATO, TABF.RECEBIMENTO_DO_EQUIP
FROM     dbo.IV_Q_GESTAO_CREDITO_IMP AS TABF INNER JOIN
                  dbo.IV_Questionario AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO INNER JOIN
                  dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
                  dbo.GE_Pessoa AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa
