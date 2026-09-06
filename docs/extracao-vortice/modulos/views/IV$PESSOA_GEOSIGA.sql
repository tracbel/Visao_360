/* ==============================================================
   Objeto ..........: dbo.IV$PESSOA_GEOSIGA
   Tipo ............: VIEW
   Criado em .......: 2013-11-14 17:42:58
   Modificado em ...: 2016-03-22 19:20:05
   Linhas ..........: 1
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, IV_ACAO, IV_HISTORICO, IV_RESULTADO
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW IV$PESSOA_GEOSIGA  AS  SELECT PES.SEQPESSOA,   	PES.NOMERAZAO AS CLIENTE,  	HS.CODUSUARIO AS ATENDENTE,  	ACA.DESCRICAO AS ACAO,   	RES.DESCRICAO AS RESULTADO,   	CONVERT(VARCHAR, HS.DTAREALIZACAO, 103) AS DATA    FROM IV_HISTORICO HS  	JOIN GE_PESSOA PES ON PES.SEQPESSOA = HS.SEQPESSOA  	JOIN IV_RESULTADO RES ON RES.RESULTADO = HS.RESULTADO  	JOIN IV_ACAO ACA   		ON ACA.ACAO = HS.ACAOGERADORA   		AND ACA.ACAO = RES.ACAO