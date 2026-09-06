using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Comercial;

/// <summary>Canal a que o consentimento se refere.</summary>
public enum CanalDeConsentimento
{
    /// <summary>E-mail.</summary>
    Email = 0,

    /// <summary>Mensagem de texto.</summary>
    Sms = 1,

    /// <summary>WhatsApp.</summary>
    WhatsApp = 2,

    /// <summary>Telefone.</summary>
    Telefone = 3,

    /// <summary>Correspondência física.</summary>
    Correspondencia = 4
}

/// <summary>Para que o titular autorizou o contato.</summary>
public enum FinalidadeDeConsentimento
{
    /// <summary>Divulgação e campanha.</summary>
    Marketing = 0,

    /// <summary>Aviso operacional sobre algo que o titular contratou.</summary>
    Transacional = 1,

    /// <summary>Pesquisa de satisfação.</summary>
    Pesquisa = 2,

    /// <summary>Cobrança.</summary>
    Cobranca = 3
}

/// <summary>
/// A autorização de contato por canal, com prova e data.
///
/// A tabela é somente-acrescentar: revogar é inserir uma linha negando, nunca apagar. A
/// resposta vigente é a linha mais recente para o trio titular, canal e finalidade — é assim
/// que se sustenta uma solicitação de titular.
///
/// [V] O opt-in do Vórtice não tem data, não tem origem e não tem prova: só um sinalizador,
/// e 23 linhas ao todo para 118 mil clientes.
/// </summary>
public sealed class ConsentimentoComunicacao
{
    private ConsentimentoComunicacao() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>Filial dona do registro.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O cliente titular. Exatamente um entre cliente e contato.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>O contato titular. Exatamente um entre cliente e contato.</summary>
    public long? ContatoId { get; private set; }

    /// <summary>Canal a que a decisão se refere.</summary>
    public CanalDeConsentimento Canal { get; private set; }

    /// <summary>Finalidade a que a decisão se refere.</summary>
    public FinalidadeDeConsentimento Finalidade { get; private set; }

    /// <summary>A decisão do titular.</summary>
    public bool Concedido { get; private set; }

    /// <summary>Quando o titular decidiu (UTC). Parte da prova.</summary>
    public DataHoraUtc DecididaEm { get; private set; }

    /// <summary>De onde veio a prova: formulário do site, telefone, contrato.</summary>
    public string OrigemEvidencia { get; private set; } = default!;

    /// <summary>Endereço da prova: URL, identificador da submissão, caminho do documento.</summary>
    public string? ReferenciaEvidencia { get; private set; }

    /// <summary>Endereço de rede de onde a decisão foi registrada.</summary>
    public string? EnderecoIp { get; private set; }

    /// <summary>Quem registrou. Nulo é ação do próprio titular.</summary>
    public long? RegistradoPorId { get; private set; }

    /// <summary>Quando a linha foi gravada (UTC).</summary>
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Registra a decisão de um cliente.</summary>
    public static ConsentimentoComunicacao DoCliente(
        int empresaId,
        long clienteId,
        CanalDeConsentimento canal,
        FinalidadeDeConsentimento finalidade,
        bool concedido,
        DataHoraUtc decididaEm,
        string origemEvidencia) => new()
    {
        EmpresaId = empresaId,
        ClienteId = clienteId,
        Canal = canal,
        Finalidade = finalidade,
        Concedido = concedido,
        DecididaEm = decididaEm,
        OrigemEvidencia = origemEvidencia
    };

    /// <summary>Registra a decisão de um contato.</summary>
    public static ConsentimentoComunicacao DoContato(
        int empresaId,
        long contatoId,
        CanalDeConsentimento canal,
        FinalidadeDeConsentimento finalidade,
        bool concedido,
        DataHoraUtc decididaEm,
        string origemEvidencia) => new()
    {
        EmpresaId = empresaId,
        ContatoId = contatoId,
        Canal = canal,
        Finalidade = finalidade,
        Concedido = concedido,
        DecididaEm = decididaEm,
        OrigemEvidencia = origemEvidencia
    };
}
