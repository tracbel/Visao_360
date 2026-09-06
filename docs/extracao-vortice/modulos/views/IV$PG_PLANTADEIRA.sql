/* ==============================================================
   Objeto ..........: dbo.IV$PG_PLANTADEIRA
   Tipo ............: VIEW
   Criado em .......: 2025-07-17 12:09:50
   Modificado em ...: 2025-07-17 12:09:50
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_PLANTADEIRA ( SeqPar,   Parametro,  NroEmpresa,   DtaAlteracao,   UsuAlteracao ,   MARCA,   ANO,   NUMERO_DE_LINHAS,   REVISADO,   DEMONSTRACAO,   MODELO,   TIPO)   AS SELECT  IV_GLOBALPAR.SEQPAR,   IV_GLOBALPARCTRL.PARAMETRO,   IV_GLOBALPAR.NROEMPRESA,   IV_GLOBALPAR.DTAALTERACAO,   IV_GLOBALPAR.USUALTERACAO ,   IV_GLOBALPAR.CAMPO1,   IV_GLOBALPAR.CAMPO6,   IV_GLOBALPAR.NUMERO1,   IV_GLOBALPAR.SIMNAO1,   IV_GLOBALPAR.SIMNAO2,   IV_GLOBALPAR.LITERAL1,   IV_GLOBALPAR.LITERAL2 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR  WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR        AND IV_GLOBALPARCTRL.SEQGLBPAR = 9406 