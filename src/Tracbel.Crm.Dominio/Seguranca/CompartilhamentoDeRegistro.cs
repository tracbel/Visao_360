using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>O que o compartilhamento permite.</summary>
public enum NivelDeCompartilhamento
{
    /// <summary>Só consultar.</summary>
    Leitura = 0,

    /// <summary>Consultar e alterar.</summary>
    Edicao = 1
}

/// <summary>Por que este sujeito enxerga este registro.</summary>
public enum MotivoDeCompartilhamento
{
    /// <summary>Alguém concedeu na tela.</summary>
    Manual = 0,

    /// <summary>Uma regra de automação concedeu.</summary>
    Regra = 1,

    /// <summary>Veio da equipe.</summary>
    Equipe = 2,

    /// <summary>Veio da hierarquia comercial.</summary>
    Hierarquia = 3,

    /// <summary>Delegação temporária, como cobertura de férias.</summary>
    Delegacao = 4
}

/// <summary>
/// Acesso pontual a um registro específico.
///
/// É a peça que falta no Vórtice para deixar o gerente ver uma oportunidade sem trocar o
/// dono. [DYN] A Microsoft precisou criar uma API só para responder por que este usuário vê
/// este registro; aqui o motivo está gravado desde o começo.
/// </summary>
public sealed class CompartilhamentoDeRegistro : EntidadeBase
{
    private CompartilhamentoDeRegistro() { }

    /// <summary>Filial dona do registro compartilhado.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Nome da entidade compartilhada. Ex.: Processo, Cliente.</summary>
    public string Entidade { get; private set; } = default!;

    /// <summary>Identificador interno do registro compartilhado.</summary>
    public long RegistroId { get; private set; }

    /// <summary>Usuário que recebe o acesso. Exatamente um entre usuário e equipe.</summary>
    public long? UsuarioId { get; private set; }

    /// <summary>Equipe que recebe o acesso. Exatamente um entre usuário e equipe.</summary>
    public long? EquipeId { get; private set; }

    /// <summary>O que o acesso permite.</summary>
    public NivelDeCompartilhamento Nivel { get; private set; }

    /// <summary>Por que o acesso existe.</summary>
    public MotivoDeCompartilhamento Motivo { get; private set; }

    /// <summary>Regra que concedeu, quando o motivo é automação.</summary>
    public int? RegraOrigemId { get; private set; }

    /// <summary>Quando expira. Nulo é permanente enquanto ninguém revogar.</summary>
    public DateTime? ExpiraEm { get; private set; }

    /// <summary>Concede acesso a um usuário.</summary>
    public static CompartilhamentoDeRegistro ParaUsuario(
        int empresaId,
        string entidade,
        long registroId,
        long usuarioId,
        NivelDeCompartilhamento nivel,
        MotivoDeCompartilhamento motivo,
        long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        Entidade = entidade,
        RegistroId = registroId,
        UsuarioId = usuarioId,
        Nivel = nivel,
        Motivo = motivo,
        CriadoPorId = criadoPorId
    };

    /// <summary>Concede acesso a uma equipe.</summary>
    public static CompartilhamentoDeRegistro ParaEquipe(
        int empresaId,
        string entidade,
        long registroId,
        long equipeId,
        NivelDeCompartilhamento nivel,
        MotivoDeCompartilhamento motivo,
        long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        Entidade = entidade,
        RegistroId = registroId,
        EquipeId = equipeId,
        Nivel = nivel,
        Motivo = motivo,
        CriadoPorId = criadoPorId
    };
}
