/* ==============================================================
   Objeto ..........: dbo.IV_Q$OS_GARANTIA
   Tipo ............: VIEW
   Criado em .......: 2025-02-14 15:51:16
   Modificado em ...: 2025-02-14 15:51:16
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_OS_GARANTIA, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$OS_GARANTIA ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario, DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao, SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , NUMERO_OS, DATA_ABERTURA_OS, CHASSI, TIPO_GARANTIA)    AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO, QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS, QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO, QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.NUMERO_OS, TABF.DATA_ABERTURA_OS, TABF.CHASSI, TABF.TIPO_GARANTIA   FROM IV_Q_OS_GARANTIA TABF 	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO 	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO 	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA  