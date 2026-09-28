using System.Globalization;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Organizacao;

// =================================================================================================
// O PLANEJAMENTO COMERCIAL DA API GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026) — além da meta (#138):
//
//   GestorDoConsultor       — o de-para de consultores da GN: quem é o gestor ("capitão") de cada consultor;
//   ForecastDaGerencia      — a previsão de cada gestor, por linha e mês: o Forecast (revisto todo mês) e o Best Guess
//                             (reavaliado toda segunda-feira);
//   CotaDeConsorcioVendida  — o realizado de consórcio, cota a cota, que fecha a meta de CONSÓRCIO (D-M4 dizia "não
//                             medido pelo CRM"; agora é medido pela mesma regra da GN).
//
// Os três são ESPELHO de outra equipe, como a meta: o CRM não edita, a linha que some da origem é excluída logicamente e a
// que volta é reativada na mesma linha. Nenhum guarda nome de cliente: a cota não traz o consorciado.
//
// O DE-PARA E O FORECAST SÃO DA ORGANIZAÇÃO, e não de uma filial — por isso não são EntidadeBase (que exige EmpresaId,
// a fronteira de multiempresa): seguem o molde do dado de organização, como o preço da máquina e as cotações, com o
// próprio carimbo de importação e a própria exclusão lógica. A cota é de uma filial, e é EntidadeBase.
// =================================================================================================

/// <summary>A leitura que trouxe o conteúdo de uma linha do planejamento — o frescor dela.</summary>
/// <param name="LidaEm">Quando o CRM leu (UTC).</param>
/// <param name="GeradaNaOrigemEm">Quando a API gerou a resposta (UTC), se disse.</param>
public sealed record LeituraDoPlanejamento(DateTime LidaEm, DateTime? GeradaNaOrigemEm);

/// <summary>
/// O GESTOR DE UM CONSULTOR — uma linha do de-para de consultores da API Gestão de Negócios
/// (<c>/api/v1/cadastros/de_para_consultores</c>).
///
/// <para><b>Para que serve.</b> O forecast é por gestor, e a meta e o realizado são por consultor: é este de-para que junta
/// os dois ("o PO do time do gestor"). O capitão É o gestor — medido em 28/09/2026: os 8 gestores do forecast e os 8 da
/// performance de consórcio são capitães aqui, e os 59 consultores com meta e os 30 com cota vendida estão todos aqui.</para>
///
/// <para><b>Sem filial, de propósito.</b> O time de um gestor atravessa filiais; a filial do consultor fica em
/// <see cref="FilialNumero"/>, que não é fronteira de acesso — o de-para é a definição da casa, e não o dado de alguém.</para>
///
/// <para><b>Um consultor pode aparecer duas vezes</b> (2 em 65, medido em 28/09): o de-para liga o NOME NO RELATÓRIO de
/// consórcio ao consultor, e uma pessoa pode ter dois nomes lá. O CRM guarda as duas linhas e, na leitura, o gestor do
/// consultor é o da linha de vigência mais recente (<see cref="GestorPorConsultor"/>).</para>
/// </summary>
public sealed class GestorDoConsultor
{
    /// <summary>O tamanho das colunas de pessoa (consultor e gestor).</summary>
    public const int TamanhoDaPessoa = 80;

    /// <summary>O fluxo da carga do de-para — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "GESTAO_NEGOCIOS.DE_PARA_CONSULTORES";

    private GestorDoConsultor() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema de onde veio (<c>GESTAO_NEGOCIOS</c>).</summary>
    public int SistemaId { get; private set; }

    /// <summary>O <c>id</c> da linha na GN — a chave natural.</summary>
    public int IdNaOrigem { get; private set; }

    /// <summary>O consultor como a GN escreve (<c>NOME.SOBRENOME</c>, em maiúsculas).</summary>
    public string ConsultorNaOrigem { get; private set; } = default!;

    /// <summary>O gestor (o "capitão" da GN), como a GN escreve.</summary>
    public string GestorNaOrigem { get; private set; } = default!;

