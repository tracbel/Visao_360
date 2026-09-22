using System.Globalization;
using System.Text.RegularExpressions;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Integracao;

// =================================================================================================
// AS INTEGRAÇÕES CONFIGURÁVEIS PELA TELA (issue 136, 22/09/2026).
//
// Até aqui, o endereço e a credencial de cada sistema moravam em variável de ambiente do servidor, e o
// calendário das rotinas em tarefa agendada do Windows. Mudar qualquer um dos três era trabalho de quem
// tem acesso ao servidor. Agora:
//
//   CONEXÃO  — o sistema com que o CRM fala (Protheus, ART, Vórtice, as fontes públicas) e, para quem
//              precisa, o endereço e a credencial. A senha é guardada PROTEGIDA (IProtetorDeSegredos) e
//              nunca volta pela API; as variáveis de ambiente continuam valendo como reserva.
//   ROTINA   — o que roda sozinho no servidor (as fontes anuais e mensais, o faturamento, o ART), com a
//              agenda, ligada ou desligada e "rodar agora". Quem roda é o ORQUESTRADOR: uma tarefa só,
//              a cada cinco minutos, que lê a agenda daqui.
//   TESTE    — o botão "Testar", executado no servidor, só leitura, com o resultado guardado.
// =================================================================================================

/// <summary>Com que frequência uma rotina roda.</summary>
public enum CadenciaDaRotina
{
    /// <summary>Uma vez por ano, num mês e dia fixos.</summary>
    Anual = 0,

    /// <summary>Todo mês, num dia fixo.</summary>
    Mensal = 1,

    /// <summary>Todo dia, numa hora fixa.</summary>
    Diaria = 2,

    /// <summary>A cada tantos minutos, contados do começo da última execução.</summary>
    Intervalo = 3
}

/// <summary>
/// A AGENDA DE UMA ROTINA, no horário de São Paulo — o do servidor: UTC−3 o ano inteiro, sem horário de verão
/// desde 2019.
///
/// <para><b>Dia até 28.</b> "Todo dia 31" não existe em fevereiro; em vez de inventar o que acontece nesse mês,
/// a agenda não aceita.</para>
/// </summary>
/// <param name="Cadencia">Anual, mensal, diária ou por intervalo.</param>
/// <param name="Mes">O mês, na anual.</param>
/// <param name="Dia">O dia do mês, na anual e na mensal.</param>
/// <param name="Hora">A hora, nas três de calendário.</param>
/// <param name="IntervaloMinutos">Os minutos, na por intervalo.</param>
public sealed record AgendaDaRotina(CadenciaDaRotina Cadencia, int? Mes, int? Dia, TimeOnly? Hora, int? IntervaloMinutos)
{
    /// <summary>
    /// O menor intervalo. O orquestrador acorda a cada cinco minutos; um intervalo menor que três voltas dele
    /// seria uma promessa que ele não cumpre.
    /// </summary>
    public const int IntervaloMinimo = 15;

    /// <summary>O maior intervalo: um dia. Mais do que isso é a cadência diária.</summary>
    public const int IntervaloMaximo = 1440;

    private static readonly TimeSpan FusoDeSaoPaulo = TimeSpan.FromHours(-3);

    private static readonly string[] NomesDosMeses =
        ["janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"];

    /// <summary>Uma vez por ano.</summary>
    public static AgendaDaRotina AnualEm(int mes, int dia, TimeOnly hora) => new(CadenciaDaRotina.Anual, mes, dia, hora, null);

    /// <summary>Todo mês.</summary>
    public static AgendaDaRotina MensalEm(int dia, TimeOnly hora) => new(CadenciaDaRotina.Mensal, null, dia, hora, null);

    /// <summary>Todo dia.</summary>
    public static AgendaDaRotina DiariaAs(TimeOnly hora) => new(CadenciaDaRotina.Diaria, null, null, hora, null);

    /// <summary>A cada tantos minutos.</summary>
    public static AgendaDaRotina ACada(int minutos) => new(CadenciaDaRotina.Intervalo, null, null, null, minutos);

    /// <summary>O que está fora da faixa, campo a campo (o nome do campo é o da requisição).</summary>
    public IReadOnlyList<(string Campo, string Mensagem)> Problemas()
    {
        var problemas = new List<(string, string)>();
        switch (Cadencia)
        {
            case CadenciaDaRotina.Anual:
                if (Mes is not (>= 1 and <= 12)) problemas.Add(("mes", "Informe o mês, de 1 a 12."));
                if (Dia is not (>= 1 and <= 28)) problemas.Add(("dia", "Informe o dia, de 1 a 28 (o 29, o 30 e o 31 não existem em todo mês)."));
                if (Hora is null) problemas.Add(("hora", "Informe a hora, no formato HH:mm."));
                break;
            case CadenciaDaRotina.Mensal:
                if (Dia is not (>= 1 and <= 28)) problemas.Add(("dia", "Informe o dia, de 1 a 28 (o 29, o 30 e o 31 não existem em todo mês)."));
                if (Hora is null) problemas.Add(("hora", "Informe a hora, no formato HH:mm."));
                break;
            case CadenciaDaRotina.Diaria:
                if (Hora is null) problemas.Add(("hora", "Informe a hora, no formato HH:mm."));
                break;
            case CadenciaDaRotina.Intervalo:
                if (IntervaloMinutos is not (>= IntervaloMinimo and <= IntervaloMaximo))
                    problemas.Add(("intervaloMinutos", $"Informe de {IntervaloMinimo} a {IntervaloMaximo} minutos."));
                break;
            default:
                problemas.Add(("cadencia", "Cadência fora da lista."));
                break;
        }

        return problemas;
    }

    /// <summary>Só os campos que a cadência usa — o resto vira nulo, para o banco não guardar um mês que não vale.</summary>
    public AgendaDaRotina Normalizada() => Cadencia switch
    {
        CadenciaDaRotina.Anual => this with { IntervaloMinutos = null },
        CadenciaDaRotina.Mensal => this with { Mes = null, IntervaloMinutos = null },
        CadenciaDaRotina.Diaria => this with { Mes = null, Dia = null, IntervaloMinutos = null },
        _ => this with { Mes = null, Dia = null, Hora = null }
    };

    /// <summary>
    /// A execução agendada mais recente até o instante (UTC). Na cadência por intervalo não há calendário: a
    /// cobrança é "um intervalo atrás".
    /// </summary>
    public DateTime UltimaPrevistaAte(DateTime agoraUtc)
    {
        if (Cadencia == CadenciaDaRotina.Intervalo) return agoraUtc.AddMinutes(-(IntervaloMinutos ?? IntervaloMinimo));

        var local = agoraUtc + FusoDeSaoPaulo;
        var candidata = Agendada(local);
        if (candidata > local) candidata = Anterior(candidata);
        return DateTime.SpecifyKind(candidata - FusoDeSaoPaulo, DateTimeKind.Utc);
    }

    /// <summary>A próxima execução agendada depois do instante (UTC).</summary>
    public DateTime ProximaDepoisDe(DateTime agoraUtc)
    {
        if (Cadencia == CadenciaDaRotina.Intervalo) return agoraUtc.AddMinutes(IntervaloMinutos ?? IntervaloMinimo);

        var local = agoraUtc + FusoDeSaoPaulo;
        var candidata = Agendada(local);
        if (candidata <= local) candidata = Seguinte(candidata);
        return DateTime.SpecifyKind(candidata - FusoDeSaoPaulo, DateTimeKind.Utc);
    }

    /// <summary>A agenda em português, como a tela mostra.</summary>
    public string Descrever()
    {
        var hora = Hora?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? "--:--";
        return Cadencia switch
        {
            CadenciaDaRotina.Anual => $"todo ano em {(Dia == 1 ? "1º" : Dia?.ToString(CultureInfo.InvariantCulture))} de {NomesDosMeses[Math.Clamp((Mes ?? 1) - 1, 0, 11)]}, às {hora}",
            CadenciaDaRotina.Mensal => $"todo mês no dia {Dia}, às {hora}",
            CadenciaDaRotina.Diaria => $"todo dia às {hora}",
            _ => IntervaloMinutos is { } m && m % 60 == 0
                ? (m == 60 ? "a cada hora" : $"a cada {m / 60} horas")
                : $"a cada {IntervaloMinutos} minutos"
        };
    }

    private DateTime Agendada(DateTime local)
    {
        var hora = Hora ?? TimeOnly.MinValue;
        return Cadencia switch
        {
            CadenciaDaRotina.Anual => new DateOnly(local.Year, Mes ?? 1, Dia ?? 1).ToDateTime(hora),
            CadenciaDaRotina.Mensal => new DateOnly(local.Year, local.Month, Dia ?? 1).ToDateTime(hora),
            _ => DateOnly.FromDateTime(local).ToDateTime(hora)
        };
    }

    private DateTime Anterior(DateTime agendada) => Cadencia switch
    {
        CadenciaDaRotina.Anual => agendada.AddYears(-1),
        CadenciaDaRotina.Mensal => agendada.AddMonths(-1),
        _ => agendada.AddDays(-1)
    };

    private DateTime Seguinte(DateTime agendada) => Cadencia switch
    {
        CadenciaDaRotina.Anual => agendada.AddYears(1),
        CadenciaDaRotina.Mensal => agendada.AddMonths(1),
        _ => agendada.AddDays(1)
    };
}

/// <summary>Como terminou a última verificação de uma conexão (o botão "Testar" ou o monitoramento).</summary>
public enum SituacaoDaVerificacao
{
    /// <summary>Ninguém testou ainda.</summary>
    NuncaVerificada = 0,

    /// <summary>Respondeu como esperado.</summary>
    NoAr = 1,

    /// <summary>Não respondeu, ou respondeu errado.</summary>
    ComFalha = 2
}

/// <summary>Como o CRM fala com o sistema — e, com isso, que campos a conexão tem e como se testa.</summary>
public enum TipoDeConexao
{
    /// <summary>API REST com usuário e senha (o Protheus): endereço-base, usuário e senha.</summary>
    ApiRest = 0,

    /// <summary>Banco SQL Server de leitura: servidor, banco, usuário e senha.</summary>
    SqlServer = 1,

    /// <summary>Banco MySQL de leitura (o ART): servidor, porta, banco, visão, usuário e senha.</summary>
    MySql = 2,

    /// <summary>Fonte pública aberta: o endereço vem do leitor, que é escrito para aquele formato.</summary>
    FontePublica = 3,

    /// <summary>API cadastrada pela tela só para ser monitorada: endereço, status esperado e, se pedir, um segredo.</summary>
    Monitorada = 4
}

/// <summary>
/// UMA CONEXÃO — o sistema com que o CRM fala, onde ele está e, para quem precisa, a credencial.
///
/// <para><b>A senha nunca sai daqui aberta.</b> <see cref="SegredoProtegido"/> é o que
/// <c>IProtetorDeSegredos</c> devolveu; só o servidor a abre, na hora de usar ou de testar. A API responde
/// "configurada em tal data por fulano" e nada mais.</para>
///
/// <para><b>O endereço de uma fonte pública não se edita.</b> O leitor de cada uma é escrito para o formato
/// daquele endereço — trocar o endereço pela tela quebraria a leitura em silêncio. Muda com código.</para>
/// </summary>
public sealed partial class Conexao
{
    /// <summary>O tamanho do resumo do último teste.</summary>
    public const int TamanhoDoResumo = 400;

    private Conexao() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O código estável (<c>PROTHEUS</c>, <c>ART</c>…).</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>O nome que a tela mostra.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Para que serve.</summary>
    public string? Descricao { get; private set; }

    /// <summary>Como o CRM fala com ele.</summary>
    public TipoDeConexao Tipo { get; private set; }

    /// <summary>Se vem do código (os sistemas que o CRM já lê) ou foi cadastrada pela tela.</summary>
    public bool EhDoSistema { get; private set; }

    /// <summary>O endereço: a URL-base, o servidor do banco ou o endereço da fonte.</summary>
    public string? Endereco { get; private set; }

    /// <summary>A porta, quando não é a padrão.</summary>
    public int? Porta { get; private set; }

    /// <summary>O banco.</summary>
    public string? Banco { get; private set; }

    /// <summary>O objeto lido — a visão do ART.</summary>
    public string? Objeto { get; private set; }

    /// <summary>O usuário. Não é segredo: a senha é.</summary>
    public string? Usuario { get; private set; }

    /// <summary>Na API monitorada, o cabeçalho que leva o segredo (<c>Authorization</c>, <c>X-Api-Key</c>).</summary>
    public string? NomeDoCabecalho { get; private set; }

    /// <summary>A senha (ou o segredo do cabeçalho), protegida. Nulo: não há credencial pela tela.</summary>
    public byte[]? SegredoProtegido { get; private set; }

    /// <summary>Quando a credencial foi gravada ou retirada pela tela.</summary>
    public DateTime? SegredoAlteradoEm { get; private set; }

    /// <summary>Quem gravou ou retirou a credencial.</summary>
    public long? SegredoAlteradoPorId { get; private set; }

    /// <summary>Na monitorada, o status HTTP que conta como "no ar".</summary>
    public int StatusEsperado { get; private set; } = 200;

    /// <summary>De quanto em quanto tempo o orquestrador testa sozinho; nulo, só pelo botão.</summary>
    public int? MinutosEntreVerificacoes { get; private set; }

    /// <summary>Se entra no monitoramento. Só a monitorada se desliga: as do sistema são o que o CRM lê.</summary>
    public bool EstaAtiva { get; private set; } = true;

    /// <summary>Quando foi testada pela última vez.</summary>
    public DateTime? UltimaVerificacaoEm { get; private set; }

    /// <summary>Como terminou a última verificação — nunca verificada, no ar ou com falha.</summary>
    public SituacaoDaVerificacao UltimaVerificacao { get; private set; } = SituacaoDaVerificacao.NuncaVerificada;

    /// <summary>O que o último teste disse, sem credencial.</summary>
    public string? UltimaVerificacaoResumo { get; private set; }

    /// <summary>Se há credencial gravada pela tela.</summary>
    public bool TemSegredo => SegredoProtegido is { Length: > 0 };

    /// <summary>Se o tipo tem credencial.</summary>
    public bool AceitaSegredo => Tipo is TipoDeConexao.ApiRest or TipoDeConexao.SqlServer or TipoDeConexao.MySql or TipoDeConexao.Monitorada;

    /// <summary>Se o endereço se edita pela tela.</summary>
    public bool EnderecoEditavel => Tipo != TipoDeConexao.FontePublica;

    /// <summary>Se tem o que precisa para ser usada pela tela: endereço e, quando o tipo pede, usuário e senha.</summary>
    public bool EstaConfiguradaPelaTela => Tipo switch
    {
        TipoDeConexao.FontePublica => true,
        TipoDeConexao.Monitorada => !string.IsNullOrWhiteSpace(Endereco),
        TipoDeConexao.MySql => !string.IsNullOrWhiteSpace(Endereco) && !string.IsNullOrWhiteSpace(Banco) && !string.IsNullOrWhiteSpace(Objeto)
                               && !string.IsNullOrWhiteSpace(Usuario) && TemSegredo,
        TipoDeConexao.SqlServer => !string.IsNullOrWhiteSpace(Endereco) && !string.IsNullOrWhiteSpace(Banco) && !string.IsNullOrWhiteSpace(Usuario) && TemSegredo,
        _ => !string.IsNullOrWhiteSpace(Endereco) && !string.IsNullOrWhiteSpace(Usuario) && TemSegredo
    };

    /// <summary>Uma conexão do sistema, como o catálogo a descreve (a migração semeia; o teste usa).</summary>
    public static Conexao DoCatalogo(ConexaoDoSistema item) => new()
    {
        Codigo = item.Codigo,
        Nome = item.Nome,
        Descricao = item.Descricao,
        Tipo = item.Tipo,
        EhDoSistema = true,
        Endereco = item.Endereco
    };

    /// <summary>Uma API cadastrada pela tela para ser monitorada.</summary>
    public static Conexao CriarMonitorada(string codigo, string nome, string? descricao, string endereco, int statusEsperado, int? minutosEntreVerificacoes)
    {
        var problemas = ProblemasDaMonitorada(endereco, null, statusEsperado, minutosEntreVerificacoes);
        if (!FormatoDoCodigo().IsMatch(codigo)) problemas.Insert(0, ("codigo", "código fora do formato"));
        if (problemas.Count > 0) throw new RegraDeNegocioViolada($"A API monitorada tem campos a corrigir: {string.Join("; ", problemas.Select(p => p.Mensagem))}");

        return new Conexao
        {
            Codigo = codigo,
            Nome = nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            Tipo = TipoDeConexao.Monitorada,
            Endereco = endereco.Trim(),
            StatusEsperado = statusEsperado,
            MinutosEntreVerificacoes = minutosEntreVerificacoes
        };
    }

    /// <summary>
    /// O que está errado nos campos de endereço, conforme o tipo — a mesma conferência que a tela recebe campo a
    /// campo e que o domínio repete antes de gravar.
    ///
    /// <para><b>Servidor, banco e visão só com letras, números e poucos sinais</b>: eles entram numa cadeia de
    /// conexão e, a visão, numa consulta. Um ponto e vírgula num servidor mudaria a cadeia inteira.</para>
    /// </summary>
    public static List<(string Campo, string Mensagem)> ProblemasDoEndereco(
        TipoDeConexao tipo, string? endereco, int? porta, string? banco, string? objeto, string? nomeDoCabecalho)
    {
        var problemas = new List<(string, string)>();
        switch (tipo)
        {
            case TipoDeConexao.FontePublica:
                problemas.Add(("endereco", "O endereço de uma fonte pública vem do leitor, escrito para aquele formato; ele muda com código, não pela tela."));
                break;
            case TipoDeConexao.ApiRest:
            case TipoDeConexao.Monitorada:
                if (!EhUrl(endereco)) problemas.Add(("endereco", "Informe uma URL completa, começando por http:// ou https://."));
                if (!string.IsNullOrWhiteSpace(nomeDoCabecalho) && !NomeDeCabecalho().IsMatch(nomeDoCabecalho))
                    problemas.Add(("nomeDoCabecalho", "Use só letras, números e hífen (ex.: Authorization, X-Api-Key)."));
                break;
            case TipoDeConexao.SqlServer:
            case TipoDeConexao.MySql:
                if (string.IsNullOrWhiteSpace(endereco) || !NomeDeServidor().IsMatch(endereco.Trim()))
                    problemas.Add(("endereco", "Informe o servidor: nome ou IP, e a instância ou a porta depois de \\ ou vírgula."));
                if (string.IsNullOrWhiteSpace(banco) || !NomeSimples().IsMatch(banco.Trim()))
                    problemas.Add(("banco", "Informe o banco: letras, números, sublinhado e hífen."));
                if (porta is not null and not (>= 1 and <= 65535)) problemas.Add(("porta", "A porta vai de 1 a 65535."));
                if (tipo == TipoDeConexao.MySql && (string.IsNullOrWhiteSpace(objeto) || !NomeDeObjeto().IsMatch(objeto.Trim())))
                    problemas.Add(("objeto", "Informe a visão lida: letras, números e sublinhado, com o banco antes do ponto se for de outro."));
                break;
        }

        return problemas;
    }

    /// <summary>O que está errado nos campos de uma API monitorada.</summary>
    public static List<(string Campo, string Mensagem)> ProblemasDaMonitorada(string? endereco, string? nomeDoCabecalho, int statusEsperado, int? minutosEntreVerificacoes)
    {
        var problemas = ProblemasDoEndereco(TipoDeConexao.Monitorada, endereco, null, null, null, nomeDoCabecalho);
        if (statusEsperado is not (>= 100 and <= 599)) problemas.Add(("statusEsperado", "O status esperado é um código HTTP, de 100 a 599."));
        if (minutosEntreVerificacoes is not null and not (>= AgendaDaRotina.IntervaloMinimo and <= AgendaDaRotina.IntervaloMaximo))
            problemas.Add(("minutosEntreVerificacoes", $"De {AgendaDaRotina.IntervaloMinimo} a {AgendaDaRotina.IntervaloMaximo} minutos, ou vazio para testar só pelo botão."));
        return problemas;
    }

    /// <summary>Troca o endereço, o banco, a visão e o usuário. A senha é à parte (<see cref="DefinirSegredo"/>).</summary>
    public void Configurar(string? endereco, int? porta, string? banco, string? objeto, string? usuario, string? nomeDoCabecalho)
    {
        var problemas = ProblemasDoEndereco(Tipo, endereco, porta, banco, objeto, nomeDoCabecalho);
        if (problemas.Count > 0) throw new RegraDeNegocioViolada(string.Join(" ", problemas.Select(p => p.Mensagem)));

        Endereco = Aparar(endereco);
        Porta = Tipo == TipoDeConexao.MySql ? porta : null;
        Banco = Tipo is TipoDeConexao.SqlServer or TipoDeConexao.MySql ? Aparar(banco) : null;
        Objeto = Tipo == TipoDeConexao.MySql ? Aparar(objeto) : null;
        Usuario = Tipo is TipoDeConexao.Monitorada ? null : Aparar(usuario);
        NomeDoCabecalho = Tipo == TipoDeConexao.Monitorada ? Aparar(nomeDoCabecalho) : null;
    }

    /// <summary>Troca o nome, a descrição e o monitoramento de uma API cadastrada pela tela.</summary>
    public void AjustarMonitoramento(string nome, string? descricao, int statusEsperado, int? minutosEntreVerificacoes)
    {
        if (Tipo != TipoDeConexao.Monitorada)
            throw new RegraDeNegocioViolada("Só a API cadastrada pela tela tem status esperado e nome editáveis.");
        var problemas = ProblemasDaMonitorada(Endereco, NomeDoCabecalho, statusEsperado, minutosEntreVerificacoes);
        if (problemas.Count > 0) throw new RegraDeNegocioViolada(string.Join(" ", problemas.Select(p => p.Mensagem)));
        if (string.IsNullOrWhiteSpace(nome)) throw new RegraDeNegocioViolada("A API precisa de um nome.");

        Nome = nome.Trim();
        Descricao = Aparar(descricao);
        StatusEsperado = statusEsperado;
        MinutosEntreVerificacoes = minutosEntreVerificacoes;
    }

    /// <summary>Grava a credencial, já protegida.</summary>
    public void DefinirSegredo(byte[] protegido, long porId, DateTime agoraUtc)
    {
        if (!AceitaSegredo) throw new RegraDeNegocioViolada("Esta conexão não tem credencial: é uma fonte pública aberta.");
        if (protegido is not { Length: > 0 }) throw new RegraDeNegocioViolada("A credencial protegida veio vazia.");

        SegredoProtegido = protegido;
        SegredoAlteradoEm = agoraUtc;
        SegredoAlteradoPorId = porId;
    }

    /// <summary>Retira a credencial da tela — a conexão volta a usar a variável de ambiente do servidor, se houver.</summary>
    public void RemoverSegredo(long porId, DateTime agoraUtc)
    {
        if (!TemSegredo) return;
        SegredoProtegido = null;
        SegredoAlteradoEm = agoraUtc;
        SegredoAlteradoPorId = porId;
    }

    /// <summary>Guarda o resultado do último teste.</summary>
    public void RegistrarVerificacao(bool ok, string resumo, DateTime quandoUtc)
    {
        UltimaVerificacaoEm = quandoUtc;
        UltimaVerificacao = ok ? SituacaoDaVerificacao.NoAr : SituacaoDaVerificacao.ComFalha;
        UltimaVerificacaoResumo = resumo.Length > TamanhoDoResumo ? resumo[..TamanhoDoResumo] : resumo;
    }

    /// <summary>Tira do monitoramento, sem apagar.</summary>
    public void Desativar()
    {
        if (EhDoSistema) throw new RegraDeNegocioViolada("As conexões do sistema são o que o CRM lê: desligue a rotina que as usa, não a conexão.");
        EstaAtiva = false;
    }

    /// <summary>Volta ao monitoramento.</summary>
    public void Reativar()
    {
        if (EhDoSistema) return;
        EstaAtiva = true;
    }

    /// <summary>Se o orquestrador deve testá-la agora.</summary>
    public bool VerificacaoVencida(DateTime agoraUtc) =>
        EstaAtiva && MinutosEntreVerificacoes is { } minutos && (UltimaVerificacaoEm is null || UltimaVerificacaoEm.Value.AddMinutes(minutos) <= agoraUtc);

