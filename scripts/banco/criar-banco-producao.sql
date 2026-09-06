/* ============================================================================
   Criação do banco do CRM próprio na instância SQL Server de produção.

   PARA QUEM RODA ISTO (DBA / infraestrutura):
   Este script cria um banco NOVO e vazio. Ele NÃO toca nos bancos CRM e
   CRM_HOMO do Vórtice, não lê dado nenhum e não altera nada da instância —
   apenas cria e configura o banco do CRM novo. Depois dele, quem cria as
   tabelas é a migração da aplicação (`dotnet ef database update`), nunca a mão.

   POR QUE CADA OPÇÃO ESTÁ AQUI:
   cada uma corrige um problema medido no banco do Vórtice, documentado em
   docs/extracao-vortice/00-RELATORIO-EXTRACAO.md e em
   docs/projeto/14-PADRAO-DE-BANCO.md. Os comentários dizem qual.

   ANTES DE RODAR, revise os três blocos marcados com AJUSTE.
   Rodar duas vezes é seguro: o script não recria o que já existe.
   ============================================================================ */

SET NOCOUNT ON;
GO

/* --------------------------------------------------------------------------
   AJUSTE 1 — nome do banco e onde ficam os arquivos.

   Coloque dado e log em volumes separados se a máquina tiver. O importante é
   NÃO deixar cair no caminho padrão junto com os arquivos do Vórtice: eles
   somam 61 GB e disputam a mesma fila de escrita.
   -------------------------------------------------------------------------- */
DECLARE @banco        sysname       = N'TracbelCrm';
DECLARE @caminhoDado  nvarchar(260) = N'D:\Dados\TracbelCrm.mdf';
DECLARE @caminhoLog   nvarchar(260) = N'E:\Log\TracbelCrm_log.ldf';

/* Tamanho inicial. O banco chega a ~18 GB depois da migração do histórico do
   Vórtice; começar pequeno faz o arquivo crescer dezenas de vezes durante a
   carga, e cada crescimento é uma pausa. Crescimento em MB e nunca em
   porcentagem — [V] o Vórtice cresce de 10 em 10% num arquivo de 36 GB, o que
   significa saltos de 3,6 GB de cada vez. */
DECLARE @dadoInicial  nvarchar(20)  = N'8GB';
DECLARE @dadoCresce   nvarchar(20)  = N'1GB';
DECLARE @logInicial   nvarchar(20)  = N'2GB';
DECLARE @logCresce    nvarchar(20)  = N'512MB';

/* A colação decide como o banco compara texto.
   CI = ignora caixa. AI = ignora acento.
   É o que faz "Jose" achar "José" e o que impede FINALIZADO e FINALIZADA de
   coexistirem num catálogo — [V] o Vórtice usa SQL_Latin1_General_CP1_CI_AS,
   que diferencia acento, e tem 437.694 processos com status em branco e
   variações do mesmo valor convivendo. */
DECLARE @colacao      sysname       = N'Latin1_General_CI_AI';

DECLARE @sql nvarchar(max);

/* --------------------------------------------------------------------------
   1. Cria o banco, se ainda não existir.
   -------------------------------------------------------------------------- */
IF DB_ID(@banco) IS NULL
BEGIN
    SET @sql = N'
        CREATE DATABASE ' + QUOTENAME(@banco) + N'
        ON PRIMARY (
            NAME = ' + QUOTENAME(@banco + N'_dado') + N',
            FILENAME = ''' + @caminhoDado + N''',
            SIZE = ' + @dadoInicial + N',
            FILEGROWTH = ' + @dadoCresce + N'
        )
        LOG ON (
            NAME = ' + QUOTENAME(@banco + N'_log') + N',
            FILENAME = ''' + @caminhoLog + N''',
            SIZE = ' + @logInicial + N',
            FILEGROWTH = ' + @logCresce + N'
        )
        COLLATE ' + @colacao + N';';
    EXEC sys.sp_executesql @sql;
    PRINT 'Banco ' + @banco + ' criado.';
END
ELSE
    PRINT 'Banco ' + @banco + ' ja existe — segue para a configuracao.';
GO

/* --------------------------------------------------------------------------
   2. Configuração do banco. Idempotente: aplicar de novo não muda nada.
   -------------------------------------------------------------------------- */
DECLARE @banco sysname = N'TracbelCrm';
DECLARE @sql nvarchar(max);

/* AUTO_SHRINK desligado.
   [V] achado 9 da extração: está LIGADO nos dois bancos do Vórtice. Num banco
   com log em FULL, ele encolhe e o arquivo volta a crescer logo depois,
   fragmentando índice e gerando pico de disco sem hora marcada. */
