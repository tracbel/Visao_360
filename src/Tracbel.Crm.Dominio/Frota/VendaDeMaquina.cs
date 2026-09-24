using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Frota;

/// <summary>
/// O que a venda de máquina traz da origem, já saneado — sem valor, custo, comissão nem margem.
/// </summary>
/// <param name="EmpresaId">A filial que vendeu.</param>
/// <param name="EmpresaDoFaturamentoId">A filial que faturou, quando declarada.</param>
/// <param name="VendidaEm">A data da venda.</param>
/// <param name="FaturadaEm">A data do faturamento.</param>
/// <param name="EntregueEm">A data da entrega.</param>
/// <param name="RegistradaNaOrigemEm">Quando a venda foi aberta na origem.</param>
/// <param name="NumeroDoPedido">O número do pedido.</param>
/// <param name="NumeroDaNotaFiscal">O número da nota fiscal de venda.</param>
/// <param name="SituacaoNaOrigem">A situação como a origem escreve (códigos sem significado documentado).</param>
/// <param name="GestaoNaOrigem">Varejo ou Grandes Contas — atributo DA VENDA, nunca do cliente.</param>
/// <param name="VendaDireta">Se a origem marca como venda direta.</param>
/// <param name="RepasseDireto">Se a origem marca como repasse direto.</param>
/// <param name="Quantidade">A quantidade declarada na venda.</param>
/// <param name="LinhaNaOrigem">A linha, exatamente como a origem escreve.</param>
/// <param name="ProdutoNaOrigem">O produto, exatamente como a origem escreve.</param>
/// <param name="EmpresaNaOrigem">A empresa da origem (Agro Norte, Agro Noroeste).</param>
/// <param name="UnidadeNaOrigem">A unidade que vendeu, como a origem escreve.</param>
/// <param name="UnidadeDoFaturamentoNaOrigem">A unidade que faturou, como a origem escreve.</param>
/// <param name="HashDaOrigem">O resumo do conteúdo lido — é o que a recarga compara.</param>
/// <param name="Transformacoes">As transformações aplicadas na leitura, em texto.</param>
public sealed record DadosDaVendaNaOrigem(
    int EmpresaId,
    int? EmpresaDoFaturamentoId,
    DateOnly? VendidaEm,
    DateOnly? FaturadaEm,
    DateOnly? EntregueEm,
    DateTime? RegistradaNaOrigemEm,
    string? NumeroDoPedido,
    string? NumeroDaNotaFiscal,
    string? SituacaoNaOrigem,
    string? GestaoNaOrigem,
    bool VendaDireta,
    bool RepasseDireto,
    int? Quantidade,
    string LinhaNaOrigem,
    string ProdutoNaOrigem,
    string? EmpresaNaOrigem,
    string? UnidadeNaOrigem,
    string? UnidadeDoFaturamentoNaOrigem,
    string HashDaOrigem,
    string? Transformacoes);

/// <summary>
/// A VENDA de uma máquina — um evento com data, filial e comprador, separado da máquina e do
/// vínculo com o cliente (documento 35, seção 10).
///
/// <para><b>Uma máquina tem quantas vendas tiver.</b> No ART, cinco chassis aparecem em duas vendas
/// cada: a máquina nova e, depois, a mesma máquina revendida como usada, para outro comprador. As
/// duas vendas são fatos, e nenhuma é duplicata da outra.</para>
///
/// <para><b>O que a venda não guarda:</b> valor, custo, comissão, lucro e margem — fora do escopo da
/// integração e sensíveis. O vendedor também não: é nome de pessoa, sem vínculo com usuário do
/// CRM.</para>
/// </summary>
public sealed class VendaDeMaquina : EntidadeBase
{
    private VendaDeMaquina() { }

    /// <summary>A filial que vendeu — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>A filial que faturou, quando a origem declara.</summary>
    public int? EmpresaDoFaturamentoId { get; private set; }

    /// <summary>A máquina vendida.</summary>
    public long EquipamentoId { get; private set; }

    /// <summary>O cliente comprador NESTA venda. Não é o dono atual da máquina.</summary>
    public long CompradorId { get; private set; }

    /// <summary>
    /// Verdadeiro quando o comprador da origem NÃO é cliente do CRM e a venda entrou com o dono atual da máquina no
    /// Protheus no lugar dele (decisão 5 de 24/09/2026). É o que impede duas conclusões falsas: que o ART trocou o
    /// comprador, quando o comprador verdadeiro passa a existir no CRM; e que o ART e o Protheus concordam sobre o
    /// dono, quando quem está no lugar do comprador é o próprio dono do Protheus.
    /// </summary>
    public bool CompradorPeloDonoNoProtheus { get; private set; }

    /// <summary>O sistema de onde a venda veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O identificador do registro na origem.</summary>
    public string ChaveOrigem { get; private set; } = default!;

    /// <summary>A data da venda.</summary>
    public DateOnly? VendidaEm { get; private set; }

    /// <summary>A data do faturamento.</summary>
    public DateOnly? FaturadaEm { get; private set; }

    /// <summary>A data da entrega.</summary>
    public DateOnly? EntregueEm { get; private set; }

    /// <summary>Quando a venda foi aberta na origem (UTC como a origem registra).</summary>
    public DateTime? RegistradaNaOrigemEm { get; private set; }

    /// <summary>O número do pedido.</summary>
    public string? NumeroDoPedido { get; private set; }

    /// <summary>O número da nota fiscal de venda.</summary>
    public string? NumeroDaNotaFiscal { get; private set; }

    /// <summary>A situação como a origem escreve.</summary>
    public string? SituacaoNaOrigem { get; private set; }

    /// <summary>Varejo ou Grandes Contas, como a origem escreve — atributo da venda.</summary>
    public string? GestaoNaOrigem { get; private set; }

    /// <summary>Venda direta, segundo a origem.</summary>
    public bool VendaDireta { get; private set; }

    /// <summary>Repasse direto, segundo a origem.</summary>
    public bool RepasseDireto { get; private set; }

    /// <summary>A quantidade declarada na venda.</summary>
    public int? Quantidade { get; private set; }

    /// <summary>A linha, exatamente como a origem escreve.</summary>
    public string LinhaNaOrigem { get; private set; } = default!;

    /// <summary>O produto, exatamente como a origem escreve.</summary>
    public string ProdutoNaOrigem { get; private set; } = default!;

    /// <summary>A empresa da origem.</summary>
    public string? EmpresaNaOrigem { get; private set; }

    /// <summary>A unidade que vendeu, como a origem escreve.</summary>
    public string? UnidadeNaOrigem { get; private set; }

    /// <summary>A unidade que faturou, como a origem escreve.</summary>
    public string? UnidadeDoFaturamentoNaOrigem { get; private set; }

    /// <summary>O resumo do conteúdo lido na última leitura que mudou a venda.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>As transformações aplicadas na leitura (datas zeradas que ficaram vazias, por exemplo).</summary>
    public string? Transformacoes { get; private set; }

    /// <summary>Quando a venda entrou no CRM (UTC).</summary>
    public DateTime ImportadaEm { get; private set; }

    /// <summary>A última vez que a origem mudou esta venda (UTC).</summary>
    public DateTime? AtualizadaPelaOrigemEm { get; private set; }

