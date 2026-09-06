/* ==============================================================
   Objeto ..........: dbo.fva_SequenciaGet
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-02-17 17:35:37
   Modificado em ...: 2025-02-17 17:35:37
   Linhas ..........: 1
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : GE_SEQUENCIA
   Tabelas referidas: GE_SEQUENCIA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

 Create Procedure dbo.fva_SequenciaGet ( @Tabela Varchar(40),@Seq Numeric = 0 OUTPUT )  As  Begin    BEGIN TRANSACTION         Set @Seq = (Select count(*)             From  GE_SEQUENCIA            Where NOMETABELA = @Tabela )                If @Seq > 0         Begin            Update GE_SEQUENCIA             Set SEQUENCIA = SEQUENCIA + 1             Where NOMETABELA =  @Tabela       End    Else            Insert Into GE_SEQUENCIA ( NOMETABELA, SEQUENCIA )                   VALUES (  @Tabela, 1 )     Set @Seq = (Select SEQUENCIA            From  GE_SEQUENCIA            Where NOMETABELA = @Tabela )         COMMIT TRANSACTION   End   