    private static string? Aparar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static bool EhUrl(string? texto) =>
        Uri.TryCreate(texto?.Trim(), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    /// <summary>O formato do código de uma API cadastrada pela tela.</summary>
    [GeneratedRegex("^[A-Z][A-Z0-9_]{2,39}$")]
    public static partial Regex FormatoDoCodigo();

    [GeneratedRegex(@"^[A-Za-z0-9._\-]+(\\[A-Za-z0-9_\-]+)?(,[0-9]{1,5})?$")]
    private static partial Regex NomeDeServidor();

    [GeneratedRegex(@"^[A-Za-z0-9_\-]+$")]
    private static partial Regex NomeSimples();

    [GeneratedRegex(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)?$")]
    private static partial Regex NomeDeObjeto();

    [GeneratedRegex(@"^[A-Za-z0-9\-]+$")]
    private static partial Regex NomeDeCabecalho();
}

/// <summary>UM TESTE DE CONEXÃO — quem apertou "Testar" (ou o orquestrador), quando, e o que respondeu.</summary>
public sealed class VerificacaoDeConexao
{
    private VerificacaoDeConexao() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A conexão testada.</summary>
    public int ConexaoId { get; private set; }

    /// <summary>Quando (UTC).</summary>
    public DateTime VerificadaEm { get; private set; }

    /// <summary>Quem apertou "Testar"; nulo quando foi o orquestrador.</summary>
    public long? VerificadaPorId { get; private set; }

    /// <summary>Se passou.</summary>
    public bool Ok { get; private set; }

    /// <summary>O que respondeu, sem credencial.</summary>
    public string Resumo { get; private set; } = default!;

    /// <summary>Quanto levou, em milissegundos.</summary>
    public int LatenciaMs { get; private set; }

    /// <summary>Registra um teste.</summary>
    public static VerificacaoDeConexao Registrar(int conexaoId, DateTime quandoUtc, long? porId, bool ok, string resumo, int latenciaMs) => new()
    {
        ConexaoId = conexaoId,
        VerificadaEm = quandoUtc,
        VerificadaPorId = porId,
        Ok = ok,
        Resumo = resumo.Length > Conexao.TamanhoDoResumo ? resumo[..Conexao.TamanhoDoResumo] : resumo,
        LatenciaMs = Math.Max(0, latenciaMs)
    };
}

/// <summary>Por que uma execução de rotina começou.</summary>
public enum MotivoDaExecucao
{
    /// <summary>Chegou a hora da agenda.</summary>
    Agenda = 0,

    /// <summary>Alguém apertou "Rodar agora".</summary>
    Pedido = 1,

    /// <summary>A rotina nunca rodou e alguma tabela dela está vazia — a primeira carga não espera o calendário.</summary>
    PrimeiraCarga = 2
}

/// <summary>
/// UMA ROTINA DO SERVIDOR — o que roda sozinho, com que agenda, ligado ou não.
///
/// <para><b>O que ela roda vem do código</b> (<see cref="RotinasDoSistema"/>): a tela muda QUANDO, não O QUE.
/// Uma linha de comando editável pela tela seria uma porta para rodar qualquer coisa no servidor.</para>
///
/// <para><b>Mudar a agenda ou religar não cobra o atraso.</b> <see cref="AgendaVigenteDesde"/> marca desde quando a
/// agenda vale: a execução "de setembro" de uma rotina religada em outubro não roda de repente.</para>
/// </summary>
public sealed class Rotina
{
    private const int TamanhoDaMensagem = 1000;

    private Rotina() { }

    /// <summary>Identificador interno.</summary>
    public int Id { get; private set; }

    /// <summary>O código estável, o mesmo de <see cref="RotinasDoSistema"/>.</summary>
    public string Codigo { get; private set; } = default!;

    /// <summary>O nome que a tela mostra.</summary>
    public string Nome { get; private set; } = default!;

    /// <summary>Anual, mensal, diária ou por intervalo.</summary>
    public CadenciaDaRotina Cadencia { get; private set; }

    /// <summary>O mês, na anual.</summary>
    public int? Mes { get; private set; }

    /// <summary>O dia, na anual e na mensal.</summary>
    public int? Dia { get; private set; }

    /// <summary>A hora de São Paulo, nas de calendário.</summary>
    public TimeOnly? Hora { get; private set; }

    /// <summary>Os minutos, na por intervalo.</summary>
    public int? IntervaloMinutos { get; private set; }

    /// <summary>Se roda pela agenda. Desligada, só roda se alguém pedir.</summary>
    public bool EstaLigada { get; private set; }

    /// <summary>Desde quando a agenda atual vale (UTC): o que ficou para trás dela não é cobrado.</summary>
    public DateTime AgendaVigenteDesde { get; private set; }

    /// <summary>Quando alguém pediu "Rodar agora"; nulo quando não há pedido na fila.</summary>
    public DateTime? ExecucaoPedidaEm { get; private set; }

    /// <summary>Quem pediu.</summary>
    public long? ExecucaoPedidaPorId { get; private set; }

    /// <summary>Quando a última execução começou.</summary>
    public DateTime? UltimaExecucaoIniciadaEm { get; private set; }

    /// <summary>Quando a última execução terminou.</summary>
    public DateTime? UltimaExecucaoTerminadaEm { get; private set; }

    /// <summary>Como terminou a última.</summary>
    public ResultadoDaExecucao? UltimoResultado { get; private set; }

    /// <summary>O resumo da última, sem credencial.</summary>
    public string? UltimaMensagem { get; private set; }

    /// <summary>A agenda.</summary>
    public AgendaDaRotina Agenda => new(Cadencia, Mes, Dia, Hora, IntervaloMinutos);

    /// <summary>Uma rotina do catálogo, com a agenda padrão (a migração semeia; o teste usa).</summary>
    public static Rotina DoCatalogo(RotinaDoSistema item, DateTime vigenteDesdeUtc) => new()
    {
        Codigo = item.Codigo,
        Nome = item.Nome,
        Cadencia = item.AgendaPadrao.Cadencia,
        Mes = item.AgendaPadrao.Mes,
        Dia = item.AgendaPadrao.Dia,
        Hora = item.AgendaPadrao.Hora,
        IntervaloMinutos = item.AgendaPadrao.IntervaloMinutos,
        EstaLigada = item.LigadaPorPadrao,
        AgendaVigenteDesde = vigenteDesdeUtc
    };

    /// <summary>Troca a agenda. O que ficou para trás da agenda antiga não é cobrado.</summary>
    public void Reagendar(AgendaDaRotina agenda, DateTime agoraUtc)
    {
        var problemas = agenda.Problemas();
        if (problemas.Count > 0) throw new RegraDeNegocioViolada(string.Join(" ", problemas.Select(p => p.Mensagem)));

        var normalizada = agenda.Normalizada();
        if (normalizada == Agenda.Normalizada()) return;

        Cadencia = normalizada.Cadencia;
        Mes = normalizada.Mes;
        Dia = normalizada.Dia;
        Hora = normalizada.Hora;
        IntervaloMinutos = normalizada.IntervaloMinutos;
        AgendaVigenteDesde = agoraUtc;
    }

    /// <summary>Liga. A agenda passa a valer daqui para a frente.</summary>
    public void Ligar(DateTime agoraUtc)
    {
        if (EstaLigada) return;
        EstaLigada = true;
        AgendaVigenteDesde = agoraUtc;
    }

    /// <summary>Desliga: a agenda para; um pedido na fila continua valendo.</summary>
    public void Desligar() => EstaLigada = false;

    /// <summary>Põe um "Rodar agora" na fila. O primeiro pedido vale; repetir não duplica.</summary>
    public void PedirExecucao(long porId, DateTime agoraUtc)
    {
        if (ExecucaoPedidaEm is not null) return;
        ExecucaoPedidaEm = agoraUtc;
        ExecucaoPedidaPorId = porId;
    }

    /// <summary>Se o orquestrador deve rodá-la agora — por pedido, ou porque a agenda chegou.</summary>
    public bool EstaVencida(DateTime agoraUtc)
    {
        if (ExecucaoPedidaEm is not null) return true;
        if (!EstaLigada) return false;

        var referencia = UltimaExecucaoIniciadaEm is { } ultima && ultima > AgendaVigenteDesde ? ultima : AgendaVigenteDesde;
        return Cadencia == CadenciaDaRotina.Intervalo
            ? referencia.AddMinutes(IntervaloMinutos ?? AgendaDaRotina.IntervaloMinimo) <= agoraUtc
            : Agenda.UltimaPrevistaAte(agoraUtc) > referencia;
    }

    /// <summary>A próxima execução pela agenda; nulo quando está desligada.</summary>
    public DateTime? ProximaExecucao(DateTime agoraUtc)
    {
        if (!EstaLigada) return null;
        if (EstaVencida(agoraUtc)) return agoraUtc;

        if (Cadencia != CadenciaDaRotina.Intervalo) return Agenda.ProximaDepoisDe(agoraUtc);

        var referencia = UltimaExecucaoIniciadaEm is { } ultima && ultima > AgendaVigenteDesde ? ultima : AgendaVigenteDesde;
        return referencia.AddMinutes(IntervaloMinutos ?? AgendaDaRotina.IntervaloMinimo);
    }

    /// <summary>Por que ela rodaria agora.</summary>
    public MotivoDaExecucao MotivoAgora() => ExecucaoPedidaEm is not null ? MotivoDaExecucao.Pedido : MotivoDaExecucao.Agenda;

    /// <summary>Marca o começo de uma execução e tira o pedido da fila. Devolve quem tinha pedido.</summary>
    public long? IniciarExecucao(DateTime agoraUtc)
    {
        var pedidoPor = ExecucaoPedidaPorId;
        ExecucaoPedidaEm = null;
        ExecucaoPedidaPorId = null;
        UltimaExecucaoIniciadaEm = agoraUtc;
        UltimaExecucaoTerminadaEm = null;
        UltimoResultado = ResultadoDaExecucao.EmAndamento;
        UltimaMensagem = null;
        return pedidoPor;
    }

    /// <summary>Marca o fim da execução.</summary>
    public void EncerrarExecucao(ResultadoDaExecucao resultado, string? mensagem, DateTime agoraUtc)
    {
        UltimaExecucaoTerminadaEm = agoraUtc;
        UltimoResultado = resultado;
        UltimaMensagem = mensagem is { Length: > TamanhoDaMensagem } ? mensagem[..TamanhoDaMensagem] : mensagem;
    }
}

/// <summary>UMA EXECUÇÃO DE ROTINA — quando, por quê, quem pediu, como terminou.</summary>
public sealed class ExecucaoDeRotina
{
    private const int TamanhoDaMensagem = 1000;

    private ExecucaoDeRotina() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>A rotina.</summary>
    public int RotinaId { get; private set; }

    /// <summary>Por que começou.</summary>
    public MotivoDaExecucao Motivo { get; private set; }

    /// <summary>Quem pediu, quando foi "Rodar agora".</summary>
    public long? PedidaPorId { get; private set; }

    /// <summary>Em que máquina rodou.</summary>
    public string Maquina { get; private set; } = default!;

    /// <summary>Quando começou (UTC).</summary>
    public DateTime IniciadaEm { get; private set; }

    /// <summary>Quando terminou (UTC).</summary>
    public DateTime? TerminadaEm { get; private set; }

    /// <summary>Como terminou.</summary>
    public ResultadoDaExecucao Resultado { get; private set; } = ResultadoDaExecucao.EmAndamento;

    /// <summary>O pior código de saída das cargas.</summary>
    public int? CodigoDeSaida { get; private set; }

    /// <summary>O resumo, sem credencial.</summary>
    public string? Mensagem { get; private set; }

    /// <summary>Começa uma execução.</summary>
    public static ExecucaoDeRotina Iniciar(int rotinaId, MotivoDaExecucao motivo, long? pedidaPorId, string maquina, DateTime agoraUtc) => new()
    {
        RotinaId = rotinaId,
        Motivo = motivo,
        PedidaPorId = pedidaPorId,
        Maquina = maquina,
        IniciadaEm = agoraUtc
    };

    /// <summary>Encerra.</summary>
    public void Encerrar(ResultadoDaExecucao resultado, int? codigoDeSaida, string? mensagem, DateTime agoraUtc)
    {
        Resultado = resultado;
        CodigoDeSaida = codigoDeSaida;
        Mensagem = mensagem is { Length: > TamanhoDaMensagem } ? mensagem[..TamanhoDaMensagem] : mensagem;
        TerminadaEm = agoraUtc;
    }
}

/// <summary>Uma conexão que o CRM já usa, como o código a conhece.</summary>
/// <param name="Codigo">O código estável.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Tipo">Como se fala com ele.</param>
/// <param name="Descricao">Para que o CRM a usa.</param>
/// <param name="Endereco">O endereço fixo das fontes públicas — o que o botão "Testar" consulta.</param>
public sealed record ConexaoDoSistema(string Codigo, string Nome, TipoDeConexao Tipo, string Descricao, string? Endereco = null);

/// <summary>
/// AS CONEXÕES QUE O CRM JÁ USA. Cada endereço de fonte pública aqui existe no leitor dela, em
/// <c>Tracbel.Crm.Integracao</c> — <c>ConexoesDoSistemaTestes</c> procura cada um no código.
/// </summary>
public static class ConexoesDoSistema
{
    /// <summary>A API REST do Protheus — o faturamento.</summary>
    public const string Protheus = "PROTHEUS";

    /// <summary>O banco do Protheus, só leitura — o dono do chassi para o ART.</summary>
    public const string ProtheusBanco = "PROTHEUS_BANCO";

    /// <summary>O ART — as vendas de máquina.</summary>
    public const string Art = "ART";

    /// <summary>O Vórtice — o sistema legado, congelado.</summary>
    public const string Vortice = "VORTICE";

    /// <summary>As conexões, na ordem da tela.</summary>
    public static readonly IReadOnlyList<ConexaoDoSistema> Todas =
    [
        new(Protheus, "Protheus — API REST", TipoDeConexao.ApiRest,
            "O faturamento (SD2) lido direto do ERP. Só leitura: o pedido de token é autenticação, não escrita."),
        new(ProtheusBanco, "Protheus — banco (leitura)", TipoDeConexao.SqlServer,
            "O dono do chassi e o cadastro do comprador, para a integração do ART. Sessão somente leitura."),
        new(Art, "ART — vendas de máquina", TipoDeConexao.MySql,
            "A view de vendas de máquina liberada para o CRM. Sessão somente leitura."),
        new(Vortice, "Vórtice — sistema legado", TipoDeConexao.SqlServer,
            "A busca ao vivo no legado, congelado desde a fase 1 (somente referência)."),
        new("IBGE_SIDRA", "IBGE — SIDRA", TipoDeConexao.FontePublica,
            "Produção agrícola, Censo Agropecuário, rebanho e área territorial.",
            "https://servicodados.ibge.gov.br/api/v3/agregados/5457/metadados"),
        new("IBGE_LOCALIDADES", "IBGE — municípios e malhas", TipoDeConexao.FontePublica,
            "O catálogo oficial de municípios e o contorno de cada um.",
            "https://servicodados.ibge.gov.br/api/v1/localidades/municipios?view=nivelado"),
        new("ANP", "ANP — usinas de etanol", TipoDeConexao.FontePublica,
            "As usinas autorizadas, com capacidade de produção.",
            "https://www.gov.br/anp/pt-br/assuntos/producao-e-fornecimento-de-biocombustiveis/etanol/arquivos-etanol/pb-da-etanol.zip"),
        new("CONAB_PRECOS", "CONAB — preço recebido", TipoDeConexao.FontePublica,
            "O preço mensal das culturas em SP.",
            "https://portaldeinformacoes.conab.gov.br/downloads/arquivos/PrecosMensalUF.txt"),
        new("CONAB_CUSTOS", "CONAB — custo de produção", TipoDeConexao.FontePublica,
            "As séries de custo de produção por cultura.",
            "https://www.gov.br/conab/pt-br/atuacao/informacoes-agropecuarias/custos-de-producao/planilhas-de-custos-de-producao/copy_of_agricolas"),
        new("SOCICANA", "Socicana — preço do ATR", TipoDeConexao.FontePublica,
            "O preço do kg de ATR da cana.",
            "https://www.socicana.com.br/calculadora-de-atr/preco-do-kg/"),
        new("BCB_PTAX", "Banco Central — dólar PTAX", TipoDeConexao.FontePublica,
            "A média mensal do dólar de venda.",
            "https://api.bcb.gov.br/dados/serie/bcdata.sgs.3698/dados?formato=json"),
        new("BCB_SICOR", "Banco Central — SICOR", TipoDeConexao.FontePublica,
            "O crédito rural de investimento por município.",
            "https://olinda.bcb.gov.br/olinda/servico/SICOR/versao/v2/odata/InvestMunicipioProduto")
    ];
}

/// <summary>Uma rotina que o CRM roda no servidor, como o código a conhece.</summary>
/// <param name="Codigo">O código estável.</param>
/// <param name="Nome">O nome.</param>
/// <param name="Descricao">O que ela faz.</param>
/// <param name="Modos">As cargas que ela roda, na ordem (<c>--somente-pam</c>…).</param>
/// <param name="AgendaPadrao">A agenda que a migração semeia.</param>
/// <param name="LigadaPorPadrao">Se nasce ligada.</param>
/// <param name="Conexoes">As conexões que ela usa.</param>
/// <param name="ConexaoExigida">A conexão sem a qual ela não roda — precisa de credencial.</param>
public sealed record RotinaDoSistema(
    string Codigo, string Nome, string Descricao, IReadOnlyList<string> Modos, AgendaDaRotina AgendaPadrao,
    bool LigadaPorPadrao, IReadOnlyList<string> Conexoes, string? ConexaoExigida);

/// <summary>
/// AS ROTINAS DO SERVIDOR. As duas das fontes públicas nascem ligadas, com o calendário que as tarefas do Windows
/// tinham até 22/09/2026; o faturamento e o ART nascem DESLIGADOS — ligá-los é trazer dado novo para produção, e
/// isso é decisão de quem administra, não da migração.
/// </summary>
public static class RotinasDoSistema
{
    /// <summary>A PAM e a estrutura agropecuária.</summary>
    public const string FontesAnuais = "FONTES_ANUAIS";

    /// <summary>Preços, custos e crédito.</summary>
    public const string PrecosMensais = "PRECOS_MENSAIS";

    /// <summary>O faturamento do Protheus.</summary>
    public const string Faturamento = "FATURAMENTO_PROTHEUS";

    /// <summary>As vendas de máquina do ART.</summary>
    public const string ArtVendas = "ART_VENDAS";

    /// <summary>
    /// Quando as agendas semeadas passam a valer: o dia em que o orquestrador substituiu as tarefas do Windows. O
    /// que era devido antes dele (a mensal de 20/09) já rodou pelas tarefas antigas.
    /// </summary>
    public static readonly DateTime AgendaSemeadaDesde = new(2026, 9, 22, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>As rotinas, na ordem da tela.</summary>
    public static readonly IReadOnlyList<RotinaDoSistema> Todas =
    [
        new(FontesAnuais, "Fontes públicas anuais",
            "A produção agrícola do IBGE (PAM), o Censo Agropecuário, o rebanho, a área territorial e as usinas da ANP.",
            ["--somente-pam", "--somente-estrutura"], AgendaDaRotina.AnualEm(10, 1, new TimeOnly(3, 0)), true,
            ["IBGE_SIDRA", "IBGE_LOCALIDADES", "ANP"], null),
        new(PrecosMensais, "Preços, custos e crédito",
            "O preço recebido (CONAB), o ATR (Socicana), o dólar PTAX, o custo de produção (CONAB) e o crédito rural (SICOR).",
            ["--somente-precos", "--somente-custos", "--somente-credito"], AgendaDaRotina.MensalEm(20, new TimeOnly(4, 0)), true,
            ["CONAB_PRECOS", "SOCICANA", "BCB_PTAX", "CONAB_CUSTOS", "BCB_SICOR"], null),
        new(Faturamento, "Faturamento do Protheus",
            "As notas de saída (SD2) do ano, que alimentam o faturamento, a curva ABC e os indicadores da diretoria.",
            ["--somente-faturamento"], AgendaDaRotina.DiariaAs(new TimeOnly(5, 0)), false,
            [ConexoesDoSistema.Protheus], ConexoesDoSistema.Protheus),
        new(ArtVendas, "Vendas de máquina do ART",
            "As vendas de máquina do ART, conferidas com o dono do chassi no Protheus.",
            ["--somente-art"], AgendaDaRotina.ACada(60), false,
            [ConexoesDoSistema.Art, ConexoesDoSistema.ProtheusBanco], ConexoesDoSistema.Art)
    ];

    /// <summary>A rotina do catálogo pelo código; nula quando não existe.</summary>
    public static RotinaDoSistema? Obter(string codigo) => Todas.FirstOrDefault(r => string.Equals(r.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
}
