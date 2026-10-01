using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// OS FILTROS DA PERFORMANCE DE CEN NA MAQUETE (01/10/2026): a classe do cliente na cobertura por carteira e a carteira
/// no painel do CEN.
///
/// <para>A classe segue a regra do painel do CEN — o cliente sem classe apurada conta como D —, para os cartões e o
/// gráfico por classe da tela não divergirem.</para>
/// </summary>
[Trait("Categoria", "Relacionamento")]
public sealed class PerformanceDaMaqueteTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string CodigoDaGrande = "PERF_GRANDE";
    private const string CodigoDaPequena = "PERF_PEQUENA";

    /// <summary>
    /// Duas carteiras comerciais da mesma pessoa: a grande com um cliente de cada classe e um sem classe, a pequena com
    /// um cliente só.
    /// </summary>
    private async Task<(Guid Grande, Guid Pequena)> SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (!await db.Carteiras.AnyAsync(c => c.Codigo == CodigoDaGrande))
        {
            var linha = LinhaDeNegocio.Criar("MAQ_PERF", "Máquinas da performance");
            db.LinhasDeNegocio.Add(linha);
            await db.SaveChangesAsync();

            var grande = Carteira.Criar(1, linha.Id, CodigoDaGrande, "Carteira grande da performance", 100, 100);
            var pequena = Carteira.Criar(1, linha.Id, CodigoDaPequena, "Carteira pequena da performance", 100, 100);
            db.Carteiras.AddRange(grande, pequena);

            var a = Cliente.Criar(1, "Performance A Ltda", TipoDePessoa.Juridica, 100, 100);
            var b = Cliente.Criar(1, "Performance B Ltda", TipoDePessoa.Juridica, 100, 100);
            var d = Cliente.Criar(1, "Performance D Ltda", TipoDePessoa.Juridica, 100, 100);
            var semClasse = Cliente.Criar(1, "Performance Sem Classe", TipoDePessoa.Juridica, 100, 100);
            var daPequena = Cliente.Criar(1, "Performance Pequena B Ltda", TipoDePessoa.Juridica, 100, 100);
            db.Clientes.AddRange(a, b, d, semClasse, daPequena);
            await db.SaveChangesAsync();

            a.ApurarClasse(ClasseDeCliente.A, 1_000_000m, DateTime.UtcNow, 100);
            b.ApurarClasse(ClasseDeCliente.B, 100_000m, DateTime.UtcNow, 100);
            d.ApurarClasse(ClasseDeCliente.D, 100m, DateTime.UtcNow, 100);
            daPequena.ApurarClasse(ClasseDeCliente.B, 90_000m, DateTime.UtcNow, 100);

            foreach (var cliente in new[] { a, b, d, semClasse })
                db.ClienteCarteiras.Add(ClienteCarteira.Criar(cliente.Id, grande.Id, ClasseDeCliente.C, 100));
            db.ClienteCarteiras.Add(ClienteCarteira.Criar(daPequena.Id, pequena.Id, ClasseDeCliente.C, 100));

            await db.SaveChangesAsync();
        }

        var chaves = await db.Carteiras
            .Where(c => c.Codigo == CodigoDaGrande || c.Codigo == CodigoDaPequena)
            .ToDictionaryAsync(c => c.Codigo, c => c.ChavePublica);
        return (chaves[CodigoDaGrande], chaves[CodigoDaPequena]);
    }

    private async Task<JsonElement> DadosAsync(string rota)
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync(rota);
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    /// <summary>Os vínculos de uma carteira no resumo, ou zero quando ela não aparece.</summary>
    private static int VinculosDa(JsonElement resumo, string codigo) =>
        resumo.GetProperty("itens").EnumerateArray()
            .Where(i => i.GetProperty("carteiraCodigo").GetString() == codigo)
            .Select(i => i.GetProperty("clientes").GetInt32())
            .SingleOrDefault();

    [Theory]
    [InlineData(null, 4)]
    [InlineData("A", 1)]
    [InlineData("B", 1)]
    [InlineData("C", 0)]
    public async Task A_classe_conta_so_os_clientes_dela_na_cobertura_por_carteira(string? classe, int esperados)
    {
        await SemearAsync();

        var rota = classe is null ? "/api/v1/relatorios/cobertura" : $"/api/v1/relatorios/cobertura?classe={classe}";
        VinculosDa(await DadosAsync(rota), CodigoDaGrande).Should().Be(esperados);
    }

    [Fact]
    public async Task O_cliente_sem_classe_apurada_conta_como_D_como_no_painel_do_CEN()
    {
        await SemearAsync();

        VinculosDa(await DadosAsync("/api/v1/relatorios/cobertura?classe=D"), CodigoDaGrande)
            .Should().Be(2, "o D apurado e o cliente sem classe");
    }

    [Fact]
    public async Task Classe_fora_do_dominio_e_recusada()
    {
        await SemearAsync();

        var resposta = await api.ClienteDeRibeirao().GetAsync("/api/v1/relatorios/cobertura?classe=Z");

        resposta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task A_carteira_recorta_o_painel_do_CEN_numa_carteira_so()
    {
        var (grande, pequena) = await SemearAsync();

        var daGrande = (await DadosAsync($"/api/v1/relatorios/cen?carteira={grande}")).GetProperty("painel");
        daGrande.GetProperty("carteiras").GetInt32().Should().Be(1);
        daGrande.GetProperty("clientes").GetInt32().Should().Be(4);

        var daPequena = (await DadosAsync($"/api/v1/relatorios/cen?carteira={pequena}")).GetProperty("painel");
        daPequena.GetProperty("clientes").GetInt32().Should().Be(1);
        daPequena.GetProperty("porClasse").EnumerateArray().Select(c => c.GetProperty("classe").GetString())
            .Should().Equal("B");

        var todas = (await DadosAsync("/api/v1/relatorios/cen")).GetProperty("painel");
        todas.GetProperty("clientes").GetInt32().Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task Carteira_fora_do_recorte_devolve_o_painel_zerado_e_nao_erro()
    {
        await SemearAsync();

        var painel = (await DadosAsync($"/api/v1/relatorios/cen?carteira={Guid.NewGuid()}")).GetProperty("painel");

        painel.GetProperty("carteiras").GetInt32().Should().Be(0);
        painel.GetProperty("clientes").GetInt32().Should().Be(0);
    }
}
