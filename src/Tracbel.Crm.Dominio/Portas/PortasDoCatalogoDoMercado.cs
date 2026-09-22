using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// O CATÁLOGO DE CULTURAS E CATEGORIAS DE MÁQUINA (issue 165).
///
/// <para>A leitura devolve o catálogo já montado para a tela — é ela que substitui as listas fixas de
/// cultura que viviam no front. A gravação é cultura a cultura, e <b>não há exclusão</b>: cultura que sai
/// de cena é desligada, porque o histórico de preço, custo e potencial continua explicando o passado.</para>
/// </summary>
public interface IRepositorioDoCatalogoDoMercado
{
    /// <summary>O catálogo inteiro, como a tela o mostra.</summary>
    /// <param name="ct">Cancelamento.</param>
    Task<CatalogoDoMercado> LerAsync(CancellationToken ct);

    /// <summary>A cultura de um código, rastreada para alteração; nula quando não existe.</summary>
    /// <param name="codigo">O código estável.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<Cultura?> ObterPorCodigoAsync(string codigo, CancellationToken ct);

    /// <summary>A cultura de um código, já montada para a tela.</summary>
    /// <param name="codigo">O código estável.</param>
    /// <param name="ct">Cancelamento.</param>
    Task<CulturaNoCatalogo> ObterNaTelaAsync(string codigo, CancellationToken ct);

    /// <summary>Põe uma cultura nova na unidade de trabalho.</summary>
    /// <param name="cultura">A cultura.</param>
    /// <param name="ct">Cancelamento.</param>
    Task AdicionarAsync(Cultura cultura, CancellationToken ct);
}

/// <summary>O catálogo inteiro: as culturas e as categorias de máquina.</summary>
/// <param name="Culturas">As culturas, em ordem de nome.</param>
/// <param name="Categorias">As categorias, na ordem de exibição.</param>
public sealed record CatalogoDoMercado(
    IReadOnlyList<CulturaNoCatalogo> Culturas, IReadOnlyList<CategoriaNoCatalogo> Categorias);

/// <summary>
/// UMA CULTURA DO CATÁLOGO, como a tela a mostra (issue 165).
///
/// <para>É este contrato que tira as listas fixas de cultura do front: a tela pede o catálogo e desenha o
/// que vier, em vez de trazer "CAFE", "SOJA" e "MILHO" escritos no código.</para>
/// </summary>
/// <param name="Codigo">O código estável.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="Segmento">O segmento do comercial.</param>
/// <param name="UnidadeComercial">A unidade em que o mercado negocia.</param>
/// <param name="QuilosPorUnidade">Quantos quilos tem a unidade — é o que converte o R$/kg da CONAB.</param>
/// <param name="FonteDoPreco">CONAB, SOCICANA ou nulo quando nenhuma publica.</param>
/// <param name="ProdutoDoPreco">O identificador do produto na fonte de preço.</param>
/// <param name="SerieDeCusto">O rótulo da série de custo da CONAB, quando existe.</param>
/// <param name="EstaAtiva">Se aparece nas telas.</param>
/// <param name="Produtos">Os produtos da PAM que a compõem.</param>
public sealed record CulturaNoCatalogo(
    string Codigo,
    string Nome,
    string Segmento,
    string UnidadeComercial,
    decimal QuilosPorUnidade,
    string? FonteDoPreco,
    string? ProdutoDoPreco,
    string? SerieDeCusto,
    bool EstaAtiva,
    IReadOnlyList<ProdutoDaPamNoCatalogo> Produtos);

/// <summary>Um produto da PAM dentro de uma cultura.</summary>
/// <param name="CodigoIbge">O código na classificação 782.</param>
/// <param name="Nome">O rótulo oficial do IBGE.</param>
/// <param name="EntraNaSomaDaLavoura">Se entra na soma — falso nos detalhados de um total.</param>
public sealed record ProdutoDaPamNoCatalogo(int CodigoIbge, string Nome, bool EntraNaSomaDaLavoura);

/// <summary>Uma categoria de máquina, como a tela a mostra.</summary>
/// <param name="Codigo">O código estável.</param>
/// <param name="Nome">O nome de exibição.</param>
/// <param name="Ordem">A ordem de exibição.</param>
/// <param name="EstaAtiva">Se aparece nas telas.</param>
/// <param name="ProdutosDoSicor">
/// Os produtos do SICOR que ela agrupa. <b>Vazio não é erro</b>: o investimento do Banco Central não
/// separa plantadeira, pulverizador nem agricultura de precisão (anexo 49C).
/// </param>
public sealed record CategoriaNoCatalogo(
    string Codigo, string Nome, short Ordem, bool EstaAtiva, IReadOnlyList<int> ProdutosDoSicor);
