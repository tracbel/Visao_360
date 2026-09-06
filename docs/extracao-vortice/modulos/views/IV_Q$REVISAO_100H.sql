/* ==============================================================
   Objeto ..........: dbo.IV_Q$REVISAO_100H
   Tipo ............: VIEW
   Criado em .......: 2025-03-21 09:18:00
   Modificado em ...: 2025-03-21 09:18:00
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_REVISAO_100H, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$REVISAO_100H ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario, DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao, SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , DATA_INICIO_REV_100H, DATA_TERM_REV_100H)    AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO, QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS, QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO, QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.DATA_INICIO_REV_100H, TABF.DATA_TERM_REV_100H   FROM IV_Q_REVISAO_100H TABF 	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO 	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO 	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA  