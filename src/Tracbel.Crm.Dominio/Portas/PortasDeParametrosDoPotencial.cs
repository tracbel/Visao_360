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
/// O CATÁLOGO VISTO PELOS PARÂMETROS (D-P01, issue 63) — a cultura e a categoria de máquina de uma regra.
///
/// <para><b>Por que é uma porta própria</b>, e não mais quatro métodos na de referências: aquela responde
/// "o que os parâmetros citam" — usuário, município, produto da PAM. Esta responde "qual é a cultura e de
/// qual máquina estamos falando", que é o par que a D-P01 fixa. São duas perguntas, e o portão de
/// arquitetura do documento 22 §6.2 recusa a porta que gruda as duas.</para>
/// </summary>
public interface IRepositorioDoCatalogoNoPotencial
{
    /// <summary>
    /// A cultura ATIVA do catálogo com este código estável, ou nula (issue 165).
    ///
    /// <para>Cultura desligada não volta a receber regra: o histórico dela continua explicando os números
    /// do passado, e uma vigência nova apontando para uma cultura fora de cena é decisão sem dono.</para>
    /// </summary>
    /// <param name="codigo">O código estável — <c>CAFE</c>, <c>CANA</c>…</param>
    /// <param name="ct">Cancelamento.</param>
    Task<ItemDoCatalogoDoPotencial?> ObterCulturaAtivaAsync(string codigo, CancellationToken ct);

    /// <summary>
    /// A categoria de máquina com este código estável, ou nula (D-IM-06).
    ///
    /// <para><paramref name="somenteAtiva"/> separa duas perguntas que parecem uma: registrar vigência nova
    /// é decisão <b>para a frente</b> e exige categoria ativa; <b>revogar</b> uma vigência de uma categoria
    /// que foi desligada depois precisa continuar sendo possível.</para>
    /// </summary>
    /// <param name="codigo">O código estável — <c>TRATOR</c>, <c>COLHEITADEIRA</c>…</param>
    /// <param name="somenteAtiva">Se recusa a categoria desligada.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<ItemDoCatalogoDoPotencial?> ObterCategoriaDeMaquinaAsync(string codigo, bool somenteAtiva, CancellationToken ct);

    /// <summary>O código e o nome de cada cultura pedida, para rotular as regras. Inclusive as desligadas.</summary>
    /// <param name="ids">As culturas, pelo identificador interno.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial>> CulturasAsync(IReadOnlyCollection<int> ids, CancellationToken ct);

    /// <summary>O código e o nome de cada categoria de máquina pedida, para rotular as regras.</summary>
    /// <param name="ids">As categorias, pelo identificador interno.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial>> CategoriasDeMaquinaAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
}

/// <summary>
/// A GRAVAÇÃO DAS VIGÊNCIAS — acrescentar uma nova e achar a de pé numa data, rastreada, para revogar. Não há
/// "alterar": mudar um parâmetro é acrescentar uma vigência.
/// </summary>
public interface IRepositorioDeVigenciasDoPotencial
{
    /// <summary>
    /// A vigência de pé da regra de um produto NUMA CATEGORIA, numa data de início, rastreada, ou nula.
    ///
    /// <para><b>A categoria entra na chave (D-P01).</b> No mesmo café cabem "um trator a cada 10 ha" e "uma
    /// colheitadeira a cada 200 ha": são duas regras do mesmo produto, na mesma data. Sem a categoria aqui, a
    /// segunda seria recusada como data ocupada — e a decisão que a #63 pede não caberia no sistema.</para>
    /// </summary>
    /// <param name="produtoCodigoIbge">O produto da classificação 782.</param>
    /// <param name="categoriaDeMaquinaId">A categoria; nulo procura a regra sem categoria (as anteriores ao catálogo).</param>
    /// <param name="vigenteDesde">A data de início.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<RegraDePotencial?> ObterRegraAsync(int produtoCodigoIbge, int? categoriaDeMaquinaId, DateOnly vigenteDesde, CancellationToken ct);

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

/// <summary>Uma cultura ou uma categoria de máquina do catálogo, como os parâmetros a citam.</summary>
/// <param name="Id">O identificador interno, que é o que a regra guarda.</param>
/// <param name="Codigo">O código estável — a identidade do catálogo, que não muda.</param>
/// <param name="Nome">O nome de exibição.</param>
public sealed record ItemDoCatalogoDoPotencial(int Id, string Codigo, string Nome);
