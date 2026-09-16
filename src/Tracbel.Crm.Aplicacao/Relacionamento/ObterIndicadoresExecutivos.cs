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
/// OS CINCO CARTÕES DA VISÃO 360 (documento 36) — faturamento em curso, meta × realizado do ano,
/// clientes na carteira, cobertura pela cadência e o que o CRM sabe do mercado.
///
/// <para><b>Uma filial por chamada.</b> O consolidado das filiais é a tela somando as respostas
/// (ponte P-8 do documento 23); por isso cada número daqui é de uma partição que não se sobrepõe
/// entre filiais. O que não se soma, a documentação do contrato diz.</para>
///
/// <para><b>O ano é civil e vem do pedido</b>, com o ano corrente como padrão: o cartão não fica
/// preso a 2026, e não finge ser ano fiscal enquanto o calendário fiscal não for confirmado.</para>
/// </summary>
public sealed class ObterIndicadoresExecutivos(IRepositorioIndicadoresExecutivos repositorio, IRelogio relogio)
{
    /// <summary>O primeiro ano aceito: antes disso não há faturamento carregado.</summary>
    private const int PrimeiroAnoAceito = 2020;

    private static readonly CultureInfo Portugues = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Executa a apuração.</summary>
    /// <param name="ano">O ano civil do cartão de meta × realizado. Nulo é o ano corrente.</param>
    /// <param name="ct">Cancelamento.</param>
    public async Task<Resultado<ComProcedencia<PainelExecutivoDaFilial>>> ExecutarAsync(int? ano, CancellationToken ct)
    {
        var agora = relogio.Agora;
        var anoPedido = ano ?? agora.Year;

        if (anoPedido < PrimeiroAnoAceito || anoPedido > agora.Year)
        {
            var erros = new ColetorDeErros();
            erros.Registrar("ano", $"O ano vai de {PrimeiroAnoAceito} a {agora.Year}.", ano?.ToString(CultureInfo.InvariantCulture));
            return erros.Recusar<ComProcedencia<PainelExecutivoDaFilial>>("A consulta tem parâmetros que não valem.");
        }

        var indicadores = await repositorio.ApurarAsync(anoPedido, agora, ct);

        return Resultado<ComProcedencia<PainelExecutivoDaFilial>>.Ok(
            ComProcedencia<PainelExecutivoDaFilial>.DoNossoBanco(
                new PainelExecutivoDaFilial(indicadores, Lacunas(indicadores, agora)),
                "comercial.FaturamentoDoCliente · comercial.FaturamentoSemCliente · organizacao.Meta · " +
                "comercial.ClienteCarteira · processo.VendaPerdida",
                relogio));
    }

    /// <summary>Um texto com número e data no formato brasileiro, qualquer que seja a cultura do servidor.</summary>
    private static string Texto(FormattableString texto) => texto.ToString(Portugues);

    /// <summary>O que os cartões não dizem — cada frase com a medida que a prova.</summary>
    private static List<MetricaSemDado> Lacunas(IndicadoresExecutivosDaFilial i, DateTime agora)
    {
        var lacunas = new List<MetricaSemDado>();
        var mesCorrente = new DateOnly(agora.Year, agora.Month, 1);

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

        lacunas.Add(new MetricaSemDado(
            "vendaDeMaquinaNoArt",
            "O ART registra a venda de máquina, não a nota fiscal, e não está no banco do CRM. Ele não é somado a este " +
            "faturamento: os dois medem coisas diferentes e não fecham mês a mês (documento 36, cartão A)."));

        lacunas.Add(new MetricaSemDado(
            "calendarioFiscal",
            Texto($"O ano do cartão é o civil ({i.Ano.Ano}). O calendário fiscal não foi confirmado (documento 32, P-4) — por isso não há FY nem FYTD nestes números.")));

        if (i.Ano.MetasDaFilial == 0)
        {
            var texto = Texto($"Nenhuma meta de faturamento da filial cadastrada para {i.Ano.Ano} (organizacao.Meta). Sem meta, não há comparação: o cartão mostra só o realizado.");
            if (i.Ano.MetasDetalhadas > 0)
                texto += Texto($" Há {i.Ano.MetasDetalhadas} meta(s) de carteira, usuário ou linha no ano; elas não são somadas, para não contar o mesmo alvo duas vezes.");
            if (i.Ano.MetasQueCruzamOAno > 0)
                texto += Texto($" {i.Ano.MetasQueCruzamOAno} meta(s) cruzam o ano e não são rateadas.");

            lacunas.Add(new MetricaSemDado("metaDeFaturamento", texto));
        }

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
                Texto($"Nenhum dos {i.Cobertura.TiposDeAtividade:N0} tipos de atividade está marcado como visita (ContaParaCobertura). A cobertura conta o último contato registrado de qualquer tipo — inclusive registro gerado pelo sistema — e não visita (documento 32, P-2).")));

        lacunas.Add(new MetricaSemDado(
            "classeDeClienteDeOutraFilial",
            "Vínculo com cliente cadastrado em outra filial é medido pela cadência da classe D: a classe do cliente " +
            "pertence ao cadastro da outra filial, fora do alcance desta leitura. É a mesma regra do mapa de cobertura."));

        lacunas.Add(new MetricaSemDado(
            "participacaoDeMercado",
            "Participação de mercado não é calculada: não há emplacamento nem dado de mercado carregado. O cartão " +
            "mostra só o que o CRM registra — as vendas perdidas e para quem."));

        if (i.Mercado.VendasPerdidasRegistradas > i.Mercado.ComConcorrente)
            lacunas.Add(new MetricaSemDado(
                "concorrenteDaVendaPerdida",
                Texto($"{i.Mercado.VendasPerdidasRegistradas - i.Mercado.ComConcorrente:N0} de {i.Mercado.VendasPerdidasRegistradas:N0} vendas perdidas não declaram o concorrente.")));

        return lacunas;
    }
}
