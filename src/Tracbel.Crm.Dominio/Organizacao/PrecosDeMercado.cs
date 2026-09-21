using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O PREÇO DE UM PRODUTO AGRÍCOLA NUM MÊS, em reais, com a fonte que o publicou (issue 66).
///
/// <para><b>Por que existe.</b> O texto-base do potencial pede "base de histórico de preço da
/// commodity em real e dólar por tipo de cultivo; não varia, só vamos acrescentando". É dela que saem
/// o momento de preço (12 meses ÷ 12 anteriores), a rentabilidade (receita − custo) e o termo de troca
/// (sacas por trator), todos da issue 73.</para>
///
/// <para><b>Só cresce.</b> A CONAB publica uma janela de 12 meses; o mês que sai da janela <b>não</b>
/// sai do CRM. Quando a fonte revisa um mês que já estava gravado, o valor é atualizado — e a trilha de
/// auditoria guarda o anterior (<c>PoliticaDeAuditoria</c>), para que nenhum número mude sem registro.</para>
///
/// <para><b>Em reais, e só em reais.</b> O valor em dólar é calculado na leitura, com o
/// <see cref="CotacaoDoDolar"/> do mesmo mês. Gravar as duas moedas seria duas verdades para o mesmo
/// preço.</para>
///
/// <para><b>Na unidade da fonte.</b> A CONAB publica R$/kg para tudo; a Socicana, R$ por kg de ATR. A
/// unidade comercial — saca de 60 kg, caixa de 40,8 kg, arroba — é conversão de quem mostra.</para>
/// </summary>
public sealed class CotacaoDeProduto
{
    private CotacaoDeProduto() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>Quem publicou: <c>CONAB</c>, <c>SOCICANA</c>.</summary>
    public string Fonte { get; private set; } = default!;

    /// <summary>
    /// O código do produto NA FONTE — o <c>id_produto</c> da CONAB, <c>ATR</c> na Socicana.
    ///
    /// <para>É ele, e não o nome, que identifica a série: a CONAB corrige grafia de classificação, e
    /// uma série identificada por texto se partiria em duas.</para>
    /// </summary>
    public string CodigoNaFonte { get; private set; } = default!;

    /// <summary>A praça: a UF (<c>SP</c>).</summary>
    public string Praca { get; private set; } = default!;

    /// <summary>
    /// O nível do preço: <c>RECEBIDO PELO PRODUTOR</c> na CONAB; <c>MENSAL</c> ou
    /// <c>ACUMULADO DA SAFRA</c> na Socicana.
    /// </summary>
    public string Nivel { get; private set; } = default!;

    /// <summary>O nome do produto, como a fonte o escreve (<c>SOJA</c>).</summary>
    public string Produto { get; private set; } = default!;

    /// <summary>A classificação, como a fonte a escreve (<c>EM GRÃOS</c>).</summary>
    public string Classificacao { get; private set; } = default!;

    /// <summary>A unidade do valor: <c>kg</c>, <c>kg de ATR</c>.</summary>
    public string Unidade { get; private set; } = default!;

    /// <summary>O mês, sempre no dia 1.</summary>
    public DateOnly Mes { get; private set; }

