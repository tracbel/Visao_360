using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Processo;

/// <summary>Em que estado o processo está.</summary>
public enum SituacaoDoProcesso
{
    /// <summary>Em andamento.</summary>
    Aberto = 0,

    /// <summary>Parado por decisão registrada, sem estar encerrado.</summary>
    Suspenso = 1,

    /// <summary>Encerrado com venda.</summary>
    Ganho = 2,

    /// <summary>Encerrado sem venda, com motivo.</summary>
    Perdido = 3,

    /// <summary>Encerrado por cancelamento, com motivo.</summary>
    Cancelado = 4
}

/// <summary>
/// O caso — oportunidade, demonstração, aferição.
///
/// A segunda entidade central do modelo. Substitui oito tabelas do Vórtice, entre elas a de
/// 1,17 milhão de linhas que NÃO tem chave estrangeira para o cadastro da pessoa — origem
/// direta das 345.535 linhas órfãs. Aqui o cliente é obrigatório e tem chave estrangeira.
/// </summary>
public sealed class Processo : EntidadeBase
{
    private Processo() { }

    /// <summary>Número legível, sequencial por empresa. É o que o usuário fala ao telefone.</summary>
    public long Numero { get; private set; }

    /// <summary>Filial dona do processo.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O modelo de fluxo deste processo.</summary>
    public int TipoProcessoId { get; private set; }

    /// <summary>O cliente. Obrigatório, com chave estrangeira.</summary>
    public long ClienteId { get; private set; }

    /// <summary>O contato do cliente com quem se negocia.</summary>
    public long? ContatoId { get; private set; }

    /// <summary>A carteira em que o processo nasceu.</summary>
    public long? CarteiraId { get; private set; }

    /// <summary>Título do negócio, o que aparece no cartão do funil.</summary>
    public string Titulo { get; private set; } = default!;

    /// <summary>Descrição livre.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Fase atual.</summary>
    public int FaseId { get; private set; }

    /// <summary>Desde quando está nesta fase (UTC).</summary>
    public DateTime FaseDesde { get; private set; } = DateTime.UtcNow;

    /// <summary>Em que estado o processo está.</summary>
    public SituacaoDoProcesso Situacao { get; private set; } = SituacaoDoProcesso.Aberto;

    /// <summary>Desde quando está nesta situação (UTC).</summary>
    public DateTime SituacaoDesde { get; private set; } = DateTime.UtcNow;

    /// <summary>Por que o negócio não fechou. Obrigatório quando a situação é perdida.</summary>
    public int? MotivoDePerdaId { get; private set; }

    /// <summary>Texto explicando a perda, quando o motivo exige.</summary>
    public string? ObservacaoDaPerda { get; private set; }

    /// <summary>Item de catálogo com o concorrente que levou o negócio.</summary>
    public int? ConcorrenteId { get; private set; }

    /// <summary>Valor estimado do negócio.</summary>
    public Dinheiro? ValorEstimado { get; private set; }

    /// <summary>Valor efetivamente fechado.</summary>
    public Dinheiro? ValorFinal { get; private set; }

    /// <summary>Quantidade negociada.</summary>
    public decimal? Quantidade { get; private set; }

    /// <summary>Previsão de conclusão vigente.</summary>
    public DateOnly? PrevisaoConclusao { get; private set; }

    /// <summary>
    /// A primeira previsão registrada. [V] O Vórtice acerta aqui e nós copiamos: sem a
    /// referência original não dá para medir derrapagem de prazo.
    /// </summary>
    public DateOnly? PrevisaoConclusaoOriginal { get; private set; }

    /// <summary>Quando o processo foi encerrado (UTC).</summary>
    public DateTime? ConcluidoEm { get; private set; }

    /// <summary>Quem responde pelo processo.</summary>
    public long ProprietarioId { get; private set; }

    /// <summary>Abre um processo.</summary>
    /// <param name="numero">Número legível, sequencial por empresa.</param>
    /// <param name="empresaId">Filial dona.</param>
    /// <param name="tipoProcessoId">O modelo de fluxo.</param>
    /// <param name="clienteId">O cliente. Obrigatório.</param>
    /// <param name="faseId">A fase em que o processo nasce.</param>
    /// <param name="titulo">O que aparece no cartão do funil.</param>
    /// <param name="proprietarioId">Quem responde pelo processo.</param>
    /// <param name="criadoPorId">Quem criou.</param>
    /// <param name="descricao">Descrição livre.</param>
    /// <param name="contatoId">O contato com quem se negocia.</param>
    /// <param name="carteiraId">A carteira em que o processo nasceu.</param>
    /// <param name="valorEstimado">Valor estimado do negócio.</param>
    /// <param name="quantidade">Quantidade negociada.</param>
    /// <param name="previsaoConclusao">Previsão vigente.</param>
    /// <param name="previsaoConclusaoOriginal">A primeira previsão registrada.</param>
    /// <param name="abertoEmUtc">
    /// Quando o processo nasceu na ORIGEM. Existe para a carga do sistema legado conseguir
    /// preservar a data real — um processo migrado com a data de hoje transformaria o funil
    /// inteiro numa única safra, e o tempo de fase deixaria de significar alguma coisa.
    /// </param>
    public static Processo Abrir(
        long numero,
        int empresaId,
        int tipoProcessoId,
        long clienteId,
        int faseId,
        string titulo,
        long proprietarioId,
        long criadoPorId,
        string? descricao = null,
        long? contatoId = null,
        long? carteiraId = null,
        Dinheiro? valorEstimado = null,
        decimal? quantidade = null,
        DateOnly? previsaoConclusao = null,
        DateOnly? previsaoConclusaoOriginal = null,
        DateTime? abertoEmUtc = null)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new RegraDeNegocioViolada("Processo sem título não existe.");

