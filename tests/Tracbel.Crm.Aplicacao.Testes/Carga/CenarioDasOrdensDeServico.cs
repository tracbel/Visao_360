using Tracbel.Crm.Carga;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Protheus;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// UMA OFICINA DE MENTIRA, para a carga das ordens de serviço: um cliente com documento e uma máquina com chassi em
/// Ribeirão, sobre a semente das metas (as filiais 010101 e 010105 e o operador). Documentos e chassis inventados. O mesmo
/// cenário serve ao SQLite e ao contêiner.
/// </summary>
internal static class CenarioDasOrdensDeServico
{
    public const string DocumentoDoCliente = "11444777000161";
    public const string ChassiDaMaquina = "1PY6155MCSS000001";

    /// <summary>Semeia o cliente e a máquina; devolve os dois identificadores.</summary>
    public static (long ClienteId, long MaquinaId) SemearAOficina(CrmDbContext db, SementeDasMetas semente)
    {
        var cliente = Cliente.Criar(semente.RibeiraoPreto, "Cliente da oficina", TipoDePessoa.Juridica, semente.Operador, semente.Operador,
            documento: CpfCnpj.Criar(DocumentoDoCliente), situacao: SituacaoDoCliente.Cliente);
        db.Clientes.Add(cliente);
        var maquina = Equipamento.RegistrarPelaIntegracao(semente.RibeiraoPreto, Chassi.Criar(ChassiDaMaquina), OrigemDoEquipamento.Protheus, semente.Operador);
        db.Equipamentos.Add(maquina);
        db.SaveChanges();
        return (cliente.Id, maquina.Id);
    }

    /// <summary>Um item de OS como a view manda.</summary>
    public static ItemDaOrdemNaOrigem Item(
        string filial, string numero, string status, DateOnly aberta, DateOnly? fechada = null, string? documento = DocumentoDoCliente,
        string? chassi = "1py6155mcss000001", string codigo = "PECA-1", string requisicao = "00000011", decimal quantidade = 1m,
        decimal unitario = 100m, decimal desconto = 0m, string pecaOuServico = "PEÇ") =>
        new(filial, numero, status, "OFICINA", aberta, null, null, fechada, chassi, "6155M", 1200m, documento, pecaOuServico, "C", codigo,
            requisicao, quantidade, unitario, desconto, null, "1001", null, null);

    /// <summary>Um serviço executado: 2 h cobradas a R$ 150, R$ 30 de desconto sobre 4 h — R$ 285 na régua do BI.</summary>
    public static ServicoExecutadoNaOrigem Servico(string filial, string numero, string tipTem = "C") =>
        new(filial, numero, "TEC01", new DateOnly(2026, 9, 1), "C", "CLIENTE", "MO", "AG", "REVISAO", null, null, "1", 0m, "1", tipTem,
            2m, 2m, 2m, 2m, 4m, 4m, 4m, 4m, 0m, 0m, 30m, 0m, 0m, 0m, 80m, 150m, 0m, 0m, 0m, 0m);

    /// <summary>
    /// Os itens padrão: A aberta em Ribeirão (cliente e máquina do CRM, duas requisições da mesma peça com o desconto
    /// repetido), B fechada em Ituverava (proprietário fora do CRM) e C de uma filial que o CRM não tem.
    /// </summary>
    public static List<ItemDaOrdemNaOrigem> ItensPadrao() =>
    [
        Item("010101", "00000001", "A", new DateOnly(2026, 9, 1), requisicao: "00000011", quantidade: 2m, desconto: 30m),
        Item("010101", "00000001", "A", new DateOnly(2026, 9, 1), requisicao: "00000012", quantidade: 1m, desconto: 30m),
        Item("010105", "00000002", "F", new DateOnly(2026, 5, 10), fechada: new DateOnly(2026, 5, 20), documento: "52998224725", chassi: "ZZZ999"),
        Item("019999", "00000003", "F", new DateOnly(2026, 4, 1), fechada: new DateOnly(2026, 4, 2))
    ];

    /// <summary>Os serviços padrão: o de A repetido, um FDLI em B e um de uma OS que não veio na capa.</summary>
    public static List<ServicoExecutadoNaOrigem> ServicosPadrao() =>
        [Servico("010101", "00000001"), Servico("010101", "00000001"), Servico("010105", "00000002", tipTem: "FDLI"), Servico("010101", "99999999")];

    /// <summary>A leitura, com a janela do instante dado.</summary>
    public static LeituraDasOrdensDeServico Leitura(
        DateTime quando, IEnumerable<ItemDaOrdemNaOrigem>? itens = null, IEnumerable<ServicoExecutadoNaOrigem>? servicos = null) =>
        new(CargaDasOrdensDeServicoDoProtheus.InicioDaJanela(DateOnly.FromDateTime(quando)), [.. itens ?? ItensPadrao()], [.. servicos ?? ServicosPadrao()]);

    /// <summary>A carga sobre a leitura dada; <paramref name="pedidos"/> anota a janela que a carga pediu à leitura.</summary>
    public static CargaDasOrdensDeServicoDoProtheus Sincronia(
        Func<CrmDbContext> abrir, LeituraDasOrdensDeServico leitura, DateTime agora, long operador, List<(DateOnly Desde, DateOnly? Mudancas)>? pedidos = null) =>
        new(abrir, (desde, mudancas, _) =>
        {
            pedidos?.Add((desde, mudancas));
            return Task.FromResult(Resultado<LeituraDasOrdensDeServico>.Ok(leitura));
        }, operador, () => agora, _ => { });
}
