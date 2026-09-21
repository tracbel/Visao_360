using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Tracbel.Crm.Integracao.BancoCentral;

/// <summary>
/// UMA LINHA DO SICOR — o crédito rural de investimento de um município, num mês, agregado pelo Banco
/// Central por produto, programa, subprograma, fonte de recurso, seguro, atividade e modalidade.
///
/// <para><b>Não é um contrato.</b> O recurso <c>InvestMunicipioProduto</c> não tem número de contrato
/// nem quantidade: cada linha é a soma dos contratos daquela combinação. A planilha do comercial conta
/// linhas como "contratos", e o texto-base diz o mesmo ("quantidade de linhas de contratos"); o CRM
/// guarda a linha e chama pelo nome certo.</para>
///
/// <para><b>Os códigos são do Banco Central</b>, não do IBGE: o estado de São Paulo é 27 (no IBGE,
/// 35), e o município tem código próprio. O de-para para o IBGE é feito pelo nome, na carga.</para>
/// </summary>
public sealed record LinhaDoSicor(
    string Municipio,
    int CodigoMunicipioBcb,
    short Ano,
    byte Mes,
    int CodigoProduto,
    string Produto,
    int CodigoPrograma,
    int CodigoSubprograma,
    int CodigoFonte,
    int CodigoSeguro,
    byte Atividade,
    int CodigoModalidade,
    decimal Valor,
    decimal Area);

/// <summary>Um item de uma tabela auxiliar do SICOR.</summary>
/// <param name="Codigo">O código, sem os zeros à esquerda.</param>
/// <param name="CodigoPai">O código do programa, para o subprograma; zero para os demais.</param>
/// <param name="Descricao">A descrição publicada.</param>
/// <param name="VigenteDe">Início da vigência, quando publicado.</param>
/// <param name="VigenteAte">Fim da vigência; nulo é vigente.</param>
public sealed record ItemDoCatalogoDoSicor(int Codigo, int CodigoPai, string Descricao, DateOnly? VigenteDe, DateOnly? VigenteAte);

/// <summary>As tabelas auxiliares do SICOR que o CRM usa.</summary>
public enum CatalogoDoSicor
{
    /// <summary>Programa de crédito (Pronaf, Pronamp, Moderfrota…).</summary>
    Programa,

    /// <summary>Subprograma, dentro do programa.</summary>
    Subprograma,

    /// <summary>Fonte de recurso (BNDES/Finame, LCA, poupança rural…).</summary>
    Fonte,

    /// <summary>Produto financiado (trator, colheitadeira, bovinos…).</summary>
    Produto
}

/// <summary>
/// A LEITURA DO CRÉDITO RURAL DO SICOR — dados abertos do Banco Central (issue 68).
///
/// <para><b>O que o texto-base pede:</b> "a nível município vai trazer todos os financiamentos
/// contratados, o nome do produto, quais são os créditos contratados naquele município"; "pegar os
/// últimos 12 meses e dividir pelos meses anteriores"; "valor do contrato, ticket médio e share em 25 e
/// 26". O índice e as faixas são da issue 73; aqui entra o dado.</para>
///
/// <para><b>Medido em 21/09/2026:</b> o serviço OData responde a consulta inteira de uma vez, sem
/// paginação forçada — 204.435 linhas de SP desde 2013 numa resposta só. O leitor pede <b>um ano por
/// vez</b>, para cada resposta ter poucos MB e uma falha custar só aquele ano. O <c>$apply=groupby</c>
/// é ignorado pelo servidor (devolve as linhas sem agrupar), e por isso não é usado.</para>
///
/// <para><b>Só leitura, só GET, só dado público.</b> Nenhuma credencial é usada nem existe.</para>
/// </summary>
/// <param name="http">O cliente HTTP.</param>
public sealed class LeitorDoSicor(HttpClient http)
{
    /// <summary>O recurso de investimento por município e produto.</summary>
    public const string EnderecoDoInvestimento =
        "https://olinda.bcb.gov.br/olinda/servico/SICOR/versao/v2/odata/InvestMunicipioProduto";

