using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>O que a equipe pode fazer com registros.</summary>
public enum TipoDeEquipe
{
    /// <summary>Pode ser DONA de registros.</summary>
    Proprietaria = 0,

    /// <summary>Só concede acesso; nunca é proprietária.</summary>
    Acesso = 1
}

/// <summary>
/// Uma equipe de vendas.
///
/// [DYN] É a equipe proprietária do Dataverse: a forma barata de dar acesso a um grupo sem
/// materializar uma linha de compartilhamento por usuário.
/// </summary>
public sealed class Equipe : EntidadeBase
{
    private Equipe() { }

    /// <summary>Filial dona da equipe.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Nome da equipe.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Para que serve, em português.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Se a equipe pode ser dona de registros.</summary>
    public TipoDeEquipe Tipo { get; private set; } = TipoDeEquipe.Acesso;

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Cria uma equipe.</summary>
    public static Equipe Criar(int empresaId, string nome, TipoDeEquipe tipo, long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        Nome = nome,
        Tipo = tipo,
        CriadoPorId = criadoPorId
    };
}

/// <summary>
/// Quem está em qual equipe, com período.
///
/// O período é o que permite responder quem estava nesta equipe em março sem apagar linha.
/// </summary>
public sealed class EquipeMembro
{
    private EquipeMembro() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A equipe.</summary>
    public long EquipeId { get; private set; }

    /// <summary>O usuário.</summary>
    public long UsuarioId { get; private set; }

    /// <summary>Se o usuário lidera a equipe.</summary>
    public bool EhLider { get; private set; }

    /// <summary>Quando entrou na equipe (UTC).</summary>
    public DateTime EntrouEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quando saiu. Nulo é vínculo vigente.</summary>
    public DateTime? SaiuEm { get; private set; }

    /// <summary>Cria um vínculo de equipe.</summary>
    public static EquipeMembro Criar(long equipeId, long usuarioId, bool ehLider = false) => new()
    {
        EquipeId = equipeId,
        UsuarioId = usuarioId,
        EhLider = ehLider
    };
}
