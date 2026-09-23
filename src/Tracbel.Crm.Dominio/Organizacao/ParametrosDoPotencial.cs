using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

/// <summary>
/// UM PARÂMETRO DO POTENCIAL DE MERCADO COM VIGÊNCIA — a base da regra por cultura, dos parâmetros
/// gerais e da percepção do gestor (issue 71, documento 48 §7, fase P2).
///
/// <para><b>Nada se altera no lugar.</b> Mudar um parâmetro é registrar uma vigência NOVA, com data de
/// início, autor e justificativa; a anterior continua gravada e continua valendo para as datas em que
/// valia. É o que o aceite da issue pede: "o cálculo de uma data passada usa o parâmetro vigente naquela
/// data". Um valor sobrescrito faria o potencial de março mudar em setembro sem ninguém ter mexido em março.</para>
///
/// <para><b>O passado não se reescreve.</b> Por isso a vigência começa hoje ou depois, e só se revoga o que
/// ainda não passou de hoje: o que já valeu num dia que acabou fica como valeu. Parâmetro errado se corrige
/// com uma vigência nova, a partir de hoje — e a trilha mostra as duas.</para>
///
/// <para><b>"Hoje" é o dia de São Paulo</b> (<see cref="HojeNoBrasil"/>): uma vigência registrada às 22h de
/// 30/09 é de 30/09, e não de 01/10 como diria o relógio UTC.</para>
/// </summary>
public abstract class ParametroComVigencia
{
    /// <summary>O tamanho máximo da justificativa e do motivo da revogação.</summary>
    public const int TamanhoDoTexto = 400;

    /// <summary>Identificador interno.</summary>
    public int Id { get; protected set; }

    /// <summary>O primeiro dia em que o valor vale. Vale até o dia anterior à vigência seguinte.</summary>
    public DateOnly VigenteDesde { get; protected set; }

    /// <summary>Por que este valor — a decisão, a fonte ou a conversa de onde ele veio.</summary>
    public string Justificativa { get; protected set; } = default!;

    /// <summary>
    /// Quem registrou. <b>Nulo só na semente da migração</b>: ali quem informou é o commit revisado, e
    /// nenhum usuário do CRM digitou o valor. Toda vigência gravada pelo CRM tem autor.
    /// </summary>
    public long? InformadoPorId { get; protected set; }

    /// <summary>Quando foi registrado (UTC).</summary>
    public DateTime InformadoEm { get; protected set; }

    /// <summary>Quando a vigência foi revogada. Nulo é vigência de pé.</summary>
    public DateTime? RevogadoEm { get; protected set; }

    /// <summary>Quem revogou.</summary>
    public long? RevogadoPorId { get; protected set; }

    /// <summary>Por que foi revogada.</summary>
    public string? MotivoDaRevogacao { get; protected set; }

    /// <summary>
    /// O dia de hoje em São Paulo. O Brasil não tem horário de verão desde 2019, e São Paulo fica em
    /// UTC−3 o ano inteiro — a conta é fixa, sem depender do banco de fusos do sistema operacional, que o
    /// servidor e o contêiner dos testes não garantem ter igual.
    /// </summary>
    /// <param name="agoraUtc">O instante, em UTC.</param>
    public static DateOnly HojeNoBrasil(DateTime agoraUtc) => DateOnly.FromDateTime(agoraUtc.AddHours(-3));

    /// <summary>
    /// A vigência que vale numa data: a de início mais recente até a data, ignorando as revogadas. Nula
    /// quando nada valia ainda — e aí o cálculo fica vazio com o motivo, nunca com um valor inventado.
    /// </summary>
    /// <typeparam name="T">O tipo do parâmetro.</typeparam>
    /// <param name="vigencias">As vigências de UMA chave (um produto, um município, ou o conjunto geral).</param>
    /// <param name="data">A data do cálculo.</param>
    public static T? VigenteEm<T>(IEnumerable<T> vigencias, DateOnly data) where T : ParametroComVigencia =>
        vigencias.Where(v => v.RevogadoEm is null && v.VigenteDesde <= data).MaxBy(v => v.VigenteDesde);

