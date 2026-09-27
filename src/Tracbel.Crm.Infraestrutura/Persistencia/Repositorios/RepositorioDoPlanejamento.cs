using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// As vigências do planejamento comercial no banco do CRM (issue 256). Como as do potencial, valem para a empresa
/// inteira: nenhuma das duas tabelas tem filial.
/// </summary>
public sealed class RepositorioDoPlanejamento(CrmDbContext contexto)
    : IRepositorioDoPlanejamento,
      IRepositorioDeVigenciasDoPlanejamento
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<ParametroDoPlanejamento>> ListarParametrosAsync(CancellationToken ct) =>
        await contexto.ParametrosDoPlanejamento.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShareAlvoDaCategoria>> ListarSharesAsync(CancellationToken ct) =>
        await contexto.SharesAlvo.AsNoTracking().ToListAsync(ct);

    /// <inheritdoc />
    public Task<ParametroDoPlanejamento?> ObterParametroAsync(DateOnly vigenteDesde, CancellationToken ct) =>
        contexto.ParametrosDoPlanejamento
            .FirstOrDefaultAsync(p => p.VigenteDesde == vigenteDesde && p.RevogadoEm == null, ct);

    /// <inheritdoc />
    public Task<ShareAlvoDaCategoria?> ObterShareAsync(int categoriaDeMaquinaId, DateOnly vigenteDesde, CancellationToken ct) =>
        contexto.SharesAlvo
            .FirstOrDefaultAsync(
                s => s.CategoriaDeMaquinaId == categoriaDeMaquinaId && s.VigenteDesde == vigenteDesde && s.RevogadoEm == null, ct);

    /// <inheritdoc />
    public async Task AdicionarAsync(ParametroComVigencia parametro, CancellationToken ct) =>
        await contexto.AddAsync((object)parametro, ct);
}
