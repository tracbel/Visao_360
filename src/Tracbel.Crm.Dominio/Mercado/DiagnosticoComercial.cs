using System.Globalization;
using Tracbel.Crm.Dominio.Organizacao;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>As cinco classes de prioridade do IOC, da mais urgente para a de manutenção.</summary>
public enum ClasseDePrioridade
{
    /// <summary>IOC de 80 em diante.</summary>
    Maxima,

    /// <summary>IOC de 60 a 80.</summary>
    Alta,

    /// <summary>IOC de 40 a 60.</summary>
    Moderada,

    /// <summary>IOC de 20 a 40.</summary>
    Baixa,

    /// <summary>IOC abaixo de 20.</summary>
    Manutencao
}

/// <summary>
/// O QUE O IOC PRECISA SABER DE UM MUNICÍPIO — tudo já apurado pelas leituras do CRM; aqui só há conta.
/// </summary>
/// <param name="CodigoIbge">O município.</param>
/// <param name="DemandaEstrutural">Máquinas por ano que a área pede, nas categorias do diagnóstico; nula sem regra ou sem área.</param>
/// <param name="DemandaAjustada">A mesma demanda ajustada pelo momento do município (fator de ciclo); nula sem fator.</param>
/// <param name="MetaDePlanejamento">Demanda ajustada × share-alvo, somada nas categorias com share; nula sem share.</param>
/// <param name="VendidasNoAno">Máquinas que a Tracbel vendeu aqui, pelo ART, levadas a um ano; nula sem carga do ART.</param>
/// <param name="Clientes">Clientes com endereço principal no município.</param>
/// <param name="VinculosComCadencia">Vínculos de carteira com cadência declarada — os que a cobertura mede.</param>
/// <param name="Cobertos">Deles, os com contato dentro da cadência.</param>
/// <param name="IndiceDeCredito">O índice de crédito do município (1,00 é igual à janela anterior); nulo sem base.</param>
/// <param name="IndiceDePrecoDaPrincipal">O momento de preço da cultura principal (1,00 é estável); nulo sem série.</param>
/// <param name="CulturaPrincipal">O nome da cultura de maior área útil, para a frase.</param>
public sealed record MunicipioParaOIoc(
    int CodigoIbge,
    decimal? DemandaEstrutural,
    decimal? DemandaAjustada,
    decimal? MetaDePlanejamento,
    decimal? VendidasNoAno,
    int Clientes,
    int VinculosComCadencia,
    int Cobertos,
    decimal? IndiceDeCredito,
    decimal? IndiceDePrecoDaPrincipal,
    string? CulturaPrincipal)
{
    /// <summary>A demanda que o IOC compara: a ajustada quando existe, senão a estrutural.</summary>
    public decimal? DemandaDoIoc => DemandaAjustada ?? DemandaEstrutural;
}

/// <summary>
/// OS SETE COMPONENTES DO IOC, cada um de 0 a 1 — 1 é "muita oportunidade". Nulo é componente sem dado neste
/// município: ele sai da conta, e não entra como zero nem como meio.
/// </summary>
/// <param name="Potencial">A demanda do município sobre o percentil 90 da região.</param>
/// <param name="Cobertura">1 − vínculos cobertos ÷ vínculos com cadência.</param>
/// <param name="Credito">O índice de crédito levado de ±40% para 0 a 1.</param>
/// <param name="Rentabilidade">O momento de preço da cultura principal, na mesma escala.</param>
/// <param name="Clientes">1 − clientes ÷ demanda anual.</param>
/// <param name="Realizacao">1 − vendidas no ano ÷ meta de planejamento.</param>
/// <param name="Penetracao">1 − vendidas no ano ÷ demanda estrutural.</param>
public sealed record ComponentesDoIoc(
    decimal? Potencial,
    decimal? Cobertura,
    decimal? Credito,
    decimal? Rentabilidade,
    decimal? Clientes,
    decimal? Realizacao,
    decimal? Penetracao);

/// <summary>O IOC de um município, com a leitura em frase e o que ficou fora da conta.</summary>
/// <param name="CodigoIbge">O município.</param>
/// <param name="Componentes">Os sete componentes.</param>
/// <param name="Ioc">O índice, de 0 a 100, com uma casa; nulo quando nenhum componente com peso tem dado.</param>
/// <param name="Classe">A classe de prioridade; nula sem índice.</param>
/// <param name="Cobertura">Vínculos cobertos ÷ vínculos com cadência, de 0 a 1; nula sem carteira com cadência.</param>
/// <param name="Penetracao">Vendidas no ano ÷ demanda estrutural; nula sem ART ou sem demanda. Pode passar de 1.</param>
/// <param name="Situacao">O que os componentes dizem, em frases curtas.</param>
/// <param name="PlanoDeAcao">Até três ações, das regras do protótipo.</param>
/// <param name="ComponentesAusentes">Os componentes com peso que ficaram fora da conta, e por quê.</param>
public sealed record IocDoMunicipio(
    int CodigoIbge,
    ComponentesDoIoc Componentes,
    decimal? Ioc,
    ClasseDePrioridade? Classe,
    decimal? Cobertura,
    decimal? Penetracao,
    IReadOnlyList<string> Situacao,
    IReadOnlyList<string> PlanoDeAcao,
    IReadOnlyList<string> ComponentesAusentes);

