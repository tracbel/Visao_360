using Microsoft.AspNetCore.Mvc;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Clientes;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>
/// As rotas do cadastro de clientes — o que grava no NOSSO banco.
///
/// Todo endpoint aqui faz três coisas e nada mais: recebe, delega ao caso de uso e traduz o
/// <c>Resultado</c> em código HTTP. Nenhuma decisão de negócio mora neste arquivo, e é isso que
/// <c>SolidTestes.A_api_nao_decide_regra_de_negocio</c> vigia.
/// </summary>
public static class EndpointsDeCliente
{
    /// <summary>Registra as rotas de cliente.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearClientes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/clientes")
            .WithTags("Clientes (banco do CRM)");

        grupo.MapGet("/", async (
                ListarClientes caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                string? termo = null,
                string? situacao = null,
                string? tipoDePessoa = null,
                string? ordenarPor = null,
                bool descendente = false,
                bool incluirInativos = false) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, termo, situacao, tipoDePessoa,
                ordenarPor, descendente, incluirInativos, ct)).Responder())
            .WithName("ListarClientes")
            .WithSummary("Lista clientes do banco do CRM, com paginação, filtro e ordenação.");

        grupo.MapGet("/{chave:guid}", async (Guid chave, ObterCliente caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithName("ObterCliente")
            .WithSummary("Traz a ficha de um cliente pela chave pública.");

        grupo.MapPost("/", async (NovoCliente corpo, CriarCliente caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(corpo, ct))
                .Responder(criado => Results.Created($"/api/v1/clientes/{criado.Chave}", criado)))
            .WithName("CriarCliente")
            .WithSummary("Cadastra um cliente na filial do contexto de acesso.");

        grupo.MapPut("/{chave:guid}", async (
                Guid chave, AlteracaoDeCliente corpo, AlterarCliente caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(chave, corpo, ct)).Responder())
            .WithName("AlterarCliente")
            .WithSummary("Altera um cliente. Devolva a versão lida no GET para a conferência de concorrência.");

        // DELETE COM CORPO, e de propósito: o motivo da inativação é obrigatório e vem de
        // catálogo. Pôr o motivo na query string colocaria dado de negócio no log do servidor
        // web e na barra do navegador; o corpo é o lugar dele.
        //
        // O [FromBody] é OBRIGATÓRIO aqui, e não é enfeite: o ASP.NET Core não INFERE corpo em
        // DELETE — sem o atributo, a aplicação nem constrói a rota e devolve 500 na primeira
        // requisição a qualquer endpoint. Foi assim que este defeito apareceu, exercitando a API
        // de verdade; nenhum teste de unidade o teria pego.
        grupo.MapDelete("/{chave:guid}", async (
                Guid chave, [FromBody] InativacaoDeCliente corpo, InativarCliente caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(chave, corpo, ct)).Responder())
            .WithName("InativarCliente")
            .WithSummary("Inativa um cliente — exclusão lógica, com motivo de catálogo. Nada é apagado.");

        return app;
    }
}
