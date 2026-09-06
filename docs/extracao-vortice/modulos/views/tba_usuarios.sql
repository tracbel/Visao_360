/* ==============================================================
   Objeto ..........: dbo.tba_usuarios
   Tipo ............: VIEW
   Criado em .......: 2025-02-12 12:13:11
   Modificado em ...: 2026-01-29 13:30:51
   Linhas ..........: 13
   Escreve em tabela: nao
   Tabelas referidas: GE_Usuario, IV_Operador
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE view [dbo].[tba_usuarios] as
select
usr.SeqUsuario,
usr.CodUsuario,
usr.TipoUsuario,
op.Departamento,
op.gerente,
op.[Status],
op.Supervisor
from GE_Usuario usr
join IV_Operador op 
on op.SeqUsuario = usr.SeqUsuario;
