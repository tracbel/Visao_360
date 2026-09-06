/* ==============================================================
   Objeto ..........: dbo.IV$PG_CAMPANHA_DE_INCENTIV
   Tipo ............: VIEW
   Criado em .......: 2019-07-03 17:14:33
   Modificado em ...: 2019-07-03 17:14:33
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_CAMPANHA_DE_INCENTIV ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    VALOR,    TIPO_DE_INCETIVO,    NOME_DO_INCETIVO)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.NUMERO1,    IV_GLOBALPAR.LITERAL1,    IV_GLOBALPAR.LITERAL2 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 8