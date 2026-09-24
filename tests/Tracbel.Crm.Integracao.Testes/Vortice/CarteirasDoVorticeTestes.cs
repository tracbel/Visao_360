using FluentAssertions;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// O DOCUMENTO E O DONO DAS CARTEIRAS DO VÓRTICE (decisão de 24/09/2026): como o CPF/CNPJ de <c>GE_Pessoa</c> vira o
/// número que casa com o cadastro do CRM, e quem é o dono de uma carteira pela regra do BI. Documentos fictícios, com
/// dígito verificador válido.
/// </summary>
public sealed class CarteirasDoVorticeTestes
{
    // ------------------------------------------------------------------ o documento

    [Fact]
    public void Sem_numero_e_sem_documento_e_zero_e_zerado()
    {
        DocumentoDoVortice.Recompor(null, null, "J").Situacao.Should().Be(SituacaoDoDocumentoNoVortice.SemDocumento);
        DocumentoDoVortice.Recompor(0, 0, "F").Situacao.Should().Be(SituacaoDoDocumentoNoVortice.Zerado);
    }

    [Fact]
    public void Pessoa_juridica_recebe_de_volta_os_zeros_ate_doze_mais_dois()
    {
        // 00.000.000/0001-91: a base de 12 dígitos é "000000000001", que a coluna numérica guarda como 1.
        var documento = DocumentoDoVortice.Recompor(1, 91, "J");

        documento.Situacao.Should().Be(SituacaoDoDocumentoNoVortice.Valido);
        documento.Numero.Should().Be("00000000000191", "o CRM compara só dígitos, com os 14 do CNPJ");
        documento.ZerosDevolvidos.Should().BeTrue();
        documento.TipoDeclaradoDiscorda.Should().BeFalse();
    }

    [Fact]
    public void Pessoa_fisica_recebe_de_volta_os_zeros_ate_nove_mais_dois()
    {
        // 012.345.678-90: a base de 9 dígitos "012345678" vira 12345678 na coluna numérica.
        var documento = DocumentoDoVortice.Recompor(12345678, 90, "F");

        documento.Situacao.Should().Be(SituacaoDoDocumentoNoVortice.Valido);
        documento.Numero.Should().Be("01234567890");
        documento.ZerosDevolvidos.Should().BeTrue();
    }

    [Fact]
    public void O_documento_completo_nao_muda()
    {
        var documento = DocumentoDoVortice.Recompor(529982247, 25, "F");

        documento.Numero.Should().Be("52998224725");
        documento.ZerosDevolvidos.Should().BeFalse();
    }

    [Fact]
    public void Quando_o_tipo_declarado_discorda_vale_o_documento()
    {
        // 11.222.333/0001-81 declarado como pessoa física: não confere com 9 + 2, confere com 12 + 2.
        var documento = DocumentoDoVortice.Recompor(112223330001, 81, "F");

        documento.Situacao.Should().Be(SituacaoDoDocumentoNoVortice.Valido);
        documento.Numero.Should().Be("11222333000181");
        documento.TipoDeclaradoDiscorda.Should().BeTrue("a discordância vira número, para ser vista e corrigida na origem");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    public void Sem_tipo_declarado_o_documento_decide_sozinho(string? tipo)
    {
        DocumentoDoVortice.Recompor(112223330001, 81, tipo).Numero.Should().Be("11222333000181");
    }

    [Fact]
    public void Digito_verificador_errado_e_invalido_e_nao_se_adivinha()
    {
        var documento = DocumentoDoVortice.Recompor(112223330001, 82, "J");

        documento.Situacao.Should().Be(SituacaoDoDocumentoNoVortice.Invalido);
        documento.Numero.Should().BeNull("um documento que não confere não pode casar cadastro nenhum");
    }

    // ------------------------------------------------------------------ o dono

    private static CarteiraNoVortice Carteira(long? responsavel, long? vendedor, long? usuarioDoVendedor, string codigo = "MAQ_13SJRP_01") =>
        new(282, 13, codigo, codigo, responsavel, vendedor, vendedor is null ? null : "CEN - FICTICIO", usuarioDoVendedor, false);

    [Fact]
    public void O_dono_e_o_usuario_do_vendedor_como_no_BI()
    {
        var carteira = Carteira(responsavel: 10, vendedor: 5, usuarioDoVendedor: 20);

        carteira.SeqUsuarioDono.Should().Be(20);
        carteira.VendedorDiferenteDoResponsavel.Should().BeTrue();
    }

    [Fact]
    public void Sem_vendedor_o_dono_e_o_responsavel()
    {
        // O pool do INT.MERCADO e parte das digitais: SeqVendedor nulo.
        Carteira(responsavel: 482, vendedor: null, usuarioDoVendedor: null, codigo: "TBA_FILIAIS").SeqUsuarioDono.Should().Be(482);
    }

    [Fact]
    public void Vendedor_sem_usuario_cai_no_responsavel_e_sem_nenhum_nao_ha_dono()
    {
        Carteira(responsavel: 10, vendedor: 5, usuarioDoVendedor: null).SeqUsuarioDono.Should().Be(10);
        Carteira(responsavel: 0, vendedor: null, usuarioDoVendedor: null).SeqUsuarioDono.Should().BeNull();
    }

    [Fact]
    public void A_chave_do_vinculo_na_trilha_e_pessoa_e_carteira()
    {
        new VinculoNoVortice(123456, 282, DocumentoDoVortice.Recompor(null, null, null), null).Chave.Should().Be("123456/282");
    }
}