    /// <summary>A pasta das tabelas auxiliares do SICOR.</summary>
    public const string EnderecoDasTabelas = "https://www.bcb.gov.br/htms/sicor/";

    /// <summary>O código do estado de São Paulo no Banco Central (no IBGE é 35).</summary>
    public const int SaoPauloNoBancoCentral = 27;

    /// <summary>O primeiro ano da série aberta do SICOR.</summary>
    public const short PrimeiroAno = 2013;

    /// <summary>O endereço da consulta de um ano de uma UF.</summary>
    /// <param name="estadoBcb">O código do estado no Banco Central.</param>
    /// <param name="ano">O ano de emissão.</param>
    public static string EnderecoDoAno(int estadoBcb, short ano) =>
        $"{EnderecoDoInvestimento}?$format=json" +
        $"&$filter=cdEstado%20eq%20'{estadoBcb}'%20and%20AnoEmissao%20eq%20'{ano}'";

    /// <summary>As linhas de investimento de uma UF num ano.</summary>
    /// <param name="estadoBcb">O código do estado no Banco Central.</param>
    /// <param name="ano">O ano de emissão.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <exception cref="InvalidDataException">Quando a resposta não tem a forma esperada.</exception>
    public async Task<IReadOnlyList<LinhaDoSicor>> LerInvestimentosAsync(int estadoBcb, short ano, CancellationToken ct) =>
        InterpretarInvestimentos(await http.GetStringAsync(EnderecoDoAno(estadoBcb, ano), ct));

    /// <summary>Interpreta a resposta do OData. Público para os testes.</summary>
    /// <param name="json">O corpo da resposta.</param>
    /// <exception cref="InvalidDataException">Quando a resposta não traz a lista <c>value</c> ou um campo esperado.</exception>
    public static IReadOnlyList<LinhaDoSicor> InterpretarInvestimentos(string json)
    {
        using var documento = JsonDocument.Parse(json);

        if (!documento.RootElement.TryGetProperty("value", out var valor) || valor.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("O SICOR não respondeu a lista \"value\" do OData.");

        var linhas = new List<LinhaDoSicor>(valor.GetArrayLength());

        foreach (var item in valor.EnumerateArray())
        {
            linhas.Add(new LinhaDoSicor(
                Texto(item, "Municipio"),
                Codigo(item, "cdMunicipio"),
                (short)Codigo(item, "AnoEmissao"),
                (byte)Codigo(item, "MesEmissao"),
                Codigo(item, "cdProduto"),
                SemAspas(Texto(item, "nomeProduto")),
                Codigo(item, "cdPrograma"),
                Codigo(item, "cdSubPrograma"),
                Codigo(item, "cdFonteRecurso"),
                Codigo(item, "cdTipoSeguro"),
                (byte)Codigo(item, "Atividade"),
                Codigo(item, "cdModalidade"),
                Numero(item, "VlInvest"),
                Numero(item, "AreaInvest")));
        }

        return linhas;
    }

    /// <summary>Lê uma tabela auxiliar do SICOR.</summary>
    /// <param name="catalogo">A tabela.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<ItemDoCatalogoDoSicor>> LerCatalogoAsync(CatalogoDoSicor catalogo, CancellationToken ct)
    {
        var bytes = await http.GetByteArrayAsync(EnderecoDasTabelas + Arquivo(catalogo), ct);

        // LATIN-1, SEM BOM: medido em 21/09/2026. Lido como UTF-8, "MÁQUINAS" vira lixo.
        return InterpretarCatalogo(Encoding.Latin1.GetString(bytes), catalogo);
    }

    /// <summary>
    /// Interpreta uma tabela auxiliar. Público para os testes.
    ///
    /// <para>As quatro tabelas têm o código na primeira coluna e a descrição na segunda; o subprograma
    /// traz o programa na quarta; programa, fonte e produto trazem início e fim de vigência na terceira
    /// e na quarta. Umas vêm entre aspas, outras não.</para>
    /// </summary>
    /// <param name="conteudo">O texto do arquivo.</param>
    /// <param name="catalogo">A tabela.</param>
    /// <exception cref="InvalidDataException">Quando a tabela não traz nenhum item.</exception>
    public static IReadOnlyList<ItemDoCatalogoDoSicor> InterpretarCatalogo(string conteudo, CatalogoDoSicor catalogo)
    {
        var itens = new List<ItemDoCatalogoDoSicor>();

        foreach (var bruta in conteudo.Split('\n'))
        {
            var linha = bruta.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(linha) || linha.StartsWith('#')) continue;

            var campos = linha.Split(';').Select(SemAspas).ToArray();
            if (campos.Length < 2 || !int.TryParse(campos[0], NumberStyles.None, CultureInfo.InvariantCulture, out var codigo))
                continue;

            var descricao = campos[1].Trim();
            if (descricao.Length == 0) continue;

            if (catalogo == CatalogoDoSicor.Subprograma)
            {
                var pai = campos.Length > 3 && int.TryParse(campos[3], NumberStyles.None, CultureInfo.InvariantCulture, out var p) ? p : 0;
                itens.Add(new ItemDoCatalogoDoSicor(codigo, pai, descricao, null, null));
            }
            else
            {
                itens.Add(new ItemDoCatalogoDoSicor(codigo, 0, descricao, Data(campos, 2), Data(campos, 3)));
            }
        }

        if (itens.Count == 0)
            throw new InvalidDataException($"A tabela {Arquivo(catalogo)} do SICOR não trouxe nenhum item. A forma mudou; nada foi gravado.");

        return itens;
    }

