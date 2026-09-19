using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Integracao.Ibge;

/// <summary>Uma linha da Produção Agrícola Municipal, como o SIDRA a devolve.</summary>
/// <param name="CodigoDoMunicipio">O código IBGE do município.</param>
/// <param name="Ano">O ano da pesquisa.</param>
/// <param name="ProdutoCodigo">O código do produto na classificação 782.</param>
/// <param name="ProdutoNome">O rótulo oficial do produto.</param>
/// <param name="ValorBruto">O campo <c>V</c>, com os símbolos do IBGE.</param>
public sealed record LinhaDaAreaPlantada(int CodigoDoMunicipio, short Ano, int ProdutoCodigo, string ProdutoNome, string ValorBruto);

/// <summary>
/// A LEITURA DAS APIS PÚBLICAS DO IBGE — o cadastro oficial de municípios e a área plantada.
///
/// <para><b>Só leitura, só GET, só dado público.</b> Nenhuma credencial é usada nem existe.</para>
///
/// <para><b>Por que a carga lê da API, e não de um arquivo no repositório.</b> O IBGE revisa a PAM e
/// cria município (o último em 2013 no Brasil, mas a lista muda de nome e grafia); guardar uma cópia
/// no Git faria o CRM envelhecer em silêncio. A carga registra o endereço lido e quando, em
/// <c>integracao.PontoDeSincronismo</c>.</para>
///
/// <para><b>A resposta vem comprimida</b> — o IBGE envia gzip mesmo sem o pedido explícito. O
/// <see cref="HttpClient"/> recebido precisa descomprimir (a carga o monta assim); sem isso o JSON
/// chega ilegível.</para>
/// </summary>
/// <param name="http">O cliente HTTP, com descompressão automática.</param>
public sealed class LeitorDoIbge(HttpClient http)
{
    /// <summary>
    /// Os 5.570 municípios, na visão "nivelada": uma linha por município com a UF no mesmo nível.
    /// A visão aninhada quebra para municípios sem microrregião, que existem na resposta atual.
    /// </summary>
    public const string EnderecoDosMunicipios =
        "https://servicodados.ibge.gov.br/api/v1/localidades/municipios?view=nivelado";

    /// <summary>A tabela do SIDRA com a área plantada por produto (PAM).</summary>
    public const short TabelaDaAreaPlantada = 5457;

    /// <summary>
    /// "Área plantada ou destinada à colheita", na tabela 5457.
    ///
    /// <para><b>Não troque por 216.</b> A 216 é a "Área colhida", e foi o que o leitor pediu até
    /// 19/09/2026 (issue 83): em cultura perene a colhida fica abaixo da plantada enquanto o cafezal ou o
    /// pomar novo não produz, e o potencial por área saía subestimado justamente onde há plantio novo.
    /// A área colhida tem uso próprio (produtividade), e entra pela #64 como outra coluna.</para>
    /// </summary>
    public const short VariavelDaAreaPlantada = 8331;

    /// <summary>
    /// A área plantada (variável <see cref="VariavelDaAreaPlantada"/>) de todos os produtos
    /// (classificação 782), no último ano publicado, para todos os municípios de uma UF.
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF. São Paulo é 35.</param>
    public static string EnderecoDaAreaPlantada(int codigoDaUf) =>
        $"https://apisidra.ibge.gov.br/values/t/{TabelaDaAreaPlantada}/n6/in%20n3%20{codigoDaUf}" +
        $"/v/{VariavelDaAreaPlantada}/p/last%201/c782/allxt?formato=json";

    /// <summary>A malha municipal de uma UF, em GeoJSON, na qualidade intermediária do IBGE.</summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    public static string EnderecoDaMalhaMunicipal(int codigoDaUf) =>
        $"https://servicodados.ibge.gov.br/api/v3/malhas/estados/{codigoDaUf}?formato=application/vnd.geo+json&intrarregiao=municipio&qualidade=intermediaria";

    /// <summary>
    /// Lê o contorno oficial de cada município de uma UF, pelo código IBGE.
    ///
    /// <para>A qualidade é a intermediária, e não a mínima do mapa da tela: para desenhar, a mínima
    /// basta; para decidir se uma coordenada cai dentro de um município, a borda simplificada
    /// erraria justamente os endereços perto da divisa.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyDictionary<int, PoligonoMunicipal>> LerMalhaMunicipalAsync(int codigoDaUf, CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(EnderecoDaMalhaMunicipal(codigoDaUf), ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        return documento.RootElement.GetProperty("features").EnumerateArray().ToDictionary(
            feicao => int.Parse(feicao.GetProperty("properties").GetProperty("codarea").GetString()!, CultureInfo.InvariantCulture),
            feicao => PoligonoMunicipal.DeGeoJson(feicao.GetProperty("geometry")));
    }

    /// <summary>Lê o cadastro oficial de municípios.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<MunicipioDoIbge>> LerMunicipiosAsync(CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(EnderecoDosMunicipios, ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        return
        [
            .. documento.RootElement.EnumerateArray().Select(m => new MunicipioDoIbge(
                m.GetProperty("municipio-id").GetInt32(),
                m.GetProperty("municipio-nome").GetString()!,
                m.GetProperty("UF-sigla").GetString()!))
        ];
    }

    /// <summary>
    /// Lê a área plantada de uma UF. A primeira linha da resposta do SIDRA é o cabeçalho, e fica
    /// de fora.
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<LinhaDaAreaPlantada>> LerAreaPlantadaAsync(int codigoDaUf, CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(EnderecoDaAreaPlantada(codigoDaUf), ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        return
        [
            .. documento.RootElement.EnumerateArray().Skip(1).Select(l => new LinhaDaAreaPlantada(
                int.Parse(l.GetProperty("D1C").GetString()!, CultureInfo.InvariantCulture),
                short.Parse(l.GetProperty("D3C").GetString()!, CultureInfo.InvariantCulture),
                int.Parse(l.GetProperty("D4C").GetString()!, CultureInfo.InvariantCulture),
                l.GetProperty("D4N").GetString()!,
                l.GetProperty("V").GetString()!))
        ];
    }
}
