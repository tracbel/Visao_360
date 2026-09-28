using System.Globalization;
using System.Text;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O FINANCIAMENTO DE UMA VENDA DE MÁQUINA DA TRACBEL, como o CEN o registrou no formulário da venda do Vórtice
/// (issue 262, decisão do Ricardo em 28/09/2026: "Formulários do Vórtice").
///
/// <para><b>Por que existe.</b> O share da Tracbel no crédito de mecanização é o valor que ela financiou dividido pelo
/// crédito de máquinas que o SICOR registrou no mesmo território e na mesma janela. O SICOR não identifica revenda
/// (D-P09); o numerador só existe do lado de cá, e o único lugar em que a Tracbel o escreveu é o formulário da venda.</para>
///
/// <para><b>Um por processo do Vórtice.</b> O mesmo processo aparece em mais de um formulário — o
/// <c>IV_Q_ACOMPANHAMENTO_VENDA</c> é cópia do <c>IV_Q_ACOMPANH_VENDA_JDE</c> (1.631 processos em comum, o mesmo valor
/// em 1.301 de 1.303, medido em 28/09/2026). A leitura escolhe um formulário por processo, do mais novo ao mais
/// antigo (<see cref="PrioridadeDoFormulario"/>).</para>
///
/// <para><b>Não identifica o cliente.</b> Guarda o processo, o município, a data, o valor, a instituição e a linha —
/// nem nome, nem documento, nem o texto livre do formulário.</para>
///
/// <para><b>Não é o contrato do banco.</b> A data é a do pedido de venda, e não a da emissão da cédula; o município é o
/// do cadastro do cliente, e não o do empreendimento que o SICOR registra. Por isso o share é lido por filial e na
/// janela de 12 meses, e o município é só indicativo.</para>
/// </summary>
public sealed class FinanciamentoDaVenda
{
    private FinanciamentoDaVenda() { }

    /// <summary>O fluxo da carga, na trilha de execuções.</summary>
    public const string FluxoDaCarga = "VORTICE.FINANCIAMENTO_DA_VENDA";

    /// <summary>Tamanho do nome do formulário.</summary>
    public const int TamanhoDoFormulario = 40;

    /// <summary>Tamanho da instituição e da linha de crédito.</summary>
    public const int TamanhoDoTexto = 60;

    /// <summary>
    /// O MAIOR VALOR FINANCIADO QUE SE ACEITA NUMA VENDA, em reais. Acima disto é digitação: 36 linhas acima de 20
    /// milhões em 17 mil (28/09/2026), e são elas que inflavam 2013 para 3,4 bilhões.
    /// </summary>
    public const decimal ValorMaximo = 20_000_000m;

    /// <summary>O primeiro ano dos formulários de venda.</summary>
    public const int PrimeiroAno = 2012;

    /// <summary>
    /// OS FORMULÁRIOS DA VENDA QUE TRAZEM O VALOR FINANCIADO, do mais novo ao mais antigo — a ordem é a prioridade
    /// quando o mesmo processo aparece em mais de um.
    /// </summary>
    public static readonly IReadOnlyList<string> PrioridadeDoFormulario =
    [
        "IV_Q_VENDA_EQUIPAMENTO",
        "IV_Q_VENDA",
        "IV_Q_ACOMPANH_VENDA_JDE",
        "IV_Q_ACOMPANHAMENTO_VENDA",
        "IV_Q_GESTAO_CREDITO"
    ];

    /// <summary>
    /// AS LINHAS QUE NÃO SÃO CRÉDITO RURAL, mesmo com "valor financiado" preenchido (decisão do Ricardo em 28/09/2026:
    /// "tudo menos próprio/consórcio"). Recurso próprio é pagamento; consórcio não passa pelo SICOR. Moderfrota, Pronaf,
    /// Pronamp, FINAME, Pró-Trator e TFBD entram.
    ///
    /// <para><b>No domínio, e provisória, de propósito</b> — como os produtos de máquina do SICOR
    /// (<see cref="ParametroDoPotencial.ProdutosDeMaquinaNoSicor"/>): a lista é decisão de negócio, e uma lista só, com o
    /// nome, é o que se acha. Levá-la ao Administrador, com vigência e trilha, é o passo seguinte.</para>
    /// </summary>
    public static readonly IReadOnlyList<string> LinhasForaDoCreditoRural = ["RECURSO PROPRIO", "CONSORCIO"];

