using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>Uma linha do de-para de consultores, COMO VEIO — tudo texto, antes do saneamento.</summary>
/// <param name="Id">O id da linha na GN.</param>
/// <param name="Consultor">O consultor (<c>NOME.SOBRENOME</c>).</param>
/// <param name="Capitao">O gestor do consultor.</param>
/// <param name="FilialNumero">O NN da filial.</param>
/// <param name="InicioVigencia">Desde quando vale, quando a GN diz.</param>
public sealed record ConsultorNaGestao(int Id, string? Consultor, string? Capitao, string? FilialNumero, string? InicioVigencia);

/// <summary>Uma linha do cadastro de forecast, COMO VEIO. A observação não é lida: é texto livre do gestor.</summary>
/// <param name="Id">O id da linha na GN.</param>
/// <param name="Mes">O mês.</param>
/// <param name="Gestor">O gestor.</param>
/// <param name="Linha">A linha, no vocabulário do ART.</param>
/// <param name="Forecast">O Forecast, em unidades; nulo quando o gestor não informou.</param>
/// <param name="BestGuess">O Best Guess, em unidades; nulo quando o gestor não informou.</param>
public sealed record ForecastNaOrigem(int Id, string? Mes, string? Gestor, string? Linha, string? Forecast, string? BestGuess);

/// <summary>Uma cota vendida da performance de consórcio (as linhas "Realizado"), COMO VEIO. O consorciado não é lido.</summary>
/// <param name="Grupo">O grupo.</param>
/// <param name="Cota">A cota.</param>
/// <param name="MesRotulo">O mês da performance (<c>Set/2026</c>).</param>
/// <param name="Filial">A filial, pelo NOME (é como o painel escreve).</param>
/// <param name="Consultor">O consultor (<c>NOME.SOBRENOME</c>).</param>
/// <param name="Gestor">O gestor.</param>
/// <param name="Contemplacao">Lance, Sorteio ou Não Contemplado.</param>
/// <param name="AlocadaEm">A data de alocação.</param>
/// <param name="ContempladaEm">A data da contemplação.</param>
/// <param name="Produto">O bem da cota.</param>
/// <param name="ValorDoBem">O valor do bem.</param>
/// <param name="ValorDaParcela">O valor da parcela.</param>
public sealed record CotaNaOrigem(
    string? Grupo, string? Cota, string? MesRotulo, string? Filial, string? Consultor, string? Gestor, string? Contemplacao,
    string? AlocadaEm, string? ContempladaEm, string? Produto, string? ValorDoBem, string? ValorDaParcela);

/// <summary>O planejamento inteiro, lido e conferido.</summary>
/// <param name="Time">O de-para de consultores.</param>
/// <param name="Forecast">O cadastro de forecast.</param>
/// <param name="Cotas">As cotas vendidas.</param>
/// <param name="MesesDaPerformance">
/// Os meses que a performance de consórcio cobre nesta leitura — o de meta E o de realizado. É a janela dentro da qual uma
/// cota que sumiu foi mesmo cancelada; fora dela, sumiu porque o ano fiscal virou.
/// </param>
/// <param name="Filiais">O de-para das lojas.</param>
/// <param name="GeradaEmUtc">Quando a API gerou o cadastro de forecast.</param>
public sealed record LeituraDoPlanejamentoNaOrigem(
    IReadOnlyList<ConsultorNaGestao> Time,
    IReadOnlyList<ForecastNaOrigem> Forecast,
    IReadOnlyList<CotaNaOrigem> Cotas,
    IReadOnlySet<DateOnly> MesesDaPerformance,
    IReadOnlyList<FilialDaGestao> Filiais,
    DateTime? GeradaEmUtc);

/// <summary>
/// A LEITURA DO PLANEJAMENTO COMERCIAL DA API GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026) — o de-para de
/// consultores, o forecast da gerência, as cotas de consórcio vendidas e o de-para das lojas. Só GET.
///
/// <para><b>O painel de consórcio tem período padrão</b>: sem <c>data_de</c> e <c>data_ate</c>, ele devolve só o recorte da
/// tela (medido em 28/09/2026 — o ART devolvia o mês, 84 de 4.197). A leitura pede sempre a janela larga.</para>
///
/// <para><b>Formato diferente falha alto</b>, como nas metas: campo obrigatório ausente, ou id que não é inteiro, devolve o
/// erro com os NOMES dos campos que chegaram — nunca os valores, que têm nome de pessoa.</para>
/// </summary>
/// <param name="cliente">O cliente da API.</param>
public sealed class LeitorDoPlanejamentoDaGestaoDeNegocios(ClienteDaGestaoDeNegocios cliente)
{
    /// <summary>A rota do de-para de consultores.</summary>
    public const string RotaDoTime = "/api/v1/cadastros/de_para_consultores";

