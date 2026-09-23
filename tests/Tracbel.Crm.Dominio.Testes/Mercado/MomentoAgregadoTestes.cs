using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// A AGREGAÇÃO DO MOMENTO (fase T3.1) — a razão entre o que o motor ajustou e o que ele estruturou.
///
/// <para><b>O que este teste existe para impedir:</b> que volte a haver uma regra de agregação
/// inventada. A primeira versão usava o índice de preço da cultura de maior área, e isso foi
/// recusado — era solução técnica sem decisão de negócio, e deixava uma cultura falar pelas outras.
/// A prova mais importante aqui é a de que <b>trocar qual cultura tem a maior área não dá salto
/// nenhum no agregado</b>.</para>
/// </summary>
public class MomentoAgregadoTestes
{
    private static readonly DateTime Agora = new(2026, 9, 23, 12, 0, 0, DateTimeKind.Utc);

    private static ParametroDoPotencial Parametros(decimal? minimo = 0.4m, decimal? maximo = 1.5m) =>
        ParametroDoPotencial.Informar(
            new ParametroDoPotencial.Valores(
                MesesDaJanela: 12, PesoDosContratosNoCredito: 0.70m,
                LimiteDeRetracao: 1.00m, LimiteDeAquecimento: 1.20m, LimiteDeSuperaquecimento: 1.40m,
                NomeDaFaixaIntermediaria: null, LimiteDaPercepcao: 5m,
                PesoDoIndicadorDePreco: 0.40m, PesoDoIndicadorDeCredito: 0.50m, PesoDoIndicadorComercial: 1.00m,
                FatorMinimo: minimo, FatorMaximo: maximo),
            ParametroComVigencia.HojeNoBrasil(Agora), "pesos do protótipo, a confirmar", 100, Agora);

    /// <summary>Uma cultura com a demanda dela e o índice de preço dela, já passada pelo motor.</summary>
    private static MomentoDaCultura Cultura(
        string nome, decimal? demanda, decimal? indiceDePreco, decimal? area = 100m,
        ParametroDoPotencial? parametro = null)
    {
        var ajustado = FatorDeCiclo.Ajustar(demanda, indiceDePreco, null, null, parametro ?? Parametros());
        return new MomentoDaCultura(nome.ToUpperInvariant(), nome, demanda, area, indiceDePreco,
            ajustado.Fator, ajustado.DemandaAjustada);
    }

    // ---------------------------------------------------------------------------------------------
    // Duas culturas com fatores diferentes
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Duas_culturas_com_fatores_diferentes_pesam_pela_demanda_que_representam()
    {
        // Cana retraída (preço 0,80 → fator 0,92) com 300 máq/ano; café aquecido (preço 1,30 →
        // fator 1,12) com 100. O agregado fica MAIS PERTO da cana, que é quem tem a demanda.
        var cana = Cultura("Cana", demanda: 300m, indiceDePreco: 0.80m);
        var cafe = Cultura("Café", demanda: 100m, indiceDePreco: 1.30m);

        var (fator, estrutural, ajustada, motivo) = MomentoAgregado.Agregar([cana, cafe]);

        motivo.Should().Be(nameof(MotivoSemFatorAgregado.Nenhum));
        estrutural.Should().Be(400m);
        ajustada.Should().Be(cana.DemandaAjustada!.Value + cafe.DemandaAjustada!.Value);
        fator.Should().Be(ajustada!.Value / 400m);

        // Entre os dois fatores, e mais perto do da cana.
        fator.Should().BeInRange(cana.Fator.Fator!.Value, cafe.Fator.Fator!.Value);
        (fator!.Value - cana.Fator.Fator!.Value).Should()
            .BeLessThan(cafe.Fator.Fator!.Value - fator.Value, "a cana carrega três quartos da demanda");
    }

    // ---------------------------------------------------------------------------------------------
    // Trocar a maior ÁREA não dá salto no agregado — o ponto da correção
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Trocar_qual_cultura_tem_a_maior_area_nao_muda_o_fator_agregado()
    {
        // As mesmas demandas e os mesmos preços; só a ÁREA troca de mãos. Na regra antiga isto
        // trocaria o índice do recorte inteiro e daria um salto; aqui não muda nada, porque a área
        // não participa da agregação.
        var comCanaMaior = new[]
        {
            Cultura("Cana", demanda: 300m, indiceDePreco: 0.80m, area: 9_000m),
            Cultura("Café", demanda: 100m, indiceDePreco: 1.30m, area: 1_000m),
        };

        var comCafeMaior = new[]
        {
            Cultura("Cana", demanda: 300m, indiceDePreco: 0.80m, area: 1_000m),
            Cultura("Café", demanda: 100m, indiceDePreco: 1.30m, area: 9_000m),
        };

        MomentoAgregado.Agregar(comCanaMaior).Fator
            .Should().Be(MomentoAgregado.Agregar(comCafeMaior).Fator,
                "a área diz qual cultura predomina, e não qual fator vale");

        // E a predominante muda, porque ela é contexto.
        MomentoAgregado.Predominante(comCanaMaior)!.Cultura.Should().Be("Cana");
        MomentoAgregado.Predominante(comCafeMaior)!.Cultura.Should().Be("Café");
    }

    // ---------------------------------------------------------------------------------------------
    // Neutralidade
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Com_todas_as_culturas_neutras_o_agregado_e_um()
    {
        var neutras = new[]
        {
            Cultura("Cana", demanda: 300m, indiceDePreco: null),
            Cultura("Café", demanda: 100m, indiceDePreco: 1.00m),
        };

        MomentoAgregado.Agregar(neutras).Fator.Should().Be(1m,
            "indicador ausente vale desvio zero, e índice 1,00 não desvia — a soma ajustada é a estrutural");
    }

