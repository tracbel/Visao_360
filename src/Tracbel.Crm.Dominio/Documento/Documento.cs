using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Documento;

/// <summary>
/// O arquivo, seus metadados e onde ele mora.
///
/// O binário NUNCA fica no banco: a coluna guarda o caminho no armazenamento de objetos. O
/// resumo criptográfico do conteúdo serve para deduplicar e para conferir integridade.
/// </summary>
public sealed class Documento : EntidadeBase
{
    private Documento() { }

    /// <summary>Filial dona do arquivo.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Item do catálogo TIPO_DOCUMENTO.</summary>
    public int TipoDocumentoId { get; private set; }

    /// <summary>Nome do arquivo como o usuário o conhece.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Tipo de conteúdo declarado no envio.</summary>
    public string TipoConteudo { get; private set; } = default!;

    /// <summary>Tamanho em bytes.</summary>
    public long TamanhoBytes { get; private set; }

    /// <summary>Resumo criptográfico do conteúdo, em hexadecimal.</summary>
    public string ResumoConteudo { get; private set; } = default!;

    /// <summary>Caminho no armazenamento de objetos.</summary>
    public string CaminhoArmazenamento { get; private set; } = default!;

    /// <summary>
    /// Versão do arquivo. Substituir gera versão nova, nunca sobrescreve.
    ///
    /// O nome é NumeroVersao e não Versao para não colidir com a coluna de concorrência
    /// otimista que vem de EntidadeBase — são duas coisas diferentes.
    /// </summary>
    public int NumeroVersao { get; private set; } = 1;

    /// <summary>Até quando o documento é válido, quando tem prazo.</summary>
    public DateOnly? ValidoAte { get; private set; }

    /// <summary>Cria o registro de um arquivo já enviado ao armazenamento.</summary>
    public static Documento Criar(
        int empresaId,
        int tipoDocumentoId,
        string nome,
        string tipoConteudo,
        long tamanhoBytes,
        string resumoConteudo,
        string caminhoArmazenamento,
        long criadoPorId)
    {
        if (tamanhoBytes <= 0)
            throw new RegraDeNegocioViolada("Documento de tamanho zero não é documento.");

        return new Documento
        {
            EmpresaId = empresaId,
            TipoDocumentoId = tipoDocumentoId,
            Nome = nome,
            TipoConteudo = tipoConteudo,
            TamanhoBytes = tamanhoBytes,
            ResumoConteudo = resumoConteudo,
            CaminhoArmazenamento = caminhoArmazenamento,
            CriadoPorId = criadoPorId
        };
    }
}

/// <summary>
/// A que registro o documento está preso.
///
/// Polimórfico de propósito: o mesmo arquivo serve a cliente, processo e equipamento. A
/// chave estrangeira para o documento é obrigatória e declarada — [V] é exatamente o que
/// falta no Vórtice, onde 5.302 vínculos órfãos travam o sincronismo do aplicativo de campo,
/// porque o SQL Server serve o órfão e o banco do aparelho recusa.
/// </summary>
public sealed class Vinculo
{
    private Vinculo() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O documento.</summary>
    public long DocumentoId { get; private set; }

    /// <summary>Nome da entidade a que o documento está preso.</summary>
    public string Entidade { get; private set; } = default!;

    /// <summary>Identificador interno do registro a que o documento está preso.</summary>
    public long RegistroId { get; private set; }

    /// <summary>Quando o vínculo foi criado (UTC).</summary>
    public DateTime VinculadoEm { get; private set; } = DateTime.UtcNow;

    /// <summary>Quem vinculou.</summary>
    public long VinculadoPorId { get; private set; }

    /// <summary>Prende um documento a um registro.</summary>
    public static Vinculo Criar(long documentoId, string entidade, long registroId, long vinculadoPorId) => new()
    {
        DocumentoId = documentoId,
        Entidade = entidade,
        RegistroId = registroId,
        VinculadoPorId = vinculadoPorId
    };
}
