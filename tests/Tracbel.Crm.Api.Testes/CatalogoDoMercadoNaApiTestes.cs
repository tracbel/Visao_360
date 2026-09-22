using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O CATÁLOGO DE CULTURAS E CATEGORIAS PELA API DE VERDADE (issue 165).
///
/// <para>É este catálogo que tira as listas fixas de cultura do front — e é por ele que cultura nova
/// entra pela tela, sem publicação. A semente é o anexo 49C, medido nas fontes em 22/09/2026.</para>
/// </summary>
[Trait("Categoria", "ParametrosDoPotencial")]
public sealed class CatalogoDoMercadoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/admin/parametros-do-potencial/catalogo";

    /// <summary>
    /// O cliente que administra: o usuário de Ribeirão com o perfil de Administrador concedido.
    ///
    /// <para>É o mesmo caminho dos testes da issue 71 — o perfil padrão LÊ os parâmetros, e só o
    /// administrador os altera.</para>
    /// </summary>
    private async Task<HttpClient> AdministradorAsync()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        return api.ClienteDeRibeirao();
    }

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    [Fact]
    public async Task O_catalogo_traz_as_seis_culturas_semeadas_com_o_vinculo_de_cada_fonte()
    {
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var culturas = dados.GetProperty("culturas").EnumerateArray().ToList();
        culturas.Select(c => c.GetProperty("codigo").GetString())
            .Should().BeEquivalentTo(["CAFE", "CANA", "SOJA", "MILHO", "LARANJA", "AMENDOIM"]);

        var cafe = culturas.Single(c => c.GetProperty("codigo").GetString() == "CAFE");
        cafe.GetProperty("segmento").GetString().Should().Be("Cafe");
        cafe.GetProperty("quilosPorUnidade").GetDecimal().Should().Be(60m);
        cafe.GetProperty("fonteDoPreco").GetString().Should().Be("CONAB");
        cafe.GetProperty("produtoDoPreco").GetString().Should().Be("11195", "o id_produto da CONAB (anexo 49C)");
        cafe.GetProperty("serieDeCusto").GetString().Should().Be("CAFÉ ARÁBICA");

        // SÓ O TOTAL ENTRA NA SOMA: Arábica e Canephora estão no catálogo, mas somar os três dobraria o café.
        var produtos = cafe.GetProperty("produtos").EnumerateArray().ToList();
        produtos.Should().HaveCount(3);
        produtos.Where(p => p.GetProperty("entraNaSomaDaLavoura").GetBoolean())
            .Select(p => p.GetProperty("codigoIbge").GetInt32()).Should().Equal([40139]);
    }

    [Fact]
    public async Task As_categorias_vem_na_ordem_e_tres_delas_sem_produto_do_sicor()
    {
        // O investimento do Banco Central não separa plantadeira, pulverizador nem agricultura de precisão.
        // Lista vazia não é erro — é o limite da fonte, e a categoria existe do mesmo jeito.
        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        var categorias = dados.GetProperty("categorias").EnumerateArray().ToList();
        categorias.Select(c => c.GetProperty("codigo").GetString()).Should().Equal(
            ["TRATOR", "PLANTADEIRA", "COLHEITADEIRA", "PULVERIZADOR", "IMPLEMENTO", "PRECISAO"]);

        categorias.Single(c => c.GetProperty("codigo").GetString() == "TRATOR")
            .GetProperty("produtosDoSicor").EnumerateArray().Select(p => p.GetInt32()).Should().Equal([7080]);

        categorias.Where(c => c.GetProperty("produtosDoSicor").GetArrayLength() == 0)
            .Select(c => c.GetProperty("codigo").GetString())
            .Should().BeEquivalentTo(["PLANTADEIRA", "PULVERIZADOR", "PRECISAO"]);
    }

    [Fact]
    public async Task Cultura_nova_entra_pela_tela_e_passa_a_aparecer_no_catalogo()
    {
        // O CRITÉRIO DE ACEITE: cadastrar uma cultura faz ela aparecer, sem publicação nova.
        var corpo = new
        {
            codigo = "sorgo",
            nome = "Sorgo",
            segmento = "Graos",
            unidadeComercial = "saca de 60 kg",
            quilosPorUnidade = "60",
            fonteDoPreco = "CONAB",
            produtoDoPreco = "4745"
        };

        var resposta = await (await AdministradorAsync()).PostAsJsonAsync($"{Rota}/culturas", corpo);
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync(Rota));
        var sorgo = dados.GetProperty("culturas").EnumerateArray()
            .Single(c => c.GetProperty("codigo").GetString() == "SORGO");

        sorgo.GetProperty("nome").GetString().Should().Be("Sorgo");
        sorgo.GetProperty("estaAtiva").GetBoolean().Should().BeTrue();
        sorgo.GetProperty("produtos").GetArrayLength().Should().Be(0, "o vínculo com a PAM é passo à parte");
    }

    [Fact]
    public async Task A_fonte_do_preco_sem_o_produto_dela_e_recusada_com_o_campo()
    {
        // Uma fonte sem produto não acha nada. A recusa vem com o campo, e não como erro de servidor.
        var resposta = await api.ClienteDeRibeirao().PostAsJsonAsync($"{Rota}/culturas", new
        {
            codigo = "TESTE_SEM_PRODUTO",
            nome = "Teste",
            segmento = "Graos",
            unidadeComercial = "saca de 60 kg",
            quilosPorUnidade = "60",
            fonteDoPreco = "CONAB"
        });

        resposta.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Quem_nao_administra_nao_cadastra_cultura()
    {
        // 403, e não 200 silencioso: o catálogo decide o que a tela inteira mostra. Barretos lê os parâmetros
        // (é o perfil padrão), mas não os administra — é o mesmo caso da issue 71.
        var resposta = await api.ClienteDeBarretos().PostAsJsonAsync($"{Rota}/culturas", new
        {
            codigo = "NAO_DEVE_ENTRAR",
            nome = "Não deve entrar",
            segmento = "Graos",
            unidadeComercial = "saca",
            quilosPorUnidade = "60"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