/// <summary>
/// O ÍNDICE DE OPORTUNIDADE COMERCIAL (IOC) — a aba "Diagnóstico Comercial" do protótipo da pasta 360 (issue 257).
///
/// <para><b>O que ele responde:</b> onde a Tracbel tem mais a ganhar agindo agora — muito potencial, pouca exploração
/// e mercado a favor. É uma ORDEM de prioridade entre municípios, e não uma previsão de venda.</para>
///
/// <para><b>As contas são as do protótipo</b> (<c>renderDiag</c>), componente por componente: o potencial sobre o
/// percentil 90, os índices de ±40% levados a 0–1, as classes de 20 em 20 e as regras de situação e de plano de
/// ação. <b>Três diferenças, todas para não afirmar o que não se mediu:</b></para>
/// <list type="number">
///   <item>componente sem dado sai da conta (os pesos dos outros se renormalizam) e é dito na linha — o protótipo
///   usava 0,5 na cobertura sem cliente e zero de venda sem dado, o que inventa oportunidade;</item>
///   <item>as vendas do período são levadas a um ano antes de comparar com a demanda anual: o padrão do CRM é o ano
///   fiscal até o último mês fechado, que tem menos de 12 meses;</item>
///   <item>a cobertura é a da cadência de cada classe de cliente (a do CRM), e não o corte fixo de 90 dias.</item>
/// </list>
///
/// <para>Os pesos são o parâmetro com vigência da issue 256, normalizados pela soma.</para>
/// </summary>
public static class DiagnosticoComercial
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>A amplitude dos índices de momento que ocupa a escala inteira: de −40% (0) a +40% (1).</summary>
    public const decimal AmplitudeDoMomento = 0.4m;

    /// <summary>Calcula o IOC de cada município.</summary>
    /// <param name="municipios">Os municípios do diagnóstico — a régua do potencial é o percentil 90 deles.</param>
    /// <param name="pesos">Os pesos vigentes.</param>
    public static IReadOnlyList<IocDoMunicipio> Calcular(IReadOnlyList<MunicipioParaOIoc> municipios, PesosDoIoc pesos)
    {
        var p90 = Percentil90([.. municipios.Select(m => m.DemandaDoIoc).OfType<decimal>().Where(d => d > 0)]);
        return [.. municipios.Select(m => Calcular(m, p90, pesos))];
    }

    /// <summary>
    /// O PERCENTIL 90 DA DEMANDA — a régua do componente de potencial, pela conta do protótipo: o item
    /// floor(n × 0,9) da lista ordenada. Com os 203 municípios da ADR a régua não é o maior, e um município enorme
    /// não achata os outros perto de zero; os acima dela valem 1.
    /// </summary>
    /// <param name="demandas">As demandas positivas.</param>
    public static decimal? Percentil90(IReadOnlyList<decimal> demandas)
    {
        if (demandas.Count == 0) return null;
        var ordenadas = demandas.OrderBy(d => d).ToList();
        return ordenadas[Math.Min((int)Math.Floor(ordenadas.Count * 0.9), ordenadas.Count - 1)];
    }

    /// <summary>A classe de prioridade de um IOC, de 20 em 20 pontos.</summary>
    /// <param name="ioc">O índice, de 0 a 100.</param>
    public static ClasseDePrioridade Classificar(decimal ioc) => ioc switch
    {
        >= 80 => ClasseDePrioridade.Maxima,
        >= 60 => ClasseDePrioridade.Alta,
        >= 40 => ClasseDePrioridade.Moderada,
        >= 20 => ClasseDePrioridade.Baixa,
        _ => ClasseDePrioridade.Manutencao
    };

    /// <summary>Um índice de momento (1,00 é estável) levado a 0–1: −40% é 0, estável é 0,5 e +40% é 1.</summary>
    /// <param name="indice">O índice.</param>
    public static decimal? NaEscala(decimal? indice) =>
        indice is { } i ? Limitar((i - 1m + AmplitudeDoMomento) / (2 * AmplitudeDoMomento)) : null;

    private static IocDoMunicipio Calcular(MunicipioParaOIoc m, decimal? p90, PesosDoIoc pesos)
    {
        var demanda = m.DemandaDoIoc;
        decimal? cobertura = m.VinculosComCadencia > 0 ? (decimal)m.Cobertos / m.VinculosComCadencia : null;
        decimal? penetracao = m.VendidasNoAno is { } vendidas && m.DemandaEstrutural is > 0 ? vendidas / m.DemandaEstrutural.Value : null;

        var c = new ComponentesDoIoc(
            Potencial: demanda is { } d && p90 is > 0 ? Limitar(d / p90.Value) : null,
            Cobertura: cobertura is { } cob ? Limitar(1 - cob) : null,
            Credito: NaEscala(m.IndiceDeCredito),
            Rentabilidade: NaEscala(m.IndiceDePrecoDaPrincipal),
            Clientes: demanda is { } dc ? Limitar(1 - m.Clientes / Math.Max(1m, dc)) : null,
            Realizacao: m.VendidasNoAno is { } v && m.MetaDePlanejamento is { } meta ? Limitar(1 - v / Math.Max(0.1m, meta)) : null,
            Penetracao: penetracao is { } pen ? Limitar(1 - pen) : null);

        var partes = new (decimal Peso, decimal? Valor, string Ausencia)[]
        {
            (pesos.Potencial, c.Potencial, "potencial: o município não tem demanda estimada"),
            (pesos.Cobertura, c.Cobertura, "cobertura: nenhum vínculo de carteira com cadência declarada"),
            (pesos.Credito, c.Credito, "crédito: o SICOR não formou o índice do município"),
            (pesos.Rentabilidade, c.Rentabilidade, "rentabilidade: a cultura principal não tem série de preço"),
            (pesos.Clientes, c.Clientes, "clientes: o município não tem demanda estimada"),
            (pesos.Realizacao, c.Realizacao, m.VendidasNoAno is null
                ? "realização: o ART não trouxe as vendas de máquina"
                : "realização: nenhuma categoria do diagnóstico tem share-alvo"),
            (pesos.Penetracao, c.Penetracao, m.VendidasNoAno is null
                ? "penetração: o ART não trouxe as vendas de máquina"
                : "penetração: o município não tem demanda estimada")
        };

        var comDado = partes.Where(p => p.Peso > 0 && p.Valor is not null).ToList();
        var somaDosPesos = comDado.Sum(p => p.Peso);
        decimal? ioc = somaDosPesos > 0
            ? decimal.Round(100m * comDado.Sum(p => p.Peso * p.Valor!.Value) / somaDosPesos, 1)
            : null;

        return new IocDoMunicipio(
            m.CodigoIbge,
            c,
            ioc,
            ioc is { } valor ? Classificar(valor) : null,
            cobertura,
            penetracao,
            Situacao(c, m.CulturaPrincipal, penetracao),
            PlanoDeAcao(c, m, ioc),
            [.. partes.Where(p => p.Peso > 0 && p.Valor is null).Select(p => p.Ausencia).Distinct()]);
    }

    /// <summary>A situação em frases — as regras do protótipo, só com os componentes que têm dado.</summary>
    private static List<string> Situacao(ComponentesDoIoc c, string? cultura, decimal? penetracao)
    {
        var frases = new List<string>();
        var nomeDaCultura = cultura ?? "cultura principal";

        if (c.Potencial >= 0.6m) frases.Add("elevado potencial");
        else if (c.Potencial < 0.3m) frases.Add("potencial modesto");

        if (c.Cobertura >= 0.6m) frases.Add("baixa cobertura comercial");

        if (c.Credito >= 0.65m) frases.Add("crédito de mecanização em alta");
        else if (c.Credito < 0.35m) frases.Add("crédito retraído");

        if (c.Rentabilidade >= 0.65m) frases.Add($"rentabilidade favorável ({nomeDaCultura})");
        else if (c.Rentabilidade < 0.35m) frases.Add($"preço de {nomeDaCultura} em queda");

        if (c.Clientes >= 0.6m) frases.Add("poucos clientes frente ao potencial");

        if (c.Realizacao >= 0.6m) frases.Add("abaixo da meta de share");
        else if (c.Realizacao < 0.2m) frases.Add("meta de share atingida");

        if (c.Penetracao >= 0.7m && penetracao is { } pen)
            frases.Add($"baixa penetração no potencial ({(pen * 100).ToString("0", PtBr)}%)");

        if (frases.Count == 0) frases.Add("situação equilibrada");
        return frases;
    }

    /// <summary>Até três ações, na ordem do protótipo.</summary>
    private static List<string> PlanoDeAcao(ComponentesDoIoc c, MunicipioParaOIoc m, decimal? ioc)
    {
        var acoes = new List<string>();
        var temCliente = m.Clientes > 0;

        if (c.Realizacao >= 0.6m && temCliente) acoes.Add("Campanha de renovação de frota");
        if (c.Cobertura >= 0.6m && temCliente) acoes.Add("Expandir cobertura e visitas presenciais");
        if (c.Clientes >= 0.6m) acoes.Add("Mapear e prospectar novos clientes");
        if (c.Credito >= 0.65m) acoes.Add("Explorar financiamento (Moderfrota, Finame)");
        if (c.Rentabilidade >= 0.65m) acoes.Add($"Atuar junto aos produtores de {(m.CulturaPrincipal ?? "cultura principal").ToLower(PtBr)}");

        if (acoes.Count == 0) acoes.Add(ioc is < 20 ? "Manutenção do relacionamento" : "Monitorar");
        return [.. acoes.Take(3)];
    }

    private static decimal Limitar(decimal valor) => valor < 0 ? 0 : valor > 1 ? 1 : valor;
}
