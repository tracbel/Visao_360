using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Processo;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>
/// O PERÍODO DE UM RELATÓRIO DO FUNIL, sempre escrito junto do número.
///
/// <para><b>O padrão é o ano fiscal até o último mês fechado</b> (decisão do Ricardo de 27/09/2026, <see cref="AnoFiscal"/>),
/// o mesmo das metas e dos indicadores: quem abre a tela vê o FY corrente sem o mês pela metade.</para>
///
/// <para><b>O dia é o de São Paulo.</b> As datas do funil são UTC; o período vira instantes UTC com o deslocamento fixo de
/// −3 h, a mesma conta de <see cref="AnoFiscal.MesCorrenteEmSaoPaulo"/> — sem isso, o processo aberto às 22h do último
/// dia do mês cairia no mês seguinte.</para>
/// </summary>
/// <param name="De">O primeiro dia, inclusive.</param>
/// <param name="Ate">O último dia, inclusive.</param>
/// <param name="Texto">"nov/2025 a ago/2026" no padrão; "01/11/2025 a 15/09/2026" num período escolhido.</param>
/// <param name="EhOPadrao">Se é o ano fiscal até o último mês fechado.</param>
public sealed record PeriodoDoRelatorio(DateOnly De, DateOnly Ate, string Texto, bool EhOPadrao)
{
    /// <summary>O primeiro dia aceito: o funil do Vórtice começa em 01/11/2023, e a venda perdida em 2012.</summary>
    private static readonly DateOnly PrimeiroDiaAceito = new(2012, 1, 1);

    /// <summary>O começo do período, em UTC.</summary>
    public DateTime DeUtc => De.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddHours(3);

    /// <summary>O fim do período, em UTC, exclusive: a meia-noite de São Paulo do dia seguinte ao último.</summary>
    public DateTime AteUtc => Ate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddHours(3);

