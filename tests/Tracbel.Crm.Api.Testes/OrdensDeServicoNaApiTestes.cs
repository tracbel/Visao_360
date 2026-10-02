using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// AS ORDENS DE SERVIÇO NA FICHA DO CLIENTE E NA DA MÁQUINA (02/10/2026), por HTTP. O que estes testes prendem: o resumo
/// (em aberto, a faixa de mais de 45 dias, os doze meses de peças e serviços), a lista com as abertas primeiro, a OS de
/// outra filial que não aparece, o 404 de quem não está ao alcance, e o motivo quando a carga nunca rodou.
/// <b>Nomes, documentos e chassis inventados.</b>
/// </summary>
[Trait("Categoria", "OrdensDeServico")]
public sealed class OrdensDeServicoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const string ChassiDaMaquina = "1OSAPI6155M000001";
    private static readonly DateOnly Hoje = DateOnly.FromDateTime(DateTime.UtcNow);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private CrmDbContext AbrirBanco(IServiceScope escopo) =>
        new(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);

    private static DadosDaOrdemDeServico Ordem(
        int empresa, string numero, long? cliente, long? maquina, SituacaoDaOrdemDeServico situacao, DateOnly aberta, decimal pecas, decimal servicos) =>
        new(empresa, numero, cliente, maquina, ChassiDaMaquina, "6155M", 1500m, situacao, "OFICINA", aberta, null,
            situacao == SituacaoDaOrdemDeServico.Fechada ? aberta.AddDays(5) : null, null, pecas, servicos, 2, 1, $"h-{numero}");

    /// <summary>Semeia uma vez: o cliente com máquina e cinco OS, o cliente sem OS, e o carimbo da carga.</summary>
    private async Task<(Guid Cliente, Guid SemOrdem, Guid Maquina)> SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        await using var db = AbrirBanco(escopo);

        if (!await db.Sistemas.AnyAsync(s => s.Codigo == "PROTHEUS"))
        {
            var protheus = Sistema.Criar("PROTHEUS", "Protheus (TOTVS) — ERP", "teste");
            db.Sistemas.Add(protheus);
            var cliente = Cliente.Criar(1, "Cliente da oficina na API", TipoDePessoa.Juridica, 100, 100,
                documento: CpfCnpj.Criar("11444777000161"), situacao: SituacaoDoCliente.Cliente);
            var semOrdem = Cliente.Criar(1, "Cliente sem OS na API", TipoDePessoa.Juridica, 100, 100);
            db.Clientes.AddRange(cliente, semOrdem);
            var maquina = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(ChassiDaMaquina), OrigemDoEquipamento.Protheus, 100);
            db.Equipamentos.Add(maquina);
            await db.SaveChangesAsync();

            var lidaEm = DateTime.UtcNow;
            db.OrdensDeServico.AddRange(
                OrdemDeServico.Registrar(protheus.Id, "010101|1", Ordem(1, "1", cliente.Id, maquina.Id, SituacaoDaOrdemDeServico.Aberta, Hoje.AddDays(-60), 1000m, 500m), lidaEm, 100),
                OrdemDeServico.Registrar(protheus.Id, "010101|2", Ordem(1, "2", cliente.Id, maquina.Id, SituacaoDaOrdemDeServico.Liberada, Hoje.AddDays(-10), 200m, 100m), lidaEm, 100),
                OrdemDeServico.Registrar(protheus.Id, "010101|3", Ordem(1, "3", cliente.Id, null, SituacaoDaOrdemDeServico.Fechada, Hoje.AddMonths(-3), 300m, 150m), lidaEm, 100),
                OrdemDeServico.Registrar(protheus.Id, "010101|4", Ordem(1, "4", cliente.Id, null, SituacaoDaOrdemDeServico.Fechada, Hoje.AddMonths(-20), 9999m, 9999m), lidaEm, 100),
                // A OS DE BARRETOS não aparece para quem está em Ribeirão: a fronteira é a filial da OS.
                OrdemDeServico.Registrar(protheus.Id, "010113|5", Ordem(2, "5", cliente.Id, maquina.Id, SituacaoDaOrdemDeServico.Aberta, Hoje.AddDays(-5), 7777m, 0m), lidaEm, 100));
            db.PontosDeSincronismo.Add(PontoDeSincronismo.Criar(protheus.Id, OrdemDeServico.FluxoDaCarga, lidaEm.ToString("O")));
            await db.SaveChangesAsync();
        }

        return (
            await db.Clientes.Where(c => c.NomeRazao == "Cliente da oficina na API").Select(c => c.ChavePublica).SingleAsync(),
            await db.Clientes.Where(c => c.NomeRazao == "Cliente sem OS na API").Select(c => c.ChavePublica).SingleAsync(),
            (await db.Equipamentos.Select(e => new { e.ChavePublica, e.Chassi }).ToListAsync()).Single(e => e.Chassi.Numero == ChassiDaMaquina).ChavePublica);
    }

    [Fact]
    public async Task A_ficha_do_cliente_traz_as_abertas_primeiro_a_faixa_de_45_dias_e_os_doze_meses()
    {
        var (cliente, _, _) = await SemearAsync();

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{cliente}/ordens-de-servico"));

        dados.GetProperty("emAberto").GetInt32().Should().Be(2, "a de Barretos não está ao alcance");
        dados.GetProperty("emAbertoHaMaisDe45Dias").GetInt32().Should().Be(1);
        dados.GetProperty("diasDaMaisAntigaEmAberto").GetInt32().Should().Be(60);
        dados.GetProperty("valorEmAberto").GetDecimal().Should().Be(1800m);
        dados.GetProperty("nosUltimos12Meses").GetInt32().Should().Be(1, "a fechada de 20 meses atrás está fora dos doze");
        (dados.GetProperty("pecasNosUltimos12Meses").GetDecimal(), dados.GetProperty("servicosNosUltimos12Meses").GetDecimal()).Should().Be((300m, 150m));
        dados.GetProperty("totalDeOrdens").GetInt32().Should().Be(4);

        var numeros = dados.GetProperty("ordens").EnumerateArray().Select(o => o.GetProperty("ordem").GetProperty("numero").GetString()).ToList();
        numeros.Should().Equal("1", "2", "3", "4");
        dados.GetProperty("ordens")[0].GetProperty("diasEmAberto").GetInt32().Should().Be(60);
        dados.GetProperty("ordens")[2].GetProperty("diasEmAberto").ValueKind.Should().Be(JsonValueKind.Null, "a fechada não tem dias em aberto");
        dados.GetProperty("metricasSemDado").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task A_ficha_da_maquina_traz_as_OS_do_chassi()
    {
        var (_, _, maquina) = await SemearAsync();

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/equipamentos/{maquina}/ordens-de-servico"));

        dados.GetProperty("ordens").EnumerateArray().Select(o => o.GetProperty("ordem").GetProperty("numero").GetString())
            .Should().Equal("1", "2");
        dados.GetProperty("ordens")[0].GetProperty("ordem").GetProperty("horimetro").GetDecimal().Should().Be(1500m);
    }

    [Fact]
    public async Task O_cliente_sem_OS_recebe_o_motivo_e_quem_nao_existe_recebe_404()
    {
        var (_, semOrdem, _) = await SemearAsync();

        var dados = await DadosAsync(await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{semOrdem}/ordens-de-servico"));
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString())
            .Should().Equal("ordensDeServicoDoRecorte");

        (await api.ClienteDeRibeirao().GetAsync($"/api/v1/clientes/{Guid.NewGuid()}/ordens-de-servico")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await api.ClienteDeRibeirao().GetAsync($"/api/v1/equipamentos/{Guid.NewGuid()}/ordens-de-servico")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
