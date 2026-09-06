// DicionarioGerador — gera docs/banco/DICIONARIO.md, docs/banco/ERD.md, docs/banco/catalogo.csv
// e docs/banco/esquema.json
// a partir do MODELO do EF Core (Tracbel.Crm.Infraestrutura.Persistencia.CrmDbContext) — nunca de
// um banco real. Mesma técnica dos testes de padrão de banco (ver
// tests/Tracbel.Crm.Arquitetura.Testes/Banco/ModeloBanco.cs): o provedor SQL Server é configurado
// só para resolver o tipo de coluna; nenhuma conexão é aberta.
//
// Por que isto documenta a verdade e não fica desatualizado: ele lê o modelo, não escreve à mão.
// Toda vez que uma entidade ou coluna nova entra no CrmDbContext, rodar este gerador de novo já
// atualiza os quatro arquivos — não existe "esquecer de atualizar o dicionário".
//
// Uso: dotnet run --project scripts/banco/DicionarioGerador -- [caminho-da-raiz-do-repositorio]
// Sem argumento, sobe a árvore de diretórios a partir daqui até achar o .sln (mesma lógica de
// ArquiteturaTestes.LocalizarRaizDoRepositorio).

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Persistencia;

var raiz = args.Length > 0 ? args[0] : LocalizarRaizDoRepositorio();
var pastaDocsBanco = Path.Combine(raiz, "docs", "banco");
Directory.CreateDirectory(pastaDocsBanco);

Console.WriteLine($"Raiz do repositório: {raiz}");

var modelo = ConstruirModelo();

// Uma linha por coluna, agrupada por schema e tabela — é a unidade que os quatro arquivos
// de saída compartilham, montada uma única vez.
var colunas = ColetarColunas(modelo).ToList();
var schemas = colunas.Select(c => c.Schema).Distinct().OrderBy(s => s, StringComparer.Ordinal).ToList();

EscreverCatalogoCsv(Path.Combine(pastaDocsBanco, "catalogo.csv"), colunas);
EscreverDicionarioMarkdown(Path.Combine(pastaDocsBanco, "DICIONARIO.md"), modelo, colunas, schemas);
EscreverErdMermaid(Path.Combine(pastaDocsBanco, "ERD.md"), modelo, schemas);
EscreverEsquemaJson(Path.Combine(pastaDocsBanco, "esquema.json"), colunas);

Console.WriteLine($"Gerado a partir de {colunas.Select(c => (c.Schema, c.Tabela)).Distinct().Count()} tabela(s) em {schemas.Count} schema(s), {colunas.Count} coluna(s) no total.");
Console.WriteLine("  docs/banco/DICIONARIO.md");
Console.WriteLine("  docs/banco/ERD.md");
Console.WriteLine("  docs/banco/catalogo.csv");
Console.WriteLine("  docs/banco/esquema.json");

// ---------------------------------------------------------------------------------------------

static string LocalizarRaizDoRepositorio()
{
    var atual = new DirectoryInfo(AppContext.BaseDirectory);
    while (atual is not null && atual.GetFiles("*.sln").Length == 0)
        atual = atual.Parent;

    return atual?.FullName
        ?? throw new InvalidOperationException("Não encontrei a raiz do repositório (nenhum .sln nos diretórios acima).");
}

static IModel ConstruirModelo()
{
    var opcoes = new DbContextOptionsBuilder<CrmDbContext>()
        // Só para o provedor SQL Server resolver o tipo de coluna — nunca conecta de verdade.
        .UseSqlServer("Server=localhost;Database=TracbelCrmDicionario;Integrated Security=true")
        .Options;

    using var contexto = new CrmDbContext(opcoes, new ProvedorDeSistemaFalso());

    // Modelo de DESIGN TIME: o único que expõe CheckConstraints (o modelo "read-optimized" de
    // runtime descarta esse metadado). Mesma escolha de tests/.../Banco/ModeloBanco.cs.
    return contexto.GetService<IDesignTimeModel>().Model;
}

