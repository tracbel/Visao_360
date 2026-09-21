using System.Globalization;
using System.Text;

namespace Tracbel.Crm.Integracao.Conab;

/// <summary>
/// UMA COTAÇÃO MENSAL DA CONAB, como o arquivo de dados abertos a escreve.
///
/// <para><b>O valor é sempre por quilo</b> (a coluna chama <c>valor_produto_kg</c>), qualquer que seja
/// a unidade em que o mercado negocia o produto: café a R$ 31,57/kg é a saca de 60 kg a R$ 1.894,20.
/// A conversão para a unidade comercial é de quem mostra, não de quem lê.</para>
/// </summary>
/// <param name="CodigoDoProduto">O <c>id_produto</c> da CONAB — estável entre os arquivos.</param>
/// <param name="Produto">O nome do produto, em caixa alta (<c>SOJA</c>).</param>
/// <param name="Classificacao">A classificação (<c>EM GRÃOS</c>, <c>ARÁBICA TIPO 6, BEBIDA DURA</c>).</param>
/// <param name="Uf">A sigla da UF.</param>
/// <param name="Ano">O ano do mês.</param>
/// <param name="Mes">O mês, de 1 a 12.</param>
/// <param name="Nivel">O nível de comercialização, como a CONAB escreve.</param>
/// <param name="ValorPorKgBruto">O valor em R$/kg, com vírgula decimal, como veio.</param>
public sealed record LinhaDaCotacaoDaConab(
    string CodigoDoProduto,
    string Produto,
    string Classificacao,
    string Uf,
    int Ano,
    int Mes,
    string Nivel,
    string ValorPorKgBruto);

/// <summary>
/// A LEITURA DOS PREÇOS AGROPECUÁRIOS DA CONAB — o preço recebido pelo produtor, por UF e mês.
///
/// <para><b>Por que a CONAB, e não o CEPEA.</b> O texto-base do potencial cita o CEPEA como fonte de
/// preço. O CEPEA tem termos de uso e bloqueou a leitura automática em 17/09/2026; a licença é a
/// decisão D-P11 da issue 63 e, até ela sair, a coleta dele não se automatiza. A CONAB publica o
/// preço <b>recebido pelo produtor</b> como dado aberto, sem cadastro, com São Paulo e as culturas do
/// texto — café, soja, milho, amendoim, laranja, cana, boi e leite (medido em 21/09/2026).</para>
///
/// <para><b>O arquivo é uma janela de 12 meses, não um histórico.</b> Em 21/09/2026 ele ia de
/// 09/2025 a 08/2026; no mês seguinte, o mês mais velho sai. O histórico é do CRM: a carga nunca
/// apaga o mês que deixou de vir, e a série cresce um mês por vez — é o "não varia, só vamos
/// acrescentando" do texto-base.</para>
///
/// <para><b>Latin-1 e ponto e vírgula.</b> O arquivo não tem BOM e vem em ISO-8859-1: lido como UTF-8,
/// "PREÇO RECEBIDO" vira lixo e o filtro por nível não acha nada. As colunas de texto chegam com
/// espaços à direita, porque o arquivo é de largura fixa disfarçado de CSV.</para>
///
/// <para><b>Só leitura, só GET, só dado público.</b> Nenhuma credencial é usada nem existe.</para>
/// </summary>
/// <param name="http">O cliente HTTP.</param>
public sealed class LeitorDaConab(HttpClient http)
{
    /// <summary>Os preços mensais por UF, dos últimos 12 meses.</summary>
    public const string EnderecoDosPrecosMensaisPorUf =
        "https://portaldeinformacoes.conab.gov.br/downloads/arquivos/PrecosMensalUF.txt";

    /// <summary>
    /// O trecho que identifica o preço recebido pelo produtor na coluna de nível.
    ///
    /// <para>O arquivo tem quatro níveis: atacado, varejo, preço PAGO pelo produtor (o que ele compra:
    /// adubo, defensivo, trator) e preço RECEBIDO (o que ele vende). A rentabilidade da cultura e o
    /// termo de troca olham o que ele recebe.</para>
    /// </summary>
    public const string NivelRecebidoPeloProdutor = "RECEBIDO";

    private static readonly Encoding Latin1 = Encoding.Latin1;

