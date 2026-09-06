/* ==============================================================
   Objeto ..........: dbo.IV_Q$DEMO_MAQUINAS
   Tipo ............: VIEW
   Criado em .......: 2020-05-04 19:52:35
   Modificado em ...: 2020-05-17 17:19:25
   Linhas ..........: 11
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_DEMO_MAQUINAS, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$DEMO_MAQUINAS
AS
SELECT        TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, 
                         QST.Obs AS Observacao, QST.SeqHistorico, QST.Resultado, QST.Processo, QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.ENDERECO_ENTREGA, TABF.TIPO_EQUIP, TABF.OPERACAO_DEMO, 
                         TABF.DATA_IDEAL, TABF.APROVAR_GERENTE, TABF.CULTURAS, TABF.AREA_TRABALHADA, TABF.POTENCIAL_DE_COMPRA, TABF.NF_DEMONSTRACAO, TABF.RESULTADAO_DEMONSTRA, TABF.MODELO_EQUIPA, 
                         TABF.DATA_AGENDADA, TABF.DATA_REALIZADA, TABF.TIPO_EQUIPAMENTO, TABF.DATA_NOTA
FROM            dbo.IV_Q_DEMO_MAQUINAS AS TABF INNER JOIN
                         dbo.IV_Questionario AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO INNER JOIN
                         dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
                         dbo.GE_Pessoa AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa
