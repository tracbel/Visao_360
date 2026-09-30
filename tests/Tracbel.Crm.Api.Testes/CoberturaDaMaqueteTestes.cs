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
/// A LISTA DA COBERTURA NA MAQUETE (30/09/2026): a caixa de busca e a coluna de prioridade.
///
/// <para>A prioridade é a CURVA ABC DO CLIENTE (decisão do Ricardo): A é alta, B é média, C, D e o cliente sem classe são
/// baixa — e não a classe do vínculo, que entra como C por assunção. A busca acha pelo cliente, pela carteira (nome ou
/// código) e pelo responsável.</para>
/// </summary>
[Trait("Categoria", "Relacionamento")]
public sealed class CoberturaDaMaqueteTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string Rota = "/api/v1/cobertura";

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.Carteiras.AnyAsync(c => c.Codigo == "BUSCA_RP")) return;

        var linha = LinhaDeNegocio.Criar("MAQ_BUSCA", "Máquinas da busca");
        db.LinhasDeNegocio.Add(linha);
        await db.SaveChangesAsync();

        var carteira = Carteira.Criar(1, linha.Id, "BUSCA_RP", "Carteira da Busca", 100, 100);
        db.Carteiras.Add(carteira);

        var grande = Cliente.Criar(1, "Fazenda Grande Ltda", TipoDePessoa.Juridica, 100, 100);
        var media = Cliente.Criar(1, "Sitio Medio Ltda", TipoDePessoa.Juridica, 100, 100);
        var pequena = Cliente.Criar(1, "Chacara Pequena Ltda", TipoDePessoa.Juridica, 100, 100);
        var semClasse = Cliente.Criar(1, "Rancho Sem Faturamento", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.AddRange(grande, media, pequena, semClasse);
        await db.SaveChangesAsync();

        grande.ApurarClasse(ClasseDeCliente.A, 1_000_000m, DateTime.UtcNow, 100);
        media.ApurarClasse(ClasseDeCliente.B, 100_000m, DateTime.UtcNow, 100);
        pequena.ApurarClasse(ClasseDeCliente.D, 100m, DateTime.UtcNow, 100);

        // A CLASSE DO VÍNCULO É C EM TODOS, como entra do legado: ela não pode decidir a prioridade.
        foreach (var cliente in new[] { grande, media, pequena, semClasse })
            db.ClienteCarteiras.Add(ClienteCarteira.Criar(cliente.Id, carteira.Id, ClasseDeCliente.C, 100));

        await db.SaveChangesAsync();
    }

    private async Task<List<JsonElement>> ItensAsync(string termo)
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync($"{Rota}?termo={Uri.EscapeDataString(termo)}&tamanho=50");
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        var corpo = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.Clone();
        return corpo.GetProperty("dados").GetProperty("itens").EnumerateArray().ToList();
    }

    private static string? Texto(JsonElement item, string campo) =>
        item.GetProperty(campo).ValueKind == JsonValueKind.Null ? null : item.GetProperty(campo).GetString();

    [Theory]
    [InlineData("Fazenda Grande", "A", "Alta")]
    [InlineData("Sitio Medio", "B", "Media")]
    [InlineData("Chacara Pequena", "D", "Baixa")]
    [InlineData("Rancho Sem Faturamento", null, "Baixa")]
    public async Task A_prioridade_e_a_curva_ABC_do_cliente_e_nao_a_classe_do_vinculo(string cliente, string? classe, string prioridade)
    {
        await SemearAsync();

        var itens = await ItensAsync(cliente);

        itens.Should().ContainSingle();
        Texto(itens[0], "classeDoCliente").Should().Be(classe);
        Texto(itens[0], "prioridade").Should().Be(prioridade);
        Texto(itens[0], "classe").Should().Be("C", "a classe do vínculo continua a do legado");
    }

    [Theory]
    [InlineData("BUSCA_RP")]
    [InlineData("Carteira da Busca")]
    public async Task A_busca_acha_pela_carteira_pelo_nome_ou_pelo_codigo(string busca)
    {
        await SemearAsync();

        (await ItensAsync(busca)).Select(i => Texto(i, "clienteNome")).Should().BeEquivalentTo(
            "Fazenda Grande Ltda", "Sitio Medio Ltda", "Chacara Pequena Ltda", "Rancho Sem Faturamento");
    }

    [Fact]
    public async Task A_busca_acha_pelo_responsavel_da_carteira()
    {
        await SemearAsync();

        // O RESPONSÁVEL DA CARTEIRA DA BUSCA é o usuário 100, que se exibe como "cen.ribeiraopreto".
        (await ItensAsync("cen.ribeiraopreto")).Select(i => Texto(i, "carteiraNome")).Should().Contain("Carteira da Busca");
    }

    [Fact]
    public async Task Sem_busca_a_lista_e_a_de_sempre_e_a_busca_sem_nada_nao_acha_nada()
    {
        await SemearAsync();

        var todas = await api.ClienteDeRibeirao().GetAsync($"{Rota}?tamanho=50");
        todas.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonDocument.Parse(await todas.Content.ReadAsStringAsync()).RootElement
            .GetProperty("dados").GetProperty("total").GetInt32().Should().BeGreaterThanOrEqualTo(4);

        (await ItensAsync("Nenhum Cliente Com Este Nome")).Should().BeEmpty();
    }
}
