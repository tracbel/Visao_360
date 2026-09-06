/* ==============================================================
   Objeto ..........: dbo.X_CRM_BI_FATURAMENTO_PECA_CARTEIRA
   Tipo ............: VIEW
   Criado em .......: 2023-08-15 13:22:20
   Modificado em ...: 2023-08-15 13:22:20
   Linhas ..........: 140
   Escreve em tabela: nao
   Tabelas referidas: GE_Empresa, GE_Pessoa, GE_Usuario, IVS_CARTEIRA, IVS_Depto, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE     VIEW  [X_CRM_BI_FATURAMENTO_PECA_CARTEIRA] AS
    
	select 
        a.Carteira_pessoa	as seq_carteira,
        Case 
            When a.Carteira_pessoa is null 
                then 'Sem Carteira'
                else 'Com Carteira' end 
                            as carteira,

        b.Carteira			as cod_carteira,
        b.Descricao			as nome_carteira,
        B.SeqUsrResp		as seq_cip,
        c.NomeReduzido		as nome_cip,
        D.Fantasia			as filial,
        substring(D.Fantasia, 9, len(D.Fantasia)) 
                            as empresa,
        a.status_pessoa,					
        a.cnpj_pessoa		as seq_pessoa,
        a.seq_pessoa		as cod_pessoa,
        a.nome_pessoa		as cliente,
        a.cidade_pessoa,
        a.tipo_pessoa		as tipo_pessoa,
        a.cnpj_conglo		as seq_conglo,
        a.seq_conglo		as cod_conglo,
        a.cidade_conglo,
        a.cliente_conglo	as grupo_conglo,
        --a.tipo_conglo,
        a.principal

    From (

    select 
        cnpj_pessoa,
        DigCGCCPF,
        NroCGCCPF,
        status_pessoa,
        seq_pessoa,
        nome_pessoa,
        tipo_pessoa,

        cidade_pessoa,

        cnpj_conglo,
        seq_conglo,
        status_conglo,

        cidade_conglo,

        cliente_conglo,
        tipo_conglo,

        case 
            when Carteira_pessoa is not null 
                then 'S'
                else principal end 
                                as principal,
        --Carteira_pessoa,
        --Carteira_Conglomerado,
        case 	
            when Carteira_pessoa is null 
                then Carteira_Conglomerado
                else Carteira_pessoa end 
                                as Carteira_pessoa
    from (


    --consulta banco de dados para cadastro / carteira do conglomerado
    select 
         CASE 
        WHEN LEN(a.DigCGCCPF) = 1 THEN 
            CAST(a.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(a.DigCGCCPF AS CHAR(2)) 
        WHEN LEN(a.DigCGCCPF) = 2 THEN 
            CAST(a.NroCGCCPF AS VARCHAR(12)) + CAST(a.DigCGCCPF AS CHAR(2)) ELSE
            CAST(a.NroCGCCPF AS VARCHAR(12)) END 
                            as cnpj_pessoa,
        a.SeqPessoa			as seq_pessoa,
        a.Status			as status_pessoa,
        a.NomeRazao			as nome_pessoa,
        a.FisicaJuridica	as tipo_pessoa,
        a.cidade			as cidade_pessoa,

        --a.SEQPESSOAPRC,

             CASE 
        WHEN LEN(b.DigCGCCPF) = 1 THEN 
            CAST(b.NroCGCCPF AS VARCHAR(12)) + '0'+ CAST(b.DigCGCCPF AS CHAR(2)) 
        WHEN LEN(b.DigCGCCPF) = 2 THEN 
            CAST(b.NroCGCCPF AS VARCHAR(12)) + CAST(b.DigCGCCPF AS CHAR(2)) ELSE
            CAST(b.NroCGCCPF AS VARCHAR(12)) END 
                            as cnpj_conglo,
        b.DigCGCCPF,
        b.NroCGCCPF,

        b.SeqPessoa			as seq_conglo,
        b.Status			as status_conglo,
        b.NomeRazao			as cliente_conglo,
        b.FisicaJuridica	as tipo_conglo,
        b.cidade			as cidade_conglo,

        Case 
            when a.SeqPessoa	=	b.SeqPessoa	 
                then 'S'
                else 'N' end 	
                            as principal,
        (
            select 
                AA.SeqCarteira
            from IVS_PES AA
            LEFT	JOIN IVS_CARTEIRA 	BB ON AA.SeqCarteira 	= BB.SeqCarteira
            INNER 	JOIN IVS_Depto		CC ON AA.SeqDepto		= CC.SeqDepto
            where 
                AA.SeqDepto IN (3)
                --and BB.Carteira not in('RAO_PEÇAS_01','RAO_PEÇAS_05','BRT_PEÇAS_03')
                and AA.SeqPessoa=a.SeqPessoa	
        ) 			as Carteira_pessoa,									
        (
            select 
                AA.SeqCarteira
            from IVS_PES AA
            LEFT	JOIN IVS_CARTEIRA 	BB ON AA.SeqCarteira 	= BB.SeqCarteira
            INNER 	JOIN IVS_Depto		CC ON AA.SeqDepto		= CC.SeqDepto
            where 
                AA.SeqDepto IN (3)
                --and BB.Carteira not in('RAO_PEÇAS_01','RAO_PEÇAS_05','BRT_PEÇAS_03')
                and AA.SeqPessoa=b.SeqPessoa
        ) 			as Carteira_Conglomerado						

        FROM 
                    GE_Pessoa (nolock)	a
        INNER JOIN	GE_Pessoa (nolock)	b on a.SEQPESSOAPRC = b.SeqPessoa 
        ) Cadastro
    ) a
    LEFT join 	IVS_CARTEIRA 	(nolock)	b on a.Carteira_pessoa 	= b.SeqCarteira
    --LEFT join IVS_PES			(nolock)	c on b.SeqCarteira		= c.SeqCarteira and 
    LEFT JOIN 	GE_Usuario		(nolock)	c ON b.SeqUsrResp		= c.SeqUsuario
    LEFT JOIN 	GE_Empresa		(nolock)	D ON B.NroEmpresa		= D.NroEmpresa

    --order by cnpj_pessoa
	