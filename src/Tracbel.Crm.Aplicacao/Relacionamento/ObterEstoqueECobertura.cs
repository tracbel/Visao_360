using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>Os totais do estoque.</summary>
/// <param name="NoPatio">As máquinas no pátio: estoque, remessa, consignado e em transferência.</param>
/// <param name="Disponiveis">As do pátio sem reserva.</param>
/// <param name="Reservadas">As do pátio reservadas para uma venda.</param>
/// <param name="Pagas">As do pátio já pagas à fábrica.</param>
/// <param name="MaisDe180Dias">As do pátio com mais de 180 dias de entrada.</param>
/// <param name="PedidosAFabrica">Os pedidos à fábrica, ainda sem entrada.</param>
public sealed record TotaisDoEstoque(int NoPatio, int Disponiveis, int Reservadas, int Pagas, int MaisDe180Dias, int PedidosAFabrica);

/// <summary>Um grupo de máquina.</summary>
/// <param name="Grupo">O grupo, como o TOTVS escreve.</param>
/// <param name="NoPatio">No pátio.</param>
/// <param name="Disponiveis">No pátio sem reserva.</param>
/// <param name="Reservadas">Reservadas.</param>
/// <param name="PedidosAFabrica">Pedidos à fábrica.</param>
/// <param name="IdadeMediaEmDias">A idade média no pátio, em dias; nula sem data de entrada.</param>
/// <param name="CoberturaEmMeses">A cobertura do grupo (da organização inteira); nula quando a GN não a calcula.</param>
public sealed record GrupoNoEstoque(
    string Grupo, int NoPatio, int Disponiveis, int Reservadas, int PedidosAFabrica, int? IdadeMediaEmDias, decimal? CoberturaEmMeses);

/// <summary>Uma máquina na lista, com a idade calculada.</summary>
public sealed record MaquinaNaLista(
    string Filial, string Grupo, string Descricao, string? Configuracao, string Situacao, string Tipo, bool EhUsado, string? AnoModelo,
    string? Chassi, DateOnly? EntradaEm, int? DiasNoPatio, DateOnly? ChegadaPrevistaEm, DateOnly? FaturamentoPrevistoEm, bool Pago,
    bool Reservado, bool EhPedidoAFabrica, string? SituacaoNaFabrica);

/// <summary>A cobertura: por mês e por grupo, com as médias como a GN as mostra.</summary>
public sealed record CoberturaDoEstoqueNaTela(
    IReadOnlyList<ItemDaCoberturaDoEstoque> PorMes,
    IReadOnlyList<ItemDaCoberturaDoEstoque> PorGrupo,
    decimal? MediaPorMes,
    decimal? MediaPorGrupo,
    DateTime? LidaEm,
    DateTime? GeradaNaOrigemEm);

