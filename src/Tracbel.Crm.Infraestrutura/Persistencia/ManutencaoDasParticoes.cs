using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Tracbel.Crm.Infraestrutura.Persistencia;

/// <summary>O que a manutenção das partições fez — ou, na simulação, o que faria.</summary>
/// <param name="MesesAbertos">Quantos limites mensais foram criados, somando as quatro tabelas.</param>
/// <param name="ParticoesExpurgadas">Quantas partições da trilha de alterações foram esvaziadas.</param>
/// <param name="LinhasExpurgadas">Quantas linhas da trilha saíram.</param>
/// <param name="MudancasDePermissaoMantidas">Quantas mudanças de permissão, das partições esvaziadas, ficaram.</param>
/// <param name="LimitesUnidos">Quantos limites antigos da trilha foram unidos depois do expurgo.</param>
/// <param name="Detalhes">Uma linha por passo, para o log da rotina.</param>
public sealed record ResultadoDaManutencaoDasParticoes(
    int MesesAbertos,
    int ParticoesExpurgadas,
    long LinhasExpurgadas,
    long MudancasDePermissaoMantidas,
    int LimitesUnidos,
    IReadOnlyList<string> Detalhes)
{
    /// <summary>Uma frase só, para a execução da rotina em Configurações › Integrações.</summary>
    public string Resumo => string.Create(CultureInfo.GetCultureInfo("pt-BR"),
        $"abriu {MesesAbertos} limite(s) de mês; expurgou {ParticoesExpurgadas} partição(ões) da trilha " +
        $"({LinhasExpurgadas:N0} linha(s)); manteve {MudancasDePermissaoMantidas:N0} mudança(s) de permissão; " +
        $"uniu {LimitesUnidos} limite(s) antigo(s)");
}

/// <summary>
/// A MANUTENÇÃO DAS TABELAS DE LOG PARTICIONADAS POR MÊS (issue 268) — a rotina mensal <c>PARTICAO_AUDITORIA</c>.
///
/// <para><b>O que ela resolve.</b> A migração inicial criou quatro tabelas de log particionadas por mês com uma janela
/// FIXA: do terceiro mês anterior ao décimo segundo seguinte (<c>ModeloInicial.Particionamento.cs</c>). A fase 1 do
/// documento 41 removeu três delas, que nunca receberam uma linha; ficou a trilha de alterações. O comentário
/// dela dizia que abrir os meses seguintes era "trabalho de job mensal" — e o job não existia. Nenhuma linha é recusada
/// depois do último limite (a função do SQL Server cobre o domínio inteiro), mas tudo cai na última partição, que cresce
/// sem fim, e o expurgo por partição deixa de funcionar para ela.</para>
///
/// <para><b>Os dois passos.</b></para>
/// <list type="number">
///   <item><b>Abrir os meses seguintes de toda tabela particionada por mês</b> — as funções <c>PF_Mensal_*</c> que o
///   banco tem, lidas do catálogo, para a próxima tabela particionada entrar sem ninguém lembrar daqui —, com
///   <c>SPLIT RANGE</c>, até o último limite ficar
///   <see cref="MesesDeFolga"/> meses depois do mês corrente: cada um dos próximos três meses tem partição própria.
///   Partir a última partição, que ainda está vazia, é só metadado.</item>
///   <item><b>Tirar da trilha de alterações o que passou de 18 meses</b> (D-10, decidida em 20/09/2026), uma partição
///   por vez, com <c>TRUNCATE TABLE ... WITH (PARTITIONS (n))</c> — <b>nunca</b> <c>DELETE</c>, que numa tabela de log
///   incharia o log de transação e travaria a tabela viva. Só sai partição INTEIRA anterior ao corte: o corte é o
///   primeiro dia do mês de 18 meses atrás, e nenhuma linha mais nova que isso é tocada. Depois, os limites antigos são
///   unidos (<c>MERGE RANGE</c>), para a função não acumular partições vazias.</item>
/// </list>
///
/// <para><b>A mudança de permissão fica.</b> O documento 05 §10 declara a trilha das tabelas de <c>seguranca</c>
/// PERMANENTE, e ela mora na mesma tabela que expira em 18 meses. Por isso, na mesma transação, as linhas das
/// entidades de <c>seguranca</c> da partição são copiadas para uma tabela temporária, a partição é esvaziada e elas
/// voltam — com o mesmo identificador. São poucas (conceder perfil é raro), e nenhuma linha é apagada por
/// <c>DELETE</c>.</para>
///
/// <para><b>Só a trilha de alterações tem retenção aplicada.</b> É a que a D-10 decidiu. Outra tabela particionada que
/// vier ganha os meses, e a retenção dela é decisão dela.</para>
///
/// <para><b>Idempotente.</b> Rodar duas vezes no mesmo mês não muda nada: os meses já estão abertos, as partições
/// vencidas já estão vazias (ou só com mudança de permissão, que não é expurgada), e os limites antigos já foram unidos.</para>
/// </summary>
/// <param name="banco">O contexto do banco do CRM, com permissão de DDL (a mesma que aplica as migrações).</param>
public sealed class ManutencaoDasParticoes(CrmDbContext banco)
{
    /// <summary>O fluxo da trava: duas rodadas ao mesmo tempo partiriam a mesma função duas vezes.</summary>
    public const string Fluxo = "PARTICAO_AUDITORIA";

