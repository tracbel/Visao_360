namespace Tracbel.Crm.Dominio.Processo;

/// <summary>Uma resposta de venda perdida como a regra do papel a enxerga.</summary>
/// <param name="Chave">O <c>SeqQuestionario</c> — a identidade da resposta.</param>
/// <param name="Formulario">O formulário (<see cref="FormulariosDaVendaPerdida"/>).</param>
/// <param name="Pessoa">A pessoa no Vórtice (<c>SeqPessoa</c>).</param>
/// <param name="Processo">O processo no Vórtice, quando o formulário aponta um.</param>
/// <param name="RegistradaEm">Quando foi preenchida (UTC).</param>
public sealed record RespostaNaRegraDoPapel(long Chave, string Formulario, long Pessoa, long? Processo, DateTime RegistradaEm);

/// <summary>O papel decidido para uma resposta.</summary>
/// <param name="Papel">Principal, complemento ou duplicata.</param>
/// <param name="ChaveDaPrincipal">A principal, quando não é principal.</param>
/// <param name="ParticipacaoNegada">
/// Se a principal tem um gêmeo no formulário <c>SEM_PARTICIPACAO</c>: a Tracbel não participou (decisão de 27/09/2026).
/// </param>
public sealed record PapelDecidido(PapelDaVendaPerdida Papel, long? ChaveDaPrincipal, bool ParticipacaoNegada);

/// <summary>
/// QUEM É PRINCIPAL, QUEM REPETE E QUEM DETALHA — a regra das decisões de 27/09/2026 (documento 52 §4), pura.
///
/// <list type="number">
/// <item><b>O <c>_JDE</c> gêmeo do antigo é duplicata</b> do antigo (300 pares medidos em 27/09/2026 para 296
/// respostas do <c>_JDE</c>).</item>
/// <item><b>No par <c>_FY25</c> × <c>SEM_PARTICIPACAO</c>, o FY25 é o principal</b> (15 pares), e a participação dele
/// passa a "Não" — é o que o gêmeo afirma.</item>
/// <item><b>Os <c>VP_*</c> acompanham o FY25</b> como complemento; o solto entra como principal.</item>
/// <item>Todo o resto é principal.</item>
/// </list>
///
/// <para><b>O par.</b> A mesma pessoa no mesmo processo, quando os dois formulários apontam processo; senão, a mesma
/// pessoa no mesmo dia de São Paulo — o mesmo evento preenchido nos dois formulários. Entre candidatos, vale o mais
/// próximo no tempo e, no empate, o menor <c>SeqQuestionario</c>: a regra dá a mesma resposta a cada rodada.</para>
/// </summary>
public static class RegraDoPapelDaVendaPerdida
{
    private static readonly TimeSpan FusoDeSaoPaulo = TimeSpan.FromHours(-3);

    /// <summary>Decide o papel de cada resposta.</summary>
    /// <param name="respostas">As respostas lidas — de todos os formulários.</param>
    public static IReadOnlyDictionary<long, PapelDecidido> Decidir(IReadOnlyCollection<RespostaNaRegraDoPapel> respostas)
    {
        var decididos = respostas.ToDictionary(r => r.Chave, _ => new PapelDecidido(PapelDaVendaPerdida.Principal, null, false));

        var antigos = Do(respostas, FormulariosDaVendaPerdida.Antigo);
        var fy25 = Do(respostas, FormulariosDaVendaPerdida.Fy25);

        foreach (var gemeo in Do(respostas, FormulariosDaVendaPerdida.Jde))
            if (Par(gemeo, antigos) is { } principal)
                decididos[gemeo.Chave] = new PapelDecidido(PapelDaVendaPerdida.Duplicata, principal.Chave, false);

        foreach (var semParticipacao in Do(respostas, FormulariosDaVendaPerdida.SemParticipacao))
        {
            if (Par(semParticipacao, fy25) is not { } principal) continue;
            decididos[semParticipacao.Chave] = new PapelDecidido(PapelDaVendaPerdida.Duplicata, principal.Chave, false);
            decididos[principal.Chave] = decididos[principal.Chave] with { ParticipacaoNegada = true };
        }

        foreach (var detalhe in respostas.Where(r => FormulariosDaVendaPerdida.Complementos.Contains(r.Formulario)))
            if (Par(detalhe, fy25) is { } principal)
                decididos[detalhe.Chave] = new PapelDecidido(PapelDaVendaPerdida.Complemento, principal.Chave, false);

        return decididos;
    }

    private static List<RespostaNaRegraDoPapel> Do(IEnumerable<RespostaNaRegraDoPapel> respostas, string formulario) =>
        respostas.Where(r => string.Equals(r.Formulario, formulario, StringComparison.Ordinal)).ToList();

    private static RespostaNaRegraDoPapel? Par(RespostaNaRegraDoPapel resposta, IEnumerable<RespostaNaRegraDoPapel> candidatas) =>
        candidatas
            .Where(c => c.Pessoa == resposta.Pessoa)
            .Where(c => c.Processo is { } p && resposta.Processo is { } q
                ? p == q
                : Dia(c.RegistradaEm) == Dia(resposta.RegistradaEm))
            .OrderBy(c => (c.RegistradaEm - resposta.RegistradaEm).Duration())
            .ThenBy(c => c.Chave)
            .FirstOrDefault();

    private static DateOnly Dia(DateTime utc) => DateOnly.FromDateTime(utc + FusoDeSaoPaulo);
}
