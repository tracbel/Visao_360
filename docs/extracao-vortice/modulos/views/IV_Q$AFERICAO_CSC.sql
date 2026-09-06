/* ==============================================================
   Objeto ..........: dbo.IV_Q$AFERICAO_CSC
   Tipo ............: VIEW
   Criado em .......: 2021-03-02 22:18:46
   Modificado em ...: 2021-03-02 22:18:46
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_FORMULARIO, IV_Q_AFERICAO_CSC, IV_QUESTIONARIO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV_Q$AFERICAO_CSC ( SeqPessoa, Pessoa, SeqQuestionario, SeqFormulario, Formulario,  DtaRealizacao, UsuInclusao, DtaAlteracao, UsuAlteracao,  Observacao,  SeqHistorico, Resultado, Processo, Departamento, LinkDocto, LinkNro, LinkSerie , VOCE_RECEBEU_EM_SEU_, VOCE_CHEGOU_A_ABRIR_, SE_SIM__DE_1_A_5_QUE, VOCE_RECEBEU_ALGUM_T, EM_UMA_ESCALA_DE_MUI)     AS SELECT TBASE.SEQPESSOA, TBASE.NOMERAZAO, QST.SEQQUESTIONARIO, FRM.SEQFORMULARIO, FRM.DESCRICAO,  QST.DTAREALIZACAO, QST.USUINCLUSAO, QST.DTAALTERACAO, QST.USUALTERACAO, QST.OBS,  QST.SEQHISTORICO, QST.RESULTADO, QST.PROCESSO, QST.DEPARTAMENTO,  QST.LINKDOCTO, QST.LINKNRO, QST.LINKSERIE , TABF.VOCE_RECEBEU_EM_SEU_, TABF.VOCE_CHEGOU_A_ABRIR_, TABF.SE_SIM__DE_1_A_5_QUE, TABF.VOCE_RECEBEU_ALGUM_T, TABF.EM_UMA_ESCALA_DE_MUI    FROM IV_Q_AFERICAO_CSC TABF  	JOIN IV_QUESTIONARIO QST ON QST.SEQQUESTIONARIO =  TABF.SEQQUESTIONARIO  	JOIN IV_FORMULARIO FRM ON   FRM.SEQFORMULARIO = QST.SEQFORMULARIO  	JOIN GE_PESSOA TBASE ON TBASE.SEQPESSOA = QST.SEQPESSOA 