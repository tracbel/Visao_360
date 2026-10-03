using System.Collections.Concurrent;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>As medidas da PAM de UM produto num município — o que o mapa C mostra no balão.</summary>
/// <param name="AreaPlantadaHectares">A área plantada.</param>
/// <param name="AreaColhidaHectares">A área colhida.</param>
/// <param name="QuantidadeProduzida">A quantidade, na unidade do IBGE.</param>
/// <param name="ValorDaProducaoMilReais">O valor, em mil reais.</param>
public readonly record struct MedidasDaCulturaNoMunicipio(
    decimal? AreaPlantadaHectares, decimal? AreaColhidaHectares, decimal? QuantidadeProduzida, decimal? ValorDaProducaoMilReais);

/// <summary>A categoria de uma regra, pelo catálogo.</summary>
/// <param name="Codigo">O código.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Ordem">A ordem de exibição.</param>
public sealed record CategoriaDaRegra(string Codigo, string Nome, short Ordem);

/// <summary>Uma categoria de máquina do motor.</summary>
/// <param name="Codigo">O código.</param>
/// <param name="Nome">O nome.</param>
public sealed record CategoriaNoMotor(string Codigo, string Nome);

/// <summary>O resultado do motor numa categoria.</summary>
/// <param name="Codigo">A categoria.</param>
/// <param name="Resultado">O parque, a demanda e as parcelas.</param>
public sealed record ResultadoDaCategoria(string Codigo, PotencialDoRecorte Resultado);

/// <summary>O potencial de um município no motor.</summary>
/// <param name="PorCategoria">Uma linha por categoria.</param>
/// <param name="Sobreposto">As categorias somadas sem somar a terra delas.</param>
/// <param name="Demanda">A demanda por categoria e cultura — só as parcelas com parque.</param>
public sealed record PotencialDoMunicipioNoMotor(
    IReadOnlyList<ResultadoDaCategoria> PorCategoria, PotencialDoRecorte Sobreposto, IReadOnlyList<DemandaNoMunicipio> Demanda);

/// <summary>
/// O POTENCIAL DE REFERÊNCIA (documento 54 §3.1) — tudo o que o motor precisa, lido uma vez por versão do assunto, e a
/// conta de cada município feita uma vez e guardada.
///
/// <para><b>A mesma conta da apuração.</b> O motor é <see cref="MotorDoPotencial"/>; a área de uma cultura é a soma dos
/// produtos dela, cada um no seu ano; produto sem área divulgada devolve nulo, e não zero.</para>
///
/// <para><b>Compartilhado entre requisições:</b> as entradas não mudam depois de montadas, e a conta por município é
/// guardada num dicionário concorrente.</para>
/// </summary>
public sealed class PotencialDeReferencia
{
    private readonly ConcurrentDictionary<(int Codigo, bool DoAnoAnterior), PotencialDoMunicipioNoMotor> _calculados = new();
    private readonly IReadOnlyList<IGrouping<(string Codigo, string Nome), RegraNoMotor>> _porCategoria;
    private readonly IReadOnlyDictionary<string, string> _nomeDaCategoria;

    /// <summary>Monta o potencial a partir do que o leitor leu.</summary>
    /// <param name="anoDaAreaPlantada">O ano mais recente da PAM.</param>
    /// <param name="regras">As regras vigentes na data, por produto e categoria.</param>
    /// <param name="categoriaDaRegra">A categoria de cada regra, pelo identificador.</param>
    /// <param name="catalogo">O catálogo do motor.</param>
    /// <param name="anoDaCultura">O ano de cada produto.</param>
    /// <param name="medidas">A PAM de cada produto em cada município, no ano dele.</param>
    /// <param name="medidasDoAnoAnterior">A área do ano anterior de cada produto.</param>
    /// <param name="producao">A lavoura inteira de cada município.</param>
    /// <param name="culturasNoEstado">As culturas das regras no total de São Paulo.</param>
    public PotencialDeReferencia(
        short? anoDaAreaPlantada,
        IReadOnlyList<RegraDePotencial> regras,
        IReadOnlyDictionary<int, CategoriaDaRegra> categoriaDaRegra,
        CatalogoDoMotor catalogo,
        IReadOnlyDictionary<int, short> anoDaCultura,
        IReadOnlyDictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio> medidas,
        IReadOnlyDictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio> medidasDoAnoAnterior,
        IReadOnlyDictionary<int, ProducaoAgricolaDoMunicipio> producao,
        IReadOnlyList<CulturaNoEstado> culturasNoEstado)
    {
        AnoDaAreaPlantada = anoDaAreaPlantada;
        Regras = regras;
        CategoriasDasRegras = categoriaDaRegra;
        Catalogo = catalogo;
        AnoDaCultura = anoDaCultura;
        Medidas = medidas;
        MedidasDoAnoAnterior = medidasDoAnoAnterior;
        Producao = producao;
        CulturasNoEstado = culturasNoEstado;

        _porCategoria = [.. catalogo.Regras
            .GroupBy(r => (Codigo: r.CategoriaCodigo, Nome: r.CategoriaNome))
            .OrderBy(g => g.Key.Codigo, StringComparer.Ordinal)];
        Categorias = [.. _porCategoria
            .Select(g => g.Key)
            .DistinctBy(k => k.Codigo, StringComparer.Ordinal)
            .Select(k => new CategoriaNoMotor(k.Codigo, k.Nome))];
        _nomeDaCategoria = Categorias.ToDictionary(c => c.Codigo, c => c.Nome, StringComparer.Ordinal);

        // A CATEGORIA VAI JUNTO (issue 240): com regra de trator e de colheitadeira no mesmo produto, é ela que diz qual é qual.
        RegrasAplicadas = [.. regras.Select(r =>
        {
            var categoria = r.CategoriaDeMaquinaId is { } id && categoriaDaRegra.TryGetValue(id, out var doCatalogo) ? doCatalogo : null;
            return new RegraDePotencialAplicada(
                r.ProdutoCodigoIbge, r.ProdutoNome, r.HectaresPorMaquina, r.ModeloDeReferencia, r.Situacao.ToString(),
                r.Justificativa, r.VigenteDesde, r.AnosDeRenovacao, categoria?.Codigo, categoria?.Nome);
        })];
    }

