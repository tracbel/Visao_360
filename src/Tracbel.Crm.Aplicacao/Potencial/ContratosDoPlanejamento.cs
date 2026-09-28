using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>Os sete pesos do Índice de Oportunidade Comercial, como a tela os mostra.</summary>
/// <param name="Potencial">O potencial ajustado pelo momento.</param>
/// <param name="Cobertura">A lacuna de cobertura da carteira.</param>
/// <param name="Credito">O momento do crédito de mecanização.</param>
/// <param name="Rentabilidade">O momento do preço da cultura principal.</param>
/// <param name="Clientes">A falta de clientes frente ao potencial.</param>
/// <param name="Realizacao">A distância até a meta de share.</param>
/// <param name="Penetracao">A baixa penetração no potencial.</param>
public sealed record PesosDoIocDetalhe(
    decimal Potencial, decimal Cobertura, decimal Credito, decimal Rentabilidade, decimal Clientes, decimal Realizacao, decimal Penetracao)
{
    /// <summary>Monta a partir do domínio.</summary>
    /// <param name="p">Os pesos.</param>
    public static PesosDoIocDetalhe De(PesosDoIoc p) =>
        new(p.Potencial, p.Cobertura, p.Credito, p.Rentabilidade, p.Clientes, p.Realizacao, p.Penetracao);
}

/// <summary>A sazonalidade e os pesos do IOC, numa vigência (issue 256).</summary>
/// <param name="Sazonalidade">Os doze percentuais da demanda anual, de janeiro a dezembro.</param>
/// <param name="Pesos">Os pesos do IOC.</param>
/// <param name="Vigencia">Desde quando, por quê e por quem.</param>
public sealed record ParametroDoPlanejamentoDetalhe(
    IReadOnlyList<decimal> Sazonalidade,
    PesosDoIocDetalhe Pesos,
    VigenciaDoParametro Vigencia)
{
    /// <summary>Monta o detalhe.</summary>
    /// <param name="p">A vigência.</param>
    /// <param name="nomes">Nome de exibição por usuário.</param>
    public static ParametroDoPlanejamentoDetalhe De(ParametroDoPlanejamento p, IReadOnlyDictionary<long, string> nomes) =>
        new(p.Sazonalidade, PesosDoIocDetalhe.De(p.Pesos), VigenciaDoParametro.De(p, nomes));
}

/// <summary>O share-alvo de uma categoria de máquina, numa vigência (issue 256).</summary>
/// <param name="CategoriaDeMaquinaCodigo">O código da categoria — <c>TRATOR</c>, <c>COLHEITADEIRA</c>…</param>
/// <param name="CategoriaDeMaquinaNome">O nome da categoria.</param>
/// <param name="Percentual">O share-alvo, em pontos percentuais.</param>
/// <param name="Vigencia">Desde quando, por quê e por quem.</param>
public sealed record ShareAlvoDetalhe(
    string CategoriaDeMaquinaCodigo,
    string CategoriaDeMaquinaNome,
    decimal Percentual,
    VigenciaDoParametro Vigencia)
{
    /// <summary>Monta o detalhe, com a categoria por extenso.</summary>
    /// <param name="s">A vigência.</param>
    /// <param name="categorias">As categorias por identificador.</param>
    /// <param name="nomes">Nome de exibição por usuário.</param>
    public static ShareAlvoDetalhe De(
        ShareAlvoDaCategoria s, IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial> categorias, IReadOnlyDictionary<long, string> nomes)
    {
        var categoria = categorias.GetValueOrDefault(s.CategoriaDeMaquinaId);
        return new ShareAlvoDetalhe(
            categoria?.Codigo ?? $"CATEGORIA_{s.CategoriaDeMaquinaId}",
            categoria?.Nome ?? $"categoria {s.CategoriaDeMaquinaId}",
            s.Percentual,
            VigenciaDoParametro.De(s, nomes));
    }
}

/// <summary>
/// O QUE O PLANEJAMENTO USA NUMA DATA — a sazonalidade, os pesos do IOC e o share-alvo de cada categoria (issue 256).
/// </summary>
/// <param name="Em">A data consultada.</param>
/// <param name="Planejamento">A sazonalidade e os pesos vigentes; nulo quando nada valia ainda.</param>
/// <param name="Shares">O share-alvo vigente de cada categoria que tem um, na ordem do catálogo.</param>
/// <param name="Pendencias">O que falta para o planejamento sair inteiro, em frase.</param>
public sealed record ParametrosDoPlanejamentoVigentes(
    DateOnly Em,
    ParametroDoPlanejamentoDetalhe? Planejamento,
    IReadOnlyList<ShareAlvoDetalhe> Shares,
    IReadOnlyList<string> Pendencias);

/// <summary>Todas as vigências do planejamento, inclusive as revogadas e as futuras, das mais novas para as mais antigas.</summary>
/// <param name="Planejamentos">As da sazonalidade e dos pesos.</param>
/// <param name="Shares">As do share-alvo.</param>
public sealed record HistoricoDoPlanejamento(
    IReadOnlyList<ParametroDoPlanejamentoDetalhe> Planejamentos,
    IReadOnlyList<ShareAlvoDetalhe> Shares);

/// <summary>
/// Uma vigência nova da sazonalidade e dos pesos do IOC — o conjunto inteiro. Números aceitam vírgula ou ponto
/// decimal; a data é aaaa-mm-dd.
/// </summary>
/// <param name="VigenteDesde">O primeiro dia em que vale — hoje ou depois.</param>
/// <param name="Sazonalidade">Os doze percentuais, de janeiro a dezembro, somando 100.</param>
/// <param name="PesoDoPotencial">De 0 a 100.</param>
/// <param name="PesoDaCobertura">De 0 a 100.</param>
/// <param name="PesoDoCredito">De 0 a 100.</param>
/// <param name="PesoDaRentabilidade">De 0 a 100.</param>
/// <param name="PesoDosClientes">De 0 a 100.</param>
/// <param name="PesoDaRealizacao">De 0 a 100.</param>
/// <param name="PesoDaPenetracao">De 0 a 100.</param>
/// <param name="Justificativa">Por que estes valores.</param>
public sealed record NovoParametroDoPlanejamento(
    string? VigenteDesde = null,
    IReadOnlyList<string?>? Sazonalidade = null,
    string? PesoDoPotencial = null,
    string? PesoDaCobertura = null,
    string? PesoDoCredito = null,
    string? PesoDaRentabilidade = null,
    string? PesoDosClientes = null,
    string? PesoDaRealizacao = null,
    string? PesoDaPenetracao = null,
    string? Justificativa = null);

/// <summary>Uma vigência nova do share-alvo de uma categoria.</summary>
/// <param name="CategoriaDeMaquinaCodigo">A categoria — <c>TRATOR</c>, <c>COLHEITADEIRA</c>…</param>
/// <param name="Percentual">O share-alvo, em pontos percentuais (ex.: 31).</param>
/// <param name="VigenteDesde">O primeiro dia em que vale — hoje ou depois.</param>
/// <param name="Justificativa">Por que este share.</param>
public sealed record NovoShareAlvo(
    string? CategoriaDeMaquinaCodigo = null,
    string? Percentual = null,
    string? VigenteDesde = null,
    string? Justificativa = null);
