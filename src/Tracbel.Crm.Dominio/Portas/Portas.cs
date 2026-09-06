using Tracbel.Crm.Dominio.Workflow;

namespace Tracbel.Crm.Dominio.Portas;

/// <summary>
/// PORTAS — o que o domínio EXIGE do mundo externo.
///
/// O domínio declara a interface; a Infraestrutura implementa. É esta inversão que permite
/// testar toda a regra de negócio sem banco, sem HTTP e sem mock complicado — e é o que o
/// teste de arquitetura protege.
/// </summary>
public interface IRepositorioRegras
{
    /// <summary>
    /// Devolve as regras candidatas a um gatilho, já filtradas pelo que é indexável.
    /// A condição fina é avaliada pelo motor, não aqui.
    /// </summary>
    Task<IReadOnlyList<Regra>> ObterPorGatilhoAsync(
        string evento, int? resultadoId, int? tipoProcessoId, CancellationToken ct);
}

/// <summary>
/// Avalia a expressão de condição de uma regra.
///
/// A implementação devolve, junto com o veredito, uma EXPLICAÇÃO EM PORTUGUÊS. Sem ela,
/// o log de execução vira "não disparou" sem motivo — que é exatamente a cegueira do
/// sistema legado.
/// </summary>
public interface IAvaliadorCondicao
{
    /// <summary>Avalia a expressão contra o contexto do evento.</summary>
    Task<AvaliacaoCondicao> AvaliarAsync(string expressao, ContextoRegra contexto, CancellationToken ct);
}

/// <summary>
/// Grava o registro de execução de regra.
///
/// [V] É a peça que faltava no Vórtice e que deixou a queda de 68% para 3% passar cinco
/// meses despercebida.
/// </summary>
public interface IRegistroExecucaoRegra
{
    /// <summary>Persiste uma linha de <c>wf.RegraExecucao</c>.</summary>
    Task GravarAsync(ExecucaoRegra execucao, CancellationToken ct);
}

/// <summary>Resolve a expressão de destinatário de uma regra.</summary>
public interface IResolvedorDestinatario
{
    /// <summary>Devolve quem recebe o efeito, ou nulo se a expressão não resolver ninguém.</summary>
    Task<Destinatario?> ResolverAsync(string expressao, ContextoRegra contexto, CancellationToken ct);
}

/// <summary>
/// Uma estratégia de resolução de destinatário. Cada implementação entende UMA forma de
/// dizer "para quem vai".
/// </summary>
public interface IEstrategiaDestinatario
{
    /// <summary>A expressão que esta estratégia sabe interpretar.</summary>
    bool Reconhece(string expressao);

    /// <summary>Resolve, ou devolve nulo.</summary>
    Task<Destinatario?> ResolverAsync(string expressao, ContextoRegra contexto, CancellationToken ct);
}

/// <summary>Hierarquia de vendas, materializada como closure table.</summary>
public interface IHierarquiaVendas
{
    /// <summary>
    /// O gestor direto de um usuário.
    ///
    /// [V] No Vórtice a hierarquia é um ponteiro de UM nível só
    /// (<c>IV_VENDEDOR.SeqUsuarioLider</c>), sem closure e sem reatribuição — por isso
    /// trocar o usuário de uma gerente esvazia o relatório de carteira dela.
    /// </summary>
    Task<Destinatario?> ObterGestorDiretoAsync(long usuarioId, CancellationToken ct);

    /// <summary>Todos os subordinados, em qualquer profundidade.</summary>
    Task<IReadOnlyList<long>> ObterSubordinadosAsync(long usuarioId, CancellationToken ct);
}

/// <summary>Calendário de dias úteis da Tracbel, com feriados.</summary>
public interface ICalendarioUtil
{
    /// <summary>Soma dias úteis a uma data, pulando fim de semana e feriado.</summary>
    DateTime AdicionarDiasUteis(DateTime inicio, int diasUteis);

    /// <summary>Horas úteis decorridas entre dois instantes. Alimenta o SLA por estágio.</summary>
    decimal HorasUteisEntre(DateTime inicio, DateTime fim);
}
