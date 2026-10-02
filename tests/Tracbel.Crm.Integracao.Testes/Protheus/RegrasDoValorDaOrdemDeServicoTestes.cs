using FluentAssertions;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Protheus;

/// <summary>
/// A RÉGUA DO VALOR DA ORDEM DE SERVIÇO, portada do script do extrator "Pós Vendas Serviços" do BI. Cada teste é uma
/// linha do script: o desconto da peça contado uma vez por peça, o serviço que vale zero (RAT, FDLI, RSF), o km de
/// socorro, o serviço de terceiro, a hora com o desconto rateado — e o nulo do Qlik, que tira a linha da soma.
/// </summary>
public sealed class RegrasDoValorDaOrdemDeServicoTestes
{
    private static ItemDaOrdemNaOrigem Peca(
        string codigo = "PECA-1", string requisicao = "00000001", decimal? quantidade = 1m, decimal? unitario = 100m, decimal? desconto = 0m,
        string pecaOuServico = "PEÇ", string tipoDeTempo = "C") =>
        new("010101", "00001234", "A", "OFICINA", new DateOnly(2026, 9, 1), null, null, null, "1PY6155MCSS000001", "6155M", 1200m,
            "52998224725", pecaOuServico, tipoDeTempo, codigo, requisicao, quantidade, unitario, desconto, null, "1001", null, null);

    private static ServicoExecutadoNaOrigem Servico(
        string? tpServico = "MO", string? vokIncMob = "1", string? sitTpo = "1", string? tipTem = "C", string? descTpTpo = "CLIENTE",
        string? codServico = "REVISAO", decimal? temPad = 2m, decimal? temTra = 2m, decimal? temCob = 2m, decimal? temVen = 2m,
        decimal? temPadTotal = 4m, decimal? temTraTotal = 4m, decimal? temCobTotal = 4m, decimal? temVenTotal = 4m,
        decimal? vz1ValDes = 0m, decimal? vscValDes = 0m, decimal? vo4ValDes = 30m, decimal? vo4PreKil = 0m, decimal? vo4KilRod = 0m,
        decimal? vscKilRod = 0m, decimal? vo4ValInt = 80m, decimal? vo4ValHor = 150m, decimal? vo4ValVen = 0m, decimal? vz1ValUni = 0m,
        decimal? vz1ValBru = 0m, decimal? vscValBru = 0m, decimal? vokPreKil = 0m) =>
        new("010101", "00001234", "TEC01", new DateOnly(2026, 9, 1), "C", descTpTpo, tpServico, "AG", codServico, null, null, vokIncMob,
            vokPreKil, sitTpo, tipTem, temPad, temTra, temCob, temVen, temPadTotal, temTraTotal, temCobTotal, temVenTotal, vz1ValDes, vscValDes,
            vo4ValDes, vo4PreKil, vo4KilRod, vscKilRod, vo4ValInt, vo4ValHor, vo4ValVen, vz1ValUni, vz1ValBru, vscValBru);

    // =============================================================================================
    // Peças
    // =============================================================================================

    [Fact]
    public void O_desconto_repetido_em_cada_requisicao_da_mesma_peca_desconta_uma_vez_so()
    {
        // Duas requisições da mesma peça (2 + 1 unidades a R$ 100), com o desconto de R$ 30 da linha repetido nas duas: o BI
        // toma o maior por peça e o rateia pela quantidade — 300 − 30.
        var itens = new[] { Peca(requisicao: "00000001", quantidade: 2m, desconto: 30m), Peca(requisicao: "00000002", quantidade: 1m, desconto: 30m) };

        RegrasDoValorDaOrdemDeServico.ValorDasPecas(itens).Should().Be(270m);
    }

    [Fact]
    public void A_linha_repetida_identica_conta_uma_vez_e_o_servico_nao_entra_nas_pecas()
    {
        var itens = new[] { Peca(), Peca(), Peca(pecaOuServico: "SRV", codigo: "MO-1", unitario: 999m) };

        RegrasDoValorDaOrdemDeServico.ValorDasPecas(itens).Should().Be(100m, "o LOAD DISTINCT do BI tira a repetida, e a linha SRV é serviço");
    }

