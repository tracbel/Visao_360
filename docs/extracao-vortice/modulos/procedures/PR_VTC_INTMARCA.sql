/* ==============================================================
   Objeto ..........: dbo.PR_VTC_INTMARCA
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-10-31 15:43:41
   Modificado em ...: 2025-10-31 15:43:41
   Linhas ..........: 29
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : EXT_VEICMARCA
   Tabelas referidas: EXT_VEICMARCA
   Outras refs .....: PRC_GET_SEQUENCIA_TABELA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create PROCEDURE PR_VTC_INTMARCA @NROEMPRESA NUMERIC(10), @MARCA VARCHAR(20) , @CODIGO NUMERIC(10) OUTPUT
AS
/*			Rotina diaria para atualizar veiculo de integracao
abr/2025 - Amaury
*/
-------------------------------
----- inicio da execução da rotina
Declare 
	@vnSeqMarca numeric(10)

BEGIN

	Set @vnSeqMarca = (Select idveicmarca From  EXT_VEICMARCA 
						Where MARCA = @MARCA
						  and NROEMPRESAPADRAO = @NROEMPRESA) 
--- testa se marca não existe
	if @vnSeqMarca is null
	begin
	--- pega sequencia
		EXEC dbo.PRC_GET_SEQUENCIA_TABELA
			 @sNomeTabela = 'EXT_VEICMARCA',
			 @vnSeqNovo = @vnSeqMarca OUTPUT,
			 @sTabelaOrigem = 'EXT_VEICMARCA',
			 @sCampoOrigem = 'IDVEICMARCA'
	--- insere marca
		insert into EXT_VEICMARCA ( IDVEICMARCA, MARCA, NROEMPRESAPADRAO ) values (@vnSeqMarca, @MARCA, @NROEMPRESA )
	end
	set @CODIGO = @vnSeqMarca
END	