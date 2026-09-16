namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Uma venda da máquina, com o comprador nela e a trilha da origem — o histórico comercial.</summary>
/// <param name="Chave">O GUID público da venda.</param>
/// <param name="SistemaCodigo">O sistema de origem. Ex.: ART.</param>
/// <param name="ChaveOrigem">O identificador do registro na origem.</param>
/// <param name="VendidaEm">A data da venda.</param>
/// <param name="FaturadaEm">A data do faturamento.</param>
/// <param name="EntregueEm">A data da entrega.</param>
/// <param name="RegistradaNaOrigemEm">Quando a venda foi aberta na origem.</param>
/// <param name="FilialCodigo">A filial que vendeu.</param>
/// <param name="FilialNome">O nome dela.</param>
/// <param name="FilialDoFaturamentoCodigo">A filial que faturou.</param>
/// <param name="CompradorChave">O GUID do comprador, quando está ao alcance de quem consulta.</param>
/// <param name="CompradorNome">O nome do comprador.</param>
/// <param name="Natureza">A natureza do vínculo — comprador na venda.</param>
/// <param name="VinculoReferenciaEm">A data de referência do vínculo.</param>
/// <param name="VinculoEncerradoEm">Quando o vínculo deixou de valer.</param>
/// <param name="MotivoDoEncerramento">Por quê.</param>
/// <param name="LinhaNaOrigem">A linha como a origem escreve.</param>
/// <param name="ProdutoNaOrigem">O produto como a origem escreve.</param>
/// <param name="GestaoNaOrigem">Varejo ou Grandes Contas — atributo desta venda.</param>
/// <param name="SituacaoNaOrigem">A situação como a origem escreve.</param>
/// <param name="NumeroDoPedido">O pedido.</param>
/// <param name="NumeroDaNotaFiscal">A nota de venda.</param>
/// <param name="VendaDireta">Venda direta.</param>
/// <param name="RepasseDireto">Repasse direto.</param>
/// <param name="UnidadeNaOrigem">A unidade que vendeu, como a origem escreve.</param>
/// <param name="UnidadeDoFaturamentoNaOrigem">A unidade que faturou, como a origem escreve.</param>
/// <param name="Transformacoes">O que a leitura mudou.</param>
/// <param name="ImportadaEm">Quando entrou no CRM.</param>
/// <param name="AtualizadaPelaOrigemEm">A última mudança vinda da origem.</param>
public sealed record VendaDaMaquinaComContexto(
    Guid Chave,
    string SistemaCodigo,
    string ChaveOrigem,
    DateOnly? VendidaEm,
    DateOnly? FaturadaEm,
    DateOnly? EntregueEm,
    DateTime? RegistradaNaOrigemEm,
    string FilialCodigo,
    string FilialNome,
    string? FilialDoFaturamentoCodigo,
    Guid? CompradorChave,
    string? CompradorNome,
    string? Natureza,
    DateOnly? VinculoReferenciaEm,
    DateTime? VinculoEncerradoEm,
    string? MotivoDoEncerramento,
    string LinhaNaOrigem,
    string ProdutoNaOrigem,
    string? GestaoNaOrigem,
    string? SituacaoNaOrigem,
    string? NumeroDoPedido,
    string? NumeroDaNotaFiscal,
    bool VendaDireta,
    bool RepasseDireto,
    string? UnidadeNaOrigem,
    string? UnidadeDoFaturamentoNaOrigem,
    string? Transformacoes,
    DateTime ImportadaEm,
    DateTime? AtualizadaPelaOrigemEm);

