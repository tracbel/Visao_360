/* ==============================================================
   Objeto ..........: dbo.IV_EQUIPE
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:34
   Modificado em ...: 2025-02-17 17:35:34
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IV_VENDEDOR
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

   CREATE VIEW IV_EQUIPE (SEQEQUIPE, CODEQUIPE, EQUIPE, CODEQUIPEFORA, USUALTEROU, CODEQUIPEFORAX, SEQUSUARIO, SEQUSUARIOLIDER) AS  SELECT V.SEQVENDEDOR AS SEQEQUIPE,         V.CODVENDEDOR AS CODEQUIPE,        V.VENDEDOR AS EQUIPE,        V.CODVENDEDORFORA AS CODEQUIPEFORA,        V.USUALTEROU,        V.CODVENDEDORFORAX AS CODEQUIPEFORAX,        V.SEQUSUARIO,        V.SEQUSUARIOLIDER  FROM IV_VENDEDOR V 