using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Processo;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Integracao;

/// <summary>
/// A CLASSIFICAÇÃO DOS RESULTADOS DO VÓRTICE (decisões de 27/09/2026, documento 52 §2.2). O que estes testes prendem:
/// guardar o MAIOR estágio de cada código e contar "alcançou E" como "estágio ≥ E" reproduz as seis listas do extrator
/// do BI, código a código — com 2607/2609/2610 também na Cobertura —; e a lista do contato é a da view
/// <c>BI_CARTEIRA_VN</c>, 53 códigos, na ordem da view.
/// </summary>
public sealed class ClassificacaoDeResultadoDoVorticeTestes
{
    // AS LISTAS DO EXTRATOR, copiadas do .qvs (l. 257, 384, 520–528, 666–672, 803–805, 936) — a especificação.
    private static readonly int[] Fat = [2529, 2530, 3440, 3494];
    private static readonly int[] ListaDoLead = [1278, 3239, .. Fat];
    private static readonly int[] ListaDoQualificado = [3803, 3239, .. Fat];

    private static readonly int[] ListaDaCobertura =
    [
        250, 252, 254, 255, 260, 263, 265, 267, 299, 300, 304, 305, 306, 307, 570, 1286, 3223, 3225, 3227, 3231, 3232, 3663,
        3234, 3639, 3640, 2605, 1929, 2612, 2547, 2553, 2554, 2548, 2563, 2564, 2565, 2566, 2568, 3572, 3573, 3239, .. Fat
    ];

    private static readonly int[] ListaDaNegociacao =
        [2548, 2563, 2564, 2565, 2566, 2568, 3231, 3232, 3234, 3572, 3573, 3663, 2612, 3223, 2607, 2609, 2610, 3239, .. Fat];

    private static readonly int[] ListaDoPedido = [2548, 3231, 3232, 3239, 3663, .. Fat];

    private static IEnumerable<int> QueAlcancam(EstagioDoFunil estagio) =>
        ClassificacaoDeResultadoDoVortice.Semente.Where(i => i.Estagio >= estagio).Select(i => i.Codigo);

    [Fact]
    public void Do_Cobertura_ao_Faturamento_a_semente_reproduz_as_listas_do_extrator_com_os_tres_da_Negociacao_na_Cobertura()
    {
        QueAlcancam(EstagioDoFunil.Cobertura).Should().BeEquivalentTo(ListaDaCobertura.Concat([2607, 2609, 2610]),
            "2607/2609/2610 são de Negociação, e o extrator os esquece na Cobertura (decisão de 27/09/2026)");
        QueAlcancam(EstagioDoFunil.Negociacao).Should().BeEquivalentTo(ListaDaNegociacao);
        QueAlcancam(EstagioDoFunil.Pedido).Should().BeEquivalentTo(ListaDoPedido);
        QueAlcancam(EstagioDoFunil.Faturamento).Should().BeEquivalentTo(Fat);
    }

    [Fact]
    public void Lead_e_Qualificado_sao_cumulativos_e_a_entrada_digital_de_cada_um_e_o_codigo_proprio()
    {
        // No BI, Lead = 1278 + venda aprovada + faturamento; aqui é cumulativo: toda a lista acima também conta.
        QueAlcancam(EstagioDoFunil.Lead).Should().Contain(ListaDoLead).And.Contain(ListaDaCobertura).And.Contain(3803);
        QueAlcancam(EstagioDoFunil.Qualificado).Should().Contain(ListaDoQualificado).And.NotContain(1278);

        ClassificacaoDeResultadoDoVortice.Semente.Where(i => i.Estagio == EstagioDoFunil.Lead).Select(i => i.Codigo).Should().Equal(1278);
        ClassificacaoDeResultadoDoVortice.Semente.Where(i => i.Estagio == EstagioDoFunil.Qualificado).Select(i => i.Codigo).Should().Equal(3803);
    }

    [Fact]
    public void A_semente_tem_um_codigo_por_linha_e_os_53_do_contato()
    {
        var semente = ClassificacaoDeResultadoDoVortice.Semente;

        semente.Select(i => i.Codigo).Should().OnlyHaveUniqueItems();
        semente.Should().HaveCount(63, "49 códigos de estágio e 14 que só contam como contato");
        semente.Count(i => i.ContaComoContato).Should().Be(53);
        semente.Where(i => i.ContaComoContato).Select(i => i.Codigo)
            .Should().BeEquivalentTo(ClassificacaoDeResultadoDoVortice.ResultadosQueContamComoContato);
        semente.Should().OnlyContain(i => i.Fonte.Length > 0 && i.Fonte.Length <= 200);
    }

    [Fact]
    public void A_lista_do_contato_e_a_das_linhas_86_e_87_da_view_da_BI_codigo_a_codigo_e_na_mesma_ordem()
    {
        // É A MESMA VERIFICAÇÃO do PR #244 sobre a constante LeitorDeCarteirasDoVortice.ResultadosQueContamComoContato;
        // quando ele entrar na main, as duas listas ficam presas à mesma view — e um teste passa a compará-las entre si.
        var view = File.ReadAllLines(Path.Combine(RaizDoRepositorio(), "docs", "extracao-vortice", "modulos", "views", "BI_CARTEIRA_VN.sql"));
        var trecho = view[85] + " " + view[86];
        trecho.Should().Contain("HIS.RESULTADO IN");

        var codigos = Regex.Match(trecho, @"IN\s*\(([^)]*)\)").Groups[1].Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(c => int.Parse(c, CultureInfo.InvariantCulture))
            .ToList();

        ClassificacaoDeResultadoDoVortice.ResultadosQueContamComoContato.Should().Equal(codigos);
        codigos.Should().HaveCount(53).And.OnlyHaveUniqueItems("são 53 distintos, não 55 (conferido em 27/09/2026)");
    }

    private static string RaizDoRepositorio()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);
        while (atual is not null && !atual.EnumerateFiles("*.sln").Any()) atual = atual.Parent;
        return atual?.FullName ?? throw new InvalidOperationException("Não encontrei a raiz do repositório (nenhum .sln acima).");
    }
}
