using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Padrão de banco — integridade referencial.
///
/// Cobre o documento 14, seção 6. É a seção que ataca o achado mais caro do diagnóstico:
/// <c>IV_Agenda</c>, <c>IV_Historico</c> e <c>IV_ProcDado</c> não têm FK para <c>IV_Processo</c>,
/// e isso produziu 345.535 linhas órfãs.
/// </summary>
[Trait("Categoria", "PadraoDeBanco")]
public sealed class IntegridadeReferencialTestes
{
    /// <summary>
    /// Relacionamentos em que a exclusão em cascata é uma decisão registrada, não um acidente
    /// de convenção do EF Core.
    /// </summary>
    private static readonly Dictionary<(string TabelaDependente, string NomeFk), string> CascadeJustificado = new()
    {
        // O item não é entidade independente: é a coleção OWNED do agregado
        // ConjuntoDePermissao. Apagar o conjunto apaga os itens dele, do mesmo jeito que
        // apagar um Pedido apagaria os ItensPedido.
        [("ConjuntoDePermissaoItem", "FK_ConjuntoDePermissaoItem_ConjuntoDePermissao_ConjuntoPermissaoId")] =
            "coleção owned do agregado ConjuntoDePermissao — linha órfã aqui não existe de propósito"

        // A SEGUNDA justificativa saiu na fase 1 (documento 41): era
        // FK_Vinculo_Documento_DocumentoId, do documento 04, seção 10 — o vínculo sem documento é o
        // registro órfão que trava o sincronismo do aplicativo de campo ([V] 5.302 deles no
        // Vórtice, 6,9% do total). As duas tabelas, documento.Documento e documento.Vinculo, nunca
        // receberam uma linha e saíram; a regra volta com elas, quando houver anexo de verdade.
    };

    [Fact]
    public void Toda_foreign_key_declarada_tem_indice_pelo_prefixo_das_colunas()
    {
        // [V] "Foreign keys: 672 — nenhuma no caminho quente do BPM" (diagnóstico, seção 2):
        // ter a FK declarada não bastou, porque sem índice toda checagem de integridade e todo
        // JOIN varrem a tabela inteira. Aqui a FK sem índice nem chega a existir sem alarme.
        var semIndice = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            foreach (var fk in tabela.GetForeignKeys())
            {
                var colunasFk = fk.Properties;
                var coberta = tabela.GetIndexes()
                    .Any(ix => ix.Properties.Count >= colunasFk.Count
                            && ix.Properties.Take(colunasFk.Count).SequenceEqual(colunasFk));

                // A própria PK e as chaves alternativas também cobrem, quando a FK é (parte
                // d)elas.
                var cobertaPorChave = tabela.GetKeys()
                    .Any(k => k.Properties.Count >= colunasFk.Count
                           && k.Properties.Take(colunasFk.Count).SequenceEqual(colunasFk));

                if (!coberta && !cobertaPorChave)
                    semIndice.Add($"{tabela.GetTableName()}.{fk.GetConstraintName()} " +
                                  $"({string.Join(", ", colunasFk.Select(p => p.GetColumnName()))})");
            }
        }