    /// <summary>O arquivo de cada tabela auxiliar.</summary>
    /// <param name="catalogo">A tabela.</param>
    public static string Arquivo(CatalogoDoSicor catalogo) => catalogo switch
    {
        CatalogoDoSicor.Programa => "Programa.csv",
        CatalogoDoSicor.Subprograma => "Subprogramas.csv",
        CatalogoDoSicor.Fonte => "FonteRecursos.csv",
        CatalogoDoSicor.Produto => "Produto.csv",
        _ => throw new ArgumentOutOfRangeException(nameof(catalogo))
    };

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private static string Texto(JsonElement item, string campo) =>
        item.TryGetProperty(campo, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()!.Trim()
            : throw new InvalidDataException($"Linha do SICOR sem o campo de texto \"{campo}\": {item}");

    /// <summary>
    /// Um código do SICOR — vem como texto com zeros à esquerda ("0001", "0450"), e o seguro pode ser
    /// "-1" (sem seguro informado). Guardado como número: "0001" e "1" são o mesmo programa, e as
    /// tabelas auxiliares escrevem de um jeito e o serviço de outro.
    /// </summary>
    private static int Codigo(JsonElement item, string campo)
    {
        var texto = Texto(item, campo);
        return int.TryParse(texto, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var codigo)
            ? codigo
            : throw new InvalidDataException($"Código do SICOR fora do formato em \"{campo}\": \"{texto}\".");
    }

    private static decimal Numero(JsonElement item, string campo) =>
        item.TryGetProperty(campo, out var v) && v.ValueKind == JsonValueKind.Number
            ? decimal.Round(v.GetDecimal(), 2)
            : throw new InvalidDataException($"Linha do SICOR sem o número \"{campo}\": {item}");

    /// <summary>O SICOR escreve o nome do produto com aspas literais dentro do texto: <c>"\"TRATOR\""</c>.</summary>
    private static string SemAspas(string texto) => texto.Trim().Trim('"').Trim();

    private static DateOnly? Data(string[] campos, int indice) =>
        campos.Length > indice && DateOnly.TryParseExact(campos[indice].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
            ? data
            : null;
}