    /// <summary>
    /// As cotações mensais de uma UF, num nível de comercialização.
    /// </summary>
    /// <param name="uf">A sigla da UF (<c>SP</c>).</param>
    /// <param name="trechoDoNivel">Um trecho do nível, sem caixa. Ver <see cref="NivelRecebidoPeloProdutor"/>.</param>
    /// <param name="ct">Cancelamento.</param>
    /// <exception cref="InvalidDataException">Quando o cabeçalho não traz as colunas esperadas.</exception>
    public async Task<IReadOnlyList<LinhaDaCotacaoDaConab>> LerPrecosMensaisAsync(
        string uf, string trechoDoNivel, CancellationToken ct)
    {
        var bytes = await http.GetByteArrayAsync(EnderecoDosPrecosMensaisPorUf, ct);
        return Interpretar(Latin1.GetString(bytes), uf, trechoDoNivel);
    }

    /// <summary>
    /// Interpreta o conteúdo do arquivo já decodificado. Público para os testes.
    /// </summary>
    /// <param name="conteudo">O texto do arquivo.</param>
    /// <param name="uf">A sigla da UF.</param>
    /// <param name="trechoDoNivel">Um trecho do nível de comercialização.</param>
    /// <exception cref="InvalidDataException">Quando o cabeçalho não traz as colunas esperadas.</exception>
    public static IReadOnlyList<LinhaDaCotacaoDaConab> Interpretar(string conteudo, string uf, string trechoDoNivel)
    {
        using var leitor = new StringReader(conteudo);

        var cabecalho = leitor.ReadLine()
            ?? throw new InvalidDataException("O arquivo de preços da CONAB veio vazio.");

        var colunas = cabecalho.Split(';').Select(c => c.Trim()).ToList();
        int Coluna(string nome) => colunas.FindIndex(c => string.Equals(c, nome, StringComparison.OrdinalIgnoreCase));

        // AS COLUNAS SÃO ACHADAS PELO NOME, e não pela posição: o arquivo por município tem duas
        // colunas a mais no meio, e uma mudança igual neste quebraria a leitura sem dar erro.
        var iProduto = Coluna("produto");
        var iClassificacao = Coluna("classificao_produto");   // sic — a CONAB escreve sem o "ç"
        var iId = Coluna("id_produto");
        var iUf = Coluna("uf");
        var iAno = Coluna("ano");
        var iMes = Coluna("mes");
        var iNivel = Coluna("dsc_nivel_comercializacao");
        var iValor = Coluna("valor_produto_kg");

        if (new[] { iProduto, iClassificacao, iId, iUf, iAno, iMes, iNivel, iValor }.Any(i => i < 0))
            throw new InvalidDataException(
                $"O arquivo de preços da CONAB não tem as colunas esperadas. Cabeçalho lido: {cabecalho}");

        var maior = new[] { iProduto, iClassificacao, iId, iUf, iAno, iMes, iNivel, iValor }.Max();
        var linhas = new List<LinhaDaCotacaoDaConab>();

        while (leitor.ReadLine() is { } linha)
        {
            if (string.IsNullOrWhiteSpace(linha)) continue;

            var campos = linha.Split(';');
            if (campos.Length <= maior) continue;

            if (!string.Equals(campos[iUf].Trim(), uf, StringComparison.OrdinalIgnoreCase)) continue;
            if (campos[iNivel].IndexOf(trechoDoNivel, StringComparison.OrdinalIgnoreCase) < 0) continue;

            if (!int.TryParse(campos[iAno].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var ano)
                || !int.TryParse(campos[iMes].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var mes))
                throw new InvalidDataException($"Ano ou mês fora do formato na linha da CONAB: {linha}");

            linhas.Add(new LinhaDaCotacaoDaConab(
                campos[iId].Trim(),
                campos[iProduto].Trim(),
                campos[iClassificacao].Trim(),
                campos[iUf].Trim(),
                ano,
                mes,
                campos[iNivel].Trim(),
                campos[iValor].Trim()));
        }

        return linhas;
    }

    /// <summary>
    /// O valor da CONAB — vírgula decimal, sem separador de milhar (<c>2,32</c>, <c>22,0</c>).
    /// </summary>
    /// <param name="bruto">O valor como veio.</param>
    /// <exception cref="FormatException">Quando não é um número positivo no formato da CONAB.</exception>
    public static decimal ValorPorKg(string bruto)
    {
        // PONTO NÃO É ACEITO. Se a CONAB passar a escrever "1.234,56", ler com ponto de milhar
        // "tolerante" daria certo por acaso; se passar a escrever "2.32", daria 232. Melhor parar.
        if (bruto.Contains('.'))
            throw new FormatException($"Valor da CONAB com ponto — o formato mudou: \"{bruto}\".");

        if (!decimal.TryParse(bruto, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("pt-BR"), out var valor)
            || valor <= 0)
            throw new FormatException($"Valor da CONAB fora do formato: \"{bruto}\".");

        return valor;
    }
}
