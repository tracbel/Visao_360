/* ==============================================================
   Objeto ..........: dbo.sp_atualiza_fones_ge_pessoa
   Tipo ............: SQL_STORED_PROCEDURE
   Criado em .......: 2026-04-20 17:51:23
   Modificado em ...: 2026-04-20 17:54:53
   Linhas ..........: 210
   Escreve em tabela: SIM (UPDATE)
   Alvos de escrita : GE_PESSOAFONE, GE_PESSOA
   Tabelas referidas: GE_PESSOA, GE_PESSOAFONE
   Outras refs .....: vrtc_p_logtableset
   Fonte: banco CRM (Vortice CRM / Tracbel) - extracao somente leitura
   ============================================================== */

CREATE   PROCEDURE dbo.sp_atualiza_fones_ge_pessoa
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
          @vnSeqPesFone       NUMERIC
        , @vnSeqPessoa        NUMERIC(10)
        , @vsDDD              VARCHAR(5)
        , @vnNumero           NUMERIC(12)
        , @vsComplemento      VARCHAR(20)
        , @vsNumeroStr        VARCHAR(20)
        , @vbCelular          BIT
        , @vnSlotEscolhido    INT
        , @vnFone1            NUMERIC(12)
        , @vnFone2            NUMERIC(12)
        , @vnFone3            NUMERIC(12)
        , @vsLog              VARCHAR(max);

    DECLARE cur_fones CURSOR LOCAL FAST_FORWARD FOR
        SELECT
              pf.SEQPESFONE
            , pf.SEQPESSOA
            , pf.DDD
            , pf.NUMERO
            , pf.COMPLEMENTO
        FROM GE_PESSOAFONE pf
        WHERE pf.INDEMUSO = 1
          AND ISNULL(pf.NROFONEPESSOA, 0) = 0
          AND pf.NUMERO IS NOT NULL                    
        ORDER BY
              pf.SEQPESSOA
            , pf.SEQPESFONE;

    OPEN cur_fones;

    FETCH NEXT FROM cur_fones
    INTO @vnSeqPesFone, @vnSeqPessoa, @vsDDD, @vnNumero, @vsComplemento;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @vnSlotEscolhido = NULL;
        Set @vsLog = 'Alt por procedure';
        SET @vsNumeroStr = LTRIM(RTRIM(CONVERT(VARCHAR(20), @vnNumero)));

        /*
           Regra de celular:
           - 9 dígitos começando com 9
           - 8 dígitos começando com 8 ou 9
        */
        SET @vbCelular =
            CASE
                WHEN LEN(@vsNumeroStr) = 9
                 AND LEFT(@vsNumeroStr, 1) = '9'
                    THEN 1
                WHEN LEN(@vsNumeroStr) = 8
                 AND LEFT(@vsNumeroStr, 1) IN ('8', '9')
                    THEN 1
                ELSE 0
            END;

        SELECT
              @vnFone1 = p.FONENRO1
            , @vnFone2 = p.FONENRO2
            , @vnFone3 = p.FONENRO3
        FROM GE_PESSOA p
        WHERE p.SEQPESSOA = @vnSeqPessoa;

        IF @vnNumero = @vnFone1
        BEGIN
            UPDATE GE_PESSOAFONE
               SET NROFONEPESSOA = 1
             WHERE SEQPESFONE = @vnSeqPesFone;
        END
        ELSE IF @vnNumero = @vnFone2
        BEGIN
            UPDATE GE_PESSOAFONE
               SET NROFONEPESSOA = 2
             WHERE SEQPESFONE = @vnSeqPesFone;
        END
        ELSE IF @vnNumero = @vnFone3
        BEGIN
            UPDATE GE_PESSOAFONE
               SET NROFONEPESSOA = 3
             WHERE SEQPESFONE = @vnSeqPesFone;
        END
        ELSE
        BEGIN
            /*
               Regras de escolha do slot:

               - se todos vazios:
                   celular -> slot 2
                   não celular -> slot 1

               - celular:
                   prefere 2, depois 1, depois 3

               - não celular:
                   prefere 1, depois 3, depois 2
            */
            IF @vnFone1 IS NULL
               AND @vnFone2 IS NULL
               AND @vnFone3 IS NULL
            BEGIN
                IF @vbCelular = 1
                    SET @vnSlotEscolhido = 2;
                ELSE
                    SET @vnSlotEscolhido = 1;
            END
            ELSE
            BEGIN
                IF @vbCelular = 1
                BEGIN
                    IF @vnFone2 IS NULL
                        SET @vnSlotEscolhido = 2;
                    ELSE IF @vnFone1 IS NULL
                        SET @vnSlotEscolhido = 1;
                    ELSE IF @vnFone3 IS NULL
                        SET @vnSlotEscolhido = 3;
                END
                ELSE
                BEGIN
                    IF @vnFone1 IS NULL
                        SET @vnSlotEscolhido = 1;
                    ELSE IF @vnFone3 IS NULL
                        SET @vnSlotEscolhido = 3;
                    ELSE IF @vnFone2 IS NULL
                        SET @vnSlotEscolhido = 2;
                END
            END;

            IF @vnSlotEscolhido = 1
            BEGIN
                UPDATE GE_PESSOA
                   SET FONENRO1   = @vnNumero
                     , FONEDDD1   = @vsDDD
                     , FONECMPL1  = @vsComplemento
                 WHERE SEQPESSOA = @vnSeqPessoa;

                 SET @vsLog = CONCAT(@vsLog, ' - fone nro1 alterado para: ', @vnNumero); 
                    EXEC dbo.vrtc_p_logtableset
                          @ps_tabela   = 'GE_PESSOA'
                        , @pn_kn1      = @vnSeqPessoa
                        , @pn_kn2      = 0
                        , @ps_ks       = NULL
                        , @ps_usrcod   = '(procedure)'
                        , @ps_codapl   = 'Ajuste fones'
                        , @ps_obs      = @vsLog
                        , @ps_nivelLog = '3';

                UPDATE GE_PESSOAFONE
                   SET NROFONEPESSOA = 1
                 WHERE SEQPESFONE = @vnSeqPesFone;
            END
            ELSE IF @vnSlotEscolhido = 2
            BEGIN
                UPDATE GE_PESSOA
                   SET FONENRO2   = @vnNumero
                     , FONEDDD2   = @vsDDD
                     , FONECMPL2  = @vsComplemento
                 WHERE SEQPESSOA = @vnSeqPessoa;
                 SET @vsLog = CONCAT(@vsLog, ' - fone nro2 alterado para: ', @vnNumero); 
                    EXEC dbo.vrtc_p_logtableset
                          @ps_tabela   = 'GE_PESSOA'
                        , @pn_kn1      = @vnSeqPessoa
                        , @pn_kn2      = 0
                        , @ps_ks       = NULL
                        , @ps_usrcod   = '(procedure)'
                        , @ps_codapl   = 'Ajuste fones'
                        , @ps_obs      = @vsLog
                        , @ps_nivelLog = '3';

                UPDATE GE_PESSOAFONE
                   SET NROFONEPESSOA = 2
                 WHERE SEQPESFONE = @vnSeqPesFone;
            END
            ELSE IF @vnSlotEscolhido = 3
            BEGIN
                UPDATE GE_PESSOA
                   SET FONENRO3   = @vnNumero
                     , FONEDDD3   = @vsDDD
                     , FONECMPL3  = @vsComplemento
                 WHERE SEQPESSOA = @vnSeqPessoa;
                 SET @vsLog = CONCAT(@vsLog, ' - fone nro3 alterado para: ', @vnNumero); 
                    EXEC dbo.vrtc_p_logtableset
                          @ps_tabela   = 'GE_PESSOA'
                        , @pn_kn1      = @vnSeqPessoa
                        , @pn_kn2      = 0
                        , @ps_ks       = NULL
                        , @ps_usrcod   = '(procedure)'
                        , @ps_codapl   = 'Ajuste fones'
                        , @ps_obs      = @vsLog
                        , @ps_nivelLog = '3';

                UPDATE GE_PESSOAFONE
                   SET NROFONEPESSOA = 3
                 WHERE SEQPESFONE = @vnSeqPesFone;

               
            END
        END;

        FETCH NEXT FROM cur_fones
        INTO @vnSeqPesFone, @vnSeqPessoa, @vsDDD, @vnNumero, @vsComplemento;
    END;

    CLOSE cur_fones;
    DEALLOCATE cur_fones;
END