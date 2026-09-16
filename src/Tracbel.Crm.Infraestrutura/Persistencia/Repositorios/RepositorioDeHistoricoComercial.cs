using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O histórico comercial de uma máquina: as vendas, o comprador em cada uma e a trilha da origem.
///
/// <para>A máquina, a venda e o comprador passam cada um pelo filtro global de filial: uma venda
/// feita por outra filial não aparece, e um comprador fora do alcance aparece sem nome.</para>
/// </summary>
public sealed class RepositorioDeHistoricoComercial(CrmDbContext contexto) : IRepositorioHistoricoComercial
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<MaquinaCompradaPeloCliente>?> ListarMaquinasCompradasAsync(Guid chaveDoCliente, CancellationToken ct)
    {
        var clienteId = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ChavePublica == chaveDoCliente)
            .Select(c => (long?)c.Id)
            .FirstOrDefaultAsync(ct);

        if (clienteId is null) return null;

        // O VÍNCULO É A FONTE, e não o dono da máquina: comprador na venda, com a data da venda.
        var vinculos = await contexto.VinculosComEquipamento.AsNoTracking()
            .Where(l => l.ClienteId == clienteId && l.EncerradoEm == null && l.VendaDeMaquinaId != null)
            .Select(l => new { l.EquipamentoId, VendaId = l.VendaDeMaquinaId!.Value, l.Natureza, l.EmpresaId, l.SistemaId })
            .ToListAsync(ct);

        if (vinculos.Count == 0) return [];

        var idsDasVendas = vinculos.Select(v => v.VendaId).ToList();
        var vendas = await contexto.VendasDeMaquina.AsNoTracking()
            .Where(v => idsDasVendas.Contains(v.Id))
            .Select(v => new { v.Id, v.VendidaEm, v.ProdutoNaOrigem })
            .ToDictionaryAsync(v => v.Id, ct);

        // A MÁQUINA PASSA PELO FILTRO DE FILIAL: a de outra filial não aparece nesta ficha.
        var idsDasMaquinas = vinculos.Select(v => v.EquipamentoId).Distinct().ToList();
        var maquinas = await contexto.Equipamentos.AsNoTracking()
            .Where(e => idsDasMaquinas.Contains(e.Id) && e.ExcluidoEm == null)
            .Select(e => new
            {
                e.Id,
                e.ChavePublica,
                e.Chassi,
                e.ClienteId,
                Modelo = contexto.Modelos.Where(m => m.Id == e.ModeloId).Select(m => m.Nome).FirstOrDefault(),
                Classificacao = contexto.LinhasDeProduto.Where(l => l.Id == e.LinhaDeProdutoId).Select(l => l.Nome).FirstOrDefault()
            })
            .ToDictionaryAsync(e => e.Id, ct);

        var idsDasFiliais = vinculos.Select(v => v.EmpresaId).Distinct().ToList();
        var filiais = await contexto.Empresas.AsNoTracking()
            .Where(e => idsDasFiliais.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Codigo, ct);

        var sistemas = await contexto.Sistemas.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Codigo, ct);

        return [.. vinculos
            .Where(v => maquinas.ContainsKey(v.EquipamentoId) && vendas.ContainsKey(v.VendaId))
            .Select(v =>
            {
                var maquina = maquinas[v.EquipamentoId];
                var venda = vendas[v.VendaId];
                return new MaquinaCompradaPeloCliente(
                    maquina.ChavePublica,
                    maquina.Chassi.Numero,
                    maquina.Modelo,
                    maquina.Classificacao,
                    venda.ProdutoNaOrigem,
                    venda.VendidaEm,
                    v.Natureza.ToString(),
                    filiais.GetValueOrDefault(v.EmpresaId, "?"),
                    v.SistemaId is { } sistema ? sistemas.GetValueOrDefault(sistema, "?") : "CRM",
                    maquina.ClienteId == clienteId);
            })
            .OrderByDescending(m => m.VendidaEm)
            .ThenBy(m => m.Chassi, StringComparer.Ordinal)];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VendaDaMaquinaComContexto>?> ListarVendasAsync(Guid chaveDoEquipamento, CancellationToken ct)
    {
        var equipamentoId = await contexto.Equipamentos.AsNoTracking()
            .Where(e => e.ChavePublica == chaveDoEquipamento)
            .Select(e => (long?)e.Id)
            .FirstOrDefaultAsync(ct);

        if (equipamentoId is null) return null;

        var vendas = await contexto.VendasDeMaquina.AsNoTracking()
            .Where(v => v.EquipamentoId == equipamentoId)
            .ToListAsync(ct);

        if (vendas.Count == 0) return [];

        var idsDasVendas = vendas.Select(v => v.Id).ToList();
        var vinculos = await contexto.VinculosComEquipamento.AsNoTracking()
            .Where(l => l.VendaDeMaquinaId != null && idsDasVendas.Contains(l.VendaDeMaquinaId.Value))
            .ToListAsync(ct);

        var idsDosCompradores = vendas.Select(v => v.CompradorId).Distinct().ToList();
        var compradores = await contexto.Clientes.AsNoTracking()
            .Where(c => idsDosCompradores.Contains(c.Id))
            .Select(c => new { c.Id, c.ChavePublica, c.NomeRazao })
            .ToDictionaryAsync(c => c.Id, ct);

        var idsDasFiliais = vendas.SelectMany(v => new[] { (int?)v.EmpresaId, v.EmpresaDoFaturamentoId }).OfType<int>().Distinct().ToList();
        var filiais = await contexto.Empresas.AsNoTracking()
            .Where(e => idsDasFiliais.Contains(e.Id))
            .Select(e => new { e.Id, e.Codigo, e.Nome })
            .ToDictionaryAsync(e => e.Id, ct);

        var sistemas = await contexto.Sistemas.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Codigo, ct);

        return [.. vendas
            .OrderByDescending(v => v.VendidaEm)
            .ThenByDescending(v => v.Id)
            .Select(v =>
            {
                // O vínculo que vale vem primeiro; o encerrado só aparece quando não há outro.
                var vinculo = vinculos
                    .Where(l => l.VendaDeMaquinaId == v.Id)
                    .OrderBy(l => l.EncerradoEm is null ? 0 : 1)
                    .ThenByDescending(l => l.Id)
                    .FirstOrDefault();

                var comprador = compradores.GetValueOrDefault(v.CompradorId);
                var filial = filiais.GetValueOrDefault(v.EmpresaId);

                return new VendaDaMaquinaComContexto(
                    v.ChavePublica,
                    sistemas.GetValueOrDefault(v.SistemaId, "?"),
                    v.ChaveOrigem,
                    v.VendidaEm,
                    v.FaturadaEm,
                    v.EntregueEm,
                    v.RegistradaNaOrigemEm,
                    filial?.Codigo ?? "?",
                    filial?.Nome ?? "?",
                    v.EmpresaDoFaturamentoId is { } faturou ? filiais.GetValueOrDefault(faturou)?.Codigo : null,
                    comprador?.ChavePublica,
                    comprador?.NomeRazao,
                    vinculo?.Natureza.ToString(),
                    vinculo?.ReferenciaEm,
                    vinculo?.EncerradoEm,
                    vinculo?.MotivoDoEncerramento,
                    v.LinhaNaOrigem,
                    v.ProdutoNaOrigem,
                    v.GestaoNaOrigem,
                    v.SituacaoNaOrigem,
                    v.NumeroDoPedido,
                    v.NumeroDaNotaFiscal,
                    v.VendaDireta,
                    v.RepasseDireto,
                    v.UnidadeNaOrigem,
                    v.UnidadeDoFaturamentoNaOrigem,
                    v.Transformacoes,
                    v.ImportadaEm,
                    v.AtualizadaPelaOrigemEm);
            })];
    }
}
