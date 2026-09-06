/* ==============================================================
   Objeto ..........: dbo.IV$PG_ANO
   Tipo ............: VIEW
   Criado em .......: 2024-08-29 15:14:13
   Modificado em ...: 2024-08-29 15:14:13
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_ANO ( SeqPar,   Parametro,  NroEmpresa,   DtaAlteracao,   UsuAlteracao ,   ANO)   AS SELECT  IV_GLOBALPAR.SEQPAR,   IV_GLOBALPARCTRL.PARAMETRO,   IV_GLOBALPAR.NROEMPRESA,   IV_GLOBALPAR.DTAALTERACAO,   IV_GLOBALPAR.USUALTERACAO ,   IV_GLOBALPAR.CAMPO6 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR  WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR        AND IV_GLOBALPARCTRL.SEQGLBPAR = 9414 