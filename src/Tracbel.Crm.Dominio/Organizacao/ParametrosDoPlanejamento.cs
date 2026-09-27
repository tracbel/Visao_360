using System.Globalization;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// OS PARÂMETROS DO PLANEJAMENTO COMERCIAL, com vigência — a sazonalidade mensal da demanda e os pesos do Índice de
/// Oportunidade Comercial (IOC), numa vigência só, como os parâmetros gerais (issue 256, épico 264).
///
/// <para><b>De onde vêm.</b> O protótipo da pasta 360 (Plataforma Inteligência Agro) guardava os dois no navegador
/// de quem editava — outra pessoa, noutra máquina, via outro número, sem saber quem mudou. Aqui eles têm vigência,
/// autor e trilha, e valem para todos. A semente são os valores do protótipo, marcados como "a confirmar" na
/// justificativa: foi o que o Ricardo pediu em 27/09/2026 ("trazer todas as informações que o protótipo traz").</para>
///
/// <para><b>A sazonalidade</b> diz que fatia da demanda ANUAL cai em cada mês. Ela reparte o número — nunca o
/// aumenta: por isso os doze meses somam 100%.</para>
///
/// <para><b>Os pesos do IOC</b> dizem quanto cada componente conta no índice. Eles são normalizados pela soma: 25 e 20
/// são o mesmo que 50 e 40. Um peso zero tira o componente da conta.</para>
/// </summary>
public sealed class ParametroDoPlanejamento : ParametroComVigencia
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>A folga da soma da sazonalidade: 99,95% a 100,05%, o arredondamento de doze números com duas casas.</summary>
    public const decimal FolgaDaSazonalidade = 0.05m;

    private ParametroDoPlanejamento() { }

    /// <summary>% da demanda anual em janeiro.</summary>
    public decimal SazonalidadeJaneiro { get; private set; }

    /// <summary>% da demanda anual em fevereiro.</summary>
    public decimal SazonalidadeFevereiro { get; private set; }

    /// <summary>% da demanda anual em março.</summary>
    public decimal SazonalidadeMarco { get; private set; }

    /// <summary>% da demanda anual em abril.</summary>
    public decimal SazonalidadeAbril { get; private set; }

    /// <summary>% da demanda anual em maio.</summary>
    public decimal SazonalidadeMaio { get; private set; }

    /// <summary>% da demanda anual em junho.</summary>
    public decimal SazonalidadeJunho { get; private set; }

    /// <summary>% da demanda anual em julho.</summary>
    public decimal SazonalidadeJulho { get; private set; }

    /// <summary>% da demanda anual em agosto.</summary>
    public decimal SazonalidadeAgosto { get; private set; }

    /// <summary>% da demanda anual em setembro.</summary>
    public decimal SazonalidadeSetembro { get; private set; }

    /// <summary>% da demanda anual em outubro.</summary>
    public decimal SazonalidadeOutubro { get; private set; }

    /// <summary>% da demanda anual em novembro.</summary>
    public decimal SazonalidadeNovembro { get; private set; }

    /// <summary>% da demanda anual em dezembro.</summary>
    public decimal SazonalidadeDezembro { get; private set; }

    /// <summary>Peso do potencial ajustado no IOC.</summary>
    public decimal PesoDoPotencial { get; private set; }

    /// <summary>Peso da lacuna de cobertura da carteira no IOC.</summary>
    public decimal PesoDaCobertura { get; private set; }

    /// <summary>Peso do momento do crédito no IOC.</summary>
    public decimal PesoDoCredito { get; private set; }

    /// <summary>Peso do momento do preço da cultura principal no IOC.</summary>
    public decimal PesoDaRentabilidade { get; private set; }

    /// <summary>Peso da falta de clientes frente ao potencial no IOC.</summary>
    public decimal PesoDosClientes { get; private set; }

    /// <summary>Peso da distância até a meta de share no IOC.</summary>
    public decimal PesoDaRealizacao { get; private set; }

    /// <summary>Peso da baixa penetração no potencial no IOC.</summary>
    public decimal PesoDaPenetracao { get; private set; }

    /// <summary>A sazonalidade, de janeiro a dezembro.</summary>
    public IReadOnlyList<decimal> Sazonalidade =>
    [
        SazonalidadeJaneiro, SazonalidadeFevereiro, SazonalidadeMarco, SazonalidadeAbril, SazonalidadeMaio, SazonalidadeJunho,
        SazonalidadeJulho, SazonalidadeAgosto, SazonalidadeSetembro, SazonalidadeOutubro, SazonalidadeNovembro, SazonalidadeDezembro
    ];

    /// <summary>Os pesos do IOC, na ordem dos componentes.</summary>
    public PesosDoIoc Pesos => new(
        PesoDoPotencial, PesoDaCobertura, PesoDoCredito, PesoDaRentabilidade, PesoDosClientes, PesoDaRealizacao, PesoDaPenetracao);

    /// <summary>
    /// A FRAÇÃO DA DEMANDA ANUAL NUM MÊS — o percentual do mês sobre a soma dos doze, para a previsão fechar exatamente no
    /// total anual mesmo com a folga do arredondamento.
    /// </summary>
    /// <param name="mes">O mês, de 1 a 12.</param>
    public decimal FracaoDoMes(int mes)
    {
        if (mes is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(mes), mes, "O mês vai de 1 a 12.");
        var soma = Sazonalidade.Sum();
        return soma <= 0 ? 0m : Sazonalidade[mes - 1] / soma;
    }

    /// <summary>Os valores de uma vigência.</summary>
    /// <param name="Sazonalidade">Os doze percentuais, de janeiro a dezembro.</param>
    /// <param name="Pesos">Os sete pesos do IOC.</param>
    public sealed record Valores(IReadOnlyList<decimal> Sazonalidade, PesosDoIoc Pesos);

    /// <summary>Registra uma vigência dos parâmetros do planejamento.</summary>
    /// <param name="valores">A sazonalidade e os pesos.</param>
    /// <param name="vigenteDesde">O primeiro dia em que vale.</param>
    /// <param name="justificativa">Por que estes valores.</param>
    /// <param name="informadoPorId">Quem registra.</param>
    /// <param name="agoraUtc">O instante do registro.</param>
    public static ParametroDoPlanejamento Informar(
        Valores valores, DateOnly vigenteDesde, string justificativa, long informadoPorId, DateTime agoraUtc)
    {
        Conferir(valores);

        var s = valores.Sazonalidade;
        var p = valores.Pesos;
        var parametro = new ParametroDoPlanejamento
        {
            SazonalidadeJaneiro = s[0], SazonalidadeFevereiro = s[1], SazonalidadeMarco = s[2], SazonalidadeAbril = s[3],
            SazonalidadeMaio = s[4], SazonalidadeJunho = s[5], SazonalidadeJulho = s[6], SazonalidadeAgosto = s[7],
            SazonalidadeSetembro = s[8], SazonalidadeOutubro = s[9], SazonalidadeNovembro = s[10], SazonalidadeDezembro = s[11],
            PesoDoPotencial = p.Potencial, PesoDaCobertura = p.Cobertura, PesoDoCredito = p.Credito,
            PesoDaRentabilidade = p.Rentabilidade, PesoDosClientes = p.Clientes, PesoDaRealizacao = p.Realizacao,
            PesoDaPenetracao = p.Penetracao
        };

        parametro.Informar(vigenteDesde, justificativa, informadoPorId, agoraUtc);
        return parametro;
    }

    private static void Conferir(Valores v)
    {
        if (v.Sazonalidade.Count != 12)
            throw new RegraDeNegocioViolada("A sazonalidade tem os doze meses, de janeiro a dezembro.");

        if (v.Sazonalidade.Any(m => m is < 0 or > 100))
            throw new RegraDeNegocioViolada("O percentual de cada mês vai de 0% a 100%.");

        // A SAZONALIDADE REPARTE, NÃO AUMENTA: somando 110%, a previsão mensal venderia 10% a mais que a demanda do ano.
        var soma = v.Sazonalidade.Sum();
        if (Math.Abs(soma - 100m) > FolgaDaSazonalidade)
            throw new RegraDeNegocioViolada(string.Format(PtBr,
                "Os doze meses da sazonalidade somam 100% — estes somam {0:0.##}%. Ela reparte a demanda do ano, não a aumenta.", soma));

        var pesos = v.Pesos.Todos;
        if (pesos.Any(x => x is < 0 or > 100))
            throw new RegraDeNegocioViolada("Cada peso do IOC vai de 0 a 100.");

        if (pesos.Sum() <= 0)
            throw new RegraDeNegocioViolada("Ao menos um peso do IOC é maior que zero — sem peso nenhum, o índice não mede nada.");
    }
}

