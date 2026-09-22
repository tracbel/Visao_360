using System.Globalization;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>Se o dado que o motor vai usar existe para tudo o que devia existir.</summary>
public enum SituacaoDaCobertura
{
    /// <summary>Todos os registros têm o dado.</summary>
    Completa = 0,

    /// <summary>Parte dos registros tem o dado.</summary>
    Parcial = 1,

    /// <summary>Nenhum registro tem o dado — ou não há registro nenhum.</summary>
    Vazia = 2
}

/// <summary>
/// UMA MEDIDA DE COBERTURA DO DADO QUE O MOTOR DE MERCADO VAI USAR (issue 150, documento 49).
///
/// <para><b>É contagem, nunca valor.</b> "Quantos municípios da ADR têm a área plantada de cana divulgada",
/// "quantas vendas de máquina têm o município do comprador" — nenhuma cifra de venda, nenhum nome de cliente
/// ou de CEN. É o que deixa o painel ser lido pela Gerência sem expor dado de cliente.</para>
///
/// <para><b>A situação e a frase são calculadas aqui, no servidor</b>, e não na tela: a mesma regra vale para
/// todo item, e a tela só mostra (documento 49, C-09 — nenhum cálculo no navegador).</para>
///
/// <para><b>Sem limite inventado.</b> Completa é 100%; vazia é zero; o resto é parcial. Qual percentual é
/// "suficiente" para o motor é decisão de negócio, e não cabe a este painel escolher.</para>
/// </summary>
/// <param name="Codigo">Chave estável do item (<c>PAM.40106</c>, <c>INTERNO.EQUIPAMENTO.ANO</c>).</param>
/// <param name="Nome">O que se mede, em português.</param>
/// <param name="Unidade">O que se conta, no plural: "municípios da ADR", "meses", "vendas".</param>
/// <param name="Total">Quantos registros deviam ter o dado.</param>
/// <param name="Cobertos">Quantos têm.</param>
/// <param name="ParaQue">O que o motor calcula com este dado, começando pelo artigo: "o potencial desta cultura".</param>
/// <param name="PeriodoInicial">O primeiro período com dado (ano, safra ou mês <c>aaaa-mm</c>), quando se aplica.</param>
/// <param name="PeriodoFinal">O último período com dado.</param>
/// <param name="Detalhe">O que ajuda a ler o número (quais meses faltam, em quantos municípios se planta…).</param>
public sealed record ItemDeCobertura(
    string Codigo,
    string Nome,
    string Unidade,
    int Total,
    int Cobertos,
    string ParaQue,
    string? PeriodoInicial = null,
    string? PeriodoFinal = null,
    string? Detalhe = null)
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Completa, parcial ou vazia.</summary>
    public SituacaoDaCobertura Situacao =>
        Total <= 0 || Cobertos <= 0 ? SituacaoDaCobertura.Vazia
        : Cobertos >= Total ? SituacaoDaCobertura.Completa
        : SituacaoDaCobertura.Parcial;

    /// <summary>Cobertos sobre o total, em pontos percentuais com uma casa; nulo quando não há total.</summary>
    public decimal? Percentual => Total <= 0 ? null : decimal.Round(100m * Math.Min(Cobertos, Total) / Total, 1);

    /// <summary>
    /// A frase que a tela mostra. Sem concordância a acertar com o que o motor calcula (que pode ser singular ou
    /// plural, masculino ou feminino): o número vem antes da unidade, e o resto é "falta N para …" ou "sem base para …".
    /// </summary>
    public string Motivo => Situacao switch
    {
        SituacaoDaCobertura.Completa => $"{N(Total)} de {N(Total)} {Unidade} com o dado.",
        SituacaoDaCobertura.Parcial =>
            $"{N(Cobertos)} de {N(Total)} {Unidade} com o dado — {(Total - Cobertos == 1 ? "falta" : "faltam")} {N(Total - Cobertos)} para {ParaQue}.",
        _ when Total <= 0 => $"Nenhum registro no banco — sem base para {ParaQue}.",
        _ => $"0 de {N(Total)} {Unidade} com o dado — sem base para {ParaQue}."
    };

    private static string N(int valor) => valor.ToString("N0", PtBr);
}

/// <summary>Um bloco do painel: uma fonte, com os itens medidos dela.</summary>
/// <param name="Codigo">Chave estável do grupo (<c>PAM</c>, <c>PRECOS</c>, <c>VENDAS_DE_MAQUINA</c>…).</param>
/// <param name="Nome">O nome do bloco.</param>
/// <param name="Fonte">De onde o dado vem ("IBGE — PAM, 2024", "ART").</param>
/// <param name="EhInterno">Se é dado da Tracbel (sujeito à fronteira de filial) ou público (o mesmo para todos).</param>
/// <param name="Itens">Os itens: as culturas da PAM pela área plantada na ADR; nos outros blocos, o que falta primeiro.</param>
public sealed record GrupoDeCobertura(string Codigo, string Nome, string Fonte, bool EhInterno, IReadOnlyList<ItemDeCobertura> Itens)
{
    /// <summary>Quantos itens não estão completos — o número que o cabeçalho do bloco destaca.</summary>
    public int Incompletos => Itens.Count(i => i.Situacao != SituacaoDaCobertura.Completa);
}

/// <summary>O painel inteiro, como o banco o mediu.</summary>
/// <param name="MunicipiosDaAdr">O denominador dos itens por município: os municípios vigentes da ADR.</param>
/// <param name="Grupos">Os blocos, na ordem das camadas do motor: produção, preço, custo, crédito e dado interno.</param>
public sealed record CoberturaDoMotor(int MunicipiosDaAdr, IReadOnlyList<GrupoDeCobertura> Grupos);
