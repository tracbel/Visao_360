/* ==============================================================
   Objeto ..........: dbo.IV$PG_PLATAFORMA_ADICIONAL
   Tipo ............: VIEW
   Criado em .......: 2017-02-07 16:02:38
   Modificado em ...: 2017-02-07 16:02:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_GLOBALPAR, IV_GLOBALPARCTRL
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW dbo.IV$PG_PLATAFORMA_ADICIONAL ( SeqPar,    Parametro,   NroEmpresa,    DtaAlteracao,    UsuAlteracao ,    MARCA,    TIPO_DA_PLATAFORMA,    TAMANHO_PLATAFORMA,    REVISADO,    MODELO)   AS SELECT  IV_GLOBALPAR.SEQPAR,    IV_GLOBALPARCTRL.PARAMETRO,    IV_GLOBALPAR.NROEMPRESA,    IV_GLOBALPAR.DTAALTERACAO,    IV_GLOBALPAR.USUALTERACAO ,    IV_GLOBALPAR.CAMPO1,    IV_GLOBALPAR.CAMPO2,    IV_GLOBALPAR.NUMERO1,    IV_GLOBALPAR.SIMNAO1,    IV_GLOBALPAR.LITERAL1 FROM IV_GLOBALPARCTRL, IV_GLOBALPAR   WHERE IV_GLOBALPARCTRL.SEQGLBPAR = IV_GLOBALPAR.SEQGLBPAR         AND IV_GLOBALPARCTRL.SEQGLBPAR = 9412