/* ==============================================================
   Objeto ..........: dbo.IV$S_RFV
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:35:38
   Modificado em ...: 2025-02-17 17:35:38
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: IVS_CARTEIRA, IVS_DEPTO, IVS_DEPTOPOT, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW IV$S_RFV      (  SEQPESSOA, RECENCIA, FRENQUENCIA, VALOR,          CARTEIRA, POTENCIAL, DEPARTAMENTO) AS    SELECT PES.SEQPESSOA       ,PES.REC        AS RECENCIA       ,PES.FREQ       AS FRENQUENCIA       ,PES.VLR        AS VALOR       ,CART.CARTEIRA       ,DPOT.POTENCIAL       ,DEPT.DEPTO FROM   IVS_PES PES JOIN   IVS_DEPTO DEPT ON DEPT.SEQDEPTO = PES.SEQDEPTO JOIN   IVS_DEPTOPOT DPOT ON DPOT.SEQDEPTO = PES.SEQDEPTO                             AND DPOT.SEQPOTENCIALDP = PES.SEQPOTENCIALDP JOIN   IVS_CARTEIRA CART ON CART.SEQCARTEIRA = PES.SEQCARTEIRA   