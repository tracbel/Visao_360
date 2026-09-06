using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso ao faturamento, sobre o <see cref="CrmDbContext"/>.
///
/// <para>Como nos outros repositórios, <b>não há um <c>Where</c> de empresa neste arquivo</b>: a
/// fronteira de filial entra sozinha, pelo filtro global, porque <c>FaturamentoDoCliente</c> tem
/// <c>EmpresaId</c>. Vale também para os agregados — que é onde o legado mais vazava.</para>
///
/// <para><b>A série ancora na competência mais recente que EXISTE, e não em hoje.</b> Se a carga
/// do ERP parar de novo, uma série ancorada em hoje mostraria três meses zerados no fim do
/// gráfico, como se a empresa tivesse deixado de vender; ancorada no dado, ela mostra os últimos
/// doze meses com movimento e a tela escreve até quando eles vão. A diferença entre "vendemos
/// zero" e "não sabemos" é a razão de este CRM existir.</para>
/// </summary>
public sealed class RepositorioDeFaturamento(CrmDbContext contexto) : IRepositorioFaturamento
{
    /// <inheritdoc />
    public Task<DateOnly?> CompetenciaMaisRecenteAsync(CancellationToken ct) =>
        contexto.FaturamentoDosClientes
            .Where(f => f.ExcluidoEm == null)
            .MaxAsync(f => (DateOnly?)f.Competencia, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MesDeFaturamento>> SerieMensalAsync(int meses, CancellationToken ct)
    {
        var ultima = await CompetenciaMaisRecenteAsync(ct);
        if (ultima is null) return [];

        var primeira = ultima.Value.AddMonths(-(meses - 1));

        var agrupado = await contexto.FaturamentoDosClientes
            .Where(f => f.ExcluidoEm == null && f.Competencia >= primeira && f.Competencia <= ultima)
            .GroupBy(f => f.Competencia)
            .Select(g => new MesDeFaturamento(
                g.Key,
                g.Sum(f => f.ValorLiquido),
                // CLIENTE DISTINTO, e não linha: a mesma empresa fatura em mais de uma filial, e
                // somar as linhas contaria o cliente duas vezes no mesmo mês.
                g.Select(f => f.ClienteId).Distinct().Count(),
                g.Sum(f => f.Notas)))
            .ToListAsync(ct);

        // O MÊS SEM MOVIMENTO ENTRA COM ZERO, e não some do gráfico. Um mês ausente faria a linha
        // pular de agosto para outubro como se setembro não tivesse existido.
        var porCompetencia = agrupado.ToDictionary(m => m.Competencia);
        var serie = new List<MesDeFaturamento>(meses);

        for (var mes = primeira; mes <= ultima; mes = mes.AddMonths(1))
        {
            serie.Add(porCompetencia.TryGetValue(mes, out var achado)
                ? achado
                : new MesDeFaturamento(mes, 0m, 0, 0));
        }

        return serie;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClienteNoRanking>> TopClientesAsync(int quantos, CancellationToken ct)
    {
        var porCliente = await contexto.FaturamentoDosClientes
            .Where(f => f.ExcluidoEm == null)
            .GroupBy(f => f.ClienteId)
            .Select(g => new
            {
                ClienteId = g.Key,
                Valor = g.Sum(f => f.ValorLiquido),
                Ultima = g.Max(f => f.Competencia)
            })
            .OrderByDescending(x => x.Valor)
            .Take(quantos)
            .ToListAsync(ct);

        var ids = porCliente.Select(x => x.ClienteId).ToList();

        var clientes = await contexto.Clientes.AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.ChavePublica, c.NomeRazao, c.Classe })
            .ToDictionaryAsync(c => c.Id, ct);

        return
        [
            .. porCliente
                .Where(x => clientes.ContainsKey(x.ClienteId))
                .Select(x => new ClienteNoRanking(
                    clientes[x.ClienteId].ChavePublica,
                    clientes[x.ClienteId].NomeRazao,
                    clientes[x.ClienteId].Classe?.ToString(),
                    decimal.Round(x.Valor, 2),
                    x.Ultima))
        ];
    }
}
