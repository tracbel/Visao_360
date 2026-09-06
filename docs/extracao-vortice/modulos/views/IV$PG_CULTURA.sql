/* ==============================================================
   Objeto ..........: dbo.IV$PG_CULTURA
   Tipo ............: VIEW
   Criado em .......: 2025-07-17 15:58:18
   Modificado em ...: 2025-07-17 15:58:18
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_CULTURA ( SeqPar,   Parametro,  NroEmpresa,   DtaAlteracao,   UsuAlteracao ,   CULTURA)   AS SELECT  IV_GLOBALPAR.SEQPAR,   IV_GLOBALPARCTRL.PARAMETRO,   IV_GLOBALPAR.NROEMPRESA,   IV_GLOBALPAR.DTAALTERACAO,   IV_GLOBALPAR.USUALTERACAO ,   IV_GLOBALPAR.LITERAL1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR  WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR        AND IV_GLOBALPARCTRL.SEQGLBPAR = 31 