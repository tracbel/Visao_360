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

    /// <summary>O que a tarefa guarda, no formato em que a origem é comparada (documento 52 §12).</summary>
    public RetratoDaTarefaDaOrigem RetratoDaOrigem => new(
        EmpresaId, ProcessoId, ClienteId, TipoTarefaId, Assunto, ResponsavelId, OrigemAtribuicao, AgendadaPara, PrazoLimite,
        Prioridade, Situacao, ConcluidaEm, ConcluidaPorId, ResultadoId, CriadoEm);

    /// <summary>
    /// Registra a tarefa que o Vórtice declara — a onda 2 da rotina <c>PROCESSOS_VORTICE</c> (documento 52 §12).
    ///
    /// <para><b>Por que não <see cref="Agendar"/> seguido de <see cref="Concluir"/>.</b> A agenda da origem ANDA entre
    /// duas rodadas — a tarefa de ontem foi concluída, empurrada ou reaberta —, e <c>Concluir</c> recusa a segunda
    /// conclusão, como deve recusar na tela. A rotina não conclui nada: ela espelha o que a origem já concluiu, com a
    /// data, quem concluiu e o desfecho que a restrição do banco exige.</para>
    /// </summary>
    /// <param name="retrato">O estado declarado.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    public static Tarefa DaOrigem(RetratoDaTarefaDaOrigem retrato, long criadoPorId)
    {
        var tarefa = new Tarefa { CriadoPorId = criadoPorId };
        tarefa.AplicarDaOrigem(Normalizar(retrato));
        return tarefa;
    }

    /// <summary>Acompanha a origem. Devolve se alguma coisa mudou — relida igual, não muda nada.</summary>
    /// <param name="retrato">O estado declarado nesta rodada.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool AtualizarDaOrigem(RetratoDaTarefaDaOrigem retrato, long usuarioId)
    {
        var normalizado = Normalizar(retrato);
        if (RetratoDaOrigem == normalizado) return false;

        AplicarDaOrigem(normalizado);
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>A tarefa voltou à origem depois de sair: deixa de estar excluída. Devolve se mudou.</summary>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool RestaurarDaOrigem(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// FECHA O DUPLO PONTEIRO com as interações que a origem declara: a que gerou a tarefa e a que a concluiu. A de
    /// conclusão só vale para a tarefa concluída — pendente não tem quem a tenha concluído. Devolve se mudou.
    /// </summary>
    /// <param name="interacaoOrigemId">A interação que gerou a tarefa, quando ela está no CRM.</param>
    /// <param name="interacaoConclusaoId">A interação que concluiu a tarefa, quando ela está no CRM.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool LigarInteracoes(long? interacaoOrigemId, long? interacaoConclusaoId, long usuarioId)
    {
        var conclusao = Situacao is SituacaoDaTarefa.Concluida ? interacaoConclusaoId : null;
        if (InteracaoOrigemId == interacaoOrigemId && InteracaoConclusaoId == conclusao) return false;

        InteracaoOrigemId = interacaoOrigemId;
        InteracaoConclusaoId = conclusao;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// O retrato com as datas no milissegundo e o assunto aparado — o que as colunas <c>datetime2(3)</c> guardam. Sem
    /// isso a data relida do Vórtice (passo de 1/300 s) nunca seria igual à gravada.
    /// </summary>
    /// <param name="r">O retrato como veio.</param>
    public static RetratoDaTarefaDaOrigem Normalizar(RetratoDaTarefaDaOrigem r) => r with
    {
        Assunto = (r.Assunto ?? string.Empty).Trim(),
        AgendadaPara = EstagioDoProcesso.NoMilissegundo(r.AgendadaPara),
        PrazoLimite = EstagioDoProcesso.NoMilissegundo(r.PrazoLimite),
        ConcluidaEm = EstagioDoProcesso.NoMilissegundo(r.ConcluidaEm),
        CriadaEm = EstagioDoProcesso.NoMilissegundo(r.CriadaEm)
    };

    private void AplicarDaOrigem(RetratoDaTarefaDaOrigem r)
    {
        // AS MESMAS REGRAS DE Agendar E DO BANCO (CK_Tarefa_Prioridade e CK_Tarefa_Conclusao): concluída tem data, quem
        // concluiu e desfecho, sem exceção — a origem que marca "realizada" sem os três fica pendente, quem decide é a rotina.
        if (r.Assunto.Length == 0)
            throw new RegraDeNegocioViolada("Tarefa sem assunto não existe.");

        if (r.AgendadaPara.Kind is not DateTimeKind.Utc)
            throw new RegraDeNegocioViolada("A data da tarefa precisa estar em UTC.");

        if (r.Prioridade is < 1 or > 5)
            throw new RegraDeNegocioViolada("A prioridade da tarefa vai de 1 (alta) a 5 (baixa).");

        if (r.Situacao is SituacaoDaTarefa.Concluida && (r.ConcluidaEm is null || r.ConcluidaPorId is null || r.ResultadoId is null))
            throw new RegraDeNegocioViolada("Tarefa concluída tem data, quem concluiu e desfecho. Sem os três, ela é pendente.");

        EmpresaId = r.EmpresaId;
        ProcessoId = r.ProcessoId;
        ClienteId = r.ClienteId;
        TipoTarefaId = r.TipoTarefaId;
        Assunto = r.Assunto;
        ResponsavelId = r.ResponsavelId;
        OrigemAtribuicao = r.OrigemAtribuicao;
        AgendadaPara = r.AgendadaPara;
        PrazoLimite = r.PrazoLimite;
        Prioridade = r.Prioridade;
        Situacao = r.Situacao;
        ConcluidaEm = r.ConcluidaEm;
        ConcluidaPorId = r.ConcluidaPorId;
        ResultadoId = r.ResultadoId;
        CriadoEm = r.CriadaEm;

        // A TAREFA QUE A ORIGEM REABRIU não tem mais quem a concluiu: o ponteiro de conclusão vai junto.
        if (Situacao is not SituacaoDaTarefa.Concluida) InteracaoConclusaoId = null;
    }
}

/// <summary>
/// O ESTADO QUE O VÓRTICE DECLARA PARA UMA TAREFA DA ONDA 2 — tudo o que a rotina compara para decidir se a tarefa
/// mudou (documento 52 §12). As datas chegam cortadas no milissegundo (<see cref="Tarefa.Normalizar"/>).
/// </summary>
/// <param name="EmpresaId">A filial — a do processo, e não a do cliente.</param>
/// <param name="ProcessoId">O processo do CRM. A onda 2 não traz tarefa avulsa (decisão P6).</param>
/// <param name="ClienteId">O cliente do processo.</param>
/// <param name="TipoTarefaId">A ação, já no catálogo do CRM; sem ação, o tipo "não informada".</param>
/// <param name="Assunto">O nome da ação no catálogo — o texto livre da agenda não entra (decisão P1).</param>
/// <param name="ResponsavelId">A conta de quem tem que fazer; sem conta, o dono do processo (decisão P2).</param>
/// <param name="OrigemAtribuicao">Regra (agendamento automático) ou manual.</param>
/// <param name="AgendadaPara">Quando está agendada (UTC).</param>
/// <param name="PrazoLimite">O prazo limite (UTC), quando a origem declara.</param>
/// <param name="Prioridade">De 1 (alta) a 5 (baixa).</param>
/// <param name="Situacao">Pendente ou concluída.</param>
/// <param name="ConcluidaEm">Quando foi concluída (UTC).</param>
/// <param name="ConcluidaPorId">Quem concluiu; sem conta, o dono do processo (decisão P2).</param>
/// <param name="ResultadoId">O desfecho registrado, já no catálogo do CRM.</param>
/// <param name="CriadaEm">Quando a tarefa nasceu na origem (UTC).</param>
public sealed record RetratoDaTarefaDaOrigem(
    int EmpresaId,
    long? ProcessoId,
    long? ClienteId,
    int TipoTarefaId,
    string Assunto,
    long ResponsavelId,
    OrigemDaAtribuicao OrigemAtribuicao,
    DateTime AgendadaPara,
    DateTime? PrazoLimite,
    short Prioridade,
    SituacaoDaTarefa Situacao,
    DateTime? ConcluidaEm,
    long? ConcluidaPorId,
    int? ResultadoId,
    DateTime CriadaEm);