    /// <summary>A rota do forecast.</summary>
    public const string RotaDoForecast = "/api/v1/cadastros/forecast";

    /// <summary>A rota da performance de consórcio, com a janela larga — o painel tem período padrão.</summary>
    public const string RotaDoConsorcio = "/api/v1/paineis/performance-consorcio?data_de=2000-01-01&data_ate=2100-12-31";

    private static readonly string[] Meses = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

    /// <summary>Lê as quatro rotas; uma que falhe derruba a leitura inteira — nada é gravado pela metade.</summary>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDoPlanejamentoNaOrigem>> LerAsync(CancellationToken ct)
    {
        var time = await cliente.LerTudoAsync<JsonElement>(RotaDoTime, ct);
        if (!time.EhSucesso) return Resultado<LeituraDoPlanejamentoNaOrigem>.Indisponivel(time.Erro!);

        var forecast = await cliente.LerTudoAsync<JsonElement>(RotaDoForecast, ct);
        if (!forecast.EhSucesso) return Resultado<LeituraDoPlanejamentoNaOrigem>.Indisponivel(forecast.Erro!);

        var consorcio = await cliente.LerTudoAsync<JsonElement>(RotaDoConsorcio, ct);
        if (!consorcio.EhSucesso) return Resultado<LeituraDoPlanejamentoNaOrigem>.Indisponivel(consorcio.Erro!);

        var filiais = await cliente.LerDocumentoAsync(FiliaisDaGestaoDeNegocios.Rota, ct);
        if (!filiais.EhSucesso) return Resultado<LeituraDoPlanejamentoNaOrigem>.Indisponivel(filiais.Erro!);

        var convertido = Converter(time.Valor.Linhas, forecast.Valor.Linhas, consorcio.Valor.Linhas, filiais.Valor);
        return convertido.EhSucesso
            ? Resultado<LeituraDoPlanejamentoNaOrigem>.Ok(convertido.Valor with { GeradaEmUtc = forecast.Valor.GeradaEmUtc })
            : convertido;
    }

    /// <summary>Converte as quatro respostas — e recusa a leitura inteira se uma linha não tiver o formato.</summary>
    /// <param name="time">As linhas do de-para de consultores.</param>
    /// <param name="forecast">As linhas do forecast.</param>
    /// <param name="consorcio">As linhas da performance de consórcio (meta e realizado).</param>
    /// <param name="filiais">O documento do de-para das lojas.</param>
    public static Resultado<LeituraDoPlanejamentoNaOrigem> Converter(
        IReadOnlyList<JsonElement> time, IReadOnlyList<JsonElement> forecast, IReadOnlyList<JsonElement> consorcio, JsonElement filiais)
    {
        var consultores = new List<ConsultorNaGestao>(time.Count);
        for (var i = 0; i < time.Count; i++)
        {
            var l = time[i];
            if (Faltando(l, "id", "consultor", "capitao") is { } falta) return Formato(RotaDoTime, i, l, falta);
            if (!Inteiro(l, "id", out var id)) return Formato(RotaDoTime, i, l, "o id não é um número inteiro");
            consultores.Add(new ConsultorNaGestao(id, Texto(l, "consultor"), Texto(l, "capitao"), Texto(l, "filial_numero"), Texto(l, "inicio_vigencia")));
        }

        var previsoes = new List<ForecastNaOrigem>(forecast.Count);
        for (var i = 0; i < forecast.Count; i++)
        {
            var l = forecast[i];
            if (Faltando(l, "id", "mes", "gestor", "linha", "forecast", "best_guess") is { } falta) return Formato(RotaDoForecast, i, l, falta);
            if (!Inteiro(l, "id", out var id)) return Formato(RotaDoForecast, i, l, "o id não é um número inteiro");
            previsoes.Add(new ForecastNaOrigem(id, Texto(l, "mes"), Texto(l, "gestor"), Texto(l, "linha"), Texto(l, "forecast"), Texto(l, "best_guess")));
        }

        var cotas = new List<CotaNaOrigem>();
        var meses = new HashSet<DateOnly>();
        for (var i = 0; i < consorcio.Count; i++)
        {
            var l = consorcio[i];
            if (Faltando(l, "tipo", "mes_rotulo") is { } falta) return Formato(RotaDoConsorcio, i, l, falta);
            if (Mes(Texto(l, "mes_rotulo")) is { } mes) meses.Add(mes);
            if (!string.Equals(Texto(l, "tipo"), "Realizado", StringComparison.OrdinalIgnoreCase)) continue;

            if (Faltando(l, "cota_grupo", "cota", "filial", "vendedor", "gestor", "contemplacao", "dt_alocacao") is { } faltaNaCota)
                return Formato(RotaDoConsorcio, i, l, faltaNaCota);
            cotas.Add(new CotaNaOrigem(
                Texto(l, "cota_grupo"), Texto(l, "cota"), Texto(l, "mes_rotulo"), Texto(l, "filial"), Texto(l, "vendedor"), Texto(l, "gestor"),
                Texto(l, "contemplacao"), Texto(l, "dt_alocacao"), Texto(l, "dt_contemplacao"), Texto(l, "produto"), Texto(l, "valor"),
                Texto(l, "valor_parcela")));
        }

        var lojas = FiliaisDaGestaoDeNegocios.Converter(filiais);
        if (!lojas.EhSucesso) return Resultado<LeituraDoPlanejamentoNaOrigem>.Indisponivel(lojas.Erro!);

        return Resultado<LeituraDoPlanejamentoNaOrigem>.Ok(new LeituraDoPlanejamentoNaOrigem(consultores, previsoes, cotas, meses, lojas.Valor, null));
    }

