using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Padrão de banco — schema e nomenclatura.
///
/// Cobre as seções 2 e 3 do documento <c>docs/projeto/14-PADRAO-DE-BANCO.md</c>. Cada teste
/// aqui existe porque uma regra sem teste é recomendação, não padrão (documento 14, seção 14).
///
/// MUDANÇA DE PROVEDOR — PostgreSQL saiu, SQL Server voltou
/// (<c>docs/projeto/20-DECISAO-SQL-SERVER.md</c>, que substitui a
/// <c>19-DECISAO-POSTGRESQL.md</c>). O que mudou nestes testes foi a FORMA do nome, não a
/// exigência: o nome volta a ser PascalCase, que é o que o documento 15 sempre documentou
/// (<c>comercial.Cliente</c>, <c>processo.Interacao</c>,
/// <c>integracao.PontoDeSincronismo</c>) — o <c>snake_case</c> era imposição do PostgreSQL,
/// que rebaixa identificador não citado para minúsculo. As palavras continuam sendo as do
/// documento 15, o veto a acento e a prefixo de módulo continua idêntico, e o regex continua
/// sendo o que torna <c>IV_</c>, <c>GE_</c> e <c>EXT_</c> estruturalmente impossíveis — agora
/// com mais força, porque PascalCase proíbe o underscore de que todo aquele prefixo depende.
/// </summary>
[Trait("Categoria", "PadraoDeBanco")]
public sealed partial class EsquemaENomenclaturaTestes
{
    /// <summary>
    /// A lista FECHADA de schemas do documento 17, seção 8, e do documento 14, seção 2.
    /// Um schema novo só entra aqui depois de uma decisão registrada — nunca por conveniência
    /// de quem está escrevendo a migration do dia.
    /// </summary>
    private static readonly HashSet<string> SchemasPermitidos =
    [
        "organizacao", // empresa, linha de negócio, carteira, hierarquia comercial, meta, praça,
                       // município e o município que cada carteira atende
        "seguranca",   // usuário, equipe, permissão, compartilhamento
        "comercial",   // cliente, contato, canal, endereço, carteira, lead, consentimento, alerta
        "processo",    // oportunidade e demais processos, fase, tarefa, interação, regra
        "auditoria",   // quem viu e quem alterou o quê
        "integracao",  // fronteira com o ERP e com o Vórtice
        "frota",       // equipamento do cliente, marca, modelo, família, horímetro
        "documento",   // arquivo anexado e seus vínculos
        "metadado",    // catálogo, campo personalizado, formulário — extensão sem release
        "relatorio"    // fontes curadas de relatório
    ];

    /// <summary>
    /// As quatro tabelas PARTICIONADAS POR DATA, com a coluna que particiona cada uma.
    ///
    /// São a única exceção à regra de chave primária de coluna única, e não por gosto: o
    /// SQL Server EXIGE que a coluna de particionamento faça parte da chave de todo índice
    /// ÚNICO da tabela particionada — a chave primária inclusive. A chave continua começando
    /// por <c>Id</c>; o que se acrescenta é a data.
    ///
    /// [V] Particionar estas quatro é a lição das 22 tabelas de log do Vórtice (43,7 milhões
    /// de linhas, 42% do banco, sem política de retenção) e das 76 tabelas de staging
    /// permanentes (19,4 milhões de linhas que nunca saem de lá).
    /// </summary>
    private static readonly Dictionary<string, string> TabelasParticionadasPorData = new()
    {
        ["AlteracaoDeCampo"] = "AlteradoEm",
        ["EventoDeAcesso"] = "OcorreuEm",
        ["RegraExecucao"] = "ExecutadoEm",
        ["Recepcao"] = "RecebidaEm"
    };

    // PascalCase: só letras ASCII e dígitos, começando por maiúscula, SEM underscore. Este
    // único regex já proíbe estruturalmente acento, "ç" e underscore — e, portanto, todo
    // prefixo de módulo no estilo Vórtice (IV_, GE_, EXT_...), porque nenhum deles sobrevive
    // à ausência de underscore.
    [GeneratedRegex("^[A-Z][A-Za-z0-9]*$")]
    private static partial Regex PadraoDeIdentificador();

    // Prefixo de módulo do Vórtice. O regex acima já o barra (o underscore não passa), mas a
    // proibição fica explícita: é o achado que este projeto existe para não repetir, e um
    // teste que diz o nome do defeito vale mais do que um que só diz "fora do padrão".
    [GeneratedRegex("^(IV|IVS|IVP|GE|GEP|EXT|IMP|OUT|DMN|X)_", RegexOptions.IgnoreCase)]
    private static partial Regex PrefixoDeModuloDoVortice();

