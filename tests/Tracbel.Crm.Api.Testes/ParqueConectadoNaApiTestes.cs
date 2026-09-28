using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// AS MÁQUINAS CONECTADAS NA FICHA DO MUNICÍPIO, POR HTTP (telemetria do Operations Center, 28/09/2026): a máquina entra
/// no município onde a última posição cai, e o "sem uso há 30 dias" é contado a partir da leitura mais nova.
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class ParqueConectadoNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int RibeiraoPreto = 3543402;
    private const int Serrana = 3551504;
    private const string Periodo = "/api/v1/territorio/indicadores?competenciaInicial=2026-01&competenciaFinal=2026-06";

    private static readonly DateTime Dia25 = new(2026, 9, 25, 10, 0, 0, DateTimeKind.Utc);

    private async Task SemearAsync()
    {
        using var escopo = api.Services.CreateScope();
        var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
        await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
        if (await db.MunicipiosDaAreaDeAtuacao.AnyAsync()) return;

        var ribeirao = Municipio.Criar("Ribeirão Preto", "SP", RibeiraoPreto);
        var serrana = Municipio.Criar("Serrana", "SP", Serrana);
        db.Municipios.AddRange(ribeirao, serrana);
        await db.SaveChangesAsync();

        foreach (var (municipio, linha) in new[] { (ribeirao, 2), (serrana, 3) })
            db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", linha, 100, DateTime.UtcNow));

        // TRÊS MÁQUINAS EM RIBEIRÃO: uma trabalhando, uma parada há 40 dias e uma sem horímetro. Serrana não tem nenhuma.
        Equipamento Conectada(string chassi, decimal? horas, DateTime? horasEm)
        {
            var maquina = Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(chassi), OrigemDoEquipamento.Protheus, 100);
            if (horas is { } h) maquina.RegistrarHorimetro(h, horasEm!.Value);
            maquina.RegistrarPosicao(Coordenada.Criar(-21.17, -47.81), Dia25, ribeirao.Id);
            return maquina;
        }

        db.Equipamentos.AddRange(
            Conectada("1RW6110JCMR900001", 1200m, Dia25),
            Conectada("1RW6110JCMR900002", 5000m, Dia25.AddDays(-40)),
            Conectada("1RW6110JCMR900003", null, null));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task A_ficha_do_municipio_conta_as_conectadas_pela_posicao_e_o_sem_uso_pela_leitura_mais_nova()
    {
        await SemearAsync();

        var resposta = await api.ClienteDeRibeirao().GetAsync(Periodo);
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, await resposta.Content.ReadAsStringAsync());
        var municipios = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync()).RootElement
            .GetProperty("dados").GetProperty("indicadores").GetProperty("municipios").Clone();

        JsonElement Municipio(int codigo) => municipios.EnumerateArray().Single(m => m.GetProperty("codigoIbge").GetInt32() == codigo);

        var parque = Municipio(RibeiraoPreto).GetProperty("parqueConectado");
        parque.GetProperty("maquinas").GetInt32().Should().Be(3);
        parque.GetProperty("comHorimetro").GetInt32().Should().Be(2);
        parque.GetProperty("semUsoHa30Dias").GetInt32().Should().Be(1, "a que ficou 40 dias sem hora nova");
        parque.GetProperty("horimetroMediano").GetDecimal().Should().Be(3100m, "(1.200 + 5.000) / 2");
        parque.GetProperty("referencia").GetDateTime().Should().Be(Dia25);

        Municipio(Serrana).GetProperty("parqueConectado").ValueKind.Should().Be(JsonValueKind.Null, "nenhuma máquina conectada ali");
    }
}
