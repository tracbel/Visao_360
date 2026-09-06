namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// A praça de mercado, com o total estimado por linha e ano.
///
/// Sustenta o indicador "Conhecimento de mercado" da Visão 360, que hoje é número fixo no
/// JavaScript do protótipo (<c>mercado-pracas.json</c>). Não existe no Vórtice.
/// </summary>
public sealed class Praca
{
    private Praca() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Código estável da praça.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome da praça.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Unidade federativa principal da praça.</summary>
    public string? Uf { get; private set; }

    /// <summary>Linha de negócio a que o potencial se refere.</summary>
    public int LinhaDeNegocioId { get; private set; }

    /// <summary>Ano de referência do potencial estimado.</summary>
    public short AnoReferencia { get; private set; }

    /// <summary>Potencial total estimado da praça no ano, em reais.</summary>
    public Comum.Dinheiro? PotencialEstimado { get; private set; }

    /// <summary>Quantidade de máquinas estimada na praça.</summary>
    public int? MaquinasEstimadas { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma praça.</summary>
    public static Praca Criar(string codigo, string nome, int linhaDeNegocioId, short anoReferencia) => new()
    {
        Codigo = codigo,
        Nome = nome,
        LinhaDeNegocioId = linhaDeNegocioId,
        AnoReferencia = anoReferencia
    };
}
