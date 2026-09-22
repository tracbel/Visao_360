namespace Tracbel.Crm.Dominio.Mercado;

/// <summary>
/// A ÁREA DE UMA CULTURA NUM GRUPO DE COMPARTILHAMENTO — o que entra na conta do parque.
/// </summary>
/// <param name="CulturaCodigo">O código da cultura no catálogo.</param>
/// <param name="CulturaNome">O nome, para a frase do tooltip.</param>
/// <param name="AreaHectares">A área da cultura no recorte; nula quando a fonte não divulgou.</param>
/// <param name="HectaresPorMaquina">A regra vigente da cultura para a categoria; nula sem regra.</param>
/// <param name="AnosDeRenovacao">A cada quantos anos a máquina é trocada; nula quando ninguém informou (D-P01).</param>
public sealed record AreaDaCulturaNoGrupo(
    string CulturaCodigo,
    string CulturaNome,
    decimal? AreaHectares,
    decimal? HectaresPorMaquina,
    decimal? AnosDeRenovacao = null);

/// <summary>
/// O PARQUE TEÓRICO DE UM GRUPO, com o porquê do número.
/// </summary>
/// <param name="Maquinas">As máquinas que a área comporta; nula quando falta área ou regra.</param>
/// <param name="AreaUtilHectares">A área que entrou na conta.</param>
/// <param name="CulturaDominante">O nome da cultura cujos parâmetros foram usados; nulo quando não há.</param>
/// <param name="Compartilhada">Os nomes das outras culturas do grupo, para o tooltip.</param>
/// <param name="DemandaAnual">Parque ÷ anos de renovação da dominante; nula sem parque ou sem ciclo informado.</param>
/// <param name="CulturaDominanteCodigo">O código da dominante — é por ele que quem chama a reencontra sem depender do nome.</param>
public sealed record ParqueDoGrupo(
    decimal? Maquinas,
    decimal? AreaUtilHectares,
    string? CulturaDominante,
    IReadOnlyList<string> Compartilhada,
    decimal? DemandaAnual = null,
    string? CulturaDominanteCodigo = null);

