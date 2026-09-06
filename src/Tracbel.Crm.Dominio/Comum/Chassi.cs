namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Chassi de máquina agrícola — a identidade real do equipamento (documento 04, seção 9).
///
/// [V] LIÇÃO DO VÓRTICE — <c>EXT_Veic</c> está morto (parou em 24/05/2024) e o cadastro
/// vivo de equipamento (<c>IV_ClientePropr</c>) não valida formato: chassi é texto livre,
/// o que impede usar a coluna como chave natural confiável para casar venda, garantia e
/// ordem de serviço do mesmo equipamento. Aqui o chassi tem forma fixa e é a chave de
/// deduplicação de equipamento (documento 16, seção 4).
/// </summary>
public readonly record struct Chassi
{
    private Chassi(string numero) => Numero = numero;

    /// <summary>17 caracteres, maiúsculos, sem os caracteres que o padrão VIN proíbe (I, O, Q).</summary>
    public string Numero { get; }

    /// <summary>Cria a partir de uma entrada de usuário. Lança se for inválida.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando não são 17 caracteres alfanuméricos válidos.</exception>
    public static Chassi Criar(string entrada) =>
        TentarCriar(entrada, out var chassi)
            ? chassi
            : throw new RegraDeNegocioViolada(
                $"Chassi inválido: '{entrada}'. Precisa ter 17 caracteres (letras e números, sem I, O, Q).");

    /// <summary>Versão que não lança. Use em importação em massa.</summary>
    public static bool TentarCriar(string? entrada, out Chassi chassi)
    {
        chassi = default;
        if (string.IsNullOrWhiteSpace(entrada)) return false;

        var normalizado = new string(entrada.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

        // Padrão VIN: 17 caracteres, sem I/O/Q — o padrão internacional os proíbe para não
        // confundir com 1/0/0 na leitura manual da plaqueta; a mesma razão vale aqui.
        if (normalizado.Length != 17) return false;
        if (!normalizado.All(c => char.IsAsciiDigit(c) || (char.IsAsciiLetterUpper(c) && c is not ('I' or 'O' or 'Q'))))
            return false;

        chassi = new Chassi(normalizado);
        return true;
    }

    /// <inheritdoc />
    public override string ToString() => Numero;
}
