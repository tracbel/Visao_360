using System.Globalization;
using System.Text.Json;

namespace Tracbel.Crm.Integracao.Ibge;

/// <summary>
/// A FROTA DE TRATORES DE UM MUNICÍPIO, numa faixa de potência — tabela SIDRA 6871, Censo 2017.
///
/// <para>As duas variáveis vêm na mesma linha, como na PAM: o SIDRA devolve uma linha por variável,
/// e o leitor as junta por (município, ano, faixa). Sem isso a carga gravaria o mesmo município duas
/// vezes.</para>
///
/// <para><b>Os valores chegam crus.</b> O <c>X</c> do sigilo e o <c>...</c> do não disponível são
/// convertidos pelo saneamento da carga, não aqui.</para>
/// </summary>
/// <param name="CodigoDoMunicipio">O código IBGE do município.</param>
/// <param name="Ano">O ano do Censo.</param>
/// <param name="PotenciaCodigo">A categoria da classificação 12605.</param>
/// <param name="PotenciaNome">O rótulo oficial da faixa.</param>
/// <param name="EstabelecimentosComTratorBruto">Variável 1918, como o SIDRA escreveu.</param>
/// <param name="TratoresBruto">Variável 1862.</param>
public sealed record LinhaDaFrotaDeTratores(
    int CodigoDoMunicipio,
    short Ano,
    int PotenciaCodigo,
    string PotenciaNome,
    string? EstabelecimentosComTratorBruto,
    string? TratoresBruto);

/// <summary>
/// UMA MEDIDA DO SIDRA por município, ano e categoria — a forma das fontes que têm UMA variável.
///
/// <para>Serve aos estabelecimentos por faixa de área (tabela 6780) e ao efetivo dos rebanhos
/// (tabela 3939): as duas contam uma coisa só, repartida por uma classificação.</para>
/// </summary>
/// <param name="CodigoDoMunicipio">O código IBGE do município.</param>
/// <param name="Ano">O ano da pesquisa.</param>
/// <param name="CategoriaCodigo">A categoria da classificação pedida.</param>
/// <param name="CategoriaNome">O rótulo oficial da categoria.</param>
/// <param name="ValorBruto">O valor, como o SIDRA escreveu.</param>
public sealed record MedidaPorCategoria(
    int CodigoDoMunicipio,
    short Ano,
    int CategoriaCodigo,
    string CategoriaNome,
    string? ValorBruto);

/// <summary>
/// A ÁREA TERRITORIAL DE UM MUNICÍPIO, em km² — tabela SIDRA 4714, Censo 2022.
/// </summary>
/// <param name="CodigoDoMunicipio">O código IBGE do município.</param>
/// <param name="Ano">O ano da apuração.</param>
/// <param name="AreaKm2Bruta">Variável 6318, como o SIDRA escreveu — com PONTO decimal.</param>
public sealed record LinhaDaAreaTerritorial(int CodigoDoMunicipio, short Ano, string? AreaKm2Bruta);

/// <summary>
/// UMA MEDIDA PUBLICADA PARA O ESTADO INTEIRO — a célula do SIDRA no nível 3 (issue 155).
///
/// <para>É a mesma consulta das quatro pesquisas, trocado o recorte: uma localidade em vez de 645. A
/// identidade dela é a do próprio SIDRA — tabela, variável, ano e categoria —, e é por isso que as
/// quatro cabem no mesmo formato.</para>
/// </summary>
/// <param name="CodigoDaUf">O código IBGE da UF. São Paulo é 35.</param>
/// <param name="Tabela">A tabela do SIDRA.</param>
/// <param name="Variavel">A variável dentro da tabela.</param>
/// <param name="Ano">O ano da pesquisa.</param>
/// <param name="CategoriaCodigo">A categoria, ou nulo na tabela sem classificação.</param>
/// <param name="CategoriaNome">O rótulo oficial da categoria, ou nulo junto com o código.</param>
/// <param name="ValorBruto">O valor, como o SIDRA escreveu.</param>
public sealed record MedidaDoEstadoNoSidra(
    int CodigoDaUf,
    short Tabela,
    short Variavel,
    short Ano,
    int? CategoriaCodigo,
    string? CategoriaNome,
    string? ValorBruto);

