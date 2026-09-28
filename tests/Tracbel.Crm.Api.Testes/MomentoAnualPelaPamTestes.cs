using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O MOMENTO DE PREÇO PELA PAM (decisão do Ricardo em 27/09/2026): enquanto a série mensal não fecha as duas janelas
/// de 12 meses, o momento da cultura é o preço implícito do último ano da PAM contra o anterior, na área de atuação.
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class MomentoAnualPelaPamTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int Franca = 3516200;
    private const int Uberaba = 3170107;

    [Fact]
    public async Task Sem_os_24_meses_da_CONAB_o_cafe_usa_o_ultimo_ano_da_PAM_contra_o_anterior_na_area_de_atuacao()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        var agora = DateTime.UtcNow;
        var franca = Municipio.Criar("Franca", "SP", Franca);
        var uberaba = Municipio.Criar("Uberaba", "MG", Uberaba);
        db.Municipios.AddRange(franca, uberaba);
        await db.SaveChangesAsync();

        db.MunicipiosDaAreaDeAtuacao.Add(
            MunicipioDaAreaDeAtuacao.Registrar(franca.Id, true, RegiaoDaAreaDeAtuacao.Norte, null, "Area de Atuação.xlsx", 2, 100, agora));

        // CAFÉ EM FRANCA (na área): R$ 20.000/t em 2024 e R$ 24.000/t em 2025 → 1,20. UBERABA fica FORA da área e tem
        // um preço absurdo em 2025: se entrasse na soma, o índice não seria 1,20.
        db.ProducoesAgricolasNosMunicipios.AddRange(
            ProducaoAgricolaNoMunicipio.Registrar(franca.Id, 2024, 40139, "Café (em grão) Total", new(1_000m, 1_000m, 1_000m, 20_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(franca.Id, 2025, 40139, "Café (em grão) Total", new(1_000m, 1_000m, 1_000m, 24_000m), 100, agora),
            ProducaoAgricolaNoMunicipio.Registrar(uberaba.Id, 2025, 40139, "Café (em grão) Total", new(10m, 10m, 1m, 999_000m), 100, agora));

        // A CONAB DO CAFÉ COM SEIS MESES: o mensal não fecha, e é por isso que o anual entra.
        for (var m = 0; m < 6; m++)
            db.CotacoesDeProdutos.Add(CotacaoDeProduto.Registrar(
                "CONAB", "11195", "SP", "RECEBIDO PELO PRODUTOR", "CAFE", "ARABICA", "kg", new DateOnly(2025, 9, 1).AddMonths(m), 30m, 100, agora));

        await db.SaveChangesAsync();

        var lido = await new RepositorioDeIndicadoresDeMercado(db).LerAsync(new DateOnly(2026, 9, 27), null, CancellationToken.None);

        var cafe = lido.PrecoPorCultura["CAFE"];
        cafe.Motivo.Should().Be(nameof(MotivoSemIndicador.Nenhum));
        cafe.Serie.Should().Be(nameof(SerieDoIndiceDePreco.AnualPam));
        cafe.AnoRecente.Should().Be((short)2025);
        cafe.Indice.Should().Be(1.20m, "24.000 ÷ 20.000, só a área de atuação");

        // A SOJA NÃO TEM PAM NEM CONAB AQUI: continua sem índice, e o motivo continua o do mensal.
        lido.PrecoPorCultura["SOJA"].Indice.Should().BeNull();
        lido.PrecoPorCultura["SOJA"].Serie.Should().Be(nameof(SerieDoIndiceDePreco.Mensal));
    }
}
