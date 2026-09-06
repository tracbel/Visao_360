using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Integracao.Vortice;
using Xunit;
using Xunit.Abstractions;

namespace Tracbel.Crm.Integracao.Testes.Vortice;

/// <summary>
/// O SANEAMENTO APLICADO NA LEITURA DO LEGADO — o defeito mais medido do banco de lá, corrigido
/// na fronteira e com o conserto registrado.
///
/// <para><b>O defeito, medido ao vivo em 04/09/2026:</b> o cadastro de pessoas do legado guarda o
/// documento em DUAS colunas NUMÉRICAS — a base e os dígitos verificadores. Número não preserva
/// zero à esquerda, e o resultado é <b>51.523 de 71.303 pessoas jurídicas (72,3%) com o CNPJ
/// gravado sem os zeros</b> que a raiz+filial de 12 dígitos exige.</para>
///
/// <para>Estes testes não dependem de VPN nem de banco: a recomposição é aritmética de texto, e é
/// justamente por isso que ela é testável de verdade. O que a VPN acrescenta é a leitura ao vivo;
/// o que garante que a leitura sai certa é isto aqui.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class SaneamentoDoLegadoTestes(ITestOutputHelper saida)
{
    /// <summary>
    /// O caso real medido no banco do legado: uma pessoa jurídica cujo CNPJ perdeu o zero.
    ///
    /// A base foi lida como <c>83441750001</c> — onze dígitos, quando a raiz+filial tem doze — e
    /// o verificador como <c>28</c>. O documento verdadeiro é <c>08.344.175/0001-28</c>, e a
    /// diferença é exatamente o zero que a coluna numérica comeu.
    /// </summary>
    [Fact]
    public void Recompoe_o_CNPJ_que_perdeu_o_zero_a_esquerda_e_registra_o_ajuste()
    {
        var ajustes = new List<AjusteNaLeitura>();

        var (documento, confere) = SaneamentoDoLegado.RecomporDocumento(
            baseNumerica: 83441750001m,
            verificador: 28m,
            ehPessoaFisica: false,
            ajustes);

        documento.Should().Be("08344175000128");
        confere.Should().BeTrue("o documento recomposto passa no dígito verificador");

        // A CORREÇÃO NÃO ACONTECE EM SILÊNCIO — princípio 1.4 do documento 16. O ajuste sai junto
        // do dado, na resposta da API, para quem olha a tela saber que aquele CNPJ não é
        // exatamente o que está gravado lá.
        ajustes.Should().ContainSingle();
        ajustes[0].Campo.Should().Be("documento");
        ajustes[0].ValorNoLegado.Should().Be("8344175000128");
        ajustes[0].ValorEntregue.Should().Be("08344175000128");
        ajustes[0].Motivo.Should().Contain("zero");

        saida.WriteLine($"{ajustes[0].ValorNoLegado} → {ajustes[0].ValorEntregue}");
        saida.WriteLine(ajustes[0].Motivo);
    }

    [Fact]
    public void Recompoe_o_verificador_de_um_digito_so()
    {
        // Caso real medido: base de 12 dígitos e verificador gravado como o número 0 — que são
        // DOIS zeros no documento, e não um.
        var ajustes = new List<AjusteNaLeitura>();

        var (documento, _) = SaneamentoDoLegado.RecomporDocumento(115574820001m, 0m, false, ajustes);

        documento.Should().Be("11557482000100");
        documento!.Length.Should().Be(14);
        ajustes.Should().NotBeEmpty();
    }

    [Fact]
    public void Recompoe_o_CPF_com_a_base_de_nove_digitos()
    {
        // Pessoa física medida no legado: base 44856318 (oito dígitos, faltando um zero) e
        // verificador 50.
        var ajustes = new List<AjusteNaLeitura>();

        var (documento, confere) = SaneamentoDoLegado.RecomporDocumento(44856318m, 50m, true, ajustes);

        documento.Should().Be("04485631850");
        documento!.Length.Should().Be(11);
        confere.Should().BeTrue();

        saida.WriteLine($"CPF recomposto: {documento} (confere: {confere})");
    }

    [Fact]
    public void Documento_que_nao_confere_sai_MARCADO_e_nao_como_valido()
    {
        // Rejeitar é melhor que corrigir em silêncio (documento 16, princípio 1.3). Recompor é
        // reversível e verificável; chutar o dígito certo não é.
        var ajustes = new List<AjusteNaLeitura>();

        var (documento, confere) = SaneamentoDoLegado.RecomporDocumento(
            baseNumerica: 111111111111m, verificador: 11m, ehPessoaFisica: false, ajustes);

        documento.Should().Be("11111111111111");
        confere.Should().BeFalse();

        ajustes.Should().Contain(a => a.Motivo.Contains("dígito verificador"));
        saida.WriteLine(ajustes.Last().Motivo);
    }

    [Fact]
    public void Documento_ausente_nao_vira_ajuste_nenhum()
    {
        var ajustes = new List<AjusteNaLeitura>();

        SaneamentoDoLegado.RecomporDocumento(null, null, false, ajustes).Documento.Should().BeNull();
        SaneamentoDoLegado.RecomporDocumento(0m, 0m, false, ajustes).Documento.Should().BeNull();

        ajustes.Should().BeEmpty("não há o que corrigir quando não há documento");
    }

    // ------------------------------------------------------------------ telefone e e-mail

    [Fact]
    public void Telefone_e_montado_do_DDD_com_o_numero_e_normalizado_pelo_tipo_de_valor()
    {
        var ajustes = new List<AjusteNaLeitura>();

        var telefone = SaneamentoDoLegado.NormalizarTelefone("17", 999990000m, ajustes);

        telefone.Should().Be(Telefone.Criar("17999990000").Formatado());
        saida.WriteLine($"telefone entregue: {telefone}");
    }

    [Fact]
    public void Telefone_que_nao_forma_numero_brasileiro_e_omitido_COM_motivo()
    {
        // [V] A coluna de telefone do legado é decimal — a mesma escolha de tipo que faz a API de
        // lá devolver erro genérico para telefone com DDI. Aqui o que não forma número sai de
        // fora, e o motivo sai junto.
        var ajustes = new List<AjusteNaLeitura>();

        var telefone = SaneamentoDoLegado.NormalizarTelefone("17", 12m, ajustes);

        telefone.Should().BeNull();
        ajustes.Should().ContainSingle().Which.Campo.Should().Be("telefone");
        saida.WriteLine(ajustes[0].Motivo);
    }

    [Fact]
    public void Email_sem_forma_de_email_e_omitido_COM_motivo()
    {
        var ajustes = new List<AjusteNaLeitura>();

        SaneamentoDoLegado.NormalizarEmail("nao tenho", ajustes).Should().BeNull();
        ajustes.Should().ContainSingle().Which.Campo.Should().Be("email");

        var limpos = new List<AjusteNaLeitura>();
        SaneamentoDoLegado.NormalizarEmail("  Fulano@Tracbel.com.BR ", limpos)
            .Should().Be("fulano@tracbel.com.br");
        limpos.Should().ContainSingle("a normalização de caixa e espaço também fica registrada");
    }

    [Fact]
    public void Espaco_duplicado_no_nome_e_colapsado()
    {
        // Sem isto, "FAZENDA  SANTA LUZIA" e "FAZENDA SANTA LUZIA" são nomes diferentes na hora
        // de comparar — e o legado tem os dois aos milhares.
        SaneamentoDoLegado.Apararar("  FAZENDA   SANTA  LUZIA ").Should().Be("FAZENDA SANTA LUZIA");
        SaneamentoDoLegado.Apararar("   ").Should().BeNull();
        SaneamentoDoLegado.Apararar(null).Should().BeNull();
    }
}
