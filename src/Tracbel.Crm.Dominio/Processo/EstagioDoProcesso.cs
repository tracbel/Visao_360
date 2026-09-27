using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Processo;

/// <summary>
/// OS SEIS ESTÁGIOS DO FUNIL, na ordem — os rótulos do extrator do BI (<c>0- Lead</c> … <c>5- Faturamento</c>).
///
/// <para><b>A ordem é a regra.</b> O estágio é cumulativo (decisão de 27/09/2026): quem alcançou um estágio alcançou
/// todos os anteriores. Por isso o valor numérico importa, e nenhum estágio novo entra no meio.</para>
/// </summary>
public enum EstagioDoFunil
{
    /// <summary>O primeiro contato — pela entrada digital (1278) ou por qualquer estágio acima.</summary>
    Lead = 0,

    /// <summary>O lead qualificado — pela entrada digital (3803) ou por qualquer estágio acima.</summary>
    Qualificado = 1,

    /// <summary>Visita, apresentação ou contato comercial registrado.</summary>
    Cobertura = 2,

    /// <summary>Proposta, negociação ou consórcio.</summary>
    Negociacao = 3,

    /// <summary>Pedido — venda aprovada.</summary>
    Pedido = 4,

    /// <summary>Faturado ou entregue.</summary>
    Faturamento = 5
}

/// <summary>
/// O ESTADO QUE A ORIGEM DECLARA para uma linha do funil — tudo o que a rotina compara para decidir se a linha mudou.
/// As datas chegam truncadas no milissegundo (ver <see cref="EstagioDoProcesso.NoMilissegundo(DateTime)"/>).
/// </summary>
/// <param name="EmpresaId">A filial do processo no Vórtice (<c>IV_PROCDADO.NroEmpresa</c>), já no CRM.</param>
/// <param name="NumeroDoProcessoNaOrigem">O número do processo no Vórtice.</param>
/// <param name="TipoDeProcessoNaOrigem">31, 41 ou 50.</param>
/// <param name="ClienteId">O cliente do CRM que casou pelo documento; nulo para o prospect.</param>
/// <param name="CarteiraId">A carteira MAQ_NOVOS da pessoa no CRM, quando a sincronia das carteiras já a trouxe.</param>
/// <param name="ResponsavelId">A conta do responsável pelo processo, ou do dono da carteira.</param>
/// <param name="AbertoEm">Quando o processo foi aberto (UTC).</param>
/// <param name="AberturaDeduzida">Se a abertura veio do primeiro andamento, porque a inclusão é nula.</param>
/// <param name="Desfecho">A situação do processo na origem.</param>
/// <param name="DesfechoEm">Quando o processo foi encerrado na origem (UTC).</param>
/// <param name="Estagio">O estágio.</param>
/// <param name="AlcancadoEm">O primeiro resultado aceito que alcança o estágio (UTC).</param>
/// <param name="UltimaAcaoDaEtapaEm">O <c>DTA_ETAPA</c> do BI, só para conciliação (UTC).</param>
/// <param name="NumeroDoProcessoDnaNaOrigem">O processo DNA de onde veio o resultado que abriu o estágio; nulo quando foi o próprio.</param>
/// <param name="ResultadoQueAbriu">O código de resultado que abriu o estágio.</param>
/// <param name="PelaEntradaDigital">Se o estágio foi alcançado pela entrada digital (ver <see cref="EstagioApurado"/>).</param>
public sealed record RetratoDoEstagio(
    int EmpresaId,
    long NumeroDoProcessoNaOrigem,
    short TipoDeProcessoNaOrigem,
    long? ClienteId,
    long? CarteiraId,
    long? ResponsavelId,
    DateTime AbertoEm,
    bool AberturaDeduzida,
    SituacaoDoProcesso Desfecho,
    DateTime? DesfechoEm,
    EstagioDoFunil Estagio,
    DateTime AlcancadoEm,
    DateTime? UltimaAcaoDaEtapaEm,
    long? NumeroDoProcessoDnaNaOrigem,
    int ResultadoQueAbriu,
    bool PelaEntradaDigital);

