namespace Tracbel.Crm.Dominio.Metadado;

/// <summary>
/// A lista fechada: cultura, concorrente, papel de contato, tipo de documento, praça.
///
/// É metadado de lista. O critério para um assunto virar catálogo genérico em vez de tabela
/// própria: não ter atributo além de código, descrição, ordem e situação; não ter filho nem
/// hierarquia; não ser apontado por tabela de milhões de linhas; e não ter tela própria de
/// administração com regra própria.
/// </summary>
public sealed class Catalogo
{
    private Catalogo() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável. Ex.: CULTURA, CONCORRENTE, PAPEL_CONTATO.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome legível, o que aparece na tela de administração.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Para que serve, em português.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Se o negócio pode acrescentar itens pela tela, sem release.</summary>
    public bool PermiteItemNovo { get; private set; } = true;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Cria um catálogo.</summary>
    public static Catalogo Criar(string codigo, string nome) => new() { Codigo = codigo, Nome = nome };
}

/// <summary>
/// Um item de catálogo, com ordem e situação.
///
/// Absorve dez tabelas do Vórtice e as três tabelas de catálogo que o documento 04 tinha
/// separadas. Nada é apagado: item que sai de uso vira inativo e o histórico continua
/// legível — [V] é o que falta no catálogo de ações do Vórtice, onde 602 das 980 estão fora
/// de uso na MESMA lista das 378 vivas.
///
/// A descrição usa comparação que não distingue caixa nem acento, com unicidade por
/// catálogo: é isso que impede FINALIZADO e FINALIZADA de conviverem.
/// </summary>
public sealed class CatalogoItem
{
    private CatalogoItem() { }

    /// <summary>Identificador interno. É int porque catálogo é lista curta, não fato.</summary>
    public int Id { get; private set; }

    /// <summary>O catálogo a que o item pertence.</summary>
    public int CatalogoId { get; private set; }

    /// <summary>Código estável dentro do catálogo.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>O rótulo que o usuário lê.</summary>
    public string Descricao { get; private set; } = default!;

    /// <summary>Item pai, para catálogo com dois níveis.</summary>
    public int? ItemPaiId { get; private set; }

    /// <summary>Ordem de exibição.</summary>
    public short Ordem { get; private set; } = 100;

    /// <summary>Se escolher este item obriga a escrever uma observação. É o caso do item Outro.</summary>
    public bool ExigeObservacao { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Quando o item foi usado pela última vez. Alimenta a higienização do catálogo.</summary>
    public DateTime? UltimoUsoEm { get; private set; }

    /// <summary>Cria um item de catálogo.</summary>
    public static CatalogoItem Criar(int catalogoId, string codigo, string descricao, short ordem = 100) => new()
    {
        CatalogoId = catalogoId,
        Codigo = codigo,
        Descricao = descricao,
        Ordem = ordem
    };
}
