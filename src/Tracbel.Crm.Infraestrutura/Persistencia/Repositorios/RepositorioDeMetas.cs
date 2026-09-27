using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Infraestrutura.Persistencia.Repositorios;

/// <summary>
/// A META DE VENDA × O REALIZADO (#138, decisões de 27/09/2026), para a filial do contexto de acesso.
///
/// <para><b>A meta</b> é a cota da API Gestão de Negócios (<c>organizacao.MetaDeVenda</c>), em unidades. As duplicatas de
/// negócio da origem SE SOMAM aqui — é por isso que a tabela guarda uma linha por id da GN. O consórcio fica à parte, em
/// cotas (D-M4).</para>
///
/// <para><b>O realizado</b> é só <c>frota.VendaDeMaquina</c> (D-M3), pela data da venda, uma máquina por venda: é o que o
/// CRM tem. As vendas que o ART tem e o CRM ainda não — comprador sem cadastro, chassi incompleto — são contadas à parte,
/// como lacuna, e não somadas. Por consultor, conta a PESSOA: o vendedor do ART (D-M2), casado com o consultor da meta em
/// maiúsculas. Por filial, conta a filial da venda — e isso quem faz é o filtro global, como em todo repositório.</para>
///
/// <para><b>Somas em memória</b>, como nos indicadores: o volume é o de um ano de metas e vendas de uma filial (centenas de
/// linhas), e o SQLite dos testes não agrega do mesmo jeito.</para>
/// </summary>
public sealed class RepositorioDeMetas(CrmDbContext contexto) : IRepositorioDeMetas
{
    /// <summary>O nome do sistema da meta, como a tela mostra na procedência.</summary>
    private const string NomeDoSistema = "API Gestão de Negócios";

    /// <summary>A rota lida pela carga.</summary>
    private const string RotaDasMetas = "/api/v1/cadastros/metas";

    private sealed record LinhaDeMeta(DateOnly Competencia, string CodigoDaLinha, string LinhaNaOrigem, string Consultor, long? ConsultorUsuarioId, bool Consorcio, int Quantidade);

    private sealed record LinhaDeVenda(DateOnly Mes, string Codigo, string LinhaNaOrigem, string? Vendedor);

