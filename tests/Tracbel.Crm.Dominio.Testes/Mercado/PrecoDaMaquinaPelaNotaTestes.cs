using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// O PREÇO DA MÁQUINA PELA NOTA (issue 70, D-P12, decidida pelo Ricardo em 27/09/2026): a venda do ART casada com o item
/// de máquina da nota do Protheus pela filial e pelo número, e a mediana por categoria e mês.
///
/// <para>O que estes testes seguram é a regra "na dúvida, fica de fora, contado": duas máquinas na mesma nota, o mesmo
/// número em duas séries, uma emissão a meses da venda — nenhum desses vira o preço de outra máquina.</para>
/// </summary>
public sealed class PrecoDaMaquinaPelaNotaTestes
{
    private const string Ribeirao = "010101";
    private const string Bebedouro = "010109";
    private const int Trator = 1;
    private const int Colheitadeira = 2;

    private static ItemDeMaquinaNaNota Item(string filial, string documento, DateOnly emissao, decimal total,
        decimal quantidade = 1m, string serie = "1", string item = "01") =>
        new(filial, serie, documento, item, emissao, quantidade, total);

    private static VendaComNota Venda(string? filial, string? nota, DateOnly? data, int? categoria = Trator) =>
        new(filial, nota, data, categoria);

    [Theory]
    [InlineData("000048351", "48351")]
    [InlineData("48351", "48351")]
    [InlineData(" 1-000048351 ", "1000048351")]
    [InlineData("000000000", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void O_numero_da_nota_vira_so_digitos_sem_os_zeros_a_esquerda(string? numero, string? esperado) =>
        PrecoDaMaquinaPelaNota.NumeroNormalizado(numero).Should().Be(esperado);

    [Fact]
    public void A_venda_casa_com_o_item_da_mesma_filial_e_do_mesmo_numero_e_o_valor_e_por_unidade()
    {
        // O PROTHEUS GUARDA 000048351, O ART TRAZ 48351 — e a nota de duas máquinas iguais dá o valor de uma.
        var (precos, desfechos) = PrecoDaMaquinaPelaNota.Casar(
            [Venda(Ribeirao, "48351", new DateOnly(2026, 3, 10))],
            [Item(Ribeirao, "000048351", new DateOnly(2026, 3, 8), 900_000m, quantidade: 2m)]);

        precos.Should().ContainSingle().Which.Should().Be((Trator, new DateOnly(2026, 3, 8), 450_000m));
        desfechos[DesfechoDoCasamento.Casada].Should().Be(1);
    }

    [Fact]
    public void O_mesmo_numero_em_outra_filial_nao_e_a_nota_da_venda()
    {
        var (precos, desfechos) = PrecoDaMaquinaPelaNota.Casar(
            [Venda(Ribeirao, "48351", new DateOnly(2026, 3, 10))],
            [Item(Bebedouro, "000048351", new DateOnly(2026, 3, 8), 900_000m)]);

        precos.Should().BeEmpty();
        desfechos[DesfechoDoCasamento.SemItemNaNota].Should().Be(1);
    }

    [Fact]
    public void Duas_maquinas_na_mesma_nota_ficam_de_fora_porque_nao_se_sabe_qual_valor_e_de_qual()
    {
        var (precos, desfechos) = PrecoDaMaquinaPelaNota.Casar(
            [Venda(Ribeirao, "48351", new DateOnly(2026, 3, 10))],
            [
                Item(Ribeirao, "000048351", new DateOnly(2026, 3, 8), 900_000m, item: "01"),
                Item(Ribeirao, "000048351", new DateOnly(2026, 3, 8), 300_000m, item: "02")
            ]);

        precos.Should().BeEmpty();
        desfechos[DesfechoDoCasamento.Ambigua].Should().Be(1);
    }

    [Fact]
    public void O_mesmo_numero_emitido_longe_da_venda_e_outra_nota_e_so_a_perto_casa()
    {
        // O NÚMERO SE REPETE AO LONGO DOS ANOS: a nota de 2024 com o mesmo número não é a desta venda.
        var vendaDe2026 = Venda(Ribeirao, "48351", new DateOnly(2026, 3, 10));

        var (precos, _) = PrecoDaMaquinaPelaNota.Casar(
            [vendaDe2026],
            [
                Item(Ribeirao, "000048351", new DateOnly(2024, 5, 2), 700_000m),
                Item(Ribeirao, "000048351", new DateOnly(2026, 3, 8), 900_000m)
            ]);
        precos.Should().ContainSingle().Which.ValorUnitario.Should().Be(900_000m);

        var (nenhum, desfechos) = PrecoDaMaquinaPelaNota.Casar(
            [vendaDe2026], [Item(Ribeirao, "000048351", new DateOnly(2024, 5, 2), 700_000m)]);
        nenhum.Should().BeEmpty();
        desfechos[DesfechoDoCasamento.ForaDaData].Should().Be(1);
    }

    [Fact]
    public void Usado_venda_sem_nota_e_venda_sem_data_ficam_de_fora_contadas()
    {
        var (precos, desfechos) = PrecoDaMaquinaPelaNota.Casar(
            [
                Venda(Ribeirao, "48351", new DateOnly(2026, 3, 10), categoria: null),
                Venda(Ribeirao, null, new DateOnly(2026, 3, 10)),
                Venda(null, "48351", new DateOnly(2026, 3, 10)),
                Venda(Ribeirao, "48351", null)
            ],
            [Item(Ribeirao, "000048351", new DateOnly(2026, 3, 8), 900_000m)]);

        precos.Should().BeEmpty();
        desfechos[DesfechoDoCasamento.SemCategoria].Should().Be(1, "usado não é preço de máquina nova");
        desfechos[DesfechoDoCasamento.SemNota].Should().Be(2);
        desfechos[DesfechoDoCasamento.SemData].Should().Be(1);
    }

    [Fact]
    public void Duas_vendas_que_apontam_a_mesma_nota_nao_contam_o_mesmo_preco_duas_vezes()
    {
        var (precos, desfechos) = PrecoDaMaquinaPelaNota.Casar(
            [Venda(Ribeirao, "48351", new DateOnly(2026, 3, 10)), Venda(Ribeirao, "048351", new DateOnly(2026, 3, 11))],
            [Item(Ribeirao, "000048351", new DateOnly(2026, 3, 8), 900_000m)]);

        precos.Should().ContainSingle();
        desfechos[DesfechoDoCasamento.Ambigua].Should().Be(1, "a duplicata da origem não repete o preço");
    }

    [Theory]
    [InlineData(new[] { 5.0 }, 5.0)]
    [InlineData(new[] { 1.0, 9.0, 5.0 }, 5.0)]
    [InlineData(new[] { 1.0, 3.0, 5.0, 100.0 }, 4.0)]
    public void A_mediana_e_o_valor_do_meio_ou_a_media_dos_dois_do_meio(double[] valores, double esperada) =>
        PrecoDaMaquinaPelaNota.Mediana([.. valores.Select(v => (decimal)v)]).Should().Be((decimal)esperada);

    [Fact]
    public void O_preco_do_mes_e_por_categoria_com_a_mediana_o_menor_o_maior_e_quantas_notas()
    {
        var porMes = PrecoDaMaquinaPelaNota.PorMes(
        [
            (Trator, new DateOnly(2026, 3, 2), 400_000m),
            (Trator, new DateOnly(2026, 3, 20), 500_000m),
            (Trator, new DateOnly(2026, 3, 25), 2_000_000m),
            (Colheitadeira, new DateOnly(2026, 3, 5), 3_000_000m),
            (Trator, new DateOnly(2026, 4, 1), 450_000m)
        ]);

        porMes.Should().Equal(
            new PrecoDaCategoriaNoMes(Trator, new DateOnly(2026, 3, 1), 500_000m, 400_000m, 2_000_000m, 3),
            new PrecoDaCategoriaNoMes(Trator, new DateOnly(2026, 4, 1), 450_000m, 450_000m, 450_000m, 1),
            new PrecoDaCategoriaNoMes(Colheitadeira, new DateOnly(2026, 3, 1), 3_000_000m, 3_000_000m, 3_000_000m, 1));
    }

    [Fact]
    public void O_preco_de_referencia_e_a_mediana_dos_meses_dos_ultimos_doze_contando_o_corrente()
    {
        // Em 27/09/2026 a janela vai de out/2025 a set/2026: setembro de 2025 fica de fora, por mais barato que seja.
        var referencia = PrecoDaMaquinaPelaNota.PrecoDeReferencia(
            "TRATOR",
            "Trator",
            [
                (new DateOnly(2025, 9, 1), 100_000m),
                (new DateOnly(2025, 10, 1), 500_000m),
                (new DateOnly(2026, 3, 1), 520_000m),
                (new DateOnly(2026, 9, 1), 2_000_000m)
            ],
            new DateOnly(2026, 9, 27));

        referencia.Should().Be(new PrecoDeReferenciaDaCategoria("TRATOR", "Trator", 520_000m, 3, new DateOnly(2026, 9, 1)),
            "cada mês é um voto, e o mês fora da curva (R$ 2 milhões) não arrasta a mediana");
    }

    [Fact]
    public void Categoria_sem_nota_nos_ultimos_doze_meses_nao_tem_preco_de_referencia() =>
        PrecoDaMaquinaPelaNota.PrecoDeReferencia("TRATOR", "Trator", [(new DateOnly(2025, 6, 1), 480_000m)], new DateOnly(2026, 9, 27))
            .Should().BeNull("um preço de mais de um ano, aplicado à demanda de hoje, seria número com cara de atual");

    [Fact]
    public void O_mes_gravado_so_muda_o_carimbo_quando_algum_numero_muda()
    {
        var gravado = new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);
        var preco = PrecoDeMaquinaNoMes.Registrar(Trator, new DateOnly(2026, 3, 15), 500_000m, 400_000m, 900_000m, 3,
            PrecoDeMaquinaNoMes.FonteDaNota, 100, gravado);

        preco.Mes.Should().Be(new DateOnly(2026, 3, 1));
        preco.Revisar(500_000m, 400_000m, 900_000m, 3, 200, gravado.AddDays(1)).Should().BeFalse();
        preco.ImportadoEm.Should().Be(gravado);

        preco.Revisar(520_000m, 400_000m, 900_000m, 4, 200, gravado.AddDays(1)).Should().BeTrue();
        preco.Notas.Should().Be(4);
        preco.ImportadoPorId.Should().Be(200);
    }

    [Fact]
    public void Mes_sem_nota_ou_com_mediana_fora_da_faixa_nao_e_preco()
    {
        var agora = DateTime.UtcNow;

        var semNota = () => PrecoDeMaquinaNoMes.Registrar(Trator, new DateOnly(2026, 3, 1), 1m, 1m, 1m, 0, "X", 1, agora);
        var foraDaFaixa = () => PrecoDeMaquinaNoMes.Registrar(Trator, new DateOnly(2026, 3, 1), 10m, 20m, 30m, 1, "X", 1, agora);

        semNota.Should().Throw<RegraDeNegocioViolada>();
        foraDaFaixa.Should().Throw<RegraDeNegocioViolada>();
    }
}
