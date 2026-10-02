namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Uma ordem de serviço como a ficha do cliente e a da máquina a mostram — sem nome de técnico nem de consultor.</summary>
/// <param name="Chave">O GUID público da OS.</param>
/// <param name="Numero">O número da OS na filial.</param>
/// <param name="FilialCodigo">O código da filial.</param>
/// <param name="FilialNome">O nome da filial.</param>
/// <param name="Situacao">Aberta, Liberada, Fechada ou Cancelada.</param>
/// <param name="TipoDeAtendimento">O tipo de atendimento, como o Protheus escreve.</param>
/// <param name="AbertaEm">A abertura.</param>
/// <param name="LiberadaEm">A liberação.</param>
/// <param name="FechadaEm">O fechamento.</param>
/// <param name="CanceladaEm">O cancelamento.</param>
/// <param name="Chassi">O chassi como o Protheus escreve.</param>
/// <param name="Modelo">O modelo como o Protheus escreve.</param>
/// <param name="Horimetro">O horímetro da abertura.</param>
/// <param name="EquipamentoChave">A máquina do CRM, quando o chassi casou.</param>
/// <param name="ClienteChave">O cliente do CRM, quando o documento casou e ele está ao alcance.</param>
/// <param name="ClienteNome">O nome do cliente, quando está ao alcance.</param>
/// <param name="ValorDePecas">As peças, sem desconto.</param>
/// <param name="ValorDeServicos">Os serviços, sem desconto.</param>
/// <param name="ItensDePeca">As peças requisitadas.</param>
/// <param name="ItensDeServico">Os serviços executados.</param>
public sealed record OrdemDeServicoParaConsulta(
    Guid Chave,
    string Numero,
    string FilialCodigo,
    string FilialNome,
    string Situacao,
    string? TipoDeAtendimento,
    DateOnly AbertaEm,
    DateOnly? LiberadaEm,
    DateOnly? FechadaEm,
    DateOnly? CanceladaEm,
    string? Chassi,
    string? Modelo,
    decimal? Horimetro,
    Guid? EquipamentoChave,
    Guid? ClienteChave,
    string? ClienteNome,
    decimal ValorDePecas,
    decimal ValorDeServicos,
    int ItensDePeca,
    int ItensDeServico);

/// <summary>As ordens de serviço lidas para uma ficha, com a hora da última carga.</summary>
/// <param name="Ordens">As OS ao alcance de quem lê.</param>
/// <param name="CarregadoEm">A última rodada da carga (UTC); nula quando ela nunca rodou.</param>
public sealed record OrdensDeServicoLidas(IReadOnlyList<OrdemDeServicoParaConsulta> Ordens, DateTime? CarregadoEm);

/// <summary>As ordens de serviço da oficina, por cliente e por máquina.</summary>
public interface IRepositorioDeOrdensDeServico
{
    /// <summary>As OS do cliente. Nulo quando o cliente não existe ou está fora do alcance de quem consulta.</summary>
    /// <param name="chaveDoCliente">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<OrdensDeServicoLidas?> DoClienteAsync(Guid chaveDoCliente, CancellationToken ct);

    /// <summary>As OS da máquina. Nulo quando a máquina não existe ou está fora do alcance de quem consulta.</summary>
    /// <param name="chaveDoEquipamento">O GUID público da máquina.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<OrdensDeServicoLidas?> DoEquipamentoAsync(Guid chaveDoEquipamento, CancellationToken ct);
}
