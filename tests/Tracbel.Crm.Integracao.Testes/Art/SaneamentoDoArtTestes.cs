using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Integracao.Art;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.Art;

/// <summary>
/// AS REGRAS DA INTEGRAÇÃO DO ART (documento 35, seção 10), exercitadas com os formatos que existem de
/// verdade na view — chassi curto de implemento, dois chassis no mesmo campo, data zerada, as quatro
/// linhas de colhedora e o produto que só se parece com um modelo.
///
/// <para><b>Chassi, documento e nome aqui são inventados</b> (CPF e CNPJ com dígito verificador
/// válido, de exemplo). O que se testa é a regra.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class SaneamentoDoArtTestes
{
    private const string ChassiValido = "1ABCD23EFGH456789";
    private const string OutroChassiValido = "9ZXY876WVUT543210";
    private const string CnpjDeExemplo = "11222333000181";

    // =============================================================================================
    // Chassi
    // =============================================================================================

    [Theory]
    [InlineData(null, SituacaoDoChassiNaOrigem.Vazio)]
    [InlineData("   ", SituacaoDoChassiNaOrigem.Vazio)]
    [InlineData(ChassiValido, SituacaoDoChassiNaOrigem.Valido)]
    [InlineData("1abcd23efgh 456789", SituacaoDoChassiNaOrigem.Valido)]
    [InlineData("1234567", SituacaoDoChassiNaOrigem.Incompleto)]
    [InlineData("1ABCD23EFGH45678O", SituacaoDoChassiNaOrigem.ForaDoPadrao)]
    [InlineData("1ABCD23EFGH456789-", SituacaoDoChassiNaOrigem.ForaDoPadrao)]
    [InlineData(ChassiValido + " / " + OutroChassiValido, SituacaoDoChassiNaOrigem.Multiplo)]
    [InlineData(ChassiValido + OutroChassiValido, SituacaoDoChassiNaOrigem.Multiplo)]
    public void O_chassi_so_entra_completo_e_valido(string? bruto, SituacaoDoChassiNaOrigem esperada)
    {
        var (situacao, chassi) = SaneamentoDoArt.ClassificarChassi(bruto);

        situacao.Should().Be(esperada);
        (chassi is not null).Should().Be(esperada == SituacaoDoChassiNaOrigem.Valido,
            "só o chassi válido vira identidade de máquina — nenhum pedaço é adivinhado");
    }

    // =============================================================================================
    // Datas
    // =============================================================================================

    [Theory]
    [InlineData("0000-00-00", "zerada")]
    [InlineData("2024-02-30", "inválida")]
    [InlineData("1899-12-31", "fora da faixa")]
    public void A_data_zerada_ou_invalida_fica_vazia_e_a_transformacao_fica_escrita(string bruto, string trecho)
    {
        var transformacoes = new List<string>();

        SaneamentoDoArt.Data("data da entrega", bruto, transformacoes).Should().BeNull("nenhuma data substituta é inventada");
        transformacoes.Should().ContainSingle().Which.Should().Contain(trecho);
    }

    [Fact]
    public void A_data_valida_passa_sem_transformacao()
    {
        var transformacoes = new List<string>();

        SaneamentoDoArt.Data("data da venda", "2025-09-16", transformacoes).Should().Be(new DateOnly(2025, 9, 16));
        transformacoes.Should().BeEmpty();
    }

    [Fact]
    public void A_data_ausente_nao_e_transformacao() =>
        SaneamentoDoArt.Data("data do faturamento", null, []).Should().BeNull();

    // =============================================================================================
    // Registro
    // =============================================================================================

    [Fact]
    public void O_registro_valido_nao_tem_motivo_de_pendencia()
    {
        var venda = SaneamentoDoArt.Sanear(Registro());

        venda.Motivos.Should().BeEmpty();
        venda.Chassi!.Value.Numero.Should().Be(ChassiValido);
        venda.Documento!.Value.Numero.Should().Be(CnpjDeExemplo);
        venda.Gestao.Should().Be("Grandes Contas", "o valor da origem é preservado como veio — não vira SAM nem KAM");
        venda.EntregueEm.Should().BeNull();
        venda.TransformacoesEmTexto.Should().Contain("data da entrega: zerada");
    }

    [Fact]
    public void Documento_que_nao_passa_no_digito_verificador_deixa_o_registro_pendente() =>
        SaneamentoDoArt.Sanear(Registro() with { CpfCnpj = "11222333000182" })
            .Motivos.Should().Contain(MotivoDePendenciaDoArt.DocumentoInvalido);

    [Fact]
    public void Chassi_de_lote_deixa_o_registro_pendente_com_o_motivo_proprio() =>
        SaneamentoDoArt.Sanear(Registro() with { Chassis = ChassiValido + ";" + OutroChassiValido })
            .Motivos.Should().Contain(MotivoDePendenciaDoArt.ChassiMultiplo);

    [Fact]
    public void O_resumo_do_conteudo_e_estavel_e_muda_quando_a_origem_muda()
    {
        var original = SaneamentoDoArt.Sanear(Registro());

        SaneamentoDoArt.Sanear(Registro()).Hash.Should().Be(original.Hash, "a recarga do mesmo conteúdo não regrava nada");
        SaneamentoDoArt.Sanear(Registro() with { CpfCnpj = "52998224725" }).Hash.Should().NotBe(original.Hash);
        SaneamentoDoArt.Sanear(Registro() with { DataDaVenda = "2025-09-17" }).Hash.Should().NotBe(original.Hash);
    }

    // =============================================================================================
    // Correspondências
    // =============================================================================================

    [Theory]
    [InlineData("TRATOR PEQUENO", "TRATOR_PEQUENO")]
    [InlineData("TRATOR MÉDIO", "TRATOR_MEDIO")]
    [InlineData("TRATOR GRANDE", "TRATOR_GRANDE")]
    [InlineData("COLHEDORA CANA CH 570", "COLHEDORA_DE_CANA")]
    [InlineData("COLHEDORA CANA", "COLHEDORA_DE_CANA")]
    [InlineData("IMPLEMENTOS OM", "IMPLEMENTO_OUTRAS_MARCAS")]
    public void A_linha_do_art_passa_pela_tabela_explicita(string linha, string classificacao)
    {
        var avaliacao = ClassificacaoDoArt.ClassificarLinha(linha);

        avaliacao.Situacao.Should().Be(SituacaoDaCorrespondencia.CorrespondenciaExata);
        avaliacao.Destino.Should().Be(classificacao);
    }

    [Fact]
    public void Usados_e_condicao_da_venda_e_nao_classifica_a_maquina() =>
        ClassificacaoDoArt.ClassificarLinha("USADOS").Situacao.Should().Be(SituacaoDaCorrespondencia.NaoEClassificacaoDeProduto);

    [Fact]
    public void Linha_fora_da_tabela_fica_pendente_de_revisao() =>
        ClassificacaoDoArt.ClassificarLinha("TRATOR COMPACTO").Situacao.Should().Be(SituacaoDaCorrespondencia.PendenteDeRevisao);

    [Fact]
    public void O_produto_so_corresponde_por_codigo_identico_e_candidato_unico()
    {
        ModeloDoCatalogo[] modelos = [new(1, "PV_M4030"), new(2, "CH_570"), new(3, "CH570"), new(4, "6135M")];

        var identico = ClassificacaoDoArt.ClassificarProduto("PV M4030", modelos);
        identico.Situacao.Should().Be(SituacaoDaCorrespondencia.CorrespondenciaExata);
        identico.Destino.Should().Be("1");

        ClassificacaoDoArt.ClassificarProduto("CH 570", modelos).Situacao
            .Should().Be(SituacaoDaCorrespondencia.PendenteDeRevisao, "dois modelos com o mesmo código normalizado é ambiguidade");

        ClassificacaoDoArt.ClassificarProduto("TR 6135M", modelos).Situacao
            .Should().Be(SituacaoDaCorrespondencia.PendenteDeRevisao, "parecer com 6135M não é ser 6135M");

        ClassificacaoDoArt.ClassificarProduto("Implementos", modelos).Situacao
            .Should().Be(SituacaoDaCorrespondencia.NaoEClassificacaoDeProduto);
    }

    [Theory]
    [InlineData(8, 8, true)]
    [InlineData(null, 9, true)]
    [InlineData(8, null, true)]
    [InlineData(8, 15, false)]
    public void A_classificacao_da_venda_nao_contradiz_a_familia_do_modelo(int? familiaDoModelo, int? familiaDaClassificacao, bool compativel) =>
        ClassificacaoDoArt.ClassificacaoCompativelComOModelo(familiaDoModelo, familiaDaClassificacao)
            .Should().Be(compativel, "um trator com modelo não vira implemento porque a linha da venda diz implemento");

    [Fact]
    public void A_unidade_corresponde_a_filial_pelo_nome_identico_e_a_inativa_fica_sinalizada()
    {
        FilialDoCrm[] filiais =
        [
            new(1, "010101", "Tracbel Agro — Ribeirão Preto", true),
            new(2, "010110", "Tracbel Agro — Monte Alto", false),
            new(3, "CEQU", "Colorado Equipamentos", false)
        ];

        var ativa = ClassificacaoDoArt.ClassificarUnidade("Ribeirão Preto", filiais);
        ativa.Situacao.Should().Be(SituacaoDaCorrespondencia.CorrespondenciaExata);
        ativa.Destino.Should().Be("1");

        var inativa = ClassificacaoDoArt.ClassificarUnidade("Monte Alto", filiais);
        inativa.Destino.Should().Be("2");
        inativa.Criterio.Should().Contain("INATIVA");

        ClassificacaoDoArt.ClassificarUnidade("Ribeirão", filiais).Situacao
            .Should().Be(SituacaoDaCorrespondencia.PendenteDeRevisao, "começo de nome não é nome");
    }

    private static RegistroDoArt Registro() => new(
        Codigo: "1001",
        Chassis: ChassiValido,
        CpfCnpj: CnpjDeExemplo,
        Cliente: "Fazenda Exemplo Ltda",
        Linha: "TRATOR PEQUENO",
        Produto: "TR 5080E",
        Empresa: "Agro Norte",
        Unidade: "Ribeirão Preto",
        UnidadeDoFaturamento: "Bebedouro",
        DataDaVenda: "2025-09-16",
        DataDoFaturamento: "2025-09-18",
        Entrega: "0000-00-00",
        AbertaEm: "2025-09-10 08:30:00",
        Situacao: "P",
        NumeroDoPedido: "123",
        NumeroDaNotaFiscal: "4567",
        Gestao: "Grandes Contas",
        VendaDireta: "Não",
        RepasseDireto: "Não",
        Quantidade: "1");
}
