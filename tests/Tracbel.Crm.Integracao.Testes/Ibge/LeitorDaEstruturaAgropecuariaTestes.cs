using System.Net;
using System.Text;
using FluentAssertions;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Ibge;

/// <summary>
/// A LEITURA DA ESTRUTURA AGROPECUÁRIA (Censo, PPM e área territorial) — o que quebra em silêncio.
///
/// <list type="number">
///   <item><b>As duas variáveis da frota numa linha só.</b> O SIDRA devolve uma linha por variável;
///   sem juntá-las, a carga gravaria o mesmo município duas vezes.</item>
///   <item><b>As classificações que não entram na URL vêm no "Total"</b>, e é isso que faz a soma dos
///   municípios bater com o total do estado. Pedir a classificação errada mudaria o número sem
///   erro.</item>
///   <item><b>O ano vem dos METADADOS</b>, não do código: a PPM ganha um ano por ano e o Censo ganhará
///   2028. Fixar o ano aqui faria a carga envelhecer em silêncio.</item>
///   <item><b>A área territorial tem PONTO decimal.</b> "1521.202" é a capital, com 1.521,202 km² —
///   lida como milhar, seriam 1.521.202 km², seis vezes o estado inteiro.</item>
/// </list>
///
/// <para><b>Nenhuma chamada sai da máquina.</b> Os códigos são os reais do SIDRA; os números são
/// inventados, salvo onde o comentário disser o contrário.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDaEstruturaAgropecuariaTestes
{
    private const int SaoPaulo = 35;
    private const int RibeiraoPreto = 3543402;

    private sealed class TratadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(responder(pedido));
    }

    /// <summary>Os metadados de uma tabela, com o último ano publicado.</summary>
    private static string MetadadosCom(short fim) => $$"""
        {
          "id": 6871,
          "periodicidade": { "frequencia": "anual", "inicio": 2017, "fim": {{fim}} },
          "variaveis": [ { "id": 1862, "nome": "Número de tratores" } ],
          "classificacoes": []
        }
        """;

    /// <summary>
    /// Uma linha do SIDRA. As classificações não pedidas chegam em D5/D6/D7 como "Total" — é assim que
    /// a resposta real vem, e o leitor precisa ignorá-las sem tropeçar.
    /// </summary>
    private static string Linha(int recorte, short variavel, short ano, int categoria, string nome, string valor) => $$"""
        {
          "NC": "6", "NN": "Município", "MC": "1020", "MN": "Unidades", "V": "{{valor}}",
          "D1C": "{{recorte}}", "D1N": "Recorte",
          "D2C": "{{variavel}}", "D2N": "Variável",
          "D3C": "{{ano}}", "D3N": "{{ano}}",
          "D4C": "{{categoria}}", "D4N": "{{nome}}",
          "D5C": "110085", "D5N": "Total",
          "D6C": "46302", "D6N": "Total"
        }
        """;

    /// <summary>Uma linha de tabela SEM classificação nenhuma — a da área territorial não tem D4C.</summary>
    private static string LinhaSemCategoria(int recorte, short variavel, short ano, string valor) => $$"""
        {
          "NC": "6", "NN": "Município", "MC": "26", "MN": "Quilômetros quadrados", "V": "{{valor}}",
          "D1C": "{{recorte}}", "D1N": "Recorte",
          "D2C": "{{variavel}}", "D2N": "Variável",
          "D3C": "{{ano}}", "D3N": "{{ano}}"
        }
        """;

    private static string RespostaCom(params string[] linhas) =>
        "[{\"NC\":\"Nível Territorial (Código)\",\"V\":\"Valor\",\"D1C\":\"Município (Código)\"}" +
        (linhas.Length == 0 ? "" : "," + string.Join(",", linhas)) + "]";

    private static LeitorDaEstruturaAgropecuaria Leitor(
        short ultimoAno, string sidra, List<Uri>? pedidos = null) =>
        new(new HttpClient(new TratadorFalso(pedido =>
        {
            pedidos?.Add(pedido.RequestUri!);

            var corpo = pedido.RequestUri!.ToString().Contains("agregados", StringComparison.Ordinal)
                ? MetadadosCom(ultimoAno)
                : sidra;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(corpo, Encoding.UTF8, "application/json")
            };
        })));

    [Fact]
    public void O_endereco_pede_a_uf_inteira_no_nivel_de_municipio()
    {
        var endereco = LeitorDaEstruturaAgropecuaria.EnderecoNosMunicipios(
            6871, [1918, 1862], 2017, SaoPaulo, 12605);

        endereco.Should().StartWith("https://apisidra.ibge.gov.br/values/t/6871/");
        endereco.Should().Contain("/n6/in%20n3%2035/", "nível 6 é município, dentro da UF pedida");
        endereco.Should().Contain("/v/1918,1862/");
        endereco.Should().Contain("/p/2017/");
        endereco.Should().Contain("/c12605/all", "sem categorias explícitas, pede todas");
        endereco.Should().EndWith("?formato=json");
    }

    [Fact]
    public void Tabela_sem_classificacao_nao_ganha_o_trecho_de_categoria()
    {
        // A da área territorial não tem classificação. Mandar "/c/" vazio devolveria erro do SIDRA.
        var endereco = LeitorDaEstruturaAgropecuaria.EnderecoNosMunicipios(4714, [6318], 2022, SaoPaulo);

        endereco.Should().NotContain("/c", "a 4714 não tem classificação nenhuma");
        endereco.Should().Contain("/v/6318/p/2022?formato=json");
    }

    [Fact]
    public void Categorias_explicitas_entram_no_lugar_do_all()
    {
        var endereco = LeitorDaEstruturaAgropecuaria.EnderecoNosMunicipios(
            3939, [105], 2024, SaoPaulo, 79, LeitorDaEstruturaAgropecuaria.RebanhosQueOCrmGuarda);

        endereco.Should().Contain("/c79/2670", "2670 é o bovino, o único que o CRM guarda hoje");
        endereco.Should().NotContain("all");
    }

    [Fact]
    public async Task O_ano_vem_dos_metadados_e_nao_do_codigo()
    {
        // A PPM GANHA UM ANO POR ANO. Se o ano estivesse fixo no código, a carga continuaria trazendo
        // 2024 para sempre — e ninguém perceberia, porque ela não falharia.
        var pedidos = new List<Uri>();
        var resposta = RespostaCom(Linha(RibeiraoPreto, 105, 2025, 2670, "Bovino", "3000"));

        await Leitor(2025, resposta, pedidos).LerRebanhoAsync(SaoPaulo, CancellationToken.None);

        pedidos[0].ToString().Should().Contain("/agregados/3939/metadados");
        pedidos[^1].ToString().Should().Contain("/p/2025/", "o ano lido dos metadados é o que vai na consulta");
    }

    [Fact]
    public async Task As_duas_variaveis_da_frota_viram_uma_linha_por_faixa()
    {
        var resposta = RespostaCom(
            Linha(RibeiraoPreto, 1918, 2017, 113521, "Total", "172"),
            Linha(RibeiraoPreto, 1862, 2017, 113521, "Total", "450"),
            Linha(RibeiraoPreto, 1918, 2017, 113522, "Menos de 100 cv", "169"),
            Linha(RibeiraoPreto, 1862, 2017, 113522, "Menos de 100 cv", "300"));

        var linhas = await Leitor(2017, resposta).LerFrotaDeTratoresAsync(SaoPaulo, CancellationToken.None);

        linhas.Should().HaveCount(2, "quatro linhas do SIDRA, duas variáveis, duas faixas");

        var total = linhas.Single(l => l.PotenciaCodigo == 113521);
        total.CodigoDoMunicipio.Should().Be(RibeiraoPreto);
        total.Ano.Should().Be(2017);
        total.PotenciaNome.Should().Be("Total");
        total.EstabelecimentosComTratorBruto.Should().Be("172");
        total.TratoresBruto.Should().Be("450");

        linhas.Single(l => l.PotenciaCodigo == 113522).TratoresBruto.Should().Be("300");
    }

    [Fact]
    public async Task O_sigilo_do_ibge_chega_cru_para_o_saneamento_decidir()
    {
        // "X" é sigilo e vira NULO na carga — nunca zero. Quem decide é o saneamento, não o leitor.
        var resposta = RespostaCom(
            Linha(RibeiraoPreto, 1918, 2017, 113523, "De 100 cv e mais", "X"),
            Linha(RibeiraoPreto, 1862, 2017, 113523, "De 100 cv e mais", "..."));

        var linha = (await Leitor(2017, resposta).LerFrotaDeTratoresAsync(SaoPaulo, CancellationToken.None)).Single();

        linha.EstabelecimentosComTratorBruto.Should().Be("X");
        linha.TratoresBruto.Should().Be("...");
    }

    [Fact]
    public async Task Variavel_que_o_ibge_nao_devolveu_fica_nula()
    {
        var resposta = RespostaCom(Linha(RibeiraoPreto, 1862, 2017, 113521, "Total", "450"));

        var linha = (await Leitor(2017, resposta).LerFrotaDeTratoresAsync(SaoPaulo, CancellationToken.None)).Single();

        linha.TratoresBruto.Should().Be("450");
        linha.EstabelecimentosComTratorBruto.Should().BeNull("a variável 1918 não veio nesta resposta");
    }

    [Fact]
    public async Task Os_estabelecimentos_por_faixa_preservam_codigo_e_rotulo_do_ibge()
    {
        var resposta = RespostaCom(
            Linha(RibeiraoPreto, 183, 2017, 110085, "Total", "655"),
            Linha(RibeiraoPreto, 183, 2017, 111553, "De 20 a menos de 50 ha", "13"));

        var linhas = await Leitor(2017, resposta)
            .LerEstabelecimentosPorAreaAsync(SaoPaulo, CancellationToken.None);

        linhas.Should().HaveCount(2);
        linhas.Single(l => l.CategoriaCodigo == 111553).CategoriaNome.Should().Be("De 20 a menos de 50 ha");
        linhas.Single(l => l.CategoriaCodigo == 110085).ValorBruto.Should().Be("655");
    }

    [Fact]
    public async Task A_area_territorial_vem_sem_categoria_e_com_ponto_decimal()
    {
        // 1521.202 é a área REAL da capital: 1.521,202 km². Lido como milhar, viraria 1.521.202 km² —
        // seis vezes São Paulo inteiro. O leitor entrega o texto; quem converte é o saneamento, em
        // cultura invariante.
        var resposta = RespostaCom(
            LinhaSemCategoria(3550308, 6318, 2022, "1521.202"),
            LinhaSemCategoria(RibeiraoPreto, 6318, 2022, "650.916"));

        var linhas = await Leitor(2022, resposta).LerAreaTerritorialAsync(SaoPaulo, CancellationToken.None);

        linhas.Should().HaveCount(2);
        linhas.Single(l => l.CodigoDoMunicipio == 3550308).AreaKm2Bruta.Should().Be("1521.202");
        linhas.Single(l => l.CodigoDoMunicipio == RibeiraoPreto).AreaKm2Bruta.Should().Be("650.916");
        linhas[0].Ano.Should().Be(2022);
    }

    [Fact]
    public async Task O_cabecalho_da_resposta_nao_vira_dado()
    {
        // A primeira linha repete o NOME de cada campo no lugar do valor. Tratá-la como dado faria
        // "Município (Código)" virar um município.
        var resposta = RespostaCom();

        var linhas = await Leitor(2022, resposta).LerAreaTerritorialAsync(SaoPaulo, CancellationToken.None);

        linhas.Should().BeEmpty();
    }
}
