using System.Text.Json;
using FluentAssertions;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Ibge;

/// <summary>
/// O contorno oficial como evidência da correção de grafia cortada (documento 32, seção 4.6): a
/// coordenada do endereço tem de cair dentro do município para onde ele vai.
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class PoligonoMunicipalTestes
{
    private const string MultiPoligonoComBuraco = """
        {
          "type": "MultiPolygon",
          "coordinates": [
            [
              [[-50, -20], [-49, -20], [-49, -21], [-50, -21], [-50, -20]],
              [[-49.6, -20.4], [-49.4, -20.4], [-49.4, -20.6], [-49.6, -20.6], [-49.6, -20.4]]
            ],
            [
              [[-48, -22], [-47.5, -22], [-47.5, -22.5], [-48, -22.5], [-48, -22]]
            ]
          ]
        }
        """;

    private static PoligonoMunicipal Contorno() =>
        PoligonoMunicipal.DeGeoJson(JsonDocument.Parse(MultiPoligonoComBuraco).RootElement);

    [Fact]
    public void Coordenada_dentro_do_contorno_esta_no_municipio() =>
        Contorno().Contem(-49.9, -20.1).Should().BeTrue();

    [Fact]
    public void Coordenada_no_segundo_poligono_tambem_esta_no_municipio() =>
        Contorno().Contem(-47.8, -22.2).Should().BeTrue();

    [Fact]
    public void Coordenada_dentro_do_buraco_nao_esta_no_municipio() =>
        Contorno().Contem(-49.5, -20.5).Should().BeFalse("o buraco é outro município encravado");

    [Fact]
    public void Coordenada_fora_de_todos_os_poligonos_nao_esta_no_municipio() =>
        Contorno().Contem(-45, -23).Should().BeFalse();

    [Fact]
    public void Geometria_de_tipo_desconhecido_para_a_leitura()
    {
        var ler = () => PoligonoMunicipal.DeGeoJson(JsonDocument.Parse("""{"type":"Point","coordinates":[0,0]}""").RootElement);
        ler.Should().Throw<FormatException>();
    }
}
