using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Territorio;
using Tracbel.Crm.Dominio.Seguranca;

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
            .ExigePermissao(Permissoes.CatalogoLer)
            .WithSummary("Busca município por começo do nome, opcionalmente dentro de uma UF.")
            .WithDescription(
                "Alimenta o campo de município do endereço. A busca é por PREFIXO — 'ribeir' " +
                "encontra Ribeirão Preto, 'eirão' não encontra nada — e ignora acento e caixa. " +
                "Não há rota para criar município: o catálogo é a lista oficial do Brasil, e " +
                "município que não aparece é falta de carga de catálogo, não item novo.");

        return app;
    }

    /// <summary>Registra a rota dos indicadores geográficos (documento 32).</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearIndicadoresTerritoriais(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/territorio")
            .WithTags("Território e indicadores geográficos (banco do CRM)");

        grupo.MapGet("/indicadores", async (
                ObterIndicadoresTerritoriais caso,
                CancellationToken ct,
                string? competenciaInicial = null,
                string? competenciaFinal = null,
                string? regiao = null,
                string? lojaCodigo = null,
                string? visao = null,
                string? filialDaVenda = null,
                string? filialDoCliente = null) =>
            (await caso.ExecutarAsync(
                competenciaInicial, competenciaFinal, regiao, lojaCodigo, visao, filialDaVenda, filialDoCliente, ct)).Responder())
            .WithName("ObterIndicadoresTerritoriais")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("Cobertura de visita, vendas e potencial por área, município a município.")
            .WithDescription(
                "Um item por município de SP da área de atuação ou com cliente, identificado pelo código " +
                "IBGE — o mesmo da malha do mapa. O que não tem polígono (cliente sem município, " +
                "município sem código IBGE, outra UF) vem somado em foraDoMapa, para o total fechar. " +
                "Período em competências aaaa-mm, inclusive; padrão = 12 meses fechados.");

        grupo.MapGet("/precos", async (ObterPrecosDeMercado caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterPrecosDeMercado")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("Preços das culturas em SP, mês a mês, em reais e em dólares (issue 66).")
            .WithDescription(
                "Uma série por produto, fonte e nível: preço recebido pelo produtor (CONAB), preço do kg " +
                "de ATR da cana (Socicana). O valor vem na unidade da fonte e o fator para a unidade " +
                "comercial (saca de 60 kg, caixa de 40,8 kg, arroba). O dólar é o PTAX médio do mesmo mês; " +
                "mês sem PTAX vem sem dólar. A base só cresce: mês que saiu da janela da fonte continua aqui.");

        // O PREÇO RECEBIDO PELO PRODUTOR, DA PAM (issue 198) — ROTA PRÓPRIA, e não mais um campo da de
        // preços. As duas séries não se emendam (documento 49D): a da CONAB é mensal e por UF, esta é anual
        // e por município, e a distância entre elas vai de +1,1% na soja a −28,4% no amendoim. Separadas na
        // API, elas chegam à tela como duas coisas — que é o que são.
        grupo.MapGet("/preco-implicito", async (
                int? municipioCodigoIbge, ObterPrecoImplicitoDaPam caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(municipioCodigoIbge, ct)).Responder())
            .WithName("ObterPrecoImplicitoDaPam")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("Preço recebido pelo produtor, anual e por município, derivado da PAM (issue 198).")
            .WithDescription(
                "O IBGE define o valor da produção da PAM como a média ponderada de quantidade e preço pago " +
                "ao produtor: `valor × 1000 ÷ quantidade` devolve o preço daquele ano. Sem município, soma a " +
                "ADR inteira — e a soma vem ANTES da divisão, o que dá a média ponderada pela colheita de " +
                "cada um. Traz as médias de 3 e 5 anos, que só saem com todos os anos da janela. Valor " +
                "NOMINAL, sem deflator. Não é a série mensal da CONAB e não se soma a ela.");

        // A RENTABILIDADE (issue 159): o que junta preço, custo e produtividade, que até aqui viviam em
        // três cartões separados da tela.
        grupo.MapGet("/rentabilidade", async (ObterRentabilidadeDasCulturas caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterRentabilidadeDasCulturas")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("Receita, custo e margem por hectare de cada cultura do catálogo (issue 159).")
            .WithDescription(
                "Cada número vem com a competência dele: o ano da PAM que deu a produtividade, quantos meses de " +
                "preço entraram na média e a safra do custo. A margem sai VAZIA COM O MOTIVO enquanto a cultura " +
                "não tiver local de referência e camada de custo escolhidos (D-P07) — nunca com um local " +
                "escolhido por conta própria.");

        grupo.MapGet("/custos", async (ObterCustosDeProducao caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterCustosDeProducao")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("Custo de produção das culturas em SP, por local de referência e safra (issue 67).")
            .WithDescription(
                "Uma série por cultura, local da CONAB e sistema de cultivo, com uma linha por aba da série " +
                "histórica: custo variável, fixo, operacional, renda de fatores e total por hectare, e o " +
                "operacional e o total por unidade comercial. Custo total e renda de fatores vêm nulos quando " +
                "a CONAB parou no custo operacional — não é zero.");

        grupo.MapGet("/credito", async (ObterCreditoRural caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("ObterCreditoRural")
            .ExigePermissao(Permissoes.TerritorioLer)
            .WithSummary("Crédito rural de investimento em SP, do SICOR (issue 68).")
            .WithDescription(
                "Por ano (máquinas e todos os produtos), por produto e por município (máquinas: trator, " +
                "máquinas e implementos e colheitadeiras), com os últimos 12 meses e os 12 anteriores, contados " +
                "do último mês com dado. Linha é a linha do SICOR — a soma dos contratos de uma combinação —, " +
                "não um contrato. O SICOR não identifica cliente nem revenda.");

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
            .ExigePermissao(Permissoes.CoberturaLer)
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
            .ExigePermissao(Permissoes.CoberturaLer)
            .WithSummary("O território de cada carteira: a filial dona e as cidades atendidas.")
            .WithDescription(
                "Segundo nível do mesmo agrupamento. A carteira sem cidade cadastrada aparece " +
                "com a lista vazia, de propósito — escondê-la faria a tela mostrar uma operação " +
                "menor do que ela é.");

        return app;
    }
}
