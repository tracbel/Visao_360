/* ==============================================================
   Objeto ..........: dbo.tba_clientes
   Tipo ............: VIEW
   Criado em .......: 2025-01-29 11:37:37
   Modificado em ...: 2025-01-29 11:37:37
   Linhas ..........: 34
   Escreve em tabela: nao
   Tabelas referidas: GE_Empresa, GE_PESSOA, GE_Usuario, IVS_CARTEIRA, IVS_DEPTO, IVS_DEPTOPOT, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE VIEW tba_clientes AS
SELECT
    P.SEQPESSOA AS SEQUENCIAL,
    p.Cidade,
    NOMERAZAO as CLIENTE,
    D.DEPTO,
    emp.Fantasia as EMPRESA,
    C.CARTEIRA,
    USR.CodUsuario as RESPONSAVEL,
        CASE
            WHEN DPOT.POTENCIAL = '85' THEN 'PAN'
            WHEN DPOT.POTENCIAL = '1' THEN 'A'
            WHEN DPOT.POTENCIAL = '22' THEN 'B'
            WHEN DPOT.POTENCIAL = '43' THEN 'C'
            WHEN DPOT.POTENCIAL = '64' THEN 'D'
            WHEN LEN(DPOT.POTENCIAL) = 0 OR DPOT.POTENCIAL IS NULL THEN 'VAZIO'
            ELSE DPOT.POTENCIAL
        END AS POTENCIAL
FROM
    GE_PESSOA P
    LEFT JOIN IVS_PES PES
    ON P.SEQPESSOA = PES.SEQPESSOA
    LEFT JOIN IVS_DEPTO D
    ON D.SEQDEPTO = PES.SEQDEPTO
    LEFT JOIN IVS_CARTEIRA C
    ON PES.SEQCARTEIRA = C.SeqCarteira
    join GE_Empresa emp
    on emp.NroEmpresa = c.NroEmpresa
    LEFT JOIN IVS_DEPTOPOT DPOT
  ON DPOT.SEQDEPTO = PES.SEQDEPTO
  AND DPOT.SEQPOTENCIALDP = PES.SEQPOTENCIALDP
  AND (dpot.POTENCIAL >= 'A' or dpot.POTENCIAL is null)
    LEFT JOIN GE_Usuario USR
    ON USR.SeqUsuario = C.SeqUsrResp;