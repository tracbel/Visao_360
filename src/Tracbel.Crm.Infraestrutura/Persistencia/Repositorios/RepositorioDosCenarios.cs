using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// As metas dos Cenários de mercado no banco do CRM (issue 263). A tabela não tem filial — a fronteira de multiempresa
/// não se aplica, e quem pode gravar em qual município é conferido no caso de uso, pela filial responsável dele.
/// </summary>
public sealed class RepositorioDosCenarios(CrmDbContext contexto) : IRepositorioDosCenarios
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<MetaGravadaNoCenario>> ListarAsync(int categoriaDeMaquinaId, short anoFiscal, CancellationToken ct)
    {
        var metas = await (
                from meta in contexto.MetasDosCenarios.AsNoTracking()
                where meta.CategoriaDeMaquinaId == categoriaDeMaquinaId && meta.AnoFiscal == anoFiscal && meta.ExcluidoEm == null
                join municipio in contexto.Municipios.AsNoTracking() on meta.MunicipioId equals municipio.Id
                where municipio.CodigoIbge != null
                select new
                {
                    CodigoIbge = municipio.CodigoIbge!.Value,
                    meta.Cenario,
                    meta.ValorManual,
                    meta.MetaNaEscolha,
                    AutorId = meta.AlteradoPorId ?? meta.CriadoPorId,
                    Em = meta.AlteradoEm ?? meta.CriadoEm
                })
            .ToListAsync(ct);

        var autores = metas.Select(m => m.AutorId).Distinct().ToList();
        var nomes = autores.Count == 0
            ? new Dictionary<long, string>()
            : await contexto.Usuarios.AsNoTracking()
                .Where(u => autores.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.NomeExibicao, ct);

        return
        [
            .. metas.Select(m => new MetaGravadaNoCenario(
                m.CodigoIbge, m.Cenario, m.ValorManual, m.MetaNaEscolha, nomes.GetValueOrDefault(m.AutorId), m.Em))
        ];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, int?>> FiliaisResponsaveisAsync(CancellationToken ct) =>
        (await (
                from area in contexto.MunicipiosDaAreaDeAtuacao.AsNoTracking()
                where area.EncerradoEm == null
                join municipio in contexto.Municipios.AsNoTracking() on area.MunicipioId equals municipio.Id
                where municipio.CodigoIbge != null
                select new { CodigoIbge = municipio.CodigoIbge!.Value, area.Id, area.EmpresaResponsavelId })
            .ToListAsync(ct))
        .GroupBy(a => a.CodigoIbge)
        .ToDictionary(g => g.Key, g => g.OrderBy(a => a.Id).First().EmpresaResponsavelId);

    /// <inheritdoc />
    public Task<MunicipioDoCenario?> ObterMunicipioAsync(int codigoIbge, CancellationToken ct) =>
        (from municipio in contexto.Municipios.AsNoTracking()
         where municipio.CodigoIbge == codigoIbge
         let area = contexto.MunicipiosDaAreaDeAtuacao
             .Where(a => a.MunicipioId == municipio.Id && a.EncerradoEm == null)
             .OrderBy(a => a.Id)
             .Select(a => new { a.PertenceAAdr, a.EmpresaResponsavelId })
             .FirstOrDefault()
         select new MunicipioDoCenario(
             municipio.Id,
             codigoIbge,
             municipio.Nome,
             area != null && area.PertenceAAdr,
             area == null ? null : area.EmpresaResponsavelId))
        .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public Task<MetaDoCenarioNoMunicipio?> ObterParaAlterarAsync(
        int municipioId, int categoriaDeMaquinaId, short anoFiscal, CancellationToken ct) =>
        contexto.MetasDosCenarios.FirstOrDefaultAsync(
            m => m.MunicipioId == municipioId && m.CategoriaDeMaquinaId == categoriaDeMaquinaId && m.AnoFiscal == anoFiscal
                 && m.ExcluidoEm == null,
            ct);

    /// <inheritdoc />
    public async Task AdicionarAsync(MetaDoCenarioNoMunicipio meta, CancellationToken ct) =>
        await contexto.MetasDosCenarios.AddAsync(meta, ct);
}
