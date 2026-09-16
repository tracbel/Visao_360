using System.Data.Common;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Infraestrutura.Persistencia;

/// <summary>
/// A TRILHA DE AUDITORIA AUTOMÁTICA — o que o <see cref="CrmDbContext"/> grava em
/// <c>auditoria.AlteracaoDeCampo</c> a cada <c>SaveChanges</c> (documento 41, fase 2).
///
/// <para><b>Duas etapas, e a ordem é obrigatória.</b> A CAPTURA roda antes de salvar, porque é só
/// ali que o rastreador de mudanças ainda sabe o valor ORIGINAL de cada campo. A GRAVAÇÃO roda
/// depois de salvar, porque é só ali que o registro que acabou de nascer tem <c>Id</c> — a coluna é
/// <c>IDENTITY</c>, e sem o <c>Id</c> a linha da trilha não teria para onde apontar. As duas
/// acontecem dentro da MESMA transação do dado: ou grava o dado e a trilha, ou nenhum dos dois.</para>
///
/// <para><b>Por que a gravação é um <c>INSERT</c> direto, e não uma entidade rastreada.</b> Na API
/// a conexão tem retentativa em falha transitória, e o padrão que o EF Core documenta para ela é
/// salvar com <c>acceptAllChangesOnSuccess: false</c> e só aceitar depois do commit. Nesse estado
/// o registro novo continua marcado como "a incluir"; um segundo <c>SaveChanges</c> para gravar a
/// trilha o incluiria DE NOVO. O comando direto grava só a trilha, na transação aberta, e a
/// retentativa pode repetir o bloco inteiro sem duplicar nada.</para>
///
/// <para><b>Erro aqui é erro da gravação.</b> Nada é engolido: se a trilha não puder ser gravada,
/// a exceção sobe, a transação é desfeita e o dado não é gravado sem rastro.</para>
/// </summary>
internal static class TrilhaDeAuditoria
{
    /// <summary>O tamanho das colunas de valor. O que passar disso é cortado, nunca recusado.</summary>
    private const int TamanhoDoValor = 400;

    /// <summary>
    /// Linhas por comando no SQL Server: doze parâmetros por linha, e o limite do protocolo é de
    /// 2.100 parâmetros por comando.
    /// </summary>
    private const int LinhasPorComando = 150;

    /// <summary>Uma linha da trilha esperando o <c>Id</c> do registro para ser gravada.</summary>
    internal sealed record Pendencia(
        EntityEntry Entrada,
        string Entidade,
        string Campo,
        string? ValorAnterior,
        string? ValorNovo,
        OperacaoAuditada Operacao,
        int? EmpresaDoRegistro,
        long? AutorDoRegistro);

