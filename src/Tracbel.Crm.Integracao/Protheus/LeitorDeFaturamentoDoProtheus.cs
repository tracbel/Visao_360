using System.Globalization;
using System.Text.Json;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Integracao.Carga;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// O FATURAMENTO LIDO DA ORIGEM — e não da cópia que morreu.
///
/// ---------------------------------------------------------------------------------------------
/// POR QUE ESTE LEITOR SUBSTITUI O DO VÓRTICE. O CRM operou meses com "o faturamento parou em
/// 11/04/2025". A frase vinha de <c>X_TOTVS_CRM_FATURAMENTO</c>, a tabela que o Vórtice
/// <b>recebe</b> do Protheus — e essa de fato para naquela data. Medido na origem em 06/09/2026:
/// a <c>SD2</c> tem nota emitida na mesma semana. <b>O que morreu foi a integração, não o
/// faturamento.</b>
///
/// Números do recorte de 2026, medidos: <b>54.158 itens de nota</b>, <b>R$ 263.369.076,97</b>,
/// dos quais <b>68,2% em máquinas</b>.
///
/// ---------------------------------------------------------------------------------------------
/// AS DUAS TABELAS, E POR QUE SÃO DUAS.
///
/// <c>SD2</c> é o item da nota fiscal de saída: filial, emissão, produto, grupo, cliente, valor.
/// Ela identifica o cliente por <b>código do Protheus</b> (<c>D2_CLIENTE</c> + <c>D2_LOJA</c>), e
/// o CRM identifica por CPF/CNPJ. Quem traduz é a <c>SA1</c>, o cadastro de clientes, onde
/// <c>A1_CGC</c> guarda o documento.
///
/// <para><b>A loja faz parte da chave, e não é detalhe.</b> Medido: o cliente <c>008096049</c>
/// tem as lojas 0001, 0004 e 0008 com <b>três CNPJs diferentes</b> — matriz e filiais do mesmo
/// grupo. Casar só por código atribuiria a nota da filial ao CNPJ da matriz.</para>
///
/// ---------------------------------------------------------------------------------------------
/// MÁQUINA OU PEÇA sai de <c>D2_GRUPO</c>, que viaja <b>na própria linha da nota</b> — não é
/// preciso juntar com a <c>SB1</c>, que tem 517 mil linhas. O catálogo dos grupos é a
/// <c>SBM</c>: <c>VEIC</c> é máquina, a faixa <c>1001..10xx</c> é peça por linha de produto,
/// <c>SRV</c> é serviço, <c>MO_O</c> é mão de obra de oficina.
/// </summary>
/// <param name="ponte">A ponte de leitura do Protheus.</param>
public sealed class LeitorDeFaturamentoDoProtheus(PonteDoProtheus ponte)
{
    /// <summary>Os campos da nota que o CRM usa. Nenhum a mais: cada um custa banda.</summary>
    private const string CamposDaNota =
        "D2_FILIAL,D2_EMISSAO,D2_CLIENTE,D2_LOJA,D2_GRUPO,D2_TOTAL,D2_DOC";

    /// <summary>Os campos do cadastro de cliente que traduzem o código para documento.</summary>
    private const string CamposDoCliente = "A1_COD,A1_LOJA,A1_CGC";

