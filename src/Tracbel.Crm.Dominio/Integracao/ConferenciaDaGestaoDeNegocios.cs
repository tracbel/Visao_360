using Tracbel.Crm.Dominio.Comum;

namespace Tracbel.Crm.Dominio.Integracao;

/// <summary>
/// UMA LINHA DA CONFERÊNCIA COM A GESTÃO DE NEGÓCIOS (decisão do Ricardo em 28/09/2026) — um indicador, numa filial e
/// num mês, como a GN conta e como o CRM conta.
///
/// <para><b>É apuração, e não espelho.</b> A rotina 13 recalcula a conferência inteira a cada rodada: a linha que não
/// aparece mais sai, e a nova entra. Não há revisão nem trilha — o que muda é o número do dia, e a divergência que o
/// explica fica em <see cref="DivergenciaDeIntegracao"/>, chassi a chassi, com o ciclo de vida dela.</para>
///
/// <para><b>É de uma filial</b> (<see cref="EmpresaId"/>, a fronteira de acesso): o filtro global a recorta como tudo o
/// que tem filial. Não é <see cref="EntidadeBase"/> porque não tem autor nem exclusão lógica — é um resultado.</para>
/// </summary>
public sealed class ConferenciaDaGestaoDeNegocios
{
    /// <summary>A meta de máquinas (o PO), em unidades — sem consórcio.</summary>
    public const string IndicadorMetaDeMaquinas = "META_MAQUINAS";

    /// <summary>O realizado de máquinas, em unidades — só entregue, no mês da entrega.</summary>
    public const string IndicadorRealizadoDeMaquinas = "REALIZADO_MAQUINAS";

    /// <summary>O fluxo da rotina da conferência — o ponto de sincronismo (o frescor).</summary>
    public const string FluxoDaCarga = "GESTAO_NEGOCIOS.CONFERENCIA";

    private ConferenciaDaGestaoDeNegocios() { }

    /// <summary>Identificador interno.</summary>
    public long Id { get; private set; }

    /// <summary>O sistema de onde veio o gabarito.</summary>
    public int SistemaId { get; private set; }

    /// <summary>A filial — a fronteira de acesso.</summary>
    public int EmpresaId { get; private set; }

    /// <summary>O indicador.</summary>
    public string Indicador { get; private set; } = default!;

    /// <summary>O mês, no dia 1.</summary>
    public DateOnly Competencia { get; private set; }

    /// <summary>Como a Gestão de Negócios conta.</summary>
    public int NaGestao { get; private set; }

    /// <summary>Como o CRM conta.</summary>
    public int NoCrm { get; private set; }

    /// <summary>Quando a rodada apurou (UTC).</summary>
    public DateTime ApuradaEm { get; private set; }

    /// <summary>Quem rodou a rotina.</summary>
    public long ApuradaPorId { get; private set; }

    /// <summary>A diferença: o CRM menos a GN.</summary>
    public int Diferenca => NoCrm - NaGestao;

    /// <summary>Apura uma linha.</summary>
    public static ConferenciaDaGestaoDeNegocios Apurar(
        int sistemaId, int empresaId, string indicador, DateOnly competencia, int naGestao, int noCrm, DateTime apuradaEm, long apuradaPorId)
    {
        if (indicador is not (IndicadorMetaDeMaquinas or IndicadorRealizadoDeMaquinas))
            throw new RegraDeNegocioViolada("A conferência apura a meta ou o realizado de máquinas.");
        if (competencia.Day != 1) throw new RegraDeNegocioViolada("A competência da conferência é o mês, no dia 1.");
        if (naGestao < 0 || noCrm < 0) throw new RegraDeNegocioViolada("A conferência conta unidades: não há número negativo.");
        return new ConferenciaDaGestaoDeNegocios
        {
            SistemaId = sistemaId, EmpresaId = empresaId, Indicador = indicador, Competencia = competencia, NaGestao = naGestao,
            NoCrm = noCrm, ApuradaEm = apuradaEm, ApuradaPorId = apuradaPorId
        };
    }
}
