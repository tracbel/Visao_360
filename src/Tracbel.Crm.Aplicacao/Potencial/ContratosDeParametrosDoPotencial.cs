using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Potencial;

/// <summary>
/// O que toda vigência mostra: desde quando vale, por quê, quem registrou e, se for o caso, quem revogou.
/// </summary>
/// <param name="VigenteDesde">O primeiro dia em que vale.</param>
/// <param name="Justificativa">Por que este valor.</param>
/// <param name="InformadoPor">
/// O nome de quem registrou. <b>Nulo na semente da migração</b>, que não foi digitada por usuário do CRM —
/// a origem dela está na justificativa.
/// </param>
/// <param name="InformadoEm">Quando foi registrada (UTC).</param>
/// <param name="RevogadoEm">Quando foi revogada; nulo é vigência de pé.</param>
/// <param name="RevogadoPor">Quem revogou.</param>
/// <param name="MotivoDaRevogacao">Por que foi revogada.</param>
public sealed record VigenciaDoParametro(
    DateOnly VigenteDesde,
    string Justificativa,
    string? InformadoPor,
    DateTime InformadoEm,
    DateTime? RevogadoEm,
    string? RevogadoPor,
    string? MotivoDaRevogacao)
{
    /// <summary>Monta a vigência de um parâmetro, com os nomes já resolvidos.</summary>
    /// <param name="parametro">O parâmetro.</param>
    /// <param name="nomes">Nome de exibição por usuário.</param>
    public static VigenciaDoParametro De(ParametroComVigencia parametro, IReadOnlyDictionary<long, string> nomes) => new(
        parametro.VigenteDesde,
        parametro.Justificativa,
        Nome(parametro.InformadoPorId, nomes),
        parametro.InformadoEm,
        parametro.RevogadoEm,
        Nome(parametro.RevogadoPorId, nomes),
        parametro.MotivoDaRevogacao);

    private static string? Nome(long? id, IReadOnlyDictionary<long, string> nomes) =>
        id is { } usuario ? nomes.GetValueOrDefault(usuario, $"usuário {usuario}") : null;
}

/// <summary>A regra de potencial de uma cultura, numa vigência.</summary>
/// <param name="ProdutoCodigoIbge">O produto, na classificação 782 do IBGE.</param>
/// <param name="ProdutoNome">O rótulo oficial do produto.</param>
/// <param name="HectaresPorMaquina">Hectares por máquina de referência.</param>
/// <param name="AnosDeRenovacao">Anos de renovação; nulo quando ninguém informou.</param>
/// <param name="ModeloDeReferencia">O modelo de referência.</param>
/// <param name="Situacao">AConfirmar ou Confirmada.</param>
/// <param name="Vigencia">Desde quando, por quê e por quem.</param>
/// <param name="CulturaCodigo">O código da cultura do catálogo; nulo nas vigências anteriores a ele.</param>
/// <param name="CulturaNome">O nome da cultura, para a tela não precisar cruzar a lista.</param>
/// <param name="CategoriaDeMaquinaCodigo">O código da categoria de máquina (D-P01); nulo nas anteriores.</param>
/// <param name="CategoriaDeMaquinaNome">O nome da categoria.</param>
public sealed record RegraDePotencialDetalhe(
    int ProdutoCodigoIbge,
    string ProdutoNome,
    decimal HectaresPorMaquina,
    decimal? AnosDeRenovacao,
    string ModeloDeReferencia,
    string Situacao,
    VigenciaDoParametro Vigencia,
    string? CulturaCodigo = null,
    string? CulturaNome = null,
    string? CategoriaDeMaquinaCodigo = null,
    string? CategoriaDeMaquinaNome = null)
{
    /// <summary>Monta o detalhe.</summary>
    /// <param name="regra">A regra.</param>
    /// <param name="nomes">Nome de exibição por usuário.</param>
    /// <param name="culturas">As culturas do catálogo, por identificador; ausente deixa o rótulo nulo.</param>
    /// <param name="categorias">As categorias de máquina, por identificador.</param>
    public static RegraDePotencialDetalhe De(
        RegraDePotencial regra,
        IReadOnlyDictionary<long, string> nomes,
        IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial>? culturas = null,
        IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial>? categorias = null)
    {
        var cultura = Achar(culturas, regra.CulturaId);
        var categoria = Achar(categorias, regra.CategoriaDeMaquinaId);

        return new RegraDePotencialDetalhe(
            regra.ProdutoCodigoIbge, regra.ProdutoNome, regra.HectaresPorMaquina, regra.AnosDeRenovacao,
            regra.ModeloDeReferencia, regra.Situacao.ToString(), VigenciaDoParametro.De(regra, nomes),
            cultura?.Codigo, cultura?.Nome, categoria?.Codigo, categoria?.Nome);
    }

    private static ItemDoCatalogoDoPotencial? Achar(
        IReadOnlyDictionary<int, ItemDoCatalogoDoPotencial>? mapa, int? id) =>
        mapa is not null && id is { } chave && mapa.TryGetValue(chave, out var item) ? item : null;
}

