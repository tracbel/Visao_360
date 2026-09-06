namespace Tracbel.Crm.Dominio.Workflow;

/// <summary>Como a atribuição foi decidida. Responde "por que essa tarefa é minha?".</summary>
public enum OrigemAtribuicao
{
    /// <summary>Uma regra de automação resolveu o destinatário.</summary>
    Regra = 0,

    /// <summary>Alguém atribuiu à mão.</summary>
    Manual = 1,

    /// <summary>Veio pela hierarquia de vendas.</summary>
    Hierarquia = 2,

    /// <summary>Veio pelo responsável da carteira.</summary>
    Carteira = 3,

    /// <summary>Distribuição circular dentro de uma equipe.</summary>
    RoundRobin = 4
}

/// <summary>Quem vai receber o efeito de uma regra, e por quê.</summary>
/// <param name="UsuarioId">O usuário resolvido.</param>
/// <param name="NomeExibicao">Nome curto, para a mensagem de log.</param>
/// <param name="Origem">Como chegamos até ele.</param>
public sealed record Destinatario(long UsuarioId, string NomeExibicao, OrigemAtribuicao Origem);

/// <summary>
/// Tudo que uma regra precisa saber para decidir. É passado para a condição, para o
/// resolvedor de destinatário e para o efeito.
/// </summary>
public sealed record ContextoRegra
{
    /// <summary>O evento que está sendo processado.</summary>
    public required string Evento { get; init; }

    /// <summary>Empresa dona do contexto.</summary>
    public required int EmpresaId { get; init; }

    /// <summary>Quem executou a ação que gerou o evento.</summary>
    public required long UsuarioExecutorId { get; init; }

    /// <summary>A regra sendo avaliada. Preenchida pelo motor, uma por vez.</summary>
    public Regra? Regra { get; init; }

    /// <summary>Processo envolvido.</summary>
    public long? ProcessoId { get; init; }

    /// <summary>Número legível do processo, para as mensagens de log.</summary>
    public long? ProcessoNumero { get; init; }

    /// <summary>Proprietário do processo.</summary>
    public long? ProcessoProprietarioId { get; init; }

    /// <summary>Valor estimado, quando existe. Usado em condições.</summary>
    public decimal? ProcessoValorEstimado { get; init; }

    /// <summary>Cliente envolvido — o termo do documento 15, nunca "conta".</summary>
    public long? ClienteId { get; init; }

    /// <summary>Carteira envolvida.</summary>
    public long? CarteiraId { get; init; }

    /// <summary>Responsável pela carteira.</summary>
    public long? CarteiraResponsavelId { get; init; }

    /// <summary>Tarefa que foi concluída.</summary>
    public long? TarefaId { get; init; }

    /// <summary>Atividade que registrou a conclusão.</summary>
    public long? InteracaoId { get; init; }

    /// <summary>Resultado escolhido pelo usuário — o desfecho que move a fase.</summary>
    public int? ResultadoId { get; init; }

    /// <summary>Tipo de fluxo do processo.</summary>
    public int? TipoProcessoId { get; init; }

    /// <summary>Linha de negócio, quando aplicável.</summary>
    public int? LinhaNegocioId { get; init; }

    /// <summary>
    /// Variáveis extras para a expressão de condição, por nome.
    /// Permite que um caso de uso enriqueça o contexto sem alterar esta classe.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Variaveis { get; init; }
        = new Dictionary<string, object?>();

    /// <summary>
    /// Resolve uma referência textual a usuário, usada pelas estratégias de destinatário.
    /// Ex.: <c>"processo.Proprietario"</c>, <c>"carteira.Responsavel"</c>, <c>"quemExecutou"</c>.
    /// </summary>
    /// <returns>O identificador do usuário, ou nulo se a referência não resolver.</returns>
    public long? ResolverReferenciaUsuario(string referencia) => referencia.Trim() switch
    {
        "processo.Proprietario" => ProcessoProprietarioId,
        "carteira.Responsavel" => CarteiraResponsavelId,
        "quemExecutou" => UsuarioExecutorId,
        _ => null
    };
}

/// <summary>O que um efeito produziu.</summary>
public sealed record ResultadoEfeito
{
    private ResultadoEfeito() { }

    /// <summary>Verdadeiro quando o efeito produziu o que devia.</summary>
    public bool Aplicou { get; private init; }

    /// <summary>Explicação em português — vai direto para o log de execução.</summary>
    public string Motivo { get; private init; } = string.Empty;

    /// <summary>Tarefa criada, quando o efeito foi CriarTarefa.</summary>
    public long? TarefaCriadaId { get; private init; }

    /// <summary>Destinatário resolvido, quando houve.</summary>
    public long? DestinatarioId { get; private init; }

    /// <summary>O efeito produziu o resultado esperado.</summary>
    public static ResultadoEfeito Aplicado(string motivo, long? tarefaCriadaId = null, long? destinatarioId = null)
        => new() { Aplicou = true, Motivo = motivo, TarefaCriadaId = tarefaCriadaId, DestinatarioId = destinatarioId };

    /// <summary>
    /// O efeito não se aplicou — e isso é INFORMAÇÃO A REGISTRAR, não erro a engolir.
    /// É esta decisão de desenho que torna o defeito 899/900 impossível.
    /// </summary>
    public static ResultadoEfeito NaoAplicado(string motivo) => new() { Aplicou = false, Motivo = motivo };
}

/// <summary>Como uma condição foi avaliada, e por quê.</summary>
/// <param name="Verdadeira">Se a condição passou.</param>
/// <param name="Explicacao">Em português, o motivo — vai para o log.</param>
public sealed record AvaliacaoCondicao(bool Verdadeira, string Explicacao);

/// <summary>Relatório do que aconteceu quando um evento foi processado.</summary>
public sealed class RelatorioExecucao(Guid correlacaoId)
{
    private readonly List<string> _falhas = [];
    private readonly List<string> _disparos = [];

    /// <summary>Identificador que agrupa todas as regras deste evento.</summary>
    public Guid CorrelacaoId { get; } = correlacaoId;

    /// <summary>Códigos das regras que dispararam.</summary>
    public IReadOnlyList<string> Disparos => _disparos;

    /// <summary>Falhas encontradas, no formato "codigo: motivo".</summary>
    public IReadOnlyList<string> Falhas => _falhas;

    /// <summary>Quantas regras foram avaliadas ao todo.</summary>
    public int TotalAvaliadas { get; private set; }

    /// <summary>Registra o desfecho de uma regra.</summary>
    public void Registrar(string codigoRegra, ResultadoEfeito resultado)
    {
        TotalAvaliadas++;
        if (resultado.Aplicou) _disparos.Add(codigoRegra);
    }

    /// <summary>Registra que uma regra falhou.</summary>
    public void RegistrarFalha(string codigoRegra, string motivo)
    {
        TotalAvaliadas++;
        _falhas.Add($"{codigoRegra}: {motivo}");
    }

    /// <summary>Registra uma regra avaliada que não produziu efeito nem falha.</summary>
    public void RegistrarSemEfeito() => TotalAvaliadas++;
}
