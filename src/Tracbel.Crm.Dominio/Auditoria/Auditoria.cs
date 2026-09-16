namespace Tracbel.Crm.Dominio.Auditoria;

// O QUE SAIU DAQUI NA FASE 1 (documento 41): `CampoAuditado` — a lista de adesão explícita que diz
// quais campos se audita — e `EventoDeAcesso`, com o enum `TipoDeEventoDeAcesso`, o registro de quem
// VIU o quê. As duas nasceram com o modelo inicial e nunca receberam uma linha; nenhum caminho da
// aplicação escrevia nelas, e por isso a adesão "sem linha aqui, não há auditoria" significava, na
// prática, auditoria nenhuma sendo gravada por elas.
//
// O QUE FICA: `AlteracaoDeCampo`, aqui embaixo, que é a tabela de log que o sistema de fato escreve.
// O alcance entre filiais continua registrado — em log de aplicação, por
// `DiarioDeAlcanceEntreEmpresasEmLog`, e não por tabela.
//
// A auditoria é o assunto da FASE 2 do documento 41: é lá que se decide qual é a adesão e onde o
// evento de acesso é gravado, com o código que grava escrito junto.

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

// A LGPD exige que o acesso a dado pessoal seja auditável, e a tabela `EventoDeAcesso` existia para
// isso — vazia. A exigência não desapareceu com ela: está anotada como pendência da fase 2 no
// documento 41, para nascer junto com o código que a preenche no login e na leitura sensível.
