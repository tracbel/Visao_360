namespace Tracbel.Crm.Dominio.Portas;

/// <summary>Uma máquina entregue — o que a "Entrega realizada" da Demanda e Previsão conta.</summary>
/// <param name="EntregueEm">O dia da entrega (ART).</param>
/// <param name="MunicipioIbge">O município do endereço principal do comprador; nulo sem endereço ou sem código IBGE.</param>
/// <param name="CategoriaCodigo">A categoria da máquina pelo de-para da linha de produto; nula quando não se sabe.</param>
public sealed record EntregaDeMaquina(DateOnly EntregueEm, int? MunicipioIbge, string? CategoriaCodigo);

/// <summary>
/// AS ENTREGAS DE MÁQUINA DO ART (a Demanda e Previsão, decisão do Ricardo em 02/10/2026) — pela data da ENTREGA, a régua da
/// GN e da meta (28/09/2026), e não a do faturamento, que é a da apuração territorial.
///
/// <para><b>O comprador diz o município</b>, pelo endereço principal — o mesmo critério dos Indicadores —, e a linha da
/// máquina diz a categoria. A venda e o comprador passam pelo filtro global: quem lê vê as entregas das filiais ao alcance.</para>
/// </summary>
public interface IRepositorioDeEntregas
{
    /// <summary>As entregas com data dentro da janela.</summary>
    /// <param name="inicio">O primeiro dia.</param>
    /// <param name="fimExclusivo">O dia seguinte ao último.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<EntregaDeMaquina>> ListarAsync(DateOnly inicio, DateOnly fimExclusivo, CancellationToken ct);
}
