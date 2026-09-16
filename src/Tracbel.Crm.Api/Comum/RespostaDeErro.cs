using Microsoft.AspNetCore.Mvc;
using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Api.Comum;

/// <summary>
/// A TRADUÇÃO DE <see cref="Resultado{T}"/> EM RESPOSTA HTTP — o único lugar da API onde um
/// erro vira um código de status.
///
/// <para><b>Por que existe um lugar só:</b> se cada endpoint decidisse o próprio código, a mesma
/// falha sairia como 400 num lugar e 409 noutro, e o front teria de tratar caso a caso. Aqui a
/// decisão é da NATUREZA da falha, que o caso de uso já declarou em
/// <see cref="Resultado{T}.Tipo"/>. O endpoint não escolhe nada — ele delega e traduz, que é o
/// trabalho dele (documento 22, seção 3.4).</para>
///
/// <para><b>O contrato de erro, e o compromisso que ele carrega:</b> toda recusa devolve
/// <c>application/problem+json</c> com <c>title</c> dizendo o que houve e, quando a falha é de
/// entrada, uma lista <c>erros</c> com <c>campo</c>, <c>mensagem</c> e <c>valorRecebido</c>. A
/// tela acende o campo certo e mostra a frase; ninguém precisa adivinhar.</para>
///
/// <para>[V] É a diferença medida contra a API do legado, que devolve
/// <c>{"Message":"An error has occurred."}</c> tanto para um telefone com DDI quanto para uma
/// falha real de infraestrutura — e por isso ninguém consegue diagnosticar nada.</para>
/// </summary>
public static class RespostaDeErro
{
    /// <summary>O prefixo dos identificadores de tipo de erro, usados no campo <c>type</c>.</summary>
    public const string PrefixoDeTipo = "https://crm.tracbel.com.br/erros/";

    /// <summary>
    /// Devolve o valor em caso de sucesso, ou a recusa traduzida.
    /// </summary>
    /// <typeparam name="T">O que a operação produz.</typeparam>
    /// <param name="resultado">O que o caso de uso devolveu.</param>
    /// <param name="aoDarCerto">Como responder o sucesso. O padrão é 200 com o valor.</param>
    public static IResult Responder<T>(this Resultado<T> resultado, Func<T, IResult>? aoDarCerto = null) =>
        resultado.EhSucesso
            ? (aoDarCerto ?? (valor => Results.Ok(valor)))(resultado.Valor)
            : Recusar(resultado);

    /// <summary>Traduz uma recusa em <c>problem+json</c>.</summary>
    /// <typeparam name="T">O que a operação produziria.</typeparam>
    /// <param name="resultado">O resultado que falhou.</param>
    public static IResult Recusar<T>(Resultado<T> resultado)
    {
        var (status, tipo, categoria) = Classificar(resultado.Tipo);

        var problema = new ProblemDetails
        {
            Type = PrefixoDeTipo + tipo,
            Title = resultado.Erro,
            Status = status,
            Detail = categoria
        };

        // A lista campo a campo só aparece quando existe. Um `erros: []` sempre presente
        // ensinaria o front a procurar detalhe onde nunca há — e a ignorar quando há.
        if (resultado.Erros.Count > 0)
            problema.Extensions["erros"] = resultado.Erros
                .Select(e => new
                {
                    campo = e.Campo,
                    mensagem = e.Mensagem,
                    valorRecebido = e.ValorRecebido
                })
                .ToArray();

        return Results.Problem(problema);
    }

    /// <summary>
    /// <b>401</b> — ninguém entrou, ou a sessão expirou.
    ///
    /// <para>Não passa por <see cref="Resultado{T}"/> porque não é resultado de caso de uso: é a
    /// porta, antes de qualquer caso de uso existir. Mas sai no MESMO formato de toda recusa, para
    /// o front ter um tratamento de erro só — e é esse <c>type</c> que ele usa para voltar à tela de
    /// login em vez de mostrar um aviso no meio da tela.</para>
    /// </summary>
    public static IResult NaoAutenticado() => Results.Problem(new ProblemDetails
    {
        Type = PrefixoDeTipo + "nao-autenticado",
        Title = "É preciso entrar com a conta Microsoft.",
        Status = StatusCodes.Status401Unauthorized,
        Detail = "A sessão não existe ou expirou. Entre de novo pela tela de login."
    });

