/* ==============================================================
   Objeto ..........: dbo.IV$DADOS_PESSOA_COMPLETA
   Tipo ............: VIEW
   Criado em .......: 2025-09-24 16:28:26
   Modificado em ...: 2025-09-24 16:28:26
   Linhas ..........: 87
   Escreve em tabela: nao
   Tabelas referidas: GE_PESSOA, GE_PESSOALINK, GE_REGIAO, GE_ROTA, IVS_CARTEIRA, IVS_Depto, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW [dbo].[IV$DADOS_PESSOA_COMPLETA] AS
-- View Criada por Matheus | Vórtice sistemas em 24/06/2025 -- Processo: 812304| Alterada em 24/09/2025

SELECT DISTINCT
	P.SEQPESSOA,
    P.STATUS,
    P.NOMERAZAO,
    P.FANTASIA,
    P.PALAVRACHAVE,
    P.FISICAJURIDICA,
    P.SEXO,
    P.CIDADE,
    P.UF,
    P.PAIS,
    P.BAIRRO,
    P.TIPOLOGRADOURO,
    P.LOGRADOURO,
    P.NROLOGRADOURO,
    P.CMPLTOLOGRADOURO,
    P.CEP,
    P.CXPOSTAL,
    P.REFENDERECO,
    P.INSCMUNIC,
    P.INSCPRODUTOR,
    P.CNAE,
    P.FONEDDD1,
    P.FONENRO1,
    P.FONECMPL1,
    P.FONEDDD2,
    P.FONENRO2,
    P.FONECMPL2,
    P.FONEDDD3,
    P.FONENRO3,
    P.FONECMPL3,
    P.FAXDDD,
    P.FAXNRO,
    P.NROCGCCPF,
    P.DIGCGCCPF,
    P.INSCRICAORG,
    P.UFEMISSOR,
    P.ORGAOEMISSOR,
    P.DTANASCFUND,
    P.EMAIL,
    P.HOMEPAGE,
    P.ESTADOCIVIL,
    P.ATIVIDADE,
    P.RENDAFATURAMENTO,
    P.GRAUINSTRUCAO,
    P.GRUPO,
    P.PORTE,

    -- Dados da Região
    G.REGIAO AS CODREGIAO,
    G.DESCRICAO AS REGIAO,

    -- Dados da Rota
    R.ROTA AS CODROTA,
    R.DESCRICAO AS ROTA,

    -- Dados do Depto e Carteira
    D.DEPTO,
    C.CARTEIRA, 
	C.NROEMPRESA,

	PL.PESSOALINK,
	PL.ORIGEM AS PESSOALINKORIGEM

FROM GE_PESSOA P
-- Join com a tabela de Regiões
LEFT JOIN GE_REGIAO G
    ON P.SEQREGIAO = G.SEQREGIAO
-- Join com a tabela de Rotas
LEFT JOIN GE_ROTA R
    ON P.SEQROTA = R.SEQROTA
-- Join com IVS_PES para conectar as pessoas às carteiras
LEFT JOIN IVS_PES I
    ON P.SEQPESSOA = I.SEQPESSOA
-- Join com IV_CARTEIRA para trazer dados da carteira
LEFT JOIN IVS_CARTEIRA C
    ON I.SEQCARTEIRA = C.SEQCARTEIRA
-- Join com a tabela de Departamento
LEFT JOIN IVS_Depto D
   ON I.SeqDepto = D.SEQDEPTO
--Join para trazer os dados do pessoalink
LEFT JOIN GE_PESSOALINK PL
   ON PL.SEQPESSOA = P.SEQPESSOA