    [Fact]
    public void Pecas_diferentes_tem_cada_uma_o_seu_desconto_e_o_desconto_ausente_vale_zero()
    {
        var itens = new[]
        {
            Peca(codigo: "PECA-1", quantidade: 1m, unitario: 100m, desconto: 10m),
            Peca(codigo: "PECA-2", quantidade: 3m, unitario: 50m, desconto: null)
        };

        RegrasDoValorDaOrdemDeServico.ValorDasPecas(itens).Should().Be(90m + 150m);
    }

    [Fact]
    public void A_linha_sem_quantidade_ou_sem_preco_nao_entra_na_soma_como_no_Qlik()
    {
        var itens = new[] { Peca(codigo: "PECA-1", quantidade: null), Peca(codigo: "PECA-2", unitario: null), Peca(codigo: "PECA-3", quantidade: 2m, unitario: 10m) };

        RegrasDoValorDaOrdemDeServico.ValorDasPecas(itens).Should().Be(20m);
    }

    [Fact]
    public void A_chave_da_peca_sem_a_requisicao_e_o_REPLACE_do_sufixo_de_nove()
    {
        var chave = RegrasDoValorDaOrdemDeServico.ChaveDaPeca(Peca(codigo: "PECA-1", requisicao: "00000001", tipoDeTempo: "C"));

        chave.Should().Be("010101|00001234|C|PECA-1|00000001");
        RegrasDoValorDaOrdemDeServico.ChaveSemARequisicao(chave).Should().Be("010101|00001234|C|PECA-1");
    }

    [Fact]
    public void O_REPLACE_do_BI_tira_o_numero_da_OS_quando_ele_e_igual_ao_da_requisicao()
    {
        // PORTADO COMO ESTÁ: o REPLACE tira TODA ocorrência do sufixo de nove, e a OS 00000001 com a requisição 00000001 perde
        // o número da OS também. O desconto dessa requisição fica numa chave à parte — é o que o painel do BI soma.
        RegrasDoValorDaOrdemDeServico.ChaveSemARequisicao("010101|00000001|C|PECA-1|00000001").Should().Be("010101|C|PECA-1");
    }

    // =============================================================================================
    // Serviços
    // =============================================================================================

    [Theory]
    [InlineData("RAT", "C")]
    [InlineData("MO", "FDLI")]
    [InlineData("MO", "RSF")]
    public void Rateio_FDLI_e_RSF_valem_zero(string tpServico, string tipTem) =>
        RegrasDoValorDaOrdemDeServico.ValorDoServico(Servico(tpServico: tpServico, tipTem: tipTem)).Should().Be(0m);

    [Fact]
    public void A_hora_do_cliente_externo_e_tempo_cobrado_vezes_valor_da_hora_com_o_desconto_rateado_pelo_tempo()
    {
        // Tempo vendido = 2 h cobradas; valor da hora = VO4_VALHOR (cliente externo); desconto de 30 rateado por 4 h de
        // tempo vendido total: 2 × 150 − (30 ÷ 4) × 2 = 285.
        RegrasDoValorDaOrdemDeServico.ValorDoServico(Servico()).Should().Be(285m);
    }

    [Fact]
    public void O_cliente_interno_usa_o_valor_interno_da_hora()
    {
        RegrasDoValorDaOrdemDeServico.ValorDoServico(Servico(sitTpo: "3", vo4ValDes: 0m)).Should().Be(2m * 80m);
    }

    [Fact]
    public void O_valor_bruto_da_VSC_dividido_pelo_tempo_cobrado_vira_o_valor_da_hora()
    {
        RegrasDoValorDaOrdemDeServico.ValorDoServico(Servico(vscValBru: 500m, vo4ValDes: 0m)).Should().Be(2m * (500m / 2m));
    }

    [Fact]
    public void O_km_de_socorro_e_km_vendido_vezes_o_preco_do_km_menos_o_desconto()
    {
        // Sem VSC e sem VZ1: o preço do km é o VO4_PREKIL, quando menor que o da hora.
        var km = Servico(vokIncMob: "5", vo4PreKil: 2.5m, vo4ValHor: 3m, vo4KilRod: 100m, vo4ValDes: 10m);

        RegrasDoValorDaOrdemDeServico.ValorDoServico(km).Should().Be(100m * 2.5m - 10m);
    }

