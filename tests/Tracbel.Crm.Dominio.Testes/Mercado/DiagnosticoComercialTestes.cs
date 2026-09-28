using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// O IOC do Diagnóstico Comercial (issue 257): as contas do protótipo da pasta 360, componente por componente, e as
/// três diferenças — componente sem dado sai da conta, venda levada a um ano, cobertura pela cadência.
/// </summary>
public sealed class DiagnosticoComercialTestes
{
    private static readonly PesosDoIoc DoPrototipo = new(25, 20, 15, 15, 10, 5, 10);

    private static MunicipioParaOIoc Municipio(
        int codigo = 1,
        decimal? estrutural = 10m,
        decimal? ajustada = 10m,
        decimal? meta = 3.1m,
        decimal? vendidas = 1m,
        int clientes = 5,
        int vinculos = 10,
        int cobertos = 4,
        decimal? credito = 1m,
        decimal? preco = 1m,
        string? cultura = "Cana") =>
        new(codigo, estrutural, ajustada, meta, vendidas, clientes, vinculos, cobertos, credito, preco, cultura);

    [Fact]
    public void Os_componentes_sao_as_contas_do_prototipo()
    {
        // Dez municípios com demanda 1..10: o percentil 90 é o décimo menos um, 10.
        var municipios = Enumerable.Range(1, 10).Select(i => Municipio(i, estrutural: i, ajustada: i)).ToList();

        var ioc = DiagnosticoComercial.Calcular(municipios, DoPrototipo).Single(r => r.CodigoIbge == 5);

        ioc.Componentes.Potencial.Should().Be(0.5m, "5 ÷ p90 (10)");
        ioc.Componentes.Cobertura.Should().Be(0.6m, "1 − 4 cobertos ÷ 10 vínculos");
        ioc.Componentes.Credito.Should().Be(0.5m, "crédito estável fica no meio da escala");
        ioc.Componentes.Rentabilidade.Should().Be(0.5m);
        ioc.Componentes.Clientes.Should().Be(0m, "5 clientes para 5 máquinas por ano");
        ioc.Componentes.Realizacao.Should().BeApproximately(1 - 1m / 3.1m, 0.0001m);
        ioc.Componentes.Penetracao.Should().Be(0.8m, "1 vendida para 5 de demanda");
    }

    [Fact]
    public void O_ioc_e_a_media_ponderada_normalizada_pela_soma_dos_pesos()
    {
        var m = Municipio(estrutural: 10, ajustada: 10, meta: 2m, vendidas: 1m, clientes: 5, vinculos: 10, cobertos: 5, credito: 1.2m, preco: 0.8m);

        var r = DiagnosticoComercial.Calcular([m], DoPrototipo).Single();

        // potencial 1 · cobertura 0,5 · crédito 0,75 · rentabilidade 0,25 · clientes 0,5 · realização 0,5 · penetração 0,9
        var esperado = (25 * 1m + 20 * 0.5m + 15 * 0.75m + 15 * 0.25m + 10 * 0.5m + 5 * 0.5m + 10 * 0.9m) / 100m * 100m;
        r.Ioc.Should().Be(decimal.Round(esperado, 1));
        r.Classe.Should().Be(ClasseDePrioridade.Alta);
    }

    [Theory]
    [InlineData(80, ClasseDePrioridade.Maxima)]
    [InlineData(79.9, ClasseDePrioridade.Alta)]
    [InlineData(60, ClasseDePrioridade.Alta)]
    [InlineData(40, ClasseDePrioridade.Moderada)]
    [InlineData(20, ClasseDePrioridade.Baixa)]
    [InlineData(19.9, ClasseDePrioridade.Manutencao)]
    public void As_classes_vao_de_vinte_em_vinte(double ioc, ClasseDePrioridade classe) =>
        DiagnosticoComercial.Classificar((decimal)ioc).Should().Be(classe);

    [Theory]
    [InlineData(0.6, 0)]
    [InlineData(0.8, 0.25)]
    [InlineData(1.0, 0.5)]
    [InlineData(1.4, 1)]
    [InlineData(2.0, 1)]
    public void Os_indices_de_momento_vao_de_menos_40_a_mais_40(double indice, double escala) =>
        DiagnosticoComercial.NaEscala((decimal)indice).Should().Be((decimal)escala);

    [Fact]
    public void Sem_o_ART_a_realizacao_e_a_penetracao_saem_da_conta_e_a_linha_diz_por_que()
    {
        var comVendas = DiagnosticoComercial.Calcular([Municipio(vendidas: 1m)], DoPrototipo).Single();
        var semArt = DiagnosticoComercial.Calcular([Municipio(vendidas: null)], DoPrototipo).Single();

        semArt.Componentes.Realizacao.Should().BeNull("ausência de carga não é venda zero");
        semArt.Componentes.Penetracao.Should().BeNull();
        semArt.Ioc.Should().NotBeNull("os outros cinco componentes continuam medindo");
        semArt.ComponentesAusentes.Should().Contain(a => a.StartsWith("realização") && a.Contains("ART"));
        semArt.ComponentesAusentes.Should().Contain(a => a.StartsWith("penetração"));
        comVendas.ComponentesAusentes.Should().BeEmpty();
    }