    // Separa um nome PascalCase em palavras: "TipoDePessoa" -> Tipo, De, Pessoa.
    // É o equivalente do split por underscore que a versão snake_case usava.
    [GeneratedRegex("[A-Z]+(?![a-z])|[A-Z][a-z0-9]*|[a-z0-9]+")]
    private static partial Regex PalavrasDoIdentificador();

    // [V] achado 5.15/9.9 e a extração de 02/09/2026: 27 tabelas com "bkp" no nome só no
    // levantamento bruto (tabelas-meta.csv), incluindo GE_Pessoa_BKPJUN (dado de produção,
    // referenciado por procedure) E GE_Pessoa_BKP28052025 — dois backups da MESMA tabela,
    // sete meses depois, sem processo nem limpeza. "MIG" cobre o padrão mig_ge_pessoa_bkp.
    private static readonly string[] TokensProibidos = ["BKP", "OLD", "TESTE", "TMP", "MIG"];

    [Fact]
    public void Toda_entidade_esta_num_schema_da_lista_fechada_e_nenhuma_esta_em_dbo()
    {
        // [V] O Vórtice inteiro mora em `dbo` — 767 tabelas sem nenhum agrupamento lógico no
        // próprio banco. `dbo` nem está na lista fechada: nenhuma tabela do modelo pode cair
        // nele, e é literalmente a mesma palavra do achado.
        var comSchemaInvalido = ModeloBanco.Modelo.GetEntityTypes()
            .Select(t => new { Tipo = t.ClrType.Name, Schema = t.GetSchema() })
            .Where(x => x.Schema is null || !SchemasPermitidos.Contains(x.Schema))
            .ToList();

        comSchemaInvalido.Should().BeEmpty(
            "toda entidade precisa estar num schema da lista fechada do documento 14, seção 2 " +
            "(organizacao, seguranca, comercial, processo, auditoria, integracao, frota, " +
            "documento, metadado, relatorio) — nunca em 'dbo'. " +
            "Encontrado fora do padrão: {0}",
            string.Join(", ", comSchemaInvalido.Select(x => $"{x.Tipo} -> '{x.Schema}'")));
    }

    [Fact]
    public void Os_dez_schemas_do_modelo_unificado_existem_e_somam_sessenta_e_oito_tabelas()
    {
        // O documento 17, seção 8.12, fixou a conta em 63 tabelas em 10 schemas. Hoje são 68, e
        // cada acréscimo tem decisão registrada:
        //
        //   +2 organizacao.Municipio e organizacao.CarteiraMunicipio — documento 26, seção 5.
        //   +1 processo.VendaPerdida — o motivo da perda mora no motor de questionário do sistema
        //      de origem, não no processo; sem tabela própria a resposta continuaria ilegível.
        //   +1 comercial.FaturamentoDoCliente — o faturamento por cliente, filial e mês, base da
        //      curva ABC.
        //   +1 comercial.FaturamentoSemCliente — documento 31: o faturamento que NÃO acha cliente
        //      no CRM. R$ 798,6 milhões que antes eram descartados em silêncio; sem esta tabela a
        //      tela mostra o numerador e esconde o denominador.
        //
        // Este teste é o que impede o modelo de crescer sem decisão registrada — o "portão" da
        // seção 10.2. Ele falhou de propósito quando as três últimas entraram, e é assim que se
        // descobre que alguém acrescentou tabela sem escrever por quê.
        var porSchema = ModeloBanco.Modelo.GetEntityTypes()
            .GroupBy(t => t.GetSchema() ?? "(sem schema)")
            .ToDictionary(g => g.Key, g => g.Count());

        var esperado = new Dictionary<string, int>
        {
            ["organizacao"] = 8,
            ["seguranca"] = 8,
            ["comercial"] = 11,
            ["processo"] = 14,
            ["frota"] = 5,
            ["documento"] = 2,
            ["auditoria"] = 3,
            ["integracao"] = 6,
            ["metadado"] = 8,
            ["relatorio"] = 3
        };

        porSchema.Should().BeEquivalentTo(esperado,
            "a conta é 68 tabelas em 10 schemas — as 63 do documento 17, seção 8.12, mais as " +
            "cinco listadas no comentário acima, cada uma com decisão registrada. Mudar este " +
            "número exige a decisão da seção 10.2 (o portão de tabela nova) e a atualização do " +
            "documento 14, seção 2.1, na MESMA mudança");

        porSchema.Values.Sum().Should().Be(68);
    }

