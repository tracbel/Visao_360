using FluentAssertions;
using Tracbel.Crm.Dominio.Comum;
using Tracbel.Crm.Dominio.Organizacao;
using Tracbel.Crm.Dominio.Seguranca;
using Xunit;

namespace Tracbel.Crm.Dominio.Testes.Organizacao;

/// <summary>
/// Os parâmetros do potencial com vigência (issue 71): a regra por cultura, os parâmetros gerais e a
/// percepção do gestor. O aceite da issue — "o cálculo de uma data passada usa o parâmetro vigente naquela
/// data" — é a regra de <see cref="ParametroComVigencia.VigenteEm{T}"/>, e começa aqui.
/// </summary>
public sealed class ParametrosDoPotencialTestes
{
    /// <summary>21/09/2026, 15h em São Paulo.</summary>
    private static readonly DateTime Agora = new(2026, 9, 21, 18, 0, 0, DateTimeKind.Utc);

    private static readonly DateOnly Hoje = new(2026, 9, 21);

    private static RegraDePotencial Cafe(DateOnly vigenteDesde, decimal hectares = 10m, DateTime? agora = null) =>
        RegraDePotencial.Informar(
            40139, "Café (em grão) Total", hectares, anosDeRenovacao: null, "3036N",
            SituacaoDaRegraDePotencial.AConfirmar, vigenteDesde, "exemplo do comercial", 100, agora ?? Agora);

    private static ParametroDoPotencial.Valores ValoresDoTexto(decimal limiteDaPercepcao = 5m) =>
        new(12, 0.70m, 1.00m, 1.20m, 1.40m, null, limiteDaPercepcao, null, null, null, null, null);

    [Fact]
    public void A_regra_do_cafe_da_uma_maquina_a_cada_dez_hectares()
    {
        var regra = Cafe(Hoje);

        regra.Situacao.Should().Be(SituacaoDaRegraDePotencial.AConfirmar);
        regra.MaquinasTeoricas(10000m).Should().Be(1000m);
        regra.MaquinasTeoricas(null).Should().BeNull("área não disponível não é área zero");
        regra.AnosDeRenovacao.Should().BeNull("o exemplo do gerente não disse o ciclo, e ninguém o inventa");
        regra.InformadoPorId.Should().Be(100);
        regra.InformadoEm.Should().Be(Agora);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(100_001)]
    public void Regra_com_hectares_fora_da_faixa_e_recusada(decimal hectares)
    {
        var informar = () => Cafe(Hoje, hectares);
        informar.Should().Throw<RegraDeNegocioViolada>().WithMessage("*Hectares por máquina*");
    }

    [Fact]
    public void A_vigencia_comeca_hoje_ou_depois_e_o_passado_nao_se_reescreve()
    {
        var ontem = () => Cafe(Hoje.AddDays(-1));

        ontem.Should().Throw<RegraDeNegocioViolada>().WithMessage("*já passou*");
        Cafe(Hoje).VigenteDesde.Should().Be(Hoje);
        Cafe(Hoje.AddMonths(1)).VigenteDesde.Should().Be(Hoje.AddMonths(1));
    }

    [Fact]
    public void Hoje_e_o_dia_de_Sao_Paulo_e_nao_o_do_relogio_UTC()
    {
        // 30/09 às 22h em São Paulo já é 01/10 em UTC. Quem registra às 22h de 30/09 está no dia 30.
        var noiteDe30EmSaoPaulo = new DateTime(2026, 10, 1, 1, 0, 0, DateTimeKind.Utc);

        ParametroComVigencia.HojeNoBrasil(noiteDe30EmSaoPaulo).Should().Be(new DateOnly(2026, 9, 30));
        Cafe(new DateOnly(2026, 9, 30), agora: noiteDe30EmSaoPaulo).VigenteDesde.Should().Be(new DateOnly(2026, 9, 30));
    }

    [Fact]
    public void Parametro_sem_autor_ou_sem_justificativa_nao_e_gravado()
    {
        var semAutor = () => RegraDePotencial.Informar(
            40139, "Café", 10m, null, "3036N", SituacaoDaRegraDePotencial.AConfirmar, Hoje, "motivo", 0, Agora);
        var semMotivo = () => RegraDePotencial.Informar(
            40139, "Café", 10m, null, "3036N", SituacaoDaRegraDePotencial.AConfirmar, Hoje, "  ", 100, Agora);

        semAutor.Should().Throw<RegraDeNegocioViolada>().WithMessage("*sem autor*");
        semMotivo.Should().Throw<RegraDeNegocioViolada>().WithMessage("*justificativa*");
    }

