/* ==============================================================
   Objeto ..........: dbo.IV_Q$COMISSAO_AMS
   Tipo ............: VIEW
   Criado em .......: 2020-08-04 09:49:55
   Modificado em ...: 2021-11-29 10:51:35
   Linhas ..........: 12
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_COMISSAO_AMS, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$COMISSAO_AMS
AS
SELECT TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, QST.Obs AS Observacao, 
                  QST.SeqHistorico, QST.Resultado, QST.Processo, QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.VALOR_DO_AMS, TABF.CEN, TABF.ESPECIALISTA_AMS, TABF.__COMISSAO_CEN, 
                  TABF.VALOR_COMISSAO_CEN, TABF.__COMISSAO_ESPECIALI, TABF.VALORCOMISSAO_ESPECI, TABF.COMISSAO_PAGA, TABF.VALOR_FINAL_DA_COMIS, TABF.OBSERVACAO_GERAL, TABF.VLR_COMISSAO_CEN, 
                  TABF.VLR_COMISSAO_ESPECIA, TABF.VLR_TOTAL_COMISSAO, TABF.SINAL_EMBUTIDO, TABF.COMISSAO__ESPECIAL, TABF.VALORCOMISSAO_CEN, TABF.VALORCOMISSAO_ESPEC, TABF.MARGEM_LUCRO, 
                  TABF.COMISSAO__CEN
FROM     dbo.IV_Q_COMISSAO_AMS AS TABF INNER JOIN
                  dbo.IV_Questionario AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO INNER JOIN
                  dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
                  dbo.GE_Pessoa AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa
