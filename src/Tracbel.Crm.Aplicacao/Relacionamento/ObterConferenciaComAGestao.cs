using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>A meta e o realizado de máquinas, lá (GN) e aqui (CRM).</summary>
public sealed record NumerosDaConferencia(int MetaNaGestao, int MetaNoCrm, int RealizadoNaGestao, int RealizadoNoCrm);

/// <summary>Os números de uma filial.</summary>
public sealed record ConferenciaDaFilial(string Filial, NumerosDaConferencia Numeros);

/// <summary>Os números de um mês.</summary>
public sealed record ConferenciaDoMes(DateOnly Competencia, NumerosDaConferencia Numeros);

/// <summary>Quantas divergências abertas de um tipo.</summary>
public sealed record ContagemDeDivergencia(string Tipo, string Rotulo, int Quantidade);

/// <summary>Uma divergência na tela.</summary>
public sealed record DivergenciaNaTela(
    string Tipo, string Rotulo, string Chassi, string Filial, string Descricao, string? NoCrm, string? NaGestao, DateTime DetectadaEm);

/// <summary>A conferência — o que <c>GET /api/v1/integracoes/conferencia-gn</c> devolve.</summary>
public sealed record ConferenciaComAGestao(
    string Alcance,
    NumerosDaConferencia Totais,
    IReadOnlyList<ConferenciaDaFilial> PorFilial,
    IReadOnlyList<ConferenciaDoMes> PorMes,
    IReadOnlyList<ContagemDeDivergencia> PorTipo,
    IReadOnlyList<DivergenciaNaTela> Divergencias,
    DateTime? ApuradaEm,
    DateTime? GeradaNaOrigemEm,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// A CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026) — os números do CRM contra o gabarito da GN:
/// a meta e o realizado de máquinas por filial e mês, e cada máquina do realizado que não bate, com o motivo.
///
/// <para><b>Pede <c>Integracao.Ler</c></b>: é a tela de quem cuida dos números — a gerência e a administração. Pela filial
/// escolhida no seletor, ou por todas, em "Todas as filiais".</para>
/// </summary>
public sealed class ObterConferenciaComAGestao(IRepositorioDaConferencia repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>O nome de cada tipo de divergência, como a tela o escreve.</summary>
    public static readonly IReadOnlyDictionary<TipoDeDivergencia, string> Rotulos = new Dictionary<TipoDeDivergencia, string>
    {
        [TipoDeDivergencia.RealizadoSoNaGestao] = "Só na Gestão de Negócios",
        [TipoDeDivergencia.RealizadoPendenteNoArt] = "Pendente na integração do ART",
        [TipoDeDivergencia.RealizadoNaoEntregueNoCrm] = "Sem a entrega no CRM",
        [TipoDeDivergencia.RealizadoEmOutraFilial] = "Em outra filial",
        [TipoDeDivergencia.RealizadoEmOutroMes] = "Em outro mês",
        [TipoDeDivergencia.RealizadoSoNoCrm] = "Só no CRM"
    };

    /// <summary>Executa a leitura.</summary>
    public async Task<Resultado<ComProcedencia<ConferenciaComAGestao>>> ExecutarAsync(CancellationToken ct)
    {
        var lida = await repositorio.LerAsync(ct);
        var resposta = Montar(lida, acesso.Atual.VeTodasAsFiliais);
        return Resultado<ComProcedencia<ConferenciaComAGestao>>.Ok(ComProcedencia<ConferenciaComAGestao>.DoNossoBanco(
            resposta,
            "integracao.ConferenciaDaGestaoDeNegocios · integracao.DivergenciaDeIntegracao (API Gestão de Negócios × CRM)",
            relogio));
    }

    /// <summary>A conta, sem banco.</summary>
    public static ConferenciaComAGestao Montar(ConferenciaLida lida, bool organizacao)
    {
        static NumerosDaConferencia Numeros(IEnumerable<LinhaDaConferencia> linhas)
        {
            var lista = linhas.ToList();
            int Soma(string indicador, Func<LinhaDaConferencia, int> valor) => lista.Where(l => l.Indicador == indicador).Sum(valor);
            return new NumerosDaConferencia(
                Soma(ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, l => l.NaGestao),
                Soma(ConferenciaDaGestaoDeNegocios.IndicadorMetaDeMaquinas, l => l.NoCrm),
                Soma(ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, l => l.NaGestao),
                Soma(ConferenciaDaGestaoDeNegocios.IndicadorRealizadoDeMaquinas, l => l.NoCrm));
        }

        var porFilial = lida.Linhas.GroupBy(l => l.Filial, StringComparer.Ordinal)
            .Select(g => new ConferenciaDaFilial(g.Key, Numeros(g)))
            .OrderBy(f => f.Filial, StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: true))
            .ToList();
        var porMes = lida.Linhas.GroupBy(l => l.Competencia)
            .Select(g => new ConferenciaDoMes(g.Key, Numeros(g)))
            .OrderBy(m => m.Competencia)
            .ToList();
        var porTipo = Rotulos
            .Select(r => new ContagemDeDivergencia(r.Key.ToString(), r.Value, lida.Divergencias.Count(d => d.Tipo == r.Key)))
            .ToList();
        var divergencias = lida.Divergencias
            .Select(d => new DivergenciaNaTela(d.Tipo.ToString(), Rotulos.GetValueOrDefault(d.Tipo, d.Tipo.ToString()), d.Chassi, d.Filial, d.Descricao,
                d.ValorNoCrm, d.ValorNaGestao, d.DetectadaEm))
            .ToList();

        var lacunas = new List<MetricaSemDado>();
        if (lida.ApuradaEm is null)
            lacunas.Add(new MetricaSemDado("naoApurada",
                "A conferência ainda não foi apurada: ela vem da rotina \"Conferência com a Gestão de Negócios\". Sem ela, a tela fica vazia — e não quer dizer que os números batem."));
        if (!organizacao)
            lacunas.Add(new MetricaSemDado("alcanceDaFilial",
                "Os números são só da filial escolhida. A máquina que conta em outra filial aparece na filial da GN; a conferência da empresa inteira é em \"Todas as filiais\"."));
        lacunas.Add(new MetricaSemDado("regua",
            "Os dois lados contam pela mesma régua: a meta sem consórcio, e o realizado só com a máquina entregue, no mês da entrega."));

        return new ConferenciaComAGestao(
            organizacao ? "Organizacao" : "Filiais", Numeros(lida.Linhas), porFilial, porMes, porTipo, divergencias, lida.ApuradaEm, lida.GeradaNaOrigemEm,
            lacunas);
    }
}
