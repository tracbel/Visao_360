using System.Globalization;
using Tracbel.Crm.Aplicacao.Comum;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;

namespace Tracbel.Crm.Aplicacao.Relacionamento;

/// <summary>Os cinco indicadores de uma filial, junto do que eles não dizem.</summary>
/// <param name="Indicadores">Os números, todos somáveis entre filiais exceto o que se declara não somável.</param>
/// <param name="MetricasSemDado">Cada limite com a medida que o sustenta.</param>
public sealed record PainelExecutivoDaFilial(
    IndicadoresExecutivosDaFilial Indicadores,
    IReadOnlyList<MetricaSemDado> MetricasSemDado);

/// <summary>
/// OS CINCO CARTÕES DA VISÃO 360 (documento 36) — o faturamento do ano, clientes na carteira, cobertura pela
/// cadência e o que o CRM sabe do mercado. A meta × realizado do cartão B é a meta de VENDA, em rota própria
/// (<see cref="ObterMetaERealizado"/>, #138).
///
/// <para><b>O faturamento do ano é o ART desde 29/09/2026</b> (decisão do Ricardo): o valor de venda das máquinas
/// ENTREGUES, com e sem comprador no CRM, nos mesmos meses do ano. A nota do Protheus continua na resposta — o mês mais
/// recente e o ano —, como conferência e como o faturamento de peça e serviço.</para>
///
/// <para><b>Uma filial por chamada.</b> O consolidado das filiais é a tela somando as respostas
/// (ponte P-8 do documento 23); por isso cada número daqui é de uma partição que não se sobrepõe
/// entre filiais. O que não se soma, a documentação do contrato diz.</para>
///
/// <para><b>O ano é o FISCAL, e vem do pedido</b> (decisão de 27/09/2026): novembro a outubro, com o nome do
/// ano em que termina. O civil continua possível por <c>ano</c>, que é como a rota era chamada antes da
/// decisão; os dois juntos não fazem sentido e são recusados.</para>
///
/// <para><b>O ano em curso vai até o ÚLTIMO MÊS FECHADO</b> (decisão do Ricardo de 27/09/2026, a mesma dos
/// Indicadores Geográficos): o mês em curso aparece à parte, no cartão "Faturamento em curso", marcado como
/// parcial. Em novembro, o padrão é o ano fiscal que acabou de fechar, inteiro — até novembro fechar, as duas
/// telas falam do mesmo ano. O mês é o de São Paulo, e não o do UTC.</para>
/// </summary>
public sealed class ObterIndicadoresExecutivos(IRepositorioIndicadoresExecutivos repositorio, IRelogio relogio)
{
    /// <summary>O primeiro ano aceito: antes disso não há faturamento carregado.</summary>
    private const int PrimeiroAnoAceito = 2020;

    private static readonly CultureInfo Portugues = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Executa a apuração.</summary>
    /// <param name="ano">O ano CIVIL do cartão de meta × realizado — só quando pedido.</param>
    /// <param name="anoFiscal">O ano FISCAL (o ano em que ele termina). Nulo, e sem <paramref name="ano"/>, é o corrente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelExecutivoDaFilial>>> ExecutarAsync(
        int? ano, int? anoFiscal, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var mesCorrente = AnoFiscal.MesCorrenteEmSaoPaulo(agora);
        var ultimoFechado = AnoFiscal.AteOUltimoMesFechado(mesCorrente).Final;
        var erros = new ColetorDeErros();

        if (ano is not null && anoFiscal is not null)
            erros.Registrar("anoFiscal", "Peça o ano fiscal (anoFiscal) ou o civil (ano), e não os dois.",
                anoFiscal.Value.ToString(CultureInfo.InvariantCulture));

        // O CALENDÁRIO FISCAL É O PADRÃO (27/09/2026), e o ano padrão é o do ÚLTIMO MÊS FECHADO: em novembro, o
        // ano fiscal que acabou de fechar — o que ainda não tem mês fechado não tem o que somar.
        var calendario = ano is not null ? CalendarioDoAno.Civil : CalendarioDoAno.Fiscal;
        var anoPedido = ano ?? anoFiscal ?? AnoFiscal.Do(ultimoFechado);
        var ultimoAceito = calendario == CalendarioDoAno.Civil ? ultimoFechado.Year : AnoFiscal.Do(ultimoFechado);
        var campo = calendario == CalendarioDoAno.Civil ? "ano" : "anoFiscal";

        if (anoPedido < PrimeiroAnoAceito || anoPedido > ultimoAceito)
            erros.Registrar(campo, $"O ano vai de {PrimeiroAnoAceito} a {ultimoAceito}.",
                (ano ?? anoFiscal)?.ToString(CultureInfo.InvariantCulture));

        if (erros.TemErro)
            return erros.Recusar<ComProcedencia<PainelExecutivoDaFilial>>("A consulta tem parâmetros que não valem.");

        var inteiro = calendario == CalendarioDoAno.Fiscal
            ? AnoFiscal.Inteiro(anoPedido)
            : new JanelaDeCompetencia(new DateOnly(anoPedido, 1, 1), new DateOnly(anoPedido, 12, 1));

