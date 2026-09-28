using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>Uma loja do de-para de filiais da GN.</summary>
/// <param name="Numero">O número — o NroEmpresa do Vórtice, e os dois últimos dígitos do TOTVS.</param>
/// <param name="Nome">O nome, como os painéis escrevem.</param>
/// <param name="CodigoTotvs">O código da filial no TOTVS (<c>0101NN</c>); nulo em Digital, Grandes Contas e na Colorado.</param>
public sealed record FilialDaGestao(int Numero, string Nome, string? CodigoTotvs);

/// <summary>
/// O DE-PARA DAS LOJAS DA API GESTÃO DE NEGÓCIOS (<c>/api/v1/filiais</c>, 28/09/2026) — número, nome e código do TOTVS.
///
/// <para><b>Os painéis escrevem a filial pelo NOME</b> (a performance de consórcio, o estoque), e é este de-para que diz o
/// código do TOTVS — o mesmo <c>Empresa.Codigo</c> do CRM. O nome casa pelo código estável (sem acento nem caixa). A loja
/// sem código (Digital, Grandes Contas, a Colorado) não é filial do CRM.</para>
///
/// <para><b>Não é paginada</b>: é lida pelo documento inteiro.</para>
/// </summary>
public static class FiliaisDaGestaoDeNegocios
{
    /// <summary>A rota do de-para das lojas.</summary>
    public const string Rota = "/api/v1/filiais";

    /// <summary>Converte o documento — e recusa a leitura inteira se ele não tiver o formato.</summary>
    /// <param name="documento">O documento de <see cref="Rota"/>.</param>
    public static Resultado<IReadOnlyList<FilialDaGestao>> Converter(JsonElement documento)
    {
        if (documento.ValueKind != JsonValueKind.Object || !documento.TryGetProperty("filiais", out var lista) || lista.ValueKind != JsonValueKind.Array)
            return Resultado<IReadOnlyList<FilialDaGestao>>.Indisponivel(
                $"{Rota} não trouxe a lista \"filiais\". A API pode ter mudado de formato — nada foi gravado.");

        var lojas = new List<FilialDaGestao>();
        foreach (var f in lista.EnumerateArray())
        {
            if (f.ValueKind != JsonValueKind.Object || Texto(f, "numero") is not { } numeroTexto
                || !int.TryParse(numeroTexto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero)
                || Texto(f, "nome") is not { Length: > 0 } nome)
            {
                var campos = f.ValueKind == JsonValueKind.Object ? string.Join(", ", f.EnumerateObject().Select(p => p.Name)) : f.ValueKind.ToString();
                return Resultado<IReadOnlyList<FilialDaGestao>>.Indisponivel(
                    $"Uma loja de {Rota} veio sem número ou sem nome. Campos recebidos: {campos}. Nada foi gravado.");
            }

            lojas.Add(new FilialDaGestao(numero, nome, Texto(f, "codigo_totvs") is { Length: > 0 } codigo ? codigo : null));
        }

        return Resultado<IReadOnlyList<FilialDaGestao>>.Ok(lojas);
    }

    /// <summary>O código do TOTVS de cada loja, pelo código estável do nome — só as lojas que têm código.</summary>
    /// <param name="filiais">O de-para.</param>
    public static IReadOnlyDictionary<string, string> CodigoPorNome(IEnumerable<FilialDaGestao> filiais) =>
        filiais
            .Where(f => f.CodigoTotvs is not null)
            .GroupBy(f => CodigoEstavel.De(f.Nome))
            .ToDictionary(g => g.Key, g => g.First().CodigoTotvs!, StringComparer.Ordinal);

    /// <summary>O campo como texto: string como veio, número pelo texto cru, nulo e ausente como nulo.</summary>
    private static string? Texto(JsonElement linha, string campo) =>
        linha.TryGetProperty(campo, out var valor)
            ? valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),
                JsonValueKind.Number => valor.GetRawText(),
                _ => null
            }
            : null;
}