    /// <summary>O NN da filial do consultor (<c>0101NN</c>); 0 é Digital e 900 é Grandes Contas, que não são filial do CRM.</summary>
    public int? FilialNumero { get; private set; }

    /// <summary>Desde quando a linha vale, quando a GN diz.</summary>
    public DateOnly? VigenteDesde { get; private set; }

    /// <summary>O resumo do conteúdo lido — é o que a recarga compara.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>Quando a linha entrou no CRM (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga que a trouxe.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>A leitura que trouxe o conteúdo atual (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>Quando a linha sumiu da origem (UTC); nula enquanto está lá.</summary>
    public DateTime? ExcluidoEm { get; private set; }

    /// <summary>
    /// O GESTOR DE CADA CONSULTOR, pela chave da pessoa (<see cref="MetaDeVenda.ChaveDaPessoa"/>) — a mesma que casa a meta e o
    /// vendedor do ART. Quando o consultor tem mais de uma linha, vale a de vigência mais recente; sem vigência, a de id maior.
    /// </summary>
    /// <param name="linhas">As linhas vigentes do de-para.</param>
    public static IReadOnlyDictionary<string, string> GestorPorConsultor(
        IEnumerable<(string Consultor, string Gestor, DateOnly? VigenteDesde, int IdNaOrigem)> linhas) =>
        linhas
            .Select(l => (Chave: MetaDeVenda.ChaveDaPessoa(l.Consultor), l.Gestor, l.VigenteDesde, l.IdNaOrigem))
            .Where(l => l.Chave.Length > 0)
            .GroupBy(l => l.Chave, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(l => l.VigenteDesde ?? DateOnly.MinValue).ThenByDescending(l => l.IdNaOrigem).First().Gestor,
                StringComparer.Ordinal);

    /// <summary>Registra uma linha lida da origem.</summary>
    /// <param name="sistemaId">O sistema de origem.</param>
    /// <param name="idNaOrigem">O id na GN.</param>
    /// <param name="consultor">O consultor.</param>
    /// <param name="gestor">O gestor.</param>
    /// <param name="filialNumero">O NN da filial.</param>
    /// <param name="vigenteDesde">Desde quando vale.</param>
    /// <param name="hash">O resumo do conteúdo.</param>
    /// <param name="leitura">A leitura.</param>
    /// <param name="importadoPorId">Quem roda a integração.</param>
    public static GestorDoConsultor Registrar(
        int sistemaId, int idNaOrigem, string consultor, string gestor, int? filialNumero, DateOnly? vigenteDesde, string hash,
        LeituraDoPlanejamento leitura, long importadoPorId)
    {
        if (idNaOrigem <= 0) throw new RegraDeNegocioViolada("O de-para de consultores precisa do id da origem, positivo.");
        var linha = new GestorDoConsultor
        {
            SistemaId = sistemaId, IdNaOrigem = idNaOrigem, ImportadoEm = leitura.LidaEm, ImportadoPorId = importadoPorId
        };
        linha.Aplicar(consultor, gestor, filialNumero, vigenteDesde, hash, leitura);
        return linha;
    }

    /// <summary>Aplica uma nova leitura; nada muda quando o resumo é o mesmo.</summary>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(string consultor, string gestor, int? filialNumero, DateOnly? vigenteDesde, string hash, LeituraDoPlanejamento leitura)
    {
        if (hash == HashDaOrigem) return false;
        Aplicar(consultor, gestor, filialNumero, vigenteDesde, hash, leitura);
        return true;
    }

    /// <summary>A linha sumiu da origem: sai sem ser apagada.</summary>
    /// <param name="agoraUtc">O instante.</param>
    public void Excluir(DateTime agoraUtc) => ExcluidoEm ??= agoraUtc;

    /// <summary>Traz de volta a linha que tinha sumido da origem.</summary>
    public bool Reativar()
    {
        if (ExcluidoEm is null) return false;
        ExcluidoEm = null;
        return true;
    }

    private void Aplicar(string consultor, string gestor, int? filialNumero, DateOnly? vigenteDesde, string hash, LeituraDoPlanejamento leitura)
    {
        ConsultorNaOrigem = Pessoa(consultor, "o consultor").ToUpperInvariant();
        GestorNaOrigem = Pessoa(gestor, "o gestor");
        if (string.IsNullOrWhiteSpace(hash)) throw new RegraDeNegocioViolada("O de-para guarda o resumo do conteúdo lido.");
        FilialNumero = filialNumero;
        VigenteDesde = vigenteDesde;
        HashDaOrigem = hash;
        LidaEm = leitura.LidaEm;
    }

    internal static string Pessoa(string texto, string oQue)
    {
        var limpo = texto?.Trim() ?? string.Empty;
        if (limpo.Length is 0 or > TamanhoDaPessoa)
            throw new RegraDeNegocioViolada($"O planejamento preserva {oQue} da origem, de 1 a {TamanhoDaPessoa} caracteres.");
        return limpo;
    }
}

/// <summary>
/// A PREVISÃO DE UM GESTOR, por linha e mês — uma linha do cadastro "Forecast da Gerência" da API Gestão de Negócios
/// (<c>/api/v1/cadastros/forecast</c>).
///
/// <para><b>Dois números, dois ritmos.</b> O Forecast é revisto uma vez por mês; o Best Guess, reavaliado toda
/// segunda-feira. Os dois são em unidades, e qualquer um pode faltar: nulo é "o gestor não informou", e não zero. O PO
/// (a meta) não fica aqui — é a <see cref="MetaDeVenda"/>, somada pelo time do gestor (<see cref="GestorDoConsultor"/>).</para>
///
/// <para><b>A observação da GN não é lida</b>: é texto livre do gestor, e pode citar cliente pelo nome.</para>
/// </summary>
public sealed class ForecastDaGerencia
{
    /// <summary>O tamanho da coluna da linha — o mesmo da meta.</summary>
    public const int TamanhoDaLinha = MetaDeVenda.TamanhoDaLinha;

    /// <summary>O fluxo da carga do forecast — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "GESTAO_NEGOCIOS.FORECAST";

    /// <summary>A maior quantidade aceita — muito acima de qualquer previsão mensal de um gestor numa linha.</summary>
    public const int QuantidadeMaxima = 10_000;

    private ForecastDaGerencia() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema de onde veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O <c>id</c> da linha na GN — a chave natural.</summary>
    public int IdNaOrigem { get; private set; }

    /// <summary>O mês, no dia 1.</summary>
    public DateOnly Competencia { get; private set; }

    /// <summary>O gestor, como a GN escreve — o mesmo texto do capitão no de-para.</summary>
    public string GestorNaOrigem { get; private set; } = default!;

    /// <summary>A linha como a GN escreve (o vocabulário do ART).</summary>
    public string LinhaNaOrigem { get; private set; } = default!;

    /// <summary>O código estável da linha — o mesmo algoritmo da meta e do ART.</summary>
    public string CodigoDaLinha { get; private set; } = default!;

    /// <summary>O Forecast, em unidades; nulo quando o gestor não informou.</summary>
    public int? Forecast { get; private set; }

    /// <summary>O Best Guess, em unidades; nulo quando o gestor não informou.</summary>
    public int? BestGuess { get; private set; }

    /// <summary>O resumo do conteúdo lido.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>Quando a linha entrou no CRM (UTC).</summary>
    public DateTime ImportadoEm { get; private set; }

    /// <summary>Quem rodou a carga que a trouxe.</summary>
    public long ImportadoPorId { get; private set; }

    /// <summary>A leitura que trouxe o conteúdo atual (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>Quando a API gerou a resposta dessa leitura (UTC).</summary>
    public DateTime? GeradaNaOrigemEm { get; private set; }

    /// <summary>Quando a linha sumiu da origem (UTC); nula enquanto está lá.</summary>
    public DateTime? ExcluidoEm { get; private set; }

