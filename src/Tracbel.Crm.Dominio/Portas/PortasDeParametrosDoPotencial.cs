using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// OS PARÂMETROS DO POTENCIAL DE MERCADO — a regra por cultura, os parâmetros gerais e a percepção do
/// gestor, com as vigências (issue 71).
///
/// <para><b>As listas trazem tudo, inclusive o revogado e o futuro.</b> São tabelas pequenas (uma linha por
/// decisão), e escolher a vigência de uma data é regra do domínio
/// (<see cref="ParametroComVigencia.VigenteEm{T}"/>), não filtro de banco escrito três vezes.</para>
/// </summary>
public interface IRepositorioDeParametrosDoPotencial
{
    /// <summary>Todas as vigências das regras por cultura, sem rastreio.</summary>
    Task<IReadOnlyList<RegraDePotencial>> ListarRegrasAsync(CancellationToken ct);

    /// <summary>Todas as vigências dos parâmetros gerais, sem rastreio.</summary>
    Task<IReadOnlyList<ParametroDoPotencial>> ListarGeraisAsync(CancellationToken ct);

    /// <summary>Todas as vigências das percepções do gestor, sem rastreio.</summary>
    Task<IReadOnlyList<PercepcaoDoGestor>> ListarPercepcoesAsync(CancellationToken ct);
}

/// <summary>
/// O QUE OS PARÂMETROS CITAM — o nome de quem registrou, o município e o produto da PAM. É consulta de apoio,
/// separada das vigências: a leitura das vigências não precisa dela para escolher o que vale numa data.
/// </summary>
public interface IRepositorioDeReferenciasDoPotencial
{
    /// <summary>O nome de exibição de cada usuário pedido — quem informou e quem revogou.</summary>
    /// <param name="ids">Os usuários.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyDictionary<long, string>> NomesDosUsuariosAsync(IReadOnlyCollection<long> ids, CancellationToken ct);

    /// <summary>O código IBGE e o nome de cada município pedido.</summary>
    /// <param name="ids">Os municípios, pelo identificador interno.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyDictionary<int, MunicipioDoParametro>> MunicipiosAsync(IReadOnlyCollection<int> ids, CancellationToken ct);

    /// <summary>O município do catálogo com este código IBGE, ou nulo.</summary>
    /// <param name="codigoIbge">O código de sete dígitos.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<MunicipioDoParametro?> ObterMunicipioPorCodigoIbgeAsync(int codigoIbge, CancellationToken ct);

    /// <summary>
    /// O rótulo oficial do produto na Produção Agrícola Municipal carregada — o do ano mais recente —, ou nulo
    /// quando o produto não está carregado. A regra divide a área dele: sem área, não há o que dividir.
    /// </summary>
    /// <param name="produtoCodigoIbge">O código da classificação 782.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<string?> ObterNomeDoProdutoNaPamAsync(int produtoCodigoIbge, CancellationToken ct);
}

/// <summary>
/// A GRAVAÇÃO DAS VIGÊNCIAS — acrescentar uma nova e achar a de pé numa data, rastreada, para revogar. Não há
/// "alterar": mudar um parâmetro é acrescentar uma vigência.
/// </summary>
public interface IRepositorioDeVigenciasDoPotencial
{
    /// <summary>A vigência de pé da regra de um produto numa data de início, rastreada, ou nula.</summary>
    Task<RegraDePotencial?> ObterRegraAsync(int produtoCodigoIbge, DateOnly vigenteDesde, CancellationToken ct);

    /// <summary>A vigência de pé dos parâmetros gerais numa data de início, rastreada, ou nula.</summary>
    Task<ParametroDoPotencial?> ObterGeralAsync(DateOnly vigenteDesde, CancellationToken ct);

    /// <summary>A vigência de pé da percepção de um município numa data de início, rastreada, ou nula.</summary>
    Task<PercepcaoDoGestor?> ObterPercepcaoAsync(int municipioId, DateOnly vigenteDesde, CancellationToken ct);

    /// <summary>Põe uma vigência nova na unidade de trabalho. A gravação é do <see cref="IUnidadeDeTrabalho"/>.</summary>
    /// <param name="parametro">A vigência.</param>
    /// <param name="ct">Cancelamento.</param>
    Task AdicionarAsync(ParametroComVigencia parametro, CancellationToken ct);
}

/// <summary>Um município, como os parâmetros o mostram.</summary>
/// <param name="Id">O identificador interno.</param>
/// <param name="CodigoIbge">O código de sete dígitos.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Uf">A sigla da UF.</param>
public sealed record MunicipioDoParametro(int Id, int CodigoIbge, string Nome, string Uf);
