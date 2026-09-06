/* ==============================================================
   Objeto ..........: dbo.IV_Q$PROSPECCAO_SERV__JD
   Tipo ............: VIEW
   Criado em .......: 2019-08-08 14:29:09
   Modificado em ...: 2019-08-08 14:29:09
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_PROSPECCAO_SERV__JD, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$PROSPECCAO_SERV__JD ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario,  DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao,  SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , ORCAMENTO, CONDICAO_DE_PAGAMENT, ORDEM_DE_SERVICO, VALOR)     AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO,  QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS,  QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO,  QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.ORCAMENTO, TABF.CONDICAO_DE_PAGAMENT, TABF.ORDEM_DE_SERVICO, TABF.VALOR    FROM IV_Q_PROSPECCAO_SERV__JD TABF  	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO  	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO  	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA 