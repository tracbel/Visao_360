using FluentAssertions;
using Tracbel.Crm.Dominio.Mercado;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Mercado;

/// <summary>
/// O COMPARTILHAMENTO DE MÁQUINA ENTRE CULTURAS (issue 160) — o que impede contar a mesma terra e a
/// mesma máquina duas vezes.
///
/// <para>Em São Paulo o milho safrinha é plantado depois da soja, no mesmo talhão, e boa parte do
/// amendoim entra em reforma de canavial (achado C-02 do documento 49). O protótipo soma as seis
/// culturas: o mesmo talhão vira duas áreas, e o parque teórico sai inflado.</para>
/// </summary>
[Trait("Categoria", "Dominio")]
public sealed class PotencialEstruturalTestes
{
    private static AreaDaCulturaNoGrupo Soja(decimal? area, decimal? haPorMaquina = 500m) =>
        new("SOJA", "Soja", area, haPorMaquina);

    private static AreaDaCulturaNoGrupo Milho(decimal? area, decimal? haPorMaquina = 400m) =>
        new("MILHO", "Milho", area, haPorMaquina);

    [Fact]
    public void A_area_util_do_grupo_e_a_MAIOR_e_nao_a_soma()
    {
        // O DEFEITO QUE ESTE TESTE IMPEDE: 10.000 ha de soja + 8.000 de milho safrinha no mesmo talhão
        // não são 18.000 ha de terra. São 10.000 — e a máquina que passa neles é dimensionada pela
        // cultura que ocupa mais.
        var parque = PotencialEstrutural.Parque([Soja(10_000m), Milho(8_000m)]);

        parque.AreaUtilHectares.Should().Be(10_000m);
        parque.CulturaDominante.Should().Be("Soja");
        parque.Maquinas.Should().Be(20m, "10.000 ha ÷ 500 ha por máquina, a regra da dominante");
        parque.Compartilhada.Should().Equal(["Milho"]);
    }

    [Fact]
    public void O_parametro_usado_e_o_da_dominante_e_nao_uma_media()
    {
        // Uma média entre 500 e 400 ha por máquina descreveria uma máquina que não existe.
        var comMilhoMaior = PotencialEstrutural.Parque([Soja(6_000m), Milho(9_000m)]);

        comMilhoMaior.CulturaDominante.Should().Be("Milho");
        comMilhoMaior.Maquinas.Should().Be(22.5m, "9.000 ÷ 400, a regra do milho");
    }

    [Fact]
    public void Sem_grupo_o_numero_e_exatamente_o_que_era()
    {
        // NEUTRALIDADE DELIBERADA: enquanto ninguém configurar grupo (D-IM-01 não saiu), cada cultura
        // soma a área dela. É o critério de aceite: "sem grupo, é igual".
        var soltas = new[] { Soja(10_000m), Milho(8_000m) };

        var comGrupo = PotencialEstrutural.ParqueDoRecorte([[.. soltas]], []);
        var semGrupo = PotencialEstrutural.ParqueDoRecorte([], soltas);

        semGrupo.Should().Be(20m + 20m, "10.000÷500 + 8.000÷400");
        comGrupo.Should().Be(20m);
        comGrupo.Should().BeLessThan(semGrupo!.Value, "com grupo, o parque nunca é maior que a soma simples");
    }

    [Fact]
    public void O_parque_com_grupo_nunca_e_maior_que_a_soma_simples()
    {
        // É o critério de aceite, e sai de graça da regra: a maior das áreas é menor ou igual à soma.
        var culturas = new[] { Soja(4_000m), Milho(11_000m) };

        var comGrupo = PotencialEstrutural.ParqueDoRecorte([[.. culturas]], [])!.Value;
        var semGrupo = PotencialEstrutural.ParqueDoRecorte([], culturas)!.Value;

        comGrupo.Should().BeLessThanOrEqualTo(semGrupo);
    }

    [Fact]
    public void Grupo_vazio_nao_devolve_zero_maquinas()
    {
        // Zero diria "não há potencial"; o certo é "não se sabe" — a fonte não divulgou a área.
        var vazio = PotencialEstrutural.Parque([]);

        vazio.Maquinas.Should().BeNull();
        vazio.AreaUtilHectares.Should().BeNull();
        vazio.CulturaDominante.Should().BeNull();
    }

    [Fact]
    public void Cultura_sem_area_no_grupo_e_ignorada_e_o_grupo_vale_pelas_outras()
    {
        // Sigilo do IBGE num município não pode zerar o grupo inteiro.
        var parque = PotencialEstrutural.Parque([Soja(null), Milho(8_000m)]);

        parque.CulturaDominante.Should().Be("Milho");
        parque.AreaUtilHectares.Should().Be(8_000m);
        parque.Compartilhada.Should().BeEmpty("a soja não entrou na conta, então não é 'compartilhada com'");
    }

    [Fact]
    public void Cultura_sem_regra_de_potencial_nao_inventa_maquina()
    {
        // Sem hectares por máquina, o parque não sai — mas a área útil continua sendo dita, porque ela
        // é conhecida e serve ao resto da tela.
        var parque = PotencialEstrutural.Parque([Soja(10_000m, haPorMaquina: null)]);

        parque.Maquinas.Should().BeNull();
        parque.AreaUtilHectares.Should().Be(10_000m);
    }

    [Fact]
    public void Dois_grupos_somam_entre_si()
    {
        var graos = new[] { Soja(10_000m), Milho(8_000m) };
        var perenes = new[] { new AreaDaCulturaNoGrupo("CAFE", "Café", 3_000m, 10m) };

        PotencialEstrutural.ParqueDoRecorte([graos, perenes], []).Should().Be(20m + 300m);
    }

    [Fact]
    public void A_frase_do_tooltip_diz_com_quem_a_area_e_compartilhada()
    {
        var comGrupo = PotencialEstrutural.Parque([Soja(10_000m), Milho(8_000m)]);
        var sozinha = PotencialEstrutural.Parque([Soja(10_000m)]);

        PotencialEstrutural.FraseDoCompartilhamento(comGrupo).Should().Contain("compartilhada com Milho");
        PotencialEstrutural.FraseDoCompartilhamento(sozinha).Should().BeEmpty(
            "a tela não mostra rodapé para dizer que não há compartilhamento");
    }
}