SET @sql = N'ALTER DATABASE ' + QUOTENAME(@banco) + N' SET AUTO_SHRINK OFF;';
EXEC sys.sp_executesql @sql;

/* Estatística automática ligada: é o que mantém o plano de consulta bom sem
   ninguém cuidar. ASYNC evita que a primeira consulta depois de uma carga
   espere o recálculo. */
SET @sql = N'
    ALTER DATABASE ' + QUOTENAME(@banco) + N' SET AUTO_CREATE_STATISTICS ON;
    ALTER DATABASE ' + QUOTENAME(@banco) + N' SET AUTO_UPDATE_STATISTICS ON;
    ALTER DATABASE ' + QUOTENAME(@banco) + N' SET AUTO_UPDATE_STATISTICS_ASYNC ON;';
EXEC sys.sp_executesql @sql;

/* Leitura não trava escrita, e vice-versa.
   Sem isto, um relatório pesado de Cobertura ou da Visão 360 segura quem está
   registrando uma interação. Exige um instante de acesso exclusivo, então rode
   com a aplicação parada — antes de publicar é o momento certo. */
SET @sql = N'ALTER DATABASE ' + QUOTENAME(@banco) +
           N' SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;';
EXEC sys.sp_executesql @sql;

/* Recuperação FULL: permite restaurar até o minuto do incidente.
   Só faz sentido com backup de log agendado — ver o item 5. */
SET @sql = N'ALTER DATABASE ' + QUOTENAME(@banco) + N' SET RECOVERY FULL;';
EXEC sys.sp_executesql @sql;

/* Query Store ligado: guarda o histórico de plano e de tempo de cada consulta.
   É a ferramenta que responde "o que ficou lento e desde quando" sem precisar
   reproduzir o problema. [V] no Vórtice não há nada equivalente, e por isso
   nenhum dos defeitos de desempenho tem data de início conhecida. */
SET @sql = N'
    ALTER DATABASE ' + QUOTENAME(@banco) + N' SET QUERY_STORE = ON;
    ALTER DATABASE ' + QUOTENAME(@banco) + N' SET QUERY_STORE (
        OPERATION_MODE = READ_WRITE,
        MAX_STORAGE_SIZE_MB = 2048,
        QUERY_CAPTURE_MODE = AUTO,
        CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 60)
    );';
EXEC sys.sp_executesql @sql;

/* Nível de compatibilidade: o mais alto que a instância suporta, nunca o
   herdado do `model`.
   [V] os bancos do Vórtice estão em 100, que é o SQL Server 2008, rodando
   dentro de um SQL Server 2019 — otimizador de duas versões atrás por herança,
   não por decisão.
   ATENÇÃO: a instância de produção é SQL Server 2019, cujo máximo é 150. O
   contêiner de desenvolvimento é 2022 e vai a 160. Enquanto os dois não forem
   a mesma versão, o desenvolvimento roda num otimizador mais novo que o de
   produção — está registrado como pendência no documento 20. */
DECLARE @nivel int = CAST(SERVERPROPERTY('ProductMajorVersion') AS int) * 10;
SET @sql = N'ALTER DATABASE ' + QUOTENAME(@banco) +
           N' SET COMPATIBILITY_LEVEL = ' + CAST(@nivel AS nvarchar(4)) + N';';
EXEC sys.sp_executesql @sql;

PRINT 'Configuracao aplicada.';
GO

/* --------------------------------------------------------------------------
   AJUSTE 2 — o login da aplicação.

   A API entra com este login, que enxerga SÓ este banco. Ele não é
   administrador, não lê os bancos do Vórtice e não cria objeto: quem cria
   tabela é a migração, rodada por outro login no momento de publicar.

   Troque a senha antes de rodar e guarde no cofre, nunca em arquivo do
   projeto. [V] o Vórtice guarda 31 parâmetros cifrados na própria tabela, com
   a chave dentro do executável — é exatamente o que não fazemos aqui.
   -------------------------------------------------------------------------- */
DECLARE @banco   sysname       = N'TracbelCrm';
DECLARE @login   sysname       = N'crm_aplicacao';
DECLARE @senha   nvarchar(128) = N'TROQUE-ESTA-SENHA-ANTES-DE-RODAR';
DECLARE @sql     nvarchar(max);