    // ---------------------------------------------------------------------------------------------
    // Cultura sem demanda válida
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Cultura_sem_demanda_valida_nao_entra_na_conta_nem_puxa_o_agregado_para_baixo()
    {
        var semCiclo = Cultura("Laranja", demanda: null, indiceDePreco: 0.50m);
        var comCiclo = Cultura("Café", demanda: 100m, indiceDePreco: 1.30m);

        var (fator, estrutural, _, motivo) = MomentoAgregado.Agregar([semCiclo, comCiclo]);

        motivo.Should().Be(nameof(MotivoSemFatorAgregado.Nenhum));
        estrutural.Should().Be(100m, "só a cultura com demanda entra no denominador");
        fator.Should().Be(comCiclo.Fator.Fator, "com uma cultura só, o agregado é o fator dela");
    }

    [Fact]
    public void Demanda_zerada_nao_conta_como_peso()
    {
        var zerada = Cultura("Laranja", demanda: 0m, indiceDePreco: 0.50m);
        var comCiclo = Cultura("Café", demanda: 100m, indiceDePreco: 1.30m);

        MomentoAgregado.Agregar([zerada, comCiclo]).Estrutural.Should().Be(100m);
    }

    // ---------------------------------------------------------------------------------------------
    // Total estrutural nulo ou zero — ausência, e não 1,00
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void Sem_nenhuma_demanda_estrutural_o_agregado_e_ausente_com_o_motivo_do_ciclo()
    {
        var semCiclo = new[]
        {
            Cultura("Cana", demanda: null, indiceDePreco: 0.80m),
            Cultura("Café", demanda: null, indiceDePreco: 1.30m),
        };

        var (fator, estrutural, ajustada, motivo) = MomentoAgregado.Agregar(semCiclo);

        fator.Should().BeNull("\"não há base para dizer\" é diferente de \"o mercado está neutro\"");
        estrutural.Should().BeNull();
        ajustada.Should().BeNull();
        motivo.Should().Be(nameof(MotivoSemFatorAgregado.SemDemandaEstrutural));
    }

    [Fact]
    public void Sem_cultura_nenhuma_o_agregado_e_ausente() =>
        MomentoAgregado.Agregar([]).Motivo.Should().Be(nameof(MotivoSemFatorAgregado.SemDemandaEstrutural));

    [Fact]
    public void Com_demanda_mas_sem_pesos_registrados_o_motivo_e_outro()
    {
        // Há ciclo de renovação, mas os pesos do fator não foram decididos (D-P05): o motor devolve
        // fator nulo, e a tela precisa dizer que o que falta é DECISÃO, não dado.
        var semPesos = ParametroDoPotencial.Informar(
            new ParametroDoPotencial.Valores(
                MesesDaJanela: 12, PesoDosContratosNoCredito: 0.70m,
                LimiteDeRetracao: 1.00m, LimiteDeAquecimento: 1.20m, LimiteDeSuperaquecimento: 1.40m,
                NomeDaFaixaIntermediaria: null, LimiteDaPercepcao: 5m,
                PesoDoIndicadorDePreco: null, PesoDoIndicadorDeCredito: null, PesoDoIndicadorComercial: null,
                FatorMinimo: null, FatorMaximo: null),
            ParametroComVigencia.HojeNoBrasil(Agora), "pesos ainda não decididos", 100, Agora);

        var comDemanda = Cultura("Cana", demanda: 300m, indiceDePreco: 0.80m, parametro: semPesos);

        var (fator, _, _, motivo) = MomentoAgregado.Agregar([comDemanda]);

        fator.Should().BeNull();
        motivo.Should().Be(nameof(MotivoSemFatorAgregado.SemFatorPorCultura));
    }

    // ---------------------------------------------------------------------------------------------
    // O agregado respeita os limites do motor
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void O_agregado_nunca_escapa_dos_limites_registrados()
    {
        // Preços extremos nos dois sentidos. Cada fator já sai cortado em 0,40 e 1,50; a média
        // ponderada de números dentro do intervalo não pode sair dele.
        var apertado = Parametros(minimo: 0.90m, maximo: 1.10m);

        var extremas = new[]
        {
            Cultura("Cana", demanda: 300m, indiceDePreco: 0.10m, parametro: apertado),
            Cultura("Café", demanda: 100m, indiceDePreco: 9.00m, parametro: apertado),
        };

        var fator = MomentoAgregado.Agregar(extremas).Fator;

        fator.Should().BeInRange(0.90m, 1.10m);
        extremas[0].Fator.Fator.Should().Be(0.90m, "o corte age em cada cultura, antes da agregação");
        extremas[1].Fator.Fator.Should().Be(1.10m);
    }

    // ---------------------------------------------------------------------------------------------
    // A predominante é contexto, e diz o critério
    // ---------------------------------------------------------------------------------------------

    [Fact]
    public void A_predominante_traz_a_fatia_e_o_criterio_por_extenso()
    {
        var predominante = MomentoAgregado.Predominante([
            Cultura("Cana", demanda: 300m, indiceDePreco: 0.80m, area: 5_800m),
            Cultura("Café", demanda: 100m, indiceDePreco: 1.30m, area: 4_200m),
        ])!;

        predominante.Cultura.Should().Be("Cana");
        predominante.Fatia.Should().BeApproximately(58m, 0.01m);
        predominante.Criterio.Should().Be("maior área útil entre as culturas com regra de potencial");
    }

    [Fact]
    public void Sem_area_nao_ha_predominante_a_declarar() =>
        MomentoAgregado.Predominante([Cultura("Cana", demanda: 300m, indiceDePreco: 0.80m, area: null)])
            .Should().BeNull();
}
