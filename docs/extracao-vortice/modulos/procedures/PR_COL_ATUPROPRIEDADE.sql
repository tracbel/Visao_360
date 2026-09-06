/* ==============================================================
   Objeto ..........: dbo.PR_COL_ATUPROPRIEDADE
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2024-12-27 19:59:49
   Modificado em ...: 2024-12-27 19:59:49
   Linhas ..........: 114
   Escreve em tabela: SIM (INSERT)
   Alvos de escrita : IV_Propriedade, GE_UsuarioPerm
   Tabelas referidas: GE_UsuarioPerm, IV_CLIENTEPROPR_ITA, IV_Propriedade, IV_Propriedade_ita
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

create procedure [dbo].[PR_COL_ATUPROPRIEDADE] 
AS

/* 13/11/2024 
  Rotina para inserir propriedade baseado nas propriedades da Noroeste
*/
BEGIN

			INSERT INTO IV_Propriedade 
			SELECT case when [SeqPropriedade]<100 then [SeqPropriedade] + 9600 else [SeqPropriedade] + 100 end
				  ,case when Pcte='JDE' 
				        then SUBSTRING('NO-' + [Propriedade], 1, 19) + '+' 
						else SUBSTRING('NO-' + [Propriedade], 1, 20) end
				  ,[Pcte]
				  ,[Nivel]
				  ,[NivelIdentificador]
				  ,[CampoListar]
				  ,[MostraRef]
				  ,[Usacomplemento]
				  ,[TipoRefVinculado]
				  ,[UmPorPessoa]
				  ,[ReferenciaSQL]
				  ,SUBSTRING([Campo1], 1, 20)
				  ,[Campo1Sql]
				  ,SUBSTRING([Campo2], 1, 20)
				  ,[Campo2Sql]
				  ,SUBSTRING([Campo3], 1, 20)
				  ,[Campo3Sql]
				  ,SUBSTRING([Campo4], 1, 20)
				  ,[Campo4Sql]
				  ,SUBSTRING([Campo5], 1, 20)
				  ,[Campo5Sql]
				  ,SUBSTRING([Campo6], 1, 20)
				  ,[Campo6Sql]
				  ,SUBSTRING([Numero1], 1, 20)
				  ,SUBSTRING([Numero2], 1, 20)
				  ,SUBSTRING([Numero3], 1, 20)
				  ,SUBSTRING([Numero4], 1, 20)
				  ,SUBSTRING([Numero5], 1, 20)
				  ,SUBSTRING([Numero6], 1, 20)
				  ,SUBSTRING([Data1],  1, 20)
				  ,SUBSTRING([Data2],  1, 20)
				  ,SUBSTRING([Data3],  1, 20)
				  ,SUBSTRING([Data4],  1, 20)
				  ,SUBSTRING([Data5],  1, 20)
				  ,SUBSTRING([Data6],  1, 20)
				  ,[SimNao1]
				  ,[SimNao2]
				  ,[SimNao3]
				  ,[SimNao4]
				  ,[SimNao5]
				  ,[SimNao6]
				  ,SUBSTRING([Literal1], 1, 20)
				  ,SUBSTRING([Literal2], 1, 20)
				  ,SUBSTRING([Literal3], 1, 20)
				  ,SUBSTRING([Literal4], 1, 20)
				  ,SUBSTRING([Literal5], 1, 20)
				  ,SUBSTRING([Literal6], 1, 20)
				  ,SUBSTRING([Literal7], 1, 20)
				  ,SUBSTRING([Literal8], 1, 20)
				  ,SUBSTRING([Literal9], 1, 20)
				  ,SUBSTRING([Literal10], 1, 20)
				  ,[DtaInclusao]
				  ,[UsuInclusao]
				  ,[DtaAlteracao]
				  ,[UsuAlteracao]
				  ,[ReferenciaDesc]
				  ,[Literal1Sql]
				  ,[Literal2Sql]
				  ,[Literal3Sql]
				  ,[Literal4Sql]
				  ,[Literal5Sql]
				  ,[Literal6Sql]
				  ,[Literal7Sql]
				  ,[Literal8Sql]
				  ,[Literal9Sql]
				  ,[Literal10Sql]
				  ,[CAMPO7]
				  ,[CAMPO8]
				  ,[CAMPO7SQL]
				  ,[CAMPO8SQL]
				  FROM IV_Propriedade_ita
			WHERE SeqPropriedade IN 
			( SELECT DISTINCT SEQPROPRIEDADE FROM IV_CLIENTEPROPR_ITA PPP )
			AND case when [SeqPropriedade]<100 then [SeqPropriedade] + 9600 else [SeqPropriedade] + 100 end
			NOT IN ( SELECT DISTINCT SEQPROPRIEDADE FROM IV_PROPRIEDADE )


			/* dando permissão para usuario todos para visualização da propriedade nova
			*/
			INSERT INTO GE_UsuarioPerm
			SELECT   XA.CODAPLICACAO
					,XA.CHAVEAPLICACAO
					,XA.SEQUSUARIO
					,XA.NROEMPRESA
					,XA.PERMISSAO
			FROM (
			select 'IV_PROPRIEDADE' AS CODAPLICACAO,
				   case when [SeqPropriedade]<100 then [SeqPropriedade] + 9600 else [SeqPropriedade] + 100 end AS CHAVEAPLICACAO,
				   1 AS SEQUSUARIO,
				   0 AS NROEMPRESA,
				   4 AS PERMISSAO
			FROM IV_Propriedade_ita xx
			WHERE xx.SeqPropriedade IN ( SELECT DISTINCT SEQPROPRIEDADE FROM IV_CLIENTEPROPR_ITA PPP)
			) XA
			WHERE 1=1
			and not exists ( select 1 from ge_usuarioperm 
			                 where CodAplicacao = 'IV_PROPRIEDADE'
							   AND sequsuario = 1
				               and ChaveAplicacao = xa.CHAVEAPLICACAO)


END
