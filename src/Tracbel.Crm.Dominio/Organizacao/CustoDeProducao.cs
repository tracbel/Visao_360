using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// O CUSTO DE PRODUÇÃO DE UMA CULTURA NUM LOCAL DE REFERÊNCIA, NUMA SAFRA — como a CONAB o calcula
/// (issue 67).
///
/// <para><b>Por que existe.</b> O texto-base do potencial pede a rentabilidade da cultura: "se o cara
/// está rentabilizando X e o custo de produção é tanto". A receita sai do preço (issue 66) e da
/// produtividade; o custo sai daqui. A margem é da issue 73 — esta tabela guarda o custo, não o
/// interpreta.</para>
///
/// <para><b>As cinco camadas da CONAB.</b> Variável (custeio, outras despesas e juros), fixo
/// (depreciações e outros fixos), operacional (variável + fixo), renda de fatores (remuneração do
/// capital e da terra própria) e total (operacional + renda). Cada uma por hectare e por unidade
/// comercial. A planilha do comercial usa o <b>total</b> por hectare; se a rentabilidade deve usar o
/// operacional ou o total é a decisão D-P07, ainda aberta — por isso as duas estão aqui.</para>
///
/// <para><b>Renda de fatores e custo total podem faltar</b>, e isso não é erro: as abas antigas de
/// laranja e duas de cana <b>param no custo operacional</b> (medido em 21/09/2026: 21 das 166 abas de
/// SP).</para>
///
/// <para><b>O local é o da CONAB</b>, não um município da carteira: é onde a CONAB levantou o custo
/// (Franca para o café, Piracicaba e Penápolis para a cana). O município do catálogo vem junto quando
/// o nome casa, para o mapa; quando não casa, o custo continua valendo.</para>
/// </summary>
public sealed class CustoDeProducao
{
    private CustoDeProducao() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A cultura, como o CRM a grava (<c>CAFÉ ARÁBICA</c>, <c>MILHO 2ª SAFRA</c>).</summary>
    public string Cultura { get; private set; } = default!;

    /// <summary>
    /// O nome da aba na planilha da CONAB (<c>SP-Franca 2025</c>) — a chave natural.
    ///
    /// <para>É a aba, e não "local + safra", porque a CONAB publica às vezes <b>dois relatórios no mesmo
    /// ano</b> (<c>Santa Salete-SP-2022-03</c> e <c>…-2022-11</c>), e as duas leituras são verdade.</para>
    /// </summary>
    public string Aba { get; private set; } = default!;

    /// <summary>O local de referência, como a CONAB o escreve.</summary>
    public string Local { get; private set; } = default!;

    /// <summary>O sistema de cultivo, quando a CONAB o separa (<c>Rasteiro</c>, <c>Ereto</c>, no amendoim).</summary>
    public string? Variante { get; private set; }

    /// <summary>O município do catálogo, quando o nome do local casa sem ambiguidade.</summary>
    public int? MunicipioId { get; private set; }

    /// <summary>O ano da safra.</summary>
    public short Safra { get; private set; }

    /// <summary>O mês a cujos preços o custo foi calculado, quando a aba diz.</summary>
    public byte? MesDoRelatorio { get; private set; }

    /// <summary>A produtividade considerada, na unidade da aba.</summary>
    public decimal? Produtividade { get; private set; }

    /// <summary>A unidade da produtividade (<c>kg/ha</c>, <c>cx/planta</c>).</summary>
    public string? UnidadeDaProdutividade { get; private set; }

    /// <summary>A unidade do custo por unidade (<c>saca de 60 kg</c>, <c>t</c>).</summary>
    public string UnidadeComercial { get; private set; } = default!;

    /// <summary>Custo variável por hectare.</summary>
    public decimal CustoVariavelHa { get; private set; }

    /// <summary>Custo variável por unidade comercial.</summary>
    public decimal CustoVariavelUnidade { get; private set; }

    /// <summary>Custo fixo por hectare.</summary>
    public decimal CustoFixoHa { get; private set; }

    /// <summary>Custo fixo por unidade comercial.</summary>
    public decimal CustoFixoUnidade { get; private set; }

    /// <summary>Custo operacional por hectare.</summary>
    public decimal CustoOperacionalHa { get; private set; }

    /// <summary>Custo operacional por unidade comercial.</summary>
    public decimal CustoOperacionalUnidade { get; private set; }

    /// <summary>Renda de fatores por hectare; nulo quando a aba para no operacional.</summary>
    public decimal? RendaDeFatoresHa { get; private set; }

    /// <summary>Renda de fatores por unidade comercial.</summary>
    public decimal? RendaDeFatoresUnidade { get; private set; }

    /// <summary>Custo total por hectare; nulo quando a aba para no operacional.</summary>
    public decimal? CustoTotalHa { get; private set; }

