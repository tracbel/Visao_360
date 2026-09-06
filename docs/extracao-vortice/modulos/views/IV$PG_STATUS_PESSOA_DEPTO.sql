/* ==============================================================
   Objeto ..........: dbo.IV$PG_STATUS_PESSOA_DEPTO
   Tipo ............: VIEW
   Criado em .......: 2018-03-23 14:16:03
   Modificado em ...: 2018-03-23 14:16:03
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_STATUS_PESSOA_DEPTO ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    QTDE_MESES__MAQ,    QTDE_MESES__PECAS,    QTDE_MESES__SERVICO,    QTDE_MESES__EQUIP)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.NUMERO1,    IV_GLOBALPAR.NUMERO2,    IV_GLOBALPAR.NUMERO3,    IV_GLOBALPAR.NUMERO4 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 6