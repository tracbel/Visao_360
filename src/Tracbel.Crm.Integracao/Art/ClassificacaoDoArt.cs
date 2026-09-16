using System.Globalization;
using System.Text;
using Tracbel.Crm.Dominio.Integracao;

namespace Tracbel.Crm.Integracao.Art;

/// <summary>Um modelo do catálogo de frota, como a correspondência de produto precisa dele.</summary>
/// <param name="Id">O identificador.</param>
/// <param name="Codigo">O código estável.</param>
public sealed record ModeloDoCatalogo(int Id, string Codigo);

/// <summary>Uma filial do CRM, como a correspondência de unidade precisa dela.</summary>
/// <param name="Id">O identificador.</param>
/// <param name="Codigo">O código 0101NN.</param>
/// <param name="Nome">O nome, no padrão "Tracbel Agro — Cidade".</param>
/// <param name="EstaAtiva">Se a filial está em operação no CRM.</param>
public sealed record FilialDoCrm(int Id, string Codigo, string Nome, bool EstaAtiva);

/// <summary>O resultado de uma regra de correspondência.</summary>
/// <param name="Situacao">Exata, pendente ou "não é classificação".</param>
/// <param name="Destino">O código da classificação, o id do modelo ou o id da filial — conforme a regra.</param>
/// <param name="Criterio">A regra aplicada, ou o motivo da pendência, em texto.</param>
public sealed record AvaliacaoDeCorrespondencia(SituacaoDaCorrespondencia Situacao, string? Destino, string Criterio);

/// <summary>
/// O DE-PARA EXPLÍCITO do ART para o catálogo do CRM (documento 35, seção 10) — linha, produto e
/// unidade.
///
/// <para><b>Nenhuma regra aqui é semelhança de nome.</b> A linha passa por uma tabela escrita, uma
/// entrada por valor da origem. O produto só corresponde a um modelo quando o código é
/// <b>idêntico</b> depois de tirar espaço e pontuação, e há um candidato só. A unidade só
/// corresponde a uma filial quando o nome é idêntico, sem acento e caixa. Tudo o que não passa fica
/// <see cref="SituacaoDaCorrespondencia.PendenteDeRevisao"/>, com o motivo escrito.</para>
/// </summary>
public static class ClassificacaoDoArt
{
    /// <summary>
    /// A TABELA DAS LINHAS DO ART, pelo código normalizado da linha. As 14 linhas medidas em
    /// 14/09/2026 estão todas aqui; uma linha nova no ART fica pendente até entrar nesta tabela.
    ///
    /// <para>As quatro linhas de colhedora ("COLHEDORA CANA" e as variantes com o modelo no nome) são
    /// a MESMA categoria: o modelo que o ART pendura na linha continua preservado na venda e no
    /// produto. "USADOS" não é categoria de produto — é condição da venda — e por isso não classifica
    /// a máquina.</para>
    /// </summary>
    private static readonly Dictionary<string, string?> Linhas = new(StringComparer.Ordinal)
    {
        ["TRATOR_PEQUENO"] = "TRATOR_PEQUENO",
        ["TRATOR_MEDIO"] = "TRATOR_MEDIO",
        ["TRATOR_GRANDE"] = "TRATOR_GRANDE",
        ["COLHEDORA_CANA"] = "COLHEDORA_DE_CANA",
        ["COLHEDORA_CANA_CH_570"] = "COLHEDORA_DE_CANA",
        ["COLHEDORA_CANA_CH_750"] = "COLHEDORA_DE_CANA",
        ["COLHEDORA_CANA_CH_950"] = "COLHEDORA_DE_CANA",
        ["COLHEITADEIRA"] = "COLHEITADEIRA",
        ["PLANTADEIRA"] = "PLANTADEIRA",
        ["PULVERIZADOR"] = "PULVERIZADOR",
        ["PLATAFORMA_CORTE"] = "PLATAFORMA_DE_CORTE",
        ["IMPLEMENTOS_JD"] = "IMPLEMENTO_JOHN_DEERE",
        ["IMPLEMENTOS_OM"] = "IMPLEMENTO_OUTRAS_MARCAS",
        ["USADOS"] = null
    };

