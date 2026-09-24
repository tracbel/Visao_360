using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O PREÇO RECEBIDO PELO PRODUTOR PELA API (issue 198).
///
/// <para>O domínio já prova a conta e as médias sem banco. O que só esta classe prova é a <b>agregação</b>:
/// que a soma vem antes da divisão, que ela cobre a ADR e não São Paulo todo, e que um município escolhido
/// devolve o preço dele.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class PrecoImplicitoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/territorio/preco-implicito";
    private const int Soja = 40441;

    /// <summary>Dois municípios da ADR e um de fora, com produções que dão contas redondas.</summary>
    private async Task<(int NaAdr, int ForaDaAdr)> SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        const int codigoDoPrimeiro = 3512345;
        const int codigoDoSegundo = 3512346;
        const int codigoDeFora = 3512347;

        if (!await db.Municipios.AnyAsync(m => m.CodigoIbge == codigoDoPrimeiro))
        {
            var primeiro = Municipio.Criar("Primeiro da ADR", "SP", codigoDoPrimeiro);
            var segundo = Municipio.Criar("Segundo da ADR", "SP", codigoDoSegundo);
            var fora = Municipio.Criar("Fora da ADR", "SP", codigoDeFora);
            db.Municipios.AddRange(primeiro, segundo, fora);
            await db.SaveChangesAsync();

            var agora = DateTime.UtcNow;

            db.MunicipiosDaAreaDeAtuacao.AddRange(
                // A LINHA COMEÇA EM 2: o domínio recusa a 1, que é o cabeçalho da planilha de origem.
                MunicipioDaAreaDeAtuacao.Registrar(primeiro.Id, true, RegiaoDaAreaDeAtuacao.Norte, null, "teste.csv", 2, 100, agora),
                MunicipioDaAreaDeAtuacao.Registrar(segundo.Id, true, RegiaoDaAreaDeAtuacao.Norte, null, "teste.csv", 3, 100, agora));

            // 2024 — o primeiro: R$ 300 mil por 100 t → R$ 3.000/t. O segundo: R$ 900 mil por 100 t →
            // R$ 9.000/t. A ADR soma 1.200 mil reais em 200 t = R$ 6.000/t, que é a média PONDERADA.
            db.ProducoesAgricolasNosMunicipios.AddRange(
                ProducaoAgricolaNoMunicipio.Registrar(primeiro.Id, 2024, Soja, "Soja (em grão)", new(1_000m, 1_000m, 100m, 300m), 100, agora),
                ProducaoAgricolaNoMunicipio.Registrar(segundo.Id, 2024, Soja, "Soja (em grão)", new(1_000m, 1_000m, 100m, 900m), 100, agora),
                // 2023 só no primeiro — é o que faz a média de 3 anos NÃO fechar.
                ProducaoAgricolaNoMunicipio.Registrar(primeiro.Id, 2023, Soja, "Soja (em grão)", new(1_000m, 1_000m, 100m, 200m), 100, agora),
                // O de FORA da ADR tem preço absurdo: se ele entrasse, o número da região mudaria.
                ProducaoAgricolaNoMunicipio.Registrar(fora.Id, 2024, Soja, "Soja (em grão)", new(1_000m, 1_000m, 1m, 900_000m), 100, agora));

            await db.SaveChangesAsync();
        }

        return (codigoDoPrimeiro, codigoDeFora);
    }

    private static async Task<JsonElement> LerAsync(HttpResponseMessage resposta)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.Clone();
    }

    private static JsonElement DaSoja(JsonElement corpo)
    {
        var series = corpo.GetProperty("dados").GetProperty("series").EnumerateArray().ToList();

        series.Should().ContainSingle(s => s.GetProperty("produtoCodigoIbge").GetInt32() == Soja,
            $"a resposta foi: {corpo.GetRawText()}");

        return series.Single(s => s.GetProperty("produtoCodigoIbge").GetInt32() == Soja);
    }

    [Fact]
    public async Task O_preco_da_regiao_e_a_soma_dividida_pela_soma_e_nao_a_media_dos_municipios()
    {
        await SemearAsync();

        var soja = DaSoja(await LerAsync(await api.ClienteDeRibeirao().GetAsync(Rota)));
        var recente = soja.GetProperty("anos").EnumerateArray().First();

        recente.GetProperty("ano").GetInt32().Should().Be(2024);
        recente.GetProperty("precoPorUnidade").GetDecimal().Should().Be(6_000m,
            "R$ 1.200 mil ÷ 200 t. A média simples dos dois municípios daria R$ 6.000 por acaso aqui só " +
            "porque as quantidades são iguais — o que este teste trava é a ORDEM: somar e depois dividir");
    }

    [Fact]
    public async Task O_municipio_de_fora_da_ADR_nao_entra_na_conta_da_regiao()
    {
        var (_, fora) = await SemearAsync();

        var regiao = DaSoja(await LerAsync(await api.ClienteDeRibeirao().GetAsync(Rota)));
        var recenteDaRegiao = regiao.GetProperty("anos").EnumerateArray().First();

        // O de fora tem R$ 900 milhões em 1 tonelada. Se ele entrasse, o preço da região explodiria.
        recenteDaRegiao.GetProperty("precoPorUnidade").GetDecimal().Should().Be(6_000m);

        // E ele existe: pedindo o município dele, o número aparece.
        var doVizinho = DaSoja(await LerAsync(await api.ClienteDeRibeirao().GetAsync($"{Rota}?municipioCodigoIbge={fora}")));
        doVizinho.GetProperty("anos").EnumerateArray().First()
            .GetProperty("precoPorUnidade").GetDecimal().Should().Be(900_000_000m);
    }

    [Fact]
    public async Task Um_municipio_escolhido_devolve_o_preco_dele_e_nao_o_da_regiao()
    {
        var (naAdr, _) = await SemearAsync();

        var soja = DaSoja(await LerAsync(await api.ClienteDeRibeirao().GetAsync($"{Rota}?municipioCodigoIbge={naAdr}")));

        soja.GetProperty("anos").EnumerateArray().First()
            .GetProperty("precoPorUnidade").GetDecimal().Should().Be(3_000m, "R$ 300 mil ÷ 100 t");
    }

    [Fact]
    public async Task A_media_de_tres_anos_nao_sai_com_dois_anos_e_diz_qual_faltou()
    {
        await SemearAsync();

        var soja = DaSoja(await LerAsync(await api.ClienteDeRibeirao().GetAsync(Rota)));
        var media = soja.GetProperty("mediaDeTresAnos");

        media.GetProperty("preco").ValueKind.Should().Be(JsonValueKind.Null,
            "só há 2023 e 2024: uma média de dois anos rotulada como de três parece completa");
        media.GetProperty("anosFaltando").EnumerateArray().Select(a => a.GetInt32()).Should().Equal(2022);
    }

    [Fact]
    public async Task A_resposta_diz_que_esta_serie_nao_se_soma_a_da_CONAB()
    {
        await SemearAsync();

        var corpo = await LerAsync(await api.ClienteDeRibeirao().GetAsync(Rota));

        corpo.GetProperty("dados").GetProperty("ressalva").GetString()
            .Should().Contain("ANUAL").And.Contain("sem deflator").And.Contain("CONAB");
    }

    [Fact]
    public async Task A_contagem_de_municipios_diz_quantos_sustentam_o_numero_da_regiao()
    {
        await SemearAsync();

        var soja = DaSoja(await LerAsync(await api.ClienteDeRibeirao().GetAsync(Rota)));

        soja.GetProperty("municipiosComDadoNoUltimoAno").GetInt32().Should().Be(2,
            "sem isto, \"R$ 6.000 a tonelada na região\" poderia ser de um município só");
    }
}
