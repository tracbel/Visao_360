using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Frota;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Frota;

/// <summary>
/// A TELEMETRIA DO OPERATIONS CENTER NO EQUIPAMENTO (decisão de 28/09/2026). O que estes testes prendem: só a leitura mais
/// nova entra, pela data e não pelo número; o horímetro implausível não entra; a posição leva o município junto; o
/// casamento é pelo chassi exato e, sem ele, pelo número de série que aponta uma máquina só; e a conta das conectadas
/// mede o "sem uso" a partir da leitura mais nova, e não do relógio.
/// </summary>
public sealed class TelemetriaDoOperationsCenterTestes
{
    private static readonly DateTime Dia20 = new(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Dia25 = new(2026, 9, 25, 10, 0, 0, DateTimeKind.Utc);

    private static Equipamento Maquina(string chassi = "1RW6110JCMR800001") =>
        Equipamento.RegistrarPelaIntegracao(1, Chassi.Criar(chassi), OrigemDoEquipamento.Protheus, 100);

    [Fact]
    public void O_horimetro_mais_novo_entra_e_o_mais_velho_nao_muda_nada()
    {
        var maquina = Maquina();

        maquina.RegistrarHorimetro(1500.456m, Dia20).Should().BeTrue();
        maquina.HorimetroAtual.Should().Be(1500.46m);
        maquina.HorimetroAtualizadoEm.Should().Be(Dia20);

        maquina.RegistrarHorimetro(1400m, Dia20.AddDays(-1)).Should().BeFalse("leitura mais velha que a gravada");
        maquina.RegistrarHorimetro(1500.456m, Dia20).Should().BeFalse("a mesma leitura, relida no dia seguinte");
        maquina.HorimetroAtual.Should().Be(1500.46m);
    }

    [Fact]
    public void Horimetro_menor_numa_leitura_mais_nova_entra_porque_manda_a_data()
    {
        var maquina = Maquina();
        maquina.RegistrarHorimetro(9000m, Dia20);

        maquina.RegistrarHorimetro(12m, Dia25).Should().BeTrue("motor ou painel trocado: a leitura nova é o que a máquina mostra hoje");
        maquina.HorimetroAtual.Should().Be(12m);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100_000.01)]
    [InlineData(374_466.75)]
    public void Horimetro_negativo_ou_acima_de_100_mil_horas_nao_entra(double horas)
    {
        var maquina = Maquina();

        maquina.RegistrarHorimetro((decimal)horas, Dia20).Should().BeFalse();
        maquina.HorimetroAtual.Should().BeNull();
        Equipamento.HorimetroPlausivel(Equipamento.HorimetroMaximoPlausivel).Should().BeTrue("o teto é aceito");
    }

    [Fact]
    public void A_posicao_mais_nova_entra_com_o_municipio_e_a_nula_quer_dizer_fora_de_Sao_Paulo()
    {
        var maquina = Maquina();

        maquina.RegistrarPosicao(Coordenada.Criar(-21.1767, -47.8208), Dia20, 42).Should().BeTrue();
        (maquina.PosicaoLatitude, maquina.PosicaoLongitude, maquina.PosicaoEm, maquina.MunicipioDaPosicaoId)
            .Should().Be((-21.1767m, -47.8208m, (DateTime?)Dia20, (int?)42));

        maquina.RegistrarPosicao(Coordenada.Criar(-22.9, -43.2), Dia20.AddHours(-1), null).Should().BeFalse("posição mais velha");
        maquina.MunicipioDaPosicaoId.Should().Be(42);

        maquina.RegistrarPosicao(Coordenada.Criar(-22.9, -43.2), Dia25, null).Should().BeTrue();
        maquina.MunicipioDaPosicaoId.Should().BeNull("a máquina foi para fora de São Paulo — o município vai junto com a posição");
    }

    [Fact]
    public void A_maquina_baixada_nao_recebe_telemetria()
    {
        var maquina = Maquina();
        maquina.Inativar(100);

        maquina.RegistrarHorimetro(10m, Dia20).Should().BeFalse();
        maquina.RegistrarPosicao(Coordenada.Criar(-21, -47), Dia20, 1).Should().BeFalse();
    }

    private static MaquinaNaTelemetria Conectada(string id, string? vin, DateTime? em = null) =>
        new(id, vin, 100m, em ?? Dia20, -21.2, -47.8, em ?? Dia20);