    /// <summary>
    /// AS INSTITUIÇÕES QUE DIZEM "NÃO HOUVE CRÉDITO": "não se aplica" e a administradora de consórcio. O formulário antigo
    /// não tem a linha, e é a instituição que separa o crédito do resto.
    /// </summary>
    public static readonly IReadOnlyList<string> InstituicoesForaDoCreditoRural = ["NAO SE APLICA", "CONSORCIO"];

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O número do processo no Vórtice — a chave natural.</summary>
    public long ProcessoNoVortice { get; private set; }

    /// <summary>O formulário de onde o registro veio.</summary>
    public string Formulario { get; private set; } = default!;

    /// <summary>A data do pedido de venda; sem ela no formulário, a data em que o formulário foi preenchido.</summary>
    public DateOnly PedidoEm { get; private set; }

    /// <summary>Se <see cref="PedidoEm"/> é a data do preenchimento, porque o formulário não trouxe a do pedido.</summary>
    public bool DataDoPreenchimento { get; private set; }

    /// <summary>O município do cadastro do cliente, casado com o catálogo; nulo fora de SP ou sem correspondência.</summary>
    public int? MunicipioId { get; private set; }

    /// <summary>O valor financiado, em reais.</summary>
    public decimal ValorFinanciado { get; private set; }

    /// <summary>A instituição financeira, como o formulário a escreve (em maiúsculas).</summary>
    public string? InstituicaoFinanceira { get; private set; }

    /// <summary>A linha de crédito (Moderfrota, Pronaf…); o formulário antigo não a tem.</summary>
    public string? LinhaDeCredito { get; private set; }

    /// <summary>Se o financiamento entra no share: crédito rural, e não recurso próprio nem consórcio.</summary>
    public bool ContaNoCreditoRural { get; private set; }

    /// <summary>O resumo do que veio da origem, para a releitura saber se mudou.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Quando o processo saiu da leitura — cancelado, ou o valor apagado —; nulo é vigente.</summary>
    public DateTime? ExcluidoEm { get; private set; }

    /// <summary>O que a leitura traz de um financiamento, já saneado.</summary>
    /// <param name="Formulario">O formulário.</param>
    /// <param name="PedidoEm">A data do pedido (ou do preenchimento).</param>
    /// <param name="DataDoPreenchimento">Se a data é a do preenchimento.</param>
    /// <param name="MunicipioId">O município do catálogo, quando casou.</param>
    /// <param name="ValorFinanciado">O valor.</param>
    /// <param name="InstituicaoFinanceira">A instituição.</param>
    /// <param name="LinhaDeCredito">A linha.</param>
    /// <param name="HashDaOrigem">O resumo da origem.</param>
    public sealed record Dados(
        string Formulario, DateOnly PedidoEm, bool DataDoPreenchimento, int? MunicipioId, decimal ValorFinanciado,
        string? InstituicaoFinanceira, string? LinhaDeCredito, string HashDaOrigem);

    /// <summary>Registra um financiamento.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando o processo, o valor, a data ou os textos não valem.</exception>
    public static FinanciamentoDaVenda Registrar(long processo, Dados dados, long importadoPorId, DateTime agoraUtc)
    {
        if (processo <= 0) throw new RegraDeNegocioViolada($"O processo do Vórtice é positivo; veio {processo}.");

        var financiamento = new FinanciamentoDaVenda { ProcessoNoVortice = processo };
        financiamento.Aplicar(dados, importadoPorId, agoraUtc);
        return financiamento;
    }

    /// <summary>Confere contra uma nova leitura e diz se mudou.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando o valor, a data ou os textos não valem.</exception>
    public bool AtualizarDaOrigem(Dados dados, long importadoPorId, DateTime agoraUtc)
    {
        var contaNoCredito = ContaComoCreditoRural(dados.LinhaDeCredito, dados.InstituicaoFinanceira);
        if (HashDaOrigem == dados.HashDaOrigem && MunicipioId == dados.MunicipioId && ContaNoCreditoRural == contaNoCredito)
            return false;

        Aplicar(dados, importadoPorId, agoraUtc);
        return true;
    }

    /// <summary>Tira o financiamento da leitura, sem apagar.</summary>
    public void Excluir(DateTime agoraUtc) => ExcluidoEm ??= agoraUtc;

    /// <summary>Devolve à leitura o financiamento que voltou à origem.</summary>
    public void Reativar() => ExcluidoEm = null;

