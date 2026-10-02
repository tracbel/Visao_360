using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

// =================================================================================================
// AS PEÇAS DO PROTHEUS (pedido do Ricardo em 02/10/2026) — a segunda parte do pós-venda, depois das ordens de serviço.
//
//   FaturamentoDePecasNoMes — o faturamento de peças da view X_V_BI_FATURAMENTO_PECAS (a do extrator "Faturamento Peças"
//                             do BI), por mês, filial, cliente, setor (balcão, oficina), grupo comercial (peças, pneus,
//                             lubrificantes…), linha do produto e vendedor. Hoje o CRM só sabia quanto da nota é peça.
//   OrcamentoDePecas        — o orçamento de peças da view X_V_BI_POSICAO_ORC_PECAS, um por orçamento, com a situação,
//                             a validade e o total: o que o cliente pediu preço e ainda não levou.
//
// O QUE NÃO ENTRA, de propósito: o custo médio, a margem, os impostos e o frete (é custo, como o lucro e a margem do ART e
// o custo do estoque, que o CRM também não lê); o nome, a cidade e o documento do cliente (o documento serve só para casar
// com o cadastro do CRM e é descartado); e o produto item a item, que fica no ERP.
// =================================================================================================

/// <summary>Uma combinação do faturamento de peças num mês, já somada e casada com o CRM.</summary>
/// <param name="EmpresaId">A filial que faturou — a fronteira de acesso.</param>
/// <param name="Competencia">O mês da emissão da nota, no dia 1.</param>
/// <param name="ClienteId">O cliente do CRM, casado pelo CPF/CNPJ; nulo quando não casa.</param>
/// <param name="Setor">O setor da venda como o Protheus escreve (balcão, oficina…).</param>
/// <param name="Grupo">O grupo comercial, na régua do painel do BI: peças, pneus, baterias, lubrificantes…</param>
/// <param name="Linha">A linha do produto como o Protheus escreve.</param>
/// <param name="VendedorCodigo">O código do vendedor no Protheus (SA3).</param>
/// <param name="VendedorNome">O nome do vendedor.</param>
/// <param name="Quantidade">A quantidade — zero na cortesia e no complemento de preço, como o BI.</param>
/// <param name="ValorLiquido">O valor líquido dos itens: o faturamento do painel, já com as devoluções descontadas.</param>
/// <param name="ValorDeDevolucoes">Quanto do valor líquido é devolução — as linhas que o BI marca como <c>DEV_</c>.</param>
/// <param name="ValorDeDesconto">O desconto concedido.</param>
/// <param name="ValorDeTabela">O valor pela tabela de preço.</param>
/// <param name="Itens">Quantos itens de nota.</param>
public sealed record DadosDoFaturamentoDePecas(
    int EmpresaId,
    DateOnly Competencia,
    long? ClienteId,
    string Setor,
    string Grupo,
    string Linha,
    string? VendedorCodigo,
    string? VendedorNome,
    decimal Quantidade,
    decimal ValorLiquido,
    decimal ValorDeDevolucoes,
    decimal ValorDeDesconto,
    decimal ValorDeTabela,
    int Itens);

/// <summary>
/// O FATURAMENTO DE PEÇAS NUM MÊS — uma combinação de filial, cliente, setor, grupo comercial, linha e vendedor, somada.
///
/// <para><b>O grão é o mês, e não o item</b> — o mesmo do faturamento do cliente (<see cref="FaturamentoDoCliente"/>): as
/// perguntas da ficha e do painel ("quanto comprou de peça em balcão e em oficina, de quê, com quem") o mês responde igual,
/// e item a item seriam centenas de milhares de linhas. É apuração: a rotina a regrava inteira na janela de três anos a cada
/// rodada, e por isso não tem trilha nem exclusão lógica.</para>
/// </summary>
public sealed class FaturamentoDePecasNoMes : EntidadeBase
{
    /// <summary>O tamanho do setor e do grupo.</summary>
    public const int TamanhoDoTextoCurto = 60;

    /// <summary>O tamanho da linha do produto e do nome do vendedor.</summary>
    public const int TamanhoDoTexto = 120;

    /// <summary>O tamanho do código do vendedor.</summary>
    public const int TamanhoDoCodigo = 20;

    /// <summary>O texto que o BI usa para o que veio sem classificação.</summary>
    public const string SemClassificacao = "S/CLASSIFICAÇÃO";

    /// <summary>O fluxo da carga — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "PROTHEUS.FATURAMENTO_PECAS";

    private FaturamentoDePecasNoMes() { }