    [Fact]
    public void O_km_vendido_por_um_centavo_ao_cliente_interno_conta_uma_vez_e_sem_desconto()
    {
        // A regra do BI de 13/10/2025: o km vendido por R$ 0,01 não é multiplicado pelos quilômetros.
        var km = Servico(vokIncMob: "5", sitTpo: "3", vscKilRod: 40m, vo4ValInt: 0.01m, vo4KilRod: 40m, vo4ValDes: 99m);

        RegrasDoValorDaOrdemDeServico.ValorDoServico(km).Should().Be(0.01m);
    }

    [Fact]
    public void O_servico_de_terceiro_e_o_valor_dele_menos_o_desconto()
    {
        var terceiro = Servico(tpServico: "ST", vscValBru: 500m, vscValDes: 50m, vo4ValDes: 0m);

        RegrasDoValorDaOrdemDeServico.ValorDoServico(terceiro).Should().Be(450m);
    }

    [Fact]
    public void A_hora_tecnica_usa_o_tempo_trabalhado()
    {
        var ht = Servico(tpServico: "HT", temTra: 3m, temTraTotal: 3m, vo4ValDes: 0m);

        RegrasDoValorDaOrdemDeServico.ValorDoServico(ht).Should().Be(3m * 150m);
    }

    [Fact]
    public void A_divisao_por_zero_do_desconto_tira_a_linha_da_soma_como_no_Qlik()
    {
        // Sem tempo cobrado nem vendido no total, o divisor do desconto é o tempo padrão total — zero: o Qlik dá nulo.
        var semTempo = Servico(temCob: 0m, temVen: 0m, temCobTotal: 0m, temVenTotal: 0m, temPadTotal: 0m);

        RegrasDoValorDaOrdemDeServico.ValorDoServico(semTempo).Should().BeNull();
    }

    [Fact]
    public void O_tipo_de_tempo_de_pecas_passa_na_frente_do_km()
    {
        // "Somente peças" vem antes do km no Tipo_Prestacao: a linha cai na conta das horas, cujo tempo do km é zero e cujo
        // divisor do desconto é zero — o BI a tira da soma.
        var pecas = Servico(vokIncMob: "5", descTpTpo: "REQUISICAO DE PECA", vo4KilRod: 100m, vo4PreKil: 2.5m, vo4ValHor: 3m);

        RegrasDoValorDaOrdemDeServico.ValorDoServico(pecas).Should().BeNull();
    }

    // =============================================================================================
    // A leitura da view
    // =============================================================================================

    [Theory]
    [InlineData("20260915", 2026, 9, 15)]
    [InlineData("15/09/2026", 2026, 9, 15)]
    [InlineData("2026-09-15 00:00:00", 2026, 9, 15)]
    public void A_data_da_view_le_os_tres_formatos(string texto, int ano, int mes, int dia) =>
        LeitorDasOrdensDeServicoDoProtheus.DataDe(texto).Should().Be(new DateOnly(ano, mes, dia));

    [Theory]
    [InlineData("")]
    [InlineData("        ")]
    [InlineData("00000000")]
    [InlineData("19000101")]
    public void A_data_vazia_zerada_ou_fora_da_faixa_e_nula(string texto) =>
        LeitorDasOrdensDeServicoDoProtheus.DataDe(texto).Should().BeNull();

    [Fact]
    public void A_data_de_verdade_do_banco_e_aceita() =>
        LeitorDasOrdensDeServicoDoProtheus.DataDe(new DateTime(2026, 9, 15, 13, 45, 0)).Should().Be(new DateOnly(2026, 9, 15));

    [Theory]
    [InlineData("1234.5", 1234.5)]
    [InlineData("1.234,50", 1234.5)]
    [InlineData("12,5", 12.5)]
    public void O_numero_em_texto_le_ponto_e_virgula(string texto, double esperado) =>
        LeitorDasOrdensDeServicoDoProtheus.NumeroDe(texto).Should().Be((decimal)esperado);

    [Fact]
    public void O_numero_do_banco_e_o_texto_vazio()
    {
        LeitorDasOrdensDeServicoDoProtheus.NumeroDe(12.5d).Should().Be(12.5m);
        LeitorDasOrdensDeServicoDoProtheus.NumeroDe(7).Should().Be(7m);
        LeitorDasOrdensDeServicoDoProtheus.NumeroDe("  ").Should().BeNull();
        LeitorDasOrdensDeServicoDoProtheus.NumeroDe(DBNull.Value).Should().BeNull();
    }
}