    /// <summary>Registra uma venda lida da origem.</summary>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="chaveOrigem">O identificador na origem.</param>
    /// <param name="equipamentoId">A máquina.</param>
    /// <param name="compradorId">O cliente comprador.</param>
    /// <param name="dados">O conteúdo saneado.</param>
    /// <param name="importadaEm">O instante da importação.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    /// <param name="compradorPeloDonoNoProtheus">Se o comprador é o dono atual no Protheus no lugar do comprador da
    /// origem, que não é cliente do CRM.</param>
    public static VendaDeMaquina Registrar(
        int sistemaId,
        string chaveOrigem,
        long equipamentoId,
        long compradorId,
        DadosDaVendaNaOrigem dados,
        DateTime importadaEm,
        long criadoPorId,
        bool compradorPeloDonoNoProtheus = false)
    {
        if (string.IsNullOrWhiteSpace(chaveOrigem))
            throw new RegraDeNegocioViolada("Venda de origem sem identificador não é rastreável.");

        var venda = new VendaDeMaquina
        {
            SistemaId = sistemaId,
            ChaveOrigem = chaveOrigem.Trim(),
            EquipamentoId = equipamentoId,
            CompradorId = compradorId,
            CompradorPeloDonoNoProtheus = compradorPeloDonoNoProtheus,
            ImportadaEm = importadaEm,
            CriadoPorId = criadoPorId
        };

        venda.Aplicar(dados);
        return venda;
    }

    /// <summary>
    /// Aplica uma nova leitura da origem e devolve, campo a campo, o que mudou — para a trilha de
    /// auditoria. Nada muda quando o resumo do conteúdo é o mesmo.
    /// </summary>
    /// <param name="dados">O conteúdo saneado da nova leitura.</param>
    /// <param name="quando">O instante da leitura.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public IReadOnlyList<(string Campo, string? Anterior, string? Novo)> AtualizarDaOrigem(
        DadosDaVendaNaOrigem dados, DateTime quando, long usuarioId)
    {
        if (dados.HashDaOrigem == HashDaOrigem) return [];

        var antes = Retrato();
        Aplicar(dados);
        var depois = Retrato();

        AtualizadaPelaOrigemEm = quando;
        MarcarAlteracao(usuarioId);

        return [.. antes.Keys
            .Where(campo => antes[campo] != depois[campo])
            .Select(campo => (campo, antes[campo], depois[campo]))];
    }

    /// <summary>Aponta a venda para outra máquina, quando a origem corrige o chassi.</summary>
    /// <param name="equipamentoId">A máquina certa.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public void TrocarEquipamento(long equipamentoId, long usuarioId)
    {
        if (EquipamentoId == equipamentoId) return;
        EquipamentoId = equipamentoId;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>
    /// Aponta a venda para outro comprador, quando a origem corrige o documento — ou quando o comprador da origem
    /// passa a existir no CRM e toma o lugar do dono do Protheus.
    /// </summary>
    /// <param name="compradorId">O comprador certo.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    /// <param name="peloDonoNoProtheus">Se o comprador é o dono atual no Protheus no lugar do da origem.</param>
    public void TrocarComprador(long compradorId, long usuarioId, bool peloDonoNoProtheus = false)
    {
        if (CompradorId == compradorId && CompradorPeloDonoNoProtheus == peloDonoNoProtheus) return;
        CompradorId = compradorId;
        CompradorPeloDonoNoProtheus = peloDonoNoProtheus;
        MarcarAlteracao(usuarioId);
    }

    private void Aplicar(DadosDaVendaNaOrigem dados)
    {
        if (string.IsNullOrWhiteSpace(dados.LinhaNaOrigem) || string.IsNullOrWhiteSpace(dados.ProdutoNaOrigem))
            throw new RegraDeNegocioViolada("A venda preserva a linha e o produto da origem; os dois são obrigatórios.");

        EmpresaId = dados.EmpresaId;
        EmpresaDoFaturamentoId = dados.EmpresaDoFaturamentoId;
        VendidaEm = dados.VendidaEm;
        FaturadaEm = dados.FaturadaEm;
        EntregueEm = dados.EntregueEm;
        RegistradaNaOrigemEm = dados.RegistradaNaOrigemEm;
        NumeroDoPedido = dados.NumeroDoPedido;
        NumeroDaNotaFiscal = dados.NumeroDaNotaFiscal;
        SituacaoNaOrigem = dados.SituacaoNaOrigem;
        GestaoNaOrigem = dados.GestaoNaOrigem;
        VendaDireta = dados.VendaDireta;
        RepasseDireto = dados.RepasseDireto;
        Quantidade = dados.Quantidade;
        LinhaNaOrigem = dados.LinhaNaOrigem;
        ProdutoNaOrigem = dados.ProdutoNaOrigem;
        EmpresaNaOrigem = dados.EmpresaNaOrigem;
        UnidadeNaOrigem = dados.UnidadeNaOrigem;
        UnidadeDoFaturamentoNaOrigem = dados.UnidadeDoFaturamentoNaOrigem;
        HashDaOrigem = dados.HashDaOrigem;
        Transformacoes = dados.Transformacoes;
    }

    private Dictionary<string, string?> Retrato() => new(StringComparer.Ordinal)
    {
        [nameof(EmpresaId)] = EmpresaId.ToString(System.Globalization.CultureInfo.InvariantCulture),
        [nameof(EmpresaDoFaturamentoId)] = EmpresaDoFaturamentoId?.ToString(System.Globalization.CultureInfo.InvariantCulture),
        [nameof(VendidaEm)] = VendidaEm?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        [nameof(FaturadaEm)] = FaturadaEm?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        [nameof(EntregueEm)] = EntregueEm?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
        [nameof(NumeroDoPedido)] = NumeroDoPedido,
        [nameof(NumeroDaNotaFiscal)] = NumeroDaNotaFiscal,
        [nameof(SituacaoNaOrigem)] = SituacaoNaOrigem,
        [nameof(GestaoNaOrigem)] = GestaoNaOrigem,
        [nameof(VendaDireta)] = VendaDireta ? "Sim" : "Não",
        [nameof(RepasseDireto)] = RepasseDireto ? "Sim" : "Não",
        [nameof(Quantidade)] = Quantidade?.ToString(System.Globalization.CultureInfo.InvariantCulture),
        [nameof(LinhaNaOrigem)] = LinhaNaOrigem,
        [nameof(ProdutoNaOrigem)] = ProdutoNaOrigem,
        [nameof(UnidadeNaOrigem)] = UnidadeNaOrigem,
        [nameof(UnidadeDoFaturamentoNaOrigem)] = UnidadeDoFaturamentoNaOrigem
    };
}

/// <summary>A natureza da ligação entre um cliente e uma máquina.</summary>
public enum NaturezaDoVinculoComEquipamento
{
    /// <summary>
    /// O cliente comprou a máquina NUMA venda, naquela data. Não prova que ele é o dono hoje.
    /// </summary>
    CompradorNaVenda = 0,

    /// <summary>
    /// O cliente é o DONO ATUAL da máquina, segundo a sincronia do parque (decisão de 24/09/2026): o proprietário
    /// atual no cadastro de veículos do Protheus, ou o comprador do ART quando ele prevalece. Um só vigente por
    /// máquina; quando o dono muda, o anterior é encerrado — nunca apagado — e fica como histórico.
    /// </summary>
    ProprietarioAtual = 1
}

/// <summary>
/// O QUE SUSTENTA O DONO ATUAL — gravado no vínculo, para a tela filtrar e mostrar com que firmeza o CRM afirma quem
/// tem a máquina (decisão de 24/09/2026).
/// </summary>
public enum EvidenciaDoProprietario
{
    /// <summary>A nota de venda válida mais recente da máquina no Protheus é para este cliente.</summary>
    NotaDeVenda = 0,

    /// <summary>Sem nota de venda para ele, mas a ordem de serviço mais recente da oficina é dele.</summary>
    OrdemDeServico = 1,

    /// <summary>
    /// Só o cadastro: nem nota de venda nem ordem de serviço apontam para ele — o dono veio de uma carga antiga do
    /// Protheus e ninguém o confirmou depois.
    /// </summary>
    CadastroAntigo = 2,

    /// <summary>
    /// A venda no ART: o comprador do ART prevaleceu sobre o Protheus (sem evidência posterior, ou com o Protheus
    /// apontando a própria Tracbel), ou o Protheus não tem a máquina.
    /// </summary>
    VendaNoArt = 3
}

/// <summary>
/// A LIGAÇÃO entre um cliente e uma máquina, com natureza, origem e data — separada da posse.
///
/// <para><b>Por que não basta <c>Equipamento.ClienteId</c>:</b> ele diz quem é o dono atual, uma
/// afirmação só. Uma máquina revendida teve dois compradores em datas diferentes, e o ART não diz
/// quem a tem hoje. O vínculo registra cada afirmação com a sua natureza, e a posse continua sendo
/// decisão de quem confirma.</para>
///
/// <para><b>Quem tem a máquina hoje</b> é o vínculo <see cref="NaturezaDoVinculoComEquipamento.ProprietarioAtual"/>,
/// um só vigente por máquina, com a evidência que o sustenta (decisão de 24/09/2026). Ele não substitui o comprador de
/// cada venda: os dois convivem, e o comprador fica como histórico.</para>
/// </summary>
public sealed class VinculoDeClienteComEquipamento : EntidadeBase
{
    private VinculoDeClienteComEquipamento() { }

    /// <summary>A filial da afirmação — a da venda, para o comprador.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O cliente.</summary>
    public long ClienteId { get; private set; }

    /// <summary>A máquina.</summary>
    public long EquipamentoId { get; private set; }

    /// <summary>A natureza da ligação.</summary>
    public NaturezaDoVinculoComEquipamento Natureza { get; private set; }

    /// <summary>A venda que sustenta a ligação, quando a natureza é de comprador.</summary>
    public long? VendaDeMaquinaId { get; private set; }

    /// <summary>O sistema de onde a afirmação veio. Nulo é afirmação feita no CRM.</summary>
    public int? SistemaId { get; private set; }

    /// <summary>
    /// A data de referência — a data da venda, para o comprador; a data da evidência mais recente, para o dono atual.
    /// </summary>
    public DateOnly? ReferenciaEm { get; private set; }

    /// <summary>O que sustenta o dono atual. Nulo no vínculo de comprador — lá quem sustenta é a venda.</summary>
    public EvidenciaDoProprietario? Evidencia { get; private set; }

    /// <summary>Quando a ligação deixou de valer (UTC). Nula enquanto vale.</summary>
    public DateTime? EncerradoEm { get; private set; }

    /// <summary>Por que a ligação deixou de valer.</summary>
    public string? MotivoDoEncerramento { get; private set; }

    /// <summary>Registra o comprador de uma venda.</summary>
    /// <param name="empresaId">A filial da venda.</param>
    /// <param name="clienteId">O comprador.</param>
    /// <param name="equipamentoId">A máquina.</param>
    /// <param name="vendaDeMaquinaId">A venda.</param>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="vendidaEm">A data da venda.</param>
    /// <param name="criadoPorId">Quem roda a integração.</param>
    public static VinculoDeClienteComEquipamento RegistrarCompradorNaVenda(
        int empresaId,
        long clienteId,
        long equipamentoId,
        long vendaDeMaquinaId,
        int sistemaId,
        DateOnly? vendidaEm,
        long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        ClienteId = clienteId,
        EquipamentoId = equipamentoId,
        Natureza = NaturezaDoVinculoComEquipamento.CompradorNaVenda,
        VendaDeMaquinaId = vendaDeMaquinaId,
        SistemaId = sistemaId,
        ReferenciaEm = vendidaEm,
        CriadoPorId = criadoPorId
    };

