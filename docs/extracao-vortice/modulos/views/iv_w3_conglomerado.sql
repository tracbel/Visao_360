/* ==============================================================
   Objeto ..........: dbo.iv_w3_conglomerado
   Tipo ............: VIEW
   Criado em .......: 2025-08-13 17:14:16
   Modificado em ...: 2025-08-13 17:14:16
   Linhas ..........: 28
   Escreve em tabela: nao
   Tabelas referidas: ge_pessoa, ge_pessoalink
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW iv_w3_conglomerado AS 
-- Pessoas associadas a outra principal
 SELECT
    pes.seqpessoa       AS seqpessoa,           -- Sequencial da pessoa atual
    pes.seqpessoaprc       AS seqprincipal,        -- Sequencial da pessoa principal (neste contexto, a própria pessoa que está ligada)
    pes.seqpessoa     AS seqpessoarelacionada,-- Sequencial da pessoa principal do conglomerado à qual está ligada
    'LIGADO AO PRINCIPAL' AS tipo_relacionamento,
    plk.ORIGEM  AS link_origem,
    'Pertence ao conglomerado.: ' + pesprc.nomerazao AS obs_relacionamento
FROM  ge_pessoa pes
JOIN  ge_pessoa pesprc ON pesprc.seqpessoa = pes.seqpessoaprc
LEFT JOIN ge_pessoalink plk ON plk.SEQPESSOA  = pes.seqpessoa 
WHERE pes.seqpessoaprc != pes.seqpessoa

UNION ALL

-- Pessoas vinculadas ao conglomerado (aqui, a pessoa principal é o foco e as relacionadas são as vinculadas)
SELECT
    pes.seqpessoaprc    AS seqpessoa,           -- Sequencial da pessoa principal do conglomerado
    pes.seqpessoaprc    AS seqprincipal,        -- Sequencial da pessoa principal do conglomerado
    pes.seqpessoa       AS seqpessoarelacionada,-- Sequencial da pessoa que está vinculada ao conglomerado
    'VINCULADOS' AS tipo_relacionamento,
    plk.ORIGEM  AS link_origem,
    'Nome/razão social: ' + pes.nomerazao AS obs_relacionamento
FROM  ge_pessoa pes
JOIN  ge_pessoa pesprc ON pesprc.seqpessoa = pes.seqpessoaprc
LEFT JOIN ge_pessoalink plk ON plk.SEQPESSOA  = pes.seqpessoaprc
WHERE pes.seqpessoaprc != pes.seqpessoa;