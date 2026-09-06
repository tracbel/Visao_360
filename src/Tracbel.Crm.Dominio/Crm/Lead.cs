using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Crm;

/// <summary>Situação de um lead no seu ciclo de vida.</summary>
public enum SituacaoLead
{
    /// <summary>Chegou e ninguém tocou ainda.</summary>
    Novo = 0,

    /// <summary>Alguém já iniciou contato.</summary>
    EmContato = 1,

    /// <summary>Virou conta, contato e (opcionalmente) processo.</summary>
    Qualificado = 2,

    /// <summary>Descartado com motivo. Pode ser reaberto.</summary>
    Descartado = 3,

    /// <summary>Identificado como repetição de outro lead.</summary>
    Duplicado = 4
}

/// <summary>
/// Lead — contato ainda não qualificado.
///
/// MODELAGEM [SF] — tabela FLAT e autossuficiente (nome, empresa e contato no mesmo
/// registro), porque antes de qualificar não se sabe se existe conta, contato ou negócio.
///
/// COMPORTAMENTO [DYN] — ao qualificar, o lead NÃO é apagado nem travado: ele muda de
/// situação e passa a apontar para o que gerou. O Salesforce trava o lead como somente
/// leitura na conversão; o Dynamics mantém vivo com <c>originatingleadid</c>. Escolhemos o
/// segundo, porque mantém a rastreabilidade ("de onde veio este cliente?") e permite
/// auditar a decisão depois.
/// </summary>
public sealed class Lead : EntidadeBase
{
    // EF Core exige um construtor sem parâmetros. Privado, para que ninguém mais use.
    private Lead() { }

    /// <summary>Empresa (filial) dona do registro. Raiz do escopo de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>Nome da pessoa que fez contato.</summary>
    public string NomeContato { get; private set; } = default!;

    /// <summary>Razão social ou nome da fazenda, quando informado.</summary>
    public string? NomeEmpresa { get; private set; }

    /// <summary>E-mail informado, já validado e normalizado.</summary>
    public Email? Email { get; private set; }

    /// <summary>Telefone informado, já normalizado.</summary>
    public Telefone? Telefone { get; private set; }

    /// <summary>Documento, quando o lead informou.</summary>
    public CpfCnpj? Documento { get; private set; }

    /// <summary>O que o lead disse que quer.</summary>
    public string? Interesse { get; private set; }

    /// <summary>De onde o lead veio. FK para catálogo — nunca texto livre.</summary>
    public int OrigemId { get; private set; }

    /// <summary>Linha de negócio de interesse (MAQ, PEC, DSI, PNEUS, PUK).</summary>
    public int? LinhaNegocioId { get; private set; }

    /// <summary>
    /// O JSON exatamente como chegou do RD Station, do site ou do chat.
    ///
    /// [V] No Vórtice, campo não mapeado na tela "Integração RD (Mapeamento)" é DESCARTADO —
    /// vira texto solto no detalhe do histórico e some. Campanhas novas que ninguém mapeou
    /// nunca preenchem o formulário, e isso parece bug de código quando não é.
    /// Aqui guardamos o payload íntegro SEMPRE, o que permite reprocessar um lead antigo
    /// quando o mapeamento for corrigido.
    /// </summary>
    public string? PayloadOriginal { get; private set; }

    /// <summary>Situação atual. Só muda pelos métodos desta classe.</summary>
    public SituacaoLead Situacao { get; private set; } = SituacaoLead.Novo;

    /// <summary>Quem responde pelo lead.</summary>
    public long ProprietarioId { get; private set; }

    /// <summary>Quando foi qualificado (UTC).</summary>
    public DateTime? QualificadoEm { get; private set; }

    /// <summary>Quem qualificou.</summary>
    public long? QualificadoPorId { get; private set; }

    /// <summary>
    /// Cliente criado na qualificação.
    ///
    /// O nome é Cliente, e não Conta: o de-para do documento 15 troca o vocabulário do
    /// Salesforce pelo da Tracbel, e a coluna no banco tem que dizer a mesma palavra.
    /// </summary>
    public long? ClienteGeradoId { get; private set; }

    /// <summary>Contato criado na qualificação.</summary>
    public long? ContatoGeradoId { get; private set; }

    /// <summary>Processo aberto na qualificação, quando houve.</summary>
    public long? ProcessoGeradoId { get; private set; }

    /// <summary>Motivo do descarte. FK para catálogo.</summary>
    public int? MotivoDescarteId { get; private set; }