        var nascimento = abertoEmUtc ?? DateTime.UtcNow;

        return new Processo
        {
            Numero = numero,
            EmpresaId = empresaId,
            TipoProcessoId = tipoProcessoId,
            ClienteId = clienteId,
            ContatoId = contatoId,
            CarteiraId = carteiraId,
            FaseId = faseId,
            Titulo = titulo.Trim(),
            Descricao = descricao,
            ValorEstimado = valorEstimado,
            Quantidade = quantidade,
            PrevisaoConclusao = previsaoConclusao,
            PrevisaoConclusaoOriginal = previsaoConclusaoOriginal,
            FaseDesde = nascimento,
            SituacaoDesde = nascimento,
            CriadoEm = nascimento,
            ProprietarioId = proprietarioId,
            CriadoPorId = criadoPorId
        };
    }

    /// <summary>
    /// Move o processo para a fase informada, carimbando desde quando ele está nela.
    /// </summary>
    /// <param name="faseId">A fase de destino.</param>
    /// <param name="desdeUtc">Desde quando o processo está nesta fase.</param>
    /// <param name="usuarioId">Quem moveu.</param>
    public void ColocarNaFase(int faseId, DateTime desdeUtc, long usuarioId)
    {
        if (FaseId == faseId && FaseDesde == desdeUtc) return;

        FaseId = faseId;
        FaseDesde = desdeUtc;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Registra em que estado o processo está, com a data e o motivo que o estado exige.
    ///
    /// <para><b>Encerrar exige data, e perder exige motivo.</b> As duas regras são restrições de
    /// verificação no banco também — aqui elas são recusadas antes, com a frase que explica.
    /// [V] no sistema de origem "Atividade Cancelada" derrubava o processo inteiro sem motivo
    /// nenhum e sem desfazer.</para>
    /// </summary>
    /// <param name="situacao">O estado.</param>
    /// <param name="desdeUtc">Desde quando o processo está neste estado.</param>
    /// <param name="usuarioId">Quem registrou.</param>
    /// <param name="motivoDePerdaId">Por que o negócio não fechou. Obrigatório quando perdido.</param>
    /// <param name="concluidoEmUtc">Quando encerrou. Obrigatório quando a situação é de encerramento.</param>
    /// <param name="valorFinal">Valor efetivamente fechado.</param>
    public void RegistrarSituacao(
        SituacaoDoProcesso situacao,
        DateTime desdeUtc,
        long usuarioId,
        int? motivoDePerdaId = null,
        DateTime? concluidoEmUtc = null,
        Dinheiro? valorFinal = null)
    {
        var encerra = situacao is not (SituacaoDoProcesso.Aberto or SituacaoDoProcesso.Suspenso);

        if (encerra && concluidoEmUtc is null && ConcluidoEm is null)
            throw new RegraDeNegocioViolada(
                "Processo encerrado sem data de encerramento não existe: informe quando ele fechou.");

        if (situacao is SituacaoDoProcesso.Perdido && motivoDePerdaId is null && MotivoDePerdaId is null)
            throw new RegraDeNegocioViolada(
                "Processo perdido exige motivo de perda. É o que faz o funil conseguir responder " +
                "por que se perde.");

        Situacao = situacao;
        SituacaoDesde = desdeUtc;
        if (motivoDePerdaId is not null) MotivoDePerdaId = motivoDePerdaId;
        if (valorFinal is not null) ValorFinal = valorFinal;
        ConcluidoEm = encerra ? concluidoEmUtc ?? ConcluidoEm : null;

        MarcarAlteracao(usuarioId);
    }
}

// O QUE SAIU DAQUI NA FASE 1 (documento 41): `PassagemDeFase` — o caminho percorrido, com o
// tempo em cada fase — e `ItemDeProposta` — o que está sendo vendido dentro do processo. As duas
// tabelas nasceram com o modelo inicial, nunca receberam uma linha e não tinham tela, carga nem
// consulta que as alimentasse; a fase do processo continua sendo a coluna `FaseId` aqui em cima, e
// o valor da oportunidade, o `ValorEstimado`/`ValorFinal` do cabeçalho.
//
// Nenhuma das duas foi descartada como ideia: a passagem de fase é o registro de trajetória que o
// funil vai precisar para responder prazo por fase, e o item de proposta é o que torna o valor
// auditável. Quando houver a tela que preenche, elas voltam pelo desenho do documento 40 — com o
// código escrito junto, e não anos antes.
