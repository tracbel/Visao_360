namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// AS CONTAS DE NÚMERO QUE TODA LEITURA FAZ (documento 54 §3.5) — num lugar só: eram dez cópias privadas em sete arquivos.
/// </summary>
public static class Numeros
{
    /// <summary>A soma do que foi divulgado. Sem nenhum valor, nula — sigilo e ausência não são zero.</summary>
    /// <param name="valores">Os valores, com os nulos.</param>
    public static decimal? SomaOuNulo(IEnumerable<decimal?> valores)
    {
        var com = valores.OfType<decimal>().ToList();
        return com.Count > 0 ? com.Sum() : null;
    }

    /// <summary>O valor arredondado (para o par, como <see cref="decimal.Round(decimal, int)"/>); nulo continua nulo.</summary>
    /// <param name="valor">O valor.</param>
    /// <param name="casas">As casas decimais; duas por padrão.</param>
    public static decimal? Arredondar(decimal? valor, int casas = 2) => valor is { } v ? decimal.Round(v, casas) : null;
}
