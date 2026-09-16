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
// A FASE 2 DO DOCUMENTO 41 decidiu a adesão: ela mora em código, em `PoliticaDeAuditoria`, e a
// trilha é gravada por um único ponto — o `SaveChanges` do `CrmDbContext` — na mesma transação
// do dado. O evento de acesso continua pendente.

/// <summary>
/// Quem mudou o quê, de quanto para quanto — e de onde veio a mudança.
///
/// Fato imutável, particionado por data e com retenção declarada na própria migração que a
/// cria. Substitui as 22 tabelas de log clonadas por assunto do Vórtice, que somam 43,7
/// milhões de linhas e nunca foram expurgadas.
///
/// <para><b>Ninguém grava esta linha à mão.</b> Desde a fase 2 ela nasce do <c>SaveChanges</c>, a
/// partir de <see cref="PoliticaDeAuditoria"/> e do contexto de acesso. A fábrica
/// <see cref="Registrar"/> continua existindo para o teste e para a leitura do modelo.</para>
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

    /// <summary>Correlaciona com a requisição — ou a execução de carga — que causou a mudança.</summary>
    public Guid? CorrelacaoId { get; private set; }

    /// <summary>De onde veio a gravação: pessoa, integração, importação, sistema ou rotina.</summary>
    public OrigemDaOperacao Origem { get; private set; } = OrigemDaOperacao.Usuario;

    /// <summary>O sistema externo, quando a origem é integração ou importação de um sistema conhecido.</summary>
    public int? SistemaId { get; private set; }

    /// <summary>O que aconteceu com o registro: inclusão, alteração ou exclusão.</summary>
    public OperacaoAuditada Operacao { get; private set; } = OperacaoAuditada.Alteracao;

    /// <summary>Registra uma alteração de campo.</summary>
    public static AlteracaoDeCampo Registrar(
        int empresaId,
        string entidade,
        long registroId,
        string campo,
        string? valorAnterior,
        string? valorNovo,
        long alteradoPorId,
        OrigemDaOperacao origem,
        OperacaoAuditada operacao,
        int? sistemaId = null,
        Guid? correlacaoId = null) => new()
    {
        EmpresaId = empresaId,
        Entidade = entidade,
        RegistroId = registroId,
        Campo = campo,
        ValorAnterior = valorAnterior,
        ValorNovo = valorNovo,
        AlteradoPorId = alteradoPorId,
        Origem = origem,
        Operacao = operacao,
        SistemaId = sistemaId,
        CorrelacaoId = correlacaoId
    };
}

// A LGPD exige que o acesso a dado pessoal seja auditável, e a tabela `EventoDeAcesso` existia para
// isso — vazia. A exigência não desapareceu com ela: está anotada como pendência da fase 2 no
// documento 41, para nascer junto com o código que a preenche no login e na leitura sensível.
