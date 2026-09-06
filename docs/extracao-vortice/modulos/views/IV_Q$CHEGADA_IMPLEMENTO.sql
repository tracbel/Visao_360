/* ==============================================================
   Objeto ..........: dbo.IV_Q$CHEGADA_IMPLEMENTO
   Tipo ............: VIEW
   Criado em .......: 2013-10-08 17:29:04
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_CHEGADA_IMPLEMENTO, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$CHEGADA_IMPLEMENTO ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario,  DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao,  SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , DTA_CHEGADA_IMPL)     AS SELECT PES.SEQPESSOA, PES.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO,  QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS,  QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO,  QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.DTA_CHEGADA_IMPL    FROM  IV_Q_CHEGADA_IMPLEMENTO TABF  	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO  	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO  	JOIN GE_PESSOA PES ON PES.SEQPESSOA = QST.SEQPESSOA 