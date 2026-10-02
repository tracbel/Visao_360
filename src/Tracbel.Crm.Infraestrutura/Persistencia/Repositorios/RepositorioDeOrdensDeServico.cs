using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// AS ORDENS DE SERVIÇO DA OFICINA (02/10/2026), por cliente e por máquina. A OS passa pelo filtro global de filial — a de
/// outra filial não aparece —, e o cliente também: o de fora do alcance aparece sem nome.
/// </summary>
public sealed class RepositorioDeOrdensDeServico(CrmDbContext contexto) : IRepositorioDeOrdensDeServico
{
    /// <inheritdoc />
    public async Task<OrdensDeServicoLidas?> DoClienteAsync(Guid chaveDoCliente, CancellationToken ct)
    {
        var clienteId = await contexto.Clientes.AsNoTracking()
            .Where(c => c.ChavePublica == chaveDoCliente)
            .Select(c => (long?)c.Id)
            .FirstOrDefaultAsync(ct);

        return clienteId is null ? null : await LerAsync(contexto.OrdensDeServico.Where(o => o.ClienteId == clienteId), ct);
    }

    /// <inheritdoc />
    public async Task<OrdensDeServicoLidas?> DoEquipamentoAsync(Guid chaveDoEquipamento, CancellationToken ct)
    {
        var equipamentoId = await contexto.Equipamentos.AsNoTracking()
            .Where(e => e.ChavePublica == chaveDoEquipamento)
            .Select(e => (long?)e.Id)
            .FirstOrDefaultAsync(ct);

        return equipamentoId is null ? null : await LerAsync(contexto.OrdensDeServico.Where(o => o.EquipamentoId == equipamentoId), ct);
    }

    private async Task<OrdensDeServicoLidas> LerAsync(IQueryable<OrdemDeServico> consulta, CancellationToken ct)
    {
        var ordens = await consulta.AsNoTracking()
            .Where(o => o.ExcluidoEm == null)
            .Select(o => new
            {
                o.ChavePublica, o.Numero, o.EmpresaId, o.Situacao, o.TipoDeAtendimento, o.AbertaEm, o.LiberadaEm, o.FechadaEm,
                o.CanceladaEm, o.Chassi, o.Modelo, o.Horimetro, o.EquipamentoId, o.ClienteId, o.ValorDePecas, o.ValorDeServicos,
                o.ItensDePeca, o.ItensDeServico
            })
            .ToListAsync(ct);

        var idsDasFiliais = ordens.Select(o => o.EmpresaId).Distinct().ToList();
        var filiais = await contexto.Empresas.AsNoTracking()
            .Where(e => idsDasFiliais.Contains(e.Id))
            .Select(e => new { e.Id, e.Codigo, e.Nome })
            .ToDictionaryAsync(e => e.Id, ct);

        var idsDasMaquinas = ordens.Select(o => o.EquipamentoId).OfType<long>().Distinct().ToList();
        var maquinas = await contexto.Equipamentos.AsNoTracking()
            .Where(e => idsDasMaquinas.Contains(e.Id))
            .Select(e => new { e.Id, e.ChavePublica })
            .ToDictionaryAsync(e => e.Id, e => e.ChavePublica, ct);

        var idsDosClientes = ordens.Select(o => o.ClienteId).OfType<long>().Distinct().ToList();
        var clientes = await contexto.Clientes.AsNoTracking()
            .Where(c => idsDosClientes.Contains(c.Id))
            .Select(c => new { c.Id, c.ChavePublica, c.NomeRazao })
            .ToDictionaryAsync(c => c.Id, ct);

        var carregadoEm = await contexto.PontosDeSincronismo.AsNoTracking()
            .Where(p => p.Fluxo == OrdemDeServico.FluxoDaCarga)
            .Select(p => (DateTime?)p.ProcessadoEm)
            .FirstOrDefaultAsync(ct);

        return new OrdensDeServicoLidas(
            [.. ordens.Select(o =>
            {
                var filial = filiais.GetValueOrDefault(o.EmpresaId);
                var cliente = o.ClienteId is { } c ? clientes.GetValueOrDefault(c) : null;
                return new OrdemDeServicoParaConsulta(
                    o.ChavePublica, o.Numero, filial?.Codigo ?? "?", filial?.Nome ?? "?", o.Situacao.ToString(), o.TipoDeAtendimento,
                    o.AbertaEm, o.LiberadaEm, o.FechadaEm, o.CanceladaEm, o.Chassi, o.Modelo, o.Horimetro,
                    o.EquipamentoId is { } e && maquinas.TryGetValue(e, out var chave) ? chave : null,
                    cliente?.ChavePublica, cliente?.NomeRazao, o.ValorDePecas, o.ValorDeServicos, o.ItensDePeca, o.ItensDeServico);
            })],
            carregadoEm is { } instante ? DateTime.SpecifyKind(instante, DateTimeKind.Utc) : null);
    }
}