    /// <summary>
    /// <b>403</b> — a pessoa provou quem é, e mesmo assim não entra.
    ///
    /// <para>É diferente do 401, e a tela precisa saber a diferença: no 401 a ação é "entrar"; aqui
    /// entrar de novo não resolve nada, porque o problema é o cadastro dela no CRM. Mandar de volta
    /// ao login seria um laço — a pessoa entra, é recusada, volta, entra.</para>
    /// </summary>
    /// <param name="titulo">O que houve.</param>
    /// <param name="detalhe">O que fazer a respeito.</param>
    public static IResult SemAcesso(string titulo, string detalhe) => Results.Problem(new ProblemDetails
    {
        Type = PrefixoDeTipo + "sem-acesso",
        Title = titulo,
        Status = StatusCodes.Status403Forbidden,
        Detail = detalhe
    });

    /// <summary>
    /// A tabela de tradução, escrita uma vez.
    ///
    /// <list type="bullet">
    ///   <item><b>422</b> para entrada que não serve — e não 400. O 400 diz "não entendi a
    ///   requisição"; aqui a requisição foi entendida perfeitamente e o CONTEÚDO é que não
    ///   passa. A distinção importa para quem depura.</item>
    ///   <item><b>404</b> tanto para "não existe" quanto para "não é seu". Distinguir contaria
    ///   a quem não pode ver que o registro existe.</item>
    ///   <item><b>409</b> para estado que não admite a operação e para colisão de concorrência —
    ///   com <c>type</c> diferente, porque a ação do usuário é diferente: num caso ele muda o
    ///   pedido, no outro ele recarrega e refaz.</item>
    ///   <item><b>503</b> para a ponte do legado fora do ar. Não é erro de quem chamou, e o
    ///   status precisa dizer isso: <c>Retry-After</c> faz sentido aqui e não faria num 500.</item>
    /// </list>
    /// </summary>
    private static (int Status, string Tipo, string Categoria) Classificar(TipoDeFalha falha) => falha switch
    {
        TipoDeFalha.Validacao => (
            StatusCodes.Status422UnprocessableEntity, "validacao",
            "A requisição foi entendida, mas o conteúdo não passa nas regras. Veja a lista 'erros'."),

        TipoDeFalha.NaoEncontrado => (
            StatusCodes.Status404NotFound, "nao-encontrado",
            "O registro não existe ou não está ao alcance do seu contexto de acesso."),

        TipoDeFalha.Conflito => (
            StatusCodes.Status409Conflict, "conflito",
            "O estado atual do registro não admite esta operação."),

        TipoDeFalha.Concorrencia => (
            StatusCodes.Status409Conflict, "concorrencia",
            "Outra pessoa alterou o registro antes de você. Recarregue e refaça."),

        TipoDeFalha.DependenciaIndisponivel => (
            StatusCodes.Status503ServiceUnavailable, "dependencia-indisponivel",
            "Um sistema externo não respondeu. O restante da API continua funcionando."),

        // 403, com o mesmo `type` do meio de campo de acesso: a pessoa é conhecida e o perfil dela
        // não abre esta visão. A tela usa isso para explicar a opção desligada.
        TipoDeFalha.SemPermissao => (
            StatusCodes.Status403Forbidden, "sem-acesso",
            "O seu perfil não alcança o que foi pedido."),

        // Não há como chegar aqui: Nenhuma só existe em resultado de sucesso, e sucesso não
        // passa por esta função. Mas um 500 mudo seria pior do que um 500 que se explica.
        _ => (
            StatusCodes.Status500InternalServerError, "indefinido",
            "Falha sem natureza declarada. Isto é defeito de programação — registre um chamado.")
    };
}
