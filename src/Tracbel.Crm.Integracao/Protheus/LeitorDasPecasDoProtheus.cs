using System.Globalization;
using Microsoft.Data.SqlClient;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// UMA COMBINAÇÃO DO FATURAMENTO DE PEÇAS, JÁ SOMADA NO BANCO — a view <c>X_V_BI_FATURAMENTO_PECAS</c> agrupada por filial,
/// mês e pelas colunas de que as regras do BI precisam. Sem nome, cidade nem produto: o documento vem só para casar.
/// </summary>
/// <param name="Filial">O <c>filial</c> — a filial que faturou, com dois dígitos ou o código completo.</param>
/// <param name="Mes">O mês da emissão, no dia 1.</param>
/// <param name="Documento">O <c>seq_pessoa</c> — o CPF/CNPJ do cliente (o <c>A1_CGC</c>), só dígitos.</param>
/// <param name="Setor">O <c>setor</c>.</param>
/// <param name="Origem">O <c>origem</c> — <c>FAT_PECAS</c> ou a devolução.</param>
/// <param name="Linha">O <c>nome_linha_produto</c>.</param>
/// <param name="CodigoDaFamilia">O <c>cod_familia_produto</c> — de onde o BI tira o grupo comercial.</param>
/// <param name="VendedorCodigo">O <c>id_vendedor</c>.</param>
/// <param name="VendedorNome">O <c>nome_vendedor</c>.</param>
/// <param name="Cortesia">O <c>tp_cortesia</c>.</param>
/// <param name="TipoDeOrdem">O <c>tp_ordem</c>.</param>
/// <param name="Operacao">O <c>nome_operacao</c>.</param>
/// <param name="Quantidade">A soma de <c>qtde</c>.</param>
/// <param name="ValorLiquido">A soma de <c>vlr_liquido_item</c>.</param>
/// <param name="ValorDeDesconto">A soma de <c>vlr_desconto</c>.</param>
/// <param name="ValorDeTabela">A soma de <c>vlr_tabela</c>.</param>
/// <param name="Itens">Quantos itens de nota.</param>
public sealed record PecasFaturadasNaOrigem(
    string Filial,
    DateOnly Mes,
    string? Documento,
    string? Setor,
    string? Origem,
    string? Linha,
    string? CodigoDaFamilia,
    string? VendedorCodigo,
    string? VendedorNome,
    string? Cortesia,
    string? TipoDeOrdem,
    string? Operacao,
    decimal Quantidade,
    decimal ValorLiquido,
    decimal ValorDeDesconto,
    decimal ValorDeTabela,
    int Itens);

/// <summary>UMA LINHA DA VIEW <c>X_V_BI_POSICAO_ORC_PECAS</c> — um item de orçamento, com o cabeçalho repetido.</summary>
/// <param name="Filial">O <c>filial_orc</c>.</param>
/// <param name="Numero">O <c>nro_orc</c>.</param>
/// <param name="Situacao">O <c>STATUS_ORC</c>.</param>
/// <param name="Prazo">O <c>Situacao_orc</c> (no prazo, vencido, atendido).</param>
/// <param name="Reserva">O <c>status_reserva_orc</c>.</param>
/// <param name="TipoDeAtendimento">O <c>tipo_atendimento_orc</c>.</param>
/// <param name="TipoDeOrcamento">O <c>tipo_orcamento_orc</c>.</param>
/// <param name="Documento">O <c>cpf_cnpj_orc</c>, só dígitos.</param>
/// <param name="OrcadoEm">O <c>data_orçamento_orc</c>.</param>
/// <param name="ValidoAte">O <c>data_validade_orc</c>.</param>
/// <param name="AlteradoEm">O <c>data_alteracao_orc</c>.</param>
/// <param name="VendedorCodigo">O <c>cod_vededor_orc</c>.</param>
/// <param name="VendedorNome">O <c>nome_vededor_orc</c>.</param>
/// <param name="CodigoDoItem">O <c>cod_item_orc</c>.</param>
/// <param name="ValorTotal">O <c>vlr_total_orc</c>.</param>
/// <param name="ValorDeDesconto">O <c>vlr_desc_orc</c>.</param>
public sealed record ItemDeOrcamentoNaOrigem(
    string Filial,
    string Numero,
    string? Situacao,
    string? Prazo,
    string? Reserva,
    string? TipoDeAtendimento,
    string? TipoDeOrcamento,
    string? Documento,
    DateOnly? OrcadoEm,
    DateOnly? ValidoAte,
    DateOnly? AlteradoEm,
    string? VendedorCodigo,
    string? VendedorNome,
    string? CodigoDoItem,
    decimal? ValorTotal,
    decimal? ValorDeDesconto);

