/* ==============================================================
   Objeto ..........: dbo.ZZ_IV$OrdemServico
   Tipo ............: VIEW
   Criado em .......: 2012-05-31 16:06:13
   Modificado em ...: 2017-01-25 12:07:27
   Linhas ..........: 44
   Escreve em tabela: nao
   Tabelas referidas: EXT_OS, EXT_OSITEM, GE_PESSOA
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[IV$OrdemServico] ( IDOS, NROEMPRESA,
       NROOS, SEQPESSOA, 
       NROCHASSI, PLACA, 
       COMBUSTIVEL, 
       CODVEICULO, 
       MODELO, 
       CORVEICULO, 
       ANOFABRICACAO, ANOMODELO, 
       DTAVENDA, CONSULTOR, TIPOOS, 
       DTAABERTURA, DTAENCERRAMENTO, DTAFECHAMENTO, 
       VALORLIQPECAS, 
	   VALORDESCPECAS,
       VALORLIQSERVICO, 
	   VALORDESCSERV,
       OBSERVACAO,
       DEPARTAMENTO, ORIGEM, NRODN, 
       KILOMETRAGEM )
AS 
SELECT OS.NROOS AS IDOS, OS.NROEMPRESA,
       OS.NROOS, (SELECT MIN(SEQPESSOA) FROM GE_PESSOA WHERE GE_PESSOA.NROCGCCPF = OS.NROCGCCPF ) AS SEQPESSOA,
       OS.NROCHASSI, OS.PLACA,
       CASE OS.COMBUSTIVEL
       WHEN 'G' THEN 'Gasolina'
       WHEN 'A' THEN 'Álcool'
       WHEN 'F' THEN 'Flex'
       WHEN 'D' THEN 'Diesel'
       ELSE OS.COMBUSTIVEL END AS COMUBUSTIVEL,
       OS.CODVEICULO AS CODVEICULO,
       OS.MODELO AS MODELO,
       OS.CORVEICULO AS CORVEICULO,
       OS.ANOFABRICACAO, OS.ANOMODELO,
       OS.DTAVENDA, OS.CONSULTOR, OS.TIPOOS,
       OS.DTAABERTURA, OS.DTAENCERRAMENTO, OS.DTAFECHAMENTO,
       0 AS VALORLIQPECAS, 
	   0 AS VALORDESCPECAS,
       ( SELECT SUM (IT.VALOR) FROM EXT_OSITEM IT WHERE IT.IDOS = OS.IDOS AND IT.TIPOITEM = 'S'  ) AS VALORLIQSERVICO, 
	   0 AS VALORDESCSERV,
       OS.OBSERVACAO,
       OS.DEPARTAMENTO,
       OS.ORIGEM, 
       OS.NRODN,
       OS.KILOMETRAGEM        
FROM EXT_OS OS 
