using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>
/// O faturamento que saiu pela porta e <b>não tem dono neste CRM</b>.
///
/// ---------------------------------------------------------------------------------------------
/// POR QUE ISTO É UMA TABELA, E NÃO UMA LINHA DE LOG DA CARGA.
///
/// <para>A carga cruza o CNPJ da nota com o cadastro de clientes. O que não casa era simplesmente
/// descartado — e o descarte não era pequeno: <b>R$ 213 milhões em três anos</b>, medido em
/// 07/09/2026. Enquanto isso ficava só num contador do relatório da carga, a tela mostrava
/// R$ 620 milhões de faturamento como se fosse o total, e ninguém tinha como saber que faltava um
/// quarto do dinheiro. Guardado, o denominador aparece: a Visão 360 pode dizer <b>"o CRM gerencia
/// 66% do que a empresa vende"</b>, que é uma frase muito diferente de "vendemos R$ 620 mi".</para>
///
/// <para><b>O grão é parceiro × filial × mês</b>, igual ao de <see cref="FaturamentoDoCliente"/>.
/// Só a contraparte muda: lá é um <c>ClienteId</c>, aqui é o CNPJ cru da nota mais o nome que o
/// ERP tem para ele — porque é exatamente isso que existe.</para>
///
/// <para><b>Nem tudo aqui é falha de cadastro, e por isso existe a natureza.</b> Dos R$ 337
/// milhões sem cliente, R$ 96,6 mi são venda para a <b>John Deere</b> (a fábrica) e R$ 27,4 mi
/// para a <b>Tracbel Agro Norte</b> (empresa irmã). Nenhum dos dois é cliente, nenhum dos dois
/// deveria entrar em meta de vendedor, e nenhum dos dois é um cadastro faltando. Somá-los ao
/// faturamento comercial infla o número; classificá-los resolve. O que sobra — o produtor rural
/// que comprou R$ 3 milhões e não existe no CRM — <b>é falha de cadastro, e é acionável.</b></para>
/// </summary>
public sealed class FaturamentoSemCliente : EntidadeBase
{
    private FaturamentoSemCliente() { }

    /// <summary>A filial que faturou. É a fronteira de acesso, como em todo o modelo.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O mês de competência, sempre no dia 1.</summary>
    public DateOnly Competencia { get; private set; }

    /// <summary>O CNPJ ou CPF que está na nota, só os dígitos.</summary>
    public string Documento { get; private set; } = string.Empty;

    /// <summary>O nome que o ERP tem para essa contraparte.</summary>
    public string Nome { get; private set; } = string.Empty;

    /// <summary>O que essa contraparte é. Ver <see cref="NaturezaDoParceiro"/>.</summary>
    public NaturezaDoParceiro Natureza { get; private set; }

    /// <summary>O valor líquido faturado no mês.</summary>
    public decimal ValorLiquido { get; private set; }

    /// <summary>Quanto do mês foi máquina.</summary>
    public decimal ValorEmMaquina { get; private set; }

    /// <summary>Quanto do mês foi peça.</summary>
    public decimal ValorEmPeca { get; private set; }

    /// <summary>Quanto do mês foi serviço.</summary>
    public decimal ValorEmServico { get; private set; }

    /// <summary>O que caiu em grupo que ainda não sabemos ler.</summary>
    public decimal ValorEmOutros { get; private set; }

    /// <summary>Quantas notas fiscais distintas.</summary>
    public int Notas { get; private set; }

    /// <summary>Quantos itens de nota.</summary>
    public int Itens { get; private set; }

