using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Integracao.GestaoDeNegocios;
using Xunit;

namespace Tracbel.Crm.Integracao.Testes.GestaoDeNegocios;

/// <summary>
/// A LEITURA E O SANEAMENTO DO CADASTRO DE METAS DA GN.
///
/// <para><b>A amostra JSON abaixo é a forma provisória de uma linha</b> — os nomes que ainda não foram conferidos contra
/// a API (<c>mes</c>, <c>linha</c>, <c>tipo</c>, <c>origem</c>) estão em <c>LinhaDeMetaNoJson</c>, um lugar só. Quando o
/// Ricardo colar a forma real, é esta amostra que muda junto, e o teste prova que a leitura acompanha. Nomes e números
/// são inventados.</para>
/// </summary>
[Trait("Categoria", "Integracao")]
public sealed class MetasDaGestaoDeNegociosTestes
{
    /// <summary>Uma página com as formas que a API pode mandar: filial como texto e como número, margem nula.</summary>
    private const string Amostra = """
        {"cadastro":"metas","tipo":"cadastro","rotulo":"Metas de venda","gerado_em":"2026-09-27T01:30:00","total":3,
         "pagina":1,"paginas":1,"por_pagina":500,"linhas":[
          {"id":1,"mes":"2025-11","filial_numero":"05","linha":"TRATOR MÉDIO","consultor":"FULANO.DE.TAL","tipo":"Concessão",
           "origem":"Campanha","quantidade":3,"valor_unitario":450000.00,"margem":0.12},
          {"id":2,"mes":"2025-12","filial_numero":5,"linha":"PULVERIZADOR","consultor":"fulano.de.tal","tipo":"Direta",
           "origem":"Campanha","quantidade":1,"valor_unitario":900000,"margem":null},
          {"id":3,"mes":"2026-01","filial_numero":"01","linha":"CONSÓRCIO","consultor":"BELTRANA.SILVA","tipo":"Concessão",
           "origem":"Consórcio","quantidade":4}
         ]}
        """;

