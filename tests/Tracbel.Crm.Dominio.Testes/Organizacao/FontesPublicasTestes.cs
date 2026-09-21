using FluentAssertions;
using Tracbel.Crm.Dominio.Organizacao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// O calendário das rotinas e a situação de cada fonte no painel do administrador (issue 77). As horas são de
/// São Paulo, UTC−3: 04:00 lá é 07:00 em UTC.
/// </summary>
public sealed class FontesPublicasTestes
{
    private static DateTime Utc(int ano, int mes, int dia, int hora = 0, int minuto = 0) =>
        new(ano, mes, dia, hora, minuto, 0, DateTimeKind.Utc);

    [Fact]
    public void A_mensal_roda_no_dia_20_as_4h_de_Sao_Paulo()
    {
        var mensal = FontesPublicas.Mensal;

        // 21/09 ao meio-dia: a última foi 20/09 04:00 (07:00 UTC) e a próxima é 20/10.
        mensal.UltimaPrevistaAte(Utc(2026, 9, 21, 15)).Should().Be(Utc(2026, 9, 20, 7));
        mensal.ProximaDepoisDe(Utc(2026, 9, 21, 15)).Should().Be(Utc(2026, 10, 20, 7));

        // 20/09 às 03:59 de São Paulo a de setembro ainda não chegou.
        mensal.UltimaPrevistaAte(Utc(2026, 9, 20, 6, 59)).Should().Be(Utc(2026, 8, 20, 7));
        mensal.ProximaDepoisDe(Utc(2026, 9, 20, 6, 59)).Should().Be(Utc(2026, 9, 20, 7));

        // A virada do ano.
        mensal.ProximaDepoisDe(Utc(2026, 12, 25)).Should().Be(Utc(2027, 1, 20, 7));
    }

    [Fact]
    public void A_anual_roda_em_1_de_outubro_as_3h_de_Sao_Paulo()
    {
        var anual = FontesPublicas.Anual;

        anual.UltimaPrevistaAte(Utc(2026, 9, 21, 15)).Should().Be(Utc(2025, 10, 1, 6));
        anual.ProximaDepoisDe(Utc(2026, 9, 21, 15)).Should().Be(Utc(2026, 10, 1, 6));
        anual.UltimaPrevistaAte(Utc(2026, 11, 5)).Should().Be(Utc(2026, 10, 1, 6));
        anual.ProximaDepoisDe(Utc(2026, 11, 5)).Should().Be(Utc(2027, 10, 1, 6));
    }

    [Fact]
    public void Tabela_vazia_e_sem_dado_antes_de_tudo()
    {
        var precos = FontesPublicas.Todas.Single(f => f.Fluxo == "CONAB.PRECO_RECEBIDO");

        // O caso do servidor em 21/09/2026: rotina registrada, nunca rodou, tabela vazia.
        var (situacao, motivo) = precos.SituacaoEm(null, 0, Utc(2026, 9, 21, 15));

        situacao.Should().Be(SituacaoDaFonte.SemDado);
        motivo.Should().Contain("nunca rodou").And.Contain("TracbelCrmPrecos");
    }

    [Fact]
    public void Fonte_que_nao_rodou_depois_da_ultima_execucao_agendada_fica_atrasada()
    {
        var precos = FontesPublicas.Todas.Single(f => f.Fluxo == "CONAB.PRECO_RECEBIDO");
        var agora = Utc(2026, 9, 21, 15);

        precos.SituacaoEm(Utc(2026, 8, 25), 500, agora).Situacao.Should().Be(SituacaoDaFonte.Atrasada,
            "a de 20/09 passou e a última rodada foi em agosto");
        precos.SituacaoEm(Utc(2026, 9, 20, 7, 30), 500, agora).Situacao.Should().Be(SituacaoDaFonte.EmDia);
        precos.SituacaoEm(null, 500, agora).Situacao.Should().Be(SituacaoDaFonte.Atrasada,
            "há linha e nunca houve rodada registrada: ninguém sabe de quando é");
    }

    [Fact]
    public void A_execucao_em_andamento_nao_aparece_como_atraso()
    {
        // 20/09 às 5h de São Paulo: a rotina das 4h pode estar rodando. Com a margem de 6 horas, a cobrança
        // ainda é a de agosto.
        var precos = FontesPublicas.Todas.Single(f => f.Fluxo == "CONAB.PRECO_RECEBIDO");

        precos.SituacaoEm(Utc(2026, 8, 20, 7, 30), 500, Utc(2026, 9, 20, 8)).Situacao.Should().Be(SituacaoDaFonte.EmDia);
        precos.SituacaoEm(Utc(2026, 8, 20, 7, 30), 500, Utc(2026, 9, 20, 14)).Situacao.Should().Be(SituacaoDaFonte.Atrasada);
    }
}
