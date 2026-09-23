using Tracbel.Crm.Dominio.Mercado;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// A RENTABILIDADE POR CULTURA (issue 159) — receita, custo e margem por hectare, com a competência de
/// cada parte.
/// </summary>
public interface IRepositorioDeRentabilidade
{
    /// <summary>A rentabilidade de cada cultura ativa do catálogo.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<RentabilidadeDaCultura>> LerAsync(CancellationToken ct);
}

/// <summary>
/// A RENTABILIDADE DE UMA CULTURA, com o rastro de cada parcela.
///
/// <para><b>As três competências andam juntas do número</b>: o ano da PAM que deu a produtividade, os
/// meses de preço que entraram na média e a safra do custo. Elas costumam ser diferentes, e um número
/// que não diz de quando é não pode ser conferido.</para>
///
/// <para><b>Margem nula vem com <see cref="Motivo"/>.</b> É a regra que atravessa a Inteligência de
/// Mercado: dado ausente ou parâmetro não decidido devolve vazio com a frase, nunca um número inventado.</para>
/// </summary>
/// <param name="CulturaCodigo">O código da cultura no catálogo.</param>
/// <param name="CulturaNome">O nome de exibição.</param>
/// <param name="UnidadeComercial">A unidade em que o mercado negocia — só para mostrar.</param>
/// <param name="AnoDaProdutividade">O ano da PAM de onde veio a produtividade.</param>
/// <param name="ProdutividadeKgPorHa">Quilos por hectare colhido; nula quando a PAM não permite calcular.</param>
/// <param name="PrecoMedioPorKg">A média dos meses de preço do ano, em R$/kg.</param>
/// <param name="MesesDePrecoNaMedia">Quantos meses entraram na média.</param>
/// <param name="ReceitaPorHectare">Produtividade × preço.</param>
/// <param name="LocalDoCusto">O local de referência da CONAB; nulo enquanto ninguém decide (D-P07).</param>
/// <param name="CamadaDoCusto">Operacional ou Total; nula enquanto ninguém decide.</param>
/// <param name="SafraDoCusto">A safra da série de custo usada.</param>
/// <param name="CustoPorHectare">O custo por hectare na camada escolhida.</param>
/// <param name="MargemPorHectare">Receita menos custo.</param>
/// <param name="MargemPorUnidade">
/// A mesma margem na unidade em que o mercado negocia — por saca, caixa ou tonelada (issue 73). A margem
/// por hectare responde "a terra paga a conta?"; esta responde "cada saca que eu vendo sobra quanto?".
/// </param>
/// <param name="AreaColhidaHectares">A área colhida de São Paulo, que multiplica a margem.</param>
/// <param name="MargemTotal">Margem por hectare × área colhida.</param>
/// <param name="Motivo">Por que a margem não saiu, como TEXTO; <c>Nenhum</c> quando saiu — a convenção do contrato é enum em texto.</param>
/// <param name="FraseDoMotivo">A frase que a tela mostra no lugar do número.</param>
public sealed record RentabilidadeDaCultura(
    string CulturaCodigo,
    string CulturaNome,
    string UnidadeComercial,
    short? AnoDaProdutividade,
    decimal? ProdutividadeKgPorHa,
    decimal? PrecoMedioPorKg,
    int MesesDePrecoNaMedia,
    decimal? ReceitaPorHectare,
    string? LocalDoCusto,
    string? CamadaDoCusto,
    short? SafraDoCusto,
    decimal? CustoPorHectare,
    decimal? MargemPorHectare,
    decimal? MargemPorUnidade,
    decimal? AreaColhidaHectares,
    decimal? MargemTotal,
    string Motivo,
    string FraseDoMotivo);