    [Fact]
    public void O_calculo_de_uma_data_usa_a_vigencia_daquela_data()
    {
        // O ACEITE DA ISSUE 71. Três vigências do café: 10 ha desde 21/09, 20 ha desde 01/11, e uma de 15 ha
        // para 01/12 que foi revogada antes de valer.
        var dez = Cafe(Hoje, 10m);
        var vinte = Cafe(new DateOnly(2026, 11, 1), 20m);
        var revogada = Cafe(new DateOnly(2026, 12, 1), 15m);
        revogada.Revogar("digitado errado", 100, Agora);
        var vigencias = new[] { vinte, revogada, dez };

        ParametroComVigencia.VigenteEm(vigencias, new DateOnly(2026, 9, 20)).Should().BeNull("antes da primeira vigência nada valia");
        ParametroComVigencia.VigenteEm(vigencias, Hoje)!.HectaresPorMaquina.Should().Be(10m);
        ParametroComVigencia.VigenteEm(vigencias, new DateOnly(2026, 10, 31))!.HectaresPorMaquina.Should().Be(10m);
        ParametroComVigencia.VigenteEm(vigencias, new DateOnly(2026, 11, 1))!.HectaresPorMaquina.Should().Be(20m);
        ParametroComVigencia.VigenteEm(vigencias, new DateOnly(2027, 1, 1))!.HectaresPorMaquina.Should().Be(20m,
            "a de 15 ha foi revogada e não vale em data nenhuma");
    }

    [Fact]
    public void So_se_revoga_o_que_ainda_nao_passou_de_hoje()
    {
        var deHoje = Cafe(Hoje);
        var futura = Cafe(Hoje.AddDays(10));

        futura.Revogar("mudou a decisão", 200, Agora);
        futura.RevogadoEm.Should().Be(Agora);
        futura.RevogadoPorId.Should().Be(200);
        futura.MotivoDaRevogacao.Should().Be("mudou a decisão");

        deHoje.Revogar("digitado errado hoje", 200, Agora);
        deHoje.RevogadoEm.Should().NotBeNull("a de hoje ainda não valeu num dia que acabou");

        var vigenteHaUmMes = Cafe(Hoje);
        var umMesDepois = Agora.AddMonths(1);
        var revogarPassada = () => vigenteHaUmMes.Revogar("tarde demais", 200, umMesDepois);
        revogarPassada.Should().Throw<RegraDeNegocioViolada>().WithMessage("*vigência nova a partir de hoje*");

        var deNovo = () => futura.Revogar("de novo", 200, Agora);
        deNovo.Should().Throw<RegraDeNegocioViolada>().WithMessage("*já foi revogada*");

        var semMotivo = () => Cafe(Hoje.AddDays(3)).Revogar(" ", 200, Agora);
        semMotivo.Should().Throw<RegraDeNegocioViolada>().WithMessage("*motivo*");
    }

    [Fact]
    public void Os_parametros_gerais_do_texto_de_21_09_sao_aceitos_e_o_que_esta_em_aberto_fica_vazio()
    {
        var geral = ParametroDoPotencial.Informar(ValoresDoTexto(), Hoje, "texto-base de 21/09", 100, Agora);

        geral.MesesDaJanela.Should().Be(12);
        geral.PesoDosContratosNoCredito.Should().Be(0.70m);
        geral.PesoDoIndicadorDePreco.Should().BeNull();
        geral.FatorMinimo.Should().BeNull();
        geral.NomeDaFaixaIntermediaria.Should().BeNull();
    }

    [Theory]
    [InlineData(0.99, FaixaDeMercado.Retraido)]
    [InlineData(1.00, FaixaDeMercado.Intermediaria)]
    [InlineData(1.20, FaixaDeMercado.Intermediaria)]
    [InlineData(1.21, FaixaDeMercado.Aquecido)]
    [InlineData(1.40, FaixaDeMercado.Aquecido)]
    [InlineData(1.41, FaixaDeMercado.Superaquecido)]
    public void As_faixas_seguem_o_texto_menor_que_1_retraido_maior_que_1_2_aquecido_maior_que_1_4_super(double indice, FaixaDeMercado esperada)
    {
        var geral = ParametroDoPotencial.Informar(ValoresDoTexto(), Hoje, "texto-base de 21/09", 100, Agora);

        geral.FaixaDe((decimal)indice).Should().Be(esperada);
    }

