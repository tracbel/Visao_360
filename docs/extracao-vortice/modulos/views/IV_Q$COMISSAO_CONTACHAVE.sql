/* ==============================================================
   Objeto ..........: dbo.IV_Q$COMISSAO_CONTACHAVE
   Tipo ............: VIEW
   Criado em .......: 2020-03-19 09:06:44
   Modificado em ...: 2020-12-14 13:13:03
   Linhas ..........: 11
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_COMISSAO_CONTACHAVE, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$COMISSAO_CONTACHAVE
AS
SELECT TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, QST.Obs AS Observacao, QST.SeqHistorico, QST.Resultado, QST.Processo, 
             QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.VALOR_VENDA, TABF.CEN, TABF.QUANTIDADE, TABF.Q004_TRATORES_LINHA5, TABF.Q004_TRATORES_LINHA6, TABF.Q004_TRATORES_LINHA7, TABF.Q004_TRATORES_LINHA8, 
             TABF.Q004_COLHEDORAS, TABF.Q004_COLHEITADEIRAS_, TABF.Q004_GREEN_SYSTEM, TABF.Q004_PLANTADEIRAS, TABF.Q004_PULVERIZADORES, TABF.VALOR_COMISSAO, TABF.PREMIO_PAGO, TABF.ESPECIALISTA_VENDA, TABF.PREMIO_ESPECIALISTA, 
             TABF.EQUIPAMENTO_AMS_FULL
FROM   dbo.IV_Q_COMISSAO_CONTACHAVE AS TABF INNER JOIN
             dbo.IV_Questionario AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO INNER JOIN
             dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
             dbo.GE_Pessoa AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa
