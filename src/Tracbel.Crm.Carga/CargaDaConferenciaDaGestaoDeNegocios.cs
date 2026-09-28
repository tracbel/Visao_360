using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.GestaoDeNegocios;

namespace Tracbel.Crm.Carga;

/// <summary>O que a conferência apurou (ou apuraria, na simulação), em número — sem nome de ninguém.</summary>
internal sealed record RelatorioDaConferencia(bool Simulada, IReadOnlyList<(string Rotulo, int Valor)> Contagens)
{
    /// <summary>A contagem de um rótulo.</summary>
    public int Valor(string rotulo) => Contagens.Where(c => c.Rotulo == rotulo).Sum(c => c.Valor);
}

/// <summary>
/// A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026) — <c>--somente-conferencia-gn</c>, a rotina 13
/// <c>CONFERENCIA_GESTAO_NEGOCIOS</c>.
///
/// <para><b>O gabarito é a performance-maquinas da GN</b>: a meta (o PO) e o realizado, que é a máquina ENTREGUE, no mês da
/// entrega — a mesma régua que o CRM usa desde 28/09/2026. A conferência compara:</para>
/// <list type="bullet">
/// <item><b>filial a filial, mês a mês</b> — a meta e o realizado lá e aqui (<see cref="ConferenciaDaGestaoDeNegocios"/>,
/// regravada inteira a cada rodada);</item>
/// <item><b>chassi a chassi</b> — cada máquina do realizado que não bate vira uma <see cref="DivergenciaDeIntegracao"/>: só na
/// GN, só no CRM, em outra filial, em outro mês, sem a entrega no CRM, ou pendente na integração do ART (com os motivos). A
/// divergência que some é encerrada como "deixou de ocorrer"; a que volta, reaberta — nunca apagada.</item>
/// </list>
///
/// <para><b>Só a janela que a GN cobre</b>: os meses que a performance traz (o ano fiscal corrente). A venda do CRM fora
/// deles não entra na conta.</para>
/// </summary>
internal sealed class CargaDaConferenciaDaGestaoDeNegocios(
    Func<CrmDbContext> abrirContexto,
    Func<CancellationToken, Task<Resultado<LeituraDaConferenciaNaOrigem>>> ler,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>Os tipos de divergência que esta conferência mantém.</summary>
    internal static readonly TipoDeDivergencia[] TiposDaConferencia =
    [
        TipoDeDivergencia.RealizadoSoNaGestao, TipoDeDivergencia.RealizadoSoNoCrm, TipoDeDivergencia.RealizadoEmOutraFilial,
        TipoDeDivergencia.RealizadoEmOutroMes, TipoDeDivergencia.RealizadoNaoEntregueNoCrm, TipoDeDivergencia.RealizadoPendenteNoArt
    ];

    /// <summary>Rótulo: máquinas do realizado da GN.</summary>
    internal const string RotuloRealizadoNaGestao = "máquinas no realizado da GN";

    /// <summary>Rótulo: máquinas do realizado do CRM, nos mesmos meses.</summary>
    internal const string RotuloRealizadoNoCrm = "máquinas no realizado do CRM (entregues, nos mesmos meses)";

    /// <summary>Rótulo: batem.</summary>
    internal const string RotuloBatem = "batem (mesmo chassi, filial e mês)";

    /// <summary>Rótulo: linhas da GN de loja sem filial no CRM.</summary>
    internal const string RotuloSemFilial = "linhas da GN de loja sem filial no CRM (não conferidas)";

    /// <summary>Rótulo: divergências novas.</summary>
    internal const string RotuloNovas = "divergências novas";

    /// <summary>Rótulo: divergências que deixaram de ocorrer.</summary>
    internal const string RotuloEncerradas = "divergências que deixaram de ocorrer";

    /// <summary>O rótulo da contagem de cada tipo.</summary>
    internal static string RotuloDoTipo(TipoDeDivergencia tipo) => $"  {tipo}";

    private readonly List<(string Rotulo, int Valor)> _contagens = [];

    /// <summary>Executa a conferência.</summary>
    public async Task<Resultado<RelatorioDaConferencia>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        relatar("Lendo o gabarito da API Gestão de Negócios (performance de máquinas e filiais; só GET)…");
        var lida = await ler(ct);
        if (!lida.EhSucesso) return Resultado<RelatorioDaConferencia>.Indisponivel(lida.Erro!);

        var origem = lida.Valor;
        if (origem.Realizado.Count == 0 && origem.Meta.Count == 0)
            return Resultado<RelatorioDaConferencia>.Indisponivel(
                "A performance de máquinas veio vazia. Isso é uma leitura a conferir — nada foi gravado e nenhuma divergência foi encerrada.");

        var agora = relogio();
        Plano plano;
        await using (var banco = abrirContexto())
        {
            plano = await PlanejarAsync(banco, origem, ct);
        }

        Contar(RotuloRealizadoNaGestao, plano.RealizadoNaGestao);
        Contar(RotuloRealizadoNoCrm, plano.RealizadoNoCrm);
        Contar(RotuloBatem, plano.Batem);
        Contar(RotuloSemFilial, plano.SemFilial);
        foreach (var tipo in TiposDaConferencia) Contar(RotuloDoTipo(tipo), plano.Divergencias.Count(d => d.Tipo == tipo));
        Contar("linhas da conferência por filial e mês", plano.Resumo.Count);

        if (!plano.EsquemaExiste && !simular)
            return Resultado<RelatorioDaConferencia>.Indisponivel(
                "O banco ainda não tem a migração da conferência. Publique a versão com ela antes — nada foi gravado.");

        if (simular)
        {
            relatar("SIMULAÇÃO: nada foi gravado.");
            return Resultado<RelatorioDaConferencia>.Ok(new RelatorioDaConferencia(true, _contagens));
        }

        await AplicarAsync(plano, origem, agora, ct);
        return Resultado<RelatorioDaConferencia>.Ok(new RelatorioDaConferencia(false, _contagens));
    }

    // =============================================================================================
    // O plano — só leitura
    // =============================================================================================

    private static async Task<Plano> PlanejarAsync(CrmDbContext banco, LeituraDaConferenciaNaOrigem origem, CancellationToken ct)
    {
        var plano = new Plano { EsquemaExiste = await EsquemaExisteAsync(banco, ct) };

        var empresas = await banco.Empresas.AsNoTracking().Select(e => new { e.Id, e.Codigo, e.Nome }).ToListAsync(ct);
        var empresaPorCodigo = empresas.ToDictionary(e => e.Codigo, e => e.Id, StringComparer.Ordinal);
        var nomeDaEmpresa = empresas.ToDictionary(e => e.Id, e => e.Nome);
        var codigoPorNome = FiliaisDaGestaoDeNegocios.CodigoPorNome(origem.Filiais);
        int? Empresa(string? filial) =>
            codigoPorNome.TryGetValue(CodigoEstavel.De(filial ?? string.Empty), out var codigo) && empresaPorCodigo.TryGetValue(codigo, out var id) ? id : null;

        // ---- o gabarito da GN ----
        var realizadoDaGestao = new Dictionary<string, (int EmpresaId, DateOnly Mes, int Quantidade)>(StringComparer.Ordinal);
        var resumoDaGestao = new Dictionary<(string Indicador, int EmpresaId, DateOnly Mes), int>();
        foreach (var r in origem.Realizado)
        {
            var mes = LeitorDoPlanejamentoDaGestaoDeNegocios.Mes(r.MesRotulo);
            var quantidade = Quantidade(r.Quantidade);
            if (Empresa(r.Filial) is not { } empresa || mes is null)
            {
                plano.SemFilial++;
                continue;
            }

            plano.RealizadoNaGestao += quantidade;
            Somar(resumoDaGestao, (ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, empresa, mes.Value), quantidade);
            if (Chassi(r.Chassi) is { } chassi) realizadoDaGestao.TryAdd(chassi, (empresa, mes.Value, quantidade));
        }

        foreach (var m in origem.Meta)
        {
            var mes = LeitorDoPlanejamentoDaGestaoDeNegocios.Mes(m.MesRotulo);
            if (Empresa(m.Filial) is not { } empresa || mes is null)
            {
                plano.SemFilial++;
                continue;
            }

            Somar(resumoDaGestao, (ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, empresa, mes.Value), Quantidade(m.Quantidade));
        }

        var meses = resumoDaGestao.Keys.Select(k => k.Mes).ToHashSet();
        var mesesDoRealizado = resumoDaGestao.Keys.Where(k => k.Indicador == ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas)
            .Select(k => k.Mes).ToHashSet();
        if (meses.Count == 0) return plano;
        var inicio = meses.Min();
        var fim = meses.Max().AddMonths(1);

        // ---- o CRM: as vendas com o chassi da máquina, e as pendentes do ART ----
        var vendas = await banco.VendasDeMaquina.IgnoreQueryFilters().AsNoTracking()
            .Where(v => v.ExcluidoEm == null)
            .Join(banco.Equipamentos.IgnoreQueryFilters().AsNoTracking(), v => v.EquipamentoId, e => e.Id,
                (v, e) => new { v.Id, v.EquipamentoId, v.EmpresaId, v.EntregueEm, v.VendidaEm, e.Chassi })
            .ToListAsync(ct);
        var vendaPorChassi = vendas
            .GroupBy(v => v.Chassi.Numero, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(v => v.EntregueEm ?? v.VendidaEm ?? DateOnly.MinValue).ThenByDescending(v => v.Id).First(),
                StringComparer.Ordinal);

        var art = await banco.Sistemas.AsNoTracking().Where(s => s.Codigo == ConexoesDoSistema.Art).Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
        var pendentes = art is { } sistemaDoArt
            ? (await banco.RegistrosDeOrigem.AsNoTracking()
                    .Where(r => r.SistemaId == sistemaDoArt && r.Fluxo == MetaDeVenda.FluxoDasVendasDoArt && r.VendaDeMaquinaId == null
                                && r.AusenteNaOrigemDesde == null && r.ChassiNaOrigem != null)
                    .Select(r => new { r.ChassiNaOrigem, r.Motivos })
                    .ToListAsync(ct))
                .Where(r => Chassi(r.ChassiNaOrigem) is not null)
                .GroupBy(r => Chassi(r.ChassiNaOrigem)!, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First().Motivos, StringComparer.Ordinal)
            : new Dictionary<string, string?>(StringComparer.Ordinal);

        string Onde(int empresaId, DateOnly mes) =>
            $"{nomeDaEmpresa.GetValueOrDefault(empresaId, $"filial {empresaId}")} · {mes.ToString("yyyy-MM", CultureInfo.InvariantCulture)}";

        // ---- chassi a chassi: o realizado da GN ----
        foreach (var (chassi, gn) in realizadoDaGestao)
        {
            var naOrigem = Onde(gn.EmpresaId, gn.Mes);
            if (vendaPorChassi.TryGetValue(chassi, out var venda))
            {
                if (venda.EntregueEm is not { } entregue)
                {
                    plano.Divergencias.Add(new Divergencia(TipoDeDivergencia.RealizadoNaoEntregueNoCrm, chassi, gn.EmpresaId, venda.EquipamentoId, venda.Id,
                        "A GN conta a máquina como entregue; no CRM a venda ainda não tem a data de entrega.", "venda sem entrega", naOrigem));
                    continue;
                }

                var mesNoCrm = new DateOnly(entregue.Year, entregue.Month, 1);
                if (venda.EmpresaId != gn.EmpresaId)
                    plano.Divergencias.Add(new Divergencia(TipoDeDivergencia.RealizadoEmOutraFilial, chassi, gn.EmpresaId, venda.EquipamentoId, venda.Id,
                        "A máquina conta numa filial na GN e em outra no CRM.", Onde(venda.EmpresaId, mesNoCrm), naOrigem));
                else if (mesNoCrm != gn.Mes)
                    plano.Divergencias.Add(new Divergencia(TipoDeDivergencia.RealizadoEmOutroMes, chassi, gn.EmpresaId, venda.EquipamentoId, venda.Id,
                        "A máquina conta num mês na GN e em outro no CRM (a data de entrega difere).", Onde(venda.EmpresaId, mesNoCrm), naOrigem));
                else
                    plano.Batem++;
                continue;
            }

            if (pendentes.TryGetValue(chassi, out var motivos))
                plano.Divergencias.Add(new Divergencia(TipoDeDivergencia.RealizadoPendenteNoArt, chassi, gn.EmpresaId, null, null,
                    "A venda está no ART, mas pendente na integração do CRM.", Limitar($"pendente: {motivos ?? "sem motivo registrado"}", 200), naOrigem));
            else
                plano.Divergencias.Add(new Divergencia(TipoDeDivergencia.RealizadoSoNaGestao, chassi, gn.EmpresaId, null, null,
                    "A GN conta a máquina, e o CRM não tem venda nem registro do ART com este chassi.", null, naOrigem));
        }

        // ---- o que só o CRM conta, nos mesmos meses ----
        foreach (var venda in vendaPorChassi.Values.Where(v => v.EntregueEm is { } e && mesesDoRealizado.Contains(new DateOnly(e.Year, e.Month, 1))))
        {
            var mes = new DateOnly(venda.EntregueEm!.Value.Year, venda.EntregueEm.Value.Month, 1);
            plano.RealizadoNoCrm++;
            Somar(plano.RealizadoDoCrm, (venda.EmpresaId, mes), 1);
            if (!realizadoDaGestao.ContainsKey(venda.Chassi.Numero))
                plano.Divergencias.Add(new Divergencia(TipoDeDivergencia.RealizadoSoNoCrm, venda.Chassi.Numero, venda.EmpresaId, venda.EquipamentoId, venda.Id,
                    "O CRM conta a máquina como entregue no período, e a GN não.", Onde(venda.EmpresaId, mes), null));
        }

        // ---- a meta do CRM, sem consórcio, nos mesmos meses ----
        var metasDoCrm = (await banco.MetasDeVenda.IgnoreQueryFilters().AsNoTracking()
                .Where(m => m.ExcluidoEm == null && m.Competencia >= inicio && m.Competencia < fim)
                .Select(m => new { m.EmpresaId, m.Competencia, m.Origem, m.CodigoDaLinha, m.Quantidade })
                .ToListAsync(ct))
            .Where(m => !MetaDeVenda.EhConsorcio(m.Origem, m.CodigoDaLinha))
            .GroupBy(m => (m.EmpresaId, m.Competencia))
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Quantidade));

        // ---- o resumo: toda filial e mês que aparece de um lado ou do outro ----
        var chaves = resumoDaGestao.Keys
            .Concat(plano.RealizadoDoCrm.Keys.Select(k => (ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, k.EmpresaId, k.Mes)))
            .Concat(metasDoCrm.Keys.Where(k => meses.Contains(k.Competencia)).Select(k => (ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, k.EmpresaId, k.Competencia)))
            .Distinct();
        foreach (var (indicador, empresa, mes) in chaves)
        {
            var noCrm = indicador == ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas
                ? metasDoCrm.GetValueOrDefault((empresa, mes))
                : plano.RealizadoDoCrm.GetValueOrDefault((empresa, mes));
            plano.Resumo.Add((indicador, empresa, mes, resumoDaGestao.GetValueOrDefault((indicador, empresa, mes)), noCrm));
        }

        return plano;
    }

    // =============================================================================================
    // A gravação — uma transação
    // =============================================================================================

    private async Task AplicarAsync(Plano plano, LeituraDaConferenciaNaOrigem origem, DateTime agora, CancellationToken ct)
    {
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, LeitorDeMetasDaGestaoDeNegocios.CodigoDoSistema, "Gestão de Negócios — API", "API REST com chave, só leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        // O RESUMO É REGRAVADO INTEIRO: é a apuração do dia.
        await banco.ConferenciasDaGestaoDeNegocios.IgnoreQueryFilters().Where(c => c.SistemaId == sistemaId).ExecuteDeleteAsync(ct);
        foreach (var (indicador, empresa, mes, naGestao, noCrm) in plano.Resumo)
            banco.ConferenciasDaGestaoDeNegocios.Add(ConferenciaDaGestaoDeNegocios.Apurar(sistemaId, empresa, indicador, mes, naGestao, noCrm, agora, usuarioId));

        // AS DIVERGÊNCIAS TÊM CICLO DE VIDA: a nova entra, a que continua é reconfirmada, a que sumiu deixa de ocorrer.
        var tipos = TiposDaConferencia.ToList();
        var existentes = (await banco.DivergenciasDeIntegracao.IgnoreQueryFilters()
                .Where(d => d.SistemaId == sistemaId && tipos.Contains(d.Tipo))
                .ToListAsync(ct))
            .ToDictionary(d => (d.Tipo, d.ChaveOrigem));
        var vistas = new HashSet<(TipoDeDivergencia, string)>();
        var novas = 0;
        foreach (var d in plano.Divergencias)
        {
            vistas.Add((d.Tipo, d.Chassi));
            if (existentes.TryGetValue((d.Tipo, d.Chassi), out var existente))
            {
                existente.Reconfirmar(d.Descricao, d.ValorNoCrm, d.ValorNaGestao, null, agora, usuarioId);
                continue;
            }

            banco.DivergenciasDeIntegracao.Add(DivergenciaDeIntegracao.Registrar(
                d.EmpresaId, sistemaId, d.Tipo, d.Chassi, d.EquipamentoId, d.VendaDeMaquinaId, d.Descricao, d.ValorNoCrm, d.ValorNaGestao, null, agora,
                usuarioId));
            novas++;
        }

        var encerradas = 0;
        foreach (var ((tipo, chave), existente) in existentes)
        {
            if (vistas.Contains((tipo, chave)) || existente.Situacao != SituacaoDaDivergencia.Aberta) continue;
            existente.MarcarQueDeixouDeOcorrer(agora, usuarioId);
            encerradas++;
        }

        await banco.SaveChangesAsync(ct);
        Contar(RotuloNovas, novas);
        Contar(RotuloEncerradas, encerradas);

        var gerada = (origem.GeradaEmUtc ?? agora).ToString("O", CultureInfo.InvariantCulture);
        await CargaDeTerritorio.RegistrarRodadaAsync(banco, sistemaId, ConferenciaDaGestaoDeNegocios.FluxoDaCarga,
            origem.Realizado.Count + origem.Meta.Count, plano.Resumo.Count + plano.Divergencias.Count, plano.SemFilial, ct, gerada);

        await transacao.CommitAsync(ct);
        relatar("Gravado.");
    }

    // =============================================================================================
    // Apoio
    // =============================================================================================

    private void Contar(string rotulo, int valor)
    {
        _contagens.Add((rotulo, valor));
        relatar($"  {rotulo}: {valor:N0}");
    }

    private static async Task<bool> EsquemaExisteAsync(CrmDbContext banco, CancellationToken ct) =>
        !banco.Database.IsSqlServer()
        || await banco.Database
            .SqlQueryRaw<int>("SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE object_id = OBJECT_ID(N'integracao.ConferenciaDaGestaoDeNegocios')")
            .SingleAsync(ct) > 0;

    /// <summary>O chassi normalizado (sem espaço, em maiúsculas); nulo quando vazio.</summary>
    internal static string? Chassi(string? bruto) => string.IsNullOrWhiteSpace(bruto) ? null : bruto.Trim().ToUpperInvariant();

    private static int Quantidade(string? bruto) =>
        decimal.TryParse(bruto, NumberStyles.Number, CultureInfo.InvariantCulture, out var q) && q > 0 ? (int)decimal.Round(q) : 1;

    private static void Somar<TChave>(Dictionary<TChave, int> soma, TChave chave, int valor) where TChave : notnull =>
        soma[chave] = soma.GetValueOrDefault(chave) + valor;

    private static string Limitar(string texto, int tamanho) => texto.Length <= tamanho ? texto : texto[..tamanho];

    private sealed record Divergencia(
        TipoDeDivergencia Tipo, string Chassi, int EmpresaId, long? EquipamentoId, long? VendaDeMaquinaId, string Descricao, string? ValorNoCrm,
        string? ValorNaGestao);

    private sealed class Plano
    {
        public bool EsquemaExiste { get; init; }
        public int RealizadoNaGestao { get; set; }
        public int RealizadoNoCrm { get; set; }
        public int Batem { get; set; }
        public int SemFilial { get; set; }
        public List<Divergencia> Divergencias { get; } = [];
        public Dictionary<(int EmpresaId, DateOnly Mes), int> RealizadoDoCrm { get; } = [];
        public List<(string Indicador, int EmpresaId, DateOnly Mes, int NaGestao, int NoCrm)> Resumo { get; } = [];
    }
}
