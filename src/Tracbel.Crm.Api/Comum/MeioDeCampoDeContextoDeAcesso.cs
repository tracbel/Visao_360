using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Infraestrutura.Identidade;

namespace Tracbel.Crm.Api.Comum;

/// <summary>
/// DESCOBRE QUEM ESTÁ AGINDO e preenche o contexto de acesso da requisição — antes de qualquer
/// coisa tocar o banco.
///
/// <para><b>A ordem é a razão de ser deste meio de campo.</b> O <c>CrmDbContext</c> lê o contexto
/// de acesso no CONSTRUTOR, para pré-computar o filtro global. Se o contexto não estiver
/// definido quando o banco for resolvido, ou o filtro nasce vazio (e a consulta devolve nada,
/// misteriosamente) ou nasce aberto (e vaza). Nenhum dos dois é aceitável, então isto roda
/// antes — e o <c>ContextoAcessoDaRequisicao</c> lança se alguém o ler antes da hora, em vez de
/// devolver um contexto vazio em silêncio.</para>
///
/// <para><b>O que fica de fora, de propósito:</b> as rotas de saúde e a documentação OpenAPI.
/// Elas não leem dado de negócio, e exigir identidade nelas transformaria a prova de vida da
/// aplicação num teste de configuração de cabeçalho.</para>
///
/// <para><b>DÍVIDA NOMEADA:</b> isto NÃO autentica ninguém — quem informa a identidade é um
/// cabeçalho, e cabeçalho qualquer um escreve. É a ponte descrita em
/// <see cref="ResolvedorDeContextoProvisorio"/>, e ela vive até a fase 0 do documento 13 entrar.
/// Quando o Entra ID chegar, este arquivo passa a ler as reivindicações do token validado e o
/// resto da API não muda uma linha.</para>
/// </summary>
public sealed class MeioDeCampoDeContextoDeAcesso(
    RequestDelegate proximo,
    IOptions<OpcoesDeContextoProvisorio> opcoes)
{
    /// <summary>Os caminhos que não exigem identidade.</summary>
    private static readonly string[] CaminhosLivres = ["/saude", "/openapi", "/scalar", "/swagger"];

    /// <summary>Executa o meio de campo.</summary>
    /// <param name="http">A requisição em curso.</param>
    /// <param name="portador">Onde o contexto desta requisição é guardado.</param>
    /// <param name="resolvedor">Quem descobre o contexto a partir dos cabeçalhos.</param>
    public async Task InvokeAsync(
        HttpContext http,
        ContextoAcessoDaRequisicao portador,
        ResolvedorDeContextoProvisorio resolvedor)
    {
        var caminho = http.Request.Path.Value ?? string.Empty;

        if (CaminhosLivres.Any(livre => caminho.StartsWith(livre, StringComparison.OrdinalIgnoreCase)))
        {
            await proximo(http);
            return;
        }

        var config = opcoes.Value;

        var resultado = await resolvedor.ResolverAsync(
            http.Request.Headers[config.CabecalhoDeUsuario].FirstOrDefault(),
            http.Request.Headers[config.CabecalhoDeEmpresa].FirstOrDefault(),
            http.RequestAborted);

        if (!resultado.EhSucesso)
        {
            // A recusa sai no MESMO formato de erro de todo o resto da API. Um formato próprio
            // aqui obrigaria o front a ter dois tratamentos de erro.
            await RespostaDeErro.Recusar(resultado).ExecuteAsync(http);
            return;
        }

        portador.Definir(resultado.Valor);

        // Quem está agindo vai para o log de TODA requisição. É o que permite responder "quem
        // alterou este cliente?" sem depender de o usuário lembrar. [V] no legado, acesso direto
        // ao banco não deixa rastro nenhum.
        http.Response.Headers["X-Tracbel-Contexto"] =
            $"{resultado.Valor.NomeExibicao}; filial {resultado.Valor.EmpresaId}";

        await proximo(http);
    }
}