static IEnumerable<InfoColuna> ColetarColunas(IModel modelo)
{
    foreach (var tabela in modelo.GetEntityTypes().OrderBy(t => t.GetSchema()).ThenBy(t => t.GetTableName(), StringComparer.Ordinal))
    {
        var schema = tabela.GetSchema() ?? "(sem schema)";
        var nomeTabela = tabela.GetTableName() ?? tabela.ClrType.Name;
        var pk = tabela.FindPrimaryKey();
        var indicesUnicos = tabela.GetIndexes().Where(i => i.IsUnique).SelectMany(i => i.Properties).ToHashSet();
        var fksPorPropriedade = tabela.GetForeignKeys()
            .SelectMany(fk => fk.Properties.Select(p => (Propriedade: p, Fk: fk)))
            .ToLookup(x => x.Propriedade, x => x.Fk);

        foreach (var propriedade in tabela.GetProperties().OrderBy(p => p.GetColumnName(), StringComparer.Ordinal))
        {
            string tipo;
            bool anulavel;
            try
            {
                tipo = propriedade.GetColumnType();
                anulavel = propriedade.IsNullable;
            }
            catch (Exception ex)
            {
                // Robustez de ferramenta de relatório: uma propriedade que não resolva tipo
                // (ex.: um conversor exótico) não pode derrubar a geração inteira.
                tipo = $"(erro ao ler: {ex.GetType().Name})";
                anulavel = true;
            }

            var fk = fksPorPropriedade[propriedade].FirstOrDefault();

            yield return new InfoColuna(
                Schema: schema,
                Tabela: nomeTabela,
                Coluna: propriedade.GetColumnName(),
                Tipo: tipo,
                Anulavel: anulavel,
                ChavePrimaria: pk is not null && pk.Properties.Contains(propriedade),
                ChaveEstrangeira: fk is not null,
                TabelaReferenciada: fk?.PrincipalEntityType.GetTableName(),
                SchemaReferenciado: fk?.PrincipalEntityType.GetSchema(),
                Unico: indicesUnicos.Contains(propriedade),
                TamanhoMaximo: propriedade.GetMaxLength(),
                Precisao: propriedade.GetPrecision(),
                Escala: propriedade.GetScale(),
                Unicode: propriedade.IsUnicode());
        }
    }
}

static void EscreverCatalogoCsv(string caminho, List<InfoColuna> colunas)
{
    using var escritor = new StreamWriter(caminho, append: false, System.Text.Encoding.UTF8);
    escritor.WriteLine("Schema,Tabela,Coluna,Tipo,Anulavel,ChavePrimaria,ChaveEstrangeira,TabelaReferenciada,TamanhoMaximo,Precisao,Escala,Unicode,Unico");

    foreach (var c in colunas)
    {
        var referencia = c.ChaveEstrangeira ? $"{c.SchemaReferenciado}.{c.TabelaReferenciada}" : "";
        escritor.WriteLine(string.Join(',', new[]
        {
            CsvCampo(c.Schema),
            CsvCampo(c.Tabela),
            CsvCampo(c.Coluna),
            CsvCampo(c.Tipo),
            c.Anulavel ? "sim" : "nao",
            c.ChavePrimaria ? "sim" : "nao",
            c.ChaveEstrangeira ? "sim" : "nao",
            CsvCampo(referencia),
            c.TamanhoMaximo?.ToString() ?? "",
            c.Precisao?.ToString() ?? "",
            c.Escala?.ToString() ?? "",
            c.Unicode is null ? "" : (c.Unicode.Value ? "sim" : "nao"),
            c.Unico ? "sim" : "nao"
        }));
    }
}

static string CsvCampo(string? valor)
{
    valor ??= "";
    return valor.Contains(',') || valor.Contains('"') || valor.Contains('\n')
        ? $"\"{valor.Replace("\"", "\"\"")}\""
        : valor;
}

