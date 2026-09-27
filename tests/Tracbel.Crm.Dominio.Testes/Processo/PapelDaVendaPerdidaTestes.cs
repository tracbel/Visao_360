using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Processo;
using Xunit;
using F = Tracbel.Crm.Dominio.Processo.FormulariosDaVendaPerdida;

namespace Tracbel.Crm.Dominio.Testes.Processo;

/// <summary>
/// OS PAPÉIS DA VENDA PERDIDA (decisões de 27/09/2026, documento 52 §4): o <c>_JDE</c> gêmeo do antigo é duplicata; no
/// par FY25 × <c>SEM_PARTICIPACAO</c>, o FY25 é o principal com participação "Não"; os <c>VP_*</c> são complemento do
/// FY25, e o solto é principal; e só a principal conta.
/// </summary>
public sealed class PapelDaVendaPerdidaTestes
{
    private static DateTime Em(int ano, int mes, int dia, int hora = 14) => new(ano, mes, dia, hora, 0, 0, DateTimeKind.Utc);

    private static RespostaNaRegraDoPapel R(long chave, string formulario, long pessoa, DateTime quando, long? processo = null) =>
        new(chave, formulario, pessoa, processo, quando);

    [Fact]
    public void O_JDE_gemeo_do_antigo_e_duplicata_do_antigo_e_o_solto_e_principal()
    {
        var papeis = RegraDoPapelDaVendaPerdida.Decidir(
        [
            R(1, F.Antigo, 100, Em(2014, 5, 10, 13)),
            R(2, F.Jde, 100, Em(2014, 5, 10, 18)),     // mesma pessoa, mesmo dia: o gêmeo
            R(3, F.Jde, 200, Em(2014, 5, 10, 18)),     // outra pessoa
            R(4, F.Jde, 100, Em(2014, 5, 11, 18))      // outro dia
        ]);

        papeis[1].Should().Be(new PapelDecidido(PapelDaVendaPerdida.Principal, null, false));
        papeis[2].Should().Be(new PapelDecidido(PapelDaVendaPerdida.Duplicata, 1, false));
        papeis[3].Papel.Should().Be(PapelDaVendaPerdida.Principal);
        papeis[4].Papel.Should().Be(PapelDaVendaPerdida.Principal);
    }

    [Fact]
    public void O_dia_do_par_e_o_de_Sao_Paulo()
    {
        // 23:30 de 10/05 em São Paulo é 02:30 de 11/05 em UTC: o mesmo dia para quem preencheu.
        var papeis = RegraDoPapelDaVendaPerdida.Decidir(
        [
            R(1, F.Antigo, 100, Em(2014, 5, 10, 13)),
            R(2, F.Jde, 100, new DateTime(2014, 5, 11, 2, 30, 0, DateTimeKind.Utc))
        ]);

        papeis[2].Papel.Should().Be(PapelDaVendaPerdida.Duplicata);
    }

    [Fact]
    public void No_par_FY25_e_SEM_PARTICIPACAO_o_FY25_e_o_principal_com_participacao_negada()
    {
        var papeis = RegraDoPapelDaVendaPerdida.Decidir(
        [
            R(10, F.Fy25, 300, Em(2025, 9, 1), processo: 5000),
            R(11, F.SemParticipacao, 300, Em(2025, 9, 20), processo: 5000),   // mesmo processo, outro dia: é o par
            R(12, F.SemParticipacao, 300, Em(2025, 9, 1), processo: 5001)     // outro processo: não é
        ]);

        papeis[10].Should().Be(new PapelDecidido(PapelDaVendaPerdida.Principal, null, true));
        papeis[11].Should().Be(new PapelDecidido(PapelDaVendaPerdida.Duplicata, 10, false));
        papeis[12].Papel.Should().Be(PapelDaVendaPerdida.Principal, "o SEM_PARTICIPACAO sem FY25 é a perda dele mesmo");
    }

    [Fact]
    public void O_VP_acompanha_o_FY25_como_complemento_e_o_solto_e_principal()
    {
        var papeis = RegraDoPapelDaVendaPerdida.Decidir(
        [
            R(20, F.Fy25, 400, Em(2025, 10, 1), processo: 6000),
            R(21, F.VpTrator, 400, Em(2025, 10, 1), processo: 6000),
            R(22, F.VpColheitadeira, 400, Em(2025, 10, 1), processo: 6000),
            R(23, F.VpPlantadeira, 500, Em(2025, 10, 1), processo: 6001)      // sem FY25
        ]);

        papeis[21].Should().Be(new PapelDecidido(PapelDaVendaPerdida.Complemento, 20, false));
        papeis[22].Should().Be(new PapelDecidido(PapelDaVendaPerdida.Complemento, 20, false));
        papeis[23].Papel.Should().Be(PapelDaVendaPerdida.Principal);
        papeis[20].Papel.Should().Be(PapelDaVendaPerdida.Principal);
    }

