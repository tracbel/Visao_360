using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O FINANCIAMENTO DA VENDA (issue 262) — o que conta como crédito rural (decisão de 28/09/2026: tudo menos recurso
/// próprio e consórcio), o processo cancelado e os limites do registro.
/// </summary>
public sealed class FinanciamentoDaVendaTestes
{
    private static readonly DateTime Agora = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    private static FinanciamentoDaVenda.Dados Dados(
        decimal valor = 450_000m, string? instituicao = "BANCO JOHN DEERE", string? linha = "MODER FROTA", int? municipio = 10,
        DateOnly? pedido = null, string hash = "a") =>
        new("IV_Q_VENDA_EQUIPAMENTO", pedido ?? new DateOnly(2026, 3, 10), false, municipio, valor, instituicao, linha, hash);

    [Theory]
    [InlineData("MODER FROTA", "BANCO JOHN DEERE", true)]
    [InlineData("PRONAF", "SICREDI", true)]
    [InlineData("PRONAMP", "Banco do Brasil", true)]
    [InlineData("FINAME", "BRADESCO", true)]
    [InlineData("PRO-TRATOR", "BANCO JOHN DEERE", true)]
    [InlineData("TFBD", "BANCO JOHN DEERE", true)]
    [InlineData(null, "SICOOB", true)]
    [InlineData("RECURSO PRÓPRIO", "NÃO SE APLICA", false)]
    [InlineData("Recurso proprio", "BANCO DO BRASIL", false)]
    [InlineData("CONSÓRCIO", "CONSÓRCIO JOHN DEERE", false)]
    [InlineData(null, "CONSORCIO JOHN DEERE", false)]
    [InlineData(null, "NÃO SE APLICA", false)]
    [InlineData("MODER FROTA", null, false)]
    [InlineData(null, null, false)]
    public void Conta_como_credito_rural_tudo_menos_proprio_e_consorcio(string? linha, string? instituicao, bool conta)
    {
        FinanciamentoDaVenda.ContaComoCreditoRural(linha, instituicao).Should().Be(conta);
    }

    [Theory]
    [InlineData("Cancelamento", "DESISTIU DA COMPRA", true)]
    [InlineData("Cancelado", null, true)]
    [InlineData("Análise", "CANCELADO", true)]
    [InlineData("Cancelamento", "VENDA PERDIDA", true)]
    [InlineData("Entrega Tecnica", "ENTREGA", false)]
    [InlineData("Recebimento", null, false)]
    [InlineData(null, null, false)]
    public void Processo_cancelado_nao_e_financiamento(string? fase, string? status, bool cancelado)
    {
        FinanciamentoDaVenda.ProcessoCancelado(fase, status).Should().Be(cancelado);
    }

    [Fact]
    public void Registra_em_maiusculas_e_marca_se_conta_no_credito()
    {
        var f = FinanciamentoDaVenda.Registrar(123, Dados(instituicao: " Banco John Deere ", linha: "Moder Frota"), 1, Agora);

        f.ProcessoNoVortice.Should().Be(123);
        f.InstituicaoFinanceira.Should().Be("BANCO JOHN DEERE");
        f.LinhaDeCredito.Should().Be("MODER FROTA");
        f.ContaNoCreditoRural.Should().BeTrue();
        f.ExcluidoEm.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(20_000_001)]
    public void Recusa_valor_fora_do_razoavel(decimal valor)
    {
        var registrar = () => FinanciamentoDaVenda.Registrar(1, Dados(valor: valor), 1, Agora);
        registrar.Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void Recusa_processo_invalido_e_data_antes_de_2012()
    {
        ((Action)(() => FinanciamentoDaVenda.Registrar(0, Dados(), 1, Agora))).Should().Throw<RegraDeNegocioViolada>();
        ((Action)(() => FinanciamentoDaVenda.Registrar(1, Dados(pedido: new DateOnly(2011, 12, 31)), 1, Agora)))
            .Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_releitura_igual_nao_muda_e_a_diferente_muda()
    {
        var f = FinanciamentoDaVenda.Registrar(1, Dados(), 1, Agora);

        f.AtualizarDaOrigem(Dados(), 2, Agora.AddDays(1)).Should().BeFalse();
        f.ImportadoPorId.Should().Be(1);

        f.AtualizarDaOrigem(Dados(linha: "RECURSO PRÓPRIO", hash: "b"), 2, Agora.AddDays(1)).Should().BeTrue();
        f.ContaNoCreditoRural.Should().BeFalse();

        f.AtualizarDaOrigem(Dados(linha: "RECURSO PRÓPRIO", hash: "b", municipio: 11), 2, Agora.AddDays(2)).Should().BeTrue(
            "o município casado depois muda a linha, mesmo com a origem igual");
    }

    [Fact]
    public void Excluir_guarda_a_primeira_data_e_reativar_limpa()
    {
        var f = FinanciamentoDaVenda.Registrar(1, Dados(), 1, Agora);
        f.Excluir(Agora);
        f.Excluir(Agora.AddDays(3));
        f.ExcluidoEm.Should().Be(Agora);

        f.Reativar();
        f.ExcluidoEm.Should().BeNull();
    }
}