    /// <summary>
    /// Os produtos que o ART usa como descrição genérica — não identificam modelo nenhum.
    /// </summary>
    private static readonly HashSet<string> ProdutosGenericos = new(StringComparer.Ordinal)
    {
        "IMPLEMENTOS", "IMPLEMENTOS_OM", "USADO_OUTRAS_MARCAS"
    };

    /// <summary>
    /// O código estável de um texto: sem acento, maiúsculo, só letra, número e sublinhado — o mesmo
    /// algoritmo do catálogo de frota da carga, para a recarga reencontrar a mesma linha.
    /// </summary>
    /// <param name="texto">O texto da origem.</param>
    /// <param name="tamanhoMaximo">O tamanho da coluna.</param>
    public static string Codificar(string texto, int tamanhoMaximo = 80)
    {
        var construtor = new StringBuilder(texto.Length);
        foreach (var caractere in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark) continue;
            construtor.Append(char.IsAsciiLetterOrDigit(caractere) ? char.ToUpperInvariant(caractere) : '_');
        }

        var codigo = construtor.ToString();
        while (codigo.Contains("__", StringComparison.Ordinal))
            codigo = codigo.Replace("__", "_", StringComparison.Ordinal);

        codigo = codigo.Trim('_');
        if (codigo.Length > tamanhoMaximo) codigo = codigo[..tamanhoMaximo].TrimEnd('_');
        return codigo.Length == 0 ? "SEM_CODIGO" : codigo;
    }

    /// <summary>Só letras e números, sem acento, maiúsculos — para comparar identidade, não parecença.</summary>
    /// <param name="texto">O texto.</param>
    public static string NormalizarParaComparar(string texto) => Codificar(texto, int.MaxValue).Replace("_", string.Empty, StringComparison.Ordinal) switch
    {
        "SEMCODIGO" => string.Empty,
        var normalizado => normalizado
    };

    /// <summary>A classificação de produto do CRM para uma linha do ART.</summary>
    /// <param name="linhaNaOrigem">A linha exatamente como o ART escreve.</param>
    public static AvaliacaoDeCorrespondencia ClassificarLinha(string linhaNaOrigem)
    {
        var codigo = Codificar(linhaNaOrigem);

        if (!Linhas.TryGetValue(codigo, out var classificacao))
            return new(SituacaoDaCorrespondencia.PendenteDeRevisao, null,
                "Linha sem entrada na tabela explícita do ART — precisa de uma decisão antes de classificar.");

        return classificacao is null
            ? new(SituacaoDaCorrespondencia.NaoEClassificacaoDeProduto, null,
                "\"Usados\" é condição da venda, não categoria de produto: a máquina não é classificada por esta linha.")
            : new(SituacaoDaCorrespondencia.CorrespondenciaExata, classificacao,
                $"Tabela explícita de linhas do ART: {codigo} → {classificacao}.");
    }

    /// <summary>
    /// O modelo do catálogo para um produto do ART — só por código idêntico e candidato único.
    /// </summary>
    /// <param name="produtoNaOrigem">O produto exatamente como o ART escreve.</param>
    /// <param name="modelos">Os modelos ativos do catálogo.</param>
    public static AvaliacaoDeCorrespondencia ClassificarProduto(string produtoNaOrigem, IReadOnlyList<ModeloDoCatalogo> modelos)
    {
        if (ProdutosGenericos.Contains(Codificar(produtoNaOrigem)))
            return new(SituacaoDaCorrespondencia.NaoEClassificacaoDeProduto, null,
                "Descrição genérica no ART — não identifica modelo.");

        var procurado = NormalizarParaComparar(produtoNaOrigem);
        var candidatos = modelos.Where(m => NormalizarParaComparar(m.Codigo) == procurado).ToList();

        return candidatos.Count switch
        {
            1 => new(SituacaoDaCorrespondencia.CorrespondenciaExata,
                candidatos[0].Id.ToString(CultureInfo.InvariantCulture),
                $"Código do modelo idêntico ao produto, sem espaço e pontuação: {candidatos[0].Codigo}."),
            0 => new(SituacaoDaCorrespondencia.PendenteDeRevisao, null,
                "Nenhum modelo do catálogo tem código idêntico ao produto. Semelhança de nome não é usada."),
            _ => new(SituacaoDaCorrespondencia.PendenteDeRevisao, null,
                $"Correspondência ambígua: {candidatos.Count} modelos com o mesmo código normalizado " +
                $"({string.Join(", ", candidatos.Select(c => c.Codigo).Order(StringComparer.Ordinal))}).")
        };
    }

    /// <summary>
    /// A CLASSIFICAÇÃO SÓ É APLICADA QUANDO NÃO CONTRADIZ O MODELO.
    ///
    /// <para>Medido na primeira carga: uma máquina que o CRM já tinha como trator John Deere recebeu
    /// "implemento de outras marcas", porque a linha da venda no ART dizia "IMPLEMENTOS OM". A linha de
    /// uma venda não é prova da categoria de uma máquina que já tem modelo: quando a família da
    /// classificação e a família do modelo existem e são diferentes, a classificação não entra.</para>
    /// </summary>
    /// <param name="familiaDoModelo">A família do modelo da máquina, quando ela tem modelo.</param>
    /// <param name="familiaDaClassificacao">A família apontada pela classificação, quando existe.</param>
    public static bool ClassificacaoCompativelComOModelo(int? familiaDoModelo, int? familiaDaClassificacao) =>
        familiaDoModelo is null || familiaDaClassificacao is null || familiaDoModelo == familiaDaClassificacao;

    /// <summary>A filial do CRM para uma unidade do ART — só por nome idêntico e filial única.</summary>
    /// <param name="unidadeNaOrigem">A unidade exatamente como o ART escreve.</param>
    /// <param name="filiais">As filiais do CRM, ativas e inativas.</param>
    public static AvaliacaoDeCorrespondencia ClassificarUnidade(string unidadeNaOrigem, IReadOnlyList<FilialDoCrm> filiais)
    {
        var procurada = NormalizarParaComparar(unidadeNaOrigem);
        var candidatas = filiais
            .Where(f => f.Codigo.StartsWith("0101", StringComparison.Ordinal) && NormalizarParaComparar(Cidade(f.Nome)) == procurada)
            .ToList();

        if (candidatas.Count != 1)
            return new(SituacaoDaCorrespondencia.PendenteDeRevisao, null,
                candidatas.Count == 0
                    ? "Nenhuma filial do CRM tem o nome idêntico ao da unidade."
                    : $"Mais de uma filial com o nome da unidade ({string.Join(", ", candidatas.Select(c => c.Codigo))}).");

        var filial = candidatas[0];
        return new(SituacaoDaCorrespondencia.CorrespondenciaExata, filial.Id.ToString(CultureInfo.InvariantCulture),
            $"Nome da unidade idêntico ao da filial {filial.Codigo}, sem acento e caixa; " +
            (filial.EstaAtiva ? "filial ativa no CRM." : "filial INATIVA no CRM — a venda é registrada nela, e a filial fica sinalizada."));
    }

    /// <summary>A cidade no nome da filial: o que vem depois do travessão.</summary>
    private static string Cidade(string nomeDaFilial)
    {
        var travessao = nomeDaFilial.LastIndexOf('—');
        return travessao < 0 ? nomeDaFilial : nomeDaFilial[(travessao + 1)..];
    }
}
