namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// CEP (Código de Endereçamento Postal) brasileiro, normalizado.
///
/// [V] LIÇÃO DO VÓRTICE — endereço é campo de texto livre (<c>GE_PessoaEnd</c>), sem
/// nenhuma validação de formato. Um CEP com letra, com menos de 8 dígitos ou repetindo o
/// mesmo dígito oito vezes entra sem aviso e só aparece quando a entrega dos Correios
/// falha ou a rota comercial é calculada errado. Aqui o formato é conferido na entrada.
/// </summary>
public readonly record struct Cep
{
    private Cep(string numero) => Numero = numero;

    /// <summary>Só dígitos, sem hífen. Sempre 8 posições. Ex.: "78455000".</summary>
    public string Numero { get; }

    /// <summary>Cria a partir de uma entrada de usuário. Lança se for inválida.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando não são 8 dígitos, ou é sequência de erro de preenchimento.</exception>
    public static Cep Criar(string entrada) =>
        TentarCriar(entrada, out var cep)
            ? cep
            : throw new RegraDeNegocioViolada($"CEP inválido: '{entrada}'. Informe os 8 dígitos.");

    /// <summary>Versão que não lança. Use em importação em massa.</summary>
    public static bool TentarCriar(string? entrada, out Cep cep)
    {
        cep = default;
        if (string.IsNullOrWhiteSpace(entrada)) return false;

        var digitos = new string(entrada.Where(char.IsDigit).ToArray());
        if (digitos.Length != 8) return false;

        // Sequência de um único dígito repetido ("00000000", "11111111"...) é erro de
        // preenchimento, não um CEP: nenhuma faixa dos Correios usa isso.
        if (digitos.Distinct().Count() == 1) return false;

        cep = new Cep(digitos);
        return true;
    }

    /// <summary>Formata para exibição: <c>00000-000</c>.</summary>
    public string Formatado() => $"{Numero[..5]}-{Numero[5..]}";

    /// <inheritdoc />
    public override string ToString() => Formatado();
}
