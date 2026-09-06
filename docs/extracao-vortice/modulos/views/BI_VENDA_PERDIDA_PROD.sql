/* ==============================================================
   Objeto ..........: dbo.BI_VENDA_PERDIDA_PROD
   Tipo ............: VIEW
   Criado em .......: 2022-11-25 11:15:42
   Modificado em ...: 2023-04-19 11:07:19
   Linhas ..........: 68
   Escreve em tabela: nao
   Tabelas referidas: IV_HISTORICO, IV_PROCDADO, IV_PROCESSO, IV_RESULTADO
   Outras refs .....: iv_q$venda_perdida, iv_q$venda_perdida_prod
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE view [dbo].[BI_VENDA_PERDIDA_PROD] AS 
select seqquestionario,
       dtarealizacao AS VPERD_DATA,			
       observacao AS VPERD_OBS,				
       processo AS ONE_PROCESSO,			
       marca  AS VPERD_marca_vp,			
       revenda  AS VPERD_revenda,			
       modelo  AS VPERD_modelo_vp,			
       data_da_venda  AS VPERD_data_da_venda,		
       RIGHT( CAST ( YEAR( data_da_venda ) AS VARCHAR(4) ), 2)  + '/' + 
       RIGHT( '0' + CAST ( MONTH( data_da_venda ) AS VARCHAR(2)), 2) AS VPERD_MES_da_venda,	
       quantidade  AS VPERD_quantidade,			
       preco_concorrente  AS VPERD_preco_concorrente,	
       preco_john_deere  AS VPERD_preco_NOSSO,		
       modelo_john_deere  AS VPERD_modelo_john_deer,	
       motiva  AS VPERD_motivo,				
       tipo_equipamento  AS VPERD_tipo_de_maquina,	
	   PARTICIPAMOS_DA_NEGO as VPERD_particip_negociacao,	
	   null as VPERD_havia_monitoramento,	
       (SELECT MAX(HST.DETALHE)
         FROM IV_HISTORICO HST
             JOIN IV_RESULTADO RES ON RES.RESULTADO = HST.RESULTADO
                AND ( UPPER(RES.DESCRICAO) LIKE '%PERD%' OR UPPER(RES.DESCRICAO) LIKE '%DESIST%' )
         WHERE HST.PROCESSO = iv_q$venda_perdida_prod.PROCESSO
           AND LEN (HST.DETALHE) > 10 ) AS VPERD_DETALHE	
  from iv_q$venda_perdida_prod
 WHERE PROCESSO IN (SELECT PDD.PROCESSO
                      FROM IV_PROCDADO PDD
                      JOIN IV_PROCESSO PRC
                        ON PRC.PROCESSO = PDD.PROCESSO
                     WHERE PDD.CODPROCESSO IN (7, 37)
                      )

UNION

select seqquestionario,
       dtarealizacao AS VPERD_DATA,			
       observacao AS VPERD_OBS,				
       processo AS ONE_PROCESSO,			
       marca_vp  AS VPERD_marca_vp,			
       revenda  AS VPERD_revenda,			
       modelo_vp  AS VPERD_modelo_vp,			
       data_da_venda  AS VPERD_data_da_venda,		
       RIGHT( CAST ( YEAR( data_da_venda ) AS VARCHAR(4) ), 2)  + '/' + 
       RIGHT( '0' + CAST ( MONTH( data_da_venda ) AS VARCHAR(2)), 2) AS VPERD_MES_da_venda,	
       quantidade  AS VPERD_quantidade,			
       preco_concorrente  AS VPERD_preco_concorrente,	
       preco_john_deere  AS VPERD_preco_NOSSO,		
       modelo_john_deer  AS VPERD_modelo_john_deer,	
       motivo  AS VPERD_motivo,				
       tipo_de_equipamento  AS VPERD_tipo_de_maquina,	
	   PARTICIPAMOS_DA_NEGO as VPERD_particip_negociacao,	
	   HAVIA_MONITORAMENTO as VPERD_havia_monitoramento,	
       (SELECT MAX(HST.DETALHE)
         FROM IV_HISTORICO HST
             JOIN IV_RESULTADO RES ON RES.RESULTADO = HST.RESULTADO
                AND ( UPPER(RES.DESCRICAO) LIKE '%PERD%' OR UPPER(RES.DESCRICAO) LIKE '%DESIST%' )
         WHERE HST.PROCESSO = iv_q$venda_perdida.PROCESSO
           AND LEN (HST.DETALHE) > 10 ) AS VPERD_DETALHE	
  from iv_q$venda_perdida
 WHERE PROCESSO IN (SELECT PDD.PROCESSO
                      FROM IV_PROCDADO PDD
                      JOIN IV_PROCESSO PRC
                        ON PRC.PROCESSO = PDD.PROCESSO
                     WHERE PDD.CODPROCESSO IN (7, 37)
                      )