    /// <summary>Registra uma previsão lida da origem.</summary>
    public static ForecastDaGerencia Registrar(
        int sistemaId, int idNaOrigem, DateOnly competencia, string gestor, string linhaNaOrigem, int? forecast, int? bestGuess,
        string hash, LeituraDoPlanejamento leitura, long importadoPorId)
    {
        if (idNaOrigem <= 0) throw new RegraDeNegocioViolada("O forecast precisa do id da origem, positivo.");
        var linha = new ForecastDaGerencia
        {
            SistemaId = sistemaId, IdNaOrigem = idNaOrigem, ImportadoEm = leitura.LidaEm, ImportadoPorId = importadoPorId
        };
        linha.Aplicar(competencia, gestor, linhaNaOrigem, forecast, bestGuess, hash, leitura);
        return linha;
    }

    /// <summary>Aplica uma nova leitura; nada muda quando o resumo é o mesmo. Cada mudança do número vai para a trilha.</summary>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(
        DateOnly competencia, string gestor, string linhaNaOrigem, int? forecast, int? bestGuess, string hash, LeituraDoPlanejamento leitura)
    {
        if (hash == HashDaOrigem) return false;
        Aplicar(competencia, gestor, linhaNaOrigem, forecast, bestGuess, hash, leitura);
        return true;
    }

    /// <summary>A previsão sumiu da origem: sai sem ser apagada.</summary>
    /// <param name="agoraUtc">O instante.</param>
    public void Excluir(DateTime agoraUtc) => ExcluidoEm ??= agoraUtc;

    /// <summary>Traz de volta a previsão que tinha sumido da origem.</summary>
    public bool Reativar()
    {
        if (ExcluidoEm is null) return false;
        ExcluidoEm = null;
        return true;
    }

    private void Aplicar(
        DateOnly competencia, string gestor, string linhaNaOrigem, int? forecast, int? bestGuess, string hash, LeituraDoPlanejamento leitura)
    {
        if (competencia.Day != 1)
            throw new RegraDeNegocioViolada(
                $"A competência do forecast é o mês, no dia 1 — veio {competencia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.");
        if (forecast is < 0 or > QuantidadeMaxima || bestGuess is < 0 or > QuantidadeMaxima)
            throw new RegraDeNegocioViolada($"O forecast e o best guess vão de 0 a {QuantidadeMaxima} unidades.");

        var linha = linhaNaOrigem?.Trim() ?? string.Empty;
        if (linha.Length is 0 or > TamanhoDaLinha)
            throw new RegraDeNegocioViolada($"O forecast preserva a linha da origem, de 1 a {TamanhoDaLinha} caracteres.");
        if (string.IsNullOrWhiteSpace(hash)) throw new RegraDeNegocioViolada("O forecast guarda o resumo do conteúdo lido.");

        Competencia = competencia;
        GestorNaOrigem = GestorDoConsultor.Pessoa(gestor, "o gestor");
        LinhaNaOrigem = linha;
        CodigoDaLinha = CodigoEstavel.De(linha, TamanhoDaLinha);
        Forecast = forecast;
        BestGuess = bestGuess;
        HashDaOrigem = hash;
        LidaEm = leitura.LidaEm;
        GeradaNaOrigemEm = leitura.GeradaNaOrigemEm;
    }
}

/// <summary>O que uma cota vendida traz da origem, já saneado.</summary>
/// <param name="EmpresaId">A filial da cota — a fronteira de acesso.</param>
/// <param name="Competencia">O mês em que a cota conta como realizado, no dia 1 (o mês da performance da GN).</param>
/// <param name="AlocadaEm">A data de alocação da cota.</param>
/// <param name="Contemplacao">Lance, Sorteio ou Não Contemplado, como a GN escreve.</param>
/// <param name="ContempladaEm">A data da contemplação, quando houve.</param>
/// <param name="ConsultorNaOrigem">O consultor, em maiúsculas.</param>
/// <param name="ConsultorUsuarioId">A conta do CRM do consultor; nula quando não há conta.</param>
/// <param name="GestorNaOrigem">O gestor do consultor, como a GN escreve.</param>
/// <param name="Produto">O bem da cota (o plano), como a GN escreve.</param>
/// <param name="ValorDoBem">O valor do bem.</param>
/// <param name="ValorDaParcela">O valor da parcela.</param>
/// <param name="HashDaOrigem">O resumo do conteúdo lido.</param>
public sealed record DadosDaCotaNaOrigem(
    int EmpresaId,
    DateOnly Competencia,
    DateOnly AlocadaEm,
    string Contemplacao,
    DateOnly? ContempladaEm,
    string ConsultorNaOrigem,
    long? ConsultorUsuarioId,
    string GestorNaOrigem,
    string? Produto,
    decimal? ValorDoBem,
    decimal? ValorDaParcela,
    string HashDaOrigem);

