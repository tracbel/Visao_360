using FluentAssertions;
using Tracbel.Crm.Dominio.Comercial;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Protheus;

/// <summary>
/// AS REGRAS DO PARQUE PELO PROPRIETÁRIO ATUAL (decisões de 24/09/2026), sem banco: a evidência do dono, quem prevalece
/// quando o Protheus e o ART discordam, o chassi repetido, o componente e a própria Tracbel pela raiz do CNPJ.
///
/// <para>Documentos inventados, com dígito verificador válido; o que se testa é a regra.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class RegrasDoParqueTestes
{
    private const string Fazenda = "11222333000181";
    private const string OutraFilialDaFazenda = "11222333000262";
    private const string Pessoa = "52998224725";
    private const string Tracbel = "03258870000100";

    private static readonly DateOnly VendaNoArt = new(2023, 5, 10);
    private static readonly IReadOnlySet<string> Grupo = ParceirosPorRaizDeCnpj.RaizesDoGrupo;

    private static MaquinaNoProtheus Maquina(
        string? dono = Fazenda, DateOnly? venda = null, bool vendaDoDono = false, DateOnly? ordem = null, bool ordemDoDono = false,
        string situacao = "1", string? grupo = "TR 6", string chassi = "1CQ7250PVMR123456", string chave = "000001", DateOnly? vendidaEm = null) =>
        new(chave, chassi, Dominio.Comum.Chassi.Normalizar(chassi), "JD", "JOHN DEERE", "6110J", "TRATOR 6110J", grupo, "TRATOR LINHA 6000",
            true, situacao, 2021, 2022, dono, vendidaEm, venda, vendaDoDono, ordem, ordemDoDono);

    // =============================================================================================
    // A evidência do dono (decisão 3)
    // =============================================================================================

    [Fact]
    public void A_nota_de_venda_do_dono_e_a_evidencia_mais_forte()
    {
        var maquina = Maquina(venda: new(2022, 1, 1), vendaDoDono: true, ordem: new(2024, 3, 1), ordemDoDono: true);

        maquina.Evidencia.Should().Be(EvidenciaDoProprietario.NotaDeVenda);
        maquina.DataDaEvidencia.Should().Be(new DateOnly(2024, 3, 1), "a data é a da evidência mais recente que sustenta o dono");
    }

    [Fact]
    public void Sem_nota_do_dono_vale_a_ordem_de_servico_dele()
    {
        var maquina = Maquina(venda: new(2020, 1, 1), vendaDoDono: false, ordem: new(2024, 3, 1), ordemDoDono: true);

        maquina.Evidencia.Should().Be(EvidenciaDoProprietario.OrdemDeServico);
        maquina.DataDaEvidencia.Should().Be(new DateOnly(2024, 3, 1));
    }

    [Fact]
    public void Sem_nota_nem_ordem_do_dono_so_o_cadastro_antigo_o_sustenta()
    {
        var maquina = Maquina(venda: new(2020, 1, 1), ordem: new(2021, 1, 1), vendidaEm: new(2015, 6, 1));

        maquina.Evidencia.Should().Be(EvidenciaDoProprietario.CadastroAntigo);
        maquina.DataDaEvidencia.Should().Be(new DateOnly(2015, 6, 1), "no só-cadastro, a data é a que o cadastro guarda");
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("8", false)]
    [InlineData("4", false)]
    public void So_a_maquina_vendida_ou_da_carga_antiga_esta_no_cliente(string situacao, bool noCliente) =>
        Maquina(situacao: situacao).EstaNoCliente.Should().Be(noCliente);

    [Theory]
    [InlineData("AP", true)]
    [InlineData("MOT", true)]
    [InlineData("CAPOTA", true)]
    [InlineData("KIT", true)]
    [InlineData("TR 6", false)]
    [InlineData(null, false)]
    public void Agricultura_de_precisao_motor_capota_e_kit_sao_componente(string? grupo, bool componente) =>
        RegrasDoParque.EhComponente(Maquina(grupo: grupo)).Should().Be(componente);

    // =============================================================================================
    // Quem prevalece (decisão 1)
    // =============================================================================================

    [Fact]
    public void O_mesmo_documento_confirma_o_dono() =>
        RegrasDoParque.Comparar(Maquina(), Fazenda, VendaNoArt, Grupo).Should().Be(DesfechoDaComparacaoComOArt.MesmoDono);

    [Fact]
    public void A_mesma_raiz_de_CNPJ_e_a_mesma_empresa_e_vale_o_Protheus()
    {
        var desfecho = RegrasDoParque.Comparar(Maquina(dono: OutraFilialDaFazenda), Fazenda, VendaNoArt, Grupo);

        desfecho.Should().Be(DesfechoDaComparacaoComOArt.ProtheusPrevalecePelaMesmaRaiz);
        RegrasDoParque.RegistraDivergencia(desfecho).Should().BeFalse();
        RegrasDoParque.ProtheusPrevalece(desfecho).Should().BeTrue();
    }

    [Fact]
    public void A_ordem_de_servico_do_dono_depois_da_venda_no_ART_faz_o_Protheus_prevalecer()
    {
        var depois = Maquina(dono: Pessoa, ordem: VendaNoArt.AddDays(1), ordemDoDono: true);

        RegrasDoParque.Comparar(depois, Fazenda, VendaNoArt, Grupo).Should().Be(DesfechoDaComparacaoComOArt.ProtheusPrevalecePorEvidenciaPosterior);
    }

    [Fact]
    public void A_nota_do_dono_depois_da_venda_no_ART_faz_o_Protheus_prevalecer() =>
        RegrasDoParque.Comparar(Maquina(dono: Pessoa, venda: VendaNoArt.AddMonths(6), vendaDoDono: true), Fazenda, VendaNoArt, Grupo)
            .Should().Be(DesfechoDaComparacaoComOArt.ProtheusPrevalecePorEvidenciaPosterior);

    [Theory]
    [InlineData(-1, true)]    // a ordem é de ANTES da venda no ART: não prova que a máquina mudou de mão depois
    [InlineData(0, true)]     // no mesmo dia: não é depois
    [InlineData(30, false)]   // depois, mas a ordem não é do dono atual
    public void Sem_evidencia_do_dono_depois_da_venda_vale_o_ART_e_fica_divergencia(int diasDepois, bool ordemDoDono)
    {
        var maquina = Maquina(dono: Pessoa, ordem: VendaNoArt.AddDays(diasDepois), ordemDoDono: ordemDoDono);

        var desfecho = RegrasDoParque.Comparar(maquina, Fazenda, VendaNoArt, Grupo);

        desfecho.Should().Be(DesfechoDaComparacaoComOArt.ArtPrevaleceSemEvidencia);
        RegrasDoParque.RegistraDivergencia(desfecho).Should().BeTrue();
        RegrasDoParque.ProtheusPrevalece(desfecho).Should().BeFalse();
    }

    [Fact]
    public void Sem_data_da_venda_no_ART_nao_ha_depois_e_vale_o_ART() =>
        RegrasDoParque.Comparar(Maquina(dono: Pessoa, ordem: VendaNoArt, ordemDoDono: true), Fazenda, null, Grupo)
            .Should().Be(DesfechoDaComparacaoComOArt.ArtPrevaleceSemEvidencia);

    [Fact]
    public void Quando_o_Protheus_diz_que_a_dona_e_a_Tracbel_vale_o_ART_mesmo_com_ordem_de_servico()
    {
        var voltou = Maquina(dono: Tracbel, ordem: VendaNoArt.AddYears(1), ordemDoDono: true);

        var desfecho = RegrasDoParque.Comparar(voltou, Fazenda, VendaNoArt, Grupo);

        desfecho.Should().Be(DesfechoDaComparacaoComOArt.ArtPrevalecePorqueOProtheusDizTracbel);
        RegrasDoParque.RegistraDivergencia(desfecho).Should().BeTrue();
    }

    [Fact]
    public void Sem_a_maquina_ou_sem_o_dono_no_Protheus_nao_ha_o_que_contrariar()
    {
        RegrasDoParque.Comparar(null, Fazenda, VendaNoArt, Grupo).Should().Be(DesfechoDaComparacaoComOArt.ProtheusSemDono);
        RegrasDoParque.Comparar(Maquina(dono: null), Fazenda, VendaNoArt, Grupo).Should().Be(DesfechoDaComparacaoComOArt.ProtheusSemDono);
        RegrasDoParque.RegistraDivergencia(DesfechoDaComparacaoComOArt.ProtheusSemDono).Should().BeFalse();
    }

    // =============================================================================================
    // A própria Tracbel, pela raiz do CNPJ — configurável
    // =============================================================================================

    [Fact]
    public void A_Tracbel_e_reconhecida_pela_raiz_e_nunca_pelo_nome()
    {
        RegrasDoParque.EhDoGrupo(Tracbel, Grupo).Should().BeTrue();
        RegrasDoParque.EhDoGrupo("09507371000155", Grupo).Should().BeTrue();
        RegrasDoParque.EhDoGrupo(Fazenda, Grupo).Should().BeFalse();
        RegrasDoParque.EhDoGrupo("03258870000", Grupo).Should().BeFalse("CPF não tem raiz de CNPJ");
        RegrasDoParque.EhDoGrupo("89674782000100", Grupo).Should().BeFalse("a fábrica não é o grupo");
    }

    [Fact]
    public void A_lista_do_grupo_e_a_mesma_do_faturamento_e_a_configuracao_a_substitui()
    {
        ParceirosPorRaizDeCnpj.PelaRaiz(Tracbel).Should().Be(NaturezaDoParceiro.EmpresaDoGrupo);
        ParceirosPorRaizDeCnpj.PelaRaiz("89674782000100").Should().Be(NaturezaDoParceiro.Fabrica);
        ParceirosPorRaizDeCnpj.PelaRaiz(Fazenda).Should().BeNull();

        ParceirosPorRaizDeCnpj.RaizesDoGrupoConfiguradas(null).Should().BeEquivalentTo(["03258870", "09507371"]);
        ParceirosPorRaizDeCnpj.RaizesDoGrupoConfiguradas("  ").Should().BeEquivalentTo(["03258870", "09507371"]);
        ParceirosPorRaizDeCnpj.RaizesDoGrupoConfiguradas("03258870, 09.507.371; 47597271")
            .Should().BeEquivalentTo(["03258870", "09507371", "47597271"]);
        ParceirosPorRaizDeCnpj.RaizesDoGrupoConfiguradas("123, abc")
            .Should().BeEquivalentTo(["03258870", "09507371"], "raiz mal digitada não tira máquina de cliente do parque");
    }

    // =============================================================================================
    // O chassi repetido no Protheus
    // =============================================================================================

    [Fact]
    public void O_chassi_repetido_com_o_mesmo_dono_e_a_mesma_maquina_e_vale_a_linha_mais_recente()
    {
        var antiga = Maquina(chave: "000001", venda: new(2019, 1, 1), vendaDoDono: true);
        var recente = Maquina(chave: "000002", ordem: new(2024, 1, 1), ordemDoDono: true);

        var parque = new ParqueNoProtheus([antiga, recente]);

        parque.Ambiguos.Should().BeEmpty();
        parque.TentarAchar(antiga.Chassi, out var escolhida).Should().BeTrue();
        escolhida.Should().BeSameAs(recente);
    }

    [Fact]
    public void O_chassi_repetido_com_donos_diferentes_e_ambiguo_e_nenhum_e_escolhido()
    {
        var parque = new ParqueNoProtheus([Maquina(chave: "000001", dono: Fazenda), Maquina(chave: "000002", dono: Pessoa)]);

        parque.Ambiguos.Should().ContainSingle();
        parque.TentarAchar("1CQ7250PVMR123456", out _).Should().BeFalse();
    }

    [Fact]
    public void O_chassi_e_indexado_normalizado_espaco_no_meio_e_minuscula()
    {
        var parque = new ParqueNoProtheus([Maquina(chassi: "1cq7250 pvmr123456")]);
        parque.TentarAchar("1CQ7250PVMR123456", out _).Should().BeTrue("aparar só as pontas perdia 98 chassis do Protheus");
    }

    // =============================================================================================
    // O que a leitura converte
    // =============================================================================================

    [Theory]
    [InlineData("20212022", (short)2021, (short)2022)]
    [InlineData("00000000", null, null)]
    [InlineData("2021    ", null, null)]
    [InlineData("", null, null)]
    [InlineData("19002100", (short)1900, null)]
    public void Os_anos_saem_do_FABMOD_so_quando_sao_ano(string fabmod, short? fabricacao, short? modelo) =>
        LeitorDoParqueDoProtheus.AnosDe(fabmod).Should().Be((fabricacao, modelo));

    [Theory]
    [InlineData("20240315", 2024, 3, 15)]
    [InlineData("        ", null, null, null)]
    [InlineData("00000000", null, null, null)]
    [InlineData("20241332", null, null, null)]
    public void A_data_do_Protheus_invalida_sai_nula(string texto, int? ano, int? mes, int? dia) =>
        LeitorDoParqueDoProtheus.Data(texto).Should().Be(ano is null ? null : new DateOnly(ano.Value, mes!.Value, dia!.Value));

    [Fact]
    public void A_consulta_le_o_proprietario_atual_com_a_loja_e_nao_o_cliente_da_ultima_venda()
    {
        LeitorDoParqueDoProtheus.Consulta.Should().Contain("d.A1_COD = v.VV1_PROATU AND d.A1_LOJA = v.VV1_LJPATU");
        LeitorDoParqueDoProtheus.Consulta.Should().NotContain("VV1_CLIULV", "o cliente da última venda está vazio em 28.979 máquinas");
        LeitorDoParqueDoProtheus.Consulta.Should().Contain("VO1_STATUS <> 'C'", "ordem de serviço cancelada não é evidência");
        LeitorDoParqueDoProtheus.Consulta.Should().Contain("VV0_OPEMOV = '0' AND c.VV0_SITNFI = '1'", "só a nota de venda válida");
        LeitorDoParqueDoProtheus.Consulta.Should().NotContainAny(["A1_NOME", "INSERT", "UPDATE", "DELETE"]);
    }
}