    [Fact]
    public void Casa_pelo_chassi_normalizado_e_pelo_numero_de_serie_que_aponta_uma_maquina_so()
    {
        MaquinaDoCrmParaTelemetria[] crm =
        [
            new(1, "1RW6110JCMR800001", null),
            new(2, "1RW6110JCMR800002", "PY6110J012345"),
            new(3, "1RW6110JCMR800003", "SERIEREPETIDA1"),
            new(4, "1RW6110JCMR800004", "SERIEREPETIDA1")
        ];
        MaquinaNaTelemetria[] conectadas =
        [
            Conectada("a", " 1rw6110jcmr 800001 "),
            Conectada("b", "py6110j012345"),
            Conectada("c", "SERIEREPETIDA1"),
            Conectada("d", "1RW9999ZZZZ999999"),
            Conectada("e", null),
            Conectada("f", "")
        ];

        var (casadas, desfechos) = CasamentoDaTelemetria.Casar(conectadas, crm);

        casadas.Select(c => (c.EquipamentoId, c.Maquina.IdNaOrigem)).Should().Equal((1L, "a"), (2L, "b"));
        desfechos[DesfechoDaTelemetria.CasadaPeloChassi].Should().Be(1);
        desfechos[DesfechoDaTelemetria.CasadaPeloNumeroDeSerie].Should().Be(1);
        desfechos[DesfechoDaTelemetria.SemParNoCrm].Should().Be(2, "o chassi fora do parque, e o número de série que está em duas máquinas");
        desfechos[DesfechoDaTelemetria.SemVin].Should().Be(2);
        desfechos.Values.Sum().Should().Be(conectadas.Length, "toda máquina lida cai em um desfecho, e um só");
    }

    [Fact]
    public void Dois_terminais_com_o_mesmo_chassi_vale_o_que_falou_por_ultimo()
    {
        MaquinaDoCrmParaTelemetria[] crm = [new(1, "1RW6110JCMR800001", null)];
        MaquinaNaTelemetria[] conectadas =
        [
            Conectada("antigo", "1RW6110JCMR800001", Dia20),
            Conectada("novo", "1RW6110JCMR800001", Dia25),
            Conectada("mais-antigo", "1RW6110JCMR800001", Dia20.AddDays(-30))
        ];

        var (casadas, desfechos) = CasamentoDaTelemetria.Casar(conectadas, crm);

        casadas.Should().ContainSingle().Which.Maquina.IdNaOrigem.Should().Be("novo");
        desfechos[DesfechoDaTelemetria.Repetida].Should().Be(2);
        desfechos[DesfechoDaTelemetria.CasadaPeloChassi].Should().Be(1);
    }

    [Fact]
    public void As_conectadas_do_municipio_medem_o_sem_uso_pela_leitura_mais_nova_e_nao_pelo_relogio()
    {
        MaquinaConectada[] maquinas =
        [
            new(3543402, 1000m, Dia25, Dia25),
            new(3543402, 3000m, Dia25.AddDays(-31), Dia25.AddDays(-31)),
            new(3543402, 5000m, Dia25.AddDays(-29), Dia25),
            new(3543402, null, null, Dia25),
            new(3551702, 800m, Dia20, Dia20)
        ];

        var porMunicipio = ParqueConectadoNoMunicipio.PorMunicipio(maquinas);

        var ribeirao = porMunicipio[3543402];
        ribeirao.Maquinas.Should().Be(4);
        ribeirao.ComHorimetro.Should().Be(3, "a sem horímetro conta como conectada, mas não entra no uso");
        ribeirao.SemUsoHa30Dias.Should().Be(1, "só a que ficou 31 dias sem hora nova antes da leitura mais nova");
        ribeirao.HorimetroMediano.Should().Be(3000m);
        ribeirao.Referencia.Should().Be(Dia25, "a leitura mais nova de toda a telemetria, igual para todos os municípios");

        porMunicipio[3551702].Referencia.Should().Be(Dia25);
        porMunicipio[3551702].SemUsoHa30Dias.Should().Be(0, "o dia 20 está a cinco dias da referência");
        ParqueConectadoNoMunicipio.PorMunicipio([]).Should().BeEmpty();
    }

    [Fact]
    public void A_mediana_de_um_numero_par_e_a_media_dos_dois_do_meio()
    {
        var porMunicipio = ParqueConectadoNoMunicipio.PorMunicipio(
        [
            new(1, 1000m, Dia20, Dia20),
            new(1, 2001m, Dia20, Dia20)
        ]);

        porMunicipio[1].HorimetroMediano.Should().Be(1500m, "(1000 + 2001) / 2 = 1500,5, arredondado para a hora");
    }
}