    /// <summary>Revoga uma vigência que ainda não passou de hoje.</summary>
    /// <param name="motivo">Por que — fica na trilha.</param>
    /// <param name="revogadoPorId">Quem revoga.</param>
    /// <param name="agoraUtc">O instante da revogação.</param>
    /// <exception cref="RegraDeNegocioViolada">Quando já foi revogada, quando começou antes de hoje, ou sem motivo ou autor.</exception>
    public void Revogar(string motivo, long revogadoPorId, DateTime agoraUtc)
    {
        if (RevogadoEm is not null)
            throw new RegraDeNegocioViolada("Esta vigência já foi revogada.");

        if (VigenteDesde < HojeNoBrasil(agoraUtc))
            throw new RegraDeNegocioViolada(
                $"A vigência de {VigenteDesde:dd/MM/yyyy} já valeu em dias que passaram, e o cálculo desses dias usa ela. " +
                "Para corrigir, registre uma vigência nova a partir de hoje.");

        ConferirAutorETexto(revogadoPorId, motivo, "o motivo da revogação");

        RevogadoEm = agoraUtc;
        RevogadoPorId = revogadoPorId;
        MotivoDaRevogacao = motivo.Trim();
    }

    /// <summary>Preenche o que toda vigência tem, conferindo autor, justificativa e data.</summary>
    /// <param name="vigenteDesde">O primeiro dia em que vale.</param>
    /// <param name="justificativa">Por que este valor.</param>
    /// <param name="informadoPorId">Quem registra.</param>
    /// <param name="agoraUtc">O instante do registro.</param>
    protected void Informar(DateOnly vigenteDesde, string justificativa, long informadoPorId, DateTime agoraUtc)
    {
        ConferirAutorETexto(informadoPorId, justificativa, "a justificativa");

        if (vigenteDesde < HojeNoBrasil(agoraUtc))
            throw new RegraDeNegocioViolada(
                $"A vigência começa hoje ou depois — {vigenteDesde:dd/MM/yyyy} já passou. O cálculo de uma data passada " +
                "usa o parâmetro que valia naquela data, e mudar o passado mudaria um número já mostrado.");

        VigenteDesde = vigenteDesde;
        Justificativa = justificativa.Trim();
        InformadoPorId = informadoPorId;
        InformadoEm = agoraUtc;
    }

    private static void ConferirAutorETexto(long autorId, string texto, string oQueEh)
    {
        if (autorId <= 0)
            throw new RegraDeNegocioViolada("Parâmetro do potencial sem autor não é gravado: a trilha precisa dizer quem decidiu.");

        if (string.IsNullOrWhiteSpace(texto))
            throw new RegraDeNegocioViolada($"Informe {oQueEh}: parâmetro que muda o potencial inteiro precisa dizer por quê.");

        if (texto.Trim().Length > TamanhoDoTexto)
            throw new RegraDeNegocioViolada($"{char.ToUpperInvariant(oQueEh[0])}{oQueEh[1..]} vai até {TamanhoDoTexto} caracteres.");
    }
}

/// <summary>Se a regra foi confirmada pelo negócio ou ainda é um exemplo informado.</summary>
public enum SituacaoDaRegraDePotencial
{
    /// <summary>Informada como exemplo; o cálculo sai com o aviso de que a regra não foi confirmada.</summary>
    AConfirmar = 0,

    /// <summary>Confirmada pelo comercial.</summary>
    Confirmada = 1
}

/// <summary>
/// A REGRA DE POTENCIAL DE UMA CULTURA — "uma máquina de referência a cada N hectares, renovada a cada M
/// anos" —, com vigência (issue 71; decisão D-P01 da issue 63: "ter um administrador do sistema definir
/// qual a relação hectare/máquina e qual a taxa de renovação por chassis").
///
/// <para><b>O resultado é necessidade teórica de frota, não venda.</b> Área ÷ hectares por máquina dá o
/// parque que a área comporta; ÷ anos de renovação dá a demanda de um ano. Quantas serão compradas depende
/// ainda do ciclo de mercado (issue 74) e da concorrência.</para>
///
/// <para><b>Anos de renovação pode faltar</b>, e falta na primeira regra: o exemplo do gerente comercial
/// ("1 trator 3036N a cada 10 hectares de café") não disse o ciclo. Sem ele, o parque sai e a demanda anual
/// fica vazia com o motivo — nunca com um ciclo inventado.</para>
///
/// <para><b>O modelo de referência é texto</b> porque o catálogo <c>frota.Modelo</c> não tem o 3036N:
/// apontar para um modelo parecido seria inventar a regra. Quando o catálogo de modelos for saneado,
/// este campo vira chave estrangeira.</para>
///
/// <para><b>O produto é o da Produção Agrícola Municipal</b> (classificação 782 do IBGE), com o rótulo
/// oficial — é a área dele que a regra divide.</para>
/// </summary>
public sealed class RegraDePotencial : ParametroComVigencia
{
    private RegraDePotencial() { }

    /// <summary>O produto, pelo código da classificação 782 do IBGE.</summary>
    public int ProdutoCodigoIbge { get; private set; }

    /// <summary>
    /// A CULTURA DO CATÁLOGO a que esta regra pertence (issue 165); nula nas vigências anteriores a ele.
    ///
    /// <para><b>Por que anulável, e por que o produto continua aqui.</b> A regra nasceu por produto da PAM,
    /// e a vigência do café de 13/09/2026 tem de continuar valendo exatamente como valia — o passado não se
    /// reescreve. A migração liga a linha existente à cultura semeada; de agora em diante, a regra nova vem
    /// com cultura e categoria.</para>
    /// </summary>
    public int? CulturaId { get; private set; }

    /// <summary>
    /// A CATEGORIA DE MÁQUINA a que esta regra se refere (D-IM-06); nula nas vigências anteriores.
    ///
    /// <para>Sem ela, "máquinas teóricas" não dizia de quê: a mesma lavoura pede um trator a cada tantos
    /// hectares e uma colheitadeira a cada outros tantos.</para>
    /// </summary>
    public int? CategoriaDeMaquinaId { get; private set; }

    /// <summary>O rótulo oficial do produto.</summary>
    public string ProdutoNome { get; private set; } = default!;

    /// <summary>Quantos hectares do produto correspondem a uma máquina de referência.</summary>
    public decimal HectaresPorMaquina { get; private set; }

    /// <summary>A cada quantos anos a máquina é trocada. Nulo quando ninguém informou.</summary>
    public decimal? AnosDeRenovacao { get; private set; }

    /// <summary>O modelo de referência, como o negócio o escreveu.</summary>
    public string ModeloDeReferencia { get; private set; } = default!;

    /// <summary>Se a regra é exemplo ou está confirmada.</summary>
    public SituacaoDaRegraDePotencial Situacao { get; private set; }

    /// <summary>Máquinas teóricas para uma área. Nulo quando a área não está disponível.</summary>
    /// <param name="areaHectares">A área plantada do produto.</param>
    public decimal? MaquinasTeoricas(decimal? areaHectares) =>
        areaHectares is { } area ? area / HectaresPorMaquina : null;

    /// <summary>Registra uma vigência da regra de uma cultura.</summary>
    /// <param name="produtoCodigoIbge">O código do produto.</param>
    /// <param name="produtoNome">O rótulo oficial do produto.</param>
    /// <param name="hectaresPorMaquina">Hectares por máquina de referência.</param>
    /// <param name="anosDeRenovacao">Anos de renovação, quando informado.</param>
    /// <param name="modeloDeReferencia">O modelo de referência.</param>
    /// <param name="situacao">A confirmar ou confirmada.</param>
    /// <param name="vigenteDesde">O primeiro dia em que vale.</param>
    /// <param name="justificativa">Por que estes valores.</param>
    /// <param name="informadoPorId">Quem registra.</param>
    /// <param name="agoraUtc">O instante do registro.</param>
    /// <param name="culturaId">A cultura do catálogo (issue 165); nula nas vigências anteriores a ele.</param>
    /// <param name="categoriaDeMaquinaId">A categoria de máquina; nula nas vigências anteriores.</param>
    public static RegraDePotencial Informar(
        int produtoCodigoIbge,
        string produtoNome,
        decimal hectaresPorMaquina,
        decimal? anosDeRenovacao,
        string modeloDeReferencia,
        SituacaoDaRegraDePotencial situacao,
        DateOnly vigenteDesde,
        string justificativa,
        long informadoPorId,
        DateTime agoraUtc,
        int? culturaId = null,
        int? categoriaDeMaquinaId = null)
    {
        if (produtoCodigoIbge <= 0 || string.IsNullOrWhiteSpace(produtoNome))
            throw new RegraDeNegocioViolada("A regra precisa de um produto da classificação do IBGE, com o rótulo oficial.");

        if (hectaresPorMaquina is <= 0 or > 100_000)
            throw new RegraDeNegocioViolada("Hectares por máquina é um número maior que zero, até 100.000.");

        if (anosDeRenovacao is <= 0 or > 50)
            throw new RegraDeNegocioViolada("Anos de renovação é um número maior que zero, até 50.");

        if (string.IsNullOrWhiteSpace(modeloDeReferencia) || modeloDeReferencia.Trim().Length > 60)
            throw new RegraDeNegocioViolada("Informe o modelo de referência, com até 60 caracteres.");

        if (!Enum.IsDefined(situacao))
            throw new RegraDeNegocioViolada("A situação da regra é AConfirmar ou Confirmada.");

        var regra = new RegraDePotencial
        {
            ProdutoCodigoIbge = produtoCodigoIbge,
            ProdutoNome = produtoNome.Trim(),
            HectaresPorMaquina = hectaresPorMaquina,
            AnosDeRenovacao = anosDeRenovacao,
            ModeloDeReferencia = modeloDeReferencia.Trim(),
            Situacao = situacao,
            CulturaId = culturaId,
            CategoriaDeMaquinaId = categoriaDeMaquinaId
        };

        regra.Informar(vigenteDesde, justificativa, informadoPorId, agoraUtc);
        return regra;
    }
}

/// <summary>
/// OS PARÂMETROS GERAIS DO MODELO DE POTENCIAL, com vigência: a janela dos índices, a composição do índice
/// de crédito, as faixas de mercado, os pesos e limites do fator de ciclo e o limite da percepção do gestor
/// (issue 71; decisões D-P02 a D-P05 da issue 63).
///
/// <para><b>Uma linha por vigência, e ela traz o conjunto inteiro.</b> Os parâmetros se leem juntos — as três
/// faixas só fazem sentido em ordem, e os limites do fator só com os pesos —, então mudar um é registrar o
/// conjunto de novo com aquele valor trocado. Quem lê uma data recebe um conjunto coerente, nunca metade de
/// uma vigência e metade de outra.</para>
///
/// <para><b>O que o texto do Ricardo de 21/09/2026 decidiu é obrigatório</b> (janela de 12 contra 12, crédito
/// 70% contratos e 30% valor, faixas 1,0 / 1,2 / 1,4, percepção de −5% a +5%). <b>O que está em aberto pode
/// ficar vazio</b> — pesos dos três indicadores, limites do fator e o nome da faixa entre 1,0 e 1,2 — e o
/// motor (issue 74) diz "sem parâmetro decidido" em vez de usar um peso inventado.</para>
/// </summary>
public sealed class ParametroDoPotencial : ParametroComVigencia
{
    private ParametroDoPotencial() { }

    /// <summary>
    /// Quantos meses cada lado do índice compara: 12 é "os últimos 12 meses ÷ os 12 anteriores, total 24
    /// meses" (D-P02, D-P03). Vale para o momento de preço e para o crédito.
    /// </summary>
    public short MesesDaJanela { get; private set; }

    /// <summary>
    /// O peso da QUANTIDADE de contratos no índice de crédito, de 0 a 1; o valor financiado pesa o resto.
    /// 0,70 é "pega 70% de contratos e 30% do valor" (D-P03). Uma coluna só, para os dois somarem 1 sempre.
    /// </summary>
    public decimal PesoDosContratosNoCredito { get; private set; }

    /// <summary>Abaixo deste índice, o mercado está retraído (texto: "&lt; 1 retraído").</summary>
    public decimal LimiteDeRetracao { get; private set; }

    /// <summary>Acima deste índice, aquecido (texto: "&gt; 1,2 aquecido").</summary>
    public decimal LimiteDeAquecimento { get; private set; }

    /// <summary>Acima deste índice, superaquecido (texto: "&gt; 1,4 super aquecido").</summary>
    public decimal LimiteDeSuperaquecimento { get; private set; }

    /// <summary>
    /// O nome da faixa entre o limite de retração e o de aquecimento. <b>Em aberto</b> (D-P02): o texto diz
    /// "= 1 anual" e pula de 1 para 1,2. Nulo até alguém decidir.
    /// </summary>
    public string? NomeDaFaixaIntermediaria { get; private set; }

    /// <summary>
    /// O maior ajuste que o gestor pode dar a um município, em pontos percentuais, para mais ou para menos:
    /// 5 é "percepção do gestor comercial por município, −5% a +5%" (D-P04).
    /// </summary>
    public decimal LimiteDaPercepcao { get; private set; }

    /// <summary>Indicador 1 — "sensibilidade e rentabilidade do preço da commodity". Em aberto (D-P05).</summary>
    public decimal? PesoDoIndicadorDePreco { get; private set; }

    /// <summary>Indicador 2 — "sensibilidade de contratação de crédito". Em aberto (D-P05).</summary>
    public decimal? PesoDoIndicadorDeCredito { get; private set; }

    /// <summary>Indicador 3 — "sensibilidade comercial que o gerente atribui". Em aberto (D-P05).</summary>
    public decimal? PesoDoIndicadorComercial { get; private set; }

    /// <summary>O menor fator de ciclo aceito. Em aberto (D-P05); vem junto com <see cref="FatorMaximo"/>.</summary>
    public decimal? FatorMinimo { get; private set; }

    /// <summary>O maior fator de ciclo aceito. Em aberto (D-P05).</summary>
    public decimal? FatorMaximo { get; private set; }

    /// <summary>
    /// QUANTOS MESES RECENTES DO SICOR FICAM DE FORA DA JANELA — a carência (D-IM-03, issue 157).
    ///
    /// <para><b>Por que existe.</b> O Banco Central continua acrescentando contrato registrado com
    /// atraso nos meses mais recentes. Terminar a janela no último mês com dado compara 12 meses
    /// cheios com 12 meses que ainda estão enchendo, e o crédito aparece caindo sem ter caído.</para>
    ///
    /// <para><b>Em aberto.</b> Quantos meses o atraso ocupa é medição que ninguém fez ainda. Até
    /// alguém decidir, fica nulo — e a tela diz "carência não decidida" e usa zero, em vez de
    /// descartar meses por um palpite.</para>
    /// </summary>
    public short? MesesDeCarenciaDoSicor { get; private set; }

    /// <summary>
    /// ABAIXO DE QUANTAS LINHAS DO SICOR A BASE É PEQUENA (D-P03, issue 73).
    ///
    /// <para><b>Por que existe.</b> Num município com duas linhas na janela anterior e quatro na atual, o
    /// índice de crédito dá 2,00 — "+100%" — e não significa nada: é a variação de uma amostra pequena,
    /// não a do mercado. O aceite da issue 73 é "município com poucos contratos não gera índice extremo".</para>
    ///
    /// <para><b>E por que ele MARCA, em vez de corrigir.</b> Nenhuma suavização transforma "de 2 para 4"
    /// em informação: o que falta ali é contexto, não maquiagem. Abaixo deste mínimo o índice sai igual,
    /// com a marca de base pequena e a contagem ao lado, para quem lê julgar.</para>
    ///
    /// <para><b>Em aberto.</b> Qual é o mínimo é medição que ninguém fez. Até alguém decidir, fica nulo —
    /// nada é marcado, e a contagem de linhas continua visível. Inventar um limiar aqui seria escolher, no
    /// código, o que conta como "poucas".</para>
    /// </summary>
    public int? MinimoDeLinhasNoCredito { get; private set; }

    /// <summary>
    /// OS PRODUTOS DO SICOR QUE SÃO MÁQUINA — trator (7080), máquinas e implementos (4860) e
    /// colheitadeiras (2700).
    ///
    /// <para><b>Uma lista só, e no domínio</b> (issue 157). Ela morava na infraestrutura, dentro do
    /// repositório de leitura: "o que conta como máquina" é decisão de negócio, e decisão de negócio
    /// não se lê num detalhe de persistência.</para>
    ///
    /// <para><b>É provisória, de propósito.</b> A issue 165 põe as categorias de máquina no
    /// Administrador, e aí o vínculo produto × categoria vem do catálogo, com autor e vigência. Até
    /// lá, uma constante que se acha pelo nome — e não três lugares que podem divergir.</para>
    /// </summary>
    public static readonly int[] ProdutosDeMaquinaNoSicor = [7080, 4860, 2700];

    /// <summary>Os valores de uma vigência dos parâmetros gerais.</summary>
    /// <param name="MesesDaJanela">Meses de cada lado do índice.</param>
    /// <param name="PesoDosContratosNoCredito">Peso da quantidade de contratos, de 0 a 1.</param>
    /// <param name="LimiteDeRetracao">Abaixo dele, retraído.</param>
    /// <param name="LimiteDeAquecimento">Acima dele, aquecido.</param>
    /// <param name="LimiteDeSuperaquecimento">Acima dele, superaquecido.</param>
    /// <param name="NomeDaFaixaIntermediaria">O nome da faixa do meio, quando decidido.</param>
    /// <param name="LimiteDaPercepcao">O maior ajuste do gestor, em pontos percentuais.</param>
    /// <param name="PesoDoIndicadorDePreco">Peso do indicador de preço, quando decidido.</param>
    /// <param name="PesoDoIndicadorDeCredito">Peso do indicador de crédito, quando decidido.</param>
    /// <param name="PesoDoIndicadorComercial">Peso do indicador comercial, quando decidido.</param>
    /// <param name="FatorMinimo">O menor fator, quando decidido.</param>
    /// <param name="FatorMaximo">O maior fator, quando decidido.</param>
    /// <param name="MesesDeCarenciaDoSicor">Meses recentes do SICOR fora da janela, quando decidido.</param>
    /// <param name="MinimoDeLinhasNoCredito">Abaixo disto a base do crédito é pequena, quando decidido.</param>
    public sealed record Valores(
        short MesesDaJanela,
        decimal PesoDosContratosNoCredito,
        decimal LimiteDeRetracao,
        decimal LimiteDeAquecimento,
        decimal LimiteDeSuperaquecimento,
        string? NomeDaFaixaIntermediaria,
        decimal LimiteDaPercepcao,
        decimal? PesoDoIndicadorDePreco,
        decimal? PesoDoIndicadorDeCredito,
        decimal? PesoDoIndicadorComercial,
        decimal? FatorMinimo,
        decimal? FatorMaximo,
        short? MesesDeCarenciaDoSicor = null,
        int? MinimoDeLinhasNoCredito = null);

    /// <summary>Registra uma vigência dos parâmetros gerais.</summary>
    /// <param name="valores">O conjunto inteiro.</param>
    /// <param name="vigenteDesde">O primeiro dia em que vale.</param>
    /// <param name="justificativa">Por que estes valores.</param>
    /// <param name="informadoPorId">Quem registra.</param>
    /// <param name="agoraUtc">O instante do registro.</param>
    public static ParametroDoPotencial Informar(
        Valores valores, DateOnly vigenteDesde, string justificativa, long informadoPorId, DateTime agoraUtc)
    {
        Conferir(valores);

        var parametro = new ParametroDoPotencial
        {
            MesesDaJanela = valores.MesesDaJanela,
            PesoDosContratosNoCredito = valores.PesoDosContratosNoCredito,
            LimiteDeRetracao = valores.LimiteDeRetracao,
            LimiteDeAquecimento = valores.LimiteDeAquecimento,
            LimiteDeSuperaquecimento = valores.LimiteDeSuperaquecimento,
            NomeDaFaixaIntermediaria = string.IsNullOrWhiteSpace(valores.NomeDaFaixaIntermediaria)
                ? null : valores.NomeDaFaixaIntermediaria.Trim(),
            LimiteDaPercepcao = valores.LimiteDaPercepcao,
            PesoDoIndicadorDePreco = valores.PesoDoIndicadorDePreco,
            PesoDoIndicadorDeCredito = valores.PesoDoIndicadorDeCredito,
            PesoDoIndicadorComercial = valores.PesoDoIndicadorComercial,
            FatorMinimo = valores.FatorMinimo,
            FatorMaximo = valores.FatorMaximo,
            MesesDeCarenciaDoSicor = valores.MesesDeCarenciaDoSicor,
            MinimoDeLinhasNoCredito = valores.MinimoDeLinhasNoCredito
        };

        parametro.Informar(vigenteDesde, justificativa, informadoPorId, agoraUtc);
        return parametro;
    }

    /// <summary>
    /// A faixa de mercado de um índice (momento de preço ou crédito). O limite pertence à faixa de baixo:
    /// 1,20 ainda não é aquecido, porque o texto diz "&gt; 1,2".
    /// </summary>
    /// <param name="indice">O índice, em que 1 é "igual à janela anterior".</param>
    public FaixaDeMercado FaixaDe(decimal indice) =>
        indice < LimiteDeRetracao ? FaixaDeMercado.Retraido
        : indice <= LimiteDeAquecimento ? FaixaDeMercado.Intermediaria
        : indice <= LimiteDeSuperaquecimento ? FaixaDeMercado.Aquecido
        : FaixaDeMercado.Superaquecido;

    private static void Conferir(Valores v)
    {
        if (v.MesesDaJanela is < 1 or > 60)
            throw new RegraDeNegocioViolada("A janela dos índices vai de 1 a 60 meses de cada lado.");

        if (v.PesoDosContratosNoCredito is < 0 or > 1)
            throw new RegraDeNegocioViolada("O peso dos contratos no crédito vai de 0 a 1 (0,70 é 70% contratos e 30% valor).");

        if (!(v.LimiteDeRetracao > 0 && v.LimiteDeRetracao < v.LimiteDeAquecimento && v.LimiteDeAquecimento < v.LimiteDeSuperaquecimento)
            || v.LimiteDeSuperaquecimento > 10)
            throw new RegraDeNegocioViolada(
                "As faixas precisam subir: retração abaixo de aquecimento, aquecimento abaixo de superaquecimento, todos entre 0 e 10.");

        if (v.NomeDaFaixaIntermediaria?.Trim().Length > 40)
            throw new RegraDeNegocioViolada("O nome da faixa intermediária vai até 40 caracteres.");

        if (v.LimiteDaPercepcao is <= 0 or > 50)
            throw new RegraDeNegocioViolada("O limite da percepção do gestor é maior que zero e vai até 50 pontos percentuais.");

        foreach (var peso in new[] { v.PesoDoIndicadorDePreco, v.PesoDoIndicadorDeCredito, v.PesoDoIndicadorComercial })
            if (peso is < 0 or > 5)
                throw new RegraDeNegocioViolada("O peso de um indicador vai de 0 a 5.");

        if ((v.FatorMinimo is null) != (v.FatorMaximo is null))
            throw new RegraDeNegocioViolada("Os limites do fator andam juntos: informe o mínimo e o máximo, ou nenhum dos dois.");

        if (v.FatorMinimo is { } minimo && v.FatorMaximo is { } maximo && !(minimo > 0 && minimo < 1 && maximo > 1 && maximo <= 10))
            throw new RegraDeNegocioViolada(
                "O fator mínimo fica entre 0 e 1 e o máximo acima de 1, até 10: com tudo neutro o fator é 1, e o limite não pode excluí-lo.");

        // A CARÊNCIA NÃO PODE COMER A JANELA INTEIRA: descartar 12 meses de uma janela de 12 deixaria
        // os dois lados vazios, e a comparação sem nada para comparar.
        if (v.MesesDeCarenciaDoSicor is { } carencia && (carencia < 0 || carencia >= v.MesesDaJanela))
            throw new RegraDeNegocioViolada(
                $"A carência do SICOR vai de zero a {v.MesesDaJanela - 1} meses — menos que a janela, senão não sobra mês para comparar.");

        // ZERO NÃO É MÍNIMO: com zero, nenhuma base seria pequena, que é o mesmo que não ter o parâmetro —
        // e aí o certo é deixá-lo nulo, que diz "não decidido" em vez de "decidido que não marca".
        if (v.MinimoDeLinhasNoCredito is { } minimoDeLinhas && minimoDeLinhas is < 1 or > 10_000)
            throw new RegraDeNegocioViolada(
                "O mínimo de linhas do SICOR vai de 1 a 10.000. Para não marcar base pequena nenhuma, deixe em branco.");
    }
}

/// <summary>A faixa de mercado de um índice (D-P02).</summary>
public enum FaixaDeMercado
{
    /// <summary>Abaixo do limite de retração.</summary>
    Retraido = 0,

    /// <summary>Entre retração e aquecimento — o nome está em aberto (D-P02).</summary>
    Intermediaria = 1,

    /// <summary>Acima do limite de aquecimento.</summary>
    Aquecido = 2,

    /// <summary>Acima do limite de superaquecimento.</summary>
    Superaquecido = 3
}

/// <summary>
/// A PERCEPÇÃO DO GESTOR COMERCIAL SOBRE UM MUNICÍPIO, com vigência — o indicador 3 do potencial, "a
/// sensibilidade comercial que o gerente atribui" (issue 71; decisão D-P04: "por município, −5% a +5%").
///
/// <para><b>É opinião registrada, e é por isso que tem autor e justificativa.</b> Ela ajusta um número
/// calculado com dado oficial; quem vê o potencial ajustado precisa poder perguntar "quem achou isso, e
/// por quê?" — e a resposta está na linha.</para>
///
/// <para><b>O limite não é fixo no código:</b> é o <see cref="ParametroDoPotencial.LimiteDaPercepcao"/>
/// vigente na data de início, que o caso de uso confere ao registrar.</para>
/// </summary>
public sealed class PercepcaoDoGestor : ParametroComVigencia
{
    private PercepcaoDoGestor() { }

    /// <summary>O município.</summary>
    public int MunicipioId { get; private set; }

    /// <summary>O ajuste, em pontos percentuais: −5 é "5% a menos", 0 é neutro.</summary>
    public decimal Percentual { get; private set; }

    /// <summary>Registra uma vigência da percepção sobre um município.</summary>
    /// <param name="municipioId">O município.</param>
    /// <param name="percentual">O ajuste, em pontos percentuais.</param>
    /// <param name="limiteDaPercepcao">O limite vigente na data de início.</param>
    /// <param name="vigenteDesde">O primeiro dia em que vale.</param>
    /// <param name="justificativa">Por que este ajuste.</param>
    /// <param name="informadoPorId">Quem registra.</param>
    /// <param name="agoraUtc">O instante do registro.</param>
    public static PercepcaoDoGestor Informar(
        int municipioId,
        decimal percentual,
        decimal limiteDaPercepcao,
        DateOnly vigenteDesde,
        string justificativa,
        long informadoPorId,
        DateTime agoraUtc)
    {
        if (municipioId <= 0)
            throw new RegraDeNegocioViolada("A percepção é de um município do catálogo.");

        // A CULTURA DA MENSAGEM É A DO LEITOR, E NÃO A DO SERVIDOR (achado da issue 74): interpolação
        // sem cultura escreve "2.5%" no runner do CI e "2,5%" na estação, e quem lê é o comercial
        // brasileiro. A vírgula não é preferência do processo — é parte do texto.
        if (Math.Abs(percentual) > limiteDaPercepcao)
            throw new RegraDeNegocioViolada(string.Format(
                System.Globalization.CultureInfo.GetCultureInfo("pt-BR"),
                "A percepção vai de −{0:0.##}% a +{0:0.##}% na data de início; {1:0.##}% passa disso.",
                limiteDaPercepcao, percentual));

        var percepcao = new PercepcaoDoGestor { MunicipioId = municipioId, Percentual = percentual };
        percepcao.Informar(vigenteDesde, justificativa, informadoPorId, agoraUtc);
        return percepcao;
    }
}
