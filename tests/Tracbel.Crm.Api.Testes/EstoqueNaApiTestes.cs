using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O ESTOQUE E A COBERTURA POR HTTP (28/09/2026), com a aplicação inteira de pé. Em Ribeirão: duas no pátio (uma
/// reservada, uma com mais de 180 dias), um pedido à fábrica e uma vendida (excluída); em Barretos, uma no pátio. A
/// cobertura: dois meses e um grupo. O que se prende: a filial do cabeçalho é a fronteira, e "Todas as filiais" soma;
/// os dias no pátio saem da data de entrada; a vendida não aparece; o que não foi lido diz que não foi lido.
/// </summary>
[Trait("Categoria", "Estoque")]
public sealed class EstoqueNaApiTestes
{
    private const string Rota = "/api/v1/relatorios/estoque";

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static IEnumerable<string?> Lacunas(JsonElement dados) =>
        dados.GetProperty("metricasSemDado").EnumerateArray().Select(m => m.GetProperty("metrica").GetString());

    private static DadosDoEquipamentoEmEstoque Maquina(int empresa, string situacao, string grupo, DateOnly? entrada, bool reservado = false, DateOnly? chegada = null) =>
        new(empresa, null, null, null, situacao, grupo, "TR 6155M", null, "MÁQUINA", false, "2026/2026", entrada, chegada, null, true, reservado, null,
            $"h-{empresa}-{situacao}-{grupo}-{entrada}-{reservado}");

    private static async Task<ApiEmMemoria> AppAsync(bool lido = true)
    {
        var app = new ApiEmMemoria();
        await app.InitializeAsync();
        if (!lido) return app;

        using var escopo = app.Services.CreateScope();
        await using var db = new CrmDbContext(escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>(), ProvedorDeContextoDeSistema.Instancia);
        var gn = Sistema.Criar(ConexoesDoSistema.GestaoDeNegocios, "Gestão de Negócios — API", "teste");
        db.Sistemas.Add(gn);
        await db.SaveChangesAsync();

        // O "HOJE" DO TESTE é o do relógio da aplicação: as datas de entrada são relativas a ele.
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));
        var lidaEm = DateTime.UtcNow;
        var vendida = EquipamentoEmEstoque.Registrar(gn.Id, "5", Maquina(1, "Estoque", "TRATOR 6000", hoje.AddDays(-10)), lidaEm, 100);
        vendida.Excluir(100);
        db.EquipamentosEmEstoque.AddRange(
            EquipamentoEmEstoque.Registrar(gn.Id, "1", Maquina(1, "Estoque", "TRATOR 6000", hoje.AddDays(-30)), lidaEm, 100),
            EquipamentoEmEstoque.Registrar(gn.Id, "2", Maquina(1, "Estoque", "TRATOR 6000", hoje.AddDays(-200), reservado: true), lidaEm, 100),
            EquipamentoEmEstoque.Registrar(gn.Id, "3", Maquina(1, "PEDIDO", "TRATOR 6000", null, chegada: hoje.AddDays(40)), lidaEm, 100),
            EquipamentoEmEstoque.Registrar(gn.Id, "4", Maquina(2, "Remessa", "AMS", hoje.AddDays(-5)), lidaEm, 100),
            vendida);

        db.CoberturasDoEstoque.AddRange(
            CoberturaDoEstoque.Registrar(gn.Id, CoberturaDoEstoque.RecortePorMes, "2026-07", new DateOnly(2026, 7, 1), 3.59m, 143, "c1", lidaEm, null, 100),
            CoberturaDoEstoque.Registrar(gn.Id, CoberturaDoEstoque.RecortePorMes, "2026-08", new DateOnly(2026, 8, 1), 3.12m, 152, "c2", lidaEm, null, 100),
            CoberturaDoEstoque.Registrar(gn.Id, CoberturaDoEstoque.RecortePorGrupo, "TRATOR 6000", null, 2.25m, 165, "c3", lidaEm, null, 100));

        db.PontosDeSincronismo.AddRange(
            PontoDeSincronismo.Criar(gn.Id, EquipamentoEmEstoque.FluxoDaCarga, "2026-09-28T11:02:00.0000000Z"),
            PontoDeSincronismo.Criar(gn.Id, CoberturaDoEstoque.FluxoDaCarga, "2026-09-28T11:02:00.0000000Z"));
        await db.SaveChangesAsync();
        return app;
    }

    [Fact]
    public async Task A_filial_do_cabecalho_ve_o_proprio_estoque_com_os_dias_no_patio()
    {
        await using var app = await AppAsync();

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota));

        dados.GetProperty("alcance").GetString().Should().Be("Filiais");
        var totais = dados.GetProperty("totais");
        (totais.GetProperty("noPatio").GetInt32(), totais.GetProperty("disponiveis").GetInt32(), totais.GetProperty("reservadas").GetInt32(),
                totais.GetProperty("maisDe180Dias").GetInt32(), totais.GetProperty("pedidosAFabrica").GetInt32())
            .Should().Be((2, 1, 1, 1, 1), "a de Barretos é de lá, e a vendida não aparece");

        var maquinas = dados.GetProperty("maquinas").EnumerateArray().ToList();
        maquinas.Should().HaveCount(3);
        maquinas.Select(m => m.GetProperty("diasNoPatio").ValueKind == JsonValueKind.Null ? (int?)null : m.GetProperty("diasNoPatio").GetInt32())
            .Should().BeEquivalentTo(new int?[] { 30, 200, null }, "o pedido à fábrica não está no pátio");
        dados.ToString().Should().NotContain("vl_custo").And.NotContain("custo\":");

        var grupo = dados.GetProperty("porGrupo").EnumerateArray().Single();
        (grupo.GetProperty("grupo").GetString(), grupo.GetProperty("noPatio").GetInt32(), grupo.GetProperty("pedidosAFabrica").GetInt32(),
                grupo.GetProperty("idadeMediaEmDias").GetInt32(), grupo.GetProperty("coberturaEmMeses").GetDecimal())
            .Should().Be(("TRATOR 6000", 2, 1, 115, 2.25m));

        var cobertura = dados.GetProperty("cobertura");
        cobertura.GetProperty("porMes").GetArrayLength().Should().Be(2);
        cobertura.GetProperty("mediaPorMes").GetDecimal().Should().Be(3.36m, "a média simples dos meses, como a GN mostra");
        Lacunas(dados).Should().Contain(["coberturaDaOrganizacao", "valor"]).And.NotContain("estoqueNaoLido");
    }

    [Fact]
    public async Task Em_todas_as_filiais_o_estoque_e_da_organizacao()
    {
        await using var app = await AppAsync();
        await app.ConcederPerfilAsync(100, PerfisDeSistema.Diretoria);

        var dados = await DadosAsync(await app.ClienteComo(ApiEmMemoria.UsuarioDeRibeirao, ContextoAcesso.CodigoDeTodasAsFiliais).GetAsync(Rota));

        dados.GetProperty("alcance").GetString().Should().Be("Organizacao");
        dados.GetProperty("totais").GetProperty("noPatio").GetInt32().Should().Be(3, "Ribeirão e Barretos");
        Lacunas(dados).Should().NotContain("coberturaDaOrganizacao");
    }

    [Fact]
    public async Task Sem_leitura_o_estoque_diz_que_nao_foi_lido_e_nao_que_nao_ha_maquina()
    {
        await using var app = await AppAsync(lido: false);

        var dados = await DadosAsync(await app.ClienteDeRibeirao().GetAsync(Rota));

        dados.GetProperty("maquinas").GetArrayLength().Should().Be(0);
        dados.GetProperty("lidoEm").ValueKind.Should().Be(JsonValueKind.Null);
        Lacunas(dados).Should().Contain(["estoqueNaoLido", "coberturaNaoLida"]);
    }

    [Fact]
    public void A_media_da_cobertura_e_nula_sem_item_e_a_idade_nao_fica_negativa()
    {
        var hoje = new DateOnly(2026, 9, 28);
        var lido = new EstoqueLido(
            [new MaquinaNoEstoque("Ribeirão", "AMS", "X", null, "Estoque", "AMS", false, null, null, hoje.AddDays(3), null, null, true, false, null)],
            [], [], null, null, null, null);

        var montado = ObterEstoqueECobertura.Montar(lido, hoje, organizacao: false);

        montado.Cobertura.MediaPorMes.Should().BeNull();
        montado.Maquinas.Single().DiasNoPatio.Should().Be(0, "entrada no futuro (fuso) não vira idade negativa");
        montado.PorGrupo.Single().CoberturaEmMeses.Should().BeNull("a GN não calculou o grupo");
    }
}
