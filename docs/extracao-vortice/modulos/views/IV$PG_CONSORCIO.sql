/* ==============================================================
   Objeto ..........: dbo.IV$PG_CONSORCIO
   Tipo ............: VIEW
   Criado em .......: 2016-04-13 11:09:49
   Modificado em ...: 2016-04-13 11:09:49
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_CONSORCIO ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    MARCA,    MODELO)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.CAMPO1,    IV_GLOBALPAR.LITERAL1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 3