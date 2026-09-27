using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Tracbel.Crm.Infraestrutura.Identidade;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// O MERCADO ANUAL EM REAIS PELA API (issue 70, 27/09/2026). Com o preço da máquina gravado pela rotina do #251, a
/// tela de Indicadores Geográficos deixa de mostrar "—": o mercado anual é a demanda de cada categoria vezes o preço
/// de referência dela — a mediana das medianas mensais dos últimos 12 meses —, na página e em cada município.
///
/// <para><b>Classe própria, banco próprio</b>: o preço gravado aqui mudaria o "sem preço" que as outras classes de
/// território conferem.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class MercadoAnualNaApiTestes(ApiEmMemoria api) : IClassFixture<ApiEmMemoria>
{
    private const int Cafe = 40139;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly (int Codigo, string Nome, decimal Hectares)[] MunicipiosDaAdr =
    [
        (3598001, "Município de Teste G", 150m), (3598002, "Município de Teste H", 300m), (3598003, "Município de Teste I", 450m)
    ];

    private static DateOnly Hoje => ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow);

    private static DateOnly MesCorrente => new(Hoje.Year, Hoje.Month, 1);

    private static async Task<JsonElement> DadosAsync(HttpResponseMessage resposta)
    {
        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.OK, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").Clone();
    }

    /// <summary>
    /// A ADR com café, uma regra do trator COM ciclo a partir de hoje (a semente não tem anos de renovação) e o preço
    /// do trator em três meses: dois dentro da janela de 12 meses e um fora dela.
    /// </summary>
    private async Task<HttpClient> AdministradorComPrecoAsync()
    {
        await api.ConcederPerfilAsync(100, PerfisDeSistema.Administrador);
        var http = api.ClienteDeRibeirao();

        using (var escopo = api.Services.CreateScope())
        {
            var opcoes = escopo.ServiceProvider.GetRequiredService<DbContextOptions<CrmDbContext>>();
            await using var db = new CrmDbContext(opcoes, ProvedorDeContextoDeSistema.Instancia);
            if (await db.MunicipiosDaAreaDeAtuacao.AnyAsync()) return http;

            var agora = DateTime.UtcNow;
            var municipios = MunicipiosDaAdr.Select(m => Municipio.Criar(m.Nome, "SP", m.Codigo)).ToList();
            db.Municipios.AddRange(municipios);
            await db.SaveChangesAsync();

            foreach (var (municipio, (_, _, hectares)) in municipios.Zip(MunicipiosDaAdr))
            {
                db.MunicipiosDaAreaDeAtuacao.Add(MunicipioDaAreaDeAtuacao.Registrar(
                    municipio.Id, true, RegiaoDaAreaDeAtuacao.Norte, 1, "Area de Atuação.xlsx", 2, 100, agora));
                db.ProducoesAgricolasNosMunicipios.Add(ProducaoAgricolaNoMunicipio.Registrar(
                    municipio.Id, 2024, Cafe, "Café (em grão) Total", new(hectares, hectares, hectares * 2, hectares * 10), 100, agora));
            }

            var trator = await db.CategoriasDeMaquina.SingleAsync(c => c.Codigo == "TRATOR");
            db.PrecosDeMaquina.AddRange(
                PrecoDeMaquinaNoMes.Registrar(trator.Id, MesCorrente, 500_000m, 450_000m, 550_000m, 3, PrecoDeMaquinaNoMes.FonteDaNota, 100, agora),
                PrecoDeMaquinaNoMes.Registrar(trator.Id, MesCorrente.AddMonths(-2), 450_000m, 450_000m, 450_000m, 1, PrecoDeMaquinaNoMes.FonteDaNota, 100, agora),
                // FORA DA JANELA: treze meses atrás. Se entrasse, a mediana dos três seria R$ 450 mil.
                PrecoDeMaquinaNoMes.Registrar(trator.Id, MesCorrente.AddMonths(-13), 100_000m, 100_000m, 100_000m, 1, PrecoDeMaquinaNoMes.FonteDaNota, 100, agora));

            await db.SaveChangesAsync();
        }

        var regra = await http.PostAsJsonAsync("/api/v1/admin/parametros-do-potencial/culturas", new
        {
            produtoCodigoIbge = Cafe.ToString(),
            hectaresPorMaquina = "10",
            anosDeRenovacao = "5",
            modeloDeReferencia = "3036N",
            situacao = "AConfirmar",
            vigenteDesde = Hoje.ToString("yyyy-MM-dd"),
            justificativa = "regra com ciclo, para a demanda anual sair",
            culturaCodigo = "CAFE",
            categoriaDeMaquinaCodigo = "TRATOR"
        }, Json);
        regra.StatusCode.Should().Be(HttpStatusCode.Created, await regra.Content.ReadAsStringAsync());

        return http;
    }

    /// <summary>A mediana dos dois meses da janela (R$ 500 mil e R$ 450 mil): a média dos dois do meio.</summary>
    private const decimal PrecoDoTrator = 475_000m;

    [Fact]
    public async Task O_mercado_anual_sai_em_reais_na_pagina_e_em_cada_municipio_com_o_preco_escrito()
    {
        var http = await AdministradorComPrecoAsync();

        var dados = await DadosAsync(await http.GetAsync("/api/v1/territorio/indicadores"));
        var numeros = dados.GetProperty("numerosDeDecisao");

        // A PÁGINA: só o trator tem regra, então a demanda inteira é dele.
        var demanda = numeros.GetProperty("demandaAnual").GetProperty("valor").GetDecimal();
        var mercado = numeros.GetProperty("mercadoAnual");
        mercado.GetProperty("valor").GetDecimal().Should().BeApproximately(demanda * PrecoDoTrator, 0.01m,
            "é a demanda da categoria vezes o preço dela — o mês de treze meses atrás não entra");
        mercado.GetProperty("parcial").GetBoolean().Should().BeFalse();

        var preco = mercado.GetProperty("precos").EnumerateArray().Single();
        preco.GetProperty("categoriaCodigo").GetString().Should().Be("TRATOR");
        preco.GetProperty("preco").GetDecimal().Should().Be(PrecoDoTrator);
        preco.GetProperty("meses").GetInt32().Should().Be(2);
        preco.GetProperty("ultimoMes").GetString().Should().Be(MesCorrente.ToString("yyyy-MM-dd"));

        // CADA MUNICÍPIO: a demanda dele vezes o mesmo preço.
        var daAdr = dados.GetProperty("indicadores").GetProperty("municipios").EnumerateArray()
            .Where(m => m.GetProperty("pertenceAAdr").GetBoolean())
            .ToList();
        daAdr.Should().HaveCount(MunicipiosDaAdr.Length);
        foreach (var municipio in daAdr)
        {
            var doMunicipio = municipio.GetProperty("potencialEstrutural").GetProperty("demandaAnualDeMaquinas").GetDecimal();
            municipio.GetProperty("numerosDeDecisao").GetProperty("mercadoAnual").GetProperty("valor").GetDecimal()
                .Should().BeApproximately(doMunicipio * PrecoDoTrator, 0.01m);
        }

        // O POTENCIAL INCREMENTAL precisa das vendas em unidades, e este cenário não tem venda do ART: ele diz isso,
        // em vez de tratar a oportunidade inteira como não capturada.
        numeros.GetProperty("potencialIncremental").GetProperty("valor").ValueKind.Should().Be(JsonValueKind.Null);
        numeros.GetProperty("potencialIncremental").GetProperty("motivo").GetString().Should().Be("SemVendasEmUnidades");
    }
}
