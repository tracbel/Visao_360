namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O PREÇO DE COMMODITIES — a série mensal de cada cultura do catálogo, a mesma que mede o momento de preço do fator de ciclo
/// (issue 260, a aba "Preço de Commodities" do protótipo da pasta 360).
///
/// <para><b>A série é a do índice quando a cultura declara uma, e senão a do preço</b> — a mesma escolha do momento de preço
/// (<c>RepositorioDeIndicadoresDeMercado</c>): a cana pelo ATR mensal da Socicana, as outras pela CONAB. Duas séries
/// diferentes para a mesma cultura fariam o R12 desta tela discordar do momento dos Indicadores.</para>
/// </summary>
public interface IRepositorioDosPrecosDasCulturas
{
    /// <summary>As culturas ativas do catálogo com fonte de preço, cada uma com a série dela, do mês mais antigo ao mais novo.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<SerieDaCultura>> LerSeriesAsync(CancellationToken ct);
}

/// <summary>Um mês da série de uma cultura.</summary>
/// <param name="Mes">O mês, no dia 1.</param>
/// <param name="Valor">O valor em reais, na unidade da série.</param>
public sealed record PrecoDaCulturaNoMes(DateOnly Mes, decimal Valor);

/// <summary>A série de preço de uma cultura.</summary>
/// <param name="Codigo">O código da cultura no catálogo.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Fonte">CONAB, SOCICANA…</param>
/// <param name="Produto">O produto na fonte.</param>
/// <param name="Unidade">A unidade em que a fonte publica — "R$/60 kg", "R$/kg ATR"…; vazia sem série.</param>
/// <param name="Meses">Os meses, do mais antigo ao mais novo; vazio quando a fonte ainda não publicou nada carregado.</param>
public sealed record SerieDaCultura(
    string Codigo,
    string Nome,
    string Fonte,
    string Produto,
    string Unidade,
    IReadOnlyList<PrecoDaCulturaNoMes> Meses);