/// <summary>O estoque e a cobertura — o que <c>GET /api/v1/relatorios/estoque</c> devolve.</summary>
public sealed record EstoqueECobertura(
    string Alcance,
    DateOnly Hoje,
    TotaisDoEstoque Totais,
    IReadOnlyList<GrupoNoEstoque> PorGrupo,
    IReadOnlyList<MaquinaNaLista> Maquinas,
    CoberturaDoEstoqueNaTela Cobertura,
    DateTime? LidoEm,
    DateTime? GeradoNaOrigemEm,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// O ESTOQUE E A COBERTURA (decisão do Ricardo em 28/09/2026) — a disponibilidade de máquina para a venda, pela filial
/// escolhida (ou por todas, em "Todas as filiais"), e quantos meses o estoque dura no ritmo de venda.
///
/// <para><b>Pede <c>Relatorio.Ler</c></b>, como o funil: o vendedor precisa saber se tem máquina para vender. Não há custo
/// nem cliente na resposta — o CRM não os lê.</para>
///
/// <para><b>Os dias no pátio são de hoje</b> (São Paulo), da data de entrada — não se gravam, porque mudariam todo dia.</para>
/// </summary>
/// <param name="repositorio">Onde o estoque é lido.</param>
/// <param name="acesso">Quem pergunta.</param>
/// <param name="relogio">O relógio.</param>
public sealed class ObterEstoqueECobertura(IRepositorioDoEstoque repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>A idade a partir da qual a máquina parada é destaque — a faixa mais velha do painel da GN.</summary>
    public const int DiasDeEstoqueVelho = 180;

    private static readonly CultureInfo Portugues = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Executa a leitura.</summary>
    public async Task<Resultado<ComProcedencia<EstoqueECobertura>>> ExecutarAsync(CancellationToken ct)
    {
        var lido = await repositorio.LerAsync(ct);
        var hoje = DateOnly.FromDateTime(relogio.Agora.AddHours(-3));
        var organizacao = acesso.Atual.VeTodasAsFiliais;

        var resposta = Montar(lido, hoje, organizacao);
        return Resultado<ComProcedencia<EstoqueECobertura>>.Ok(
            ComProcedencia<EstoqueECobertura>.DoNossoBanco(
                resposta,
                "frota.EquipamentoEmEstoque · frota.CoberturaDoEstoque (API Gestão de Negócios, do TOTVS)",
                relogio));
    }

    /// <summary>A conta, sem banco: os totais, os grupos, a lista com a idade e as médias da cobertura.</summary>
    public static EstoqueECobertura Montar(EstoqueLido lido, DateOnly hoje, bool organizacao)
    {
        int? Dias(MaquinaNoEstoque m) =>
            EquipamentoEmEstoque.EhPedido(m.Situacao) || m.EntradaEm is not { } entrada ? null : Math.Max(0, hoje.DayNumber - entrada.DayNumber);

        var lista = lido.Maquinas
            .Select(m => new MaquinaNaLista(
                m.Filial, m.Grupo, m.Descricao, m.Configuracao, m.Situacao, m.Tipo, m.EhUsado, m.AnoModelo, m.Chassi, m.EntradaEm, Dias(m),
                m.ChegadaPrevistaEm, m.FaturamentoPrevistoEm, m.Pago, m.Reservado, EquipamentoEmEstoque.EhPedido(m.Situacao), m.SituacaoNaFabrica))
            .OrderBy(m => m.EhPedidoAFabrica)
            .ThenBy(m => m.Grupo, StringComparer.Create(Portugues, ignoreCase: true))
            .ThenBy(m => m.Descricao, StringComparer.Create(Portugues, ignoreCase: true))
            .ThenByDescending(m => m.DiasNoPatio ?? -1)
            .ToList();

        var patio = lista.Where(m => !m.EhPedidoAFabrica).ToList();
        var totais = new TotaisDoEstoque(
            patio.Count,
            patio.Count(m => !m.Reservado),
            patio.Count(m => m.Reservado),
            patio.Count(m => m.Pago),
            patio.Count(m => m.DiasNoPatio > DiasDeEstoqueVelho),
            lista.Count(m => m.EhPedidoAFabrica));

        var coberturaDoGrupo = lido.CoberturaPorGrupo
            .GroupBy(c => CodigoEstavel.De(c.Chave), StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Meses, StringComparer.Ordinal);

        var grupos = lista
            .GroupBy(m => m.Grupo, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var noPatio = g.Where(m => !m.EhPedidoAFabrica).ToList();
                var comIdade = noPatio.Where(m => m.DiasNoPatio is not null).ToList();
                return new GrupoNoEstoque(
                    g.Key,
                    noPatio.Count,
                    noPatio.Count(m => !m.Reservado),
                    noPatio.Count(m => m.Reservado),
                    g.Count(m => m.EhPedidoAFabrica),
                    comIdade.Count == 0 ? null : (int)Math.Round(comIdade.Average(m => m.DiasNoPatio!.Value)),
                    coberturaDoGrupo.TryGetValue(CodigoEstavel.De(g.Key), out var meses) ? meses : null);
            })
            .OrderByDescending(g => g.NoPatio)
            .ThenBy(g => g.Grupo, StringComparer.Create(Portugues, ignoreCase: true))
            .ToList();

        static decimal? Media(IReadOnlyList<ItemDaCoberturaDoEstoque> itens) =>
            itens.Count == 0 ? null : decimal.Round(itens.Average(i => i.Meses), 2);

        var cobertura = new CoberturaDoEstoqueNaTela(
            lido.CoberturaPorMes, lido.CoberturaPorGrupo, Media(lido.CoberturaPorMes), Media(lido.CoberturaPorGrupo),
            lido.CoberturaLidaEm, lido.CoberturaGeradaNaOrigemEm);

        return new EstoqueECobertura(
            organizacao ? "Organizacao" : "Filiais",
            hoje,
            totais,
            grupos,
            lista,
            cobertura,
            lido.EstoqueLidoEm,
            lido.EstoqueGeradoNaOrigemEm,
            Lacunas(lido, organizacao));
    }

    private static List<MetricaSemDado> Lacunas(EstoqueLido lido, bool organizacao)
    {
        var lacunas = new List<MetricaSemDado>();

        if (lido.EstoqueLidoEm is null)
            lacunas.Add(new MetricaSemDado("estoqueNaoLido",
                "O estoque da API Gestão de Negócios ainda não foi lido: ele vem da rotina \"Estoque e cobertura (Gestão de Negócios)\". " +
                "Sem a leitura, a lista fica vazia — e não quer dizer que não há máquina."));

        if (lido.CoberturaLidaEm is null)
            lacunas.Add(new MetricaSemDado("coberturaNaoLida",
                "A cobertura em meses ainda não foi lida: ela vem da mesma rotina do estoque."));
        else if (!organizacao)
            lacunas.Add(new MetricaSemDado("coberturaDaOrganizacao",
                "A cobertura em meses é da empresa inteira: a GN a calcula com o estoque e as vendas de todas as filiais, e não da filial escolhida."));

        lacunas.Add(new MetricaSemDado("valor",
            "O valor do estoque não aparece: está a custo no TOTVS, e o CRM não lê custo, como não lê o lucro e a margem do ART."));

        return lacunas;
    }
}