// O mesmo conteúdo do catálogo, em JSON compacto — a forma que uma ferramenta lê sem
// precisar de um parser de CSV nem de Markdown. Formato estável: uma entrada por tabela, com a
// lista de colunas em ordem alfabética.
static void EscreverEsquemaJson(string caminho, List<InfoColuna> colunas)
{
    var tabelas = colunas
        .GroupBy(c => (c.Schema, c.Tabela))
        .OrderBy(g => g.Key.Schema, StringComparer.Ordinal)
        .ThenBy(g => g.Key.Tabela, StringComparer.Ordinal)
        .Select(g => new
        {
            schema = g.Key.Schema,
            tabela = g.Key.Tabela,
            colunas = g.OrderBy(c => c.Coluna, StringComparer.Ordinal).Select(c => new
            {
                c = c.Coluna,
                t = c.Tipo,
                pk = c.ChavePrimaria,
                fk = c.ChaveEstrangeira ? $"{c.SchemaReferenciado}.{c.TabelaReferenciada}" : null,
                nn = !c.Anulavel
            }).ToList()
        })
        .ToList();

    var json = System.Text.Json.JsonSerializer.Serialize(tabelas, new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    File.WriteAllText(caminho, json + Environment.NewLine, System.Text.Encoding.UTF8);
}

static void EscreverDicionarioMarkdown(string caminho, IModel modelo, List<InfoColuna> colunas, List<string> schemas)
{
    using var w = new StreamWriter(caminho, append: false, System.Text.Encoding.UTF8);

    w.WriteLine("# Dicionário de dados — CRM Tracbel");
    w.WriteLine();
    w.WriteLine("> Gerado automaticamente por `scripts/banco/gerar-dicionario-crm.ps1` a partir do");
    w.WriteLine("> modelo do EF Core (`Tracbel.Crm.Infraestrutura.Persistencia.CrmDbContext`).");
    w.WriteLine("> **Não edite à mão** — rode o gerador de novo depois de mudar o modelo.");
    w.WriteLine(">");
    w.WriteLine("> Padrão de banco (schema, nomenclatura, tipos, integridade):");
    w.WriteLine("> [14-PADRAO-DE-BANCO](../projeto/14-PADRAO-DE-BANCO.md). O modelo completo,");
    w.WriteLine("> tabela a tabela: [17-MODELO-UNIFICADO](../projeto/17-MODELO-UNIFICADO.md), seção 8.");
    w.WriteLine(">");
    w.WriteLine("> Banco: **SQL Server**, com nomes em PascalCase — as palavras do documento 15,");
    w.WriteLine("> letra por letra iguais em C# e no banco. Decisão em");
    w.WriteLine("> [20-DECISAO-SQL-SERVER](../projeto/20-DECISAO-SQL-SERVER.md).");
    w.WriteLine(">");
    w.WriteLine("> Quatro tabelas são PARTICIONADAS POR MÊS e por isso têm a data na chave primária:");
    w.WriteLine("> `auditoria.AlteracaoDeCampo`, `auditoria.EventoDeAcesso`,");
    w.WriteLine("> `processo.RegraExecucao` e `integracao.Recepcao`. A coluna `Versao`");
    w.WriteLine("> (`rowversion`) faz o controle de concorrência otimista, gerada pelo banco.");
    w.WriteLine();
    w.WriteLine($"Gerado em {DateTime.Now:yyyy-MM-dd HH:mm} — {schemas.Count} schema(s), " +
                $"{colunas.Select(c => (c.Schema, c.Tabela)).Distinct().Count()} tabela(s), {colunas.Count} coluna(s).");
    w.WriteLine();

    foreach (var schema in schemas)
    {
        w.WriteLine("---");
        w.WriteLine();
        w.WriteLine($"## Schema `{schema}`");
        w.WriteLine();

        var tabelasDoSchema = modelo.GetEntityTypes()
            .Where(t => (t.GetSchema() ?? "(sem schema)") == schema)
            .OrderBy(t => t.GetTableName(), StringComparer.Ordinal);

        foreach (var tabela in tabelasDoSchema)
        {
            var nomeTabela = tabela.GetTableName() ?? tabela.ClrType.Name;
            w.WriteLine($"### `{schema}.{nomeTabela}`");
            w.WriteLine();

            var checks = tabela.GetCheckConstraints().ToList();
            if (checks.Count > 0)
            {
                w.WriteLine("**Check constraints:**");
                foreach (var check in checks)
                    w.WriteLine($"- `{check.Name}`: `{check.Sql}`");
                w.WriteLine();
            }

            w.WriteLine("| Coluna | Tipo | Anulável | PK | FK | Único | Observação |");
            w.WriteLine("|---|---|---|---|---|---|---|");

            var colunasDaTabela = colunas.Where(c => c.Schema == schema && c.Tabela == nomeTabela);
            foreach (var c in colunasDaTabela)
            {
                var referencia = c.ChaveEstrangeira ? $"→ `{c.SchemaReferenciado}.{c.TabelaReferenciada}`" : "";
                w.WriteLine($"| `{c.Coluna}` | `{c.Tipo}` | {(c.Anulavel ? "sim" : "não")} | " +
                            $"{(c.ChavePrimaria ? "**PK**" : "")} | {(c.ChaveEstrangeira ? "**FK**" : "")} | " +
                            $"{(c.Unico ? "sim" : "")} | {referencia} |");
            }
            w.WriteLine();
        }
    }
}

static void EscreverErdMermaid(string caminho, IModel modelo, List<string> schemas)
{
    using var w = new StreamWriter(caminho, append: false, System.Text.Encoding.UTF8);

    w.WriteLine("# ERD — CRM Tracbel");
    w.WriteLine();
    w.WriteLine("> Gerado automaticamente por `scripts/banco/gerar-dicionario-crm.ps1` a partir do");
    w.WriteLine("> modelo do EF Core. **Não edite à mão.** Um diagrama por schema — ver");
    w.WriteLine("> [14-PADRAO-DE-BANCO, seção 2](../projeto/14-PADRAO-DE-BANCO.md) sobre a divisão");
    w.WriteLine("> por schema.");
    w.WriteLine();

    foreach (var schema in schemas)
    {
        w.WriteLine($"## Schema `{schema}`");
        w.WriteLine();
        w.WriteLine("```mermaid");
        w.WriteLine("erDiagram");

        var tabelasDoSchema = modelo.GetEntityTypes()
            .Where(t => (t.GetSchema() ?? "(sem schema)") == schema)
            .OrderBy(t => t.GetTableName(), StringComparer.Ordinal)
            .ToList();

        foreach (var tabela in tabelasDoSchema)
        {
            var nomeTabela = tabela.GetTableName() ?? tabela.ClrType.Name;
            var pk = tabela.FindPrimaryKey();
            w.WriteLine($"    {nomeTabela} {{");

            foreach (var propriedade in tabela.GetProperties().OrderBy(p => p.GetColumnName(), StringComparer.Ordinal))
            {
                var tipo = SanitizarTipoMermaid(SeguroObterTipo(propriedade));
                var papel = pk is not null && pk.Properties.Contains(propriedade) ? " PK" : "";
                w.WriteLine($"        {tipo} {propriedade.GetColumnName()}{papel}");
            }

            w.WriteLine("    }");
        }

        // Relacionamentos — só entre tabelas do MESMO diagrama (schema); FK para outro schema
        // é anotada como comentário, porque o mermaid erDiagram não desenha entidade fora do
        // bloco atual.
        foreach (var tabela in tabelasDoSchema)
        {
            var nomeTabela = tabela.GetTableName() ?? tabela.ClrType.Name;
            foreach (var fk in tabela.GetForeignKeys())
            {
                var alvo = fk.PrincipalEntityType;
                var nomeAlvo = alvo.GetTableName() ?? alvo.ClrType.Name;
                var schemaAlvo = alvo.GetSchema() ?? "(sem schema)";

                if (schemaAlvo == schema)
                {
                    w.WriteLine($"    {nomeAlvo} ||--o{{ {nomeTabela} : \"{fk.GetConstraintName()}\"");
                }
                else
                {
                    w.WriteLine($"    %% {nomeTabela} -> {schemaAlvo}.{nomeAlvo} ({fk.GetConstraintName()}) — schema diferente, ver diagrama de '{schemaAlvo}'");
                }
            }
        }

        w.WriteLine("```");
        w.WriteLine();
    }
}

static string SeguroObterTipo(IProperty propriedade)
{
    try { return propriedade.GetColumnType(); }
    catch { return propriedade.ClrType.Name; }
}

// Mermaid erDiagram é estrito com o token de tipo: sem espaço, vírgula ou parênteses.
// "decimal(18,2)" vira "decimal_18_2_", "nvarchar(200)" vira "nvarchar_200_".
static string SanitizarTipoMermaid(string tipo)
{
    var limpo = new System.Text.StringBuilder(tipo.Length);
    foreach (var ch in tipo)
        limpo.Append(char.IsLetterOrDigit(ch) ? ch : '_');
    return limpo.ToString();
}

/// <summary>Uma coluna, com tudo que os três arquivos de saída precisam.</summary>
internal sealed record InfoColuna(
    string Schema,
    string Tabela,
    string Coluna,
    string Tipo,
    bool Anulavel,
    bool ChavePrimaria,
    bool ChaveEstrangeira,
    string? TabelaReferenciada,
    string? SchemaReferenciado,
    bool Unico,
    int? TamanhoMaximo,
    int? Precisao,
    int? Escala,
    bool? Unicode);

/// <summary>Contexto de acesso mínimo, só para satisfazer o construtor do DbContext — nunca usado
/// para consultar dado nenhum (a ferramenta lê só o MODELO, nunca abre conexão).</summary>
internal sealed class ProvedorDeSistemaFalso : IProvedorContextoAcesso
{
    public ContextoAcesso Atual { get; } = new(
        usuarioId: 0,
        nomeExibicao: "sistema",
        empresaId: 0,
        empresasVisiveis: new HashSet<int>(),
        subordinadosIds: new HashSet<long>(),
        equipesIds: new HashSet<long>(),
        profundidades: new Dictionary<string, Profundidade>(),
        ehServicoDeSistema: true);
}