/// <summary>O que a leitura das peças trouxe.</summary>
/// <param name="FaturamentoDesde">O primeiro mês do faturamento.</param>
/// <param name="OrcamentosDesde">A primeira data dos orçamentos (o aberto entra de qualquer data).</param>
/// <param name="Faturamento">As combinações do faturamento, já somadas no banco.</param>
/// <param name="Orcamentos">Os itens de orçamento.</param>
public sealed record LeituraDasPecas(
    DateOnly FaturamentoDesde,
    DateOnly OrcamentosDesde,
    IReadOnlyList<PecasFaturadasNaOrigem> Faturamento,
    IReadOnlyList<ItemDeOrcamentoNaOrigem> Orcamentos);

/// <summary>
/// A LEITURA DAS PEÇAS DO PROTHEUS — pelas mesmas views que o extrator "Faturamento Peças" do BI lê (pedido do Ricardo em
/// 02/10/2026). O faturamento é somado no próprio banco, por mês e pelas colunas de que as regras do painel precisam: item a
/// item seriam centenas de milhares de linhas atravessando a rede para virar soma aqui. O orçamento vem item a item — são
/// poucos — e é somado na carga.
///
/// <para><b>O que não vem:</b> o custo médio, a margem, os impostos e o frete (é custo, e o CRM não guarda custo), o nome e a
/// cidade do cliente, e o produto. <b>SÓ LEITURA</b>, com <c>ApplicationIntent=ReadOnly</c> e <c>NOLOCK</c>.</para>
/// </summary>
/// <param name="opcoes">A configuração do banco do Protheus.</param>
public sealed class LeitorDasPecasDoProtheus(OpcoesDoBancoDoProtheus opcoes)
{
    /// <summary>A data da view nos formatos possíveis — o BI a compara com <c>AAAAMMDD</c>, mas o tipo não se vê daqui.</summary>
    private const string Data = "COALESCE(TRY_CONVERT(date, {0}, 112), TRY_CONVERT(date, {0}, 103))";

    private static string DataDe(string coluna) => string.Format(CultureInfo.InvariantCulture, Data, coluna);

    private static readonly string[] ColunasDoAgrupamento =
    [
        "RTRIM(CONVERT(varchar(20), filial))",
        $"CONVERT(char(6), {DataDe("data_emissao_nf")}, 112)",
        "RTRIM(CONVERT(varchar(30), seq_pessoa))",
        "RTRIM(CONVERT(nvarchar(60), setor))",
        "RTRIM(CONVERT(varchar(40), origem))",
        "RTRIM(CONVERT(nvarchar(120), nome_linha_produto))",
        "RTRIM(CONVERT(varchar(20), cod_familia_produto))",
        "RTRIM(CONVERT(varchar(20), id_vendedor))",
        "RTRIM(CONVERT(nvarchar(120), nome_vendedor))",
        "RTRIM(CONVERT(varchar(10), tp_cortesia))",
        "RTRIM(CONVERT(varchar(10), tp_ordem))",
        "RTRIM(CONVERT(nvarchar(60), nome_operacao))"
    ];

    /// <summary>A consulta do faturamento, somado por mês. Pública para o teste de contêiner.</summary>
    public static readonly string ConsultaDoFaturamento = $"""
        SELECT {string.Join(",\n       ", ColunasDoAgrupamento)},
               SUM(TRY_CONVERT(decimal(19, 4), qtde)), SUM(TRY_CONVERT(decimal(19, 4), vlr_liquido_item)),
               SUM(TRY_CONVERT(decimal(19, 4), vlr_desconto)), SUM(TRY_CONVERT(decimal(19, 4), vlr_tabela)), COUNT(*)
        FROM dbo.X_V_BI_FATURAMENTO_PECAS WITH (NOLOCK)
        WHERE {DataDe("data_emissao_nf")} >= @desde
        GROUP BY {string.Join(",\n         ", ColunasDoAgrupamento)}
        """;

    /// <summary>A consulta dos orçamentos: os da janela e os ainda abertos, de qualquer data. Pública para o teste de contêiner.</summary>
    public static readonly string ConsultaDosOrcamentos = $"""
        SELECT RTRIM(CONVERT(varchar(20), filial_orc)), RTRIM(CONVERT(varchar(20), nro_orc)),
               RTRIM(CONVERT(nvarchar(60), STATUS_ORC)), RTRIM(CONVERT(nvarchar(60), Situacao_orc)), RTRIM(CONVERT(nvarchar(60), status_reserva_orc)),
               RTRIM(CONVERT(nvarchar(60), tipo_atendimento_orc)), RTRIM(CONVERT(nvarchar(60), tipo_orcamento_orc)),
               RTRIM(CONVERT(varchar(30), cpf_cnpj_orc)),
               [data_orçamento_orc], data_validade_orc, data_alteracao_orc,
               RTRIM(CONVERT(varchar(20), cod_vededor_orc)), RTRIM(CONVERT(nvarchar(120), nome_vededor_orc)),
               RTRIM(CONVERT(varchar(40), cod_item_orc)), vlr_total_orc, vlr_desc_orc
        FROM dbo.X_V_BI_POSICAO_ORC_PECAS WITH (NOLOCK)
        WHERE {DataDe("[data_orçamento_orc]")} >= @desde
           OR UPPER(LTRIM(RTRIM(CONVERT(nvarchar(60), STATUS_ORC)))) IN (N'ABERTO', N'PARCIALMENTE ATENDIDO')
        """;

    /// <summary>Lê as duas views.</summary>
    /// <param name="faturamentoDesde">O primeiro mês do faturamento.</param>
    /// <param name="orcamentosDesde">A primeira data dos orçamentos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDasPecas>> LerAsync(DateOnly faturamentoDesde, DateOnly orcamentosDesde, CancellationToken ct)
    {
        if (!opcoes.EstaConfigurada)
            return Resultado<LeituraDasPecas>.Indisponivel(
                "A leitura das peças do Protheus exige ProtheusBanco__Servidor, __Banco, __Usuario e __Senha.");

        var construtor = new SqlConnectionStringBuilder
        {
            DataSource = opcoes.Servidor,
            InitialCatalog = opcoes.Banco,
            UserID = opcoes.Usuario,
            Password = opcoes.Senha,
            TrustServerCertificate = true,
            ApplicationIntent = ApplicationIntent.ReadOnly,
            ConnectTimeout = 15
        };

        try
        {
            await using var conexao = new SqlConnection(construtor.ConnectionString);
            await conexao.OpenAsync(ct);
            return Resultado<LeituraDasPecas>.Ok(await LerAsync(conexao, faturamentoDesde, orcamentosDesde, ct));
        }
        catch (SqlException falha)
        {
            // A MENSAGEM DO CONECTOR PODE CITAR SERVIDOR E USUÁRIO: sai só o código, como nas ordens de serviço.
            var causa = falha.Number switch
            {
                208 => " A view do BI não existe neste banco — confira o banco configurado na conexão.",
                229 => " O login da conexão não tem permissão de leitura na view do BI — peça o GRANT SELECT a quem administra o Protheus.",
                _ => string.Empty
            };
            return Resultado<LeituraDasPecas>.Indisponivel(
                $"O banco do Protheus não respondeu à leitura das peças (erro SQL {falha.Number}).{causa} Nada foi gravado.");
        }
    }

    /// <summary>Lê as duas views numa conexão já aberta.</summary>
    public static async Task<LeituraDasPecas> LerAsync(SqlConnection conexao, DateOnly faturamentoDesde, DateOnly orcamentosDesde, CancellationToken ct)
    {
        var faturamento = new List<PecasFaturadasNaOrigem>(100_000);
        await using (var comando = new SqlCommand(ConsultaDoFaturamento, conexao) { CommandTimeout = 900 })
        {
            comando.Parameters.Add(new SqlParameter("@desde", System.Data.SqlDbType.Date) { Value = faturamentoDesde.ToDateTime(TimeOnly.MinValue) });
            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct))
            {
                string? Texto(int i) => leitor.IsDBNull(i) ? null : Convert.ToString(leitor.GetValue(i), CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } t ? t : null;
                decimal Numero(int i) => leitor.IsDBNull(i) ? 0m : LeitorDasOrdensDeServicoDoProtheus.NumeroDe(leitor.GetValue(i)) ?? 0m;

                var mes = Texto(1) is { Length: 6 } m
                          && DateOnly.TryParseExact(m + "01", "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lido)
                    ? lido
                    : (DateOnly?)null;
                if (mes is null) continue;

                var documento = LeitorDeClientesDoProtheus.SoDigitos(Texto(2));
                faturamento.Add(new PecasFaturadasNaOrigem(
                    Filial: Texto(0) ?? string.Empty,
                    Mes: mes.Value,
                    Documento: documento.Length is 11 or 14 ? documento : null,
                    Setor: Texto(3),
                    Origem: Texto(4),
                    Linha: Texto(5),
                    CodigoDaFamilia: Texto(6),
                    VendedorCodigo: Texto(7),
                    VendedorNome: Texto(8),
                    Cortesia: Texto(9),
                    TipoDeOrdem: Texto(10),
                    Operacao: Texto(11),
                    Quantidade: Numero(12),
                    ValorLiquido: Numero(13),
                    ValorDeDesconto: Numero(14),
                    ValorDeTabela: Numero(15),
                    Itens: leitor.IsDBNull(16) ? 0 : Convert.ToInt32(leitor.GetValue(16), CultureInfo.InvariantCulture)));
            }
        }

        var orcamentos = new List<ItemDeOrcamentoNaOrigem>(50_000);
        await using (var comando = new SqlCommand(ConsultaDosOrcamentos, conexao) { CommandTimeout = 900 })
        {
            comando.Parameters.Add(new SqlParameter("@desde", System.Data.SqlDbType.Date) { Value = orcamentosDesde.ToDateTime(TimeOnly.MinValue) });
            await using var leitor = await comando.ExecuteReaderAsync(ct);
            while (await leitor.ReadAsync(ct))
            {
                string? Texto(int i) => leitor.IsDBNull(i) ? null : Convert.ToString(leitor.GetValue(i), CultureInfo.InvariantCulture)?.Trim() is { Length: > 0 } t ? t : null;
                object? Valor(int i) => leitor.IsDBNull(i) ? null : leitor.GetValue(i);

                var documento = LeitorDeClientesDoProtheus.SoDigitos(Texto(7));
                orcamentos.Add(new ItemDeOrcamentoNaOrigem(
                    Filial: Texto(0) ?? string.Empty,
                    Numero: Texto(1) ?? string.Empty,
                    Situacao: Texto(2),
                    Prazo: Texto(3),
                    Reserva: Texto(4),
                    TipoDeAtendimento: Texto(5),
                    TipoDeOrcamento: Texto(6),
                    Documento: documento.Length is 11 or 14 ? documento : null,
                    OrcadoEm: LeitorDasOrdensDeServicoDoProtheus.DataDe(Valor(8)),
                    ValidoAte: LeitorDasOrdensDeServicoDoProtheus.DataDe(Valor(9)),
                    AlteradoEm: LeitorDasOrdensDeServicoDoProtheus.DataDe(Valor(10)),
                    VendedorCodigo: Texto(11),
                    VendedorNome: Texto(12),
                    CodigoDoItem: Texto(13),
                    ValorTotal: LeitorDasOrdensDeServicoDoProtheus.NumeroDe(Valor(14)),
                    ValorDeDesconto: LeitorDasOrdensDeServicoDoProtheus.NumeroDe(Valor(15))));
            }
        }

        return new LeituraDasPecas(faturamentoDesde, orcamentosDesde, faturamento, orcamentos);
    }
}

