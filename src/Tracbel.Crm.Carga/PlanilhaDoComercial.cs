using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Tracbel.Crm.Carga;

/// <summary>Uma linha de dado da planilha, com o número que ela tem no Excel.</summary>
/// <param name="Numero">O número da linha no Excel — o cabeçalho é a linha 1.</param>
/// <param name="Celulas">O texto de cada coluna, pelo nome do cabeçalho.</param>
internal sealed record LinhaDaPlanilha(int Numero, IReadOnlyDictionary<string, string?> Celulas)
{
    /// <summary>O texto de uma coluna, ou nulo quando a célula não existe.</summary>
    /// <param name="coluna">O nome da coluna, como está no cabeçalho.</param>
    public string? this[string coluna] => Celulas.GetValueOrDefault(coluna);
}

/// <summary>
/// A LEITURA DE UMA PLANILHA DO COMERCIAL — a primeira aba, com cabeçalho na linha 1.
///
/// <para><b>Confere as colunas antes de qualquer gravação.</b> Uma planilha com a coluna renomeada
/// ("Municipio" em vez de "Município") não pode virar 238 recusas silenciosas: a carga para na
/// leitura, dizendo quais colunas faltam, e o banco não é tocado.</para>
///
/// <para><b>Tudo chega como texto</b>, do jeito que o Excel guarda: a chave <c>00204</c> continua
/// com os zeros à esquerda, e é a carga que decide o que cada coluna significa. Data e número não
/// são interpretados aqui porque as duas planilhas territoriais não precisam deles.</para>
/// </summary>
internal sealed class PlanilhaDoComercial
{
    private PlanilhaDoComercial(string nomeDoArquivo, IReadOnlyList<LinhaDaPlanilha> linhas)
    {
        NomeDoArquivo = nomeDoArquivo;
        Linhas = linhas;
    }

    /// <summary>O nome do arquivo, sem a pasta — é o que vai para o rastro de origem.</summary>
    public string NomeDoArquivo { get; }

    /// <summary>As linhas de dado, sem o cabeçalho e sem as linhas inteiramente vazias.</summary>
    public IReadOnlyList<LinhaDaPlanilha> Linhas { get; }

    /// <summary>Lê a primeira aba e confere as colunas exigidas.</summary>
    /// <param name="caminho">O caminho do arquivo <c>.xlsx</c>.</param>
    /// <param name="colunasExigidas">Os cabeçalhos que a carga vai usar.</param>
    public static PlanilhaDoComercial Ler(string caminho, IReadOnlyCollection<string> colunasExigidas)
    {
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Planilha não encontrada: {caminho}. Nada foi gravado.", caminho);

        using var documento = SpreadsheetDocument.Open(caminho, false);

        var pasta = documento.WorkbookPart
                    ?? throw new InvalidDataException($"{Path.GetFileName(caminho)} não é uma pasta de trabalho do Excel.");
        var aba = pasta.Workbook?.Sheets?.Elements<Sheet>().FirstOrDefault()
                  ?? throw new InvalidDataException($"{Path.GetFileName(caminho)} não tem nenhuma aba.");
        var planilha = (WorksheetPart)pasta.GetPartById(aba.Id!.Value!);

        var compartilhados = pasta.SharedStringTablePart?.SharedStringTable?
            .Elements<SharedStringItem>().Select(item => item.InnerText).ToArray() ?? [];

        var linhas = planilha.Worksheet?.Descendants<Row>().ToList() ?? [];
        if (linhas.Count == 0)
            throw new InvalidDataException($"{Path.GetFileName(caminho)} está vazia.");

        // O CABEÇALHO EM NFC. O Excel costuma gravar "Município" com o acento já composto, mas
        // uma planilha exportada de outro sistema pode trazê-lo decomposto — e aí "Município" do
        // arquivo e "Município" do código seriam textos diferentes com a mesma aparência.
        var cabecalho = LerCelulas(linhas[0], compartilhados)
            .Where(c => !string.IsNullOrWhiteSpace(c.Value))
            .GroupBy(c => c.Value!.Trim().Normalize(NormalizationForm.FormC), StringComparer.Ordinal)
            .ToDictionary(g => g.First().Key, g => g.Key);

        var faltando = colunasExigidas
            .Where(coluna => !cabecalho.ContainsValue(coluna.Normalize(NormalizationForm.FormC)))
            .ToList();

        if (faltando.Count > 0)
            throw new InvalidDataException(
                $"A planilha {Path.GetFileName(caminho)} não tem a(s) coluna(s): {string.Join(", ", faltando)}. " +
                "Nada foi gravado.");

        var dados = linhas.Skip(1)
            .Select(linha =>
            {
                var valores = LerCelulas(linha, compartilhados);
                var porNome = cabecalho.ToDictionary(
                    c => c.Value, c => valores.GetValueOrDefault(c.Key), StringComparer.Ordinal);
                return new LinhaDaPlanilha((int)(linha.RowIndex?.Value ?? 0), porNome);
            })
            .Where(linha => linha.Celulas.Values.Any(valor => !string.IsNullOrWhiteSpace(valor)))
            .ToList();

        return new PlanilhaDoComercial(Path.GetFileName(caminho), dados);
    }

    private static Dictionary<int, string?> LerCelulas(Row linha, string[] compartilhados)
    {
        var celulas = new Dictionary<int, string?>();

        foreach (var celula in linha.Elements<Cell>())
        {
            var coluna = IndiceDaColuna(celula.CellReference?.Value);
            if (coluna < 0) continue;

            var tipo = celula.DataType?.Value;
            celulas[coluna] =
                tipo == CellValues.SharedString && int.TryParse(celula.CellValue?.Text, out var indice)
                    ? compartilhados[indice]
                    : tipo == CellValues.InlineString
                        ? celula.InlineString?.InnerText
                        : celula.CellValue?.Text;
        }

        return celulas;
    }

    /// <summary><c>A7</c> é a coluna 0, <c>AB12</c> é a 27.</summary>
    private static int IndiceDaColuna(string? referencia)
    {
        if (string.IsNullOrEmpty(referencia)) return -1;

        var indice = 0;
        foreach (var letra in referencia.TakeWhile(char.IsLetter))
            indice = (indice * 26) + (char.ToUpperInvariant(letra) - 'A' + 1);

        return indice - 1;
    }
}
