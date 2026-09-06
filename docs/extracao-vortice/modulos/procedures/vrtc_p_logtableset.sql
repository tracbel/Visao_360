/* ==============================================================
   Objeto ..........: dbo.vrtc_p_logtableset
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2026-04-20 17:53:15
   Modificado em ...: 2026-04-20 17:53:15
   Linhas ..........: 184
   Escreve em tabela: SIM (INSERT, UPDATE)
   Alvos de escrita : ge_log_historico, iv_agendalog
   Tabelas referidas: ge_log_historico, iv_agendalog
   Outras refs .....: vrtc_f_aliastablelogget
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE PROCEDURE dbo.vrtc_p_logtableset
    @ps_tabela    VARCHAR(200),   -- Nome da tabela a que o log referencia
    @pn_kn1       NUMERIC(18, 0), -- Chave numerica 1
    @pn_kn2       NUMERIC(18, 0), -- Chave numerica 2
    @ps_ks        VARCHAR(200),   -- Chave string
    @ps_usrcod    VARCHAR(50),    -- Usuario que gerou o log
    @ps_codapl    VARCHAR(50),    -- Aplicacao/procedure que gerou
    @ps_obs       VARCHAR(MAX),   -- Texto do log
    @ps_nivelLog  CHAR(1) = '5'   -- Nivel do log
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
          @vs_tablename VARCHAR(100)
        , @vdhora       DATETIME
        , @vs_obsorig   VARCHAR(MAX)
        , @vs_obs       VARCHAR(4000)
        , @v_loc        INT
        , @v_antigo     VARCHAR(MAX)
        , @v_seqlog     NUMERIC(18, 0)
        , @v_limite     INT
        , @v_crlf       VARCHAR(2)
        , @v_kd         DATETIME
        , @sql          NVARCHAR(MAX);

    SET @v_limite = 1000;
    SET @v_crlf   = CHAR(13) + CHAR(10);

    -- validacoes iniciais
    IF @ps_tabela IS NULL
       OR LEN(LTRIM(RTRIM(ISNULL(@ps_obs, '')))) < 5
        RETURN;

    -- limpeza / normalizacao
    SET @ps_tabela = LEFT(UPPER(LTRIM(RTRIM(@ps_tabela))), 40);
    SET @ps_obs    = LEFT(ISNULL(@ps_obs, ''), 4000);
    SET @ps_ks     = ISNULL(NULLIF(@ps_ks, ''), 'X');
    SET @ps_usrcod = LEFT(ISNULL(@ps_usrcod, '* SISTEMA *'), 20);
    SET @ps_codapl = LEFT(ISNULL(@ps_codapl, ''), 30);
    SET @pn_kn1    = ISNULL(@pn_kn1, 0);
    SET @pn_kn2    = ISNULL(@pn_kn2, 0);
    SET @vdhora    = GETDATE();

    -- descobrir a tabela de log
    SET @vs_tablename = dbo.vrtc_f_aliastablelogget(@ps_tabela);

    IF @vs_tablename IS NULL
        RETURN;

    /*
       tentativa de aglutinar com ultimo log
       somente para ge_log_historico
    */
    IF @vs_tablename = 'GE_LOG_HISTORICO'
    BEGIN
        SET @v_kd = DATEADD(MINUTE, -1, @vdhora);

        SELECT TOP 1
              @v_seqlog = seqlogtb
            , @v_antigo = obs
        FROM ge_log_historico
        WHERE tb = @ps_tabela
          AND kn1 = @pn_kn1
          AND kn2 = @pn_kn2
          AND ks = @ps_ks
          AND dtalog >= @v_kd
          AND usr = @ps_usrcod
        ORDER BY dtalog DESC;

        IF @v_seqlog IS NOT NULL
           AND LEN(ISNULL(@v_antigo, '') + @ps_obs) < (@v_limite - 4)
        BEGIN
            SET @vs_obs = ISNULL(@v_antigo, '') + @v_crlf + @ps_obs;

            UPDATE ge_log_historico
               SET obs = @vs_obs
             WHERE seqlogtb = @v_seqlog;

            RETURN;
        END
    END

    -- quebra em blocos de ate 1000, preferindo cortar em espaco por volta de 995
    SET @vs_obsorig = @ps_obs;

    WHILE LEN(@vs_obsorig) > 0
    BEGIN
        IF LEN(@vs_obsorig) > @v_limite
        BEGIN
            SET @v_loc = CHARINDEX(' ', REVERSE(LEFT(@vs_obsorig, 995)));

            IF @v_loc > 0
            BEGIN
                SET @v_loc = 995 - @v_loc;
                SET @vs_obs = LEFT(@vs_obsorig, @v_loc);
                SET @vs_obsorig = ' ... ' + SUBSTRING(@vs_obsorig, @v_loc + 1, LEN(@vs_obsorig));
            END
            ELSE
            BEGIN
                SET @vs_obs = LEFT(@vs_obsorig, @v_limite);
                SET @vs_obsorig = SUBSTRING(@vs_obsorig, @v_limite + 1, LEN(@vs_obsorig));
            END
        END
        ELSE
        BEGIN
            SET @vs_obs = @vs_obsorig;
            SET @vs_obsorig = '';
        END

        IF @vs_tablename = 'IV_AGENDALOG'
        BEGIN
            INSERT INTO iv_agendalog (
                  tb
                , kn1
                , kn2
                , ks
                , dtalog
                , usr
                , obs
                , codapl
            )
            VALUES (
                  @ps_tabela
                , @pn_kn1
                , @pn_kn2
                , @ps_ks
                , @vdhora
                , @ps_usrcod
                , @vs_obs
                , @ps_codapl
            );
        END
        ELSE
        BEGIN
            SET @sql =
                N'INSERT INTO ' + QUOTENAME(@vs_tablename) + N'
                (
                      tb
                    , kn1
                    , kn2
                    , ks
                    , dtalog
                    , usr
                    , obs
                    , codapl
                    , nivel
                )
                VALUES
                (
                      @p_tabela
                    , @p_kn1
                    , @p_kn2
                    , @p_ks
                    , @p_dtalog
                    , @p_usr
                    , @p_obs
                    , @p_codapl
                    , @p_nivel
                );';

            EXEC sp_executesql
                  @sql
                , N'@p_tabela VARCHAR(40),
                    @p_kn1 NUMERIC(18,0),
                    @p_kn2 NUMERIC(18,0),
                    @p_ks VARCHAR(200),
                    @p_dtalog DATETIME,
                    @p_usr VARCHAR(20),
                    @p_obs VARCHAR(4000),
                    @p_codapl VARCHAR(30),
                    @p_nivel CHAR(1)'
                , @p_tabela = @ps_tabela
                , @p_kn1    = @pn_kn1
                , @p_kn2    = @pn_kn2
                , @p_ks     = @ps_ks
                , @p_dtalog = @vdhora
                , @p_usr    = @ps_usrcod
                , @p_obs    = @vs_obs
                , @p_codapl = @ps_codapl
                , @p_nivel  = @ps_nivelLog;
        END
    END
END