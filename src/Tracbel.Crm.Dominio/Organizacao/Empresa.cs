namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// A filial da Tracbel — a fronteira de segurança, não o organograma.
///
/// É a coluna <c>EmpresaId</c> gravada em cada linha que torna a checagem de acesso barata,
/// sem join. [V] No Vórtice a multiempresa é nominal: 18 filiais, <c>NroEmpresa</c> em toda
/// tabela de permissão, e 340 dos 409 usuários ativos com acesso a 17 empresas. Isolamento
/// que ninguém usa não é isolamento.
/// </summary>
public sealed class Empresa
{
    private Empresa() { }

    /// <summary>Identificador interno. É <c>int</c> porque aparece como coluna em toda tabela.</summary>
    public int Id { get; private set; }

    /// <summary>O identificador que a API expõe.</summary>
    public Guid ChavePublica { get; private set; } = Guid.NewGuid();

    /// <summary>Código da filial usado no ERP.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>Nome da filial.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>CNPJ da filial, quando cadastrado.</summary>
    public Comum.CpfCnpj? Cnpj { get; private set; }

    /// <summary>Empresa acima na hierarquia. Nulo é a raiz (a holding).</summary>
    public int? EmpresaPaiId { get; private set; }

    /// <summary>
    /// Caminho materializado: <c>/1/4/9/</c>. Responde "esta empresa e todas abaixo" com um
    /// <c>LIKE</c>, sem consulta recursiva no caminho quente.
    /// </summary>
    public string Caminho { get; private set; } = "/";

    /// <summary>Profundidade na hierarquia. Zero é a raiz.</summary>
    public short Nivel { get; private set; }

    /// <summary>Desativar nunca apaga: o histórico e as permissões continuam de pé.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Quando o registro nasceu (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Última alteração (UTC).</summary>
    public DateTime? AlteradoEm { get; private set; }

    /// <summary>Cria uma filial.</summary>
    public static Empresa Criar(string codigo, string nome, int? empresaPaiId = null) => new()
    {
        Codigo = codigo,
        Nome = nome,
        EmpresaPaiId = empresaPaiId
    };
}
