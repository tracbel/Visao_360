using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Infraestrutura.Identidade;

namespace Tracbel.Crm.Api.Seguranca;

/// <summary>
/// Se o login pelo Entra ID está ligado nesta instância.
///
/// <para>Existe como tipo, e não como leitura de configuração espalhada, porque é a decisão que
/// separa "o cabeçalho vale" de "o cabeçalho não vale nada". Lida em um lugar só, ela não tem como
/// divergir entre o meio de campo e as rotas.</para>
/// </summary>
/// <param name="EntraLigado">Verdadeiro quando tenant, cliente e segredo estão configurados.</param>
public sealed record EstadoDaAutenticacao(bool EntraLigado);

/// <summary>O que a tela precisa saber sobre a sessão, antes de decidir o que desenhar.</summary>
/// <param name="Modo"><c>entra</c> quando há login de verdade; <c>provisorio</c> quando não há.</param>
/// <param name="Nome">Como a pessoa aparece na tela.</param>
/// <param name="NomePrincipal">O nome principal da conta Microsoft.</param>
/// <param name="Email">O e-mail da conta.</param>
public sealed record SessaoAtual(string Modo, string? Nome, string? NomePrincipal, string? Email);

/// <summary>
/// As rotas de sessão: entrar, saber quem está dentro, e sair.
///
/// <para><b>Moram fora de <c>/api</c></b>, e não é arbitrário: elas não leem dado de negócio e não
/// têm filial. O meio de campo de contexto só olha <c>/api</c>, então nenhuma delas exige o contexto
/// que elas mesmas existem para criar.</para>
/// </summary>
public static class RotasDeAutenticacao
{
    /// <summary>
    /// Lê do token o que a infraestrutura precisa — e só isso.
    /// </summary>
    /// <param name="usuario">O principal da requisição.</param>
    /// <returns>Nulo quando não há login, ou quando o token não traz o mínimo.</returns>
    public static IdentidadeDoEntra? LerIdentidade(ClaimsPrincipal usuario)
    {
        if (usuario.Identity?.IsAuthenticated != true) return null;

        // Os nomes são os crus do token (`oid`, `preferred_username`): o EntraId desliga o
        // mapeamento de reivindicações, que traduziria `oid` numa URL de esquema da Microsoft.
        var oid = usuario.FindFirst("oid")?.Value;
        var upn = usuario.FindFirst("preferred_username")?.Value ?? usuario.FindFirst("upn")?.Value;

        if (!Guid.TryParse(oid, out var identificador) || string.IsNullOrWhiteSpace(upn)) return null;

        return new IdentidadeDoEntra(
            identificador,
            upn.Trim(),
            usuario.FindFirst("email")?.Value,
            usuario.FindFirst("name")?.Value);
    }

    /// <summary>
    /// O destino depois do login — sempre dentro da própria aplicação.
    ///
    /// <para><b>É a proteção contra redirecionamento aberto.</b> Sem ela,
    /// <c>/auth/entrar?voltarPara=https://site-falso</c> faria a tela de login VERDADEIRA da
    /// Microsoft devolver a pessoa, já autenticada, para um endereço de terceiro — o golpe clássico
    /// de phishing que usa a credibilidade do login real. Só passa caminho relativo: começa com uma
    /// barra, e não com duas (<c>//site</c> é endereço absoluto para o navegador).</para>
    /// </summary>
    /// <param name="voltarPara">O que a tela pediu.</param>
    public static string DestinoLocal(string? voltarPara)
    {
        if (string.IsNullOrWhiteSpace(voltarPara)) return "/";

        var destino = voltarPara.Trim();

        if (!destino.StartsWith('/') || destino.StartsWith("//") || destino.StartsWith("/\\")) return "/";
        if (destino.Contains('\r') || destino.Contains('\n')) return "/";

        return destino;
    }

    /// <summary>Publica as rotas de sessão.</summary>
    /// <param name="app">A aplicação.</param>
    /// <param name="entraLigado">Se o login pelo Entra ID está ligado.</param>
    public static void MapearAutenticacao(this WebApplication app, bool entraLigado)
    {
        var sessao = app.MapGroup("/auth")
            .SemPermissaoExigida("entrar, sair e saber quem está entrando: é o que acontece ANTES de haver contexto de acesso");

        // QUEM ESTÁ DENTRO — e se o CRM conhece essa pessoa. A pergunta ao cadastro é feita AQUI,
        // e não na primeira chamada de dado: descobrir "sem cadastro" já com a Visão 360 aberta
        // produziria vinte cartões de erro ao mesmo tempo, em vez de uma frase na tela de login.
        sessao.MapGet("/eu", async (HttpContext http, ResolvedorDeContextoDoEntraId resolvedor, CancellationToken ct) =>
        {
            if (!entraLigado) return Results.Ok(new SessaoAtual("provisorio", null, null, null));

            var identidade = LerIdentidade(http.User);
            if (identidade is null) return RespostaDeErro.NaoAutenticado();

            var contexto = await resolvedor.ResolverAsync(identidade, null, ct);

            return contexto.EhSucesso
                ? Results.Ok(new SessaoAtual(
                    "entra",
                    contexto.Valor.NomeExibicao,
                    identidade.NomePrincipal,
                    identidade.Email ?? identidade.NomePrincipal))
                : RespostaDeErro.SemAcesso(
                    contexto.Erro!,
                    "O login na Microsoft deu certo; o que falta é o cadastro no CRM. Entrar de novo não resolve.");
        });

        if (!entraLigado) return;

        sessao.MapGet("/entrar", (string? voltarPara) =>
            Results.Challenge(
                new AuthenticationProperties { RedirectUri = DestinoLocal(voltarPara) },
                [OpenIdConnectDefaults.AuthenticationScheme]));

        // SAIR DERRUBA AS DUAS SESSÕES. Só o cookie local não bastaria: a sessão na Microsoft
        // continuaria viva, e o próximo "Entrar" numa máquina compartilhada de filial entraria
        // direto na conta de quem saiu — sem pedir senha.
        sessao.MapGet("/sair", () =>
            Results.SignOut(
                new AuthenticationProperties { RedirectUri = "/#/login?saiu=1" },
                [EntraId.EsquemaDeCookie, OpenIdConnectDefaults.AuthenticationScheme]));
    }
}
