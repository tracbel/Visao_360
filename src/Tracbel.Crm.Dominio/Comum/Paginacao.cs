namespace Tracbel.Crm.Dominio.Comum;

/// <summary>
/// Quantas linhas e a partir de qual — a fatia que uma listagem devolve.
///
/// O TAMANHO TEM TETO, e o teto não é decoração: sem ele, um <c>?tamanho=1000000</c> feito por
/// engano (ou de propósito) transforma qualquer listagem numa exportação da base inteira. [V]
/// é literalmente o que os 125 relatórios sem predicado de usuário do Vórtice permitem hoje.
///
/// DÍVIDA NOMEADA: o documento 03, seção 7, pede paginação POR CURSOR, nunca <c>OFFSET</c> em
/// tabela grande. O que está aqui é <c>OFFSET</c>, porque a tela de listagem aprovada tem
/// numeração de páginas e contagem total, e nenhuma das duas tabelas desta fase
/// (<c>comercial.Cliente</c>, <c>frota.Equipamento</c>) tem volume que faça o <c>OFFSET</c>
/// doer. Está registrado no documento 23, seção de dívida, para a listagem de
/// <c>processo.Interacao</c> — essa sim com milhões de linhas — nascer por cursor.
/// </summary>
/// <param name="Pagina">A página pedida, começando em 1.</param>
/// <param name="Tamanho">Quantas linhas por página.</param>
public sealed record Paginacao(int Pagina, int Tamanho)
{
    /// <summary>Quantas linhas uma página traz quando ninguém escolhe.</summary>
    public const int TamanhoPadrao = 25;

    /// <summary>O teto por página. Pedir mais é erro de entrada, não um pedido gigante aceito.</summary>
    public const int TamanhoMaximo = 200;

    /// <summary>Quantas linhas pular para chegar nesta página.</summary>
    public int Saltar => (Pagina - 1) * Tamanho;

    /// <summary>
    /// Monta a paginação a partir do que veio na consulta, recusando o que não faz sentido em
    /// vez de silenciosamente "consertar" — corrigir em silêncio é o que faz o usuário achar
    /// que pediu uma coisa e receber outra (documento 16, princípio 1.3).
    /// </summary>
    /// <param name="pagina">A página pedida; nulo vira 1.</param>
    /// <param name="tamanho">O tamanho pedido; nulo vira <see cref="TamanhoPadrao"/>.</param>
    public static Resultado<Paginacao> Criar(int? pagina, int? tamanho)
    {
        var erros = new List<ErroDeCampo>();

        var p = pagina ?? 1;
        var t = tamanho ?? TamanhoPadrao;

        if (p < 1)
            erros.Add(new ErroDeCampo("pagina", "A página começa em 1.", p.ToString()));

        if (t < 1)
            erros.Add(new ErroDeCampo("tamanho", "O tamanho da página começa em 1.", t.ToString()));
        else if (t > TamanhoMaximo)
            erros.Add(new ErroDeCampo(
                "tamanho",
                $"O tamanho máximo da página é {TamanhoMaximo}. Para levar a base inteira, use a exportação.",
                t.ToString()));

        return erros.Count > 0
            ? Resultado<Paginacao>.FalhaDeValidacao("A paginação pedida não é válida.", erros)
            : Resultado<Paginacao>.Ok(new Paginacao(p, t));
    }
}

/// <summary>Uma fatia de resultado, com o suficiente para a tela montar a barra de paginação.</summary>
/// <typeparam name="T">O que a listagem devolve.</typeparam>
/// <param name="Itens">As linhas desta página.</param>
/// <param name="Pagina">Qual página é esta.</param>
/// <param name="Tamanho">Quantas linhas cabem por página.</param>
/// <param name="Total">Quantas linhas existem ao todo, já respeitada a fronteira de acesso.</param>
public sealed record PaginaDe<T>(IReadOnlyList<T> Itens, int Pagina, int Tamanho, int Total)
{
    /// <summary>Quantas páginas o total ocupa.</summary>
    public int TotalDePaginas => Tamanho <= 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamanho);

    /// <summary>Existe página depois desta?</summary>
    public bool TemProxima => Pagina * Tamanho < Total;
}
