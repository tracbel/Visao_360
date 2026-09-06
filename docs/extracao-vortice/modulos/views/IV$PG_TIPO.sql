/* ==============================================================
   Objeto ..........: dbo.IV$PG_TIPO
   Tipo ............: VIEW
   Criado em .......: 2025-01-14 15:28:34
   Modificado em ...: 2025-01-14 15:28:34
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_TIPO ( SeqPar,   Parametro,  NroEmpresa,   DtaAlteracao,   UsuAlteracao ,   TIPO)   AS SELECT  IV_GLOBALPAR.SEQPAR,   IV_GLOBALPARCTRL.PARAMETRO,   IV_GLOBALPAR.NROEMPRESA,   IV_GLOBALPAR.DTAALTERACAO,   IV_GLOBALPAR.USUALTERACAO ,   IV_GLOBALPAR.CAMPO1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR  WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR        AND IV_GLOBALPARCTRL.SEQGLBPAR = 27 