/// <summary>
/// UMA LINHA DO FUNIL: um processo do Vórtice num estágio que ele alcançou (decisões de 27/09/2026, documento 52).
///
/// <para><b>Não depende de <see cref="Processo"/> nem de cliente.</b> 62% do funil é de prospect sem cadastro no CRM
/// (medido em 27/09/2026), e o funil precisa deles. As ligações — processo, cliente, carteira, responsável — são
/// opcionais: a linha existe pelo que o Vórtice declara, e se liga ao que o CRM já tem. A rotina não cria cadastro
/// nenhum para ligar.</para>
///
/// <para><b>Uma linha por processo por estágio</b>, e é o índice único que garante. O BI tem um registro por
/// departamento do histórico da família DNA — 1,8 a 3,4 linhas por processo em cada estágio (medido em
/// 27/09/2026) —; aqui não há esse efeito.</para>
///
/// <para><b>Fato da origem, sincronizado.</b> A linha nasce, muda e sai pela rotina <c>PROCESSOS_VORTICE</c>; não
/// há edição pela tela. Por isso não herda <c>EntidadeBase</c>: não tem autoria nem exclusão lógica — o estágio que
/// some da origem sai daqui, e a trilha de auditoria guarda o que era.</para>
/// </summary>
public sealed class EstagioDoProcesso
{
    private EstagioDoProcesso() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A filial do processo — a fronteira de acesso, com o filtro global de empresa.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O número do processo no Vórtice — a chave combinada com a API Gestão de Negócios.</summary>
    public long NumeroDoProcessoNaOrigem { get; private set; }

    /// <summary>O tipo de processo no Vórtice: 31, 41 ou 50.</summary>
    public short TipoDeProcessoNaOrigem { get; private set; }

    /// <summary>O processo do CRM — nulo até a onda 2 carregar os processos dos clientes casados.</summary>
    public long? ProcessoId { get; private set; }

    /// <summary>O cliente do CRM; nulo para o prospect.</summary>
    public long? ClienteId { get; private set; }

    /// <summary>A carteira MAQ_NOVOS da pessoa no CRM.</summary>
    public long? CarteiraId { get; private set; }

    /// <summary>A conta do responsável pelo processo (<c>UsuResponsavel</c>), senão a do dono da carteira.</summary>
    public long? ResponsavelId { get; private set; }

    /// <summary>Quando o processo foi aberto: <c>COALESCE(DtaInclusao, 1º andamento)</c> (UTC).</summary>
    public DateTime AbertoEm { get; private set; }

    /// <summary>Se a abertura foi deduzida do primeiro andamento, porque a inclusão é nula na origem.</summary>
    public bool AberturaDeduzida { get; private set; }

    /// <summary>A situação do processo na origem.</summary>
    public SituacaoDoProcesso Desfecho { get; private set; }

    /// <summary>Quando o processo foi encerrado na origem (UTC).</summary>
    public DateTime? DesfechoEm { get; private set; }

    /// <summary>O estágio.</summary>
    public EstagioDoFunil Estagio { get; private set; }

    /// <summary>O primeiro resultado aceito que alcança o estágio, do processo ou do pai DNA (UTC).</summary>
    public DateTime AlcancadoEm { get; private set; }

    /// <summary>
    /// O <c>DTA_ETAPA</c> do BI: a última ação geradora da etapa no próprio processo. Só para conciliar com o BI —
    /// é mal definido (na Cobertura, 23% ficam sem data e 50% diferem de <see cref="AlcancadoEm"/> em mais de 30
    /// dias, medido em 27/09/2026) e não é a data da etapa.
    /// </summary>
    public DateTime? UltimaAcaoDaEtapaEm { get; private set; }

    /// <summary>Se o resultado que abriu o estágio veio do processo pai (DNA), e não do próprio.</summary>
    public bool HerdadoDoProcessoDna { get; private set; }

    /// <summary>O processo DNA de onde veio o resultado que abriu o estágio — o "herdado do processo nº X" do selo.</summary>
    public long? NumeroDoProcessoDnaNaOrigem { get; private set; }

    /// <summary>O código de resultado do histórico que abriu o estágio.</summary>
    public int ResultadoQueAbriu { get; private set; }

    /// <summary>Se o estágio foi alcançado pela entrada digital (1278/3803).</summary>
    public bool PelaEntradaDigital { get; private set; }

    /// <summary>O que a linha guarda, no formato em que a origem é comparada.</summary>
    public RetratoDoEstagio Retrato => new(
        EmpresaId, NumeroDoProcessoNaOrigem, TipoDeProcessoNaOrigem, ClienteId, CarteiraId, ResponsavelId, AbertoEm,
        AberturaDeduzida, Desfecho, DesfechoEm, Estagio, AlcancadoEm, UltimaAcaoDaEtapaEm, NumeroDoProcessoDnaNaOrigem,
        ResultadoQueAbriu, PelaEntradaDigital);

    /// <summary>Registra a linha que a origem declarou.</summary>
    /// <param name="retrato">O estado declarado.</param>
    public static EstagioDoProcesso Registrar(RetratoDoEstagio retrato)
    {
        var linha = new EstagioDoProcesso
        {
            NumeroDoProcessoNaOrigem = retrato.NumeroDoProcessoNaOrigem,
            Estagio = retrato.Estagio
        };
        linha.Aplicar(retrato);
        return linha;
    }

    /// <summary>Acompanha a origem. Devolve se alguma coisa mudou.</summary>
    /// <param name="retrato">O estado declarado nesta rodada — do mesmo processo e do mesmo estágio.</param>
    public bool AtualizarDaOrigem(RetratoDoEstagio retrato)
    {
        // A CHAVE NÃO MUDA: processo e estágio são a identidade da linha. Outro processo ou outro estágio é outra linha.
        if (retrato.NumeroDoProcessoNaOrigem != NumeroDoProcessoNaOrigem || retrato.Estagio != Estagio)
            throw new RegraDeNegocioViolada(
                "A linha do funil não troca de processo nem de estágio: outro processo ou outro estágio é outra linha.");

        var normalizado = Normalizar(retrato);
        if (Retrato == normalizado) return false;

        Aplicar(normalizado);
        return true;
    }

    /// <summary>
    /// A DATA NO MILISSEGUNDO. A coluna é <c>datetime2(3)</c>, e o Vórtice grava <c>datetime</c> com passo de 1/300 s:
    /// sem truncar aqui, a data relida da origem nunca seria igual à gravada, e toda rodada "alteraria" o funil
    /// inteiro sem nada ter mudado.
    /// </summary>
    /// <param name="data">A data.</param>
    public static DateTime NoMilissegundo(DateTime data) =>
        new(data.Ticks - (data.Ticks % TimeSpan.TicksPerMillisecond), data.Kind);

    /// <inheritdoc cref="NoMilissegundo(DateTime)"/>
    public static DateTime? NoMilissegundo(DateTime? data) => data is { } valor ? NoMilissegundo(valor) : null;

    /// <summary>O retrato com as datas no milissegundo — o que a coluna guarda e o que se compara.</summary>
    /// <param name="r">O retrato como veio.</param>
    public static RetratoDoEstagio Normalizar(RetratoDoEstagio r) => r with
    {
        AbertoEm = NoMilissegundo(r.AbertoEm),
        DesfechoEm = NoMilissegundo(r.DesfechoEm),
        AlcancadoEm = NoMilissegundo(r.AlcancadoEm),
        UltimaAcaoDaEtapaEm = NoMilissegundo(r.UltimaAcaoDaEtapaEm)
    };

    private void Aplicar(RetratoDoEstagio r)
    {
        if (r.TipoDeProcessoNaOrigem is not (31 or 41 or 50))
            throw new RegraDeNegocioViolada($"O funil é dos processos 31, 41 e 50 — não do tipo {r.TipoDeProcessoNaOrigem}.");

        EmpresaId = r.EmpresaId;
        TipoDeProcessoNaOrigem = r.TipoDeProcessoNaOrigem;
        ClienteId = r.ClienteId;
        CarteiraId = r.CarteiraId;
        ResponsavelId = r.ResponsavelId;
        AbertoEm = NoMilissegundo(r.AbertoEm);
        AberturaDeduzida = r.AberturaDeduzida;
        Desfecho = r.Desfecho;
        DesfechoEm = NoMilissegundo(r.DesfechoEm);
        AlcancadoEm = NoMilissegundo(r.AlcancadoEm);
        UltimaAcaoDaEtapaEm = NoMilissegundo(r.UltimaAcaoDaEtapaEm);
        NumeroDoProcessoDnaNaOrigem = r.NumeroDoProcessoDnaNaOrigem;
        HerdadoDoProcessoDna = r.NumeroDoProcessoDnaNaOrigem is not null;
        ResultadoQueAbriu = r.ResultadoQueAbriu;
        PelaEntradaDigital = r.PelaEntradaDigital;
    }
}
