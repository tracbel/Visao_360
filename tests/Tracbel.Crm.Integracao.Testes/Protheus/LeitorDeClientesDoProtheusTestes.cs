using FluentAssertions;
using Tracbel.Crm.Integracao.Protheus;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Protheus;

/// <summary>
/// O DE-PARA DO MUNICÍPIO ENTRE O PROTHEUS E O IBGE (issue 69, decisão de 24/09/2026).
///
/// <para>É a conta de que depende TODO o cadastro: sem município não há filial responsável, sem
/// filial não há dono, e sem dono o cliente não entra. Ela foi medida contra o cadastro real —
/// 35.030 documentos, 1 falha — e o que se testa aqui é a regra, com os municípios que de fato
/// concentram a carteira agro.</para>
///
/// <para><b>O prefixo da UF não é uma tabela escrita no código.</b> Ele sai do catálogo de
/// municípios do CRM, e chega aqui como parâmetro justamente para que uma segunda lista de UFs não
/// exista em lugar nenhum. Estes testes passam o prefixo à mão porque são testes da CONTA, não do
/// catálogo.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class LeitorDeClientesDoProtheusTestes
{
    // =============================================================================================
    // O código do município
    // =============================================================================================

    /// <summary>
    /// Os oito municípios que mais concentram clientes na SA1, conferidos um a um contra o catálogo
    /// do CRM em 24/09/2026. São a região agro da Tracbel, e é por isso que estão aqui: se a conta
    /// quebrar, quebra primeiro onde dói mais.
    /// </summary>
    [Theory]
    [InlineData(35, "43402", 3543402)] // Ribeirão Preto
    [InlineData(35, "49805", 3549805)] // São José do Rio Preto
    [InlineData(35, "17406", 3517406)] // Guaíra
    [InlineData(35, "05500", 3505500)] // Barretos
    [InlineData(35, "22703", 3522703)] // Itápolis
    [InlineData(35, "24303", 3524303)] // Jaboticabal
    [InlineData(35, "03208", 3503208)] // Araraquara
    [InlineData(35, "31308", 3531308)] // Monte Alto
    [InlineData(31, "06200", 3106200)] // Belo Horizonte — a segunda UF em volume
    public void O_codigo_do_ibge_e_o_prefixo_da_uf_mais_o_que_a_SA1_guarda(int prefixo, string codigoNaOrigem, int esperado)
    {
        LeitorDeClientesDoProtheus.CodigoIbge(prefixo, codigoNaOrigem).Should().Be(esperado);
    }

    /// <summary>
    /// O ZERO À ESQUERDA NÃO PODE SUMIR. "05500" é Barretos; lido como 5500 daria 3505500 do mesmo
    /// jeito, mas "00550" daria outro município. O teste fixa que o valor é numérico e posicional.
    /// </summary>
    [Fact]
    public void O_zero_a_esquerda_nao_desloca_o_municipio()
    {
        LeitorDeClientesDoProtheus.CodigoIbge(35, "05500").Should().Be(3505500);
        LeitorDeClientesDoProtheus.CodigoIbge(35, "00550").Should().Be(3500550);
    }

    /// <summary>
    /// CÓDIGO ILEGÍVEL NÃO VIRA MUNICÍPIO INVENTADO. Devolve nulo, e a carga põe o cliente na lista
    /// de pendências com o motivo — nunca num município qualquer.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("00000")]
    [InlineData("ABCDE")]
    [InlineData("12-34")]
    public void Codigo_ilegivel_nao_vira_municipio(string codigoNaOrigem)
    {
        LeitorDeClientesDoProtheus.CodigoIbge(35, codigoNaOrigem).Should().BeNull();
    }

    // =============================================================================================
    // O documento
    // =============================================================================================

    [Theory]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    [InlineData("123.456.789-09", "12345678909")]
    [InlineData("  11222333000181  ", "11222333000181")]
    [InlineData(null, "")]
    public void O_documento_perde_a_pontuacao_para_poder_ser_comparado(string? bruto, string esperado)
    {
        LeitorDeClientesDoProtheus.SoDigitos(bruto).Should().Be(esperado);
    }
}
