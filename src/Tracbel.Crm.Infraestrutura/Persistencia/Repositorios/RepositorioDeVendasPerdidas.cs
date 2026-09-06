using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Metadado;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O acesso às vendas perdidas, sobre o <see cref="CrmDbContext"/>.
///
/// <para>Como nos outros repositórios, <b>não há um <c>Where</c> de empresa neste arquivo</b>: a
/// fronteira de filial entra sozinha, pelo filtro global, porque <c>VendaPerdida</c> tem
/// <c>EmpresaId</c>. Vale também para os agregados abaixo — que é onde o legado mais vazava.</para>
///
/// <para><b>Os dois agrupamentos saem de um <c>GROUP BY</c> no banco</b>, e trazem o denominador
/// junto: a diferença média de preço só se calcula sobre as linhas que declararam os DOIS
/// preços, e <c>ComOsDoisPrecos</c> viaja com a média para a tela poder dizer sobre quantas ela
/// foi feita. Média sem denominador é exatamente o defeito que este CRM existe para não
/// repetir.</para>
/// </summary>
public sealed class RepositorioDeVendasPerdidas(CrmDbContext contexto) : IRepositorioVendasPerdidas
{
    /// <inheritdoc />
    public Task<int> ContarAsync(CancellationToken ct) =>
        contexto.VendasPerdidas.Where(v => v.ExcluidoEm == null).CountAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<FatiaDeVendaPerdida>> ResumirPorMotivoAsync(CancellationToken ct)
    {
        var agrupado = await contexto.VendasPerdidas
            .Where(v => v.ExcluidoEm == null)
            .GroupBy(v => v.MotivoDePerdaId)
            .Select(g => new Bruto(
                g.Key,
                g.Count(),
                g.Sum(v => v.Quantidade),
                g.Count(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null),
                g.Average(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null
                    ? v.PrecoOfertado - v.PrecoDoConcorrente
                    : null)))
            .ToListAsync(ct);

        var motivos = await contexto.MotivosDePerda.AsNoTracking()
            .Select(m => new { m.Id, m.Codigo, m.Nome })
            .ToDictionaryAsync(m => m.Id, ct);

        return
        [
            .. agrupado
                .Select(a => new FatiaDeVendaPerdida(
                    motivos.TryGetValue(a.Chave, out var motivo) ? motivo.Codigo : "SEM_MOTIVO",
                    motivos.TryGetValue(a.Chave, out var nome) ? nome.Nome : "Sem motivo registrado",
                    a.Quantidade,
                    a.Maquinas,
                    a.ComOsDoisPrecos,
                    Arredondar(a.DiferencaMedia)))
                .OrderByDescending(f => f.Quantidade)
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FatiaDeVendaPerdida>> ResumirPorConcorrenteAsync(CancellationToken ct)
    {
        // A LINHA SEM CONCORRENTE FICA DE FORA, e não vira "não informado": um ranking de para
        // quem se perdeu com uma fatia "não sei" no topo não responde nada. Quantas linhas não
        // declaram o concorrente é conta de outra pergunta, e a tela a faz pelo total.
        var agrupado = await contexto.VendasPerdidas
            .Where(v => v.ExcluidoEm == null && v.ConcorrenteId != null)
            .GroupBy(v => v.ConcorrenteId!.Value)
            .Select(g => new Bruto(
                g.Key,
                g.Count(),
                g.Sum(v => v.Quantidade),
                g.Count(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null),
                g.Average(v => v.PrecoDoConcorrente != null && v.PrecoOfertado != null
                    ? v.PrecoOfertado - v.PrecoDoConcorrente
                    : null)))
            .ToListAsync(ct);

        var itens = await contexto.CatalogoItens.AsNoTracking()
            .Where(i => i.CatalogoId == CatalogosDeSistema.Concorrente)
            .Select(i => new { i.Id, i.Codigo, i.Descricao })
            .ToDictionaryAsync(i => i.Id, ct);

        return
        [
            .. agrupado
                .Select(a => new FatiaDeVendaPerdida(
                    itens.TryGetValue(a.Chave, out var item) ? item.Codigo : "DESCONHECIDO",
                    itens.TryGetValue(a.Chave, out var nome) ? nome.Descricao : "Concorrente não catalogado",
                    a.Quantidade,
                    a.Maquinas,
                    a.ComOsDoisPrecos,
                    Arredondar(a.DiferencaMedia)))
                .OrderByDescending(f => f.Quantidade)
        ];
    }

    /// <summary>Centavo não muda decisão de diretoria, e engorda o JSON.</summary>
    private static decimal? Arredondar(decimal? valor) =>
        valor is null ? null : decimal.Round(valor.Value, 2);

    /// <summary>O agrupamento como o banco o devolve, antes de ganhar nome.</summary>
    private sealed record Bruto(
        int Chave, int Quantidade, int Maquinas, int ComOsDoisPrecos, decimal? DiferencaMedia);
}