    [Fact]
    public void Faixas_fora_de_ordem_e_fator_pela_metade_sao_recusados()
    {
        var foraDeOrdem = () => ParametroDoPotencial.Informar(
            ValoresDoTexto() with { LimiteDeAquecimento = 1.50m }, Hoje, "x", 100, Agora);
        var fatorPelaMetade = () => ParametroDoPotencial.Informar(
            ValoresDoTexto() with { FatorMinimo = 0.4m }, Hoje, "x", 100, Agora);
        var fatorQueExcluiONeutro = () => ParametroDoPotencial.Informar(
            ValoresDoTexto() with { FatorMinimo = 1.1m, FatorMaximo = 1.5m }, Hoje, "x", 100, Agora);
        var contratosAcimaDeUm = () => ParametroDoPotencial.Informar(
            ValoresDoTexto() with { PesoDosContratosNoCredito = 1.3m }, Hoje, "x", 100, Agora);

        foraDeOrdem.Should().Throw<RegraDeNegocioViolada>().WithMessage("*faixas precisam subir*");
        fatorPelaMetade.Should().Throw<RegraDeNegocioViolada>().WithMessage("*andam juntos*");
        fatorQueExcluiONeutro.Should().Throw<RegraDeNegocioViolada>().WithMessage("*fator mínimo*");
        contratosAcimaDeUm.Should().Throw<RegraDeNegocioViolada>().WithMessage("*de 0 a 1*");

        ParametroDoPotencial.Informar(ValoresDoTexto() with { FatorMinimo = 0.4m, FatorMaximo = 1.5m }, Hoje, "planilha", 100, Agora)
            .FatorMaximo.Should().Be(1.5m);
    }

    [Fact]
    public void A_percepcao_do_gestor_respeita_o_limite_da_vigencia()
    {
        var dentro = PercepcaoDoGestor.Informar(3543402, -5m, 5m, Hoje, "safra ruim", 100, Agora);
        var fora = () => PercepcaoDoGestor.Informar(3543402, 5.5m, 5m, Hoje, "otimista demais", 100, Agora);

        dentro.Percentual.Should().Be(-5m);
        fora.Should().Throw<RegraDeNegocioViolada>().WithMessage("*passa disso*");
    }

    [Fact]
    public void O_padrao_le_os_parametros_e_so_o_administrador_os_altera()
    {
        var padrao = PerfisDeSistema.Todos.Single(p => p.EhPadrao).Permissoes.Select(p => p.Codigo).ToList();
        var administrador = PerfisDeSistema.Todos.Single(p => p.Codigo == PerfisDeSistema.Administrador).Permissoes.Select(p => p.Codigo).ToList();
        var gestor = PerfisDeSistema.Todos.Single(p => p.Codigo == PerfisDeSistema.GestorComercial).Permissoes.Select(p => p.Codigo).ToList();

        padrao.Should().Contain(Permissoes.ParametroDoPotencialLer);
        padrao.Should().NotContain([Permissoes.ParametroDoPotencialAdministrar, Permissoes.PercepcaoDoGestorInformar]);
        administrador.Should().BeEquivalentTo(Permissoes.Catalogo.Keys, "o administrador tem todo o catálogo");
        gestor.Should().Equal([Permissoes.PercepcaoDoGestorInformar]);
    }

    [Fact]
    public void Permissao_nova_entra_no_fim_do_perfil_para_nao_renumerar_a_semente()
    {
        // O Id semeado de PerfilPermissao é 100 × perfil + ordem. Permissão inserida no meio renumeraria as
        // seguintes, e a migração REESCREVERIA linhas que já existem no banco em vez de só acrescentar.
        var padrao = PerfisDeSistema.Todos.Single(p => p.EhPadrao).Permissoes.Select(p => p.Codigo).ToList();
        var administrador = PerfisDeSistema.Todos.Single(p => p.Codigo == PerfisDeSistema.Administrador).Permissoes.Select(p => p.Codigo).ToList();

        padrao[^1].Should().Be(Permissoes.ParametroDoPotencialLer);
        padrao.IndexOf(Permissoes.EquipamentoEditar).Should().Be(15, "a 16ª permissão do padrão desde a fase 3");
        administrador.Skip(21).Take(3).Should().Equal(
            [Permissoes.ParametroDoPotencialLer, Permissoes.ParametroDoPotencialAdministrar, Permissoes.PercepcaoDoGestorInformar],
            "as três da issue 71 continuam nas posições 22 a 24");
        administrador.IndexOf(Permissoes.UsuarioAdministrar).Should().Be(20, "a 21ª permissão do administrador desde a fase 3");
        administrador.IndexOf(Permissoes.UsuarioLer).Should().Be(24, "a da issue 113 entrou depois, no fim");
        administrador.IndexOf(Permissoes.AuditoriaLer).Should().Be(25, "a da issue 135 entrou depois dela, no fim");
        administrador[^1].Should().Be(Permissoes.IntegracaoAdministrar, "a da issue 136 entrou depois, no fim");
    }
}
