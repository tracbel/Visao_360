using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tracbel.Crm.Arquitetura.Testes.Banco;

/// <summary>
/// Padrão de banco — tipos de coluna.
///
/// Cobre o documento 14, seção 4.
///
/// MUDANÇA DE PROVEDOR — PostgreSQL saiu, SQL Server voltou
/// (<c>docs/projeto/20-DECISAO-SQL-SERVER.md</c>). A regra original era
/// "<c>HasColumnType</c> é proibido, ponto", porque ela é a ÚNICA via de entrada dos tipos
/// errados do Vórtice (<c>text</c>, <c>ntext</c>, <c>image</c>, <c>money</c>, <c>float</c>
/// para dinheiro, <c>datetime</c>, <c>numeric(18,0)</c>). Ela volta a valer, com UMA exceção
/// registrada: o JSON, que no SQL Server é <c>nvarchar(max)</c> com
/// <c>CHECK (ISJSON(...) = 1)</c> e não tem API agnóstica de provedor que o produza.
///
/// Os dois reforços ganhos na passagem pelo PostgreSQL FICAM:
///   1. <c>HasColumnType</c> continua proibido, com uma lista curta de tipos justificados
///      (hoje: <c>nvarchar(max)</c>) — mesmo mecanismo de exceção revisada em PR que já
///      existia para texto sem tamanho;
///   2. os tipos proibidos continuam verificados CONTRA O MODELO, e não só contra o
///      texto-fonte: nenhuma coluna pode resolver para <c>text</c>, <c>ntext</c>,
///      <c>image</c>, <c>money</c>, <c>float</c>, <c>real</c> ou <c>datetime</c>,
///      independentemente de como tenha sido configurada. O teste do texto-fonte não
///      conseguia ver isso.
///
/// E a lista <c>TextoIlimitadoJustificado</c> deixa de ser vazia: cada uma das oito colunas
/// de JSON está nomeada aqui, uma por uma. É mais forte do que a exclusão genérica por tipo
/// que a versão PostgreSQL usava — lá, bastava ser <c>jsonb</c>; aqui, é preciso estar na
/// lista.
/// </summary>
[Trait("Categoria", "PadraoDeBanco")]
public sealed class TiposDeColunaTestes
{
    /// <summary>
    /// Os únicos tipos que podem aparecer num <c>HasColumnType("...")</c> escrito à mão, com o
    /// motivo. Acrescentar uma entrada aqui é decisão consciente, revisada em PR.
    /// </summary>
    private static readonly Dictionary<string, string> TiposLiteraisJustificados = new()
    {
        // JSON. No SQL Server é nvarchar(max) com CHECK (ISJSON(coluna) = 1) — o banco recusa
        // documento malformado, e OPENJSON/JSON_VALUE consultam por dentro. Não existe API
        // agnóstica de provedor que produza isso.
        // [V] A alternativa seria o campo de texto do Vórtice, onde o dado não mapeado some.
        ["nvarchar(max)"] = "JSON validado por CHECK (ISJSON) — sem equivalente na API agnóstica de provedor"
    };

    /// <summary>
    /// Colunas de texto sem tamanho máximo que já têm justificativa registrada. Adicionar uma
    /// entrada aqui é uma decisão consciente, revisada em PR — não um esquecimento de
    /// <c>HasMaxLength</c>.
    ///
    /// ERAM OITO até a fase 1 (documento 41); sobrou UMA. As outras sete estavam em tabelas que
    /// nunca receberam uma linha e saíram: <c>CampoPersonalizado.Validacao</c>,
    /// <c>Recepcao.Conteudo</c>, <c>MensagemDeSaida.Conteudo</c>, <c>Relatorio.Definicao</c>,
    /// <c>Lead.PayloadOriginal</c>, <c>Regra.EfeitoParametros</c> e
    /// <c>Resposta.ValorEstruturado</c> — esta última era a única que podia vir a exigir consulta
    /// por dentro do JSON, observação registrada no documento 20, seção 5, e que volta com ela.
    /// </summary>
    private static readonly HashSet<(string Tabela, string Coluna)> TextoIlimitadoJustificado =
    [
        ("MensagemDescartada", "Conteudo")         // a mensagem que a fila desistiu de entregar
    ];

