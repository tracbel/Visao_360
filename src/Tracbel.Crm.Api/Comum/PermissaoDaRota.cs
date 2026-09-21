using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Comum;

/// <summary>A permissão que a rota exige — metadado lido por <see cref="MeioDeCampoDePermissao"/>.</summary>
/// <param name="Codigo">O código, do catálogo em código (<see cref="Permissoes"/>).</param>
public sealed record PermissaoExigida(string Codigo);

/// <summary>
/// A rota que NÃO exige permissão, com o motivo escrito. Declarar é obrigatório: o teste de arquitetura
/// recusa rota que não diga nem uma coisa nem outra (fase 3 do documento 41).
/// </summary>
/// <param name="Motivo">Por que a rota é aberta.</param>
public sealed record RotaSemPermissao(string Motivo);

/// <summary>
/// A DECLARAÇÃO DE PERMISSÃO NAS ROTAS (fase 3 do documento 41, achado C-1: "nenhuma conferência de
/// permissão").
///
/// <para><b>Por que metadado e não <c>RequireAuthorization</c> com política.</b> O ASP.NET decide
/// autorização pelo <c>ClaimsPrincipal</c>; a permissão do CRM mora no <see cref="ContextoAcesso"/>, que
/// é montado a partir do banco depois da autenticação — e é o mesmo com o Entra ligado ou com a ponte
/// provisória. Uma política teria de reconstruir o contexto ou duplicá-lo em reivindicações. O
/// metadado diz o que a rota exige; o meio de campo confere contra o contexto que já existe.</para>
/// </summary>
public static class DeclaracaoDePermissao
{
    /// <summary>Declara a permissão que a rota exige.</summary>
    /// <param name="rota">A rota.</param>
    /// <param name="codigo">A permissão, de <see cref="Permissoes"/>.</param>
    /// <exception cref="InvalidOperationException">Quando o código não existe no catálogo — na subida, e não na primeira chamada.</exception>
    public static TRota ExigePermissao<TRota>(this TRota rota, string codigo) where TRota : IEndpointConventionBuilder
    {
        if (!Permissoes.Existe(codigo))
            throw new InvalidOperationException(
                $"A rota declara a permissão '{codigo}', que não existe em Permissoes.Catalogo.");

        return rota.WithMetadata(new PermissaoExigida(codigo));
    }

    /// <summary>Declara que a rota não exige permissão, e por quê.</summary>
    /// <param name="rota">A rota.</param>
    /// <param name="motivo">O motivo, escrito.</param>
    public static TRota SemPermissaoExigida<TRota>(this TRota rota, string motivo) where TRota : IEndpointConventionBuilder =>
        rota.WithMetadata(new RotaSemPermissao(motivo));
}

/// <summary>
/// CONFERE A PERMISSÃO QUE A ROTA DECLARA contra o contexto de acesso da requisição — 403 quando falta.
///
/// <para>Roda depois do <see cref="MeioDeCampoDeContextoDeAcesso"/>, que monta o contexto. A rota sem
/// declaração nenhuma passa daqui — e é o teste de arquitetura, não este meio de campo, que impede
/// que ela exista: recusar em tempo de execução uma rota esquecida seria descobrir o esquecimento em
/// produção.</para>
///
/// <para><b>O 403 diz o que falta</b>, no mesmo formato de erro de toda a API, com o tipo
/// <c>sem-acesso</c> — é o que evita o chamado "não consigo abrir a tela".</para>
/// </summary>
public sealed class MeioDeCampoDePermissao(RequestDelegate proximo)
{
    /// <summary>Executa a conferência.</summary>
    /// <param name="http">A requisição.</param>
    /// <param name="provedor">O contexto de acesso da requisição.</param>
    public async Task InvokeAsync(HttpContext http, IProvedorContextoAcesso provedor)
    {
        var exigidas = http.GetEndpoint()?.Metadata.GetOrderedMetadata<PermissaoExigida>() ?? [];

        if (exigidas.Count > 0)
        {
            var contexto = provedor.Atual;
            var faltam = exigidas.Where(p => !contexto.Tem(p.Codigo)).Select(p => p.Codigo).Distinct().ToList();

            if (faltam.Count > 0)
            {
                var nomes = string.Join(", ", faltam.Select(c => $"'{c}' ({Permissoes.Catalogo[c]})"));
                await RespostaDeErro.SemAcesso(
                    "O seu perfil não dá acesso a isto.",
                    $"Falta a permissão {nomes}. Peça a quem administra os perfis a concessão de um perfil que a tenha.")
                    .ExecuteAsync(http);
                return;
            }
        }

        await proximo(http);
    }
}
