using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Processo;

/// <summary>Em que estado a tarefa está.</summary>
public enum SituacaoDaTarefa
{
    /// <summary>Ainda não começou.</summary>
    Pendente = 0,

    /// <summary>Começou e não terminou.</summary>
    EmAndamento = 1,

    /// <summary>Terminou com desfecho registrado.</summary>
    Concluida = 2,

    /// <summary>Cancelada com motivo.</summary>
    Cancelada = 3,

    /// <summary>Passada a outra pessoa.</summary>
    Reatribuida = 4
}

/// <summary>Por que esta tarefa é desta pessoa.</summary>
public enum OrigemDaAtribuicao
{
    /// <summary>Uma regra de automação atribuiu.</summary>
    Regra = 0,

    /// <summary>Alguém atribuiu na tela.</summary>
    Manual = 1,

    /// <summary>Veio pela hierarquia comercial.</summary>
    Hierarquia = 2,

    /// <summary>Veio pelo responsável da carteira.</summary>
    Carteira = 3,

    /// <summary>Veio por rodízio dentro da equipe.</summary>
    Rodizio = 4
}

/// <summary>
/// O compromisso na agenda de alguém.
///
/// [V] A agenda do Vórtice guarda o LOGIN do vendedor num campo de texto, não o
/// identificador: trocar o login de alguém quebra em silêncio todo o histórico. Aqui é
/// sempre chave estrangeira.
///
/// A coluna de origem da atribuição responde "por que essa tarefa é minha?" sem investigação
/// — é o caso de suporte mais comum do legado.
/// </summary>
public sealed class Tarefa : EntidadeBase
{
    private Tarefa() { }

    /// <summary>Filial dona da tarefa.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O processo. Nulo é tarefa avulsa.</summary>
    public long? ProcessoId { get; private set; }

    /// <summary>O cliente. Preenchido mesmo sem processo, porque a agenda mostra as duas.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>O contato com quem o compromisso é.</summary>
    public long? ContatoId { get; private set; }

    /// <summary>Que tipo de tarefa é esta.</summary>
    public int TipoTarefaId { get; private set; }

    /// <summary>Assunto, o que aparece na agenda.</summary>
    public string Assunto { get; private set; } = default!;

    /// <summary>Detalhe livre.</summary>
    public string? Detalhe { get; private set; }

    /// <summary>Quem tem que fazer.</summary>
    public long ResponsavelId { get; private set; }

    /// <summary>Como a atribuição foi decidida.</summary>
    public OrigemDaAtribuicao OrigemAtribuicao { get; private set; } = OrigemDaAtribuicao.Manual;

    /// <summary>Quando está agendada (UTC).</summary>
    public DateTime AgendadaPara { get; private set; }

    /// <summary>Prazo limite (UTC).</summary>
    public DateTime? PrazoLimite { get; private set; }

    /// <summary>Prioridade, de 1 (alta) a 5 (baixa).</summary>
    public short Prioridade { get; private set; } = 3;

    /// <summary>Em que estado a tarefa está.</summary>
    public SituacaoDaTarefa Situacao { get; private set; } = SituacaoDaTarefa.Pendente;

    /// <summary>Quando foi concluída (UTC).</summary>
    public DateTime? ConcluidaEm { get; private set; }

    /// <summary>Quem concluiu.</summary>
    public long? ConcluidaPorId { get; private set; }

    /// <summary>Qual desfecho foi registrado na conclusão.</summary>
    public int? ResultadoId { get; private set; }

    /// <summary>
    /// A interação que registrou a conclusão. Junto com o ponteiro inverso na interação forma
    /// o duplo ponteiro que o Vórtice acertou.
    /// </summary>
    public long? InteracaoConclusaoId { get; private set; }

    /// <summary>Qual interação disparou a regra que gerou esta tarefa.</summary>
    public long? InteracaoOrigemId { get; private set; }

