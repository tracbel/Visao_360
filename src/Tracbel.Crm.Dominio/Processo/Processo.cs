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

    /// <summary>O que o processo guarda, no formato em que a origem é comparada (documento 52 §12).</summary>
    public RetratoDoProcessoDaOrigem RetratoDaOrigem => new(
        Numero, EmpresaId, TipoProcessoId, ClienteId, CarteiraId, Titulo, FaseId, FaseDesde, Situacao, SituacaoDesde,
        ConcluidoEm, MotivoDePerdaId, ConcorrenteId, ValorEstimado, Quantidade, PrevisaoConclusao, PrevisaoConclusaoOriginal,
        ProprietarioId, CriadoEm);

    /// <summary>
    /// Registra o processo que o Vórtice declara — a onda 2 da rotina <c>PROCESSOS_VORTICE</c> (documento 52 §12).
    ///
    /// <para><b>Por que não <see cref="Abrir"/>.</b> <c>Abrir</c> é o nascimento pela tela: fase, situação, dono e
    /// carteira entram por métodos separados, cada um com a sua regra. A origem declara tudo de uma vez, e o que a rotina
    /// precisa é ACOMPANHAR — título, valor, dono, carteira e filial mudam lá, e nenhum método de tela os sincroniza. O
    /// retrato é o mesmo na criação e na atualização, e as regras de encerramento (data e motivo) valem nos dois.</para>
    /// </summary>
    /// <param name="retrato">O estado declarado.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    public static Processo DaOrigem(RetratoDoProcessoDaOrigem retrato, long criadoPorId)
    {
        var processo = new Processo { Numero = retrato.Numero, CriadoPorId = criadoPorId };
        processo.AplicarDaOrigem(Normalizar(retrato));
        return processo;
    }

    /// <summary>Acompanha a origem. Devolve se alguma coisa mudou — relido igual, não muda nada.</summary>
    /// <param name="retrato">O estado declarado nesta rodada, do mesmo número.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool AtualizarDaOrigem(RetratoDoProcessoDaOrigem retrato, long usuarioId)
    {
        // O NÚMERO É A IDENTIDADE na origem: outro número é outro processo, e não o mesmo que mudou.
        if (retrato.Numero != Numero)
            throw new RegraDeNegocioViolada("O processo da origem não troca de número: outro número é outro processo.");

        var normalizado = Normalizar(retrato);
        if (RetratoDaOrigem == normalizado) return false;

        AplicarDaOrigem(normalizado);
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>O processo voltou ao universo da origem depois de sair: deixa de estar excluído. Devolve se mudou.</summary>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool RestaurarDaOrigem(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>
    /// O retrato com as datas no milissegundo e o título aparado — o que as colunas <c>datetime2(3)</c> e
    /// <c>nvarchar(200)</c> guardam. Sem isso a data relida do Vórtice (passo de 1/300 s) nunca seria igual à gravada, e
    /// toda rodada "alteraria" todos os processos sem nada ter mudado.
    /// </summary>
    /// <param name="r">O retrato como veio.</param>
    public static RetratoDoProcessoDaOrigem Normalizar(RetratoDoProcessoDaOrigem r) => r with
    {
        Titulo = (r.Titulo ?? string.Empty).Trim(),
        FaseDesde = EstagioDoProcesso.NoMilissegundo(r.FaseDesde),
        SituacaoDesde = EstagioDoProcesso.NoMilissegundo(r.SituacaoDesde),
        ConcluidoEm = EstagioDoProcesso.NoMilissegundo(r.ConcluidoEm),
        AbertoEm = EstagioDoProcesso.NoMilissegundo(r.AbertoEm)
    };

    private void AplicarDaOrigem(RetratoDoProcessoDaOrigem r)
    {
        // AS MESMAS REGRAS DA TELA E DO BANCO (CK_Processo_Encerramento e CK_Processo_MotivoDePerda), recusadas aqui com a
        // frase que explica: a origem não ganha exceção por ser a origem.
        if (r.Titulo.Length == 0)
            throw new RegraDeNegocioViolada("Processo sem título não existe.");

        if (r.Situacao is not (SituacaoDoProcesso.Aberto or SituacaoDoProcesso.Suspenso) && r.ConcluidoEm is null)
            throw new RegraDeNegocioViolada("Processo encerrado sem data de encerramento não existe: informe quando ele fechou.");

        if (r.Situacao is SituacaoDoProcesso.Perdido && r.MotivoDePerdaId is null)
            throw new RegraDeNegocioViolada("Processo perdido exige motivo de perda — sem motivo na origem, NAO_INFORMADO_NA_ORIGEM.");

        EmpresaId = r.EmpresaId;
        TipoProcessoId = r.TipoProcessoId;
        ClienteId = r.ClienteId;
        CarteiraId = r.CarteiraId;
        Titulo = r.Titulo;
        FaseId = r.FaseId;
        FaseDesde = r.FaseDesde;
        Situacao = r.Situacao;
        SituacaoDesde = r.SituacaoDesde;
        ConcluidoEm = r.ConcluidoEm;
        MotivoDePerdaId = r.MotivoDePerdaId;
        ConcorrenteId = r.ConcorrenteId;
        ValorEstimado = r.ValorEstimado;
        Quantidade = r.Quantidade;
        PrevisaoConclusao = r.PrevisaoConclusao;
        PrevisaoConclusaoOriginal = r.PrevisaoConclusaoOriginal;
        ProprietarioId = r.ProprietarioId;

        // A ABERTURA NA ORIGEM É O NASCIMENTO AQUI, como em Abrir: um processo de 2024 carregado hoje não é de hoje, e a
        // lista e o funil leem a idade pela criação.
        CriadoEm = r.AbertoEm;
    }
}

/// <summary>
/// O ESTADO QUE O VÓRTICE DECLARA PARA UM PROCESSO DA ONDA 2 — tudo o que a rotina compara para decidir se o processo
/// mudou (documento 52 §12). As datas chegam cortadas no milissegundo (<see cref="Processo.Normalizar"/>).
/// </summary>
/// <param name="Numero">O número do processo no Vórtice — o mesmo que o usuário fala ao telefone.</param>
/// <param name="EmpresaId">A filial do processo na origem, já no CRM.</param>
/// <param name="TipoProcessoId">O tipo (31, 41 ou 50), já no catálogo do CRM.</param>
/// <param name="ClienteId">O cliente que casou pelo documento — obrigatório: só o casado entra.</param>
/// <param name="CarteiraId">A carteira MAQ_NOVOS da pessoa, quando a sincronia das carteiras a trouxe.</param>
/// <param name="Titulo">O resumo da origem; em branco, composto do tipo e do número (decisão P1).</param>
/// <param name="FaseId">A fase do BPM na origem, já no catálogo do CRM.</param>
/// <param name="FaseDesde">Desde quando está na fase (UTC).</param>
/// <param name="Situacao">A situação traduzida do status da origem.</param>
/// <param name="SituacaoDesde">Desde quando está na situação (UTC).</param>
/// <param name="ConcluidoEm">Quando encerrou (UTC) — obrigatório quando encerrado, nulo quando não.</param>
/// <param name="MotivoDePerdaId">O motivo da venda perdida principal do mesmo processo, ou o "não informado" (decisão P8).</param>
/// <param name="ConcorrenteId">O concorrente da venda perdida principal, quando o formulário o declara.</param>
/// <param name="ValorEstimado">O valor da origem, quando é valor monetário válido e positivo.</param>
/// <param name="Quantidade">A quantidade da origem, quando positiva.</param>
/// <param name="PrevisaoConclusao">A previsão de conclusão vigente.</param>
/// <param name="PrevisaoConclusaoOriginal">A primeira previsão registrada.</param>
/// <param name="ProprietarioId">O dono: a conta do responsável, senão o dono da carteira, senão quem roda a rotina.</param>
/// <param name="AbertoEm">A abertura na origem (UTC) — vira a criação do processo.</param>
public sealed record RetratoDoProcessoDaOrigem(
    long Numero,
    int EmpresaId,
    int TipoProcessoId,
    long ClienteId,
    long? CarteiraId,
    string Titulo,
    int FaseId,
    DateTime FaseDesde,
    SituacaoDoProcesso Situacao,
    DateTime SituacaoDesde,
    DateTime? ConcluidoEm,
    int? MotivoDePerdaId,
    int? ConcorrenteId,
    Dinheiro? ValorEstimado,
    decimal? Quantidade,
    DateOnly? PrevisaoConclusao,
    DateOnly? PrevisaoConclusaoOriginal,
    long ProprietarioId,
    DateTime AbertoEm);

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