/// <summary>Uma máquina que o cliente comprou numa venda registrada — a partir do vínculo.</summary>
/// <param name="EquipamentoChave">O GUID público da máquina.</param>
/// <param name="Chassi">O chassi.</param>
/// <param name="ModeloNome">O modelo do catálogo, quando há correspondência.</param>
/// <param name="ClassificacaoNome">A classificação de produto.</param>
/// <param name="ProdutoNaOrigem">O produto como a origem da venda escreve.</param>
/// <param name="VendidaEm">A data da venda.</param>
/// <param name="Natureza">A natureza do vínculo: CompradorNaVenda.</param>
/// <param name="FilialCodigo">A filial da venda.</param>
/// <param name="SistemaCodigo">O sistema de origem.</param>
/// <param name="EhDonoAtual">Se o cliente também é o dono atual registrado da máquina.</param>
public sealed record MaquinaCompradaPeloCliente(
    Guid EquipamentoChave,
    string Chassi,
    string? ModeloNome,
    string? ClassificacaoNome,
    string? ProdutoNaOrigem,
    DateOnly? VendidaEm,
    string Natureza,
    string FilialCodigo,
    string SistemaCodigo,
    bool EhDonoAtual);

/// <summary>O histórico comercial de uma máquina e as compras de um cliente.</summary>
public interface IRepositorioHistoricoComercial
{
    /// <summary>
    /// As vendas da máquina, da mais recente para a mais antiga. Nulo quando a máquina não existe ou
    /// está fora do alcance de quem consulta.
    /// </summary>
    /// <param name="chaveDoEquipamento">O GUID público da máquina.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<VendaDaMaquinaComContexto>?> ListarVendasAsync(Guid chaveDoEquipamento, CancellationToken ct);

    /// <summary>
    /// As máquinas que o cliente comprou (vínculo "comprador na venda" em vigor), da venda mais recente
    /// para a mais antiga. Nulo quando o cliente não existe ou está fora do alcance.
    /// </summary>
    /// <param name="chaveDoCliente">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<MaquinaCompradaPeloCliente>?> ListarMaquinasCompradasAsync(Guid chaveDoCliente, CancellationToken ct);
}

/// <summary>Uma execução de sincronização, como a administração a mostra.</summary>
/// <param name="Maquina">A máquina que executou.</param>
/// <param name="IniciadaEm">Quando começou (UTC).</param>
/// <param name="TerminadaEm">Quando terminou (UTC).</param>
/// <param name="Resultado">EmAndamento, Sucesso, Falha ou Ignorada.</param>
/// <param name="Tentativas">Tentativas feitas.</param>
/// <param name="RegistrosLidos">Registros lidos.</param>
/// <param name="Incluidos">Incluídos.</param>
/// <param name="Atualizados">Atualizados.</param>
/// <param name="Pendentes">Pendentes.</param>
/// <param name="Mensagem">Resumo ou motivo.</param>
public sealed record ExecucaoParaConsulta(
    string Maquina,
    DateTime IniciadaEm,
    DateTime? TerminadaEm,
    string Resultado,
    int Tentativas,
    int RegistrosLidos,
    int Incluidos,
    int Atualizados,
    int Pendentes,
    string? Mensagem);

/// <summary>A situação de um fluxo de sincronização: a última execução, o último sucesso e as recentes.</summary>
/// <param name="SistemaCodigo">O sistema. Ex.: ART.</param>
/// <param name="Fluxo">O fluxo.</param>
/// <param name="UltimaExecucaoEm">Quando a última execução começou.</param>
/// <param name="UltimoResultado">Como ela terminou.</param>
/// <param name="UltimoSucessoEm">Quando terminou a última execução com sucesso.</param>
/// <param name="Execucoes">As execuções recentes, da mais nova para a mais antiga.</param>
public sealed record SituacaoDaSincronizacao(
    string SistemaCodigo,
    string Fluxo,
    DateTime? UltimaExecucaoEm,
    string? UltimoResultado,
    DateTime? UltimoSucessoEm,
    IReadOnlyList<ExecucaoParaConsulta> Execucoes);

/// <summary>O registro das sincronizações, para a administração.</summary>
public interface IRepositorioSincronizacoes
{
    /// <summary>Cada fluxo com as execuções mais recentes.</summary>
    /// <param name="execucoesPorFluxo">Quantas execuções trazer de cada fluxo.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<SituacaoDaSincronizacao>> ListarAsync(int execucoesPorFluxo, CancellationToken ct);
}
