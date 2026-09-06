/* ==============================================================
   Objeto ..........: dbo.IV$PG_MARCAS_E_FAMILIA
   Tipo ............: VIEW
   Criado em .......: 2023-01-24 10:37:00
   Modificado em ...: 2023-01-24 10:37:00
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_MARCAS_E_FAMILIA ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    MARCA,    FAMILIA,    TIPO__MAQ_OU_IMPL)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.LITERAL1,    IV_GLOBALPAR.LITERAL2,    IV_GLOBALPAR.LITERAL3 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 24