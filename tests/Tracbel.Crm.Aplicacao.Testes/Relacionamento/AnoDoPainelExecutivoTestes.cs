using FluentAssertions;
using Tracbel.Crm.Aplicacao.Relacionamento;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Portas;
using Xunit;

namespace Tracbel.Crm.Aplicacao.Testes.Relacionamento;

/// <summary>
/// O ANO DA VISÃO 360 — até o ÚLTIMO MÊS FECHADO, com o mês de São Paulo (decisão do Ricardo de 27/09/2026).
///
/// <para><b>Por que um dublê do repositório aqui</b>, contra a regra deste projeto de testar com o repositório
/// de verdade: o que está sob prova é a CONTA DO CALENDÁRIO do caso de uso — que meses ele pede ao repositório,
/// e com que ano —, e não uma consulta. O dublê só anota o pedido; a soma dos meses tem teste de ponta a ponta
/// em <c>IndicadoresExecutivosTestes</c>.</para>
/// </summary>
public sealed class AnoDoPainelExecutivoTestes
{
    private sealed class RelogioFixo(DateTime agora) : IRelogio
    {
        public DateTime Agora { get; } = agora;
    }

    /// <summary>Anota o pedido e devolve um painel vazio.</summary>
    private sealed class RepositorioQueAnota : IRepositorioIndicadoresExecutivos
    {
        public int Ano { get; private set; }
        public CalendarioDoAno Calendario { get; private set; }
        public JanelaDeCompetencia? Meses { get; private set; }

        public Task<IndicadoresExecutivosDaFilial> ApurarAsync(
            int ano, CalendarioDoAno calendario, JanelaDeCompetencia meses, DateTime agoraUtc, CancellationToken ct)
        {
            (Ano, Calendario, Meses) = (ano, calendario, meses);
            return Task.FromResult(new IndicadoresExecutivosDaFilial(
                agoraUtc,
                null,
                new FaturamentoDoAno(ano, null, null, 0, 0m, 0m, calendario.ToString(), meses.Inicial, meses.Final),
                new CarteiraDaFilial(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                new CoberturaDaFilial(0, 0, 0, 0, 0, 0, null, 0, 0),
                new MercadoDaFilial(0, 0, 0, 0, 0, null, null)));
        }
    }

    private static (RepositorioQueAnota Repositorio, ObterIndicadoresExecutivos Caso) Montar(DateTime agoraUtc)
    {
        var repositorio = new RepositorioQueAnota();
        return (repositorio, new ObterIndicadoresExecutivos(repositorio, new RelogioFixo(agoraUtc)));
    }

    private static DateOnly Mes(int ano, int mes) => new(ano, mes, 1);

    [Fact]
    public async Task No_fim_de_setembro_o_padrao_e_o_FY2026_ate_agosto()
    {
        var (repositorio, caso) = Montar(new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc));

        var resultado = await caso.ExecutarAsync(null, null, CancellationToken.None);

        resultado.EhSucesso.Should().BeTrue(resultado.Erro);
        repositorio.Ano.Should().Be(2026);
        repositorio.Calendario.Should().Be(CalendarioDoAno.Fiscal);
        repositorio.Meses.Should().Be(new JanelaDeCompetencia(Mes(2025, 11), Mes(2026, 8)),
            "setembro está em curso e fica à parte, no cartão do mês — como nos Indicadores Geográficos");
        resultado.Valor.Dados.MetricasSemDado.Select(m => m.Metrica).Should().Contain("anoAteOUltimoMesFechado");
    }

    [Fact]
    public async Task As_22h_de_31_de_outubro_em_Sao_Paulo_outubro_ainda_esta_em_curso()
    {
        // 01/11/2026 01:00 UTC = 31/10/2026 22:00 em São Paulo. Pelo UTC o ano fiscal já teria virado.
        var (repositorio, caso) = Montar(new DateTime(2026, 11, 1, 1, 0, 0, DateTimeKind.Utc));

        (await caso.ExecutarAsync(null, null, CancellationToken.None)).EhSucesso.Should().BeTrue();

        repositorio.Ano.Should().Be(2026);
        repositorio.Meses.Should().Be(new JanelaDeCompetencia(Mes(2025, 11), Mes(2026, 9)));
    }

    [Fact]
    public async Task Em_novembro_o_padrao_e_o_ano_fiscal_que_fechou_inteiro_e_o_novo_ainda_nao_existe()
    {
        // 01/11/2026 00:00 em São Paulo: o último mês fechado é outubro — o FY2026 inteiro, até novembro fechar.
        var agora = new DateTime(2026, 11, 1, 3, 0, 0, DateTimeKind.Utc);
        var (repositorio, caso) = Montar(agora);

        var padrao = await caso.ExecutarAsync(null, null, CancellationToken.None);

        padrao.EhSucesso.Should().BeTrue();
        repositorio.Ano.Should().Be(2026);
        repositorio.Meses.Should().Be(AnoFiscal.Inteiro(2026));
        padrao.Valor.Dados.MetricasSemDado.Select(m => m.Metrica).Should().NotContain("anoAteOUltimoMesFechado",
            "o ano que acabou de fechar está inteiro");

        var doNovo = await Montar(agora).Caso.ExecutarAsync(null, 2027, CancellationToken.None);
        doNovo.EhSucesso.Should().BeFalse("o FY2027 ainda não tem mês fechado para somar");
        doNovo.Erros.Select(e => e.Campo).Should().Contain("anoFiscal");
    }

    [Fact]
    public async Task Um_ano_fiscal_que_ja_fechou_vem_inteiro()
    {
        var (repositorio, caso) = Montar(new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc));

        (await caso.ExecutarAsync(null, 2025, CancellationToken.None)).EhSucesso.Should().BeTrue();

        repositorio.Meses.Should().Be(AnoFiscal.Inteiro(2025));
    }

    [Fact]
    public async Task O_ano_civil_pedido_tambem_para_no_ultimo_mes_fechado()
    {
        var (repositorio, caso) = Montar(new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc));

        (await caso.ExecutarAsync(2026, null, CancellationToken.None)).EhSucesso.Should().BeTrue();

        repositorio.Calendario.Should().Be(CalendarioDoAno.Civil);
        repositorio.Meses.Should().Be(new JanelaDeCompetencia(Mes(2026, 1), Mes(2026, 8)));
    }

    [Fact]
    public async Task O_cartao_do_ano_nao_tem_mais_a_lacuna_da_meta_de_faturamento()
    {
        // A META DE FATURAMENTO SAIU DESTE CARTÃO (#138, 27/09/2026): a decidida é a de VENDA, em unidades, da API Gestão
        // de Negócios, com rota própria (/relatorios/metas) e as lacunas dela lá.
        var (_, caso) = Montar(new DateTime(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc));

        var resultado = await caso.ExecutarAsync(null, null, CancellationToken.None);

        resultado.Valor.Dados.MetricasSemDado.Select(m => m.Metrica).Should().NotContain("metaDeFaturamento");
    }
}