    /// <summary>O período pedido, ou o padrão; nulo com o erro registrado quando o pedido não vale.</summary>
    /// <param name="de">O primeiro dia; vazio com <paramref name="ate"/> vazio é o padrão.</param>
    /// <param name="ate">O último dia.</param>
    /// <param name="agoraUtc">O instante, para o padrão.</param>
    /// <param name="erros">Onde registrar a recusa.</param>
    public static PeriodoDoRelatorio? Resolver(DateOnly? de, DateOnly? ate, DateTime agoraUtc, ColetorDeErros erros)
    {
        if (de is null && ate is null)
        {
            var janela = AnoFiscal.AteOUltimoMesFechado(AnoFiscal.MesCorrenteEmSaoPaulo(agoraUtc));
            return new PeriodoDoRelatorio(janela.Inicial, janela.Final.AddMonths(1).AddDays(-1), janela.Texto, true);
        }

        if (de is null || ate is null)
        {
            erros.Registrar(de is null ? "de" : "ate",
                "Informe o primeiro e o último dia juntos — ou nenhum dos dois, para o ano fiscal até o último mês fechado.");
            return null;
        }

        if (ate < de)
        {
            erros.Registrar("ate", "O último dia vem depois do primeiro.", ate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            return null;
        }

        if (de < PrimeiroDiaAceito)
        {
            erros.Registrar("de", "O período começa em 2012-01-01 ou depois.", de.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            return null;
        }

        var texto = string.Create(CultureInfo.InvariantCulture, $"{de.Value:dd/MM/yyyy} a {ate.Value:dd/MM/yyyy}");
        return new PeriodoDoRelatorio(de.Value, ate.Value, texto, false);
    }
}

/// <summary>Um estágio do funil como a tela o consome.</summary>
/// <param name="Estagio">O código: <c>Lead</c>, <c>Qualificado</c>, <c>Cobertura</c>, <c>Negociacao</c>, <c>Pedido</c>, <c>Faturamento</c>.</param>
/// <param name="Nome">O rótulo da tela.</param>
/// <param name="Processos">Quantos processos.</param>
/// <param name="PercentualSobreOAnterior">Sobre o estágio anterior; nulo no Lead e quando o anterior é zero.</param>
/// <param name="PercentualSobreOLead">Sobre o Lead; nulo quando o Lead é zero.</param>
/// <param name="PelaEntradaDigital">O subfunil digital: quantos alcançaram o estágio pela entrada digital.</param>
/// <param name="Ganhos">Desfecho ganho.</param>
/// <param name="Perdidos">Desfecho perdido.</param>
/// <param name="Abertos">Ainda abertos.</param>
/// <param name="Outros">Cancelados e suspensos.</param>
public sealed record EstagioNoFunil(
    string Estagio, string Nome, int Processos, decimal? PercentualSobreOAnterior, decimal? PercentualSobreOLead,
    int PelaEntradaDigital, int Ganhos, int Perdidos, int Abertos, int Outros);

/// <summary>O funil por estágio como a tela o consome.</summary>
/// <param name="Periodo">O período, escrito.</param>
/// <param name="Base"><c>abertura</c> (coorte) ou <c>etapa</c> (fluxo).</param>
/// <param name="Estagios">Os seis estágios; vazio quando o funil não tem dado — o motivo vai em <paramref name="MetricasSemDado"/>.</param>
/// <param name="ParadosEmNegociacaoOuPedido">O alerta: parados há mais de <see cref="ObterFunilPorEstagio.DiasParaParado"/> dias; nulo sem dado.</param>
/// <param name="DiasParaParado">O corte do alerta, em dias.</param>
/// <param name="Rotina">A última execução da rotina que traz o funil.</param>
/// <param name="MetricasSemDado">O que o número não diz, com o motivo verdadeiro.</param>
public sealed record FunilPorEstagio(
    PeriodoDoRelatorio Periodo,
    string Base,
    IReadOnlyList<EstagioNoFunil> Estagios,
    int? ParadosEmNegociacaoOuPedido,
    int DiasParaParado,
    ExecucaoDaRotinaDoFunil? Rotina,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// O FUNIL POR ESTÁGIO — Lead, Qualificado, Cobertura, Negociação, Pedido e Faturamento — dos processos 31/41/50 do
/// Vórtice (decisões do Ricardo de 27/09/2026, documento 52).
///
/// <para><b>Não é a fase do BPM.</b> A fase do fluxo (a da API Gestão de Negócios e de <c>processo.Processo</c>) diz em
/// que caixa do fluxo o processo está; o estágio diz até onde ele chegou, pelo resultado do histórico, como o BI. O
/// funil é o estágio.</para>
///
/// <para><b>Lead e Qualificado são cumulativos</b>: quem chegou à Cobertura também conta como Lead e Qualificado. Por isso
/// o percentual sobre o estágio anterior nunca passa de 100% por construção — a linha é gravada assim pela rotina, e esta
/// leitura só conta.</para>
///
/// <para><b>O vazio tem motivo verdadeiro.</b> Sem linha nenhuma na filial, o funil não é "zero": a rotina
/// <c>PROCESSOS_VORTICE</c> ainda não rodou (ela nasce desligada), ou rodou e não trouxe processo desta filial — a
/// resposta diz qual dos dois, pela <c>integracao.Rotina</c>.</para>
/// </summary>
/// <param name="repositorio">O acesso ao funil.</param>
/// <param name="relogio">O relógio.</param>
public sealed class ObterFunilPorEstagio(IRepositorioFunilPorEstagio repositorio, IRelogio relogio)
{
    /// <summary>
    /// O CORTE DO ALERTA DE PROCESSO PARADO: 60 dias em Negociação ou Pedido sem avançar nem encerrar (§8 do plano de
    /// 27/09/2026). É o dobro do ciclo de visita mais curto das carteiras — menos que isso é negociação em curso.
    /// </summary>
    public const int DiasParaParado = 60;

    private static readonly CultureInfo Portugues = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly IReadOnlyDictionary<EstagioDoFunil, string> Nomes = new Dictionary<EstagioDoFunil, string>
    {
        [EstagioDoFunil.Lead] = "Lead",
        [EstagioDoFunil.Qualificado] = "Qualificado",
        [EstagioDoFunil.Cobertura] = "Cobertura",
        [EstagioDoFunil.Negociacao] = "Negociação",
        [EstagioDoFunil.Pedido] = "Pedido",
        [EstagioDoFunil.Faturamento] = "Faturamento"
    };

    /// <summary>Executa a apuração.</summary>
    /// <param name="de">O primeiro dia; vazio com o último vazio é o ano fiscal até o último mês fechado.</param>
    /// <param name="ate">O último dia, inclusive.</param>
    /// <param name="base">A leitura: <c>abertura</c> (coorte, o padrão) ou <c>etapa</c> (fluxo).</param>
    /// <param name="carteira">A carteira, pela chave pública.</param>
    /// <param name="responsavel">O responsável, pela chave pública.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<FunilPorEstagio>>> ExecutarAsync(
        DateOnly? de, DateOnly? ate, string? @base, Guid? carteira, Guid? responsavel, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var erros = new ColetorDeErros();
        var periodo = PeriodoDoRelatorio.Resolver(de, ate, agora, erros);
        var leitura = erros.ItemDeDominioOuPadrao("base", @base, BaseDoFunil.Abertura);

        if (erros.TemErro || periodo is null)
            return erros.Recusar<ComProcedencia<FunilPorEstagio>>("A consulta tem parâmetros que não valem.");

        var apurado = await repositorio.ApurarAsync(
            new ConsultaDoFunilPorEstagio(periodo.DeUtc, periodo.AteUtc, leitura, carteira, responsavel, agora.AddDays(-DiasParaParado)), ct);

        if (apurado is null)
            return Resultado<ComProcedencia<FunilPorEstagio>>.NaoEncontrado(
                "Não há carteira ou responsável com essa chave ao alcance deste contexto de acesso.");

        var lacunas = new List<MetricaSemDado>();
        var semLinha = apurado.LinhasNaFilial == 0;

        if (semLinha)
            lacunas.Add(new MetricaSemDado("funil", MotivoDoFunilVazio(apurado.Rotina)));
        else if (apurado.Estagios.All(e => e.Processos == 0))
            lacunas.Add(new MetricaSemDado("funilNoPeriodo", leitura == BaseDoFunil.Abertura
                ? $"Nenhum processo do Vórtice aberto em {periodo.Texto} alcançou um estágio, no recorte pedido."
                : $"Nenhum processo do Vórtice alcançou um estágio em {periodo.Texto}, no recorte pedido."));

        IReadOnlyList<EstagioNoFunil> estagios = semLinha ? [] : Estagios(apurado.Estagios);

        var resposta = new FunilPorEstagio(
            periodo,
            leitura == BaseDoFunil.Abertura ? "abertura" : "etapa",
            estagios,
            semLinha ? null : apurado.ParadosEmNegociacaoOuPedido,
            DiasParaParado,
            apurado.Rotina,
            lacunas);

        return Resultado<ComProcedencia<FunilPorEstagio>>.Ok(new ComProcedencia<FunilPorEstagio>(resposta, Carimbo(apurado.Rotina, agora)));
    }

    /// <summary>Os seis estágios, com os dois percentuais — na ordem, e com zero onde o banco não devolveu linha.</summary>
    private static List<EstagioNoFunil> Estagios(IReadOnlyList<EstagioContado> contados)
    {
        var porEstagio = contados.ToDictionary(c => c.Estagio);
        var lead = porEstagio.GetValueOrDefault(EstagioDoFunil.Lead)?.Processos ?? 0;
        var lista = new List<EstagioNoFunil>();
        int? anterior = null;

        foreach (var estagio in Enum.GetValues<EstagioDoFunil>())
        {
            var c = porEstagio.GetValueOrDefault(estagio) ?? new EstagioContado(estagio, 0, 0, 0, 0, 0, 0);
            lista.Add(new EstagioNoFunil(
                estagio.ToString(),
                Nomes[estagio],
                c.Processos,
                anterior is > 0 ? Percentual(c.Processos, anterior.Value) : null,
                lead > 0 ? Percentual(c.Processos, lead) : null,
                c.PelaEntradaDigital,
                c.Ganhos,
                c.Perdidos,
                c.Abertos,
                c.Outros));
            anterior = c.Processos;
        }

        return lista;
    }

    private static decimal Percentual(int parte, int todo) => decimal.Round(100m * parte / todo, 1);

    /// <summary>
    /// O MOTIVO VERDADEIRO DO FUNIL VAZIO, lido da <c>integracao.Rotina</c>: nunca rodou, rodou e falhou, ou rodou e não
    /// trouxe processo desta filial. Cada um pede uma ação diferente de quem administra.
    /// </summary>
    private static string MotivoDoFunilVazio(ExecucaoDaRotinaDoFunil? rotina)
    {
        var nome = rotina?.Nome ?? "Funil e vendas perdidas do Vórtice";

        if (rotina?.IniciadaEm is not { } iniciada)
            return $"A rotina \"{nome}\" (PROCESSOS_VORTICE) ainda não rodou: ela nasce desligada, e quem administra a liga em " +
                   "Configurações › Integrações. Até ela rodar, o CRM não tem estágio de processo nenhum — e não mostra zero no lugar.";

        var quando = iniciada.AddHours(-3).ToString("dd/MM/yyyy HH:mm", Portugues);
        return rotina.Resultado switch
        {
            "Falha" => $"A última execução da rotina \"{nome}\" ({quando}) terminou em falha e não gravou o funil. A mensagem " +
                       "dela está em Configurações › Integrações.",
            "EmAndamento" => $"A rotina \"{nome}\" está rodando desde {quando}; o funil aparece quando ela terminar.",
            _ => $"A rotina \"{nome}\" rodou em {quando} e não trouxe processo 31/41/50 do Vórtice desta filial."
        };
    }

    /// <summary>O carimbo: a tabela lida e a última execução da rotina que a grava.</summary>
    private static Procedencia Carimbo(ExecucaoDaRotinaDoFunil? rotina, DateTime agora) =>
        new(
            Procedencias.SistemaProprio,
            "processo.EstagioDoProcesso (rotina PROCESSOS_VORTICE)",
            agora,
            rotina?.TerminadaEm ?? rotina?.IniciadaEm,
            rotina?.IniciadaEm is null,
            rotina?.IniciadaEm is null ? "A rotina que traz o funil do Vórtice ainda não rodou." : null);
}
