using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Infraestrutura.Persistencia.Cache;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O CACHE DE REFERÊNCIA (documento 54 §3.2): a conta é feita uma vez por versão do assunto, só a versão nova a refaz, a
/// conta que falha não fica guardada, e a versão muda quando o dado muda — pela rotina, em outro processo, ou pela tela.
/// </summary>
[Trait("Categoria", "Api")]
public sealed class CacheDeReferenciaTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private sealed class AssinaturaDeMentira : IAssinaturaDosAssuntos
    {
        public string Versao { get; set; } = "v1";

        public Task<string> LerAsync(AssuntoDeReferencia assunto, CancellationToken ct) => Task.FromResult(Versao);
    }

    [Fact]
    public async Task A_conta_e_feita_uma_vez_por_versao_e_a_versao_nova_a_refaz()
    {
        var assinatura = new AssinaturaDeMentira();
        var cache = new CacheDeReferencia(new MemoryCache(new MemoryCacheOptions()), assinatura);
        var contas = 0;
        Task<string> Calcular()
        {
            contas++;
            return Task.FromResult($"resultado {contas}");
        }

        (await cache.ObterAsync(AssuntoDeReferencia.Potencial, "x", Calcular, default)).Should().Be("resultado 1");
        (await cache.ObterAsync(AssuntoDeReferencia.Potencial, "x", Calcular, default)).Should().Be("resultado 1");
        assinatura.Versao = "v2";
        (await cache.ObterAsync(AssuntoDeReferencia.Potencial, "x", Calcular, default)).Should().Be("resultado 2");
    }

    [Fact]
    public async Task A_conta_que_falha_nao_fica_guardada_e_a_proxima_leitura_recalcula()
    {
        var cache = new CacheDeReferencia(new MemoryCache(new MemoryCacheOptions()), new AssinaturaDeMentira());
        var primeira = true;
        Task<string> Calcular()
        {
            if (primeira)
            {
                primeira = false;
                throw new InvalidOperationException("banco fora do ar");
            }

            return Task.FromResult("ok");
        }

        var falha = () => cache.ObterAsync(AssuntoDeReferencia.Territorio, "y", Calcular, default);
        await falha.Should().ThrowAsync<InvalidOperationException>();
        (await cache.ObterAsync(AssuntoDeReferencia.Territorio, "y", Calcular, default)).Should().Be("ok");
    }

    [Fact]
    public async Task A_pam_gravada_por_outro_processo_muda_a_assinatura_do_potencial()
    {
        var assinatura = api.Services.GetRequiredService<IAssinaturaDosAssuntos>();
        var antes = await assinatura.LerAsync(AssuntoDeReferencia.Potencial, default);

        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            var municipio = Municipio.Criar("Município da Assinatura", "SP", 3596001);
            db.Municipios.Add(municipio);
            await db.SaveChangesAsync();
            db.ProducoesAgricolasNosMunicipios.Add(ProducaoAgricolaNoMunicipio.Registrar(
                municipio.Id, 2024, 40139, "Café (em grão) Total", new(10m, 10m, 20m, 100m), 100, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        (await assinatura.LerAsync(AssuntoDeReferencia.Potencial, default)).Should().NotBe(antes, "a rotina gravou PAM nova");
    }

    [Fact]
    public async Task A_gravacao_pela_tela_das_configuracoes_do_potencial_muda_a_assinatura_e_as_outras_nao()
    {
        var assinatura = api.Services.GetRequiredService<IAssinaturaDosAssuntos>();
        var contador = api.Services.GetRequiredService<ContadorDeGravacoesNaReferencia>();
        var antes = await assinatura.LerAsync(AssuntoDeReferencia.Potencial, default);

        contador.RegistrarGravacao("/api/v1/mercado/cenarios/3597001");
        (await assinatura.LerAsync(AssuntoDeReferencia.Potencial, default)).Should().Be(antes, "a meta do município não muda o potencial");

        contador.RegistrarGravacao("/api/v1/admin/parametros-do-potencial/culturas");
        (await assinatura.LerAsync(AssuntoDeReferencia.Potencial, default)).Should().NotBe(antes);
    }

    [Fact]
    public async Task O_territorio_vem_do_cache_na_segunda_leitura_e_muda_quando_a_area_de_atuacao_muda()
    {
        await CenarioDosCenarios.SemearAsync(api);
        var leitor = api.Services.GetRequiredService<Dominio.Portas.IRepositorioDoTerritorioDeReferencia>();

        var primeira = await leitor.LerAsync(default);
        primeira.Area[CenarioDosCenarios.C3].PertenceAAdr.Should().BeTrue();
        (await leitor.LerAsync(default)).Should().BeSameAs(primeira, "a segunda leitura é a mesma conta guardada");

        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            var municipio = Municipio.Criar("Município novo da ADR", "SP", 3596002);
            db.Municipios.Add(municipio);
            await db.SaveChangesAsync();
            db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 99, 100, DateTime.UtcNow));
            await db.SaveChangesAsync();
        }

        (await leitor.LerAsync(default)).Area.Should().ContainKey(3596002, "a rotina gravou um município novo na área");
    }
}
