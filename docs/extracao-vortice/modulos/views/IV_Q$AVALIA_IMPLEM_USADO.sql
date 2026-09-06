/* ==============================================================
   Objeto ..........: dbo.IV_Q$AVALIA_IMPLEM_USADO
   Tipo ............: VIEW
   Criado em .......: 2019-11-21 16:20:04
   Modificado em ...: 2019-12-01 18:51:22
   Linhas ..........: 13
   Escreve em tabela: nao
   Tabelas referidas: GE_Pessoa, IV_Formulario, IV_Q_AVALIA_IMPLEM_USADO, IV_Questionario
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$AVALIA_IMPLEM_USADO
AS
SELECT        TBASE.SeqPessoa, TBASE.NomeRazao AS Pessoa, QST.SeqQuestionario, FRM.SeqFormulario, FRM.Descricao AS Formulario, QST.DtaRealizacao, QST.UsuInclusao, QST.DtaAlteracao, QST.UsuAlteracao, 
                         QST.Obs AS Observacao, QST.SeqHistorico, QST.Resultado, QST.Processo, QST.Departamento, QST.LinkDocto, QST.LinkNro, QST.LinkSerie, TABF.MARCA_IMP_USADO, TABF.MODELO_IMP_USADO, TABF.ANO_IMP_USADO, 
                         TABF.VALOR_IMP_USADO, TABF.CHASSI_IMP_USADO, TABF.MOTOR_IMP, TABF.BOMBA_HIDRAULICA, TABF.ENGATE, TABF.BARRA_DE_TRACAO, TABF.APOIO_DE_ENGATE, TABF.QTD_DISCO, TABF.ESTADO_DOS_DISCOS, 
                         TABF.QUANTIDADE_DE_HASTES, TABF.ESTADO_HASTES, TABF.CALCOS_DE_PROFUNDIDA, TABF.MANCAIS, TABF.MANGUEIRAS, TABF.CARACOL, TABF.QTD_BICOS, TABF.SUPORTE_FILTROS, TABF.REGULADORES, 
                         TABF.TURBINA, TABF.CABOS_CONTROLE, TABF.BARRAS, TABF.ESTADO_TANQUE, TABF.ESTEIRAS, TABF.CORREIAS, TABF.RECOLHEDOR, TABF.MONITORES, TABF.PINTURA, TABF.PNEUS_DIANTEIRO, 
                         TABF.ESTADO_DOS_PNEUS, TABF.PNEUS_TRASEIRO, TABF.RETIRADA_USADO, TABF.VALOR_FINAL_AVA
FROM            dbo.IV_Q_AVALIA_IMPLEM_USADO AS TABF INNER JOIN
                         dbo.IV_Questionario AS QST ON QST.SeqQuestionario = TABF.SEQQUESTIONARIO INNER JOIN
                         dbo.IV_Formulario AS FRM ON FRM.SeqFormulario = QST.SeqFormulario INNER JOIN
                         dbo.GE_Pessoa AS TBASE ON TBASE.SeqPessoa = QST.SeqPessoa