    /// <summary>Registra um mês de faturamento de uma contraparte sem cliente no CRM.</summary>
    /// <param name="empresaId">Filial que faturou.</param>
    /// <param name="competencia">Mês de competência.</param>
    /// <param name="documento">CNPJ ou CPF da nota, só dígitos.</param>
    /// <param name="nome">Nome da contraparte no ERP.</param>
    /// <param name="natureza">O que ela é.</param>
    /// <param name="valorLiquido">Valor líquido do mês.</param>
    /// <param name="notas">Notas distintas.</param>
    /// <param name="itens">Itens de nota.</param>
    /// <param name="quebra">Quanto foi máquina, peça, serviço e outros.</param>
    public static FaturamentoSemCliente Criar(
        int empresaId,
        DateOnly competencia,
        string documento,
        string nome,
        NaturezaDoParceiro natureza,
        decimal valorLiquido,
        int notas,
        int itens,
        QuebraDoFaturamento quebra) => new()
        {
            EmpresaId = empresaId,
            Competencia = new DateOnly(competencia.Year, competencia.Month, 1),
            Documento = documento,
            Nome = nome,
            Natureza = natureza,
            ValorLiquido = valorLiquido,
            Notas = notas,
            Itens = itens,
            ValorEmMaquina = quebra.Maquina,
            ValorEmPeca = quebra.Peca,
            ValorEmServico = quebra.Servico,
            ValorEmOutros = quebra.Outros
        };

    /// <summary>
    /// Substitui o mês pelos valores reapurados, na reconciliação da carga.
    ///
    /// <para>Substitui e não soma, pela mesma razão de
    /// <see cref="FaturamentoDoCliente.Reapurar"/>: a carga é feita para ser reexecutada.</para>
    /// </summary>
    /// <param name="nome">O nome, que o ERP pode ter corrigido.</param>
    /// <param name="natureza">A natureza reavaliada.</param>
    /// <param name="valorLiquido">O total do mês.</param>
    /// <param name="notas">As notas do mês.</param>
    /// <param name="itens">Os itens do mês.</param>
    /// <param name="quebra">A quebra do mês.</param>
    public void Reapurar(
        string nome,
        NaturezaDoParceiro natureza,
        decimal valorLiquido,
        int notas,
        int itens,
        QuebraDoFaturamento quebra)
    {
        Nome = nome;
        Natureza = natureza;
        ValorLiquido = valorLiquido;
        Notas = notas;
        Itens = itens;
        ValorEmMaquina = quebra.Maquina;
        ValorEmPeca = quebra.Peca;
        ValorEmServico = quebra.Servico;
        ValorEmOutros = quebra.Outros;
    }
}

/// <summary>
/// O que é a contraparte de uma nota que não tem cliente no CRM.
///
/// <para>É seleção, e não texto: quem lê a Visão 360 precisa somar "o que deveria estar no CRM e
/// não está" sem somar junto a venda para a fábrica. Com campo livre, cada carga escreveria um
/// rótulo diferente e a soma nunca fecharia.</para>
///
/// <para>A classificação sai do <b>CNPJ</b>, não do nome. Nome é grafado de dez jeitos —
/// "TRACBEL AGRO NOR.", "TRACBEL AGRO NORTE" —, raiz de CNPJ é uma só.</para>
/// </summary>
public enum NaturezaDoParceiro
{
    /// <summary>Ainda não classificada.</summary>
    Indefinida = 0,

    /// <summary>
    /// Deveria ser cliente e não está cadastrado.
    ///
    /// <para>É a única natureza acionável: 884 CNPJs, R$ 102,4 milhões em três anos.</para>
    /// </summary>
    ClienteNaoCadastrado = 1,

    /// <summary>
    /// A fábrica — John Deere Brasil.
    ///
    /// <para>R$ 96,6 milhões em três anos, com CFOP 5102, exatamente igual a uma venda. Só o CNPJ
    /// separa. Não é cliente e não entra em meta.</para>
    /// </summary>
    Fabrica = 2,

    /// <summary>
    /// Outra empresa do grupo Tracbel.
    ///
    /// <para>R$ 27,4 milhões. Transferência disfarçada de venda — a parte do intercompany que o
    /// CFOP de transferência não pega.</para>
    /// </summary>
    EmpresaDoGrupo = 3,

    /// <summary>
    /// Outra concessionária.
    ///
    /// <para>Repasse de máquina entre revendas. É venda, mas não é cliente final e não deve entrar
    /// em cobertura de carteira.</para>
    /// </summary>
    OutraRevenda = 4,

    /// <summary>A nota saiu sem CNPJ na origem. Não dá para classificar nem cobrar.</summary>
    SemDocumento = 5
}
