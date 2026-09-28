using System.Text.Json;
using FluentAssertions;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.GestaoDeNegocios;

/// <summary>
/// A LEITURA DO GABARITO DA CONFERÊNCIA (28/09/2026): a performance-maquinas da GN, com a forma conferida na API real.
/// Chassis inventados; o cliente e o vendedor da amostra existem só para provar que não são lidos.
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class ConferenciaDaGestaoDeNegociosTestes
{
    private const string Linhas = """
        [
          {"tipo":"Realizado","mes_rotulo":"Ago/2026","filial":"Ribeirão Preto","chassi":"1PY6155MXSS000001","qt":1,
           "cliente":"NOME DO CLIENTE","vendedor":"FULANO.DE.TAL","gestor":"GESTOR.NORTE","valor":650000,"lucro":42000,"dt_entrega":"2026-08-20"},
          {"tipo":"Meta","mes_rotulo":"Ago/2026","filial":"Ribeirão Preto","chassi":null,"qt":3,"vendedor":"FULANO.DE.TAL"}
        ]
        """;

    private const string Filiais = """{"filiais":[{"numero":1,"nome":"Ribeirão Preto","codigo_totvs":"010101"}]}""";

    private static List<JsonElement> Json(string texto) => [.. JsonDocument.Parse(texto).RootElement.EnumerateArray()];

    [Fact]
    public void O_realizado_vira_maquina_e_a_meta_vira_PO_sem_nome_nem_valor()
    {
        var convertido = LeitorDaConferenciaDaGestaoDeNegocios.Converter(Json(Linhas), JsonDocument.Parse(Filiais).RootElement);

        convertido.EhSucesso.Should().BeTrue(convertido.Erro);
        convertido.Valor.Realizado.Should().Equal(new RealizadoNaGestao("1PY6155MXSS000001", "Ribeirão Preto", "Ago/2026", "1"));
        convertido.Valor.Meta.Should().Equal(new MetaNaGestao("Ribeirão Preto", "Ago/2026", "3"));
        JsonSerializer.Serialize(convertido.Valor).Should().NotContain("NOME DO CLIENTE").And.NotContain("FULANO").And.NotContain("42000");
    }

    [Fact]
    public void Tipo_desconhecido_ou_campo_ausente_recusa_a_leitura_com_os_nomes_dos_campos()
    {
        var tipo = LeitorDaConferenciaDaGestaoDeNegocios.Converter(
            Json("""[{"tipo":"Previsão","mes_rotulo":"Ago/2026","filial":"X","qt":1,"cliente":"NOME"}]"""), JsonDocument.Parse(Filiais).RootElement);
        var semChassi = LeitorDaConferenciaDaGestaoDeNegocios.Converter(
            Json("""[{"tipo":"Realizado","mes_rotulo":"Ago/2026","filial":"X","qt":1}]"""), JsonDocument.Parse(Filiais).RootElement);

        tipo.Erro.Should().Contain("não é \"Meta\" nem \"Realizado\"").And.Contain("Campos recebidos: tipo, mes_rotulo, filial, qt, cliente").And.NotContain("NOME");
        semChassi.Erro.Should().Contain("falta o campo \"chassi\"");
    }
}
