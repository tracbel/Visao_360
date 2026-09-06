using System.Globalization;

namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Valor monetário em reais, sempre com exatamente duas casas decimais e não negativo.
///
/// [V] LIÇÃO DO VÓRTICE — o documento 04 já proíbe <c>float</c>/<c>money</c> para dinheiro
/// no BANCO, pelo arredondamento silencioso. O mesmo cuidado precisa valer na ENTRADA: um
/// valor como <c>1234.567</c> vindo de planilha ou de outro sistema não é "quase dois
/// centavos" — é dado corrompido. Arredondar sem avisar é o mesmo erro de silêncio que o
/// Vórtice comete na esteira de integração (documento 01, achado 3.8). Rejeitar aqui é
/// mais barato do que reconciliar caixa depois.
/// </summary>
public readonly record struct Dinheiro
{
    private Dinheiro(decimal valor) => Valor = valor;

    /// <summary>O valor, sempre com escala 2 e não negativo.</summary>
    public decimal Valor { get; }

    /// <summary>Cria a partir de um <see cref="decimal"/>. Lança se tiver mais de duas casas ou for negativo.</summary>
    /// <exception cref="RegraDeNegocioViolada">Mais de duas casas decimais, ou valor negativo.</exception>
    public static Dinheiro Criar(decimal valor) =>
        TentarCriar(valor, out var dinheiro)
            ? dinheiro
            : throw new RegraDeNegocioViolada(
                $"Valor monetário inválido: '{valor}'. Use no máximo duas casas decimais e um valor não negativo.");

    /// <summary>Versão que não lança. Use em importação em massa.</summary>
    public static bool TentarCriar(decimal valor, out Dinheiro dinheiro)
    {
        dinheiro = default;

        if (valor < 0) return false;

        // Compara com o valor arredondado: se diferem, havia uma terceira casa decimal.
        if (decimal.Round(valor, 2) != valor) return false;

        dinheiro = new Dinheiro(valor);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Valor.ToString("F2", CultureInfo.InvariantCulture);
}