    /// <summary>A filial que faturou — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de onde veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O mês, no dia 1.</summary>
    public DateOnly Competencia { get; private set; }

    /// <summary>O cliente do CRM; nulo quando o documento não casou.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>O setor da venda.</summary>
    public string Setor { get; private set; } = default!;

    /// <summary>O grupo comercial.</summary>
    public string Grupo { get; private set; } = default!;

    /// <summary>A linha do produto.</summary>
    public string Linha { get; private set; } = default!;

    /// <summary>O código do vendedor no Protheus.</summary>
    public string? VendedorCodigo { get; private set; }

    /// <summary>O nome do vendedor.</summary>
    public string? VendedorNome { get; private set; }

    /// <summary>A quantidade.</summary>
    public decimal Quantidade { get; private set; }

    /// <summary>O valor líquido, com as devoluções descontadas.</summary>
    public decimal ValorLiquido { get; private set; }

    /// <summary>Quanto do valor líquido é devolução.</summary>
    public decimal ValorDeDevolucoes { get; private set; }

    /// <summary>O desconto.</summary>
    public decimal ValorDeDesconto { get; private set; }

    /// <summary>O valor de tabela.</summary>
    public decimal ValorDeTabela { get; private set; }

    /// <summary>Os itens de nota.</summary>
    public int Itens { get; private set; }

    /// <summary>Quando a rodada que a apurou leu o Protheus (UTC).</summary>
    public DateTime ApuradoEm { get; private set; }

    /// <summary>Apura uma combinação.</summary>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="dados">A combinação somada.</param>
    /// <param name="apuradoEm">A leitura (UTC).</param>
    /// <param name="criadoPorId">Quem rodou a carga.</param>
    public static FaturamentoDePecasNoMes Apurar(int sistemaId, DadosDoFaturamentoDePecas dados, DateTime apuradoEm, long criadoPorId)
    {
        if (dados.Competencia.Day != 1) throw new RegraDeNegocioViolada("O faturamento de peças guarda o mês, no dia 1.");
        if (dados.Itens <= 0) throw new RegraDeNegocioViolada("Uma combinação do faturamento de peças tem pelo menos um item.");

        return new FaturamentoDePecasNoMes
        {
            SistemaId = sistemaId,
            EmpresaId = dados.EmpresaId,
            Competencia = dados.Competencia,
            ClienteId = dados.ClienteId,
            Setor = Obrigatorio(dados.Setor, TamanhoDoTextoCurto, "o setor"),
            Grupo = Obrigatorio(dados.Grupo, TamanhoDoTextoCurto, "o grupo"),
            Linha = Obrigatorio(dados.Linha, TamanhoDoTexto, "a linha do produto"),
            VendedorCodigo = Opcional(dados.VendedorCodigo, TamanhoDoCodigo, "o código do vendedor"),
            VendedorNome = Opcional(dados.VendedorNome, TamanhoDoTexto, "o nome do vendedor"),
            Quantidade = decimal.Round(dados.Quantidade, 3),
            ValorLiquido = decimal.Round(dados.ValorLiquido, 2),
            ValorDeDevolucoes = decimal.Round(dados.ValorDeDevolucoes, 2),
            ValorDeDesconto = decimal.Round(dados.ValorDeDesconto, 2),
            ValorDeTabela = decimal.Round(dados.ValorDeTabela, 2),
            Itens = dados.Itens,
            ApuradoEm = apuradoEm,
            CriadoPorId = criadoPorId
        };
    }

    private static string Obrigatorio(string texto, int tamanho, string oQue)
    {
        var limpo = texto?.Trim() ?? string.Empty;
        if (limpo.Length == 0 || limpo.Length > tamanho)
            throw new RegraDeNegocioViolada($"O faturamento de peças preserva {oQue} da origem, de 1 a {tamanho} caracteres.");
        return limpo;
    }

    private static string? Opcional(string? texto, int tamanho, string oQue)
    {
        var limpo = texto?.Trim();
        if (string.IsNullOrEmpty(limpo)) return null;
        if (limpo.Length > tamanho) throw new RegraDeNegocioViolada($"O faturamento de peças preserva {oQue} da origem, até {tamanho} caracteres.");
        return limpo;
    }
}

