/* ==============================================================
   Objeto ..........: dbo.V_C5SYSINDEX
   Tipo ............: VIEW
   Criado em .......: 2025-02-17 17:46:05
   Modificado em ...: 2025-02-17 17:46:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: (nenhuma)
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 CREATE VIEW V_C5SYSINDEX AS   SELECT      sch.name AS TBDONO,      tbl.name AS TBNOME,     idx.name AS IDXNOME,     idx.type_desc AS IDXTYPE,     col.name AS COLNOME,     ic.index_column_id AS COLORDER,     ic.is_included_column AS COLISINCLUDED,     idx.is_primary_key AS IDXISPRIMARYKEY,     idx.is_unique AS IDXISUNIQUE  FROM      sys.indexes idx INNER JOIN      sys.tables tbl ON idx.object_id = tbl.object_id INNER JOIN      sys.schemas sch ON tbl.schema_id = sch.schema_id INNER JOIN      sys.index_columns ic ON idx.object_id = ic.object_id AND idx.index_id = ic.index_id INNER JOIN      sys.columns col ON ic.object_id = col.object_id AND ic.column_id = col.column_id INNER JOIN      sys.objects obj ON tbl.object_id = obj.object_id  