namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// A UNIDADE EM QUE O MERCADO NEGOCIA CADA PRODUTO — a conversão do "por kg" da CONAB.
///
/// <para><b>Por que existe.</b> O texto-base do potencial fala em "preço da saca" e "lógica para
/// todas as sacas: café, cana etc.". A CONAB publica tudo em R$/kg; o termo de troca (issue 73) é
/// "quantas sacas pagam um trator", e só faz sentido na unidade que o produtor usa.</para>
///
/// <para><b>São convenções de mercado, não parâmetros do negócio</b>: a saca de café e de grãos tem
/// 60 kg, a de amendoim em casca 25 kg, a caixa de laranja 40,8 kg, a arroba 15 kg. Não mudam por
/// decisão da Tracbel, e por isso moram aqui e não na tela do administrador (issue 71).</para>
///
/// <para>Produto sem convenção conhecida fica no quilo — melhor mostrar a unidade da fonte do que
/// inventar uma.</para>
/// </summary>
public static class UnidadeComercial
{
    private static readonly (string Prefixo, string Nome, decimal Quilos)[] Convencoes =
    [
        ("CAFE", "saca de 60 kg", 60m),
        ("SOJA", "saca de 60 kg", 60m),
        ("MILHO", "saca de 60 kg", 60m),
        ("SORGO", "saca de 60 kg", 60m),
        ("TRIGO", "saca de 60 kg", 60m),
        ("FEIJAO", "saca de 60 kg", 60m),
        ("ARROZ", "saca de 50 kg", 50m),
        ("AMENDOIM", "saca de 25 kg", 25m),
        ("LARANJA", "caixa de 40,8 kg", 40.8m),
        ("BOI", "arroba (15 kg)", 15m),
        ("ALGODAO", "arroba (15 kg)", 15m),
        ("CANA DE ACUCAR", "tonelada", 1000m)
    ];

    /// <summary>
    /// A unidade comercial de um produto medido em <paramref name="unidadeDaFonte"/>.
    /// </summary>
    /// <param name="produto">O nome do produto, como a fonte o escreve.</param>
    /// <param name="unidadeDaFonte">A unidade do valor gravado.</param>
    /// <returns>O nome da unidade e quantas unidades da fonte cabem nela.</returns>
    public static (string Nome, decimal Fator) Para(string produto, string unidadeDaFonte)
    {
        // SÓ O QUILO SE CONVERTE. O kg de ATR da Socicana já é a unidade de negociação da cana do
        // fornecedor; convertê-lo para tonelada seria confundir açúcar recuperável com cana entregue.
        if (!string.Equals(unidadeDaFonte, "kg", StringComparison.OrdinalIgnoreCase))
            return (unidadeDaFonte, 1m);

        var nome = (produto ?? string.Empty).Trim().ToUpperInvariant();

        foreach (var (prefixo, unidade, quilos) in Convencoes)
        {
            if (nome.StartsWith(prefixo, StringComparison.Ordinal))
                return (unidade, quilos);
        }

        return ("kg", 1m);
    }
}