    /// <summary>O preço em reais, por <see cref="Unidade"/>.</summary>
    public decimal ValorEmReais { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra o preço de um mês.</summary>
    /// <param name="fonte">Quem publicou.</param>
    /// <param name="codigoNaFonte">O código do produto na fonte.</param>
    /// <param name="praca">A praça.</param>
    /// <param name="nivel">O nível do preço.</param>
    /// <param name="produto">O nome do produto.</param>
    /// <param name="classificacao">A classificação.</param>
    /// <param name="unidade">A unidade do valor.</param>
    /// <param name="mes">O mês — qualquer dia; é gravado no dia 1.</param>
    /// <param name="valorEmReais">O preço, positivo.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando um campo obrigatório falta ou o preço não é positivo.</exception>
    public static CotacaoDeProduto Registrar(
        string fonte, string codigoNaFonte, string praca, string nivel, string produto,
        string classificacao, string unidade, DateOnly mes, decimal valorEmReais,
        long importadoPorId, DateTime agoraUtc)
    {
        foreach (var (campo, valor) in new[]
                 {
                     ("fonte", fonte), ("código na fonte", codigoNaFonte), ("praça", praca),
                     ("nível", nivel), ("produto", produto), ("unidade", unidade)
                 })
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new RegraDeNegocioViolada($"A cotação precisa de {campo}.");
        }

        ConferirValor(valorEmReais);

        return new CotacaoDeProduto
        {
            Fonte = fonte.Trim(),
            CodigoNaFonte = codigoNaFonte.Trim(),
            Praca = praca.Trim(),
            Nivel = nivel.Trim(),
            Produto = produto.Trim(),
            Classificacao = (classificacao ?? string.Empty).Trim(),
            Unidade = unidade.Trim(),
            Mes = PrimeiroDia(mes),
            ValorEmReais = valorEmReais,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>
    /// Confere o mês contra uma nova leitura da fonte e diz se o VALOR mudou.
    ///
    /// <para>O nome e a classificação acompanham a fonte sem contar como revisão: a CONAB corrige
    /// grafia, e isso não é o preço mudando.</para>
    /// </summary>
    /// <param name="produto">O nome do produto na nova leitura.</param>
    /// <param name="classificacao">A classificação na nova leitura.</param>
    /// <param name="valorEmReais">O preço na nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o preço não é positivo.</exception>
    public bool Revisar(string produto, string classificacao, decimal valorEmReais, long importadoPorId, DateTime agoraUtc)
    {
        ConferirValor(valorEmReais);

        var mudou = ValorEmReais != valorEmReais;

        Produto = produto.Trim();
        Classificacao = (classificacao ?? string.Empty).Trim();
        ValorEmReais = valorEmReais;

        // O CARIMBO SÓ ANDA QUANDO ALGO MUDA. Recarimbar a cada rodada mensal as centenas de meses que
        // não mudaram é o erro que encheu o log do Vórtice.
        if (mudou)
        {
            ImportadoEm = agoraUtc;
            ImportadoPorId = importadoPorId;
        }

        return mudou;
    }

    /// <summary>O dia 1 do mês de uma data.</summary>
    /// <param name="data">Qualquer dia do mês.</param>
    public static DateOnly PrimeiroDia(DateOnly data) => new(data.Year, data.Month, 1);

    private static void ConferirValor(decimal valor)
    {
        if (valor <= 0)
            throw new RegraDeNegocioViolada("Preço de mercado é positivo; zero ou negativo é falta de dado.");
    }
}

/// <summary>
/// O DÓLAR DE UM MÊS — PTAX de venda, média mensal, do Banco Central (série 3698 do SGS).
///
/// <para>É o que converte a <see cref="CotacaoDeProduto"/> para dólar, na leitura. Uma linha por mês;
/// a revisão do Banco Central atualiza o valor e entra na trilha de auditoria.</para>
/// </summary>
public sealed class CotacaoDoDolar
{
    private CotacaoDoDolar() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O mês, sempre no dia 1.</summary>
    public DateOnly Mes { get; private set; }

    /// <summary>Quantos reais vale um dólar, na média do mês.</summary>
    public decimal ReaisPorDolar { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra o dólar de um mês.</summary>
    /// <param name="mes">O mês — gravado no dia 1.</param>
    /// <param name="reaisPorDolar">A cotação, positiva.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a cotação não é positiva.</exception>
    public static CotacaoDoDolar Registrar(DateOnly mes, decimal reaisPorDolar, long importadoPorId, DateTime agoraUtc)
    {
        ConferirValor(reaisPorDolar);

        return new CotacaoDoDolar
        {
            Mes = CotacaoDeProduto.PrimeiroDia(mes),
            ReaisPorDolar = reaisPorDolar,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>Confere o mês contra uma nova leitura e diz se mudou.</summary>
    /// <param name="reaisPorDolar">A cotação na nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a cotação não é positiva.</exception>
    public bool Revisar(decimal reaisPorDolar, long importadoPorId, DateTime agoraUtc)
    {
        ConferirValor(reaisPorDolar);

        if (ReaisPorDolar == reaisPorDolar) return false;

        ReaisPorDolar = reaisPorDolar;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
        return true;
    }

    private static void ConferirValor(decimal valor)
    {
        if (valor <= 0)
            throw new RegraDeNegocioViolada("A cotação do dólar é positiva.");
    }
}
