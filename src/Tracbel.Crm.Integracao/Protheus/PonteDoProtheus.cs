using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.Protheus;

/// <summary>
/// A ponte de leitura do TOTVS Protheus — e o único caminho por onde o CRM fala com o ERP.
///
/// ---------------------------------------------------------------------------------------------
/// COMO ESTE ACESSO FOI ENCONTRADO, porque ele não está em documentação nenhuma do fornecedor e
/// quem vier depois não vai adivinhar.
///
/// A API REST de produção publica quase nada: <c>/api/framework/v1/users</c> (SCIM) e mais nada
/// dos módulos. <c>GET /rest/</c> devolve uma tela de login em HTML, não um catálogo de serviços.
/// Todos os prefixos de módulo — <c>/api/fat</c>, <c>/api/est</c>, <c>/api/com</c> — dão 404.
///
/// <b>O que existe é <c>/api/framework/v1/genericQuery</c></b>, e ele foi achado porque respondeu
/// <b>400</b> — parâmetro faltando — onde todo o resto respondia <b>404</b>. A diferença entre os
/// dois códigos foi a única pista. Com <c>tables</c> e <c>fields</c>, ele lê qualquer tabela do
/// dicionário do Protheus.
///
/// ---------------------------------------------------------------------------------------------
/// TRÊS ARMADILHAS, todas medidas contra a produção, e todas tratadas aqui:
///
/// 1. <b><c>fields</c> é obrigatório e não aceita <c>*</c>.</b> Sem ele, 400; com asterisco, 500.
/// 2. <b>Campo inexistente não dá erro — some da resposta em silêncio.</b> Um agrupamento por
///    coluna ausente vira "vazio" para tudo, e o relatório sai errado sem ninguém perceber. Por
///    isso <see cref="LerPaginaAsync"/> devolve o dicionário cru: quem chama consegue conferir se
///    o campo que pediu voltou.
/// 3. <b>A <c>SX3</c> lista campos que a tabela física não tem.</b> <c>VV1_DESMAR</c> está no
///    dicionário e não volta na consulta. O dicionário do Protheus não é contrato.
///
/// ---------------------------------------------------------------------------------------------
/// SOMENTE LEITURA, e não por convenção: <b>esta classe não tem um único método que emita POST,
/// PUT ou DELETE</b> além do token — que é autenticação, não escrita. É um ERP de produção, e a
/// defesa de dentro é não haver caminho.
///
/// <para><b>Retentativa só em GET.</b> O AppServer devolve 428 e 503 intermitentes — o próprio
/// fornecedor documenta. Três tentativas com espera crescente é seguro numa leitura e seria
/// perigoso numa escrita; como aqui não há escrita, a questão não se coloca.</para>
/// </summary>
/// <param name="fabrica">De onde sai o <c>HttpClient</c>.</param>
/// <param name="opcoes">A configuração da ponte.</param>
public sealed partial class PonteDoProtheus(IHttpClientFactory fabrica, IOptions<OpcoesDoProtheus> opcoes)
{
    /// <summary>O nome do cliente HTTP registrado no contêiner.</summary>
    public const string NomeDoCliente = "Protheus";

    private const string NaoConfigurada =
        "A ponte do Protheus não está configurada. Ela exige as variáveis de ambiente " +
        "Protheus__Base, Protheus__Usuario e Protheus__Senha — que nunca moram em arquivo " +
        "versionado, porque a senha viaja na query string do endpoint de token.";

    /// <summary>Cinco minutos de folga antes de o token vencer.</summary>
    private static readonly TimeSpan FolgaDoToken = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly SemaphoreSlim _trava = new(1, 1);
    private string? _token;
    private DateTime _tokenValidoAte = DateTime.MinValue;

    /// <summary>
    /// Uma página de uma tabela do Protheus.
    /// </summary>
    /// <param name="tabela">O nome da tabela no dicionário. Ex.: <c>SD2</c>.</param>
    /// <param name="campos">Os campos, separados por vírgula. Obrigatório; <c>*</c> não vale.</param>
    /// <param name="onde">O filtro, na sintaxe do Protheus. Ex.: <c>D2_EMISSAO >= '20260101'</c>.</param>
    /// <param name="pagina">A página, a partir de 1.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<PaginaDoProtheus>> LerPaginaAsync(
        string tabela, string campos, string? onde, int pagina, CancellationToken ct)
    {
        var config = opcoes.Value;
        if (!config.EstaConfigurada) return Resultado<PaginaDoProtheus>.Indisponivel(NaoConfigurada);

        var token = await ObterTokenAsync(ct);
        if (!token.EhSucesso) return Resultado<PaginaDoProtheus>.Indisponivel(token.Erro!);

        var caminho =
            $"{config.Base}/api/framework/v1/genericQuery" +
            $"?tables={Uri.EscapeDataString(tabela)}" +
            $"&fields={Uri.EscapeDataString(campos)}" +
            $"&page={pagina}&pageSize={config.TamanhoDaPagina}";

        if (!string.IsNullOrWhiteSpace(onde)) caminho += $"&where={Uri.EscapeDataString(onde)}";

        for (var tentativa = 1; tentativa <= 3; tentativa++)
        {
            try
            {
                using var cliente = fabrica.CreateClient(NomeDoCliente);
                cliente.Timeout = TimeSpan.FromSeconds(config.TempoLimiteSegundos);

                using var pedido = new HttpRequestMessage(HttpMethod.Get, caminho);
                pedido.Headers.Authorization = new("Bearer", token.Valor);

                using var resposta = await cliente.SendAsync(pedido, ct);

                if (!resposta.IsSuccessStatusCode)
                {
                    if (tentativa < 3 && EhIntermitente(resposta.StatusCode))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2 * tentativa), ct);
                        continue;
                    }

                    return Resultado<PaginaDoProtheus>.Indisponivel(
                        $"O Protheus recusou a leitura de {tabela} (HTTP {(int)resposta.StatusCode}). " +
                        DicaPara(resposta.StatusCode));
                }

                var corpo = await resposta.Content.ReadAsStringAsync(ct);
                var lida = JsonSerializer.Deserialize<PaginaDoProtheus>(corpo, Json);

                return lida is null
                    ? Resultado<PaginaDoProtheus>.Indisponivel(
                        $"O Protheus devolveu uma resposta que não é a página esperada de {tabela}.")
                    : Resultado<PaginaDoProtheus>.Ok(lida);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                if (tentativa == 3)
                    return Resultado<PaginaDoProtheus>.Indisponivel(
                        $"O Protheus não respondeu a leitura de {tabela} em " +
                        $"{config.TempoLimiteSegundos}s.");

                await Task.Delay(TimeSpan.FromSeconds(2 * tentativa), ct);
            }
            catch (HttpRequestException erro)
            {
                if (tentativa == 3)
                    return Resultado<PaginaDoProtheus>.Indisponivel(
                        $"Não foi possível falar com o Protheus: {Ocultar(erro.Message)}");

                await Task.Delay(TimeSpan.FromSeconds(2 * tentativa), ct);
            }
        }

        return Resultado<PaginaDoProtheus>.Indisponivel($"Leitura de {tabela} esgotou as tentativas.");
    }

    /// <summary>
    /// A tabela inteira, página a página.
    /// </summary>
    /// <param name="tabela">O nome da tabela.</param>
    /// <param name="campos">Os campos, separados por vírgula.</param>
    /// <param name="onde">O filtro, ou nulo.</param>
    /// <param name="aoAvancar">Chamado a cada página, com o acumulado e o total. Para relatar.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<IReadOnlyList<Dictionary<string, JsonElement>>>> LerTudoAsync(
        string tabela,
        string campos,
        string? onde,
        Action<int, int>? aoAvancar,
        CancellationToken ct)
    {
        var todas = new List<Dictionary<string, JsonElement>>();
        var pagina = 1;

        while (true)
        {
            var lote = await LerPaginaAsync(tabela, campos, onde, pagina, ct);
            if (!lote.EhSucesso)
                return Resultado<IReadOnlyList<Dictionary<string, JsonElement>>>.Indisponivel(lote.Erro!);

            var itens = lote.Valor.Items ?? [];
            if (itens.Count == 0) break;

            todas.AddRange(itens);
            aoAvancar?.Invoke(todas.Count, lote.Valor.Total);

            if (!lote.Valor.HasNext) break;
            pagina++;
        }

        return Resultado<IReadOnlyList<Dictionary<string, JsonElement>>>.Ok(todas);
    }

    /// <summary>
    /// O token, renovado sozinho cinco minutos antes de vencer.
    ///
    /// <para>A renovação automática não é conforto: o token vale uma hora e uma extração da SD2
    /// inteira passa disso. Descobrir o vencimento no meio de trezentas páginas custaria a
    /// extração toda.</para>
    /// </summary>
    private async Task<Resultado<string>> ObterTokenAsync(CancellationToken ct)
    {
        if (_token is not null && DateTime.UtcNow < _tokenValidoAte) return Resultado<string>.Ok(_token);

        await _trava.WaitAsync(ct);
        try
        {
            if (_token is not null && DateTime.UtcNow < _tokenValidoAte) return Resultado<string>.Ok(_token);

            var config = opcoes.Value;
            var caminho =
                $"{config.Base}/api/oauth2/v1/token?grant_type=password" +
                $"&username={Uri.EscapeDataString(config.Usuario!)}" +
                $"&password={Uri.EscapeDataString(config.Senha!)}";

            using var cliente = fabrica.CreateClient(NomeDoCliente);
            cliente.Timeout = TimeSpan.FromSeconds(60);

            using var resposta = await cliente.PostAsync(caminho, null, ct);

            if (!resposta.IsSuccessStatusCode)
                return Resultado<string>.Indisponivel(
                    $"O Protheus recusou a autenticação (HTTP {(int)resposta.StatusCode}). " +
                    "Confira Protheus__Usuario e Protheus__Senha.");

            var corpo = await resposta.Content.ReadAsStringAsync(ct);
            var token = JsonSerializer.Deserialize<RespostaDeToken>(corpo, Json);

            if (token?.AccessToken is null)
                return Resultado<string>.Indisponivel("O Protheus autenticou mas não devolveu token.");

            _token = token.AccessToken;
            _tokenValidoAte = DateTime.UtcNow.AddSeconds(token.ExpiresIn) - FolgaDoToken;
            return Resultado<string>.Ok(_token);
        }
        catch (Exception erro) when (erro is HttpRequestException or TaskCanceledException)
        {
            return Resultado<string>.Indisponivel(
                $"Não foi possível autenticar no Protheus: {Ocultar(erro.Message)}");
        }
        finally
        {
            _trava.Release();
        }
    }

    /// <summary>
    /// Tira a senha de qualquer texto antes de ele virar log ou tela.
    ///
    /// <para>Não é excesso de zelo: a senha vai na URL, então toda exceção de rede que cite o
    /// endereço carrega a credencial junto. Sem este filtro, a primeira falha de conexão publica
    /// a senha do ERP no log da aplicação.</para>
    /// </summary>
    private static string Ocultar(string texto) => SenhaNaUrl().Replace(texto, "password=***");

    [GeneratedRegex(@"password=[^&\s""]*", RegexOptions.IgnoreCase)]
    private static partial Regex SenhaNaUrl();

    private static bool EhIntermitente(HttpStatusCode codigo) =>
        codigo is HttpStatusCode.PreconditionRequired
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout
            or HttpStatusCode.InternalServerError;

    private static string DicaPara(HttpStatusCode codigo) => codigo switch
    {
        HttpStatusCode.BadRequest =>
            "400 quase sempre é `fields` ausente — ele é obrigatório no genericQuery.",
        HttpStatusCode.InternalServerError =>
            "500 costuma ser campo que não existe na tabela física. Confira com uma amostra: " +
            "campo inexistente some da resposta em silêncio, e o dicionário SX3 lista campos " +
            "que a tabela não tem.",
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
            "A conta autenticou mas não tem permissão para esta tabela.",
        _ => "Ver docs/projeto/28-PROTHEUS-ACESSO-E-TABELAS.md."
    };
}

