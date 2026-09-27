using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O ÍNDICE DE MOMENTO DA CANA PELA SOCICANA (27/09/2026): a CONAB só tem os meses que o CRM acumulou, e o índice pede 24.
/// O catálogo semeado aponta o índice da cana para o ATR MENSAL da Socicana — e o preço dela na Rentabilidade continua o
/// da CONAB.
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class IndiceDaCanaPelaSocicanaTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    [Fact]
    public async Task A_cana_mede_o_momento_pelo_ATR_mensal_e_o_acumulado_da_safra_nao_entra_na_janela()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);

        var cana = await db.Culturas.SingleAsync(c => c.Codigo == "CANA");
        cana.FonteDoPreco.Should().Be("CONAB", "o preço da Rentabilidade continua o da CONAB, em R$ por tonelada");
        (cana.FonteDoIndice, cana.ProdutoDoIndice, cana.NivelDoIndice).Should().Be(("SOCICANA", "ATR", "MENSAL"));

        // VINTE E QUATRO MESES DE ATR: R$ 1,00 nos doze antigos, R$ 1,10 nos doze recentes. O ACUMULADO DA SAFRA vem
        // junto, a R$ 5,00 — se entrasse na janela, o índice não seria 1,10. E a CONAB da cana só com doze meses: se ela
        // fosse a série do índice, o motivo seria "série curta".
        var agora = DateTime.UtcNow;
        var inicio = new DateOnly(2024, 9, 1);
        for (var m = 0; m < 24; m++)
        {
            var mes = inicio.AddMonths(m);
            db.CotacoesDeProdutos.Add(CotacaoDeProduto.Registrar(
                "SOCICANA", "ATR", "SP", "MENSAL", "CANA DE AÇÚCAR", "ATR", "kg de ATR", mes, m < 12 ? 1.00m : 1.10m, 100, agora));
            db.CotacoesDeProdutos.Add(CotacaoDeProduto.Registrar(
                "SOCICANA", "ATR", "SP", "ACUMULADO DA SAFRA", "CANA DE AÇÚCAR", "ATR", "kg de ATR", mes, 5.00m, 100, agora));
            if (m >= 12)
                db.CotacoesDeProdutos.Add(CotacaoDeProduto.Registrar(
                    "CONAB", "4238", "SP", "RECEBIDO PELO PRODUTOR", "CANA DE AÇÚCAR", "EM TONELADA", "kg", mes, 0.13m, 100, agora));
        }

        await db.SaveChangesAsync();

        var lido = await new RepositorioDeIndicadoresDeMercado(db).LerAsync(new DateOnly(2026, 9, 27), null, CancellationToken.None);

        var indice = lido.PrecoPorCultura["CANA"];
        indice.Motivo.Should().Be("Nenhum");
        indice.Indice.Should().Be(1.10m, "1,10 ÷ 1,00 — só o mensal");
    }

    [Fact]
    public void A_serie_do_indice_pede_fonte_e_produto_juntos_e_o_nivel_so_com_os_dois()
    {
        var cultura = Cultura.Registrar("TESTE", "Teste", SegmentoDaCultura.Graos, "saca de 60 kg", 60m);

        cultura.DefinirSerieDoIndice("socicana", "ATR", "MENSAL").Should().BeTrue();
        cultura.FonteDoIndice.Should().Be("SOCICANA");
        cultura.DefinirSerieDoIndice("SOCICANA", "ATR", "MENSAL").Should().BeFalse("nada mudou");

        var soFonte = () => cultura.DefinirSerieDoIndice("SOCICANA", null, null);
        var soNivel = () => cultura.DefinirSerieDoIndice(null, null, "MENSAL");
        soFonte.Should().Throw<RegraDeNegocioViolada>();
        soNivel.Should().Throw<RegraDeNegocioViolada>();

        cultura.DefinirSerieDoIndice(null, null, null).Should().BeTrue("os três vazios voltam à série do preço");
        cultura.FonteDoIndice.Should().BeNull();
    }
}