    /// <summary>
    /// Lê o faturamento a partir de uma data, agregado por cliente, filial e mês.
    /// </summary>
    /// <param name="desde">A primeira emissão a considerar.</param>
    /// <param name="relatar">Para dizer o que está acontecendo numa leitura de minutos.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LoteDoProtheus>> LerAsync(
        DateOnly desde, Action<string> relatar, CancellationToken ct)
    {
        relatar("  lendo o cadastro de clientes do Protheus (SA1), para traduzir código em CNPJ…");

        var clientes = await ponte.LerTudoAsync(
            "SA1", CamposDoCliente, null,
            (lidos, total) => { if (lidos % 10_000 == 0) relatar($"    SA1: {lidos:N0} de {total:N0}"); },
            ct);

        if (!clientes.EhSucesso) return Resultado<LoteDoProtheus>.Indisponivel(clientes.Erro!);

        // A CHAVE É CÓDIGO + LOJA. Ver o comentário da classe: o mesmo código com lojas
        // diferentes tem CNPJs diferentes.
        var documentoPorCliente = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var linha in clientes.Valor)
        {
            var documento = SoDigitos(Texto(linha, "a1_cgc"));
            if (documento.Length is not (11 or 14)) continue;

            documentoPorCliente[$"{Texto(linha, "a1_cod")}/{Texto(linha, "a1_loja")}"] = documento;
        }

        relatar($"    {documentoPorCliente.Count:N0} clientes com documento válido.");
        relatar($"  lendo as notas emitidas desde {desde:dd/MM/yyyy} (SD2)…");

        var notas = await ponte.LerTudoAsync(
            "SD2", CamposDaNota, $"D2_EMISSAO >= '{desde:yyyyMMdd}'",
            (lidos, total) => { if (lidos % 10_000 == 0) relatar($"    SD2: {lidos:N0} de {total:N0}"); },
            ct);

        if (!notas.EhSucesso) return Resultado<LoteDoProtheus>.Indisponivel(notas.Erro!);

        var porChave = new Dictionary<(string Documento, string Filial, DateOnly Mes), Acumulado>();
        var semCliente = new HashSet<string>(StringComparer.Ordinal);
        var semData = 0;
        DateOnly? maisRecente = null;

        foreach (var linha in notas.Valor)
        {
            var emissao = Data(Texto(linha, "d2_emissao"));
            if (emissao is null) { semData++; continue; }

            if (maisRecente is null || emissao > maisRecente) maisRecente = emissao;

            var chaveDoCliente = $"{Texto(linha, "d2_cliente")}/{Texto(linha, "d2_loja")}";
            if (!documentoPorCliente.TryGetValue(chaveDoCliente, out var documento))
            {
                semCliente.Add(chaveDoCliente);
                continue;
            }

            var mes = new DateOnly(emissao.Value.Year, emissao.Value.Month, 1);
            var chave = (documento, Texto(linha, "d2_filial"), mes);

            if (!porChave.TryGetValue(chave, out var acumulado))
                acumulado = porChave[chave] = new Acumulado();

            acumulado.Valor += Decimal(Texto(linha, "d2_total"));
            acumulado.Itens++;
            acumulado.Notas.Add(Texto(linha, "d2_doc"));
        }

        var agregado = porChave
            // VALOR ZERO OU NEGATIVO NÃO ENTRA. A SD2 guarda devolução e ajuste como linha
            // própria, e um mês que fecha em zero para um cliente não é faturamento — é o
            // encontro de contas de uma venda com a devolução dela.
            .Where(p => p.Value.Valor > 0)
            .Select(p => new FaturamentoParaCarga(
                ChaveDeOrigem: $"{p.Key.Documento}/{p.Key.Filial}/{p.Key.Mes:yyyy-MM}",
                DocumentoDoCliente: p.Key.Documento,
                // A FILIAL VEM COMO CÓDIGO DE SEIS DÍGITOS ('010101'), que é exatamente o código
                // de `organizacao.Empresa` — melhor que o `NroEmpresa` do Vórtice, que exigia
                // de-para. O campo numérico do registro fica em zero e o código vai no lugar dele.
                CodigoDaFilialNoLegado: 0,
                CodigoDaFilial: p.Key.Filial,
                Competencia: p.Key.Mes,
                ValorLiquido: decimal.Round(p.Value.Valor, 2),
                Notas: p.Value.Notas.Count,
                Itens: p.Value.Itens))
            .ToList();

        relatar(
            $"    {notas.Valor.Count:N0} itens de nota lidos · {agregado.Count:N0} meses de " +
            $"faturamento por cliente · {semCliente.Count:N0} código(s) de cliente sem cadastro.");

        return Resultado<LoteDoProtheus>.Ok(new LoteDoProtheus(
            agregado,
            [.. porChave.Keys.Select(k => k.Filial).Distinct()],
            notas.Valor.Count,
            semCliente.Count,
            semData,
            maisRecente));
    }

    private sealed class Acumulado
    {
        public decimal Valor;
        public int Itens;
        public readonly HashSet<string> Notas = new(StringComparer.Ordinal);
    }

    private static string Texto(Dictionary<string, JsonElement> linha, string campo) =>
        linha.TryGetValue(campo, out var valor)
            ? (valor.ValueKind == JsonValueKind.String ? valor.GetString() ?? string.Empty : valor.ToString()).Trim()
            : string.Empty;

    private static string SoDigitos(string texto) =>
        new([.. texto.Where(char.IsAsciiDigit)]);

    /// <summary>
    /// A data como o Protheus a devolve.
    ///
    /// <para>Ele alterna entre <c>yyyy-MM-dd</c> e <c>yyyyMMdd</c> conforme o campo, e uma data
    /// não reconhecida vira nulo — nunca <c>DateTime.Today</c>, que jogaria a nota no mês errado
    /// em silêncio.</para>
    /// </summary>
    private static DateOnly? Data(string bruto)
    {
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        return DateOnly.TryParseExact(bruto, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                   DateTimeStyles.None, out var comTraco)
            ? comTraco
            : DateOnly.TryParseExact(bruto, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var semTraco)
                ? semTraco
                : null;
    }

    /// <summary>
    /// O valor como o Protheus o devolve: texto, com PONTO decimal.
    ///
    /// <para>Ler com a cultura da máquina faria "674.45" virar 67.445 numa estação em pt-BR — um
    /// erro de cem vezes que nenhum total denunciaria.</para>
    /// </summary>
    private static decimal Decimal(string bruto) =>
        decimal.TryParse(bruto, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : 0m;
}

/// <summary>
/// O que uma leitura de faturamento do Protheus produziu.
/// </summary>
/// <param name="Faturamento">O agregado por cliente, filial e mês.</param>
/// <param name="FiliaisVistas">Os códigos de filial encontrados nas notas.</param>
/// <param name="ItensLidos">Quantos itens de nota a origem devolveu.</param>
/// <param name="ClientesSemCadastro">Códigos de cliente da nota que não estão na SA1.</param>
/// <param name="ItensSemData">Itens descartados por não ter data de emissão legível.</param>
/// <param name="EmissaoMaisRecente">A nota mais nova vista. Vira a marca de sincronismo.</param>
public sealed record LoteDoProtheus(
    IReadOnlyList<FaturamentoParaCarga> Faturamento,
    IReadOnlyList<string> FiliaisVistas,
    int ItensLidos,
    int ClientesSemCadastro,
    int ItensSemData,
    DateOnly? EmissaoMaisRecente);
