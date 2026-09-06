/* ==============================================================
   Objeto ..........: dbo.PR_VTC_INTMODELO
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-10-31 15:46:20
   Modificado em ...: 2025-10-31 15:46:20
   Linhas ..........: 31
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : EXT_VEICMODELO
   Tabelas referidas: EXT_VEICMODELO
   Outras refs .....: PRC_GET_SEQUENCIA_TABELA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create PROCEDURE PR_VTC_INTMODELO @IDFAMILIA NUMERIC(10), @MODELO VARCHAR(50), @DESCRICAO VARCHAR(50) , @CODIGO NUMERIC(10) OUTPUT
AS
/*			Rotina diaria para atualizar veiculo de integracao
abr/2025 - Amaury
*/
-------------------------------
----- inicio da execução da rotina
Declare 
	@vnSeqModelo numeric(10)

BEGIN

	Set @vnSeqModelo = (Select IDVEICMODELO From  EXT_VEICMODELO
						Where DESCRICAO = @DESCRICAO
						  and MODELO = @MODELO
						  and IDVEICFAMILIA = @IDFAMILIA) 
--- testa se modelo não existe
	if @vnSeqModelo is null
	begin
	--- pega sequencia
		EXEC dbo.PRC_GET_SEQUENCIA_TABELA
			 @sNomeTabela = 'EXT_VEICMODELO',
			 @vnSeqNovo = @vnSeqModelo OUTPUT,
			 @sTabelaOrigem = 'EXT_VEICMODELO',
			 @sCampoOrigem = 'IDVEICMODELO'

	--- insere modelo
		insert into EXT_VEICMODELO ( IDVEICMODELO, IDVEICFAMILIA, MODELO, DESCRICAO ) values (@vnSeqModelo, @IDFAMILIA, @MODELO, @DESCRICAO )
	end
	set @CODIGO = @vnSeqModelo
END	