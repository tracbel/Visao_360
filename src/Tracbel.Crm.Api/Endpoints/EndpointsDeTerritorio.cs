using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Territorio;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// As rotas de TERRITÓRIO — o catálogo de municípios e a cobertura por filial e por carteira.
///
/// <para><b>Não existe rota de regional, e é o achado que este arquivo representa.</b> A tela de
/// Cobertura mostra hoje sete regionais que vieram do protótipo; no sistema de origem a tabela de
/// regional existe e tem ZERO linhas. O que está preenchido é a filial da carteira
/// (<c>IVS_Carteira.NroEmpresa</c>) e as cidades dela (<c>IVS_CartCid</c>), e é esse agrupamento
/// que estas rotas entregam. Documento 26.</para>
///
/// <para><b>A rota de municípios é a que fecha o campo de endereço.</b> Enquanto ela não existir,
/// a tela não tem de onde puxar a lista e o município volta a ser digitado — que é exatamente
/// como a coluna de texto livre nasceu no legado (documento 16, seção 3).</para>
/// </summary>
public static class EndpointsDeTerritorio
{
    /// <summary>Registra a rota do catálogo de municípios.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearMunicipios(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/municipios")
            .WithTags("Municípios (catálogo nacional)");

        grupo.MapGet("/", async (
                ListarMunicipios caso,
                CancellationToken ct,
                string? termo = null,
                string? uf = null,
                int? pagina = null,
                int? tamanho = null) =>
            (await caso.ExecutarAsync(pagina, tamanho, termo, uf, ct)).Responder())
            .WithName("ListarMunicipios")
            .WithSummary("Busca município por começo do nome, opcionalmente dentro de uma UF.")
            .WithDescription(
                "Alimenta o campo de município do endereço. A busca é por PREFIXO — 'ribeir' " +
                "encontra Ribeirão Preto, 'eirão' não encontra nada — e ignora acento e caixa. " +
                "Não há rota para criar município: o catálogo é a lista oficial do Brasil, e " +
                "município que não aparece é falta de carga de catálogo, não item novo.");

        return app;
    }

    /// <summary>Registra as rotas de cobertura territorial.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearCoberturaTerritorial(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/cobertura")
            .WithTags("Cobertura de carteira (banco do CRM)");

        grupo.MapGet("/filiais", async (ObterCoberturaPorFilial caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterCoberturaPorFilial")
            .WithSummary("A cobertura agrupada por filial: carteiras, municípios atendidos e estados.")
            .WithDescription(
                "É o primeiro nível do agrupamento REAL, que substitui a regional do protótipo. " +
                "Quando parte das carteiras não declara município, a resposta traz o número em " +
                "metricasSemDado em vez de mostrar uma cobertura menor sem dizer por quê.");

        grupo.MapGet("/carteiras", async (
                ListarTerritorioPorCarteira caso,
                CancellationToken ct,
                string? empresaCodigo = null) =>
            (await caso.ExecutarAsync(empresaCodigo, ct)).Responder())
            .WithName("ListarTerritorioPorCarteira")
            .WithSummary("O território de cada carteira: a filial dona e as cidades atendidas.")
            .WithDescription(
                "Segundo nível do mesmo agrupamento. A carteira sem cidade cadastrada aparece " +
                "com a lista vazia, de propósito — escondê-la faria a tela mostrar uma operação " +
                "menor do que ela é.");

        return app;
    }
}
