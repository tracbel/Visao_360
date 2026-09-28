namespace Tracbel.Crm.Integracao.Ibge;

/// <summary>
/// EM QUE MUNICÍPIO UM PONTO CAI — pela malha oficial do IBGE (a telemetria das máquinas, 28/09/2026).
///
/// <para><b>O primeiro contorno que contém o ponto.</b> Os municípios não se sobrepõem; o ponto exatamente na divisa cai
/// no primeiro da lista, que é ordenada pelo código IBGE — a resposta é a mesma a cada rodada.</para>
///
/// <para><b>Fora da malha, nulo.</b> A malha lida é a de um estado; o ponto fora dele não tem município aqui, e isso é
/// dito como "fora de SP", nunca como um município vizinho por aproximação.</para>
/// </summary>
public sealed class LocalizadorDeMunicipio
{
    private readonly (int CodigoIbge, PoligonoMunicipal Contorno)[] _municipios;

    /// <summary>Monta o localizador a partir da malha de uma UF.</summary>
    /// <param name="malha">O contorno de cada município, pelo código IBGE.</param>
    public LocalizadorDeMunicipio(IReadOnlyDictionary<int, PoligonoMunicipal> malha) =>
        _municipios = [.. malha.OrderBy(m => m.Key).Select(m => (m.Key, m.Value))];

    /// <summary>Quantos municípios a malha tem.</summary>
    public int Municipios => _municipios.Length;

    /// <summary>O código IBGE do município onde o ponto cai; nulo fora da malha.</summary>
    /// <param name="longitude">Longitude em graus decimais.</param>
    /// <param name="latitude">Latitude em graus decimais.</param>
    public int? CodigoIbgeDe(double longitude, double latitude)
    {
        foreach (var (codigo, contorno) in _municipios)
            if (contorno.Contem(longitude, latitude))
                return codigo;

        return null;
    }
}