IF @senha = N'TROQUE-ESTA-SENHA-ANTES-DE-RODAR'
BEGIN
    RAISERROR(N'AJUSTE 2: defina a senha do login da aplicacao antes de rodar este bloco. Os blocos 1 e 2 (banco e configuracao) ja foram aplicados e nao precisam rodar de novo.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @login)
BEGIN
    SET @sql = N'CREATE LOGIN ' + QUOTENAME(@login) +
               N' WITH PASSWORD = ''' + REPLACE(@senha, '''', '''''') + N''',
                    DEFAULT_DATABASE = ' + QUOTENAME(@banco) + N',
                    CHECK_POLICY = ON;';
    EXEC sys.sp_executesql @sql;
    PRINT 'Login ' + @login + ' criado. TROQUE A SENHA se ainda nao trocou.';
END
ELSE
    PRINT 'Login ' + @login + ' ja existe.';

/* Usuário dentro do banco, com leitura e escrita nos dados e nada além disso. */
SET @sql = N'
    USE ' + QUOTENAME(@banco) + N';
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = ' + QUOTENAME(@login, '''') + N')
        CREATE USER ' + QUOTENAME(@login) + N' FOR LOGIN ' + QUOTENAME(@login) + N';
    ALTER ROLE db_datareader ADD MEMBER ' + QUOTENAME(@login) + N';
    ALTER ROLE db_datawriter ADD MEMBER ' + QUOTENAME(@login) + N';';
EXEC sys.sp_executesql @sql;

PRINT 'Usuario da aplicacao configurado.';
GO

/* --------------------------------------------------------------------------
   AJUSTE 3 — o que fica FORA deste script e precisa ser combinado.

   1. BACKUP. Este banco precisa entrar na rotina: completo semanal,
      diferencial diário e log a cada 15 minutos, coerente com a recuperação
      FULL do item 2. Sem backup de log, o arquivo de log cresce sem parar.

      🔴 O destino NÃO pode ser o compartilhamento `\\10.150.6.230\bkpbd`.
      A extração de 02/09/2026 encontrou ali o backup completo do Vórtice, com
      29,9 GB, legível por "Todos" da rede. Enquanto isso não for corrigido,
      mandar o nosso backup para lá é publicar a base de clientes.

   2. MANUTENÇÃO DE ÍNDICE E ESTATÍSTICA, em janela própria, fora da janela do
      Vórtice, para os dois não disputarem a mesma madrugada.

   3. MONITORAMENTO DE ESPAÇO. [V] o terminal server do Vórtice chegou a
      0,42 GB livres em 129 GB sem ninguém notar. Alarme em 20% livre.

   4. LIMITAÇÃO CONHECIDA, para constar: na edição Standard não há como isolar
      processador e memória entre bancos da mesma instância. Se o Vórtice tiver
      um pico, o CRM novo sente. Com o volume medido isso é improvável, e a
      saída existe: um banco SQL Server muda de instância com backup e restore.
   -------------------------------------------------------------------------- */

/* --------------------------------------------------------------------------
   3. Conferência. Rode e compare com a coluna "esperado".
   -------------------------------------------------------------------------- */
SELECT
    d.name                                              AS banco,
    d.collation_name                                    AS colacao,        -- Latin1_General_CI_AI
    d.compatibility_level                               AS compatibilidade,-- 150 no 2019, 160 no 2022
    d.is_auto_shrink_on                                 AS auto_shrink,    -- 0
    d.is_read_committed_snapshot_on                     AS leitura_sem_trava, -- 1
    d.recovery_model_desc                               AS recuperacao,    -- FULL
    d.is_query_store_on                                 AS query_store,    -- 1
    (SELECT COUNT(*) FROM sys.master_files f WHERE f.database_id = d.database_id) AS arquivos
FROM sys.databases AS d
WHERE d.name = N'TracbelCrm';

/* Crescimento dos arquivos: a coluna is_percent_growth precisa vir 0 nos dois.
   [V] no Vórtice ela é 1, com crescimento de 10% num arquivo de 36 GB. */
SELECT
    f.name                                              AS arquivo,
    f.type_desc                                         AS tipo,
    f.size / 128                                        AS tamanho_mb,
    f.growth / 128                                      AS cresce_mb,
    f.is_percent_growth                                 AS cresce_em_porcentagem, -- 0
    f.physical_name                                     AS caminho
FROM sys.master_files AS f
WHERE f.database_id = DB_ID(N'TracbelCrm');
GO

/* --------------------------------------------------------------------------
   4. Depois deste script, quem cria as tabelas é a aplicação:

        dotnet ef database update --project src/Tracbel.Crm.Infraestrutura

      Ao final devem existir 63 tabelas em 10 schemas e nenhuma em `dbo`.
      Confira com:

        SELECT s.name, COUNT(*) FROM sys.tables t
        JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE t.name <> '__EFMigrationsHistory'
        GROUP BY s.name ORDER BY s.name;
   -------------------------------------------------------------------------- */
