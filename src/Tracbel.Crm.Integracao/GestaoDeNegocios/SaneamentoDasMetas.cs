using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>Os códigos de recusa de uma linha do cadastro de metas.</summary>
public static class MotivoDeRecusaDaMeta
{
    /// <summary>O mês não é <c>AAAA-MM</c> (ou <c>AAAA-MM-DD</c>, <c>MM/AAAA</c>) de um ano plausível.</summary>
    public const string MesInvalido = "MES_INVALIDO";

    /// <summary>A filial não é um NN de <c>0101NN</c>.</summary>
    public const string FilialInvalida = "FILIAL_INVALIDA";

    /// <summary>A linha veio vazia, ou maior que a coluna.</summary>
    public const string LinhaInvalida = "LINHA_INVALIDA";

    /// <summary>O consultor veio vazio, ou maior que a coluna.</summary>
    public const string ConsultorInvalido = "CONSULTOR_INVALIDO";

    /// <summary>O tipo não é Concessão nem Direta.</summary>
    public const string TipoDesconhecido = "TIPO_DESCONHECIDO";

    /// <summary>A origem não é Campanha nem Consórcio.</summary>
    public const string OrigemDesconhecida = "ORIGEM_DESCONHECIDA";

    /// <summary>A quantidade não é um inteiro de 0 a 100.000.</summary>
    public const string QuantidadeInvalida = "QUANTIDADE_INVALIDA";

    /// <summary>A filial é um NN que nenhuma filial do CRM tem — decidido na carga, que conhece as filiais.</summary>
    public const string FilialSemEmpresaNoCrm = "FILIAL_SEM_EMPRESA_NO_CRM";
}

/// <summary>Uma linha de meta depois do saneamento — pronta para a carga casar filial, linha e consultor.</summary>
/// <param name="IdNaOrigem">O id da GN.</param>
/// <param name="Competencia">O mês, no dia 1.</param>
/// <param name="FilialCodigo">O código da filial no CRM (<c>0101NN</c>).</param>
/// <param name="LinhaNaOrigem">A linha como a GN escreve, sem espaço nas pontas.</param>
/// <param name="CodigoDaLinha">O código estável da linha — o MESMO algoritmo do ART, para o realizado casar.</param>
/// <param name="ConsultorNaOrigem">O consultor em maiúsculas, como a GN escreve.</param>
/// <param name="VendaDireta">Tipo Direta; falso é Concessão.</param>
/// <param name="Origem">Campanha ou Consórcio.</param>
/// <param name="Quantidade">A meta em unidades.</param>
/// <param name="ValorUnitario">O valor unitário; nulo quando não veio ou não se leu.</param>
/// <param name="Margem">A margem; nula quando não veio ou não se leu.</param>
/// <param name="Hash">O resumo do conteúdo lido — é o que a recarga compara.</param>
/// <param name="Transformacoes">O que o saneamento deixou de lado sem recusar a linha (valor ilegível, por exemplo).</param>
public sealed record MetaSaneada(
    int IdNaOrigem,
    DateOnly Competencia,
    string FilialCodigo,
    string LinhaNaOrigem,
    string CodigoDaLinha,
    string ConsultorNaOrigem,
    bool VendaDireta,
    OrigemDaMeta Origem,
    int Quantidade,
    decimal? ValorUnitario,
    decimal? Margem,
    string Hash,
    IReadOnlyList<string> Transformacoes)
{
    /// <summary>
    /// A META DE CONSÓRCIO fica à parte (D-M4): pela origem ou pela linha CONSÓRCIO. As duas marcas vieram juntas nas 266
    /// linhas medidas; a regra aceita qualquer uma, para uma linha de consórcio nunca cair na soma de máquinas.
    /// </summary>
    public bool EhConsorcio => MetaDeVenda.EhConsorcio(Origem, CodigoDaLinha);
}

/// <summary>O resultado do saneamento de uma linha: a meta, ou os motivos de recusa.</summary>
/// <param name="Meta">A meta saneada; nula quando recusada.</param>
/// <param name="Motivos">Os códigos de recusa; vazio quando a linha entra.</param>
public sealed record SaneamentoDaMeta(MetaSaneada? Meta, IReadOnlyList<string> Motivos);

