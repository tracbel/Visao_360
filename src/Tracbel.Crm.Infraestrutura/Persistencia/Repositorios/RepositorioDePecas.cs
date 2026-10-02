using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// AS PEÇAS DO PROTHEUS POR CLIENTE (02/10/2026). O faturamento e o orçamento passam pelo filtro global de filial: o que foi
/// faturado ou orçado por outra filial não aparece.
/// </summary>
public sealed class RepositorioDePecas(CrmDbContext contexto) : IRepositorioDePecas
{
    /// <inheritdoc />
    public async Task<PecasDoClienteLidas?> DoClienteAsync(Guid chaveDoCliente, DateOnly desde, CancellationToken ct)
    {
        var clienteId = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ChavePublica == chaveDoCliente)
            .Select(c => (long?)c.Id)
            .FirstOrDefaultAsync(ct);
        if (clienteId is null) return null;

        var faturamento = await contexto.FaturamentosDePecasNoMes.AsNoTracking()
            .Where(f => f.ClienteId == clienteId && f.Competencia >= desde)
            .Select(f => new
            {
                f.Competencia, f.EmpresaId, f.Setor, f.Grupo, f.Linha, f.VendedorNome, f.ValorLiquido, f.ValorDeDevolucoes,
                f.ValorDeDesconto, f.ValorDeTabela, f.Itens
            })
            .ToListAsync(ct);

        var orcamentos = await contexto.OrcamentosDePecas.AsNoTracking()
            .Where(o => o.ClienteId == clienteId && o.ExcluidoEm == null)
            .Select(o => new
            {
                o.ChavePublica, o.Numero, o.EmpresaId, o.Situacao, o.Prazo, o.Reserva, o.OrcadoEm, o.ValidoAte, o.VendedorNome, o.ValorTotal, o.Itens
            })
            .ToListAsync(ct);

        var idsDasFiliais = faturamento.Select(f => f.EmpresaId).Concat(orcamentos.Select(o => o.EmpresaId)).Distinct().ToList();
        var filiais = await contexto.Empresas.AsNoTracking()
            .Where(e => idsDasFiliais.Contains(e.Id))
            .Select(e => new { e.Id, e.Codigo, e.Nome })
            .ToDictionaryAsync(e => e.Id, ct);

        var pontos = await contexto.PontosDeSincronismo.AsNoTracking()
            .Where(p => p.Fluxo == FaturamentoDePecasNoMes.FluxoDaCarga || p.Fluxo == OrcamentoDePecas.FluxoDaCarga)
            .Select(p => new { p.Fluxo, p.ProcessadoEm })
            .ToListAsync(ct);
        DateTime? Lido(string fluxo) => pontos.FirstOrDefault(p => p.Fluxo == fluxo) is { } p ? DateTime.SpecifyKind(p.ProcessadoEm, DateTimeKind.Utc) : null;

        return new PecasDoClienteLidas(
            [.. faturamento.Select(f => new PecasDoClienteNoMes(
                f.Competencia, filiais.GetValueOrDefault(f.EmpresaId)?.Codigo ?? "?", filiais.GetValueOrDefault(f.EmpresaId)?.Nome ?? "?",
                f.Setor, f.Grupo, f.Linha, f.VendedorNome, f.ValorLiquido, f.ValorDeDevolucoes, f.ValorDeDesconto, f.ValorDeTabela, f.Itens))],
            [.. orcamentos.Select(o => new OrcamentoDePecasParaConsulta(
                o.ChavePublica, o.Numero, filiais.GetValueOrDefault(o.EmpresaId)?.Codigo ?? "?", filiais.GetValueOrDefault(o.EmpresaId)?.Nome ?? "?",
                o.Situacao, o.Prazo, o.Reserva, o.OrcadoEm, o.ValidoAte, o.VendedorNome, o.ValorTotal, o.Itens))],
            Lido(FaturamentoDePecasNoMes.FluxoDaCarga),
            Lido(OrcamentoDePecas.FluxoDaCarga));
    }
}
