using Microsoft.EntityFrameworkCore;
using Tracbel.Crm.Dominio.Auditoria;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Infraestrutura.Persistencia;
using Tracbel.Crm.Integracao.Art;

namespace Tracbel.Crm.Carga;

/// <summary>O que a carga do preço de máquina fez, contado.</summary>
/// <param name="Simulada">Se nada foi gravado.</param>
/// <param name="Contagens">Cada número, com o rótulo que o relatório imprime.</param>
internal sealed record RelatorioDoPrecoDeMaquina(bool Simulada, IReadOnlyList<(string Rotulo, int Valor)> Contagens)
{
    /// <summary>O número de um rótulo; zero quando não houve.</summary>
    /// <param name="rotulo">O rótulo.</param>
    public int Valor(string rotulo) => Contagens.FirstOrDefault(c => c.Rotulo == rotulo).Valor;
}

/// <summary>
/// A CARGA DO PREÇO DE REFERÊNCIA DA MÁQUINA POR CATEGORIA (issue 70, D-P12, decidida pelo Ricardo em 27/09/2026) —
/// a rotina <c>PRECOS_DE_MAQUINA</c>.
///
/// <para><b>Duas fontes que o CRM já alcança.</b> A venda de máquina do ART, que o CRM carrega todo dia
/// (<c>ART_VENDAS</c>), diz a filial que faturou, o número da nota e a linha da máquina; o item da nota de saída do
/// Protheus (<c>SD2</c>, grupo <c>VEIC</c>) diz quanto foi faturado. O casamento é pela filial e pelo número, com
/// a emissão perto da data da venda (<see cref="PrecoDaMaquinaPelaNota"/>).</para>
///
/// <para><b>O que fica de fora, e por quê</b> — tudo contado no relatório:</para>
/// <list type="bullet">
///   <item>venda direta e repasse direto: quem fatura é a fábrica, e a nota não é da Tracbel;</item>
///   <item>linha "USADOS" e linha sem categoria: usado não é preço de máquina nova, e sem categoria não há onde
///   pôr o preço;</item>
///   <item>nota sem item de máquina, com dois itens, ou emitida longe da data da venda.</item>
/// </list>
///
/// <para><b>Grava só o agregado</b> — mediana, menor, maior e quantas notas por categoria e mês
/// (<see cref="PrecoDeMaquinaNoMes"/>). O valor de cada venda fica no ERP.</para>
///
/// <para><b>Reexecutável.</b> Relê tudo desde a primeira venda do ART e grava só o mês que mudou; nada é apagado.</para>
/// </summary>
/// <param name="abrirContexto">Abre um contexto de banco com alcance de sistema.</param>
/// <param name="lerItens">A leitura dos itens de máquina das notas do Protheus, a partir de uma data.</param>
/// <param name="usuarioId">Quem roda a carga.</param>
/// <param name="relogio">O instante da carga (UTC).</param>
/// <param name="relatar">Onde a carga escreve o andamento.</param>
internal sealed class CargaDePrecoDeMaquina(
    Func<CrmDbContext> abrirContexto,
    Func<DateOnly, CancellationToken, Task<Resultado<IReadOnlyList<ItemDeMaquinaNaNota>>>> lerItens,
    long usuarioId,
    Func<DateTime> relogio,
    Action<string> relatar)
{
    /// <summary>O fluxo da trava e do registro de rodada.</summary>
    public const string Fluxo = "PROTHEUS.PRECO_DE_MAQUINA";

    /// <summary>Rótulo: vendas do ART lidas.</summary>
    public const string VendasLidas = "vendas do ART lidas";

    /// <summary>Rótulo: vendas diretas e repasses, fora porque a nota não é da Tracbel.</summary>
    public const string VendasDiretas = "venda direta ou repasse (a nota é da fábrica)";

    /// <summary>Rótulo: itens de máquina lidos da SD2.</summary>
    public const string ItensLidos = "itens de máquina lidos da SD2";

    /// <summary>Rótulo: meses gravados pela primeira vez.</summary>
    public const string MesesNovos = "meses novos";

    /// <summary>Rótulo: meses cujo preço mudou.</summary>
    public const string MesesRevisados = "meses revisados";

    /// <summary>Rótulo: meses iguais, não tocados.</summary>
    public const string MesesMantidos = "meses sem mudança";

    /// <summary>O rótulo de cada desfecho do casamento, na ordem do relatório.</summary>
    public static readonly IReadOnlyDictionary<DesfechoDoCasamento, string> RotuloDoDesfecho = new Dictionary<DesfechoDoCasamento, string>
    {
        [DesfechoDoCasamento.Casada] = "vendas casadas com a nota (entram no preço)",
        [DesfechoDoCasamento.SemCategoria] = "usado ou linha sem categoria",
        [DesfechoDoCasamento.SemNota] = "venda sem número de nota ou sem filial que faturou",
        [DesfechoDoCasamento.SemData] = "venda sem data para conferir",
        [DesfechoDoCasamento.SemItemNaNota] = "nota sem item de máquina de venda na SD2",
        [DesfechoDoCasamento.Ambigua] = "nota com mais de um item de máquina possível",
        [DesfechoDoCasamento.ForaDaData] = "nota emitida longe da data da venda"
    };

    /// <summary>
    /// A PRIMEIRA EMISSÃO LIDA: dois meses antes da venda mais antiga, para a nota emitida antes de o ART registrar
    /// a venda também casar. Sem venda nenhuma, não há o que ler.
    /// </summary>
    private const int MesesAntesDaPrimeiraVenda = 2;

    /// <summary>Executa a carga.</summary>
    /// <param name="simular">Lê e casa tudo, conta, e não grava nada.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<RelatorioDoPrecoDeMaquina>> ExecutarAsync(bool simular, CancellationToken ct)
    {
        var contagens = new List<(string, int)>();

        // ---- 1. as vendas do ART, com a categoria da LINHA DA VENDA ----
        List<VendaComNota> vendas;
        int diretas;
        await using (var contexto = abrirContexto())
        {
            var empresas = await contexto.Empresas.AsNoTracking().ToDictionaryAsync(e => e.Id, e => e.Codigo, ct);

            // A CATEGORIA DA LINHA É CRUZADA EM MEMÓRIA: `CodigoDaLinha` tem colação binária e o resto do banco não,
            // e um JOIN entre as duas colunas falha no SQL Server.
            var categoriaDaLinha = (await contexto.LinhasDeProdutoNasCategorias.AsNoTracking()
                    .Select(l => new { l.CodigoDaLinha, l.CategoriaDeMaquinaId })
                    .ToListAsync(ct))
                .GroupBy(l => l.CodigoDaLinha, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First().CategoriaDeMaquinaId, StringComparer.Ordinal);

            var lidas = await contexto.VendasDeMaquina.AsNoTracking()
                .Where(v => v.ExcluidoEm == null)
                .Select(v => new
                {
                    v.EmpresaDoFaturamentoId,
                    v.NumeroDaNotaFiscal,
                    v.FaturadaEm,
                    v.VendidaEm,
                    v.LinhaNaOrigem,
                    v.VendaDireta,
                    v.RepasseDireto
                })
                .ToListAsync(ct);

            contagens.Add((VendasLidas, lidas.Count));
            diretas = lidas.Count(v => v.VendaDireta || v.RepasseDireto);
            contagens.Add((VendasDiretas, diretas));

            // O USADO NÃO É CLASSIFICADO: "USADOS" é condição da venda, e o preço de um usado não é preço de máquina
            // nova. A categoria sai da linha da venda — e não do equipamento, que guarda a da primeira venda.
            int? Categoria(string linhaNaOrigem) =>
                ClassificacaoDoArt.ClassificarLinha(linhaNaOrigem).Destino is { } linha
                && categoriaDaLinha.TryGetValue(linha, out var categoria)
                    ? categoria
                    : null;

            vendas =
            [
                .. lidas
                    .Where(v => !v.VendaDireta && !v.RepasseDireto)
                    .Select(v => new VendaComNota(
                        v.EmpresaDoFaturamentoId is { } empresa ? empresas.GetValueOrDefault(empresa) : null,
                        v.NumeroDaNotaFiscal,
                        v.FaturadaEm ?? v.VendidaEm,
                        Categoria(v.LinhaNaOrigem)))
            ];
        }

        var primeira = vendas.Where(v => v.Data is not null).Select(v => v.Data!.Value).DefaultIfEmpty().Min();
        if (primeira == default)
            return Resultado<RelatorioDoPrecoDeMaquina>.Indisponivel(
                "Nenhuma venda do ART com data está no CRM — sem venda não há preço de máquina para casar. Nada foi gravado.");

        var desde = new DateOnly(primeira.Year, primeira.Month, 1).AddMonths(-MesesAntesDaPrimeiraVenda);

        // ---- 2. os itens de máquina das notas do Protheus ----
        relatar($"  lendo os itens de máquina das notas de venda emitidas desde {desde:dd/MM/yyyy} (SD2, grupo VEIC)…");
        var itens = await lerItens(desde, ct);
        if (!itens.EhSucesso) return Resultado<RelatorioDoPrecoDeMaquina>.Indisponivel(itens.Erro!);
        contagens.Add((ItensLidos, itens.Valor.Count));

        // ---- 3. o casamento e a mediana — domínio puro ----
        var (precos, desfechos) = PrecoDaMaquinaPelaNota.Casar(vendas, itens.Valor);
        foreach (var (desfecho, rotulo) in RotuloDoDesfecho)
            contagens.Add((rotulo, desfechos.GetValueOrDefault(desfecho)));

        var porMes = PrecoDaMaquinaPelaNota.PorMes(precos);
        relatar($"  {precos.Count} vendas casadas viram {porMes.Count} meses de preço em " +
                $"{porMes.Select(p => p.CategoriaDeMaquinaId).Distinct().Count()} categorias.");

        if (simular)
            return Resultado<RelatorioDoPrecoDeMaquina>.Ok(new RelatorioDoPrecoDeMaquina(true, contagens));

        // ---- 4. a gravação, numa transação ----
        var agora = relogio();
        await using var banco = abrirContexto();
        await using var transacao = await banco.Database.BeginTransactionAsync(ct);

        var sistemaId = await CargaDeTerritorio.SistemaAsync(
            banco, "PROTHEUS", "Protheus — banco (leitura)", "Banco do ERP, somente leitura", ct);
        banco.DeclararOrigemDasGravacoes(OrigemDaOperacao.Integracao, sistemaId);

        var existentes = (await banco.PrecosDeMaquina.ToListAsync(ct))
            .ToDictionary(p => (p.CategoriaDeMaquinaId, p.Mes));

        int novos = 0, revisados = 0, mantidos = 0;
        foreach (var p in porMes)
        {
            if (existentes.TryGetValue((p.CategoriaDeMaquinaId, p.Mes), out var existente))
            {
                if (existente.Revisar(p.Mediana, p.Menor, p.Maior, p.Notas, usuarioId, agora)) revisados++;
                else mantidos++;
            }
            else
            {
                banco.PrecosDeMaquina.Add(PrecoDeMaquinaNoMes.Registrar(
                    p.CategoriaDeMaquinaId, p.Mes, p.Mediana, p.Menor, p.Maior, p.Notas,
                    PrecoDeMaquinaNoMes.FonteDaNota, usuarioId, agora));
                novos++;
            }
        }

        await banco.SaveChangesAsync(ct);
        await CargaDeTerritorio.RegistrarRodadaAsync(
            banco, sistemaId, Fluxo, itens.Valor.Count, novos + revisados, 0, ct,
            porMes.Count == 0 ? null : $"{porMes.Min(p => p.Mes):MM/yyyy} a {porMes.Max(p => p.Mes):MM/yyyy}");
        await transacao.CommitAsync(ct);

        contagens.Add((MesesNovos, novos));
        contagens.Add((MesesRevisados, revisados));
        contagens.Add((MesesMantidos, mantidos));

        return Resultado<RelatorioDoPrecoDeMaquina>.Ok(new RelatorioDoPrecoDeMaquina(false, contagens));
    }
}
