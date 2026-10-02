using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// AS PEÇAS DO CLIENTE NA FICHA (02/10/2026), por HTTP: os doze meses com balcão × oficina e o grupo, a série com o mês sem
/// compra em zero, o vendedor principal, os orçamentos em aberto (o vencido contado), a filial de fora que não aparece, e o
/// 404 de quem não está ao alcance. <b>Nomes e documentos inventados.</b>
/// </summary>
[Trait("Categoria", "Pecas")]
public sealed class PecasNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private static readonly DateOnly MesCorrente = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.UtcNow);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private CrmDbContext AbrirBanco(IServiceScope escopo) =>
        new(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);

    private static DadosDoFaturamentoDePecas Linha(int empresa, DateOnly mes, long cliente, string setor, string grupo, string vendedor, decimal valor) =>
        new(empresa, mes, cliente, setor, grupo, "PECAS JD", vendedor, $"VENDEDOR {vendedor}", 1m, valor, 0m, 0m, valor, 1);

    private static DadosDoOrcamentoDePecas Orcamento(int empresa, string numero, long cliente, string situacao, DateOnly validoAte, decimal valor) =>
        new(empresa, numero, cliente, situacao, null, null, "BALCAO", "VENDA", Hoje.AddDays(-30), validoAte, null, "000123", "VENDEDOR 000123", valor, 0m, 2, $"h-{numero}");

    private async Task<(Guid Cliente, Guid SemCompra)> SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        await using var db = AbrirBanco(escopo);

        if (!await db.Clientes.AnyAsync(c => c.NomeRazao == "Cliente de peças na API"))
        {
            var protheus = await db.Sistemas.FirstOrDefaultAsync(s => s.Codigo == "PROTHEUS") ?? Sistema.Criar("PROTHEUS", "Protheus (TOTVS) — ERP", "teste");
            if (protheus.Id == 0) db.Sistemas.Add(protheus);
            var cliente = Cliente.Criar(1, "Cliente de peças na API", TipoDePessoa.Juridica, 100, 100,
                documento: CpfCnpj.Criar("11444777000161"), situacao: SituacaoDoCliente.Cliente);
            var semCompra = Cliente.Criar(1, "Cliente sem peças na API", TipoDePessoa.Juridica, 100, 100);
            db.Clientes.AddRange(cliente, semCompra);
            await db.SaveChangesAsync();

            var agora = DateTime.UtcNow;
            db.FaturamentosDePecasNoMes.AddRange(
                FaturamentoDePecasNoMes.Apurar(protheus.Id, Linha(1, MesCorrente, cliente.Id, "BALCAO", "PECAS", "000123", 1000m), agora, 100),
                FaturamentoDePecasNoMes.Apurar(protheus.Id, Linha(1, MesCorrente.AddMonths(-2), cliente.Id, "OFICINA", "PNEU", "000456", 600m), agora, 100),
                FaturamentoDePecasNoMes.Apurar(protheus.Id, Linha(1, MesCorrente.AddMonths(-20), cliente.Id, "BALCAO", "PECAS", "000123", 5000m), agora, 100),
                // A NOTA DE BARRETOS não aparece para quem está em Ribeirão.
                FaturamentoDePecasNoMes.Apurar(protheus.Id, Linha(2, MesCorrente, cliente.Id, "BALCAO", "PECAS", "000999", 9999m), agora, 100));
            db.OrcamentosDePecas.AddRange(
                OrcamentoDePecas.Registrar(protheus.Id, "01|1", Orcamento(1, "1", cliente.Id, "Aberto", Hoje.AddDays(10), 300m), agora, 100),
                OrcamentoDePecas.Registrar(protheus.Id, "01|2", Orcamento(1, "2", cliente.Id, "Parcialmente Atendido", Hoje.AddDays(-5), 200m), agora, 100),
                OrcamentoDePecas.Registrar(protheus.Id, "01|3", Orcamento(1, "3", cliente.Id, "Encerrado", Hoje.AddDays(-40), 999m), agora, 100));
            await db.SaveChangesAsync();
            db.PontosDeSincronismo.AddRange(
                PontoDeSincronismo.Criar(protheus.Id, FaturamentoDePecasNoMes.FluxoDaCarga, agora.ToString("O")),
                PontoDeSincronismo.Criar(protheus.Id, OrcamentoDePecas.FluxoDaCarga, agora.ToString("O")));
            await db.SaveChangesAsync();
        }

        return (
            await db.Clientes.Where(c => c.NomeRazao == "Cliente de peças na API").Select(c => c.ChavePublica).SingleAsync(),
            await db.Clientes.Where(c => c.NomeRazao == "Cliente sem peças na API").Select(c => c.ChavePublica).SingleAsync());
    }

    [Fact]
    public async Task A_ficha_traz_os_doze_meses_por_setor_e_grupo_o_vendedor_e_os_orcamentos_em_aberto()
    {
        var (cliente, _) = await SemearAsync();

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{cliente}/pecas"));

        dados.GetProperty("dozeMeses").GetDecimal().Should().Be(1600m, "a nota de 20 meses atrás fica fora e a de Barretos não está ao alcance");
        dados.GetProperty("porSetor").EnumerateArray().Select(f => f.GetProperty("nome").GetString()).Should().Equal("BALCAO", "OFICINA");
        dados.GetProperty("porGrupo").EnumerateArray().Select(f => f.GetProperty("nome").GetString()).Should().Equal("PECAS", "PNEU");
        dados.GetProperty("serie").GetArrayLength().Should().Be(12);
        dados.GetProperty("serie")[10].GetProperty("valor").GetDecimal().Should().Be(0m, "o mês sem compra entra com zero");
        dados.GetProperty("vendedorPrincipal").GetString().Should().Be("VENDEDOR 000123");
        dados.GetProperty("ultimaCompraEm").GetString().Should().Be(MesCorrente.ToString("yyyy-MM-dd"));

        dados.GetProperty("orcamentosEmAberto").GetInt32().Should().Be(2, "o encerrado não espera o cliente");
        dados.GetProperty("valorEmOrcamentosAbertos").GetDecimal().Should().Be(500m);
        dados.GetProperty("orcamentosVencidos").GetInt32().Should().Be(1);
        dados.GetProperty("orcamentos").EnumerateArray().Select(o => o.GetProperty("numero").GetString()).Should().Equal("2", "1");
        dados.GetProperty("metricasSemDado").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task O_cliente_sem_compra_recebe_o_motivo_e_quem_nao_existe_recebe_404()
    {
        var (_, semCompra) = await SemearAsync();

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{semCompra}/pecas"));
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString()).Should().Equal("pecasDoCliente");
        dados.GetProperty("ultimaCompraEm").ValueKind.Should().Be(JsonValueKind.Null);

        (await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{Guid.NewGuid()}/pecas")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
