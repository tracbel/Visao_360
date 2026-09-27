using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// AS VIGÊNCIAS DO PLANEJAMENTO COMERCIAL, para ler — a sazonalidade, os pesos do IOC e o share-alvo por categoria
/// (issue 256).
///
/// <para>As listas trazem tudo, inclusive o revogado e o futuro, pelo mesmo motivo das do potencial: são tabelas
/// pequenas, e escolher a vigência de uma data é regra do domínio
/// (<see cref="ParametroComVigencia.VigenteEm{T}"/>).</para>
/// </summary>
public interface IRepositorioDoPlanejamento
{
    /// <summary>Todas as vigências da sazonalidade e dos pesos, sem rastreio.</summary>
    Task<IReadOnlyList<ParametroDoPlanejamento>> ListarParametrosAsync(CancellationToken ct);

    /// <summary>Todas as vigências do share-alvo, de todas as categorias, sem rastreio.</summary>
    Task<IReadOnlyList<ShareAlvoDaCategoria>> ListarSharesAsync(CancellationToken ct);
}

/// <summary>
/// A GRAVAÇÃO DAS VIGÊNCIAS DO PLANEJAMENTO — acrescentar uma nova e achar a de pé numa data, rastreada, para revogar.
/// Como no potencial, não há "alterar": mudar é acrescentar uma vigência.
/// </summary>
public interface IRepositorioDeVigenciasDoPlanejamento
{
    /// <summary>A vigência de pé da sazonalidade e dos pesos numa data de início, rastreada, ou nula.</summary>
    Task<ParametroDoPlanejamento?> ObterParametroAsync(DateOnly vigenteDesde, CancellationToken ct);

    /// <summary>A vigência de pé do share-alvo de uma categoria numa data de início, rastreada, ou nula.</summary>
    Task<ShareAlvoDaCategoria?> ObterShareAsync(int categoriaDeMaquinaId, DateOnly vigenteDesde, CancellationToken ct);

    /// <summary>Põe uma vigência nova na unidade de trabalho. A gravação é do <see cref="IUnidadeDeTrabalho"/>.</summary>
    Task AdicionarAsync(ParametroComVigencia parametro, CancellationToken ct);
}
