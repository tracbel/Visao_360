using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Api.Testes;

/// <summary>
/// A REGRA GRAVADA PELA TELA CHEGA À DEMANDA NA HORA, mesmo com o potencial em cache (documento 54 §3.2): a gravação pela
/// API muda a versão do assunto, e a leitura seguinte refaz a conta.
///
/// <para><b>Classe própria, API e banco próprios</b>: a regra trocada mudaria a demanda que as outras classes conferem. O
/// cenário é o dos Cenários de mercado — café em 150, 300 e 450 ha, 10 ha por máquina e 5 anos: demanda 18.</para>
/// </summary>
[Trait("Categoria", "Territorio")]
public sealed class DemandaComARegraTrocadaNaApiTestes : IAsyncLifetime
{
    private const string Rota = "/api/v1/mercado/demanda";
    private const int Cafe = 40139;

    private readonly ApiEmMemoria _api = new();

    public Task InitializeAsync() => _api.InitializeAsync();

    public async Task DisposeAsync()
    {
        await ((IAsyncLifetime)_api).DisposeAsync();
        await _api.DisposeAsync();
    }

    private static async Task<decimal> DemandaEstruturalAsync(HttpClient http)
    {
        var resposta = await http.GetAsync(Rota);
        var corpo = await resposta.Content.ReadAsStringAsync();
        ((int)resposta.StatusCode).Should().Be(200, corpo);
        return JsonDocument.Parse(corpo).RootElement.GetProperty("dados").GetProperty("totais").GetProperty("demandaEstrutural").GetDecimal();
    }

    [Fact]
    public async Task A_regra_gravada_pela_tela_aparece_na_leitura_seguinte_mesmo_com_a_demanda_em_cache()
    {
        await CenarioDosCenarios.SemearAsync(_api);
        var http = _api.ClienteDeRibeirao();
        (await DemandaEstruturalAsync(http)).Should().Be(18m);

        // A VIGÊNCIA DE HOJE É TROCADA: o cadastro não aceita data no passado e a de hoje já está ocupada — revoga-se a de
        // 10 ha e registra-se outra, no mesmo dia, de 5 ha por máquina. O parque dobra, e a demanda também.
        var hoje = ParametroComVigencia.HojeNoBrasil(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var revogacao = await http.PostAsJsonAsync(
            $"/api/v1/admin/parametros-do-potencial/culturas/{Cafe}/TRATOR/{hoje}/revogacao",
            new { motivo = "teste do cache de referência" }, CenarioDosCenarios.Json);
        ((int)revogacao.StatusCode).Should().BeLessThan(300, await revogacao.Content.ReadAsStringAsync());

        var regra = await http.PostAsJsonAsync("/api/v1/admin/parametros-do-potencial/culturas", new
        {
            produtoCodigoIbge = Cafe.ToString(),
            hectaresPorMaquina = "5",
            anosDeRenovacao = "5",
            modeloDeReferencia = "3036N",
            situacao = "AConfirmar",
            vigenteDesde = hoje,
            justificativa = "teste do cache de referência",
            culturaCodigo = "CAFE",
            categoriaDeMaquinaCodigo = "TRATOR"
        }, CenarioDosCenarios.Json);
        ((int)regra.StatusCode).Should().BeLessThan(300, await regra.Content.ReadAsStringAsync());

        (await DemandaEstruturalAsync(http)).Should().Be(36m, "a gravação pela tela muda a versão do potencial na hora");
    }
}
