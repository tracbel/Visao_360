/* ==============================================================
   Objeto ..........: dbo.IV_Q$AFERICAO_SERV_WEB
   Tipo ............: VIEW
   Criado em .......: 2020-05-13 14:25:27
   Modificado em ...: 2020-05-14 16:40:50
   Linhas ..........: 11
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_AFERICAO_SERV_WEB, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$AFERICAO_SERV_WEB
AS
SELECT        TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, 
                         QST.Obs AS Observacao, QST.SeqHistorico, QST.Resultado, QST.Processo, QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.OS_PRAZOS_AGENDADOS_, TABF.AGENDAMENTO_ENTREGA, 
                         TABF.EXPECTATIVA_TECNICO, TABF.SERV_CORRETAMENTE, TABF.Q005_1, TABF.Q005_2, TABF.Q005_3, TABF.Q005_4, TABF.Q005_5, TABF.Q005_6, TABF.Q005_7, TABF.Q005_8, TABF.Q005_9, TABF.Q005_, 
                         TABF.NOTANPS
FROM            dbo.IV_Q_AFERICAO_SERV_WEB AS TABF INNER JOIN
                         dbo.IV_Questionario AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO INNER JOIN
                         dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
                         dbo.GE_Pessoa AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa
