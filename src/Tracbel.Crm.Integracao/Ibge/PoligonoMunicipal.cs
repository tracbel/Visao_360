using System.Text.Json;

namespace Tracbel.Crm.Integracao.Ibge;

/// <summary>
/// O CONTORNO OFICIAL DE UM MUNICÍPIO, e a única pergunta que a carga faz a ele: esta coordenada
/// cai dentro?
///
/// <para><b>Para que serve.</b> É a evidência independente da correção de grafia cortada
/// (documento 32, seção 4.6): quando o endereço tem latitude e longitude, a coordenada tem de cair
/// no município para onde ele vai ser reapontado. Se cair em outro, a correção não é feita — duas
/// fontes discordando não se resolvem por maioria.</para>
///
/// <para><b>Como.</b> Paridade de cruzamentos (ray casting) sobre cada polígono, descontando os
/// buracos. A malha do IBGE usa longitude e latitude em graus, e a comparação é feita no mesmo
/// sistema — não há projeção, porque a pergunta é de pertencimento, não de distância.</para>
/// </summary>
public sealed class PoligonoMunicipal
{
    private readonly IReadOnlyList<IReadOnlyList<(double Lon, double Lat)[]>> _poligonos;

    private PoligonoMunicipal(IReadOnlyList<IReadOnlyList<(double Lon, double Lat)[]>> poligonos) =>
        _poligonos = poligonos;

    /// <summary>
    /// Lê a geometria GeoJSON de um município — <c>Polygon</c> ou <c>MultiPolygon</c>. Em cada
    /// polígono, o primeiro anel é o contorno e os seguintes são buracos.
    /// </summary>
    /// <param name="geometria">O objeto <c>geometry</c> da feição.</param>
    public static PoligonoMunicipal DeGeoJson(JsonElement geometria)
    {
        var tipo = geometria.GetProperty("type").GetString();
        var coordenadas = geometria.GetProperty("coordinates");

        var poligonos = tipo switch
        {
            "Polygon" => [LerPoligono(coordenadas)],
            "MultiPolygon" => coordenadas.EnumerateArray().Select(LerPoligono).ToList(),
            _ => throw new FormatException($"Geometria municipal de tipo inesperado: {tipo}.")
        };

        return new PoligonoMunicipal(poligonos);
    }

    /// <summary>Monta a partir de anéis já lidos — é o caminho dos testes.</summary>
    /// <param name="poligonos">Os polígonos; em cada um, contorno e depois buracos.</param>
    public static PoligonoMunicipal DeAneis(IReadOnlyList<IReadOnlyList<(double Lon, double Lat)[]>> poligonos) =>
        new(poligonos);

    /// <summary>Se a coordenada cai dentro do município (e fora de qualquer buraco dele).</summary>
    /// <param name="longitude">Longitude em graus decimais.</param>
    /// <param name="latitude">Latitude em graus decimais.</param>
    public bool Contem(double longitude, double latitude) =>
        _poligonos.Any(poligono =>
            poligono.Count > 0
            && Dentro(poligono[0], longitude, latitude)
            && !poligono.Skip(1).Any(buraco => Dentro(buraco, longitude, latitude)));

    private static IReadOnlyList<(double Lon, double Lat)[]> LerPoligono(JsonElement aneis) =>
    [
        .. aneis.EnumerateArray().Select(anel =>
            anel.EnumerateArray().Select(ponto => (ponto[0].GetDouble(), ponto[1].GetDouble())).ToArray())
    ];

    private static bool Dentro((double Lon, double Lat)[] anel, double longitude, double latitude)
    {
        var dentro = false;

        for (int i = 0, j = anel.Length - 1; i < anel.Length; j = i++)
        {
            var (xi, yi) = anel[i];
            var (xj, yj) = anel[j];

            if ((yi > latitude) != (yj > latitude)
                && longitude < ((xj - xi) * (latitude - yi) / (yj - yi)) + xi)
                dentro = !dentro;
        }

        return dentro;
    }
}
