using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>Por onde se fala com a pessoa.</summary>
public enum TipoDeCanal
{
    /// <summary>Endereço de e-mail.</summary>
    Email = 0,

    /// <summary>Telefone fixo.</summary>
    Telefone = 1,

    /// <summary>Telefone celular.</summary>
    Celular = 2,

    /// <summary>Número de WhatsApp.</summary>
    WhatsApp = 3
}

/// <summary>
/// Telefone, e-mail e WhatsApp — um modelo, não três.
///
/// [V] O Vórtice tem três modelos concorrentes de e-mail e três de telefone dentro do mesmo
/// banco, mais onze colunas numeradas de telefone e fax no cadastro da pessoa. Ninguém sabe
/// qual é a verdade. Aqui existe uma tabela só, polimórfica entre cliente e contato, com a
/// regra de exatamente um dono garantida por restrição de verificação.
/// </summary>
public sealed class CanalContato
{
    private CanalContato() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>Filial dona do registro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O cliente dono do canal. Exatamente um entre cliente e contato.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>O contato dono do canal. Exatamente um entre cliente e contato.</summary>
    public long? ContatoId { get; private set; }

    /// <summary>Que canal é este.</summary>
    public TipoDeCanal Tipo { get; private set; }

    /// <summary>O valor como o usuário digitou.</summary>
    public string Valor { get; private set; } = default!;

    /// <summary>
    /// O valor só com o que interessa para comparar: dígitos no telefone, minúsculas no
    /// e-mail. É o que sustenta busca e deduplicação.
    /// </summary>
    public string ValorNormalizado { get; private set; } = default!;

    /// <summary>Rótulo dado pelo usuário: comercial, fazenda, pessoal.</summary>
    public string? Rotulo { get; private set; }

    /// <summary>Se é o canal principal daquele tipo.</summary>
    public bool EhPrincipal { get; private set; }

    /// <summary>Se o canal foi confirmado como válido.</summary>
    public bool EhValido { get; private set; } = true;

    /// <summary>Quando a validade foi confirmada (UTC).</summary>
    public DateTime? ValidadoEm { get; private set; }

    /// <summary>Quando o registro nasceu (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Exclusão lógica. Nulo é ativo.</summary>
    public DateTime? ExcluidoEm { get; private set; }

    /// <summary>Cria um canal de e-mail do cliente.</summary>
    public static CanalContato EmailDoCliente(int empresaId, long clienteId, Email email) => new()
    {
        EmpresaId = empresaId,
        ClienteId = clienteId,
        Tipo = TipoDeCanal.Email,
        Valor = email.Endereco,
        ValorNormalizado = email.Endereco.ToLowerInvariant()
    };

    /// <summary>Cria um canal de telefone do contato.</summary>
    public static CanalContato TelefoneDoContato(int empresaId, long contatoId, Telefone telefone, TipoDeCanal tipo)
    {
        if (tipo is TipoDeCanal.Email)
            throw new RegraDeNegocioViolada("Telefone não pode ser cadastrado como canal de e-mail.");

        return new CanalContato
        {
            EmpresaId = empresaId,
            ContatoId = contatoId,
            Tipo = tipo,
            Valor = telefone.Numero,
            ValorNormalizado = telefone.Numero
        };
    }
}