    /// <summary>A retenção da trilha de alterações (D-10, documento 45 §5.3).</summary>
    public const int MesesDeRetencaoDaTrilha = 18;

    /// <summary>Quantos meses depois do corrente têm de ter partição própria.</summary>
    public const int MesesDeFolga = 3;

    /// <summary>O prefixo das funções de partição mensais; o resto do nome é a tabela.</summary>
    public const string PrefixoDaFuncao = "PF_Mensal_";

    private const string FuncaoDaTrilha = "PF_Mensal_AlteracaoDeCampo";

    /// <summary>Roda a manutenção.</summary>
    /// <param name="agoraUtc">O instante de referência — o mês corrente sai dele.</param>
    /// <param name="simular">Só conta: não parte, não esvazia e não une nada.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<ResultadoDaManutencaoDasParticoes> ExecutarAsync(DateTime agoraUtc, bool simular, CancellationToken ct)
    {
        var detalhes = new List<string>();
        var inicioDoMes = new DateTime(agoraUtc.Year, agoraUtc.Month, 1);
        var ultimoLimiteExigido = inicioDoMes.AddMonths(MesesDeFolga + 1);
        var corte = inicioDoMes.AddMonths(-MesesDeRetencaoDaTrilha);

        // ---------------------------------------------------------------------------------------------------------
        // 1. Os meses seguintes, em toda tabela particionada por mês que o banco tem.
        // ---------------------------------------------------------------------------------------------------------
        var abertos = 0;
        foreach (var funcao in await LerFuncoesMensaisAsync(ct))
        {
            var tabela = funcao[PrefixoDaFuncao.Length..];
            var limites = await LerLimitesAsync(funcao, ct);
            if (limites.Count == 0)
            {
                detalhes.Add($"{tabela}: a função {funcao} não tem limite — nada a abrir.");
                continue;
            }

            var novos = new List<DateTime>();
            for (var mes = limites[^1].AddMonths(1); mes <= ultimoLimiteExigido; mes = mes.AddMonths(1))
                novos.Add(mes);

            var esquema = $"PS_Mensal_{tabela}";
            foreach (var mes in novos)
            {
                // `NEXT USED` antes de todo SPLIT: o esquema foi criado com `ALL TO ([PRIMARY])`, que já o mantém, mas
                // dizê-lo aqui é o que torna o comando independente de como o esquema nasceu. Os nomes chegam como
                // parâmetro e viram identificador por QUOTENAME; o limite vai como parâmetro de verdade.
                if (!simular)
                    await banco.Database.ExecuteSqlAsync($"""
                        DECLARE @sql nvarchar(max) =
                              N'ALTER PARTITION SCHEME ' + QUOTENAME({esquema}) + N' NEXT USED [PRIMARY]; '
                            + N'ALTER PARTITION FUNCTION ' + QUOTENAME({funcao}) + N'() SPLIT RANGE (@limite);';
                        EXEC sys.sp_executesql @sql, N'@limite datetime2(3)', @limite = {mes};
                        """, ct);

                abertos++;
            }

            detalhes.Add(novos.Count == 0
                ? $"{tabela}: os meses até {ultimoLimiteExigido:yyyy-MM} já estavam abertos."
                : $"{tabela}: {(simular ? "abriria" : "abriu")} {novos.Count} mês(es), de {novos[0]:yyyy-MM} a {novos[^1]:yyyy-MM}.");
        }

        // ---------------------------------------------------------------------------------------------------------
        // 2. A retenção da trilha de alterações: as partições inteiras anteriores ao corte.
        // ---------------------------------------------------------------------------------------------------------
        var listaDePermanentes = string.Join(',', EntidadesDePermissao());

        var limitesDaTrilha = await LerLimitesAsync(FuncaoDaTrilha, ct);

        // A partição n (a partir de 1) vai do limite n-1 ao limite n; ela é inteira anterior ao corte quando o limite de
        // cima é menor ou igual a ele. A da esquerda de tudo não tem limite de baixo e entra pela mesma regra.
        var vencidas = limitesDaTrilha.Count(l => l <= corte);
        int expurgadas = 0;
        long linhasExpurgadas = 0, mantidas = 0;

        for (var particao = 1; particao <= vencidas; particao++)
        {
            var (total, dePermissao) = await ContarAsync(particao, listaDePermanentes, ct);
            if (total == dePermissao) continue;

            if (!simular) await EsvaziarMantendoAsPermissoesAsync(particao, listaDePermanentes, ct);

            expurgadas++;
            linhasExpurgadas += total - dePermissao;
            mantidas += dePermissao;
            detalhes.Add(
                $"trilha, partição {particao} (até {limitesDaTrilha[particao - 1]:yyyy-MM}): " +
                $"{(simular ? "sairiam" : "saíram")} {total - dePermissao:N0} linha(s); ficaram {dePermissao:N0} mudança(s) de permissão.");
        }

        if (vencidas == 0)
            detalhes.Add($"trilha: nenhuma partição inteira anterior ao corte de {corte:yyyy-MM} — nada a expurgar.");

        // Os limites antigos são unidos DEPOIS do expurgo: partição vazia se une só com metadado, e a que guarda
        // mudança de permissão move poucas linhas. O limite do próprio corte fica — é ele que separa o que vale do que
        // já passou.
        var aUnir = limitesDaTrilha.Where(l => l < corte).ToList();
        if (!simular)
            foreach (var limite in aUnir)
                await banco.Database.ExecuteSqlAsync($"""
                    EXEC sys.sp_executesql
                        N'ALTER PARTITION FUNCTION [PF_Mensal_AlteracaoDeCampo]() MERGE RANGE (@limite);',
                        N'@limite datetime2(3)', @limite = {limite};
                    """, ct);

        if (aUnir.Count > 0)
            detalhes.Add($"trilha: {(simular ? "uniria" : "uniu")} {aUnir.Count} limite(s) anterior(es) a {corte:yyyy-MM}.");

        return new ResultadoDaManutencaoDasParticoes(abertos, expurgadas, linhasExpurgadas, mantidas, aUnir.Count, detalhes);
    }

