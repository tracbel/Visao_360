/* ==============================================================
   Objeto ..........: dbo.IV$PG_AUDITORIA
   Tipo ............: VIEW
   Criado em .......: 2019-08-14 20:11:28
   Modificado em ...: 2019-08-14 20:11:28
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_AUDITORIA ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    EMPRESA,    VENDEDOR,    ASSISTENTE)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.CAMPO1,    IV_GLOBALPAR.CAMPO2,    IV_GLOBALPAR.CAMPO3 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 12