    /// <summary>
    /// Lê do rastreador de mudanças o que a política manda auditar. Chamada ANTES de salvar.
    /// </summary>
    /// <param name="rastreador">O rastreador do contexto.</param>
    /// <param name="origem">De onde vêm as gravações deste contexto.</param>
    public static List<Pendencia> Capturar(ChangeTracker rastreador, OrigemDaOperacao origem)
    {
        var pendencias = new List<Pendencia>();

        foreach (var entrada in rastreador.Entries().ToList())
        {
            if (entrada.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            var entidade = entrada.Metadata.ClrType.Name;
            var campos = PoliticaDeAuditoria.CamposDe(entidade);
            if (campos.Count == 0) continue;

            if (entrada.State == EntityState.Added && !PoliticaDeAuditoria.RegistraInclusao(origem)) continue;

            var operacao = OperacaoDa(entrada);
            var empresa = LerEmpresa(entrada);
            var autor = LerAutor(entrada);

            foreach (var campo in campos)
            {
                var metadado = entrada.Metadata.FindProperty(campo)
                    ?? throw new InvalidOperationException(
                        $"PoliticaDeAuditoria manda auditar {entidade}.{campo}, e a entidade não tem esse campo " +
                        "mapeado. A política precisa acompanhar a entidade no mesmo commit.");

                var propriedade = entrada.Property(campo);
                if (entrada.State == EntityState.Modified && !propriedade.IsModified) continue;

                var anterior = entrada.State == EntityState.Added ? null : Formatar(metadado, propriedade.OriginalValue);
                var novo = entrada.State == EntityState.Deleted ? null : Formatar(metadado, propriedade.CurrentValue);

                if (MesmoValor(anterior, novo)) continue;

                pendencias.Add(new Pendencia(entrada, entidade, campo, anterior, novo, operacao, empresa, autor));
            }
        }

        return pendencias;
    }

    /// <summary>
    /// Grava as linhas capturadas, na transação aberta do contexto. Chamada DEPOIS de salvar.
    /// </summary>
    public static async Task GravarAsync(
        CrmDbContext contexto, IReadOnlyList<Pendencia> pendencias, TrilhaDoContexto trilha, CancellationToken ct)
    {
        foreach (var (sql, parametros) in Comandos(contexto, pendencias, trilha))
            await contexto.Database.ExecuteSqlRawAsync(sql, parametros, ct);
    }

    /// <summary>A versão síncrona de <see cref="GravarAsync"/>.</summary>
    public static void Gravar(CrmDbContext contexto, IReadOnlyList<Pendencia> pendencias, TrilhaDoContexto trilha)
    {
        foreach (var (sql, parametros) in Comandos(contexto, pendencias, trilha))
            contexto.Database.ExecuteSqlRaw(sql, parametros);
    }

    /// <summary>O que o contexto sabe sobre quem e de onde, no momento da gravação.</summary>
    internal readonly record struct TrilhaDoContexto(
        OrigemDaOperacao Origem, int? SistemaId, Guid CorrelacaoId, long UsuarioId, int EmpresaId);

    // ---------------------------------------------------------------------------------------------

    private static IEnumerable<(string Sql, DbParameter[] Parametros)> Comandos(
        CrmDbContext contexto, IReadOnlyList<Pendencia> pendencias, TrilhaDoContexto trilha)
    {
        if (pendencias.Count == 0) yield break;

        var tipo = contexto.Model.FindEntityType(typeof(AlteracaoDeCampo))
            ?? throw new InvalidOperationException("AlteracaoDeCampo não está no modelo.");
        var tabela = StoreObjectIdentifier.Table(tipo.GetTableName()!, tipo.GetSchema());
        var sqlHelper = contexto.GetService<ISqlGenerationHelper>();

        string Coluna(string propriedade) =>
            sqlHelper.DelimitIdentifier(tipo.FindProperty(propriedade)!.GetColumnName(tabela)!);

        var nomeDaTabela = sqlHelper.DelimitIdentifier(tabela.Name, tabela.Schema);
        var ehSqlServer = contexto.Database.IsSqlServer();

        string[] colunas =
        [
            nameof(AlteracaoDeCampo.EmpresaId), nameof(AlteracaoDeCampo.Entidade), nameof(AlteracaoDeCampo.RegistroId),
            nameof(AlteracaoDeCampo.Campo), nameof(AlteracaoDeCampo.ValorAnterior), nameof(AlteracaoDeCampo.ValorNovo),
            nameof(AlteracaoDeCampo.AlteradoEm), nameof(AlteracaoDeCampo.AlteradoPorId), nameof(AlteracaoDeCampo.CorrelacaoId),
            nameof(AlteracaoDeCampo.Origem), nameof(AlteracaoDeCampo.SistemaId), nameof(AlteracaoDeCampo.Operacao)
        ];
        var listaDeColunas = string.Join(", ", colunas.Select(Coluna));
        var agora = DateTime.UtcNow;
        var fabrica = contexto.Database.GetDbConnection();

        var linhas = pendencias.Select(p => Valores(p, trilha, agora)).ToList();

        if (ehSqlServer)
        {
            // O SQL Server gera o Id (IDENTITY) e aceita várias linhas por comando.
            for (var inicio = 0; inicio < linhas.Count; inicio += LinhasPorComando)
            {
                var bloco = linhas.Skip(inicio).Take(LinhasPorComando).ToList();
                var parametros = new List<DbParameter>(bloco.Count * colunas.Length);
                var sql = new StringBuilder($"INSERT INTO {nomeDaTabela} ({listaDeColunas}) VALUES ");

                for (var i = 0; i < bloco.Count; i++)
                {
                    if (i > 0) sql.Append(", ");
                    sql.Append('(');
                    for (var c = 0; c < colunas.Length; c++)
                    {
                        if (c > 0) sql.Append(", ");
                        var nome = $"@t{parametros.Count}";
                        sql.Append(nome);
                        parametros.Add(Parametro(fabrica, nome, bloco[i][c]));
                    }
                    sql.Append(')');
                }

                yield return (sql.ToString(), parametros.ToArray());
            }

            yield break;
        }

        // FORA DO SQL SERVER — o SQLite dos testes de comportamento — a chave composta (Id,
        // AlteradoEm) não gera Id sozinha: o SQLite só gera valor para chave primária de uma coluna.
        // Aqui o Id é o próximo número, uma linha por comando para o MAX enxergar a anterior.
        var colunaId = Coluna(nameof(AlteracaoDeCampo.Id));
        foreach (var linha in linhas)
        {
            var parametros = linha.Select((valor, c) => Parametro(fabrica, $"@t{c}", valor)).ToArray();
            var sql = $"INSERT INTO {nomeDaTabela} ({colunaId}, {listaDeColunas}) " +
                      $"VALUES ((SELECT COALESCE(MAX({colunaId}), 0) + 1 FROM {nomeDaTabela}), " +
                      string.Join(", ", parametros.Select(p => p.ParameterName)) + ")";
            yield return (sql, parametros);
        }
    }

    private static object?[] Valores(Pendencia p, TrilhaDoContexto trilha, DateTime agora)
    {
        var registroId = Convert.ToInt64(p.Entrada.Property("Id").CurrentValue, CultureInfo.InvariantCulture);

        // A FILIAL DA LINHA é a do registro; entidade sem filial (município, catálogo) fica com a
        // filial de casa de quem grava. Sem nenhuma das duas não há o que gravar em EmpresaId.
        var empresa = p.EmpresaDoRegistro is > 0 ? p.EmpresaDoRegistro.Value
            : trilha.EmpresaId > 0 ? trilha.EmpresaId
            : throw new InvalidOperationException(
                $"Não há filial para a trilha de {p.Entidade}.{p.Campo}: o registro não tem EmpresaId e o " +
                "contexto de acesso não tem filial de casa. Gravação auditada sem filial não é gravada.");

        // O AUTOR é quem está agindo. O contexto do sistema (usuário 0) não é autor de nada: nele,
        // vale quem a entidade registrou como autor da mudança.
        var autor = trilha.UsuarioId > 0 ? trilha.UsuarioId
            : p.AutorDoRegistro is > 0 ? p.AutorDoRegistro.Value
            : throw new InvalidOperationException(
                $"Não há autor para a trilha de {p.Entidade}.{p.Campo}: o contexto de acesso não identifica " +
                "usuário e o registro não traz quem o alterou. Gravação auditada sem autor não é gravada.");

        return
        [
            empresa, p.Entidade, registroId, p.Campo, p.ValorAnterior, p.ValorNovo,
            agora, autor, trilha.CorrelacaoId, trilha.Origem.ToString(), trilha.SistemaId, p.Operacao.ToString()
        ];
    }

    private static DbParameter Parametro(DbConnection conexao, string nome, object? valor)
    {
        using var comando = conexao.CreateCommand();
        var parametro = comando.CreateParameter();
        parametro.ParameterName = nome;
        parametro.Value = valor ?? DBNull.Value;

        // Sem isto o driver do SQL Server manda DateTime como `datetime`, arredondando para 3,33 ms
        // antes de chegar à coluna datetime2(3).
        if (valor is DateTime && conexao is Microsoft.Data.SqlClient.SqlConnection) parametro.DbType = System.Data.DbType.DateTime2;

        return parametro;
    }

    private static OperacaoAuditada OperacaoDa(EntityEntry entrada)
    {
        if (entrada.State == EntityState.Added) return OperacaoAuditada.Inclusao;
        if (entrada.State == EntityState.Deleted) return OperacaoAuditada.Exclusao;

        // A exclusão LÓGICA é exclusão, não alteração: é o que alguém procura quando um registro
        // "sumiu" da tela.
        var exclusao = entrada.Metadata.FindProperty(PoliticaDeAuditoria.CampoDeExclusaoLogica);
        if (exclusao is not null)
        {
            var campo = entrada.Property(PoliticaDeAuditoria.CampoDeExclusaoLogica);
            if (campo.IsModified && campo.OriginalValue is null && campo.CurrentValue is not null)
                return OperacaoAuditada.Exclusao;
        }

        return OperacaoAuditada.Alteracao;
    }

    private static int? LerEmpresa(EntityEntry entrada)
    {
        if (entrada.Metadata.FindProperty("EmpresaId") is null) return null;
        var propriedade = entrada.Property("EmpresaId");
        var valor = entrada.State == EntityState.Deleted ? propriedade.OriginalValue : propriedade.CurrentValue;
        return valor is null ? null : Convert.ToInt32(valor, CultureInfo.InvariantCulture);
    }

    private static long? LerAutor(EntityEntry entrada) =>
        entrada.Entity is EntidadeBase entidade
            ? entidade.AlteradoPorId is > 0 ? entidade.AlteradoPorId : entidade.CriadoPorId
            : null;

    /// <summary>
    /// O valor como TEXTO LEGÍVEL: o que o banco guarda, e não o objeto do domínio. Um documento vira
    /// os dígitos, um enum vira o nome, uma data vira ISO.
    /// </summary>
    private static string? Formatar(IProperty propriedade, object? valor)
    {
        if (valor is null) return null;

        var conversor = propriedade.GetTypeMapping().Converter ?? propriedade.GetValueConverter();
        var gravado = conversor is null ? valor : conversor.ConvertToProvider(valor);
        if (gravado is null) return null;

        var texto = gravado switch
        {
            string s => s,
            DateTime d => d.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture),
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => gravado.ToString()
        };

        return texto is { Length: > TamanhoDoValor } ? texto[..TamanhoDoValor] : texto;
    }

    /// <summary>
    /// "MUDOU?" COMO O BANCO PERGUNTA. <c>CK_AlteracaoDeCampo_Mudou</c> compara o antes e o depois
    /// na colação do banco, que ignora maiúscula e acento: para ela, "RIBEIRAO PRETO" e "Ribeirão
    /// Preto" são o mesmo texto, e a linha seria recusada — derrubando a gravação do dado junto. A
    /// comparação aqui ignora as mesmas duas coisas, e o que o banco não reconhece como mudança não
    /// vai para a trilha.
    /// </summary>
    private static bool MesmoValor(string? anterior, string? novo)
    {
        if (anterior is null || novo is null) return anterior is null && novo is null;

        return string.Compare(anterior, novo, CultureInfo.InvariantCulture,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
    }
}
