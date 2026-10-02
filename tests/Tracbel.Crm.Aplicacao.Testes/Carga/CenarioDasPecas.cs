using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;
using static Tracbel.Crm.Aplicacao.Testes.Carga.CenarioDasOrdensDeServico;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UM BALCÃO DE PEÇAS DE MENTIRA, sobre a oficina das ordens de serviço (o cliente com documento em Ribeirão). Documentos,
/// vendedores e números inventados. O mesmo cenário serve ao SQLite e ao contêiner.
/// </summary>
internal static class CenarioDasPecas
{
    /// <summary>Uma linha do faturamento como o banco a soma.</summary>
    public static PecasFaturadasNaOrigem Peca(
        string filial = "01", int mes = 9, string? documento = DocumentoDoCliente, string setor = "BALCAO", string origem = "FAT_PECAS",
        string linha = "PECAS JD", string? familia = "9999", string vendedor = "000123", string? cortesia = "NAO", string? ordem = "VEN",
        decimal quantidade = 3m, decimal valor = 1000m, decimal desconto = 50m, decimal tabela = 1100m, int itens = 2) =>
        new(filial, new DateOnly(2026, mes, 1), documento, setor, origem, linha, familia, vendedor, $"VENDEDOR {vendedor}", cortesia, ordem,
            "VENDA", quantidade, valor, desconto, tabela, itens);

    /// <summary>Um item de orçamento como a view manda.</summary>
    public static ItemDeOrcamentoNaOrigem ItemDeOrcamento(
        string filial = "01", string numero = "000777", string? situacao = "Aberto", string? documento = DocumentoDoCliente,
        DateOnly? orcadoEm = null, DateOnly? validoAte = null, string item = "PECA-1", decimal total = 300m, decimal desconto = 10m) =>
        new(filial, numero, situacao, "NO PRAZO", "RESERVADO", "BALCAO", "VENDA", documento, orcadoEm ?? new DateOnly(2026, 9, 20),
            validoAte ?? new DateOnly(2026, 10, 20), null, "000123", "VENDEDOR 000123", item, total, desconto);

    /// <summary>
    /// O faturamento padrão: em Ribeirão, peças no balcão (com uma devolução na mesma combinação) e pneus na oficina por
    /// cortesia (a quantidade não conta); em Ituverava, de quem não é cliente do CRM; e uma filial que o CRM não tem.
    /// </summary>
    public static List<PecasFaturadasNaOrigem> FaturamentoPadrao() =>
    [
        Peca(),
        Peca(origem: "DEV_PECAS", quantidade: -1m, valor: -200m, desconto: 0m, tabela: -220m, itens: 1),
        Peca(setor: "OFICINA", linha: "PNEUS/RODADOS", cortesia: "SIM", ordem: "OFI", quantidade: 2m, valor: 500m, desconto: 0m, tabela: 500m, itens: 1),
        Peca(filial: "05", documento: "52998224725", valor: 300m, quantidade: 1m, desconto: 0m, tabela: 300m, itens: 1),
        Peca(filial: "99", valor: 77m, itens: 1)
    ];

    /// <summary>Os orçamentos padrão: um aberto de dois itens do cliente, e um encerrado de quem não é cliente.</summary>
    public static List<ItemDeOrcamentoNaOrigem> OrcamentosPadrao() =>
    [
        ItemDeOrcamento(item: "PECA-1", total: 300m),
        ItemDeOrcamento(item: "PECA-2", total: 200m),
        ItemDeOrcamento(filial: "05", numero: "000778", situacao: "Encerrado", documento: "52998224725", orcadoEm: new DateOnly(2026, 6, 1),
            validoAte: new DateOnly(2026, 7, 1), total: 100m, desconto: 0m)
    ];

    /// <summary>A leitura, com as janelas do instante dado.</summary>
    public static LeituraDasPecas LeituraDePecas(
        DateTime quando, IEnumerable<PecasFaturadasNaOrigem>? faturamento = null, IEnumerable<ItemDeOrcamentoNaOrigem>? orcamentos = null)
    {
        var hoje = DateOnly.FromDateTime(quando);
        return new LeituraDasPecas(CargaDasOrdensDeServicoDoProtheus.InicioDaJanela(hoje), CargaDasPecasDoProtheus.InicioDosOrcamentos(hoje),
            [.. faturamento ?? FaturamentoPadrao()], [.. orcamentos ?? OrcamentosPadrao()]);
    }

    /// <summary>A carga sobre a leitura dada.</summary>
    public static CargaDasPecasDoProtheus CargaDePecas(Func<CrmDbContext> abrir, LeituraDasPecas leitura, DateTime agora, long operador) =>
        new(abrir, (_, _, _) => Task.FromResult(Resultado<LeituraDasPecas>.Ok(leitura)), operador, () => agora, _ => { });
}