/// <summary>
/// O SANEAMENTO DO CADASTRO DE METAS — funções puras, sem banco e sem rede, testáveis linha a linha (princípio 1.3 do
/// documento 16: o que não passa sai com o motivo escrito; nada é completado nem adivinhado).
///
/// <para><b>O que recusa a linha</b> é o que a meta não existe sem: mês, filial, linha, consultor, tipo, origem e
/// quantidade. <b>O que não recusa</b> é o valor unitário e a margem, que não entram na conta de unidades: ilegíveis,
/// ficam nulos, com a transformação anotada.</para>
///
/// <para><b>A linha é codificada com o algoritmo do ART</b> (<see cref="ClassificacaoDoArt.Codificar"/>): o realizado
/// casa com a meta por código, dos dois lados — "TRATOR MÉDIO" e "TRATOR MEDIO" são o mesmo <c>TRATOR_MEDIO</c>.</para>
/// </summary>
public static partial class SaneamentoDasMetas
{
    /// <summary>O tamanho da coluna da linha.</summary>
    public const int TamanhoDaLinha = 60;

    /// <summary>O tamanho da coluna do consultor.</summary>
    public const int TamanhoDoConsultor = 80;

    /// <summary>A maior quantidade aceita numa linha — muito acima de qualquer meta mensal de uma pessoa.</summary>
    public const int QuantidadeMaxima = 100_000;

    /// <summary>Saneia uma linha.</summary>
    /// <param name="linha">A linha como veio.</param>
    public static SaneamentoDaMeta Sanear(MetaNaOrigem linha)
    {
        var motivos = new List<string>();
        var transformacoes = new List<string>();

        var competencia = Competencia(linha.Mes);
        if (competencia is null) motivos.Add(MotivoDeRecusaDaMeta.MesInvalido);

        var filial = CodigoDaFilial(linha.FilialNumero);
        if (filial is null) motivos.Add(MotivoDeRecusaDaMeta.FilialInvalida);

        var textoDaLinha = linha.Linha?.Trim() ?? string.Empty;
        if (textoDaLinha.Length is 0 or > TamanhoDaLinha) motivos.Add(MotivoDeRecusaDaMeta.LinhaInvalida);

        var consultor = linha.Consultor?.Trim().ToUpperInvariant() ?? string.Empty;
        if (consultor.Length is 0 or > TamanhoDoConsultor) motivos.Add(MotivoDeRecusaDaMeta.ConsultorInvalido);

        bool? direta = ClassificacaoDoArt.Codificar(linha.Tipo ?? string.Empty) switch
        {
            "DIRETA" or "VENDA_DIRETA" => true,
            "CONCESSAO" => false,
            _ => null
        };
        if (direta is null) motivos.Add(MotivoDeRecusaDaMeta.TipoDesconhecido);

        OrigemDaMeta? origem = ClassificacaoDoArt.Codificar(linha.Origem ?? string.Empty) switch
        {
            "CAMPANHA" => OrigemDaMeta.Campanha,
            "CONSORCIO" => OrigemDaMeta.Consorcio,
            _ => null
        };
        if (origem is null) motivos.Add(MotivoDeRecusaDaMeta.OrigemDesconhecida);

        var quantidade = Quantidade(linha.Quantidade);
        if (quantidade is null) motivos.Add(MotivoDeRecusaDaMeta.QuantidadeInvalida);

        var valor = Decimal("valor unitário", linha.ValorUnitario, transformacoes);
        var margem = Decimal("margem", linha.Margem, transformacoes);

        if (motivos.Count > 0) return new SaneamentoDaMeta(null, motivos);

        return new SaneamentoDaMeta(
            new MetaSaneada(
                linha.Id, competencia!.Value, filial!, textoDaLinha, ClassificacaoDoArt.Codificar(textoDaLinha, TamanhoDaLinha),
                consultor, direta!.Value, origem!.Value, quantidade!.Value, valor, margem, Resumir(linha), transformacoes),
            []);
    }