    /// <summary>Custo total por unidade comercial.</summary>
    public decimal? CustoTotalUnidade { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>Os valores de uma leitura, na ordem da metodologia.</summary>
    /// <param name="MesDoRelatorio">O mês do relatório.</param>
    /// <param name="Produtividade">A produtividade.</param>
    /// <param name="UnidadeDaProdutividade">A unidade da produtividade.</param>
    /// <param name="CustoVariavelHa">Variável por hectare.</param>
    /// <param name="CustoVariavelUnidade">Variável por unidade.</param>
    /// <param name="CustoFixoHa">Fixo por hectare.</param>
    /// <param name="CustoFixoUnidade">Fixo por unidade.</param>
    /// <param name="CustoOperacionalHa">Operacional por hectare.</param>
    /// <param name="CustoOperacionalUnidade">Operacional por unidade.</param>
    /// <param name="RendaDeFatoresHa">Renda de fatores por hectare.</param>
    /// <param name="RendaDeFatoresUnidade">Renda de fatores por unidade.</param>
    /// <param name="CustoTotalHa">Total por hectare.</param>
    /// <param name="CustoTotalUnidade">Total por unidade.</param>
    public sealed record Valores(
        byte? MesDoRelatorio,
        decimal? Produtividade,
        string? UnidadeDaProdutividade,
        decimal CustoVariavelHa,
        decimal CustoVariavelUnidade,
        decimal CustoFixoHa,
        decimal CustoFixoUnidade,
        decimal CustoOperacionalHa,
        decimal CustoOperacionalUnidade,
        decimal? RendaDeFatoresHa,
        decimal? RendaDeFatoresUnidade,
        decimal? CustoTotalHa,
        decimal? CustoTotalUnidade);

    /// <summary>Registra o custo de uma aba.</summary>
    /// <param name="cultura">A cultura.</param>
    /// <param name="aba">O nome da aba.</param>
    /// <param name="local">O local de referência.</param>
    /// <param name="variante">O sistema de cultivo, ou nulo.</param>
    /// <param name="municipioId">O município do catálogo, ou nulo.</param>
    /// <param name="safra">O ano da safra.</param>
    /// <param name="unidadeComercial">A unidade do custo por unidade.</param>
    /// <param name="valores">Os valores lidos.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando falta identificação ou os valores não fecham.</exception>
    public static CustoDeProducao Registrar(
        string cultura, string aba, string local, string? variante, int? municipioId, short safra,
        string unidadeComercial, Valores valores, long importadoPorId, DateTime agoraUtc)
    {
        foreach (var (campo, valor) in new[] { ("cultura", cultura), ("aba", aba), ("local", local), ("unidade comercial", unidadeComercial) })
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new RegraDeNegocioViolada($"O custo de produção precisa de {campo}.");
        }

        if (safra is < 1990 or > 2100)
            throw new RegraDeNegocioViolada($"Safra fora do razoável: {safra}.");

        var custo = new CustoDeProducao
        {
            Cultura = cultura.Trim(),
            Aba = aba.Trim(),
            Local = local.Trim(),
            Variante = string.IsNullOrWhiteSpace(variante) ? null : variante.Trim(),
            MunicipioId = municipioId,
            Safra = safra,
            UnidadeComercial = unidadeComercial.Trim(),
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };

        custo.Aplicar(valores);
        return custo;
    }

    /// <summary>
    /// Confere a aba contra uma nova leitura e diz se algum valor mudou. A CONAB revisa série antiga
    /// — o que muda entra na trilha de auditoria.
    /// </summary>
    /// <param name="municipioId">O município da nova leitura.</param>
    /// <param name="valores">Os valores da nova leitura.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando os valores não fecham.</exception>
    public bool Revisar(int? municipioId, Valores valores, long importadoPorId, DateTime agoraUtc)
    {
        var antes = ValoresAtuais();
        var municipioAntes = MunicipioId;

        Aplicar(valores);
        MunicipioId = municipioId;

        var mudou = antes != valores || municipioAntes != municipioId;
        if (mudou)
        {
            ImportadoEm = agoraUtc;
            ImportadoPorId = importadoPorId;
        }

        return mudou;
    }

    /// <summary>Os valores gravados, no mesmo formato de uma leitura.</summary>
    public Valores ValoresAtuais() => new(
        MesDoRelatorio, Produtividade, UnidadeDaProdutividade,
        CustoVariavelHa, CustoVariavelUnidade, CustoFixoHa, CustoFixoUnidade,
        CustoOperacionalHa, CustoOperacionalUnidade, RendaDeFatoresHa, RendaDeFatoresUnidade,
        CustoTotalHa, CustoTotalUnidade);

    private void Aplicar(Valores v)
    {
        // O OPERACIONAL É O MÍNIMO, E É POSITIVO. Custo zero não existe numa lavoura; zero é aba vazia.
        if (v.CustoOperacionalHa <= 0 || v.CustoOperacionalUnidade <= 0)
            throw new RegraDeNegocioViolada("O custo operacional é positivo; zero é aba sem custo.");

        if (v.CustoVariavelHa < 0 || v.CustoFixoHa < 0 || v.RendaDeFatoresHa < 0)
            throw new RegraDeNegocioViolada("Nenhuma camada do custo é negativa.");

        // O TOTAL NÃO É MENOR QUE O OPERACIONAL: total = operacional + renda de fatores. Uma leitura que
        // o desse menor teria pegado a coluna errada.
        if (v.CustoTotalHa is { } total && total < v.CustoOperacionalHa)
            throw new RegraDeNegocioViolada(
                $"O custo total ({total}) é menor que o operacional ({v.CustoOperacionalHa}): a leitura pegou a coluna errada.");

        if (v.MesDoRelatorio is < 1 or > 12)
            throw new RegraDeNegocioViolada($"Mês do relatório fora de 1 a 12: {v.MesDoRelatorio}.");

        MesDoRelatorio = v.MesDoRelatorio;
        Produtividade = v.Produtividade;
        UnidadeDaProdutividade = v.UnidadeDaProdutividade;
        CustoVariavelHa = v.CustoVariavelHa;
        CustoVariavelUnidade = v.CustoVariavelUnidade;
        CustoFixoHa = v.CustoFixoHa;
        CustoFixoUnidade = v.CustoFixoUnidade;
        CustoOperacionalHa = v.CustoOperacionalHa;
        CustoOperacionalUnidade = v.CustoOperacionalUnidade;
        RendaDeFatoresHa = v.RendaDeFatoresHa;
        RendaDeFatoresUnidade = v.RendaDeFatoresUnidade;
        CustoTotalHa = v.CustoTotalHa;
        CustoTotalUnidade = v.CustoTotalUnidade;
    }
}
