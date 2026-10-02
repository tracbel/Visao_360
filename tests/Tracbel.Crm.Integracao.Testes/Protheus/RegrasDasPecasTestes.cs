using FluentAssertions;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Protheus;

/// <summary>
/// AS REGRAS DO PAINEL "FATURAMENTO PEÇAS" DO BI, portadas do script: o grupo comercial (pneus pela linha, o resto pelo
/// código da família, e a origem sem o prefixo), a devolução pela origem, a quantidade zerada na cortesia e no complemento
/// de preço, e a filial de dois dígitos no código do CRM.
/// </summary>
public sealed class RegrasDasPecasTestes
{
    [Theory]
    [InlineData("FAT_PECAS", "PNEUS/RODADOS", "1104", "PNEU")]
    [InlineData("FAT_PECAS", "PECAS JD", "1104", "BATERIA")]
    [InlineData("FAT_PECAS", "PECAS JD", "1103", "LUBRIFICANTE")]
    [InlineData("FAT_PECAS", "PECAS JD", "2005", "UNIMIL BY JD")]
    [InlineData("FAT_PECAS", "PECAS JD", "5000", "PRECISION UPGRADE")]
    [InlineData("FAT_PECAS", "PECAS JD", "9999", "PECAS")]
    [InlineData("DEV_PECAS", "PECAS JD", "9999", "PECAS")]
    [InlineData("DEV_PECAS", "PNEUS/RODADOS", null, "PNEU")]
    [InlineData("FAT_PECAS", null, null, "PECAS")]
    public void O_grupo_comercial_segue_a_ordem_do_script(string origem, string? linha, string? familia, string esperado) =>
        RegrasDasPecas.Grupo(origem, linha, familia).Should().Be(esperado);

    [Fact]
    public void Sem_origem_e_sem_familia_conhecida_o_grupo_e_sem_classificacao() =>
        RegrasDasPecas.Grupo(null, "PECAS JD", null).Should().Be("S/CLASSIFICAÇÃO");

    [Theory]
    [InlineData("FAT_PECAS", false)]
    [InlineData("DEV_PECAS", true)]
    [InlineData(null, true)]
    public void Tudo_o_que_nao_e_faturamento_cai_no_ramo_da_devolucao(string? origem, bool devolucao) =>
        RegrasDasPecas.EhDevolucao(origem).Should().Be(devolucao);

    [Theory]
    [InlineData("SIM", "OFI", null, false)]
    [InlineData("SIM", "FGP", null, true)]
    [InlineData("NAO", "OFI", "COMPLEMENTO PRECO", false)]
    [InlineData("NAO", "OFI", "VENDA", true)]
    [InlineData(null, null, null, true)]
    public void A_quantidade_e_zerada_na_cortesia_fora_da_garantia_e_no_complemento_de_preco(string? cortesia, string? ordem, string? operacao, bool conta) =>
        RegrasDasPecas.QuantidadeConta(cortesia, ordem, operacao).Should().Be(conta);

    [Theory]
    [InlineData("01", "010101")]
    [InlineData("5", "010105")]
    [InlineData("010116", "010116")]
    [InlineData("ABC", null)]
    [InlineData("0101", null)]
    [InlineData(null, null)]
    public void A_filial_da_view_vira_o_codigo_do_CRM(string? filial, string? esperado) =>
        RegrasDasPecas.CodigoDaFilial(filial).Should().Be(esperado);
}
