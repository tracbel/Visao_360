/* ==============================================================
   Objeto ..........: dbo.IV_Q$APRESENT_IMPLEMENTO
   Tipo ............: VIEW
   Criado em .......: 2013-10-08 14:23:25
   Modificado em ...: 2016-03-22 19:20:03
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_APRESENT_IMPLEMENTO, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$APRESENT_IMPLEMENTO ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario,  DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao,  SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , Q001_EQUIP, Q001_EVENTOS, Q001_FOLD, Q001_VIS_CL_RE, QUAL_EV, QUAL_CLI)     AS SELECT PES.SEQPESSOA, PES.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO,  QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS,  QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO,  QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.Q001_EQUIP, TABF.Q001_EVENTOS, TABF.Q001_FOLD, TABF.Q001_VIS_CL_RE, TABF.QUAL_EV, TABF.QUAL_CLI    FROM  IV_Q_APRESENT_IMPLEMENTO TABF  	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO  	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO  	JOIN GE_PESSOA PES ON PES.SEQPESSOA = QST.SEQPESSOA 