using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// OS GRUPOS DO RECORTE (plano 2 do documento 54): cada linha da apuração cai num município de São Paulo, pelo código do
/// IBGE, ou num destes grupos negativos — fora do mapa, mas dentro do total.
/// </summary>
internal static class GruposDoRecorte
{
    internal const int SemMunicipio = -1;
    internal const int MunicipioSemCodigoIbge = -2;
    internal const int OutraUf = -3;
    internal const int ClienteDeOutraFilial = -4;
    internal const int ClienteInativado = -5;
    internal const int NotaSemCadastroDeCliente = -6;
    internal const int NotaParaFabrica = -7;
    internal const int NotaParaEmpresaDoGrupo = -8;
    internal const int NotaParaOutraRevenda = -9;

    /// <summary>Grupo de quem ficou fora do recorte pedido — não é somado em lugar nenhum.</summary>
    internal const int ForaDoFiltro = int.MinValue;

    /// <summary>O município que não tem nenhuma das cinco fontes carregadas.</summary>
    internal static readonly EstruturaDoMunicipio EstruturaVazia =
        new(null, null, null, null, null, null, [], null, null, null, []);

    internal static readonly (int Grupo, string Codigo, string Descricao)[] GruposForaDoMapa =
    [
        (SemMunicipio, "SemMunicipio",
            "Cliente sem endereço principal, ou com o município do endereço fora do catálogo (o texto do legado que a carga não casou)."),
        (MunicipioSemCodigoIbge, "MunicipioSemCodigoIbge",
            "O endereço aponta para uma linha do catálogo sem código IBGE — segunda grafia do mesmo município ou distrito. Não há polígono para ela."),
        (OutraUf, "OutraUf",
            "Cliente com endereço principal fora de São Paulo."),
        (ClienteDeOutraFilial, "ClienteDeOutraFilial",
            "Venda ou vínculo desta filial com cliente CADASTRADO em outra filial. O endereço do cliente pertence ao cadastro da outra filial e fica fora do seu alcance; na visão da empresa este grupo não existe, porque cada venda vai para o município do cliente."),
        (ClienteInativado, "ClienteInativado",
            "Venda ou vínculo de cliente inativado no CRM. Fica somado à parte para o total fechar, sem voltar a aparecer no mapa."),
        (NotaSemCadastroDeCliente, "ContraparteSemCadastro",
            "Nota desta filial para quem não tem cadastro de cliente no CRM: cliente não cadastrado, nota sem documento ou contraparte ainda não classificada. Sem cadastro não há endereço nem município. Conta documentos distintos, não clientes."),
        (NotaParaFabrica, "RepasseDeFabrica",
            "Nota desta filial para a fábrica. Tem CFOP de venda, mas não é cliente final: fica à parte para o total da filial fechar sem inflar a venda a cliente."),
        (NotaParaEmpresaDoGrupo, "EmpresaDoGrupo",
            "Nota desta filial para outra empresa do grupo. Não é cliente final."),
        (NotaParaOutraRevenda, "OutraRevenda",
            "Nota desta filial para outra concessionária (repasse entre revendas). Não é cliente final.")
    ];

    /// <summary>O grupo fora do mapa de cada natureza de contraparte sem cliente no CRM.</summary>
    internal static int GrupoDaNatureza(NaturezaDoParceiro natureza) => natureza switch
    {
        NaturezaDoParceiro.Fabrica => NotaParaFabrica,
        NaturezaDoParceiro.EmpresaDoGrupo => NotaParaEmpresaDoGrupo,
        NaturezaDoParceiro.OutraRevenda => NotaParaOutraRevenda,
        _ => NotaSemCadastroDeCliente
    };
}

/// <summary>As vendas de um município num mês — o ponto do mini-gráfico, antes de somar a ADR.</summary>
internal sealed class VendasDoMes
{
    public decimal ValorLiquido;
    public decimal Maquina;
    public decimal PosVenda;
    public int MaquinasVendidas;
}

/// <summary>O contador de um grupo enquanto clientes, vínculos e notas são percorridos.</summary>
internal sealed class Acumulador
{
    public int Clientes;
    public int Vinculos;
    public int Cobertos;
    public int ForaDaCadencia;
    public int NuncaContatados;
    public int SemCadencia;
    public int ClientesQueCompraram;
    public decimal ValorLiquido;
    public decimal Maquina;
    public decimal Peca;
    public decimal Servico;
    public decimal Outros;

    /// <summary>As máquinas vendidas a clientes deste grupo, em UNIDADES — o ART (issue 69).</summary>
    public int MaquinasVendidas;

    /// <summary>Delas, as que a classificação de produto do CRM não alcança.</summary>
    public int MaquinasSemClassificacao;

    /// <summary>Delas, as de linha que existe e ainda não foi ligada a uma categoria.</summary>
    public int MaquinasEmLinhaSemCategoria;

    /// <summary>Delas, quantas em cada categoria de máquina.</summary>
    public readonly Dictionary<string, int> MaquinasPorCategoria = new(StringComparer.Ordinal);

    public readonly Dictionary<long, int> VinculosPorResponsavel = [];
    public readonly Dictionary<long, HashSet<long>> CarteirasPorResponsavel = [];

    /// <summary>Os clientes distintos com vínculo em carteira comercial — o mesmo cliente em duas carteiras conta uma vez.</summary>
    public readonly HashSet<long> ClientesComVinculo = [];

    /// <summary>Os clientes do grupo pela classe ABC: A, B, C, D e sem classe.</summary>
    private readonly int[] porClasse = new int[5];

    public void ContarClasse(ClasseDeCliente? classe) => porClasse[classe switch
    {
        ClasseDeCliente.A => 0,
        ClasseDeCliente.B => 1,
        ClasseDeCliente.C => 2,
        ClasseDeCliente.D => 3,
        _ => 4
    }]++;

    public CoberturaTerritorial Cobertura() => new(
        Clientes, Vinculos, Cobertos + ForaDaCadencia + NuncaContatados, Cobertos, ForaDaCadencia, NuncaContatados, SemCadencia,
        new ClientesPorClasse(porClasse[0], porClasse[1], porClasse[2], porClasse[3], porClasse[4]),
        ClientesComVinculo.Count);

    public VendasTerritoriais Vendas() => new(
        ClientesQueCompraram, decimal.Round(ValorLiquido, 2), decimal.Round(Maquina, 2),
        decimal.Round(Peca, 2), decimal.Round(Servico, 2), decimal.Round(Outros, 2));
}