    /// <summary>
    /// O mês da meta, no dia 1. Aceita <c>AAAA-MM</c>, <c>AAAA-MM-DD</c> (com hora ou não) e <c>MM/AAAA</c>, de 2000 a
    /// 2100 — o formato exato do campo ainda não foi conferido, e os três são leituras sem ambiguidade do mesmo mês.
    /// </summary>
    /// <param name="bruto">O campo como veio.</param>
    public static DateOnly? Competencia(string? bruto)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;
        var texto = bruto.Trim();

        int ano, mes;
        if (AnoMes().Match(texto) is { Success: true } iso)
        {
            ano = int.Parse(iso.Groups["ano"].Value, CultureInfo.InvariantCulture);
            mes = int.Parse(iso.Groups["mes"].Value, CultureInfo.InvariantCulture);
        }
        else if (MesAno().Match(texto) is { Success: true } br)
        {
            ano = int.Parse(br.Groups["ano"].Value, CultureInfo.InvariantCulture);
            mes = int.Parse(br.Groups["mes"].Value, CultureInfo.InvariantCulture);
        }
        else return null;

        return ano is >= 2000 and <= 2100 && mes is >= 1 and <= 12 ? new DateOnly(ano, mes, 1) : null;
    }

    /// <summary>
    /// O FORMATO de um mês que não se leu — cada dígito vira <c>N</c> e cada letra vira <c>A</c> (<c>"11/25"</c> →
    /// <c>NN/NN</c>). É o que a mensagem de uma carga abortada pode dizer sem repetir o conteúdo da origem: o formato
    /// basta para saber o que mudou na API.
    /// </summary>
    /// <param name="bruto">O campo como veio.</param>
    public static string FormatoDoMes(string? bruto)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return "(vazio)";
        var texto = bruto.Trim();
        if (texto.Length > 30) texto = texto[..30];
        return new string([.. texto.Select(c => char.IsDigit(c) ? 'N' : char.IsLetter(c) ? 'A' : c)]);
    }

    /// <summary>
    /// O código da filial no CRM: <c>0101</c> + NN. Aceita o NN com um ou dois dígitos (<c>5</c> ou <c>"05"</c>) e o código
    /// inteiro (<c>010105</c>).
    /// </summary>
    /// <param name="bruto">O campo como veio.</param>
    public static string? CodigoDaFilial(string? bruto)
    {
        var texto = bruto?.Trim() ?? string.Empty;
        if (texto.Length == 6 && texto.StartsWith("0101", StringComparison.Ordinal) && texto.All(char.IsAsciiDigit)) return texto;
        if (texto.Length is 1 or 2 && texto.All(char.IsAsciiDigit) && texto != "0" && texto != "00")
            return "0101" + texto.PadLeft(2, '0');
        return null;
    }

    private static int? Quantidade(string? bruto)
    {
        if (!decimal.TryParse(bruto?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor)) return null;
        if (valor != decimal.Truncate(valor) || valor < 0 || valor > QuantidadeMaxima) return null;
        return (int)valor;
    }

    private static decimal? Decimal(string campo, string? bruto, List<string> transformacoes)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;
        if (decimal.TryParse(bruto.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor)
            && valor is >= -999_999_999_999m and <= 999_999_999_999m)
            return valor;

        transformacoes.Add($"{campo}: ilegível na origem, ficou vazio");
        return null;
    }

    /// <summary>
    /// O resumo do conteúdo lido — é o que a recarga compara para saber se a meta mudou. Entra tudo o que foi lido, e nada
    /// mais: o resumo não é reversível para o nome do consultor.
    /// </summary>
    private static string Resumir(MetaNaOrigem m) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\u001f',
            m.Id.ToString(CultureInfo.InvariantCulture), m.Mes, m.FilialNumero, m.Linha, m.Consultor, m.Tipo, m.Origem,
            m.Quantidade, m.ValorUnitario, m.Margem))));

    [GeneratedRegex(@"^(?<ano>\d{4})-(?<mes>\d{2})(-\d{2}([T ].*)?)?$")]
    private static partial Regex AnoMes();

    [GeneratedRegex(@"^(?<mes>\d{1,2})/(?<ano>\d{4})$")]
    private static partial Regex MesAno();
}
