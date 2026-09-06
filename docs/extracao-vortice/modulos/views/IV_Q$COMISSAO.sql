/* ==============================================================
   Objeto ..........: dbo.IV_Q$COMISSAO
   Tipo ............: VIEW
   Criado em .......: 2017-06-07 15:33:15
   Modificado em ...: 2021-10-04 11:26:37
   Linhas ..........: 14
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_COMISSAO, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$COMISSAO
AS
SELECT TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, QST.Obs AS Observacao, 
                  QST.SeqHistorico, QST.Resultado, QST.Processo, QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.VALOR_DO_EQUIPAMENTO, TABF.DESCONTO_INCONDICION, TABF.VALOR_DO_EQUIP__USAD, 
                  TABF.VALOR_BASE_COMISSAO, TABF.__COMISSAO__RH_, TABF.VALOR_COMISSAO, TABF.COMISSAO_PAGA___RH_, TABF.LINHA, TABF.VLR_COMISSAO, TABF.PERC_COMISSAO, TABF.ACELERADOR___RISCO__, 
                  TABF.ACELERADOR___MARGEM_, TABF.Q013_AGRICULTURA_DE_, TABF.Q013_CONTRATOS_DE_PE, TABF.Q013_TREINAMENTO, TABF.Q013_VENDA_1__TRATOR, TABF.VENDA_DE_PACOTE_OU_P, TABF.UTILIZACAO_DE_MARGEM, 
                  TABF.VALOR_DE_VENDA_SEM_I, TABF.ESPECIALISTA_NA_VEND, TABF.ESPECIALISTA_VENDA_A, TABF.CAMPANHA_PROMOCIONAL, TABF.VLR_TOTAL_COMISSAO, TABF.POSSUI_ACEL_PROD_PAC, TABF.SINAL_EMBUTIDO, 
                  TABF.VLR_COMISSAO_ESPECIA, TABF.VLR_DESCONTO_INCO, TABF.COMISSAO_DESCONTO_IN, TABF.MARGEM_LUCRO_OPE, TABF.VLR_COMISSAO_CEN, TABF.MARGEM_LUCRO_OPERACI, TABF.VALOR_COMISSAO_ESP, 
                  TABF.VALOR_COMISSAO_CEN, TABF.VLR_COMISSAO_ADC_ESP, TABF.VLR_COMISSAO_ADC_CEN
FROM     dbo.IV_Q_COMISSAO AS TABF INNER JOIN
                  dbo.IV_Questionario AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO INNER JOIN
                  dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
                  dbo.GE_Pessoa AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa
