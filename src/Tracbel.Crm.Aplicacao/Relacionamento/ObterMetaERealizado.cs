using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Tracbel.Crm.Dominio.Seguranca;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>O período da resposta, sempre escrito junto com o número.</summary>
/// <param name="Inicial">O primeiro mês, no dia 1.</param>
/// <param name="Final">O último mês, no dia 1, inclusive.</param>
/// <param name="Meses">Quantos meses.</param>
/// <param name="AnoFiscal">O ano fiscal do último mês (novembro a outubro, com o nome do ano em que termina).</param>
/// <param name="Texto">"nov/2025 a ago/2026".</param>
/// <param name="EhOPadrao">Se é o período padrão — o ano fiscal até o último mês fechado (27/09/2026).</param>
/// <param name="InicialDoAnterior">O primeiro mês do mesmo trecho do ano fiscal anterior.</param>
/// <param name="FinalDoAnterior">O último mês do mesmo trecho do ano fiscal anterior.</param>
public sealed record PeriodoDaMeta(
    DateOnly Inicial, DateOnly Final, int Meses, int AnoFiscal, string Texto, bool EhOPadrao, DateOnly InicialDoAnterior, DateOnly FinalDoAnterior);

/// <summary>Os totais do período.</summary>
/// <param name="MetaMaquinas">A meta de máquinas, em unidades.</param>
/// <param name="RealizadoMaquinas">As máquinas vendidas que o CRM tem (<c>frota.VendaDeMaquina</c>, D-M3).</param>
/// <param name="PendentesNoArt">As vendas que aguardam na integração do ART (cadastro, chassi ou outro motivo) — nulo no
/// alcance Próprios.</param>
/// <param name="MetaConsorcio">A meta de consórcio, em cotas — à parte, sem realizado (D-M4).</param>
/// <param name="VendasSemVendedor">As vendas do período sem vendedor no ART: contam no total e em consultor nenhum. Em número,
/// para a tela somar as filiais (revisão do PR #248) — a frase da lacuna é de uma filial só.</param>
public sealed record TotaisDaMeta(int MetaMaquinas, int RealizadoMaquinas, int? PendentesNoArt, int MetaConsorcio, int VendasSemVendedor);

/// <summary>O mesmo trecho do ano fiscal anterior — só o realizado: a meta daquele ano não está no cadastro.</summary>
/// <param name="RealizadoMaquinas">As máquinas vendidas no mesmo trecho do ano anterior.</param>
public sealed record MesmoTrechoDoAnoAnterior(int RealizadoMaquinas);