/// <summary>
/// A ESTRUTURA AGROPECUÁRIA DE UM MUNICÍPIO, em quatro fontes públicas do IBGE (issue 64 do
/// documento 48; issue 65 no GitHub).
///
/// <list type="table">
///   <item><term>6871</term><description>tratores e estabelecimentos com trator, por faixa de
///   potência — Censo Agropecuário <b>2017</b>, que é o único ano e só sai de novo em 2028;</description></item>
///   <item><term>6780</term><description>estabelecimentos por grupo de área total — Censo 2017;</description></item>
///   <item><term>3939</term><description>efetivo dos rebanhos — Pesquisa da Pecuária Municipal,
///   <b>anual</b>, bem mais recente que o Censo;</description></item>
///   <item><term>4714</term><description>área territorial em km² — Censo <b>2022</b>.</description></item>
/// </list>
///
/// <para><b>Nenhuma delas precisa de lote</b> (ao contrário da PAM, issue 95). Medido em 20/09/2026,
/// para os 645 municípios de São Paulo numa consulta só: a 6871 devolveu 3.799 linhas e 1,2 MB; a
/// 6780, 12.801 linhas e 4,0 MB; a 3939, 644 linhas; a 4714, 646. Todas com HTTP 200, longe do teto
/// de ~50 mil valores que derruba a PAM.</para>
///
/// <para><b>As classificações que não entram na URL vêm no "Total".</b> A 6871 tem quatro
/// classificações (potência, grupos de área total, tipologia e grupos de área de lavoura); pedindo só
/// a potência, as outras três chegam agregadas — conferido na resposta, que traz
/// <c>D5N=Total, D6N=Total, D7N=Total</c>. É o que faz a soma dos municípios bater com o total do
/// estado.</para>
///
/// <para><b>Só leitura, só GET, só dado público.</b> Nenhuma credencial é usada nem existe.</para>
/// </summary>
/// <param name="http">O cliente HTTP, com descompressão automática.</param>
public sealed class LeitorDaEstruturaAgropecuaria(HttpClient http)
{
    /// <summary>Censo Agropecuário: tratores e estabelecimentos com trator, por potência.</summary>
    public const short TabelaDaFrotaDeTratores = 6871;

    /// <summary>Censo Agropecuário: estabelecimentos por grupo de área total.</summary>
    public const short TabelaDeEstabelecimentos = 6780;

    /// <summary>Pesquisa da Pecuária Municipal: efetivo dos rebanhos.</summary>
    public const short TabelaDoRebanho = 3939;

    /// <summary>Censo Demográfico: população, área territorial e densidade.</summary>
    public const short TabelaDaAreaTerritorial = 4714;

    /// <summary>"Número de estabelecimentos agropecuários com tratores" (6871).</summary>
    public const short VariavelDeEstabelecimentosComTrator = 1918;

    /// <summary>"Número de tratores existentes nos estabelecimentos agropecuários" (6871).</summary>
    public const short VariavelDeTratores = 1862;

    /// <summary>"Número de estabelecimentos agropecuários" (6780).</summary>
    public const short VariavelDeEstabelecimentos = 183;

    /// <summary>"Efetivo dos rebanhos", em cabeças (3939).</summary>
    public const short VariavelDoEfetivoDoRebanho = 105;

    /// <summary>"Área da unidade territorial", em km² (4714).</summary>
    public const short VariavelDaAreaTerritorial = 6318;

    /// <summary>As duas variáveis da 6871, na ordem em que a URL as pede.</summary>
    public static readonly short[] VariaveisDaFrota =
        [VariavelDeEstabelecimentosComTrator, VariavelDeTratores];

    /// <summary>"Potência dos tratores": Total, "Menos de 100 cv" e "De 100 cv e mais".</summary>
    public const short ClassificacaoDePotencia = 12605;

    /// <summary>
    /// "Grupos de área total": 18 faixas, mais "Produtor sem área" e o Total — 20 categorias.
    ///
    /// <para><b>A faixa "mais de 2.500 ha" da planilha do comercial é a SOMA de duas</b> categorias do
    /// IBGE: "De 2.500 a menos de 10.000 ha" (41139) e "De 10.000 ha e mais" (40645). Quem reagrupar
    /// precisa somar as duas; as 18 originais ficam guardadas para que o reagrupamento seja
    /// refazível.</para>
    /// </summary>
    public const short ClassificacaoDeGrupoDeArea = 220;

    /// <summary>"Tipo de rebanho": bovino, bubalino, equino, suíno, caprino, ovino e aves.</summary>
    public const short ClassificacaoDeRebanho = 79;