/// <summary>
/// UMA COTA DE CONSÓRCIO VENDIDA — o realizado da meta de CONSÓRCIO, como a performance de consórcio da API Gestão de
/// Negócios conta (<c>/api/v1/paineis/performance-consorcio</c>, as linhas "Realizado").
///
/// <para><b>A chave é o grupo e a cota</b> (185 cotas, 185 pares distintos, medido em 28/09/2026). O mês é o que a GN
/// atribui — é por ele que a performance da GN soma, e é por ele que os números batem.</para>
///
/// <para><b>O painel só traz o ano fiscal corrente.</b> Quando o ano vira, as cotas do anterior somem da resposta — e isso
/// não é cota cancelada. Por isso a carga só exclui a cota que sumiu DENTRO dos meses que a leitura cobre; a de fora fica
/// como estava.</para>
///
/// <para><b>O consorciado não é lido.</b> A cota é do cliente, mas o nome dele não entra no CRM por aqui — a GN mesma não
/// devolve o documento pela API.</para>
/// </summary>
public sealed class CotaDeConsorcioVendida : EntidadeBase
{
    /// <summary>O tamanho das colunas de grupo e cota.</summary>
    public const int TamanhoDoCodigo = 20;

    /// <summary>O tamanho da coluna do produto.</summary>
    public const int TamanhoDoProduto = 160;

    /// <summary>O tamanho da coluna da contemplação.</summary>
    public const int TamanhoDaContemplacao = 30;

    /// <summary>O fluxo da carga das cotas — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "GESTAO_NEGOCIOS.CONSORCIO";

    private CotaDeConsorcioVendida() { }

    /// <summary>A filial da cota — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O sistema de onde veio.</summary>
    public int SistemaId { get; private set; }

    /// <summary>O grupo do consórcio.</summary>
    public string Grupo { get; private set; } = default!;

    /// <summary>A cota, dentro do grupo.</summary>
    public string Cota { get; private set; } = default!;

    /// <summary>O mês em que a cota conta como realizado, no dia 1.</summary>
    public DateOnly Competencia { get; private set; }

    /// <summary>A data de alocação.</summary>
    public DateOnly AlocadaEm { get; private set; }

    /// <summary>Lance, Sorteio ou Não Contemplado.</summary>
    public string Contemplacao { get; private set; } = default!;

    /// <summary>A data da contemplação, quando houve.</summary>
    public DateOnly? ContempladaEm { get; private set; }

    /// <summary>O consultor, em maiúsculas.</summary>
    public string ConsultorNaOrigem { get; private set; } = default!;

    /// <summary>A conta do CRM do consultor; nula quando não há conta.</summary>
    public long? ConsultorUsuarioId { get; private set; }

    /// <summary>O gestor do consultor, como a GN escreve.</summary>
    public string GestorNaOrigem { get; private set; } = default!;

    /// <summary>O bem da cota, como a GN escreve.</summary>
    public string? Produto { get; private set; }

    /// <summary>O valor do bem.</summary>
    public decimal? ValorDoBem { get; private set; }

    /// <summary>O valor da parcela.</summary>
    public decimal? ValorDaParcela { get; private set; }

    /// <summary>O resumo do conteúdo lido.</summary>
    public string HashDaOrigem { get; private set; } = default!;

    /// <summary>A leitura que trouxe o conteúdo atual (UTC).</summary>
    public DateTime LidaEm { get; private set; }

    /// <summary>A chave da cota na origem — grupo e cota, que é como a GN a identifica.</summary>
    public static string Chave(string grupo, string cota) => $"{grupo.Trim()}/{cota.Trim()}";

    /// <summary>Registra uma cota lida da origem.</summary>
    public static CotaDeConsorcioVendida Registrar(
        int sistemaId, string grupo, string cota, DadosDaCotaNaOrigem dados, LeituraDoPlanejamento leitura, long criadoPorId)
    {
        var linha = new CotaDeConsorcioVendida
        {
            SistemaId = sistemaId,
            Grupo = Codigo(grupo, "o grupo"),
            Cota = Codigo(cota, "a cota"),
            CriadoPorId = criadoPorId
        };
        linha.Aplicar(dados, leitura);
        return linha;
    }

    /// <summary>Aplica uma nova leitura; nada muda quando o resumo e o casamento são os mesmos.</summary>
    /// <returns>Verdadeiro quando algo mudou.</returns>
    public bool AtualizarDaOrigem(DadosDaCotaNaOrigem dados, LeituraDoPlanejamento leitura, long usuarioId)
    {
        if (dados.HashDaOrigem == HashDaOrigem && dados.EmpresaId == EmpresaId && dados.ConsultorUsuarioId == ConsultorUsuarioId)
            return false;
        Aplicar(dados, leitura);
        MarcarAlteracao(usuarioId);
        return true;
    }

    /// <summary>Traz de volta a cota que tinha sumido da origem.</summary>
    /// <param name="usuarioId">Quem roda a integração.</param>
    public bool Reativar(long usuarioId)
    {
        if (!EstaExcluido) return false;
        ExcluidoEm = null;
        MarcarAlteracao(usuarioId);
        return true;
    }

    private void Aplicar(DadosDaCotaNaOrigem dados, LeituraDoPlanejamento leitura)
    {
        if (dados.Competencia.Day != 1)
            throw new RegraDeNegocioViolada(
                $"A competência da cota é o mês, no dia 1 — veio {dados.Competencia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.");
        var contemplacao = dados.Contemplacao?.Trim() ?? string.Empty;
        if (contemplacao.Length is 0 or > TamanhoDaContemplacao)
            throw new RegraDeNegocioViolada($"A cota preserva a contemplação da origem, de 1 a {TamanhoDaContemplacao} caracteres.");
        if (dados.ValorDoBem is < 0 || dados.ValorDaParcela is < 0) throw new RegraDeNegocioViolada("O valor da cota não é negativo.");
        if (string.IsNullOrWhiteSpace(dados.HashDaOrigem)) throw new RegraDeNegocioViolada("A cota guarda o resumo do conteúdo lido.");

        EmpresaId = dados.EmpresaId;
        Competencia = dados.Competencia;
        AlocadaEm = dados.AlocadaEm;
        Contemplacao = contemplacao;
        ContempladaEm = dados.ContempladaEm;
        ConsultorNaOrigem = GestorDoConsultor.Pessoa(dados.ConsultorNaOrigem, "o consultor").ToUpperInvariant();
        ConsultorUsuarioId = dados.ConsultorUsuarioId;
        GestorNaOrigem = GestorDoConsultor.Pessoa(dados.GestorNaOrigem, "o gestor");
        var produto = dados.Produto?.Trim();
        Produto = string.IsNullOrEmpty(produto) ? null : produto.Length > TamanhoDoProduto ? produto[..TamanhoDoProduto] : produto;
        ValorDoBem = dados.ValorDoBem;
        ValorDaParcela = dados.ValorDaParcela;
        HashDaOrigem = dados.HashDaOrigem;
        LidaEm = leitura.LidaEm;
    }

    private static string Codigo(string texto, string oQue)
    {
        var limpo = texto?.Trim() ?? string.Empty;
        if (limpo.Length is 0 or > TamanhoDoCodigo)
            throw new RegraDeNegocioViolada($"A cota preserva {oQue} da origem, de 1 a {TamanhoDoCodigo} caracteres.");
        return limpo;
    }
}