/// <summary>O que um orçamento de peças traz da origem, já somado e casado com o CRM.</summary>
/// <param name="EmpresaId">A filial do orçamento — a fronteira de acesso.</param>
/// <param name="Numero">O número do orçamento.</param>
/// <param name="ClienteId">O cliente do CRM, casado pelo CPF/CNPJ; nulo quando não casa.</param>
/// <param name="Situacao">A situação como o Protheus escreve: Aberto, Parcialmente Atendido, Encerrado…</param>
/// <param name="Prazo">No prazo, Vencido ou Atendido, como o Protheus escreve.</param>
/// <param name="Reserva">Reservado, Não reservado ou Parcialmente reservado.</param>
/// <param name="TipoDeAtendimento">O tipo de atendimento.</param>
/// <param name="TipoDeOrcamento">O tipo do orçamento.</param>
/// <param name="OrcadoEm">A data do orçamento.</param>
/// <param name="ValidoAte">A validade.</param>
/// <param name="AlteradoNaOrigemEm">A última alteração na origem.</param>
/// <param name="VendedorCodigo">O código do vendedor no Protheus.</param>
/// <param name="VendedorNome">O nome do vendedor.</param>
/// <param name="ValorTotal">O total do orçamento, já sem desconto.</param>
/// <param name="ValorDeDesconto">O desconto.</param>
/// <param name="Itens">Quantos itens.</param>
/// <param name="HashDaOrigem">O resumo do conteúdo lido.</param>
public sealed record DadosDoOrcamentoDePecas(
    int EmpresaId,
    string Numero,
    long? ClienteId,
    string Situacao,
    string? Prazo,
    string? Reserva,
    string? TipoDeAtendimento,
    string? TipoDeOrcamento,
    DateOnly OrcadoEm,
    DateOnly? ValidoAte,
    DateOnly? AlteradoNaOrigemEm,
    string? VendedorCodigo,
    string? VendedorNome,
    decimal ValorTotal,
    decimal ValorDeDesconto,
    int Itens,
    string HashDaOrigem);

/// <summary>
/// UM ORÇAMENTO DE PEÇAS — o cabeçalho, com o total dos itens. A chave é a filial e o número. É espelho do Protheus: o CRM
/// não edita; o que some da origem dentro da janela é excluído logicamente, e o que volta é reativado.
/// </summary>
public sealed class OrcamentoDePecas : EntidadeBase
{
    /// <summary>O tamanho da chave na origem.</summary>
    public const int TamanhoDaChave = 40;

    /// <summary>O tamanho do número.</summary>
    public const int TamanhoDoNumero = 20;

    /// <summary>O tamanho dos textos curtos (situação, prazo, reserva, tipos).</summary>
    public const int TamanhoDoTextoCurto = 60;

    /// <summary>O tamanho do nome do vendedor.</summary>
    public const int TamanhoDoTexto = 120;

    /// <summary>O fluxo da carga — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "PROTHEUS.ORCAMENTOS_PECAS";

    private OrcamentoDePecas() { }

    /// <summary>A filial — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de onde veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary>A chave natural: <c>FILIAL|NUMERO</c>.</summary>
    public string ChaveNaOrigem { get; private set; } = default!;

    /// <summary>O número.</summary>
    public string Numero { get; private set; } = default!;

    /// <summary>O cliente do CRM; nulo quando não casou.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>A situação, como o Protheus escreve.</summary>
    public string Situacao { get; private set; } = default!;

    /// <summary>No prazo, vencido ou atendido.</summary>
    public string? Prazo { get; private set; }

    /// <summary>A reserva de estoque.</summary>
    public string? Reserva { get; private set; }

    /// <summary>O tipo de atendimento.</summary>
    public string? TipoDeAtendimento { get; private set; }

    /// <summary>O tipo do orçamento.</summary>
    public string? TipoDeOrcamento { get; private set; }

    /// <summary>A data do orçamento.</summary>
    public DateOnly OrcadoEm { get; private set; }

    /// <summary>A validade.</summary>
    public DateOnly? ValidoAte { get; private set; }

    /// <summary>A última alteração na origem.</summary>
    public DateOnly? AlteradoNaOrigemEm { get; private set; }

    /// <summary>O código do vendedor.</summary>
    public string? VendedorCodigo { get; private set; }

    /// <summary>O nome do vendedor.</summary>
    public string? VendedorNome { get; private set; }

    /// <summary>O total, sem desconto.</summary>
    public decimal ValorTotal { get; private set; }

    /// <summary>O desconto.</summary>
    public decimal ValorDeDesconto { get; private set; }

    /// <summary>Os itens.</summary>
    public int Itens { get; private set; }

    /// <summary>O resumo do conteúdo lido.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>A leitura que trouxe o conteúdo atual (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>Se o orçamento ainda espera o cliente — aberto ou parcialmente atendido.</summary>
    public bool EstaEmAberto => EstaEmAbertoNa(Situacao);