    /// <summary>
    /// A categoria "Bovino" da classificação 79 — a única que a carga traz hoje (issue 65).
    ///
    /// <para>A tabela do CRM guarda o tipo de rebanho, então acrescentar bubalino ou equino é mudar
    /// esta lista, não o modelo. O escopo da issue é o bovino, e trazer nove rebanhos que ninguém
    /// pediu encheria a tela de linha sem leitor.</para>
    /// </summary>
    public static readonly int[] RebanhosQueOCrmGuarda = [2670];

    /// <summary>
    /// AS QUATRO PESQUISAS, do jeito que a URL as pede — a mesma lista que a leitura do estado percorre.
    ///
    /// <para>Ela fica aqui, e não espalhada pelos quatro métodos, porque o total do estado precisa
    /// pedir exatamente o que o município pediu: variável trocada ou classificação a mais no
    /// denominador daria um "% de São Paulo" errado sem erro nenhum.</para>
    /// </summary>
    private static readonly (short Tabela, short[] Variaveis, short? Classificacao, int[]? Categorias)[] Pesquisas =
    [
        (TabelaDaFrotaDeTratores, VariaveisDaFrota, ClassificacaoDePotencia, null),
        (TabelaDeEstabelecimentos, [VariavelDeEstabelecimentos], ClassificacaoDeGrupoDeArea, null),
        (TabelaDoRebanho, [VariavelDoEfetivoDoRebanho], ClassificacaoDeRebanho, RebanhosQueOCrmGuarda),
        (TabelaDaAreaTerritorial, [VariavelDaAreaTerritorial], null, null)
    ];

    /// <summary>O endereço dos metadados de uma tabela do SIDRA, para saber o último ano publicado.</summary>
    /// <param name="tabela">O número da tabela.</param>
    public static string EnderecoDosMetadados(short tabela) =>
        $"https://servicodados.ibge.gov.br/api/v3/agregados/{tabela}/metadados";

    /// <summary>
    /// Monta uma consulta ao SIDRA para todos os municípios de uma UF.
    /// </summary>
    /// <param name="tabela">O número da tabela.</param>
    /// <param name="variaveis">As variáveis pedidas.</param>
    /// <param name="ano">O ano.</param>
    /// <param name="codigoDaUf">O código IBGE da UF. São Paulo é 35.</param>
    /// <param name="classificacao">A classificação pedida, ou nulo quando a tabela não tem.</param>
    /// <param name="categorias">As categorias da classificação; vazio pede <c>all</c>.</param>
    public static string EnderecoNosMunicipios(
        short tabela,
        IEnumerable<short> variaveis,
        short ano,
        int codigoDaUf,
        short? classificacao = null,
        IEnumerable<int>? categorias = null) =>
        Endereco(tabela, $"n6/in%20n3%20{codigoDaUf}", variaveis, ano, classificacao, categorias);

    /// <summary>
    /// A mesma consulta no TOTAL da UF — a linha que o IBGE publica para o estado inteiro.
    ///
    /// <para><b>Ela não é a soma dos municípios</b> (issue 155): onde poucos estabelecimentos
    /// respondem, o valor municipal sai sob sigilo e entra no total do estado sem aparecer embaixo.
    /// Somar os 645 devolve um número menor que o oficial.</para>
    /// </summary>
    /// <param name="tabela">O número da tabela.</param>
    /// <param name="variaveis">As variáveis pedidas.</param>
    /// <param name="ano">O ano.</param>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="classificacao">A classificação pedida, ou nulo quando a tabela não tem.</param>
    /// <param name="categorias">As categorias da classificação; vazio pede <c>all</c>.</param>
    public static string EnderecoNoEstado(
        short tabela,
        IEnumerable<short> variaveis,
        short ano,
        int codigoDaUf,
        short? classificacao = null,
        IEnumerable<int>? categorias = null) =>
        Endereco(tabela, $"n3/{codigoDaUf}", variaveis, ano, classificacao, categorias);

    private static string Endereco(
        short tabela, string nivel, IEnumerable<short> variaveis, short ano,
        short? classificacao, IEnumerable<int>? categorias)
    {
        var recorte = classificacao is null
            ? string.Empty
            : $"/c{classificacao}/" + (categorias is null || !categorias.Any()
                ? "all"
                : string.Join(',', categorias));

        return $"https://apisidra.ibge.gov.br/values/t/{tabela}/{nivel}" +
               $"/v/{string.Join(',', variaveis)}/p/{ano}{recorte}?formato=json";
    }

