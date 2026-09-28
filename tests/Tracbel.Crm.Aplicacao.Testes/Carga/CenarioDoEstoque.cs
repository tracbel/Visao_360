using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UM ESTOQUE DE MENTIRA, para a carga do estoque e da cobertura da API Gestão de Negócios — sobre a semente das metas
/// (Ribeirão 010101 e Ituverava 010105). Chassis e pedidos inventados.
/// </summary>
internal static class CenarioDoEstoque
{
    /// <summary>O de-para das lojas: as duas filiais da semente e a Digital, sem código no TOTVS.</summary>
    public static IReadOnlyList<FilialDaGestao> Filiais() =>
    [
        new(1, "Ribeirão Preto", "010101"),
        new(5, "Ituverava", "010105"),
        new(0, "Digital", null)
    ];

    /// <summary>Uma máquina como o painel manda.</summary>
    public static EquipamentoNaOrigem Maquina(
        string chaint, string filial = "Ituverava", string situacao = "Estoque", string grupo = "TRATOR 6000", string reservado = "Não",
        string? entrada = "2026-06-01", string? chegada = null, string pago = "Sim", string novoUsado = "NOVO") =>
        new(chaint, situacao == "PEDIDO" ? null : $"1PY{chaint}", $"45{chaint}", null, filial, situacao, grupo, "TR 6155M", null, "MÁQUINA",
            novoUsado, "2026/2026", entrada, chegada, null, pago, reservado, situacao == "PEDIDO" ? "Confirmado" : null);

    /// <summary>O estoque: duas no pátio de Ituverava (uma reservada), uma em Ribeirão, um pedido e uma sem filial.</summary>
    public static List<EquipamentoNaOrigem> Estoque() =>
    [
        Maquina("100001"),
        Maquina("100002", reservado: "Sim"),
        Maquina("100003", filial: "Ribeirão Preto", grupo: "AMS"),
        Maquina("100004", situacao: "PEDIDO", entrada: null, chegada: "2026-11-30", pago: "Não"),
        Maquina("100005", filial: "Digital")
    ];

    /// <summary>Um estoque grande e só válido — para a trava de remoção.</summary>
    public static List<EquipamentoNaOrigem> EstoqueGrande(int quantas) =>
        [.. Enumerable.Range(1, quantas).Select(i => Maquina((200000 + i).ToString(System.Globalization.CultureInfo.InvariantCulture)))];

    /// <summary>A cobertura: dois meses e dois grupos.</summary>
    public static List<ItemDaCoberturaNaOrigem> Cobertura() =>
    [
        new("MES", "Ago/2026", "3.12", "152"),
        new("MES", "Jul/2026", "3.59", "143"),
        new("GRUPO", "AMS", "3.55", "512"),
        new("GRUPO", "TRATOR 6000", "2.25", "165")
    ];

    /// <summary>A leitura inteira.</summary>
    public static LeituraDoEstoqueNaOrigem Leitura(IReadOnlyList<EquipamentoNaOrigem>? estoque = null, IReadOnlyList<ItemDaCoberturaNaOrigem>? cobertura = null) =>
        new(estoque ?? Estoque(), cobertura ?? Cobertura(), Filiais(), new DateTime(2026, 9, 28, 11, 2, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 28, 11, 2, 0, DateTimeKind.Utc));

    /// <summary>A carga sobre a leitura dada.</summary>
    public static CargaDoEstoqueDaGestaoDeNegocios Sincronia(Func<CrmDbContext> abrir, LeituraDoEstoqueNaOrigem leitura, DateTime agora, long operador) =>
        new(abrir, _ => Task.FromResult(Resultado<LeituraDoEstoqueNaOrigem>.Ok(leitura)), operador, () => agora, _ => { });
}
