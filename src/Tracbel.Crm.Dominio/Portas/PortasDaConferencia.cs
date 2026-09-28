using Tracbel.Crm.Dominio.Integracao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Uma linha da conferência, com o nome da filial.</summary>
public sealed record LinhaDaConferencia(string Filial, string Indicador, DateOnly Competencia, int NaGestao, int NoCrm);

/// <summary>Uma divergência aberta da conferência — chassi a chassi.</summary>
public sealed record DivergenciaDaConferencia(
    TipoDeDivergencia Tipo, string Chassi, string Filial, string Descricao, string? ValorNoCrm, string? ValorNaGestao, DateTime DetectadaEm);

/// <summary>O que o banco tem da conferência — pelas filiais ao alcance (o filtro global).</summary>
public sealed record ConferenciaLida(
    IReadOnlyList<LinhaDaConferencia> Linhas, IReadOnlyList<DivergenciaDaConferencia> Divergencias, DateTime? ApuradaEm, DateTime? GeradaNaOrigemEm);

/// <summary>A leitura da conferência com a Gestão de Negócios.</summary>
public interface IRepositorioDaConferencia
{
    /// <summary>Lê a última apuração e as divergências abertas.</summary>
    Task<ConferenciaLida> LerAsync(CancellationToken ct);
}