        // O ANO QUE AINDA CORRE PARA NO ÚLTIMO MÊS FECHADO — o mesmo AteOUltimoMesFechado dos Indicadores, no
        // fiscal; no civil, de janeiro até ele.
        var meses = inteiro.Final <= ultimoFechado
            ? inteiro
            : calendario == CalendarioDoAno.Fiscal
                ? AnoFiscal.AteOUltimoMesFechado(mesCorrente)
                : new JanelaDeCompetencia(inteiro.Inicial, ultimoFechado);

        var indicadores = await repositorio.ApurarAsync(anoPedido, calendario, meses, agora, ct);

        return Resultado<ComProcedencia<PainelExecutivoDaFilial>>.Ok(
            ComProcedencia<PainelExecutivoDaFilial>.DoNossoBanco(
                new PainelExecutivoDaFilial(indicadores, Lacunas(indicadores, mesCorrente, meses, inteiro)),
                "integracao.RegistroDeOrigem (ART) · comercial.FaturamentoDoCliente · comercial.FaturamentoSemCliente · " +
                "comercial.ClienteCarteira · processo.VendaPerdida",
                relogio));
    }

    /// <summary>Um texto com número e data no formato brasileiro, qualquer que seja a cultura do servidor.</summary>
    private static string Texto(FormattableString texto) => texto.ToString(Portugues);

    /// <summary>O que os cartões não dizem — cada frase com a medida que a prova.</summary>
    private static List<MetricaSemDado> Lacunas(
        IndicadoresExecutivosDaFilial i, DateOnly mesCorrente, JanelaDeCompetencia somado, JanelaDeCompetencia inteiro)
    {
        var lacunas = new List<MetricaSemDado>();

        // O ANO QUE AINDA CORRE vai até o último mês fechado, e a frase diz até quando — o mês em curso está no
        // cartão ao lado, marcado como parcial.
        if (somado.Final < inteiro.Final)
            lacunas.Add(new MetricaSemDado(
                "anoAteOUltimoMesFechado",
                Texto($"O realizado do ano vai de {JanelaDeCompetencia.Mes(somado.Inicial)} a {JanelaDeCompetencia.Mes(somado.Final)}, o último mês fechado: o mês em curso ({JanelaDeCompetencia.Mes(mesCorrente)}) está pela metade e fica à parte, na dica do cartão de faturamento. O ano vai até {JanelaDeCompetencia.Mes(inteiro.Final)}.")));

        // O FATURAMENTO DO ANO É O ART (decisão de 29/09/2026): as máquinas ENTREGUES, pelo valor de venda. Cada frase
        // abaixo diz o que o número dele não é.
        if (i.EntreguesNoAno is { } entregues)
        {
            if (entregues.Maquinas == 0)
                lacunas.Add(new MetricaSemDado(
                    "faturamentoPeloArt",
                    Texto($"Nenhuma máquina com entrega entre {JanelaDeCompetencia.Mes(entregues.Inicio)} e {JanelaDeCompetencia.Mes(entregues.Fim)} no ART, nesta filial. A data de entrega e o valor de venda passaram a ser gravados em 29/09/2026: até a primeira leitura do ART depois da publicação, as vendas que ainda não viraram venda no CRM não os têm.")));
            else if (entregues.SemValor == entregues.Maquinas)
                lacunas.Add(new MetricaSemDado(
                    "valorDeVendaNaoLido",
                    Texto($"As {entregues.Maquinas:N0} máquinas entregues no período ainda estão sem valor de venda: o valor do ART passou a ser lido em 29/09/2026 e chega na primeira leitura do ART depois da publicação. Até lá o faturamento não tem número.")));
            else if (entregues.SemValor > 0)
                lacunas.Add(new MetricaSemDado(
                    "maquinaSemValorNoArt",
                    Texto($"{entregues.SemValor:N0} das {entregues.Maquinas:N0} máquinas entregues não têm valor de venda no ART: contam nas máquinas, e não no faturamento.")));

            if (entregues.AguardandoNoCrm > 0)
                lacunas.Add(new MetricaSemDado(
                    "entregueAguardandoNoCrm",
                    Texto($"{entregues.AguardandoNoCrm:N0} das {entregues.Maquinas:N0} máquinas entregues ainda não viraram venda no CRM (comprador sem cadastro, chassi incompleto ou outro motivo da integração). Elas entram no faturamento, que é o ART inteiro, e não no realizado da meta, que conta só as vendas do CRM.")));
        }

        if (i.FaturamentoDoMes is not { } mes)
            lacunas.Add(new MetricaSemDado(
                "faturamentoDoMes",
                "Não há faturamento carregado para esta filial até o mês corrente. Ele vem das notas de saída do " +
                "Protheus, pela carga."));
        else if (mes.Competencia == mesCorrente)
            lacunas.Add(new MetricaSemDado(
                "mesEmCurso",
                Texto($"{mes.Competencia:MM/yyyy} ainda está em curso: o valor é o que a carga gravou até {mes.CarregadoEm:dd/MM/yyyy HH:mm} (UTC), e não o mês inteiro. Compará-lo com um mês fechado subestima o mês.")));
        else
            lacunas.Add(new MetricaSemDado(
                "mesCorrenteSemNota",
                Texto($"A competência mais recente carregada nesta filial é {mes.Competencia:MM/yyyy}: o mês corrente ainda não tem nota carregada. O cartão mostra o último mês que existe, e não um mês corrente zerado.")));

        lacunas.Add(new MetricaSemDado(
            "devolucoes",
            "Devolução e cancelamento não são abatidos: o valor é a nota de saída de venda (documento 32, P-5)."));

        // O CARTÃO A É O ART DESDE 29/09/2026 (decisão do Ricardo). A frase dizia que o ART "não entra neste faturamento
        // em reais" — valia quando o cartão era a nota do Protheus. O que continua valendo é que as duas não se somam.
        lacunas.Add(new MetricaSemDado(
            "notaDoProtheusNaoSomada",
            "O faturamento do ano é o valor de venda do ART das máquinas entregues. A nota de saída do Protheus " +
            "(máquina, peça e serviço) fica ao lado, como conferência, e não é somada: as duas medem a mesma máquina por " +
            "caminhos diferentes, e não fecham mês a mês (documento 36, cartão A)."));

        // O CALENDÁRIO FOI CONFIRMADO (24/09/2026) E VIROU O PADRÃO (27/09/2026). A lacuna dizia que ele "não foi
        // confirmado" e que por isso não havia FY; agora ela só aparece quando alguém pede o ano civil — e diz
        // que o civil é escolha, e não falta de calendário.
        if (i.Ano.Calendario == nameof(CalendarioDoAno.Civil))
            lacunas.Add(new MetricaSemDado(
                "calendarioCivil",
                Texto($"O ano do cartão é o CIVIL ({i.Ano.Ano}), porque foi o pedido. O padrão é o ano fiscal da Tracbel, de novembro a outubro, decidido em 27/09/2026.")));

        // A META SAIU DESTE CARTÃO (#138, 27/09/2026): a decidida é a de VENDA, em unidades, da API Gestão de Negócios, com
        // rota própria (/relatorios/metas) e as lacunas dela lá. A de faturamento nunca teve fonte — a frase que dizia
        // "nenhuma meta cadastrada (organizacao.Meta)" apontava para uma tabela que saiu na fase 1.

        lacunas.Add(new MetricaSemDado(
            "previsao",
            "Não há previsão do ano: nenhum modelo de projeção foi aprovado, e extrapolar o realizado pela média dos " +
            "meses não é previsão."));

        if (i.Carteira.SemDocumento > 0)
            lacunas.Add(new MetricaSemDado(
                "clienteSemDocumento",
                Texto($"{i.Carteira.SemDocumento:N0} de {i.Carteira.ClientesCadastradosComVinculo:N0} clientes em carteira cadastrados nesta filial não têm CPF/CNPJ: a identidade deles não se confere com o Protheus.")));

        if (i.Cobertura.TiposMarcadosComoVisita == 0)
            lacunas.Add(new MetricaSemDado(
                "visita",
                Texto($"O contato usado na cobertura vem da regra da BI de carteiras do Vórtice (53 resultados que contam como contato, em qualquer canal), apurada todo dia pela rotina das carteiras — e não dos {i.Cobertura.TiposDeAtividade:N0} tipos de atividade cadastrados no CRM (nenhum marcado como visita, ContaParaCobertura). O Vórtice registra o canal do contato, mas o CRM ainda não o carrega: por isso a cobertura não abre por canal (documento 32, P-2).")));

        lacunas.Add(new MetricaSemDado(
            "classeDeClienteDeOutraFilial",
            "Vínculo com cliente cadastrado em outra filial é medido pela cadência da classe D: a classe do cliente " +
            "pertence ao cadastro da outra filial, fora do alcance desta leitura. É a mesma regra do mapa de cobertura."));

        // A CAPTURA EXISTE (issue 162): máquinas vendidas sobre a demanda estimada, nos Indicadores Geográficos. O
        // que não existe é participação de mercado, que exigiria o total vendido por todos os fabricantes.
        lacunas.Add(new MetricaSemDado(
            "participacaoDeMercado",
            "Participação de mercado não é calculada: não há emplacamento, e nenhuma fonte aberta publica o total " +
            "vendido por todos os fabricantes. O que existe é a CAPTURA TRACBEL — máquinas vendidas sobre a demanda " +
            "estimada —, medida nos Indicadores Geográficos. Este cartão mostra só o que o CRM registra: as vendas " +
            "perdidas e para quem."));

        if (i.Mercado.VendasPerdidasRegistradas > i.Mercado.ComConcorrente)
            lacunas.Add(new MetricaSemDado(
                "concorrenteDaVendaPerdida",
                Texto($"{i.Mercado.VendasPerdidasRegistradas - i.Mercado.ComConcorrente:N0} de {i.Mercado.VendasPerdidasRegistradas:N0} vendas perdidas não declaram o concorrente.")));

        return lacunas;
    }
}
