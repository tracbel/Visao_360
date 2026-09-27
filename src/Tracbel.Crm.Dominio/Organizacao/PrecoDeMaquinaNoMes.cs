using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O PREÇO DE UMA CATEGORIA DE MÁQUINA NUM MÊS — a mediana do valor unitário das notas de venda da Tracbel
/// (issue 70, D-P12, decidida pelo Ricardo em 27/09/2026).
///
/// <para><b>De onde vem.</b> O item da nota fiscal de saída do Protheus (<c>SD2</c>, grupo <c>VEIC</c>),
/// casado com a venda do ART pela filial que faturou e pelo número da nota — o ART dá a categoria da máquina
/// (trator, colheitadeira, colhedora de cana…), a nota dá o preço faturado de verdade. Não é tabela de preço,
/// não é preço declarado pelo CEN numa venda perdida: é o que foi faturado.</para>
///
/// <para><b>Só o agregado.</b> O valor de cada venda fica no ERP: a venda do ART no CRM não guarda valor, de
/// propósito (<c>VendaDeMaquina</c>), e esta tabela também não. Mediana, menor, maior e quantas notas — o que o
/// mercado anual e o termo de troca pedem, e nada que identifique um cliente ou uma negociação.</para>
///
/// <para><b>Mediana, e não média.</b> Uma nota de uma máquina fora da curva — a colheitadeira mais equipada da
/// linha — puxaria a média do mês inteiro; a mediana diz o preço típico.</para>
///
/// <para><b>Sem filial.</b> O preço de referência é da empresa, e não de quem lê: o mesmo mercado anual para
/// todo mundo. Por isso a tabela não tem <c>EmpresaId</c>, como as cotações da CONAB.</para>
/// </summary>
public sealed class PrecoDeMaquinaNoMes
{
    /// <summary>A fonte gravada em cada mês: o item da nota de saída do Protheus.</summary>
    public const string FonteDaNota = "PROTHEUS.SD2";

    private PrecoDeMaquinaNoMes() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A categoria de máquina (trator, colheitadeira…), do catálogo.</summary>
    public int CategoriaDeMaquinaId { get; private set; }

    /// <summary>O mês da emissão das notas, sempre no dia 1.</summary>
    public DateOnly Mes { get; private set; }

    /// <summary>A mediana do valor unitário das notas do mês, em reais.</summary>
    public decimal Mediana { get; private set; }

    /// <summary>O menor valor unitário do mês.</summary>
    public decimal Menor { get; private set; }

    /// <summary>O maior valor unitário do mês.</summary>
    public decimal Maior { get; private set; }

    /// <summary>Quantas notas entraram na mediana — o tamanho da base.</summary>
    public int Notas { get; private set; }

    /// <summary>De onde o preço saiu (<see cref="FonteDaNota"/>).</summary>
    public string Fonte { get; private set; } = default!;

    /// <summary>Quando a carga gravou ou revisou a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Registra o preço de uma categoria num mês.</summary>
    /// <param name="categoriaDeMaquinaId">A categoria.</param>
    /// <param name="mes">O mês — qualquer dia; é gravado no dia 1.</param>
    /// <param name="mediana">A mediana do mês.</param>
    /// <param name="menor">O menor valor do mês.</param>
    /// <param name="maior">O maior valor do mês.</param>
    /// <param name="notas">Quantas notas entraram.</param>
    /// <param name="fonte">De onde saiu.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando os números não fecham — base vazia, valor não positivo, mediana fora da faixa.</exception>
    public static PrecoDeMaquinaNoMes Registrar(
        int categoriaDeMaquinaId, DateOnly mes, decimal mediana, decimal menor, decimal maior, int notas,
        string fonte, long importadoPorId, DateTime agoraUtc)
    {
        if (string.IsNullOrWhiteSpace(fonte))
            throw new RegraDeNegocioViolada("O preço de máquina precisa dizer a fonte.");

        Conferir(mediana, menor, maior, notas);

        return new PrecoDeMaquinaNoMes
        {
            CategoriaDeMaquinaId = categoriaDeMaquinaId,
            Mes = CotacaoDeProduto.PrimeiroDia(mes),
            Mediana = mediana,
            Menor = menor,
            Maior = maior,
            Notas = notas,
            Fonte = fonte.Trim(),
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>
    /// Confere o mês contra uma nova leitura e diz se algum número mudou — a nota cancelada que saiu, a venda
    /// que o ART trouxe atrasada.
    /// </summary>
    /// <param name="mediana">A mediana na nova leitura.</param>
    /// <param name="menor">O menor valor na nova leitura.</param>
    /// <param name="maior">O maior valor na nova leitura.</param>
    /// <param name="notas">Quantas notas na nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public bool Revisar(decimal mediana, decimal menor, decimal maior, int notas, long importadoPorId, DateTime agoraUtc)
    {
        Conferir(mediana, menor, maior, notas);

        var mudou = Mediana != mediana || Menor != menor || Maior != maior || Notas != notas;
        if (!mudou) return false;

        Mediana = mediana;
        Menor = menor;
        Maior = maior;
        Notas = notas;

        // O CARIMBO SÓ ANDA QUANDO ALGO MUDA, como nas cotações: a rodada diária relê três anos de meses que
        // não mudaram.
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
        return true;
    }

    private static void Conferir(decimal mediana, decimal menor, decimal maior, int notas)
    {
        if (notas <= 0)
            throw new RegraDeNegocioViolada("Preço de máquina sem nota nenhuma não é preço: é falta de dado.");

        if (menor <= 0)
            throw new RegraDeNegocioViolada("Valor de máquina é positivo; zero ou negativo é falta de dado.");

        if (mediana < menor || mediana > maior)
            throw new RegraDeNegocioViolada("A mediana fica entre o menor e o maior valor do mês.");
    }
}