    [Fact]
    public void O_schema_padrao_do_contexto_esta_na_lista_fechada()
    {
        // CrmDbContext.OnModelCreating chama HasDefaultSchema("comercial") — a entidade que não
        // declarar schema explicitamente cai aqui, nunca em "dbo".
        var schemaPadrao = ModeloBanco.Modelo.GetDefaultSchema();

        schemaPadrao.Should().NotBeNullOrEmpty("o contexto precisa declarar HasDefaultSchema explicitamente");
        SchemasPermitidos.Should().Contain(schemaPadrao,
            "o schema padrão do CrmDbContext precisa estar na lista fechada do documento 14, seção 2");
    }

    [Fact]
    public void Toda_tabela_tem_chave_primaria()
    {
        // [V] achado 10 da extração: 5 tabelas grandes (a maior com 1.092.904 linhas) são
        // HEAP — sem chave primária nem índice clusterizado. Duas delas nem têm um índice
        // sequer. Isso nunca deveria acontecer de novo.
        var semChave = ModeloBanco.Modelo.GetEntityTypes()
            .Where(t => t.FindPrimaryKey() is null)
            .Select(t => t.ClrType.Name)
            .ToList();

        semChave.Should().BeEmpty(
            "toda tabela precisa de chave primária (documento 14, seção 3). Sem PK: {0}",
            string.Join(", ", semChave));
    }

    [Fact]
    public void A_chave_primaria_e_a_coluna_Id_salvo_nas_tabelas_particionadas_por_data()
    {
        var foraDoPadrao = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var pk = tabela.FindPrimaryKey();
            if (pk is null) continue; // coberto pelo teste anterior

            var nomeTabela = tabela.GetTableName() ?? tabela.ClrType.Name;
            var colunas = pk.Properties.Select(p => p.Name).ToArray();

            if (TabelasParticionadasPorData.TryGetValue(nomeTabela, out var colunaDeParticao))
            {
                // Numa tabela particionada, a exigência do SQL Server é que a coluna de
                // particionamento faça parte da chave de todo índice único — a chave primária
                // inclusive. Continua começando por Id.
                if (colunas is not ["Id", _] || colunas[1] != colunaDeParticao)
                    foraDoPadrao.Add(
                        $"{nomeTabela}: [{string.Join(", ", colunas)}] " +
                        $"(particionada — esperado [Id, {colunaDeParticao}])");
                continue;
            }

            if (colunas is not ["Id"])
                foraDoPadrao.Add($"{nomeTabela}: [{string.Join(", ", colunas)}]");
        }

        foraDoPadrao.Should().BeEmpty(
            "a chave primária é sempre a coluna 'Id' (documento 14, seção 3). A ÚNICA exceção " +
            "são as tabelas particionadas por data, listadas em 'TabelasParticionadasPorData' " +
            "deste arquivo, onde o SQL Server exige a coluna de particionamento na chave — e " +
            "mesmo lá a chave começa por Id. Fora do padrão: {0}",
            string.Join("; ", foraDoPadrao));
    }

    [Fact]
    public void Nome_de_tabela_segue_o_padrao_PascalCase_sem_acento_e_sem_underscore()
    {
        var foraDoPadrao = ModeloBanco.Modelo.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(nome => nome is not null)
            .Where(nome => !PadraoDeIdentificador().IsMatch(nome!) || PrefixoDeModuloDoVortice().IsMatch(nome!))
            .ToList();

        foraDoPadrao.Should().BeEmpty(
            "nome de tabela é PascalCase, só ASCII, sem acento/ç e sem underscore " +
            "(documento 14, seção 3; documento 15, seção 1) — o padrão do Vórtice " +
            "(IV_, GE_, EXT_...) é estruturalmente impossível aqui. Fora do padrão: {0}",
            string.Join(", ", foraDoPadrao));
    }

    [Fact]
    public void Nome_de_coluna_segue_o_padrao_PascalCase_sem_acento_e_sem_underscore()
    {
        var foraDoPadrao = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetProperties(), (t, p) => new { Tabela = t.GetTableName(), Coluna = p.GetColumnName() })
            .Where(x => !PadraoDeIdentificador().IsMatch(x.Coluna) || PrefixoDeModuloDoVortice().IsMatch(x.Coluna))
            .Select(x => $"{x.Tabela}.{x.Coluna}")
            .ToList();

        foraDoPadrao.Should().BeEmpty(
            "nome de coluna é PascalCase, só ASCII, sem acento/ç e sem underscore " +
            "(documento 14, seção 3). Fora do padrão: {0}",
            string.Join(", ", foraDoPadrao));
    }

    [Fact]
    public void Nome_de_coluna_nao_usa_abreviacao_proibida()
    {
        // Documento 15, seção 5: a palavra é inteira. O Vórtice escreve SeqPessoa, CodProcesso,
        // DtaRealizacao, VlrTotal, IndEstorno — e o mapa desses nomes mora na cabeça de duas
        // pessoas. Aqui a abreviação nem chega a entrar.
        //
        // A checagem é por PALAVRA do PascalCase, e não por trecho de texto: "Cor" não pode
        // ser confundido com "Cod", e "Descricao" não é "Desc".
        var abreviacoesProibidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "qtd", "cod", "desc", "seq", "dt", "dta", "num", "nro", "vlr", "ind",
            "tp", "ender", "proc", "usr", "cli", "op", "cmpl", "obs", "qtde", "nr"
        };

        var violacoes = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var nomeTabela = tabela.GetTableName();
            foreach (var coluna in tabela.GetProperties())
            {
                var nomeColuna = coluna.GetColumnName();

                violacoes.AddRange(
                    from palavra in PalavrasDoIdentificador().Matches(nomeColuna).Select(m => m.Value)
                    where abreviacoesProibidas.Contains(palavra)
                    select $"{nomeTabela}.{nomeColuna} (palavra '{palavra}')");
            }
        }

        violacoes.Should().BeEmpty(
            "nome de coluna usa a palavra inteira, em português (documento 15, seção 5): " +
            "'Quantidade' e não 'Qtd', 'Codigo' e não 'Cod', 'Data' e não 'Dta', 'Valor' e não " +
            "'Vlr'. Violações: {0}",
            string.Join("; ", violacoes));
    }

    [Fact]
    public void Nenhuma_tabela_ou_coluna_usa_nome_de_backup_teste_ou_migracao()
    {
        // [V] é o medo concreto do projeto: 27 tabelas "*bkp*" no Vórtice, GE_Pessoa_BKPJUN
        // sendo lida por procedure em produção, e mig_ge_pessoa_bkp esquecida desde 2024.
        // "Não fazer isso de novo" precisa ser um teste, não uma boa intenção.
        var violacoes = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var nomeTabela = tabela.GetTableName() ?? "";
            violacoes.AddRange(
                from token in TokensProibidos
                where nomeTabela.Contains(token, StringComparison.OrdinalIgnoreCase)
                select $"tabela {nomeTabela} (contém '{token}')");

            foreach (var coluna in tabela.GetProperties())
            {
                var nomeColuna = coluna.GetColumnName();
                violacoes.AddRange(
                    from token in TokensProibidos
                    where nomeColuna.Contains(token, StringComparison.OrdinalIgnoreCase)
                    select $"coluna {nomeTabela}.{nomeColuna} (contém '{token}')");
            }
        }

        violacoes.Should().BeEmpty(
            "nomes com 'BKP', 'OLD', 'TESTE', 'TMP' ou 'MIG' são proibidos em tabela e coluna " +
            "(documento 14, seções 3 e 13) — backup e teste não moram no banco de produção. " +
            "Violações: {0}",
            string.Join("; ", violacoes));
    }

    [Fact]
    public void Indices_e_chaves_seguem_o_prefixo_padrao()
    {
        // Os prefixos do documento 14, seção 3, em MAIÚSCULAS — que é como eles aparecem no
        // catálogo do SQL Server e no SSMS, letra por letra iguais ao que está escrito aqui.
        var violacoes = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var nomeTabela = tabela.GetTableName();

            var pk = tabela.FindPrimaryKey();
            if (pk is not null && pk.GetName() is { } nomePk && !nomePk.StartsWith("PK_", StringComparison.Ordinal))
                violacoes.Add($"chave primária {nomeTabela}: '{nomePk}' (esperado prefixo PK_)");

            foreach (var chaveAlternativa in tabela.GetKeys().Where(k => !k.IsPrimaryKey()))
            {
                var nome = chaveAlternativa.GetName();
                if (nome is not null && !nome.StartsWith("AK_", StringComparison.Ordinal))
                    violacoes.Add($"chave alternativa {nomeTabela}: '{nome}' (esperado prefixo AK_)");
            }

            foreach (var fk in tabela.GetForeignKeys())
            {
                var nomeFk = fk.GetConstraintName();
                if (nomeFk is not null && !nomeFk.StartsWith("FK_", StringComparison.Ordinal))
                    violacoes.Add($"foreign key {nomeTabela}: '{nomeFk}' (esperado prefixo FK_)");
            }

            foreach (var indice in tabela.GetIndexes())
            {
                var nomeIndice = indice.GetDatabaseName();
                var prefixoEsperado = indice.IsUnique ? "UX_" : "IX_";
                if (nomeIndice is null || !nomeIndice.StartsWith(prefixoEsperado, StringComparison.Ordinal))
                    violacoes.Add(
                        $"índice {nomeTabela}: '{nomeIndice}' (esperado prefixo {prefixoEsperado} — " +
                        $"{(indice.IsUnique ? "é único" : "não é único")})");
            }

            foreach (var check in tabela.GetCheckConstraints())
            {
                var nomeCheck = check.Name;
                if (nomeCheck is null || !nomeCheck.StartsWith("CK_", StringComparison.Ordinal))
                    violacoes.Add($"check constraint {nomeTabela}: '{nomeCheck}' (esperado prefixo CK_)");
            }
        }

        violacoes.Should().BeEmpty(
            "índice único usa UX_, índice comum usa IX_, chave primária usa PK_, chave " +
            "alternativa usa AK_, foreign key usa FK_ e check constraint usa CK_ " +
            "(documento 14, seção 3). EF Core já nomeia PK_/FK_/IX_ sozinho por convenção — " +
            "só o índice ÚNICO precisa de HasDatabaseName(\"UX_...\") explícito, porque a " +
            "convenção do EF nomeia índice único como IX_ também. Fora do padrão: {0}",
            string.Join("; ", violacoes));
    }

    [Fact]
    public void Todo_nome_de_objeto_cabe_no_limite_de_identificador_do_SqlServer()
    {
        // O SQL Server RECUSA qualquer identificador acima de 128 caracteres, e a migration
        // falha na hora de aplicar — em produção, não aqui. Este teste é o que faz falhar
        // aqui. O limite é mais folgado que o do PostgreSQL (63 bytes), mas PascalCase gasta
        // mais caractere que snake_case em nome composto de FK, então a checagem continua
        // valendo a pena.
        const int LimiteDoSqlServer = 128;

        var longos = new List<string>();
        var todosOsNomes = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var nomeTabela = tabela.GetTableName();
            if (nomeTabela is not null) todosOsNomes.Add(nomeTabela);

            if (tabela.FindPrimaryKey()?.GetName() is { } pk) todosOsNomes.Add(pk);
            todosOsNomes.AddRange(tabela.GetKeys().Select(k => k.GetName()).OfType<string>());
            todosOsNomes.AddRange(tabela.GetForeignKeys().Select(f => f.GetConstraintName()).OfType<string>());
            todosOsNomes.AddRange(tabela.GetIndexes().Select(i => i.GetDatabaseName()).OfType<string>());
            todosOsNomes.AddRange(tabela.GetCheckConstraints().Select(c => c.Name).OfType<string>());
            todosOsNomes.AddRange(tabela.GetProperties().Select(p => p.GetColumnName()));
        }

        // O limite do SQL Server é em CARACTERES (sysname é nvarchar(128)), não em bytes.
        longos.AddRange(todosOsNomes.Where(n => n.Length > LimiteDoSqlServer));

        longos.Should().BeEmpty(
            "o SQL Server recusa identificador acima de {0} caracteres, e a migration falha na " +
            "hora de aplicar. Longos: {1}",
            LimiteDoSqlServer, string.Join(", ", longos.Distinct()));

        // E, truncados ou não, os nomes precisam continuar distintos dentro do mesmo schema.
        var colisoes = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetIndexes()
                .Select(i => i.GetDatabaseName())
                .OfType<string>()
                .Select(n => new { Schema = t.GetSchema(), Nome = n[..Math.Min(n.Length, LimiteDoSqlServer)] }))
            .GroupBy(x => (x.Schema, x.Nome))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.Schema}.{g.Key.Nome} ({g.Count()}x)")
            .ToList();

        colisoes.Should().BeEmpty(
            "dois índices do mesmo schema não podem ter o mesmo nome depois do corte em " +
            "{0} caracteres. Colisões: {1}",
            LimiteDoSqlServer, string.Join(", ", colisoes));
    }
}
