using Microsoft.AspNetCore.Mvc;
using Tracbel.Crm.Api.Comum;
using Tracbel.Crm.Aplicacao.Equipamentos;

namespace Tracbel.Crm.Api.Endpoints;

/// <summary>As rotas do cadastro de máquinas — o que grava no NOSSO banco.</summary>
public static class EndpointsDeEquipamento
{
    /// <summary>Registra as rotas de equipamento.</summary>
    /// <param name="app">O construtor de rotas.</param>
    public static IEndpointRouteBuilder MapearEquipamentos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/equipamentos")
            .WithTags("Equipamentos (banco do CRM)");

        grupo.MapGet("/", async (
                ListarEquipamentos caso,
                CancellationToken ct,
                int? pagina = null,
                int? tamanho = null,
                string? termo = null,
                string? situacao = null,
                string? origem = null,
                Guid? clienteChave = null,
                string? ordenarPor = null,
                bool descendente = false,
                bool incluirInativos = false) =>
            (await caso.ExecutarAsync(
                pagina, tamanho, termo, situacao, origem, clienteChave,
                ordenarPor, descendente, incluirInativos, ct)).Responder())
            .WithName("ListarEquipamentos")
            .WithSummary("Lista máquinas do banco do CRM. Filtre por clienteChave para o parque de um cliente.");

        grupo.MapGet("/{chave:guid}", async (Guid chave, ObterEquipamento caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(chave, ct)).Responder())
            .WithName("ObterEquipamento")
            .WithSummary("Traz a ficha de uma máquina pela chave pública.");

        grupo.MapPost("/", async (NovoEquipamento corpo, CriarEquipamento caso, CancellationToken ct) =>
                (await caso.ExecutarAsync(corpo, ct))
                .Responder(criado => Results.Created($"/api/v1/equipamentos/{criado.Chave}", criado)))
            .WithName("CriarEquipamento")
            .WithSummary("Cadastra uma máquina — inclusive a do concorrente, que é o que a Cobertura usa.");

        grupo.MapPut("/{chave:guid}", async (
                Guid chave, AlteracaoDeEquipamento corpo, AlterarEquipamento caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(chave, corpo, ct)).Responder())
            .WithName("AlterarEquipamento")
            .WithSummary("Altera uma máquina. Chassi e origem não mudam por edição — veja Equipamento.Alterar.");

        // [FromBody] obrigatório: o ASP.NET Core não infere corpo em DELETE. Ver a nota em
        // EndpointsDeCliente, onde o mesmo defeito foi encontrado exercitando a API.
        grupo.MapDelete("/{chave:guid}", async (
                Guid chave, [FromBody] BaixaDeEquipamento corpo, InativarEquipamento caso, CancellationToken ct) =>
            (await caso.ExecutarAsync(chave, corpo, ct)).Responder())
            .WithName("InativarEquipamento")
            .WithSummary("Baixa uma máquina — exclusão lógica. A linha e o histórico de horímetro ficam.");

        return app;
    }
}