    /// <summary>
    /// Tipos que NÃO podem aparecer no modelo, resolvidos pelo provedor. É a lista do
    /// documento 14, seção 4.
    /// </summary>
    private static readonly Dictionary<string, string> TiposProibidos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["text"] =
            "text é LOB legado, descontinuado pela própria Microsoft, e não aceita a maioria dos " +
            "operadores de texto. Use nvarchar com HasMaxLength",
        ["ntext"] = "idem",
        ["image"] = "idem — binário no banco não é o padrão deste projeto: documento vai para " +
                    "armazenamento de objetos e o banco guarda o caminho",
        ["money"] =
            "money tem escala fixa de 4 casas e arredonda em silêncio nas operações intermediárias. " +
            "Dinheiro é decimal(18,2)",
        ["smallmoney"] = "idem, e ainda estoura em valor de máquina agrícola",
        ["float"] =
            "ponto flutuante não representa dinheiro nem quantidade exata. [V] o Vórtice arredonda " +
            "em silêncio por causa disso",
        ["real"] = "idem",
        ["datetime"] =
            "[V] datetime tem precisão irregular de ~3,33 ms e faixa curta — é o tipo que o Vórtice " +
            "usa. Só datetime2(3)",
        ["smalldatetime"] =
            "idem, e ainda arredonda para o minuto",
        ["sql_variant"] =
            "coluna sem tipo é o oposto de domínio fechado — é o EAV que este modelo recusa"
    };

    [Fact]
    public void HasColumnType_literal_so_aparece_com_tipo_da_lista_justificada()
    {
        // [V] os tipos errados do Vórtice não foram um mau dia: são um padrão sistemático —
        // varchar em nome de gente (corrompe acento), decimal(12) para telefone (estoura com
        // DDI), numeric(18,0) como resposta padrão para "não sei que precisão usar". A defesa
        // é fechar a via que os introduziria e obrigar quem precisar de um tipo literal a
        // registrar POR ESCRITO qual e por quê.
        //
        // Verificado no TEXTO-FONTE das configurações, não no modelo: o modelo não expõe de
        // forma pública e estável se um tipo foi configurado à mão ou computado por convenção.
        var pastaConfiguracoes = Path.Combine(
            LocalizarRaizDoRepositorio(), "src", "Tracbel.Crm.Infraestrutura", "Persistencia", "Configuracoes");

        Directory.Exists(pastaConfiguracoes).Should().BeTrue(
            $"a pasta de configurações do EF Core precisa existir em '{pastaConfiguracoes}'");

        var naoJustificados = new List<string>();

        foreach (var arquivo in Directory.EnumerateFiles(pastaConfiguracoes, "*.cs", SearchOption.AllDirectories))
        {
            // Comentário (inclusive XML doc) explicando a regra NÃO conta como violação — só o
            // código conta. Sem isso, o próprio comentário desta convenção se autodenunciaria.
            var codigo = string.Join('\n', File.ReadAllLines(arquivo)
                .Where(l => !l.TrimStart().StartsWith("//", StringComparison.Ordinal)
                         && !l.TrimStart().StartsWith("///", StringComparison.Ordinal)
                         && !l.TrimStart().StartsWith("*", StringComparison.Ordinal)));

            foreach (System.Text.RegularExpressions.Match uso in
                     System.Text.RegularExpressions.Regex.Matches(codigo, @"HasColumnType\(\s*""([^""]*)""\s*\)"))
            {
                var tipo = uso.Groups[1].Value;
                if (!TiposLiteraisJustificados.ContainsKey(tipo))
                    naoJustificados.Add($"{Path.GetFileName(arquivo)}: HasColumnType(\"{tipo}\")");
            }

            // Qualquer outra forma de chamar HasColumnType (com variável, com interpolação)
            // escapa do regex acima — e por isso também é proibida.
            var usosTotais = System.Text.RegularExpressions.Regex.Matches(codigo, @"HasColumnType\(").Count;
            var usosLiterais = System.Text.RegularExpressions.Regex
                .Matches(codigo, @"HasColumnType\(\s*""[^""]*""\s*\)").Count;
            if (usosTotais > usosLiterais)
                naoJustificados.Add(
                    $"{Path.GetFileName(arquivo)}: HasColumnType com argumento que não é literal de texto");
        }

        naoJustificados.Should().BeEmpty(
            "HasColumnType(\"...\") só é aceito com um tipo da lista 'TiposLiteraisJustificados' " +
            "deste arquivo (hoje: {0}) — documento 14, seção 4. Para todo o resto use " +
            "HasMaxLength, IsUnicode e HasPrecision, que são agnósticas de provedor e não sabem " +
            "produzir text/ntext/image/money/float/datetime. Fora da lista: {1}",
            string.Join(", ", TiposLiteraisJustificados.Keys),
            string.Join("; ", naoJustificados));
    }

    [Fact]
    public void Nenhuma_coluna_do_modelo_resolve_para_um_tipo_proibido()
    {
        // Este teste é a versão FORTE da regra: não olha como a coluna foi configurada, olha o
        // tipo que o provedor realmente vai criar no banco. É o que pega o tipo errado que
        // entrou por convenção, e não por HasColumnType.
        var violacoes = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            foreach (var propriedade in tabela.GetProperties())
            {
                string tipo;
                try { tipo = propriedade.GetColumnType(); }
                catch { continue; }

                var tipoBase = tipo.Split('(')[0].Trim();
                var chave = TiposProibidos.Keys.FirstOrDefault(
                    k => string.Equals(k, tipo, StringComparison.OrdinalIgnoreCase)
                      || string.Equals(k, tipoBase, StringComparison.OrdinalIgnoreCase));

                if (chave is not null)
                    violacoes.Add(
                        $"{tabela.GetTableName()}.{propriedade.GetColumnName()} -> '{tipo}': {TiposProibidos[chave]}");
            }
        }

        violacoes.Should().BeEmpty(
            "nenhuma coluna pode resolver para um tipo proibido (documento 14, seção 4). " +
            "Violações: {0}",
            string.Join("; ", violacoes));
    }

    [Fact]
    public void Toda_coluna_de_data_e_hora_e_datetime2()
    {
        // A regra do documento 14, seção 4, verificada contra o TIPO RESOLVIDO — não contra a
        // configuração. É o que pega o `datetime` que tivesse entrado por convenção.
        //
        // O SQL Server não tem tipo com fuso embutido (datetimeoffset carrega o deslocamento,
        // não o fuso, e não é o que o padrão pede): a garantia de que todo instante é UTC vem
        // do tipo de valor DataHoraUtc no domínio, e a garantia de que a coluna tem faixa e
        // precisão corretas vem daqui. Ver documento 20, seção 4.
        var foraDoPadrao = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetProperties(), (t, p) => new { Tabela = t.GetTableName(), Propriedade = p })
            .Where(x => TipoDeRelogio(x.Propriedade) is not null)
            .Where(x => !SeguroTipo(x.Propriedade).StartsWith("datetime2", StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.Tabela}.{x.Propriedade.GetColumnName()} ({SeguroTipo(x.Propriedade)})")
            .ToList();

        foraDoPadrao.Should().BeEmpty(
            "toda coluna de data e hora é datetime2 — nunca datetime nem smalldatetime " +
            "(documento 14, seção 4). [V] o Vórtice usa datetime, com precisão irregular de " +
            "~3,33 ms. Fora do padrão: {0}",
            string.Join(", ", foraDoPadrao));
    }

    [Fact]
    public void Toda_data_hora_declara_precisao_de_milissegundos()
    {
        // Continua valendo, e agora enxerga através do conversor: DataHoraUtc é um tipo de
        // valor sobre DateTime, e o teste antigo (que olhava só ClrType) passaria por cima
        // dele sem ver.
        var semPrecisao = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetProperties(), (t, p) => new { Tabela = t.GetTableName(), Propriedade = p })
            .Where(x => TipoDeRelogio(x.Propriedade) == typeof(DateTime))
            .Where(x => x.Propriedade.GetPrecision() != 3)
            .Select(x => $"{x.Tabela}.{x.Propriedade.GetColumnName()} " +
                         $"(precisão: {x.Propriedade.GetPrecision()?.ToString() ?? "nenhuma"})")
            .ToList();

        semPrecisao.Should().BeEmpty(
            "toda coluna de data/hora declara HasPrecision(3) — datetime2(3) " +
            "(documento 14, seção 4). Fora do padrão: {0}",
            string.Join(", ", semPrecisao));
    }

    [Fact]
    public void Texto_sem_tamanho_maximo_so_e_permitido_na_lista_de_justificativas()
    {
        // Sem HasMaxLength o SQL Server resolve string para nvarchar(max): "campo que aceita
        // qualquer coisa". Cada exceção é NOMEADA em TextoIlimitadoJustificado, uma por uma —
        // não há exclusão genérica por tipo.
        var semTamanho = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetProperties(), (t, p) => (Tabela: t, Propriedade: p))
            .Where(x => x.Propriedade.ClrType == typeof(string))
            .Where(x => x.Propriedade.GetMaxLength() is null)
            .Select(x => (Tabela: x.Tabela.GetTableName()!, Coluna: x.Propriedade.GetColumnName()))
            .Where(x => !TextoIlimitadoJustificado.Contains(x))
            .ToList();

        semTamanho.Should().BeEmpty(
            "toda coluna de texto declara HasMaxLength (texto sem tamanho vira 'campo que " +
            "aceita qualquer coisa', documento 14, seção 4). Se o conteúdo é JSON, a coluna " +
            "entra em 'TextoIlimitadoJustificado' deste arquivo e ganha CHECK (ISJSON(...) = 1). " +
            "Sem tamanho e sem justificativa: {0}",
            string.Join(", ", semTamanho.Select(x => $"{x.Tabela}.{x.Coluna}")));
    }

    [Fact]
    public void Toda_coluna_de_json_tem_check_de_json_valido()
    {
        // A contrapartida da lista acima: se a coluna pode guardar qualquer texto, o banco
        // precisa pelo menos garantir que aquilo É JSON. No PostgreSQL o tipo jsonb já
        // recusava a entrada malformada; no SQL Server quem recusa é o CHECK.
        var semCheck = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var nomeTabela = tabela.GetTableName()!;
            var checks = tabela.GetCheckConstraints().Select(c => c.Sql).ToList();

            foreach (var coluna in tabela.GetProperties().Select(p => p.GetColumnName()))
            {
                if (!TextoIlimitadoJustificado.Contains((nomeTabela, coluna))) continue;

                var esperado = $"ISJSON([{coluna}]) = 1";
                if (!checks.Any(sql => sql.Contains(esperado, StringComparison.OrdinalIgnoreCase)))
                    semCheck.Add($"{nomeTabela}.{coluna}");
            }
        }

        semCheck.Should().BeEmpty(
            "toda coluna de JSON precisa de CHECK (ISJSON([Coluna]) = 1) — é o que substitui a " +
            "validação que o tipo jsonb do PostgreSQL fazia sozinho (documento 20, seção 5). " +
            "Sem CHECK: {0}",
            string.Join(", ", semCheck));
    }

    [Fact]
    public void Todo_decimal_declara_precisao_e_escala_explicitas()
    {
        // [V] o Vórtice usa numeric(18,0) como resposta padrão para "não sei que precisão
        // usar" — o que arredonda dinheiro em silêncio. Aqui HasPrecision(p, s) é obrigatório;
        // não existe precisão "padrão" aceitável. Enxerga através do conversor: Dinheiro é um
        // tipo de valor sobre decimal.
        var semPrecisao = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetProperties(), (t, p) => new
            {
                Local = $"{t.GetTableName()}.{p.GetColumnName()}",
                Propriedade = p
            })
            .Where(x => TipoNoBanco(x.Propriedade) == typeof(decimal))
            .Where(x => x.Propriedade.GetPrecision() is null || x.Propriedade.GetScale() is null)
            .Select(x => x.Local)
            .ToList();

        semPrecisao.Should().BeEmpty(
            "todo decimal declara HasPrecision(precisao, escala) explicitamente — dinheiro é " +
            "decimal(18,2); quantidade e outras grandezas têm a escala que o negócio pedir, " +
            "mas SEMPRE explícita (documento 14, seção 4). Sem precisão: {0}",
            string.Join(", ", semPrecisao));
    }

    [Fact]
    public void Toda_coluna_de_dinheiro_e_decimal_18_2()
    {
        // O tipo de valor Dinheiro existe justamente para que ninguém escolha a escala errada
        // caso a caso. Aqui o banco concorda com ele.
        var foraDoPadrao = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetProperties(), (t, p) => new { Tabela = t.GetTableName(), Propriedade = p })
            .Where(x => (x.Propriedade.GetProviderClrType() ?? x.Propriedade.ClrType) is var _
                     && EhTipoDeValorDinheiro(x.Propriedade))
            .Where(x => x.Propriedade.GetPrecision() != 18 || x.Propriedade.GetScale() != 2)
            .Select(x => $"{x.Tabela}.{x.Propriedade.GetColumnName()} " +
                         $"({x.Propriedade.GetPrecision()},{x.Propriedade.GetScale()})")
            .ToList();

        foraDoPadrao.Should().BeEmpty(
            "toda coluna gravada a partir do tipo de valor Dinheiro é decimal(18,2) " +
            "(documento 14, seção 4). Fora do padrão: {0}",
            string.Join(", ", foraDoPadrao));
    }

    [Fact]
    public void Nenhuma_propriedade_booleana_e_anulavel()
    {
        // [V] IV_Agenda.Realizada é char(1) e aceita NULL — um "sim/não" que na prática tem
        // TRÊS estados, e o terceiro (NULL) nunca foi uma decisão de negócio.
        //
        // NÃO HÁ MAIS EXCEÇÃO. A única era metadado.Resposta.ValorBooleano, onde o nulo não era um
        // terceiro estado do sim/não e sim "esta resposta não é do tipo booleano"; a tabela nunca
        // recebeu uma linha e saiu na fase 1 (documento 41). A lista fica, vazia, porque é aqui que
        // a próxima exceção precisará ser escrita — com o motivo, e não em silêncio.
        var justificadas = new HashSet<(string, string)>();

        var boolAnulavel = ModeloBanco.Modelo.GetEntityTypes()
            .SelectMany(t => t.GetProperties(), (t, p) => (Tabela: t.GetTableName()!, Coluna: p.GetColumnName(), p.ClrType))
            .Where(x => x.ClrType == typeof(bool?))
            .Where(x => !justificadas.Contains((x.Tabela, x.Coluna)))
            .Select(x => $"{x.Tabela}.{x.Coluna}")
            .ToList();

        boolAnulavel.Should().BeEmpty(
            "booleano não é anulável — 'talvez' não é um valor de negócio (documento 14, " +
            "seção 4). Anulável em: {0}",
            string.Join(", ", boolAnulavel));
    }

    [Fact]
    public void Coluna_de_caractere_fixo_unico_tem_check_constraint()
    {
        // Guarda o dia em que alguém precisar de um código de 1 caractere. Hoje a única é
        // comercial.ClienteCarteira.Classe ('A'..'D'), que TEM o CHECK.
        var semCheck = new List<string>();

        foreach (var tabela in ModeloBanco.Modelo.GetEntityTypes())
        {
            var checks = tabela.GetCheckConstraints().Select(c => c.Sql).ToList();

            foreach (var propriedade in tabela.GetProperties())
            {
                if (propriedade.ClrType != typeof(string) && !EhEnumConvertidoParaTexto(propriedade)) continue;
                if (propriedade.GetMaxLength() != 1 || propriedade.IsFixedLength() != true) continue;

                var coluna = propriedade.GetColumnName();
                if (!checks.Any(sql => sql.Contains(coluna, StringComparison.OrdinalIgnoreCase)))
                    semCheck.Add($"{tabela.GetTableName()}.{coluna}");
            }
        }

        semCheck.Should().BeEmpty(
            "coluna de um caractere sem CHECK constraint é exatamente como TipoPessoa in " +
            "('F','J') deveria NUNCA ser mapeado (documento 14, seções 4 e 14) — sem o CHECK, " +
            "ela aceita qualquer caractere. Sem CHECK: {0}",
            string.Join(", ", semCheck));
    }

    // -----------------------------------------------------------------------------------------
    // Auxiliares: enxergam ATRAVÉS do conversor de valor.
    //
    // Sem isto, uma coluna mapeada a partir de DataHoraUtc ou de Dinheiro escaparia dos testes
    // de precisão e de fuso, porque o ClrType da propriedade é o tipo de valor, não DateTime
    // nem decimal.
    // -----------------------------------------------------------------------------------------

    private static Type? TipoNoBanco(Microsoft.EntityFrameworkCore.Metadata.IProperty propriedade)
    {
        var tipo = propriedade.GetProviderClrType()
                   ?? propriedade.GetValueConverter()?.ProviderClrType
                   ?? propriedade.ClrType;

        return Nullable.GetUnderlyingType(tipo) ?? tipo;
    }

    private static Type? TipoDeRelogio(Microsoft.EntityFrameworkCore.Metadata.IProperty propriedade)
    {
        var tipo = TipoNoBanco(propriedade);
        return tipo == typeof(DateTime) || tipo == typeof(DateTimeOffset) ? tipo : null;
    }

    private static bool EhTipoDeValorDinheiro(Microsoft.EntityFrameworkCore.Metadata.IProperty propriedade)
    {
        var clr = Nullable.GetUnderlyingType(propriedade.ClrType) ?? propriedade.ClrType;
        return clr == typeof(Dominio.Comum.Dinheiro);
    }

    private static bool EhEnumConvertidoParaTexto(Microsoft.EntityFrameworkCore.Metadata.IProperty propriedade)
    {
        var clr = Nullable.GetUnderlyingType(propriedade.ClrType) ?? propriedade.ClrType;
        return clr.IsEnum && TipoNoBanco(propriedade) == typeof(string);
    }

    private static string SeguroTipo(Microsoft.EntityFrameworkCore.Metadata.IProperty propriedade)
    {
        try { return propriedade.GetColumnType(); }
        catch { return propriedade.ClrType.Name; }
    }

    /// <summary>Sobe pelas pastas até achar o arquivo de solução — mesma lógica de
    /// <c>ArquiteturaTestes.LocalizarRaizDoRepositorio</c>.</summary>
    private static string LocalizarRaizDoRepositorio()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);

        while (atual is not null && !atual.EnumerateFiles("*.sln").Any())
            atual = atual.Parent;

        return atual?.FullName
            ?? throw new InvalidOperationException(
                "Não encontrei a raiz do repositório (nenhum .sln nos diretórios acima).");
    }
}
