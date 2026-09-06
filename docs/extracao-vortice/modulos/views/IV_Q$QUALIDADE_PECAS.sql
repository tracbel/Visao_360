/* ==============================================================
   Objeto ..........: dbo.IV_Q$QUALIDADE_PECAS
   Tipo ............: VIEW
   Criado em .......: 2015-11-06 10:23:22
   Modificado em ...: 2016-03-22 19:20:03
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_QUALIDADE_PECAS, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$QUALIDADE_PECAS ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario,  DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao,  SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , SATISFEITO_ATEND, MOTIVO, INDICARIA, PRECISA_FAZER)     AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO,  QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS,  QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO,  QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.SATISFEITO_ATEND, TABF.MOTIVO, TABF.INDICARIA, TABF.PRECISA_FAZER    FROM IV_Q_QUALIDADE_PECAS TABF  	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO  	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO  	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA 