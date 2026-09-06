/* ==============================================================
   Objeto ..........: dbo.fva_CGCCPFFormat
   Tipo ............: SQL_SCALAR_FUNCTION
   Criado em .......: 2025-02-17 17:35:36
   Modificado em ...: 2025-02-17 17:35:36
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

  create  FUNCTION   dbo.fva_CGCCPFFormat (@pnNroCGCCPF Numeric, @pnDigito Int, @psFisicaJuridica Varchar(1) ) RETURNS varchar(30) AS BEGIN    DECLARE @Retorno varchar(30)        DECLARE @vnTam   int    DECLARE @vsCNPJ varchar(20)    DECLARE @vsDIG varchar(2)    Set @vsCNPJ = CONVERT ( varchar (12),@pnNroCGCCPF)    Set @vsCNPJ = REPLICATE ( '0', 12 - len (@vsCNPJ )) ++ @vsCNPJ    Set @vsDIG = CONVERT ( varchar (2),@pnDigito)    Set @vsDIG = REPLICATE ( '0', 2 - len (@vsDIG )) ++ @vsDIG    IF  @psFisicaJuridica  = 'J'        Set @Retorno = SUBSTRING(@vsCNPJ, 1, 2) ++ '.' ++ SUBSTRING(@vsCNPJ, 3, 3) ++ '.' ++ SUBSTRING(@vsCNPJ, 6, 3)                  ++ '/' ++ SUBSTRING(@vsCNPJ, 9, 4) ++ '-' ++ @vsDIG    ELSE        Set @Retorno = SUBSTRING(@vsCNPJ, 4, 3) ++ '.' ++ SUBSTRING(@vsCNPJ, 7, 3)                   ++ '.' ++ SUBSTRING(@vsCNPJ, 10, 3) ++ '-' ++ @vsDIG    Return (@Retorno) END  