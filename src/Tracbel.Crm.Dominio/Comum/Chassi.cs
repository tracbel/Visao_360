namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Chassi de máquina agrícola — a identidade real do equipamento (documento 04, seção 9).
///
/// [V] LIÇÃO DO VÓRTICE — <c>EXT_Veic</c> está morto (parou em 24/05/2024) e o cadastro
/// vivo de equipamento (<c>IV_ClientePropr</c>) não valida formato: chassi é texto livre,
/// o que impede usar a coluna como chave natural confiável para casar venda, garantia e
/// ordem de serviço do mesmo equipamento. Aqui o chassi tem forma fixa e é a chave de
/// deduplicação de equipamento (documento 16, seção 4).
///
/// <para><b>Duas formas, e só duas</b> (decisões de 24/09/2026):</para>
/// <list type="bullet">
/// <item><b>VIN de 17 posições</b>, só letras e números. O padrão internacional proíbe I, O e Q, e até 24/09 esta
/// regra também proibia — mas a John Deere grava o prefixo <c>1CQ</c> nos chassis dela (1.211 máquinas no cadastro
/// de veículos do Protheus, medido nessa data), e recusar a plaqueta real da máquina por causa de uma letra deixava
/// 125 vendas do ART pendentes sem erro nenhum de digitação. A letra continua sem ser trocada: o chassi é guardado
/// como está na plaqueta.</item>
/// <item><b>Identificador curto CONFIRMADO pelo Protheus</b> — o número de série de implemento e de máquina antiga,
/// com menos de 17 posições. Sozinho ele não prova nada (um pedaço de chassi também é curto); ele só vira identidade
/// quando o cadastro de veículos do Protheus (VV1) tem EXATAMENTE aquele identificador. Por isso ele não nasce de
/// <see cref="TentarCriar"/>, que é o que a tela e as importações usam: nasce de
/// <see cref="TentarCriarIdentificadorConfirmado"/>, e quem chama responde pela confirmação.</item>
/// </list>
/// </summary>
public readonly record struct Chassi
{
    /// <summary>O tamanho do VIN.</summary>
    public const int TamanhoDoVin = 17;

    /// <summary>
    /// O MENOR IDENTIFICADOR CURTO ACEITO. Medido em 24/09/2026 no cadastro de veículos do Protheus: os de 3 e 4
    /// posições (128 linhas) são só dígitos, sem cara de número de série — e um número de quatro dígitos se repete
    /// entre marcas. Cinco é o menor tamanho em que as vendas do ART com número de série aparecem.
    /// </summary>
    public const int TamanhoMinimoDoIdentificadorCurto = 5;

    /// <summary>O maior tamanho que a coluna guarda.</summary>
    public const int TamanhoMaximo = 40;

    private Chassi(string numero) => Numero = numero;

    /// <summary>O identificador normalizado: sem espaço nenhum, maiúsculo.</summary>
    public string Numero { get; }

    /// <summary>Verdadeiro quando é um VIN de 17 posições; falso quando é identificador curto confirmado.</summary>
    public bool EhVin => Numero?.Length == TamanhoDoVin;

    /// <summary>Cria a partir de uma entrada de usuário. Lança se for inválida.</summary>
    /// <exception cref="RegraDeNegocioViolada">Quando não são 17 caracteres alfanuméricos.</exception>
    public static Chassi Criar(string entrada) =>
        TentarCriar(entrada, out var chassi)
            ? chassi
            : throw new RegraDeNegocioViolada(
                $"Chassi inválido: '{entrada}'. Precisa ter 17 caracteres, só letras e números.");

    /// <summary>
    /// Versão que não lança — o VIN de 17 posições, a única forma que entra sem confirmação de fora. Use em
    /// importação em massa.
    /// </summary>
    public static bool TentarCriar(string? entrada, out Chassi chassi)
    {
        chassi = default;
        var normalizado = Normalizar(entrada);

        // Só letras e números ASCII: símbolo não é chassi, e adivinhar qual parte do texto é o chassi seria
        // inventar a identidade da máquina.
        if (normalizado.Length != TamanhoDoVin || !normalizado.All(EhAlfanumerico)) return false;

        chassi = new Chassi(normalizado);
        return true;
    }

    /// <summary>
    /// O IDENTIFICADOR CURTO CONFIRMADO pelo cadastro de veículos do Protheus (VV1): de
    /// <see cref="TamanhoMinimoDoIdentificadorCurto"/> a 16 posições, letras e números — com hífen, barra e ponto no
    /// meio, que são como o fabricante escreve alguns números de série (medido em 24/09/2026) — e ao menos um
    /// dígito, porque "SEM CHASSI" e "USADO" não são número de série de ninguém.
    ///
    /// <para><b>A confirmação é de quem chama</b>: este método só confere a forma. Quem o usa tem de ter achado o
    /// identificador, normalizado da mesma maneira, no cadastro do Protheus — é isso que o separa de um chassi
    /// cortado.</para>
    /// </summary>
    /// <param name="entrada">O identificador como veio.</param>
    /// <param name="chassi">O identificador, quando a forma é aceita.</param>
    public static bool TentarCriarIdentificadorConfirmado(string? entrada, out Chassi chassi)
    {
        chassi = default;
        var normalizado = Normalizar(entrada);

        if (normalizado.Length is < TamanhoMinimoDoIdentificadorCurto or >= TamanhoDoVin) return false;
        if (!EhAlfanumerico(normalizado[0]) || !EhAlfanumerico(normalizado[^1])) return false;
        if (!normalizado.All(c => EhAlfanumerico(c) || c is '-' or '/' or '.')) return false;
        if (!normalizado.Any(char.IsAsciiDigit)) return false;

        chassi = new Chassi(normalizado);
        return true;
    }

    /// <summary>
    /// Reconstrói o que o banco guarda — um VIN ou um identificador curto que entrou confirmado. Não serve para
    /// entrada de usuário: a confirmação do identificador curto aconteceu quando ele foi gravado.
    /// </summary>
    /// <exception cref="RegraDeNegocioViolada">Quando o valor guardado não tem nenhuma das duas formas.</exception>
    public static Chassi Restaurar(string guardado) =>
        TentarCriar(guardado, out var chassi) || TentarCriarIdentificadorConfirmado(guardado, out chassi)
            ? chassi
            : throw new RegraDeNegocioViolada($"O chassi guardado '{guardado}' não é VIN nem identificador confirmado.");

    /// <summary>
    /// A NORMALIZAÇÃO ÚNICA — sem espaço nenhum (inclusive no meio) e maiúsculo. É a mesma para a plaqueta digitada,
    /// a venda do ART e o cadastro do Protheus; comparar chassis normalizados de jeitos diferentes deixaria de achar
    /// a mesma máquina (medido em 24/09/2026: aparar só as pontas perdia 98 chassis do Protheus).
    /// </summary>
    /// <param name="entrada">O texto como veio.</param>
    public static string Normalizar(string? entrada) =>
        string.IsNullOrWhiteSpace(entrada)
            ? string.Empty
            : new string([.. entrada.Where(c => !char.IsWhiteSpace(c))]).ToUpperInvariant();

    private static bool EhAlfanumerico(char c) => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c);

    /// <inheritdoc />
    public override string ToString() => Numero;
}
