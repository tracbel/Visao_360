/* ==============================================================
   Objeto ..........: dbo.IV$PG_COLHEITADEIRA_GRAOS
   Tipo ............: VIEW
   Criado em .......: 2025-07-17 08:41:24
   Modificado em ...: 2025-07-17 08:41:24
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_COLHEITADEIRA_GRAOS ( SeqPar,   Parametro,  NroEmpresa,   DtaAlteracao,   UsuAlteracao ,   MARCA,   TIPO_DA_PLATAFORMA,   SEPARADOR,   ANO,   REVISADO,   MODELO)   AS SELECT  IV_GLOBALPAR.SEQPAR,   IV_GLOBALPARCTRL.PARAMETRO,   IV_GLOBALPAR.NROEMPRESA,   IV_GLOBALPAR.DTAALTERACAO,   IV_GLOBALPAR.USUALTERACAO ,   IV_GLOBALPAR.CAMPO1,   IV_GLOBALPAR.CAMPO2,   IV_GLOBALPAR.CAMPO3,   IV_GLOBALPAR.CAMPO6,   IV_GLOBALPAR.SIMNAO1,   IV_GLOBALPAR.LITERAL1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR  WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR        AND IV_GLOBALPARCTRL.SEQGLBPAR = 9405 