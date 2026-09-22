using FluentAssertions;
using Tracbel.Crm.Carga;
using Tracbel.Crm.Integracao.Ibge;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Carga;

/// <summary>
/// A SÉRIE LONGA DA PAM E O MILHO POR SAFRA (issue 156) — as duas regras que decidem o que a rodada
/// anual faz, e que só se vê quando a fonte tem quinze anos em vez de três.
///
/// <list type="number">
///   <item><b>A segunda rodada não grava nada.</b> O ano que já está no banco não é relido; releem-se
///   apenas os anos recentes, que o IBGE ainda revisa. Sem isso, a rotina anual rebaixaria quinze anos
///   de 645 municípios × 85 produtos toda vez, para regravar o que já está igual.</item>
///   <item><b>1ª + 2ª safra tem de bater com o milho da PAM.</b> São duas pesquisas do mesmo
///   instituto, e é essa conferência que prova que a separação por safra pode ser usada para
///   descontar a terra que soja e milho compartilham.</item>
/// </list>
///
/// <para>Os códigos são os reais do SIDRA (114253 e 114254 na classificação 81; 40122 é o milho da
/// 782); as áreas são inventadas.</para>
/// </summary>
[Trait("Categoria", "Carga")]
public sealed class SerieLongaDaPamTestes
{
    private const short UltimoAno = 2025;
    private const int RibeiraoPreto = 3543402;
    private const int Serrana = 3551504;

    private static readonly IReadOnlyDictionary<int, int> Catalogo =
        new Dictionary<int, int> { [RibeiraoPreto] = 1, [Serrana] = 2 };

    private static LinhaDaProducaoAgricola DaPam(int municipio, int produto, string? plantada) =>
        new(municipio, UltimoAno, produto, "Milho (em grão)", plantada, null, null, null);

    private static LinhaDaProducaoAgricola DaSafra(int municipio, int safra, string? plantada) =>
        new(municipio, UltimoAno, safra, $"Milho (em grão) - safra {safra}", plantada, null, null, null);

    // =============================================================================================
    // Quais anos a rodada lê
    // =============================================================================================

    [Fact]
    public void A_primeira_rodada_le_a_serie_inteira_do_ano_pedido_ate_o_ultimo()
    {
        var anos = CargaDeTerritorio.AnosASeremLidos(2010, UltimoAno, new HashSet<short>());

        anos.Should().HaveCount(16, "de 2010 a 2025, inclusive");
        anos[0].Should().Be(2025, "do mais recente para o mais antigo: o ano que interessa entra primeiro");
        anos[^1].Should().Be(2010);
    }

    [Fact]
    public void A_segunda_rodada_le_so_a_janela_que_o_ibge_ainda_revisa()
    {
        // O CRITÉRIO DE ACEITE DA ISSUE: com a série inteira no banco, a rodada seguinte não sai
        // buscando quinze anos de novo. Os três recentes voltam porque o IBGE os revisa — é o que
        // impede a revisão de passar batida (issue 153).
        var jaCarregados = new HashSet<short>();
        for (short ano = 2010; ano <= UltimoAno; ano++) jaCarregados.Add(ano);

        var anos = CargaDeTerritorio.AnosASeremLidos(2010, UltimoAno, jaCarregados);

        anos.Should().Equal([(short)2025, (short)2024, (short)2023]);
    }

    [Fact]
    public void O_ano_que_falta_no_meio_da_serie_entra_sozinho()
    {
        // Série contínua é o que a issue pede: um buraco no meio — uma carga interrompida, um ano que
        // o IBGE publicou depois — é preenchido sem reler tudo.
        var jaCarregados = new HashSet<short>();
        for (short ano = 2010; ano <= UltimoAno; ano++) if (ano != 2016) jaCarregados.Add(ano);

        var anos = CargaDeTerritorio.AnosASeremLidos(2010, UltimoAno, jaCarregados);

        anos.Should().Equal([(short)2025, (short)2024, (short)2023, (short)2016]);
    }