/// <summary>
/// A META DE VENDA × O REALIZADO de uma filial — ou da própria pessoa (#138).
/// </summary>
/// <param name="Periodo">Os meses.</param>
/// <param name="Alcance"><c>Proprios</c> ou <c>Filial</c> — o que a profundidade de <c>Meta.Ler</c> deu.</param>
/// <param name="Totais">Os totais do período.</param>
/// <param name="PorMes">Mês a mês.</param>
/// <param name="PorLinha">Por linha de produto.</param>
/// <param name="PorConsultor">Por consultor (no alcance Próprios, só a pessoa).</param>
/// <param name="MesEmCurso">O mês em curso, quando ficou fora do período — meta e realizado até aqui.</param>
/// <param name="MesmoTrechoDoFyAnterior">O realizado do mesmo trecho do ano fiscal anterior.</param>
/// <param name="Origem">A última leitura do cadastro de metas; nula quando a carga nunca rodou.</param>
/// <param name="MetricasSemDado">O que o número não diz, cada frase com a medida que a sustenta.</param>
public sealed record MetaERealizadoDaFilial(
    PeriodoDaMeta Periodo,
    string Alcance,
    TotaisDaMeta Totais,
    IReadOnlyList<MetaERealizadoNoMes> PorMes,
    IReadOnlyList<MetaERealizadoNaLinha> PorLinha,
    IReadOnlyList<MetaERealizadoDoConsultor> PorConsultor,
    MetaERealizadoNoMes? MesEmCurso,
    MesmoTrechoDoAnoAnterior MesmoTrechoDoFyAnterior,
    OrigemDaMetaDeVenda? Origem,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// A META DE VENDA × O REALIZADO (#138, decisões D-M1..D-M5 de 27/09/2026) — o cartão "Meta e realizado" da Visão 360.
///
/// <para><b>A meta é a cota da API Gestão de Negócios</b>, em unidades por consultor, linha, mês e filial; <b>o realizado
/// são as máquinas do ART que o CRM tem</b> (<c>frota.VendaDeMaquina</c>, D-M3), e as que o ART tem e o CRM ainda não vêm
/// em número, como lacuna. O consórcio fica à parte, em cotas, com o realizado "não medido pelo CRM" (D-M4).</para>
///
/// <para><b>Quem vê o quê é a profundidade de <c>Meta.Ler</c></b> (D-M5): em <c>Proprios</c>, só a própria meta e as
/// próprias vendas; em <c>EmpresaEAbaixo</c> ou mais, a filial inteira. A rota é por filial, como as outras da Visão 360:
/// o consolidado é a tela somando as filiais.</para>
///
/// <para><b>O período padrão é o ano fiscal até o último mês FECHADO</b> (decisão de 27/09/2026, <see cref="AnoFiscal"/>),
/// comparado com o mesmo trecho do ano fiscal anterior: a meta do mês inteiro contra um mês pela metade erraria para
/// baixo. O mês em curso vem à parte, com a meta e o que já foi vendido nele.</para>
/// </summary>
/// <param name="repositorio">Onde a meta e o realizado são apurados.</param>
/// <param name="acesso">Quem pergunta.</param>
/// <param name="relogio">O relógio.</param>
public sealed class ObterMetaERealizado(IRepositorioDeMetas repositorio, IProvedorContextoAcesso acesso, IRelogio relogio)
{
    /// <summary>O primeiro mês aceito: antes disso não há meta nem venda carregada.</summary>
    private static readonly DateOnly PrimeiroMesAceito = new(2020, 1, 1);

    /// <summary>O maior período de uma consulta: dois anos fiscais.</summary>
    private const int MesesNoMaximo = 24;

    private static readonly CultureInfo Portugues = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>
    /// O FUSO DA OPERAÇÃO. O relógio é UTC, e o mês fechado é o de São Paulo: das 21h do último dia até a meia-noite UTC,
    /// o UTC já está no mês seguinte e o período padrão pularia um mês que ainda não fechou (revisão do PR #248). Sem o
    /// fuso IANA no sistema, as mesmas −3 h que o resto do código usa — o Brasil não tem horário de verão desde 2019.
    /// </summary>
    private static readonly TimeZoneInfo FusoDeSaoPaulo = FusoDaOperacao();

    /// <summary>O mês corrente em São Paulo, no dia 1, a partir do instante em UTC.</summary>
    /// <param name="agoraUtc">O instante, em UTC.</param>
    public static DateOnly MesCorrenteEmSaoPaulo(DateTime agoraUtc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(agoraUtc, DateTimeKind.Utc), FusoDeSaoPaulo);
        return new DateOnly(local.Year, local.Month, 1);
    }

    private static TimeZoneInfo FusoDaOperacao()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (Exception falha) when (falha is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("America/Sao_Paulo", TimeSpan.FromHours(-3), "São Paulo", "São Paulo");
        }
    }

    /// <summary>Executa a apuração.</summary>
    /// <param name="competenciaInicial">O primeiro mês (<c>AAAA-MM</c> ou <c>AAAA-MM-DD</c>); vazio com o final vazio é o padrão.</param>
    /// <param name="competenciaFinal">O último mês, inclusive.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<MetaERealizadoDaFilial>>> ExecutarAsync(
        string? competenciaInicial, string? competenciaFinal, CancellationToken ct)
    {
        var profundidade = acesso.Atual.ProfundidadeDe(Permissoes.MetaLer);
        if (profundidade == Profundidade.Nenhum)
            return Resultado<ComProcedencia<MetaERealizadoDaFilial>>.SemPermissao(
                $"Falta a permissão '{Permissoes.MetaLer}' ({Permissoes.Catalogo[Permissoes.MetaLer]}).");

        // PRÓPRIOS E EQUIPE VEEM SÓ A PRÓPRIA META: a matriz da D-M5 não tem "equipe", e mostrar a mais seria abrir o que
        // ninguém decidiu abrir.
        var alcance = profundidade is Profundidade.Proprios or Profundidade.Equipe ? AlcanceDaMeta.Proprios : AlcanceDaMeta.Filial;

        var mesCorrente = MesCorrenteEmSaoPaulo(relogio.Agora);

        var erros = new ColetorDeErros();
        var inicial = Mes(erros, "competenciaInicial", competenciaInicial, mesCorrente.Year);
        var final = Mes(erros, "competenciaFinal", competenciaFinal, mesCorrente.Year);

        if (string.IsNullOrWhiteSpace(competenciaInicial) != string.IsNullOrWhiteSpace(competenciaFinal))
            erros.Registrar(string.IsNullOrWhiteSpace(competenciaInicial) ? "competenciaInicial" : "competenciaFinal",
                "Informe o mês inicial e o final juntos — ou nenhum dos dois, para o ano fiscal até o último mês fechado.");

        if (inicial is { } i && final is { } f)
        {
            if (f < i) erros.Registrar("competenciaFinal", "O mês final vem depois do inicial.", competenciaFinal);
            else if (new JanelaDeCompetencia(i, f).Meses > MesesNoMaximo)
                erros.Registrar("competenciaFinal", $"O período vai até {MesesNoMaximo} meses.", competenciaFinal);
        }

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<MetaERealizadoDaFilial>>("A consulta tem parâmetros que não valem.");

        var ehOPadrao = inicial is null;
        var periodo = ehOPadrao ? AnoFiscal.AteOUltimoMesFechado(mesCorrente) : new JanelaDeCompetencia(inicial!.Value, final!.Value);
        var anterior = periodo.DoAnoAnterior();
        DateOnly? mesEmCurso = periodo.Contem(mesCorrente) ? null : mesCorrente;

        var apurado = await repositorio.ApurarAsync(periodo, anterior, ehOPadrao ? mesEmCurso : null, alcance, ct);

        var resposta = new MetaERealizadoDaFilial(
            new PeriodoDaMeta(periodo.Inicial, periodo.Final, periodo.Meses, AnoFiscal.Do(periodo.Final), periodo.Texto, ehOPadrao,
                anterior.Inicial, anterior.Final),
            alcance.ToString(),
            new TotaisDaMeta(apurado.MetaMaquinas, apurado.RealizadoMaquinas, apurado.PendentesNoArt, apurado.MetaConsorcio, apurado.VendasSemVendedor),
            apurado.PorMes,
            apurado.PorLinha,
            apurado.PorConsultor,
            apurado.MesEmCurso,
            new MesmoTrechoDoAnoAnterior(apurado.RealizadoNoAnterior),
            apurado.Origem,
            Lacunas(apurado, periodo, anterior, alcance));

        return Resultado<ComProcedencia<MetaERealizadoDaFilial>>.Ok(
            ComProcedencia<MetaERealizadoDaFilial>.DoNossoBanco(
                resposta, "organizacao.MetaDeVenda (API Gestão de Negócios) · frota.VendaDeMaquina (ART) · integracao.RegistroDeOrigem", relogio));
    }

    /// <summary>O mês do pedido: <c>AAAA-MM</c> ou <c>AAAA-MM-DD</c> (o dia é ignorado), de 2020 até o ano que vem.</summary>
    private static DateOnly? Mes(ColetorDeErros erros, string campo, string? texto, int anoCorrente)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        var bruto = texto.Trim();
        var formato = bruto.Length == 7 ? "yyyy-MM" : "yyyy-MM-dd";
        if (!DateOnly.TryParseExact(bruto, formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
        {
            erros.Registrar(campo, "Informe o mês como AAAA-MM.", texto);
            return null;
        }

        var mes = new DateOnly(data.Year, data.Month, 1);
        if (mes < PrimeiroMesAceito || mes.Year > anoCorrente + 1)
        {
            erros.Registrar(campo, "O mês vai de 2020-01 ao fim do ano que vem.", texto);
            return null;
        }

        return mes;
    }

    private static string Texto(FormattableString texto) => texto.ToString(Portugues);

    /// <summary>O que o número não diz — cada frase com a medida que a sustenta.</summary>
    private static List<MetricaSemDado> Lacunas(MetaERealizadoApurado a, JanelaDeCompetencia periodo, JanelaDeCompetencia anterior, AlcanceDaMeta alcance)
    {
        var lacunas = new List<MetricaSemDado>();

        if (a.Origem is null)
            lacunas.Add(new MetricaSemDado("cadastroDeMetasNaoLido",
                "O cadastro de metas da API Gestão de Negócios ainda não foi lido: grave o endereço pelo nome e a chave em " +
                "Configurações › Integrações, teste, e ligue a rotina \"Metas de venda (Gestão de Negócios)\". Sem a leitura, a meta é zero."));
        else if (a.LoginSemCasamento)
            lacunas.Add(new MetricaSemDado("loginSemCasamento",
                "Seu login não casa com consultor nenhum da API Gestão de Negócios nem com vendedor nenhum do ART nesta filial: por " +
                "isso não aparece meta nem venda em seu nome — e não porque não haja. Quem administra o CRM confere se a sua conta " +
                "tem o mesmo nome.sobrenome da GN e do ART."));
        else if (a.MetaMaquinas == 0 && a.MetaConsorcio == 0)
            lacunas.Add(new MetricaSemDado("semMetaNoPeriodo",
                Texto($"Nenhuma meta cadastrada na API Gestão de Negócios para {periodo.Texto}{(alcance == AlcanceDaMeta.Proprios ? " em seu nome" : " nesta filial")}. Sem meta, não há comparação: o cartão mostra só o realizado.")));

        if (a.MesEmCurso is { } emCurso)
            lacunas.Add(new MetricaSemDado("mesEmCurso",
                Texto($"{JanelaDeCompetencia.Mes(emCurso.Competencia)} está em curso e fica fora da comparação — a meta do mês inteiro contra um mês pela metade erraria para baixo. Até agora: {emCurso.RealizadoMaquinas:N0} máquina(s) vendida(s), para uma meta de {emCurso.MetaMaquinas:N0} no mês.")));

        if (a.UltimaCompetenciaComMeta is { } ultima && ultima < periodo.Final)
            lacunas.Add(new MetricaSemDado("metaParcial",
                Texto($"O cadastro de metas vai até {JanelaDeCompetencia.Mes(ultima)}: os meses depois disso ficam SEM meta, e não com meta zero.")));

        if (a.PendentesNoArt is > 0)
            lacunas.Add(new MetricaSemDado("pendentesNoArt",
                Texto($"{a.PendentesNoArt:N0} vendas do ART em {periodo.Texto} aguardam na integração do ART (cadastro, chassi ou outro motivo) e ainda não entram no realizado (D-M3): o realizado é o que o CRM tem.")));
        else if (alcance == AlcanceDaMeta.Proprios)
            lacunas.Add(new MetricaSemDado("pendentesNoArt",
                "As vendas que aguardam na integração do ART (cadastro, chassi ou outro motivo) não entram no realizado (D-M3), e a pendente ainda não tem vendedor atribuído — por isso não aparecem aqui. A visão da filial as conta."));

        // A VENDA POR OUTRA FILIAL (revisão do PR #248): no alcance Próprios, o recorte é o da filial do pedido, como toda
        // leitura. Ampliar para "todas as vendas da pessoa" é decisão pendente — a tela diz o que conta.
        if (alcance == AlcanceDaMeta.Proprios)
            lacunas.Add(new MetricaSemDado("vendasPelaFilial",
                "Suas vendas contam pela filial da venda: a que você fez por outra filial aparece na meta de lá, e não aqui."));

        if (a.PendentesSemFilial > 0 && alcance == AlcanceDaMeta.Filial)
            lacunas.Add(new MetricaSemDado("pendentesSemFilial",
                Texto($"{a.PendentesSemFilial:N0} vendas pendentes do ART no período têm unidade sem filial no CRM e não são contadas em filial nenhuma.")));

        lacunas.Add(new MetricaSemDado("consorcio",
            a.MetaConsorcio > 0
                ? Texto($"Meta de consórcio: {a.MetaConsorcio:N0} cotas no período. O realizado de consórcio não é medido pelo CRM — o ART registra máquina, não cota (D-M4).")
                : "O realizado de consórcio não é medido pelo CRM — o ART registra máquina, não cota (D-M4)."));

        if (alcance == AlcanceDaMeta.Filial && a.ConsultoresSemConta > 0)
            lacunas.Add(new MetricaSemDado("consultoresSemConta",
                Texto($"{a.ConsultoresSemConta:N0} consultor(es) com meta nesta filial não têm conta no CRM: a meta deles aparece só aqui, na visão da filial.")));

        if (a.VendasSemVendedor > 0)
            lacunas.Add(new MetricaSemDado("vendasSemVendedor",
                Texto($"{a.VendasSemVendedor:N0} venda(s) do período sem vendedor no ART entram no total da filial e em consultor nenhum. O vendedor passou a ser lido em 27/09/2026 (D-M2); a primeira leitura do ART depois da publicação preenche as vendas já importadas que o ART ainda traz, inclusive as que ficaram pendentes.")));

        lacunas.Add(new MetricaSemDado("metaDoAnoAnterior",
            Texto($"O mesmo trecho do ano anterior ({anterior.Texto}) traz só o realizado: a meta daquele ano não está no cadastro da API Gestão de Negócios.")));

        lacunas.Add(new MetricaSemDado("previsao",
            "Não há previsão do ano: nenhum modelo de projeção foi aprovado. A API Gestão de Negócios tem uma previsão feita por pessoas (/cadastros/forecast), que ainda não é lida."));

        return lacunas;
    }
}