    /// <summary>Agenda uma tarefa.</summary>
    /// <param name="empresaId">Filial dona.</param>
    /// <param name="tipoTarefaId">Que tipo de tarefa é esta.</param>
    /// <param name="assunto">O que aparece na agenda.</param>
    /// <param name="responsavelId">Quem tem que fazer.</param>
    /// <param name="agendadaParaUtc">Quando está agendada.</param>
    /// <param name="origemAtribuicao">Por que essa tarefa é dessa pessoa.</param>
    /// <param name="criadoPorId">Quem criou.</param>
    /// <param name="processoId">O processo, quando a tarefa pertence a um.</param>
    /// <param name="clienteId">O cliente. A agenda mostra os dois.</param>
    /// <param name="contatoId">O contato com quem o compromisso é.</param>
    /// <param name="detalhe">Detalhe livre.</param>
    /// <param name="prazoLimite">Prazo limite, quando a origem declara um.</param>
    /// <param name="prioridade">Prioridade, de 1 (alta) a 5 (baixa).</param>
    /// <param name="criadaEmUtc">
    /// Quando a tarefa nasceu na ORIGEM. A carga do sistema legado precisa preservá-la: sem ela,
    /// toda tarefa migrada teria nascido hoje e o passivo de agenda deixaria de ter idade.
    /// </param>
    public static Tarefa Agendar(
        int empresaId,
        int tipoTarefaId,
        string assunto,
        long responsavelId,
        DateTime agendadaParaUtc,
        OrigemDaAtribuicao origemAtribuicao,
        long criadoPorId,
        long? processoId = null,
        long? clienteId = null,
        long? contatoId = null,
        string? detalhe = null,
        DateTime? prazoLimite = null,
        short prioridade = 3,
        DateTime? criadaEmUtc = null)
    {
        if (string.IsNullOrWhiteSpace(assunto))
            throw new RegraDeNegocioViolada("Tarefa sem assunto não existe.");

        if (agendadaParaUtc.Kind is not DateTimeKind.Utc)
            throw new RegraDeNegocioViolada("A data da tarefa precisa estar em UTC.");

        if (prioridade is < 1 or > 5)
            throw new RegraDeNegocioViolada("A prioridade da tarefa vai de 1 (alta) a 5 (baixa).");

        return new Tarefa
        {
            EmpresaId = empresaId,
            TipoTarefaId = tipoTarefaId,
            ProcessoId = processoId,
            ClienteId = clienteId,
            ContatoId = contatoId,
            Assunto = assunto.Trim(),
            Detalhe = detalhe,
            ResponsavelId = responsavelId,
            AgendadaPara = agendadaParaUtc,
            PrazoLimite = prazoLimite,
            Prioridade = prioridade,
            OrigemAtribuicao = origemAtribuicao,
            CriadoEm = criadaEmUtc ?? DateTime.UtcNow,
            CriadoPorId = criadoPorId
        };
    }

    /// <summary>Conclui a tarefa. Sem desfecho e sem quem concluiu, não conclui.</summary>
    /// <param name="usuarioId">Quem concluiu.</param>
    /// <param name="resultadoId">O desfecho registrado.</param>
    /// <param name="interacaoConclusaoId">A interação que registrou a conclusão.</param>
    /// <param name="concluidaEmUtc">
    /// Quando a conclusão aconteceu. Nulo é agora — e é o caso da tela. A carga do sistema
    /// legado informa a data real, porque uma conclusão de março carimbada com a data de hoje
    /// destruiria toda medição de prazo.
    /// </param>
    public void Concluir(
        long usuarioId, int resultadoId, long? interacaoConclusaoId = null, DateTime? concluidaEmUtc = null)
    {
        if (Situacao is SituacaoDaTarefa.Concluida)
            throw new RegraDeNegocioViolada("Tarefa já concluída não se conclui de novo.");

        Situacao = SituacaoDaTarefa.Concluida;
        ConcluidaEm = concluidaEmUtc ?? DateTime.UtcNow;
        ConcluidaPorId = usuarioId;
        ResultadoId = resultadoId;
        InteracaoConclusaoId = interacaoConclusaoId;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Cancela a tarefa. [V] no sistema de origem uma agenda "não realizada" some da lista e
    /// nunca é fechada — é assim que 35.662 tarefas pendentes se acumularam, a mais antiga de
    /// 2012.
    /// </summary>
    /// <param name="usuarioId">Quem cancelou.</param>
    public void Cancelar(long usuarioId)
    {
        if (Situacao is SituacaoDaTarefa.Concluida)
            throw new RegraDeNegocioViolada("Tarefa já concluída não se cancela.");

        Situacao = SituacaoDaTarefa.Cancelada;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Reprograma a tarefa para outra data.
    ///
    /// <para>[V] 27,6% das agendas do período já foram empurradas pelo menos uma vez, e a origem
    /// preserva a data original numa coluna própria — um acerto que vale copiar quando a
    /// reprogramação virar operação de tela. Aqui o método existe para a RECONCILIAÇÃO da carga:
    /// uma tarefa que foi reagendada na origem entre duas execuções precisa mudar de dia aqui
    /// também, senão a agenda migrada envelhece em silêncio.</para>
    /// </summary>
    /// <param name="agendadaParaUtc">A nova data.</param>
    /// <param name="prazoLimite">O novo prazo limite, quando há.</param>
    /// <param name="usuarioId">Quem reprogramou.</param>
    public void Reprogramar(DateTime agendadaParaUtc, DateTime? prazoLimite, long usuarioId)
    {
        if (agendadaParaUtc.Kind is not DateTimeKind.Utc)
            throw new RegraDeNegocioViolada("A data da tarefa precisa estar em UTC.");

        if (AgendadaPara == agendadaParaUtc && PrazoLimite == prazoLimite) return;

        AgendadaPara = agendadaParaUtc;
        PrazoLimite = prazoLimite;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>Liga a tarefa à interação que a gerou, e à regra que a criou.</summary>
    /// <param name="interacaoOrigemId">A interação que disparou a criação desta tarefa.</param>
    public void RegistrarOrigem(long? interacaoOrigemId)
    {
        if (interacaoOrigemId is not null) InteracaoOrigemId = interacaoOrigemId;
    }
}
