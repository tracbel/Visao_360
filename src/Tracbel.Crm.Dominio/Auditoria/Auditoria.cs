namespace Tracbel.Crm.Dominio.Auditoria;

/// <summary>
/// Quais campos se audita — a lista de adesão explícita.
///
/// É esta tabela que impede os 39 milhões de linhas de log. [V] No Vórtice audita-se tudo, e
/// por isso o log ocupa 42% do banco; 96,8% de uma das tabelas de log são recarimbos de um
/// job de cobrança, 617 linhas por processo. [SF] O Salesforce limita a 20 campos por objeto
/// e 18 meses, e essa é a lição.
///
/// Sem linha aqui, não há auditoria.
/// </summary>
public sealed class CampoAuditado
{
    private CampoAuditado() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>Nome da entidade auditada.</summary>
    public string Entidade { get; private set; } = default!;

    /// <summary>Nome do campo auditado.</summary>
    public string Campo { get; private set; } = default!;

    /// <summary>Por quanto tempo a alteração fica disponível para consulta.</summary>
    public short RetencaoMeses { get; private set; } = 18;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Declara que um campo passa a ser auditado.</summary>
    public static CampoAuditado Criar(string entidade, string campo, short retencaoMeses = 18) => new()
    {
        Entidade = entidade,
        Campo = campo,
        RetencaoMeses = retencaoMeses
    };
}

/// <summary>
/// Quem mudou o quê, de quanto para quanto.
///
/// Fato imutável, particionado por data e com retenção declarada na própria migração que a
/// cria. Substitui as 22 tabelas de log clonadas por assunto do Vórtice, que somam 43,7
/// milhões de linhas e nunca foram expurgadas.
/// </summary>
public sealed class AlteracaoDeCampo
{
    private AlteracaoDeCampo() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>Filial dona do registro alterado.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Nome da entidade alterada.</summary>
    public string Entidade { get; private set; } = default!;

    /// <summary>Identificador interno do registro alterado.</summary>
    public long RegistroId { get; private set; }

    /// <summary>Nome do campo alterado.</summary>
    public string Campo { get; private set; } = default!;

    /// <summary>Como estava.</summary>
    public string? ValorAnterior { get; private set; }

    /// <summary>Como ficou.</summary>
    public string? ValorNovo { get; private set; }

    /// <summary>Quando mudou (UTC). É a coluna de particionamento.</summary>
    public DateTime AlteradoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quem mudou. Sempre identificador, nunca texto.</summary>
    public long AlteradoPorId { get; private set; }

    /// <summary>Correlaciona com a requisição que causou a mudança.</summary>
    public Guid? CorrelacaoId { get; private set; }

    /// <summary>Registra uma alteração de campo.</summary>
    public static AlteracaoDeCampo Registrar(
        int empresaId,
        string entidade,
        long registroId,
        string campo,
        string? valorAnterior,
        string? valorNovo,
        long alteradoPorId) => new()
    {
        EmpresaId = empresaId,
        Entidade = entidade,
        RegistroId = registroId,
        Campo = campo,
        ValorAnterior = valorAnterior,
        ValorNovo = valorNovo,
        AlteradoPorId = alteradoPorId
    };
}

/// <summary>O que aconteceu no acesso.</summary>
public enum TipoDeEventoDeAcesso
{
    /// <summary>Entrou.</summary>
    Login = 0,

    /// <summary>Tentou entrar e não conseguiu.</summary>
    LoginFalhou = 1,

    /// <summary>Saiu.</summary>
    Logout = 2,

    /// <summary>Pediu algo que a permissão não alcança.</summary>
    AcessoNegado = 3,

    /// <summary>Exportou dados.</summary>
    ExportacaoDados = 4,

    /// <summary>Leu dado pessoal sensível.</summary>
    LeituraDadoSensivel = 5
}

/// <summary>
/// Quem VIU o quê.
///
/// A LGPD exige que o acesso a dado pessoal seja auditável, e o Vórtice tem 114 eventos de
/// login em nove anos — ou seja, nenhum. Particionada por data, com 24 meses disponíveis.
/// </summary>
public sealed class EventoDeAcesso
{
    private EventoDeAcesso() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>Quem. Nulo quando o login falhou sem identificar o usuário.</summary>
    public long? UsuarioId { get; private set; }

    /// <summary>Nome principal informado na tentativa, mesmo quando não identificou usuário.</summary>
    public string? NomePrincipal { get; private set; }

    /// <summary>O que aconteceu.</summary>
    public TipoDeEventoDeAcesso Tipo { get; private set; }

    /// <summary>Nome da entidade acessada, quando o evento é de leitura.</summary>
    public string? Entidade { get; private set; }

    /// <summary>Identificador do registro acessado, quando o evento é de leitura.</summary>
    public long? RegistroId { get; private set; }

    /// <summary>De onde veio.</summary>
    public string? EnderecoIp { get; private set; }

    /// <summary>Com que cliente.</summary>
    public string? AgenteUsuario { get; private set; }

    /// <summary>Detalhe legível do evento.</summary>
    public string? Detalhe { get; private set; }

    /// <summary>Quando aconteceu (UTC). É a coluna de particionamento.</summary>
    public DateTime OcorreuEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Registra um evento de acesso.</summary>
    public static EventoDeAcesso Registrar(TipoDeEventoDeAcesso tipo, long? usuarioId = null, string? detalhe = null)
        => new() { Tipo = tipo, UsuarioId = usuarioId, Detalhe = detalhe };
}
