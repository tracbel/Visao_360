using System.Text.RegularExpressions;
using FluentAssertions;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Protheus;

/// <summary>
/// O FATURAMENTO DO PROTHEUS, LIDO NO BANCO (24/09/2026): o filtro de venda, a quebra por grupo e a
/// agregação no grão do CRM — documento, filial e mês.
///
/// <para>A consulta em si roda contra um SQL Server de verdade em
/// <c>Tracbel.Crm.Arquitetura.Testes/Banco/LeitorDeFaturamentoNoContainerTestes</c>. Aqui fica a
/// regra, que é pura e não precisa de banco.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDeFaturamentoDoProtheusTestes
{
    private static readonly DateOnly Desde = new(2023, 9, 1);
    private static readonly ResumoDaSd2 SemResumo = new(0, 0, 0m, null);

    // =============================================================================================
    // O que é venda
    // =============================================================================================

    [Theory]
    [InlineData("N", "5102", true)]
    [InlineData("N", "6102", true)]
    [InlineData("N", "5405", true)]
    [InlineData("N", "5933", true)]
    [InlineData("N", "5102 ", true)] // D2_CF é varchar(5): o espaço à direita não muda o CFOP
    [InlineData("N", "5152", false)] // transferência entre filiais
    [InlineData("N", "5916", false)] // retorno de conserto
    [InlineData("N", "5949", false)] // a pergunta em aberto: fica de fora até o fiscal responder
    [InlineData("B", "5102", false)] // beneficiamento: CFOP de venda, tipo que não é
    [InlineData("D", "5102", false)] // devolução
    [InlineData(null, "5102", false)]
    [InlineData("N", null, false)]
    public void So_nota_normal_com_CFOP_da_lista_e_venda(string? tipo, string? cfop, bool esperado) =>
        LeitorDeFaturamentoDoProtheus.EhVenda(tipo, cfop).Should().Be(esperado);

    [Fact]
    public void O_filtro_das_duas_consultas_sai_da_mesma_lista_e_de_mais_nenhuma()
    {
        foreach (var consulta in new[] { LeitorDeFaturamentoDoProtheus.ConsultaDaVenda, LeitorDeFaturamentoDoProtheus.ConsultaDoResumo })
        {
            var cfops = Regex.Matches(consulta, @"'(\d{4})'").Select(m => m.Groups[1].Value).Distinct().ToList();

            cfops.Should().BeEquivalentTo(LeitorDeFaturamentoDoProtheus.CfopsDeVenda,
                "a lista de CFOPs mora num lugar só; um CFOP escrito à mão no SQL seria a segunda cópia que um dia discorda");
            consulta.Should().Contain($"d.D2_TIPO = '{LeitorDeFaturamentoDoProtheus.TipoDeVenda}'");
        }
    }

    [Fact]
    public void As_consultas_so_leem_e_nao_seguram_trava_no_ERP()
    {
        foreach (var consulta in new[] { LeitorDeFaturamentoDoProtheus.ConsultaDaVenda, LeitorDeFaturamentoDoProtheus.ConsultaDoResumo })
        {
            consulta.Should().NotContainAny("INSERT", "UPDATE", "DELETE", "MERGE", "EXEC", "INTO");
            Regex.Matches(consulta, @"dbo\.\w+ \w+ WITH \(NOLOCK\)").Count
                .Should().Be(Regex.Matches(consulta, @"dbo\.\w+").Count, "toda tabela do ERP é lida com NOLOCK");
            consulta.Should().Contain("D_E_L_E_T_ = ' '", "linha apagada no Protheus não é venda");
        }
    }

    // =============================================================================================
    // Máquina, peça, serviço
    // =============================================================================================

    [Theory]
    [InlineData("VEIC", GrupoDaNota.Maquina)]
    [InlineData("SRV", GrupoDaNota.Servico)]
    [InlineData("MO_O", GrupoDaNota.Servico)]
    [InlineData("MO_T", GrupoDaNota.Servico)]
    [InlineData("1001", GrupoDaNota.Peca)]
    [InlineData("1042", GrupoDaNota.Peca)] // o grupo de amanhã já entra como peça
    [InlineData("1001 ", GrupoDaNota.Peca)]
    [InlineData("2001", GrupoDaNota.Outros)]
    [InlineData("100A", GrupoDaNota.Outros)]
    [InlineData("AMS", GrupoDaNota.Outros)]
    [InlineData("", GrupoDaNota.Outros)]
    [InlineData(null, GrupoDaNota.Outros)]
    public void O_grupo_da_nota_diz_se_foi_maquina_peca_ou_servico(string? grupo, GrupoDaNota esperado) =>
        LeitorDeFaturamentoDoProtheus.Classificar(grupo).Should().Be(esperado);

    // =============================================================================================
    // A agregação
    // =============================================================================================

    private static VendaAgregadaNoProtheus Venda(
        string filial = "010101", string mes = "202608", string cliente = "000000001", string loja = "0001",
        string? documento = "11222333000181", string? nome = "FAZENDA BOA VISTA", string grupo = "1001",
        decimal valor = 100m, int itens = 1, int notas = 1) =>
        new(filial, mes, cliente, loja, documento, nome, grupo, valor, itens, notas);

    [Fact]
    public void A_filial_de_cada_nota_e_preservada_e_nao_cai_toda_em_010101()
    {
        var lote = LeitorDeFaturamentoDoProtheus.Agregar(Desde,
        [
            Venda(filial: "010101", valor: 500m),
            Venda(filial: "010113", valor: 300m),
            Venda(filial: "010109", grupo: "VEIC", valor: 900_000m)
        ], SemResumo);

        lote.Faturamento.Select(f => (f.CodigoDaFilial, f.ValorLiquido)).Should().BeEquivalentTo(
            [("010101", 500m), ("010109", 900_000m), ("010113", 300m)],
            "D2_FILIAL é a filial que emitiu a nota; o comentário antigo dizia 100% em 010101, e era efeito da leitura REST sem tenantId");
        lote.FiliaisVistas.Should().Equal("010101", "010109", "010113");
    }

    [Fact]
    public void Lojas_do_mesmo_documento_viram_um_cliente_e_as_notas_somam_sem_contar_duas_vezes()
    {
        var lote = LeitorDeFaturamentoDoProtheus.Agregar(Desde,
        [
            // A loja 0001 comprou máquina e peça em 3 notas: a contagem vem repetida nos dois grupos.
            Venda(loja: "0001", grupo: "VEIC", valor: 1_000m, itens: 1, notas: 3),
            Venda(loja: "0001", grupo: "1001", valor: 200m, itens: 4, notas: 3),
            // A loja 0004 tem o MESMO CNPJ, formatado, e 2 notas próprias.
            Venda(loja: "0004", documento: "11.222.333/0001-81", grupo: "SRV", valor: 50m, itens: 2, notas: 2)
        ], SemResumo);

        var mes = lote.Faturamento.Should().ContainSingle().Subject;
        mes.DocumentoDoCliente.Should().Be("11222333000181");
        mes.Notas.Should().Be(5, "3 da loja 0001, contadas uma vez, mais 2 da loja 0004 — cada nota tem uma loja só");
        mes.Itens.Should().Be(7);
        mes.ValorLiquido.Should().Be(1_250m);
        mes.Quebra.Maquina.Should().Be(1_000m);
        mes.Quebra.Peca.Should().Be(200m);
        mes.Quebra.Servico.Should().Be(50m);
        mes.Quebra.Outros.Should().Be(0m);
        mes.ChaveDeOrigem.Should().Be("11222333000181/010101/2026-08");
        mes.Competencia.Should().Be(new DateOnly(2026, 8, 1));
    }

    [Fact]
    public void A_quebra_fecha_com_o_total_e_o_grupo_desconhecido_vai_para_outros()
    {
        var lote = LeitorDeFaturamentoDoProtheus.Agregar(Desde,
        [
            Venda(grupo: "VEIC", valor: 0.333m),
            Venda(grupo: "1001", valor: 0.333m),
            Venda(grupo: "SRV", valor: 0.333m),
            Venda(grupo: "AMS", valor: 10m)
        ], SemResumo);

        var quebra = lote.Faturamento.Single().Quebra;
        (quebra.Maquina + quebra.Peca + quebra.Servico + quebra.Outros).Should().Be(lote.Faturamento.Single().ValorLiquido,
            "a restrição do banco recusa a linha cuja quebra não fecha com o total");
        quebra.Outros.Should().BeGreaterThanOrEqualTo(10m);
    }

    [Fact]
    public void Codigo_sem_documento_valido_fica_de_fora_com_o_valor_contado()
    {
        var lote = LeitorDeFaturamentoDoProtheus.Agregar(Desde,
        [
            Venda(cliente: "000000001", documento: null, valor: 70m, itens: 2),     // código sem SA1
            Venda(cliente: "000000002", documento: "   ", valor: 20m, itens: 1),    // SA1 sem CGC
            Venda(cliente: "000000002", documento: "   ", grupo: "VEIC", valor: 5m), // mesmo código, outro grupo
            Venda(cliente: "000000003", documento: "12345", valor: 1m),             // não é CPF nem CNPJ
            Venda(cliente: "000000004", valor: 10m)
        ], SemResumo);

        lote.Faturamento.Should().ContainSingle().Which.ValorLiquido.Should().Be(10m);
        lote.CodigosSemDocumento.Should().Be(3);
        lote.ItensSemDocumento.Should().Be(5);
        lote.ValorSemDocumento.Should().Be(96m, "o descarte diz quanto dinheiro levou junto");
    }

    [Fact]
    public void Mes_ilegivel_e_valor_zerado_nao_entram()
    {
        var lote = LeitorDeFaturamentoDoProtheus.Agregar(Desde,
        [
            Venda(mes: "2026AB", itens: 3),
            Venda(mes: "202607", valor: 0m),
            Venda(mes: "202608", valor: 1m)
        ], SemResumo);

        lote.ItensSemData.Should().Be(3);
        lote.Faturamento.Should().ContainSingle().Which.Competencia.Should().Be(new DateOnly(2026, 8, 1));
    }

    [Fact]
    public void O_nome_do_documento_e_o_da_loja_de_menor_codigo_e_nao_muda_de_carga_para_carga()
    {
        VendaAgregadaNoProtheus[] linhas =
        [
            Venda(loja: "0008", nome: "FAZENDA BOA VISTA FILIAL", valor: 1m),
            Venda(loja: "0001", nome: "FAZENDA BOA VISTA", valor: 1m, mes: "202607"),
            Venda(loja: "0004", nome: "FAZ. BOA VISTA", valor: 1m)
        ];

        var ida = LeitorDeFaturamentoDoProtheus.Agregar(Desde, linhas, SemResumo);
        var volta = LeitorDeFaturamentoDoProtheus.Agregar(Desde, Enumerable.Reverse(linhas), SemResumo);

        ida.Faturamento.Should().OnlyContain(f => f.NomeNaOrigem == "FAZENDA BOA VISTA");
        volta.Faturamento.Select(f => f.NomeNaOrigem).Should().Equal(ida.Faturamento.Select(f => f.NomeNaOrigem));
    }

    [Fact]
    public void O_resumo_da_leitura_chega_ao_lote_com_a_janela()
    {
        var lote = LeitorDeFaturamentoDoProtheus.Agregar(
            Desde, [Venda()], new ResumoDaSd2(1_000, 400, 1_234.567m, new DateOnly(2026, 9, 24)));

        lote.Desde.Should().Be(Desde);
        lote.ItensLidos.Should().Be(1_000);
        lote.ItensQueNaoSaoVenda.Should().Be(400);
        lote.ValorQueNaoEhVenda.Should().Be(1_234.57m);
        lote.EmissaoMaisRecente.Should().Be(new DateOnly(2026, 9, 24));
    }

    [Fact]
    public async Task Sem_configuracao_a_leitura_recusa_sem_tentar_conectar()
    {
        var resultado = await new LeitorDeFaturamentoDoProtheus(new Tracbel.Crm.Integracao.Art.OpcoesDoBancoDoProtheus())
            .LerAsync(Desde, _ => { }, CancellationToken.None);

        resultado.EhSucesso.Should().BeFalse();
        resultado.Erro.Should().Contain("ProtheusBanco__Servidor").And.Contain("Configurações > Integrações");
    }
}
