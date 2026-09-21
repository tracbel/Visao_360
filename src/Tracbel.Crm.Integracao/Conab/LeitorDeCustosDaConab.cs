using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;

namespace Tracbel.Crm.Integracao.Conab;

/// <summary>Uma série histórica de custo da CONAB — um arquivo por cultura.</summary>
/// <param name="Cultura">A cultura, como o CRM a grava (<c>CAFÉ ARÁBICA</c>).</param>
/// <param name="UnidadeComercial">A unidade em que a planilha dá o custo por unidade.</param>
/// <param name="Endereco">O endereço do arquivo <c>.xls</c>.</param>
public sealed record PlanilhaDeCustos(string Cultura, string UnidadeComercial, Uri Endereco);

/// <summary>
/// O CUSTO DE PRODUÇÃO DE UMA ABA — um local de referência, numa safra, como a CONAB o calcula.
///
/// <para>As camadas seguem a metodologia da CONAB: <b>variável</b> (custeio, outras despesas e juros),
/// <b>fixo</b> (depreciações e outros fixos), <b>operacional</b> (variável + fixo), <b>renda de
/// fatores</b> (remuneração do capital e da terra própria) e <b>total</b> (operacional + renda). Cada
/// uma vem por hectare e por unidade comercial.</para>
/// </summary>
public sealed record LinhaDeCustoDaConab(
    string Cultura,
    string Aba,
    string Local,
    string? Variante,
    short Safra,
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

/// <summary>
/// A LEITURA DAS SÉRIES HISTÓRICAS DE CUSTO DE PRODUÇÃO DA CONAB — as abas de São Paulo (issue 67).
///
/// <para><b>Por que as planilhas, e não o arquivo aberto.</b> A CONAB publica um
/// <c>CustoProducao.txt</c> aberto, mas ele é nacional e ralo para São Paulo: em 21/09/2026 trazia café
/// de Franca só até 10/2024 e <b>nenhuma cana paulista</b>. As séries históricas em <c>.xls</c> da
/// página "Planilhas de Custos de Produção" têm o que a planilha do comercial usa — cana em Piracicaba
/// e Penápolis, café em Franca até 2025, laranja, amendoim, soja e milho em locais de SP. Download
/// direto, sem cadastro nem barreira.</para>
///
/// <para><b>O nome do arquivo muda todo ano</b> (<c>…-2008-a-2025.xls</c>). O leitor não guarda
/// endereço de arquivo: acha o link na página pelo NOME DA CULTURA, como uma pessoa faria.</para>
///
/// <para><b>O layout muda com os anos</b> — caixa dos rótulos, produtividade numa célula de texto ou em
/// célula separada, cabeçalho partido em duas linhas nas abas antigas, relatório que para no custo
/// operacional. Por isso a aba é lida por RÓTULO (sem acento e sem caixa), e nunca por posição: o
/// valor é o primeiro par de números à direita do rótulo — por hectare e por unidade.</para>
///
/// <para><b>Só leitura, só GET, só dado público.</b> Nenhuma credencial é usada nem existe.</para>
/// </summary>
/// <param name="http">O cliente HTTP.</param>
public sealed partial class LeitorDeCustosDaConab(HttpClient http)
{
    /// <summary>A página com a lista das séries históricas agrícolas.</summary>
    public const string EnderecoDaPaginaAgricola =
        "https://www.gov.br/conab/pt-br/atuacao/informacoes-agropecuarias/custos-de-producao/planilhas-de-custos-de-producao/copy_of_agricolas";

    /// <summary>
    /// As culturas do potencial, pelo texto do link na página — e a unidade da série.
    ///
    /// <para>A unidade é a da série da CONAB, estável entre os anos (medido em 21/09/2026 nas 166 abas
    /// de SP): o cabeçalho que a escreve está partido nas abas antigas e escrito de quatro jeitos nas
    /// novas ("CUSTO / t", "SACO (JUTA) - 60 KG", "R$/CX 40,8 KG"…).</para>
    /// </summary>
    private static readonly (string TextoDoLink, string Cultura, string Unidade)[] Culturas =
    [
        ("CAFE ARABICA", "CAFÉ ARÁBICA", "saca de 60 kg"),
        ("CANA-DE-ACUCAR", "CANA DE AÇÚCAR", "t"),
        ("SOJA", "SOJA", "saca de 60 kg"),
        ("MILHO", "MILHO", "saca de 60 kg"),
        ("AMENDOIM", "AMENDOIM", "saca de 25 kg"),
        ("LARANJA", "LARANJA", "caixa de 40,8 kg")
    ];

    private static readonly string[] Variantes = ["RASTEIRO", "ERETO"];

    private static readonly string[] Meses =
        ["JANEIRO", "FEVEREIRO", "MARCO", "ABRIL", "MAIO", "JUNHO", "JULHO", "AGOSTO", "SETEMBRO", "OUTUBRO", "NOVEMBRO", "DEZEMBRO"];

    static LeitorDeCustosDaConab()
    {
        // O .XLS ANTIGO GUARDA TEXTO EM PÁGINA DE CÓDIGO DO WINDOWS (1252). O .NET não traz essas páginas
        // por padrão; sem o provedor, a leitura quebra na primeira aba com acento.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>Acha, na página da CONAB, o arquivo de cada cultura do potencial.</summary>
    /// <param name="ct">Cancelamento.</param>
    /// <exception cref="InvalidDataException">Quando a página não traz nenhuma cultura reconhecível.</exception>
    public async Task<IReadOnlyList<PlanilhaDeCustos>> LocalizarPlanilhasAsync(CancellationToken ct)
    {
        var pagina = await http.GetStringAsync(EnderecoDaPaginaAgricola, ct);
        var planilhas = new List<PlanilhaDeCustos>();

        foreach (var (texto, endereco) in Links(pagina, new Uri(EnderecoDaPaginaAgricola)))
        {
            var cultura = Culturas.FirstOrDefault(c => Normalizar(texto) == c.TextoDoLink);
            if (cultura.Cultura is null) continue;

            if (EhPlanilha(endereco))
            {
                planilhas.Add(new PlanilhaDeCustos(cultura.Cultura, cultura.Unidade, SemVisualizacao(endereco)));
                continue;
            }

            // O MILHO É UMA PASTA, com um arquivo por safra (1ª e 2ª). A cultura ganha a safra no nome,
            // porque os custos e a produtividade das duas não se comparam.
            var pasta = await http.GetStringAsync(endereco, ct);
            foreach (var (textoDoArquivo, arquivo) in Links(pasta, endereco).Where(l => EhPlanilha(l.Endereco)))
            {
                var safra = SafraDoMilho().Match(textoDoArquivo);
                var nome = safra.Success ? $"{cultura.Cultura} {safra.Groups[1].Value}ª SAFRA" : cultura.Cultura;
                planilhas.Add(new PlanilhaDeCustos(nome, cultura.Unidade, SemVisualizacao(arquivo)));
            }
        }

        if (planilhas.Count == 0)
            throw new InvalidDataException(
                "A página de custos da CONAB não trouxe nenhuma cultura reconhecível. A página mudou de forma; nada foi gravado.");

        return [.. planilhas.DistinctBy(p => p.Endereco)];
    }

    /// <summary>Baixa uma série e lê as abas de São Paulo.</summary>
    /// <param name="planilha">A série.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<IReadOnlyList<LinhaDeCustoDaConab>> LerAsync(PlanilhaDeCustos planilha, CancellationToken ct)
    {
        var bytes = await http.GetByteArrayAsync(planilha.Endereco, ct);
        return LerPlanilha(new MemoryStream(bytes), planilha.Cultura);
    }

    /// <summary>Lê as abas de São Paulo de um arquivo <c>.xls</c>.</summary>
    /// <param name="arquivo">O conteúdo do arquivo.</param>
    /// <param name="cultura">A cultura da série.</param>
    public static IReadOnlyList<LinhaDeCustoDaConab> LerPlanilha(Stream arquivo, string cultura)
    {
        var linhas = new List<LinhaDeCustoDaConab>();
        using var leitor = ExcelReaderFactory.CreateReader(arquivo);

        do
        {
            if (!EhAbaDeSaoPaulo(leitor.Name)) continue;

            var celulas = new List<object?[]>();
            while (leitor.Read())
            {
                var linha = new object?[leitor.FieldCount];
                for (var i = 0; i < leitor.FieldCount; i++) linha[i] = leitor.GetValue(i);
                celulas.Add(linha);
            }

            if (InterpretarAba(cultura, leitor.Name, celulas) is { } custo) linhas.Add(custo);
        }
        while (leitor.NextResult());

        return linhas;
    }

    /// <summary>A aba é de um local de São Paulo? (<c>Franca-SP-2010</c>, <c>SP-Franca 2025</c>)</summary>
    /// <param name="nome">O nome da aba.</param>
    public static bool EhAbaDeSaoPaulo(string nome) => AbaDeSaoPaulo().IsMatch(nome);

    /// <summary>
    /// Interpreta uma aba já lida em linhas. Público para os testes, que usam os layouts medidos.
    /// </summary>
    /// <param name="cultura">A cultura da série.</param>
    /// <param name="aba">O nome da aba.</param>
    /// <param name="linhas">As células, linha a linha.</param>
    /// <returns>O custo, ou nulo quando a aba não traz nem o custo operacional (não é uma aba de custo).</returns>
    public static LinhaDeCustoDaConab? InterpretarAba(string cultura, string aba, IReadOnlyList<object?[]> linhas)
    {
        (decimal Ha, decimal Unidade)? variavel = null, fixo = null, operacional = null, renda = null, total = null;
        decimal? produtividade = null;
        string? unidadeDaProdutividade = null;
        byte? mesNoTexto = null;

        foreach (var linha in linhas)
        {
            for (var i = 0; i < linha.Length; i++)
            {
                if (linha[i] is not string bruto) continue;
                var rotulo = Normalizar(bruto);
                if (rotulo.Length == 0) continue;

                // CADA RÓTULO VALE NA PRIMEIRA VEZ. "CUSTO FIXO" não casa com "OUTROS CUSTOS FIXOS" (o
                // começo é outro), e "CUSTO TOTAL" aparece uma vez só — mas guardar a primeira protege de
                // uma nota de rodapé que repita o rótulo.
                if (variavel is null && rotulo.StartsWith("CUSTO VARIAVEL", StringComparison.Ordinal)) variavel = Par(linha, i);
                else if (fixo is null && rotulo.StartsWith("CUSTO FIXO", StringComparison.Ordinal)) fixo = Par(linha, i);
                else if (operacional is null && rotulo.StartsWith("CUSTO OPERACIONAL", StringComparison.Ordinal)) operacional = Par(linha, i);
                else if (renda is null && rotulo.StartsWith("TOTAL", StringComparison.Ordinal) && rotulo.Contains("RENDA DE FATORES", StringComparison.Ordinal)) renda = Par(linha, i);
                else if (total is null && rotulo.StartsWith("CUSTO TOTAL", StringComparison.Ordinal)) total = Par(linha, i);
                else if (produtividade is null && rotulo.StartsWith("PRODUTIVIDADE", StringComparison.Ordinal))
                    (produtividade, unidadeDaProdutividade) = Produtividade(linha, i, rotulo);

                if (mesNoTexto is null && (rotulo.Contains("MES/ANO", StringComparison.Ordinal) || rotulo.Contains("A PRECOS DE", StringComparison.Ordinal)))
                    mesNoTexto = MesNoTexto(linha, i);
            }
        }

        // SEM CUSTO OPERACIONAL NÃO HÁ CUSTO. Variável e fixo sem o operacional seriam uma aba pela
        // metade, e o operacional é o mínimo que toda aba da série traz — as antigas param nele.
        if (variavel is null || fixo is null || operacional is null) return null;

        var (local, variante, safra, mesNoNome) = DesmontarNomeDaAba(aba);

        return new LinhaDeCustoDaConab(
            cultura, aba.Trim(), local, variante, safra, mesNoNome ?? mesNoTexto,
            produtividade, unidadeDaProdutividade,
            variavel.Value.Ha, variavel.Value.Unidade,
            fixo.Value.Ha, fixo.Value.Unidade,
            operacional.Value.Ha, operacional.Value.Unidade,
            renda?.Ha, renda?.Unidade,
            total?.Ha, total?.Unidade);
    }

    /// <summary>
    /// Local, variante, safra e mês do nome da aba.
    ///
    /// <list type="bullet">
    ///   <item><c>Franca-SP-2010</c> → Franca, 2010;</item>
    ///   <item><c>SP-Franca 2025</c> → Franca, 2025 (o formato novo);</item>
    ///   <item><c>Rasteiro-Jaboticabal-SP-2014</c> → Jaboticabal, variante Rasteiro, 2014;</item>
    ///   <item><c>Santa Salete-SP-2022-11</c> → Santa Salete, 2022, mês 11 (dois relatórios no ano).</item>
    /// </list>
    /// </summary>
    /// <param name="aba">O nome da aba.</param>
    /// <exception cref="FormatException">Quando o nome não traz ano.</exception>
    public static (string Local, string? Variante, short Safra, byte? Mes) DesmontarNomeDaAba(string aba)
    {
        var ano = AnoNoNome().Match(aba);
        if (!ano.Success) throw new FormatException($"A aba \"{aba}\" da CONAB não traz o ano no nome.");

        byte? mes = byte.TryParse(ano.Groups[2].Value, out var m) && m is >= 1 and <= 12 ? m : null;

        var partes = aba[..ano.Index]
            .Replace('´', '\'')
            .Split(['-', ' '], StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !string.Equals(p, "SP", StringComparison.OrdinalIgnoreCase))
            .ToList();

        string? variante = null;
        if (partes.Count > 1 && Variantes.Contains(Normalizar(partes[0])))
        {
            variante = partes[0];
            partes.RemoveAt(0);
        }

        return (string.Join(' ', partes), variante, short.Parse(ano.Groups[1].Value, CultureInfo.InvariantCulture), mes);
    }

    /// <summary>Texto sem acento, em caixa alta e com espaço simples — a chave de comparação dos rótulos.</summary>
    /// <param name="texto">O texto como veio.</param>
    public static string Normalizar(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var sem = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sem.Append(c);

        return Espacos().Replace(sem.ToString(), " ").Trim().ToUpperInvariant();
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    /// <summary>Os dois primeiros números à direita do rótulo: por hectare e por unidade.</summary>
    private static (decimal Ha, decimal Unidade)? Par(object?[] linha, int rotulo)
    {
        var numeros = linha.Skip(rotulo + 1).Where(v => v is double).Select(v => (double)v!).Take(2).ToList();
        if (numeros.Count < 2) return null;

        // DUAS CASAS NO HECTARE, CINCO NA UNIDADE: a CONAB calcula com dízima ("10330.594025974025") e
        // publica o custo por tonelada de cana com cinco casas; guardar a dízima seria fingir precisão.
        return (decimal.Round((decimal)numeros[0], 2), decimal.Round((decimal)numeros[1], 5));
    }

    /// <summary>
    /// A produtividade: "Produtividade Média: 1800,00 kg/ha" num texto só, ou o número na célula ao
    /// lado e a unidade na seguinte ("PRODUTIVIDADE MEDIA: | 65280.0 | KG/HA").
    /// </summary>
    private static (decimal?, string?) Produtividade(object?[] linha, int rotulo, string texto)
    {
        // O NÚMERO PODE ESTAR NO RÓTULO OU NA CÉLULA DE TEXTO AO LADO ("PRODUTIVIDADE | 1800,00 KG/HA"):
        // junta o rótulo com os textos à direita antes de procurar.
        texto = string.Join(' ', linha.Skip(rotulo).OfType<string>().Select(Normalizar));
        var noTexto = ProdutividadeNoTexto().Match(texto);
        if (noTexto.Success)
        {
            // "4125,00 KG" É POR HECTARE — produtividade é sempre por área ou por planta; a aba só não
            // escreveu o denominador.
            var unidade = noTexto.Groups[2].Value.ToLowerInvariant();
            return (decimal.Parse(noTexto.Groups[1].Value.Replace(".", "").Replace(',', '.'), CultureInfo.InvariantCulture),
                    unidade.Contains('/') ? unidade : unidade + "/ha");
        }

        for (var i = rotulo + 1; i < linha.Length; i++)
        {
            if (linha[i] is not double numero) continue;

            var unidade = linha.Skip(i + 1).OfType<string>().Select(Normalizar).FirstOrDefault(u => u.Contains('/'));
            return (decimal.Round((decimal)numero, 2), unidade?.ToLowerInvariant());
        }

        return (null, null);
    }

    /// <summary>O mês de "Mês/Ano: Outubro/2025" ou de "A PREÇOS DE: | AGO/2011".</summary>
    private static byte? MesNoTexto(object?[] linha, int rotulo)
    {
        // NAS ABAS ANTIGAS O MÊS É UMA DATA DO EXCEL ("A PREÇOS DE: | 30/11/2005"), e a leitura a entrega
        // como data — ou como o número de série, conforme a formatação da célula.
        foreach (var celula in linha.Skip(rotulo + 1))
        {
            if (celula is DateTime data) return (byte)data.Month;
            if (celula is double serie && serie is > 20000 and < 80000) return (byte)DateTime.FromOADate(serie).Month;
        }

        var texto = string.Join(' ', linha.Skip(rotulo).OfType<string>().Select(Normalizar));

        for (var i = 0; i < Meses.Length; i++)
        {
            if (Regex.IsMatch(texto, $@"\b({Meses[i]}|{Meses[i][..3]})/\d{{2,4}}"))
                return (byte)(i + 1);
        }

        return null;
    }

    private static IEnumerable<(string Texto, Uri Endereco)> Links(string html, Uri base_)
    {
        foreach (Match a in Ancora().Matches(html))
        {
            var texto = WebUtility.HtmlDecode(Marcacao().Replace(a.Groups[2].Value, " ")).Trim();
            if (Uri.TryCreate(base_, WebUtility.HtmlDecode(a.Groups[1].Value), out var endereco))
                yield return (texto, endereco);
        }
    }

    private static bool EhPlanilha(Uri endereco) =>
        SemVisualizacao(endereco).AbsolutePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)
        || SemVisualizacao(endereco).AbsolutePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

    /// <summary>O Plone publica o arquivo em <c>…/arquivo.xls/view</c> (a página) e em <c>…/arquivo.xls</c> (o arquivo).</summary>
    private static Uri SemVisualizacao(Uri endereco) =>
        endereco.AbsolutePath.EndsWith("/view", StringComparison.OrdinalIgnoreCase)
            ? new Uri(endereco, endereco.AbsolutePath[..^"/view".Length])
            : endereco;

    [GeneratedRegex(@"(^|[-\s])SP([-\s]|$)", RegexOptions.IgnoreCase)]
    private static partial Regex AbaDeSaoPaulo();

    [GeneratedRegex(@"((?:19|20)\d{2})(?:-(\d{2}))?\s*$")]
    private static partial Regex AnoNoNome();

    [GeneratedRegex(@"([\d\.]+,\d+)\s*(KG/HA|T/HA|SC/HA|CX/PLANTA|CX/HA|KG|T\b|SC\b)")]
    private static partial Regex ProdutividadeNoTexto();

    [GeneratedRegex(@"(\d)ª")]
    private static partial Regex SafraDoMilho();

    [GeneratedRegex(@"<a[^>]+href=""([^""]+)""[^>]*>(.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex Ancora();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Marcacao();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Espacos();
}
