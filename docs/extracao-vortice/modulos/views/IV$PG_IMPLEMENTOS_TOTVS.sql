/* ==============================================================
   Objeto ..........: dbo.IV$PG_IMPLEMENTOS_TOTVS
   Tipo ............: VIEW
   Criado em .......: 2022-08-12 08:44:00
   Modificado em ...: 2022-08-12 08:44:00
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_IMPLEMENTOS_TOTVS ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao )   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO  FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 19