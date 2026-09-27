using FluentAssertions;
using Tracbel.Crm.Dominio.Integracao;
using Tracbel.Crm.Dominio.Processo;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Processo;

/// <summary>
/// A REGRA DO ESTÁGIO DO FUNIL (decisões de 27/09/2026, documento 52 §3). O que estes testes prendem, uma linha da regra
/// por teste: cumulativo (inclusive Lead e Qualificado); 2607/2609/2610 também na Cobertura; o pai DNA sai e o filho
/// herda com a data mínima; o neto herda da cadeia; a inclusão nula vira o primeiro andamento; a data futura é recusada;
/// a janela; e o pai de outro tipo não transmite nada. A classificação é a SEMENTE de verdade, não uma inventada aqui.
/// </summary>
public sealed class RegraDoEstagioTestes
{
    private static readonly DateTime Agora = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    private static readonly IReadOnlyDictionary<int, EstagioDoFunil> Classificacao =
        ClassificacaoDeResultadoDoVortice.Semente.Where(i => i.Estagio is not null).ToDictionary(i => i.Codigo, i => i.Estagio!.Value);

    private static DateTime Em(int ano, int mes, int dia) => new(ano, mes, dia, 13, 0, 0, DateTimeKind.Utc);

    private static ProcessoNaRegraDoEstagio Processo(long numero, DateTime? incluido, long? dna = null, short tipo = 41, DateTime? primeiroAndamento = null) =>
        new(numero, tipo, dna ?? numero, incluido, primeiroAndamento ?? incluido);

    private static long _seq;

    private static ResultadoNaRegraDoEstagio Resultado(long processo, int codigo, DateTime quando, int? acao = null) =>
        new(Interlocked.Increment(ref _seq), processo, codigo, quando, acao);

    private static IReadOnlyList<ApuracaoDoProcesso> Apurar(IEnumerable<ProcessoNaRegraDoEstagio> processos, params ResultadoNaRegraDoEstagio[] historico) =>
        RegraDoEstagio.Apurar(processos, historico, Classificacao, Agora);

    private static ApuracaoDoProcesso Do(IEnumerable<ApuracaoDoProcesso> apuracoes, long numero) => apuracoes.Single(a => a.Numero == numero);

    [Fact]
    public void O_estagio_e_cumulativo_quem_chegou_ao_pedido_conta_em_todos_os_anteriores_inclusive_Lead_e_Qualificado()
    {
        var apuracao = Do(Apurar([Processo(1, Em(2024, 1, 5))],
            Resultado(1, 250, Em(2024, 1, 10)),   // visita: Cobertura
            Resultado(1, 3239, Em(2024, 3, 1))),  // venda aprovada: Pedido
            1);

        apuracao.Situacao.Should().Be(SituacaoNaRegraDoEstagio.NoFunil);
        apuracao.Estagios.Select(e => e.Estagio).Should().Equal(
            EstagioDoFunil.Lead, EstagioDoFunil.Qualificado, EstagioDoFunil.Cobertura, EstagioDoFunil.Negociacao, EstagioDoFunil.Pedido);

        // A DATA DE CADA ESTÁGIO é o PRIMEIRO resultado que o alcança: Lead, Qualificado e Cobertura pela visita; a
        // Negociação e o Pedido pela venda aprovada.
        apuracao.Estagios.Single(e => e.Estagio == EstagioDoFunil.Lead).AlcancadoEm.Should().Be(Em(2024, 1, 10));
        apuracao.Estagios.Single(e => e.Estagio == EstagioDoFunil.Cobertura).ResultadoQueAbriu.Should().Be(250);
        apuracao.Estagios.Single(e => e.Estagio == EstagioDoFunil.Negociacao).AlcancadoEm.Should().Be(Em(2024, 3, 1));
        apuracao.Estagios.Single(e => e.Estagio == EstagioDoFunil.Pedido).ResultadoQueAbriu.Should().Be(3239);
        apuracao.Estagios.Should().OnlyContain(e => !e.PelaEntradaDigital && !e.Herdado);
    }

