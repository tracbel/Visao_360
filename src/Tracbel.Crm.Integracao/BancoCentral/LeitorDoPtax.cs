using System.Globalization;
using System.Text.Json;

namespace Tracbel.Crm.Integracao.BancoCentral;

/// <summary>
/// UM MÊS DO DÓLAR PTAX, como o Sistema Gerenciador de Séries do Banco Central o publica.
/// </summary>
/// <param name="DataBruta">O primeiro dia do mês, como <c>dd/MM/aaaa</c>.</param>
/// <param name="ValorBruto">Reais por dólar, com PONTO decimal (<c>5.1532</c>).</param>
public sealed record LinhaDoPtax(string DataBruta, string ValorBruto);

/// <summary>
/// A LEITURA DO DÓLAR PTAX MENSAL — a conversão dos preços para dólar.
///
/// <para><b>Por que existe.</b> O texto-base do potencial pede a base de preços "em real e dólar por
/// tipo de cultivo". Os preços chegam em reais (CONAB, Socicana); o dólar é o PTAX de venda, média do
/// mês — a série 3698 do SGS, a mesma que o mercado usa para contrato em dólar.</para>
///
/// <para><b>O CRM não grava o preço em dólar.</b> Grava o preço em reais e o dólar do mês; a conversão
/// é feita na leitura. Gravar as duas seria uma segunda fonte para a mesma verdade, livre para
/// divergir quando o Banco Central revisar um mês.</para>
///
/// <para><b>Só leitura, só GET, API pública.</b> Nenhuma credencial é usada nem existe.</para>
/// </summary>
/// <param name="http">O cliente HTTP.</param>
public sealed class LeitorDoPtax(HttpClient http)
{
    /// <summary>O código da série no SGS: dólar americano, venda, PTAX, média mensal.</summary>
    public const int SerieDoDolarMensal = 3698;

    /// <summary>
    /// O primeiro mês lido — o mesmo da série mais antiga que o CRM tem (a safra 2015/16 da cana).
    /// </summary>
    public static readonly DateOnly InicioDaSerie = new(2015, 1, 1);

    /// <summary>O endereço da consulta, do início da série até hoje.</summary>
    public static string Endereco(DateOnly inicio) =>
        $"https://api.bcb.gov.br/dados/serie/bcdata.sgs.{SerieDoDolarMensal}/dados?formato=json" +
        $"&dataInicial={inicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";

    /// <summary>Lê o dólar mensal desde <paramref name="inicio"/>.</summary>
    /// <param name="inicio">O primeiro mês.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <exception cref="InvalidDataException">Quando a resposta não é a lista esperada.</exception>
    public async Task<IReadOnlyList<LinhaDoPtax>> LerAsync(DateOnly inicio, CancellationToken ct) =>
        Interpretar(await http.GetStringAsync(Endereco(inicio), ct));

    /// <summary>Interpreta a resposta. Público para os testes.</summary>
    /// <param name="json">O corpo da resposta.</param>
    /// <exception cref="InvalidDataException">Quando a resposta não é a lista esperada.</exception>
    public static IReadOnlyList<LinhaDoPtax> Interpretar(string json)
    {
        using var documento = JsonDocument.Parse(json);

        // O SGS RESPONDE ERRO COM STATUS 200 E UM OBJETO no lugar da lista — por exemplo, quando o
        // intervalo passa do limite. Tratar o objeto como "lista vazia" apagaria o problema.
        if (documento.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException($"O SGS do Banco Central não respondeu uma lista: {Resumo(json)}");

        var linhas = new List<LinhaDoPtax>();

        foreach (var item in documento.RootElement.EnumerateArray())
        {
            if (!item.TryGetProperty("data", out var data) || !item.TryGetProperty("valor", out var valor))
                throw new InvalidDataException($"Item do SGS sem \"data\" ou \"valor\": {item}");

            linhas.Add(new LinhaDoPtax(data.GetString() ?? string.Empty, valor.GetString() ?? string.Empty));
        }

        return linhas;
    }

    /// <summary>O mês da linha, como data do primeiro dia.</summary>
    /// <param name="bruta">A data como veio (<c>01/08/2026</c>).</param>
    /// <exception cref="FormatException">Quando a data não está no formato ou não é dia 1.</exception>
    public static DateOnly Mes(string bruta)
    {
        var data = DateOnly.ParseExact(bruta, "dd/MM/yyyy", CultureInfo.InvariantCulture);

        if (data.Day != 1)
            throw new FormatException($"A série mensal do SGS deveria vir no dia 1; veio \"{bruta}\".");

        return data;
    }

    /// <summary>Reais por dólar, com ponto decimal.</summary>
    /// <param name="bruto">O valor como veio.</param>
    /// <exception cref="FormatException">Quando não é um número positivo.</exception>
    public static decimal ReaisPorDolar(string bruto)
    {
        if (!decimal.TryParse(bruto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var valor)
            || valor <= 0)
            throw new FormatException($"Dólar do SGS fora do formato: \"{bruto}\".");

        return valor;
    }

    private static string Resumo(string texto) => texto.Length <= 200 ? texto : texto[..200] + "…";
}
