using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Integracao.Ibge;

/// <summary>
/// UM PRODUTO, NUM RECORTE, NUM ANO — com as quatro medidas que a PAM publica, ainda como texto.
///
/// <para><b>Os valores chegam crus, com os símbolos do IBGE</b> (<c>-</c> para zero, <c>...</c> e
/// <c>X</c> para não disponível). Quem converte é o saneamento da carga: o leitor não decide o que é
/// zero e o que é ausência.</para>
///
/// <para><b>Uma linha por (recorte, ano, produto)</b>, e não uma por variável: o SIDRA devolve uma
/// linha por variável, e o leitor as junta. É assim que a carga grava uma linha só, em vez de quatro
/// que repetem município, ano e produto.</para>
/// </summary>
/// <param name="CodigoDoRecorte">O código IBGE do município (nível 6) ou da UF (nível 3).</param>
/// <param name="Ano">O ano da pesquisa.</param>
/// <param name="ProdutoCodigo">O código do produto na classificação 782.</param>
/// <param name="ProdutoNome">O rótulo oficial do produto.</param>
/// <param name="AreaPlantadaBruta">Variável 8331, como o SIDRA escreveu.</param>
/// <param name="AreaColhidaBruta">Variável 216.</param>
/// <param name="QuantidadeProduzidaBruta">Variável 214.</param>
/// <param name="ValorDaProducaoBruto">Variável 215, em mil reais.</param>
public sealed record LinhaDaProducaoAgricola(
    int CodigoDoRecorte,
    short Ano,
    int ProdutoCodigo,
    string ProdutoNome,
    string? AreaPlantadaBruta,
    string? AreaColhidaBruta,
    string? QuantidadeProduzidaBruta,
    string? ValorDaProducaoBruto);

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

    /// <summary>A tabela do SIDRA da Produção Agrícola Municipal, por produto.</summary>
    public const short TabelaDaProducaoAgricola = 5457;

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

    /// <summary>"Área colhida" — o que de fato se colheu, contra o que se plantou.</summary>
    public const short VariavelDaAreaColhida = 216;

    /// <summary>"Quantidade produzida", na unidade que o IBGE usa para cada produto.</summary>
    public const short VariavelDaQuantidadeProduzida = 214;

    /// <summary>"Valor da produção", em MIL reais.</summary>
    public const short VariavelDoValorDaProducao = 215;

    /// <summary>
    /// As quatro variáveis da PAM que o CRM guarda, na ordem em que a URL as pede.
    ///
    /// <para>A 112 (rendimento médio) fica de fora de propósito: ela é quantidade ÷ área colhida, e
    /// guardar um número derivado ao lado das duas parcelas é criar uma terceira fonte para a mesma
    /// verdade — a que diverge primeiro.</para>
    /// </summary>
    public static readonly short[] VariaveisDaProducao =
        [VariavelDaAreaPlantada, VariavelDaAreaColhida, VariavelDaQuantidadeProduzida, VariavelDoValorDaProducao];

    /// <summary>
    /// Quantos produtos cabem numa consulta ao SIDRA, com as quatro variáveis.
    ///
    /// <para><b>O limite é de TAMANHO da resposta, e ele morde de verdade</b> (issue 95). Medido em
    /// 20/09/2026, para os 645 municípios de São Paulo: 1 variável × 10 produtos devolveu 6.421
    /// linhas e HTTP 200; 1 variável × 85 produtos (~55 mil valores) devolveu <b>400</b>;
    /// <b>4 variáveis × 10 produtos devolveu 25.681 linhas e 5,3 MB, com HTTP 200</b>; e 4 × 20
    /// (~51,6 mil) devolveu <b>400</b>. O teto está perto de 50 mil valores por consulta.</para>
    ///
    /// <para>Dez deixa cada consulta em ~26 mil linhas, com metade do teto de folga — inclusive para
    /// o dia em que o IBGE publicar mais município ou mais produto.</para>
    /// </summary>
    public const int ProdutosPorConsulta = 10;

    /// <summary>
    /// As quatro medidas de um LOTE de produtos, num ano, para todos os municípios de uma UF.
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF. São Paulo é 35.</param>
    /// <param name="produtos">Os códigos dos produtos na classificação 782.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    public static string EnderecoDaProducaoNosMunicipios(int codigoDaUf, IEnumerable<int> produtos, short ano) =>
        EnderecoDaProducao($"n6/in%20n3%20{codigoDaUf}", produtos, ano);

    /// <summary>
    /// As quatro medidas de um LOTE de produtos, num ano, no TOTAL da UF.
    ///
    /// <para>É a linha que o IBGE publica para o estado inteiro — e que não é a soma dos municípios,
    /// porque o valor municipal sigiloso entra nela sem aparecer embaixo.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="produtos">Os códigos dos produtos.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    public static string EnderecoDaProducaoNoEstado(int codigoDaUf, IEnumerable<int> produtos, short ano) =>
        EnderecoDaProducao($"n3/{codigoDaUf}", produtos, ano);

    private static string EnderecoDaProducao(string nivel, IEnumerable<int> produtos, short ano) =>
        $"https://apisidra.ibge.gov.br/values/t/{TabelaDaProducaoAgricola}/{nivel}" +
        $"/v/{string.Join(',', VariaveisDaProducao)}/p/{ano}/c{ClassificacaoDeProduto}/" +
        string.Join(',', produtos) +
        "?formato=json";

    // =============================================================================================
    // O milho por safra — tabela 839 (issue 156)
    // =============================================================================================

    /// <summary>
    /// "Área plantada, área colhida, quantidade produzida e rendimento médio de milho, 1ª e 2ª safras".
    ///
    /// <para><b>Conferida nos metadados em 22/09/2026</b>, como a issue manda: a 839 tem nível
    /// municipal (N6), vai de 2003 a 2025 e traz a classificação 81 com "Total", "1ª safra" e
    /// "2ª safra". Serve.</para>
    /// </summary>
    public const short TabelaDoMilhoPorSafra = 839;

    /// <summary>A classificação "Produto das lavouras temporárias", onde moram as duas safras.</summary>
    public const short ClassificacaoDaSafra = 81;

    /// <summary>"Milho (em grão) - 1ª safra".</summary>
    public const int MilhoPrimeiraSafra = 114253;

    /// <summary>"Milho (em grão) - 2ª safra".</summary>
    public const int MilhoSegundaSafra = 114254;

    /// <summary>As duas safras que a carga traz — o "Total" (31693) fica de fora, porque já é o milho da 5457.</summary>
    public static readonly int[] SafrasDoMilho = [MilhoPrimeiraSafra, MilhoSegundaSafra];

    /// <summary>
    /// "Área plantada" NESTA tabela é a variável <b>109</b>, e não a 8331 da PAM.
    ///
    /// <para>Pedir a 8331 aqui devolve resposta vazia — não erro —, e a área do safrinha sumiria em
    /// silêncio, que é exatamente o defeito que esta issue existe para impedir.</para>
    /// </summary>
    public const short VariavelDaAreaPlantadaNoMilho = 109;

    /// <summary>As três medidas da 839 que o CRM guarda; a 112 (rendimento) é derivada e fica de fora.</summary>
    public static readonly short[] VariaveisDoMilhoPorSafra =
        [VariavelDaAreaPlantadaNoMilho, VariavelDaAreaColhida, VariavelDaQuantidadeProduzida];

    /// <summary>O endereço do milho por safra, nos municípios de uma UF, num ano.</summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    public static string EnderecoDoMilhoPorSafra(int codigoDaUf, short ano) =>
        $"https://apisidra.ibge.gov.br/values/t/{TabelaDoMilhoPorSafra}/n6/in%20n3%20{codigoDaUf}" +
        $"/v/{string.Join(',', VariaveisDoMilhoPorSafra)}/p/{ano}" +
        $"/c{ClassificacaoDaSafra}/{string.Join(',', SafrasDoMilho)}?formato=json";

    /// <summary>Os metadados da 839, para saber até quando ela publica.</summary>
    public const string EnderecoDosMetadadosDoMilhoPorSafra =
        "https://servicodados.ibge.gov.br/api/v3/agregados/839/metadados";

    /// <summary>
    /// O período que a 839 publica, pelos metadados — ela começa em 2003, e a PAM em 1974.
    ///
    /// <para>Sem isso, uma série pedida desde 2000 faria a carga bater na 839 três vezes para
    /// receber resposta vazia, e "vazio" é ambíguo demais para se confiar nele (issue 153).</para>
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<(short Inicio, short Fim)> LerPeriodoDoMilhoPorSafraAsync(CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(EnderecoDosMetadadosDoMilhoPorSafra, ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        var periodicidade = documento.RootElement.GetProperty("periodicidade");
        return (periodicidade.GetProperty("inicio").GetInt16(), periodicidade.GetProperty("fim").GetInt16());
    }

    /// <summary>
    /// O milho de 1ª e 2ª safra dos municípios de uma UF, num ano.
    ///
    /// <para><b>Sem lote:</b> são duas categorias e três variáveis, contra as 85 da PAM — cabe numa
    /// consulta só.</para>
    ///
    /// <para>As linhas voltam no mesmo formato da PAM, com o valor da produção sempre nulo: esta
    /// tabela não publica valor, e ele existe só para o milho inteiro, na 5457.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<LinhaDaProducaoAgricola>> LerMilhoPorSafraAsync(
        int codigoDaUf, short ano, CancellationToken ct)
    {
        var porChave = new Dictionary<(int Recorte, short Ano, int Safra), string?[]>();
        var nomes = new Dictionary<int, string>();
        var ordem = new List<(int Recorte, short Ano, int Safra)>();

        await using var corpo = await http.GetStreamAsync(EnderecoDoMilhoPorSafra(codigoDaUf, ano), ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        foreach (var l in documento.RootElement.EnumerateArray().Skip(1))
        {
            var variavel = short.Parse(l.GetProperty("D2C").GetString()!, CultureInfo.InvariantCulture);
            var posicao = Array.IndexOf(VariaveisDoMilhoPorSafra, variavel);
            if (posicao < 0) continue;

            var chave = (
                int.Parse(l.GetProperty("D1C").GetString()!, CultureInfo.InvariantCulture),
                short.Parse(l.GetProperty("D3C").GetString()!, CultureInfo.InvariantCulture),
                int.Parse(l.GetProperty("D4C").GetString()!, CultureInfo.InvariantCulture));

            if (!porChave.TryGetValue(chave, out var valores))
            {
                valores = new string?[VariaveisDoMilhoPorSafra.Length];
                porChave[chave] = valores;
                ordem.Add(chave);
            }

            valores[posicao] = l.GetProperty("V").GetString();
            nomes[chave.Item3] = l.GetProperty("D4N").GetString()!;
        }

        return
        [
            .. ordem.Select(c => new LinhaDaProducaoAgricola(
                c.Recorte, c.Ano, c.Safra, nomes[c.Safra],
                porChave[c][0], porChave[c][1], porChave[c][2], ValorDaProducaoBruto: null))
        ];
    }

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
    /// O último ano publicado da PAM, pelos metadados (<c>periodicidade.fim</c>).
    ///
    /// <para><b>Por que não <c>p/last 1</c>:</b> a carga pede ano por ano para poder trazer a série
    /// curta, e "o último" precisa ser um número antes da primeira consulta. Perguntar aos metadados
    /// custa uma requisição e não depende de adivinhar.</para>
    /// </summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<short> LerUltimoAnoDaPamAsync(CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(EnderecoDosMetadadosDaPam, ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        return documento.RootElement.GetProperty("periodicidade").GetProperty("fim").GetInt16();
    }

    /// <summary>
    /// A produção agrícola dos MUNICÍPIOS de uma UF, num ano, com as quatro medidas.
    ///
    /// <para><b>Em lotes de produto:</b> pedir todos de uma vez devolve 400 no SIDRA (issue 95). Ver
    /// <see cref="ProdutosPorConsulta"/>.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<IReadOnlyList<LinhaDaProducaoAgricola>> LerProducaoNosMunicipiosAsync(
        int codigoDaUf, short ano, CancellationToken ct) =>
        LerProducaoAsync(lote => EnderecoDaProducaoNosMunicipios(codigoDaUf, lote, ano), ct);

    /// <summary>
    /// A produção agrícola no TOTAL da UF, num ano, com as quatro medidas.
    ///
    /// <para>Uma linha por produto, e não 645: cabe numa consulta só por lote, sem chegar perto do
    /// teto de tamanho.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ano">O ano da pesquisa.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<IReadOnlyList<LinhaDaProducaoAgricola>> LerProducaoNoEstadoAsync(
        int codigoDaUf, short ano, CancellationToken ct) =>
        LerProducaoAsync(lote => EnderecoDaProducaoNoEstado(codigoDaUf, lote, ano), ct);

    /// <summary>
    /// O laço comum das duas leituras: lote a lote, junta as QUATRO LINHAS que o SIDRA devolve para
    /// cada (recorte, ano, produto) — uma por variável — numa linha só.
    ///
    /// <para><b>A variável vem em <c>D2C</c></b>, e é por ela que cada valor encontra o seu lugar.
    /// Variável que o IBGE não devolveu fica nula, e nulo aqui significa "o SIDRA não trouxe" — é
    /// diferente do <c>...</c>, que é "existe e não foi divulgado" e chega como texto.</para>
    /// </summary>
    private async Task<IReadOnlyList<LinhaDaProducaoAgricola>> LerProducaoAsync(
        Func<int[], string> endereco, CancellationToken ct)
    {
        var produtos = await LerProdutosDaPamAsync(ct);
        var porChave = new Dictionary<(int Recorte, short Ano, int Produto), string?[]>();
        var nomes = new Dictionary<int, string>();
        var ordem = new List<(int Recorte, short Ano, int Produto)>();

        foreach (var lote in produtos.Chunk(ProdutosPorConsulta))
        {
            await using var corpo = await http.GetStreamAsync(endereco(lote), ct);
            using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

            foreach (var l in documento.RootElement.EnumerateArray().Skip(1))
            {
                var recorte = int.Parse(l.GetProperty("D1C").GetString()!, CultureInfo.InvariantCulture);
                var variavel = short.Parse(l.GetProperty("D2C").GetString()!, CultureInfo.InvariantCulture);
                var ano = short.Parse(l.GetProperty("D3C").GetString()!, CultureInfo.InvariantCulture);
                var produto = int.Parse(l.GetProperty("D4C").GetString()!, CultureInfo.InvariantCulture);

                var posicao = Array.IndexOf(VariaveisDaProducao, variavel);
                if (posicao < 0) continue;

                var chave = (recorte, ano, produto);
                if (!porChave.TryGetValue(chave, out var valores))
                {
                    valores = new string?[VariaveisDaProducao.Length];
                    porChave[chave] = valores;
                    ordem.Add(chave);
                }

                valores[posicao] = l.GetProperty("V").GetString();
                nomes[produto] = l.GetProperty("D4N").GetString()!;
            }
        }

        return
        [
            .. ordem.Select(c => new LinhaDaProducaoAgricola(
                c.Recorte, c.Ano, c.Produto, nomes[c.Produto],
                porChave[c][0], porChave[c][1], porChave[c][2], porChave[c][3]))
        ];
    }
}
