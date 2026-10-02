using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Equipamentos;

/// <summary>
/// As ordens de serviço de um cliente ou de uma máquina, resumidas para a ficha — os mesmos recortes do painel de
/// pós-venda do BI: em aberto (aberta ou liberada, o "empenhado"), há mais de 45 dias, e os doze meses.
/// </summary>
/// <param name="EmAberto">As OS ainda na oficina.</param>
/// <param name="EmAbertoHaMaisDe45Dias">As que estão abertas há mais de 45 dias — a faixa vermelha do BI.</param>
/// <param name="DiasDaMaisAntigaEmAberto">Há quantos dias a mais antiga ainda aberta foi aberta.</param>
/// <param name="ValorEmAberto">Peças e serviços das OS ainda na oficina.</param>
/// <param name="NosUltimos12Meses">As OS fechadas abertas nos últimos doze meses.</param>
/// <param name="PecasNosUltimos12Meses">As peças dessas OS.</param>
/// <param name="ServicosNosUltimos12Meses">Os serviços dessas OS.</param>
/// <param name="UltimaAbertaEm">A abertura mais recente, de qualquer situação (menos cancelada).</param>
/// <param name="TotalDeOrdens">Quantas OS há, ao todo — a lista traz no máximo <see cref="ListarOrdensDeServico.MaximoNaLista"/>.</param>
/// <param name="Ordens">As OS: primeiro as ainda abertas, da mais antiga para a mais nova; depois as outras, da mais recente.</param>
/// <param name="CarregadoEm">A última rodada da carga (UTC).</param>
/// <param name="MetricasSemDado">O que a ficha pediu e não pôde mostrar, com o motivo.</param>
public sealed record OrdensDeServicoResumidas(
    int EmAberto,
    int EmAbertoHaMaisDe45Dias,
    int? DiasDaMaisAntigaEmAberto,
    decimal ValorEmAberto,
    int NosUltimos12Meses,
    decimal PecasNosUltimos12Meses,
    decimal ServicosNosUltimos12Meses,
    DateOnly? UltimaAbertaEm,
    int TotalDeOrdens,
    IReadOnlyList<OrdemDeServicoResumida> Ordens,
    DateTime? CarregadoEm,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>Uma OS da lista da ficha, com os dias em aberto já contados.</summary>
/// <param name="Ordem">A OS.</param>
/// <param name="DiasEmAberto">Há quantos dias está aberta; nulo para a fechada ou cancelada.</param>
public sealed record OrdemDeServicoResumida(OrdemDeServicoParaConsulta Ordem, int? DiasEmAberto);

/// <summary>
/// AS ORDENS DE SERVIÇO NA FICHA (02/10/2026) — do cliente ou da máquina, lidas da tabela que a rotina 15
/// <c>POS_VENDA_PROTHEUS</c> mantém a partir das views do BI.
///
/// <para><b>Esta consulta troca uma frase.</b> A ficha do cliente dizia "o CRM ainda não carrega as ordens de serviço da
/// oficina". Com a rotina desligada ou sem nenhuma rodada, a frase continua — agora dizendo o porquê real.</para>
///
/// <para><b>Os doze meses ancoram em hoje</b>, e contam a OS fechada pela abertura, como o painel do BI conta "F e C"
/// pelo período da abertura. A cancelada não entra em valor nenhum.</para>
/// </summary>
/// <param name="repositorio">O acesso às OS.</param>
/// <param name="relogio">O relógio, para os dias em aberto e a procedência.</param>
public sealed class ListarOrdensDeServico(IRepositorioDeOrdensDeServico repositorio, IRelogio relogio)
{
    /// <summary>Quantas OS a lista traz no máximo — os números do resumo contam todas.</summary>
    public const int MaximoNaLista = 100;

    /// <summary>A faixa vermelha do painel de pós-venda: aberta há mais de 45 dias.</summary>
    public const int DiasDaFaixaVermelha = 45;

    /// <summary>As OS de um cliente.</summary>
    /// <param name="chave">O GUID público do cliente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<OrdensDeServicoResumidas>>> DoClienteAsync(Guid chave, CancellationToken ct)
    {
        var lidas = await repositorio.DoClienteAsync(chave, ct);
        return lidas is null
            ? Resultado<ComProcedencia<OrdensDeServicoResumidas>>.NaoEncontrado($"Não há cliente {chave} ao seu alcance.")
            : Resumir(lidas, "deste cliente",
                "A OS entra na ficha do cliente quando o CPF/CNPJ do proprietário, na OS do Protheus, é o de um cliente do CRM.");
    }

    /// <summary>As OS de uma máquina.</summary>
    /// <param name="chave">O GUID público da máquina.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<OrdensDeServicoResumidas>>> DoEquipamentoAsync(Guid chave, CancellationToken ct)
    {
        var lidas = await repositorio.DoEquipamentoAsync(chave, ct);
        return lidas is null
            ? Resultado<ComProcedencia<OrdensDeServicoResumidas>>.NaoEncontrado($"Não há equipamento {chave} ao seu alcance.")
            : Resumir(lidas, "desta máquina", "A OS entra na ficha da máquina quando o chassi da OS do Protheus é o desta máquina.");
    }

    private Resultado<ComProcedencia<OrdensDeServicoResumidas>> Resumir(OrdensDeServicoLidas lidas, string deQuem, string comoCasa)
    {
        var hoje = DateOnly.FromDateTime(relogio.Agora);
        var inicioDos12Meses = hoje.AddMonths(-12);
        var ausentes = new List<MetricaSemDado>();

        if (lidas.CarregadoEm is null)
            ausentes.Add(new MetricaSemDado(
                "ordensDeServico",
                "As ordens de serviço ainda não foram carregadas: elas vêm das views do BI no banco do Protheus, pela rotina " +
                "POS_VENDA_PROTHEUS, que nasce desligada — sem nenhuma rodada dela, não há o que mostrar."));
        else if (lidas.Ordens.Count == 0)
            ausentes.Add(new MetricaSemDado(
                "ordensDeServicoDoRecorte",
                $"Nenhuma ordem de serviço {deQuem} nos últimos três anos, nas filiais ao seu alcance. {comoCasa}"));

        static bool Aberta(OrdemDeServicoParaConsulta o) =>
            Enum.TryParse<SituacaoDaOrdemDeServico>(o.Situacao, out var s) && OrdemDeServico.EstaEmAbertoNa(s);
        int? Dias(OrdemDeServicoParaConsulta o) => Aberta(o) ? Math.Max(0, hoje.DayNumber - o.AbertaEm.DayNumber) : null;

        var abertas = lidas.Ordens.Where(Aberta).ToList();
        var fechadas12 = lidas.Ordens
            .Where(o => o.Situacao == nameof(SituacaoDaOrdemDeServico.Fechada) && o.AbertaEm >= inicioDos12Meses)
            .ToList();
        var naoCanceladas = lidas.Ordens.Where(o => o.Situacao != nameof(SituacaoDaOrdemDeServico.Cancelada)).ToList();

        var lista = abertas.OrderBy(o => o.AbertaEm).ThenBy(o => o.Numero, StringComparer.Ordinal)
            .Concat(lidas.Ordens.Where(o => !Aberta(o)).OrderByDescending(o => o.AbertaEm).ThenByDescending(o => o.Numero, StringComparer.Ordinal))
            .Take(MaximoNaLista)
            .Select(o => new OrdemDeServicoResumida(o, Dias(o)))
            .ToList();

        var resumo = new OrdensDeServicoResumidas(
            abertas.Count,
            abertas.Count(o => Dias(o) > DiasDaFaixaVermelha),
            abertas.Count == 0 ? null : abertas.Max(o => Dias(o)),
            abertas.Sum(o => o.ValorDePecas + o.ValorDeServicos),
            fechadas12.Count,
            fechadas12.Sum(o => o.ValorDePecas),
            fechadas12.Sum(o => o.ValorDeServicos),
            naoCanceladas.Count == 0 ? null : naoCanceladas.Max(o => o.AbertaEm),
            lidas.Ordens.Count,
            lista,
            lidas.CarregadoEm,
            ausentes);

        return Resultado<ComProcedencia<OrdensDeServicoResumidas>>.Ok(
            ComProcedencia<OrdensDeServicoResumidas>.DoNossoBanco(resumo, "frota.OrdemDeServico", relogio));
    }
}
