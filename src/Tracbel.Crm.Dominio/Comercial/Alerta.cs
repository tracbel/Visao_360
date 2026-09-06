using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>Quanto o alerta pesa na tela.</summary>
public enum SeveridadeDeAlerta
{
    /// <summary>Só informa.</summary>
    Informativo = 0,

    /// <summary>Pede atenção antes de agir.</summary>
    Atencao = 1,

    /// <summary>Bloqueia a ação até alguém decidir.</summary>
    Critico = 2
}

/// <summary>
/// A advertência fixada no cliente.
///
/// É decisão humana com vigência — restrição de crédito, pendência jurídica, cliente em
/// negociação especial —, nunca um número calculado. Os alertas derivados, como quatro
/// clientes classe A sem visita há noventa dias, NÃO são tabela: são leitura, e essa é a
/// regra de não copiar dado derivado.
/// </summary>
public sealed class Alerta : EntidadeBase
{
    private Alerta() { }

    /// <summary>Filial dona do registro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O cliente advertido.</summary>
    public long ClienteId { get; private set; }

    /// <summary>Quanto o alerta pesa.</summary>
    public SeveridadeDeAlerta Severidade { get; private set; } = SeveridadeDeAlerta.Informativo;

    /// <summary>Título curto, o que aparece no topo da ficha.</summary>
    public string Titulo { get; private set; } = default!;

    /// <summary>O texto completo da advertência.</summary>
    public string? Detalhe { get; private set; }

    /// <summary>Início da vigência.</summary>
    public DateOnly VigenteDe { get; private set; }

    /// <summary>Fim da vigência. Nulo é sem prazo.</summary>
    public DateOnly? VigenteAte { get; private set; }

    /// <summary>Desligar sem apagar.</summary>
    public bool EstaAtivo { get; private set; } = true;

    /// <summary>Fixa um alerta no cliente.</summary>
    public static Alerta Criar(
        int empresaId,
        long clienteId,
        SeveridadeDeAlerta severidade,
        string titulo,
        DateOnly vigenteDe,
        long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        ClienteId = clienteId,
        Severidade = severidade,
        Titulo = titulo,
        VigenteDe = vigenteDe,
        CriadoPorId = criadoPorId
    };
}