    // =============================================================================================
    // A soma das safras contra o milho da PAM
    // =============================================================================================

    [Fact]
    public void A_soma_das_duas_safras_bate_com_o_milho_da_pam()
    {
        var conta = new CargaDeTerritorio.ContagemDaPam();

        CargaDeTerritorio.ConferirOMilhoContraAPam(
            [DaPam(RibeiraoPreto, 40122, "5000")],
            [DaSafra(RibeiraoPreto, 114253, "1000"), DaSafra(RibeiraoPreto, 114254, "4000")],
            Catalogo, conta);

        conta.MilhoConferidoComAPam.Should().Be(1);
        conta.MilhoDivergenteDaPam.Should().Be(0);
        conta.Recusas.Should().BeEmpty();
    }

    [Fact]
    public void O_arredondamento_de_ate_um_hectare_nao_vira_aviso()
    {
        // As duas tabelas são pesquisas diferentes, publicadas com arredondamento próprio. Tratar um
        // hectare de diferença como erro encheria o painel de fontes de aviso que ninguém pode agir.
        var conta = new CargaDeTerritorio.ContagemDaPam();

        CargaDeTerritorio.ConferirOMilhoContraAPam(
            [DaPam(RibeiraoPreto, 40122, "5000")],
            [DaSafra(RibeiraoPreto, 114253, "1000"), DaSafra(RibeiraoPreto, 114254, "4001")],
            Catalogo, conta);

        conta.MilhoConferidoComAPam.Should().Be(1);
        conta.Recusas.Should().BeEmpty();
    }

    [Fact]
    public void A_divergencia_de_verdade_vira_recusa_registrada_e_o_dado_continua_gravado()
    {
        // AVISO NÃO É DESCARTE: as duas leituras ficam no banco, e a diferença vai para o painel de
        // fontes com os dois números, para alguém conferir.
        var conta = new CargaDeTerritorio.ContagemDaPam();

        CargaDeTerritorio.ConferirOMilhoContraAPam(
            [DaPam(RibeiraoPreto, 40122, "5000")],
            [DaSafra(RibeiraoPreto, 114253, "1000"), DaSafra(RibeiraoPreto, 114254, "9000")],
            Catalogo, conta);

        conta.MilhoDivergenteDaPam.Should().Be(1);
        conta.Recusas.Should().HaveCount(1);
        conta.Recusas[0].Motivo.Should().Contain("10000").And.Contain("5000");
    }

    [Fact]
    public void Municipio_sem_milho_na_pam_ou_sob_sigilo_nao_vira_divergencia()
    {
        // Sem o termo de comparação não há o que conferir — e "não divulgado" não é zero. Contar isso
        // como divergência transformaria o sigilo do IBGE em erro da carga.
        var conta = new CargaDeTerritorio.ContagemDaPam();

        CargaDeTerritorio.ConferirOMilhoContraAPam(
            [DaPam(Serrana, 40122, "X")],
            [DaSafra(Serrana, 114253, "300"), DaSafra(RibeiraoPreto, 114253, "1000")],
            Catalogo, conta);

        conta.MilhoConferidoComAPam.Should().Be(0);
        conta.MilhoDivergenteDaPam.Should().Be(0);
        conta.Recusas.Should().BeEmpty();
    }

    [Fact]
    public void Sem_milho_por_safra_no_ano_nao_ha_conferencia_nenhuma()
    {
        // A 839 começa em 2003; nos anos anteriores a lista chega vazia, e vazio não é divergência.
        var conta = new CargaDeTerritorio.ContagemDaPam();

        CargaDeTerritorio.ConferirOMilhoContraAPam([DaPam(RibeiraoPreto, 40122, "5000")], [], Catalogo, conta);

        conta.MilhoConferidoComAPam.Should().Be(0);
        conta.Recusas.Should().BeEmpty();
    }
}
