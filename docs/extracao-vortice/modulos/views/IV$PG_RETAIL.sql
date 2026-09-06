/* ==============================================================
   Objeto ..........: dbo.IV$PG_RETAIL
   Tipo ............: VIEW
   Criado em .......: 2019-07-09 13:01:55
   Modificado em ...: 2019-07-09 13:01:55
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_RETAIL ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    VALOR,    TIPO)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.NUMERO1,    IV_GLOBALPAR.LITERAL1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 10