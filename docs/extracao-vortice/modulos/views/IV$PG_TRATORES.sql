/* ==============================================================
   Objeto ..........: dbo.IV$PG_TRATORES
   Tipo ............: VIEW
   Criado em .......: 2024-12-16 14:58:17
   Modificado em ...: 2024-12-16 14:58:17
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_TRATORES ( SeqPar,   Parametro,  NroEmpresa,   DtaAlteracao,   UsuAlteracao ,   MARCA,   FAIXA_DE_POTENCIA,   ANO,   REVISADO,   DEMONSTRACAO,   MODELO)   AS SELECT  IV_GLOBALPAR.SEQPAR,   IV_GLOBALPARCTRL.PARAMETRO,   IV_GLOBALPAR.NROEMPRESA,   IV_GLOBALPAR.DTAALTERACAO,   IV_GLOBALPAR.USUALTERACAO ,   IV_GLOBALPAR.CAMPO1,   IV_GLOBALPAR.CAMPO2,   IV_GLOBALPAR.CAMPO6,   IV_GLOBALPAR.SIMNAO1,   IV_GLOBALPAR.SIMNAO2,   IV_GLOBALPAR.LITERAL1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR  WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR        AND IV_GLOBALPARCTRL.SEQGLBPAR = 9401 