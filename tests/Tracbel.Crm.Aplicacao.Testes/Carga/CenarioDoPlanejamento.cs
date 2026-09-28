using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasMetas;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UM PLANEJAMENTO DE MENTIRA, para a carga do de-para, do forecast e do consórcio da API Gestão de Negócios — sobre a
/// mesma semente das metas (Ribeirão 010101, Ituverava 010105, o consultor com conta e o sem). Dois gestores, um consultor
/// em duas linhas do de-para, duas previsões, e três cotas: uma em cada filial e uma na Digital, que o CRM não tem. Nomes
/// inventados.
/// </summary>
internal static class CenarioDoPlanejamento
{
    public const string GestorNorte = "GESTOR.NORTE";
    public const string GestorSul = "GESTOR.SUL";

    /// <summary>Os meses que a performance de consórcio cobre na leitura padrão: agosto e setembro de 2026.</summary>
    public static readonly IReadOnlySet<DateOnly> AgostoESetembro = new HashSet<DateOnly> { new(2026, 8, 1), new(2026, 9, 1) };

    /// <summary>O de-para das lojas: as duas filiais da semente e a Digital, sem código no TOTVS.</summary>
    public static IReadOnlyList<FilialDaGestao> Filiais() =>
    [
        new(1, "RIBEIRAO PRETO", "010101"),
        new(5, "ITUVERAVA", "010105"),
        new(0, "DIGITAL", null)
    ];

    /// <summary>
    /// O de-para: o consultor com conta é do Norte; o sem conta aparece duas vezes — a linha mais nova (do Sul) é a que vale.
    /// </summary>
    public static List<ConsultorNaGestao> Time() =>
    [
        new(1, ConsultorComConta, GestorNorte, "5", "2025-11-01"),
        new(2, ConsultorSemConta, GestorNorte, "1", "2025-11-01"),
        new(3, ConsultorSemConta, GestorSul, "1", "2026-06-01")
    ];

    /// <summary>Uma previsão como a API manda.</summary>
    public static ForecastNaOrigem Previsao(
        int id, string mes = "2026-09-01", string gestor = GestorNorte, string linha = "TRATOR MÉDIO", string? forecast = "5",
        string? bestGuess = "6") =>
        new(id, mes, gestor, linha, forecast, bestGuess);

    /// <summary>O forecast: o Norte com os dois números, o Sul só com o best guess.</summary>
    public static List<ForecastNaOrigem> Forecast() =>
    [
        Previsao(1),
        Previsao(2, gestor: GestorSul, forecast: null, bestGuess: "2")
    ];

    /// <summary>Uma cota vendida como a performance manda (a linha "Realizado").</summary>
    public static CotaNaOrigem Cota(
        string grupo, string cota, string mes = "Set/2026", string filial = "ITUVERAVA", string consultor = ConsultorComConta,
        string gestor = GestorNorte, string contemplacao = "Lance", string alocada = "2026-09-10", string? valor = "250000.00") =>
        new(grupo, cota, mes, filial, consultor, gestor, contemplacao, alocada, null, "TRATOR 6M", valor, "3100.50");

    /// <summary>As cotas: Ituverava em setembro, Ribeirão em agosto, e uma da Digital.</summary>
    public static List<CotaNaOrigem> Cotas() =>
    [
        Cota("1000", "1"),
        Cota("1000", "2", mes: "Ago/2026", filial: "RIBEIRAO PRETO", consultor: ConsultorSemConta, gestor: GestorSul,
            contemplacao: "Não Contemplado", alocada: "2026-08-03"),
        Cota("1001", "7", filial: "DIGITAL")
    ];

    /// <summary>A leitura inteira.</summary>
    public static LeituraDoPlanejamentoNaOrigem Leitura(
        IReadOnlyList<ConsultorNaGestao>? time = null, IReadOnlyList<ForecastNaOrigem>? forecast = null,
        IReadOnlyList<CotaNaOrigem>? cotas = null, IReadOnlySet<DateOnly>? meses = null) =>
        new(time ?? Time(), forecast ?? Forecast(), cotas ?? Cotas(), meses ?? AgostoESetembro, Filiais(),
            new DateTime(2026, 9, 28, 4, 30, 0, DateTimeKind.Utc));

    /// <summary>A carga sobre a leitura dada.</summary>
    public static CargaDoPlanejamentoDaGestaoDeNegocios Sincronia(
        Func<CrmDbContext> abrir, LeituraDoPlanejamentoNaOrigem leitura, DateTime agora, long operador) =>
        new(abrir, _ => Task.FromResult(Resultado<LeituraDoPlanejamentoNaOrigem>.Ok(leitura)), operador, () => agora, _ => { });
}
