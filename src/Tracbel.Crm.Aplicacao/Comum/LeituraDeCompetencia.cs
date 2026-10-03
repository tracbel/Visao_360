using System.Globalization;

namespace Tracbel.Crm.Aplicacao.Comum;

/// <summary>O MÊS "aaaa-mm" DOS FILTROS DE PERÍODO (documento 54 §3.5) — num lugar só: os Indicadores e o Diagnóstico liam cada um o seu.</summary>
public static class LeituraDeCompetencia
{
    /// <summary>O mês lido, ou o padrão; o texto que não é "aaaa-mm" vira erro no campo, com o exemplo.</summary>
    /// <param name="texto">O que veio no pedido.</param>
    /// <param name="campo">O nome do campo, para o erro.</param>
    /// <param name="padrao">O mês quando o pedido não traz nenhum.</param>
    /// <param name="erros">Onde o erro é registrado.</param>
    public static DateOnly Ler(string? texto, string campo, DateOnly padrao, ColetorDeErros erros)
    {
        if (string.IsNullOrWhiteSpace(texto)) return padrao;

        if (DateOnly.TryParseExact(texto.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mes))
            return mes;

        erros.Registrar(campo, "Use o formato aaaa-mm, por exemplo 2026-08.", texto);
        return padrao;
    }
}
