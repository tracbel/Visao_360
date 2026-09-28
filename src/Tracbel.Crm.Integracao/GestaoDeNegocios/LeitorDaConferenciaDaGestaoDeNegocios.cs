using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>Uma máquina do realizado da GN, COMO VEIO — só o que a conferência usa: sem cliente, vendedor, valor nem lucro.</summary>
/// <param name="Chassi">O chassi.</param>
/// <param name="Filial">A filial, pelo NOME.</param>
/// <param name="MesRotulo">O mês da performance (<c>Set/2026</c>) — o da entrega.</param>
/// <param name="Quantidade">A quantidade da linha.</param>
public sealed record RealizadoNaGestao(string? Chassi, string? Filial, string? MesRotulo, string? Quantidade);

/// <summary>Uma linha da meta (o PO) da GN, COMO VEIO.</summary>
/// <param name="Filial">A filial, pelo NOME.</param>
/// <param name="MesRotulo">O mês.</param>
/// <param name="Quantidade">A quantidade.</param>
public sealed record MetaNaGestao(string? Filial, string? MesRotulo, string? Quantidade);

/// <summary>O gabarito da conferência, lido e conferido.</summary>
public sealed record LeituraDaConferenciaNaOrigem(
    IReadOnlyList<RealizadoNaGestao> Realizado,
    IReadOnlyList<MetaNaGestao> Meta,
    IReadOnlyList<FilialDaGestao> Filiais,
    DateTime? GeradaEmUtc);

/// <summary>
/// A LEITURA DO GABARITO DA CONFERÊNCIA (decisão do Ricardo em 28/09/2026) — a <c>performance-maquinas</c> da API Gestão
/// de Negócios: a meta (o PO) e o realizado, que é a máquina ENTREGUE, no mês da entrega. Só GET, sempre com a janela
/// larga (o painel tem período padrão).
///
/// <para><b>Nenhum nome sai daqui</b>: o cliente, o vendedor e o gestor ficam na GN. A conferência casa pelo chassi, pela
/// filial e pelo mês. O valor e o lucro também não são lidos.</para>
/// </summary>
/// <param name="cliente">O cliente da API.</param>
public sealed class LeitorDaConferenciaDaGestaoDeNegocios(ClienteDaGestaoDeNegocios cliente)
{
    /// <summary>A rota da performance de máquinas, com a janela larga.</summary>
    public const string RotaDaPerformance = "/api/v1/paineis/performance-maquinas?data_de=2000-01-01&data_ate=2100-12-31";

    /// <summary>Lê a performance e o de-para das lojas.</summary>
    public async Task<Resultado<LeituraDaConferenciaNaOrigem>> LerAsync(CancellationToken ct)
    {
        var performance = await cliente.LerTudoAsync<JsonElement>(RotaDaPerformance, ct);
        if (!performance.EhSucesso) return Resultado<LeituraDaConferenciaNaOrigem>.Indisponivel(performance.Erro!);

        var filiais = await cliente.LerDocumentoAsync(FiliaisDaGestaoDeNegocios.Rota, ct);
        if (!filiais.EhSucesso) return Resultado<LeituraDaConferenciaNaOrigem>.Indisponivel(filiais.Erro!);

        var convertido = Converter(performance.Valor.Linhas, filiais.Valor);
        return convertido.EhSucesso
            ? Resultado<LeituraDaConferenciaNaOrigem>.Ok(convertido.Valor with { GeradaEmUtc = performance.Valor.GeradaEmUtc })
            : convertido;
    }

    /// <summary>Converte as linhas — a do realizado vira máquina, a da meta vira PO — e recusa a leitura fora do formato.</summary>
    public static Resultado<LeituraDaConferenciaNaOrigem> Converter(IReadOnlyList<JsonElement> performance, JsonElement filiais)
    {
        var realizado = new List<RealizadoNaGestao>();
        var meta = new List<MetaNaGestao>();
        for (var i = 0; i < performance.Count; i++)
        {
            var l = performance[i];
            if (l.ValueKind != JsonValueKind.Object) return Formato(i, l, "não é um objeto");
            if (Obrigatorios.FirstOrDefault(c => !l.TryGetProperty(c, out _)) is { } falta) return Formato(i, l, $"falta o campo \"{falta}\"");

            switch (Texto(l, "tipo"))
            {
                case "Realizado":
                    if (!l.TryGetProperty("chassi", out _)) return Formato(i, l, "falta o campo \"chassi\"");
                    realizado.Add(new RealizadoNaGestao(Texto(l, "chassi"), Texto(l, "filial"), Texto(l, "mes_rotulo"), Texto(l, "qt")));
                    break;
                case "Meta":
                    meta.Add(new MetaNaGestao(Texto(l, "filial"), Texto(l, "mes_rotulo"), Texto(l, "qt")));
                    break;
                default:
                    return Formato(i, l, "o tipo não é \"Meta\" nem \"Realizado\"");
            }
        }

        var lojas = FiliaisDaGestaoDeNegocios.Converter(filiais);
        return lojas.EhSucesso
            ? Resultado<LeituraDaConferenciaNaOrigem>.Ok(new LeituraDaConferenciaNaOrigem(realizado, meta, lojas.Valor, null))
            : Resultado<LeituraDaConferenciaNaOrigem>.Indisponivel(lojas.Erro!);
    }

    private static readonly string[] Obrigatorios = ["tipo", "filial", "mes_rotulo", "qt"];

    private static string? Texto(JsonElement linha, string campo) =>
        linha.TryGetProperty(campo, out var valor)
            ? valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),
                JsonValueKind.Number => valor.GetRawText(),
                _ => null
            }
            : null;

    private static Resultado<LeituraDaConferenciaNaOrigem> Formato(int posicao, JsonElement linha, string motivo)
    {
        var campos = linha.ValueKind == JsonValueKind.Object ? string.Join(", ", linha.EnumerateObject().Select(p => p.Name)) : linha.ValueKind.ToString();
        return Resultado<LeituraDaConferenciaNaOrigem>.Indisponivel(string.Create(CultureInfo.InvariantCulture,
            $"A linha {posicao + 1} da performance de máquinas não tem o formato esperado ({motivo}). Campos recebidos: {campos}. A API pode ter mudado de formato — nada foi gravado."));
    }
}