    [Fact]
    public void Entre_candidatos_vale_o_mais_proximo_no_tempo_e_no_empate_o_menor_questionario()
    {
        var papeis = RegraDoPapelDaVendaPerdida.Decidir(
        [
            R(30, F.Antigo, 100, Em(2014, 5, 10, 11)),
            R(31, F.Antigo, 100, Em(2014, 5, 10, 16)),
            R(32, F.Jde, 100, Em(2014, 5, 10, 15)),    // mais perto do 31
            R(40, F.Fy25, 700, Em(2025, 11, 5, 12)),
            R(41, F.Fy25, 700, Em(2025, 11, 5, 16)),
            R(42, F.VpTrator, 700, Em(2025, 11, 5, 14))   // a duas horas dos dois: vale o menor
        ]);

        papeis[32].ChaveDaPrincipal.Should().Be(31);
        papeis[42].ChaveDaPrincipal.Should().Be(40);
    }

    [Fact]
    public void Com_processo_so_de_um_lado_o_par_e_pelo_dia()
    {
        var papeis = RegraDoPapelDaVendaPerdida.Decidir(
        [
            R(50, F.Fy25, 800, Em(2025, 11, 5), processo: 7000),
            R(51, F.SemParticipacao, 800, Em(2025, 11, 5))
        ]);

        papeis[51].Should().Be(new PapelDecidido(PapelDaVendaPerdida.Duplicata, 50, false));
    }

    // =============================================================================================
    // A entidade
    // =============================================================================================

    private static ConteudoDaVendaPerdida Conteudo(string formulario = F.Fy25) => new(
        1, new DateTime(2025, 9, 1, 13, 0, 0, 3, DateTimeKind.Utc).AddTicks(3333), new DateOnly(2025, 8, 30), 1, null, null, null,
        null, "8R 410", "8R 370", 1, 1_500_000m, 1_450_000m, ParticipacaoNaNegociacao.NaoInformado, formulario, 5000);

    [Fact]
    public void So_a_principal_nao_excluida_conta()
    {
        var principal = VendaPerdida.DaOrigem(Conteudo(), 1);
        principal.Conta.Should().BeTrue();

        var duplicata = VendaPerdida.DaOrigem(Conteudo(F.SemParticipacao), 1);
        duplicata.DefinirPapel(PapelDaVendaPerdida.Duplicata, 99, 1).Should().BeTrue();
        duplicata.Conta.Should().BeFalse();

        principal.Excluir(1);
        principal.Conta.Should().BeFalse();
        principal.RestaurarDaOrigem(1).Should().BeTrue();
        principal.Conta.Should().BeTrue();
    }

    [Fact]
    public void O_papel_so_aceita_a_combinacao_coerente_com_a_principal()
    {
        var venda = VendaPerdida.DaOrigem(Conteudo(), 1);

        FluentActions.Invoking(() => venda.DefinirPapel(PapelDaVendaPerdida.Principal, 5, 1)).Should().Throw<RegraDeNegocioViolada>();
        FluentActions.Invoking(() => venda.DefinirPapel(PapelDaVendaPerdida.Complemento, null, 1)).Should().Throw<RegraDeNegocioViolada>();
        venda.DefinirPapel(PapelDaVendaPerdida.Principal, null, 1).Should().BeFalse("já era principal");
    }

    [Fact]
    public void A_origem_relida_igual_nao_altera_nada_mesmo_com_a_data_do_Vortice_fora_do_milissegundo()
    {
        var venda = VendaPerdida.DaOrigem(Conteudo(), 1);
        venda.RegistradaEm.Ticks.Should().Be(new DateTime(2025, 9, 1, 13, 0, 0, 3, DateTimeKind.Utc).Ticks, "datetime2(3) guarda o milissegundo");

        venda.AtualizarDaOrigem(Conteudo(), 1).Should().BeFalse();
        venda.AtualizarDaOrigem(Conteudo() with { PrecoDoConcorrente = 1_400_000m }, 1).Should().BeTrue();
        venda.PrecoDoConcorrente.Should().Be(1_400_000m);
    }

    [Fact]
    public void Formulario_fora_da_lista_e_recusado()
    {
        FluentActions.Invoking(() => VendaPerdida.DaOrigem(Conteudo("IV_Q_VENDA_PERDIDA_MANITO"), 1))
            .Should().Throw<RegraDeNegocioViolada>("o _MANITO (Colorado) fica fora — decisão de 27/09/2026");
        F.Todos.Should().HaveCount(12).And.OnlyHaveUniqueItems();
    }
}
