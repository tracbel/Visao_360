using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>
/// A pessoa física dentro do cliente — com identidade própria.
///
/// [V] No Vórtice o contato é entidade fraca: chave composta com o cadastro da pessoa, sem
/// chave estrangeira e sem identidade própria. O resultado medido são 625 contatos órfãos, e
/// um contato que troca de empresa precisa ser recadastrado. Aqui o contato existe por si, e
/// o vínculo com o cliente é uma tabela à parte, com papel e período.
/// </summary>
public sealed class Contato : EntidadeBase
{
    private Contato() { }

    /// <summary>Filial dona do cadastro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Primeiro nome.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Sobrenome.</summary>
    public string? Sobrenome { get; private set; }

    /// <summary>CPF do contato, quando informado.</summary>
    public CpfCnpj? Documento { get; private set; }

    /// <summary>Cargo declarado.</summary>
    public string? Cargo { get; private set; }

    /// <summary>Data de nascimento.</summary>
    public DateOnly? DataNascimento { get; private set; }

    /// <summary>Quem responde pelo contato.</summary>
    public long ProprietarioId { get; private set; }

    /// <summary>Cria um contato.</summary>
    public static Contato Criar(
        int empresaId,
        string nome,
        long proprietarioId,
        long criadoPorId,
        string? sobrenome = null,
        CpfCnpj? documento = null,
        string? cargo = null,
        DateOnly? dataNascimento = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioViolada("Contato sem nome não existe.");

        ConferirDocumento(documento);

        return new Contato
        {
            EmpresaId = empresaId,
            Nome = nome.Trim(),
            Sobrenome = string.IsNullOrWhiteSpace(sobrenome) ? null : sobrenome.Trim(),
            Documento = documento,
            Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo.Trim(),
            DataNascimento = dataNascimento,
            ProprietarioId = proprietarioId,
            CriadoPorId = criadoPorId
        };
    }

    /// <summary>
    /// Altera o cadastro do contato.
    ///
    /// A EMPRESA NÃO ENTRA, pela mesma razão de <see cref="Cliente.Alterar"/>: a filial dona é a
    /// fronteira de acesso, não um campo de formulário.
    /// </summary>
    public void Alterar(
        string nome,
        long usuarioId,
        string? sobrenome = null,
        CpfCnpj? documento = null,
        string? cargo = null,
        DateOnly? dataNascimento = null)
    {
        if (EstaExcluido)
            throw new RegraDeNegocioViolada("Contato inativado não aceita alteração. Reative antes.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraDeNegocioViolada("Contato sem nome não existe.");

        ConferirDocumento(documento);

        Nome = nome.Trim();
        Sobrenome = string.IsNullOrWhiteSpace(sobrenome) ? null : sobrenome.Trim();
        Documento = documento;
        Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo.Trim();
        DataNascimento = dataNascimento;

        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Contato é PESSOA FÍSICA — o documento dele só pode ser CPF.
    ///
    /// A restrição <c>CK_Contato_Documento</c> diz o mesmo no banco; aqui a recusa vem com a
    /// frase que o usuário lê, em vez de um erro de constraint.
    /// </summary>
    private static void ConferirDocumento(CpfCnpj? documento)
    {
        if (documento is { } doc && !doc.EhPessoaFisica)
            throw new RegraDeNegocioViolada(
                "O contato é uma pessoa física: informe um CPF, não um CNPJ.");
    }
}

/// <summary>
/// O vínculo entre cliente e contato, com papel e período.
///
/// É N para N de propósito: o mesmo contato aparece em duas empresas do grupo sem
/// recadastro, e sair de uma delas é encerrar o vínculo, não apagar a pessoa.
/// </summary>
public sealed class ClienteContato
{
    private ClienteContato() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O cliente.</summary>
    public long ClienteId { get; private set; }

    /// <summary>O contato.</summary>
    public long ContatoId { get; private set; }

    /// <summary>
    /// Item do catálogo PAPEL_CONTATO: comprador, operador, financeiro, decisor.
    ///
    /// É chave estrangeira para catálogo, nunca texto livre — a regra da seção 6 do doc 17.
    /// </summary>
    public int PapelId { get; private set; }

    /// <summary>Se é o contato principal do cliente. No máximo um por cliente.</summary>
    public bool EhPrincipal { get; private set; }

    /// <summary>Início do vínculo.</summary>
    public DateOnly? IniciouEm { get; private set; }

    /// <summary>Fim do vínculo. Nulo é vínculo vigente.</summary>
    public DateOnly? EncerrouEm { get; private set; }

    /// <summary>Cria um vínculo entre cliente e contato.</summary>
    public static ClienteContato Criar(long clienteId, long contatoId, int papelId, bool ehPrincipal = false) => new()
    {
        ClienteId = clienteId,
        ContatoId = contatoId,
        PapelId = papelId,
        EhPrincipal = ehPrincipal
    };
}