    /// <inheritdoc />
    public async Task<MetaERealizadoApurado> ApurarAsync(
        JanelaDeCompetencia periodo, JanelaDeCompetencia anterior, DateOnly? mesEmCurso, AlcanceDaMeta alcance, CancellationToken ct)
    {
        var acesso = contexto.Acesso;

        // O LOGIN DA PESSOA: a parte antes do @ do nome principal, em maiúsculas — é o consultor da meta e o vendedor do ART.
        var logins = await contexto.Usuarios.AsNoTracking()
            .Where(u => u.ExcluidoEm == null)
            .Select(u => new { u.Id, u.NomePrincipal })
            .ToListAsync(ct);
        var loginsComConta = logins.Select(u => Login(u.NomePrincipal)).ToHashSet(StringComparer.Ordinal);
        var meuLogin = alcance == AlcanceDaMeta.Proprios
            ? logins.Where(u => u.Id == acesso.UsuarioId).Select(u => Login(u.NomePrincipal)).FirstOrDefault() ?? string.Empty
            : null;

        var ultimoMes = mesEmCurso is { } emCurso && emCurso > periodo.Final ? emCurso : periodo.Final;
        var inicio = periodo.Inicial;

        // ---------------------------------------------------------------------------------------------
        // A meta: do começo do período até o mês em curso, quando ele entra na conta à parte.
        // ---------------------------------------------------------------------------------------------
        var metas = (await contexto.MetasDeVenda.AsNoTracking()
                .Where(m => m.ExcluidoEm == null && m.Competencia >= inicio && m.Competencia <= ultimoMes)
                .Select(m => new { m.Competencia, m.CodigoDaLinha, m.LinhaNaOrigem, m.ConsultorNaOrigem, m.ConsultorUsuarioId, m.Origem, m.Quantidade })
                .ToListAsync(ct))
            .Select(m => new LinhaDeMeta(m.Competencia, m.CodigoDaLinha, m.LinhaNaOrigem, m.ConsultorNaOrigem, m.ConsultorUsuarioId,
                MetaDeVenda.EhConsorcio(m.Origem, m.CodigoDaLinha), m.Quantidade))
            .Where(m => meuLogin is null || m.ConsultorUsuarioId == acesso.UsuarioId || m.Consultor == meuLogin)
            .ToList();

        var ultimaComMeta = await contexto.MetasDeVenda.AsNoTracking()
            .Where(m => m.ExcluidoEm == null)
            .MaxAsync(m => (DateOnly?)m.Competencia, ct);

        // ---------------------------------------------------------------------------------------------
        // O realizado: as vendas do período, do mês em curso e do mesmo trecho do ano fiscal anterior.
        // ---------------------------------------------------------------------------------------------
        var fimDoPeriodo = ultimoMes.AddMonths(1);
        var inicioDoAnterior = anterior.Inicial;
        var fimDoAnterior = anterior.Final.AddMonths(1);

        var vendas = (await contexto.VendasDeMaquina.AsNoTracking()
                .Where(v => v.ExcluidoEm == null && v.VendidaEm != null
                            && ((v.VendidaEm >= inicio && v.VendidaEm < fimDoPeriodo) || (v.VendidaEm >= inicioDoAnterior && v.VendidaEm < fimDoAnterior)))
                .Select(v => new { VendidaEm = v.VendidaEm!.Value, v.LinhaNaOrigem, v.VendedorNaOrigem })
                .ToListAsync(ct))
            .Select(v => new LinhaDeVenda(new DateOnly(v.VendidaEm.Year, v.VendidaEm.Month, 1), CodigoEstavel.De(v.LinhaNaOrigem, MetaDeVenda.TamanhoDaLinha),
                v.LinhaNaOrigem, string.IsNullOrWhiteSpace(v.VendedorNaOrigem) ? null : v.VendedorNaOrigem.Trim().ToUpperInvariant()))
            .Where(v => meuLogin is null || v.Vendedor == meuLogin)
            .ToList();

        bool NoPeriodo(DateOnly mes) => periodo.Contem(mes);

        var metasDoPeriodo = metas.Where(m => NoPeriodo(m.Competencia)).ToList();
        var metasDeMaquinas = metasDoPeriodo.Where(m => !m.Consorcio).ToList();
        var vendasDoPeriodo = vendas.Where(v => NoPeriodo(v.Mes)).ToList();

        var porMes = Enumerable.Range(0, periodo.Meses)
            .Select(i => periodo.Inicial.AddMonths(i))
            .Select(mes => new MetaERealizadoNoMes(
                mes,
                metasDeMaquinas.Where(m => m.Competencia == mes).Sum(m => m.Quantidade),
                vendasDoPeriodo.Count(v => v.Mes == mes),
                metasDoPeriodo.Where(m => m.Consorcio && m.Competencia == mes).Sum(m => m.Quantidade)))
            .ToList();

        var nomeDaLinha = metasDeMaquinas.GroupBy(m => m.CodigoDaLinha, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().LinhaNaOrigem, StringComparer.Ordinal);
        foreach (var venda in vendasDoPeriodo) nomeDaLinha.TryAdd(venda.Codigo, venda.LinhaNaOrigem);

        var porLinha = nomeDaLinha
            .Select(p => new MetaERealizadoNaLinha(
                p.Key, p.Value,
                metasDeMaquinas.Where(m => m.CodigoDaLinha == p.Key).Sum(m => m.Quantidade),
                vendasDoPeriodo.Count(v => v.Codigo == p.Key)))
            .OrderByDescending(l => l.Meta).ThenByDescending(l => l.Realizado).ThenBy(l => l.Nome, StringComparer.Ordinal)
            .ToList();

        var consultores = metasDeMaquinas.Select(m => m.Consultor)
            .Concat(vendasDoPeriodo.Where(v => v.Vendedor is not null).Select(v => v.Vendedor!))
            .Distinct(StringComparer.Ordinal);

        var porConsultor = consultores
            .Select(c => new MetaERealizadoDoConsultor(
                c,
                loginsComConta.Contains(c) || metasDeMaquinas.Any(m => m.Consultor == c && m.ConsultorUsuarioId is not null),
                metasDeMaquinas.Where(m => m.Consultor == c).Sum(m => m.Quantidade),
                vendasDoPeriodo.Count(v => v.Vendedor == c)))
            .OrderByDescending(c => c.Meta).ThenByDescending(c => c.Realizado).ThenBy(c => c.Consultor, StringComparer.Ordinal)
            .ToList();

        MetaERealizadoNoMes? doMesEmCurso = mesEmCurso is { } mesCorrente && !NoPeriodo(mesCorrente)
            ? new MetaERealizadoNoMes(
                mesCorrente,
                metas.Where(m => !m.Consorcio && m.Competencia == mesCorrente).Sum(m => m.Quantidade),
                vendas.Count(v => v.Mes == mesCorrente),
                metas.Where(m => m.Consorcio && m.Competencia == mesCorrente).Sum(m => m.Quantidade))
            : null;

        var (pendentes, pendentesSemFilial) = alcance == AlcanceDaMeta.Filial
            ? await PendentesNoArtAsync(periodo, ct)
            : ((int?)null, 0);

        return new MetaERealizadoApurado(
            metasDeMaquinas.Sum(m => m.Quantidade),
            vendasDoPeriodo.Count,
            pendentes,
            pendentesSemFilial,
            metasDoPeriodo.Where(m => m.Consorcio).Sum(m => m.Quantidade),
            vendasDoPeriodo.Count(v => v.Vendedor is null),
            metasDeMaquinas.Where(m => !porConsultor.Any(c => c.Consultor == m.Consultor && c.TemConta)).Select(m => m.Consultor).Distinct(StringComparer.Ordinal).Count(),
            ultimaComMeta,
            porMes,
            porLinha,
            porConsultor,
            vendas.Count(v => anterior.Contem(v.Mes)),
            doMesEmCurso,
            await OrigemAsync(ct));
    }

