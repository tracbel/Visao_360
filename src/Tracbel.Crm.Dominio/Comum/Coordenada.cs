using System.Globalization;

namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Coordenada geográfica (latitude/longitude), com faixa plausível para a área de atuação
/// da Tracbel — hoje, só Brasil.
///
/// [V] LIÇÃO DO VÓRTICE — <c>IV_Historico</c> grava lat/long do atendimento em campo sem
/// nenhuma validação de faixa (um dos acertos do Vórtice é registrar geolocalização —
/// documento 01, seção 12; o que falta é validá-la). Latitude/longitude trocadas de eixo,
/// ou um ponto perdido em outro continente por erro de GPS, hoje entram sem aviso e só
/// aparecem quando o pino cai fora do mapa.
/// </summary>
public readonly record struct Coordenada
{
    // Caixa aproximada do território brasileiro, com margem. Hoje 100% das empresas da
    // Tracbel operam no Brasil (documento 05) — expandir a área de atuação exige rever
    // esta constante, de propósito, num único lugar.
    private const double LatitudeMinima = -34.0;
    private const double LatitudeMaxima = 6.0;
    private const double LongitudeMinima = -75.0;
    private const double LongitudeMaxima = -32.0;

    private Coordenada(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Graus decimais, positivo ao norte.</summary>
    public double Latitude { get; }

    /// <summary>Graus decimais, negativo a oeste (o Brasil inteiro é oeste).</summary>
    public double Longitude { get; }

    /// <summary>Cria a partir de latitude/longitude. Lança se for matematicamente inválida ou estiver fora do Brasil.</summary>
    /// <exception cref="RegraDeNegocioViolada">Coordenada fora da faixa matemática ou fora da área de atuação.</exception>
    public static Coordenada Criar(double latitude, double longitude) =>
        TentarCriar(latitude, longitude, out var coordenada)
            ? coordenada
            : throw new RegraDeNegocioViolada(
                $"Coordenada inválida ou fora da área de atuação: ({latitude}, {longitude}).");

    /// <summary>Versão que não lança. Use em importação em massa.</summary>
    public static bool TentarCriar(double latitude, double longitude, out Coordenada coordenada)
    {
        coordenada = default;

        if (double.IsNaN(latitude) || double.IsNaN(longitude)) return false;
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180) return false;

        // Faixa plausível: não basta ser "matematicamente válida", precisa ser "plausível
        // para o negócio" — mesmo princípio da data (documento 16, seção 2). Faixa larga o
        // bastante para nunca barrar um ponto real da operação, apertada o bastante para
        // pegar erro grosseiro (eixo trocado, sinal perdido, ponto em outro continente).
        if (latitude < LatitudeMinima || latitude > LatitudeMaxima) return false;
        if (longitude < LongitudeMinima || longitude > LongitudeMaxima) return false;

        coordenada = new Coordenada(latitude, longitude);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{Latitude.ToString("F6", CultureInfo.InvariantCulture)}, {Longitude.ToString("F6", CultureInfo.InvariantCulture)}";
}
