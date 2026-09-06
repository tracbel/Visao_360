/* ==============================================================
   Objeto ..........: dbo.V_UTIL_TELEFONES_CONTATOS
   Tipo ............: VIEW
   Criado em .......: 2025-08-26 14:30:33
   Modificado em ...: 2025-09-11 13:06:21
   Linhas ..........: 156
   Escreve em tabela: nao
   Tabelas referidas: GE_Contato, GE_Pessoa
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE   VIEW V_UTIL_TELEFONES_CONTATOS AS

WITH QR1 AS (
	SELECT		SeqPessoa									AS Seq,
				Max(DT_MAX)									AS DT_MAX
	FROM(		SELECT		SeqPessoa						AS SeqPessoa,
							Max(DT_MAX)						AS DT_MAX
				FROM(		SELECT 		gp.SeqPessoa		AS SeqPessoa,
										Max(DTAINCLUSAO)	AS DT_MAX
							FROM		GE_Pessoa			gp
							GROUP BY	gp.SeqPessoa
							UNION ALL
							SELECT 		gp.SeqPessoa		AS SeqPessoa,
										Max(DtaAlteracao)	AS DT_MAX
							FROM		GE_Pessoa			gp
							GROUP BY	gp.SeqPessoa
				) A
				GROUP BY	SeqPessoa
				UNION ALL
				SELECT		SeqPessoa						AS SeqPessoa,
							Max(DT_MAX)						AS DT_MAX
				FROM(		SELECT 		gc.SeqPessoa		AS SeqPessoa,
										Max(DTAINCLUSAO)	AS DT_MAX
							FROM		GE_Contato			gc
							GROUP BY	gc.SeqPessoa
							UNION ALL
							SELECT 		gc.SeqPessoa		AS SeqPessoa,
										Max(DtaAlteracao)	AS DT_MAX
							FROM		GE_Contato			gc
							GROUP BY	gc.SeqPessoa
				) B
				GROUP BY	SeqPessoa
	) C
	GROUP BY	SeqPessoa
)

SELECT	main.*,
		QR1.DT_MAX
FROM(
	SELECT		CONCAT(gp.SEQPESSOA, '|0')										AS	Chave,
				gp.SEQPESSOA													AS	Seq,
				0																AS	Seq_Contato,
				gp.NomeRazao													AS	Nome,
				gp.NomeRazao													AS	CONTATO,
				'Cliente'														AS	ORIGEM,
				gp.FoneDDD1														AS	FoneDDD1,
				gp.FoneNro1														AS	FoneNro1,
				-----------------------------------------------------------------------------------
				CASE WHEN gp.FoneNro1 IS NOT NULL AND gp.FoneNro1 > 10000000
						THEN (
							CASE WHEN gp.FoneDDD1 IS NOT NULL AND LEN(TRIM(gp.FoneDDD1)) > 1
									THEN CONCAT(TRIM(gp.FoneDDD1), ' - ', gp.FoneNro1)
									ELSE CAST(gp.FoneNro1 AS varchar) END )
						ELSE NULL 	END 										AS	DDD_FONE_1,
				-----------------------------------------------------------------------------------
				CASE WHEN gp.FoneNro1 IS NOT NULL AND gp.FoneNro1 > 10000000
						THEN (
							CASE WHEN gp.FoneDDD1 IS NOT NULL AND LEN(TRIM(gp.FoneDDD1)) > 1
									THEN CONCAT('55', TRIM(gp.FoneDDD1), gp.FoneNro1)
									ELSE CONCAT('55', gp.FoneNro1) END )
						ELSE NULL 	END 										AS	DDI_DDD_FONE_1,
				-----------------------------------------------------------------------------------
				gp.FoneDDD2														AS	FoneDDD2,
				gp.FoneNro2														AS	FoneNro2,
				-----------------------------------------------------------------------------------
				CASE WHEN gp.FoneNro2 IS NOT NULL AND gp.FoneNro2 > 10000000
						THEN (
							CASE WHEN gp.FoneDDD2 IS NOT NULL AND LEN(TRIM(gp.FoneDDD2)) > 1
									THEN CONCAT(TRIM(gp.FoneDDD2), ' - ', gp.FoneNro2)
									ELSE CAST(gp.FoneNro2 AS varchar) END )
						ELSE NULL 	END 										AS	DDD_FONE_2,
				-----------------------------------------------------------------------------------
				CASE WHEN gp.FoneNro2 IS NOT NULL AND gp.FoneNro2 > 10000000
						THEN (
							CASE WHEN gp.FoneDDD2 IS NOT NULL AND LEN(TRIM(gp.FoneDDD2)) > 1
									THEN CONCAT('55', TRIM(gp.FoneDDD2), gp.FoneNro2)
									ELSE CONCAT('55', gp.FoneNro2) END )
						ELSE NULL 	END 										AS	DDI_DDD_FONE_2,
				-----------------------------------------------------------------------------------
				gp.FoneDDD3									AS	FoneDDD3,
				gp.FoneNro3									AS	FoneNro3,
				-----------------------------------------------------------------------------------
				CASE WHEN gp.FoneNro3 IS NOT NULL AND gp.FoneNro3 > 10000000
						THEN (
							CASE WHEN gp.FoneDDD3 IS NOT NULL AND LEN(TRIM(gp.FoneDDD3)) > 1
									THEN CONCAT(TRIM(gp.FoneDDD3), ' - ', gp.FoneNro3)
									ELSE CAST(gp.FoneNro3 AS varchar) END )
						ELSE NULL 	END 										AS	DDD_FONE_3,
				-----------------------------------------------------------------------------------
				CASE WHEN gp.FoneNro3 IS NOT NULL AND gp.FoneNro3 > 10000000
						THEN (
							CASE WHEN gp.FoneDDD3 IS NOT NULL AND LEN(TRIM(gp.FoneDDD3)) > 1
									THEN CONCAT('55', TRIM(gp.FoneDDD3), gp.FoneNro3)
									ELSE CONCAT('55', gp.FoneNro3) END )
						ELSE NULL 	END 										AS	DDI_DDD_FONE_3,
				-----------------------------------------------------------------------------------
				gp.Email														AS	Email,
				gp.DtaInclusao													AS	DtaInclusao,
				gp.DtaAlteracao													AS	DtaAlteracao
	FROM		GE_Pessoa	gp
	-----------------------------------------------------------------------------------------------
	UNION
	-----------------------------------------------------------------------------------------------
	SELECT		CONCAT(gp.SEQPESSOA, '|',  gc.SeqContato)						AS	Chave,
				gp.SEQPESSOA													AS	Seq,
				gc.SeqContato													AS	Seq_Contato,
				gp.NomeRazao													AS	Nome,
				gc.Contato														AS	CONTATO,
				'Contato'														AS	ORIGEM,
				gc.FoneDDD1														AS	FoneDDD1,
				gc.FoneNro1														AS	FoneNro1,
				-----------------------------------------------------------------------------------
				CASE WHEN gc.FoneNro1 IS NOT NULL AND gc.FoneNro1 > 10000000
						THEN (
							CASE WHEN gc.FoneDDD1 IS NOT NULL AND LEN(TRIM(gc.FoneDDD1)) > 1
									THEN CONCAT(TRIM(gc.FoneDDD1), ' - ', gc.FoneNro1)
									ELSE CAST(gc.FoneNro1 AS varchar) END )
						ELSE NULL 	END 										AS	DDD_FONE_1,
				-----------------------------------------------------------------------------------
				CASE WHEN gc.FoneNro1 IS NOT NULL AND gc.FoneNro1 > 10000000
						THEN (
							CASE WHEN gc.FoneDDD1 IS NOT NULL AND LEN(TRIM(gc.FoneDDD1)) > 1
									THEN CONCAT('55', TRIM(gc.FoneDDD1), gc.FoneNro1)
									ELSE CONCAT('55', gc.FoneNro1) END )
						ELSE NULL 	END 										AS	DDI_DDD_FONE_1,
				-----------------------------------------------------------------------------------
				gc.FoneDDD2														AS	FoneDDD2,
				gc.FoneNro2														AS	FoneNro2,
				-----------------------------------------------------------------------------------
				CASE WHEN gc.FoneNro2 IS NOT NULL AND gc.FoneNro2 > 10000000
						THEN (
							CASE WHEN gc.FoneDDD2 IS NOT NULL AND LEN(TRIM(gc.FoneDDD2)) > 1
									THEN CONCAT(TRIM(gc.FoneDDD2), ' - ', gc.FoneNro2)
									ELSE CAST(gc.FoneNro2 AS varchar) END )
						ELSE NULL 	END 										AS	DDD_FONE_2,
				-----------------------------------------------------------------------------------
				CASE WHEN gc.FoneNro2 IS NOT NULL AND gc.FoneNro2 > 10000000
						THEN (
							CASE WHEN gc.FoneDDD2 IS NOT NULL AND LEN(TRIM(gc.FoneDDD2)) > 1
									THEN CONCAT('55', TRIM(gc.FoneDDD2), gc.FoneNro2)
									ELSE CONCAT('55', gc.FoneNro2) END )
						ELSE NULL 	END 										AS	DDI_DDD_FONE_2,
				-----------------------------------------------------------------------------------
				NULL															AS	FoneDDD3,
				NULL															AS	FoneNro3,
				NULL															AS	DDD_FONE_3,
				NULL															AS	DDI_DDD_FONE_3,
				gc.Email														AS	Email,
				gc.DTAINCLUSAO													AS	DtaInclusao,
				gc.DtaAlteracao													AS	DtaAlteracao
	FROM		GE_Contato			gc
	--LEFT JOIN	GE_Pessoa			gp		ON	 	gc.SEQPESSOA 	=	gp.SeqPessoa
	INNER JOIN	GE_Pessoa			gp		ON	 	gc.SEQPESSOA 	=	gp.SeqPessoa		/*	Alteração feita pois existem registroos na tabela GE_Contato com SeqPessoa que n existe na tabela GE_Pessoa	*/
) main
LEFT JOIN	QR1		ON		QR1.Seq = main.Seq
;