    /// <summary>
    /// AS VENDAS DO ART QUE O CRM AINDA NÃO TEM (D-M3), no período e na filial: registro sem venda, ainda presente na
    /// origem, cuja unidade corresponde a uma filial que a pessoa alcança. A trilha da origem não tem filial — ela vem da
    /// correspondência da unidade, a mesma que a carga do ART usa. A pendente de unidade sem filial não é de filial nenhuma.
    /// </summary>
    private async Task<(int? Pendentes, int SemFilial)> PendentesNoArtAsync(JanelaDeCompetencia periodo, CancellationToken ct)
    {
        var art = await contexto.Sistemas.AsNoTracking()
            .Where(s => s.Codigo == ConexoesDoSistema.Art).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
        if (art is not { } sistemaDoArt) return (0, 0);

        var inicio = periodo.Inicial;
        var fim = periodo.Final.AddMonths(1);

        var unidades = await contexto.RegistrosDeOrigem.AsNoTracking()
            .Where(r => r.SistemaId == sistemaDoArt && r.Fluxo == MetaDeVenda.FluxoDasVendasDoArt && r.VendaDeMaquinaId == null
                        && r.AusenteNaOrigemDesde == null && r.VendidaEm != null && r.VendidaEm >= inicio && r.VendidaEm < fim)
            .Select(r => r.UnidadeNaOrigem)
            .ToListAsync(ct);

        var filialDaUnidade = await contexto.CorrespondenciasDaOrigem.AsNoTracking()
            .Where(c => c.SistemaId == sistemaDoArt && c.Tipo == TipoDeCorrespondencia.Unidade && c.EmpresaCorrespondenteId != null
                        && (c.Situacao == SituacaoDaCorrespondencia.CorrespondenciaExata || c.Situacao == SituacaoDaCorrespondencia.ConfirmadaPorRevisao))
            .Select(c => new { c.TextoNaOrigem, c.EmpresaCorrespondenteId })
            .ToListAsync(ct);
        var porTexto = filialDaUnidade
            .GroupBy(c => c.TextoNaOrigem, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().EmpresaCorrespondenteId!.Value, StringComparer.OrdinalIgnoreCase);

        var acesso = contexto.Acesso;
        bool Alcanca(int empresa) => acesso.EhServicoDeSistema || acesso.EmpresasVisiveis.Contains(empresa) || acesso.EmpresaId == empresa;

        var daFilial = unidades.Count(u => u is not null && porTexto.TryGetValue(u, out var empresa) && Alcanca(empresa));
        var semFilial = unidades.Count(u => u is null || !porTexto.ContainsKey(u));
        return (daFilial, semFilial);
    }

    private async Task<OrigemDaMetaDeVenda?> OrigemAsync(CancellationToken ct)
    {
        var ponto = await contexto.PontosDeSincronismo.AsNoTracking()
            .Where(p => p.Fluxo == MetaDeVenda.FluxoDaCarga)
            .Select(p => new { p.ProcessadoEm, p.UltimoValor })
            .FirstOrDefaultAsync(ct);
        if (ponto is null) return null;

        DateTime? gerada = DateTime.TryParse(ponto.UltimoValor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var instante)
            ? DateTime.SpecifyKind(instante.Kind == DateTimeKind.Local ? instante.ToUniversalTime() : instante, DateTimeKind.Utc)
            : null;

        return new OrigemDaMetaDeVenda(NomeDoSistema, RotaDasMetas, DateTime.SpecifyKind(ponto.ProcessadoEm, DateTimeKind.Utc), gerada);
    }

    /// <summary>O login da conta, em maiúsculas: a parte antes do <c>@</c> do nome principal.</summary>
    private static string Login(string nomePrincipal)
    {
        var arroba = nomePrincipal.IndexOf('@', StringComparison.Ordinal);
        return (arroba < 0 ? nomePrincipal : nomePrincipal[..arroba]).Trim().ToUpperInvariant();
    }
}
