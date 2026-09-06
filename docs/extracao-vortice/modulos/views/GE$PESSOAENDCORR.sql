/* ==============================================================
   Objeto ..........: dbo.GE$PESSOAENDCORR
   Tipo ............: VIEW
   Criado em .......: 2011-12-19 11:48:38
   Modificado em ...: 2016-03-22 19:20:04
   Linhas ..........: 39
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOAEND
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

Create view GE$PESSOAENDCORR (  SEQPESSOA,  NOMERAZAO, ENDCORRESPOND, CORRESPONDENCIA, STATUS, SEXO,
        FISICAJURIDICA, EMAIL, TIPOENDERECO, TIPOLOGRADOURO, LOGRADOURO,
        NROLOGRADOURO, CMPLTOLOGRADOURO, ENDERECO, BAIRRO,
        CIDADE, UF, CEP)
 AS 
SELECT PES.SEQPESSOA,
       PES.NOMERAZAO, 
       case when PDR.SEQPESSOAEND is null then 0 else 1 end AS ENDCORRESPOND, 
       isnull ( PES.CORRESPONDENCIA, 0) AS CORRESPONDENCIA, 
       PES.STATUS,
       PES.SEXO,
       PES.FISICAJURIDICA,
       PES.EMAIL,
       isnull (PDR.TIPOENDERECO, 'B' ) AS TIPOENDERECO, 
       case when PDR.SEQPESSOAEND  is NULL
		  then PES.TIPOLOGRADOURO Else PDR.TIPOLOGRADOURO end AS TIPOLOGRADOURO  , 
       case when PDR.SEQPESSOAEND is NULL
		  then PES.LOGRADOURO Else PDR.LOGRADOURO End AS LOGRADOURO, 
       case when PDR.SEQPESSOAEND is NULL
		  then PES.NROLOGRADOURO Else PDR.NROLOGRADOURO End AS NROLOGRADOURO, 
       Case when PDR.SEQPESSOAEND   Is Null
		  then PES.CMPLTOLOGRADOURO Else PDR.CMPLTOLOGRADOURO End AS CMPLTOLOGRADOURO ,
       Case when PDR.SEQPESSOAEND   Is Null 
           then isnull (PES.TIPOLOGRADOURO, '') ++' '++ isnull(PES.LOGRADOURO,'') ++' '++ isnull (PES.NROLOGRADOURO,'') ++' '++ isnull(PES.CMPLTOLOGRADOURO,'')
		   Else isnull (PDR.TIPOLOGRADOURO,'') ++' '++ isnull(PDR.LOGRADOURO,'') ++' '++ isnull (PDR.NROLOGRADOURO, '')  ++' '++ isnull(PDR.CMPLTOLOGRADOURO, '') 
           End  AS ENDERECO , 
       Case when PDR.SEQPESSOAEND is Null 
		  then PES.BAIRRO Else PDR.BAIRRO End AS BAIRRO, 
       Case when PDR.SEQPESSOAEND is Null
		  then PES.CIDADE Else PDR.CIDADE End AS CIDADE ,
       Case when PDR.SEQPESSOAEND is Null
		  then PES.UF Else PDR.UF End AS UF, 
       Case when PDR.SEQPESSOAEND is Null
		  then PES.CEP Else PDR.CEP End AS CEP  
FROM GE_PESSOA PES 
   LEFT JOIN  GE_PESSOAEND PDR 
	   on PES.SEQPESSOA = PDR.SEQPESSOA  
	  AND PDR.TIPOENDERECO   = 'R'
