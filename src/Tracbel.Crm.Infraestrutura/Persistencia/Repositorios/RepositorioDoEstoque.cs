using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// O ESTOQUE E A COBERTURA (28/09/2026). As máquinas passam pelo filtro global — as filiais ao alcance de quem lê; a
/// cobertura é da organização e não tem filial. Umas setecentas máquinas: a tela soma em memória.
/// </summary>
public sealed class RepositorioDoEstoque(CrmDbContext contexto) : IRepositorioDoEstoque
{
    /// <inheritdoc />
    public async Task<EstoqueLido> LerAsync(CancellationToken ct)
    {
        var maquinas = await contexto.EquipamentosEmEstoque.AsNoTracking()
            .Where(e => e.ExcluidoEm == null)
            .Join(contexto.Empresas.AsNoTracking(), e => e.EmpresaId, f => f.Id, (e, f) => new MaquinaNoEstoque(
                f.Nome, e.Grupo, e.Descricao, e.Configuracao, e.Situacao, e.Tipo, e.EhUsado, e.AnoModelo, e.Chassi, e.EntradaEm,
                e.ChegadaPrevistaEm, e.FaturamentoPrevistoEm, e.Pago, e.Reservado, e.SituacaoNaFabrica))
            .ToListAsync(ct);

        var cobertura = await contexto.CoberturasDoEstoque.AsNoTracking()
            .Where(c => c.ExcluidoEm == null)
            .Select(c => new { c.Recorte, Item = new ItemDaCoberturaDoEstoque(c.Chave, c.Competencia, c.MesesDeEstoque, c.Vendas) })
            .ToListAsync(ct);

        var pontos = await contexto.PontosDeSincronismo.AsNoTracking()
            .Where(p => p.Fluxo == EquipamentoEmEstoque.FluxoDaCarga || p.Fluxo == CoberturaDoEstoque.FluxoDaCarga)
            .Select(p => new { p.Fluxo, p.ProcessadoEm, p.UltimoValor })
            .ToListAsync(ct);

        DateTime? Lido(string fluxo) => pontos.FirstOrDefault(p => p.Fluxo == fluxo) is { } p ? DateTime.SpecifyKind(p.ProcessadoEm, DateTimeKind.Utc) : null;
        DateTime? Gerado(string fluxo) =>
            pontos.FirstOrDefault(p => p.Fluxo == fluxo) is { } p
            && DateTime.TryParse(p.UltimoValor, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var instante)
                ? DateTime.SpecifyKind(instante, DateTimeKind.Utc)
                : null;

        return new EstoqueLido(
            maquinas,
            [.. cobertura.Where(c => c.Recorte == CoberturaDoEstoque.RecortePorMes).Select(c => c.Item).OrderBy(i => i.Competencia)],
            [.. cobertura.Where(c => c.Recorte == CoberturaDoEstoque.RecortePorGrupo).Select(c => c.Item).OrderByDescending(i => i.Meses)],
            Lido(EquipamentoEmEstoque.FluxoDaCarga),
            Gerado(EquipamentoEmEstoque.FluxoDaCarga),
            Lido(CoberturaDoEstoque.FluxoDaCarga),
            Gerado(CoberturaDoEstoque.FluxoDaCarga));
    }
}