    /// <summary>
    /// Registra o DONO ATUAL de uma máquina — a afirmação da sincronia do parque, com a evidência e a data dela. Não
    /// aponta venda: o comprador de cada venda continua no vínculo dele.
    /// </summary>
    /// <param name="empresaId">A filial do cliente — a fronteira de acesso.</param>
    /// <param name="clienteId">O dono atual.</param>
    /// <param name="equipamentoId">A máquina.</param>
    /// <param name="sistemaId">O sistema que sustenta a afirmação (o Protheus, ou o ART quando ele prevalece).</param>
    /// <param name="referenciaEm">A data da evidência mais recente.</param>
    /// <param name="evidencia">O que sustenta a afirmação.</param>
    /// <param name="criadoPorId">Quem roda a sincronia.</param>
    public static VinculoDeClienteComEquipamento RegistrarProprietarioAtual(
        int empresaId,
        long clienteId,
        long equipamentoId,
        int sistemaId,
        DateOnly? referenciaEm,
        EvidenciaDoProprietario evidencia,
        long criadoPorId) => new()
    {
        EmpresaId = empresaId,
        ClienteId = clienteId,
        EquipamentoId = equipamentoId,
        Natureza = NaturezaDoVinculoComEquipamento.ProprietarioAtual,
        SistemaId = sistemaId,
        ReferenciaEm = referenciaEm,
        Evidencia = evidencia,
        CriadoPorId = criadoPorId
    };

    /// <summary>
    /// Acompanha a evidência, a data, o sistema e a filial do dono atual quando a origem os muda — o dono é o
    /// mesmo. Nada muda quando tudo é igual, e é isso que deixa a trilha limpa numa rodada sem novidade.
    /// </summary>
    /// <param name="empresaId">A filial do cliente.</param>
    /// <param name="sistemaId">O sistema que sustenta a afirmação.</param>
    /// <param name="referenciaEm">A data da evidência mais recente.</param>
    /// <param name="evidencia">O que sustenta a afirmação.</param>
    /// <param name="usuarioId">Quem roda a sincronia.</param>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AcompanharProprietario(int empresaId, int sistemaId, DateOnly? referenciaEm, EvidenciaDoProprietario evidencia, long usuarioId)
    {
        if (Natureza != NaturezaDoVinculoComEquipamento.ProprietarioAtual)
            throw new RegraDeNegocioViolada("Só o vínculo de dono atual acompanha evidência.");

        if (EmpresaId == empresaId && SistemaId == sistemaId && ReferenciaEm == referenciaEm && Evidencia == evidencia) return false;

        EmpresaId = empresaId;
        SistemaId = sistemaId;
        ReferenciaEm = referenciaEm;
        Evidencia = evidencia;
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>Acompanha a data e a filial da venda quando a origem as corrige.</summary>
    /// <param name="empresaId">A filial da venda.</param>
    /// <param name="referenciaEm">A data da venda.</param>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public void AcompanharVenda(int empresaId, DateOnly? referenciaEm, long usuarioId)
    {
        if (EmpresaId == empresaId && ReferenciaEm == referenciaEm) return;
        EmpresaId = empresaId;
        ReferenciaEm = referenciaEm;
        MarcarAlteracao(usuarioId);
    }

    /// <summary>Encerra a ligação, sem apagar — o motivo fica.</summary>
    /// <param name="motivo">Por quê.</param>
    /// <param name="quando">O instante.</param>
    /// <param name="usuarioId">Quem encerra.</param>
    public void Encerrar(string motivo, DateTime quando, long usuarioId)
    {
        if (EncerradoEm is not null) return;
        if (string.IsNullOrWhiteSpace(motivo))
            throw new RegraDeNegocioViolada("Vínculo encerrado precisa do motivo.");

        EncerradoEm = quando;
        MotivoDoEncerramento = motivo.Trim();
        MarcarAlteracao(usuarioId);
    }
}