/// <summary>Os parâmetros gerais do modelo, numa vigência. Os campos em aberto vêm nulos.</summary>
/// <param name="MesesDaJanela">Meses de cada lado do índice (12 é "12 contra 12").</param>
/// <param name="PesoDosContratosNoCredito">Peso da quantidade de contratos no índice de crédito, de 0 a 1.</param>
/// <param name="PesoDoValorNoCredito">O que sobra para o valor financiado — sempre 1 menos o anterior.</param>
/// <param name="LimiteDeRetracao">Abaixo dele, retraído.</param>
/// <param name="LimiteDeAquecimento">Acima dele, aquecido.</param>
/// <param name="LimiteDeSuperaquecimento">Acima dele, superaquecido.</param>
/// <param name="NomeDaFaixaIntermediaria">O nome da faixa do meio, quando decidido.</param>
/// <param name="LimiteDaPercepcao">O maior ajuste do gestor, em pontos percentuais.</param>
/// <param name="PesoDoIndicadorDePreco">Peso do indicador 1, quando decidido.</param>
/// <param name="PesoDoIndicadorDeCredito">Peso do indicador 2, quando decidido.</param>
/// <param name="PesoDoIndicadorComercial">Peso do indicador 3, quando decidido.</param>
/// <param name="FatorMinimo">O menor fator de ciclo, quando decidido.</param>
/// <param name="FatorMaximo">O maior fator de ciclo, quando decidido.</param>
/// <param name="MesesDeCarenciaDoSicor">Meses recentes do SICOR fora da janela; nulo é "não decidida" (D-IM-03).</param>
/// <param name="Vigencia">Desde quando, por quê e por quem.</param>
public sealed record ParametrosGeraisDetalhe(
    short MesesDaJanela,
    decimal PesoDosContratosNoCredito,
    decimal PesoDoValorNoCredito,
    decimal LimiteDeRetracao,
    decimal LimiteDeAquecimento,
    decimal LimiteDeSuperaquecimento,
    string? NomeDaFaixaIntermediaria,
    decimal LimiteDaPercepcao,
    decimal? PesoDoIndicadorDePreco,
    decimal? PesoDoIndicadorDeCredito,
    decimal? PesoDoIndicadorComercial,
    decimal? FatorMinimo,
    decimal? FatorMaximo,
    short? MesesDeCarenciaDoSicor,
    VigenciaDoParametro Vigencia)
{
    /// <summary>Monta o detalhe.</summary>
    /// <param name="p">Os parâmetros.</param>
    /// <param name="nomes">Nome de exibição por usuário.</param>
    public static ParametrosGeraisDetalhe De(ParametroDoPotencial p, IReadOnlyDictionary<long, string> nomes) => new(
        p.MesesDaJanela, p.PesoDosContratosNoCredito, 1 - p.PesoDosContratosNoCredito,
        p.LimiteDeRetracao, p.LimiteDeAquecimento, p.LimiteDeSuperaquecimento, p.NomeDaFaixaIntermediaria,
        p.LimiteDaPercepcao, p.PesoDoIndicadorDePreco, p.PesoDoIndicadorDeCredito, p.PesoDoIndicadorComercial,
        p.FatorMinimo, p.FatorMaximo, p.MesesDeCarenciaDoSicor, VigenciaDoParametro.De(p, nomes));
}

/// <summary>A percepção do gestor sobre um município, numa vigência.</summary>
/// <param name="MunicipioCodigoIbge">O código IBGE do município.</param>
/// <param name="MunicipioNome">O nome.</param>
/// <param name="Uf">A UF.</param>
/// <param name="Percentual">O ajuste, em pontos percentuais.</param>
/// <param name="Vigencia">Desde quando, por quê e por quem.</param>
public sealed record PercepcaoDoGestorDetalhe(
    int MunicipioCodigoIbge,
    string MunicipioNome,
    string Uf,
    decimal Percentual,
    VigenciaDoParametro Vigencia);

/// <summary>
/// OS PARÂMETROS QUE VALEM NUMA DATA — o conjunto que um cálculo daquela data usa (issue 71: "o cálculo de
/// uma data passada usa o parâmetro vigente naquela data").
/// </summary>
/// <param name="Em">A data consultada.</param>
/// <param name="Geral">Os parâmetros gerais vigentes; nulo quando nada valia ainda.</param>
/// <param name="Culturas">A regra vigente de cada cultura que tem regra.</param>
/// <param name="Percepcoes">A percepção vigente de cada município que tem uma.</param>
/// <param name="Pendencias">
/// O que falta decidir para o potencial sair completo — dito em frase, e não escondido num valor padrão.
/// </param>
public sealed record ParametrosDoPotencialVigentes(
    DateOnly Em,
    ParametrosGeraisDetalhe? Geral,
    IReadOnlyList<RegraDePotencialDetalhe> Culturas,
    IReadOnlyList<PercepcaoDoGestorDetalhe> Percepcoes,
    IReadOnlyList<string> Pendencias);

