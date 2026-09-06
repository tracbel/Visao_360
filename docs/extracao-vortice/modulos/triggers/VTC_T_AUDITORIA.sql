/* ==============================================================
   Objeto ..........: dbo.VTC_T_AUDITORIA
   Tipo ............: SQL_TRIGGER
   Tabela pai ......: IV_ClientePropr
   Criado em .......: 2019-08-15 10:30:16
   Modificado em ...: 2019-08-15 16:38:50
   Linhas ..........: 261
   Escreve em tabela: SIM (INSERT, UPDATE, DELETE)
   Alvos de escrita : gep_import
   Tabelas referidas: GE_EMPRESA, gep_import, IV_Agenda, IV_GlobalPar, IV_ProcLink, IV_Propriedade
   Outras refs .....: f_s_dado, f_s_dado_N
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE TRIGGER [dbo].[VTC_T_AUDITORIA]
    ON [dbo].[IV_ClientePropr]
    AFTER INSERT, UPDATE, DELETE
AS

Declare 

/*
##########################################
##########################################
##Criada por André Rizzatti (15/08/2019)##
##########################################
##########################################
*/

--Dados da tabela iv_clientepropr

	@SeqPropPessoa numeric(18, 0),
	@SeqPessoa numeric(10, 0),
	@SeqPropriedade numeric(4, 0),
	@Referencia varchar(30),
	@Identificador varchar(30),
	@Ativo char(1),
	@Notas varchar(250),
	@Campo1 varchar(40),
	@Campo2 varchar(40),
	@Campo3 varchar(40),
	@Campo4 varchar(40),
	@Campo5 varchar(40),
	@Campo6 varchar(40),
	@Numero1 decimal(15, 2),
	@Numero2 decimal(15, 2),
	@Numero3 decimal(15, 2),
	@Numero4 decimal(15, 2),
	@Numero5 decimal(15, 2),
	@Numero6 decimal(15, 2),
	@Data1 datetime,
	@Data2 datetime,
	@Data3 datetime,
	@Data4 datetime,
	@Data5 datetime,
	@Data6 datetime,
	@SimNao1 numeric(1, 0),
	@SimNao2 numeric(1, 0),
	@SimNao3 numeric(1, 0),
	@SimNao4 numeric(1, 0),
	@SimNao5 numeric(1, 0),
	@SimNao6 numeric(1, 0),
	@Literal1 varchar(40),
	@Literal2 varchar(40),
	@Literal3 varchar(40),
	@Literal4 varchar(40),
	@Literal5 varchar(40),
	@Literal6 varchar(40),
	@Literal7 varchar(40),
	@Literal8 varchar(40),
	@Literal9 varchar(40),
	@Literal10 varchar(40),
	@CodOrigem varchar(20),
	@UltOrigem varchar(20),
	@DtaInclusao datetime,
	@UsuInclusao varchar(20),
	@DtaAlteracao datetime,
	@UsuAlteracao varchar(20),
	@CAMPO7 varchar(40),
	@CAMPO8 varchar(40),
	@CAMPO7SQL numeric(1, 0),
	@CAMPO8SQL numeric(1, 0),

--Utilizados no corpo
	@vsAssistente	 varchar(20),
	@vnNroEmpresa	 numeric,
	@vnProcesso		 numeric,
	@vsAcao			 varchar(20),
	@vsPropriedade	 varchar(20),
	@vbExiste		 int,

	--Dados das tabelas IMPORT
	@processo	varchar(20),
	@origem 	varchar(20),
	@acao		varchar(1),
	@tabela		varchar(30),
	@separador	varchar(1),
	@colunaidentific 	varchar(100),
	@dadoidentificador	varchar(200),
	@campos		varchar(1000),
	@dados		varchar(1000),
	@dtageracao datetime,
	@conteudo	varchar(250)

BEGIN
    SET NOCOUNT ON;

	--Dados DELETANDO
    IF NOT EXISTS(SELECT * FROM INSERTED)
        BEGIN 
			PRINT 'DELETANDO'
		END 

	--Dados INSERT/UPDATE
    ELSE 
        BEGIN 
			PRINT 'INSERT/UPDATE' 

			SELECT @SeqPropPessoa  = I.SeqPropPessoa 
				  ,@SeqPessoa      = I.SeqPessoa     
				  ,@SeqPropriedade = I.SeqPropriedade
				  ,@Referencia     = I.Referencia    
				  ,@Identificador  = I.Identificador 
				  ,@Ativo          = I.Ativo         
				  ,@Notas          = I.Notas         
				  ,@Campo1         = I.Campo1        
				  ,@Campo2         = I.Campo2        
				  ,@Campo3         = I.Campo3        
				  ,@Campo4         = I.Campo4        
				  ,@Campo5         = I.Campo5        
				  ,@Campo6         = I.Campo6        
				  ,@Numero1        = I.Numero1       
				  ,@Numero2        = I.Numero2       
				  ,@Numero3        = I.Numero3       
				  ,@Numero4        = I.Numero4       
				  ,@Numero5        = I.Numero5       
				  ,@Numero6        = I.Numero6       
				  ,@Data1          = I.Data1         
				  ,@Data2          = I.Data2         
				  ,@Data3          = I.Data3         
				  ,@Data4          = I.Data4         
				  ,@Data5          = I.Data5         
				  ,@Data6          = I.Data6         
				  ,@SimNao1        = I.SimNao1       
				  ,@SimNao2        = I.SimNao2       
				  ,@SimNao3        = I.SimNao3       
				  ,@SimNao4        = I.SimNao4       
				  ,@SimNao5        = I.SimNao5       
				  ,@SimNao6        = I.SimNao6       
				  ,@Literal1       = I.Literal1      
				  ,@Literal2       = I.Literal2      
				  ,@Literal3       = I.Literal3      
				  ,@Literal4       = I.Literal4      
				  ,@Literal5       = I.Literal5      
				  ,@Literal6       = I.Literal6      
				  ,@Literal7       = I.Literal7      
				  ,@Literal8       = I.Literal8      
				  ,@Literal9       = I.Literal9      
				  ,@Literal10      = I.Literal10     
				  ,@CodOrigem      = I.CodOrigem     
				  ,@UltOrigem      = I.UltOrigem     
				  ,@DtaInclusao    = I.DtaInclusao   
				  ,@UsuInclusao    = I.UsuInclusao   
				  ,@DtaAlteracao   = I.DtaAlteracao  
				  ,@UsuAlteracao   = I.UsuAlteracao  
				  ,@CAMPO7         = I.CAMPO7        
				  ,@CAMPO8         = I.CAMPO8        
				  ,@CAMPO7SQL      = I.CAMPO7SQL     
				  ,@CAMPO8SQL      = I.CAMPO8SQL    
				FROM inserted I	
								
				--Get número da empresa e assistente do vendedor que alterou a propriedade
				SELECT @vsAssistente = GPAR.Campo3,
					   @vnNroEmpresa = EMP.NroEmpresa
				  FROM IV_GlobalPar GPAR 
						JOIN GE_EMPRESA EMP ON EMP.NOMEREDUZIDO = GPAR.CAMPO1
				WHERE 1 = 1 
				AND GPAR.SeqGlbPar = 12 --Auditoria
				AND GPAR.Campo2 = @UsuAlteracao

				--Verificando se existe agenda de auditoria em aberto para a pessoa
				SELECT @vbExiste = COUNT(*)
				  FROM IV_Agenda AGD
					JOIN IV_ProcLink PLK 
						ON PLK.Processo = AGD.Processo
					   AND PLK.LinkNro = @SeqPropPessoa
					   AND PLK.LinkDocto = 'AUDIT'
				WHERE SeqPessoa = @SeqPessoa
				AND AGD.Acao = 358 --Validação da Propriedade
				AND AGD.Realizada = 'N'

				
				If (Len(@vsAssistente) > 0 And	@vbExiste = 0 And				
					(     @SeqPropriedade = 9400  --Origem da Receita
						Or @SeqPropriedade = 9402 --Trator 
						Or @SeqPropriedade = 9406 --Implemento
						Or @SeqPropriedade = 9411 --Colhedora
						Or @SeqPropriedade = 9414 --NJUR					
					))
					Begin
					
							---Identificando se esta atualizando ou incluindo a propriedade
							IF EXISTS(SELECT * FROM INSERTED)  AND EXISTS(SELECT * FROM DELETED) 
								BEGIN 
									Set @vsAcao = 'atualizado(a)' 
								END    
							ELSE 
								BEGIN
									Set @vsAcao = 'inserido(a)'
								END

							--Verificando o nome da propriedade
							SELECT @vsPropriedade = PROPRIEDADE
							FROM IV_Propriedade
							WHERE SeqPropriedade = @SeqPropriedade

							---Identificando se esta atualizando ou incluindo a propriedade
					
							set @campos = 'NROEMPRESA|SEQPESSOA|ORIGEM|EVENTO|DTAREALIZACAO|DETALHE|VENDEDOR|CODUSUARIO|LINKDOCTO|LINKNRO|LINKNROEMPRESA'
							set @processo 	= 'VORTICOCRM'
							set @origem = 'Import'
							set @acao	= 'I'
							set @tabela	= 'IV_HISTORICO'
							set @separador	= '|'
							set @colunaidentific 	= 'SEQPESSOA'
							set @dadoidentificador	= cast ( @SeqPessoa as varchar(19) )
							set @dtageracao	= DATEADD(MINUTE, -15, getdate())

							set @dados = ''
							set @dados = cast (@vnNroEmpresa as varchar(2))

							set @conteudo	= dbo.f_s_dado( @SeqPessoa )
							set @dados  	= @dados + RTrim(@conteudo)

							set @conteudo	= dbo.f_s_dado( 'AUDIT' )
							set @dados  	= @dados + RTrim(@conteudo)

							set @conteudo	= dbo.f_s_dado( 'AUDITORIA' )
							set @dados  	= @dados + RTrim(@conteudo)

							set @conteudo = dbo.f_s_dado( cast (convert(varchar(8), @DtaAlteracao, 112) as varchar(10) ) + ' 00:00:00' )
							set @dados  	= @dados + RTrim(@conteudo)
							

--							set @conteudo	= dbo.f_s_dado( @vsPropriedade + ' de identificação: (' + IsNull(@Identificador, '**Não informado**')  + ') ' + @vsAcao + ' ' + convert(varchar(12), @DtaAlteracao, 103)  + ' pelo Vendedor ***' + @UsuAlteracao + '***' + ' (assistente ' + @vsAssistente + ')')
							set @conteudo	= dbo.f_s_dado( @vsPropriedade + ' de identificação: (' + IsNull(@Identificador, '**Não informado**')  + ') ' + @vsAcao + ' ' + CONVERT(VARCHAR(12), @DtaAlteracao, 103) + ' ' + CONVERT(VARCHAR(8), @DtaAlteracao, 108)  + ' pelo Vendedor ***' + @UsuAlteracao + '***' + ' (assistente ' + @vsAssistente + ')')
							set @dados	= @dados + isNULL(RTrim(@conteudo), '')

							set @conteudo	= dbo.f_s_dado( @UsuAlteracao ) --vendedor (VENDEDOR)
							set @dados  	= @dados + RTrim(@conteudo)

							set @conteudo	= dbo.f_s_dado( @UsuAlteracao ) --usuario (CODUSUARIO)
							set @dados  	= @dados + RTrim(@conteudo)

							set @conteudo	= dbo.f_s_dado( 'AUDIT' )
							set @dados  	= @dados + RTrim(@conteudo)
						
							set @conteudo	= dbo.f_s_dado_N( isnull ( @SeqPropPessoa, ' ' ) )
							set @dados  	= @dados + @conteudo

							set @conteudo	= dbo.f_s_dado_N( isnull ( @vnNroEmpresa, ' ' ) )
							set @dados  	= @dados + @conteudo
						

							insert into gep_import (processo, origem, acao, tabela, separador, coluna, dado, colunaidentific, dadoidentificador, dtageracao) 
								values (@processo, 'AUDIT', @acao, @tabela, @separador, @campos, @dados, @colunaidentific, @dadoidentificador, @dtageracao )
					
						
					End --GEP_IMPORT										  					
		END --Dados INSERT/UPDATE


END --BEGIN INICIAL
