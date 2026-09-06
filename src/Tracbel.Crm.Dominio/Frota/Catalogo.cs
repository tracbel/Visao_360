namespace Tracbel.Crm.Dominio.Frota;

/// <summary>
/// A marca da máquina: John Deere, Case, New Holland.
///
/// Referenciada por modelo e por equipamento — inclusive a marca do concorrente, que é
/// justamente a que interessa para a tela de Cobertura.
/// </summary>
public sealed class Marca
{
    private Marca() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Se a Tracbel representa a marca. Falso é marca de concorrente.</summary>
    public bool EhRepresentada { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma marca.</summary>
    public static Marca Criar(string codigo, string nome, bool ehRepresentada = false) => new()
    {
        Codigo = codigo,
        Nome = nome,
        EhRepresentada = ehRepresentada
    };
}

/// <summary>
/// A família de máquinas: tratores 7J, colheitadeiras, pulverizadores.
///
/// Nível intermediário do catálogo que a tela de Nova Oportunidade percorre.
/// </summary>
public sealed class Familia
{
    private Familia() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>A marca a que a família pertence.</summary>
    public int MarcaId { get; private set; }

    /// <summary>Código estável dentro da marca.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma família.</summary>
    public static Familia Criar(int marcaId, string codigo, string nome) => new()
    {
        MarcaId = marcaId,
        Codigo = codigo,
        Nome = nome
    };
}

/// <summary>
/// O modelo da máquina, com potência.
///
/// [V] Aqui está o maior ganho de catálogo do modelo: 11,5 milhões de linhas do Vórtice —
/// entre modelo, plano de manutenção materializado e o produto cartesiano de modelo por item
/// — viram um catálogo de algumas centenas de linhas. Plano de manutenção é regra, não dado.
/// </summary>
public sealed class Modelo
{
    private Modelo() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>A família a que o modelo pertence.</summary>
    public int FamiliaId { get; private set; }

    /// <summary>Código estável.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível. Ex.: 7250R.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Potência nominal, em cavalos.</summary>
    public short? PotenciaCv { get; private set; }

    /// <summary>Intervalo padrão de manutenção, em horas de operação.</summary>
    public int? IntervaloManutencaoHoras { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Cria um modelo.</summary>
    public static Modelo Criar(int familiaId, string codigo, string nome) => new()
    {
        FamiliaId = familiaId,
        Codigo = codigo,
        Nome = nome
    };
}
