using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Processo;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A SETA DE CADA FASE DO PIPELINE NA MAQUETE (30/09/2026): a lista filtrada por fluxo e fase, com o total da fase.
///
/// <para>Era a dívida P-7: o caso de uso filtrava o código da fase e o do fluxo sobre a página que o banco já tinha
/// paginado, e a resposta trazia menos linhas que o tamanho pedido com o total da lista inteira. Agora os dois códigos
/// vão ao banco, antes da paginação.</para>
/// </summary>
[Trait("Categoria", "Relacionamento")]
public sealed class PipelineDaMaqueteTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int NaApresentacaoDaVenda = 30;
    private const int NaNegociacaoDaVenda = 2;
    private const int NaApresentacaoDoPosVenda = 1;

    /// <summary>
    /// Dois fluxos, e a fase <c>PIPE_APRES</c> nos dois — o código de fase é único só dentro do fluxo. Os processos da
    /// apresentação da venda são a maioria, para uma página de 5 cair inteira dentro deles.
    /// </summary>
    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        if (await db.TiposDeProcesso.AnyAsync(t => t.Codigo == "PIPE_VENDA")) return;

        var venda = TipoProcesso.Criar("PIPE_VENDA", "Venda do Pipeline");
        var posVenda = TipoProcesso.Criar("PIPE_POS", "Pós-venda do Pipeline");
        db.TiposDeProcesso.AddRange(venda, posVenda);
        await db.SaveChangesAsync();

        var apresentacao = Fase.Criar(venda.Id, "PIPE_APRES", "Apresentação", ordem: 1);
        var negociacao = Fase.Criar(venda.Id, "PIPE_NEGOC", "Negociação", ordem: 2);
        var apresentacaoDoPos = Fase.Criar(posVenda.Id, "PIPE_APRES", "Apresentação", ordem: 1);
        db.Fases.AddRange(apresentacao, negociacao, apresentacaoDoPos);

        var cliente = Cliente.Criar(1, "Fazenda do Pipeline Ltda", TipoDePessoa.Juridica, 100, 100);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        var numero = 90_000L;
        void Abrir(TipoProcesso tipo, Fase fase, int quantos)
        {
            for (var i = 0; i < quantos; i++)
            {
                numero++;
                db.Processos.Add(Dominio.Processo.Processo.Abrir(
                    numero: numero,
                    empresaId: 1,
                    tipoProcessoId: tipo.Id,
                    clienteId: cliente.Id,
                    faseId: fase.Id,
                    titulo: $"Processo do pipeline {numero}",
                    proprietarioId: 100,
                    criadoPorId: 100,
                    abertoEmUtc: new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc).AddHours(numero % 50)));
            }
        }

        Abrir(venda, apresentacao, NaApresentacaoDaVenda);
        Abrir(venda, negociacao, NaNegociacaoDaVenda);
        Abrir(posVenda, apresentacaoDoPos, NaApresentacaoDoPosVenda);
        await db.SaveChangesAsync();
    }

    private async Task<JsonElement> PaginaAsync(string consulta)
    {
        var resposta = await api.ClienteDeRibeirao().GetAsync($"/api/v1/processos?{consulta}");
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement.GetProperty("dados").Clone();
    }

    private static List<string> Campo(JsonElement pagina, string campo) =>
        pagina.GetProperty("itens").EnumerateArray().Select(i => i.GetProperty(campo).GetString()!).ToList();

    [Fact]
    public async Task A_fase_com_o_fluxo_devolve_a_pagina_cheia_e_o_total_da_fase()
    {
        await SemearAsync();

        var pagina = await PaginaAsync("tipoProcessoCodigo=PIPE_VENDA&faseCodigo=PIPE_APRES&tamanho=5");

        pagina.GetProperty("total").GetInt32().Should().Be(NaApresentacaoDaVenda);
        Campo(pagina, "faseCodigo").Should().HaveCount(5).And.OnlyContain(c => c == "PIPE_APRES");
        Campo(pagina, "tipoProcessoCodigo").Should().OnlyContain(c => c == "PIPE_VENDA");
    }

    [Fact]
    public async Task A_fase_pequena_nao_herda_o_total_da_lista_inteira()
    {
        await SemearAsync();

        // ERA O DEFEITO: com tamanho 1, a página vinha vazia ou com uma linha e o total dizia a lista inteira.
        var pagina = await PaginaAsync("tipoProcessoCodigo=PIPE_VENDA&faseCodigo=PIPE_NEGOC&tamanho=1");

        pagina.GetProperty("total").GetInt32().Should().Be(NaNegociacaoDaVenda);
        Campo(pagina, "faseCodigo").Should().ContainSingle().Which.Should().Be("PIPE_NEGOC");
    }

    [Fact]
    public async Task O_codigo_da_fase_sozinho_vale_para_os_dois_fluxos_e_o_do_fluxo_sozinho_para_as_fases_dele()
    {
        await SemearAsync();

        (await PaginaAsync("faseCodigo=PIPE_APRES&tamanho=1")).GetProperty("total").GetInt32()
            .Should().Be(NaApresentacaoDaVenda + NaApresentacaoDoPosVenda);

        (await PaginaAsync("tipoProcessoCodigo=PIPE_POS&tamanho=50")).GetProperty("total").GetInt32()
            .Should().Be(NaApresentacaoDoPosVenda);

        (await PaginaAsync("tipoProcessoCodigo=PIPE_VENDA&tamanho=1")).GetProperty("total").GetInt32()
            .Should().Be(NaApresentacaoDaVenda + NaNegociacaoDaVenda);
    }

    [Fact]
    public async Task Codigo_que_nao_existe_devolve_lista_vazia_e_total_zero()
    {
        await SemearAsync();

        var pagina = await PaginaAsync("faseCodigo=NAO_EXISTE&tamanho=25");

        pagina.GetProperty("total").GetInt32().Should().Be(0);
        pagina.GetProperty("itens").GetArrayLength().Should().Be(0);
    }
}
