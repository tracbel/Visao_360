namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Uma combinação do faturamento de peças de um cliente num mês, como a ficha a soma.</summary>
/// <param name="Competencia">O mês, no dia 1.</param>
/// <param name="FilialCodigo">A filial que faturou.</param>
/// <param name="FilialNome">O nome da filial.</param>
/// <param name="Setor">O setor (balcão, oficina…).</param>
/// <param name="Grupo">O grupo comercial (peças, pneus, lubrificantes…).</param>
/// <param name="Linha">A linha do produto.</param>
/// <param name="VendedorNome">O vendedor.</param>
/// <param name="ValorLiquido">O valor líquido, com as devoluções descontadas.</param>
/// <param name="ValorDeDevolucoes">Quanto é devolução.</param>
/// <param name="ValorDeDesconto">O desconto.</param>
/// <param name="ValorDeTabela">O valor de tabela.</param>
/// <param name="Itens">Os itens de nota.</param>
public sealed record PecasDoClienteNoMes(
    DateOnly Competencia,
    string FilialCodigo,
    string FilialNome,
    string Setor,
    string Grupo,
    string Linha,
    string? VendedorNome,
    decimal ValorLiquido,
    decimal ValorDeDevolucoes,
    decimal ValorDeDesconto,
    decimal ValorDeTabela,
    int Itens);

/// <summary>Um orçamento de peças, como a ficha o mostra.</summary>
/// <param name="Chave">O GUID público.</param>
/// <param name="Numero">O número.</param>
/// <param name="FilialCodigo">A filial.</param>
/// <param name="FilialNome">O nome da filial.</param>
/// <param name="Situacao">A situação, como o Protheus escreve.</param>
/// <param name="Prazo">No prazo, vencido ou atendido.</param>
/// <param name="Reserva">A reserva de estoque.</param>
/// <param name="OrcadoEm">A data do orçamento.</param>
/// <param name="ValidoAte">A validade.</param>
/// <param name="VendedorNome">O vendedor.</param>
/// <param name="ValorTotal">O total, sem desconto.</param>
/// <param name="Itens">Os itens.</param>
public sealed record OrcamentoDePecasParaConsulta(
    Guid Chave,
    string Numero,
    string FilialCodigo,
    string FilialNome,
    string Situacao,
    string? Prazo,
    string? Reserva,
    DateOnly OrcadoEm,
    DateOnly? ValidoAte,
    string? VendedorNome,
    decimal ValorTotal,
    int Itens);

/// <summary>As peças de um cliente: o faturamento por mês e os orçamentos, com a hora das cargas.</summary>
/// <param name="Faturamento">As combinações do faturamento desde a data pedida.</param>
/// <param name="Orcamentos">Os orçamentos ativos.</param>
/// <param name="FaturamentoCarregadoEm">A última apuração do faturamento (UTC); nula quando nunca rodou.</param>
/// <param name="OrcamentosCarregadosEm">A última sincronia dos orçamentos (UTC); nula quando nunca rodou.</param>
public sealed record PecasDoClienteLidas(
    IReadOnlyList<PecasDoClienteNoMes> Faturamento,
    IReadOnlyList<OrcamentoDePecasParaConsulta> Orcamentos,
    DateTime? FaturamentoCarregadoEm,
    DateTime? OrcamentosCarregadosEm);

/// <summary>As peças do Protheus, por cliente.</summary>
public interface IRepositorioDePecas
{
    /// <summary>As peças do cliente. Nulo quando o cliente não existe ou está fora do alcance de quem consulta.</summary>
    /// <param name="chaveDoCliente">O GUID público do cliente.</param>
    /// <param name="desde">O primeiro mês do faturamento.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<PecasDoClienteLidas?> DoClienteAsync(Guid chaveDoCliente, DateOnly desde, CancellationToken ct);
}