    /// <summary>
    /// O último ano publicado de uma tabela, pelos metadados (<c>periodicidade.fim</c>).
    ///
    /// <para>É o que impede a carga de envelhecer em silêncio: a PPM ganha um ano todo ano, e o Censo
    /// ganhará 2028 — nenhum dos dois precisa de alteração no código para entrar.</para>
    /// </summary>
    /// <param name="tabela">O número da tabela.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<short> LerUltimoAnoAsync(short tabela, CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(EnderecoDosMetadados(tabela), ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        return documento.RootElement.GetProperty("periodicidade").GetProperty("fim").GetInt16();
    }

    /// <summary>
    /// A frota de tratores dos municípios de uma UF, por faixa de potência, no Censo mais recente.
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<LinhaDaFrotaDeTratores>> LerFrotaDeTratoresAsync(
        int codigoDaUf, CancellationToken ct)
    {
        var ano = await LerUltimoAnoAsync(TabelaDaFrotaDeTratores, ct);
        var endereco = EnderecoNosMunicipios(
            TabelaDaFrotaDeTratores, VariaveisDaFrota, ano, codigoDaUf, ClassificacaoDePotencia);

        var porChave = new Dictionary<(int Municipio, short Ano, int Potencia), string?[]>();
        var nomes = new Dictionary<int, string>();
        var ordem = new List<(int Municipio, short Ano, int Potencia)>();

        await foreach (var celula in LerCelulasAsync(endereco, ct))
        {
            var posicao = Array.IndexOf(VariaveisDaFrota, celula.Variavel);
            if (posicao < 0) continue;

            var chave = (celula.Localidade, celula.Ano, celula.Categoria);
            if (!porChave.TryGetValue(chave, out var valores))
            {
                valores = new string?[VariaveisDaFrota.Length];
                porChave[chave] = valores;
                ordem.Add(chave);
            }

            valores[posicao] = celula.Valor;
            nomes[celula.Categoria] = celula.CategoriaNome;
        }

        return
        [
            .. ordem.Select(c => new LinhaDaFrotaDeTratores(
                c.Municipio, c.Ano, c.Potencia, nomes[c.Potencia], porChave[c][0], porChave[c][1]))
        ];
    }

    /// <summary>
    /// Os estabelecimentos agropecuários dos municípios de uma UF, por grupo de área total.
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<IReadOnlyList<MedidaPorCategoria>> LerEstabelecimentosPorAreaAsync(
        int codigoDaUf, CancellationToken ct) =>
        LerPorCategoriaAsync(
            TabelaDeEstabelecimentos, VariavelDeEstabelecimentos, ClassificacaoDeGrupoDeArea,
            categorias: null, codigoDaUf, ct);

    /// <summary>
    /// O efetivo dos rebanhos que o CRM guarda, nos municípios de uma UF, no ano mais recente.
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public Task<IReadOnlyList<MedidaPorCategoria>> LerRebanhoAsync(int codigoDaUf, CancellationToken ct) =>
        LerPorCategoriaAsync(
            TabelaDoRebanho, VariavelDoEfetivoDoRebanho, ClassificacaoDeRebanho,
            RebanhosQueOCrmGuarda, codigoDaUf, ct);

    /// <summary>
    /// A área territorial dos municípios de uma UF, em km².
    ///
    /// <para><b>O separador decimal é PONTO, e não há separador de milhar</b> — conferido em
    /// 20/09/2026: São Paulo vem como <c>1521.202</c>, que são 1.521,202 km². Ler isso com a cultura
    /// do Brasil daria 1.521.202 km², mil vezes a área do estado inteiro.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<LinhaDaAreaTerritorial>> LerAreaTerritorialAsync(
        int codigoDaUf, CancellationToken ct)
    {
        var ano = await LerUltimoAnoAsync(TabelaDaAreaTerritorial, ct);
        var endereco = EnderecoNosMunicipios(
            TabelaDaAreaTerritorial, [VariavelDaAreaTerritorial], ano, codigoDaUf);

        var linhas = new List<LinhaDaAreaTerritorial>();
        await foreach (var celula in LerCelulasAsync(endereco, ct))
            linhas.Add(new LinhaDaAreaTerritorial(celula.Localidade, celula.Ano, celula.Valor));

        return linhas;
    }

    /// <summary>
    /// AS QUATRO PESQUISAS NO TOTAL DO ESTADO, cada uma no ano mais recente dela (issue 155).
    ///
    /// <para><b>Quatro consultas de uma linha (ou poucas), não 645.</b> O estado é uma localidade só;
    /// a resposta inteira das quatro cabe em alguns quilobytes.</para>
    ///
    /// <para>O ano sai dos metadados de cada tabela, como nas municipais — o Censo continua em 2017 e
    /// a PPM anda sozinha.</para>
    /// </summary>
    /// <param name="codigoDaUf">O código IBGE da UF.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<MedidaDoEstadoNoSidra>> LerMedidasNoEstadoAsync(
        int codigoDaUf, CancellationToken ct)
    {
        var medidas = new List<MedidaDoEstadoNoSidra>();

        foreach (var (tabela, variaveis, classificacao, categorias) in Pesquisas)
        {
            var ano = await LerUltimoAnoAsync(tabela, ct);
            var endereco = EnderecoNoEstado(tabela, variaveis, ano, codigoDaUf, classificacao, categorias);

            await foreach (var celula in LerCelulasAsync(endereco, ct))
                medidas.Add(new MedidaDoEstadoNoSidra(
                    celula.Localidade,
                    tabela,
                    celula.Variavel,
                    celula.Ano,
                    celula.Categoria == 0 ? null : celula.Categoria,
                    string.IsNullOrWhiteSpace(celula.CategoriaNome) ? null : celula.CategoriaNome,
                    celula.Valor));
        }

        return medidas;
    }

    // =============================================================================================

    /// <summary>
    /// Uma célula da resposta do SIDRA, já com os campos que interessam separados.
    ///
    /// <para><c>Localidade</c> é o município no nível 6 e a UF no nível 3 — o campo é o mesmo
    /// (<c>D1C</c>), e quem chama sabe qual dos dois pediu.</para>
    /// </summary>
    private readonly record struct CelulaDoSidra(
        int Localidade, short Ano, short Variavel, int Categoria, string CategoriaNome, string? Valor);

    /// <summary>O laço das fontes de UMA variável repartida por UMA classificação.</summary>
    private async Task<IReadOnlyList<MedidaPorCategoria>> LerPorCategoriaAsync(
        short tabela, short variavel, short classificacao, IEnumerable<int>? categorias,
        int codigoDaUf, CancellationToken ct)
    {
        var ano = await LerUltimoAnoAsync(tabela, ct);
        var endereco = EnderecoNosMunicipios(tabela, [variavel], ano, codigoDaUf, classificacao, categorias);

        var linhas = new List<MedidaPorCategoria>();
        await foreach (var celula in LerCelulasAsync(endereco, ct))
            linhas.Add(new MedidaPorCategoria(
                celula.Localidade, celula.Ano, celula.Categoria, celula.CategoriaNome, celula.Valor));

        return linhas;
    }

    /// <summary>
    /// Lê a resposta do SIDRA célula a célula.
    ///
    /// <para><b>A primeira linha é o CABEÇALHO</b> — ela repete o nome de cada campo no lugar do
    /// valor, e por isso é descartada. <c>D1C</c> é o recorte, <c>D2C</c> a variável, <c>D3C</c> o ano
    /// e <c>D4C</c> a primeira classificação pedida; as classificações que não entraram na URL chegam
    /// no "Total", em <c>D5</c> e adiante.</para>
    ///
    /// <para>Tabela sem classificação nenhuma (a 4714) não tem <c>D4C</c>: a categoria sai como zero,
    /// e quem chama não a usa.</para>
    /// </summary>
    private async IAsyncEnumerable<CelulaDoSidra> LerCelulasAsync(
        string endereco, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        await using var corpo = await http.GetStreamAsync(endereco, ct);
        using var documento = await JsonDocument.ParseAsync(corpo, cancellationToken: ct);

        foreach (var l in documento.RootElement.EnumerateArray().Skip(1))
        {
            var categoria = l.TryGetProperty("D4C", out var d4c)
                ? int.Parse(d4c.GetString()!, CultureInfo.InvariantCulture)
                : 0;

            var categoriaNome = l.TryGetProperty("D4N", out var d4n) ? d4n.GetString()! : string.Empty;

            yield return new CelulaDoSidra(
                int.Parse(l.GetProperty("D1C").GetString()!, CultureInfo.InvariantCulture),
                short.Parse(l.GetProperty("D3C").GetString()!, CultureInfo.InvariantCulture),
                short.Parse(l.GetProperty("D2C").GetString()!, CultureInfo.InvariantCulture),
                categoria,
                categoriaNome,
                l.GetProperty("V").GetString());
        }
    }
}