/// <summary>
/// OS SETE PESOS DO ÍNDICE DE OPORTUNIDADE COMERCIAL — o que cada componente conta. São normalizados pela soma.
/// </summary>
/// <param name="Potencial">O potencial ajustado pelo momento.</param>
/// <param name="Cobertura">A lacuna de cobertura da carteira.</param>
/// <param name="Credito">O momento do crédito de mecanização.</param>
/// <param name="Rentabilidade">O momento do preço da cultura principal.</param>
/// <param name="Clientes">A falta de clientes frente ao potencial.</param>
/// <param name="Realizacao">A distância até a meta de share.</param>
/// <param name="Penetracao">A baixa penetração no potencial.</param>
public sealed record PesosDoIoc(
    decimal Potencial, decimal Cobertura, decimal Credito, decimal Rentabilidade, decimal Clientes, decimal Realizacao, decimal Penetracao)
{
    /// <summary>Os sete, na ordem.</summary>
    public IReadOnlyList<decimal> Todos => [Potencial, Cobertura, Credito, Rentabilidade, Clientes, Realizacao, Penetracao];
}

/// <summary>
/// O SHARE-ALVO DE UMA CATEGORIA DE MÁQUINA, com vigência — a fatia da demanda que a Tracbel planeja entregar (issue
/// 256). É ele que transforma "o mercado pede 120 tratores por ano" em "a meta da Tracbel é 37".
///
/// <para><b>Não é a meta do vendedor</b> — essa vem da API Gestão de Negócios (#138). O share-alvo é o ponto de
/// partida do planejamento por município: a demanda estimada vezes a fatia que a empresa quer levar.</para>
/// </summary>
public sealed class ShareAlvoDaCategoria : ParametroComVigencia
{
    private ShareAlvoDaCategoria() { }

