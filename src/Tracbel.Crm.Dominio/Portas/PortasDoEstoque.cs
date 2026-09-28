namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Uma máquina do estoque ou do pedido à fábrica, como a tela a lê (28/09/2026). Sem custo e sem cliente.</summary>
public sealed record MaquinaNoEstoque(
    string Filial,
    string Grupo,
    string Descricao,
    string? Configuracao,
    string Situacao,
    string Tipo,
    bool EhUsado,
    string? AnoModelo,
    string? Chassi,
    DateOnly? EntradaEm,
    DateOnly? ChegadaPrevistaEm,
    DateOnly? FaturamentoPrevistoEm,
    bool Pago,
    bool Reservado,
    string? SituacaoNaFabrica);

/// <summary>Um item da cobertura em meses de estoque.</summary>
/// <param name="Chave">O mês (<c>AAAA-MM</c>) ou o grupo.</param>
/// <param name="Competencia">O mês, no dia 1, quando o recorte é por mês.</param>
/// <param name="Meses">Os meses de estoque.</param>
/// <param name="Vendas">As vendas do período.</param>
public sealed record ItemDaCoberturaDoEstoque(string Chave, DateOnly? Competencia, decimal Meses, int Vendas);

/// <summary>O que o banco tem do estoque e da cobertura, e o frescor de cada um.</summary>
public sealed record EstoqueLido(
    IReadOnlyList<MaquinaNoEstoque> Maquinas,
    IReadOnlyList<ItemDaCoberturaDoEstoque> CoberturaPorMes,
    IReadOnlyList<ItemDaCoberturaDoEstoque> CoberturaPorGrupo,
    DateTime? EstoqueLidoEm,
    DateTime? EstoqueGeradoNaOrigemEm,
    DateTime? CoberturaLidaEm,
    DateTime? CoberturaGeradaNaOrigemEm);

/// <summary>A leitura do estoque — as máquinas pelas filiais ao alcance (o filtro global); a cobertura, da organização.</summary>
public interface IRepositorioDoEstoque
{
    /// <summary>Lê o estoque vigente e a cobertura.</summary>
    Task<EstoqueLido> LerAsync(CancellationToken ct);
}
