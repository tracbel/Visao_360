/* ==============================================================
   Objeto ..........: dbo.IV_Q$AFERICAO_MAQ_2_WEB
   Tipo ............: VIEW
   Criado em .......: 2020-07-23 22:02:19
   Modificado em ...: 2020-07-23 22:02:19
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_AFERICAO_MAQ_2_WEB, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$AFERICAO_MAQ_2_WEB ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario,  DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao,  SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , UTILIZA_PRODUTO, CONTATO_CONSULTOR)     AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO,  QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS,  QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO,  QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.UTILIZA_PRODUTO, TABF.CONTATO_CONSULTOR    FROM IV_Q_AFERICAO_MAQ_2_WEB TABF  	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO  	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO  	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA 