using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Integracao.GestaoDeNegocios;

/// <summary>
/// UMA PÁGINA DA API GESTÃO DE NEGÓCIOS, com o envelope que TODA rota de cadastro e de painel traz (medido em
/// 27/09/2026): <c>cadastro</c>, <c>tipo</c>, <c>rotulo</c>, <c>gerado_em</c>, <c>total</c>, <c>pagina</c>,
/// <c>paginas</c>, <c>por_pagina</c> e <c>linhas</c>.
///
/// <para><b>Os nomes em <c>snake_case</c> ficam nos atributos</b>, um lugar só: o padrão Web do
/// <c>System.Text.Json</c> não os resolve, e sem os atributos a leitura "funcionaria" com tudo zerado — a falha
/// silenciosa que a ponte do Protheus já ensinou. Os campos de paginação são OBRIGATÓRIOS: se a API mudar o envelope,
/// a leitura falha alto em vez de devolver uma página vazia.</para>
///
/// <para><b><c>idade_segundos</c> é opcional</b>: os painéis são espelho e trazem a idade (~21 h medidas); o cadastro de
/// metas é da própria GN e NÃO traz. O cliente guarda o que vier, sem inventar.</para>
/// </summary>
/// <typeparam name="T">O tipo de cada linha — <see cref="JsonElement"/> quando quem lê quer conferir os campos.</typeparam>
public sealed class PaginaDaGestaoDeNegocios<T>
{
    /// <summary>O nome do cadastro ou do painel (<c>metas</c>).</summary>
    [JsonPropertyName("cadastro")]
    public string? Cadastro { get; set; }

    /// <summary>O tipo da rota (cadastro ou painel).</summary>
    [JsonPropertyName("tipo")]
    public string? Tipo { get; set; }

    /// <summary>O rótulo legível da rota.</summary>
    [JsonPropertyName("rotulo")]
    public string? Rotulo { get; set; }

    /// <summary>Quando a API gerou a resposta — texto ISO, ou o que ela mandar.</summary>
    [JsonPropertyName("gerado_em")]
    public JsonElement? GeradoEm { get; set; }

    /// <summary>A idade do espelho, em segundos. Só os painéis trazem.</summary>
    [JsonPropertyName("idade_segundos")]
    public double? IdadeSegundos { get; set; }

    /// <summary>Quantas linhas a rota tem ao todo.</summary>
    [JsonRequired]
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>Esta página, a partir de 1.</summary>
    [JsonRequired]
    [JsonPropertyName("pagina")]
    public int Pagina { get; set; }

    /// <summary>Quantas páginas há.</summary>
    [JsonRequired]
    [JsonPropertyName("paginas")]
    public int Paginas { get; set; }

    /// <summary>O tamanho de página que a API aplicou.</summary>
    [JsonPropertyName("por_pagina")]
    public int PorPagina { get; set; }

    /// <summary>As linhas desta página.</summary>
    [JsonRequired]
    [JsonPropertyName("linhas")]
    public List<T> Linhas { get; set; } = [];
}

/// <summary>Uma rota inteira da API, lida página a página e conferida.</summary>
/// <param name="Linhas">Todas as linhas, na ordem das páginas.</param>
/// <param name="Total">O total que a API declarou — e que as linhas fecharam.</param>
/// <param name="Paginas">Quantas páginas foram lidas.</param>
/// <param name="GeradaEmUtc">Quando a API gerou a primeira página, em UTC; nulo quando ela não disse ou não deu para ler.</param>
/// <param name="IdadeSegundos">A idade do espelho, quando a rota é painel.</param>
public sealed record LeituraDaGestaoDeNegocios<T>(
    IReadOnlyList<T> Linhas, int Total, int Paginas, DateTime? GeradaEmUtc, int? IdadeSegundos);

/// <summary>
/// O CLIENTE DA API GESTÃO DE NEGÓCIOS — a leitura paginada, genérica, de qualquer rota da API. É o componente que as
/// metas (#138) usam hoje e que a leitura de pedidos e financiamento (#12) reutiliza.
///
/// <para><b>Só GET, com Bearer.</b> A chave é única para a API inteira (issue [001]): ela vai no cabeçalho
/// <c>Authorization</c>, nunca na URL, e TODA mensagem de erro passa pelo <see cref="Sigilo"/> com a chave como
/// segredo conhecido. Nenhum método desta classe escreve na API.</para>
///
/// <para><b>A leitura inteira FECHA ou falha.</b> <see cref="LerTudoAsync{T}"/> confere que o total e o número de
/// páginas não mudaram no meio (o cadastro sendo editado enquanto se lê) e que as linhas somam o total declarado.
/// Uma leitura pela metade não é devolvida: quem grava a partir dela apagaria o que não veio.</para>
///
/// <para><b>O certificado é validado por inteiro</b> (D-M1, 27/09/2026). Quando a validação falha, a mensagem diz o
/// motivo mais provável — o endereço pelo IP, e não pelo nome — e nada é desligado para "fazer funcionar".</para>
///
/// <para><b>Redirecionamento é recusa.</b> Sem a chave aceita, a API manda para a tela de login (<c>/entrar</c>) com
/// um 302. O cliente registrado não segue redirecionamento; se algum seguir, a resposta HTML também é tratada como
/// chave recusada, e não como "página vazia".</para>
/// </summary>
/// <param name="fabrica">De onde sai o <c>HttpClient</c>.</param>
/// <param name="opcoes">O endereço, a chave e o tamanho da página.</param>
/// <param name="nomeDoCliente">O cliente HTTP registrado — o próprio, ou o do botão "Testar".</param>
/// <param name="esperaEntreTentativas">A espera base entre tentativas; o teste passa zero.</param>
public sealed class ClienteDaGestaoDeNegocios(
    IHttpClientFactory fabrica,
    IOptions<OpcoesDaGestaoDeNegocios> opcoes,
    string nomeDoCliente = ClienteDaGestaoDeNegocios.NomeDoCliente,
    TimeSpan? esperaEntreTentativas = null)
{
    /// <summary>O nome do cliente HTTP registrado: sem seguir redirecionamento.</summary>
    public const string NomeDoCliente = "GestaoDeNegocios";

    /// <summary>O endereço que o Ricardo decidiu usar (D-M1) — o que a mensagem de erro de certificado sugere.</summary>
    public const string EnderecoPeloNome = "https://agro-sistemas-w.tracbel.com.br:5001";

    private const int Tentativas = 3;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly TimeSpan _espera = esperaEntreTentativas ?? TimeSpan.FromSeconds(2);

    /// <summary>Uma página de uma rota.</summary>
    /// <param name="rota">O caminho da rota, começando por <c>/api/</c>. Ex.: <c>/api/v1/cadastros/metas</c>.</param>
    /// <param name="pagina">A página, a partir de 1.</param>
    /// <param name="porPagina">Quantas linhas por página (1 a 5000).</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<PaginaDaGestaoDeNegocios<T>>> LerPaginaAsync<T>(string rota, int pagina, int porPagina, CancellationToken ct)
    {
        var config = opcoes.Value;
        if (config.Problema() is { } problema) return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(problema);

        var endereco = string.Create(CultureInfo.InvariantCulture,
            $"{config.Base!.Trim().TrimEnd('/')}{rota}?pagina={pagina}&por_pagina={porPagina}");

        for (var tentativa = 1; tentativa <= Tentativas; tentativa++)
        {
            try
            {
                using var cliente = fabrica.CreateClient(nomeDoCliente);
                cliente.Timeout = TimeSpan.FromSeconds(config.TempoLimiteSegundos);

                using var pedido = new HttpRequestMessage(HttpMethod.Get, endereco);
                pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.Chave!.Trim());
                pedido.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var resposta = await cliente.SendAsync(pedido, ct);
                var codigo = (int)resposta.StatusCode;

                if (codigo is >= 300 and < 400 || resposta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(
                        $"A API Gestão de Negócios recusou a chave (HTTP {codigo}{(codigo is >= 300 and < 400 ? ", redirecionou para a tela de login" : string.Empty)}). " +
                        "Grave a chave de novo em Configurações › Integrações. Nada foi lido.");

                if (!resposta.IsSuccessStatusCode)
                {
                    if (tentativa < Tentativas && EhIntermitente(resposta.StatusCode))
                    {
                        await Task.Delay(_espera * tentativa, ct);
                        continue;
                    }

                    return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(
                        $"A API Gestão de Negócios não entregou {rota} (HTTP {codigo}).");
                }

                var tipoDoConteudo = resposta.Content.Headers.ContentType?.MediaType;
                if (tipoDoConteudo is not null && !tipoDoConteudo.Contains("json", StringComparison.OrdinalIgnoreCase))
                    return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(
                        $"A API Gestão de Negócios respondeu {rota} com {tipoDoConteudo}, e não JSON — é a tela de login: a chave não " +
                        "foi aceita. Grave a chave de novo em Configurações › Integrações. Nada foi lido.");

                var corpo = await resposta.Content.ReadAsStringAsync(ct);
                try
                {
                    var lida = JsonSerializer.Deserialize<PaginaDaGestaoDeNegocios<T>>(corpo, Json);
                    return lida is null
                        ? Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel($"A API Gestão de Negócios devolveu {rota} vazia.")
                        : Resultado<PaginaDaGestaoDeNegocios<T>>.Ok(lida);
                }
                catch (JsonException erro)
                {
                    // A MENSAGEM DO LEITOR DE JSON diz o campo e a posição, nunca o valor: pode sair.
                    return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(
                        $"A resposta de {rota} não tem o envelope esperado da API Gestão de Negócios ({erro.Message}). " +
                        "A API pode ter mudado de formato — nada foi gravado.");
                }
            }
            catch (HttpRequestException erro) when (erro.GetBaseException() is AuthenticationException)
            {
                // CERTIFICADO: não se repete — outra tentativa daria o mesmo erro, e desligar a validação não é opção.
                return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(
                    "O certificado da API Gestão de Negócios não passou na validação. O motivo mais provável é o endereço pelo " +
                    $"IP: use o NOME ({EnderecoPeloNome}), que é o que o certificado declara. A validação não é desligada — " +
                    $"a chave vale para a API inteira. Detalhe: {Ocultar(erro.GetBaseException().Message)}");
            }
            catch (HttpRequestException erro)
            {
                if (tentativa == Tentativas)
                    return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(
                        $"Não foi possível falar com a API Gestão de Negócios: {Ocultar(erro.Message)}");

                await Task.Delay(_espera * tentativa, ct);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                if (tentativa == Tentativas)
                    return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel(
                        $"A API Gestão de Negócios não respondeu {rota} em {config.TempoLimiteSegundos}s.");

                await Task.Delay(_espera * tentativa, ct);
            }
        }

        return Resultado<PaginaDaGestaoDeNegocios<T>>.Indisponivel($"A leitura de {rota} esgotou as tentativas.");
    }

    /// <summary>
    /// A rota inteira, página a página — e só se ela FECHAR: o total e o número de páginas não mudam no meio, e as linhas
    /// somam o total declarado.
    /// </summary>
    /// <param name="rota">O caminho da rota.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<LeituraDaGestaoDeNegocios<T>>> LerTudoAsync<T>(string rota, CancellationToken ct)
    {
        var porPagina = opcoes.Value.TamanhoDaPagina;

        var primeira = await LerPaginaAsync<T>(rota, 1, porPagina, ct);
        if (!primeira.EhSucesso) return Resultado<LeituraDaGestaoDeNegocios<T>>.Indisponivel(primeira.Erro!);

        var total = primeira.Valor.Total;
        var paginas = primeira.Valor.Paginas;
        var linhas = new List<T>(Math.Max(total, 0));
        linhas.AddRange(primeira.Valor.Linhas);

        for (var pagina = 2; pagina <= paginas; pagina++)
        {
            var lida = await LerPaginaAsync<T>(rota, pagina, porPagina, ct);
            if (!lida.EhSucesso) return Resultado<LeituraDaGestaoDeNegocios<T>>.Indisponivel(lida.Erro!);

            // O CADASTRO MUDOU ENQUANTO ERA LIDO: as páginas já lidas e as seguintes não são do mesmo retrato. Rodar de
            // novo é barato; gravar um retrato misturado não tem volta.
            if (lida.Valor.Total != total || lida.Valor.Paginas != paginas)
                return Resultado<LeituraDaGestaoDeNegocios<T>>.Indisponivel(
                    string.Create(CultureInfo.InvariantCulture,
                        $"{rota} mudou durante a leitura: a página 1 dizia {total} linhas em {paginas} páginas, e a página {pagina} " +
                        $"diz {lida.Valor.Total} em {lida.Valor.Paginas}. O cadastro estava sendo editado — nada foi gravado; rode de novo."));

            linhas.AddRange(lida.Valor.Linhas);
        }

        if (linhas.Count != total)
            return Resultado<LeituraDaGestaoDeNegocios<T>>.Indisponivel(
                string.Create(CultureInfo.InvariantCulture,
                    $"A leitura de {rota} não fecha: a API declarou {total} linhas e entregou {linhas.Count} em {paginas} página(s). Nada foi gravado."));

        return Resultado<LeituraDaGestaoDeNegocios<T>>.Ok(new LeituraDaGestaoDeNegocios<T>(
            linhas, total, paginas, InstanteUtc(primeira.Valor.GeradoEm),
            primeira.Valor.IdadeSegundos is { } idade ? (int)Math.Round(idade) : null));
    }

    /// <summary>
    /// O <c>gerado_em</c> em UTC. Com fuso no texto, vale o fuso; sem fuso, é o horário de São Paulo — o do servidor
    /// da API, o mesmo do CRM (UTC−3, sem horário de verão desde 2019). O que não for data fica nulo: é carimbo
    /// informativo, e não vale uma leitura inteira.
    /// </summary>
    /// <param name="valor">O campo como veio.</param>
    public static DateTime? InstanteUtc(JsonElement? valor)
    {
        if (valor is not { ValueKind: JsonValueKind.String } texto) return null;
        var bruto = texto.GetString();
        if (string.IsNullOrWhiteSpace(bruto)) return null;

        var temFuso = bruto.EndsWith('Z') || bruto.LastIndexOfAny(['+', '-']) > 10;
        if (temFuso && DateTimeOffset.TryParse(bruto, CultureInfo.InvariantCulture, DateTimeStyles.None, out var comFuso))
            return comFuso.UtcDateTime;

        return DateTime.TryParse(bruto, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local)
            ? DateTime.SpecifyKind(local.AddHours(3), DateTimeKind.Utc)
            : null;
    }

    private string Ocultar(string texto) => Sigilo.Mascarar(texto, [opcoes.Value.Chave]);

    private static bool EhIntermitente(HttpStatusCode codigo) =>
        codigo is HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;
}
