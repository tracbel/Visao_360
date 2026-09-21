using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Legado;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// AS ROTAS DA PONTE DE LEITURA DO SISTEMA LEGADO — separadas e marcadas, de propósito.
///
/// <para><b>Por que um prefixo próprio (<c>/api/v1/legado/...</c>) e não misturar com o
/// cadastro:</b> porque a origem do dado muda o que ele significa. O que sai daqui é uma
/// FOTOGRAFIA de outro sistema, que ninguém aqui mantém, lida sem gravar nada — e o front
/// precisa marcar isso na tela. Um cliente do nosso banco e um cliente do legado no mesmo
/// endpoint seriam indistinguíveis na hora de decidir se dá para confiar.</para>
///
/// <para><b>Toda resposta traz procedência.</b> Sistema, objeto lido, quando foi lido, qual é a
/// alteração mais recente que aquele objeto tem, e um aviso escrito quando o dado está velho. É
/// o que impede a repetição dos 17 meses de dado parado passando por atual.</para>
///
/// <para><b>A ponte pode estar fora, e a API continua de pé.</b> Sem VPN, estes endpoints
/// devolvem 503 com a frase que diz o que fazer; nenhum outro endpoint é afetado.</para>
/// </summary>
public static class EndpointsDoLegado
{
    /// <summary>Registra as rotas da ponte.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearLegado(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/legado")
            .WithTags("Ponte de leitura do Vórtice (somente leitura)");

        grupo.MapGet("/clientes", async (
                BuscarClientesNoLegado caso,
                CancellationToken ct,
                string? termo = null,
                int? limite = null) =>
            (await caso.ExecutarAsync(termo, limite, ct)).Responder())
            .WithName("BuscarClientesNoLegado")
            .ExigePermissao(Permissoes.LegadoLer)
            .WithSummary("Busca clientes no Vórtice por nome, nome fantasia ou documento. Somente leitura.");

        grupo.MapGet("/clientes/{identificador:long}/parque", async (
                long identificador, ListarParqueNoLegado caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(identificador, ct)).Responder())
            .WithName("ListarParqueNoLegado")
            .ExigePermissao(Permissoes.LegadoLer)
            .WithSummary("O parque de máquinas do cliente no Vórtice — a lista que o CEN mantém e que é atualizada hoje.");

        grupo.MapGet("/saude", async (VerificarPonteDoLegado caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(ct)).Responder())
            .WithName("VerificarPonteDoLegado")
            .ExigePermissao(Permissoes.LegadoLer)
            .WithSummary("Diz se a ponte responde, e quais objetos do legado estão vivos e quais estão parados.");

        return app;
    }
}
