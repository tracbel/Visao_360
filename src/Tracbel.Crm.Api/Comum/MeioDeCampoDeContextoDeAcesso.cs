using Microsoft.Extensions.Options;
using Tracbel.Crm.Api.Seguranca;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Seguranca;
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
/// ---------------------------------------------------------------------------------------------
/// OS DOIS MODOS, e a regra que separa um do outro.
///
/// <para><b>Com o Entra ID ligado, o cabeçalho de usuário deixa de valer.</b> Não é detalhe: se ele
/// continuasse aceito, a tela de login seria decoração — qualquer um contornaria o login mandando
/// <c>X-Tracbel-Usuario</c> na mão. Ligado o Entra, quem não tem sessão recebe 401, ponto. O
/// cabeçalho de FILIAL continua valendo, porque ele não diz quem a pessoa é: diz onde ela está
/// olhando.</para>
///
/// <para><b>Com o Entra ID desligado</b>, vale a ponte provisória de cabeçalho, com os avisos que
/// já existiam. É o modo de desenvolvimento e o de qualquer instalação sem registro de aplicativo.</para>
/// </summary>
public sealed class MeioDeCampoDeContextoDeAcesso(
    RequestDelegate proximo,
    IOptions<OpcoesDeContextoProvisorio> opcoes)
{
    /// <summary>Os caminhos que não exigem identidade.</summary>
    private static readonly string[] CaminhosLivres = ["/saude", "/openapi", "/scalar", "/swagger"];

    /// <summary>
    /// O prefixo que EXIGE identidade.
    ///
    /// <para><b>Identidade é exigência da API, não do portal.</b> Um arquivo estático — o
    /// <c>index.html</c>, o pacote de JavaScript, a folha de estilo — não lê banco, não tem
    /// fronteira de filial e não precisa saber quem está pedindo. Quem precisa é o endpoint que o
    /// navegador chama depois, e esse vive sob <c>/api</c>. É também o que deixa a tela de login
    /// abrir para quem ainda não entrou.</para>
    /// </summary>
    private const string PrefixoDaApi = "/api";

    /// <summary>Executa o meio de campo.</summary>
    /// <param name="http">A requisição em curso.</param>
    /// <param name="portador">Onde o contexto desta requisição é guardado.</param>
    /// <param name="estado">Se o login pelo Entra ID está ligado.</param>
    /// <param name="provisorio">Quem resolve pelo cabeçalho, quando o Entra está desligado.</param>
    /// <param name="entra">Quem resolve pelo token, quando o Entra está ligado.</param>
    public async Task InvokeAsync(
        HttpContext http,
        ContextoAcessoDaRequisicao portador,
        EstadoDaAutenticacao estado,
        ResolvedorDeContextoProvisorio provisorio,
        ResolvedorDeContextoDoEntraId entra)
    {
        var caminho = http.Request.Path.Value ?? string.Empty;

        var ehApi = caminho.StartsWith(PrefixoDaApi, StringComparison.OrdinalIgnoreCase);
        var ehLivre = CaminhosLivres.Any(livre => caminho.StartsWith(livre, StringComparison.OrdinalIgnoreCase));

        if (!ehApi || ehLivre)
        {
            await proximo(http);
            return;
        }

        var config = opcoes.Value;
        var filial = http.Request.Headers[config.CabecalhoDeEmpresa].FirstOrDefault();

        Resultado<ContextoAcesso> resultado;

        if (estado.EntraLigado)
        {
            var identidade = RotasDeAutenticacao.LerIdentidade(http.User);

            if (identidade is null)
            {
                await RespostaDeErro.NaoAutenticado().ExecuteAsync(http);
                return;
            }

            resultado = await entra.ResolverAsync(identidade, filial, http.RequestAborted);

            // "SEM CADASTRO" É 403, E NÃO 422. A pessoa provou quem é; o que falta é o CRM
            // conhecê-la. Já filial inválida é entrada ruim, e segue o 422 de sempre.
            if (!resultado.EhSucesso && resultado.Tipo == TipoDeFalha.NaoEncontrado)
            {
                await RespostaDeErro.SemAcesso(
                    resultado.Erro!,
                    "O login na Microsoft deu certo; o que falta é o CRM liberar a conta.").ExecuteAsync(http);
                return;
            }
        }
        else
        {
            resultado = await provisorio.ResolverAsync(
                http.Request.Headers[config.CabecalhoDeUsuario].FirstOrDefault(),
                filial,
                http.RequestAborted);
        }

        // A ROTA DE ESCOPO NÃO FICA PRESA NA FILIAL RECUSADA (P-20): ela é a que diz quais filiais são
        // permitidas, e a tela precisa dela justamente quando a filial guardada deixou de ser. Só ela é
        // remontada pela filial de casa; qualquer outra rota recebe o 403.
        if (!resultado.EhSucesso && resultado.Tipo == TipoDeFalha.SemPermissao && !string.IsNullOrWhiteSpace(filial)
            && caminho.Equals(Endpoints.EndpointsDeAcesso.CaminhoDoEscopo, StringComparison.OrdinalIgnoreCase))
        {
            http.Items[Endpoints.EndpointsDeAcesso.ChaveDaFilialRecusada] = filial;
            resultado = estado.EntraLigado
                ? await entra.ResolverAsync(RotasDeAutenticacao.LerIdentidade(http.User)!, null, http.RequestAborted)
                : await provisorio.ResolverAsync(http.Request.Headers[config.CabecalhoDeUsuario].FirstOrDefault(), null, http.RequestAborted, naFilialDeCasa: true);
        }

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