    /// <summary>
    /// As entidades cuja mudança é permanente: as do schema <c>seguranca</c> (documento 05 §10). Saem do próprio modelo,
    /// e não de uma lista escrita à mão, para a tabela nova de segurança entrar sem ninguém lembrar daqui.
    /// </summary>
    public IReadOnlyList<string> EntidadesDePermissao() =>
        banco.Model.GetEntityTypes()
            .Where(t => string.Equals(t.GetSchema(), "seguranca", StringComparison.Ordinal))
            .Select(t => t.ClrType.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

    /// <summary>As funções de partição mensais do banco, pelo nome — as que o banco tem, e não uma lista escrita aqui.</summary>
    public async Task<List<string>> LerFuncoesMensaisAsync(CancellationToken ct)
    {
        const string Sql = "SELECT name FROM sys.partition_functions WHERE name LIKE N'PF[_]Mensal[_]%' ORDER BY name;";

        var funcoes = new List<string>();
        await ComComandoAsync(Sql, async comando =>
        {
            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct)) funcoes.Add(leitor.GetString(0));
        }, ct);

        return funcoes;
    }

    private async Task<List<DateTime>> LerLimitesAsync(string funcao, CancellationToken ct)
    {
        const string Sql = """
            SELECT CAST(prv.value AS datetime2(3))
            FROM sys.partition_range_values prv
            JOIN sys.partition_functions pf ON pf.function_id = prv.function_id
            WHERE pf.name = @funcao
            ORDER BY prv.boundary_id;
            """;

        var limites = new List<DateTime>();
        await ComComandoAsync(Sql, async comando =>
        {
            var parametro = comando.CreateParameter();
            parametro.ParameterName = "@funcao";
            parametro.Value = funcao;
            comando.Parameters.Add(parametro);

            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct)) limites.Add(leitor.GetDateTime(0));
        }, ct);

        return limites;
    }

    private async Task<(long Total, long DePermissao)> ContarAsync(int particao, string listaDePermanentes, CancellationToken ct)
    {
        // A lista das entidades permanentes entra por JOIN, e não por subconsulta dentro do COUNT: o SQL Server não
        // aceita agregação sobre expressão que contém subconsulta. Os nomes são distintos, e a junção não duplica linha.
        const string Sql = """
            SELECT COUNT_BIG(*), COUNT_BIG(p.[value])
            FROM [auditoria].[AlteracaoDeCampo] a
            LEFT JOIN STRING_SPLIT(@permanentes, N',') p ON p.[value] = a.[Entidade]
            WHERE $PARTITION.[PF_Mensal_AlteracaoDeCampo](a.[AlteradoEm]) = @particao;
            """;

        (long, long) contagem = (0, 0);
        await ComComandoAsync(Sql, async comando =>
        {
            Parametro(comando, "@particao", particao);
            Parametro(comando, "@permanentes", listaDePermanentes);

            await using var leitor = await comando.ExecuteReaderAsync(ct);
            if (await leitor.ReadAsync(ct)) contagem = (leitor.GetInt64(0), leitor.GetInt64(1));
        }, ct);

        return contagem;
    }

    /// <summary>
    /// Esvazia a partição e devolve as mudanças de permissão dela, numa transação só: se algo falhar no meio, nada saiu.
    /// A lista de colunas vem do catálogo do banco, e o <c>IDENTITY_INSERT</c> mantém o identificador de cada linha. O
    /// número da partição e a lista das entidades chegam como parâmetro; o <c>TRUNCATE</c> só aceita o número escrito,
    /// e por isso ele é montado dentro do próprio T-SQL, a partir do parâmetro inteiro.
    /// </summary>
    private Task EsvaziarMantendoAsPermissoesAsync(int particao, string listaDePermanentes, CancellationToken ct) =>
        banco.Database.ExecuteSqlAsync($"""
            SET XACT_ABORT ON;
            DECLARE @particao int = {particao};
            DECLARE @permanentes nvarchar(max) = {listaDePermanentes};

            BEGIN TRANSACTION;

            SELECT * INTO #mantidas
            FROM [auditoria].[AlteracaoDeCampo]
            WHERE $PARTITION.[PF_Mensal_AlteracaoDeCampo]([AlteradoEm]) = @particao
              AND [Entidade] IN (SELECT [value] FROM STRING_SPLIT(@permanentes, N','));

            DECLARE @esvaziar nvarchar(max) =
                N'TRUNCATE TABLE [auditoria].[AlteracaoDeCampo] WITH (PARTITIONS (' + CAST(@particao AS nvarchar(10)) + N'));';
            EXEC sys.sp_executesql @esvaziar;

            IF EXISTS (SELECT 1 FROM #mantidas)
            BEGIN
                DECLARE @colunas nvarchar(max) = (
                    SELECT STRING_AGG(CAST(QUOTENAME(c.name) AS nvarchar(max)), N', ') WITHIN GROUP (ORDER BY c.column_id)
                    FROM sys.columns c
                    WHERE c.object_id = OBJECT_ID(N'auditoria.AlteracaoDeCampo')
                      AND c.is_computed = 0
                      AND TYPE_NAME(c.system_type_id) <> N'timestamp');

                DECLARE @comIdentidade bit =
                    CASE WHEN EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'auditoria.AlteracaoDeCampo'))
                         THEN 1 ELSE 0 END;

                DECLARE @devolver nvarchar(max) =
                      CASE WHEN @comIdentidade = 1 THEN N'SET IDENTITY_INSERT [auditoria].[AlteracaoDeCampo] ON; ' ELSE N'' END
                    + N'INSERT INTO [auditoria].[AlteracaoDeCampo] (' + @colunas + N') SELECT ' + @colunas + N' FROM #mantidas; '
                    + CASE WHEN @comIdentidade = 1 THEN N'SET IDENTITY_INSERT [auditoria].[AlteracaoDeCampo] OFF;' ELSE N'' END;

                EXEC sys.sp_executesql @devolver;
            END;

            DROP TABLE #mantidas;
            COMMIT TRANSACTION;
            """, ct);

    private static void Parametro(System.Data.Common.DbCommand comando, string nome, object valor)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = nome;
        parametro.Value = valor;
        comando.Parameters.Add(parametro);
    }

    private async Task ComComandoAsync(string sql, Func<System.Data.Common.DbCommand, Task> usar, CancellationToken ct)
    {
        var conexao = banco.Database.GetDbConnection();
        var abriAqui = conexao.State != ConnectionState.Open;
        if (abriAqui) await conexao.OpenAsync(ct);

        try
        {
            await using var comando = conexao.CreateCommand();
            comando.CommandText = sql;
            comando.CommandTimeout = 600;
            await usar(comando);
        }
        finally
        {
            if (abriAqui) await conexao.CloseAsync();
        }
    }
}
