/* ==============================================================
   Objeto ..........: dbo.IV_Q$EXP_FLUXO_MODELER
   Tipo ............: VIEW
   Criado em .......: 2024-07-25 14:06:03
   Modificado em ...: 2024-07-25 14:06:03
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_EXP_FLUXO_MODELER, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$EXP_FLUXO_MODELER ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario, DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao, SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , NUMERO, NOME)    AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO, QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS, QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO, QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.NUMERO, TABF.NOME   FROM IV_Q_EXP_FLUXO_MODELER TABF 	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO 	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO 	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA  