using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Integracao;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Integracao;

/// <summary>
/// AS REGRAS DAS INTEGRAÇÕES CONFIGURÁVEIS (issue 136): quando uma rotina está vencida, o que a agenda aceita, o que
/// a conexão aceita e a credencial que não sai aberta.
/// </summary>
public sealed class ConexoesERotinasTestes
{
    private static DateTime Utc(int ano, int mes, int dia, int hora = 0, int minuto = 0) => new(ano, mes, dia, hora, minuto, 0, DateTimeKind.Utc);

    private static Rotina Precos(DateTime vigenteDesde) => Rotina.DoCatalogo(RotinasDoSistema.Obter(RotinasDoSistema.PrecosMensais)!, vigenteDesde);

    [Fact]
    public void A_agenda_semeada_nao_cobra_o_que_ficou_para_tras_dela()
    {
        // A mensal de 20/09 rodou pelas tarefas do Windows; o orquestrador entra em 22/09 e não a repete.
        var precos = Precos(RotinasDoSistema.AgendaSemeadaDesde);

        precos.EstaVencida(Utc(2026, 9, 23)).Should().BeFalse();
        precos.EstaVencida(Utc(2026, 10, 20, 7, 1)).Should().BeTrue("20/10 às 04:01 de São Paulo a de outubro chegou");
        precos.ProximaExecucao(Utc(2026, 9, 23)).Should().Be(Utc(2026, 10, 20, 7));
    }

    [Fact]
    public void Rodar_marca_o_comeco_e_so_volta_a_vencer_na_proxima_agenda()
    {
        var precos = Precos(RotinasDoSistema.AgendaSemeadaDesde);

        precos.IniciarExecucao(Utc(2026, 10, 20, 7, 5));
        precos.EstaVencida(Utc(2026, 10, 20, 9)).Should().BeFalse();
        precos.EstaVencida(Utc(2026, 11, 20, 7, 5)).Should().BeTrue();
    }

    [Fact]
    public void Rodar_agora_vence_na_hora_mesmo_desligada_e_o_pedido_sai_da_fila_ao_comecar()
    {
        var precos = Precos(RotinasDoSistema.AgendaSemeadaDesde);
        precos.Desligar();
        precos.EstaVencida(Utc(2026, 12, 25)).Should().BeFalse("desligada, a agenda não roda");

        precos.PedirExecucao(100, Utc(2026, 9, 23, 10));
        precos.PedirExecucao(200, Utc(2026, 9, 23, 10, 1));
        precos.EstaVencida(Utc(2026, 9, 23, 10, 2)).Should().BeTrue();
        precos.MotivoAgora().Should().Be(MotivoDaExecucao.Pedido);

        precos.IniciarExecucao(Utc(2026, 9, 23, 10, 5)).Should().Be(100, "o primeiro pedido vale; repetir não duplica");
        precos.ExecucaoPedidaEm.Should().BeNull();
    }

    [Fact]
    public void Religar_ou_reagendar_vale_daqui_para_a_frente()
    {
        var precos = Precos(RotinasDoSistema.AgendaSemeadaDesde);
        precos.Desligar();
        precos.Ligar(Utc(2026, 11, 25));
        precos.EstaVencida(Utc(2026, 11, 26)).Should().BeFalse("a de 20/11 ficou antes de religar");

        precos.Reagendar(AgendaDaRotina.DiariaAs(new TimeOnly(5, 0)), Utc(2026, 11, 26, 12));
        precos.EstaVencida(Utc(2026, 11, 26, 13)).Should().BeFalse("a das 5h de 26/11 ficou antes de reagendar");
        precos.EstaVencida(Utc(2026, 11, 27, 7, 30)).Should().BeFalse("27/11 às 04:30 de São Paulo a das 5h ainda não chegou");
        precos.EstaVencida(Utc(2026, 11, 27, 8)).Should().BeTrue("05:00 de São Paulo é 08:00 em UTC");
    }

    [Fact]
    public void Intervalo_conta_do_comeco_da_ultima()
    {
        var art = Rotina.DoCatalogo(RotinasDoSistema.Obter(RotinasDoSistema.ArtVendas)!, Utc(2026, 9, 22, 12));
        art.Ligar(Utc(2026, 9, 22, 12));

        art.EstaVencida(Utc(2026, 9, 22, 12, 59)).Should().BeFalse();
        art.EstaVencida(Utc(2026, 9, 22, 13)).Should().BeTrue();
        art.IniciarExecucao(Utc(2026, 9, 22, 13, 2));
        art.ProximaExecucao(Utc(2026, 9, 22, 13, 30)).Should().Be(Utc(2026, 9, 22, 14, 2));
    }

    [Theory]
    [InlineData(CadenciaDaRotina.Mensal, null, 31, "04:00", null, "dia")]
    [InlineData(CadenciaDaRotina.Anual, 13, 1, "03:00", null, "mes")]
    [InlineData(CadenciaDaRotina.Diaria, null, null, null, null, "hora")]
    [InlineData(CadenciaDaRotina.Intervalo, null, null, null, 5, "intervaloMinutos")]
    public void A_agenda_recusa_o_que_nao_existe_em_todo_mes_e_o_que_o_orquestrador_nao_cumpre(
        CadenciaDaRotina cadencia, int? mes, int? dia, string? hora, int? minutos, string campo)
    {
        var agenda = new AgendaDaRotina(cadencia, mes, dia, hora is null ? null : TimeOnly.Parse(hora), minutos);

        agenda.Problemas().Select(p => p.Campo).Should().Contain(campo);
        FluentActions.Invoking(() => Precos(DateTime.UtcNow).Reagendar(agenda, DateTime.UtcNow)).Should().Throw<RegraDeNegocioViolada>();
    }

    [Fact]
    public void A_agenda_se_descreve_em_portugues()
    {
        AgendaDaRotina.AnualEm(10, 1, new TimeOnly(3, 0)).Descrever().Should().Be("todo ano em 1º de outubro, às 03:00");
        AgendaDaRotina.MensalEm(20, new TimeOnly(4, 0)).Descrever().Should().Be("todo mês no dia 20, às 04:00");
        AgendaDaRotina.DiariaAs(new TimeOnly(5, 0)).Descrever().Should().Be("todo dia às 05:00");
        AgendaDaRotina.ACada(60).Descrever().Should().Be("a cada hora");
        AgendaDaRotina.ACada(45).Descrever().Should().Be("a cada 45 minutos");
    }

    [Fact]
    public void O_endereco_da_fonte_publica_nao_se_edita_e_o_servidor_nao_aceita_ponto_e_virgula()
    {
        var ibge = Conexao.DoCatalogo(ConexoesDoSistema.Todas.First(c => c.Tipo == TipoDeConexao.FontePublica));
        FluentActions.Invoking(() => ibge.Configurar("https://outro.exemplo.invalid", null, null, null, null, null))
            .Should().Throw<RegraDeNegocioViolada>().WithMessage("*muda com código*");

        Conexao.ProblemasDoEndereco(TipoDeConexao.SqlServer, "servidor;Password=x", null, "Banco", null, null)
            .Select(p => p.Campo).Should().Contain("endereco", "o servidor entra numa cadeia de conexão");
        Conexao.ProblemasDoEndereco(TipoDeConexao.MySql, "servidor", 3306, "banco", "visao; DROP", null)
            .Select(p => p.Campo).Should().Contain("objeto", "a visão entra numa consulta");
        Conexao.ProblemasDoEndereco(TipoDeConexao.SqlServer, @"10.0.0.5\INSTANCIA,1433", null, "Banco_1", null, null).Should().BeEmpty();
    }

    [Fact]
    public void A_credencial_so_conta_completa_e_retirar_volta_ao_ambiente()
    {
        var protheus = Conexao.DoCatalogo(ConexoesDoSistema.Todas.Single(c => c.Codigo == ConexoesDoSistema.Protheus));
        protheus.Configurar("http://erp.exemplo.invalid:5891/rest", null, null, null, "leitura", null);
        protheus.EstaConfiguradaPelaTela.Should().BeFalse("falta a senha");

        protheus.DefinirSegredo([1, 2, 3], 100, Utc(2026, 9, 22, 13));
        protheus.EstaConfiguradaPelaTela.Should().BeTrue();
        protheus.SegredoAlteradoPorId.Should().Be(100);

        protheus.RemoverSegredo(100, Utc(2026, 9, 22, 14));
        protheus.TemSegredo.Should().BeFalse();
        protheus.SegredoAlteradoEm.Should().Be(Utc(2026, 9, 22, 14));
    }

    [Fact]
    public void So_a_api_monitorada_sai_do_monitoramento_e_o_teste_vence_pelo_intervalo()
    {
        var clima = Conexao.CriarMonitorada("CLIMA", "Clima", null, "https://api.exemplo.invalid/saude", 200, 30);
        clima.VerificacaoVencida(Utc(2026, 9, 22, 12)).Should().BeTrue("nunca foi testada");
        clima.RegistrarVerificacao(true, "HTTP 200", Utc(2026, 9, 22, 12));
        clima.VerificacaoVencida(Utc(2026, 9, 22, 12, 29)).Should().BeFalse();
        clima.VerificacaoVencida(Utc(2026, 9, 22, 12, 30)).Should().BeTrue();

        clima.Desativar();
        clima.VerificacaoVencida(Utc(2026, 9, 23)).Should().BeFalse();

        var protheus = Conexao.DoCatalogo(ConexoesDoSistema.Todas[0]);
        FluentActions.Invoking(protheus.Desativar).Should().Throw<RegraDeNegocioViolada>();
    }
    // =============================================================================================
    // O primeiro ano da série histórica (issue 156)
    // =============================================================================================

    private static Rotina FontesAnuais() =>
        Rotina.DoCatalogo(RotinasDoSistema.Obter(RotinasDoSistema.FontesAnuais)!, RotinasDoSistema.AgendaSemeadaDesde);

    [Fact]
    public void A_rotina_das_fontes_anuais_nasce_com_o_primeiro_ano_da_serie()
    {
        // A PAM tem série desde 1974; 2010 é a escolha da issue 156, semeada pela migração. As outras
        // rotinas nascem sem parâmetro nenhum, porque as fontes delas não têm série para buscar.
        FontesAnuais().AnoInicialDoHistorico.Should().Be(RotinasDoSistema.AnoInicialPadraoDaPam);
        Precos(RotinasDoSistema.AgendaSemeadaDesde).AnoInicialDoHistorico.Should().BeNull();
    }

    [Fact]
    public void Trocar_o_primeiro_ano_diz_se_mudou_e_o_vazio_volta_a_janela_curta()
    {
        var rotina = FontesAnuais();

        rotina.DefinirAnoInicialDoHistorico(2010, Utc(2026, 9, 22)).Should().BeFalse("já era 2010");
        rotina.DefinirAnoInicialDoHistorico(2005, Utc(2026, 9, 22)).Should().BeTrue();
        rotina.AnoInicialDoHistorico.Should().Be(2005);

        rotina.DefinirAnoInicialDoHistorico(null, Utc(2026, 9, 22)).Should().BeTrue();
        rotina.AnoInicialDoHistorico.Should().BeNull("sem parâmetro, a carga fica na janela curta dos anos recentes");
    }

    [Theory]
    [InlineData((short)1973)]
    [InlineData((short)2027)]
    public void O_primeiro_ano_fora_do_que_a_pam_publica_e_recusado(short ano)
    {
        // 1974 é o início da PAM: pedir antes disso faria a carga percorrer décadas vazias a cada
        // rodada. Ano futuro não traz nada nunca.
        var rotina = FontesAnuais();

        FluentActions.Invoking(() => rotina.DefinirAnoInicialDoHistorico(ano, Utc(2026, 9, 22)))
            .Should().Throw<RegraDeNegocioViolada>();
    }
}