    /// <summary>A categoria de máquina do catálogo.</summary>
    public int CategoriaDeMaquinaId { get; private set; }

    /// <summary>O share-alvo, em pontos percentuais — 31 é 31%.</summary>
    public decimal Percentual { get; private set; }

    /// <summary>Registra uma vigência do share-alvo de uma categoria.</summary>
    /// <param name="categoriaDeMaquinaId">A categoria.</param>
    /// <param name="percentual">O share-alvo, de 0 (exclusivo) a 100.</param>
    /// <param name="vigenteDesde">O primeiro dia em que vale.</param>
    /// <param name="justificativa">Por que este share.</param>
    /// <param name="informadoPorId">Quem registra.</param>
    /// <param name="agoraUtc">O instante do registro.</param>
    public static ShareAlvoDaCategoria Informar(
        int categoriaDeMaquinaId, decimal percentual, DateOnly vigenteDesde, string justificativa, long informadoPorId, DateTime agoraUtc)
    {
        if (categoriaDeMaquinaId <= 0)
            throw new RegraDeNegocioViolada("O share-alvo é de uma categoria de máquina do catálogo.");

        // ZERO NÃO É META: com 0% o planejamento não entrega nada, e o certo é não registrar share para a categoria.
        if (percentual is <= 0 or > 100)
            throw new RegraDeNegocioViolada("O share-alvo é maior que 0% e vai até 100%.");

        var share = new ShareAlvoDaCategoria { CategoriaDeMaquinaId = categoriaDeMaquinaId, Percentual = percentual };
        share.Informar(vigenteDesde, justificativa, informadoPorId, agoraUtc);
        return share;
    }
}