    /// <summary>Se a situação é de orçamento ainda em aberto.</summary>
    /// <param name="situacao">A situação, como o Protheus escreve.</param>
    public static bool EstaEmAbertoNa(string? situacao) =>
        string.Equals(situacao?.Trim(), "Aberto", StringComparison.OrdinalIgnoreCase)
        || string.Equals(situacao?.Trim(), "Parcialmente Atendido", StringComparison.OrdinalIgnoreCase);

    /// <summary>A chave natural: a filial e o número, aparados.</summary>
    public static string ChaveDe(string filial, string numero) => $"{filial.Trim()}|{numero.Trim()}";

    /// <summary>Registra um orçamento lido da origem.</summary>
    public static OrcamentoDePecas Registrar(int sistemaId, string chaveNaOrigem, DadosDoOrcamentoDePecas dados, DateTime lidaEm, long criadoPorId)
    {
        var chave = chaveNaOrigem?.Trim() ?? string.Empty;
        if (chave.Length is 0 or > TamanhoDaChave)
            throw new RegraDeNegocioViolada($"O orçamento de peças precisa da chave da origem (filial e número), de 1 a {TamanhoDaChave} caracteres.");

        var orcamento = new OrcamentoDePecas { SistemaId = sistemaId, ChaveNaOrigem = chave, CriadoPorId = criadoPorId };
        orcamento.Aplicar(dados, lidaEm);
        return orcamento;
    }

    /// <summary>Aplica uma nova leitura; nada muda quando o resumo, a filial e o cliente casado são os mesmos.</summary>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(DadosDoOrcamentoDePecas dados, DateTime lidaEm, long usuarioId)
    {
        if (dados.HashDaOrigem == HashDaOrigem && dados.EmpresaId == EmpresaId && dados.ClienteId == ClienteId) return false;
        Aplicar(dados, lidaEm);
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>Traz de volta o orçamento que tinha sumido da origem.</summary>
    public bool Reativar(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    private void Aplicar(DadosDoOrcamentoDePecas dados, DateTime lidaEm)
    {
        if (string.IsNullOrWhiteSpace(dados.HashDaOrigem)) throw new RegraDeNegocioViolada("O orçamento de peças guarda o resumo do conteúdo lido.");
        if (dados.Itens <= 0) throw new RegraDeNegocioViolada("O orçamento de peças tem pelo menos um item.");

        var numero = dados.Numero?.Trim() ?? string.Empty;
        if (numero.Length is 0 or > TamanhoDoNumero)
            throw new RegraDeNegocioViolada($"O orçamento de peças preserva o número da origem, de 1 a {TamanhoDoNumero} caracteres.");
        var situacao = dados.Situacao?.Trim() ?? string.Empty;
        if (situacao.Length is 0 or > TamanhoDoTextoCurto)
            throw new RegraDeNegocioViolada($"O orçamento de peças preserva a situação da origem, de 1 a {TamanhoDoTextoCurto} caracteres.");

        EmpresaId = dados.EmpresaId;
        Numero = numero;
        ClienteId = dados.ClienteId;
        Situacao = situacao;
        Prazo = Opcional(dados.Prazo, TamanhoDoTextoCurto, "o prazo");
        Reserva = Opcional(dados.Reserva, TamanhoDoTextoCurto, "a reserva");
        TipoDeAtendimento = Opcional(dados.TipoDeAtendimento, TamanhoDoTextoCurto, "o tipo de atendimento");
        TipoDeOrcamento = Opcional(dados.TipoDeOrcamento, TamanhoDoTextoCurto, "o tipo do orçamento");
        OrcadoEm = dados.OrcadoEm;
        ValidoAte = dados.ValidoAte;
        AlteradoNaOrigemEm = dados.AlteradoNaOrigemEm;
        VendedorCodigo = Opcional(dados.VendedorCodigo, FaturamentoDePecasNoMes.TamanhoDoCodigo, "o código do vendedor");
        VendedorNome = Opcional(dados.VendedorNome, TamanhoDoTexto, "o nome do vendedor");
        ValorTotal = decimal.Round(dados.ValorTotal, 2);
        ValorDeDesconto = decimal.Round(dados.ValorDeDesconto, 2);
        Itens = dados.Itens;
        HashDaOrigem = dados.HashDaOrigem;
        LidaEm = lidaEm;
    }

    private static string? Opcional(string? texto, int tamanho, string oQue)
    {
        var limpo = texto?.Trim();
        if (string.IsNullOrEmpty(limpo)) return null;
        if (limpo.Length > tamanho) throw new RegraDeNegocioViolada($"O orçamento de peças preserva {oQue} da origem, até {tamanho} caracteres.");
        return limpo;
    }
}
