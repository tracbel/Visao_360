/* ==============================================================
   Objeto ..........: dbo.IV_Q$CANCELAMENTO_SEGURO
   Tipo ............: VIEW
   Criado em .......: 2024-12-26 17:51:06
   Modificado em ...: 2024-12-26 17:51:06
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_CANCELAMENTO_SEGURO, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$CANCELAMENTO_SEGURO ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario, DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao, SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , SEGURADORA, PRECO, MOTIVO, APOLICE)    AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO, QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS, QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO, QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.SEGURADORA, TABF.PRECO, TABF.MOTIVO, TABF.APOLICE   FROM IV_Q_CANCELAMENTO_SEGURO TABF 	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO 	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO 	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA  