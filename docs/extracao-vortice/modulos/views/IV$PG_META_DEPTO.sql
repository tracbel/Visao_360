/* ==============================================================
   Objeto ..........: dbo.IV$PG_META_DEPTO
   Tipo ............: VIEW
   Criado em .......: 2025-05-12 12:27:43
   Modificado em ...: 2025-05-12 12:27:43
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_META_DEPTO ( SeqPar,   Parametro,  NroEmpresa,   DtaAlteracao,   UsuAlteracao ,   DEPTO,   POTENCIAL,   META)   AS SELECT  IV_GLOBALPAR.SEQPAR,   IV_GLOBALPARCTRL.PARAMETRO,   IV_GLOBALPAR.NROEMPRESA,   IV_GLOBALPAR.DTAALTERACAO,   IV_GLOBALPAR.USUALTERACAO ,   IV_GLOBALPAR.CAMPO1,   IV_GLOBALPAR.CAMPO2,   IV_GLOBALPAR.NUMERO1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR  WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR        AND IV_GLOBALPARCTRL.SEQGLBPAR = 29 