    private sealed class TratadorFalso(string corpo) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage pedido, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(corpo, Encoding.UTF8, "application/json") });
    }

    private sealed class FabricaFalsa(HttpMessageHandler tratador) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(tratador, disposeHandler: false);
    }

    private static LeitorDeMetasDaGestaoDeNegocios Leitor(string corpo) => new(new ClienteDaGestaoDeNegocios(
        new FabricaFalsa(new TratadorFalso(corpo)),
        Options.Create(new OpcoesDaGestaoDeNegocios { Base = "https://gn.exemplo.invalido:5001", Chave = "chave-inventada" }),
        esperaEntreTentativas: TimeSpan.Zero));

    private static MetaNaOrigem Linha() =>
        new(10, "2026-03", "7", "Trator Pequeno", " ciclano.souza ", "Concessão", "Campanha", "2", "120000.50", null);

    // =============================================================================================
    // A leitura
    // =============================================================================================

    [Fact]
    public async Task A_amostra_da_api_vira_tres_linhas_com_tudo_como_texto()
    {
        var leitura = await Leitor(Amostra).LerAsync(CancellationToken.None);

        leitura.EhSucesso.Should().BeTrue(leitura.Erro);
        leitura.Valor.Linhas.Select(l => l.Id).Should().Equal(1, 2, 3);
        leitura.Valor.Linhas[0].Should().Be(new MetaNaOrigem(1, "2025-11", "05", "TRATOR MÉDIO", "FULANO.DE.TAL", "Concessão", "Campanha", "3", "450000.00", "0.12"));
        leitura.Valor.Linhas[1].FilialNumero.Should().Be("5", "número ou texto na origem viram o mesmo texto");
        leitura.Valor.Linhas[1].Margem.Should().BeNull();
        leitura.Valor.Linhas[2].ValorUnitario.Should().BeNull("o valor unitário não é obrigatório");
        leitura.Valor.IdadeSegundos.Should().BeNull("o cadastro de metas é da própria GN, não espelho");
        leitura.Valor.GeradaEmUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Campo_obrigatorio_ausente_falha_alto_com_os_nomes_que_chegaram_e_sem_os_valores()
    {
        // O MÊS COM OUTRO NOME: é o que acontece se o provisório "mes" estiver errado.
        var corpo = Amostra.Replace("\"mes\":", "\"competencia\":", StringComparison.Ordinal);

        var leitura = await Leitor(corpo).LerAsync(CancellationToken.None);

        leitura.EhSucesso.Should().BeFalse();
        leitura.Erro.Should().Contain("linha 1").And.Contain("mes").And.Contain("Campos recebidos: id, competencia, filial_numero")
            .And.Contain("nada foi gravado");
        leitura.Erro.Should().NotContain("FULANO", "o erro diz os nomes dos campos, nunca os valores");
    }

    [Fact]
    public void Id_repetido_e_leitura_quebrada()
    {
        var linhas = JsonDocument.Parse("""
            [{"id":7,"mes":"2025-11","filial_numero":"1","linha":"X","consultor":"A.B","tipo":"Direta","origem":"Campanha","quantidade":1},
             {"id":7,"mes":"2025-12","filial_numero":"1","linha":"X","consultor":"A.B","tipo":"Direta","origem":"Campanha","quantidade":1}]
            """).RootElement.EnumerateArray().ToList();

        var convertidas = LeitorDeMetasDaGestaoDeNegocios.Converter(linhas);

        convertidas.EhSucesso.Should().BeFalse();
        convertidas.Erro.Should().Contain("id(s) repetido(s) — 7");
    }

    [Fact]
    public void Id_que_nao_e_inteiro_recusa_a_leitura()
    {
        var linhas = JsonDocument.Parse("""[{"id":"x1","mes":"2025-11","filial_numero":"1","linha":"X","consultor":"A.B","tipo":"Direta","origem":"Campanha","quantidade":1}]""")
            .RootElement.EnumerateArray().ToList();

        LeitorDeMetasDaGestaoDeNegocios.Converter(linhas).Erro.Should().Contain("o id não é um número inteiro");
    }

    // =============================================================================================
    // O saneamento
    // =============================================================================================

    [Fact]
    public void A_linha_boa_sai_com_a_filial_do_crm_a_linha_codificada_como_no_art_e_o_consultor_em_maiusculas()
    {
        var saneada = SaneamentoDasMetas.Sanear(Linha());

        saneada.Motivos.Should().BeEmpty();
        var meta = saneada.Meta!;
        meta.Competencia.Should().Be(new DateOnly(2026, 3, 1));
        meta.FilialCodigo.Should().Be("010107");
        meta.LinhaNaOrigem.Should().Be("Trator Pequeno");
        meta.CodigoDaLinha.Should().Be("TRATOR_PEQUENO", "o mesmo código que o ART dá, para o realizado casar");
        meta.ConsultorNaOrigem.Should().Be("CICLANO.SOUZA");
        meta.VendaDireta.Should().BeFalse();
        meta.Origem.Should().Be(OrigemDaMeta.Campanha);
        meta.Quantidade.Should().Be(2);
        meta.ValorUnitario.Should().Be(120000.50m);
        meta.Margem.Should().BeNull();
        meta.EhConsorcio.Should().BeFalse();
    }

    [Theory]
    [InlineData("2025-11", 2025, 11)]
    [InlineData("2025-11-01", 2025, 11)]
    [InlineData("2025-11-01T00:00:00", 2025, 11)]
    [InlineData("11/2025", 2025, 11)]
    public void O_mes_aceita_as_leituras_sem_ambiguidade(string bruto, int ano, int mes) =>
        SaneamentoDasMetas.Competencia(bruto).Should().Be(new DateOnly(ano, mes, 1));

    [Theory]
    [InlineData("2025-13")]
    [InlineData("1999-11")]
    [InlineData("nov/2025")]
    [InlineData("")]
    public void Mes_que_nao_se_le_recusa_a_linha(string bruto) =>
        SaneamentoDasMetas.Sanear(Linha() with { Mes = bruto }).Motivos.Should().Equal(MotivoDeRecusaDaMeta.MesInvalido);

    [Theory]
    [InlineData("5", "010105")]
    [InlineData("05", "010105")]
    [InlineData("18", "010118")]
    [InlineData("010103", "010103")]
    public void A_filial_vira_0101NN(string bruto, string esperado) =>
        SaneamentoDasMetas.CodigoDaFilial(bruto).Should().Be(esperado);

    [Theory]
    [InlineData("0")]
    [InlineData("123")]
    [InlineData("Ribeirão")]
    [InlineData(null)]
    public void Filial_que_nao_e_NN_recusa_a_linha(string? bruto) =>
        SaneamentoDasMetas.Sanear(Linha() with { FilialNumero = bruto }).Motivos.Should().Equal(MotivoDeRecusaDaMeta.FilialInvalida);

    [Theory]
    [InlineData("Direta", true)]
    [InlineData("DIRETA", true)]
    [InlineData("Concessão", false)]
    [InlineData("CONCESSAO", false)]
    public void O_tipo_decide_a_venda_direta(string tipo, bool direta) =>
        SaneamentoDasMetas.Sanear(Linha() with { Tipo = tipo }).Meta!.VendaDireta.Should().Be(direta);

    [Fact]
    public void Consorcio_fica_a_parte_pela_origem_ou_pela_linha()
    {
        SaneamentoDasMetas.Sanear(Linha() with { Origem = "Consórcio" }).Meta!.EhConsorcio.Should().BeTrue();
        SaneamentoDasMetas.Sanear(Linha() with { Linha = "CONSÓRCIO" }).Meta!.EhConsorcio.Should().BeTrue();
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("2.5")]
    [InlineData("três")]
    [InlineData(null)]
    public void Quantidade_que_nao_e_inteiro_nao_negativo_recusa_a_linha(string? bruto) =>
        SaneamentoDasMetas.Sanear(Linha() with { Quantidade = bruto }).Motivos.Should().Equal(MotivoDeRecusaDaMeta.QuantidadeInvalida);

    [Fact]
    public void Quantidade_com_zero_decimal_e_inteira() =>
        SaneamentoDasMetas.Sanear(Linha() with { Quantidade = "3.0" }).Meta!.Quantidade.Should().Be(3);

    [Fact]
    public void Tudo_errado_diz_cada_motivo()
    {
        var saneada = SaneamentoDasMetas.Sanear(new MetaNaOrigem(1, null, null, " ", "", "Leasing", "Feira", "x", null, null));

        saneada.Meta.Should().BeNull();
        saneada.Motivos.Should().BeEquivalentTo(
        [
            MotivoDeRecusaDaMeta.MesInvalido, MotivoDeRecusaDaMeta.FilialInvalida, MotivoDeRecusaDaMeta.LinhaInvalida,
            MotivoDeRecusaDaMeta.ConsultorInvalido, MotivoDeRecusaDaMeta.TipoDesconhecido, MotivoDeRecusaDaMeta.OrigemDesconhecida,
            MotivoDeRecusaDaMeta.QuantidadeInvalida
        ]);
    }

    [Fact]
    public void Valor_ilegivel_nao_recusa_a_linha_e_fica_anotado()
    {
        var saneada = SaneamentoDasMetas.Sanear(Linha() with { ValorUnitario = "R$ 1.234,00" });

        saneada.Motivos.Should().BeEmpty("o valor não entra na conta de unidades");
        saneada.Meta!.ValorUnitario.Should().BeNull();
        saneada.Meta.Transformacoes.Should().ContainSingle(t => t.StartsWith("valor unitário: ilegível", StringComparison.Ordinal));
    }

    [Fact]
    public void O_resumo_muda_quando_a_meta_muda_e_nao_muda_na_releitura()
    {
        var original = SaneamentoDasMetas.Sanear(Linha()).Meta!.Hash;

        SaneamentoDasMetas.Sanear(Linha()).Meta!.Hash.Should().Be(original, "a releitura do mesmo conteúdo não grava nada");
        SaneamentoDasMetas.Sanear(Linha() with { Quantidade = "3" }).Meta!.Hash.Should().NotBe(original);
        SaneamentoDasMetas.Sanear(Linha() with { Consultor = "outro.nome" }).Meta!.Hash.Should().NotBe(original);
    }
}