/// <summary>Todas as vigências já registradas, inclusive as revogadas e as futuras, das mais novas para as mais antigas.</summary>
/// <param name="Gerais">As dos parâmetros gerais.</param>
/// <param name="Culturas">As das regras por cultura.</param>
/// <param name="Percepcoes">As das percepções do gestor.</param>
public sealed record HistoricoDosParametrosDoPotencial(
    IReadOnlyList<ParametrosGeraisDetalhe> Gerais,
    IReadOnlyList<RegraDePotencialDetalhe> Culturas,
    IReadOnlyList<PercepcaoDoGestorDetalhe> Percepcoes);

/// <summary>
/// Uma vigência nova dos parâmetros gerais. Vem o conjunto inteiro: quem muda um valor manda os outros como
/// estão. Números aceitam vírgula ou ponto decimal; a data é aaaa-mm-dd.
/// </summary>
/// <param name="VigenteDesde">O primeiro dia em que vale — hoje ou depois.</param>
/// <param name="MesesDaJanela">Meses de cada lado do índice.</param>
/// <param name="PesoDosContratosNoCredito">De 0 a 1.</param>
/// <param name="LimiteDeRetracao">Ex.: 1,00.</param>
/// <param name="LimiteDeAquecimento">Ex.: 1,20.</param>
/// <param name="LimiteDeSuperaquecimento">Ex.: 1,40.</param>
/// <param name="NomeDaFaixaIntermediaria">Opcional.</param>
/// <param name="LimiteDaPercepcao">Em pontos percentuais. Ex.: 5.</param>
/// <param name="PesoDoIndicadorDePreco">Opcional.</param>
/// <param name="PesoDoIndicadorDeCredito">Opcional.</param>
/// <param name="PesoDoIndicadorComercial">Opcional.</param>
/// <param name="FatorMinimo">Opcional, junto com o máximo.</param>
/// <param name="FatorMaximo">Opcional, junto com o mínimo.</param>
/// <param name="MesesDeCarenciaDoSicor">Meses recentes do SICOR fora da janela; vazio é "não decidida" (D-IM-03).</param>
/// <param name="Justificativa">Por que estes valores.</param>
public sealed record NovoParametroDoPotencial(
    string? VigenteDesde = null,
    string? MesesDaJanela = null,
    string? PesoDosContratosNoCredito = null,
    string? LimiteDeRetracao = null,
    string? LimiteDeAquecimento = null,
    string? LimiteDeSuperaquecimento = null,
    string? NomeDaFaixaIntermediaria = null,
    string? LimiteDaPercepcao = null,
    string? PesoDoIndicadorDePreco = null,
    string? PesoDoIndicadorDeCredito = null,
    string? PesoDoIndicadorComercial = null,
    string? FatorMinimo = null,
    string? FatorMaximo = null,
    string? MesesDeCarenciaDoSicor = null,
    string? Justificativa = null);

/// <summary>Uma vigência nova da regra de uma cultura.</summary>
/// <param name="ProdutoCodigoIbge">O código do produto na classificação 782 (ex.: 40139, café em grão).</param>
/// <param name="HectaresPorMaquina">Hectares por máquina de referência.</param>
/// <param name="AnosDeRenovacao">Opcional.</param>
/// <param name="ModeloDeReferencia">O modelo, como o negócio o escreve.</param>
/// <param name="Situacao">AConfirmar ou Confirmada.</param>
/// <param name="VigenteDesde">O primeiro dia em que vale — hoje ou depois.</param>
/// <param name="Justificativa">Por que estes valores.</param>
/// <param name="CulturaCodigo">A cultura do catálogo — <c>CAFE</c>, <c>CANA</c>… (issue 165).</param>
/// <param name="CategoriaDeMaquinaCodigo">A categoria — <c>TRATOR</c>, <c>COLHEITADEIRA</c>… (D-P01).</param>
public sealed record NovaRegraDePotencial(
    string? ProdutoCodigoIbge = null,
    string? HectaresPorMaquina = null,
    string? AnosDeRenovacao = null,
    string? ModeloDeReferencia = null,
    string? Situacao = null,
    string? VigenteDesde = null,
    string? Justificativa = null,
    string? CulturaCodigo = null,
    string? CategoriaDeMaquinaCodigo = null);

/// <summary>Uma vigência nova da percepção do gestor sobre um município.</summary>
/// <param name="MunicipioCodigoIbge">O código IBGE de sete dígitos.</param>
/// <param name="Percentual">O ajuste, em pontos percentuais (ex.: -2,5).</param>
/// <param name="VigenteDesde">O primeiro dia em que vale — hoje ou depois.</param>
/// <param name="Justificativa">Por que este ajuste.</param>
public sealed record NovaPercepcaoDoGestor(
    string? MunicipioCodigoIbge = null,
    string? Percentual = null,
    string? VigenteDesde = null,
    string? Justificativa = null);

/// <summary>A revogação de uma vigência que ainda não passou de hoje.</summary>
/// <param name="Motivo">Por que — fica na trilha.</param>
public sealed record RevogacaoDeVigencia(string? Motivo = null);
