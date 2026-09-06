/* ==============================================================
   Objeto ..........: dbo.IV_Q$TESTE1
   Tipo ............: VIEW
   Criado em .......: 2022-08-05 10:06:38
   Modificado em ...: 2022-08-05 10:06:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_TESTE1, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$TESTE1 ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario,  DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao,  SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , MARCA, EQPTO)     AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO,  QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS,  QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO,  QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.MARCA, TABF.EQPTO    FROM IV_Q_TESTE1 TABF  	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO  	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO  	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA 