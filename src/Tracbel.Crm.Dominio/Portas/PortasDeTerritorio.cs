using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O que a busca de município pede — é o que alimenta o campo de seleção do endereço.
///
/// <para><b>A busca é por prefixo, não por trecho.</b> "Ribeir" encontra Ribeirão Preto;
/// "eirão" não encontra nada. É uma decisão de desempenho com consequência de comportamento, e
/// ela está declarada aqui em vez de escondida no repositório: busca por trecho
/// (<c>LIKE '%x%'</c>) não usa índice e varreria as dez mil linhas a cada tecla digitada. Quem
/// digita o nome de uma cidade começa pelo começo.</para>
/// </summary>
/// <param name="Paginacao">A fatia pedida.</param>
/// <param name="Termo">O começo do nome do município. Nulo traz a UF inteira.</param>
/// <param name="Uf">Filtro por estado. É o que reduz a lista de 5 mil para algumas centenas.</param>
public sealed record ConsultaDeMunicipios(Paginacao Paginacao, string? Termo = null, string? Uf = null);

/// <summary>
/// A cobertura territorial de uma FILIAL — o primeiro nível do agrupamento que existe de verdade.
///
/// <para>Substitui a "regional" do protótipo, que não tem lastro: <c>IVS_Regional</c> existe no
/// sistema de origem e tem ZERO linhas. O que existe preenchido é a filial da carteira e as
/// cidades dela.</para>
/// </summary>
/// <param name="EmpresaChave">O GUID público da filial.</param>
/// <param name="EmpresaCodigo">O código da filial no ERP.</param>
/// <param name="EmpresaNome">O nome da filial.</param>
/// <param name="Carteiras">Quantas carteiras a filial tem.</param>
/// <param name="CarteirasComMunicipio">
/// Quantas delas declaram pelo menos uma cidade. A diferença para <paramref name="Carteiras"/> é
/// a medida da lacuna do cadastro de origem — e ela é grande.
/// </param>
/// <param name="Municipios">Quantos municípios distintos a filial atende.</param>
/// <param name="Ufs">Os estados alcançados, em ordem.</param>
public sealed record CoberturaDeFilial(
    Guid EmpresaChave,
    string EmpresaCodigo,
    string EmpresaNome,
    int Carteiras,
    int CarteirasComMunicipio,
    int Municipios,
    IReadOnlyList<string> Ufs);

/// <summary>Um município como a tela o mostra e como o campo de seleção o oferece.</summary>
/// <param name="Id">O identificador interno — é o que o formulário devolve ao gravar o endereço.</param>
/// <param name="Nome">Nome do município.</param>
/// <param name="Uf">Unidade federativa.</param>
/// <param name="CodigoIbge">O código do IBGE, quando conhecido. Nulo é o normal hoje.</param>
public sealed record MunicipioParaSelecao(int Id, string Nome, string Uf, int? CodigoIbge)
{
    /// <summary>Traduz a entidade para a linha de seleção.</summary>
    /// <param name="municipio">O município.</param>
    public static MunicipioParaSelecao De(Municipio municipio) =>
        new(municipio.Id, municipio.Nome, municipio.Uf, municipio.CodigoIbge);
}

/// <summary>
/// O território de uma CARTEIRA — a carteira com a filial dela e as cidades que ela atende.
/// </summary>
/// <param name="CarteiraChave">O GUID público da carteira.</param>
/// <param name="CarteiraCodigo">O código da carteira.</param>
/// <param name="CarteiraNome">O nome da carteira.</param>
/// <param name="LinhaDeNegocioNome">A linha de negócio da carteira.</param>
/// <param name="ResponsavelNome">O CEN responsável.</param>
/// <param name="EmpresaCodigo">O código da filial dona da carteira.</param>
/// <param name="EmpresaNome">O nome da filial dona da carteira.</param>
/// <param name="Municipios">As cidades atendidas, em ordem de UF e nome.</param>
public sealed record TerritorioDeCarteira(
    Guid CarteiraChave,
    string CarteiraCodigo,
    string CarteiraNome,
    string LinhaDeNegocioNome,
    string ResponsavelNome,
    string EmpresaCodigo,
    string EmpresaNome,
    IReadOnlyList<MunicipioParaSelecao> Municipios);

/// <summary>
/// O acesso ao TERRITÓRIO — o catálogo de municípios e o agrupamento real de cobertura.
///
/// <para>É porta separada de <see cref="IRepositorioCarteiras"/> de propósito: aquela responde
/// "quem está sem contato há quanto tempo", esta responde "onde a operação atua". Juntá-las
/// daria uma porta de cinco membros, que é onde o documento 22, seção 6.2, diz que quase sempre
/// há duas portas coladas.</para>
///
/// <para><b>A fronteira de multiempresa continua sendo invariante do contexto</b>, não parâmetro:
/// a cobertura por filial só enxerga as filiais que o usuário alcança, porque a carteira tem o
/// filtro global. O município em si é nacional e não é filtrado — nem deve ser: o CEN de
/// Ribeirão Preto precisa poder cadastrar um cliente em Uberaba.</para>
/// </summary>
public interface IRepositorioTerritorio
{
    /// <summary>Uma fatia do catálogo de municípios — é o que alimenta o campo de seleção.</summary>
    /// <param name="consulta">O que buscar.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<PaginaDe<MunicipioParaSelecao>> ListarMunicipiosAsync(
        ConsultaDeMunicipios consulta, CancellationToken ct);

    /// <summary>A cobertura por filial, contada no banco, dentro da fronteira de acesso.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<CoberturaDeFilial>> ResumirCoberturaPorFilialAsync(CancellationToken ct);

    /// <summary>O território de cada carteira, com as cidades.</summary>
    /// <param name="empresaCodigo">Filtro pela filial; nulo traz todas as ao alcance.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<IReadOnlyList<TerritorioDeCarteira>> ListarTerritorioPorCarteiraAsync(
        string? empresaCodigo, CancellationToken ct);
}
