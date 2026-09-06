/* ==============================================================
   Objeto ..........: dbo.vrtc_p_atualizar_data_ultimo_contatoExterno
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2025-08-07 17:50:46
   Modificado em ...: 2025-08-07 17:50:46
   Linhas ..........: 83
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : GE_PARAMETROGLOBAL
   Tabelas referidas: GE_PARAMETROGLOBAL, IV_HISTORICO, IVS_DEPTORES, IVS_PES
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */


CREATE PROCEDURE dbo.vrtc_p_atualizar_data_ultimo_contatoExterno
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @ultima_execucao_param   DATETIME     = NULL,
        @data_inicio_processamento DATETIME   = GETDATE(),
        @data_fim_processamento  DATETIME,
        @linhas_afetadas         INT,
        @mensagem_log_valor      NVARCHAR(4000),
        @tempo_segundos          INT,
        @minutos_decorridos      INT,
        @segundos_decorridos     INT;

    -- 1. Ler última execução externa
    SELECT @ultima_execucao_param = DTAALTERACAO
      FROM GE_PARAMETROGLOBAL
     WHERE SISTEMA   = 'CRM'         AND MODULO    = 'VORTICO'           AND PARAMETRO = 'ULTCALCCTTOEX';

    IF @ultima_execucao_param IS NULL
        SET @ultima_execucao_param = '1900-01-01 00:00:00';

    SET @ultima_execucao_param = DATEADD(MINUTE, -5, @ultima_execucao_param);

    -- 2. Atualizar IVS_PES (EXTERNO)
    UPDATE ip
       SET DTAULTCTTOEXT = sq.DTAREALIZACAO
      FROM IVS_PES ip
      JOIN (
            SELECT
                pes.SEQPESSOA,
                pes.SEQPESDEPTO,
                MAX(ht.DTAREALIZACAO) AS DTAREALIZACAO
              FROM IV_HISTORICO ht
              JOIN IVS_PES pes       ON pes.SEQPESSOA = ht.SEQPESSOA
              JOIN IVS_DEPTORES res  ON res.SEQDEPTO = pes.SEQDEPTO
                AND res.RESULTADO  = ht.RESULTADO
                AND res.INDEXTERNO = 1
             WHERE ht.DTAREALIZACAO >= @ultima_execucao_param    
             GROUP BY pes.SEQPESSOA, pes.SEQPESDEPTO
           ) AS sq
        ON ip.SEQPESSOA   = sq.SEQPESSOA
       AND ip.SEQPESDEPTO = sq.SEQPESDEPTO;

    SET @linhas_afetadas = @@ROWCOUNT;
    SET @data_fim_processamento = GETDATE();
    SET @tempo_segundos = DATEDIFF(SECOND, @data_inicio_processamento, @data_fim_processamento);
    SET @minutos_decorridos = @tempo_segundos / 60;
    SET @segundos_decorridos = @tempo_segundos % 60;

    SET @mensagem_log_valor =
         N'Qtde de clientes externos atualizados: ' + CAST(@linhas_afetadas AS NVARCHAR(10))
       + N' | Tempo proces.(mm:seg): '
       + RIGHT('0' + CAST(@minutos_decorridos AS NVARCHAR(2)), 2) + N':'
       + RIGHT('0' + CAST(@segundos_decorridos AS NVARCHAR(2)), 2);

    UPDATE GE_PARAMETROGLOBAL
       SET DTAALTERACAO = @data_fim_processamento,
           VALOR         = @mensagem_log_valor
     WHERE SISTEMA   = 'CRM'
       AND MODULO    = 'VORTICO'
       AND PARAMETRO = 'ULTCALCCTTOEX';

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO GE_PARAMETROGLOBAL
            (SISTEMA, MODULO, PARAMETRO, NROEMPRESA, DTAALTERACAO, VALOR)
        VALUES
            ('CRM', 'VORTICO', 'ULTCALCCTTOEX', 0, @data_fim_processamento, @mensagem_log_valor);
    END

    PRINT
        N'Atualização incremental da data de último contato externo concluída em '
      + CONVERT(VARCHAR(19), @data_fim_processamento, 120)
      + N'. ' + CAST(@linhas_afetadas AS VARCHAR(10))
      + N' registros alterados. Tempo: '
      + RIGHT('0' + CAST(@minutos_decorridos AS VARCHAR(2)),2) + N':'
      + RIGHT('0' + CAST(@segundos_decorridos AS VARCHAR(2)),2);

END;