    /// <summary>O ano mais recente da PAM.</summary>
    public short? AnoDaAreaPlantada { get; }

    /// <summary>As regras vigentes na data da leitura, por produto e categoria.</summary>
    public IReadOnlyList<RegraDePotencial> Regras { get; }

    /// <summary>As mesmas regras, como a tela as cita.</summary>
    public IReadOnlyList<RegraDePotencialAplicada> RegrasAplicadas { get; }

    /// <summary>A categoria de cada regra, pelo identificador.</summary>
    public IReadOnlyDictionary<int, CategoriaDaRegra> CategoriasDasRegras { get; }

    /// <summary>O catálogo do motor.</summary>
    public CatalogoDoMotor Catalogo { get; }

    /// <summary>O ano de cada produto — cada cultura no seu (issue 152).</summary>
    public IReadOnlyDictionary<int, short> AnoDaCultura { get; }

    /// <summary>A PAM de cada produto em cada município, no ano dele.</summary>
    public IReadOnlyDictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio> Medidas { get; }

    /// <summary>A área do ano anterior de cada produto — só a área.</summary>
    public IReadOnlyDictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio> MedidasDoAnoAnterior { get; }

    /// <summary>A lavoura inteira de cada município, no ano mais recente.</summary>
    public IReadOnlyDictionary<int, ProducaoAgricolaDoMunicipio> Producao { get; }

    /// <summary>As culturas das regras no total de São Paulo.</summary>
    public IReadOnlyList<CulturaNoEstado> CulturasNoEstado { get; }

    /// <summary>As categorias do motor, na ordem do código.</summary>
    public IReadOnlyList<CategoriaNoMotor> Categorias { get; }

    /// <summary>A conta do motor num município — feita uma vez e guardada.</summary>
    /// <param name="codigo">O código do IBGE.</param>
    /// <param name="doAnoAnterior">Com a área do ano anterior da PAM.</param>
    public PotencialDoMunicipioNoMotor DoMunicipio(int codigo, bool doAnoAnterior = false) =>
        _calculados.GetOrAdd((codigo, doAnoAnterior), chave => Calcular(chave.Codigo, chave.DoAnoAnterior));

    private PotencialDoMunicipioNoMotor Calcular(int codigo, bool doAnoAnterior)
    {
        var medidas = doAnoAnterior ? MedidasDoAnoAnterior : Medidas;
        List<ResultadoDaCategoria> porCategoria =
        [
            .. _porCategoria.Select(categoria => new ResultadoDaCategoria(
                categoria.Key.Codigo,
                MotorDoPotencial.Potencial(
                [
                    .. categoria.Select(r => new CulturaNoRecorte(
                        r.CulturaCodigo, r.CulturaNome, AreaDaCultura(codigo, r.ProdutosDaPam, medidas),
                        r.HectaresPorMaquina, r.AnosDeRenovacao, r.Confirmada, r.GrupoCodigo))
                ])))
        ];

        // A DEMANDA POR CATEGORIA E CULTURA (27/09/2026): o ingrediente dos números de decisão da ficha do município. Só as
        // parcelas com parque — as outras não têm demanda para ajustar.
        List<DemandaNoMunicipio> demanda =
        [
            .. porCategoria.SelectMany(c => c.Resultado.Parcelas
                .Where(p => p.Parque is not null)
                .Select(p => new DemandaNoMunicipio(
                    c.Codigo, _nomeDaCategoria[c.Codigo], p.CulturaCodigo, p.Cultura, p.DemandaAnual, p.AreaUtilHectares, p.Parque)))
        ];

        return new PotencialDoMunicipioNoMotor(porCategoria, MotorDoPotencial.Sobrepor([.. porCategoria.Select(c => c.Resultado)]), demanda);
    }

    // A ÁREA DE UMA CULTURA É A SOMA DOS PRODUTOS DELA, cada um no ano dele. Nenhum produto com área divulgada devolve
    // NULO, e não zero: sigilo do IBGE não é ausência de lavoura.
    private static decimal? AreaDaCultura(
        int municipio, IReadOnlyList<int> produtos, IReadOnlyDictionary<(int Codigo, int Produto), MedidasDaCulturaNoMunicipio> medidas)
    {
        decimal? soma = null;
        foreach (var produto in produtos)
            if (medidas.TryGetValue((municipio, produto), out var medida) && medida.AreaPlantadaHectares is { } plantada)
                soma = (soma ?? 0m) + plantada;
        return soma;
    }
}