/// <summary>
/// O POTENCIAL ESTRUTURAL SEM CONTAR A MESMA TERRA DUAS VEZES (issue 160).
///
/// <para><b>O defeito que isto conserta.</b> O protótipo soma as seis culturas e divide cada área pelos
/// hectares por máquina dela. Em São Paulo isso conta terra que não existe: o <b>milho safrinha é
/// plantado depois da soja, na mesma terra</b>, e boa parte do amendoim entra em reforma de canavial
/// (achado C-02 do documento 49). O mesmo talhão vira duas áreas, e o parque teórico sai inflado
/// justamente onde a rotação é mais comum.</para>
///
/// <para><b>A regra, proposta em D-IM-01:</b> a área útil de um grupo é a <b>maior</b> área entre as
/// culturas dele, com os parâmetros da cultura dominante — a dona dessa maior área. A intuição é
/// física: o talhão é um só, e a máquina que passa nele é dimensionada pela cultura que ocupa mais.</para>
///
/// <para><b>Sem grupo, nada muda.</b> Cultura fora de qualquer grupo soma a área dela normalmente. É
/// neutralidade deliberada: enquanto o administrador não configurar um grupo, o número é exatamente o
/// que era — e o critério de aceite da issue diz isso ("sem grupo, é igual").</para>
/// </summary>
public static class PotencialEstrutural
{
    /// <summary>
    /// O parque teórico de um grupo de culturas que compartilham a mesma terra e a mesma máquina.
    ///
    /// <para><b>A dominante é a de maior área</b>, e é a regra dela que divide — não uma média, que
    /// seria uma máquina que não existe. Empate fica com a primeira da lista, que vem ordenada pelo
    /// chamador.</para>
    ///
    /// <para><b>Cultura sem área não atrapalha:</b> ela é ignorada, e o grupo vale pelas que têm. Grupo
    /// inteiro sem área devolve nulo com a área útil nula — e não zero, que diria "não há potencial"
    /// quando o certo é "não se sabe".</para>
    ///
    /// <para><b>O ciclo de renovação também é parâmetro da dominante</b> (issue 72). É o mesmo talhão e a
    /// mesma máquina: quem dimensiona o parque dimensiona a troca. Sem ciclo informado, o parque sai e a
    /// demanda anual fica vazia — nunca com um ciclo inventado (D-P01).</para>
    /// </summary>
    /// <param name="culturas">As culturas do grupo, com área e regra.</param>
    public static ParqueDoGrupo Parque(IReadOnlyList<AreaDaCulturaNoGrupo> culturas)
    {
        // ÁREA ZERO NÃO É ÁREA AUSENTE. Zero é o IBGE dizendo "não se planta café aqui", e a resposta
        // certa é zero máquina; nulo é "não se sabe", e aí não há resposta. Só o nulo sai da conta.
        var conhecidas = culturas.Where(c => c.AreaHectares is not null).ToList();

        if (conhecidas.Count == 0)
            return new ParqueDoGrupo(null, null, null, []);

        // A DOMINANTE DECIDE O PARÂMETRO. MaxBy devolve o primeiro em caso de empate, e a lista chega
        // ordenada por quem chama — o resultado não depende da ordem do banco.
        var dominante = conhecidas.MaxBy(c => c.AreaHectares!.Value)!;
        var areaUtil = dominante.AreaHectares!.Value;

        // "COMPARTILHADA COM" É SOBRE TERRA, e quem tem zero hectare não divide terra com ninguém.
        var outras = conhecidas
            .Where(c => c.CulturaCodigo != dominante.CulturaCodigo && c.AreaHectares is > 0)
            .Select(c => c.CulturaNome)
            .ToList();

        var maquinas = dominante.HectaresPorMaquina is > 0 ? areaUtil / dominante.HectaresPorMaquina!.Value : (decimal?)null;
        var demanda = maquinas is { } parque && dominante.AnosDeRenovacao is > 0
            ? parque / dominante.AnosDeRenovacao!.Value
            : (decimal?)null;

        return new ParqueDoGrupo(maquinas, areaUtil, dominante.CulturaNome, outras, demanda, dominante.CulturaCodigo);
    }

    /// <summary>
    /// A frase do tooltip: com quem esta área é compartilhada.
    ///
    /// <para>Vazia quando não há compartilhamento — a tela não mostra rodapé para dizer "nada".</para>
    /// </summary>
    /// <param name="parque">O parque do grupo.</param>
    public static string FraseDoCompartilhamento(ParqueDoGrupo parque) =>
        parque.Compartilhada.Count == 0
            ? string.Empty
            : $"área compartilhada com {string.Join(", ", parque.Compartilhada)} — a mesma terra, contada uma vez";

    /// <summary>
    /// O PARQUE DE UM RECORTE INTEIRO: a soma dos grupos e das culturas soltas.
    ///
    /// <para><b>Nunca maior que a soma simples.</b> É o critério de aceite da issue, e sai de graça da
    /// regra: a área útil de um grupo é a maior das áreas dele, que é menor ou igual à soma delas.</para>
    /// </summary>
    /// <param name="grupos">Os grupos configurados, cada um com as culturas dele.</param>
    /// <param name="soltas">As culturas que não estão em grupo nenhum.</param>
    public static decimal? ParqueDoRecorte(
        IReadOnlyList<IReadOnlyList<AreaDaCulturaNoGrupo>> grupos, IReadOnlyList<AreaDaCulturaNoGrupo> soltas)
    {
        var parcelas = grupos
            .Select(g => Parque(g).Maquinas)
            .Concat(soltas.Select(c => Parque([c]).Maquinas))
            .Where(m => m is not null)
            .Select(m => m!.Value)
            .ToList();

        return parcelas.Count == 0 ? null : parcelas.Sum();
    }
}
