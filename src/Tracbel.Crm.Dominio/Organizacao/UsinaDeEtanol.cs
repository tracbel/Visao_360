using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// UMA USINA DE ETANOL AUTORIZADA PELA ANP, no município onde ela fica.
///
/// <para><b>Por que existe.</b> Usina é o que faz a cana virar demanda de máquina pesada num raio de
/// dezenas de quilômetros: onde há usina, há colheita mecanizada, transbordo e frota. A pasta do
/// comercial trazia uma lista de 68 municípios "com usina", sem fonte nem data; aqui ela vira dado
/// com procedência, CNPJ e <b>tamanho</b>.</para>
///
/// <para><b>Por que a ANP, e não o MAPA.</b> O cadastro de produtores de cana do MAPA (SAPCana) é
/// diário e mais completo, mas o download dele exige CAPTCHA — medido em 20/09/2026 —, e contornar um
/// controle de acesso deliberado não é caminho. A ANP autoriza e fiscaliza todo produtor de etanol do
/// país e publica a base do painel como dado aberto, sem barreira.</para>
///
/// <para><b>O que esta fonte não enxerga:</b> usina que produz só açúcar, sem etanol, não é
/// autorizada pela ANP e não aparece. Quase toda usina paulista é mista, mas <b>a ausência de um
/// município nesta tabela não prova que não há usina lá</b> — prova que não há usina de etanol. Quem
/// escrever a tela diz isso em voz alta, como o resto da Visão 360 faz com dado que não fecha.</para>
///
/// <para><b>Não é cliente.</b> Esta tabela descreve o território, não a carteira: o fato de uma usina
/// existir não diz nada sobre a Tracbel atendê-la. O cruzamento com o cadastro de clientes, se
/// alguém o quiser, é outra coisa e tem o CNPJ para fazê-lo.</para>
/// </summary>
public sealed class UsinaDeEtanol
{
    private UsinaDeEtanol() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>
    /// O CNPJ do ESTABELECIMENTO, com 14 dígitos e sem pontuação.
    ///
    /// <para>É o estabelecimento, e não a empresa: um mesmo grupo tem várias usinas, cada uma no seu
    /// município e com a sua capacidade. A chave do CRM segue o CNPJ completo por isso.</para>
    /// </summary>
    public string Cnpj { get; private set; } = default!;

    /// <summary>A razão social, como a ANP a publica.</summary>
    public string RazaoSocial { get; private set; } = default!;

    /// <summary>O município onde a usina está.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O mês da autorização que esta linha reflete — o mais recente publicado pela ANP.</summary>
    public DateOnly MesDeReferencia { get; private set; }

    /// <summary>Capacidade autorizada de etanol anidro, em m³/dia. Nulo é não informado.</summary>
    public int? CapacidadeDeAnidroM3Dia { get; private set; }

    /// <summary>Capacidade autorizada de etanol hidratado, em m³/dia. Nulo é não informado.</summary>
    public int? CapacidadeDeHidratadoM3Dia { get; private set; }

    /// <summary>Quando a carga gravou ou conferiu a linha (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>
    /// QUANDO A USINA SAIU DA LISTA DA ANP (UTC). Nulo é usina vigente.
    ///
    /// <para><b>Sair da lista não apaga a linha</b> (issue 153). Até 22/09/2026 a usina que perdia a autorização era
    /// apagada, com o argumento de que o histórico estava na ANP — mas a ANP publica só o cadastro de hoje, e uma usina
    /// que o CRM mostrou em agosto sumia sem rastro em setembro. Agora ela fica encerrada: some da contagem de usinas
    /// vigentes e continua consultável, com a data em que saiu. Se voltar à lista, reabre.</para>
    /// </summary>
    public DateTime? EncerradaEm { get; private set; }

    /// <summary>Se a usina está na lista mais recente da ANP.</summary>
    public bool EstaVigente => EncerradaEm is null;

    /// <summary>
    /// A capacidade total autorizada, somando anidro e hidratado.
    ///
    /// <para>Nulo só quando as duas são nulas: uma usina que informou uma e não a outra tem
    /// capacidade conhecida, e tratá-la como desconhecida a esconderia do ranking.</para>
    /// </summary>
    public int? CapacidadeTotalM3Dia =>
        CapacidadeDeAnidroM3Dia is null && CapacidadeDeHidratadoM3Dia is null
            ? null
            : (CapacidadeDeAnidroM3Dia ?? 0) + (CapacidadeDeHidratadoM3Dia ?? 0);

    /// <summary>Registra uma usina.</summary>
    /// <param name="cnpj">O CNPJ do estabelecimento, só dígitos.</param>
    /// <param name="razaoSocial">A razão social publicada pela ANP.</param>
    /// <param name="municipioId">O município reconhecido.</param>
    /// <param name="mesDeReferencia">O mês da autorização.</param>
    /// <param name="capacidadeDeAnidroM3Dia">Capacidade de anidro, ou nulo.</param>
    /// <param name="capacidadeDeHidratadoM3Dia">Capacidade de hidratado, ou nulo.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando o CNPJ, a razão social ou a capacidade não valem.</exception>
    public static UsinaDeEtanol Registrar(
        string cnpj, string razaoSocial, int municipioId, DateOnly mesDeReferencia,
        int? capacidadeDeAnidroM3Dia, int? capacidadeDeHidratadoM3Dia,
        long importadoPorId, DateTime agoraUtc)
    {
        var limpo = ApenasDigitos(cnpj);

        if (limpo.Length != 14)
            throw new RegraDeNegocioViolada($"O CNPJ da usina precisa de 14 dígitos; veio \"{cnpj}\".");

        if (string.IsNullOrWhiteSpace(razaoSocial))
            throw new RegraDeNegocioViolada("A usina precisa da razão social publicada pela ANP.");

        ConferirCapacidade(capacidadeDeAnidroM3Dia, capacidadeDeHidratadoM3Dia);

        return new UsinaDeEtanol
        {
            Cnpj = limpo,
            RazaoSocial = razaoSocial.Trim(),
            MunicipioId = municipioId,
            MesDeReferencia = mesDeReferencia,
            CapacidadeDeAnidroM3Dia = capacidadeDeAnidroM3Dia,
            CapacidadeDeHidratadoM3Dia = capacidadeDeHidratadoM3Dia,
            ImportadoEm = agoraUtc,
            ImportadoPorId = importadoPorId
        };
    }

    /// <summary>
    /// Confere a usina contra uma nova leitura e diz se alguma coisa mudou.
    ///
    /// <para>Inclusive o município: uma usina não se muda de cidade, mas a ANP corrige grafia, e a
    /// correção precisa chegar.</para>
    /// </summary>
    /// <param name="razaoSocial">A razão social da nova leitura.</param>
    /// <param name="municipioId">O município da nova leitura.</param>
    /// <param name="mesDeReferencia">O mês da nova leitura.</param>
    /// <param name="capacidadeDeAnidroM3Dia">Capacidade de anidro.</param>
    /// <param name="capacidadeDeHidratadoM3Dia">Capacidade de hidratado.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando a capacidade não vale.</exception>
    public bool Reapurar(
        string razaoSocial, int municipioId, DateOnly mesDeReferencia,
        int? capacidadeDeAnidroM3Dia, int? capacidadeDeHidratadoM3Dia,
        long importadoPorId, DateTime agoraUtc)
    {
        ConferirCapacidade(capacidadeDeAnidroM3Dia, capacidadeDeHidratadoM3Dia);

        var mudou = RazaoSocial != razaoSocial.Trim()
                    || MunicipioId != municipioId
                    || MesDeReferencia != mesDeReferencia
                    || CapacidadeDeAnidroM3Dia != capacidadeDeAnidroM3Dia
                    || CapacidadeDeHidratadoM3Dia != capacidadeDeHidratadoM3Dia
                    || EncerradaEm is not null;

        RazaoSocial = razaoSocial.Trim();
        MunicipioId = municipioId;
        MesDeReferencia = mesDeReferencia;
        CapacidadeDeAnidroM3Dia = capacidadeDeAnidroM3Dia;
        CapacidadeDeHidratadoM3Dia = capacidadeDeHidratadoM3Dia;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;

        // A USINA QUE VOLTA À LISTA REABRE — a trilha guarda quando ela tinha saído.
        EncerradaEm = null;

        return mudou;
    }

    /// <summary>
    /// A usina saiu da lista da ANP: fica encerrada, sem apagar a linha. Devolve se mudou — encerrar de novo o que já
    /// está encerrado não muda nada, nem a data.
    /// </summary>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public bool Encerrar(long importadoPorId, DateTime agoraUtc)
    {
        if (EncerradaEm is not null) return false;

        EncerradaEm = agoraUtc;
        ImportadoEm = agoraUtc;
        ImportadoPorId = importadoPorId;
        return true;
    }

    /// <summary>
    /// ENCERRA AS USINAS QUE SAÍRAM DA LISTA DA ANP e devolve as que mudaram.
    ///
    /// <para><b>"Na lista" é o CNPJ que veio no arquivo, recusado ou não.</b> Uma linha recusada (município ambíguo,
    /// mês fora do formato) continua sendo uma usina autorizada — só não pôde ser gravada desta vez.</para>
    ///
    /// <para><b>Lista vazia não encerra nada.</b> A ANP não fica sem nenhuma usina em São Paulo de um mês para o outro;
    /// lista vazia é arquivo que não veio inteiro, e encerrar todas por isso seria inventar um fato.</para>
    /// </summary>
    /// <param name="existentes">As usinas do banco.</param>
    /// <param name="cnpjsNaLista">Os CNPJs, só dígitos, que a leitura trouxe.</param>
    /// <param name="importadoPorId">Quem rodou a carga.</param>
    /// <param name="agoraUtc">O instante da carga.</param>
    public static IReadOnlyList<UsinaDeEtanol> EncerrarAsQueSairam(
        IEnumerable<UsinaDeEtanol> existentes, IReadOnlySet<string> cnpjsNaLista, long importadoPorId, DateTime agoraUtc) =>
        cnpjsNaLista.Count == 0
            ? []
            : [.. existentes.Where(u => !cnpjsNaLista.Contains(u.Cnpj)).Where(u => u.Encerrar(importadoPorId, agoraUtc))];

    /// <summary>Só dígitos — a ANP publica o CNPJ sem pontuação, mas isso pode mudar.</summary>
    /// <param name="texto">O CNPJ como veio.</param>
    public static string ApenasDigitos(string texto) =>
        new([.. (texto ?? string.Empty).Where(char.IsAsciiDigit)]);

    private static void ConferirCapacidade(int? anidro, int? hidratado)
    {
        // CAPACIDADE ZERO EXISTE e não é erro: em 07/2026 uma das 145 usinas paulistas estava com as
        // duas zeradas — usina parada, autorização viva. Negativa é que não existe.
        if (anidro < 0 || hidratado < 0)
            throw new RegraDeNegocioViolada("A capacidade de produção de uma usina não é negativa.");
    }
}
