namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// CPF ou CNPJ, com dígito verificador conferido.
///
/// [V] LIÇÃO DO VÓRTICE — foram medidos <b>116 CPFs repetidos entre clientes distintos</b>,
/// porque a validação nunca aconteceu na entrada. Documento com dígito errado entra no
/// cadastro, duplica o cliente, quebra a integração com o Protheus e some da carteira.
///
/// Validar aqui é mais barato do que deduplicar depois: a fila de deduplicação do Vórtice
/// tem 124 mil pares abandonados desde 2018.
/// </summary>
public readonly record struct CpfCnpj
{
    private CpfCnpj(string numero, bool ehPessoaFisica)
    {
        Numero = numero;
        EhPessoaFisica = ehPessoaFisica;
    }

    /// <summary>Só dígitos, sem máscara. 11 para CPF, 14 para CNPJ.</summary>
    public string Numero { get; }

    /// <summary>Verdadeiro para CPF, falso para CNPJ.</summary>
    public bool EhPessoaFisica { get; }

    /// <summary>Cria a partir de uma entrada de usuário. Lança se for inválida.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando o documento não passa no dígito verificador.</exception>
    public static CpfCnpj Criar(string entrada) =>
        TentarCriar(entrada, out var doc)
            ? doc
            : throw new RegraDeNegocioViolada(
                $"CPF/CNPJ inválido: '{entrada}'. Confira os dígitos.");

    /// <summary>Versão que não lança. Use em importação em massa.</summary>
    public static bool TentarCriar(string? entrada, out CpfCnpj documento)
    {
        documento = default;
        if (string.IsNullOrWhiteSpace(entrada)) return false;

        var digitos = new string(entrada.Where(char.IsDigit).ToArray());

        return digitos.Length switch
        {
            11 when ValidarCpf(digitos)  => Aceitar(digitos, true,  out documento),
            14 when ValidarCnpj(digitos) => Aceitar(digitos, false, out documento),
            _ => false
        };

        static bool Aceitar(string d, bool fisica, out CpfCnpj saida)
        {
            saida = new CpfCnpj(d, fisica);
            return true;
        }
    }

    /// <summary>Formata para exibição: <c>000.000.000-00</c> ou <c>00.000.000/0000-00</c>.</summary>
    public string Formatado() => EhPessoaFisica
        ? $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}"
        : $"{Numero[..2]}.{Numero[2..5]}.{Numero[5..8]}/{Numero[8..12]}-{Numero[12..]}";

    private static bool ValidarCpf(string cpf)
    {
        // Sequências repetidas (00000000000, 11111111111...) passam no cálculo mas não existem.
        if (cpf.Distinct().Count() == 1) return false;

        var primeiro = DigitoVerificador(cpf[..9], 10);
        var segundo = DigitoVerificador(cpf[..9] + primeiro, 11);
        return cpf[9] == primeiro && cpf[10] == segundo;

        static char DigitoVerificador(string baseNumero, int pesoInicial)
        {
            var soma = baseNumero.Select((c, i) => (c - '0') * (pesoInicial - i)).Sum();
            var resto = soma % 11;
            return (char)('0' + (resto < 2 ? 0 : 11 - resto));
        }
    }

    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;

        int[] pesosPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesosSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var primeiro = DigitoVerificador(cnpj[..12], pesosPrimeiro);
        var segundo = DigitoVerificador(cnpj[..12] + primeiro, pesosSegundo);
        return cnpj[12] == primeiro && cnpj[13] == segundo;

        static char DigitoVerificador(string baseNumero, int[] pesos)
        {
            var soma = baseNumero.Select((c, i) => (c - '0') * pesos[i]).Sum();
            var resto = soma % 11;
            return (char)('0' + (resto < 2 ? 0 : 11 - resto));
        }
    }

    /// <inheritdoc />
    public override string ToString() => Formatado();
}