        semIndice.Should().BeEmpty(
            "toda foreign key precisa ter as colunas cobertas por um índice — próprio ou como " +
            "prefixo de um composto (documento 14, seção 7). Sem índice: {0}",
            string.Join("; ", semIndice));
    }

    [Fact]
    public void Toda_referencia_a_entidade_mapeada_tem_foreign_key_declarada()
    {
        // HEURÍSTICA POR NOME, e é preciso ser honesto sobre o limite dela: ela só enxerga uma
        // referência quando a propriedade se chama EXATAMENTE "<NomeDaEntidade>Id" (ex.:
        // "RegraId" -> entidade "Regra"). Referência por PAPEL — "ProprietarioId" apontando
        // para "Usuario" — não é pega aqui, porque o nome não bate. Essa classe de caso
        // continua dependendo da revisão de PR (documento 14, seção 15).
        //
        // Mesmo limitada, esta heurística tem dentes: ela CRESCE junto com o modelo. Com 63
        // entidades mapeadas, quase toda coluna de referência passa a ser alcançada por ela.
        // [V] é exatamente a lacuna que produziu 345.535 linhas órfãs em IV_ProcDado.
        var entidadesPorNome = ModeloBanco.Modelo.GetEntityTypes()
            .ToDictionary(t => t.ClrType.Name, t => t, StringComparer.Ordinal);

        var semFk = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            foreach (var propriedade in tabela.GetProperties())
            {
                if (propriedade.Name is "Id" || !propriedade.Name.EndsWith("Id", StringComparison.Ordinal))
                    continue;

                var nomeCandidato = propriedade.Name[..^"Id".Length];
                if (!entidadesPorNome.TryGetValue(nomeCandidato, out var entidadeAlvo))
                    continue; // não é o nome exato de nenhuma entidade mapeada — fora do alcance.

                if (entidadeAlvo == tabela && tabela.FindPrimaryKey()!.Properties.Contains(propriedade))
                    continue; // autorreferência que É a própria PK não se aplica aqui.

                var jaTemFk = tabela.GetForeignKeys().Any(fk => fk.Properties.Contains(propriedade));
                if (!jaTemFk)
                    semFk.Add($"{tabela.GetTableName()}.{propriedade.GetColumnName()} -> {nomeCandidato}");
            }
        }

        semFk.Should().BeEmpty(
            "toda coluna cujo nome bate exatamente com '<Entidade>Id' de uma entidade já " +
            "mapeada precisa de uma foreign key declarada (documento 14, seção 6 — 'toda FK é " +
            "declarada no banco'). Sem FK: {0}",
            string.Join("; ", semFk));
    }

    [Fact]
    public void Exclusao_em_cascata_so_e_usada_na_lista_de_excecoes_documentadas()
    {
        // Cascade apagando em silêncio é como o Vórtice perde rastro sem ninguém decidir isso
        // de propósito. O padrão do projeto é Restrict; Cascade exige registrar o motivo aqui.
        var cascadesNaoDocumentados = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys(), (t, fk) => new { Tabela = t, Fk = fk })
            .Where(x => x.Fk.DeleteBehavior == DeleteBehavior.Cascade)
            .Where(x => !CascadeJustificado.ContainsKey((x.Tabela.GetTableName()!, x.Fk.GetConstraintName()!)))
            .Select(x => $"{x.Tabela.GetTableName()}.{x.Fk.GetConstraintName()}")
            .ToList();

        cascadesNaoDocumentados.Should().BeEmpty(
            "DeleteBehavior.Cascade só é aceito quando listado em 'CascadeJustificado' " +
            "(documento 14, seção 6) — a exclusão em cascata precisa ser uma decisão " +
            "registrada, não a convenção padrão do EF Core para FK obrigatória. Sem " +
            "justificativa: {0}",
            string.Join(", ", cascadesNaoDocumentados));
    }

    [Fact]
    public void Coluna_baseada_em_enum_tem_check_constraint_de_dominio()
    {
        // [V] "IV_Processo.Status tem mais de 20 valores de texto livre com duplicatas
        // semânticas: FINALIZADO (18.416) convive com FINALIZADA (11.563), e 437.694 linhas
        // estão em branco" (extração, achado 7) — porque o Vórtice tem ZERO CHECK constraints
        // em 767 tabelas. Aqui, todo enum gravado como texto tem o domínio fechado no banco,
        // não só no compilador do C#.
        var semCheck = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var checks = tabela.GetCheckConstraints().Select(c => c.Sql).ToList();

            foreach (var propriedade in tabela.GetProperties())
            {
                var tipoBase = Nullable.GetUnderlyingType(propriedade.ClrType) ?? propriedade.ClrType;
                if (!tipoBase.IsEnum) continue;

                var coluna = propriedade.GetColumnName();
                if (!checks.Any(sql => sql.Contains(coluna, StringComparison.OrdinalIgnoreCase)))
                    semCheck.Add($"{tabela.GetTableName()}.{coluna} ({tipoBase.Name})");
            }
        }

        semCheck.Should().BeEmpty(
            "toda coluna que representa um enum do C# precisa de CHECK constraint " +
            "enumerando os valores válidos (documento 14, seções 4 e 6) — o enum já é o " +
            "domínio fechado; falta o banco concordar. Sem CHECK: {0}",
            string.Join(", ", semCheck));
    }

    [Fact]
    public void Toda_restricao_de_verificacao_usa_sintaxe_de_SqlServer()
    {
        // A tradução mais barata de esquecer, agora no sentido inverso: no SQL Server o nome
        // da coluna vai ENTRE COLCHETES. Sem eles, um nome que coincida com palavra reservada
        // (Ordem, Nivel, Tipo, Valor...) faz a migration falhar na hora de aplicar, e não
        // aqui. Este teste faz falhar aqui.
        //
        // A checagem é pelo nome REAL da coluna: se um identificador que é coluna desta tabela
        // aparece fora de colchete e fora de literal de texto, é violação. Nome de função
        // (LEN, ISJSON, COALESCE) e palavra-chave de SQL não são colunas e passam.
        var comSintaxeErrada = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var colunas = tabela.GetProperties()
                .Select(p => p.GetColumnName())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var check in tabela.GetCheckConstraints())
            {
                var sql = SemLiteraisDeTexto(check.Sql);

                var soltos = System.Text.RegularExpressions.Regex
                    .Matches(sql, @"(?<!\[)\b[A-Za-z_][A-Za-z0-9_]*\b(?!\])")
                    .Select(m => m.Value)
                    .Where(colunas.Contains)
                    .Distinct()
                    .ToList();

                if (soltos.Count > 0)
                    comSintaxeErrada.Add(
                        $"{tabela.GetTableName()}.{check.Name}: {string.Join(", ", soltos)}");
            }
        }

        comSintaxeErrada.Should().BeEmpty(
            "restrição de verificação escreve o nome da coluna ENTRE COLCHETES — é a sintaxe " +
            "do SQL Server, e é o que impede uma coluna com nome de palavra reservada de " +
            "quebrar a migration. Sem colchete: {0}",
            string.Join("; ", comSintaxeErrada));
    }

    /// <summary>Apaga o conteúdo dos literais de texto, preservando o comprimento do SQL.</summary>
    private static string SemLiteraisDeTexto(string sql)
        => System.Text.RegularExpressions.Regex.Replace(sql, "'[^']*'", "''");

    [Fact]
    public void Todo_indice_filtrado_filtra_por_coluna_que_existe_na_tabela()
    {
        // Índice filtrado é o mecanismo do documento 04, seção 1.4, item 4 ("WHERE ExcluidoEm
        // IS NULL em todo índice de busca"). O filtro é texto cru: um nome de coluna escrito
        // errado só apareceria na hora de aplicar a migration.
        //
        // No SQL Server o predicado de índice filtrado é ainda mais restrito que o CHECK: só
        // aceita `coluna <operador> constante`, `coluna IS [NOT] NULL`, `coluna IN (...)` e
        // `AND` — sem OR e sem função. O teste exige, portanto, que TODO identificador entre
        // colchetes seja coluna da tabela, e que nenhum identificador solto sobre.
        var filtrosInvalidos = new List<string>();

        var palavrasChave = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "IS", "NOT", "NULL", "AND", "IN", "BETWEEN" };

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var colunas = tabela.GetProperties().Select(p => p.GetColumnName()).ToHashSet(StringComparer.Ordinal);

            foreach (var indice in tabela.GetIndexes())
            {
                var filtro = indice.GetFilter();
                if (string.IsNullOrWhiteSpace(filtro)) continue;

                var semLiterais = SemLiteraisDeTexto(filtro);

                // 1) Todo [Identificador] precisa ser coluna desta tabela.
                filtrosInvalidos.AddRange(
                    from m in System.Text.RegularExpressions.Regex.Matches(semLiterais, @"\[([A-Za-z0-9_]+)\]")
                    let identificador = m.Groups[1].Value
                    where !colunas.Contains(identificador)
                    select $"{tabela.GetTableName()}.{indice.GetDatabaseName()}: '{identificador}' " +
                           $"não é coluna da tabela (filtro: {filtro})");

                // 2) Nenhum identificador pode aparecer FORA de colchete — nem nome de coluna
                //    nem chamada de função (que o SQL Server recusa em índice filtrado).
                filtrosInvalidos.AddRange(
                    from m in System.Text.RegularExpressions.Regex.Matches(
                        semLiterais, @"(?<!\[)\b[A-Za-z_][A-Za-z0-9_]*\b(?!\])")
                    let palavra = m.Value
                    where !palavrasChave.Contains(palavra)
                    select $"{tabela.GetTableName()}.{indice.GetDatabaseName()}: '{palavra}' " +
                           $"está fora de colchete (filtro: {filtro})");
            }
        }

        filtrosInvalidos.Should().BeEmpty(
            "o filtro de índice é texto cru enviado ao banco: um nome de coluna errado, ou " +
            "escrito sem colchete, só falharia ao aplicar a migration. Inválidos: {0}",
            string.Join("; ", filtrosInvalidos));
    }
}
