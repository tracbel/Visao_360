using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Tracbel.Crm.Api.Seguranca;

/// <summary>
/// O QUE FAZ ALGUÉM SER ALGUÉM NESTA API — login pelo Microsoft Entra ID.
///
/// ---------------------------------------------------------------------------------------------
/// O QUE ISTO SUBSTITUI. Até aqui a identidade vinha de dois cabeçalhos HTTP
/// (<c>X-Tracbel-Usuario</c>, <c>X-Tracbel-Empresa</c>) que qualquer um pode escrever. Isso não
/// autenticava ninguém: quem alcançasse o endereço via faturamento, carteira e cliente das
/// dezesseis filiais — R$ 2,5 bilhões, 38 mil cadastros. Era aceitável enquanto rodava em
/// <c>localhost</c>; deixou de ser no dia em que virou endereço de rede.
///
/// <para><b>O padrão é o mesmo do projeto user-onboarding</b>, que já roda neste servidor:
/// Authorization Code Flow, escopo <c>openid profile email</c> e nada mais. <b>A API não consulta
/// o Microsoft Graph</b> — ela valida o token e lê as informações de dentro dele. Isso mantém o
/// registro do aplicativo sem permissão nenhuma de diretório, que é o que torna o pedido à
/// segurança da informação trivial de aprovar.</para>
///
/// <para><b>Autorização é por grupo, e a ausência de informação NÃO vira permissão.</b> Quando
/// <c>Entra:GrupoPermitido</c> está configurado, o token precisa trazer a lista de grupos. Se não
/// trouxer, ninguém entra — e a mensagem diz que isso é configuração do aplicativo, não problema
/// da conta de quem tentou. Confundir "você não está no grupo" com "não consegui verificar" custa
/// horas de suporte, e é o tipo de erro que só aparece no dia da virada.</para>
///
/// <para><b>Enquanto não estiver configurado, nada muda.</b> A API continua no modo de cabeçalho,
/// com o aviso alto que já existe. Isso é deliberado: o registro do aplicativo depende do DNS
/// definitivo, e uma implantação não pode ficar refém de um cadastro que ainda não foi feito.</para>
/// </summary>
public static class EntraId
{
    /// <summary>O esquema do cookie de sessão.</summary>
    public const string EsquemaDeCookie = "TracbelCrm.Sessao";

    /// <summary>
    /// Liga a autenticação, se houver configuração para isso.
    /// </summary>
    /// <param name="servicos">A coleção de serviços da aplicação.</param>
    /// <param name="configuracao">De onde saem as chaves <c>Entra:*</c>.</param>
    /// <returns>Verdadeiro quando a autenticação foi ligada.</returns>
    public static bool AdicionarEntraId(this IServiceCollection servicos, IConfiguration configuracao)
    {
        var tenant = configuracao["Entra:TenantId"];
        var cliente = configuracao["Entra:ClientId"];
        var segredo = configuracao["Entra:ClientSecret"];

        // OU ESTÁ TUDO CONFIGURADO, OU NÃO ESTÁ. Ligar a autenticação sem o segredo produziria um
        // erro no meio do login, depois do usuário já ter digitado a senha na tela da Microsoft.
        if (string.IsNullOrWhiteSpace(tenant) ||
            string.IsNullOrWhiteSpace(cliente) ||
            string.IsNullOrWhiteSpace(segredo))
        {
            return false;
        }

        var grupoPermitido = configuracao["Entra:GrupoPermitido"];

        servicos.AddAuthentication(options =>
            {
                options.DefaultScheme = EsquemaDeCookie;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(EsquemaDeCookie, options =>
            {
                options.Cookie.Name = EsquemaDeCookie;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;

                // OITO HORAS, DESLIZANTE. Um turno de trabalho: quem passa o dia na tela não é
                // deslogado no meio de um atendimento, e a sessão esquecida numa máquina
                // compartilhada não vale para sempre.
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/acesso-negado";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
                options.ClientId = cliente;
                options.ClientSecret = segredo;

                // CÓDIGO DE AUTORIZAÇÃO, e não token implícito: a troca acontece no servidor, e o
                // segredo nunca chega ao navegador.
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = false;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.CallbackPath = "/auth/callback";
                options.SignedOutCallbackPath = "/auth/saida";
                options.SignInScheme = EsquemaDeCookie;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://login.microsoftonline.com/{tenant}/v2.0",
                    ValidateAudience = true,
                    ValidAudience = cliente,
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username"
                };

                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = contexto =>
                    {
                        if (string.IsNullOrWhiteSpace(grupoPermitido)) return Task.CompletedTask;

                        var grupos = contexto.Principal?.FindAll("groups").Select(g => g.Value).ToList() ?? [];

                        // A AUSÊNCIA DA DECLARAÇÃO NÃO É PERMISSÃO — mas também não é a mesma coisa
                        // que "você não está no grupo". São dois problemas com a mesma consequência,
                        // e a mensagem precisa dizer qual dos dois é.
                        if (grupos.Count == 0)
                        {
                            contexto.Fail(
                                "O token não trouxe a lista de grupos, então não dá para confirmar o " +
                                "acesso. Isto é configuração do aplicativo, e não problema da conta: no " +
                                "Entra, Registros de aplicativo → Configuração do token → Adicionar " +
                                "declaração de grupos. Enquanto isso, ninguém entra.");
                            return Task.CompletedTask;
                        }

                        if (!grupos.Contains(grupoPermitido, StringComparer.OrdinalIgnoreCase))
                        {
                            contexto.Fail("Sua conta não está no grupo autorizado para este painel.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        servicos.AddAuthorization();
        return true;
    }

    /// <summary>
    /// O nome principal de quem está logado, ou nulo quando a autenticação não está ligada.
    /// </summary>
    /// <param name="usuario">O principal da requisição.</param>
    public static string? NomePrincipal(this ClaimsPrincipal? usuario) =>
        usuario?.Identity?.IsAuthenticated == true
            ? usuario.FindFirst("preferred_username")?.Value ?? usuario.Identity.Name
            : null;
}
