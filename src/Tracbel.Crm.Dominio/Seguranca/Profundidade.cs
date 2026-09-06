namespace Tracbel.Crm.Dominio.Seguranca;

/// <summary>
/// Até onde uma permissão alcança — a CAMADA 3 do modelo de segurança.
///
/// [DYN] Copiada da matriz privilégio × access level do Dataverse, que é o achado mais
/// valioso da pesquisa: ela substitui, sozinha, o trio OWD + role hierarchy + sharing rules
/// do Salesforce, e é mais barata, porque resolve com colunas da PRÓPRIA LINHA
/// (ProprietarioId, EmpresaId) — sem join.
///
/// A profundidade é gravada POR PERMISSÃO, não por usuário. Um CEN pode ter
/// Processo.Ler = Equipe e Processo.Editar = Proprios: vê o do colega, mas não altera.
/// Essa granularidade é o que o Vórtice nunca teve — lá, permissão por registro
/// simplesmente não existe.
///
/// A ordem dos valores importa: comparações usam &gt;=, então nunca reordene.
/// </summary>
public enum Profundidade
{
    /// <summary>Não vê nada.</summary>
    Nenhum = 0,

    /// <summary>Só os registros de que ele é proprietário.</summary>
    Proprios = 1,

    /// <summary>Os próprios mais os de quem está abaixo dele na hierarquia de vendas.</summary>
    Equipe = 2,

    /// <summary>Tudo da empresa (filial) dele.</summary>
    Empresa = 3,

    /// <summary>A empresa dele e as filhas, pelo caminho materializado.</summary>
    EmpresaEAbaixo = 4,

    /// <summary>Tudo. Reservado a administração e auditoria.</summary>
    Organizacao = 5
}

/// <summary>Verbos de permissão. Combinados com a entidade formam o código: <c>Lead.Qualificar</c>.</summary>
public static class Verbos
{
    /// <summary>Consultar.</summary>
    public const string Ler = "Ler";

    /// <summary>Criar registro novo.</summary>
    public const string Criar = "Criar";

    /// <summary>Alterar registro existente.</summary>
    public const string Editar = "Editar";

    /// <summary>Excluir logicamente.</summary>
    public const string Excluir = "Excluir";

    /// <summary>Trocar o proprietário.</summary>
    public const string Atribuir = "Atribuir";

    /// <summary>Conceder acesso a outra pessoa.</summary>
    public const string Compartilhar = "Compartilhar";
}
