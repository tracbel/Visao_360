namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A SÉRIE ANUAL DA PAM DE UM RECORTE (issue 198) — valor e quantidade, por produto e por ano.
///
/// <para><b>Ela é a matéria-prima do preço implícito, e não o preço.</b> O repositório soma o que o IBGE
/// publicou; quem divide é o domínio. Isso mantém a conta num lugar só e testável sem banco.</para>
///
/// <para><b>Por que é uma porta própria e não um método da de preços</b>: aquela serve a série MENSAL da
/// CONAB, e as duas não se emendam (documento 49D). Portas separadas para séries que não se somam é a
/// forma mais barata de impedir que alguém as concatene.</para>
/// </summary>
public interface IRepositorioDoPrecoImplicito
{
    /// <summary>
    /// Valor e quantidade somados por produto e ano, no recorte.
    /// </summary>
    /// <param name="municipioCodigoIbge">
    /// O município; nulo soma a ADR inteira.
    ///
    /// <para><b>A soma vem ANTES da divisão</b>, e isso não é detalhe: o preço da região é
    /// <c>Σ valor ÷ Σ quantidade</c>, que é a média PONDERADA pela colheita de cada município — exatamente
    /// o que o IBGE faz. A média simples dos preços municipais daria peso igual a um município que colheu
    /// dez toneladas e a outro que colheu dez mil.</para>
    /// </param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<ProducaoAnualDoProduto>> LerAsync(int? municipioCodigoIbge, CancellationToken ct);
}

/// <summary>O que a PAM publicou de um produto num ano, já somado no recorte.</summary>
/// <param name="ProdutoCodigoIbge">O produto na classificação 782.</param>
/// <param name="ProdutoNome">O rótulo oficial do produto.</param>
/// <param name="Ano">O ano da safra.</param>
/// <param name="ValorDaProducaoMilReais">Soma do valor; nulo quando nenhum município divulgou.</param>
/// <param name="QuantidadeProduzida">Soma da quantidade; nulo quando nenhum município divulgou.</param>
/// <param name="MunicipiosComDado">Quantos municípios entraram na soma — é o que diz se ela é do recorte todo.</param>
public sealed record ProducaoAnualDoProduto(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    short Ano,
    decimal? ValorDaProducaoMilReais,
    decimal? QuantidadeProduzida,
    int MunicipiosComDado);
