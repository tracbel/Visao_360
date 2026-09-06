using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O commit — e a tradução da colisão de concorrência em resultado de negócio.
///
/// POR QUE A EXCEÇÃO NÃO PODE SUBIR: <c>DbUpdateConcurrencyException</c> é o EF Core dizendo
/// "o <c>UPDATE</c> afetou zero linhas porque o <c>rowversion</c> mudou". Isso não é defeito de
/// programa — é duas pessoas salvando o mesmo registro, que é um FATO DE NEGÓCIO e precisa
/// chegar ao usuário como uma frase que ele entende. Deixá-la subir produziria um 500, e 500
/// significa "o servidor quebrou", que é mentira aqui.
///
/// [V] É a diferença exata contra o legado, cuja API devolve
/// <c>{"Message":"An error has occurred."}</c> para tudo. E é também a razão de a mensagem
/// abaixo dizer o que FAZER — recarregar e refazer — e não só o que houve.
/// </summary>
public sealed class UnidadeDeTrabalho(CrmDbContext contexto) : IUnidadeDeTrabalho
{
    /// <inheritdoc />
    public async Task<Resultado<int>> SalvarAsync(CancellationToken ct)
    {
        try
        {
            return Resultado<int>.Ok(await contexto.SaveChangesAsync(ct));
        }
        catch (DbUpdateConcurrencyException)
        {
            return Resultado<int>.Concorrencia(
                "Outra pessoa gravou este registro no mesmo instante. Nada foi perdido: " +
                "recarregue o registro e refaça a sua alteração.");
        }
    }
}