    [Fact]
    public void A_entrada_digital_marca_o_subfunil_sem_somar_os_dois_codigos()
    {
        var apuracoes = Apurar([Processo(1, Em(2024, 1, 5)), Processo(2, Em(2024, 1, 5))],
            Resultado(1, 1278, Em(2024, 1, 6)),   // só o contato digital: Lead
            Resultado(2, 1278, Em(2024, 1, 6)),
            Resultado(2, 3803, Em(2024, 1, 8)),   // qualificado
            Resultado(2, 252, Em(2024, 2, 1)));   // e a visita

        Do(apuracoes, 1).Estagios.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new { Estagio = EstagioDoFunil.Lead, ResultadoQueAbriu = 1278, PelaEntradaDigital = true });

        var digital = Do(apuracoes, 2).Estagios;
        digital.Select(e => (e.Estagio, e.AlcancadoEm, e.PelaEntradaDigital)).Should().Equal(
            (EstagioDoFunil.Lead, Em(2024, 1, 6), true),
            (EstagioDoFunil.Qualificado, Em(2024, 1, 8), true),
            (EstagioDoFunil.Cobertura, Em(2024, 2, 1), true));
    }

    [Theory]
    [InlineData(2607)]
    [InlineData(2609)]
    [InlineData(2610)]
    public void Os_tres_codigos_que_o_extrator_esquece_na_Cobertura_contam_na_Cobertura_e_na_Negociacao(int codigo)
    {
        var apuracao = Do(Apurar([Processo(1, Em(2024, 6, 1))], Resultado(1, codigo, Em(2024, 6, 2))), 1);

        apuracao.Estagios.Select(e => e.Estagio).Should().Equal(
            EstagioDoFunil.Lead, EstagioDoFunil.Qualificado, EstagioDoFunil.Cobertura, EstagioDoFunil.Negociacao);
    }

    [Fact]
    public void O_pai_com_filho_DNA_sai_e_o_filho_herda_com_a_data_minima()
    {
        var apuracoes = Apurar(
            [Processo(10, Em(2024, 1, 5)), Processo(11, Em(2024, 2, 20), dna: 10)],
            Resultado(10, 250, Em(2024, 1, 10)),    // Cobertura do pai
            Resultado(10, 3239, Em(2024, 3, 1)),    // Pedido do pai, DEPOIS da negociação do filho
            Resultado(11, 2563, Em(2024, 2, 25)));  // Negociação do próprio filho

        Do(apuracoes, 10).Situacao.Should().Be(SituacaoNaRegraDoEstagio.PaiSubstituidoPeloFilhoDna);
        Do(apuracoes, 10).Estagios.Should().BeEmpty("o pai com filho DNA sai: quem fica é o filho");

        var filho = Do(apuracoes, 11).Estagios.ToDictionary(e => e.Estagio);
        filho.Keys.Should().BeEquivalentTo([EstagioDoFunil.Lead, EstagioDoFunil.Qualificado, EstagioDoFunil.Cobertura,
            EstagioDoFunil.Negociacao, EstagioDoFunil.Pedido]);

        // A COBERTURA E O PEDIDO VÊM DO PAI; a Negociação é do próprio filho, que chegou lá antes do pedido do pai.
        filho[EstagioDoFunil.Cobertura].Should().BeEquivalentTo(new { AlcancadoEm = Em(2024, 1, 10), NumeroDoProcessoDna = (long?)10, Herdado = true });
        filho[EstagioDoFunil.Negociacao].Should().BeEquivalentTo(new { AlcancadoEm = Em(2024, 2, 25), NumeroDoProcessoDna = (long?)null, Herdado = false });
        filho[EstagioDoFunil.Pedido].Should().BeEquivalentTo(new { AlcancadoEm = Em(2024, 3, 1), NumeroDoProcessoDna = (long?)10, ResultadoQueAbriu = 3239 });
    }

    [Fact]
    public void O_neto_herda_da_cadeia_inteira_e_so_o_ultimo_da_cadeia_fica()
    {
        var apuracoes = Apurar(
            [Processo(20, Em(2024, 1, 5)), Processo(21, Em(2024, 2, 1), dna: 20), Processo(22, Em(2024, 3, 1), dna: 21)],
            Resultado(20, 2529, Em(2024, 1, 20)));  // o avô faturou

        Do(apuracoes, 20).Situacao.Should().Be(SituacaoNaRegraDoEstagio.PaiSubstituidoPeloFilhoDna);
        Do(apuracoes, 21).Situacao.Should().Be(SituacaoNaRegraDoEstagio.PaiSubstituidoPeloFilhoDna);

        var neto = Do(apuracoes, 22);
        neto.Situacao.Should().Be(SituacaoNaRegraDoEstagio.NoFunil);
        neto.Estagios.Should().HaveCount(6);
        neto.Estagios.Should().OnlyContain(e => e.NumeroDoProcessoDna == 20 && e.AlcancadoEm == Em(2024, 1, 20));
    }

    [Fact]
    public void O_pai_de_outro_tipo_nao_transmite_nada_e_nao_e_substituido()
    {
        // O processo 30 é do tipo 12 (fora do funil): o BI filtra o histórico por 31/41/50, e a herança não passa.
        var apuracoes = Apurar(
            [Processo(30, Em(2024, 1, 5), tipo: 12), Processo(31, Em(2024, 2, 1), dna: 30)],
            Resultado(30, 3239, Em(2024, 1, 20)),
            Resultado(31, 250, Em(2024, 2, 10)));

        apuracoes.Select(a => a.Numero).Should().Equal(31);
        Do(apuracoes, 31).Estagios.Should().OnlyContain(e => !e.Herdado)
            .And.Subject.Max(e => e.Estagio).Should().Be(EstagioDoFunil.Cobertura);
    }

    [Fact]
    public void A_inclusao_nula_vira_o_primeiro_andamento_pelo_COALESCE_e_fica_marcada()
    {
        var apuracao = Do(Apurar(
            [new ProcessoNaRegraDoEstagio(40, 41, 40, IncluidoEm: null, PrimeiroAndamentoEm: Em(2024, 5, 1))],
            Resultado(40, 250, Em(2024, 5, 3))), 40);

        apuracao.AbertoEm.Should().Be(Em(2024, 5, 1));
        apuracao.AberturaDeduzida.Should().BeTrue();
        apuracao.Situacao.Should().Be(SituacaoNaRegraDoEstagio.NoFunil, "a carga antiga recusava estes 3.280 processos");
    }

    [Fact]
    public void Sem_inclusao_e_sem_andamento_proprio_a_abertura_e_o_primeiro_resultado_herdado()
    {
        var apuracao = Do(Apurar(
            [Processo(50, Em(2024, 1, 5)), new ProcessoNaRegraDoEstagio(51, 41, 50, IncluidoEm: null, PrimeiroAndamentoEm: null)],
            Resultado(50, 250, Em(2024, 1, 10))), 51);

        apuracao.AbertoEm.Should().Be(Em(2024, 1, 10));
        apuracao.AberturaDeduzida.Should().BeTrue();
    }

    [Fact]
    public void O_resultado_com_data_futura_e_recusado()
    {
        var apuracoes = Apurar([Processo(60, Em(2024, 1, 5)), Processo(61, Em(2024, 1, 5))],
            Resultado(60, 2529, Agora.AddDays(2)),        // digitado no futuro
            Resultado(61, 250, Em(2024, 1, 10)),
            Resultado(61, 2529, Agora.AddHours(20)));     // dentro da tolerância de um dia

        var soFuturo = Do(apuracoes, 60);
        soFuturo.Situacao.Should().Be(SituacaoNaRegraDoEstagio.SemResultadoAceito);
        soFuturo.ResultadosComDataFutura.Should().Be(1);

        Do(apuracoes, 61).Estagios.Max(e => e.Estagio).Should().Be(EstagioDoFunil.Faturamento, "até agora + 1 dia ainda vale");
    }

    [Fact]
    public void A_janela_unica_comeca_em_1_de_novembro_de_2023_em_Sao_Paulo()
    {
        var apuracoes = Apurar(
            [Processo(70, new DateTime(2023, 11, 1, 2, 59, 59, DateTimeKind.Utc)), Processo(71, RegraDoEstagio.InicioDaJanela)],
            Resultado(70, 250, Em(2024, 1, 10)),
            Resultado(71, 250, Em(2024, 1, 10)));

        Do(apuracoes, 70).Situacao.Should().Be(SituacaoNaRegraDoEstagio.ForaDaJanela, "31/10/2023 23:59 em São Paulo");
        Do(apuracoes, 71).Situacao.Should().Be(SituacaoNaRegraDoEstagio.NoFunil);
    }

    [Fact]
    public void Sem_resultado_aceito_o_processo_fica_fora_do_funil_com_o_motivo()
    {
        var apuracao = Do(Apurar([Processo(80, Em(2024, 1, 5))], Resultado(80, 9999, Em(2024, 1, 10))), 80);

        apuracao.Situacao.Should().Be(SituacaoNaRegraDoEstagio.SemResultadoAceito);
        apuracao.Estagios.Should().BeEmpty();
    }

    [Fact]
    public void A_ultima_acao_da_etapa_e_a_do_BI_so_no_proprio_processo_e_nula_em_Lead_e_Qualificado()
    {
        var apuracao = Do(Apurar([Processo(90, Em(2024, 1, 5))],
            Resultado(90, 250, Em(2024, 1, 10), acao: 50),
            Resultado(90, 9999, Em(2024, 4, 1), acao: 767),     // a ação conta mesmo sem resultado aceito
            Resultado(90, 2563, Em(2024, 2, 1), acao: 614)), 90);

        var porEstagio = apuracao.Estagios.ToDictionary(e => e.Estagio, e => e.UltimaAcaoDaEtapaEm);
        porEstagio[EstagioDoFunil.Lead].Should().BeNull();
        porEstagio[EstagioDoFunil.Qualificado].Should().BeNull();
        porEstagio[EstagioDoFunil.Cobertura].Should().Be(Em(2024, 4, 1), "767 é ação da Cobertura e da Negociação");
        porEstagio[EstagioDoFunil.Negociacao].Should().Be(Em(2024, 4, 1));
    }

    [Fact]
    public void O_pai_aberto_antes_da_janela_transmite_e_o_filho_herda_um_alcance_anterior_a_propria_abertura()
    {
        // O PAI É DE 2022 (fora da janela) e o filho, de 2024. O filho herda a Cobertura do pai, com a data de 2022 —
        // anterior à abertura dele e à janela. A visão por fluxo (PR 2) precisa saber disso: filtrar AlcancadoEm pelo
        // período deixa esse estágio de fora do fluxo de 2024, embora o processo esteja na coorte de 2024.
        var apuracoes = Apurar(
            [Processo(100, Em(2022, 5, 1)), Processo(101, Em(2024, 2, 1), dna: 100)],
            Resultado(100, 250, Em(2022, 5, 10)),
            Resultado(101, 2563, Em(2024, 2, 5)));

        Do(apuracoes, 100).Situacao.Should().Be(SituacaoNaRegraDoEstagio.ForaDaJanela, "o pai é de antes de 01/11/2023");

        var filho = Do(apuracoes, 101);
        filho.AbertoEm.Should().Be(Em(2024, 2, 1));
        var cobertura = filho.Estagios.Single(e => e.Estagio == EstagioDoFunil.Cobertura);
        cobertura.AlcancadoEm.Should().Be(Em(2022, 5, 10)).And.BeBefore(filho.AbertoEm!.Value);
        cobertura.NumeroDoProcessoDna.Should().Be(100);
        filho.Estagios.Single(e => e.Estagio == EstagioDoFunil.Negociacao).AlcancadoEm.Should().Be(Em(2024, 2, 5));
    }

    [Fact]
    public void A_entrada_digital_e_por_onde_o_processo_entrou_e_nao_o_que_ele_teve_depois()
    {
        // O PROCESSO CHEGOU À COBERTURA POR VISITA; o contato digital (1278) e o lead qualificado (3803) vieram depois.
        var apuracao = Do(Apurar([Processo(110, Em(2024, 1, 5))],
            Resultado(110, 250, Em(2024, 1, 10)),
            Resultado(110, 1278, Em(2024, 2, 1)),
            Resultado(110, 3803, Em(2024, 2, 2))), 110);

        apuracao.Estagios.Should().OnlyContain(e => !e.PelaEntradaDigital,
            "o Lead, o Qualificado e a Cobertura foram alcançados pela visita de 10/01; o digital veio depois");

        // A MESMA DATA CONTA: o contato digital no dia da visita é a entrada.
        var noMesmoDia = Do(Apurar([Processo(111, Em(2024, 1, 5))],
            Resultado(111, 250, Em(2024, 1, 10)),
            Resultado(111, 1278, Em(2024, 1, 10))), 111);
        noMesmoDia.Estagios.Single(e => e.Estagio == EstagioDoFunil.Cobertura).PelaEntradaDigital.Should().BeTrue();
        noMesmoDia.Estagios.Single(e => e.Estagio == EstagioDoFunil.Qualificado).PelaEntradaDigital.Should().BeFalse("não houve 3803");
    }

    [Fact]
    public void A_abertura_absurda_e_recusada_e_vale_a_seguinte_e_sem_nenhuma_crivel_o_processo_fica_pendente()
    {
        var apuracoes = Apurar(
            [
                // INCLUSÃO EM 1900: campo vazio virado data — vale o primeiro andamento.
                new ProcessoNaRegraDoEstagio(120, 41, 120, new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc), Em(2024, 3, 1)),
                // INCLUSÃO EM 2103 E NENHUMA OUTRA DATA CRÍVEL: pendente.
                new ProcessoNaRegraDoEstagio(121, 41, 121, new DateTime(2103, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(1899, 12, 31, 0, 0, 0, DateTimeKind.Utc))
            ],
            Resultado(120, 250, Em(2024, 3, 2)),
            Resultado(121, 250, new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc)));

        var recuperado = Do(apuracoes, 120);
        recuperado.Should().BeEquivalentTo(new
        {
            AbertoEm = (DateTime?)Em(2024, 3, 1), AberturaDeduzida = true, AberturaRecusada = true, Situacao = SituacaoNaRegraDoEstagio.NoFunil
        }, o => o.ExcludingMissingMembers());

        var semData = Do(apuracoes, 121);
        semData.Situacao.Should().Be(SituacaoNaRegraDoEstagio.AberturaInvalida);
        semData.AbertoEm.Should().BeNull();
        semData.Estagios.Should().BeEmpty();
    }

    [Fact]
    public void O_DNA_em_ciclo_e_tratado_como_processo_sem_pai_e_ninguem_some_do_funil()
    {
        // A→B E B→A: sem a proteção, cada um seria o "pai com filho" do outro, e os dois sairiam do funil.
        var apuracoes = Apurar(
            [Processo(130, Em(2024, 1, 5), dna: 131), Processo(131, Em(2024, 1, 6), dna: 130), Processo(132, Em(2024, 2, 1), dna: 130)],
            Resultado(130, 250, Em(2024, 1, 10)),
            Resultado(131, 3239, Em(2024, 1, 20)),
            Resultado(132, 2563, Em(2024, 2, 5)));

        var a = Do(apuracoes, 130);
        var b = Do(apuracoes, 131);
        a.DnaEmCiclo.Should().BeTrue();
        b.DnaEmCiclo.Should().BeTrue();

        // B NÃO TEM FILHO (o A, em ciclo, vale como sem pai): fica com os próprios estágios, nada herdado.
        b.Situacao.Should().Be(SituacaoNaRegraDoEstagio.NoFunil);
        b.Estagios.Should().OnlyContain(e => !e.Herdado).And.Subject.Max(e => e.Estagio).Should().Be(EstagioDoFunil.Pedido);

        // A TEM O FILHO 132 (fora do ciclo): sai, e o 132 herda dele — e só dele, sem atravessar o ciclo até B.
        a.Situacao.Should().Be(SituacaoNaRegraDoEstagio.PaiSubstituidoPeloFilhoDna);
        var filho = Do(apuracoes, 132);
        filho.DnaEmCiclo.Should().BeFalse();
        filho.Estagios.Single(e => e.Estagio == EstagioDoFunil.Cobertura).NumeroDoProcessoDna.Should().Be(130);
        filho.Estagios.Max(e => e.Estagio).Should().Be(EstagioDoFunil.Negociacao, "a venda aprovada é de B, que não é pai de ninguém");
    }

    [Fact]
    public void As_acoes_da_etapa_sao_as_do_extrator()
    {
        RegraDoEstagio.TodasAsAcoesDaEtapa.Should().Equal(50, 54, 597, 608, 609, 614, 767, 768, 769, 808, 823, 841);
    }
}