    /// <summary>
    /// SE O FINANCIAMENTO ENTRA NO SHARE (decisão de 28/09/2026). Sai o que a linha diz que é recurso próprio ou consórcio,
    /// e o que a instituição diz que não houve crédito. Sem linha e sem instituição também sai: o valor sozinho não diz
    /// que houve crédito — em 2022 e 2023, 712 formulários vêm assim.
    /// </summary>
    /// <param name="linha">A linha de crédito.</param>
    /// <param name="instituicao">A instituição.</param>
    public static bool ContaComoCreditoRural(string? linha, string? instituicao)
    {
        var l = Normalizar(linha);
        var i = Normalizar(instituicao);

        if (l.Length == 0 && i.Length == 0) return false;
        if (LinhasForaDoCreditoRural.Any(fora => l.StartsWith(fora, StringComparison.Ordinal))) return false;
        if (InstituicoesForaDoCreditoRural.Any(fora => i.StartsWith(fora, StringComparison.Ordinal))) return false;

        // A INSTITUIÇÃO SEM NOME NÃO SUSTENTA O CRÉDITO: a linha pode vir preenchida por padrão.
        return i.Length > 0;
    }

    /// <summary>
    /// SE O PROCESSO FOI CANCELADO — a venda que não aconteceu não é financiamento. A fase <c>Cancelamento</c> ou
    /// <c>Cancelado</c>, ou o status que diz cancelado, desistência ou venda perdida (medido em 28/09/2026: 1.522 "DESISTIU
    /// DA COMPRA" só no formulário JDE).
    /// </summary>
    /// <param name="fase">A fase do processo.</param>
    /// <param name="status">A descrição do status.</param>
    public static bool ProcessoCancelado(string? fase, string? status)
    {
        var f = Normalizar(fase);
        var s = Normalizar(status);
        return f.StartsWith("CANCEL", StringComparison.Ordinal)
               || s.Contains("CANCEL", StringComparison.Ordinal)
               || s.Contains("DESIST", StringComparison.Ordinal)
               || s.Contains("PERDIDA", StringComparison.Ordinal);
    }

    /// <summary>O texto sem acento, em maiúsculas e com os espaços apertados — como a regra compara.</summary>
    /// <param name="texto">O texto.</param>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        var decomposto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sem = new StringBuilder(decomposto.Length);
        var espaco = false;
        foreach (var c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            var letra = c is '-' or '_' ? ' ' : char.ToUpperInvariant(c);
            if (char.IsWhiteSpace(letra))
            {
                if (!espaco) sem.Append(' ');
                espaco = true;
                continue;
            }

            sem.Append(letra);
            espaco = false;
        }

        return sem.ToString().Trim();
    }

    private void Aplicar(Dados dados, long importadoPorId, DateTime agoraUtc)
    {
        var formulario = dados.Formulario?.Trim() ?? string.Empty;
        if (formulario.Length is 0 or > TamanhoDoFormulario)
            throw new RegraDeNegocioViolada("O formulário de origem é obrigatório e vai até 40 caracteres.");

        if (dados.ValorFinanciado is <= 0 or > ValorMaximo)
            throw new RegraDeNegocioViolada(string.Create(CultureInfo.InvariantCulture,
                $"O valor financiado vai de zero a {ValorMaximo:N0}; veio {dados.ValorFinanciado}."));

        if (dados.PedidoEm.Year < PrimeiroAno || dados.PedidoEm.Year > 2100)
            throw new RegraDeNegocioViolada($"A data do pedido está fora do razoável: {dados.PedidoEm:dd/MM/yyyy}.");

        if (string.IsNullOrWhiteSpace(dados.HashDaOrigem))
            throw new RegraDeNegocioViolada("O resumo da origem é obrigatório.");

        Formulario = formulario;
        PedidoEm = dados.PedidoEm;
        DataDoPreenchimento = dados.DataDoPreenchimento;
        MunicipioId = dados.MunicipioId;
        ValorFinanciado = dados.ValorFinanciado;
        InstituicaoFinanceira = Cortar(dados.InstituicaoFinanceira);
        LinhaDeCredito = Cortar(dados.LinhaDeCredito);
        ContaNoCreditoRural = ContaComoCreditoRural(dados.LinhaDeCredito, dados.InstituicaoFinanceira);
        HashDaOrigem = dados.HashDaOrigem;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
    }

    /// <summary>O texto em maiúsculas, no tamanho da coluna; vazio é nulo.</summary>
    private static string? Cortar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var limpo = texto.Trim().ToUpperInvariant();
        return limpo.Length <= TamanhoDoTexto ? limpo : limpo[..TamanhoDoTexto];
    }
}
