/* ==============================================================
   Objeto ..........: dbo.GEL$CIDADE
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GEL_CEP, GEL_CIDADE
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE  VIEW GEL$CIDADE ( SEQCID, CEP,  CIDADE, UF, SEQCIDADE)   AS   SELECT CID.SEQCIDADE,         CEP.CEP,        CID.CIDADE,        CID.UF,        CID.SEQCIDADE FROM GEL_CIDADE CID     LEFT OUTER JOIN GEL_CEP CEP on CEP.SEQCIDADE = CID.SEQCIDADE        AND CEP.CEPCIDADE = 1  