/// <summary>
/// AS REGRAS DO PAINEL "FATURAMENTO PEÇAS" DO BI — portadas do script dele (aba "05 - FATURAMENTO_PEÇAS"): o grupo
/// comercial pelo código da família (pneus pela linha), a devolução pela origem, e a quantidade zerada na cortesia que não
/// movimenta estoque e no complemento de preço (chamado GLPI 78148, 18/10/2022). O valor não muda: o painel soma o
/// <c>vlr_liquido_item</c> de faturamento e de devolução juntos.
/// </summary>
public static class RegrasDasPecas
{
    /// <summary>A origem do faturamento; tudo o que não é ela cai no ramo da devolução, como no script.</summary>
    public const string OrigemDoFaturamento = "FAT_PECAS";

    private static readonly Dictionary<string, string> GrupoPelaFamilia = new(StringComparer.Ordinal)
    {
        ["1104"] = "BATERIA",
        ["1106"] = "ADITIVOS",
        ["1109"] = "COOLGARD",
        ["1113"] = "FORQUIMICA",
        ["1105"] = "GRAXAS",
        ["1103"] = "LUBRIFICANTE",
        ["1115"] = "METISA",
        ["1112"] = "TEEJET",
        ["2001"] = "UNIMIL BY JD", ["2003"] = "UNIMIL BY JD", ["2004"] = "UNIMIL BY JD", ["2005"] = "UNIMIL BY JD",
        ["2006"] = "UNIMIL BY JD", ["2007"] = "UNIMIL BY JD", ["2008"] = "UNIMIL BY JD", ["2009"] = "UNIMIL BY JD",
        ["2010"] = "UNIMIL BY JD", ["2011"] = "UNIMIL BY JD", ["2012"] = "UNIMIL BY JD", ["2202"] = "UNIMIL BY JD",
        ["5000"] = "PRECISION UPGRADE"
    };

    /// <summary>Se a linha é devolução — o ramo "DEV" do script: tudo o que não é <see cref="OrigemDoFaturamento"/>.</summary>
    public static bool EhDevolucao(string? origem) => !string.Equals(origem?.Trim(), OrigemDoFaturamento, StringComparison.Ordinal);

    /// <summary>
    /// O GRUPO COMERCIAL: pneus pela linha <c>PNEUS/RODADOS</c>; baterias, lubrificantes, graxas e os outros pelo código da
    /// família; e o resto pela própria origem, sem o prefixo (<c>FAT_PECAS</c> vira <c>PECAS</c>).
    /// </summary>
    public static string Grupo(string? origem, string? linha, string? codigoDaFamilia)
    {
        var nomeDaLinha = string.IsNullOrWhiteSpace(linha) ? "S/CLASSIFICAÇÃO" : linha.Trim();
        if (nomeDaLinha == "PNEUS/RODADOS") return "PNEU";
        if (codigoDaFamilia?.Trim() is { Length: > 0 } familia && GrupoPelaFamilia.TryGetValue(familia, out var grupo)) return grupo;

        var semPrefixo = origem?.Trim() ?? string.Empty;
        foreach (var prefixo in new[] { "FAT_", "DEV_" })
            if (semPrefixo.StartsWith(prefixo, StringComparison.Ordinal)) semPrefixo = semPrefixo[prefixo.Length..];
        return semPrefixo.Trim() is { Length: > 0 } restante ? restante : "S/CLASSIFICAÇÃO";
    }

    /// <summary>Se a quantidade conta — zero na cortesia fora da garantia de fábrica (FGP) e no complemento de preço.</summary>
    public static bool QuantidadeConta(string? cortesia, string? tipoDeOrdem, string? operacao) =>
        !(string.Equals(cortesia?.Trim(), "SIM", StringComparison.OrdinalIgnoreCase) && !string.Equals(tipoDeOrdem?.Trim(), "FGP", StringComparison.OrdinalIgnoreCase))
        && !string.Equals(operacao?.Trim(), "COMPLEMENTO PRECO", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// A filial da view no código do CRM (<c>0101NN</c>): dois dígitos ganham o prefixo da empresa; o código completo fica como
    /// está; o resto é nulo — e a linha fica de fora, contada.
    /// </summary>
    public static string? CodigoDaFilial(string? filial)
    {
        var limpo = filial?.Trim() ?? string.Empty;
        if (limpo.Length is 1 or 2 && limpo.All(char.IsAsciiDigit)) return "0101" + limpo.PadLeft(2, '0');
        if (limpo.Length == 6 && limpo.All(char.IsAsciiDigit)) return limpo;
        return null;
    }
}