/// <summary>Uma página do <c>genericQuery</c>, como o Protheus a devolve.</summary>
public sealed class PaginaDoProtheus
{
    /// <summary>As linhas. Cada uma é um dicionário de campo para valor, em minúsculas.</summary>
    public List<Dictionary<string, JsonElement>>? Items { get; set; }

    /// <summary>Se há mais páginas.</summary>
    public bool HasNext { get; set; }

    /// <summary>Quantas linhas a consulta inteira tem.</summary>
    public int Total { get; set; }

    /// <summary>Quantas ainda faltam depois desta página.</summary>
    public int RemainingRecords { get; set; }
}

/// <summary>
/// A resposta do endpoint de token.
///
/// <para>OS NOMES VÊM EM <c>snake_case</c>, e o resto da API não. O <c>genericQuery</c> devolve
/// <c>items</c>, <c>hasNext</c> e <c>total</c> em camelCase, que o padrão Web do
/// <c>System.Text.Json</c> resolve sozinho; o endpoint de token devolve <c>access_token</c> e
/// <c>expires_in</c>, que ele NÃO resolve. Sem os atributos abaixo a autenticação "funciona" e
/// devolve token nulo — falha silenciosa que custa uma carga inteira para diagnosticar.</para>
/// </summary>
internal sealed class RespostaDeToken
{
    /// <summary>O token, para o cabeçalho <c>Authorization</c>.</summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>Em quantos segundos ele vence. Medido: 3600.</summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