    /// <summary>
    /// Único jeito de nascer um lead.
    ///
    /// Se os dados não servem, o objeto NÃO EXISTE — em vez de existir inválido e quebrar
    /// três camadas adiante, que é o padrão do sistema legado.
    /// </summary>
    /// <exception cref="RegraDeNegocioViolada">Sem nome, ou sem nenhuma forma de contato.</exception>
    public static Lead Criar(
        int empresaId,
        string nomeContato,
        int origemId,
        long proprietarioId,
        long criadoPorId,
        Email? email = null,
        Telefone? telefone = null,
        string? nomeEmpresa = null,
        CpfCnpj? documento = null,
        string? interesse = null,
        int? linhaNegocioId = null,
        string? payloadOriginal = null)
    {
        if (string.IsNullOrWhiteSpace(nomeContato))
            throw new RegraDeNegocioViolada("Lead precisa de um nome de contato.");

        // Um lead sem NENHUMA forma de contato é lixo: ninguém consegue trabalhá-lo.
        // Barrar na entrada é mais barato do que descobrir na carteira do vendedor.
        // [V] os 489 processos gerados pelo conector do TALLOS Chat, de 99 pessoas,
        // ficaram 98% parados em "Apresentação / EM ABERTO" — exatamente por isso.
        if (email is null && telefone is null)
            throw new RegraDeNegocioViolada(
                "Lead precisa de e-mail ou telefone. Sem contato, ninguém consegue atendê-lo.");

        var lead = new Lead
        {
            EmpresaId = empresaId,
            NomeContato = nomeContato.Trim(),
            NomeEmpresa = string.IsNullOrWhiteSpace(nomeEmpresa) ? null : nomeEmpresa.Trim(),
            Email = email,
            Telefone = telefone,
            Documento = documento,
            Interesse = string.IsNullOrWhiteSpace(interesse) ? null : interesse.Trim(),
            OrigemId = origemId,
            LinhaNegocioId = linhaNegocioId,
            ProprietarioId = proprietarioId,
            CriadoPorId = criadoPorId,
            PayloadOriginal = payloadOriginal,
            Situacao = SituacaoLead.Novo
        };

        lead.RegistrarEvento(new LeadRecebido(lead.ChavePublica, empresaId, origemId, DateTime.UtcNow));
        return lead;
    }

    /// <summary>Marca que alguém iniciou contato. Idempotente.</summary>
    public void MarcarEmContato(long usuarioId)
    {
        if (Situacao is SituacaoLead.Qualificado or SituacaoLead.Descartado)
            throw new RegraDeNegocioViolada(
                $"Lead {Situacao.ToString().ToLowerInvariant()} não volta para 'em contato'.");

        if (Situacao == SituacaoLead.EmContato) return;

        Situacao = SituacaoLead.EmContato;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Qualifica o lead, ligando-o aos registros criados.
    ///
    /// A transição é validada AQUI, dentro da entidade. É impossível qualificar duas vezes
    /// ou qualificar um lead descartado, porque não existe caminho de código que pule esta
    /// verificação — não há setter público de <see cref="Situacao"/>.
    /// </summary>
    /// <exception cref="RegraDeNegocioViolada">Se já qualificado, ou se descartado.</exception>
    public void Qualificar(long clienteId, long contatoId, long? processoId, long usuarioId)
    {
        if (Situacao == SituacaoLead.Qualificado)
            throw new RegraDeNegocioViolada("Este lead já foi qualificado.");

        if (Situacao == SituacaoLead.Descartado)
            throw new RegraDeNegocioViolada(
                "Lead descartado não pode ser qualificado. Reabra o lead antes.");

        Situacao = SituacaoLead.Qualificado;
        QualificadoEm = DateTime.UtcNow;
        QualificadoPorId = usuarioId;
        ClienteGeradoId = clienteId;
        ContatoGeradoId = contatoId;
        ProcessoGeradoId = processoId;
        MotivoDescarteId = null;
        MarcarAlteracao(usuarioId);

        RegistrarEvento(new LeadQualificado(
            ChavePublica, clienteId, contatoId, processoId, LinhaNegocioId, usuarioId, DateTime.UtcNow));
    }

    /// <summary>
    /// Descarta com motivo obrigatório.
    ///
    /// [V] No Vórtice, o resultado "Atividade Cancelada" cancela o PROCESSO INTEIRO,
    /// gravando <c>Fase=Finalizado, Status=CANCELADO</c>, sem registrar motivo e SEM DESFAZER —
    /// a tela não tem "desfazer andamento" e o histórico não pode ser excluído pelo usuário.
    /// Aqui: motivo é obrigatório, e <see cref="Reabrir"/> existe.
    /// </summary>
    public void Descartar(int motivoId, string? observacao, long usuarioId)
    {
        if (Situacao == SituacaoLead.Qualificado)
            throw new RegraDeNegocioViolada(
                "Lead já qualificado não pode ser descartado. Encerre o processo gerado.");

        if (Situacao == SituacaoLead.Descartado) return;

        Situacao = SituacaoLead.Descartado;
        MotivoDescarteId = motivoId;
        MarcarAlteracao(usuarioId);

        RegistrarEvento(new LeadDescartado(ChavePublica, motivoId, observacao, usuarioId, DateTime.UtcNow));
    }

    /// <summary>
    /// Desfazer existe. Sempre. Toda transição de estado é reversível ou justificada —
    /// é o princípio 3 do projeto.
    /// </summary>
    /// <exception cref="RegraDeNegocioViolada">Se não estiver descartado, ou sem justificativa.</exception>
    public void Reabrir(string justificativa, long usuarioId)
    {
        if (Situacao != SituacaoLead.Descartado)
            throw new RegraDeNegocioViolada("Só lead descartado pode ser reaberto.");

        if (string.IsNullOrWhiteSpace(justificativa))
            throw new RegraDeNegocioViolada("Reabertura exige justificativa.");

        Situacao = SituacaoLead.Novo;
        MotivoDescarteId = null;
        MarcarAlteracao(usuarioId);

        RegistrarEvento(new LeadReaberto(ChavePublica, justificativa, usuarioId, DateTime.UtcNow));
    }

    /// <summary>Marca como repetição de um lead já existente.</summary>
    public void MarcarComoDuplicado(long leadOriginalId, long usuarioId)
    {
        if (Situacao == SituacaoLead.Qualificado)
            throw new RegraDeNegocioViolada("Lead já qualificado não pode virar duplicado.");

        Situacao = SituacaoLead.Duplicado;
        MarcarAlteracao(usuarioId);
        RegistrarEvento(new LeadMarcadoDuplicado(ChavePublica, leadOriginalId, usuarioId, DateTime.UtcNow));
    }
}