    /// <summary>O mês de um rótulo do painel (<c>Set/2026</c>), no dia 1; nulo quando não se lê.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public static DateOnly? Mes(string? rotulo)
    {
        if (string.IsNullOrWhiteSpace(rotulo)) return null;
        var partes = rotulo.Trim().Split('/');
        if (partes.Length != 2) return null;
        var mes = Array.IndexOf(Meses, partes[0].Trim().ToLowerInvariant()) + 1;
        return mes > 0 && int.TryParse(partes[1].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var ano) && ano is >= 2000 and <= 2100
            ? new DateOnly(ano, mes, 1)
            : null;
    }

    /// <summary>O campo como texto: string como veio, número pelo texto cru, nulo e ausente como nulo.</summary>
    private static string? Texto(JsonElement linha, string campo) =>
        linha.TryGetProperty(campo, out var valor)
            ? valor.ValueKind switch
            {
                JsonValueKind.String => valor.GetString(),
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => valor.GetRawText(),
                _ => null
            }
            : null;

    private static bool Inteiro(JsonElement linha, string campo, out int valor)
    {
        valor = 0;
        return Texto(linha, campo) is { } texto && int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out valor);
    }

    /// <summary>O primeiro campo obrigatório que não veio (a PROPRIEDADE ausente — o valor nulo é do saneamento).</summary>
    private static string? Faltando(JsonElement linha, params string[] campos)
    {
        if (linha.ValueKind != JsonValueKind.Object) return "a linha não é um objeto";
        var ausente = campos.FirstOrDefault(c => !linha.TryGetProperty(c, out _));
        return ausente is null ? null : $"falta o campo \"{ausente}\"";
    }

    private static string Campos(JsonElement linha) =>
        linha.ValueKind == JsonValueKind.Object ? string.Join(", ", linha.EnumerateObject().Select(p => p.Name)) : linha.ValueKind.ToString();

    /// <summary>A recusa por formato: a rota, a posição, o motivo e os NOMES dos campos que chegaram — nunca os valores.</summary>
    private static Resultado<LeituraDoPlanejamentoNaOrigem> Formato(string rota, int posicao, JsonElement linha, string motivo) =>
        Resultado<LeituraDoPlanejamentoNaOrigem>.Indisponivel(
            string.Create(CultureInfo.InvariantCulture,
                $"A linha {posicao + 1} de {rota} não tem o formato esperado ({motivo}). Campos recebidos: {Campos(linha)}. A API pode ter mudado de formato — nada foi gravado."));
}
