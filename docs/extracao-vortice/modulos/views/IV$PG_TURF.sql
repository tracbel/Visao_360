/* ==============================================================
   Objeto ..........: dbo.IV$PG_TURF
   Tipo ............: VIEW
   Criado em .......: 2017-02-03 17:26:20
   Modificado em ...: 2017-02-03 17:26:20
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_TURF ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    MARCA,    REVISADO,    MODELO,    TIPO)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.CAMPO1,    IV_GLOBALPAR.SIMNAO1,    IV_GLOBALPAR.LITERAL1,    IV_GLOBALPAR.LITERAL2 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 9410