    [Fact]
    public void Sem_carteira_a_cobertura_sai_da_conta_em_vez_de_valer_meio()
    {
        var r = DiagnosticoComercial.Calcular([Municipio(vinculos: 0, cobertos: 0, clientes: 0)], DoPrototipo).Single();

        r.Componentes.Cobertura.Should().BeNull();
        r.Cobertura.Should().BeNull();
        r.ComponentesAusentes.Should().Contain(a => a.StartsWith("cobertura"));
    }

    [Fact]
    public void Peso_zero_nao_conta_nem_como_ausente()
    {
        var semRealizacao = DoPrototipo with { Realizacao = 0 };

        var r = DiagnosticoComercial.Calcular([Municipio(meta: null)], semRealizacao).Single();

        r.ComponentesAusentes.Should().NotContain(a => a.StartsWith("realização"));
    }

    [Fact]
    public void Sem_nenhum_componente_com_dado_nao_ha_indice_nem_classe()
    {
        var vazio = new MunicipioParaOIoc(1, null, null, null, null, 0, 0, 0, null, null, null);

        var r = DiagnosticoComercial.Calcular([vazio], DoPrototipo).Single();

        r.Ioc.Should().BeNull();
        r.Classe.Should().BeNull();
        r.Situacao.Should().Equal("situação equilibrada");
    }

    [Fact]
    public void A_demanda_ajustada_vale_mais_que_a_estrutural_quando_existe()
    {
        var municipios = new[] { Municipio(1, estrutural: 10, ajustada: 5), Municipio(2, estrutural: 10, ajustada: 10) };

        var r = DiagnosticoComercial.Calcular(municipios, DoPrototipo);

        r.Single(x => x.CodigoIbge == 1).Componentes.Potencial.Should().Be(0.5m);
    }

    [Fact]
    public void Situacao_e_plano_seguem_as_regras_do_prototipo()
    {
        // Potencial alto, ninguém coberto, crédito e preço em alta, poucos clientes, nada vendido.
        var quente = Municipio(estrutural: 20, ajustada: 20, meta: 6m, vendidas: 0m, clientes: 2, vinculos: 10, cobertos: 0,
            credito: 1.3m, preco: 1.3m, cultura: "Soja");

        var r = DiagnosticoComercial.Calcular([quente], DoPrototipo).Single();

        r.Situacao.Should().Contain("elevado potencial").And.Contain("baixa cobertura comercial")
            .And.Contain("crédito de mecanização em alta").And.Contain("rentabilidade favorável (Soja)")
            .And.Contain("poucos clientes frente ao potencial").And.Contain("abaixo da meta de share")
            .And.Contain("baixa penetração no potencial (0%)");
        r.PlanoDeAcao.Should().Equal(
            "Campanha de renovação de frota", "Expandir cobertura e visitas presenciais", "Mapear e prospectar novos clientes");
        r.Classe.Should().Be(ClasseDePrioridade.Maxima);
    }

    [Fact]
    public void Municipio_frio_pede_manutencao()
    {
        var frio = Municipio(1, estrutural: 1, ajustada: 1, meta: 0.3m, vendidas: 2m, clientes: 5, vinculos: 10, cobertos: 10,
            credito: 0.6m, preco: 0.6m);
        var regua = Municipio(2, estrutural: 100, ajustada: 100);

        var r = DiagnosticoComercial.Calcular([frio, regua], DoPrototipo).Single(x => x.CodigoIbge == 1);

        r.Classe.Should().Be(ClasseDePrioridade.Manutencao);
        r.PlanoDeAcao.Should().Equal("Manutenção do relacionamento");
        r.Situacao.Should().Contain("potencial modesto").And.Contain("crédito retraído").And.Contain("meta de share atingida");
    }

    [Fact]
    public void O_percentil_90_e_o_do_prototipo()
    {
        // O índice é floor(n × 0,9) na lista ordenada: com 20 municípios, o 19º; com 10, o último.
        DiagnosticoComercial.Percentil90([.. Enumerable.Range(1, 20).Select(i => (decimal)i)]).Should().Be(19m);
        DiagnosticoComercial.Percentil90([.. Enumerable.Range(1, 10).Select(i => (decimal)i)]).Should().Be(10m);
        DiagnosticoComercial.Percentil90([]).Should().BeNull();
    }
}
