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

    /// <summary>A classificação do SIDRA que lista os produtos da PAM.</summary>
    public const short ClassificacaoDeProduto = 782;

    /// <summary>
    /// Os metadados da tabela 5457 — é deles que sai a lista de produtos.
    ///
    /// <para><b>Por que não uma lista fixa no código:</b> o IBGE acrescenta e renomeia produto, e uma
    /// lista escrita aqui envelheceria em silêncio, exatamente como a cópia de municípios que o
    /// comentário da classe já recusa.</para>
    /// </summary>
    public const string EnderecoDosMetadadosDaPam =
        "https://servicodados.ibge.gov.br/api/v3/agregados/5457/metadados";

    /// <summary>
    /// Quantos produtos cabem numa consulta ao SIDRA.
    ///
    /// <para><b>O limite é de TAMANHO da resposta, e ele morde de verdade</b> (issue 95). Em
    /// 20/09/2026 a consulta de um produto para os 645 municípios de São Paulo devolveu ~645 linhas;
    /// com 10 produtos, 6.421 linhas e HTTP 200; com os 85 produtos da classificação — ~55 mil
    /// valores — o SIDRA passou a responder <b>400 Bad Request</b>. A mesma URL funcionava em
    /// 14/09/2026 e trouxe 45.582 linhas, então o teto mudou de lado de lá, não daqui.</para>
    ///
    /// <para>Vinte deixa cada consulta em torno de 13 mil linhas, com folga para o dia em que o IBGE
    /// publicar mais município ou mais produto.</para>
    /// </summary>
    public const int ProdutosPorConsulta = 20;

    /// <summary>
    /// A área plantada (variável <see cref="VariavelDaAreaPlantada"/>) de um LOTE de produtos, no
    /// último ano publicado, para todos os municípios de uma UF.
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF. São Paulo é 35.</param>
    /// <param name="produtos">Os códigos dos produtos na classificação 782.</param>
    public static string EnderecoDaAreaPlantada(int codigoDaUf, IEnumerable<int> produtos) =>
        $"https://apisidra.ibge.gov.br/values/t/{TabelaDaAreaPlantada}/n6/in%20n3%20{codigoDaUf}" +
        $"/v/{VariavelDaAreaPlantada}/p/last%201/c{ClassificacaoDeProduto}/" +
        string.Join(',', produtos) +
        "?formato=json";

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
    /// Os produtos da classificação 782, pelos metadados da tabela.
    ///
    /// <para><b>O "Total" fica de fora.</b> A categoria 0 é a soma dos demais; trazê-la junto
    /// dobraria a área de cada município na hora de somar.</para>
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<int>> LerProdutosDaPamAsync(CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(EnderecoDosMetadadosDaPam, ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        var classificacao = documento.RootElement.GetProperty("classificacoes").EnumerateArray()
            .First(c => c.GetProperty("id").GetInt32() == ClassificacaoDeProduto);

        return
        [
            .. classificacao.GetProperty("categorias").EnumerateArray()
                .Select(c => c.GetProperty("id").GetInt32())
                .Where(id => id != 0)
        ];
    }

    /// <summary>
    /// Lê a área plantada de uma UF, EM LOTES DE PRODUTO.
    ///
    /// <para><b>Por que em lotes:</b> pedir os 85 produtos de uma vez passou a devolver 400 no SIDRA
    /// (issue 95) — são ~55 mil valores. Ver <see cref="ProdutosPorConsulta"/>.</para>
    ///
    /// <para>Cada resposta traz o próprio cabeçalho na primeira posição, e cada um fica de fora.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<LinhaDaAreaPlantada>> LerAreaPlantadaAsync(int codigoDaUf, CancellationToken ct)
    {
        var produtos = await LerProdutosDaPamAsync(ct);
        var linhas = new List<LinhaDaAreaPlantada>();

        foreach (var lote in produtos.Chunk(ProdutosPorConsulta))
        {
            await using var corpo = await http.GetStreamAsync(EnderecoDaAreaPlantada(codigoDaUf, lote), ct);
            using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

            linhas.AddRange(documento.RootElement.EnumerateArray().Skip(1).Select(l => new LinhaDaAreaPlantada(
                int.Parse(l.GetProperty("D1C").GetString()!, CultureInfo.InvariantCulture),
                short.Parse(l.GetProperty("D3C").GetString()!, CultureInfo.InvariantCulture),
                int.Parse(l.GetProperty("D4C").GetString()!, CultureInfo.InvariantCulture),
                l.GetProperty("D4N").GetString()!,
                l.GetProperty("V").GetString()!)));
        }

        return linhas;
    }
}
