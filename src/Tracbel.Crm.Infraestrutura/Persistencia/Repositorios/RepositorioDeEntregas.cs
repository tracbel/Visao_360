using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// AS ENTREGAS DE MÁQUINA DO ART (02/10/2026) — a "Entrega realizada" da Demanda e Previsão, pela data da entrega.
///
/// <para><b>Três leituras pequenas e o cruzamento em memória</b>, como na apuração territorial: o de-para da linha de produto
/// é texto ASCII de colação binária, e um JOIN com o código Unicode da categoria daria conflito de colação no SQL Server.
/// As tabelas do de-para têm dezenas de linhas; as entregas de dois anos fiscais, poucos milhares.</para>
///
/// <para><b>A máquina e o comprador entram por fora (junção à esquerda)</b>: a máquina de outra filial, fora do alcance de
/// quem lê, não apaga a entrega — só a categoria fica sem saber; o comprador sem endereço principal fica sem município.</para>
/// </summary>
public sealed class RepositorioDeEntregas(CrmDbContext contexto) : IRepositorioDeEntregas
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<EntregaDeMaquina>> ListarAsync(DateOnly inicio, DateOnly fimExclusivo, CancellationToken ct)
    {
        var entregas = await (
                from venda in contexto.VendasDeMaquina.AsNoTracking()
                where venda.ExcluidoEm == null && venda.EntregueEm != null && venda.EntregueEm >= inicio && venda.EntregueEm < fimExclusivo
                join maquina in contexto.Equipamentos.AsNoTracking().Where(e => e.ExcluidoEm == null)
                    on venda.EquipamentoId equals maquina.Id into maquinas
                from maquina in maquinas.DefaultIfEmpty()
                join endereco in contexto.Enderecos.AsNoTracking().Where(e => e.EhPrincipal && e.ExcluidoEm == null)
                    on venda.CompradorId equals endereco.ClienteId into enderecos
                from endereco in enderecos.DefaultIfEmpty()
                join municipio in contexto.Municipios.AsNoTracking()
                    on endereco.MunicipioId equals (int?)municipio.Id into municipios
                from municipio in municipios.DefaultIfEmpty()
                select new
                {
                    venda.Id,
                    EntregueEm = venda.EntregueEm!.Value,
                    LinhaDeProdutoId = maquina == null ? null : maquina.LinhaDeProdutoId,
                    MunicipioIbge = municipio == null ? null : municipio.CodigoIbge,
                    EnderecoId = endereco == null ? (long?)null : endereco.Id
                })
            .ToListAsync(ct);

        var codigoDaLinha = await contexto.LinhasDeProduto.AsNoTracking().ToDictionaryAsync(l => l.Id, l => l.Codigo, ct);
        var categoriaDaLinha = (await (
                from ligacao in contexto.LinhasDeProdutoNasCategorias.AsNoTracking()
                join categoria in contexto.CategoriasDeMaquina.AsNoTracking() on ligacao.CategoriaDeMaquinaId equals categoria.Id
                select new { ligacao.CodigoDaLinha, categoria.Codigo })
            .ToListAsync(ct))
            .GroupBy(l => l.CodigoDaLinha, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Codigo, StringComparer.Ordinal);

        // UMA LINHA POR VENDA: o comprador com mais de um endereço principal vale pelo de menor Id, como na apuração.
        return
        [
            .. entregas
                .GroupBy(e => e.Id)
                .Select(g => g.OrderBy(e => e.EnderecoId ?? long.MaxValue).First())
                .Select(e => new EntregaDeMaquina(
                    e.EntregueEm,
                    e.MunicipioIbge,
                    e.LinhaDeProdutoId is { } linha
                    && codigoDaLinha.TryGetValue(linha, out var codigo)
                    && categoriaDaLinha.TryGetValue(codigo, out var categoria)
                        ? categoria
                        : null))
        ];
    }
}
